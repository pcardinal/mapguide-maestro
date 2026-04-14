// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Maestro.Next.ViewModels;

namespace Maestro.Next.Views;

public partial class ExpressionBuilderWindow : Window
{
    public ExpressionBuilderWindow() => InitializeComponent();

    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (DataContext is ExpressionBuilderViewModel vm && !vm.IsLoaded)
            await vm.LoadCommand.ExecuteAsync(null);
    }

    private void OnPropertyDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is ListBox lb &&
            lb.SelectedItem is ExpressionPropertyItem prop &&
            DataContext is ExpressionBuilderViewModel vm)
        {
            vm.InsertPropertyCommand.Execute(prop);
        }
    }

    private void OnFunctionDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is ListBox lb &&
            lb.SelectedItem is ExpressionFunctionItem func &&
            DataContext is ExpressionBuilderViewModel vm)
        {
            vm.InsertFunctionCommand.Execute(func);
        }
    }

    private void OnOperatorClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn &&
            btn.Content is string op &&
            DataContext is ExpressionBuilderViewModel vm)
        {
            vm.InsertOperatorCommand.Execute(op);
        }
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e) => Close(null);

    private void OnOkClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ExpressionBuilderViewModel vm)
            Close(vm.ExpressionText);
    }
}
