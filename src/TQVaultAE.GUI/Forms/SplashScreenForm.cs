using TQVaultAE.Presentation.Win32;

//-----------------------------------------------------------------------
// <copyright file="SplashScreenForm.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Forms;

using System;
using System.Drawing;
using System.Timers;
using System.Windows.Forms;
using TQVaultAE.Presentation;

/// <summary>
/// Class for inital form which load the resources.
/// </summary>
internal sealed partial class SplashScreenForm : VaultForm
{
	/// <summary>
	/// Flag to signal color flip on please wait message.
	/// </summary>
	private int waitTimerFlip;

	/// <summary>
	/// Holds the color of the label prior to flipping to white.
	/// </summary>
	private Color flipColor;

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

		this.flipColor = this.labelPleaseWait.ForeColor;
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