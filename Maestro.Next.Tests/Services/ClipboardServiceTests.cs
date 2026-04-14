// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Maestro.Next.Services;
using Xunit;

namespace Maestro.Next.Tests.Services;

public sealed class ClipboardServiceTests
{
    [Fact]
    public void Initially_HasNoContent()
    {
        var svc = new ClipboardService();

        Assert.False(svc.HasContent);
        Assert.Null(svc.ResourceId);
        Assert.Equal(ClipboardOperation.None, svc.Operation);
    }

    [Fact]
    public void SetCopy_StoresResourceIdAndOperation()
    {
        var svc = new ClipboardService();
        svc.SetCopy("Library://Test/Map.MapDefinition");

        Assert.True(svc.HasContent);
        Assert.Equal("Library://Test/Map.MapDefinition", svc.ResourceId);
        Assert.Equal(ClipboardOperation.Copy, svc.Operation);
    }

    [Fact]
    public void SetCut_StoresResourceIdAndOperation()
    {
        var svc = new ClipboardService();
        svc.SetCut("Library://Data/Source.FeatureSource");

        Assert.True(svc.HasContent);
        Assert.Equal("Library://Data/Source.FeatureSource", svc.ResourceId);
        Assert.Equal(ClipboardOperation.Cut, svc.Operation);
    }

    [Fact]
    public void Clear_RemovesContent()
    {
        var svc = new ClipboardService();
        svc.SetCopy("Library://Test.FeatureSource");
        svc.Clear();

        Assert.False(svc.HasContent);
        Assert.Null(svc.ResourceId);
        Assert.Equal(ClipboardOperation.None, svc.Operation);
    }

    [Fact]
    public void SetCopy_OverwritesPreviousCut()
    {
        var svc = new ClipboardService();
        svc.SetCut("Library://A.LayerDefinition");
        svc.SetCopy("Library://B.MapDefinition");

        Assert.Equal(ClipboardOperation.Copy, svc.Operation);
        Assert.Equal("Library://B.MapDefinition", svc.ResourceId);
    }
}
