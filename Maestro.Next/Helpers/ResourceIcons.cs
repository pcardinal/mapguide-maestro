// Copyright (C) 2025, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro

namespace Maestro.Next.Helpers;

/// <summary>
/// Maps resource types to consistent Unicode/emoji icons.
/// Centralizes icon assignment for use across the application.
/// </summary>
public static class ResourceIcons
{
    public static string ForResourceType(string resourceType) => resourceType switch
    {
        "FeatureSource"         => "🗄️",
        "LayerDefinition"       => "🗺️",
        "MapDefinition"         => "🌍",
        "WebLayout"             => "🌐",
        "ApplicationDefinition" => "📱",
        "SymbolDefinition"      => "✦",
        "DrawingSource"         => "📐",
        "PrintLayout"           => "🖨️",
        "LoadProcedure"         => "📥",
        "WatermarkDefinition"   => "💧",
        "TileSetDefinition"     => "🧩",
        "Folder"                => "📁",
        _                       => "📄"
    };

    public static string ForEditorType(string iconKey) => iconKey switch
    {
        "FeatureSource"    => "🗄️",
        "LayerDefinition"  => "🗺️",
        "MapDefinition"    => "🌍",
        "WebLayout"        => "🌐",
        "Fusion"           => "📱",
        "SymbolDefinition" => "✦",
        "Xml"              => "📝",
        "Generic"          => "📄",
        _                  => "📄"
    };
}
