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

    protected override Task SaveAsync()
    {
        // TODO: implement XML save
        return Task.CompletedTask;
    }
}
