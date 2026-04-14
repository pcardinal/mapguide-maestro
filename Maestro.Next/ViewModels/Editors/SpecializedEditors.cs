// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maestro.Next.Services;
using Microsoft.Extensions.DependencyInjection;
using OSGeo.MapGuide.ObjectModels;

namespace Maestro.Next.ViewModels.Editors;

/// <summary>
/// Editor for DrawingSource resources (DWF files)
/// </summary>
public partial class DrawingSourceEditorViewModel : DocumentViewModel
{
    public override string IconKey => "DrawingSource";

    public DrawingSourceEditorViewModel(string resourceId)
    {
        ResourceId = resourceId;
        Title = System.IO.Path.GetFileNameWithoutExtension(
            resourceId.TrimEnd('/').Split('/').Last()) + " [DrawingSource]";
    }

    [ObservableProperty] private string _sourceName = string.Empty;
    [ObservableProperty] private string _coordinateSpace = string.Empty;
    public ObservableCollection<string> Sheets { get; } = new();

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            BusyMessage = "Loading drawing source...";
            var conn = Program.Services!.GetRequiredService<IConnectionService>().CurrentConnection!;
            var res = await Task.Run(() => conn.ResourceService.GetResource(ResourceId!));

            if (res is OSGeo.MapGuide.ObjectModels.DrawingSource.IDrawingSource ds)
            {
                SourceName = ds.SourceName ?? string.Empty;
                CoordinateSpace = ds.CoordinateSpace ?? string.Empty;
                foreach (var sheet in ds.Sheet)
                    Sheets.Add($"{sheet.Name} (Extent: {sheet.Extent?.MinX:F2},{sheet.Extent?.MinY:F2} → {sheet.Extent?.MaxX:F2},{sheet.Extent?.MaxY:F2})");
            }
            IsLoaded = true;
        }
        catch (Exception ex)
        {
            Program.Services!.GetRequiredService<INotificationService>()
                   .Error($"Load failed: {ex.Message}");
        }
        finally { IsBusy = false; BusyMessage = null; }
    }

    [ObservableProperty] private bool _isLoaded;

    protected override Task SaveAsync()
    {
        Program.Services!.GetRequiredService<INotificationService>()
               .Info("Use 'Edit as XML' to modify this resource.");
        return Task.CompletedTask;
    }
}

/// <summary>
/// Editor for WatermarkDefinition resources
/// </summary>
public partial class WatermarkEditorViewModel : DocumentViewModel
{
    public override string IconKey => "WatermarkDefinition";

    public WatermarkEditorViewModel(string resourceId)
    {
        ResourceId = resourceId;
        Title = System.IO.Path.GetFileNameWithoutExtension(
            resourceId.TrimEnd('/').Split('/').Last()) + " [Watermark]";
    }

    [ObservableProperty] private string _watermarkType = string.Empty;
    [ObservableProperty] private string _resourceContent = string.Empty;
    [ObservableProperty] private bool _isLoaded;

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            var conn = Program.Services!.GetRequiredService<IConnectionService>().CurrentConnection!;
            var res = await Task.Run(() => conn.ResourceService.GetResource(ResourceId!));
            ResourceContent = res?.Serialize() ?? "(empty)";
            WatermarkType = res?.ResourceType ?? "WatermarkDefinition";
            IsLoaded = true;
        }
        catch (Exception ex)
        {
            Program.Services!.GetRequiredService<INotificationService>()
                   .Error($"Load failed: {ex.Message}");
        }
        finally { IsBusy = false; BusyMessage = null; }
    }

    protected override Task SaveAsync()
    {
        Program.Services!.GetRequiredService<INotificationService>()
               .Info("Use 'Edit as XML' to modify this resource.");
        return Task.CompletedTask;
    }
}

/// <summary>
/// Editor for TileSetDefinition resources (MGOS 3.0+)
/// </summary>
public partial class TileSetEditorViewModel : DocumentViewModel
{
    public override string IconKey => "TileSetDefinition";

    public TileSetEditorViewModel(string resourceId)
    {
        ResourceId = resourceId;
        Title = System.IO.Path.GetFileNameWithoutExtension(
            resourceId.TrimEnd('/').Split('/').Last()) + " [TileSet]";
    }

    [ObservableProperty] private string _tileProvider = string.Empty;
    [ObservableProperty] private string _resourceContent = string.Empty;
    [ObservableProperty] private bool _isLoaded;

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            var conn = Program.Services!.GetRequiredService<IConnectionService>().CurrentConnection!;
            var res = await Task.Run(() => conn.ResourceService.GetResource(ResourceId!));

            if (res is OSGeo.MapGuide.ObjectModels.TileSetDefinition.ITileSetDefinition tsd)
                TileProvider = tsd.TileStoreParameters?.TileProvider ?? "(unknown)";

            ResourceContent = res?.Serialize() ?? "(empty)";
            IsLoaded = true;
        }
        catch (Exception ex)
        {
            Program.Services!.GetRequiredService<INotificationService>()
                   .Error($"Load failed: {ex.Message}");
        }
        finally { IsBusy = false; BusyMessage = null; }
    }

    protected override Task SaveAsync()
    {
        Program.Services!.GetRequiredService<INotificationService>()
               .Info("Use 'Edit as XML' to modify this resource.");
        return Task.CompletedTask;
    }
}
