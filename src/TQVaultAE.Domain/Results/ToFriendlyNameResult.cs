﻿using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using TQVaultAE.Domain.Comparison;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Domain.Results;

public partial class ToFriendlyNameResult
{

	IEnumerable<string>? _FullText;
	[MemberNotNull(nameof(_FullText))]
	private void FulltextBuild()
	{
		if (_FullText is null)
		{
			// No mutation, just a comprehensive list of string ref 
			string?[][] parts = [
					[
						FullName
						, ItemThrown
						, ArtifactClass
						, RelicCompletionFormat
						, RelicBonusFormat
						, ItemSeed
						, ItemOrigin 
						// Socketed Relic name
						, Item.HasRelicOrCharmSlot1 ? RelicInfo1Description : null
						, Item.HasRelicOrCharmSlot2 ? RelicInfo2Description : null
						// Socketed Relic Completion label
						, Item.HasRelicOrCharmSlot1 ? RelicInfo1CompletionResolved : null
						, Item.HasRelicOrCharmSlot2 ? RelicInfo2CompletionResolved : null
						// Socketed Relic Completion Bonus label
						, Item.HasRelicOrCharmSlot1 ? RelicInfo1CompletionBonusResolved : null
						, Item.HasRelicOrCharmSlot2 ? RelicInfo2CompletionBonusResolved : null
					]
					, AttributesAll
					, FlavorText
					, ItemSet?.Translations.Select(si => si.Value).ToArray() ?? []
					, Requirements
				];
			_FullText = parts.SelectMany(s => s).OfType<string>().Where(s => s.Length > 0);
		}
	}


	public bool FulltextIsMatch(string? search)
	{
		if (string.IsNullOrWhiteSpace(search)) return false;

		var isrex = StringHelper.IsTQVaultSearchRegEx(search);

		if (isrex.IsRegex)
			return FulltextIsMatchRegex(isrex.Pattern);

		return FulltextIsMatchIndexOf(search);
	}

	public bool FulltextIsMatchIndexOf(string? search)
	{
		if (string.IsNullOrWhiteSpace(search)) return false;

		FulltextBuild();

		// Use span-based comparison for better performance
		var searchSpan = search.AsSpan();
		foreach (var str in _FullText)
		{
			if (str.AsSpan().Contains(searchSpan, StringComparison.OrdinalIgnoreCase)) return true;
		}

		return false;
	}

	/// <summary>
	/// Optimized full-text search using ReadOnlySpan for bounds-check elimination.
	/// </summary>
	public bool FulltextIsMatchIndexOf(ReadOnlySpan<char> search)
	{
		if (search.IsEmpty) return false;

		FulltextBuild();

		foreach (var str in _FullText)
		{
			if (str.AsSpan().Contains(search, StringComparison.OrdinalIgnoreCase)) return true;
		}

		return false;
	}

	public bool FulltextIsMatchRegex(string? pattern)
	{
		if (string.IsNullOrWhiteSpace(pattern)) return false;

		FulltextBuild();

		try
		{
			var rex = new Regex(pattern, RegexOptions.IgnoreCase);
			return FulltextIsMatchRegex(rex);
		}
		catch (ArgumentException)
		{ }

		return false;
	}

	public bool FulltextIsMatchRegex(Regex pattern)
	{
		if (pattern is null) return false;

		FulltextBuild();

		foreach (var str in _FullText)
		{
			if (pattern.IsMatch(str))
				return true;
		}

		return false;
	}

	public string FullNameClean => FullName.RemoveAllTQTags();
	public string FullName
	{
		get
		{
			string?[] parts = [
				PrefixInfoDescription
				, BaseItemInfoQuality
				, BaseItemInfoDescription
				, Item.IsThrownWeapon ? null : BaseItemInfoStyle
				, SuffixInfoDescription
				, Item.DoesStack ? NumberFormat : null
			];
			return parts.RemoveEmptyAndSanitize().JoinWithoutStartingSpaces(" ");
		}
	}

	public string FullNameBagTooltipClean => FullNameBagTooltip.RemoveAllTQTags();
	public string FullNameBagTooltip
	{
		get
		{
			string?[] parts = [
				PrefixInfoDescription
				, BaseItemInfoQuality
				, Item.IsRelicOrCharm && !Item.IsCharmOnly ? TQColor.Silver.ColorTag() : null // Make a color diff for Relic & Charm
				, BaseItemInfoDescription
				, Item.IsThrownWeapon ? null : BaseItemInfoStyle
				, SuffixInfoDescription
				, Item.DoesStack ? NumberFormat : null
				, Item.IsRelicOrCharm ? "- " + RelicBonusFormat : null
				, Item.IsQuestItem ? this.ItemQuest : null
				, Item.GameDlcSuffix
			];
			return parts.RemoveEmptyAndSanitize().JoinWithoutStartingSpaces(" ");
		}
	}

	public readonly Item Item;
	public string? PrefixInfoDescription { get; internal set; }
	public string[] PrefixAttributes { get; internal set; } = [];
	public DBRecordCollection? PrefixInfoRecords { get; internal set; }
	public string? SuffixInfoDescription { get; internal set; }
	public string[] SuffixAttributes { get; internal set; } = [];
	public DBRecordCollection? SuffixInfoRecords { get; internal set; }

	public RecordId BaseItemId { get; internal set; } = RecordId.Empty;
	public string? BaseItemRarity { get; internal set; }

	[GeneratedRegex(@"[^\w\s']")]
	private static partial Regex BaseItemInfoClassRegEx();
	string _BaseItemInfoClass = string.Empty;
	public string BaseItemInfoClass
	{
		get => _BaseItemInfoClass;
		internal set => _BaseItemInfoClass = BaseItemInfoClassRegEx().Replace((value ?? string.Empty), string.Empty);// Clean everything except few things;
	}

	public string? BaseItemInfoStyle { get; internal set; }
	public string? BaseItemInfoQuality { get; internal set; }
	public string? BaseItemInfoDescription { get; internal set; }
	public string? ItemSeed { get; internal set; }
	public string? ItemQuest { get; internal set; }
	public string? ItemOrigin { get; internal set; }
	public string? ItemThrown { get; internal set; }
	public string? NumberFormat { get; internal set; }
	public string? ItemWith { get; internal set; }
	public string[] FlavorText { get; internal set; } = [];
	public string[] Requirements { get; internal set; } = [];
	public SortedList<string, Variable> RequirementVariables { get; internal set; } = [];
	public RequirementInfo? RequirementInfo { get; internal set; }
	public string[] BaseAttributes { get; internal set; } = [];

	/// <summary>
	/// The lines of <see cref="BaseAttributes"/>, <see cref="PrefixAttributes"/> and <see cref="SuffixAttributes"/> merged in
	/// the order the game lists them, for the classic display; null when they could not be merged.
	/// </summary>
	public string[]? ClassicAttributes { get; internal set; }

	/// <summary>
	/// Possible in-game values of each <see cref="ClassicAttributes"/> line (see <see cref="PrefixAttributeRanges"/>).
	/// Same length as <see cref="ClassicAttributes"/>, or empty without in-game values.
	/// </summary>
	public string?[] ClassicAttributeRanges { get; internal set; } = [];

	/// <summary>
	/// The record each <see cref="ClassicAttributes"/> line comes from (<see cref="StatSource.Base"/>,
	/// <see cref="StatSource.Prefix"/> or <see cref="StatSource.Suffix"/>), for the item comparison's sources. Same length as
	/// <see cref="ClassicAttributes"/>, or empty when they could not be merged.
	/// </summary>
	public StatSource[] ClassicAttributeSources { get; internal set; } = [];

	/// <summary>
	/// One line per value changed by the in-game jitter ("database → in-game"), when <c>ShowInGameValues</c> is on.
	/// </summary>
	public string[] InGameRolls { get; internal set; } = [];

	/// <summary>
	/// The skills granted by the item, its affixes and its relics (<see cref="FriendlyNamesExtraScopes.GrantedSkills"/>).
	/// </summary>
	public GrantedSkill[] GrantedSkills { get; internal set; } = [];

	/// <summary>The granted skills are shown apart (<see cref="GrantedSkills"/>): the attributes do not repeat their details.</summary>
	internal bool GrantedSkillsApart { get; set; }

	/// <summary>
	/// Possible in-game values of each <see cref="PrefixAttributes"/> line, e.g. "(90 - 126)", or <see langword="null"/>
	/// when the line is not randomized. Same length as <see cref="PrefixAttributes"/>, or empty without in-game values.
	/// </summary>
	public string?[] PrefixAttributeRanges { get; internal set; } = [];

	/// <summary>Possible in-game values of each <see cref="SuffixAttributes"/> line (see <see cref="PrefixAttributeRanges"/>).</summary>
	public string?[] SuffixAttributeRanges { get; internal set; } = [];

	/// <summary>Possible in-game values of each <see cref="BaseAttributes"/> line (see <see cref="PrefixAttributeRanges"/>).</summary>
	public string?[] BaseAttributeRanges { get; internal set; } = [];
	public SetItemInfo? ItemSet { get; internal set; }
	public DBRecordCollection? BaseItemInfoRecords { get; internal set; }

	#region Relic Common

	public string? AnimalPartComplete { get; internal set; }
	public string? AnimalPartCompleteBonus { get; internal set; }
	public string? AnimalPart { get; internal set; }
	public string? AnimalPartRatio { get; internal set; }
	public string? RelicComplete { get; internal set; }
	public string? RelicBonus { get; internal set; }
	public string? RelicShard { get; internal set; }
	public string? RelicRatio { get; internal set; }
	public string? RelicClass { get; internal set; }
	public string? RelicCompletionFormat { get; internal set; }
	public string? RelicPattern { get; internal set; }
	public string? RelicBonusPattern { get; internal set; }
	public string? RelicBonusFormat { get; internal set; }
	public string? RelicBonusFileName { get; internal set; }
	public string? RelicBonusTitle { get; internal set; }
	public DBRecordCollection? RelicInfoRecords { get; internal set; }

	#endregion

	#region RelicInfo 1 & 2

	public string? ItemRelicBonus1Format { get; internal set; }
	public string? ItemRelicBonus2Format { get; internal set; }
	public string? RelicInfo1Description { get; internal set; }
	public string? RelicInfo2Description { get; internal set; }
	public string[] Relic1Attributes { get; internal set; } = [];
	public string[] Relic2Attributes { get; internal set; } = [];
	public string[] RelicBonus1Attributes { get; internal set; } = [];
	public string[] RelicBonus2Attributes { get; internal set; } = [];
	public DBRecordCollection? RelicBonus1InfoRecords { get; internal set; }
	public DBRecordCollection? RelicBonus2InfoRecords { get; internal set; }
	public DBRecordCollection? Relic2InfoRecords { get; internal set; }
	/// <summary>
	/// Resolve first relic completion label
	/// </summary>
	public string? RelicInfo1CompletionResolved
	{
		get
		{
			if (this.Item.RelicInfo is null) return null;
			else
			{
				if (this.Item.IsRelic1Charm)
				{
					if (this.Item.IsRelicBonus1Complete)
						return this.AnimalPartComplete;
					else
						return string.Format(this.AnimalPartRatio ?? string.Empty
							, this.AnimalPart
							, this.Item.Var1
							, this.Item.RelicInfo.CompletedRelicLevel
						);
				}
				else
				{
					if (this.Item.IsRelicBonus1Complete)
						return this.RelicComplete;
					else
						return string.Format(this.RelicRatio ?? string.Empty
							, this.RelicShard
							, this.Item.Var1
							, this.Item.RelicInfo.CompletedRelicLevel
						);
				}
			}
		}
	}
	/// <summary>
	/// Resolve first relic completion bonus label
	/// </summary>
	public string? RelicInfo1CompletionBonusResolved
	{
		get
		{
			if (this.Item.IsRelic1Charm)
				return this.Item.IsRelicBonus1Complete ? this.AnimalPartCompleteBonus : null;
			else
				return this.Item.IsRelicBonus1Complete ? this.RelicBonus : null;
		}
	}
	/// <summary>
	/// Resolve second relic completion label
	/// </summary>
	public string? RelicInfo2CompletionResolved
	{
		get
		{
			if (this.Item.Relic2Info is null) return null;
			else
			{
				if (this.Item.IsRelic2Charm)
				{
					if (this.Item.IsRelicBonus2Complete)
						return this.AnimalPartComplete;
					else
						return string.Format(this.AnimalPartRatio ?? string.Empty
							, this.AnimalPart
							, this.Item.Var2
							, this.Item.Relic2Info.CompletedRelicLevel
						);
				}
				else
				{
					if (this.Item.IsRelicBonus2Complete)
						return this.RelicComplete;
					else
						return string.Format(this.RelicRatio ?? string.Empty
							, this.RelicShard
							, this.Item.Var2
							, this.Item.Relic2Info.CompletedRelicLevel
						);
				}
			}
		}
	}

	/// <summary>
	/// Resolve second relic completion bonus label
	/// </summary>
	public string? RelicInfo2CompletionBonusResolved
	{
		get
		{
			if (this.Item.IsRelic2Charm)
				return this.Item.IsRelicBonus2Complete ? this.AnimalPartCompleteBonus : null;
			else
				return this.Item.IsRelicBonus2Complete ? this.RelicBonus : null;
		}
	}

	#endregion

	public string? ArtifactClass { get; internal set; }
	public string? ArtifactRecipe { get; internal set; }
	public string? ArtifactReagents { get; internal set; }
	public string? ArtifactBonus { get; internal set; }
	public string? ArtifactBonusFormat { get; internal set; }

	public string? FormulaeArtifactClass { get; internal set; }
	public string? FormulaeArtifactName { get; internal set; }
	public string? FormulaeFormat { get; internal set; }
	public string[] FormulaeArtifactAttributes { get; internal set; } = [];
	public DBRecordCollection? FormulaeArtifactRecords { get; internal set; }

	public ToFriendlyNameResult(Item itm)
	{
		this.Item = itm;
	}

	string[]? _AttributesAll;

	/// <summary>
	/// Return the collection of all attributes without any color tags.
	/// <see cref="BaseAttributes"/>
	/// <see cref="FormulaeArtifactAttributes"/>
	/// <see cref="PrefixAttributes"/>
	/// <see cref="SuffixAttributes"/>
	/// <see cref="Relic1Attributes"/>
	/// <see cref="Relic2Attributes"/>
	/// <see cref="RelicBonus1Attributes"/>
	/// <see cref="RelicBonus2Attributes"/>
	/// </summary>
	public string[] AttributesAll
	{
		get
		{
			if (_AttributesAll is null)
			{
				string[][] groups = [
					BaseAttributes
					, PrefixAttributes
					, SuffixAttributes
					, Relic1Attributes
					, Relic2Attributes
					, RelicBonus1Attributes
					, RelicBonus2Attributes
					, FormulaeArtifactAttributes
				];
				_AttributesAll = groups.Where(c => c is { Length: > 0 })
				.SelectMany(a => a)
				.Where(a => !(string.IsNullOrWhiteSpace(a) || a.IsColorTagOnly()))
				.Select(a => a.RemoveAllTQTags())
				.ToArray();
			}
			return _AttributesAll;
		}
	}


	/// <summary>
	/// Used to give attribute list factory some kind of global awareness during its process
	/// </summary>
	internal List<string> TmpAttrib { get; } = [];

}
