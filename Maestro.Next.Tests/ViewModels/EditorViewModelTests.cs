// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Maestro.Next.ViewModels;
using Xunit;

namespace Maestro.Next.Tests.ViewModels;

public sealed class WebLayoutEditorViewModelTests
{
    [Fact]
    public void Ctor_SetsTitle()
    {
        var vm = new WebLayoutEditorViewModel("Library://Layouts/Main.WebLayout");

        Assert.Contains("Main", vm.Title);
        Assert.Contains("[WebLayout]", vm.Title);
        Assert.Equal("WebLayout", vm.IconKey);
    }
}

public sealed class SymbolDefinitionEditorViewModelTests
{
    [Fact]
    public void Ctor_SetsTitle()
    {
        var vm = new SymbolDefinitionEditorViewModel("Library://Symbols/Arrow.SymbolDefinition");

        Assert.Contains("Arrow", vm.Title);
        Assert.Contains("[Symbol]", vm.Title);
    }
}

public sealed class ApplicationDefinitionEditorViewModelTests
{
    [Fact]
    public void Ctor_SetsTitle()
    {
        var vm = new ApplicationDefinitionEditorViewModel("Library://Fusion/App.ApplicationDefinition");

        Assert.Contains("App", vm.Title);
        Assert.Contains("[Fusion]", vm.Title);
    }
}

public sealed class GenericResourceEditorViewModelTests
{
    [Fact]
    public void Ctor_SetsTitle()
    {
        var vm = new GenericResourceEditorViewModel("Library://Data/Drawing.DrawingSource", "DrawingSource");

        Assert.Contains("Drawing", vm.Title);
        Assert.Contains("[DrawingSource]", vm.Title);
    }

    [Fact]
    public void Ctor_FallbackForUnknownType()
    {
        var vm = new GenericResourceEditorViewModel("Library://Custom/Thing.CustomType", "CustomType");

        Assert.Contains("Thing", vm.Title);
        Assert.Contains("[CustomType]", vm.Title);
    }
}
