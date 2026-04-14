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
        if (tree != null)
        {
            tree.DoubleTapped += OnDoubleTapped;

            // Drag-and-drop
            tree.PointerPressed += OnTreePointerPressed;
            DragDrop.SetAllowDrop(tree, true);
            tree.AddHandler(DragDrop.DragOverEvent, OnDragOver);
            tree.AddHandler(DragDrop.DropEvent, OnDrop);
        }

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
            vm.RenameRequested         = ShowRenameDialogAsync;
            vm.PropertiesRequested     = ShowPropertiesDialogAsync;
            vm.SaveToFileRequested     = ShowSaveToFileDialogAsync;
            vm.SpatialContextsRequested = ShowSpatialContextsAsync;
            vm.DependencyListRequested  = ShowDependencyListAsync;
            vm.RepointRequested         = ShowRepointDialogAsync;
            vm.EditHeaderRequested      = ShowHeaderDialogAsync;
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

    private async Task<string?> ShowRenameDialogAsync(string currentName)
    {
        var window = VisualRoot as Window;
        if (window is null) return null;

        var tb = new TextBox { Text = currentName, Margin = new Avalonia.Thickness(20, 10, 20, 0) };
        var lbl = new TextBlock
        {
            Text   = "Enter new name:",
            Margin = new Avalonia.Thickness(20, 20, 20, 0)
        };

        Button okBtn, cancelBtn;
        okBtn     = new Button { Content = "OK",     Width = 80 };
        cancelBtn = new Button { Content = "Cancel", Width = 80 };
        okBtn.Classes.Add("accent");

        var btnRow = new StackPanel
        {
            Orientation        = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Spacing  = 8,
            Margin   = new Avalonia.Thickness(0, 12, 20, 20),
            Children = { cancelBtn, okBtn }
        };

        var box = new Window
        {
            Title     = "Rename", Width = 380, Height = 170,
            CanResize = false, ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new StackPanel { Children = { lbl, tb, btnRow } }
        };

        string? result = null;
        okBtn.Click     += (_, _) => { result = tb.Text; box.Close(); };
        cancelBtn.Click += (_, _) => box.Close();

        await box.ShowDialog(window);
        return result;
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

    // ── Drag-and-Drop ─────────────────────────────────────────────

    private async Task ShowPropertiesDialogAsync(string resourceId)
    {
        var window = VisualRoot as Window;
        if (window is null) return;

        var vm = new ResourcePropertiesViewModel(resourceId);
        var dialog = new ResourcePropertiesWindow { DataContext = vm };
        await dialog.ShowDialog(window);
    }

    private async Task<string?> ShowSaveToFileDialogAsync(string suggestedName)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider is null) return null;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(
            new Avalonia.Platform.Storage.FilePickerSaveOptions
            {
                SuggestedFileName = suggestedName,
                DefaultExtension = "xml",
                FileTypeChoices =
                [
                    new Avalonia.Platform.Storage.FilePickerFileType("XML") { Patterns = ["*.xml"] },
                    new Avalonia.Platform.Storage.FilePickerFileType("All files") { Patterns = ["*"] }
                ]
            });
        return file?.Path.LocalPath;
    }

    private async Task ShowSpatialContextsAsync(string resourceId)
    {
        var window = VisualRoot as Window;
        if (window is null) return;

        string info;
        try
        {
            var conn = Program.Services!.GetRequiredService<IConnectionService>().CurrentConnection!;
            var feature = conn.FeatureService;
            var contexts = await Task.Run(() => feature.GetSpatialContextInfo(resourceId, false));
            if (contexts.SpatialContext.Count == 0)
            {
                info = "No spatial contexts found.";
            }
            else
            {
                var lines = contexts.SpatialContext.Select(sc =>
                    $"• {sc.Name}\n  CS: {sc.CoordinateSystemName}\n  WKT: {(sc.CoordinateSystemWkt?.Length > 80 ? sc.CoordinateSystemWkt[..80] + "..." : sc.CoordinateSystemWkt)}\n  Extent: ({sc.Extent?.LowerLeftCoordinate?.X}, {sc.Extent?.LowerLeftCoordinate?.Y}) → ({sc.Extent?.UpperRightCoordinate?.X}, {sc.Extent?.UpperRightCoordinate?.Y})");
                info = string.Join("\n\n", lines);
            }
        }
        catch (Exception ex)
        {
            info = $"Error: {ex.Message}";
        }

        var box = new Window
        {
            Title = "Spatial Contexts",
            Width = 500, Height = 350,
            CanResize = true,
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new ScrollViewer
            {
                Content = new TextBlock
                {
                    Text = info,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    FontFamily = new Avalonia.Media.FontFamily("Consolas,Monospace"),
                    FontSize = 12,
                    Margin = new Avalonia.Thickness(16)
                }
            }
        };
        await box.ShowDialog(window);
    }

    private ResourceTreeNode? _dragSource;

    private async void OnTreePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Drag-and-drop initiation — Avalonia 12 uses a different API.
        // For now, we use Cut/Copy/Paste via the clipboard service (A.1).
        // Full DnD will be implemented when Avalonia 12's DataTransfer API stabilizes.
        // This handler captures the drag source for potential future use.
        if (DataContext is not SiteExplorerViewModel vm) return;
        if (vm.SelectedNode is null or { IsPlaceholder: true }) return;

        var point = e.GetCurrentPoint(sender as Avalonia.Visual);
        if (!point.Properties.IsLeftButtonPressed) return;

        _dragSource = vm.SelectedNode;
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        // Accept drops on folder nodes
        e.DragEffects = DragDropEffects.None;
        if (DataContext is SiteExplorerViewModel vm && vm.SelectedNode is { IsFolder: true })
            e.DragEffects = DragDropEffects.Move;
    }

    private async void OnDrop(object? sender, DragEventArgs e)
    {
        if (DataContext is not SiteExplorerViewModel vm) return;
        if (_dragSource is null) return;

        var targetFolder = vm.SelectedNode?.IsFolder == true
            ? vm.SelectedNode.ResourceId
            : "Library://";

        var srcId = _dragSource.ResourceId;
        var name = srcId.TrimEnd('/').Split('/').Last();
        var destId = targetFolder.TrimEnd('/') + "/" + name;
        if (srcId.EndsWith("/", StringComparison.Ordinal))
            destId += "/";

        try
        {
            var conn = Program.Services!.GetRequiredService<IConnectionService>()
                              .CurrentConnection!;
            var notif = Program.Services!.GetRequiredService<INotificationService>();

            await Task.Run(() => conn.ResourceService.MoveResource(srcId, destId, false));
            notif.Success($"Moved to {targetFolder}");

            await vm.RefreshCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            Program.Services!.GetRequiredService<INotificationService>()
                   .Error($"Drop failed: {ex.Message}");
        }
        finally
        {
            _dragSource = null;
        }
    }

    private async Task ShowDependencyListAsync(string resourceId, string info)
    {
        var window = VisualRoot as Window;
        if (window is null) return;

        var box = new Window
        {
            Title = $"Dependencies — {resourceId}",
            Width = 500, Height = 350,
            CanResize = true,
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new ScrollViewer
            {
                Content = new TextBlock
                {
                    Text = info,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    FontFamily = new Avalonia.Media.FontFamily("Consolas,Monospace"),
                    FontSize = 12,
                    Margin = new Avalonia.Thickness(16)
                }
            }
        };
        await box.ShowDialog(window);
    }

    private async Task<string?> ShowRepointDialogAsync(string currentFeatureSourceId)
    {
        var window = VisualRoot as Window;
        if (window is null) return null;

        string? result = null;
        var inputBox = new TextBox
        {
            Text = currentFeatureSourceId,
            Watermark = "Library://Data/NewFeatureSource.FeatureSource",
            FontFamily = new Avalonia.Media.FontFamily("Consolas,Monospace"),
            FontSize = 11,
            Margin = new Avalonia.Thickness(16, 8, 16, 4)
        };

        var label = new TextBlock
        {
            Text = $"Repoint all references from:\n  {currentFeatureSourceId}\n\nNew FeatureSource ID:",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Avalonia.Thickness(16, 12, 16, 0)
        };

        var okBtn = new Button { Content = "Repoint", Width = 90 };
        okBtn.Classes.Add("accent");
        var cancelBtn = new Button { Content = "Cancel", Width = 90 };

        var btnRow = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Spacing = 8,
            Margin = new Avalonia.Thickness(0, 8, 16, 0),
            Children = { cancelBtn, okBtn }
        };

        var dialog = new Window
        {
            Title = "Repoint Feature Source",
            Width = 500, Height = 220,
            CanResize = false,
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new StackPanel { Children = { label, inputBox, btnRow } }
        };

        okBtn.Click += (_, _) => { result = inputBox.Text; dialog.Close(); };
        cancelBtn.Click += (_, _) => dialog.Close();

        await dialog.ShowDialog(window);
        return result;
    }

    private async Task ShowHeaderDialogAsync(string resourceId, string headerXml)
    {
        var window = VisualRoot as Window;
        if (window is null) return;

        var box = new Window
        {
            Title = $"Resource Header — {resourceId}",
            Width = 550, Height = 400,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ShowInTaskbar = false,
            Content = new ScrollViewer
            {
                Content = new TextBox
                {
                    Text = headerXml,
                    IsReadOnly = true,
                    AcceptsReturn = true,
                    TextWrapping = Avalonia.Media.TextWrapping.NoWrap,
                    FontFamily = new Avalonia.Media.FontFamily("Consolas,Monospace"),
                    FontSize = 12,
                    Margin = new Avalonia.Thickness(8)
                }
            }
        };
        await box.ShowDialog(window);
    }
}
