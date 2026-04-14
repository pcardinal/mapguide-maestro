// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Maestro.Next.ViewModels;
using Xunit;

namespace Maestro.Next.Tests.ViewModels;

public sealed class CreatePackageViewModelTests
{
    [Fact]
    public void Initially_NotCreating()
    {
        var vm = new CreatePackageViewModel();

        Assert.False(vm.IsCreating);
        Assert.False(vm.Completed);
        Assert.Equal(0, vm.Progress);
        Assert.Empty(vm.Resources);
        Assert.Equal("Library://", vm.FolderResourceId);
    }

    [Fact]
    public void OutputFilePath_CanBeSet()
    {
        var vm = new CreatePackageViewModel();
        vm.OutputFilePath = @"C:\temp\test.mgp";

        Assert.Equal(@"C:\temp\test.mgp", vm.OutputFilePath);
    }
}

public sealed class PackageResourceItemTests
{
    [Fact]
    public void Ctor_SetsProperties()
    {
        var item = new PackageResourceItem("Library://Data/Test.FeatureSource", true);

        Assert.Equal("Library://Data/Test.FeatureSource", item.ResourceId);
        Assert.True(item.IsSelected);
    }

    [Fact]
    public void IsSelected_CanBeToggled()
    {
        var item = new PackageResourceItem("Library://Maps/Test.MapDefinition", true);
        item.IsSelected = false;

        Assert.False(item.IsSelected);
    }
}
