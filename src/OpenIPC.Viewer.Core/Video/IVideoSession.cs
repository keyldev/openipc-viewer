using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenIPC.Viewer.Core.Video;

public interface IVideoSession : IAsyncDisposable
{
    SessionState State { get; }
    string? LastError { get; }

    IObservable<SessionState> StateChanged { get; }
    IObservable<VideoFrame> Frames { get; }
    IObservable<SessionTelemetry> Telemetry { get; }

    Task StartAsync(CancellationToken ct);
    Task<byte[]> SnapshotAsync(SnapshotFormat format, CancellationToken ct);
}
