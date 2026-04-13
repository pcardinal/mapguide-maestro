// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.

using OSGeo.MapGuide.MaestroAPI;

namespace Maestro.Next.Services;

public class ConnectionService : IConnectionService
{
    private IServerConnection? _connection;

    public IServerConnection? CurrentConnection => _connection;

    public bool IsConnected => _connection != null;

    public event EventHandler<bool>? ConnectionStateChanged;

    public Task<IServerConnection> ConnectAsync(string url, string username, string password)
    {
        return Task.Run(() =>
        {
            var conn = ConnectionProviderRegistry.CreateConnection(
                "Maestro.Http", //NOXLATE
                "Url", url, //NOXLATE
                "Username", username, //NOXLATE
                "Password", password); //NOXLATE

            _connection = conn;
            ConnectionStateChanged?.Invoke(this, true);
            return conn;
        });
    }

    public void Disconnect()
    {
        _connection = null;
        ConnectionStateChanged?.Invoke(this, false);
    }
}
