using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Contracts.Providers
{
	public interface IDatabase
	{
		/// <summary>
		/// Gets the instance of the Titan Quest Database ArzFile.
		/// </summary>
		ArzFile ArzFile { get; }
		/// <summary>
		/// Gets the instance of the Immortal Throne Database ArzFile.
		/// </summary>
		ArzFile ArzFileIT { get; }
		/// <summary>
		/// Gets the instance of a custom map Database ArzFile.
		/// </summary>
		ArzFile ArzFileMod { get; }
		/// <summary>
		/// Gets or sets a value indicating whether the game language is being auto detected.
		/// </summary>
		bool AutoDetectLanguage { get; set; }
		/// <summary>
		/// Gets the game language setting as a an English DisplayName.
		/// </summary>
		/// <remarks>Changed to property by VillageIdiot to support changing of Language in UI</remarks>
		string GameLanguage { get; }
		/// <summary>
		/// Gets or sets the game language from the config file.
		/// </summary>
		string TQLanguage { get; set; }

		/// <summary>
		/// Used to Extract an ARC file into the destination directory.
		/// The ARC file will not be added to the cache.
		/// </summary>
		/// <remarks>Added by VillageIdiot</remarks>
		/// <param name="arcFileName">Name of the arc file</param>
		/// <param name="destination">Destination path for extracted data</param>
		/// <returns>Returns true on success otherwise false</returns>
		bool ExtractArcFile(string arcFileName, string destination);
		/// <summary>
		/// Uses the text database to convert the tag to a name in the localized language.
		/// The tag is normalized to upper case internally.
		/// </summary>
		/// <param name="tagId">Tag to be looked up in the text database normalized to upper case.</param>
		/// <returns>Returns localized string, tagId if it cannot find a string or "?ErrorName?" in case of uncaught exception.</returns>
		string GetFriendlyName(string tagId);
		/// <summary>
		/// Gets the Infor for a specific item id.
		/// </summary>
		/// <param name="itemId">Item ID which we are looking up.  Will be normalized internally.</param>
		/// <returns>Returns Infor for item ID and NULL if not found.</returns>
		Info GetInfo(string itemId);
		/// <summary>
		/// Converts the item attribute to a name in the localized language
		/// </summary>
		/// <param name="itemAttribute">Item attribure to be looked up.</param>
		/// <param name="addVariable">Flag for whether the variable is added to the text string.</param>
		/// <returns>Returns localized item attribute</returns>
		string GetItemAttributeFriendlyText(string itemAttribute, bool addVariable = true);
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
		DBRecordCollection GetRecordFromFile(string itemId);
		/// <summary>
		/// Gets a resource from the database using the resource Id.
		/// Modified by VillageIdiot to support loading resources from a custom map folder.
		/// </summary>
		/// <param name="resourceId">Resource which we are fetching</param>
		/// <returns>Retruns a byte array of the resource.</returns>
		byte[] LoadResource(string resourceId);
		/// <summary>
		/// Gets the formatted string for the variable attribute.
		/// </summary>
		/// <param name="variable">variable for which we are making a nice string.</param>
		/// <returns>Formatted string in the format of:  Attribute: value</returns>
		string VariableToStringNice(Variable variable);
	}
}