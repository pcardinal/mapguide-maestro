// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Avalonia.Controls;
using Maestro.Next.ViewModels;

namespace Maestro.Next.Views.Editors;

public partial class XmlEditorView : UserControl
{
    public XmlEditorView() => InitializeComponent();

    protected override async void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is XmlEditorViewModel vm && !vm.IsLoaded)
            await vm.LoadCommand.ExecuteAsync(null);
    }

    private async void OnFindReplaceClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not XmlEditorViewModel xmlVm) return;
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is not Window window) return;

        var frVm = new FindReplaceViewModel(
            () => xmlVm.XmlContent,
            text => xmlVm.XmlContent = text);

        var dialog = new FindReplaceWindow { DataContext = frVm };
        await dialog.ShowDialog(window);
    }
}
