// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Maestro.Next.ViewModels;
using Xunit;

namespace Maestro.Next.Tests.ViewModels;

public sealed class MapDefinitionEditorViewModelTests
{
    [Fact]
    public void Ctor_SetsTitle()
    {
        var vm = new MapDefinitionEditorViewModel("Library://Maps/World.MapDefinition");

        Assert.Contains("World", vm.Title);
        Assert.Contains("[MapDefinition]", vm.Title);
        Assert.Equal("MapDefinition", vm.IconKey);
    }

    [Fact]
    public void Initially_NotLoaded()
    {
        var vm = new MapDefinitionEditorViewModel("Library://Maps/Test.MapDefinition");

        Assert.False(vm.IsLoaded);
        Assert.False(vm.IsDirty);
        Assert.Empty(vm.LayerTree);
    }

    [Fact]
    public void ExtentFields_UpdateDisplay()
    {
        var vm = new MapDefinitionEditorViewModel("Library://Maps/Test.MapDefinition");
        vm.MinX = -180;
        vm.MinY = -90;
        vm.MaxX = 180;
        vm.MaxY = 90;

        Assert.Contains("-180", vm.Extents);
        Assert.Contains("90", vm.Extents);
    }
}

public sealed class MapLayerTreeNodeTests
{
    [Fact]
    public void GroupNode_HasFolderIcon()
    {
        var node = new MapLayerTreeNode("TestGroup", true, true, "Test Group");

        Assert.True(node.IsGroup);
        Assert.Equal("📁", node.Icon);
        Assert.Empty(node.ResourceId);
    }

    [Fact]
    public void LayerNode_HasMapIcon()
    {
        var node = new MapLayerTreeNode("TestLayer", false, true, "Test Layer",
            "Library://Layers/Test.LayerDefinition", true);

        Assert.False(node.IsGroup);
        Assert.Equal("🗺", node.Icon);
        Assert.NotEmpty(node.ResourceId);
        Assert.True(node.Selectable);
    }
}
