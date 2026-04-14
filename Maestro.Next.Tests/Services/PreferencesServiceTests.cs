// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Maestro.Next.Services;
using Xunit;

namespace Maestro.Next.Tests.Services;

public sealed class PreferencesServiceTests
{
    [Fact]
    public void Defaults_AreReasonable()
    {
        var svc = new PreferencesService();

        Assert.Equal(AppTheme.Dark, svc.Theme);
        Assert.Contains("mapagent", svc.LastServerUrl);
        Assert.Equal("Administrator", svc.LastUsername);
        Assert.Empty(svc.RecentConnections);
    }

    [Fact]
    public void AddRecentConnection_AddsToFront()
    {
        var svc = new PreferencesService();
        svc.AddRecentConnection("http://server1/mapagent");
        svc.AddRecentConnection("http://server2/mapagent");

        Assert.Equal(2, svc.RecentConnections.Count);
        Assert.Equal("http://server2/mapagent", svc.RecentConnections[0]);
    }

    [Fact]
    public void AddRecentConnection_DeduplicatesAndMovesToFront()
    {
        var svc = new PreferencesService();
        svc.AddRecentConnection("http://server1/mapagent");
        svc.AddRecentConnection("http://server2/mapagent");
        svc.AddRecentConnection("http://server1/mapagent");

        Assert.Equal(2, svc.RecentConnections.Count);
        Assert.Equal("http://server1/mapagent", svc.RecentConnections[0]);
    }

    [Fact]
    public void AddRecentConnection_MaximumTen()
    {
        var svc = new PreferencesService();
        for (int i = 0; i < 15; i++)
            svc.AddRecentConnection($"http://server{i}/mapagent");

        Assert.Equal(10, svc.RecentConnections.Count);
        Assert.Equal("http://server14/mapagent", svc.RecentConnections[0]);
    }

    [Fact]
    public void Theme_CanBeChanged()
    {
        var svc = new PreferencesService();
        svc.Theme = AppTheme.Light;
        Assert.Equal(AppTheme.Light, svc.Theme);
    }
}
