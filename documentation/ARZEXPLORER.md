# ARZ Explorer

This tool let you explore Titan Quest *.arc and *.arz game files.

It is very useful for TQVaultAE contributors to be able to dig into these files for support and maintenance reasons.

---

## Table of contents
* [Executable](#Exec)
* [UI](#UI)
    + [Data Panel](#DataPanel)
    + [Menu](#Menu)
* [Main TQ Files](#MainFiles)
* [Hidden features](#HiddenFeatures)

---

## <a id="Exec"></a>Executable
The executable is located in the same directory as TQVault.

Just run `ARZExplorer.exe`.

Create a shortcut on your desktop if you need to access it frequently.

---

## <a id="UI"></a>UI
Here's a brief overview of the UI.

![ArzExplorer UI](arzexplorer/ui.png)

---

### <a id="DataPanel"></a>Data Panel
This panel adjust it's display to the type of data.

#### Type Record (.dbr)

![TypeRecord](arzexplorer/typerecord.png)

#### Type Image (.tex)

![TypeRecord](arzexplorer/typeimage.png)

#### Type Text (.txt)

![TypeRecord](arzexplorer/typetext.png)

---

### <a id="Menu"></a>Menu

- File/Open : Select and open an archive
- File/Exit : Exit tool
- Extract/Selected File : Unpack a selected archive directly to disk without displaying it.
- Extract/All Files : Unpack the whole database directly to disk without displaying it.
- View/Hide Zero Value : Enable/Disable the display of records holding value equal to zero (default value).
- Help/About : About window 

---

### <a id="MainFiles"></a>Main TQ Files

Most resources files are located in these subdirectories of TQ install directory.

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
 