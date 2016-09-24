TQVAULT by bman654 / saydc / VillageIdiot
Version 2.3.0.4 - 3/26/2010

This program allows you to trade items between your characters or even store them offline in private vaults.

Features
--------
- View Item Stats! -- Special thanks for saydc for his contribution to this.
- View the inventory, stash and equipment of all your characters using in-game graphics
- Re-arrange the inventory of all your characters
- Search your inventory and vaults for specific items.
- Move items from one character to another
- Move items from your character into a vault, freeing up valuable inventory space
- Create an unlimited number of vaults with any name you want.  All vaults are stored in My Documents\My Games\Titan Quest\TQVaultData
- Combine potion stacks, relics and charms by dropping them onto each other.
- Split potion stacks apart.
- Extract relics from items so you can use them in a different item, or use a different relic with your item
- Displays item names using the game language settings
- Displays custom items from custom maps.
- Makes backups of all files before modifying them.  If you encounter any errors, you can find the backups in My Documents\My Games\Titan Quest\TQVaultData\Backup
- Ability to correctly extract database.arz file (TQ Toolset currently does not extract array records correctly and does not let you extract entire database)
- Package also includes TQVaultMon, a utility to prevent Titan Quest from detecting the inventory modifications.


Installation instructions
-------------------------
1. Unzip all the files anywhere you like
2. Run setup.exe

*** REQUIRES MICROSOFT .NET FRAMEWORK 2.0 TO BE INSTALLED ON YOUR COMPUTER ***
If you get an error trying to install TQVault, then please install the framework.

*** REQUIRES THE NEW C++ REDISTRIBUTABLE FROM SECURITY UPDATE (MS09-035) ***
It can be downloaded from the following link:
http://www.microsoft.com/downloads/details.aspx?familyid=766a6af7-ec73-40ff-b072-9112bab119c2&displaylang=en
TQVault uses the x86 version.


Version History
---------------
2.3.0.4  3/26/2010
---
- See changelog.txt for full history.
- Added native language support.
- Updated and enhanced searching.

2.2.2.2  8/22/2009
---
- See changelog.txt for full history.
- Added Search capability.
- Added ability to switch out the player panel with another vault.
- Added ability to select multiple items for moving and deleting.
- Added bag merging.
- Added auto updating.

2.2  6/23/2009
----
- See changelog.txt for full history.
- Complete most of the item stat decoding.
- Added panels for player stash and transfer stash with IT.
- Added equipment panel.
- Added Custom Map support.
- Added context menus.

2.1  3/15/2007
----
- Added item stats!  saydc did the initial work and wrote the core algorithms to get the information and do some calculations.  I wrote the code to convert the stats to in-game descriptions with full language support
- Currently the stats and level requirements are not quite right.  They are close but not exact to the in-game values.
- Not all stats have been mapped.  If you see something in purple, that means it has not been mapped properly.  Some I know about and some I don't.
- Modified bag tooltip to display contents color-coded correctly.
- Wrote an installer.  There are some technical changes that made this a requirement to install corectly.

2.0   3/9/2007
----
- Updated to work with Titan Quest Immortal Throne and also Titan Quest 1.30
- Removed requirement that program be installed in Titan Quest folder.  You can now install it anywhere.
- Note: TQVaultMon is not needed if you run the new Titan Quest Immortal Throne executable.  TQVault will not offer to start VaultMon if it detects TQIT installed.


1.620 9/25/2006
----
- Updated TQVaultMon to support TQ V1.20

1.615 8/29/2006
----
- Updated TQVaultMon to support TQ V1.15

1.6 8/11/2006
----
- Now reads item data directly from database.arz file.  (I got bored waiting for Dark Messiah to download [which I DO NOT like afterall] so I decoded the file :-)
- Added Extract database.arz button.  This is only useful for mod makers.  Currently the toolset does not offer a way to pull out all the records at once.  And also it currently has a bug which messes up arrays when you extract records, causing problems with skills and other stuff in your mods.  This Extract option suffers from neither of those problems :-).  Just extract and then copy the records you want over into your mod folder.
- Removed TQVaultData.dat from the zip archive as it is not needed anymore.  You can delete TQVaultData.dat from your computer if you wish.

1.5 8/7/2006
----
- Improved error handling to display better errors for the unhandled exceptions a few people are reporting
- Added ability to start TQVaultData in non-default locations.  To do this, put TQVaultData wherever you want (like on a network drive?).  Place a shortcut called TQVaultData inside "My Documents\My Games\Titan Quest" that points to the correct location for the data.  For example, move TQVaultData to "\\myfriendspc\games\TQVaultData"  Make a shortcut called "My Documents\My Games\Titan Quest\TQVaultData" and have this shortcut point to "\\myfriendspc\games\TQVaultData".  TQVault will honor the shortcut.
- Added command-line argument: TQVault [charactername] so that you can start TQVault data right on the character in question.  Mainly useful for integration with TQ Defiler.
Examples:

TQVault Terminator
TQVault "Lady Dazzler"
TQVault "Im gonna get you sucka!"

1.4 8/3/2006
----
- Fixed rare problem where item graphics would not load on certain non-US Windows cultural settings

1.3 8/1/2006
----
- Fixed TQVault hanging when you exit if the Process() library throws an exception.

1.2 7/31/2006
----
- Do not display prefixes and suffixes for relics.  Someone had a relic with a partial prefix/suffix
- Displays completion bonus tag for relics.  Not the actual stats but the programming tag to give you an idea of what the bonus is.
- Added Start TQVault button to TQVaultMon for one-stop TQ shopping.

1.1 7/31/2006
----
- Fixed TQVaultMon support for TQ versions 1.01 and 1.08
- Modified TQVaultMon to look for TQStart.bat and if it exists, use it to start TQ instead of directly running the executable.
- Fixed bag popups so they go away properly when you move the mouse off the bag.

1.00 7/31/2006
- Initial Release


----

Full Source code is included.
You are granted the right to make use of this source code in any non-commercial application.  This code may not be used in any commercial applications.

Notes
-----
I made use of the following 3rd party libraries:
FreeImage - image library I used to read DDS formatted image data
zlib_100 - a .NET version of the zlib library
CQuickFill & QuickFill.NET - I heavily modified this quickfill algorithm for use in my own algorithm to reconstruct the transparency data in the TQ images.
