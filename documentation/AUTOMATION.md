# Automation endpoint (Debug builds only)

Debug builds of TQVault host an [MCP](https://modelcontextprotocol.io) server inside the application.
An AI agent (Claude Code, or any MCP client) can use it to inspect and drive the running UI
without screenshots or real mouse clicks.

The item grids (`SackPanel`, `EquipmentPanel`) are owner-drawn, so UI Automation sees each one as a single pane.
This endpoint reads the app's own models instead, and drives drags through the panels' own mouse handlers.

**Release builds contain none of this.** The `Automation/` folder, the `ModelContextProtocol.AspNetCore` package
and the ASP.NET Core framework reference are only part of the `Debug` configuration, and every hook
in existing code is inside `#if DEBUG`.

---

## Transport and address

- Transport: MCP streamable HTTP, through the official C# SDK (`ModelContextProtocol.AspNetCore`) on Kestrel.
- Address: `http://127.0.0.1:47017/mcp`. It listens on loopback only.
- It starts with the application, before the first dialog, so startup message boxes can already be answered.
- The address is written to the log at startup:
  `Automation MCP endpoint listening on http://127.0.0.1:47017/mcp`

| Environment variable | Effect |
|---|---|
| `TQVAULT_MCP_PORT` | Listen on another port. |
| `TQVAULT_MCP_DISABLED=1` | Do not start the endpoint. |

## Headless mode (`--headless`)

`TQVaultAE.exe --headless` (Debug builds only) runs TQVault on a hidden Win32 desktop of its own, so the user sees
no window, no taskbar button and no focus change while an agent drives it through the endpoint.

- The process started with the switch creates the desktop, starts TQVault again on it and exits once that instance
  is idle: one `TQVaultAE.exe` is left, and the endpoint comes up as usual (wait for it, about 5 s).
  An instance restarted by Duplicate Character stays on the hidden desktop.
- Every window is still created, shown, laid out and painted at its normal place on the primary monitor, with that
  monitor's DPI: `capture`, `hover_item`, `control_tree`, `open_form`, the forge and every other tool work as usual.
  `list_windows` gives the desktop name as `headless`.
- Message boxes are never shown. A queued answer (`queue_dialog_answers`) is used first; without one, the box takes
  the answer that changes nothing (Cancel, else No, else Ignore, else OK), which is logged and listed by
  `dialog_answers` under `unanswered`. This covers the startup notices, the startup git sync questions, the
  vault import dialog (which takes Cancel) and the "Windows Forms Error" box too.
- Native dialogs (folder browser, open/save file, color picker) are cancelled as soon
  as they open, and listed by `dialog_answers` under `cancelledNativeDialogs`.
- Fatal errors: a "Windows Forms Error" before the main form has loaded takes Abort (TQVault quits) instead of Ignore.
  When startup fails (the global error box of `Program.Main`), the box is listed under `unanswered`, the endpoint
  stays up 10 s so `dialog_answers` and `recent_log` can report it, then the process exits with code 1.
  Without `--headless` that box is no longer a `DefaultDesktopOnly` box in Debug, so `list_windows` and
  `close_dialog` see it, and the process also exits 10 s after it is answered.
- Nothing can be seen or clicked by hand. To stop it, use the Exit button (`click_control("TQVaultAE", "exitButton")`),
  which saves like a normal exit, or end the process.
- Use another port (`TQVAULT_MCP_PORT`) when a visible Debug instance already listens on 47017.

```bash
TQVAULT_MCP_PORT=47018 src/TQVaultAE.GUI/bin/Debug/net10.0-windows/TQVaultAE.exe --headless
```

Release builds ignore the switch: the code is in the Debug-only `Automation/` folder, and the call is in `#if DEBUG`.

## Register it in Claude Code

The repository's `.mcp.json` already registers it as `tqvault`. Follow these steps:

1. Start a Debug build: run `dotnet build TQVaultAE.slnx`, then `src/TQVaultAE.GUI/bin/Debug/net10.0-windows/TQVaultAE.exe`.
2. Open Claude Code at the repository root, and approve the project server when you are asked.

The tools show up as `mcp__tqvault__*` in sessions started at the repository root. A local registration counts only for the folder the command runs in, not the whole repository. To register the server by hand from another folder, use:

```bash
claude mcp add --transport http tqvault http://127.0.0.1:47017/mcp
```

If you use another port (`TQVAULT_MCP_PORT`), update the URL in `.mcp.json` too.

## Tools

All UI access is marshalled to the UI thread with `Control.Invoke`.
Panel ids are `vault`, `secondaryVault`, `player` (main inventory), `playerBag`, `stash` and `equipment`.
Cells are grid coordinates, as reported by `get_state`.

| Tool | What it does |
|---|---|
| `list_windows` | The process' visible top-level windows: WinForms forms and native message boxes, with title, class, modal state, message text and buttons. |
| `close_dialog(title, button)` | Presses a dialog button by its text, for example `("Default Vault Path", "OK")`. It works for message boxes and WinForms forms, even when they are not in the foreground. |
| `list_forms` | The form and dialog types registered in the container. |
| `open_form(name, show)` | Creates a form or dialog from the container, as `FormFactory` does, so its `InitializeComponent` and resources run. It reports the title, size and control count. With `show=false` (the default) the form is then disposed; with `show=true` it opens modeless over the main window, for `capture` and `close_dialog`. Singletons (`MainForm`, `BagButtonSettings`) are only reported. Dialogs that need data first may fail when shown; the error goes to `recent_log`. |
| `open_item_dialog(panel, x, y, dialog)` | Opens the `properties` or `seed` dialog of the item at a cell, like its context menu entries but modeless, to check its layout with `text_fit` and `capture`. Close it with `close_dialog`. The Seed dialog's OK only validates the typed seed (an invalid one shows its error box); applying it is the context menu entry's job, so use `context_menu_click(..., "Seed")` to change a seed. |
| `open_input_dialog(prompt?, title?, value?)` | Opens the text input dialog (used to name a duplicated character) modeless, to check its layout; close it with `close_dialog`. |
| `open_bag_settings(panel)` | Opens the bag button settings (icon and label picker) for the current bag of a panel, like its "Change icon" menu entry. It waits for the game icons and reports how many loaded. The dialog is modal: capture it with `capture("Bag Button Settings")` and close it with `close_dialog`, using the Cancel button's text in the game language (for example "Annuler"). |
| `bag_menu(panel, bag, path?)` | Right-clicks a bag button like the user (it selects that bag) and returns the bag context menu (move, copy, merge, empty, tooltips, CSV and JSON export, change, copy and paste icon, import) and the icon settings of every bag; with `path` it clicks that entry after the call returns, for example `Copy Bag > Bag #3` or `Change icon`, which opens the bag button settings. Only vault bags have this menu. The CSV and *Share to Clipboard* entries write to the clipboard the user shares with the hidden desktop: do not click them. |
| `bag_settings_pick_icon(tab, index)` | In the open bag button settings, right-clicks an icon of a tab's grid (Skills, Relics, Artifacts...) like the user: it picks the icon's pictures and switches to Custom icon. Apply with `click_control('Bag Button Settings', 'applyButton')`. |
| `highlight_filters(check?, uncheck?, apply?)` | Opens the highlight filters panel of the main window (the "Highlight items having" link), ticks and unticks its check boxes by text or name (`"Having Skill,Set Item"`), and applies the filters like its Apply button when `apply` is true. Returns the check boxes and how many items are highlighted. Untick them and apply again, or use the panel's Reset, to clear the highlight. |
| `get_state(panel?)` | The selected vault and character, the available characters and vaults, `PlayerReadonly`, the drag state, every panel (shown, sack type, bag, item count, modified). With `panel`, it adds the items of that panel's current sack: name, base name, record id, `missingRecords` (role and id of each record the loaded game data lacks, for an incomplete item; else null), cell location, size, seed and `place`, where the search shows the item (container name and file, sack type and number, stash type). For `equipment` it also lists the slot rectangles. |
| `search_entries(baseItemId)` | The search database entries of one base item record id: their number and, for each, the place (container name and file, sack type and number, stash type), cell and stack size. Checks that a moved or reloaded item has exactly one entry, at the right place. |
| `owned_skill_items` | The owned items that grant a skill, from the search database (vaults, characters and stashes when *Load all files* is on): record ids, seed, vars, stack size, size in cells, location, and the skills with their level and the record that grants them. It writes nothing; its output is enough to write a vault file. |
| `raw_tooltip_lines(items?, examples?)` | Scans every owned item and reports the tooltip lines still shown raw (`?name?`, `<unknown>`) in the granted skills, and with `items=true` in the item attributes too: grouped by raw name, with the count, where it appears and a sample line. |
| `tooltip_text_dump(path?, views?, panel?, x?, y?, skip?, take?)` | Builds the item tooltip of every owned item (same scope as `raw_tooltip_lines`), or of the item on one panel cell, with the tooltip's own `FillToolTip`, without showing it and bypassing its image cache, and reads back the rows it created, in display order. `views` is a comma separated list of `classic`, `detailed` (detailed with the extras checked in the settings), `full` (detailed with every extra: in-game rolls, value ranges, granted skills, set piece counts) and `settings` (what a hover shows now); default `classic,full`. Each item has its location, record ids, vars, seed and stack size, and per view its rows: `kind` (`text`, `spacer`, `delimiter`, `range`, `icon`, `separator` between two skills of a column, `column` before the relic column or a skill column, with `text` = `relics` or `skills`), `text`, `color` (`#RRGGBB`), `bold`, `segments` for a line with several colors and `range` for the value on the right. Items are sorted by container, sack and cell, with no timestamp, so two dumps can be diffed. With `path`, the dump is written to that JSON file and the result is a summary (counts, rows, `unknownRows`, SHA-256); `skip` and `take` dump the owned items in batches. The relic column and the skill columns depend on the height of the main window's screen. |
| `item_properties_dump(path?, panel?, x?, y?, skip?, take?)` | Opens the Item Properties dialog (context menu > Properties) on every owned item (same scope and order as `tooltip_text_dump`), or on the item of one panel cell, and reads back what it shows with the "filter extra info" box unchecked (`all`) and checked (`filtered`): the item name and its color, then the base, prefix and suffix sections with their label, whether they are visible and their rows (`Text`, `Color` and `SelectionColor` as `#AARRGGBB`). The dialog is shown transparent off screen, reused for every item and closed at the end; nothing is changed. With `path`, the dump is written to that JSON file and the result is a summary with its SHA-256; `skip` and `take` dump in batches (about 1000 items in 2 minutes). |
| `select_character(name)` / `select_vault(name)` | Select an entry in the drop-downs, as listed by `get_state`. The first vault entry, *Maintain Vault Files...*, opens the modal maintenance dialog after the call returns: drive it with `control_values`, `set_control` and `click_control` (form `VaultMaintenanceDialog`). |
| `select_bag(panel, bag)` | Clicks a bag button. `stash` bag 0 shows the equipment panel. |
| `pick_item(panel, x, y)` | Picks up the item at a cell, like a left click there. |
| `move_drag(panel, x, y)` | Hovers the dragged item so that its top-left corner lands on cell (x, y). The result reports the cells under the item. |
| `drop_item(panel, x, y)` | Drops the dragged item with its top-left corner on cell (x, y). It reports whether the panel accepted the drop. |
| `cancel_drag` | Returns the dragged item to where it was picked up, like a right click. |
| `create_vault(name)` | Creates a test vault like the New action of *Maintain Vault Files...*, then selects and loads it. The name must start with `claude-` (`claude-vault1`, ...). The file is written on the first save, so an empty vault that is never changed is not kept. |
| `export_vault(path)` | Exports the vault shown to a JSON file, like the vault menu's "Export to file" without its file dialog. |
| `export_items(panel, x, y, path)` | Right-clicks a cell, then runs the item menu's `🔗 Share Item` → `Share to File...` without its file dialog: the selection (`select_items`) when there is one, else the item under the cursor. `exported` is false when there was nothing to export. Never touches the clipboard or PasteBin. |
| `export_csv(path, bag?)` | Writes the CSV export of the vault shown (or of bag `bag`, zero-based) to a file, like the vault context menu's *Export CSV* entries without the clipboard, with the CSV delimiter of the settings. |
| `import_file(path)` | Imports an exported JSON file like "Import from file" without its file dialog: items go into the current tab; a whole vault asks to replace the vault shown or import it as a new vault (`queue_dialog_answers`: `Yes` replaces, `No` creates `<name>_<date>_<time>`, `Cancel`). The vault shown must be a `claude-` test vault. Queue the answers first. |
| `duplicate_character(name)` | Duplicates the selected character like *Duplicate Character* (player file, stash and all) under a name that must start with `claude-` (`claude-character1`, ...). TQVault then restarts, as it does for a user; the endpoint comes back within a few seconds (it retries while the old process still holds the port): wait for `loadingComplete`, then select the copy. |
| `save_files` | Saves the modified characters, vaults and stashes like the Save button. A file error (locked or read-only file) shows the Abort/Retry/Ignore box and the call waits until it is answered, so answer it from another call with `close_dialog`. Returns whether characters were saved and the panels still modified. |
| `control_values(form, labels?)` | Reads the input controls of an open form or dialog (check boxes, radio buttons, numeric boxes, combo boxes, text boxes, check group boxes): name, type, enabled, visible, value. |
| `set_control(form, control, value)` | Sets one control like a user, so its own events run; a text box then gets Validating, Validated and Leave, as when the user leaves it. Refuses a disabled control. Returns the form's input controls afterwards, to check the dialog's rules. A combo box or a list box takes an item text or `#index`; an editable combo box (the search dialog's query name) takes any text. |
| `click_control(form, control)` | Clicks a button of an open form (`''` for the main window) after the call returns, since it may open a modal dialog. `configureButton` opens the settings from the main window, so its OK runs the restart rules. |
| `open_character_editor` | Opens the character editor of the selected character, like the Edit button of the stash panel; read and change it with `control_values`/`set_control` (form `CharacterEditDialog`), close it with `close_dialog`. Its Skills box has one page per mastery slot: `SkillsMasteryNComboBox`, `SkillsMasteryNLevelUpDown` and the grid `SkillsMasteryNGrid` (N = 1, 2). Check the selected character with `get_state` first: the editor opens on whichever one is selected. |
| `mod_state` | The custom map (mod) data of the session: active map, maps whose database is loaded, the active transfer stash and relic vault files, the installed mods, and per listed character its link state, mod, map and whether it was read with another map's database. Read only. |
| `mod_suggest(character)` | Runs the mod link dialog's Suggest on any listed character (reads its `Player.chr` and `winsys.dxb` only): records the base game lacks, and the installed mods having them, best first. Links nothing. |
| `open_mod_link` | Opens the mod link dialog of the selected custom map character, like *Mod link...* in the character menu (refused for non-`claude-` characters, since Link and Unlink write `TQVaultAE.ModLink.json` in the save folder). Read it with `control_values(form 'ModLinkDialog', labels true)`, pick a mod with `set_control(listBoxMods, '<display name>')`, then `click_control` `buttonSuggest`, `buttonLink`, `buttonUnlink` or `buttonClose`; closing after a change reloads the character with its new mod's data. |
| `grid_values(form, grid)` | Reads a `DataGridView` of an open form: column headers, and per row the text of each cell (`image` for pictures), the row text color (the character editor greys out locked skills) and the cell tool tip. |
| `set_grid_cell(form, grid, row, column, value)` | Changes a grid cell like a user: a button cell is clicked (`CellContentClick`, e.g. the `+`/`-` columns of the skill grids, by index 5 and 3), a text cell is edited then left (`CellEndEdit`). Row: index or the text of one of its cells (a skill name); column: index or header text. Read the grid again afterwards. |
| `tag_box(box, text, tag?)` | Types into the add or rename tag box of the character menu and presses Enter; `delete` removes a test tag; `assign` and `unassign` tick or untick an existing tag for the selected `claude-` character. Names must start with `claude-`. `list_tags` lists the tags and those of the selected character. |
| `archive_character(archived)` | Ticks or unticks *Archived* in the character menu for the selected character, which moves its save folder to or from `ArchivedCharacters`. Only characters whose name starts with `claude-`; the global *Archive all* entries are never driven, since they move every character. |
| `tag_archive_all(tag, archived)` | Clicks *Archive all* or *Unarchive all* in the menu of a tag, which moves the save folders of the characters having that tag. Only a `claude-` tag whose characters are all `claude-` characters; returns those characters with their state and folder. |
| `queue_dialog_answers(answers)` | Queues answers (`OK`, `Cancel`, `Yes`, `No`, `Abort`, `Retry`, `Ignore`, comma separated) for the next message boxes, which are then answered at once without being shown, so nothing waits for a click (startup git sync, confirmations, save errors). A box takes the next answer only when it is one of its buttons; an OK-only box takes only `OK`. `clear` empties the queue. `dialog_answers` lists the boxes answered and the answers left and, in headless mode, the boxes that took their default answer and the native dialogs cancelled. The startup notices are covered, and so is the vault import dialog: `Yes` replaces the vault, `No` imports it as a new vault, `Cancel` cancels. |
| `create_item(panel, baseItem, prefix?, suffix?, relic?, relicBonus?, relic2?, relicBonus2?, seed?)` | Builds an item in memory from database record ids and drags it, like the Duplicate menu entry; place it with `drop_item` (a created item cannot be cancelled back). It tests combinations no vault holds, for example a relic whose completion bonus grants a skill, or any record in any slot as the Forge allows in God mode. It is saved with its vault or character like any item, so create it in a test file (see Safety). |
| `hover_item(panel, x, y)` | Rests the virtual cursor on a cell without dragging. TQVault shows the item's tooltip and writes its name in the notification bar. The result gives the item, the notification text, the tooltips shown, and a PNG of the tooltip taken at once, since a tooltip can close by itself. |
| `hover_end` | Moves the virtual cursor off the panel, which closes the tooltip. |
| `context_menu(panel, x, y)` | Right-clicks a cell through the panel's mouse handlers and returns the context menu built for it: entry texts, `-` for separators, sub menus with their entries. The menu is closed without choosing anything. The current selection counts, as for a real right click. Disabled entries have `Enabled: false`, the current choice (affix, bonus) `Bold: true`. |
| `context_menu_click(panel, x, y, path)` | Right-clicks a cell like `context_menu`, then clicks the entry found by its path of texts (`' > '` between levels, for example `Move Item To > Bag #3` or `Prefix > Arctic : ...`; an exact text first, else the first entry containing it). The click runs after the call returns, since an entry may open a modal dialog (properties, seed, a confirmation): queue the answers first (`queue_dialog_answers`) or follow with `list_windows`. A path ending on a sub menu, or a disabled entry, is refused. |
| `select_items(panel, cells)` | Ctrl-clicks cells (`'x,y;x,y'`) like a user holding Ctrl, which adds each item to the selection or removes it, and returns the selected items; empty `cells` only reads the selection. The context menu then acts on the selection (multiple delete, move, export, clear selection). |
| `press_key(panel, x, y, key)` | Rests the cursor on a cell and presses a key there through the panel's key handler: `c` copies and `d` duplicates the item (picked up, like the menu entries), `backspace` deletes it (with its confirmation: queue the answer first), `escape` cancels a drag. The keys follow the context menu's rules: nothing is deleted from the equipment of a read-only player. Runs after the call returns. |
| `zoom(step)` | Zooms the main window like its shortcuts: `in` (Ctrl +), `out` (Ctrl -), `reset` (Ctrl Home). Returns the zoom, the DPI factor, the scale (their product) and the client size. The zoom is written to `UserConfig.xml` like a user zoom, so end with `reset`. To compare with the 96 DPI layout on a high DPI screen, start the app with the environment variable `__COMPAT_LAYER=DPIUNAWARE`. |
| `move_window(x, y, form?)` | Moves the main window, or the open form whose title contains `form`, to a screen position, in virtual screen pixels (negative on a monitor left of the primary one), without resizing it, as a drag does. On a monitor of another DPI, Windows sends the DPI change and the window scales. Returns the monitors and the window's DPI, zoom, `dpiScale`, `systemDpiScale`, `scale` and `fontZoom`. Move it back to its monitor when done. |
| `control_tree(form, depth)` | Lists the controls of an open form (the main window by default) with type, bounds, font and visibility, plus the form's DPI and the scale, zoom and DPI factor, to check layout and scaling. |
| `text_fit(form, includeHidden, tolerance)` | Measures the text of every label, button, check box, radio button and group box of an open form (the main window by default) in its font, and lists the ones whose text does not fit: wider or taller than the control after word wrap, or a control sticking out of its parent. Run it after switching the UI language (`<TQLanguage>` in the bin's `UserConfig.xml`, then restart) to find translations that overflow. Icon buttons (title bar) are skipped. |
| `capture(control)` | A PNG made with `Control.DrawToBitmap`, returned as an image and also saved under `%TEMP%`. Targets: a panel id, `<panel>.container` (the panel with its bag buttons), `main`, `tooltip` (the last tooltip shown), `characterList[:name]` (opens the character list, scrolled to a character), `playerInfo` (the stash panel with the player information labels drawn over it one by one, since `DrawToBitmap` leaves them out), or part of a form title. It works when the window is behind other windows. |
| `player_info` | Reads the player information panel next to the equipment (stash bag 0) as displayed: one row per line, label and value, in display order (for example Money then Electrum). Empty when no character is selected or another stash tab is shown. |
| `list_items(form, control?, skip?, take?)` | The list controls of an open form: check box lists (the search dialog's categories), list boxes and data grids (the search results, the item properties). Without `control`, each list with its entry count and checked entries; with `control`, its entries with their checked or selected state. |
| `check_list_items(form, control, items, check?)` | Ticks (or unticks) entries of a check box list like a click, so its change event runs: the search dialog updates its filters and result count. Entries by text (exact, else containing) or `#index`, separated by `\|`. |
| `grid_click(form, row, control?, column?, double?)` | Clicks a row of a data grid like the user (current row, cell click, and a double click with `double`). In the results dialog a click selects the character or vault and highlights the item. Runs after the call returns. |
| `recent_log(n)` | The last `n` log lines, kept in memory. Every message box the GUI shows through `IMessageBoxService` is logged with its caption, buttons and text, and so is the answer to a question. |
| `recent_sounds(n)` | The last `n` sounds played. For a named sound, `loaded` and `wavBytes` tell whether its data could be loaded. The level-up MP3 is converted to WAV by NAudio first. |
| `load_resource(resourceIds)` | Loads resources (textures, sounds; ids as the database references them, `/` or `\`, comma separated) through the database like the app, custom map first: per id the game archive it maps to, whether that archive exists, `loaded` and `bytes`. Also gives the custom map in use. Checks sounds TQVault never plays itself. |
| `game_text(tags)` | Translates text tags with the loaded text database (game, expansions, custom map strings); an empty text is an unknown tag. |
| `forge_show(show)` | Shows or hides the forge, like the Forge button. Items for the forge are picked from the `vault` panel with `pick_item`. |
| `forge_state` | Mode, hardcore, the item in each slot (`Base`, `Prefix`, `Suffix`, `Relic1`, `Relic2`) with the parts it offers in God mode, the preview of the forged item, and the messages shown to the user. |
| `forge_set_mode(mode)` / `forge_set_hardcore(enabled)` | Like the mode radio buttons (`Game`, `Strict`, `Relax`, `God`) and the Hardcore box. A stricter mode clears the slots, as in the app. |
| `forge_mode_card(mode)` | The card shown when the mouse rests on a mode radio button (`Game`, `Strict`, `Relax`, `God`): title, summary, sections with their lines (`+` allowed, `-` restricted) and footer, with a PNG of the card drawn as the tool tip draws it. Read only; the forge must have been shown once. |
| `forge_drop(slot)` | Drops the dragged item on a forge slot. The rules of the current mode decide. A refusal is reported in `newMessages`, and the item stays dragged. |
| `forge_select_part(slot, part)` | God mode: chooses which part of a material item is merged, like its combo box. |
| `forge_drag_cursor` | Runs the drag cursor update of a mouse move over the forge while an item is dragged: whether the cursor was built from the item's bitmap (native `CreateIconIndirect`), its size and hot spot. |
| `forge_reset` / `forge_cancel` / `forge_forge` | Like the Reset, Cancel and Forge buttons. `forge_forge` merges into the base item, which is saved with its vault or character like any change, and reports the forged item and the sounds played. |

Every forge tool returns the forge state, with the `newMessages` and `sounds` that the call produced.

### How drags are simulated

The real cursor is never moved. The tools call the panels' own `MouseMove` / `MouseDown` / `MouseLeave` handlers
with synthetic `MouseEventArgs`, and a Debug-only `VirtualCursor` replaces `Cursor.Position` in
`SackPanel.PaintCallback`. After each move, the panel is painted synchronously, so the drop target cells
(`CellsUnderDragItem`) are computed by the regular code.

All the app's rules still apply: item requirements, slot types, stacking and swapping.

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

- **`PlayerReadonly`**: when it is on (the default), `pick_item` and `drop_item` refuse the equipment panel,
  just as the panel ignores real clicks. Hovering it with `move_drag` is allowed.
- **Test files**: nothing stops automation from saving. TQVault saves like it does for a user: modified characters,
  vaults and stashes on Save and on exit, its configuration, character duplication, vault import, and the git backup
  when it is enabled. So automation works only on its own files, never on the user's:
  - vaults named `claude-vault1`, `claude-vault2`, ... (create them as needed);
  - characters named `claude-character1`, ..., made with the Duplicate Character feature from a real character
    (the copy is a full save set: player file, stash and all);
  - change nothing else: a real character or vault may be selected and read, but no item is moved, dropped,
    created or forged in it. If one was changed by mistake, close the app without saving it (end the process).
  With git backup enabled, the test files are committed and pushed with the others.

## Example session

```text
get_state()                                  -> characters, vaults, panels
select_character("Warfare, Guerrier - Niveau : 67")
select_bag("stash", 0)                       -> equipment panel shown
get_state("player")                          -> helmet at x=0, y=0, 2x2
pick_item("player", 0, 0)                    -> dragging, cellsUnderItem (0,0 2x2)
move_drag("equipment", 4, 0)                 -> hoveredPanel equipment, cellsUnderItem (4,0 2x2)
capture("equipment.container")               -> helmet drawn over the head slot, green highlight
drop_item("player", 0, 0)                    -> accepted, dragging false
capture("player")
```

Dropping an item back on its own cells re-places it and marks the sack modified, exactly like a real drop.
Use `cancel_drag` to put it back untouched.

## Limitations

- Task dialogs (`TaskDialog`) draw their buttons without child windows. For those, `close_dialog` only knows the
  standard buttons (OK, Cancel, Yes, No, Retry, Close...), through their command ids.
- The tools that need the main window answer "not ready" until it has been created.
  `list_windows` and `close_dialog` work from the start.
- `capture("equipment")` shows the item grid only. Use `equipment.container` to include the stash tabs.
- `capture` of `main` or `equipment.container` leaves the player information labels out (`DrawToBitmap`): use
  `capture("playerInfo")` for a picture of them, `player_info` for their text.
- The real mouse still works while automation drives the app. Moving it over the panels during a drag
  repaints them, but the virtual cursor keeps deciding where the dragged item is drawn and dropped.

## ARZExplorer endpoint (`arzexplorer` MCP)

Debug builds of `ARZExplorer` host their own MCP server, built the same way (`src/ARZExplorer/Automation/`), at
`http://127.0.0.1:47020/mcp` (`ARZEXPLORER_MCP_PORT` to change it, `ARZEXPLORER_MCP_DISABLED=1` to turn it off).
Release builds contain none of it.

- `ARZExplorer.exe --headless` runs it on a hidden desktop, like TQVault. ARZExplorer calls `MessageBox.Show` directly,
  so every message box and file dialog there is cancelled as soon as it opens and recorded with its caption and text:
  read them with `dialogs`. Its audio volume is set to 0 (`dialogs` reports `muted`): sounds play through the normal
  code path without reaching the speakers.
- Stop it with `exit`, or kill your own process id.

| Tool | Does |
|---|---|
| `get_state` | Title, opened files, node count, top tree nodes, selected node, path box, status bar, shown view (picture, grid, text, sound), picture size. Fails within 5 s when the UI thread is blocked. |
| `open_file(path)` | Opens an ARC or ARZ file as the Open menu does. |
| `list_items`, `click_item(name)` | Menu and toolbar items (name, text, enabled, checked), and a click on one (`toolStripButtonLoadDataBase`, `loadAllFilesToolStripMenuItem`, `toolStripButtonCaps`, `copyDBRToolStripMenuItem`...). |
| `search(text, forward)` | Types in the search box and runs Find Next or Find Previous (toolbar, F3); returns the time it took. |
| `clipboard` | What a Copy menu item put on the clipboard: text (start, length, lines), image size, audio length. |
| `extract(path, all)` | Extract > Selected File (`all=false`, to the file `path`) or Extract > All Files (`all=true`, under the folder `path`) without the file dialogs. Write under a temporary folder. |
| `find_nodes(contains, max)` | Keys of the leaf nodes containing a text (`.tex`, `U_E_LYPIKOURI`). |
| `select_node(key)` | Selects a node as a click does: record grid, text, picture or sound. |
| `grid_links`, `click_grid_cell(row, column)` | The link cells of a DBR record grid, and a click on one (navigates, loading the archive it needs). |
| `check_links(contains, maxRecords, render, path)` | Every link of the database records (load everything first: `click_item("loadAllFilesToolStripMenuItem")`): the grid's link detection, `NavigateTo`'s resolution, the archives a click opens with only the database loaded, and the loading of every distinct target (record, texture decoded, text, sound converted and loaded by a SoundPlayer as the sound view does). Problems by kind with examples, the full list in the JSON file `path`. About 4 min for the whole database. |
| `decode_tex(key)` | Reads and decodes a TEX off the UI thread, with timings: tells a decoder problem from a UI one. |
| `capture` | PNG of the window (`DrawToBitmap`). |
| `about` | Help > About without blocking: its texts and a PNG, then closes it. |
| `drop_files(paths)` | Drops files on the window through its DragEnter and DragDrop handlers, as Explorer does. |
| `sound_player(action)` | The sound player of a .wav or .mp3 entry: `state`, or a click on `play`, `loop`, `pause`, `stop`; the sound is then loaded, which checks its wave data. |
| `dialogs(clear)` | Headless state and the dialogs cancelled there. |

The UI tools take a timeout and report "blocked (hang)" instead of waiting forever. The call still runs once the UI
thread is free, so poll `get_state` to see when it does.

