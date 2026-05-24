using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using OpenIPC.Viewer.Core.Entities;
using OpenIPC.Viewer.Core.Persistence;

namespace OpenIPC.Viewer.Infrastructure.Persistence;

public sealed class SqliteGroupRepository : IGroupRepository
{
    private readonly IDbConnectionFactory _factory;

    public SqliteGroupRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<CameraGroup>> GetAllAsync(CancellationToken ct)
    {
        await using var conn = _factory.OpenConnection();
        var rows = await conn.QueryAsync<GroupRow>(
            "SELECT Id, Name, SortOrder, CreatedAt FROM Groups ORDER BY SortOrder, Name COLLATE NOCASE;")
            .ConfigureAwait(false);
        return rows.Select(MapRow).ToList();
    }

    public async Task<CameraGroup?> GetAsync(GroupId id, CancellationToken ct)
    {
        await using var conn = _factory.OpenConnection();
        var row = await conn.QuerySingleOrDefaultAsync<GroupRow>(
            "SELECT Id, Name, SortOrder, CreatedAt FROM Groups WHERE Id = @id;",
            new { id = id.Value }).ConfigureAwait(false);
        return row is null ? null : MapRow(row);
    }

    public async Task<GroupId> AddAsync(string name, int sortOrder, CancellationToken ct)
    {
        await using var conn = _factory.OpenConnection();
        await using var tx = await conn.BeginTransactionAsync(ct).ConfigureAwait(false);
        var id = await conn.ExecuteScalarAsync<long>(
            """
            INSERT INTO Groups (Name, SortOrder, CreatedAt)
            VALUES (@name, @sortOrder, @createdAt);
            SELECT last_insert_rowid();
            """,
            new
            {
                name,
                sortOrder,
                createdAt = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
            },
            transaction: tx).ConfigureAwait(false);
        await tx.CommitAsync(ct).ConfigureAwait(false);
        return new GroupId((int)id);
    }

    public async Task RenameAsync(GroupId id, string name, CancellationToken ct)
    {
        await using var conn = _factory.OpenConnection();
        await using var tx = await conn.BeginTransactionAsync(ct).ConfigureAwait(false);
        var affected = await conn.ExecuteAsync(
            "UPDATE Groups SET Name = @name WHERE Id = @id;",
            new { id = id.Value, name }, transaction: tx).ConfigureAwait(false);
        await tx.CommitAsync(ct).ConfigureAwait(false);
        if (affected == 0)
            throw new InvalidOperationException($"Group {id} not found");
    }

    public async Task RemoveAsync(GroupId id, CancellationToken ct)
    {
        await using var conn = _factory.OpenConnection();
        await using var tx = await conn.BeginTransactionAsync(ct).ConfigureAwait(false);
        await conn.ExecuteAsync(
            "DELETE FROM Groups WHERE Id = @id;",
            new { id = id.Value }, transaction: tx).ConfigureAwait(false);
        await tx.CommitAsync(ct).ConfigureAwait(false);
    }

    private static CameraGroup MapRow(GroupRow row) => new(
        Id: new GroupId((int)row.Id),
        Name: row.Name,
        SortOrder: row.SortOrder,
        CreatedAt: DateTime.Parse(row.CreatedAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));

    private sealed class GroupRow
    {
        public long Id { get; init; }
        public string Name { get; init; } = default!;
        public int SortOrder { get; init; }
        public string CreatedAt { get; init; } = default!;
    }
}
