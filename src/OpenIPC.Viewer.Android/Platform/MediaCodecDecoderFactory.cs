using System.Runtime.Versioning;
using Android.Media;
using OpenIPC.Viewer.Core.Video;

namespace OpenIPC.Viewer.Android.Platform;

// Probe via MediaCodecList — Android 10+ ships H.264 hardware decoders on
// every device; H.265 coverage is broader on >2020 hardware. We just need
// *some* hw decoder for video/avc to call this available; the per-stream
// codec choice happens inside FFmpegKit when Phase 9b wires it up.
[SupportedOSPlatform("android")]
public sealed class MediaCodecDecoderFactory : IHwDecoderFactory
{
    public HwAccelHint Kind => HwAccelHint.MediaCodec;

    public HwProbeResult Probe()
    {
        try
        {
            using var list = new MediaCodecList(MediaCodecListKind.RegularCodecs);
            var codecs = list.GetCodecInfos() ?? System.Array.Empty<MediaCodecInfo>();
            foreach (var info in codecs)
            {
                if (info is null || info.IsEncoder) continue;
                var types = info.GetSupportedTypes() ?? System.Array.Empty<string>();
                foreach (var type in types)
                {
                    if (type.Equals("video/avc", System.StringComparison.OrdinalIgnoreCase) ||
                        type.Equals("video/hevc", System.StringComparison.OrdinalIgnoreCase))
                    {
                        return HwProbeResult.Ok();
                    }
                }
            }
            return HwProbeResult.Unavailable("No video/avc or video/hevc decoder reported by MediaCodecList");
        }
        catch (System.Exception ex)
        {
            return HwProbeResult.Unavailable($"MediaCodecList probe failed: {ex.Message}");
        }
    }
}
