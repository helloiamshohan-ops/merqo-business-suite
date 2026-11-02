namespace Merqo.Infrastructure;

/// <summary>
/// Resolves every mutable runtime location. Business data lives in the user's local application-data
/// folder (never in Program Files) so that uninstall and version upgrades cannot destroy a shop's books.
/// </summary>
public sealed class AppPaths
{
    public const string EnvironmentOverride = "TAHABIL_DATA_DIR";
    public const string FolderName = "MERQO";
    public const string ProductFolderName = "Tahabil";

    public AppPaths(string? rootOverride = null)
    {
        var configured = rootOverride
                         ?? Environment.GetEnvironmentVariable(EnvironmentOverride)
                         ?? Path.Combine(
                             Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
                                 Environment.SpecialFolderOption.Create),
                             FolderName,
                             ProductFolderName);

        Root = Path.GetFullPath(configured);
        DatabasePath = Path.Combine(Root, Persistence.TahabilDbContext.DatabaseFileName);
        LogsDirectory = Path.Combine(Root, "logs");
        BackupsDirectory = Path.Combine(Root, "backups");
        LicensesDirectory = Path.Combine(Root, "license");
        ExportsDirectory = Path.Combine(Root, "exports");
        DocumentsDirectory = Path.Combine(Root, "documents");
    }

    public string Root { get; }
    public string DatabasePath { get; }
    public string LogsDirectory { get; }
    public string BackupsDirectory { get; }
    public string LicensesDirectory { get; }
    public string ExportsDirectory { get; }
    public string DocumentsDirectory { get; }

    public IReadOnlyList<string> AllDirectories => new[]
    {
        Root, LogsDirectory, BackupsDirectory, LicensesDirectory, ExportsDirectory, DocumentsDirectory
    };

    public void EnsureCreated()
    {
        Directory.CreateDirectory(Root);
        foreach (var directory in AllDirectories) Directory.CreateDirectory(directory);
    }
}
