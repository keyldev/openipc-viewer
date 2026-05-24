using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenIPC.Viewer.Core.Entities;

namespace OpenIPC.Viewer.Core.Persistence;

public interface ICameraRepository
{
    Task<IReadOnlyList<Camera>> GetAllAsync(CancellationToken ct);
    Task<Camera?> GetAsync(CameraId id, CancellationToken ct);
    Task<CameraId> AddAsync(Camera camera, CancellationToken ct);
    Task UpdateAsync(Camera camera, CancellationToken ct);
    Task RemoveAsync(CameraId id, CancellationToken ct);
}
