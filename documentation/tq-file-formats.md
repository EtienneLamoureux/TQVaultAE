# Titan Quest Game File Formats

This document documents the reverse-engineered binary file formats used by Titan Quest / Titan Quest Immortal Throne.
This knowledge was obtained through reverse engineering and is essential for anyone working on the ARC/ARZ file I/O layer.

---

## ARC File Format (`.arc`)

ARC files are archive containers that store compressed game resources (textures, models, etc.).

### File Header (first 0x21 bytes)

| Offset | Size | Field | Description |
|--------|------|-------|-------------|
| 0x00 | 3 bytes | Magic | `0x41 0x52 0x43` ("ARC") |
| 0x08 | 4 bytes | numEntries | Number of files in the archive |
| 0x0C | 4 bytes | numParts | Total number of compressed parts across all files |
| 0x18 | 4 bytes | tocOffset | Offset to the directory structure |

### Directory Structure (at tocOffset)

#### Part Entries (numParts * 12 bytes)

| Offset | Size | Field | Description |
|--------|------|-------|-------------|
| +0 | 4 bytes | fileOffset | Offset in file where this part begins |
| +4 | 4 bytes | compressedSize | Size of the compressed part |
| +8 | 4 bytes | realSize | Size of the uncompressed part |

These triplets repeat for each part.

After the part entries: a series of **null-terminated ASCII strings** which are the sub-filenames.

#### File Record Entries (44 bytes each, read from end of file)

Offset for file records = `fileLength - (44 * numEntries)`.

| Offset | Size | Field | Description |
|--------|------|-------|-------------|
| +0 | 4 bytes | storageType | `3` = compressed, `1` = stored (uncompressed) |
| +4 | 4 bytes | fileOffset | Offset where first part of this subfile begins |
| +8 | 4 bytes | compressedSize | Compressed size of this file |
| +12 | 4 bytes | realSize | Uncompressed size of this file |
| +16 | 4 bytes | crap | Unknown (timestamp?) |
| +20 | 4 bytes | crap | Unknown (timestamp?) |
| +24 | 4 bytes | crap | Unknown (timestamp?) |
| +28 | 4 bytes | numParts | Number of parts this file uses |
| +32 | 4 bytes | firstPart | Part index of first part for this file (0-based) |
| +36 | 4 bytes | filenameLength | Length of the filename string |
| +40 | 4 bytes | filenameOffset | Offset in directory structure for filename |

### Compression

- ARC uses **zlib/deflate** compression.
- Compressed data starts with 2 zlib header bytes (method + flags), which are skipped before passing to the deflate stream.
- For stored (non-compressed) files: `storageType == 1` and `compressedSize == realSize`.

---

## ARZ File Format (`.arz`)

ARZ files are the Titan Quest game database files containing all item/character/skill definitions.

### File Header (24 bytes)

| Offset | Size | Field | Description |
|--------|------|-------|-------------|
| 0x000000 | 4 bytes | (unknown) | Always an int32 value |
| 0x000004 | 4 bytes | recordTableStart | Start offset of the dbRecord table |
| 0x000008 | 4 bytes | recordTableSize | Size in bytes of the dbRecord table |
| 0x00000c | 4 bytes | numEntries | Number of entries in the dbRecord table |
| 0x000010 | 4 bytes | stringTableStart | Start offset of the string table |
| 0x000014 | 4 bytes | stringTableSize | Size in bytes of the string table |

After these 6 header values, the file contains:

1. **dbRecord table** at `recordTableStart` (described below)
2. **String table** at `stringTableStart`
3. **Record data** at offset 24 (each record is individually compressed)

### Record Entry Format (in dbRecord table)

Each record entry in the dbRecord table:

| Offset | Size | Field | Description |
|--------|------|-------|-------------|
| 0x0000 | 4 bytes | stringEntryID | Index into the string table (the .dbr filename) |
| 0x0004 | 4 bytes | stringLength | Length of the record type string |
| 0x0008 | variable | recordType | Null-terminated string (record type) |
| varies | 4 bytes | offset | Offset in the ARZ file where this record's data begins (add baseOffset of 24) |
| varies | 4 bytes | compressedSize | Size of the compressed record data |
| varies | 4 bytes | timestamp? | Unknown — skipped |
| varies | 4 bytes | timestamp? | Unknown — skipped |

The records are stored in the file at offset 24 + each record's `offset` field.

### String Table Format

| Offset | Size | Field | Description |
|--------|------|-------|-------------|
| 0 | 4 bytes | numStrings | Number of strings in the table |
| 4 | variable | strings[] | Series of null-terminated C-strings |

### Record Variable Format (inside decompressed record data)

Each decompressed record is a sequence of variables. Each variable has this header:

| Offset | Size | Field | Description |
|--------|------|-------|-------------|
| 0x00 | 2 bytes | dataType | Type of data: `0` = int32, `1` = float, `2` = string (index), `3` = bool |
| 0x02 | 2 bytes | valCount | Number of values (usually 1, can be > 1 for arrays) |
| 0x04 | 4 bytes | keyStringID | Index into the string table for the variable name |
| 0x08 | variable | data | The value(s) — see below |

**Data types:**
- `0x0000` (Integer): 4-byte int32
- `0x0001` (Float): 4-byte Single (bitcast from int32)
- `0x0002` (String): 4-byte int32 index into string table
- `0x0003` (Boolean): 4-byte int32 (0/1)

### Compression

- Each record is individually compressed using **zlib/deflate**.
- Compressed data starts with 2 zlib header bytes (skipped before deflate).

---

## Player Save File Format (`.player`)

Player save files use a binary key-value format.

The file is modified by:
1. Searching for a key string in the binary content
2. Finding the existing value offset after the key
3. Replacing the value with the new content
4. Adjusting lengths as needed

### Key File Sections
- Mastery/allowed sections
- Equipment segment
- Item segments
- Skill segments

---

## Notes

- All integers are **little-endian**.
- String table indices are 0-based.
- The ARZ base offset of 24 accounts for the 6 int32 header fields.
- "Crap" / "timestamp?" fields are known to exist but their exact meaning has not been determined.
- This format documentation was obtained through reverse engineering by the original TQVault authors (Brandon Wallace, Jesse Calhoun, and VillageIdiot).
