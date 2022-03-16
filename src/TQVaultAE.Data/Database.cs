//-----------------------------------------------------------------------
// <copyright file="Database.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Data
{
	using Microsoft.Extensions.Logging;
	using System;
	using System.Globalization;
	using System.IO;
	using System.Text;
	using TQVaultAE.Config;
	using TQVaultAE.Domain.Contracts.Providers;
	using TQVaultAE.Domain.Contracts.Services;
	using TQVaultAE.Domain.Entities;
	using TQVaultAE.Domain.Helpers;
	using TQVaultAE.Logs;


	/// <summary>
	/// Reads a Titan Quest database file.
	/// </summary>
	public class Database : IDatabase
	{
		private readonly ILogger Log = null;

		#region Database Fields

		/// <summary>
		/// Dictionary of all database info records
		/// </summary>
		private LazyConcurrentDictionary<string, Info> infoDB = new LazyConcurrentDictionary<string, Info>();

		/// <summary>
		/// Dictionary of all text database entries
		/// </summary>
		private LazyConcurrentDictionary<string, string> textDB = new LazyConcurrentDictionary<string, string>();

		/// <summary>
		/// Dictionary of all associated arc files in the database.
		/// </summary>
		private LazyConcurrentDictionary<string, ArcFile> arcFiles = new LazyConcurrentDictionary<string, ArcFile>();

		/// <summary>
		/// Game language to support setting language in UI
		/// </summary>
		private string gameLanguage;

		private readonly IArcFileProvider arcProv;
		private readonly IArzFileProvider arzProv;
		private readonly IItemAttributeProvider ItemAttributeProvider;
		private readonly IGamePathService GamePathResolver;
		private readonly ITQDataService TQData;

		#endregion Database Fields

		/// <summary>
		/// Initializes a new instance of the Database class.
		/// </summary>
		public Database(
			ILogger<Database> log
			, IArcFileProvider arcFileProvider
			, IArzFileProvider arzFileProvider
			, IItemAttributeProvider itemAttributeProvider
			, IGamePathService gamePathResolver
			, ITQDataService tQData
		)
		{
			this.Log = log;
			this.AutoDetectLanguage = Config.Settings.Default.AutoDetectLanguage;
			this.TQLanguage = Config.Settings.Default.TQLanguage;
			this.arcProv = arcFileProvider;
			this.arzProv = arzFileProvider;
			this.ItemAttributeProvider = itemAttributeProvider;
			this.GamePathResolver = gamePathResolver;
			this.TQData = tQData;
			this.LoadDBFile();
		}


		#region Database Properties

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
						string optionsFile = GamePathResolver.TQSettingsFile;
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
		public bool ExtractArcFile(string arcFileName, string destination)
		{
			bool result = false;

			if (TQDebug.DatabaseDebugLevel > 0)
				Log.LogDebug("Database.ExtractARCFile('{0}', '{1}')", arcFileName, destination);

			try
			{
				ArcFile arcFile = new ArcFile(arcFileName);

				// Extract the files
				result = arcProv.ExtractArcFile(arcFile, destination);
			}
			catch (IOException exception)
			{
				Log.LogError(exception, "Exception occurred");
				result = false;
			}

			if (TQDebug.DatabaseDebugLevel > 1)
				Log.LogDebug("Extraction Result = {0}", result);

			if (TQDebug.DatabaseDebugLevel > 0)
				Log.LogDebug("Exiting Database.ReadARCFile()");

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
			if (string.IsNullOrEmpty(itemId))
				return null;

			itemId = TQData.NormalizeRecordPath(itemId);

			return this.infoDB.GetOrAddAtomic(itemId, k =>
			{
				DBRecordCollection record = null;
				// Add support for searching a custom map database
				if (this.ArzFileMod != null)
					record = arzProv.GetItem(this.ArzFileMod, k);

				// Try the expansion pack database first.
				if (record == null && this.ArzFileIT != null)
					record = arzProv.GetItem(this.ArzFileIT, k);

				// Try looking in TQ database now
				if (record == null || this.ArzFileIT == null)
					record = arzProv.GetItem(this.ArzFile, k);

				if (record == null)
					return null;

				return new Info(record);
			});
		}

		/// <summary>
		/// Uses the text database to convert the tag to a name in the localized language.
		/// The tag is normalized to upper case internally.
		/// </summary>
		/// <param name="tagId">Tag to be looked up in the text database normalized to upper case.</param>
		/// <returns>Returns localized string, empty string if it cannot find a string or "?ErrorName?" in case of uncaught exception.</returns>
		public string GetFriendlyName(string tagId)
			=> this.textDB.TryGetValue(tagId.ToUpperInvariant(), out var text) ? text.Value : string.Empty;

		/// <summary>
		/// Gets the formatted string for the variable attribute.
		/// </summary>
		/// <param name="variable">variable for which we are making a nice string.</param>
		/// <returns>Formatted string in the format of:  Attribute: value</returns>
		public string VariableToStringNice(Variable variable)
			=> $"{this.GetItemAttributeFriendlyText(variable.Name)}: {variable.ToStringValue()}";

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
			{
				this.Log.LogDebug($"Attribute unknown : {itemAttribute}");
				return string.Concat("?", itemAttribute, "?");
			}

			string attributeTextTag = ItemAttributeProvider.GetAttributeTextTag(data);
			if (string.IsNullOrEmpty(attributeTextTag))
			{
				this.Log.LogDebug($"Attribute unknown : {itemAttribute}");
				return string.Concat("?", itemAttribute, "?");
			}

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
				DBRecordCollection recordMod = arzProv.GetItem(this.ArzFileMod, itemId);
				if (recordMod != null)
				{
					// Custom Map records have highest precedence.
					return recordMod;
				}
			}

			if (this.ArzFileIT != null)
			{
				// see if it's in IT ARZ file
				DBRecordCollection recordIT = arzProv.GetItem(this.ArzFileIT, itemId);
				if (recordIT != null)
				{
					// IT file takes precedence over TQ.
					return recordIT;
				}
			}

			return arzProv.GetItem(ArzFile, itemId);
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
				Log.LogDebug("Database.LoadResource({0})", resourceId);

			resourceId = TQData.NormalizeRecordPath(resourceId);

			if (TQDebug.DatabaseDebugLevel > 1)
				Log.LogDebug(" Normalized({0})", resourceId);

			// First we need to figure out the correct file to
			// open, by grabbing it off the front of the resourceID
			int backslashLocation = resourceId.IndexOf('\\');

			// not a proper resourceID.
			if (backslashLocation <= 0)
				return null;

			string arcFileBase = resourceId.Substring(0, backslashLocation);
			if (TQDebug.DatabaseDebugLevel > 1)
				Log.LogDebug("arcFileBase = {0}", arcFileBase);

			string rootFolder;
			string arcFile;
			byte[] arcFileData = null;

			// Added by VillageIdiot
			// Check the mod folder for the image resource.
			if (GamePathResolver.IsCustom)
			{
				if (TQDebug.DatabaseDebugLevel > 1)
					Log.LogDebug("Checking Custom Resources.");

				rootFolder = Path.Combine(GamePathResolver.MapName, "resources");

				arcFile = Path.Combine(rootFolder, Path.ChangeExtension(arcFileBase, ".arc"));
				arcFileData = this.ReadARCFile(arcFile, resourceId);
			}

			// We either didn't load the resource or didn't find what we were looking for so check the normal game resources.
			if (arcFileData == null)
			{
				// See if this guy is from Immortal Throne expansion pack.
				if (TQDebug.DatabaseDebugLevel > 1)
					Log.LogDebug("Checking IT Resources.");

				rootFolder = GamePathResolver.ImmortalThronePath;

				bool xpack = false;

				if (arcFileBase.ToUpperInvariant().Equals("XPACK"))
				{
					// Comes from Immortal Throne
					xpack = true;
					rootFolder = Path.Combine(rootFolder, "Resources", "XPack");
				}
				else if (arcFileBase.ToUpperInvariant().Equals("XPACK2"))
				{
					// Comes from Ragnarok
					xpack = true;
					rootFolder = Path.Combine(rootFolder, "Resources", "XPack2");
				}
				else if (arcFileBase.ToUpperInvariant().Equals("XPACK3"))
				{
					// Comes from Atlantis
					xpack = true;
					rootFolder = Path.Combine(rootFolder, "Resources", "XPack3");
				}
				else if (arcFileBase.ToUpperInvariant().Equals("XPACK4"))
				{
					// Comes from Eternal Embers
					xpack = true;
					rootFolder = Path.Combine(rootFolder, "Resources", "XPack4");
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
				rootFolder = Path.Combine(GamePathResolver.ImmortalThronePath, "Resources", "XPack");
				arcFile = Path.Combine(rootFolder, Path.ChangeExtension(arcFileBase, ".arc"));
				arcFileData = this.ReadARCFile(arcFile, resourceId);
			}

			// Now, let's check if the item is in Ragnarok DLC
			if (arcFileData == null && GamePathResolver.IsRagnarokInstalled)
			{
				rootFolder = Path.Combine(GamePathResolver.ImmortalThronePath, "Resources", "XPack2");
				arcFile = Path.Combine(rootFolder, Path.ChangeExtension(arcFileBase, ".arc"));
				arcFileData = this.ReadARCFile(arcFile, resourceId);
			}

			if (arcFileData == null && GamePathResolver.IsAtlantisInstalled)
			{
				rootFolder = Path.Combine(GamePathResolver.ImmortalThronePath, "Resources", "XPack3");
				arcFile = Path.Combine(rootFolder, Path.ChangeExtension(arcFileBase, ".arc"));
				arcFileData = this.ReadARCFile(arcFile, resourceId);
			}

			if (arcFileData == null && GamePathResolver.IsEmbersInstalled)
			{
				rootFolder = Path.Combine(GamePathResolver.ImmortalThronePath, "Resources", "XPack4");
				arcFile = Path.Combine(rootFolder, Path.ChangeExtension(arcFileBase, ".arc"));
				arcFileData = this.ReadARCFile(arcFile, resourceId);
			}

			if (arcFileData == null)
			{
				// We are either vanilla TQ or have not found our resource yet.
				// from the original TQ folder
				if (TQDebug.DatabaseDebugLevel > 1)
					Log.LogDebug("Checking TQ Resources.");

				rootFolder = GamePathResolver.TQPath;
				rootFolder = Path.Combine(rootFolder, "Resources");

				arcFile = Path.Combine(rootFolder, Path.ChangeExtension(arcFileBase, ".arc"));
				arcFileData = this.ReadARCFile(arcFile, resourceId);
			}

			if (TQDebug.DatabaseDebugLevel > 0)
				Log.LogDebug("Exiting Database.LoadResource()");

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
					Log.LogDebug("Database.ReadARCFile('{0}', '{1}')", arcFileName, dataId);

				ArcFile arcFile = this.arcFiles.GetOrAddAtomic(arcFileName, k =>
				{
					var file = new ArcFile(k);
					arcProv.ReadARCToC(file);// Heavy lifting in GetOrAddAtomic
					return file;
				});

				// Now retrieve the data
				byte[] ans = arcProv.GetData(arcFile, dataId);

				if (TQDebug.DatabaseDebugLevel > 0)
					Log.LogDebug("Exiting Database.ReadARCFile()");

				return ans;
			}
			catch (Exception e)
			{
				Log.LogError(e, "Exception occurred");
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
				Log.LogDebug("Database.FigureDBFileToUse({0})", isImmortalThrone);

			string rootFolder;
			if (isImmortalThrone)
			{
				if (GamePathResolver.ImmortalThronePath.Contains("Anniversary"))
					rootFolder = Path.Combine(GamePathResolver.ImmortalThronePath, "Text");
				else
					rootFolder = Path.Combine(GamePathResolver.ImmortalThronePath, "Resources");

				if (TQDebug.DatabaseDebugLevel > 1)
				{
					Log.LogDebug("Detecting Immortal Throne text files");
					Log.LogDebug("rootFolder = {0}", rootFolder);
				}
			}
			else
			{
				// from the original TQ folder
				rootFolder = Path.Combine(GamePathResolver.TQPath, "Text");

				if (TQDebug.DatabaseDebugLevel > 1)
				{
					Log.LogDebug("Detecting Titan Quest text files");
					Log.LogDebug("rootFolder = {0}", rootFolder);
				}
			}

			// make sure the damn directory exists
			if (!Directory.Exists(rootFolder))
			{
				if (TQDebug.DatabaseDebugLevel > 0)
					Log.LogDebug("Error - Root Folder does not exist");

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
				Log.LogDebug("gameLanguage = {0}", gameLanguage == null ? "NULL" : gameLanguage);
				Log.LogDebug("baseFile = {0}", baseFile);
			}

			if (gameLanguage != null)
			{
				// Try this method of getting the culture
				if (TQDebug.DatabaseDebugLevel > 2)
					Log.LogDebug("Try looking up cultureID");

				foreach (CultureInfo cultureInfo in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
				{
					if (TQDebug.DatabaseDebugLevel > 2)
						Log.LogDebug("Trying {0}", cultureInfo.EnglishName.ToUpperInvariant());

					if (cultureInfo.EnglishName.ToUpperInvariant().Equals(gameLanguage.ToUpperInvariant()) || cultureInfo.DisplayName.ToUpperInvariant().Equals(gameLanguage.ToUpperInvariant()))
					{
						cultureID = cultureInfo.TwoLetterISOLanguageName;
						break;
					}
				}

				// Titan Quest doesn't use the ISO language code for some languages
				// Added null check to fix exception when there is no culture found.
				if (cultureID != null)
				{
					if (cultureID.ToUpperInvariant() == "CS")
					{
						// Force Czech to use CZ instead of CS for the 2 letter code.
						cultureID = "CZ";
					} 
					else if (cultureID.ToUpperInvariant() == "PT")
					{
						// Force brazilian portuguese to use BR instead of PT
						cultureID = "BR";
					}
				}
					
				if (TQDebug.DatabaseDebugLevel > 1)
					Log.LogDebug("cultureID = {0}", cultureID);

				// Moved this inital check for the file into the loop
				// and added a check to verify that we actually have a cultureID
				if (cultureID != null)
				{
					filename = string.Concat(baseFile, cultureID, suffix);
					if (TQDebug.DatabaseDebugLevel > 1)
					{
						Log.LogDebug("Detected cultureID from gameLanguage");
						Log.LogDebug("filename = {0}", filename);
					}

					if (File.Exists(filename))
					{
						if (TQDebug.DatabaseDebugLevel > 0)
							Log.LogDebug("Exiting Database.FigureDBFileToUse()");

						return filename;
					}
				}
			}

			// try to use the default culture for the OS
			cultureID = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
			if (TQDebug.DatabaseDebugLevel > 1)
			{
				Log.LogDebug("Using cultureID from OS");
				Log.LogDebug("cultureID = {0}", cultureID);
			}

			// Added a check to verify that we actually have a cultureID
			// though it may not be needed
			if (cultureID != null)
			{
				filename = string.Concat(baseFile, cultureID, suffix);
				if (TQDebug.DatabaseDebugLevel > 1)
					Log.LogDebug("filename = {0}", filename);

				if (File.Exists(filename))
				{
					if (TQDebug.DatabaseDebugLevel > 0)
						Log.LogDebug("Exiting Database.FigureDBFileToUse()");

					return filename;
				}
			}

			// Now just try EN
			cultureID = "EN";
			filename = string.Concat(baseFile, cultureID, suffix);
			if (TQDebug.DatabaseDebugLevel > 1)
			{
				Log.LogDebug("Forcing English Language");
				Log.LogDebug("cultureID = {0}", cultureID);
				Log.LogDebug("filename = {0}", filename);
			}

			if (File.Exists(filename))
			{
				if (TQDebug.DatabaseDebugLevel > 0)
					Log.LogDebug("Database.Exiting FigureDBFileToUse()");

				return filename;
			}

			// Now just see if we can find anything.
			if (TQDebug.DatabaseDebugLevel > 0)
				Log.LogDebug("Detection Failed - searching for files");

			string[] files = Directory.GetFiles(rootFolder, "Text_??.arc");

			// Added check that files is not null.
			if (files != null && files.Length > 0)
			{
				if (TQDebug.DatabaseDebugLevel > 1)
				{
					Log.LogDebug("Found some files");
					Log.LogDebug("filename = {0}", files[0]);
				}

				if (TQDebug.DatabaseDebugLevel > 0)
					Log.LogDebug("Exiting Database.FigureDBFileToUse()");

				return files[0];
			}

			if (TQDebug.DatabaseDebugLevel > 0)
			{
				Log.LogDebug("Failed to determine Language file!");
				Log.LogDebug("Exiting Database.FigureDBFileToUse()");
			}

			return null;
		}

		/// <summary>
		/// Loads the Text database
		/// </summary>
		private void LoadTextDB()
		{
			if (TQDebug.DatabaseDebugLevel > 0)
				Log.LogDebug("Database.LoadTextDB()");

			string databaseFile = this.FigureDBFileToUse(false);
			if (TQDebug.DatabaseDebugLevel > 1)
			{
				Log.LogDebug("Find Titan Quest text file");
				Log.LogDebug("dbFile = {0}", databaseFile);
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
				this.ParseTextDB(databaseFile, "text\\tutorial.txt");

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

				if (GamePathResolver.IsRagnarokInstalled)
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

				if (GamePathResolver.IsAtlantisInstalled)
				{
					this.ParseTextDB(databaseFile, "text\\x3basegame_nonvoiced.txt");
					this.ParseTextDB(databaseFile, "text\\x3items_nonvoiced.txt");
					this.ParseTextDB(databaseFile, "text\\x3mainquest_nonvoiced.txt");
					this.ParseTextDB(databaseFile, "text\\x3misctags_nonvoiced.txt");
					this.ParseTextDB(databaseFile, "text\\x3sidequests_nonvoiced.txt");
				}

				if (GamePathResolver.IsEmbersInstalled)
				{
					this.ParseTextDB(databaseFile, "text\\x4basegame_nonvoiced.txt");
					this.ParseTextDB(databaseFile, "text\\x4items_nonvoiced.txt");
					this.ParseTextDB(databaseFile, "text\\x4mainquest_nonvoiced.txt");
					this.ParseTextDB(databaseFile, "text\\x4misctags_nonvoiced.txt");
					this.ParseTextDB(databaseFile, "text\\x4nametags_nonvoiced.txt");
					this.ParseTextDB(databaseFile, "text\\x4sidequests_nonvoiced.txt");
				}
			}

			// For loading custom map text database.
			if (GamePathResolver.IsCustom)
			{
				databaseFile = Path.Combine(GamePathResolver.MapName, "resources", "text.arc");

				if (TQDebug.DatabaseDebugLevel > 1)
				{
					Log.LogDebug("Find Custom Map text file");
					Log.LogDebug("dbFile = {0}", databaseFile);
				}

				if (databaseFile != null)
					this.ParseTextDB(databaseFile, "text\\modstrings.txt");
			}

			// Added this check to see if anything was loaded.
			if (this.textDB.Count == 0)
			{
				if (TQDebug.DatabaseDebugLevel > 0)
					Log.LogDebug("Exception - Could not load Text DB.");

				throw new FileLoadException("Could not load Text DB.");
			}

			if (TQDebug.DatabaseDebugLevel > 0)
				Log.LogDebug("Exiting Database.LoadTextDB()");
		}

		/// <summary>
		/// Parses the text database to put the entries into a hash table.
		/// </summary>
		/// <param name="databaseFile">Database file name (arc file)</param>
		/// <param name="filename">Name of the text DB file within the arc file</param>
		private void ParseTextDB(string databaseFile, string filename)
		{
			if (TQDebug.DatabaseDebugLevel > 0)
				Log.LogDebug("Database.ParseTextDB({0}, {1})", databaseFile, filename);

			byte[] data = this.ReadARCFile(databaseFile, filename);

			if (data == null)
			{
				// Changed for mod support.  Sometimes the text file has more entries than just the x or non-x prefix files.
				if (TQDebug.DatabaseDebugLevel > 0)
					Log.LogDebug("Error in ARC File: {0} does not contain an entry for '{1}'", databaseFile, filename);

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
							// If it was the only [...] tag in the string then take the whole string after the tag
							label = label.Substring(textStart);
						else
							// else take the string between the first 2 [...] tags
							label = label.Substring(textStart, textEnd - textStart);

						label = label.Trim();
					}

					// If this field is already in the db, then replace it
					string key = fields[0].Trim().ToUpperInvariant();
					this.textDB.AddOrUpdateAtomic(key, label);
				}
			}

			if (TQDebug.DatabaseDebugLevel > 0)
				Log.LogDebug("Exiting Database.ParseTextDB()");
		}

		/// <summary>
		/// Loads a database arz file.
		/// </summary>
		private void LoadARZFile()
		{
			if (TQDebug.DatabaseDebugLevel > 0)
				Log.LogDebug("Database.LoadARZFile()");

			// from the original TQ folder
			string file = Path.Combine(Path.Combine(GamePathResolver.TQPath, "Database"), "database.arz");

			if (TQDebug.DatabaseDebugLevel > 1)
			{
				Log.LogDebug("Load Titan Quest database arz file");
				Log.LogDebug("file = {0}", file);
			}

			this.ArzFile = new ArzFile(file);
			arzProv.Read(this.ArzFile);

			// now Immortal Throne expansion pack
			this.ArzFileIT = this.ArzFile;

			// Added to load a custom map database file.
			if (GamePathResolver.IsCustom)
			{
				file = Path.Combine(GamePathResolver.MapName, "database", $"{Path.GetFileName(GamePathResolver.MapName)}.arz");

				if (TQDebug.DatabaseDebugLevel > 1)
				{
					Log.LogDebug("Load Custom Map database arz file");
					Log.LogDebug("file = {0}", file);
				}

				if (File.Exists(file))
				{
					this.ArzFileMod = new ArzFile(file);
					arzProv.Read(this.ArzFileMod);
				}
				else
					this.ArzFileMod = null;
			}

			if (TQDebug.DatabaseDebugLevel > 0)
				Log.LogDebug("Exiting Database.LoadARZFile()");
		}

		#endregion Database Private Methods
	}
}