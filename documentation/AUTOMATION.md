# Automation endpoint

TQVault hosts an [MCP](https://modelcontextprotocol.io) server inside the application. An AI agent (any MCP client)
uses it to inspect and drive the running UI without screenshots or real mouse clicks. Two levels:

- **User tools**, in every build: reading the state, the user's actions, captures. Release builds serve them only when
  the user turns the endpoint on in the settings (off by default). User guide, with the rules the agent must follow:
  [mcp.md](mcp.md).
- **Developer tools**, in Debug builds only: UI internals probes, raw mouse and keyboard input, diagnostic dumps, fault
  injection, headless mode. See [User and developer tools](#user-and-developer-tools).

This page is the full reference of both.

The item grids (`SackPanel`, `EquipmentPanel`) are owner-drawn: UI Automation sees each as a single pane. The endpoint
reads the app's own models instead, and drives drags through the panels' own mouse handlers.

**What each build contains.**

- Every build: the server (`Automation/`, the `ModelContextProtocol.AspNetCore` package, the ASP.NET Core framework
  reference, `TQVaultAE.Automation`) and the user tools (methods of `AutomationTools`).
- Debug only: the developer tools (methods of the nested class `AutomationTools.DevTools`, in `Automation/Dev/`), the
  headless mode (`Dev/Headless.cs`, `TQVaultAE.Automation/HeadlessDesktop.cs`), the recorded sounds and the in-memory
  log. The accessors and helpers only these need go in `Automation/Dev/` partial files (`<Class>.Dev.Automation.cs`),
  or inside `#if DEBUG` in code outside `Automation/`.
- The test `AutomationToolLevelsTests` holds both lists: a new tool goes in one of them (run it with `-c Release` to
  check a Release build).
- At connection, the server sends the agent the rules of [mcp.md](mcp.md) (the `instructions` field,
  `AutomationInstructions`).

---

## Transport and address

- Transport: MCP streamable HTTP, through the official C# SDK (`ModelContextProtocol.AspNetCore`) on Kestrel.
- Address: `http://127.0.0.1:47017/mcp`, loopback only, without authentication.
- Release builds: starts when the settings turn it on (*MCP endpoint for AI agents*, `EnableMcpServer`, off by default),
  on their port (`McpServerPort`, default 47017, 1024 to 65535). Starts at startup once the settings are read; starts,
  moves or stops at once when they change. The environment variables below are not read.
- Debug builds: starts with the application whatever the settings, before the first dialog, so startup message boxes
  can already be answered; the settings leave it alone.
- The log gives the address at startup: `Automation MCP endpoint listening on http://127.0.0.1:47017/mcp`

| Environment variable (Debug builds) | Effect |
|---|---|
| `TQVAULT_MCP_PORT` | Listen on another port. |
| `TQVAULT_MCP_DISABLED=1` | Do not start the endpoint. |

## Headless mode (`--headless`)

`TQVaultAE.exe --headless` (every build) runs TQVault on a hidden Win32 desktop of its own: no window, taskbar
button or focus change reaches the user while an agent drives it.

- The process started with the switch creates the desktop, starts TQVault again on it, and exits once that instance is
  idle: one `TQVaultAE.exe` is left, and the endpoint comes up as usual (wait about 5 s). An instance restarted by
  Duplicate Character stays on the hidden desktop.
- Every window is still created, shown, laid out and painted at its normal place on the primary monitor, with its DPI:
  `capture`, `hover_item`, `control_tree`, `open_form`, the forge and every other tool work as usual. `list_windows`
  gives the desktop name `headless`.
- Message boxes are never shown:
  - a queued answer (`queue_dialog_answers`) is used first;
  - otherwise the box takes the answer that changes nothing (Cancel, else No, else Ignore, else OK), logged and listed
    by `dialog_answers` under `unanswered`;
  - this covers the startup notices, the startup git sync questions, the vault import dialog (takes Cancel) and the
    "Windows Forms Error" box.
- Native dialogs (folder browser, open/save file, color picker) are cancelled as they open, listed by `dialog_answers`
  under `cancelledNativeDialogs`.
- Fatal errors:
  - a "Windows Forms Error" before the main form has loaded takes Abort (TQVault quits) instead of Ignore;
  - when startup fails (global error box of `Program.Main`), the box is listed under `unanswered`, the endpoint stays up
    10 s for `dialog_answers` and `recent_log` to report it, then the process exits with code 1;
  - without `--headless`, that box is no longer a `DefaultDesktopOnly` box in Debug: `list_windows` and `close_dialog`
    see it, and the process exits 10 s after it is answered.
- Nothing can be seen or clicked by hand. Stop it with the Exit button (`click_control("TQVaultAE", "exitButton")`),
  which saves like a normal exit, or end the process.
- Use another port (`TQVAULT_MCP_PORT`) when a visible Debug instance already listens on 47017.

```bash
TQVAULT_MCP_PORT=47018 src/TQVaultAE.GUI/bin/Debug/net10.0-windows/TQVaultAE.exe --headless
```

Release builds: the endpoint starts whatever the settings (a hidden TQVault is driven through it only), on the settings'
port (`McpServerPort`; the environment variables stay Debug-only), with the user tools. The rules above hold, the
fatal-error grace delay excepted. Agent rules for that case: [mcp.md](mcp.md#headless). ARZExplorer's `--headless`
stays Debug-only (it references `TQVaultAE.Automation` in Debug builds only).

## Register it in an MCP client

The repository's `.mcp.json` already registers it as `tqvault`:

1. Start a Debug build: `dotnet build TQVaultAE.slnx`, then `src/TQVaultAE.GUI/bin/Debug/net10.0-windows/TQVaultAE.exe`.
2. Open your MCP client at the repository root and approve the project server when asked.

The tools show under the server name `tqvault` in sessions started at the repository root. From another folder, add by
hand an HTTP MCP server named `tqvault` with this URL:

```text
http://127.0.0.1:47017/mcp
```

With another port (`TQVAULT_MCP_PORT`), update the URL in `.mcp.json` too.

## User and developer tools

Developer tools are marked *(Debug)*; the others are user tools, served by Release builds too.

- **User tools**:
  - reading the state: characters, vaults, stashes, item search, loadouts, real values, tool tip texts;
  - the user's actions: moving and dropping items, loadouts, the forge, the item menus such as *Search*, vault, bag and
    character menus, files;
  - captures: `capture`, `capture_menu`, and the pictures of `hover_item`, `compare_items`, `forge_mode_card`,
    `skill_tooltip`, `character_tooltip`.
- **Developer tools** *(Debug)*:
  - UI internals: `control_tree`, `control_values`, `text_fit`, `zoom`, `window_bounds`, `window_state`,
    `move_window`, `list_forms`, `open_form`, `open_input_dialog`, the list and grid probes, the Advanced Search dialog
    state;
  - raw input: `set_control`, `click_control`, `mouse_click`, `mouse_wheel`, `press_key`, `drag_select`,
    `key_binds_dialog`;
  - virtual cursor hovers of UI parts: `player_stats_hover`, `forge_hover`, `forge_drag_cursor`,
    `bag_settings_hover_icon`, `bag_settings_drag_icon`;
  - diagnostics: `recent_log`, `recent_sounds`, `load_resource`, `player_stats_fonts`, `stash_background_switch`, `gdi_census`,
    `raw_tooltip_lines`, `tooltip_text_dump`, `item_properties_dump`;
  - item menu flows: `socket_click`;
  - the statistics panels' Buffs tab and weapon set gems: `character_buffs`, `character_buff_toggle`, `character_weapon_set`;
  - the character editor's class sets: `class_sets`;
  - floating character windows: `character_window_open`, `character_window`, `character_window_loadout`, `character_window_list`, `character_window_close`;
  - test aids: `create_item`, `picture_fault`, the `gameRunning` and `realGame` parameters of `loadouts`;
  - the headless mode.

Borderline choices:

| Tool | Level | Why |
|---|---|---|
| `queue_dialog_answers`, `dialog_answers`, `close_dialog`, `list_windows`, `close_form` | User | The user's actions open TQVault's boxes and dialogs (confirmations, the name dialog of the loadout actions and Duplicate Character): the agent answers those the operator agreed to, or closes what it opened. |
| `click_cell` | User | A cell gesture (Alt copies, Ctrl+Alt duplicates) addressed by cell, as the user does it. Raw key messages (`press_key`) and the rubber band (`drag_select`, covered by `select_items`) stay developer tools. |
| `select_tab` | User | Captures of a tab page (a mastery of the character editor). |
| `hover_item`, `hover_bag`, `hover_end` | User | Tool tip texts and pictures. The other hovers probe UI parts whose content other user tools read (`forge_state`, `player_info`, `character_tooltip`). |
| `key_binds` / `key_binds_dialog` | User / Debug | Reading the bindings is harmless; driving the dialog takes real clicks and keys. |
| `open_item_dialog` | User | The item menu's Properties and Seed dialogs. |
| `bag_settings_pick_icon` / `bag_settings_drag_icon` | User / Debug | Same result; the drag is a mouse path through the virtual cursor. |
| `game_text` / `load_resource` | User / Debug | Translating text tags is a harmless read; loading resources is a diagnostic. |
| `create_item` *(Debug)* | Debug | Builds any item from record ids: a test aid, not a TQVault feature. |
| `results_move`, `capture_results_menu` *(Debug)* | Debug | Moving items is a user action, but the tool picks a window's rows and menu entries by index and path; a user tool would rather name the items and the target. Its test-data guard holds in Debug only. |
| `character_attribute_click`, `skill_panel_click` | User | The character editor's own buttons, shown only with *Allow character edit*. |

**Name guard.** The test-name guard of [Safety](#safety) holds in Debug builds only (`AutomationTestNames.Enforced`).
Release builds serve the user's own agent on the user's own files: every name is accepted, and the operator confirms
each change ([mcp.md](mcp.md)).

## Tools

General rules:

- Every UI access is marshalled to the UI thread with `Control.Invoke`. Tools reading state the request thread can read
  safely run on it: `recent_log`, `queue_dialog_answers`, `dialog_answers`, `recent_sounds`, `list_forms`,
  `load_resource`, `picture_fault`, `game_text`, and the native window calls of `list_windows` and `close_dialog`.
- A refusal (hidden entry, greyed button, bag out of range) comes back as a tool error with its reason.
- Panel ids: `vault`, `secondaryVault`, `player` (main inventory), `playerBag`, `stash`, `equipment`, and `loadout`
  (the loadout panel shown in place of the statistics panel once a loadout is chosen in the bar; it exists from the
  first time the Equipment tab shows a character).
- Cells are grid coordinates, as `get_state` reports them.
- Geometry and colours have one JSON format in every result and dump:
  - rectangles `{"x", "y", "width", "height"}` (may carry extra keys after these four, e.g. `visible`), points
    `{"x", "y"}`, sizes `{"width", "height"}`: integers, in the coordinate space each tool states (client pixels of the
    parent, screen pixels, cells...);
  - colours `"#RRGGBB"`, `"#AARRGGBB"` when not opaque, `null` for no colour.
- "After the call returns": the click runs once the tool has answered, since it may open a modal dialog. Queue the
  dialog's answers first (`queue_dialog_answers`), or follow with `list_windows`.
- Menu paths use `' > '` between levels; each level matches an exact text first, else the first entry containing it.
  A path ending on a sub menu, or a disabled entry, is refused.

### Windows and dialogs

- **`list_windows`**: the process' visible top-level windows (WinForms forms and native message boxes): title, class,
  modal state, message text, buttons.
- **`close_dialog(title, button)`**: presses a dialog button by its text, e.g. `("Default Vault Path", "OK")`.
  - Works for message boxes and WinForms forms, even not in the foreground.
  - The settings shown in the main window: `("Settings", "OK")` or `"Cancel"`, pressed after the call returns (OK may ask
    to restart). A message box the settings open, titled like them ("Configure Settings"), is answered first.
  - The character editor shown in the main window: `("Character Editor", "OK")` applies the edits to the character in
    memory, `"Cancel"` drops them. Button texts follow the UI language.
- **`list_forms`** *(Debug)*: the form and dialog types registered in the container.
- **`open_form(name, show)`** *(Debug)*: creates a form or dialog from the container, as `FormFactory` does (its
  `InitializeComponent` and resources run); reports title, size and control count.
  - `show=false` (default): then disposed. `show=true`: opens modeless over the main window, for `capture` and
    `close_dialog`.
  - Singletons (`MainForm`, `BagButtonSettings`) are only reported.
  - Dialogs needing data first may fail when shown; the error goes to `recent_log`.
- **`close_form(form)`**: closes an open modeless form without a button to answer it (`close_dialog` needs one), like
  its close box, e.g. the search results window of `find_copies` or the item menu's *Search*. The main window is
  refused. Returns the title of the form closed.
- **`open_item_dialog(panel, x, y, dialog)`**: opens the `properties` or `seed` dialog of the item at a cell, like its
  context menu entries but modeless, to check its layout with `text_fit` and `capture`. Close it with `close_dialog`.
  The Seed dialog's OK only validates the typed seed (an invalid one shows its error box); to change a seed, use
  `context_menu_click(..., "Seed")`.
- **`open_input_dialog(prompt?, title?, value?)`** *(Debug)*: opens the text input dialog (naming a duplicated
  character or a loadout) modeless, created from the container like the app does (marble look, user zoom, DPI of the
  main window's monitor), to check its layout with `text_fit`, `control_tree` and `capture` (its title). Close it with
  `close_dialog`.
  - The modal dialog of the real flow (*Duplicate Character*, loadout actions) is answered with
    `queue_dialog_answers("text:<name>")`.
  - Left open, `press_key("", 0, 0, key)` reaches its text box (Enter accepts, Escape cancels).
- **`queue_dialog_answers(answers)`**: queues answers (`OK`, `Cancel`, `Yes`, `No`, `Abort`, `Retry`, `Ignore`, comma
  separated) for the next message boxes, answered at once without being shown, so nothing waits for a click (startup
  git sync, confirmations, save errors).
  - A box takes the next answer only when it is one of its buttons; an OK-only box takes only `OK`.
  - `text:<name>` answers the next text input dialog (name asked by *Duplicate Character* and the loadout actions): the
    dialog opens, the text is typed and OK clicked, so its checks run (a refused name shows its error box, then the
    dialog again). `Cancel` cancels it. The text cannot hold a comma.
  - `clear` empties the queue.
  - Covers the startup notices and the vault import dialog: `Yes` replaces the vault, `No` imports it as a new vault,
    `Cancel` cancels.
- **`dialog_answers`**: read only.
  - `answered`: boxes answered from the queue (caption, message, answer); `pending`: answers not used yet.
  - Headless mode, what nobody answered: `unanswered` (boxes that took their default answer, text input dialogs left
    open for want of a queued text; answer those with `close_dialog`, or `set_control` and `click_control`), and
    `cancelledNativeDialogs` (file, folder, color, task dialogs cancelled as they opened).
  - `queue_dialog_answers('clear')` empties it.

### Form controls

- **`control_values(form, labels?)`** *(Debug)*: reads the input controls of an open form, or of the settings shown in
  the main window (form `Settings`): check boxes, radio buttons, numeric boxes, track bars, combo boxes, list boxes,
  text boxes, check group boxes. Name, type, enabled, visible, value.
- **`set_control(form, control, value)`** *(Debug)*: sets one control like a user, so its events run. Returns the form's
  input controls afterwards, to check the dialog's rules.
  - A text box then gets Validating, Validated and Leave, as when the user leaves it.
  - Combo box or list box: an item text or `#index`. A combo box then raises SelectionChangeCommitted, as after a pick
    in its list (the search dialog's *Operator* filters on it); an editable combo box (the search dialog's query name)
    takes any text.
  - Track bar: a number in its range, then Scroll, as after a drag (the settings' item background opacity).
  - Refuses a disabled control and the types it does not set (use `click_control` or `mouse_click`).
- **`click_control(form, control)`** *(Debug)*: clicks a button of an open form, or of the settings shown in the main
  window (form `Settings`), after the call returns. `''` for the main window.
  - `configureButton` shows the settings in place of the main content, or leaves them as their Cancel does (asks first
    when changed); their OK runs the restart rules.
  - While the settings show, the main content sleeps: a held item goes back to its place, the window's shortcut keys are
    off, the toolbar buttons acting on the content are disabled, and game file hot reloads wait for its return.
- **`mouse_click(form, control, x, y, button?, clicks?)`** *(Debug)*: clicks a point of a control with the mouse
  messages Windows sends, to the deepest visible window under it, so the control's own mouse handling, hit testing and
  repaint run as for a user (`click_control` only raises `OnClick`).
  - Move, then button down and up. `clicks=2` adds `WM_LBUTTONDBLCLK` (or `WM_RBUTTONDBLCLK`) and up, so `DoubleClick`
    and `CellDoubleClick` run (results grid, mod link dialog's list).
  - `button="right"` ends with the `WM_CONTEXTMENU` Windows sends after a right click: the control's own
    `ContextMenuStrip` opens (its `Opening`, `SourceControl`).
  - `x`, `y` in the control's client coordinates (bounds from `control_tree`). E.g. the glyph or text of the check box
    in a check group box's caption (`detectGamePathsCheckBox`), the loadout bar (`loadoutBar`: [+] and the name box
    at the bounds `loadouts` reports), the statistics panel (`PlayerStatsPanel`), the character editor's tabs
    (`EditorTabs`), the search dialog's category labels, link labels.
  - Works headless. Runs after the call returns: read the result with `control_values`, `list_windows` or `capture`. A
    menu left open by a right click closes with `press_key` Escape or `menu_click`.
- **`mouse_wheel(form, control, x, y, notches, modifiers?)`** *(Debug)*: turns the wheel over a point of a control:
  `WM_MOUSEWHEEL` (screen point, 120 per notch) sent to the deepest visible window under it, as Windows sends it to the
  window under the cursor. A window not using the wheel passes it to its parents, as for the user.
  - `notches` > 0 turns away from the user (up), < 0 towards (down). `modifiers` (`ctrl`, `shift`, `alt`, or a
    combination like `ctrl+shift`) are held in the keyboard state during the message.
  - Runs before the call returns. Returns the receiver and the scroll position of each scrollable control from it up to
    the form: `x`, `y` of an AutoScroll panel, `topIndex` of a list box, `value` of a visible scroll bar child (e.g. an
    icon grid's). `list_state` reads a check box list's first column.
  - Works headless: the search dialog's lists, the bag settings' icon grids, the character list.
- **`select_tab(form, tab)`**: selects a page (index, name or part of its text) of a tab control of an open form, or of
  the character editor's game-styled frame, for `capture`.
- **`grid_values(form, grid)`** *(Debug)*: reads a `DataGridView` of an open form (`grid` empty for the form's only
  grid): column headers; per row each cell's text (`image` for pictures), the row text color, the cell tool tip.
- **`set_grid_cell(form, grid, row, column, value)`** *(Debug)*: changes a grid cell like a user: a button cell is
  clicked (`CellContentClick`), a text cell edited then left (`CellEndEdit`). Row: index or the text of one of its cells
  (a skill name); column: index or header text. Read the grid again afterwards.
- **`grid_click(form, row, control?, column?, double?)`** *(Debug)*: clicks a data grid row like the user (current row,
  cell click, double click with `double`). In the results dialog, a click selects the character or vault and highlights
  the item. Runs after the call returns.
- **`list_items(form, control?, skip?, take?)`** *(Debug)*: the list controls of an open form: check box lists (the
  search dialog's categories), list boxes, data grids (search results, item properties). Without `control`: each list
  with its entry count and checked entries. With `control`: its entries with their checked or selected state.
- **`check_list_items(form, control, items, check?)`** *(Debug)*: ticks (or unticks) check box list entries like a
  click, so its change event runs (the search dialog updates its filters and result count). Entries by text (exact,
  else containing) or `#index`, separated by `\|`.
- **`list_keys(form, control, keys)`** *(Debug)*: focuses a check box list and posts key presses to it through the
  message loop, like a user typing (WinForms' key pre-processing runs, with the scan code and modifiers held, as
  `press_key` posts them).
  - Keys, separated by `\|`: `up`, `down`, `left`, `right`, `pageup` (`pgup`), `pagedown` (`pgdn`), `home`, `end`,
    `space` (toggles the focus entry: the search dialog updates as after a click), `tab`, or any key `press_key` reads.
  - Runs after the call returns: read the result with `list_state`.
- **`list_state(form, control, widths?)`** *(Debug)*: a check box list's keyboard state: `focused`, the focus entry
  (`focusedIndex`, `focusedText`, `focusedChecked`), `focusRectangleDrawn`, `rowsPerColumn`, `visibleColumns`,
  `firstColumn`, the fully visible entries (`firstVisibleIndex`..`lastVisibleIndex`), `columnWidth`.
  - `widths=true` adds the text width check: the list's measure (`maxTextWidth`, texts measured, ms) against measuring
    every entry (`maxTextWidthAll`, ms), and `largestUnderestimate`, the largest shortfall of a text's per-character
    estimate against its real width (the `margin` must cover it).
- **`search_dialog_state`** *(Debug)*: the user's state of the open Advanced Search dialog, read from its controls:
  - `zoom`, `size`, `layoutSize` (the size its layout gave it; another size is the user's), `windowState`,
    `statePending` (a state given by the dialog it replaces, not restored yet);
  - the search text typed and applied, the category filter, the saved query box (text, selected query), operator,
    reduce, values per category, quick filters, requirements, `selectedFilters`, `categoryOrder`, `hiddenCategories`;
  - per category list: checked entries, `collapsed`, `focusedIndex`, `firstColumn`; the categories area `scroll`;
  - results: `resultCount`, `resultHash` (of the sorted results), `resultsFirst`.
  - The dialog is made again after a zoom change: compare this before closing it and after opening it at the new zoom
    (only `zoom`, `size`, `layoutSize` and `scroll`, scaled, may differ).
- **`search_dialog_scroll(x?, y?, list?, firstColumn?)`** *(Debug)*: scrolls the open Advanced Search dialog like its
  scroll bars: the categories area to `x`, `y` pixels, and/or a category list to its first shown column. Returns
  `search_dialog_state`.
- **`search_tooltip(kind)`** *(Debug)*: shows a tool tip of the open Advanced Search dialog as resting the pointer on
  its label does, through the label's own handler: `filters` (selected filters; select some first with
  `check_list_items`), `results` (results found), `none` hides them. Returns `shown` (false with nothing to show).
  Capture with `capture("tooltip")`.

### State, search and mods

- **`get_state(panel?)`**:
  - the selected vault (with `vaultMod`, the mod it is linked to) and character; the available characters and vaults;
    `PlayerReadonly`;
  - `notification`: the last message shown to the user in the main window (e.g. an import's end message);
  - `view`: the view under the toolbar, `main`, `settings` or `characterEditor`;
  - the drag state; every panel (shown, sack type, bag, grid size, item count, modified, `isReadOnly` when its sack
    belongs to a read-only character, `viewport` when the grid is larger than its room and scrolls: the part shown, in
    pixels of the grid). The viewports are the controls `stashBagViewport`, `playerMainViewport` and `playerBagViewport`
    (`mouse_wheel`).
  - With `panel`, the items of its current sack: name, base name, record id, cell location, seed, and:
    - `missingRecords`: role and id of each record the loaded game data lacks (incomplete item), else null;
    - `mod`: the item's mod, computed from the databases its records came from (null when all come from the game);
      `containerMod`: the map it is read with, its container's (null for the base game);
    - size, with `lacksRoom`, `realWidth` and `realHeight` when shown 1 x 1 for want of room for its real size;
    - `itemClass`, `rarity`, `requiredLevel` (what the sort orders read); `shards` and `relicBonus` for a relic or
      charm;
    - `place`: where the search shows the item (container name and file, sack type and number, stash type).
  - For `equipment`, also the slot rectangles.
- **`search_entries(baseItemId)`**: the search database entries of one base item record id: their number and, per
  entry, the place (container name and file, sack type and number, stash type), cell, stack size, `mod` (computed from
  its records), `containerMod` (the map it is read with), `lacksRoom` when shown 1 x 1, the name the search shows
  (loaded as the search loads it), the records it could not resolve. Checks that a moved or reloaded item has exactly
  one entry, at the right place, and that its names come from its container's map whatever character is selected.
- **`search_containers(filter?)`**: the search database's containers (file, name the search shows) with their number
  of entries and their items' base records; `filter` keeps those whose file or name contains it. Checks that a file
  read again (hot reload of a shared stash, also another map's) replaced its items in the search.
- **`select_search_result(container, x?, y?)`**: shows a search result like a click on its row in the results dialog,
  through the main window's handler: the first entry whose container file or name contains `container` (at cell `x`,
  `y` when given). Returns the entry, the character and vault shown, the notification, the active map, and per panel
  the bag shown and the selected items. Checks results in another map's shared stash.
- **`find_copies(panel, x, y, kind)`**: right-clicks a cell and clicks the item menu's *Search* entry for `kind` like
  the user: `SameSeed` (same seed), `AllCopies` (same base, prefix and suffix, any seed), `SameBase`, `SamePrefix`,
  `SameSuffix`.
  - Reads the results window it opens: `title` (search, item name, number found) and `grid` as `grid_values` reports
    it: the source item first, marked `(source)`, then the matches of every vault, character, stash and loadout piece in
    the search.
  - A disabled entry (prefix or suffix search of an item without one) is refused; `context_menu` shows which are.
  - The window stays open: `grid_click` a row to jump to it like the user, `close_form` to close it.
- **`results_move(action, rows?, path?, menu?)`** *(Debug)*: drives *Move To* and *Undo* of the open results window
  (from `find_copies` or the Advanced Search) through its own handlers.
  - `state`: per row the item, container, container type, bag and selection; the report line under the buttons; `undo`
    (the moves *Undo* can still put back); the enabled buttons; the entries of its right-click menu as it opens (vaults
    by name only, since opening one reads it).
  - `select`: selects rows like Ctrl clicks; `rows` = `all` (as the *Select All* button), `none`, or indexes `0,2,5`.
  - `menu`: clicks the entry along `path` of the right-click menu (`Move To > Vaults > vault-test1 > Bag #2`,
    `Move To > Transfer Stash`, `Undo`), or with `menu` = `move` of the *Move To* button's menu (`Relic Vault`); then
    returns the state.
  - Every selected item must be in a test character or vault, and a vault target must be a test vault. A move to the
    transfer stash or the relic vault changes those shared files once saved: undo it, never save it.
  - The loadouts file of a character whose piece moves is written at once.
- **`capture_results_menu(menu?, open?)`** *(Debug)*: renders a menu of the open results window to PNG as it opens,
  sub menus opened along `open` (`Move To`), then closes it without choosing: `results` for the list's right-click
  menu, `move` for the *Move To* button's menu.
- **`owned_skill_items`**: the owned items granting a skill, from the search database (vaults, characters and stashes
  with *Load all files* on): record ids, seed, vars, stack size, size in cells, location, the skills with their level
  and granting record. Writes nothing; its output is enough to write a vault file.
- **`highlight_filters(check?, uncheck?, apply?, checkItems?, uncheckItems?)`**: opens the main window's highlight
  filters panel (the "Highlight items having" link).
  - Ticks and unticks its check boxes by text or name (`"Having Skill,Set Item,Modded"`), and its list entries as
    `list:entry`, separated by semicolons (`"Mods:shadowchampions;Origin:Atlantis"`). Lists: `Types`, `Rarity`,
    `Origin`, `Mods`; entries by the text shown (`Mods` lists the installed mods as the mod menus name them).
  - `apply=true` applies like its Apply button.
  - Returns the check boxes, the lists (shown or not: `Mods` is hidden without any installed mod) with each entry's text
    and state, and the number of items highlighted.
  - To clear: untick and apply again, or use the panel's Reset.
- **`mod_state`**: read only. The session's custom map (mod) data: active map, maps whose database is loaded, shown
  transfer stash and relic vault files (`activeSharedStashMap`), the data the base game's are read with
  (`baseSharedStashData`), installed mods, and per listed character its link state, mod, map, shared stash map, and
  whether it was read with another map's database.
- **`mod_suggest(character)`**: runs the mod link dialog's Suggest on any listed character (reads only its `Player.chr`
  and `winsys.dxb`): records the base game lacks, and the installed mods having them, best first. Links nothing.
- **`game_text(tags)`**: translates text tags with the loaded text database (game, expansions, custom map strings); an
  empty text is an unknown tag.

### Navigation, items and keys

- **`select_character(name)`** / **`select_vault(name)`**: select a drop-down entry, as `get_state` lists them. The
  first vault entry, *Maintain Vault Files...*, opens the modal maintenance dialog after the call returns: drive it with
  `control_values`, `set_control` and `click_control` (form `VaultMaintenanceDialog`).
- **`select_bag(panel, bag)`**: clicks a bag button. `stash` bag 0 shows the equipment panel.
- **`pick_item(panel, x, y)`**: picks up the item at a cell, like a left click. A read-only character's panel refuses;
  the error gives the notification.
- **`move_drag(panel, x, y)`**: hovers the dragged item so that its top-left corner lands on cell (x, y); reports the
  cells under it.
- **`drop_item(panel, x, y)`**: drops the dragged item, top-left corner on cell (x, y); reports whether the panel
  accepted it.
- **`cancel_drag`**: returns the dragged item where it was picked up, like a right click.
- **`create_item(panel, baseItem, prefix?, suffix?, relic?, relicBonus?, relic2?, relicBonus2?, seed?, relicShards?, relic2Shards?)`** *(Debug)*:
  builds an item in memory from database record ids and drags it, like the Duplicate menu entry; place it with
  `drop_item` (a created item cannot be cancelled back). `relicShards` and `relic2Shards` give the relics' piece counts
  (one by default); `relicShards` is also the count of an item that is itself a relic or charm.
  - Tests combinations no vault holds, e.g. a relic whose completion bonus grants a skill, or any record in any slot as
    the Forge allows in God mode.
  - Saved with its vault or character like any item: create it in a test file (see [Safety](#safety)).
- **`select_items(panel, cells)`**: Ctrl-clicks cells (`'x,y;x,y'`) like a user holding Ctrl, adding or removing each
  item from the selection; returns the selected items. Empty `cells` only reads the selection. The context menu then
  acts on the selection (multiple delete, move, export, clear selection).
- **`click_cell(panel, x, y, modifiers)`**: left-clicks a cell through the panel's mouse handler with `modifiers`
  (`ctrl+alt`, `alt`, `ctrl`, `shift`, empty) held in the keyboard state, so the click key binds apply. Defaults:
  `ctrl+alt` duplicates the item into the cursor, `alt` copies it (with item copy allowed; without, the click picks the
  item up), `ctrl` adds it to the selection. Returns the item clicked, the dragged item, `draggedIsNew` (a copy is in
  the cursor) and the selection.
- **`drag_select(panel, x1, y1, x2, y2, modifiers)`** *(Debug)*: left button down on one cell with `modifiers` held,
  move to another, release there, through the panel's mouse handlers. With the rectangle selection's modifiers (`shift`
  by default) it selects the rectangle's items. Returns the selection and the dragged item (a click bound to copy picks
  a copy up instead).
- **`press_key(panel, x, y, key)`** *(Debug)*: rests the cursor on a cell (the panel gets its MouseEnter, as from the
  mouse) and presses a key like the keyboard. Runs after the call returns.
  - Key down and up messages are posted to the window with the keyboard focus and handled by the message loop
    (`ProcessCmdKey`, `IsInputKey`, `ProcessDialogKey`, the main window's key preview, then the control's `KeyDown`).
    The modifiers are held in the UI thread's keyboard state, where `Control.ModifierKeys` reads them.
  - So the key binds apply as for the user. Defaults: `c` copies and `d` duplicates the item (picked up, like the menu
    entries), `backspace` deletes it (with its confirmation: queue the answer first), `escape` cancels a drag. Any
    gesture works (`ctrl+a`, `alt+w`, `ctrl+oemplus`, `f5`).
  - An empty `panel` leaves the cursor where it is (the forge keys `enter`, `space`).
  - Returns `target`, the control that received the key (a `SackPanel` while the cursor is on a panel; the highlight
    box after `mouse_click` on it).
  - The context menu's rules apply: nothing is copied without item copying, nothing deleted from the equipment of a
    read-only player or from a read-only character.
  - Every bound key that runs writes `Key <gesture>: <action>` to `recent_log` (Debug level), `(nothing done)` when the
    rules refused it.
  - The modifier keys send no message of their own (Windows' menu mode on a lone Alt cannot be reproduced).
- **`key_binds()`**: read only. The key and mouse bindings in use: every action with its kind (`Key`, `Click`,
  `Fixed`), scope, gestures as saved and as shown, whether it has its default; the changes saved in `UserConfig.xml`
  (`<KeyBinds .../>`); the conflicts.
- **`key_binds_dialog(action?, slot?, gesture?, defaults?, click?, right?, key?)`** *(Debug)*: drives the open key
  binds dialog (open it with `click_control("", "configureButton")`, then `click_control("Settings", "keyBindsButton")`).
  - Returns every row as shown (cells, conflicts, `Greyed` with its `ToolTip` for an action turned off by a setting),
    the text under the list, whether OK and Clear are enabled, the armed slot, and `keyTarget` (the control that
    received `key`).
  - `action` rebinds `slot` (0 shortcut, 1 alternate) with real messages: a left click on the cell's window arms it,
    then the key goes to the focused window through the message loop's steps (the dialog's `ProcessCmdKey` takes it);
    for a click action, the cell is clicked again with the modifiers held (`ctrl+shift+click`).
  - An empty `gesture` presses Clear (`PerformClick`: a button's mouse click checks the window under the screen point,
    which a hidden desktop does not answer).
  - `click=true` only clicks the cell (with the modifiers of `gesture`; `right=true` for the right button, which
    disarms).
  - `key` presses a key in the dialog as is, last (`escape` disarms a click action; on a key action it is the new key).
  - `defaults=true` presses Defaults first.
  - Close with `close_dialog("Key binds", "OK")`, then `close_dialog("Settings", "OK")` to save.

### Tool tips and menus

- **`hover_item(panel, x, y)`**: rests the virtual cursor on a cell without dragging. TQVault shows the item's tooltip
  and writes its name in the notification bar. Returns the item, the notification text, the tooltips shown (position,
  size, and the working area of their monitor: checks the placement on any monitor), and a PNG of the tooltip taken at
  once (a tooltip can close by itself).
- **`hover_bag(panel, bag, hide?)`**: shows a bag button's tooltips like resting the pointer on it: the summary of the
  bag's items and, for a bag with a custom label, the label above the button.
  - Returns each tooltip shown with its size and text font (checks a tooltip keeps its size over zoom and
    maximize/restore cycles).
  - Capture the summary with `capture('tooltip')`, the label with `capture('tooltip:BagButtonLabelTooltip')`.
  - `hide=true` closes them.
- **`hover_end`**: moves the virtual cursor off the panel, closing the tooltip.
- **`context_menu(panel, x, y)`**: right-clicks a cell through the panel's mouse handlers and returns the context menu
  built for it, then closes it without choosing: entry texts, `-` for separators, sub menus with their entries. The
  current selection counts, as for a real right click. Disabled entries have `Enabled: false`, the current choice
  (affix, bonus) `Bold: true`.
- **`context_menu_click(panel, x, y, path)`**: right-clicks like `context_menu`, then clicks the entry along `path`
  (`Move Item To > Bag #3`, `Prefix > Arctic : ...`) after the call returns, since an entry may open a modal dialog
  (properties, seed, a confirmation).
- **`menu_click(target, path?, form?)`**: opens a menu as the user does and returns its entries (text, sub menus,
  `Enabled` false, `Checked`, `Hint`; hidden entries left out), then closes it. With `path`, clicks that entry after
  the call returns (it may open a dialog: queue its answers first).
  - Targets:
    - `characterTools`: left click on the tools button next to the character list (*Duplicate Character*, *Mod
      link...*, *Archived*, *Read-only*, *Tags*);
    - `loadouts`: right click on the loadout bar (its actions); `loadouts.list`: left click on the bar's name box (the
      loadouts);
    - `stash.bag<N>`: right click on bag button N of the stash panel, which also shows that bag (its tooltip toggle);
    - `systemMenu`: right click on the title bar of the main window, or of the TQVault form named by `form` (*Restore*,
      *Minimize*, *Maximize*, *Close*...).
  - The right clicks send button down and up, then the `WM_CONTEXTMENU` Windows sends, so the control's own
    `ContextMenuStrip`, `Opening` event and `SourceControl` work as for the user.
  - Refused (see [Safety](#safety)): `characterTools` entries other than tags on a non-test character; a tag's entries
    on a tag not starting with `test-`; *Duplicate Character* without a test name queued first (`text:player-test...`);
    *Archive all* / *Unarchive all* (global and per tag: `tag_archive_all` checks a tag's characters first); loadout
    actions on a non-test character; the window menu's *Move* and *Size* (they wait for the arrow keys: use
    `move_window`).
  - `duplicate_character`, `open_mod_link`, `archive_character`, `character_read_only(toggle)` and `tag_box` delete
    click their entry this way.
- **`vault_menu(path?)`**: the menu of the vault tools button next to the vault list (maintain vault files, *Modded
  vault*, share, import) as it opens for the shown vault, with the shown vault's `vaultMod`. With `path` (e.g.
  `Modded vault > shadowchampions`), clicks that entry after the call returns.
- **`bag_menu(panel, bag, path?)`**: right-clicks a bag button like the user (selects that bag) and returns the bag
  context menu (move, copy, merge, empty, tooltips, CSV and JSON export, change, copy and paste icon, import) and the
  icon settings of every bag.
  - With `path`, clicks that entry after the call returns, e.g. `Copy Bag > Bag #3`, or `Change icon` (opens the bag
    button settings).
  - Only vault bags have this menu.
  - The CSV and *Share to Clipboard* entries write to the clipboard the user shares with the hidden desktop: do not
    click them.
- **`sort_menu(panel, path?)`**: right-clicks the sort button of `vault`, `secondaryVault`, `player` (main inventory),
  `playerBag` or `stash` (the shown Transfer Area, Storage Area or Relic Vault tab) like the user and returns its menu:
  one entry per order (*Sort by size (normal)*, *Sort by required level*, *Sort by type*, *Sort by rarity*), `Checked`
  on the one this button's left click uses.
  - Each sort button keeps its own order (the stash's one button keeps one for its three tabs).
  - With `path`, clicks that entry after the call returns: the order becomes this button's only, saved as its attribute
    (`<Sort Algorithm="Normal" Vault="Level" Player="Type" />` in `UserConfig.xml`), and the bag is sorted at once.
  - Also returns the button's choices before the click: `sort.key` (`Vault`, `SecondaryVault`, `Player`, `PlayerBag`,
    `Stash`), `sort.algorithm` (its order), `sort.own` (false while it still uses the default),
    `sort.defaultAlgorithm` (`Algorithm`), `sort.mergeRelics`.
- **`sort_click(panel)`**: left-clicks a panel's sort button like the user: the shown bag is sorted with this button's
  own order, else the default `Algorithm` (rows and columns alternate at each click), relic and charm shards merged
  first with *Merge relic shards when sorting* on.
  - Returns the order used (`sort`, same fields as `sort_menu`). Runs after the call returns; read the result with
    `get_state(panel)`.
  - A sort that does not fit leaves the bag as it was and shows a message (`dialog_answers`).
  - `sort_menu` and `sort_click` refuse a hidden sort button, which the user cannot click: the `stash` button on the
    Equipment tab, a panel with no bag.
- **`open_bag_settings(panel, bag?)`**: opens the bag button settings (icon and label picker) of a vault bag like the
  user: right-clicks its bag button (shown bag by default), which shows that bag, then clicks *Change icon* in its menu
  (as `bag_menu` does) after the call returns. Its OK redraws the button, as for the user.
  - Only the vault panels' bags have the entry: `playerBag` and `stash` are refused.
  - Waits for the game icons and reports how many loaded.
  - Modal: capture it with `capture("Bag Button Settings")`, close it with `close_dialog` and the Cancel button's text
    in the game language (e.g. "Annuler").
- **`bag_settings_pick_icon(tab, index)`**: in the open bag button settings, right-clicks an icon of a tab's grid
  (Skills, Relics, Artifacts...) like the user: picks the icon's pictures and switches to Custom icon. Apply with
  `click_control('Bag Button Settings', 'applyButton')`.
- **`bag_settings_hover_icon(tab, index?)`** *(Debug)*: in the open bag button settings, rests the pointer on an icon of
  a tab's grid through the grid's hover handler: the magnifier (`iconMagnifier`) shows it enlarged below, with its name
  and size; capture with `capture("Bag Button Settings")`. Without `index`, the pointer leaves the grid and the
  magnifier hides.
- **`bag_settings_drag_icon(tab, index, target)`** *(Debug)*: in the open bag button settings, drags an icon of a tab's
  grid onto a picture box like the user.
  - Left button down on the icon (mouse messages to the grid), the virtual cursor moved onto the box, button up to the
    grid, which has the mouse capture. The grid's handler finds the box under the cursor by reading the cursor
    position, which automation answers with its virtual cursor.
  - `target`: `on`, `off` or `over` (*Detailed* pictures) or `simple` (*Simple* picture); a box hidden in the current
    mode is refused.
  - The drop switches to Custom icon; apply with `click_control('Bag Button Settings', 'applyButton')`.

### Item comparison

- **`compare_items(panel, x, y, x2, y2, panel2?, path?, keepOpen?, alignRows?, hideUnchanged?, showSources?, toggle?, height?, scroll?)`**:
  opens the item comparison directly on two items, possibly on different panels (e.g. an equipped weapon and a vault
  one). Nothing is changed. Steps:
  1. Ticks the toolbar check boxes as asked (empty keeps the saved choice; a change is saved in the launched copy's
     `UserConfig.xml`, like a user's).
  2. `toggle`: clicks rows like the mouse (`'|'`-separated, each the start of a clickable row's text, white space
     trimmed: the difference or item line of a stat with sources). The click goes through the row's own handler and
     opens or closes that stat's sources.
  3. `height`: gives the dialog that height in pixels, as dragging its bottom border does.
  4. `scroll`: scrolls its columns, `page:N` (N pages down with the scroll bar's messages, negative up) or `wheel:N` (N
     wheel notches down, negative up), sent as that input's window messages so the panel's own handlers run. To check
     the marble behind the columns stays in place: `keepOpen`, then `capture("Item Comparison", shown=true)`.
  - Returns `first`, `second` (the two items), `options` used, `toggled` (rows clicked), `scrolled` (scroll position),
    `dialogSize`, `contentSize`, the four columns as rendered under `columns` (`firstDifferences`, `first`, `second`,
    `secondDifferences`, rows as in `tooltip_text_dump`), and a PNG of the dialog with its border (of the whole table
    when the content scrolls), also written to `path` (or a temporary file).
  - Opens like the user's, through the main content: a new modeless window owned by the main window, one step down and
    right of each comparison already at its place, comparing copies taken when it opens, following the main window's
    zoom while open (`zoom`, then `control_tree("Item Comparison")` matches a comparison opened at that zoom). `window`
    gives its index, bounds and state as `comparison_windows` lists it.
  - Closed at the end unless `keepOpen`, which leaves it beside the others (`capture("Item Comparison")` captures the
    newest; `comparison_windows` lists and closes them).
  - The user's way in: `context_menu_click(panel, x, y, "Compare")`, then `compare_click` on the second item.
- **`compare_click(panel, x, y, button?)`**: clicks a cell through the panel's mouse handler while the compare
  selection (started by the item menu's `Compare`) may be on.
  - `left` on an item picks the second item and opens the comparison in a new modeless window after the call returns,
    beside those already open. Wait for it with `comparison_windows`, then `set_control("Item Comparison",
    "alignRowsCheckBox", "hideUnchangedCheckBox" or "showSourcesCheckBox", ...)` and `capture("Item Comparison")` (both
    on the newest), and `comparison_windows("close", index)`.
  - `left` on the first item ends the selection; on an empty cell keeps it. `right` cancels it (`press_key(...,
    "escape")` too).
  - Without the selection on, a left click takes the item like `pick_item`.
  - The main window stays usable while comparisons are open.
  - Returns the selection `before` and `after` (active, first item) and the panel's cursor (`hand` while the selection
    is on).
  - The same selection serves *Insert into item*: drive that one with `socket_click`, since `compare_click` closes the
    slot menu.
- **`socket_click(panel, x, y, slot?, button?)`** *(Debug)*: clicks the item that receives the relic or charm of the
  *Insert into item* selection, through the panel's mouse handler.
  - Start it with `context_menu_click(panel, x, y, "Insert into item > Game rules")` (or `> No rules`) on a relic or
    charm, item editing on. The selection survives `select_vault`, `select_bag` and `select_character`.
  - `left` on another item: refused (the forge's reason in `notification`), inserted at once into the only empty slot
    that takes it, or the slot menu shows (`slotMenu`: `Slot N` or `Slot N: replace <name>`). `slot` (1 or 2) then
    clicks that entry like the user; no `slot` closes the menu without inserting.
  - `left` on the relic itself ends the selection, `right` cancels it.
  - Returns the selection `before` and `after` (active, purpose, first item), the clicked item's relic slots `before`
    and `after` (relic, bonus, shards), `relicInItsSack`, `targetModified` and the `notification`.
- **`comparison_windows(action?, index?)`**: lists the open comparison windows, oldest first: `index`, `title`, the two
  items compared (copies taken at opening: moving, changing or deleting the originals does not change them), `bounds`,
  `active`, `visible`, toolbar `options`.
  - `action` = `close` closes window `index`, `closeAll` closes them all, like their OK button (each disposes itself);
    then returns the windows left.
  - Closing the main window closes them too.

### Files

- **`create_vault(name)`**: creates a test vault like the New action of *Maintain Vault Files...*, then selects and
  loads it. The name must start with `vault-` (`vault-test1`, ...). Written on the first save: an empty vault never
  changed is not kept.
- **`export_vault(path)`**: exports the vault shown to a JSON file, like the vault menu's "Export to file" without its
  file dialog.
- **`export_items(panel, x, y, path)`**: right-clicks a cell, then runs the item menu's `🔗 Share Item` →
  `Share to File...` without its file dialog: the selection (`select_items`) when there is one, else the item under the
  cursor. `exported` is false with nothing to export. Never touches the clipboard or PasteBin.
- **`export_tab(path, bag, panel?)`**: right-clicks bag button `bag` of a vault panel (`vault` by default, or
  `secondaryVault`) like the user, which shows that bag, then runs its menu's *Export tab* > *File* without the file
  dialog: the tab is written as exported JSON to `path` (the handler's own serialization, shared with the dialog path).
  Only vault bags holding items have the entry: an empty bag, `playerBag` and `stash` are refused. Never touches the
  clipboard or PasteBin.
- **`export_csv(path, bag?)`**: writes the CSV export of the vault shown (or of bag `bag`, zero-based) to a file, like
  the vault context menu's *Export CSV* entries without the clipboard, with the settings' CSV delimiter.
- **`import_text(text?, path?)`**: pastes like Ctrl+V with the clipboard holding the text, without touching the
  clipboard: exported JSON (item, items, tab or vault, as copied by *Share to Clipboard*) or a PasteBin link, given as
  `text` or read from the file at `path`.
  - Same parsing and placement as the paste: items go into the current tab of the vault shown; a whole vault asks to
    replace it or create a new one (`queue_dialog_answers` first: Yes replaces, No creates).
  - Returns the vault shown, its `vaultMod` and the import's `error` (null on success). Test vaults only.
- **`import_file(path)`**: imports an exported JSON file like "Import from file" without its file dialog: items go into
  the current tab; a whole vault asks to replace the vault shown or import it as a new vault (`queue_dialog_answers`
  first: `Yes` replaces, `No` creates `<name>_<date>_<time>`, `Cancel`). The vault shown must be a `vault-` test vault.
- **`duplicate_character(name)`**: duplicates the selected character like the user (player file, stash and all) under a
  name starting with `player-test` or `player-mod` (`player-test-copy`, ...).
  - Clicks *Duplicate Character* in the character tools menu (`menu_click`) after the call returns, and types the name
    into its name dialog, queued first as its `text:` answer, so the dialog's checks run (a refused name shows the
    error box and the dialog again: see `dialog_answers`, close it with `close_dialog`).
  - The entry shows only with *Allow character edit* on in the settings.
  - TQVault then restarts, as for a user. The endpoint comes back within a few seconds (it retries while the old
    process still holds the port): wait for `loadingComplete`, then select the copy.
- **`save_files`**: saves the modified characters, vaults and stashes like the Save button. Returns whether characters
  were saved and the panels still modified. A file error (locked or read-only file) shows the Abort/Retry/Ignore box
  and the call waits for its answer: answer it from another call with `close_dialog`.

### Characters

- **`character_list(refresh?, pick?)`**: opens the character list and reads it as drawn: `header`, the text of the
  selected character's box, and per entry `name`, `className`, `level`, `masteries`, `mod` and the `badges` shown
  (Archived, Read-only, Modded, link state). Also returns `selected` and `characterFile`; the list is left open, except
  after a `pick`.
  - A linked character's class and masteries are its mod's names whatever character is selected.
  - `refresh=true` describes the entries again first, as after a tag change, with whatever map is active.
  - `pick` (a character name) first clicks its entry like the user, with mouse messages to the label under the entry's
    middle: the list scrolls to it, the click selects the character (which loads it); the list is closed afterwards
    (`listOpen` tells whether the click left it open, as when picking the selected character).
  - `select_character` sets the selection directly instead.
- **`open_character_editor`**: shows the selected character's editor in place of the main content, like the Edit
  button of the stash panel, before the call returns (`get_state` gives `view` `characterEditor`). Check the selected
  character with `get_state` first: the editor opens on whichever is selected.
  - Read and change it with `control_values` / `set_control` (form `Character Editor` or
    `CharacterEditorUserControl`).
  - Close with `close_dialog("Character Editor", "OK" or "Cancel")`: OK applies the edits in memory, `save_files` writes
    them; `press_key("", 0, 0, "escape")` cancels too. Leaving another way (`click_control("", "configureButton")`,
    closing the window) asks first when it holds edits.
  - Built at each opening at the window's DPI; keeps its size during a zoom (scroll bars when the window is smaller);
    follows a move to a monitor of another DPI, edits kept. `capture("Character Editor")` draws it.
  - Left, always shown: the statistics panel of the game's character window (`character_panel`,
    `character_attribute_click`; the attribute boxes stay as hidden models). Under it the side panel
    `CharacterSidePanel`: the statistics option `StatsDifficultyComboBox` (`set_control`), the leveling editors (shown
    while they can be changed), the masteries.
  - Right: a frame drawn like the game's skill window (`EditorTabs`, pages chosen with `select_tab`), one page per
    mastery slot: `SkillsMasteryNComboBox`, `SkillsMasteryNLevelUpDown`, and the skill pane `SkillsMasteryNPane`
    (N = 1, 2), read and clicked with `skill_panel` / `skill_panel_click`.
- **`character_panel(tab?, capture?)`**: reads the character editor's statistics panel on tab 1, 2, 3 or 4 (IV, buffs).
  - `stats`: name, class, level and XP lines, points line, attribute rows (label, value, tool tip), whether the `+`
    buttons and Undo show, tab III statistics, gold and Electrum; `Stats`, the values computed like the game (combined
    armor, the 16 resistances, tab II rows, tab III damage and race bonuses with their tool tips).
  - `side` (the side panel): leveling rows, masteries, the statistics option and its editor.
  - `capture=true` adds a PNG of the panel.
- **`character_tooltip(tab, index?)`**: renders a rollover of the editor's statistics panel (tab 1 to 4) as the
  game-style tool tip shows it on hover: lines (text, colour, title) and a PNG. Without `index`, lists the tab's
  rollovers by number with their first line.
- **`character_stat_sources(filter?, sources?)`**: the sums behind those computed values in the open editor, with the
  racial bonuses (pet bonus records summed under `pet:` names); `filter` keeps the variable names containing it.
  - `gear`: variables of the gear sources turned on in the Buffs tab (equipped items, item sets).
  - `skills`: variables of the other sources turned on: skills at their level (a modifier with its skill), skills the
    equipped items grant, curses, shrine buffs, pet auras, Tartarus effects, the scroll's buff.
  - `buffsOn`: the keys of the sources turned on; `sources=true` adds each source (key, section, name, on, level, gear
    role, its own variables).
  - `offense`: tab I's average damage, DPS and attack speed with their inputs: stance, each weapon in hand, the other
    items, bare-hands damage.
- **`character_buffs(panel?, scroll?, capture?)`** *(Debug)*: shows the Buffs tab (tab 4) of the editor's statistics panel, or
  of the main window's with `panel="main"`, or of a floating character window's with `panel="window:<name>"`, and reads it: the scroll choice (title, selected, drop-down), then each
  section (`Gear`, `Passive`, `Active`, `Pet`, `Curse`, `Shrine`, `Tartarus`, `Scroll`) with its sources: key, name,
  icon, on, bounds in client pixels when in sight, tool tip lines. `scroll` first scrolls the list by game pixels, as the
  wheel. `capture=true` adds a PNG of the panel.
- **`character_buff_toggle(id?, panel?, scrollIndex?)`** *(Debug)*: clicks a source of the Buffs tab through the panel's hit test
  (scrolled into sight first); `/` is accepted for `\` in the key. `scrollIndex` picks a scroll in the drop-down (0 for
  none; the choices are listed when out of range). Toggles are memory only and never saved: allowed on any character.
  Returns the tab's state.
- **`character_weapon_set(panel?, set?)`** *(Debug)*: reads the weapon set gems of tab I (`editor`, `main`,
  `window:<name>`): set counted, each gem's bounds and tool tip, armor, offense and attribute rows. `set=1|2` first clicks
  that gem through the panel's hit test. Memory only, never saved: allowed on any character.
- **`class_sets(action?, name?, newName?, menu?)`** *(Debug)*: the class set bar of the open character editor
  ([class sets](CHAREDIT.md#class-sets)): bar bounds and parts (field, mark, [+]), names, selected, `canChange`,
  `markToolTip` (why the build shown differs from the selected set, null when it holds it); the file (`fileExists`,
  `readFailed`, `lastApplied`); the sets as edited (`changed`: something for OK to write), each with its masteries,
  skills, attribute points and `matches` (holds the build shown); the build shown (`current`), `skillPointsLeft`,
  `attributePointsLeft`. `menu=list|actions` reads a menu. `hover=mark|add|field` sends a mouse move to that part,
  shows the tool tip it set as a hover does and returns it (`hoverToolTip`: text, window bounds).
  - Actions through the bar like the user: `select` (name) applies the set; `saveAs` (name) clicks [+] and types the
    name; `save`, `rename` (newName) and `delete` act on the selected set (`name` picks it first, which applies it).
    All but `select` run after the call returns (queue the answers: save and delete confirm). OK (`close_dialog`)
    writes the file. Test characters only.
- **Floating character windows** *(Debug)*: the window **Pop out** opens (`click_control("TQVaultAE","floatButton")`).
  - `character_window_open(character, x?, y?)`: opens the window of a listed character (key, name or drop-down label),
    or brings it to the front; `x`/`y` place it as a window reopened at start. Writes nothing.
  - `character_window(character?, full?, capture?)`: per window the key, bounds, DPI, scale, cell size, read-only,
    `loadout` shown (null: the equipment worn) and `loadouts` listed, `classSet` counted (null: the save's build), `classSets` listed and `classSetMark` (applied in part), `sharedWithMain`, the equipped items, the numbers of tabs I-III and the buff keys on; `saved` lists the places
    kept in `UserConfig.xml`. `full=true` adds each panel's view, `capture=true` a PNG.
  - `character_window_loadout(character, loadout?, step?)`: shows a loadout's pieces read-only in place of the equipment
    (empty: the equipment worn), or `step=-1/+1` clicks an arrow. Writes nothing.
  - `character_window_list(character, list?, pick?)`: opens its `loadouts` list, its `classsets` list or tab IV's `scrolls` choice like a
    click, returns the entries and the open list as a PNG; `wheel` scrolls it first as wheel notches do, `pick` clicks an
    entry instead.
  - `character_window_close(character)`: closes it as its close button (not reopened at start).
  - Its equipment is panel `window:<name>` for the item tools (`pick_item`, `drop_item`, `get_state`, `capture`...);
    `character_buffs` / `character_buff_toggle` take `panel="window:<name>"`.
- **`character_attribute_click(attribute, button?, shift?, modifiers?)`**: clicks the `+` button of an attribute
  (`health`, `energy`, `strength`, `intelligence`, `dexterity`, or 0-4) through the panel's hit test and mouse handler,
  `modifiers` held in the keyboard state (`shift=true` adds Shift). Test characters only.
  - `left` spends a point, `right` takes one back; with the "every point at once" modifiers of the key binds (Shift by
    default), all the points or back to the start.
  - `undo` clicks the Undo button.
  - Refused when the button does not show (no `+` while no point is left).
- **`skill_panel(slot, capture?)`**: shows the page of a mastery slot (from 1) in the open editor and reads its skill
  pane as drawn: mastery, level, bar fill, points left; per skill its id, name, tier, button position, round or square
  border, level, item bonus, level text, state border (`CanRaise` green, `NoPointLeft` gold, `Maxed` blue,
  `TierLocked` gray, `ParentLocked` red), icon, connector pieces, tool tip. `capture=true` adds a PNG of the pane.
- **`skill_panel_click(slot, skill, button?, shift?, modifiers?)`**: clicks a skill button (record id, part of its
  name, or `mastery`) through the pane's hit test and mouse handler, `modifiers` (`ctrl`, `shift`...; `shift=true` adds
  Shift) held in the keyboard state. `left` +1 point, `right` -1; with the "every point at once" modifiers of the key
  binds (Shift by default), the highest level the points allow or the lowest. The editor's rules apply. Returns the
  pane state. Test characters only.
- **`skill_tooltip(slot, skill)`**: renders the game-style tool tip of a skill (record id, part of its name, or
  `mastery`) of a mastery slot's pane as it shows on hover: lines with their colours, and a PNG.
- **`player_info(tab?)`**: reads the statistics panel next to the equipment (stash bag 0), the game's character window
  read only, like `character_panel` reads the editor's: name, class, level, attributes, computed values of tabs I to
  III, gold, Electrum.
  - `requirementAttributes`: the attributes item requirements are checked against (strength, dexterity, intelligence,
    health, energy with the gear and skill bonuses: the panel's sums).
  - `tab` (1 to 4) shows that tab first. `visible` is false with no character selected or another stash tab shown.
- **`player_stats_hover(tab, index?, hide?)`** *(Debug)*: rests the virtual cursor on a rollover of that statistics
  panel (tab 1 to 4; without `index`, the list of its rollovers); the panel's own mouse move shows the game tool tip,
  placed from the virtual cursor as a hover places it.
  - Returns its lines, its window rectangle on screen, the hovered area, the statistics and equipment panels, the main
    window and the working area on screen, `insideWorkingArea`, `coversHoveredArea`, and a PNG of the main window with
    the tool tip drawn at its place (the tool tip window itself cannot be captured headless).
  - `hide=true` ends the hover.
- **`player_stats_fonts`** *(Debug)*: that statistics panel's fonts (name, class, text, title, number, tab and
  resistance fonts: family, size, unit, style) and the base font. Checks the panel follows a live font change made in
  the settings.
- **`stash_background_switch(bag)`** *(Debug)*: clicks a stash bag button like `select_bag` and reports the panel
  background before and after: whether the picture drawn before was a source picture or a scaled copy (zoom other than
  1), and whether that copy is disposed once replaced.
- **`gdi_census(action, target?, count?)`** *(Debug)*: runs an action `count` times (`repaint` a panel or the main
  window, `zoom` in and out, `characterList` refresh, `openForm` create and dispose) and reports GDI/USER objects and
  handles before, after and after the finalizers, and `undisposed`: the finalizable objects (bitmaps, fonts, pens...)
  dropped without `Dispose` (exact when `gcDuring` is 0). Proves a leak fix: run it on the build before and after.
- **`archive_character(archived)`**: archives or unarchives the selected character like the user: opens the character
  tools menu and clicks *Archived* when its tick differs (`menu_click`), before returning; TQVault moves its save
  folder to or from `ArchivedCharacters`. Returns `clicked` (false when already so) and `isArchived`. Test characters
  only (see [Safety](#safety)); the global *Archive all* entries, which move every character, are never driven.
- **`character_read_only(toggle?)`**: reads the selected character's read-only state: the character menu's *Read-only*
  tick (`menuChecked`), its list entry's badge (`badge`), the state of the loaded character and its sacks, and the
  read-only attribute of each file TQVault writes for it (`files`: `Player.chr`, `winsys.dxb`, `winsys.dxg`, the mod
  link file).
  - `toggle=true` first opens the character tools menu and clicks *Read-only* like a user (`menu_click`), setting or
    clearing that attribute (test characters only, see [Safety](#safety)). `notification` then shows a refusal (unsaved
    changes) or an error.
- **`tag_box(box, text, tag?)`**: names must start with `test-`.
  - Add or rename box: types into it in the character menu and presses Enter.
  - `delete`: clicks *Tags* > the tag > *Delete* in the character tools menu (`menu_click`) after the call returns, and
    answers its confirmation with `OK` (the menu's own handler refreshes the menu and the character list).
  - `assign` / `unassign`: ticks or unticks an existing tag for the selected test character (see [Safety](#safety)).
  - Tags are shared by every character (`Config\TagConfig.json`): back the file up first.
- **`list_tags`**: read only. The character tags (`name`, `color` as `#RRGGBB`) and the selected character's
  (`selectedCharacterTags`).
- **`tag_archive_all(tag, archived)`**: clicks *Archive all* or *Unarchive all* in a tag's menu, moving the save folders
  of the characters having that tag. Only a `test-` tag whose characters are all test characters; returns those
  characters with their state and folder.
- **`open_mod_link`**: opens the mod link dialog of the selected character (custom map, or Immortal Throne main
  campaign: bounce mod), by a click on *Mod link...* in
  the character tools menu (`menu_click`) after the call returns. Test characters only (see [Safety](#safety)): Link
  and Unlink write `TQVaultAE.ModLink.json` in the save folder.
  - Read it with `control_values(form 'ModLinkDialog', labels true)`; pick a mod with
    `set_control(listBoxMods, '<display name>')`; then `click_control` `buttonSuggest`, `buttonLink`, `buttonUnlink` or
    `buttonClose`.
  - Closing after a change reloads the character with its new mod's data.

### Layout and scaling

- **`zoom(step)`** *(Debug)*: zooms the main window like its shortcuts, one step at a time: `in` (Ctrl +), `out`
  (Ctrl -), `reset` (Ctrl Home). Steps go from 0.4 to 2 by 0.1, each made exact by a whole item cell size.
  - Returns `step` (0 to 16, 6 for zoom 1), `nominalZoom` (the zoom saved), the item `cell` size in pixels, the
    effective `zoom` (the cell over the 32 px design cell, without the DPI: 0.90625 for step 0.9 at 96 dpi), the DPI
    factor, the scale (cell / 32) and the client size.
  - Written to `UserConfig.xml` like a user zoom: end with `reset`.
  - To compare with the 96 DPI layout on a high DPI screen, start the app with the environment variable
    `__COMPAT_LAYER=DPIUNAWARE`.
- **`move_window(x, y, form?)`** *(Debug)*: moves the main window, or the open form found like `control_values` finds
  it (part of its title or its type name; a view shown in the main window moves the main window), to a screen position
  in virtual screen pixels (negative on a monitor left of the primary), without resizing, as a drag does.
  - On a monitor of another DPI, Windows sends the DPI change and the window scales.
  - Returns the monitors and the window's DPI, zoom, `dpiScale`, `systemDpiScale`, `scale` and `fontZoom`.
  - Move it back to its monitor when done.
- **`window_bounds()`** *(Debug)*: the main window's geometry.
  - Bounds (virtual screen pixels), client size, state (`Normal`, `Maximized`, `Minimized`), DPI, zoom step (`step`,
    `nominalZoom`, item `cell` size), effective zoom and scale.
  - `contentRight`, `contentBottom`: right and bottom edges of its visible child controls.
  - The monitor it is on, with its working area; `insideWorkingArea`.
  - `windowRect`: the window as Windows has it (after Windows cut a size larger than it allows, `bounds` and
    `clientSize` may still give the size asked for).
  - `maxClientSize`: the largest client size Windows lets the window take. `largestStep` / `largestZoom`: the largest
    zoom step fitting it and its nominal zoom (a zoom in stops there; a larger saved zoom is lowered to it at startup).
  - `resizeBounds`: the smallest and largest window sizes a user resize is kept between (the design size at the
    smallest and largest zoom).
  - `layout` compares the panels' table with the window: border picture sizes; the area inside them (`inside`); the
    table bounds and preferred size; `overflowRight` / `overflowBottom` (table edge minus inside edge: above 0 it
    reaches under the border); `shownRight` / `shownBottom` (edges of the panels in the table; a human sees them cut past
    the inside edge); the control under each resize grip (`gripRight`, `gripBottom`, `gripBottomRight`: `(window)` when
    the grip works); column widths and row heights.
  - With `move_window` and `click_control` on `buttonMaximize`: checks a maximize on each monitor, and that the layout
    comes back after maximize and restore cycles (compare `control_tree` before and after).
- **`window_state(state)`** *(Debug)*: sets the main window's state like the taskbar or caption buttons: `minimize`,
  `restore` (back from minimized or maximized, as a click on the taskbar icon) or `maximize`. Returns the state, bounds
  and client size after it, and the bounds of the custom border's caption buttons (to check they are back in place
  after a restore).
- **`control_tree(form, depth)`** *(Debug)*: lists the controls of an open form (main window by default) with type,
  bounds, font, visibility, enabled state, and for a container with its own `AutoScaleMode`, `autoScale` (the mode,
  its design and current dimensions); plus the form's DPI, scale, zoom and DPI factor.
  - Like every tool taking a form, it looks among the shown forms only, then the settings shown in the main window
    (`Settings`): the search dialog is kept hidden between openings, and the settings are out of the window while the
    main content shows.
- **`text_fit(form, includeHidden, tolerance)`** *(Debug)*: measures the text of every label, button, check box, radio
  button and group box of an open form (main window by default) in its font, and lists those that do not fit:
  - wider or taller than the control after word wrap;
  - a word wider than an auto size label's maximum width (cut, without ellipsis);
  - a control sticking out of its parent.
  - A fixed size label with an ellipsis (`AutoEllipsis`) is shortened on purpose, its whole text in a tool tip: only its
    place is checked. Icon buttons (title bar) are skipped.
  - Run it after switching the UI language (`<TQLanguage>` in the bin's `UserConfig.xml`, then restart) to find
    overflowing translations.

### Diagnostics and dumps

- **`raw_tooltip_lines(items?, examples?)`** *(Debug)*: scans every owned item and reports the tooltip lines still shown
  raw (`?name?`, `<unknown>`) in the granted skills, and with `items=true` in the item attributes too: grouped by raw
  name, with the count, where it appears and a sample line.
- **`tooltip_text_dump(path?, views?, panel?, x?, y?, skip?, take?)`** *(Debug)*: builds the tooltip of every owned item
  (same scope as `raw_tooltip_lines`), or of the item on one panel cell, with the tooltip's own `FillToolTip`, without
  showing it and bypassing its image cache, and reads back its rows in display order.
  - `views`, comma separated (default `classic,full`): `bare` (classic without any extra), `classic` (classic with the
    extras checked in the settings), `detailed` (detailed with the extras checked in the settings), `full` (detailed
    with every extra: in-game rolls, value ranges, granted skills, set piece counts), `settings` (what a hover shows
    now).
  - Each item: location, record ids, vars, seed, stack size; per view its rows:
    - `kind`: `text`, `spacer`, `delimiter`, `range`, `icon`, `separator` (between two skills of a column), `column`
      (before the relic column or a skill column, with `text` = `relics` or `skills`);
    - `text`, `color` (`#RRGGBB`), `bold`, `segments` (a line with several colors), `range` (the value on the right).
  - Sorted by container, sack and cell, without timestamp: two dumps can be diffed.
  - With `path`, written to that JSON file; the result is a summary (counts, rows, `unknownRows`, SHA-256). `skip` and
    `take` dump in batches.
  - The relic column and the skill columns depend on the height of the main window's screen.
- **`item_properties_dump(path?, panel?, x?, y?, skip?, take?)`** *(Debug)*: opens the Item Properties dialog (context
  menu > Properties) on every owned item (same scope and order as `tooltip_text_dump`), or on one panel cell's item.
  - Reads what it shows with the "filter extra info" box unchecked (`all`) and checked (`filtered`): the item name and
    its color, then the base, prefix and suffix sections with their label, visibility and rows (`Text`, `Color` and
    `SelectionColor` as colours).
  - The dialog is shown transparent off screen, reused for every item, closed at the end; nothing is changed.
  - With `path`, written to that JSON file; the result is a summary with its SHA-256. `skip` and `take` dump in batches
    (about 1000 items in 2 minutes).
- **`recent_log(n)`** *(Debug)*: the last `n` log lines, kept in memory. Every message box the GUI shows through
  `IMessageBoxService` is logged with its caption, buttons and text, and so is the answer to a question.
- **`recent_sounds(n)`** *(Debug)*: the last `n` sounds played. For a named sound, `loaded` and `wavBytes` tell whether
  its data loaded. NAudio converts the level-up MP3 to WAV first.
- **`load_resource(resourceIds)`** *(Debug)*: loads resources (textures, sounds; ids as the database references them,
  `/` or `\`, comma separated) through the database like the app, custom map first. Per id: the game archive it maps
  to, whether that archive exists, `loaded`, `bytes`. Also gives the custom map in use. Checks sounds TQVault never
  plays itself.
- **`picture_fault(resourceId, mode)`** *(Debug)*: makes a resource read as `missing` (no data, no texture header) or
  `corrupt` (only its first 64 bytes: the texture header reads, the picture cannot be decoded) from now on; `clear`
  undoes it. Returns the faults in place.
  - No file is touched; the fault lasts until the app ends.
  - Pictures already decoded are forgotten. Items already loaded keep the size read at their load: build one with
    `create_item` to see the size rule.
  - Checks how an item whose record is found but whose picture cannot load is shown: a warning tile of its size
    (texture header, else one cell), a working tooltip, one warning in `recent_log` per picture.

### Captures

- **`capture(control, shown?)`**: a PNG made with `Control.DrawToBitmap`, returned as an image and saved under `%TEMP%`.
  Works when the window is behind others.
  - Like every tool saving its picture, the last text block reads `<name>: <width>x<height> px, saved to <file>`, the
    file `tqvault-capture-<name>-<time>.png`.
  - `shown=true`: the pixels the window really shows on its desktop (the hidden one in headless mode): its top-level
    window copied from the desktop composition (`PrintWindow` with full content), cropped to the target. Use it for
    documentation pictures and whenever `DrawToBitmap` may differ from the screen (overlaps, transparent children, the
    character list opened over the main window: `capture("characterList")` then `capture("main", shown=true)`).
  - Targets:
    - a panel id; `<panel>.container` (the panel with its bag buttons); `main`;
    - `tooltip` (the last tooltip shown); `tooltip:<type>` (the shown tooltip of that type when several show at once,
      e.g. `tooltip:BagButtonLabelTooltip`);
    - `characterList[:name]` (opens the character list, scrolled to a character);
    - `playerInfo` (the stash panel with its statistics panel and Edit button drawn over it, since `DrawToBitmap` draws
      the equipment over them);
    - part of the title of a shown form, or the title or type name of the view shown in the main window (`Settings`,
      `Character Editor`).
- **`capture_menu(target, x?, y?, open?)`**: opens a menu like the user and renders it to PNG, sub menus opened along
  `open` drawn to the right, level with their entry (drawn selected, as under the mouse); then closes it without
  choosing. Saved under `%TEMP%` too. `target`:
  - `vaultTools` (the vault tools button);
  - `characterTools` (the tools button next to the character list, as it opens for the selected character: *Archived*
    tick, tags and their ticks);
  - a panel id with the cell `x`, `y`, for its right-click menu;
  - `<panel>.sort` (`vault.sort`, `player.sort`, `playerBag.sort`, `stash.sort`...): that panel's sort button menu;
  - `<panel>.bag<N>` (`vault.bag3`, `stash.bag1`...): that bag button's right-click menu (the vault bag menu, which
    selects that bag, or a stash tab's tool tip menu);
  - `loadouts`: the loadout bar's right-click menu; `loadouts.list`: its drop-down list.

### Forge

- **`forge_show(show)`**: shows or hides the forge, like the Forge button. Pick items for the forge from the `vault`
  panel with `pick_item`.
- **`forge_state`**: mode, hardcore, the item in each slot (`Base`, `Prefix`, `Suffix`, `Relic1`, `Relic2`) with the
  parts it offers in God mode, the preview of the forged item, the messages shown to the user.
  - `layout`: the panel's size against its height limit: `width`, `height`, `autoScale` (the Windows Forms scaling mode
    with its design and current dimensions), `preferredHeight`, `heightLimit`, the slots' `slotCut` and `slotHeight`,
    its table's `rowHeights`, the visible God mode lists (`combos`).
- **`forge_set_mode(mode)`** / **`forge_set_hardcore(enabled)`**: like the mode radio buttons (`Game`, `Strict`,
  `Relax`, `God`) and the Hardcore box. A stricter mode clears the slots, as in the app.
- **`forge_mode_card(mode)`**: read only. The card shown when the mouse rests on a mode radio button: title, summary,
  sections with their lines (`+` allowed, `-` restricted), footer, and a PNG of the card drawn as the tool tip draws it.
  The forge must have been shown once.
- **`forge_drop(slot)`**: drops the dragged item on a forge slot like the user: the virtual cursor moves onto the slot's
  picture (the panel hovered before gets its mouse leave) and a click goes to the picture with mouse messages; the
  forge's click handler drops on the slot under the cursor, read from the virtual cursor.
  - The current mode's rules decide. A refusal is reported in `newMessages`; the item stays dragged, the virtual cursor
    on the slot.
  - A slot hidden in the current mode is refused.
- **`forge_select_part(slot, part)`**: God mode: chooses which part of a material item is merged, like its combo box.
- **`forge_drag_cursor`** *(Debug)*: runs the drag cursor update of a mouse move over the forge while an item is
  dragged: whether the cursor was built from the item's bitmap (native `CreateIconIndirect`), its size and hot spot.
- **`forge_hover(slot, hide?)`** *(Debug)*: rests the mouse on a forge slot through the slot's own mouse handler: the
  item tooltip shows; for the `Base` slot, the preview of the forged item.
  - Returns the tooltips shown, the tooltip window and the forge on screen, and a PNG of the main window with the
    tooltip drawn at its place (magenta where the tooltip leaves the window).
  - `hide=true` ends the hover and closes the tooltip.
  - Refused while an item is dragged (the forge shows no tooltip then).
- **`forge_reset`** / **`forge_cancel`** / **`forge_forge`**: like the Reset, Cancel and Forge buttons; refused while
  the button is greyed or hidden, as for the user. `forge_forge` merges into the base item, saved with its vault or
  character like any change, and reports the forged item and the sounds played.

Every forge tool returns the forge state, with the `newMessages` and, in Debug builds, the `sounds` the call produced.

### Loadouts

The loadout bar, the loadout panel and the marks (see `documentation/LOADOUTS.md`). Bounds are in the stash panel's
pixels unless noted.

`loadouts(action?, name?, newName?, menu?, to?, gameRunning?, realGame?, slot?)` reads:

- `bar`: `bounds`, `field`, `add`, `names`, `selected`, `canChange`, `equipmentLocked` (why Gear swap is greyed),
  `copyLocked` (why Update and Copy are greyed), font.
- `layout`: `statsPanel`, `editButton` (visibility, font), `stashClient`.
- The loadouts file (`file`, `fileExists`, `readFailed`, `modified`), each loadout's pieces (record, seed, `worn` when
  identical to the worn piece) and the worn pieces.
- `marks`: `slot`, `match` (`Same`, `Different`, `OneEmpty`), `canCompare`, `bounds` (equipment panel pixels).
- `panel` (the loadout panel): `visible`, `bounds`, `loadout`, `panelBounds`, `cellScaleFactor`, `unitSize`, the
  `back`, `previous`, `next`, `swap` and `empty` buttons (view pixels), `canTurn`, `canSwap`, `swapLocked` and
  `emptyLocked` (the greyed button's tool tip), `statsPanelVisible`, `slots`.
- `menu` = `list` or `actions`: the entries of the drop-down or of the right-click menu, with `tip` (a submenu's `items` too).

`action`, through the bar's and panel's own handlers:

| Action | Does | Who |
|---|---|---|
| `select` (`name`) | Picks the loadout in the box; opens the loadout panel | test characters |
| `add` (`name`) | Clicks [+] | test characters |
| `update`, `rename` / `copy` (`newName`), `delete`, `swap` | Clicks the right-click menu entry, on `name` or the selected loadout | test characters |
| `empty` (`to`: `inventory`, `stash`, `vault`) | Clicks the entry of the Empty loadout submenu, on `name` or the selected loadout | test characters |
| `swapButton` | Clicks the panel's Gear swap button | test characters |
| `emptyButton` (`to`) | Clicks the panel's Empty loadout button, then the destination in its menu | test characters |
| `back`, `previous`, `next` | Clicks the panel's arrows | any character |
| `compare` (`slot`) | Clicks a mark: Item Comparison in a new window | any character |
| `hover` (`slot`) | Hovers a mark, returns `markTooltip`; no `slot` leaves the panel | any character |

- Greyed actions are refused, as the user would be.
- Names are typed into the name dialog as its queued `text:` answer.
- Actions other than `select`, `compare`, `hover` and the arrows run after the call returns: queue the answers of their
  boxes first (`queue_dialog_answers`).
- `gameRunning=true/false` makes the next actions take the game as running or not; `realGame=true` restores the real
  check.
- The panel's items are driven as panel `loadout` (`pick_item`, `drop_item`, `cancel_drag`, `get_state`, `hover_item`,
  `context_menu`); `save_files` writes the loadouts file.

`select_search_result` gives a loadout result's `loadout` and opens it in the panel. `search_containers("Loadouts")`
lists the loadout pieces, one container per loadouts file.

### How drags are simulated

- The real cursor never moves. The tools call the panels' own `MouseMove` / `MouseDown` / `MouseLeave` handlers with
  synthetic `MouseEventArgs`; a Debug-only `VirtualCursor` replaces `Cursor.Position` in `SackPanel.PaintCallback`.
- After each move, the panel is painted synchronously, so the regular code computes the drop target cells
  (`CellsUnderDragItem`).
- Code that hit-tests the cursor on the screen outside a mouse event (the forge's click, dropping on the slot under the
  cursor; the end of an icon drag in the bag settings) reads `ScreenCursor.Position`, which gives the virtual cursor
  while automation holds it, in Debug builds only.
- All the app's rules still apply: item requirements, slot types, stacking and swapping.

### Example: forge in God mode

```text
forge_show()                                 -> forge shown, mode Game
forge_set_mode("God")
pick_item("vault", 0, 3); forge_drop("Base")  -> base item in the Base slot
pick_item("vault", 0, 1); forge_drop("Prefix")
forge_select_part("Prefix", "Prefix")        -> preview: base item with the other item's prefix
forge_forge()                                -> forgedItem, sounds: PlayLevelUp UI_LEVELUP.MP3 loaded, wavBytes 626732
```

In Debug builds, `ISoundService` is wrapped by `RecordingSoundService`, which records every sound for `recent_sounds`.

## Safety

- **`PlayerReadonly`** (on by default): `pick_item` and `drop_item` refuse the equipment panel, as the panel ignores
  real clicks. Hovering it with `move_drag` is allowed.
- **Read-only characters**: their panels refuse `pick_item`, `drop_item` and the moves with their own rules (and say so
  in the notification), so automation cannot change one either. Toggle the state only on test characters
  (`character_read_only`), and make them writable again before deleting them.
- **Test files**: nothing stops automation from saving. TQVault saves as for a user: modified characters, vaults and
  stashes on Save and on exit, its configuration, character duplication, vault import, and the git backup when enabled.
  So automation works only on its own files, never the user's:
  - vaults named `vault-*` (create them as needed: `vault-test1`, ...);
  - characters named `player-test*` or `player-mod*`, made with Duplicate Character from a real character (the copy is
    a full save set: player file, stash and all);
  - these neutral names may show in documentation captures (usually from a separate home folder, see below);
  - change nothing else: a real character or vault may be selected and read, but no item is moved, dropped, created or
    forged in it. If one was changed by mistake, close the app without saving (end the process).
  - With git backup enabled, the test files are committed and pushed with the others.
- **Name guard** (Debug builds):
  - The tools writing a character or vault file (`create_vault`, `duplicate_character`, `import_file`, `import_text`,
    `open_mod_link`, `archive_character`, `tag_box` assign, `menu_click` on the character tools and loadout menus,
    `class_sets` actions) refuse names starting with none of `player-test`, `player-mod`, `vault-`. Tags must start with `test-`.
  - For captures without a real name, start the app with `USERPROFILE` pointed at a folder holding a copy of
    `Documents\My Games` with only such files (junctions to the real custom maps if needed), and `VaultPath` set to a
    vault folder of copies.

## Example session

```text
get_state()                                  -> characters, vaults, panels
select_character("player-test, Warrior - Level : 67")
select_bag("stash", 0)                       -> equipment panel shown
get_state("player")                          -> helmet at x=0, y=0, 2x2
pick_item("player", 0, 0)                    -> dragging, cellsUnderItem (0,0 2x2)
move_drag("equipment", 4, 0)                 -> hoveredPanel equipment, cellsUnderItem (4,0 2x2)
capture("equipment.container")               -> helmet drawn over the head slot, green highlight
drop_item("player", 0, 0)                    -> accepted, dragging false
capture("player")
```

Dropping an item back on its own cells re-places it and marks the sack modified, exactly like a real drop. Use
`cancel_drag` to put it back untouched.

## Limitations

- Task dialogs (`TaskDialog`) draw their buttons without child windows: `close_dialog` only knows their standard
  buttons (OK, Cancel, Yes, No, Retry, Close...), through their command ids.
- Tools needing the main window answer "not ready" until it is created. `list_windows` and `close_dialog` work from the
  start.
- `capture("equipment")` shows the item grid only; `equipment.container` includes the stash tabs.
- `capture` of `main` or `equipment.container` does not show the character statistics panel and its Edit button as the
  screen does (`DrawToBitmap` draws the equipment over them): use `capture("playerInfo")` for a picture of them,
  `player_info` for their content.
- The real mouse still works while automation drives the app. Moving it over the panels during a drag repaints them,
  but the virtual cursor keeps deciding where the dragged item is drawn and dropped.

## ARZExplorer endpoint (`arzexplorer` MCP)

Debug builds of `ARZExplorer` host their own MCP server, built the same way (`src/ARZExplorer/Automation/`), at
`http://127.0.0.1:47020/mcp` (`ARZEXPLORER_MCP_PORT` to change it, `ARZEXPLORER_MCP_DISABLED=1` to turn it off).
Release builds contain none of it.

Code shared by both endpoints, in `src/TQVaultAE.Automation/`: the server start (`McpEndpoint`), control pictures
(`ControlImages`), the headless mode's hidden desktop (`HeadlessDesktop`), the calls on the main form's UI thread
(`UiThread`), the control walk (`ControlTree`), the window helpers (`Win32Window`). TQVault references that project in
every build, ARZExplorer in Debug builds only; each keeps its own part in its
`Automation/Headless.cs` (desktop name, how a cancelled dialog is recorded, ARZExplorer's muting).

- `ARZExplorer.exe --headless` runs it on a hidden desktop, like TQVault.
  - ARZExplorer calls `MessageBox.Show` directly, so every message box and file dialog there is cancelled as it opens
    and recorded with its caption and text: read them with `dialogs`.
  - Its audio volume is set to 0 (`dialogs` reports `muted`): sounds play through the normal code path without reaching
    the speakers.
- Stop it with `exit`, or kill your own process id.

| Tool | Does |
|---|---|
| `get_state` | Title, opened files, node count, top tree nodes, selected node, path box, status bar, shown view (picture, grid, text, sound), picture size. Fails within 5 s when the UI thread is blocked. |
| `open_file(path)` | Opens an ARC or ARZ file as the Open menu does. |
| `list_items`, `click_item(name)` | Menu and toolbar items (name, text, enabled, checked); a click on one (`toolStripButtonLoadDataBase`, `loadAllFilesToolStripMenuItem`, `toolStripButtonCaps`, `copyDBRToolStripMenuItem`...). |
| `search(text, forward)` | Types in the search box and runs Find Next or Find Previous (toolbar, F3); returns the time taken. |
| `clipboard` | What a Copy menu item put on the clipboard: text (start, length, lines), image size, audio length. |
| `extract(path, all)` | Extract > Selected File (`all=false`, to the file `path`) or Extract > All Files (`all=true`, under the folder `path`), without the file dialogs. Write under a temporary folder. |
| `find_nodes(contains, max)` | Keys of the leaf nodes containing a text (`.tex`, `U_E_LYPIKOURI`). |
| `select_node(key)` | Selects a node as a click does: record grid, text, picture or sound. |
| `grid_links`, `click_grid_cell(row, column)` | The link cells of a DBR record grid; a click on one (navigates, loading the archive it needs). |
| `check_links(contains, maxRecords, render, path)` | Every link of the database records (see below). |
| `decode_tex(key)` | Reads and decodes a TEX off the UI thread, with timings: tells a decoder problem from a UI one. |
| `capture` | PNG of the window (`DrawToBitmap`). |
| `about` | Help > About without blocking: its texts and a PNG, then closes it. |
| `drop_files(paths)` | Drops files on the window through its DragEnter and DragDrop handlers, as Explorer does. |
| `sound_player(action)` | The sound player of a .wav or .mp3 entry: `state`, or a click on `play`, `loop`, `stop`; the sound is then loaded, which checks its wave data. |
| `forms`, `close_form(name)` | The open forms (name, title, visible, modal); closes one as its title bar X does (see below). |
| `mouse_click(form, control, x, y)`, `alt_f4(form)` | User input through the message loop, returning at once (see below). |
| `process_resources(collect)` | GDI and USER object counts, handles, private and managed bytes, and the disposables the root service provider keeps until exit, by type. `collect=true` runs a full GC with finalizers first. Leak checks: compare before and after repeating an action. |
| `dialogs(clear)` | Headless state and the dialogs cancelled there. |

- **`check_links`**: load everything first (`click_item("loadAllFilesToolStripMenuItem")`). Checks the grid's link
  detection, `NavigateTo`'s resolution, the archives a click opens with only the database loaded, and the loading of
  every distinct target (record, texture decoded, text, sound converted and loaded by a SoundPlayer as the sound view
  does). Reports problems by kind with examples; the full list goes to the JSON file `path`. About 4 min for the whole
  database.
- **`close_form`**: posts `WM_SYSCOMMAND SC_CLOSE` (close reason `UserClosing`) and returns at once; works while
  `extract(all=true)` still waits on its progress dialog (`ArzExtractProgress`).
- **`mouse_click`**: mouse move, left button down and up posted to a control (its centre by default), e.g. the progress
  dialog's `cancelButton`. **`alt_f4`**: Alt+F4 posted as `WM_SYSKEYDOWN`/`WM_SYSKEYUP` with the Alt flag to the focused
  control, which the default window procedure turns into `SC_CLOSE` (the keyboard state is not changed). Both work while
  `extract(all=true)` still waits.

The UI tools take a timeout and report "blocked (hang)" instead of waiting forever. The call still runs once the UI
thread is free: poll `get_state` to see when.
