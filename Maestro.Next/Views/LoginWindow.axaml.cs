// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Avalonia.Controls;
using Avalonia.Interactivity;
using Maestro.Next.ViewModels;

namespace Maestro.Next.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Gets the view model, typed for convenience
    /// </summary>
    private LoginViewModel? ViewModel => DataContext as LoginViewModel;

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        // Auto-close when connection succeeds
        if (ViewModel is { } vm)
        {
            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(LoginViewModel.IsConnecting)
                    && !vm.IsConnecting
                    && vm.ConnectionSucceeded)
                {
                    Close(true);
                }
            };
        }
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
