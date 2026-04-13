using Avalonia;
using Maestro.Next.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Maestro.Next;

internal static class Program
{
    internal static ServiceProvider? Services { get; private set; }

    [STAThread]
    public static void Main(string[] args)
    {
        // Build DI container
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        // Start Avalonia application
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    private static void ConfigureServices(IServiceCollection services)
    {
        // Core services
        services.AddSingleton<IConnectionService, ConnectionService>();
        services.AddSingleton<IResourceService, ResourceService>();
        services.AddSingleton<INotificationService, NotificationService>();

        // Workbench-level singletons
        services.AddSingleton<ViewModels.DocumentManagerViewModel>();
        services.AddSingleton<ViewModels.SiteExplorerViewModel>();
        services.AddSingleton<ViewModels.MainWindowViewModel>();

        // Per-use ViewModels — transient
        services.AddTransient<ViewModels.LoginViewModel>();
    }
}
