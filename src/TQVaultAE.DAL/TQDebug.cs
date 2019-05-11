//-----------------------------------------------------------------------
// <copyright file="TQDebug.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.DAL
{
	using System.IO;
	using TQVaultAE.Logging;

	/// <summary>
	/// Class used for runtime debugging with the configuration file.
	/// </summary>
	public static class TQDebug
	{
		private static log4net.ILog Log = Logger.Get(typeof(TQDebug));

		/// <summary>
		/// Indicates whether debugging is globally enabled.
		/// </summary>
		private static bool debugEnabled;

		/// <summary>
		/// Holds the arc file debug level.
		/// </summary>
		private static int arcFileDebugLevel;

		/// <summary>
		/// Holds the database debug level.
		/// </summary>
		private static int databaseDebugLevel;

		/// <summary>
		/// Holds the item debug level.
		/// </summary>
		private static int itemDebugLevel;

		/// <summary>
		/// Holds the item attributes debug level.
		/// </summary>
		private static int itemAttributesDebugLevel;

		/// <summary>
		/// Gets or sets a value indicating whether debugging has been enabled
		/// </summary>
		public static bool DebugEnabled
		{
			get
			{
				return debugEnabled;
			}

			set
			{
				bool lastValue = debugEnabled;
				debugEnabled = value;

				// Only delete the file the first time we turn it on.
				if (debugEnabled && (lastValue != debugEnabled) && File.Exists("tqdebug.txt"))
				{
					// Delete any existing debug log.
					File.Delete("tqdebug.txt");
				}
			}
		}

		/// <summary>
		/// Gets or sets the database debug level
		/// </summary>
		public static int DatabaseDebugLevel
		{
			get
			{
				if (DebugEnabled)
				{
					return databaseDebugLevel;
				}
				else
				{
					return 0;
				}
			}

			set
			{
				databaseDebugLevel = value;
			}
		}

		/// <summary>
		/// Gets or sets the arc file debug level
		/// </summary>
		public static int ArcFileDebugLevel
		{
			get
			{
				if (DebugEnabled)
				{
					return arcFileDebugLevel;
				}
				else
				{
					return 0;
				}
			}

			set
			{
				arcFileDebugLevel = value;
			}
		}

		/// <summary>
		/// Gets or sets the item debug level
		/// </summary>
		public static int ItemDebugLevel
		{
			get
			{
				if (DebugEnabled)
				{
					return itemDebugLevel;
				}
				else
				{
					return 0;
				}
			}

			set
			{
				itemDebugLevel = value;
			}
		}

		/// <summary>
		/// Gets or sets the item attributes debug level
		/// </summary>
		public static int ItemAttributesDebugLevel
		{
			get
			{
				if (DebugEnabled)
				{
					return itemAttributesDebugLevel;
				}
				else
				{
					return 0;
				}
			}

			set
			{
				itemAttributesDebugLevel = value;
			}
		}

		/// <summary>
		/// Writes an entry into the debug log without trailing Carriage Return.
		/// Will only write if global debugging is enabled.
		/// </summary>
		/// <param name="message">Message to be written to the file.</param>
		public static void DebugWrite(string message)
		{
			if (DebugEnabled)
			{
				StreamWriter outStream = new StreamWriter("tqdebug.txt", true);
				outStream.Write(message);
				outStream.Close();
			}
		}

		/// <summary>
		/// Writes an entry into the debug log with a trailing Carriage Return.
		/// Will only write if global debugging is enabled.
		/// </summary>
		/// <param name="message">Message to be written to the file.</param>
		public static void DebugWriteLine(string message)
		{
			if (DebugEnabled)
			{
				StreamWriter outStream = new StreamWriter("tqdebug.txt", true);
				outStream.WriteLine(message);
				outStream.Close();
			}
		}
	}
}