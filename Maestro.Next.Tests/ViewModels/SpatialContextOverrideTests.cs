// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Maestro.Next.ViewModels;
using Xunit;

namespace Maestro.Next.Tests.ViewModels;

public sealed class SpatialContextOverrideItemTests
{
    [Fact]
    public void Ctor_SetsProperties()
    {
        var item = new SpatialContextOverrideItem("Default", "GEOGCS[\"WGS84\"]", "EPSG:4326");

        Assert.Equal("Default", item.Name);
        Assert.Equal("EPSG:4326", item.CurrentCoordSys);
        Assert.Equal("EPSG:4326", item.NewCoordSys);
        Assert.NotEmpty(item.CurrentWkt);
    }

    [Fact]
    public void NewCoordSys_CanBeChanged()
    {
        var item = new SpatialContextOverrideItem("Default", "GEOGCS[\"WGS84\"]", "EPSG:4326");
        item.NewCoordSys = "EPSG:3857";

        Assert.Equal("EPSG:3857", item.NewCoordSys);
    }
}

public sealed class ConnectionPropertyViewModelTests
{
    [Fact]
    public void Ctor_SetsProperties()
    {
        var vm = new ConnectionPropertyViewModel("File", "/data/test.sdf");

        Assert.Equal("File", vm.Name);
        Assert.Equal("/data/test.sdf", vm.Value);
    }

    [Fact]
    public void Value_RaisesPropertyChanged()
    {
        var vm = new ConnectionPropertyViewModel("File", "/old.sdf");
        bool raised = false;
        vm.PropertyChanged += (_, _) => raised = true;

        vm.Value = "/new.sdf";

        Assert.True(raised);
    }
}
