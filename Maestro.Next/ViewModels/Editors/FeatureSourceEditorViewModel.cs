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
using OSGeo.MapGuide.ObjectModels.FeatureSource;

namespace Maestro.Next.ViewModels;

/// <summary>
/// Editor ViewModel for FeatureSource resources
/// </summary>
public partial class FeatureSourceEditorViewModel : DocumentViewModel
{
    public override string IconKey => "FeatureSource";

    private IFeatureSource? _featureSource;

    public FeatureSourceEditorViewModel(string resourceId)
    {
        ResourceId = resourceId;
        Title = System.IO.Path.GetFileNameWithoutExtension(
            resourceId.TrimEnd('/').Split('/').Last()) + " [FeatureSource]";
    }

    [ObservableProperty]
    private string _provider = string.Empty;

    [ObservableProperty]
    private string _connectionString = string.Empty;

    [ObservableProperty]
    private bool _isLoaded;

    [ObservableProperty]
    private string? _testConnectionResult;

    [ObservableProperty]
    private bool _testConnectionPassed;

    public ObservableCollection<ConnectionPropertyViewModel> ConnectionProperties { get; } = new();

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            BusyMessage = "Loading feature source...";

            var connSvc = Program.Services!.GetRequiredService<IConnectionService>();
            var conn = connSvc.CurrentConnection!;

            _featureSource = (IFeatureSource)await Task.Run(() =>
                conn.ResourceService.GetResource(ResourceId!));

            Provider = _featureSource.Provider;

            ConnectionProperties.Clear();
            foreach (var propName in _featureSource.ConnectionPropertyNames)
            {
                var value = _featureSource.GetConnectionProperty(propName);
                var prop = new ConnectionPropertyViewModel(propName, value);
                prop.PropertyChanged += (_, _) =>
                {
                    IsDirty = true;
                    UpdateConnectionStringPreview();
                };
                ConnectionProperties.Add(prop);
            }

            UpdateConnectionStringPreview();
            IsLoaded = true;
        }
        catch (Exception ex)
        {
            TestConnectionResult = $"Load failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            BusyMessage = null;
        }
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        if (_featureSource == null) return;

        try
        {
            IsBusy = true;
            BusyMessage = "Testing connection...";

            var connSvc = Program.Services!.GetRequiredService<IConnectionService>();
            var conn = connSvc.CurrentConnection!;

            // Flush any pending edits to the model before testing
            ApplyPropertyEdits();

            var result = await Task.Run(() =>
                conn.FeatureService.TestConnection(ResourceId!));

            TestConnectionPassed = result == "TRUE"; // MapGuide returns "TRUE" on success
            TestConnectionResult = TestConnectionPassed
                ? "✅ Connection successful"
                : $"❌ Connection failed: {result}";
        }
        catch (Exception ex)
        {
            TestConnectionPassed = false;
            TestConnectionResult = $"❌ {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            BusyMessage = null;
        }
    }

    protected override async Task SaveAsync()
    {
        if (_featureSource == null) return;

        try
        {
            IsBusy = true;
            BusyMessage = "Saving...";

            ApplyPropertyEdits();

            var connSvc = Program.Services!.GetRequiredService<IConnectionService>();
            var conn = connSvc.CurrentConnection!;

            await Task.Run(() => conn.ResourceService.SaveResource(_featureSource));

            IsDirty = false;
        }
        catch (Exception ex)
        {
            TestConnectionResult = $"Save failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            BusyMessage = null;
        }
    }

    private void ApplyPropertyEdits()
    {
        if (_featureSource == null) return;

        foreach (var prop in ConnectionProperties)
            _featureSource.SetConnectionProperty(prop.Name, prop.Value);
    }

    private void UpdateConnectionStringPreview()
    {
        ConnectionString = string.Join("; ",
            ConnectionProperties
                .Where(p => !string.IsNullOrEmpty(p.Value))
                .Select(p => $"{p.Name}={p.Value}"));
    }

    // ── Schema Preview ──────────────────────────────────────────
    public ObservableCollection<SchemaClassViewModel> SchemaClasses { get; } = new();

    [ObservableProperty] private bool _schemaLoaded;

    [RelayCommand]
    private async Task LoadSchemaAsync()
    {
        if (ResourceId is null) return;
        try
        {
            IsBusy = true;
            BusyMessage = "Loading schema...";
            SchemaClasses.Clear();

            var conn = Program.Services!.GetRequiredService<IConnectionService>()
                              .CurrentConnection!;

            var schemas = await Task.Run(() =>
                conn.FeatureService.GetSchemas(ResourceId));

            foreach (var schemaName in schemas)
            {
                var classNames = await Task.Run(() =>
                    conn.FeatureService.GetClassNames(ResourceId, schemaName));

                foreach (var className in classNames)
                {
                    try
                    {
                        var classDef = await Task.Run(() =>
                            conn.FeatureService.GetClassDefinition(ResourceId, className));

                        var props = new ObservableCollection<SchemaPropertyViewModel>();
                        foreach (var p in classDef.Properties)
                        {
                            var isKey = classDef.IdentityProperties
                                .Any(ip => ip.Name == p.Name);
                            props.Add(new SchemaPropertyViewModel(
                                p.Name,
                                p.Type.ToString(),
                                isKey));
                        }

                        SchemaClasses.Add(new SchemaClassViewModel(
                            classDef.QualifiedName, props));
                    }
                    catch
                    {
                        SchemaClasses.Add(new SchemaClassViewModel(
                            className, new ObservableCollection<SchemaPropertyViewModel>()));
                    }
                }
            }

            SchemaLoaded = true;
            if (SchemaClasses.Count > 0)
                SelectedClassName = SchemaClasses[0].QualifiedName;

            Program.Services!.GetRequiredService<INotificationService>()
                   .Success($"Schema loaded: {SchemaClasses.Count} class(es)");
        }
        catch (Exception ex)
        {
            Program.Services!.GetRequiredService<INotificationService>()
                   .Error($"Schema load failed: {ex.Message}");
        }
        finally { IsBusy = false; BusyMessage = null; }
    }

    // ── Data Preview ────────────────────────────────────────────
    [ObservableProperty] private string _dataPreview = string.Empty;
    [ObservableProperty] private string? _selectedClassName;

    [RelayCommand]
    private async Task PreviewDataAsync()
    {
        if (ResourceId is null || string.IsNullOrEmpty(SelectedClassName)) return;
        try
        {
            IsBusy = true;
            BusyMessage = "Loading data preview...";

            var conn = Program.Services!.GetRequiredService<IConnectionService>()
                              .CurrentConnection!;

            var reader = await Task.Run(() =>
                conn.FeatureService.QueryFeatureSource(ResourceId, SelectedClassName));

            var sb = new System.Text.StringBuilder();
            int count = 0;
            const int maxRows = 100;

            // Header
            var colNames = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
                colNames.Add(reader.GetName(i));
            sb.AppendLine(string.Join("\t", colNames));
            sb.AppendLine(new string('─', Math.Min(colNames.Count * 15, 120)));

            while (reader.ReadNext() && count < maxRows)
            {
                var vals = new List<string>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    try
                    {
                        if (reader.IsNull(i))
                            vals.Add("NULL");
                        else if (reader[i] is byte[])
                            vals.Add("[binary]");
                        else
                            vals.Add(reader[i]?.ToString()?.Truncate(50) ?? "");
                    }
                    catch
                    {
                        vals.Add("[error]");
                    }
                }
                sb.AppendLine(string.Join("\t", vals));
                count++;
            }
            reader.Close();

            sb.Insert(0, $"Showing {count} of {(count >= maxRows ? maxRows + "+" : count.ToString())} rows\n\n");
            DataPreview = sb.ToString();
        }
        catch (Exception ex)
        {
            DataPreview = $"Preview failed: {ex.Message}";
        }
        finally { IsBusy = false; BusyMessage = null; }
    }

    // ── Coordinate System Override ──────────────────────────────
    public ObservableCollection<SpatialContextOverrideItem> SpatialContextOverrides { get; } = new();
    [ObservableProperty] private bool _overridesLoaded;

    [RelayCommand]
    private async Task LoadSpatialContextOverridesAsync()
    {
        if (ResourceId is null) return;
        SpatialContextOverrides.Clear();
        try
        {
            IsBusy = true;
            BusyMessage = "Loading spatial contexts...";
            var conn = Program.Services!.GetRequiredService<IConnectionService>()
                              .CurrentConnection!;

            var contexts = await Task.Run(() =>
                conn.FeatureService.GetSpatialContextInfo(ResourceId, false));

            foreach (var ctx in contexts.SpatialContext)
            {
                var sc = (OSGeo.MapGuide.ObjectModels.Common.IFdoSpatialContext)ctx;
                SpatialContextOverrides.Add(new SpatialContextOverrideItem(
                    sc.Name, sc.CoordinateSystemWkt ?? "", sc.CoordinateSystemName ?? ""));
            }
            OverridesLoaded = true;
        }
        catch (Exception ex)
        {
            Program.Services!.GetRequiredService<INotificationService>()
                   .Error($"Failed to load spatial contexts: {ex.Message}");
        }
        finally { IsBusy = false; BusyMessage = null; }
    }

    [RelayCommand]
    private async Task ApplyOverridesAsync()
    {
        if (_featureSource == null || ResourceId == null) return;
        try
        {
            IsBusy = true;
            BusyMessage = "Applying overrides...";

            var existing = _featureSource.SupplementalSpatialContextInfo.ToList();
            foreach (var e in existing)
                _featureSource.RemoveSpatialContextOverride(e);

            foreach (var ovr in SpatialContextOverrides.Where(o => !string.IsNullOrEmpty(o.NewCoordSys)))
                _featureSource.AddSpatialContextOverride(ovr.Name, ovr.NewCoordSys);

            var conn = Program.Services!.GetRequiredService<IConnectionService>()
                              .CurrentConnection!;
            await Task.Run(() => conn.ResourceService.SaveResource(_featureSource));

            IsDirty = false;
            Program.Services!.GetRequiredService<INotificationService>()
                   .Success("Coordinate system overrides applied.");
        }
        catch (Exception ex)
        {
            Program.Services!.GetRequiredService<INotificationService>()
                   .Error($"Override failed: {ex.Message}");
        }
        finally { IsBusy = false; BusyMessage = null; }
    }

    // ── Extensions (joins + calculated properties) ──────────────
    public ObservableCollection<ExtensionSummaryItem> Extensions { get; } = new();

    [RelayCommand]
    private void LoadExtensions()
    {
        Extensions.Clear();
        if (_featureSource == null) return;

        foreach (var ext in _featureSource.Extension)
        {
            var calcProps = ext.CalculatedProperty.Select(cp => $"{cp.Name} = {cp.Expression}").ToList();
            var joins = ext.AttributeRelate.Select(ar => $"Join: {ar.Name} → {ar.RelateType}").ToList();
            Extensions.Add(new ExtensionSummaryItem(
                ext.Name, ext.FeatureClass, calcProps, joins));
        }
    }
}

public partial class SpatialContextOverrideItem : ViewModelBase
{
    public SpatialContextOverrideItem(string name, string currentWkt, string currentCoordSys)
    {
        Name = name;
        CurrentWkt = currentWkt.Truncate(80) ?? "";
        CurrentCoordSys = currentCoordSys;
        NewCoordSys = currentCoordSys;
    }

    public string Name { get; }
    public string CurrentWkt { get; }
    public string CurrentCoordSys { get; }
    [ObservableProperty] private string _newCoordSys;
}

public partial class SchemaClassViewModel : ViewModelBase
{
    public SchemaClassViewModel(string qualifiedName, ObservableCollection<SchemaPropertyViewModel> properties)
    {
        QualifiedName = qualifiedName;
        Properties = properties;
    }

    public string QualifiedName { get; }
    public ObservableCollection<SchemaPropertyViewModel> Properties { get; }

    [ObservableProperty] private bool _isExpanded;
}

public record SchemaPropertyViewModel(string Name, string Type, bool IsKey)
{
    public string Icon => IsKey ? "🔑" : Type == "Geometry" ? "📐" : "📋";
}

/// <summary>
/// Summary of a FeatureSource extension (calculated properties + joins)
/// </summary>
public record ExtensionSummaryItem(
    string Name,
    string FeatureClass,
    List<string> CalculatedProperties,
    List<string> Joins)
{
    public string Summary => $"{Name} ({FeatureClass}) — {CalculatedProperties.Count} calc, {Joins.Count} join(s)";
}

/// <summary>
/// Represents a single key/value connection property
/// </summary>
public partial class ConnectionPropertyViewModel : ViewModelBase
{
    public ConnectionPropertyViewModel(string name, string value)
    {
        Name = name;
        _value = value;
    }

    public string Name { get; }

    [ObservableProperty]
    private string _value;
}

/// <summary>
/// Generic "read-only XML" editor for resource types not yet fully implemented
/// </summary>
public partial class GenericResourceEditorViewModel : DocumentViewModel
{
    public override string IconKey => ResourceType;

    public string ResourceType { get; }

    [ObservableProperty]
    private string _xmlContent = string.Empty;

    [ObservableProperty]
    private bool _isLoaded;

    public GenericResourceEditorViewModel(string resourceId, string resourceType)
    {
        ResourceId = resourceId;
        ResourceType = resourceType;
        var name = System.IO.Path.GetFileNameWithoutExtension(
            resourceId.TrimEnd('/').Split('/').Last());
        Title = $"{name} [{resourceType}]";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            BusyMessage = "Loading...";

            var connSvc = Program.Services!.GetRequiredService<IConnectionService>();
            var conn = connSvc.CurrentConnection!;

            XmlContent = await Task.Run(() =>
            {
                using var stream = conn.ResourceService.GetResourceXmlData(ResourceId!);
                using var reader = new System.IO.StreamReader(stream);
                return reader.ReadToEnd();
            });

            IsLoaded = true;
        }
        catch (Exception ex)
        {
            XmlContent = $"<!-- Error loading resource: {ex.Message} -->";
        }
        finally
        {
            IsBusy = false;
            BusyMessage = null;
        }
    }

    protected override async Task SaveAsync()
    {
        if (ResourceId is null) return;
        try
        {
            IsBusy = true;
            BusyMessage = "Saving XML...";

            var conn = Program.Services!.GetRequiredService<IConnectionService>()
                              .CurrentConnection!;

            await Task.Run(() =>
            {
                using var ms = new System.IO.MemoryStream(
                    System.Text.Encoding.UTF8.GetBytes(XmlContent));
                conn.ResourceService.SetResourceXmlData(ResourceId, ms);
            });

            IsDirty = false;
            Program.Services!.GetRequiredService<INotificationService>()
                   .Success("Resource XML saved.");
        }
        catch (Exception ex)
        {
            Program.Services!.GetRequiredService<INotificationService>()
                   .Error($"Save failed: {ex.Message}");
        }
        finally { IsBusy = false; BusyMessage = null; }
    }
}
