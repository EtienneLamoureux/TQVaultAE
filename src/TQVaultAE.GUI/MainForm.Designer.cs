//-----------------------------------------------------------------------
// <copyright file="MainForm.Designer.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Drawing;
using TQVaultAE.GUI.Properties;

namespace TQVaultAE.GUI
{
	/// <summary>
	/// MainForm Designer class
	/// </summary>
	internal partial class MainForm
	{
		/// <summary>
		/// Windows Form Exit button
		/// </summary>
		private ScalingButton exitButton;

		/// <summary>
		/// Windows Form Character dropdown
		/// </summary>
		private ScalingComboBox characterComboBox;

		/// <summary>
		/// Windows Form Character Label
		/// </summary>
		private ScalingLabel characterLabel;

		/// <summary>
		/// Windows Form Item Text Panel
		/// </summary>
		private System.Windows.Forms.Panel itemTextPanel;

		/// <summary>
		/// Windows Form item text label
		/// </summary>
		private ScalingLabel itemText;

		/// <summary>
		/// Windows Form Vault List Dropdown
		/// </summary>
		private ScalingComboBox vaultListComboBox;

		/// <summary>
		/// Windows Form Vault Label
		/// </summary>
		private ScalingLabel vaultLabel;

		/// <summary>
		/// Windows Form Configure Button
		/// </summary>
		private ScalingButton configureButton;

		/// <summary>
		/// Windows Form Custom Map Text Label
		/// </summary>
		private ScalingLabel customMapText;

		/// <summary>
		/// Windows Form Panel Selection Button
		/// </summary>
		private ScalingButton panelSelectButton;

		/// <summary>
		/// Windows Form Secondary Vault List Dropdown
		/// </summary>
		private ScalingComboBox secondaryVaultListComboBox;

		/// <summary>
		/// Windows Form About Button
		/// </summary>
		private ScalingButton aboutButton;

		/// <summary>
		/// Windows Form TQVault Title Label
		/// </summary>
		private ScalingLabel titleLabel;

		/// <summary>
		/// Windows Form Search Button
		/// </summary>
		private ScalingButton searchButton;

		/// <summary>
		/// Background worker to load resources.
		/// </summary>
		private System.ComponentModel.BackgroundWorker backgroundWorker1;

		/// <summary>
		/// Timer for fading in the main form.
		/// </summary>
		private System.Windows.Forms.Timer fadeInTimer;

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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.exitButton = new TQVaultAE.GUI.ScalingButton();
            this.characterComboBox = new TQVaultAE.GUI.ScalingComboBox();
            this.characterLabel = new TQVaultAE.GUI.ScalingLabel();
            this.itemTextPanel = new System.Windows.Forms.Panel();
            this.itemText = new TQVaultAE.GUI.ScalingLabel();
            this.vaultListComboBox = new TQVaultAE.GUI.ScalingComboBox();
            this.vaultLabel = new TQVaultAE.GUI.ScalingLabel();
            this.configureButton = new TQVaultAE.GUI.ScalingButton();
            this.customMapText = new TQVaultAE.GUI.ScalingLabel();
            this.panelSelectButton = new TQVaultAE.GUI.ScalingButton();
            this.secondaryVaultListComboBox = new TQVaultAE.GUI.ScalingComboBox();
            this.aboutButton = new TQVaultAE.GUI.ScalingButton();
            this.titleLabel = new TQVaultAE.GUI.ScalingLabel();
            this.searchButton = new TQVaultAE.GUI.ScalingButton();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.fadeInTimer = new System.Windows.Forms.Timer(this.components);
            this.itemTextPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // exitButton
            // 
            this.exitButton.BackColor = System.Drawing.Color.Transparent;
            this.exitButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.exitButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("exitButton.DownBitmap")));
            this.exitButton.FlatAppearance.BorderSize = 0;
            this.exitButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.exitButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.exitButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.exitButton.Font = new System.Drawing.Font("Albertus MT Light", 12F);
            this.exitButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.exitButton.Image = ((System.Drawing.Image)(resources.GetObject("exitButton.Image")));
            this.exitButton.Location = new System.Drawing.Point(434, 24);
            this.exitButton.Name = "exitButton";
            this.exitButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("exitButton.OverBitmap")));
            this.exitButton.Size = new System.Drawing.Size(137, 30);
            this.exitButton.SizeToGraphic = false;
            this.exitButton.TabIndex = 0;
            this.exitButton.Text = "Exit";
            this.exitButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("exitButton.UpBitmap")));
            this.exitButton.UseCustomGraphic = true;
            this.exitButton.UseVisualStyleBackColor = false;
            this.exitButton.Click += new System.EventHandler(this.ExitButtonClick);
            // 
            // characterComboBox
            // 
            this.characterComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.characterComboBox.Font = new System.Drawing.Font("Albertus MT Light", 13F);
            this.characterComboBox.Location = new System.Drawing.Point(777, 72);
            this.characterComboBox.MaxDropDownItems = 10;
            this.characterComboBox.Name = "characterComboBox";
            this.characterComboBox.Size = new System.Drawing.Size(481, 28);
            this.characterComboBox.TabIndex = 1;
            this.characterComboBox.SelectedIndexChanged += new System.EventHandler(this.CharacterComboBoxSelectedIndexChanged);
            // 
            // characterLabel
            // 
            this.characterLabel.BackColor = System.Drawing.Color.Transparent;
            this.characterLabel.Font = new System.Drawing.Font("Albertus MT Light", 11F);
            this.characterLabel.ForeColor = System.Drawing.Color.White;
            this.characterLabel.Location = new System.Drawing.Point(681, 72);
            this.characterLabel.Name = "characterLabel";
            this.characterLabel.Size = new System.Drawing.Size(90, 24);
            this.characterLabel.TabIndex = 2;
            this.characterLabel.Text = "Character:";
            this.characterLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // itemTextPanel
            // 
            this.itemTextPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(188)))), ((int)(((byte)(97)))));
            this.itemTextPanel.Controls.Add(this.itemText);
            this.itemTextPanel.Location = new System.Drawing.Point(6, 606);
            this.itemTextPanel.Name = "itemTextPanel";
            this.itemTextPanel.Size = new System.Drawing.Size(592, 22);
            this.itemTextPanel.TabIndex = 4;
            // 
            // itemText
            // 
            this.itemText.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(41)))), ((int)(((byte)(31)))));
            this.itemText.Font = new System.Drawing.Font("Arial Black", 8.25F);
            this.itemText.Location = new System.Drawing.Point(2, 2);
            this.itemText.Name = "itemText";
            this.itemText.Size = new System.Drawing.Size(576, 18);
            this.itemText.TabIndex = 0;
            // 
            // vaultListComboBox
            // 
            this.vaultListComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.vaultListComboBox.Font = new System.Drawing.Font("Albertus MT Light", 13F);
            this.vaultListComboBox.Location = new System.Drawing.Point(108, 72);
            this.vaultListComboBox.Name = "vaultListComboBox";
            this.vaultListComboBox.Size = new System.Drawing.Size(481, 28);
            this.vaultListComboBox.TabIndex = 5;
            this.vaultListComboBox.SelectedIndexChanged += new System.EventHandler(this.VaultListComboBoxSelectedIndexChanged);
            // 
            // vaultLabel
            // 
            this.vaultLabel.BackColor = System.Drawing.Color.Transparent;
            this.vaultLabel.Font = new System.Drawing.Font("Albertus MT Light", 11F);
            this.vaultLabel.ForeColor = System.Drawing.Color.White;
            this.vaultLabel.Location = new System.Drawing.Point(12, 72);
            this.vaultLabel.Name = "vaultLabel";
            this.vaultLabel.Size = new System.Drawing.Size(90, 24);
            this.vaultLabel.TabIndex = 6;
            this.vaultLabel.Text = "Vault:";
            this.vaultLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // configureButton
            // 
            this.configureButton.BackColor = System.Drawing.Color.Transparent;
            this.configureButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("configureButton.DownBitmap")));
            this.configureButton.FlatAppearance.BorderSize = 0;
            this.configureButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.configureButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.configureButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.configureButton.Font = new System.Drawing.Font("Albertus MT Light", 12F);
            this.configureButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.configureButton.Image = ((System.Drawing.Image)(resources.GetObject("configureButton.Image")));
            this.configureButton.Location = new System.Drawing.Point(6, 24);
            this.configureButton.Name = "configureButton";
            this.configureButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("configureButton.OverBitmap")));
            this.configureButton.Size = new System.Drawing.Size(137, 30);
            this.configureButton.SizeToGraphic = false;
            this.configureButton.TabIndex = 9;
            this.configureButton.Text = "Configure";
            this.configureButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("configureButton.UpBitmap")));
            this.configureButton.UseCustomGraphic = true;
            this.configureButton.UseVisualStyleBackColor = false;
            this.configureButton.Click += new System.EventHandler(this.ConfigureButtonClick);
            // 
            // customMapText
            // 
            this.customMapText.BackColor = System.Drawing.Color.Gold;
            this.customMapText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.customMapText.Font = new System.Drawing.Font("Albertus MT", 11.25F);
            this.customMapText.ForeColor = System.Drawing.Color.Black;
            this.customMapText.Location = new System.Drawing.Point(312, 811);
            this.customMapText.Name = "customMapText";
            this.customMapText.Size = new System.Drawing.Size(323, 46);
            this.customMapText.TabIndex = 11;
            this.customMapText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelSelectButton
            // 
            this.panelSelectButton.BackColor = System.Drawing.Color.Transparent;
            this.panelSelectButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("panelSelectButton.DownBitmap")));
            this.panelSelectButton.FlatAppearance.BorderSize = 0;
            this.panelSelectButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.panelSelectButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.panelSelectButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.panelSelectButton.Font = new System.Drawing.Font("Albertus MT Light", 12F);
            this.panelSelectButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.panelSelectButton.Image = ((System.Drawing.Image)(resources.GetObject("panelSelectButton.Image")));
            this.panelSelectButton.Location = new System.Drawing.Point(148, 24);
            this.panelSelectButton.Name = "panelSelectButton";
            this.panelSelectButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("panelSelectButton.OverBitmap")));
            this.panelSelectButton.Size = new System.Drawing.Size(137, 30);
            this.panelSelectButton.SizeToGraphic = false;
            this.panelSelectButton.TabIndex = 12;
            this.panelSelectButton.Text = "Show Vault";
            this.panelSelectButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("panelSelectButton.UpBitmap")));
            this.panelSelectButton.UseCustomGraphic = true;
            this.panelSelectButton.UseVisualStyleBackColor = false;
            this.panelSelectButton.Click += new System.EventHandler(this.PanelSelectButtonClick);
            // 
            // secondaryVaultListComboBox
            // 
            this.secondaryVaultListComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.secondaryVaultListComboBox.Font = new System.Drawing.Font("Albertus MT Light", 11F);
            this.secondaryVaultListComboBox.Location = new System.Drawing.Point(777, 59);
            this.secondaryVaultListComboBox.MaxDropDownItems = 10;
            this.secondaryVaultListComboBox.Name = "secondaryVaultListComboBox";
            this.secondaryVaultListComboBox.Size = new System.Drawing.Size(481, 25);
            this.secondaryVaultListComboBox.TabIndex = 15;
            this.secondaryVaultListComboBox.SelectedIndexChanged += new System.EventHandler(this.SecondaryVaultListComboBoxSelectedIndexChanged);
            // 
            // aboutButton
            // 
            this.aboutButton.BackColor = System.Drawing.Color.Transparent;
            this.aboutButton.DownBitmap = null;
            this.aboutButton.FlatAppearance.BorderSize = 0;
            this.aboutButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.aboutButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.aboutButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.aboutButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.aboutButton.Image = ((System.Drawing.Image)(resources.GetObject("aboutButton.Image")));
            this.aboutButton.Location = new System.Drawing.Point(1130, 21);
            this.aboutButton.Name = "aboutButton";
            this.aboutButton.OverBitmap = null;
            this.aboutButton.Size = new System.Drawing.Size(48, 38);
            this.aboutButton.SizeToGraphic = false;
            this.aboutButton.TabIndex = 16;
            this.aboutButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("aboutButton.UpBitmap")));
            this.aboutButton.UseCustomGraphic = true;
            this.aboutButton.UseVisualStyleBackColor = false;
            this.aboutButton.Click += new System.EventHandler(this.AboutButtonClick);
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.BackColor = System.Drawing.Color.Transparent;
            this.titleLabel.Font = new System.Drawing.Font("Albertus MT Light", 24F);
            this.titleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(195)))), ((int)(((byte)(112)))));
            this.titleLabel.Location = new System.Drawing.Point(1180, 18);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(135, 38);
            this.titleLabel.TabIndex = 17;
            this.titleLabel.Text = "TQVault";
            // 
            // searchButton
            // 
            this.searchButton.BackColor = System.Drawing.Color.Transparent;
            this.searchButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("searchButton.DownBitmap")));
            this.searchButton.FlatAppearance.BorderSize = 0;
            this.searchButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.searchButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.searchButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.searchButton.Font = new System.Drawing.Font("Albertus MT Light", 12F);
            this.searchButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.searchButton.Image = ((System.Drawing.Image)(resources.GetObject("searchButton.Image")));
            this.searchButton.Location = new System.Drawing.Point(291, 24);
            this.searchButton.Name = "searchButton";
            this.searchButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("searchButton.OverBitmap")));
            this.searchButton.Size = new System.Drawing.Size(137, 30);
            this.searchButton.SizeToGraphic = false;
            this.searchButton.TabIndex = 18;
            this.searchButton.Text = "Search";
            this.searchButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("searchButton.UpBitmap")));
            this.searchButton.UseCustomGraphic = true;
            this.searchButton.UseVisualStyleBackColor = false;
            this.searchButton.Click += new System.EventHandler(this.SearchButtonClick);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorker1_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.BackgroundWorker1_ProgressChanged);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundWorker1_RunWorkerCompleted);
            // 
            // fadeInTimer
            // 
            this.fadeInTimer.Interval = 50;
            this.fadeInTimer.Tick += new System.EventHandler(this.FadeInTimerTick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackgroundImage = global::TQVaultAE.GUI.Properties.Resources.MainForm_NewBackground;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.CancelButton = this.exitButton;
            this.ClientSize = new System.Drawing.Size(1350, 910);
            this.ConstrainToDesignRatio = true;
            this.Controls.Add(this.searchButton);
            this.Controls.Add(this.aboutButton);
            this.Controls.Add(this.secondaryVaultListComboBox);
            this.Controls.Add(this.panelSelectButton);
            this.Controls.Add(this.customMapText);
            this.Controls.Add(this.configureButton);
            this.Controls.Add(this.vaultLabel);
            this.Controls.Add(this.vaultListComboBox);
            this.Controls.Add(this.itemTextPanel);
            this.Controls.Add(this.characterLabel);
            this.Controls.Add(this.characterComboBox);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.titleLabel);
            this.DrawCustomBorder = true;
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.ResizeCustomAllowed = true;
            this.ScaleOnResize = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Titan Quest Item Vault";
            this.TitleTextColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.Closing += new System.ComponentModel.CancelEventHandler(this.MainFormClosing);
            this.Load += new System.EventHandler(this.MainFormLoad);
            this.Shown += new System.EventHandler(this.MainFormShown);
            this.ResizeEnd += new System.EventHandler(this.ResizeEndCallback);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainFormKeyDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MainFormKeyPress);
            this.Resize += new System.EventHandler(this.ResizeBeginCallback);
            this.Controls.SetChildIndex(this.titleLabel, 0);
            this.Controls.SetChildIndex(this.exitButton, 0);
            this.Controls.SetChildIndex(this.characterComboBox, 0);
            this.Controls.SetChildIndex(this.characterLabel, 0);
            this.Controls.SetChildIndex(this.itemTextPanel, 0);
            this.Controls.SetChildIndex(this.vaultListComboBox, 0);
            this.Controls.SetChildIndex(this.vaultLabel, 0);
            this.Controls.SetChildIndex(this.configureButton, 0);
            this.Controls.SetChildIndex(this.customMapText, 0);
            this.Controls.SetChildIndex(this.panelSelectButton, 0);
            this.Controls.SetChildIndex(this.secondaryVaultListComboBox, 0);
            this.Controls.SetChildIndex(this.aboutButton, 0);
            this.Controls.SetChildIndex(this.searchButton, 0);
            this.itemTextPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
	}
}