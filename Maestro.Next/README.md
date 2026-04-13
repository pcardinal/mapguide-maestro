# MapGuide Maestro — Next Generation

Cross-platform MapGuide authoring and administration tool rebuilt from the ground up with **Avalonia UI** on **.NET 10**.

> This is the successor to the WinForms-based MapGuide Maestro 6.x. It runs natively on **Windows**, **Linux**, and **macOS**.

---

## Features

### Resource Editors (7 native + 1 generic XML)

| Resource Type | Editor | Capabilities |
|---|---|---|
| Feature Source | Native | Connection properties, Test Connection, Save |
| Layer Definition | Native | Vector/Raster/Drawing, Scale Ranges, Save |
| Map Definition | Native | Layer tree, Groups, MoveUp/Down, Visibility toggles, Save |
| Web Layout | Native | Title, Map, Initial View, UI Panels, Toolbar commands, Save |
| Application Definition | Native | Fusion: Map Groups, Widget Sets, Template URL, Save |
| Symbol Definition | Native | Simple/Compound, Graphics, Parameters, Usage flags, Save |
| All other types | XML Editor | Full XML editing with Save |

### Site Explorer

- **Lazy-loading tree** with folder expansion
- **Real-time search** (filters by name and resource type)
- **Context menu**: Open, New Resource, Rename, Copy, Refresh, Delete
- **Double-click** to open in editor

### Resource Management

- **New Resource dialog** — create Folders, Feature Sources, Layer Definitions, Map Definitions, Web Layouts
- **Delete** with confirmation
- **Rename** and **Copy** resources
- **Validation** — run MapGuide resource validators with results panel

### Application

- **Dark/Light/System theme** — persisted in user preferences
- **Keyboard shortcuts**: `Ctrl+S` Save, `Ctrl+W` Close tab, `Ctrl+Q` Quit
- **Toast notifications** with auto-dismiss
- **Server Info panel** — version, FDO providers
- **About dialog** with runtime info

---

## Architecture

```
Maestro.Next/
├── Services/           # DI-injectable services
│   ├── IConnectionService     # MapGuide server connection
│   ├── IResourceService       # Resource listing (async)
│   ├── INotificationService   # Toast notifications
│   ├── INewResourceService    # Resource creation
│   └── IPreferencesService    # User settings persistence
├── ViewModels/         # MVVM ViewModels (CommunityToolkit.Mvvm)
│   ├── MainWindowViewModel
│   ├── SiteExplorerViewModel
│   ├── DocumentManagerViewModel
│   ├── Editors/               # One ViewModel per resource type
│   └── ...
├── Views/              # Avalonia AXAML views
│   ├── MainWindow
│   ├── SiteExplorerView
│   ├── DocumentAreaView
│   ├── Editors/               # One View per resource type
│   └── ...
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

### Publish (self-contained)

```bash
# Linux
dotnet publish Maestro.Next/Maestro.Next.csproj -c Release --self-contained -r linux-x64

# macOS
dotnet publish Maestro.Next/Maestro.Next.csproj -c Release --self-contained -r osx-x64

# Windows
dotnet publish Maestro.Next/Maestro.Next.csproj -c Release --self-contained -r win-x64
```

---

## CI/CD

GitHub Actions workflow at `.github/workflows/maestro-next.yml` builds and publishes for all 3 platforms on every push.

---

## License

GNU Lesser General Public License v2.1 — see [LICENSE](../LICENSE) for details.
