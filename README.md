# TQVaultAE
[![Steam](https://img.shields.io/badge/steam-link-lightgrey.svg)](https://steamcommunity.com/sharedfiles/filedetails/?id=1136716167)
[![Release](https://img.shields.io/badge/stable-3.6.0-blue.svg)](https://github.com/EtienneLamoureux/TQVaultAE/releases)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](https://github.com/EtienneLamoureux/TQVaultAE/blob/master/LICENSE)

TQVaultAE is an external tool for [Titan Quest Anniversary Edition](https://www.thqnordic.com/games/titan-quest) that allows you to store and search your items outside the game.
Works with all expansions!

![TQVaultAE screenshot](https://raw.githubusercontent.com/EtienneLamoureux/TQVaultAE/master/documentation/screenshot.PNG "Hey, I can see my inventory from here!")

![TQVaultAE search screenshot](https://raw.githubusercontent.com/EtienneLamoureux/TQVaultAE/master/documentation/screenshot_search.PNG "Find anything, anywhere!")

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
      - [Translation team](#translation-team)
  * [Disclaimer](#disclaimer)

## Features
- **Infinite bank space**
- Powerful search
- Cheats
    - Items
        - Extract relic/charm from items at no cost, keeping both
        - Modify the relic/charm/artefact completion bonus
        - Complete relic/charm from a single piece
        - Craft an artifact from its recipe
        - Create missing set pieces
        - Duplicate any item
    - Characters
        - Redisribute attribute points
        - Unlock difficulties
        - Level up
- QOL
    - Bulk item transfer (<kbd>CTRL</kbd>+click, right-click)
    - Combine stacks (potions, relics and charms) by dropping them onto each other
    - Split potion stacks apart
    - Keyboard shortcuts
        - <kbd>CTRL</kbd>+<kbd>F</kbd>  : Open search form
        - <kbd>CTRL</kbd>+<kbd>+</kbd> : Increase vault size
        - <kbd>CTRL</kbd>+<kbd>-</kbd> : Reduce vault size
        - <kbd>CTRL</kbd>+<kbd>Home</kbd> : Default vault size
        - <kbd>CTRL</kbd>+<kbd>A</kbd> : Select all items in the vault
        - <kbd>CTRL</kbd>+<kbd>D</kbd> : De-select all selected items
        - <kbd>BACKSPACE</kbd> : Deletes currently hightlighted item
        - <kbd>CTRL</kbd>+click : Activate multi selection
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
- The cheats (To enable/disable these options, see the F.A.Q. below)

## Troubleshooting and F.A.Q.
**Q. Does TQVaultAE modify my items? The stats I see are not the same as the ones ingame.**

*A. No, unless you specifically use the cheats, TQVaultAE doesn't alter items in any way. 
The difference you see is simply due to the way stats are generated in Titan Quest: each item has base stats and a unique seed that modifies those stats. 
TQVaultAE only displays the base stats (and not the modifications due to the RNG).*

**Q. Can I use TQVaultAE while playing the game?**

*A. No, using TQVaultAE while running the game may lead to loss of progress or items. Best practice is to close the game before using TQVaultAE.*

**Q. What happened to my items, I transferred items to my character and they are not there in game?**

*A. If you are using the Steam version of the game, make sure Steam Cloud synchronization is disabled as it will overwrite local game saves modified by TQVaultAE with cloud older saves.*

**Q. How to enable/disable the cheats (character edition, item edition, item copy)?**

*A. Follow these steps:*
1. *Navigate the the installation folder of TQVaultAE*
2. *Open `TQVaultAE.exe.config` in a text editor (i.e. notepad, **not Microsoft Word**)*
3. *Find the key `AllowCheats` and change the value to `True` or `False`*
    - *`True` will allow you to toggle the cheats individually in the configuration menu*
    - *`False` will disable the cheats completely and make it impossible to enable them in the configuration menu*

**Q. Can TQVaultAE use my old vault files?**

*A. Yes, TQVaultAE is compatible with the legacy TQvault vault files.*

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
- *Look up if your problem is featured in [our previously answered questions](https://github.com/EtienneLamoureux/TQVaultAE/issues?q=+is%3Aissue+label%3Aquestion+)*
- *Look up if your problem is featured in [TQVault's documentation](https://github.com/EtienneLamoureux/TQVaultAE/blob/master/documentation/TQVault%20common%20issues.pdf)*
- *Create an issue in [our issue tracking board](https://github.com/EtienneLamoureux/TQVaultAE/issues)*

## Contributors
This project could not go on without the continued volunteer contributions of the Titan Quest community. If you're thinking about contributing, please read our [contributing guidelines](/CONTRIBUTING.md).

### TQVaultAE
- [Open-source contributors](https://github.com/EtienneLamoureux/TQVaultAE/graphs/contributors)

### TQVault
- Brandon "bman654" Wallace, *original author*
- saydc, *item stats*
- Jesse "VillageIdiot/EJFudd" Calhoun, *item stats & ARZExplorer util*
- AvunaOs, *new UI*

#### Translation team
- FOE, *german*
- Jean, *French*
- Vifarc, *French*
- Cygi, *Polish*
- Xelat, *Russian*
- Kurrus, *Spanish*

## Disclaimer
Titan Quest, THQ and their respective logos are trademarks and/or registered trademarks of THQ Nordic AB. This non-commercial project is in no way associated with THQ Nordic AB.
