using TQVaultAE.Presentation.Win32;

//-----------------------------------------------------------------------
// <copyright file="SplashScreenForm.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Forms;

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Timers;
using System.Windows.Forms;
using TQVaultAE.Presentation;

/// <summary>
/// Class for inital form which load the resources.
/// </summary>
internal sealed partial class SplashScreenForm : VaultForm
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
	/// Holds the color of the label prior to flipping to white.
	/// </summary>
	private Color flipColor;

	/// <summary>
	/// Indicates whether the form will be drawn with the outer edges clipped to a rounded rectangle.
	/// </summary>
	private bool useRoundedRectangle;

	/// <summary>
	/// GraphicsPath used for clipping to a rounded rectangle.
	/// </summary>
	private GraphicsPath? graphicsPath;

	/// <summary>
	/// Initializes a new instance of the SplashScreenForm class.
	/// </summary>
	public SplashScreenForm(MainForm instance) : base(instance.Services)
	{
		this.Owner = instance;

		this.InitializeComponent();

		// The form follows the DPI, the picture keeps its pixels: stretch it, as a Form tiles its background by default.
		this.BackgroundImageLayout = ImageLayout.Stretch;

		#region Apply custom font

		this.label3.Font = FontService.GetFontLight(12F);
		this.exitButton.Font = FontService.GetFontLight(12F);
		this.labelPleaseWait.Font = FontService.GetFontLight(14.25F);

		#endregion

		this.Text = Resources.Form1Text;
		this.label3.Text = Resources.Form1Label3;
		this.exitButton.Text = Resources.GlobalExit;
		this.labelPleaseWait.Text = Resources.Form1LblPleaseWait;
		this.Icon = Images.TQVIcon;

		this.FormBorderStyle = FormBorderStyle.None;

		this.NormalizeBox = false;
		this.DrawCustomBorder = true;

		if (this.UseRoundedRectangle)
		{
			this.graphicsPath = this.CreatePath(20);
			if (this.graphicsPath is not null)
			{
				this.Region = new Region(this.graphicsPath);
			}
		}

		this.flipColor = this.labelPleaseWait.ForeColor;
	}

	/// <summary>
	/// Gets or sets a value indicating whether the form will be drawn with the outer edges clipped to a rounded rectangle.
	/// </summary>
	public bool UseRoundedRectangle
	{
		get => (this.FormBorderStyle != FormBorderStyle.None || this.DrawCustomBorder) ? false : this.useRoundedRectangle;
		set => this.useRoundedRectangle = value;
	}

	/// <summary>
	/// Gets or sets the maximum value for the progress bar
	/// </summary>
	public int MaximumValue
	{
		get => this.progressBar.Maximum;
		set => this.progressBar.Maximum = value;
	}

	/// <summary>
	/// Increments the progress bar
	/// </summary>
	public void IncrementValue() => this.progressBar.Increment(1);

	/// <summary>
	/// Closes the form.
	/// </summary>
	public void CloseForm()
	{
		this.waitTimer.Stop();
		this.Close();
	}

	/// <summary>
	/// Paint handler.
	/// </summary>
	/// <param name="e">PaintEventArgs data</param>
	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);

		// Paint a black border around the form.
		if (this.UseRoundedRectangle && this.graphicsPath is not null)
		{
			using var pen = new Pen(Color.Black, borderPenWidth);
			e.Graphics.DrawPath(pen, this.graphicsPath);
		}
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
	private void ExitButtonClick(object? sender, EventArgs e)
	{
		this.CloseForm();
	}

	/// <summary>
	/// Wait timer has been elapsed.
	/// Used to signal flipping of the please wait animation.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">ElapsedEventArgs data</param>
	private void WaitTimerElapsed(object? sender, ElapsedEventArgs e)
	{
		this.waitTimerFlip = 1 - this.waitTimerFlip;
		if (this.waitTimerFlip == 0)
			this.labelPleaseWait.ForeColor = Color.FromArgb((byte)255, (byte)255, (byte)255);
		else
			this.labelPleaseWait.ForeColor = this.flipColor;
	}
}