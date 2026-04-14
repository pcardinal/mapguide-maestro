// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace Maestro.Next.Views;

public partial class LoadPackageWindow : Window
{
    public LoadPackageWindow() => InitializeComponent();

    private async void OnBrowseClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = GetTopLevel(this);
        if (topLevel?.StorageProvider is null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select MapGuide Package",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("MapGuide Package") { Patterns = ["*.mgp"] },
                new FilePickerFileType("All files") { Patterns = ["*"] }
            ]
        });

        if (files.Count > 0 && DataContext is ViewModels.LoadPackageViewModel vm)
            vm.PackageFilePath = files[0].Path.LocalPath;
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}
