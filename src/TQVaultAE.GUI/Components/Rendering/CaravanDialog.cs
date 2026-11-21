using TQVaultAE.GUI.Components.Rendering;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Forms;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Components.Rendering;

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
	/// A designer dialog calls it before <c>InitializeComponent</c>, as its designer set these: after it, the settings
	/// lay the dialog out again and move some controls 1-2 px.
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
	/// A Cancel button: the dialog's result Cancel, and a click closes it, also when it is shown modeless (automation).
	/// </summary>
	public static ScalingButton CancelButton(VaultForm dialog, float zoom, string name, string text)
	{
		var button = Button(dialog.Services.FontService, zoom, name, text);
		button.DialogResult = DialogResult.Cancel;
		button.Click += (_, _) => dialog.Close();
		return button;
	}

	/// <summary>A row of controls side by side (buttons), centred in the dialog's table, <paramref name="top"/> pixels under the row above.</summary>
	public static FlowLayoutPanel Row(int top, params Control[] controls)
	{
		var row = new FlowLayoutPanel()
		{
			Anchor = AnchorStyles.None,
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			FlowDirection = FlowDirection.LeftToRight,
			Margin = new Padding(0, top, 0, 0),
			WrapContents = false,
		};
		row.Controls.AddRange(controls);
		return row;
	}

	/// <summary>The dialog's table: one column of <paramref name="rows"/>, sized to its content.</summary>
	public static TableLayoutPanel Table(params Control[] rows)
	{
		var table = new TableLayoutPanel()
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			ColumnCount = 1,
			Margin = new Padding(0),
		};
		table.Controls.AddRange(rows);
		return table;
	}

	/// <summary>
	/// Puts a text in a caravan frame with one character of space inside it on the left and right, as the instructions
	/// of Vault Maintenance.
	/// </summary>
	public static void FrameText(ScalingLabel label, Font font)
	{
		int space = CaravanFrame.Border + CaravanFrame.CharacterWidth(font);
		CaravanFrame.ApplyTo(label, new Padding(space, CaravanFrame.Border, space, CaravanFrame.Border));
	}

	/// <summary>
	/// Builds the window of a dialog laid out in code around its <paramref name="table"/>: <see cref="SetUpWindow"/>, its
	/// Enter and Escape buttons, font, title (null: set later) and padding, the table inside the border, the marble look
	/// (<see cref="CaravanLook.Apply(Control, Control[])"/>, <paramref name="transparent"/> showing it), then
	/// <paramref name="finish"/> (colours set after the look) before the layout runs. The dialog then calls
	/// <c>FitAround(table)</c> so the window follows its table.
	/// </summary>
	public static void Lay(VaultForm dialog, string name, string? title, TableLayoutPanel table, Font font, float zoom
		, IButtonControl? accept, IButtonControl? cancel, Control[] transparent, Action? finish = null)
	{
		dialog.SuspendLayout();
		SetUpWindow(dialog, name);
		dialog.AcceptButton = accept;
		dialog.CancelButton = cancel;
		dialog.Font = font;
		if (title is not null)
			dialog.Text = title;
		dialog.Padding = ContentPadding(zoom);
		table.Location = new Point(dialog.Padding.Left, dialog.Padding.Top);
		dialog.Controls.Add(table);
		dialog.DrawCustomBorder = true;
		CaravanLook.Apply(dialog, [table, .. transparent]);
		finish?.Invoke();
		dialog.ResumeLayout(false);
		dialog.PerformLayout();

		// The keyboard starts on the accept button, not on the first button of the window, the close button of its
		// border, which took Enter and closed the dialog (a dialog may pick another control after this). A disabled
		// button cannot take it (Mod link's Link before a mod is chosen).
		if (accept is Control { Enabled: true } first && dialog.Contains(first))
			dialog.ActiveControl = first;
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
