//-----------------------------------------------------------------------
// <copyright file="SplashScreenForm.Designer.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using TQVaultAE.GUI.Properties;

namespace TQVaultAE.GUI
{
	/// <summary>
	/// Class for inital form which load the resources.
	/// </summary>
	internal partial class SplashScreenForm
	{

		/// <summary>
		/// Description label
		/// </summary>
		private ScalingLabel label3;

		/// <summary>
		/// Next button
		/// </summary>
		private ScalingButton nextButton;

		/// <summary>
		/// Exit button
		/// </summary>
		private ScalingButton exitButton;

		/// <summary>
		/// Please wait message
		/// </summary>
		private ScalingLabel labelPleaseWait;

		/// <summary>
		/// Wait timer.  Used for animation of please wait message.
		/// </summary>
		private System.Timers.Timer waitTimer;

		/// <summary>
		/// Custom Progress Bar control to show the progress of the files loading.
		/// </summary>
		private VaultProgressBar progressBar;

		/// <summary>
		/// Timer for fading in and out of the form.
		/// </summary>
		private System.Windows.Forms.Timer fadeTimer;

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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashScreenForm));
            this.label3 = new TQVaultAE.GUI.ScalingLabel();
            this.nextButton = new TQVaultAE.GUI.ScalingButton();
            this.exitButton = new TQVaultAE.GUI.ScalingButton();
            this.labelPleaseWait = new TQVaultAE.GUI.ScalingLabel();
            this.waitTimer = new System.Timers.Timer();
            this.fadeTimer = new System.Windows.Forms.Timer(this.components);
            this.progressBar = new TQVaultAE.GUI.VaultProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.waitTimer)).BeginInit();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("Albertus MT Light", 12F);
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(55, 255);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(607, 54);
            this.label3.TabIndex = 2;
            this.label3.Text = "A magical place where you can store your artifacts of power that you will need to" +
    " save the world from the Titans.";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // nextButton
            // 
            this.nextButton.BackColor = System.Drawing.Color.Transparent;
            this.nextButton.DownBitmap = global::TQVaultAE.GUI.Properties.Resources.MainButtonDown;
            this.nextButton.FlatAppearance.BorderSize = 0;
            this.nextButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.nextButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.nextButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.nextButton.Font = new System.Drawing.Font("Albertus MT Light", 12F);
            this.nextButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.nextButton.Image = global::TQVaultAE.GUI.Properties.Resources.MainButtonUp;
            this.nextButton.Location = new System.Drawing.Point(286, 365);
            this.nextButton.Name = "nextButton";
            this.nextButton.OverBitmap = global::TQVaultAE.GUI.Properties.Resources.MainButtonOver;
            this.nextButton.Size = new System.Drawing.Size(137, 30);
            this.nextButton.SizeToGraphic = false;
            this.nextButton.TabIndex = 3;
            this.nextButton.Text = "Enter The Vault";
            this.nextButton.UpBitmap = global::TQVaultAE.GUI.Properties.Resources.MainButtonUp;
            this.nextButton.UseCustomGraphic = true;
            this.nextButton.UseVisualStyleBackColor = false;
            this.nextButton.Visible = false;
            this.nextButton.Click += new System.EventHandler(this.NextButtonClick);
            // 
            // exitButton
            // 
            this.exitButton.BackColor = System.Drawing.Color.Transparent;
            this.exitButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.exitButton.DownBitmap = global::TQVaultAE.GUI.Properties.Resources.MainButtonDown;
            this.exitButton.FlatAppearance.BorderSize = 0;
            this.exitButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.exitButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.exitButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.exitButton.Font = new System.Drawing.Font("Albertus MT Light", 12F);
            this.exitButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.exitButton.Image = global::TQVaultAE.GUI.Properties.Resources.MainButtonUp;
            this.exitButton.Location = new System.Drawing.Point(555, 420);
            this.exitButton.Name = "exitButton";
            this.exitButton.OverBitmap = global::TQVaultAE.GUI.Properties.Resources.MainButtonOver;
            this.exitButton.Size = new System.Drawing.Size(137, 30);
            this.exitButton.SizeToGraphic = false;
            this.exitButton.TabIndex = 4;
            this.exitButton.Text = "Exit";
            this.exitButton.UpBitmap = global::TQVaultAE.GUI.Properties.Resources.MainButtonUp;
            this.exitButton.UseCustomGraphic = true;
            this.exitButton.UseVisualStyleBackColor = false;
            this.exitButton.Click += new System.EventHandler(this.ExitButtonClick);
            // 
            // labelPleaseWait
            // 
            this.labelPleaseWait.BackColor = System.Drawing.Color.Transparent;
            this.labelPleaseWait.Font = new System.Drawing.Font("Albertus MT Light", 14.25F);
            this.labelPleaseWait.ForeColor = System.Drawing.Color.Gold;
            this.labelPleaseWait.Location = new System.Drawing.Point(131, 318);
            this.labelPleaseWait.Name = "labelPleaseWait";
            this.labelPleaseWait.Size = new System.Drawing.Size(457, 32);
            this.labelPleaseWait.TabIndex = 5;
            this.labelPleaseWait.Text = "Loading Game Resources...Please Wait";
            this.labelPleaseWait.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // waitTimer
            // 
            this.waitTimer.Enabled = true;
            this.waitTimer.Interval = 500D;
            this.waitTimer.SynchronizingObject = this;
            this.waitTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.WaitTimerElapsed);
            // 
            // fadeTimer
            // 
            this.fadeTimer.Interval = 50;
            this.fadeTimer.Tick += new System.EventHandler(this.FadeTimerTick);
            // 
            // progressBar
            // 
            this.progressBar.BackColor = System.Drawing.Color.Transparent;
            this.progressBar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.progressBar.Location = new System.Drawing.Point(55, 353);
            this.progressBar.Maximum = 0;
            this.progressBar.Minimum = 0;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(602, 58);
            this.progressBar.TabIndex = 7;
            this.progressBar.Value = 0;
            // 
            // SplashScreenForm
            // 
            this.AcceptButton = this.nextButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackgroundImage = global::TQVaultAE.GUI.Properties.Resources.NewSplashScreen;
            this.CancelButton = this.exitButton;
            this.ClientSize = new System.Drawing.Size(716, 463);
            this.Controls.Add(this.labelPleaseWait);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.nextButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.progressBar);
            this.DrawCustomBorder = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SplashScreenForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Titan Quest Item Vault";
            this.Click += new System.EventHandler(this.SplashScreen_Click);
            this.Controls.SetChildIndex(this.progressBar, 0);
            this.Controls.SetChildIndex(this.label3, 0);
            this.Controls.SetChildIndex(this.nextButton, 0);
            this.Controls.SetChildIndex(this.exitButton, 0);
            this.Controls.SetChildIndex(this.labelPleaseWait, 0);
            ((System.ComponentModel.ISupportInitialize)(this.waitTimer)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion
	}
}

