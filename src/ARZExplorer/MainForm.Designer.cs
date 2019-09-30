//-----------------------------------------------------------------------
// <copyright file="Form1.Designer.cs" company="VillageIdiot">
//     Copyright (c) Village Idiot. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace ArzExplorer
{
    /// <summary>
    /// Class for Form1 Designer Generated code.
    /// </summary>
    public partial class MainForm
    {
        /// <summary>
        /// Generated menu strip
        /// </summary>
        private System.Windows.Forms.MenuStrip menuStrip1;

        /// <summary>
        /// Generated File menu item
        /// </summary>
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;

        /// <summary>
        /// Generated Open menu item
        /// </summary>
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;

        /// <summary>
        /// Generated Exit menu item
        /// </summary>
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;

        /// <summary>
        /// Generated Extract menu item
        /// </summary>
        private System.Windows.Forms.ToolStripMenuItem extractToolStripMenuItem;

        /// <summary>
        /// Generated Help menu item
        /// </summary>
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;

        /// <summary>
        /// Generated About menu item
        /// </summary>
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;

        /// <summary>
        /// Generated Selected File menu item
        /// </summary>
        private System.Windows.Forms.ToolStripMenuItem selectedFileToolStripMenuItem;

        /// <summary>
        /// Generated All Files menu item
        /// </summary>
        private System.Windows.Forms.ToolStripMenuItem allFilesToolStripMenuItem;

        /// <summary>
        /// Generated TreeView
        /// </summary>
        private System.Windows.Forms.TreeView treeViewTOC;

        /// <summary>
        /// Generated TextBox
        /// </summary>
        private System.Windows.Forms.TextBox textBoxDetails;

        /// <summary>
        /// Generated PictureBox
        /// </summary>
        private System.Windows.Forms.PictureBox pictureBoxItem;

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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectedFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.treeViewTOC = new System.Windows.Forms.TreeView();
            this.textBoxDetails = new System.Windows.Forms.TextBox();
            this.pictureBoxItem = new System.Windows.Forms.PictureBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxItem)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.extractToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(803, 24);
            this.menuStrip1.TabIndex = 11;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.BackColor = System.Drawing.SystemColors.Control;
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fileToolStripMenuItem.ForeColor = System.Drawing.SystemColors.ControlText;
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.OpenToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // extractToolStripMenuItem
            // 
            this.extractToolStripMenuItem.BackColor = System.Drawing.SystemColors.Control;
            this.extractToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectedFileToolStripMenuItem,
            this.allFilesToolStripMenuItem});
            this.extractToolStripMenuItem.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.extractToolStripMenuItem.ForeColor = System.Drawing.SystemColors.ControlText;
            this.extractToolStripMenuItem.Name = "extractToolStripMenuItem";
            this.extractToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.extractToolStripMenuItem.Text = "&Extract";
            // 
            // selectedFileToolStripMenuItem
            // 
            this.selectedFileToolStripMenuItem.Name = "selectedFileToolStripMenuItem";
            this.selectedFileToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.selectedFileToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.selectedFileToolStripMenuItem.Text = "&Selected File";
            this.selectedFileToolStripMenuItem.Click += new System.EventHandler(this.SelectedFileToolStripMenuItem_Click);
            // 
            // allFilesToolStripMenuItem
            // 
            this.allFilesToolStripMenuItem.Name = "allFilesToolStripMenuItem";
            this.allFilesToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.allFilesToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.allFilesToolStripMenuItem.Text = "&All Files";
            this.allFilesToolStripMenuItem.Click += new System.EventHandler(this.AllFilesToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.BackColor = System.Drawing.SystemColors.Control;
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.helpToolStripMenuItem.ForeColor = System.Drawing.SystemColors.ControlText;
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(41, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(102, 22);
            this.aboutToolStripMenuItem.Text = "&About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
            // 
            // treeViewTOC
            // 
            this.treeViewTOC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewTOC.Location = new System.Drawing.Point(0, 0);
            this.treeViewTOC.Name = "treeViewTOC";
            this.treeViewTOC.Size = new System.Drawing.Size(369, 453);
            this.treeViewTOC.TabIndex = 12;
            this.treeViewTOC.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeView1_AfterSelect);
            // 
            // textBoxDetails
            // 
            this.textBoxDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxDetails.Location = new System.Drawing.Point(50, 0);
            this.textBoxDetails.Multiline = true;
            this.textBoxDetails.Name = "textBoxDetails";
            this.textBoxDetails.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.textBoxDetails.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxDetails.Size = new System.Drawing.Size(380, 453);
            this.textBoxDetails.TabIndex = 13;
            // 
            // pictureBoxItem
            // 
            this.pictureBoxItem.Dock = System.Windows.Forms.DockStyle.Left;
            this.pictureBoxItem.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxItem.MinimumSize = new System.Drawing.Size(50, 0);
            this.pictureBoxItem.Name = "pictureBoxItem";
            this.pictureBoxItem.Size = new System.Drawing.Size(50, 453);
            this.pictureBoxItem.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxItem.TabIndex = 14;
            this.pictureBoxItem.TabStop = false;
            this.pictureBoxItem.Visible = false;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeViewTOC);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.textBoxDetails);
            this.splitContainer1.Panel2.Controls.Add(this.pictureBoxItem);
            this.splitContainer1.Size = new System.Drawing.Size(803, 453);
            this.splitContainer1.SplitterDistance = 369;
            this.splitContainer1.TabIndex = 15;
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(803, 477);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ARZ Explorer";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form1_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Form1_DragEnter);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxItem)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer1;
	}
}

