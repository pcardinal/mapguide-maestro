// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maestro.Next.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Maestro.Next.ViewModels;

/// <summary>
/// ViewModel for the global quick-open search palette (Ctrl+P)
/// </summary>
public partial class QuickSearchViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResults))]
    private string _searchText = string.Empty;

    [ObservableProperty] private bool _isVisible;

    public ObservableCollection<QuickSearchResult> Results { get; } = new();
    public bool HasResults => Results.Count > 0;

    /// <summary>Raised when a result is selected — passes the ResourceId</summary>
    public Action<string>? ResultSelected { get; set; }

    partial void OnSearchTextChanged(string value)
    {
        Search(value);
    }

    private void Search(string query)
    {
        Results.Clear();

        if (string.IsNullOrWhiteSpace(query) || query.Length < 2) return;

        var connSvc = Program.Services?.GetService<IConnectionService>();
        if (connSvc?.CurrentConnection is null) return;

        try
        {
            var conn = connSvc.CurrentConnection;
            var list = conn.ResourceService.GetRepositoryResources("Library://", -1);
            if (list.Items == null) return;

            var q = query.Trim();
            int count = 0;
            foreach (var item in list.Items)
            {
                if (item is OSGeo.MapGuide.ObjectModels.Common.ResourceListResourceDocument doc)
                {
                    var name = doc.ResourceId.TrimEnd('/').Split('/').Last();
                    if (name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                        doc.ResourceId.Contains(q, StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = doc.ResourceId.Split('.');
                        var type = parts.Length > 1 ? parts[^1] : "Resource";
                        Results.Add(new QuickSearchResult(doc.ResourceId, name, type));
                        count++;
                        if (count >= 20) break;
                    }
                }
            }
        }
        catch
        {
            // Silently ignore search errors
        }

        OnPropertyChanged(nameof(HasResults));
    }

    [RelayCommand]
    private void SelectResult(QuickSearchResult? result)
    {
        if (result is null) return;
        IsVisible = false;
        SearchText = string.Empty;
        ResultSelected?.Invoke(result.ResourceId);
    }

    [RelayCommand]
    private void Toggle()
    {
        IsVisible = !IsVisible;
        if (!IsVisible)
        {
            SearchText = string.Empty;
            Results.Clear();
        }
    }
}

public record QuickSearchResult(string ResourceId, string Name, string ResourceType)
{
    public string Icon => ResourceTypeIconMap.GetIcon(ResourceType, false);
    public string Display => $"{Icon} {Name}  ({ResourceType})";
}
