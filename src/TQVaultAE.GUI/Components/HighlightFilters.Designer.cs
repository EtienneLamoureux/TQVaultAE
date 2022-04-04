namespace TQVaultAE.GUI.Components
{
	partial class HighlightFilters
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
            this.tableLayoutPanelHighlight = new System.Windows.Forms.TableLayoutPanel();
            this.buttonApply = new System.Windows.Forms.Button();
            this.flowLayoutPanelMax = new System.Windows.Forms.FlowLayoutPanel();
            this.numericUpDownMaxLvl = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownMaxStr = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownMaxDex = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownMaxInt = new System.Windows.Forms.NumericUpDown();
            this.flowLayoutPanelMin = new System.Windows.Forms.FlowLayoutPanel();
            this.numericUpDownMinLvl = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownMinStr = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownMinDex = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownMinInt = new System.Windows.Forms.NumericUpDown();
            this.buttonReset = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.scalingLabelMaxLvl = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingLabelMaxStr = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingLabelMaxDex = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingLabelMaxInt = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingCheckBoxMax = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.scalingCheckBoxMin = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.scalingLabelMinLvl = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingLabelMinStr = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingLabelMinDex = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingLabelMinInt = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingCheckedListBoxTypes = new TQVaultAE.GUI.Components.ScalingCheckedListBox();
            this.tableLayoutPanelHighlight.SuspendLayout();
            this.flowLayoutPanelMax.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxLvl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxStr)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxDex)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxInt)).BeginInit();
            this.flowLayoutPanelMin.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinLvl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinStr)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinDex)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinInt)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanelHighlight
            // 
            this.tableLayoutPanelHighlight.AutoSize = true;
            this.tableLayoutPanelHighlight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanelHighlight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.tableLayoutPanelHighlight.ColumnCount = 2;
            this.tableLayoutPanelHighlight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelHighlight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelHighlight.Controls.Add(this.buttonApply, 1, 3);
            this.tableLayoutPanelHighlight.Controls.Add(this.flowLayoutPanelMax, 1, 1);
            this.tableLayoutPanelHighlight.Controls.Add(this.scalingCheckBoxMax, 0, 1);
            this.tableLayoutPanelHighlight.Controls.Add(this.scalingCheckBoxMin, 0, 0);
            this.tableLayoutPanelHighlight.Controls.Add(this.flowLayoutPanelMin, 1, 0);
            this.tableLayoutPanelHighlight.Controls.Add(this.scalingCheckedListBoxTypes, 0, 2);
            this.tableLayoutPanelHighlight.Controls.Add(this.buttonReset, 0, 3);
            this.tableLayoutPanelHighlight.Location = new System.Drawing.Point(1, 1);
            this.tableLayoutPanelHighlight.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanelHighlight.Name = "tableLayoutPanelHighlight";
            this.tableLayoutPanelHighlight.RowCount = 4;
            this.tableLayoutPanelHighlight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelHighlight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelHighlight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelHighlight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelHighlight.Size = new System.Drawing.Size(480, 239);
            this.tableLayoutPanelHighlight.TabIndex = 25;
            // 
            // buttonApply
            // 
            this.buttonApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonApply.AutoSize = true;
            this.buttonApply.Font = new System.Drawing.Font("Albertus MT Light", 9.75F);
            this.buttonApply.ForeColor = System.Drawing.Color.Black;
            this.buttonApply.Location = new System.Drawing.Point(402, 211);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(75, 25);
            this.buttonApply.TabIndex = 29;
            this.buttonApply.Text = "Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // flowLayoutPanelMax
            // 
            this.flowLayoutPanelMax.AutoSize = true;
            this.flowLayoutPanelMax.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelMax.Controls.Add(this.scalingLabelMaxLvl);
            this.flowLayoutPanelMax.Controls.Add(this.numericUpDownMaxLvl);
            this.flowLayoutPanelMax.Controls.Add(this.scalingLabelMaxStr);
            this.flowLayoutPanelMax.Controls.Add(this.numericUpDownMaxStr);
            this.flowLayoutPanelMax.Controls.Add(this.scalingLabelMaxDex);
            this.flowLayoutPanelMax.Controls.Add(this.numericUpDownMaxDex);
            this.flowLayoutPanelMax.Controls.Add(this.scalingLabelMaxInt);
            this.flowLayoutPanelMax.Controls.Add(this.numericUpDownMaxInt);
            this.flowLayoutPanelMax.Location = new System.Drawing.Point(144, 29);
            this.flowLayoutPanelMax.Name = "flowLayoutPanelMax";
            this.flowLayoutPanelMax.Size = new System.Drawing.Size(333, 20);
            this.flowLayoutPanelMax.TabIndex = 25;
            this.flowLayoutPanelMax.WrapContents = false;
            // 
            // numericUpDownMaxLvl
            // 
            this.numericUpDownMaxLvl.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownMaxLvl.Location = new System.Drawing.Point(47, 0);
            this.numericUpDownMaxLvl.Margin = new System.Windows.Forms.Padding(0);
            this.numericUpDownMaxLvl.Name = "numericUpDownMaxLvl";
            this.numericUpDownMaxLvl.Size = new System.Drawing.Size(44, 20);
            this.numericUpDownMaxLvl.TabIndex = 1;
            this.numericUpDownMaxLvl.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownMaxLvl.ValueChanged += new System.EventHandler(this.numericUpDownMaxLvl_ValueChanged);
            // 
            // numericUpDownMaxStr
            // 
            this.numericUpDownMaxStr.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownMaxStr.Location = new System.Drawing.Point(126, 0);
            this.numericUpDownMaxStr.Margin = new System.Windows.Forms.Padding(0);
            this.numericUpDownMaxStr.Maximum = new decimal(new int[] {
            9000,
            0,
            0,
            0});
            this.numericUpDownMaxStr.Name = "numericUpDownMaxStr";
            this.numericUpDownMaxStr.Size = new System.Drawing.Size(44, 20);
            this.numericUpDownMaxStr.TabIndex = 3;
            this.numericUpDownMaxStr.Value = new decimal(new int[] {
            9000,
            0,
            0,
            0});
            this.numericUpDownMaxStr.ValueChanged += new System.EventHandler(this.numericUpDownMaxLvl_ValueChanged);
            // 
            // numericUpDownMaxDex
            // 
            this.numericUpDownMaxDex.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownMaxDex.Location = new System.Drawing.Point(210, 0);
            this.numericUpDownMaxDex.Margin = new System.Windows.Forms.Padding(0);
            this.numericUpDownMaxDex.Maximum = new decimal(new int[] {
            9000,
            0,
            0,
            0});
            this.numericUpDownMaxDex.Name = "numericUpDownMaxDex";
            this.numericUpDownMaxDex.Size = new System.Drawing.Size(44, 20);
            this.numericUpDownMaxDex.TabIndex = 5;
            this.numericUpDownMaxDex.Value = new decimal(new int[] {
            9000,
            0,
            0,
            0});
            this.numericUpDownMaxDex.ValueChanged += new System.EventHandler(this.numericUpDownMaxLvl_ValueChanged);
            // 
            // numericUpDownMaxInt
            // 
            this.numericUpDownMaxInt.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownMaxInt.Location = new System.Drawing.Point(289, 0);
            this.numericUpDownMaxInt.Margin = new System.Windows.Forms.Padding(0);
            this.numericUpDownMaxInt.Maximum = new decimal(new int[] {
            9000,
            0,
            0,
            0});
            this.numericUpDownMaxInt.Name = "numericUpDownMaxInt";
            this.numericUpDownMaxInt.Size = new System.Drawing.Size(44, 20);
            this.numericUpDownMaxInt.TabIndex = 7;
            this.numericUpDownMaxInt.Value = new decimal(new int[] {
            9000,
            0,
            0,
            0});
            this.numericUpDownMaxInt.ValueChanged += new System.EventHandler(this.numericUpDownMaxLvl_ValueChanged);
            // 
            // flowLayoutPanelMin
            // 
            this.flowLayoutPanelMin.AutoSize = true;
            this.flowLayoutPanelMin.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelMin.Controls.Add(this.scalingLabelMinLvl);
            this.flowLayoutPanelMin.Controls.Add(this.numericUpDownMinLvl);
            this.flowLayoutPanelMin.Controls.Add(this.scalingLabelMinStr);
            this.flowLayoutPanelMin.Controls.Add(this.numericUpDownMinStr);
            this.flowLayoutPanelMin.Controls.Add(this.scalingLabelMinDex);
            this.flowLayoutPanelMin.Controls.Add(this.numericUpDownMinDex);
            this.flowLayoutPanelMin.Controls.Add(this.scalingLabelMinInt);
            this.flowLayoutPanelMin.Controls.Add(this.numericUpDownMinInt);
            this.flowLayoutPanelMin.Location = new System.Drawing.Point(144, 3);
            this.flowLayoutPanelMin.Name = "flowLayoutPanelMin";
            this.flowLayoutPanelMin.Size = new System.Drawing.Size(333, 20);
            this.flowLayoutPanelMin.TabIndex = 4;
            this.flowLayoutPanelMin.WrapContents = false;
            // 
            // numericUpDownMinLvl
            // 
            this.numericUpDownMinLvl.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownMinLvl.Location = new System.Drawing.Point(47, 0);
            this.numericUpDownMinLvl.Margin = new System.Windows.Forms.Padding(0);
            this.numericUpDownMinLvl.Name = "numericUpDownMinLvl";
            this.numericUpDownMinLvl.Size = new System.Drawing.Size(44, 20);
            this.numericUpDownMinLvl.TabIndex = 1;
            this.numericUpDownMinLvl.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownMinLvl.ValueChanged += new System.EventHandler(this.numericUpDownMinLvl_ValueChanged);
            // 
            // numericUpDownMinStr
            // 
            this.numericUpDownMinStr.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownMinStr.Location = new System.Drawing.Point(126, 0);
            this.numericUpDownMinStr.Margin = new System.Windows.Forms.Padding(0);
            this.numericUpDownMinStr.Maximum = new decimal(new int[] {
            9000,
            0,
            0,
            0});
            this.numericUpDownMinStr.Name = "numericUpDownMinStr";
            this.numericUpDownMinStr.Size = new System.Drawing.Size(44, 20);
            this.numericUpDownMinStr.TabIndex = 3;
            this.numericUpDownMinStr.Value = new decimal(new int[] {
            9000,
            0,
            0,
            0});
            this.numericUpDownMinStr.ValueChanged += new System.EventHandler(this.numericUpDownMinLvl_ValueChanged);
            // 
            // numericUpDownMinDex
            // 
            this.numericUpDownMinDex.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownMinDex.Location = new System.Drawing.Point(210, 0);
            this.numericUpDownMinDex.Margin = new System.Windows.Forms.Padding(0);
            this.numericUpDownMinDex.Maximum = new decimal(new int[] {
            9000,
            0,
            0,
            0});
            this.numericUpDownMinDex.Name = "numericUpDownMinDex";
            this.numericUpDownMinDex.Size = new System.Drawing.Size(44, 20);
            this.numericUpDownMinDex.TabIndex = 5;
            this.numericUpDownMinDex.Value = new decimal(new int[] {
            9000,
            0,
            0,
            0});
            this.numericUpDownMinDex.ValueChanged += new System.EventHandler(this.numericUpDownMinLvl_ValueChanged);
            // 
            // numericUpDownMinInt
            // 
            this.numericUpDownMinInt.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownMinInt.Location = new System.Drawing.Point(289, 0);
            this.numericUpDownMinInt.Margin = new System.Windows.Forms.Padding(0);
            this.numericUpDownMinInt.Maximum = new decimal(new int[] {
            9000,
            0,
            0,
            0});
            this.numericUpDownMinInt.Name = "numericUpDownMinInt";
            this.numericUpDownMinInt.Size = new System.Drawing.Size(44, 20);
            this.numericUpDownMinInt.TabIndex = 7;
            this.numericUpDownMinInt.Value = new decimal(new int[] {
            9000,
            0,
            0,
            0});
            this.numericUpDownMinInt.ValueChanged += new System.EventHandler(this.numericUpDownMinLvl_ValueChanged);
            // 
            // buttonReset
            // 
            this.buttonReset.AutoSize = true;
            this.buttonReset.Font = new System.Drawing.Font("Albertus MT Light", 9.75F);
            this.buttonReset.ForeColor = System.Drawing.Color.Black;
            this.buttonReset.Location = new System.Drawing.Point(3, 211);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(75, 25);
            this.buttonReset.TabIndex = 28;
            this.buttonReset.Text = "Reset";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // scalingLabelMaxLvl
            // 
            this.scalingLabelMaxLvl.AutoSize = true;
            this.scalingLabelMaxLvl.Font = new System.Drawing.Font("Albertus MT Light", 9.75F);
            this.scalingLabelMaxLvl.ForeColor = System.Drawing.Color.White;
            this.scalingLabelMaxLvl.Location = new System.Drawing.Point(3, 0);
            this.scalingLabelMaxLvl.Name = "scalingLabelMaxLvl";
            this.scalingLabelMaxLvl.Size = new System.Drawing.Size(41, 15);
            this.scalingLabelMaxLvl.TabIndex = 0;
            this.scalingLabelMaxLvl.Text = "Level :";
            // 
            // scalingLabelMaxStr
            // 
            this.scalingLabelMaxStr.AutoSize = true;
            this.scalingLabelMaxStr.Font = new System.Drawing.Font("Albertus MT Light", 9.75F);
            this.scalingLabelMaxStr.ForeColor = System.Drawing.Color.White;
            this.scalingLabelMaxStr.Location = new System.Drawing.Point(94, 0);
            this.scalingLabelMaxStr.Name = "scalingLabelMaxStr";
            this.scalingLabelMaxStr.Size = new System.Drawing.Size(29, 15);
            this.scalingLabelMaxStr.TabIndex = 2;
            this.scalingLabelMaxStr.Text = "Str :";
            // 
            // scalingLabelMaxDex
            // 
            this.scalingLabelMaxDex.AutoSize = true;
            this.scalingLabelMaxDex.Font = new System.Drawing.Font("Albertus MT Light", 9.75F);
            this.scalingLabelMaxDex.ForeColor = System.Drawing.Color.White;
            this.scalingLabelMaxDex.Location = new System.Drawing.Point(173, 0);
            this.scalingLabelMaxDex.Name = "scalingLabelMaxDex";
            this.scalingLabelMaxDex.Size = new System.Drawing.Size(34, 15);
            this.scalingLabelMaxDex.TabIndex = 4;
            this.scalingLabelMaxDex.Text = "Dex :";
            // 
            // scalingLabelMaxInt
            // 
            this.scalingLabelMaxInt.AutoSize = true;
            this.scalingLabelMaxInt.Font = new System.Drawing.Font("Albertus MT Light", 9.75F);
            this.scalingLabelMaxInt.ForeColor = System.Drawing.Color.White;
            this.scalingLabelMaxInt.Location = new System.Drawing.Point(257, 0);
            this.scalingLabelMaxInt.Name = "scalingLabelMaxInt";
            this.scalingLabelMaxInt.Size = new System.Drawing.Size(29, 15);
            this.scalingLabelMaxInt.TabIndex = 6;
            this.scalingLabelMaxInt.Text = "Int :";
            // 
            // scalingCheckBoxMax
            // 
            this.scalingCheckBoxMax.AutoSize = true;
            this.scalingCheckBoxMax.Font = new System.Drawing.Font("Albertus MT Light", 9.75F);
            this.scalingCheckBoxMax.ForeColor = System.Drawing.Color.White;
            this.scalingCheckBoxMax.Location = new System.Drawing.Point(3, 29);
            this.scalingCheckBoxMax.Name = "scalingCheckBoxMax";
            this.scalingCheckBoxMax.Size = new System.Drawing.Size(135, 19);
            this.scalingCheckBoxMax.TabIndex = 5;
            this.scalingCheckBoxMax.Text = "Max Requierement :";
            this.scalingCheckBoxMax.UseVisualStyleBackColor = true;
            // 
            // scalingCheckBoxMin
            // 
            this.scalingCheckBoxMin.AutoSize = true;
            this.scalingCheckBoxMin.Font = new System.Drawing.Font("Albertus MT Light", 9.75F);
            this.scalingCheckBoxMin.ForeColor = System.Drawing.Color.White;
            this.scalingCheckBoxMin.Location = new System.Drawing.Point(3, 3);
            this.scalingCheckBoxMin.Name = "scalingCheckBoxMin";
            this.scalingCheckBoxMin.Size = new System.Drawing.Size(134, 19);
            this.scalingCheckBoxMin.TabIndex = 3;
            this.scalingCheckBoxMin.Text = "Min Requierement :";
            this.scalingCheckBoxMin.UseVisualStyleBackColor = true;
            // 
            // scalingLabelMinLvl
            // 
            this.scalingLabelMinLvl.AutoSize = true;
            this.scalingLabelMinLvl.Font = new System.Drawing.Font("Albertus MT Light", 9.75F);
            this.scalingLabelMinLvl.ForeColor = System.Drawing.Color.White;
            this.scalingLabelMinLvl.Location = new System.Drawing.Point(3, 0);
            this.scalingLabelMinLvl.Name = "scalingLabelMinLvl";
            this.scalingLabelMinLvl.Size = new System.Drawing.Size(41, 15);
            this.scalingLabelMinLvl.TabIndex = 0;
            this.scalingLabelMinLvl.Text = "Level :";
            // 
            // scalingLabelMinStr
            // 
            this.scalingLabelMinStr.AutoSize = true;
            this.scalingLabelMinStr.Font = new System.Drawing.Font("Albertus MT Light", 9.75F);
            this.scalingLabelMinStr.ForeColor = System.Drawing.Color.White;
            this.scalingLabelMinStr.Location = new System.Drawing.Point(94, 0);
            this.scalingLabelMinStr.Name = "scalingLabelMinStr";
            this.scalingLabelMinStr.Size = new System.Drawing.Size(29, 15);
            this.scalingLabelMinStr.TabIndex = 2;
            this.scalingLabelMinStr.Text = "Str :";
            // 
            // scalingLabelMinDex
            // 
            this.scalingLabelMinDex.AutoSize = true;
            this.scalingLabelMinDex.Font = new System.Drawing.Font("Albertus MT Light", 9.75F);
            this.scalingLabelMinDex.ForeColor = System.Drawing.Color.White;
            this.scalingLabelMinDex.Location = new System.Drawing.Point(173, 0);
            this.scalingLabelMinDex.Name = "scalingLabelMinDex";
            this.scalingLabelMinDex.Size = new System.Drawing.Size(34, 15);
            this.scalingLabelMinDex.TabIndex = 4;
            this.scalingLabelMinDex.Text = "Dex :";
            // 
            // scalingLabelMinInt
            // 
            this.scalingLabelMinInt.AutoSize = true;
            this.scalingLabelMinInt.Font = new System.Drawing.Font("Albertus MT Light", 9.75F);
            this.scalingLabelMinInt.ForeColor = System.Drawing.Color.White;
            this.scalingLabelMinInt.Location = new System.Drawing.Point(257, 0);
            this.scalingLabelMinInt.Name = "scalingLabelMinInt";
            this.scalingLabelMinInt.Size = new System.Drawing.Size(29, 15);
            this.scalingLabelMinInt.TabIndex = 6;
            this.scalingLabelMinInt.Text = "Int :";
            // 
            // scalingCheckedListBoxTypes
            // 
            this.scalingCheckedListBoxTypes.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.scalingCheckedListBoxTypes.CheckOnClick = true;
            this.tableLayoutPanelHighlight.SetColumnSpan(this.scalingCheckedListBoxTypes, 2);
            this.scalingCheckedListBoxTypes.ColumnWidth = 100;
            this.scalingCheckedListBoxTypes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scalingCheckedListBoxTypes.Font = new System.Drawing.Font("Albertus MT Light", 9.75F);
            this.scalingCheckedListBoxTypes.ForeColor = System.Drawing.Color.White;
            this.scalingCheckedListBoxTypes.Items.AddRange(new object[] {
            "Spear",
            "Bow",
            "Sword",
            "Mace",
            "Axe",
            "Shield",
            "Staff",
            "Thrown",
            "Torso",
            "Head",
            "Forearm",
            "Leg",
            "Ring",
            "Amulet",
            "Scroll",
            "Charm",
            "Relic",
            "Quest Item",
            "Formula",
            "Artifact",
            "Potion",
            "Equipment",
            "Dye"});
            this.scalingCheckedListBoxTypes.Location = new System.Drawing.Point(3, 55);
            this.scalingCheckedListBoxTypes.MultiColumn = true;
            this.scalingCheckedListBoxTypes.Name = "scalingCheckedListBoxTypes";
            this.scalingCheckedListBoxTypes.Size = new System.Drawing.Size(474, 150);
            this.scalingCheckedListBoxTypes.TabIndex = 27;
            // 
            // HighlightFilters
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.Gold;
            this.Controls.Add(this.tableLayoutPanelHighlight);
            this.Name = "HighlightFilters";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.Size = new System.Drawing.Size(482, 241);
            this.tableLayoutPanelHighlight.ResumeLayout(false);
            this.tableLayoutPanelHighlight.PerformLayout();
            this.flowLayoutPanelMax.ResumeLayout(false);
            this.flowLayoutPanelMax.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxLvl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxStr)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxDex)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxInt)).EndInit();
            this.flowLayoutPanelMin.ResumeLayout(false);
            this.flowLayoutPanelMin.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinLvl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinStr)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinDex)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinInt)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelHighlight;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelMax;
		private ScalingLabel scalingLabelMaxLvl;
		private System.Windows.Forms.NumericUpDown numericUpDownMaxLvl;
		private ScalingLabel scalingLabelMaxStr;
		private System.Windows.Forms.NumericUpDown numericUpDownMaxStr;
		private ScalingLabel scalingLabelMaxDex;
		private System.Windows.Forms.NumericUpDown numericUpDownMaxDex;
		private ScalingLabel scalingLabelMaxInt;
		private System.Windows.Forms.NumericUpDown numericUpDownMaxInt;
		private ScalingCheckBox scalingCheckBoxMax;
		private ScalingCheckBox scalingCheckBoxMin;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelMin;
		private ScalingLabel scalingLabelMinLvl;
		private System.Windows.Forms.NumericUpDown numericUpDownMinLvl;
		private ScalingLabel scalingLabelMinStr;
		private System.Windows.Forms.NumericUpDown numericUpDownMinStr;
		private ScalingLabel scalingLabelMinDex;
		private System.Windows.Forms.NumericUpDown numericUpDownMinDex;
		private ScalingLabel scalingLabelMinInt;
		private System.Windows.Forms.NumericUpDown numericUpDownMinInt;
		private ScalingCheckedListBox scalingCheckedListBoxTypes;
		private System.Windows.Forms.Button buttonApply;
		private System.Windows.Forms.Button buttonReset;
		private System.Windows.Forms.ToolTip toolTip;
	}
}
