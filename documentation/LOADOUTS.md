# Loadouts

A loadout is a saved equipment set of a character: the pieces worn, slot by slot, both weapon sets, and the weapon set
in hand. Switch builds in a click.

## The loadout bar

On the **Equipment** tab, under the character statistics (the **Edit** button goes on the line below):

![The loadout bar under the gold](loadouts/loadout_bar.png)

- The box lists the character's loadouts: click it to choose one.
- **[+]** adds an empty loadout, under a name you type (unique per character, case ignored). Fill it in the
  [loadout panel](#composing-a-loadout) or with **Update with the gear worn now**.
- The gold arrow (**Load**) wears the chosen loadout. Grey with nothing to load, for a read-only character, or with the
  setting **Player Equipment ReadOnly** (its tool tip says why).
- Right click: **Load**, **Gear swap**, **Update with the gear worn now** ([below](#update), asks first), **Rename**,
  **Copy**, **Delete** (asks first), **Add**.

## Composing a loadout

Choosing a loadout shows it as a second equipment panel in place of the statistics. **Edit** is hidden and the bar
moves down meanwhile.

![The loadout panel in place of the statistics](loadouts/loadout_panel.png)

- Drag and drop as on the equipment. A dropped piece **moves** into the loadout; to keep the original, drop a copy
  (**Copy Item**, **Duplicate Item**). A piece picked up goes to the hand.
- The equipment rules apply: slot kinds, two-handed weapons, **Enable Item Requirement Restriction**.
- The item menu works, except **Move Item To**.
- Buttons on the stone:
  - Upper right arrow: **Back to the statistics** (the loadout stays chosen).
  - Lower corner arrows: **Previous loadout**, **Next loadout**, in the box's order, wrapping around; grey with a single
    loadout.
  - Under the rings: **Gear swap** (below). Greyed, its tool tip says why.

Changes are written with the other files (**Save**, or on closing), or when a bar action writes the loadouts file
(add, update, rename, copy, delete). **Delete** discards the pieces moved into the loadout, **Update** those it
replaces: take them out first.

The loadouts file is apart from the save: a read-only character, or **Player Equipment ReadOnly**, still allows
composing.

## Comparison marks

While a loadout is chosen, a mark beside each equipment slot compares the worn piece with the loadout's:

![The marks beside the equipment slots](loadouts/loadout_marks.png)

- **Blue**: the same piece (item, affixes, relics or charms, seed). Load keeps it.
- **Red**: different. Rings or weapon sets worn the other way round count as different.
- **Grey**: one side empty. No mark when both are.

Click a blue or red mark for the [item comparison](ITEMCOMPARE.md) (worn piece first). Hover a mark for its state.
The marks follow every change, and are hidden for a read-only character or with no loadout chosen. A held item counts
as out of its slot.

## Load

- A worn piece identical to the loadout's stays on, moved to its slot if needed.
- The other pieces are put on as copies of the saved ones.
- Worn pieces the loadout does not use go to the **vault bag shown**. Without room, nothing changes (status line).
- With **Replace the worn pieces instead of storing them in the vault** (Settings, **Loadouts**), they are deleted
  instead, after a confirmation (unless **Bypass Confirmation Messages**).
- The loadout's weapon set is put in hand. Requirements are not checked, as in the game.

A loadout keeps **copies** for **Load** and **Update**: loading it while its sword lies in a vault gives a second
sword. Use **Gear swap** to move pieces instead.

## Update

**Update with the gear worn now** merges the worn gear into the loadout, slot by slot, weapon sets included:

- A worn piece replaces the loadout's (as a copy).
- A slot empty on the character keeps the loadout's piece.
- A two-handed weapon cannot stay beside another piece: its weapon set is then taken whole from the worn gear.
- The weapon set in hand is recorded.

## Gear swap

**Gear swap** (bar menu, or the button under the rings) exchanges the loadout's pieces and the worn ones, slot by slot:

- Nothing is copied, stored or deleted; no vault needed. Empty slots exchange too. The weapon set in hand follows.
- **Gear swap** again swaps back.
- Refused like **Load** (read-only character, **Player Equipment ReadOnly**); asks first while the game runs.

## Good to know

- The game overwrites the save of the character it plays: TQVault asks before **Load**, **Gear swap** and **Update**
  while the game runs.
- Modded characters: pieces use the character's mod; taken out into a bag of another mod, they take that bag's mod
  ([mod links](MODLINK.md)).
- The loadouts are in the [search](ADVANCEDSEARCH.md) (type *Loadout*); a result opens the loadout panel.
- File: `TQVaultAE.Loadouts.json` next to `Player.chr`. It follows the character when archived, duplicated
  (**Duplicate Character**) or backed up ([Git backup](GITBACKUP.md)).
- Steam Cloud does not sync this file (`.json` is not a synced save type); use the [Git backup](GITBACKUP.md) to carry
  it to another computer.
