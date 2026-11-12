using TQVaultAE.GUI.Components.Scaling;

namespace TQVaultAE.GUI.Infrastructure;

/// <summary>
/// The look of the marble dialogs (Vault Maintenance, Settings, Advanced Search), applied by one call.
/// </summary>
internal static class CaravanLook
{
	/// <summary>
	/// Gives <paramref name="form"/> the tiled marble as its background, frames its group boxes not framed yet with their
	/// caption over the frame's top edge (<see cref="CaravanFrame.ApplyWithCaption"/>), and a drop shadow to the labels
	/// and check boxes drawn over the marble. Call it once the dialog's own frames (<see cref="CaravanFrame.ApplyTo(Control, Padding?)"/>,
	/// <see cref="CaravanFrame.FrameLabel"/>, a caption-less group) are made: the text in a frame, on black, gets no shadow.
	/// </summary>
	/// <param name="form">The dialog.</param>
	/// <param name="transparent">The panels laid over the whole dialog, made transparent so that the marble shows.</param>
	public static void Apply(Form form, params Control[] transparent)
	{
		MarbleBackground.ApplyTo(form);
		foreach (var control in transparent)
			control.BackColor = Color.Transparent;

		CaravanFrame.ApplyWithCaptionToAll(form);
		form.ProcessAllControls(c =>
		{
			bool inFrame = CaravanFrame.IsInFrame(c);
			// A track bar cannot be transparent: on the black of its frame.
			if (c is TrackBar && inFrame)
				c.BackColor = Color.Black;
			else if (!inFrame && c is ScalingLabel label)
				label.TextShadow = true;
			else if (!inFrame && c is ScalingCheckBox box)
				box.TextShadow = true;
		});
	}
}
