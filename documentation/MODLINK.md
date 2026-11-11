# Modded characters and their mod

_The user guide, with screenshots, is [MODS.md](MODS.md); this page holds the technical details._

Titan Quest keeps custom map (mod) characters in `SaveData\User\_<name>`, apart from the main campaign ones in
`SaveData\Main`, but nothing in their save files names the mod they were played with. TQVault lists both kinds of
characters together and loads each one with its own mod's data, in the same session. The link of a character to its mod
is the only way to choose a mod: there is no mod setting nor `/mod:` command-line argument any more.

## The link file

The character menu (the tools button of the character list) has a *Mod link...* entry for custom map characters. The
dialog lists the installed mods (legacy custom maps in `My Games\Titan Quest - Immortal Throne\CustomMaps` and Steam
Workshop items) and links the character to one, or removes its link. The choice is kept in the character's save folder:

`SaveData\User\_<name>\TQVaultAE.ModLink.json`

```json
{
  "version": 1,
  "mod": "shadowchampions",
  "folder": "e:\\games\\steam\\steamapps\\workshop\\content\\475150\\1949340853\\shadowchampions",
  "linkedUtc": "2026-11-09T00:12:53.1422315Z"
}
```

- `mod` is the mod's folder name: the game names the mod's database (`database\<mod>.arz`) and its shared stash folder
  (`SaveData\Sys\<mod>`) after it, so it finds the mod again when it moved (another Steam library, another computer).
- `folder` is where the mod was when linked, preferred while it exists.
- The game never writes a file with this name; it is copied with the character (*Duplicate*, archive).

In the character list, a custom map character shows its mod's name in brackets beside its name, and on its tags line,
on the right, a *Modded* tag and the link state: *Linked* (green), *Not installed* (red, the linked mod is gone) or *Not
linked* (yellow).

## Suggest

*Suggest* reads the record ids that `Player.chr` and the character's `winsys.dxb` use, keeps those the base game
database does not have, and selects the installed mod whose database has the most of them. When several mods tie, it
selects none and names them; when no installed mod has the records, it says so. It never links: the user
clicks *Link*. A character that only uses base game records (a new one, or one without mod items) cannot be told
apart: the dialog says so.

## Which data a character is loaded with

| Character | Data |
|---|---|
| Main campaign (`SaveData\Main`) | the base game |
| Custom map, linked to an installed mod | that mod |
| Custom map, not linked, or linked to a mod not installed | the base game |

Selecting a character makes its map the active one: its database and archives, and its shared stash files
(`SaveData\Sys\<mod>\winsys.dxb` and `miscsys.dxb`, or `SaveData\Sys\` for the base game). Each map's database is read
the first time it is needed (about half a second for a large mod) and kept for the session, so switching back is
immediate. A character read with another map's database (before it was linked) is read again, unless it has unsaved
changes.

*Load all files* at startup only reads the characters of the startup map; the others are read when selected.

## Items and vaults linked to a mod

An item can be linked to a mod: its records then resolve in that mod's database (mod data first, then the base game's),
whatever map is active, for its picture, its size, its names and its tooltip. The tooltip names it under the item's name:
*Mod : shadowchampions*. The link follows the item inside TQVault (moves, copies, relics taken out, crafted artifacts) and
is saved in the vault files.

- **From a linked character:** when a character linked to an installed mod is loaded, its items that the base game does
  not know (equipment, bags, its stash, the mod's transfer stash and relic vault) are linked to that mod at once. The
  character files cannot keep the link: it is set again each time, and moving such an item to a vault keeps it.
- **By hand:** the item menu has *Modded*, the base game and the installed mods, the current one checked. A link to a mod
  that is not installed stays listed, disabled.
- **Suggested mods:** when the *Modded* submenu opens, each installed mod having records of the item that the base
  game lacks is marked *(suggested)* on the right (*(suggested 1/3)* when it has only some of them), like the dialog's
  Suggest button. A record found in several mods marks each of them. Where each record was found is kept for the
  session.
- **A whole vault:** the vault tools button next to the vault list has *Modded vault*, for the vault shown, with the
  same hints for the records of all its items. A linked vault shows its mod after its name in the vault lists:
  *Main Vault [shadowchampions]*. Its items without a mod of their own (or of the
  vault's previous mod) follow it and resolve with that mod's data first, then the base game's; items linked to another
  mod keep theirs.

An item either has a link of its own or follows the vault's. Items without a link of their own follow the vault's link
when it changes; items with their own link keep it. An incomplete item gets the vault's mod as its own link when that
mod resolves all its missing records:

- when the vault is linked to the mod (or to another mod), for the items already in it;
- when the item is put in a vault linked to the mod: dropped, moved with *Move Item To*, imported from a file or pasted
  (also when a whole vault export replaces a linked vault: without a mod of its own, the export keeps the vault's link).
  Once resolved, the item takes its real size: an item that was not dropped by hand and no longer fits where it lies
  moves to the first free spot of the bag. Read with the mod's data, the item may take its real size
  (a one-cell incomplete item becomes a 1x5 lance): when it would then overlap another item or leave the grid, it is
  left as it was dropped, incomplete and without a link.

The other items of a linked vault (base game items, items the mod does not complete) follow the vault's link.

Choosing *None (base game)* in an item's *Modded* menu is a choice of its own: the item no longer follows the vault's
link, now, after a reload, or when the vault is linked to another mod; it is read with the base game data alone (a record
only the mod has stays unknown). Choosing a mod for it again ends that choice. On a character the choice changes nothing:
an item without a link follows the character's map there.

In the vault file, the vault's mod is `"mod"` at the top, an item has its own `"mod"` only when it has a link of its
own, and `"noMod": true` when the base game was chosen for it (older TQVault versions ignore it, and the item follows
the vault's link there):

```json
{
  "mod": "shadowchampions",
  "sacks": [ { "items": [ { "baseName": "records\\customitems\\mi\\e_centurionlance.dbr" },
                          { "baseName": "...", "mod": "Lilith" },
                          { "baseName": "...", "noMod": true } ] } ]
}
```

Exports keep the links: an item or tab export writes each item's mod (and `"noMod"`); a vault export writes the vault's
mod and each item's own link or `"noMod"`, and importing it (as a new vault, or replacing the vault shown) brings both back.

An item without any link (its own or its vault's), or linked to a mod that is not installed, resolves by where it is:

| Where | Data used |
|---|---|
| In a vault | the base game, whatever character is selected |
| On a character (equipment, bags, stashes) | the active map: the selected character's |

It shows incomplete when that data lacks its records. So *Modded > None (base game)* on a vault item of a mod gives the
incomplete item back, even while a character of that mod is selected. Moving an item between a vault and a character
reads it again with the data of its new place.

## Incomplete items

An item that names records the loaded game data lacks (its base item, an affix, a relic or charm, or a relic bonus of a
mod whose data is not loaded: the character is not linked, its mod is not installed, or the item sits in a vault shown
with another map's data) is shown incomplete:

- its tooltip has a warning line with an icon under its name, then the missing records, one per line;
- a missing name part (base item, prefix, suffix) shows the record's file name;
- without its base item record it has no picture: the grid shows a warning tile in one cell, so it stays visible and can be moved.

## Planned: an explicit data map for every item

_Status: design agreed on 2026-11-11, not implemented yet. Once done, it replaces the "resolves by where it is" rule
above and the `Mod` / `ModFromVault` / `NoMod` fields._

### Why

Today an item without a link of its own is resolved when its data is read: in a vault the base game, elsewhere the
active map, that is the selected character's. The result depends on when the data is read and on which character is
selected at that moment:

- The search builds an item's names once, the first time they are needed, with the map active at that moment. An
  item of a character linked to a mod, read while another character is selected, gets the other map's data.
- The tooltip or stats of an item from another character (search results) are read with the selected character's map.
- A base game record that a mod changes (a sword whose stats the mod overrides) is read with whatever map is active.
- A new way of making an item that forgets to copy the link fields silently falls back on the active map: the mistake
  does not show.

### The model

While an item is in a container, it always has two values:

- **Map**: the database its records resolve in, a mod (its folder name, as in a link file) or the base game.
- **Origin**:
  - **Own**: a mod chosen for the item, the base game chosen for it, or the mod that alone completes it (see below).
  - **Inherited**: the map of the container holding it.

Reading an item's data uses its map, and only its map. When the map names a mod that is not installed, the item is
read with the base game data and shown incomplete, as today; its map is kept, not rewritten.

The active map is never used to read an item. It remains for what belongs to the selected character:
- the shared stash files;
- the pictures and texts of the window;
- the caps and formulas of the character's statistics;
- the database the character's own files are read with.

The map is **explicit but not frozen**. It is always known and changes only on the events listed below: never because
another character is selected, and never because of when the item's data is read.

### The map a container gives

| Container | Inherited map |
|---|---|
| Vault linked to a mod | that mod |
| Vault without a link | the base game |
| Main campaign character (`SaveData\Main`): equipment, bags, its stash | the base game |
| Custom map character linked to a mod | that mod |
| Custom map character without a link | the base game |
| Transfer stash and relic vault | the map whose files they are (`SaveData\Sys\<mod>`, or `SaveData\Sys\` for the base game) |

A container linked to a mod that is not installed gives that mod: its items are read with the base game and shown
incomplete until the mod is installed or the link changed.

### The events that set or change the map

1. **A container is read** (startup preload, selecting a character, reading a vault).
   - An item with an own link in the file (`"mod"` or `"noMod": true` in a vault) gets it as Own.
   - Any other item gets the container's map as Inherited.
   - An item of a character linked to an installed mod whose records the base game lacks gets that mod as Own, as
     today: moved to a vault, it keeps the mod's data.
   - The map is set before the item's records are read.
2. **An item changes container**: dropped, *Move Item To*, equipped, imported, pasted, moved with its tab.
   - An Own map is kept.
   - An Inherited map becomes the new container's.
   - An incomplete item still gets the vault's mod as Own when that mod completes it, with the rules above (real
     size, no overlap).
3. **A container's link changes**: *Modded vault*, or the character's *Mod link...* dialog. Every item of that
   container with an Inherited map takes the new map; Own maps are kept. A character linked or unlinked is read again,
   as today.
4. **The user chooses for an item** in its *Modded* menu: a mod, or *None (base game)*. The choice becomes the item's
   Own map.
5. **A copy is made**: it takes the map and origin of its source, then event 2 applies in the container it lands in.

On a first start with this feature, nothing is linked yet: every item gets the base game as Inherited. Linking a vault
or a character afterwards gives its new map to all those items (event 3); nothing has to be read again by hand.

### Where items are made

Each path that makes an item states where its map comes from. An item without a map is a mistake: Debug builds log it
and assert, and a test checks that every owned item has one.

| Path | Code | Map |
|---|---|---|
| Vault read | `PlayerCollectionProvider` | the file's own link, else the vault's (event 1) |
| Character, its stash, transfer stash, relic vault read | `PlayerService`, `StashService`, `SackCollectionProvider` | the container's; mod-only items of a linked character, Own (event 1) |
| Import of an export or a paste | `ItemDtoExtensions.ToItem`, `ItemExchangeService` | the export's link, then event 2 in the target vault |
| Split stack, duplicate, copy | `Item.Duplicate`, `Item.Clone` | the source's (event 5) |
| Copy of a whole tab | `SackCollection.Duplicate` (`VaultService.CopyBag`) | the source's, then event 2 |
| Relic or charm taken out of an item | `ItemProvider.RemoveRelic1/2` | the item's (event 5) |
| Crafted artifact | `ItemProvider.CraftArtifact` | the formula's (event 5) |
| New set member | `ItemEditService.NewSetMember` | the set item's (event 5) |
| Forge preview and result | `ForgeService` | the target item's; a material must have the same map, or the base game (a mod falls back on base game data) |
| Granted skill shown in a skill window | `ItemProvider.GetGrantedSkill` with a placeholder item | not an owned item: the selected character's map, stated by the caller |

### Search and preload

- The search reads each item with its own map, so its names and tooltips are the same whenever they are built and
  whatever character is selected.
- *Load all files* could then read every character with its own map, so the search covers characters linked to a mod
  from startup. The cost is reading the database of each mod in use at startup (about half a second for a large mod).
  Today it reads only the characters of the startup map.

### Files

Unchanged. In a vault file, `"mod"` on an item and `"noMod": true` are Own maps; an item without them inherits the
vault's `"mod"`. Character files have no place for a link: their items get their map each time they are read.

### Open questions

1. **Item and tab exports**: write each item's map with its origin, so that an inherited mod comes back inherited and
   follows the target vault? Today they write the mod the item is read with, and the import makes it an own link.
2. **A choice on a character item**: today *None (base game)* on a character item changes nothing. With Own maps it
   would be read with the base game data even on a character linked to a mod. A character file cannot keep that choice,
   so it would be lost at the next read, except for mod-only items, which get their mod again. Keep the menu on
   characters, or limit it to vaults?
3. **Preload**: read every character with its own map at startup, or keep reading only the startup map's characters?
