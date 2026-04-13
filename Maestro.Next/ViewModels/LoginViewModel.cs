// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maestro.Next.Services;

namespace Maestro.Next.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly IConnectionService _connectionService;

    public LoginViewModel(IConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private string _serverUrl = "http://localhost/mapguide/mapagent/mapagent.fcgi";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private string _username = "Administrator";

    [ObservableProperty]
    private string _password = "admin";

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isConnecting;

    /// <summary>
    /// Set by the dialog to indicate successful connection
    /// </summary>
    public bool ConnectionSucceeded { get; private set; }

    private bool CanConnect =>
        !string.IsNullOrWhiteSpace(ServerUrl) &&
        !string.IsNullOrWhiteSpace(Username) &&
        !IsConnecting;

    [RelayCommand(CanExecute = nameof(CanConnect))]
    private async Task ConnectAsync()
    {
        try
        {
            IsConnecting = true;
            ErrorMessage = null;

            await _connectionService.ConnectAsync(ServerUrl, Username, Password);
            ConnectionSucceeded = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Connection failed: {ex.Message}";
            ConnectionSucceeded = false;
        }
        finally
        {
            IsConnecting = false;
        }
    }
}
