// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maestro.Next.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Maestro.Next.ViewModels;

/// <summary>
/// Opens any resource as raw XML for editing.
/// Used for "Edit as XML" and as fallback for unknown resource types.
/// Supports viewing the server version for diff comparison.
/// </summary>
public partial class XmlEditorViewModel : DocumentViewModel
{
    public override string IconKey => "Xml";

    public XmlEditorViewModel(string resourceId)
    {
        ResourceId = resourceId;
        var name = resourceId.TrimEnd('/').Split('/').Last();
        Title = $"{System.IO.Path.GetFileNameWithoutExtension(name)} [XML]";
    }

    [ObservableProperty] private string _xmlContent = string.Empty;
    [ObservableProperty] private string _serverXml = string.Empty;
    [ObservableProperty] private bool _isLoaded;
    [ObservableProperty] private bool _showDiff;

    partial void OnXmlContentChanged(string value)
    {
        if (IsLoaded) IsDirty = true;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            BusyMessage = "Loading XML...";

            var conn = Program.Services!.GetRequiredService<IConnectionService>()
                              .CurrentConnection!;

            using var stream = await Task.Run(() =>
                conn.ResourceService.GetResourceXmlData(ResourceId!));
            using var reader = new System.IO.StreamReader(stream);
            var xml = await reader.ReadToEndAsync();

            XmlContent = xml;
            ServerXml = xml;
            IsLoaded = true;
            IsDirty = false;
        }
        catch (Exception ex)
        {
            Program.Services!.GetRequiredService<INotificationService>()
                   .Error($"Failed to load XML: {ex.Message}");
        }
        finally { IsBusy = false; BusyMessage = null; }
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

            using var ms = new System.IO.MemoryStream(
                System.Text.Encoding.UTF8.GetBytes(XmlContent));

            await Task.Run(() =>
                conn.ResourceService.SetResourceXmlData(ResourceId, ms));

            ServerXml = XmlContent;
            IsDirty = false;
            Program.Services!.GetRequiredService<INotificationService>()
                   .Success("XML saved.");
        }
        catch (Exception ex)
        {
            Program.Services!.GetRequiredService<INotificationService>()
                   .Error($"Save failed: {ex.Message}");
        }
        finally { IsBusy = false; BusyMessage = null; }
    }

    [RelayCommand]
    private void ToggleDiff() => ShowDiff = !ShowDiff;

    /// <summary>True if the XML has changed from the server version</summary>
    public bool HasChanges => !string.Equals(XmlContent, ServerXml, StringComparison.Ordinal);
}
