// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Maestro.Next.Services;

public enum NotificationKind { Info, Success, Warning, Error }

public record Notification(string Message, NotificationKind Kind, DateTimeOffset Timestamp)
{
    public string Icon => Kind switch
    {
        NotificationKind.Success => "✅",
        NotificationKind.Warning => "⚠️",
        NotificationKind.Error   => "❌",
        _                        => "ℹ️",
    };
}

/// <summary>
/// Pushes toast notifications visible at the bottom of the workbench
/// </summary>
public interface INotificationService
{
    void Info(string message);
    void Success(string message);
    void Warning(string message);
    void Error(string message);

    ObservableCollection<Notification> Recent { get; }
}

public class NotificationService : ObservableObject, INotificationService
{
    private static readonly TimeSpan AutoDismiss = TimeSpan.FromSeconds(5);

    public ObservableCollection<Notification> Recent { get; } = new();

    public void Info(string message)    => Push(message, NotificationKind.Info);
    public void Success(string message) => Push(message, NotificationKind.Success);
    public void Warning(string message) => Push(message, NotificationKind.Warning);
    public void Error(string message)   => Push(message, NotificationKind.Error);

    private void Push(string message, NotificationKind kind)
    {
        var n = new Notification(message, kind, DateTimeOffset.Now);

        // Ensure UI thread
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            Recent.Add(n);

            // Auto-dismiss after 5 s
            _ = Task.Delay(AutoDismiss).ContinueWith(_ =>
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    Recent.Remove(n)));
        });
    }
}
