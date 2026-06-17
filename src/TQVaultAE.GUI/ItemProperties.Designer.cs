//-----------------------------------------------------------------------
// <copyright file="ItemProperties.Designer.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Windows.Forms;
using TQVaultAE.GUI.Components;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI
{
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
		private System.Windows.Forms.Label labelItemName;

		/// <summary>
		/// DataGridView for prefix properties
		/// </summary>
		private DataGridView dataGridViewPrefixProperties;

		/// <summary>
		/// Column for prefix properties
		/// </summary>
		private DataGridViewTextBoxColumn columnPrefixProperty;

		/// <summary>
		/// Label for prefix section
		/// </summary>
		private ScalingLabel labelPrefixProperties;

		/// <summary>
		/// Label for base item section
		/// </summary>
		private ScalingLabel labelBaseItemProperties;

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
		/// Label for suffix section
		/// </summary>
		private ScalingLabel labelSuffixProperties;

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
			this.ButtonOK = new TQVaultAE.GUI.Components.ScalingButton();
			this.labelItemName = new System.Windows.Forms.Label();
			this.checkBoxFilterExtraInfo = new TQVaultAE.GUI.Components.ScalingCheckBox();
			this.labelPrefixProperties = new TQVaultAE.GUI.Components.ScalingLabel();
			this.labelBaseItemProperties = new TQVaultAE.GUI.Components.ScalingLabel();
			this.labelSuffixProperties = new TQVaultAE.GUI.Components.ScalingLabel();
			this.dataGridViewBaseItemProperties = new System.Windows.Forms.DataGridView();
			this.columnBaseItemProperty = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewPrefixProperties = new System.Windows.Forms.DataGridView();
			this.columnPrefixProperty = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridViewSuffixProperties = new System.Windows.Forms.DataGridView();
			this.columnSuffixProperty = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
			this.ButtonOK.Location = new System.Drawing.Point(781, 419);
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
			this.labelItemName.Font = new System.Drawing.Font("Albertus MT Light", 22F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelItemName.Location = new System.Drawing.Point(15, 30);
			this.labelItemName.Margin = new System.Windows.Forms.Padding(3);
			this.labelItemName.MinimumSize = new System.Drawing.Size(23, 22);
			this.labelItemName.Name = "labelItemName";
			this.labelItemName.Size = new System.Drawing.Size(730, 39);
			this.labelItemName.TabIndex = 3;
			this.labelItemName.Text = "Item Fullname";
			// 
			// checkBoxFilterExtraInfo
			// 
			this.checkBoxFilterExtraInfo.AutoSize = true;
			this.checkBoxFilterExtraInfo.BackColor = System.Drawing.Color.Transparent;
			this.checkBoxFilterExtraInfo.Checked = true;
			this.checkBoxFilterExtraInfo.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxFilterExtraInfo.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
			this.checkBoxFilterExtraInfo.Location = new System.Drawing.Point(763, 42);
			this.checkBoxFilterExtraInfo.Name = "checkBoxFilterExtraInfo";
			this.checkBoxFilterExtraInfo.Size = new System.Drawing.Size(126, 21);
			this.checkBoxFilterExtraInfo.TabIndex = 7;
			this.checkBoxFilterExtraInfo.Text = "Filter Extra Info";
			this.checkBoxFilterExtraInfo.UseVisualStyleBackColor = false;
			this.checkBoxFilterExtraInfo.CheckedChanged += new System.EventHandler(this.CheckBox1_CheckedChanged);
			// 
			// labelPrefixProperties
			// 
			this.labelPrefixProperties.AutoSize = true;
			this.labelPrefixProperties.BackColor = System.Drawing.Color.Transparent;
			this.labelPrefixProperties.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
			this.labelPrefixProperties.Location = new System.Drawing.Point(324, 101);
			this.labelPrefixProperties.Name = "labelPrefixProperties";
			this.labelPrefixProperties.Size = new System.Drawing.Size(111, 17);
			this.labelPrefixProperties.TabIndex = 5;
			this.labelPrefixProperties.Text = "Prefix Properties";
			// 
			// labelBaseItemProperties
			// 
			this.labelBaseItemProperties.AutoSize = true;
			this.labelBaseItemProperties.BackColor = System.Drawing.Color.Transparent;
			this.labelBaseItemProperties.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
			this.labelBaseItemProperties.Location = new System.Drawing.Point(12, 101);
			this.labelBaseItemProperties.Name = "labelBaseItemProperties";
			this.labelBaseItemProperties.Size = new System.Drawing.Size(136, 17);
			this.labelBaseItemProperties.TabIndex = 6;
			this.labelBaseItemProperties.Text = "Base Item Properties";
			// 
			// labelSuffixProperties
			// 
			this.labelSuffixProperties.AutoSize = true;
			this.labelSuffixProperties.BackColor = System.Drawing.Color.Transparent;
			this.labelSuffixProperties.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
			this.labelSuffixProperties.Location = new System.Drawing.Point(655, 101);
			this.labelSuffixProperties.Name = "labelSuffixProperties";
			this.labelSuffixProperties.Size = new System.Drawing.Size(111, 17);
			this.labelSuffixProperties.TabIndex = 9;
			this.labelSuffixProperties.Text = "Suffix Properties";
			// 
			// dataGridViewBaseItemProperties
			// 
			this.dataGridViewBaseItemProperties.AllowUserToAddRows = false;
			this.dataGridViewBaseItemProperties.AllowUserToDeleteRows = false;
			this.dataGridViewBaseItemProperties.AllowUserToResizeRows = false;
			this.dataGridViewBaseItemProperties.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
			this.dataGridViewBaseItemProperties.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.dataGridViewBaseItemProperties.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
			this.dataGridViewBaseItemProperties.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			this.dataGridViewBaseItemProperties.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.dataGridViewBaseItemProperties.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
			this.columnBaseItemProperty});
			this.dataGridViewBaseItemProperties.ColumnHeadersVisible = false;
			this.dataGridViewBaseItemProperties.Dock = System.Windows.Forms.DockStyle.None;
			this.dataGridViewBaseItemProperties.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
			this.dataGridViewBaseItemProperties.EnableHeadersVisualStyles = false;
			this.dataGridViewBaseItemProperties.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
			this.dataGridViewBaseItemProperties.Location = new System.Drawing.Point(12, 123);
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
			this.dataGridViewBaseItemProperties.Size = new System.Drawing.Size(292, 269);
			this.dataGridViewBaseItemProperties.TabIndex = 2;
			// 
			// columnBaseItemProperty
			// 
			this.columnBaseItemProperty.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.columnBaseItemProperty.DefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle
			{
				BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21))))),
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
			this.dataGridViewPrefixProperties.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
			this.dataGridViewPrefixProperties.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.dataGridViewPrefixProperties.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
			this.dataGridViewPrefixProperties.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			this.dataGridViewPrefixProperties.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.dataGridViewPrefixProperties.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
			this.columnPrefixProperty});
			this.dataGridViewPrefixProperties.ColumnHeadersVisible = false;
			this.dataGridViewPrefixProperties.Dock = System.Windows.Forms.DockStyle.None;
			this.dataGridViewPrefixProperties.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
			this.dataGridViewPrefixProperties.EnableHeadersVisualStyles = false;
			this.dataGridViewPrefixProperties.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
			this.dataGridViewPrefixProperties.Location = new System.Drawing.Point(327, 123);
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
			this.dataGridViewPrefixProperties.Size = new System.Drawing.Size(292, 269);
			this.dataGridViewPrefixProperties.TabIndex = 4;
			// 
			// columnPrefixProperty
			// 
			this.columnPrefixProperty.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.columnPrefixProperty.DefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle
			{
				BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21))))),
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
			this.dataGridViewSuffixProperties.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
			this.dataGridViewSuffixProperties.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.dataGridViewSuffixProperties.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
			this.dataGridViewSuffixProperties.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
			this.dataGridViewSuffixProperties.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.dataGridViewSuffixProperties.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
			this.columnSuffixProperty});
			this.dataGridViewSuffixProperties.ColumnHeadersVisible = false;
			this.dataGridViewSuffixProperties.Dock = System.Windows.Forms.DockStyle.None;
			this.dataGridViewSuffixProperties.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
			this.dataGridViewSuffixProperties.EnableHeadersVisualStyles = false;
			this.dataGridViewSuffixProperties.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
			this.dataGridViewSuffixProperties.Location = new System.Drawing.Point(655, 123);
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
			this.dataGridViewSuffixProperties.Size = new System.Drawing.Size(262, 269);
			this.dataGridViewSuffixProperties.TabIndex = 8;
			// 
			// columnSuffixProperty
			// 
			this.columnSuffixProperty.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.columnSuffixProperty.DefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle
			{
				BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21))))),
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
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.ClientSize = new System.Drawing.Size(944, 461);
			this.Controls.Add(this.labelSuffixProperties);
			this.Controls.Add(this.dataGridViewSuffixProperties);
			this.Controls.Add(this.checkBoxFilterExtraInfo);
			this.Controls.Add(this.labelBaseItemProperties);
			this.Controls.Add(this.labelPrefixProperties);
			this.Controls.Add(this.dataGridViewPrefixProperties);
			this.Controls.Add(this.labelItemName);
			this.Controls.Add(this.dataGridViewBaseItemProperties);
			this.Controls.Add(this.ButtonOK);
			this.DrawCustomBorder = true;
			this.Font = new System.Drawing.Font("Albertus MT Light", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
			this.Controls.SetChildIndex(this.dataGridViewBaseItemProperties, 0);
			this.Controls.SetChildIndex(this.labelItemName, 0);
			this.Controls.SetChildIndex(this.dataGridViewPrefixProperties, 0);
			this.Controls.SetChildIndex(this.labelPrefixProperties, 0);
			this.Controls.SetChildIndex(this.labelBaseItemProperties, 0);
			this.Controls.SetChildIndex(this.checkBoxFilterExtraInfo, 0);
			this.Controls.SetChildIndex(this.dataGridViewSuffixProperties, 0);
			this.Controls.SetChildIndex(this.labelSuffixProperties, 0);
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewBaseItemProperties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewPrefixProperties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewSuffixProperties)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
