//-----------------------------------------------------------------------
// <copyright file="ItemProperties.Designer.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Windows.Forms;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Components;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Dialogs;

/// <summary>
/// ItemProperties form designer class
/// </summary>
internal partial class ItemProperties
{
	/// <summary>
	/// OK Button control
	/// </summary>
	private ScalingButton ButtonOK;

	/// <summary>
	/// DataGridView for base item properties
	/// </summary>
	private DataGridView dataGridViewBaseItemProperties;

	/// <summary>
	/// Column for base item properties
	/// </summary>
	private DataGridViewTextBoxColumn columnBaseItemProperty;

	/// <summary>
	/// Item Name for the header
	/// </summary>
	private ScalingLabel labelItemName;

	/// <summary>
	/// DataGridView for prefix properties
	/// </summary>
	private DataGridView dataGridViewPrefixProperties;

	/// <summary>
	/// Column for prefix properties
	/// </summary>
	private DataGridViewTextBoxColumn columnPrefixProperty;

	/// <summary>
	/// Group of the prefix section, its caption the section's title
	/// </summary>
	private GroupBox groupBoxPrefixProperties;

	/// <summary>
	/// Group of the base item section, its caption the section's title
	/// </summary>
	private GroupBox groupBoxBaseItemProperties;

	/// <summary>
	/// Checkbox for filtering extra info
	/// </summary>
	private ScalingCheckBox checkBoxFilterExtraInfo;

	/// <summary>
	/// DataGridView for suffix properties
	/// </summary>
	private DataGridView dataGridViewSuffixProperties;

	/// <summary>
	/// Column for suffix properties
	/// </summary>
	private DataGridViewTextBoxColumn columnSuffixProperty;

	/// <summary>
	/// Group of the suffix section, its caption the section's title
	/// </summary>
	private GroupBox groupBoxSuffixProperties;

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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ItemProperties));
		this.ButtonOK = new TQVaultAE.GUI.Components.Scaling.ScalingButton();
		this.labelItemName = new TQVaultAE.GUI.Components.Scaling.ScalingLabel();
		this.checkBoxFilterExtraInfo = new TQVaultAE.GUI.Components.Scaling.ScalingCheckBox();
		this.groupBoxPrefixProperties = new System.Windows.Forms.GroupBox();
		this.groupBoxBaseItemProperties = new System.Windows.Forms.GroupBox();
		this.groupBoxSuffixProperties = new System.Windows.Forms.GroupBox();
		this.dataGridViewBaseItemProperties = new System.Windows.Forms.DataGridView();
		this.columnBaseItemProperty = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewPrefixProperties = new System.Windows.Forms.DataGridView();
		this.columnPrefixProperty = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dataGridViewSuffixProperties = new System.Windows.Forms.DataGridView();
		this.columnSuffixProperty = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.groupBoxBaseItemProperties.SuspendLayout();
		this.groupBoxPrefixProperties.SuspendLayout();
		this.groupBoxSuffixProperties.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)(this.dataGridViewBaseItemProperties)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.dataGridViewPrefixProperties)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.dataGridViewSuffixProperties)).BeginInit();
		this.SuspendLayout();
		//
		// ButtonOK
		//
		this.ButtonOK.BackColor = System.Drawing.Color.Transparent;
		this.ButtonOK.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ButtonOK.DownBitmap")));
		this.ButtonOK.FlatAppearance.BorderSize = 0;
		this.ButtonOK.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
		this.ButtonOK.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
		this.ButtonOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.ButtonOK.Font = new System.Drawing.Font("Albertus MT Light", 12F);
		this.ButtonOK.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
		this.ButtonOK.Image = ((System.Drawing.Image)(resources.GetObject("ButtonOK.Image")));
		this.ButtonOK.Location = new System.Drawing.Point(851, 418);
		this.ButtonOK.Name = "ButtonOK";
		this.ButtonOK.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ButtonOK.OverBitmap")));
		this.ButtonOK.Size = new System.Drawing.Size(137, 30);
		this.ButtonOK.SizeToGraphic = false;
		this.ButtonOK.TabIndex = 0;
		this.ButtonOK.Text = "OK";
		this.ButtonOK.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ButtonOK.UpBitmap")));
		this.ButtonOK.UseCustomGraphic = true;
		this.ButtonOK.UseVisualStyleBackColor = false;
		this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Button_Click);
		//
		// labelItemName
		//
		this.labelItemName.AutoEllipsis = true;
		this.labelItemName.Font = new System.Drawing.Font("Albertus MT Light", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
		this.labelItemName.Location = new System.Drawing.Point(12, 34);
		this.labelItemName.Name = "labelItemName";
		this.labelItemName.Size = new System.Drawing.Size(830, 46);
		this.labelItemName.TabIndex = 3;
		this.labelItemName.Text = "Item Fullname";
		this.labelItemName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.labelItemName.UseMnemonic = false;
		//
		// checkBoxFilterExtraInfo
		//
		this.checkBoxFilterExtraInfo.AutoSize = true;
		this.checkBoxFilterExtraInfo.BackColor = System.Drawing.Color.Transparent;
		this.checkBoxFilterExtraInfo.Checked = true;
		this.checkBoxFilterExtraInfo.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkBoxFilterExtraInfo.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
		this.checkBoxFilterExtraInfo.Location = new System.Drawing.Point(862, 47);
		this.checkBoxFilterExtraInfo.Name = "checkBoxFilterExtraInfo";
		this.checkBoxFilterExtraInfo.Size = new System.Drawing.Size(126, 21);
		this.checkBoxFilterExtraInfo.TabIndex = 7;
		this.checkBoxFilterExtraInfo.Text = "Filter Extra Info";
		this.checkBoxFilterExtraInfo.UseVisualStyleBackColor = false;
		this.checkBoxFilterExtraInfo.CheckedChanged += new System.EventHandler(this.CheckBox1_CheckedChanged);
		//
		// groupBoxBaseItemProperties
		//
		this.groupBoxBaseItemProperties.Controls.Add(this.dataGridViewBaseItemProperties);
		this.groupBoxBaseItemProperties.ForeColor = System.Drawing.Color.Gold;
		this.groupBoxBaseItemProperties.Location = new System.Drawing.Point(12, 90);
		this.groupBoxBaseItemProperties.Name = "groupBoxBaseItemProperties";
		this.groupBoxBaseItemProperties.Size = new System.Drawing.Size(317, 316);
		this.groupBoxBaseItemProperties.TabIndex = 6;
		this.groupBoxBaseItemProperties.TabStop = false;
		this.groupBoxBaseItemProperties.Text = "Base Item Properties";
		//
		// groupBoxPrefixProperties
		//
		this.groupBoxPrefixProperties.Controls.Add(this.dataGridViewPrefixProperties);
		this.groupBoxPrefixProperties.ForeColor = System.Drawing.Color.Gold;
		this.groupBoxPrefixProperties.Location = new System.Drawing.Point(341, 90);
		this.groupBoxPrefixProperties.Name = "groupBoxPrefixProperties";
		this.groupBoxPrefixProperties.Size = new System.Drawing.Size(317, 316);
		this.groupBoxPrefixProperties.TabIndex = 5;
		this.groupBoxPrefixProperties.TabStop = false;
		this.groupBoxPrefixProperties.Text = "Prefix Properties";
		//
		// groupBoxSuffixProperties
		//
		this.groupBoxSuffixProperties.Controls.Add(this.dataGridViewSuffixProperties);
		this.groupBoxSuffixProperties.ForeColor = System.Drawing.Color.Gold;
		this.groupBoxSuffixProperties.Location = new System.Drawing.Point(670, 90);
		this.groupBoxSuffixProperties.Name = "groupBoxSuffixProperties";
		this.groupBoxSuffixProperties.Size = new System.Drawing.Size(318, 316);
		this.groupBoxSuffixProperties.TabIndex = 9;
		this.groupBoxSuffixProperties.TabStop = false;
		this.groupBoxSuffixProperties.Text = "Suffix Properties";
		//
		// dataGridViewBaseItemProperties
		//
		this.dataGridViewBaseItemProperties.AllowUserToAddRows = false;
		this.dataGridViewBaseItemProperties.AllowUserToDeleteRows = false;
		this.dataGridViewBaseItemProperties.AllowUserToResizeRows = false;
		this.dataGridViewBaseItemProperties.BackgroundColor = System.Drawing.Color.Black;
		this.dataGridViewBaseItemProperties.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.dataGridViewBaseItemProperties.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
		this.dataGridViewBaseItemProperties.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
		this.dataGridViewBaseItemProperties.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
		this.dataGridViewBaseItemProperties.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
		this.columnBaseItemProperty});
		this.dataGridViewBaseItemProperties.ColumnHeadersVisible = false;
		this.dataGridViewBaseItemProperties.Dock = System.Windows.Forms.DockStyle.Fill;
		this.dataGridViewBaseItemProperties.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
		this.dataGridViewBaseItemProperties.EnableHeadersVisualStyles = false;
		this.dataGridViewBaseItemProperties.GridColor = System.Drawing.Color.Black;
		this.dataGridViewBaseItemProperties.MultiSelect = true;
		this.dataGridViewBaseItemProperties.Name = "dataGridViewBaseItemProperties";
		this.dataGridViewBaseItemProperties.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
		this.dataGridViewBaseItemProperties.RowHeadersVisible = false;
		this.dataGridViewBaseItemProperties.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
		this.dataGridViewBaseItemProperties.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
		this.dataGridViewBaseItemProperties.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.dataGridViewBaseItemProperties.ShowCellErrors = false;
		this.dataGridViewBaseItemProperties.ShowCellToolTips = false;
		this.dataGridViewBaseItemProperties.ShowEditingIcon = false;
		this.dataGridViewBaseItemProperties.ShowRowErrors = false;
		this.dataGridViewBaseItemProperties.TabIndex = 2;
		//
		// columnBaseItemProperty
		//
		this.columnBaseItemProperty.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.columnBaseItemProperty.DefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle
		{
			BackColor = System.Drawing.Color.Black,
			ForeColor = System.Drawing.Color.White,
			SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(45)))), ((int)(((byte)(30))))),
			SelectionForeColor = System.Drawing.Color.White
		};
		this.columnBaseItemProperty.FillWeight = 100F;
		this.columnBaseItemProperty.HeaderText = "Property";
		this.columnBaseItemProperty.Name = "columnBaseItemProperty";
		this.columnBaseItemProperty.ReadOnly = true;
		this.columnBaseItemProperty.Resizable = System.Windows.Forms.DataGridViewTriState.False;
		this.columnBaseItemProperty.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
		//
		// dataGridViewPrefixProperties
		//
		this.dataGridViewPrefixProperties.AllowUserToAddRows = false;
		this.dataGridViewPrefixProperties.AllowUserToDeleteRows = false;
		this.dataGridViewPrefixProperties.AllowUserToResizeRows = false;
		this.dataGridViewPrefixProperties.BackgroundColor = System.Drawing.Color.Black;
		this.dataGridViewPrefixProperties.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.dataGridViewPrefixProperties.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
		this.dataGridViewPrefixProperties.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
		this.dataGridViewPrefixProperties.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
		this.dataGridViewPrefixProperties.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
		this.columnPrefixProperty});
		this.dataGridViewPrefixProperties.ColumnHeadersVisible = false;
		this.dataGridViewPrefixProperties.Dock = System.Windows.Forms.DockStyle.Fill;
		this.dataGridViewPrefixProperties.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
		this.dataGridViewPrefixProperties.EnableHeadersVisualStyles = false;
		this.dataGridViewPrefixProperties.GridColor = System.Drawing.Color.Black;
		this.dataGridViewPrefixProperties.MultiSelect = true;
		this.dataGridViewPrefixProperties.Name = "dataGridViewPrefixProperties";
		this.dataGridViewPrefixProperties.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
		this.dataGridViewPrefixProperties.RowHeadersVisible = false;
		this.dataGridViewPrefixProperties.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
		this.dataGridViewPrefixProperties.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
		this.dataGridViewPrefixProperties.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.dataGridViewPrefixProperties.ShowCellErrors = false;
		this.dataGridViewPrefixProperties.ShowCellToolTips = false;
		this.dataGridViewPrefixProperties.ShowEditingIcon = false;
		this.dataGridViewPrefixProperties.ShowRowErrors = false;
		this.dataGridViewPrefixProperties.TabIndex = 4;
		//
		// columnPrefixProperty
		//
		this.columnPrefixProperty.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.columnPrefixProperty.DefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle
		{
			BackColor = System.Drawing.Color.Black,
			ForeColor = System.Drawing.Color.White,
			SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(45)))), ((int)(((byte)(30))))),
			SelectionForeColor = System.Drawing.Color.White
		};
		this.columnPrefixProperty.FillWeight = 100F;
		this.columnPrefixProperty.HeaderText = "Property";
		this.columnPrefixProperty.Name = "columnPrefixProperty";
		this.columnPrefixProperty.ReadOnly = true;
		this.columnPrefixProperty.Resizable = System.Windows.Forms.DataGridViewTriState.False;
		this.columnPrefixProperty.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
		//
		// dataGridViewSuffixProperties
		//
		this.dataGridViewSuffixProperties.AllowUserToAddRows = false;
		this.dataGridViewSuffixProperties.AllowUserToDeleteRows = false;
		this.dataGridViewSuffixProperties.AllowUserToResizeRows = false;
		this.dataGridViewSuffixProperties.BackgroundColor = System.Drawing.Color.Black;
		this.dataGridViewSuffixProperties.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.dataGridViewSuffixProperties.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
		this.dataGridViewSuffixProperties.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
		this.dataGridViewSuffixProperties.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
		this.dataGridViewSuffixProperties.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
		this.columnSuffixProperty});
		this.dataGridViewSuffixProperties.ColumnHeadersVisible = false;
		this.dataGridViewSuffixProperties.Dock = System.Windows.Forms.DockStyle.Fill;
		this.dataGridViewSuffixProperties.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
		this.dataGridViewSuffixProperties.EnableHeadersVisualStyles = false;
		this.dataGridViewSuffixProperties.GridColor = System.Drawing.Color.Black;
		this.dataGridViewSuffixProperties.MultiSelect = true;
		this.dataGridViewSuffixProperties.Name = "dataGridViewSuffixProperties";
		this.dataGridViewSuffixProperties.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
		this.dataGridViewSuffixProperties.RowHeadersVisible = false;
		this.dataGridViewSuffixProperties.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
		this.dataGridViewSuffixProperties.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
		this.dataGridViewSuffixProperties.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.dataGridViewSuffixProperties.ShowCellErrors = false;
		this.dataGridViewSuffixProperties.ShowCellToolTips = false;
		this.dataGridViewSuffixProperties.ShowEditingIcon = false;
		this.dataGridViewSuffixProperties.ShowRowErrors = false;
		this.dataGridViewSuffixProperties.TabIndex = 8;
		//
		// columnSuffixProperty
		//
		this.columnSuffixProperty.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.columnSuffixProperty.DefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle
		{
			BackColor = System.Drawing.Color.Black,
			ForeColor = System.Drawing.Color.White,
			SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(45)))), ((int)(((byte)(30))))),
			SelectionForeColor = System.Drawing.Color.White
		};
		this.columnSuffixProperty.FillWeight = 100F;
		this.columnSuffixProperty.HeaderText = "Property";
		this.columnSuffixProperty.Name = "columnSuffixProperty";
		this.columnSuffixProperty.ReadOnly = true;
		this.columnSuffixProperty.Resizable = System.Windows.Forms.DataGridViewTriState.False;
		this.columnSuffixProperty.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
		//
		// ItemProperties
		//
		this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
		this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
		this.ClientSize = new System.Drawing.Size(1000, 460);
		this.Controls.Add(this.groupBoxSuffixProperties);
		this.Controls.Add(this.checkBoxFilterExtraInfo);
		this.Controls.Add(this.groupBoxBaseItemProperties);
		this.Controls.Add(this.groupBoxPrefixProperties);
		this.Controls.Add(this.labelItemName);
		this.Controls.Add(this.ButtonOK);
		this.DrawCustomBorder = true;
		this.Font = new System.Drawing.Font("Albertus MT Light", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
		this.ForeColor = System.Drawing.Color.White;
		this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		this.Margin = new System.Windows.Forms.Padding(4);
		this.MaximizeBox = false;
		this.MinimizeBox = false;
		this.Name = "ItemProperties";
		this.ShowInTaskbar = false;
		this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
		this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Item Properties";
		this.TopMost = true;
		this.Load += new System.EventHandler(this.ItemProperties_Load);
		this.Controls.SetChildIndex(this.ButtonOK, 0);
		this.Controls.SetChildIndex(this.labelItemName, 0);
		this.Controls.SetChildIndex(this.groupBoxPrefixProperties, 0);
		this.Controls.SetChildIndex(this.groupBoxBaseItemProperties, 0);
		this.Controls.SetChildIndex(this.checkBoxFilterExtraInfo, 0);
		this.Controls.SetChildIndex(this.groupBoxSuffixProperties, 0);
		this.groupBoxBaseItemProperties.ResumeLayout(false);
		this.groupBoxPrefixProperties.ResumeLayout(false);
		this.groupBoxSuffixProperties.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)(this.dataGridViewBaseItemProperties)).EndInit();
		((System.ComponentModel.ISupportInitialize)(this.dataGridViewPrefixProperties)).EndInit();
		((System.ComponentModel.ISupportInitialize)(this.dataGridViewSuffixProperties)).EndInit();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion
}
