# ARZ Explorer

This tool let you explore Titan Quest *.arc and *.arz game files.

It is very useful for TQVaultAE contributors to be able to dig into these files for support and maintenance reasons.

The binary layout of these files is described in [Titan Quest Game File Formats](tq-file-formats.md). For scripted searches and exports, see the [DB CLI (`tqdb`)](DBCLI.md).

---

## Table of contents
* [Executable](#Exec)
* [UI](#UI)
    + [Data Panel](#DataPanel)
    + [Toolbar](#Toolbar)
    + [Search](#Search)
    + [Menu](#Menu)
* [Main TQ Files](#MainFiles)
* [Hidden features](#HiddenFeatures)

---

## <a id="Exec"></a>Executable
ARZ Explorer has its own archive on the [release page](https://github.com/EtienneLamoureux/TQVaultAE/releases): `ARZExplorer-<version>.zip`.

Extract it and run `ARZExplorer.exe`. At the first start, when it does not find the game, it asks for the game folder and remembers it.

Create a shortcut on your desktop if you need to access it frequently.

---

## <a id="UI"></a>UI
Here's a brief overview of the UI.

![ArzExplorer UI](arzexplorer/ui.png)

---

### <a id="DataPanel"></a>Data Panel
This panel adjust it's display to the type of data.

#### Type Record (.dbr)

You can navigate through **multiple files** by following links.

**_The navigation panel load and merge all files dynamicaly._**

![TypeRecord](arzexplorer/typerecord.png)

#### Type Image (.tex)

![TypeRecord](arzexplorer/typeimage.png)

#### Type Text (.txt)

![TypeRecord](arzexplorer/typetext.png)

#### Type Sound (.Wav/.Mp3)

![TypeRecord](arzexplorer/typesound.png)

---

### <a id="Toolbar"></a>Toolbar

The toolbar contains the following buttons:

- **Prev** / **Next** : Go back and forth in the entries already shown
- **Clear History** : Forget that history
- **Caps** : UpperCase/LowerCase the TOC
- **Load Database** : Open the database.arz file
- **Load All Files** : Load all ARZ and ARC files from the game directory
- **Search** : Text box to enter search terms, with "Search Prev" / "Search Next" buttons to navigate through results

![TypeRecord](arzexplorer/toolbar.png)

_The screenshots of this page are older: they show a shorter toolbar and no "Search" menu._

A file dropped on the window is opened.

---

### <a id="Search"></a>Search

The search feature allows you to find records across all loaded files. It searches through:

- Record keys/names
- Text content, for the `.txt` entries already displayed since the tool started
- Record variables (for ARZ files)

**Usage:**
1. Enter search text in the search box
2. Press F3, or click "Search Next" in the toolbar or "Find Next" in the menu, to search forward
3. Press Shift+F3, or click "Search Prev" in the toolbar or "Find Previous" in the menu, to search backward
4. Results wrap around when reaching the end of the list

The status bar shows the search result location.

---

### <a id="Menu"></a>Menu

- File/Open : Select and open an archive (Ctrl+O)
- File/Load All Files : Load all ARZ and ARC files from the game directory
- File/Exit : Exit tool
- Navigation/Previous : Navigate to previous item (F1)
- Navigation/Next : Navigate to next item (F2)
- Edit/Copy Path : Copy path to clipboard
- Edit/Copy .txt : Copy text to clipboard
- Edit/Copy .dbr : Copy DBR record to clipboard
- Edit/Copy .tex : Copy Image to clipboard
- Edit/Copy Sound : Copy Audio to clipboard
- Search/Find Next : Find next occurrence of search text (F3)
- Search/Find Previous : Find previous occurrence of search text (Shift+F3)
- Extract/Selected File : Write the selected record or archive entry to a file you choose (Ctrl+S).
- Extract/All Files : Write every entry of the selected file (the database or one archive) into a folder you choose (Ctrl+A). It can be cancelled.
- View/Hide Zero Values : Enable/Disable the display of the variables of a record holding a value equal to zero (default value). Ticked at start, for the database only.
- View/Caps : UpperCase/LowerCase the TOC.
- Help/About : About window 

---

### <a id="MainFiles"></a>Main TQ Files

Most resources files are located in these subdirectories of TQ install directory (full layout in [Game file layout](tq-file-formats.md#Layout)).

- `./Audio`
- `./Database`
- `./Resources`
- `./Resources/XPack`
- `./Resources/XPack2`
- `./Resources/XPack3`
- `./Resources/XPack4`
- `./Text`

---

### <a id="HiddenFeatures"></a>Hidden features
Some behaviors are not obvious. So it's describe here.

- Double click inside "Path" textbox copy it's content into the clipboard.
- Double click inside "Data Panel - Type Text" copy it's content into the clipboard.
 