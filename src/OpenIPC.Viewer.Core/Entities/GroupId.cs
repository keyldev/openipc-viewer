namespace OpenIPC.Viewer.Core.Entities;

// Note: schema (architecture §7.1) uses INTEGER PRIMARY KEY AUTOINCREMENT for Groups,
// so GroupId wraps an int even though phase-01 prose mentions GUID for both ids.
public readonly record struct GroupId(int Value)
{
    public override string ToString() => Value.ToString();
}
