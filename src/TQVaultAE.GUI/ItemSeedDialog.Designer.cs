//-----------------------------------------------------------------------
// <copyright file="ItemSeedDialog.Designer.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI
{
	/// <summary>
	/// Form designer class for ItemSeedDialog
	/// </summary>
	internal partial class ItemSeedDialog
	{
		/// <summary>
		/// Item Seed input box
		/// </summary>
		private ScalingTextBox itemSeedBox;

		/// <summary>
		/// Random seed button
		/// </summary>
		private ScalingButton randomButton;

		/// <summary>
		/// OK button control
		/// </summary>
		private ScalingButton ok;

		/// <summary>
		/// Cancel button
		/// </summary>
		private ScalingButton cancel;

		/// <summary>
		/// label1 control
		/// </summary>
		private ScalingLabel label1;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ItemSeedDialog));
            this.itemSeedBox = new TQVaultAE.GUI.ScalingTextBox();
            this.randomButton = new TQVaultAE.GUI.ScalingButton();
            this.ok = new TQVaultAE.GUI.ScalingButton();
            this.cancel = new TQVaultAE.GUI.ScalingButton();
            this.label1 = new TQVaultAE.GUI.ScalingLabel();
            this.SuspendLayout();
            // 
            // itemSeedBox
            // 
            this.itemSeedBox.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.itemSeedBox.Location = new System.Drawing.Point(79, 225);
            this.itemSeedBox.MaxLength = 5;
            this.itemSeedBox.Name = "itemSeedBox";
            this.itemSeedBox.Size = new System.Drawing.Size(85, 25);
            this.itemSeedBox.TabIndex = 0;
            this.itemSeedBox.Text = "123456";
            // 
            // randomButton
            // 
            this.randomButton.BackColor = System.Drawing.Color.Transparent;
            this.randomButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("randomButton.DownBitmap")));
            this.randomButton.FlatAppearance.BorderSize = 0;
            this.randomButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.randomButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.randomButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.randomButton.Font = new System.Drawing.Font("Albertus MT Light", 12F);
            this.randomButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.randomButton.Image = ((System.Drawing.Image)(resources.GetObject("randomButton.Image")));
            this.randomButton.Location = new System.Drawing.Point(196, 224);
            this.randomButton.Name = "randomButton";
            this.randomButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("randomButton.OverBitmap")));
            this.randomButton.Size = new System.Drawing.Size(137, 30);
            this.randomButton.SizeToGraphic = false;
            this.randomButton.TabIndex = 1;
            this.randomButton.Text = "Random";
            this.randomButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("randomButton.UpBitmap")));
            this.randomButton.UseCustomGraphic = true;
            this.randomButton.UseVisualStyleBackColor = false;
            this.randomButton.Click += new System.EventHandler(this.RandomButtonClicked);
            // 
            // ok
            // 
            this.ok.BackColor = System.Drawing.Color.Transparent;
            this.ok.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ok.DownBitmap")));
            this.ok.FlatAppearance.BorderSize = 0;
            this.ok.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ok.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ok.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ok.Font = new System.Drawing.Font("Albertus MT Light", 12F);
            this.ok.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ok.Image = ((System.Drawing.Image)(resources.GetObject("ok.Image")));
            this.ok.Location = new System.Drawing.Point(54, 285);
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
            this.cancel.Font = new System.Drawing.Font("Albertus MT Light", 12F);
            this.cancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.cancel.Image = ((System.Drawing.Image)(resources.GetObject("cancel.Image")));
            this.cancel.Location = new System.Drawing.Point(214, 285);
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.label1.Location = new System.Drawing.Point(12, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(376, 119);
            this.label1.TabIndex = 4;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // ItemSeedDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.ClientSize = new System.Drawing.Size(410, 334);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.ok);
            this.Controls.Add(this.randomButton);
            this.Controls.Add(this.itemSeedBox);
            this.DrawCustomBorder = true;
            this.Font = new System.Drawing.Font("Albertus MT Light", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ItemSeedDialog";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Change Item Seed";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.ItemSeedDlg_Load);
            this.Controls.SetChildIndex(this.itemSeedBox, 0);
            this.Controls.SetChildIndex(this.randomButton, 0);
            this.Controls.SetChildIndex(this.ok, 0);
            this.Controls.SetChildIndex(this.cancel, 0);
            this.Controls.SetChildIndex(this.label1, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
	}
}