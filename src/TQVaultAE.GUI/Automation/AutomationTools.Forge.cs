using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using TQVaultAE.Domain.Forge;
using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Forms;

namespace TQVaultAE.GUI.Automation;

/// <summary>
/// MCP tools that drive the forge, and the sound recorder.
/// </summary>
internal static partial class AutomationTools
{
	#region Forge

	[McpServerTool(Name = "forge_show", Destructive = true)]
	[Description("Shows or hides the forge, like the Forge button. The forge hides the player, stash and secondary vault panels; items for it are picked from the vault panel with pick_item.")]
	public static string ForgeShow([Description("true to show the forge, false to hide it.")] bool show = true)
		=> AutomationHost.OnUi(form =>
		{
			var sounds = SoundSequence;
			form.AutomationShowForge(show);
			return Serialize(ForgeStatus(form, sounds, int.MaxValue));
		});

	[McpServerTool(Name = "forge_state", ReadOnly = true)]
	[Description("Returns the forge state: shown, mode, hardcore, the item in each slot (Base, Prefix, Suffix, Relic1, Relic2) with the parts it offers in God mode, the preview of the forged item and the last messages shown to the user (in 'newMessages').")]
	public static string ForgeState()
		=> AutomationHost.OnUi(form => Serialize(ForgeStatus(form, SoundSequence, 0)));

	[McpServerTool(Name = "forge_set_mode", Destructive = true)]
	[Description("Selects the forge mode, like a click on its radio button: Game, Strict, Relax or God. Choosing a stricter mode than before clears the slots, as in the app.")]
	public static string ForgeSetMode([Description("Game, Strict, Relax or God.")] string mode)
		=> OnForge((form, forge) =>
		{
			var value = ParseEnum<ForgeMode>(mode, "mode");
			forge.AutomationSetMode(value);
		});

	[McpServerTool(Name = "forge_mode_card", ReadOnly = true)]
	[Description("Returns the card shown when the mouse rests on a forge mode radio button (Game, Strict, Relax or God): its title, summary, sections with their lines (allowed or restricted) and footer, with a PNG of the card drawn as the tool tip draws it. Nothing is changed.")]
	public static IEnumerable<ContentBlock> ForgeModeCard([Description("Game, Strict, Relax or God.")] string mode)
	{
		var (json, png) = AutomationHost.OnUi(form =>
		{
			var value = ParseEnum<ForgeMode>(mode, "mode");
			var forge = form.AutomationForgePanel ?? throw new McpException("The forge is not created yet. Use forge_show first.");
			var (card, image) = forge.AutomationModeCard(value) ?? throw new McpException("The mode cards are not set yet.");
			using (image)
			{
				return (Serialize(new
				{
					mode = value.ToString(),
					card.Title,
					card.Summary,
					sections = card.Sections.Select(s => new
					{
						s.Title,
						lines = s.Lines.Select(l => (l.Allowed ? "+ " : "- ") + l.Text).ToList(),
					}).ToList(),
					card.Footer,
				}), AutomationImages.Png(image));
			}
		});

		return AutomationImages.Result(json, png);
	}

	[McpServerTool(Name = "forge_set_hardcore", Destructive = true)]
	[Description("Checks or unchecks the Hardcore box: in hardcore, forging consumes the material items.")]
	public static string ForgeSetHardcore([Description("true to consume the material items.")] bool enabled)
		=> OnForge((form, forge) => forge.AutomationSetHardcore(enabled));

	[McpServerTool(Name = "forge_drop", Destructive = true)]
	[Description("Drops the item being dragged (picked with pick_item) on a forge slot like the user: the virtual cursor moves onto the slot's picture (the panel hovered before gets its mouse leave) and a click goes to the picture with mouse messages; the forge's click handler drops on the slot under the cursor. The forge rules of the current mode decide; a refusal is reported in 'newMessages' and the item stays dragged, the virtual cursor on the slot. A slot hidden in this mode is refused.")]
	public static string ForgeDrop([Description("Base, Prefix, Suffix, Relic1 or Relic2.")] string slot)
		=> OnForge((form, forge) =>
		{
			var value = ParseEnum<ForgeSlot>(slot, "slot");
			RequireDrag(form);
			ReleaseCursor(VirtualCursor.Target as SackPanel);
			forge.AutomationDrop(value);

			// An accepted drop returns the item to its sack: the cursor goes back to the real mouse.
			if (!form.AutomationDragInfo.IsActive)
				VirtualCursor.Release();
		});

	[McpServerTool(Name = "forge_select_part", Destructive = true)]
	[Description("God mode: chooses which part of a material slot's item is merged (Base, Prefix, Suffix, Relic1, Relic2), like its combo box. forge_state lists the parts each slot offers.")]
	public static string ForgeSelectPart(
		[Description("Prefix, Suffix, Relic1 or Relic2.")] string slot,
		[Description("Base, Prefix, Suffix, Relic1 or Relic2.")] string part)
		=> OnForge((form, forge) =>
		{
			var slotValue = ParseEnum<ForgeSlot>(slot, "slot");
			var partValue = ParseEnum<ForgePart>(part, "part");
			if (forge.AutomationMode != ForgeMode.God)
				throw new McpException("Parts can only be chosen in God mode. Use forge_set_mode first.");
			if (!forge.AutomationSelectPart(slotValue, partValue))
				throw new McpException($"Slot {slotValue} has no part {partValue}. See forge_state.");
		});

	[McpServerTool(Name = "forge_reset", Destructive = true)]
	[Description("Clears every forge slot, like the Reset button. The items stay where they are. Refused while the button is greyed or hidden, as for the user.")]
	public static string ForgeReset() => OnForge((form, forge) => forge.AutomationReset());

	[McpServerTool(Name = "forge_forge", Destructive = true)]
	[Description("Forges, like the Forge button: merges the selected parts into the base item (in memory, saved with the vault or character like any change) and closes the forge; refused while the button is greyed or hidden (no base item, nothing to merge), as for the user. In God mode it plays the level-up MP3, converted by NAudio. 'forgedItem' is the base item after the merge, 'sounds' what was played.")]
	public static string ForgeForge()
		=> AutomationHost.OnUi(form =>
		{
			var forge = RequireForge(form);
			var baseItem = forge.AutomationSlots.First(s => s.Slot == ForgeSlot.Base).Item;
			var sounds = SoundSequence;
			var messages = forge.AutomationMessages.Count;

			forge.AutomationForge();

			var status = ForgeStatus(form, sounds, messages);
			return Serialize(new
			{
				forged = baseItem is not null && !form.AutomationForgeShown,
				forgedItem = DescribeItem(baseItem, null),
				status,
			});
		});

	[McpServerTool(Name = "forge_cancel", Destructive = true)]
	[Description("Cancels the forge, like its Cancel button: clears the slots and closes the forge. Refused while the button is greyed or hidden, as for the user.")]
	public static string ForgeCancel() => OnForge((form, forge) => forge.AutomationCancel());

	#endregion

	#region Sounds

	#endregion

	#region Forge helpers

	/// <summary>The sequence number of the last sound played: recorded in Debug builds only, 0 in Release builds.</summary>
#if DEBUG
	private static long SoundSequence => RecordingSoundService.LastSequence;
#else
	private static long SoundSequence => 0;
#endif

	/// <summary>Runs a forge action and reports the forge state with the messages and sounds it produced.</summary>
	private static string OnForge(Action<MainForm, ForgePanel> action)
		=> AutomationHost.OnUi(form =>
		{
			var forge = RequireForge(form);
			var sounds = SoundSequence;
			var messages = forge.AutomationMessages.Count;
			action(form, forge);
			return Serialize(ForgeStatus(form, sounds, messages));
		});

	private static ForgePanel RequireForge(MainForm form)
	{
		var forge = form.AutomationForgePanel
			?? throw new McpException("The forge is not created yet.");
		if (!form.AutomationForgeShown)
			throw new McpException("The forge is not shown. Use forge_show first.");
		return forge;
	}

	/// <param name="soundsSince">Sounds recorded after this sequence number are reported as 'sounds'.</param>
	/// <param name="messagesFrom">Messages from this index on are reported as 'newMessages'.</param>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1859", Justification = "Anonymous JSON payload.")]
	private static object ForgeStatus(MainForm form, long soundsSince, int messagesFrom)
	{
		var forge = form.AutomationForgePanel;
		var messages = forge?.AutomationMessages ?? [];
		return new
		{
			shown = form.AutomationForgeShown,
			mode = forge?.AutomationMode.ToString(),
			hardcore = forge?.AutomationHardcore,
			layout = forge?.AutomationLayout,
			slots = forge?.AutomationSlots.Select(s => new
			{
				slot = s.Slot.ToString(),
				item = DescribeItem(s.Item, null),
				sackType = s.Sack?.SackType.ToString(),
				parts = s.Parts.Select(p => p.ToString()),
				selectedPart = s.SelectedPart?.ToString(),
			}),
			preview = form.AutomationForgeShown ? DescribeItem(forge?.AutomationPreview, null) : null,
			dragging = form.AutomationDragInfo.IsActive ? DescribeItem(form.AutomationDragInfo.Item, null) : null,
			newMessages = messages.Skip(Math.Min(messagesFrom, messages.Count)).Select(m => new { message = m.Message.ToString(), text = m.Text }),
#if DEBUG
			// The sounds played, recorded in Debug builds only.
			sounds = RecordingSoundService.Since(soundsSince).Select(DevTools.DescribeSound),
#endif
		};
	}

	#endregion
}
