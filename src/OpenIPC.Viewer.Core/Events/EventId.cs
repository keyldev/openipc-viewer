using System;

namespace OpenIPC.Viewer.Core.Events;

public readonly record struct EventId(Guid Value)
{
    public static EventId New() => new(Guid.NewGuid());

    public static EventId Parse(string text) => new(Guid.Parse(text));

    public override string ToString() => Value.ToString("D");
}
