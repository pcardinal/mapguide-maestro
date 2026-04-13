
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


## [2026-04-13 00:05] TASK-003: Upgrade Tier 2 core API layer

Status: Complete.

- **Verified**: ICSharpCode.Core.WinForms and LocalConfigure TFMs updated to net10.0-windows
- **Verified**: OSGeo.MapGuide.MaestroAPI (netstandard2.0) — no changes needed, builds cleanly
- **Verified**: All 3 Tier 2 projects build with 0 errors
- **Commits**: 4cd99b78: "TASK-003: Upgrade Tier 2 core API layer to net10.0"
- **Files Modified**: Thirdparty/SharpDevelop/ICSharpCode.Core.WinForms/ICSharpCode.Core.WinForms.csproj, LocalConfigure/LocalConfigure.csproj
- **Build Status**: Successful — 0 errors

Success - Tier 2 core API layer upgraded to net10.0/net10.0-windows.


## [2026-04-13 00:07] TASK-004: Upgrade Tier 3 services and utilities

Status: Complete.

- **Verified**: TFMs updated — Maestro.AddInManager, Maestro.Login, Maestro.Packaging, OSGeo.MapGuide.MaestroAPI.FxBridge → net10.0-windows; MgTileSeeder → net10.0
- **Verified**: System.Drawing.Common updated 6.0.0 → 10.0.5 in OSGeo.MapGuide.MaestroAPI.FxBridge
- **Verified**: All 5 Tier 3 regular projects build with 0 errors
- **Commits**: 00923f29: "TASK-004: Upgrade Tier 3 services and utilities to net10.0"
- **Files Modified**: Maestro.AddInManager.csproj, Maestro.Login.csproj, Maestro.Packaging.csproj, OSGeo.MapGuide.MaestroAPI.FxBridge.csproj, MgTileSeeder.csproj
- **Build Status**: Successful — 0 errors

Success - All Tier 3 services and utilities upgraded to net10.0/net10.0-windows.


## [2026-04-13 00:13] TASK-005: Remove OSGeo.MapGuide.MaestroAPI.Local provider

Status: Complete.

- **Verified**: OSGeo.MapGuide.MaestroAPI.Local removed from Maestro.sln via dotnet sln remove
- **Verified**: Maestro.AddIn.Local removed from Maestro.sln via dotnet sln remove
- **Verified**: No remaining GUID or path references to either removed project in Maestro.sln
- **Verified**: Maestro.csproj has no ProjectReference to either removed project (only LocalConfigure remains)
- **Verified**: MaestroAPITestRunner.csproj and MaestroAPITestRunner64.csproj (not in solution) reference removed projects but are excluded from build
- **Verified**: No mapguide-api-base-x64, mg-desktop-x64, mg-desktop-viewer-x64 references remain in active solution projects
- **Verified**: Build output contains zero errors referencing removed Local provider projects
- **Commits**: 632a4150: "TASK-005: Remove Local provider per Option B recommendation"
- **Files Modified**: Maestro.sln
- **Build Status**: NU1201 errors are pre-existing incremental-upgrade state (Maestro.csproj still on net6.0, resolved in later tasks) — no errors from removed projects

Success - Local provider (Option B) cleanly removed from solution.


## [2026-04-13 00:20] TASK-006: Upgrade Tier 3 test projects

Status: Complete.

- **Verified**: Both test projects updated — OSGeo.MapGuide.MaestroAPI.Tests and OSGeo.MapGuide.MaestroAPI.IntegrationTests → net10.0
- **Verified**: Both projects build with 0 errors
- **Tests**: OSGeo.MapGuide.MaestroAPI.Tests — 311/311 passed, 0 failures
- **Tests**: OSGeo.MapGuide.MaestroAPI.IntegrationTests — 54 skipped (local provider, expected), 60 failed at fixture init (no MapGuide server available — pre-existing behavior, not a regression from upgrade)
- **Errors Fixed**: Culture-sensitivity bug in TileSetInterfaces.cs (Convert.ToDouble → InvariantCulture), also fixed same pattern in FdoDataValue.cs, GdalRasterItem.cs, ExtensionMethods.cs, ManualScaleEditor.cs, VectorLayerDefinitionImpl.cs (6 versions), LegendPresenter.cs, HighlightColor.cs
- **Commits**: c5efd50a: "TASK-006: Upgrade Tier 3 test projects to net10.0"
- **Files Modified**: OSGeo.MapGuide.MaestroAPI.Tests.csproj, OSGeo.MapGuide.MaestroAPI.IntegrationTests.csproj, TileSetInterfaces.cs, FdoDataValue.cs, GdalRasterItem.cs, ExtensionMethods.cs, ManualScaleEditor.cs, 6x VectorLayerDefinitionImpl.cs, LegendPresenter.cs, HighlightColor.cs
- **Build Status**: Successful — 0 errors

Success - Tier 3 test projects upgraded; 311 unit tests pass; integration tests fail at fixture init (no server — pre-existing, not a regression).


## [2026-04-13 00:22] TASK-007: Upgrade Tier 4 viewer components

Status: Complete.

- **Verified**: Maestro.MapPublisher → net10.0; Maestro.MapViewer → net10.0-windows
- **Verified**: Both projects build with 0 errors
- **Commits**: 1504f319: "TASK-007: Upgrade Tier 4 viewer components to net10.0"
- **Files Modified**: Maestro.MapPublisher/Maestro.MapPublisher.csproj, Maestro.MapViewer/Maestro.MapViewer.csproj
- **Build Status**: Successful — 0 errors

Success - Tier 4 viewer components upgraded to net10.0/net10.0-windows.

