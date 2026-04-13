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

namespace Maestro.Next.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IConnectionService _connectionService;

    public SiteExplorerViewModel SiteExplorer { get; }
    public DocumentManagerViewModel Documents { get; }

    public MainWindowViewModel(
        IConnectionService connectionService,
        SiteExplorerViewModel siteExplorer,
        DocumentManagerViewModel documents)
    {
        _connectionService = connectionService;
        SiteExplorer = siteExplorer;
        Documents = documents;

        _connectionService.ConnectionStateChanged += OnConnectionStateChanged;
    }

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private string _title = "MapGuide Maestro (Next)";

    [ObservableProperty]
    private string _statusMessage = "Not connected";

    /// <summary>
    /// Raised when the UI should show the Login dialog
    /// </summary>
    public event Func<Task<bool>>? LoginRequested;

    [RelayCommand]
    private async Task ConnectAsync()
    {
        if (LoginRequested != null)
        {
            var success = await LoginRequested.Invoke();
            if (success)
            {
                await SiteExplorer.LoadRootCommand.ExecuteAsync(null);
            }
        }
    }

    [RelayCommand]
    private void Disconnect()
    {
        _connectionService.Disconnect();
        SiteExplorer.RootNodes.Clear();
        Documents.CloseAllDocumentsCommand.Execute(null);
    }

    [RelayCommand]
    private static void Exit()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime
            is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    private void OnConnectionStateChanged(object? sender, bool connected)
    {
        IsConnected = connected;
        if (connected)
        {
            var conn = _connectionService.CurrentConnection!;
            StatusMessage = $"Connected to MapGuide {conn.SiteVersion}";
            Title = $"MapGuide Maestro (Next) — {conn.DisplayName}";
        }
        else
        {
            StatusMessage = "Not connected";
            Title = "MapGuide Maestro (Next)";
        }
    }
}
