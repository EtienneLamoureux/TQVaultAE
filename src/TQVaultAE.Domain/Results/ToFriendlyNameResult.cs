using System.Collections.Generic;
using System.Linq;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Domain.Results
{
	public class ToFriendlyNameResult
	{
		public string FullNameClean => FullName.RemoveAllTQTags();
		public string FullName => new string[] {
				PrefixInfoDescription
				, BaseItemInfoQuality
				, BaseItemInfoDescription
				, BaseItemInfoStyle
				, SuffixInfoDescription
				, Item.DoesStack ? NumberFormat : null
			}.RemoveEmptyAndSanitize()
		.JoinWithoutStartingSpaces(" ");

		public string FullNameBagTooltipClean => FullNameBagTooltip.RemoveAllTQTags();
		public string FullNameBagTooltip => new string[] {
					PrefixInfoDescription
					, BaseItemInfoQuality
					, Item.IsRelic && !Item.IsCharm ? TQColor.Silver.ColorTag() : null // Make a color diff for Relic & Charm
					, BaseItemInfoDescription
					, BaseItemInfoStyle
					, SuffixInfoDescription
					, Item.DoesStack ? NumberFormat : null
					, Item.IsRelic ? "- " + RelicBonusFormat : null
					, Item.IsQuestItem ? this.ItemQuest : null
					, Item.GameExtensionSuffix
				}.RemoveEmptyAndSanitize()
			.JoinWithoutStartingSpaces(" ");

		public readonly Item Item;

		public string PrefixInfoDescription;
		public string[] PrefixAttributes;
		public DBRecordCollection PrefixInfoRecords;
		public string SuffixInfoDescription;
		public string[] SuffixAttributes;
		public DBRecordCollection SuffixInfoRecords;

		public string BaseItemId;
		public string BaseItemInfoStyle;
		public string BaseItemInfoQuality;
		public string BaseItemInfoDescription;
		public string ItemSeed;
		public string ItemQuest;
		public string NumberFormat;
		public string ItemWith;
		public string[] FlavorText = new string[] { };
		public string[] Requirements = new string[] { };
		public SortedList<string, Variable> RequirementVariables;
		public string[] BaseAttributes = new string[] { };
		public string[] ItemSet = new string[] { };
		public DBRecordCollection BaseItemInfoRecords;

		#region Relic Common

		public string AnimalPartComplete;
		public string AnimalPartCompleteBonus;
		public string AnimalPart;
		public string AnimalPartRatio;
		public string RelicComplete;
		public string RelicBonus;
		public string RelicShard;
		public string RelicRatio;
		public string RelicClass;
		public string RelicCompletionFormat;
		public string RelicPattern;
		public string RelicBonusPattern;
		public string RelicBonusFormat;
		public string RelicBonusFileName;
		public string RelicBonusTitle;
		public DBRecordCollection RelicInfoRecords;

		#endregion

		#region RelicInfo 1 & 2

		public string ItemRelicBonus1Format;
		public string ItemRelicBonus2Format;
		public string RelicInfo1Description;
		public string RelicInfo2Description;
		public string[] Relic1Attributes = new string[] { };
		public string[] Relic2Attributes = new string[] { };
		public string[] RelicBonus1Attributes = new string[] { };
		public string[] RelicBonus2Attributes = new string[] { };
		public DBRecordCollection RelicBonus1InfoRecords;
		public DBRecordCollection RelicBonus2InfoRecords;
		public DBRecordCollection Relic2InfoRecords;
		/// <summary>
		/// Resolve first relic completion label
		/// </summary>
		public string RelicInfo1CompletionResolved
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
							return string.Format(this.AnimalPartRatio
								, this.AnimalPart
								, this.Item.Var1
								, this.Item.RelicBonusInfo?.CompletedRelicLevel.ToString() ?? "??"
							);
					}
					else
					{
						if (this.Item.IsRelicBonus1Complete)
							return this.RelicComplete;
						else
							return string.Format(this.RelicRatio
								, this.RelicShard
								, this.Item.Var1
								, this.Item.RelicBonusInfo?.CompletedRelicLevel.ToString() ?? "??"
							);
					}
				}
			}
		}
		/// <summary>
		/// Resolve first relic completion bonus label
		/// </summary>
		public string RelicInfo1CompletionBonusResolved
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
		public string RelicInfo2CompletionResolved
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
							return string.Format(this.AnimalPartRatio
								, this.AnimalPart
								, this.Item.Var2
								, this.Item.RelicBonus2Info?.CompletedRelicLevel.ToString() ?? "??"
							);
					}
					else
					{
						if (this.Item.IsRelicBonus2Complete)
							return this.RelicComplete;
						else
							return string.Format(this.RelicRatio
								, this.RelicShard
								, this.Item.Var2
								, this.Item.RelicBonus2Info?.CompletedRelicLevel.ToString() ?? "??"
							);
					}
				}
			}
		}

		/// <summary>
		/// Resolve second relic completion bonus label
		/// </summary>
		public string RelicInfo2CompletionBonusResolved
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

		public string ArtifactClass;
		public string ArtifactRecipe;
		public string ArtifactReagents;
		public string ArtifactBonus;
		public string ArtifactBonusFormat;

		public string FormulaeArtifactClass;
		public string FormulaeArtifactName;
		public string FormulaeFormat;
		public string[] FormulaeArtifactAttributes = new string[] { };
		public DBRecordCollection FormulaeArtifactRecords;

		public ToFriendlyNameResult(Item itm)
		{
			this.Item = itm;
		}

		string[] _AttributesAll = null;

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
					_AttributesAll = new string[][] {
						BaseAttributes
						, PrefixAttributes
						, SuffixAttributes
						, Relic1Attributes
						, Relic2Attributes
						, RelicBonus1Attributes
						, RelicBonus2Attributes
						, FormulaeArtifactAttributes
					}.Where(c => c?.Any() ?? false)
					.SelectMany(a => a)
					.Where(a => !string.IsNullOrWhiteSpace(a) || !a.IsColorTagOnly())
					.Select(a => a.RemoveAllTQTags())
					.ToArray();
				}
				return _AttributesAll;
			}
		}

		/// <summary>
		/// Used to give attribute list factory some kind of global awareness during its process
		/// </summary>
		public readonly List<string> TmpAttrib = new List<string>();

	}
}
