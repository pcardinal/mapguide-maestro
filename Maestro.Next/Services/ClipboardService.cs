// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

namespace Maestro.Next.Services;

public enum ClipboardOperation { None, Copy, Cut }

/// <summary>
/// Manages the internal resource clipboard for Cut/Copy/Paste operations
/// </summary>
public interface IClipboardService
{
    string? ResourceId { get; }
    ClipboardOperation Operation { get; }
    bool HasContent { get; }

    void SetCopy(string resourceId);
    void SetCut(string resourceId);
    void Clear();
}

public class ClipboardService : IClipboardService
{
    public string? ResourceId { get; private set; }
    public ClipboardOperation Operation { get; private set; }
    public bool HasContent => ResourceId != null && Operation != ClipboardOperation.None;

    public void SetCopy(string resourceId)
    {
        ResourceId = resourceId;
        Operation = ClipboardOperation.Copy;
    }

    public void SetCut(string resourceId)
    {
        ResourceId = resourceId;
        Operation = ClipboardOperation.Cut;
    }

    public void Clear()
    {
        ResourceId = null;
        Operation = ClipboardOperation.None;
    }
}
