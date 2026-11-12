# Titan Quest Game File Formats

This document describes the binary file formats used by Titan Quest / Immortal Throne / Anniversary Edition.
It is essential for anyone working on the ARC/ARZ file I/O layer (`src/TQVaultAE.GameFiles`, shared by TQVaultAE
and `tqdb`; `src/TQVaultAE.Data` holds TQVaultAE's adapters over it) or on the [`tqdb` command line](DBCLI.md)
(`src/TQVaultAE.DbCli`).

The original layouts come from the TQVault authors. They were re-verified byte by byte in 2026 against the
complete Steam Anniversary Edition install (with all DLCs):
* `database.arz`: 74,013 records.
* 135 `.arc` files: 78,811 entries, 13.56 GB.
* Every `.tex`, `.msh` and audio entry.

Figures quoted below come from that install.

---

## Table of contents
* [Conventions](#Conventions)
* [Game file layout](#Layout)
* [ARC archives (`.arc`)](#ARC)
* [ARZ database (`.arz`)](#ARZ)
* [Localized text (`Text_XX.arc`)](#Text)
* [Display names](#DisplayNames)
* [Textures (`.tex`)](#TEX)
* [Other asset formats](#OtherAssets)
* [Item stat randomization (jitter)](#Jitter)
* [Player save files (`Player.chr`)](#Player)
* [Common pitfalls](#Pitfalls)
* [Credits](#Credits)

---

## <a id="Conventions"></a>Conventions

* All integers are **little-endian**.
* `u32`/`i32` are 4 bytes and `u16`/`i16` are 2 bytes.
* `FILETIME` is the Windows 64-bit count of 100 ns intervals since 1601-01-01 UTC.
* `adler32` is the zlib Adler-32 checksum.
* Strings are **cp1252** (Windows-1252), never UTF-8 or Latin-1 (`0x92` is `’` in cp1252 but a control character in Latin-1), except the UTF-16 text files described [below](#Text) and the UTF-16 strings of the save files.

---

## <a id="Layout"></a>Game file layout

| Folder | Content |
|---|---|
| `Database\database.arz` | The only database. It is 60 MB and holds the DLC records too (under `records\xpack*`). |
| `Resources\*.arc` | Base game resources (24 archives). |
| `Resources\xpack\*.arc` | Immortal Throne. The folder name is lowercase. |
| `Resources\XPack2\*.arc` | Ragnarok. |
| `Resources\XPack3\*.arc` | Atlantis. |
| `Resources\XPack4\*.arc` | Eternal Embers. Its items archive is `Item.arc`, not `Items.arc`. |
| `Audio\*.arc` | Sounds, music and dialogs (`Dialog_XX.arc`). |
| `Text\Text_XX.arc` | Localized text in 13 languages: BR CH CZ DE EN ES FR IT JA KO PL RU UK. |
| `Toolset\*.arc` | Editor templates and tutorials. |

**Resource ids.** Records reference assets by a *resource id*: `<archive stem>\<path inside the archive>`,
for example `Items\EquipmentHelm\UIBitmaps\mi_n_gorgonmelee.tex`. The first segments give the archive, as the
archives of the game install hold them:

| Resource id | Archive | Entry path |
|---|---|---|
| `XPack\<name>\...`, `XPack2\<name>\...`, `XPack3\<name>\...`, `XPack4\<name>\...` | `Resources\XPackN\<name>.arc` (sounds, dialogs and music too) | after `XPackN\<name>\` |
| `Sounds\XPack\...`, `Music\XPack\...` (Immortal Throne sounds and music, `.wav` / `.mp3`) | `Resources\Sounds.arc`, `Resources\Music.arc` | after `<name>\`: `xpack/...` |
| other `.wav` / `.mp3`: `Sounds\...`, `Dialog\...`, `PlayerSounds\...`, `Music\...` | `Audio\<name>.arc` | after `<name>\` |
| anything else (`<name>\...`) | `Resources\<name>.arc` | after `<name>\` |

The resource ids never start with `XPack\Sounds\`: there is no `Resources\XPack\Sounds.arc`. One exception to the
table: the Eternal Embers drop sounds (`Sounds\item\embers*.mp3` / `.wav`) are in `Resources\Sounds.arc`, not in
`Audio\Sounds.arc`. The `Audio\*_XX.arc` and `Resources\XPackN\*_XX.arc` archives (`Dialog_DE`, `Sounds_FR`...) hold
the localized versions.

TQVaultAE's mapping is `GameArchives.ArcFileSegments` (`src/TQVaultAE.Domain/Helpers/GameArchives.cs`), the table
above. When the first archive does not hold the entry, `GameArchives.ArcFileCandidates` tries the archive of the same
name in the expansion folders and in `Resources` (which finds the Eternal Embers drop sounds). The other way,
`ArchiveLayout.ResourcePrefix` (`src/TQVaultAE.GameFiles/ArchiveLayout.cs`) gives the first segments of the ids of an
archive's entries: its path relative to `Resources` or `Audio`, without extension and without the `_XX` language
suffix (`XPack2\Items`, `Dialog` for `Audio\Dialog_DE.arc`). tqdb's `resourceId` column uses it.

---

## <a id="ARC"></a>ARC archives (`.arc`)

All 135 archives are version 1. One of them is empty (`XPack4\Underground.arc`, 2,048 bytes, 0 files).

### Header (28 bytes)

| Offset | Type | Field | Description |
|---|---|---|---|
| 0x00 | 4 bytes | magic | `ARC\0` |
| 0x04 | u32 | version | `1` |
| 0x08 | u32 | numFiles | Number of file records |
| 0x0C | u32 | numParts | Number of parts across all files |
| 0x10 | u32 | partTableBytes | `numParts * 12` |
| 0x14 | u32 | namesBytes | Size of the names blob |
| 0x18 | u32 | tocOffset | Offset of the part table |

### Table of contents

At `tocOffset`:

1. **Part table**: `numParts` entries of 12 bytes.

   | Offset | Type | Field |
   |---|---|---|
   | +0 | u32 | offset of the part data in the file |
   | +4 | u32 | compressed size |
   | +8 | u32 | uncompressed size |

2. **Names blob** (`namesBytes`): file names separated by NUL bytes.
   * Names are cp1252: 21 names contain byte `0x92` (`’`), e.g. `iniquity’sbane_dif.tex` in `XPack4\Item.arc`.
   * They are stored in lowercase with `/` separators and no archive prefix. The exception is `Toolset\Tutorials.arc`, which has 127 mixed-case names.

**File records** are the last `44 * numFiles` bytes of the file:

| Offset | Type | Field | Description |
|---|---|---|---|
| +0 | u32 | storageType | `1` = stored (raw bytes), `3` = zlib parts |
| +4 | u32 | offset | File offset of the data (type 1) or of the first part |
| +8 | u32 | compressedSize | |
| +12 | u32 | size | Uncompressed size |
| +16 | u32 | adler32 | Adler-32 of the **uncompressed** bytes (matched on every entry checked) |
| +20 | u32 | fileTimeLow | `FILETIME` of the source file |
| +24 | u32 | fileTimeHigh | |
| +28 | u32 | numParts | Number of parts (`0` = inactive entry, see below) |
| +32 | u32 | firstPart | Index of the first part in the part table |
| +36 | u32 | nameLength | Length of the name |
| +40 | u32 | nameOffset | Offset of the name **relative to the names blob** |

### Data

* **Type 3:** the file is the concatenation of `numParts` consecutive parts. Each part is a complete zlib stream (header `78 DA` or `78 9C`) of at most 262,144 uncompressed bytes. Some parts have `compressedSize == size` but are **still zlib-compressed** (`Toolset\Tutorials.arc`).
* **Type 1:** raw bytes at `offset`, with `compressedSize == size`. Nearly all of them are `.mp3` dialog files. `Audio\Dialog_DE`, `_FR` and `_RU` contain only stored entries.
* **Inactive entries:** type 3 with `numParts == 0` and size 0 (15 in `Tutorials.arc`). They still occupy a name in the blob, so names must be read through `nameOffset`/`nameLength`, never sequentially.
* Names are unique within an archive, even ignoring case. All offsets fit in a signed 32-bit integer.
* The largest entry is `world/world01.map` in `Resources\Levels.arc`: 2,004,303,764 bytes across 7,646 parts. Stream it part by part instead of allocating the whole file.

### Content (extension histogram)

| Ext | Count | Size | Ext | Count | Size |
|---|---|---|---|---|---|
| tex | 25,278 | 9.66 GB | anm | 4,379 | 588 MB |
| mp3 | 22,830 | 4.70 GB | pfx | 2,053 | 7.5 MB |
| msh | 15,357 | 2.36 GB | txt | 700 | 35 MB |
| wav | 7,102 | 1.66 GB | tpl / qst / ssh | 565 / 269 / 148 | |

Also present in small numbers: `map`, `lvl`, `rlv`, `sd`, `wrl`, `fnt`, `bin`, `tga`, `dbr` and `csv`.

---

## <a id="ARZ"></a>ARZ database (`.arz`)

### Layout

1. **Header** (24 bytes) at offset 0.
2. **Record data** from offset 24: one independent zlib stream per record.
3. **Record table** at `recordTableStart`.
4. **String table** at `stringTableStart`.
5. **Trailer** (16 bytes) at the end of the file.

### Header (24 bytes)

| Offset | Type | Field | Description |
|---|---|---|---|
| 0x00 | 2 × u16 | magic / version | `0x0004`, `0x0003` (read as one int32: `0x00030004`) |
| 0x04 | u32 | recordTableStart | |
| 0x08 | u32 | recordTableSize | |
| 0x0C | u32 | recordCount | 74,013 |
| 0x10 | u32 | stringTableStart | |
| 0x14 | u32 | stringTableSize | |

### String table

`u32 count` (190,522), followed by `count` strings, each stored as `u32 length` plus `length` bytes of cp1252.
The strings are **length-prefixed, not NUL-terminated**. String ids are 0-based indices into this table.

### Record table entry

| Type | Field | Description |
|---|---|---|
| u32 | nameStringId | Record id, e.g. `records\item\equipmentweapon\sword\c10_sword01.dbr` |
| u32 | classLength | |
| bytes | class | The record's class (`classLength` bytes, no NUL), equal to its `Class` variable |
| u32 | offset | Data offset, **relative to 24** |
| u32 | compressedSize | |
| 8 bytes | FILETIME | Last write time of the source `.dbr` |

* Record ids are unique, even ignoring case.
* There are 286 distinct classes. 8,126 records have an empty class (item sets, loot tables...).
* Because the class is in this table, a query can filter by class or id **without decompressing anything**.

### Record data

Each record is a zlib stream (header `78 5E`). Decompressed, it is a sequence of variables:

| Type | Field | Description |
|---|---|---|
| u16 | dataType | `0` int32, `1` float32, `2` string id, `3` bool (int32 0/1) |
| u16 | valueCount | `1` for scalars; arrays are used for per-level values, weights... |
| u32 | nameStringId | Variable name |
| `valueCount` × 4 bytes | values | |

All records together decompress to 212.8 MB, about 17.6 million values:
* 13.47 M floats
* 1.79 M bools
* 1.26 M ints
* 1.11 M strings

### Trailer (16 bytes)

Four `adler32` values, covering:
1. the whole file before the trailer;
2. the string table;
3. the record table;
4. the record-data region.

---

## <a id="Text"></a>Localized text (`Text_XX.arc`)

`Text_EN.arc` contains 57 `.txt` files; the other languages have 47 to 53.

**Encoding**
* 53 of the 57 files are UTF-16LE with a BOM.
* 4 are **cp1252 without a BOM**: `x2mainquest`, `x2sidequest`, `x2skills`, `x2tutorial`.
* Detect the encoding by the BOM.

**Syntax**
* Each line is `tag=text`. Lines starting with `//` are comments.
* Split on the **first** `=` only: some texts contain `=`.
* Tags are case-insensitive. 374 tags are defined twice; the **last** definition wins.
* Load all the files: the dialog, quest and `x*` files hold tags that items and skills use.

**Markup inside texts**
* `{^x}` is a colour code (e.g. `{^l}`).
* `[ms]`, `[fs]`, `[ns]` are gender variants used by languages such as FR and DE. For example, FR `tagItemHookedBlade=[fs]Lame crochue`. Keep the first variant.

---

## <a id="DisplayNames"></a>Display names

The display name of a record is the localized text of a *name tag* variable. Which variable holds it depends on the class:

| Class | Name tag variable | Notes |
|---|---|---|
| `Armor*`, `Weapon*`, shields | `itemNameTag` | Plus `itemQualityTag` and `itemStyleTag` for the full item name |
| `Monster`, `Npc`, `Pet`, `FixedItem*` | `description` | NPC descriptions are often literal text, not a tag |
| `ItemRelic`, `ItemCharm`, `ItemArtifact`, `ItemArtifactFormula`, `OneShot_*`, quest items, dyes | `description` | |
| `Skill*`, `Skill_Mastery` | `skillDisplayName` | Plus `skillBaseDescription` |
| `LootRandomizer` (affixes) | `lootRandomizerName` | |
| Item sets (empty class) | `setName` | |

`FileDescription`, `ActorName` and `itemSetName` are designer notes. They never resolve to text, but they are useful for searching.

---

## <a id="TEX"></a>Textures (`.tex`)

A `.tex` wraps one or more DirectDraw Surface frames. All 25,278 files in the game parse with one of two versions:

| Version | Layout | Count |
|---|---|---|
| 1 | `"TEX" 0x01`, `u32 fps`, then frames | 19,381 |
| 2 | `"TEX" 0x02`, `u32 fps`, `u8 flag` (0 or 1), then frames | 5,897 |

* Each frame is `u32 size` followed by `size` bytes of DDS.
* `fps > 0` marks about 63 **animated** textures with several frames (e.g. `lstrikebolta.tex`, 5 frames).
* Formats are DXT1/3/5 and uncompressed 24- or 32-bit RGB(A). There are 1 to 14 mip levels.
* Each DDS payload is exactly 128 bytes of header plus the computed mip chain.

**Converting a frame to a standard `.dds` takes more than stripping the header:**
1. The DDS magic is `DDSR` in all but 258 files: write `DDS ` instead.
2. Uncompressed formats have RGB(A) bit masks of 0 and no `DDPF_ALPHAPIXELS` flag. Fill in the BGRA masks, and set the alpha flag for 32-bit formats.
3. `caps1` is 0 in about 97% of the files: OR in `DDSCAPS_TEXTURE` (`0x1000`).

`BitmapService.LoadFromTexMemory` and `TexConverter` (DbCli) both apply these fix-ups.

**Item size.** An item takes one inventory cell per 32 pixels of its picture. `TexHeader.TryReadSize` reads the size from the DDS header (height at byte 12, width at byte 16, counted from the start of the DDS data, magic included) without decoding, so `ItemProvider.GetDBData` knows the size of every item when it is loaded, before its picture is drawn.

---

## <a id="OtherAssets"></a>Other asset formats

All of these export as **byte-for-byte copies**: the raw entry is the native format.

| Ext | Magic | Notes |
|---|---|---|
| `.msh` | `MSH` version 11 (11,369) or 10 (3,982); 6 files use `AST\x02` | After the magic: `u32 0, u32, u32`, then the source `.mif` path |
| `.anm` | `ANM\x02` (one file `ANM\x03`) | Animation |
| `.ssh` | `SSH\x05` | |
| `.pfx` | first byte `0x65` | Particle effect |
| `.qst` | first byte `0x0B`, then `begin_block` | Quest |
| `.wav` | `RIFF`/`WAVE` | 6,093 PCM; some have `bext` or `JUNK` chunks |
| `.mp3` | `FF FB`, `ID3`, or zero padding before the first frame | |
| `.fnt` | `00 01 00 00` (TrueType, 8 files) or `OTTO` (OpenType, 1 file) | A renamed `.ttf`/`.otf` |

---

## <a id="Jitter"></a>Item stat randomization (jitter)

The values in the database are not always the values shown in game. When an item is created, the game rolls part of its stats from the **item seed** saved with the item. Issue [#552](https://github.com/EtienneLamoureux/TQVaultAE/issues/552) describes it. It is implemented in `src/TQVaultAE.Domain/Jitter/` and reproduces the in-game tooltips exactly.

### Random number generator

* Park–Miller MINSTD: `s' = s × 16807 mod (2³¹ − 1)`.
* The item uses one generator, seeded with the item seed and **advanced once** before the first roll.
* A draw in `[min, max]`:
  * `d = (uint)s' / 2147483647.0`, then `d = d × (max − min) + min`, in double precision;
  * the result is `(int)((float)d + 0.5f)`: truncation **toward zero**, so negative values round up.

### One roll (`Jitter`)

```
if v == 0: return v                                 (no draw)
k = (int)(pct / 100 × v); if k == 0: k = 1
r = draw(−k, k) + v
return |r| < 1 ? v : r
```

### Roll order

The five equipment stores roll in this fixed order: **Character → Skill → Damage → Retaliation → Defense**.
Inside each store, attributes roll in a fixed order (246 attributes, listed in `EquipmentRollTable.Data.cs`).

### Rules

* **Character and Skill affixes** use the affix's `lootRandomizerJitter` (0 when absent), with no cap. They roll whenever the value is non-zero, **even at 0%**: k is forced to 1, so the value can gain +1.
* **Base values** of the item roll with the percent hard-coded by the store: 20% for most attributes, 0 for some Character ones and every Skill one.
* **Damage, Retaliation and Defense affixes** roll at the affix's `lootRandomizerJitter`, only when it is above 0, capped at 50%.
* **Damage ranges** are rolled as `(min, max − min)`: in-game max = rolled min + rolled range. There is no second draw when max is absent or not above min.
* An attribute **does not exist, and uses no draw**, when its required value is 0. Examples: a damage pair without min; a damage over time without `*DurationMin`.
* **Never rolled:**
  * base armor (`defensiveProtection`);
  * base weapon damage, base attack speed and level-requirement reductions;
  * every `*Chance` variable;
  * durations.
* **Affix armor bonuses** roll at the store's 20%, on armor items only.
* Some attributes load **only on armor items** (helms, chest, arms, legs) or **only on non-armor items** (weapons, shields, jewellery).

### After the rolls: `attributeScalePercent`

When the **base item** has `attributeScalePercent = p`, every store is scaled once all the rolls are done. This concerns 152 rings and amulets (20–25%, some amulets up to 100%) and 8 weapons:

```
v = v + (int)(v × p / 100)        (single precision, truncation toward zero)
```

* The scale applies to base, prefix and suffix values. It uses no draw.
* **Damage pairs:** only the min is scaled; the range is kept, so the max moves with the min.
* **Base armor** is scaled; **affix armor bonuses** are not.
* **Not scaled:** speeds (`characterAttackSpeed`, `characterSpellCastSpeed`, `characterRunSpeed`), `characterLevelReqReduction`, `characterBaseAttackSpeed` and the physical `offensivePierceRatio`.

Example, a "Jewel of Essence" ring (`attributeScalePercent` 25, seed 21654). Its suffix has Energy 86 and Energy Regen 12%, with `lootRandomizerJitter` 18:
* the jitter rolls 82 and 14;
* the scale gives 82 + (int)20.5 = **102** and 14 + (int)3.5 = **17**, as shown in game.

### Last: Skill store base jitter

The base Skill values (`skillCooldownReduction`, `skillManaCostReduction`…) are rolled **last**, after the scaling, at 20%.

### Socketed relics and charms

Relics and charms socketed in an item are **neither jittered nor scaled** by the item's `attributeScalePercent`. The game shows the raw database values:
* Their arrays are indexed by the number of shards: `var1` / `var2` in the save file. A completed relic uses the last entry.
* The completion bonus (`relicBonus`) also keeps its database value.

Example, Hecate's Crescent completed (3 shards) in a ring with `attributeScalePercent` 25:
* `characterManaModifier = 4;8;12` and `characterManaRegenModifier = 20;40;60` give **+12% / +60%** in game;
* the bonus record `Character_AttributeIntelligence_02` (11) gives **+11** Intelligence.

The jitter of relics and charms as standalone items (relic/charm records rolled on their own) was not analysed.

---

## <a id="Player"></a>Player save files (`Player.chr`)

Player save files use a binary key-value format. The [Save File Explorer](SAVEFILEEXPLORER.md) shows the keys of a file.

The file is modified by:
1. Searching for a key string in the binary content
2. Finding the existing value offset after the key
3. Replacing the value with the new content
4. Adjusting lengths as needed

### Key file sections
- Mastery/allowed sections
- Equipment segment
- Item segments
- Skill segments

---

## <a id="Pitfalls"></a>Common pitfalls

| Pitfall | Correct handling |
|---|---|
| Reading ARC names sequentially | Use each record's `nameOffset`/`nameLength`; inactive entries break sequential reading. |
| Decoding names as ASCII, UTF-8 or Latin-1 | Use cp1252 (`’` = `0x92`). |
| Treating `compressedSize == size` as "stored" in a type 3 entry | Only `storageType == 1` is raw. |
| Allocating a whole entry in memory | Stream part by part (`world01.map` is 2 GB). |
| Reading text with the default (UTF-8) encoding | Use the BOM if present, otherwise cp1252. |
| `line.Split('=')[1]` | Split on the first `=`. |
| Reading the u32 at `.tex`+4 as a data offset | It is the frame rate; frames follow. |
| Assuming DB values are in-game values | Apply the [jitter](#Jitter) with the item seed. |

A zlib stream can be inflated with a raw deflate decoder after skipping its 2 header bytes, but that skips the Adler-32 check. ARC entries also carry their own `adler32`, and the ARZ trailer covers the whole file.

---

## <a id="Credits"></a>Credits

The original ARC/ARZ layouts were documented by the original TQVault authors (Brandon Wallace, Jesse Calhoun and VillageIdiot). The 2026 update re-verified every field and resolved the unknown ones: ARC header, checksums and timestamps; ARZ magic, trailer and string encoding. It added the text, texture, asset and jitter sections.
