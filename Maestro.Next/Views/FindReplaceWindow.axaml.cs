// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Maestro.Next.Views;

public partial class FindReplaceWindow : Window
{
    public FindReplaceWindow() => InitializeComponent();

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}
