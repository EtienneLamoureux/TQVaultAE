# Modded characters and their mod

_User guide with screenshots: [MODS.md](MODS.md). This page holds the technical details._

Titan Quest keeps custom map (mod) characters in `SaveData\User\_<name>`, apart from the main campaign ones in
`SaveData\Main`, but their save files never name the mod they were played with. TQVault lists both kinds together and
loads each with its own mod's data, in one session. A character's link to its mod is the only way to choose a mod: no
mod setting, no `/mod:` command-line argument.

## The link file

Character menu (tools button of the character list) > *Mod link...*, for custom map characters. The dialog lists the
installed mods (legacy custom maps in `My Games\Titan Quest - Immortal Throne\CustomMaps`, Steam Workshop items) and
links the character to one, or removes its link. The choice is kept in the character's save folder:

`SaveData\User\_<name>\TQVaultAE.ModLink.json`

```json
{
  "version": 1,
  "mod": "shadowchampions",
  "source": "workshop:1949340853",
  "linkedUtc": "2026-11-09T00:12:53.1422315Z"
}
```

- `mod`: the mod's folder name. The game names the mod's database (`database\<mod>.arz`) and its shared stash folder
  (`SaveData\Sys\<mod>`) after it, so the mod is found again after a move (another Steam library, another computer).
- `source`: `workshop:<item id>` (Steam Workshop) or `custommaps`. It tells apart a Workshop mod and a custom map of the
  same folder name ([The identity of a mod](#the-identity-of-a-mod)).
- No path: name and source find the mod wherever it is installed, so the file moves to another computer and holds none
  of the user's folders.
- The game never writes a file of this name; it is copied with the character (*Duplicate*, archive).

In the character list, a custom map character shows its mod's name in brackets beside its name, and on the right, with
its other badges, a *Modded* badge and the link state: *Linked* (green), *Not installed* (red: the linked mod is gone)
or *Not linked* (yellow).

A character linked to an installed mod is described with that mod's data, over the base game's, whatever character is
selected: its class name (a mod can rename classes) and its masteries (a mod can bring its own), in the character list,
the selected character's box, its statistics panel, its editor and its skill tool tips. Without a link, or linked to a
mod not installed: base game data.

## Suggest

- Reads the record ids used by `Player.chr` and the character's `winsys.dxb`, keeps those the base game database lacks,
  and selects the installed mod whose database has the most of them.
- Several mods tie: selects none and names them. No installed mod has them: says so.
- Never links: the user clicks *Link*.
- A character using only base game records (a new one, or one without mod items) cannot be told apart: the dialog says
  so.

## Which data a character is loaded with

| Character | Data |
|---|---|
| Main campaign (`SaveData\Main`) | the base game |
| Custom map, linked to an installed mod | that mod |
| Custom map, not linked, or linked to a mod not installed | the base game |

- Selecting a character makes its map active: its database and archives, and its shared stash files
  (`SaveData\Sys\<mod>\winsys.dxb` and `miscsys.dxb`, or `SaveData\Sys\` for the base game).
- Each map's database is read when first needed (about half a second for a large mod) and kept for the session:
  switching back is immediate.
- After the startup loading, the databases of the mods linked to listed characters are read in the background (*Load
  all files* has read them already): selecting a modded character does not wait.
- A *Modded vault* choice of a mod not read yet reads it in the background too, under the wait cursor, then applies the
  choice: no freeze.
- A database being read only makes that map's users wait; the maps already read stay usable.
- A character read with another map's database (before it was linked) is read again, unless it has unsaved changes.
- *Load all files* at startup reads every character with its own map ([Search and preload](#search-and-preload)).

## The map of an item

Every item is read with the **map of its container**: a mod (mod data first, then the base game's) or the base game.
Picture, size, names, tooltip and statistics come from that map only, never from the selected character or the moment
its data is read. The search, a tooltip and a vault show an item the same way whatever character is selected.

- An item has no map of its own, and none is saved: the container gives it each time the item is read or changes
  container (`Item.ContainerMod`).
- A container linked to a mod not installed reads its items with the base game data.
- The *active map* (the selected character's) remains only for that character's things: its shared stash files, the
  window's pictures and texts, the caps and formulas of its statistics.

### The map a container gives

| Container | Map its items are read with |
|---|---|
| Vault linked to a mod | that mod |
| Vault without a link | the base game |
| Main campaign character (`SaveData\Main`): equipment, bags, its stash | the base game |
| Custom map character linked to a mod (installed or not) | that mod |
| Custom map character without a link | the base game |
| Transfer stash and relic vault | the map whose files they are (`SaveData\Sys\<mod>`, or `SaveData\Sys\` for the base game) |

Each character and each shared stash is read with its own map's database, whatever map is active.

### The mod of an item

An item's mod (`Item.Mod`, shown in its tooltip, used by the *Modded* and *Mod* filters) is **computed** from the
records it was read with:

- It is a mod's when at least one record (base item, prefix, suffix, relic or charm 1 and 2, their completion bonuses)
  came from that mod's database: a record of the mod's own, or one it overrides (same id as the base game, found first
  in the mod's database).
- Records all from the game: no mod, even in a linked vault.
- Each record keeps the database it came from (`DBRecordCollection.Source`, carried by `Info.Source`): base game,
  Immortal Throne, or a mod by its identity. Never the record id, the same key in every map: each map's database
  (`Database`, one per map) caches its own records.

So an item's mod follows its container: moved to a vault without a link, a modded item is read with the base game data,
loses its mod and shows incomplete (its mod records are missing); moved back to a vault linked to its mod, it is whole
again. **Linked vaults keep mod items resolved.**

### What reads an item again

1. **A container is read** (startup, selecting a character, reading a vault): its items, with its map.
2. **An item changes container** (dropped, equipped, *Move Item To*, imported, pasted, results window *Move To*): read
   with the new container's map when it differs. On a character, or in a stash shown with it: the shown character's
   map.
3. **A container's link changes** (*Modded vault*; a character linked or unlinked with *Mod link...* is read again when
   selected, unless it has unsaved changes): all its items, with the new map.
4. **A copy is made** (split stack, duplicate, tab copy, relic or charm taken out, crafted artifact, new set member,
   forge result): read with its source's map until placed, then event 2 applies.

Nothing is guessed from the records: an item gets a mod only from the database it was read with.

### When the item's size changes

Read with another map, an item may take another size: an incomplete one-cell tile becomes a 1x5 lance.

| Event | Where it goes | No room at all |
|---|---|---|
| Drop by hand into a container of another map | the drop spot, else the tab's first free spot | refused like any drop without room: stays in hand, read with its previous map; a message says the tab has no room; the user picks another tab |
| *Move Item To*, import, paste, results window *Move To* | where it lies, else the first free spot of its tab, then of the vault's other tabs in order | shown 1 x 1 where it lies (below) |
| A container's link changes | same as above | shown 1 x 1 where it lies (below) |
| A vault read whose link changed since it was saved | where it lies | shown 1 x 1 where it lies (below) |

- A drop by hand never moves the item to another tab: the user chooses the tab.
- A drop is checked before anything changes (`IItemEditService.HasRoomOnceSettled`, `SackLayout.HasRoomAt`): the item
  is read with the map it would take, then with its own again.
- The free spot search goes row by row (`SackLayout.FitsOrMoves`).
- Every item read with a new map gets its tooltip, names and search entry made again.

### Items shown 1 x 1 for want of room

An item whose real size has no room where it lies is shown **1 x 1** (`Item.LacksRoom`):

- Its width and height give one cell (`RealWidth` and `RealHeight` keep the real size): layout, drawing, search and room
  checks see it as it lies, over no neighbour.
- Drawn with the incomplete item's warning tile; its tooltip blames the lack of room for its size, not missing records.
- Its records are read as usual: names, statistics and mod are whole.
- An export writes its real size.

Never saved. It clears when:

- the item is dropped or moved where its real size fits (a drop into the same tab checks the real size at the drop
  spot, then at a free spot; without room it stays 1 x 1);
- its container's link changes again and gives it room;
- its vault is read again and its saved place has room for its size.

### The identity of a mod

A mod is its folder name plus where it is installed from: a Steam Workshop mod and a custom map of the same folder name
are two mods, told apart by the dialog, the *Modded vault* menu, the filters and the files.

- Files keep the name in `"mod"` and the source beside it in `"modSource"` (`"source"` in a link file):
  `workshop:<item id>` or `custommaps`.
- A mod's database names its records after the installed mod in its folder (`IModLinkService.ModIdOfMap`), else after
  the folder.
- Both are needed: a link file without its source is not read (character not linked); a vault naming a mod by name
  alone names no installed mod (base game data, like a mod not installed).
- The match is exact: the same mod installed the other way here (Workshop instead of custom maps, or the reverse) is
  another mod; a character linked to it shows its mod as not installed until linked again.

### In the files

The vault's mod is `"mod"` at the top of the vault file. Items write no mod: an item's own `"mod"`, `"modSource"` or
`"noMod"`, written by older TQVault versions of this branch, are ignored when read and dropped at the next save.

```json
{
  "mod": "shadowchampions",
  "modSource": "workshop:1949340853",
  "sacks": [ { "items": [ { "baseName": "records\\customitems\\mi\\e_centurionlance.dbr" } ] } ]
}
```

### Exports and imports

A **vault** or **tab** export writes its container's mod once, at its top, as the vault files do. Items write no mod;
an **item** export (one item or a selection) none at all: an imported item takes the map of the container it is put in.

On import, the vault's or tab's mod is checked against the installed mods:

- **Installed**: kept. A vault import makes it the vault's link (an export without one keeps the target's link). A
  tab's items are read with it until placed, then take their target's map.
- **Not installed** (another computer, a mod removed): dropped. The items take their target vault's map; an item needing
  the mod's records shows incomplete. The end message names the dropped mods: install the mod, then link the vault.

### Hand tools

- **A whole vault**: vault tools button (next to the vault list) > *Modded vault*, for the vault shown.
  - When the submenu opens, each installed mod having records of the vault's items that the base game lacks is marked
    *(suggested)* on the right (*(suggested 1/3)* with only some of them), like the dialog's Suggest. Where each record
    was found is kept for the session.
  - A linked vault shows its mod after its name in the vault lists: *Main Vault [shadowchampions]*.
- Items have no *Modded* menu: their mod comes from their container.
- The tooltip names an item's mod last in the seed and expansion section: *Mod : shadowchampions*.

### Search and preload

- *Load all files* reads every listed character with its own map, and the transfer stash and relic vault of the base
  game and of every mod a listed Immortal Throne character is linked to. Each map's database is read when first needed
  (about half a second for a large mod).
- The search covers them all from startup and names a mod's shared stashes after it: *Transfer Stash
  [shadowchampions]*.
- The stash panel shows the shared stashes of the shown character's map only. A result in another map's selects the
  first listed character played with that map (as a result in a character's bags selects that character). With no such
  character left (its link changed), the notification line says so and nothing is selected.
- With *Allow hot reload features* on, the transfer stash and relic vault files of every map in use (base game, listed
  characters' maps, active one) are watched. When the game writes one, it is read again with its own map: the search
  finds its new items, and the stash panel shows them when it is the active map's.
- Debug builds log a warning for an item indexed without a map (`Item without a map`): a path that forgot to give one.

## Incomplete items

An item naming records its map's data lacks (base item, affix, relic or charm, relic bonus: a mod's records while its
map is the base game, or a mod not installed) is shown incomplete:

- its tooltip has a warning line with an icon under its name, then the missing records, one per line;
- a missing name part (base item, prefix, suffix) shows the record's file name;
- without its base item record it has no picture: the grid shows a warning tile in one cell, so it stays visible and
  movable.

An item shown 1 x 1 for want of room ([Items shown 1 x 1 for want of room](#items-shown-1-x-1-for-want-of-room)) has
the same warning tile, but its tooltip warns of the lack of room and lists no missing record.
