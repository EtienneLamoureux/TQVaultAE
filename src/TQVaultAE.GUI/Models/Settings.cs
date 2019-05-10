//-----------------------------------------------------------------------
// <copyright file="Settings.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Models.Properties
{
	using System.ComponentModel;
	using System.Configuration;

	/// <summary>
	/// This class allows you to handle specific events on the settings class:
	/// The SettingChanging event is raised before a setting's value is changed.
	/// The PropertyChanged event is raised after a setting's value is changed.
	/// The SettingsLoaded event is raised after the setting values are loaded.
	/// The SettingsSaving event is raised before the setting values are saved.
	/// </summary>
	internal sealed partial class Settings
	{
		/// <summary>
		/// Initializes a new instance of the Settings class.
		/// </summary>
		public Settings()
		{
			// To add event handlers for saving and changing settings, uncomment the lines below:
			//
			//// this.SettingChanging += this.SettingChangingEventHandler;
			//
			//// this.SettingsSaving += this.SettingsSavingEventHandler;
		}

		/// <summary>
		/// Handler for changing the settings
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">SettingChangingEventArgs data</param>
		private void SettingChangingEventHandler(object sender, SettingChangingEventArgs e)
		{
			// Add code to handle the SettingChangingEvent event here.
		}

		/// <summary>
		/// Handler for saving the settings
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">CancelEventArgs data</param>
		private void SettingsSavingEventHandler(object sender, CancelEventArgs e)
		{
			// Add code to handle the SettingsSaving event here.
		}
	}
}