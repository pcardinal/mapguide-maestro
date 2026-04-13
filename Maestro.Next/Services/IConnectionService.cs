// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.

using OSGeo.MapGuide.MaestroAPI;

namespace Maestro.Next.Services;

/// <summary>
/// Manages connections to MapGuide servers
/// </summary>
public interface IConnectionService
{
    /// <summary>
    /// Gets the current active connection, or null if not connected
    /// </summary>
    IServerConnection? CurrentConnection { get; }

    /// <summary>
    /// Indicates whether a connection is currently active
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Connects to a MapGuide server via HTTP
    /// </summary>
    Task<IServerConnection> ConnectAsync(string url, string username, string password);

    /// <summary>
    /// Disconnects the current connection
    /// </summary>
    void Disconnect();

    /// <summary>
    /// Raised when connection state changes
    /// </summary>
    event EventHandler<bool>? ConnectionStateChanged;
}

/// <summary>
/// Provides access to MapGuide resources
/// </summary>
public interface IResourceService
{
    /// <summary>
    /// Lists child resources under the specified folder
    /// </summary>
    Task<IReadOnlyList<ResourceListItem>> GetResourceListAsync(string folderId);
}

/// <summary>
/// Represents an item in the resource tree
/// </summary>
public record ResourceListItem(
    string ResourceId,
    string Name,
    string ResourceType,
    bool IsFolder);
