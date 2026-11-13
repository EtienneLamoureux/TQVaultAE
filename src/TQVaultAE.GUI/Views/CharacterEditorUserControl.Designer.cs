using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Components;

namespace TQVaultAE.GUI.Views;

/// <summary>
/// Designer part of the character editor
/// </summary>
internal partial class CharacterEditorUserControl
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CharacterEditorUserControl));
        this.ok = new TQVaultAE.GUI.Components.Scaling.ScalingButton();
        this.cancel = new TQVaultAE.GUI.Components.Scaling.ScalingButton();
        this.manacUpDown = new System.Windows.Forms.NumericUpDown();
        this.healthUpDown = new System.Windows.Forms.NumericUpDown();
        this.intelligenceUpDown = new System.Windows.Forms.NumericUpDown();
        this.dexterityUpDown = new System.Windows.Forms.NumericUpDown();
        this.strengthUpDown = new System.Windows.Forms.NumericUpDown();
        this.levelingCheckBox = new TQVaultAE.GUI.Components.Scaling.ScalingCheckBox();
        this.difficultlyComboBox = new System.Windows.Forms.ComboBox();
        this.skillPointsNumericUpDown = new System.Windows.Forms.NumericUpDown();
        this.attributeNumericUpDown = new System.Windows.Forms.NumericUpDown();
        this.xpTextBox = new System.Windows.Forms.TextBox();
        this.levelNumericUpDown = new System.Windows.Forms.NumericUpDown();
        this.ResetMasteriesScalingButton = new TQVaultAE.GUI.Components.Scaling.ScalingButton();
        this.ResetAttributesScalingButton = new TQVaultAE.GUI.Components.Scaling.ScalingButton();
        ((System.ComponentModel.ISupportInitialize)(this.manacUpDown)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.healthUpDown)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.intelligenceUpDown)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.dexterityUpDown)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.strengthUpDown)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.skillPointsNumericUpDown)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.attributeNumericUpDown)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.levelNumericUpDown)).BeginInit();
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
        this.ok.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.6F);
        this.ok.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.ok.Image = ((System.Drawing.Image)(resources.GetObject("ok.Image")));
        this.ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.ok.Location = new System.Drawing.Point(699, 665);
        this.ok.Name = "ok";
        this.ok.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ok.OverBitmap")));
        this.ok.Size = new System.Drawing.Size(137, 30);
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
        this.cancel.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("cancel.DownBitmap")));
        this.cancel.FlatAppearance.BorderSize = 0;
        this.cancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.cancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.cancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.cancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.6F);
        this.cancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.cancel.Image = ((System.Drawing.Image)(resources.GetObject("cancel.Image")));
        this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.cancel.Location = new System.Drawing.Point(844, 665);
        this.cancel.Name = "cancel";
        this.cancel.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("cancel.OverBitmap")));
        this.cancel.Size = new System.Drawing.Size(137, 30);
        this.cancel.SizeToGraphic = false;
        this.cancel.TabIndex = 14;
        this.cancel.Text = "Cancel";
        this.cancel.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("cancel.UpBitmap")));
        this.cancel.UseCustomGraphic = true;
        this.cancel.UseVisualStyleBackColor = false;
        this.cancel.Click += new System.EventHandler(this.CancelButton_Click);
        this.Controls.Add(this.manacUpDown);
        this.Controls.Add(this.healthUpDown);
        this.Controls.Add(this.intelligenceUpDown);
        this.Controls.Add(this.dexterityUpDown);
        this.Controls.Add(this.strengthUpDown);
        // 
        // manacUpDown
        // 
        this.manacUpDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.manacUpDown.Increment = new decimal(new int[] {
        40,
        0,
        0,
        0});
        this.manacUpDown.Location = new System.Drawing.Point(134, 151);
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
        this.manacUpDown.Size = new System.Drawing.Size(65, 24);
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
        this.healthUpDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.healthUpDown.Increment = new decimal(new int[] {
        40,
        0,
        0,
        0});
        this.healthUpDown.Location = new System.Drawing.Point(134, 121);
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
        this.healthUpDown.Size = new System.Drawing.Size(65, 24);
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
        this.intelligenceUpDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.intelligenceUpDown.Increment = new decimal(new int[] {
        4,
        0,
        0,
        0});
        this.intelligenceUpDown.Location = new System.Drawing.Point(134, 91);
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
        this.intelligenceUpDown.Size = new System.Drawing.Size(65, 24);
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
        this.dexterityUpDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.dexterityUpDown.Increment = new decimal(new int[] {
        4,
        0,
        0,
        0});
        this.dexterityUpDown.Location = new System.Drawing.Point(134, 61);
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
        this.dexterityUpDown.Size = new System.Drawing.Size(65, 24);
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
        this.strengthUpDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.strengthUpDown.Increment = new decimal(new int[] {
        4,
        0,
        0,
        0});
        this.strengthUpDown.Location = new System.Drawing.Point(134, 31);
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
        this.strengthUpDown.Size = new System.Drawing.Size(65, 24);
        this.strengthUpDown.TabIndex = 1;
        this.strengthUpDown.Value = new decimal(new int[] {
        50,
        0,
        0,
        0});
        this.strengthUpDown.ValueChanged += new System.EventHandler(this.StatsUpDown_ValueChanged);
        this.Controls.Add(this.levelingCheckBox);
        this.Controls.Add(this.difficultlyComboBox);
        this.Controls.Add(this.skillPointsNumericUpDown);
        this.Controls.Add(this.attributeNumericUpDown);
        this.Controls.Add(this.xpTextBox);
        this.Controls.Add(this.levelNumericUpDown);
        // 
        // levelingCheckBox
        // 
        this.levelingCheckBox.AutoSize = true;
        this.levelingCheckBox.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
        this.levelingCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.64F);
        this.levelingCheckBox.Location = new System.Drawing.Point(69, 179);
        this.levelingCheckBox.Name = "levelingCheckBox";
        this.levelingCheckBox.Size = new System.Drawing.Size(114, 19);
        this.levelingCheckBox.TabIndex = 12;
        this.levelingCheckBox.Text = "Enable Leveling";
        this.levelingCheckBox.UseVisualStyleBackColor = true;
        this.levelingCheckBox.CheckedChanged += new System.EventHandler(this.LevelingCheckBox_CheckedChanged);
        // 
        // difficultlyComboBox
        // 
        this.difficultlyComboBox.Enabled = false;
        this.difficultlyComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.difficultlyComboBox.FormattingEnabled = true;
        this.difficultlyComboBox.Location = new System.Drawing.Point(184, 147);
        this.difficultlyComboBox.Name = "difficultlyComboBox";
        this.difficultlyComboBox.Size = new System.Drawing.Size(160, 25);
        this.difficultlyComboBox.TabIndex = 11;
        this.difficultlyComboBox.EnabledChanged += new System.EventHandler(this.difficultlyComboBox_EnabledChanged);
        // 
        // skillPointsNumericUpDown
        // 
        this.skillPointsNumericUpDown.Enabled = false;
        this.skillPointsNumericUpDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.skillPointsNumericUpDown.Location = new System.Drawing.Point(184, 117);
        this.skillPointsNumericUpDown.Maximum = new decimal(new int[] {
        286,
        0,
        0,
        0});
        this.skillPointsNumericUpDown.Name = "skillPointsNumericUpDown";
        this.skillPointsNumericUpDown.ReadOnly = true;
        this.skillPointsNumericUpDown.Size = new System.Drawing.Size(160, 24);
        this.skillPointsNumericUpDown.TabIndex = 10;
        // 
        // attributeNumericUpDown
        // 
        this.attributeNumericUpDown.Enabled = false;
        this.attributeNumericUpDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.attributeNumericUpDown.Location = new System.Drawing.Point(184, 87);
        this.attributeNumericUpDown.Maximum = new decimal(new int[] {
        186,
        0,
        0,
        0});
        this.attributeNumericUpDown.Name = "attributeNumericUpDown";
        this.attributeNumericUpDown.ReadOnly = true;
        this.attributeNumericUpDown.Size = new System.Drawing.Size(160, 24);
        this.attributeNumericUpDown.TabIndex = 9;
        // 
        // xpTextBox
        // 
        this.xpTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.xpTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.xpTextBox.Location = new System.Drawing.Point(184, 57);
        this.xpTextBox.Name = "xpTextBox";
        this.xpTextBox.ReadOnly = true;
        this.xpTextBox.Size = new System.Drawing.Size(160, 24);
        this.xpTextBox.TabIndex = 8;
        this.xpTextBox.WordWrap = false;
        // 
        // levelNumericUpDown
        // 
        this.levelNumericUpDown.Enabled = false;
        this.levelNumericUpDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.levelNumericUpDown.Location = new System.Drawing.Point(184, 27);
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
        this.levelNumericUpDown.Size = new System.Drawing.Size(56, 24);
        this.levelNumericUpDown.TabIndex = 7;
        this.levelNumericUpDown.Value = new decimal(new int[] {
        1,
        0,
        0,
        0});
        this.levelNumericUpDown.ValueChanged += new System.EventHandler(this.LevelNumericUpDown_ValueChanged);
        // 
        // ResetMasteriesScalingButton
        // 
        this.ResetMasteriesScalingButton.BackColor = System.Drawing.Color.Transparent;
        this.ResetMasteriesScalingButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ResetMasteriesScalingButton.DownBitmap")));
        this.ResetMasteriesScalingButton.FlatAppearance.BorderSize = 0;
        this.ResetMasteriesScalingButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.ResetMasteriesScalingButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.ResetMasteriesScalingButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ResetMasteriesScalingButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.6F);
        this.ResetMasteriesScalingButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.ResetMasteriesScalingButton.Image = ((System.Drawing.Image)(resources.GetObject("ResetMasteriesScalingButton.Image")));
        this.ResetMasteriesScalingButton.Location = new System.Drawing.Point(8, 330);
        this.ResetMasteriesScalingButton.Name = "ResetMasteriesScalingButton";
        this.ResetMasteriesScalingButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ResetMasteriesScalingButton.OverBitmap")));
        this.ResetMasteriesScalingButton.Size = new System.Drawing.Size(137, 30);
        this.ResetMasteriesScalingButton.SizeToGraphic = false;
        this.ResetMasteriesScalingButton.TabIndex = 16;
        this.ResetMasteriesScalingButton.Text = "Reset Masteries";
        this.ResetMasteriesScalingButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ResetMasteriesScalingButton.UpBitmap")));
        this.ResetMasteriesScalingButton.UseCustomGraphic = true;
        this.ResetMasteriesScalingButton.UseVisualStyleBackColor = false;
        this.ResetMasteriesScalingButton.Click += new System.EventHandler(this.ResetMasteriesScalingButton_Click);
        this.Controls.Add(this.ResetMasteriesScalingButton);
        this.Controls.Add(this.ResetAttributesScalingButton);
        // 
        // ResetAttributesScalingButton
        // 
        this.ResetAttributesScalingButton.BackColor = System.Drawing.Color.Transparent;
        this.ResetAttributesScalingButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ResetAttributesScalingButton.DownBitmap")));
        this.ResetAttributesScalingButton.FlatAppearance.BorderSize = 0;
        this.ResetAttributesScalingButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.ResetAttributesScalingButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.ResetAttributesScalingButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.ResetAttributesScalingButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.6F);
        this.ResetAttributesScalingButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
        this.ResetAttributesScalingButton.Image = ((System.Drawing.Image)(resources.GetObject("ResetAttributesScalingButton.Image")));
        this.ResetAttributesScalingButton.Location = new System.Drawing.Point(166, 330);
        this.ResetAttributesScalingButton.Name = "ResetAttributesScalingButton";
        this.ResetAttributesScalingButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ResetAttributesScalingButton.OverBitmap")));
        this.ResetAttributesScalingButton.Size = new System.Drawing.Size(137, 30);
        this.ResetAttributesScalingButton.SizeToGraphic = false;
        this.ResetAttributesScalingButton.TabIndex = 18;
        this.ResetAttributesScalingButton.Text = "Reset Attributes";
        this.ResetAttributesScalingButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ResetAttributesScalingButton.UpBitmap")));
        this.ResetAttributesScalingButton.UseCustomGraphic = true;
        this.ResetAttributesScalingButton.UseVisualStyleBackColor = false;
        this.ResetAttributesScalingButton.Click += new System.EventHandler(this.ResetAttributesScalingButton_Click);
        // 
        // CharacterEditorUserControl
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.BackColor = System.Drawing.Color.Transparent;
        this.Controls.Add(this.cancel);
        this.Controls.Add(this.ok);
        this.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        this.ForeColor = System.Drawing.Color.White;
        this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
        this.Name = "CharacterEditorUserControl";
        this.Size = new System.Drawing.Size(1010, 710);
        this.Controls.SetChildIndex(this.ok, 0);
        this.Controls.SetChildIndex(this.cancel, 0);
        ((System.ComponentModel.ISupportInitialize)(this.manacUpDown)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.healthUpDown)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.intelligenceUpDown)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.dexterityUpDown)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.strengthUpDown)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.skillPointsNumericUpDown)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.attributeNumericUpDown)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.levelNumericUpDown)).EndInit();
        this.ResumeLayout(false);

	}

	#endregion

	private System.Windows.Forms.NumericUpDown manacUpDown;
	private System.Windows.Forms.NumericUpDown healthUpDown;
	private System.Windows.Forms.NumericUpDown intelligenceUpDown;
	private System.Windows.Forms.NumericUpDown dexterityUpDown;
	private System.Windows.Forms.NumericUpDown strengthUpDown;
	private System.Windows.Forms.TextBox xpTextBox;
	private System.Windows.Forms.NumericUpDown levelNumericUpDown;
	private System.Windows.Forms.NumericUpDown attributeNumericUpDown;
	private System.Windows.Forms.NumericUpDown skillPointsNumericUpDown;
	private System.Windows.Forms.ComboBox difficultlyComboBox;
	private ScalingCheckBox levelingCheckBox;
	private ScalingButton ResetMasteriesScalingButton;
	private ScalingButton ResetAttributesScalingButton;
}
