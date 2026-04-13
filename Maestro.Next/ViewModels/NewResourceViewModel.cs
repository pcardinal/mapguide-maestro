// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maestro.Next.Services;

namespace Maestro.Next.ViewModels;

public enum NewResourceKind
{
    Folder,
    FeatureSource,
    LayerDefinition,
    MapDefinition,
    WebLayout,
}

public partial class NewResourceViewModel : ViewModelBase
{
    private readonly INewResourceService _newResourceService;
    private readonly INotificationService _notifications;

    /// <summary>The folder in which to create the new resource</summary>
    public string TargetFolderId { get; }

    public NewResourceViewModel(
        string targetFolderId,
        INewResourceService newResourceService,
        INotificationService notifications)
    {
        TargetFolderId      = targetFolderId;
        _newResourceService = newResourceService;
        _notifications      = notifications;
    }

    // ── common ────────────────────────────────────────────────────
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    private NewResourceKind _kind = NewResourceKind.Folder;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    private string _resourceName = string.Empty;

    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string? _validationError;

    // ── FeatureSource specific ────────────────────────────────────
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    private string _fdoProvider = "OSGeo.SDF";   // sensible default

    // ── LayerDefinition specific ──────────────────────────────────
    [ObservableProperty] private string _featureSourceId = string.Empty;
    [ObservableProperty] private string _featureClass    = string.Empty;
    [ObservableProperty] private string _geometryProperty = "Geometry";

    // ── WebLayout specific ────────────────────────────────────────
    [ObservableProperty] private string _linkedMapDefinitionId = string.Empty;

    // ── Result ───────────────────────────────────────────────────
    public string? CreatedResourceId { get; private set; }
    public bool    Succeeded         { get; private set; }

    // ── Visibility helpers ───────────────────────────────────────
    public bool IsFolder          => Kind == NewResourceKind.Folder;
    public bool IsFeatureSource   => Kind == NewResourceKind.FeatureSource;
    public bool IsLayerDefinition => Kind == NewResourceKind.LayerDefinition;
    public bool IsMapDefinition   => Kind == NewResourceKind.MapDefinition;
    public bool IsWebLayout       => Kind == NewResourceKind.WebLayout;

    partial void OnKindChanged(NewResourceKind value)
    {
        OnPropertyChanged(nameof(IsFolder));
        OnPropertyChanged(nameof(IsFeatureSource));
        OnPropertyChanged(nameof(IsLayerDefinition));
        OnPropertyChanged(nameof(IsMapDefinition));
        OnPropertyChanged(nameof(IsWebLayout));
    }

    private bool CanCreate() =>
        !string.IsNullOrWhiteSpace(ResourceName) &&
        (Kind != NewResourceKind.FeatureSource || !string.IsNullOrWhiteSpace(FdoProvider));

    [RelayCommand(CanExecute = nameof(CanCreate))]
    public async Task CreateAsync()
    {
        if (!Validate()) return;

        try
        {
            IsBusy = true;

            CreatedResourceId = Kind switch
            {
                NewResourceKind.Folder =>
                    await _newResourceService.CreateFolderAsync(TargetFolderId, ResourceName),

                NewResourceKind.FeatureSource =>
                    await _newResourceService.CreateFeatureSourceAsync(
                        TargetFolderId, ResourceName, FdoProvider),

                NewResourceKind.LayerDefinition =>
                    await _newResourceService.CreateLayerDefinitionAsync(
                        TargetFolderId, ResourceName,
                        FeatureSourceId, FeatureClass, GeometryProperty),

                NewResourceKind.MapDefinition =>
                    await _newResourceService.CreateMapDefinitionAsync(
                        TargetFolderId, ResourceName),

                NewResourceKind.WebLayout =>
                    await _newResourceService.CreateWebLayoutAsync(
                        TargetFolderId, ResourceName, LinkedMapDefinitionId),

                _ => throw new InvalidOperationException("Unknown resource kind")
            };

            Succeeded = true;
            _notifications.Success($"Created: {CreatedResourceId}");
        }
        catch (Exception ex)
        {
            ValidationError = ex.Message;
            _notifications.Error($"Create failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool Validate()
    {
        ValidationError = null;

        if (string.IsNullOrWhiteSpace(ResourceName))
        {
            ValidationError = "Name is required.";
            return false;
        }
        if (ResourceName.Contains('/') || ResourceName.Contains('\\'))
        {
            ValidationError = "Name cannot contain path separators.";
            return false;
        }
        if (Kind == NewResourceKind.FeatureSource && string.IsNullOrWhiteSpace(FdoProvider))
        {
            ValidationError = "FDO provider is required for a Feature Source.";
            return false;
        }
        return true;
    }
}
