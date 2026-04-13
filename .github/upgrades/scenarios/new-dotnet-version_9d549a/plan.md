# MapGuide Maestro — .NET 10 Upgrade Plan

> **Branch:** `upgrade-to-NET10` | **Strategy:** Bottom-Up (Dependency-First) | **Target:** `net10.0` / `net10.0-windows`

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Migration Strategy](#2-migration-strategy)
3. [Detailed Dependency Analysis](#3-detailed-dependency-analysis)
4. [Project-by-Project Plans](#4-project-by-project-plans)
   - [Tier 1 — Foundation Libraries (Level 0)](#tier-1--foundation-libraries-level-0)
   - [Tier 2 — Core API Layer (Level 1)](#tier-2--core-api-layer-level-1)
   - [Tier 3 — Services & Utilities (Level 2)](#tier-3--services--utilities-level-2)
   - [Tier 4 — Viewers (Level 3)](#tier-4--viewers-level-3)
   - [Tier 5 — Editors (Level 4)](#tier-5--editors-level-4)
   - [Tier 6 — Composite UI Components (Level 5)](#tier-6--composite-ui-components-level-5)
   - [Tier 7 — Application Shell (Level 6)](#tier-7--application-shell-level-6)
   - [Tier 8 — Add-ins & Main Application (Level 7)](#tier-8--add-ins--main-application-level-7)
   - [Special Case — OSGeo.MapGuide.MaestroAPI.Local (net48)](#special-case--osgeomap-guide-maestroapilocal-net48)
   - [Test Projects](#test-projects)
5. [Package Update Reference](#5-package-update-reference)
6. [Breaking Changes Catalog](#6-breaking-changes-catalog)
7. [Testing & Validation Strategy](#7-testing--validation-strategy)
8. [Risk Management](#8-risk-management)
9. [Complexity & Effort Assessment](#9-complexity--effort-assessment)
10. [Source Control Strategy](#10-source-control-strategy)
11. [Success Criteria](#11-success-criteria)

---

## 1. Executive Summary

### Scenario Description

MapGuide Maestro is a Windows Forms-based MapGuide authoring and administration tool. This plan covers the **final 6.0 release upgrade** of the current WinForms codebase to .NET 10 (LTS), establishing a stable, modern baseline before the platform is rebuilt from the ground up as a cross-platform tool using Avalonia. As stated in the project vision:

> *"This 6.0 final release represents the end of MapGuide Maestro in its current form as a Windows Forms based MapGuide authoring and administration tool … MapGuide Maestro will be rebuilt from the ground-up as a true cross platform MapGuide authoring and administration tool. It will be built on modern .NET (and all of its patterns and practices) and our ability to have a true cross-platform UI will be achieved through the use of Avalonia as our UI toolkit."*

### Scope

| Metric | Value |
|---|---|
| Total projects | 33 |
| Projects requiring framework upgrade | 28 |
| Projects already compatible (`netstandard2.0`) | 5 |
| Total issues | 39 (35 mandatory, 4 potential) |
| Incompatible NuGet packages (no replacement) | 4 |
| Packages requiring version upgrade | 3 |
| Dependency tiers | 8 (Level 0 → Level 7) |

### Current → Target Framework Matrix

| Current Framework | Projects | Target Framework |
|---|---|---|
| `netstandard2.0` | ICSharpCode.Core, OSGeo.MapGuide.ObjectModels, OSGeo.MapGuide.MaestroAPI, OSGeo.FDO.Expressions, Maestro.MapPublisher.Common | `netstandard2.0` (no change — compatible with .NET 10) |
| `net6.0-windows` | 22 WinForms/Windows projects | `net10.0-windows` |
| `net6.0` | Maestro.MapPublisher, MgTileSeeder, OSGeo.MapGuide.MaestroAPI.Tests, OSGeo.MapGuide.MaestroAPI.IntegrationTests | `net10.0` |
| `net48` | OSGeo.MapGuide.MaestroAPI.Local | ⚠️ **Special case — see below** |

### Critical Issues

> ⚠️ **OSGeo.MapGuide.MaestroAPI.Local** (currently `net48`) uses three native MapGuide packages (`mapguide-api-base-x64`, `mg-desktop-x64`, `mg-desktop-viewer-x64` v3.1.2.9484) that have **no .NET 10-compatible replacement**. This project cannot be straightforwardly migrated and requires a dedicated decision (see [Special Case section](#special-case--osgeomap-guide-maestroapilocal-net48)).

> ⚠️ **TreeViewAdv** v1.7.0 used in `Maestro.Editors` is **incompatible with .NET 10 and has no suggested replacement**. A UI control substitution is required.

### Selected Strategy

**Bottom-Up (Dependency-First), Incremental** — projects are migrated tier-by-tier, starting from leaf nodes with no internal dependencies and progressing up to the main application. Each tier is fully upgraded and validated before the next tier begins.

**Justification:**
- 33 projects with 8 dependency levels (depth > 4) → Complex classification
- WinForms codebase with deep inter-project coupling requires staged validation
- Multiple high-risk blockers (native packages, incompatible controls) benefit from early isolation
- Incremental approach keeps the solution buildable after each tier

---

## 2. Migration Strategy

### Approach: Incremental Bottom-Up

The solution is classified as **Complex** based on the following criteria:

| Criterion | Value | Threshold | Result |
|---|---|---|---|
| Project count | 33 | > 15 | ✅ Complex |
| Dependency depth | 8 tiers | > 4 | ✅ Complex |
| High-risk blockers | 2 (native pkgs, TreeViewAdv) | > 0 | ✅ Complex |
| Total LOC indicator | 936 files in Maestro.Editors alone | — | ✅ Complex |

### Bottom-Up Strategy Rationale

1. **Dependency safety**: Upgrading leaf nodes first ensures that when a higher-tier project is migrated, all its dependencies are already on .NET 10. No multi-targeting complexity.
2. **Early isolation of blockers**: The native-API blocker (`OSGeo.MapGuide.MaestroAPI.Local`) and the incompatible control blocker (`TreeViewAdv` in `Maestro.Editors`) are identified in early-to-mid tiers and can be resolved before the main application is touched.
3. **Continuous buildability**: After each tier, the partial solution remains buildable on .NET 10, with higher tiers temporarily still on .NET 6 (they compile fine against lower-tier `netstandard2.0` and `net10.0-windows` projects).
4. **Risk isolation**: Issues in `Maestro.Editors` (936 files, incompatible TreeViewAdv) are confined to Tier 5 and do not block Tiers 1–4.

### Phase Definitions

| Phase | Tiers | Theme |
|---|---|---|
| Phase A | Tier 1 | Foundation — leaf libraries & standalone apps |
| Phase B | Tier 2 | Core API layer |
| Phase C | Tier 3 | Services, utilities & test projects |
| Phase D | Tier 4 | Viewer components |
| Phase E | Tier 5 | Editors (highest complexity) |
| Phase F | Tier 6 | Composite UI components |
| Phase G | Tier 7 | Application shell |
| Phase H | Tier 8 | Add-ins & main application |
| Phase X | Special | OSGeo.MapGuide.MaestroAPI.Local (net48 blocker) |

### Parallel Execution Within Tiers

Projects within the same tier have **no dependencies on each other** and can be upgraded in parallel by separate developers. However, for a single-developer workflow they should be done sequentially within a single commit batch per tier.

### `netstandard2.0` Projects — No Framework Change Required

The five `netstandard2.0` projects (`OSGeo.MapGuide.ObjectModels`, `OSGeo.MapGuide.MaestroAPI`, `OSGeo.FDO.Expressions`, `Maestro.MapPublisher.Common`, `ICSharpCode.Core`) are **fully compatible with .NET 10** consumers without any framework change. Only package updates are needed where flagged.

---

## 3. Detailed Dependency Analysis

### Dependency Graph (Bottom-Up View)

```
Tier 8: [Maestro] [AddIn.Local] [AddIn.FdoToolbox] [AddIn.ExtendedObjectModels] [AddIn.Rest] [AddIn.Scripting]
              ↑               ↑              ↑                    ↑                     ↑               ↑
Tier 7: [Maestro.Base]────────────────────────────────────────────────────────────────────────────────────────
              ↑
Tier 6: [MaestroFsPreview] [RtMapInspector] [Maestro.LiveMapEditor] [Maestro.Scripting.Core]
              ↑                    ↑                  ↑                          ↑
Tier 5: [Maestro.Editors] ─────────────────────────────────────────────────────────────────
              ↑
Tier 4: [Maestro.MapViewer]  [Maestro.MapPublisher]
              ↑                       ↑
Tier 3: [OSGeo.MapGuide.MaestroAPI.FxBridge] [Maestro.Login] [Maestro.Packaging]
         [Maestro.MapPublisher.Common(*)] [MgTileSeeder] [Maestro.AddInManager]
         [OSGeo.MapGuide.MaestroAPI.Local(**)]
              ↑
Tier 2: [OSGeo.MapGuide.MaestroAPI(*)] [ICSharpCode.Core.WinForms] [LocalConfigure]
              ↑
Tier 1: [OSGeo.MapGuide.ObjectModels(*)] [ICSharpCode.Core(*)] [ICSharpCode.TextEditor]
         [Maestro.Shared.UI] [OSGeo.FDO.Expressions(*)]
         ── standalone ── [MpuCalc] [SignMapGuideApi]

(*) netstandard2.0 — no framework change needed
(**) net48 — special case, native bindings blocker
```

### Tier Assignments

#### Tier 1 — Foundation Libraries (Level 0 leaf nodes)

| Project | Framework | Dependency Count | Used By (count) |
|---|---|---|---|
| `ICSharpCode.Core` | `netstandard2.0` | 0 | 11 |
| `ICSharpCode.TextEditor` | `net6.0-windows` | 0 | 4 |
| `Maestro.Shared.UI` | `net6.0-windows` | 0 | 12 |
| `OSGeo.FDO.Expressions` | `netstandard2.0` | 0 | 3 |
| `OSGeo.MapGuide.ObjectModels` | `netstandard2.0` | 0 | 12 |
| `MpuCalc` *(standalone)* | `net6.0-windows` | 0 | 0 |
| `SignMapGuideApi` *(standalone)* | `net6.0-windows` | 0 | 0 |

#### Tier 2 — Core API Layer (Level 1)

| Project | Framework | Depends On |
|---|---|---|
| `ICSharpCode.Core.WinForms` | `net6.0-windows` | ICSharpCode.Core |
| `LocalConfigure` | `net6.0-windows` | ICSharpCode.Core |
| `OSGeo.MapGuide.MaestroAPI` | `netstandard2.0` | OSGeo.MapGuide.ObjectModels |

#### Tier 3 — Services & Utilities (Level 2)

| Project | Framework | Depends On |
|---|---|---|
| `Maestro.AddInManager` | `net6.0-windows` | ICSharpCode.Core.WinForms, ICSharpCode.Core |
| `Maestro.Login` | `net6.0-windows` | Maestro.Shared.UI, OSGeo.MapGuide.MaestroAPI |
| `Maestro.MapPublisher.Common` | `netstandard2.0` | OSGeo.MapGuide.MaestroAPI, OSGeo.FDO.Expressions |
| `Maestro.Packaging` | `net6.0-windows` | Maestro.Shared.UI, OSGeo.MapGuide.MaestroAPI |
| `MgTileSeeder` | `net6.0` | OSGeo.MapGuide.MaestroAPI |
| `OSGeo.MapGuide.MaestroAPI.FxBridge` | `net6.0-windows` | OSGeo.MapGuide.MaestroAPI |
| `OSGeo.MapGuide.MaestroAPI.Local` ⚠️ | `net48` | OSGeo.MapGuide.ObjectModels, OSGeo.MapGuide.MaestroAPI |
| `OSGeo.MapGuide.MaestroAPI.IntegrationTests` *(test)* | `net6.0` | OSGeo.MapGuide.ObjectModels, OSGeo.MapGuide.MaestroAPI |
| `OSGeo.MapGuide.MaestroAPI.Tests` *(test)* | `net6.0` | OSGeo.MapGuide.ObjectModels, OSGeo.MapGuide.MaestroAPI, OSGeo.FDO.Expressions |

#### Tier 4 — Viewers (Level 3)

| Project | Framework | Depends On |
|---|---|---|
| `Maestro.MapPublisher` | `net6.0` | OSGeo.MapGuide.ObjectModels, Maestro.MapPublisher.Common |
| `Maestro.MapViewer` | `net6.0-windows` | OSGeo.MapGuide.ObjectModels, OSGeo.MapGuide.MaestroAPI, OSGeo.MapGuide.MaestroAPI.FxBridge |

#### Tier 5 — Editors (Level 4) — HIGH COMPLEXITY

| Project | Framework | Depends On |
|---|---|---|
| `Maestro.Editors` | `net6.0-windows` | Maestro.Shared.UI, ICSharpCode.TextEditor, OSGeo.MapGuide.MaestroAPI, OSGeo.FDO.Expressions, Maestro.MapViewer, Maestro.Packaging |

#### Tier 6 — Composite UI Components (Level 5)

| Project | Framework | Depends On |
|---|---|---|
| `Maestro.LiveMapEditor` | `net6.0-windows` | OSGeo.MapGuide.MaestroAPI, Maestro.Login, Maestro.Editors |
| `Maestro.Scripting.Core` | `net6.0-windows` | ICSharpCode.Core, OSGeo.MapGuide.MaestroAPI, Maestro.Editors |
| `MaestroFsPreview` | `net6.0-windows` | Maestro.Shared.UI, OSGeo.MapGuide.MaestroAPI, Maestro.Login, Maestro.Editors |
| `RtMapInspector` | `net6.0-windows` | Maestro.Shared.UI, OSGeo.MapGuide.ObjectModels, ICSharpCode.TextEditor, OSGeo.MapGuide.MaestroAPI, OSGeo.MapGuide.MaestroAPI.FxBridge, Maestro.Login, Maestro.Editors, Maestro.MapViewer |

#### Tier 7 — Application Shell (Level 6)

| Project | Framework | Depends On |
|---|---|---|
| `Maestro.Base` | `net6.0-windows` | ICSharpCode.Core.WinForms, ICSharpCode.Core, Maestro.Shared.UI, OSGeo.MapGuide.MaestroAPI, MaestroFsPreview, Maestro.Login, Maestro.Editors |

#### Tier 8 — Add-ins & Main Application (Level 7)

| Project | Framework | Depends On |
|---|---|---|
| `Maestro.AddIn.ExtendedObjectModels` | `net6.0-windows` | Maestro.Base, ICSharpCode.Core, Maestro.Shared.UI, OSGeo.MapGuide.ObjectModels, OSGeo.MapGuide.MaestroAPI, Maestro.Editors |
| `Maestro.AddIn.FdoToolbox` | `net6.0-windows` | ICSharpCode.Core.WinForms, Maestro.Base, ICSharpCode.Core, Maestro.Shared.UI, OSGeo.MapGuide.MaestroAPI |
| `Maestro.AddIn.Local` ⚠️ | `net6.0-windows` | OSGeo.MapGuide.MaestroAPI.Local, Maestro.Base, ICSharpCode.Core, Maestro.Shared.UI, OSGeo.MapGuide.ObjectModels, OSGeo.MapGuide.MaestroAPI, Maestro.Editors |
| `Maestro.AddIn.Rest` | `net6.0-windows` | Maestro.Base, ICSharpCode.Core, Maestro.Shared.UI, ICSharpCode.TextEditor, OSGeo.MapGuide.MaestroAPI |
| `Maestro.AddIn.Scripting` | `net6.0-windows` | Maestro.Base, ICSharpCode.Core, Maestro.Shared.UI, Maestro.Scripting.Core, ICSharpCode.TextEditor, Maestro.Editors |
| `Maestro` *(main app)* | `net6.0-windows` | LocalConfigure, ICSharpCode.Core.WinForms, Maestro.Base, ICSharpCode.Core, Maestro.MapPublisher, Maestro.LiveMapEditor, Maestro.Shared.UI, Maestro.Scripting.Core, OSGeo.MapGuide.MaestroAPI, MaestroFsPreview, Maestro.Login, Maestro.Editors, RtMapInspector, MgTileSeeder |

### Critical Path

```
OSGeo.MapGuide.ObjectModels
  → OSGeo.MapGuide.MaestroAPI
      → Maestro.Packaging + Maestro.Login
          → Maestro.Editors  ← [TreeViewAdv BLOCKER]
              → Maestro.Base
                  → Maestro (main app)
```

### No Circular Dependencies

No circular dependencies were detected in the assessment. All 8 tiers have a strict, acyclic dependency order.

---

## 4. Project-by-Project Plans

### Tier 1 — Foundation Libraries (Level 0)

**Complexity:** Low-Medium | **Risk:** Low (except MpuCalc — incompatible package) | **Parallel execution:** Yes

---

#### `ICSharpCode.Core` — `Thirdparty\SharpDevelop\ICSharpCode.Core\ICSharpCode.Core.csproj`

**Current State**
- Framework: `netstandard2.0` → **no change** (compatible with .NET 10)
- Issues: 1 mandatory (`NuGet.0003` — `Microsoft.Win32.Registry` absorbed by .NET framework)
- Risk: Low

**Target State**
- Framework: `netstandard2.0` (unchanged)
- Remove `Microsoft.Win32.Registry` package reference (functionality built into .NET 10)

**Migration Steps**
1. **Prerequisites:** None — leaf node
2. **Framework Update:** None required
3. **Package Updates:**

| Package | Action | Reason |
|---|---|---|
| `Microsoft.Win32.Registry` 5.0.0 | **Remove** PackageReference | Absorbed by .NET framework (NuGet.0003) |

4. **Expected Breaking Changes:** Removing the explicit `Microsoft.Win32.Registry` package reference should be transparent since the type is available in-framework.
5. **Code Modifications:** None expected — API surface is identical.
6. **Testing:** Build + verify consumers compile.
7. **Validation Checklist:**
   - [ ] `ICSharpCode.Core` builds without errors
   - [ ] `ICSharpCode.Core` builds without warnings
   - [ ] `Microsoft.Win32.Registry` package reference removed
   - [ ] All 11 consumer projects still compile

---

#### `ICSharpCode.TextEditor` — `Thirdparty\SharpDevelop\ICSharpCode.TextEditor\ICSharpCode.TextEditor.csproj`

**Current State**
- Framework: `net6.0-windows`
- Issues: 1 mandatory (`Project.0002` — framework upgrade)
- Risk: Low

**Target State**
- Framework: `net10.0-windows`

**Migration Steps**
1. **Prerequisites:** ICSharpCode.Core upgraded (in same tier — process first)
2. **Framework Update:** Change `<TargetFramework>net6.0-windows</TargetFramework>` → `<TargetFramework>net10.0-windows</TargetFramework>`
3. **Package Updates:** None flagged
4. **Expected Breaking Changes:** Standard WinForms API changes between .NET 6 and .NET 10 may surface. Third-party SharpDevelop code may use deprecated drawing APIs.
5. **Code Modifications:** Review for `System.Drawing` usage — `System.Drawing.Common` must be upgraded (see Package Update Reference).
6. **Testing:** Build only (no tests in this project)
7. **Validation Checklist:**
   - [ ] `ICSharpCode.TextEditor` builds targeting `net10.0-windows`
   - [ ] No new compiler warnings
   - [ ] Downstream consumers (`Maestro.Editors`, `RtMapInspector`, `Maestro.AddIn.Scripting`, `Maestro.AddIn.Rest`) still build on net6.0

---

#### `Maestro.Shared.UI` — `Maestro.Shared.UI\Maestro.Shared.UI.csproj`

**Current State**
- Framework: `net6.0-windows`
- Issues: 1 mandatory (`Project.0002` — framework upgrade)
- Risk: Low

**Target State**
- Framework: `net10.0-windows`

**Migration Steps**
1. **Prerequisites:** None — leaf node
2. **Framework Update:** `net6.0-windows` → `net10.0-windows`
3. **Package Updates:** None flagged
4. **Expected Breaking Changes:** WinForms API compatibility generally high between .NET 6 and .NET 10.
5. **Code Modifications:** Scan for any deprecated WinForms APIs (e.g., obsolete `Control` properties). Check `System.Drawing.Common` usage.
6. **Testing:** Build; 12 consumer projects depend on this.
7. **Validation Checklist:**
   - [ ] Builds targeting `net10.0-windows`
   - [ ] No errors or warnings
   - [ ] 12 consumer projects continue to compile

---

#### `OSGeo.FDO.Expressions` — `OSGeo.FDO.Expressions\OSGeo.FDO.Expressions.csproj`

**Current State**
- Framework: `netstandard2.0` → **no change**
- Issues: 0
- Risk: None

**Target State**
- Framework: `netstandard2.0` (unchanged)
- No changes required

**Migration Steps**
- No action needed. Project is already compatible with .NET 10 consumers.

**Validation Checklist:**
- [ ] Confirm no issues surface after other projects upgrade

---

#### `OSGeo.MapGuide.ObjectModels` — `OSGeo.MapGuide.ObjectModels\OSGeo.MapGuide.ObjectModels.csproj`

**Current State**
- Framework: `netstandard2.0` → **no change**
- Issues: 1 potential (`NuGet.0002` — `Newtonsoft.Json` upgrade recommended)
- Risk: Low

**Target State**
- Framework: `netstandard2.0` (unchanged)
- `Newtonsoft.Json` updated: `13.0.3` → `13.0.4`

**Migration Steps**
1. **Prerequisites:** None — leaf node
2. **Framework Update:** None required
3. **Package Updates:**

| Package | Current | Target | Reason |
|---|---|---|---|
| `Newtonsoft.Json` | 13.0.3 | 13.0.4 | Recommended upgrade (NuGet.0002) |

4. **Expected Breaking Changes:** `Newtonsoft.Json` 13.0.3 → 13.0.4 is a patch update; no breaking changes expected.
5. **Code Modifications:** None expected.
6. **Validation Checklist:**
   - [ ] Builds without errors
   - [ ] `Newtonsoft.Json` updated to 13.0.4
   - [ ] 12 consumer projects unaffected

---

#### `MpuCalc` — `MpuCalc\MpuCalc.csproj` *(Standalone app)*

**Current State**
- Framework: `net6.0-windows`
- Issues: 2 mandatory (`NuGet.0001` — incompatible package, `Project.0002` — framework upgrade)
- Incompatible Package: `TreeViewAdv` v1.7.0 (no replacement suggested)
- Risk: **Medium** — TreeViewAdv replacement required

**Target State**
- Framework: `net10.0-windows`
- `TreeViewAdv` removed/replaced

**Migration Steps**
1. **Prerequisites:** None — standalone app
2. **Framework Update:** `net6.0-windows` → `net10.0-windows`
3. **Package Updates:**

| Package | Action | Reason |
|---|---|---|
| `TreeViewAdv` 1.7.0 | **Remove + Replace** | Incompatible, no .NET 10 version available |

4. **Expected Breaking Changes:**
   - `TreeViewAdv` removal: All usages of `Aga.Controls.Tree.TreeViewAdv`, `TreeModel`, `NodeTextBox` etc. must be replaced.
   - Replacement options: (a) .NET 10 native `TreeView` (WinForms built-in), (b) a community fork of TreeViewAdv targeting .NET 10, (c) remove advanced tree features if not critical.
5. **Code Modifications:**
   - Identify all files using `TreeViewAdv` in `MpuCalc`
   - Replace tree control instantiation and data-binding patterns
   - Update namespace imports (`using Aga.Controls.Tree` → standard WinForms)
6. **Testing:** Manual UI smoke test — tree view renders and behaves correctly.
7. **Validation Checklist:**
   - [ ] Builds targeting `net10.0-windows`
   - [ ] `TreeViewAdv` reference removed
   - [ ] Tree view functionality verified manually
   - [ ] No compiler warnings

---

#### `SignMapGuideApi` — `SignMapGuideApi\SignMapGuideApi.csproj` *(Standalone app)*

**Current State**
- Framework: `net6.0-windows`
- Issues: 1 mandatory (`Project.0002`)
- Risk: Low

**Target State**
- Framework: `net10.0-windows`

**Migration Steps**
1. **Framework Update:** `net6.0-windows` → `net10.0-windows`
2. **Package Updates:** None flagged
3. **Expected Breaking Changes:** Minimal — utility tool.
4. **Validation Checklist:**
   - [ ] Builds targeting `net10.0-windows`
   - [ ] Tool executes without runtime errors

---

### Tier 2 — Core API Layer (Level 1)

**Complexity:** Low | **Risk:** Low | **Prerequisite:** Tier 1 complete

---

#### `ICSharpCode.Core.WinForms` — `Thirdparty\SharpDevelop\ICSharpCode.Core.WinForms\ICSharpCode.Core.WinForms.csproj`

**Current State**
- Framework: `net6.0-windows`
- Issues: 1 mandatory (`Project.0002`)
- Risk: Low

**Target State**
- Framework: `net10.0-windows`

**Migration Steps**
1. **Prerequisites:** `ICSharpCode.Core` (Tier 1) complete
2. **Framework Update:** `net6.0-windows` → `net10.0-windows`
3. **Package Updates:** None flagged
4. **Expected Breaking Changes:** WinForms APIs used in SharpDevelop integration layer may include minor obsolete methods between .NET 6 and .NET 10. Inspect for `Application.ThreadException`, `Control.Invoke` patterns.
5. **Code Modifications:** Address any compiler warnings about obsolete WinForms APIs.
6. **Validation Checklist:**
   - [ ] Builds targeting `net10.0-windows`
   - [ ] No warnings
   - [ ] 4 consumer projects (Maestro.Base, Maestro.AddIn.FdoToolbox, Maestro.AddInManager, Maestro) still compile

---

#### `LocalConfigure` — `LocalConfigure\LocalConfigure.csproj`

**Current State**
- Framework: `net6.0-windows`
- Issues: 1 mandatory (`Project.0002`)
- Risk: Low

**Target State**
- Framework: `net10.0-windows`

**Migration Steps**
1. **Prerequisites:** `ICSharpCode.Core` (Tier 1) complete
2. **Framework Update:** `net6.0-windows` → `net10.0-windows`
3. **Package Updates:** None flagged
4. **Expected Breaking Changes:** Minimal — configuration utility.
5. **Validation Checklist:**
   - [ ] Builds targeting `net10.0-windows`
   - [ ] `Maestro` (main app) still compiles

---

#### `OSGeo.MapGuide.MaestroAPI` — `OSGeo.MapGuide.MaestroAPI\OSGeo.MapGuide.MaestroAPI.csproj`

**Current State**
- Framework: `netstandard2.0` → **no change**
- Issues: 0 — no changes required
- Risk: None
- Used by: 20 projects (the most-consumed project in the solution)

**Target State**
- Framework: `netstandard2.0` (unchanged)
- No changes required

**Migration Steps**
- No action needed. This is the core API library; it is already .NET 10 compatible via `netstandard2.0`.

**Validation Checklist:**
- [ ] Confirm all 20 consumer projects compile successfully after their own upgrades

---

### Tier 3 — Services & Utilities (Level 2)

**Complexity:** Medium (due to `OSGeo.MapGuide.MaestroAPI.Local` blocker) | **Risk:** High for Local provider, Low for others | **Prerequisite:** Tier 2 complete

---

#### `Maestro.AddInManager` — `Maestro.AddInManager\Maestro.AddInManager.csproj`

**Current State**
- Framework: `net6.0-windows`
- Issues: 1 mandatory (`Project.0002`)
- Risk: Low

**Target State**
- Framework: `net10.0-windows`

**Migration Steps**
1. **Prerequisites:** `ICSharpCode.Core.WinForms` (Tier 2) complete
2. **Framework Update:** `net6.0-windows` → `net10.0-windows`
3. **Package Updates:** None flagged
4. **Expected Breaking Changes:** Minimal — add-in manager UI.
5. **Validation Checklist:**
   - [ ] Builds targeting `net10.0-windows`
   - [ ] No errors or warnings

---

#### `Maestro.Login` — `Maestro.Login\Maestro.Login.csproj`

**Current State**
- Framework: `net6.0-windows`
- Issues: 1 mandatory (`Project.0002`)
- Risk: Low

**Target State**
- Framework: `net10.0-windows`

**Migration Steps**
1. **Prerequisites:** `Maestro.Shared.UI`, `OSGeo.MapGuide.MaestroAPI` (Tiers 1–2) complete
2. **Framework Update:** `net6.0-windows` → `net10.0-windows`
3. **Package Updates:** None flagged
4. **Expected Breaking Changes:** Login dialog WinForms patterns — minimal impact.
5. **Validation Checklist:**
   - [ ] Builds targeting `net10.0-windows`
   - [ ] 5 consumer projects compile: `Maestro.Base`, `MaestroFsPreview`, `RtMapInspector`, `Maestro.LiveMapEditor`, `Maestro`

---

#### `Maestro.MapPublisher.Common` — `Maestro.MapPublisher.Common\Maestro.MapPublisher.Common.csproj`

**Current State**
- Framework: `netstandard2.0` → **no change**
- Issues: 0
- Risk: None

**Target State**
- Framework: `netstandard2.0` (unchanged)
- No changes required

**Migration Steps**
- No action needed.

---

#### `Maestro.Packaging` — `Maestro.Packaging\Maestro.Packaging.csproj`

**Current State**
- Framework: `net6.0-windows`
- Issues: 1 mandatory (`Project.0002`)
- Risk: Low

**Target State**
- Framework: `net10.0-windows`

**Migration Steps**
1. **Prerequisites:** `Maestro.Shared.UI`, `OSGeo.MapGuide.MaestroAPI` complete
2. **Framework Update:** `net6.0-windows` → `net10.0-windows`
3. **Package Updates:** None flagged
4. **Expected Breaking Changes:** Packaging ZIP/file operations — check `SharpZipLib` compatibility (currently v1.4.2, confirmed compatible).
5. **Validation Checklist:**
   - [ ] Builds targeting `net10.0-windows`
   - [ ] `Maestro.Editors` (Tier 5 consumer) still compiles

---

#### `MgTileSeeder` — `MgTileSeeder\MgTileSeeder.csproj`

**Current State**
- Framework: `net6.0`
- Issues: 1 mandatory (`Project.0002`)
- Risk: Low

**Target State**
- Framework: `net10.0`

**Migration Steps**
1. **Prerequisites:** `OSGeo.MapGuide.MaestroAPI` complete
2. **Framework Update:** `net6.0` → `net10.0`
3. **Package Updates:** None flagged
4. **Expected Breaking Changes:** CLI tool — check `CommandLineParser` usage (v2.9.1, confirmed compatible).
5. **Validation Checklist:**
   - [ ] Builds targeting `net10.0`
   - [ ] CLI tool runs and processes arguments correctly

---

#### `OSGeo.MapGuide.MaestroAPI.FxBridge` — `OSGeo.MapGuide.MaestroAPI.FxBridge\OSGeo.MapGuide.MaestroAPI.FxBridge.csproj`

**Current State**
- Framework: `net6.0-windows`
- Issues: 2 (`NuGet.0002` — package upgrade recommended, `Project.0002`)
- Risk: Low

**Target State**
- Framework: `net10.0-windows`
- `System.Drawing.Common`: `6.0.0` → `10.0.5`

**Migration Steps**
1. **Prerequisites:** `OSGeo.MapGuide.MaestroAPI` complete
2. **Framework Update:** `net6.0-windows` → `net10.0-windows`
3. **Package Updates:**

| Package | Current | Target | Reason |
|---|---|---|---|
| `System.Drawing.Common` | 6.0.0 | 10.0.5 | Recommended upgrade (NuGet.0002) |

4. **Expected Breaking Changes:** `System.Drawing.Common` 10.0.5 — cross-platform image support removed for non-Windows in v6+; on Windows this is a non-breaking update. Confirm all `System.Drawing` types are Windows-only (the `-windows` TFM already enforces this).
5. **Validation Checklist:**
   - [ ] Builds targeting `net10.0-windows`
   - [ ] `System.Drawing.Common` updated to 10.0.5
   - [ ] `Maestro.MapViewer` and `RtMapInspector` compile

---

#### `OSGeo.MapGuide.MaestroAPI.Local` — `OSGeo.MapGuide.MaestroAPI.Local\OSGeo.MapGuide.MaestroAPI.Local.csproj` ⚠️ BLOCKER

> **See dedicated section: [Special Case — OSGeo.MapGuide.MaestroAPI.Local](#special-case--osgeomap-guide-maestroapilocal-net48)**
> This project cannot be upgraded to .NET 10 without resolving the native package dependency. It is handled as a separate special case and does **not** block the rest of Tier 3.

---

#### `OSGeo.MapGuide.MaestroAPI.IntegrationTests` *(Test project)* — `OSGeo.MapGuide.MaestroAPI.IntegrationTests\OSGeo.MapGuide.MaestroAPI.IntegrationTests.csproj`

> Handled in [Test Projects](#test-projects) section.

---

#### `OSGeo.MapGuide.MaestroAPI.Tests` *(Test project)* — `OSGeo.MapGuide.MaestroAPI.Tests\OSGeo.MapGuide.MaestroAPI.Tests.csproj`

> Handled in [Test Projects](#test-projects) section.

---

### Tier 4 — Viewers (Level 3)

**Complexity:** Low-Medium | **Risk:** Low | **Prerequisite:** Tier 3 complete (except OSGeo.MapGuide.MaestroAPI.Local)

---

#### `Maestro.MapPublisher` — `Maestro.MapPublisher\Maestro.MapPublisher.csproj`

**Current State**
- Framework: `net6.0`
- Issues: 1 mandatory (`Project.0002`)
- Risk: Low

**Target State**
- Framework: `net10.0`

**Migration Steps**
1. **Prerequisites:** `OSGeo.MapGuide.ObjectModels`, `Maestro.MapPublisher.Common` complete
2. **Framework Update:** `net6.0` → `net10.0`
3. **Package Updates:** None flagged
4. **Expected Breaking Changes:** Map publishing pipeline — check `RazorEngine.NetCore` v3.1.0 (confirmed compatible) and `Polly` v8.4.1 (confirmed compatible).
5. **Validation Checklist:**
   - [ ] Builds targeting `net10.0`
   - [ ] `Maestro` main app still compiles

---

#### `Maestro.MapViewer` — `Maestro.MapViewer\Maestro.MapViewer.csproj`

**Current State**
- Framework: `net6.0-windows`
- Issues: 1 mandatory (`Project.0002`)
- Risk: Low

**Target State**
- Framework: `net10.0-windows`

**Migration Steps**
1. **Prerequisites:** `OSGeo.MapGuide.ObjectModels`, `OSGeo.MapGuide.MaestroAPI`, `OSGeo.MapGuide.MaestroAPI.FxBridge` complete
2. **Framework Update:** `net6.0-windows` → `net10.0-windows`
3. **Package Updates:** None flagged
4. **Expected Breaking Changes:** WinForms map viewer — check painting/GDI+ code using `System.Drawing`. Update to use `System.Drawing.Common` 10.0.5 patterns.
5. **Code Modifications:** Inspect custom paint event handlers for deprecated GDI+ patterns.
6. **Validation Checklist:**
   - [ ] Builds targeting `net10.0-windows`
   - [ ] Map viewer renders correctly (smoke test)
   - [ ] `Maestro.Editors` and `RtMapInspector` compile

---

### Tier 5 — Editors (Level 4)

**Complexity:** HIGH | **Risk:** HIGH | **Prerequisite:** Tier 4 complete

> ⚠️ **This is the most complex project in the solution** — 936 files, incompatible `TreeViewAdv` control, large WinForms editor surface. Allocate significant review effort.

---

#### `Maestro.Editors` — `Maestro.Editors\Maestro.Editors.csproj`

**Current State**
- Framework: `net6.0-windows`
- Files: **936**
- Issues: 3 (`NuGet.0001` — TreeViewAdv incompatible, `NuGet.0002` — System.Data.Odbc upgrade, `Project.0002`)
- Risk: **High**

**Target State**
- Framework: `net10.0-windows`
- `TreeViewAdv` removed and replaced
- `System.Data.Odbc`: `6.0.0` → `10.0.5`

**Migration Steps**
1. **Prerequisites:** All of Tier 4 complete: `Maestro.Shared.UI`, `ICSharpCode.TextEditor`, `OSGeo.MapGuide.MaestroAPI`, `OSGeo.FDO.Expressions`, `Maestro.MapViewer`, `Maestro.Packaging`
2. **Framework Update:** `net6.0-windows` → `net10.0-windows`
3. **Package Updates:**

| Package | Current | Target | Reason |
|---|---|---|---|
| `TreeViewAdv` | 1.7.0 | **Remove** | Incompatible, no .NET 10 replacement (NuGet.0001) |
| `System.Data.Odbc` | 6.0.0 | 10.0.5 | Recommended upgrade (NuGet.0002) |

4. **Expected Breaking Changes:**

   **TreeViewAdv Removal (Critical):**
   - `TreeViewAdv` (`Aga.Controls.Tree`) is used throughout the Editors project for resource tree browsing, layer tree panels, and advanced node selection.
   - All `TreeViewAdv`-based controls must be replaced. Options:
     - **Option A (Recommended):** Replace with standard WinForms `TreeView` — available natively on .NET 10, no additional package required. Requires reimplementing data-binding patterns.
     - **Option B:** Search for a community fork of `TreeViewAdv` compatible with .NET 10 (e.g., check GitHub for `ObjectListView` or `AdvancedTreeView` forks).
     - **Option C:** Since this is the "final 6.0 release" before Avalonia rebuild, a minimal replacement strategy (standard `TreeView` with limited feature parity) may be acceptable.

   **System.Data.Odbc Update:**
   - Patch-level upgrade; no breaking changes expected.

   **WinForms .NET 6 → .NET 10 differences:**
   - `ToolStripMenuItem` and `ContextMenuStrip` rendering may change.
   - High-DPI scaling improvements in .NET 10 WinForms may alter layout.
   - `DataGridView` improvements — verify editor grids behave correctly.

5. **Code Modifications:**
   - **Phase 5a — TreeViewAdv audit:** Run a solution-wide search for `using Aga.Controls` and `TreeViewAdv` to enumerate all usage points.
   - **Phase 5b — Control replacement:** Replace each `TreeViewAdv` with chosen alternative. Update `.Designer.cs` files for affected Forms.
   - **Phase 5c — Package cleanup:** Remove the `TreeViewAdv` `PackageReference` from project file.
   - **Phase 5d — ODBC update:** Update `System.Data.Odbc` to 10.0.5.
   - **Phase 5e — WinForms review:** Scan for deprecated APIs; use build warnings as a guide.

6. **Testing Strategy:**
   - Build without errors (mandatory gate)
   - Open each major editor panel (Layer editor, Map editor, Feature source editor, Print layout editor) as smoke test
   - Verify tree-based navigation panels render and respond to interaction
   - Run `OSGeo.MapGuide.MaestroAPI.Tests` to catch API regressions

7. **Validation Checklist:**
   - [ ] Builds targeting `net10.0-windows` with zero errors
   - [ ] `TreeViewAdv` package reference removed
   - [ ] All former `TreeViewAdv` usages replaced
   - [ ] `System.Data.Odbc` updated to 10.0.5
   - [ ] Major editor panels open without runtime exceptions
   - [ ] No compiler warnings from deprecated API usage
   - [ ] 8 consumer projects compile: `Maestro.Base`, `MaestroFsPreview`, `Maestro.AddIn.ExtendedObjectModels`, `Maestro.AddIn.Local`, `RtMapInspector`, `Maestro.AddIn.Scripting`, `Maestro.LiveMapEditor`, `Maestro.Scripting.Core`

---

### Tier 6 — Composite UI Components (Level 5)

**Complexity:** Medium | **Risk:** Medium | **Prerequisite:** Tier 5 (`Maestro.Editors`) complete

---

#### `Maestro.LiveMapEditor` — `Maestro.LiveMapEditor\Maestro.LiveMapEditor.csproj`

**Current State**
- Framework: `net6.0-windows`
- Issues: 1 mandatory (`Project.0002`)
- Risk: Low-Medium

**Target State**
- Framework: `net10.0-windows`

**Migration Steps**
1. **Prerequisites:** `OSGeo.MapGuide.MaestroAPI`, `Maestro.Login`, `Maestro.Editors` complete
2. **Framework Update:** `net6.0-windows` → `net10.0-windows`
3. **Package Updates:** None flagged
4. **Expected Breaking Changes:** Live map editor WinForms panel — inherits complexity from `Maestro.Editors` changes. Verify integration points with replaced TreeViewAdv components.
5. **Validation Checklist:**
   - [ ] Builds targeting `net10.0-windows`
   - [ ] Live map editor opens in application without runtime errors

---

#### `Maestro.Scripting.Core` — `Maestro.Scripting.Core\Maestro.Scripting.Core.csproj`

**Current State**
- Framework: `net6.0-windows`
- Issues: 1 mandatory (`Project.0002`)
- Risk: Low-Medium

**Target State**
- Framework: `net10.0-windows`

**Migration Steps**
1. **Prerequisites:** `ICSharpCode.Core`, `OSGeo.MapGuide.MaestroAPI`, `Maestro.Editors` complete
2. **Framework Update:** `net6.0-windows` → `net10.0-windows`
3. **Package Updates:** None flagged (IronPython 2.7.11 confirmed compatible)
4. **Expected Breaking Changes:** IronPython scripting host integration — verify `IronPython` v2.7.11 works on .NET 10. Note: IronPython 2.x may have limited .NET 10 compatibility; verify scripting engine initializes correctly.
   - ⚠️ **Requires validation:** IronPython 2.7.11 runtime on .NET 10 — run a basic script execution test.
5. **Validation Checklist:**
   - [ ] Builds targeting `net10.0-windows`
   - [ ] IronPython scripting engine initializes without errors
   - [ ] A sample script executes successfully

---

#### `MaestroFsPreview` — `MaestroFsPreview\MaestroFsPreview.csproj`

**Current State**
- Framework: `net6.0-windows`
- Issues: 1 mandatory (`Project.0002`)
- Risk: Low

**Target State**
- Framework: `net10.0-windows`

**Migration Steps**
1. **Prerequisites:** `Maestro.Shared.UI`, `OSGeo.MapGuide.MaestroAPI`, `Maestro.Login`, `Maestro.Editors` complete
2. **Framework Update:** `net6.0-windows` → `net10.0-windows`
3. **Package Updates:** None flagged
4. **Expected Breaking Changes:** Preview tool — minimal changes expected.
5. **Validation Checklist:**
   - [ ] Builds targeting `net10.0-windows`
   - [ ] Preview window opens and renders feature source data

---

#### `RtMapInspector` — `RtMapInspector\RtMapInspector.csproj`

**Current State**
- Framework: `net6.0-windows`
- Issues: 1 mandatory (`Project.0002`)
- Risk: Low-Medium (complex dependencies — 8 direct dependencies)

**Target State**
- Framework: `net10.0-windows`

**Migration Steps**
1. **Prerequisites:** All 8 dependencies complete: `Maestro.Shared.UI`, `OSGeo.MapGuide.ObjectModels`, `ICSharpCode.TextEditor`, `OSGeo.MapGuide.MaestroAPI`, `OSGeo.MapGuide.MaestroAPI.FxBridge`, `Maestro.Login`, `Maestro.Editors`, `Maestro.MapViewer`
2. **Framework Update:** `net6.0-windows` → `net10.0-windows`
3. **Package Updates:** None flagged
4. **Expected Breaking Changes:** WinForms runtime map inspector — verify `ICSharpCode.TextEditor` code display and `Maestro.MapViewer` render integration.
5. **Validation Checklist:**
   - [ ] Builds targeting `net10.0-windows`
   - [ ] Inspector opens, connects to runtime map, and displays tree data

---

### Tier 7 — Application Shell (Level 6)

**Complexity:** Medium | **Risk:** Medium | **Prerequisite:** Tier 6 complete

---

#### `Maestro.Base` — `Maestro.Base\Maestro.Base.csproj`

**Current State**
- Framework: `net6.0-windows`
- Issues: 1 mandatory (`Project.0002`)
- Risk: Medium (central hub for all add-ins, 7 direct dependencies)

**Target State**
- Framework: `net10.0-windows`

**Migration Steps**
1. **Prerequisites:** All 7 dependencies complete: `ICSharpCode.Core.WinForms`, `ICSharpCode.Core`, `Maestro.Shared.UI`, `OSGeo.MapGuide.MaestroAPI`, `MaestroFsPreview`, `Maestro.Login`, `Maestro.Editors`
2. **Framework Update:** `net6.0-windows` → `net10.0-windows`
3. **Package Updates:** None flagged
4. **Expected Breaking Changes:**
   - This is the add-in host framework shell (based on SharpDevelop ICSharpCode.Core). Verify add-in loading mechanism works on .NET 10.
   - SharpDevelop's `AddInTree` and `ServiceManager` patterns — confirm no static initializers break under .NET 10's assembly loading changes.
   - WinForms `DockPanel` suite (v3.1.1, confirmed compatible) — verify docking layout persists correctly.
5. **Code Modifications:**
   - Review `Program.cs` / application startup for any .NET 10 hosting model changes.
   - Verify `AppDomain`-based add-in loading patterns — `AppDomain.CreateDomain` is no longer supported in .NET 10; if used, must migrate to `AssemblyLoadContext`.
   - ⚠️ **Requires validation:** Check if `ICSharpCode.Core`'s add-in loading uses `AppDomain.CreateDomain`. If so, this is a breaking change requiring `AssemblyLoadContext` migration.
6. **Validation Checklist:**
   - [ ] Builds targeting `net10.0-windows`
   - [ ] Application shell starts (splash screen appears)
   - [ ] Add-in loading mechanism works (at least one add-in loads)
   - [ ] DockPanel layout persists between sessions
   - [ ] 6 consumer projects (all add-ins + main app) compile

---

### Tier 8 — Add-ins & Main Application (Level 7)

**Complexity:** Medium | **Risk:** Medium | **Prerequisite:** Tier 7 (`Maestro.Base`) complete

---

#### `Maestro.AddIn.ExtendedObjectModels` — `Maestro.AddIn.ExtendedObjectModels\Maestro.AddIn.ExtendedObjectModels.csproj`

**Current State**
- Framework: `net6.0-windows`
- Issues: 1 mandatory (`Project.0002`)
- Risk: Low

**Target State**
- Framework: `net10.0-windows`

**Migration Steps**
1. **Prerequisites:** `Maestro.Base`, `ICSharpCode.Core`, `Maestro.Shared.UI`, `OSGeo.MapGuide.ObjectModels`, `OSGeo.MapGuide.MaestroAPI`, `Maestro.Editors` complete
2. **Framework Update:** `net6.0-windows` → `net10.0-windows`
3. **Package Updates:** None flagged
4. **Validation Checklist:**
   - [ ] Builds targeting `net10.0-windows`
   - [ ] Add-in loads in Maestro application

---

#### `Maestro.AddIn.FdoToolbox` — `Maestro.AddIn.FdoToolbox\Maestro.AddIn.FdoToolbox.csproj`

**Current State**
- Framework: `net6.0-windows`
- Issues: 1 mandatory (`Project.0002`)
- Risk: Low

**Target State**
- Framework: `net10.0-windows`

**Migration Steps**
1. **Prerequisites:** `ICSharpCode.Core.WinForms`, `Maestro.Base`, `ICSharpCode.Core`, `Maestro.Shared.UI`, `OSGeo.MapGuide.MaestroAPI` complete
2. **Framework Update:** `net6.0-windows` → `net10.0-windows`
3. **Package Updates:** None flagged
4. **Validation Checklist:**
   - [ ] Builds targeting `net10.0-windows`
   - [ ] FDO Toolbox add-in loads and functions

---

#### `Maestro.AddIn.Local` — `Maestro.AddIn.Local\Maestro.AddIn.Local.csproj` ⚠️

> **Note:** This add-in depends on `OSGeo.MapGuide.MaestroAPI.Local` which is the **net48 native-binding blocker**. Its migration is **blocked** until the special case is resolved. See [Special Case section](#special-case--osgeomap-guide-maestroapilocal-net48).

**Current State**
- Framework: `net6.0-windows`
- Issues: 3 mandatory (`NuGet.0001` ×2 — incompatible packages, `Project.0002`)
- Risk: **High** (blocked by native dependency)

**Target State**
- Framework: `net10.0-windows` (contingent on OSGeo.MapGuide.MaestroAPI.Local resolution)

**Migration Steps**
1. **Prerequisites:** `OSGeo.MapGuide.MaestroAPI.Local` special case resolved + all other Tier 7 dependencies complete
2. **Framework Update:** `net6.0-windows` → `net10.0-windows`
3. **Package Updates:** Depends on resolution of native packages in `OSGeo.MapGuide.MaestroAPI.Local`
4. **Validation Checklist:**
   - [ ] `OSGeo.MapGuide.MaestroAPI.Local` resolved (prerequisite)
   - [ ] Builds targeting `net10.0-windows`
   - [ ] Local provider connects to MapGuide server

---

#### `Maestro.AddIn.Rest` — `Maestro.AddIn.Rest\Maestro.AddIn.Rest.csproj`

**Current State**
- Framework: `net6.0-windows`
- Issues: 2 (`NuGet.0002` — RestSharp upgrade recommended, `Project.0002`)
- Risk: Low

**Target State**
- Framework: `net10.0-windows`

**Migration Steps**
1. **Prerequisites:** `Maestro.Base`, `ICSharpCode.Core`, `Maestro.Shared.UI`, `ICSharpCode.TextEditor`, `OSGeo.MapGuide.MaestroAPI` complete
2. **Framework Update:** `net6.0-windows` → `net10.0-windows`
3. **Package Updates:**

| Package | Current | Target | Reason |
|---|---|---|---|
| `RestSharp` | 106.15.0 | Latest compatible | NuGet.0002 — upgrade recommended |

   > ⚠️ Note: `RestSharp` v106.x is the legacy API. v107+ introduced breaking changes (async-only, different request API). Evaluate whether to upgrade to latest RestSharp or pin at 106.x with a .NET 10-compatible build. Check NuGet for a 106.x .NET 10 patch.
4. **Expected Breaking Changes:** If RestSharp is upgraded beyond v106.x, request/response patterns throughout the REST add-in will require refactoring.
5. **Validation Checklist:**
   - [ ] Builds targeting `net10.0-windows`
   - [ ] REST add-in connects to MapGuide REST API
   - [ ] RestSharp version decision documented

---

#### `Maestro.AddIn.Scripting` — `Maestro.AddIn.Scripting\Maestro.AddIn.Scripting.csproj`

**Current State**
- Framework: `net6.0-windows`
- Issues: 1 mandatory (`Project.0002`)
- Risk: Low-Medium

**Target State**
- Framework: `net10.0-windows`

**Migration Steps**
1. **Prerequisites:** `Maestro.Base`, `ICSharpCode.Core`, `Maestro.Shared.UI`, `Maestro.Scripting.Core`, `ICSharpCode.TextEditor`, `Maestro.Editors` complete
2. **Framework Update:** `net6.0-windows` → `net10.0-windows`
3. **Package Updates:** None flagged
4. **Expected Breaking Changes:** IronPython scripting panel — inherits validation from `Maestro.Scripting.Core`.
5. **Validation Checklist:**
   - [ ] Builds targeting `net10.0-windows`
   - [ ] Scripting add-in opens and executes a test script

---

#### `Maestro` — `Maestro\Maestro.csproj` *(Main Application)*

**Current State**
- Framework: `net6.0-windows`
- Issues: 1 mandatory (`Project.0002`)
- Risk: Medium (entry point — validates full solution integration)
- Direct dependencies: 14 projects

**Target State**
- Framework: `net10.0-windows`

**Migration Steps**
1. **Prerequisites:** All 14 direct dependencies complete (entire Tier 1–7 chain, plus `Maestro.MapPublisher`, `Maestro.LiveMapEditor`, `Maestro.Scripting.Core`, `MgTileSeeder`)
2. **Framework Update:** `net6.0-windows` → `net10.0-windows`
3. **Package Updates:** None flagged
4. **Expected Breaking Changes:**
   - `Program.cs` entry point — verify `[STAThread]` attribute and WinForms application initialization work on .NET 10.
   - Check `Application.SetHighDpiMode()` settings — .NET 10 WinForms DPI handling may differ.
   - Verify application manifest settings are correct for .NET 10.
   - Check for any `app.config` / `App.config` entries that need updating.
5. **Code Modifications:**
   - Review `Program.cs` for modern WinForms bootstrap pattern (`.NET 6+` style already used, ensure no `net48` remnants).
   - Update `app.config` if it contains obsolete runtime binding redirects.
6. **Testing Strategy:**
   - Full application smoke test:
     - Application launches and login dialog appears
     - Can connect to a MapGuide server (REST provider at minimum)
     - Resource browser opens and navigates
     - At least one resource editor opens
     - Scripting console opens
   - Run all unit tests.
7. **Validation Checklist:**
   - [ ] Builds targeting `net10.0-windows`
   - [ ] Application starts without runtime exceptions
   - [ ] Login dialog appears and accepts credentials
   - [ ] Resource browser populates
   - [ ] At least one editor opens
   - [ ] All unit tests pass
   - [ ] No compiler warnings

---

### Special Case — OSGeo.MapGuide.MaestroAPI.Local (net48)

> ⚠️ **CRITICAL BLOCKER** — This project cannot be migrated to .NET 10 without a decision on the native MapGuide bindings.

**Current State**
- Framework: `net48` (the only project still targeting .NET Framework 4.8)
- Files: 20
- Issues: 3 mandatory:
  - `NuGet.0001` — `mg-desktop-viewer-x64` v3.1.2.9484 incompatible, no replacement
  - `NuGet.0001` — `mg-desktop-x64` v3.1.2.9484 incompatible, no replacement
  - `Project.0002` — framework upgrade required
- Additional incompatible package in solution: `mapguide-api-base-x64` v3.1.2.9484

**Why this is blocked**

The three native MapGuide packages (`mapguide-api-base-x64`, `mg-desktop-x64`, `mg-desktop-viewer-x64`) provide the C++ P/Invoke layer for the MapGuide Desktop (local) API. These packages wrap unmanaged DLLs compiled against the .NET Framework runtime. There is **no .NET 10-compatible version** of these packages and the assessment tool confirms they have no suggested replacement version.

**Decision Required**

Before migrating this project, one of the following decisions must be made:

| Option | Description | Impact |
|---|---|---|
| **Option A: Keep net48** | Keep `OSGeo.MapGuide.MaestroAPI.Local` and `Maestro.AddIn.Local` targeting `net48`. The local provider remains a .NET Framework add-in. | The main application running on .NET 10 cannot load `net48` add-ins directly. Interop bridging required. |
| **Option B: Remove Local Provider** | Remove the Local MapGuide connection provider from the 6.0 final release. The REST-based connection remains fully functional. | Loss of local (in-process) MapGuide server connection. Acceptable for the final WinForms release before the Avalonia rebuild. |
| **Option C: P/Invoke Rebuild** | Rewrite the native bindings using direct P/Invoke against the MapGuide native DLLs, targeting .NET 10. | High effort — requires native interop expertise. Not recommended for a "final" release. |
| **Option D: Out-of-process provider** | Run the local provider as a separate `net48` process, expose it via named pipes or gRPC. | Medium effort — viable architectural solution for the Avalonia rebuild but over-engineered for the final 6.0 WinForms release. |

**Recommendation for 6.0 Final Release**

> **Option B** (Remove Local Provider) is the pragmatic choice for the 6.0 final release. The project's stated goal is to ship a stable .NET 10 final release before the full Avalonia rebuild. Since the Avalonia rebuild will also need to address the native binding problem from scratch, investing in Option A, C, or D for the WinForms final release has poor ROI.

**Migration Steps (if Option B — Remove)**
1. Remove `OSGeo.MapGuide.MaestroAPI.Local` project from solution
2. Remove `Maestro.AddIn.Local` project from solution
3. Remove references from `Maestro.csproj` to `Maestro.AddIn.Local`
4. Remove references to `mapguide-api-base-x64`, `mg-desktop-x64`, `mg-desktop-viewer-x64` NuGet packages
5. Update documentation and changelog to note Local Provider removal

**Migration Steps (if Option A — Keep net48)**
1. Keep both projects at `net48`
2. Do **not** attempt framework change
3. Accept that the Local Provider add-in will not load in the .NET 10 application without an assembly resolver or side-loading strategy
4. Document limitation clearly in release notes

**Validation Checklist (Option B):**
- [ ] `OSGeo.MapGuide.MaestroAPI.Local` removed from solution
- [ ] `Maestro.AddIn.Local` removed from solution
- [ ] No orphaned references to removed projects
- [ ] Solution builds cleanly
- [ ] Release notes updated

---

### Test Projects

**Prerequisite:** Projects under test must be fully upgraded first.

---

#### `OSGeo.MapGuide.MaestroAPI.Tests` — `OSGeo.MapGuide.MaestroAPI.Tests\OSGeo.MapGuide.MaestroAPI.Tests.csproj`

**Current State**
- Framework: `net6.0`
- Issues: 1 mandatory (`Project.0002`)
- Test Framework: `xunit` 2.9.0 (confirmed compatible)

**Target State**
- Framework: `net10.0`

**Migration Steps**
1. **Prerequisites:** `OSGeo.MapGuide.ObjectModels`, `OSGeo.MapGuide.MaestroAPI`, `OSGeo.FDO.Expressions` complete
2. **Framework Update:** `net6.0` → `net10.0`
3. **Package Updates:** None flagged
4. **Validation Checklist:**
   - [ ] Builds targeting `net10.0`
   - [ ] All existing tests pass
   - [ ] No test regressions

---

#### `OSGeo.MapGuide.MaestroAPI.IntegrationTests` — `OSGeo.MapGuide.MaestroAPI.IntegrationTests\OSGeo.MapGuide.MaestroAPI.IntegrationTests.csproj`

**Current State**
- Framework: `net6.0`
- Issues: 1 mandatory (`Project.0002`)
- Test Framework: `xunit` + `Xunit.SkippableFact` (both confirmed compatible)

**Target State**
- Framework: `net10.0`

**Migration Steps**
1. **Prerequisites:** `OSGeo.MapGuide.ObjectModels`, `OSGeo.MapGuide.MaestroAPI` complete
2. **Framework Update:** `net6.0` → `net10.0`
3. **Package Updates:** None flagged
4. **Validation Checklist:**
   - [ ] Builds targeting `net10.0`
   - [ ] Skippable integration tests pass (or skip correctly when server unavailable)

---

## 5. Package Update Reference

### Packages Requiring Action

| Package | Current Version | Target Version | Action | Affected Projects | Reason |
|---|---|---|---|---|---|
| `Newtonsoft.Json` | 13.0.3 | **13.0.4** | Update | `OSGeo.MapGuide.ObjectModels` | Recommended upgrade (NuGet.0002) |
| `System.Data.Odbc` | 6.0.0 | **10.0.5** | Update | `Maestro.Editors` | Recommended upgrade (NuGet.0002) |
| `System.Drawing.Common` | 6.0.0 | **10.0.5** | Update | `OSGeo.MapGuide.MaestroAPI.FxBridge` | Recommended upgrade (NuGet.0002) |
| `Microsoft.Win32.Registry` | 5.0.0 | *(remove)* | **Remove** | `ICSharpCode.Core` | Absorbed by .NET framework (NuGet.0003) |
| `TreeViewAdv` | 1.7.0 | *(remove)* | **Remove + Replace** | `Maestro.Editors`, `MpuCalc` | Incompatible, no .NET 10 version (NuGet.0001) |
| `mapguide-api-base-x64` | 3.1.2.9484 | *(remove)* | **Remove** | `OSGeo.MapGuide.MaestroAPI.Local` | Incompatible, no .NET 10 version (NuGet.0001) |
| `mg-desktop-x64` | 3.1.2.9484 | *(remove)* | **Remove** | `OSGeo.MapGuide.MaestroAPI.Local` | Incompatible, no .NET 10 version (NuGet.0001) |
| `mg-desktop-viewer-x64` | 3.1.2.9484 | *(remove)* | **Remove** | `OSGeo.MapGuide.MaestroAPI.Local` | Incompatible, no .NET 10 version (NuGet.0001) |

### Confirmed Compatible Packages (No Action Required)

| Package | Version | Notes |
|---|---|---|
| `CommandLineParser` | 2.9.1 | ✅ Compatible |
| `coverlet.msbuild` | 6.0.2 | ✅ Compatible |
| `DockPanelSuite` | 3.1.1 | ✅ Compatible |
| `DockPanelSuite.ThemeVS2003/2005/2012/2013/2015` | 3.1.1 | ✅ Compatible |
| `IronPython` | 2.7.11 | ✅ Compatible (runtime validation recommended) |
| `IronPython.StdLib` | 2.7.11 | ✅ Compatible |
| `Irony.Core` | 1.0.7 | ✅ Compatible |
| `Microsoft.CSharp` | 4.7.0 | ✅ Compatible |
| `Microsoft.IO.RecyclableMemoryStream` | 3.0.1 | ✅ Compatible |
| `Microsoft.NET.Test.Sdk` | 17.11.1 | ✅ Compatible |
| `Microsoft.SourceLink.GitHub` | 1.1.1 | ✅ Compatible |
| `Moq` | 4.18.4 | ✅ Compatible |
| `NETStandard.Library` | 2.0.3 | ✅ Compatible |
| `NetTopologySuite` | 2.5.0 | ✅ Compatible |
| `Newtonsoft.Json.Schema` | 3.0.14 | ✅ Compatible |
| `NSIS` | 2.51.0 | ✅ Compatible |
| `Polly` | 8.4.1 | ✅ Compatible |
| `ProjNet` | 2.0.0 | ✅ Compatible |
| `RazorEngine.NetCore` | 3.1.0 | ✅ Compatible |
| `reportgenerator` | 5.1.19 | ✅ Compatible |
| `RestSharp` | 106.15.0 | ✅ Compatible (upgrade evaluation needed for Maestro.AddIn.Rest) |
| `SharpZipLib` | 1.4.2 | ✅ Compatible |
| `xunit` | 2.9.0 | ✅ Compatible |
| `xunit.runner.console` | 2.9.0 | ✅ Compatible |
| `xunit.runner.visualstudio` | 2.8.2 | ✅ Compatible |
| `Xunit.SkippableFact` | 1.4.13 | ✅ Compatible |

---

## 6. Breaking Changes Catalog

### Category 1: Framework Target Changes (net6.0 / net6.0-windows → net10.0 / net10.0-windows)

All projects upgrading from .NET 6 to .NET 10 cross four major version boundaries (.NET 7, 8, 9, 10). The following categories of breaking changes should be validated during each tier's compilation:

| Area | Description | Affected Projects | Severity |
|---|---|---|---|
| **WinForms High-DPI** | .NET 10 improves per-monitor DPI awareness. Layout of forms/controls may shift. | All `net10.0-windows` projects | Medium |
| **System.Drawing.Common** | On non-Windows, many APIs throw. On Windows (our case with `-windows` TFM), behavior is preserved. Upgrading to 10.0.5 ensures correct behavior. | `OSGeo.MapGuide.MaestroAPI.FxBridge`, `ICSharpCode.TextEditor`, `Maestro.MapViewer` | Low |
| **AppDomain.CreateDomain removed** | `AppDomain.CreateDomain()` is not supported in .NET Core/.NET 5+. If ICSharpCode.Core's add-in loader used this for isolation, it must be migrated to `AssemblyLoadContext`. | `ICSharpCode.Core`, `Maestro.Base` | **High** |
| **Thread.Abort removed** | `Thread.Abort()` throws `PlatformNotSupportedException` on .NET 5+. Any background worker that relied on it must use `CancellationToken`. | Any project with long-running background tasks | Medium |
| **BinaryFormatter removed** | `BinaryFormatter` is removed in .NET 9+. If used for settings/state serialization, replace with `System.Text.Json` or `XmlSerializer`. | ⚠️ Search all projects for `BinaryFormatter` usage | **High** |
| **Nullable reference types** | .NET 10 projects may surface more nullable warnings. Not breaking unless `<Nullable>enable</Nullable>` is set. | All projects | Low |
| **XmlReader/XmlWriter changes** | Minor behavioral changes in XML parsing. MapGuide API responses are XML-heavy — verify serialization round-trips. | `OSGeo.MapGuide.MaestroAPI`, `OSGeo.MapGuide.ObjectModels` | Low |
| **HttpClient changes** | If using `HttpClient`, default TLS versions and connection handling changed between .NET 6–10. | `Maestro.AddIn.Rest`, `OSGeo.MapGuide.MaestroAPI` | Low |

### Category 2: Package-Specific Breaking Changes

| Package Change | Breaking Change Risk | Notes |
|---|---|---|
| `Newtonsoft.Json` 13.0.3 → 13.0.4 | None — patch release | No API changes |
| `System.Data.Odbc` 6.0.0 → 10.0.5 | Low — minor API additions only | No breaking removals |
| `System.Drawing.Common` 6.0.0 → 10.0.5 | Low on Windows | Cross-platform guards already in place via `-windows` TFM |
| `Microsoft.Win32.Registry` removed | None — types available in-framework | Transparent removal |
| `TreeViewAdv` removed | **Critical** — full UI control replacement required | All `TreeViewAdv` usage must be replaced |

### Category 3: WinForms-Specific Changes (.NET 6 → .NET 10)

| Change | Details |
|---|---|
| `Control.DefaultFont` | Changed to `Segoe UI 9pt` in .NET 6+. If custom fonts were set explicitly, behavior is preserved. If relying on ambient default, verify form layouts. |
| `DataGridView` improvements | Several rendering improvements; existing code unaffected unless using undocumented behavior. |
| `ToolStripRenderer` | Minor rendering updates between versions. Verify toolbar/menu appearance. |
| `MessageBox` on .NET 10 | Now uses task dialog internally on some Windows versions; appearance may differ. |

### Category 4: IronPython on .NET 10

> ⚠️ **Requires explicit validation during Tier 6**

`IronPython` 2.7.11 was built targeting .NET Standard 2.0 / .NET Framework. While it is marked as compatible by the assessment tool, .NET 10 includes runtime changes that could affect dynamic scripting. Validate:
- Engine initialization: `ScriptEngine engine = Python.CreateEngine()`
- Basic script execution
- Access to .NET types from Python scripts
- `import clr` functionality

---

## 7. Testing & Validation Strategy

### Multi-Level Testing Approach

#### Level 1 — Per-Tier Build Gate
After upgrading **each tier**, verify:
- [ ] All projects in the tier build without errors
- [ ] All projects in the tier build without warnings (or warnings are documented/suppressed)
- [ ] Projects in higher tiers (still on .NET 6) continue to compile — a `netstandard2.0` or .NET 10 project is consumable by .NET 6 projects

#### Level 2 — Smoke Tests Per Key Project

| Project | Smoke Test |
|---|---|
| `MpuCalc` | Application starts, tree view renders |
| `Maestro.MapViewer` | Map viewer control paints without exception |
| `Maestro.Editors` | Open one resource editor (e.g., Layer Definition editor) |
| `Maestro.Base` | Application shell starts, menus appear |
| `Maestro` (main app) | Login dialog appears; connect and browse resources |

#### Level 3 — Automated Tests

Run after each tier's test projects are upgraded:

| Test Project | When to Run | Expected Outcome |
|---|---|---|
| `OSGeo.MapGuide.MaestroAPI.Tests` | After Tier 2 (API upgraded) | 100% pass |
| `OSGeo.MapGuide.MaestroAPI.IntegrationTests` | After Tier 2 (requires MapGuide server) | Pass or skip (server unavailable) |

#### Level 4 — Full Solution Validation (after Tier 8)
- Complete solution build: zero errors, zero new warnings
- Full test suite run: all unit tests pass
- Manual integration test:
  1. Launch Maestro on .NET 10
  2. Connect to MapGuide server via REST provider
  3. Browse resource repository
  4. Open and save a Layer Definition
  5. Open and save a Map Definition
  6. Publish a map via MapPublisher
  7. Run a simple IronPython script
  8. Open RtMapInspector on a running map

### Per-Tier Test Summary

| Tier | Build Gate | Automated Tests | Smoke Test |
|---|---|---|---|
| 1 | ✅ required | — | MpuCalc tree view |
| 2 | ✅ required | ✅ MaestroAPI.Tests | — |
| 3 | ✅ required | ✅ Integration tests | — |
| 4 | ✅ required | — | MapViewer renders |
| 5 | ✅ required | ✅ Run all tests | Editor panel opens |
| 6 | ✅ required | ✅ Run all tests | Components launch |
| 7 | ✅ required | ✅ Run all tests | Shell starts |
| 8 | ✅ required | ✅ Run all tests | Full app scenario |

---

## 8. Risk Management

### High-Risk Items

| Project | Risk Level | Description | Mitigation |
|---|---|---|---|
| `OSGeo.MapGuide.MaestroAPI.Local` | 🔴 **Critical** | Native packages (`mg-desktop-x64`, etc.) have no .NET 10 version. Project targets `net48`. | **Decision required before migration starts.** Recommend removing for 6.0 final (Option B). Document in release notes. |
| `Maestro.Editors` | 🔴 **High** | 936 files. `TreeViewAdv` incompatible with no direct replacement. Largest project in solution. | Address `TreeViewAdv` replacement first before framework upgrade. Use standard WinForms `TreeView` as replacement. Allocate extra review capacity. |
| `Maestro.AddIn.Local` | 🟠 **High** | Depends on `OSGeo.MapGuide.MaestroAPI.Local` — blocked until that special case is resolved. | Defer until special case decision made. If Option B chosen, remove entirely. |
| `Maestro.Base` | 🟠 **Medium-High** | ICSharpCode add-in loader may use `AppDomain.CreateDomain` (removed in .NET Core). | Audit `ICSharpCode.Core` add-in loading for `AppDomain` usage before upgrading. If found, migrate to `AssemblyLoadContext`. |
| `IronPython` scripting | 🟠 **Medium** | IronPython 2.7.11 compatibility on .NET 10 is marked compatible but runtime behavior unverified. | Run scripting smoke test immediately after Tier 6. If fails, evaluate IronPython 3.x upgrade or script engine removal for 6.0 final. |
| `MpuCalc` | 🟡 **Medium** | `TreeViewAdv` incompatible (same issue as Maestro.Editors). Standalone app. | Replace with standard `TreeView` control. Lower risk as standalone tool. |
| `RestSharp` 106.x | 🟡 **Low-Medium** | v106.x is legacy; upgrade to v107+ involves breaking API changes. | Pin to 106.x if a .NET 10-compatible build exists. Only upgrade to v107+ if explicitly justified. |

### Security Vulnerabilities

No security vulnerabilities were flagged in the assessment for any NuGet packages. ✅

### Contingency Plans

#### If `AppDomain.CreateDomain` is found in ICSharpCode.Core
- **Plan A:** Migrate to `AssemblyLoadContext` — standard .NET Core pattern for plugin isolation.
- **Plan B:** Remove plugin isolation (all add-ins load in the default context) — acceptable for the final WinForms release.

#### If IronPython 2.7.11 fails on .NET 10
- **Plan A:** Upgrade to IronPython 3.4+ (targets .NET 6+). Requires testing for Python 2 vs Python 3 script compatibility.
- **Plan B:** Disable scripting for 6.0 final release and note in changelog that scripting will be redesigned in the Avalonia rebuild.

#### If TreeViewAdv replacement causes major regressions in Maestro.Editors
- **Plan A:** Use the standard WinForms `TreeView` with a compat shim for the most common `TreeViewAdv` APIs.
- **Plan B:** Source a community fork of TreeViewAdv that targets .NET 6+ (check GitHub for `Aga.Controls.Tree` forks).
- **Plan C:** Given this is the "final" WinForms release, accept reduced tree view feature parity as a known limitation.

#### If OSGeo.MapGuide.MaestroAPI.Local cannot be resolved
- Remove `OSGeo.MapGuide.MaestroAPI.Local` and `Maestro.AddIn.Local` from the 6.0 final release. Document Local Provider as deprecated.

### Rollback Strategy

All work is done on the `upgrade-to-NET10` branch. The `master` branch remains on .NET 6 until the migration is approved and merged. Each tier's changes can be committed separately, allowing rollback to any tier boundary using `git revert` or `git reset`.

---

## 9. Complexity & Effort Assessment

### Per-Project Complexity

| Project | Tier | Framework Change | Complexity | Risk | Key Challenges |
|---|---|---|---|---|---|
| `ICSharpCode.Core` | 1 | None | **Low** | Low | Remove one package reference |
| `ICSharpCode.TextEditor` | 1 | net6.0-windows → net10.0-windows | **Low** | Low | Third-party code, WinForms APIs |
| `Maestro.Shared.UI` | 1 | net6.0-windows → net10.0-windows | **Low** | Low | Widely consumed — validate all consumers |
| `OSGeo.FDO.Expressions` | 1 | None | **None** | None | No changes needed |
| `OSGeo.MapGuide.ObjectModels` | 1 | None | **Low** | Low | Newtonsoft.Json patch update |
| `MpuCalc` | 1 | net6.0-windows → net10.0-windows | **Medium** | Medium | TreeViewAdv replacement |
| `SignMapGuideApi` | 1 | net6.0-windows → net10.0-windows | **Low** | Low | Utility tool |
| `ICSharpCode.Core.WinForms` | 2 | net6.0-windows → net10.0-windows | **Low** | Low | Third-party SharpDevelop code |
| `LocalConfigure` | 2 | net6.0-windows → net10.0-windows | **Low** | Low | Config utility |
| `OSGeo.MapGuide.MaestroAPI` | 2 | None | **None** | None | Most-used library — no changes |
| `Maestro.AddInManager` | 3 | net6.0-windows → net10.0-windows | **Low** | Low | Add-in manager dialog |
| `Maestro.Login` | 3 | net6.0-windows → net10.0-windows | **Low** | Low | Login UI |
| `Maestro.MapPublisher.Common` | 3 | None | **None** | None | No changes needed |
| `Maestro.Packaging` | 3 | net6.0-windows → net10.0-windows | **Low** | Low | Packaging/ZIP |
| `MgTileSeeder` | 3 | net6.0 → net10.0 | **Low** | Low | CLI tool |
| `OSGeo.MapGuide.MaestroAPI.FxBridge` | 3 | net6.0-windows → net10.0-windows | **Low** | Low | System.Drawing.Common update |
| `OSGeo.MapGuide.MaestroAPI.Local` | Special | net48 → blocked | **Critical** | Critical | Native packages, no .NET 10 support |
| `Maestro.MapPublisher` | 4 | net6.0 → net10.0 | **Low** | Low | Map publishing pipeline |
| `Maestro.MapViewer` | 4 | net6.0-windows → net10.0-windows | **Low** | Low | GDI+/painting review |
| `Maestro.Editors` | 5 | net6.0-windows → net10.0-windows | **High** | High | 936 files, TreeViewAdv replacement, System.Data.Odbc |
| `Maestro.LiveMapEditor` | 6 | net6.0-windows → net10.0-windows | **Low** | Low | Inherits Editors changes |
| `Maestro.Scripting.Core` | 6 | net6.0-windows → net10.0-windows | **Medium** | Medium | IronPython runtime validation |
| `MaestroFsPreview` | 6 | net6.0-windows → net10.0-windows | **Low** | Low | Preview tool |
| `RtMapInspector` | 6 | net6.0-windows → net10.0-windows | **Low** | Low | 8 dependencies, all upgraded |
| `Maestro.Base` | 7 | net6.0-windows → net10.0-windows | **Medium** | Medium | Add-in loader, AppDomain check |
| `Maestro.AddIn.ExtendedObjectModels` | 8 | net6.0-windows → net10.0-windows | **Low** | Low | Simple add-in |
| `Maestro.AddIn.FdoToolbox` | 8 | net6.0-windows → net10.0-windows | **Low** | Low | FDO toolbox add-in |
| `Maestro.AddIn.Local` | 8 | Blocked | **High** | High | Blocked by OSGeo.MapGuide.MaestroAPI.Local |
| `Maestro.AddIn.Rest` | 8 | net6.0-windows → net10.0-windows | **Low** | Low | RestSharp version evaluation |
| `Maestro.AddIn.Scripting` | 8 | net6.0-windows → net10.0-windows | **Low** | Low | Inherits Scripting.Core |
| `Maestro` | 8 | net6.0-windows → net10.0-windows | **Medium** | Medium | Entry point, 14 dependencies |
| `OSGeo.MapGuide.MaestroAPI.Tests` | Test | net6.0 → net10.0 | **Low** | Low | xunit compatible |
| `OSGeo.MapGuide.MaestroAPI.IntegrationTests` | Test | net6.0 → net10.0 | **Low** | Low | Skippable tests |

### Phase Complexity Summary

| Phase | Tiers | Relative Complexity | Key Effort Driver |
|---|---|---|---|
| Phase A | Tier 1 | Medium | MpuCalc TreeViewAdv, 7 projects |
| Phase B | Tier 2 | Low | Mostly no-change projects |
| Phase C | Tier 3 | Medium | OSGeo.MapGuide.MaestroAPI.Local decision |
| Phase D | Tier 4 | Low | Two viewer projects |
| Phase E | Tier 5 | **High** | Maestro.Editors — 936 files, TreeViewAdv |
| Phase F | Tier 6 | Medium | IronPython validation |
| Phase G | Tier 7 | Medium | Add-in loader AppDomain check |
| Phase H | Tier 8 | Medium | Full app integration testing |
| Phase X | Special | **Critical** | Native package decision required |

---

## 10. Source Control Strategy

### Branch Layout

```
master  ──────────────────────────────────────────── (stays on .NET 6, stable)
               │
               └── upgrade-to-NET10  ─── [all upgrade work here]
                        │
                        ├── commit: [Tier 1] Upgrade foundation libraries to net10.0
                        ├── commit: [Tier 2] Upgrade core API layer to net10.0
                        ├── commit: [Tier 3] Upgrade services & utilities to net10.0
                        ├── commit: [Tier 3-special] Resolve/remove Local provider
                        ├── commit: [Tier 4] Upgrade viewer components to net10.0
                        ├── commit: [Tier 5] Upgrade Maestro.Editors to net10.0 (+ TreeViewAdv replacement)
                        ├── commit: [Tier 6] Upgrade composite UI components to net10.0
                        ├── commit: [Tier 7] Upgrade Maestro.Base to net10.0
                        ├── commit: [Tier 8] Upgrade add-ins and main application to net10.0
                        └── commit: [Tests] Upgrade test projects to net10.0
```

### Commit Strategy

- **One commit per tier** — keeps the history clean and each commit represents a buildable, testable state
- **Commit message format:** `[Tier N] <description>`
- **Special case commit:** `[Special] Resolve OSGeo.MapGuide.MaestroAPI.Local — <chosen option>`
- Commits should include both project file changes AND any code fixes needed to make that tier compile

### Pull Request Process

1. Open a single PR from `upgrade-to-NET10` → `master`
2. PR title: `feat: Upgrade MapGuide Maestro 6.0 final release to .NET 10`
3. PR description should reference this plan and list completed tiers
4. PR checklist:
   - [ ] All projects targeting net10.0 or net10.0-windows (except netstandard2.0 projects)
   - [ ] All mandatory package actions completed
   - [ ] All unit tests pass
   - [ ] Full application smoke test completed
   - [ ] Release notes updated with Local Provider status
   - [ ] CHANGELOG updated noting .NET 10 upgrade and Avalonia rebuild roadmap

### Do Not Merge Until

- Tier 8 (main application) fully validated
- OSGeo.MapGuide.MaestroAPI.Local decision documented and implemented
- All automated tests pass on `upgrade-to-NET10` branch

---

## 11. Success Criteria

### Technical Criteria

| Criterion | Definition of Done |
|---|---|
| **All projects migrated** | Every project targets `net10.0`, `net10.0-windows`, or `netstandard2.0` (no `net6.0` or `net48` targets remain, except `OSGeo.MapGuide.MaestroAPI.Local` per chosen option) |
| **All package actions applied** | `Newtonsoft.Json` → 13.0.4, `System.Data.Odbc` → 10.0.5, `System.Drawing.Common` → 10.0.5, `Microsoft.Win32.Registry` removed, `TreeViewAdv` removed/replaced, native MapGuide packages handled |
| **Solution builds clean** | `dotnet build Maestro.sln` completes with zero errors and zero warnings on the `upgrade-to-NET10` branch |
| **All unit tests pass** | `OSGeo.MapGuide.MaestroAPI.Tests` — 100% pass rate |
| **Integration tests pass or skip** | `OSGeo.MapGuide.MaestroAPI.IntegrationTests` — pass or skip (when server unavailable) |
| **No security vulnerabilities** | No packages flagged with security vulnerabilities (already clean — confirm after updates) |
| **TreeViewAdv replaced** | Zero references to `Aga.Controls.Tree` namespace remaining in solution |
| **Local Provider decision enacted** | `OSGeo.MapGuide.MaestroAPI.Local` either removed (Option B) or documented as net48-only (Option A) |

### Quality Criteria

| Criterion | Definition of Done |
|---|---|
| **Application launches** | Maestro.exe starts on .NET 10 runtime without exceptions |
| **REST connection works** | Can connect to a MapGuide server via REST provider |
| **Editors functional** | At least Layer Definition, Map Definition, and Feature Source editors open and save |
| **Scripting works** | IronPython script executes in scripting console (or scripting removed with documented decision) |
| **Map viewer renders** | MapViewer control renders a map without GDI+ exceptions |
| **Release notes updated** | `changelog.txt` updated with .NET 10 upgrade note and Avalonia rebuild announcement |

### Strategy-Specific Criteria (Bottom-Up)

| Criterion | Definition of Done |
|---|---|
| **Tier order respected** | No project was upgraded before all its dependencies were upgraded |
| **Each tier buildable** | At every tier boundary, `dotnet build` succeeded for the upgraded projects |
| **No regressions per tier** | Lower-tier automated tests still pass after each higher tier is upgraded |
| **OSGeo.MapGuide.MaestroAPI.Local handled** | A documented, deliberate decision was made and implemented before Tier 8 |

### The MapGuide Maestro 6.0 Final Release is Ready When:

> ✅ All 33 projects are on their target framework  
> ✅ The solution builds clean on .NET 10  
> ✅ All automated tests pass  
> ✅ The application runs end-to-end on .NET 10  
> ✅ The path to the Avalonia rebuild is clear and documented
