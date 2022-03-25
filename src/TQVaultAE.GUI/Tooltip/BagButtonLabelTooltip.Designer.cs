namespace TQVaultAE.GUI.Tooltip
{
	partial class BagButtonLabelTooltip
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
            this.scalingLabel = new TQVaultAE.GUI.Components.ScalingLabel();
            this.SuspendLayout();
            // 
            // scalingLabel
            // 
            this.scalingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scalingLabel.BackColor = System.Drawing.Color.Transparent;
            this.scalingLabel.Font = new System.Drawing.Font("Albertus MT", 15F);
            this.scalingLabel.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabel.Location = new System.Drawing.Point(5, 5);
            this.scalingLabel.Margin = new System.Windows.Forms.Padding(0);
            this.scalingLabel.Name = "scalingLabel";
            this.scalingLabel.Size = new System.Drawing.Size(177, 30);
            this.scalingLabel.TabIndex = 0;
            this.scalingLabel.Text = "scalingLabel";
            this.scalingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // BagButtonLabelTooltip
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(187, 40);
            this.ControlBox = false;
            this.Controls.Add(this.scalingLabel);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BagButtonLabelTooltip";
            this.Opacity = 0.9D;
            this.Padding = new System.Windows.Forms.Padding(5);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "BagButtonTooltip";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.ItemTooltip_Load);
            this.ResumeLayout(false);

		}

		#endregion

		private Components.ScalingLabel scalingLabel;
	}
}