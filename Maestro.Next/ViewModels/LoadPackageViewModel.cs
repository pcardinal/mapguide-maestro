// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maestro.Next.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Maestro.Next.ViewModels;

/// <summary>
/// ViewModel for loading a .mgp package file to the MapGuide server
/// </summary>
public partial class LoadPackageViewModel : ViewModelBase
{
    [ObservableProperty] private string _packageFilePath = string.Empty;
    [ObservableProperty] private bool _isUploading;
    [ObservableProperty] private double _progress;
    [ObservableProperty] private string _statusMessage = "Select a .mgp package file to upload.";

    public bool Completed { get; private set; }

    [RelayCommand]
    private async Task UploadAsync()
    {
        if (string.IsNullOrWhiteSpace(PackageFilePath)) return;
        if (!System.IO.File.Exists(PackageFilePath))
        {
            StatusMessage = "File not found.";
            return;
        }

        try
        {
            IsUploading = true;
            Progress = 0;
            StatusMessage = "Uploading...";

            var conn = Program.Services!.GetRequiredService<IConnectionService>()
                              .CurrentConnection!;

            await Task.Run(() =>
            {
                conn.ResourceService.UploadPackage(PackageFilePath,
                    (copied, total, _) =>
                    {
                        if (total > 0)
                            Progress = (double)copied / total * 100;
                    });
            });

            Progress = 100;
            StatusMessage = "Package uploaded successfully!";
            Completed = true;
            Program.Services!.GetRequiredService<INotificationService>()
                   .Success("Package uploaded.");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Upload failed: {ex.Message}";
            Program.Services!.GetRequiredService<INotificationService>()
                   .Error($"Upload failed: {ex.Message}");
        }
        finally
        {
            IsUploading = false;
        }
    }
}
