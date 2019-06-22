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
		private System.Windows.Forms.WebBrowser webBrowserBaseItemProperties;

		/// <summary>
		/// Item Name for the header
		/// </summary>
		private System.Windows.Forms.WebBrowser webBrowserItemName;

		/// <summary>
		/// WebBrowser2 for the second attribute set
		/// </summary>
		private System.Windows.Forms.WebBrowser webBrowserPrefixProperties;

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
		private System.Windows.Forms.WebBrowser webBrowserSuffixProperties;

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
            this.webBrowserBaseItemProperties = new System.Windows.Forms.WebBrowser();
            this.webBrowserItemName = new System.Windows.Forms.WebBrowser();
            this.webBrowserPrefixProperties = new System.Windows.Forms.WebBrowser();
            this.labelPrefixProperties = new TQVaultAE.GUI.Components.ScalingLabel();
            this.labelBaseItemProperties = new TQVaultAE.GUI.Components.ScalingLabel();
            this.checkBoxFilterExtraInfo = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.webBrowserSuffixProperties = new System.Windows.Forms.WebBrowser();
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
            this.ButtonOK.Font = new System.Drawing.Font("Albertus MT Light", 15F);
            this.ButtonOK.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ButtonOK.Image = ((System.Drawing.Image)(resources.GetObject("ButtonOK.Image")));
            this.ButtonOK.Location = new System.Drawing.Point(976, 524);
            this.ButtonOK.Margin = new System.Windows.Forms.Padding(4);
            this.ButtonOK.Name = "ButtonOK";
            this.ButtonOK.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ButtonOK.OverBitmap")));
            this.ButtonOK.Size = new System.Drawing.Size(171, 38);
            this.ButtonOK.SizeToGraphic = false;
            this.ButtonOK.TabIndex = 0;
            this.ButtonOK.Text = "OK";
            this.ButtonOK.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("ButtonOK.UpBitmap")));
            this.ButtonOK.UseCustomGraphic = true;
            this.ButtonOK.UseVisualStyleBackColor = false;
            this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Button_Click);
            // 
            // webBrowserBaseItemProperties
            // 
            this.webBrowserBaseItemProperties.AllowWebBrowserDrop = false;
            this.webBrowserBaseItemProperties.IsWebBrowserContextMenuEnabled = false;
            this.webBrowserBaseItemProperties.Location = new System.Drawing.Point(15, 154);
            this.webBrowserBaseItemProperties.Margin = new System.Windows.Forms.Padding(4);
            this.webBrowserBaseItemProperties.MinimumSize = new System.Drawing.Size(29, 28);
            this.webBrowserBaseItemProperties.Name = "webBrowserBaseItemProperties";
            this.webBrowserBaseItemProperties.Size = new System.Drawing.Size(365, 336);
            this.webBrowserBaseItemProperties.TabIndex = 2;
            this.webBrowserBaseItemProperties.TabStop = false;
            this.webBrowserBaseItemProperties.WebBrowserShortcutsEnabled = false;
            // 
            // webBrowserItemName
            // 
            this.webBrowserItemName.AllowNavigation = false;
            this.webBrowserItemName.IsWebBrowserContextMenuEnabled = false;
            this.webBrowserItemName.Location = new System.Drawing.Point(19, 38);
            this.webBrowserItemName.Margin = new System.Windows.Forms.Padding(4);
            this.webBrowserItemName.MinimumSize = new System.Drawing.Size(29, 28);
            this.webBrowserItemName.Name = "webBrowserItemName";
            this.webBrowserItemName.ScrollBarsEnabled = false;
            this.webBrowserItemName.Size = new System.Drawing.Size(912, 49);
            this.webBrowserItemName.TabIndex = 3;
            this.webBrowserItemName.TabStop = false;
            this.webBrowserItemName.WebBrowserShortcutsEnabled = false;
            // 
            // webBrowserPrefixProperties
            // 
            this.webBrowserPrefixProperties.AllowWebBrowserDrop = false;
            this.webBrowserPrefixProperties.IsWebBrowserContextMenuEnabled = false;
            this.webBrowserPrefixProperties.Location = new System.Drawing.Point(409, 154);
            this.webBrowserPrefixProperties.Margin = new System.Windows.Forms.Padding(4);
            this.webBrowserPrefixProperties.MinimumSize = new System.Drawing.Size(29, 28);
            this.webBrowserPrefixProperties.Name = "webBrowserPrefixProperties";
            this.webBrowserPrefixProperties.Size = new System.Drawing.Size(365, 336);
            this.webBrowserPrefixProperties.TabIndex = 4;
            this.webBrowserPrefixProperties.TabStop = false;
            this.webBrowserPrefixProperties.WebBrowserShortcutsEnabled = false;
            // 
            // labelPrefixProperties
            // 
            this.labelPrefixProperties.AutoSize = true;
            this.labelPrefixProperties.BackColor = System.Drawing.Color.Transparent;
            this.labelPrefixProperties.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.labelPrefixProperties.Location = new System.Drawing.Point(405, 126);
            this.labelPrefixProperties.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelPrefixProperties.Name = "labelPrefixProperties";
            this.labelPrefixProperties.Size = new System.Drawing.Size(182, 28);
            this.labelPrefixProperties.TabIndex = 5;
            this.labelPrefixProperties.Text = "Prefix Properties";
            // 
            // labelBaseItemProperties
            // 
            this.labelBaseItemProperties.AutoSize = true;
            this.labelBaseItemProperties.BackColor = System.Drawing.Color.Transparent;
            this.labelBaseItemProperties.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.labelBaseItemProperties.Location = new System.Drawing.Point(15, 126);
            this.labelBaseItemProperties.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelBaseItemProperties.Name = "labelBaseItemProperties";
            this.labelBaseItemProperties.Size = new System.Drawing.Size(218, 28);
            this.labelBaseItemProperties.TabIndex = 6;
            this.labelBaseItemProperties.Text = "Base Item Properties";
            // 
            // checkBoxFilterExtraInfo
            // 
            this.checkBoxFilterExtraInfo.AutoSize = true;
            this.checkBoxFilterExtraInfo.BackColor = System.Drawing.Color.Transparent;
            this.checkBoxFilterExtraInfo.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.checkBoxFilterExtraInfo.Location = new System.Drawing.Point(954, 52);
            this.checkBoxFilterExtraInfo.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxFilterExtraInfo.Name = "checkBoxFilterExtraInfo";
            this.checkBoxFilterExtraInfo.Size = new System.Drawing.Size(197, 32);
            this.checkBoxFilterExtraInfo.TabIndex = 7;
            this.checkBoxFilterExtraInfo.Text = "Filter Extra Info";
            this.checkBoxFilterExtraInfo.UseVisualStyleBackColor = false;
            this.checkBoxFilterExtraInfo.CheckedChanged += new System.EventHandler(this.CheckBox1_CheckedChanged);
            // 
            // webBrowserSuffixProperties
            // 
            this.webBrowserSuffixProperties.AllowWebBrowserDrop = false;
            this.webBrowserSuffixProperties.IsWebBrowserContextMenuEnabled = false;
            this.webBrowserSuffixProperties.Location = new System.Drawing.Point(819, 154);
            this.webBrowserSuffixProperties.Margin = new System.Windows.Forms.Padding(4);
            this.webBrowserSuffixProperties.MinimumSize = new System.Drawing.Size(25, 25);
            this.webBrowserSuffixProperties.Name = "webBrowserSuffixProperties";
            this.webBrowserSuffixProperties.Size = new System.Drawing.Size(328, 336);
            this.webBrowserSuffixProperties.TabIndex = 8;
            this.webBrowserSuffixProperties.TabStop = false;
            this.webBrowserSuffixProperties.WebBrowserShortcutsEnabled = false;
            // 
            // labelSuffixProperties
            // 
            this.labelSuffixProperties.AutoSize = true;
            this.labelSuffixProperties.BackColor = System.Drawing.Color.Transparent;
            this.labelSuffixProperties.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.labelSuffixProperties.Location = new System.Drawing.Point(819, 126);
            this.labelSuffixProperties.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelSuffixProperties.Name = "labelSuffixProperties";
            this.labelSuffixProperties.Size = new System.Drawing.Size(181, 28);
            this.labelSuffixProperties.TabIndex = 9;
            this.labelSuffixProperties.Text = "Suffix Properties";
            // 
            // ItemProperties
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1180, 576);
            this.Controls.Add(this.labelSuffixProperties);
            this.Controls.Add(this.webBrowserSuffixProperties);
            this.Controls.Add(this.checkBoxFilterExtraInfo);
            this.Controls.Add(this.labelBaseItemProperties);
            this.Controls.Add(this.labelPrefixProperties);
            this.Controls.Add(this.webBrowserPrefixProperties);
            this.Controls.Add(this.webBrowserItemName);
            this.Controls.Add(this.webBrowserBaseItemProperties);
            this.Controls.Add(this.ButtonOK);
            this.DrawCustomBorder = true;
            this.Font = new System.Drawing.Font("Albertus MT Light", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(5);
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
            this.Controls.SetChildIndex(this.webBrowserBaseItemProperties, 0);
            this.Controls.SetChildIndex(this.webBrowserItemName, 0);
            this.Controls.SetChildIndex(this.webBrowserPrefixProperties, 0);
            this.Controls.SetChildIndex(this.labelPrefixProperties, 0);
            this.Controls.SetChildIndex(this.labelBaseItemProperties, 0);
            this.Controls.SetChildIndex(this.checkBoxFilterExtraInfo, 0);
            this.Controls.SetChildIndex(this.webBrowserSuffixProperties, 0);
            this.Controls.SetChildIndex(this.labelSuffixProperties, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
	}
}