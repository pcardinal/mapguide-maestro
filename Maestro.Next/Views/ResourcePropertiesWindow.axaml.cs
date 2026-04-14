// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Avalonia.Controls;
using Avalonia.Interactivity;
using Maestro.Next.ViewModels;

namespace Maestro.Next.Views;

public partial class ResourcePropertiesWindow : Window
{
    public ResourcePropertiesWindow() => InitializeComponent();

    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (DataContext is ResourcePropertiesViewModel vm && !vm.IsLoaded)
            await vm.LoadCommand.ExecuteAsync(null);
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}
