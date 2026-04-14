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
                    ScaleRanges.Add(new ScaleRangeSummaryViewModel(sr, () => IsDirty = true));
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

    // ── Scale range management ──────────────────────────────────
    [ObservableProperty] private ScaleRangeSummaryViewModel? _selectedScaleRange;

    [RelayCommand]
    private void AddScaleRange()
    {
        if (_layerDef?.SubLayer is not IVectorLayerDefinition vld) return;

        try
        {
            // Get the schema version of this layer definition
            var verStr = _layerDef.ResourceVersion.ToString();
            var ver = new Version(verStr);

            var tmpLayer = OSGeo.MapGuide.ObjectModels.ObjectFactory.CreateDefaultLayer(
                OSGeo.MapGuide.ObjectModels.LayerDefinition.LayerType.Vector, ver);

            if (tmpLayer.SubLayer is IVectorLayerDefinition tmpVld)
            {
                var newRange = tmpVld.VectorScaleRange.First();
                vld.AddVectorScaleRange(newRange);
                ScaleRanges.Add(new ScaleRangeSummaryViewModel(newRange, () => IsDirty = true));
                IsDirty = true;
            }
        }
        catch (Exception ex)
        {
            Program.Services!.GetRequiredService<INotificationService>()
                   .Error($"Failed to add scale range: {ex.Message}");
        }
    }

    [RelayCommand]
    private void RemoveScaleRange()
    {
        if (_layerDef?.SubLayer is not IVectorLayerDefinition vld) return;
        if (SelectedScaleRange is null) return;

        vld.RemoveVectorScaleRange(SelectedScaleRange.UnderlyingRange);
        ScaleRanges.Remove(SelectedScaleRange);
        SelectedScaleRange = null;
        IsDirty = true;
    }
}

/// <summary>Summary row for a vector scale range with expandable style rules</summary>
public partial class ScaleRangeSummaryViewModel : ViewModelBase
{
    private readonly IVectorScaleRange _sr;

    public ScaleRangeSummaryViewModel(IVectorScaleRange sr, Action? onChanged = null)
    {
        _sr = sr;
        UnderlyingRange = sr;
        MinScale = sr.MinScale.HasValue
            ? $"1:{sr.MinScale.Value:N0}"
            : "0 (always)";
        MaxScale = sr.MaxScale.HasValue
            ? $"1:{sr.MaxScale.Value:N0}"
            : "∞ (always)";

        // Populate style rules
        if (sr.PointStyle != null)
        {
            for (int i = 0; i < sr.PointStyle.RuleCount; i++)
            {
                var rule = sr.PointStyle.GetRuleAt(i);
                Rules.Add(new StyleRuleViewModel("Point", i, rule, onChanged));
            }
        }
        if (sr.LineStyle != null)
        {
            for (int i = 0; i < sr.LineStyle.RuleCount; i++)
            {
                var rule = sr.LineStyle.GetRuleAt(i);
                Rules.Add(new StyleRuleViewModel("Line", i, rule, onChanged));
            }
        }
        if (sr.AreaStyle != null)
        {
            for (int i = 0; i < sr.AreaStyle.RuleCount; i++)
            {
                var rule = sr.AreaStyle.GetRuleAt(i);
                Rules.Add(new StyleRuleViewModel("Area", i, rule, onChanged));
            }
        }
    }

    public string MinScale { get; }
    public string MaxScale { get; }
    public string Summary => $"{MinScale} → {MaxScale}  ({Rules.Count} rule(s))";
    public IVectorScaleRange UnderlyingRange { get; }
    public ObservableCollection<StyleRuleViewModel> Rules { get; } = new();

    [ObservableProperty] private bool _isExpanded;
}

public partial class StyleRuleViewModel : ViewModelBase
{
    private readonly IVectorRule _rule;
    private readonly Action? _onChanged;

    public StyleRuleViewModel(string geometryType, int index, IVectorRule rule, Action? onChanged = null)
    {
        GeometryType = geometryType;
        Index = index;
        _rule = rule;
        _onChanged = onChanged;
        LegendLabel = rule.LegendLabel ?? "";
        Filter = rule.Filter ?? "";
    }

    public string GeometryType { get; }
    public int Index { get; }

    [ObservableProperty] private string _legendLabel;
    [ObservableProperty] private string _filter;

    partial void OnLegendLabelChanged(string value)
    {
        _rule.LegendLabel = value;
        _onChanged?.Invoke();
    }

    partial void OnFilterChanged(string value)
    {
        _rule.Filter = value;
        _onChanged?.Invoke();
    }

    /// <summary>Raised when UI requests the expression builder for the filter</summary>
    public Func<string, string, string?, Task<string?>>? EditFilterRequested { get; set; }

    [RelayCommand]
    private async Task EditFilterAsync()
    {
        if (EditFilterRequested is null) return;
        var result = await EditFilterRequested(string.Empty, string.Empty, Filter);
        if (result != null)
            Filter = result;
    }

    public string Icon => GeometryType switch
    {
        "Point" => "📍",
        "Line"  => "📏",
        "Area"  => "🔲",
        _       => "❔"
    };

    public string DisplayName => $"{Icon} {GeometryType} Rule #{Index}: {LegendLabel}";
}
