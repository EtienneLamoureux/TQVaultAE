//-----------------------------------------------------------------------
// <copyright file="TQDebug.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Config
{
	using log4net.Core;
	using TQVaultAE.Logs;

	/// <summary>
	/// Class used for runtime debugging with the configuration file.
	/// </summary>
	public static class TQDebug
	{
		/// <summary>
		/// Indicates whether debugging is globally enabled.
		/// </summary>
		private static bool debugEnabled = Config.Settings.Default.DebugEnabled;

		/// <summary>
		/// Current debug level
		/// </summary>
		private static Level debugEnabledLevel = Level.Info;

		/// <summary>
		/// Holds the arc file debug level.
		/// </summary>
		private static int arcFileDebugLevel = Config.Settings.Default.ARCFileDebugLevel;

		/// <summary>
		/// Holds the database debug level.
		/// </summary>
		private static int databaseDebugLevel = Config.Settings.Default.DatabaseDebugLevel;

		/// <summary>
		/// Holds the item debug level.
		/// </summary>
		private static int itemDebugLevel = Config.Settings.Default.ItemDebugLevel;

		/// <summary>
		/// Holds the item attributes debug level.
		/// </summary>
		private static int itemAttributesDebugLevel = Config.Settings.Default.ItemAttributesDebugLevel;

		/// <summary>
		/// Ctor
		/// </summary>
		static TQDebug()
		{
			// Apply config at startup
			if (debugEnabled && debugEnabledLevel != Level.Debug)
				debugEnabledLevel = Level.Debug;

			Logger.ChangeRootLogLevel(debugEnabledLevel);
		}

		/// <summary>
		/// Gets or sets a value indicating whether debugging has been enabled
		/// </summary>
		public static bool DebugEnabled
		{
			get => debugEnabled;
			set
			{
				bool lastValue = debugEnabled;
				debugEnabled = value;

				if (lastValue != debugEnabled)
					Logger.ChangeRootLogLevel(value ? Level.Debug : Level.Info);
			}
		}

		/// <summary>
		/// Gets or sets the database debug level
		/// </summary>
		public static int DatabaseDebugLevel
		{
			get => DebugEnabled ? databaseDebugLevel : 0;
			set => databaseDebugLevel = value;
		}

		/// <summary>
		/// Gets or sets the arc file debug level
		/// </summary>
		public static int ArcFileDebugLevel
		{
			get => DebugEnabled ? arcFileDebugLevel : 0;
			set => arcFileDebugLevel = value;
		}

		/// <summary>
		/// Gets or sets the item debug level
		/// </summary>
		public static int ItemDebugLevel
		{
			get => DebugEnabled ? itemDebugLevel : 0;
			set => itemDebugLevel = value;
		}

		/// <summary>
		/// Gets or sets the item attributes debug level
		/// </summary>
		public static int ItemAttributesDebugLevel
		{
			get => DebugEnabled ? itemAttributesDebugLevel : 0;
			set => itemAttributesDebugLevel = value;
		}

	}
}