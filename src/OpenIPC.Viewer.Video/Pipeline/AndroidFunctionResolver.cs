using System;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;

namespace OpenIPC.Viewer.Video.Pipeline;

// Backport of FFmpeg.AutoGen 8.x's mobile fix (Ruslan-B/FFmpeg.AutoGen PR #344)
// for the pinned 7.1.1 binding. In 7.1.1, FunctionResolverFactory.Create() throws
// PlatformNotSupportedException when Environment.OSVersion.Platform isn't one of
// Win32NT/Unix/MacOSX — and on Mono/Android it's reported as Other. We pre-set
// DynamicallyLoadedBindings.FunctionResolver from FfmpegRuntime so Initialize()
// sees a non-null resolver and skips the factory call entirely.
//
// Loading uses System.Runtime.InteropServices.NativeLibrary which routes through
// Android's dynamic linker — bare "libavcodec.so" resolves out of the APK's
// lib/<abi>/ directory packaged by <AndroidNativeLibrary>.
internal sealed class AndroidFunctionResolver : FunctionResolverBase
{
    // Android .so are unversioned (no libavcodec.so.61 chain inside an APK —
    // see --disable-symver in tools/build-ffmpeg-android.sh).
    protected override string GetNativeLibraryName(string libraryName, int version)
        => "lib" + libraryName + ".so";

    protected override IntPtr LoadNativeLibrary(string libraryName)
        => NativeLibrary.Load(libraryName);

    protected override IntPtr FindFunctionPointer(IntPtr libraryHandle, string functionName)
    {
        NativeLibrary.TryGetExport(libraryHandle, functionName, out var p);
        return p;
    }
}
