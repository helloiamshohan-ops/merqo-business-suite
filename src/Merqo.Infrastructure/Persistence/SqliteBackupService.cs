using System.Security.Cryptography;
using Microsoft.Data.Sqlite;

namespace Merqo.Infrastructure.Persistence;

public sealed record BackupResult(string FilePath, long SizeBytes, string Sha256, bool Verified, DateTime CreatedAtUtc);

/// <summary>
/// SQLite-aware backup, verification and restore.
/// A backup is taken through the SQLite **online backup API**, never by copying the live file, so the
/// copy is transactionally consistent and includes WAL content while the shop keeps working.
/// </summary>
public sealed class SqliteBackupService
{
    private const string SqliteHeader = "SQLite format 3";

    public async Task<BackupResult> CreateAsync(
        string sourceDatabasePath,
        string destinationDirectory,
        string kind,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(sourceDatabasePath))
            throw new FileNotFoundException("Cannot back up a database that does not exist.", sourceDatabasePath);

        Directory.CreateDirectory(destinationDirectory);
        var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff");
        var safeKind = string.Concat(kind.Where(c => char.IsLetterOrDigit(c) || c is '-' or '_'));
        var destinationPath = Path.Combine(destinationDirectory, $"tahabil-{safeKind}-{stamp}.db");

        var sourceConnectionString = TahabilDbContextFactory.BuildConnectionString(sourceDatabasePath);
        var destinationConnectionString = TahabilDbContextFactory.BuildConnectionString(destinationPath);

        await using (var source = new SqliteConnection(sourceConnectionString))
        await using (var destination = new SqliteConnection(destinationConnectionString))
        {
            await source.OpenAsync(cancellationToken).ConfigureAwait(false);
            await destination.OpenAsync(cancellationToken).ConfigureAwait(false);
            source.BackupDatabase(destination);
            await destination.CloseAsync().ConfigureAwait(false);
        }

        SqliteConnection.ClearAllPools();

        var verified = await VerifyAsync(destinationPath, cancellationToken).ConfigureAwait(false);
        var info = new FileInfo(destinationPath);
        var hash = await ComputeSha256Async(destinationPath, cancellationToken).ConfigureAwait(false);

        return new BackupResult(destinationPath, info.Length, hash, verified, DateTime.UtcNow);
    }

    /// <summary>Opens the file as a database and runs <c>PRAGMA integrity_check</c> plus a header sanity check.</summary>
    public async Task<bool> VerifyAsync(string databasePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(databasePath)) return false;

            await using (var stream = new FileStream(databasePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var header = new byte[16];
                var read = await stream.ReadAsync(header, cancellationToken).ConfigureAwait(false);
                if (read < 16) return false;
                if (!System.Text.Encoding.ASCII.GetString(header, 0, 15).Equals(SqliteHeader, StringComparison.Ordinal)) return false;
            }

            await using var connection = new SqliteConnection(TahabilDbContextFactory.BuildConnectionString(databasePath));
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            await using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA integrity_check;";
            var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false) as string;
            return string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase);
        }
        catch (SqliteException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }

    /// <summary>
    /// Replaces the live database with a verified backup. The backup is re-verified first, and the
    /// restore is refused if it is not intact — the application never destroys the current file
    /// without proof that the replacement is usable.
    /// </summary>
    public async Task RestoreAsync(string backupPath, string targetDatabasePath, CancellationToken cancellationToken = default)
    {
        if (!await VerifyAsync(backupPath, cancellationToken).ConfigureAwait(false))
            throw new InvalidOperationException("Refusing to restore: the backup failed integrity verification.");

        // Any pooled connection would keep stale pages/handles on the old file.
        SqliteConnection.ClearAllPools();

        foreach (var suffix in new[] { "-wal", "-shm" })
        {
            var sidecar = targetDatabasePath + suffix;
            if (File.Exists(sidecar)) File.Delete(sidecar);
        }

        File.Copy(backupPath, targetDatabasePath, overwrite: true);

        if (!await VerifyAsync(targetDatabasePath, cancellationToken).ConfigureAwait(false))
            throw new InvalidOperationException("Restore completed but the database failed integrity verification.");
    }

    public static async Task<string> ComputeSha256Async(string filePath, CancellationToken cancellationToken = default)
    {
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var hash = await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
