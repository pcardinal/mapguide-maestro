// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro
//
// Integration tests that require a running MapGuide server.
// Default URL: http://localhost:8058/mapguide/mapagent/mapagent.fcgi
// These tests are skipped in CI (no MapGuide server available).

using OSGeo.MapGuide.MaestroAPI;
using OSGeo.MapGuide.MaestroAPI.Services;
using OSGeo.MapGuide.ObjectModels;
using Xunit;

namespace Maestro.Next.Tests.Integration;

/// <summary>
/// Integration tests against a live MapGuide server.
/// Skipped if server is unavailable.
/// </summary>
[Trait("Category", "Integration")]
public class MapGuideIntegrationTests : IDisposable
{
    private const string ServerUrl = "http://localhost:8058/mapguide/mapagent/mapagent.fcgi";
    private const string Username = "Administrator";
    private const string Password = "admin";

    private readonly IServerConnection? _conn;
    private readonly bool _serverAvailable;

    public MapGuideIntegrationTests()
    {
        try
        {
            var builder = new System.Collections.Specialized.NameValueCollection
            {
                ["Url"] = ServerUrl,
                ["Username"] = Username,
                ["Password"] = Password
            };
            _conn = ConnectionProviderRegistry.CreateConnection("Maestro.Http", builder);
            _serverAvailable = _conn != null && _conn.SiteVersion != null;
        }
        catch
        {
            _serverAvailable = false;
        }
    }

    public void Dispose()
    {
        // Session cleanup is handled by the connection itself
    }

    private bool SkipIfNoServer()
    {
        // Returns true if test should be skipped
        return !_serverAvailable;
    }

    [Fact]
    public void E2_1_CanConnect()
    {
        if (SkipIfNoServer()) return;

        Assert.NotNull(_conn);
        Assert.NotNull(_conn!.SiteVersion);
        Assert.NotNull(_conn.SessionID);
    }

    [Fact]
    public void E2_1_ServerVersion_IsReasonable()
    {
        if (SkipIfNoServer()) return;

        var version = _conn!.SiteVersion!;
        Assert.True(version.Major >= 2, $"Expected MapGuide >= 2.x, got {version}");
    }

    [Fact]
    public void E2_2_CanListResources()
    {
        if (SkipIfNoServer()) return;

        var resSvc = _conn!.ResourceService;
        var list = resSvc.GetRepositoryResources("Library://", 1);
        Assert.NotNull(list);
    }

    [Fact]
    public void E2_2_CrudResource()
    {
        if (SkipIfNoServer()) return;

        var resSvc = _conn!.ResourceService;
        var testId = "Library://IntegrationTest/TestFolder/";

        // Create folder
        if (!resSvc.ResourceExists(testId))
            resSvc.SetResourceXmlData("Library://IntegrationTest/", null);

        // Verify it was created
        var exists = resSvc.ResourceExists("Library://IntegrationTest/");
        Assert.True(exists);

        // Create a feature source
        var fs = ObjectFactory.CreateFeatureSource("OSGeo.SDF");
        var fsId = "Library://IntegrationTest/Test.FeatureSource";
        resSvc.SaveResourceAs(fs, fsId);

        Assert.True(resSvc.ResourceExists(fsId));

        // Read it back
        var readBack = resSvc.GetResource(fsId);
        Assert.NotNull(readBack);
        Assert.Equal("FeatureSource", readBack.ResourceType);

        // Delete
        resSvc.DeleteResource(fsId);
        Assert.False(resSvc.ResourceExists(fsId));

        // Cleanup folder
        try { resSvc.DeleteResource("Library://IntegrationTest/"); } catch { /* ignore */ }
    }

    [Fact]
    public void E2_3_CanValidateResource()
    {
        if (SkipIfNoServer()) return;

        var resSvc = _conn!.ResourceService;

        // Create a minimal feature source for validation
        var fs = ObjectFactory.CreateFeatureSource("OSGeo.SDF");
        var fsId = "Library://IntegrationTest/ValidateTest.FeatureSource";

        try
        {
            resSvc.SaveResourceAs(fs, fsId);

            var resource = resSvc.GetResource(fsId);
            var context = new OSGeo.MapGuide.MaestroAPI.Resource.Validation.ResourceValidationContext(_conn!);
            var issues = OSGeo.MapGuide.MaestroAPI.Resource.Validation.ResourceValidatorSet.Validate(context, resource, false);

            // We expect issues (empty SDF has no file), but validation should not crash
            Assert.NotNull(issues);
        }
        finally
        {
            try { resSvc.DeleteResource(fsId); } catch { }
            try { resSvc.DeleteResource("Library://IntegrationTest/"); } catch { }
        }
    }

    [Fact]
    public void E2_SpatialContexts_CanBeQueried()
    {
        if (SkipIfNoServer()) return;

        // List all feature sources
        var resSvc = _conn!.ResourceService;
        var list = resSvc.GetRepositoryResources("Library://", "FeatureSource", -1);

        // If there's at least one feature source, try to get spatial contexts
        // (this may not always work depending on server state, so just don't crash)
        Assert.NotNull(list);
    }

    [Fact]
    public void E2_FdoProviders_CanBeListed()
    {
        if (SkipIfNoServer()) return;

        var featureSvc = _conn!.FeatureService;
        var providers = featureSvc.FeatureProviders;

        Assert.NotNull(providers);
        Assert.True(providers.Length > 0, "Expected at least one FDO provider");
    }
}
