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

namespace Maestro.Next.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IConnectionService _connectionService;

    public SiteExplorerViewModel SiteExplorer { get; }
    public DocumentManagerViewModel Documents { get; }
    public ServerInfoViewModel ServerInfo { get; }

    public MainWindowViewModel(
        IConnectionService connectionService,
        SiteExplorerViewModel siteExplorer,
        DocumentManagerViewModel documents)
    {
        _connectionService = connectionService;
        SiteExplorer = siteExplorer;
        Documents = documents;
        ServerInfo = new ServerInfoViewModel();

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

    /// <summary>Raised when the About dialog should be shown</summary>
    public event Func<Task>? AboutRequested;

    /// <summary>Raised when the Options dialog should be shown</summary>
    public event Func<Task>? OptionsRequested;

    [RelayCommand]
    private async Task ShowAboutAsync()
    {
        if (AboutRequested != null)
            await AboutRequested.Invoke();
    }

    [RelayCommand]
    private async Task ShowOptionsAsync()
    {
        if (OptionsRequested != null)
            await OptionsRequested.Invoke();
    }

    /// <summary>Save the active document (Ctrl+S)</summary>
    [RelayCommand]
    private async Task SaveActiveDocumentAsync()
    {
        if (Documents.ActiveDocument is { IsDirty: true } doc)
            await doc.SaveCommand.ExecuteAsync(null);
    }

    /// <summary>Save all dirty documents (Ctrl+Shift+S)</summary>
    [RelayCommand]
    private async Task SaveAllDocumentsAsync()
    {
        var dirtyDocs = Documents.OpenDocuments.Where(d => d.IsDirty).ToList();
        foreach (var doc in dirtyDocs)
            await doc.SaveCommand.ExecuteAsync(null);

        if (dirtyDocs.Count > 0)
            Program.Services!.GetRequiredService<INotificationService>()
                   .Success($"Saved {dirtyDocs.Count} document(s).");
    }

    /// <summary>Close the active tab (Ctrl+W)</summary>
    [RelayCommand]
    private void CloseActiveDocument()
    {
        if (Documents.ActiveDocument is { } doc)
            Documents.CloseDocumentCommand.Execute(doc);
    }

    private void OnConnectionStateChanged(object? sender, bool connected)
    {
        IsConnected = connected;
        if (connected)
        {
            var conn = _connectionService.CurrentConnection!;
            StatusMessage = $"Connected to MapGuide {conn.SiteVersion}";
            Title = $"MapGuide Maestro (Next) — {conn.DisplayName}";
            _ = ServerInfo.RefreshCommand.ExecuteAsync(null);
        }
        else
        {
            StatusMessage = "Not connected";
            Title = "MapGuide Maestro (Next)";
            _ = ServerInfo.RefreshCommand.ExecuteAsync(null);
        }
    }
}
