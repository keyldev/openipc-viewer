using Microsoft.Extensions.Logging.Abstractions;
using OpenIPC.Viewer.Core.Video;
using OpenIPC.Viewer.Video;

namespace OpenIPC.Viewer.Video.Tests;

public sealed class VideoSessionIntegrationTests
{
    private static IVideoEngine NewEngine() => new FfmpegVideoEngine(NullLoggerFactory.Instance);

    [SkippableFact]
    public async Task SessionReachesPlaying_AndReceivesFrames()
    {
        Skip.IfNot(MediaMtxFixture.IsReachable(),
            "MediaMTX not running on localhost:8554 — start tools/mediamtx/docker-compose.yml first.");

        await using var session = NewEngine().CreateSession(
            VideoSessionOptions.Default(new Uri(MediaMtxFixture.TestStreamUri)));

        var playing = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var sub = session.StateChanged.Subscribe(s =>
        {
            if (s == SessionState.Playing) playing.TrySetResult(true);
            if (s == SessionState.Failed) playing.TrySetException(new Exception(session.LastError ?? "failed"));
        });

        await session.StartAsync(CancellationToken.None);

        var reachedPlaying = await Task.WhenAny(playing.Task, Task.Delay(TimeSpan.FromSeconds(15)));
        Assert.True(reachedPlaying == playing.Task, "Session never reached Playing within 15s");

        var collected = 0;
        var done = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var frameSub = session.Frames.Subscribe(_ =>
        {
            if (Interlocked.Increment(ref collected) >= 30)
                done.TrySetResult(true);
        });

        var got30 = await Task.WhenAny(done.Task, Task.Delay(TimeSpan.FromSeconds(10)));
        Assert.True(got30 == done.Task, $"Only got {collected} frames in 10s, expected 30");
    }
}
