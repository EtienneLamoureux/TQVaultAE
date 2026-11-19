//-----------------------------------------------------------------------
// <copyright file="Item.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Domain.Entities;

/// <summary>
/// Class for holding item information
/// </summary>
public class Item
{
	private const StringComparison noCase = StringComparison.OrdinalIgnoreCase;

	#region Item Fields

	/// <summary>
	/// Default value for empty var2.
	/// </summary>
	public const int var2Default = 2035248;

	/// <summary>
	/// Binary marker for the beginning of a block
	/// </summary>
	public int beginBlockCrap1;

	/// <summary>
	/// Binary marker for the end of a block
	/// </summary>
	public int endBlockCrap1;

	/// <summary>
	/// A different binary marker for the beginning of a block
	/// </summary>
	public int beginBlockCrap2;

	/// <summary>
	/// A different binary marker for the end of a block
	/// </summary>
	public int endBlockCrap2;

	public bool atlantis = false;

	/// <summary>
	/// Prefix database record ID
	/// </summary>
	public RecordId prefixID = RecordId.Empty;

	/// <summary>
	/// Suffix database record ID
	/// </summary>
	public RecordId suffixID = RecordId.Empty;

	/// <summary>
	/// Relic database record ID
	/// </summary>
	public RecordId relicID = RecordId.Empty;

	/// <summary>
	/// Relic database record ID
	/// </summary>
	public RecordId relic2ID = RecordId.Empty;

	/// <summary>
	/// Info structure for the base item
	/// </summary>
	public Info? baseItemInfo;

	/// <summary>
	/// Info structure for the item's prefix
	/// </summary>
	public Info? prefixInfo;

	/// <summary>
	/// Info structure for the item's suffix
	/// </summary>
	public Info? suffixInfo;

	/// <summary>
	/// Used for level calculation
	/// </summary>
	public int attributeCount;

	/// <summary>
	/// Used so that attributes are not counted multiple times
	/// </summary>
	public bool isAttributeCounted;

	/// <summary>
	/// Used for itemScalePercent calculation
	/// </summary>
	public float itemScalePercent;

	#endregion Item Fields

	/// <summary>
	/// Initializes a new instance of the Item class.
	/// </summary>
	public Item()
	{
		// Added by VillageIdiot
		// Used for bare item attributes in properties display in this order:
		// baseinfo, artifactCompletionBonus, prefixinfo, suffixinfo, relicinfo, relicCompletionBonus
		this.itemScalePercent = 1.00F;
		this.StackSize = 1;
	}

	#region Item Properties

	public bool HasPrefix
		=> !RecordId.IsNullOrEmpty(prefixID);

	public bool HasSuffix
		=> !RecordId.IsNullOrEmpty(suffixID);

	/// <summary>
	/// Gets the value indicating whether the item allows 2 relic socketing
	/// </summary>
	public bool AcceptExtraRelic
		=> HasSuffix && suffixID.Normalized.EndsWith("RARE_EXTRARELIC_01.DBR", StringComparison.Ordinal);

	/// <summary>
	/// Tell if the item is modified
	/// </summary>
	public bool IsModified { get; set; }

	/// <summary>
	/// Gets the weapon slot indicator value.
	/// This is a special value in the coordinates that signals an item is in a weapon slot.
	/// </summary>
	public const int WeaponSlotIndicator = -3;

	/// <summary>
	/// Gets the base item id
	/// </summary>
	public RecordId BaseItemId { get; set; } = RecordId.Empty;

	/// <summary>
	/// True for an empty slot's item: it has no base record.
	/// </summary>
	public bool IsEmpty => RecordId.IsNullOrEmpty(this.BaseItemId);

	/// <summary>
	/// True for no item, or an empty slot's item (no base record).
	/// </summary>
	public static bool IsNullOrEmpty([NotNullWhen(false)] Item? item) => item is null || item.IsEmpty;

	/// <summary>
	/// Gets or sets the relic bonus id
	/// </summary>
	public RecordId RelicBonusId { get; set; } = RecordId.Empty;

	/// <summary>
	/// Gets or sets the relic bonus2 id
	/// </summary>
	public RecordId RelicBonus2Id { get; set; } = RecordId.Empty;

	/// <summary>
	/// Gets or sets the item seed
	/// </summary>
	public int Seed { get; set; }

	/// <summary>
	/// Gets the number of relics
	/// </summary>
	private int var1;

	/// <summary>
	/// Last <see cref="ToFriendlyNameResult"/> queried for this item.
	/// </summary>
	public ToFriendlyNameResult? CurrentFriendlyNameResult;

	public int Var1
	{
		get
		{
			// The "Power of Nerthus" relic is a special quest-reward relic with only 1 shard (it is always complete). 
			// Since its database var1 value is 0, we hard-set it to 1.
			if (IsRelicOrCharm && var1 == 0)
				return 1;

			return var1;
		}

		set => var1 = value;
	}

	/// <summary>
	/// Gets the number of relics for the second relic slot
	/// </summary>
	public int Var2 { get; set; }

	/// <summary>
	/// The records the item is made of, always in this order: base, prefix, suffix, first relic or charm at its number of
	/// pieces (<see cref="Var1"/>), its completion bonus, second relic (<see cref="Var2"/>), its completion bonus. An empty
	/// slot gives a null or empty id; the other records count 1.
	/// </summary>
	public (RecordId? Id, int Pieces)[] RecordSources()
		=> [
			(this.BaseItemId, 1),
			(this.prefixID, 1),
			(this.suffixID, 1),
			(this.relicID, this.Var1),
			(this.RelicBonusId, 1),
			(this.relic2ID, this.Var2),
			(this.RelicBonus2Id, 1),
		];

	/// <summary>The records of <see cref="RecordSources"/> the item has, each once.</summary>
	public IEnumerable<RecordId> RecordIds()
		=> this.RecordSources().Select(s => s.Id!).Where(id => !RecordId.IsNullOrEmpty(id)).Distinct();

	/// <summary>
	/// Gets or sets the stack size.
	/// Used for stackable items like potions.
	/// </summary>
	public int StackSize { get; set; }

	/// <summary>
	/// Gets or sets the X cell position of the item.
	/// </summary>
	public int PositionX { get; set; }

	/// <summary>
	/// Gets or sets the Y cell position of the item.
	/// </summary>
	public int PositionY { get; set; }

	/// <summary>
	/// Gets or sets the item's upper left corner in cell coordinates.
	/// </summary>
	public Point Location
	{
		get => new Point(this.PositionX, this.PositionY);

		set
		{
			this.PositionX = value.X;
			this.PositionY = value.Y;
		}
	}

	/// <summary>
	/// Gets the relic info
	/// </summary>
	public Info? RelicInfo { get; set; }

	/// <summary>
	/// Gets or sets the relic bonus info
	/// </summary>
	public Info? RelicBonusInfo { get; set; }

	/// <summary>
	/// Gets the second relic info
	/// </summary>
	public Info? Relic2Info { get; set; }

	/// <summary>
	/// Gets or sets the second relic bonus info
	/// </summary>
	public Info? RelicBonus2Info { get; set; }

	/// <summary>
	/// The mod the item comes from, computed from the records it was read with (<see cref="Info.Source"/>): the mod whose
	/// database gave at least one of its records (base item, affixes, relics and their bonuses), a record of its own or
	/// one it overrides; null when they all come from the game. Never saved: read again with another map (its container
	/// changed, or its container's link), the item may lose it or take another.
	/// </summary>
	public string? Mod
		=> this.RecordInfos().FirstOrDefault(info => info.Source.IsMod)?.Source.Mod;

	/// <summary>The records the item was read with, those found.</summary>
	private IEnumerable<Info> RecordInfos()
	{
		foreach (var info in new[] { this.baseItemInfo, this.prefixInfo, this.suffixInfo, this.RelicInfo, this.RelicBonusInfo, this.Relic2Info, this.RelicBonus2Info })
		{
			if (info is not null)
				yield return info;
		}
	}

	/// <summary>
	/// The map the item's records are read with: its container's (a character's link, a vault's link, the map of a
	/// shared stash), as a mod identity; null for the base game. Volatile: set when the item is put in a container
	/// (<see cref="SetContainerMod"/>), never saved. Kept by copies until they are placed.
	/// </summary>
	public string? ContainerMod { get; private set; }

	/// <summary>
	/// True once a container gave the item its map (<see cref="ContainerMod"/>): before, a placeholder or an item being
	/// read uses the data in use at that moment.
	/// </summary>
	public bool HasContainerMod { get; private set; }

	/// <summary>Gives the item the map of the container holding it.</summary>
	/// <param name="containerMod">The container's mod; null or blank for the base game.</param>
	/// <returns>True when the item's map changed: its records must be read again.</returns>
	public bool SetContainerMod(string? containerMod)
	{
		string? mod = string.IsNullOrWhiteSpace(containerMod) ? null : containerMod;
		bool changed = !this.HasContainerMod || !string.Equals(this.ContainerMod, mod, StringComparison.OrdinalIgnoreCase);
		this.ContainerMod = mod;
		this.HasContainerMod = true;
		return changed;
	}

	/// <summary>Takes the item's map away: it is read with the data in use at that moment, as before it was placed.</summary>
	public void ClearContainerMod()
	{
		this.ContainerMod = null;
		this.HasContainerMod = false;
	}

	/// <summary>
	/// True while the item is shown 1 x 1 because its real size has no room where it lies: its container's link changed
	/// and, read again, it took a size its bag cannot hold there. <see cref="Width"/> and <see cref="Height"/> then give
	/// one cell (<see cref="RealWidth"/> and <see cref="RealHeight"/> keep the real one), so that layout, drawing (a
	/// warning tile) and search see it as it lies. Never saved. Cleared (<see cref="ClearNoRoom"/>) once the item is put
	/// where its real size fits: moved or dropped there, or its container's link changed again.
	/// </summary>
	public bool LacksRoom { get; private set; }

	/// <summary>Shows the item 1 x 1: its real size has no room where it lies (<see cref="LacksRoom"/>).</summary>
	public void ShowNoRoom() => this.LacksRoom = true;

	/// <summary>Gives the item back its real size (<see cref="LacksRoom"/>).</summary>
	public void ClearNoRoom() => this.LacksRoom = false;

	/// <summary>
	/// True when the item shows the warning of an item that cannot be shown whole: records missing
	/// (<see cref="IsIncomplete"/>), or no room for its real size (<see cref="LacksRoom"/>).
	/// </summary>
	public bool HasWarning => this.LacksRoom || this.IsIncomplete;

	/// <summary>
	/// The records the item names that the loaded game data does not have (once read with <c>GetDBData</c>): typically
	/// those of a mod whose data is not loaded. The item is then shown incomplete.
	/// </summary>
	public IReadOnlyList<(ItemRecordRole Role, RecordId Id)> MissingRecords
	{
		get
		{
			var missing = new List<(ItemRecordRole, RecordId)>();
			Check(ItemRecordRole.BaseItem, this.BaseItemId, this.baseItemInfo);
			Check(ItemRecordRole.Prefix, this.prefixID, this.prefixInfo);
			Check(ItemRecordRole.Suffix, this.suffixID, this.suffixInfo);
			Check(ItemRecordRole.Relic, this.relicID, this.RelicInfo);
			Check(ItemRecordRole.RelicBonus, this.RelicBonusId, this.RelicBonusInfo);
			Check(ItemRecordRole.Relic, this.relic2ID, this.Relic2Info);
			Check(ItemRecordRole.RelicBonus, this.RelicBonus2Id, this.RelicBonus2Info);
			return missing;

			void Check(ItemRecordRole role, RecordId? id, Info? info)
			{
				if (!RecordId.IsNullOrEmpty(id) && info is null)
					missing.Add((role, id));
			}
		}
	}

	/// <summary>
	/// The item names records the loaded game data does not have: see <see cref="MissingRecords"/>.
	/// </summary>
	public bool IsIncomplete => this.MissingRecords.Count > 0;

	/// <summary>
	/// Resource id of the item's picture, read from the game files when the item is first drawn.
	/// </summary>
	public RecordId? TexImageResourceId { get; set; }

	/// <summary>
	/// Gets the item's width in cells
	/// </summary>
	public int Width
	{
		get => this.LacksRoom ? 1 : this.RealWidth;
		set => this.RealWidth = value;
	}

	/// <summary>
	/// The item's width in cells, read from its picture, even while it is shown 1 x 1 (<see cref="LacksRoom"/>).
	/// </summary>
	public int RealWidth { get; private set; }

	/// <summary>
	/// Gets the item's height in cells.
	/// </summary>
	public int Height
	{
		get => this.LacksRoom ? 1 : this.RealHeight;
		set => this.RealHeight = value;
	}

	/// <summary>
	/// The item's height in cells, read from its picture, even while it is shown 1 x 1 (<see cref="LacksRoom"/>).
	/// </summary>
	public int RealHeight { get; private set; }

	/// <summary>
	/// Gets the item's size in cells.
	/// </summary>
	public Size Size => new Size(this.Width, this.Height);

	/// <summary>
	/// Gets the item's right cell location
	/// </summary>
	public int Right => this.Location.X + this.Width;

	/// <summary>
	/// Gets the item's bottom cell location.
	/// </summary>
	public int Bottom => this.Location.Y + this.Height;

	/// <summary>
	/// Gets or sets the container location information for this item
	/// </summary>
	public ContainerPlace Place { get; set; } = new();

	/// <summary>
	/// Gets a value indicating whether the item is in an equipment weapon slot.
	/// </summary>
	public bool IsInWeaponSlot => this.PositionX == WeaponSlotIndicator;

	/// <summary>
	/// Tells whether the item is a weapon of the set not in hand, whose bonuses do not count: rows 2 and 3 of the weapon
	/// slots, or rows 0 and 1 when the second set is in hand (the save's <c>useAlternate</c>).
	/// </summary>
	public bool IsInInactiveWeaponSlot(bool useAlternate)
		=> this.IsInWeaponSlot && (useAlternate ? this.PositionY is 0 or 1 : this.PositionY is 2 or 3);

	/// <summary>
	/// Gets a value indicating whether or not the item comes from Immortal Throne expansion pack.
	/// </summary>
	public bool IsImmortalThrone
		=> (this.BaseItemId?.Dlc == GameDlc.ImmortalThrone
		|| (this.prefixID?.Dlc.Equals(GameDlc.ImmortalThrone) ?? false)
		|| (this.suffixID?.Dlc.Equals(GameDlc.ImmortalThrone) ?? false)
		) && !IsRagnarok && !IsAtlantis && !IsEmbers;

	/// <summary>
	/// Gets a value indicating whether or not the item comes from Ragnarok DLC.
	/// </summary>
	public bool IsRagnarok
		=> (this.BaseItemId?.Dlc == GameDlc.Ragnarok
		|| (this.prefixID?.Dlc.Equals(GameDlc.Ragnarok) ?? false)
		|| (this.suffixID?.Dlc.Equals(GameDlc.Ragnarok) ?? false)
		) && !IsAtlantis && !IsEmbers;

	/// <summary>
	/// Gets a value indicating whether or not the item comes from Atlantis DLC.
	/// </summary>
	public bool IsAtlantis
		=> (this.BaseItemId?.Dlc == GameDlc.Atlantis
		|| (this.prefixID?.Dlc.Equals(GameDlc.Atlantis) ?? false)
		|| (this.suffixID?.Dlc.Equals(GameDlc.Atlantis) ?? false)
		) && !IsEmbers;

	/// <summary>
	/// Gets a value indicating whether or not the item comes from Eternal Embers DLC.
	/// </summary>
	public bool IsEmbers
		=> this.BaseItemId?.Dlc == GameDlc.EternalEmbers
		|| (this.prefixID?.Dlc.Equals(GameDlc.EternalEmbers) ?? false)
		|| (this.suffixID?.Dlc.Equals(GameDlc.EternalEmbers) ?? false);


	/// <summary>
	/// The item or one of its affixes comes from an expansion (Immortal Throne, Ragnarok, Atlantis, Eternal Embers).
	/// </summary>
	private bool IsFromExpansion
		=> this.IsImmortalThrone || this.IsRagnarok || this.IsAtlantis || this.IsEmbers;

	/// <summary>
	/// Gets a value indicating whether the item is a scroll.
	/// </summary>
	public bool IsScroll
	{
		get
		{
			if (this.baseItemInfo is not null)
				return this.baseItemInfo.ItemClass.Equals(ICLASS_SCROLL, noCase);
			else if (this.BaseItemId is not null
				&& this.IsFromExpansion
				&& this.BaseItemId.IsScroll)
				return true;

			return false;
		}
	}

	/// <summary>
	/// Gets a value indicating whether the item is a parchment.
	/// </summary>
	public bool IsParchment
	{
		get
		{
			if (this.BaseItemId is not null
				&& this.IsFromExpansion
				&& this.BaseItemId.IsParchment)
				return true;

			return false;
		}
	}

	/// <summary>
	/// Gets a value indicating whether the item is a formulae.
	/// </summary>
	public bool IsFormulae
	{
		get
		{
			if (this.baseItemInfo is not null)
				return this.baseItemInfo.ItemClass.Equals(ICLASS_FORMULA, noCase);
			else if (this.BaseItemId is not null
				&& this.IsFromExpansion
				&& this.BaseItemId.IsFormulae)
				return true;

			return false;
		}
	}

	/// <summary>
	/// Gets a value indicating whether the item is an artifact.
	/// </summary>
	public bool IsArtifact
	{
		get
		{
			if (this.baseItemInfo is not null)
				return this.baseItemInfo.ItemClass.Equals(ICLASS_ARTIFACT, noCase);
			else if (this.BaseItemId is not null
				&& this.IsFromExpansion
				&& this.BaseItemId.IsArtifact)
				return true;

			return false;
		}
	}

	/// <summary>
	/// Gets a value indicating whether the item is a shield.
	/// </summary>
	public bool IsShield
		=> this.HasClass(ICLASS_SHIELD);

	/// <summary>
	/// Gets a value indicating whether the item is armor.
	/// </summary>
	public bool IsArmor
		=> this.HasClassPrefix(ICLASS_PREFIX_ARMORPROTECTIVE);

	/// <summary>
	/// Gets a value indicating whether the item is a helm.
	/// </summary>
	public bool IsHelm
		=> this.HasClass(ICLASS_HEAD);

	/// <summary>
	/// Gets a value indicating whether the item is a bracer.
	/// </summary>
	public bool IsBracer
		=> this.HasClass(ICLASS_FOREARM);

	/// <summary>
	/// Gets a value indicating whether the item is torso armor.
	/// </summary>
	public bool IsTorsoArmor
		=> this.HasClass(ICLASS_UPPERBODY);

	/// <summary>
	/// Gets a value indicating whether the item is a greave.
	/// </summary>
	public bool IsGreave
		=> this.HasClass(ICLASS_LOWERBODY);

	/// <summary>
	/// Gets a value indicating whether the item is Jewellery.
	/// </summary>
	public bool IsJewellery
		=> IsRing || IsAmulet;

	/// <summary>
	/// Gets a value indicating whether the item is a ring.
	/// </summary>
	public bool IsRing
		=> this.HasClass(ICLASS_RING);

	/// <summary>
	/// Gets a value indicating whether the item is an amulet.
	/// </summary>
	public bool IsAmulet
		=> this.HasClass(ICLASS_AMULET);

	/// <summary>
	/// Gets a value indicating whether the item is a weapon.
	/// </summary>
	public bool IsWeapon
		=> !this.IsShield && this.HasClassPrefix(ICLASS_PREFIX_WEAPON);

	/// <summary>
	/// Gets a value indicating whether the item is a thrown weapon.
	/// </summary>
	public bool IsThrownWeapon
		=> this.HasClass(ICLASS_RANGEDONEHAND);

	/// <summary>
	/// Gets a value indicating whether the item is a two handed weapon.
	/// </summary>
	public bool Is2HWeapon
		=> this.HasClass(ICLASS_BOW) || this.HasClass(ICLASS_STAFF);

	/// <summary>
	/// Gets a value indicating whether the item is a weapon or shield.
	/// </summary>
	public bool IsWeaponShield
		=> this.HasClassPrefix(ICLASS_PREFIX_WEAPON);

	/// <summary>The item's class (from its base record) is <paramref name="itemClass"/>, whatever the case.</summary>
	private bool HasClass(string itemClass)
		=> this.baseItemInfo is not null && this.baseItemInfo.ItemClass.Equals(itemClass, noCase);

	/// <summary>The item's class (from its base record) starts with <paramref name="prefix"/>, whatever the case.</summary>
	private bool HasClassPrefix(string prefix)
		=> this.baseItemInfo is not null && this.baseItemInfo.ItemClass.StartsWith(prefix, noCase);

	/// <summary>
	/// Gets a value indicating whether the item is a quest item.
	/// </summary>
	public bool IsQuestItem
	{
		get
		{
			if (this.baseItemInfo is not null)
			{
				if (this.baseItemInfo.ItemClassification.Equals("QUEST", noCase)
					|| this.baseItemInfo.ItemClass.Equals(ICLASS_QUESTITEM, noCase))
					return true;
			}
			else if (this.BaseItemId is not null && this.BaseItemId.IsQuestItem)
				return true;

			return false;
		}
	}

	/// <summary>
	/// Get a value indicating gear level from <see cref="Rarity.Broken"/> to <see cref="Rarity.Legendary"/>
	/// </summary>
	public Rarity Rarity
	{
		get
		{
			switch (ItemStyle)
			{
				case ItemStyle.Broken:
					return Rarity.Broken;
				case ItemStyle.Mundane:
					return Rarity.Mundane;
				case ItemStyle.Common:
					return Rarity.Common;
				case ItemStyle.Rare:
					return Rarity.Rare;
				case ItemStyle.Epic:
					return Rarity.Epic;
				case ItemStyle.Legendary:
					return Rarity.Legendary;
				case ItemStyle.Quest:
				case ItemStyle.Relic:
				case ItemStyle.Potion:
				case ItemStyle.Scroll:
				case ItemStyle.Parchment:
				case ItemStyle.Formulae:
				case ItemStyle.Artifact:
				default:
					return Rarity.NoGear;
			}
		}
	}

	/// <summary>
	/// Gets the item style enumeration
	/// </summary>
	public ItemStyle ItemStyle
	{
		get
		{
			if (this.prefixInfo?.ItemClassification.Equals("BROKEN", noCase) ?? false)
				return ItemStyle.Broken;

			if (this.IsArtifact)
				return ItemStyle.Artifact;

			if (this.IsFormulae)
				return ItemStyle.Formulae;

			if (this.IsScroll)
				return ItemStyle.Scroll;

			if (this.IsParchment)
				return ItemStyle.Parchment;

			if (this.IsRelicOrCharm)
				return ItemStyle.Relic;

			if (this.IsPotion)
				return ItemStyle.Potion;

			if (this.IsQuestItem)
				return ItemStyle.Quest;

			if (this.baseItemInfo is not null)
			{
				if (this.baseItemInfo.ItemClassification.Equals("EPIC", noCase))
					return ItemStyle.Epic;

				if (this.baseItemInfo.ItemClassification.Equals("LEGENDARY", noCase))
					return ItemStyle.Legendary;

				if (this.baseItemInfo.ItemClassification.Equals("RARE", noCase))
					return ItemStyle.Rare;
			}

			// At this point baseItem indicates Common.  Let's check affixes
			if ((this.suffixInfo?.ItemClassification.Equals("RARE", noCase) ?? false)
				|| (this.prefixInfo?.ItemClassification.Equals("RARE", noCase) ?? false)
			) return ItemStyle.Rare;

			// Not rare.  If we have a suffix or prefix, then call it common, else mundane
			if (this.suffixInfo is not null || this.prefixInfo is not null)
				return ItemStyle.Common;

			return ItemStyle.Mundane;
		}
	}

	/// <summary>
	/// Gets the item class
	/// </summary>
	public string ItemClass
		=> (this.baseItemInfo is null) ? string.Empty : this.baseItemInfo.ItemClass;

	/// <summary>
	/// Gets a value indicating whether or not the item will stack.
	/// </summary>
	public bool DoesStack => this.IsPotion || this.IsScroll;

	/// <summary>
	/// Gets a value indicating whether the item has a number attached.
	/// </summary>
	public bool HasNumber => this.DoesStack || (this.IsRelicOrCharm && !this.IsRelicComplete);

	/// <summary>
	/// Gets or sets the number attached to the item.
	/// </summary>
	public int Number
	{
		get
		{
			if (this.DoesStack)
				return this.StackSize;

			if (this.IsRelicOrCharm)
				return Math.Max(this.Var1, 1);

			return 0;
		}

		set
		{
			// Added by VillageIdiot
			if (this.DoesStack)
				this.StackSize = value;

			else if (this.IsRelicOrCharm)
			{
				// Limit value to complete Relic level
				if (this.baseItemInfo is { } info && value >= info.CompletedRelicLevel)
					this.Var1 = info.CompletedRelicLevel;
				else
					this.Var1 = value;
			}
		}
	}

	/// <summary>
	/// Gets a value indicating whether the item has an embedded charm in first slot.
	/// </summary>
	public bool HasCharmSlot1 => this.HasSocketedSlot1 && SocketedKind(this.relicID, this.RelicInfo) == SocketedRecordKind.Charm;

	/// <summary>
	/// Gets a value indicating whether the item has an embedded charm in second slot.
	/// </summary>
	public bool HasCharmSlot2 => this.HasSocketedSlot2 && SocketedKind(this.relic2ID, this.Relic2Info) == SocketedRecordKind.Charm;

	/// <summary>
	/// Gets a value indicating whether the item has an embedded relic only in first slot.
	/// </summary>
	public bool HasRelicSlot1 => this.HasSocketedSlot1 && SocketedKind(this.relicID, this.RelicInfo) == SocketedRecordKind.Relic;

	/// <summary>
	/// Gets a value indicating whether the item has an embedded relic only in second slot.
	/// </summary>
	public bool HasRelicSlot2 => this.HasSocketedSlot2 && SocketedKind(this.relic2ID, this.Relic2Info) == SocketedRecordKind.Relic;

	private enum SocketedRecordKind { Other, Relic, Charm }

	/// <summary>
	/// What a relic slot holds. The loaded record's class decides, as in the game: the golden scarabs are relics although
	/// their file names end in "_charm". The record path is only the fallback when the record is not loaded.
	/// </summary>
	private static SocketedRecordKind SocketedKind(RecordId? id, Info? info)
	{
		if (info is not null)
			return info.ItemClass.Equals(ICLASS_CHARM, noCase) ? SocketedRecordKind.Charm
				: info.ItemClass.Equals(ICLASS_RELIC, noCase) ? SocketedRecordKind.Relic
				: SocketedRecordKind.Other;

		if (RecordId.IsNullOrEmpty(id))
			return SocketedRecordKind.Other;

		return id.IsCharm ? SocketedRecordKind.Charm
			: id.IsRelic ? SocketedRecordKind.Relic
			: SocketedRecordKind.Other;
	}

	/// <summary>
	/// Gets a value indicating whether the item has an embedded relic or a charm.
	/// </summary>
	public bool HasRelicOrCharmSlot1 => HasCharmSlot1 || HasRelicSlot1;

	/// <summary>
	/// Gets a value indicating whether the item has a second embedded relic or a charm.
	/// </summary>
	public bool HasRelicOrCharmSlot2 => HasCharmSlot2 || HasRelicSlot2;

	/// <summary>
	/// Indicate that the item has an embedded relic or a charm.
	/// </summary>
	public bool HasRelicOrCharm => HasRelicOrCharmSlot1 || HasRelicOrCharmSlot2;

	/// <summary>
	/// The first relic slot holds a record, whatever it is: a relic, a charm, or anything the Forge God mode put there
	/// (e.g. a weapon). Unlike <see cref="HasRelicOrCharmSlot1"/>, it does not look at the record type.
	/// </summary>
	public bool HasSocketedSlot1 => !this.IsRelicOrCharm && !RecordId.IsNullOrEmpty(this.relicID);

	/// <summary>The second relic slot holds a record, whatever it is (see <see cref="HasSocketedSlot1"/>).</summary>
	public bool HasSocketedSlot2 => !this.IsRelicOrCharm && !RecordId.IsNullOrEmpty(this.relic2ID);

	/// <summary>
	/// Indicate that the item has an embedded relic in slot 1 or 2.
	/// </summary>
	public bool HasRelic => HasRelicSlot1 || HasRelicSlot2;

	/// <summary>
	/// Indicate that the item has an embedded charm in slot 1 or 2.
	/// </summary>
	public bool HasCharm => HasCharmSlot1 || HasCharmSlot2;

	/// <summary>
	/// Gets a value indicating whether the item is a potion.
	/// </summary>
	public bool IsPotion
	{
		get
		{
			if (this.baseItemInfo is not null)
				return this.baseItemInfo.ItemClass.Equals(ICLASS_POTIONHEALTH, noCase)
					|| this.baseItemInfo.ItemClass.Equals(ICLASS_POTIONMANA, noCase)
					|| this.baseItemInfo.ItemClass.Equals(ICLASS_SCROLL_ETERNAL, noCase); //AMS: New EE Potions (Mystical Potions)

			return this.BaseItemId is not null && this.BaseItemId.IsPotion;
		}
	}

	/// <summary>
	/// Gets a value indicating whether the item is a charm and only a charm.
	/// </summary>
	public bool IsCharmOnly
	{
		get
		{
			if (this.baseItemInfo is not null)
				return this.baseItemInfo.ItemClass.Equals(ICLASS_CHARM, noCase);

			return this.BaseItemId is not null && this.BaseItemId.IsCharm;
		}
	}

	/// <summary>
	/// Gets a value indicating whether the item first relic is a charm and only a charm.
	/// </summary>
	public bool IsRelic1Charm => SocketedKind(this.relicID, this.RelicInfo) == SocketedRecordKind.Charm;

	/// <summary>
	/// Gets a value indicating whether the item second relic is a charm and only a charm.
	/// </summary>
	public bool IsRelic2Charm => SocketedKind(this.relic2ID, this.Relic2Info) == SocketedRecordKind.Charm;

	/// <summary>
	/// Gets a value indicating whether the item is a relic or a charm.
	/// </summary>
	public bool IsRelicOrCharm
	{
		get
		{
			if (this.baseItemInfo is not null)
				return this.baseItemInfo.ItemClass.Equals(ICLASS_RELIC, noCase)
					|| this.baseItemInfo.ItemClass.Equals(ICLASS_CHARM, noCase);

			return this.BaseItemId is not null && (this.BaseItemId.IsRelic || this.BaseItemId.IsCharm);
		}
	}

	/// <summary>
	/// Indicate if this item is a completed relic.
	/// </summary>
	public bool IsRelicComplete
		=> (this.baseItemInfo is not null) ? this.Var1 >= this.baseItemInfo.CompletedRelicLevel : false;

	/// <summary>
	/// Indicate if the first relic completion bonus apply.
	/// </summary>
	public bool IsRelicBonus1Complete
		=> (this.RelicBonusInfo is not null) ? this.Var1 >= this.RelicBonusInfo.CompletedRelicLevel : false;

	/// <summary>
	/// Indicate if the second relic completion bonus apply.
	/// </summary>
	public bool IsRelicBonus2Complete
		=> (this.RelicBonus2Info is not null) ? this.Var2 >= this.RelicBonus2Info.CompletedRelicLevel : false;

	#region	ItemClass const

	/// <summary>Start of the classes of armor pieces (head, forearm, upper and lower body).</summary>
	public const string ICLASS_PREFIX_ARMORPROTECTIVE = "ArmorProtective";
	/// <summary>Start of the classes of jewellery (rings, amulets).</summary>
	public const string ICLASS_PREFIX_ARMORJEWELRY = "ArmorJewelry";
	/// <summary>Start of the classes of weapons and shields.</summary>
	public const string ICLASS_PREFIX_WEAPON = "Weapon";
	public const string ICLASS_PREFIX_MELEE = "WeaponMelee_";
	public const string ICLASS_HEAD = "ArmorProtective_Head";
	public const string ICLASS_FOREARM = "ArmorProtective_Forearm";
	public const string ICLASS_UPPERBODY = "ArmorProtective_UpperBody";
	public const string ICLASS_LOWERBODY = "ArmorProtective_LowerBody";
	public const string ICLASS_AXE = "WeaponMelee_Axe";
	public const string ICLASS_MACE = "WeaponMelee_Mace";
	public const string ICLASS_SWORD = "WeaponMelee_Sword";
	public const string ICLASS_SHIELD = "WeaponArmor_Shield";
	public const string ICLASS_RANGEDONEHAND = "WeaponHunting_RangedOneHand";
	public const string ICLASS_BOW = "WeaponHunting_Bow";
	public const string ICLASS_SPEAR = "WeaponHunting_Spear";
	public const string ICLASS_STAFF = "WeaponMagical_Staff";
	public const string ICLASS_AMULET = "ArmorJewelry_Amulet";
	public const string ICLASS_RING = "ArmorJewelry_Ring";
	public const string ICLASS_ARTIFACT = "ItemArtifact";
	public const string ICLASS_FORMULA = "ItemArtifactFormula";
	public const string ICLASS_RELIC = "ItemRelic";
	public const string ICLASS_CHARM = "ItemCharm";
	public const string ICLASS_SCROLL = "OneShot_Scroll";
	public const string ICLASS_POTIONMANA = "OneShot_PotionMana";
	public const string ICLASS_POTIONHEALTH = "OneShot_PotionHealth";
	public const string ICLASS_SCROLL_ETERNAL = "OneShot_Scroll_Eternal";
	public const string ICLASS_QUESTITEM = "QuestItem";
	public const string ICLASS_EQUIPMENT = "ItemEquipment";
	public const string ICLASS_DYE = "OneShot_Dye";

	#endregion

	internal static ReadOnlyCollection<ItemClassMapItem<string>> ItemClassToTagNameMap = new List<ItemClassMapItem<string>>
	{
		new (ICLASS_QUESTITEM, "tagQuestItem"),
		new (ICLASS_POTIONHEALTH, "tagHUDHealthPotion"),
		new (ICLASS_POTIONMANA, "tagHUDEnergyPotion"),
		new (ICLASS_SCROLL_ETERNAL, "x4tag_PotionReward"), // Translate into: Mystical Potion
		new (ICLASS_AMULET, "tagItemAmulet") ,
		new (ICLASS_RING, "tagItemRing") ,
		new (ICLASS_CHARM, "tagItemCharm") ,
		new (ICLASS_RELIC, "tagRelic"),
		new (ICLASS_FORMULA, "xtagEnchant02"),
		new (ICLASS_ARTIFACT, "tagArtifact"),
		new (ICLASS_SCROLL, "xtagLogScroll"),
		new (ICLASS_LOWERBODY, "tagCR_Leg"),
		new (ICLASS_FOREARM, "tagCR_Arm"),
		new (ICLASS_HEAD, "tagCR_Head"),
		new (ICLASS_UPPERBODY, "tagCR_Torso"),
		new (ICLASS_SHIELD, "tagItemWarShield"),
		new (ICLASS_AXE, "tagItemAxe"),
		new (ICLASS_MACE, "tagItemMace"),
		new (ICLASS_SWORD, "tagItemWarBlade"),//tagSword
		new (ICLASS_BOW, "tagItemWarBow"),
		new (ICLASS_RANGEDONEHAND, "x2tagThrownWeapon"),
		new (ICLASS_STAFF, "tagItemBattleStaff"),// xtagLogStaff
		new (ICLASS_SPEAR,"tagItemLance"),
		new (ICLASS_EQUIPMENT,"xtagArtifactReagentTypeEquipment"),
		new (ICLASS_DYE,"tagDye"),
	}.AsReadOnly();

	/// <summary>
	/// Get the tagName assigned to the <see cref="ItemClass"/>
	/// </summary>
	public string ItemClassTagName
		=> GetClassTagName(this.baseItemInfo?.ItemClass ?? string.Empty);

	/// <summary>
	/// Get the tagName assigned to the <see cref="ItemClass"/>
	/// </summary>
	public static string GetClassTagName(string ItemClass)
		=> ItemClassToTagNameMap.Where(i => i.ItemClass.Equals(ItemClass, noCase)).Select(i => i.Value).FirstOrDefault() ?? ItemClass;

	/// <summary>
	/// Gets the item group.
	/// Used for grouping during autosort.
	/// </summary>
	public int ItemGroup
	{
		get
		{
			if (this.baseItemInfo is null)
				return 0;

			var iclass = this.baseItemInfo.ItemClass;
			return ItemClassToTagNameMap.Select((val, index) => new { val, index })
				.Where(m => m.val.ItemClass.Equals(iclass, noCase))
				.Select(m => m.index)
				.FirstOrDefault();
		}
	}

	#endregion Item Properties

	#region Item Public Static Methods


	/// <summary>Highest item seed: the seeds in the player files are limited to 0x7FFF.</summary>
	public const int MaxSeed = 0x7FFF;

	/// <summary>Whether a seed can be given to an item: from 1 to <see cref="MaxSeed"/>.</summary>
	public static bool IsValidSeed(int seed) => seed is >= 1 and <= MaxSeed;

	/// <summary>
	/// Generates a new seed that can be used on a new item
	/// </summary>
	/// <returns>New seed from 1 to <see cref="MaxSeed"/></returns>
	public static int GenerateSeed()
		=> Random.Shared.Next(1, MaxSeed + 1);

	#endregion Item Public Static Methods

	/// <summary>
	/// Creates an empty item
	/// </summary>
	/// <returns>Empty Item structure</returns>
	public Item MakeEmptyItem() =>
		new Item()
		{
			beginBlockCrap1 = this.beginBlockCrap1,
			endBlockCrap1 = this.endBlockCrap1,
			beginBlockCrap2 = this.beginBlockCrap2,
			endBlockCrap2 = this.endBlockCrap2,
			BaseItemId = RecordId.Empty,
			prefixID = RecordId.Empty,
			suffixID = RecordId.Empty,
			relicID = RecordId.Empty,
			relic2ID = RecordId.Empty,
			RelicBonusId = RecordId.Empty,
			RelicBonus2Id = RecordId.Empty,
			Seed = GenerateSeed(),
			Var1 = this.var1,
			Var2 = var2Default,
			PositionX = -1,
			PositionY = -1,
		}.WithContainerModOf(this);

	/// <summary>
	/// A new item made from this one (a relic taken out, an artifact, a set member) is read with its map.
	/// </summary>
	private Item WithContainerModOf(Item source)
	{
		this.ContainerMod = source.ContainerMod;
		this.HasContainerMod = source.HasContainerMod;
		return this;
	}

	/// <summary>
	/// Makes a new item based on the passed item id string
	/// </summary>
	/// <param name="baseItemId">base item id of the new item</param>
	/// <returns>Empty Item structure based on the passed item string</returns>
	public Item MakeEmptyCopy(RecordId baseItemId)
	{
		if (RecordId.IsNullOrEmpty(baseItemId))
			throw new ArgumentNullException(nameof(baseItemId), "The base item ID cannot be NULL or Empty.");

		Item newItem = MakeEmptyItem();

		newItem.BaseItemId = baseItemId;

		return newItem;
	}

	/// <summary>
	/// Shallow copy of this item that shares no mutable state with it: its own <see cref="Place"/>, and no
	/// <see cref="CurrentFriendlyNameResult"/> (that result describes this item, not the copy).
	/// </summary>
	/// <remarks>
	/// A shared <see cref="Place"/> made both items report the location of whichever was moved last, so search
	/// results pointed the original to the copy's bag.
	/// </remarks>
	private Item CopyDetached()
	{
		Item newItem = (Item)this.MemberwiseClone();
		newItem.Place = this.Place with { };
		newItem.CurrentFriendlyNameResult = null;
		return newItem;
	}

	/// <summary>
	/// Makes a duplicate of the item
	/// VillageIdiot - Added option to keep item seed.
	/// </summary>
	/// <param name="keepItemSeed">flag on whether we are keeping our item seed or creating a new one.</param>
	/// <returns>New Duplicated item</returns>
	public Item Duplicate(bool keepItemSeed)
	{
		Item newItem = this.CopyDetached();
		if (!keepItemSeed)
			newItem.Seed = GenerateSeed();

		newItem.PositionX = -1;
		newItem.PositionY = -1;
		newItem.IsModified = true;

		return newItem;
	}

	/// <summary>
	/// Clones the item
	/// </summary>
	/// <returns>A new clone of the item</returns>
	public Item Clone()
	{
		Item newItem = this.CopyDetached();
		newItem.IsModified = true;
		return newItem;
	}

	/// <summary>
	/// Pops all but one item from the stack.
	/// </summary>
	/// <returns>Returns popped items as a new Item stack</returns>
	public Item? PopAllButOneItem()
	{
		if (!this.DoesStack || this.StackSize < 2)
			return null;

		// make a complete copy then change a few things
		Item newItem = this.CopyDetached();

		newItem.StackSize = this.StackSize - 1;
		newItem.Var1 = 0;
		newItem.Seed = GenerateSeed();
		newItem.PositionX = -1;
		newItem.PositionY = -1;
		newItem.IsModified = true;

		this.IsModified = true;

		this.StackSize = 1;

		return newItem;
	}

	#region GearType related

	/// <summary>
	/// <see cref="GearType"/> of this Item.
	/// </summary>
	/// <remarks>return <see cref="GearType.Undefined"/> if not a piece of gear</remarks>
	public GearType GearType
		=> GearTypeExtension.FromItemClass(this.ItemClass);

	#endregion

	#region IsRelicAllowed

	/// <summary>
	/// Tells if <paramref name="relicBaseItemId"/> is allowed.
	/// </summary>
	/// <param name="relicBaseItemId"></param>
	/// <returns></returns>
	/// <param name="relicInfo">Record of the relic: its slot flags give the gear it allows.</param>
	public bool IsRelicAllowed(RecordId relicBaseItemId, Info? relicInfo = null)
		=> IsRelicAllowed(this, relicBaseItemId, relicInfo);

	/// <summary>
	/// Tells if <paramref name="relicItem"/> is allowed.
	/// </summary>
	/// <param name="relicItem"></param>
	/// <returns></returns>
	public bool IsRelicAllowed(Item relicItem)
		=> IsRelicAllowed(this, relicItem);

	/// <summary>
	/// Tells if <paramref name="relicItem"/> is allowed on <paramref name="item"/>.
	/// </summary>
	/// <param name="item"></param>
	/// <param name="relicItem"></param>
	/// <returns></returns>
	public static bool IsRelicAllowed(Item item, Item relicItem)
	{
		// relicItem a relic ?
		if (!relicItem.IsRelicOrCharm) throw new ArgumentException("Must be a relic or a charm!", nameof(relicItem));

		var itemGearType = item.GearType;

		if (itemGearType == GearType.Undefined) return false;

		// Is it allowed ?
		if (!TryGetAllowedGearTypes(relicItem, out var relicAllowedGearTypes))
			return false;

		return relicAllowedGearTypes.HasFlag(itemGearType);
	}

	/// <summary>
	/// Tells if <paramref name="relicBaseItemId"/> is allowed on <paramref name="item"/>.
	/// </summary>
	/// <param name="item"></param>
	/// <param name="relicBaseItemId"></param>
	/// <returns></returns>
	/// <param name="relicInfo">Record of the relic: its slot flags give the gear it allows.</param>
	public static bool IsRelicAllowed(Item item, RecordId relicBaseItemId, Info? relicInfo = null)
	{
		var itemGearType = item.GearType;

		if (itemGearType == GearType.Undefined) return false;

		// Is it allowed ?
		if (!TryGetAllowedGearTypes(relicBaseItemId, out var relicAllowedGearTypes, relicInfo))
			return false;

		return relicAllowedGearTypes.HasFlag(itemGearType);
	}

	#endregion

	#region TryGetAllowedGearTypes

	/// <summary>
	/// Try to get <paramref name="relicItem"/> allowed <see cref="GearType"/>
	/// </summary>
	/// <param name="relicItem"></param>
	/// <param name="types"></param>
	/// <returns></returns>
	public static bool TryGetAllowedGearTypes(Item relicItem, out GearType types)
	{
		types = GearType.Undefined;

		// relicItem a relic ?
		if (!relicItem.IsRelicOrCharm) return false;

		return TryGetAllowedGearTypes(relicItem.BaseItemId, out types, relicItem.baseItemInfo);
	}

	/// <summary>
	/// Try to get allowed <see cref="GearType"/> for that <paramref name="relicBaseItemId"/>
	/// </summary>
	/// <param name="relicBaseItemId"></param>
	/// <param name="types"></param>
	/// <param name="relicInfo">Record of the relic: its slot flags give the gear. Ignored when it is the record of another item.</param>
	/// <returns><see langword="false"/> without the relic's record, or when its record allows no gear.</returns>
	public static bool TryGetAllowedGearTypes(RecordId relicBaseItemId, out GearType types, Info? relicInfo = null)
	{
		types = GearType.Undefined;

		if (RecordId.IsNullOrEmpty(relicBaseItemId)) return false;

		if (relicInfo is not null && relicInfo.ItemId == relicBaseItemId)
			types = RelicAndCharmExtension.GetAllowedGearTypes(relicInfo);

		return types != GearType.Undefined;
	}

	#endregion

	#region GameDlc

	public GameDlc GameDlc
	{
		get
		{
			if (this.IsImmortalThrone) return GameDlc.ImmortalThrone;
			else if (this.IsRagnarok) return GameDlc.Ragnarok;
			else if (this.IsAtlantis) return GameDlc.Atlantis;
			else if (this.IsEmbers) return GameDlc.EternalEmbers;
			else return GameDlc.TitanQuest;
		}
	}

	public string GameDlcCode
		=> this.GameDlc.GetCode();

	public string GameDlcSuffix
	{
		get
		{
			string code;
			if ((code = this.GameDlc.GetSuffix()) == string.Empty) return string.Empty;
			return ItemStyle.Quest.TQColor().ColorTag() + code;
		}
	}

	#endregion

	/// <summary>
	/// Extract color from <paramref name="TQText"/> and fallback to default item color if none.
	/// </summary>
	/// <param name="TQText">text containing the color tag</param>
	/// <returns><see cref="System.Drawing.Color"/> of the embedded color code</returns>
	public Color ExtractTextColorOrItemColor(string? TQText)
	{
		if (string.IsNullOrWhiteSpace(TQText))
			// Use the standard color code for the item
			return ItemStyle.Color();

		// Look for a formatting tag in the beginning of the string
		TQColor? colorCode = TQColorHelper.GetColorFromTaggedString(TQText);

		// We didn't find a code so use the standard color code for the item
		if (colorCode is null)
			return ItemStyle.Color();

		// We found something so lets try to find the code
		return colorCode.Value.Color();
	}

}
