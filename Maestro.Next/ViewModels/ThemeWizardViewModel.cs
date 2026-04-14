// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maestro.Next.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Maestro.Next.ViewModels;

/// <summary>
/// ViewModel for the Theme Generation Wizard.
/// Queries distinct values of a property and generates one style rule per value.
/// </summary>
public partial class ThemeWizardViewModel : ViewModelBase
{
    private readonly string _featureSourceId;
    private readonly string _className;

    public ThemeWizardViewModel(string featureSourceId, string className, IEnumerable<string> propertyNames)
    {
        _featureSourceId = featureSourceId;
        _className = className;
        foreach (var p in propertyNames)
            PropertyNames.Add(p);
        if (PropertyNames.Count > 0)
            SelectedPropertyName = PropertyNames[0];
    }

    public ObservableCollection<string> PropertyNames { get; } = new();

    [ObservableProperty] private string _selectedPropertyName = string.Empty;
    [ObservableProperty] private int _maxValues = 25;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _statusMessage = "Select a property and click 'Load Values'.";

    public ObservableCollection<ThemeValueItem> Values { get; } = new();

    /// <summary>True when the user clicks Generate</summary>
    public bool Confirmed { get; private set; }

    /// <summary>Generated filter/label pairs for each theme rule</summary>
    public List<ThemeRuleSpec> GeneratedRules { get; } = new();

    [RelayCommand]
    private async Task LoadValuesAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedPropertyName))
        {
            StatusMessage = "Please select a property.";
            return;
        }

        Values.Clear();
        try
        {
            IsLoading = true;
            StatusMessage = "Querying distinct values...";

            var conn = Program.Services!.GetRequiredService<IConnectionService>()
                              .CurrentConnection!;

            var reader = await Task.Run(() =>
                conn.FeatureService.QueryFeatureSource(
                    _featureSourceId,
                    _className,
                    null,        // no filter
                    new[] { $"UNIQUE({SelectedPropertyName})" }));

            int count = 0;
            while (reader.ReadNext() && count < MaxValues)
            {
                try
                {
                    var val = reader.IsNull(0) ? "(null)" : (reader[0]?.ToString() ?? "(null)");
                    Values.Add(new ThemeValueItem(val, GenerateColor(count)));
                    count++;
                }
                catch { break; }
            }
            reader.Close();

            StatusMessage = $"{Values.Count} distinct value(s) loaded.";
        }
        catch (Exception ex)
        {
            // Fallback: try aggregated DISTINCT via SQL-like approach
            StatusMessage = $"Could not query distinct values: {ex.Message}. Enter values manually.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Generate()
    {
        GeneratedRules.Clear();
        foreach (var v in Values.Where(v => v.IsSelected))
        {
            var filter = v.Value == "(null)"
                ? $"{SelectedPropertyName} NULL"
                : $"{SelectedPropertyName} = '{v.Value.Replace("'", "''")}'";
            GeneratedRules.Add(new ThemeRuleSpec(filter, v.Value, v.ColorHex));
        }
        Confirmed = true;
        StatusMessage = $"Generated {GeneratedRules.Count} rule(s).";
    }

    /// <summary>Generate a color from the HSL color wheel</summary>
    private static string GenerateColor(int index)
    {
        double hue = (index * 137.508) % 360; // golden angle
        var (r, g, b) = HslToRgb(hue, 0.65, 0.55);
        return $"ff{r:x2}{g:x2}{b:x2}";
    }

    private static (byte r, byte g, byte b) HslToRgb(double h, double s, double l)
    {
        double c = (1 - Math.Abs(2 * l - 1)) * s;
        double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
        double m = l - c / 2;
        double r, g, b;

        if (h < 60) { r = c; g = x; b = 0; }
        else if (h < 120) { r = x; g = c; b = 0; }
        else if (h < 180) { r = 0; g = c; b = x; }
        else if (h < 240) { r = 0; g = x; b = c; }
        else if (h < 300) { r = x; g = 0; b = c; }
        else { r = c; g = 0; b = x; }

        return ((byte)((r + m) * 255), (byte)((g + m) * 255), (byte)((b + m) * 255));
    }
}

public partial class ThemeValueItem : ViewModelBase
{
    public ThemeValueItem(string value, string colorHex)
    {
        Value = value;
        ColorHex = colorHex;
        IsSelected = true;
    }

    public string Value { get; }
    [ObservableProperty] private string _colorHex;
    [ObservableProperty] private bool _isSelected;
}

/// <summary>Specification for a single themed rule</summary>
public record ThemeRuleSpec(string Filter, string LegendLabel, string ColorHex);
