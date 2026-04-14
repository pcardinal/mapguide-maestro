#!/usr/bin/env bash
# Build self-contained packages for multiple platforms
# Usage: ./publish.sh [runtime]
# Examples:
#   ./publish.sh                    # Build for all platforms
#   ./publish.sh linux-x64          # Build for Linux x64 only

set -euo pipefail

PROJECT="Maestro.Next/Maestro.Next.csproj"
OUTPUT_BASE="publish"
RUNTIMES=("win-x64" "linux-x64" "osx-x64" "osx-arm64")

if [ $# -gt 0 ]; then
    RUNTIMES=("$1")
fi

for RID in "${RUNTIMES[@]}"; do
    echo "📦 Publishing for $RID..."
    dotnet publish "$PROJECT" \
        --configuration Release \
        --runtime "$RID" \
        --self-contained true \
        -p:PublishSingleFile=true \
        -p:PublishTrimmed=false \
        -p:IncludeNativeLibrariesForSelfExtract=true \
        --output "$OUTPUT_BASE/$RID"
    echo "✅ $RID → $OUTPUT_BASE/$RID/"
done

echo ""
echo "🎉 All builds complete!"
ls -la "$OUTPUT_BASE"/*/Maestro.Next* 2>/dev/null || echo "(Check publish folders)"
