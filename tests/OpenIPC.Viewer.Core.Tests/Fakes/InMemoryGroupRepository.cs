using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenIPC.Viewer.Core.Entities;
using OpenIPC.Viewer.Core.Persistence;

namespace OpenIPC.Viewer.Core.Tests.Fakes;

internal sealed class InMemoryGroupRepository : IGroupRepository
{
    private readonly Dictionary<GroupId, CameraGroup> _groups = new();

    public Task<IReadOnlyList<CameraGroup>> GetAllAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<CameraGroup>>(_groups.Values.OrderBy(g => g.SortOrder).ToList());

    public Task<CameraGroup?> GetAsync(GroupId id, CancellationToken ct) =>
        Task.FromResult(_groups.TryGetValue(id, out var g) ? g : null);

    public Task<GroupId> AddAsync(string name, int sortOrder, CancellationToken ct)
    {
        var id = new GroupId(_groups.Count + 1);
        _groups[id] = new CameraGroup(id, name, sortOrder, System.DateTime.UtcNow);
        return Task.FromResult(id);
    }

    public Task RenameAsync(GroupId id, string name, CancellationToken ct)
    {
        if (_groups.TryGetValue(id, out var g))
            _groups[id] = g with { Name = name };
        return Task.CompletedTask;
    }

    public Task RemoveAsync(GroupId id, CancellationToken ct)
    {
        _groups.Remove(id);
        return Task.CompletedTask;
    }
}
