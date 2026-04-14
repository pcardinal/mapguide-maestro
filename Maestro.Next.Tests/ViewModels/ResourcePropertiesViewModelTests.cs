// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Maestro.Next.ViewModels;
using Xunit;

namespace Maestro.Next.Tests.ViewModels;

public sealed class ResourcePropertiesViewModelTests
{
    [Fact]
    public void Ctor_ParsesResourceId()
    {
        var vm = new ResourcePropertiesViewModel("Library://Maps/World.MapDefinition");

        Assert.Equal("Library://Maps/World.MapDefinition", vm.ResourceId);
        Assert.Equal("World", vm.Name);
        Assert.Equal("MapDefinition", vm.ResourceType);
        Assert.False(vm.IsFolder);
    }

    [Fact]
    public void Ctor_Folder_DetectedCorrectly()
    {
        var vm = new ResourcePropertiesViewModel("Library://Maps/");

        Assert.Equal("Maps", vm.Name);
        Assert.True(vm.IsFolder);
    }

    [Fact]
    public void Defaults_BeforeLoad()
    {
        var vm = new ResourcePropertiesViewModel("Library://Test.FeatureSource");

        Assert.Equal("—", vm.CreatedDate);
        Assert.Equal("—", vm.ModifiedDate);
        Assert.Equal("—", vm.Owner);
        Assert.Equal(0, vm.ReferenceCount);
        Assert.Equal(string.Empty, vm.XmlPreview);
        Assert.False(vm.IsLoaded);
    }
}
