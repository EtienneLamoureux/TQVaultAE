//-----------------------------------------------------------------------
// <copyright file="VaultForm.Designer.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using TQVaultAE.GUI.Components;

namespace TQVaultAE.GUI
{
	/// <summary>
	/// VaultForm Designer file.
	/// </summary>
	public partial class VaultForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// ScalingButton for the Close button.
		/// </summary>
		private ScalingButton buttonClose;

		/// <summary>
		/// ScalingButton for the Minimize button.
		/// </summary>
		private ScalingButton buttonMinimize;

		/// <summary>
		/// ScalingButton for the Maximize button.
		/// </summary>
		private ScalingButton buttonMaximize;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (this.components != null))
			{
				this.components.Dispose();
				this.topBorder.Dispose();
				this.bottomBorder.Dispose();
				this.sideBorder.Dispose();
				this.bottomRightCorner.Dispose();
				this.bottomLeftCorner.Dispose();
				this.titleFont.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VaultForm));
            this.buttonMaximize = new TQVaultAE.GUI.Components.ScalingButton();
            this.buttonMinimize = new TQVaultAE.GUI.Components.ScalingButton();
            this.buttonClose = new TQVaultAE.GUI.Components.ScalingButton();
            this.toolTipButton = new System.Windows.Forms.ToolTip(this.components);
            this.ButtonScaleTo1 = new TQVaultAE.GUI.Components.ScalingButton();
            this.SuspendLayout();
            // 
            // buttonMaximize
            // 
            this.buttonMaximize.BackColor = System.Drawing.Color.Transparent;
            this.buttonMaximize.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("buttonMaximize.DownBitmap")));
            this.buttonMaximize.FlatAppearance.BorderSize = 0;
            this.buttonMaximize.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.buttonMaximize.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.buttonMaximize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonMaximize.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.buttonMaximize.Image = ((System.Drawing.Image)(resources.GetObject("buttonMaximize.Image")));
            this.buttonMaximize.Location = new System.Drawing.Point(45, -2);
            this.buttonMaximize.Name = "buttonMaximize";
            this.buttonMaximize.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("buttonMaximize.OverBitmap")));
            this.buttonMaximize.Size = new System.Drawing.Size(15, 15);
            this.buttonMaximize.SizeToGraphic = false;
            this.buttonMaximize.TabIndex = 2;
            this.toolTipButton.SetToolTip(this.buttonMaximize, "Maximize");
            this.buttonMaximize.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("buttonMaximize.UpBitmap")));
            this.buttonMaximize.UseCustomGraphic = true;
            this.buttonMaximize.UseVisualStyleBackColor = true;
            this.buttonMaximize.Click += new System.EventHandler(this.MaximizeButtonClick);
            // 
            // buttonMinimize
            // 
            this.buttonMinimize.BackColor = System.Drawing.Color.Transparent;
            this.buttonMinimize.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("buttonMinimize.DownBitmap")));
            this.buttonMinimize.FlatAppearance.BorderSize = 0;
            this.buttonMinimize.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.buttonMinimize.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.buttonMinimize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonMinimize.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.buttonMinimize.Image = ((System.Drawing.Image)(resources.GetObject("buttonMinimize.Image")));
            this.buttonMinimize.Location = new System.Drawing.Point(5, -2);
            this.buttonMinimize.Name = "buttonMinimize";
            this.buttonMinimize.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("buttonMinimize.OverBitmap")));
            this.buttonMinimize.Size = new System.Drawing.Size(15, 15);
            this.buttonMinimize.SizeToGraphic = false;
            this.buttonMinimize.TabIndex = 1;
            this.toolTipButton.SetToolTip(this.buttonMinimize, "Minimize");
            this.buttonMinimize.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("buttonMinimize.UpBitmap")));
            this.buttonMinimize.UseCustomGraphic = true;
            this.buttonMinimize.UseVisualStyleBackColor = true;
            this.buttonMinimize.Click += new System.EventHandler(this.MinimizeButtonClick);
            // 
            // buttonClose
            // 
            this.buttonClose.BackColor = System.Drawing.Color.Transparent;
            this.buttonClose.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("buttonClose.DownBitmap")));
            this.buttonClose.FlatAppearance.BorderSize = 0;
            this.buttonClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.buttonClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.buttonClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.buttonClose.Image = ((System.Drawing.Image)(resources.GetObject("buttonClose.Image")));
            this.buttonClose.Location = new System.Drawing.Point(67, -2);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("buttonClose.OverBitmap")));
            this.buttonClose.Size = new System.Drawing.Size(26, 26);
            this.buttonClose.SizeToGraphic = false;
            this.buttonClose.TabIndex = 0;
            this.toolTipButton.SetToolTip(this.buttonClose, "Close");
            this.buttonClose.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("buttonClose.UpBitmap")));
            this.buttonClose.UseCustomGraphic = true;
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.CloseButtonClick);
            // 
            // ButtonScaleTo1
            // 
            this.ButtonScaleTo1.BackColor = System.Drawing.Color.Transparent;
            this.ButtonScaleTo1.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ButtonScaleTo1.DownBitmap")));
            this.ButtonScaleTo1.FlatAppearance.BorderSize = 0;
            this.ButtonScaleTo1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ButtonScaleTo1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ButtonScaleTo1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ButtonScaleTo1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.ButtonScaleTo1.Image = ((System.Drawing.Image)(resources.GetObject("ButtonScaleTo1.Image")));
            this.ButtonScaleTo1.Location = new System.Drawing.Point(26, -2);
            this.ButtonScaleTo1.Name = "ButtonScaleTo1";
            this.ButtonScaleTo1.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ButtonScaleTo1.OverBitmap")));
            this.ButtonScaleTo1.Size = new System.Drawing.Size(15, 15);
            this.ButtonScaleTo1.SizeToGraphic = false;
            this.ButtonScaleTo1.TabIndex = 3;
            this.toolTipButton.SetToolTip(this.ButtonScaleTo1, "Scale back to normal size");
            this.ButtonScaleTo1.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ButtonScaleTo1.UpBitmap")));
            this.ButtonScaleTo1.UseCustomGraphic = true;
            this.ButtonScaleTo1.UseVisualStyleBackColor = true;
            this.ButtonScaleTo1.Click += new System.EventHandler(this.ButtonScaleTo1_Click);
            // 
            // VaultForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.ButtonScaleTo1);
            this.Controls.Add(this.buttonMaximize);
            this.Controls.Add(this.buttonMinimize);
            this.Controls.Add(this.buttonClose);
            this.Name = "VaultForm";
            this.Text = "VaultForm";
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ToolTip toolTipButton;
		private ScalingButton ButtonScaleTo1;
	}
}