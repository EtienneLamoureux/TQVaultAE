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
database does not have, and selects the installed mod whose database has the most of them. It never links: the user
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

In the vault file, the vault's mod is `"mod"` at the top, and an item has its own `"mod"` only when it has a link of its
own:

```json
{
  "mod": "shadowchampions",
  "sacks": [ { "items": [ { "baseName": "records\\customitems\\mi\\e_centurionlance.dbr" },
                          { "baseName": "...", "mod": "Lilith" } ] } ]
}
```

Exports keep the links: an item or tab export writes each item's mod; a vault export writes the vault's mod and each
item's own link, and importing it (as a new vault, or replacing the vault shown) brings both back.

An item linked to a mod that is not installed resolves with the active map's data, and shows incomplete when that data
lacks its records.

## Incomplete items

An item that names records the loaded game data lacks (its base item, an affix, a relic or charm, or a relic bonus of a
mod whose data is not loaded: the character is not linked, its mod is not installed, or the item sits in a vault shown
with another map's data) is shown incomplete:

- its tooltip has a warning line with an icon under its name, then the missing records, one per line;
- a missing name part (base item, prefix, suffix) shows the record's file name;
- without its base item record it has no picture: the grid shows a warning tile in one cell, so it stays visible and can be moved.
