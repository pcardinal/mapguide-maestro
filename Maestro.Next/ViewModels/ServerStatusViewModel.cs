// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maestro.Next.Services;
using Microsoft.Extensions.DependencyInjection;
using OSGeo.MapGuide.MaestroAPI;
using OSGeo.MapGuide.MaestroAPI.Services;

namespace Maestro.Next.ViewModels;

/// <summary>
/// ViewModel for server status/info display
/// </summary>
public partial class ServerStatusViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isLoaded;
    [ObservableProperty] private string _serverVersion = string.Empty;
    [ObservableProperty] private string _siteStatus = string.Empty;
    [ObservableProperty] private string _sessionId = string.Empty;
    [ObservableProperty] private string _providerName = string.Empty;

    public ObservableCollection<string> Users { get; } = new();
    public ObservableCollection<string> Groups { get; } = new();

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            var conn = Program.Services!.GetRequiredService<IConnectionService>()
                              .CurrentConnection!;

            SessionId = conn.SessionID ?? "(none)";
            ProviderName = conn.ProviderName ?? "(unknown)";

            // Server version via SiteVersion property
            ServerVersion = conn.SiteVersion?.ToString() ?? "(unknown)";

            // Try to load site info
            try
            {
                var siteSvc = (ISiteService)conn.GetService((int)ServiceType.Site);

                var siteInfo = await Task.Run(() => siteSvc.GetSiteInfo());
                SiteStatus = siteInfo?.Statistics?.AdminOperationsQueueCount != null
                    ? $"Admin Ops: {siteInfo.Statistics.AdminOperationsQueueCount}, " +
                      $"Client Ops: {siteInfo.Statistics.ClientOperationsQueueCount}, " +
                      $"Site Ops: {siteInfo.Statistics.SiteOperationsQueueCount}"
                    : "Site info loaded";

                // Users
                var userList = await Task.Run(() => siteSvc.EnumerateUsers());
                if (userList?.Items != null)
                    foreach (var item in userList.Items)
                    {
                        if (item is OSGeo.MapGuide.ObjectModels.Common.UserListUser u)
                            Users.Add($"{u.Name} ({u.FullName})");
                    }

                // Groups
                var groupList = await Task.Run(() => siteSvc.EnumerateGroups());
                if (groupList?.Group != null)
                    foreach (var g in groupList.Group)
                        Groups.Add(g.Name);
            }
            catch
            {
                SiteStatus = "Site service not available for this connection type.";
            }

            IsLoaded = true;
        }
        catch (Exception ex)
        {
            SiteStatus = $"Error: {ex.Message}";
        }
    }
}
