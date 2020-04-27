﻿namespace TQVaultAE.GUI.Tooltip
{
	partial class FoundResultsTooltip
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.flowLayoutPanelFriendlyNames = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // flowLayoutPanelFriendlyNames
            // 
            this.flowLayoutPanelFriendlyNames.AutoSize = true;
            this.flowLayoutPanelFriendlyNames.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelFriendlyNames.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(10)))), ((int)(((byte)(10)))));
            this.flowLayoutPanelFriendlyNames.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanelFriendlyNames.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelFriendlyNames.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelFriendlyNames.Location = new System.Drawing.Point(1, 1);
            this.flowLayoutPanelFriendlyNames.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.flowLayoutPanelFriendlyNames.Name = "flowLayoutPanelFriendlyNames";
            this.flowLayoutPanelFriendlyNames.Size = new System.Drawing.Size(351, 252);
            this.flowLayoutPanelFriendlyNames.TabIndex = 0;
            // 
            // FoundResultsTooltip
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.DarkGray;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(353, 254);
            this.ControlBox = false;
            this.Controls.Add(this.flowLayoutPanelFriendlyNames);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FoundResultsTooltip";
            this.Opacity = 0.9D;
            this.Padding = new System.Windows.Forms.Padding(1);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "BagButtonTooltip";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ItemTooltip_FormClosing);
            this.Load += new System.EventHandler(this.ItemTooltip_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelFriendlyNames;
	}
}