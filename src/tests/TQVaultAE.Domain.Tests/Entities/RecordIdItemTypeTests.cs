using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Entities;

/// <summary>
/// Tests for RecordId item type detection (IsRelic, IsCharm, IsPotion, etc.)
/// These test the path-based classification logic in RecordId.ForItems.cs
/// </summary>
public class RecordIdItemTypeTests
{
	#region IsRelic Tests

	[Theory]
	[InlineData("records\\items\\relics\\aegisofathena_01")]
	[InlineData("records/items/relics/aegisofathena_01")]
	public void IsRelic_TitanQuestBaseGameRelicPath_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsRelic;

		// Assert
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData("records\\XPACK\\relics\\aegisofathena_01")]
	[InlineData("records/XPACK4/relics/some_relic")]
	public void IsRelic_ExtensionRelicPath_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsRelic;

		// Assert
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData("records\\HCDUNGEON\\XPACK4\\03_X4_ESSENCEOFORDER_CHARM")]
	public void IsRelic_HCDungeonEERelic_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsRelic;

		// Assert
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData("records\\items\\armor\\helmet")]
	[InlineData("records\\items\\weapons\\sword")]
	[InlineData("records\\items\\quest\\somequestitem")]
	public void IsRelic_NonRelicPath_ReturnsFalse(string path)
	{
		// Act
		var result = RecordId.Create(path).IsRelic;

		// Assert
		result.Should().BeFalse();
	}

	[Theory]
	[InlineData("records\\XPACK\\charms\\some_charm")]
	public void IsRelic_CharmPath_ReturnsFalse(string path)
	{
		// Charms are not relics
		var result = RecordId.Create(path).IsRelic;

		result.Should().BeFalse();
	}

	#endregion

	#region IsCharm Tests

	[Theory]
	[InlineData("records\\items\\animalrelics\\testcharm_01")]
	[InlineData("records/items/animalrelics/testcharm_01")]
	public void IsCharm_TitanQuestBaseGameCharmPath_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsCharm;

		// Assert
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData("records\\XPACK\\charms\\golden_scarab")]
	public void IsCharm_HCDungeonEEGoldenScarab_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsCharm;

		// Assert
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData("records\\XPACK\\charms\\some_charm")]
	public void IsCharm_ExtensionCharmPath_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsCharm;

		// Assert
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData("records\\items\\relics\\aegisofathena_01")]
	[InlineData("records\\items\\armor\\helmet")]
	public void IsCharm_NonCharmPath_ReturnsFalse(string path)
	{
		// Act
		var result = RecordId.Create(path).IsCharm;

		// Assert
		result.Should().BeFalse();
	}

	#endregion

	#region IsPotion Tests

	[Theory]
	[InlineData("records\\items\\oneshot\\potion\\healthpotion")]
	[InlineData("records/items/oneshot/potion/healthpotion")]
	public void IsPotion_PotionPath_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsPotion;

		// Assert
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData("records\\items\\relics\\aegisofathena_01")]
	[InlineData("records\\items\\scrolls\\scrolloffire")]
	public void IsPotion_NonPotionPath_ReturnsFalse(string path)
	{
		// Act
		var result = RecordId.Create(path).IsPotion;

		// Assert
		result.Should().BeFalse();
	}

	#endregion

	#region IsQuestItem Tests

	[Theory]
	[InlineData("records\\quest\\testquestitem")]
	[InlineData("records/items/quest/some_quest")]
	public void IsQuestItem_TitanQuestQuestPath_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsQuestItem;

		// Assert
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData("records\\XPACK\\quests\\mainquest")]
	[InlineData("records/XPACK3/quests/some_quest")]
	public void IsQuestItem_ExtensionQuestPath_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsQuestItem;

		// Assert
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData("records\\items\\armor\\helmet")]
	[InlineData("records\\items\\relics\\aegisofathena")]
	public void IsQuestItem_NonQuestPath_ReturnsFalse(string path)
	{
		// Act
		var result = RecordId.Create(path).IsQuestItem;

		// Assert
		result.Should().BeFalse();
	}

	#endregion

	#region IsArtifact Tests

	[Theory]
	[InlineData("records\\items\\artifacts\\testartifact")]
	[InlineData("records/items/artifacts/some_artifact")]
	public void IsArtifact_ArtifactPath_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsArtifact;

		// Assert
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData("records\\items\\arcaneformulae\\testformula")]
	[InlineData("records\\items\\armor\\helmet")]
	[InlineData("records\\items\\relics\\aegisofathena")]
	public void IsArtifact_NonArtifactPath_ReturnsFalse(string path)
	{
		// Act
		var result = RecordId.Create(path).IsArtifact;

		// Assert
		result.Should().BeFalse();
	}

	[Theory]
	[InlineData("records\\items\\arcaneformulae\\testformula")]
	public void IsArtifact_FormulaePath_ReturnsFalse(string path)
	{
		// Formulae should NOT be classified as artifacts
		var result = RecordId.Create(path).IsArtifact;

		result.Should().BeFalse();
	}

	#endregion

	#region IsFormulae Tests

	[Theory]
	[InlineData("records\\items\\arcaneformulae\\testformula")]
	[InlineData("records/items/arcaneformulae/testformula")]
	public void IsFormulae_FormulaePath_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsFormulae;

		// Assert
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData("records\\items\\artifacts\\testartifact")]
	[InlineData("records\\items\\armor\\helmet")]
	public void IsFormulae_NonFormulaePath_ReturnsFalse(string path)
	{
		// Act
		var result = RecordId.Create(path).IsFormulae;

		// Assert
		result.Should().BeFalse();
	}

	#endregion

	#region IsParchment Tests

	[Theory]
	[InlineData("records\\items\\parchment\\testparchment")]
	[InlineData("records/items/parchment/some_scroll")]
	public void IsParchment_ParchmentPath_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsParchment;

		// Assert
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData("records\\items\\scrolls\\scrolloffire")]
	[InlineData("records\\items\\armor\\helmet")]
	public void IsParchment_NonParchmentPath_ReturnsFalse(string path)
	{
		// Act
		var result = RecordId.Create(path).IsParchment;

		// Assert
		result.Should().BeFalse();
	}

	#endregion

	#region IsScroll Tests

	[Theory]
	[InlineData("records\\items\\scrolls\\scrolloffire")]
	[InlineData("records/items/scrolls/some_scroll")]
	public void IsScroll_ScrollPath_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsScroll;

		// Assert
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData("records\\items\\parchment\\testparchment")]
	[InlineData("records\\items\\armor\\helmet")]
	public void IsScroll_NonScrollPath_ReturnsFalse(string path)
	{
		// Act
		var result = RecordId.Create(path).IsScroll;

		// Assert
		result.Should().BeFalse();
	}

	#endregion

	#region IsItem Tests

	[Theory]
	[InlineData("records\\items\\armor\\helmet")]
	[InlineData("records/items/weapons/sword")]
	[InlineData("records\\item\\singleitem")]
	public void IsItem_ItemPath_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsItem;

		// Assert
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData("records\\skills\\some_skill")]
	[InlineData("records\\quest\\some_quest")]
	public void IsItem_NonItemPath_ReturnsFalse(string path)
	{
		// Act
		var result = RecordId.Create(path).IsItem;

		// Assert
		result.Should().BeFalse();
	}

	#endregion

	#region IsHardCoreDungeonEE Tests

	[Theory]
	[InlineData("records\\HCDUNGEON\\XPACK4\\some_relic")]
	[InlineData("records/HCDUNGEON/some_item")]
	public void IsHardCoreDungeonEE_HCDungeonPath_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsHardCoreDungeonEE;

		// Assert
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData("records\\XPACK\\items\\relics")]
	[InlineData("records\\items\\armor\\helmet")]
	public void IsHardCoreDungeonEE_NonHCDungeonPath_ReturnsFalse(string path)
	{
		// Act
		var result = RecordId.Create(path).IsHardCoreDungeonEE;

		// Assert
		result.Should().BeFalse();
	}

	#endregion

	#region Equipment Weapon Tests

	[Theory]
	[InlineData("records\\XPACK\\equipmentweapon\\axe\\some_axe")]
	public void IsEquipmentWeapon_WeaponPath_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsEquipmentWeapon;

		// Assert
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData("records\\XPACK\\equipmentweapon\\sword\\blades")]
	public void IsEquipmentWeaponSword_SwordPath_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsEquipmentWeaponSword;

		// Assert
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData("records\\XPACK\\equipmentweapon\\axe\\some_axe")]
	public void IsEquipmentWeaponSword_AxePath_ReturnsFalse(string path)
	{
		// Act
		var result = RecordId.Create(path).IsEquipmentWeaponSword;

		// Assert
		result.Should().BeFalse();
	}

	[Theory]
	[InlineData("records\\XPACK\\equipmentweapon\\bow\\some_bow")]
	public void IsEquipmentWeaponBow_BowPath_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsEquipmentWeaponBow;

		// Assert
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData("records\\XPACK\\equipmentweapon\\staff\\some_staff")]
	public void IsEquipmentWeaponStaff_StaffPath_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsEquipmentWeaponStaff;

		// Assert
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData("records\\XPACK\\equipmentweapon\\spear\\some_spear")]
	public void IsEquipmentWeaponSpear_SpearPath_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsEquipmentWeaponSpear;

		// Assert
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData("records\\XPACK\\equipmentweapon\\1hranged\\some_thrown")]
	public void IsEquipmentWeaponThrown_ThrownPath_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsEquipmentWeaponThrown;

		// Assert
		result.Should().BeTrue();
	}

	#endregion

	#region Equipment Armor Tests

	[Theory]
	[InlineData("records\\XPACK\\equipmenthelm\\some_helm")]
	[InlineData("records\\XPACK\\equipmentarmor\\helm\\some_helm")]
	public void IsEquipmentHelm_HelmPath_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsEquipmentHelm;

		// Assert
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData("records\\XPACK\\equipmentring\\some_ring")]
	[InlineData("records\\XPACK\\equipmentarmor\\ring\\some_ring")]
	public void IsEquipmentRing_RingPath_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsEquipmentRing;

		// Assert
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData("records\\XPACK\\equipmentamulet\\some_amulet")]
	[InlineData("records\\XPACK\\equipmentarmor\\amulet\\some_amulet")]
	public void IsEquipmentAmulet_AmuletPath_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsEquipmentAmulet;

		// Assert
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData("records\\XPACK\\equipmentgreaves\\some_greaves")]
	[InlineData("records\\XPACK\\equipmentarmor\\greaves\\some_greaves")]
	public void IsEquipmentGreaves_GreavesPath_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsEquipmentGreaves;

		// Assert
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData("records\\XPACK\\equipmentarmor\\torso\\some_torso")]
	public void IsEquipmentTorso_TorsoPath_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsEquipmentTorso;

		// Assert
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData("records\\XPACK\\equipmentarmband\\some_armband")]
	[InlineData("records\\XPACK\\equipmentarmor\\armband\\some_armband")]
	public void IsEquipmentArmband_ArmbandPath_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsEquipmentArmband;

		// Assert
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData("records\\XPACK\\equipmentshield\\some_shield")]
	[InlineData("records\\XPACK\\equipmentweapon\\sword\\shield\\tower_shield")]
	public void IsEquipmentShield_ShieldPath_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsEquipmentShield;

		// Assert
		result.Should().BeTrue();
	}

	#endregion

	#region Empty RecordId Tests

	[Fact]
	public void IsRelic_EmptyRecord_ReturnsFalse()
	{
		// Act
		var result = RecordId.Empty.IsRelic;

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void IsCharm_EmptyRecord_ReturnsFalse()
	{
		// Act
		var result = RecordId.Empty.IsCharm;

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void IsPotion_EmptyRecord_ReturnsFalse()
	{
		// Act
		var result = RecordId.Empty.IsPotion;

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void IsQuestItem_EmptyRecord_ReturnsFalse()
	{
		// Act
		var result = RecordId.Empty.IsQuestItem;

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void IsArtifact_EmptyRecord_ReturnsFalse()
	{
		// Act
		var result = RecordId.Empty.IsArtifact;

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void IsFormulae_EmptyRecord_ReturnsFalse()
	{
		// Act
		var result = RecordId.Empty.IsFormulae;

		// Assert
		result.Should().BeFalse();
	}

	#endregion

	#region Case Insensitivity Tests

	[Theory]
	[InlineData("RECORDS\\ITEMS\\RELICS\\TEST")]
	[InlineData("records\\items\\relics\\test")]
	[InlineData("Records\\Items\\Relics\\Test")]
	public void IsRelic_CaseInsensitive_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsRelic;

		// Assert
		result.Should().BeTrue();
	}

	[Theory]
	[InlineData("RECORDS\\ITEMS\\ONESHOT\\POTION\\HEALTH")]
	[InlineData("records\\items\\oneshot\\potion\\health")]
	[InlineData("Records\\Items\\OneShot\\Potion\\Health")]
	public void IsPotion_CaseInsensitive_ReturnsTrue(string path)
	{
		// Act
		var result = RecordId.Create(path).IsPotion;

		// Assert
		result.Should().BeTrue();
	}

	#endregion
}
