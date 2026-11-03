using Microsoft.EntityFrameworkCore.Design;

namespace Merqo.Infrastructure.Persistence;

/// <summary>
/// Used only by the EF Core tooling (dotnet ef migrations add / script). Never used at runtime:
/// the application always resolves its real data directory through <see cref="AppPaths"/>.
/// </summary>
public sealed class TahabilDesignTimeDbContextFactory : IDesignTimeDbContextFactory<TahabilDbContext>
{
    public TahabilDbContext CreateDbContext(string[] args)
    {
        var directory = Path.Combine(Path.GetTempPath(), "tahabil-design-time");
        Directory.CreateDirectory(directory);
        return TahabilDbContextFactory.Create(Path.Combine(directory, "design.db"));
    }
}
