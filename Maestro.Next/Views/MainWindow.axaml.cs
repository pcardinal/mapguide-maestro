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
}