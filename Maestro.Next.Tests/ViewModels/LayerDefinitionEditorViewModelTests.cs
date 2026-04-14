// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Maestro.Next.ViewModels;
using Xunit;

namespace Maestro.Next.Tests.ViewModels;

public sealed class LayerDefinitionEditorViewModelTests
{
    [Fact]
    public void Ctor_SetsTitle()
    {
        var vm = new LayerDefinitionEditorViewModel("Library://Layers/Roads.LayerDefinition");

        Assert.Contains("Roads", vm.Title);
        Assert.Contains("[LayerDefinition]", vm.Title);
        Assert.Equal("LayerDefinition", vm.IconKey);
    }

    [Fact]
    public void Initially_NotLoaded()
    {
        var vm = new LayerDefinitionEditorViewModel("Library://Layers/Test.LayerDefinition");

        Assert.False(vm.IsLoaded);
        Assert.False(vm.IsDirty);
        Assert.Empty(vm.ScaleRanges);
    }
}
