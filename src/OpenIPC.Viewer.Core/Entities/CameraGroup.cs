using System;

namespace OpenIPC.Viewer.Core.Entities;

public sealed record CameraGroup(
    GroupId Id,
    string Name,
    int SortOrder,
    DateTime CreatedAt);
