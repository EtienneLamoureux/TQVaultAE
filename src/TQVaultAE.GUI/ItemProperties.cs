//-----------------------------------------------------------------------
// <copyright file="ItemProperties.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI
{
	using Properties;
	using System;
	using System.Drawing;
	using System.Globalization;
	using System.Windows.Forms;
	using TQVaultData;

	/// <summary>
	/// Form for the item properties display
	/// </summary>
	internal partial class ItemProperties : VaultForm
	{
		/// <summary>
		/// Item instance of the item we are displaying
		/// </summary>
		private Item item;

		/// <summary>
		/// Flag indicating if we are showing the extra information
		/// </summary>
		private bool filterExtra;

		/// <summary>
		/// Initializes a new instance of the ItemProperties class.
		/// </summary>
		public ItemProperties()
		{
			this.InitializeComponent();

			#region Apply custom font

			this.ok.Font = Program.GetFontAlbertusMTLight(12F);
			this.label1.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.label2.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.checkBox1.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.label3.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.Font = Program.GetFontAlbertusMT(9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

			#endregion

			this.Text = Resources.ItemPropertiesText;
			this.ok.Text = Resources.GlobalOK;
			this.label1.Text = Resources.ItemPropertiesLabel1;
			this.label2.Text = Resources.ItemPropertiesLabel2;
			this.label3.Text = Resources.ItemPropertiesLabel3;
			this.checkBox1.Text = Resources.ItemPropertiesCheckBox1Label;

			this.DrawCustomBorder = true;
		}

		/// <summary>
		/// Sets the item for which we are displaying the properties.
		/// </summary>
		public Item Item
		{
			set
			{
				this.item = value;
			}
		}

		/// <summary>
		/// Gets the name of the item
		/// </summary>
		/// <param name="item">item being displayed</param>
		/// <returns>string with the item name</returns>
		private static string GetName(Item item)
		{
			string itemName = Database.MakeSafeForHtml(item.ToString(true, false));
			string bgcolor = "#2e1f15";

			Color color = item.GetColorTag(itemName);
			itemName = Item.ClipColorTag(itemName);
			itemName = string.Format(CultureInfo.InvariantCulture, "<font size=+1 color={0}><b>{1}</b></font>", Database.HtmlColor(color), itemName);
			return string.Format(CultureInfo.InvariantCulture, "<body bgcolor={0} text=white><font face=\"Albertus MT\" size=2>{1}", bgcolor, itemName);
		}

		/// <summary>
		/// Dialog load methond
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void ItemProperties_Load(object sender, EventArgs e)
		{
			this.filterExtra = false;
			this.itemName.DocumentText = GetName(this.item);
			this.LoadProperties();
		}

		/// <summary>
		/// Loads the item properties
		/// </summary>
		private void LoadProperties()
		{
			string[] bareAttr;
			bareAttr = this.item.GetBareAttributes(this.filterExtra);
			string bgcolor = "#2e1f15";

			// Base Item Attributes
			if (bareAttr[0].Length == 0)
			{
				this.webBrowser1.Hide();
				this.label2.Hide();
			}
			else
			{
				this.webBrowser1.DocumentText = string.Format(CultureInfo.InvariantCulture, "<body bgcolor={0} text=white><font face=\"Albertus MT\" size=1>{1}", bgcolor, bareAttr[0]);
				this.webBrowser1.Show();
				this.label2.Show();
			}

			// Prefix Attributes
			if (bareAttr[2].Length == 0)
			{
				this.webBrowser2.Hide();
				this.label1.Hide();
			}
			else
			{
				this.webBrowser2.DocumentText = string.Format(CultureInfo.InvariantCulture, "<body bgcolor={0} text=white><font face=\"Albertus MT\" size=1>{1}", bgcolor, bareAttr[2]);
				this.webBrowser2.Show();
				this.label1.Show();
			}

			// Suffix Attributes
			if (bareAttr[3].Length == 0)
			{
				this.webBrowser3.Hide();
				this.label3.Hide();
			}
			else
			{
				this.webBrowser3.DocumentText = string.Format(CultureInfo.InvariantCulture, "<body bgcolor={0} text=white><font face=\"Albertus MT\" size=1>{1}", bgcolor, bareAttr[3]);
				this.webBrowser3.Show();
				this.label3.Show();
			}
		}

		/// <summary>
		/// Handler for the OK button
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void OK_Button_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		/// <summary>
		/// Handler for clicking the check box
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void CheckBox1_CheckedChanged(object sender, EventArgs e)
		{
			if (this.checkBox1.Checked)
			{
				if (this.filterExtra == false)
				{
					this.filterExtra = true;
					this.item.RefreshBareAttributes();
					this.LoadProperties();
				}
			}
			else
			{
				if (this.filterExtra == true)
				{
					this.filterExtra = false;
					this.item.RefreshBareAttributes();
					this.LoadProperties();
				}
			}
		}

		/// <summary>
		/// Item name completed handler
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void ItemName_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
		}
	}
}