//-----------------------------------------------------------------------
// <copyright file="InputDialog.cs" company="None">
//     Copyright (c) TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TQVaultAE.GUI;

using System;
using System.Windows.Forms;

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
		this.textBox = new TextBox();
		this.okButton = new Button();
		this.cancelButton = new Button();
		this.promptLabel = new Label();

		this.SuspendLayout();

		// promptLabel
		this.promptLabel.AutoSize = true;
		this.promptLabel.Location = new System.Drawing.Point(12, 9);
		this.promptLabel.Name = "promptLabel";
		this.promptLabel.Size = new System.Drawing.Size(100, 13);
		this.promptLabel.TabIndex = 0;
		this.promptLabel.Text = prompt;

		// textBox
		this.textBox.Location = new System.Drawing.Point(12, 32);
		this.textBox.Name = "textBox";
		this.textBox.Size = new System.Drawing.Size(260, 20);
		this.textBox.TabIndex = 1;
		this.textBox.Text = defaultValue;

		// okButton
		this.okButton.Location = new System.Drawing.Point(98, 65);
		this.okButton.Name = "okButton";
		this.okButton.Size = new System.Drawing.Size(75, 23);
		this.okButton.TabIndex = 2;
		this.okButton.Text = "OK";
		this.okButton.UseVisualStyleBackColor = true;
		this.okButton.Click += this.OkButton_Click;

		// cancelButton
		this.cancelButton.DialogResult = DialogResult.Cancel;
		this.cancelButton.Location = new System.Drawing.Point(179, 65);
		this.cancelButton.Name = "cancelButton";
		this.cancelButton.Size = new System.Drawing.Size(75, 23);
		this.cancelButton.TabIndex = 3;
		this.cancelButton.Text = "Cancel";
		this.cancelButton.UseVisualStyleBackColor = true;

		// InputDialog
		this.AcceptButton = this.okButton;
		this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
		this.AutoScaleMode = AutoScaleMode.Font;
		this.CancelButton = this.cancelButton;
		this.ClientSize = new System.Drawing.Size(284, 101);
		this.Controls.Add(this.cancelButton);
		this.Controls.Add(this.okButton);
		this.Controls.Add(this.textBox);
		this.Controls.Add(this.promptLabel);
		this.FormBorderStyle = FormBorderStyle.FixedDialog;
		this.MaximizeBox = false;
		this.MinimizeBox = false;
		this.Name = "InputDialog";
		this.StartPosition = FormStartPosition.CenterParent;
		this.Text = title;

		this.ResumeLayout(false);
		this.PerformLayout();
	}

	private void OkButton_Click(object sender, EventArgs e)
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
