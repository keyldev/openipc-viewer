using System.Threading;
using System.Threading.Tasks;

namespace OpenIPC.Viewer.Core.Platform;

public interface ISecretsStore
{
    Task<string?> GetAsync(string key, CancellationToken ct);
    Task SetAsync(string key, string value, CancellationToken ct);
    Task RemoveAsync(string key, CancellationToken ct);
}
