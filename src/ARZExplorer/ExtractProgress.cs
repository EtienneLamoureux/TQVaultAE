//-----------------------------------------------------------------------
// <copyright file="ExtractProgress.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ArzExplorer;

using ArzExplorer.Models;
using ArzExplorer.Properties;
using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using TQVaultAE.Domain.Entities;

/// <summary>
/// ARZ file extraction progress dialog
/// </summary>
internal sealed partial class ExtractProgress : Form
{
	/// <summary>
	/// MessageBoxOptions for right to left reading.
	/// </summary>
	private static MessageBoxOptions rightToLeftOptions = (MessageBoxOptions)0;

	/// <summary>
	/// Base extraction folder for database
	/// </summary>
	internal string BaseFolder = string.Empty;
	private readonly MainForm MainForm;
	private readonly ArchiveExtractor Extractor;

	/// <summary>
	/// Stops the extraction
	/// </summary>
	private readonly CancellationTokenSource cancellation = new();

	/// <summary>
	/// How the extraction ended
	/// </summary>
	private ExtractionResult? result;

	/// <summary>
	/// Initializes a new instance of the ExtractProgress class.
	/// </summary>
	/// <param name="baseFolder">Base extraction folder for TQ database</param>
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
		{
			rightToLeftOptions = MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;
		}
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
		var token = this.cancellation.Token;
		Task.Run(() =>
		{
			this.result = this.Extractor.ExtractAll(
				file
				, baseFolder
				, recordId => this.Invoke(() => this.UpdateLabel(recordId))
				, () => this.Invoke(this.IncrementProgress)
				, token
			);

			if (this.result.Outcome == ExtractionOutcome.Failed)
				this.Invoke(this.ExtractFailed);
			else
				this.Invoke(this.ExtractComplete);
		});
	}

	/// <summary>
	/// Called if there is a failure to display a message box.
	/// </summary>
	private void ExtractFailed()
	{
		this.DialogResult = DialogResult.Abort;

		MessageBox.Show(
			this.result?.Error?.ToString() ?? string.Empty,
			Resources.ARZProgressFailedText,
			MessageBoxButtons.OK,
			MessageBoxIcon.Error,
			MessageBoxDefaultButton.Button1,
			rightToLeftOptions);

		this.Close();
	}

	/// <summary>
	/// Called when extraction has completed successfully.
	/// </summary>
	private void ExtractComplete()
	{
		if (this.result?.Outcome == ExtractionOutcome.Cancelled)
		{
			MessageBox.Show(
				Resources.ARZProgressCancelledText,
				string.Empty,
				MessageBoxButtons.OK,
				MessageBoxIcon.None,
				MessageBoxDefaultButton.Button1,
				rightToLeftOptions);

			this.DialogResult = DialogResult.Cancel;
		}
		else
		{
			MessageBox.Show(
				Resources.ARZProgressCompleteText,
				string.Empty,
				MessageBoxButtons.OK,
				MessageBoxIcon.None,
				MessageBoxDefaultButton.Button1,
				rightToLeftOptions);

			this.DialogResult = DialogResult.OK;
		}

		this.Close();
	}

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
		this.cancellation.Cancel();
	}
}