//-----------------------------------------------------------------------
// <copyright file="TQDebug.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Config;

/// <summary>
/// Class used for runtime debugging with the configuration file.
/// </summary>
public static class TQDebug
{
	/// <summary>
	/// Holds the arc file debug level.
	/// </summary>
	private static int _ArcFileDebugLevel = Config.Settings.Default.ARCFileDebugLevel;

	/// <summary>
	/// Holds loot table debug enabled?
	/// </summary>
	private static bool _LootTableDebugEnabled = Config.Settings.Default.LootTableDebugEnabled;

	/// <summary>
	/// Holds the database debug level.
	/// </summary>
	private static int _DatabaseDebugLevel = Config.Settings.Default.DatabaseDebugLevel;

	/// <summary>
	/// Holds the item debug level.
	/// </summary>
	private static int _ItemDebugLevel = Config.Settings.Default.ItemDebugLevel;

	/// <summary>
	/// Holds the item attributes debug level.
	/// </summary>
	private static int _ItemAttributesDebugLevel = Config.Settings.Default.ItemAttributesDebugLevel;

	/// <summary>
	/// Gets or sets a value indicating whether debugging has been enabled
	/// </summary>
	public static bool DebugEnabled { get; set; } = Config.Settings.Default.DebugEnabled;

	/// <summary>
	/// Gets or sets the database debug level
	/// </summary>
	public static int DatabaseDebugLevel
	{
		get => DebugEnabled ? _DatabaseDebugLevel : 0;
		set => _DatabaseDebugLevel = value;
	}

	/// <summary>
	/// Gets or sets the arc file debug level
	/// </summary>
	public static int ArcFileDebugLevel
	{
		get => DebugEnabled ? _ArcFileDebugLevel : 0;
		set => _ArcFileDebugLevel = value;
	}

	/// <summary>
	/// Gets or sets the item debug level
	/// </summary>
	public static int ItemDebugLevel
	{
		get => DebugEnabled ? _ItemDebugLevel : 0;
		set => _ItemDebugLevel = value;
	}

	/// <summary>
	/// Gets or sets the item attributes debug level
	/// </summary>
	public static int ItemAttributesDebugLevel
	{
		get => DebugEnabled ? _ItemAttributesDebugLevel : 0;
		set => _ItemAttributesDebugLevel = value;
	}

	/// <summary>
	/// Is loot table debug enabled?
	/// </summary>
	public static bool LootTableDebugEnabled
	{
		get => DebugEnabled ? _LootTableDebugEnabled : false;
		set => _LootTableDebugEnabled = value;
	}
}