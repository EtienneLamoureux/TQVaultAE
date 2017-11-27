﻿//-----------------------------------------------------------------------
// <copyright file="SearchDialog.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI
{
	using Properties;
	using System;
	using System.Drawing;
	using System.Windows.Forms;

	/// <summary>
	/// Class for the Search Dialog box.
	/// </summary>
	public partial class SearchDialog : VaultForm
	{
		/// <summary>
		/// Initializes a new instance of the SearchDialog class.
		/// </summary>
		public SearchDialog()
		{
			this.InitializeComponent();

			// Load localized strings
			this.Text = Resources.SearchDialogCaption;
			this.searchLabel.Text = Resources.SearchDialogText;
			this.findButton.Text = Resources.MainFormSearchButtonText;
			this.cancelButton.Text = Resources.GlobalCancel;

			this.searchTextBox.Focus();
		}

		/// <summary>
		/// Gets the search text from the search text box on the form.
		/// </summary>
		public string SearchText
		{
			get
			{
				return this.searchTextBox.Text;
			}
		}

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
		{
			this.searchTextBox.Focus();
		}
	}
}