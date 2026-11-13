using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using TQVaultAE.Domain.Actions;
using TQVaultAE.GUI.Components.Character;

namespace TQVaultAE.GUI.Automation;

internal static partial class AutomationTools
{
	#region Loadouts

	/// <summary>When set, what the loadout actions take for "the game is running" instead of the real processes.</summary>
	internal static bool? GameRunningOverride { get; private set; }

	[McpServerTool(Name = "loadouts", Destructive = true)]
	[Description("The loadouts (saved equipment sets) of the character shown in the stash panel: the loadout bar under the statistics panel (visible, bounds and its parts, names, selected one, whether they can change), the loadouts file, and each loadout's pieces by slot (record, seed, and whether the piece worn in that slot is identical). With menu='list' or 'actions', the entries of the bar's drop-down or right-click menu (text, enabled, checked). With action, first does it like the bar: select (name), add (name), update, rename (newName), copy (newName), delete, load — on the selected loadout unless name is given; add, rename and copy take the name instead of the name dialog. Actions that change something run after the call returns (queue the answers of their boxes first: update and delete confirm, load and update ask when the game runs) and only on test characters (names starting with 'claude-', 'player-test' or 'player-mod'). gameRunning=true/false makes the actions take the game as running or not; null uses the real processes.")]
	public static string Loadouts(
		[Description("select, add, update, rename, copy, delete or load; empty to only read.")] string? action = null,
		[Description("The loadout (select, and the others instead of the selected one); the new loadout's name for add.")] string? name = null,
		[Description("The new name for rename and copy.")] string? newName = null,
		[Description("'list' or 'actions' to read that menu of the bar.")] string? menu = null,
		[Description("true or false to take the game as running or not for the next actions; omit to keep the current choice.")] bool? gameRunning = null,
		[Description("true to go back to the real game processes.")] bool realGame = false)
		=> AutomationHost.OnUi(form =>
		{
			if (gameRunning is not null)
				GameRunningOverride = gameRunning;
			if (realGame)
				GameRunningOverride = null;

			var stash = form.AutomationStashPanel ?? throw new McpException("No stash panel.");
			var player = stash.Player;
			var bar = stash.LoadoutBar;

			if (!string.IsNullOrWhiteSpace(action))
			{
				if (player is null)
					throw new McpException("No character is shown.");
				if (!AutomationTestNames.IsTestFile(player.PlayerName))
					throw new McpException($"Loadout actions only on test characters ({AutomationTestNames.Accepted}).");
				if (bar is null)
					throw new McpException("The loadout bar is not built yet: show the Equipment tab first.");

				string target = name ?? bar.Selected ?? string.Empty;
				switch (action.Trim().ToLowerInvariant())
				{
					case "select":
						if (!bar.Perform(LoadoutBarAction.Select, name))
							throw new McpException("Nothing to select.");
						break;
					case "add":
						form.BeginInvoke(() => form.AddLoadout(name));
						break;
					case "update":
						form.BeginInvoke(() => form.UpdateLoadout(target));
						break;
					case "rename":
						form.BeginInvoke(() => form.RenameLoadout(target, newName));
						break;
					case "copy":
						form.BeginInvoke(() => form.CopyLoadout(target, newName));
						break;
					case "delete":
						form.BeginInvoke(() => form.DeleteLoadout(target));
						break;
					default:
						throw new McpException($"Unknown action '{action}'.");
				}
			}

			object? menuEntries = null;
			if (bar is not null && menu is not null)
			{
				var strip = menu.Equals("list", StringComparison.OrdinalIgnoreCase) ? bar.ListMenu : bar.ActionMenu;
				strip.Show(bar, new Point(0, bar.Height));
				menuEntries = strip.Items.OfType<ToolStripItem>()
					.Select(i => new { text = i.Text, enabled = i.Enabled, @checked = (i as ToolStripMenuItem)?.Checked ?? false, separator = i is ToolStripSeparator })
					.ToList();
				strip.Close();
			}

			var set = player is null ? null : form.Services.Loadouts.Find(Path.GetDirectoryName(player.PlayerFile) ?? string.Empty);
			var worn = player?.EquipmentSack?.ToList();
			return Serialize(new
			{
				character = player?.PlayerName,
				useAlternate = player?.UseAlternate,
				gameRunningOverride = GameRunningOverride,
				bar = bar is null ? null : new
				{
					visible = bar.Visible,
					bounds = Box(bar.Bounds),
					field = Box(bar.FieldBounds),
					add = Box(bar.AddBounds),
					load = Box(bar.LoadBounds),
					names = bar.Names,
					selected = bar.Selected,
					canChange = bar.CanChange,
					font = bar.TextFont?.ToString(),
				},
				menu = menuEntries,
				file = set?.FilePath,
				fileExists = set is not null && File.Exists(set.FilePath),
				readFailed = set?.ReadFailed,
				loadouts = set?.Loadouts.Select(l => new
				{
					name = l.Name,
					useAlternate = l.UseAlternate,
					pieces = l.Pieces.Select((piece, slot) => piece is null ? null : new
					{
						slot,
						record = piece.BaseItemId.Raw,
						seed = piece.Seed,
						worn = worn is not null && slot < worn.Count && ItemIdentity.AreIdentical(worn[slot], piece),
						search = piece.Place.SackType.ToString(),
					}).Where(p => p is not null),
				}),
				worn = worn?.Select((item, slot) => ItemIdentity.IsEmpty(item) ? null : new { slot, record = item.BaseItemId.Raw, seed = item.Seed }).Where(w => w is not null),
				notification = form.AutomationNotificationText,
			});
		});

	private static int[] Box(Rectangle r) => [r.X, r.Y, r.Width, r.Height];

	#endregion
}
