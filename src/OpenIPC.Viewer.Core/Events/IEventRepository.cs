using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenIPC.Viewer.Core.Entities;

namespace OpenIPC.Viewer.Core.Events;

public interface IEventRepository
{
    Task<IReadOnlyList<CameraEvent>> ListAsync(CameraId? cameraId, EventKind? kind, DateTime? since, int limit, CancellationToken ct);
    Task AddAsync(CameraEvent ev, CancellationToken ct);
    Task UpdateAsync(CameraEvent ev, CancellationToken ct);
    Task<int> DeleteOlderThanAsync(DateTime cutoff, CancellationToken ct);
}
