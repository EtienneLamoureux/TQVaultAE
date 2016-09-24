//-----------------------------------------------------------------------
// <copyright file="ArzExtract.designer.cs" company="bman654">
//     Copyright (c) Brandon Wallace. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVault
{
    /// <summary>
    /// Class for the Arz Extration form.
    /// </summary>
    internal partial class ArzExtract
    {
        /// <summary>
        /// label1 on the form
        /// </summary>
        private ScalingLabel label1;

        /// <summary>
        /// label2 on the form
        /// </summary>
        private ScalingLabel label2;

        /// <summary>
        /// label3 on the form
        /// </summary>
        private ScalingLabel label3;

        /// <summary>
        /// label4 on the form
        /// </summary>
        private ScalingLabel label4;

        /// <summary>
        /// label5 on the form
        /// </summary>
        private ScalingLabel label5;

        /// <summary>
        /// Folder Text box on the form
        /// </summary>
        private ScalingTextBox folderTextBox;

        /// <summary>
        /// Browse button on the form
        /// </summary>
        private ScalingButton browseButton;

        /// <summary>
        /// Cancel button on the form
        /// </summary>
        private ScalingButton cancelButton;

        /// <summary>
        /// Extract button on the form
        /// </summary>
        private ScalingButton extractButton;

        /// <summary>
        /// Browse button for the IT database.
        /// </summary>
        private ScalingButton browseITButton;

        /// <summary>
        /// Text box for the IT folder
        /// </summary>
        private ScalingTextBox folderITTextBox;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ArzExtract));
            this.label1 = new ScalingLabel();
            this.label2 = new ScalingLabel();
            this.label3 = new ScalingLabel();
            this.folderTextBox = new ScalingTextBox();
            this.browseButton = new ScalingButton();
            this.cancelButton = new ScalingButton();
            this.extractButton = new ScalingButton();
            this.browseITButton = new ScalingButton();
            this.folderITTextBox = new ScalingTextBox();
            this.label4 = new ScalingLabel();
            this.label5 = new ScalingLabel();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(7, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(577, 59);
            this.label1.TabIndex = 0;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(7, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(577, 32);
            this.label2.TabIndex = 1;
            this.label2.Text = "After you have extracted the records, just copy the ones you want to use in your " +
                "mod over to your mod folder.";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(15, 119);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(127, 38);
            this.label3.TabIndex = 2;
            this.label3.Text = "Choose a destination folder:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbFolder
            // 
            this.folderTextBox.Location = new System.Drawing.Point(147, 127);
            this.folderTextBox.Name = "tbFolder";
            this.folderTextBox.Size = new System.Drawing.Size(366, 21);
            this.folderTextBox.TabIndex = 3;
            this.folderTextBox.Leave += new System.EventHandler(this.FolderTextBoxLeave);
            // 
            // btnBrowse
            // 
            this.browseButton.AutoSize = true;
            this.browseButton.BackColor = System.Drawing.Color.Gold;
            this.browseButton.ForeColor = System.Drawing.Color.Black;
            this.browseButton.Location = new System.Drawing.Point(522, 127);
            this.browseButton.Name = "btnBrowse";
            this.browseButton.Size = new System.Drawing.Size(78, 24);
            this.browseButton.TabIndex = 4;
            this.browseButton.Text = "Browse...";
            this.browseButton.UseVisualStyleBackColor = false;
            this.browseButton.Click += new System.EventHandler(this.BrowseButtonClick);
            // 
            // btnCancel
            // 
            this.cancelButton.AutoSize = true;
            this.cancelButton.BackColor = System.Drawing.Color.Gold;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.ForeColor = System.Drawing.Color.Black;
            this.cancelButton.Location = new System.Drawing.Point(381, 278);
            this.cancelButton.Name = "btnCancel";
            this.cancelButton.Size = new System.Drawing.Size(78, 24);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = false;
            this.cancelButton.Click += new System.EventHandler(this.CancelButtonClick);
            // 
            // btnExtract
            // 
            this.extractButton.AutoSize = true;
            this.extractButton.BackColor = System.Drawing.Color.Gold;
            this.extractButton.ForeColor = System.Drawing.Color.Black;
            this.extractButton.Location = new System.Drawing.Point(163, 278);
            this.extractButton.Name = "btnExtract";
            this.extractButton.Size = new System.Drawing.Size(78, 24);
            this.extractButton.TabIndex = 6;
            this.extractButton.Text = "Extract";
            this.extractButton.UseVisualStyleBackColor = false;
            this.extractButton.Click += new System.EventHandler(this.ExtractButtonClick);
            // 
            // btnBrowseIT
            // 
            this.browseITButton.AutoSize = true;
            this.browseITButton.BackColor = System.Drawing.Color.Gold;
            this.browseITButton.ForeColor = System.Drawing.Color.Black;
            this.browseITButton.Location = new System.Drawing.Point(522, 230);
            this.browseITButton.Name = "btnBrowseIT";
            this.browseITButton.Size = new System.Drawing.Size(78, 24);
            this.browseITButton.TabIndex = 10;
            this.browseITButton.Text = "Browse...";
            this.browseITButton.UseVisualStyleBackColor = false;
            this.browseITButton.Click += new System.EventHandler(this.BrowseITButtonClick);
            // 
            // tbFolderIT
            // 
            this.folderITTextBox.AcceptsReturn = true;
            this.folderITTextBox.Location = new System.Drawing.Point(147, 232);
            this.folderITTextBox.Name = "tbFolderIT";
            this.folderITTextBox.Size = new System.Drawing.Size(366, 21);
            this.folderITTextBox.TabIndex = 9;
            this.folderITTextBox.Leave += new System.EventHandler(this.FolderITTextBoxLeave);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(15, 227);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(127, 31);
            this.label4.TabIndex = 8;
            this.label4.Text = "Choose a destination folder for IT Database Files:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(7, 157);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(577, 70);
            this.label5.TabIndex = 11;
            this.label5.Text = resources.GetString("label5.Text");
            // 
            // ARZExtract
            // 
            this.AcceptButton = this.extractButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(612, 320);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.browseITButton);
            this.Controls.Add(this.folderITTextBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.extractButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.browseButton);
            this.Controls.Add(this.folderTextBox);
            this.Controls.Add(this.label3);
            this.Font = new System.Drawing.Font("Albertus MT", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ARZExtract";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Extract database.arz";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}

