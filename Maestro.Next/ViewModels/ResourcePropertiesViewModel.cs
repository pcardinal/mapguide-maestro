// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maestro.Next.Services;
using Microsoft.Extensions.DependencyInjection;
using OSGeo.MapGuide.ObjectModels;

namespace Maestro.Next.ViewModels;

/// <summary>
/// Displays read-only properties about a repository resource
/// </summary>
public partial class ResourcePropertiesViewModel : ViewModelBase
{
    public ResourcePropertiesViewModel(string resourceId)
    {
        ResourceId = resourceId;
        Name = ResourceIdentifier.GetName(resourceId);
        ResourceType = ResourceIdentifier.GetResourceTypeAsString(resourceId);
        IsFolder = ResourceIdentifier.IsFolderResource(resourceId);
    }

    public string ResourceId { get; }
    public string Name { get; }
    public string ResourceType { get; }
    public bool IsFolder { get; }

    [ObservableProperty] private string _createdDate = "—";
    [ObservableProperty] private string _modifiedDate = "—";
    [ObservableProperty] private string _owner = "—";
    [ObservableProperty] private int _referenceCount;
    [ObservableProperty] private string _xmlPreview = string.Empty;
    [ObservableProperty] private bool _isLoaded;

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            var conn = Program.Services!.GetRequiredService<IConnectionService>()
                              .CurrentConnection!;

            // Header info
            try
            {
                if (!IsFolder)
                {
                    var header = await Task.Run(() =>
                        conn.ResourceService.GetResourceHeader(ResourceId));
                    if (header?.General != null)
                    {
                        Owner = header.General.IconName ?? "—";
                    }
                }
            }
            catch { /* Header may not be available */ }

            // References
            try
            {
                var refs = await Task.Run(() =>
                    conn.ResourceService.EnumerateResourceReferences(ResourceId));
                ReferenceCount = refs?.ResourceId?.Count ?? 0;
            }
            catch { ReferenceCount = 0; }

            // XML preview
            if (!IsFolder)
            {
                try
                {
                    using var stream = await Task.Run(() =>
                        conn.ResourceService.GetResourceXmlData(ResourceId));
                    using var reader = new System.IO.StreamReader(stream);
                    var xml = await reader.ReadToEndAsync();
                    XmlPreview = xml.Length > 4000 ? xml[..4000] + "\n... (truncated)" : xml;
                }
                catch { XmlPreview = "(unable to load XML)"; }
            }

            IsLoaded = true;
        }
        catch (Exception ex)
        {
            Program.Services!.GetRequiredService<INotificationService>()
                   .Error($"Failed to load properties: {ex.Message}");
        }
    }
}
