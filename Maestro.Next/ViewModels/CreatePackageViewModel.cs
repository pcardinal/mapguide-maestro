// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using System.Collections.ObjectModel;
using System.IO.Compression;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maestro.Next.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Maestro.Next.ViewModels;

/// <summary>
/// ViewModel for creating a .mgp package from selected resources.
/// Uses a simplified cross-platform approach (ZIP with resource XML).
/// </summary>
public partial class CreatePackageViewModel : ViewModelBase
{
    [ObservableProperty] private string _folderResourceId = "Library://";
    [ObservableProperty] private string _outputFilePath = string.Empty;
    [ObservableProperty] private bool _isCreating;
    [ObservableProperty] private double _progress;
    [ObservableProperty] private string _statusMessage = "Select a folder and output path.";

    /// <summary>Available resource items to package (flat list from folder)</summary>
    public ObservableCollection<PackageResourceItem> Resources { get; } = new();

    public bool Completed { get; private set; }

    [RelayCommand]
    private async Task LoadResourcesAsync()
    {
        Resources.Clear();
        try
        {
            var conn = Program.Services!.GetRequiredService<IConnectionService>()
                              .CurrentConnection!;
            var list = await Task.Run(() =>
                conn.ResourceService.GetRepositoryResources(FolderResourceId, -1));

            if (list.Items != null)
            {
                foreach (var item in list.Items)
                {
                    if (item is OSGeo.MapGuide.ObjectModels.Common.ResourceListResourceDocument doc)
                    {
                        Resources.Add(new PackageResourceItem(doc.ResourceId, true));
                    }
                }
            }
            StatusMessage = $"{Resources.Count} resource(s) found in {FolderResourceId}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CreatePackageAsync()
    {
        if (string.IsNullOrWhiteSpace(OutputFilePath))
        {
            StatusMessage = "Please select an output file path.";
            return;
        }

        var selectedIds = Resources.Where(r => r.IsSelected)
                                   .Select(r => r.ResourceId)
                                   .ToList();
        if (selectedIds.Count == 0)
        {
            StatusMessage = "No resources selected.";
            return;
        }

        try
        {
            IsCreating = true;
            Progress = 0;
            StatusMessage = $"Creating package with {selectedIds.Count} resource(s)...";

            var conn = Program.Services!.GetRequiredService<IConnectionService>()
                              .CurrentConnection!;

            await Task.Run(() =>
            {
                using var zip = ZipFile.Open(OutputFilePath, ZipArchiveMode.Create);
                int done = 0;
                foreach (var resId in selectedIds)
                {
                    try
                    {
                        // Get XML content
                        using var xmlStream = conn.ResourceService.GetResourceXmlData(resId);
                        var entryName = resId.Replace("Library://", "")
                                             .Replace("//", "/")
                                             .Replace("/", System.IO.Path.DirectorySeparatorChar.ToString())
                                             + ".xml";
                        var entry = zip.CreateEntry(entryName, CompressionLevel.Optimal);
                        using var entryStream = entry.Open();
                        xmlStream.CopyTo(entryStream);
                    }
                    catch
                    {
                        // Skip resources that can't be exported
                    }
                    done++;
                    Progress = (double)done / selectedIds.Count * 100;
                }
            });

            Progress = 100;
            StatusMessage = "Package created successfully!";
            Completed = true;
            Program.Services!.GetRequiredService<INotificationService>()
                   .Success("Package created.");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed: {ex.Message}";
        }
        finally
        {
            IsCreating = false;
        }
    }
}

public partial class PackageResourceItem : ViewModelBase
{
    public PackageResourceItem(string resourceId, bool isSelected)
    {
        ResourceId = resourceId;
        IsSelected = isSelected;
    }

    public string ResourceId { get; }
    [ObservableProperty] private bool _isSelected;
}
