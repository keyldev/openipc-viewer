using System;
using System.Buffers;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using FFmpeg.AutoGen;
using Microsoft.Extensions.Logging;
using OpenIPC.Viewer.Core.Entities;
using OpenIPC.Viewer.Core.Video;
using SkiaSharp;

namespace OpenIPC.Viewer.Video.Pipeline;

internal sealed class FfmpegVideoSession : IVideoSession
{
    private readonly VideoSessionOptions _options;
    private readonly ILogger<FfmpegVideoSession> _logger;

    private readonly Subject<VideoFrame> _frames = new();
    private readonly Subject<SessionState> _stateChanged = new();
    private readonly Subject<SessionTelemetry> _telemetry = new();

    private readonly object _stateLock = new();
    private readonly object _snapshotLock = new();

    private Thread? _thread;
    private CancellationTokenSource? _cts;
    private SessionState _state = SessionState.Idle;
    private string? _lastError;

    private int _framesDecoded;
    private DateTime _lastFpsTick;
    private int _framesSinceFpsTick;
    private string? _codecName;
    private int _width;
    private int _height;

    // Snapshot of the most recently decoded frame, kept around for SnapshotAsync.
    private byte[]? _snapshotBgra;
    private int _snapshotWidth;
    private int _snapshotHeight;
    private int _snapshotStride;

    public FfmpegVideoSession(VideoSessionOptions options, ILogger<FfmpegVideoSession> logger)
    {
        _options = options;
        _logger = logger;
    }

    public SessionState State
    {
        get { lock (_stateLock) return _state; }
    }

    public string? LastError
    {
        get { lock (_stateLock) return _lastError; }
    }

    public IObservable<VideoFrame> Frames => _frames;
    public IObservable<SessionState> StateChanged => _stateChanged;
    public IObservable<SessionTelemetry> Telemetry => _telemetry;

    public Task StartAsync(CancellationToken ct)
    {
        if (_thread is not null)
            throw new InvalidOperationException("Session already started");

        FfmpegRuntime.EnsureInitialized();
        SetState(SessionState.Connecting);

        _cts = new CancellationTokenSource();
        _thread = new Thread(Run)
        {
            IsBackground = true,
            Priority = ThreadPriority.AboveNormal,
            Name = $"rtsp-{_options.RtspUri.Host}",
        };
        _thread.Start();
        return Task.CompletedTask;
    }

    public Task<byte[]> SnapshotAsync(SnapshotFormat format, CancellationToken ct)
    {
        byte[]? bgra;
        int w, h, stride;
        lock (_snapshotLock)
        {
            if (_snapshotBgra is null)
                throw new InvalidOperationException("No frame available yet");
            bgra = (byte[])_snapshotBgra.Clone();
            w = _snapshotWidth;
            h = _snapshotHeight;
            stride = _snapshotStride;
        }

        using var bitmap = new SKBitmap(new SKImageInfo(w, h, SKColorType.Bgra8888, SKAlphaType.Premul));
        Marshal.Copy(bgra, 0, bitmap.GetPixels(), stride * h);

        using var image = SKImage.FromBitmap(bitmap);
        var skFormat = format == SnapshotFormat.Png ? SKEncodedImageFormat.Png : SKEncodedImageFormat.Jpeg;
        using var data = image.Encode(skFormat, quality: 92);
        return Task.FromResult(data.ToArray());
    }

    public async ValueTask DisposeAsync()
    {
        _cts?.Cancel();
        if (_thread is { IsAlive: true })
        {
            await Task.Run(() => _thread.Join(TimeSpan.FromSeconds(2))).ConfigureAwait(false);
        }
        _frames.OnCompleted();
        _stateChanged.OnCompleted();
        _telemetry.OnCompleted();
        _cts?.Dispose();
    }

    private unsafe void Run()
    {
        AVFormatContext* fmtCtx = null;
        AVCodecContext* codecCtx = null;
        AVFrame* frame = null;
        AVPacket* packet = null;
        SwsContext* sws = null;
        AVDictionary* opts = null;
        var videoStreamIndex = -1;

        try
        {
            BuildOpts(&opts);
            fmtCtx = ffmpeg.avformat_alloc_context();
            var url = BuildUrlWithCredentials(_options.RtspUri, _options.Credentials);

            var ret = ffmpeg.avformat_open_input(&fmtCtx, url, null, &opts);
            FfmpegError.ThrowIfError(ret, "avformat_open_input");

            ret = ffmpeg.avformat_find_stream_info(fmtCtx, null);
            FfmpegError.ThrowIfError(ret, "avformat_find_stream_info");

            for (var i = 0; i < (int)fmtCtx->nb_streams; i++)
            {
                if (fmtCtx->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
                {
                    videoStreamIndex = i;
                    break;
                }
            }
            if (videoStreamIndex < 0)
                throw new InvalidOperationException("No video stream in input");

            var codecpar = fmtCtx->streams[videoStreamIndex]->codecpar;
            var codec = ffmpeg.avcodec_find_decoder(codecpar->codec_id);
            if (codec == null)
                throw new InvalidOperationException($"No decoder available for codec id {codecpar->codec_id}");

            codecCtx = ffmpeg.avcodec_alloc_context3(codec);
            ret = ffmpeg.avcodec_parameters_to_context(codecCtx, codecpar);
            FfmpegError.ThrowIfError(ret, "avcodec_parameters_to_context");

            ret = ffmpeg.avcodec_open2(codecCtx, codec, null);
            FfmpegError.ThrowIfError(ret, "avcodec_open2");

            _width = codecCtx->width;
            _height = codecCtx->height;
            _codecName = Marshal.PtrToStringAnsi((IntPtr)codec->name);

            sws = ffmpeg.sws_getContext(
                _width, _height, codecCtx->pix_fmt,
                _width, _height, AVPixelFormat.AV_PIX_FMT_BGRA,
                ffmpeg.SWS_BILINEAR, null, null, null);
            if (sws == null)
                throw new InvalidOperationException("sws_getContext returned null");

            packet = ffmpeg.av_packet_alloc();
            frame = ffmpeg.av_frame_alloc();

            SetState(SessionState.Playing);
            _lastFpsTick = DateTime.UtcNow;

            var ct = _cts!.Token;
            while (!ct.IsCancellationRequested)
            {
                ret = ffmpeg.av_read_frame(fmtCtx, packet);
                if (ret < 0)
                {
                    if (ret == ffmpeg.AVERROR_EOF)
                    {
                        _logger.LogInformation("RTSP stream EOF");
                        break;
                    }
                    _logger.LogWarning("av_read_frame failed: {Err}", FfmpegError.Describe(ret));
                    break;
                }

                if (packet->stream_index != videoStreamIndex)
                {
                    ffmpeg.av_packet_unref(packet);
                    continue;
                }

                ret = ffmpeg.avcodec_send_packet(codecCtx, packet);
                ffmpeg.av_packet_unref(packet);
                if (ret < 0 && ret != ffmpeg.AVERROR(ffmpeg.EAGAIN))
                {
                    _logger.LogWarning("avcodec_send_packet failed: {Err}", FfmpegError.Describe(ret));
                    continue;
                }

                while (!ct.IsCancellationRequested)
                {
                    ret = ffmpeg.avcodec_receive_frame(codecCtx, frame);
                    if (ret == ffmpeg.AVERROR(ffmpeg.EAGAIN) || ret == ffmpeg.AVERROR_EOF)
                        break;
                    if (ret < 0)
                    {
                        _logger.LogWarning("avcodec_receive_frame failed: {Err}", FfmpegError.Describe(ret));
                        break;
                    }

                    EmitFrame(sws, frame);
                    ffmpeg.av_frame_unref(frame);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Video session loop failed");
            SetState(SessionState.Failed, ex.Message);
            return;
        }
        finally
        {
            if (sws != null) ffmpeg.sws_freeContext(sws);
            if (frame != null) { var p = frame; ffmpeg.av_frame_free(&p); }
            if (packet != null) { var p = packet; ffmpeg.av_packet_free(&p); }
            if (codecCtx != null) { var p = codecCtx; ffmpeg.avcodec_free_context(&p); }
            if (fmtCtx != null) ffmpeg.avformat_close_input(&fmtCtx);
            if (opts != null) ffmpeg.av_dict_free(&opts);
        }

        SetState(SessionState.Idle);
    }

    private unsafe void EmitFrame(SwsContext* sws, AVFrame* frame)
    {
        var stride = _width * 4;
        var bufSize = stride * _height;
        var bgra = ArrayPool<byte>.Shared.Rent(bufSize);
        try
        {
            fixed (byte* dst = bgra)
            {
                var dstData = new byte_ptrArray4 { [0] = dst };
                var dstLinesize = new int_array4 { [0] = stride };
                ffmpeg.sws_scale(sws, frame->data, frame->linesize, 0, _height, dstData, dstLinesize);
            }

            UpdateSnapshotBuffer(bgra, stride);

            var vf = new VideoFrame(bgra, _width, _height, stride, frame->pts, DateTime.UtcNow);

            // Synchronous delivery: subscribers must finish (incl. UI Marshal.Copy) before
            // OnNext returns. After that we return the buffer to the pool. The natural
            // backpressure is that a slow subscriber blocks the decoder thread —
            // intentional per architecture §6.3 (drop, don't buffer).
            try
            {
                _frames.OnNext(vf);
                Interlocked.Increment(ref _framesDecoded);
                Interlocked.Increment(ref _framesSinceFpsTick);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Subscriber threw in frame OnNext");
            }

            MaybePublishTelemetry();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(bgra);
        }
    }

    private void UpdateSnapshotBuffer(byte[] sourceBgra, int stride)
    {
        var size = stride * _height;
        lock (_snapshotLock)
        {
            if (_snapshotBgra is null || _snapshotBgra.Length < size)
                _snapshotBgra = new byte[size];
            Buffer.BlockCopy(sourceBgra, 0, _snapshotBgra, 0, size);
            _snapshotWidth = _width;
            _snapshotHeight = _height;
            _snapshotStride = stride;
        }
    }

    private void MaybePublishTelemetry()
    {
        var now = DateTime.UtcNow;
        var elapsed = now - _lastFpsTick;
        if (elapsed.TotalSeconds < 1)
            return;

        var sinceLast = Interlocked.Exchange(ref _framesSinceFpsTick, 0);
        var fps = sinceLast / elapsed.TotalSeconds;
        _lastFpsTick = now;

        _telemetry.OnNext(new SessionTelemetry(
            FramesDecoded: _framesDecoded,
            FramesDropped: 0, // synchronous delivery — no internal drops yet; Phase 3 grid adds backpressure counters
            Fps: fps,
            AverageLatency: TimeSpan.Zero,
            Codec: _codecName,
            Width: _width,
            Height: _height,
            CapturedAt: now));
    }

    private void SetState(SessionState newState, string? error = null)
    {
        lock (_stateLock)
        {
            _state = newState;
            _lastError = error;
        }
        _stateChanged.OnNext(newState);
    }

    private unsafe void BuildOpts(AVDictionary** opts)
    {
        var transport = _options.Transport switch
        {
            RtspTransport.Tcp => "tcp",
            RtspTransport.Udp => "udp",
            _ => "tcp",
        };
        ffmpeg.av_dict_set(opts, "rtsp_transport", transport, 0);
        ffmpeg.av_dict_set(opts, "stimeout", "5000000", 0);          // 5s socket timeout (µs)
        ffmpeg.av_dict_set(opts, "max_delay", "200000", 0);          // 200ms reorder window
        ffmpeg.av_dict_set(opts, "buffer_size", "1048576", 0);
        ffmpeg.av_dict_set(opts, "reorder_queue_size", "0", 0);
        ffmpeg.av_dict_set(opts, "fflags", "nobuffer", 0);
    }

    private static string BuildUrlWithCredentials(Uri uri, CameraCredentials? creds)
    {
        if (creds is null || !string.IsNullOrEmpty(uri.UserInfo))
            return uri.ToString();

        var builder = new UriBuilder(uri)
        {
            UserName = Uri.EscapeDataString(creds.Username),
            Password = Uri.EscapeDataString(creds.Password),
        };
        return builder.Uri.ToString();
    }
}
