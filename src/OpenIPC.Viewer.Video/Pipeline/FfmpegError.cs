using System;
using FFmpeg.AutoGen.Abstractions;

namespace OpenIPC.Viewer.Video.Pipeline;

internal static class FfmpegError
{
    public static unsafe string Describe(int code)
    {
        const int bufSize = 1024;
        var buffer = stackalloc byte[bufSize];
        ffmpeg.av_strerror(code, buffer, (ulong)bufSize);
        return System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)buffer) ?? $"FFmpeg error {code}";
    }

    public static void ThrowIfError(int code, string operation)
    {
        if (code < 0)
            throw new FfmpegException(operation, code);
    }
}

internal sealed class FfmpegException : Exception
{
    public int Code { get; }

    public FfmpegException(string operation, int code)
        : base($"{operation} failed: {FfmpegError.Describe(code)} ({code})")
    {
        Code = code;
    }
}
