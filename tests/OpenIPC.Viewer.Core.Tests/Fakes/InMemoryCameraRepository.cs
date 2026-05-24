using System.Collections.Concurrent;
using OpenIPC.Viewer.Core.Entities;
using OpenIPC.Viewer.Core.Persistence;

namespace OpenIPC.Viewer.Core.Tests.Fakes;

internal sealed class InMemoryCameraRepository : ICameraRepository
{
    private readonly ConcurrentDictionary<CameraId, Camera> _items = new();

    public Task<IReadOnlyList<Camera>> GetAllAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Camera>>(_items.Values.OrderBy(c => c.Name).ToList());

    public Task<Camera?> GetAsync(CameraId id, CancellationToken ct) =>
        Task.FromResult(_items.TryGetValue(id, out var c) ? c : null);

    public Task<CameraId> AddAsync(Camera camera, CancellationToken ct)
    {
        _items[camera.Id] = camera;
        return Task.FromResult(camera.Id);
    }

    public Task UpdateAsync(Camera camera, CancellationToken ct)
    {
        _items[camera.Id] = camera;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(CameraId id, CancellationToken ct)
    {
        _items.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
