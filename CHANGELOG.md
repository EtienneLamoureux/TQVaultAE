# Changelog
## 3.0.0
### Features
- #107 Support the new [Atlantis](https://titanquestgame.com/titan-quest-atlantis-release/) expansion

### Bugs
- Fix some localization issues

## 2.6.0
### Features
- Create an artifact from its recipe
- Load embedded font
- Add style to combobox

### Bugs
- Fix custom map label
- #83 Display the icon for the "Power of Nerthus" relics
- #82 Correctly show status of "Brennus' Striped Pants"
- #99 Correctly handle neutral cultures

## 2.5.6
### Features
- Always center window on startup
- Improve title bar scaling

### Bugs
- #75 Fix corruption during equipment background image scaling

## 2.5.5
### Bugs
- #62 Fix ui scale bug introduced in 2.5.4

## 2.5.4
### Bugs
- Fix attribute inconsistency when viewing similar items
- #21 Fix UI corruption on low resolution displays

## 2.5.3
### Features
- #48 Added ini file support for gamepath, mod and disable editing/copy features (replaces command line arguments)

### Bugs
- Fixed issue where one couldn't search if a character didn't have an extra inventory space tab
- Fixed crash while saving when the backup directory delete fails

## 2.5.2
### Features
- Added support for throwing weapons
- Vault size increased to 18x20
- Reworked UI
- RemoveRelic disabled when editing is off

## 2.5.1
### Bugs
- Fixed various items description issues
### Other
- Optimized images file size

## 2.5.0
### Features
- Added support for Ragnarök items
  - #29 Make TQVault recognize Ragnarök items
- #22 Improved search and search results display
  - Added "required level" column
  - Removed "stack number" column
  - Fixed some tooltips
- #25 Added an option to make equipped items read-only
  - Items equipped to a character cannot be interacted with by default
### Bugs
- Fixed automatic path detection when using Steam
- #7 Fixed interface flickering when dragging an item across
- #30 Fixed an issue where switching between characters with different number of bags caused an error
### Other
- Changed the font back to Albertus MT
- #8 Disabled the old UI entierly
- #11 Removed player filters
  - All characters are "Immortal Throne" characters in the Aniversary Edition

## 2.4.0
### Features
- Added support for the Anniversary Edition (steam version)
  - #1 Make the whole stash accessible from the UI
### Other
- Make the new UI the default
- Remove TQVaultMon

## 2.3.1.4
- Enable new UI.
  - Defaults to Off in this release.
  - Added new key for enabling and disabling the new UI.
  - Change ScalingButton so that it can have a graphic in addition to the standard Windows styles.
  - Absract New UI into VaultForm.  Adds custom border, system menu, custom title bar, etc.
  - Update existing forms to use the new UI.
- Change startup logic so that MainForm does all of the loading.
  - Move the progress bar into Splash Screen (Form1).
  - Splash screen now fades out and Main Form fades in when new UI is enabled.  Can be disabled by manually changing the fade settings.
  - Combine loading of all character files and resources.
- Change the results dialog.
  - Now shows a tooltip for the highlighed item.
  - Does not close when an item is selected so that multiple items can now be reviewed.
  - The item text now matches the item quality.
- Fixed border of the text panel.  Bottom was cut off.
- Fixed Move To -> Player panel.  
  - Was using the size for secondary bags to find the space on automove so it would fail if the free space was outside of the size of the player secondary bags.
- Increased font size for bag labels.

## 2.3.1.3
- Fixed issue with double entries in the loot table for certain items (Lupine Claw, Turtle Shell, etc.).
- Fixed relic bonus not being carried into removed relics.
- Fixed issue where completed relics and charms without a bonus did not show 'Change Completion Bonus' context menu item.

## 2.3.1.2
- Fixed skewing of equipment background when no player is loaded.
- Added more error handling and logging in the file parsing routines.
- Added message with option to turn off loading all files if it did not complete on the last run.

## 2.3.1.1
- Exception when dragging an item outside of the parent sack.
- Only 1 of the additional player bags shows up.

## 2.3.1.0
- Refactor Code:
  - Ported to VS2010.
  - Simplify code and remove redundancy.  Also make things more 'C# like'.
  - Use LINQ for internal data queries.
  - Switch to Generic Collections.
  - Much better FxCop and StyleCop compliance.
  - Split project moving the data handling and decoding into a separate TQVaultData dll.
  - Moved resource loading into individual classes instead of in the database class.
    - This was left over from when the resources were stashed in zlib.
  - Moved expression-eval to its own project.
  - Changed to use .NET compression and removed dependency on zlib.
  - Merged ArzExplorer into solution and changed to use TQVaultData.
  - Removed Database extraction button from main form.
- Added support for DDS transparency (Da_FileServer).
  - Removed dependency on QuickFill.NET and Visual C++ runtime.
- Switched from FreeImage to DevIL/Tao.DevIl since there is an issue with Freeimage converting DDS files to bitmaps that are not DWORD aligned.
  - Fixed TEX to DDS header calculation so it is more compliant to the DDS spec.
- Added main window resizing.
  - Can use the maximize button.
  - Can grab the edges of the window to manually resize.
  - Can use CTRL+ and CTRL- to incrementally resize.
  - Can use CTRL+Home to reset scaling to 1.0.
  - Saves scaling value in config file and will scale on load if not set to 1.0.
- Increased Stash panel size to 10x15 to accomodate increased stash size.
- Resized Stash tabs to accomodate 2 lines of text more comfortably.
- Increased number of characters allowed in Vault Maintenance text box to 256.
- Fixed flickering (flicker fix from Da_FileServer).
- Fixed crashing with VS2008 (Da_FileServer).
- Fixed issue where set name tag would show up in the create set items context menu.
- Fix issue where completed relics and charms would not be saved if TQVault's completion function was the only action performed on the vault before closing.
- Fixed error/crash with update check if network is not available.
- Change Update program to show progress bar instead of message queue.

## 2.3.0.4
- Port the source to C#:
  - Clean up the code a little bit getting rid of some unused items.
  - QuickFill is now a separate DLL (and project which is included with the source).
  - Moved the image resources out of zlib and into the main project resources.
  - Settings are also now part of the resources and are stored in the user's application data area.
  - Strings are also part of the resources to support localization.
- Rewrote Search code:
  - Fixes issue with exception on new characters that have not entered the game.
  - Added item quality to search results.
  - Added ability to filter by quality when searching by type.  Use the ampersand (&).
    - For example to search for rare swords use: @sword&rare
  - Added ability to search by item type.  Prefix the string with at (@) to search by type.
    - For example: @sword will search for all swords.
- Added some UI Enhancements:
  - The UI will now translate to French, Spanish, German and Russian.
    - Thanks to Jean, Vifarc, FOE and Xelat for providing translations.
    - Spanish was done by Google to test localization.  Probably very bad and needs to be redone.
  - UI Language will use the game language setting if not autodetecting.
    - If autodetecting the OS language is used.
    - The UILanguage key in the config file will override both values and set the UI language.
  - Game Language dropdown should now display in the native language of the installed .NET framework.
  - Moved contact information to an About box.
  - Added support for scaling.
    - Now shows correctly with 120 DPI or 144 DPI fonts.
  - Changed button text to dynamically resize if it is too large to fit in the button bounding box.
  - Changed icon.
    - The old icon looked corrupted on most displays.
    - Now uses the in in game model of the Majestic Chest.
- Added warning for the empty bag function.
- Added that turning off copying also supresses bag copying in the vault.
- Improve Vault Maintenance a little:
  - Can now use maintenance functions on the Main Vault.
  - Selecting from the source dropdown now automatically highlights and sets focus to the destination.
- Fix AutoUpdate:
  - Fixed issue where update would trigger if update server could not be contacted.
  - Force updater always pull version info from the server.  Previously it would pull from the cache.
  - Fixed issue with Update dialog closing during full setup download.
  - Added support for subdirectories in ARC files.  Done for full setup updates.
- Fix strange requirement values.
  - Changed numeric conversions to now use the Invariant culture since internally everything is in US English format.
- Fixed issue where stash CRC would overflow when attempting to inject the calculated CRC back into the save stream.
- Fixed a bug where the search grid would crash if the header was double clicked.
- Fix issue that IT was sometimes not detected
  - Add searching for any file that matches TQIT*.exe to determine if IT is installed.
- Fix decoding of extended characters in the database. (Thanks Vorbis)

## 2.2.2.2:
- Now requires [new C++ redistributable](http://www.microsoft.com/downloads/details.aspx?familyid=766a6af7-ec73-40ff-b072-9112bab119c2&displaylang=en) from security update (MS09-035):
- Changed versioning for auto updating.  Build is no longer included and BETA build number is no longer in title.
  - Version is now <Major>.<Minor>.<Sub_Minor>.<Revision>
- Added abililty to switch upper panel between player and an additional vault.
- Added basic search based on item name as it shows in the text box below the player pane.
  - Shows a results dialog where an item can be double-clicked and the container of that item will be loaded.
  - Search results highlight (select) the item chosen from the results list.
  - Pre-loads all of the vaults, stashes and character files.  This increases startup time and can be turned
    - off in the configuration UI.  Searching will only search the files resident in memory.
  - Can also be Activated by hitting CTRL-F.
- Added Move To... item for stash.
- Added Merge Bag context menu item which combines sacks.
- Added mousewheel scrolling of vault list.
- Added CTRL-Clicking for multiple item selection.
  - Can be used to delete and move.
  - Added context menu item for clearing selection.
  - Selection will be cleared if the vault bag is switched or if an item on another panel is selected.
  - CTRL-A will select all items in the active (the one under the mouse) panel.
  - CTRL-D will deselect all selected items.
- Added ability to bypass warning messages.
  - Can be set in configuration UI.  Defaults to false.
  - Will automatically bypass item deletion, relic/charm removal and items in the trash confirmation messages.
- Added auto update functionality.  Thanks to SoulSeekkor for providing the update program and hosting the files. 
  - Checks for new version on startup and prompts if an update is available.
  - Configuration UI can turn off version check at startup.
  - Configuration UI also has a button to manually check for updates.
  - Will download and run the full setup if minimum version is not met.
  - Will patch if minimum is met.
  - Has the ability to update the patch program.
  - Shows the changes in the new version.
- Added sack numbers to trash panel and player sack panel.
- Implemented Th's tooltip display fix.  The old behavior can be turned back on in the config file.
- Added exception text to "Error Loading Resources" message.  This should help in debugging in the future.
- Added manual screen refresh (F5).
- Fixed shield base attribute sorting.
- Fixed custom map text showing on top of autosort button.
- Fixed 2H logic since some axes and maces were detected as 2H.
- Fixed config file update logic.
- Fixed background image in main dialog.
- Fixed a bug in language detection when culture ID was not found in the System Neutral Culture List it would abort language detection.
- Fixed null reference error when vault data folder is empty.
- Fixed background highlighting when picking up and then putting down the same item in the Equipment panel.
- Fixed background highlighting and shadow images for 2H weapons when right click cancelling a picked up piece of equipment.
- Fixed display issue with Stash panel title without character loaded.
- Fixed text formatting of TQVaultMon prompt.
- Fixed issue displaying unknown items.  Before, if the base item record was not found the item would be invisible.
  - Now displays with default bitmap (Orange ?).
  - Shows record ID as the name.
  - Decodes whatever stats it can find the records for.
- Fixed Move to selections for player main panel and player sack panels.
- Fixed issue with D2D exe file not getting detected for IT determination.
- Replaced the New Vault dialog with a vault maintenance dialog and added the ability to copy, rename and delete existing vaults.
- Cleaned up UI a little bit and harmonized dialogs.
- Cleaned up context menu code a little bit.  Now uses an arraylist instead of static arrays with a bunch of conditions.

## Before versionning shift
### v2.20:
- Enabled picking up and dropping of items on the equipment panel.
- Added my e-mail address to the main dialog.
- Now reads in most of the text DB from the IT files and uses the newer entries if available.
  - Previously only the x-prefix files were loaded.
- Single item stacks no longer show the number of the items in the stack.
- Added remaining text color codes.
- Added numeric labels to vault bags
- Added Empty Bag, Copy Bag and Move Bag functions to vault.
- Added context menu for moving items from stash.
- Added keyboard shortcuts for deleting 'BkSpc', copying 'c' items and dropping 'd' (move to trash)..
- Added context menu item to Move item to a different sack.
- Added checking the IT Resources folder for updated arc files when searching for a resource.  Fixes Lil' Lued bitmap.
- Added loading of more txt files.  Fixes Lil' Lued Artifact name and summon.  Should also fix allskins bottles.
- Rearranged Item attributes to match the order within Art Manager.  Also added a few missing ones.
- Added grouping of multiple Damage Qualifier tags.
- Implement skill levels for granted skills.
- Added Autosort button.
- Clean up source a little bit.  Get rid of remnants of changes that have been incorporated
- Implement debugging at runtime instead of compile time.  Turn on the commented debugging lines.
  - Use DebugEnabled key to turn debugging on and off.  The level keys have no effect with this turned off.
  - ARCFileDebugLevel - Used for debugging the decoding of the arc files.
  - DatabaseDebugLevel - Used for debugging of decoding and loading the databases.  Resource loading issues should be shown here.
  - ItemDebugLevel - Used debugging item stat decoding.
  - ItemAttributesDebugLevel - Used for decoding the format specs of the items.
  - The levels range from 0 to 3.
    - 0 - No logging except for some error messages.
    - 1 - Basic logging showing entering and exiting of functions.
    - 2 - Includes internals of functions but not loops.  This is the most common setting.
    - 3 - Includes the internals of loops.  This can slow down the program especially when used with the ARCFile.
- Fixed decoding for:
  - RetaliationStun
  - quest
  - cannotPickUpMultiple
- Fixed display of base vitality damage.
- Try to fix the overflow in the dynamic level calculation.  (Hopefully once and for all.)
- Fixed issue with saving character after dropping item from inventory to vault.
- Fix version checking of config file.

### v2.18:
- Added Player Stash and Transfer Stash
- Added Equipment panel (Read Only)
- Added Custom Map support. Will require a restart of TQVault every time the custom map settings are changed.
- Added new context menu item 'Duplicate' which preserves the item seed on copying.
- Added new context menu item 'Split' for splitting stackable items.
- Added support for color tags within text strings.
- Added decoding for:
  - offensiveTotalDamageReductionAbsolute
  - offensiveBaseLife
  - augmented buff skills
- Dynamically calculate transfer stash position based on size.
  - This should fix issues displaying hacked stashes.
- Add command line arguments to enable mods. Use /mod:<modname> to automatically load a mod.
  - Can also use <charactername><Immortal Throne><Custom Map> to auto load a custom map character.
  - For example:
```
To autoload the custom map character Sparticus with Lilith you use a shortcut like this:
TQVault.exe "Sparticus<Immortal Throne><Custom Map>" /mod:Lilith
To start with the Lilith map and load the last character you use a shortcut like this:
TQVault.exe /mod:Lilith
```
- Changed targets to x86 and Win32
- Changed the method for item type determination. It now uses the class in the database record instead of the record path.
- Changed scanf function to use TryParse function to handle the format exceptions that are appearing on occasion.
- Fixed issue with inability to change Kingslayer's completion bonus.
- Fixed issue where if records were in both TQ and IT DBs only the TQ version was loaded. Changed so that the IT DB has priority.
  - This fixes the issue with changing the completion bonus on the Hermes' Sandal Relic.
- Fixed (hopefully this time) an overflow issue with the dynamic requirements calculation.
- Fixed issue where some item properties would cause a crash.
- Added message for characters that are missing the stash file so we no longer get an exception.
  - This can happen with characters which have never visited the Caravan Trader.
- Fixed issue where extended characters in the character name would get converted to ? in the stash file.
- Fixed issue where combined stacks did not update number in tooltip display.

### v2.16:
- Fixed some context menu display issues
- Fixed a bug where the tooltip shows constantly if you right click while dragging on another panel.
- Fixed error handling in loading resources routine.
- Fixed an issue loading for Czech language resources.
- Fixed an issue where sometimes the language code was not found.
- Fixed a bug in extracting the database without IT installed.
- Fixed a bug in set completion.
- Fix I_Raps issue where items display as ? with repacked files
  - It looks like ArchiveTool does not compress the data that it adds so I had to add support for entries that were not packed.
- Fixed an issue with the level requirement where the attribute count was counted multiple times for item copies and changing the item seed.
- Fixed Granted Skill display code. Now includes activation text and handles the community patch fix for mana cooldown.
- Fixed issues with ordering of racial bonuses and granted skills.
- Rewrote language detection loop. Hopefully this fixes the detection issues once and for all.
- Fixed error message when no characters are present in the dropdown list.
- Added background to Properties screen
- Added confirmation box for relic removal
- Changed text on item delete box
- Added Configuration Dialog
- Added a rough icon.
- Added ability to combine Charms and Relics in the vault.
- Added context menu item to complete a Charm or Relic shard.
- Added Set Name to set items.
- Added Artifact stats to formulae.
- Added Level Requirement text for Artifact formula.
- Added effects for activated skills, scrolls, and artifacts
- Added stats for summoned pets from scrolls and artifacts
- Added versioning to config file.
- Added some error handling so that missing entries will not cause a crash.
- Added ability to create a default configuration so the .default file is no longer needed.

### v2.14:
- Comes with an installer. In theory this will make sure all the VC stuff gets installed.
- Changed some UI behavior:
  - Add mouse click to enter vault
  - Changed focus to "Enter Vault" button upon resource load complete so Enter and Space bar will enter vault
  - Added deleting items with right click
  - Added relic removal with right click
  - Added rough properties screen
- Change Extract Diaglog for IT database
- Change color of requirements to grey
- Added dynamic level decoding which counts number of attributes (still needs testing)
- Added Right-click on an item and you have the option to change the itemSeed.
- Added Right-click on an artifact/charm/relic or item with a charm/relic and you can see all possible completion bonuses for that item and their chance of occuring in the game. You can also change the completion bonus by just selecting a different bonus.
- Added Right-click and item and you have the option to make a copy of the item.
- Added Right-click a set item and you have the option to create the rest of the items in the set.
- Fixed decoding for:
  - When .dbr extension is not written into item
  - Skill Descriptions for buff skills
  - Augmented pet skills
  - Granted skill level
  - Use single value when min and max are the same
  - Sleep Damage / Resistance
  - Reduced Resistance
  - Bleed Resistance
  - Skill Disruption Damage / Resistance
  - +Total Speed
  - +Total Damage
  - Slow Resistance
  - Reduced Confusion
  - Reduced Entrapment
  - Increased Projectile Speed
  - Freeze / Reduced Freeze Duration
  - Energy Drain
  - Chance for Character Effects
  - Chance for Offensive Modifiers
  - Chance for Retaliation Modifiers
  - Percent Life Retaliation
  - Reduced Damage Percent
  - Petrify
  - Artifact creation gold cost
  - Racial bonuses for multiple races
- Fix issue with creating new vaults
- Fixed static level decoding issue with string comparisons
- Fixed bug in database extraction tool for non-US versions of Windows.
- Fixed crash bug with Azurite Armbands
- Fixed (hopefully) bug with TQVaultMon not being able to find/start Titan Quest when you click the "Start TQ" button.

