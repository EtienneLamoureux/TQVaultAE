//-----------------------------------------------------------------------
// <copyright file="SearchDialog.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI
{
	using System;
	using System.Windows.Forms;
	using TQVaultAE.Presentation;

	/// <summary>
	/// Class for the Search Dialog box.
	/// </summary>
	public partial class SearchDialog : VaultForm
	{
		/// <summary>
		/// Initializes a new instance of the SearchDialog class.
		/// </summary>
		public SearchDialog(MainForm instance) : base(instance.ServiceProvider)
		{
			this.Owner = instance;

			this.InitializeComponent();

			#region Apply custom font

			this.searchLabel.Font = FontService.GetFontAlbertusMT(9F);
			this.searchTextBox.Font = FontService.GetFontAlbertusMT(9F);
			this.findButton.Font = FontService.GetFontAlbertusMTLight(12F);
			this.cancelButton.Font = FontService.GetFontAlbertusMTLight(12F);
			this.Font = FontService.GetFontAlbertusMT(9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte)(0));

			#endregion

			// Load localized strings
			this.Text = Resources.SearchDialogCaption;
			this.searchLabel.Text = Resources.SearchDialogText;
			this.findButton.Text = Resources.MainFormSearchButtonText;
			this.cancelButton.Text = Resources.GlobalCancel;

			this.searchTextBox.Focus();
			this.NormalizeBox = false;
		}

		/// <summary>
		/// Gets the search text from the search text box on the form.
		/// </summary>
		public string SearchText 
			=> this.searchTextBox.Text;

		/// <summary>
		/// Handler for clicking the search button on the form.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void FindButtonClicked(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		/// <summary>
		/// Handler for clicking the cancel button on the form.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void CancelButtonClicked(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		/// <summary>
		/// Handler for showing the search dialog.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void SearchDialogShown(object sender, EventArgs e) 
			=> this.searchTextBox.Focus();
	}
}