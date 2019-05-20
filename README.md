# TQVaultAE
[![Steam](https://img.shields.io/badge/steam-link-lightgrey.svg)](https://steamcommunity.com/sharedfiles/filedetails/?id=1136716167)
[![Release](https://img.shields.io/badge/stable-2.6.0-blue.svg)](https://github.com/EtienneLamoureux/TQVaultAE/releases)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](https://github.com/EtienneLamoureux/TQVaultAE/blob/master/LICENSE)

TQVaultAE is an external tool for [Titan Quest Anniversary Edition](https://www.thqnordic.com/games/titan-quest) that allows you to store and search your items outside the game.
Works with all expansions!

![TitanQuestAE screenshot](https://raw.githubusercontent.com/EtienneLamoureux/TQVaultAE/master/documentation/screenshot.PNG "Hey, I can see my inventory from here!")

## Table of contents
- [TQVaultAE](#tqvaultae)
  * [Table of contents](#table-of-contents)
  * [Features](#features)
  * [Installation](#installation)
    + [Installer](#installer)
    + [DIY Archive](#diy-archive)
    + [Configuration](#configuration)
  * [Troubleshooting and F.A.Q.](#troubleshooting-and-faq)
  * [Contributors](#contributors)
    + [TQVaultAE](#tqvaultae-1)
    + [TQVault](#tqvault)
    + [Translation team](#translation-team)

## Features
- **Infinite bank space**
- Powerful search
- Cheats
  - Extract relic/charm from items at no cost, keeping both
  - Modify the relic/charm completion bonus
  - Complete relic/charm from a single piece
  - Craft an artifact from its recipe
  - Create missing set pieces
  - Duplicate any item
- QOL
  - Bulk item transfer (CTRL-click, right-click)
  - Combine stacks (potions, relics and charms) by dropping them onto each other
  - Split potion stacks apart
- Character backups
  - If an error occurs, backups are located at `My Documents\My Games\Titan Quest\TQVaultData\Backup`

## Installation
### Installer
1. Navigate to the [release page](https://github.com/EtienneLamoureux/TQVaultAE/releases)
2. Download the latest release's `.exe` file.
   - **Please note:** If you opt for a pre-release, be aware that they are alpha builds.
3. Double-click the `.exe`.
4. Navigate to the folder where you installed TQVaultAE. Double-click `TQVaultAE.exe`
5. Enjoy!

### DIY Archive
1. Navigate to the [release page](https://github.com/EtienneLamoureux/TQVaultAE/releases)
2. Download the latest release's `.zip` file.
   - **Please note:** If you opt for a pre-release, be aware that they are alpha builds.
3. Extract the content of the archive on your computer.
4. Navigate to the folder where you extracted the artefacts. Double-click `TQVaultAE.exe`
5. Enjoy!

### Configuration
The "Configure" button (top-left) opens up the configuration menu. That's where you can change:
- The language used by the application
- The paths where the vault files are located
- The paths where the game files are located
- The cheats (listed under the "item edition" options)

## Troubleshooting and F.A.Q.
**Q. Can TQVaultAE use my old vault files?**

*A. Yes, TQVaultAE is compatible with the legacy TQvault vault files.*

**Q. Does TQVaultAE modify my items? The stats I see are not the same as the ones ingame.**

*A. No, unless you specifically use the cheats, TQVaultAE doesn't alter items in any way. 
The difference you see is simply due to the way stats are generated in Titan Quest: each item has base stats and a unique seed that modifies those stats. 
TQVaultAE only displays the base stats (and not the modifications due to the RNG).*

**Q. After updating from an older version of TQVault (2.18 or older), the new program won't start.**

*A. Follow these steps (See [this comment](https://github.com/EtienneLamoureux/TQVaultAE/issues/85#issuecomment-447326792) for the original solution):*
1. *Press the `Windows` key*
2. *Type `cmd`*
3. *Right-click the first option and run as administrator*
4. *Type `regsvr32 /u VXPLib.dll`*
5. *Hit enter*

**Q. Error Loading Resources. This may be caused by a bad language or game path setting.**

*A. Follow these steps:*
1. *Navigate the the installation folder of TQVaultAE*
2. *Open `TQVaultAE.exe.config` in a text editor (i.e. notepad, **not Microsoft Word**)*
3. *Replace the following sections:*
```xml
<setting name="AutoDetectGamePath" serializeAs="String">
    <value>True</value>
</setting>
...
<setting name="TQITPath" serializeAs="String">
    <value />
</setting>
<setting name="TQPath" serializeAs="String">
    <value />
</setting>
```
*by (replace the path to the correct one for your computer)*
```xml
<setting name="AutoDetectGamePath" serializeAs="String">
    <value>False</value>
</setting>
...
<setting name="TQITPath" serializeAs="String">
    <value>C:\examplePath\Titan Quest Anniversary Edition</value>
</setting>
<setting name="TQPath" serializeAs="String">
    <value>C:\examplePath\Titan Quest Anniversary Edition</value>
</setting>
```
4. *Open TQVaultAE*
   - *You might be greeted with a warning dialog about the vault path not being set. Click OK.*
5. *Open the configuration menu by clicking the top-left button*
6. *Validate the vault path and the game paths shown*
7. *Click OK to close the configuration menu*

**Q. I have this game as a stand-alone (i.e. not through Steam or GOG). How can I make TQVaultAE work?**

*A. See the answer to "**Error Loading Resources. This may be caused by a bad language or game path setting.**" above*

**Q. Does TQVaultAE work with the Immortal Throne expansion?**

*A. Yes*

**Q. Does TQVaultAE work with the Ragnarok expansion?**

*A. Yes*

**Q. Does TQVaultAE work with the Atlantis expansion?**

*A. Yes*

**Q. Can I still earn achievements while using TQVaultAE?**

*A. Yes*

**Q. I have a problem not listed here. What can I do?**

*A. There are several things you can do:*
- *Close TQVaultAE and open it up again. It may fix your problem*
- *Look up if your problem is featured in [TQVault's documentation](https://github.com/EtienneLamoureux/TQVaultAE/blob/master/documentation/TQVault%20common%20issues.pdf)*
- *Create an issue in [our issue tracking board](https://github.com/EtienneLamoureux/TQVaultAE/issues)*

## Contributors
This project could not go on without the continued contributions of the Titan Quest community.

### TQVaultAE
- Malgardian (a.k.a. Spectre), *Anniversary Edition port*
- EPinter, *XL vault, bug fixes, general improvements*
- NorthFury, *search improvements, bug fixes, general improvements*
- EtienneLamoureux, *maintenance, bug fixes, general improvements*
- [Open-source contributors](https://github.com/EtienneLamoureux/TQVaultAE/graphs/contributors)

### TQVault
- Brandon "bman654" Wallace, *original author*
- Jesse "saydc" Calhoun, *item stats*
- VillageIdiot, *ARZExplorer util*
- AvunaOs, *new UI*

### Translation team
- FOE, *german*
- Jean, *French*
- Vifarc, *French*
- Cygi, *Polish*
- Xelat, *Russian*
- Kurrus, *Spanish*
