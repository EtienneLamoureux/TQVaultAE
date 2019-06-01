//-----------------------------------------------------------------------
// <copyright file="Database.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.DAL
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Drawing;
	using System.Globalization;
	using System.IO;
	using System.Text;
	using TQVaultAE.Logging;

	/// <summary>
	/// Enumeration of the compressed file types
	/// </summary>
	public enum CompressedFileType
	{
		/// <summary>
		/// Unknown file type
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// ARZ compressed file
		/// </summary>
		ArzFile = 1,

		/// <summary>
		/// ARC compressed file
		/// </summary>
		ArcFile = 2
	}

	/// <summary>
	/// Reads a Titan Quest database file.
	/// </summary>
	public class Database
	{
		private readonly log4net.ILog Log = null;

		#region Database Fields

		/// <summary>
		/// Item unit size in pixels for a 1x1 item.
		/// </summary>
		private const float ITEMUNITSIZE = 32.0F;

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
		/// Dictionary of all bitmaps in the database.
		/// </summary>
		private Dictionary<string, Bitmap> bitmaps;

		/// <summary>
		/// Default bitmap when one cannot be found in the database.
		/// </summary>
		private Bitmap defaultBitmap;

		/// <summary>
		/// Game language to support setting language in UI
		/// </summary>
		private string gameLanguage;

		/// <summary>
		/// Scaling factor used to scale the UI for higher DPI values than the default of 96.
		/// </summary>
		private float scale = 1.00F;

		#endregion Database Fields

		/// <summary>
		/// Initializes a new instance of the Database class.
		/// </summary>
		public Database()
		{
			this.Log = Logger.Get(this);

			this.arcFiles = new Dictionary<string, ArcFile>();
			this.bitmaps = new Dictionary<string, Bitmap>();
		}

		#region Database Properties

		/// <summary>
		/// Gets or sets the static database.
		/// </summary>
		public static Database DB { get; set; }

		/// <summary>
		/// Gets the UI design DPI which is used to for scaling comparisons.
		/// </summary>
		public static float DesignDpi
		{
			get
			{
				// Use 96 DPI which is "normal" for Windows.
				return 96.0F;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the game language is being auto detected.
		/// </summary>
		public bool AutoDetectLanguage { get; set; }

		/// <summary>
		/// Gets or sets the game language from the config file.
		/// </summary>
		public string TQLanguage { get; set; }

		/// <summary>
		/// Gets the Tooltip body tag.
		/// </summary>
		public string TooltipBodyTag
		{
			get
			{
				return string.Format(CultureInfo.CurrentCulture, "<body bgcolor=#2e291f text=white><font face=\"Albertus MT\" size={0}>", Convert.ToInt32(9.0F * this.Scale));
			}
		}

		/// <summary>
		/// Gets the Tooltip Title tag.
		/// </summary>
		public string TooltipTitleTag
		{
			get
			{
				return string.Format(CultureInfo.CurrentCulture, "<body bgcolor=#2e291f text=white><font face=\"Albertus MT\" size={0}>", Convert.ToInt32(10.0F * this.Scale));
			}
		}

		/// <summary>
		/// Gets the instance of the Titan Quest Database ArzFile.
		/// </summary>
		[CLSCompliantAttribute(false)]
		public ArzFile ArzFile { get; private set; }

		/// <summary>
		/// Gets the instance of the Immortal Throne Database ArzFile.
		/// </summary>
		[CLSCompliantAttribute(false)]
		public ArzFile ArzFileIT { get; private set; }

		/// <summary>
		/// Gets the instance of a custom map Database ArzFile.
		/// </summary>
		[CLSCompliantAttribute(false)]
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
					{
						this.gameLanguage = this.TQLanguage;
					}
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
									{
										continue;
									}

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

		/// <summary>
		/// Gets the default bitmap
		/// </summary>
		public Bitmap DefaultBitmap
		{
			get
			{
				if (this.defaultBitmap == null)
				{
					this.CreateDefaultBitmap();
				}

				return this.defaultBitmap;
			}
		}

		/// <summary>
		/// Gets the item unit size which is the unit of measure of item size in TQ.
		/// An item with a ItemUnitSize x ItemUnitSize bitmap would be 1x1.
		/// Internally scaled by db scale.
		/// </summary>
		public int ItemUnitSize
		{
			get
			{
				return Convert.ToInt32(ITEMUNITSIZE * this.Scale);
			}
		}

		/// <summary>
		/// Gets the half of an item unit size which is the unit of measure of item size in TQ.
		/// Division takes place after internal scaling by db scale.
		/// </summary>
		public int HalfUnitSize
		{
			get
			{
				return this.ItemUnitSize / 2;
			}
		}

		/// <summary>
		/// Gets or sets the scaling of the UI
		/// </summary>
		public float Scale
		{
			get
			{
				return this.scale;
			}

			set
			{
				// Set some bounds for the scale factors.
				if (value < 0.4F)
				{
					// Should be good enough for a 640 x 480 screen.
					value = 0.4F;
				}
				else if (value > 2.0F)
				{
					// Large fonts are 1.50 so this should be good enough.
					value = 2.0F;
				}

				this.scale = value;
			}
		}

		#endregion Database Properties

		#region Database Public Methods

		#region Database Public Static Methods

		/// <summary>
		/// Wraps the words in a text description.
		/// </summary>
		/// <param name="text">Text to be word wrapped</param>
		/// <param name="columns">maximum number of columns before wrapping</param>
		/// <returns>List of wrapped text</returns>
		public static Collection<string> WrapWords(string text, int columns)
		{
			List<string> choppedLines = new List<string>();

			int chopped = 0;
			int nextSpace = text.IndexOf(' ');

			// Added by VillageIdiot
			// Account for {^N} tag in text descriptions
			int nextLF = text.IndexOf("{^N}", StringComparison.OrdinalIgnoreCase);

			while (((text.Length - chopped) > columns) && nextSpace >= 0)
			{
				while ((nextSpace >= 0) && (nextSpace - chopped) < columns)
				{
					if (nextSpace < nextLF || nextLF < 0)
					{
						// Added check for LF tag
						nextSpace = text.IndexOf(' ', nextSpace + 1);
					}
					else if (nextLF >= 0)
					{
						// we need to split at ^N which occurs < column size
						string choppedPart = text.Substring(chopped, nextLF - chopped);
						chopped = nextLF + 4;
						choppedLines.Add(choppedPart);
						nextLF = text.IndexOf("{^N}", chopped, StringComparison.OrdinalIgnoreCase);
					}
				}

				if (nextSpace >= 0 && (nextSpace < nextLF || nextLF < 0))
				{
					// we need to split here.
					string choppedPart = text.Substring(chopped, nextSpace - chopped);
					// Added checks for LF tags
					if (nextLF == nextSpace + 1)
					{
						chopped = nextSpace + 5;
						nextLF = text.IndexOf("{^N}", chopped, StringComparison.OrdinalIgnoreCase);
					}
					else
					{
						chopped = nextSpace + 1;
					}
					choppedLines.Add(choppedPart);
				}
				else if (nextLF >= 0)
				{
					// we need to split at ^N which occurs > column size
					string choppedPart = text.Substring(chopped, nextLF - chopped);
					chopped = nextLF + 4;
					choppedLines.Add(choppedPart);
					nextLF = text.IndexOf("{^N}", chopped, StringComparison.OrdinalIgnoreCase);
				}
			}

			// We need to split a string that is < column size in length
			while (nextLF >= 0)
			{
				string choppedPart = text.Substring(chopped, nextLF - chopped);
				chopped = nextLF + 4;
				choppedLines.Add(choppedPart);
				nextLF = text.IndexOf("{^N}", chopped, StringComparison.OrdinalIgnoreCase);
			}

			choppedLines.Add(text.Substring(chopped));
			return new Collection<string>(choppedLines);
		}

		/// <summary>
		/// Takes plain text and replaces any characters that do not belong in html with the appropriate stuff.
		/// i.e. replaces > with &gt; etc
		/// </summary>
		/// <param name="text">Text to be formatted</param>
		/// <returns>Formatted text string</returns>
		public static string MakeSafeForHtml(string text)
		{
			text = System.Text.RegularExpressions.Regex.Replace(text, "&", "&amp;");
			text = System.Text.RegularExpressions.Regex.Replace(text, "<", "&lt;");
			text = System.Text.RegularExpressions.Regex.Replace(text, ">", "&gt;");
			return text;
		}

		/// <summary>
		/// Gets the HTML formatted color of a specified color
		/// </summary>
		/// <param name="color">Color to be HTMLized</param>
		/// <returns>string of HTML formatted color</returns>
		public static string HtmlColor(Color color)
		{
			return string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
		}

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
			{
				Logger.Log.DebugFormat(CultureInfo.InvariantCulture, "Database.ExtractARCFile('{0}', '{1}')", arcFileName, destination);
			}

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
			{
				Logger.Log.DebugFormat(CultureInfo.InvariantCulture, "Extraction Result = {0}", result);
			}

			if (TQDebug.DatabaseDebugLevel > 0)
			{
				Logger.Log.Debug("Exiting Database.ReadARCFile()");
			}

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
			{
				return result;
			}

			itemId = TQData.NormalizeRecordPath(itemId);
			Info info;

			if (infoDB.ContainsKey(itemId))
			{
				info = this.infoDB[itemId];
			}
			else
			{
				DBRecordCollection record = null;

				// Add support for searching a custom map database
				if (this.ArzFileMod != null)
				{
					record = this.ArzFileMod.GetItem(itemId);
				}

				// Try the expansion pack database first.
				if (record == null && this.ArzFileIT != null)
				{
					record = this.ArzFileIT.GetItem(itemId);
				}

				if (record == null || this.ArzFileIT == null)
				{
					// Try looking in TQ database now
					record = this.ArzFile.GetItem(itemId);
				}

				if (record == null)
				{
					return null;
				}

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
		[CLSCompliantAttribute(false)]
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
			ItemAttributesData data = ItemAttributes.GetAttributeData(itemAttribute);
			if (data == null)
			{
				return string.Concat("?", itemAttribute, "?");
			}

			string attributeTextTag = ItemAttributes.GetAttributeTextTag(data);
			if (string.IsNullOrEmpty(attributeTextTag))
			{
				return string.Concat("?", itemAttribute, "?");
			}

			string textFromTag = this.GetFriendlyName(attributeTextTag);
			if (string.IsNullOrEmpty(textFromTag))
			{
				textFromTag = string.Concat("ATTR<", itemAttribute, "> TAG<");
				textFromTag = string.Concat(textFromTag, attributeTextTag, ">");
			}

			if (addVariable && data.Variable.Length > 0)
			{
				textFromTag = string.Concat(textFromTag, " ", data.Variable);
			}

			return textFromTag;
		}

		/// <summary>
		/// Load our data from the db file.
		/// </summary>
		public void LoadDBFile()
		{
			this.LoadTextDB();
			this.LoadARZFile();

			this.infoDB = new Dictionary<string, Info>();

			// read the ARC file that has the bitmaps
			this.LoadRelicOverlayBitmap();
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
		[CLSCompliantAttribute(false)]
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
			{
				Log.DebugFormat(CultureInfo.InvariantCulture, "Database.LoadResource({0})", resourceId);
			}

			resourceId = TQData.NormalizeRecordPath(resourceId);
			if (TQDebug.DatabaseDebugLevel > 1)
			{
				Log.DebugFormat(CultureInfo.InvariantCulture, " Normalized({0})", resourceId);
			}

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
			{
				Log.DebugFormat(CultureInfo.InvariantCulture, "arcFileBase = {0}", arcFileBase);
			}

			string rootFolder;
			string arcFile;
			byte[] arcFileData = null;

			// Added by VillageIdiot
			// Check the mod folder for the image resource.
			if (TQData.IsCustom)
			{
				if (TQDebug.DatabaseDebugLevel > 1)
				{
					Log.Debug("Checking Custom Resources.");
				}

				rootFolder = Path.Combine(TQData.ImmortalThroneSaveFolder, "CustomMaps");
				rootFolder = Path.Combine(Path.Combine(rootFolder, TQData.MapName), "resources");

				arcFile = Path.Combine(rootFolder, Path.ChangeExtension(arcFileBase, ".arc"));
				arcFileData = this.ReadARCFile(arcFile, resourceId);
			}

			// We either didn't load the resource or didn't find what we were looking for so check the normal game resources.
			if (arcFileData == null)
			{
				// See if this guy is from Immortal Throne expansion pack.
				if (TQDebug.DatabaseDebugLevel > 1)
				{
					Log.Debug("Checking IT Resources.");
				}

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
                    {
                        // not a proper resourceID
                        return null;
                    }

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
				{
					Log.Debug("Checking TQ Resources.");
				}

				rootFolder = TQData.TQPath;
				rootFolder = Path.Combine(rootFolder, "Resources");

				arcFile = Path.Combine(rootFolder, Path.ChangeExtension(arcFileBase, ".arc"));
				arcFileData = this.ReadARCFile(arcFile, resourceId);
			}

			if (TQDebug.DatabaseDebugLevel > 0)
			{
				Log.Debug("Exiting Database.LoadResource()");
			}

			return arcFileData;
		}

		/// <summary>
		/// Loads a bitmap from a resource Id string
		/// </summary>
		/// <param name="resourceId">Resource Id which we are looking up.</param>
		/// <returns>Bitmap fetched from the database</returns>
		public Bitmap LoadBitmap(string resourceId)
		{
			Bitmap result = null;
			if (string.IsNullOrEmpty(resourceId))
			{
				return result;
			}

			if (TQDebug.DatabaseDebugLevel > 0)
			{
				Log.DebugFormat(CultureInfo.InvariantCulture, "Database.LoadBitmap({0})", resourceId);
			}

			resourceId = TQData.NormalizeRecordPath(resourceId);
			Bitmap bitmap;

			if (bitmaps.ContainsKey(resourceId))
			{
				bitmap = this.bitmaps[resourceId];
			}
			else
			{
				// Load the resource
				byte[] data = this.LoadResource(resourceId);

				if (data == null)
				{
					if (TQDebug.DatabaseDebugLevel > 0)
					{
						Log.Debug("Failure loading resource.  Using default bitmap");
					}

					// could not load the data.  Use a default bitmap
					bitmap = this.DefaultBitmap;
				}
				else
				{
					if (TQDebug.DatabaseDebugLevel > 1)
					{
						Log.DebugFormat(CultureInfo.InvariantCulture, "Loaded resource size={0}", data.Length);
					}

					// Create the bitmap
					bitmap = BitmapCode.LoadFromTexMemory(data, 0, data.Length);
					if (bitmap == null)
					{
						if (TQDebug.DatabaseDebugLevel > 0)
						{
							Log.DebugFormat(CultureInfo.InvariantCulture, "Failure creating bitmap from resource data len={0}", data.Length);
						}

						// could not create the bitmap
						bitmap = this.DefaultBitmap;
					}

					if (TQDebug.DatabaseDebugLevel > 1)
					{
						Log.DebugFormat(CultureInfo.InvariantCulture, "Created Bitmap {0} x {1}", bitmap.Width, bitmap.Height);
					}
				}

				this.bitmaps.Add(resourceId, bitmap);
			}

			if (TQDebug.DatabaseDebugLevel > 0)
			{
				Log.Debug("Exiting Database.LoadBitmap()");
			}

			return bitmap;
		}

		/// <summary>
		/// Loads the relic overlay bitmap from the database.
		/// </summary>
		/// <returns>Relic overlay bitmap</returns>
		public Bitmap LoadRelicOverlayBitmap()
		{
			Bitmap relicOverlayBitmap = this.LoadBitmap("Items\\Relic\\ItemRelicOverlay.tex");

			// do not return the defaultbitmap
			if (relicOverlayBitmap == this.DefaultBitmap)
			{
				return null;
			}

			return relicOverlayBitmap;
		}

		#endregion Database Public Methods

		#region Database Private Methods

		/// <summary>
		/// Creates a default bitmap for use when a bitmap cannot be found.
		/// </summary>
		private void CreateDefaultBitmap()
		{
			if (TQDebug.DatabaseDebugLevel > 0)
			{
				Log.Debug("Database.CreateDefaultBitmap()");
			}

			// Make a bitmap with a small orange square with a ? mark in it.
			Bitmap tempDefaultBitmap = new Bitmap(this.ItemUnitSize, this.ItemUnitSize);
			Graphics graphics = Graphics.FromImage(tempDefaultBitmap);

			// First fill the whole thing black so we can set our transparency
			SolidBrush black = new SolidBrush(Color.Black);
			graphics.FillRectangle(black, 0, 0, this.ItemUnitSize, this.ItemUnitSize);

			// Now draw the orange square
			SolidBrush orange = new SolidBrush(Color.Orange);
			float border = 5.0F * this.Scale; // amount of area to leave black
			graphics.FillRectangle(orange, Convert.ToInt32(border), Convert.ToInt32(border), this.ItemUnitSize - Convert.ToInt32(2.0F * border), this.ItemUnitSize - Convert.ToInt32(2.0F * border));

			// Now put the Question Mark at the center
			// use a color that is slightly off-black so it does not become transparent
			SolidBrush textBrush = new SolidBrush(Color.FromArgb(1, 1, 1));
			Font textFont = new Font("Arial", (float)this.ItemUnitSize - Convert.ToInt32(4.0F * border), GraphicsUnit.Pixel);
			StringFormat textFormat = new StringFormat();
			textFormat.Alignment = StringAlignment.Center;

			graphics.DrawString("?", textFont, textBrush, new RectangleF(border, border, (float)this.ItemUnitSize - (2.0F * border), (float)this.ItemUnitSize - (2.0F * border)), textFormat);

			// now set our transparency
			tempDefaultBitmap.MakeTransparent(Color.Black);

			// all done
			this.defaultBitmap = tempDefaultBitmap;

			if (TQDebug.DatabaseDebugLevel > 0)
			{
				Log.Debug("Exiting Database.CreateDefaultBitmap()");
			}
		}

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
				{
					Log.DebugFormat(CultureInfo.InvariantCulture, "Database.ReadARCFile('{0}', '{1}')", arcFileName, dataId);
				}

				ArcFile arcFile;

				if (arcFiles.ContainsKey(arcFileName))
				{
					arcFile = this.arcFiles[arcFileName];
				}
				else
				{
					arcFile = new ArcFile(arcFileName);
					this.arcFiles.Add(arcFileName, arcFile);
				}

				// Now retrieve the data
				byte[] ans = arcFile.GetData(dataId);

				if (TQDebug.DatabaseDebugLevel > 0)
				{
					Log.Debug("Exiting Database.ReadARCFile()");
				}

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
			{
				Log.DebugFormat(CultureInfo.InvariantCulture, "Database.FigureDBFileToUse({0})", isImmortalThrone);
			}

			string rootFolder;
			if (isImmortalThrone)
			{
				//rootFolder = Path.Combine(TQData.ImmortalThronePath, "Resources");
				if (TQData.ImmortalThronePath.Contains("Anniversary"))
				{
					rootFolder = Path.Combine(TQData.ImmortalThronePath, "Text");
				}
				else
				{
					rootFolder = Path.Combine(TQData.ImmortalThronePath, "Resources");
				}

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
				{
					Log.Debug("Error - Root Folder does not exist");
				}

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
				{
					Log.Debug("Try looking up cultureID");
				}

				foreach (CultureInfo cultureInfo in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
				{
					if (TQDebug.DatabaseDebugLevel > 2)
					{
						Log.DebugFormat(CultureInfo.InvariantCulture, "Trying {0}", cultureInfo.EnglishName.ToUpperInvariant());
					}

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
				{
					cultureID = "CZ";
				}

				if (TQDebug.DatabaseDebugLevel > 1)
				{
					Log.DebugFormat(CultureInfo.InvariantCulture, "cultureID = {0}", cultureID);
				}

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
						{
							Log.Debug("Exiting Database.FigureDBFileToUse()");
						}

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
				{
					Log.DebugFormat(CultureInfo.InvariantCulture, "filename = {0}", filename);
				}

				if (File.Exists(filename))
				{
					if (TQDebug.DatabaseDebugLevel > 0)
					{
						Log.Debug("Exiting Database.FigureDBFileToUse()");
					}

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
				{
					Log.Debug("Database.Exiting FigureDBFileToUse()");
				}

				return filename;
			}

			// Now just see if we can find anything.
			if (TQDebug.DatabaseDebugLevel > 0)
			{
				Log.Debug("Detection Failed - searching for files");
			}

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
				{
					Log.Debug("Exiting Database.FigureDBFileToUse()");
				}

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
			{
				Log.Debug("Database.LoadTextDB()");
			}

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

                if (TQData.IsRagnarokInstalled) {
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
				string baseFolder = Path.Combine(TQData.ImmortalThroneSaveFolder, "CustomMaps");

				databaseFile = Path.Combine(Path.Combine(Path.Combine(baseFolder, TQData.MapName), "resources"), "text.arc");

				if (TQDebug.DatabaseDebugLevel > 1)
				{
					Log.Debug("Find Custom Map text file");
					Log.DebugFormat(CultureInfo.InvariantCulture, "dbFile = {0}", databaseFile);
				}

				if (databaseFile != null)
				{
					this.ParseTextDB(databaseFile, "text\\modstrings.txt");
				}
			}

			// Added this check to see if anything was loaded.
			if (this.textDB.Count == 0)
			{
				if (TQDebug.DatabaseDebugLevel > 0)
				{
					Log.Debug("Exception - Could not load Text DB.");
				}

				throw new FileLoadException("Could not load Text DB.");
			}

			if (TQDebug.DatabaseDebugLevel > 0)
			{
				Log.Debug("Exiting Database.LoadTextDB()");
			}
		}

		/// <summary>
		/// Parses the text database to put the entries into a hash table.
		/// </summary>
		/// <param name="databaseFile">Database file name (arc file)</param>
		/// <param name="filename">Name of the text DB file within the arc file</param>
		private void ParseTextDB(string databaseFile, string filename)
		{
			if (TQDebug.DatabaseDebugLevel > 0)
			{
				Log.DebugFormat(CultureInfo.InvariantCulture, "Database.ParseTextDB({0}, {1})", databaseFile, filename);
			}

			byte[] data = this.ReadARCFile(databaseFile, filename);

			if (data == null)
			{
				// Changed for mod support.  Sometimes the text file has more entries than just the x or non-x prefix files.
				if (TQDebug.DatabaseDebugLevel > 0)
				{
					Log.DebugFormat(CultureInfo.InvariantCulture, "Error in ARC File: {0} does not contain an entry for '{1}'", databaseFile, filename);
				}

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
					{
						continue;
					}

					// comment line
					if (line.StartsWith("//", StringComparison.Ordinal))
					{
						continue;
					}

					// split on the equal sign
					string[] fields = line.Split(delimiter);

					// bad line
					if (fields.Length < 2)
					{
						continue;
					}

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
			{
				Log.Debug("Exiting Database.ParseTextDB()");
			}
		}

		/// <summary>
		/// Loads a database arz file.
		/// </summary>
		private void LoadARZFile()
		{
			if (TQDebug.DatabaseDebugLevel > 0)
			{
				Log.Debug("Database.LoadARZFile()");
			}

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
			/* if (TQData.IsITInstalled)
			{
				file = Path.Combine(Path.Combine(TQData.ImmortalThronePath, "Database"), "database.arz");

				if (TQDebug.DatabaseDebugLevel > 1)
				{
					Log.Debug("Load Immortal Throne database arz file");
					Log.DebugFormat(CultureInfo.InvariantCulture, "file = {0}", file));
				}

				if (File.Exists(file))
				{
					this.ArzFileIT = new ArzFile(file);
					this.ArzFileIT.Read();
				}
			} */

			// Added to load a custom map database file.
			if (TQData.IsCustom)
			{
				string baseFolder = Path.Combine(TQData.ImmortalThroneSaveFolder, "CustomMaps");

				file = Path.Combine(Path.Combine(Path.Combine(baseFolder, TQData.MapName), "database"), string.Concat(TQData.MapName, ".arz"));

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
				{
					this.ArzFileMod = null;
				}
			}

			if (TQDebug.DatabaseDebugLevel > 0)
			{
				Log.Debug("Exiting Database.LoadARZFile()");
			}
		}

		#endregion Database Private Methods
	}
}