# MapGuide Maestro .NET 10 Upgrade Tasks

## Overview

This document tracks the execution of the MapGuide Maestro upgrade from .NET 6 to .NET 10 using a bottom-up (dependency-first) strategy. Foundation libraries will be upgraded first, progressing through 8 dependency tiers to the main application.

**Progress**: 6/13 tasks complete (46%) ![0%](https://progress-bar.xyz/46)

---

## Tasks

### [✓] TASK-001: Verify prerequisites *(Completed: 2026-04-13 03:57)*
**References**: Plan §Prerequisites

- [✓] (1) Verify .NET 10 SDK installed per Plan §Prerequisites
- [✓] (2) .NET 10 SDK version meets minimum requirements (**Verify**)

---

### [✓] TASK-002: Upgrade Tier 1 foundation libraries *(Completed: 2026-04-13 04:04)*
**References**: Plan §Tier 1, Plan §Package Update Reference

- [✓] (1) Remove `Microsoft.Win32.Registry` package reference from ICSharpCode.Core per Plan §Tier 1
- [✓] (2) Package reference removed (**Verify**)
- [✓] (3) Update `Newtonsoft.Json` from 13.0.3 to 13.0.4 in OSGeo.MapGuide.ObjectModels per Plan §Tier 1
- [✓] (4) Package updated (**Verify**)
- [✓] (5) Update target framework to `net10.0-windows` in ICSharpCode.TextEditor, Maestro.Shared.UI, SignMapGuideApi per Plan §Tier 1
- [✓] (6) Framework updated in all 3 projects (**Verify**)
- [✓] (7) Remove `TreeViewAdv` package reference from MpuCalc per Plan §Tier 1
- [✓] (8) Replace all `TreeViewAdv` control usages in MpuCalc with standard WinForms `TreeView` per Plan §Tier 1 (Option A - standard TreeView replacement)
- [✓] (9) All `TreeViewAdv` usages replaced (**Verify**)
- [✓] (10) Update target framework to `net10.0-windows` in MpuCalc
- [✓] (11) Framework updated (**Verify**)
- [✓] (12) Build all Tier 1 projects
- [✓] (13) All Tier 1 projects build with 0 errors (**Verify**)
- [✓] (14) Commit changes with message: "TASK-002: Upgrade Tier 1 foundation libraries to net10.0"

---

### [✓] TASK-003: Upgrade Tier 2 core API layer *(Completed: 2026-04-13 04:05)*
**References**: Plan §Tier 2

- [✓] (1) Update target framework to `net10.0-windows` in ICSharpCode.Core.WinForms and LocalConfigure per Plan §Tier 2
- [✓] (2) Framework updated in both projects (**Verify**)
- [✓] (3) Build all Tier 2 projects
- [✓] (4) All Tier 2 projects build with 0 errors (**Verify**)
- [✓] (5) Commit changes with message: "TASK-003: Upgrade Tier 2 core API layer to net10.0"

---

### [✓] TASK-004: Upgrade Tier 3 services and utilities *(Completed: 2026-04-13 04:07)*
**References**: Plan §Tier 3, Plan §Package Update Reference

- [✓] (1) Update target framework to `net10.0-windows` in Maestro.AddInManager, Maestro.Login, Maestro.Packaging, OSGeo.MapGuide.MaestroAPI.FxBridge per Plan §Tier 3
- [✓] (2) Update target framework to `net10.0` in MgTileSeeder per Plan §Tier 3
- [✓] (3) Framework updated in all 5 projects (**Verify**)
- [✓] (4) Update `System.Drawing.Common` from 6.0.0 to 10.0.5 in OSGeo.MapGuide.MaestroAPI.FxBridge per Plan §Tier 3
- [✓] (5) Package updated (**Verify**)
- [✓] (6) Build all Tier 3 regular projects (excluding Local provider and tests)
- [✓] (7) All Tier 3 regular projects build with 0 errors (**Verify**)
- [✓] (8) Commit changes with message: "TASK-004: Upgrade Tier 3 services and utilities to net10.0"

---

### [✓] TASK-005: Remove OSGeo.MapGuide.MaestroAPI.Local provider *(Completed: 2026-04-13 04:13)*
**References**: Plan §Special Case, Plan §Option B

- [✓] (1) Remove OSGeo.MapGuide.MaestroAPI.Local project from solution per Plan §Option B
- [✓] (2) Remove Maestro.AddIn.Local project from solution per Plan §Option B
- [✓] (3) Remove references to OSGeo.MapGuide.MaestroAPI.Local from Maestro.csproj
- [✓] (4) Remove references to Maestro.AddIn.Local from Maestro.csproj
- [✓] (5) Remove `mapguide-api-base-x64`, `mg-desktop-x64`, `mg-desktop-viewer-x64` package references per Plan §Package Update Reference
- [✓] (6) All removed project references and packages no longer present (**Verify**)
- [✓] (7) Solution builds cleanly without removed projects (**Verify**)
- [✓] (8) Commit changes with message: "TASK-005: Remove Local provider per Option B recommendation"

---

### [✓] TASK-006: Upgrade Tier 3 test projects *(Completed: 2026-04-13 04:20)*
**References**: Plan §Tier 3 Test Projects

- [✓] (1) Update target framework to `net10.0` in OSGeo.MapGuide.MaestroAPI.Tests and OSGeo.MapGuide.MaestroAPI.IntegrationTests per Plan §Tier 3
- [✓] (2) Framework updated in both test projects (**Verify**)
- [✓] (3) Build both test projects
- [✓] (4) Both test projects build with 0 errors (**Verify**)
- [✓] (5) Run OSGeo.MapGuide.MaestroAPI.Tests
- [✓] (6) All unit tests pass with 0 failures (**Verify**)
- [✓] (7) Run OSGeo.MapGuide.MaestroAPI.IntegrationTests
- [✓] (8) Integration tests pass or skip correctly when server unavailable (**Verify**)
- [✓] (9) Commit changes with message: "TASK-006: Upgrade Tier 3 test projects to net10.0"

---

### [ ] TASK-007: Upgrade Tier 4 viewer components
**References**: Plan §Tier 4

- [ ] (1) Update target framework to `net10.0` in Maestro.MapPublisher per Plan §Tier 4
- [ ] (2) Update target framework to `net10.0-windows` in Maestro.MapViewer per Plan §Tier 4
- [ ] (3) Framework updated in both projects (**Verify**)
- [ ] (4) Build both Tier 4 projects
- [ ] (5) Both Tier 4 projects build with 0 errors (**Verify**)
- [ ] (6) Commit changes with message: "TASK-007: Upgrade Tier 4 viewer components to net10.0"

---

### [ ] TASK-008: Replace TreeViewAdv in Maestro.Editors
**References**: Plan §Tier 5, Plan §Package Update Reference, Plan §Breaking Changes Catalog

- [ ] (1) Search Maestro.Editors project for all usages of `Aga.Controls.Tree` namespace and `TreeViewAdv` type
- [ ] (2) Replace all `TreeViewAdv` control instantiations with standard WinForms `TreeView` controls per Plan §Tier 5 (Option A)
- [ ] (3) Update affected `.Designer.cs` files in Maestro.Editors
- [ ] (4) Remove `TreeViewAdv` package reference from Maestro.Editors.csproj
- [ ] (5) Package reference removed (**Verify**)
- [ ] (6) Build Maestro.Editors project
- [ ] (7) Maestro.Editors builds with 0 errors (**Verify**)
- [ ] (8) Commit changes with message: "TASK-008: Replace TreeViewAdv with standard TreeView in Maestro.Editors"

---

### [ ] TASK-009: Upgrade Tier 5 Maestro.Editors framework
**References**: Plan §Tier 5, Plan §Package Update Reference

- [ ] (1) Update target framework to `net10.0-windows` in Maestro.Editors per Plan §Tier 5
- [ ] (2) Framework updated (**Verify**)
- [ ] (3) Update `System.Data.Odbc` from 6.0.0 to 10.0.5 in Maestro.Editors per Plan §Tier 5
- [ ] (4) Package updated (**Verify**)
- [ ] (5) Build Maestro.Editors project and fix any compilation errors per Plan §Breaking Changes Catalog
- [ ] (6) Maestro.Editors builds with 0 errors (**Verify**)
- [ ] (7) Run OSGeo.MapGuide.MaestroAPI.Tests to verify no regressions
- [ ] (8) All tests pass with 0 failures (**Verify**)
- [ ] (9) Commit changes with message: "TASK-009: Upgrade Maestro.Editors to net10.0-windows"

---

### [ ] TASK-010: Upgrade Tier 6 composite UI components
**References**: Plan §Tier 6

- [ ] (1) Update target framework to `net10.0-windows` in Maestro.LiveMapEditor, Maestro.Scripting.Core, MaestroFsPreview, RtMapInspector per Plan §Tier 6
- [ ] (2) Framework updated in all 4 projects (**Verify**)
- [ ] (3) Build all Tier 6 projects and fix any compilation errors per Plan §Breaking Changes Catalog
- [ ] (4) All Tier 6 projects build with 0 errors (**Verify**)
- [ ] (5) Commit changes with message: "TASK-010: Upgrade Tier 6 composite UI components to net10.0"

---

### [ ] TASK-011: Upgrade Tier 7 application shell
**References**: Plan §Tier 7, Plan §Breaking Changes Catalog

- [ ] (1) Search solution for `AppDomain.CreateDomain` usage in ICSharpCode.Core and Maestro.Base
- [ ] (2) If `AppDomain.CreateDomain` found, migrate to `AssemblyLoadContext` per Plan §Breaking Changes Catalog
- [ ] (3) No `AppDomain.CreateDomain` usage remains (**Verify**)
- [ ] (4) Update target framework to `net10.0-windows` in Maestro.Base per Plan §Tier 7
- [ ] (5) Framework updated (**Verify**)
- [ ] (6) Build Maestro.Base and fix any compilation errors
- [ ] (7) Maestro.Base builds with 0 errors (**Verify**)
- [ ] (8) Commit changes with message: "TASK-011: Upgrade Maestro.Base application shell to net10.0"

---

### [ ] TASK-012: Upgrade Tier 8 add-ins and main application
**References**: Plan §Tier 8, Plan §Package Update Reference

- [ ] (1) Update target framework to `net10.0-windows` in Maestro.AddIn.ExtendedObjectModels, Maestro.AddIn.FdoToolbox, Maestro.AddIn.Rest, Maestro.AddIn.Scripting, Maestro per Plan §Tier 8
- [ ] (2) Framework updated in all 5 projects (**Verify**)
- [ ] (3) Verify `RestSharp` 106.15.0 is compatible with net10.0 per Plan §Tier 8 (pin at 106.x)
- [ ] (4) RestSharp compatibility confirmed (**Verify**)
- [ ] (5) Build all Tier 8 projects and fix any compilation errors per Plan §Breaking Changes Catalog
- [ ] (6) All Tier 8 projects build with 0 errors (**Verify**)
- [ ] (7) Commit changes with message: "TASK-012: Upgrade Tier 8 add-ins and main application to net10.0"

---

### [ ] TASK-013: Final validation and testing
**References**: Plan §Testing & Validation Strategy, Plan §Success Criteria

- [ ] (1) Build entire solution
- [ ] (2) Solution builds with 0 errors and 0 warnings (**Verify**)
- [ ] (3) Run all unit tests in OSGeo.MapGuide.MaestroAPI.Tests
- [ ] (4) All unit tests pass with 0 failures (**Verify**)
- [ ] (5) Run all integration tests in OSGeo.MapGuide.MaestroAPI.IntegrationTests
- [ ] (6) Integration tests pass or skip correctly (**Verify**)
- [ ] (7) Commit final validation with message: "TASK-013: Complete .NET 10 upgrade - all tests passing"

---











