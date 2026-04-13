// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Avalonia.Controls;
using Avalonia.Input;
using Maestro.Next.Services;
using Maestro.Next.ViewModels;
using Microsoft.Extensions.DependencyInjection;

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
        if (tree != null) tree.DoubleTapped += OnDoubleTapped;

        var list = this.FindControl<ListBox>("SearchResultsList");
        if (list != null) list.DoubleTapped += OnDoubleTapped;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is SiteExplorerViewModel vm)
        {
            vm.NewResourceRequested    = ShowNewResourceDialogAsync;
            vm.DeleteConfirmRequested  = ShowDeleteConfirmAsync;
        }
    }

    private void OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is SiteExplorerViewModel vm)
            vm.OpenResourceCommand.Execute(vm.SelectedNode);
    }

    private async Task<string?> ShowNewResourceDialogAsync(string targetFolderId)
    {
        var window = VisualRoot as Window;
        if (window is null) return null;

        var newResSvc = Program.Services!.GetRequiredService<INewResourceService>();
        var notifSvc  = Program.Services!.GetRequiredService<INotificationService>();

        var vm = new NewResourceViewModel(targetFolderId, newResSvc, notifSvc);
        var dialog = new NewResourceWindow { DataContext = vm };

        return await dialog.ShowDialog<string?>(window);
    }

    private async Task<bool> ShowDeleteConfirmAsync(string resourceId)
    {
        var window = VisualRoot as Window;
        if (window is null) return false;

        var name = resourceId.TrimEnd('/').Split('/').Last();
        var box = new Window
        {
            Title      = "Confirm Delete",
            Width      = 380, Height = 160,
            CanResize  = false,
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content    = BuildConfirmContent(name, out var yesBtn, out var noBtn)
        };

        bool confirmed = false;
        yesBtn.Click += (_, _) => { confirmed = true; box.Close(); };
        noBtn.Click  += (_, _) => box.Close();

        await box.ShowDialog(window);
        return confirmed;
    }

    private static Panel BuildConfirmContent(string name,
        out Button yesBtn, out Button noBtn)
    {
        var lbl = new TextBlock
        {
            Text        = $"Delete \"{name}\"?\nThis action cannot be undone.",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin      = new Avalonia.Thickness(20, 20, 20, 12)
        };

        yesBtn = new Button { Content = "Delete", Width = 90 };
        yesBtn.Classes.Add("accent");
        noBtn  = new Button { Content = "Cancel", Width = 90 };

        var btnRow = new StackPanel
        {
            Orientation        = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Spacing            = 8,
            Margin             = new Avalonia.Thickness(0, 0, 20, 0),
            Children           = { noBtn, yesBtn }
        };

        return new StackPanel { Children = { lbl, btnRow } };
    }
}
