//-----------------------------------------------------------------------
// <copyright file="Item.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Data;

/// <summary>
/// Class for holding item information
/// </summary>
public partial class ItemProvider : IItemProvider
{
	private const StringComparison noCase = StringComparison.OrdinalIgnoreCase;

	private readonly ILogger Log;
	private readonly IDatabase Database;
	private readonly ILootTableCollectionProvider LootTableCollectionProvider;
	private readonly IItemAttributeProvider ItemAttributeProvider;
	private readonly ITQDataService TQData;
	private readonly ITranslationService TranslationService;
	private readonly IPathIO PathIO;
	private readonly UserSettings USettings;
	private readonly LazyConcurrentDictionary<(Item Item, FriendlyNamesExtraScopes? Scope, bool FilterExtra), ToFriendlyNameResult> FriendlyNamesCache = new LazyConcurrentDictionary<(Item, FriendlyNamesExtraScopes?, bool), ToFriendlyNameResult>();
	private readonly LazyConcurrentDictionary<RecordId, ItemAffixes> ItemAffixesCache = new();


	public ItemProvider(
			ILogger<ItemProvider> log
			, IDatabase database
			, ILootTableCollectionProvider lootTableCollectionProvider
			, IItemAttributeProvider itemAttributeProvider
			, ITQDataService tQData
			, ITranslationService translationService, IPathIO pathIO
			, UserSettings uSettings
		)
	{
		this.Log = log;
		this.Database = database;
		this.LootTableCollectionProvider = lootTableCollectionProvider;
		this.ItemAttributeProvider = itemAttributeProvider;
		this.TQData = tQData;
		this.TranslationService = translationService;
		PathIO = pathIO;
		USettings = uSettings;
	}


	public bool InvalidateFriendlyNamesCache(params IEnumerable<Item> items)
	{
		items = items.Where(i => i != null).ToList();
		var keylist = this.FriendlyNamesCache.Where(i => items.Contains(i.Key.Item)).Select(i => i.Key).ToList();
		foreach (var k in keylist) this.FriendlyNamesCache.TryRemove(k, out var outVal);
		return keylist.Any();
	}

	/// <summary>
	/// Formats a string based on the formatspec
	/// </summary>
	/// <param name="formatSpec">format specification</param>
	/// <param name="parameter1">first parameter</param>
	/// <param name="parameter2">second parameter</param>
	/// <param name="parameter3">third parameter</param>
	/// <returns>formatted string.</returns>
	public string Format(string formatSpec, object parameter1, object parameter2 = null, object parameter3 = null)
	{
		try
		{
			return string.Format(CultureInfo.CurrentCulture, formatSpec, parameter1, parameter2, parameter3);
		}
		catch (ArgumentException)
		{
			string parameters = string.Format(
				CultureInfo.InvariantCulture,
				"\", '{0}', '{1}', '{2}'>",
				parameter1 == null ? "NULL" : parameter1,
				parameter2 == null ? "NULL" : parameter2,
				parameter3 == null ? "NULL" : parameter3);

			string error = string.Concat("FormatErr(\"", formatSpec, parameters);

			Log.LogDebug(error);

			return error;
		}
	}

	/// <summary>
	/// Gets the item name and attributes.
	/// </summary>
	/// <param name="itm"></param>
	/// <param name="scopes">Extra data scopes as a bitmask</param>
	/// <param name="filterExtra">filter extra properties</param>
	/// <returns>An object containing the item name and attributes</returns>
	public ToFriendlyNameResult GetFriendlyNames(Item itm, FriendlyNamesExtraScopes? scopes = null, bool filterExtra = true)
	{
		var key = (itm, scopes, filterExtra);
		return FriendlyNamesCache.GetOrAddAtomic(key, k =>
		{

			var res = new ToFriendlyNameResult(k.Item);
			k.Item.CurrentFriendlyNameResult = res;

			#region Minimal Info (ButtonBag tooltip + Common item properties)

			// Item Seed
			res.ItemSeed = string.Format(CultureInfo.CurrentCulture, this.TranslationService.ItemSeed, k.Item.Seed, (k.Item.Seed != 0) ? (k.Item.Seed / (float)Int16.MaxValue) : 0.0f);
			res.ItemQuest = this.TranslationService.ItemQuest;

			res.ItemThrown = itm.IsThrownWeapon ? this.TranslationService.TranslateXTag("x2tagThrownWeapon") : null;

			res.ItemOrigin = itm.GameDlc switch
			{
				GameDlc.Atlantis => this.TranslationService.ItemAtlantis,
				GameDlc.EternalEmbers => this.TranslationService.ItemEmbers,
				GameDlc.Ragnarok => this.TranslationService.ItemRagnarok,
				GameDlc.ImmortalThrone => this.TranslationService.ItemIT,
				_ => null
			};

			#region Prefix translation

			if (!k.Item.IsRelicOrCharm && !RecordId.IsNullOrEmpty(k.Item.prefixID))
			{
				res.PrefixInfoDescription = k.Item.prefixID.Raw;
				if (k.Item.prefixInfo != null)
				{
					if (TranslationService.TryTranslateXTag(k.Item.prefixInfo.DescriptionTag, out var desc))
						res.PrefixInfoDescription = desc.TQCleanup(true);
				}
			}

			#endregion

			#region Base Item translation

			// Load common relic translations if item is relic related by any means
			if (k.Item.IsRelicOrCharm || k.Item.HasRelicOrCharmSlot1 || k.Item.HasRelicOrCharmSlot2 || k.Item.RelicInfo != null || k.Item.Relic2Info != null)
			{
				res.ItemWith = this.TranslationService.ItemWith;

				if (k.Item.RelicInfo != null)
					TranslationService.TryTranslateXTag(k.Item.RelicInfo.DescriptionTag, out res.RelicInfo1Description);

				if (k.Item.Relic2Info != null)
					TranslationService.TryTranslateXTag(k.Item.Relic2Info.DescriptionTag, out res.RelicInfo2Description);

				var labelCompleted = "Completed";
				if (!TranslationService.TryTranslateXTag("tagAnimalPartComplete", out res.AnimalPartComplete))
					res.AnimalPartComplete = labelCompleted;

				if (!TranslationService.TryTranslateXTag("tagRelicComplete", out res.RelicComplete))
					res.RelicComplete = labelCompleted;

				var labelPartcomplete = "Completion Bonus: ";
				if (!TranslationService.TryTranslateXTag("tagAnimalPartcompleteBonus", out res.AnimalPartCompleteBonus))
					res.AnimalPartCompleteBonus = labelPartcomplete;

				if (!TranslationService.TryTranslateXTag("tagRelicBonus", out res.RelicBonus))
					res.RelicBonus = labelPartcomplete;

				var labelRelic = "Relic";
				if (!TranslationService.TryTranslateXTag("tagAnimalPart", out res.AnimalPart))
					res.AnimalPart = labelRelic;

				if (!TranslationService.TryTranslateXTag("tagRelicShard", out res.RelicShard))
					res.RelicShard = labelRelic;

				var labelRelicPattern = "{0} - {1} / {2}";
				if (TranslationService.TryTranslateXTag("tagAnimalPartRatio", out res.AnimalPartRatio))
					res.AnimalPartRatio = ItemAttributeProvider.ConvertFormat(res.AnimalPartRatio);
				else
					res.AnimalPartRatio = labelRelicPattern;

				if (TranslationService.TryTranslateXTag("tagRelicRatio", out res.RelicRatio))
					res.RelicRatio = ItemAttributeProvider.ConvertFormat(res.RelicRatio);
				else
					res.RelicRatio = labelRelicPattern;
			}

			res.BaseItemId = k.Item.BaseItemId;

			// Set Rarity translation
			res.BaseItemRarity = TranslationService.Translate(k.Item.ItemStyle);

			if (k.Item.baseItemInfo != null)
			{
				// style quality description
				if (!string.IsNullOrEmpty(k.Item.baseItemInfo.StyleTag))
				{
					if (!k.Item.IsPotion && !k.Item.IsRelicOrCharm && !k.Item.IsScroll && !k.Item.IsParchment && !k.Item.IsQuestItem)
					{
						if (!TranslationService.TryTranslateXTag(k.Item.baseItemInfo.StyleTag, out res.BaseItemInfoStyle))
							res.BaseItemInfoStyle = k.Item.baseItemInfo.StyleTag;
					}
				}

				if (!string.IsNullOrEmpty(k.Item.baseItemInfo.QualityTag))
				{
					if (!TranslationService.TryTranslateXTag(k.Item.baseItemInfo.QualityTag, out res.BaseItemInfoQuality))
						res.BaseItemInfoQuality = k.Item.baseItemInfo.QualityTag;
				}

				if (!TranslationService.TryTranslateXTag(k.Item.baseItemInfo.DescriptionTag, out res.BaseItemInfoDescription))
					res.BaseItemInfoDescription = k.Item.BaseItemId.Raw;

				res.BaseItemInfoClass = TranslationService.TranslateXTag(k.Item.ItemClassTagName, removeAllTQTags: true);

				res.BaseItemInfoRecords = Database.GetRecordFromFile(k.Item.BaseItemId);

				if (k.Item.IsRelicOrCharm)
				{
					// Add the number of charms in the set acquired.
					if (k.Item.IsRelicComplete)
					{
						if (k.Item.IsCharmOnly)
						{
							res.RelicCompletionFormat = res.AnimalPartComplete;
							res.RelicBonusTitle = res.AnimalPartCompleteBonus;
						}
						else
						{
							res.RelicCompletionFormat = res.RelicComplete;
							res.RelicBonusTitle = res.RelicBonus;
						}

						if (!RecordId.IsNullOrEmpty(k.Item.RelicBonusId))
						{
							res.RelicBonusFileName = k.Item.RelicBonusId.PrettyFileName;
							res.RelicBonusPattern = "{0} {1}";
							res.RelicBonusFormat = string.Format(CultureInfo.CurrentCulture, res.RelicBonusPattern
								, res.RelicBonusTitle
								, TQColor.Yellow.ColorTag() + res.RelicBonusFileName
							);
						}
						else
						{
							res.RelicBonusPattern = "{0}";
							res.RelicBonusFormat = string.Format(CultureInfo.CurrentCulture, res.RelicBonusPattern, res.RelicBonusTitle);
						}
					}
					else
					{
						if (k.Item.IsCharmOnly)
						{
							res.RelicClass = res.AnimalPart;
							res.RelicPattern = res.AnimalPartRatio;
						}
						else
						{
							res.RelicClass = res.RelicShard;
							res.RelicPattern = res.RelicRatio;
						}

						res.RelicCompletionFormat = Format(res.RelicPattern, res.RelicClass, k.Item.Number, k.Item.baseItemInfo.CompletedRelicLevel);
						res.RelicBonusFormat = res.RelicCompletionFormat;
					}

				}
				else if (k.Item.IsArtifact)
				{
					// Add Artifact completion bonus
					if (!RecordId.IsNullOrEmpty(k.Item.RelicBonusId))
					{
						var RelicBonusIdExt = PathIO.GetFileNameWithoutExtension(k.Item.RelicBonusId.Normalized);
						res.ArtifactBonus = TranslationService.TranslateXTag("xtagArtifactBonus");
						res.ArtifactBonusFormat = string.Format(CultureInfo.CurrentCulture, "({0} {1})", res.ArtifactBonus, RelicBonusIdExt);
					}

					// Show Artifact Class (Lesser / Greater / Divine).
					string artifactClassification = k.Item.baseItemInfo.GetString("artifactClassification").ToUpperInvariant();
					res.ArtifactClass = TranslateArtifactClassification(artifactClassification);

				}
				else if (k.Item.IsFormulae)
				{
					// Added to show recipe type for Formulae
					if (!TranslationService.TryTranslateXTag("xtagArtifactRecipe", out res.ArtifactRecipe))
						res.ArtifactRecipe = "Recipe";

					// Get Reagents format
					if (TranslationService.TryTranslateXTag("xtagArtifactReagents", out res.ArtifactReagents))
						res.ArtifactReagents = ItemAttributeProvider.ConvertFormat(res.ArtifactReagents);
					else
						res.ArtifactReagents = "Required Reagents  ({0}/{1})";


					// it looks like the formulae reagents is hard coded at 3
					res.FormulaeFormat = Format(res.ArtifactReagents, (object)0, 3);
					res.FormulaeFormat = $"{TQColor.Orange.ColorTag()}{res.FormulaeFormat}";

				}
				else if (k.Item.DoesStack)
				{
					// display the # potions
					if (k.Item.Number > 1)
						res.NumberFormat = string.Format(CultureInfo.CurrentCulture, "({0:n0})", k.Item.Number);
				}
			}

			#endregion

			#region Suffix translation

			if (!k.Item.IsRelicOrCharm && !RecordId.IsNullOrEmpty(k.Item.suffixID))
			{
				if (k.Item.suffixInfo != null)
				{
					res.SuffixInfoDescription =
							TranslationService.TryTranslateXTag(k.Item.suffixInfo.DescriptionTag, out res.SuffixInfoDescription)
								? res.SuffixInfoDescription.TQCleanup(true)
								: k.Item.suffixID.Raw;
				}
				else
					res.SuffixInfoDescription = k.Item.suffixID.Raw;
			}

			#endregion

			#region flavor text

			// Removed Scroll flavor text since it gets printed by the skill effect code
			if ((k.Item.IsPotion || k.Item.IsRelicOrCharm || k.Item.IsScroll || k.Item.IsParchment || k.Item.IsQuestItem) && !string.IsNullOrWhiteSpace(k.Item.baseItemInfo?.StyleTag))
			{
				if (TranslationService.TryTranslateXTag(k.Item.baseItemInfo.StyleTag, out var flavor))
				{
					var ft = StringHelper.WrapWords(flavor, 40);
					res.FlavorText = ft.ToArray();
				}
			}

			#endregion

			#endregion

			List<string> results = new List<string>();

			if (k.Scope?.HasFlag(FriendlyNamesExtraScopes.PrefixAttributes) ?? false)
			{
				if (k.Item.prefixInfo != null)
					res.PrefixInfoRecords = Database.GetRecordFromFile(k.Item.prefixID);

				if (res.PrefixInfoRecords?.Any() ?? false)
					GetAttributesFromRecord(k.Item, res.PrefixInfoRecords, k.FilterExtra, k.Item.prefixID, results);

				res.PrefixAttributes = results.ToArray();
			}

			if (k.Scope?.HasFlag(FriendlyNamesExtraScopes.SuffixAttributes) ?? false)
			{
				results.Clear();

				if (k.Item.suffixInfo != null)
					res.SuffixInfoRecords = Database.GetRecordFromFile(k.Item.suffixID);

				if (res.SuffixInfoRecords?.Any() ?? false)
					GetAttributesFromRecord(k.Item, res.SuffixInfoRecords, k.FilterExtra, k.Item.suffixID, results);

				res.SuffixAttributes = results.ToArray();
			}

			if (k.Scope?.HasFlag(FriendlyNamesExtraScopes.BaseAttributes) ?? false)
			{
				results.Clear();

				// res.baseItemInfoRecords should be already loaded
				if (res.BaseItemInfoRecords?.Any() ?? false)
					GetAttributesFromRecord(k.Item, res.BaseItemInfoRecords, k.FilterExtra, k.Item.BaseItemId, results);
			}

			res.BaseAttributes = results.ToArray();

			if (k.Scope?.HasFlag(FriendlyNamesExtraScopes.RelicAttributes) ?? false)
			{
				var tmp = new List<string>();

				if (k.Item.RelicInfo != null)
					res.RelicInfoRecords = Database.GetRecordFromFile(k.Item.relicID);

				if (res.RelicInfoRecords?.Any() ?? false)
					GetAttributesFromRecord(k.Item, res.RelicInfoRecords, k.FilterExtra, k.Item.relicID, tmp);

				res.Relic1Attributes = tmp.ToArray();
			}

			if (k.Scope?.HasFlag(FriendlyNamesExtraScopes.Relic2Attributes) ?? false)
			{
				var tmp = new List<string>();

				if (k.Item.Relic2Info != null)
					res.Relic2InfoRecords = Database.GetRecordFromFile(k.Item.relic2ID);

				if (res.Relic2InfoRecords?.Any() ?? false)
					GetAttributesFromRecord(k.Item, res.Relic2InfoRecords, k.FilterExtra, k.Item.relic2ID, tmp);

				res.Relic2Attributes = tmp.ToArray();
			}

			if (k.Scope?.HasFlag(FriendlyNamesExtraScopes.ItemSet) ?? false)
				res.ItemSet = GetItemSetTranslations(k.Item);

			if (k.Scope?.HasFlag(FriendlyNamesExtraScopes.Requirements) ?? false)
			{
				var reqs = GetRequirements(k.Item);
				res.Requirements = reqs.Requirements;
				res.RequirementVariables = reqs.RequirementVariables;
				res.RequirementInfo = GetRequirementInfo(k.Item, reqs.RequirementVariables);
			}

			#region Extra Attributes for specific types

			// Shows Artifact stats for the formula
			if (k.Item.IsFormulae && k.Item.baseItemInfo != null && (k.Scope?.HasFlag(FriendlyNamesExtraScopes.BaseAttributes) ?? false))
			{
				string artifactName = k.Item.baseItemInfo.GetString("artifactName");

				if (!string.IsNullOrWhiteSpace(artifactName))
				{
					List<string> tmp = new List<string>();

					res.FormulaeArtifactRecords = Database.GetRecordFromFile(artifactName);

					// Display the name of the Artifact
					if (!TranslationService.TryTranslateXTag(res.FormulaeArtifactRecords.GetString("description", 0), out res.FormulaeArtifactName))
						res.FormulaeArtifactName = "?Unknown Artifact Name?";

					// Class
					string artifactClassification = res.FormulaeArtifactRecords.GetString("artifactClassification", 0).ToUpperInvariant();
					res.FormulaeArtifactClass = TranslateArtifactClassification(artifactClassification);

					// Attributes
					GetAttributesFromRecord(k.Item, res.FormulaeArtifactRecords, true, artifactName, tmp);
					res.FormulaeArtifactAttributes = tmp.ToArray();
				}
			}

			// Show the completion bonus.
			if (k.Item.RelicBonusInfo != null
				&& (k.Scope?.HasFlag(FriendlyNamesExtraScopes.RelicAttributes) ?? false)
				&& (k.Item.IsArtifact // Artifact completion bonus
					|| k.Item.IsRelicOrCharm // Relic completion bonus
					|| k.Item.HasRelicOrCharmSlot1
				)
			)
			{
				var tmp = new List<string>();

				res.RelicBonus1InfoRecords = Database.GetRecordFromFile(k.Item.RelicBonusId);

				if (res.RelicBonus1InfoRecords?.Any() ?? false)
					GetAttributesFromRecord(k.Item, res.RelicBonus1InfoRecords, k.FilterExtra, k.Item.RelicBonusId, tmp);

				res.RelicBonus1Attributes = tmp.ToArray();
			}

			// Show the Relic2 completion bonus.
			if (k.Item.HasRelicOrCharmSlot2 && k.Item.RelicBonus2Info != null && (k.Scope?.HasFlag(FriendlyNamesExtraScopes.Relic2Attributes) ?? false))
			{
				var tmp = new List<string>();
				res.RelicBonus2InfoRecords = Database.GetRecordFromFile(k.Item.RelicBonus2Id);

				if (res.RelicBonus2InfoRecords?.Any() ?? false)
					GetAttributesFromRecord(k.Item, res.RelicBonus2InfoRecords, k.FilterExtra, k.Item.RelicBonus2Id, tmp);

				res.RelicBonus2Attributes = tmp.ToArray();
			}

			#endregion

			k.Item.CurrentFriendlyNameResult.TmpAttrib.Clear();
			k.Item.CurrentFriendlyNameResult = null;
			return res;

		});

		#region Local Helper

		string TranslateArtifactClassification(string artifactClassificationKey)
		{
			string tag = string.Empty;
			string resartifactClass = string.Empty;
			if (artifactClassificationKey == null)
				tag = null;
			else if (artifactClassificationKey == "LESSER")
				tag = "xtagArtifactClass01";
			else if (artifactClassificationKey == "GREATER")
				tag = "xtagArtifactClass02";
			else if (artifactClassificationKey == "DIVINE")
				tag = "xtagArtifactClass03";
			else
				tag = null;

			if (tag != null)
				resartifactClass = TranslationService.TranslateXTag(tag);
			else
				resartifactClass = "Unknown Artifact Class";

			return string.IsNullOrWhiteSpace(resartifactClass) ? "Unknown Artifact Class" : resartifactClass;
		}

		#endregion
	}
}

