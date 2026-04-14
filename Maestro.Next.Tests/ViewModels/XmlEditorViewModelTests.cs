// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Maestro.Next.ViewModels;
using Xunit;

namespace Maestro.Next.Tests.ViewModels;

public sealed class XmlEditorViewModelTests
{
    [Fact]
    public void Ctor_SetsResourceIdAndTitle()
    {
        var vm = new XmlEditorViewModel("Library://Maps/World.MapDefinition");

        Assert.Equal("Library://Maps/World.MapDefinition", vm.ResourceId);
        Assert.Contains("World", vm.Title);
        Assert.Contains("[XML]", vm.Title);
        Assert.Equal("Xml", vm.IconKey);
    }

    [Fact]
    public void Initially_NotLoadedNotDirty()
    {
        var vm = new XmlEditorViewModel("Library://Test.FeatureSource");

        Assert.False(vm.IsLoaded);
        Assert.False(vm.IsDirty);
        Assert.Equal(string.Empty, vm.XmlContent);
        Assert.Equal(string.Empty, vm.ServerXml);
    }

    [Fact]
    public void ShowDiff_TogglesCorrectly()
    {
        var vm = new XmlEditorViewModel("Library://Test.FeatureSource");

        Assert.False(vm.ShowDiff);

        vm.ToggleDiffCommand.Execute(null);
        Assert.True(vm.ShowDiff);

        vm.ToggleDiffCommand.Execute(null);
        Assert.False(vm.ShowDiff);
    }

    [Fact]
    public void HasChanges_DetectsModification()
    {
        var vm = new XmlEditorViewModel("Library://Test.FeatureSource");
        // Simulate loaded state
        vm.XmlContent = "<xml>original</xml>";
        vm.ServerXml = "<xml>original</xml>";

        Assert.False(vm.HasChanges);

        vm.XmlContent = "<xml>modified</xml>";
        Assert.True(vm.HasChanges);
    }
}
