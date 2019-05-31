namespace TQVaultAE.GUI
{
	using Properties;
	using System;
	using System.Drawing;
	using System.Globalization;
	using System.Windows.Forms;
	using TQVaultData;

	/// <summary>
	/// Dialog box class for the Item Seed Dialog
	/// </summary>
	internal partial class CharacterEditDialog : VaultForm
	{
		/// <summary>
		/// MessageBoxOptions for right to left reading.
		/// </summary>
		private static MessageBoxOptions rightToLeftOptions = (MessageBoxOptions)0;

		/// <summary>
		/// Selected Item
		/// </summary>
		private Item selectedItem;

		/// <summary>
		/// Initializes a new instance of the ItemSeedDialog class.
		/// </summary>
		public CharacterEditDialog()
		{
			this.InitializeComponent();

			#region Apply custom font

			this.ok.Font = Program.GetFontAlbertusMTLight(12F);
			this.cancel.Font = Program.GetFontAlbertusMTLight(12F);
			this.Font = Program.GetFontAlbertusMTLight(11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

			#endregion

			this.DrawCustomBorder = true;

			this.cancel.Text = Resources.GlobalCancel;
			this.ok.Text = Resources.GlobalOK;

			// Set options for Right to Left reading.
			if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
			{
				rightToLeftOptions = MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;
			}
		}

		/// <summary>
		/// Gets or sets the selected item
		/// </summary>
		public Item SelectedItem
		{
			get
			{
				return this.selectedItem;
			}

			set
			{
				this.selectedItem = value;
			}
		}


		/// <summary>
		/// Cancel button handler
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void CancelButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		/// <summary>
		/// OK button handler
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void OKButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		/// <summary>
		/// Dialog load handler
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void ItemSeedDlg_Load(object sender, EventArgs e)
		{
		}
	}
}