using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Merqo.Infrastructure;
using Merqo.Infrastructure.Logging;
using Merqo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Merqo.Desktop.ViewModels;

/// <summary>
/// Startup readiness view. Every value shown is read from the real local database; nothing is hardcoded.
/// </summary>
public sealed partial class SystemStatusViewModel : ObservableObject
{
    private readonly AppPaths _paths;
    private readonly IAppLogger _logger;
    private readonly DatabaseInitializer _initializer;

    [ObservableProperty] private string _databasePath = string.Empty;
    [ObservableProperty] private string _statusHeadline = "প্রস্তুতি চলছে…";
    [ObservableProperty] private string _statusDetail = string.Empty;
    [ObservableProperty] private string _integrityText = "—";
    [ObservableProperty] private string _unitCountText = "—";
    [ObservableProperty] private string _productCountText = "—";
    [ObservableProperty] private string _databaseSizeText = "—";
    [ObservableProperty] private string _journalModeText = "—";
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isHealthy;

    public SystemStatusViewModel(AppPaths paths, IAppLogger logger, DatabaseInitializer initializer)
    {
        _paths = paths;
        _logger = logger;
        _initializer = initializer;
        DatabasePath = _paths.DatabasePath;
        Version = typeof(App).Assembly.GetName().Version?.ToString(3) ?? "0.0.0";
        BuildLabel = typeof(App).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? Version;
    }

    public string Version { get; }
    public string BuildLabel { get; }
    public string LogsDirectory => _paths.LogsDirectory;
    public string BackupsDirectory => _paths.BackupsDirectory;
    public string ProductName => "তাহাবিল";
    public string BrandName => "MERQO";

    [RelayCommand]
    public async Task RefreshAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        StatusHeadline = "যাচাই করা হচ্ছে…";
        try
        {
            var result = await _initializer.InitializeAsync().ConfigureAwait(true);

            await using var context = TahabilDbContextFactory.Create(_paths.DatabasePath);
            var units = await context.Units.CountAsync().ConfigureAwait(true);
            var products = await context.Products.CountAsync().ConfigureAwait(true);
            var journalMode = await context.Database
                .SqlQueryRaw<string>("PRAGMA journal_mode;")
                .SingleAsync()
                .ConfigureAwait(true);

            IntegrityText = result.IntegrityOk ? "সঠিক (ok)" : "সমস্যা পাওয়া গেছে";
            UnitCountText = units.ToString("N0");
            ProductCountText = products.ToString("N0");
            JournalModeText = journalMode.ToUpperInvariant();
            DatabaseSizeText = FormatSize(_paths.DatabasePath);

            IsHealthy = result.IntegrityOk;
            StatusHeadline = result.IntegrityOk ? "সিস্টেম প্রস্তুত" : "ডাটাবেস যাচাই প্রয়োজন";
            StatusDetail = result.Messages.Count > 0 ? string.Join(" · ", result.Messages) : string.Empty;

            _logger.Info(LogCategories.Startup, "Startup self-check completed", new Dictionary<string, object?>
            {
                ["integrityOk"] = result.IntegrityOk,
                ["units"] = units,
                ["products"] = products,
                ["journalMode"] = journalMode
            });
        }
        catch (Exception ex)
        {
            IsHealthy = false;
            StatusHeadline = "প্রস্তুতি ব্যর্থ";
            StatusDetail = ex.Message;
            _logger.Error(LogCategories.Startup, "Startup self-check failed", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void OpenDataFolder()
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = _paths.Root,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _logger.Warn(LogCategories.Ui, "Could not open data folder", new Dictionary<string, object?> { ["error"] = ex.Message });
        }
    }

    private static string FormatSize(string path)
    {
        try
        {
            var info = new FileInfo(path);
            if (!info.Exists) return "—";
            var bytes = info.Length;
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:0.0} KB";
            return $"{bytes / (1024.0 * 1024.0):0.00} MB";
        }
        catch (IOException)
        {
            return "—";
        }
    }
}
