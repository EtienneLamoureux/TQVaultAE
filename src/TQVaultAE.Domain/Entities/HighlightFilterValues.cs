namespace TQVaultAE.Domain.Entities;

public class HighlightFilterValues
{
	public int MaxStr;
	public int MaxDex;
	public int MaxInt;
	public int MinStr;
	public int MinDex;
	public int MinInt;
	public bool MinRequierement;
	public bool MaxRequierement;
	public List<string> ClassItem = [];
	public List<Rarity> Rarity = [];
	public int MaxLvl;
	public int MinLvl;
	public List<GameDlc> Origin = [];
	public bool HavingPrefix;
	public bool HavingSuffix;
	public bool HavingRelic;
	public bool HavingCharm;
	public bool IsSetItem;
	/// <summary>Only the items that grant a skill.</summary>
	public bool HavingSkill;
	/// <summary>Only the items from a mod (<see cref="Item.Mod"/> set).</summary>
	public bool Modded;
	/// <summary>Only the items from one of these mods (identities, as <see cref="Item.Mod"/>); none for every item.</summary>
	public List<string> Mods = [];

	/// <summary>
	/// <c>true</c> when no filter is active: no requirement range, no type, rarity, origin or mod, and no "having" option.
	/// The level and attribute values alone do not count without their minimum or maximum check box.
	/// </summary>
	public bool IsEmpty
		=> !(this.MaxRequierement || this.MinRequierement
			|| this.HavingPrefix || this.HavingSuffix || this.HavingRelic || this.HavingCharm || this.IsSetItem || this.HavingSkill || this.Modded
			|| this.ClassItem.Count != 0 || this.Rarity.Count != 0 || this.Origin.Count != 0 || this.Mods.Count != 0);
}