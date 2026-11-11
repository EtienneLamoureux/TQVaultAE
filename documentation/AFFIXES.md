# Item editing features

Beside the [Forge](FORGE.md) that can be overkill, TQvault let you tweek some items in a respectful manner.

You can choose item affixes from regular loot table instead of farming duplicate and using the forge.

### _Theory crafting made easy!!_

_**Note : "Item editing feature" must be enable in the settings: tick "Allow Cheats", then "Allow Item Editing Features".**_

The entries are in the right-click menu of an item. The affix entries only show for weapons, shields, armors and jewellery.

_The screenshots of this page were taken with an older version. The menu now also has "🔗 Share Item", "Modded",
"Change Item Seed (Force)" and the "Display affixes by name/effect" entry, and the copy entries read
"Copy Item (Randomized seed)" and "Duplicate Item (Seed unchanged)"; these two need the "Allow Item Copying" setting._

---

## Table of contents
* [Prefix change](#prefix-change)
* [Suffix change](#suffix-change)
* [Broken items](#broken-affix-when-available)
* [Affixes removal](#affixes-removal)
* [Affixes display mode](#affixes-display-mode)
* [Affixes for Epic & Legendary](#affixes-for-epic--legendary)
* [Artefact creation](#artefact-creation)
* [Artefact completion bonus change](#artefact-completion-bonus-change)
* [Relic and charm completion](#relic-and-charm-completion)
* [Relic and charm completion bonus change](#relic-and-charm-completion-bonus-change)
* [Socketed relic and charm completion bonus change](#socketed-relic-and-charm-completion-bonus-change)
* [Relics removal](#relics-removal)
* [Missing records/translations](#missing-records--translations)
* [Item seed change](#item-seed-change)
* [Create missing set pieces](#create-missing-set-pieces)

---

### Prefix change

![Prefix change](affixes/prefix.png)

---

### Suffix change

![Suffix change](affixes/suffix.png)

---

### Broken affix when available

![Broken affix when available](affixes/broken.png)

---

### Affixes removal

![Affixes removal](affixes/removeaffixes.png)

---

### Affixes display mode

![Affixes removal](affixes/displaymode.png)

---

### Affixes for Epic & Legendary
You can enable this feature in the settings.

It try to suck all known Prefix/Suffix per gear type (e.g, all affixes for axe if the unique is an axe).

![Artefact creation](affixes/uniques.png)

---

### Artefact creation

![Artefact creation](affixes/artefactcreation.png)

---

### Artefact completion bonus change

![Artefact completion bonus change](affixes/artefactcompletion.png)

---

### Relic and charm completion

![Relic and charm completion](affixes/reliccompletestack.png)

---

### Relic and charm completion bonus change

![Relic and charm completion bonus change](affixes/reliccompletion.png)

---

### Socketed relic and charm completion bonus change

![Socketed relic and charm completion bonus change](affixes/socketedreliccompletion.png)

---

### Relics removal

![Relics removal](affixes/removerelic.png)


---

### Missing records & translations

![Missing records](affixes/brokenrecord.png)

![Missing translations](affixes/missingtranslation.png)

if you want the full list of incomplete records, add these settings in `UserConfig.xml`, next to `TQVaultAE.exe`

- DebugEnabled = True
- LootTableDebugEnabled = True

By using the feature, you will see something like [that](affixes/loottablelogs.png) in the log file of the tool (`./Logging` directory).

---

### Item seed change

| ![Open Seed form](affixes/seed1.png) | ![Change Seed](affixes/seed2.png)

"Change Item Seed..." opens the form above; the seed is a number from 1 to 32767.
"Change Item Seed (Force)" gives the item a new random seed at once, without the form.

---

### Create missing set pieces

![Create missing set pieces](affixes/missingset.png)


