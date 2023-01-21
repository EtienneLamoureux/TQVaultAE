namespace TQVaultAE.GUI.Components
{
	partial class ComboBoxCharacter
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
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.duplicateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.duplicateSeparatorToolStripMenuItem = new System.Windows.Forms.ToolStripSeparator();
            this.archiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ArchiveToolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.tagsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addTagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripTextBoxAdd = new System.Windows.Forms.ToolStripTextBox();
            this.AddTagtoolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.tagname1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripTextBoxRename = new System.Windows.Forms.ToolStripTextBox();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.archiveAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unarchiveAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.bufferedTableLayoutPanelSkeleton = new TQVaultAE.GUI.Components.BufferedTableLayoutPanel();
            this.scalingButtonTools = new TQVaultAE.GUI.Components.ScalingButton();
            this.bufferedFlowLayoutPanelContent = new TQVaultAE.GUI.Components.BufferedFlowLayoutPanel();
            this.scalingLabelCharName = new TQVaultAE.GUI.Components.ScalingLabel();
            this.pictureBoxChar = new System.Windows.Forms.PictureBox();
            this.archiveAllToolStripMenuItemGlobal = new System.Windows.Forms.ToolStripMenuItem();
            this.unarchiveAllToolStripMenuItemGlobal = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip.SuspendLayout();
            this.bufferedTableLayoutPanelSkeleton.SuspendLayout();
            this.bufferedFlowLayoutPanelContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxChar)).BeginInit();
            this.SuspendLayout();
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.duplicateToolStripMenuItem,
            this.duplicateSeparatorToolStripMenuItem,
            this.archiveToolStripMenuItem,
            this.archiveAllToolStripMenuItemGlobal,
            this.unarchiveAllToolStripMenuItemGlobal,
            this.ArchiveToolStripSeparator,
            this.tagsToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(181, 148);
            // 
            // duplicateToolStripMenuItem
            // 
            this.duplicateToolStripMenuItem.Name = "duplicateToolStripMenuItem";
            this.duplicateToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.duplicateToolStripMenuItem.Text = "Duplicate";
            this.duplicateToolStripMenuItem.Click += new System.EventHandler(this.duplicateToolStripMenuItem_Click);
            // 
            // duplicateSeparatorToolStripMenuItem
            // 
            this.duplicateSeparatorToolStripMenuItem.Name = "duplicateSeparatorToolStripMenuItem";
            this.duplicateSeparatorToolStripMenuItem.Size = new System.Drawing.Size(177, 6);
            // 
            // archiveToolStripMenuItem
            // 
            this.archiveToolStripMenuItem.Checked = true;
            this.archiveToolStripMenuItem.CheckOnClick = true;
            this.archiveToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.archiveToolStripMenuItem.Name = "archiveToolStripMenuItem";
            this.archiveToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.archiveToolStripMenuItem.Text = "Archived";
            this.archiveToolStripMenuItem.CheckedChanged += new System.EventHandler(this.archiveToolStripMenuItem_CheckedChanged);
            // 
            // ArchiveToolStripSeparator
            // 
            this.ArchiveToolStripSeparator.Name = "ArchiveToolStripSeparator";
            this.ArchiveToolStripSeparator.Size = new System.Drawing.Size(177, 6);
            // 
            // tagsToolStripMenuItem
            // 
            this.tagsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addTagToolStripMenuItem,
            this.AddTagtoolStripSeparator,
            this.tagname1ToolStripMenuItem});
            this.tagsToolStripMenuItem.Name = "tagsToolStripMenuItem";
            this.tagsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.tagsToolStripMenuItem.Text = "Tags";
            // 
            // addTagToolStripMenuItem
            // 
            this.addTagToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripTextBoxAdd});
            this.addTagToolStripMenuItem.Name = "addTagToolStripMenuItem";
            this.addTagToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.addTagToolStripMenuItem.Text = "Add Tag";
            // 
            // toolStripTextBoxAdd
            // 
            this.toolStripTextBoxAdd.MaxLength = 50;
            this.toolStripTextBoxAdd.Name = "toolStripTextBoxAdd";
            this.toolStripTextBoxAdd.Size = new System.Drawing.Size(100, 23);
            this.toolStripTextBoxAdd.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ToolStripTextBoxAdd_KeyDown);
            this.toolStripTextBoxAdd.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.toolStripTextBox_KeyPress);
            // 
            // AddTagtoolStripSeparator
            // 
            this.AddTagtoolStripSeparator.Name = "AddTagtoolStripSeparator";
            this.AddTagtoolStripSeparator.Size = new System.Drawing.Size(124, 6);
            // 
            // tagname1ToolStripMenuItem
            // 
            this.tagname1ToolStripMenuItem.Checked = true;
            this.tagname1ToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tagname1ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.colorToolStripMenuItem,
            this.renameToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.archiveAllToolStripMenuItem,
            this.unarchiveAllToolStripMenuItem});
            this.tagname1ToolStripMenuItem.Name = "tagname1ToolStripMenuItem";
            this.tagname1ToolStripMenuItem.Size = new System.Drawing.Size(127, 22);
            this.tagname1ToolStripMenuItem.Text = "tagname1";
            // 
            // colorToolStripMenuItem
            // 
            this.colorToolStripMenuItem.Name = "colorToolStripMenuItem";
            this.colorToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.colorToolStripMenuItem.Text = "Color";
            // 
            // renameToolStripMenuItem
            // 
            this.renameToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripTextBoxRename});
            this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            this.renameToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.renameToolStripMenuItem.Text = "Rename";
            // 
            // toolStripTextBoxRename
            // 
            this.toolStripTextBoxRename.Name = "toolStripTextBoxRename";
            this.toolStripTextBoxRename.Size = new System.Drawing.Size(100, 23);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            // 
            // archiveAllToolStripMenuItem
            // 
            this.archiveAllToolStripMenuItem.Name = "archiveAllToolStripMenuItem";
            this.archiveAllToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.archiveAllToolStripMenuItem.Text = "Archive all";
            // 
            // unarchiveAllToolStripMenuItem
            // 
            this.unarchiveAllToolStripMenuItem.Name = "unarchiveAllToolStripMenuItem";
            this.unarchiveAllToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.unarchiveAllToolStripMenuItem.Text = "Unarchive all";
            // 
            // colorDialog
            // 
            this.colorDialog.FullOpen = true;
            // 
            // bufferedTableLayoutPanelSkeleton
            // 
            this.bufferedTableLayoutPanelSkeleton.AutoSize = true;
            this.bufferedTableLayoutPanelSkeleton.BackColor = System.Drawing.Color.Transparent;
            this.bufferedTableLayoutPanelSkeleton.ColumnCount = 3;
            this.bufferedTableLayoutPanelSkeleton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.bufferedTableLayoutPanelSkeleton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bufferedTableLayoutPanelSkeleton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.bufferedTableLayoutPanelSkeleton.Controls.Add(this.scalingButtonTools, 2, 0);
            this.bufferedTableLayoutPanelSkeleton.Controls.Add(this.bufferedFlowLayoutPanelContent, 1, 0);
            this.bufferedTableLayoutPanelSkeleton.Controls.Add(this.pictureBoxChar, 0, 0);
            this.bufferedTableLayoutPanelSkeleton.Location = new System.Drawing.Point(0, 0);
            this.bufferedTableLayoutPanelSkeleton.Margin = new System.Windows.Forms.Padding(0);
            this.bufferedTableLayoutPanelSkeleton.Name = "bufferedTableLayoutPanelSkeleton";
            this.bufferedTableLayoutPanelSkeleton.RowCount = 1;
            this.bufferedTableLayoutPanelSkeleton.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.bufferedTableLayoutPanelSkeleton.Size = new System.Drawing.Size(550, 32);
            this.bufferedTableLayoutPanelSkeleton.TabIndex = 0;
            // 
            // scalingButtonTools
            // 
            this.scalingButtonTools.BackColor = System.Drawing.Color.Transparent;
            this.scalingButtonTools.DownBitmap = null;
            this.scalingButtonTools.FlatAppearance.BorderSize = 0;
            this.scalingButtonTools.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonTools.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonTools.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.scalingButtonTools.Font = new System.Drawing.Font("Microsoft Sans Serif", 5.75F);
            this.scalingButtonTools.ForeColor = System.Drawing.Color.Transparent;
            this.scalingButtonTools.Location = new System.Drawing.Point(525, 0);
            this.scalingButtonTools.Margin = new System.Windows.Forms.Padding(0);
            this.scalingButtonTools.Name = "scalingButtonTools";
            this.scalingButtonTools.OverBitmap = null;
            this.scalingButtonTools.Size = new System.Drawing.Size(25, 32);
            this.scalingButtonTools.SizeToGraphic = false;
            this.scalingButtonTools.TabIndex = 1;
            this.scalingButtonTools.UpBitmap = null;
            this.scalingButtonTools.UseCustomGraphic = true;
            this.scalingButtonTools.UseVisualStyleBackColor = false;
            this.scalingButtonTools.Click += new System.EventHandler(this.ScalingButtonTools_Click);
            // 
            // bufferedFlowLayoutPanelContent
            // 
            this.bufferedFlowLayoutPanelContent.AutoSize = true;
            this.bufferedFlowLayoutPanelContent.BackColor = System.Drawing.Color.White;
            this.bufferedFlowLayoutPanelContent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.bufferedFlowLayoutPanelContent.Controls.Add(this.scalingLabelCharName);
            this.bufferedFlowLayoutPanelContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bufferedFlowLayoutPanelContent.Location = new System.Drawing.Point(25, 0);
            this.bufferedFlowLayoutPanelContent.Margin = new System.Windows.Forms.Padding(0);
            this.bufferedFlowLayoutPanelContent.Name = "bufferedFlowLayoutPanelContent";
            this.bufferedFlowLayoutPanelContent.Size = new System.Drawing.Size(500, 32);
            this.bufferedFlowLayoutPanelContent.TabIndex = 2;
            this.bufferedFlowLayoutPanelContent.Click += new System.EventHandler(this.OpenDropDown_Click);
            // 
            // scalingLabelCharName
            // 
            this.scalingLabelCharName.AutoSize = true;
            this.scalingLabelCharName.BackColor = System.Drawing.Color.Transparent;
            this.scalingLabelCharName.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F);
            this.scalingLabelCharName.ForeColor = System.Drawing.Color.Black;
            this.scalingLabelCharName.Location = new System.Drawing.Point(3, 3);
            this.scalingLabelCharName.Margin = new System.Windows.Forms.Padding(3);
            this.scalingLabelCharName.Name = "scalingLabelCharName";
            this.scalingLabelCharName.Size = new System.Drawing.Size(247, 22);
            this.scalingLabelCharName.TabIndex = 0;
            this.scalingLabelCharName.Text = "CharacterName, Lvl : 22 (TQ)";
            this.scalingLabelCharName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.scalingLabelCharName.Click += new System.EventHandler(this.OpenDropDown_Click);
            // 
            // pictureBoxChar
            // 
            this.pictureBoxChar.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxChar.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBoxChar.Name = "pictureBoxChar";
            this.pictureBoxChar.Size = new System.Drawing.Size(25, 32);
            this.pictureBoxChar.TabIndex = 3;
            this.pictureBoxChar.TabStop = false;
            this.pictureBoxChar.Click += new System.EventHandler(this.OpenDropDown_Click);
            // 
            // archiveAllToolStripMenuItemGlobal
            // 
            this.archiveAllToolStripMenuItemGlobal.Name = "archiveAllToolStripMenuItemGlobal";
            this.archiveAllToolStripMenuItemGlobal.Size = new System.Drawing.Size(180, 22);
            this.archiveAllToolStripMenuItemGlobal.Text = "Archive all";
            this.archiveAllToolStripMenuItemGlobal.Click += new System.EventHandler(this.archiveAllToolStripMenuItemGlobal_Click);
            // 
            // unarchiveAllToolStripMenuItemGlobal
            // 
            this.unarchiveAllToolStripMenuItemGlobal.Name = "unarchiveAllToolStripMenuItemGlobal";
            this.unarchiveAllToolStripMenuItemGlobal.Size = new System.Drawing.Size(180, 22);
            this.unarchiveAllToolStripMenuItemGlobal.Text = "Unarchive all";
            this.unarchiveAllToolStripMenuItemGlobal.Click += new System.EventHandler(this.unarchiveAllToolStripMenuItemGlobal_Click);
            // 
            // ComboBoxCharacter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.bufferedTableLayoutPanelSkeleton);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "ComboBoxCharacter";
            this.Size = new System.Drawing.Size(550, 32);
            this.contextMenuStrip.ResumeLayout(false);
            this.bufferedTableLayoutPanelSkeleton.ResumeLayout(false);
            this.bufferedTableLayoutPanelSkeleton.PerformLayout();
            this.bufferedFlowLayoutPanelContent.ResumeLayout(false);
            this.bufferedFlowLayoutPanelContent.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxChar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private BufferedTableLayoutPanel bufferedTableLayoutPanelSkeleton;
		private ScalingButton scalingButtonTools;
		private BufferedFlowLayoutPanel bufferedFlowLayoutPanelContent;
		private ScalingLabel scalingLabelCharName;
        private System.Windows.Forms.PictureBox pictureBoxChar;
		private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem duplicateToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator duplicateSeparatorToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem archiveToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator ArchiveToolStripSeparator;
		private System.Windows.Forms.ToolStripMenuItem tagsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem addTagToolStripMenuItem;
		private System.Windows.Forms.ToolStripTextBox toolStripTextBoxAdd;
		private System.Windows.Forms.ToolStripSeparator AddTagtoolStripSeparator;
		private System.Windows.Forms.ToolStripMenuItem tagname1ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
		private System.Windows.Forms.ToolStripTextBox toolStripTextBoxRename;
		private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
		private System.Windows.Forms.ColorDialog colorDialog;
		private System.Windows.Forms.ToolStripMenuItem colorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem archiveAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem unarchiveAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem archiveAllToolStripMenuItemGlobal;
        private System.Windows.Forms.ToolStripMenuItem unarchiveAllToolStripMenuItemGlobal;
    }
}
