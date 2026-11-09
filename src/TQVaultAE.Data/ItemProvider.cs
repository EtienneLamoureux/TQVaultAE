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
using TQVaultAE.Domain.Jitter;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Data;

/// <summary>
/// Class for holding item information
/// </summary>
public partial class ItemProvider : IItemProvider, IGameDataCache
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
	private readonly LazyConcurrentDictionary<RecordId, ItemAffixes?> ItemAffixesCache = new();
	private volatile bool _cachedShowInGameValues;

	/// <inheritdoc/>
	public void ClearGameDataCache()
	{
		this.FriendlyNamesCache.Clear();
		this.ItemAffixesCache.Clear();
	}


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

		this.SortArmorAttributeGroups = ((IComparer<List<Variable?>>)new ItemAttributeListCompare(true, itemAttributeProvider)).Compare;
		this.SortOtherAttributeGroups = ((IComparer<List<Variable?>>)new ItemAttributeListCompare(false, itemAttributeProvider)).Compare;
		this.SortDamageQualifiers = ((IComparer<Variable?>)new ItemAttributeSubListCompare(itemAttributeProvider)).Compare;
	}

	// Sort delegates made once: Array.Sort and List.Sort given an IComparer build a Comparison from it on every call,
	// and building that delegate was the most contended step of the parallel search warm-up.
	private readonly Comparison<List<Variable?>> SortArmorAttributeGroups;
	private readonly Comparison<List<Variable?>> SortOtherAttributeGroups;
	private readonly Comparison<Variable?> SortDamageQualifiers;


	/// <summary>
	/// Replays the game's stat jitter for equipment (armor, jewellery, weapons, shields).
	/// </summary>
	/// <returns><see langword="null"/> when the item is not equipment or has no seed.</returns>
	private ItemRollResult? RollInGameValues(Item itm, DBRecordCollection? baseRecord)
	{
		var itemClass = itm.baseItemInfo?.ItemClass;
		if (baseRecord is null || itm.Seed == 0 || itemClass is null || itm.IsRelicOrCharm
			|| !(itemClass.StartsWith("ArmorProtective", StringComparison.OrdinalIgnoreCase)
				|| itemClass.StartsWith("ArmorJewelry", StringComparison.OrdinalIgnoreCase)
				|| itemClass.StartsWith("Weapon", StringComparison.OrdinalIgnoreCase)))
			return null;

		try {
			var prefix = RecordId.IsNullOrEmpty(itm.prefixID) ? null : this.Database.GetRecordFromFile(itm.prefixID);
			var suffix = RecordId.IsNullOrEmpty(itm.suffixID) ? null : this.Database.GetRecordFromFile(itm.suffixID);
			return ItemRollSimulator.Roll((uint)itm.Seed, itm.IsArmor, baseRecord, prefix, suffix);
		} catch (Exception ex) {
			this.Log.LogError(ex, "Failed to compute in-game values for {Item}", itm.BaseItemId);
			return null;
		}
	}

	private static string FormatInGameRoll(RolledValue roll)
		=> string.Format(CultureInfo.CurrentCulture, "{0} {1}: {2:0.##} -> {3:0.##}", roll.Source, roll.Variable, roll.Original, roll.Rolled);

	public bool InvalidateFriendlyNamesCache(params IEnumerable<Item> items)
	{
		items = items.Where(i => i is not null).ToList();
		var keylist = this.FriendlyNamesCache.Where(i => items.Contains(i.Key.Item)).Select(i => i.Key).ToList();
		foreach (var k in keylist) this.FriendlyNamesCache.TryRemove(k, out var outVal);
		return keylist.Count != 0;
	}

	/// <summary>
	/// Formats a string based on the formatspec
	/// </summary>
	/// <param name="formatSpec">format specification</param>
	/// <param name="parameter1">first parameter</param>
	/// <param name="parameter2">second parameter</param>
	/// <param name="parameter3">third parameter</param>
	/// <returns>formatted string.</returns>
	public string Format(string? formatSpec, object? parameter1, object? parameter2 = null, object? parameter3 = null)
	{
		try
		{
			// A null format throws ArgumentNullException, reported below like a bad format.
			return string.Format(CultureInfo.CurrentCulture, formatSpec!, parameter1, parameter2, parameter3);
		}
		// A malformed format (often a translation) shows this one line raw instead of failing the whole item text.
		catch (Exception ex) when (ex is ArgumentException or FormatException)
		{
			string parameters = string.Format(
				CultureInfo.InvariantCulture,
				"\", '{0}', '{1}', '{2}'>",
				parameter1 is null ? "NULL" : parameter1,
				parameter2 is null ? "NULL" : parameter2,
				parameter3 is null ? "NULL" : parameter3);

			string error = string.Concat("FormatErr(\"", formatSpec, parameters);

			Log.LogWarning(ex, "Failed to format an item text line: {Message}", error);

			return error;
		}
	}

	/// <summary>
	/// <see cref="ITranslationService.TryTranslateXTag"/> for a <see cref="ToFriendlyNameResult"/> property, which
	/// cannot be an <c>out</c> argument: <paramref name="assign"/> always receives the translation, as the out value did.
	/// </summary>
	private bool TryTranslateXTag(string tag, Action<string> assign)
	{
		bool found = this.TranslationService.TryTranslateXTag(tag, out var value);
		assign(value);
		return found;
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
		// Cached results depend on the in-game values setting: drop them when it changes.
		if (this.USettings.ShowInGameValues != this._cachedShowInGameValues) {
			this.FriendlyNamesCache.Clear();
			this._cachedShowInGameValues = this.USettings.ShowInGameValues;
		}

		var key = (itm, scopes, filterExtra);
		return FriendlyNamesCache.GetOrAddAtomic(key, k =>
		{

			var res = new ToFriendlyNameResult(k.Item)
			{
				GrantedSkillsApart = k.Scope?.HasFlag(FriendlyNamesExtraScopes.GrantedSkills) ?? false,
			};
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
				if (k.Item.prefixInfo is not null)
				{
					if (TranslationService.TryTranslateXTag(k.Item.prefixInfo.DescriptionTag, out var desc))
						res.PrefixInfoDescription = desc.TQCleanup(true);
				}
			}

			#endregion

			#region Base Item translation

			// Load common relic translations if item is relic related by any means
			if (k.Item.IsRelicOrCharm || k.Item.HasRelicOrCharmSlot1 || k.Item.HasRelicOrCharmSlot2 || k.Item.RelicInfo is not null || k.Item.Relic2Info is not null)
			{
				res.ItemWith = this.TranslationService.ItemWith;

				if (k.Item.RelicInfo is not null)
					this.TryTranslateXTag(k.Item.RelicInfo.DescriptionTag, v => res.RelicInfo1Description = v);

				if (k.Item.Relic2Info is not null)
					this.TryTranslateXTag(k.Item.Relic2Info.DescriptionTag, v => res.RelicInfo2Description = v);

				var labelCompleted = "Completed";
				if (!this.TryTranslateXTag("tagAnimalPartComplete", v => res.AnimalPartComplete = v))
					res.AnimalPartComplete = labelCompleted;

				if (!this.TryTranslateXTag("tagRelicComplete", v => res.RelicComplete = v))
					res.RelicComplete = labelCompleted;

				var labelPartcomplete = "Completion Bonus: ";
				if (!this.TryTranslateXTag("tagAnimalPartcompleteBonus", v => res.AnimalPartCompleteBonus = v))
					res.AnimalPartCompleteBonus = labelPartcomplete;

				if (!this.TryTranslateXTag("tagRelicBonus", v => res.RelicBonus = v))
					res.RelicBonus = labelPartcomplete;

				var labelRelic = "Relic";
				if (!this.TryTranslateXTag("tagAnimalPart", v => res.AnimalPart = v))
					res.AnimalPart = labelRelic;

				if (!this.TryTranslateXTag("tagRelicShard", v => res.RelicShard = v))
					res.RelicShard = labelRelic;

				var labelRelicPattern = "{0} - {1} / {2}";
				if (this.TryTranslateXTag("tagAnimalPartRatio", v => res.AnimalPartRatio = v))
					res.AnimalPartRatio = ItemAttributeProvider.ConvertFormat(res.AnimalPartRatio!);
				else
					res.AnimalPartRatio = labelRelicPattern;

				if (this.TryTranslateXTag("tagRelicRatio", v => res.RelicRatio = v))
					res.RelicRatio = ItemAttributeProvider.ConvertFormat(res.RelicRatio!);
				else
					res.RelicRatio = labelRelicPattern;
			}

			res.BaseItemId = k.Item.BaseItemId;

			// Set Rarity translation
			res.BaseItemRarity = TranslationService.Translate(k.Item.ItemStyle);

			if (k.Item.baseItemInfo is not null)
			{
				// style quality description
				if (!string.IsNullOrEmpty(k.Item.baseItemInfo.StyleTag))
				{
					if (!k.Item.IsPotion && !k.Item.IsRelicOrCharm && !k.Item.IsScroll && !k.Item.IsParchment && !k.Item.IsQuestItem)
					{
						if (!this.TryTranslateXTag(k.Item.baseItemInfo.StyleTag, v => res.BaseItemInfoStyle = v))
							res.BaseItemInfoStyle = k.Item.baseItemInfo.StyleTag;
					}
				}

				if (!string.IsNullOrEmpty(k.Item.baseItemInfo.QualityTag))
				{
					if (!this.TryTranslateXTag(k.Item.baseItemInfo.QualityTag, v => res.BaseItemInfoQuality = v))
						res.BaseItemInfoQuality = k.Item.baseItemInfo.QualityTag;
				}

				if (!this.TryTranslateXTag(k.Item.baseItemInfo.DescriptionTag, v => res.BaseItemInfoDescription = v))
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

						res.RelicCompletionFormat = Format(res.RelicPattern, res.RelicClass, k.Item.Number, k.Item.baseItemInfo?.CompletedRelicLevel);
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
					if (!this.TryTranslateXTag("xtagArtifactRecipe", v => res.ArtifactRecipe = v))
						res.ArtifactRecipe = "Recipe";

					// Get Reagents format
					if (this.TryTranslateXTag("xtagArtifactReagents", v => res.ArtifactReagents = v))
						res.ArtifactReagents = ItemAttributeProvider.ConvertFormat(res.ArtifactReagents!);
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
				if (k.Item.suffixInfo is not null)
				{
					res.SuffixInfoDescription =
							this.TryTranslateXTag(k.Item.suffixInfo.DescriptionTag, v => res.SuffixInfoDescription = v)
								? res.SuffixInfoDescription!.TQCleanup(true)
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

			List<string> results = [];
			List<string?> ranges = [];

			// The lines of each record with where the game lists them, merged in the game's order for the classic display.
			List<GameAttributeSlot> prefixSlots = [];
			List<GameAttributeSlot> suffixSlots = [];
			List<GameAttributeSlot> baseSlots = [];

			// In-game values (issue #552): jittered copies used for display only.
			// The legacy display multiplier (attributeScalePercent) may remain from a render without in-game values.
			if (this.USettings.ShowInGameValues)
				k.Item.itemScalePercent = 1.00F;

			var inGame = this.USettings.ShowInGameValues ? this.RollInGameValues(k.Item, res.BaseItemInfoRecords) : null;
			if (inGame is not null)
				res.InGameRolls = inGame.Rolls.Select(FormatInGameRoll).ToArray();

			if (k.Scope?.HasFlag(FriendlyNamesExtraScopes.PrefixAttributes) ?? false)
			{
				if (k.Item.prefixInfo is not null)
					res.PrefixInfoRecords = Database.GetRecordFromFile(k.Item.prefixID);

				if (res.PrefixInfoRecords is { Count: > 0 })
					GetAttributesFromRecord(k.Item, inGame?.Prefix ?? res.PrefixInfoRecords, k.FilterExtra, k.Item.prefixID, results
						, valueRanges: inGame?.RangesOf(RollSource.Prefix), lineRanges: ranges, lineSlots: prefixSlots);

				res.PrefixAttributes = results.ToArray();
				res.PrefixAttributeRanges = inGame is null ? [] : ranges.ToArray();
			}

			if (k.Scope?.HasFlag(FriendlyNamesExtraScopes.SuffixAttributes) ?? false)
			{
				results.Clear();
				ranges.Clear();

				if (k.Item.suffixInfo is not null)
					res.SuffixInfoRecords = Database.GetRecordFromFile(k.Item.suffixID);

				if (res.SuffixInfoRecords is { Count: > 0 })
					GetAttributesFromRecord(k.Item, inGame?.Suffix ?? res.SuffixInfoRecords, k.FilterExtra, k.Item.suffixID, results
						, valueRanges: inGame?.RangesOf(RollSource.Suffix), lineRanges: ranges, lineSlots: suffixSlots);

				res.SuffixAttributes = results.ToArray();
				res.SuffixAttributeRanges = inGame is null ? [] : ranges.ToArray();
			}

			if (k.Scope?.HasFlag(FriendlyNamesExtraScopes.BaseAttributes) ?? false)
			{
				results.Clear();
				ranges.Clear();

				// res.baseItemInfoRecords should be already loaded
				if (res.BaseItemInfoRecords is { Count: > 0 })
					GetAttributesFromRecord(k.Item, inGame?.Base ?? res.BaseItemInfoRecords, k.FilterExtra, k.Item.BaseItemId, results
						, valueRanges: inGame?.RangesOf(RollSource.Base), lineRanges: ranges, lineSlots: baseSlots);
			}

			res.BaseAttributes = results.ToArray();
			res.BaseAttributeRanges = inGame is null ? [] : ranges.ToArray();

			if (prefixSlots.Count == res.PrefixAttributes.Length && suffixSlots.Count == res.SuffixAttributes.Length
				&& baseSlots.Count == res.BaseAttributes.Length)
			{
				res.ClassicAttributes = GameAttributeOrder.Merge(GameAttributeOrder.ClassOf(k.Item)
					, [.. res.BaseAttributes.Zip(baseSlots)]
					, [.. res.PrefixAttributes.Zip(prefixSlots)]
					, [.. res.SuffixAttributes.Zip(suffixSlots)]);
			}

			if (k.Scope?.HasFlag(FriendlyNamesExtraScopes.RelicAttributes) ?? false)
			{
				var tmp = new List<string>();

				if (k.Item.RelicInfo is not null)
					res.RelicInfoRecords = Database.GetRecordFromFile(k.Item.relicID);

				if (res.RelicInfoRecords is { Count: > 0 })
					GetAttributesFromRecord(k.Item, res.RelicInfoRecords, k.FilterExtra, k.Item.relicID, tmp);

				res.Relic1Attributes = tmp.ToArray();
			}

			if (k.Scope?.HasFlag(FriendlyNamesExtraScopes.Relic2Attributes) ?? false)
			{
				var tmp = new List<string>();

				if (k.Item.Relic2Info is not null)
					res.Relic2InfoRecords = Database.GetRecordFromFile(k.Item.relic2ID);

				if (res.Relic2InfoRecords is { Count: > 0 })
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
			if (k.Item.IsFormulae && k.Item.baseItemInfo is not null && (k.Scope?.HasFlag(FriendlyNamesExtraScopes.BaseAttributes) ?? false))
			{
				string artifactName = k.Item.baseItemInfo.GetString("artifactName");

				if (!string.IsNullOrWhiteSpace(artifactName))
				{
					List<string> tmp = [];

					res.FormulaeArtifactRecords = Database.GetRecordFromFile(artifactName);

					// Display the name of the Artifact
					if (!this.TryTranslateXTag(res.FormulaeArtifactRecords?.GetString("description", 0) ?? string.Empty, v => res.FormulaeArtifactName = v))
						res.FormulaeArtifactName = "?Unknown Artifact Name?";

					// Class
					string artifactClassification = (res.FormulaeArtifactRecords?.GetString("artifactClassification", 0) ?? string.Empty).ToUpperInvariant();
					res.FormulaeArtifactClass = TranslateArtifactClassification(artifactClassification);

					// Attributes
					GetAttributesFromRecord(k.Item, res.FormulaeArtifactRecords, true, artifactName, tmp);
					res.FormulaeArtifactAttributes = tmp.ToArray();
				}
			}

			// Show the completion bonus.
			if (k.Item.RelicBonusInfo is not null
				&& (k.Scope?.HasFlag(FriendlyNamesExtraScopes.RelicAttributes) ?? false)
				&& (k.Item.IsArtifact // Artifact completion bonus
					|| k.Item.IsRelicOrCharm // Relic completion bonus
					|| k.Item.HasRelicOrCharmSlot1
				)
			)
			{
				var tmp = new List<string>();

				res.RelicBonus1InfoRecords = Database.GetRecordFromFile(k.Item.RelicBonusId);

				if (res.RelicBonus1InfoRecords is { Count: > 0 })
					GetAttributesFromRecord(k.Item, res.RelicBonus1InfoRecords, k.FilterExtra, k.Item.RelicBonusId, tmp);

				res.RelicBonus1Attributes = tmp.ToArray();
			}

			// Show the Relic2 completion bonus.
			if (k.Item.HasRelicOrCharmSlot2 && k.Item.RelicBonus2Info is not null && (k.Scope?.HasFlag(FriendlyNamesExtraScopes.Relic2Attributes) ?? false))
			{
				var tmp = new List<string>();
				res.RelicBonus2InfoRecords = Database.GetRecordFromFile(k.Item.RelicBonus2Id);

				if (res.RelicBonus2InfoRecords is { Count: > 0 })
					GetAttributesFromRecord(k.Item, res.RelicBonus2InfoRecords, k.FilterExtra, k.Item.RelicBonus2Id, tmp);

				res.RelicBonus2Attributes = tmp.ToArray();
			}

			#endregion

			if (k.Scope?.HasFlag(FriendlyNamesExtraScopes.GrantedSkills) ?? false)
				res.GrantedSkills = this.GetGrantedSkills(k.Item);

			k.Item.CurrentFriendlyNameResult.TmpAttrib.Clear();
			k.Item.CurrentFriendlyNameResult = null;
			return res;

		});

		#region Local Helper

		string TranslateArtifactClassification(string artifactClassificationKey)
		{
			string? tag = string.Empty;
			string resartifactClass = string.Empty;
			if (artifactClassificationKey is null)
				tag = null;
			else if (artifactClassificationKey == "LESSER")
				tag = "xtagArtifactClass01";
			else if (artifactClassificationKey == "GREATER")
				tag = "xtagArtifactClass02";
			else if (artifactClassificationKey == "DIVINE")
				tag = "xtagArtifactClass03";
			else
				tag = null;

			if (tag is not null)
				resartifactClass = TranslationService.TranslateXTag(tag);
			else
				resartifactClass = "Unknown Artifact Class";

			return string.IsNullOrWhiteSpace(resartifactClass) ? "Unknown Artifact Class" : resartifactClass;
		}

		#endregion
	}
}

