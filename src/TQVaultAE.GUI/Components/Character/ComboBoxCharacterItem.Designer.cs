using TQVaultAE.GUI.Components.Layout;
using TQVaultAE.GUI.Components.Scaling;

namespace TQVaultAE.GUI.Components.Character;

partial class ComboBoxCharacterItem
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
        this.bufferedTableLayoutPanelGrid = new TQVaultAE.GUI.Components.Layout.BufferedTableLayoutPanel();
        this.bufferedFlowLayoutPanelSubTitle = new TQVaultAE.GUI.Components.Layout.BufferedFlowLayoutPanel();
        this.scalingLabelMasteries = new TQVaultAE.GUI.Components.Scaling.ScalingLabel();
        this.bufferedFlowLayoutPanelMod = new TQVaultAE.GUI.Components.Layout.BufferedFlowLayoutPanel();
        this.scalingLabelModded = new TQVaultAE.GUI.Components.Scaling.ScalingLabel();
        this.scalingLabelModName = new TQVaultAE.GUI.Components.Scaling.ScalingLabel();
        this.scalingLabelModState = new TQVaultAE.GUI.Components.Scaling.ScalingLabel();
        this.bufferedFlowLayoutPanelTags = new TQVaultAE.GUI.Components.Layout.BufferedFlowLayoutPanel();
        this.scalingLabelTags = new TQVaultAE.GUI.Components.Scaling.ScalingLabel();
        this.scalingLabelArchived = new TQVaultAE.GUI.Components.Scaling.ScalingLabel();
        this.scalingLabelCharName = new TQVaultAE.GUI.Components.Scaling.ScalingLabel();
        this.scalingLabelCLassName = new TQVaultAE.GUI.Components.Scaling.ScalingLabel();
        this.scalingLabelLevel = new TQVaultAE.GUI.Components.Scaling.ScalingLabel();
        this.scalingLabelPlayerHeader = new TQVaultAE.GUI.Components.Scaling.ScalingLabel();
        this.bufferedTableLayoutPanelGrid.SuspendLayout();
        this.bufferedFlowLayoutPanelSubTitle.SuspendLayout();
        this.bufferedFlowLayoutPanelMod.SuspendLayout();
        this.bufferedFlowLayoutPanelTags.SuspendLayout();
        this.SuspendLayout();
        // 
        // bufferedTableLayoutPanelGrid
        // 
        this.bufferedTableLayoutPanelGrid.AutoSize = true;
        this.bufferedTableLayoutPanelGrid.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.bufferedTableLayoutPanelGrid.ColumnCount = 4;
        this.bufferedTableLayoutPanelGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.bufferedTableLayoutPanelGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.bufferedTableLayoutPanelGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.bufferedTableLayoutPanelGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
        this.bufferedTableLayoutPanelGrid.Controls.Add(this.bufferedFlowLayoutPanelSubTitle, 0, 1);
        this.bufferedTableLayoutPanelGrid.Controls.Add(this.bufferedFlowLayoutPanelMod, 1, 1);
        this.bufferedTableLayoutPanelGrid.Controls.Add(this.bufferedFlowLayoutPanelTags, 0, 2);
        this.bufferedTableLayoutPanelGrid.Controls.Add(this.scalingLabelCharName, 0, 0);
        this.bufferedTableLayoutPanelGrid.Controls.Add(this.scalingLabelCLassName, 1, 0);
        this.bufferedTableLayoutPanelGrid.Controls.Add(this.scalingLabelLevel, 2, 0);
        this.bufferedTableLayoutPanelGrid.Controls.Add(this.scalingLabelPlayerHeader, 3, 0);
        this.bufferedTableLayoutPanelGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        this.bufferedTableLayoutPanelGrid.Location = new System.Drawing.Point(0, 0);
        this.bufferedTableLayoutPanelGrid.Margin = new System.Windows.Forms.Padding(0);
        this.bufferedTableLayoutPanelGrid.Name = "bufferedTableLayoutPanelGrid";
        this.bufferedTableLayoutPanelGrid.Padding = new System.Windows.Forms.Padding(3);
        this.bufferedTableLayoutPanelGrid.RowCount = 3;
        this.bufferedTableLayoutPanelGrid.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.bufferedTableLayoutPanelGrid.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.bufferedTableLayoutPanelGrid.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.bufferedTableLayoutPanelGrid.Size = new System.Drawing.Size(365, 60);
        this.bufferedTableLayoutPanelGrid.TabIndex = 0;
        // 
        // bufferedFlowLayoutPanelSubTitle
        // 
        this.bufferedFlowLayoutPanelSubTitle.AutoSize = true;
        this.bufferedFlowLayoutPanelSubTitle.BackColor = System.Drawing.Color.Transparent;
        this.bufferedFlowLayoutPanelSubTitle.Controls.Add(this.scalingLabelMasteries);
        this.bufferedFlowLayoutPanelSubTitle.Dock = System.Windows.Forms.DockStyle.Fill;
        this.bufferedFlowLayoutPanelSubTitle.Location = new System.Drawing.Point(3, 25);
        this.bufferedFlowLayoutPanelSubTitle.Margin = new System.Windows.Forms.Padding(0);
        this.bufferedFlowLayoutPanelSubTitle.Name = "bufferedFlowLayoutPanelSubTitle";
        this.bufferedFlowLayoutPanelSubTitle.Size = new System.Drawing.Size(221, 16);
        this.bufferedFlowLayoutPanelSubTitle.TabIndex = 4;
        //
        // bufferedFlowLayoutPanelMod
        //
        this.bufferedFlowLayoutPanelMod.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
        this.bufferedFlowLayoutPanelMod.AutoSize = true;
        this.bufferedFlowLayoutPanelMod.BackColor = System.Drawing.Color.Transparent;
        this.bufferedTableLayoutPanelGrid.SetColumnSpan(this.bufferedFlowLayoutPanelMod, 3);
        this.bufferedFlowLayoutPanelMod.Controls.Add(this.scalingLabelModded);
        this.bufferedFlowLayoutPanelMod.Controls.Add(this.scalingLabelModName);
        this.bufferedFlowLayoutPanelMod.Controls.Add(this.scalingLabelModState);
        this.bufferedFlowLayoutPanelMod.Location = new System.Drawing.Point(224, 25);
        this.bufferedFlowLayoutPanelMod.Margin = new System.Windows.Forms.Padding(0);
        this.bufferedFlowLayoutPanelMod.Name = "bufferedFlowLayoutPanelMod";
        this.bufferedFlowLayoutPanelMod.Size = new System.Drawing.Size(138, 16);
        this.bufferedFlowLayoutPanelMod.TabIndex = 6;
        this.bufferedFlowLayoutPanelMod.WrapContents = false;
        //
        // scalingLabelModded
        //
        this.scalingLabelModded.AutoSize = true;
        this.scalingLabelModded.BackColor = System.Drawing.Color.Plum;
        this.scalingLabelModded.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
        this.scalingLabelModded.Location = new System.Drawing.Point(0, 0);
        this.scalingLabelModded.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
        this.scalingLabelModded.Name = "scalingLabelModded";
        this.scalingLabelModded.Size = new System.Drawing.Size(51, 16);
        this.scalingLabelModded.TabIndex = 0;
        this.scalingLabelModded.Text = "Modded";
        // 
        // scalingLabelMasteries
        // 
        this.scalingLabelMasteries.AutoSize = true;
        this.scalingLabelMasteries.BackColor = System.Drawing.Color.Transparent;
        this.scalingLabelMasteries.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
        this.scalingLabelMasteries.Location = new System.Drawing.Point(0, 0);
        this.scalingLabelMasteries.Margin = new System.Windows.Forms.Padding(0);
        this.scalingLabelMasteries.Name = "scalingLabelMasteries";
        this.scalingLabelMasteries.Size = new System.Drawing.Size(171, 16);
        this.scalingLabelMasteries.TabIndex = 5;
        this.scalingLabelMasteries.Text = "Masteries : Hunting - Dream";
        // 
        // scalingLabelModName
        // 
        this.scalingLabelModName.AutoSize = true;
        this.scalingLabelModName.BackColor = System.Drawing.Color.Transparent;
        this.scalingLabelModName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
        this.scalingLabelModName.Location = new System.Drawing.Point(54, 0);
        this.scalingLabelModName.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
        this.scalingLabelModName.Name = "scalingLabelModName";
        this.scalingLabelModName.Size = new System.Drawing.Size(60, 16);
        this.scalingLabelModName.TabIndex = 1;
        this.scalingLabelModName.Text = "Soulvizier";
        //
        // scalingLabelModState
        //
        this.scalingLabelModState.AutoSize = true;
        this.scalingLabelModState.BackColor = System.Drawing.Color.LightGreen;
        this.scalingLabelModState.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
        this.scalingLabelModState.Location = new System.Drawing.Point(117, 0);
        this.scalingLabelModState.Margin = new System.Windows.Forms.Padding(0);
        this.scalingLabelModState.Name = "scalingLabelModState";
        this.scalingLabelModState.Size = new System.Drawing.Size(45, 16);
        this.scalingLabelModState.TabIndex = 2;
        this.scalingLabelModState.Text = "Linked";
        // 
        // bufferedFlowLayoutPanelTags
        // 
        this.bufferedFlowLayoutPanelTags.AutoSize = true;
        this.bufferedFlowLayoutPanelTags.BackColor = System.Drawing.Color.Transparent;
        this.bufferedTableLayoutPanelGrid.SetColumnSpan(this.bufferedFlowLayoutPanelTags, 4);
        this.bufferedFlowLayoutPanelTags.Controls.Add(this.scalingLabelTags);
        this.bufferedFlowLayoutPanelTags.Controls.Add(this.scalingLabelArchived);
        this.bufferedFlowLayoutPanelTags.Dock = System.Windows.Forms.DockStyle.Fill;
        this.bufferedFlowLayoutPanelTags.Location = new System.Drawing.Point(3, 41);
        this.bufferedFlowLayoutPanelTags.Margin = new System.Windows.Forms.Padding(0);
        this.bufferedFlowLayoutPanelTags.Name = "bufferedFlowLayoutPanelTags";
        this.bufferedFlowLayoutPanelTags.Size = new System.Drawing.Size(359, 16);
        this.bufferedFlowLayoutPanelTags.TabIndex = 3;
        // 
        // scalingLabelTags
        // 
        this.scalingLabelTags.AutoSize = true;
        this.scalingLabelTags.BackColor = System.Drawing.Color.Transparent;
        this.scalingLabelTags.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
        this.scalingLabelTags.Location = new System.Drawing.Point(0, 0);
        this.scalingLabelTags.Margin = new System.Windows.Forms.Padding(0);
        this.scalingLabelTags.Name = "scalingLabelTags";
        this.scalingLabelTags.Size = new System.Drawing.Size(48, 16);
        this.scalingLabelTags.TabIndex = 1;
        this.scalingLabelTags.Text = "Tags : ";
        // 
        // scalingLabelArchived
        // 
        this.scalingLabelArchived.AutoSize = true;
        this.scalingLabelArchived.BackColor = System.Drawing.Color.Silver;
        this.scalingLabelArchived.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
        this.scalingLabelArchived.Location = new System.Drawing.Point(51, 0);
        this.scalingLabelArchived.Name = "scalingLabelArchived";
        this.scalingLabelArchived.Size = new System.Drawing.Size(60, 16);
        this.scalingLabelArchived.TabIndex = 5;
        this.scalingLabelArchived.Text = "Archived";
        // 
        // scalingLabelCharName
        // 
        this.scalingLabelCharName.AutoSize = true;
        this.scalingLabelCharName.BackColor = System.Drawing.Color.Transparent;
        this.scalingLabelCharName.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F);
        this.scalingLabelCharName.Location = new System.Drawing.Point(3, 3);
        this.scalingLabelCharName.Margin = new System.Windows.Forms.Padding(0);
        this.scalingLabelCharName.Name = "scalingLabelCharName";
        this.scalingLabelCharName.Size = new System.Drawing.Size(221, 22);
        this.scalingLabelCharName.TabIndex = 2;
        this.scalingLabelCharName.Text = "EarthDefense CharaName";
        // 
        // scalingLabelCLassName
        // 
        this.scalingLabelCLassName.AutoSize = true;
        this.scalingLabelCLassName.BackColor = System.Drawing.Color.Transparent;
        this.scalingLabelCLassName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
        this.scalingLabelCLassName.Location = new System.Drawing.Point(224, 3);
        this.scalingLabelCLassName.Margin = new System.Windows.Forms.Padding(0);
        this.scalingLabelCLassName.Name = "scalingLabelCLassName";
        this.scalingLabelCLassName.Size = new System.Drawing.Size(65, 16);
        this.scalingLabelCLassName.TabIndex = 1;
        this.scalingLabelCLassName.Text = "Haruspex";
        // 
        // scalingLabelLevel
        // 
        this.scalingLabelLevel.AutoSize = true;
        this.scalingLabelLevel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
        this.scalingLabelLevel.Location = new System.Drawing.Point(289, 3);
        this.scalingLabelLevel.Margin = new System.Windows.Forms.Padding(0);
        this.scalingLabelLevel.Name = "scalingLabelLevel";
        this.scalingLabelLevel.Size = new System.Drawing.Size(47, 16);
        this.scalingLabelLevel.TabIndex = 5;
        this.scalingLabelLevel.Text = "Lvl : 22";
        // 
        // scalingLabelPlayerHeader
        // 
        this.scalingLabelPlayerHeader.AutoSize = true;
        this.scalingLabelPlayerHeader.BackColor = System.Drawing.Color.Transparent;
        this.scalingLabelPlayerHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F);
        this.scalingLabelPlayerHeader.Location = new System.Drawing.Point(336, 3);
        this.scalingLabelPlayerHeader.Margin = new System.Windows.Forms.Padding(0);
        this.scalingLabelPlayerHeader.Name = "scalingLabelPlayerHeader";
        this.scalingLabelPlayerHeader.Size = new System.Drawing.Size(26, 16);
        this.scalingLabelPlayerHeader.TabIndex = 0;
        this.scalingLabelPlayerHeader.Text = "TQ";
        // 
        // ComboBoxCharacterItem
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.AutoSize = true;
        this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.BackColor = System.Drawing.Color.White;
        this.Controls.Add(this.bufferedTableLayoutPanelGrid);
        this.ForeColor = System.Drawing.Color.Black;
        this.Margin = new System.Windows.Forms.Padding(0);
        this.Name = "ComboBoxCharacterItem";
        this.Size = new System.Drawing.Size(365, 60);
        this.bufferedTableLayoutPanelGrid.ResumeLayout(false);
        this.bufferedTableLayoutPanelGrid.PerformLayout();
        this.bufferedFlowLayoutPanelSubTitle.ResumeLayout(false);
        this.bufferedFlowLayoutPanelSubTitle.PerformLayout();
        this.bufferedFlowLayoutPanelMod.ResumeLayout(false);
        this.bufferedFlowLayoutPanelMod.PerformLayout();
        this.bufferedFlowLayoutPanelTags.ResumeLayout(false);
        this.bufferedFlowLayoutPanelTags.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();

	}

	#endregion

	private BufferedTableLayoutPanel bufferedTableLayoutPanelGrid;
	private ScalingLabel scalingLabelCLassName;
	private ScalingLabel scalingLabelPlayerHeader;
	private ScalingLabel scalingLabelCharName;
	private BufferedFlowLayoutPanel bufferedFlowLayoutPanelTags;
	private ScalingLabel scalingLabelTags;
    private ScalingLabel scalingLabelModName;
    private ScalingLabel scalingLabelArchived;
    private BufferedFlowLayoutPanel bufferedFlowLayoutPanelSubTitle;
    private BufferedFlowLayoutPanel bufferedFlowLayoutPanelMod;
    private ScalingLabel scalingLabelModded;
    private ScalingLabel scalingLabelModState;
    private ScalingLabel scalingLabelLevel;
    private ScalingLabel scalingLabelMasteries;
}
