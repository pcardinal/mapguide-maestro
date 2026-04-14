// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace Maestro.Next.Views;

public partial class CreatePackageWindow : Window
{
    public CreatePackageWindow() => InitializeComponent();

    private async void OnBrowseOutputClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = GetTopLevel(this);
        if (topLevel?.StorageProvider is null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Package As",
            SuggestedFileName = "package.mgp",
            DefaultExtension = "mgp",
            FileTypeChoices =
            [
                new FilePickerFileType("MapGuide Package") { Patterns = ["*.mgp"] },
                new FilePickerFileType("ZIP Archive") { Patterns = ["*.zip"] },
                new FilePickerFileType("All files") { Patterns = ["*"] }
            ]
        });

        if (file != null && DataContext is ViewModels.CreatePackageViewModel vm)
            vm.OutputFilePath = file.Path.LocalPath;
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}
