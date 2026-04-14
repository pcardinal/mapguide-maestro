// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.RegularExpressions;

namespace Maestro.Next.ViewModels;

/// <summary>
/// Find & Replace dialog ViewModel — operates on a text string (XML content)
/// </summary>
public partial class FindReplaceViewModel : ViewModelBase
{
    private readonly Func<string> _getText;
    private readonly Action<string> _setText;

    public FindReplaceViewModel(Func<string> getText, Action<string> setText)
    {
        _getText = getText;
        _setText = setText;
    }

    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _replaceText = string.Empty;
    [ObservableProperty] private bool _useRegex;
    [ObservableProperty] private bool _caseSensitive;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private int _matchCount;

    [RelayCommand]
    private void CountMatches()
    {
        if (string.IsNullOrEmpty(SearchText))
        {
            MatchCount = 0;
            StatusMessage = "";
            return;
        }

        try
        {
            var text = _getText();
            if (UseRegex)
            {
                var opts = CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                var matches = Regex.Matches(text, SearchText, opts);
                MatchCount = matches.Count;
            }
            else
            {
                var comparison = CaseSensitive
                    ? StringComparison.Ordinal
                    : StringComparison.OrdinalIgnoreCase;
                int count = 0, idx = 0;
                while ((idx = text.IndexOf(SearchText, idx, comparison)) >= 0)
                {
                    count++;
                    idx += SearchText.Length;
                }
                MatchCount = count;
            }
            StatusMessage = $"{MatchCount} match(es) found.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            MatchCount = 0;
        }
    }

    [RelayCommand]
    private void ReplaceAll()
    {
        if (string.IsNullOrEmpty(SearchText))
        {
            StatusMessage = "Nothing to search for.";
            return;
        }

        try
        {
            var text = _getText();
            string result;
            if (UseRegex)
            {
                var opts = CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                result = Regex.Replace(text, SearchText, ReplaceText ?? "", opts);
            }
            else
            {
                result = CaseSensitive
                    ? text.Replace(SearchText, ReplaceText ?? "", StringComparison.Ordinal)
                    : text.Replace(SearchText, ReplaceText ?? "", StringComparison.OrdinalIgnoreCase);
            }

            _setText(result);
            CountMatches(); // refresh count
            StatusMessage = $"Replaced. {MatchCount} remaining match(es).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Replace error: {ex.Message}";
        }
    }
}
