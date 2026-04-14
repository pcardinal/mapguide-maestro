// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Maestro.Next.ViewModels;

/// <summary>
/// ViewModel for the Tip of the Day dialog
/// </summary>
public partial class TipOfTheDayViewModel : ViewModelBase
{
    private static readonly string[] Tips =
    [
        "Use Ctrl+S to save the active document and Ctrl+Shift+S to save all open documents.",
        "Right-click a resource in the Site Explorer for context menu actions like Copy, Cut, Rename, and Delete.",
        "Use the Expression Builder (... button) to build FDO filter expressions with property auto-completion.",
        "Click '✓ Validate' in the Expression Builder to check your filter syntax before applying.",
        "Press F5 to refresh the Site Explorer tree, F2 to rename, and Delete to remove a resource.",
        "Hold Ctrl while dropping a resource to copy instead of move.",
        "Use Tools → Server Status to view server version, active users, and groups.",
        "The Theme Wizard generates style rules automatically from distinct property values.",
        "Use Tools → Create Package to export resources to a .mgp file for backup or migration.",
        "Edit any resource as raw XML by right-clicking and selecting 'Edit as XML'.",
        "The View → Close All Documents command closes all open editor tabs at once.",
        "Use the Find/Replace feature (Ctrl+F in XML editor) with regex support for advanced searches.",
        "The CS Override tab in FeatureSource lets you override coordinate systems per spatial context.",
        "MapGuide Maestro (Next) is built with Avalonia UI and runs on Windows, macOS, and Linux."
    ];

    private int _currentIndex;

    public TipOfTheDayViewModel()
    {
        _currentIndex = Random.Shared.Next(Tips.Length);
        CurrentTip = Tips[_currentIndex];
    }

    [ObservableProperty] private string _currentTip;
    [ObservableProperty] private bool _showOnStartup = true;

    [RelayCommand]
    private void NextTip()
    {
        _currentIndex = (_currentIndex + 1) % Tips.Length;
        CurrentTip = Tips[_currentIndex];
    }

    [RelayCommand]
    private void PreviousTip()
    {
        _currentIndex = (_currentIndex - 1 + Tips.Length) % Tips.Length;
        CurrentTip = Tips[_currentIndex];
    }

    public string TipNumber => $"Tip {_currentIndex + 1} of {Tips.Length}";
}
