namespace TQ.SaveFilesExplorer
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
            this.components = new System.ComponentModel.Container();
            this.statusStripMain = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabelMessage = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStripMain = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem_File = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_OpenPlayerDir = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_OpenFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_DetectedPlayers = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Help = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_About = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialogMain = new System.Windows.Forms.OpenFileDialog();
            this.tabControlFiles = new System.Windows.Forms.TabControl();
            this.tabPageTemplate = new System.Windows.Forms.TabPage();
            this.folderBrowserDialogMain = new System.Windows.Forms.FolderBrowserDialog();
            this.contextMenuStripCloseTab = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.closeTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStripMain.SuspendLayout();
            this.menuStripMain.SuspendLayout();
            this.tabControlFiles.SuspendLayout();
            this.contextMenuStripCloseTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStripMain
            // 
            this.statusStripMain.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabelMessage});
            this.statusStripMain.Location = new System.Drawing.Point(0, 815);
            this.statusStripMain.Name = "statusStripMain";
            this.statusStripMain.Padding = new System.Windows.Forms.Padding(1, 0, 13, 0);
            this.statusStripMain.Size = new System.Drawing.Size(1156, 22);
            this.statusStripMain.TabIndex = 0;
            this.statusStripMain.Text = "statusStrip1";
            // 
            // toolStripStatusLabelMessage
            // 
            this.toolStripStatusLabelMessage.Name = "toolStripStatusLabelMessage";
            this.toolStripStatusLabelMessage.Size = new System.Drawing.Size(201, 20);
            this.toolStripStatusLabelMessage.Text = "toolStripStatusLabelMessage";
            this.toolStripStatusLabelMessage.Visible = false;
            // 
            // menuStripMain
            // 
            this.menuStripMain.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_File,
            this.toolStripMenuItem_DetectedPlayers,
            this.toolStripMenuItem_Help});
            this.menuStripMain.Location = new System.Drawing.Point(0, 0);
            this.menuStripMain.Name = "menuStripMain";
            this.menuStripMain.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.menuStripMain.Size = new System.Drawing.Size(1156, 28);
            this.menuStripMain.TabIndex = 1;
            this.menuStripMain.Text = "menuStrip1";
            // 
            // toolStripMenuItem_File
            // 
            this.toolStripMenuItem_File.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_OpenPlayerDir,
            this.toolStripMenuItem_OpenFile});
            this.toolStripMenuItem_File.Name = "toolStripMenuItem_File";
            this.toolStripMenuItem_File.Size = new System.Drawing.Size(44, 24);
            this.toolStripMenuItem_File.Text = "File";
            // 
            // toolStripMenuItem_OpenPlayerDir
            // 
            this.toolStripMenuItem_OpenPlayerDir.Name = "toolStripMenuItem_OpenPlayerDir";
            this.toolStripMenuItem_OpenPlayerDir.Size = new System.Drawing.Size(228, 26);
            this.toolStripMenuItem_OpenPlayerDir.Text = "Open player directory";
            this.toolStripMenuItem_OpenPlayerDir.Click += new System.EventHandler(this.OpenToolStripMenuItem_Click);
            // 
            // toolStripMenuItem_OpenFile
            // 
            this.toolStripMenuItem_OpenFile.Name = "toolStripMenuItem_OpenFile";
            this.toolStripMenuItem_OpenFile.Size = new System.Drawing.Size(228, 26);
            this.toolStripMenuItem_OpenFile.Text = "Open file";
            this.toolStripMenuItem_OpenFile.Click += new System.EventHandler(this.OpenFileToolStripMenuItem_Click);
            // 
            // toolStripMenuItem_DetectedPlayers
            // 
            this.toolStripMenuItem_DetectedPlayers.Name = "toolStripMenuItem_DetectedPlayers";
            this.toolStripMenuItem_DetectedPlayers.Size = new System.Drawing.Size(132, 24);
            this.toolStripMenuItem_DetectedPlayers.Text = "Detected Players";
            // 
            // toolStripMenuItem_Help
            // 
            this.toolStripMenuItem_Help.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_About});
            this.toolStripMenuItem_Help.Name = "toolStripMenuItem_Help";
            this.toolStripMenuItem_Help.Size = new System.Drawing.Size(53, 24);
            this.toolStripMenuItem_Help.Text = "Help";
            // 
            // toolStripMenuItem_About
            // 
            this.toolStripMenuItem_About.Name = "toolStripMenuItem_About";
            this.toolStripMenuItem_About.Size = new System.Drawing.Size(125, 26);
            this.toolStripMenuItem_About.Text = "About";
            this.toolStripMenuItem_About.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
            // 
            // tabControlFiles
            // 
            this.tabControlFiles.ContextMenuStrip = this.contextMenuStripCloseTab;
            this.tabControlFiles.Controls.Add(this.tabPageTemplate);
            this.tabControlFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlFiles.Location = new System.Drawing.Point(0, 28);
            this.tabControlFiles.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabControlFiles.Name = "tabControlFiles";
            this.tabControlFiles.SelectedIndex = 0;
            this.tabControlFiles.Size = new System.Drawing.Size(1156, 787);
            this.tabControlFiles.TabIndex = 4;
            // 
            // tabPageTemplate
            // 
            this.tabPageTemplate.ContextMenuStrip = this.contextMenuStripCloseTab;
            this.tabPageTemplate.Location = new System.Drawing.Point(4, 25);
            this.tabPageTemplate.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabPageTemplate.Name = "tabPageTemplate";
            this.tabPageTemplate.Padding = new System.Windows.Forms.Padding(11, 10, 11, 10);
            this.tabPageTemplate.Size = new System.Drawing.Size(1148, 758);
            this.tabPageTemplate.TabIndex = 0;
            this.tabPageTemplate.Text = "tabPageTemplate";
            this.tabPageTemplate.UseVisualStyleBackColor = true;
            // 
            // contextMenuStripCloseTab
            // 
            this.contextMenuStripCloseTab.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStripCloseTab.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.closeTabToolStripMenuItem});
            this.contextMenuStripCloseTab.Name = "contextMenuStripCloseTab";
            this.contextMenuStripCloseTab.Size = new System.Drawing.Size(142, 28);
            // 
            // closeTabToolStripMenuItem
            // 
            this.closeTabToolStripMenuItem.Name = "closeTabToolStripMenuItem";
            this.closeTabToolStripMenuItem.Size = new System.Drawing.Size(210, 24);
            this.closeTabToolStripMenuItem.Text = "Close Tab";
            this.closeTabToolStripMenuItem.Click += new System.EventHandler(this.CloseTabToolStripMenuItem_Click);
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
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "MainForm";
            this.Text = "Titan Quest Save File Explorer";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.statusStripMain.ResumeLayout(false);
            this.statusStripMain.PerformLayout();
            this.menuStripMain.ResumeLayout(false);
            this.menuStripMain.PerformLayout();
            this.tabControlFiles.ResumeLayout(false);
            this.contextMenuStripCloseTab.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.MenuStrip menuStripMain;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Help;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_About;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_File;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_OpenPlayerDir;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_OpenFile;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_DetectedPlayers;
		private System.Windows.Forms.OpenFileDialog openFileDialogMain;
		private System.Windows.Forms.TabControl tabControlFiles;
		private System.Windows.Forms.TabPage tabPageTemplate;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialogMain;
		internal System.Windows.Forms.StatusStrip statusStripMain;
		internal System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelMessage;
		private System.Windows.Forms.ContextMenuStrip contextMenuStripCloseTab;
		private System.Windows.Forms.ToolStripMenuItem closeTabToolStripMenuItem;
	}
}

