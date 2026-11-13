using TQVaultAE.GUI.Components.Scaling;

namespace TQVaultAE.GUI.Infrastructure;

/// <summary>
/// The look of the marble dialogs and views (Vault Maintenance, Settings, Advanced Search), applied by one call.
/// </summary>
internal static class CaravanLook
{
	/// <summary>The size of the text of these dialogs (labels, check boxes, group captions, lines), in points.</summary>
	public const float TextSize = 11.25F;

	/// <summary>The size of the text of their buttons, in points.</summary>
	public const float ButtonTextSize = 12F;

	/// <summary>
	/// Gives <paramref name="form"/> (a dialog, or a view laid in a window) the tiled marble as its background, frames its
	/// group boxes not framed yet with their caption over the frame's top edge (<see cref="CaravanFrame.ApplyWithCaption"/>),
	/// and a drop shadow to the labels
	/// and check boxes drawn over the marble. Call it once the dialog's own frames (<see cref="CaravanFrame.ApplyTo(Control, Padding?)"/>,
	/// <see cref="CaravanFrame.FrameLabel"/>, a caption-less group) are made: the text in a frame, on black, gets no shadow.
	/// </summary>
	/// <param name="form">The dialog or the view.</param>
	/// <param name="transparent">The panels laid over the whole dialog, made transparent so that the marble shows.</param>
	public static void Apply(Control form, params Control[] transparent)
		=> Apply(form, CaravanFrame.CaptionEdge.Broken, transparent);

	/// <summary>
	/// <see cref="Apply(Control, Control[])"/> with the way the group frames' top edge meets their caption.
	/// </summary>
	/// <param name="form">The dialog or the view.</param>
	/// <param name="captionEdge">Whether the frame's top edge is broken behind a group's caption or runs whole under it.</param>
	/// <param name="transparent">The panels laid over the whole dialog, made transparent so that the marble shows.</param>
	public static void Apply(Control form, CaravanFrame.CaptionEdge captionEdge, params Control[] transparent)
	{
		MarbleBackground.ApplyTo(form);
		ApplyFrames(form, captionEdge, transparent);
	}

	/// <summary>
	/// <see cref="Apply(Control, Control[])"/> without the marble: for a view laid in the main window, over the window's
	/// own background, which shows through it (the view and <paramref name="transparent"/> made transparent).
	/// </summary>
	/// <param name="view">The view.</param>
	/// <param name="transparent">The panels laid over the whole view, made transparent so that the background shows.</param>
	public static void ApplyFrames(Control view, params Control[] transparent)
	{
		view.BackColor = Color.Transparent;
		view.BackgroundImage = null;
		ApplyFrames(view, CaravanFrame.CaptionEdge.Broken, transparent);
	}

	/// <summary>The frames, captions and text shadows of <see cref="Apply(Control, CaravanFrame.CaptionEdge, Control[])"/>.</summary>
	private static void ApplyFrames(Control form, CaravanFrame.CaptionEdge captionEdge, Control[] transparent)
	{
		foreach (var control in transparent)
			control.BackColor = Color.Transparent;

		CaravanFrame.ApplyWithCaptionToAll(form, captionEdge);
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
