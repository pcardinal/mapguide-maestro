# Maestro.Next — Tâches restantes pour compléter la migration Avalonia

> **Généré le** : 2025-07-15
> **Branche** : `upgrade-to-NET10`
> **État actuel** : 57 fichiers source, 39 tests, 12 commits, build ✅

---

## Légende

- `[ ]` — À faire
- `[~]` — Partiellement fait
- `[✓]` — Terminé (référence seulement)

---

## Phase A — Fonctionnalités manquantes du Site Explorer

> Le WinForms original a 31 commandes dans `Maestro.Base/Commands/SiteExplorer/`.
> Maestro.Next en implémente 6. Il en manque 25.

### [ ] A.1 — Clipboard (Cut / Copy / Paste de ressources)

- [ ] A.1.1 — Ajouter un `IClipboardService` (ResourceId + opération Cut/Copy)
- [ ] A.1.2 — Commandes `CutCommand`, `CopyCommand`, `PasteCommand` dans `SiteExplorerViewModel`
- [ ] A.1.3 — Raccourcis Ctrl+X, Ctrl+C, Ctrl+V dans le TreeView
- [ ] A.1.4 — Indicateur visuel "coupé" (opacité réduite sur le nœud source)
- **Réf WinForms** : `Maestro.Base/Commands/CutCommand.cs`, `CopyCommand.cs`, `PasteCommand.cs`

### [ ] A.2 — Drag-and-Drop (déplacer ressources entre dossiers)

- [ ] A.2.1 — Activer `DragDrop` sur le `TreeView` Avalonia
- [ ] A.2.2 — Gérer `DragOver` (highlight du dossier cible)
- [ ] A.2.3 — Appeler `MoveResource` / `CopyResource` selon la touche Ctrl
- [ ] A.2.4 — Rafraîchir les nœuds source et destination
- **Réf WinForms** : `Maestro.Base/UI/SiteExplorer.cs` lignes 907-932

### [ ] A.3 — Copy Resource ID to Clipboard

- [ ] A.3.1 — Commande dans le context menu
- [ ] A.3.2 — Copier le ResourceId dans le clipboard système
- **Réf WinForms** : `SiteExplorer/CopyResourceIdCommand.cs`

### [ ] A.4 — Duplicate Resource

- [ ] A.4.1 — Commande "Duplicate" qui crée une copie `_copy` dans le même dossier
- **Réf WinForms** : `SiteExplorer/DuplicateResourceCommand.cs`

### [ ] A.5 — Resource Properties dialog

- [ ] A.5.1 — Dialog affichant : ResourceId, type, date création/modification, taille, header XML
- [ ] A.5.2 — Édition des permissions (header resource)
- **Réf WinForms** : `SiteExplorer/ResourcePropertiesCommand.cs`, `Maestro.Base/UI/ResourcePropertiesDialog.cs`

### [ ] A.6 — Find & Replace dans le XML

- [ ] A.6.1 — Dialog Find/Replace avec regex
- [ ] A.6.2 — Opérer sur le XML brut d'une ou plusieurs ressources
- **Réf WinForms** : `SiteExplorer/FindReplaceXmlContentCommand.cs`

### [ ] A.7 — Commandes additionnelles du Site Explorer

- [ ] A.7.1 — `CompileFullDependencyList` — afficher l'arbre de dépendances complet
- [ ] A.7.2 — `SaveResourceContentToDisk` — exporter le XML vers un fichier local
- [ ] A.7.3 — `SetupFolderStructure` — créer la structure standard Library://
- [ ] A.7.4 — `ShowSpatialContexts` — afficher les contextes spatiaux d'un FeatureSource
- [ ] A.7.5 — `PurgeFeatureSourceCache` — vider le cache FDO
- [ ] A.7.6 — `TestResourceCompatibility` — tester la compatibilité avec une autre version
- [ ] A.7.7 — `RepointCommand` — re-pointer les références d'un FeatureSource
- [ ] A.7.8 — `MigrateResourceCommand` — migrer une ressource vers un autre serveur
- [ ] A.7.9 — `EditResourceHeader` — éditer le XML header directement

---

## Phase B — Éditeurs de ressources — fonctionnalités manquantes

> Les éditeurs actuels sont fonctionnels mais basiques.
> Le WinForms original a des capacités bien plus riches.

### [ ] B.1 — FeatureSource : Extensions & Joins

- [ ] B.1.1 — Onglet Extensions (FDO joins, calculated properties)
- [ ] B.1.2 — Dialog de configuration des extensions
- [ ] B.1.3 — Preview des données (data grid avec FeatureReader)
- **Réf WinForms** : `Maestro.Editors/FeatureSource/ExtensionsCtrl.cs`

### [ ] B.2 — FeatureSource : Coordinate System Override

- [ ] B.2.1 — Onglet CS Override pour surcharger le système de coordonnées
- **Réf WinForms** : `Maestro.Editors/FeatureSource/CoordSysOverrideCtrl.cs`

### [ ] B.3 — LayerDefinition : Style Rules complet

- [ ] B.3.1 — Éditeur de Point/Line/Area rules avec symbologie
- [ ] B.3.2 — Color picker intégré
- [ ] B.3.3 — Expression builder (FDO filter expressions)
- [ ] B.3.4 — Theme generation wizard
- **Réf WinForms** : `Maestro.Editors/LayerDefinition/` (25+ fichiers)

### [ ] B.4 — MapDefinition : fonctionnalités avancées

- [ ] B.4.1 — Extent editor (saisie/calcul des bornes)
- [ ] B.4.2 — Watermark configuration
- [ ] B.4.3 — Tile set configuration (base map groups)
- [ ] B.4.4 — Preview carte (rendu serveur via RuntimeMap)
- **Réf WinForms** : `Maestro.Editors/MapDefinition/`

### [ ] B.5 — Éditeurs spécialisés restants

- [ ] B.5.1 — **DrawingSource** editor (DWF)
- [ ] B.5.2 — **LoadProcedure** editor (SDF/SHP/SQLite/DWG load)
- [ ] B.5.3 — **PrintLayout** editor (DWF print templates)
- [ ] B.5.4 — **WatermarkDefinition** editor
- [ ] B.5.5 — **TileSetDefinition** editor (MGOS 3.0+)
- **Réf WinForms** : `Maestro.Editors/DrawingSource/`, `LoadProcedure/`, etc.

### [ ] B.6 — Expression Builder

- [ ] B.6.1 — Dialog modale avec auto-complétion des noms de propriétés
- [ ] B.6.2 — Fonctions FDO disponibles (spatial, string, math, aggregate)
- [ ] B.6.3 — Validation de syntaxe en temps réel
- **Réf WinForms** : `Maestro.Editors/Common/ExpressionEditor.cs`

### [ ] B.7 — Preview de ressources

- [ ] B.7.1 — Rendu serveur pour MapDefinition (GetMapImage)
- [ ] B.7.2 — Preview WMS pour LayerDefinition
- [ ] B.7.3 — Preview de symboles (SymbolDefinition → image)
- **Réf WinForms** : `Maestro.Editors/Preview/`, `Maestro.Base/Commands/PreviewResourceCommand.cs`

---

## Phase C — Commandes globales de l'application

> `Maestro.Base/Commands/` contient 34 commandes. Maestro.Next en implémente ~10.

### [ ] C.1 — Packaging (créer/charger/éditer des .mgp)

- [ ] C.1.1 — Dialog "Create Package" — sélection de ressources → fichier .mgp
- [ ] C.1.2 — Dialog "Load Package" — charger un .mgp vers le serveur
- [ ] C.1.3 — Dialog "Edit Package" — modifier un .mgp existant
- [ ] C.1.4 — Barre de progression avec annulation
- **Réf** : `Maestro.Packaging/PackageBuilder.cs`, `Maestro.Base/Commands/CreatePackageCommand.cs`

### [ ] C.2 — Save As / Save All

- [ ] C.2.1 — "Save As" — sauvegarder sous un autre ResourceId
- [ ] C.2.2 — "Save All" — sauvegarder tous les documents dirty
- **Réf WinForms** : `SaveResourceAsCommand.cs`, `SaveAllCommand.cs`

### [ ] C.3 — Edit as XML (vue XML de n'importe quelle ressource)

- [ ] C.3.1 — Commande "Edit as XML" qui ouvre un second onglet XML brut
- [ ] C.3.2 — Détection de conflit si l'éditeur natif et l'XML sont ouverts simultanément
- **Réf WinForms** : `XmlEditCommand.cs`, `Maestro.Base/Editor/XmlEditor.cs`

### [ ] C.4 — View XML Changes (diff avant/après)

- [ ] C.4.1 — Dialog diff side-by-side entre la version serveur et les modifications locales
- **Réf WinForms** : `ViewXmlChangesCommand.cs`, `Maestro.Editors/Diff/`

### [ ] C.5 — Site Administration

- [ ] C.5.1 — Gestion des utilisateurs et groupes
- [ ] C.5.2 — Gestion des sessions actives
- [ ] C.5.3 — Server logs viewer
- **Réf WinForms** : `SiteAdministratorCommand.cs`

### [ ] C.6 — Map Publishing

- [ ] C.6.1 — Batch publish de tuiles (MgTileSeeder intégré)
- **Réf** : `MgTileSeeder/`, `Maestro.MapPublisher/`

### [ ] C.7 — Profiling de ressources

- [ ] C.7.1 — Mesure de performance de rendu d'une MapDefinition
- **Réf WinForms** : `ProfileResourceCommand.cs`

### [ ] C.8 — Tip of the Day

- [ ] C.8.1 — Dialog au démarrage avec tips (données dans `Data/TipOfTheDay/en.xml`)
- **Réf** : `TipOfTheDayCommand.cs`

---

## Phase D — Infrastructure & Architecture

### [ ] D.1 — Fichier solution (.sln) propre

- [ ] D.1.1 — Créer un `Maestro.sln` contenant :
  - `Maestro.Next`
  - `Maestro.Next.Tests`
  - `OSGeo.MapGuide.MaestroAPI`
  - `OSGeo.MapGuide.ObjectModels`
  - `OSGeo.FDO.Expressions`
  - `OSGeo.MapGuide.ExtendedObjectModels`
  - `Maestro.Packaging` (lib cross-platform)
- [ ] D.1.2 — Solution folders pour organiser (Core, App, Tests, Legacy)

### [ ] D.2 — Supprimer les projets WinForms (quand prêt)

- [ ] D.2.1 — Retirer de la solution :
  - `Maestro` (WinForms exe) → remplacé par `Maestro.Next`
  - `Maestro.Base` → logique migrée dans `Maestro.Next/ViewModels/`
  - `Maestro.Editors` → migrée dans `Maestro.Next/Views/Editors/`
  - `Maestro.Login` → `LoginWindow.axaml`
  - `Maestro.Shared.UI` → plus nécessaire
  - `Maestro.MapViewer` → à recréer pour Avalonia (Phase B.7)
  - `Maestro.AddInManager` → plus nécessaire (pas de système AddIn)
  - `Maestro.AddIn.*` (5 projets) → intégrés directement
  - `Thirdparty/ICSharpCode.*` (3 projets) → remplacé par Avalonia
  - `LocalConfigure`, `SignMapGuideApi` — utilitaires legacy
  - `RtMapInspector`, `MaestroFsPreview`, `Maestro.LiveMapEditor` — outils Windows
- [ ] D.2.2 — Supprimer `publish.bat` et le répertoire `out/`
- [ ] D.2.3 — Mettre à jour `.gitignore`

### [ ] D.3 — Migrer Maestro.Packaging vers cross-platform

- [ ] D.3.1 — Vérifier que `Maestro.Packaging` ne dépend pas de WinForms
- [ ] D.3.2 — L'ajouter comme référence dans `Maestro.Next.csproj`
- [ ] D.3.3 — Créer les ViewModels de packaging (Phase C.1)

### [ ] D.4 — Système de plugins (remplacer ICSharpCode.Core)

- [ ] D.4.1 — Décider : plugins .NET (assembly loading) ou intégration directe ?
- [ ] D.4.2 — Si plugins : Interface `IMaestroPlugin` + `PluginLoader` + discovery
- [ ] D.4.3 — Migrer la logique de `Maestro.AddIn.ExtendedObjectModels` (enregistrement des sérialiseurs)
- [ ] D.4.4 — Migrer la logique de `Maestro.AddIn.Rest` (connexion REST)
- [ ] D.4.5 — Migrer la logique de `Maestro.AddIn.Scripting` (IronPython)
- **Note** : Pour une v1, l'intégration directe (sans AddIns) est suffisante.

---

## Phase E — Tests & Qualité

### [~] E.1 — Tests unitaires (39 existants, objectif ~100+)

- [✓] E.1.1 — DocumentManagerViewModel (5 tests)
- [✓] E.1.2 — NewResourceViewModel (6 tests)
- [✓] E.1.3 — OptionsViewModel (3 tests)
- [✓] E.1.4 — ResourceEditorFactory (8 tests)
- [✓] E.1.5 — ValidationIssueViewModel (4 tests)
- [ ] E.1.6 — SiteExplorerViewModel (load, search, open, refresh)
- [ ] E.1.7 — MainWindowViewModel (connect, disconnect, save, close)
- [ ] E.1.8 — LoginViewModel (validation, connection)
- [ ] E.1.9 — ServerInfoViewModel (refresh, display)
- [ ] E.1.10 — PreferencesService (load, save, theme apply)
- [ ] E.1.11 — ConnectionService (connect, disconnect, state change)

### [ ] E.2 — Tests d'intégration

- [ ] E.2.1 — Test de connexion HTTP vers un serveur MapGuide de test
- [ ] E.2.2 — Test CRUD de ressources (create, read, update, delete)
- [ ] E.2.3 — Test de validation de ressources
- **Prérequis** : Serveur MapGuide accessible en CI (docker ou instance de test)

### [✓] E.3 — Roslyn Analyzers

- [✓] E.3.1 — `AnalysisLevel=latest-Recommended`
- [✓] E.3.2 — `Meziantou.Analyzer`
- [✓] E.3.3 — `xunit.analyzers`
- [✓] E.3.4 — `.editorconfig` complet
- [✓] E.3.5 — 0 violations dans Maestro.Next et tests

---

## Phase F — Packaging & Distribution

### [~] F.1 — Publish profiles

- [✓] F.1.1 — win-x64 (single-file, self-contained)
- [✓] F.1.2 — linux-x64
- [✓] F.1.3 — osx-x64
- [ ] F.1.4 — win-arm64
- [ ] F.1.5 — linux-arm64
- [ ] F.1.6 — osx-arm64

### [✓] F.2 — CI/CD GitHub Actions

- [✓] F.2.1 — Build multi-plateforme
- [ ] F.2.2 — Exécution des tests dans le CI
- [ ] F.2.3 — Upload des artifacts (publish output)
- [ ] F.2.4 — Release automatique sur tag

### [ ] F.3 — Packaging natif

- [ ] F.3.1 — Windows : MSI ou MSIX
- [ ] F.3.2 — Linux : AppImage ou .deb/.rpm
- [ ] F.3.3 — macOS : .app bundle dans .dmg
- [ ] F.3.4 — Flatpak / Snap (optionnel)

---

## Phase G — UX & Accessibilité

### [ ] G.1 — Localisation (i18n)

- [ ] G.1.1 — Extraire toutes les chaînes UI dans des fichiers `.resx`
- [ ] G.1.2 — Support français (traduction existante dans `Localization/`)
- [ ] G.1.3 — Sélecteur de langue dans les Options

### [ ] G.2 — Accessibilité

- [ ] G.2.1 — `AutomationProperties.Name` sur tous les contrôles interactifs
- [ ] G.2.2 — Navigation clavier complète (Tab order)
- [ ] G.2.3 — Contraste suffisant dans les deux thèmes
- [ ] G.2.4 — Support lecteur d'écran (Avalonia `AutomationPeer`)

### [ ] G.3 — UX polish

- [ ] G.3.1 — Icônes SVG pour les types de ressources (remplacer les emoji)
- [ ] G.3.2 — Splash screen au démarrage
- [ ] G.3.3 — Recent connections (historique des serveurs)
- [ ] G.3.4 — Dirty indicator dans le titre de la fenêtre
- [ ] G.3.5 — Confirmation avant fermeture si documents non sauvegardés
- [ ] G.3.6 — Barre de recherche globale (Ctrl+P style VS Code)

---

## Résumé par priorité

| Priorité | Phase | Effort estimé | Impact |
|---|---|---|---|
| 🔴 Critique | D.1 Fichier .sln | 30 min | Bloquant pour le dev |
| 🔴 Critique | B.3 LayerDef style rules | 3-5 jours | Fonctionnalité #1 de Maestro |
| 🔴 Critique | B.6 Expression builder | 2 jours | Utilisé partout |
| 🟠 Haute | A.1 Clipboard | 2h | UX de base |
| 🟠 Haute | A.2 Drag-and-drop | 3h | UX de base |
| 🟠 Haute | C.1 Packaging | 2 jours | Workflow principal |
| 🟠 Haute | C.2 Save As / Save All | 2h | UX de base |
| 🟠 Haute | D.4 Plugins / intégration | 2-3 jours | ExtendedObjectModels, REST |
| 🟡 Moyenne | B.1 FeatureSource extensions | 1 jour | Fonctionnel avancé |
| 🟡 Moyenne | B.4 MapDef avancé | 2 jours | Preview, watermarks |
| 🟡 Moyenne | B.7 Preview ressources | 2-3 jours | Rendu carte |
| 🟡 Moyenne | C.3-C.4 XML edit/diff | 1 jour | Dev workflow |
| 🟡 Moyenne | E.1 Tests supplémentaires | 1 jour | Qualité |
| 🟢 Basse | B.5 Éditeurs restants | 3 jours | LoadProc, Print, etc. |
| 🟢 Basse | C.5 Site Admin | 2 jours | Admin seulement |
| 🟢 Basse | D.2 Nettoyage WinForms | 1 jour | Après validation complète |
| 🟢 Basse | F.3 Packaging natif | 2 jours | Distribution |
| 🟢 Basse | G.1-G.3 i18n/A11y/Polish | 3-5 jours | Polish |

**Effort total estimé : ~30-40 jours-développeur** pour atteindre la parité fonctionnelle complète avec le WinForms.

**Effort pour un MVP utilisable : ~8-10 jours** (Phases A.1-A.2, B.3, B.6, C.1-C.2, D.1).
