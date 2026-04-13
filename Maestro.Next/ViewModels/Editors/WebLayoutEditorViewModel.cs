// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maestro.Next.Services;
using Microsoft.Extensions.DependencyInjection;
using OSGeo.MapGuide.ObjectModels.WebLayout;

namespace Maestro.Next.ViewModels;

public partial class WebLayoutEditorViewModel : DocumentViewModel
{
    public override string IconKey => "WebLayout";

    private IWebLayout? _webLayout;

    public WebLayoutEditorViewModel(string resourceId)
    {
        ResourceId = resourceId;
        var name = resourceId.TrimEnd('/').Split('/').Last();
        Title = $"{System.IO.Path.GetFileNameWithoutExtension(name)} [WebLayout]";
    }

    [ObservableProperty] private bool _isLoaded;

    // ── General ──────────────────────────────────────────────────
    [ObservableProperty] private string _layoutTitle = string.Empty;
    [ObservableProperty] private string _mapDefinitionId = string.Empty;

    // ── Initial View ─────────────────────────────────────────────
    [ObservableProperty] private string _initialViewCenterX = string.Empty;
    [ObservableProperty] private string _initialViewCenterY = string.Empty;
    [ObservableProperty] private string _initialViewScale = string.Empty;

    // ── UI Panels visibility ──────────────────────────────────────
    [ObservableProperty] private bool _toolbarVisible;
    [ObservableProperty] private bool _statusBarVisible;
    [ObservableProperty] private bool _zoomControlVisible;
    [ObservableProperty] private bool _taskPaneVisible;
    [ObservableProperty] private bool _infoLegendVisible;
    [ObservableProperty] private bool _infoPropertiesVisible;

    // ── Task Pane ────────────────────────────────────────────────
    [ObservableProperty] private string _initialTaskUrl = string.Empty;

    // ── Toolbar items summary ────────────────────────────────────
    public ObservableCollection<ToolbarItemViewModel> ToolbarItems { get; } = new();

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            BusyMessage = "Loading web layout...";

            var conn = Program.Services!.GetRequiredService<IConnectionService>()
                              .CurrentConnection!;

            _webLayout = (IWebLayout)await Task.Run(
                () => conn.ResourceService.GetResource(ResourceId!));

            // General
            LayoutTitle      = _webLayout.Title ?? string.Empty;
            MapDefinitionId  = _webLayout.Map?.ResourceId ?? string.Empty;

            // Initial view
            var view = _webLayout.Map?.InitialView;
            InitialViewCenterX = view?.CenterX.ToString("F4") ?? string.Empty;
            InitialViewCenterY = view?.CenterY.ToString("F4") ?? string.Empty;
            InitialViewScale   = view?.Scale.ToString("N0") ?? string.Empty;

            // Panel visibility
            ToolbarVisible       = _webLayout.ToolBar?.Visible     ?? false;
            StatusBarVisible     = _webLayout.StatusBar?.Visible   ?? false;
            ZoomControlVisible   = _webLayout.ZoomControl?.Visible ?? false;
            TaskPaneVisible      = _webLayout.TaskPane?.Visible    ?? false;
            InfoLegendVisible     = _webLayout.InformationPane?.LegendVisible    ?? false;
            InfoPropertiesVisible  = _webLayout.InformationPane?.PropertiesVisible ?? false;

            // Task pane URL
            InitialTaskUrl = _webLayout.TaskPane?.InitialTask ?? string.Empty;

            // Toolbar items
            ToolbarItems.Clear();
            if (_webLayout.ToolBar != null)
            {
                foreach (var item in _webLayout.ToolBar.Items)
                {
                    ToolbarItems.Add(item.Function switch
                    {
                        UIItemFunctionType.Separator => new ToolbarItemViewModel("—", "Separator"),
                        UIItemFunctionType.Command   => new ToolbarItemViewModel(
                            (item as ICommandItem)?.Command ?? "(command)", "Command"),
                        UIItemFunctionType.Flyout    => new ToolbarItemViewModel(
                            (item as IFlyoutItem)?.Label ?? "(flyout)", "Flyout"),
                        _ => new ToolbarItemViewModel("?", "Unknown")
                    });
                }
            }

            IsLoaded = true;
        }
        catch (Exception ex)
        {
            Program.Services!.GetRequiredService<INotificationService>()
                   .Error($"Failed to load web layout: {ex.Message}");
        }
        finally { IsBusy = false; BusyMessage = null; }
    }

    protected override async Task SaveAsync()
    {
        if (_webLayout is null) return;
        try
        {
            IsBusy = true;
            BusyMessage = "Saving...";

            _webLayout.Title = LayoutTitle;
            if (_webLayout.Map != null)
                _webLayout.Map.ResourceId = MapDefinitionId;

            if (_webLayout.ToolBar  != null) _webLayout.ToolBar.Visible      = ToolbarVisible;
            if (_webLayout.StatusBar != null) _webLayout.StatusBar.Visible   = StatusBarVisible;
            if (_webLayout.ZoomControl != null) _webLayout.ZoomControl.Visible = ZoomControlVisible;
            if (_webLayout.TaskPane != null)
            {
                _webLayout.TaskPane.Visible     = TaskPaneVisible;
                _webLayout.TaskPane.InitialTask = InitialTaskUrl;
            }
            if (_webLayout.InformationPane != null)
            {
                _webLayout.InformationPane.LegendVisible    = InfoLegendVisible;
                _webLayout.InformationPane.PropertiesVisible = InfoPropertiesVisible;
            }

            var conn = Program.Services!.GetRequiredService<IConnectionService>()
                              .CurrentConnection!;
            await Task.Run(() => conn.ResourceService.SaveResource(_webLayout));

            IsDirty = false;
            Program.Services!.GetRequiredService<INotificationService>()
                   .Success("Web layout saved.");
        }
        catch (Exception ex)
        {
            Program.Services!.GetRequiredService<INotificationService>()
                   .Error($"Save failed: {ex.Message}");
        }
        finally { IsBusy = false; BusyMessage = null; }
    }
}

public record ToolbarItemViewModel(string Label, string Kind);
