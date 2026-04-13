// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Maestro.Next.ViewModels;

/// <summary>
/// Base class for all editor document tabs
/// </summary>
public abstract partial class DocumentViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = "Untitled";

    [ObservableProperty]
    private bool _isDirty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _busyMessage;

    /// <summary>
    /// The resource ID this document is editing (null for new documents)
    /// </summary>
    public string? ResourceId { get; protected set; }

    /// <summary>
    /// Icon key identifying the resource type
    /// </summary>
    public abstract string IconKey { get; }

    /// <summary>
    /// Save changes to the server
    /// </summary>
    [RelayCommand]
    protected abstract Task SaveAsync();

    /// <summary>
    /// Called when the document is closed
    /// </summary>
    public virtual void OnClose() { }
}

/// <summary>
/// Manages the open document tabs in the workbench
/// </summary>
public partial class DocumentManagerViewModel : ViewModelBase
{
    public ObservableCollection<DocumentViewModel> OpenDocuments { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasOpenDocuments))]
    private DocumentViewModel? _activeDocument;

    public bool HasOpenDocuments => OpenDocuments.Count > 0;

    /// <summary>
    /// Opens a document tab, or activates it if already open
    /// </summary>
    public void OpenDocument(DocumentViewModel doc)
    {
        // If already open, just activate it
        if (doc.ResourceId != null)
        {
            var existing = OpenDocuments.FirstOrDefault(d => d.ResourceId == doc.ResourceId);
            if (existing != null)
            {
                ActiveDocument = existing;
                return;
            }
        }

        OpenDocuments.Add(doc);
        ActiveDocument = doc;
    }

    [RelayCommand]
    private void CloseDocument(DocumentViewModel doc)
    {
        doc.OnClose();
        var idx = OpenDocuments.IndexOf(doc);
        OpenDocuments.Remove(doc);

        // Activate the adjacent tab
        if (OpenDocuments.Count > 0)
        {
            var newIdx = Math.Max(0, Math.Min(idx, OpenDocuments.Count - 1));
            ActiveDocument = OpenDocuments[newIdx];
        }
        else
        {
            ActiveDocument = null;
        }
    }

    [RelayCommand]
    private void CloseAllDocuments()
    {
        foreach (var doc in OpenDocuments.ToList())
            doc.OnClose();

        OpenDocuments.Clear();
        ActiveDocument = null;
    }
}
