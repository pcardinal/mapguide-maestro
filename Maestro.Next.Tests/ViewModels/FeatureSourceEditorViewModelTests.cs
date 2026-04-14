// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Maestro.Next.ViewModels;
using Xunit;

namespace Maestro.Next.Tests.ViewModels;

public sealed class FeatureSourceEditorViewModelTests
{
    [Fact]
    public void Ctor_SetsTitle()
    {
        var vm = new FeatureSourceEditorViewModel("Library://Data/Parcels.FeatureSource");

        Assert.Contains("Parcels", vm.Title);
        Assert.Contains("[FeatureSource]", vm.Title);
        Assert.Equal("FeatureSource", vm.IconKey);
    }

    [Fact]
    public void Initially_NotLoaded()
    {
        var vm = new FeatureSourceEditorViewModel("Library://Data/Test.FeatureSource");

        Assert.False(vm.IsLoaded);
        Assert.False(vm.IsDirty);
        Assert.Empty(vm.ConnectionProperties);
        Assert.Empty(vm.SchemaClasses);
        Assert.False(vm.SchemaLoaded);
    }

    [Fact]
    public void ConnectionPropertyChanged_MarksDirty()
    {
        var prop = new ConnectionPropertyViewModel("File", "/data/test.sdf");
        bool changed = false;
        prop.PropertyChanged += (_, _) => changed = true;

        prop.Value = "/data/updated.sdf";

        Assert.True(changed);
    }
}

public sealed class SchemaClassViewModelTests
{
    [Fact]
    public void Properties_Accessible()
    {
        var props = new System.Collections.ObjectModel.ObservableCollection<SchemaPropertyViewModel>
        {
            new("ID", "Int32", true),
            new("Name", "String", false),
            new("Geometry", "Geometry", false)
        };
        var cls = new SchemaClassViewModel("Default:Parcels", props);

        Assert.Equal("Default:Parcels", cls.QualifiedName);
        Assert.Equal(3, cls.Properties.Count);
    }

    [Fact]
    public void PropertyIcons_CorrectForTypes()
    {
        var key = new SchemaPropertyViewModel("ID", "Int32", true);
        var geom = new SchemaPropertyViewModel("Shape", "Geometry", false);
        var normal = new SchemaPropertyViewModel("Name", "String", false);

        Assert.Equal("🔑", key.Icon);
        Assert.Equal("📐", geom.Icon);
        Assert.Equal("📋", normal.Icon);
    }
}
