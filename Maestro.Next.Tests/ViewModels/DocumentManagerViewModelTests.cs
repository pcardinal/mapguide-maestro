// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Maestro.Next.ViewModels;
using Xunit;

namespace Maestro.Next.Tests.ViewModels;

public class DocumentManagerViewModelTests
{
    [Fact]
    public void OpenDocument_AddsToOpenDocuments()
    {
        var mgr = new DocumentManagerViewModel();
        var doc = new TestDocumentViewModel("Library://Test.FeatureSource");

        mgr.OpenDocument(doc);

        Assert.Single(mgr.OpenDocuments);
        Assert.Same(doc, mgr.ActiveDocument);
        Assert.True(mgr.HasOpenDocuments);
    }

    [Fact]
    public void OpenDocument_SameResourceId_ActivatesExisting()
    {
        var mgr = new DocumentManagerViewModel();
        var doc1 = new TestDocumentViewModel("Library://Test.FeatureSource");
        var doc2 = new TestDocumentViewModel("Library://Other.LayerDefinition");
        var doc3 = new TestDocumentViewModel("Library://Test.FeatureSource");

        mgr.OpenDocument(doc1);
        mgr.OpenDocument(doc2);
        mgr.OpenDocument(doc3); // same ResourceId as doc1

        Assert.Equal(2, mgr.OpenDocuments.Count);
        Assert.Same(doc1, mgr.ActiveDocument); // reactivated, not added
    }

    [Fact]
    public void CloseDocument_RemovesAndActivatesAdjacent()
    {
        var mgr = new DocumentManagerViewModel();
        var doc1 = new TestDocumentViewModel("Library://A.FeatureSource");
        var doc2 = new TestDocumentViewModel("Library://B.LayerDefinition");
        var doc3 = new TestDocumentViewModel("Library://C.MapDefinition");

        mgr.OpenDocument(doc1);
        mgr.OpenDocument(doc2);
        mgr.OpenDocument(doc3);

        mgr.CloseDocumentCommand.Execute(doc2);

        Assert.Equal(2, mgr.OpenDocuments.Count);
        Assert.DoesNotContain(doc2, mgr.OpenDocuments);
        // Active should be an adjacent tab
        Assert.NotNull(mgr.ActiveDocument);
    }

    [Fact]
    public void CloseAllDocuments_ClearsEverything()
    {
        var mgr = new DocumentManagerViewModel();
        mgr.OpenDocument(new TestDocumentViewModel("Library://A.FeatureSource"));
        mgr.OpenDocument(new TestDocumentViewModel("Library://B.LayerDefinition"));

        mgr.CloseAllDocumentsCommand.Execute(null);

        Assert.Empty(mgr.OpenDocuments);
        Assert.Null(mgr.ActiveDocument);
        Assert.False(mgr.HasOpenDocuments);
    }

    [Fact]
    public void CloseLastDocument_ActiveBecomesNull()
    {
        var mgr = new DocumentManagerViewModel();
        var doc = new TestDocumentViewModel("Library://X.FeatureSource");
        mgr.OpenDocument(doc);

        mgr.CloseDocumentCommand.Execute(doc);

        Assert.Empty(mgr.OpenDocuments);
        Assert.Null(mgr.ActiveDocument);
    }
}

/// <summary>Test double for DocumentViewModel</summary>
internal class TestDocumentViewModel : DocumentViewModel
{
    public override string IconKey => "Test";
    public bool SaveWasCalled { get; private set; }

    public TestDocumentViewModel(string resourceId)
    {
        ResourceId = resourceId;
        Title = resourceId;
    }

    protected override Task SaveAsync()
    {
        SaveWasCalled = true;
        IsDirty = false;
        return Task.CompletedTask;
    }
}
