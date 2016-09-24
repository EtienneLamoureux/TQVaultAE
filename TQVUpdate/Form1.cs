//-----------------------------------------------------------------------
// <copyright file="Form1.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVUpdate
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Net;
    using System.Net.Cache;
    using System.Text;
    using System.Windows.Forms;
    using TQVaultData;

    /// <summary>
    /// Windows Form Class for TQ Updater
    /// </summary>
    public partial class Form1 : Form
    {
        /// <summary>
        /// Holds the instance of the WebClient used to access the internet.
        /// </summary>
        private WebClient client;

        /// <summary>
        /// Holds the URL to the updates.
        /// </summary>
        private string updateFileURL;

        /// <summary>
        /// Holds the specific filename for the update.
        /// </summary>
        private string updateFileName;

        /// <summary>
        /// Initializes a new instance of the Form1 class.
        /// </summary>
        public Form1()
        {
            this.InitializeComponent();

            this.progressBar1.Maximum = 100;

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                this.updateFileURL = args[1];
                this.updateFileName = Path.GetFileName(this.updateFileURL);

                this.client = new WebClient();
                this.client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.Reload);

                this.DownloadFile();
            }
        }

        /// <summary>
        /// Used to download the update file from the internet.
        /// </summary>
        private void DownloadFile()
        {
            if (this.client == null)
            {
                return;
            }

            ////Stream strm = null;

            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {  
                // Verify network connection
                try
                {
                    string tmpFile = string.Concat(Application.StartupPath, "\\", this.updateFileName);

                    // Delete the existing setup archive file if it exists.
                    if (File.Exists(tmpFile))
                    {
                        File.Delete(tmpFile);
                    }

                    // Specify that the DownloadFileCallback method gets called when the download completes.
                    this.client.DownloadFileCompleted += new AsyncCompletedEventHandler(this.DownloadFileCallback);

                    // Specify a progress notification handler.
                    this.client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(this.DownloadProgressCallback);

                    // Update the title bar with the filename.
                    this.Text = string.Format("Downloading {0}...", this.updateFileName);

                    Uri uri = new Uri(this.updateFileURL);
                    this.client.DownloadFileAsync(uri, tmpFile);

                    // The download complete event will trigger the remaining tasks.
                }
                catch (Exception)
                {
                    ////if (strm != null) strm.Close();
                    ////m_streamException = true;
                }
            }
        }

        /// <summary>
        /// Callback to update the download progress.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">DownloadProgressChangedEventArgs data</param>
        private void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            // Set the progress bar to the completed %
            this.progressBar1.Value = e.ProgressPercentage;
        }

        /// <summary>
        /// Callback for the asynchronous file download.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">AsyncCompletedEventArgs data</param>
        private void DownloadFileCallback(object sender, AsyncCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
            {
                ArcFile arcFile = new ArcFile(this.updateFileName);
                bool result = arcFile.ExtractArcFile(Application.StartupPath);

                if (result)
                {
                    // Start TQVault
                    if (File.Exists(string.Concat(Application.StartupPath, "\\TQVault.exe")))
                    {
                        Process.Start(string.Concat(Application.StartupPath, "\\TQVault.exe"));
                    }

                    Environment.Exit(0);
                }
                else
                {
                    MessageBox.Show(string.Format("Error extracting {0}", this.updateFileName));
                }
            }

            if (e.Cancelled)
            {
                this.DialogResult = DialogResult.Cancel;
            }
        }

        /// <summary>
        /// Processes the clicking the cancel button.  Stops the current download.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">EventArgs data</param>
        private void CancelButtonClick(object sender, EventArgs e)
        {
            if (this.client != null)
            {
                this.client.CancelAsync();
            }

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}