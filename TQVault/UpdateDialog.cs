//-----------------------------------------------------------------------
// <copyright file="UpdateDialog.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVault
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Drawing;
	using System.Globalization;
	using System.IO;
	using System.Net;
	using System.Net.Cache;
	using System.Reflection;
	using System.Security;
	using System.Security.Permissions;
	using System.Windows.Forms;
	using TQVault.Properties;
	using TQVaultData;

	/// <summary>
	/// Class for the UpdateDialog dialog box
	/// </summary>
	internal partial class UpdateDialog : VaultForm
	{
		/// <summary>
		/// MessageBoxOptions for right to left reading.
		/// </summary>
		private static MessageBoxOptions rightToLeftOptions = (MessageBoxOptions)0;

		/// <summary>
		/// Indicates that there was a stream exception
		/// </summary>
		private bool streamException;

		/// <summary>
		/// Indicates whether or not we show a message when everything is up to date
		/// </summary>
		private bool showUpToDateMessage;

		/// <summary>
		/// Indicates if there was an exception getting the updater program version
		/// </summary>
		private bool updaterVersionException;

		/// <summary>
		/// Address of the update server
		/// </summary>
		private string httpAddress;

		/// <summary>
		/// Latest version from the server
		/// </summary>
		private string latestVersion;

		/// <summary>
		/// Minimum version from the server
		/// Used to trigger a full update over a patch
		/// </summary>
		private string minimumVersion;

		/// <summary>
		/// Name of the full setup archive
		/// </summary>
		private string setupArchive;

		/// <summary>
		/// Name of the setup program within the archive
		/// </summary>
		private string setupName;

		/// <summary>
		/// Current version
		/// </summary>
		private string currentVersion;

		/// <summary>
		/// Current update program version
		/// </summary>
		private string currentUpdaterVersion;

		/// <summary>
		/// Update program version from the server
		/// </summary>
		private string updaterVersion;

		/// <summary>
		/// Update program name from the server
		/// </summary>
		private string updaterName;

		/// <summary>
		/// Additional parameters for the update program
		/// </summary>
		private string updaterParameters;

		/// <summary>
		/// List of all of the changes from the server
		/// </summary>
		private List<string> changes;

		/// <summary>
		/// Holds the type of update that will be performed
		/// </summary>
		private UpdateType updateType;

		/// <summary>
		/// Webclient instance
		/// </summary>
		private WebClient client;

		/// <summary>
		/// Initializes a new instance of the UpdateDialog class.
		/// </summary>
		public UpdateDialog()
		{
			this.InitializeComponent();
			this.Text = Resources.UpdateText;
			this.cancelButton.Text = Resources.GlobalCancel;
			this.okayButton.Text = Resources.GlobalOK;

			if (Settings.Default.EnableNewUI)
			{
				this.DrawCustomBorder = true;
			}
			else
			{
				this.Revert(new Size(349, 350));
			}

			// Triggers a message box if the files are up to date.
			// Used when user forces an upate check to inform that there are none available.
			this.client = new WebClient();
			this.client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.Reload);
			this.changes = new List<string>();
			this.httpAddress = Settings.Default.UpdateURL;

			// Set options for Right to Left reading.
			if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
			{
				rightToLeftOptions = MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;
			}
		}

		/// <summary>
		/// Enumerates the different type of updates available
		/// </summary>
		private enum UpdateType
		{
			/// <summary>
			/// No update at all
			/// </summary>
			None = 0,

			/// <summary>
			/// Patch update
			/// </summary>
			Patch,

			/// <summary>
			/// Update program needs update
			/// </summary>
			Updater,

			/// <summary>
			/// Update program needs update and TQVault needs a patch
			/// </summary>
			UpdaterAndPatch,

			/// <summary>
			/// TQVault needs a full installation to perform an update
			/// </summary>
			FullInstall
		}

		/// <summary>
		/// Sets a value indicating whether or not the up to date message will be shown
		/// </summary>
		public bool ShowUpToDateMessage
		{
			set
			{
				this.showUpToDateMessage = value;
			}
		}

		/// <summary>
		/// Downloads the update file from the udpate server and populates the version variables.
		/// </summary>
		[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
		public void CheckForUpdates()
		{
			if (this.client == null)
			{
				return;
			}

			// Get the current version of TQVault.
			Assembly assembly = Assembly.GetExecutingAssembly();
			AssemblyName assemblyName = assembly.GetName();
			this.currentVersion = assemblyName.Version.ToString();

			if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
			{
				// Verify network connection
				Stream stream = this.client.OpenRead(string.Concat(this.httpAddress, "latestversions.txt"));
				try
				{
					// Get our version file.
					using (StreamReader streamReader = new StreamReader(stream))
					{
						string line;
						char delims = '=';

						while ((line = streamReader.ReadLine()) != null)
						{
							// Skip over comments
							if (line.StartsWith(";", StringComparison.OrdinalIgnoreCase))
							{
								continue;
							}

							if (line.StartsWith("!", StringComparison.OrdinalIgnoreCase))
							{
								this.changes.Add(line.Substring(1));
								continue;
							}

							// Split the line on the = sign
							string[] fields = line.Split(delims);
							if (fields.Length < 2)
							{
								continue;
							}

							string key = fields[0].Trim().ToUpperInvariant();
							string val = fields[1].Trim();

							if (key == "LATESTVERSION")
							{
								this.latestVersion = val;
							}
							else if (key == "MINIMUMVERSION")
							{
								this.minimumVersion = val;
							}
							else if (key == "SETUPARCHIVE")
							{
								this.setupArchive = val;
							}
							else if (key == "SETUPNAME")
							{
								this.setupName = val;
							}
							else if (key == "UPDATERVERSION")
							{
								this.updaterVersion = val;
							}
							else if (key == "UPDATERNAME")
							{
								this.updaterName = val;
							}
							else if (key == "UPDATERPARAMS")
							{
								string temp = val.ToUpperInvariant().Replace("<APPPATH>", Application.StartupPath);
								temp = temp.Replace("<TQVAULT>", Application.ExecutablePath);

								this.updaterParameters = temp;
							}
						}
					}
				}
				catch (IOException)
				{
					this.streamException = true;
				}
				catch (WebException)
				{
					this.streamException = true;
				}
				finally
				{
					if (stream != null)
					{
						stream.Close();
					}
				}
			}

			// Get the current version of the update program.
			try
			{
				FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(string.Concat(Application.StartupPath, "\\", this.updaterName));
				this.currentUpdaterVersion = myFileVersionInfo.FileVersion;
				this.updaterVersionException = false;
			}
			catch (IOException exception)
			{
				// Write the exception in the log.
				TQVaultData.TQDebug.DebugWriteLine("File exception when getting updater version info:");
				TQVaultData.TQDebug.DebugWriteLine(exception.Message);

				// Fill in some dummy values an move on.
				this.currentUpdaterVersion = "0.0.0.0";
				this.updaterVersionException = true;
			}
			catch (SecurityException securityException)
			{
				// Write the exception in the log.
				TQVaultData.TQDebug.DebugWriteLine("Security exception when getting updater version info:");
				TQVaultData.TQDebug.DebugWriteLine(securityException.Message);

				// Fill in some dummy values an move on.
				this.currentUpdaterVersion = "0.0.0.0";
				this.updaterVersionException = true;
			}

			// Stick some dummy values in if there is something missing.
			if (this.latestVersion == null)
			{
				this.latestVersion = "0.0.0.0";
			}

			if (this.minimumVersion == null)
			{
				this.minimumVersion = "9.9.9.9";
			}

			if (this.updaterVersion == null)
			{
				this.updaterVersion = "9.9.9.9";
			}

			if (this.currentVersion == null)
			{
				this.currentVersion = "0.0.0.0";
			}

			if (this.currentUpdaterVersion == null)
			{
				this.currentUpdaterVersion = "0.0.0.0";
			}

			// These are the setup defaults which should work unless the setup program is changed.
			if (this.setupArchive == null)
			{
				this.setupArchive = "FullInstall.arc";
			}

			if (this.setupName == null)
			{
				this.setupName = "setup.exe";
			}

			// These are the default values which should work unless the update program is changed.
			if (this.updaterName == null)
			{
				this.updaterName = "SSUpdateTQV.exe";
			}

			if (this.updaterParameters == null)
			{
				this.updaterParameters = string.Concat(" -$", Application.ExecutablePath);
			}

			this.ProcessUpdates();
		}

		/// <summary>
		/// Reverts the form back to the original size and UI style.
		/// </summary>
		/// <param name="originalSize">Original size of the form.</param>
		protected override void Revert(Size originalSize)
		{
			this.DrawCustomBorder = false;
			this.ClientSize = originalSize;

			this.messageTextBox.Font = new Font("Albertus MT", 9.0F, FontStyle.Regular, GraphicsUnit.Point, (byte)0);
			this.messageTextBox.Location = new Point(12, 12);
			this.messageTextBox.Size = new Size(325, 266);

			this.okayButton.Revert(new Point(82, 315), new Size(75, 23));
			this.cancelButton.Revert(new Point(200, 315), new Size(75, 23));
		}

		/// <summary>
		/// Converts the version string to an int.
		/// Version numbers can be up to 2 digits.
		/// For example "1.2.3.4" would return 1020304
		/// </summary>
		/// <param name="version">version string to be converted</param>
		/// <returns>Integer value of the version string</returns>
		private static int ConvertVersionToInt(string version)
		{
			int total = 0;
			if (version == null)
			{
				return total;
			}

			string tempVersion = version;

			// Make sure we have 3 decimal points in our version string.
			if (version.Replace(".", string.Empty).Length != tempVersion.Length - 3)
			{
				return 0;
			}

			for (int i = 0; i < 3; ++i)
			{
				// Find the decimal point
				int location = tempVersion.LastIndexOf(".", StringComparison.OrdinalIgnoreCase);

				// Split the string at the last decimal point
				// Take the number following the decimal and weight it by 100^position
				total += Convert.ToInt32(tempVersion.Substring(location + 1), CultureInfo.InvariantCulture) *
					Convert.ToInt32(Math.Pow(100.0, Convert.ToDouble(i, CultureInfo.InvariantCulture)), CultureInfo.InvariantCulture);

				// Remove the converted digit along with the decimal point.
				tempVersion = tempVersion.Substring(0, location);
			}

			total += Convert.ToInt32(tempVersion, CultureInfo.InvariantCulture) * 1000000;

			return total;
		}

		/// <summary>
		/// Once all of the versions are known process them accordingly.
		/// </summary>
		private void ProcessUpdates()
		{
			this.updateType = UpdateType.None;
			List<string> output = new List<string>();

			// Check to see if we are less than the minimumVersion and then download and run setup.exe.
			if (ConvertVersionToInt(this.currentVersion) < ConvertVersionToInt(this.minimumVersion))
			{
				this.updateType = UpdateType.FullInstall;
				output.Add(Resources.UpdateFullInstall);
				output.Add(string.Format(CultureInfo.CurrentCulture, Resources.UpdateLatestVer, this.latestVersion));
				output.Add(string.Format(CultureInfo.CurrentCulture, Resources.UpdateMinVer, this.minimumVersion));
				output.Add(string.Format(CultureInfo.CurrentCulture, Resources.UpdateCurrentVer, this.currentVersion));

				// Roll in the list of changes.
				if (this.changes.Count > 0)
				{
					output.Add(string.Empty);
					output.AddRange(this.changes);
				}

				output.Add(string.Empty);
				output.Add(Resources.UpdateRunInstall);
				output.Add(string.Empty);
			}
			else if (ConvertVersionToInt(this.updaterVersion) > ConvertVersionToInt(this.currentUpdaterVersion))
			{
				// Check to see if the update program needs an update itself.
				this.updateType = UpdateType.Updater;
			}

			// We are doing a regular patch using the update program.
			if (ConvertVersionToInt(this.latestVersion) > ConvertVersionToInt(this.currentVersion))
			{
				if (this.updateType != UpdateType.FullInstall)
				{
					// Skip this update if we are doing a full installation.
					if (this.updateType == UpdateType.Updater)
					{
						// We need to update both the Update Program and TQVault
						this.updateType = UpdateType.UpdaterAndPatch;
						if (this.updaterVersionException)
						{
							output.Add(Resources.UpdateUpdaterNotFound);
						}
						else
						{
							output.Add(Resources.UpdateNewUpdater);
						}

						output.Add(string.Format(CultureInfo.CurrentCulture, Resources.UpdateLatestVer, this.updaterVersion));
						output.Add(string.Format(CultureInfo.CurrentCulture, Resources.UpdateCurrentVer, this.currentUpdaterVersion));
						output.Add(string.Empty);
						output.Add(Resources.UpdateNewTQVault);
						output.Add(string.Format(CultureInfo.CurrentCulture, Resources.UpdateLatestVer, this.latestVersion));
						output.Add(string.Format(CultureInfo.CurrentCulture, Resources.UpdateCurrentVer, this.currentVersion));

						// Roll in the list of changes.
						if (this.changes.Count > 0)
						{
							output.Add(string.Empty);
							output.AddRange(this.changes);
						}

						output.Add(string.Empty);
						output.Add(Resources.UpdatePerformBoth);
						output.Add(string.Empty);
					}
					else
					{
						// Just a TQVault update.
						this.updateType = UpdateType.Patch;
						output.Add(Resources.UpdateNewTQVault);
						output.Add(string.Format(CultureInfo.CurrentCulture, Resources.UpdateLatestVer, this.latestVersion));
						output.Add(string.Format(CultureInfo.CurrentCulture, Resources.UpdateCurrentVer, this.currentVersion));

						// Roll in the list of changes.
						if (this.changes.Count > 0)
						{
							output.Add(string.Empty);
							output.AddRange(this.changes);
						}

						output.Add(string.Empty);
						output.Add(Resources.UpdatePerform);
						output.Add(string.Empty);
					}
				}
			}
			else if (this.updateType == UpdateType.Updater)
			{
				// We update the text here since there might be a combined update.
				if (this.updaterVersionException)
				{
					output.Add(Resources.UpdateUpdaterNotFound);
				}
				else
				{
					output.Add(Resources.UpdateNewUpdater);
				}

				output.Add(string.Format(CultureInfo.CurrentCulture, Resources.UpdateLatestVer, this.updaterVersion));
				output.Add(string.Format(CultureInfo.CurrentCulture, Resources.UpdateCurrentVer, this.currentUpdaterVersion));

				// Roll in the list of changes.
				if (this.changes.Count > 0)
				{
					output.Add(string.Empty);
					output.AddRange(this.changes);
				}

				output.Add(string.Empty);
				output.Add(Resources.UpdatePerform);
				output.Add(string.Empty);
			}
			else if (this.streamException)
			{
				// This is used when the version file cannot be read.
				output.Clear();
				output.Add(Resources.UpdateStreamError);
				output.Add(string.Empty);
			}
			else if (this.updateType == UpdateType.None && this.showUpToDateMessage)
			{
				// This is used for the manual update check.
				output.Clear();
				output.Add(Resources.UpdateNoUpdate);
				output.Add(string.Empty);
			}

			// Make sure we have some update to perform before showing the dialog.
			if (this.updateType != UpdateType.None || this.showUpToDateMessage || this.streamException)
			{
				if (output != null && output.Count != 0)
				{
					// Convert the output to String array
					string[] lines = new string[output.Count];
					output.CopyTo(lines);
					this.messageTextBox.Lines = lines;
					this.messageTextBox.SelectionStart = this.messageTextBox.Text.Length;
					this.messageTextBox.ScrollToCaret();

					// Now we can show the dialog.
					this.okayButton.Focus();
					this.ShowDialog();
				}
			}
		}

		/// <summary>
		/// File download progress callback
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">DownloadProgressChangedEventArgs data</param>
		private void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
		{
			string[] originalLines = this.messageTextBox.Lines;

			// Assume the start of the download put this message on the bottom of
			// the message box and now we are just updating it.
			originalLines[originalLines.Length - 1] = string.Format(CultureInfo.CurrentCulture, Resources.UpdateCompletion, this.setupArchive, e.ProgressPercentage);

			this.messageTextBox.Focus();
			this.messageTextBox.Lines = originalLines;
			this.messageTextBox.SelectionStart = this.messageTextBox.TextLength;
			this.messageTextBox.ScrollToCaret();
			this.messageTextBox.Focus();
		}

		/// <summary>
		/// Download file callback.  Used to download the files asynchronously
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">AsyncCompletedEventArgs data</param>
		private void DownloadFileCallback(object sender, AsyncCompletedEventArgs e)
		{
			if (!e.Cancelled && e.Error == null)
			{
				// Extract the archive files.
				bool result = Database.ExtractArcFile(string.Concat(Application.StartupPath, "\\", this.setupArchive), Application.StartupPath);

				// Start the setup program
				if (result && File.Exists(string.Concat(Application.StartupPath, "\\", this.setupName)))
				{
					Process.Start(string.Concat(Application.StartupPath, "\\", this.setupName));
				}
				else
				{
					MessageBox.Show(Resources.UpdateErrorMsg, Resources.UpdateError, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, rightToLeftOptions);
				}

				// Close TQVault.exe so it can be updated.
				Environment.Exit(0);
			}

			if (e.Cancelled)
			{
				this.DialogResult = DialogResult.Cancel;
			}
		}

		/// <summary>
		/// Handler for clicking the OK button
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void OkayButtonClick(object sender, EventArgs e)
		{
			if (this.client != null)
			{
				string newFile;
				string tempFile;
				if (this.updateType == UpdateType.FullInstall)
				{
					newFile = string.Concat(Application.StartupPath, "\\", this.setupName);
					tempFile = string.Concat(Application.StartupPath, "\\", this.setupArchive);

					// Delete the existing setup archive file if it exists.
					if (File.Exists(tempFile))
					{
						File.Delete(tempFile);
					}

					if (File.Exists(newFile))
					{
						File.Delete(newFile);
					}

					// Specify that the DownloadFileCallback method gets called when the download completes.
					this.client.DownloadFileCompleted += new AsyncCompletedEventHandler(this.DownloadFileCallback);

					// Specify a progress notification handler.
					this.client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(this.DownloadProgressCallback);

					// Setup the display to show the progress
					string[] originalLines = this.messageTextBox.Lines;
					string[] newLines = new string[originalLines.Length + 1];
					originalLines.CopyTo(newLines, 0);
					newLines[originalLines.Length] = string.Format(CultureInfo.CurrentCulture, Resources.UpdateCompletion2, this.setupArchive);
					this.messageTextBox.Focus();
					this.messageTextBox.Lines = newLines;
					this.messageTextBox.ScrollToCaret();
					this.okayButton.Enabled = false;

					// Download the setup program.
					Uri uri = new Uri(string.Concat(this.httpAddress, this.setupArchive));
					this.client.DownloadFileAsync(uri, tempFile);

					// The download complete event will trigger the remaining tasks.
				}
				else if (this.updateType == UpdateType.Updater)
				{
					newFile = string.Concat(Application.StartupPath, "\\", this.updaterName);
					tempFile = string.Concat(newFile, ".tmp");
					string oldFile = string.Concat(newFile, ".old");

					// Download the latest update program.
					this.client.DownloadFile(string.Concat(this.httpAddress, this.updaterName, ".tmp"), tempFile);

					// Delete the existing backup of a previous update
					if (File.Exists(oldFile))
					{
						File.Delete(oldFile);
					}

					// Rename the existing file to .old
					if (File.Exists(newFile))
					{
						File.Move(newFile, oldFile);
					}

					// Rename the tmp file
					File.Move(tempFile, newFile);

					this.DialogResult = DialogResult.OK;
					this.Close();
				}
				else if (this.updateType == UpdateType.Patch)
				{
					// Start the update program
					Process.Start(
						string.Concat(Application.StartupPath, "\\", this.updaterName),
						string.Concat(" ", this.updaterParameters));

					// Close TQVault.exe so it can be updated.
					Environment.Exit(0);
				}
				else if (this.updateType == UpdateType.UpdaterAndPatch)
				{
					newFile = string.Concat(Application.StartupPath, "\\", this.updaterName);
					tempFile = string.Concat(newFile, ".tmp");
					string oldFile = string.Concat(newFile, ".old");

					// Download the latest update program.
					this.client.DownloadFile(string.Concat(this.httpAddress, this.updaterName, ".tmp"), tempFile);

					// Delete the existing backup of a previous update
					if (File.Exists(oldFile))
					{
						File.Delete(oldFile);
					}

					// Rename the existing file to .old
					if (File.Exists(newFile))
					{
						File.Move(newFile, oldFile);
					}

					// Rename the tmp file
					File.Move(tempFile, newFile);

					// Start the update program
					Process.Start(newFile, string.Concat(" ", this.updaterParameters));

					// Close TQVault.exe so it can be updated.
					Environment.Exit(0);
				}
			}

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		/// <summary>
		/// Handler for clicking the Cancel button
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void CancelButtonClick(object sender, EventArgs e)
		{
			this.client.CancelAsync();
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}
	}
}