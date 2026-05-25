using System;
using System.IO;
using System.Threading;
using FFmpeg.AutoGen;

namespace OpenIPC.Viewer.Video.Pipeline;

internal static class FfmpegRuntime
{
    private static int _initialized;

    public static void EnsureInitialized()
    {
        if (Interlocked.CompareExchange(ref _initialized, 1, 0) != 0)
            return;

        ffmpeg.RootPath = ResolveNativeDir();
        // Touch one function to surface load errors early.
        _ = ffmpeg.av_version_info();
        ffmpeg.avformat_network_init();
    }

    private static string ResolveNativeDir()
    {
        var baseDir = AppContext.BaseDirectory;
        var candidate = Path.Combine(baseDir, "runtimes", "win-x64", "native");
        if (Directory.Exists(candidate))
            return candidate;

        // Fall back to baseDir — FFmpeg.AutoGen tolerates DLLs sitting next to the exe.
        return baseDir;
    }
}
