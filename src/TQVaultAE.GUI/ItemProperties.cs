//-----------------------------------------------------------------------
// <copyright file="ItemProperties.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI
{
	using System;
	using System.Linq;
	using TQVaultAE.Data;
	using TQVaultAE.Entities;
	using TQVaultAE.Entities.Results;
	using TQVaultAE.GUI.Tooltip;
	using TQVaultAE.Presentation;

	/// <summary>
	/// Form for the item properties display
	/// </summary>
	internal partial class ItemProperties : VaultForm
	{
		/// <summary>
		/// Item instance of the item we are displaying
		/// </summary>
		public Item Item { get; set; }

		/// <summary>
		/// Item human readable data
		/// </summary>
		private ToFriendlyNameResult Data;

		/// <summary>
		/// Initializes a new instance of the ItemProperties class.
		/// </summary>
		public ItemProperties(MainForm instance)
		{
			this.Owner = instance;
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
		/// Dialog load methond
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void ItemProperties_Load(object sender, EventArgs e)
		{
			this.LoadProperties();
		}

		/// <summary>
		/// Loads the item properties
		/// </summary>
		private void LoadProperties()
		{
			this.Data = ItemProvider.GetFriendlyNames(this.Item, FriendlyNamesExtraScopes.ItemFullDisplay, this.checkBoxFilterExtraInfo.Checked);

			// ItemName
			this.labelItemName.ForeColor = this.Data.Item.GetColor(Data.BaseItemInfoDescription);
			this.labelItemName.Text = this.Data.FullName.RemoveAllTQTags();

			// Base Item Attributes
			if (this.Data.BaseAttributes.Any())
			{
				this.flowLayoutBaseItemProperties.Controls.Clear();
				foreach (var prop in this.Data.BaseAttributes)
					this.flowLayoutBaseItemProperties.Controls.Add(ItemTooltip.MakeRow(prop));
				this.flowLayoutBaseItemProperties.Show();
				this.labelBaseItemProperties.Show();
			}
			else
			{
				this.flowLayoutBaseItemProperties.Hide();
				this.labelBaseItemProperties.Hide();
			}

			// Prefix Attributes
			if (this.Data.PrefixAttributes.Any())
			{
				this.flowLayoutPrefixProperties.Controls.Clear();
				foreach (var prop in this.Data.PrefixAttributes)
					this.flowLayoutPrefixProperties.Controls.Add(ItemTooltip.MakeRow(prop));
				this.flowLayoutPrefixProperties.Show();
				this.labelPrefixProperties.Show();
			}
			else
			{
				this.flowLayoutPrefixProperties.Hide();
				this.labelPrefixProperties.Hide();
			}

			// Suffix Attributes
			if (this.Data.SuffixAttributes.Any())
			{
				this.flowLayoutSuffixProperties.Controls.Clear();
				foreach (var prop in this.Data.SuffixAttributes)
					this.flowLayoutSuffixProperties.Controls.Add(ItemTooltip.MakeRow(prop));
				this.flowLayoutSuffixProperties.Show();
				this.labelSuffixProperties.Show();
			}
			else
			{
				this.flowLayoutSuffixProperties.Hide();
				this.labelSuffixProperties.Hide();
			}
		}

		/// <summary>
		/// Handler for the OK button
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void ButtonOK_Button_Click(object sender, EventArgs e) => this.Close();

		/// <summary>
		/// Handler for clicking the check box
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void CheckBox1_CheckedChanged(object sender, EventArgs e)
		{
			System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
			this.LoadProperties();
			System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
		}
	}
}