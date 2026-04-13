// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using CommunityToolkit.Mvvm.ComponentModel;

namespace Maestro.Next.Services;

public enum AppTheme { Light, Dark, System }

/// <summary>
/// Manages user preferences that persist across sessions
/// </summary>
public interface IPreferencesService
{
    AppTheme Theme { get; set; }
    string LastServerUrl { get; set; }
    string LastUsername { get; set; }

    void Save();
    void Load();
}

/// <summary>
/// Simple in-memory preferences (will later persist to JSON file)
/// </summary>
public class PreferencesService : ObservableObject, IPreferencesService
{
    private static readonly string _settingsPath =
        System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MaestroNext", "settings.json");

    private AppTheme _theme = AppTheme.Dark;
    public AppTheme Theme
    {
        get => _theme;
        set { _theme = value; OnPropertyChanged(); ApplyTheme(value); }
    }

    private string _lastServerUrl = "http://localhost/mapguide/mapagent/mapagent.fcgi";
    public string LastServerUrl
    {
        get => _lastServerUrl;
        set { _lastServerUrl = value; OnPropertyChanged(); }
    }

    private string _lastUsername = "Administrator";
    public string LastUsername
    {
        get => _lastUsername;
        set { _lastUsername = value; OnPropertyChanged(); }
    }

    public void Load()
    {
        try
        {
            if (!System.IO.File.Exists(_settingsPath)) return;
            var json = System.IO.File.ReadAllText(_settingsPath);
            var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("theme", out var t))
                _theme = Enum.TryParse<AppTheme>(t.GetString(), true, out var th) ? th : AppTheme.Dark;
            if (root.TryGetProperty("lastServerUrl", out var u))
                _lastServerUrl = u.GetString() ?? _lastServerUrl;
            if (root.TryGetProperty("lastUsername", out var n))
                _lastUsername = n.GetString() ?? _lastUsername;

            ApplyTheme(_theme);
        }
        catch { /* ignore corrupted settings */ }
    }

    public void Save()
    {
        try
        {
            var dir = System.IO.Path.GetDirectoryName(_settingsPath)!;
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            var json = System.Text.Json.JsonSerializer.Serialize(new
            {
                theme = _theme.ToString(),
                lastServerUrl = _lastServerUrl,
                lastUsername = _lastUsername,
            });
            System.IO.File.WriteAllText(_settingsPath, json);
        }
        catch { /* ignore write errors */ }
    }

    private static void ApplyTheme(AppTheme theme)
    {
        if (Avalonia.Application.Current is not { } app) return;

        app.RequestedThemeVariant = theme switch
        {
            AppTheme.Light  => Avalonia.Styling.ThemeVariant.Light,
            AppTheme.Dark   => Avalonia.Styling.ThemeVariant.Dark,
            _               => Avalonia.Styling.ThemeVariant.Default,
        };
    }
}
