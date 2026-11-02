using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

        var renderPath = GetArgumentValue(e.Args, "--render-ui");
        if (renderPath is not null)
        {
            // Automated visual verification: render the real window to a PNG and exit.
            _ = RenderUiToFileAsync(viewModel, renderPath);
            return;
        }

        var window = new MainWindow { DataContext = viewModel };
        MainWindow = window;
        window.Show();

        _ = viewModel.RefreshAsync();
    }

    private static string? GetArgumentValue(string[] args, string name)
    {
        var index = Array.IndexOf(args, name);
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }

    private async Task RenderUiToFileAsync(SystemStatusViewModel viewModel, string outputPath)
    {
        try
        {
            var window = new MainWindow { DataContext = viewModel, Width = 1180, Height = 720 };
            MainWindow = window;
            window.Show();

            await viewModel.RefreshAsync();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            window.UpdateLayout();
            await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);

            var width = (int)Math.Max(window.ActualWidth, 960);
            var height = (int)Math.Max(window.ActualHeight, 600);
            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(window);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            var directory = Path.GetDirectoryName(Path.GetFullPath(outputPath));
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            await using var stream = File.Create(outputPath);
            encoder.Save(stream);

            Logger.Info(LogCategories.Ui, "UI rendered to file", new Dictionary<string, object?>
            {
                ["path"] = outputPath,
                ["width"] = width,
                ["height"] = height
            });
            Shutdown(0);
        }
        catch (Exception ex)
        {
            Logger.Critical(LogCategories.Ui, "UI render failed", ex);
            Shutdown(2);
        }
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
