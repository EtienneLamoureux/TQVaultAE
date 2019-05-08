//-----------------------------------------------------------------------
// <copyright file="ItemProperties.Designer.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using TQVaultAE.GUI.Properties;

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
		private ScalingButton ok;

		/// <summary>
		/// WebBrowser1 for first attribute set
		/// </summary>
		private System.Windows.Forms.WebBrowser webBrowser1;

		/// <summary>
		/// Item Name for the header
		/// </summary>
		private System.Windows.Forms.WebBrowser itemName;

		/// <summary>
		/// WebBrowser2 for the second attribute set
		/// </summary>
		private System.Windows.Forms.WebBrowser webBrowser2;

		/// <summary>
		/// Label1 control
		/// </summary>
		private ScalingLabel label1;

		/// <summary>
		/// Label2 control
		/// </summary>
		private ScalingLabel label2;

		/// <summary>
		/// Checkbox1 used to turn on and off extended values
		/// </summary>
		private ScalingCheckBox checkBox1;

		/// <summary>
		/// WebBrowser3 for the third attribute set
		/// </summary>
		private System.Windows.Forms.WebBrowser webBrowser3;

		/// <summary>
		/// label3 control
		/// </summary>
		private ScalingLabel label3;

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
            this.ok = new TQVaultAE.GUI.ScalingButton();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.itemName = new System.Windows.Forms.WebBrowser();
            this.webBrowser2 = new System.Windows.Forms.WebBrowser();
            this.label1 = new TQVaultAE.GUI.ScalingLabel();
            this.label2 = new TQVaultAE.GUI.ScalingLabel();
            this.checkBox1 = new TQVaultAE.GUI.ScalingCheckBox();
            this.webBrowser3 = new System.Windows.Forms.WebBrowser();
            this.label3 = new TQVaultAE.GUI.ScalingLabel();
            this.SuspendLayout();
            // 
            // ok
            // 
            this.ok.BackColor = System.Drawing.Color.Transparent;
            this.ok.DownBitmap = global::TQVaultAE.GUI.Properties.Resources.MainButtonDown;
            this.ok.FlatAppearance.BorderSize = 0;
            this.ok.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ok.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ok.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ok.Font = new System.Drawing.Font("Albertus MT Light", 12F);
            this.ok.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.ok.Image = global::TQVaultAE.GUI.Properties.Resources.MainButtonUp;
			this.ok.Location = new System.Drawing.Point(781, 419);
			this.ok.Name = "ok";
            this.ok.OverBitmap = global::TQVaultAE.GUI.Properties.Resources.MainButtonOver;
            this.ok.Size = new System.Drawing.Size(137, 30);
            this.ok.SizeToGraphic = false;
            this.ok.TabIndex = 0;
            this.ok.Text = "OK";
            this.ok.UpBitmap = global::TQVaultAE.GUI.Properties.Resources.MainButtonUp;
            this.ok.UseCustomGraphic = true;
            this.ok.UseVisualStyleBackColor = false;
            this.ok.Click += new System.EventHandler(this.OK_Button_Click);
            // 
            // webBrowser1
            // 
            this.webBrowser1.AllowWebBrowserDrop = false;
            this.webBrowser1.IsWebBrowserContextMenuEnabled = false;
            this.webBrowser1.Location = new System.Drawing.Point(12, 123);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(23, 22);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(292, 269);
            this.webBrowser1.TabIndex = 2;
            this.webBrowser1.TabStop = false;
            this.webBrowser1.WebBrowserShortcutsEnabled = false;
            // 
            // itemName
            // 
            this.itemName.AllowNavigation = false;
            this.itemName.IsWebBrowserContextMenuEnabled = false;
            this.itemName.Location = new System.Drawing.Point(15, 30);
            this.itemName.MinimumSize = new System.Drawing.Size(23, 22);
            this.itemName.Name = "itemName";
            this.itemName.ScrollBarsEnabled = false;
            this.itemName.Size = new System.Drawing.Size(730, 39);
            this.itemName.TabIndex = 3;
            this.itemName.TabStop = false;
            this.itemName.WebBrowserShortcutsEnabled = false;
            this.itemName.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.ItemName_DocumentCompleted);
            // 
            // webBrowser2
            // 
            this.webBrowser2.AllowWebBrowserDrop = false;
            this.webBrowser2.IsWebBrowserContextMenuEnabled = false;
            this.webBrowser2.Location = new System.Drawing.Point(327, 123);
            this.webBrowser2.MinimumSize = new System.Drawing.Size(23, 22);
            this.webBrowser2.Name = "webBrowser2";
            this.webBrowser2.Size = new System.Drawing.Size(292, 269);
            this.webBrowser2.TabIndex = 4;
            this.webBrowser2.TabStop = false;
            this.webBrowser2.WebBrowserShortcutsEnabled = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.label1.Location = new System.Drawing.Point(324, 101);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(111, 17);
            this.label1.TabIndex = 5;
            this.label1.Text = "Prefix Properties";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.label2.Location = new System.Drawing.Point(12, 101);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(136, 17);
            this.label2.TabIndex = 6;
            this.label2.Text = "Base Item Properties";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.BackColor = System.Drawing.Color.Transparent;
            this.checkBox1.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.checkBox1.Location = new System.Drawing.Point(763, 42);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(126, 21);
            this.checkBox1.TabIndex = 7;
            this.checkBox1.Text = "Filter Extra Info";
            this.checkBox1.UseVisualStyleBackColor = false;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.CheckBox1_CheckedChanged);
            // 
            // webBrowser3
            // 
            this.webBrowser3.AllowWebBrowserDrop = false;
            this.webBrowser3.IsWebBrowserContextMenuEnabled = false;
            this.webBrowser3.Location = new System.Drawing.Point(655, 123);
            this.webBrowser3.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser3.Name = "webBrowser3";
            this.webBrowser3.Size = new System.Drawing.Size(262, 269);
            this.webBrowser3.TabIndex = 8;
            this.webBrowser3.TabStop = false;
            this.webBrowser3.WebBrowserShortcutsEnabled = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.label3.Location = new System.Drawing.Point(655, 101);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(111, 17);
            this.label3.TabIndex = 9;
            this.label3.Text = "Suffix Properties";
            // 
            // ItemProperties
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(944, 461);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.webBrowser3);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.webBrowser2);
            this.Controls.Add(this.itemName);
            this.Controls.Add(this.webBrowser1);
            this.Controls.Add(this.ok);
            this.DrawCustomBorder = true;
            this.Font = new System.Drawing.Font("Albertus MT Light", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ItemProperties";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Item Properties";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.ItemProperties_Load);
            this.Controls.SetChildIndex(this.ok, 0);
            this.Controls.SetChildIndex(this.webBrowser1, 0);
            this.Controls.SetChildIndex(this.itemName, 0);
            this.Controls.SetChildIndex(this.webBrowser2, 0);
            this.Controls.SetChildIndex(this.label1, 0);
            this.Controls.SetChildIndex(this.label2, 0);
            this.Controls.SetChildIndex(this.checkBox1, 0);
            this.Controls.SetChildIndex(this.webBrowser3, 0);
            this.Controls.SetChildIndex(this.label3, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
	}
}