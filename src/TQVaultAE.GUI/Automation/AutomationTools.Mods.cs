using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.GUI.Forms;

namespace TQVaultAE.GUI.Automation;

internal static partial class AutomationTools
{
	#region Mod link

	[McpServerTool(Name = "mod_state", ReadOnly = true)]
	[Description("The custom map (mod) data of the session: the active map, the maps whose database is loaded, the active map's transfer stash and relic vault files, the installed mods, and for each listed character whether it is a custom map one, its link state, its mod and the map it is loaded with. Reads the link files, changes nothing.")]
	public static string ModState()
		=> AutomationHost.OnUi(form =>
		{
			var maps = form.AutomationGameMapService;
			var links = Program.ServiceProvider.GetRequiredService<IModLinkService>();
			return Serialize(new
			{
				activeMap = maps.ActiveMap,
				loadedMaps = Program.ServiceProvider.GetRequiredService<IDatabaseMaps>().LoadedMapFolders,
				transferStash = Program.ServiceProvider.GetRequiredService<IGamePathService>().TransferStashFileFullPath,
				relicVault = Program.ServiceProvider.GetRequiredService<IGamePathService>().RelicVaultStashFileFullPath,
				installedMods = links.GetInstalledMods().Select(m => new { m.Name, m.Folder, source = m.Source.ToString() }),
				characters = form.AutomationPlayerSaves.Select(save =>
				{
					var mod = maps.ModOf(save);
					return new
					{
						label = save.ToString(),
						folder = save.Folder,
						isCustom = save.IsCustom,
						state = mod.State.ToString(),
						mod = mod.Mod?.Name ?? mod.Link?.Mod,
						map = maps.MapFor(save),
						loadedWithOtherMap = maps.LoadedWithOtherMap(save),
					};
				}),
			});
		});

	[McpServerTool(Name = "mod_suggest", ReadOnly = true)]
	[Description("Runs the mod suggestion of the mod link dialog's Suggest button for a listed character (any character: it only reads its save files): the number of records it uses that the base game lacks and the installed mods having them, best first. Links nothing.")]
	public static string ModSuggest([Description("The character's name (folder name without '_') or its label in get_state.")] string character)
		=> AutomationHost.OnUi(form =>
		{
			var save = FindSave(form, character);
			var suggestion = Program.ServiceProvider.GetRequiredService<IModLinkService>().Suggest(save.Folder);
			return Serialize(new
			{
				character = save.Name,
				suggestion.ModOnlyRecords,
				suggestion.OnlyBaseGameRecords,
				best = suggestion.Best?.Mod.Name,
				matches = suggestion.Matches.Select(m => new { mod = m.Mod.Name, m.Matched }),
			});
		});

	[McpServerTool(Name = "open_mod_link", Destructive = true)]
	[Description("Opens the mod link dialog of the selected character like the user: clicks 'Mod link...' in the menu of the tools button next to the character list (menu_click('characterTools', 'Mod link')), after this call returns (custom map characters only). It is modal: read it with control_values(form 'ModLinkDialog', labels true), pick a mod with set_control(listBoxMods, '<display name>'), then click_control buttonSuggest, buttonLink, buttonUnlink or buttonClose. Link and Unlink write the link file in the save folder: only on test characters (claude-*, player-test*, player-mod*), so this tool refuses the others.")]
	public static string OpenModLink()
		=> AutomationHost.OnUi(form =>
		{
			var save = form.AutomationSelectedSave ?? throw new McpException("No character is selected. Use select_character first.");
			RequireTestName(save.Name);
			if (!save.IsCustom)
				throw new McpException($"'{save.Name}' is a main campaign character: the menu has no mod link entry for it.");
			ClickCharacterTool(form, [form.AutomationCharacterCombo.AutomationModLinkEntry]);
			return Serialize(new { opening = "ModLinkDialog", character = save.Name });
		});

	private static PlayerSave FindSave(MainForm form, string character)
		=> form.AutomationPlayerSaves.FirstOrDefault(s => string.Equals(s.Name, character, StringComparison.OrdinalIgnoreCase)
				|| string.Equals(s.ToString(), character, StringComparison.OrdinalIgnoreCase))
			?? throw new McpException($"Unknown character '{character}'. Characters: {string.Join(", ", form.AutomationPlayerSaves.Select(s => s.Name))}");

	#endregion
}
