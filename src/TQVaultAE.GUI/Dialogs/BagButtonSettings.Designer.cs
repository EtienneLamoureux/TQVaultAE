using System;
using TQVaultAE.GUI.Components.Icons;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Components;

namespace TQVaultAE.GUI.Dialogs;

partial class BagButtonSettings
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
		if (disposing && (components != null))
		{
			components.Dispose();
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BagButtonSettings));
        this.tableLayoutPanelSkeleton = new System.Windows.Forms.TableLayoutPanel();
        this.tableLayoutPanelBottom = new System.Windows.Forms.TableLayoutPanel();
        this.scalingTextBoxDebug = new TQVaultAE.GUI.Components.Scaling.ScalingTextBox();
        this.applyButton = new TQVaultAE.GUI.Components.Scaling.ScalingButton();
        this.cancelButton = new TQVaultAE.GUI.Components.Scaling.ScalingButton();
        this.flowLayoutPanelSelectedButton = new System.Windows.Forms.FlowLayoutPanel();
        this.scalingLabelSelectedButton = new TQVaultAE.GUI.Components.Scaling.ScalingLabel();
        this.scalingLabelSelectedButtonValue = new TQVaultAE.GUI.Components.Scaling.ScalingLabel();
        this.groupBoxEdit = new System.Windows.Forms.GroupBox();
        this.flowLayoutPanelRadioProperties = new System.Windows.Forms.FlowLayoutPanel();
        this.scalingRadioButtonDefaultIcon = new TQVaultAE.GUI.Components.Scaling.ScalingRadioButton();
        this.scalingRadioButtonCustomIcon = new TQVaultAE.GUI.Components.Scaling.ScalingRadioButton();
        this.scalingCheckBoxNumber = new TQVaultAE.GUI.Components.Scaling.ScalingCheckBox();
        this.flowLayoutPanelLabelInput = new System.Windows.Forms.FlowLayoutPanel();
        this.scalingCheckBoxLabel = new TQVaultAE.GUI.Components.Scaling.ScalingCheckBox();
        this.scalingTextBoxLabel = new TQVaultAE.GUI.Components.Scaling.ScalingTextBox();
        this.groupBoxSelectedPictures = new System.Windows.Forms.GroupBox();
        this.flowLayoutPanelPicturePickupMode = new System.Windows.Forms.FlowLayoutPanel();
        this.radioButtonPictureSimple = new System.Windows.Forms.RadioButton();
        this.radioButtonPictureDetailed = new System.Windows.Forms.RadioButton();
        this.flowLayoutPanelPicturePickup = new System.Windows.Forms.FlowLayoutPanel();
        this.groupBoxSimpleImage = new System.Windows.Forms.GroupBox();
        this.pictureBoxSimple = new System.Windows.Forms.PictureBox();
        this.groupBoxOffBitmap = new System.Windows.Forms.GroupBox();
        this.pictureBoxOff = new System.Windows.Forms.PictureBox();
        this.groupBoxOnBitmap = new System.Windows.Forms.GroupBox();
        this.pictureBoxOn = new System.Windows.Forms.PictureBox();
        this.groupBoxOverBitmap = new System.Windows.Forms.GroupBox();
        this.pictureBoxOver = new System.Windows.Forms.PictureBox();
        this.flowLayoutPanelUpperParts = new System.Windows.Forms.FlowLayoutPanel();
        this.groupBoxAvailablePictures = new System.Windows.Forms.GroupBox();
        this.tabControlPics = new TQVaultAE.GUI.Components.Layout.CaravanTabControl();
        this.tabPageSkills = new System.Windows.Forms.TabPage();
        this.iconGridSkills = new TQVaultAE.GUI.Components.Icons.IconGrid();
        this.tabPageRelics = new System.Windows.Forms.TabPage();
        this.iconGridRelics = new TQVaultAE.GUI.Components.Icons.IconGrid();
        this.tabPageArtifacts = new System.Windows.Forms.TabPage();
        this.iconGridArtifacts = new TQVaultAE.GUI.Components.Icons.IconGrid();
        this.tabPageJewellery = new System.Windows.Forms.TabPage();
        this.iconGridJewellery = new TQVaultAE.GUI.Components.Icons.IconGrid();
        this.tabPageScrolls = new System.Windows.Forms.TabPage();
        this.iconGridScrolls = new TQVaultAE.GUI.Components.Icons.IconGrid();
        this.tabPagePotions = new System.Windows.Forms.TabPage();
        this.iconGridPotions = new TQVaultAE.GUI.Components.Icons.IconGrid();
        this.tabPageButtons = new System.Windows.Forms.TabPage();
        this.iconGridButtons = new TQVaultAE.GUI.Components.Icons.IconGrid();
        this.tabPageMisc = new System.Windows.Forms.TabPage();
        this.iconGridMisc = new TQVaultAE.GUI.Components.Icons.IconGrid();
        this.tabPageHelmets = new System.Windows.Forms.TabPage();
        this.iconGridHelmets = new TQVaultAE.GUI.Components.Icons.IconGrid();
        this.tabPageShields = new System.Windows.Forms.TabPage();
        this.iconGridShields = new TQVaultAE.GUI.Components.Icons.IconGrid();
        this.tabPageArmbands = new System.Windows.Forms.TabPage();
        this.iconGridArmbands = new TQVaultAE.GUI.Components.Icons.IconGrid();
        this.tabPageGreaves = new System.Windows.Forms.TabPage();
        this.iconGridGreaves = new TQVaultAE.GUI.Components.Icons.IconGrid();
        this.scalingLabelDragDropNotice = new TQVaultAE.GUI.Components.Scaling.ScalingLabel();
        this.iconMagnifier = new TQVaultAE.GUI.Components.Icons.IconMagnifier();
        this.tableLayoutPanelSkeleton.SuspendLayout();
        this.tableLayoutPanelBottom.SuspendLayout();
        this.flowLayoutPanelSelectedButton.SuspendLayout();
        this.groupBoxEdit.SuspendLayout();
        this.flowLayoutPanelRadioProperties.SuspendLayout();
        this.flowLayoutPanelLabelInput.SuspendLayout();
        this.groupBoxSelectedPictures.SuspendLayout();
        this.flowLayoutPanelPicturePickupMode.SuspendLayout();
        this.flowLayoutPanelPicturePickup.SuspendLayout();
        this.groupBoxSimpleImage.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSimple)).BeginInit();
        this.groupBoxOffBitmap.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOff)).BeginInit();
        this.groupBoxOnBitmap.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOn)).BeginInit();
        this.groupBoxOverBitmap.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOver)).BeginInit();
        this.flowLayoutPanelUpperParts.SuspendLayout();
        this.groupBoxAvailablePictures.SuspendLayout();
        this.tabControlPics.SuspendLayout();
        this.tabPageSkills.SuspendLayout();
        this.tabPageRelics.SuspendLayout();
        this.tabPageArtifacts.SuspendLayout();
        this.tabPageJewellery.SuspendLayout();
        this.tabPageScrolls.SuspendLayout();
        this.tabPagePotions.SuspendLayout();
        this.tabPageButtons.SuspendLayout();
        this.tabPageMisc.SuspendLayout();
        this.tabPageHelmets.SuspendLayout();
        this.tabPageShields.SuspendLayout();
        this.tabPageArmbands.SuspendLayout();
        this.tabPageGreaves.SuspendLayout();
        this.SuspendLayout();
        // 
        // tableLayoutPanelSkeleton
        // 
        this.tableLayoutPanelSkeleton.ColumnCount = 1;
        this.tableLayoutPanelSkeleton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tableLayoutPanelSkeleton.Controls.Add(this.flowLayoutPanelSelectedButton, 0, 0);
        this.tableLayoutPanelSkeleton.Controls.Add(this.flowLayoutPanelUpperParts, 0, 1);
        this.tableLayoutPanelSkeleton.Controls.Add(this.groupBoxAvailablePictures, 0, 2);
        this.tableLayoutPanelSkeleton.Controls.Add(this.tableLayoutPanelBottom, 0, 3);
        this.tableLayoutPanelSkeleton.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tableLayoutPanelSkeleton.Location = new System.Drawing.Point(23, 32);
        this.tableLayoutPanelSkeleton.Margin = new System.Windows.Forms.Padding(0);
        this.tableLayoutPanelSkeleton.Name = "tableLayoutPanelSkeleton";
        this.tableLayoutPanelSkeleton.RowCount = 4;
        this.tableLayoutPanelSkeleton.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanelSkeleton.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanelSkeleton.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.tableLayoutPanelSkeleton.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanelSkeleton.Size = new System.Drawing.Size(1002, 628);
        this.tableLayoutPanelSkeleton.TabIndex = 15;
        // 
        // tableLayoutPanelBottom
        // 
        this.tableLayoutPanelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tableLayoutPanelBottom.AutoSize = true;
        this.tableLayoutPanelBottom.ColumnCount = 3;
        this.tableLayoutPanelBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
        this.tableLayoutPanelBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
        this.tableLayoutPanelBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
        this.tableLayoutPanelBottom.Controls.Add(this.scalingTextBoxDebug, 0, 0);
        this.tableLayoutPanelBottom.Controls.Add(this.applyButton, 0, 0);
        this.tableLayoutPanelBottom.Controls.Add(this.cancelButton, 2, 0);
        this.tableLayoutPanelBottom.Location = new System.Drawing.Point(26, 620);
        this.tableLayoutPanelBottom.Name = "tableLayoutPanelBottom";
        this.tableLayoutPanelBottom.RowCount = 1;
        this.tableLayoutPanelBottom.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tableLayoutPanelBottom.Size = new System.Drawing.Size(996, 37);
        this.tableLayoutPanelBottom.TabIndex = 7;
        // 
        // scalingTextBoxDebug
        // 
        this.scalingTextBoxDebug.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
        this.scalingTextBoxDebug.Location = new System.Drawing.Point(152, 6);
        this.scalingTextBoxDebug.Name = "scalingTextBoxDebug";
        this.scalingTextBoxDebug.Size = new System.Drawing.Size(691, 25);
        this.scalingTextBoxDebug.TabIndex = 4;
        this.scalingTextBoxDebug.Visible = false;
        // 
        // applyButton
        // 
        this.applyButton.BackColor = System.Drawing.Color.Transparent;
        this.applyButton.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.applyButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("applyButton.DownBitmap")));
        this.applyButton.FlatAppearance.BorderSize = 0;
        this.applyButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.applyButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.applyButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.applyButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.applyButton.Image = ((System.Drawing.Image)(resources.GetObject("applyButton.Image")));
        this.applyButton.Location = new System.Drawing.Point(3, 3);
        this.applyButton.Name = "applyButton";
        this.applyButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("applyButton.OverBitmap")));
        this.applyButton.Size = new System.Drawing.Size(80, 30);
        this.applyButton.SizeToGraphic = false;
        this.applyButton.TabIndex = 2;
        this.applyButton.Text = "Apply";
        this.applyButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("applyButton.UpBitmap")));
        this.applyButton.UseCustomGraphic = true;
        this.applyButton.UseVisualStyleBackColor = false;
        this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
        // 
        // cancelButton
        // 
        this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.cancelButton.BackColor = System.Drawing.Color.Transparent;
        this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.cancelButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("cancelButton.DownBitmap")));
        this.cancelButton.FlatAppearance.BorderSize = 0;
        this.cancelButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.cancelButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.cancelButton.Image = ((System.Drawing.Image)(resources.GetObject("cancelButton.Image")));
        this.cancelButton.Location = new System.Drawing.Point(911, 3);
        this.cancelButton.Name = "cancelButton";
        this.cancelButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("cancelButton.OverBitmap")));
        this.cancelButton.Size = new System.Drawing.Size(82, 30);
        this.cancelButton.SizeToGraphic = false;
        this.cancelButton.TabIndex = 3;
        this.cancelButton.Text = "Cancel";
        this.cancelButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("cancelButton.UpBitmap")));
        this.cancelButton.UseCustomGraphic = true;
        this.cancelButton.UseVisualStyleBackColor = false;
        this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
        // 
        // flowLayoutPanelSelectedButton
        // 
        this.flowLayoutPanelSelectedButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.flowLayoutPanelSelectedButton.AutoSize = true;
        this.flowLayoutPanelSelectedButton.Controls.Add(this.scalingLabelSelectedButton);
        this.flowLayoutPanelSelectedButton.Controls.Add(this.scalingLabelSelectedButtonValue);
        this.flowLayoutPanelSelectedButton.Location = new System.Drawing.Point(26, 35);
        this.flowLayoutPanelSelectedButton.Name = "flowLayoutPanelSelectedButton";
        this.flowLayoutPanelSelectedButton.Size = new System.Drawing.Size(996, 28);
        this.flowLayoutPanelSelectedButton.TabIndex = 8;
        // 
        // scalingLabelSelectedButton
        // 
        this.scalingLabelSelectedButton.AutoSize = true;
        this.scalingLabelSelectedButton.ForeColor = System.Drawing.Color.Gold;
        this.scalingLabelSelectedButton.Location = new System.Drawing.Point(3, 0);
        this.scalingLabelSelectedButton.Name = "scalingLabelSelectedButton";
        this.scalingLabelSelectedButton.Size = new System.Drawing.Size(119, 18);
        this.scalingLabelSelectedButton.TabIndex = 0;
        this.scalingLabelSelectedButton.Tag = "{0} : ";
        this.scalingLabelSelectedButton.Text = "Selected button :";
        // 
        // scalingLabelSelectedButtonValue
        // 
        this.scalingLabelSelectedButtonValue.AutoSize = true;
        this.scalingLabelSelectedButtonValue.Location = new System.Drawing.Point(128, 0);
        this.scalingLabelSelectedButtonValue.Name = "scalingLabelSelectedButtonValue";
        this.scalingLabelSelectedButtonValue.Size = new System.Drawing.Size(156, 17);
        this.scalingLabelSelectedButtonValue.TabIndex = 1;
        this.scalingLabelSelectedButtonValue.Text = "VaultName - 10 - (label)";
        // 
        // groupBoxEdit
        // 
        this.groupBoxEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.groupBoxEdit.AutoSize = true;
        this.groupBoxEdit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.groupBoxEdit.Controls.Add(this.flowLayoutPanelRadioProperties);
        this.groupBoxEdit.ForeColor = System.Drawing.Color.Gold;
        this.groupBoxEdit.Location = new System.Drawing.Point(3, 3);
        this.groupBoxEdit.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
        this.groupBoxEdit.Name = "groupBoxEdit";
        this.groupBoxEdit.Size = new System.Drawing.Size(250, 162);
        this.groupBoxEdit.TabIndex = 9;
        this.groupBoxEdit.TabStop = false;
        this.groupBoxEdit.Text = "Button properties";
        // 
        // flowLayoutPanelRadioProperties
        // 
        this.flowLayoutPanelRadioProperties.AutoSize = true;
        this.flowLayoutPanelRadioProperties.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.flowLayoutPanelRadioProperties.Controls.Add(this.scalingRadioButtonDefaultIcon);
        this.flowLayoutPanelRadioProperties.Controls.Add(this.scalingRadioButtonCustomIcon);
        this.flowLayoutPanelRadioProperties.Controls.Add(this.scalingCheckBoxNumber);
        this.flowLayoutPanelRadioProperties.Controls.Add(this.flowLayoutPanelLabelInput);
        this.flowLayoutPanelRadioProperties.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
        this.flowLayoutPanelRadioProperties.Location = new System.Drawing.Point(14, 25);
        this.flowLayoutPanelRadioProperties.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
        this.flowLayoutPanelRadioProperties.Name = "flowLayoutPanelRadioProperties";
        this.flowLayoutPanelRadioProperties.Size = new System.Drawing.Size(223, 112);
        this.flowLayoutPanelRadioProperties.TabIndex = 10;
        // 
        // scalingRadioButtonDefaultIcon
        // 
        this.scalingRadioButtonDefaultIcon.AutoSize = true;
        this.scalingRadioButtonDefaultIcon.Checked = true;
        this.scalingRadioButtonDefaultIcon.ForeColor = System.Drawing.Color.White;
        this.scalingRadioButtonDefaultIcon.Location = new System.Drawing.Point(3, 3);
        this.scalingRadioButtonDefaultIcon.Name = "scalingRadioButtonDefaultIcon";
        this.scalingRadioButtonDefaultIcon.Size = new System.Drawing.Size(100, 21);
        this.scalingRadioButtonDefaultIcon.TabIndex = 14;
        this.scalingRadioButtonDefaultIcon.TabStop = true;
        this.scalingRadioButtonDefaultIcon.Text = "Use Default";
        this.scalingRadioButtonDefaultIcon.UseVisualStyleBackColor = true;
        this.scalingRadioButtonDefaultIcon.CheckedChanged += new System.EventHandler(this.scalingRadioButtonDefaultIcon_CheckedChanged);
        // 
        // scalingRadioButtonCustomIcon
        // 
        this.scalingRadioButtonCustomIcon.AutoSize = true;
        this.scalingRadioButtonCustomIcon.ForeColor = System.Drawing.Color.White;
        this.scalingRadioButtonCustomIcon.Location = new System.Drawing.Point(3, 30);
        this.scalingRadioButtonCustomIcon.Name = "scalingRadioButtonCustomIcon";
        this.scalingRadioButtonCustomIcon.Size = new System.Drawing.Size(107, 21);
        this.scalingRadioButtonCustomIcon.TabIndex = 15;
        this.scalingRadioButtonCustomIcon.TabStop = true;
        this.scalingRadioButtonCustomIcon.Text = "Custom Icon";
        this.scalingRadioButtonCustomIcon.UseVisualStyleBackColor = true;
        // 
        // scalingCheckBoxNumber
        // 
        this.scalingCheckBoxNumber.AutoSize = true;
        this.scalingCheckBoxNumber.ForeColor = System.Drawing.Color.White;
        this.scalingCheckBoxNumber.Location = new System.Drawing.Point(3, 57);
        this.scalingCheckBoxNumber.Name = "scalingCheckBoxNumber";
        this.scalingCheckBoxNumber.Size = new System.Drawing.Size(80, 21);
        this.scalingCheckBoxNumber.TabIndex = 12;
        this.scalingCheckBoxNumber.Text = "Number";
        this.scalingCheckBoxNumber.UseVisualStyleBackColor = true;
        // 
        // flowLayoutPanelLabelInput
        // 
        this.flowLayoutPanelLabelInput.AutoSize = true;
        this.flowLayoutPanelLabelInput.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.flowLayoutPanelLabelInput.Controls.Add(this.scalingCheckBoxLabel);
        this.flowLayoutPanelLabelInput.Controls.Add(this.scalingTextBoxLabel);
        this.flowLayoutPanelLabelInput.Location = new System.Drawing.Point(3, 84);
        this.flowLayoutPanelLabelInput.Margin = new System.Windows.Forms.Padding(3, 3, 14, 3);
        this.flowLayoutPanelLabelInput.Name = "flowLayoutPanelLabelInput";
        this.flowLayoutPanelLabelInput.Size = new System.Drawing.Size(206, 25);
        this.flowLayoutPanelLabelInput.TabIndex = 9;
        // 
        // scalingCheckBoxLabel
        // 
        this.scalingCheckBoxLabel.AutoSize = true;
        this.scalingCheckBoxLabel.ForeColor = System.Drawing.Color.White;
        this.scalingCheckBoxLabel.Location = new System.Drawing.Point(0, 0);
        this.scalingCheckBoxLabel.Margin = new System.Windows.Forms.Padding(0);
        this.scalingCheckBoxLabel.Name = "scalingCheckBoxLabel";
        this.scalingCheckBoxLabel.Size = new System.Drawing.Size(66, 21);
        this.scalingCheckBoxLabel.TabIndex = 13;
        this.scalingCheckBoxLabel.Text = "Label :";
        this.scalingCheckBoxLabel.UseVisualStyleBackColor = true;
        this.scalingCheckBoxLabel.CheckedChanged += new System.EventHandler(this.checkBoxLabel_CheckedChanged);
        // 
        // scalingTextBoxLabel
        // 
        this.scalingTextBoxLabel.Location = new System.Drawing.Point(66, 0);
        this.scalingTextBoxLabel.Margin = new System.Windows.Forms.Padding(0);
        this.scalingTextBoxLabel.Name = "scalingTextBoxLabel";
        this.scalingTextBoxLabel.Size = new System.Drawing.Size(140, 25);
        this.scalingTextBoxLabel.TabIndex = 3;
        // 
        // groupBoxSelectedPictures
        // 
        this.groupBoxSelectedPictures.AutoSize = true;
        this.groupBoxSelectedPictures.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.groupBoxSelectedPictures.Controls.Add(this.flowLayoutPanelPicturePickupMode);
        this.groupBoxSelectedPictures.Controls.Add(this.flowLayoutPanelPicturePickup);
        this.groupBoxSelectedPictures.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.groupBoxSelectedPictures.ForeColor = System.Drawing.Color.Gold;
        this.groupBoxSelectedPictures.Location = new System.Drawing.Point(266, 6);
        this.groupBoxSelectedPictures.Name = "groupBoxSelectedPictures";
        this.groupBoxSelectedPictures.Size = new System.Drawing.Size(317, 159);
        this.groupBoxSelectedPictures.TabIndex = 10;
        this.groupBoxSelectedPictures.TabStop = false;
        this.groupBoxSelectedPictures.Text = "Selected pictures";
        // 
        // flowLayoutPanelPicturePickupMode
        // 
        this.flowLayoutPanelPicturePickupMode.AutoSize = true;
        this.flowLayoutPanelPicturePickupMode.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.flowLayoutPanelPicturePickupMode.Controls.Add(this.radioButtonPictureSimple);
        this.flowLayoutPanelPicturePickupMode.Controls.Add(this.radioButtonPictureDetailed);
        this.flowLayoutPanelPicturePickupMode.Location = new System.Drawing.Point(17, 23);
        this.flowLayoutPanelPicturePickupMode.Margin = new System.Windows.Forms.Padding(3, 3, 17, 3);
        this.flowLayoutPanelPicturePickupMode.Name = "flowLayoutPanelPicturePickupMode";
        this.flowLayoutPanelPicturePickupMode.Size = new System.Drawing.Size(153, 27);
        this.flowLayoutPanelPicturePickupMode.TabIndex = 11;
        // 
        // radioButtonPictureSimple
        // 
        this.radioButtonPictureSimple.AutoSize = true;
        this.radioButtonPictureSimple.Checked = true;
        this.radioButtonPictureSimple.ForeColor = System.Drawing.Color.White;
        this.radioButtonPictureSimple.Location = new System.Drawing.Point(3, 3);
        this.radioButtonPictureSimple.Name = "radioButtonPictureSimple";
        this.radioButtonPictureSimple.Size = new System.Drawing.Size(66, 21);
        this.radioButtonPictureSimple.TabIndex = 0;
        this.radioButtonPictureSimple.TabStop = true;
        this.radioButtonPictureSimple.Text = "Simple";
        this.radioButtonPictureSimple.UseVisualStyleBackColor = true;
        this.radioButtonPictureSimple.CheckedChanged += new System.EventHandler(this.radioButtonPictureSimple_CheckedChanged);
        // 
        // radioButtonPictureDetailed
        // 
        this.radioButtonPictureDetailed.AutoSize = true;
        this.radioButtonPictureDetailed.ForeColor = System.Drawing.Color.White;
        this.radioButtonPictureDetailed.Location = new System.Drawing.Point(75, 3);
        this.radioButtonPictureDetailed.Name = "radioButtonPictureDetailed";
        this.radioButtonPictureDetailed.Size = new System.Drawing.Size(75, 21);
        this.radioButtonPictureDetailed.TabIndex = 1;
        this.radioButtonPictureDetailed.Text = "Detailed";
        this.radioButtonPictureDetailed.UseVisualStyleBackColor = true;
        this.radioButtonPictureDetailed.CheckedChanged += new System.EventHandler(this.radioButtonPictureDetailed_CheckedChanged);
        // 
        // flowLayoutPanelPicturePickup
        // 
        this.flowLayoutPanelPicturePickup.AutoSize = true;
        this.flowLayoutPanelPicturePickup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.flowLayoutPanelPicturePickup.Controls.Add(this.groupBoxSimpleImage);
        this.flowLayoutPanelPicturePickup.Controls.Add(this.groupBoxOffBitmap);
        this.flowLayoutPanelPicturePickup.Controls.Add(this.groupBoxOnBitmap);
        this.flowLayoutPanelPicturePickup.Controls.Add(this.groupBoxOverBitmap);
        this.flowLayoutPanelPicturePickup.Location = new System.Drawing.Point(17, 56);
        this.flowLayoutPanelPicturePickup.Margin = new System.Windows.Forms.Padding(3, 3, 17, 3);
        this.flowLayoutPanelPicturePickup.Name = "flowLayoutPanelPicturePickup";
        this.flowLayoutPanelPicturePickup.Size = new System.Drawing.Size(280, 78);
        this.flowLayoutPanelPicturePickup.TabIndex = 12;
        // 
        // groupBoxSimpleImage
        // 
        this.groupBoxSimpleImage.Controls.Add(this.pictureBoxSimple);
        this.groupBoxSimpleImage.ForeColor = System.Drawing.Color.DarkOrange;
        this.groupBoxSimpleImage.Location = new System.Drawing.Point(3, 3);
        this.groupBoxSimpleImage.Name = "groupBoxSimpleImage";
        this.groupBoxSimpleImage.Size = new System.Drawing.Size(64, 72);
        this.groupBoxSimpleImage.TabIndex = 17;
        this.groupBoxSimpleImage.TabStop = false;
        this.groupBoxSimpleImage.Text = "Image";
        // 
        // pictureBoxSimple
        // 
        this.pictureBoxSimple.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.pictureBoxSimple.Location = new System.Drawing.Point(16, 24);
        this.pictureBoxSimple.Name = "pictureBoxSimple";
        this.pictureBoxSimple.Size = new System.Drawing.Size(32, 32);
        this.pictureBoxSimple.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        this.pictureBoxSimple.TabIndex = 0;
        this.pictureBoxSimple.TabStop = false;
        this.pictureBoxSimple.MouseEnter += new System.EventHandler(this.PicOff_MouseEnter);
        this.pictureBoxSimple.MouseLeave += new System.EventHandler(this.PicOff_MouseLeave);
        // 
        // groupBoxOffBitmap
        // 
        this.groupBoxOffBitmap.Controls.Add(this.pictureBoxOff);
        this.groupBoxOffBitmap.ForeColor = System.Drawing.Color.DarkOrange;
        this.groupBoxOffBitmap.Location = new System.Drawing.Point(73, 3);
        this.groupBoxOffBitmap.Name = "groupBoxOffBitmap";
        this.groupBoxOffBitmap.Size = new System.Drawing.Size(64, 72);
        this.groupBoxOffBitmap.TabIndex = 15;
        this.groupBoxOffBitmap.TabStop = false;
        this.groupBoxOffBitmap.Text = "Off";
        // 
        // pictureBoxOff
        // 
        this.pictureBoxOff.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.pictureBoxOff.Location = new System.Drawing.Point(16, 24);
        this.pictureBoxOff.Name = "pictureBoxOff";
        this.pictureBoxOff.Size = new System.Drawing.Size(32, 32);
        this.pictureBoxOff.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        this.pictureBoxOff.TabIndex = 0;
        this.pictureBoxOff.TabStop = false;
        this.pictureBoxOff.MouseEnter += new System.EventHandler(this.PicOff_MouseEnter);
        this.pictureBoxOff.MouseLeave += new System.EventHandler(this.PicOff_MouseLeave);
        // 
        // groupBoxOnBitmap
        // 
        this.groupBoxOnBitmap.Controls.Add(this.pictureBoxOn);
        this.groupBoxOnBitmap.ForeColor = System.Drawing.Color.DarkOrange;
        this.groupBoxOnBitmap.Location = new System.Drawing.Point(143, 3);
        this.groupBoxOnBitmap.Name = "groupBoxOnBitmap";
        this.groupBoxOnBitmap.Size = new System.Drawing.Size(64, 72);
        this.groupBoxOnBitmap.TabIndex = 14;
        this.groupBoxOnBitmap.TabStop = false;
        this.groupBoxOnBitmap.Text = "On";
        // 
        // pictureBoxOn
        // 
        this.pictureBoxOn.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.pictureBoxOn.Location = new System.Drawing.Point(16, 24);
        this.pictureBoxOn.Name = "pictureBoxOn";
        this.pictureBoxOn.Size = new System.Drawing.Size(32, 32);
        this.pictureBoxOn.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        this.pictureBoxOn.TabIndex = 0;
        this.pictureBoxOn.TabStop = false;
        this.pictureBoxOn.MouseEnter += new System.EventHandler(this.PicOff_MouseEnter);
        this.pictureBoxOn.MouseLeave += new System.EventHandler(this.PicOff_MouseLeave);
        // 
        // groupBoxOverBitmap
        // 
        this.groupBoxOverBitmap.Controls.Add(this.pictureBoxOver);
        this.groupBoxOverBitmap.ForeColor = System.Drawing.Color.DarkOrange;
        this.groupBoxOverBitmap.Location = new System.Drawing.Point(213, 3);
        this.groupBoxOverBitmap.Name = "groupBoxOverBitmap";
        this.groupBoxOverBitmap.Size = new System.Drawing.Size(64, 72);
        this.groupBoxOverBitmap.TabIndex = 16;
        this.groupBoxOverBitmap.TabStop = false;
        this.groupBoxOverBitmap.Text = "Over";
        // 
        // pictureBoxOver
        // 
        this.pictureBoxOver.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.pictureBoxOver.Location = new System.Drawing.Point(16, 24);
        this.pictureBoxOver.Name = "pictureBoxOver";
        this.pictureBoxOver.Size = new System.Drawing.Size(32, 32);
        this.pictureBoxOver.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        this.pictureBoxOver.TabIndex = 0;
        this.pictureBoxOver.TabStop = false;
        this.pictureBoxOver.MouseEnter += new System.EventHandler(this.PicOff_MouseEnter);
        this.pictureBoxOver.MouseLeave += new System.EventHandler(this.PicOff_MouseLeave);
        // 
        // flowLayoutPanelUpperParts
        // 
        this.flowLayoutPanelUpperParts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.flowLayoutPanelUpperParts.AutoSize = true;
        this.flowLayoutPanelUpperParts.Controls.Add(this.groupBoxEdit);
        this.flowLayoutPanelUpperParts.Controls.Add(this.groupBoxSelectedPictures);
        this.flowLayoutPanelUpperParts.Location = new System.Drawing.Point(26, 69);
        this.flowLayoutPanelUpperParts.Name = "flowLayoutPanelUpperParts";
        this.flowLayoutPanelUpperParts.Size = new System.Drawing.Size(996, 192);
        this.flowLayoutPanelUpperParts.TabIndex = 11;
        // 
        // groupBoxAvailablePictures
        // 
        this.groupBoxAvailablePictures.Dock = System.Windows.Forms.DockStyle.Fill;
        this.groupBoxAvailablePictures.Controls.Add(this.tabControlPics);
        this.groupBoxAvailablePictures.Controls.Add(this.scalingLabelDragDropNotice);
        this.groupBoxAvailablePictures.ForeColor = System.Drawing.Color.Gold;
        this.groupBoxAvailablePictures.Location = new System.Drawing.Point(26, 251);
        this.groupBoxAvailablePictures.Name = "groupBoxAvailablePictures";
        this.groupBoxAvailablePictures.Padding = new System.Windows.Forms.Padding(14, 4, 20, 12);
        this.groupBoxAvailablePictures.Size = new System.Drawing.Size(996, 363);
        this.groupBoxAvailablePictures.TabIndex = 12;
        this.groupBoxAvailablePictures.TabStop = false;
        this.groupBoxAvailablePictures.Text = "Available pictures";
        // 
        // tabControlPics
        // 
        this.tabControlPics.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tabControlPics.Controls.Add(this.tabPageSkills);
        this.tabControlPics.Controls.Add(this.tabPageRelics);
        this.tabControlPics.Controls.Add(this.tabPageArtifacts);
        this.tabControlPics.Controls.Add(this.tabPageJewellery);
        this.tabControlPics.Controls.Add(this.tabPageScrolls);
        this.tabControlPics.Controls.Add(this.tabPagePotions);
        this.tabControlPics.Controls.Add(this.tabPageButtons);
        this.tabControlPics.Controls.Add(this.tabPageMisc);
        this.tabControlPics.Controls.Add(this.tabPageHelmets);
        this.tabControlPics.Controls.Add(this.tabPageShields);
        this.tabControlPics.Controls.Add(this.tabPageArmbands);
        this.tabControlPics.Controls.Add(this.tabPageGreaves);
        this.tabControlPics.Location = new System.Drawing.Point(17, 39);
        this.tabControlPics.Name = "tabControlPics";
        this.tabControlPics.SelectedIndex = 0;
        this.tabControlPics.Size = new System.Drawing.Size(956, 309);
        this.tabControlPics.TabIndex = 3;
        // 
        // tabPageSkills
        // 
        this.tabPageSkills.Controls.Add(this.iconGridSkills);
        this.tabPageSkills.Location = new System.Drawing.Point(4, 27);
        this.tabPageSkills.Name = "tabPageSkills";
        this.tabPageSkills.Size = new System.Drawing.Size(948, 278);
        this.tabPageSkills.TabIndex = 6;
        this.tabPageSkills.Text = "Skills";
        this.tabPageSkills.UseVisualStyleBackColor = true;
        // 
        // iconGridSkills
        // 
        this.iconGridSkills.AutoScroll = true;
        this.iconGridSkills.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
        this.iconGridSkills.Dock = System.Windows.Forms.DockStyle.Fill;
        this.iconGridSkills.Location = new System.Drawing.Point(0, 0);
        this.iconGridSkills.Name = "iconGridSkills";
        this.iconGridSkills.Size = new System.Drawing.Size(948, 278);
        this.iconGridSkills.TabIndex = 3;
        // 
        // tabPageRelics
        // 
        this.tabPageRelics.Controls.Add(this.iconGridRelics);
        this.tabPageRelics.Location = new System.Drawing.Point(4, 27);
        this.tabPageRelics.Name = "tabPageRelics";
        this.tabPageRelics.Padding = new System.Windows.Forms.Padding(3);
        this.tabPageRelics.Size = new System.Drawing.Size(948, 278);
        this.tabPageRelics.TabIndex = 2;
        this.tabPageRelics.Text = "Relics";
        this.tabPageRelics.UseVisualStyleBackColor = true;
        // 
        // iconGridRelics
        // 
        this.iconGridRelics.AutoScroll = true;
        this.iconGridRelics.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
        this.iconGridRelics.Dock = System.Windows.Forms.DockStyle.Fill;
        this.iconGridRelics.Location = new System.Drawing.Point(3, 3);
        this.iconGridRelics.Name = "iconGridRelics";
        this.iconGridRelics.Size = new System.Drawing.Size(942, 272);
        this.iconGridRelics.TabIndex = 3;
        // 
        // tabPageArtifacts
        // 
        this.tabPageArtifacts.Controls.Add(this.iconGridArtifacts);
        this.tabPageArtifacts.Location = new System.Drawing.Point(4, 27);
        this.tabPageArtifacts.Name = "tabPageArtifacts";
        this.tabPageArtifacts.Padding = new System.Windows.Forms.Padding(3);
        this.tabPageArtifacts.Size = new System.Drawing.Size(948, 278);
        this.tabPageArtifacts.TabIndex = 1;
        this.tabPageArtifacts.Text = "Artifacts";
        this.tabPageArtifacts.UseVisualStyleBackColor = true;
        // 
        // iconGridArtifacts
        // 
        this.iconGridArtifacts.AutoScroll = true;
        this.iconGridArtifacts.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
        this.iconGridArtifacts.Dock = System.Windows.Forms.DockStyle.Fill;
        this.iconGridArtifacts.Location = new System.Drawing.Point(3, 3);
        this.iconGridArtifacts.Name = "iconGridArtifacts";
        this.iconGridArtifacts.Size = new System.Drawing.Size(942, 272);
        this.iconGridArtifacts.TabIndex = 3;
        // 
        // tabPageJewellery
        // 
        this.tabPageJewellery.Controls.Add(this.iconGridJewellery);
        this.tabPageJewellery.Location = new System.Drawing.Point(4, 27);
        this.tabPageJewellery.Name = "tabPageJewellery";
        this.tabPageJewellery.Size = new System.Drawing.Size(948, 278);
        this.tabPageJewellery.TabIndex = 3;
        this.tabPageJewellery.Text = "Jewellery";
        this.tabPageJewellery.UseVisualStyleBackColor = true;
        // 
        // iconGridJewellery
        // 
        this.iconGridJewellery.AutoScroll = true;
        this.iconGridJewellery.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
        this.iconGridJewellery.Dock = System.Windows.Forms.DockStyle.Fill;
        this.iconGridJewellery.Location = new System.Drawing.Point(0, 0);
        this.iconGridJewellery.Name = "iconGridJewellery";
        this.iconGridJewellery.Size = new System.Drawing.Size(948, 278);
        this.iconGridJewellery.TabIndex = 3;
        // 
        // tabPageScrolls
        // 
        this.tabPageScrolls.Controls.Add(this.iconGridScrolls);
        this.tabPageScrolls.Location = new System.Drawing.Point(4, 27);
        this.tabPageScrolls.Name = "tabPageScrolls";
        this.tabPageScrolls.Size = new System.Drawing.Size(948, 278);
        this.tabPageScrolls.TabIndex = 5;
        this.tabPageScrolls.Text = "Scrolls";
        this.tabPageScrolls.UseVisualStyleBackColor = true;
        // 
        // iconGridScrolls
        // 
        this.iconGridScrolls.AutoScroll = true;
        this.iconGridScrolls.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
        this.iconGridScrolls.Dock = System.Windows.Forms.DockStyle.Fill;
        this.iconGridScrolls.Location = new System.Drawing.Point(0, 0);
        this.iconGridScrolls.Name = "iconGridScrolls";
        this.iconGridScrolls.Size = new System.Drawing.Size(948, 278);
        this.iconGridScrolls.TabIndex = 3;
        // 
        // tabPagePotions
        // 
        this.tabPagePotions.Controls.Add(this.iconGridPotions);
        this.tabPagePotions.Location = new System.Drawing.Point(4, 27);
        this.tabPagePotions.Name = "tabPagePotions";
        this.tabPagePotions.Size = new System.Drawing.Size(948, 278);
        this.tabPagePotions.TabIndex = 4;
        this.tabPagePotions.Text = "Potions";
        this.tabPagePotions.UseVisualStyleBackColor = true;
        // 
        // iconGridPotions
        // 
        this.iconGridPotions.AutoScroll = true;
        this.iconGridPotions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
        this.iconGridPotions.Dock = System.Windows.Forms.DockStyle.Fill;
        this.iconGridPotions.Location = new System.Drawing.Point(0, 0);
        this.iconGridPotions.Name = "iconGridPotions";
        this.iconGridPotions.Size = new System.Drawing.Size(948, 278);
        this.iconGridPotions.TabIndex = 3;
        // 
        // tabPageButtons
        // 
        this.tabPageButtons.Controls.Add(this.iconGridButtons);
        this.tabPageButtons.Location = new System.Drawing.Point(4, 27);
        this.tabPageButtons.Name = "tabPageButtons";
        this.tabPageButtons.Size = new System.Drawing.Size(948, 278);
        this.tabPageButtons.TabIndex = 7;
        this.tabPageButtons.Text = "Buttons";
        this.tabPageButtons.UseVisualStyleBackColor = true;
        // 
        // iconGridButtons
        // 
        this.iconGridButtons.AutoScroll = true;
        this.iconGridButtons.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
        this.iconGridButtons.Dock = System.Windows.Forms.DockStyle.Fill;
        this.iconGridButtons.Location = new System.Drawing.Point(0, 0);
        this.iconGridButtons.Name = "iconGridButtons";
        this.iconGridButtons.Size = new System.Drawing.Size(948, 278);
        this.iconGridButtons.TabIndex = 3;
        // 
        // tabPageMisc
        // 
        this.tabPageMisc.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        this.tabPageMisc.Controls.Add(this.iconGridMisc);
        this.tabPageMisc.Location = new System.Drawing.Point(4, 27);
        this.tabPageMisc.Name = "tabPageMisc";
        this.tabPageMisc.Padding = new System.Windows.Forms.Padding(3);
        this.tabPageMisc.Size = new System.Drawing.Size(948, 278);
        this.tabPageMisc.TabIndex = 0;
        this.tabPageMisc.Text = "Misc";
        this.tabPageMisc.UseVisualStyleBackColor = true;
        // 
        // iconGridMisc
        // 
        this.iconGridMisc.AutoScroll = true;
        this.iconGridMisc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
        this.iconGridMisc.Dock = System.Windows.Forms.DockStyle.Fill;
        this.iconGridMisc.Location = new System.Drawing.Point(3, 3);
        this.iconGridMisc.Name = "iconGridMisc";
        this.iconGridMisc.Size = new System.Drawing.Size(942, 272);
        this.iconGridMisc.TabIndex = 2;
        // 
        // tabPageHelmets
        // 
        this.tabPageHelmets.Controls.Add(this.iconGridHelmets);
        this.tabPageHelmets.Location = new System.Drawing.Point(4, 27);
        this.tabPageHelmets.Name = "tabPageHelmets";
        this.tabPageHelmets.Padding = new System.Windows.Forms.Padding(3);
        this.tabPageHelmets.Size = new System.Drawing.Size(948, 278);
        this.tabPageHelmets.TabIndex = 8;
        this.tabPageHelmets.Text = "Helmets";
        this.tabPageHelmets.UseVisualStyleBackColor = true;
        // 
        // iconGridHelmets
        // 
        this.iconGridHelmets.AutoScroll = true;
        this.iconGridHelmets.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
        this.iconGridHelmets.Dock = System.Windows.Forms.DockStyle.Fill;
        this.iconGridHelmets.Location = new System.Drawing.Point(3, 3);
        this.iconGridHelmets.Name = "iconGridHelmets";
        this.iconGridHelmets.Size = new System.Drawing.Size(942, 272);
        this.iconGridHelmets.TabIndex = 3;
        // 
        // tabPageShields
        // 
        this.tabPageShields.Controls.Add(this.iconGridShields);
        this.tabPageShields.Location = new System.Drawing.Point(4, 27);
        this.tabPageShields.Name = "tabPageShields";
        this.tabPageShields.Padding = new System.Windows.Forms.Padding(3);
        this.tabPageShields.Size = new System.Drawing.Size(948, 278);
        this.tabPageShields.TabIndex = 9;
        this.tabPageShields.Text = "Shields";
        this.tabPageShields.UseVisualStyleBackColor = true;
        // 
        // iconGridShields
        // 
        this.iconGridShields.AutoScroll = true;
        this.iconGridShields.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
        this.iconGridShields.Dock = System.Windows.Forms.DockStyle.Fill;
        this.iconGridShields.Location = new System.Drawing.Point(3, 3);
        this.iconGridShields.Name = "iconGridShields";
        this.iconGridShields.Size = new System.Drawing.Size(942, 272);
        this.iconGridShields.TabIndex = 4;
        // 
        // tabPageArmbands
        // 
        this.tabPageArmbands.Controls.Add(this.iconGridArmbands);
        this.tabPageArmbands.Location = new System.Drawing.Point(4, 27);
        this.tabPageArmbands.Name = "tabPageArmbands";
        this.tabPageArmbands.Padding = new System.Windows.Forms.Padding(3);
        this.tabPageArmbands.Size = new System.Drawing.Size(948, 278);
        this.tabPageArmbands.TabIndex = 10;
        this.tabPageArmbands.Text = "Armbands";
        this.tabPageArmbands.UseVisualStyleBackColor = true;
        // 
        // iconGridArmbands
        // 
        this.iconGridArmbands.AutoScroll = true;
        this.iconGridArmbands.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
        this.iconGridArmbands.Dock = System.Windows.Forms.DockStyle.Fill;
        this.iconGridArmbands.Location = new System.Drawing.Point(3, 3);
        this.iconGridArmbands.Name = "iconGridArmbands";
        this.iconGridArmbands.Size = new System.Drawing.Size(942, 272);
        this.iconGridArmbands.TabIndex = 5;
        // 
        // tabPageGreaves
        // 
        this.tabPageGreaves.Controls.Add(this.iconGridGreaves);
        this.tabPageGreaves.Location = new System.Drawing.Point(4, 27);
        this.tabPageGreaves.Name = "tabPageGreaves";
        this.tabPageGreaves.Size = new System.Drawing.Size(948, 278);
        this.tabPageGreaves.TabIndex = 11;
        this.tabPageGreaves.Text = "Greaves";
        this.tabPageGreaves.UseVisualStyleBackColor = true;
        // 
        // iconGridGreaves
        // 
        this.iconGridGreaves.AutoScroll = true;
        this.iconGridGreaves.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
        this.iconGridGreaves.Dock = System.Windows.Forms.DockStyle.Fill;
        this.iconGridGreaves.Location = new System.Drawing.Point(0, 0);
        this.iconGridGreaves.Name = "iconGridGreaves";
        this.iconGridGreaves.Size = new System.Drawing.Size(948, 278);
        this.iconGridGreaves.TabIndex = 6;
        // 
        // scalingLabelDragDropNotice
        // 
        this.scalingLabelDragDropNotice.AutoSize = true;
        this.scalingLabelDragDropNotice.Dock = System.Windows.Forms.DockStyle.Top;
        this.scalingLabelDragDropNotice.ForeColor = System.Drawing.Color.White;
        this.scalingLabelDragDropNotice.Location = new System.Drawing.Point(14, 19);
        this.scalingLabelDragDropNotice.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
        this.scalingLabelDragDropNotice.Name = "scalingLabelDragDropNotice";
        this.scalingLabelDragDropNotice.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
        this.scalingLabelDragDropNotice.Size = new System.Drawing.Size(318, 17);
        this.scalingLabelDragDropNotice.TabIndex = 1;
        this.scalingLabelDragDropNotice.Tag = "";
        this.scalingLabelDragDropNotice.Text = "Drag and drop in \"Selected Pictures\" placeholders";
        // 
        // iconMagnifier
        // 
        this.iconMagnifier.AutoSize = true;
        this.iconMagnifier.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.iconMagnifier.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
        this.iconMagnifier.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.iconMagnifier.Location = new System.Drawing.Point(845, 0);
        this.iconMagnifier.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        this.iconMagnifier.Name = "iconMagnifier";
        this.iconMagnifier.Size = new System.Drawing.Size(77, 99);
        this.iconMagnifier.TabIndex = 14;
        this.iconMagnifier.Visible = false;
        // 
        // BagButtonSettings
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
        this.ClientSize = new System.Drawing.Size(1047, 680);
        this.Controls.Add(this.iconMagnifier);
        this.Controls.Add(this.tableLayoutPanelSkeleton);
        this.DrawCustomBorder = true;
        this.ForeColor = System.Drawing.Color.White;
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.MinimumSize = new System.Drawing.Size(630, 586);
        this.Name = "BagButtonSettings";
        this.ResizeCustomAllowed = true;
        this.ShowIcon = false;
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Vault button settings";
        this.TitleTextColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.TopMost = true;
        this.Load += new System.EventHandler(this.BagButtonSettings_Load);
        this.Controls.SetChildIndex(this.tableLayoutPanelSkeleton, 0);
        this.Controls.SetChildIndex(this.iconMagnifier, 0);
        this.tableLayoutPanelSkeleton.ResumeLayout(false);
        this.tableLayoutPanelSkeleton.PerformLayout();
        this.tableLayoutPanelBottom.ResumeLayout(false);
        this.tableLayoutPanelBottom.PerformLayout();
        this.flowLayoutPanelSelectedButton.ResumeLayout(false);
        this.flowLayoutPanelSelectedButton.PerformLayout();
        this.groupBoxEdit.ResumeLayout(false);
        this.groupBoxEdit.PerformLayout();
        this.flowLayoutPanelRadioProperties.ResumeLayout(false);
        this.flowLayoutPanelRadioProperties.PerformLayout();
        this.flowLayoutPanelLabelInput.ResumeLayout(false);
        this.flowLayoutPanelLabelInput.PerformLayout();
        this.groupBoxSelectedPictures.ResumeLayout(false);
        this.groupBoxSelectedPictures.PerformLayout();
        this.flowLayoutPanelPicturePickupMode.ResumeLayout(false);
        this.flowLayoutPanelPicturePickupMode.PerformLayout();
        this.flowLayoutPanelPicturePickup.ResumeLayout(false);
        this.groupBoxSimpleImage.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSimple)).EndInit();
        this.groupBoxOffBitmap.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOff)).EndInit();
        this.groupBoxOnBitmap.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOn)).EndInit();
        this.groupBoxOverBitmap.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOver)).EndInit();
        this.flowLayoutPanelUpperParts.ResumeLayout(false);
        this.flowLayoutPanelUpperParts.PerformLayout();
        this.groupBoxAvailablePictures.ResumeLayout(false);
        this.groupBoxAvailablePictures.PerformLayout();
        this.tabControlPics.ResumeLayout(false);
        this.tabPageSkills.ResumeLayout(false);
        this.tabPageRelics.ResumeLayout(false);
        this.tabPageArtifacts.ResumeLayout(false);
        this.tabPageJewellery.ResumeLayout(false);
        this.tabPageScrolls.ResumeLayout(false);
        this.tabPagePotions.ResumeLayout(false);
        this.tabPageButtons.ResumeLayout(false);
        this.tabPageMisc.ResumeLayout(false);
        this.tabPageHelmets.ResumeLayout(false);
        this.tabPageShields.ResumeLayout(false);
        this.tabPageArmbands.ResumeLayout(false);
        this.tabPageGreaves.ResumeLayout(false);
        this.ResumeLayout(false);
        this.PerformLayout();

	}

	#endregion

	private System.Windows.Forms.TableLayoutPanel tableLayoutPanelSkeleton;
	private System.Windows.Forms.TableLayoutPanel tableLayoutPanelBottom;
	private ScalingButton applyButton;
	private ScalingButton cancelButton;
	private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelSelectedButton;
	private ScalingLabel scalingLabelSelectedButton;
	private ScalingLabel scalingLabelSelectedButtonValue;
	private System.Windows.Forms.GroupBox groupBoxEdit;
	private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelLabelInput;
	private ScalingTextBox scalingTextBoxLabel;
	private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelRadioProperties;
	private System.Windows.Forms.GroupBox groupBoxSelectedPictures;
	private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelPicturePickupMode;
	private System.Windows.Forms.RadioButton radioButtonPictureSimple;
	private System.Windows.Forms.RadioButton radioButtonPictureDetailed;
	private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelPicturePickup;
	private System.Windows.Forms.GroupBox groupBoxOnBitmap;
	private System.Windows.Forms.PictureBox pictureBoxOn;
	private System.Windows.Forms.GroupBox groupBoxOffBitmap;
	private System.Windows.Forms.PictureBox pictureBoxOff;
	private System.Windows.Forms.GroupBox groupBoxOverBitmap;
	private System.Windows.Forms.PictureBox pictureBoxOver;
	private System.Windows.Forms.GroupBox groupBoxSimpleImage;
	private System.Windows.Forms.PictureBox pictureBoxSimple;
	private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelUpperParts;
	private System.Windows.Forms.GroupBox groupBoxAvailablePictures;
	private ScalingLabel scalingLabelDragDropNotice;
	private TQVaultAE.GUI.Components.Icons.IconGrid iconGridMisc;
	private ScalingTextBox scalingTextBoxDebug;
	private TQVaultAE.GUI.Components.Layout.CaravanTabControl tabControlPics;
	private System.Windows.Forms.TabPage tabPageMisc;
	private System.Windows.Forms.TabPage tabPageArtifacts;
	private System.Windows.Forms.TabPage tabPageRelics;
	private System.Windows.Forms.TabPage tabPageJewellery;
	private System.Windows.Forms.TabPage tabPagePotions;
	private System.Windows.Forms.TabPage tabPageScrolls;
	private System.Windows.Forms.TabPage tabPageSkills;
	private System.Windows.Forms.TabPage tabPageButtons;
	private TQVaultAE.GUI.Components.Icons.IconGrid iconGridArtifacts;
	private TQVaultAE.GUI.Components.Icons.IconGrid iconGridRelics;
	private TQVaultAE.GUI.Components.Icons.IconGrid iconGridJewellery;
	private TQVaultAE.GUI.Components.Icons.IconGrid iconGridPotions;
	private TQVaultAE.GUI.Components.Icons.IconGrid iconGridScrolls;
	private TQVaultAE.GUI.Components.Icons.IconGrid iconGridSkills;
	private TQVaultAE.GUI.Components.Icons.IconGrid iconGridButtons;
	private ScalingCheckBox scalingCheckBoxNumber;
	private ScalingCheckBox scalingCheckBoxLabel;
	private ScalingRadioButton scalingRadioButtonDefaultIcon;
	private ScalingRadioButton scalingRadioButtonCustomIcon;
	private System.Windows.Forms.TabPage tabPageHelmets;
	private TQVaultAE.GUI.Components.Icons.IconGrid iconGridHelmets;
	private System.Windows.Forms.TabPage tabPageShields;
	private TQVaultAE.GUI.Components.Icons.IconGrid iconGridShields;
	private System.Windows.Forms.TabPage tabPageArmbands;
	private TQVaultAE.GUI.Components.Icons.IconGrid iconGridArmbands;
	private System.Windows.Forms.TabPage tabPageGreaves;
	private TQVaultAE.GUI.Components.Icons.IconGrid iconGridGreaves;
	private IconMagnifier iconMagnifier;
}
