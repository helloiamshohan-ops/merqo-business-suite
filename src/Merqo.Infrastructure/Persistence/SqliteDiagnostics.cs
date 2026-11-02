using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;

namespace Merqo.Infrastructure.Persistence;

/// <summary>
/// Direct ADO.NET access to SQLite pragmas and diagnostics. PRAGMA statements are not composable
/// SQL, so they must not go through EF's <c>SqlQuery</c> pipeline.
/// </summary>
public static class SqliteDiagnostics
{
    public static async Task<T> ScalarAsync<T>(TahabilDbContext context, string sql, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        var connection = context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        var value = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        if (value is null || value is DBNull) throw new InvalidOperationException($"Query returned no value: {sql}");
        if (value is T typed) return typed;
        return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
    }

    /// <summary>Runs <c>PRAGMA integrity_check</c> and returns true when SQLite reports the database as intact.</summary>
    public static async Task<bool> IntegrityOkAsync(TahabilDbContext context, CancellationToken cancellationToken = default)
    {
        var result = await ScalarAsync<string>(context, "PRAGMA integrity_check;", cancellationToken).ConfigureAwait(false);
        return string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase);
    }

    public static Task<string> JournalModeAsync(TahabilDbContext context, CancellationToken cancellationToken = default)
        => ScalarAsync<string>(context, "PRAGMA journal_mode;", cancellationToken);

    public static Task<long> ForeignKeysAsync(TahabilDbContext context, CancellationToken cancellationToken = default)
        => ScalarAsync<long>(context, "PRAGMA foreign_keys;", cancellationToken);

    public static Task<long> SynchronousAsync(TahabilDbContext context, CancellationToken cancellationToken = default)
        => ScalarAsync<long>(context, "PRAGMA synchronous;", cancellationToken);

    /// <summary>Reported storage class ("integer", "text", "real", "null") of a column value.</summary>
    public static Task<string> StorageClassAsync(TahabilDbContext context, string table, string column, string whereClause, CancellationToken cancellationToken = default)
        => ScalarAsync<string>(context, $"SELECT typeof({column}) FROM {table} WHERE {whereClause} LIMIT 1;", cancellationToken);
}
