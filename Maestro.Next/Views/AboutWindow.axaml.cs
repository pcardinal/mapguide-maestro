// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Maestro.Next.Views;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        var tb = this.FindControl<TextBlock>("RuntimeTextBlock");
        if (tb != null)
            tb.Text = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();
}
