namespace TQVaultAE.GUI.Components
{
	partial class IconMagnifier
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.scalingLabelFilename = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingLabelSize = new TQVaultAE.GUI.Components.ScalingLabel();
            this.flowLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // flowLayoutPanel
            // 
            this.flowLayoutPanel.AutoSize = true;
            this.flowLayoutPanel.Controls.Add(this.pictureBox);
            this.flowLayoutPanel.Controls.Add(this.scalingLabelFilename);
            this.flowLayoutPanel.Controls.Add(this.scalingLabelSize);
            this.flowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel.Name = "flowLayoutPanel";
            this.flowLayoutPanel.Size = new System.Drawing.Size(64, 90);
            this.flowLayoutPanel.TabIndex = 0;
            // 
            // pictureBox
            // 
            this.pictureBox.Location = new System.Drawing.Point(0, 0);
            this.pictureBox.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(64, 64);
            this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox.TabIndex = 0;
            this.pictureBox.TabStop = false;
            // 
            // scalingLabelFilename
            // 
            this.scalingLabelFilename.AutoSize = true;
            this.scalingLabelFilename.Font = new System.Drawing.Font("Albertus MT", 8F);
            this.scalingLabelFilename.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelFilename.Location = new System.Drawing.Point(3, 64);
            this.scalingLabelFilename.Name = "scalingLabelFilename";
            this.scalingLabelFilename.Size = new System.Drawing.Size(48, 13);
            this.scalingLabelFilename.TabIndex = 1;
            this.scalingLabelFilename.Text = "Filename";
            // 
            // scalingLabelSize
            // 
            this.scalingLabelSize.AutoSize = true;
            this.scalingLabelSize.Font = new System.Drawing.Font("Albertus MT Light", 8F);
            this.scalingLabelSize.ForeColor = System.Drawing.Color.White;
            this.scalingLabelSize.Location = new System.Drawing.Point(3, 77);
            this.scalingLabelSize.Name = "scalingLabelSize";
            this.scalingLabelSize.Size = new System.Drawing.Size(44, 13);
            this.scalingLabelSize.TabIndex = 2;
            this.scalingLabelSize.Text = "64 x 64";
            // 
            // IconMagnifier
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.flowLayoutPanel);
            this.Name = "IconMagnifier";
            this.Size = new System.Drawing.Size(64, 90);
            this.flowLayoutPanel.ResumeLayout(false);
            this.flowLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel;
		internal System.Windows.Forms.PictureBox pictureBox;
		internal ScalingLabel scalingLabelFilename;
		internal ScalingLabel scalingLabelSize;
	}
}
