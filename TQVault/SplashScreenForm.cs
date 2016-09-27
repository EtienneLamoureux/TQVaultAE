//-----------------------------------------------------------------------
// <copyright file="SplashScreenForm.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVault
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Timers;
    using System.Windows.Forms;
    using TQVault.Properties;

    /// <summary>
    /// Class for inital form which load the resources.
    /// </summary>
    internal partial class SplashScreenForm : VaultForm
    {
        /// <summary>
        /// Holds the width of the border pen.
        /// </summary>
        private static int borderPenWidth = 4;

        /// <summary>
        /// Flag to signal color flip on please wait message.
        /// </summary>
        private int waitTimerFlip;

        /// <summary>
        /// Indicates whether to fade in or fade out the form.
        /// </summary>
        private bool fadeOut;

        /// <summary>
        /// Holds the color of the label prior to flipping to white.
        /// </summary>
        private Color flipColor;

        /// <summary>
        /// Interval used to adjust the opacity during form fade in.
        /// </summary>
        private double fadeInInterval;

        /// <summary>
        /// Interval used to adjust the opacity during form fade out.
        /// </summary>
        private double fadeOutInterval;

        /// <summary>
        /// Indicates whether the form will be drawn with the outer edges clipped to a rounded rectangle.
        /// </summary>
        private bool useRoundedRectangle;

        /// <summary>
        /// GraphicsPath used for clipping to a rounded rectangle.
        /// </summary>
        private GraphicsPath graphicsPath;

        /// <summary>
        /// Initializes a new instance of the SplashScreenForm class.
        /// </summary>
        public SplashScreenForm()
        {
            this.InitializeComponent();

            this.Text = Resources.Form1Text;
            this.label1.Text = Resources.Form1Label1;
            this.label2.Text = Resources.Form1Label2;
            this.label3.Text = Resources.Form1Label3;
            this.exitButton.Text = Resources.GlobalExit;
            this.nextButton.Text = Resources.Form1bNext;
            this.labelPleaseWait.Text = Resources.Form1LblPleaseWait;
            this.Icon = Resources.TQVIcon;
            this.ShowMainForm = false;
            this.Opacity = 0.0F;

            if (Settings.Default.EnableNewUI)
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.label1.Visible = false;
                this.label2.Visible = false;
                this.progressBar.Visible = false;
                this.progressBar.Enabled = false;
                this.fadeInInterval = Settings.Default.FadeInInterval;
                this.fadeOutInterval = Settings.Default.FadeOutInterval;
                this.DrawCustomBorder = true;
            }
            else
            {
                this.Revert(new Size(780, 550));
                this.fadeInInterval = 1.0F;
                this.fadeOutInterval = 1.0F;
            }

            if (this.UseRoundedRectangle)
            {
                this.graphicsPath = this.CreatePath(20);
                if (this.graphicsPath != null)
                {
                    this.Region = new Region(this.graphicsPath);
                }
            }

            this.flipColor = this.labelPleaseWait.ForeColor;
            this.fadeTimer.Start();
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not to show the main form.
        /// </summary>
        public bool ShowMainForm { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the form will be drawn with the outer edges clipped to a rounded rectangle.
        /// </summary>
        public bool UseRoundedRectangle
        {
            get
            {
                if (this.FormBorderStyle != FormBorderStyle.None || this.DrawCustomBorder)
                {
                    return false;
                }

                return this.useRoundedRectangle;
            }

            set
            {
                this.useRoundedRectangle = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum value for the progress bar
        /// </summary>
        public int MaximumValue
        {
            get
            {
                if (this.progressBar.Enabled)
                {
                    return this.progressBar.Maximum;
                }
                else
                {
                    return this.newProgressBar.Maximum;
                }
            }

            set
            {
                if (this.progressBar.Enabled)
                {
                    this.progressBar.Maximum = value;
                }
                else
                {
                    this.newProgressBar.Maximum = value;
                }
            }
        }

        /// <summary>
        /// Updates text after the resources have been loaded.
        /// </summary>
        public void UpdateText()
        {
            this.progressBar.Visible = false;
            this.newProgressBar.Visible = false;
            this.labelPleaseWait.Visible = false;
            this.nextButton.Visible = true;
            this.waitTimer.Enabled = false;
            this.nextButton.Focus();
        }

        /// <summary>
        /// Increments the progress bar
        /// </summary>
        public void IncrementValue()
        {
            if (this.progressBar.Enabled)
            {
                this.progressBar.Increment(1);
            }
            else
            {
                this.newProgressBar.Increment(1);
            }
        }

        /// <summary>
        /// Close the form and start the fade out.
        /// </summary>
        public void CloseForm()
        {
            this.fadeOut = true;
            this.fadeTimer.Start();
        }

        /// <summary>
        /// Paint handler.
        /// </summary>
        /// <param name="e">PaintEventArgs data</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Paint a black border around the form.
            if (this.UseRoundedRectangle && this.graphicsPath != null)
            {
                Pen pen = new Pen(Color.Black, borderPenWidth);
                e.Graphics.DrawPath(pen, this.graphicsPath);
            }
        }

        /// <summary>
        /// Reverts the form back to the original size and UI style.
        /// </summary>
        /// <param name="originalSize">Original size of the form.</param>
        protected override void Revert(Size originalSize)
        {
            this.DrawCustomBorder = false;
            this.ClientSize = originalSize;
            this.BackgroundImage = Resources.SplashScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.UseRoundedRectangle = false;

            this.label1.Visible = true;
            this.label1.Font = new Font("Microsoft Sans Serif", 10.0F, FontStyle.Bold, GraphicsUnit.Point, (byte)0);
            this.label1.ForeColor = Color.FromArgb((int)((byte)255), (int)((byte)128), (int)((byte)128));
            this.label1.Location = new Point(16, 16);
            this.label1.Size = new Size(467, 24);

            this.label2.Visible = true;
            this.label2.Font = new Font("Microsoft Sans Serif", 30.0F, (FontStyle)(FontStyle.Bold | FontStyle.Italic), GraphicsUnit.Point, (byte)0);
            this.label2.ForeColor = Color.FromArgb((int)((byte)255), (int)((byte)128), (int)((byte)128));
            this.label2.Location = new Point(196, 118);
            this.label2.Size = new Size(392, 112);

            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.0F, FontStyle.Bold, GraphicsUnit.Point, (byte)0);
            this.label3.ForeColor = System.Drawing.Color.FromArgb((int)((byte)255), (int)((byte)128), (int)((byte)128));
            this.label3.Location = new System.Drawing.Point(162, 247);
            this.label3.Size = new System.Drawing.Size(461, 64);

            this.labelPleaseWait.Font = new Font("Microsoft Sans Serif", 12.0F, FontStyle.Bold, GraphicsUnit.Point, (byte)0);
            this.labelPleaseWait.ForeColor = Color.FromArgb((int)((byte)255), (int)((byte)128), (int)((byte)128));
            this.labelPleaseWait.Location = new Point(161, 357);
            this.labelPleaseWait.Size = new Size(457, 32);

            this.nextButton.Revert(new Point(344, 416), new Size(96, 44));
            this.exitButton.Revert(new Point(664, 504), new Size(75, 23));

            this.progressBar.Enabled = true;
            this.progressBar.Visible = true;
            this.progressBar.Location = new System.Drawing.Point(144, 428);
            this.progressBar.Size = new System.Drawing.Size(498, 23);

            this.newProgressBar.Visible = false;
            this.newProgressBar.Enabled = false;
        }

        /// <summary>
        /// Creates a rounded rectangle GraphicsPath around the edges of the Form.
        /// </summary>
        /// <param name="cornerRadius">Radius of the rounded corner</param>
        /// <returns>GraphicsPath representing a rounded recangle around the Form.</returns>
        private GraphicsPath CreatePath(int cornerRadius)
        {
            int doubleRadius = cornerRadius * 2;

            GraphicsPath path = new GraphicsPath();
            path.StartFigure();

            // Top Left Corner
            path.AddArc(0, 0, doubleRadius, doubleRadius, 180.0F, 90.0F);

            // Top Edge
            path.AddLine(cornerRadius, 0, this.Width - cornerRadius, 0);

            // Top Right Corner
            path.AddArc(this.Width - doubleRadius, 0, doubleRadius, doubleRadius, 270, 90);

            // Right Edge
            path.AddLine(this.Width, cornerRadius, this.Width, this.Height - cornerRadius);

            // Bottom Right Corner
            path.AddArc(this.Width - doubleRadius, this.Height - doubleRadius, doubleRadius, doubleRadius, 0, 90);

            // Bottom Edge
            path.AddLine(this.Width - cornerRadius, this.Height, cornerRadius, this.Height);

            // Bottom Left Corner
            path.AddArc(0, this.Height - doubleRadius, doubleRadius, doubleRadius, 90, 90);

            // Left Edge
            path.AddLine(0, this.Height - cornerRadius, 0, cornerRadius);

            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// Handles the exit button
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs data</param>
        private void ExitButtonClick(object sender, EventArgs e)
        {
            this.CloseForm();
        }

        /// <summary>
        /// Handles the next button
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs data</param>
        private void NextButtonClick(object sender, EventArgs e)
        {
            this.CloseForm();
        }

        /// <summary>
        /// Wait timer has been elapsed.
        /// Used to signal flipping of the please wait animation.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">ElapsedEventArgs data</param>
        private void WaitTimerElapsed(object sender, ElapsedEventArgs e)
        {
            this.waitTimerFlip = 1 - this.waitTimerFlip;
            if (this.waitTimerFlip == 0)
            {
                this.labelPleaseWait.ForeColor = Color.FromArgb((byte)255, (byte)255, (byte)255);
            }
            else
            {
                this.labelPleaseWait.ForeColor = this.flipColor;
            }
        }

        /// <summary>
        /// Splash Screen Click handler
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">EventArgs data</param>
        private void SplashScreen_Click(object sender, EventArgs e)
        {
            if (this.ShowMainForm)
            {
                this.CloseForm();
            }
        }

        /// <summary>
        /// Fade Timer handler for fading in and out of the form.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">EventArgs data</param>
        private void FadeTimerTick(object sender, EventArgs e)
        {
            if (!this.fadeOut)
            {
                if (this.Opacity < 1)
                {
                    this.Opacity = Math.Min(1.0F, this.Opacity + this.fadeInInterval);
                }
                else
                {
                    this.fadeTimer.Stop();
                }
            }
            else
            {
                if (this.Opacity > 0)
                {
                    this.Opacity = Math.Max(0.0F, this.Opacity - this.fadeOutInterval);
                }
                else
                {
                    this.waitTimer.Stop();
                    this.fadeTimer.Stop();
                    this.Close();
                }
            }
        }
    }
}