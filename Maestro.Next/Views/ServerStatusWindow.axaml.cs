// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Maestro.Next.Views;

public partial class ServerStatusWindow : Window
{
    public ServerStatusWindow() => InitializeComponent();

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}
