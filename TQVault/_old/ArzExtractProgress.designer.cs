//-----------------------------------------------------------------------
// <copyright file="ArzExtractProgress.designer.cs" company="bman654">
//     Copyright (c) Brandon Wallace. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVault
{
    /// <summary>
    /// ARZ file extraction progress dialog
    /// </summary>
    internal partial class ArzExtractProgress
    {
        /// <summary>
        /// label1 on the form
        /// </summary>
        private System.Windows.Forms.Label label1;

        /// <summary>
        /// Progress bar on the form
        /// </summary>
        private System.Windows.Forms.ProgressBar progressBar1;

        /// <summary>
        /// Cancel Button on the form.
        /// </summary>
        private System.Windows.Forms.Button cancelButton;

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
            this.label1 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(16, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(536, 24);
            this.label1.TabIndex = 0;
            this.label1.Text = "Extracting";
            // 
            // progressBar1
            // 
            this.progressBar1.ForeColor = System.Drawing.Color.Gold;
            this.progressBar1.Location = new System.Drawing.Point(16, 64);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(536, 23);
            this.progressBar1.Step = 1;
            this.progressBar1.TabIndex = 1;
            this.progressBar1.Value = 30;
            // 
            // button1
            // 
            this.cancelButton.BackColor = System.Drawing.Color.Gold;
            this.cancelButton.ForeColor = System.Drawing.Color.Black;
            this.cancelButton.Location = new System.Drawing.Point(247, 120);
            this.cancelButton.Name = "button1";
            this.cancelButton.Size = new System.Drawing.Size(75, 32);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = false;
            this.cancelButton.Click += new System.EventHandler(this.CancelButtonClick);
            // 
            // ARZExtractProgressDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.ClientSize = new System.Drawing.Size(568, 167);
            this.ControlBox = false;
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Albertus MT", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "ARZExtractProgressDlg";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Extracting database.arz...";
            this.Load += new System.EventHandler(this.ARZExtractProgressDlg_Load);
            this.ResumeLayout(false);

        }

        #endregion
    }
}