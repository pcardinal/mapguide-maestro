// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Maestro.Next.ViewModels;
using Xunit;

namespace Maestro.Next.Tests.ViewModels;

public sealed class LoadPackageViewModelTests
{
    [Fact]
    public void Initially_NotUploading()
    {
        var vm = new LoadPackageViewModel();

        Assert.False(vm.IsUploading);
        Assert.False(vm.Completed);
        Assert.Equal(0, vm.Progress);
        Assert.Equal(string.Empty, vm.PackageFilePath);
        Assert.NotEmpty(vm.StatusMessage);
    }

    [Fact]
    public void PackageFilePath_CanBeSet()
    {
        var vm = new LoadPackageViewModel();
        vm.PackageFilePath = @"C:\temp\test.mgp";

        Assert.Equal(@"C:\temp\test.mgp", vm.PackageFilePath);
    }
}
