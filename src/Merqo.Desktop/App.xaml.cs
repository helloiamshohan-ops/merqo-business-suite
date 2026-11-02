using System.Windows;
using System.Windows.Threading;
using Merqo.Desktop.ViewModels;
using Merqo.Infrastructure;
using Merqo.Infrastructure.Logging;
using Merqo.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Merqo.Desktop;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    public static AppPaths Paths { get; private set; } = null!;
    public static IAppLogger Logger { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Paths = new AppPaths();
        Paths.EnsureCreated();
        Logger = new FileAppLogger(Paths.LogsDirectory);

        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception ex) Logger.Critical(LogCategories.Startup, "Unhandled domain-level exception", ex);
        };

        Logger.Info(LogCategories.Startup, "Application starting", new Dictionary<string, object?>
        {
            ["version"] = typeof(App).Assembly.GetName().Version?.ToString(),
            ["dataRoot"] = Paths.Root
        });

        var services = new ServiceCollection();
        services.AddSingleton(Paths);
        services.AddSingleton(Logger);
        services.AddSingleton<DatabaseInitializer>();
        services.AddTransient<SystemStatusViewModel>();
        Services = services.BuildServiceProvider();

        var viewModel = Services.GetRequiredService<SystemStatusViewModel>();
        var window = new MainWindow { DataContext = viewModel };
        MainWindow = window;
        window.Show();

        _ = viewModel.RefreshAsync();
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Logger.Critical(LogCategories.Startup, "Unhandled UI exception", e.Exception);
        MessageBox.Show(
            "একটি অপ্রত্যাশিত সমস্যা হয়েছে। আপনার ডাটা নিরাপদ আছে।\n\n" +
            "সমস্যাটি পুনরায় হলে অ্যাপ্লিকেশনটি বন্ধ করে আবার চালু করুন।\n" +
            $"প্রযুক্তিগত লগ: {Paths.LogsDirectory}",
            "তাহাবিল — ত্রুটি",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }
}
