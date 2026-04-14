// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Maestro.Next.ViewModels;
using Xunit;

namespace Maestro.Next.Tests.ViewModels;

public sealed class ServerStatusViewModelTests
{
    [Fact]
    public void Initially_NotLoaded()
    {
        var vm = new ServerStatusViewModel();

        Assert.False(vm.IsLoaded);
        Assert.Empty(vm.ServerVersion);
        Assert.Empty(vm.Users);
        Assert.Empty(vm.Groups);
    }
}

public sealed class ExtensionSummaryItemTests
{
    [Fact]
    public void Summary_IncludesCounts()
    {
        var item = new ExtensionSummaryItem(
            "MyExt", "Default:Roads",
            new List<string> { "FullName = Name || ' Road'" },
            new List<string> { "Join: Cities → LeftOuter" });

        Assert.Contains("MyExt", item.Summary, StringComparison.Ordinal);
        Assert.Contains("1 calc", item.Summary, StringComparison.Ordinal);
        Assert.Contains("1 join", item.Summary, StringComparison.Ordinal);
    }

    [Fact]
    public void Summary_EmptyExtension()
    {
        var item = new ExtensionSummaryItem("Empty", "S:C", new(), new());

        Assert.Contains("0 calc", item.Summary, StringComparison.Ordinal);
        Assert.Contains("0 join", item.Summary, StringComparison.Ordinal);
    }
}

public sealed class SchemaClassViewModelAdditionalTests
{
    [Fact]
    public void Ctor_SetsProperties()
    {
        var props = new System.Collections.ObjectModel.ObservableCollection<SchemaPropertyViewModel>
        {
            new("Name", "String", false),
            new("ID", "Int32", true)
        };
        var vm = new SchemaClassViewModel("Default:Cities", props);

        Assert.Equal("Default:Cities", vm.QualifiedName);
        Assert.Equal(2, vm.Properties.Count);
    }
}

public sealed class SchemaPropertyViewModelTests
{
    [Fact]
    public void Icon_KeyIsKey()
    {
        var prop = new SchemaPropertyViewModel("ID", "Int32", true);
        Assert.Equal("🔑", prop.Icon);
    }

    [Fact]
    public void Icon_GeometryIsCompass()
    {
        var prop = new SchemaPropertyViewModel("Shape", "Geometry", false);
        Assert.Equal("📐", prop.Icon);
    }

    [Fact]
    public void Icon_DataIsClipboard()
    {
        var prop = new SchemaPropertyViewModel("Name", "String", false);
        Assert.Equal("📋", prop.Icon);
    }

    [Fact]
    public void Record_EqualityByValue()
    {
        var a = new SchemaPropertyViewModel("Name", "String", false);
        var b = new SchemaPropertyViewModel("Name", "String", false);
        Assert.Equal(a, b);
    }
}
