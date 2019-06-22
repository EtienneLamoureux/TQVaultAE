//-----------------------------------------------------------------------
// <copyright file="ItemSeedDialog.Designer.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using TQVaultAE.GUI.Components;

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
		private ScalingButton ButtonRandom;

		/// <summary>
		/// OK button control
		/// </summary>
		private ScalingButton ButtonOk;

		/// <summary>
		/// Cancel button
		/// </summary>
		private ScalingButton ButtonCancel;

		/// <summary>
		/// label1 control
		/// </summary>
		private ScalingLabel labelInfos;

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
            this.itemSeedBox = new TQVaultAE.GUI.Components.ScalingTextBox();
            this.ButtonRandom = new TQVaultAE.GUI.Components.ScalingButton();
            this.ButtonOk = new TQVaultAE.GUI.Components.ScalingButton();
            this.ButtonCancel = new TQVaultAE.GUI.Components.ScalingButton();
            this.labelInfos = new TQVaultAE.GUI.Components.ScalingLabel();
            this.SuspendLayout();
            // 
            // itemSeedBox
            // 
            this.itemSeedBox.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.itemSeedBox.Location = new System.Drawing.Point(99, 281);
            this.itemSeedBox.Margin = new System.Windows.Forms.Padding(4);
            this.itemSeedBox.MaxLength = 5;
            this.itemSeedBox.Name = "itemSeedBox";
            this.itemSeedBox.Size = new System.Drawing.Size(105, 35);
            this.itemSeedBox.TabIndex = 0;
            this.itemSeedBox.Text = "123456";
            // 
            // ButtonRandom
            // 
            this.ButtonRandom.BackColor = System.Drawing.Color.Transparent;
            this.ButtonRandom.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ButtonRandom.DownBitmap")));
            this.ButtonRandom.FlatAppearance.BorderSize = 0;
            this.ButtonRandom.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ButtonRandom.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ButtonRandom.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonRandom.Font = new System.Drawing.Font("Albertus MT Light", 15F);
            this.ButtonRandom.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ButtonRandom.Image = ((System.Drawing.Image)(resources.GetObject("ButtonRandom.Image")));
            this.ButtonRandom.Location = new System.Drawing.Point(245, 280);
            this.ButtonRandom.Margin = new System.Windows.Forms.Padding(4);
            this.ButtonRandom.Name = "ButtonRandom";
            this.ButtonRandom.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ButtonRandom.OverBitmap")));
            this.ButtonRandom.Size = new System.Drawing.Size(171, 38);
            this.ButtonRandom.SizeToGraphic = false;
            this.ButtonRandom.TabIndex = 1;
            this.ButtonRandom.Text = "Random";
            this.ButtonRandom.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ButtonRandom.UpBitmap")));
            this.ButtonRandom.UseCustomGraphic = true;
            this.ButtonRandom.UseVisualStyleBackColor = false;
            this.ButtonRandom.Click += new System.EventHandler(this.RandomButtonClicked);
            // 
            // ButtonOk
            // 
            this.ButtonOk.BackColor = System.Drawing.Color.Transparent;
            this.ButtonOk.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ButtonOk.DownBitmap")));
            this.ButtonOk.FlatAppearance.BorderSize = 0;
            this.ButtonOk.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ButtonOk.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ButtonOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonOk.Font = new System.Drawing.Font("Albertus MT Light", 15F);
            this.ButtonOk.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ButtonOk.Image = ((System.Drawing.Image)(resources.GetObject("ButtonOk.Image")));
            this.ButtonOk.Location = new System.Drawing.Point(68, 356);
            this.ButtonOk.Margin = new System.Windows.Forms.Padding(4);
            this.ButtonOk.Name = "ButtonOk";
            this.ButtonOk.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ButtonOk.OverBitmap")));
            this.ButtonOk.Size = new System.Drawing.Size(171, 38);
            this.ButtonOk.SizeToGraphic = false;
            this.ButtonOk.TabIndex = 2;
            this.ButtonOk.Text = "OK";
            this.ButtonOk.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ButtonOk.UpBitmap")));
            this.ButtonOk.UseCustomGraphic = true;
            this.ButtonOk.UseVisualStyleBackColor = false;
            this.ButtonOk.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.BackColor = System.Drawing.Color.Transparent;
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ButtonCancel.DownBitmap")));
            this.ButtonCancel.FlatAppearance.BorderSize = 0;
            this.ButtonCancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ButtonCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ButtonCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonCancel.Font = new System.Drawing.Font("Albertus MT Light", 15F);
            this.ButtonCancel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ButtonCancel.Image = ((System.Drawing.Image)(resources.GetObject("ButtonCancel.Image")));
            this.ButtonCancel.Location = new System.Drawing.Point(268, 356);
            this.ButtonCancel.Margin = new System.Windows.Forms.Padding(4);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ButtonCancel.OverBitmap")));
            this.ButtonCancel.Size = new System.Drawing.Size(171, 38);
            this.ButtonCancel.SizeToGraphic = false;
            this.ButtonCancel.TabIndex = 3;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ButtonCancel.UpBitmap")));
            this.ButtonCancel.UseCustomGraphic = true;
            this.ButtonCancel.UseVisualStyleBackColor = false;
            this.ButtonCancel.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // labelInfos
            // 
            this.labelInfos.AutoSize = true;
            this.labelInfos.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.labelInfos.Location = new System.Drawing.Point(15, 36);
            this.labelInfos.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelInfos.Name = "labelInfos";
            this.labelInfos.Size = new System.Drawing.Size(604, 196);
            this.labelInfos.TabIndex = 4;
            this.labelInfos.Text = resources.GetString("labelInfos.Text");
            // 
            // ItemSeedDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.ClientSize = new System.Drawing.Size(512, 418);
            this.Controls.Add(this.labelInfos);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.ButtonOk);
            this.Controls.Add(this.ButtonRandom);
            this.Controls.Add(this.itemSeedBox);
            this.DrawCustomBorder = true;
            this.Font = new System.Drawing.Font("Albertus MT Light", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(5);
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
            this.Controls.SetChildIndex(this.ButtonRandom, 0);
            this.Controls.SetChildIndex(this.ButtonOk, 0);
            this.Controls.SetChildIndex(this.ButtonCancel, 0);
            this.Controls.SetChildIndex(this.labelInfos, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
	}
}