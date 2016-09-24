TQVault Development Readme.
Lists all of the changes and WIP.

TODO for v2.12:
Formulae reagent requirements - DONE
Set item information - DONE
Artifact completion bonuses - DONE
Artifact type (greater/lesser/divine) - DONE
Item granted skills and descriptions - DONE
Fix {^N} tags in easter egg parchments - DONE

Changes v2.13 BETA1:
Change Extract Diaglog for IT database - DONE
Fixed decoding for:
	Fix when .dbr extension is not written into item
	Skill Descriptions for buff skills
	Augmented pet skills
	Granted skill level
	Use single value when min and max are the same
	Sleep Damage / Resistance
	Reduced Resistance
	Bleed Resistance
	Skill Disruption Damage / Resistance
	+Total Speed
	+Total Damage
	Slow Resistance
	Reduced Confusion
	Reduced Entrapment
	Increased Projectile Speed
	Freeze / Reduced Freeze Duration
	Energy Drain
	Chance for Character Effects
	Chance for Offensive Modifiers
	Chance for Retaliation Modifiers
	Percent Life Retaliation
	Reduced Damage Percent
	Petrify
	
Changes v2.13 BETA2:
Fix bug with creating new vaults
Change Requirement text to grey (like in the game)
Fix decoding Artifact Creation Cost

Changes v2.13 BETA3:
Fix decoding of racial bonuses for multiple races
Added dynamic level decoding which counts number of attributes (still needs testing)
Fixed static level decoding issue with string comparisons
Changed some UI behavior:
	Add mouse click to enter vault
	Changed focus to "Enter Vault" button upon resource load complete so Enter and Space bar will enter vault
	Added deleting items with right click
	Added relic removal with right click
	Added rough properties screen

Changes v2.13 BETA4:
Fixed bug in database extraction tool for non-US versions of Windows.
Right-click on an item and you have the option to change the itemSeed.
Right-click on an artifact/charm/relic or item with a charm/relic and you can see all possible completion bonuses
    for that item and their chance of occuring in the game.
    You can also change the completion bonus by just selecting a different bonus.

Changes v2.13 BETA5:
Comes with an installer.
    In theory this will make sure all the VC stuff gets installed.
Right click an item and you have the option to make a copy of the item.
Right click a set item and you have the option to create the rest of the items in the set.

Changes in v2.14 by bman654
- Fixed crash bug with Azurite Armbands
- Fixed (hopefully) bug with TQVaultMon not being able to find/start Titan Quest when you click the "Start TQ" button.

Changes in v2.15 BETA1:
Fixed (hopefully) an overflow issue with the dynamic requirements calculation (bman654)
Added background to Properties screen
Added confirmation box for relic removal
Changed text on item delete box
Fixed some context menu display issues
Fixed a bug where the tooltip shows constantly if you right click while dragging on another panel.
Added Configuration Dialog:
	Sets Vault path
	Filters Character List
	Turns on/off Editing
	Turns on/off copying
	Skips Title screen
	Auto load last opened vault
	Auto load last opened character
	
Changes in v2.15 BETA2:
Added a rough icon.
Fixed command line character being overridden by last opened character parameter.
Fixed error handling in loading resources routine.
Fixed an issue loading for Czech language resources.
Fixed an issue where sometimes the language code was not found.
Added Language selection to Configuration Dialog.
Fixed manual entry of vault path not being handled by Configuration UI.
Added ability to change game path in Configuration UI.

Changes in v2.15 BETA3:
Fixed a bug in extracting the database without IT installed.
Add ability to combine Charms and Relics in the vault.
Add context menu item to complete a Charm or Relic shard.

Changes in v2.15 BETA4:
Fixed a bug in set completion.

Changes in v2.15 BETA5:
Fix I_Raps issue where items display as ? with repacked files
	It looks like ArchiveTool does not compress the data that it adds so I had to add support for entries that were not packed.
Attempt to fix a bug in language detection. 
	I do not have BMan's original fix so this is based on his description of the issue.
Fixed a bug in auto-loading the last character.
Fixed an issue where the Main Vault and the "no character" selections would never auto load.
Fixed an issue with the level requirement where the attribute count was counted multiple times for item copies and changing the item seed.
Added Set Name to set items.
Added Artifact stats to formulae.
Added Level Requirement text for Artifact formula.
Fixed Granted Skill display code.  Now includes activation text and handles the community patch fix for mana cooldown.
Fixed issues with ordering of racial bonuses and granted skills.
Added effects for activated skills, scrolls, and artifacts which includes decoding for:
	itemSkillAutoController
	itemSkillName
	projectileLaunchNumber
	projectilePiercingChance
	projectileLaunchRotation
	projectileExplosionRadius
	skillChanceWeight
	skillTargetNumber
	skillTargetAngle
	skillTargetRadius
	skillActiveDuration
	skillManaCost
	skillActiveManaCost	
Added stats for summoned pets from scrolls and artifacts which includes decoding for:
	spawnObjects
	skillPetLimit
	spawnObjectsTimeToLive
Added decoding for:
	skillLifeBonus
	damageAbsorption
	damageAbsorptionPercent
	offensiveProjectileFumble
	defensivePetrify
	petBonusName
	attributeScalePercent
	
Changes in v2.15 BETA6:
Fix issue with integer stats and attributeScalePercent
Turn on tqdebug for language issues

Changes in v2.15 BETA7:
Rewrote language detection loop.  Hopefully this fixes the detection issues once and for all.
Fixed error message when no characters are present in the dropdown list.
Turned off debugging messages

Changes in v2.16:
Added versioning to config file.
Added some error handling so that missing entries will not cause a crash.
Added ability to create a default configuration so the .default file is no longer needed.

Changes in v2.17 BETA1:
Added Player Stash and Transfer Stash

Changes in v2.17 BETA2:
Added Equipment panel
Changed targets to x86 and Win32

Changes in v2.17 BETA3:
Fixed issue with inability to change Kingslayer's completion bonus.
Fixed issue where if records were in both TQ and IT DBs only the TQ version was loaded.  Changed so that the IT DB has priority.
  This fixes the issue with changing the completion bonus on the Hermes' Sandal Relic.
Added new context menu item 'Exact Copy Item' which preserves the item seed on copying.
Changed scanf function to use TryParse function to handle the format exceptions that are appearing on occasion.
Improved equipment panel display
	Fixed crash is an item is dragged over the equipment panel weapon slots.
	Fixed issue with displaying relic overlay in weapon slots.
	Improved item background display and item dragging backgrounds.
	Fixed issue where non-existant items in equipment panel would show a tooltip.
	Added functionality for 2 handed weapons in weapon slots.

Changes in v2.17 BETA4:
Fixed (hopefully this time) an overflow issue with the dynamic requirements calculation.
Fixed issue where some item properties would cause a crash.
Added message for characters that are missing the stash file so we no longer get an exception.
	This can happen with characters which have never visited the Caravan Trader.
Fixed issue where removing a relic from an item within the stash would crash the game when the character visited the Caravan Trader.
Fixed issue where extended characters in the character name would get converted to ? in the stash file.
Fixed issue where combined stacks did not update number in tooltip display.
More work on the equipment panel display
	Fixed issue where cancelling item dragging did not work.
	Fixed issue where Equipment panel tooltip would show empty slots.
	Added highlighting of open slots when dragging.

Changes in v2.17 BETA5:
Initial MOD support.  Will require a restart of TQVault every time the custom map settings are changed.
Changed the method for item type determination.  It now uses the class in the database record instead of the record path.
Added support for color tags within text strings.

Changes in v2.18:
Added decoding for:
	offensiveTotalDamageReductionAbsolute
	offensiveBaseLife
	augmented buff skills
Dynamically calculate transfer stash position based on size.  This should fix issues displaying hacked stashes.
Add command line arguments to enable mods.  Use /mod:<modname> to automatically load a mod.
	Can also use <charactername><Immortal Throne><Custom Map> to auto load a custom map character.
	For example to autoload Sarina with Lilith you use a shortcut with: TQVault.exe "Sarina<Immortal Throne><Custom Map>" /mod:Lilith
Added context menu Split for splitting stackable items.
Changed Exact Item Copy menu item to Duplicate.

Changes in v2.19 BETA1:
Fixed display of base vitality damage.
Now reads in most of the text DB from the IT files and uses the newer entries if available.
	Previously only the x-prefix files were loaded.
Single item stacks no longer show the number of the items in the stack.
Added remaining text color codes.
Enabled picking up and dropping of items on the equipment panel.

Changes in v2.19 BETA2:
Fix up equipment panel parsing and encoding.
	Now properly set the itemAttached and alternate flags.  This might be an issue with BETA1 since these values could be random.
	Moved the itemAttached logic to the sack (where it should be) instead of the item.
Added numeric labels to vault bags
Added Empty Bag, Copy Bag and Move Bag functions to vault.
Try to fix the overflow in the dynamic level calculation.  (Hopefully once and for all.)

Changes in v2.19 BETA3:
Fixed "Error Loading Resources" issue when loading TQIT text file without fanpatch installed.
Fixed issue with saving character after dropping item from inventory to vault.

Changes in v2.19 BETA4:
Fix version checking of config file.
Clean up source a little bit.  Get rid of remnants of changes that have been incorporated.
Implement debugging at runtime instead of compile time.  Turn on the commented debugging lines.
	Use DebugEnabled key to turn debugging on and off.  The level keys have no effect with this turned off.
	ARCFileDebugLevel - Used for debugging the decoding of the arc files.
	DatabaseDebugLevel - Used for debugging of decoding and loading the databases.  Resource loading issues should be shown here.
	ItemDebugLevel - Used debugging item stat decoding.
	ItemAttributesDebugLevel - Used for decoding the format specs of the items.
	The levels range from 0 to 3.
		0 - No logging except for some error messages.
		1 - Basic logging showing entering and exiting of functions.
		2 - Includes internals of functions but not loops.  This is the most common setting.
		3 - Includes the internals of loops.  This can slow down the program especially when used with the ARCFile.

Changes in v2.19 BETA5:
Added checking the IT Resources folder for updated arc files when searching for a resource.  Fixes Lil' Lued bitmap.
Added loading of more txt files.  Fixes Lil' Lued Artifact name and summon.  Should also fix allskins bottles.
Rearranged Item attributes to match the order within Art Manager.  Also added a few missing ones.
Added grouping of multiple Damage Qualifier tags.
Implement skill levels for granted skills.
Added Autosort button.

Changes in v2.19 BETA6:
Autosort now groups items based on size and then by type.  Same size items should now group with same type items.
Fixed issue with autosorting where sometimes items could overlap.
Fixed potential issue where items could extend over the edges of the sack after an Autosort.  Also added a little bit of error handling.
Changed Sack move and copy destination to context sub menu instead of dialog.  This should make it a little more user friendly.
Added keyboard shortcuts for deleting 'BkSpc', copying 'c' items and dropping 'd' (move to trash)..
Added context menu item to Move item to a different sack.

Changes in v2.19 BETA7:
Fixed issue where right clicking on stash or equipment panel caused exception.
Context menu now shows for empty bags.  Only Move is available if the bag is empty.
Added context menu for moving items from stash.

Changes in v2.20:
Added my e-mail address to the main dialog.
Fixed decoding for:
	RetaliationStun
	quest
	cannotPickUpMultiple
	
Changes in v2.21 BETA1:
Add abililty to switch upper panel between player and an additional vault.
Add basic search based on item name as it shows in the text box below the player pane.
	Shows a results dialog where an item can be double-clicked and the container of that item will be loaded.
	Pre-loads all of the vaults, stashes and character files.  This increases startup time and can be turned
		off in the configuration UI.  Searching will only search the files resident in memory.
	Can also be Activated by hitting CTRL-F.
Add mousewheel scrolling of vault list.
Fix shield base attribute sorting.
Fix custom map text showing on top of autosort button.
Fix 2H logic since some axes and maces were detected as 2H.
Fix config file update logic.
Cleaned up UI a little bit and harmonized dialogs.
Fixed background image in main dialog.

Changes in 2.2.1.2 BETA (v2.21 BETA2):
Fixed a bug in language detection when culture ID was not found in the System Neutral Culture List it would abort language detection.
Fixed null reference error when vault data folder is empty.
Fix background highlighting when picking up and then putting down the same item in the Equipment panel.
Fix background highlighting and shadow images for 2H weapons when right click cancelling a picked up piece of equipment.
Fix where Move To Bag on secondary vault panel sends to main vault panel.
Fix mouse wheel vault selection list scrolling.  Now only scrolls one line per mouse delta event.
Added Move To... item for stash.
Added Merge Bag context menu item which combines sacks.
Added CTRL-Clicking for multiple item selection.
	Can be used to delete and move.
	Added context menu item for clearing selection.
	Selection will be cleared if the vault bag is switched or if an item on another panel is selected.
	CTRL-A will select all items in the active (the one under the mouse) panel.
	CTRL-D will deselect all selected items.
Search results now highlight (select) the item chosen from the results.
Added ability to bypass warning messages.
	Can be set in configuration UI.  Defaults to false.
	Will automatically bypass item deletion, relic/charm removal and items in the trash confirmation messages.
Change versioning for auto updating.  Build is no longer included and BETA build number is no longer in title.
	Version is now <Major>.<Minor>.<Sub_Minor>.<Revision>
Added auto update functionality.  Thanks to SoulSeekkor for providing the update program and hosting the files. 
	Checks for new version on startup and prompts if an update is available.
	Configuration UI can turn off version check at startup.
	Configuration UI also has a button to manually check for updates.
	
Changes in 2.2.1.3 BETA (v2.21 BETA3):
TQVaultMon:
	Updated process start for TQVault to use the full path.
	Updated copyright information and version uses new numbering.

Fixed Null exception error when using secondary vault with vanilla TQ.
Fixed display issue with Stash panel title without character loaded.
Fixed text formatting of TQVaultMon prompt.
Fixed issue displaying unknown items.  Before, if the base item record was not found the item would be invisible.
	Now displays with default bitmap (Orange ?).
	Shows record ID as the name.
	Decodes whatever stats it can find the records for.
Fixed Move to selections for player main panel and player sack panels.
Added sack numbers to trash panel and player sack panel.
Cleaned up context menu code a little bit.  Now uses an arraylist instead of static arrays with a bunch of conditions.

Changes in 2.2.1.4 BETA (v2.21 BETA4):
Fixed issue with D2D exe file not getting detected for IT determination.
Replaced the New Vault dialog with a vault maintenance dialog and added the ability to copy, rename and delete existing vaults.

Changes in 2.2.2.0:
This version was never officially released though it did get out with autoupdate to a few users.
Need new C++ redistributable from security update (MS09-035):
http://www.microsoft.com/downloads/details.aspx?familyid=766a6af7-ec73-40ff-b072-9112bab119c2&displaylang=en

Implement Th's tooltip display fix.  The old behavior can be turned back on in the config file.
Port TQVaultMon to C#.
Clarify config file upgrade message.
Added exception text to "Error Loading Resources" message.  This should help in debugging in the future.
Added a couple more debugging messages in the database code.
Fixed issue with search when there was an underline '_' in the character name.
Added search highlighting to player and stash panels.
Fixed issue with CTRL key status getting stuck when CTRL key was held down and the mouse was dragged to a different panel.

Changes in 2.2.2.1:
Fixed redundant references to zlib_100 and TQVaultTTLib in Setup.
Added SSUpdateTQV to setup.
Added version and debug level dump to tqdebug file.
Improved autoupdate.
	Will download and run the full setup if minimum version is not met.
	Uses current patch method if minimum is met.
	Added ability to update the patch program.
	Shows the changes in the new version.
Added manual screen refresh (F5).

Changes in 2.2.2.2:
Removed some unused files from source.
Autoupdate changes:
	Version numbers can now be 2 digits instead of 1.
	Update program name is now set via update script and not hard coded.
	Fixed update program version detection when the filename has changed or the file does not exist.
Fixed issue where new vaults do not populate in the secondary vault panel dropdown.

Changes in 2.3.0.0:
Port the source to C#:
	Clean up the code a little bit getting rid of some unused items.
	Moved the resources out of zlib and into the main project.
	QuickFill is now a separate DLL (and project).
	Settings are also now part of the resources and are stored in the user's application data area.
Fixed a bug where the search grid would crash if the header was double clicked.
Turning off copying also supresses bag copying in the vault.
Add searching for any file that matches TQIT*.exe to determine if IT is installed.
Added item quality to search results.
Added ability to search by item type.  Prefix the string with at (@) to search by type.
	For example: @sword will search for all swords.
Improve Vault Maintenance a little bit.
	Can now use maintenance functions on the Main Vault.
	Selecting from the source dropdown now automatically highlights and sets focus to the destination.
Add Vorbis' fix for extended characters in the database.

Changes in 2.3.0.1:
Fixed issue where stash CRC would overflow when attempting to inject the calculated CRC back into the save stream.

Changes in 2.3.0.2:
Changed numeric conversions to now use the Invariant culture since internally everything is in US English format.
	This should get rid of all of the strange requirement values showing up.
Updated the CRC update code to use AND and SHIFT insead of calculation.
Added ability to filter by quality when searching by type.  Use the ampersand (&).
	For example to search for rare swords use: @sword&rare
Added warning for the empty bag function.

Changes in 2.3.0.3:
Fixed issue with command line not being read.
Rewrite Search code.
	Added debugging statements.
	Added more error checking.
	Removed redundant code and modularize.
	Fixes issue with exception on new characters that have not entered the game.
Fixed issue with vault list when vault is renamed to Main Vault and one did not already exist.
Fixed issue where update would trigger if update server could not be contacted.
Force updater always pull version info from the server.  Previously it would pull from the cache.
Moved contact information to About box.
Globalize application strings.
Game Language dropdown should now display in the native language of the installed .NET framework.
Include C++ redistributable in the setup.

Changes in 2.3.0.4:
Fixed issue with Update dialog closing during full setup download.
Added support for subdirectories in ARC files.  Done for full setup updates.
Added support for scaling.
	Now shows correctly with 120 DPI or 144 DPI fonts.
Added French, Spanish, German and Russian translations.
	Thanks to Jean, Vifarc, FOE and Xelat for providing translations.
	Spanish was done by Google to test localization.  Probably very bad and needs to be redone.
UI Language setting now follows game language setting if not autodetecting.
	If autodetecting it will use the OS language.
	Can be overridden by the UILanguage key in the config file.
Changed button text to dynamically resize if it is too large to fit in the button bounding box.  Done for Language support.
Added new icon & About box image.
Fixed Cancel button on update dialog.
Fixed OK button on update dialog when no update was available.
Created new update program to support downloading and extracting arc files.
Moved update URL to settings.  Updated address to soulseekkor's new site.
Added some error checking to verify that IT is installed before attempting to load the database arz file.
Added new version of standalone update program.

Changes in 2.3.1.0:
Refactor Code:
	Ported to VS2010.
	Simplify code and remove redundancy.  Also make things more 'C# like'.
	Use LINQ for internal data queries.
	Switch to Generic Collections.
	Much better FxCop and StyleCop compliance.
	Split project moving the data handling and decoding into a separate TQVaultData dll.
	Moved resource loading into individual classes instead of in the database class.
		This was left over from when the resources were stashed in zlib.
	Moved expression-eval to its own project.
	Changed to use .NET compression and removed dependency on zlib.
	Merged ArzExplorer into solution and changed to use TQVaultData.
	Removed Database extraction button from main form.
Added support for DDS transparency (Da_FileServer).
	Removed dependency on QuickFill.NET
Switched from FreeImage to DevIL/Tao.DevIl since there is an issue with Freeimage converting DDS files to bitmaps that are not DWORD aligned.
	Fixed TEX to DDS header calculation so it is more compliant to the DDS spec.
Added main window resizing.
	Can use the maximize button.
	Can grab the edges of the window to manually resize.
	Can use CTRL+ and CTRL- to incrementally resize.
	Can use CTRL+Home to reset scaling to 1.0.
	Saves scaling value in config file and will scale on load if not set to 1.0.
Increased Stash panel size to 10x15 to accomodate increased stash size.
Resized Stash tabs to accomodate 2 lines of text more comfortably.
Increased number of characters allowed in Vault Maintenance text box to 256.
Fixed flickering (flicker fix from Da_FileServer).
Fixed crashing with VS2008 (Da_FileServer).
Fixed issue where set name tag would show up in the create set items context menu.
Fix issue where completed relics and charms would not be saved if TQVault's completion function was the only action performed on the vault before closing.
Fixed error/crash with update check if network is not available.
Change Update program to show progress bar instead of message queue.

Changes in 2.3.1.1:
Fix silly bugs:
Exception when dragging an item outside of the parent sack.
Only 1 of the additional player bags shows up.

Changes in 2.3.1.2:
Fixed skewing of equipment background when no player is loaded.
Added more error handling and logging in the file parsing routines.
Added message with option to turn off loading all files if it did not complete on the last run.

Changes in 2.3.1.3:
Fixed issue with double entries in the loot table for certain items (Lupine Claw, Turtle Shell, etc.).
Fixed relic bonus not being carried into removed relics.
Fixed issue where completed relics and charms without a bonus did not show 'Change Completion Bonus' context menu item.

Changes in 2.3.1.4:
Enable new UI.
	Defaults to Off in this release.
	Added new key for enabling and disabling the new UI.
	Change ScalingButton so that it can have a graphic in addition to the standard Windows styles.
	Absract New UI into VaultForm.  Adds custom border, system menu, custom title bar, etc.
	Update existing forms to use the new UI.
Change startup logic so that MainForm does all of the loading.
	Move the progress bar into Splash Screen (Form1).
	Splash screen now fades out and Main Form fades in when new UI is enabled.  Can be disabled by manually changing the fade settings.
	Combine loading of all character files and resources.
Change the results dialog.
	Now shows a tooltip for the highlighed item.
	Does not close when an item is selected so that multiple items can now be reviewed.
	The item text now matches the item quality.
Fixed border of the text panel.  Bottom was cut off.
Fixed Move To -> Player panel.  
	Was using the size for secondary bags to find the space on automove so it would fail if the free space was outside of the size of the player secondary bags.
Increased font size for bag labels.

TODO:
Add Iterator blocks where needed.
Convert to LINQ where applicable.
Convert to Lambdas where applicable.
Change Stash and Player to be subclasses of Vault.
Possibly change sack, arzfile, arcfile, etc to derive from a single class.
Change Item format string to method instead of writing it many many times.
Change long if then else blocks to case with strings where applicable.
Implement interfaces where applicable.
Continue removing redundant code.
Continue implementing auto properties.
Continue refining documentation and variable naming, adding region blocks, etc.
Change TQVaultData so that no exceptions are swallowed.
Normalize internal database entries to upperInvariant.  This should help a bit with mod support.
Need to fix issue with character corruption in plain TQ when secondary shield slot is empty.
Clean up text processing to add support the escape codes.  Probably combined with color support.
Fix bug with attributeScalePercent
Add Edit Dialog