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
  "folder": "e:\\games\\steam\\steamapps\\workshop\\content\\475150\\1949340853\\shadowchampions",
  "linkedUtc": "2026-11-09T00:12:53.1422315Z"
}
```

- `mod` is the mod's folder name: the game names the mod's database (`database\<mod>.arz`) and its shared stash folder
  (`SaveData\Sys\<mod>`) after it, so it finds the mod again when it moved (another Steam library, another computer).
- `source` is where it is installed from: `workshop:<item id>` for a Steam Workshop mod, `custommaps` for a custom map.
  A Workshop mod and a custom map can have the same folder name: the source tells them apart (see *The identity of a
  mod* below).
- `folder` is where the mod was when linked, preferred while it exists.
- The game never writes a file with this name; it is copied with the character (*Duplicate*, archive).

In the character list, a custom map character shows its mod's name in brackets beside its name, and on its tags line,
on the right, a *Modded* tag and the link state: *Linked* (green), *Not installed* (red, the linked mod is gone) or *Not
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
*Modded* choice (item or vault) of a mod whose database is not read yet reads it in the background too, under the wait
cursor, then applies the choice: the window does not freeze. A database being read only makes the users of that map wait; the maps already read stay usable. A character read with another map's database (before it was linked) is read again, unless it has unsaved
changes.

*Load all files* at startup reads every character with its own map (see *Search and preload* below).

## The map of an item

Every item TQVault holds has a **map**: the data its records resolve in, a mod (its folder name, as in a link file:
mod data first, then the base game's) or the base game. Its picture, size, names, tooltip and statistics all come from
that map, and only from it: never from the selected character or from the moment its data is read. The search, a
tooltip and a vault therefore show an item the same way whatever character is selected.

The map has an **origin**:

- **Own**: chosen for the item, read as its own from a vault file or an export, or made explicit when it left a
  character (see below). It goes with the item wherever it goes.
- **Inherited**: the map of the container holding it. It changes with the container's link, and with the container
  when the item moves.

A map naming a mod that is not installed is kept as it is: the item is read with the base game data and shown
incomplete until the mod is installed or the map changed.

The *active map* (the selected character's) remains only for what belongs to that character: its shared stash files,
the pictures and texts of the window, the caps and formulas of its statistics.

### The map a container gives

| Container | Inherited map |
|---|---|
| Vault linked to a mod | that mod |
| Vault without a link | the base game |
| Main campaign character (`SaveData\Main`): equipment, bags, its stash | the base game |
| Custom map character linked to a mod (installed or not) | that mod |
| Custom map character without a link | the base game |
| Transfer stash and relic vault | the map whose files they are (`SaveData\Sys\<mod>`, or `SaveData\Sys\` for the base game) |

Each character and each shared stash is read with its own map's database, whatever map is active, and its items
inherit that map before anything else reads them.

### What sets or changes an item's map

The map changes only on these events.

1. **A container is read** (startup, selecting a character, reading a vault). An item with an own link in a vault file
   gets it as its own; every other item inherits the container's map. On a first start nothing is linked yet: every
   item inherits the base game, and linking a container afterwards gives its new map to them all (event 3).
2. **An item changes container**: dropped, equipped, *Move Item To*, imported, pasted.
   - An own map is kept, also on a character or in a game stash: keeping the containers consistent with the game is the
     user's responsibility. The game tolerates records it cannot resolve.
   - **Leaving a character for a vault makes the map explicit**: an inherited map becomes the item's own, with the same
     value. This covers the equipment, the bags, the character's stash, the transfer stash and the relic vault. The item
     keeps the data the game reads it with: a mod's own item, and a base game item that the mod changes, alike. A vault
     without a link may then hold items linked to a mod: the tooltip shows it, and *Modded > None (base game)* gives them
     the base game.
   - In any other move an inherited map becomes the new container's. On a character (or in a stash shown with it) that is
     the shown character's map.
3. **A vault's link changes** (*Modded vault*): its items with an inherited map take the new one; own maps are kept. A
   character linked or unlinked (*Mod link...*) is read again when selected, unless it has unsaved changes.
4. **The user chooses for a vault item** in its *Modded* menu: a mod, or *None (base game)*. The choice becomes the
   item's own map. Character items have no such menu: their files cannot keep the choice.
5. **A copy is made** (split stack, duplicate, copy of a tab, relic or charm taken out, crafted artifact, new set member,
   forge result): it has the map and origin of its source, then event 2 applies where it lands.

There is no guess from the records: an item a vault's mod completes still only inherits it (relinking the vault to
another mod makes it incomplete again, and the user gives it a link with *Modded*, where the mods that have its records
are *(suggested)*), and items of a linked character are not linked one by one to its mod.

**On a character, an own map lasts for the session.** The game's character and stash files have no place for a link,
and TQVault keeps none beside them: when they are read again, their items inherit the character's map. An item moved
back to a vault during the session keeps its own map, which the vault file saves.

### When the item's size changes

Read with another map, an item may take another size: an incomplete one-cell tile becomes a 1x5 lance. The item is
always shown with its map's data, so it is placed again when it no longer fits where it lies:

| Event | Where it goes | No room at all |
|---|---|---|
| Drop by hand | where it was dropped, else the first free spot of the tab | the drop is refused: the item stays in hand, and a message invites the user to drop it in another tab |
| *Move Item To*, import, paste, a vault's link changed | where it lies, else the first free spot of its tab, else of the vault's other tabs, in order | it keeps its previous map, as its own, and a message names it |
| *Modded* choice | where it lies, else the first free spot of its tab | the choice is refused, and a message invites the user to move the item to another tab first |

A drop by hand and a *Modded* choice never move the item to another tab: the user chooses the tab. A drop is checked
before anything changes (`IItemEditService.HasRoomOnceSettled`, `SackLayout.HasRoomAt`): the item is read with the map
it would take, then with its own again. The free spot search goes row by row (`SackLayout.FitsOrMoves`). Every item
read with a new map has its tooltip, its names and its search entry made again.

### The identity of a mod

A mod is its folder name and where it is installed from: a Steam Workshop mod and a custom map with the same folder name
are two mods, which the dialog, the *Modded* menus and the files tell apart. Files keep the name in `"mod"` and the
source beside it in `"modSource"` (`"source"` in a link file): `workshop:<item id>` or `custommaps`.

Both are needed: a link file without its source is not read (the character is not linked), and a vault or an item that
names a mod by its name alone names no installed mod (read with the base game data, like a mod that is not installed).
A source not installed here still finds the mod when one installed mod only has that name (the same mod installed the
other way); with homonyms, nothing is guessed.

### In the files

In the vault file, the vault's mod is `"mod"` at the top. An item writes only its own map: `"mod"` for a mod,
`"noMod": true` for the base game (older TQVault versions ignore it, and the item follows the vault's link there). An item
without them inherits the vault's mod.

```json
{
  "mod": "shadowchampions",
  "modSource": "workshop:1949340853",
  "sacks": [ { "items": [ { "baseName": "records\\customitems\\mi\\e_centurionlance.dbr" },
                          { "baseName": "...", "mod": "Lilith", "modSource": "workshop:890319896" },
                          { "baseName": "...", "noMod": true } ] } ]
}
```

### Exports and imports

An export keeps the map at the level it belongs to:

- A **vault** or **tab** export writes the mod of its container once, at its top, and each item's own map only. A mod
  is written as in the vault files: `"mod"` and `"modSource"`.
- An **item** export (one item or a selection) has no container: each item writes the map it is read with, made
  explicit (`"mod"`, or `"noMod": true` for the base game), and the import gives it as the item's own.

On import, each mod named is checked against the installed mods:

- **Installed**: kept. A vault import gets it as the vault's link (an export without one keeps the target's link). The
  items of a tab export that inherited the tab's mod keep it: inherited in a vault of that mod, else as their own.
- **Not installed** (another computer, a mod removed): dropped, at the vault or tab level as on the items. The items
  take their target vault's map, and an item that needs the mod's records is shown incomplete. The end message names
  the dropped mods: the user installs the mod, then links the vault or the items.

### Hand tools

- **The item menu** (vault items only) has *Modded*: the base game and the installed mods, the current map checked. A
  link to a mod that is not installed stays listed, disabled. When the submenu opens, each installed mod having records
  of the item that the base game lacks is marked *(suggested)* on the right (*(suggested 1/3)* when it has only some of
  them), like the dialog's Suggest button. Where each record was found is kept for the session.
- **A whole vault**: the vault tools button next to the vault list has *Modded vault*, for the vault shown, with the same
  hints for the records of all its items. A linked vault shows its mod after its name in the vault lists:
  *Main Vault [shadowchampions]*.
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
