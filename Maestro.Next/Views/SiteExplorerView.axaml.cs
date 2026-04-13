// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Avalonia.Controls;
using Avalonia.Input;
using Maestro.Next.ViewModels;

namespace Maestro.Next.Views;

public partial class SiteExplorerView : UserControl
{
    public SiteExplorerView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(Avalonia.Interactivity.RoutedEventArgs e)
    {
        base.OnLoaded(e);

        var tree = this.FindControl<TreeView>("ResourceTree");
        if (tree != null)
            tree.DoubleTapped += OnTreeDoubleTapped;
    }

    private void OnTreeDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is SiteExplorerViewModel vm)
            vm.OpenResourceCommand.Execute(vm.SelectedNode);
    }
}
