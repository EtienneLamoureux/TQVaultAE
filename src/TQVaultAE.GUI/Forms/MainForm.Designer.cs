//-----------------------------------------------------------------------
// <copyright file="MainForm.Designer.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using TQVaultAE.GUI.Components.Character;
using TQVaultAE.GUI.Components.Layout;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Components;

namespace TQVaultAE.GUI.Forms;

/// <summary>
/// MainForm Designer class
/// </summary>
public partial class MainForm
{
	/// <summary>
	/// Windows Form Exit button
	/// </summary>
	private ScalingButton exitButton;



	/// <summary>
	/// Windows Form Configure Button
	/// </summary>
	private ScalingButton configureButton;

	/// <summary>
	/// Windows Form Custom Map Text Label
	/// </summary>

	/// <summary>
	/// Windows Form Panel Selection Button
	/// </summary>
	private ScalingButton showVaulButton;

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
        this.exitButton = new TQVaultAE.GUI.Components.Scaling.ScalingButton();
        this.configureButton = new TQVaultAE.GUI.Components.Scaling.ScalingButton();
        this.showVaulButton = new TQVaultAE.GUI.Components.Scaling.ScalingButton();
        this.aboutButton = new TQVaultAE.GUI.Components.Scaling.ScalingButton();
        this.titleLabel = new TQVaultAE.GUI.Components.Scaling.ScalingLabel();
        this.searchButton = new TQVaultAE.GUI.Components.Scaling.ScalingButton();
        this.fadeInTimer = new System.Windows.Forms.Timer(this.components);
        this.saveButton = new TQVaultAE.GUI.Components.Scaling.ScalingButton();
        this.flowLayoutPanelMenuButtons = new System.Windows.Forms.FlowLayoutPanel();
        this.forgeButton = new TQVaultAE.GUI.Components.Scaling.ScalingButton();
        this.toolTip = new System.Windows.Forms.ToolTip(this.components);
        this.flowLayoutPanelMenuButtons.SuspendLayout();
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
        this.exitButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
        this.exitButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.exitButton.Image = ((System.Drawing.Image)(resources.GetObject("exitButton.Image")));
        this.exitButton.Location = new System.Drawing.Point(718, 3);
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
        // configureButton
        // 
        this.configureButton.BackColor = System.Drawing.Color.Transparent;
        this.configureButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("configureButton.DownBitmap")));
        this.configureButton.FlatAppearance.BorderSize = 0;
        this.configureButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.configureButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.configureButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.configureButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
        this.configureButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.configureButton.Image = ((System.Drawing.Image)(resources.GetObject("configureButton.Image")));
        this.configureButton.Location = new System.Drawing.Point(3, 3);
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
        // showVaulButton
        // 
        this.showVaulButton.BackColor = System.Drawing.Color.Transparent;
        this.showVaulButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("showVaulButton.DownBitmap")));
        this.showVaulButton.FlatAppearance.BorderSize = 0;
        this.showVaulButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.showVaulButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.showVaulButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.showVaulButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
        this.showVaulButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.showVaulButton.Image = ((System.Drawing.Image)(resources.GetObject("showVaulButton.Image")));
        this.showVaulButton.Location = new System.Drawing.Point(146, 3);
        this.showVaulButton.Name = "showVaulButton";
        this.showVaulButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("showVaulButton.OverBitmap")));
        this.showVaulButton.Size = new System.Drawing.Size(137, 30);
        this.showVaulButton.SizeToGraphic = false;
        this.showVaulButton.TabIndex = 12;
        this.showVaulButton.Text = "Show Vault";
        this.showVaulButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("showVaulButton.UpBitmap")));
        this.showVaulButton.UseCustomGraphic = true;
        this.showVaulButton.UseVisualStyleBackColor = false;
        this.showVaulButton.Click += new System.EventHandler(this.PanelSelectButtonClick);
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
        this.titleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F);
        this.titleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(195)))), ((int)(((byte)(112)))));
        this.titleLabel.Location = new System.Drawing.Point(1180, 18);
        this.titleLabel.Name = "titleLabel";
        this.titleLabel.Size = new System.Drawing.Size(136, 37);
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
        this.searchButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
        this.searchButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.searchButton.Image = ((System.Drawing.Image)(resources.GetObject("searchButton.Image")));
        this.searchButton.Location = new System.Drawing.Point(289, 3);
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
        // fadeInTimer
        // 
        this.fadeInTimer.Interval = 50;
        this.fadeInTimer.Tick += new System.EventHandler(this.FadeInTimerTick);
        // 
        // saveButton
        // 
        this.saveButton.BackColor = System.Drawing.Color.Transparent;
        this.saveButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.saveButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("saveButton.DownBitmap")));
        this.saveButton.FlatAppearance.BorderSize = 0;
        this.saveButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.saveButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.saveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.saveButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
        this.saveButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.saveButton.Image = ((System.Drawing.Image)(resources.GetObject("saveButton.Image")));
        this.saveButton.Location = new System.Drawing.Point(575, 3);
        this.saveButton.Name = "saveButton";
        this.saveButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("saveButton.OverBitmap")));
        this.saveButton.Size = new System.Drawing.Size(137, 30);
        this.saveButton.SizeToGraphic = false;
        this.saveButton.TabIndex = 23;
        this.saveButton.Text = "Save";
        this.saveButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("saveButton.UpBitmap")));
        this.saveButton.UseCustomGraphic = true;
        this.saveButton.UseVisualStyleBackColor = false;
        this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
        // 
        // flowLayoutPanelMenuButtons
        // 
        this.flowLayoutPanelMenuButtons.AutoSize = true;
        this.flowLayoutPanelMenuButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.flowLayoutPanelMenuButtons.BackColor = System.Drawing.Color.Transparent;
        this.flowLayoutPanelMenuButtons.Controls.Add(this.configureButton);
        this.flowLayoutPanelMenuButtons.Controls.Add(this.showVaulButton);
        this.flowLayoutPanelMenuButtons.Controls.Add(this.searchButton);
        this.flowLayoutPanelMenuButtons.Controls.Add(this.forgeButton);
        this.flowLayoutPanelMenuButtons.Controls.Add(this.saveButton);
        this.flowLayoutPanelMenuButtons.Controls.Add(this.exitButton);
        this.flowLayoutPanelMenuButtons.Location = new System.Drawing.Point(15, 18);
        this.flowLayoutPanelMenuButtons.Name = "flowLayoutPanelMenuButtons";
        this.flowLayoutPanelMenuButtons.Size = new System.Drawing.Size(858, 36);
        this.flowLayoutPanelMenuButtons.TabIndex = 24;
        // 
        // forgeButton
        // 
        this.forgeButton.BackColor = System.Drawing.Color.Transparent;
        this.forgeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.forgeButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("forgeButton.DownBitmap")));
        this.forgeButton.FlatAppearance.BorderSize = 0;
        this.forgeButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.forgeButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.forgeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.forgeButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
        this.forgeButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.forgeButton.Image = ((System.Drawing.Image)(resources.GetObject("forgeButton.Image")));
        this.forgeButton.Location = new System.Drawing.Point(432, 3);
        this.forgeButton.Name = "forgeButton";
        this.forgeButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("forgeButton.OverBitmap")));
        this.forgeButton.Size = new System.Drawing.Size(137, 30);
        this.forgeButton.SizeToGraphic = false;
        this.forgeButton.TabIndex = 24;
        this.forgeButton.Text = "Forge";
        this.forgeButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("forgeButton.UpBitmap")));
        this.forgeButton.UseCustomGraphic = true;
        this.forgeButton.UseVisualStyleBackColor = false;
        this.forgeButton.Click += new System.EventHandler(this.scalingButtonForge_Click);
        // 
        // mainView
        // 
        this.mainView.Location = new System.Drawing.Point(15, 60);
        this.mainView.Name = "mainView";
        this.mainView.Size = new System.Drawing.Size(1320, 235);
        this.mainView.TabIndex = 21;
        // 
        // MainForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
        this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
        this.ClientSize = new System.Drawing.Size(1350, 910);
        this.ConstrainToDesignRatio = true;
        this.Controls.Add(this.flowLayoutPanelMenuButtons);
        this.Controls.Add(this.mainView);
        this.Controls.Add(this.aboutButton);
        this.Controls.Add(this.titleLabel);
        this.DrawCustomBorder = true;
        this.ForeColor = System.Drawing.Color.White;
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.KeyPreview = true;
        this.Margin = new System.Windows.Forms.Padding(4);
        this.Name = "MainForm";
        this.ResizeCustomAllowed = true;
        this.ScaleOnResize = true;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Titan Quest Item Vault";
        this.TitleTextColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainFormClosing);
        this.Load += new System.EventHandler(this.MainFormLoad);
        this.Shown += new System.EventHandler(this.MainFormShown);
        this.ResizeEnd += new System.EventHandler(this.ResizeEndCallback);
        this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainFormKeyDown);
        this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MainFormKeyPress);
        this.Resize += new System.EventHandler(this.ResizeBeginCallback);
        this.Controls.SetChildIndex(this.titleLabel, 0);
        this.Controls.SetChildIndex(this.aboutButton, 0);
        this.Controls.SetChildIndex(this.mainView, 0);
        this.Controls.SetChildIndex(this.flowLayoutPanelMenuButtons, 0);
        this.flowLayoutPanelMenuButtons.ResumeLayout(false);
        this.ResumeLayout(false);
        this.PerformLayout();

	}

	#endregion

	private ScalingButton saveButton;
	private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelMenuButtons;
	private System.Windows.Forms.ToolTip toolTip;
	private ScalingButton forgeButton;
}
