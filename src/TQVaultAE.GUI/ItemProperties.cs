//-----------------------------------------------------------------------
// <copyright file="ItemProperties.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI
{
	using System;
	using System.Linq;
	using TQVaultAE.Entities;
	using TQVaultAE.Entities.Results;
	using TQVaultAE.Presentation;
	using TQVaultAE.Presentation.Html;
	using TQVaultAE.Services;

	/// <summary>
	/// Form for the item properties display
	/// </summary>
	internal partial class ItemProperties : VaultForm
	{
		/// <summary>
		/// Item instance of the item we are displaying
		/// </summary>
		private Item _item;

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

			this.ButtonOK.Font = FontHelper.GetFontAlbertusMTLight(12F);
			this.labelPrefixProperties.Font = FontHelper.GetFontAlbertusMTLight(11.25F);
			this.labelBaseItemProperties.Font = FontHelper.GetFontAlbertusMTLight(11.25F);
			this.checkBoxFilterExtraInfo.Font = FontHelper.GetFontAlbertusMTLight(11.25F);
			this.labelSuffixProperties.Font = FontHelper.GetFontAlbertusMTLight(11.25F);
			this.Font = FontHelper.GetFontAlbertusMT(9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

			#endregion

			this.Text = Resources.ItemPropertiesText;
			this.ButtonOK.Text = Resources.GlobalOK;
			this.labelPrefixProperties.Text = Resources.ItemPropertiesLabelPrefixProperties;
			this.labelBaseItemProperties.Text = Resources.ItemPropertiesLabelBaseItemProperties;
			this.labelSuffixProperties.Text = Resources.ItemPropertiesLabelSuffixProperties;
			this.checkBoxFilterExtraInfo.Text = Resources.ItemPropertiesCheckBoxLabelFilterExtraInfo;

			this.DrawCustomBorder = true;
		}

		/// <summary>
		/// Sets the item for which we are displaying the properties.
		/// </summary>
		public Item Item
		{
			set
			{
				this._item = value;
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
			this.webBrowserItemName.DocumentText = ItemHtmlHelper.GetName(this._item).RemoveAllTQTags();
			this.LoadProperties();
		}

		/// <summary>
		/// Loads the item properties
		/// </summary>
		private void LoadProperties()
		{
			var res = ItemHtmlHelper.LoadProperties(this._item, this.filterExtra);
			// Base Item Attributes
			if (res.BaseItemAttributes.Any())
			{
				this.webBrowserBaseItemProperties.DocumentText = res.BaseItemAttributes;
				this.webBrowserBaseItemProperties.Show();
				this.labelBaseItemProperties.Show();
			}
			else
			{
				this.webBrowserBaseItemProperties.Hide();
				this.labelBaseItemProperties.Hide();
			}

			// Prefix Attributes
			if (res.PrefixAttributes.Any())
			{
				this.webBrowserPrefixProperties.DocumentText = res.PrefixAttributes;
				this.webBrowserPrefixProperties.Show();
				this.labelPrefixProperties.Show();
			}
			else
			{
				this.webBrowserPrefixProperties.Hide();
				this.labelPrefixProperties.Hide();
			}

			// Suffix Attributes
			if (res.PrefixAttributes.Any())
			{
				this.webBrowserSuffixProperties.DocumentText = res.PrefixAttributes;
				this.webBrowserSuffixProperties.Show();
				this.labelSuffixProperties.Show();
			}
			else
			{
				this.webBrowserSuffixProperties.Hide();
				this.labelSuffixProperties.Hide();
			}
		}

		/// <summary>
		/// Handler for the OK button
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void ButtonOK_Button_Click(object sender, EventArgs e)
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
			if (this.filterExtra != this.checkBoxFilterExtraInfo.Checked)
			{
				this.filterExtra = this.checkBoxFilterExtraInfo.Checked;
				this.LoadProperties();
			}
		}
	}
}