// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maestro.Next.Services;
using Microsoft.Extensions.DependencyInjection;
using OSGeo.MapGuide.ObjectModels.ApplicationDefinition;

namespace Maestro.Next.ViewModels;

public partial class ApplicationDefinitionEditorViewModel : DocumentViewModel
{
    public override string IconKey => "ApplicationDefinition";

    private IApplicationDefinition? _appDef;

    public ApplicationDefinitionEditorViewModel(string resourceId)
    {
        ResourceId = resourceId;
        var name = resourceId.TrimEnd('/').Split('/').Last();
        Title = $"{System.IO.Path.GetFileNameWithoutExtension(name)} [Fusion]";
    }

    [ObservableProperty] private bool _isLoaded;
    [ObservableProperty] private string _layoutTitle = string.Empty;
    [ObservableProperty] private string _templateUrl  = string.Empty;

    public ObservableCollection<FusionMapGroupViewModel> MapGroups  { get; } = new();
    public ObservableCollection<FusionWidgetSetViewModel> WidgetSets { get; } = new();

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            BusyMessage = "Loading Fusion layout...";

            var conn = Program.Services!.GetRequiredService<IConnectionService>()
                              .CurrentConnection!;

            _appDef = (IApplicationDefinition)await Task.Run(
                () => conn.ResourceService.GetResource(ResourceId!));

            LayoutTitle = _appDef.Title       ?? string.Empty;
            TemplateUrl = _appDef.TemplateUrl ?? string.Empty;

            MapGroups.Clear();
            if (_appDef.MapSet != null)
            {
                foreach (var mg in _appDef.MapSet.MapGroups)
                {
                    var vm = new FusionMapGroupViewModel(mg.id);
                    foreach (var map in mg.Map)
                        vm.Maps.Add(new FusionMapViewModel(
                            map.Type ?? "MapGuide",
                            map.GetMapDefinition() ?? string.Empty,
                            map.SingleTile));
                    MapGroups.Add(vm);
                }
            }

            WidgetSets.Clear();
            foreach (var ws in _appDef.WidgetSets)
            {
                var vm = new FusionWidgetSetViewModel();
                foreach (var w in ws.Widgets)
                    vm.Widgets.Add(new FusionWidgetViewModel(
                        w.Name ?? "(unnamed)",
                        w.Type ?? "(unknown)"));
                WidgetSets.Add(vm);
            }

            IsLoaded = true;
        }
        catch (Exception ex)
        {
            Program.Services!.GetRequiredService<INotificationService>()
                   .Error($"Failed to load Fusion layout: {ex.Message}");
        }
        finally { IsBusy = false; BusyMessage = null; }
    }

    protected override async Task SaveAsync()
    {
        if (_appDef is null) return;
        try
        {
            IsBusy = true;
            BusyMessage = "Saving...";

            _appDef.Title       = LayoutTitle;
            _appDef.TemplateUrl = TemplateUrl;

            var conn = Program.Services!.GetRequiredService<IConnectionService>()
                              .CurrentConnection!;
            await Task.Run(() => conn.ResourceService.SaveResource(_appDef));

            IsDirty = false;
            Program.Services!.GetRequiredService<INotificationService>()
                   .Success("Fusion layout saved.");
        }
        catch (Exception ex)
        {
            Program.Services!.GetRequiredService<INotificationService>()
                   .Error($"Save failed: {ex.Message}");
        }
        finally { IsBusy = false; BusyMessage = null; }
    }
}

public partial class FusionMapGroupViewModel : ViewModelBase
{
    public FusionMapGroupViewModel(string id) => Id = id;
    public string Id { get; }
    public ObservableCollection<FusionMapViewModel> Maps { get; } = new();
}

public record FusionMapViewModel(string Type, string MapDefinitionId, bool SingleTile);

public partial class FusionWidgetSetViewModel : ViewModelBase
{
    public ObservableCollection<FusionWidgetViewModel> Widgets { get; } = new();
}

public record FusionWidgetViewModel(string Name, string Type);
