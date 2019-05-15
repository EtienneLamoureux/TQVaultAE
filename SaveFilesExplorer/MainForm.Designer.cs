namespace SaveFilesExplorer
{
	partial class MainForm
	{
		/// <summary>
		/// Variable nécessaire au concepteur.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Nettoyage des ressources utilisées.
		/// </summary>
		/// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Code généré par le Concepteur Windows Form

		/// <summary>
		/// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
		/// le contenu de cette méthode avec l'éditeur de code.
		/// </summary>
		private void InitializeComponent()
		{
            this.statusStripMain = new System.Windows.Forms.StatusStrip();
            this.menuStripMain = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialogMain = new System.Windows.Forms.OpenFileDialog();
            this.tabControlFiles = new System.Windows.Forms.TabControl();
            this.tabPageTemplate = new System.Windows.Forms.TabPage();
            this.folderBrowserDialogMain = new System.Windows.Forms.FolderBrowserDialog();
            this.menuStripMain.SuspendLayout();
            this.tabControlFiles.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStripMain
            // 
            this.statusStripMain.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStripMain.Location = new System.Drawing.Point(0, 815);
            this.statusStripMain.Name = "statusStripMain";
            this.statusStripMain.Size = new System.Drawing.Size(1156, 22);
            this.statusStripMain.TabIndex = 0;
            this.statusStripMain.Text = "statusStrip1";
            // 
            // menuStripMain
            // 
            this.menuStripMain.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2,
            this.toolStripMenuItem1,
            this.helpToolStripMenuItem});
            this.menuStripMain.Location = new System.Drawing.Point(0, 0);
            this.menuStripMain.Name = "menuStripMain";
            this.menuStripMain.Size = new System.Drawing.Size(1156, 28);
            this.menuStripMain.TabIndex = 1;
            this.menuStripMain.Text = "menuStrip1";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.openFileToolStripMenuItem});
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(44, 24);
            this.toolStripMenuItem2.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(228, 26);
            this.openToolStripMenuItem.Text = "Open player directory";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.OpenToolStripMenuItem_Click);
            // 
            // openFileToolStripMenuItem
            // 
            this.openFileToolStripMenuItem.Name = "openFileToolStripMenuItem";
            this.openFileToolStripMenuItem.Size = new System.Drawing.Size(228, 26);
            this.openFileToolStripMenuItem.Text = "Open file";
            this.openFileToolStripMenuItem.Click += new System.EventHandler(this.OpenFileToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(132, 24);
            this.toolStripMenuItem1.Text = "Detected Players";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(53, 24);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(125, 26);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
            // 
            // openFileDialogMain
            // 
            this.openFileDialogMain.FileOk += new System.ComponentModel.CancelEventHandler(this.OpenFileDialogMain_FileOk);
            // 
            // tabControlFiles
            // 
            this.tabControlFiles.Controls.Add(this.tabPageTemplate);
            this.tabControlFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlFiles.Location = new System.Drawing.Point(0, 28);
            this.tabControlFiles.Name = "tabControlFiles";
            this.tabControlFiles.SelectedIndex = 0;
            this.tabControlFiles.Size = new System.Drawing.Size(1156, 787);
            this.tabControlFiles.TabIndex = 4;
            // 
            // tabPageTemplate
            // 
            this.tabPageTemplate.Location = new System.Drawing.Point(4, 25);
            this.tabPageTemplate.Name = "tabPageTemplate";
            this.tabPageTemplate.Padding = new System.Windows.Forms.Padding(10);
            this.tabPageTemplate.Size = new System.Drawing.Size(1148, 758);
            this.tabPageTemplate.TabIndex = 0;
            this.tabPageTemplate.Text = "tabPageTemplate";
            this.tabPageTemplate.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1156, 837);
            this.Controls.Add(this.tabControlFiles);
            this.Controls.Add(this.statusStripMain);
            this.Controls.Add(this.menuStripMain);
            this.MainMenuStrip = this.menuStripMain;
            this.Name = "MainForm";
            this.Text = "TQIT Save File Explorer";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStripMain.ResumeLayout(false);
            this.menuStripMain.PerformLayout();
            this.tabControlFiles.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.StatusStrip statusStripMain;
		private System.Windows.Forms.MenuStrip menuStripMain;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openFileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
		private System.Windows.Forms.OpenFileDialog openFileDialogMain;
		private System.Windows.Forms.TabControl tabControlFiles;
		private System.Windows.Forms.TabPage tabPageTemplate;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialogMain;
	}
}

