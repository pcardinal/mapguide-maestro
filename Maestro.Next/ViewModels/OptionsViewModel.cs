// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maestro.Next.Services;

namespace Maestro.Next.ViewModels;

public partial class OptionsViewModel : ViewModelBase
{
    private readonly IPreferencesService _prefs;

    public OptionsViewModel(IPreferencesService prefs)
    {
        _prefs = prefs;
        _selectedTheme = prefs.Theme;
    }

    [ObservableProperty]
    private AppTheme _selectedTheme;

    public AppTheme[] AvailableThemes { get; } =
        [AppTheme.Light, AppTheme.Dark, AppTheme.System];

    [RelayCommand]
    private void Apply()
    {
        _prefs.Theme = SelectedTheme;
        _prefs.Save();
    }
}
