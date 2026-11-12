using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Entities;

/// <summary>
/// <see cref="RelicAndCharmExtension.GetAllowedGearTypes"/>: the gear a relic or charm allows, from the slot flags of its record.
/// </summary>
public class RelicAndCharmExtensionTests
{
	private static Info Relic(params string[] slotFlags)
	{
		var record = new DBRecordCollection(RecordId.Create(@"records\item\relics\01_act1_aegisofathena.dbr"), "ItemRelic");
		foreach (var flag in slotFlags)
		{
			var variable = new Variable(flag, VariableDataType.Boolean, 1);
			variable[0] = 1;
			record.Set(variable);
		}
		return new Info(record);
	}

	[Fact]
	public void GetAllowedGearTypes_NoSlotFlag_ReturnsUndefined()
		=> Assert.Equal(GearType.Undefined, RelicAndCharmExtension.GetAllowedGearTypes(Relic()));

	[Fact]
	public void GetAllowedGearTypes_Shield_ReturnsShield()
		=> Assert.Equal(GearType.Shield, RelicAndCharmExtension.GetAllowedGearTypes(Relic("shield")));

	[Fact]
	public void GetAllowedGearTypes_AllArmorFlags_ReturnsAllArmor()
		=> Assert.Equal(GearType.AllArmor, RelicAndCharmExtension.GetAllowedGearTypes(Relic("helmet", "bodyArmor", "armband", "greaves")));

	[Fact]
	public void GetAllowedGearTypes_RingAndAmulet_ReturnsJewellery()
		=> Assert.Equal(GearType.Jewellery, RelicAndCharmExtension.GetAllowedGearTypes(Relic("ring", "amulet")));

	[Fact]
	public void GetAllowedGearTypes_Bow_AlsoAllowsThrownWeapons()
		=> Assert.Equal(GearType.Bow | GearType.Thrown, RelicAndCharmExtension.GetAllowedGearTypes(Relic("bow")));

	[Fact]
	public void GetAllowedGearTypes_Weapons_ReturnsEachWeapon()
		=> Assert.Equal(GearType.Spear | GearType.Staff | GearType.Sword | GearType.Mace | GearType.Axe,
			RelicAndCharmExtension.GetAllowedGearTypes(Relic("spear", "staff", "sword", "mace", "axe")));
}
