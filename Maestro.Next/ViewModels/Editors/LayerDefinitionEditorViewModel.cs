// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maestro.Next.Services;
using Microsoft.Extensions.DependencyInjection;
using OSGeo.MapGuide.ObjectModels.LayerDefinition;

namespace Maestro.Next.ViewModels;

public partial class LayerDefinitionEditorViewModel : DocumentViewModel
{
    public override string IconKey => "LayerDefinition";

    private ILayerDefinition? _layerDef;

    public LayerDefinitionEditorViewModel(string resourceId)
    {
        ResourceId = resourceId;
        var name = resourceId.TrimEnd('/').Split('/').Last();
        Title = $"{System.IO.Path.GetFileNameWithoutExtension(name)} [LayerDefinition]";
    }

    // ── common ──────────────────────────────────────────────────
    [ObservableProperty] private bool _isLoaded;
    [ObservableProperty] private string _layerType = string.Empty;

    // ── vector sub-layer ────────────────────────────────────────
    [ObservableProperty] private bool _isVector;
    [ObservableProperty] private string _featureSourceId = string.Empty;
    [ObservableProperty] private string _featureName = string.Empty;
    [ObservableProperty] private string _geometry = string.Empty;
    [ObservableProperty] private string _filter = string.Empty;
    [ObservableProperty] private string _tooltip = string.Empty;

    public ObservableCollection<ScaleRangeSummaryViewModel> ScaleRanges { get; } = new();

    // ── raster sub-layer ────────────────────────────────────────
    [ObservableProperty] private bool _isRaster;
    [ObservableProperty] private string _rasterFeatureName = string.Empty;
    [ObservableProperty] private string _rasterGeometry = string.Empty;

    // ── drawing sub-layer ───────────────────────────────────────
    [ObservableProperty] private bool _isDrawing;
    [ObservableProperty] private string _drawingResourceId = string.Empty;
    [ObservableProperty] private string _drawingSheet = string.Empty;

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            BusyMessage = "Loading layer definition...";

            var conn = Program.Services!.GetRequiredService<IConnectionService>()
                              .CurrentConnection!;

            _layerDef = (ILayerDefinition)await Task.Run(
                () => conn.ResourceService.GetResource(ResourceId!));

            var sub = _layerDef.SubLayer;
            LayerType = sub.LayerType.ToString();

            IsVector  = sub is IVectorLayerDefinition;
            IsRaster  = sub is IRasterLayerDefinition;
            IsDrawing = !IsVector && !IsRaster;

            if (sub is IVectorLayerDefinition vld)
            {
                FeatureSourceId = vld.ResourceId ?? string.Empty;
                FeatureName     = vld.FeatureName ?? string.Empty;
                Geometry        = vld.Geometry ?? string.Empty;
                Filter          = vld.Filter ?? string.Empty;
                Tooltip         = vld.ToolTip ?? string.Empty;

                ScaleRanges.Clear();
                foreach (var sr in vld.VectorScaleRange)
                    ScaleRanges.Add(new ScaleRangeSummaryViewModel(sr));
            }
            else if (sub is IRasterLayerDefinition rld)
            {
                RasterFeatureName = rld.FeatureName ?? string.Empty;
                RasterGeometry    = rld.Geometry ?? string.Empty;
            }
            else if (sub is IDrawingLayerDefinition dld)
            {
                DrawingResourceId = dld.ResourceId ?? string.Empty;
                DrawingSheet      = dld.Sheet ?? string.Empty;
            }

            IsLoaded = true;
        }
        catch (Exception ex)
        {
            Program.Services!.GetRequiredService<INotificationService>()
                   .Error($"Failed to load layer definition: {ex.Message}");
        }
        finally { IsBusy = false; BusyMessage = null; }
    }

    protected override async Task SaveAsync()
    {
        if (_layerDef is null) return;
        try
        {
            IsBusy = true;
            BusyMessage = "Saving...";

            // Write back vector edits
            if (_layerDef.SubLayer is IVectorLayerDefinition vld)
            {
                vld.ResourceId  = FeatureSourceId;
                vld.FeatureName = FeatureName;
                vld.Geometry    = Geometry;
                vld.Filter      = Filter;
                vld.ToolTip     = Tooltip;
            }
            else if (_layerDef.SubLayer is IDrawingLayerDefinition dld)
            {
                dld.ResourceId = DrawingResourceId;
                dld.Sheet      = DrawingSheet;
            }

            var conn = Program.Services!.GetRequiredService<IConnectionService>()
                              .CurrentConnection!;

            await Task.Run(() => conn.ResourceService.SaveResource(_layerDef));

            IsDirty = false;
            Program.Services!.GetRequiredService<INotificationService>()
                   .Success("Layer definition saved.");
        }
        catch (Exception ex)
        {
            Program.Services!.GetRequiredService<INotificationService>()
                   .Error($"Save failed: {ex.Message}");
        }
        finally { IsBusy = false; BusyMessage = null; }
    }
}

/// <summary>Summary row for a vector scale range</summary>
public partial class ScaleRangeSummaryViewModel : ViewModelBase
{
    public ScaleRangeSummaryViewModel(IVectorScaleRange sr)
    {
        MinScale = sr.MinScale.HasValue
            ? $"1:{sr.MinScale.Value:N0}"
            : "0 (always)";
        MaxScale = sr.MaxScale.HasValue
            ? $"1:{sr.MaxScale.Value:N0}"
            : "∞ (always)";

        // Count non-null styles
        int count = 0;
        if (sr.PointStyle  != null) count++;
        if (sr.LineStyle   != null) count++;
        if (sr.AreaStyle   != null) count++;
        StyleCount = count;
    }

    public string MinScale  { get; }
    public string MaxScale  { get; }
    public int    StyleCount { get; }
    public string Summary   => $"{MinScale} → {MaxScale}  ({StyleCount} styles)";
}
