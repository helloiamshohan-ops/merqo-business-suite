using Merqo.Infrastructure;
using Merqo.Infrastructure.Persistence;

namespace Merqo.Infrastructure.Tests;

/// <summary>Creates a throwaway, fully initialised database per test.</summary>
public sealed class DatabaseTestContext : IAsyncDisposable
{
    private DatabaseTestContext(AppPaths paths)
    {
        Paths = paths;
    }

    public AppPaths Paths { get; }
    public TahabilDbContext Context { get; private set; } = null!;
    public DatabaseInitializationResult Result { get; private set; } = null!;

    public static async Task<DatabaseTestContext> CreateAsync()
    {
        var root = Path.Combine(Path.GetTempPath(), "tahabil-tests", Guid.NewGuid().ToString("N"));
        var paths = new AppPaths(root);
        var initializer = new DatabaseInitializer(paths);
        var result = await initializer.InitializeAsync();
        var context = TahabilDbContextFactory.Create(paths.DatabasePath);
        return new DatabaseTestContext(paths) { Context = context, Result = result };
    }

    public TahabilDbContext NewContext() => TahabilDbContextFactory.Create(Paths.DatabasePath);

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        try
        {
            if (Directory.Exists(Paths.Root)) Directory.Delete(Paths.Root, recursive: true);
        }
        catch (IOException)
        {
        }
    }
}
