using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Forge;
using TQVaultAE.GUI.Automation;
using TQVaultAE.GUI.Tooltips;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Components.Panels;

/// <summary>Debug access to the forge: slot hover and drag cursor.</summary>
public partial class ForgePanel
{
	/// <summary>Rests the mouse on a slot, like its MouseEnter: shows the item tooltip (the preview for the base slot).</summary>
	internal void AutomationHover(ForgeSlot slot) => this.pictureBox_MouseEnter(this.SlotOf(slot).Picture, EventArgs.Empty);

	/// <summary>Leaves a slot, like its MouseLeave: hides the item tooltip.</summary>
	internal void AutomationHoverEnd(ForgeSlot slot) => this.pictureBox_MouseLeave(this.SlotOf(slot).Picture, EventArgs.Empty);

	/// <summary>
	/// Runs the drag cursor update of a mouse move over the forge: the cursor built from the dragged item's bitmap.
	/// </summary>
	/// <returns>The cursor, or <see langword="null"/> when nothing is dragged or it could not be built.</returns>
	internal Cursor? AutomationUpdateDragCursor()
	{
		this.UpdateDragCursor();
		return this.dragCursor?.Cursor;
	}
}
