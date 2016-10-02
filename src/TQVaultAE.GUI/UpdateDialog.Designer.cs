//-----------------------------------------------------------------------
// <copyright file="UpdateDialog.Designer.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI
{
	/// <summary>
	/// Form Designer class for UpdateDialog
	/// </summary>
	internal partial class UpdateDialog
	{
		/// <summary>
		/// User messages text box
		/// </summary>
		private ScalingTextBox messageTextBox;

		/// <summary>
		/// OK button control
		/// </summary>
		private ScalingButton okayButton;

		/// <summary>
		/// Cancel button control
		/// </summary>
		private ScalingButton cancelButton;

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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateDialog));
			this.messageTextBox = new ScalingTextBox();
			this.okayButton = new ScalingButton();
			this.cancelButton = new ScalingButton();
			this.SuspendLayout();
			// 
			// messageTextBox
			// 
			this.messageTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
			this.messageTextBox.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
			this.messageTextBox.ForeColor = System.Drawing.Color.White;
			this.messageTextBox.Location = new System.Drawing.Point(20, 32);
			this.messageTextBox.Multiline = true;
			this.messageTextBox.Name = "messageTextBox";
			this.messageTextBox.ReadOnly = true;
			this.messageTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.messageTextBox.Size = new System.Drawing.Size(440, 384);
			this.messageTextBox.TabIndex = 0;
			// 
			// okayButton
			// 
			this.okayButton.BackColor = System.Drawing.Color.Transparent;
			this.okayButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("okayButton.DownBitmap")));
			this.okayButton.FlatAppearance.BorderSize = 0;
			this.okayButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
			this.okayButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
			this.okayButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.okayButton.Font = new System.Drawing.Font("Albertus MT Light", 12F);
			this.okayButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
			this.okayButton.Image = ((System.Drawing.Image)(resources.GetObject("okayButton.Image")));
			this.okayButton.Location = new System.Drawing.Point(94, 434);
			this.okayButton.Name = "okayButton";
			this.okayButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("okayButton.OverBitmap")));
			this.okayButton.Size = new System.Drawing.Size(137, 30);
			this.okayButton.SizeToGraphic = false;
			this.okayButton.TabIndex = 1;
			this.okayButton.Text = "OK";
			this.okayButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("okayButton.UpBitmap")));
			this.okayButton.UseCustomGraphic = true;
			this.okayButton.UseVisualStyleBackColor = false;
			this.okayButton.Click += new System.EventHandler(this.OkayButtonClick);
			// 
			// cancelButton
			// 
			this.cancelButton.BackColor = System.Drawing.Color.Transparent;
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("cancelButton.DownBitmap")));
			this.cancelButton.FlatAppearance.BorderSize = 0;
			this.cancelButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
			this.cancelButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
			this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cancelButton.Font = new System.Drawing.Font("Albertus MT Light", 12F);
			this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
			this.cancelButton.Image = ((System.Drawing.Image)(resources.GetObject("cancelButton.Image")));
			this.cancelButton.Location = new System.Drawing.Point(237, 434);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("cancelButton.OverBitmap")));
			this.cancelButton.Size = new System.Drawing.Size(137, 30);
			this.cancelButton.SizeToGraphic = false;
			this.cancelButton.TabIndex = 2;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("cancelButton.UpBitmap")));
			this.cancelButton.UseCustomGraphic = true;
			this.cancelButton.UseVisualStyleBackColor = false;
			this.cancelButton.Click += new System.EventHandler(this.CancelButtonClick);
			// 
			// UpdateDialog
			// 
			this.AcceptButton = this.okayButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(472, 476);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okayButton);
			this.Controls.Add(this.messageTextBox);
			this.DrawCustomBorder = true;
			this.Font = new System.Drawing.Font("Albertus MT", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ForeColor = System.Drawing.Color.White;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "UpdateDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "TQVault Updates";
			this.Controls.SetChildIndex(this.messageTextBox, 0);
			this.Controls.SetChildIndex(this.okayButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}