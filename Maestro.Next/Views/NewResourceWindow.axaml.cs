// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Avalonia.Controls;
using Avalonia.Interactivity;
using Maestro.Next.ViewModels;

namespace Maestro.Next.Views;

public partial class NewResourceWindow : Window
{
    public NewResourceWindow() => InitializeComponent();

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (DataContext is NewResourceViewModel vm)
        {
            vm.PropertyChanged += (_, args) =>
            {
                // Auto-close when creation succeeds
                if (args.PropertyName == nameof(NewResourceViewModel.Succeeded)
                    && vm.Succeeded)
                {
                    Close(vm.CreatedResourceId);
                }
            };
        }
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e) => Close(null);
}
