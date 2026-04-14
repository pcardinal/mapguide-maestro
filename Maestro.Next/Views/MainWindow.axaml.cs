// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Avalonia.Controls;
using Maestro.Next.Services;
using Maestro.Next.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Maestro.Next.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Closing += OnWindowClosing;
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        // Load user preferences (theme, etc.) at startup
        var prefs = Program.Services!.GetRequiredService<IPreferencesService>();
        prefs.Load();

        if (DataContext is MainWindowViewModel vm)
        {
            vm.LoginRequested += ShowLoginDialogAsync;
            vm.AboutRequested += ShowAboutDialogAsync;
            vm.OptionsRequested += ShowOptionsDialogAsync;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.LoginRequested -= ShowLoginDialogAsync;
            vm.AboutRequested -= ShowAboutDialogAsync;
            vm.OptionsRequested -= ShowOptionsDialogAsync;
        }
        base.OnClosed(e);
    }

    private async Task<bool> ShowLoginDialogAsync()
    {
        var connectionService = Program.Services!.GetRequiredService<IConnectionService>();
        var loginVm = new LoginViewModel(connectionService);
        var dialog = new LoginWindow { DataContext = loginVm };

        var result = await dialog.ShowDialog<bool?>(this);
        return result == true;
    }

    private async Task ShowAboutDialogAsync()
    {
        var dialog = new AboutWindow();
        await dialog.ShowDialog(this);
    }

    private async Task ShowOptionsDialogAsync()
    {
        var prefs = Program.Services!.GetRequiredService<IPreferencesService>();
        var vm = new OptionsViewModel(prefs);
        var dialog = new OptionsWindow { DataContext = vm };
        await dialog.ShowDialog(this);
    }

    private async void OnLoadPackageClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var vm = new LoadPackageViewModel();
        var dialog = new LoadPackageWindow { DataContext = vm };
        await dialog.ShowDialog(this);

        // Refresh the site explorer after upload
        if (vm.Completed && DataContext is MainWindowViewModel mainVm)
            await mainVm.SiteExplorer.RefreshCommand.ExecuteAsync(null);
    }

    private async void OnWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        if (DataContext is not MainWindowViewModel mainVm) return;

        var dirtyDocs = mainVm.Documents.OpenDocuments.Where(d => d.IsDirty).ToList();
        if (dirtyDocs.Count == 0) return;

        // Cancel the close, show confirmation
        e.Cancel = true;

        var names = string.Join("\n", dirtyDocs.Select(d => $"  • {d.Title}"));
        var msg = new TextBlock
        {
            Text = $"You have {dirtyDocs.Count} unsaved document(s):\n{names}\n\nDiscard changes and close?",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Avalonia.Thickness(20, 20, 20, 12)
        };

        var discardBtn = new Button { Content = "Discard & Close", Width = 130 };
        discardBtn.Classes.Add("accent");
        var cancelBtn = new Button { Content = "Cancel", Width = 90 };

        var btnRow = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Spacing = 8,
            Margin = new Avalonia.Thickness(0, 0, 20, 0),
            Children = { cancelBtn, discardBtn }
        };

        var box = new Window
        {
            Title = "Unsaved Changes",
            Width = 420, Height = 200,
            CanResize = false,
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new StackPanel { Children = { msg, btnRow } }
        };

        bool discard = false;
        discardBtn.Click += (_, _) => { discard = true; box.Close(); };
        cancelBtn.Click += (_, _) => box.Close();

        await box.ShowDialog(this);

        if (discard)
        {
            // Unsubscribe to avoid re-entrancy, then close
            Closing -= OnWindowClosing;
            Close();
        }
    }
}
