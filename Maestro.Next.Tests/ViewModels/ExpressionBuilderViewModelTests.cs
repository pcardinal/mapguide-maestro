// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Maestro.Next.ViewModels;
using Xunit;

namespace Maestro.Next.Tests.ViewModels;

public sealed class ExpressionBuilderViewModelTests
{
    [Fact]
    public void Ctor_SetsInitialExpression()
    {
        var vm = new ExpressionBuilderViewModel("Library://FS.FeatureSource", "SHP_Schema:Cities", "Name = 'Paris'");

        Assert.Equal("Name = 'Paris'", vm.ExpressionText);
        Assert.False(vm.IsLoaded);
        Assert.False(vm.Confirmed);
    }

    [Fact]
    public void Ctor_DefaultsToEmpty()
    {
        var vm = new ExpressionBuilderViewModel("Library://FS.FeatureSource", "Default:Points");

        Assert.Equal(string.Empty, vm.ExpressionText);
    }

    [Fact]
    public void InsertProperty_AppendsToExpression()
    {
        var vm = new ExpressionBuilderViewModel("Library://FS.FeatureSource", "S:C");
        vm.ExpressionText = "Name = ";

        var prop = new ExpressionPropertyItem("City", "Data", "String");
        vm.InsertPropertyCommand.Execute(prop);

        Assert.Equal("Name = City", vm.ExpressionText);
    }

    [Fact]
    public void InsertFunction_AppendsTemplate()
    {
        var vm = new ExpressionBuilderViewModel("Library://FS.FeatureSource", "S:C");

        var fn = new ExpressionFunctionItem("UPPER", "Uppercase", "UPPER(str)");
        vm.InsertFunctionCommand.Execute(fn);

        Assert.Equal("UPPER(str)", vm.ExpressionText);
    }

    [Fact]
    public void InsertOperator_AddsWithSpaces()
    {
        var vm = new ExpressionBuilderViewModel("Library://FS.FeatureSource", "S:C");
        vm.ExpressionText = "A";

        vm.InsertOperatorCommand.Execute("AND");

        Assert.Equal("A AND ", vm.ExpressionText);
    }

    [Fact]
    public void InsertOperator_OnEmpty_NoLeadingSpace()
    {
        var vm = new ExpressionBuilderViewModel("Library://FS.FeatureSource", "S:C");

        vm.InsertOperatorCommand.Execute("=");

        Assert.Equal("= ", vm.ExpressionText);
    }

    [Fact]
    public void Confirm_SetsConfirmedTrue()
    {
        var vm = new ExpressionBuilderViewModel("Library://FS.FeatureSource", "S:C");
        vm.ExpressionText = "test";

        vm.ConfirmCommand.Execute(null);

        Assert.True(vm.Confirmed);
    }

    [Fact]
    public void Operators_ContainsExpectedValues()
    {
        Assert.Contains("=", ExpressionBuilderViewModel.Operators);
        Assert.Contains("AND", ExpressionBuilderViewModel.Operators);
        Assert.Contains("LIKE", ExpressionBuilderViewModel.Operators);
        Assert.Contains("IS NULL", ExpressionBuilderViewModel.Operators);
        Assert.Contains("||", ExpressionBuilderViewModel.Operators);
    }

    [Fact]
    public void ExpressionPropertyItem_IconReflectsType()
    {
        var geom = new ExpressionPropertyItem("Shape", "Geometry", "Geometry");
        var data = new ExpressionPropertyItem("Name", "Data", "String");

        Assert.Equal("📐", geom.Icon);
        Assert.Equal("📋", data.Icon);
    }

    [Fact]
    public void Validate_ValidFilter_IsValid()
    {
        var vm = new ExpressionBuilderViewModel("Library://FS.FeatureSource", "S:C");
        vm.ExpressionText = "Name = 'Paris'";

        vm.ValidateCommand.Execute(null);

        Assert.True(vm.IsValid);
        Assert.Contains("Valid", vm.ValidationResult);
    }

    [Fact]
    public void Validate_ValidExpression_IsValid()
    {
        var vm = new ExpressionBuilderViewModel("Library://FS.FeatureSource", "S:C");
        vm.ExpressionText = "Name LIKE 'Paris%'";

        vm.ValidateCommand.Execute(null);

        Assert.True(vm.IsValid);
        Assert.Contains("Valid", vm.ValidationResult);
    }

    [Fact]
    public void Validate_InvalidSyntax_NotValid()
    {
        var vm = new ExpressionBuilderViewModel("Library://FS.FeatureSource", "S:C");
        vm.ExpressionText = "= = = INVALID {{{}}}";

        vm.ValidateCommand.Execute(null);

        Assert.False(vm.IsValid);
        Assert.Contains("error", vm.ValidationResult, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_Empty_NullResult()
    {
        var vm = new ExpressionBuilderViewModel("Library://FS.FeatureSource", "S:C");
        vm.ExpressionText = "";

        vm.ValidateCommand.Execute(null);

        Assert.Null(vm.ValidationResult);
        Assert.False(vm.IsValid);
    }
}
