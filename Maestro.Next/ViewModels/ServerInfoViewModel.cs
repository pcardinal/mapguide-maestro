// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maestro.Next.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Maestro.Next.ViewModels;

/// <summary>
/// Displays server connection info in a fly-out panel
/// </summary>
public partial class ServerInfoViewModel : ViewModelBase
{
    [ObservableProperty] private string _serverVersion = "—";
    [ObservableProperty] private string _siteUrl = "—";
    [ObservableProperty] private string _displayName = "—";
    [ObservableProperty] private bool _isConnected;
    [ObservableProperty] private string _providerList = string.Empty;

    [RelayCommand]
    public async Task RefreshAsync()
    {
        var connSvc = Program.Services?.GetService<IConnectionService>();
        var conn = connSvc?.CurrentConnection;

        if (conn is null)
        {
            IsConnected = false;
            ServerVersion = "—";
            SiteUrl = "—";
            DisplayName = "—";
            ProviderList = string.Empty;
            return;
        }

        IsConnected   = true;
        ServerVersion = conn.SiteVersion?.ToString() ?? "Unknown";
        DisplayName   = conn.DisplayName ?? "Unknown";

        try
        {
            var providers = await Task.Run(() => conn.FeatureService.FeatureProviders);
            ProviderList = string.Join("\n",
                providers.Select(p => $"{p.DisplayName} ({p.Name})"));
        }
        catch
        {
            ProviderList = "(unable to enumerate providers)";
        }
    }
}
