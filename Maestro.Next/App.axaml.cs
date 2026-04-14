using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Maestro.Next.ViewModels;
using Maestro.Next.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Maestro.Next;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var vm = Program.Services!.GetRequiredService<MainWindowViewModel>();

            var main = new MainWindow { DataContext = vm };

            // Show splash briefly, then switch to main window
            var splash = new SplashWindow();
            desktop.MainWindow = splash;
            splash.Show();

            // Use a timer to close splash after a short delay
            var timer = new System.Threading.Timer(_ =>
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    desktop.MainWindow = main;
                    main.Show();
                    splash.Close();
                });
            }, null, 1200, System.Threading.Timeout.Infinite);
        }

        base.OnFrameworkInitializationCompleted();
    }
}
