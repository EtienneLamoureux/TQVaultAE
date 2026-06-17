using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using TQVaultAE.GUI.Models;
using TQVaultAE.Logs;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI;

/// <summary>
/// Main Dialog class
/// </summary>
public partial class MainForm
{
	#region Mainform Events

	/// <summary>
	/// Handler for the ResizeEnd event.  Used to scale the internal controls after the window has been resized.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	protected override void ResizeEndCallback(object sender, EventArgs e)
		// That override Look dumb but needed by Visual Studio WInform Designer
		=> base.ResizeEndCallback(sender, e);

	/// <summary>
	/// Handler for the Resize event.  Used for handling the maximize and minimize functions.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	protected override void ResizeBeginCallback(object sender, EventArgs e)
		// That override Look dumb but needed by Visual Studio WInform Designer
		=> base.ResizeBeginCallback(sender, e);

	/// <summary>
	/// Handler for closing the main form
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">CancelEventArgs data</param>
	private void MainFormClosing(object sender, CancelEventArgs e)
		=> e.Cancel = !this.DoCloseStuff();

	/// <summary>
	/// Shows things that you may want to know before a close.
	/// Like holding an item
	/// </summary>
	/// <returns>TRUE if none of the conditions exist or the user selected to ignore the message</returns>
	private bool DoCloseStuff()
	{
		bool ok = false;
		try
		{
			// Make sure we are not dragging anything
			if (this.DragInfo.IsActive)
			{
				MessageBox.Show(Resources.MainFormHoldingItem, Resources.MainFormHoldingItem2, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				return false;
			}

			this.SaveAllModifiedFiles();

			// Added by VillageIdiot
			this.SaveConfiguration();

			this.GameFileService.GitAddCommitTagAndPush();

			ok = true;
		}
		catch (IOException exception)
		{
			Log.LogError(exception, "Save files failed !");
			MessageBox.Show(Log.FormatException(exception), Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, RightToLeftOptions);
		}

		return ok;
	}

	/// <summary>
	/// Handler for loading the main form
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void MainFormLoad(object sender, EventArgs e)
	{
		// Sync git local repo first
		this.GameFileService.GitRepositorySetup();

		this.splashScreen = this.ServiceProvider.GetService<SplashScreenForm>();
		this.splashScreen.MaximumValue = 1;
		this.splashScreen.FormClosed += new FormClosedEventHandler(this.SplashScreenClosed);

		if (USettings.LoadAllFiles)
			this.splashScreen.MaximumValue += LoadAllFilesTotal();

		this.splashScreen.Show();
		this.splashScreen.Update();
		this.splashScreen.BringToFront();

		this.backgroundWorkerLoadAllFiles.RunWorkerAsync();
	}

	/// <summary>
	/// Handler for key presses on the main form
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">KeyPressEventArgs data</param>
	private void MainFormKeyPress(object sender, KeyPressEventArgs e)
	{
		// Don't handle this here since we handle key presses within each component.
		////if (e.KeyChar != (char)27)
		////e.Handled = true;
	}

	/// <summary>
	/// Handler for showing the main form.
	/// Used to switch focus to the search text box.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void MainFormShown(object sender, EventArgs e)
	{
		this.scalingTextBoxHighlight.Dock = DockStyle.Fill;
		this.scalingTextBoxHighlight.Focus();
	}

	/// <summary>
	/// Handler for moving the mouse wheel.
	/// Used to scroll through the vault list.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">MouseEventArgs data</param>
	private void MainFormMouseWheel(object sender, MouseEventArgs e)
	{
		// Force a single line regardless of the delta value.
		int numberOfTextLinesToMove = ((e.Delta > 0) ? 1 : 0) - ((e.Delta < 0) ? 1 : 0);
		if (numberOfTextLinesToMove != 0)
		{
			int vaultSelection = this.vaultListComboBox.SelectedIndex;
			vaultSelection -= numberOfTextLinesToMove;
			if (vaultSelection < 1)
				vaultSelection = 1;

			if (vaultSelection >= this.vaultListComboBox.Items.Count)
				vaultSelection = this.vaultListComboBox.Items.Count - 1;

			this.vaultListComboBox.SelectedIndex = vaultSelection;
		}
	}

	/// <summary>
	/// Key Handler for the main form.  Most keystrokes should be handled by the individual panels or the search text box.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">KeyEventArgs data</param>
	private void MainFormKeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyData == (Keys.Control | Keys.C))
		{
			HandleClipboardExport();
			return;
		}

		if (e.KeyData == (Keys.Control | Keys.V))
		{
			_ = HandleClipboardImportAsync();
			return;
		}

		if (e.KeyData == (Keys.Control | Keys.F))
			this.ActivateSearchCallback(this, new SackPanelEventArgs(null, null));

		if (e.KeyData == (Keys.Control | Keys.Add) || e.KeyData == (Keys.Control | Keys.Oemplus))
			this.ResizeFormCallback(this, new ResizeEventArgs(0.1F));

		if (e.KeyData == (Keys.Control | Keys.Subtract) || e.KeyData == (Keys.Control | Keys.OemMinus))
			this.ResizeFormCallback(this, new ResizeEventArgs(-0.1F));

		if (e.KeyData == (Keys.Control | Keys.Home))
			this.ResizeFormCallback(this, new ResizeEventArgs(1.0F));
	}


	/// <summary>
	/// Handles Timer ticks for fading in the main form.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void FadeInTimerTick(object sender, EventArgs e)
	{
		if (this.Opacity < 1)
			this.Opacity = Math.Min(1.0F, this.Opacity + this.fadeInterval);
		else
			this.fadeInTimer.Stop();
	}

	/// <summary>
	/// Handler for the exit button.
	/// Closes the main form
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void ExitButtonClick(object sender, EventArgs e) => this.Close();

	#endregion

	#region About

	/// <summary>
	/// Handler for clicking the about button.
	/// Shows the about dialog box.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void AboutButtonClick(object sender, EventArgs e)
	{
		AboutBox dlg = this.ServiceProvider.GetService<AboutBox>();
		dlg.Scale(new SizeF(UIService.Scale, UIService.Scale));
		dlg.ShowDialog();
	}

	#endregion

	#region Scaling

	/// <summary>
	/// Scales the main form according to the scale factor.
	/// </summary>
	/// <param name="scaleFactor">Float which signifies the scale factor of the form.  This is an absolute from the original size unless useRelativeScaling is set to true.</param>
	/// <param name="useRelativeScaling">Indicates whether the scale factor is relative.  Used to support a resize operation.</param>
	protected override void ScaleForm(float scaleFactor, bool useRelativeScaling)
	{
		base.ScaleForm(scaleFactor, useRelativeScaling);

		this.NotificationText.Text = string.Empty;

		this.Invalidate();
	}

	/// <summary>
	/// Sets the size of the main form along with scaling the internal controls for startup.
	/// </summary>
	private void SetupFormSize()
	{
		this.DrawCustomBorder = true;
		this.ResizeCustomAllowed = true;
		this.fadeInterval = USettings.FadeInInterval;

		Rectangle workingArea = Screen.FromControl(this).WorkingArea;

		this.ScaleOnResize = false;

		this.ClientSize = InitialScaling(workingArea);

		this.ScaleOnResize = true;

		UIService.Scale = USettings.Scale;
		this.Log.LogDebug("Config.Settings.Default.Scale changed to {0} !", UIService.Scale);

		// Save the height / width ratio for resizing.
		this.FormDesignRatio = (float)this.Height / (float)this.Width;
		this.FormMaximumSize = new Size(this.Width * 2, this.Height * 2);
		this.FormMinimumSize = new Size(
			Convert.ToInt32((float)this.Width * 0.4F),
			Convert.ToInt32((float)this.Height * 0.4F));

		this.OriginalFormSize = this.Size;
		this.OriginalFormScale = USettings.Scale;

		if (CurrentAutoScaleDimensions.Width != UIService.DESIGNDPI)
		{
			// We do not need to scale the main form controls since autoscaling will handle it.
			// Scale internally to 96 dpi for the drawing functions.
			UIService.Scale = this.CurrentAutoScaleDimensions.Width / UIService.DESIGNDPI;
			this.OriginalFormScale = UIService.Scale;
		}

		this.LastFormSize = this.Size;

		// Set the maximized size but keep the aspect ratio.
		if (Convert.ToInt32((float)workingArea.Width * this.FormDesignRatio) < workingArea.Height)
		{
			this.MaximizedBounds = new Rectangle(0
				, (workingArea.Height - Convert.ToInt32((float)workingArea.Width * this.FormDesignRatio)) / 2
				, workingArea.Width
				, Convert.ToInt32((float)workingArea.Width * this.FormDesignRatio)
			);
		}
		else
		{
			this.MaximizedBounds = new Rectangle(
				(workingArea.Width - Convert.ToInt32((float)workingArea.Height / this.FormDesignRatio)) / 2,
				0,
				Convert.ToInt32((float)workingArea.Height / this.FormDesignRatio),
				workingArea.Height);
		}
		this.Location = new Point(workingArea.Left + Convert.ToInt16((workingArea.Width - this.ClientSize.Width) / 2), workingArea.Top + Convert.ToInt16((workingArea.Height - this.ClientSize.Height) / 2));
	}


	#endregion
}
