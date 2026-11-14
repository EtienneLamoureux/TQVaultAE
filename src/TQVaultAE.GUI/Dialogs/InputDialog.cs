//-----------------------------------------------------------------------
// <copyright file="InputDialog.cs" company="None">
//     Copyright (c) TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TQVaultAE.GUI.Dialogs;

using System;
using System.Windows.Forms;
using TQVaultAE.Presentation;

/// <summary>
/// A simple input dialog that replaces Microsoft.VisualBasic.Interaction.InputBox
/// </summary>
public class InputDialog : Form
{
	private TextBox textBox;
	private Button okButton;
	private Button cancelButton;
	private Label promptLabel;

	/// <summary>
	/// Gets the input value from the text box
	/// </summary>
	public string InputValue => this.textBox.Text;

	/// <summary>
	/// Initializes a new instance of the InputDialog class
	/// </summary>
	/// <param name="prompt">The prompt text to display</param>
	/// <param name="title">The dialog title</param>
	/// <param name="defaultValue">The default value for the text box</param>
	public InputDialog(string prompt, string title, string defaultValue = "")
	{
		// Laid out by a table and sized to its content: a long prompt or a translation widens the dialog instead of
		// being cut, and the layout follows the DPI.
		this.promptLabel = new Label() { AutoSize = true, Text = prompt, Margin = new Padding(0, 0, 0, 6), Anchor = AnchorStyles.Left };
		this.textBox = new TextBox() { Text = defaultValue, Dock = DockStyle.Fill, MinimumSize = new System.Drawing.Size(260, 0), Margin = new Padding(0, 0, 0, 10) };
		this.okButton = new Button() { Text = Resources.GlobalOK, AutoSize = true, MinimumSize = new System.Drawing.Size(75, 23), UseVisualStyleBackColor = true };
		this.cancelButton = new Button() { Text = Resources.GlobalCancel, AutoSize = true, MinimumSize = new System.Drawing.Size(75, 23), UseVisualStyleBackColor = true, DialogResult = DialogResult.Cancel };
		this.okButton.Click += this.OkButton_Click;

		var buttons = new FlowLayoutPanel()
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			FlowDirection = FlowDirection.RightToLeft,
			Anchor = AnchorStyles.Right,
			Margin = new Padding(0),
			WrapContents = false,
		};
		buttons.Controls.Add(this.cancelButton);
		buttons.Controls.Add(this.okButton);

		var layout = new TableLayoutPanel()
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			ColumnCount = 1,
			RowCount = 3,
			Dock = DockStyle.Fill,
			Padding = new Padding(12),
		};
		layout.Controls.Add(this.promptLabel, 0, 0);
		layout.Controls.Add(this.textBox, 0, 1);
		layout.Controls.Add(buttons, 0, 2);

		this.SuspendLayout();
		this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
		this.AutoScaleMode = AutoScaleMode.Dpi;
		this.AutoSize = true;
		this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
		this.AcceptButton = this.okButton;
		this.CancelButton = this.cancelButton;
		this.Controls.Add(layout);
		this.FormBorderStyle = FormBorderStyle.FixedDialog;
		this.MaximizeBox = false;
		this.MinimizeBox = false;
		this.Name = "InputDialog";
		this.ShowInTaskbar = false;
		this.StartPosition = FormStartPosition.CenterParent;
		this.Text = title;
		this.ResumeLayout(false);
		this.PerformLayout();
	}

#if DEBUG
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
#endif

	private void OkButton_Click(object? sender, EventArgs e)
	{
		this.DialogResult = DialogResult.OK;
		this.Close();
	}

	/// <summary>
	/// Shows an input dialog box
	/// </summary>
	/// <param name="prompt">The prompt text to display</param>
	/// <param name="title">The dialog title</param>
	/// <param name="defaultValue">The default value for the text box</param>
	/// <returns>The input value if OK was clicked, otherwise an empty string</returns>
	public static string Show(string prompt, string title, string defaultValue = "")
	{
		using var dialog = new InputDialog(prompt, title, defaultValue);
		return dialog.ShowDialog() == DialogResult.OK ? dialog.InputValue : string.Empty;
	}
}
