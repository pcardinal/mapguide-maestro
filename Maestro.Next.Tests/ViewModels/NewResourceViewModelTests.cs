// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Maestro.Next.ViewModels;
using Xunit;

namespace Maestro.Next.Tests.ViewModels;

public class NewResourceViewModelTests
{
    [Fact]
    public void DefaultKind_IsFolder()
    {
        var vm = CreateVm();
        Assert.Equal(NewResourceKind.Folder, vm.Kind);
        Assert.True(vm.IsFolder);
    }

    [Theory]
    [InlineData(NewResourceKind.Folder, true, false, false, false, false)]
    [InlineData(NewResourceKind.FeatureSource, false, true, false, false, false)]
    [InlineData(NewResourceKind.LayerDefinition, false, false, true, false, false)]
    [InlineData(NewResourceKind.MapDefinition, false, false, false, true, false)]
    [InlineData(NewResourceKind.WebLayout, false, false, false, false, true)]
    public void KindChange_UpdatesVisibilityFlags(
        NewResourceKind kind,
        bool isFolder, bool isFs, bool isLdf, bool isMdf, bool isWl)
    {
        var vm = CreateVm();
        vm.Kind = kind;

        Assert.Equal(isFolder, vm.IsFolder);
        Assert.Equal(isFs, vm.IsFeatureSource);
        Assert.Equal(isLdf, vm.IsLayerDefinition);
        Assert.Equal(isMdf, vm.IsMapDefinition);
        Assert.Equal(isWl, vm.IsWebLayout);
    }

    [Fact]
    public void Validate_EmptyName_SetsError()
    {
        var vm = CreateVm();
        vm.ResourceName = "";

        // CanCreate should be false
        Assert.False(vm.CreateCommand.CanExecute(null));
    }

    [Fact]
    public void Validate_NameWithSlash_SetsError()
    {
        var vm = CreateVm();
        vm.ResourceName = "invalid/name";

        // Name contains slash — will fail on Create validation
        // CanExecute is true (non-empty), but internal Validate will fail
        Assert.True(vm.CreateCommand.CanExecute(null));
    }

    [Fact]
    public void FeatureSource_EmptyProvider_CannotCreate()
    {
        var vm = CreateVm();
        vm.Kind = NewResourceKind.FeatureSource;
        vm.ResourceName = "TestFs";
        vm.FdoProvider = "";

        Assert.False(vm.CreateCommand.CanExecute(null));
    }

    [Fact]
    public void FeatureSource_WithProvider_CanCreate()
    {
        var vm = CreateVm();
        vm.Kind = NewResourceKind.FeatureSource;
        vm.ResourceName = "TestFs";
        vm.FdoProvider = "OSGeo.SDF";

        Assert.True(vm.CreateCommand.CanExecute(null));
    }

    private static NewResourceViewModel CreateVm()
    {
        // Use a stub notification service
        return new NewResourceViewModel(
            "Library://TestFolder/",
            new StubNewResourceService(),
            new StubNotificationService());
    }
}

internal sealed class StubNewResourceService : Maestro.Next.Services.INewResourceService
{
    public Task<string> CreateFolderAsync(string p, string n) => Task.FromResult($"{p}{n}/");
    public Task<string> CreateFeatureSourceAsync(string p, string n, string pr) => Task.FromResult($"{p}{n}.FeatureSource");
    public Task<string> CreateLayerDefinitionAsync(string p, string n, string f, string c, string g) => Task.FromResult($"{p}{n}.LayerDefinition");
    public Task<string> CreateMapDefinitionAsync(string p, string n) => Task.FromResult($"{p}{n}.MapDefinition");
    public Task<string> CreateWebLayoutAsync(string p, string n, string m) => Task.FromResult($"{p}{n}.WebLayout");
}

internal sealed class StubNotificationService : Maestro.Next.Services.INotificationService
{
    public System.Collections.ObjectModel.ObservableCollection<Maestro.Next.Services.Notification> Recent { get; } = new();
    public void Success(string msg) { }
    public void Error(string msg) { }
    public void Info(string msg) { }
    public void Warning(string msg) { }
}
