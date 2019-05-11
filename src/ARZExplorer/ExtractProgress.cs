//-----------------------------------------------------------------------
// <copyright file="ExtractProgress.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace ArzExplorer
{
	using ArzExplorer.Properties;
	using System;
	using System.Globalization;
	using System.Threading;
	using System.Windows.Forms;
	using TQVaultAE.DAL;

	/// <summary>
	/// ARZ file extraction progress dialog
	/// </summary>
	internal partial class ExtractProgress : Form
	{
		/// <summary>
		/// MessageBoxOptions for right to left reading.
		/// </summary>
		private static MessageBoxOptions rightToLeftOptions = (MessageBoxOptions)0;

		/// <summary>
		/// Base extraction folder for database
		/// </summary>
		private string baseFolder;

		/// <summary>
		/// ID for current records
		/// </summary>
		private string recordIdBeingProcessed;

		/// <summary>
		/// Holds any exception we have encountered
		/// </summary>
		private Exception exception;

		/// <summary>
		/// Holds cancel status
		/// </summary>
		private bool cancel;

		/// <summary>
		/// Initializes a new instance of the ExtractProgress class.
		/// </summary>
		/// <param name="baseFolder">Base extraction folder for TQ database</param>
		public ExtractProgress(string baseFolder)
		{
			this.baseFolder = baseFolder;
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
			this.cancel = false;

			// Setup the progress bar
			if (Form1.FileType == CompressedFileType.ArcFile)
			{
				this.progressBar1.Maximum = Form1.ARCFile.Count;
			}
			else
			{
				this.progressBar1.Maximum = Form1.ARZFile.Count;
			}

			this.progressBar1.Value = 0;

			// Create a thread to do the extraction
			ThreadStart tstart;
			if (Form1.FileType == CompressedFileType.ArcFile)
			{
				tstart = new ThreadStart(this.DoArcExtraction);
			}
			else
			{
				tstart = new ThreadStart(this.DoArzExtraction);
			}

			Thread t = new Thread(tstart);
			t.Priority = ThreadPriority.Normal;
			t.Start();
		}

		/// <summary>
		/// Performs the extraction of an ARZ file.
		/// </summary>
		private void DoArzExtraction()
		{
			try
			{
				bool canceled = false;

				foreach (string recordID in Form1.ARZFile.GetKeyTable())
				{
					if (canceled)
					{
						break;
					}

					// update label with recordID
					this.recordIdBeingProcessed = recordID;
					this.Invoke(new MethodInvoker(this.UpdateLabel));

					// Write the record
					Form1.ARZFile.GetRecordNotCached(recordID).Write(this.baseFolder);

					// Update progressbar
					this.Invoke(new MethodInvoker(this.IncrementProgress));

					// see if we need to cancel
					Monitor.Enter(this);
					canceled = this.cancel;
					Monitor.Exit(this);
				}

				// notify complete.
				this.Invoke(new MethodInvoker(this.ExtractComplete));
			}
			catch (Exception err)
			{
				// notify failure
				this.exception = err;
				this.Invoke(new MethodInvoker(this.ExtractFailed));
				throw;
			}
		}

		/// <summary>
		/// Performs the extraction of an ARC file.
		/// </summary>
		private void DoArcExtraction()
		{
			try
			{
				bool canceled = false;

				foreach (string recordID in Form1.ARCFile.GetKeyTable())
				{
					if (canceled)
					{
						break;
					}

					// update label with recordID
					this.recordIdBeingProcessed = recordID;
					this.Invoke(new MethodInvoker(this.UpdateLabel));

					// Write the record
					Form1.ARCFile.Write(this.baseFolder, recordID, recordID);

					// Update progressbar
					this.Invoke(new MethodInvoker(this.IncrementProgress));

					// see if we need to cancel
					Monitor.Enter(this);
					canceled = this.cancel;
					Monitor.Exit(this);
				}

				// notify complete.
				this.Invoke(new MethodInvoker(this.ExtractComplete));
			}
			catch (Exception err)
			{
				// notify failure
				this.exception = err;
				this.Invoke(new MethodInvoker(this.ExtractFailed));
				throw;
			}
		}

		/// <summary>
		/// Called if there is a failure to display a message box.
		/// </summary>
		private void ExtractFailed()
		{
			this.DialogResult = DialogResult.Abort;

			MessageBox.Show(
				this.exception.ToString(),
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
			if (this.cancel)
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
		private void UpdateLabel()
		{
			this.label1.Text = string.Format(CultureInfo.CurrentCulture, Resources.ARZProgressLabel, this.recordIdBeingProcessed);
		}

		/// <summary>
		/// Handle cancellation of the form.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void CancelButtonClick(object sender, EventArgs e)
		{
			Monitor.Enter(this);
			this.cancel = true;
			Monitor.Exit(this);
		}
	}
}