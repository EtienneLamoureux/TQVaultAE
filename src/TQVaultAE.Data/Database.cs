//-----------------------------------------------------------------------
// <copyright file="Database.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Data
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Globalization;
	using System.IO;
	using System.Text;
	using TQVaultAE.Config;
	using TQVaultAE.Entities;
	using TQVaultAE.Logs;


	/// <summary>
	/// Reads a Titan Quest database file.
	/// </summary>
	public class Database
	{
		private readonly log4net.ILog Log = null;

		#region Database Fields


		/// <summary>
		/// Dictionary of all database info records
		/// </summary>
		private Dictionary<string, Info> infoDB;

		/// <summary>
		/// Dictionary of all text database entries
		/// </summary>
		private Dictionary<string, string> textDB;

		/// <summary>
		/// Dictionary of all associated arc files in the database.
		/// </summary>
		private Dictionary<string, ArcFile> arcFiles;

		/// <summary>
		/// Game language to support setting language in UI
		/// </summary>
		private string gameLanguage;

		#endregion Database Fields

		/// <summary>
		/// Initializes a new instance of the Database class.
		/// </summary>
		private Database()
		{
			this.Log = Logger.Get(this);
			this.arcFiles = new Dictionary<string, ArcFile>();
		}

		/// <summary>
		/// Load DB in static constructor
		/// </summary>
		static Database()
		{
			Database.DB = new Database();
			Database.DB.AutoDetectLanguage = Config.Settings.Default.AutoDetectLanguage;
			Database.DB.TQLanguage = Config.Settings.Default.TQLanguage;
			Database.DB.LoadDBFile();
		}

		#region Database Properties

		/// <summary>
		/// Gets or sets the static database.
		/// </summary>
		public static Database DB { get; private set; }


		/// <summary>
		/// Gets or sets a value indicating whether the game language is being auto detected.
		/// </summary>
		public bool AutoDetectLanguage { get; set; }

		/// <summary>
		/// Gets or sets the game language from the config file.
		/// </summary>
		public string TQLanguage { get; set; }


		/// <summary>
		/// Gets the instance of the Titan Quest Database ArzFile.
		/// </summary>

		public ArzFile ArzFile { get; private set; }

		/// <summary>
		/// Gets the instance of the Immortal Throne Database ArzFile.
		/// </summary>

		public ArzFile ArzFileIT { get; private set; }

		/// <summary>
		/// Gets the instance of a custom map Database ArzFile.
		/// </summary>

		public ArzFile ArzFileMod { get; private set; }

		/// <summary>
		/// Gets the game language setting as a an English DisplayName.
		/// </summary>
		/// <remarks>Changed to property by VillageIdiot to support changing of Language in UI</remarks>
		public string GameLanguage
		{
			get
			{
				// Added by VillageIdiot
				// Check if the user configured the language
				if (this.gameLanguage == null)
				{
					if (!this.AutoDetectLanguage)
						this.gameLanguage = this.TQLanguage;
				}

				// Try to read the language from the settings file
				if (string.IsNullOrEmpty(this.gameLanguage))
				{
					try
					{
						string optionsFile = TQData.TQSettingsFile;
						if (File.Exists(optionsFile))
						{
							using (StreamReader reader = new StreamReader(optionsFile))
							{
								// scan the file for the language line
								string line;
								char delims = '=';
								while ((line = reader.ReadLine()) != null)
								{
									// Split the line on the = sign
									string[] fields = line.Split(delims);
									if (fields.Length < 2)
										continue;

									string key = fields[0].Trim();
									string val = fields[1].Trim();

									if (key.ToUpperInvariant().Equals("LANGUAGE"))
									{
										this.gameLanguage = val.ToUpperInvariant();
										return this.gameLanguage;
									}
								}

								return null;
							}
						}

						return null;
					}
					catch (IOException exception)
					{
						Log.ErrorException(exception);
						return null;
					}
				}

				// Added by VillageIdiot
				// We have something so we need to return it
				// This was added to support setting the language in the config file
				return this.gameLanguage;
			}
		}

		#endregion Database Properties

		#region Database Public Methods

		#region Database Public Static Methods



		/// <summary>
		/// Used to Extract an ARC file into the destination directory.
		/// The ARC file will not be added to the cache.
		/// </summary>
		/// <remarks>Added by VillageIdiot</remarks>
		/// <param name="arcFileName">Name of the arc file</param>
		/// <param name="destination">Destination path for extracted data</param>
		/// <returns>Returns true on success otherwise false</returns>
		public static bool ExtractArcFile(string arcFileName, string destination)
		{
			bool result = false;

			if (TQDebug.DatabaseDebugLevel > 0)
				Logger.Log.DebugFormat(CultureInfo.InvariantCulture, "Database.ExtractARCFile('{0}', '{1}')", arcFileName, destination);

			try
			{
				ArcFile arcFile = new ArcFile(arcFileName);

				// Extract the files
				result = arcFile.ExtractArcFile(destination);
			}
			catch (IOException exception)
			{
				Logger.Log.Error("Exception occurred", exception);
				result = false;
			}

			if (TQDebug.DatabaseDebugLevel > 1)
				Logger.Log.DebugFormat(CultureInfo.InvariantCulture, "Extraction Result = {0}", result);

			if (TQDebug.DatabaseDebugLevel > 0)
				Logger.Log.Debug("Exiting Database.ReadARCFile()");

			return result;
		}

		#endregion Database Public Static Methods

		/// <summary>
		/// Gets the Infor for a specific item id.
		/// </summary>
		/// <param name="itemId">Item ID which we are looking up.  Will be normalized internally.</param>
		/// <returns>Returns Infor for item ID and NULL if not found.</returns>
		public Info GetInfo(string itemId)
		{
			Info result = null;
			if (string.IsNullOrEmpty(itemId))
				return result;

			itemId = TQData.NormalizeRecordPath(itemId);
			Info info;

			if (infoDB.ContainsKey(itemId))
				info = this.infoDB[itemId];
			else
			{
				DBRecordCollection record = null;

				// Add support for searching a custom map database
				if (this.ArzFileMod != null)
					record = this.ArzFileMod.GetItem(itemId);

				// Try the expansion pack database first.
				if (record == null && this.ArzFileIT != null)
					record = this.ArzFileIT.GetItem(itemId);

				// Try looking in TQ database now
				if (record == null || this.ArzFileIT == null)
					record = this.ArzFile.GetItem(itemId);

				if (record == null)
					return null;

				info = new Info(record);
				this.infoDB.Add(itemId, info);
			}

			return info;
		}

		/// <summary>
		/// Uses the text database to convert the tag to a name in the localized language.
		/// The tag is normalized to upper case internally.
		/// </summary>
		/// <param name="tagId">Tag to be looked up in the text database normalized to upper case.</param>
		/// <returns>Returns localized string, tagId if it cannot find a string or "?ErrorName?" in case of uncaught exception.</returns>
		public string GetFriendlyName(string tagId)
		{
			try
			{
				return this.textDB[tagId.ToUpperInvariant()];
			}
			catch (KeyNotFoundException)
			{
				return tagId;
			}
			catch (Exception)
			{
				return "?ErrorName?";
			}
		}

		/// <summary>
		/// Gets the formatted string for the variable attribute.
		/// </summary>
		/// <param name="variable">variable for which we are making a nice string.</param>
		/// <returns>Formatted string in the format of:  Attribute: value</returns>

		public string VariableToStringNice(Variable variable)
		{
			StringBuilder ans = new StringBuilder(64);
			ans.Append(this.GetItemAttributeFriendlyText(variable.Name));
			ans.Append(": ");
			ans.Append(variable.ToStringValue());
			return ans.ToString();
		}

		/// <summary>
		/// Converts the item attribute to a name in the localized language
		/// </summary>
		/// <param name="itemAttribute">Item attribure to be looked up.</param>
		/// <param name="addVariable">Flag for whether the variable is added to the text string.</param>
		/// <returns>Returns localized item attribute</returns>
		public string GetItemAttributeFriendlyText(string itemAttribute, bool addVariable = true)
		{
			ItemAttributesData data = ItemAttributeProvider.GetAttributeData(itemAttribute);
			if (data == null)
				return string.Concat("?", itemAttribute, "?");

			string attributeTextTag = ItemAttributeProvider.GetAttributeTextTag(data);
			if (string.IsNullOrEmpty(attributeTextTag))
				return string.Concat("?", itemAttribute, "?");

			string textFromTag = this.GetFriendlyName(attributeTextTag);
			if (string.IsNullOrEmpty(textFromTag))
			{
				textFromTag = string.Concat("ATTR<", itemAttribute, "> TAG<");
				textFromTag = string.Concat(textFromTag, attributeTextTag, ">");
			}

			if (addVariable && data.Variable.Length > 0)
				textFromTag = string.Concat(textFromTag, " ", data.Variable);

			return textFromTag;
		}

		/// <summary>
		/// Load our data from the db file.
		/// </summary>
		private void LoadDBFile()
		{
			this.LoadTextDB();
			this.LoadARZFile();

			this.infoDB = new Dictionary<string, Info>();
		}

		/// <summary>
		/// Gets a DBRecord for the specified item ID string.
		/// </summary>
		/// <remarks>
		/// Changed by VillageIdiot
		/// Changed search order so that IT records have precedence of TQ records.
		/// Add Custom Map database.  Custom Map records have precedence over IT records.
		/// </remarks>
		/// <param name="itemId">Item Id which we are looking up</param>
		/// <returns>Returns the DBRecord for the item Id</returns>

		public DBRecordCollection GetRecordFromFile(string itemId)
		{
			itemId = TQData.NormalizeRecordPath(itemId);

			if (this.ArzFileMod != null)
			{
				DBRecordCollection recordMod = this.ArzFileMod.GetItem(itemId);
				if (recordMod != null)
				{
					// Custom Map records have highest precedence.
					return recordMod;
				}
			}

			if (this.ArzFileIT != null)
			{
				// see if it's in IT ARZ file
				DBRecordCollection recordIT = this.ArzFileIT.GetItem(itemId);
				if (recordIT != null)
				{
					// IT file takes precedence over TQ.
					return recordIT;
				}
			}

			return ArzFile.GetItem(itemId);
		}

		/// <summary>
		/// Gets a resource from the database using the resource Id.
		/// Modified by VillageIdiot to support loading resources from a custom map folder.
		/// </summary>
		/// <param name="resourceId">Resource which we are fetching</param>
		/// <returns>Retruns a byte array of the resource.</returns>
		public byte[] LoadResource(string resourceId)
		{
			if (TQDebug.DatabaseDebugLevel > 0)
				Log.DebugFormat(CultureInfo.InvariantCulture, "Database.LoadResource({0})", resourceId);

			resourceId = TQData.NormalizeRecordPath(resourceId);

			if (TQDebug.DatabaseDebugLevel > 1)
				Log.DebugFormat(CultureInfo.InvariantCulture, " Normalized({0})", resourceId);

			// First we need to figure out the correct file to
			// open, by grabbing it off the front of the resourceID
			int backslashLocation = resourceId.IndexOf('\\');
			if (backslashLocation <= 0)
			{
				// not a proper resourceID.
				return null;
			}

			string arcFileBase = resourceId.Substring(0, backslashLocation);
			if (TQDebug.DatabaseDebugLevel > 1)
				Log.DebugFormat(CultureInfo.InvariantCulture, "arcFileBase = {0}", arcFileBase);

			string rootFolder;
			string arcFile;
			byte[] arcFileData = null;

			// Added by VillageIdiot
			// Check the mod folder for the image resource.
			if (TQData.IsCustom)
			{
				if (TQDebug.DatabaseDebugLevel > 1)
					Log.Debug("Checking Custom Resources.");

				rootFolder = Path.Combine(TQData.MapName, "resources");

				arcFile = Path.Combine(rootFolder, Path.ChangeExtension(arcFileBase, ".arc"));
				arcFileData = this.ReadARCFile(arcFile, resourceId);
			}

			// We either didn't load the resource or didn't find what we were looking for so check the normal game resources.
			if (arcFileData == null)
			{
				// See if this guy is from Immortal Throne expansion pack.
				if (TQDebug.DatabaseDebugLevel > 1)
					Log.Debug("Checking IT Resources.");

				rootFolder = TQData.ImmortalThronePath;

				bool xpack = false;

				if (arcFileBase.ToUpperInvariant().Equals("XPACK"))
				{
					// Comes from Immortal Throne
					xpack = true;
					rootFolder = Path.Combine(Path.Combine(rootFolder, "Resources"), "XPack");
				}
				else if (arcFileBase.ToUpperInvariant().Equals("XPACK2"))
				{
					// Comes from Ragnarok
					xpack = true;
					rootFolder = Path.Combine(Path.Combine(rootFolder, "Resources"), "XPack2");
				}
				else if (arcFileBase.ToUpperInvariant().Equals("XPACK3"))
				{
					// Comes from Atlantis
					xpack = true;
					rootFolder = Path.Combine(Path.Combine(rootFolder, "Resources"), "XPack3");
				}


				if (xpack == true)
				{
					// throw away that value and use the next field.
					int previousBackslash = backslashLocation;
					backslashLocation = resourceId.IndexOf('\\', backslashLocation + 1);

					if (backslashLocation <= 0)
						return null;// not a proper resourceID

					arcFileBase = resourceId.Substring(previousBackslash + 1, backslashLocation - previousBackslash - 1);
					resourceId = resourceId.Substring(previousBackslash + 1);
				}
				else
				{
					// Changed by VillageIdiot to search the IT resources folder for updated resources
					// if IT is installed otherwise just the TQ folder.
					rootFolder = Path.Combine(rootFolder, "Resources");
				}

				arcFile = Path.Combine(rootFolder, Path.ChangeExtension(arcFileBase, ".arc"));
				arcFileData = this.ReadARCFile(arcFile, resourceId);
			}

			// Added by VillageIdiot
			// Maybe the arc file is in the XPack folder even though the record does not state it.
			// Also could be that it says xpack in the record but the file is in the root.
			if (arcFileData == null)
			{
				rootFolder = Path.Combine(Path.Combine(TQData.ImmortalThronePath, "Resources"), "XPack");
				arcFile = Path.Combine(rootFolder, Path.ChangeExtension(arcFileBase, ".arc"));
				arcFileData = this.ReadARCFile(arcFile, resourceId);
			}

			// Now, let's check if the item is in Ragnarok DLC
			if (arcFileData == null && TQData.IsRagnarokInstalled)
			{
				rootFolder = Path.Combine(Path.Combine(TQData.ImmortalThronePath, "Resources"), "XPack2");
				arcFile = Path.Combine(rootFolder, Path.ChangeExtension(arcFileBase, ".arc"));
				arcFileData = this.ReadARCFile(arcFile, resourceId);
			}

			if (arcFileData == null && TQData.IsAtlantisInstalled)
			{
				rootFolder = Path.Combine(Path.Combine(TQData.ImmortalThronePath, "Resources"), "XPack3");
				arcFile = Path.Combine(rootFolder, Path.ChangeExtension(arcFileBase, ".arc"));
				arcFileData = this.ReadARCFile(arcFile, resourceId);
			}

			if (arcFileData == null)
			{
				// We are either vanilla TQ or have not found our resource yet.
				// from the original TQ folder
				if (TQDebug.DatabaseDebugLevel > 1)
					Log.Debug("Checking TQ Resources.");

				rootFolder = TQData.TQPath;
				rootFolder = Path.Combine(rootFolder, "Resources");

				arcFile = Path.Combine(rootFolder, Path.ChangeExtension(arcFileBase, ".arc"));
				arcFileData = this.ReadARCFile(arcFile, resourceId);
			}

			if (TQDebug.DatabaseDebugLevel > 0)
				Log.Debug("Exiting Database.LoadResource()");

			return arcFileData;
		}


		#endregion Database Public Methods

		#region Database Private Methods


		/// <summary>
		/// Reads data from an ARC file and puts it into a Byte array
		/// </summary>
		/// <param name="arcFileName">Name of the arc file.</param>
		/// <param name="dataId">Id of data which we are getting from the arc file</param>
		/// <returns>Byte array of the data from the arc file.</returns>
		private byte[] ReadARCFile(string arcFileName, string dataId)
		{
			// See if we have this arcfile already and if not create it.
			try
			{
				if (TQDebug.DatabaseDebugLevel > 0)
					Log.DebugFormat(CultureInfo.InvariantCulture, "Database.ReadARCFile('{0}', '{1}')", arcFileName, dataId);

				ArcFile arcFile;

				if (arcFiles.ContainsKey(arcFileName))
					arcFile = this.arcFiles[arcFileName];
				else
				{
					arcFile = new ArcFile(arcFileName);
					this.arcFiles.Add(arcFileName, arcFile);
				}

				// Now retrieve the data
				byte[] ans = arcFile.GetData(dataId);

				if (TQDebug.DatabaseDebugLevel > 0)
					Log.Debug("Exiting Database.ReadARCFile()");

				return ans;
			}
			catch (Exception e)
			{
				Log.Error("Exception occurred", e);
				throw;
			}
		}

		/// <summary>
		/// Tries to determine the name of the text database file.
		/// This is based on the game language and the UI language.
		/// Will use English if all else fails.
		/// </summary>
		/// <param name="isImmortalThrone">Signals whether we are looking for Immortal Throne files or vanilla Titan Quest files.</param>
		/// <returns>Path to the text db file</returns>
		private string FigureDBFileToUse(bool isImmortalThrone)
		{
			if (TQDebug.DatabaseDebugLevel > 0)
				Log.DebugFormat(CultureInfo.InvariantCulture, "Database.FigureDBFileToUse({0})", isImmortalThrone);

			string rootFolder;
			if (isImmortalThrone)
			{
				if (TQData.ImmortalThronePath.Contains("Anniversary"))
					rootFolder = Path.Combine(TQData.ImmortalThronePath, "Text");
				else
					rootFolder = Path.Combine(TQData.ImmortalThronePath, "Resources");

				if (TQDebug.DatabaseDebugLevel > 1)
				{
					Log.Debug("Detecting Immortal Throne text files");
					Log.DebugFormat(CultureInfo.InvariantCulture, "rootFolder = {0}", rootFolder);
				}
			}
			else
			{
				// from the original TQ folder
				rootFolder = Path.Combine(TQData.TQPath, "Text");

				if (TQDebug.DatabaseDebugLevel > 1)
				{
					Log.Debug("Detecting Titan Quest text files");
					Log.DebugFormat(CultureInfo.InvariantCulture, "rootFolder = {0}", rootFolder);
				}
			}

			// make sure the damn directory exists
			if (!Directory.Exists(rootFolder))
			{
				if (TQDebug.DatabaseDebugLevel > 0)
					Log.Debug("Error - Root Folder does not exist");

				return null; // silently fail
			}

			string baseFile = Path.Combine(rootFolder, "Text_");
			string suffix = ".arc";

			// Added explicit set to null though may not be needed
			string cultureID = null;

			// Moved this declaration since the first use is inside of the loop.
			string filename = null;

			// First see if we can use the game setting
			string gameLanguage = this.GameLanguage;
			if (TQDebug.DatabaseDebugLevel > 1)
			{
				Log.DebugFormat(CultureInfo.InvariantCulture, "gameLanguage = {0}", gameLanguage == null ? "NULL" : gameLanguage);
				Log.DebugFormat(CultureInfo.InvariantCulture, "baseFile = {0}", baseFile);
			}

			if (gameLanguage != null)
			{
				// Try this method of getting the culture
				if (TQDebug.DatabaseDebugLevel > 2)
					Log.Debug("Try looking up cultureID");

				foreach (CultureInfo cultureInfo in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
				{
					if (TQDebug.DatabaseDebugLevel > 2)
						Log.DebugFormat(CultureInfo.InvariantCulture, "Trying {0}", cultureInfo.EnglishName.ToUpperInvariant());

					if (cultureInfo.EnglishName.ToUpperInvariant().Equals(gameLanguage.ToUpperInvariant()) || cultureInfo.DisplayName.ToUpperInvariant().Equals(gameLanguage.ToUpperInvariant()))
					{
						cultureID = cultureInfo.TwoLetterISOLanguageName;
						break;
					}
				}

				// For some reason they use CZ which is not in the cultures list.
				// Force Czech to use CZ instead of CS for the 2 letter code.
				// Added null check to fix exception when there is no culture found.
				if (cultureID != null && cultureID.ToUpperInvariant() == "CS")
					cultureID = "CZ";

				if (TQDebug.DatabaseDebugLevel > 1)
					Log.DebugFormat(CultureInfo.InvariantCulture, "cultureID = {0}", cultureID);

				// Moved this inital check for the file into the loop
				// and added a check to verify that we actually have a cultureID
				if (cultureID != null)
				{
					filename = string.Concat(baseFile, cultureID, suffix);
					if (TQDebug.DatabaseDebugLevel > 1)
					{
						Log.Debug("Detected cultureID from gameLanguage");
						Log.DebugFormat(CultureInfo.InvariantCulture, "filename = {0}", filename);
					}

					if (File.Exists(filename))
					{
						if (TQDebug.DatabaseDebugLevel > 0)
							Log.Debug("Exiting Database.FigureDBFileToUse()");

						return filename;
					}
				}
			}

			// try to use the default culture for the OS
			cultureID = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
			if (TQDebug.DatabaseDebugLevel > 1)
			{
				Log.Debug("Using cultureID from OS");
				Log.DebugFormat(CultureInfo.InvariantCulture, "cultureID = {0}", cultureID);
			}

			// Added a check to verify that we actually have a cultureID
			// though it may not be needed
			if (cultureID != null)
			{
				filename = string.Concat(baseFile, cultureID, suffix);
				if (TQDebug.DatabaseDebugLevel > 1)
					Log.DebugFormat(CultureInfo.InvariantCulture, "filename = {0}", filename);

				if (File.Exists(filename))
				{
					if (TQDebug.DatabaseDebugLevel > 0)
						Log.Debug("Exiting Database.FigureDBFileToUse()");

					return filename;
				}
			}

			// Now just try EN
			cultureID = "EN";
			filename = string.Concat(baseFile, cultureID, suffix);
			if (TQDebug.DatabaseDebugLevel > 1)
			{
				Log.Debug("Forcing English Language");
				Log.DebugFormat(CultureInfo.InvariantCulture, "cultureID = {0}", cultureID);
				Log.DebugFormat(CultureInfo.InvariantCulture, "filename = {0}", filename);
			}

			if (File.Exists(filename))
			{
				if (TQDebug.DatabaseDebugLevel > 0)
					Log.Debug("Database.Exiting FigureDBFileToUse()");

				return filename;
			}

			// Now just see if we can find anything.
			if (TQDebug.DatabaseDebugLevel > 0)
				Log.Debug("Detection Failed - searching for files");

			string[] files = Directory.GetFiles(rootFolder, "Text_??.arc");

			// Added check that files is not null.
			if (files != null && files.Length > 0)
			{
				if (TQDebug.DatabaseDebugLevel > 1)
				{
					Log.Debug("Found some files");
					Log.DebugFormat(CultureInfo.InvariantCulture, "filename = {0}", files[0]);
				}

				if (TQDebug.DatabaseDebugLevel > 0)
					Log.Debug("Exiting Database.FigureDBFileToUse()");

				return files[0];
			}

			if (TQDebug.DatabaseDebugLevel > 0)
			{
				Log.Debug("Failed to determine Language file!");
				Log.Debug("Exiting Database.FigureDBFileToUse()");
			}

			return null;
		}

		/// <summary>
		/// Loads the Text database
		/// </summary>
		private void LoadTextDB()
		{
			if (TQDebug.DatabaseDebugLevel > 0)
				Log.Debug("Database.LoadTextDB()");

			this.textDB = new Dictionary<string, string>();
			string databaseFile = this.FigureDBFileToUse(false);
			if (TQDebug.DatabaseDebugLevel > 1)
			{
				Log.Debug("Find Titan Quest text file");
				Log.DebugFormat(CultureInfo.InvariantCulture, "dbFile = {0}", databaseFile);
			}

			if (databaseFile != null)
			{
				// Try to suck what we want into memory and then parse it.
				this.ParseTextDB(databaseFile, "text\\commonequipment.txt");
				this.ParseTextDB(databaseFile, "text\\uniqueequipment.txt");
				this.ParseTextDB(databaseFile, "text\\quest.txt");
				this.ParseTextDB(databaseFile, "text\\ui.txt");
				this.ParseTextDB(databaseFile, "text\\skills.txt");
				this.ParseTextDB(databaseFile, "text\\monsters.txt"); // Added by VillageIdiot
				this.ParseTextDB(databaseFile, "text\\menu.txt"); // Added by VillageIdiot

				// Immortal Throne data
				this.ParseTextDB(databaseFile, "text\\xcommonequipment.txt");
				this.ParseTextDB(databaseFile, "text\\xuniqueequipment.txt");
				this.ParseTextDB(databaseFile, "text\\xquest.txt");
				this.ParseTextDB(databaseFile, "text\\xui.txt");
				this.ParseTextDB(databaseFile, "text\\xskills.txt");
				this.ParseTextDB(databaseFile, "text\\xmonsters.txt"); // Added by VillageIdiot
				this.ParseTextDB(databaseFile, "text\\xmenu.txt"); // Added by VillageIdiot
				this.ParseTextDB(databaseFile, "text\\xnpc.txt"); // Added by VillageIdiot
				this.ParseTextDB(databaseFile, "text\\modstrings.txt"); // Added by VillageIdiot

				if (TQData.IsRagnarokInstalled)
				{
					this.ParseTextDB(databaseFile, "text\\x2commonequipment.txt");
					this.ParseTextDB(databaseFile, "text\\x2uniqueequipment.txt");
					this.ParseTextDB(databaseFile, "text\\x2quest.txt");
					this.ParseTextDB(databaseFile, "text\\x2ui.txt");
					this.ParseTextDB(databaseFile, "text\\x2skills.txt");
					this.ParseTextDB(databaseFile, "text\\x2monsters.txt"); // Added by VillageIdiot
					this.ParseTextDB(databaseFile, "text\\x2menu.txt"); // Added by VillageIdiot
					this.ParseTextDB(databaseFile, "text\\x2npc.txt"); // Added by VillageIdiot
				}

				if (TQData.IsAtlantisInstalled)
				{
					this.ParseTextDB(databaseFile, "text\\x3basegame_nonvoiced.txt");
					this.ParseTextDB(databaseFile, "text\\x3items_nonvoiced.txt");
					this.ParseTextDB(databaseFile, "text\\x3mainquest_nonvoiced.txt");
					this.ParseTextDB(databaseFile, "text\\x3misctags_nonvoiced.txt");
				}
			}

			// For loading custom map text database.
			if (TQData.IsCustom)
			{
				databaseFile = Path.Combine(TQData.MapName, "resources", "text.arc");

				if (TQDebug.DatabaseDebugLevel > 1)
				{
					Log.Debug("Find Custom Map text file");
					Log.DebugFormat(CultureInfo.InvariantCulture, "dbFile = {0}", databaseFile);
				}

				if (databaseFile != null)
					this.ParseTextDB(databaseFile, "text\\modstrings.txt");
			}

			// Added this check to see if anything was loaded.
			if (this.textDB.Count == 0)
			{
				if (TQDebug.DatabaseDebugLevel > 0)
					Log.Debug("Exception - Could not load Text DB.");

				throw new FileLoadException("Could not load Text DB.");
			}

			if (TQDebug.DatabaseDebugLevel > 0)
				Log.Debug("Exiting Database.LoadTextDB()");
		}

		/// <summary>
		/// Parses the text database to put the entries into a hash table.
		/// </summary>
		/// <param name="databaseFile">Database file name (arc file)</param>
		/// <param name="filename">Name of the text DB file within the arc file</param>
		private void ParseTextDB(string databaseFile, string filename)
		{
			if (TQDebug.DatabaseDebugLevel > 0)
				Log.DebugFormat(CultureInfo.InvariantCulture, "Database.ParseTextDB({0}, {1})", databaseFile, filename);

			byte[] data = this.ReadARCFile(databaseFile, filename);

			if (data == null)
			{
				// Changed for mod support.  Sometimes the text file has more entries than just the x or non-x prefix files.
				if (TQDebug.DatabaseDebugLevel > 0)
					Log.DebugFormat(CultureInfo.InvariantCulture, "Error in ARC File: {0} does not contain an entry for '{1}'", databaseFile, filename);

				return;
			}

			// now read it like a text file
			// Changed to system default encoding since there might be extended ascii (or something else) in the text db.
			using (StreamReader reader = new StreamReader(new MemoryStream(data), Encoding.Default))
			{
				char delimiter = '=';
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					line = line.Trim();

					// delete short lines
					if (line.Length < 2)
						continue;

					// comment line
					if (line.StartsWith("//", StringComparison.Ordinal))
						continue;

					// split on the equal sign
					string[] fields = line.Split(delimiter);

					// bad line
					if (fields.Length < 2)
						continue;

					string label = fields[1].Trim();

					// Now for the foreign languages there is a bunch of crap in here so the proper version of the adjective can be used with the proper
					// noun form.  I don' want to code all that so this next code will just take the first version of the adjective and then
					// throw away all the metadata.
					if (label.IndexOf('[') != -1)
					{
						// find first [xxx]
						int textStart = label.IndexOf(']') + 1;

						// find second [xxx]
						int textEnd = label.IndexOf('[', textStart);
						if (textEnd == -1)
						{
							// If it was the only [...] tag in the string then take the whole string after the tag
							label = label.Substring(textStart);
						}
						else
						{
							// else take the string between the first 2 [...] tags
							label = label.Substring(textStart, textEnd - textStart);
						}

						label = label.Trim();
					}

					// If this field is already in the db, then replace it
					string key = fields[0].Trim().ToUpperInvariant();
					this.textDB[key] = label;
				}
			}

			if (TQDebug.DatabaseDebugLevel > 0)
				Log.Debug("Exiting Database.ParseTextDB()");
		}

		/// <summary>
		/// Loads a database arz file.
		/// </summary>
		private void LoadARZFile()
		{
			if (TQDebug.DatabaseDebugLevel > 0)
				Log.Debug("Database.LoadARZFile()");

			// from the original TQ folder
			string file = Path.Combine(Path.Combine(TQData.TQPath, "Database"), "database.arz");

			if (TQDebug.DatabaseDebugLevel > 1)
			{
				Log.Debug("Load Titan Quest database arz file");
				Log.DebugFormat(CultureInfo.InvariantCulture, "file = {0}", file);
			}

			this.ArzFile = new ArzFile(file);
			this.ArzFile.Read();

			// now Immortal Throne expansion pack
			this.ArzFileIT = this.ArzFile;

			// Added to load a custom map database file.
			if (TQData.IsCustom)
			{
				file = Path.Combine(TQData.MapName, "database", $"{Path.GetFileName(TQData.MapName)}.arz");

				if (TQDebug.DatabaseDebugLevel > 1)
				{
					Log.Debug("Load Custom Map database arz file");
					Log.DebugFormat(CultureInfo.InvariantCulture, "file = {0}", file);
				}

				if (File.Exists(file))
				{
					this.ArzFileMod = new ArzFile(file);
					this.ArzFileMod.Read();
				}
				else
					this.ArzFileMod = null;
			}

			if (TQDebug.DatabaseDebugLevel > 0)
				Log.Debug("Exiting Database.LoadARZFile()");
		}

		#endregion Database Private Methods
	}
}