using Microsoft.Extensions.Logging;
using System.Globalization;
using TQVaultAE.Application;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.GUI.Models;
using TQVaultAE.GUI.Models.Search;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Forms;

public partial class MainForm
{
	/// <summary>
	/// Handler for clicking the search button on the form.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void SearchButtonClick(object? sender, EventArgs e)
	{
		this.OpenSearchDialog();
	}

	/// <summary>
	/// Search dialog kept between openings: building its thousands of category entries is the slow part,
	/// so it is created once and only rebuilt when the items change. Disposed with the main form.
	/// </summary>
	private readonly KeptDialog<SearchDialogAdvanced> keptSearchDialog = new();

	/// <summary>
	/// Opens the search dialog and shows its results.
	/// </summary>
	private void OpenSearchDialog()
	{
		// A dialog made at another zoom is made again at this one, scaled once from its design like the other dialogs;
		// the new one restores what the user had set in the old one.
		SearchDialogState? keptState = null;
		var watch = System.Diagnostics.Stopwatch.StartNew();
		var zoom = UIService.Zoom;
		var searchDialog = this.keptSearchDialog.Get(this, zoom, () =>
		{
			var dialog = this.Services.Forms.Create<SearchDialogAdvanced>();
			dialog.RestoreWhenBuilt(keptState);
			return dialog;
		}, out bool built, stale => keptState = stale.ClosedState);

		if (built)
			this.Log.LogInformation("Search dialog created in {Elapsed} ms", watch.ElapsedMilliseconds);

		var result = searchDialog.ShowDialog();

		if (result == DialogResult.OK && searchDialog.QueryResults.Length != 0)
			this.DisplayResults(null, searchDialog.QueryResults);
	}

	/// <summary>
	/// Shows the items matching <paramref name="item"/> for the item menu's <i>Search</i> in the results window, the item
	/// first, marked as the source; a click on a result shows it like an Advanced Search result.
	/// </summary>
	/// <param name="item">The item clicked.</param>
	/// <param name="kind">The search.</param>
	internal void ShowItemCopies(Item item, CopySearchKind kind)
	{
		var results = this.Services.ItemDatabaseService.FindCopies(item, kind);
		if (results.Count == 0)
			return;

		var source = results[0];
		string title = ItemCopySearchText.Title(kind, source.ItemName, results.Count - 1);
		this.DisplayResults(null, results, title, source);
	}

	private void DisplayResults(string? searchString, IEnumerable<SearchResult> results, string? title = null, SearchResult? source = null)
	{
		if (results is null || !results.Any())
		{
			this.Services.MessageBoxes.Inform(string.Format(Resources.MainFormNoItemsFound, searchString), Resources.MainFormNoItemsFound2, MessageBoxIcon.Information);
			return;
		}

		// Display a dialog with the results.
		ResultsDialog dlg = this.Services.Forms.Create<ResultsDialog>();
		dlg.ResultChanged += this.SelectResult;
		dlg.ResultsList.Clear();
		dlg.ResultsList.AddRange(results);
		dlg.SearchString = searchString ?? string.Empty;
		dlg.Title = title;
		dlg.SourceResult = source;
		dlg.Show();
	}

	/// <summary>
	/// Selects the item highlighted in the results list.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">ResultChangedEventArgs data</param>
	private void SelectResult(object? sender, ResultChangedEventArgs e)
		=> this.mainView.ShowSearchResult(e.Result);




}
