// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maestro.Next.Services;
using Microsoft.Extensions.DependencyInjection;
using OSGeo.MapGuide.ObjectModels.MapDefinition;

namespace Maestro.Next.ViewModels;

public partial class MapDefinitionEditorViewModel : DocumentViewModel
{
    public override string IconKey => "MapDefinition";

    private IMapDefinition? _mapDef;

    public MapDefinitionEditorViewModel(string resourceId)
    {
        ResourceId = resourceId;
        var name = resourceId.TrimEnd('/').Split('/').Last();
        Title = $"{System.IO.Path.GetFileNameWithoutExtension(name)} [MapDefinition]";
    }

    [ObservableProperty] private bool _isLoaded;
    [ObservableProperty] private string _mapName = string.Empty;
    [ObservableProperty] private string _coordinateSystem = string.Empty;
    [ObservableProperty] private string _extents = string.Empty;
    [ObservableProperty] private MapLayerTreeNode? _selectedNode;

    /// <summary>Flat tree: groups at root, layers nested under their group</summary>
    public ObservableCollection<MapLayerTreeNode> LayerTree { get; } = new();

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            BusyMessage = "Loading map definition...";

            var conn = Program.Services!.GetRequiredService<IConnectionService>()
                              .CurrentConnection!;

            _mapDef = (IMapDefinition)await Task.Run(
                () => conn.ResourceService.GetResource(ResourceId!));

            MapName          = _mapDef.Name ?? string.Empty;
            CoordinateSystem = _mapDef.CoordinateSystem ?? string.Empty;

            var ext = _mapDef.Extents;
            Extents = ext != null
                ? $"({ext.MinX:F4}, {ext.MinY:F4}) — ({ext.MaxX:F4}, {ext.MaxY:F4})"
                : "(not set)";

            BuildLayerTree();
            IsLoaded = true;
        }
        catch (Exception ex)
        {
            Program.Services!.GetRequiredService<INotificationService>()
                   .Error($"Failed to load map definition: {ex.Message}");
        }
        finally { IsBusy = false; BusyMessage = null; }
    }

    private void BuildLayerTree()
    {
        if (_mapDef is null) return;
        LayerTree.Clear();

        // Group nodes (layers that have a group are children of it)
        var groupNodes = new Dictionary<string, MapLayerTreeNode>(StringComparer.Ordinal);
        foreach (var grp in _mapDef.MapLayerGroup)
        {
            var node = new MapLayerTreeNode(grp.Name, true, grp.Visible, grp.LegendLabel);
            groupNodes[grp.Name] = node;
            LayerTree.Add(node);
        }

        // Layer nodes — nested under their group if it exists, else at root
        foreach (var layer in _mapDef.MapLayer)
        {
            var node = new MapLayerTreeNode(layer.Name, false, layer.Visible,
                layer.LegendLabel, layer.ResourceId, layer.Selectable);

            if (!string.IsNullOrEmpty(layer.Group) && groupNodes.TryGetValue(layer.Group, out var grpNode))
                grpNode.Children.Add(node);
            else
                LayerTree.Add(node);
        }
    }

    [RelayCommand(CanExecute = nameof(CanMoveUp))]
    private void MoveUp()
    {
        if (_mapDef is null || SelectedNode is null) return;

        if (SelectedNode.IsGroup)
        {
            var grp = _mapDef.MapLayerGroup
                .FirstOrDefault(g => g.Name == SelectedNode.Name);
            if (grp != null) _mapDef.MoveUpGroup(grp);
        }
        else
        {
            var layer = _mapDef.MapLayer
                .FirstOrDefault(l => l.Name == SelectedNode.Name);
            if (layer != null) _mapDef.MoveUp(layer);
        }

        IsDirty = true;
        BuildLayerTree();
    }

    [RelayCommand(CanExecute = nameof(CanMoveDown))]
    private void MoveDown()
    {
        if (_mapDef is null || SelectedNode is null) return;

        if (SelectedNode.IsGroup)
        {
            var grp = _mapDef.MapLayerGroup
                .FirstOrDefault(g => g.Name == SelectedNode.Name);
            if (grp != null) _mapDef.MoveDownGroup(grp);
        }
        else
        {
            var layer = _mapDef.MapLayer
                .FirstOrDefault(l => l.Name == SelectedNode.Name);
            if (layer != null) _mapDef.MoveDown(layer);
        }

        IsDirty = true;
        BuildLayerTree();
    }

    private bool CanMoveUp()   => SelectedNode != null;
    private bool CanMoveDown() => SelectedNode != null;

    protected override async Task SaveAsync()
    {
        if (_mapDef is null) return;
        try
        {
            IsBusy = true;
            BusyMessage = "Saving...";

            _mapDef.Name             = MapName;
            _mapDef.CoordinateSystem = CoordinateSystem;

            var conn = Program.Services!.GetRequiredService<IConnectionService>()
                              .CurrentConnection!;

            await Task.Run(() => conn.ResourceService.SaveResource(_mapDef));

            IsDirty = false;
            Program.Services!.GetRequiredService<INotificationService>()
                   .Success("Map definition saved.");
        }
        catch (Exception ex)
        {
            Program.Services!.GetRequiredService<INotificationService>()
                   .Error($"Save failed: {ex.Message}");
        }
        finally { IsBusy = false; BusyMessage = null; }
    }
}

/// <summary>A node in the map layer tree (group or layer)</summary>
public partial class MapLayerTreeNode : ViewModelBase
{
    public MapLayerTreeNode(
        string name, bool isGroup, bool visible, string legendLabel,
        string? resourceId = null, bool selectable = true)
    {
        Name        = name;
        IsGroup     = isGroup;
        Visible     = visible;
        LegendLabel = legendLabel;
        ResourceId  = resourceId ?? string.Empty;
        Selectable  = selectable;
        Icon        = isGroup ? "📁" : "🗺";
    }

    public string Name        { get; }
    public bool   IsGroup     { get; }
    public string Icon        { get; }
    public string ResourceId  { get; }
    public string LegendLabel { get; }

    [ObservableProperty] private bool _visible;
    [ObservableProperty] private bool _selectable;

    public ObservableCollection<MapLayerTreeNode> Children { get; } = new();
}
