//-----------------------------------------------------------------------
// <copyright file="ArzExtractProgress.cs" company="bman654">
//     Copyright (c) Brandon Wallace. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVault
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Globalization;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
    using TQVault.Properties;
    using TQVaultData;

    /// <summary>
    /// ARZ file extraction progress dialog
    /// </summary>
    internal partial class ArzExtractProgress : Form
    {
        /// <summary>
        /// MessageBoxOptions for right to left reading.
        /// </summary>
        private static MessageBoxOptions rightToLeftOptions = (MessageBoxOptions)0;

        /// <summary>
        /// Base extraction folder for TQ database
        /// </summary>
        private string baseFolder;

        /// <summary>
        /// Base extraction folder for IT database
        /// </summary>
        private string baseFolderIT;

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
        /// Initializes a new instance of the ArzExtractProgress class.
        /// </summary>
        /// <param name="baseFolder">Base extraction folder for TQ database</param>
        /// <param name="baseFolderIT">Base extraction folder for IT database</param>
        public ArzExtractProgress(string baseFolder, string baseFolderIT)
        {
            this.baseFolder = baseFolder;
            this.baseFolderIT = baseFolderIT;
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
        /// <param name="e">EventArgs structure</param>
        private void ARZExtractProgressDlg_Load(object sender, EventArgs e)
        {
            this.cancel = false;

            // Setup the progress bar
            this.progressBar1.Maximum = Database.DB.ArzFile.Count;
            if (TQData.IsITInstalled)
            {
                this.progressBar1.Maximum += Database.DB.ArzFileIT.Count;
            }

            this.progressBar1.Value = 0;

            // Create a thread to do the extraction
            ThreadStart tstart = new ThreadStart(this.DoExtraction);
            Thread t = new Thread(tstart);
            t.Priority = ThreadPriority.Normal;
            t.Start();
        }

        /// <summary>
        /// Performs the extraction of the database files.
        /// </summary>
        private void DoExtraction()
        {
            try
            {
                /*IEnumerator<string> records = Database.DB.ArzFile.RecordEnumerator;
                bool canceled = false;

                while (!canceled && records.MoveNext())
                {
                    string recordID = (string)records.Current;

                    // update label with recordID
                    this.recordIdBeingProcessed = recordID;
                    this.Invoke(new MethodInvoker(this.UpdateLabel));

                    // Write the record
                    Database.DB.ArzFile.GetRecordNotCached(recordID).Write(this.baseFolder);

                    // Update progressbar
                    this.Invoke(new MethodInvoker(this.IncrementProgress));

                    // see if we need to cancel
                    Monitor.Enter(this);
                    canceled = this.cancel;
                    Monitor.Exit(this);
                }

                if (TQData.IsITInstalled)
                {
                    IEnumerator<string> recordsIT = Database.DB.ArzFileIT.RecordEnumerator;
                    bool cancelledIT = false;

                    while (!cancelledIT && recordsIT.MoveNext())
                    {
                        string recordID = (string)recordsIT.Current;

                        // update label with recordID
                        this.recordIdBeingProcessed = recordID;
                        this.Invoke(new MethodInvoker(this.UpdateLabel));

                        // Write the record
                        Database.DB.ArzFileIT.GetRecordNotCached(recordID).Write(this.baseFolderIT);

                        // Update progressbar
                        this.Invoke(new MethodInvoker(this.IncrementProgress));

                        // see if we need to cancel
                        Monitor.Enter(this);
                        cancelledIT = this.cancel;
                        Monitor.Exit(this);
                    }
                }
                */
                // notify complete if not IT
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
        /// <param name="e">EventArgs structure</param>
        private void CancelButtonClick(object sender, EventArgs e)
        {
            Monitor.Enter(this);
            this.cancel = true;
            Monitor.Exit(this);
        }
    }
}