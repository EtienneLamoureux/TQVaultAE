using TQVaultAE.GUI.Components;

namespace TQVaultAE.GUI
{
	/// <summary>
	/// Form designer class for ItemSeedDialog
	/// </summary>
	internal partial class CharacterEditDialog
	{

		/// <summary>
		/// OK button control
		/// </summary>
		private ScalingButton ok;

		/// <summary>
		/// Cancel button
		/// </summary>
		private ScalingButton cancel;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CharacterEditDialog));
            this.ok = new TQVaultAE.GUI.Components.ScalingButton();
            this.cancel = new TQVaultAE.GUI.Components.ScalingButton();
            this.strengthLabel = new TQVaultAE.GUI.Components.ScalingLabel();
            this.dexterityLabel = new TQVaultAE.GUI.Components.ScalingLabel();
            this.IntelligenceLabel = new TQVaultAE.GUI.Components.ScalingLabel();
            this.healthLabel = new TQVaultAE.GUI.Components.ScalingLabel();
            this.manaLabel = new TQVaultAE.GUI.Components.ScalingLabel();
            this.attribGroupBox = new System.Windows.Forms.GroupBox();
            this.manacUpDown = new System.Windows.Forms.NumericUpDown();
            this.healthUpDown = new System.Windows.Forms.NumericUpDown();
            this.intelligenceUpDown = new System.Windows.Forms.NumericUpDown();
            this.dexterityUpDown = new System.Windows.Forms.NumericUpDown();
            this.strengthUpDown = new System.Windows.Forms.NumericUpDown();
            this.levelingGroupBox = new System.Windows.Forms.GroupBox();
            this.levelingCheckBox = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.difficultyLabel = new TQVaultAE.GUI.Components.ScalingLabel();
            this.difficultlyComboBox = new System.Windows.Forms.ComboBox();
            this.skillPointsNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.skillPointsLabel1 = new TQVaultAE.GUI.Components.ScalingLabel();
            this.attributeNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.attributeLabel1 = new TQVaultAE.GUI.Components.ScalingLabel();
            this.xpTextBox = new System.Windows.Forms.TextBox();
            this.xpLabel1 = new TQVaultAE.GUI.Components.ScalingLabel();
            this.levelNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.levelLabel1 = new TQVaultAE.GUI.Components.ScalingLabel();
            this.ResetMasteriesScalingButton = new TQVaultAE.GUI.Components.ScalingButton();
            this.MasteriesGroupBox = new System.Windows.Forms.GroupBox();
            this.Mastery2ValueScalingLabel = new TQVaultAE.GUI.Components.ScalingLabel();
            this.Mastery1ValueScalingLabel = new TQVaultAE.GUI.Components.ScalingLabel();
            this.Mastery2NameScalingLabel = new TQVaultAE.GUI.Components.ScalingLabel();
            this.Mastery1NameScalingLabel = new TQVaultAE.GUI.Components.ScalingLabel();
            this.attribGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.manacUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.healthUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.intelligenceUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dexterityUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.strengthUpDown)).BeginInit();
            this.levelingGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.skillPointsNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.attributeNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.levelNumericUpDown)).BeginInit();
            this.MasteriesGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // ok
            // 
            this.ok.BackColor = System.Drawing.Color.Transparent;
            this.ok.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ok.DownBitmap")));
            this.ok.FlatAppearance.BorderSize = 0;
            this.ok.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ok.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ok.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ok.Font = new System.Drawing.Font("Albertus MT", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ok.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ok.Image = ((System.Drawing.Image)(resources.GetObject("ok.Image")));
            this.ok.Location = new System.Drawing.Point(540, 438);
            this.ok.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ok.Name = "ok";
            this.ok.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ok.OverBitmap")));
            this.ok.Size = new System.Drawing.Size(171, 38);
            this.ok.SizeToGraphic = false;
            this.ok.TabIndex = 13;
            this.ok.Text = "OK";
            this.ok.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ok.UpBitmap")));
            this.ok.UseCustomGraphic = true;
            this.ok.UseVisualStyleBackColor = false;
            this.ok.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // cancel
            // 
            this.cancel.BackColor = System.Drawing.Color.Transparent;
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("cancel.DownBitmap")));
            this.cancel.FlatAppearance.BorderSize = 0;
            this.cancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.cancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.cancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cancel.Font = new System.Drawing.Font("Albertus MT", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.cancel.Image = ((System.Drawing.Image)(resources.GetObject("cancel.Image")));
            this.cancel.Location = new System.Drawing.Point(722, 438);
            this.cancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cancel.Name = "cancel";
            this.cancel.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("cancel.OverBitmap")));
            this.cancel.Size = new System.Drawing.Size(171, 38);
            this.cancel.SizeToGraphic = false;
            this.cancel.TabIndex = 14;
            this.cancel.Text = "Cancel";
            this.cancel.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("cancel.UpBitmap")));
            this.cancel.UseCustomGraphic = true;
            this.cancel.UseVisualStyleBackColor = false;
            this.cancel.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // strengthLabel
            // 
            this.strengthLabel.Font = new System.Drawing.Font("Albertus MT", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.strengthLabel.Location = new System.Drawing.Point(8, 41);
            this.strengthLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.strengthLabel.Name = "strengthLabel";
            this.strengthLabel.Size = new System.Drawing.Size(128, 22);
            this.strengthLabel.TabIndex = 5;
            this.strengthLabel.Text = "Strength";
            this.strengthLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // dexterityLabel
            // 
            this.dexterityLabel.Font = new System.Drawing.Font("Albertus MT", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dexterityLabel.Location = new System.Drawing.Point(11, 79);
            this.dexterityLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.dexterityLabel.Name = "dexterityLabel";
            this.dexterityLabel.Size = new System.Drawing.Size(124, 22);
            this.dexterityLabel.TabIndex = 7;
            this.dexterityLabel.Text = "Dexterity";
            this.dexterityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // IntelligenceLabel
            // 
            this.IntelligenceLabel.Font = new System.Drawing.Font("Albertus MT", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IntelligenceLabel.Location = new System.Drawing.Point(15, 116);
            this.IntelligenceLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.IntelligenceLabel.Name = "IntelligenceLabel";
            this.IntelligenceLabel.Size = new System.Drawing.Size(120, 22);
            this.IntelligenceLabel.TabIndex = 9;
            this.IntelligenceLabel.Text = "Intelligence";
            this.IntelligenceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // healthLabel
            // 
            this.healthLabel.CausesValidation = false;
            this.healthLabel.Font = new System.Drawing.Font("Albertus MT", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.healthLabel.Location = new System.Drawing.Point(11, 154);
            this.healthLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.healthLabel.Name = "healthLabel";
            this.healthLabel.Size = new System.Drawing.Size(124, 22);
            this.healthLabel.TabIndex = 11;
            this.healthLabel.Text = "Health";
            this.healthLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // manaLabel
            // 
            this.manaLabel.CausesValidation = false;
            this.manaLabel.Font = new System.Drawing.Font("Albertus MT", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.manaLabel.Location = new System.Drawing.Point(15, 191);
            this.manaLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.manaLabel.Name = "manaLabel";
            this.manaLabel.Size = new System.Drawing.Size(120, 22);
            this.manaLabel.TabIndex = 13;
            this.manaLabel.Text = "Mana";
            this.manaLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // attribGroupBox
            // 
            this.attribGroupBox.BackColor = System.Drawing.Color.Transparent;
            this.attribGroupBox.Controls.Add(this.manacUpDown);
            this.attribGroupBox.Controls.Add(this.healthUpDown);
            this.attribGroupBox.Controls.Add(this.intelligenceUpDown);
            this.attribGroupBox.Controls.Add(this.dexterityUpDown);
            this.attribGroupBox.Controls.Add(this.strengthUpDown);
            this.attribGroupBox.Controls.Add(this.manaLabel);
            this.attribGroupBox.Controls.Add(this.healthLabel);
            this.attribGroupBox.Controls.Add(this.IntelligenceLabel);
            this.attribGroupBox.Controls.Add(this.dexterityLabel);
            this.attribGroupBox.Controls.Add(this.strengthLabel);
            this.attribGroupBox.Font = new System.Drawing.Font("Albertus MT", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.attribGroupBox.ForeColor = System.Drawing.Color.Gold;
            this.attribGroupBox.Location = new System.Drawing.Point(39, 35);
            this.attribGroupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.attribGroupBox.Name = "attribGroupBox";
            this.attribGroupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.attribGroupBox.Size = new System.Drawing.Size(339, 265);
            this.attribGroupBox.TabIndex = 14;
            this.attribGroupBox.TabStop = false;
            this.attribGroupBox.Text = "Base Attributes";
            // 
            // manacUpDown
            // 
            this.manacUpDown.Font = new System.Drawing.Font("Albertus MT", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.manacUpDown.Increment = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.manacUpDown.Location = new System.Drawing.Point(142, 189);
            this.manacUpDown.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.manacUpDown.Maximum = new decimal(new int[] {
            9996,
            0,
            0,
            0});
            this.manacUpDown.Minimum = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.manacUpDown.Name = "manacUpDown";
            this.manacUpDown.ReadOnly = true;
            this.manacUpDown.Size = new System.Drawing.Size(81, 28);
            this.manacUpDown.TabIndex = 5;
            this.manacUpDown.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.manacUpDown.ValueChanged += new System.EventHandler(this.StatsUpDown_ValueChanged);
            // 
            // healthUpDown
            // 
            this.healthUpDown.Font = new System.Drawing.Font("Albertus MT", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.healthUpDown.Increment = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.healthUpDown.Location = new System.Drawing.Point(142, 151);
            this.healthUpDown.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.healthUpDown.Maximum = new decimal(new int[] {
            9996,
            0,
            0,
            0});
            this.healthUpDown.Minimum = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.healthUpDown.Name = "healthUpDown";
            this.healthUpDown.ReadOnly = true;
            this.healthUpDown.Size = new System.Drawing.Size(81, 28);
            this.healthUpDown.TabIndex = 4;
            this.healthUpDown.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.healthUpDown.ValueChanged += new System.EventHandler(this.StatsUpDown_ValueChanged);
            // 
            // intelligenceUpDown
            // 
            this.intelligenceUpDown.Font = new System.Drawing.Font("Albertus MT", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.intelligenceUpDown.Increment = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.intelligenceUpDown.Location = new System.Drawing.Point(142, 114);
            this.intelligenceUpDown.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.intelligenceUpDown.Maximum = new decimal(new int[] {
            996,
            0,
            0,
            0});
            this.intelligenceUpDown.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.intelligenceUpDown.Name = "intelligenceUpDown";
            this.intelligenceUpDown.ReadOnly = true;
            this.intelligenceUpDown.Size = new System.Drawing.Size(81, 28);
            this.intelligenceUpDown.TabIndex = 3;
            this.intelligenceUpDown.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.intelligenceUpDown.ValueChanged += new System.EventHandler(this.StatsUpDown_ValueChanged);
            // 
            // dexterityUpDown
            // 
            this.dexterityUpDown.Font = new System.Drawing.Font("Albertus MT", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dexterityUpDown.Increment = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.dexterityUpDown.Location = new System.Drawing.Point(142, 76);
            this.dexterityUpDown.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.dexterityUpDown.Maximum = new decimal(new int[] {
            996,
            0,
            0,
            0});
            this.dexterityUpDown.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.dexterityUpDown.Name = "dexterityUpDown";
            this.dexterityUpDown.ReadOnly = true;
            this.dexterityUpDown.Size = new System.Drawing.Size(81, 28);
            this.dexterityUpDown.TabIndex = 2;
            this.dexterityUpDown.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.dexterityUpDown.ValueChanged += new System.EventHandler(this.StatsUpDown_ValueChanged);
            // 
            // strengthUpDown
            // 
            this.strengthUpDown.Font = new System.Drawing.Font("Albertus MT", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.strengthUpDown.Increment = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.strengthUpDown.Location = new System.Drawing.Point(142, 39);
            this.strengthUpDown.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.strengthUpDown.Maximum = new decimal(new int[] {
            996,
            0,
            0,
            0});
            this.strengthUpDown.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.strengthUpDown.Name = "strengthUpDown";
            this.strengthUpDown.ReadOnly = true;
            this.strengthUpDown.Size = new System.Drawing.Size(81, 28);
            this.strengthUpDown.TabIndex = 1;
            this.strengthUpDown.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.strengthUpDown.ValueChanged += new System.EventHandler(this.StatsUpDown_ValueChanged);
            // 
            // levelingGroupBox
            // 
            this.levelingGroupBox.BackColor = System.Drawing.Color.Transparent;
            this.levelingGroupBox.Controls.Add(this.levelingCheckBox);
            this.levelingGroupBox.Controls.Add(this.difficultyLabel);
            this.levelingGroupBox.Controls.Add(this.difficultlyComboBox);
            this.levelingGroupBox.Controls.Add(this.skillPointsNumericUpDown);
            this.levelingGroupBox.Controls.Add(this.skillPointsLabel1);
            this.levelingGroupBox.Controls.Add(this.attributeNumericUpDown);
            this.levelingGroupBox.Controls.Add(this.attributeLabel1);
            this.levelingGroupBox.Controls.Add(this.xpTextBox);
            this.levelingGroupBox.Controls.Add(this.xpLabel1);
            this.levelingGroupBox.Controls.Add(this.levelNumericUpDown);
            this.levelingGroupBox.Controls.Add(this.levelLabel1);
            this.levelingGroupBox.Font = new System.Drawing.Font("Albertus MT", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.levelingGroupBox.ForeColor = System.Drawing.Color.Gold;
            this.levelingGroupBox.Location = new System.Drawing.Point(411, 35);
            this.levelingGroupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.levelingGroupBox.Name = "levelingGroupBox";
            this.levelingGroupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.levelingGroupBox.Size = new System.Drawing.Size(482, 265);
            this.levelingGroupBox.TabIndex = 15;
            this.levelingGroupBox.TabStop = false;
            this.levelingGroupBox.Text = "Leveling";
            // 
            // levelingCheckBox
            // 
            this.levelingCheckBox.AutoSize = true;
            this.levelingCheckBox.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.levelingCheckBox.Font = new System.Drawing.Font("Albertus MT", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.levelingCheckBox.Location = new System.Drawing.Point(86, 224);
            this.levelingCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.levelingCheckBox.Name = "levelingCheckBox";
            this.levelingCheckBox.Size = new System.Drawing.Size(156, 26);
            this.levelingCheckBox.TabIndex = 12;
            this.levelingCheckBox.Text = "Enable Leveling";
            this.levelingCheckBox.UseVisualStyleBackColor = true;
            this.levelingCheckBox.CheckedChanged += new System.EventHandler(this.LevelingCheckBox_CheckedChanged);
            // 
            // difficultyLabel
            // 
            this.difficultyLabel.BackColor = System.Drawing.Color.Transparent;
            this.difficultyLabel.Font = new System.Drawing.Font("Albertus MT", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.difficultyLabel.Location = new System.Drawing.Point(19, 188);
            this.difficultyLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.difficultyLabel.Name = "difficultyLabel";
            this.difficultyLabel.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.difficultyLabel.Size = new System.Drawing.Size(204, 22);
            this.difficultyLabel.TabIndex = 15;
            this.difficultyLabel.Text = "Difficultly";
            this.difficultyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // difficultlyComboBox
            // 
            this.difficultlyComboBox.Enabled = false;
            this.difficultlyComboBox.Font = new System.Drawing.Font("Albertus MT", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.difficultlyComboBox.FormattingEnabled = true;
            this.difficultlyComboBox.Location = new System.Drawing.Point(230, 184);
            this.difficultlyComboBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.difficultlyComboBox.Name = "difficultlyComboBox";
            this.difficultlyComboBox.Size = new System.Drawing.Size(199, 29);
            this.difficultlyComboBox.TabIndex = 11;
            // 
            // skillPointsNumericUpDown
            // 
            this.skillPointsNumericUpDown.Enabled = false;
            this.skillPointsNumericUpDown.Font = new System.Drawing.Font("Albertus MT", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.skillPointsNumericUpDown.Location = new System.Drawing.Point(230, 146);
            this.skillPointsNumericUpDown.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.skillPointsNumericUpDown.Maximum = new decimal(new int[] {
            286,
            0,
            0,
            0});
            this.skillPointsNumericUpDown.Name = "skillPointsNumericUpDown";
            this.skillPointsNumericUpDown.ReadOnly = true;
            this.skillPointsNumericUpDown.Size = new System.Drawing.Size(200, 28);
            this.skillPointsNumericUpDown.TabIndex = 10;
            // 
            // skillPointsLabel1
            // 
            this.skillPointsLabel1.Font = new System.Drawing.Font("Albertus MT", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.skillPointsLabel1.Location = new System.Drawing.Point(19, 149);
            this.skillPointsLabel1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.skillPointsLabel1.Name = "skillPointsLabel1";
            this.skillPointsLabel1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.skillPointsLabel1.Size = new System.Drawing.Size(208, 22);
            this.skillPointsLabel1.TabIndex = 12;
            this.skillPointsLabel1.Text = "Skill Points";
            this.skillPointsLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // attributeNumericUpDown
            // 
            this.attributeNumericUpDown.Enabled = false;
            this.attributeNumericUpDown.Font = new System.Drawing.Font("Albertus MT", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.attributeNumericUpDown.Location = new System.Drawing.Point(230, 109);
            this.attributeNumericUpDown.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.attributeNumericUpDown.Maximum = new decimal(new int[] {
            186,
            0,
            0,
            0});
            this.attributeNumericUpDown.Name = "attributeNumericUpDown";
            this.attributeNumericUpDown.ReadOnly = true;
            this.attributeNumericUpDown.Size = new System.Drawing.Size(200, 28);
            this.attributeNumericUpDown.TabIndex = 9;
            // 
            // attributeLabel1
            // 
            this.attributeLabel1.Font = new System.Drawing.Font("Albertus MT", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.attributeLabel1.Location = new System.Drawing.Point(19, 111);
            this.attributeLabel1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.attributeLabel1.Name = "attributeLabel1";
            this.attributeLabel1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.attributeLabel1.Size = new System.Drawing.Size(208, 22);
            this.attributeLabel1.TabIndex = 10;
            this.attributeLabel1.Text = "Attribute Points";
            this.attributeLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // xpTextBox
            // 
            this.xpTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.xpTextBox.Font = new System.Drawing.Font("Albertus MT", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xpTextBox.Location = new System.Drawing.Point(230, 71);
            this.xpTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.xpTextBox.Name = "xpTextBox";
            this.xpTextBox.ReadOnly = true;
            this.xpTextBox.Size = new System.Drawing.Size(200, 28);
            this.xpTextBox.TabIndex = 8;
            this.xpTextBox.WordWrap = false;
            // 
            // xpLabel1
            // 
            this.xpLabel1.Font = new System.Drawing.Font("Albertus MT", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xpLabel1.Location = new System.Drawing.Point(19, 74);
            this.xpLabel1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.xpLabel1.Name = "xpLabel1";
            this.xpLabel1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.xpLabel1.Size = new System.Drawing.Size(208, 22);
            this.xpLabel1.TabIndex = 8;
            this.xpLabel1.Text = "XP";
            this.xpLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // levelNumericUpDown
            // 
            this.levelNumericUpDown.Enabled = false;
            this.levelNumericUpDown.Font = new System.Drawing.Font("Albertus MT", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.levelNumericUpDown.Location = new System.Drawing.Point(230, 34);
            this.levelNumericUpDown.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.levelNumericUpDown.Maximum = new decimal(new int[] {
            84,
            0,
            0,
            0});
            this.levelNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.levelNumericUpDown.Name = "levelNumericUpDown";
            this.levelNumericUpDown.ReadOnly = true;
            this.levelNumericUpDown.Size = new System.Drawing.Size(70, 28);
            this.levelNumericUpDown.TabIndex = 7;
            this.levelNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.levelNumericUpDown.ValueChanged += new System.EventHandler(this.LevelNumericUpDown_ValueChanged);
            // 
            // levelLabel1
            // 
            this.levelLabel1.BackColor = System.Drawing.Color.Transparent;
            this.levelLabel1.Font = new System.Drawing.Font("Albertus MT", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.levelLabel1.Location = new System.Drawing.Point(19, 36);
            this.levelLabel1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.levelLabel1.Name = "levelLabel1";
            this.levelLabel1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.levelLabel1.Size = new System.Drawing.Size(208, 22);
            this.levelLabel1.TabIndex = 6;
            this.levelLabel1.Text = "Level";
            this.levelLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ResetMasteriesScalingButton
            // 
            this.ResetMasteriesScalingButton.BackColor = System.Drawing.Color.Transparent;
            this.ResetMasteriesScalingButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ResetMasteriesScalingButton.DownBitmap")));
            this.ResetMasteriesScalingButton.FlatAppearance.BorderSize = 0;
            this.ResetMasteriesScalingButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ResetMasteriesScalingButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ResetMasteriesScalingButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ResetMasteriesScalingButton.Font = new System.Drawing.Font("Albertus MT", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ResetMasteriesScalingButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ResetMasteriesScalingButton.Image = ((System.Drawing.Image)(resources.GetObject("ResetMasteriesScalingButton.Image")));
            this.ResetMasteriesScalingButton.Location = new System.Drawing.Point(39, 438);
            this.ResetMasteriesScalingButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ResetMasteriesScalingButton.Name = "ResetMasteriesScalingButton";
            this.ResetMasteriesScalingButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ResetMasteriesScalingButton.OverBitmap")));
            this.ResetMasteriesScalingButton.Size = new System.Drawing.Size(171, 38);
            this.ResetMasteriesScalingButton.SizeToGraphic = false;
            this.ResetMasteriesScalingButton.TabIndex = 16;
            this.ResetMasteriesScalingButton.Text = "Reset Masteries";
            this.ResetMasteriesScalingButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ResetMasteriesScalingButton.UpBitmap")));
            this.ResetMasteriesScalingButton.UseCustomGraphic = true;
            this.ResetMasteriesScalingButton.UseVisualStyleBackColor = false;
            this.ResetMasteriesScalingButton.Click += new System.EventHandler(this.ResetMasteriesScalingButton_Click);
            // 
            // MasteriesGroupBox
            // 
            this.MasteriesGroupBox.BackColor = System.Drawing.Color.Transparent;
            this.MasteriesGroupBox.Controls.Add(this.Mastery2ValueScalingLabel);
            this.MasteriesGroupBox.Controls.Add(this.Mastery1ValueScalingLabel);
            this.MasteriesGroupBox.Controls.Add(this.Mastery2NameScalingLabel);
            this.MasteriesGroupBox.Controls.Add(this.Mastery1NameScalingLabel);
            this.MasteriesGroupBox.Font = new System.Drawing.Font("Albertus MT", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MasteriesGroupBox.ForeColor = System.Drawing.Color.Gold;
            this.MasteriesGroupBox.Location = new System.Drawing.Point(39, 308);
            this.MasteriesGroupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MasteriesGroupBox.Name = "MasteriesGroupBox";
            this.MasteriesGroupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MasteriesGroupBox.Size = new System.Drawing.Size(855, 110);
            this.MasteriesGroupBox.TabIndex = 17;
            this.MasteriesGroupBox.TabStop = false;
            this.MasteriesGroupBox.Text = "Masteries";
            // 
            // Mastery2ValueScalingLabel
            // 
            this.Mastery2ValueScalingLabel.Font = new System.Drawing.Font("Albertus MT", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Mastery2ValueScalingLabel.Location = new System.Drawing.Point(258, 69);
            this.Mastery2ValueScalingLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Mastery2ValueScalingLabel.Name = "Mastery2ValueScalingLabel";
            this.Mastery2ValueScalingLabel.Size = new System.Drawing.Size(545, 22);
            this.Mastery2ValueScalingLabel.TabIndex = 21;
            this.Mastery2ValueScalingLabel.Tag = "{0} skills, {1} points";
            this.Mastery2ValueScalingLabel.Text = "{0} skills, {1} points";
            this.Mastery2ValueScalingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Mastery1ValueScalingLabel
            // 
            this.Mastery1ValueScalingLabel.Font = new System.Drawing.Font("Albertus MT", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Mastery1ValueScalingLabel.Location = new System.Drawing.Point(258, 31);
            this.Mastery1ValueScalingLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Mastery1ValueScalingLabel.Name = "Mastery1ValueScalingLabel";
            this.Mastery1ValueScalingLabel.Size = new System.Drawing.Size(545, 22);
            this.Mastery1ValueScalingLabel.TabIndex = 20;
            this.Mastery1ValueScalingLabel.Tag = "{0} skills, {1} points";
            this.Mastery1ValueScalingLabel.Text = "{0} skills, {1} points";
            this.Mastery1ValueScalingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Mastery2NameScalingLabel
            // 
            this.Mastery2NameScalingLabel.Font = new System.Drawing.Font("Albertus MT", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Mastery2NameScalingLabel.Location = new System.Drawing.Point(8, 69);
            this.Mastery2NameScalingLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Mastery2NameScalingLabel.Name = "Mastery2NameScalingLabel";
            this.Mastery2NameScalingLabel.Size = new System.Drawing.Size(242, 22);
            this.Mastery2NameScalingLabel.TabIndex = 19;
            this.Mastery2NameScalingLabel.Tag = "{0} :";
            this.Mastery2NameScalingLabel.Text = "Defense :";
            this.Mastery2NameScalingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Mastery1NameScalingLabel
            // 
            this.Mastery1NameScalingLabel.Font = new System.Drawing.Font("Albertus MT", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Mastery1NameScalingLabel.Location = new System.Drawing.Point(8, 31);
            this.Mastery1NameScalingLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Mastery1NameScalingLabel.Name = "Mastery1NameScalingLabel";
            this.Mastery1NameScalingLabel.Size = new System.Drawing.Size(242, 22);
            this.Mastery1NameScalingLabel.TabIndex = 7;
            this.Mastery1NameScalingLabel.Tag = "{0} :";
            this.Mastery1NameScalingLabel.Text = "Warfare :";
            this.Mastery1NameScalingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // CharacterEditDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.ClientSize = new System.Drawing.Size(932, 504);
            this.Controls.Add(this.MasteriesGroupBox);
            this.Controls.Add(this.ResetMasteriesScalingButton);
            this.Controls.Add(this.levelingGroupBox);
            this.Controls.Add(this.attribGroupBox);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.ok);
            this.DrawCustomBorder = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CharacterEditDialog";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Character Editor";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.CharacterEditDlg_Load);
            this.Controls.SetChildIndex(this.ok, 0);
            this.Controls.SetChildIndex(this.cancel, 0);
            this.Controls.SetChildIndex(this.attribGroupBox, 0);
            this.Controls.SetChildIndex(this.levelingGroupBox, 0);
            this.Controls.SetChildIndex(this.ResetMasteriesScalingButton, 0);
            this.Controls.SetChildIndex(this.MasteriesGroupBox, 0);
            this.attribGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.manacUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.healthUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.intelligenceUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dexterityUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.strengthUpDown)).EndInit();
            this.levelingGroupBox.ResumeLayout(false);
            this.levelingGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.skillPointsNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.attributeNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.levelNumericUpDown)).EndInit();
            this.MasteriesGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion

		private ScalingLabel strengthLabel;
		private ScalingLabel dexterityLabel;
		private ScalingLabel IntelligenceLabel;
		private ScalingLabel healthLabel;
		private ScalingLabel manaLabel;
		private System.Windows.Forms.GroupBox attribGroupBox;
		private System.Windows.Forms.NumericUpDown manacUpDown;
		private System.Windows.Forms.NumericUpDown healthUpDown;
		private System.Windows.Forms.NumericUpDown intelligenceUpDown;
		private System.Windows.Forms.NumericUpDown dexterityUpDown;
		private System.Windows.Forms.NumericUpDown strengthUpDown;
		private System.Windows.Forms.GroupBox levelingGroupBox;
		private System.Windows.Forms.TextBox xpTextBox;
		private ScalingLabel xpLabel1;
		private System.Windows.Forms.NumericUpDown levelNumericUpDown;
		private ScalingLabel levelLabel1;
		private System.Windows.Forms.NumericUpDown attributeNumericUpDown;
		private ScalingLabel attributeLabel1;
		private System.Windows.Forms.NumericUpDown skillPointsNumericUpDown;
		private ScalingLabel skillPointsLabel1;
		private ScalingLabel difficultyLabel;
		private System.Windows.Forms.ComboBox difficultlyComboBox;
		private ScalingCheckBox levelingCheckBox;
		private ScalingButton ResetMasteriesScalingButton;
		private System.Windows.Forms.GroupBox MasteriesGroupBox;
		private ScalingLabel Mastery2ValueScalingLabel;
		private ScalingLabel Mastery1ValueScalingLabel;
		private ScalingLabel Mastery2NameScalingLabel;
		private ScalingLabel Mastery1NameScalingLabel;
	}
}