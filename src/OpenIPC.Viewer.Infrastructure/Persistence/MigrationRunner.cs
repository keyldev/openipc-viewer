using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using OpenIPC.Viewer.Core.Persistence;

namespace OpenIPC.Viewer.Infrastructure.Persistence;

public sealed partial class MigrationRunner : IMigrationRunner
{
    private const string MigrationResourcePrefix = "OpenIPC.Viewer.Infrastructure.Persistence.Migrations.";

    private readonly IDbConnectionFactory _factory;
    private readonly ILogger<MigrationRunner> _logger;
    private readonly Assembly _resourceAssembly;

    public MigrationRunner(IDbConnectionFactory factory, ILogger<MigrationRunner> logger)
    {
        _factory = factory;
        _logger = logger;
        _resourceAssembly = typeof(MigrationRunner).Assembly;
    }

    public async Task MigrateAsync(CancellationToken ct)
    {
        await using var connection = _factory.OpenConnection();

        await connection.ExecuteAsync("PRAGMA journal_mode = WAL;").ConfigureAwait(false);
        await connection.ExecuteAsync("""
            CREATE TABLE IF NOT EXISTS Schema (
                Version   INTEGER PRIMARY KEY,
                AppliedAt TEXT NOT NULL
            );
            """).ConfigureAwait(false);

        var currentVersion = await connection
            .ExecuteScalarAsync<long?>("SELECT COALESCE(MAX(Version), 0) FROM Schema;")
            .ConfigureAwait(false) ?? 0L;

        var pending = DiscoverMigrations()
            .Where(m => m.Version > currentVersion)
            .OrderBy(m => m.Version)
            .ToList();

        if (pending.Count == 0)
        {
            _logger.LogInformation("Database is at version {Version}; no migrations to apply.", currentVersion);
            return;
        }

        foreach (var migration in pending)
        {
            ct.ThrowIfCancellationRequested();
            await using var tx = await connection.BeginTransactionAsync(ct).ConfigureAwait(false);

            _logger.LogInformation("Applying migration {Version} ({Name})", migration.Version, migration.Name);
            await connection.ExecuteAsync(migration.Sql, transaction: tx).ConfigureAwait(false);
            await connection.ExecuteAsync(
                "INSERT INTO Schema (Version, AppliedAt) VALUES (@version, @appliedAt);",
                new { version = migration.Version, appliedAt = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture) },
                transaction: tx).ConfigureAwait(false);

            await tx.CommitAsync(ct).ConfigureAwait(false);
        }
    }

    private IEnumerable<Migration> DiscoverMigrations()
    {
        var names = _resourceAssembly.GetManifestResourceNames()
            .Where(n => n.StartsWith(MigrationResourcePrefix, StringComparison.Ordinal) &&
                        n.EndsWith(".sql", StringComparison.OrdinalIgnoreCase));

        foreach (var resource in names)
        {
            var fileName = resource.Substring(MigrationResourcePrefix.Length);
            var match = MigrationFileRegex().Match(fileName);
            if (!match.Success)
                continue;

            var version = long.Parse(match.Groups["version"].Value, CultureInfo.InvariantCulture);
            using var stream = _resourceAssembly.GetManifestResourceStream(resource)
                ?? throw new InvalidOperationException($"Embedded migration {resource} missing");
            using var reader = new StreamReader(stream);
            var sql = reader.ReadToEnd();

            yield return new Migration(version, Path.GetFileNameWithoutExtension(fileName), sql);
        }
    }

    private readonly record struct Migration(long Version, string Name, string Sql);

    [GeneratedRegex("^(?<version>\\d+)_.*\\.sql$", RegexOptions.IgnoreCase)]
    private static partial Regex MigrationFileRegex();
}
