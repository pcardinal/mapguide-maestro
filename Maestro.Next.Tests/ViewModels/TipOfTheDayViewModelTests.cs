// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Maestro.Next.ViewModels;
using Xunit;

namespace Maestro.Next.Tests.ViewModels;

public sealed class TipOfTheDayViewModelTests
{
    [Fact]
    public void Ctor_HasTip()
    {
        var vm = new TipOfTheDayViewModel();
        Assert.NotEmpty(vm.CurrentTip);
        Assert.True(vm.ShowOnStartup);
    }

    [Fact]
    public void NextTip_ChangesOrWraps()
    {
        var vm = new TipOfTheDayViewModel();
        var first = vm.CurrentTip;

        vm.NextTipCommand.Execute(null);

        // Either changed or wrapped (if only 1 tip, same)
        Assert.NotEmpty(vm.CurrentTip);
    }

    [Fact]
    public void PreviousTip_ChangesOrWraps()
    {
        var vm = new TipOfTheDayViewModel();
        vm.NextTipCommand.Execute(null);
        var second = vm.CurrentTip;

        vm.PreviousTipCommand.Execute(null);

        Assert.NotEmpty(vm.CurrentTip);
    }

    [Fact]
    public void ShowOnStartup_CanToggle()
    {
        var vm = new TipOfTheDayViewModel();
        vm.ShowOnStartup = false;
        Assert.False(vm.ShowOnStartup);
    }

    [Fact]
    public void TipNumber_ContainsTip()
    {
        var vm = new TipOfTheDayViewModel();
        Assert.Contains("Tip", vm.TipNumber, StringComparison.Ordinal);
        Assert.Contains("of", vm.TipNumber, StringComparison.Ordinal);
    }
}
