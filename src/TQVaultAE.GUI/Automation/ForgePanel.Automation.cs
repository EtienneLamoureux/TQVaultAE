using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Forge;
using TQVaultAE.GUI.Tooltips;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Components.Panels;

/// <summary>
/// Debug-only access to the forge for the automation endpoint. Every action goes through the forge's own code.
/// </summary>
public partial class ForgePanel
{
	/// <summary>One forge slot as the automation endpoint reports it.</summary>
	internal sealed record AutomationSlotState(ForgeSlot Slot, Item? Item, SackCollection? Sack, IReadOnlyList<ForgePart> Parts, ForgePart? SelectedPart);

	private readonly List<(ForgeMessage Message, string Text)> automationMessages = [];

	internal ForgeMode AutomationMode => this.Mode;

	internal bool AutomationHardcore => this.HardcoreMode;

	/// <summary>The base item with the current selection applied, as its tooltip shows it.</summary>
	internal Item? AutomationPreview => this.PreviewItem;

	internal IReadOnlyList<AutomationSlotState> AutomationSlots
		=> this.Slots.Select(view => new AutomationSlotState(
			view.Slot,
			this.Selection[view.Slot],
			this.Sacks.GetValueOrDefault(view.Slot),
			view.Combo?.Items.OfType<ForgeComboItem>().Select(i => i.Value).ToList() ?? [],
			view.Combo is { Visible: true } ? this.GodPart(view.Slot) : null)).ToList();

	/// <summary>The panel's height against its limit: the height, the preferred height, the limit, and the slots' cut and height.</summary>
	internal object AutomationLayout => new
	{
		width = this.Width,
		height = this.Height,
		autoScale = new { mode = this.AutoScaleMode.ToString(), design = this.AutoScaleDimensions, current = this.CurrentAutoScaleDimensions },
		preferredHeight = this.GetPreferredSize(Size.Empty).Height,
		heightLimit = this.HeightLimit?.Invoke() ?? 0,
		slotCut = this.slotCut,
		slotHeight = this.pictureBoxBaseItem.Height,
		rowHeights = this.tableLayoutPanelForge.GetRowHeights(),
		combos = this.Slots.Where(s => s.Combo is { Visible: true }).Select(s => new { s.Slot, s.Combo!.Height, Font = s.Combo.Font.SizeInPoints }),
	};

	/// <summary>Messages the forge showed to the user, oldest first.</summary>
	internal IReadOnlyList<(ForgeMessage Message, string Text)> AutomationMessages => this.automationMessages;

	internal void AutomationRecordMessage(ForgeMessage message)
	{
		this.automationMessages.Add((message, ForgeMessageText.Of(message, this.OfTheTinkererTranslation)));
		if (this.automationMessages.Count > 50)
			this.automationMessages.RemoveAt(0);
	}

	/// <summary>The card shown when the mouse rests on the radio button of <paramref name="mode"/>, with its rendering.</summary>
	internal (RichToolTip.Card Card, Bitmap Image)? AutomationModeCard(ForgeMode mode)
		=> this.modeToolTip?.AutomationCard(this.ModeRadio(mode)) is { } card
			? (card, this.modeToolTip.AutomationRender(card))
			: null;

	private ScalingRadioButton ModeRadio(ForgeMode mode)
		=> mode switch
		{
			ForgeMode.God => this.scalingRadioButtonGod,
			ForgeMode.Relax => this.scalingRadioButtonRelax,
			ForgeMode.Strict => this.scalingRadioButtonStrict,
			_ => this.scalingRadioButtonGame,
		};

	/// <summary>Selects a mode like a click on its radio button, including the selection reset on a stricter mode.</summary>
	internal void AutomationSetMode(ForgeMode mode)
	{
		var radio = this.ModeRadio(mode);
		radio.Checked = true;
		this.ForgeMode_Clicked(radio, EventArgs.Empty);
	}

	internal void AutomationSetHardcore(bool enabled) => this.scalingCheckBoxHardcore.Checked = enabled;

	/// <summary>
	/// Runs the drag cursor update of a mouse move over the forge: the cursor built from the dragged item's bitmap.
	/// </summary>
	/// <returns>The cursor, or <see langword="null"/> when nothing is dragged or it could not be built.</returns>
	internal Cursor? AutomationUpdateDragCursor()
	{
		this.UpdateDragCursor();
		return this.dragCursor?.Cursor;
	}

	/// <summary>Drops the dragged item on <paramref name="slot"/>, like a click on that slot's picture.</summary>
	/// <returns>false when nothing is dragged.</returns>
	internal bool AutomationDrop(ForgeSlot slot)
	{
		if (!this.IsDraging)
			return false;

		// A click anywhere in the forge drops on the slot under the cursor: the virtual cursor rests on the slot, and the
		// click goes to its picture with mouse messages.
		var picture = this.SlotOf(slot).Picture;
		if (!picture.Visible)
			throw new InvalidOperationException($"The {slot} slot is hidden in this mode: a user could not click it.");
		var center = Automation.AutomationInput.Center(picture);
		Automation.VirtualCursor.Set(picture, center);
		Automation.AutomationInput.SendMouse(picture, center, MouseButtons.Left, 1);
		return true;
	}

	/// <summary>God mode: picks which part of a material item is used, like choosing it in the slot's combo box.</summary>
	/// <returns>false when the slot has no such part.</returns>
	internal bool AutomationSelectPart(ForgeSlot slot, ForgePart part)
	{
		var combo = this.SlotOf(slot).Combo;
		var entry = combo?.Items.OfType<ForgeComboItem>().FirstOrDefault(i => i.Value == part);
		if (combo is null || entry is null)
			return false;

		combo.SelectedItem = entry;
		return true;
	}

	/// <summary>Rests the mouse on a slot, like its MouseEnter: shows the item tooltip (the preview for the base slot).</summary>
	internal void AutomationHover(ForgeSlot slot) => this.pictureBox_MouseEnter(this.SlotOf(slot).Picture, EventArgs.Empty);

	/// <summary>Leaves a slot, like its MouseLeave: hides the item tooltip.</summary>
	internal void AutomationHoverEnd(ForgeSlot slot) => this.pictureBox_MouseLeave(this.SlotOf(slot).Picture, EventArgs.Empty);

	internal void AutomationReset() => this.scalingButtonReset_Click(this.ResetButton, EventArgs.Empty);

	internal void AutomationForge() => this.ForgeButton_Click(this.ForgeButton, EventArgs.Empty);

	internal void AutomationCancel() => this.CancelButton_Click(this.CancelButton, EventArgs.Empty);
}
