using System.Threading;
using System.Threading.Tasks;

namespace OpenIPC.Viewer.Core.Persistence;

public interface IMigrationRunner
{
    Task MigrateAsync(CancellationToken ct);
}
