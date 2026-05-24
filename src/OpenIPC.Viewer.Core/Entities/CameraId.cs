using System;

namespace OpenIPC.Viewer.Core.Entities;

public readonly record struct CameraId(Guid Value)
{
    public static CameraId New() => new(Guid.NewGuid());

    public static CameraId Parse(string text) => new(Guid.Parse(text));

    public override string ToString() => Value.ToString("D");
}
