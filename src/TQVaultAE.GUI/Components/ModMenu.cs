using System.Globalization;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Components;

/// <summary>
/// The "Modded" submenu of the item and vault menus: the base game, then the installed mods, the current one checked.
/// </summary>
internal static class ModMenu
{
	/// <summary>
	/// Builds the submenu in the style of the menu that holds it.
	/// </summary>
	/// <param name="owner">The menu that holds the submenu, for its colors and font.</param>
	/// <param name="text">The submenu's text.</param>
	/// <param name="modService">The installed mods, and the loading of their data.</param>
	/// <param name="current">The current mod (its identity, as items and vaults name it); null for the base game.</param>
	/// <param name="pick">Called with the chosen mod's identity (<see cref="InstalledMod.Id"/>), null for the base game,
	/// once that mod's data is loaded.</param>
	/// <param name="suggest">The mod suggestion for the items concerned, read when the submenu opens; null for none.</param>
	public static ToolStripMenuItem Create(ToolStrip owner, string text, IItemModService modService, string? current, Action<string?> pick
		, Func<ModSuggestion>? suggest = null)
	{
		var menu = Styled(owner, text);
		var mods = modService.InstalledMods;
		var currentMod = modService.Find(current);

		// The suggestion reads the mods' databases: only when the submenu opens, once.
		if (suggest is not null)
		{
			bool hinted = false;
			menu.DropDownOpening += (_, _) =>
			{
				if (hinted)
					return;

				hinted = true;
				Hint(menu, suggest());
			};
		}

		var none = Styled(owner, Resources.ItemModNone);
		none.Checked = current is null;
		none.Click += (_, _) => pick(null);
		menu.DropDownItems.Add(none);

		if (mods.Count != 0 || current is not null)
			menu.DropDownItems.Add(new ToolStripSeparator());

		foreach (var mod in mods)
		{
			// A Steam Workshop mod and a custom map of the same name show where they come from.
			bool homonym = mods.Count(m => m.Name.Equals(mod.Name, StringComparison.OrdinalIgnoreCase)) > 1;
			var entry = Styled(owner, homonym ? mod.DisplayName : mod.Name);
			entry.Tag = mod;
			entry.ToolTipText = mod.DisplayName;
			entry.Checked = mod == currentMod;
			entry.Click += async (_, _) => await PickAsync(modService, mod.Id, pick);
			menu.DropDownItems.Add(entry);
		}

		// A link to a mod that is not installed (any more) stays visible.
		if (current is not null && currentMod is null)
		{
			var missing = Styled(owner, string.Format(CultureInfo.CurrentCulture, Resources.ModLinkMissingLabel, ModIdentity.NameOf(current)));
			missing.Checked = true;
			missing.Enabled = false;
			menu.DropDownItems.Add(missing);
		}

		return menu;
	}

	/// <summary>
	/// Calls <paramref name="pick"/> once the chosen mod's database is loaded: the first choice of a mod no character
	/// uses loads it (about a second for a large mod) on a background thread, under the wait cursor, instead of freezing
	/// the window.
	/// </summary>
	private static async Task PickAsync(IItemModService modService, string mod, Action<string?> pick)
	{
		var loading = modService.LoadDataAsync(mod);
		if (!loading.IsCompleted)
		{
			System.Windows.Forms.Application.UseWaitCursor = true;
			try
			{
				await loading;
			}
			finally
			{
				System.Windows.Forms.Application.UseWaitCursor = false;
			}
		}

		pick(mod);
	}

	/// <summary>
	/// Marks every mod having records the base game lacks with a hint on the right: a record found in several mods
	/// hints each of them. A mod having only some of the records shows how many.
	/// </summary>
	private static void Hint(ToolStripMenuItem menu, ModSuggestion suggestion)
	{
		foreach (var match in suggestion.Matches)
		{
			// The entry carries its mod, not compared by its text.
			var entry = menu.DropDownItems.OfType<ToolStripMenuItem>().FirstOrDefault(i => Equals(i.Tag, match.Mod));
			if (entry is null)
				continue;

			(entry.ShortcutKeyDisplayString, entry.ToolTipText) = ModLinkText.MenuHint(match, suggestion);
			entry.ShowShortcutKeys = true;
		}
	}

	private static ToolStripMenuItem Styled(ToolStrip owner, string text)
		=> new(text)
		{
			BackColor = owner.BackColor,
			Font = owner.Font,
			ForeColor = owner.ForeColor,
		};
}
