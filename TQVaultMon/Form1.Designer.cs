//-----------------------------------------------------------------------
// <copyright file="Form1.Designer.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultMon
{
    /// <summary>
    /// Form Designer class for TQVaultMon
    /// </summary>
    internal partial class Form1
    {
        /// <summary>
        /// Text Box holding any messages
        /// </summary>
        private System.Windows.Forms.TextBox outputTextBox;

        /// <summary>
        /// Timer for patching the TQ process
        /// </summary>
        private System.Windows.Forms.Timer timer;

        /// <summary>
        /// Button to start Titan Quest.
        /// </summary>
        private System.Windows.Forms.Button startTitanQuestButton;

        /// <summary>
        /// Button to start TQ Vault.
        /// </summary>
        private System.Windows.Forms.Button startTQVaultButton;

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
            this.components = new System.ComponentModel.Container();
            this.outputTextBox = new System.Windows.Forms.TextBox();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.startTitanQuestButton = new System.Windows.Forms.Button();
            this.startTQVaultButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // outputTextBox
            // 
            this.outputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.outputTextBox.BackColor = System.Drawing.Color.Black;
            this.outputTextBox.ForeColor = System.Drawing.Color.DeepSkyBlue;
            this.outputTextBox.Location = new System.Drawing.Point(10, 8);
            this.outputTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.outputTextBox.Multiline = true;
            this.outputTextBox.Name = "outputTextBox";
            this.outputTextBox.ReadOnly = true;
            this.outputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.outputTextBox.Size = new System.Drawing.Size(834, 248);
            this.outputTextBox.TabIndex = 0;
            this.outputTextBox.Text = "Titan Quest Item Vault Monitor";
            // 
            // timer
            // 
            this.timer.Enabled = true;
            this.timer.Interval = 2000;
            this.timer.Tick += new System.EventHandler(this.TimerTick);
            // 
            // startTitanQuestButton
            // 
            this.startTitanQuestButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.startTitanQuestButton.Location = new System.Drawing.Point(480, 264);
            this.startTitanQuestButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.startTitanQuestButton.Name = "startTitanQuestButton";
            this.startTitanQuestButton.Size = new System.Drawing.Size(125, 32);
            this.startTitanQuestButton.TabIndex = 1;
            this.startTitanQuestButton.Text = "Start Titan Quest";
            this.startTitanQuestButton.Click += new System.EventHandler(this.StartTitanQuestButtonClick);
            // 
            // startTQVaultButton
            // 
            this.startTQVaultButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.startTQVaultButton.Location = new System.Drawing.Point(250, 264);
            this.startTQVaultButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.startTQVaultButton.Name = "startTQVaultButton";
            this.startTQVaultButton.Size = new System.Drawing.Size(125, 32);
            this.startTQVaultButton.TabIndex = 2;
            this.startTQVaultButton.Text = "Start TQVault";
            this.startTQVaultButton.Click += new System.EventHandler(this.StartTQVaultButtonClick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(854, 301);
            this.Controls.Add(this.outputTextBox);
            this.Controls.Add(this.startTitanQuestButton);
            this.Controls.Add(this.startTQVaultButton);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "Form1";
            this.Text = "TQVault Monitor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}

