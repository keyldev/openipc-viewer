using System.Collections.Concurrent;
using OpenIPC.Viewer.Core.Platform;

namespace OpenIPC.Viewer.Core.Tests.Fakes;

internal sealed class InMemorySecretsStore : ISecretsStore
{
    public readonly ConcurrentDictionary<string, string> Items = new();

    public Task<string?> GetAsync(string key, CancellationToken ct) =>
        Task.FromResult(Items.TryGetValue(key, out var v) ? v : null);

    public Task SetAsync(string key, string value, CancellationToken ct)
    {
        Items[key] = value;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken ct)
    {
        Items.TryRemove(key, out _);
        return Task.CompletedTask;
    }
}
