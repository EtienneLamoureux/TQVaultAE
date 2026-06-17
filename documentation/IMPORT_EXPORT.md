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
| **Single item** | Right-click an item → `Share to` → `Export Item` | Exports the selected item with all its properties (seed, affixes, relic sockets, stack size, position, etc.). |
| **Multi-select** | **Ctrl+LeftClick** to select multiple items → Right-click → `Share to` | Exports all selected items as a single `MultiSelect`-scoped JSON payload. |
| **Vault tab** | Right-click a **tab button** (BagButton) → `Share Tab` | Exports all items within that tab, preserving their positions, grid layout, and bag icon info. |
| **Full vault** | Click the **button** next to the vault combo box → `Share Vault` | Exports all tabs and all their contents, including the vault name. |

---

### <a id="export-channels"></a>Export Channels

Every scope supports three transport channels.

#### <a id="clipboard"></a>Clipboard Export

- **Single / multi-select items:** Select items in the grid and press **Ctrl+C**.
- **Tab:** Right-click a tab button → `Share Tab` → `Copy to Clipboard`.
- **Vault:** Click the vault export button → `Share Vault to Clipboard`.

The export data is serialized as JSON and placed on your clipboard. 
You can paste this directly into a text editor, chat, another bag, or another TQVaultAE instance.

#### <a id="file-export"></a>File Export (.json)

- **Single / multi-select items:** Right-click an item → `Share to` → `Save to File...`.
- **Tab:** Right-click a tab button → `Share Tab` → `Save to File...`.
- **Vault:** Vault export button → `Share Vault to File`.

A Save File dialog opens. The file is saved as **raw JSON**, so you can inspect or edit it in any text editor before importing.

#### <a id="pastebin-export"></a>PasteBin Export

- **Single / multi-select items:** Right-click an item → `Share to` → `Share to PasteBin`.
- **Tab:** Right-click a tab button → `Share Tab` → `Share to PasteBin`.
- **Vault:** Vault export button → `Share Vault to PasteBin`.

The data is uploaded to PasteBin as an **Unlisted** paste (accessible via direct URL but not published on the public feed). The resulting PasteBin URL is automatically copied to your clipboard.

**Note:** PasteBin export menu items are **disabled (greyed out)** until you configure a PasteBin API key in Settings (see [below](#api-key)).

---

### <a id="import-channels"></a>Import Channels

#### <a id="clipboard-import"></a>Clipboard Import (Ctrl+V)

Press **Ctrl+V** from the vault panel. TQVaultAE detects what's on your clipboard:

1. If it's a **PasteBin URL** (`https://pastebin.com/...`), the paste is fetched from PasteBin.
2. Otherwise the clipboard content is parsed directly as JSON.

On success, items are placed into the currently active vault tab. On failure, a status-bar notification is shown instead of a blocking dialog.

#### <a id="file-import"></a>File Import (.json)

Two entry points:

- **Tab button:** Right-click any tab button → `Import from File...`
- **Vault combo box:** Click the button next to the vault selector → `Import from File...`

A file picker opens for `.json` files. The content is parsed identically to clipboard import, and items are placed into the currently active vault tab.

#### <a id="pastebin-import"></a>PasteBin Import

**No API key is required** to import from a PasteBin link. Simply paste a PasteBin URL into your clipboard and press **Ctrl+V**. TQVaultAE fetches the raw paste content, parses the JSON, and imports the items.

---

### <a id="vault-import"></a>Full Vault Import Strategy

When importing a **full vault**, a dialog presents three options:

| Option | Behavior |
|--------|----------|
| **Replace** (Yes) | Clears all tabs in the current vault and imports the source data. If the target vault is **not empty**, a second confirmation warns: *"WARNING: This will erase all items in the current vault. Continue?"* |
| **Create New Vault** (No) | Creates a new vault file with the imported vault's name. A timestamp suffix is appended (e.g., `MyVault_20260714_153022`) to avoid collisions. The new vault is automatically loaded. |
| **Cancel** | Aborts the import. Nothing is changed. |

Partial tab merge into an existing vault is not supported — it's all-or-nothing.

---

### <a id="non-vault-import"></a>Non-Vault Import (Items / Tabs / Multi-Select)

When importing a scope other than `Vault` (i.e. `Item`, `Tab`, or `MultiSelect`), items are placed directly into the **currently active vault tab** using the placement logic below.

For **Tab** exports, the original bag button icon (label, icon set, display mode) is restored on the active tab's bag button after import.

---

### <a id="placement"></a>Item Placement Logic

When importing items into a target vault tab, TQVaultAE attempts placement in this order:

1. Try the item's **original position** (PositionX, PositionY from the export).
2. If the original cell is occupied or out of bounds, search for the **first available open cell** in the tab's grid.
3. If no open cell exists in that tab, the item is **skipped**.

After all items are processed, a notification shows: *"Imported N of M items."*

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
  - `MultiSelect`: Array of `ItemDto` objects.
  - `Tab`: Array of `ItemDto` objects plus `sackNumber` and optional `iconInfo` (bag button label, display mode, icon RecordIds).
  - `Vault`: Array of sack arrays plus vault `name`.

---

### <a id="settings"></a>Settings

PasteBin settings are configured in **Settings** (gear icon in the toolbar):

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