// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Maestro.Next.ViewModels;
using Xunit;

namespace Maestro.Next.Tests.ViewModels;

public class ValidationIssueViewModelTests
{
    [Theory]
    [InlineData("Error", "❌")]
    [InlineData("Warning", "⚠️")]
    [InlineData("Information", "ℹ️")]
    [InlineData("Unknown", "❔")]
    public void Icon_MatchesStatus(string status, string expectedIcon)
    {
        var vm = new ValidationIssueViewModel(status, "test message", "CODE001");
        Assert.Equal(expectedIcon, vm.Icon);
    }
}
