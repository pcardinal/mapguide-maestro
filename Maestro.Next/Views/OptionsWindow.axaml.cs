// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Maestro.Next.Views;

public partial class OptionsWindow : Window
{
    public OptionsWindow() => InitializeComponent();

    private void OnCancelClick(object? sender, RoutedEventArgs e) => Close();
    private void OnApplyClick(object? sender, RoutedEventArgs e) => Close();
}
