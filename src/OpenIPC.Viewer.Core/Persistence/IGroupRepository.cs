using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenIPC.Viewer.Core.Entities;

namespace OpenIPC.Viewer.Core.Persistence;

public interface IGroupRepository
{
    Task<IReadOnlyList<CameraGroup>> GetAllAsync(CancellationToken ct);
    Task<CameraGroup?> GetAsync(GroupId id, CancellationToken ct);
    Task<GroupId> AddAsync(string name, int sortOrder, CancellationToken ct);
    Task RenameAsync(GroupId id, string name, CancellationToken ct);
    Task RemoveAsync(GroupId id, CancellationToken ct);
}
