using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using OpenIPC.Viewer.Core.Entities;
using OpenIPC.Viewer.Core.Events;
using OpenIPC.Viewer.Core.Persistence;

namespace OpenIPC.Viewer.Infrastructure.Persistence;

public sealed class SqliteEventRepository : IEventRepository
{
    private readonly IDbConnectionFactory _factory;

    public SqliteEventRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IReadOnlyList<CameraEvent>> ListAsync(CameraId? cameraId, EventKind? kind, DateTime? since, int limit, CancellationToken ct)
    {
        var sql = new StringBuilder("SELECT * FROM Events WHERE 1=1");
        var p = new DynamicParameters();
        if (cameraId is { } cid)
        {
            sql.Append(" AND CameraId = @cid");
            p.Add("cid", cid.ToString());
        }
        if (kind is { } k)
        {
            sql.Append(" AND Kind = @kind");
            p.Add("kind", (int)k);
        }
        if (since is { } s)
        {
            sql.Append(" AND OccurredAt >= @since");
            p.Add("since", s.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture));
        }
        sql.Append(" ORDER BY OccurredAt DESC LIMIT @limit;");
        p.Add("limit", limit);

        await using var conn = _factory.OpenConnection();
        var rows = await conn.QueryAsync<Row>(sql.ToString(), p).ConfigureAwait(false);
        return rows.Select(Map).ToList();
    }

    public async Task AddAsync(CameraEvent ev, CancellationToken ct)
    {
        await using var conn = _factory.OpenConnection();
        await conn.ExecuteAsync(
            """
            INSERT INTO Events (Id, CameraId, Kind, Severity, OccurredAt, EndedAt, Source, Summary)
            VALUES (@Id, @CameraId, @Kind, @Severity, @OccurredAt, @EndedAt, @Source, @Summary);
            """,
            ToRow(ev)).ConfigureAwait(false);
    }

    public async Task UpdateAsync(CameraEvent ev, CancellationToken ct)
    {
        await using var conn = _factory.OpenConnection();
        await conn.ExecuteAsync(
            """
            UPDATE Events SET
                Kind = @Kind, Severity = @Severity,
                OccurredAt = @OccurredAt, EndedAt = @EndedAt,
                Source = @Source, Summary = @Summary
            WHERE Id = @Id;
            """,
            ToRow(ev)).ConfigureAwait(false);
    }

    public async Task<int> DeleteOlderThanAsync(DateTime cutoff, CancellationToken ct)
    {
        await using var conn = _factory.OpenConnection();
        return await conn.ExecuteAsync(
            "DELETE FROM Events WHERE OccurredAt < @cutoff;",
            new { cutoff = cutoff.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture) }).ConfigureAwait(false);
    }

    private static CameraEvent Map(Row r) => new(
        Id: EventId.Parse(r.Id),
        CameraId: CameraId.Parse(r.CameraId),
        Kind: (EventKind)r.Kind,
        Severity: (EventSeverity)r.Severity,
        OccurredAt: DateTime.Parse(r.OccurredAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
        EndedAt: r.EndedAt is null ? null : DateTime.Parse(r.EndedAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
        Source: r.Source,
        Summary: r.Summary);

    private static object ToRow(CameraEvent e) => new
    {
        Id = e.Id.ToString(),
        CameraId = e.CameraId.ToString(),
        Kind = (int)e.Kind,
        Severity = (int)e.Severity,
        OccurredAt = e.OccurredAt.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture),
        EndedAt = e.EndedAt?.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture),
        e.Source,
        e.Summary,
    };

    private sealed class Row
    {
        public string Id { get; init; } = default!;
        public string CameraId { get; init; } = default!;
        public int Kind { get; init; }
        public int Severity { get; init; }
        public string OccurredAt { get; init; } = default!;
        public string? EndedAt { get; init; }
        public string? Source { get; init; }
        public string? Summary { get; init; }
    }
}
