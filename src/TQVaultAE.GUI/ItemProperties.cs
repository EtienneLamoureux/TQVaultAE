//-----------------------------------------------------------------------
// <copyright file="ItemProperties.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI
{
	using System;
	using System.Linq;
	using System.Windows.Forms;
	using TQVaultAE.Entities;
	using TQVaultAE.Presentation;
	using TQVaultAE.Presentation.Html;

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

			this.ok.Font = FontHelper.GetFontAlbertusMTLight(12F);
			this.label1.Font = FontHelper.GetFontAlbertusMTLight(11.25F);
			this.label2.Font = FontHelper.GetFontAlbertusMTLight(11.25F);
			this.checkBox1.Font = FontHelper.GetFontAlbertusMTLight(11.25F);
			this.label3.Font = FontHelper.GetFontAlbertusMTLight(11.25F);
			this.Font = FontHelper.GetFontAlbertusMT(9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

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
		/// Dialog load methond
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void ItemProperties_Load(object sender, EventArgs e)
		{
			this.filterExtra = false;
			this.itemName.DocumentText = ItemHtmlHelper.GetName(this.item);
			this.LoadProperties();
		}

		/// <summary>
		/// Loads the item properties
		/// </summary>
		private void LoadProperties()
		{
			var result = ItemHtmlHelper.LoadProperties(this.item, this.filterExtra);

			// Base Item Attributes
			if (result.BaseItemAttributes.Any())
			{
				this.webBrowser1.DocumentText = result.BaseItemAttributes;
				this.webBrowser1.Show();
				this.label2.Show();
			}
			else
			{
				this.webBrowser1.Hide();
				this.label2.Hide();
			}

			// Prefix Attributes
			if (result.PrefixAttributes.Any())
			{
				this.webBrowser2.DocumentText = result.PrefixAttributes;
				this.webBrowser2.Show();
				this.label1.Show();
			}
			else
			{
				this.webBrowser2.Hide();
				this.label1.Hide();
			}

			// Suffix Attributes
			if (result.SuffixAttributes.Any())
			{
				this.webBrowser3.DocumentText = result.SuffixAttributes;
				this.webBrowser3.Show();
				this.label3.Show();
			}
			else
			{
				this.webBrowser3.Hide();
				this.label3.Hide();
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