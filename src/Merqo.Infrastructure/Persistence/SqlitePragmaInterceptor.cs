using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Merqo.Infrastructure.Persistence;

/// <summary>
/// Applies the durability and concurrency pragmas required by the architecture on every opened
/// connection. WAL keeps readers running during a sale; FULL synchronous guarantees an acknowledged
/// sale survives a power cut; the busy timeout prevents spurious "database is locked" errors when the
/// backup thread and the UI touch the file at the same time.
/// </summary>
public sealed class SqlitePragmaInterceptor : DbConnectionInterceptor
{
    public const int BusyTimeoutMilliseconds = 8000;

    private static readonly string[] Pragmas =
    {
        "PRAGMA journal_mode=WAL;",
        "PRAGMA synchronous=FULL;",
        "PRAGMA foreign_keys=ON;",
        $"PRAGMA busy_timeout={BusyTimeoutMilliseconds};",
        "PRAGMA temp_store=MEMORY;",
        "PRAGMA cache_size=-16000;"
    };

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        => Execute(connection);

    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
        => await ExecuteAsync(connection, cancellationToken).ConfigureAwait(false);

    private static void Execute(DbConnection connection)
    {
        foreach (var pragma in Pragmas)
        {
            using var command = connection.CreateCommand();
            command.CommandText = pragma;
            command.ExecuteNonQuery();
        }
    }

    private static async Task ExecuteAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        foreach (var pragma in Pragmas)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = pragma;
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
