//-----------------------------------------------------------------------
// <copyright file="Info.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Domain.Entities
{
	using System;
	using TQVaultAE.Domain.Helpers;

	/// <summary>
	/// Holds information on magical prefixes and suffixes
	/// </summary>
	public class Info
	{
		private const StringComparison noCase = StringComparison.OrdinalIgnoreCase;
		// tags
		// animalrelics - description relicBitmap shardBitmap Class itemClassification completedRelicLevel
		// equipment* - itemQualityTag itemStyleTag itemNameTag bitmap Class itemClassification
		// lootmagicalaffixes - lootRandomizerName itemClassification
		// miscellaneous\oneshot - description bitmap Class itemClassification
		// questitems - description bitmap Class itemClassification
		// relics - description relicBitmap shardBitmap Class itemClassification completedRelicLevel
		// dyes - description bitmap Class
		// sets - tagSetName

		/// <summary>
		/// database record
		/// </summary>
		private DBRecordCollection record;

		/// <summary>
		/// description variable
		/// </summary>
		private string descriptionVar;

		/// <summary>
		/// item classification variable
		/// </summary>
		private string itemClassificationVar;

		/// <summary>
		/// bitmap variable
		/// </summary>
		private string bitmapVar;

		/// <summary>
		/// shard bitmap variable
		/// </summary>
		private string shardBitmapVar;

		/// <summary>
		/// item class variable
		/// </summary>
		private string itemClassVar;

		/// <summary>
		/// completed relic level variable
		/// </summary>
		private string completedRelicLevelVar;

		/// <summary>
		/// item quality variable
		/// </summary>
		private string qualityVar;

		/// <summary>
		/// item style variable
		/// </summary>
		private string styleVar;

		/// <summary>
		/// itemscalepercent attribute
		/// </summary>
		private string itemScalePercent;

		/// <summary>
		/// Initializes a new instance of the Info class.
		/// </summary>
		/// <param name="record">database record for which this info is for.</param>

		public Info(DBRecordCollection record)
		{
			this.record = record;
			this.AssignVariableNames();
		}

		#region Info Properties

		/// <summary>
		/// Gets the item ID
		/// </summary>
		public RecordId ItemId => this.record.Id;

		/// <summary>
		/// Gets the item description tag
		/// </summary>
		public string DescriptionTag => this.GetString(this.descriptionVar);

		/// <summary>
		/// Gets the item classification
		/// </summary>
		public string ItemClassification => this.GetString(this.itemClassificationVar);

		/// <summary>
		/// Gets the item quality tag
		/// </summary>
		public string QualityTag => this.GetString(this.qualityVar);

		/// <summary>
		/// Gets the item style tag
		/// </summary>
		public string StyleTag => this.GetString(this.styleVar);

		/// <summary>
		/// Gets the item bitmap
		/// </summary>
		public RecordId Bitmap => this.GetString(this.bitmapVar);

		/// <summary>
		/// Gets the item shard bitmap
		/// </summary>
		public RecordId ShardBitmap => this.GetString(this.shardBitmapVar);

		/// <summary>
		/// Gets the item class
		/// </summary>
		public string ItemClass => this.GetString(this.itemClassVar);

		/// <summary>
		/// Gets the relic level
		/// </summary>
		public int CompletedRelicLevel => this.GetInt32(this.completedRelicLevelVar);

		/// <summary>
		/// Gets the item scale percentage
		/// </summary>
		public float ItemScalePercent => 1.0F + (this.GetSingle(this.itemScalePercent) / 100);

		#endregion Info Properties

		#region Info Public Methods

		/// <summary>
		/// Gets a string from the record
		/// </summary>
		/// <param name="variable">variable which we are getting the string from</param>
		/// <returns>string from the variable.</returns>
		public string GetString(string variable) => this.record.GetString(variable, 0);

		/// <summary>
		/// Gets an int from the record
		/// </summary>
		/// <param name="variable">variable which we are getting the integer from</param>
		/// <returns>integer value from the variable.</returns>
		public int GetInt32(string variable) => this.record.GetInt32(variable, 0);

		/// <summary>
		/// Gets a float value
		/// </summary>
		/// <param name="variable">variable which we are getting the float from</param>
		/// <returns>float value from the variable</returns>
		public float GetSingle(string variable) => this.record.GetSingle(variable, 0);

		#endregion Info Public Methods

		#region Info Private Methods

		/// <summary>
		/// Find the type using the type from the database record instead of the path.
		/// </summary>
		/// <remarks>
		/// Changed by VillageIdiot
		/// </remarks>
		private void AssignVariableNames()
		{
			string id = this.record.RecordType;

			if (id.StartsWith("LOOTRANDOMIZER", noCase))
			{
				this.descriptionVar = "lootRandomizerName";
				this.itemClassificationVar = "itemClassification";
				this.bitmapVar = string.Empty;
				this.shardBitmapVar = string.Empty;
				this.itemClassVar = string.Empty;
				this.completedRelicLevelVar = string.Empty;
				this.qualityVar = string.Empty;
				this.styleVar = string.Empty;
			}
			else if (id.StartsWith(Item.ICLASS_RELIC, noCase) || id.StartsWith(Item.ICLASS_CHARM, noCase))
			{
				this.descriptionVar = "description";
				this.itemClassificationVar = "itemClassification";
				this.bitmapVar = "relicBitmap";
				this.shardBitmapVar = "shardBitmap";
				this.itemClassVar = "Class";
				this.completedRelicLevelVar = "completedRelicLevel";
				this.qualityVar = string.Empty;
				this.styleVar = "itemText";
			}
			else if (id.StartsWith(Item.ICLASS_DYE, noCase))
			{
				this.descriptionVar = "description";
				this.itemClassificationVar = string.Empty;
				this.bitmapVar = "bitmap";
				this.shardBitmapVar = string.Empty;
				this.itemClassVar = "Class";
				this.completedRelicLevelVar = string.Empty;
				this.qualityVar = string.Empty;
				this.styleVar = string.Empty;
			}
			else if (id.StartsWith("ONESHOT", noCase) || id.StartsWith(Item.ICLASS_QUESTITEM, noCase))
			{
				this.descriptionVar = "description";
				this.itemClassificationVar = "itemClassification";
				this.bitmapVar = "bitmap";
				this.shardBitmapVar = string.Empty;
				this.itemClassVar = "Class";
				this.completedRelicLevelVar = string.Empty;
				this.qualityVar = string.Empty;
				this.styleVar = "itemText";
			}
			else if (id.StartsWith(Item.ICLASS_FORMULA, noCase))
			{
				this.descriptionVar = "description";
				this.itemClassificationVar = "itemClassification";
				this.bitmapVar = "artifactFormulaBitmapName";
				this.shardBitmapVar = string.Empty;
				this.itemClassVar = "Class"; // ItemArtifactFormula
				this.completedRelicLevelVar = string.Empty;
				this.qualityVar = string.Empty;
				this.styleVar = string.Empty;
			}
			else if (id.StartsWith(Item.ICLASS_ARTIFACT, noCase))
			{
				this.descriptionVar = "description";
				this.itemClassificationVar = "itemClassification";
				this.bitmapVar = "artifactBitmap";
				this.shardBitmapVar = string.Empty;
				this.itemClassVar = "Class"; // ItemArtifact
				this.completedRelicLevelVar = string.Empty;
				this.qualityVar = string.Empty;
				this.styleVar = string.Empty;
			}
			else if (id.StartsWith(Item.ICLASS_EQUIPMENT, noCase))
			{
				this.descriptionVar = "description";
				this.itemClassificationVar = "itemClassification";
				this.bitmapVar = "bitmap";
				this.shardBitmapVar = string.Empty;
				this.itemClassVar = "Class"; // ItemEquipment
				this.completedRelicLevelVar = string.Empty;
				this.qualityVar = string.Empty;
				this.styleVar = "itemText";
			}
			else if (id.StartsWith("Skill_Mastery", noCase))
			{
				this.descriptionVar = "skillDisplayName";//skillBaseDescription
				this.itemClassificationVar = "itemClassification";
				this.bitmapVar = "skillUpBitmapName";
				this.shardBitmapVar = string.Empty;
				this.itemClassVar = "Class";
				this.completedRelicLevelVar = string.Empty;
				this.qualityVar = string.Empty;
				this.styleVar = string.Empty;
			}
			else if (id.Equals(Item.ICLASS_RANGEDONEHAND, noCase))
			{
				this.descriptionVar = "itemNameTag";
				this.itemClassificationVar = "itemClassification";
				this.bitmapVar = "bitmap";
				this.shardBitmapVar = string.Empty;
				this.itemClassVar = "Class";
				this.completedRelicLevelVar = string.Empty;
				this.qualityVar = "itemQualityTag";
				this.styleVar = "itemText";
			}
			else
			{
				this.descriptionVar = "itemNameTag";
				this.itemClassificationVar = "itemClassification";
				this.bitmapVar = "bitmap";
				this.shardBitmapVar = string.Empty;
				this.itemClassVar = "Class";
				this.completedRelicLevelVar = string.Empty;
				this.qualityVar = "itemQualityTag";
				this.styleVar = "itemStyleTag";
			}

			this.itemScalePercent = "itemScalePercent";
		}

		#endregion Info Private Methods
	}
}