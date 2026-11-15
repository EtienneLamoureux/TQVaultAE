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
  "source": "workshop:1949340853",
  "linkedUtc": "2026-11-09T00:12:53.1422315Z"
}
```

- `mod` is the mod's folder name: the game names the mod's database (`database\<mod>.arz`) and its shared stash folder
  (`SaveData\Sys\<mod>`) after it, so it finds the mod again when it moved (another Steam library, another computer).
- `source` is where it is installed from: `workshop:<item id>` for a Steam Workshop mod, `custommaps` for a custom map.
  A Workshop mod and a custom map can have the same folder name: the source tells them apart (see *The identity of a
  mod* below).
- No path is kept: the name and the source find the mod wherever it is installed (a Workshop item in any Steam library,
  a custom map in `CustomMaps`), so the file can move to another computer, and it carries no folder of the user's.
- The game never writes a file with this name; it is copied with the character (*Duplicate*, archive).

In the character list, a custom map character shows its mod's name in brackets beside its name, and on the
right, with its other status badges, a *Modded* badge and the link state: *Linked* (green), *Not installed* (red, the linked mod is gone) or *Not
linked* (yellow).

A character linked to an installed mod is described with that mod's data, which takes precedence over the base game's,
whatever character is selected: its class name (a mod can rename the classes) and its masteries (a mod can bring its own)
in the character list and in the box of the selected character, as in its statistics panel, its editor and its skill
tool tips. A character without a link, or linked to a mod that is not installed, keeps the base game data.

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
immediate. Once the startup loading ends, the databases of the mods the listed characters are linked to are read in
the background (*Load all files* has read them already), so selecting a modded character does not wait for them. A
*Modded vault* choice of a mod whose database is not read yet reads it in the background too, under the wait
cursor, then applies the choice: the window does not freeze. A database being read only makes the users of that map wait; the maps already read stay usable. A character read with another map's database (before it was linked) is read again, unless it has unsaved
changes.

*Load all files* at startup reads every character with its own map (see *Search and preload* below).

## The map of an item

Every item TQVault holds is read with the **map of its container**: the data its records resolve in, a mod (mod data
first, then the base game's) or the base game. Its picture, size, names, tooltip and statistics all come from that map,
and only from it: never from the selected character or from the moment its data is read. The search, a tooltip and a
vault therefore show an item the same way whatever character is selected.

An item has no map of its own, and none is saved: the map is the container's, given again each time the item is read
or changes container (`Item.ContainerMod`). A container linked to a mod that is not installed reads its items with the
base game data.

The *active map* (the selected character's) remains only for what belongs to that character: its shared stash files,
the pictures and texts of the window, the caps and formulas of its statistics.

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

An item's mod (`Item.Mod`, shown in its tooltip and used by the *Modded* and *Mod* filters) is **computed** from the
records it was read with: it is a mod when at least one of its records (base item, prefix, suffix, relic or charm 1 and
2, their completion bonuses) was read from that mod's database, a record of the mod's own or one the mod overrides (the
same record id as the base game, found first in the mod's database). An item whose records all come from the game is no
mod's, even in a linked vault.

Each record read keeps the database it came from (`DBRecordCollection.Source`, carried by `Info.Source`): the base game,
Immortal Throne, or a mod, by its identity. Never the record id, which is the same key in every map: each map's
database (`Database`, one per map) caches its own records.

So an item's mod follows its container: a modded item moved to a vault without a link is read with the base game data,
loses its mod and is shown incomplete (its mod records are missing); moved back to a vault linked to its mod, it is
whole again. **Linked vaults are the way to keep mod items resolved.**

### What reads an item again

1. **A container is read** (startup, selecting a character, reading a vault): its items are read with its map.
2. **An item changes container** (dropped, equipped, *Move Item To*, imported, pasted, results window *Move To*): it is
   read with the new container's map when it differs. On a character, or in a stash shown with it, that is the shown
   character's map.
3. **A container's link changes** (*Modded vault*; a character linked or unlinked with *Mod link...* is read again when
   selected, unless it has unsaved changes): every item of the container is read again with the new map.
4. **A copy is made** (split stack, duplicate, copy of a tab, relic or charm taken out, crafted artifact, new set member,
   forge result): it is read with the map of its source until placed, then event 2 applies.

Nothing is guessed from the records: an item gets a mod only from the database it was read with.

### When the item's size changes

Read with another map, an item may take another size: an incomplete one-cell tile becomes a 1x5 lance.

| Event | Where it goes | No room at all |
|---|---|---|
| Drop by hand into a container of another map | where it was dropped, else the first free spot of the tab | the drop is refused, like any drop without room: the item stays in hand, read with its previous map, and a message invites the user to drop it in another tab |
| *Move Item To*, import, paste, results window *Move To* | where it lies, else the first free spot of its tab, else of the vault's other tabs, in order | it is shown 1 x 1 where it lies (below) |
| A container's link changes | where it lies, else the first free spot of its tab, else of the vault's other tabs, in order | it is shown 1 x 1 where it lies (below) |
| A vault read whose link changed since it was saved | where it lies | it is shown 1 x 1 where it lies (below) |

A drop by hand never moves the item to another tab: the user chooses the tab. A drop is checked before anything changes
(`IItemEditService.HasRoomOnceSettled`, `SackLayout.HasRoomAt`): the item is read with the map it would take, then with
its own again. The free spot search goes row by row (`SackLayout.FitsOrMoves`). Every item read with a new map has its
tooltip, its names and its search entry made again.

### Items shown 1 x 1 for want of room

An item whose real size has no room where it lies is shown **1 x 1** (`Item.LacksRoom`): its width and height give one
cell (`RealWidth` and `RealHeight` keep the real size), so the layout, the drawing, the search and the room checks all
see it as it lies, over no neighbour. It is drawn with the warning tile of an incomplete item, and its tooltip says that
the problem is a lack of room for its size, not missing records. Its records are read as usual: its names, statistics
and mod are whole.

The state is never saved, and it clears:

- when the item is dropped or moved where its real size fits (a drop into the same tab checks the real size at the drop
  spot, then at a free spot; without room it stays 1 x 1);
- when its container's link changes again and gives it room;
- when its vault is read again and its saved place has room for its size.

An export writes the item's real size.

### The identity of a mod

A mod is its folder name and where it is installed from: a Steam Workshop mod and a custom map with the same folder name
are two mods, which the dialog, the *Modded vault* menu, the filters and the files tell apart. Files keep the name in
`"mod"` and the source beside it in `"modSource"` (`"source"` in a link file): `workshop:<item id>` or `custommaps`. A
mod's database names its records after the installed mod in its folder (`IModLinkService.ModIdOfMap`), else after the
folder.

Both are needed: a link file without its source is not read (the character is not linked), and a vault that names a mod
by its name alone names no installed mod (read with the base game data, like a mod that is not installed). The match is
exact: the same mod installed the other way here (from the Workshop instead of the custom maps, or the reverse) is
another mod, and a character linked to it shows its mod as not installed until it is linked again.

### In the files

In the vault file, the vault's mod is `"mod"` at the top. Items write no mod: older TQVault versions of this branch wrote
an item's own `"mod"`, `"modSource"` or `"noMod"`; they are ignored when read and dropped at the next save.

```json
{
  "mod": "shadowchampions",
  "modSource": "workshop:1949340853",
  "sacks": [ { "items": [ { "baseName": "records\\customitems\\mi\\e_centurionlance.dbr" } ] } ]
}
```

### Exports and imports

A **vault** or **tab** export writes the mod of its container once, at its top, as in the vault files. Items write no
mod, and an **item** export (one item or a selection) none at all: an imported item takes the map of the container it is
put in.

On import, the vault's or tab's mod is checked against the installed mods:

- **Installed**: kept. A vault import gets it as the vault's link (an export without one keeps the target's link). The
  items of a tab are read with it until placed, then take their target's map.
- **Not installed** (another computer, a mod removed): dropped. The items take their target vault's map, and an item
  that needs the mod's records is shown incomplete. The end message names the dropped mods: the user installs the mod,
  then links the vault.

### Hand tools

- **A whole vault**: the vault tools button next to the vault list has *Modded vault*, for the vault shown. When the
  submenu opens, each installed mod having records of the vault's items that the base game lacks is marked
  *(suggested)* on the right (*(suggested 1/3)* when it has only some of them), like the dialog's Suggest button. Where
  each record was found is kept for the session. A linked vault shows its mod after its name in the vault lists:
  *Main Vault [shadowchampions]*.
- Items have no *Modded* menu: their mod comes from their container.
- The tooltip names an item's mod last in the section of the seed and the expansion: *Mod : shadowchampions*.

### Search and preload

*Load all files* reads every listed character with its own map, and the transfer stash and relic vault of the base game
and of every mod a listed Immortal Throne character is linked to. Each map's database is read the first time it is
needed (about half a second for a large mod). The search covers them all from startup, and names a mod's shared
stashes after it: *Transfer Stash [shadowchampions]*. The stash panel shows the shared stashes of the shown
character's map only: a result in another map's selects the first listed character played with that map, as a result
in a character's bags selects that character. When no listed character is played with it any more (its link changed),
the notification line says so and nothing is selected.

With *Allow hot reload features* on, the transfer stash and relic vault files of every map in use (the base game, the
maps of the listed characters and the active one) are watched. When the game writes one, it is read again with its own
map: the search finds its new items, and the stash panel shows them when it is the active map's.

Debug builds log a warning for an item indexed without a map (`Item without a map`): a path that forgot to give one.

## Incomplete items

An item that names records its map's data lacks (its base item, an affix, a relic or charm, or a relic bonus: a mod's
records while its map is the base game, or a mod that is not installed) is shown incomplete:

- its tooltip has a warning line with an icon under its name, then the missing records, one per line;
- a missing name part (base item, prefix, suffix) shows the record's file name;
- without its base item record it has no picture: the grid shows a warning tile in one cell, so it stays visible and can be moved.

An item shown 1 x 1 for want of room (see *Items shown 1 x 1 for want of room*) has the same warning tile, but its
tooltip warns of the lack of room instead, and lists no missing record.
