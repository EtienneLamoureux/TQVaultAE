//-----------------------------------------------------------------------
// <copyright file="VaultMaintenanceDialog.Designer.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI
{
	/// <summary>
	/// Form Designer class for VaultMaintenanceDialog
	/// </summary>
	internal partial class VaultMaintenanceDialog
	{
		/// <summary>
		/// Vault list combo box.  Displays the list of available vaults
		/// </summary>
		private ScalingComboBox vaultListComboBox;

		/// <summary>
		/// Radio Button for creating a new vault
		/// </summary>
		private ScalingRadioButton newRadioButton;

		/// <summary>
		/// Group box for radio button selections
		/// </summary>
		private System.Windows.Forms.GroupBox selectFunctionGroupBox;

		/// <summary>
		/// Radio Button for renaming a vault
		/// </summary>
		private ScalingRadioButton renameRadioButton;

		/// <summary>
		/// Radio Button for copying a vault
		/// </summary>
		private ScalingRadioButton copyRadioButton;

		/// <summary>
		/// Radio Button for deleting a vault
		/// </summary>
		private ScalingRadioButton deleteRadioButton;

		/// <summary>
		/// Source vault label
		/// </summary>
		private ScalingLabel sourceLabel;

		/// <summary>
		/// Target vault label
		/// </summary>
		private ScalingLabel targetLabel;

		/// <summary>
		/// Text box for the target vault.
		/// </summary>
		private ScalingTextBox targetTextBox;

		/// <summary>
		/// Label for instructions.  Changes based on function
		/// </summary>
		private ScalingLabel instructionsLabel;

		/// <summary>
		/// OK Button control
		/// </summary>
		private ScalingButton okayButton;

		/// <summary>
		/// Cancel Button control
		/// </summary>
		private ScalingButton cancelButton;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (this.components != null))
			{
				this.components.Dispose();
			}

			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VaultMaintenanceDialog));
            this.targetTextBox = new TQVaultAE.GUI.ScalingTextBox();
            this.instructionsLabel = new TQVaultAE.GUI.ScalingLabel();
            this.okayButton = new TQVaultAE.GUI.ScalingButton();
            this.cancelButton = new TQVaultAE.GUI.ScalingButton();
            this.vaultListComboBox = new TQVaultAE.GUI.ScalingComboBox();
            this.newRadioButton = new TQVaultAE.GUI.ScalingRadioButton();
            this.selectFunctionGroupBox = new System.Windows.Forms.GroupBox();
            this.renameRadioButton = new TQVaultAE.GUI.ScalingRadioButton();
            this.copyRadioButton = new TQVaultAE.GUI.ScalingRadioButton();
            this.deleteRadioButton = new TQVaultAE.GUI.ScalingRadioButton();
            this.sourceLabel = new TQVaultAE.GUI.ScalingLabel();
            this.targetLabel = new TQVaultAE.GUI.ScalingLabel();
            this.selectFunctionGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // targetTextBox
            // 
            this.targetTextBox.Font = new System.Drawing.Font("Albertus MT Light", 11F);
            this.targetTextBox.Location = new System.Drawing.Point(90, 316);
            this.targetTextBox.MaxLength = 256;
            this.targetTextBox.Name = "targetTextBox";
            this.targetTextBox.Size = new System.Drawing.Size(329, 25);
            this.targetTextBox.TabIndex = 0;
            this.targetTextBox.Text = "target";
            // 
            // instructionsLabel
            // 
            this.instructionsLabel.Font = new System.Drawing.Font("Albertus MT Light", 11F);
            this.instructionsLabel.Location = new System.Drawing.Point(12, 178);
            this.instructionsLabel.Name = "instructionsLabel";
            this.instructionsLabel.Size = new System.Drawing.Size(440, 74);
            this.instructionsLabel.TabIndex = 1;
            this.instructionsLabel.Text = "Instructions";
            // 
            // okayButton
            // 
            this.okayButton.BackColor = System.Drawing.Color.Transparent;
            this.okayButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("okayButton.DownBitmap")));
            this.okayButton.FlatAppearance.BorderSize = 0;
            this.okayButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.okayButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.okayButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.okayButton.Font = new System.Drawing.Font("Albertus MT Light", 12F);
            this.okayButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.okayButton.Image = ((System.Drawing.Image)(resources.GetObject("okayButton.Image")));
            this.okayButton.Location = new System.Drawing.Point(70, 377);
            this.okayButton.Name = "okayButton";
            this.okayButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("okayButton.OverBitmap")));
            this.okayButton.Size = new System.Drawing.Size(137, 30);
            this.okayButton.SizeToGraphic = false;
            this.okayButton.TabIndex = 2;
            this.okayButton.Text = "OK";
            this.okayButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("okayButton.UpBitmap")));
            this.okayButton.UseCustomGraphic = true;
            this.okayButton.UseVisualStyleBackColor = false;
            this.okayButton.Click += new System.EventHandler(this.OkayButtonClick);
            // 
            // cancelButton
            // 
            this.cancelButton.BackColor = System.Drawing.Color.Transparent;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("cancelButton.DownBitmap")));
            this.cancelButton.FlatAppearance.BorderSize = 0;
            this.cancelButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.cancelButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cancelButton.Font = new System.Drawing.Font("Albertus MT Light", 12F);
            this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.cancelButton.Image = ((System.Drawing.Image)(resources.GetObject("cancelButton.Image")));
            this.cancelButton.Location = new System.Drawing.Point(258, 377);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("cancelButton.OverBitmap")));
            this.cancelButton.Size = new System.Drawing.Size(137, 30);
            this.cancelButton.SizeToGraphic = false;
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("cancelButton.UpBitmap")));
            this.cancelButton.UseCustomGraphic = true;
            this.cancelButton.UseVisualStyleBackColor = false;
            this.cancelButton.Click += new System.EventHandler(this.CancelButtonClick);
            // 
            // vaultListComboBox
            // 
            this.vaultListComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.vaultListComboBox.Font = new System.Drawing.Font("Albertus MT Light", 11F);
            this.vaultListComboBox.FormattingEnabled = true;
            this.vaultListComboBox.Location = new System.Drawing.Point(90, 275);
            this.vaultListComboBox.Name = "vaultListComboBox";
            this.vaultListComboBox.Size = new System.Drawing.Size(329, 25);
            this.vaultListComboBox.TabIndex = 4;
            this.vaultListComboBox.SelectedIndexChanged += new System.EventHandler(this.VaultListComboBoxSelectedIndexChanged);
            // 
            // newRadioButton
            // 
            this.newRadioButton.AutoSize = true;
            this.newRadioButton.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.newRadioButton.Location = new System.Drawing.Point(6, 20);
            this.newRadioButton.Name = "newRadioButton";
            this.newRadioButton.Size = new System.Drawing.Size(150, 21);
            this.newRadioButton.TabIndex = 5;
            this.newRadioButton.TabStop = true;
            this.newRadioButton.Text = "Create a New Vault";
            this.newRadioButton.UseVisualStyleBackColor = true;
            this.newRadioButton.CheckedChanged += new System.EventHandler(this.NewRadioButtonCheckedChanged);
            // 
            // selectFunctionGroupBox
            // 
            this.selectFunctionGroupBox.Controls.Add(this.renameRadioButton);
            this.selectFunctionGroupBox.Controls.Add(this.copyRadioButton);
            this.selectFunctionGroupBox.Controls.Add(this.deleteRadioButton);
            this.selectFunctionGroupBox.Controls.Add(this.newRadioButton);
            this.selectFunctionGroupBox.Font = new System.Drawing.Font("Albertus MT Light", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.selectFunctionGroupBox.Location = new System.Drawing.Point(119, 38);
            this.selectFunctionGroupBox.Name = "selectFunctionGroupBox";
            this.selectFunctionGroupBox.Size = new System.Drawing.Size(225, 126);
            this.selectFunctionGroupBox.TabIndex = 6;
            this.selectFunctionGroupBox.TabStop = false;
            this.selectFunctionGroupBox.Text = "Select Function";
            // 
            // renameRadioButton
            // 
            this.renameRadioButton.AutoSize = true;
            this.renameRadioButton.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.renameRadioButton.Location = new System.Drawing.Point(6, 92);
            this.renameRadioButton.Name = "renameRadioButton";
            this.renameRadioButton.Size = new System.Drawing.Size(124, 21);
            this.renameRadioButton.TabIndex = 8;
            this.renameRadioButton.TabStop = true;
            this.renameRadioButton.Text = "Rename a Vault";
            this.renameRadioButton.UseVisualStyleBackColor = true;
            this.renameRadioButton.CheckedChanged += new System.EventHandler(this.RenameRadioButtonCheckedChanged);
            // 
            // copyRadioButton
            // 
            this.copyRadioButton.AutoSize = true;
            this.copyRadioButton.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.copyRadioButton.Location = new System.Drawing.Point(6, 68);
            this.copyRadioButton.Name = "copyRadioButton";
            this.copyRadioButton.Size = new System.Drawing.Size(109, 21);
            this.copyRadioButton.TabIndex = 7;
            this.copyRadioButton.TabStop = true;
            this.copyRadioButton.Text = "Copy a Vault";
            this.copyRadioButton.UseVisualStyleBackColor = true;
            this.copyRadioButton.CheckedChanged += new System.EventHandler(this.CopyRadioButtonCheckedChanged);
            // 
            // deleteRadioButton
            // 
            this.deleteRadioButton.AutoSize = true;
            this.deleteRadioButton.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.deleteRadioButton.Location = new System.Drawing.Point(6, 44);
            this.deleteRadioButton.Name = "deleteRadioButton";
            this.deleteRadioButton.Size = new System.Drawing.Size(114, 21);
            this.deleteRadioButton.TabIndex = 6;
            this.deleteRadioButton.TabStop = true;
            this.deleteRadioButton.Text = "Delete a Vault";
            this.deleteRadioButton.UseVisualStyleBackColor = true;
            this.deleteRadioButton.CheckedChanged += new System.EventHandler(this.DeleteRadioButtonCheckedChanged);
            // 
            // sourceLabel
            // 
            this.sourceLabel.AutoSize = true;
            this.sourceLabel.Font = new System.Drawing.Font("Albertus MT Light", 11F);
            this.sourceLabel.Location = new System.Drawing.Point(12, 278);
            this.sourceLabel.Name = "sourceLabel";
            this.sourceLabel.Size = new System.Drawing.Size(51, 17);
            this.sourceLabel.TabIndex = 7;
            this.sourceLabel.Text = "Source";
            // 
            // targetLabel
            // 
            this.targetLabel.AutoSize = true;
            this.targetLabel.Font = new System.Drawing.Font("Albertus MT Light", 11F);
            this.targetLabel.Location = new System.Drawing.Point(12, 319);
            this.targetLabel.Name = "targetLabel";
            this.targetLabel.Size = new System.Drawing.Size(49, 17);
            this.targetLabel.TabIndex = 8;
            this.targetLabel.Text = "Target";
            // 
            // VaultMaintenanceDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(464, 419);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.targetLabel);
            this.Controls.Add(this.sourceLabel);
            this.Controls.Add(this.selectFunctionGroupBox);
            this.Controls.Add(this.instructionsLabel);
            this.Controls.Add(this.okayButton);
            this.Controls.Add(this.vaultListComboBox);
            this.Controls.Add(this.targetTextBox);
            this.DrawCustomBorder = true;
            this.Font = new System.Drawing.Font("Albertus MT Light", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VaultMaintenanceDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Vault Maintenance";
            this.Load += new System.EventHandler(this.VaultMaintenanceDialogLoad);
            this.Controls.SetChildIndex(this.targetTextBox, 0);
            this.Controls.SetChildIndex(this.vaultListComboBox, 0);
            this.Controls.SetChildIndex(this.okayButton, 0);
            this.Controls.SetChildIndex(this.instructionsLabel, 0);
            this.Controls.SetChildIndex(this.selectFunctionGroupBox, 0);
            this.Controls.SetChildIndex(this.sourceLabel, 0);
            this.Controls.SetChildIndex(this.targetLabel, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.selectFunctionGroupBox.ResumeLayout(false);
            this.selectFunctionGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
	}
}