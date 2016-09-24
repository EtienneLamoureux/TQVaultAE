//-----------------------------------------------------------------------
// <copyright file="ArzExtract.cs" company="bman654">
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
    using System.IO;
    using System.Text;
    using System.Windows.Forms;
    using TQVault.Properties;
    using TQVaultData;
    
    /// <summary>
    /// Class for the ARZ extraction form.
    /// </summary>
    internal partial class ArzExtract : Form
    {
        /// <summary>
        /// MessageBoxOptions for right to left reading.
        /// </summary>
        private static MessageBoxOptions rightToLeftOptions = (MessageBoxOptions)0;

        /// <summary>
        /// Initializes a new instance of the ArzExtract class.
        /// </summary>
        public ArzExtract()
        {
            this.InitializeComponent();

            Database.DB = new Database();
            TQData.MapName = "main";
            Database.DB.LoadDBFile();

            // Scaling for the custom controls.
            if (CurrentAutoScaleDimensions.Width != Database.DesignDpi)
            {
                // Scale to 96 dpi
                Database.DB.Scale = this.CurrentAutoScaleDimensions.Width / Database.DesignDpi;
            }

            this.label1.Text = Resources.ARZExtractLabel1;
            this.label2.Text = Resources.ARZExtractLabel2;
            this.label3.Text = Resources.ARZExtractLabel3;
            this.label4.Text = Resources.ARZExtractLabel4;
            this.label5.Text = Resources.ARZExtractLabel5;
            this.browseButton.Text = Resources.GlobalBrowse;
            this.browseITButton.Text = Resources.GlobalBrowse;
            this.cancelButton.Text = Resources.GlobalCancel;
            this.extractButton.Text = Resources.ARZExtractBtnExtract;
            this.Text = Resources.ARZExtractText;

            // Set options for Right to Left reading.
            if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
            {
                rightToLeftOptions = MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;
            }
        }

        /// <summary>
        /// Do the extraction after parameter validation.
        /// </summary>
        /// <param name="path">Path for TQ database items.</param>
        /// <param name="pathIT">Path for IT database items.</param>
        /// <returns>Returns true if OK</returns>
        private static bool DoExtract(string path, string pathIT)
        {
            return true;
            ////ArzExtractProgress d = new ArzExtractProgress(path, pathIT);
            ////return d.ShowDialog() == DialogResult.OK;
        }

        /// <summary>
        /// Handle browsing for the TQ path.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">EventArgs structure</param>
        private void BrowseButtonClick(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.Description = Resources.ARZExtractDestination;
            dlg.ShowNewFolderButton = true;
            dlg.SelectedPath = TQData.TQVaultSaveFolder;
            DialogResult r = dlg.ShowDialog();
            if (r == DialogResult.OK)
            {
                this.folderTextBox.Text = dlg.SelectedPath;

                // Set the paths the same if not populated.
                if (this.folderITTextBox.Text.Length == 0 && TQData.IsITInstalled)
                {
                    this.folderITTextBox.Text = this.folderTextBox.Text;
                }
            }
        }

        /// <summary>
        /// Handle cancellation of the form
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">EventArgs structure</param>
        private void CancelButtonClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// Validates the parameters when the extract button is hit.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">EventArgs structure</param>
        private void ExtractButtonClick(object sender, EventArgs e)
        {
            // Verify that we have a path.
            string path = this.folderTextBox.Text.Trim();
            string pathIT = null;
            string fullPath = null;
            string fullPathIT = null;

            if (TQData.IsITInstalled)
            {
                pathIT = this.folderITTextBox.Text.Trim();
            }
            else
            {
                pathIT = null;
            }

            if (path == null || path.Length < 1 || ((pathIT == null || pathIT.Length < 1) && TQData.IsITInstalled))
            {
                MessageBox.Show(Resources.ARZExtractValidDest, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, rightToLeftOptions);
                return;
            }

            // See if path exists and create it if necessary
            if (path != null && path.Length > 1)
            {
                fullPath = System.IO.Path.GetFullPath(path);
            }

            if (pathIT != null && pathIT.Length > 1)
            {
                fullPathIT = Path.GetFullPath(pathIT);
            }

            if (File.Exists(fullPath) || (File.Exists(fullPathIT) && TQData.IsITInstalled))
            {
                // they gave us a file??
                MessageBox.Show(
                    Resources.ARZExtractFileDest, 
                    string.Empty, 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.None, 
                    MessageBoxDefaultButton.Button1, 
                    rightToLeftOptions);
                return;
            }

            if (!System.IO.Directory.Exists(fullPath))
            {
                // see if they want to create it
                string q = string.Format(CultureInfo.CurrentCulture, Resources.ARZExtractCreateFolder, fullPath);
                if (MessageBox.Show(q, Resources.ARZExtractCreateFolder2, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, rightToLeftOptions) != DialogResult.Yes)
                {
                    return;
                }

                Directory.CreateDirectory(fullPath);
            }

            if (!Directory.Exists(fullPathIT) && TQData.IsITInstalled)
            {
                // see if they want to create it
                string q = string.Format(CultureInfo.CurrentCulture, Resources.ARZExtractCreateFolderIT, fullPathIT);
                if (MessageBox.Show(q, Resources.ARZExtractCreateFolder2, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, rightToLeftOptions) != DialogResult.Yes)
                {
                    return;
                }

                Directory.CreateDirectory(fullPathIT);
            }

            if (ArzExtract.DoExtract(fullPath, fullPathIT))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        /// <summary>
        /// Handle the browsing for the IT browse button.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">EventArgs structure</param>
        private void BrowseITButtonClick(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.Description = Resources.ARZExtractDestination;
            dlg.ShowNewFolderButton = true;
            dlg.SelectedPath = TQData.TQVaultSaveFolder;
            DialogResult r = dlg.ShowDialog();

            if (r == DialogResult.OK)
            {
                this.folderITTextBox.Text = dlg.SelectedPath;

                // Set the paths the same if not populated.
                if (this.folderTextBox.Text.Length == 0)
                {
                    this.folderTextBox.Text = this.folderITTextBox.Text;
                }
            }
        }

        /// <summary>
        /// Updates the text when the focus is moved from the Folder text box.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventAgrs structure</param>
        private void FolderTextBoxLeave(object sender, EventArgs e)
        {
            // Set the paths the same if not populated.
            if (this.folderITTextBox.Text.Length == 0 && TQData.IsITInstalled)
            {
                this.folderITTextBox.Text = this.folderTextBox.Text;
            }
        }

        /// <summary>
        /// Updates the text when the focus is moved from the IT Folder text box.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventAgrs structure</param>
        private void FolderITTextBoxLeave(object sender, EventArgs e)
        {
            // Set the paths the same if not populated.
            if (this.folderTextBox.Text.Length == 0)
            {
                this.folderTextBox.Text = this.folderITTextBox.Text;
            }
        }
    }
}