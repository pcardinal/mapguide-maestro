// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.

using OSGeo.MapGuide.MaestroAPI;
using OSGeo.MapGuide.ObjectModels.Common;

namespace Maestro.Next.Services;

public class ResourceService : IResourceService
{
    private readonly IConnectionService _connectionService;

    public ResourceService(IConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    public Task<IReadOnlyList<ResourceListItem>> GetResourceListAsync(string folderId)
    {
        return Task.Run<IReadOnlyList<ResourceListItem>>(() =>
        {
            var conn = _connectionService.CurrentConnection
                ?? throw new InvalidOperationException("Not connected to a MapGuide server.");

            var resSvc = conn.ResourceService;
            var list = resSvc.GetRepositoryResources(folderId, 1);

            var items = new List<ResourceListItem>();

            foreach (var folder in list.Children.OfType<IRepositoryItem>())
            {
                bool isFolder = folder.ResourceId.EndsWith("/"); //NOXLATE
                var name = isFolder
                    ? folder.ResourceId.TrimEnd('/').Split('/').Last()
                    : folder.Name ?? folder.ResourceId.Split('/').Last();

                items.Add(new ResourceListItem(
                    folder.ResourceId,
                    name,
                    folder.ResourceType,
                    isFolder));
            }

            return items;
        });
    }
}
