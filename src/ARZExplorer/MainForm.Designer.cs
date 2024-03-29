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
        private System.Windows.Forms.MenuStrip menuStrip;

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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.navigationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.previousToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyTXTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyDBRToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyBitmapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copySoundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.searchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.searchNextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findPreviousToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectedFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hideZeroValuesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.capsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.treeViewTOC = new System.Windows.Forms.TreeView();
            this.pictureBoxItem = new System.Windows.Forms.PictureBox();
            this.splitContainerBrowser = new System.Windows.Forms.SplitContainer();
            this.simpleSoundPlayer = new ArzExplorer.Components.SimpleSoundPlayer();
            this.panelPicture = new System.Windows.Forms.Panel();
            this.dataGridViewDetails = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.textBoxDetails = new System.Windows.Forms.TextBox();
            this.labelPath = new System.Windows.Forms.Label();
            this.textBoxPath = new System.Windows.Forms.TextBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tableLayoutPanelSkeleton = new System.Windows.Forms.TableLayoutPanel();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonPrev = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonNext = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonClearHistory = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonCaps = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonLoadDataBase = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabelSearch = new System.Windows.Forms.ToolStripLabel();
            this.toolStripTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButtonSearchPrev = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonSearchNext = new System.Windows.Forms.ToolStripButton();
            this.flowLayoutPanelPath = new System.Windows.Forms.FlowLayoutPanel();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.menuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxItem)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerBrowser)).BeginInit();
            this.splitContainerBrowser.Panel1.SuspendLayout();
            this.splitContainerBrowser.Panel2.SuspendLayout();
            this.splitContainerBrowser.SuspendLayout();
            this.panelPicture.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDetails)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.tableLayoutPanelSkeleton.SuspendLayout();
            this.toolStrip.SuspendLayout();
            this.flowLayoutPanelPath.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.navigationToolStripMenuItem,
            this.editToolStripMenuItem,
            this.searchToolStripMenuItem,
            this.extractToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(826, 24);
            this.menuStrip.TabIndex = 11;
            this.menuStrip.Text = "menuStrip1";
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
            // navigationToolStripMenuItem
            // 
            this.navigationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.previousToolStripMenuItem,
            this.nextToolStripMenuItem});
            this.navigationToolStripMenuItem.Name = "navigationToolStripMenuItem";
            this.navigationToolStripMenuItem.Size = new System.Drawing.Size(77, 20);
            this.navigationToolStripMenuItem.Text = "&Navigation";
            // 
            // previousToolStripMenuItem
            // 
            this.previousToolStripMenuItem.Name = "previousToolStripMenuItem";
            this.previousToolStripMenuItem.ShortcutKeyDisplayString = "";
            this.previousToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.previousToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.previousToolStripMenuItem.Text = "&Previous";
            this.previousToolStripMenuItem.Click += new System.EventHandler(this.previousToolStripMenuItem_Click);
            // 
            // nextToolStripMenuItem
            // 
            this.nextToolStripMenuItem.Name = "nextToolStripMenuItem";
            this.nextToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F2;
            this.nextToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.nextToolStripMenuItem.Text = "&Next";
            this.nextToolStripMenuItem.Click += new System.EventHandler(this.nextToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyPathToolStripMenuItem,
            this.copyTXTToolStripMenuItem,
            this.copyDBRToolStripMenuItem,
            this.copyBitmapToolStripMenuItem,
            this.copySoundToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "&Edit";
            // 
            // copyPathToolStripMenuItem
            // 
            this.copyPathToolStripMenuItem.Name = "copyPathToolStripMenuItem";
            this.copyPathToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.P)));
            this.copyPathToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.copyPathToolStripMenuItem.Text = "Copy &Path";
            this.copyPathToolStripMenuItem.Click += new System.EventHandler(this.copyPathToolStripMenuItem_Click);
            // 
            // copyTXTToolStripMenuItem
            // 
            this.copyTXTToolStripMenuItem.Name = "copyTXTToolStripMenuItem";
            this.copyTXTToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.T)));
            this.copyTXTToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.copyTXTToolStripMenuItem.Text = "Copy .&txt";
            this.copyTXTToolStripMenuItem.Click += new System.EventHandler(this.copyTXTToolStripMenuItem_Click);
            // 
            // copyDBRToolStripMenuItem
            // 
            this.copyDBRToolStripMenuItem.Name = "copyDBRToolStripMenuItem";
            this.copyDBRToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.D)));
            this.copyDBRToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.copyDBRToolStripMenuItem.Text = "Copy .&dbr";
            this.copyDBRToolStripMenuItem.Click += new System.EventHandler(this.copyDBRToolStripMenuItem_Click);
            // 
            // copyBitmapToolStripMenuItem
            // 
            this.copyBitmapToolStripMenuItem.Name = "copyBitmapToolStripMenuItem";
            this.copyBitmapToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.X)));
            this.copyBitmapToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.copyBitmapToolStripMenuItem.Text = "Copy .te&x";
            this.copyBitmapToolStripMenuItem.Click += new System.EventHandler(this.copyBitmapToolStripMenuItem_Click);
            // 
            // copySoundToolStripMenuItem
            // 
            this.copySoundToolStripMenuItem.Name = "copySoundToolStripMenuItem";
            this.copySoundToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.S)));
            this.copySoundToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.copySoundToolStripMenuItem.Text = "Copy Sound";
            this.copySoundToolStripMenuItem.Click += new System.EventHandler(this.copySoundToolStripMenuItem_Click);
            // 
            // searchToolStripMenuItem
            // 
            this.searchToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.searchNextToolStripMenuItem,
            this.findPreviousToolStripMenuItem});
            this.searchToolStripMenuItem.Name = "searchToolStripMenuItem";
            this.searchToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.searchToolStripMenuItem.Text = "&Search";
            // 
            // searchNextToolStripMenuItem
            // 
            this.searchNextToolStripMenuItem.Name = "searchNextToolStripMenuItem";
            this.searchNextToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.searchNextToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.searchNextToolStripMenuItem.Text = "Find &Next";
            this.searchNextToolStripMenuItem.Click += new System.EventHandler(this.searchNextToolStripMenuItem_Click);
            // 
            // findPreviousToolStripMenuItem
            // 
            this.findPreviousToolStripMenuItem.Name = "findPreviousToolStripMenuItem";
            this.findPreviousToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F3)));
            this.findPreviousToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.findPreviousToolStripMenuItem.Text = "Find &Previous";
            this.findPreviousToolStripMenuItem.Click += new System.EventHandler(this.findPreviousToolStripMenuItem_Click);
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
            this.extractToolStripMenuItem.Text = "E&xtract";
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
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.hideZeroValuesToolStripMenuItem,
            this.capsToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "&View";
            // 
            // hideZeroValuesToolStripMenuItem
            // 
            this.hideZeroValuesToolStripMenuItem.Checked = true;
            this.hideZeroValuesToolStripMenuItem.CheckOnClick = true;
            this.hideZeroValuesToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.hideZeroValuesToolStripMenuItem.Enabled = false;
            this.hideZeroValuesToolStripMenuItem.Name = "hideZeroValuesToolStripMenuItem";
            this.hideZeroValuesToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.hideZeroValuesToolStripMenuItem.Text = "Hide &Zero Values";
            this.hideZeroValuesToolStripMenuItem.Click += new System.EventHandler(this.hideZeroValuesToolStripMenuItem_Click);
            // 
            // capsToolStripMenuItem
            // 
            this.capsToolStripMenuItem.CheckOnClick = true;
            this.capsToolStripMenuItem.Name = "capsToolStripMenuItem";
            this.capsToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.capsToolStripMenuItem.Text = "&Caps";
            this.capsToolStripMenuItem.Click += new System.EventHandler(this.capsToolStripMenuItem_Click);
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
            this.treeViewTOC.Size = new System.Drawing.Size(286, 368);
            this.treeViewTOC.TabIndex = 12;
            this.treeViewTOC.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewTOC_AfterSelect);
            // 
            // pictureBoxItem
            // 
            this.pictureBoxItem.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxItem.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxItem.Name = "pictureBoxItem";
            this.pictureBoxItem.Size = new System.Drawing.Size(50, 50);
            this.pictureBoxItem.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBoxItem.TabIndex = 14;
            this.pictureBoxItem.TabStop = false;
            // 
            // splitContainerBrowser
            // 
            this.splitContainerBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerBrowser.Location = new System.Drawing.Point(3, 58);
            this.splitContainerBrowser.Name = "splitContainerBrowser";
            // 
            // splitContainerBrowser.Panel1
            // 
            this.splitContainerBrowser.Panel1.Controls.Add(this.treeViewTOC);
            // 
            // splitContainerBrowser.Panel2
            // 
            this.splitContainerBrowser.Panel2.Controls.Add(this.simpleSoundPlayer);
            this.splitContainerBrowser.Panel2.Controls.Add(this.panelPicture);
            this.splitContainerBrowser.Panel2.Controls.Add(this.dataGridViewDetails);
            this.splitContainerBrowser.Panel2.Controls.Add(this.textBoxDetails);
            this.splitContainerBrowser.Size = new System.Drawing.Size(820, 368);
            this.splitContainerBrowser.SplitterDistance = 286;
            this.splitContainerBrowser.TabIndex = 15;
            // 
            // simpleSoundPlayer
            // 
            this.simpleSoundPlayer.AutoSize = true;
            this.simpleSoundPlayer.CurrentSoundId = null;
            this.simpleSoundPlayer.CurrentSoundPlayer = null;
            this.simpleSoundPlayer.CurrentSoundWavData = null;
            this.simpleSoundPlayer.Dock = System.Windows.Forms.DockStyle.Top;
            this.simpleSoundPlayer.Location = new System.Drawing.Point(53, 0);
            this.simpleSoundPlayer.MinimumSize = new System.Drawing.Size(292, 28);
            this.simpleSoundPlayer.Name = "simpleSoundPlayer";
            this.simpleSoundPlayer.Size = new System.Drawing.Size(292, 28);
            this.simpleSoundPlayer.TabIndex = 17;
            this.simpleSoundPlayer.Visible = false;
            // 
            // panelPicture
            // 
            this.panelPicture.AutoSize = true;
            this.panelPicture.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panelPicture.Controls.Add(this.pictureBoxItem);
            this.panelPicture.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelPicture.Location = new System.Drawing.Point(0, 0);
            this.panelPicture.MinimumSize = new System.Drawing.Size(50, 0);
            this.panelPicture.Name = "panelPicture";
            this.panelPicture.Size = new System.Drawing.Size(53, 237);
            this.panelPicture.TabIndex = 16;
            this.panelPicture.Visible = false;
            // 
            // dataGridViewDetails
            // 
            this.dataGridViewDetails.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridViewDetails.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewDetails.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewDetails.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewDetails.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewDetails.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewDetails.Dock = System.Windows.Forms.DockStyle.Right;
            this.dataGridViewDetails.Location = new System.Drawing.Point(288, 0);
            this.dataGridViewDetails.MultiSelect = false;
            this.dataGridViewDetails.Name = "dataGridViewDetails";
            this.dataGridViewDetails.ReadOnly = true;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewDetails.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridViewDetails.RowHeadersVisible = false;
            this.dataGridViewDetails.Size = new System.Drawing.Size(242, 237);
            this.dataGridViewDetails.TabIndex = 15;
            this.dataGridViewDetails.Visible = false;
            this.dataGridViewDetails.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewDetails_CellClick);
            // 
            // Column1
            // 
            this.Column1.HeaderText = "Variable";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Width = 70;
            // 
            // Column2
            // 
            this.Column2.HeaderText = "Values";
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            this.Column2.Width = 64;
            // 
            // textBoxDetails
            // 
            this.textBoxDetails.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.textBoxDetails.Location = new System.Drawing.Point(0, 237);
            this.textBoxDetails.MinimumSize = new System.Drawing.Size(451, 131);
            this.textBoxDetails.Multiline = true;
            this.textBoxDetails.Name = "textBoxDetails";
            this.textBoxDetails.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.textBoxDetails.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxDetails.Size = new System.Drawing.Size(530, 131);
            this.textBoxDetails.TabIndex = 13;
            this.textBoxDetails.Visible = false;
            this.textBoxDetails.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.textBoxDetails_MouseDoubleClick);
            // 
            // labelPath
            // 
            this.labelPath.AutoSize = true;
            this.labelPath.Location = new System.Drawing.Point(3, 7);
            this.labelPath.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            this.labelPath.Name = "labelPath";
            this.labelPath.Size = new System.Drawing.Size(35, 13);
            this.labelPath.TabIndex = 0;
            this.labelPath.Text = "Path :";
            // 
            // textBoxPath
            // 
            this.textBoxPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPath.Location = new System.Drawing.Point(44, 3);
            this.textBoxPath.Name = "textBoxPath";
            this.textBoxPath.Size = new System.Drawing.Size(737, 20);
            this.textBoxPath.TabIndex = 17;
            this.textBoxPath.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.textBoxPath_MouseDoubleClick);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 453);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(826, 22);
            this.statusStrip.TabIndex = 18;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(112, 17);
            this.toolStripStatusLabel.Text = "toolStripStatusLabel";
            // 
            // tableLayoutPanelSkeleton
            // 
            this.tableLayoutPanelSkeleton.ColumnCount = 1;
            this.tableLayoutPanelSkeleton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelSkeleton.Controls.Add(this.toolStrip, 0, 0);
            this.tableLayoutPanelSkeleton.Controls.Add(this.flowLayoutPanelPath, 0, 1);
            this.tableLayoutPanelSkeleton.Controls.Add(this.splitContainerBrowser, 0, 2);
            this.tableLayoutPanelSkeleton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelSkeleton.Location = new System.Drawing.Point(0, 24);
            this.tableLayoutPanelSkeleton.Name = "tableLayoutPanelSkeleton";
            this.tableLayoutPanelSkeleton.RowCount = 3;
            this.tableLayoutPanelSkeleton.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelSkeleton.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelSkeleton.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelSkeleton.Size = new System.Drawing.Size(826, 429);
            this.tableLayoutPanelSkeleton.TabIndex = 19;
            // 
            // toolStrip
            // 
            this.toolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonPrev,
            this.toolStripButtonNext,
            this.toolStripSeparator3,
            this.toolStripButtonClearHistory,
            this.toolStripSeparator1,
            this.toolStripButtonCaps,
            this.toolStripSeparator5,
            this.toolStripButtonLoadDataBase,
            this.toolStripSeparator2,
            this.toolStripLabelSearch,
            this.toolStripTextBox,
            this.toolStripSeparator4,
            this.toolStripButtonSearchPrev,
            this.toolStripButtonSearchNext});
            this.toolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(445, 23);
            this.toolStrip.TabIndex = 20;
            // 
            // toolStripButtonPrev
            // 
            this.toolStripButtonPrev.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonPrev.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonPrev.Image")));
            this.toolStripButtonPrev.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonPrev.Name = "toolStripButtonPrev";
            this.toolStripButtonPrev.Size = new System.Drawing.Size(23, 20);
            this.toolStripButtonPrev.Text = "Prev";
            this.toolStripButtonPrev.Click += new System.EventHandler(this.toolStripButtonPrev_Click);
            // 
            // toolStripButtonNext
            // 
            this.toolStripButtonNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonNext.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonNext.Image")));
            this.toolStripButtonNext.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonNext.Name = "toolStripButtonNext";
            this.toolStripButtonNext.Size = new System.Drawing.Size(23, 20);
            this.toolStripButtonNext.Text = "Next";
            this.toolStripButtonNext.Click += new System.EventHandler(this.toolStripButtonNext_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 23);
            // 
            // toolStripButtonClearHistory
            // 
            this.toolStripButtonClearHistory.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonClearHistory.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonClearHistory.Image")));
            this.toolStripButtonClearHistory.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonClearHistory.Name = "toolStripButtonClearHistory";
            this.toolStripButtonClearHistory.Size = new System.Drawing.Size(23, 20);
            this.toolStripButtonClearHistory.Text = "Clear History";
            this.toolStripButtonClearHistory.Click += new System.EventHandler(this.toolStripButtonClearHistory_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 23);
            // 
            // toolStripButtonCaps
            // 
            this.toolStripButtonCaps.CheckOnClick = true;
            this.toolStripButtonCaps.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonCaps.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonCaps.Image")));
            this.toolStripButtonCaps.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonCaps.Name = "toolStripButtonCaps";
            this.toolStripButtonCaps.Size = new System.Drawing.Size(23, 20);
            this.toolStripButtonCaps.Text = "Caps";
            this.toolStripButtonCaps.CheckedChanged += new System.EventHandler(this.toolStripButtonCaps_CheckedChanged);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 23);
            // 
            // toolStripButtonLoadDataBase
            // 
            this.toolStripButtonLoadDataBase.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonLoadDataBase.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonLoadDataBase.Image")));
            this.toolStripButtonLoadDataBase.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonLoadDataBase.Name = "toolStripButtonLoadDataBase";
            this.toolStripButtonLoadDataBase.Size = new System.Drawing.Size(23, 20);
            this.toolStripButtonLoadDataBase.Text = "Load Database";
            this.toolStripButtonLoadDataBase.Click += new System.EventHandler(this.toolStripButtonLoadDataBase_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 23);
            // 
            // toolStripLabelSearch
            // 
            this.toolStripLabelSearch.Name = "toolStripLabelSearch";
            this.toolStripLabelSearch.Size = new System.Drawing.Size(51, 15);
            this.toolStripLabelSearch.Text = "Search : ";
            // 
            // toolStripTextBox
            // 
            this.toolStripTextBox.Name = "toolStripTextBox";
            this.toolStripTextBox.Size = new System.Drawing.Size(200, 23);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 23);
            // 
            // toolStripButtonSearchPrev
            // 
            this.toolStripButtonSearchPrev.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonSearchPrev.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonSearchPrev.Image")));
            this.toolStripButtonSearchPrev.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonSearchPrev.Name = "toolStripButtonSearchPrev";
            this.toolStripButtonSearchPrev.Size = new System.Drawing.Size(23, 20);
            this.toolStripButtonSearchPrev.Text = "Search Prev";
            this.toolStripButtonSearchPrev.Click += new System.EventHandler(this.toolStripButtonSearchPrev_Click);
            // 
            // toolStripButtonSearchNext
            // 
            this.toolStripButtonSearchNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonSearchNext.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonSearchNext.Image")));
            this.toolStripButtonSearchNext.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonSearchNext.Name = "toolStripButtonSearchNext";
            this.toolStripButtonSearchNext.Size = new System.Drawing.Size(23, 20);
            this.toolStripButtonSearchNext.Text = "Search Next";
            this.toolStripButtonSearchNext.Click += new System.EventHandler(this.toolStripButtonSearchNext_Click);
            // 
            // flowLayoutPanelPath
            // 
            this.flowLayoutPanelPath.AutoSize = true;
            this.flowLayoutPanelPath.Controls.Add(this.labelPath);
            this.flowLayoutPanelPath.Controls.Add(this.textBoxPath);
            this.flowLayoutPanelPath.Location = new System.Drawing.Point(3, 26);
            this.flowLayoutPanelPath.Name = "flowLayoutPanelPath";
            this.flowLayoutPanelPath.Size = new System.Drawing.Size(784, 26);
            this.flowLayoutPanelPath.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(826, 475);
            this.Controls.Add(this.tableLayoutPanelSkeleton);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.MainMenuStrip = this.menuStrip;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ARZ Explorer";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form1_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Form1_DragEnter);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxItem)).EndInit();
            this.splitContainerBrowser.Panel1.ResumeLayout(false);
            this.splitContainerBrowser.Panel2.ResumeLayout(false);
            this.splitContainerBrowser.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerBrowser)).EndInit();
            this.splitContainerBrowser.ResumeLayout(false);
            this.panelPicture.ResumeLayout(false);
            this.panelPicture.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDetails)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.tableLayoutPanelSkeleton.ResumeLayout(false);
            this.tableLayoutPanelSkeleton.PerformLayout();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.flowLayoutPanelPath.ResumeLayout(false);
            this.flowLayoutPanelPath.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

		#endregion

		private System.Windows.Forms.SplitContainer splitContainerBrowser;
		private System.Windows.Forms.DataGridView dataGridViewDetails;
		private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
		private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
		private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem hideZeroValuesToolStripMenuItem;
		private System.Windows.Forms.Label labelPath;
		private System.Windows.Forms.TextBox textBoxPath;
		private System.Windows.Forms.TextBox textBoxDetails;
		private System.Windows.Forms.Panel panelPicture;
		private System.Windows.Forms.StatusStrip statusStrip;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelSkeleton;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelPath;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton toolStripButtonPrev;
        private System.Windows.Forms.ToolStripButton toolStripButtonNext;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButtonCaps;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel toolStripLabelSearch;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton toolStripButtonClearHistory;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton toolStripButtonLoadDataBase;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
		private System.Windows.Forms.ToolStripMenuItem navigationToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem previousToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem nextToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem copyPathToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem copyTXTToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem copyDBRToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem copyBitmapToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem searchToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem searchNextToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem findPreviousToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem capsToolStripMenuItem;
		private System.Windows.Forms.ToolStripButton toolStripButtonSearchPrev;
		private System.Windows.Forms.ToolStripButton toolStripButtonSearchNext;
		private Components.SimpleSoundPlayer simpleSoundPlayer;
		private System.Windows.Forms.ToolStripMenuItem copySoundToolStripMenuItem;
	}
}

