// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maestro.Next.Services;
using Microsoft.Extensions.DependencyInjection;
using OSGeo.MapGuide.ObjectModels;

namespace Maestro.Next.ViewModels;

public partial class SiteExplorerViewModel : ViewModelBase
{
    private readonly IResourceService _resourceService;
    private readonly DocumentManagerViewModel _documentManager;
    private readonly INewResourceService _newResourceService;
    private readonly INotificationService _notifications;

    /// <summary>Raised when UI must show the NewResource dialog for the given folder</summary>
    public Func<string, Task<string?>>? NewResourceRequested { get; set; }

    /// <summary>Raised when UI must confirm deletion of a resource</summary>
    public Func<string, Task<bool>>? DeleteConfirmRequested { get; set; }

    public SiteExplorerViewModel(
        IResourceService resourceService,
        DocumentManagerViewModel documentManager,
        INewResourceService newResourceService,
        INotificationService notifications)
    {
        _resourceService    = resourceService;
        _documentManager    = documentManager;
        _newResourceService = newResourceService;
        _notifications      = notifications;
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private ResourceTreeNode? _selectedNode;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSearchActive))]
    private string _searchText = string.Empty;

    public bool IsSearchActive => !string.IsNullOrWhiteSpace(SearchText);

    public ObservableCollection<ResourceTreeNode> RootNodes { get; } = new();

    /// <summary>Flat list of all leaf (resource) nodes — used for search</summary>
    private readonly List<ResourceTreeNode> _flatLeaves = new();

    /// <summary>Search results (shown when IsSearchActive)</summary>
    public ObservableCollection<ResourceTreeNode> SearchResults { get; } = new();

    partial void OnSearchTextChanged(string value) => ApplyFilter(value);

    private void ApplyFilter(string text)
    {
        SearchResults.Clear();
        if (string.IsNullOrWhiteSpace(text)) return;

        var lower = text.ToLowerInvariant();
        foreach (var node in _flatLeaves)
            if (node.Name.Contains(lower, StringComparison.OrdinalIgnoreCase)
                || node.ResourceType.Contains(lower, StringComparison.OrdinalIgnoreCase))
                SearchResults.Add(node);
    }

    [RelayCommand]
    private void ClearSearch() => SearchText = string.Empty;

    [RelayCommand]
    private async Task LoadRootAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            RootNodes.Clear();
            _flatLeaves.Clear();

            var items = await _resourceService.GetResourceListAsync("Library://");
            foreach (var item in items.OrderByDescending(i => i.IsFolder).ThenBy(i => i.Name))
            {
                var node = new ResourceTreeNode(item, _resourceService, _flatLeaves);
                RootNodes.Add(node);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void OpenResource(ResourceTreeNode? node)
    {
        if (node is null || node.IsFolder) return;

        var doc = ResourceEditorFactory.CreateEditor(node.ResourceId, node.ResourceType);
        if (doc != null)
            _documentManager.OpenDocument(doc);
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (SelectedNode is { IsFolder: true } folder)
            await folder.RefreshAsync();
        else
            await LoadRootAsync();
    }

    [RelayCommand]
    private async Task NewResourceAsync()
    {
        var folderId = SelectedNode?.IsFolder == true
            ? SelectedNode.ResourceId
            : "Library://";

        if (NewResourceRequested is null) return;

        var createdId = await NewResourceRequested.Invoke(folderId);
        if (createdId != null)
            await RefreshAsync();
    }

    [RelayCommand(CanExecute = nameof(CanDeleteSelected))]
    private async Task DeleteSelectedAsync()
    {
        if (SelectedNode is null) return;

        if (DeleteConfirmRequested != null)
        {
            var confirmed = await DeleteConfirmRequested.Invoke(SelectedNode.ResourceId);
            if (!confirmed) return;
        }

        try
        {
            var conn = Program.Services!.GetRequiredService<IConnectionService>()
                              .CurrentConnection!;
            await Task.Run(() => conn.ResourceService.DeleteResource(SelectedNode.ResourceId));
            _notifications.Success($"Deleted: {SelectedNode.Name}");
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            _notifications.Error($"Delete failed: {ex.Message}");
        }
    }

    private bool CanDeleteSelected() =>
        SelectedNode is { IsFolder: false, IsPlaceholder: false };
}

/// <summary>
/// Factory that creates the correct editor ViewModel for a given resource type
/// </summary>
public static class ResourceEditorFactory
{
    public static DocumentViewModel? CreateEditor(string resourceId, string resourceType)
    {
        return resourceType switch
        {
            nameof(ResourceTypes.FeatureSource)         => new FeatureSourceEditorViewModel(resourceId),
            nameof(ResourceTypes.LayerDefinition)       => new LayerDefinitionEditorViewModel(resourceId),
            nameof(ResourceTypes.MapDefinition)         => new MapDefinitionEditorViewModel(resourceId),
            nameof(ResourceTypes.WebLayout)             => new WebLayoutEditorViewModel(resourceId),
            nameof(ResourceTypes.ApplicationDefinition) => new ApplicationDefinitionEditorViewModel(resourceId),
            nameof(ResourceTypes.SymbolDefinition)      => new GenericResourceEditorViewModel(resourceId, resourceType),
            nameof(ResourceTypes.PrintLayout)           => new GenericResourceEditorViewModel(resourceId, resourceType),
            nameof(ResourceTypes.LoadProcedure)         => new GenericResourceEditorViewModel(resourceId, resourceType),
            nameof(ResourceTypes.WatermarkDefinition)   => new GenericResourceEditorViewModel(resourceId, resourceType),
            _ => null
        };
    }
}

/// <summary>
/// Represents a node in the Site Explorer tree
/// </summary>
public partial class ResourceTreeNode : ViewModelBase
{
    private readonly IResourceService _resourceService;
    private readonly List<ResourceTreeNode>? _flatLeaves;
    private bool _hasLoadedChildren;

    public ResourceTreeNode(ResourceListItem item, IResourceService resourceService,
        List<ResourceTreeNode>? flatLeaves = null)
    {
        Item = item;
        _resourceService = resourceService;
        _flatLeaves = flatLeaves;
        Icon = ResourceTypeIconMap.GetIcon(item.ResourceType, item.IsFolder);

        if (!item.IsFolder && flatLeaves != null)
            flatLeaves.Add(this);

        if (item.IsFolder)
            Children.Add(PlaceholderNode(resourceService));
    }

    private static ResourceTreeNode PlaceholderNode(IResourceService svc) =>
        new(new ResourceListItem("__placeholder__", "Loading...", "", false), svc)
        { IsPlaceholder = true };

    public ResourceListItem Item { get; }

    public string Name      => Item.Name;
    public string ResourceId  => Item.ResourceId;
    public string ResourceType => Item.ResourceType;
    public bool   IsFolder   => Item.IsFolder;
    public bool   IsPlaceholder { get; private init; }

    /// <summary>Unicode emoji used as icon in the tree</summary>
    public string Icon { get; }

    [ObservableProperty] private bool _isExpanded;
    [ObservableProperty] private bool _isSelected;

    public ObservableCollection<ResourceTreeNode> Children { get; } = new();

    partial void OnIsExpandedChanged(bool value)
    {
        if (value && IsFolder && !_hasLoadedChildren)
            _ = RefreshAsync();
    }

    public async Task RefreshAsync()
    {
        try
        {
            var items = await _resourceService.GetResourceListAsync(ResourceId);
            Children.Clear();
            foreach (var item in items.OrderByDescending(i => i.IsFolder).ThenBy(i => i.Name))
                Children.Add(new ResourceTreeNode(item, _resourceService, _flatLeaves));
            _hasLoadedChildren = true;
        }
        catch
        {
            // Keep placeholder visible on error
        }
    }
}

/// <summary>
/// Maps resource types to display icons (unicode emoji — will be replaced by vector icons later)
/// </summary>
public static class ResourceTypeIconMap
{
    public static string GetIcon(string resourceType, bool isFolder) => resourceType switch
    {
        nameof(ResourceTypes.FeatureSource)         => "🗄",
        nameof(ResourceTypes.LayerDefinition)       => "🗺",
        nameof(ResourceTypes.MapDefinition)         => "🗺",
        nameof(ResourceTypes.WebLayout)             => "🌐",
        nameof(ResourceTypes.ApplicationDefinition) => "📱",
        nameof(ResourceTypes.SymbolDefinition)      => "🔷",
        nameof(ResourceTypes.PrintLayout)           => "🖨",
        nameof(ResourceTypes.LoadProcedure)         => "📥",
        nameof(ResourceTypes.WatermarkDefinition)   => "💧",
        _  when isFolder                            => "📁",
        _                                           => "📄",
    };
}
