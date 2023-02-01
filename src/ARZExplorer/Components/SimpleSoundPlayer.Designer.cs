namespace ArzExplorer.Components
{
	partial class SimpleSoundPlayer
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
            this.components = new System.ComponentModel.Container();
            this.flowLayoutPanelPlayer = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonPlay = new System.Windows.Forms.Button();
            this.buttonLoop = new System.Windows.Forms.Button();
            this.buttonPause = new System.Windows.Forms.Button();
            this.buttonStop = new System.Windows.Forms.Button();
            this.labelFileName = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.flowLayoutPanelPlayer.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanelPlayer
            // 
            this.flowLayoutPanelPlayer.AutoSize = true;
            this.flowLayoutPanelPlayer.Controls.Add(this.buttonPlay);
            this.flowLayoutPanelPlayer.Controls.Add(this.buttonLoop);
            this.flowLayoutPanelPlayer.Controls.Add(this.buttonPause);
            this.flowLayoutPanelPlayer.Controls.Add(this.buttonStop);
            this.flowLayoutPanelPlayer.Controls.Add(this.labelFileName);
            this.flowLayoutPanelPlayer.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanelPlayer.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanelPlayer.Name = "flowLayoutPanelPlayer";
            this.flowLayoutPanelPlayer.Size = new System.Drawing.Size(292, 28);
            this.flowLayoutPanelPlayer.TabIndex = 0;
            // 
            // buttonPlay
            // 
            this.buttonPlay.Image = global::ArzExplorer.Properties.Resources.Play;
            this.buttonPlay.Location = new System.Drawing.Point(3, 3);
            this.buttonPlay.Name = "buttonPlay";
            this.buttonPlay.Size = new System.Drawing.Size(26, 22);
            this.buttonPlay.TabIndex = 0;
            this.buttonPlay.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip1.SetToolTip(this.buttonPlay, "Play");
            this.buttonPlay.UseVisualStyleBackColor = true;
            this.buttonPlay.Click += new System.EventHandler(this.buttonPlay_Click);
            // 
            // buttonLoop
            // 
            this.buttonLoop.AutoSize = true;
            this.buttonLoop.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.buttonLoop.Image = global::ArzExplorer.Properties.Resources.Loop;
            this.buttonLoop.Location = new System.Drawing.Point(35, 3);
            this.buttonLoop.Name = "buttonLoop";
            this.buttonLoop.Size = new System.Drawing.Size(26, 22);
            this.buttonLoop.TabIndex = 1;
            this.buttonLoop.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip1.SetToolTip(this.buttonLoop, "Loop");
            this.buttonLoop.UseVisualStyleBackColor = false;
            this.buttonLoop.Click += new System.EventHandler(this.buttonLoop_Click);
            // 
            // buttonPause
            // 
            this.buttonPause.Image = global::ArzExplorer.Properties.Resources.Pause;
            this.buttonPause.Location = new System.Drawing.Point(67, 3);
            this.buttonPause.Name = "buttonPause";
            this.buttonPause.Size = new System.Drawing.Size(26, 22);
            this.buttonPause.TabIndex = 2;
            this.buttonPause.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip1.SetToolTip(this.buttonPause, "Pause");
            this.buttonPause.UseVisualStyleBackColor = true;
            this.buttonPause.Click += new System.EventHandler(this.buttonPause_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.Image = global::ArzExplorer.Properties.Resources.Stop;
            this.buttonStop.Location = new System.Drawing.Point(99, 3);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(26, 22);
            this.buttonStop.TabIndex = 3;
            this.buttonStop.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.toolTip1.SetToolTip(this.buttonStop, "Stop");
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // labelFileName
            // 
            this.labelFileName.AutoSize = true;
            this.labelFileName.Location = new System.Drawing.Point(131, 3);
            this.labelFileName.Margin = new System.Windows.Forms.Padding(3);
            this.labelFileName.Name = "labelFileName";
            this.labelFileName.Padding = new System.Windows.Forms.Padding(3);
            this.labelFileName.Size = new System.Drawing.Size(96, 19);
            this.labelFileName.TabIndex = 4;
            this.labelFileName.Text = "MyFileName.MP3";
            this.labelFileName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // SimpleSoundPlayer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.flowLayoutPanelPlayer);
            this.MinimumSize = new System.Drawing.Size(292, 28);
            this.Name = "SimpleSoundPlayer";
            this.Size = new System.Drawing.Size(292, 28);
            this.Load += new System.EventHandler(this.SimpleSoundPlayer_Load);
            this.flowLayoutPanelPlayer.ResumeLayout(false);
            this.flowLayoutPanelPlayer.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelPlayer;
		private System.Windows.Forms.Button buttonPlay;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.Button buttonLoop;
		private System.Windows.Forms.Button buttonPause;
		private System.Windows.Forms.Button buttonStop;
		private System.Windows.Forms.Label labelFileName;
	}
}
