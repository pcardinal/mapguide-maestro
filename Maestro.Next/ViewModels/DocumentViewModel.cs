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
using Maestro.Next.Services;
using Microsoft.Extensions.DependencyInjection;
using OSGeo.MapGuide.MaestroAPI.Resource;
using OSGeo.MapGuide.MaestroAPI.Resource.Validation;

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
    /// Callback to prompt user for a new ResourceId. Set by the view layer.
    /// </summary>
    public Func<string, Task<string?>>? SaveAsRequested { get; set; }

    /// <summary>
    /// Save As — saves under a different ResourceId
    /// </summary>
    [RelayCommand]
    protected virtual async Task SaveAsAsync()
    {
        if (ResourceId is null || SaveAsRequested is null) return;
        var newId = await SaveAsRequested(ResourceId);
        if (string.IsNullOrWhiteSpace(newId) || newId == ResourceId) return;

        try
        {
            IsBusy = true;
            BusyMessage = "Saving As...";

            var conn = Program.Services!.GetRequiredService<IConnectionService>()
                              .CurrentConnection!;

            // Copy the resource to the new ID, then re-point this editor
            await Task.Run(() =>
                conn.ResourceService.CopyResource(ResourceId, newId, true));

            ResourceId = newId;
            var name = newId.TrimEnd('/').Split('/').Last();
            Title = $"{System.IO.Path.GetFileNameWithoutExtension(name)} [{IconKey}]";
            IsDirty = false;

            Program.Services!.GetRequiredService<INotificationService>()
                   .Success($"Saved as {newId}");
        }
        catch (Exception ex)
        {
            Program.Services!.GetRequiredService<INotificationService>()
                   .Error($"Save As failed: {ex.Message}");
        }
        finally { IsBusy = false; BusyMessage = null; }
    }

    /// <summary>
    /// Called when the document is closed
    /// </summary>
    public virtual void OnClose() { }

    // ── Validation ──────────────────────────────────────────────
    public ObservableCollection<ValidationIssueViewModel> ValidationIssues { get; } = new();

    [ObservableProperty]
    private bool _hasValidationIssues;

    [RelayCommand]
    protected virtual async Task ValidateAsync()
    {
        if (ResourceId is null) return;
        try
        {
            IsBusy = true;
            BusyMessage = "Validating...";
            ValidationIssues.Clear();

            var conn = Program.Services!.GetRequiredService<IConnectionService>()
                              .CurrentConnection!;
            var resource = await Task.Run(
                () => conn.ResourceService.GetResource(ResourceId));
            var context = new ResourceValidationContext(conn);
            var issues = await Task.Run(
                () => ResourceValidatorSet.Validate(context, resource, true));

            foreach (var issue in issues)
            {
                ValidationIssues.Add(new ValidationIssueViewModel(
                    issue.Status.ToString(),
                    issue.Message,
                    issue.StatusCode.ToString()));
            }

            HasValidationIssues = ValidationIssues.Count > 0;

            if (!HasValidationIssues)
                Program.Services!.GetRequiredService<INotificationService>()
                       .Success("Validation passed — no issues found.");
        }
        catch (Exception ex)
        {
            Program.Services!.GetRequiredService<INotificationService>()
                   .Error($"Validation error: {ex.Message}");
        }
        finally { IsBusy = false; BusyMessage = null; }
    }

    // ── Resource References ─────────────────────────────────────
    public ObservableCollection<string> ResourceReferences { get; } = new();

    [ObservableProperty]
    private bool _hasResourceReferences;

    [RelayCommand]
    protected virtual async Task ShowReferencesAsync()
    {
        if (ResourceId is null) return;
        try
        {
            IsBusy = true;
            BusyMessage = "Finding references...";
            ResourceReferences.Clear();

            var conn = Program.Services!.GetRequiredService<IConnectionService>()
                              .CurrentConnection!;
            var refs = await Task.Run(
                () => conn.ResourceService.EnumerateResourceReferences(ResourceId));

            if (refs?.ResourceId != null)
            {
                foreach (var refId in refs.ResourceId)
                    ResourceReferences.Add(refId);
            }

            HasResourceReferences = ResourceReferences.Count > 0;

            var svc = Program.Services!.GetRequiredService<INotificationService>();
            svc.Info($"{ResourceReferences.Count} resource(s) reference this resource.");
        }
        catch (Exception ex)
        {
            Program.Services!.GetRequiredService<INotificationService>()
                   .Error($"References lookup failed: {ex.Message}");
        }
        finally { IsBusy = false; BusyMessage = null; }
    }
}

public record ValidationIssueViewModel(string Status, string Message, string Code)
{
    public string Icon => Status switch
    {
        "Error"       => "❌",
        "Warning"     => "⚠️",
        "Information" => "ℹ️",
        _             => "❔"
    };
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
