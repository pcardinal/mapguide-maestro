// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Maestro.Next.Helpers;
using Maestro.Next.Services;
using Maestro.Next.ViewModels;
using Xunit;

namespace Maestro.Next.Tests.ViewModels;

public sealed class ResourceTreeNodeTests
{
    [Fact]
    public void IsCutSource_DefaultFalse()
    {
        var item = new ResourceListItem("Library://Test.FeatureSource", "Test", "FeatureSource", false);
        var node = CreateNode(item);

        Assert.False(node.IsCutSource);
        Assert.Equal(1.0, node.Opacity);
    }

    [Fact]
    public void IsCutSource_True_ReducesOpacity()
    {
        var item = new ResourceListItem("Library://Test.FeatureSource", "Test", "FeatureSource", false);
        var node = CreateNode(item);

        node.IsCutSource = true;

        Assert.True(node.IsCutSource);
        Assert.Equal(0.4, node.Opacity);
    }

    [Fact]
    public void IsCutSource_Reset_RestoresOpacity()
    {
        var item = new ResourceListItem("Library://Test.FeatureSource", "Test", "FeatureSource", false);
        var node = CreateNode(item);

        node.IsCutSource = true;
        node.IsCutSource = false;

        Assert.Equal(1.0, node.Opacity);
    }

    [Fact]
    public void FolderNode_HasIcon()
    {
        var item = new ResourceListItem("Library://Data/", "Data", "Folder", true);
        var node = CreateNode(item);

        Assert.True(node.IsFolder);
        Assert.NotEmpty(node.Icon);
    }

    [Fact]
    public void NonFolderNode_Properties()
    {
        var item = new ResourceListItem("Library://Map.MapDefinition", "Map", "MapDefinition", false);
        var node = CreateNode(item);

        Assert.Equal("Map", node.Name);
        Assert.Equal("Library://Map.MapDefinition", node.ResourceId);
        Assert.Equal("MapDefinition", node.ResourceType);
        Assert.False(node.IsFolder);
    }

    private static ResourceTreeNode CreateNode(ResourceListItem item)
    {
        // We need a mock IResourceService — use null-forgiving since we won't call methods
        return new ResourceTreeNode(item, null!);
    }
}

public sealed class ResourceTypeIconMapTests
{
    [Theory]
    [InlineData("FeatureSource", false)]
    [InlineData("LayerDefinition", false)]
    [InlineData("MapDefinition", false)]
    [InlineData("Folder", true)]
    public void GetIcon_ReturnsNonEmpty(string resourceType, bool isFolder)
    {
        var icon = ResourceTypeIconMap.GetIcon(resourceType, isFolder);
        Assert.NotEmpty(icon);
    }

    [Fact]
    public void GetIcon_UnknownType_ReturnsFallback()
    {
        var icon = ResourceTypeIconMap.GetIcon("SomethingUnknown", false);
        Assert.NotEmpty(icon);
    }
}
