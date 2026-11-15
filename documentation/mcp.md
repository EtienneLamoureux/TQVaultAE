# AI agents (MCP endpoint)

TQVault can let an AI agent running on your computer (Claude Code, Claude Desktop, or any other
[MCP](https://modelcontextprotocol.io) client) read and drive it: list your characters, vaults and stashes, search
items, move them, apply loadouts, use the forge, and take pictures of the window for your own game documentation.

It is **off by default**. Read the rules below before you turn it on.

---

## ⚠️ Read this first: the rules

An agent connected to TQVault changes the same files as you do with the mouse: your vaults and your character saves.
TQVault sends these rules to the agent when it connects, and the agent must remind you of them. Keep them in mind too.

1. **Local only.** The endpoint listens on `127.0.0.1` (this computer only) and has no password. Never open or
   forward its port to your network or to the internet.
2. **Backups exist, know where.** Before each save, TQVault copies the file it replaces to the `Backups` folder of your
   vault folder (by default `My Documents\My Games\Titan Quest\TQVaultData\Backups`), named
   `<name>-<file>-<date>-<time>-<number>.<ext>`. If you turned *Disable legacy backup* on for the
   [Git Backup](GITBACKUP.md), the git repository holds the history instead.
   **To restore a file:** close the game and TQVault, then copy the backup over the file it came from, under the
   original name: for example `Backups\Main Vault-Main Vault.vault-20261115-101500-1234567-000.json` back to
   `TQVaultData\Main Vault.vault.json`, or `Backups\<character>-Player-<date>-<time>-<number>.chr` back to
   `Player.chr` in the character's `SaveData\Main\_<character>` folder.
   Making your own copy of `My Games\Titan Quest - Immortal Throne\SaveData` and of your vault folder before a big
   session costs nothing.
3. **The agent asks before every change.** It announces each change before doing it (what moves, from where to
   where, which file it changes) and waits for your confirmation. Reading is free; moving, dropping, deleting,
   forging, applying a loadout, importing, creating or saving files are changes. TQVault's own confirmation boxes
   still show on your screen.
4. **Nobody else edits the same files meanwhile:** no second TQVault, save editor or other tool on the vaults and
   saves the agent changes.
5. **The game and your save files.** TQVault cannot tell which character the game has loaded or whether its caravan
   (the storage window) is open, so the agent must ask you.

   | Situation | What the agent may change |
   |---|---|
   | Game closed | Everything. |
   | Game running, on its start or character selection screen | Everything. |
   | Characters that are not the one loaded in play | Everything. |
   | **The character loaded in play** | Its relic bag, transfer stash and player stash **only while the caravan is closed** (the agent asks you to confirm it is closed). Its **equipment and bags: don't** — the game overwrites them at its next save, so the changes are lost (it does not crash). The agent must warn you and advise against it. |

   Vaults are TQVault's own files: the game never reads them, so changing a vault is always safe for the game.

## Turn it on

1. Click the configure button (top-left) to open the settings.
2. Tick **MCP endpoint for AI agents** (right column).
3. Keep the **Port** at `47017`, or pick another one between 1024 and 65535 if that one is used.
4. Click **OK**. The endpoint starts at once, no restart needed, and starts with TQVault from then on. Untick it to
   stop it.

The address is then `http://127.0.0.1:<port>/mcp` (streamable HTTP). TQVault writes
`Automation MCP endpoint listening on http://127.0.0.1:47017/mcp` to its log (`Logging` folder next to
`TQVaultAE.exe`), or the reason when it could not start (usually the port in use: choose another one).

## Connect a client

Most MCP clients take a JSON configuration like this one (the key holding the servers, `mcpServers` here, and the
`type` value vary a little from one client to another: see your client's documentation):

```json
{
  "mcpServers": {
    "tqvault": {
      "type": "http",
      "url": "http://127.0.0.1:47017/mcp"
    }
  }
}
```

With Claude Code, one command does it:

```bash
claude mcp add --transport http tqvault http://127.0.0.1:47017/mcp
```

Start TQVault first, then the client. The tools show up under the name you gave (`tqvault`). Ask the agent to start
with `get_state`: it returns your characters, vaults and the panels shown.

## What the agent can do

Coordinates are item cells (`x`, `y`) as `get_state` gives them. Panels are `vault`, `secondaryVault`, `player`
(main inventory), `playerBag`, `stash` and `equipment`.

| Purpose | Tools |
|---|---|
| Read the state | `get_state` (characters, vaults, panels, items), `character_list`, `player_info` (the statistics panel), `mod_state`, `mod_suggest`, `list_tags`, `key_binds`, `game_text` (translate game text tags) |
| Item search | `search_entries`, `search_containers`, `select_search_result`, `find_copies` (copies of an item: same seed, same base, prefix, suffix), `owned_skill_items`, `highlight_filters` |
| Tool tips and comparison | `hover_item` (an item's tool tip, text and picture), `hover_end`, `hover_bag`, `compare_items` / `compare_click` (the item comparison dialog) |
| Navigate | `select_character`, `select_vault`, `select_bag`, `select_tab` |
| Move items | `pick_item`, `move_drag`, `drop_item`, `cancel_drag`, `click_cell` (clicks with modifiers: copy, duplicate), `select_items` |
| Menus | `context_menu` / `context_menu_click` (an item's right-click menu), `bag_menu`, `vault_menu`, `menu_click` (character tools, loadout menus), `sort_menu` / `sort_click`, `open_bag_settings` / `bag_settings_pick_icon` (bag icons) |
| Loadouts | `loadouts` (read, select, add, update, rename, copy, delete, load; see [Loadouts](LOADOUTS.md)) |
| Forge | `forge_show`, `forge_state`, `forge_set_mode`, `forge_set_hardcore`, `forge_drop`, `forge_select_part`, `forge_reset`, `forge_forge`, `forge_cancel`, `forge_mode_card` (see [Forge](FORGE.md)) |
| Characters | `open_character_editor`, `character_panel`, `character_tooltip`, `character_stat_sources`, `character_attribute_click`, `skill_panel`, `skill_tooltip`, `skill_panel_click` (with *Allow character edit* on), `duplicate_character`, `archive_character`, `character_read_only`, `tag_box`, `tag_archive_all`, `open_mod_link` |
| Files | `save_files` (like the Save button), `create_vault`, `export_vault`, `export_tab`, `export_items`, `export_csv`, `import_file`, `import_text` |
| Dialogs | `list_windows`, `close_dialog`, `close_form`, `open_item_dialog` (an item's Properties or Seed dialog), `queue_dialog_answers` / `dialog_answers` (answer TQVault's next boxes, only after you agreed) |
| Pictures | `capture`, `capture_menu` |

Every tool describes itself to the agent (parameters and result); the full reference, with the developer tools of
Debug builds, is [AUTOMATION.md](AUTOMATION.md).

## Pictures for your game documentation

`capture` renders a PNG of the main window (`main`), a panel (`vault`, `player`, `stash`, `equipment`, ...), the
player information panel (`playerInfo`), the tool tip shown (`tooltip`), or an open dialog by its title. `capture_menu`
renders a menu with its sub menus, `hover_item` an item's tool tip, `forge_mode_card` a forge mode card,
`compare_items` the comparison dialog. Pictures are saved in your temporary folder and the agent gets them too.

Pictures show what TQVault shows: character names, vault names and, in the settings, your folders. Check them before
you publish them.

## Good to know

- Changes reach the files when TQVault saves them: the Save button (`save_files`), or when TQVault closes.
- The agent drives TQVault on your screen: you see the items move. You can still use the mouse, but don't drag items
  while the agent does.
- Message boxes TQVault shows during an agent's action wait for you to answer them, unless you let the agent answer
  them (`queue_dialog_answers`).
- To stop the agent at once, untick the setting, or close TQVault.
