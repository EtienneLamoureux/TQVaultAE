# DB CLI (`tqdb`) - Titan Quest database command line

`tqdb` searches the Titan Quest Anniversary Edition game database (`Database\database.arz`) and the game archives (`*.arc`), and exports what it finds as JSON, TSV, ARZExplorer-style `.dbr` text or raw asset files (sounds, meshes, textures, optionally converted to `.dds` / `.png`).

It is the scriptable counterpart of [ARZ Explorer](ARZEXPLORER.md): no UI, fast, pipe-friendly. The file formats it reads are described in [Titan Quest Game File Formats](tq-file-formats.md). Typical uses: find the record behind an in-game item, compare items or affixes, feed spreadsheets and scripts, extract icons and sounds for a wiki or a mod.

> **Usage notice.** `tqdb` only reads *your own* game installation. Assets and data extracted from the game remain the property of their owners: use them for personal, modding or reference purposes and do not redistribute them.

---

## Table of contents
* [Executable](#Exec)
* [Quick start](#QuickStart)
* [Recipes](#Recipes)
* [Global options](#Global)
* [Patterns](#Patterns)
* [Commands](#Commands)
    + [info](#info)
    + [records](#records)
    + [record](#record)
    + [text](#text)
    + [files](#files)
    + [extract](#extract)
* [Output formats](#Formats)
* [Exit codes](#ExitCodes)
* [Performance notes](#Performance)

---

## <a id="Exec"></a>Executable
`tqdb` is built from `src/TQVaultAE.DbCli`. Run it from a terminal (`cmd`, PowerShell, Windows Terminal, a Linux shell...).

Each GitHub Release ships it as a self-contained single-file executable, which needs no .NET runtime:

| Archive | Platform | Executable |
|---|---|---|
| `tqdb-win-x64-<version>.zip` | Windows x64 | `tqdb.exe`, next to `Magick.Native-Q8-x64.dll` |
| `tqdb-linux-x64-<version>.zip` | Linux x64 | `tqdb` (run `chmod +x tqdb` once: zip archives do not keep the executable bit), next to `Magick.Native-Q8-x64.dll.so` |

Keep the two files of the archive together: the Magick.NET native library is loaded from the executable's folder.

To build it yourself (needs the .NET 10 SDK; the result needs the .NET 10 runtime):

```
dotnet build src/TQVaultAE.DbCli/TQVaultAE.DbCli.csproj --configuration Release
src\TQVaultAE.DbCli\bin\Release\net10.0\tqdb.exe --help
```

To publish the same single files as the releases:

```
pwsh -File .github/scripts/Publish-Release.ps1 -OutputDir publish
```

The game is found automatically (in this order): `--game <path>`, the `TQ_GAME_PATH` environment variable, Steam (app 475150, every Steam library), GOG, well-known install folders.

Every command has its own help: `tqdb <command> --help` (or `tqdb help <command>`).

---

## <a id="QuickStart"></a>Quick start
```
tqdb info
tqdb records --class ItemRelic --fields id,displayName,completedRelicLevel
tqdb records --name "*hooked*" --fields "*" --format json
tqdb record records/item/equipmentweapon/sword/c10_sword02.dbr --resolve
tqdb text --key tagItemHookedBlade
tqdb files --archive Items --ext tex --limit 20
tqdb extract --archive Items --ext tex --convert png --out .\textures
```

---

## <a id="Recipes"></a>Recipes

**Identify an in-game item from its tooltip.** Base armor is never randomized, so it pins the base item; affixes are then found by their values:
```
tqdb records --class ArmorProtective_Head --where defensiveProtection=210 --fields id,displayName,characterDexterity,defensivePierce
tqdb records --class LootRandomizer --where characterDefensiveAbility=35 --fields id,displayName,lootRandomizerJitter
```

**Strongest fire weapons, as a spreadsheet:**
```
tqdb records --class "Weapon*" --where "offensiveFireMin>50" --sort offensiveFireMax:desc --fields id,displayName,itemLevel,offensiveFireMin,offensiveFireMax --out fire.tsv
```

**Everything about one item, with the records it references inlined:**
```
tqdb record records/item/equipmenthelm/mi_e_gorgonmelee.dbr --resolve --follow "*" --format json
```

**Translate a tag, or find the tag of a text:**
```
tqdb text --key tagItemHookedBlade --lang FR
tqdb text --value "*de la chasse*" --lang FR
```

**Item icons as PNG, from the `bitmap` path of a record:**
```
tqdb extract --resource "Items\EquipmentHelm\UIBitmaps\mi_n_gorgonmelee.tex" --convert png --out .\icons
```

**Whole database to JSON Lines** (about 5 s, 600 MB), for `jq`, a database or a script:
```
tqdb records --fields "*" --format jsonl --out database.jsonl
```

---

## <a id="Global"></a>Global options
Accepted before or after the command name.

| Option | Description |
|---|---|
| `-g, --game <path>` | Game install folder. |
| `-q, --quiet` | No progress or informational messages on stderr (results still go to stdout). |
| `--threads <N>` | Worker threads for record decoding and archive I/O (default: CPU count). |
| `--version` | Print the version. |
| `-h, --help` | Help. |

Option syntax: `--name value`, `--name=value`, `-n value`, `-nvalue`; combined short flags (`-qr`); repeatable options may be given several times; list options also accept commas (`--fields id,class --fields itemLevel`); `--` ends the options.

---

## <a id="Patterns"></a>Patterns
Options documented as `glob|/re/` accept:
* a **glob**, matched against the whole value: `*` any text, `?` one character, `[abc]` / `[!abc]` character sets. Example: `--class "Item*"`.
* a **regular expression** between slashes, searched anywhere in the value: `--id "/sword0[1-3]/"`.

Matching is always case-insensitive. For paths (record ids, `templateName`, archive entry names, resource ids) `/` and `\` are equivalent, so `records/item/*` and `records\item\*` are the same pattern; inside a regex write `/`.

Repeating the same filter ORs the patterns (`--class ItemRelic --class ItemCharm`); different filters are ANDed.

---

## <a id="Commands"></a>Commands

### <a id="info"></a>info
Game root, database path and size, record count, class histogram, archive count/size/entries, available text languages.

| Option | Description |
|---|---|
| `--top <N>` | Classes in the histogram (default 20, `0` = all). |
| `--format text\|json` | Default `text`. |
| `-o, --out <path>` | Output file. |

### <a id="records"></a>records
Search records.

**Filters**

| Option | Description |
|---|---|
| `-i, --id <glob\|/re/>` | Record path. Filtered on the record table, before decompression. An exact id (no wildcard) is an indexed lookup. |
| `-c, --class <glob>` | Record class. Filtered on the record table, before decompression. |
| `--template <glob\|/re/>` | `templateName` value. |
| `-n, --name <glob\|/re/>` | Resolved display name, in `--lang`. |
| `--tag <glob\|/re/>` | The name tag itself (`tagItemHookedBlade`). The name tag comes from `itemNameTag`, `description`, `skillDisplayName`, `lootRandomizerName`, `setName`... |
| `-w, --where <expr>` | Field predicate (repeatable, ANDed), see below. |
| `-t, --text <substring>` | Case-insensitive substring in any string value. |
| `-l, --lang <code>` | Language of display names and `--resolve` (default `EN`; see `tqdb info`). |

**`--where` expressions**

| Expression | Meaning |
|---|---|
| `field=value` | Equal. Numbers compare numerically (floats with single-precision tolerance), strings ignore case and treat `/` and `\` alike. |
| `field!=value` | No value equals (true when the field is absent). |
| `field>n` `field>=n` `field<n` `field<=n` | Numeric comparison when both sides are numbers, else ordinal case-insensitive text comparison. |
| `field~pattern` | Glob or `/regex/` on the value text. |
| `field?` | Field exists and is non-zero / non-empty. |
| `!expr` | Negation, e.g. `!levelRequirement?`. |

Field names are case-insensitive and may be globs (`offensive*Min>10` = any matching field). Multi-value fields match when **any** value matches. Pseudo fields `id`, `class`, `displayName` and `nameTag` can be used too. Quote expressions in the shell (`"itemLevel>=40"`): `>` and `<` are redirections otherwise.

**Output**

| Option | Description |
|---|---|
| `-f, --fields <list>` | Columns: `id`, `class`, `displayName`, `nameTag`, variable names, globs over variable names, `*` = every variable. Default `id,class,displayName`. |
| `-r, --resolve` | Add the resolved text of tag-valued string fields. |
| `--follow <field>` | Inline the records referenced by this field (string values ending with `.dbr`; glob allowed), one level. |
| `-s, --sort <field[:desc]>` | Sort by a column or variable; numbers numerically; records without the field last. |
| `--limit <N>` | Maximum records. |
| `--count` | Print only the number of matches. |
| `--format json\|jsonl\|tsv\|dbr` | Default `tsv`, or inferred from the `--out` extension. |
| `--compact` | Non-indented JSON. |
| `-o, --out <path>` | Output file; with `--format dbr`, a **directory** receiving one `.dbr` per record (path mirrored, like ARZ Explorer), unless the path ends with `.dbr`/`.txt`. |

Examples:
```
tqdb records --id "records/item/equipmentweapon/sword/*" --where "itemLevel>=40" --sort itemLevel:desc --limit 20 --fields id,displayName,itemLevel
tqdb records --class "WeaponMelee*" --where "offensive*Min?" --count
tqdb records --where "itemClassification=Epic" --fields id,displayName,itemLevel,levelRequirement --out epics.tsv
tqdb records --class LootRandomizer --fields id,displayName,"offensive*" --format jsonl
tqdb records --class ItemArtifactFormula --format dbr --out .\dbr
tqdb records --tag tagItemHookedBlade --fields "*" --resolve --format json
```

### <a id="record"></a>record
`tqdb record <id> [<id>...]`: one record (or a few) with every field. The id accepts `/` or `\` and any case. An unknown id exits with code 3 and suggests records with the same file name.

| Option | Description |
|---|---|
| `--format json\|jsonl\|tsv\|dbr` | Default `json` (a single object for a single id). |
| `-f, --fields <list>` | Variables to show (names or globs), default all. |
| `-r, --resolve` | Add the resolved text of tag-valued fields. |
| `--follow <field>` | Inline referenced records one level (`--follow "*"` follows every reference). |
| `-l, --lang`, `--compact`, `-o, --out` | As for `records`. |

```
tqdb record records\xpack\items\loottables\arms\mixed_arms_n01.dbr --format dbr
tqdb record records/item/equipmentweapon/sword/c10_sword02.dbr --fields "item*" --resolve
tqdb record <loot table id> --follow "lootName*"
```

### <a id="text"></a>text
Search localization tags (`Text\Text_XX.arc`): tag code <-> display text.

| Option | Description |
|---|---|
| `-k, --key <glob\|/re/>` | Tag name (`tagItem*`). |
| `-v, --value <glob\|/re/>` | Text (`"*hooked*"`). |
| `-l, --lang <code>` | Default `EN`. |
| `--limit`, `--count` | As for `records`. |
| `--format json\|jsonl\|tsv` | Default `tsv`. Columns `tag`, `text`; sorted by tag. |

```
tqdb text --value "*of the hunt*" --lang FR
tqdb text --key "/^tagSkill/" --format json --out skills.json
```

### <a id="files"></a>files
List archive entries. Columns: `archive` (path relative to the game root without extension, e.g. `Resources/XPack/Items`), `name` (entry path), `resourceId` (as the database references it, e.g. `Items\weapons\x.tex`), `size`, `compressed`, `storage` (1 stored, 3 zlib), `type` (texture, mesh, animation, sound, font, text, effect...), `lastWrite` (UTC).

| Option | Description |
|---|---|
| `-a, --archive <glob\|/re/>` | Archive key (`Resources/XPack/Items`), file name (`Items.arc`) or stem (`Items`). Only matching archives are opened. |
| `-p, --path <glob\|/re/>` | Entry name. |
| `--resource <glob\|/re/>` | Resource id, e.g. the `bitmap` / `baseTexture` / `mesh` value of a record. |
| `-e, --ext <list>` | Extensions: `tex,mp3,msh`. |
| `--limit`, `--count`, `--format json\|jsonl\|tsv`, `-o, --out` | |

```
tqdb files --resource "Items\EquipmentWeapon\Sword\*" --ext tex
tqdb files --archive "Audio/*" --ext mp3,wav --count
```

### <a id="extract"></a>extract
Copy archive entries to a folder. Same selectors as `files`.

| Option | Description |
|---|---|
| `-o, --out <dir>` | Destination folder (required). |
| `--convert none\|dds\|png` | Textures only (`.tex`). `none` (default) copies the raw `.tex`. `dds` writes a valid DirectDraw Surface, `png` a PNG. A texture that cannot be converted is written raw, with a warning. |
| `--frames first\|all` | Animated textures with `--convert`: first frame (default) or every frame as `name_frameN.ext`. |
| `--verify` | Check Adler-32 checksums. |
| `--parallel <N>` | Concurrent extractions (default `--threads`). |
| `--flat` | `<out>/<name>` instead of `<out>/<archive>/<name>`. When several archives hold the same name, only the entry of the last archive in path order is written (expansions such as `Resources/XPack*` win over the base game). |
| `--skip-existing` | Keep existing files of the same size (raw copies). |

Raw copies are the native form of every non-texture asset (`.mp3`, `.wav`, `.msh`, `.anm`, `.fnt`, `.pfx`...). Paths are `<out>/<archive key>/<entry name>`; names are sanitized (`..` and rooted names rejected, characters invalid on Windows replaced by `_`, accented letters kept). Progress is shown on stderr, a summary at the end; the exit code is 1 when any entry failed.

```
tqdb extract --archive Items --ext tex --convert png --out .\textures
tqdb extract --archive "Audio/*" --ext mp3,wav --out .\sounds --verify
tqdb extract --resource "Creatures\Monster\Boarman\*" --out .\boarman
```

---

## <a id="Formats"></a>Output formats
* **json**: an array of objects (`tqdb record` with one id: a single object):
  ```json
  {"id": "records\\item\\...\\c10_sword02.dbr", "class": "WeaponMelee_Sword", "displayName": "Hooked Blade",
   "fields": {"itemLevel": 25, "characterBaseAttackSpeed": 0.45, "castsShadows": true, "weights": [5, 50]},
   "resolved": {"itemNameTag": "Hooked Blade"},
   "linked": {"lootName1": [ { ...record... } ]}}
  ```
  Values are typed (int, float, bool, string); one value is a scalar, several an array. `displayName` / `nameTag` appear when projected (`null` when the record has none); `fields` when variables are projected; `resolved` with `--resolve`; `linked` with `--follow`.
* **jsonl**: one compact object per line (streams well into `jq`, scripts, databases).
* **tsv**: a header row, one row per record; multi-values joined by `;`; tab, CR and LF inside values written as `\t`, `\r`, `\n`. With `--resolve` each variable column gets a `<name>:text` column. When the projection is dynamic (`*` or a field glob) the TSV switches to **long format**: one row per variable with columns `id`, `class`, [`displayName`], `field`, `type`, `value`[, `text`].
* **dbr**: ARZ Explorer text format, one `name,value1;value2,` line per variable (floats with 6 decimals, booleans `1`/`0`). To stdout, each record is preceded by a `// <id>` line when several records can be written.

Output goes to stdout unless `--out` is given; messages go to stderr, so `tqdb records ... > out.tsv` keeps the file clean. The `--out` extension (`.json`, `.jsonl`, `.tsv`, `.dbr`) selects the format when `--format` is omitted.

---

## <a id="ExitCodes"></a>Exit codes
| Code | Meaning |
|---|---|
| 0 | Success (also when nothing matched). |
| 1 | Runtime failure (corrupt file, I/O error, an entry failed to extract). |
| 2 | Usage error (unknown option, bad value, invalid pattern or `--where`). |
| 3 | Not found (game install, record id, text language). |
| 130 | Cancelled (Ctrl+C). |

Set `TQDB_DEBUG=1` to print stack traces on failures.

---

## <a id="Performance"></a>Performance notes
* `--id` and `--class` run on the record table and never decompress non-matching records: put them first when you can.
* Other filters decode the candidate records in parallel (`--threads`).
* Display names are only resolved when a name filter, column or sort needs them.
* Results stream as they are found; only `--sort` buffers matches.
* Archives are opened lazily and in parallel; only the archives selected by `--archive` are opened.
