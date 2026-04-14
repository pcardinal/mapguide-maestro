// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Maestro.Next.ViewModels;
using Xunit;

namespace Maestro.Next.Tests.ViewModels;

public sealed class FindReplaceViewModelTests
{
    private static FindReplaceViewModel Create(ref string text)
    {
        var captured = text;
        return new FindReplaceViewModel(
            () => captured,
            t => captured = t);
    }

    [Fact]
    public void CountMatches_FindsOccurrences()
    {
        string text = "<Name>Paris</Name><Name>Lyon</Name>";
        var vm = new FindReplaceViewModel(() => text, t => text = t);
        vm.SearchText = "Name";

        vm.CountMatchesCommand.Execute(null);

        Assert.Equal(4, vm.MatchCount); // <Name> x2 + </Name> x2
    }

    [Fact]
    public void CountMatches_CaseInsensitive()
    {
        string text = "Hello hello HELLO";
        var vm = new FindReplaceViewModel(() => text, t => text = t);
        vm.SearchText = "hello";
        vm.CaseSensitive = false;

        vm.CountMatchesCommand.Execute(null);

        Assert.Equal(3, vm.MatchCount);
    }

    [Fact]
    public void CountMatches_CaseSensitive()
    {
        string text = "Hello hello HELLO";
        var vm = new FindReplaceViewModel(() => text, t => text = t);
        vm.SearchText = "hello";
        vm.CaseSensitive = true;

        vm.CountMatchesCommand.Execute(null);

        Assert.Equal(1, vm.MatchCount);
    }

    [Fact]
    public void CountMatches_Regex()
    {
        string text = "abc 123 def 456";
        var vm = new FindReplaceViewModel(() => text, t => text = t);
        vm.SearchText = @"\d+";
        vm.UseRegex = true;

        vm.CountMatchesCommand.Execute(null);

        Assert.Equal(2, vm.MatchCount);
    }

    [Fact]
    public void ReplaceAll_ReplacesText()
    {
        string text = "<old>value</old>";
        var vm = new FindReplaceViewModel(() => text, t => text = t);
        vm.SearchText = "old";
        vm.ReplaceText = "new";

        vm.ReplaceAllCommand.Execute(null);

        Assert.Equal("<new>value</new>", text);
    }

    [Fact]
    public void ReplaceAll_Regex()
    {
        string text = "width=\"100\" height=\"200\"";
        var vm = new FindReplaceViewModel(() => text, t => text = t);
        vm.SearchText = @"""(\d+)""";
        vm.ReplaceText = "\"0\"";
        vm.UseRegex = true;

        vm.ReplaceAllCommand.Execute(null);

        Assert.Equal("width=\"0\" height=\"0\"", text);
    }

    [Fact]
    public void EmptySearch_DoesNothing()
    {
        string text = "unchanged";
        var vm = new FindReplaceViewModel(() => text, t => text = t);
        vm.SearchText = "";

        vm.CountMatchesCommand.Execute(null);
        Assert.Equal(0, vm.MatchCount);

        vm.ReplaceAllCommand.Execute(null);
        Assert.Equal("unchanged", text);
    }
}
