namespace TQVaultAE.Domain.Entities;

/// <summary>
/// Gear a relic or charm allows, read from the slot flags of its record.
/// </summary>
public static class RelicAndCharmExtension
{
	/// <summary>Slot flag of a relic or charm record, and the gear it allows.</summary>
	private static readonly (string Flag, GearType Type)[] SlotFlags =
	[
		("helmet", GearType.Head),
		("bodyArmor", GearType.Torso),
		("armband", GearType.Arm),
		("greaves", GearType.Leg),
		("ring", GearType.Ring),
		("amulet", GearType.Amulet),
		("spear", GearType.Spear),
		("staff", GearType.Staff),
		// Relics have no thrown flag: the ones made for thrown weapons set bow.
		("bow", GearType.Bow | GearType.Thrown),
		("sword", GearType.Sword),
		("mace", GearType.Mace),
		("axe", GearType.Axe),
		("shield", GearType.Shield),
	];

	/// <summary>
	/// Gear allowed by the slot flags of a relic or charm record, base game or mod alike.
	/// </summary>
	public static GearType GetAllowedGearTypes(Info relicInfo)
	{
		var types = GearType.Undefined;
		foreach (var (flag, type) in SlotFlags)
		{
			if (relicInfo.GetInt32(flag) != 0)
				types |= type;
		}
		return types;
	}
}
