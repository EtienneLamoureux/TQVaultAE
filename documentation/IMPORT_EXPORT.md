# Import / Export & PasteBin Sharing

Share items, vault tabs, and entire vaults between players — via clipboard, `.json` files, or PasteBin URL links.

---

## Table of contents

* [Export Scopes](#export-scopes)
* [Export Channels](#export-channels)
    + [Clipboard Export (Ctrl+C)](#clipboard)
    + [File Export (.json)](#file-export)
    + [PasteBin Export](#pastebin-export)
* [Import Channels](#import-channels)
    + [Clipboard Import (Ctrl+V)](#clipboard-import)
    + [File Import (.json)](#file-import)
    + [PasteBin Import](#pastebin-import)
* [Full Vault Import Strategy](#vault-import)
* [Non-Vault Import (Items / Tabs / Multi-Select)](#non-vault-import)
* [Item Placement Logic](#placement)
* [Export Format](#export-format)
* [Settings](#settings)
    + [Obtaining a PasteBin API Key](#api-key)

---

### <a id="export-scopes"></a>Export Scopes

You can export items at four levels:

| Scope | How to trigger | Behavior |
|-------|---------------|----------|
| **Single item** | Right-click an item → `🔗 Share Item` | The item under the cursor with all its properties (seed, affixes, relic sockets, stack size, position, size, etc.), as an `Item`-scoped JSON payload. |
| **Multi-select** | **Ctrl+LeftClick** to select items → Right-click → `🔗 Share Item` | All selected items as one `MultiSelect`-scoped payload. A selection of **one** item exports as `Item`. |
| **Vault tab** | Right-click a **tab button** (BagButton) → `🔗 Share Tab` | All items of the tab with their positions, and the bag button's label and icons. |
| **Full vault** | Click the **menu button** next to the vault combo box (a small game button; `...` when the game pictures are not loaded) | All tabs and their contents, with the vault name and its mod link. |

- With a selection, `🔗 Share Item` exports the selection, whichever item you right-click.
- The menu and **Ctrl+C** give the same format: one item is an `Item` export, several a `MultiSelect` export.

---

### <a id="export-channels"></a>Export Channels

Every scope supports three transport channels.

#### <a id="clipboard"></a>Clipboard Export

- **Single / multi-select items:** Right-click an item → `🔗 Share Item` → `Share to Clipboard`, or select items in the grid and press **Ctrl+C**.
  - Ctrl+C copies the selection only (nothing without one).
  - It works in both vaults, the character's main bag and bags, and the stash; not in the equipment.
- **Tab:** Right-click a tab button → `🔗 Share Tab` → `Share to Clipboard`.
- **Vault:** Click the vault menu button → `Share to Clipboard`.

The export data is serialized as JSON and placed on your clipboard. 
You can paste this directly into a text editor, chat, another bag, or another TQVaultAE instance.

#### <a id="file-export"></a>File Export (.json)

- **Single / multi-select items:** Right-click an item → `🔗 Share Item` → `Share to File...`.
- **Tab:** Right-click a tab button → `🔗 Share Tab` → `Share to File...`.
- **Vault:** Click the vault menu button → `Share to File...`.

A Save File dialog opens; the file is written where you choose, as **raw JSON**, to inspect or edit in any text editor before importing. Suggested file names:

| Export | Suggested file name |
|--------|---------------------|
| One item (or a selection of one item) | `item_export.json` |
| Several selected items | `items_export.json` |
| Tab | `tab_export.json` |
| Vault | `<vault name>.json` |

#### <a id="pastebin-export"></a>PasteBin Export

- **Single / multi-select items:** Right-click an item → `🔗 Share Item` → `Share to PasteBin`.
- **Tab:** Right-click a tab button → `🔗 Share Tab` → `Share to PasteBin`.
- **Vault:** Click the vault menu button → `Share to PasteBin`.

The data is uploaded to PasteBin as an **Unlisted** paste (accessible via direct URL but not published on the public feed). The resulting PasteBin URL is automatically copied to your clipboard.

Paste names:

| Export | Paste name |
|--------|------------|
| One item (or a selection of one item) | The item's full name (untitled if it has none) |
| Several selected items | `Multi-item export` |
| Tab | `<vault name>/<tab label>`, or `<vault name>/Tab <n>` when the tab has no label |
| Vault | `<vault name>` |

**Note:** PasteBin export menu items are **disabled (greyed out)** until you configure a PasteBin API key in Settings (see [below](#api-key)); they follow the key as soon as the settings are saved. For a selection of several items, the entry is hidden.

---

### <a id="import-channels"></a>Import Channels

#### <a id="clipboard-import"></a>Clipboard Import (Ctrl+V)

Press **Ctrl+V** anywhere in the main window. TQVaultAE detects what's on your clipboard:

1. If it's a **PasteBin URL** (`https://pastebin.com/...`), the paste is fetched from PasteBin.
2. Otherwise the clipboard content is parsed directly as JSON.

On success, items go into the tab shown in the main (left) vault panel ([below](#non-vault-import)). On failure, a status-bar notification shows instead of a blocking dialog.

#### <a id="file-import"></a>File Import (.json)

Two entry points:

- **Tab button:** Right-click any tab button → `Import from File...`
- **Vault combo box:** Click the button next to the vault selector → `Import from File...`

A file picker opens for `.json` files. The content is parsed as for a clipboard import; items go into the tab shown in the main vault panel (the tab you right-clicked becomes the shown one).

#### <a id="pastebin-import"></a>PasteBin Import

**No API key is required** to import from a PasteBin link. Simply paste a PasteBin URL into your clipboard and press **Ctrl+V**. TQVaultAE fetches the raw paste content, parses the JSON, and imports the items.

---

### <a id="vault-import"></a>Full Vault Import Strategy

When importing a **full vault**, a dialog presents three options:

| Option | Behavior |
|--------|----------|
| **Replace** (Yes) | Clears all tabs in the current vault and imports the source data. If the target vault is **not empty**, a second confirmation warns: *"WARNING: This will erase all items in the current vault. Continue?"* |
| **Create New Vault** (No) | Creates a vault file named after the imported vault, without the characters a file name cannot hold (`Imported` when nothing is left), plus a timestamp suffix against collisions (e.g., `MyVault_20260714_153022`). Saved to disk at once, with the export's mod link, and loaded. |
| **Cancel** | Aborts the import. Nothing is changed. |

Partial tab merge into an existing vault is not supported — it's all-or-nothing.

- The import also restores every bag's icon and the vault's mod link ([Mods](MODS.md)). With **Replace**, an export without a mod link leaves the target vault's own link, which the imported items then follow.
- Each item goes back to its position in its tab (no free cell search, unless, read with its map, it no longer fits there).
- A tab numbered beyond the target vault's tabs is dropped.
- The main window then says *"Imported N items into vault."*, or *"Created new vault "…" with imported data."*

---

### <a id="non-vault-import"></a>Non-Vault Import (Items / Tabs / Multi-Select)

A scope other than `Vault` (`Item`, `Tab`, `MultiSelect`) goes into the **tab shown in the main (left) vault panel**, whichever panel has the focus, with the placement logic below; never into the second vault, the character or the stash.

For **Tab** exports, the original bag button icon (label, icon set, display mode) is restored on the active tab's bag button after import.

---

### <a id="placement"></a>Item Placement Logic

When importing items into a target vault tab, TQVaultAE attempts placement in this order:

An item's size in cells comes from its picture in the game files, not from the file's `width` and `height`: files with a missing or zero size still import correctly.

1. Try the item's **original position** (PositionX, PositionY from the export).
2. If the original cell is occupied or out of bounds, search for the **first available open cell** in the tab's grid, column by column from the top left. Items are placed in the order of the export.
3. If no open cell exists in that tab, the item is **skipped**.

After all items are processed, a notification shows: *"Imported N of M items."*

Then the imported items are read with the vault's map like dropped ones ([Mod link](MODLINK.md)):

- Read with a new map, an item may take another size and move to a free spot of the tab, else of another tab; without
  any, it shows in one cell with a warning until moved where it fits.
- A mod named by the export but not installed here is dropped; a second notification names it.

---

### <a id="export-format"></a>Export Format

All exports use a JSON envelope with the following structure:

```json
{
  "formatVersion": 1,
  "scope": "Item | Tab | Vault | MultiSelect",
  "data": { ... }
}
```

- **`formatVersion`** — Always `1`.
- **`scope`** — Identifies the export type.
- **`data`** — Scope-specific payload:
  - `Item`: Single `ItemDto` object (includes `stackSize`, `seed`, `baseName`, `prefixName`, `suffixName`, `relicName`, `relicBonus`, `var1`, `relicName2`, `relicBonus2`, `var2`, `pointX`, `pointY`, `width`, `height`).
  - `width` and `height`: the item's real size in cells (one cell per 32 pixels of its picture), even when it shows in one cell for want of room. Older versions could write `0` for items whose picture was not drawn yet; imports ignore them.
  - An `ItemDto` has no mod: an item's mod is computed from the data of the container it is put in ([Mod link](MODLINK.md)).
  - `MultiSelect`: array of `ItemDto` objects (two or more: one selected item exports as `Item`).
  - `Tab`: object with `sackNumber`, `items` (array of `ItemDto`), optional `iconInfo` (bag button label, display mode, icon RecordIds), and `mod` and `modSource` when its vault or character is linked to a mod. Its items are read with that mod until placed, then with their target's map. An import drops a mod not installed here.
  - `Vault`: object with the vault `name`, `sacks` (array of tabs with `sackNumber`, `items` and `iconInfo`, no mod of their own), and `mod` and `modSource` when the vault is linked to a mod.

---

### <a id="settings"></a>Settings

PasteBin settings are configured in **Settings** (the **Configure** button of the main window):

| Setting | Description | Default |
|---------|-------------|---------|
| **PasteBin API Key** | Your PasteBin developer API key. Required to export pastes. | _(empty)_ |
| **PasteBin Expiration** | How long before the paste automatically expires. Options: 10 Minutes, 1 Hour, 1 Day, 1 Week, 2 Weeks, 1 Month, 6 Months, 1 Year, Never. | 1 Month |

#### <a id="api-key"></a>Obtaining a PasteBin API Key

1. Go to **[https://pastebin.com/doc_api](https://pastebin.com/doc_api)**.
2. Sign in or create a free PasteBin account.
3. Under the **"Your Unique Developer API Key"** section, copy the key shown.
4. Paste the key into TQVaultAE's **Settings → PasteBin API Key** field.

That's it — PasteBin export menu items will now be enabled.

---

**Version:** 2.0