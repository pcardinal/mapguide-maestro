// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maestro.Next.Services;
using Microsoft.Extensions.DependencyInjection;
using OSGeo.MapGuide.ObjectModels.SymbolDefinition;

namespace Maestro.Next.ViewModels;

public partial class SymbolDefinitionEditorViewModel : DocumentViewModel
{
    public override string IconKey => "SymbolDefinition";

    private ISymbolDefinitionBase? _symDef;

    public SymbolDefinitionEditorViewModel(string resourceId)
    {
        ResourceId = resourceId;
        var name = resourceId.TrimEnd('/').Split('/').Last();
        Title = $"{System.IO.Path.GetFileNameWithoutExtension(name)} [Symbol]";
    }

    [ObservableProperty] private bool _isLoaded;
    [ObservableProperty] private string _symbolName = string.Empty;
    [ObservableProperty] private string _symbolDescription = string.Empty;
    [ObservableProperty] private string _symbolType = string.Empty;

    // Simple symbol specifics
    [ObservableProperty] private bool _isSimple;
    [ObservableProperty] private bool _hasPointUsage;
    [ObservableProperty] private bool _hasLineUsage;
    [ObservableProperty] private bool _hasAreaUsage;

    public ObservableCollection<GraphicElementViewModel> Graphics { get; } = new();
    public ObservableCollection<SymbolParameterViewModel> Parameters { get; } = new();

    // Compound symbol specifics
    [ObservableProperty] private bool _isCompound;
    public ObservableCollection<CompoundSymbolPartViewModel> Parts { get; } = new();

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            BusyMessage = "Loading symbol definition...";

            var conn = Program.Services!.GetRequiredService<IConnectionService>()
                              .CurrentConnection!;

            _symDef = (ISymbolDefinitionBase)await Task.Run(
                () => conn.ResourceService.GetResource(ResourceId!));

            SymbolName        = _symDef.Name ?? string.Empty;
            SymbolDescription = _symDef.Description ?? string.Empty;
            SymbolType        = _symDef.Type.ToString();

            IsSimple   = _symDef is ISimpleSymbolDefinition;
            IsCompound = _symDef is ICompoundSymbolDefinition;

            if (_symDef is ISimpleSymbolDefinition simple)
            {
                HasPointUsage = simple.PointUsage != null;
                HasLineUsage  = simple.LineUsage  != null;
                HasAreaUsage  = simple.AreaUsage  != null;

                Graphics.Clear();
                foreach (var g in simple.Graphics)
                {
                    Graphics.Add(new GraphicElementViewModel(
                        g.GetType().Name.Replace("Type", ""),
                        g is ITextGraphic tg ? tg.Content ?? "" : ""));
                }

                Parameters.Clear();
                if (simple.ParameterDefinition != null)
                {
                    foreach (var p in simple.ParameterDefinition.Parameter)
                    {
                        Parameters.Add(new SymbolParameterViewModel(
                            p.Identifier ?? "",
                            p.DefaultValue ?? "",
                            p.Description ?? "",
                            p.DataType ?? ""));
                    }
                }
            }
            else if (_symDef is ICompoundSymbolDefinition compound)
            {
                Parts.Clear();
                foreach (var sr in compound.SimpleSymbol)
                {
                    var refType = sr.Type;
                    string label;
                    if (refType == SimpleSymbolReferenceType.Inline &&
                        sr is ISimpleSymbolInlineReference inl)
                        label = inl.SimpleSymbolDefinition?.Name ?? "(inline)";
                    else if (refType == SimpleSymbolReferenceType.Library &&
                             sr is ISimpleSymbolLibraryReference lib)
                        label = lib.ResourceId ?? "(library ref)";
                    else
                        label = "(unknown)";

                    Parts.Add(new CompoundSymbolPartViewModel(
                        refType.ToString(), label, sr.RenderingPass ?? "0"));
                }
            }

            IsLoaded = true;
        }
        catch (Exception ex)
        {
            Program.Services!.GetRequiredService<INotificationService>()
                   .Error($"Failed to load symbol: {ex.Message}");
        }
        finally { IsBusy = false; BusyMessage = null; }
    }

    protected override async Task SaveAsync()
    {
        if (_symDef is null) return;
        try
        {
            IsBusy = true;
            BusyMessage = "Saving...";

            _symDef.Name        = SymbolName;
            _symDef.Description = SymbolDescription;

            var conn = Program.Services!.GetRequiredService<IConnectionService>()
                              .CurrentConnection!;
            await Task.Run(() => conn.ResourceService.SaveResource(_symDef));

            IsDirty = false;
            Program.Services!.GetRequiredService<INotificationService>()
                   .Success("Symbol definition saved.");
        }
        catch (Exception ex)
        {
            Program.Services!.GetRequiredService<INotificationService>()
                   .Error($"Save failed: {ex.Message}");
        }
        finally { IsBusy = false; BusyMessage = null; }
    }
}

public record GraphicElementViewModel(string Kind, string Preview);
public record SymbolParameterViewModel(string Identifier, string DefaultValue, string Description, string DataType);
public record CompoundSymbolPartViewModel(string ReferenceType, string Label, string RenderingPass);
