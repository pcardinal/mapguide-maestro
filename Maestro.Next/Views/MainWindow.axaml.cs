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

        if (DataContext is MainWindowViewModel vm)
        {
            vm.LoginRequested += ShowLoginDialogAsync;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.LoginRequested -= ShowLoginDialogAsync;
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
}