namespace TQVaultAE.GUI.Components
{
	partial class ForgePanel
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ForgePanel));
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.pictureBoxDragDrop = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanelForge = new TQVaultAE.GUI.Components.BufferedTableLayoutPanel();
            this.scalingLabelSuffix = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingLabelPrefix = new TQVaultAE.GUI.Components.ScalingLabel();
            this.CancelButton = new TQVaultAE.GUI.Components.ScalingButton();
            this.ForgeButton = new TQVaultAE.GUI.Components.ScalingButton();
            this.pictureBoxRelic2 = new System.Windows.Forms.PictureBox();
            this.pictureBoxSuffix = new System.Windows.Forms.PictureBox();
            this.pictureBoxRelic1 = new System.Windows.Forms.PictureBox();
            this.pictureBoxPrefix = new System.Windows.Forms.PictureBox();
            this.pictureBoxBaseItem = new System.Windows.Forms.PictureBox();
            this.flowLayoutPanelPropertiesLeft = new TQVaultAE.GUI.Components.BufferedFlowLayoutPanel();
            this.scalingLabelRelic1 = new TQVaultAE.GUI.Components.ScalingLabel();
            this.flowLayoutPanel2PropertiesRight = new TQVaultAE.GUI.Components.BufferedFlowLayoutPanel();
            this.scalingLabelRelic2 = new TQVaultAE.GUI.Components.ScalingLabel();
            this.flowLayoutPanelTop = new TQVaultAE.GUI.Components.BufferedFlowLayoutPanel();
            this.scalingLabelBaseItem = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingRadioButtonGod = new TQVaultAE.GUI.Components.ScalingRadioButton();
            this.scalingRadioButtonRelax = new TQVaultAE.GUI.Components.ScalingRadioButton();
            this.scalingRadioButtonStrict = new TQVaultAE.GUI.Components.ScalingRadioButton();
            this.scalingRadioButtonGame = new TQVaultAE.GUI.Components.ScalingRadioButton();
            this.comboBoxSuffix = new System.Windows.Forms.ComboBox();
            this.comboBoxPrefix = new System.Windows.Forms.ComboBox();
            this.comboBoxRelic1 = new System.Windows.Forms.ComboBox();
            this.comboBoxRelic2 = new System.Windows.Forms.ComboBox();
            this.ResetButton = new TQVaultAE.GUI.Components.ScalingButton();
            this.flowLayoutPanelBottom = new TQVaultAE.GUI.Components.BufferedFlowLayoutPanel();
            this.scalingCheckBoxHardcore = new TQVaultAE.GUI.Components.ScalingCheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxDragDrop)).BeginInit();
            this.tableLayoutPanelForge.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxRelic2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSuffix)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxRelic1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPrefix)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBaseItem)).BeginInit();
            this.flowLayoutPanelPropertiesLeft.SuspendLayout();
            this.flowLayoutPanel2PropertiesRight.SuspendLayout();
            this.flowLayoutPanelTop.SuspendLayout();
            this.flowLayoutPanelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBoxDragDrop
            // 
            this.pictureBoxDragDrop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(41)))), ((int)(((byte)(31)))));
            this.pictureBoxDragDrop.Location = new System.Drawing.Point(466, 21);
            this.pictureBoxDragDrop.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBoxDragDrop.Name = "pictureBoxDragDrop";
            this.pictureBoxDragDrop.Size = new System.Drawing.Size(78, 95);
            this.pictureBoxDragDrop.TabIndex = 1;
            this.pictureBoxDragDrop.TabStop = false;
            this.pictureBoxDragDrop.Visible = false;
            this.pictureBoxDragDrop.Click += new System.EventHandler(this.pictureBoxDragDrop_Click);
            this.pictureBoxDragDrop.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBoxDragDrop_MouseMove);
            // 
            // tableLayoutPanelForge
            // 
            this.tableLayoutPanelForge.AutoSize = true;
            this.tableLayoutPanelForge.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanelForge.ColumnCount = 6;
            this.tableLayoutPanelForge.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelForge.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelForge.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelForge.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelForge.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelForge.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelForge.Controls.Add(this.scalingLabelSuffix, 4, 1);
            this.tableLayoutPanelForge.Controls.Add(this.scalingLabelPrefix, 1, 1);
            this.tableLayoutPanelForge.Controls.Add(this.CancelButton, 1, 7);
            this.tableLayoutPanelForge.Controls.Add(this.ForgeButton, 4, 7);
            this.tableLayoutPanelForge.Controls.Add(this.pictureBoxRelic2, 4, 5);
            this.tableLayoutPanelForge.Controls.Add(this.pictureBoxSuffix, 4, 2);
            this.tableLayoutPanelForge.Controls.Add(this.pictureBoxRelic1, 1, 5);
            this.tableLayoutPanelForge.Controls.Add(this.pictureBoxPrefix, 1, 2);
            this.tableLayoutPanelForge.Controls.Add(this.pictureBoxBaseItem, 2, 4);
            this.tableLayoutPanelForge.Controls.Add(this.flowLayoutPanelPropertiesLeft, 1, 4);
            this.tableLayoutPanelForge.Controls.Add(this.flowLayoutPanel2PropertiesRight, 4, 4);
            this.tableLayoutPanelForge.Controls.Add(this.flowLayoutPanelTop, 2, 2);
            this.tableLayoutPanelForge.Controls.Add(this.comboBoxSuffix, 4, 3);
            this.tableLayoutPanelForge.Controls.Add(this.comboBoxPrefix, 1, 3);
            this.tableLayoutPanelForge.Controls.Add(this.comboBoxRelic1, 1, 6);
            this.tableLayoutPanelForge.Controls.Add(this.comboBoxRelic2, 4, 6);
            this.tableLayoutPanelForge.Controls.Add(this.ResetButton, 2, 7);
            this.tableLayoutPanelForge.Controls.Add(this.flowLayoutPanelBottom, 2, 5);
            this.tableLayoutPanelForge.Location = new System.Drawing.Point(15, 15);
            this.tableLayoutPanelForge.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanelForge.Name = "tableLayoutPanelForge";
            this.tableLayoutPanelForge.RowCount = 9;
            this.tableLayoutPanelForge.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelForge.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForge.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForge.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForge.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForge.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForge.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForge.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelForge.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelForge.Size = new System.Drawing.Size(432, 730);
            this.tableLayoutPanelForge.TabIndex = 0;
            // 
            // scalingLabelSuffix
            // 
            this.scalingLabelSuffix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.scalingLabelSuffix.Font = new System.Drawing.Font("Albertus MT", 9.75F, System.Drawing.FontStyle.Bold);
            this.scalingLabelSuffix.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelSuffix.Location = new System.Drawing.Point(284, 20);
            this.scalingLabelSuffix.Margin = new System.Windows.Forms.Padding(3, 0, 0, 3);
            this.scalingLabelSuffix.Name = "scalingLabelSuffix";
            this.scalingLabelSuffix.Size = new System.Drawing.Size(128, 15);
            this.scalingLabelSuffix.TabIndex = 0;
            this.scalingLabelSuffix.Text = "Suffix Item";
            this.scalingLabelSuffix.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // scalingLabelPrefix
            // 
            this.scalingLabelPrefix.Font = new System.Drawing.Font("Albertus MT", 9.75F, System.Drawing.FontStyle.Bold);
            this.scalingLabelPrefix.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelPrefix.Location = new System.Drawing.Point(20, 20);
            this.scalingLabelPrefix.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.scalingLabelPrefix.Name = "scalingLabelPrefix";
            this.scalingLabelPrefix.Size = new System.Drawing.Size(128, 15);
            this.scalingLabelPrefix.TabIndex = 0;
            this.scalingLabelPrefix.Text = "Prefix Item";
            this.scalingLabelPrefix.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CancelButton
            // 
            this.CancelButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CancelButton.BackColor = System.Drawing.Color.Transparent;
            this.CancelButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("CancelButton.DownBitmap")));
            this.CancelButton.FlatAppearance.BorderSize = 0;
            this.CancelButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.CancelButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.CancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CancelButton.Font = new System.Drawing.Font("Albertus MT", 12F);
            this.CancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.CancelButton.Image = ((System.Drawing.Image)(resources.GetObject("CancelButton.Image")));
            this.CancelButton.Location = new System.Drawing.Point(36, 670);
            this.CancelButton.Margin = new System.Windows.Forms.Padding(0, 10, 0, 10);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("CancelButton.OverBitmap")));
            this.CancelButton.Size = new System.Drawing.Size(100, 30);
            this.CancelButton.SizeToGraphic = false;
            this.CancelButton.TabIndex = 6;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("CancelButton.UpBitmap")));
            this.CancelButton.UseCustomGraphic = true;
            this.CancelButton.UseVisualStyleBackColor = false;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // ForgeButton
            // 
            this.ForgeButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.ForgeButton.BackColor = System.Drawing.Color.Transparent;
            this.ForgeButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ForgeButton.DownBitmap")));
            this.ForgeButton.FlatAppearance.BorderSize = 0;
            this.ForgeButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ForgeButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ForgeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ForgeButton.Font = new System.Drawing.Font("Albertus MT", 12F);
            this.ForgeButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ForgeButton.Image = ((System.Drawing.Image)(resources.GetObject("ForgeButton.Image")));
            this.ForgeButton.Location = new System.Drawing.Point(296, 670);
            this.ForgeButton.Margin = new System.Windows.Forms.Padding(0);
            this.ForgeButton.Name = "ForgeButton";
            this.ForgeButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ForgeButton.OverBitmap")));
            this.ForgeButton.Size = new System.Drawing.Size(100, 30);
            this.ForgeButton.SizeToGraphic = false;
            this.ForgeButton.TabIndex = 5;
            this.ForgeButton.Text = "Forge";
            this.ForgeButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ForgeButton.UpBitmap")));
            this.ForgeButton.UseCustomGraphic = true;
            this.ForgeButton.UseVisualStyleBackColor = false;
            this.ForgeButton.Click += new System.EventHandler(this.ForgeButton_Click);
            // 
            // pictureBoxRelic2
            // 
            this.pictureBoxRelic2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxRelic2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(41)))), ((int)(((byte)(31)))));
            this.pictureBoxRelic2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBoxRelic2.Location = new System.Drawing.Point(284, 445);
            this.pictureBoxRelic2.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.pictureBoxRelic2.Name = "pictureBoxRelic2";
            this.pictureBoxRelic2.Size = new System.Drawing.Size(128, 192);
            this.pictureBoxRelic2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBoxRelic2.TabIndex = 7;
            this.pictureBoxRelic2.TabStop = false;
            this.pictureBoxRelic2.MouseEnter += new System.EventHandler(this.pictureBox_MouseEnter);
            this.pictureBoxRelic2.MouseLeave += new System.EventHandler(this.pictureBox_MouseLeave);
            // 
            // pictureBoxSuffix
            // 
            this.pictureBoxSuffix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxSuffix.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(41)))), ((int)(((byte)(31)))));
            this.pictureBoxSuffix.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBoxSuffix.Location = new System.Drawing.Point(284, 38);
            this.pictureBoxSuffix.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.pictureBoxSuffix.Name = "pictureBoxSuffix";
            this.pictureBoxSuffix.Size = new System.Drawing.Size(128, 192);
            this.pictureBoxSuffix.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBoxSuffix.TabIndex = 8;
            this.pictureBoxSuffix.TabStop = false;
            this.pictureBoxSuffix.MouseEnter += new System.EventHandler(this.pictureBox_MouseEnter);
            this.pictureBoxSuffix.MouseLeave += new System.EventHandler(this.pictureBox_MouseLeave);
            // 
            // pictureBoxRelic1
            // 
            this.pictureBoxRelic1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(41)))), ((int)(((byte)(31)))));
            this.pictureBoxRelic1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBoxRelic1.Location = new System.Drawing.Point(20, 445);
            this.pictureBoxRelic1.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.pictureBoxRelic1.Name = "pictureBoxRelic1";
            this.pictureBoxRelic1.Size = new System.Drawing.Size(128, 192);
            this.pictureBoxRelic1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBoxRelic1.TabIndex = 9;
            this.pictureBoxRelic1.TabStop = false;
            this.pictureBoxRelic1.MouseEnter += new System.EventHandler(this.pictureBox_MouseEnter);
            this.pictureBoxRelic1.MouseLeave += new System.EventHandler(this.pictureBox_MouseLeave);
            // 
            // pictureBoxPrefix
            // 
            this.pictureBoxPrefix.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(41)))), ((int)(((byte)(31)))));
            this.pictureBoxPrefix.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBoxPrefix.Location = new System.Drawing.Point(20, 38);
            this.pictureBoxPrefix.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBoxPrefix.Name = "pictureBoxPrefix";
            this.pictureBoxPrefix.Size = new System.Drawing.Size(128, 192);
            this.pictureBoxPrefix.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBoxPrefix.TabIndex = 10;
            this.pictureBoxPrefix.TabStop = false;
            this.pictureBoxPrefix.MouseEnter += new System.EventHandler(this.pictureBox_MouseEnter);
            this.pictureBoxPrefix.MouseLeave += new System.EventHandler(this.pictureBox_MouseLeave);
            // 
            // pictureBoxBaseItem
            // 
            this.pictureBoxBaseItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(41)))), ((int)(((byte)(31)))));
            this.pictureBoxBaseItem.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tableLayoutPanelForge.SetColumnSpan(this.pictureBoxBaseItem, 2);
            this.pictureBoxBaseItem.Location = new System.Drawing.Point(152, 253);
            this.pictureBoxBaseItem.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBoxBaseItem.Name = "pictureBoxBaseItem";
            this.pictureBoxBaseItem.Size = new System.Drawing.Size(128, 192);
            this.pictureBoxBaseItem.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBoxBaseItem.TabIndex = 11;
            this.pictureBoxBaseItem.TabStop = false;
            this.pictureBoxBaseItem.MouseEnter += new System.EventHandler(this.pictureBox_MouseEnter);
            this.pictureBoxBaseItem.MouseLeave += new System.EventHandler(this.pictureBox_MouseLeave);
            // 
            // flowLayoutPanelPropertiesLeft
            // 
            this.flowLayoutPanelPropertiesLeft.AutoSize = true;
            this.flowLayoutPanelPropertiesLeft.Controls.Add(this.scalingLabelRelic1);
            this.flowLayoutPanelPropertiesLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelPropertiesLeft.FlowDirection = System.Windows.Forms.FlowDirection.BottomUp;
            this.flowLayoutPanelPropertiesLeft.Location = new System.Drawing.Point(20, 253);
            this.flowLayoutPanelPropertiesLeft.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanelPropertiesLeft.Name = "flowLayoutPanelPropertiesLeft";
            this.flowLayoutPanelPropertiesLeft.Size = new System.Drawing.Size(132, 192);
            this.flowLayoutPanelPropertiesLeft.TabIndex = 12;
            // 
            // scalingLabelRelic1
            // 
            this.scalingLabelRelic1.Font = new System.Drawing.Font("Albertus MT", 9.75F, System.Drawing.FontStyle.Bold);
            this.scalingLabelRelic1.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelRelic1.Location = new System.Drawing.Point(0, 174);
            this.scalingLabelRelic1.Margin = new System.Windows.Forms.Padding(0, 0, 3, 3);
            this.scalingLabelRelic1.Name = "scalingLabelRelic1";
            this.scalingLabelRelic1.Size = new System.Drawing.Size(128, 15);
            this.scalingLabelRelic1.TabIndex = 0;
            this.scalingLabelRelic1.Text = "First Relic";
            this.scalingLabelRelic1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // flowLayoutPanel2PropertiesRight
            // 
            this.flowLayoutPanel2PropertiesRight.AutoSize = true;
            this.flowLayoutPanel2PropertiesRight.Controls.Add(this.scalingLabelRelic2);
            this.flowLayoutPanel2PropertiesRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2PropertiesRight.FlowDirection = System.Windows.Forms.FlowDirection.BottomUp;
            this.flowLayoutPanel2PropertiesRight.Location = new System.Drawing.Point(280, 253);
            this.flowLayoutPanel2PropertiesRight.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel2PropertiesRight.Name = "flowLayoutPanel2PropertiesRight";
            this.flowLayoutPanel2PropertiesRight.Size = new System.Drawing.Size(132, 192);
            this.flowLayoutPanel2PropertiesRight.TabIndex = 13;
            // 
            // scalingLabelRelic2
            // 
            this.scalingLabelRelic2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.scalingLabelRelic2.Font = new System.Drawing.Font("Albertus MT", 9.75F, System.Drawing.FontStyle.Bold);
            this.scalingLabelRelic2.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelRelic2.Location = new System.Drawing.Point(3, 174);
            this.scalingLabelRelic2.Margin = new System.Windows.Forms.Padding(3, 0, 0, 3);
            this.scalingLabelRelic2.Name = "scalingLabelRelic2";
            this.scalingLabelRelic2.Size = new System.Drawing.Size(128, 15);
            this.scalingLabelRelic2.TabIndex = 0;
            this.scalingLabelRelic2.Text = "Second Relic";
            this.scalingLabelRelic2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // flowLayoutPanelTop
            // 
            this.flowLayoutPanelTop.AutoSize = true;
            this.tableLayoutPanelForge.SetColumnSpan(this.flowLayoutPanelTop, 2);
            this.flowLayoutPanelTop.Controls.Add(this.scalingLabelBaseItem);
            this.flowLayoutPanelTop.Controls.Add(this.scalingRadioButtonGod);
            this.flowLayoutPanelTop.Controls.Add(this.scalingRadioButtonRelax);
            this.flowLayoutPanelTop.Controls.Add(this.scalingRadioButtonStrict);
            this.flowLayoutPanelTop.Controls.Add(this.scalingRadioButtonGame);
            this.flowLayoutPanelTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelTop.FlowDirection = System.Windows.Forms.FlowDirection.BottomUp;
            this.flowLayoutPanelTop.Location = new System.Drawing.Point(152, 38);
            this.flowLayoutPanelTop.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanelTop.Name = "flowLayoutPanelTop";
            this.flowLayoutPanelTop.Size = new System.Drawing.Size(128, 192);
            this.flowLayoutPanelTop.TabIndex = 14;
            // 
            // scalingLabelBaseItem
            // 
            this.scalingLabelBaseItem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.scalingLabelBaseItem.Font = new System.Drawing.Font("Albertus MT", 9.75F, System.Drawing.FontStyle.Bold);
            this.scalingLabelBaseItem.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelBaseItem.Location = new System.Drawing.Point(0, 177);
            this.scalingLabelBaseItem.Margin = new System.Windows.Forms.Padding(0);
            this.scalingLabelBaseItem.Name = "scalingLabelBaseItem";
            this.scalingLabelBaseItem.Size = new System.Drawing.Size(128, 15);
            this.scalingLabelBaseItem.TabIndex = 1;
            this.scalingLabelBaseItem.Text = "Base Item";
            this.scalingLabelBaseItem.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // scalingRadioButtonGod
            // 
            this.scalingRadioButtonGod.AutoSize = true;
            this.scalingRadioButtonGod.Font = new System.Drawing.Font("Albertus MT Light", 9.75F);
            this.scalingRadioButtonGod.ForeColor = System.Drawing.Color.Gold;
            this.scalingRadioButtonGod.Location = new System.Drawing.Point(20, 128);
            this.scalingRadioButtonGod.Margin = new System.Windows.Forms.Padding(20, 3, 3, 30);
            this.scalingRadioButtonGod.Name = "scalingRadioButtonGod";
            this.scalingRadioButtonGod.Size = new System.Drawing.Size(85, 19);
            this.scalingRadioButtonGod.TabIndex = 5;
            this.scalingRadioButtonGod.Text = "God Mode";
            this.scalingRadioButtonGod.UseVisualStyleBackColor = true;
            this.scalingRadioButtonGod.Click += new System.EventHandler(this.ForgeMode_Clicked);
            // 
            // scalingRadioButtonRelax
            // 
            this.scalingRadioButtonRelax.AutoSize = true;
            this.scalingRadioButtonRelax.Font = new System.Drawing.Font("Albertus MT Light", 9.75F);
            this.scalingRadioButtonRelax.ForeColor = System.Drawing.Color.Gold;
            this.scalingRadioButtonRelax.Location = new System.Drawing.Point(20, 103);
            this.scalingRadioButtonRelax.Margin = new System.Windows.Forms.Padding(20, 3, 3, 3);
            this.scalingRadioButtonRelax.Name = "scalingRadioButtonRelax";
            this.scalingRadioButtonRelax.Size = new System.Drawing.Size(89, 19);
            this.scalingRadioButtonRelax.TabIndex = 4;
            this.scalingRadioButtonRelax.Text = "Relax Mode";
            this.scalingRadioButtonRelax.UseVisualStyleBackColor = true;
            this.scalingRadioButtonRelax.Click += new System.EventHandler(this.ForgeMode_Clicked);
            // 
            // scalingRadioButtonStrict
            // 
            this.scalingRadioButtonStrict.AutoSize = true;
            this.scalingRadioButtonStrict.Font = new System.Drawing.Font("Albertus MT Light", 9.75F);
            this.scalingRadioButtonStrict.ForeColor = System.Drawing.Color.Gold;
            this.scalingRadioButtonStrict.Location = new System.Drawing.Point(20, 78);
            this.scalingRadioButtonStrict.Margin = new System.Windows.Forms.Padding(20, 3, 3, 3);
            this.scalingRadioButtonStrict.Name = "scalingRadioButtonStrict";
            this.scalingRadioButtonStrict.Size = new System.Drawing.Size(89, 19);
            this.scalingRadioButtonStrict.TabIndex = 3;
            this.scalingRadioButtonStrict.Text = "Strict Mode";
            this.scalingRadioButtonStrict.UseVisualStyleBackColor = true;
            this.scalingRadioButtonStrict.Click += new System.EventHandler(this.ForgeMode_Clicked);
            // 
            // scalingRadioButtonGame
            // 
            this.scalingRadioButtonGame.AutoSize = true;
            this.scalingRadioButtonGame.Checked = true;
            this.scalingRadioButtonGame.Font = new System.Drawing.Font("Albertus MT Light", 9.75F);
            this.scalingRadioButtonGame.ForeColor = System.Drawing.Color.Gold;
            this.scalingRadioButtonGame.Location = new System.Drawing.Point(20, 53);
            this.scalingRadioButtonGame.Margin = new System.Windows.Forms.Padding(20, 3, 3, 3);
            this.scalingRadioButtonGame.Name = "scalingRadioButtonGame";
            this.scalingRadioButtonGame.Size = new System.Drawing.Size(92, 19);
            this.scalingRadioButtonGame.TabIndex = 6;
            this.scalingRadioButtonGame.TabStop = true;
            this.scalingRadioButtonGame.Text = "Game Mode";
            this.scalingRadioButtonGame.UseVisualStyleBackColor = true;
            this.scalingRadioButtonGame.Click += new System.EventHandler(this.ForgeMode_Clicked);
            // 
            // comboBoxSuffix
            // 
            this.comboBoxSuffix.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxSuffix.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSuffix.Font = new System.Drawing.Font("Albertus MT Light", 9.75F);
            this.comboBoxSuffix.FormattingEnabled = true;
            this.comboBoxSuffix.Items.AddRange(new object[] {
            "Use Base",
            "Use Prefix",
            "Use Suffix",
            "Use Relic1",
            "Use Relic2"});
            this.comboBoxSuffix.Location = new System.Drawing.Point(280, 230);
            this.comboBoxSuffix.Margin = new System.Windows.Forms.Padding(0);
            this.comboBoxSuffix.Name = "comboBoxSuffix";
            this.comboBoxSuffix.Size = new System.Drawing.Size(132, 23);
            this.comboBoxSuffix.TabIndex = 19;
            this.comboBoxSuffix.SelectedIndexChanged += new System.EventHandler(this.comboBox_SelectedIndexChanged);
            // 
            // comboBoxPrefix
            // 
            this.comboBoxPrefix.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxPrefix.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPrefix.Font = new System.Drawing.Font("Albertus MT Light", 9.75F);
            this.comboBoxPrefix.FormattingEnabled = true;
            this.comboBoxPrefix.Items.AddRange(new object[] {
            "Use Base",
            "Use Prefix",
            "Use Suffix",
            "Use Relic1",
            "Use Relic2"});
            this.comboBoxPrefix.Location = new System.Drawing.Point(20, 230);
            this.comboBoxPrefix.Margin = new System.Windows.Forms.Padding(0);
            this.comboBoxPrefix.Name = "comboBoxPrefix";
            this.comboBoxPrefix.Size = new System.Drawing.Size(132, 23);
            this.comboBoxPrefix.TabIndex = 20;
            this.comboBoxPrefix.SelectedIndexChanged += new System.EventHandler(this.comboBox_SelectedIndexChanged);
            // 
            // comboBoxRelic1
            // 
            this.comboBoxRelic1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxRelic1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxRelic1.Font = new System.Drawing.Font("Albertus MT Light", 9.75F);
            this.comboBoxRelic1.FormattingEnabled = true;
            this.comboBoxRelic1.Items.AddRange(new object[] {
            "Use Base",
            "Use Prefix",
            "Use Suffix",
            "Use Relic1",
            "Use Relic2"});
            this.comboBoxRelic1.Location = new System.Drawing.Point(20, 637);
            this.comboBoxRelic1.Margin = new System.Windows.Forms.Padding(0);
            this.comboBoxRelic1.Name = "comboBoxRelic1";
            this.comboBoxRelic1.Size = new System.Drawing.Size(132, 23);
            this.comboBoxRelic1.TabIndex = 21;
            this.comboBoxRelic1.SelectedIndexChanged += new System.EventHandler(this.comboBox_SelectedIndexChanged);
            // 
            // comboBoxRelic2
            // 
            this.comboBoxRelic2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxRelic2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxRelic2.Font = new System.Drawing.Font("Albertus MT Light", 9.75F);
            this.comboBoxRelic2.FormattingEnabled = true;
            this.comboBoxRelic2.Items.AddRange(new object[] {
            "Use Base",
            "Use Prefix",
            "Use Suffix",
            "Use Relic1",
            "Use Relic2"});
            this.comboBoxRelic2.Location = new System.Drawing.Point(280, 637);
            this.comboBoxRelic2.Margin = new System.Windows.Forms.Padding(0);
            this.comboBoxRelic2.Name = "comboBoxRelic2";
            this.comboBoxRelic2.Size = new System.Drawing.Size(132, 23);
            this.comboBoxRelic2.TabIndex = 22;
            this.comboBoxRelic2.SelectedIndexChanged += new System.EventHandler(this.comboBox_SelectedIndexChanged);
            // 
            // ResetButton
            // 
            this.ResetButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.ResetButton.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanelForge.SetColumnSpan(this.ResetButton, 2);
            this.ResetButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ResetButton.DownBitmap")));
            this.ResetButton.FlatAppearance.BorderSize = 0;
            this.ResetButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ResetButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ResetButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ResetButton.Font = new System.Drawing.Font("Albertus MT", 12F);
            this.ResetButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ResetButton.Image = ((System.Drawing.Image)(resources.GetObject("ResetButton.Image")));
            this.ResetButton.Location = new System.Drawing.Point(166, 670);
            this.ResetButton.Margin = new System.Windows.Forms.Padding(0, 10, 0, 10);
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ResetButton.OverBitmap")));
            this.ResetButton.Size = new System.Drawing.Size(100, 30);
            this.ResetButton.SizeToGraphic = false;
            this.ResetButton.TabIndex = 23;
            this.ResetButton.Text = "Reset";
            this.ResetButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ResetButton.UpBitmap")));
            this.ResetButton.UseCustomGraphic = true;
            this.ResetButton.UseVisualStyleBackColor = false;
            this.ResetButton.Click += new System.EventHandler(this.scalingButtonReset_Click);
            // 
            // flowLayoutPanelBottom
            // 
            this.flowLayoutPanelBottom.AutoSize = true;
            this.tableLayoutPanelForge.SetColumnSpan(this.flowLayoutPanelBottom, 2);
            this.flowLayoutPanelBottom.Controls.Add(this.scalingCheckBoxHardcore);
            this.flowLayoutPanelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelBottom.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelBottom.Location = new System.Drawing.Point(155, 448);
            this.flowLayoutPanelBottom.Name = "flowLayoutPanelBottom";
            this.flowLayoutPanelBottom.Size = new System.Drawing.Size(122, 186);
            this.flowLayoutPanelBottom.TabIndex = 24;
            // 
            // scalingCheckBoxHardcore
            // 
            this.scalingCheckBoxHardcore.AutoSize = true;
            this.scalingCheckBoxHardcore.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.scalingCheckBoxHardcore.Font = new System.Drawing.Font("Albertus MT Light", 9.75F);
            this.scalingCheckBoxHardcore.ForeColor = System.Drawing.Color.Gold;
            this.scalingCheckBoxHardcore.Location = new System.Drawing.Point(3, 3);
            this.scalingCheckBoxHardcore.Name = "scalingCheckBoxHardcore";
            this.scalingCheckBoxHardcore.Size = new System.Drawing.Size(76, 19);
            this.scalingCheckBoxHardcore.TabIndex = 24;
            this.scalingCheckBoxHardcore.Text = "Hardcore";
            this.toolTip.SetToolTip(this.scalingCheckBoxHardcore, "Materials are destroyed in the process");
            this.scalingCheckBoxHardcore.UseVisualStyleBackColor = true;
            this.scalingCheckBoxHardcore.CheckedChanged += new System.EventHandler(this.scalingCheckBoxHardcore_CheckedChanged);
            // 
            // ForgePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.pictureBoxDragDrop);
            this.Controls.Add(this.tableLayoutPanelForge);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.Name = "ForgePanel";
            this.Padding = new System.Windows.Forms.Padding(15);
            this.Size = new System.Drawing.Size(559, 760);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxDragDrop)).EndInit();
            this.tableLayoutPanelForge.ResumeLayout(false);
            this.tableLayoutPanelForge.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxRelic2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSuffix)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxRelic1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPrefix)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBaseItem)).EndInit();
            this.flowLayoutPanelPropertiesLeft.ResumeLayout(false);
            this.flowLayoutPanel2PropertiesRight.ResumeLayout(false);
            this.flowLayoutPanelTop.ResumeLayout(false);
            this.flowLayoutPanelTop.PerformLayout();
            this.flowLayoutPanelBottom.ResumeLayout(false);
            this.flowLayoutPanelBottom.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private BufferedTableLayoutPanel tableLayoutPanelForge;
		private ScalingLabel scalingLabelPrefix;
		private ScalingLabel scalingLabelRelic2;
		private ScalingLabel scalingLabelRelic1;
		private ScalingLabel scalingLabelSuffix;
		private ScalingLabel scalingLabelBaseItem;
		private ScalingButton CancelButton;
		private ScalingButton ForgeButton;
		private System.Windows.Forms.PictureBox pictureBoxRelic2;
		private System.Windows.Forms.PictureBox pictureBoxSuffix;
		private System.Windows.Forms.PictureBox pictureBoxRelic1;
		private System.Windows.Forms.PictureBox pictureBoxPrefix;
		private System.Windows.Forms.PictureBox pictureBoxBaseItem;
		private BufferedFlowLayoutPanel flowLayoutPanelPropertiesLeft;
		private BufferedFlowLayoutPanel flowLayoutPanel2PropertiesRight;
		private BufferedFlowLayoutPanel flowLayoutPanelTop;
		private ScalingRadioButton scalingRadioButtonGod;
		private ScalingRadioButton scalingRadioButtonRelax;
		private ScalingRadioButton scalingRadioButtonStrict;
		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.ComboBox comboBoxSuffix;
		private System.Windows.Forms.ComboBox comboBoxPrefix;
		private System.Windows.Forms.ComboBox comboBoxRelic1;
		private System.Windows.Forms.ComboBox comboBoxRelic2;
		private System.Windows.Forms.PictureBox pictureBoxDragDrop;
		private ScalingButton ResetButton;
        private ScalingRadioButton scalingRadioButtonGame;
        private BufferedFlowLayoutPanel flowLayoutPanelBottom;
        private ScalingCheckBox scalingCheckBoxHardcore;
    }
}
