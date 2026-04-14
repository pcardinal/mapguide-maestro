// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maestro.Next.Services;
using Microsoft.Extensions.DependencyInjection;
using OSGeo.MapGuide.MaestroAPI.Schema;

namespace Maestro.Next.ViewModels;

/// <summary>
/// ViewModel for the FDO Expression Builder dialog.
/// Provides property auto-completion, FDO function list, and expression text editing.
/// </summary>
public partial class ExpressionBuilderViewModel : ViewModelBase
{
    private readonly string _featureSourceId;
    private readonly string _className;

    public ExpressionBuilderViewModel(string featureSourceId, string className, string? initialExpression = null)
    {
        _featureSourceId = featureSourceId;
        _className = className;
        ExpressionText = initialExpression ?? string.Empty;
    }

    [ObservableProperty] private string _expressionText = string.Empty;
    [ObservableProperty] private bool _isLoaded;
    [ObservableProperty] private string? _validationResult;
    [ObservableProperty] private bool _isValid;

    /// <summary>True when the user clicks OK</summary>
    public bool Confirmed { get; private set; }

    // ── Property columns from the class definition ──────────────
    public ObservableCollection<ExpressionPropertyItem> Properties { get; } = new();

    // ── FDO functions from provider capabilities ────────────────
    public ObservableCollection<ExpressionFunctionItem> Functions { get; } = new();

    // ── Common operators ────────────────────────────────────────
    public static string[] Operators { get; } =
    [
        "=", "<>", "<", ">", "<=", ">=",
        "AND", "OR", "NOT", "LIKE", "IN",
        "NULL", "IS NULL", "IS NOT NULL",
        "+", "-", "*", "/", "||"
    ];

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            var conn = Program.Services!.GetRequiredService<IConnectionService>()
                              .CurrentConnection!;

            // Load class properties
            var classDef = await Task.Run(() =>
                conn.FeatureService.GetClassDefinition(_featureSourceId, _className));

            Properties.Clear();
            foreach (var p in classDef.Properties)
            {
                Properties.Add(new ExpressionPropertyItem(
                    p.Name,
                    p.Type.ToString(),
                    p is DataPropertyDefinition dpd ? dpd.DataType.ToString() : p.Type.ToString()));
            }

            // Load FDO functions from capabilities
            try
            {
                var caps = await Task.Run(() =>
                    conn.FeatureService.GetProviderCapabilities(
                        conn.ResourceService.GetResource(_featureSourceId)
                            is OSGeo.MapGuide.ObjectModels.FeatureSource.IFeatureSource fs
                            ? fs.Provider : string.Empty));

                Functions.Clear();
                if (caps?.Expression?.SupportedFunctions != null)
                {
                    foreach (var fn in caps.Expression.SupportedFunctions)
                    {
                        var args = fn.Signatures.Length > 0
                            ? string.Join(", ", fn.Signatures[0].Arguments.Select(a => a.Name))
                            : "";
                        Functions.Add(new ExpressionFunctionItem(
                            fn.Name, fn.Description ?? "", $"{fn.Name}({args})"));
                    }
                }
            }
            catch
            {
                // Capabilities may not be available for all providers
            }

            IsLoaded = true;
        }
        catch (Exception ex)
        {
            Program.Services!.GetRequiredService<INotificationService>()
                   .Error($"Failed to load expression data: {ex.Message}");
        }
    }

    [RelayCommand]
    private void InsertProperty(ExpressionPropertyItem prop)
    {
        ExpressionText += prop.Name;
    }

    [RelayCommand]
    private void InsertFunction(ExpressionFunctionItem func)
    {
        ExpressionText += func.Template;
    }

    [RelayCommand]
    private void InsertOperator(string op)
    {
        if (!ExpressionText.EndsWith(" ", StringComparison.Ordinal) && ExpressionText.Length > 0)
            ExpressionText += " ";
        ExpressionText += op + " ";
    }

    [RelayCommand]
    private void Confirm()
    {
        Confirmed = true;
    }

    [RelayCommand]
    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(ExpressionText))
        {
            ValidationResult = null;
            IsValid = false;
            return;
        }

        try
        {
            // Try parsing as a filter first
            OSGeo.FDO.Expressions.FdoFilter.Parse(ExpressionText);
            IsValid = true;
            ValidationResult = "✅ Valid filter expression.";
        }
        catch
        {
            try
            {
                // Try as an expression
                OSGeo.FDO.Expressions.FdoExpression.Parse(ExpressionText);
                IsValid = true;
                ValidationResult = "✅ Valid expression.";
            }
            catch (Exception ex)
            {
                IsValid = false;
                ValidationResult = $"❌ Syntax error: {ex.Message}";
            }
        }
    }
}

public record ExpressionPropertyItem(string Name, string PropertyType, string DataType)
{
    public string Icon => PropertyType switch
    {
        "Geometry" => "📐",
        "Data" => "📋",
        "Raster" => "🖼️",
        _ => "📋"
    };
}

public record ExpressionFunctionItem(string Name, string Description, string Template);
