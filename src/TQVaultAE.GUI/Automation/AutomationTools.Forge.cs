using System.ComponentModel;
using System.Drawing.Imaging;
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
			var sounds = RecordingSoundService.LastSequence;
			form.AutomationShowForge(show);
			return ForgeStatus(form, sounds, int.MaxValue);
		});

	[McpServerTool(Name = "forge_state", ReadOnly = true)]
	[Description("Returns the forge state: shown, mode, hardcore, the item in each slot (Base, Prefix, Suffix, Relic1, Relic2) with the parts it offers in God mode, the preview of the forged item and the last messages shown to the user (in 'newMessages').")]
	public static string ForgeState()
		=> AutomationHost.OnUi(form => ForgeStatus(form, RecordingSoundService.LastSequence, 0));

	[McpServerTool(Name = "forge_set_mode", Destructive = true)]
	[Description("Selects the forge mode, like a click on its radio button: Game, Strict, Relax or God. Choosing a stricter mode than before clears the slots, as in the app.")]
	public static string ForgeSetMode([Description("Game, Strict, Relax or God.")] string mode)
		=> OnForge((form, forge) =>
		{
			if (!Enum.TryParse<ForgeMode>(mode, true, out var value))
				throw new McpException($"Unknown mode '{mode}'. Modes: {string.Join(", ", Enum.GetNames<ForgeMode>())}");
			forge.AutomationSetMode(value);
		});

	[McpServerTool(Name = "forge_mode_card", ReadOnly = true)]
	[Description("Returns the card shown when the mouse rests on a forge mode radio button (Game, Strict, Relax or God): its title, summary, sections with their lines (allowed or restricted) and footer, with a PNG of the card drawn as the tool tip draws it. Nothing is changed.")]
	public static IEnumerable<ContentBlock> ForgeModeCard([Description("Game, Strict, Relax or God.")] string mode)
	{
		var (json, png) = AutomationHost.OnUi(form =>
		{
			if (!Enum.TryParse<ForgeMode>(mode, true, out var value))
				throw new McpException($"Unknown mode '{mode}'. Modes: {string.Join(", ", Enum.GetNames<ForgeMode>())}");
			var forge = form.AutomationForgePanel ?? throw new McpException("The forge is not created yet. Use forge_show first.");
			var (card, image) = forge.AutomationModeCard(value) ?? throw new McpException("The mode cards are not set yet.");
			using (image)
			using (var stream = new MemoryStream())
			{
				image.Save(stream, ImageFormat.Png);
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
				}), stream.ToArray());
			}
		});

		return [new TextContentBlock { Text = json }, ImageContentBlock.FromBytes(png, "image/png")];
	}

	[McpServerTool(Name = "forge_set_hardcore", Destructive = true)]
	[Description("Checks or unchecks the Hardcore box: in hardcore, forging consumes the material items.")]
	public static string ForgeSetHardcore([Description("true to consume the material items.")] bool enabled)
		=> OnForge((form, forge) => forge.AutomationSetHardcore(enabled));

	[McpServerTool(Name = "forge_drop", Destructive = true)]
	[Description("Drops the item being dragged (picked with pick_item) on a forge slot, like a click on that slot. The forge rules of the current mode decide; a refusal is reported in 'newMessages' and the item stays dragged.")]
	public static string ForgeDrop([Description("Base, Prefix, Suffix, Relic1 or Relic2.")] string slot)
		=> OnForge((form, forge) =>
		{
			var value = ParseSlot(slot);
			RequireDrag(form);
			forge.AutomationDrop(value);

			// An accepted drop returns the item to its sack: stop hovering the panel it came from.
			if (!form.AutomationDragInfo.IsActive)
				ReleaseCursor(VirtualCursor.Target as SackPanel);
		});

	[McpServerTool(Name = "forge_select_part", Destructive = true)]
	[Description("God mode: chooses which part of a material slot's item is merged (Base, Prefix, Suffix, Relic1, Relic2), like its combo box. forge_state lists the parts each slot offers.")]
	public static string ForgeSelectPart(
		[Description("Prefix, Suffix, Relic1 or Relic2.")] string slot,
		[Description("Base, Prefix, Suffix, Relic1 or Relic2.")] string part)
		=> OnForge((form, forge) =>
		{
			var slotValue = ParseSlot(slot);
			if (!Enum.TryParse<ForgePart>(part, true, out var partValue))
				throw new McpException($"Unknown part '{part}'. Parts: {string.Join(", ", Enum.GetNames<ForgePart>())}");
			if (forge.AutomationMode != ForgeMode.God)
				throw new McpException("Parts can only be chosen in God mode. Use forge_set_mode first.");
			if (!forge.AutomationSelectPart(slotValue, partValue))
				throw new McpException($"Slot {slotValue} has no part {partValue}. See forge_state.");
		});

	[McpServerTool(Name = "forge_reset", Destructive = true)]
	[Description("Clears every forge slot, like the Reset button. The items stay where they are.")]
	public static string ForgeReset() => OnForge((form, forge) => forge.AutomationReset());

	[McpServerTool(Name = "forge_forge", Destructive = true)]
	[Description("Forges, like the Forge button: merges the selected parts into the base item (in memory, saved with the vault or character like any change) and closes the forge. In God mode it plays the level-up MP3, converted by NAudio. 'forgedItem' is the base item after the merge, 'sounds' what was played.")]
	public static string ForgeForge()
		=> AutomationHost.OnUi(form =>
		{
			var forge = RequireForge(form);
			var baseItem = forge.AutomationSlots.First(s => s.Slot == ForgeSlot.Base).Item;
			var sounds = RecordingSoundService.LastSequence;
			var messages = forge.AutomationMessages.Count;

			forge.AutomationForge();

			var status = System.Text.Json.JsonDocument.Parse(ForgeStatus(form, sounds, messages)).RootElement;
			return Serialize(new
			{
				forged = baseItem is not null && !form.AutomationForgeShown,
				forgedItem = DescribeItem(baseItem, null),
				status,
			});
		});

	[McpServerTool(Name = "forge_drag_cursor")]
	[Description("Runs the forge's drag cursor update, like a mouse move over the forge while an item is dragged (pick_item first). Returns whether a cursor was built from the item's bitmap, its size and hot spot (the bitmap centre).")]
	public static string ForgeDragCursor()
		=> AutomationHost.OnUi(form =>
		{
			var cursor = RequireForge(form).AutomationUpdateDragCursor();
			return Serialize(new
			{
				built = cursor is not null,
				width = cursor?.Size.Width,
				height = cursor?.Size.Height,
				hotSpot = cursor is null ? null : new { x = cursor.HotSpot.X, y = cursor.HotSpot.Y },
			});
		});

	[McpServerTool(Name = "forge_cancel", Destructive = true)]
	[Description("Cancels the forge, like its Cancel button: clears the slots and closes the forge.")]
	public static string ForgeCancel() => OnForge((form, forge) => forge.AutomationCancel());

	#endregion

	#region Sounds

	[McpServerTool(Name = "recent_sounds", ReadOnly = true)]
	[Description("Returns the last sounds TQVault played, oldest first. For a named sound, 'loaded' and 'wavBytes' tell whether its data could be loaded (an MP3 is converted to WAV by NAudio first).")]
	public static string RecentSounds([Description("Number of sounds (default 20).")] int n = 20)
		=> Serialize(new
		{
			soundsEnabled = Program.ServiceProvider.GetService(typeof(TQVaultAE.Application.Contracts.Services.ISoundService)) is RecordingSoundService recorder
				? recorder.SoundsEnabled
				: (bool?)null,
			sounds = RecordingSoundService.Last(Math.Clamp(n, 1, 200)).Select(DescribeSound),
		});

	#endregion

	#region Forge helpers

	/// <summary>Runs a forge action and reports the forge state with the messages and sounds it produced.</summary>
	private static string OnForge(Action<MainForm, ForgePanel> action)
		=> AutomationHost.OnUi(form =>
		{
			var forge = RequireForge(form);
			var sounds = RecordingSoundService.LastSequence;
			var messages = forge.AutomationMessages.Count;

			action(form, forge);
			return ForgeStatus(form, sounds, messages);
		});

	private static ForgePanel RequireForge(MainForm form)
	{
		var forge = form.AutomationForgePanel
			?? throw new McpException("The forge is not created yet.");
		if (!form.AutomationForgeShown)
			throw new McpException("The forge is not shown. Use forge_show first.");
		return forge;
	}

	private static ForgeSlot ParseSlot(string slot)
		=> Enum.TryParse<ForgeSlot>(slot, true, out var value)
			? value
			: throw new McpException($"Unknown slot '{slot}'. Slots: {string.Join(", ", Enum.GetNames<ForgeSlot>())}");

	/// <param name="soundsSince">Sounds recorded after this sequence number are reported as 'sounds'.</param>
	/// <param name="messagesFrom">Messages from this index on are reported as 'newMessages'.</param>
	private static string ForgeStatus(MainForm form, long soundsSince, int messagesFrom)
	{
		var forge = form.AutomationForgePanel;
		var messages = forge?.AutomationMessages ?? [];
		return Serialize(new
		{
			shown = form.AutomationForgeShown,
			mode = forge?.AutomationMode.ToString(),
			hardcore = forge?.AutomationHardcore,
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
			sounds = RecordingSoundService.Since(soundsSince).Select(DescribeSound),
		});
	}

	private static object DescribeSound(RecordingSoundService.PlayedSound sound) => new
	{
		time = sound.Time.ToString("HH:mm:ss.fff"),
		call = sound.Call,
		resource = sound.Resource,
		loaded = sound.Loaded,
		wavBytes = sound.WavBytes,
		error = sound.Error,
	};

	#endregion
}
