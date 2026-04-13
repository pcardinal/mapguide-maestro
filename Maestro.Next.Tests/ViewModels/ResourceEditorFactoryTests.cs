// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Maestro.Next.ViewModels;
using OSGeo.MapGuide.ObjectModels;
using Xunit;

namespace Maestro.Next.Tests.ViewModels;

public class ResourceEditorFactoryTests
{
    [Theory]
    [InlineData(nameof(ResourceTypes.FeatureSource), typeof(FeatureSourceEditorViewModel))]
    [InlineData(nameof(ResourceTypes.LayerDefinition), typeof(LayerDefinitionEditorViewModel))]
    [InlineData(nameof(ResourceTypes.MapDefinition), typeof(MapDefinitionEditorViewModel))]
    [InlineData(nameof(ResourceTypes.WebLayout), typeof(WebLayoutEditorViewModel))]
    [InlineData(nameof(ResourceTypes.ApplicationDefinition), typeof(ApplicationDefinitionEditorViewModel))]
    [InlineData(nameof(ResourceTypes.SymbolDefinition), typeof(SymbolDefinitionEditorViewModel))]
    public void CreateEditor_KnownType_ReturnsCorrectViewModel(string resourceType, Type expectedType)
    {
        var editor = ResourceEditorFactory.CreateEditor("Library://Test." + resourceType, resourceType);

        Assert.NotNull(editor);
        Assert.IsType(expectedType, editor);
    }

    [Theory]
    [InlineData(nameof(ResourceTypes.PrintLayout))]
    [InlineData(nameof(ResourceTypes.LoadProcedure))]
    [InlineData(nameof(ResourceTypes.WatermarkDefinition))]
    public void CreateEditor_GenericType_ReturnsGenericEditor(string resourceType)
    {
        var editor = ResourceEditorFactory.CreateEditor("Library://Test." + resourceType, resourceType);

        Assert.NotNull(editor);
        Assert.IsType<GenericResourceEditorViewModel>(editor);
    }

    [Fact]
    public void CreateEditor_UnknownType_ReturnsNull()
    {
        var editor = ResourceEditorFactory.CreateEditor("Library://Test.SomethingElse", "SomethingElse");

        Assert.Null(editor);
    }

    [Theory]
    [InlineData(nameof(ResourceTypes.FeatureSource), "FeatureSource")]
    [InlineData(nameof(ResourceTypes.LayerDefinition), "LayerDefinition")]
    [InlineData(nameof(ResourceTypes.MapDefinition), "MapDefinition")]
    [InlineData(nameof(ResourceTypes.WebLayout), "WebLayout")]
    [InlineData(nameof(ResourceTypes.ApplicationDefinition), "ApplicationDefinition")]
    [InlineData(nameof(ResourceTypes.SymbolDefinition), "SymbolDefinition")]
    public void CreateEditor_SetsResourceId(string resourceType, string _)
    {
        var resourceId = $"Library://Folder/{resourceType}Test.{resourceType}";
        var editor = ResourceEditorFactory.CreateEditor(resourceId, resourceType);

        Assert.NotNull(editor);
        Assert.Equal(resourceId, editor!.ResourceId);
    }

    [Fact]
    public void CreateEditor_SetsTitle()
    {
        var editor = ResourceEditorFactory.CreateEditor(
            "Library://Maps/MyMap.MapDefinition",
            nameof(ResourceTypes.MapDefinition));

        Assert.NotNull(editor);
        Assert.Contains("MyMap", editor!.Title);
    }
}
