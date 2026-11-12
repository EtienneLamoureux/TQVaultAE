//-----------------------------------------------------------------------
// <copyright file="ExtractProgress.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using ArzExplorer.Models;
using ArzExplorer.Properties;
using System.Globalization;
using TQVaultAE.Domain.Entities;

namespace ArzExplorer;

/// <summary>
/// Progress dialog of Extract All Files: extracts the selected file on a worker thread.
/// </summary>
internal sealed partial class ExtractProgress : Form
{
	/// <summary>
	/// MessageBoxOptions for right to left reading.
	/// </summary>
	private readonly MessageBoxOptions _rightToLeftOptions;

	/// <summary>
	/// Base extraction folder for database
	/// </summary>
	internal string BaseFolder = string.Empty;
	private readonly MainForm MainForm;
	private readonly ArchiveExtractor Extractor;

	/// <summary>
	/// Stops the extraction
	/// </summary>
	private readonly CancellationTokenSource _cancellation = new();

	/// <summary>
	/// How the extraction ended
	/// </summary>
	private ExtractionResult? _result;

	/// <summary>
	/// True while the worker thread runs; set and cleared on the UI thread.
	/// </summary>
	private bool _running;

	/// <summary>
	/// Initializes a new instance of the ExtractProgress class.
	/// </summary>
	/// <param name="mainForm">The main window, whose selected file is extracted.</param>
	/// <param name="extractor">Writes the records or entries.</param>
	public ExtractProgress(MainForm mainForm, ArchiveExtractor extractor)
	{
		this.MainForm = mainForm;
		this.Extractor = extractor;
		this.InitializeComponent();

		this.Text = Resources.ARZProgressText;
		this.label1.Text = Resources.ARZProgressLabel1;
		this.cancelButton.Text = Resources.GlobalCancel;

		// Set options for Right to Left reading.
		if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
			this._rightToLeftOptions = MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;
	}

	/// <summary>
	/// Called when the dialog loads.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void ARZExtractProgressDlg_Load(object sender, EventArgs e)
	{
		var file = this.MainForm.SelectedFile;

		// Setup the progress bar
		this.progressBar1.Maximum = ArchiveExtractor.Count(file);
		this.progressBar1.Value = 0;

		// Extract on a worker thread. A failure comes back in the result:
		// an exception thrown on that thread would end the application.
		var baseFolder = this.BaseFolder;
		var token = this._cancellation.Token;
		this._running = true;
		Task.Run(() =>
		{
			var result = this.Extractor.ExtractAll(
				file
				, baseFolder
				, recordId => this.OnUi(() => this.UpdateLabel(recordId))
				, () => this.OnUi(this.IncrementProgress)
				, token
			);

			this.OnUi(() =>
			{
				this._result = result;
				this._running = false;
				if (result.Outcome == ExtractionOutcome.Failed)
					this.ExtractFailed();
				else
					this.ExtractComplete();
			});
		});
	}

	/// <summary>
	/// Runs <paramref name="action"/> on the UI thread from the worker; does nothing once the dialog is gone
	/// (closed by the owner or at shutdown), so the worker never fails because of the window.
	/// </summary>
	private void OnUi(Action action)
	{
		if (this.IsDisposed || !this.IsHandleCreated)
			return;

		try
		{
			this.Invoke(action);
		}
		catch (ObjectDisposedException)
		{
			// Disposed between the check and the call.
		}
		catch (InvalidOperationException) when (this.IsDisposed || !this.IsHandleCreated)
		{
			// Handle destroyed between the check and the call.
		}
	}

	/// <summary>
	/// Closing the dialog (Alt+F4, or its owner closing) stops the extraction. A user close waits, like Cancel,
	/// for the entry being written: the dialog then closes itself once the worker has stopped.
	/// </summary>
	/// <param name="e">FormClosingEventArgs data</param>
	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		if (this._running)
		{
			this._cancellation.Cancel();
			this.cancelButton.Enabled = false;
			if (e.CloseReason == CloseReason.UserClosing)
				e.Cancel = true;
		}

		base.OnFormClosing(e);
	}

	/// <summary>
	/// Called if there is a failure to display a message box.
	/// </summary>
	private void ExtractFailed()
	{
		this.DialogResult = DialogResult.Abort;

		this.ShowMessage(this._result?.Error?.ToString() ?? string.Empty, Resources.ARZProgressFailedText, MessageBoxIcon.Error);

		this.Close();
	}

	/// <summary>
	/// Called when extraction has completed successfully.
	/// </summary>
	private void ExtractComplete()
	{
		if (this._result?.Outcome == ExtractionOutcome.Cancelled)
		{
			this.ShowMessage(Resources.ARZProgressCancelledText, string.Empty, MessageBoxIcon.None);
			this.DialogResult = DialogResult.Cancel;
		}
		else
		{
			this.ShowMessage(Resources.ARZProgressCompleteText, string.Empty, MessageBoxIcon.None);
			this.DialogResult = DialogResult.OK;
		}

		this.Close();
	}

	/// <summary>
	/// Shows an OK message box, right to left when the UI culture reads that way.
	/// </summary>
	private void ShowMessage(string text, string caption, MessageBoxIcon icon)
		=> MessageBox.Show(text, caption, MessageBoxButtons.OK, icon, MessageBoxDefaultButton.Button1, this._rightToLeftOptions);

	/// <summary>
	/// Increment the progress bar.
	/// One step per file extracted.
	/// </summary>
	private void IncrementProgress()
	{
		this.progressBar1.PerformStep();
	}

	/// <summary>
	/// Update the current file name on the display.
	/// </summary>
	private void UpdateLabel(RecordId recordId)
	{
		this.label1.Text = string.Format(CultureInfo.CurrentCulture, Resources.ARZProgressLabel, recordId);
	}

	/// <summary>
	/// Handle cancellation of the form.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void CancelButtonClick(object sender, EventArgs e)
	{
		this._cancellation.Cancel();
		this.cancelButton.Enabled = false;
	}
}
