# Modded characters and their mod

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
- **A whole vault:** the vault tools button next to the vault list has *Modded vault*, for the vault shown. Its items without a mod of their own (or of the
  vault's previous mod) follow it and resolve with that mod's data first, then the base game's; items linked to another
  mod keep theirs.

In the vault file, the vault's mod is `"mod"` at the top, and an item has its own `"mod"` only when it differs from the
vault's:

```json
{
  "mod": "shadowchampions",
  "sacks": [ { "items": [ { "baseName": "records\\customitems\\mi\\e_centurionlance.dbr" },
                          { "baseName": "...", "mod": "Lilith" } ] } ]
}
```

An item linked to a mod that is not installed resolves with the active map's data, and shows incomplete when that data
lacks its records.

## Incomplete items

An item that names records the loaded game data lacks (its base item, an affix, a relic or charm, or a relic bonus of a
mod whose data is not loaded: the character is not linked, its mod is not installed, or the item sits in a vault shown
with another map's data) is shown incomplete:

- its tooltip has a warning line with an icon under its name, then the missing records, one per line;
- a missing name part (base item, prefix, suffix) shows the record's file name;
- without its base item record it has no picture: the grid shows a warning tile in one cell, so it stays visible and can be moved.
