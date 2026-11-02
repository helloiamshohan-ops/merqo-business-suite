using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Merqo.Infrastructure.Persistence;

public static class TahabilDbContextFactory
{
    public static string BuildConnectionString(string databasePath) =>
        new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Private,
            Pooling = true,
            ForeignKeys = true
        }.ToString();

    public static DbContextOptions<TahabilDbContext> BuildOptions(string databasePath) =>
        new DbContextOptionsBuilder<TahabilDbContext>()
            .UseSqlite(BuildConnectionString(databasePath))
            .AddInterceptors(new SqlitePragmaInterceptor())
            .EnableDetailedErrors()
            .Options;

    public static TahabilDbContext Create(string databasePath) => new(BuildOptions(databasePath));
}
