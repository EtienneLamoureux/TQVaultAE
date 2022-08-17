using System.IO;

namespace TQVaultAE.Domain.Entities;

public partial class RecordId
{

	#region IsBroken

	bool? _IsBroken;
	/// <summary>
	/// This <see cref="RecordId"/> leads to Broken content.
	/// </summary>
	public bool IsBroken
	{
		get
		{
			if (_IsBroken is null)
				_IsBroken = this.Normalized.Contains(@"\BROKEN\");
			return _IsBroken.Value;
		}
	}

	#endregion

	#region IsSuffix

	bool? _IsSuffix;
	/// <summary>
	/// This <see cref="RecordId"/> leads to Suffix content.
	/// </summary>
	public bool IsSuffix
	{
		get
		{
			if (_IsSuffix is null)
				_IsSuffix = this.Normalized.Contains(@"\SUFFIX\");
			return _IsSuffix.Value;
		}
	}

	#endregion

	#region IsPrefix

	bool? _IsPrefix;
	/// <summary>
	/// This <see cref="RecordId"/> leads to Prefix content.
	/// </summary>
	public bool IsPrefix
	{
		get
		{
			if (_IsPrefix is null)
				_IsPrefix = this.Normalized.Contains(@"\PREFIX\");
			return _IsPrefix.Value;
		}
	}

	#endregion

	#region IsLootMagicalAffixes

	bool? _IsLootMagicalAffixes;
	/// <summary>
	/// This <see cref="RecordId"/> leads to LootMagicalAffixes content.
	/// </summary>
	public bool IsLootMagicalAffixes
	{
		get
		{
			if (_IsLootMagicalAffixes is null)
				_IsLootMagicalAffixes = this.Normalized.Contains(@"\LOOTMAGICALAFFIXES\");
			return _IsLootMagicalAffixes.Value;
		}
	}

	#endregion

	#region IsTablesWeapon

	bool? _IsTablesWeapon;
	/// <summary>
	/// This <see cref="RecordId"/> leads to TablesUnique content.
	/// </summary>
	public bool IsTablesWeapon
	{
		get
		{
			if (_IsTablesWeapon is null)
				_IsTablesWeapon = this.Normalized.Contains(@"\TABLESWEAPON") || Path.GetFileName(this.Normalized).StartsWith(@"TABLE_WEAPON");
			return _IsTablesWeapon.Value;
		}
	}

	#endregion

	#region IsTablesUnique

	bool? _IsTablesUnique;
	/// <summary>
	/// This <see cref="RecordId"/> leads to TablesUnique content.
	/// </summary>
	public bool IsTablesUnique
	{
		get
		{
			if (_IsTablesUnique is null)
				_IsTablesUnique = this.Normalized.Contains(@"\TABLESUNIQUE");
			return _IsTablesUnique.Value;
		}
	}

	#endregion

	#region IsTablesShields

	bool? _IsTablesShields;
	/// <summary>
	/// This <see cref="RecordId"/> leads to TablesJewelry content.
	/// </summary>
	public bool IsTablesShields
	{
		get
		{
			if (_IsTablesShields is null)
				_IsTablesShields = this.Normalized.Contains(@"\TABLESSHIELD") || Path.GetFileName(this.Normalized).StartsWith(@"TABLE_SHIELD");
			return _IsTablesShields.Value;
		}
	}

	#endregion

	#region IsTablesJewelry

	bool? _IsTablesJewelry;
	/// <summary>
	/// This <see cref="RecordId"/> leads to TablesJewelry content.
	/// </summary>
	public bool IsTablesJewelry
	{
		get
		{
			if (_IsTablesJewelry is null)
				_IsTablesJewelry = this.Normalized.Contains(@"\TABLESJEWELRY");
			return _IsTablesJewelry.Value;
		}
	}

	#endregion

	#region IsTablesArmor

	bool? _IsTablesArmor;
	/// <summary>
	/// This <see cref="RecordId"/> leads to TablesArmor content.
	/// </summary>
	public bool IsTablesArmor
	{
		get
		{
			if (_IsTablesArmor is null)
				_IsTablesArmor = this.Normalized.Contains(@"\TABLESARMOR") || Path.GetFileName(this.Normalized).StartsWith(@"TABLE_ARMOR");
			return _IsTablesArmor.Value;
		}
	}

	#endregion


	#region LootTableGearType

	GearType? _LootTableGearType;
	/// <summary>
	/// return GearType based on file naming rules for loot table <see cref="RecordId"/>.
	/// </summary>
	public GearType LootTableGearType
	{
		get
		{
			if (!IsLootMagicalAffixes) return GearType.Undefined;

			if (_LootTableGearType is null)
				_LootTableGearType = Path.GetFileName(this.Normalized) switch
				{
					var x when x.StartsWith(@"ARMSMAGE") | x.StartsWith(@"ARMMAGE") => GearType.Arm | GearType.ForMage,
					var x when x.StartsWith(@"ARMSMELEE") | x.StartsWith(@"ARMMELEE") => GearType.Arm | GearType.ForMelee,
					var x when x.StartsWith(@"HEADMAGE") => GearType.Head | GearType.ForMage,
					var x when x.StartsWith(@"HEADMELEE") => GearType.Head | GearType.ForMelee,
					var x when x.StartsWith(@"LEGSMAGE") | x.StartsWith(@"LEGMAGE") => GearType.Leg | GearType.ForMage,
					var x when x.StartsWith(@"LEGSMELEE") | x.StartsWith(@"LEGMELEE") => GearType.Leg | GearType.ForMelee,
					var x when x.StartsWith(@"TORSOMAGE") => GearType.Torso | GearType.ForMage,
					var x when x.StartsWith(@"TORSOMELEE") => GearType.Torso | GearType.ForMelee,
					var x when x.StartsWith(@"RING") => GearType.Ring,
					var x when x.StartsWith(@"AMULET") => GearType.Amulet,
					var x when x.StartsWith(@"SHIELD") => GearType.Shield,
					var x when x.StartsWith(@"AXE") => GearType.Axe,
					var x when x.StartsWith(@"BOW") => GearType.Bow,
					var x when x.StartsWith(@"CLUB") => GearType.Mace,
					var x when x.StartsWith(@"ROH") => GearType.Thrown,
					var x when x.StartsWith(@"SPEAR") => GearType.Spear,
					var x when x.StartsWith(@"STAFF") => GearType.Staff,
					var x when x.StartsWith(@"SWORD") => GearType.Sword,
					// For Broken Affixes
					var x when x.StartsWith(@"TABLE_ARMOR") => GearType.AllArmor,
					var x when x.StartsWith(@"TABLE_SHIELD") => GearType.Shield,
					var x when x.StartsWith(@"TABLE_WEAPONSCLUB") => GearType.Mace,
					var x when x.StartsWith(@"TABLE_WEAPONSMETAL") => GearType.Sword | GearType.Axe | GearType.Thrown,
					var x when x.StartsWith(@"TABLE_WEAPONSWOOD") => GearType.Spear | GearType.Staff | GearType.Bow,
					//RECORDS\XPACK4\ITEM\LOOTMAGICALAFFIXES\SUFFIX\TABLESARMOR\CHINAMONSTERSUFFIX_L05.DBR
					//RECORDS\XPACK4\ITEM\LOOTMAGICALAFFIXES\SUFFIX\TABLESARMOR\EGYPTMONSTERSUFFIX_L05.DBR
					var x when x.Contains(@"MONSTER") => GearType.MonsterInfrequent,
					_ => GearType.Undefined,
				};
			return _LootTableGearType.Value;
		}
	}

	#endregion
}
