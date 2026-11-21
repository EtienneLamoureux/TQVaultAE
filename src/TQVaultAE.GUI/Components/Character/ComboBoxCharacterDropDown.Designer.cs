using TQVaultAE.GUI.Components.Layout;

namespace TQVaultAE.GUI.Components.Character;

partial class ComboBoxCharacterDropDown
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
        this.bufferedFlowLayoutPanelVertical = new TQVaultAE.GUI.Components.Layout.BufferedFlowLayoutPanel();
        this.SuspendLayout();
        // 
        // bufferedFlowLayoutPanelVertical
        // 
        this.bufferedFlowLayoutPanelVertical.AutoSize = true;
        this.bufferedFlowLayoutPanelVertical.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
        this.bufferedFlowLayoutPanelVertical.Location = new System.Drawing.Point(0, 0);
        this.bufferedFlowLayoutPanelVertical.Margin = new System.Windows.Forms.Padding(0);
        this.bufferedFlowLayoutPanelVertical.Name = "bufferedFlowLayoutPanelVertical";
        this.bufferedFlowLayoutPanelVertical.Size = new System.Drawing.Size(234, 116);
        this.bufferedFlowLayoutPanelVertical.TabIndex = 0;
        // 
        // ComboBoxCharacterDropDown
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.AutoScroll = true;
        this.BackColor = System.Drawing.Color.White;
        this.Controls.Add(this.bufferedFlowLayoutPanelVertical);
        this.ForeColor = System.Drawing.Color.Black;
        this.Margin = new System.Windows.Forms.Padding(0);
        this.Name = "ComboBoxCharacterDropDown";
        this.Size = new System.Drawing.Size(584, 173);
        this.ResumeLayout(false);
        this.PerformLayout();

	}

	#endregion

	private BufferedFlowLayoutPanel bufferedFlowLayoutPanelVertical;
}
