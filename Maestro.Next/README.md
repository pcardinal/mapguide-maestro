# MapGuide Maestro — Next Generation

Cross-platform MapGuide authoring and administration tool rebuilt from the ground up with **Avalonia UI** on **.NET 10**.

> This is the successor to the WinForms-based MapGuide Maestro 6.x. It runs natively on **Windows**, **Linux**, and **macOS**.

📊 **155 tests** · **8,400+ lines** · **87 source files** · **25 test files**

---

## Features

### Resource Editors (7 native + 1 generic XML)

- **Feature Source** — Connection properties, Test Connection, Schema Preview, Data Preview, Extensions (joins/calc), CS Override, Save
- **Layer Definition** — Vector/Raster/Drawing, Scale Ranges with inline editing (add/remove/reorder), Color picker, Theme Wizard, Save
- **Map Definition** — Layer tree, Groups, MoveUp/Down, Visibility toggles, Extent editor, Save
- **Web Layout** — Title, Map, Initial View, UI Panels, Toolbar commands, Save
- **Application Definition** — Fusion: Map Groups, Widget Sets, Template URL, Save
- **Symbol Definition** — Simple/Compound, Graphics, Parameters, Usage flags, Save
- **All other types** — XML Editor with Find/Replace, diff view, syntax validation

### Site Explorer

- **Lazy-loading tree** with folder expansion
- **Real-time search** (filters by name and resource type)
- **Context menu**: Open, New, Rename, Copy, Cut, Paste, Delete, Edit as XML
- **Drag-and-Drop**: Move (default) / Copy (Ctrl+drag)
- **Cut indicator**: reduced opacity on cut source nodes
- **Advanced commands**: Repoint FeatureSource, Validate Resource, View Header XML, Dependencies, Spatial Contexts, Purge Cache

### Expression Builder

- Property auto-completion from class definition
- FDO function list from provider capabilities
- Operator palette
- **Syntax validation** via FdoFilter.Parse / FdoExpression.Parse

### Theme Wizard

- Query distinct values from a property
- Golden-angle color generation
- Per-value rule generation with selectable entries

### Package Management

- **Create Package** — export resources to .mgp with folder selection
- **Load Package** — upload .mgp to server with progress

### Server Administration

- **Server Status** — version, session, users, groups (via ISiteService)
- **Setup Folder Structure** — create standard Library folders

### Application

- **Dark/Light/System theme** — persisted in user preferences
- **Keyboard shortcuts**: `Ctrl+S` Save, `Ctrl+Shift+S` Save All, `Ctrl+W` Close tab, `Ctrl+N` New, `F2` Rename, `F5` Refresh, `Del` Delete, `Ctrl+Q` Quit
- **Toast notifications** with auto-dismiss
- **Status bar** with connection info (provider + version)
- **Server Info panel** — version, FDO providers
- **Splash screen**, **About dialog**, **Tip of the Day**
- **Options dialog** — theme, default folder preferences

---

## Architecture

```
Maestro.Next/
├── Helpers/            # ResourceIcons, StringExtensions, ResourceTypeIconMap
├── Services/           # DI-injectable services
│   ├── IConnectionService     # MapGuide server connection
│   ├── IClipboardService      # Cut/Copy/Paste clipboard
│   ├── INotificationService   # Toast notifications
│   ├── INewResourceService    # Resource creation
│   └── IPreferencesService    # User settings persistence
├── ViewModels/         # MVVM ViewModels (CommunityToolkit.Mvvm)
│   ├── MainWindowViewModel
│   ├── SiteExplorerViewModel  # Tree, search, context commands
│   ├── DocumentManagerViewModel
│   ├── ExpressionBuilderViewModel
│   ├── ThemeWizardViewModel
│   ├── ServerStatusViewModel
│   ├── TipOfTheDayViewModel
│   ├── Editors/               # One ViewModel per resource type
│   └── ...
├── Views/              # Avalonia AXAML views
│   ├── MainWindow             # Menu, toolbar, status bar
│   ├── SiteExplorerView       # TreeView + search + DnD
│   ├── DocumentAreaView       # Tab container
│   ├── Editors/               # One View per resource type
│   └── ...
├── publish.ps1         # Cross-platform publish (PowerShell)
├── publish.sh          # Cross-platform publish (Bash)
└── Program.cs          # Entry point + DI container setup
```

### Key Design Decisions

- **MVVM** with `CommunityToolkit.Mvvm` (source generators, `[ObservableProperty]`, `[RelayCommand]`)
- **Dependency Injection** via `Microsoft.Extensions.DependencyInjection`
- **No WinForms dependency** — pure Avalonia UI
- **Reuses existing ObjectModels** — `OSGeo.MapGuide.ObjectModels` and `OSGeo.MapGuide.MaestroAPI` (both .NET Standard 2.0)

---

## Building

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Build

```bash
dotnet build Maestro.Next/Maestro.Next.csproj
```

### Run

```bash
dotnet run --project Maestro.Next/Maestro.Next.csproj
```

### Run Tests

```bash
dotnet test Maestro.Next.Tests/Maestro.Next.Tests.csproj
```

### Publish (self-contained, all platforms)

```powershell
# PowerShell (all platforms at once)
.\Maestro.Next\publish.ps1

# Or single platform
.\Maestro.Next\publish.ps1 -Runtime win-x64
```

```bash
# Bash
./Maestro.Next/publish.sh

# Or single platform
./Maestro.Next/publish.sh linux-x64
```

---

## License

LGPL 2.1 — same as the original MapGuide Maestro.
