//-----------------------------------------------------------------------
// <copyright file="MainForm.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI
{
	using System;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Drawing;
	using System.Globalization;
	using System.IO;
	using System.Reflection;
	using System.Security.Permissions;
	using System.Windows.Forms;
	using Tooltip;
	using TQVaultAE.GUI.Components;
	using TQVaultAE.GUI.Models;
	using TQVaultAE.Data;
	using TQVaultAE.Logs;
	using TQVaultAE.Entities;
	using TQVaultAE.Presentation;
	using TQVaultAE.Config;
	using TQVaultAE.Services;

	/// <summary>
	/// Main Dialog class
	/// </summary>
	internal partial class MainForm : VaultForm
	{
		private readonly log4net.ILog Log = null;

		#region	Fields

		/// <summary>
		/// Indicates whether the database resources have completed loading.
		/// </summary>
		private bool resourcesLoaded;

		/// <summary>
		/// Indicates whether the entire load process has completed.
		/// </summary>
		private bool loadingComplete;

		/// <summary>
		/// Instance of the player panel control
		/// </summary>
		private PlayerPanel playerPanel;

		/// <summary>
		/// Holds the last sack that had mouse focus
		/// Used for Automoving
		/// </summary>
		private SackCollection lastSackHighlighted;

		/// <summary>
		/// Holds the last sack panel that had mouse focus
		/// Used for Automoving
		/// /// </summary>
		private SackPanel lastSackPanelHighlighted;

		/// <summary>
		/// Info for the current item being dragged by the mouse
		/// </summary>
		private ItemDragInfo dragInfo;

		/// <summary>
		/// Holds the coordinates of the last drag item
		/// </summary>
		private Point lastDragPoint;

		/// <summary>
		/// User current data context
		/// </summary>
		private SessionContext userContext = new SessionContext();

		/// <summary>
		/// Instance of the vault panel control
		/// </summary>
		private VaultPanel vaultPanel;

		/// <summary>
		/// Instance of the second vault panel control
		/// This gets toggled with the player panel
		/// </summary>
		private VaultPanel secondaryVaultPanel;

		/// <summary>
		/// Instance of the stash panel control
		/// </summary>
		private StashPanel stashPanel;

		/// <summary>
		/// Holds that last stash that had focus
		/// </summary>
		private Stash lastStash;

		/// <summary>
		/// Bag number of the last bag with focus
		/// </summary>
		private int lastBag;

		/// <summary>
		/// Instance of the Action Button
		/// This is the animated button which pops relics and separates stacks
		/// </summary>
		private ActionButton actionButton;

		/// <summary>
		/// Holds the current program version
		/// </summary>
		private string currentVersion;

		/// <summary>
		/// Instance of the popup tool tip.
		/// </summary>
		private TTLib tooltip;

		/// <summary>
		/// Text in the tool tip
		/// </summary>
		private string tooltipText;

		/// <summary>
		/// Signals that the configuration UI was loaded and the user changed something in there.
		/// </summary>
		private bool configChanged;

		/// <summary>
		/// Flag which holds whether we are showing the secondary vault panel or the player panel.
		/// </summary>
		private bool showSecondaryVault;

		/// <summary>
		/// Form for the splash screen.
		/// </summary>
		private SplashScreenForm splashScreen;

		/// <summary>
		/// Holds the opacity interval for fading the form.
		/// </summary>
		private double fadeInterval;

		#endregion

		/// <summary>
		/// Initializes a new instance of the MainForm class.
		/// </summary>
		[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
		public MainForm()
		{
			Log = Logger.Get(this);
			Log.Info("TQVaultAE Initialization !");

			this.Enabled = false;
			this.ShowInTaskbar = false;
			this.Opacity = 0;
			this.Hide();

			this.InitializeComponent();

			this.SetupFormSize();

			#region Apply custom font & scaling

			this.exitButton.Font = FontHelper.GetFontAlbertusMTLight(12F, UIService.UI.Scale);
			ScaleControl(this.exitButton);
			this.characterComboBox.Font = FontHelper.GetFontAlbertusMTLight(13F, UIService.UI.Scale);
			ScaleControl(this.characterComboBox);
			ScaleControl(this.characterLabel);
			ScaleControl(this.itemTextPanel);
			ScaleControl(this.itemText);
			this.vaultListComboBox.Font = FontHelper.GetFontAlbertusMTLight(13F, UIService.UI.Scale);
			ScaleControl(this.vaultListComboBox);
			this.vaultLabel.Font = FontHelper.GetFontAlbertusMTLight(11F, UIService.UI.Scale);
			ScaleControl(this.vaultLabel);
			this.configureButton.Font = FontHelper.GetFontAlbertusMTLight(12F, UIService.UI.Scale);
			ScaleControl(this.configureButton);
			this.customMapText.Font = FontHelper.GetFontAlbertusMT(11.25F, UIService.UI.Scale);
			ScaleControl(this.customMapText);
			this.panelSelectButton.Font = FontHelper.GetFontAlbertusMTLight(12F, UIService.UI.Scale);
			ScaleControl(this.panelSelectButton);
			this.secondaryVaultListComboBox.Font = FontHelper.GetFontAlbertusMTLight(11F, UIService.UI.Scale);
			ScaleControl(this.secondaryVaultListComboBox);
			this.aboutButton.Font = FontHelper.GetFontMicrosoftSansSerif(8.25F, UIService.UI.Scale);
			ScaleControl(this.aboutButton);
			this.titleLabel.Font = FontHelper.GetFontAlbertusMTLight(24F, UIService.UI.Scale);
			ScaleControl(this.titleLabel);
			this.searchButton.Font = FontHelper.GetFontAlbertusMTLight(12F, UIService.UI.Scale);
			ScaleControl(this.searchButton);

			#endregion

			try
			{
				this.tooltip = new TTLib();
			}
			catch (Exception ex)
			{
				Log.Error("Get TTLib fail !", ex);
				MessageBox.Show(Log.FormatException(ex));
				// Handle failure to create to tooltip here
				// VXPlib not registered
				checkVXPlibrary();
				this.tooltip = new TTLib();
			}

			// Changed to a global for versions in tqdebug
			AssemblyName aname = Assembly.GetExecutingAssembly().GetName();
			this.currentVersion = aname.Version.ToString();
			this.Text = string.Format(CultureInfo.CurrentCulture, "{0} {1}", aname.Name, this.currentVersion);

			if (TQDebug.DebugEnabled)
			{
				// Write this version into the debug file.
				Log.DebugFormat(CultureInfo.InvariantCulture, "Current TQVault Version: {0}", this.currentVersion);
				Log.Debug(string.Empty);
				Log.Debug("Debug Levels");
				Log.DebugFormat(CultureInfo.InvariantCulture, "ARCFileDebugLevel: {0}", TQDebug.ArcFileDebugLevel);
				Log.DebugFormat(CultureInfo.InvariantCulture, "DatabaseDebugLevel: {0}", TQDebug.DatabaseDebugLevel);
				Log.DebugFormat(CultureInfo.InvariantCulture, "ItemAttributesDebugLevel: {0}", TQDebug.ItemAttributesDebugLevel);
				Log.DebugFormat(CultureInfo.InvariantCulture, "ItemDebugLevel: {0}", TQDebug.ItemDebugLevel);
				Log.Debug(string.Empty);
			}


			// Setup localized strings.
			this.characterLabel.Text = Resources.MainFormLabel1;
			this.vaultLabel.Text = Resources.MainFormLabel2;
			this.configureButton.Text = Resources.MainFormBtnConfigure;
			this.exitButton.Text = Resources.GlobalExit;
			this.panelSelectButton.Text = Resources.MainFormBtnPanelSelect;
			this.Icon = Resources.TQVIcon;
			this.searchButton.Text = Resources.MainFormSearchButtonText;

			// Set up Item strings
			Item.ItemWith = Resources.ItemWith;
			Item.ItemRelicBonus = Resources.ItemRelicBonus;
			Item.ItemRelicCompleted = Resources.ItemRelicCompleted;
			Item.ItemQuest = Resources.ItemQuest;
			Item.ItemSeed = Resources.ItemSeed;
			Item.ItemIT = Resources.ItemIT;
			Item.ItemRagnarok = Resources.ItemRagnarok;
			Item.ItemAtlantis = Resources.ItemAtlantis;
			Item.ShowSkillLevel = Config.Settings.Default.ShowSkillLevel;

			if (Config.Settings.Default.NoToolTipDelay)
			{
				this.tooltip.SetNoDelay();
			}

			this.lastDragPoint.X = -1;
			this.dragInfo = new ItemDragInfo();

			this.CreatePanels();

			// Process the mouse scroll wheel to cycle through the vaults.
			this.MouseWheel += new MouseEventHandler(this.MainFormMouseWheel);

			this.splashScreen = new SplashScreenForm();
			this.splashScreen.MaximumValue = 1;
			this.splashScreen.FormClosed += new FormClosedEventHandler(this.SplashScreenClosed);

			if (Config.Settings.Default.LoadAllFiles)
			{
				this.splashScreen.MaximumValue += LoadAllFilesTotal();
			}

			this.splashScreen.Show();
			this.splashScreen.Update();
		}

		#region VXPlibrary

		/// <summary>
		/// Handles the registering and placement of the VXPlibrary.dll for tooltip creation.
		/// </summary>
		private void checkVXPlibrary()
		{
			if (File.Exists(Directory.GetCurrentDirectory() + "\\VXPlibrary.dll"))
			{
				// VXPlibrary.dll exists, but if it is not registered --> register it
				registerVXPlibrary();
			}
			else
			{
				var ex = new FileNotFoundException("VXPlibrary.dll missing from TQVault directory!");
				Log.ErrorException(ex);
				throw ex;
			}
		}

		/// <summary>
		/// Registers the VXPlibrary.dll
		/// </summary>
		private void registerVXPlibrary()
		{
			Process proc = new Process();
			ProcessStartInfo info = new ProcessStartInfo();
			info.UseShellExecute = true;
			info.WorkingDirectory = "\"" + Directory.GetCurrentDirectory() + "\"";
			info.FileName = Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\Regsvr32.exe";
			info.Verb = "runas";
			info.Arguments = "/c \"" + Directory.GetCurrentDirectory() + "\\VXPlib.dll\"";
			try
			{
				proc.StartInfo = info;
				proc.Start();
				proc.WaitForExit();
			}
			catch (Exception ex)
			{
				Log.ErrorException(ex);
				MessageBox.Show(Log.FormatException(ex));
			}
		}

		#endregion

		#region Mainform Events

		/// <summary>
		/// Handler for the ResizeEnd event.  Used to scale the internal controls after the window has been resized.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		protected override void ResizeEndCallback(object sender, EventArgs e)
		{
			// That override Look dumb but needed by Visual Studio WInform Designer
			base.ResizeEndCallback(sender, e);
		}

		/// <summary>
		/// Handler for the Resize event.  Used for handling the maximize and minimize functions.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		protected override void ResizeBeginCallback(object sender, EventArgs e)
		{
			// That override Look dumb but needed by Visual Studio WInform Designer
			base.ResizeBeginCallback(sender, e);
		}

		/// <summary>
		/// Handler for closing the main form
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">CancelEventArgs data</param>
		private void MainFormClosing(object sender, CancelEventArgs e)
		{
			if (this.DoCloseStuff())
			{
				e.Cancel = false;
			}
			else
			{
				e.Cancel = true;
			}
		}

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
				if (this.dragInfo.IsActive)
				{
					MessageBox.Show(Resources.MainFormHoldingItem, Resources.MainFormHoldingItem2, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, RightToLeftOptions);
					return false;
				}

				this.SaveAllModifiedFiles();

				// Added by VillageIdiot
				this.SaveConfiguration();

				ok = true;
			}
			catch (IOException exception)
			{
				Log.Error("Save files failed !", exception);
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
			this.backgroundWorker1.RunWorkerAsync();
		}

		/// <summary>
		/// Handler for key presses on the main form
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">KeyPressEventArgs data</param>
		private void MainFormKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar != (char)27)
			{
				e.Handled = true;
			}
		}

		/// <summary>
		/// Handler for showing the main form.
		/// Used to switch focus to the search text box.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void MainFormShown(object sender, EventArgs e)
		{
			this.vaultPanel.SackPanel.Focus();
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
				{
					vaultSelection = 1;
				}

				if (vaultSelection >= this.vaultListComboBox.Items.Count)
				{
					vaultSelection = this.vaultListComboBox.Items.Count - 1;
				}

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
			if (e.KeyData == (Keys.Control | Keys.F))
			{
				this.ActivateSearchCallback(this, new SackPanelEventArgs(null, null));
			}

			if (e.KeyData == (Keys.Control | Keys.Add) || e.KeyData == (Keys.Control | Keys.Oemplus))
			{
				this.ResizeFormCallback(this, new ResizeEventArgs(0.1F));
			}

			if (e.KeyData == (Keys.Control | Keys.Subtract) || e.KeyData == (Keys.Control | Keys.OemMinus))
			{
				this.ResizeFormCallback(this, new ResizeEventArgs(-0.1F));
			}

			if (e.KeyData == (Keys.Control | Keys.Home))
			{
				this.ResizeFormCallback(this, new ResizeEventArgs(1.0F));
			}
		}

		/// <summary>
		/// Handles Timer ticks for fading in the main form.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void FadeInTimerTick(object sender, EventArgs e)
		{
			if (this.Opacity < 1)
			{
				this.Opacity = Math.Min(1.0F, this.Opacity + this.fadeInterval);
			}
			else
			{
				this.fadeInTimer.Stop();
			}
		}

		/// <summary>
		/// Handler for the exit button.
		/// Closes the main form
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void ExitButtonClick(object sender, EventArgs e)
		{
			this.Close();
		}

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
			AboutBox dlg = new AboutBox();
			dlg.Scale(new SizeF(UIService.UI.Scale, UIService.UI.Scale));
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

			this.itemText.Text = string.Empty;

			// hide the tooltip
			this.tooltipText = null;
			this.tooltip.ChangeText(this.tooltipText);

			this.Invalidate();
		}

		/// <summary>
		/// Sets the size of the main form along with scaling the internal controls for startup.
		/// </summary>
		private void SetupFormSize()
		{
			this.DrawCustomBorder = true;
			this.ResizeCustomAllowed = true;
			this.fadeInterval = Config.Settings.Default.FadeInInterval;

			Rectangle workingArea = Screen.FromControl(this).WorkingArea;

			int formWidth = 1350;
			int formHeight = 910;
			float initialScale = 1.0F;
			Config.Settings.Default.Scale = initialScale;

			// Ninakoru: trick to force close/min/max buttons to reposition...
			this.ScaleOnResize = false;
			if (workingArea.Width < formWidth || workingArea.Height < formHeight)
			{
				initialScale = Math.Min(Convert.ToSingle(workingArea.Width) / Convert.ToSingle(formWidth), Convert.ToSingle(workingArea.Height) / Convert.ToSingle(formHeight));

				if (Config.Settings.Default.Scale > initialScale)
				{
					Config.Settings.Default.Scale = initialScale;
				}

				this.ClientSize = new System.Drawing.Size((int)System.Math.Round(formWidth * Config.Settings.Default.Scale), (int)System.Math.Round(formHeight * Config.Settings.Default.Scale));
			}
			else
			{
				this.ClientSize = new System.Drawing.Size(formWidth, formHeight);
			}

			this.ScaleOnResize = true;

			UIService.UI.Scale = Config.Settings.Default.Scale;

			Config.Settings.Default.Save();

			// Save the height / width ratio for resizing.
			this.FormDesignRatio = (float)this.Height / (float)this.Width;
			this.FormMaximumSize = new Size(this.Width * 2, this.Height * 2);
			this.FormMinimumSize = new Size(
				Convert.ToInt32((float)this.Width * 0.4F),
				Convert.ToInt32((float)this.Height * 0.4F));

			this.OriginalFormSize = this.Size;
			this.OriginalFormScale = Config.Settings.Default.Scale;

			if (CurrentAutoScaleDimensions.Width != UIService.DESIGNDPI)
			{
				// We do not need to scale the main form controls since autoscaling will handle it.
				// Scale internally to 96 dpi for the drawing functions.
				UIService.UI.Scale = this.CurrentAutoScaleDimensions.Width / UIService.DESIGNDPI;
				this.OriginalFormScale = UIService.UI.Scale;
			}

			this.LastFormSize = this.Size;

			// Set the maximized size but keep the aspect ratio.
			if (Convert.ToInt32((float)workingArea.Width * this.FormDesignRatio) < workingArea.Height)
			{
				this.MaximizedBounds = new Rectangle(
					0,
					(workingArea.Height - Convert.ToInt32((float)workingArea.Width * this.FormDesignRatio)) / 2,
					workingArea.Width,
					Convert.ToInt32((float)workingArea.Width * this.FormDesignRatio));
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


		#region Files


		/// <summary>
		/// Counts the number of files which LoadAllFiles will load.  Used to set the max value of the progress bar.
		/// </summary>
		/// <returns>Total number of files that LoadAllFiles() will load.</returns>
		private static int LoadAllFilesTotal()
		{
			string[] list;

			list = TQData.GetCharacterList();
			int numIT = list?.Length ?? 0;

			list = TQData.GetVaultList();
			int numVaults = list?.Length ?? 0;

			return Math.Max(0, numIT + numIT + numVaults - 1);
		}

		/// <summary>
		/// Loads all of the players, stashes, and vaults.
		/// Shows a progress dialog.
		/// Used for the searching function.
		/// </summary>
		private void LoadAllFiles()
		{
			// Check to see if we failed the last time we tried loading all of the files.
			// If we did fail then turn it off and skip it.
			if (!Config.Settings.Default.LoadAllFilesCompleted)
			{
				if (MessageBox.Show(
					Resources.MainFormDisableLoadAllFiles,
					Resources.MainFormDisableLoadAllFilesCaption,
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Information,
					MessageBoxDefaultButton.Button1,
					RightToLeftOptions) == DialogResult.Yes)
				{
					Config.Settings.Default.LoadAllFilesCompleted = true;
					Config.Settings.Default.LoadAllFiles = false;
					Config.Settings.Default.Save();
					return;
				}
			}

			string[] vaults = TQData.GetVaultList();

			string[] charactersIT = TQData.GetCharacterList();

			int numIT = charactersIT?.Length ?? 0;

			int numVaults = vaults?.Length ?? 0;

			// Since this takes a while, show a progress dialog box.
			int total = numIT + numIT + numVaults - 1;

			if (total > 0)
			{
				// We were successful last time so we reset the flag for this attempt.
				Config.Settings.Default.LoadAllFilesCompleted = false;
				Config.Settings.Default.Save();
			}
			else
				return;

			// Load all of the Immortal Throne player files and stashes.
			for (int i = 0; i < numIT; ++i)
			{
				// Get the player & player's stash
				try
				{
					var result = this.playerService.LoadPlayer(charactersIT[i], true);

					if (result.PlayerArgumentException != null)
					{
						string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, result.PlayerFile, result.PlayerArgumentException.Message);
						MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
					}
					if (result.StashArgumentException != null)
					{
						string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, result.StashFile, result.StashArgumentException.Message);
						MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
					}
				}
				catch (IOException exception)
				{
					Log.ErrorException(exception);
					MessageBox.Show(Log.FormatException(exception), Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				}

				this.backgroundWorker1.ReportProgress(1);
			}

			// Load all of the vaults.
			for (int i = 0; i < numVaults; ++i)
			{
				var result = this.vaultService.LoadVault(vaults[i]);
				if (result.ArgumentException != null)
				{
					string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, result.Filename, result.ArgumentException.Message);
					MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				}

				this.backgroundWorker1.ReportProgress(1);
			}

			// We made it so set the flag to indicate we were successful.
			Config.Settings.Default.LoadAllFilesCompleted = true;
			Config.Settings.Default.Save();
		}

		/// <summary>
		/// Attempts to save all modified files.
		/// </summary>
		/// <returns>true if players have been modified</returns>
		private bool SaveAllModifiedFiles()
		{
			bool playersModified = this.SaveAllModifiedPlayers();
			this.SaveAllModifiedVaults();
			this.SaveAllModifiedStashes();
			return playersModified;
		}

		#endregion

		#region SplashScreen & Tooltip

		/// <summary>
		/// Handler for closing the splash screen
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">FormClosedEventArgs data</param>
		private void SplashScreenClosed(object sender, FormClosedEventArgs e)
		{
			if (this.resourcesLoaded)
			{
				if (!this.loadingComplete)
				{
					this.backgroundWorker1.CancelAsync();
				}

				this.ShowMainForm();

				// the tooltip must be initialized after the main form is shown and active.
				this.tooltip.Initialize(this);
				this.tooltip.ActivateCallback = new TTLibToolTipActivate(this.ToolTipCallback);
			}
			else
			{
				Application.Exit();
			}
		}

		/// <summary>
		/// Starts the fade in of the main form.
		/// </summary>
		private void ShowMainForm()
		{
			this.fadeInTimer.Start();
			this.ShowInTaskbar = true;
			this.Enabled = true;
			this.Show();
			this.Activate();
		}


		/// <summary>
		/// Tooltip callback
		/// </summary>
		/// <param name="windowHandle">handle of the main window form</param>
		/// <returns>tooltip string</returns>
		private string ToolTipCallback(int windowHandle)
		{
			string answer;

			answer = this.vaultPanel.ToolTipCallback(windowHandle);
			if (answer != null)
			{
				return answer;
			}

			answer = this.playerPanel.ToolTipCallback(windowHandle);
			if (answer != null)
			{
				return answer;
			}

			answer = this.stashPanel.ToolTipCallback(windowHandle);
			if (answer != null)
			{
				return answer;
			}

			answer = this.secondaryVaultPanel.ToolTipCallback(windowHandle);
			if (answer != null)
			{
				return answer;
			}

			// Changed by VillageIdiot
			// If we are dragging something around, clear the tooltip and text box.
			if (this.dragInfo.IsActive)
			{
				this.itemText.Text = string.Empty;
				this.tooltipText = null;
				this.tooltip.ChangeText(this.tooltipText);
				return null;
			}

			// If nothing else returned a tooltip then display the current item text
			return this.tooltipText;
		}
		#endregion

		#region Game Resources

		/// <summary>
		/// Loads the resources.
		/// </summary>
		/// <param name="worker">Background worker</param>
		/// <param name="e">DoWorkEventArgs data</param>
		/// <returns>true when resource loading has completed successfully</returns>
		private bool LoadResources(BackgroundWorker worker, DoWorkEventArgs e)
		{
			// Abort the operation if the user has canceled.
			// Note that a call to CancelAsync may have set
			// CancellationPending to true just after the
			// last invocation of this method exits, so this
			// code will not have the opportunity to set the
			// DoWorkEventArgs.Cancel flag to true. This means
			// that RunWorkerCompletedEventArgs.Cancelled will
			// not be set to true in your RunWorkerCompleted
			// event handler. This is a race condition.
			if (worker.CancellationPending)
			{
				e.Cancel = true;
				return this.resourcesLoaded;
			}
			else
			{
				//read map name from ini file, main section
				if (!String.IsNullOrEmpty(Config.Settings.Default.Mod))
				{
					TQData.MapName = Config.Settings.Default.Mod;
				}

				if (!Config.Settings.Default.AllowCheats)
				{
					Config.Settings.Default.AllowItemCopy = false;
					Config.Settings.Default.AllowItemEdit = false;
					Config.Settings.Default.AllowCharacterEdit = false;
				}

				CommandLineArgs args = new CommandLineArgs();

				// Check to see if we loaded something from the command line.
				if (args.HasMapName)
				{
					TQData.MapName = args.MapName;
				}

				this.resourcesLoaded = true;
				this.backgroundWorker1.ReportProgress(1);

				if (Config.Settings.Default.LoadAllFiles)
				{
					this.LoadAllFiles();
				}

				// Notify the form that the resources are loaded.
				return true;
			}
		}

		/// <summary>
		/// Background worker call to load the resources.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">DoWorkEventArgs data</param>
		private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
		{
			// Get the BackgroundWorker that raised this event.
			BackgroundWorker worker = sender as BackgroundWorker;

			// Assign the result of the resource loader
			// to the Result property of the DoWorkEventArgs
			// object. This is will be available to the
			// RunWorkerCompleted eventhandler.
			e.Result = this.LoadResources(worker, e);
		}

		/// <summary>
		/// Background worker has finished
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">RunWorkerCompletedEventArgs data</param>
		private void BackgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			// First, handle the case where an exception was thrown.
			if (e.Error != null)
			{
				if (MessageBox.Show(
					string.Concat(e.Error.Message, Resources.Form1BadLanguage),
					Resources.Form1ErrorLoadingResources,
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Exclamation,
					MessageBoxDefaultButton.Button1,
					RightToLeftOptions) == DialogResult.Yes)
				{
					Application.Restart();
				}
				else
				{
					Application.Exit();
				}
			}
			else if (e.Cancelled && !this.resourcesLoaded)
			{
				Application.Exit();
			}
			else if (e.Result.Equals(true))
			{
				this.loadingComplete = true;
				this.Enabled = true;
				this.LoadTransferStash();
				this.LoadRelicVaultStash();

				// Load last character here if selected
				if (Config.Settings.Default.LoadLastCharacter)
				{
					int ind = this.characterComboBox.FindStringExact(Config.Settings.Default.LastCharacterName);
					if (ind != -1)
					{
						this.characterComboBox.SelectedIndex = ind;
					}
				}

				string currentVault = VaultService.MAINVAULT;

				// See if we should load the last loaded vault
				if (Config.Settings.Default.LoadLastVault)
				{
					currentVault = Config.Settings.Default.LastVaultName;

					// Make sure there is something in the config file to load else load the Main Vault
					// We do not want to create new here.
					if (string.IsNullOrEmpty(currentVault) || !File.Exists(TQData.GetVaultFile(currentVault)))
					{
						currentVault = VaultService.MAINVAULT;
					}
				}

				this.vaultListComboBox.SelectedItem = currentVault;

				// Finally load Vault
				this.LoadVault(currentVault, false);

				this.splashScreen.UpdateText();
				this.splashScreen.ShowMainForm = true;

				CommandLineArgs args = new CommandLineArgs();

				// Allows skipping of title screen with setting
				if (args.IsAutomatic || Config.Settings.Default.SkipTitle == true)
				{
					string player = args.Player;
					int index = this.characterComboBox.FindStringExact(player);
					if (index != -1)
					{
						this.characterComboBox.SelectedIndex = index;
					}

					this.splashScreen.CloseForm();
				}
			}
			else
			{
				// If for some reason the loading failed, but there was no error raised.
				MessageBox.Show(
					Resources.Form1ErrorLoadingResources,
					Resources.Form1ErrorLoadingResources,
					MessageBoxButtons.OK,
					MessageBoxIcon.Exclamation,
					MessageBoxDefaultButton.Button1,
					RightToLeftOptions);
				Application.Exit();
			}
		}

		/// <summary>
		/// Handler for updating the splash screen progress bar.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">ProgressChangedEventArgs data</param>
		private void BackgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			this.splashScreen.IncrementValue();
		}

		#endregion

		#region Settings Dialog

		/// <summary>
		/// Updates configuration settings
		/// </summary>
		private void SaveConfiguration()
		{
			// Update last loaded vault
			if (Config.Settings.Default.LoadLastVault)
			{
				// Changed by VillageIdiot
				// Now check to see if the value is changed since the Main Vault would never auto load
				if (this.vaultListComboBox.SelectedItem != null && this.vaultListComboBox.SelectedItem.ToString().ToUpperInvariant() != Config.Settings.Default.LastVaultName.ToUpperInvariant())
				{
					Config.Settings.Default.LastVaultName = this.vaultListComboBox.SelectedItem.ToString();
					this.configChanged = true;
				}
			}

			// Update last loaded character
			if (Config.Settings.Default.LoadLastCharacter)
			{
				// Changed by VillageIdiot
				// Now check the last value to see if it has changed since the logic would
				// always load a character even if no character was selected on the last run
				if (this.characterComboBox.SelectedItem.ToString().ToUpperInvariant() != Config.Settings.Default.LastCharacterName.ToUpperInvariant())
				{
					// Clear the value if no character is selected
					string name = this.characterComboBox.SelectedItem.ToString();
					if (name == Resources.MainFormSelectCharacter)
					{
						name = string.Empty;
					}

					Config.Settings.Default.LastCharacterName = name;
					this.configChanged = true;
				}
			}

			// Update custom map settings
			if (Config.Settings.Default.ModEnabled)
			{
				this.configChanged = true;
			}

			// Clear out the key if we are autodetecting.
			if (Config.Settings.Default.AutoDetectLanguage)
			{
				Config.Settings.Default.TQLanguage = string.Empty;
			}

			// Clear out the settings if auto detecting.
			if (Config.Settings.Default.AutoDetectGamePath)
			{
				Config.Settings.Default.TQITPath = string.Empty;
				Config.Settings.Default.TQPath = string.Empty;
			}

			if (UIService.UI.Scale != 1.0F)
			{
				Config.Settings.Default.Scale = UIService.UI.Scale;
				this.configChanged = true;
			}

			if (this.configChanged)
			{
				Config.Settings.Default.Save();
			}
		}

		/// <summary>
		/// Handler for clicking the configure button.
		/// Shows the Settings Dialog.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void ConfigureButtonClick(object sender, EventArgs e)
		{
			SettingsDialog settingsDialog = new SettingsDialog();
			DialogResult result = DialogResult.Cancel;
			settingsDialog.Scale(new SizeF(UIService.UI.Scale, UIService.UI.Scale));

			string title = string.Empty;
			string message = string.Empty;
			if (settingsDialog.ShowDialog() == DialogResult.OK && settingsDialog.ConfigurationChanged)
			{
				if (settingsDialog.PlayerFilterChanged)
				{
					this.characterComboBox.SelectedItem = Resources.MainFormSelectCharacter;
					if (this.playerPanel.Player != null)
					{
						this.playerPanel.Player = null;
					}

					this.GetPlayerList();
				}

				if (settingsDialog.VaultPathChanged)
				{
					TQData.TQVaultSaveFolder = settingsDialog.VaultPath;
					this.vaultService.UpdateVaultPath(settingsDialog.VaultPath);
					this.GetVaultList(true);
				}

				if (settingsDialog.LanguageChanged || settingsDialog.GamePathChanged || settingsDialog.CustomMapsChanged || settingsDialog.UISettingChanged)
				{
					if ((settingsDialog.GamePathChanged && settingsDialog.LanguageChanged) || settingsDialog.UISettingChanged)
					{
						title = Resources.MainFormSettingsChanged;
						message = Resources.MainFormSettingsChangedMsg;
					}
					else if (settingsDialog.GamePathChanged)
					{
						title = Resources.MainFormPathsChanged;
						message = Resources.MainFormPathsChangedMsg;
					}
					else if (settingsDialog.CustomMapsChanged)
					{
						title = Resources.MainFormMapsChanged;
						message = Resources.MainFormMapsChangedMsg;
					}
					else
					{
						title = Resources.MainFormLanguageChanged;
						message = Resources.MainFormLanguageChangedMsg;
					}

					result = MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				}

				this.configChanged = true;
				this.SaveConfiguration();
				if (result == DialogResult.Yes)
				{
					if (this.DoCloseStuff())
					{
						Application.Restart();
					}
				}
			}
		}


		#endregion

	}
}
