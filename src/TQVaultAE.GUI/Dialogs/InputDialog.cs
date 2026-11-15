//-----------------------------------------------------------------------
// <copyright file="InputDialog.cs" company="None">
//     Copyright (c) TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TQVaultAE.GUI.Dialogs;

using System;
using System.Windows.Forms;
using TQVaultAE.GUI.Components.Rendering;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Forms;
using TQVaultAE.Presentation;

/// <summary>
/// Asks the user for a text (the name of a duplicated character, of a loadout). A TQ window like Vault Maintenance:
/// marble, the prompt in a caravan frame, the text box under it, gold buttons; laid out in code by a table, at the
/// user's zoom, and following the DPI. Shown by <see cref="Show(FormFactory, string, string, string)"/>.
/// </summary>
internal sealed class InputDialog : VaultForm
{
	/// <summary>Width of the prompt's frame and of the text box, at zoom 1: a longer prompt wraps.</summary>
	private const int ContentWidth = 380;

	private readonly ScalingLabel promptLabel;
	private readonly ScalingTextBox textBox;
	private readonly ScalingButton okButton;
	private readonly ScalingButton cancelButton;

	/// <summary>
	/// Initializes a new instance of the InputDialog class. Created by <see cref="FormFactory.Create{TForm}"/>, then
	/// given its <see cref="Prompt"/>, title (<see cref="Form.Text"/>) and <see cref="InputValue"/>.
	/// </summary>
	public InputDialog(MainForm instance) : base(instance.Services)
	{
		this.Owner = instance;

		float zoom = this.UserZoomForLayout();
		int Zoomed(int value) => (int)Math.Round(value * zoom);
		int width = Zoomed(ContentWidth);
		var font = this.FontService.GetFontLight(CaravanLook.TextSize, zoom);

		// The prompt in a caravan frame as wide as the text box, one character of space inside it on the left and right,
		// as the instructions of Vault Maintenance.
		this.promptLabel = new ScalingLabel()
		{
			Name = nameof(this.promptLabel),
			AutoSize = true,
			Dock = DockStyle.Fill,
			Font = font,
			Margin = new Padding(0, 0, 0, Zoomed(8)),
			MaximumSize = new Size(width, 0),
			MinimumSize = new Size(width, 0),
			UseMnemonic = false,
		};
		int space = CaravanFrame.Border + CaravanFrame.CharacterWidth(font);
		CaravanFrame.ApplyTo(this.promptLabel, new Padding(space, CaravanFrame.Border, space, CaravanFrame.Border));

		this.textBox = new ScalingTextBox()
		{
			Name = nameof(this.textBox),
			Dock = DockStyle.Fill,
			Font = font,
			Margin = new Padding(0),
			Width = width,
		};

		this.okButton = CaravanDialog.Button(this.FontService, zoom, nameof(this.okButton), Resources.GlobalOK);
		this.okButton.Click += this.OkButton_Click;
		this.cancelButton = CaravanDialog.Button(this.FontService, zoom, nameof(this.cancelButton), Resources.GlobalCancel);
		this.cancelButton.DialogResult = DialogResult.Cancel;
		// Also closes the dialog shown modeless (automation).
		this.cancelButton.Click += (_, _) => this.Close();

		var buttons = new FlowLayoutPanel()
		{
			Anchor = AnchorStyles.None,
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			FlowDirection = FlowDirection.LeftToRight,
			Margin = new Padding(0, Zoomed(14), 0, 0),
			WrapContents = false,
		};
		buttons.Controls.AddRange([this.okButton, this.cancelButton]);

		var layout = new TableLayoutPanel()
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			ColumnCount = 1,
			Margin = new Padding(0),
		};
		layout.Controls.AddRange([this.promptLabel, this.textBox, buttons]);

		this.SuspendLayout();
		CaravanDialog.SetUpWindow(this, nameof(InputDialog));
		// Enter accepts the text, Escape cancels.
		this.AcceptButton = this.okButton;
		this.CancelButton = this.cancelButton;
		this.Font = font;
		this.Padding = CaravanDialog.ContentPadding(zoom);
		layout.Location = new Point(this.Padding.Left, this.Padding.Top);
		this.Controls.Add(layout);
		this.DrawCustomBorder = true;
		CaravanLook.Apply(this, layout, buttons);
		this.ResumeLayout(false);
		this.PerformLayout();

		// The window follows its table: a longer prompt makes it higher.
		this.FitAround(layout);
	}

	/// <summary>The text above the text box.</summary>
	public string Prompt
	{
		get => this.promptLabel.Text;
		set => this.promptLabel.Text = value;
	}

	/// <summary>The text in the text box: the default text before the dialog is shown, the user's after.</summary>
	public string InputValue
	{
		get => this.textBox.Text;
		set => this.textBox.Text = value;
	}

	/// <summary>
	/// Shows the dialog over the main window and returns the text accepted with OK or Enter, or an empty string when it
	/// was cancelled (Cancel, Escape, the close button).
	/// </summary>
	/// <param name="forms">The form factory, which creates the dialog.</param>
	/// <param name="prompt">The prompt text to display.</param>
	/// <param name="title">The dialog title.</param>
	/// <param name="defaultValue">The text shown first, selected.</param>
	/// <returns>The input value if OK was clicked, otherwise an empty string.</returns>
	public static string Show(FormFactory forms, string prompt, string title, string defaultValue = "")
	{
		using var dialog = forms.Create<InputDialog>();
		dialog.Prompt = prompt;
		dialog.Text = title;
		dialog.InputValue = defaultValue;
		return dialog.ShowDialog() == DialogResult.OK ? dialog.InputValue : string.Empty;
	}

	/// <summary>
	/// The keyboard starts in the text box, its text selected, not on the close button of the border.
	/// </summary>
	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		this.textBox.SelectAll();
		this.ActiveControl = this.textBox;
	}

	/// <summary>Automation answers the dialog like a user: types the queued text and clicks OK, or clicks Cancel.</summary>
	protected override void OnShown(EventArgs e)
	{
		base.OnShown(e);
		if (TQVaultAE.GUI.Automation.AutomationHost.TryTakeInputAnswer(this.Text, out var text))
		{
			if (text is null)
			{
				this.cancelButton.PerformClick();
				return;
			}

			this.textBox.Text = text;
			this.okButton.PerformClick();
		}
	}

	private void OkButton_Click(object? sender, EventArgs e)
	{
		this.DialogResult = DialogResult.OK;
		this.Close();
	}
}
