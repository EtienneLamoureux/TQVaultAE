//-----------------------------------------------------------------------
// <copyright file="VaultMaintenanceDialog.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI
{
	using Properties;
	using System;
	using System.Drawing;
	using System.Globalization;
	using System.IO;
	using System.Text;
	using System.Windows.Forms;
	using TQVaultData;

	/// <summary>
	/// Class for VaultMaintenanceDialog form
	/// </summary>
	internal partial class VaultMaintenanceDialog : VaultForm
	{
		/// <summary>
		/// MessageBoxOptions for right to left reading.
		/// </summary>
		private static MessageBoxOptions rightToLeftOptions = (MessageBoxOptions)0;

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
		private string source;

		/// <summary>
		/// Target vault string
		/// </summary>
		private string target;

		/// <summary>
		/// Initializes a new instance of the VaultMaintenanceDialog class.
		/// </summary>
		public VaultMaintenanceDialog()
		{
			this.InitializeComponent();

			#region Apply custom font

			this.targetTextBox.Font = Program.GetFontAlbertusMTLight(11F);
			this.instructionsLabel.Font = Program.GetFontAlbertusMTLight(11F);
			this.okayButton.Font = Program.GetFontAlbertusMTLight(12F);
			this.cancelButton.Font = Program.GetFontAlbertusMTLight(12F);
			this.vaultListComboBox.Font = Program.GetFontAlbertusMTLight(11F);
			this.newRadioButton.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.selectFunctionGroupBox.Font = Program.GetFontAlbertusMTLight(11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.renameRadioButton.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.copyRadioButton.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.deleteRadioButton.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.sourceLabel.Font = Program.GetFontAlbertusMTLight(11F);
			this.targetLabel.Font = Program.GetFontAlbertusMTLight(11F);
			this.Font = Program.GetFontAlbertusMT(9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

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

			// Set options for Right to Left reading.
			if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
			{
				rightToLeftOptions = MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;
			}

			this.DrawCustomBorder = true;

			// Load the invalid characters
			this.invalidChars = Path.GetInvalidFileNameChars();

			this.GetVaultList();
		}

		/// <summary>
		/// Enumeration of the different maintenance functions
		/// </summary>
		public enum VaultMaintenance
		{
			/// <summary>
			/// Nothing selected
			/// </summary>
			None = 0,

			/// <summary>
			/// Create a new vault
			/// </summary>
			New,

			/// <summary>
			/// Copy a vault
			/// </summary>
			Copy,

			/// <summary>
			/// Delete a vault
			/// </summary>
			Delete,

			/// <summary>
			/// Rename a vault
			/// </summary>
			Rename
		}

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
		public string Source
		{
			get
			{
				return this.source;
			}
		}

		/// <summary>
		/// Gets the target vault string
		/// </summary>
		public string Target
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
			this.Font = new Font(this.Font.FontFamily, this.Font.SizeInPoints * factor.Height, this.Font.Style);
			if (this.selectFunctionGroupBox != null && this.selectFunctionGroupBox.Font != null)
			{
				this.selectFunctionGroupBox.Font = new Font(
				   this.selectFunctionGroupBox.Font.Name,
				   this.selectFunctionGroupBox.Font.SizeInPoints * factor.Height,
				   this.selectFunctionGroupBox.Font.Style);
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
		private void VaultMaintenanceDialogLoad(object sender, EventArgs e)
		{
			this.newRadioButton.Checked = true;
			this.targetTextBox.Text = Resources.MaintenanceNewVault;
			this.targetTextBox.SelectAll();
			this.targetTextBox.Focus();
			this.source = null;
			this.target = null;
		}

		/// <summary>
		/// Gets the list of available vaults and loads them into the drop down list
		/// </summary>
		private void GetVaultList()
		{
			string[] vaults = TQData.GetVaultList();

			// Make sure we have something to add.
			if (vaults != null && vaults.Length > 0)
			{
				// Put Main Vault at the top of the list only if it exists.
				if (Array.IndexOf(vaults, "Main Vault") != -1)
				{
					this.vaultListComboBox.Items.Add("Main Vault");
				}

				foreach (string vault in vaults)
				{
					if (!vault.Equals("Main Vault"))
					{
						// now add everything EXCEPT for main vault
						this.vaultListComboBox.Items.Add(vault);
					}
				}
			}
		}

		/// <summary>
		/// Gets whether a vault file exists in the drop down list
		/// </summary>
		/// <param name="vault">Name of the vault</param>
		/// <returns>true if the vault is in the drop down list</returns>
		private bool IsInList(string vault)
		{
			if (vault == null)
			{
				return false;
			}

			bool found = false;

			foreach (string vaultName in this.vaultListComboBox.Items)
			{
				if (vault.ToUpperInvariant() == vaultName.ToUpperInvariant())
				{
					found = true;
					break;
				}
			}

			return found;
		}

		/// <summary>
		/// Handler for clicking the OK button
		/// Processes the user selection.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void OkayButtonClick(object sender, EventArgs e)
		{
			if (this.action == VaultMaintenance.New)
			{
				// validate the text.
				string target = this.targetTextBox.Text.Trim();

				if (string.IsNullOrEmpty(target))
				{
					MessageBox.Show(Resources.MaintenanceBadName, Resources.MaintenanceInvalidName, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, rightToLeftOptions);
				}
				else if (target.IndexOfAny(this.invalidChars) != -1)
				{
					MessageBox.Show(Resources.MaintenanceBadChars, Resources.MaintenanceInvalidName, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, rightToLeftOptions);
				}
				else
				{
					if (this.IsInList(target))
					{
						MessageBox.Show(
							string.Format(CultureInfo.CurrentCulture, Resources.MaintenanceExists, target),
							Resources.MaintenanceDuplicate,
							MessageBoxButtons.OK,
							MessageBoxIcon.None,
							MessageBoxDefaultButton.Button1,
							rightToLeftOptions);
						this.targetTextBox.SelectAll();
						this.targetTextBox.Focus();
					}
					else
					{
						this.target = target;
						this.DialogResult = DialogResult.OK;
						Close();
					}
				}
			}
			else if (this.action == VaultMaintenance.Copy)
			{
				if (this.vaultListComboBox.SelectedItem == null)
				{
					return;
				}

				// validate the text.
				string target = this.targetTextBox.Text.Trim();
				string source = this.vaultListComboBox.SelectedItem.ToString();

				if (string.IsNullOrEmpty(target))
				{
					MessageBox.Show(Resources.MaintenanceNoCopyTarget, Resources.MaintenanceInvalidName, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, rightToLeftOptions);
				}
				else if (source == null || !this.IsInList(source))
				{
					MessageBox.Show(Resources.MaintenanceSourceNoExist, Resources.MaintenanceInvalidSourceName, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, rightToLeftOptions);
				}
				else
				{
					if (this.IsInList(target))
					{
						MessageBox.Show(
							string.Format(CultureInfo.CurrentCulture, Resources.MaintenanceExists, target),
							Resources.MaintenanceDuplicate,
							MessageBoxButtons.OK,
							MessageBoxIcon.None,
							MessageBoxDefaultButton.Button1,
							rightToLeftOptions);
						this.targetTextBox.SelectAll();
						this.targetTextBox.Focus();
					}
					else
					{
						this.target = target;
						this.source = source;
						this.DialogResult = DialogResult.OK;
						Close();
					}
				}
			}
			else if (this.action == VaultMaintenance.Delete)
			{
				if (this.vaultListComboBox.SelectedItem == null)
				{
					return;
				}

				string source = this.vaultListComboBox.SelectedItem.ToString();

				if (!this.IsInList(source))
				{
					MessageBox.Show(Resources.MaintenanceSourceNoExist, Resources.MaintenanceInvalidSourceName, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, rightToLeftOptions);
				}
				else
				{
					this.source = source;
					this.DialogResult = DialogResult.OK;
					Close();
				}
			}
			else if (this.action == VaultMaintenance.Rename)
			{
				if (this.vaultListComboBox.SelectedItem == null)
				{
					return;
				}

				// validate the text.
				string target = this.targetTextBox.Text.Trim();
				string source = this.vaultListComboBox.SelectedItem.ToString();

				if (string.IsNullOrEmpty(target))
				{
					MessageBox.Show(Resources.MaintenanceBadName, Resources.MaintenanceInvalidName, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, rightToLeftOptions);
				}
				else if (source == null || !this.IsInList(source))
				{
					MessageBox.Show(Resources.MaintenanceSourceNoExist, Resources.MaintenanceInvalidSourceName, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, rightToLeftOptions);
				}
				else
				{
					if (this.IsInList(target))
					{
						MessageBox.Show(
							string.Format(CultureInfo.CurrentCulture, Resources.MaintenanceExists, target),
							Resources.MaintenanceDuplicate,
							MessageBoxButtons.OK,
							MessageBoxIcon.None,
							MessageBoxDefaultButton.Button1,
							rightToLeftOptions);
						this.targetTextBox.SelectAll();
						this.targetTextBox.Focus();
					}
					else
					{
						this.target = target;
						this.source = source;
						this.DialogResult = DialogResult.OK;
						Close();
					}
				}
			}
		}

		/// <summary>
		/// Handler for clicking the cancel button.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void CancelButtonClick(object sender, EventArgs e)
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
		private void RenameRadioButtonCheckedChanged(object sender, EventArgs e)
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
		private void NewRadioButtonCheckedChanged(object sender, EventArgs e)
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
		private void DeleteRadioButtonCheckedChanged(object sender, EventArgs e)
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
		private void CopyRadioButtonCheckedChanged(object sender, EventArgs e)
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
		private void VaultListComboBoxSelectedIndexChanged(object sender, EventArgs e)
		{
			if (!this.deleteRadioButton.Checked)
			{
				this.targetTextBox.Text = this.vaultListComboBox.SelectedItem.ToString();
				this.targetTextBox.SelectAll();
				this.targetTextBox.Focus();
			}
		}
	}
}