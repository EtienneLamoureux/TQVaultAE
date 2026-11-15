namespace TQVaultAE.GUI.Automation;

/// <summary>
/// The instructions the MCP server sends to the client at connection (the <c>instructions</c> field of the
/// initialization): the rules the connected agent must follow and remind the operator of. The same rules are in the user
/// guide, documentation/mcp.md: keep both in step.
/// </summary>
internal static class AutomationInstructions
{
	/// <summary>The instructions text.</summary>
	public const string Text = """
		TQVault (TQVaultAE) MCP endpoint: it reads and changes the Titan Quest vaults, characters and stashes of the person
		running TQVault on this computer (the operator). Changes reach the save files when TQVault saves them (save_files,
		or when TQVault closes). Follow these rules and remind the operator of them.

		1. Local only. The endpoint listens on 127.0.0.1 and has no password. Never ask the operator to open its port to the
		   network or to forward it.
		2. Backups. Before each save TQVault copies the file it replaces to the Backups folder of the vault folder, named
		   <name>-<file>-<date>-<time>-<number>.<ext>, unless the operator turned that off for the git backup (then the git
		   repository of the settings holds the history). To restore one: close the game and TQVault, then copy the backup
		   over the file it came from, under the original name. Tell the operator this before the first change.
		3. Ask before every change. Announce each change before doing it (what moves, from where to where, which file it
		   changes) and wait for the operator's confirmation. Reading is free; moving, dropping, deleting, forging,
		   applying a loadout, importing, creating or saving files are changes.
		4. Nobody else changes the same files meanwhile: no other TQVault, editor or tool on the vaults and saves the agent
		   changes.
		5. The game and the save files. TQVault cannot tell which character the game has loaded in play or whether its
		   caravan (the storage window) is open: ask the operator, and whether the game runs.
		   - Game closed: no restriction.
		   - Game running on its start or character selection screen: no restriction.
		   - Characters not loaded in play: no restriction.
		   - The character loaded in play: its relic bag, transfer stash and player stash may be changed only while the
		     caravan is closed; ask the operator to confirm that it is closed before each such change. Changes to its
		     equipment and bags will be lost: the game overwrites them at its next save (it does not crash). Warn the
		     operator and advise against them.
		6. Vaults are TQVault's own files: the game never reads them, so changing a vault is always safe for the game.

		Start with get_state, character_list and select_character / select_vault; capture renders PNG pictures of the
		window, panels and dialogs. Full guide: documentation/mcp.md of TQVaultAE.
		""";
}
