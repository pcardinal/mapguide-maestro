// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Maestro.Next.ViewModels;
using Maestro.Next.ViewModels.Editors;
using Xunit;

namespace Maestro.Next.Tests.ViewModels;

public sealed class DocumentManagerExtendedTests
{
    [Fact]
    public void OpenDocument_AddsToCollection()
    {
        var mgr = new DocumentManagerViewModel();
        var doc = new GenericResourceEditorViewModel("Library://Test.FeatureSource", "FeatureSource");

        mgr.OpenDocument(doc);

        Assert.Single(mgr.OpenDocuments);
        Assert.Same(doc, mgr.ActiveDocument);
    }

    [Fact]
    public void OpenDocument_SameResourceId_ActivatesExisting()
    {
        var mgr = new DocumentManagerViewModel();
        var doc1 = new GenericResourceEditorViewModel("Library://Test.FeatureSource", "FeatureSource");
        var doc2 = new GenericResourceEditorViewModel("Library://Test.FeatureSource", "FeatureSource");

        mgr.OpenDocument(doc1);
        mgr.OpenDocument(doc2);

        Assert.Single(mgr.OpenDocuments);
        Assert.Same(doc1, mgr.ActiveDocument);
    }

    [Fact]
    public void ActivateDocument_SetsActiveDocument()
    {
        var mgr = new DocumentManagerViewModel();
        var doc1 = new GenericResourceEditorViewModel("Library://A.FeatureSource", "FeatureSource");
        var doc2 = new GenericResourceEditorViewModel("Library://B.FeatureSource", "FeatureSource");

        mgr.OpenDocument(doc1);
        mgr.OpenDocument(doc2);
        mgr.ActivateDocument(doc1);

        Assert.Same(doc1, mgr.ActiveDocument);
    }

    [Fact]
    public void CloseDocument_RemovesFromCollection()
    {
        var mgr = new DocumentManagerViewModel();
        var doc = new GenericResourceEditorViewModel("Library://Test.FeatureSource", "FeatureSource");

        mgr.OpenDocument(doc);
        mgr.CloseDocumentCommand.Execute(doc);

        Assert.Empty(mgr.OpenDocuments);
        Assert.Null(mgr.ActiveDocument);
    }

    [Fact]
    public void CloseAllDocuments_ClearsAll()
    {
        var mgr = new DocumentManagerViewModel();
        mgr.OpenDocument(new GenericResourceEditorViewModel("Library://A.FeatureSource", "FeatureSource"));
        mgr.OpenDocument(new GenericResourceEditorViewModel("Library://B.MapDefinition", "MapDefinition"));

        mgr.CloseAllDocumentsCommand.Execute(null);

        Assert.Empty(mgr.OpenDocuments);
        Assert.False(mgr.HasOpenDocuments);
    }

    [Fact]
    public void HasOpenDocuments_ReflectsState()
    {
        var mgr = new DocumentManagerViewModel();
        Assert.False(mgr.HasOpenDocuments);

        mgr.OpenDocument(new GenericResourceEditorViewModel("Library://X.FeatureSource", "FeatureSource"));
        Assert.True(mgr.HasOpenDocuments);
    }
}

public sealed class ResourceEditorFactoryExtendedTests
{
    [Theory]
    [InlineData("DrawingSource")]
    [InlineData("WatermarkDefinition")]
    [InlineData("TileSetDefinition")]
    public void CreateEditor_SpecializedTypes_ReturnsEditor(string type)
    {
        var editor = ResourceEditorFactory.CreateEditor($"Library://Test.{type}", type);
        Assert.NotNull(editor);
    }

    [Fact]
    public void CreateEditor_UnknownType_ReturnsNull()
    {
        var editor = ResourceEditorFactory.CreateEditor("Library://Test.Unknown", "SomethingNew");
        Assert.Null(editor);
    }
}

public sealed class SpecializedEditorTests
{
    [Fact]
    public void DrawingSourceEditor_SetsTitle()
    {
        var vm = new DrawingSourceEditorViewModel("Library://DWF/Building.DrawingSource");
        Assert.Contains("Building", vm.Title, StringComparison.Ordinal);
        Assert.Contains("[DrawingSource]", vm.Title, StringComparison.Ordinal);
    }

    [Fact]
    public void WatermarkEditor_SetsTitle()
    {
        var vm = new WatermarkEditorViewModel("Library://Watermarks/Logo.WatermarkDefinition");
        Assert.Contains("Logo", vm.Title, StringComparison.Ordinal);
    }

    [Fact]
    public void TileSetEditor_SetsTitle()
    {
        var vm = new TileSetEditorViewModel("Library://Tiles/Base.TileSetDefinition");
        Assert.Contains("Base", vm.Title, StringComparison.Ordinal);
    }
}

public sealed class LoginViewModelTests
{
    [Fact]
    public void Ctor_DefaultValues()
    {
        var vm = new LoginViewModel(null!);
        Assert.NotEmpty(vm.ServerUrl);
        Assert.NotEmpty(vm.Username);
        Assert.False(vm.IsConnecting);
    }

    [Fact]
    public void ServerUrl_CanBeChanged()
    {
        var vm = new LoginViewModel(null!);
        vm.ServerUrl = "http://myserver:8008/mapguide/mapagent/mapagent.fcgi";
        Assert.Equal("http://myserver:8008/mapguide/mapagent/mapagent.fcgi", vm.ServerUrl);
    }

    [Fact]
    public void Password_CanBeSet()
    {
        var vm = new LoginViewModel(null!);
        vm.Password = "secret";
        Assert.Equal("secret", vm.Password);
    }
}
