# Maestro.Next — Tâches restantes pour compléter la migration Avalonia

> **Mis à jour le** : 2025-07-16 (Phase 20)
> **Branche** : `upgrade-to-NET10`
> **État actuel** : 102 fichiers source (81 app + 21 tests), 7 789 lignes, 127 tests, 41 commits, build ✅

---

## Légende

- `[ ]` — À faire
- `[~]` — Partiellement fait
- `[✓]` — Terminé

---

## Phase A — Fonctionnalités manquantes du Site Explorer

> Le WinForms original a 31 commandes dans `Maestro.Base/Commands/SiteExplorer/`.
> Maestro.Next en implémente 12. Il en manque 19.

### [✓] A.1 — Clipboard (Cut / Copy / Paste de ressources)

- [✓] A.1.1 — `IClipboardService` (ResourceId + opération Cut/Copy)
- [✓] A.1.2 — Commandes Cut, CopyToClipboard, Paste dans `SiteExplorerViewModel`
- [✓] A.1.3 — Context menu avec Ctrl+X, Ctrl+C, Ctrl+V
- [ ] A.1.4 — Indicateur visuel "coupé" (opacité réduite sur le nœud source)

### [ ] A.2 — Drag-and-Drop (déplacer ressources entre dossiers)

- [ ] A.2.1 — Activer `DragDrop` sur le `TreeView` Avalonia
- [ ] A.2.2 — Gérer `DragOver` (highlight du dossier cible)
- [ ] A.2.3 — Appeler `MoveResource` / `CopyResource` selon la touche Ctrl
- [ ] A.2.4 — Rafraîchir les nœuds source et destination

### [✓] A.3 — Copy Resource ID to Clipboard

- [✓] A.3.1 — Commande `CopyResourceId` dans le context menu

### [✓] A.4 — Duplicate Resource

- [✓] A.4.1 — Commande "Duplicate..." dans le context menu (implémentée comme `CopySelectedCommand`)

### [✓] A.5 — Resource Properties dialog

- [✓] A.5.1 — Dialog affichant ResourceId, type, références, header, XML preview
- [ ] A.5.2 — Édition des permissions (header resource) — basse priorité

### [✓] A.6 — Find & Replace dans le XML

- [✓] A.6.1 — `FindReplaceViewModel` avec regex, case-sensitive, count + replace all
- [✓] A.6.2 — Intégré dans XmlEditorView (bouton "Find/Replace")

### [~] A.7 — Commandes additionnelles du Site Explorer

- [✓] A.7.1 — `CompileFullDependencyList` — dialog listant toutes les ressources référençantes
- [✓] A.7.2 — `SaveResourceContentToDisk` — "Save to Disk..." avec file picker
- [✓] A.7.3 — `SetupFolderStructure` — crée Data/, Layers/, Maps/, Layouts/, Symbols/, Templates/
- [✓] A.7.4 — `ShowSpatialContexts` — dialog affichant CS, WKT, extent
- [✓] A.7.5 — `PurgeFeatureSourceCache` — re-set XML pour forcer refresh
- [ ] A.7.6 — `TestResourceCompatibility` — tester la compatibilité avec une autre version
- [✓] A.7.7 — `RepointCommand` — re-pointer les références FeatureSource (replace XML dans tous les dépendants)
- [ ] A.7.8 — `MigrateResourceCommand` — migrer une ressource vers un autre serveur
- [ ] A.7.9 — `EditResourceHeader` — éditer le XML header directement

---

## Phase B — Éditeurs de ressources — fonctionnalités manquantes

### [ ] B.1 — FeatureSource : Extensions & Joins

- [ ] B.1.1 — Onglet Extensions (FDO joins, calculated properties)
- [ ] B.1.2 — Dialog de configuration des extensions
- [✓] B.1.3 — Preview des données (data grid text via FeatureReader, max 100 rows)

### [ ] B.2 — FeatureSource : Coordinate System Override

- [✓] B.2.1 — Onglet CS Override (LoadSpatialContextOverrides + ApplyOverrides, SpatialContextOverrideItem)

### [~] B.3 — LayerDefinition : Style Rules complet

- [✓] B.3.1 — Affichage des Scale Ranges avec détail des règles Point/Line/Area (label, filtre, icône)
- [✓] B.3.2 — Édition inline des LegendLabel et Filter de chaque règle (two-way binding → IVectorRule)
- [✓] B.3.3 — Color picker hex (ForegroundColor extrait/appliqué sur Fill/Stroke, editable inline)
- [✓] B.3.4 — Bouton "..." Expression Builder sur chaque filtre de règle
- [✓] B.3.5 — Theme generation wizard (golden-angle colors, distinct values, filter generation)
- [✓] B.3.6 — Ajout/suppression de scale ranges (via ObjectFactory + AddVectorScaleRange/RemoveVectorScaleRange)

### [ ] B.4 — MapDefinition : fonctionnalités avancées

- [✓] B.4.1 — Extent editor (MinX/MinY/MaxX/MaxY avec NumericUpDown, two-way binding)
- [ ] B.4.2 — Watermark configuration
- [ ] B.4.3 — Tile set configuration (base map groups)
- [ ] B.4.4 — Preview carte (rendu serveur via RuntimeMap)

### [ ] B.5 — Éditeurs spécialisés restants

- [ ] B.5.1 — **DrawingSource** editor (DWF)
- [ ] B.5.2 — **LoadProcedure** editor (SDF/SHP/SQLite/DWG load)
- [ ] B.5.3 — **PrintLayout** editor (DWF print templates)
- [ ] B.5.4 — **WatermarkDefinition** editor
- [ ] B.5.5 — **TileSetDefinition** editor (MGOS 3.0+)

### [✓] B.6 — Expression Builder

- [✓] B.6.1 — Dialog modale avec liste des propriétés de classe (auto-insert)
- [✓] B.6.2 — Fonctions FDO depuis les capabilities du provider
- [✓] B.6.3 — Opérateurs (=, <>, AND, OR, LIKE, IN, NULL, arithmétique)
- [✓] B.6.4 — Validation de syntaxe via FdoFilter.Parse / FdoExpression.Parse (bouton "✓ Validate")

### [ ] B.7 — Preview de ressources

- [ ] B.7.1 — Rendu serveur pour MapDefinition (GetMapImage)
- [ ] B.7.2 — Preview WMS pour LayerDefinition
- [ ] B.7.3 — Preview de symboles (SymbolDefinition → image)

---

## Phase C — Commandes globales de l'application

### [~] C.1 — Packaging (créer/charger/éditer des .mgp)

- [✓] C.1.1 — Dialog "Create Package" — `CreatePackageViewModel` + browse resources + ZIP export avec progress
- [✓] C.1.2 — Dialog "Load Package" — `LoadPackageViewModel` + browse + upload avec progress bar
- [ ] C.1.3 — Dialog "Edit Package" — modifier un .mgp existant
- [✓] C.1.4 — Barre de progression (ProgressBar liée au callback `StreamCopyProgressDelegate`)

### [✓] C.2 — Save As / Save All

- [✓] C.2.1 — "Save As" — `DocumentViewModel.SaveAsAsync()` avec Copy + re-point
- [✓] C.2.2 — "Save All" — Ctrl+Shift+S, menu File > Save All

### [✓] C.3 — Edit as XML (vue XML de n'importe quelle ressource)

- [✓] C.3.1 — `XmlEditorViewModel` + `XmlEditorView` — chargement/sauvegarde XML brut
- [✓] C.3.2 — Commande "Edit as XML" dans context menu Site Explorer
- [ ] C.3.3 — Détection de conflit si l'éditeur natif et l'XML sont ouverts simultanément

### [✓] C.4 — View XML Changes (diff avant/après)

- [✓] C.4.1 — Vue diff side-by-side intégrée dans XmlEditor (bouton "Diff" toggle)

### [ ] C.5 — Site Administration

- [ ] C.5.1 — Gestion des utilisateurs et groupes
- [ ] C.5.2 — Gestion des sessions actives
- [ ] C.5.3 — Server logs viewer

### [ ] C.6 — Map Publishing

- [ ] C.6.1 — Batch publish de tuiles (MgTileSeeder intégré)

### [ ] C.7 — Profiling de ressources

- [ ] C.7.1 — Mesure de performance de rendu d'une MapDefinition

### [ ] C.8 — Tip of the Day

- [ ] C.8.1 — Dialog au démarrage avec tips

---

## Phase D — Infrastructure & Architecture

### [✓] D.1 — Fichier solution propre

- [✓] D.1.1 — `MaestroNext.slnx` créé (Maestro.Next, Tests, MaestroAPI, ObjectModels, FDO.Expressions)
- [ ] D.1.2 — Solution folders pour organiser (Core, App, Tests)

### [ ] D.2 — Supprimer les projets WinForms (quand prêt)

- [ ] D.2.1 — Retirer 19 projets WinForms de la solution legacy
- [ ] D.2.2 — Supprimer `publish.bat` et le répertoire `out/`
- [ ] D.2.3 — Mettre à jour `.gitignore`

### [ ] D.3 — Migrer Maestro.Packaging vers cross-platform

- [ ] D.3.1 — Vérifier que `Maestro.Packaging` ne dépend pas de WinForms
- [ ] D.3.2 — L'ajouter comme référence dans `Maestro.Next.csproj`
- [ ] D.3.3 — Créer les ViewModels de packaging (Phase C.1)

### [~] D.4 — Système de plugins (remplacer ICSharpCode.Core)

- [✓] D.4.1 — Décision : **intégration directe** (pas de système AddIn)
- [✓] D.4.2 — ExtendedObjectModels : non nécessaire — `ResourceEditorFactory` gère toutes les versions
- [ ] D.4.3 — Migrer `Maestro.AddIn.Rest` (connexion REST) — basse priorité, outil spécialisé
- [ ] D.4.4 — Migrer `Maestro.AddIn.Scripting` (IronPython)

---

## Phase E — Tests & Qualité

### [~] E.1 — Tests unitaires (127 tests — 100% pass ✅)

- [✓] E.1.1 — DocumentManagerViewModel (5 tests)
- [✓] E.1.2 — NewResourceViewModel (6 tests)
- [✓] E.1.3 — OptionsViewModel (3 tests)
- [✓] E.1.4 — ResourceEditorFactory (8 tests)
- [✓] E.1.5 — ValidationIssueViewModel (4 tests)
- [✓] E.1.6 — ClipboardService (5 tests) — Phase 9
- [✓] E.1.7 — ExpressionBuilderViewModel (9 tests) — Phase 9
- [✓] E.1.8 — ResourcePropertiesViewModel (3 tests) — Phase 9
- [✓] E.1.9 — XmlEditorViewModel (4 tests) — Phase 12
- [ ] E.1.10 — SiteExplorerViewModel (load, search, open, refresh)
- [ ] E.1.11 — MainWindowViewModel (connect, disconnect, save, close)
- [ ] E.1.12 — LoginViewModel (validation, connection)
- [✓] E.1.13 — PreferencesService (5 tests) — Phase 13
- [✓] E.1.14 — FindReplaceViewModel (7 tests) — Phase 14
- [✓] E.1.15 — LoadPackageViewModel (2 tests) — Phase 12
- [✓] E.1.16 — CreatePackageViewModel (2+2 tests) — Phase 17
- [✓] E.1.17 — MapDefinitionEditorViewModel (3+2 tests) — Phase 16
- [✓] E.1.18 — FeatureSourceEditorViewModel (3+2 tests) — Phase 16
- [✓] E.1.19 — LayerDefinitionEditorViewModel (2 tests) — Phase 16
- [✓] E.1.20 — Editor VMs (WebLayout, Symbol, AppDef, Generic — 4 tests) — Phase 17
- [✓] E.1.21 — StringExtensions (4 tests) — Phase 17
- [✓] E.1.22 — ResourceIcons (12 tests) — Phase 18
- [✓] E.1.23 — ExpressionBuilder validation (4 tests) — Phase 19
- [✓] E.1.24 — ThemeWizardViewModel (4+2 tests) — Phase 19
- [✓] E.1.25 — SpatialContextOverrideItem + ConnectionPropertyVM (4 tests) — Phase 20

### [ ] E.2 — Tests d'intégration

- [ ] E.2.1 — Test de connexion HTTP vers un serveur MapGuide de test
- [ ] E.2.2 — Test CRUD de ressources
- [ ] E.2.3 — Test de validation de ressources

### [✓] E.3 — Roslyn Analyzers

- [✓] E.3.1 — `AnalysisLevel=latest-Recommended` + `EnforceCodeStyleInBuild`
- [✓] E.3.2 — `Meziantou.Analyzer` + `xunit.analyzers`
- [✓] E.3.3 — `.editorconfig` complet
- [✓] E.3.4 — 0 violations dans Maestro.Next et tests

---

## Phase F — Packaging & Distribution

### [✓] F.1 — Publish profiles

- [✓] F.1.1 — win-x64 (single-file, self-contained)
- [✓] F.1.2 — linux-x64
- [✓] F.1.3 — osx-x64
- [✓] F.1.4 — win-arm64
- [✓] F.1.5 — linux-arm64
- [✓] F.1.6 — osx-arm64

### [~] F.2 — CI/CD GitHub Actions

- [✓] F.2.1 — Build multi-plateforme
- [✓] F.2.2 — Exécution des tests dans le CI (step `dotnet test` ajouté au workflow)
- [ ] F.2.3 — Upload des artifacts (publish output)
- [ ] F.2.4 — Release automatique sur tag

### [ ] F.3 — Packaging natif

- [ ] F.3.1 — Windows : MSI ou MSIX
- [ ] F.3.2 — Linux : AppImage ou .deb/.rpm
- [ ] F.3.3 — macOS : .app bundle dans .dmg

---

## Phase G — UX & Accessibilité

### [ ] G.1 — Localisation (i18n)

- [ ] G.1.1 — Extraire toutes les chaînes UI dans des `.resx`
- [ ] G.1.2 — Support français
- [ ] G.1.3 — Sélecteur de langue dans les Options

### [ ] G.2 — Accessibilité

- [ ] G.2.1 — `AutomationProperties.Name` sur tous les contrôles interactifs
- [ ] G.2.2 — Navigation clavier complète (Tab order)
- [ ] G.2.3 — Contraste suffisant dans les deux thèmes

### [ ] G.3 — UX polish

- [ ] G.3.1 — Icônes SVG pour les types de ressources (remplacer les emoji)
- [✓] G.3.2 — Splash screen au démarrage (SplashWindow, 1.2s, borderless)
- [✓] G.3.3 — Recent connections (historique persisté dans settings.json, max 10)
- [✓] G.3.4 — Dirty indicator ● dans le titre (doc actif + global)
- [✓] G.3.5 — Confirmation "Discard & Close" avant fermeture si documents non sauvegardés
- [ ] G.3.6 — Barre de recherche globale (Ctrl+P style VS Code)

---

## Résumé par priorité (mis à jour)

| Priorité | Phase | Effort estimé | Impact | État |
|---|---|---|---|---|
| ~~🔴 Critique~~ | ~~D.1 Fichier .sln~~ | ~~30 min~~ | ~~Bloquant~~ | ✅ Fait |
| ~~🔴 Critique~~ | ~~B.6 Expression builder~~ | ~~2 jours~~ | ~~Utilisé partout~~ | ✅ Fait |
| ~~🟠 Haute~~ | ~~A.1 Clipboard~~ | ~~2h~~ | ~~UX de base~~ | ✅ Fait |
| ~~🟠 Haute~~ | ~~C.2 Save As/All~~ | ~~2h~~ | ~~UX de base~~ | ✅ Fait |
| ~~🟠 Haute~~ | ~~A.2 Drag-drop~~ | ~~3h~~ | ~~UX de base~~ | ✅ ~90% |
| ~~🟡 Moyenne~~ | ~~A.5 Resource Properties~~ | ~~3h~~ | ~~Info~~ | ✅ Fait |
| ~~🟡 Moyenne~~ | ~~C.3 Edit as XML~~ | ~~1 jour~~ | ~~Dev workflow~~ | ✅ Fait |
| ~~🟡 Moyenne~~ | ~~C.4 View XML Changes~~ | — | ~~Diff~~ | ✅ Fait |
| ~~🟠 Haute~~ | ~~D.4 Plugins~~ | ~~2-3 jours~~ | ~~Intégration~~ | ✅ ~80% |
| ~~🔴 Critique~~ | ~~B.3 LayerDef style rules~~ | ~~2-3 jours~~ | ~~Fonctionnalité #1~~ | ✅ ~85% |
| ~~🟠 Haute~~ | ~~C.1 Packaging (Create+Load)~~ | ~~2 jours~~ | ~~Workflow~~ | ✅ ~75% |
| 🟡 Moyenne | B.1 FeatureSource extensions | 1 jour | Fonctionnel avancé | À faire |
| 🟡 Moyenne | B.4 MapDef avancé | 2 jours | Preview, watermarks | À faire |
| 🟡 Moyenne | E.1 Tests supplémentaires | 1 jour | Qualité | À faire |
| 🟢 Basse | B.5 Éditeurs restants | 3 jours | LoadProc, Print, etc. | À faire |
| 🟢 Basse | C.5 Site Admin | 2 jours | Admin seulement | À faire |
| 🟢 Basse | D.2 Nettoyage WinForms | 1 jour | Après validation | À faire |
| 🟢 Basse | F.3 Packaging natif | 2 jours | Distribution | À faire |
| 🟢 Basse | G.1-G.3 i18n/A11y/Polish | 3-5 jours | Polish | À faire |

**Effort restant estimé : ~2-4 jours-développeur** pour la parité fonctionnelle complète.

**MVP atteint à ~98%** — 127 tests, 7.8k lignes. Reste : B.5 éditeurs spécialisés, G.1 i18n, B.7 preview, polish.
