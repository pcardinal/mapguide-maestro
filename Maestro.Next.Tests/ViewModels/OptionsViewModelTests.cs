// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Maestro.Next.ViewModels;
using Maestro.Next.Services;
using Xunit;

namespace Maestro.Next.Tests.ViewModels;

public class OptionsViewModelTests
{
    [Fact]
    public void DefaultTheme_MatchesPreferences()
    {
        var prefs = new StubPreferencesService { Theme = AppTheme.Dark };
        var vm = new OptionsViewModel(prefs);

        Assert.Equal(AppTheme.Dark, vm.SelectedTheme);
    }

    [Fact]
    public void Apply_UpdatesPreferencesTheme()
    {
        var prefs = new StubPreferencesService { Theme = AppTheme.Dark };
        var vm = new OptionsViewModel(prefs);

        vm.SelectedTheme = AppTheme.Light;
        vm.ApplyCommand.Execute(null);

        Assert.Equal(AppTheme.Light, prefs.Theme);
        Assert.True(prefs.SaveCalled);
    }

    [Fact]
    public void AvailableThemes_Contains3Options()
    {
        var prefs = new StubPreferencesService();
        var vm = new OptionsViewModel(prefs);

        Assert.Equal(3, vm.AvailableThemes.Length);
        Assert.Contains(AppTheme.Light, vm.AvailableThemes);
        Assert.Contains(AppTheme.Dark, vm.AvailableThemes);
        Assert.Contains(AppTheme.System, vm.AvailableThemes);
    }
}

internal sealed class StubPreferencesService : IPreferencesService
{
    public AppTheme Theme { get; set; } = AppTheme.Dark;
    public string LastServerUrl { get; set; } = "";
    public string LastUsername { get; set; } = "";
    public List<string> RecentConnections { get; } = new();
    public bool SaveCalled { get; private set; }
    public void AddRecentConnection(string url) => RecentConnections.Add(url);
    public void Save() => SaveCalled = true;
    public void Load() { }
}
