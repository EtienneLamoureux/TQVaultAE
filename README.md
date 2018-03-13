# TQVaultAE
An external tool that provides extra bank space for Titan Quest Anniversary Edition. Also contains some item edition features.

![TitanQuestAE screenshot](https://raw.githubusercontent.com/EtienneLamoureux/TQVaultAE/master/documentation/screenshot.PNG "Hey, I can see my inventory from here!")

## Table of contents
- [TQVaultAE](#tqvaultae)
  * [Table of contents](#table-of-contents)
  * [Features](#features)
  * [Installation](#installation)
    + [Installer](#installer)
    + [DIY Archive](#diy-archive)
    + [Configuration](#configuration)
    + [Troubleshooting and F.A.Q.](#troubleshooting-and-faq)
  * [Contributors](#contributors)
    + [TQVaultAE](#tqvaultae-1)
    + [TQVault](#tqvault)
    + [Translation team](#translation-team)

## Features
- **Infinite bank space**
- Search across every vault, character and bag you have
- Item edition
  - Extract relic/charm from items at no cost, keeping both
  - Modify the relic/charm completion bonus
  - Create missing set pieces
- Item management
  - Move items in bulk by CTRL-clicking them, then using the bulk-move options in the right-click menu.
  - Combine potion stacks, relics and charms by dropping them onto each other.
  - Split potion stacks apart.
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
The "Configure" button opens up the configuration menu. That's where you can change:
- The language used by the application
- The paths where the vault files are located
- The paths where the game files are located
- The "item edition" options

### Troubleshooting and F.A.Q.
**Q. Can TQVaultAE use my old vault files?**
:  *A. Yes, TQVaultAE is compatible with the legacy TQvault vault files.*

**Q. Does TQVaultAE modify my items? The stats I see are not the same as the ones ingame.**
:  *A. No, unless you specifically use the "item edition" options, TQVaultAE doesn't alter items in any way. The difference you see is simply due to the way stats are generated in Titan Quest: each item has base stats and a unique seed that modifies those stats. TQVaultAE only displays the base stats (and not the modifications due to the RNG).*

## Contributors
This project could not go on without the continued contributions of the Titan Quest community.

### TQVaultAE
- Malgardian (a.k.a. Spectre), *Anniversary Edition port*
- EPinter, *bug fixes, UI rework, new big vault, general improvements*
- NorthFury, *bug fixes, search features, general improvements*
- [Open source contributors](https://github.com/EtienneLamoureux/TQVaultAE/graphs/contributors)

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
