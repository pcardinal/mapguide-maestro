# Build self-contained packages for multiple platforms
# Usage: .\publish.ps1 [-Runtime win-x64]

param(
    [string]$Runtime = ""
)

$Project = "Maestro.Next/Maestro.Next.csproj"
$OutputBase = "publish"
$Runtimes = @("win-x64", "linux-x64", "osx-x64", "osx-arm64")

if ($Runtime) {
    $Runtimes = @($Runtime)
}

foreach ($rid in $Runtimes) {
    Write-Host "`n📦 Publishing for $rid..." -ForegroundColor Cyan
    dotnet publish $Project `
        --configuration Release `
        --runtime $rid `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:PublishTrimmed=false `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        --output "$OutputBase/$rid"

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ $rid → $OutputBase/$rid/" -ForegroundColor Green
    } else {
        Write-Host "❌ $rid failed" -ForegroundColor Red
    }
}

Write-Host "`n🎉 All builds complete!" -ForegroundColor Yellow
Get-ChildItem "$OutputBase/*/Maestro.Next*" -ErrorAction SilentlyContinue |
    Format-Table Name, Length, Directory -AutoSize
