using System.Threading;
using System.Threading.Tasks;
using OpenIPC.Viewer.Core.Recording;

namespace OpenIPC.Viewer.Android.Platform;

// Placeholder until Phase 9c wires the foreground-service + FFmpegKit
// recording path. Throws only if a caller actually tries to start a
// recording — DI resolution and migrations are unaffected.
internal sealed class NotImplementedRecorder : IRecorder
{
    public Task<IRecordingSession> StartAsync(RecordingOptions options, CancellationToken ct)
        => throw new System.NotSupportedException(
            "Recording on Android is wired in Phase 9c (foreground service + FFmpegKit).");
}
