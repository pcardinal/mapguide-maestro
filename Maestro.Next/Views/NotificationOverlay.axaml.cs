// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using Avalonia.Controls;
using Maestro.Next.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Maestro.Next.Views;

public partial class NotificationOverlay : ItemsControl
{
    public NotificationOverlay()
    {
        InitializeComponent();
        DataContext = Program.Services?.GetRequiredService<INotificationService>();
    }
}
