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
            this.ok = new TQVaultAE.GUI.ScalingButton();
            this.cancel = new TQVaultAE.GUI.ScalingButton();
            this.strengthLabel = new TQVaultAE.GUI.ScalingLabel();
            this.dexterityLabel = new TQVaultAE.GUI.ScalingLabel();
            this.IntelligenceLabel = new TQVaultAE.GUI.ScalingLabel();
            this.healthLabel = new TQVaultAE.GUI.ScalingLabel();
            this.manaLabel = new TQVaultAE.GUI.ScalingLabel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.strengthUpDown = new System.Windows.Forms.NumericUpDown();
            this.dexterityUpDown = new System.Windows.Forms.NumericUpDown();
            this.intelligenceUpDown = new System.Windows.Forms.NumericUpDown();
            this.healthUpDown = new System.Windows.Forms.NumericUpDown();
            this.manacUpDown = new System.Windows.Forms.NumericUpDown();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.strengthUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dexterityUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.intelligenceUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.healthUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.manacUpDown)).BeginInit();
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
            this.ok.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.ok.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ok.Image = ((System.Drawing.Image)(resources.GetObject("ok.Image")));
            this.ok.Location = new System.Drawing.Point(31, 349);
            this.ok.Name = "ok";
            this.ok.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ok.OverBitmap")));
            this.ok.Size = new System.Drawing.Size(137, 30);
            this.ok.SizeToGraphic = false;
            this.ok.TabIndex = 2;
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
            this.cancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.cancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.cancel.Image = ((System.Drawing.Image)(resources.GetObject("cancel.Image")));
            this.cancel.Location = new System.Drawing.Point(174, 349);
            this.cancel.Name = "cancel";
            this.cancel.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("cancel.OverBitmap")));
            this.cancel.Size = new System.Drawing.Size(137, 30);
            this.cancel.SizeToGraphic = false;
            this.cancel.TabIndex = 3;
            this.cancel.Text = "Cancel";
            this.cancel.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("cancel.UpBitmap")));
            this.cancel.UseCustomGraphic = true;
            this.cancel.UseVisualStyleBackColor = false;
            this.cancel.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // strengthLabel
            // 
            this.strengthLabel.AutoSize = true;
            this.strengthLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.strengthLabel.Location = new System.Drawing.Point(55, 35);
            this.strengthLabel.Name = "strengthLabel";
            this.strengthLabel.Size = new System.Drawing.Size(63, 18);
            this.strengthLabel.TabIndex = 5;
            this.strengthLabel.Text = "Strength";
            // 
            // dexterityLabel
            // 
            this.dexterityLabel.AutoSize = true;
            this.dexterityLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.dexterityLabel.Location = new System.Drawing.Point(53, 77);
            this.dexterityLabel.Name = "dexterityLabel";
            this.dexterityLabel.Size = new System.Drawing.Size(65, 18);
            this.dexterityLabel.TabIndex = 7;
            this.dexterityLabel.Text = "Dexterity";
            // 
            // IntelligenceLabel
            // 
            this.IntelligenceLabel.AutoSize = true;
            this.IntelligenceLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.IntelligenceLabel.Location = new System.Drawing.Point(38, 117);
            this.IntelligenceLabel.Name = "IntelligenceLabel";
            this.IntelligenceLabel.Size = new System.Drawing.Size(80, 18);
            this.IntelligenceLabel.TabIndex = 9;
            this.IntelligenceLabel.Text = "Intelligence";
            // 
            // healthLabel
            // 
            this.healthLabel.AutoSize = true;
            this.healthLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.healthLabel.Location = new System.Drawing.Point(68, 162);
            this.healthLabel.Name = "healthLabel";
            this.healthLabel.Size = new System.Drawing.Size(50, 18);
            this.healthLabel.TabIndex = 11;
            this.healthLabel.Text = "Health";
            // 
            // manaLabel
            // 
            this.manaLabel.AutoSize = true;
            this.manaLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.manaLabel.Location = new System.Drawing.Point(73, 208);
            this.manaLabel.Name = "manaLabel";
            this.manaLabel.Size = new System.Drawing.Size(45, 18);
            this.manaLabel.TabIndex = 13;
            this.manaLabel.Text = "Mana";
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Transparent;
            this.groupBox1.Controls.Add(this.manacUpDown);
            this.groupBox1.Controls.Add(this.healthUpDown);
            this.groupBox1.Controls.Add(this.intelligenceUpDown);
            this.groupBox1.Controls.Add(this.dexterityUpDown);
            this.groupBox1.Controls.Add(this.strengthUpDown);
            this.groupBox1.Controls.Add(this.manaLabel);
            this.groupBox1.Controls.Add(this.healthLabel);
            this.groupBox1.Controls.Add(this.IntelligenceLabel);
            this.groupBox1.Controls.Add(this.dexterityLabel);
            this.groupBox1.Controls.Add(this.strengthLabel);
            this.groupBox1.ForeColor = System.Drawing.Color.Gold;
            this.groupBox1.Location = new System.Drawing.Point(31, 50);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(261, 254);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Attributes";
            // 
            // strengthUpDown
            // 
            this.strengthUpDown.Increment = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.strengthUpDown.Location = new System.Drawing.Point(124, 33);
            this.strengthUpDown.Maximum = new decimal(new int[] {
            9996,
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
            this.strengthUpDown.Size = new System.Drawing.Size(120, 24);
            this.strengthUpDown.TabIndex = 1;
            this.strengthUpDown.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // dexterityUpDown
            // 
            this.dexterityUpDown.Increment = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.dexterityUpDown.Location = new System.Drawing.Point(124, 75);
            this.dexterityUpDown.Maximum = new decimal(new int[] {
            9996,
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
            this.dexterityUpDown.Size = new System.Drawing.Size(120, 24);
            this.dexterityUpDown.TabIndex = 2;
            this.dexterityUpDown.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // intelligenceUpDown
            // 
            this.intelligenceUpDown.Increment = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.intelligenceUpDown.Location = new System.Drawing.Point(124, 115);
            this.intelligenceUpDown.Maximum = new decimal(new int[] {
            9996,
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
            this.intelligenceUpDown.Size = new System.Drawing.Size(120, 24);
            this.intelligenceUpDown.TabIndex = 3;
            this.intelligenceUpDown.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // healthUpDown
            // 
            this.healthUpDown.Increment = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.healthUpDown.Location = new System.Drawing.Point(124, 160);
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
            this.healthUpDown.Size = new System.Drawing.Size(120, 24);
            this.healthUpDown.TabIndex = 4;
            this.healthUpDown.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
            // 
            // manacUpDown
            // 
            this.manacUpDown.Increment = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.manacUpDown.Location = new System.Drawing.Point(124, 206);
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
            this.manacUpDown.Size = new System.Drawing.Size(120, 24);
            this.manacUpDown.TabIndex = 5;
            this.manacUpDown.Value = new decimal(new int[] {
            300,
            0,
            0,
            0});
            // 
            // CharacterEditDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.ClientSize = new System.Drawing.Size(348, 391);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.ok);
            this.DrawCustomBorder = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CharacterEditDialog";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Character Editor";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.ItemSeedDlg_Load);
            this.Controls.SetChildIndex(this.ok, 0);
            this.Controls.SetChildIndex(this.cancel, 0);
            this.Controls.SetChildIndex(this.groupBox1, 0);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.strengthUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dexterityUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.intelligenceUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.healthUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.manacUpDown)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion

		private ScalingLabel strengthLabel;
		private ScalingLabel dexterityLabel;
		private ScalingLabel IntelligenceLabel;
		private ScalingLabel healthLabel;
		private ScalingLabel manaLabel;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.NumericUpDown manacUpDown;
		private System.Windows.Forms.NumericUpDown healthUpDown;
		private System.Windows.Forms.NumericUpDown intelligenceUpDown;
		private System.Windows.Forms.NumericUpDown dexterityUpDown;
		private System.Windows.Forms.NumericUpDown strengthUpDown;
	}
}