using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Infrastructure;

//-----------------------------------------------------------------------
// <copyright file="VaultMaintenanceDialog.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Dialogs;

using System;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Presentation;
using TQVaultAE.GUI.Components.Scaling;

/// <summary>
/// Class for VaultMaintenanceDialog form
/// </summary>
internal sealed partial class VaultMaintenanceDialog : VaultForm
{
	/// <summary>
	/// Invalid filename characters
	/// </summary>
	private char[] invalidChars;

	/// <summary>
	/// Dialog action.  Holds what the user has selected.
	/// </summary>
	private VaultMaintenance action;

	/// <summary>
	/// Source vault string
	/// </summary>
	private string? source;

	/// <summary>
	/// Target vault string
	/// </summary>
	private string? target;

	/// <summary>
	/// Validates the maintenance requests.
	/// </summary>
	private readonly IVaultService vaultService;

	/// <summary>
	/// The main window, whose new vaults not saved yet have no file but take their name.
	/// </summary>
	private readonly MainForm mainForm;

	/// <summary>
	/// Initializes a new instance of the VaultMaintenanceDialog class.
	/// </summary>
	public VaultMaintenanceDialog(MainForm instance, IVaultService vaultService) : base(instance.Services)
	{
		this.Owner = instance;
		this.mainForm = instance;
		this.vaultService = vaultService;

		this.InitializeComponent();

		#region Apply custom font

		this.targetTextBox.Font = FontService.GetFontLight(11F);
		this.instructionsLabel.Font = FontService.GetFontLight(11F);
		this.okayButton.Font = FontService.GetFontLight(12F);
		this.cancelButton.Font = FontService.GetFontLight(12F);
		this.vaultListComboBox.Font = FontService.GetFontLight(11F);
		this.newRadioButton.Font = FontService.GetFontLight(11.25F);
		this.selectFunctionGroupBox.Font = FontService.GetFontLight(11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
		this.renameRadioButton.Font = FontService.GetFontLight(11.25F);
		this.copyRadioButton.Font = FontService.GetFontLight(11.25F);
		this.deleteRadioButton.Font = FontService.GetFontLight(11.25F);
		this.sourceLabel.Font = FontService.GetFontLight(11F);
		this.targetLabel.Font = FontService.GetFontLight(11F);
		this.Font = FontService.GetFont(9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

		#endregion

		// Load the localized resources
		this.Text = Resources.MaintenanceText;
		this.copyRadioButton.Text = Resources.MaintenanceRbCopy;
		this.deleteRadioButton.Text = Resources.MaintenanceRbDelete;
		this.newRadioButton.Text = Resources.MaintenanceRbNew;
		this.renameRadioButton.Text = Resources.MaintenanceRbRename;
		this.selectFunctionGroupBox.Text = Resources.MaintenanceGroup;
		this.selectFunctionGroupBox.ForeColor = Color.White;
		this.cancelButton.Text = Resources.GlobalCancel;
		this.okayButton.Text = Resources.GlobalOK;
		this.instructionsLabel.Text = Resources.MaintenanceInstructions;
		this.sourceLabel.Text = Resources.MaintenanceSource;
		this.targetLabel.Text = Resources.MaintenanceTarget;

		// The text boxes and the function group in caravan frames. The Source and Target labels take the height of the
		// box they name, from their left to just before it.
		// The function group and the instructions keep one character of space inside their frame, on the left and right:
		// the instructions' three lines need their height.
		CaravanFrame.ApplyTo(this.selectFunctionGroupBox, CharacterWidth(this.newRadioButton.Font));
		int space = CaravanFrame.Border + CharacterWidth(this.instructionsLabel.Font);
		CaravanFrame.ApplyTo(this.instructionsLabel, new Padding(space, CaravanFrame.Border, space, CaravanFrame.Border));
		CaravanFrame.FrameLabel(this.sourceLabel, this.vaultListComboBox);
		CaravanFrame.FrameLabel(this.targetLabel, this.targetTextBox);
		CaravanLook.Apply(this);

		this.DrawCustomBorder = true;

		// Load the invalid characters
		this.invalidChars = this.PathIO.GetInvalidFileNameChars();

		this.GetVaultList();
	}

	/// <summary>The width of one character of <paramref name="font"/>.</summary>
	private static int CharacterWidth(Font font)
		=> TextRenderer.MeasureText("0", font, Size.Empty, TextFormatFlags.NoPadding).Width;

	/// <summary>
	/// Gets the user selected function.
	/// </summary>
	public VaultMaintenance Action
	{
		get
		{
			return this.action;
		}
	}

	/// <summary>
	/// Gets the source vault string
	/// </summary>
	public string? Source
	{
		get
		{
			return this.source;
		}
	}

	/// <summary>
	/// Gets the target vault string
	/// </summary>
	public string? Target
	{
		get
		{
			return this.target;
		}
	}

	/// <summary>
	/// Override of ScaleControl which supports groupbox scaling.
	/// </summary>
	/// <param name="factor">SizeF for the scale factor</param>
	/// <param name="specified">BoundsSpecified value.</param>
	protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
	{
		this.Font = ZoomScope.ScaleFont(this.Font, factor.Height);
		if (this.selectFunctionGroupBox is not null && this.selectFunctionGroupBox.Font is not null)
		{
			this.selectFunctionGroupBox.Font = ZoomScope.ScaleFont(this.selectFunctionGroupBox.Font, factor.Height);
		}

		base.ScaleControl(factor, specified);
	}

	/// <summary>
	/// Converts the invalid character array to printable array.
	/// Character codes 0-31 are included in the array, but cannot be shown on screen.
	/// The ToString() fails to print all of the characters because 0 is a string terminator.
	/// </summary>
	/// <param name="invalidCharacters">invalid character array</param>
	/// <returns>string with only the printable characters</returns>
	private static string DisplayInvalidChars(char[] invalidCharacters)
	{
		StringBuilder temp = new StringBuilder();

		foreach (char currentCharacter in invalidCharacters)
		{
			// Filter out the control characters
			if (!Char.IsControl(currentCharacter))
			{
				temp.Append(currentCharacter);
			}
		}

		return temp.ToString();
	}

	/// <summary>
	/// Handler for loading the vault maintenance dialog
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void VaultMaintenanceDialogLoad(object? sender, EventArgs e)
	{
		// The zoom is applied by now (the dialog is scaled before it is shown).
		this.FitInstructions();
		this.newRadioButton.Checked = true;
		this.targetTextBox.Text = Resources.MaintenanceNewVault;
		this.targetTextBox.SelectAll();
		this.targetTextBox.Focus();
		this.source = null;
		this.target = null;
	}

	/// <summary>
	/// On a monitor of another DPI the fonts change: the instructions may need more lines.
	/// </summary>
	protected override void OnDpiChanged(DpiChangedEventArgs e)
	{
		base.OnDpiChanged(e);
		this.FitInstructions();
	}

	/// <summary>
	/// Makes the instructions frame tall enough for the longest instructions, the dialog growing below it. The frame is
	/// sized for the design font, and a font scaled for a zoom or a DPI grows its text a little more than the frame
	/// (pixel rounding of the glyphs): at a zoom of 1.2 the first line wrapped and the last one was cut. Never shrinks,
	/// so the design layout stays as it is when everything fits.
	/// </summary>
	private void FitInstructions()
	{
		var label = this.instructionsLabel;
		int width = label.ClientSize.Width - label.Padding.Horizontal;
		if (width <= 0)
			return;

		string[] texts = [
			Resources.MaintenanceRename,
			string.Concat(Resources.MaintenanceNew, DisplayInvalidChars(this.invalidChars)),
			Resources.MaintenanceDelete,
			Resources.MaintenanceCopy,
		];
		var flags = TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl;
		int needed = texts.Max(t => TextRenderer.MeasureText(t, label.Font, new Size(width, int.MaxValue), flags).Height)
			+ label.Padding.Vertical;
		int delta = needed - label.Height;
		if (delta <= 0)
			return;

		int bottom = label.Bottom;
		this.SuspendLayout();
		foreach (Control control in this.Controls)
		{
			if (control != label && control.Top >= bottom)
				control.Top += delta;
		}

		label.Height = needed;
		this.Height += delta;
		this.ResumeLayout();
	}

	/// <summary>
	/// Gets the list of available vaults and loads them into the drop down list
	/// </summary>
	private void GetVaultList()
	{
		// Main Vault at the top of the list only if it exists.
		var choices = this.vaultService.BuildVaultChoices(GamePathResolver.GetVaultList(), alwaysIncludeMainVault: false);
		this.vaultListComboBox.Items.AddRange(choices.ToArray<object>());
	}

	/// <summary>
	/// Handler for clicking the OK button
	/// Processes the user selection.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void OkayButtonClick(object? sender, EventArgs e)
	{
		if (this.action == VaultMaintenance.None)
		{
			return;
		}

		bool needsSource = this.action != VaultMaintenance.New;
		bool needsTarget = this.action != VaultMaintenance.Delete;

		if (needsSource && this.vaultListComboBox.SelectedItem is null)
		{
			return;
		}

		string? target = needsTarget ? this.targetTextBox.Text.Trim() : null;
		string? source = needsSource ? this.vaultListComboBox.SelectedItemText() : null;

		// The vault files, and the new vaults of the main window not saved yet: their name is taken too.
		var existingVaults = this.vaultListComboBox.Items.Cast<object>().Select(item => item.ToString() ?? string.Empty)
			.Concat(this.mainForm.KnownVaultNames);
		var error = this.vaultService.ValidateMaintenance(this.action, source, target, existingVaults);

		switch (error)
		{
			case VaultMaintenanceError.None:
				this.target = target;
				this.source = source;
				this.DialogResult = DialogResult.OK;
				Close();
				break;

			case VaultMaintenanceError.EmptyName:
				string emptyMessage = this.action == VaultMaintenance.Copy ? Resources.MaintenanceNoCopyTarget : Resources.MaintenanceBadName;
				this.Services.MessageBoxes.Inform(emptyMessage, Resources.MaintenanceInvalidName);
				break;

			case VaultMaintenanceError.InvalidCharacters:
				this.Services.MessageBoxes.Inform(Resources.MaintenanceBadChars, Resources.MaintenanceInvalidName);
				break;

			case VaultMaintenanceError.SourceMissing:
				this.Services.MessageBoxes.Inform(Resources.MaintenanceSourceNoExist, Resources.MaintenanceInvalidSourceName);
				break;

			case VaultMaintenanceError.DuplicateName:
				this.Services.MessageBoxes.Inform(string.Format(CultureInfo.CurrentCulture, Resources.MaintenanceExists, target), Resources.MaintenanceDuplicate);
				this.targetTextBox.SelectAll();
				this.targetTextBox.Focus();
				break;
		}
	}

	/// <summary>
	/// Handler for clicking the cancel button.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void CancelButtonClick(object? sender, EventArgs e)
	{
		this.action = VaultMaintenance.None;
		this.source = null;
		this.target = null;
		this.DialogResult = DialogResult.Cancel;
		Close();
	}

	/// <summary>
	/// Handler for clicking the rename radio button
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void RenameRadioButtonCheckedChanged(object? sender, EventArgs e)
	{
		if (this.renameRadioButton.Checked)
		{
			if (this.action != VaultMaintenance.Rename)
			{
				this.action = VaultMaintenance.Rename;
				this.instructionsLabel.Text = Resources.MaintenanceRename;
				this.targetTextBox.Enabled = true;
				this.targetTextBox.Show();
				this.targetLabel.Show();
				this.vaultListComboBox.Enabled = true;
				this.vaultListComboBox.Show();
				this.sourceLabel.Show();
			}
		}
	}

	/// <summary>
	/// Handler for clicking the new radio button
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void NewRadioButtonCheckedChanged(object? sender, EventArgs e)
	{
		if (this.newRadioButton.Checked)
		{
			if (this.action != VaultMaintenance.New)
			{
				this.action = VaultMaintenance.New;
				this.instructionsLabel.Text = string.Concat(Resources.MaintenanceNew, DisplayInvalidChars(this.invalidChars));
				this.targetTextBox.Enabled = true;
				this.targetTextBox.Show();
				this.targetLabel.Show();
				this.vaultListComboBox.Enabled = false;
				this.vaultListComboBox.Hide();
				this.sourceLabel.Hide();
			}
		}
	}

	/// <summary>
	/// Handler for clicking the delete radio button
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void DeleteRadioButtonCheckedChanged(object? sender, EventArgs e)
	{
		if (this.deleteRadioButton.Checked)
		{
			if (this.action != VaultMaintenance.Delete)
			{
				this.action = VaultMaintenance.Delete;
				this.instructionsLabel.Text = Resources.MaintenanceDelete;
				this.targetTextBox.Enabled = false;
				this.targetTextBox.Hide();
				this.targetLabel.Hide();
				this.vaultListComboBox.Enabled = true;
				this.vaultListComboBox.Show();
				this.sourceLabel.Show();
			}
		}
	}

	/// <summary>
	/// Handler for clicking the copy radio button
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void CopyRadioButtonCheckedChanged(object? sender, EventArgs e)
	{
		if (this.copyRadioButton.Checked)
		{
			if (this.action != VaultMaintenance.Copy)
			{
				this.action = VaultMaintenance.Copy;
				this.instructionsLabel.Text = Resources.MaintenanceCopy;
				this.targetTextBox.Enabled = true;
				this.targetTextBox.Show();
				this.targetLabel.Show();
				this.vaultListComboBox.Enabled = true;
				this.vaultListComboBox.Show();
				this.sourceLabel.Show();
			}
		}
	}

	/// <summary>
	/// Handler for changing the selected vault on the drop down list
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void VaultListComboBoxSelectedIndexChanged(object? sender, EventArgs e)
	{
		if (!this.deleteRadioButton.Checked)
		{
			this.targetTextBox.Text = this.vaultListComboBox.SelectedItemText();
			this.targetTextBox.SelectAll();
			this.targetTextBox.Focus();
		}
	}
}