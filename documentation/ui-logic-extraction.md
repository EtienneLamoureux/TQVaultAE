# Logic trapped in the GUI: inventory and extraction plan

Inventory made on 2026-11-08 over `src/TQVaultAE.GUI` (Forms, Components, Dialogs, Tooltips, Models; Designer files and the
Debug-only `Automation` folder excluded). Line numbers are those of that date and have drifted since: check them before
moving code.

The goal: the GUI only displays and takes input. Rules, state changes, validation and text built from game data move to
`Application` (contracts, DTOs, results, session state), `Services` (orchestration), `Domain` (pure rules on the entities) or
`Presentation` (text). Once moved, they can be tested without Windows Forms.

## End state (lot 10 done)

All ten lots are done: the commits from `7b420c1` to `e1fba5c` (`git log --oneline 7b420c1..HEAD`). Each inventory row below
has a status: **done**, **partial** (what is left is noted) or **left**. What remains in the GUI on purpose is listed in
[What stays in the GUI](#what-stays-in-the-gui).

## What is already out of the GUI

`Domain/Actions/ItemActionRules`, `AutoMovePlanner`, `EquipmentRules`, `Domain/Forge/ForgeRules` and `ForgeMerge`,
`LootTableCollection.PickWeighted`, `TQColorHelper.SplitColorSegments`, `Application/Results/CsvRow`, `IMessageBoxService` and
`IUIService`, and the services `ItemMovementService`, `ItemStackingService`, `ItemDropService`, `ItemEditService`,
`AffixCatalogService`, `ItemExchangeService`, `VaultService`, `StashService`, `PlayerService`, `ForgeService`,
`HighlightService`, `SaveOrchestrator`, `LoadAllService`, `StartupService`, `SettingsPersistenceService`, `SettingsEditor`,
`SearchCategoryService`, `SearchQueryService`, `CharacterEditService`, with `ItemTooltipContentBuilder`, `ItemPropertiesContent`, `PlayerInfoText` and the
container label helpers in Presentation.

## Cross-cutting findings

- `GuiServices`, the bundle every form and control receives, exposed four Data providers (`IDatabase`, `IItemProvider`,
  `IPlayerCollectionProvider`, `ITQDataService`). **Partial**: `IDatabase`, `IPlayerCollectionProvider` and `ITQDataService` are
  gone (`e1fba5c`); `IItemProvider` stays, see [What stays in the GUI](#what-stays-in-the-gui).
- Some Services code was written but unused while the GUI kept its own copy. **Done**: `ItemMovementService` is used
  (`eae894c`), `VaultService.DeleteVault/RenameVault/GetVaultList` (`bd4a2ef`, `b993c50`) and `ItemDatabaseService.WarmUpAsync`
  are used. `PlayerService.CreatePlayerFileWatcher` and `StashService.Create*FileWatcher` are removed: they watched
  `LastWrite | Size` (several events per write) on a thread pool thread, while the GUI watchers
  (`MainForm.Player.CreateFileWatcher`, the stash watchers of `MainForm`) watch `Attributes`, which changes once after the
  game writes, and raise on the UI thread.
- Message boxes asked in the middle of a rule (`goto askAgain`, Abort/Retry/Ignore loops). **Done** for the moved code: it
  returns a result or takes a callback and the GUI asks.
- Session state kept in form fields or combo selections belongs to `SessionContext`. **Partial**: the startup selection and
  load-all state moved (`bca066e`); some flags (`configChanged`, `resourcesLoaded`) are still form fields.

## Bugs found during the inventory

All fixed in lot 1.

| Where | Bug | Status |
|---|---|---|
| `Dialogs/ItemSeedDialog.cs:104` | Accepts `> 0 && < 0x7fff`: 32767 is refused though the message says 1–32767; `Item.GenerateSeed` returns 0–32766, so the Random button can give 0, refused by OK. | done `2a8035b` |
| `Components/Panels/VaultPanel.ContextMenu.cs:385-418` | `CopyBagClicked`: `if (SuppressWarnings \|\| !Confirm) return;` never copies onto a non-empty bag when warnings are suppressed (it should copy without asking). | done `5bf2613` |
| `Components/Character/ComboBoxCharacter.cs` | Archive and unarchive are written three times each; the two "unarchive all" paths do not update the player file path (only `:813` does). | done `27e033c` |
| `Forms/MainForm.Loading.cs:30-134` | Progress totals disagree: `LoadAllFilesTotal` counts 2c+v-1, `total` c+v-1, and 2c+v steps are reported. | done `8096fc7` |
| `Forms/MainForm.Lifecycle.cs`, `MainForm.Loading.cs:140-159` | The vault and stash save errors (`vaultOnError`, `stashOnError`) are discarded, never reported. | done `b1ea29c` |
| `Forms/MainForm.Vault.cs:97-98` | `break` in the vault list loop, probably meant as `continue`. | done `1f43c3a` |
| `Dialogs/SettingsDialog*` | Game paths are upper-cased on Browse, not when typed. | done `6ecb32a` |
| `Program.cs:278-285` | Dead duplicated `else if` in `SetUILanguage`. | done `4624f23` |

Found on the way and fixed too: `554d9f8` (completing a relic with shards to spare duplicated it), `d59b9c4` (Clear
selection from an empty cell), `29d6385` (exceptions escaping the async void startup and dialogs).

## Inventory by theme

Risk: low (pure code, few callers), med (state or threading), high (core drag and drop path).

### Items: moves, stacking, editing

| # | Where | What | Target | Risk | Status |
|---|---|---|---|---|---|
| I1 | `SackPanel.DragDrop.cs:235-252`, `SackPanel.ContextMenuHandlers.cs:636-653, 673-692` | Weighted bonus roll from a loot table, copied 3 times | `LootTableCollection.PickWeighted` (Domain), random source injected | low | done `a47dc0e` |
| I2 | `SackPanel.DragDrop.cs:212-318` | Relic/charm and potion stacking, relic completion | `ItemStackingService` (Services) | med | done `0c7c2cb`, fix `554d9f8` |
| I3 | `SackPanel.DragDrop.cs:407-422`, `SackPanel.Geometry.cs:69-186`, `MainForm.Panel.cs:429-459` | Place, find, update location: copies of `ItemMovementService` | `ItemMovementService` (settle the `Place.Name` difference first) | low | done `eae894c`; `SackPanel.FindAllItems/FindOpenCells/IsCellAvailable` stay, they go through the virtual `FindItem` of the equipment panel |
| I4 | `SackPanel.SackManagement.cs:32-141`, `SackPanel.Selection.cs:96-151` | Autosort, moving a selection: cell maths only | `ItemMovementService` | med | done `1f10ac4` |
| I5 | `SackPanel.ContextMenuHandlers.cs:346-727` | Change affixes and bonuses, complete relic, craft artifact, split, copy, seed; dispatch on localized menu text | `ItemEditService` + dispatch on `ItemActions` | low-med | done `94358ca`; new set member `e1fba5c` |
| I6 | `SackPanel.ContextMenu.cs:208-497, 628-665` | Affix, bonus and set member catalogs (LINQ) | `AffixCatalogService` (Application/Services) | low | done `bdfaaf9` |
| I7 | `SackPanel.ContextMenu.cs:22-81`, `ContextMenuHandlers.cs:114-201`, `SackManagement.cs:148-275` | Auto-move destinations and the "sort by size, move each" loop, written 3 times | `ItemActionRules.MoveDestinations`, `AutoMovePlanner` | low / med | done `df96660` |
| I8 | `EquipmentPanel.DragDrop.cs:454-511` (slot rules), `:150-258` (2H/1H swap) | Equipment rules | `EquipmentRules` (Domain) | low / high | done `f778717`: slot/class check, two-handed weapon swaps, shadow slot, slot geometry, take-out of an equipped item (from `ItemDragInfo.MarkPlaced`); `EquipmentPanel.IsWeaponSlot` replaced by `SackCollection.IsWeaponSlot` |
| I9 | `SackPanel.DragDrop.cs:96-210` | Drop rules | `ItemMovementService.TryDrop` | high, last | done `bb54638` as `ItemDropService.Decide` and `Drop`; the equipment panel keeps its own drop flow (slot and requirement checks) on top of `EquipmentRules` |
| I10 | `SackPanel.cs:568-605`, `EquipmentPanel.cs:59-85`, `StashPanel.PlayerInfo.cs:32-105` | Requirements met, gear and skill stat bonuses, player info lines | `PlayerService`, a Presentation builder | low | done `1bbd2ff` |

### Vaults, characters, stashes

| # | Where | What | Target | Risk | Status |
|---|---|---|---|---|---|
| V1 | `MainForm.Vault.cs:197-336`, `VaultMaintenanceDialog.cs:217-396` | New, copy, rename, delete vault and their validation (3 copies) | `VaultService` (`ValidateMaintenance`, `CopyVault`, fixed `Rename/Delete`) | low-med | done `bd4a2ef` |
| V2 | `MainForm.Vault.cs:67-158` | Vault list: Main Vault first, the other panel's vault excluded | `VaultService.BuildVaultChoices` | low | done `b993c50` |
| V3 | `VaultPanel.ContextMenu.cs:30-84, 219-418` | Bag copy/move/merge, CSV rows (2 copies), icon info copy (2 copies) | `VaultService`, `BagButtonIconInfo.Clone` | low | done `9fc20fe` |
| V4 | `MainForm.Player.cs:59-87, 331-392` | Character list, new name validation, duplication with rollback | `PlayerService` | low-med | done `0d74d9d` |
| V5 | `ComboBoxCharacter.cs:617-867` | Archive/unarchive (6 copies), tag name validation | `GameFileService`/`PlayerService`, `TagService` | low | done: archive/unarchive `27e033c`; tag name check in `TagService.ValidateTagName` |
| V6 | `MainForm.Player.cs:201-296`, `MainForm.Stash.cs:46-130` | Load error → message mapping, copied 5 times | a load facade returning results + messages | low | done `186297d` |
| V7 | `StashPanel.cs:384-614`, `StashPanel.BagButtons.cs:113-206`, `BagButtonBase.cs:245-263` | Bag id clamping, tooltip settings switch (3 copies) | `StashService`, a tooltip policy over `UserSettings` | low | done `a10f6bd` |

### Startup, saving, settings

| # | Where | What | Target | Risk | Status |
|---|---|---|---|---|---|
| S1 | `MainForm.Lifecycle.cs:54-89`, `MainForm.Loading.cs:140-159`, save wrappers in `MainForm.Player/Vault/Stash` | Save order (players, vaults, stashes, config, git), retry loop | `ISaveOrchestrator` returning a `SaveResult` | med | done `19dc5cd` |
| S2 | `MainForm.Loading.cs:371-517` | Configuration persistence, restart decision after settings | `SettingsPersistence`, pure `RestartReason Evaluate(...)` | low | done `d3d1bf7` |
| S3 | `MainForm.Loading.cs:30-134, 280-362` | Load all, startup selection (last character, last vault) | `ILoadAllService`, `StartupSelection.Resolve` | med | done `bca066e` |
| S4 | `Program.cs:218-336`, `Models/CommandLineArgs.cs` | Game paths, UI culture, map name, command line parsed twice | `StartupService`; `CommandLineArgs(string[])` in Application | med | done `c835c0d` |
| S5 | `SettingsDialog.cs`, `SettingsDialog.EventHandlers.cs` (~750 lines) | Change flags set by hand in 28 handlers; rules between settings (cheats off clears edits, item edit off clears epic affixes, git off clears legacy backup...) | `SettingsEditor` over a copy of `UserSettings`, flags from a comparison | med | done `342b5be`; the game language comes from `SettingsPersistenceService.GameLanguage` (`e1fba5c`) |
| S6 | `SettingsDialog.cs:296-307, 547`, `UserSettings.cs:170`, `PasteBinService.cs:30` | PasteBin default expiration written 3 times; git URL check | constants and validator in Services/Config | low | done `8b94e9d` |
| S7 | `MainForm.Export.cs:99-366`, `MainForm.Clipboard.cs:38-97` | Import placement, import as new vault, text import dispatch (3 copies) | `ItemExchangeService` | low | done `b99803c`; PasteBin item export name `e1fba5c` |
| S8 | `CharacterEditDialog.cs:86-374` | Masteries by level, attribute and skill refunds, money floor; `int.Parse` throws on bad XP text | `CharacterEditService`, `PlayerLevel` helpers | med | done `bfa4182`; mastery names `e1fba5c` |

### Search and tooltips

| # | Where | What | Target | Risk | Status |
|---|---|---|---|---|---|
| T1 | `SearchDialogAdvanced.Populate.cs:42-437` | 17 copies of the category builder (one uses `TQCleanup`, the others `RemoveAllTQTags`) | `SearchCategoryService` | med | done `c294785` |
| T2 | `SearchDialogAdvanced.Filters.cs:195-222, 487-514` | AND/OR combination of filter results, min ≤ max | `ItemDatabaseService`, `AdvancedSearchRequest` | low | done `2b57fa4` |
| T3 | `SearchDialogAdvanced.Queries.cs`, `Models/Search/SearchQueries.cs` | Saved queries: validation, JSON storage in the GUI, `BoxItem` holding controls | `SearchQueryService` + DTO in Application | med | done `5f703e6` |
| T4 | `ResultsDialog.cs:107-131`, `FoundResultsTooltip.cs:135-145`, `BagButtonTooltip.cs:128-140` | Container labels, "N x name" summary (2 copies) | Presentation helpers | low | done `56c6d39` |
| T5 | `ItemTooltip.cs:17-452`, `ItemTooltip.GrantedSkills.cs:95-277` | Which lines the item tooltip shows and in what order | `ItemTooltipContentBuilder` (Presentation) returning lines; the GUI renders | med-high | done `4a6cf0c` (checked with `tooltip_text_dump`, `cc28db1`) |
| T6 | `BaseTooltip.cs:92-124`, `ItemProperties.cs:89-178` | Color tag splitting (2 copies), property sections (3 copies) | `TQColorHelper.SplitColorSegments` (Domain), `ItemPropertiesContent` (Presentation) | low | done: color tag splitting `07b7e86`; property sections built once by `ItemPropertiesContent.Build` (rows, colors, visibility), the dialog loops over them (checked with `item_properties_dump` on 9367 items, both filter states: identical) |
| T7 | `ForgePanel.cs:484-686` | Forge commit, consuming materials, reset on mode downgrade | `ForgeService`, `ForgeRules.IsDowngrade` | low-med | done `4f861e0`; preview item `e1fba5c` |
| T8 | `HighlightFilters.cs:195-385` | Building the filter values, "any filter active" | `HighlightFilterValues.IsEmpty`, `HighlightService` | low | done `9f42903` |

### Second sweep (lot 11)

A second sweep after lot 10 (2026-11-08) found these. Line numbers are from that day.

| # | Where | What | Target | Risk | Status |
|---|---|---|---|---|---|
| L1 | `MainForm.Player.cs:201-282`, `MainForm.Stash.cs:46-110`, `MainForm.Loading.cs:234-235` | 4 nearly identical character load wrappers, each with the rule "a TQ original character has no player, transfer or relic stash"; the relic stash one does not log its error | `PlayerService`/`StashService.LoadCharacterSession` returning one result | med | to do |
| L2 | `MainForm.Panel.cs:263-408, 436-438` | Auto-move re-implements placement (stash availability, free cells, add, location) and builds the container name from the file name, unlike `SackPanel.PlaceHere` | `ItemMovementService.TryPlace`, stash rule in `AutoMovePlanner` | med-high | to do |
| L3 | `VaultPanel.cs:315-349, 427-447` | `CurrentBag` setter writes the selected bag into the vault and marks it modified; `AssignSacks` clamps and restores the bag | `VaultService.RememberSelectedBag` / `ResolveInitialBag` | low-med | to do |
| L4 | `StashPanel.cs:382-481`, `StashPanel.BagButtons.cs:86-91`, `StashPanel.PlayerInfo.cs:40` | 3 copied bag setters re-set `StashType`; a copy of `StashService.ClampBagId`; equipment bag found by comparing translated text | `StashService` bag descriptor; compare the bag id | low | to do |
| L5 | `MainForm.Vault.cs:75-101` | Copy of the `StartupSelection` vault rule; `VaultFolderChanged` updates the path from the UI | `StartupSelection` / `VaultService.ResolveVaultToLoad` | low | done `1dcac29` |
| L6 | `ComboBoxCharacter.cs:681-823`, `ComboBoxCharacterItem.cs:136` | Archive/unarchive all, globally and per tag: 4 copies of the selection and loop; tags loaded in the display refresh | `PlayerService.SetArchived(saves, archive, tag)`; tags loaded with the list | low | to do |
| L7 | `BagButtonSettings.cs:268-292, 382-391, 561-621` | Bag icon validation, building `BagButtonIconInfo`, simple-mode rule, icon cell de-duplication; marks the sack modified when nothing changed | `BagButtonIconInfo.Create` + `Validate` | low | to do |
| L8 | `CharacterEditDialog.cs:165-182, 263-264`, `ComboBoxCharacterItem.cs:108-121` | Mastery summary computed twice; "XP = level minimum + 1" rule in the dialog | `CharacterEditService` / `PlayerInfoText`; `PlayerLevel.LevelStartXp` | low | to do |
| L9 | `SackPanel.ContextMenuHandlers.cs:501-515`, `ItemSeedDialog.cs:100-113` | The seed dialog sets the seed and `IsModified`; the panel compares old and new seed | `ItemEditService.SetSeed` | low | to do |
| L10 | `SackPanel.ContextMenuHandlers.cs:184-284`, `MainForm.Clipboard.cs:11-34`, `VaultPanel.ContextMenu.cs:141-191` | Single item vs selection serialization in 4 handlers; `File.WriteAllText` in the GUI; PasteBin tab name built in the panel | `ItemExchangeService.SerializeSelection` / `ExportToFile` / `ExportTabToPasteBinAsync` | low | to do |
| L11 | `VaultPanel.ContextMenu.cs:31-43`, `Models/CsvDelimiter.cs`, `SettingsDialog.cs:414-432` | `CsvDelimiter` and its parsing in the GUI; the settings dialog reads the saved value, not the one edited | `CsvDelimiter` in Config/Application; `VaultService.BuildCsvExport` reads the setting | low | to do |
| L12 | `MainForm.Search.cs:94-147` | Search result → panel and bag index arithmetic; unreachable `StashType` branch | `SearchResultNavigation.Resolve` | low-med | to do |
| L13 | `ForgePanel.cs:230-289` | Mode cards hard-code what each forge mode allows | `ForgeRules.Capabilities(mode)` | low | to do |
| L14 | `SackPanel.cs:572-579`, `SackPanel.Input.cs:44`, `SackPanel.SackManagement.cs:98, 131`, `DragDrop.cs:111-117`, `EquipmentPanel.DragDrop.cs:119` | Read-only rule computed 3 times; `IsCurrentPlayerReadOnly` used only by commented-out code | One rule (`ItemActionRules` context or `PlayerService.IsSackReadOnly`); dead code removed | low | to do |

## What stays in the GUI

`IItemProvider` is the last Data provider in `GuiServices`. Its uses read or cache the display text of an item, deep in the
tooltip rendering; a pass-through service would only rename the dependency:

| Where | Call | Why it stays |
|---|---|---|
| `Tooltips/BaseTooltip.cs` and its subclasses (`ItemTooltip`, `BagButtonTooltip`, `BagButtonLabelTooltip`, `FoundResultsTooltip`, `SearchFiltersTooltip`) | `GetFriendlyNames`, `GetSetItems` | The item and bag tooltips render the friendly names; the line content is already built in Presentation (`ItemTooltipContentBuilder`). |
| `Dialogs/ItemProperties.cs` (through `VaultForm.ItemProvider`) | `GetFriendlyNames(item, ItemFullDisplay, filterExtra)` | Shows the raw record properties of an item; display only. |
| `Components/Panels/SackPanel.cs` (`InvalidateItemCacheAll`, `InvalidateItemCacheItemTooltip`) | `InvalidateFriendlyNamesCache` | Goes with the tooltip caches the panel clears after an item change. |
| `Automation/*` (Debug only) | `IItemProvider`, `IDatabase` from the container | The automation endpoint reads the app's state as it is. |
| `Program.cs` | `IDatabase.LoadDBFile` | Composition root: loads the game database once the game paths are known. |

Also left in the GUI, by design: the drag state (`ItemDragInfo`), the cell and screen geometry of the panels, the equipment
panel's drop flow on top of `EquipmentRules` (slot under the mouse, item requirement setting, repaint of the shadow slot), and
the message boxes.

## Order of the lots

Each lot is one commit (or a few), with unit tests on the moved code, and is checked in the running app through the
`tqvault` MCP.

1. **Bugs** found above, each with a test where the code allows it. Done.
2. **Quick wins, low risk**: I1, I3, I6, T4, T6, V5, V6, S6. Done.
3. **Vaults and characters**: V1, V2, V3, V4, V7. Done.
4. **Item editing and moves**: I5, I7, I10, T7, T8, I2, I4. Done.
5. **Import/export and startup**: S7, S2, S4, S3. Done.
6. **Saving**: S1 (data safety: extra care, tests on the order and on the error reporting). Done.
7. **Search**: T2, T1, T3. Done.
8. **Settings and character edit**: S5, S8. Done.
9. **Item tooltip content**: T5. Done.
10. **Last**: I8 swap, I9 drop rules; then remove the Data providers from `GuiServices`. Done (`IItemProvider` kept).
11. **Second sweep**: L1-L14, one worktree session each, merged one by one.
