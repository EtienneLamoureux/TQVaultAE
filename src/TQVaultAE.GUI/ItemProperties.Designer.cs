//-----------------------------------------------------------------------
// <copyright file="ItemProperties.Designer.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
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
		/// WebBrowser1 for first attribute set
		/// </summary>
		private System.Windows.Forms.FlowLayoutPanel flowLayoutBaseItemProperties;

		/// <summary>
		/// Item Name for the header
		/// </summary>
		private System.Windows.Forms.Label labelItemName;

		/// <summary>
		/// WebBrowser2 for the second attribute set
		/// </summary>
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPrefixProperties;

		/// <summary>
		/// Label1 control
		/// </summary>
		private ScalingLabel labelPrefixProperties;

		/// <summary>
		/// Label2 control
		/// </summary>
		private ScalingLabel labelBaseItemProperties;

		/// <summary>
		/// Checkbox1 used to turn on and off extended values
		/// </summary>
		private ScalingCheckBox checkBoxFilterExtraInfo;

		/// <summary>
		/// WebBrowser3 for the third attribute set
		/// </summary>
		private System.Windows.Forms.FlowLayoutPanel flowLayoutSuffixProperties;

		/// <summary>
		/// label3 control
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
			this.flowLayoutBaseItemProperties = new System.Windows.Forms.FlowLayoutPanel();
			this.labelItemName = new System.Windows.Forms.Label();
			this.flowLayoutPrefixProperties = new System.Windows.Forms.FlowLayoutPanel();
			this.labelPrefixProperties = new TQVaultAE.GUI.Components.ScalingLabel();
			this.labelBaseItemProperties = new TQVaultAE.GUI.Components.ScalingLabel();
			this.checkBoxFilterExtraInfo = new TQVaultAE.GUI.Components.ScalingCheckBox();
			this.flowLayoutSuffixProperties = new System.Windows.Forms.FlowLayoutPanel();
			this.labelSuffixProperties = new TQVaultAE.GUI.Components.ScalingLabel();
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
			// flowLayoutBaseItemProperties
			// 
			this.flowLayoutBaseItemProperties.AutoScroll = true;
			this.flowLayoutBaseItemProperties.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.flowLayoutBaseItemProperties.Location = new System.Drawing.Point(12, 123);
			this.flowLayoutBaseItemProperties.MinimumSize = new System.Drawing.Size(23, 22);
			this.flowLayoutBaseItemProperties.Name = "flowLayoutBaseItemProperties";
			this.flowLayoutBaseItemProperties.Size = new System.Drawing.Size(292, 269);
			this.flowLayoutBaseItemProperties.TabIndex = 2;
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
			// flowLayoutPrefixProperties
			// 
			this.flowLayoutPrefixProperties.AutoScroll = true;
			this.flowLayoutPrefixProperties.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.flowLayoutPrefixProperties.Location = new System.Drawing.Point(327, 123);
			this.flowLayoutPrefixProperties.MinimumSize = new System.Drawing.Size(23, 22);
			this.flowLayoutPrefixProperties.Name = "flowLayoutPrefixProperties";
			this.flowLayoutPrefixProperties.Size = new System.Drawing.Size(292, 269);
			this.flowLayoutPrefixProperties.TabIndex = 4;
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
			// flowLayoutSuffixProperties
			// 
			this.flowLayoutSuffixProperties.AutoScroll = true;
			this.flowLayoutSuffixProperties.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.flowLayoutSuffixProperties.Location = new System.Drawing.Point(655, 123);
			this.flowLayoutSuffixProperties.MinimumSize = new System.Drawing.Size(20, 20);
			this.flowLayoutSuffixProperties.Name = "flowLayoutSuffixProperties";
			this.flowLayoutSuffixProperties.Size = new System.Drawing.Size(262, 269);
			this.flowLayoutSuffixProperties.TabIndex = 8;
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
			// ItemProperties
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.ClientSize = new System.Drawing.Size(944, 461);
			this.Controls.Add(this.labelSuffixProperties);
			this.Controls.Add(this.flowLayoutSuffixProperties);
			this.Controls.Add(this.checkBoxFilterExtraInfo);
			this.Controls.Add(this.labelBaseItemProperties);
			this.Controls.Add(this.labelPrefixProperties);
			this.Controls.Add(this.flowLayoutPrefixProperties);
			this.Controls.Add(this.labelItemName);
			this.Controls.Add(this.flowLayoutBaseItemProperties);
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
			this.Controls.SetChildIndex(this.flowLayoutBaseItemProperties, 0);
			this.Controls.SetChildIndex(this.labelItemName, 0);
			this.Controls.SetChildIndex(this.flowLayoutPrefixProperties, 0);
			this.Controls.SetChildIndex(this.labelPrefixProperties, 0);
			this.Controls.SetChildIndex(this.labelBaseItemProperties, 0);
			this.Controls.SetChildIndex(this.checkBoxFilterExtraInfo, 0);
			this.Controls.SetChildIndex(this.flowLayoutSuffixProperties, 0);
			this.Controls.SetChildIndex(this.labelSuffixProperties, 0);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}