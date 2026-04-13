// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using OSGeo.MapGuide.MaestroAPI;
using OSGeo.MapGuide.ObjectModels;
using OSGeo.MapGuide.ObjectModels.FeatureSource;
using OSGeo.MapGuide.ObjectModels.LayerDefinition;
using OSGeo.MapGuide.ObjectModels.MapDefinition;
using OSGeo.MapGuide.ObjectModels.WebLayout;

namespace Maestro.Next.Services;

/// <summary>
/// Creates and saves new MapGuide resources on the server
/// </summary>
public interface INewResourceService
{
    Task<string> CreateFolderAsync(string parentFolderId, string name);
    Task<string> CreateFeatureSourceAsync(string targetFolderId, string name, string provider);
    Task<string> CreateLayerDefinitionAsync(string targetFolderId, string name, string featureSourceId, string featureClass, string geometry);
    Task<string> CreateMapDefinitionAsync(string targetFolderId, string name);
    Task<string> CreateWebLayoutAsync(string targetFolderId, string name, string mapDefinitionId);
}

public class NewResourceService : INewResourceService
{
    private readonly IConnectionService _connectionService;

    public NewResourceService(IConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    private IServerConnection Conn => _connectionService.CurrentConnection
        ?? throw new InvalidOperationException("Not connected to a MapGuide server.");

    public async Task<string> CreateFolderAsync(string parentFolderId, string name)
    {
        var folderId = $"{parentFolderId.TrimEnd('/')}/{name}/";
        // MapGuide convention: SetResourceXmlData with null stream creates a folder
        await Task.Run(() => Conn.ResourceService.SetResourceXmlData(folderId, null));
        return folderId;
    }

    public async Task<string> CreateFeatureSourceAsync(string targetFolderId, string name, string provider)
    {
        var resourceId = $"{targetFolderId.TrimEnd('/')}/{name}.FeatureSource";
        var fs = ObjectFactory.CreateFeatureSource(provider);
        fs.ResourceID = resourceId;

        await Task.Run(() => Conn.ResourceService.SaveResource(fs));
        return resourceId;
    }

    public async Task<string> CreateLayerDefinitionAsync(
        string targetFolderId, string name,
        string featureSourceId, string featureClass, string geometry)
    {
        var resourceId = $"{targetFolderId.TrimEnd('/')}/{name}.LayerDefinition";

        // Create the latest supported vector layer definition
        var ldf = ObjectFactory.CreateDefaultLayer(LayerType.Vector, new Version(1, 3, 0));
        ldf.ResourceID = resourceId;

        var vld = (IVectorLayerDefinition)ldf.SubLayer;
        vld.ResourceId  = featureSourceId;
        vld.FeatureName = featureClass;
        vld.Geometry    = geometry;

        await Task.Run(() => Conn.ResourceService.SaveResource(ldf));
        return resourceId;
    }

    public async Task<string> CreateMapDefinitionAsync(string targetFolderId, string name)
    {
        var resourceId = $"{targetFolderId.TrimEnd('/')}/{name}.MapDefinition";

        var mdf = ObjectFactory.CreateMapDefinition(new Version(2, 3, 0), name);
        mdf.ResourceID = resourceId;

        await Task.Run(() => Conn.ResourceService.SaveResource(mdf));
        return resourceId;
    }

    public async Task<string> CreateWebLayoutAsync(string targetFolderId, string name, string mapDefinitionId)
    {
        var resourceId = $"{targetFolderId.TrimEnd('/')}/{name}.WebLayout";

        var wl = ObjectFactory.CreateWebLayout(new Version(1, 1, 0), mapDefinitionId);
        wl.ResourceID = resourceId;

        await Task.Run(() => Conn.ResourceService.SaveResource(wl));
        return resourceId;
    }
}
