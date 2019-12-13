//-----------------------------------------------------------------------
// <copyright file="ResultsDialog.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using TQVaultAE.GUI.Tooltip;
using TQVaultAE.GUI.Models;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Domain.Search;

namespace TQVaultAE.GUI
{
	/// <summary>
	/// Results dialog form class
	/// </summary>
	public partial class ResultsDialog : VaultForm
	{
		/// <summary>
		/// List of all results
		/// </summary>
		private List<Result> resultsList;

		/// <summary>
		/// User selected result from the list
		/// </summary>
		private Result selectedResult;

		/// <summary>
		/// Search string passed from user
		/// </summary>
		private string searchString;

		/// <summary>
		/// Initializes a new instance of the ResultsDialog class.
		/// </summary>
		public ResultsDialog(MainForm instance) : base(instance.ServiceProvider)
		{
			this.Owner = instance;

			this.InitializeComponent();

			#region Apply custom font

			this.resultsDataGridView.ColumnHeadersDefaultCellStyle.Font = FontService.GetFontAlbertusMT(9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.item.DefaultCellStyle.Font = FontService.GetFontAlbertusMT(9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.quality.DefaultCellStyle.Font = FontService.GetFontAlbertusMT(9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.containerName.DefaultCellStyle.Font = FontService.GetFontAlbertusMT(9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.containerType.DefaultCellStyle.Font = FontService.GetFontAlbertusMT(9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.level.DefaultCellStyle.Font = FontService.GetFontAlbertusMT(9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Font = FontService.GetFontAlbertusMT(9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

			#endregion

			this.resultsList = new List<Result>();
			////this.selectedResult = new Result();
			this.item.HeaderText = Resources.ResultsItem;
			this.containerName.HeaderText = Resources.ResultsContainer;
			this.containerType.HeaderText = Resources.ResultsContainerType;
			this.quality.HeaderText = Resources.ResultsQuality;
			this.level.HeaderText = Resources.ResultsLevel;

			this.NormalizeBox = false;
			this.DrawCustomBorder = true;

			this.FormDesignRatio = 0.0F; //// (float)this.Height / (float)this.Width;
										 ////this.FormMaximumSize = new Size(this.Width * 2, this.Height * 2);
										 ////this.FormMinimumSize = new Size(
										 ////Convert.ToInt32((float)this.Width * 0.4F),
										 ////Convert.ToInt32((float)this.Height * 0.4F));
			this.OriginalFormSize = this.Size;
			this.OriginalFormScale = 1.0F;
			this.LastFormSize = this.Size;
		}

		/// <summary>
		/// Event Handler for the ResultChanged event.
		/// </summary>
		/// <typeparam name="ResultChangedEventArgs">ResultChangedEventArgs type</typeparam>
		/// <param name="sender">sender object</param>
		/// <param name="e">ResultChangedEventArgs data</param>
		public delegate void EventHandler<ResultChangedEventArgs>(object sender, ResultChangedEventArgs e);

		/// <summary>
		/// Event for changing to a different highlighted result.
		/// </summary>
		public event EventHandler<ResultChangedEventArgs> ResultChanged;

		/// <summary>
		/// Gets the list of results collection
		/// </summary>
		public List<Result> ResultsList => this.resultsList;

		/// <summary>
		/// Sets the user search string
		/// </summary>
		public string SearchString
		{
			set => this.searchString = value;
		}

		/// <summary>
		/// Gets the string name for the corresponding SackType
		/// </summary>
		/// <param name="containterType">SackType which we are looking up</param>
		/// <returns>string containing the sack type</returns>
		private static string GetContainerTypeString(SackType containterType)
		{
			switch (containterType)
			{
				case SackType.Vault:
					return Resources.ResultsContainerVault;

				case SackType.Player:
					return Resources.ResultsContainerPlayer;

				case SackType.Equipment:
					return Resources.ResultsContainerEquip;

				case SackType.Stash:
					return Resources.ResultsContainerStash;

				case SackType.TransferStash:
					return Resources.GlobalTransferStash;

				case SackType.RelicVaultStash:
					return Resources.GlobalRelicVaultStash;

				default:
					return "Unknown";
			}
		}

		/// <summary>
		/// Handler for showing the results dialog.
		/// Populates the data grid view.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void ResultsDialogShown(object sender, EventArgs e)
		{
			this.PopulateDataGridView();

			int totalWidth = 0;
			foreach (DataGridViewColumn column in this.resultsDataGridView.Columns)
				totalWidth += column.Width;

			totalWidth += this.resultsDataGridView.Margin.Horizontal;

			int totalHeight = 0;
			foreach (DataGridViewRow row in this.resultsDataGridView.Rows)
				totalHeight += row.Height;

			totalHeight += this.Padding.Vertical;

			if (totalHeight > this.Height)
				totalWidth += SystemInformation.VerticalScrollBarWidth;

			this.Width = Math.Min(Screen.PrimaryScreen.WorkingArea.Width, totalWidth + this.Padding.Horizontal);
			this.Height = Math.Max(this.Height, Math.Min(Screen.PrimaryScreen.WorkingArea.Height - SystemInformation.HorizontalScrollBarHeight, totalHeight - SystemInformation.HorizontalScrollBarHeight));
			this.Location = new Point(this.Location.X, (Screen.PrimaryScreen.WorkingArea.Height - this.Height) / 2);

			this.selectedResult = null;

		}

		/// <summary>
		/// Returns a string containing the tool tip text for the highlighted result.
		/// </summary>
		/// <param name="selectedResult">Currently selected Result</param>
		/// <returns>String containing the tool tip for the Result.</returns>
		private void GetToolTip(Result selectedResult)
		{
			// hide the tooltip
			if (selectedResult == null || selectedResult.FriendlyNames == null)
				ItemTooltip.HideTooltip();
			// show tooltip
			else
				ItemTooltip.ShowTooltip(this.ServiceProvider, selectedResult.FriendlyNames.Item, this);
		}

		/// <summary>
		/// Populates the data grid view from the results list.
		/// </summary>
		private void PopulateDataGridView()
		{
			if (this.resultsList == null || this.resultsList.Count < 1)
				return;

			// Update the dialog text.
			this.Text = string.Format(CultureInfo.CurrentCulture, Resources.ResultsText, this.resultsList.Count, this.searchString);

			for (int i = 0; i < this.resultsList.Count; i++)
			{
				Result result = this.resultsList[i];
				// Add the result to the DataGridView
				int currentRow = this.resultsDataGridView.Rows.Add(
					result.ItemName,
					result.ItemStyle,
					result.ContainerName,
					GetContainerTypeString(result.SackType),
					result.RequiredLevel
				);

				// Change the text color of the item string and style to match the style color.
				if (currentRow > -1)
				{
					this.resultsDataGridView.Rows[currentRow].Cells[0].Style.ForeColor = result.TQColor.Color();
					this.resultsDataGridView.Rows[currentRow].Cells[1].Style.ForeColor = result.TQColor.Color();
					this.resultsDataGridView.Rows[currentRow].Cells[4].Style.Alignment = DataGridViewContentAlignment.MiddleRight;
					this.resultsDataGridView.Rows[currentRow].Tag = i;
				}
			}
		}

		/// <summary>
		/// Handler for user clicking on one of the result cells
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">DataGridViewCellEventArgs data</param>
		private void ResultsDataGridViewCellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			// Ignore double click on the header.
			if (e.RowIndex < 0)
				return;

			this.Close();
		}

		/// <summary>
		/// Handler for pressing a key
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">KeyPressEventArgs data</param>
		private void ResultsDataGridViewKeyPress(object sender, KeyPressEventArgs e)
		{
			// Escape key
			if (e.KeyChar == 27)
				this.Close();
		}

		/// <summary>
		/// Handler for the focus changing to a different row.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">DataGridViewCellEventArgs data</param>
		private void ResultsDataGridViewRowEnter(object sender, DataGridViewCellEventArgs e)
		{
			// Ignore click on the header.
			if (e.RowIndex < 0)
				return;

			var currentRow = this.resultsDataGridView.Rows[e.RowIndex];
			if (currentRow.Tag == null)
				return;

			this.selectedResult = this.resultsList[(int)currentRow.Tag];

			if (this.selectedResult.FriendlyNames != null && this.ResultChanged != null)
				this.ResultChanged(this, new ResultChangedEventArgs(this.selectedResult));
		}

		/// <summary>
		/// Handler for the mouse leaving the DataGridView.  Used to clear the tooltip.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void ResultsDataGridViewMouseLeave(object sender, EventArgs e) => ItemTooltip.HideTooltip();

		/// <summary>
		/// Handler for entering a cell in the DataGridView.
		/// Used to highlight the row under the mouse and updating the tool tip text.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">DataGridViewCellEventArgs data</param>
		private void ResultsDataGridViewCellMouseEnter(object sender, DataGridViewCellEventArgs e)
		{
			// Ignore click on the header.
			if (e.RowIndex < 0)
				return;

			var currentRow = this.resultsDataGridView.Rows[e.RowIndex];
			if (currentRow.Tag == null)
				return;

			this.selectedResult = this.resultsList[(int)currentRow.Tag];
			this.resultsDataGridView.CurrentCell = currentRow.Cells[e.ColumnIndex];
			this.GetToolTip(this.selectedResult);
		}

		/// <summary>
		/// Handler for the mouse leaving a DataGridView cell.
		/// Used to clear the tool tip and the selected result.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">DataGridViewCellEventArgs data</param>
		private void ResultsDataGridViewCellMouseLeave(object sender, DataGridViewCellEventArgs e)
		{
			this.selectedResult = null;
			ItemTooltip.HideTooltip();
		}

		/// <summary>
		/// Handler for leaving a DataGridView row.
		/// Used to clear the tool tip and the selected result.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">DataGridViewCellEventArgs data</param>
		private void ResultsDataGridViewRowLeave(object sender, DataGridViewCellEventArgs e)
		{
			this.selectedResult = null;
			ItemTooltip.HideTooltip();
		}

		/// <summary>
		/// Handler for Resizing the Results Dialog Form.  Resizes the DataGridView based on the Results Dialog size.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void ResultsDialog_Resize(object sender, EventArgs e)
		{
			this.resultsDataGridView.Size = new Size(
				Math.Max(this.Padding.Horizontal + 1, this.Width - this.Padding.Horizontal),
				Math.Max(this.Padding.Vertical + 1, this.Height - this.Padding.Vertical)
			);
		}
	}
}