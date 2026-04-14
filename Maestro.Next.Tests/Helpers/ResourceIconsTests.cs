// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Maestro.Next.Helpers;
using Xunit;

namespace Maestro.Next.Tests.Helpers;

public sealed class ResourceIconsTests
{
    [Theory]
    [InlineData("FeatureSource", "🗄️")]
    [InlineData("LayerDefinition", "🗺️")]
    [InlineData("MapDefinition", "🌍")]
    [InlineData("WebLayout", "🌐")]
    [InlineData("ApplicationDefinition", "📱")]
    [InlineData("SymbolDefinition", "✦")]
    [InlineData("Folder", "📁")]
    [InlineData("UnknownType", "📄")]
    public void ForResourceType_ReturnsExpected(string resourceType, string expected)
    {
        Assert.Equal(expected, ResourceIcons.ForResourceType(resourceType));
    }

    [Theory]
    [InlineData("FeatureSource", "🗄️")]
    [InlineData("Xml", "📝")]
    [InlineData("Generic", "📄")]
    [InlineData("Unknown", "📄")]
    public void ForEditorType_ReturnsExpected(string editorType, string expected)
    {
        Assert.Equal(expected, ResourceIcons.ForEditorType(editorType));
    }
}
