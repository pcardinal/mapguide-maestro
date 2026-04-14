// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Maestro.Next.ViewModels;
using Xunit;

namespace Maestro.Next.Tests.ViewModels;

public sealed class ThemeWizardViewModelTests
{
    [Fact]
    public void Ctor_SetsProperties()
    {
        var vm = new ThemeWizardViewModel("Library://FS.FeatureSource", "Default:Cities",
            new[] { "Name", "Population", "Country" });

        Assert.Equal(3, vm.PropertyNames.Count);
        Assert.Equal("Name", vm.SelectedPropertyName);
        Assert.Equal(25, vm.MaxValues);
        Assert.False(vm.Confirmed);
    }

    [Fact]
    public void Generate_CreatesRulesFromValues()
    {
        var vm = new ThemeWizardViewModel("Library://FS.FeatureSource", "S:C",
            new[] { "Type" });

        vm.Values.Add(new ThemeValueItem("Road", "ff0000ff"));
        vm.Values.Add(new ThemeValueItem("Rail", "ff00ff00"));
        vm.Values.Add(new ThemeValueItem("Water", "ffff0000") { IsSelected = false });

        vm.GenerateCommand.Execute(null);

        Assert.True(vm.Confirmed);
        Assert.Equal(2, vm.GeneratedRules.Count); // Water is deselected
        Assert.Contains("Road", vm.GeneratedRules[0].LegendLabel);
        Assert.Contains("Type = 'Road'", vm.GeneratedRules[0].Filter);
    }

    [Fact]
    public void Generate_HandlesNullValue()
    {
        var vm = new ThemeWizardViewModel("Library://FS.FeatureSource", "S:C",
            new[] { "Status" });

        vm.Values.Add(new ThemeValueItem("(null)", "ff808080"));

        vm.GenerateCommand.Execute(null);

        Assert.Single(vm.GeneratedRules);
        Assert.Contains("NULL", vm.GeneratedRules[0].Filter);
    }

    [Fact]
    public void Generate_EscapesSingleQuotes()
    {
        var vm = new ThemeWizardViewModel("Library://FS.FeatureSource", "S:C",
            new[] { "Name" });

        vm.Values.Add(new ThemeValueItem("O'Brien", "ffaabbcc"));

        vm.GenerateCommand.Execute(null);

        Assert.Single(vm.GeneratedRules);
        Assert.Contains("O''Brien", vm.GeneratedRules[0].Filter);
    }
}

public sealed class ThemeValueItemTests
{
    [Fact]
    public void Ctor_SetsDefaults()
    {
        var item = new ThemeValueItem("test", "ff112233");

        Assert.Equal("test", item.Value);
        Assert.Equal("ff112233", item.ColorHex);
        Assert.True(item.IsSelected);
    }

    [Fact]
    public void IsSelected_CanBeToggled()
    {
        var item = new ThemeValueItem("x", "ff000000");
        item.IsSelected = false;
        Assert.False(item.IsSelected);
    }
}
