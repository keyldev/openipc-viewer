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

        if (OperatingSystem.IsAndroid())
        {
            // FFmpeg.AutoGen 7.1.1's FunctionResolverFactory.Create() throws
            // PlatformNotSupportedException on Android (Environment.OSVersion.Platform
            // reports Other). DynamicallyLoadedBindings.Initialize() — called from the
            // ffmpeg static ctor — uses this field if non-null, otherwise invokes the
            // throwing factory. Pre-setting bypasses the broken path entirely.
            DynamicallyLoadedBindings.FunctionResolver = new AndroidFunctionResolver();
        }

        var nativeDir = ResolveNativeDir();

        try
        {
            // RootPath assignment is the FIRST touch of the ffmpeg static class
            // and triggers its type initializer — any DllImport/loader failure
            // inside surfaces here wrapped in TypeInitializationException.
            // Must stay inside try.
            ffmpeg.RootPath = nativeDir;

            // Touch one function so the P/Invoke loader resolves the native
            // libs early — anything missing surfaces here instead of mid-stream.
            _ = ffmpeg.av_version_info();
            ffmpeg.avformat_network_init();
        }
        catch (Exception ex) when (IsNativeLoadFailure(ex))
        {
            // Most-likely cause on Android: the APK shipped without FFmpeg
            // shared libs because tools/build-ffmpeg-android.sh wasn't run for
            // the device's ABI (arm64-v8a / x86_64), or the .so are present but
            // depend on something missing (libc++_shared, libmediandk, etc.).
            // Re-throw with the full inner chain so the underlying loader
            // message is preserved.
            var (rid, _) = RuntimeIds.Current();
            var probe = string.IsNullOrEmpty(nativeDir) ? "(loader path)" : nativeDir;
            throw new FfmpegNativeLibsMissingException(
                $"FFmpeg native libraries failed to load for runtime '{rid ?? "unknown"}'. " +
                $"Probed: {probe}. " +
                $"Android: ensure runtimes/android-{{arm64,x64}}/native/*.so are populated " +
                $"(run tools/build-ffmpeg-android.sh via WSL or pull .so from a CI APK artifact). " +
                $"Desktop: tools/fetch-ffmpeg.ps1 (Windows) or apt/brew. " +
                $"Underlying: {DescribeChain(ex)}", ex);
        }
    }

    private static bool IsNativeLoadFailure(Exception ex)
    {
        for (var cur = ex; cur is not null; cur = cur.InnerException)
        {
            if (cur is DllNotFoundException) return true;
            if (cur is BadImageFormatException) return true;
        }
        return ex is TypeInitializationException;
    }

    private static string DescribeChain(Exception ex)
    {
        var sb = new System.Text.StringBuilder();
        for (var cur = ex; cur is not null; cur = cur.InnerException)
        {
            if (sb.Length > 0) sb.Append(" → ");
            sb.Append(cur.GetType().Name).Append(": ").Append(cur.Message);
        }
        return sb.ToString();
    }

    private static string ResolveNativeDir()
    {
        var baseDir = AppContext.BaseDirectory;
        var (rid, _) = RuntimeIds.Current();
        if (rid is not null)
        {
            var candidate = Path.Combine(baseDir, "runtimes", rid, "native");
            if (Directory.Exists(candidate))
                return candidate;
        }

        // Fall back to "" — FFmpeg.AutoGen then uses the platform loader path
        // (apt/brew-installed libs on Linux/macOS, DLLs next to the exe on Windows,
        // and the bundled .so files added via <AndroidNativeLibrary> on Android).
        return "";
    }
}

public sealed class FfmpegNativeLibsMissingException : Exception
{
    public FfmpegNativeLibsMissingException(string message, Exception inner)
        : base(message, inner) { }
}
