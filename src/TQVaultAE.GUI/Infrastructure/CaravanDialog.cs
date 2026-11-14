using TQVaultAE.GUI.Components.Rendering;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Forms;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Infrastructure;

/// <summary>
/// The pieces of the marble dialogs laid out in code (Key Binds, Mod Link, Item Seed, the text input, Item Comparison):
/// their window settings, their game buttons and the space around their content. Sizes are given at zoom 1 and 96 dpi:
/// the zoom is applied here, the DPI by Windows Forms (<see cref="AutoScaleMode.Dpi"/>).
/// </summary>
internal static class CaravanDialog
{
	/// <summary>
	/// The window settings every dialog laid out in code shares: designed at 96 dpi and scaled by Windows Forms to its
	/// monitor's DPI, the dialog background and white text, no maximize, minimize or zoom reset button, no icon nor
	/// taskbar button, centered on its owner. The caller then sets its text, font, buttons and content.
	/// </summary>
	/// <param name="dialog">The dialog, while it is built.</param>
	/// <param name="name">Its name, used by the automation.</param>
	public static void SetUpWindow(VaultForm dialog, string name)
	{
		dialog.AutoScaleDimensions = new SizeF(96F, 96F);
		dialog.AutoScaleMode = AutoScaleMode.Dpi;
		dialog.BackColor = Theme.DialogBackground;
		dialog.ForeColor = Color.White;
		dialog.MaximizeBox = false;
		dialog.MinimizeBox = false;
		dialog.NormalizeBox = false;
		dialog.Name = name;
		dialog.ShowIcon = false;
		dialog.ShowInTaskbar = false;
		dialog.StartPosition = FormStartPosition.CenterParent;
	}

	/// <summary>
	/// A game button (the gold picture, its text dark) in the button size of the marble dialogs
	/// (<see cref="CaravanLook.ButtonTextSize"/>).
	/// </summary>
	/// <param name="fonts">The font service.</param>
	/// <param name="zoom">The user's zoom.</param>
	/// <param name="name">The control's name, used by the automation.</param>
	/// <param name="text">Its text.</param>
	public static ScalingButton Button(IFontService fonts, float zoom, string name, string text)
	{
		int Zoomed(int value) => (int)Math.Round(value * zoom);
		var button = new ScalingButton()
		{
			Name = name,
			BackColor = Color.Transparent,
			FlatStyle = FlatStyle.Flat,
			Font = fonts.GetFontLight(CaravanLook.ButtonTextSize, zoom),
			ForeColor = Theme.ButtonText,
			Margin = new Padding(Zoomed(3), 0, Zoomed(3), 0),
			Size = new Size(Zoomed(132), Zoomed(30)),
			SizeToGraphic = false,
			UseCustomGraphic = true,
			UseVisualStyleBackColor = false,
		};
		button.FlatAppearance.BorderSize = 0;
		button.FlatAppearance.MouseDownBackColor = Theme.TransparentButton;
		button.FlatAppearance.MouseOverBackColor = Theme.TransparentButton;
		// After the font: the button fits its text to its width.
		button.Text = text;
		return button;
	}

	/// <summary>
	/// The padding of a dialog around its table: the custom border's thickness, and a gap of 10 on the sides, 4 under the
	/// title and 6 at the bottom.
	/// </summary>
	/// <param name="zoom">The user's zoom.</param>
	public static Padding ContentPadding(float zoom)
	{
		int Zoomed(int value) => (int)Math.Round(value * zoom);
		int gap = Zoomed(10);
		return BorderPadding(new Padding(gap, Zoomed(4), gap, Zoomed(6)));
	}

	/// <summary>
	/// The padding of a dialog drawn with the custom border: the border's thickness (<see cref="VaultForm.CustomBorderThickness"/>)
	/// plus <paramref name="gap"/>, in pixels, between the border and the content.
	/// </summary>
	public static Padding BorderPadding(Padding gap)
	{
		var border = VaultForm.CustomBorderThickness;
		return new Padding(border.Left + gap.Left, border.Top + gap.Top, border.Right + gap.Right, border.Bottom + gap.Bottom);
	}
}
