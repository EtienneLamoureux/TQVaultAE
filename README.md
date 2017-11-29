# TQVaultAE
An external tool for Titan Quest Anniversary Edition that allows you to trade items between your characters or even store them offline in private vaults.

![TitanQuestAE screenshot](https://raw.githubusercontent.com/EtienneLamoureux/TQVaultAE/master/documentation/screenshot.PNG "Hey, I can see my inventory from here!")

## Table of contents
- [TQVaultAE](#tqvaultae)
  * [Features](#features)
  * [Installation](#installation)
    + [Configuration](#configuration)
    + [Troubleshooting and F.A.Q.](#troubleshooting-and-faq)
  * [Contributors](#contributors)
    + [Translation team](#translation-team)

## Features
- View Item Stats!
- View the inventory, stash and equipment of all your characters using in-game graphics
- Re-arrange the inventory of all your characters
- Search your inventory and vaults for specific items.
- Move items from one character to another
- Move items from your character into a vault, freeing up valuable inventory space
- Create an unlimited number of vaults with any name you want.  By default, all vaults are stored in `My Documents\My Games\Titan Quest\TQVaultData`
- Combine potion stacks, relics and charms by dropping them onto each other.
- Split potion stacks apart.
- Extract relics from items so you can use them in a different item, or use a different relic with your item
- Displays item names using the game language settings
- Displays custom items from custom maps.
- Makes backups of all files before modifying them.  If you encounter any errors, you can find the backups in `My Documents\My Games\Titan Quest\TQVaultData\Backup`
- Ability to correctly extract database.arz file (TQ Toolset currently does not extract array records correctly and does not let you extract entire database)

## Installation
1. Navigate to the [release page](https://github.com/NorthFury/TQVaultAE/releases)
2. Download the latest release
   - If you want an "installer", choose the `.exe` file.
   - To extract the artefacts yourself, choose the `.zip` file.
   - **Please note:** If you opt for a pre-release, be aware that they are alpha builds.
3. Extract the content of the archive on your computer
   - If you have downloaded the `.exe`, simply double-click it.
4. Navigate to the folder where you extracted the artefacts. Double-click `TQVaultAE.exe`
5. Enjoy!

### Configuration
The "Configure" button opens up the configuration menu. That's where you can change:
- The language used by the application
- The paths where the vault files are located
- The paths where the game files are located
- The cheating options

### Troubleshooting and F.A.Q.
**Q. Can TQVaultAE use my old vault files?**
:  *A. Yes, TQVaultAE is compatible with the legacy TQvault vault files.*

**Q. Does TQVaultAE modify my items? The stats I see are not the same as the ones ingame.**
:  *A. No, unless you specifically use the cheating options, TQVaultAE doesn't alter items in any way. The difference you see is simply due to the way stats are generated in Titan Quest: each item has base stats and a unique seed that modifies those stats. TQVaultAE only displays the base stats (and not the modifications due to the RNG).*

## Contributors
- Brandon "bman654" Wallace, *original author*
- Jesse "saydc" Calhoun, *item stats*
- VillageIdiot, *ARZExplorer util*
- AvunaOs, *new UI*
- Spectre, *Anniversary Edition port*
- Etienne "TheHoardingRitualist" Lamoureux, *various*

### Translation team
- FOE, *german*
- Jean, *French*
- Vifarc, *French*
- Cygi, *Polish*
- Xelat, *Russian*
- Kurrus, *Spanish*
