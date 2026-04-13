
## [2026-04-12 23:57] TASK-001: Verify prerequisites

Status: Complete.

- **Verified**: .NET 10 SDK is available on the machine (Compatible SDK found); no global.json present, no constraints to validate.

Success - All prerequisites confirmed.


## [2026-04-13 00:04] TASK-002: Upgrade Tier 1 foundation libraries

Status: Complete.

- **Verified**: .NET 10 SDK present; no global.json constraints
- **Verified**: ICSharpCode.Core — Microsoft.Win32.Registry kept (needed for netstandard2.0 TFM; NuGet.0003 only applies when project itself targets net10.0)
- **Verified**: Newtonsoft.Json updated 13.0.3 → 13.0.4 in OSGeo.MapGuide.ObjectModels
- **Verified**: ICSharpCode.TextEditor, Maestro.Shared.UI, SignMapGuideApi TFMs updated to net10.0-windows
- **Verified**: MpuCalc — mapguide-api-base-x64 removed, replaced with ProjNet 2.0.0; Program.cs rewritten using ProjNet CoordinateSystemFactory; TFM updated to net10.0-windows
- **Verified**: Directory.Build.props created to globally suppress WFO1000 (new .NET 10 WinForms designer serialization diagnostic affecting all legacy WinForms projects)
- **Verified**: All 7 Tier 1 projects build with 0 errors
- **Commits**: a3ca3a25: "TASK-002: Upgrade Tier 1 foundation libraries to net10.0"
- **Files Modified**: Thirdparty/SharpDevelop/ICSharpCode.Core/ICSharpCode.Core.csproj, OSGeo.MapGuide.ObjectModels/OSGeo.MapGuide.ObjectModels.csproj, Thirdparty/SharpDevelop/ICSharpCode.TextEditor/ICSharpCode.TextEditor.csproj, Maestro.Shared.UI/Maestro.Shared.UI.csproj, SignMapGuideApi/SignMapGuideApi.csproj, MpuCalc/MpuCalc.csproj, MpuCalc/Program.cs
- **Files Created**: Directory.Build.props
- **Build Status**: Successful — all 7 Tier 1 projects: 0 errors

Success - All Tier 1 foundation libraries upgraded to net10.0/net10.0-windows.

