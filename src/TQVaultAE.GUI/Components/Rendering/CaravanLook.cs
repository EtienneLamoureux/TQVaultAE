using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.Presentation.Win32.Contracts;
using TQVaultAE.GUI.Components.Scaling;

namespace TQVaultAE.GUI.Components.Rendering;

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
	/// The one exception to <see cref="TextSize"/>: the text of Advanced Search, whose 17 category lists must fit on one
	/// screen (at 11.25 pt the last two categories went below the window at zoom 1), in points.
	/// </summary>
	public const float DenseTextSize = 9F;

	/// <summary>
	/// The one font rule of the marble dialogs and views: <paramref name="root"/> and the controls without a font of their
	/// own in <see cref="TextSize"/>, the buttons in <see cref="ButtonTextSize"/>. The scaling controls get the text font
	/// itself (they hold their font and grow it with the zoom); the others, group captions included, take the dialog's,
	/// which grows with the zoom (<see cref="Forms.VaultForm.ApplyUserZoom"/>). Called before the dialog is zoomed; a
	/// dialog sets its exceptions (a title) after it.
	/// </summary>
	/// <param name="root">The dialog or view.</param>
	/// <param name="fonts">The font service.</param>
	/// <param name="textSize">The text size: <see cref="TextSize"/>, or <see cref="DenseTextSize"/> for Advanced Search.</param>
	public static void ApplyFonts(Control root, IFontService fonts, float textSize = TextSize)
	{
		var text = fonts.GetFontLight(textSize);
		var button = fonts.GetFontLight(ButtonTextSize);
		root.Font = text;
		root.ProcessAllControls(control =>
		{
			if (control is Button)
				control.Font = button;
			else if (control is IScalingControl)
				control.Font = text;
			else
				control.ResetFont();
		});
	}

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
	/// <see cref="Apply(Control, Control[])"/> with another marble, the main window's for example.
	/// </summary>
	/// <param name="form">The dialog or the view.</param>
	/// <param name="marble">The marble tiled behind it.</param>
	/// <param name="transparent">The panels laid over the whole dialog, made transparent so that the marble shows.</param>
	public static void Apply(Control form, MarbleBackground.Marble marble, params Control[] transparent)
	{
		MarbleBackground.ApplyTo(form, marble);
		ApplyFrames(form, CaravanFrame.CaptionEdge.Broken, transparent);
	}

	/// <summary>
	/// Draws the labels and check boxes over the marble of <paramref name="form"/> like the group captions over the frames:
	/// a dark outline around the text instead of the drop shadow. Call it after <see cref="Apply(Control, Control[])"/>.
	/// </summary>
	/// <param name="form">The dialog or the view.</param>
	public static void OutlineText(Control form)
		=> form.ProcessAllControls(c =>
		{
			if (c is ScalingLabel { TextShadow: true } label)
				label.TextOutline = true;
			else if (c is ScalingCheckBox { TextShadow: true } box)
				box.TextOutline = true;
		});

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
