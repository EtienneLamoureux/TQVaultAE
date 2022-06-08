using System;

namespace TQVaultAE.Domain.Entities
{
	/// <summary>
	/// Gear classification
	/// </summary>
	[Flags]
	public enum GearType
	{
		Undefined = 0,
		Head = 1 << 0,
		Torso = 1 << 1,
		Arm = 1 << 2,
		Leg = 1 << 3,
		Ring = 1 << 4,
		Amulet = 1 << 5,
		Artifact = 1 << 6,
		Spear = 1 << 7,
		Staff = 1 << 8,
		Thrown = 1 << 9,
		Bow = 1 << 10,
		Sword = 1 << 11,
		Mace = 1 << 12,
		Axe = 1 << 13,
		Shield = 1 << 14,
		Jewellery = Ring | Amulet,
		AllArmor = Head | Torso | Arm | Leg,
		AllWeapons = Spear | Staff | Thrown | Bow | Sword | Mace | Axe,
	}
}
