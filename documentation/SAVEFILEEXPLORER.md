# TQ Save File Explorer

This tool let you explore Titan Quest save files.

It is very useful for TQVaultAE contributors to be able to dig into these files for support and maintenance reasons.

---

## Table of contents
* [Executable](#Exec)
* [UI](#UI)
    + [Menu](#Menu)
    + [File infos](#FileInfos)
    + [Detected keys](#DetectedKeys)
    + [Key Infos](#KeyInfos)
    + [Key Data](#KeyData)
* [Hidden features](#HiddenFeatures)

---

## <a id="Exec"></a>Executable
Save File Explorer has its own archive on the [release page](https://github.com/EtienneLamoureux/TQVaultAE/releases): `TQSaveFilesExplorer-<version>.zip`. Extract it.

It reads character files (`.chr`) and stash files (`.dxb`, and their `.dxg` backups).

Just run `TQ.SaveFilesExplorer.exe`.

Create a shortcut on your desktop if you need to access it frequently.

---

## <a id="UI"></a>UI
Here's a brief overview of the UI.

![Save File Explorer UI](savefileexplorer/ui.png)

---

### <a id="Menu"></a>Menu

- File/Open player directory : Select character directory and open all player save file inside (player, caravane)
- File/Open file : Choose one save file to open. A file of another type is ignored.
- Detected Players : Easy access to all character files

![Detected Players](savefileexplorer/menuplayers.png)

- Help/About : About window 

---

### <a id="FileInfos"></a>File Infos
This panel display file parsing details.

![File Infos](savefileexplorer/fileinfos.png)

- The link open windows explorer directly to the file.
- DataType Errors : Unknow key count. 
- Unknown segment : Unknow value count. 

---

### <a id="DetectedKeys"></a>Detected Keys
This panel display all detected keys in a tree.

![File Infos](savefileexplorer/keys.png)

- A known key is displayed in black.
- A key whose data type is unknown is orange and bold.
- An unknown segment (data the tool does not recognize) is red and bold.
- A parent key takes the colour of its worst child.

You can copy keys into clipboard using the dedicated button.
The combobox let you choose what type of data you want.

---

### <a id="KeyInfos"></a>Key Infos
This panel gives you details about the key.

![File Infos](savefileexplorer/keyinfos.png)

- `IsSubStructureOpening` : true when the key embed a substructure and can be expanded in the tree. The only known opening key is `begin_block`
- `IsStructureClosing` : true when the key end a substructure. The only known closing key is `end_block`
- `IsUnknownSegment` : true when the tool doesn't recognize a segment of data. _**The tool need update to recognize the data.**_
- `IsDataTypeError` : true when the tool doesn't recognize the type of a key. _**The tool need update to recognize the data.**_
- `IsKeyValue` : true when the key is a known value without key. It is the `CRC` at the start of the stash files, and every unknown segment.

---

### <a id="KeyData"></a>Key Data
This panel gives you details about the value.

Nothing special here, it try to display the value in all data types supported by TQ save files.

![File Infos](savefileexplorer/datainfos.png)

---

### <a id="HiddenFeatures"></a>Hidden features
Some behaviors are not obvious. So it's describe here.

- With a key selected, a click on one of these labels changes its value to hexadecimal, and back: the two "Offset" labels, "As a Int", "As a Float" and "As a byte array".
- A right click on a tab offers "Close Tab".

![File Infos](savefileexplorer/hexa.png)
 