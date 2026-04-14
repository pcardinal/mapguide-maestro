// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Maestro.Next.ViewModels;
using Xunit;

namespace Maestro.Next.Tests.ViewModels;

public sealed class DocumentViewModelBaseTests
{
    [Fact]
    public void IsDirty_RaisesPropertyChanged()
    {
        var vm = new GenericResourceEditorViewModel("Library://Test.DrawingSource", "DrawingSource");
        bool raised = false;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.IsDirty)) raised = true;
        };

        vm.IsDirty = true;

        Assert.True(raised);
        Assert.True(vm.IsDirty);
    }

    [Fact]
    public void ResourceId_IsSet()
    {
        var vm = new GenericResourceEditorViewModel("Library://Maps/Test.MapDefinition", "MapDefinition");

        Assert.Equal("Library://Maps/Test.MapDefinition", vm.ResourceId);
    }

    [Fact]
    public void IsBusy_DefaultFalse()
    {
        var vm = new GenericResourceEditorViewModel("Library://Test.FeatureSource", "FeatureSource");

        Assert.False(vm.IsBusy);
        Assert.Null(vm.BusyMessage);
    }

    [Fact]
    public void LoadPackage_FilePath_CanBeSet()
    {
        var vm = new LoadPackageViewModel();
        vm.PackageFilePath = @"C:\temp\test.mgp";

        Assert.Equal(@"C:\temp\test.mgp", vm.PackageFilePath);
    }

    [Fact]
    public void XmlEditor_Initially_NotLoaded()
    {
        var vm = new XmlEditorViewModel("Library://Data/Test.FeatureSource");

        Assert.False(vm.IsLoaded);
        Assert.Empty(vm.XmlContent);
    }
}
