# TQVaultAE Agent Guidelines

## Build Commands

```bash
# Build solution (Any CPU)
dotnet build TQVaultAE.slnx
dotnet build TQVaultAE.slnx --configuration Release

# Build specific project
dotnet build src/TQVaultAE.GUI/TQVaultAE.GUI.csproj

# Restore packages
dotnet restore
```

**Solution File:** `TQVaultAE.slnx` (modern .NET 10.0 SDK-style format)

## Test Commands

The tests use xunit v3 on Microsoft.Testing.Platform (selected in `global.json`): `dotnet test` takes `--solution` or `--project`, and filters are xunit options.

There is one test project per layer, `src/tests/TQVaultAE.<Layer>.Tests`, plus `src/tests/TQVaultAE.Tests.Win32` for the
Windows-only projects. To check a change, run the test projects of the layers it touches, not the whole solution:

| Changed project | Test project to run |
|-----------------|---------------------|
| TQVaultAE.Domain, TQVaultAE.GameFiles | TQVaultAE.Domain.Tests (and the layers above when an API changes) |
| TQVaultAE.Application | TQVaultAE.Application.Tests |
| TQVaultAE.Config | TQVaultAE.Config.Tests |
| TQVaultAE.Data | TQVaultAE.Data.Tests |
| TQVaultAE.Services | TQVaultAE.Services.Tests (and TQVaultAE.Services.Git.Tests for `GitBackup`, slow: real git, about 3 min) |
| TQVaultAE.Presentation | TQVaultAE.Presentation.Tests |
| TQVaultAE.DbCli | TQVaultAE.DbCli.Tests |
| TQVaultAE.Logs | TQVaultAE.Logs.Tests |
| TQVaultAE.Presentation.Win32, TQVaultAE.Services.Win32, ARZExplorer, TQSaveFilesExplorer | TQVaultAE.Tests.Win32 |

The test projects share their settings through `src/tests/TestProject.props`. A lower layer's change can break the tests of
the layers above it: run the whole solution before a commit that changes a shared API.

```bash
# Run all tests
dotnet test --solution TQVaultAE.slnx

# Run the tests of one layer
dotnet test --project src/tests/TQVaultAE.Services.Tests

# Run single test class
dotnet test --project src/tests/TQVaultAE.Services.Tests --filter-class "*TagServiceTests"

# Run single test method
dotnet test --project src/tests/TQVaultAE.Services.Tests --filter-method "*TagServiceTests.AddTag_WithNewTagName_ReturnsTrue"

# Run with coverage (coverlet.MTP, written to TestResults/; src/tests/testconfig.json limits it to our assemblies)
dotnet test --solution TQVaultAE.slnx --coverlet --coverlet-output-format opencover

# Build before testing
dotnet build src/tests/TQVaultAE.Services.Tests/TQVaultAE.Services.Tests.csproj --configuration Debug
dotnet test --project src/tests/TQVaultAE.Services.Tests --no-build
```

## Code Style

### Formatting
- **Indentation**: Tabs
- **Braces**: Allman style (opening brace on its own line), as in the existing code
- **Namespaces**: File-scoped
- **This Prefix**: Use `this.` for all instance members

### Naming Conventions
| Element | Convention | Example |
|---------|------------|---------|
| Classes/Interfaces | PascalCase | `ItemService`, `IItemService` |
| Private fields | camelCase with underscore | `_itemCount` |
| Interfaces | Prefix with `I` | `IFileIO` |

### Types & Nullable
- Enable nullable reference types: `<Nullable>enable</Nullable>`
- Use `?` for nullable parameters
- Initialize collections as empty rather than null

### String Literals
- Use C# raw string literals (`"""..."""`) for:
  - JSON strings (replaces `\"` escaping)
  - File paths (replaces `\\` or `@""` escaping)
  - Multi-line strings with `\n`/`\t`
- Keep regular `"..."` for short InlineData parameters and trivial single-word strings

### Async Patterns
- Use `async`/`await` for I/O operations
- Name async methods with `Async` suffix: `LoadPlayerAsync()`
- Do not block on async code (avoid `.Result`, `.Wait()`)

### Error Handling
- Prefer Result types over exceptions for expected failures
- Log exceptions with context: `Log.LogError(ex, "Failed to load {Name}", name)`

## Testing

### Framework
- **xUnit** for tests, **AwesomeAssertions** for assertions, **Moq** for mocking

### What is a Meaningful Test? ⚠️ IMPORTANT

**AVOID trivial tests that add NO value:**
- ❌ Property getter/setter without logic (`*_CanBeSet`)
- ❌ Default value tests without behavior
- ❌ Constructor verification (`new Class()`)
- ❌ `x.Should().Be(y)` with constant values

**WRITE meaningful tests that verify:**
- ✅ Business logic and calculations
- ✅ Error handling and edge cases
- ✅ Interactions between components
- ✅ State transitions and side effects

**Rule:** If you can't answer "What bug would this test catch?" → DELETE it

### Cross-Platform Testing
- Use `Path.GetTempPath()` for paths
- Mock `IPathIO` for path control
- DirectoryNotFoundException handling differs on Linux

## Architecture

### Layer Dependencies
```
GUI → Presentation → Services → Domain → Data
     ↓
Application (DTOs, Results, Search)
```

### Projects
| Project | Framework | Purpose |
|---------|-----------|---------|
| TQVaultAE.GUI | .NET 10.0 | Windows Forms UI |
| TQVaultAE.Services | .NET 10.0 | Business logic |
| TQVaultAE.Application | .NET 10.0 | DTOs, Results |
| TQVaultAE.Data | .NET 10.0 | Data access |
| TQVaultAE.Domain | .NET 10.0 | Entities |
| TQVaultAE.Presentation | .NET 10.0 | Localized UI strings (`Resources`), translation |
| TQVaultAE.Presentation.Win32 | .NET 10.0 Windows | `System.Drawing` bitmaps, fonts, icons, tooltip image cache, UI images (`Images`) |
| TQVaultAE.GameFiles | .NET 10.0 | Game file readers (`.arz`, `.arc`, text), shared by Data and `tqdb` |
| TQVaultAE.DbCli | .NET 10.0 | `tqdb` command line |
| TQVaultAE.<Layer>.Tests | .NET 10.0 | Unit tests, one project per layer (Domain, Application, Config, Data, Services, Presentation, DbCli, Logs) |
| TQVaultAE.Tests.Win32 | .NET 10.0 Windows | Unit tests of the Windows-only projects |

Keep `System.Drawing` (except `Color`, `Point`, `Size`, `Rectangle`) and other Windows-only APIs in the `*.Win32` projects and the GUI, so the other projects stay cross-platform.

### Key Abstractions (for DI)
- `IFileIO` - File operations
- `IPathIO` - Path operations
- `IGamePathService` - Game path resolution
- Register services in `src/TQVaultAE.GUI/Program.cs`

## Adding New Code

### Adding a Service
1. Create interface in `src/TQVaultAE.Application/contracts/Services/`
2. Implement in `src/TQVaultAE.Services/`
3. Use `IFileIO`/`IPathIO` for file operations
4. Register in `Program.cs`
5. Add tests with meaningful assertions

### Adding a Test
1. Create test class in the test project of the tested class's layer: `src/tests/TQVaultAE.<Layer>.Tests/<Name>Tests.cs`
2. Mock all dependencies in constructor
3. Follow Arrange-Act-Assert pattern
4. Verify actual behavior, not property assignments

## Verifying GUI Changes (`tqvault` MCP)

Debug builds of `TQVaultAE.GUI` host an MCP server inside the app at `http://127.0.0.1:47017/mcp`. It is registered for Claude Code in `.mcp.json`. Use it to check UI changes on the running app, instead of screenshots and real mouse clicks.

1. Build Debug: `dotnet build TQVaultAE.slnx`.
2. Run `src/TQVaultAE.GUI/bin/Debug/net10.0-windows/TQVaultAE.exe`. Prefer `TQVaultAE.exe --headless`: it runs on a hidden desktop, so no window, taskbar button or focus change reaches the user. Message boxes then take their default answer (Cancel/No/Ignore/OK) unless one is queued, and native file/folder dialogs are cancelled. Add `TQVAULT_MCP_PORT=47018` when another Debug instance already uses 47017. See "Headless mode" in `documentation/AUTOMATION.md`.
3. Use the `mcp__tqvault__*` tools:

| Purpose | Tools |
|---|---|
| Dialog controls | `control_values(form)`, `set_control(form, control, value)`, `click_control(form, control)`, `mouse_click(form, control, x, y)` (mouse messages at a point, e.g. a check group box caption), `open_character_editor` (shows the editor in place of the main content; form `Character Editor`, closed with `close_dialog`), `skill_panel` / `skill_panel_click` / `skill_tooltip` (character editor skill panes), `character_panel` / `character_attribute_click` / `character_tooltip` / `character_stat_sources` (its Character page and the sums behind its computed statistics), `select_tab`, `grid_values` / `set_grid_cell`, `tag_box` / `list_tags`, `list_items` / `check_list_items` / `grid_click` for check box lists and grids (search dialog, results), `list_keys` / `list_state` (keyboard and width of a check box list), `search_dialog_state` / `search_dialog_scroll` (the Advanced Search dialog's whole user state, kept across zoom changes), `archive_character`, `character_read_only` (the character menu's *Read-only* toggle and the files' read-only attribute) and `open_mod_link` (mod link dialog): test characters only (`claude-`, `player-test`, `player-mod`) |
| Message boxes | `queue_dialog_answers(answers)` answers the next boxes without showing them; `dialog_answers` lists them, and with `--headless` also the boxes that took their default answer and the native dialogs cancelled; `close_dialog` for the others |
| Windows and dialogs | `list_windows`, `close_dialog` (for example "Default Vault Path" → OK; also the settings, a view of the main window shown by `click_control("", "configureButton")`: "Settings" → OK or Cancel, and the form `Settings` for `control_values`, `set_control`, `click_control`, `control_tree`; the same for the character editor view, "Character Editor"), `list_forms`, `open_form(name, show?)` to create any registered form or dialog, `open_item_dialog(panel, x, y, dialog)` for an item's Properties or Seed dialog, `open_bag_settings(panel)` for the bag icon picker |
| Read the state from the app's models | `get_state(panel?)`, which returns characters, vaults, panels and items with their cells; `character_list(refresh?)` for the character list entries as drawn (class, masteries, mod, badges) and the selected character's box; `mod_state` for the active custom map (mod), the loaded databases and each character's mod link; `mod_suggest(character)` for the mod suggestion; `search_containers(filter?)` for the containers of the search database and their items; `select_search_result(container, x?, y?)` shows a search result like the results dialog; `player_info` for the player information panel as displayed, with the attributes the item requirements are checked against (`player_stats_hover(tab, index)` hovers one of its rollovers and reports where the tool tip lands); `player_stats_fonts` for its fonts; `stash_background_switch(bag)` to check that the stash backgrounds replaced at a tab switch are disposed; `owned_skill_items` for every owned item that grants a skill; `raw_tooltip_lines` for the tooltip lines still shown raw; `tooltip_text_dump(path, views)` for the rows of every item tooltip as rendered (text, color, bold, kind), written to a JSON file to diff; `item_properties_dump(path)` for the sections of the Item Properties dialog of every item; `compare_items(panel, x, y, x2, y2, panel2?, alignRows?, hideUnchanged?, showSources?, toggle?)` for the four columns of the item comparison dialog (rows and a PNG; `toggle` clicks stats open or closed to show their sources); `compare_click(panel, x, y, button?)` for the second click of the compare selection started by the item menu's `Compare` |
| Navigation | `select_character`, `select_vault` (its first entry opens *Maintain Vault Files*), `select_bag` |
| Test files | `create_vault(name)` and `duplicate_character(name)`, which only accept names starting with `claude-`; `save_files` like the Save button (answer its error box with `close_dialog` from another call); `export_vault(path)`, `export_items(panel, x, y, path)` (the item menu's `Share to File...`) and `import_file(path)` (into a `claude-` vault) without the file dialogs; `import_text(text?, path?)` pastes like Ctrl+V without touching the clipboard |
| Drag and drop through the panels' own handlers | `pick_item`, `move_drag`, `drop_item`, `cancel_drag`; `create_item` drags a new item built from record ids |
| Keyboard and modifier clicks | `press_key(panel, x, y, key)` posts real key messages to the focused window (message loop, key preview, `KeyDown`) with the modifiers in the keyboard state; `click_cell(panel, x, y, modifiers)`, `drag_select(panel, x1, y1, x2, y2, modifiers)` and `select_items(panel, cells)` click with modifiers held; `key_binds()` reads the bindings, `key_binds_dialog(...)` drives the key binds dialog with real clicks and keys |
| Item tooltips | `hover_item(panel, x, y)`, which returns the tooltip as a PNG; `hover_end`; `hover_bag(panel, bag)` for a bag button's tooltips (summary and custom label, with their sizes and fonts; `capture("tooltip:BagButtonLabelTooltip")`) |
| Context menus | `context_menu(panel, x, y)`, which returns the entries of the right-click menu and closes it; `context_menu_click(panel, x, y, path)` clicks one (`Move Item To > Bag #3`); `vault_menu(path?)` reads or clicks the menu of the vault tools button (`Modded vault > shadowchampions`); `capture_menu(target, x?, y?, open?)` renders a menu with its opened sub menus to PNG (vault tools, character tools, a cell's menu, or `<panel>.sort` for a sort button's menu); menu entries give `Checked` and the right-hand `Hint`; `select_items(panel, cells)` Ctrl-clicks a selection; `press_key(panel, x, y, key)` presses a key through the window's then the panel's handler (c, d, Backspace, any gesture; empty panel for the forge keys); `click_cell(panel, x, y, modifiers)` clicks with modifiers held (Ctrl+Alt duplicates, Alt copies by default); `key_binds` / `key_binds_dialog` read the bindings and drive the key binds dialog; `bag_menu(panel, bag, path?)` for the bag buttons, `bag_settings_pick_icon` in the icon picker; `sort_menu(panel, path?)` reads or picks the sort order of a sort button (vault, player, playerBag, stash tabs), `sort_click(panel)` sorts like a left click |
| Highlight | `highlight_filters(check, uncheck, apply)` ticks the highlight filter check boxes and applies them |
| Layout and scaling | `zoom(step)` (in, out, reset); `control_tree(form, depth)` for control bounds and fonts; `text_fit(form)` for text cut by its control (check each UI language); `move_window(x, y, form?)` to move the window or a dialog to a monitor of another DPI; `window_bounds()` for the window state, whether the panels' table fits inside the border (`layout`) and the largest zoom Windows lets the window take (`largestZoom`); `window_state(state)` to minimize, restore (as from the taskbar) or maximize it |
| Forge | `forge_show`, `forge_state`, `forge_set_mode`, `forge_mode_card(mode)` (the text and PNG of a mode card), `forge_set_hardcore`, `forge_drop(slot)`, `forge_select_part`, `forge_drag_cursor`, `forge_hover(slot, hide?)` (a slot's tooltip, for Base the preview, drawn on a PNG of the main window), `forge_reset`, `forge_forge`, `forge_cancel` |
| Evidence | `capture(control, shown?)`, a PNG made with `DrawToBitmap` that works even when the window is hidden (`shown=true` copies the pixels the window shows on its desktop instead) (`capture("playerInfo")` for the player information panel, whose labels `DrawToBitmap` leaves out); `recent_log(n)`; `recent_sounds(n)`, with the sounds played and whether their data loaded; `picture_fault(resourceId, mode)` makes a picture read as missing or damaged (in memory) to check the placeholder of an item whose picture cannot be loaded |

Rules:
- Nothing blocks saves: work only in your own test files, vaults `claude-vault1`, `claude-vault2`, ... and characters `claude-character1`, ... made with Duplicate Character. Never change a real character or vault (see Safety in `documentation/AUTOMATION.md`).
- Put items back with `cancel_drag`.
- `PlayerReadonly` blocks picking from and dropping on the equipment panel. A read-only character (its save files carry the read-only attribute) blocks all of its bags, equipment and storage area.
- Release builds contain none of this. Keep any new hook inside `#if DEBUG`.

Full reference: `documentation/AUTOMATION.md`.

ARZExplorer has its own Debug endpoint at `http://127.0.0.1:47020/mcp` (`ARZExplorer.exe --headless`, message boxes recorded by `dialogs`): see "ARZExplorer endpoint" in `documentation/AUTOMATION.md`.

## Resources
- Game file formats (ARC, ARZ, text, TEX, item stat jitter): `documentation/tq-file-formats.md`
- DB CLI (`tqdb`) usage: `documentation/DBCLI.md`
- In-app automation endpoint (`tqvault` MCP, Debug only): `documentation/AUTOMATION.md`
- GitHub: https://github.com/EtienneLamoureux/TQVaultAE
- Issues: https://github.com/EtienneLamoureux/TQVaultAE/issues
