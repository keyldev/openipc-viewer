using System;

namespace OpenIPC.Viewer.Core.Video;

// Frame data is BGRA8888 (matches Avalonia WriteableBitmap default).
// The byte[] is owned by the session — subscriber must copy synchronously
// inside OnNext, never retain the reference past the callback.
public readonly record struct VideoFrame(
    byte[] Bgra,
    int Width,
    int Height,
    int Stride,
    long PtsMicroseconds,
    DateTime ReceivedAt);
