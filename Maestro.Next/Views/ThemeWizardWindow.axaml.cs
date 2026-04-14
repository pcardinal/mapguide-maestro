// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Maestro.Next.Views;

public partial class ThemeWizardWindow : Window
{
    public ThemeWizardWindow() => InitializeComponent();

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();

    private void OnGenerateClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.ThemeWizardViewModel vm && vm.Confirmed)
            Close();
    }
}
