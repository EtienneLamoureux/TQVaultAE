//-----------------------------------------------------------------------
// <copyright file="MainFormUserControl.Designer.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TQVaultAE.GUI.Views;

/// <summary>
/// Designer class for MainFormUserControl
/// </summary>
public partial class MainFormUserControl
{
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

	#region Component Designer generated code

	/// <summary>
	/// Required method for Designer support - do not modify
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
        this.components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainFormUserControl));
        this.itemTextPanel = new System.Windows.Forms.Panel();
        this.NotificationText = new TQVaultAE.GUI.Components.Scaling.ScalingLabel();
        this.vaultListComboBox = new TQVaultAE.GUI.Components.Scaling.ScalingComboBox();
        this.flowLayoutPanelVaultSelector = new TQVaultAE.GUI.Components.Layout.BufferedFlowLayoutPanel();
        this.vaultPictureBox = new System.Windows.Forms.PictureBox();
        this.tableLayoutPanelMain = new TQVaultAE.GUI.Components.Layout.BufferedTableLayoutPanel();
        this.tableLayoutPanelHighlight = new System.Windows.Forms.TableLayoutPanel();
        this.scalingLabelHighlight = new System.Windows.Forms.LinkLabel();
        this.scalingTextBoxHighlight = new TQVaultAE.GUI.Components.Scaling.ScalingTextBox();
        this.flowLayoutPanelRightPanels = new TQVaultAE.GUI.Components.Layout.BufferedFlowLayoutPanel();
        this.flowLayoutPanelRightComboBox = new TQVaultAE.GUI.Components.Layout.BufferedFlowLayoutPanel();
        this.comboBoxCharacter = new TQVaultAE.GUI.Components.Character.ComboBoxCharacter();
        this.bufferedFlowLayoutPanelsecondaryVaultList = new TQVaultAE.GUI.Components.Layout.BufferedFlowLayoutPanel();
        this.pictureBoxSecondVault = new System.Windows.Forms.PictureBox();
        this.secondaryVaultListComboBox = new TQVaultAE.GUI.Components.Scaling.ScalingComboBox();
        this.typeAssistant = new TQVaultAE.GUI.Components.TypeAssistant();
        this.highlightFilters = new TQVaultAE.GUI.Components.HighlightFilters();
        this.itemTextPanel.SuspendLayout();
        this.flowLayoutPanelVaultSelector.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.vaultPictureBox)).BeginInit();
        this.tableLayoutPanelMain.SuspendLayout();
        this.tableLayoutPanelHighlight.SuspendLayout();
        this.flowLayoutPanelRightComboBox.SuspendLayout();
        this.bufferedFlowLayoutPanelsecondaryVaultList.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSecondVault)).BeginInit();
        this.SuspendLayout();
        // 
        // itemTextPanel
        // 
        this.itemTextPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(188)))), ((int)(((byte)(97)))));
        this.itemTextPanel.Controls.Add(this.NotificationText);
        this.itemTextPanel.Location = new System.Drawing.Point(4, 154);
        this.itemTextPanel.Margin = new System.Windows.Forms.Padding(3, 3, 3, 20);
        this.itemTextPanel.Name = "itemTextPanel";
        this.itemTextPanel.Padding = new System.Windows.Forms.Padding(2);
        this.itemTextPanel.Size = new System.Drawing.Size(592, 22);
        this.itemTextPanel.TabIndex = 4;
        // 
        // NotificationText
        // 
        this.NotificationText.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(41)))), ((int)(((byte)(31)))));
        this.NotificationText.Dock = System.Windows.Forms.DockStyle.Fill;
        this.NotificationText.Font = new System.Drawing.Font("Arial Black", 8.25F);
        this.NotificationText.Location = new System.Drawing.Point(2, 2);
        this.NotificationText.Name = "NotificationText";
        this.NotificationText.Size = new System.Drawing.Size(588, 18);
        this.NotificationText.TabIndex = 0;
        // 
        // vaultListComboBox
        // 
        this.vaultListComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
        this.vaultListComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.vaultListComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F);
        this.vaultListComboBox.Location = new System.Drawing.Point(51, 3);
        this.vaultListComboBox.Name = "vaultListComboBox";
        this.vaultListComboBox.Size = new System.Drawing.Size(481, 28);
        this.vaultListComboBox.TabIndex = 5;
        // 
        // flowLayoutPanelVaultSelector
        // 
        this.flowLayoutPanelVaultSelector.AutoSize = true;
        this.flowLayoutPanelVaultSelector.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.flowLayoutPanelVaultSelector.BackColor = System.Drawing.Color.Transparent;
        this.flowLayoutPanelVaultSelector.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        this.flowLayoutPanelVaultSelector.Controls.Add(this.vaultPictureBox);
        this.flowLayoutPanelVaultSelector.Controls.Add(this.vaultListComboBox);
        this.flowLayoutPanelVaultSelector.Location = new System.Drawing.Point(4, 4);
        this.flowLayoutPanelVaultSelector.Name = "flowLayoutPanelVaultSelector";
        this.flowLayoutPanelVaultSelector.Size = new System.Drawing.Size(539, 42);
        this.flowLayoutPanelVaultSelector.TabIndex = 20;
        // 
        // vaultPictureBox
        // 
        this.vaultPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        this.vaultPictureBox.Location = new System.Drawing.Point(0, 0);
        this.vaultPictureBox.Margin = new System.Windows.Forms.Padding(0);
        this.vaultPictureBox.Name = "vaultPictureBox";
        this.vaultPictureBox.Size = new System.Drawing.Size(48, 38);
        this.vaultPictureBox.TabIndex = 7;
        this.vaultPictureBox.TabStop = false;
        // 
        // tableLayoutPanelMain
        // 
        this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tableLayoutPanelMain.BackColor = System.Drawing.Color.Transparent;
        this.tableLayoutPanelMain.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
        this.tableLayoutPanelMain.ColumnCount = 4;
        this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 5F));
        this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tableLayoutPanelMain.Controls.Add(this.tableLayoutPanelHighlight, 0, 3);
        this.tableLayoutPanelMain.Controls.Add(this.flowLayoutPanelRightPanels, 2, 1);
        this.tableLayoutPanelMain.Controls.Add(this.flowLayoutPanelVaultSelector, 0, 0);
        this.tableLayoutPanelMain.Controls.Add(this.flowLayoutPanelRightComboBox, 2, 0);
        this.tableLayoutPanelMain.Controls.Add(this.itemTextPanel, 0, 4);
        this.tableLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
        this.tableLayoutPanelMain.Margin = new System.Windows.Forms.Padding(0);
        this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
        this.tableLayoutPanelMain.RowCount = 6;
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tableLayoutPanelMain.Size = new System.Drawing.Size(1320, 235);
        this.tableLayoutPanelMain.TabIndex = 21;
        // 
        // tableLayoutPanelHighlight
        // 
        this.tableLayoutPanelHighlight.AutoSize = false;
        this.tableLayoutPanelHighlight.ColumnCount = 2;
        this.tableLayoutPanelHighlight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.tableLayoutPanelHighlight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tableLayoutPanelHighlight.Controls.Add(this.scalingLabelHighlight, 0, 0);
        this.tableLayoutPanelHighlight.Controls.Add(this.scalingTextBoxHighlight, 1, 0);
        this.tableLayoutPanelHighlight.Location = new System.Drawing.Point(1, 128);
        this.tableLayoutPanelHighlight.Margin = new System.Windows.Forms.Padding(0);
        this.tableLayoutPanelHighlight.Name = "tableLayoutPanelHighlight";
        this.tableLayoutPanelHighlight.RowCount = 1;
        this.tableLayoutPanelHighlight.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanelHighlight.Size = new System.Drawing.Size(300, 22);
        this.tableLayoutPanelHighlight.TabIndex = 24;
        // 
        // scalingLabelHighlight
        // 
        this.scalingLabelHighlight.AutoSize = true;
        this.scalingLabelHighlight.BackColor = System.Drawing.Color.Transparent;
        this.scalingLabelHighlight.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
        this.scalingLabelHighlight.LinkColor = System.Drawing.Color.Gold;
        this.scalingLabelHighlight.Location = new System.Drawing.Point(3, 0);
        this.scalingLabelHighlight.Name = "scalingLabelHighlight";
        this.scalingLabelHighlight.Size = new System.Drawing.Size(143, 16);
        this.scalingLabelHighlight.TabIndex = 25;
        this.scalingLabelHighlight.TabStop = true;
        this.scalingLabelHighlight.Text = "Highlight items having :";
        this.scalingLabelHighlight.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(255)))), ((int)(((byte)(64)))));
        this.scalingLabelHighlight.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.scalingLabelHighlight_LinkClicked);
        // 
        // scalingTextBoxHighlight
        // 
        this.scalingTextBoxHighlight.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
        this.scalingTextBoxHighlight.Location = new System.Drawing.Point(149, 0);
        this.scalingTextBoxHighlight.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
        this.scalingTextBoxHighlight.Name = "scalingTextBoxHighlight";
        this.scalingTextBoxHighlight.Size = new System.Drawing.Size(262, 22);
        this.scalingTextBoxHighlight.TabIndex = 2;
        this.scalingTextBoxHighlight.TextChanged += new System.EventHandler(this.scalingTextBoxHighlight_TextChanged);
        // 
        // flowLayoutPanelRightPanels
        // 
        this.flowLayoutPanelRightPanels.AutoSize = true;
        this.flowLayoutPanelRightPanels.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.flowLayoutPanelRightPanels.BackColor = System.Drawing.Color.Transparent;
        this.flowLayoutPanelRightPanels.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        this.flowLayoutPanelRightPanels.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
        this.flowLayoutPanelRightPanels.Location = new System.Drawing.Point(606, 96);
        this.flowLayoutPanelRightPanels.Margin = new System.Windows.Forms.Padding(0);
        this.flowLayoutPanelRightPanels.MinimumSize = new System.Drawing.Size(200, 100);
        this.flowLayoutPanelRightPanels.Name = "flowLayoutPanelRightPanels";
        this.tableLayoutPanelMain.SetRowSpan(this.flowLayoutPanelRightPanels, 4);
        this.flowLayoutPanelRightPanels.Size = new System.Drawing.Size(200, 100);
        this.flowLayoutPanelRightPanels.TabIndex = 22;
        // 
        // flowLayoutPanelRightComboBox
        // 
        this.flowLayoutPanelRightComboBox.AutoSize = true;
        this.flowLayoutPanelRightComboBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.flowLayoutPanelRightComboBox.BackColor = System.Drawing.Color.Transparent;
        this.flowLayoutPanelRightComboBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        this.flowLayoutPanelRightComboBox.Controls.Add(this.comboBoxCharacter);
        this.flowLayoutPanelRightComboBox.Controls.Add(this.bufferedFlowLayoutPanelsecondaryVaultList);
        this.flowLayoutPanelRightComboBox.Location = new System.Drawing.Point(609, 4);
        this.flowLayoutPanelRightComboBox.Name = "flowLayoutPanelRightComboBox";
        this.flowLayoutPanelRightComboBox.Size = new System.Drawing.Size(549, 88);
        this.flowLayoutPanelRightComboBox.TabIndex = 20;
        // 
        // comboBoxCharacter
        // 
        this.comboBoxCharacter.AutoSize = true;
        this.comboBoxCharacter.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        this.comboBoxCharacter.Location = new System.Drawing.Point(0, 0);
        this.comboBoxCharacter.Margin = new System.Windows.Forms.Padding(0);
        this.comboBoxCharacter.MaxVisibleItems = 10;
        this.comboBoxCharacter.Name = "comboBoxCharacter";
        this.comboBoxCharacter.SelectedIndex = -1;
        this.comboBoxCharacter.SelectedItem = null;
        this.comboBoxCharacter.Size = new System.Drawing.Size(504, 32);
        this.comboBoxCharacter.TabIndex = 16;
        // 
        // bufferedFlowLayoutPanelsecondaryVaultList
        // 
        this.bufferedFlowLayoutPanelsecondaryVaultList.AutoSize = true;
        this.bufferedFlowLayoutPanelsecondaryVaultList.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        this.bufferedFlowLayoutPanelsecondaryVaultList.Controls.Add(this.pictureBoxSecondVault);
        this.bufferedFlowLayoutPanelsecondaryVaultList.Controls.Add(this.secondaryVaultListComboBox);
        this.bufferedFlowLayoutPanelsecondaryVaultList.Location = new System.Drawing.Point(3, 39);
        this.bufferedFlowLayoutPanelsecondaryVaultList.Name = "bufferedFlowLayoutPanelsecondaryVaultList";
        this.bufferedFlowLayoutPanelsecondaryVaultList.Size = new System.Drawing.Size(539, 42);
        this.bufferedFlowLayoutPanelsecondaryVaultList.TabIndex = 17;
        // 
        // pictureBoxSecondVault
        // 
        this.pictureBoxSecondVault.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
        this.pictureBoxSecondVault.Location = new System.Drawing.Point(0, 0);
        this.pictureBoxSecondVault.Margin = new System.Windows.Forms.Padding(0);
        this.pictureBoxSecondVault.Name = "pictureBoxSecondVault";
        this.pictureBoxSecondVault.Size = new System.Drawing.Size(48, 38);
        this.pictureBoxSecondVault.TabIndex = 8;
        this.pictureBoxSecondVault.TabStop = false;
        // 
        // secondaryVaultListComboBox
        // 
        this.secondaryVaultListComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
        this.secondaryVaultListComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.secondaryVaultListComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F);
        this.secondaryVaultListComboBox.Location = new System.Drawing.Point(51, 3);
        this.secondaryVaultListComboBox.MaxDropDownItems = 10;
        this.secondaryVaultListComboBox.Name = "secondaryVaultListComboBox";
        this.secondaryVaultListComboBox.Size = new System.Drawing.Size(481, 28);
        this.secondaryVaultListComboBox.TabIndex = 15;
        // 
        // typeAssistant
        // 
        this.typeAssistant.Idled += new System.EventHandler(this.typeAssistant_Idled);
        // 
        // highlightFilters
        // 
        this.highlightFilters.AutoSize = true;
        this.highlightFilters.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.highlightFilters.BackColor = System.Drawing.Color.Transparent;
        this.highlightFilters.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("highlightFilters.BackgroundImage")));
        this.highlightFilters.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
        this.highlightFilters.Location = new System.Drawing.Point(0, 210);
        this.highlightFilters.Margin = new System.Windows.Forms.Padding(0);
        this.highlightFilters.Name = "highlightFilters";
        this.highlightFilters.Padding = new System.Windows.Forms.Padding(10);
        this.highlightFilters.Size = new System.Drawing.Size(515, 377);
        this.highlightFilters.TabIndex = 25;
        this.highlightFilters.Visible = false;
        //
        // MainFormUserControl
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.BackColor = System.Drawing.Color.Transparent;
        this.Controls.Add(this.highlightFilters);
        this.Controls.Add(this.tableLayoutPanelMain);
        this.ForeColor = System.Drawing.Color.White;
        this.Margin = new System.Windows.Forms.Padding(0);
        this.Name = "MainFormUserControl";
        this.Size = new System.Drawing.Size(1320, 850);
        this.itemTextPanel.ResumeLayout(false);
        this.flowLayoutPanelVaultSelector.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.vaultPictureBox)).EndInit();
        this.tableLayoutPanelMain.ResumeLayout(false);
        this.tableLayoutPanelMain.PerformLayout();
        this.tableLayoutPanelHighlight.ResumeLayout(false);
        this.tableLayoutPanelHighlight.PerformLayout();
        this.flowLayoutPanelRightComboBox.ResumeLayout(false);
        this.flowLayoutPanelRightComboBox.PerformLayout();
        this.bufferedFlowLayoutPanelsecondaryVaultList.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSecondVault)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();

	}

	#endregion

	internal System.Windows.Forms.Panel itemTextPanel;
	internal TQVaultAE.GUI.Components.Scaling.ScalingLabel NotificationText;
	internal TQVaultAE.GUI.Components.Scaling.ScalingComboBox vaultListComboBox;
	internal TQVaultAE.GUI.Components.Layout.BufferedFlowLayoutPanel flowLayoutPanelVaultSelector;
	internal System.Windows.Forms.PictureBox vaultPictureBox;
	internal TQVaultAE.GUI.Components.Layout.BufferedTableLayoutPanel tableLayoutPanelMain;
	internal System.Windows.Forms.TableLayoutPanel tableLayoutPanelHighlight;
	internal System.Windows.Forms.LinkLabel scalingLabelHighlight;
	internal TQVaultAE.GUI.Components.Scaling.ScalingTextBox scalingTextBoxHighlight;
	internal TQVaultAE.GUI.Components.Layout.BufferedFlowLayoutPanel flowLayoutPanelRightPanels;
	internal TQVaultAE.GUI.Components.Layout.BufferedFlowLayoutPanel flowLayoutPanelRightComboBox;
	internal TQVaultAE.GUI.Components.Character.ComboBoxCharacter comboBoxCharacter;
	internal TQVaultAE.GUI.Components.Layout.BufferedFlowLayoutPanel bufferedFlowLayoutPanelsecondaryVaultList;
	internal System.Windows.Forms.PictureBox pictureBoxSecondVault;
	internal TQVaultAE.GUI.Components.Scaling.ScalingComboBox secondaryVaultListComboBox;
	internal TQVaultAE.GUI.Components.TypeAssistant typeAssistant;
	internal TQVaultAE.GUI.Components.HighlightFilters highlightFilters;
}
