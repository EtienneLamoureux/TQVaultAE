using AwesomeAssertions;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Tests.Shared;

namespace TQVaultAE.Tests.Application;

public class PlayerInfoTests
{
	private const string Healer = """Records\Skills\ModMasteries\HealerMastery.dbr""";
	private const string Witch = """Records\Skills\ModMasteries\WitchMastery.dbr""";

	private readonly PlayerInfo _playerInfo = new() { MasteryTrees = TestMasteries.BaseGame };

	#region Class

	[Fact]
	public void ClassTag_TheSavesClass_ElseTheOneTheMasteriesMake()
	{
		_playerInfo.SkillRecordList.Add(new SkillRecord { skillName = """Records\Skills\Warfare\WarfareMastery.dbr""", skillLevel = 40 });
		_playerInfo.SkillRecordList.Add(new SkillRecord { skillName = """Records\Xpack4\Skills\Neidan\NeidanMastery.dbr""", skillLevel = 40 });

		_playerInfo.Class = "tagCClass08";
		_playerInfo.ClassTag.Should().Be("tagCClass08");

		_playerInfo.Class = string.Empty;
		_playerInfo.ClassTag.Should().Be("x4tagNeidanWarfare", "a change of masteries clears the save's class until the game sets it again");
	}

	[Fact]
	public void ClassTag_ClearedOnAModdedCharacter_TheOneItsMasteryTreesMake()
	{
		// A mod's masteries have their own records: the character's trees, read with its map, tell them.
		_playerInfo.SkillRecordList.Add(new SkillRecord { skillName = Healer, skillLevel = 32 });
		_playerInfo.SkillRecordList.Add(new SkillRecord { skillName = Witch, skillLevel = 32 });
		_playerInfo.Class = string.Empty;
		_playerInfo.ClassTag.Should().BeEmpty("the base game's trees know no such mastery");

		_playerInfo.MasteryTrees = [TestMasteries.ModTree(Masteries.Spirit, Witch), TestMasteries.ModTree(Masteries.Nature, Healer)];

		_playerInfo.ActiveMasteries.Should().Be(Masteries.Spirit | Masteries.Nature);
		_playerInfo.ClassTag.Should().Be("tagCClass36");
	}

	[Fact]
	public void ClassTag_NoMasteryTrees_TheSaves()
	{
		var info = new PlayerInfo { Class = "tagCClass01" };
		info.SkillRecordList.Add(new SkillRecord { skillName = TestMasteries.BaseGameRecords[Masteries.Warfare], skillLevel = 40 });

		info.ActiveMasteries.Should().BeNull("no game data read: no mastery known");
		info.ClassTag.Should().Be("tagCClass01");
	}

	#endregion

	#region Stat Calculation Tests

	/// <summary>The sums of the statistics panel with the given variables.</summary>
	private static AttributeSum Bonus(params (string Name, double Value)[] values)
	{
		var sum = new AttributeSum();
		foreach (var (name, value) in values)
			sum.Add(name, value);
		return sum;
	}


	[Fact]
	public void CalculatedStrength_WithNoBonuses_ReturnsBaseStrength()
	{
		// Arrange
		_playerInfo.BaseStrength = 50;
		_playerInfo.SkillBonus = null;
		_playerInfo.GearBonus = null;

		// Act
		var result = _playerInfo.CalculatedStrength;

		// Assert
		result.Should().Be(50);
	}

	[Fact]
	public void CalculatedStrength_WithSkillBonus_IncludesBonus()
	{
		// Arrange
		_playerInfo.BaseStrength = 50;
		_playerInfo.SkillBonus = Bonus(("characterStrength", 10));
		_playerInfo.GearBonus = null;

		// Act
		var result = _playerInfo.CalculatedStrength;

		// Assert
		result.Should().Be(60);
	}

	[Fact]
	public void CalculatedStrength_WithGearBonus_IncludesBonus()
	{
		// Arrange
		_playerInfo.BaseStrength = 50;
		_playerInfo.SkillBonus = null;
		_playerInfo.GearBonus = Bonus(("characterStrength", 15));

		// Act
		var result = _playerInfo.CalculatedStrength;

		// Assert
		result.Should().Be(65);
	}

	[Fact]
	public void CalculatedStrength_WithBothBonuses_IncludesBoth()
	{
		// Arrange
		_playerInfo.BaseStrength = 50;
		_playerInfo.SkillBonus = Bonus(("characterStrength", 10));
		_playerInfo.GearBonus = Bonus(("characterStrength", 15));

		// Act
		var result = _playerInfo.CalculatedStrength;

		// Assert
		result.Should().Be(75);
	}

	[Fact]
	public void CalculatedStrength_WithModifier_AppliesPercentage()
	{
		// Arrange
		_playerInfo.BaseStrength = 100;
		_playerInfo.SkillBonus = null;
		_playerInfo.GearBonus = Bonus(("characterStrengthModifier", 50)); // +50% modifier

		// Act
		var result = _playerInfo.CalculatedStrength;

		// Assert
		// (100 + 0 + 0) * (100 + 0 + 50) / 100 = 100 * 1.5 = 150
		result.Should().Be(150);
	}

	[Fact]
	public void CalculatedStrength_WithNegativeModifier_AppliesReduction()
	{
		// Arrange
		_playerInfo.BaseStrength = 100;
		_playerInfo.SkillBonus = null;
		_playerInfo.GearBonus = Bonus(("characterStrengthModifier", -20)); // -20% modifier

		// Act
		var result = _playerInfo.CalculatedStrength;

		// Assert
		// (100 + 0 + 0) * (100 + 0 - 20) / 100 = 100 * 0.8 = 80
		result.Should().Be(80);
	}

	[Fact]
	public void CalculatedStrength_UnroundedBonuses_AreTruncatedAsTheGameShows()
	{
		// A mastery's 13.5 strength on 58 shows 71 in the game, with 0.6 of an item 72.
		_playerInfo.BaseStrength = 58;
		_playerInfo.SkillBonus = Bonus(("characterStrength", 13.5));
		_playerInfo.GearBonus = null;

		_playerInfo.CalculatedStrength.Should().Be(71);

		_playerInfo.GearBonus = Bonus(("characterStrength", 0.6));

		_playerInfo.CalculatedStrength.Should().Be(72);
	}

	[Fact]
	public void CalculatedDexterity_WithNoBonuses_ReturnsBaseDexterity()
	{
		// Arrange
		_playerInfo.BaseDexterity = 75;
		_playerInfo.SkillBonus = null;
		_playerInfo.GearBonus = null;

		// Act
		var result = _playerInfo.CalculatedDexterity;

		// Assert
		result.Should().Be(75);
	}

	[Fact]
	public void CalculatedIntelligence_WithNoBonuses_ReturnsBaseIntelligence()
	{
		// Arrange
		_playerInfo.BaseIntelligence = 40;
		_playerInfo.SkillBonus = null;
		_playerInfo.GearBonus = null;

		// Act
		var result = _playerInfo.CalculatedIntelligence;

		// Assert
		result.Should().Be(40);
	}

	[Fact]
	public void CalculatedHealth_WithNoBonuses_ReturnsBaseHealth()
	{
		// Arrange
		_playerInfo.BaseHealth = 500;
		_playerInfo.SkillBonus = null;
		_playerInfo.GearBonus = null;

		// Act
		var result = _playerInfo.CalculatedHealth;

		// Assert
		result.Should().Be(500);
	}

	[Fact]
	public void CalculatedMana_WithNoBonuses_ReturnsBaseMana()
	{
		// Arrange
		_playerInfo.BaseMana = 200;
		_playerInfo.SkillBonus = null;
		_playerInfo.GearBonus = null;

		// Act
		var result = _playerInfo.CalculatedMana;

		// Assert
		result.Should().Be(200);
	}

	[Fact]
	public void CalculatedHealth_WithBothBonuses_CalculatesCorrectly()
	{
		// Arrange
		_playerInfo.BaseHealth = 100;
		_playerInfo.SkillBonus = Bonus(("characterLife", 50), ("characterLifeModifier", 25));
		_playerInfo.GearBonus = Bonus(("characterLife", 30));

		// Act
		var result = _playerInfo.CalculatedHealth;

		// Assert
		// (100 + 50 + 30) * (100 + 25 + 0) / 100 = 180 * 1.25 = 225
		result.Should().Be(225);
	}

	[Fact]
	public void CalculatedMana_WithBothBonuses_CalculatesCorrectly()
	{
		// Arrange
		_playerInfo.BaseMana = 100;
		_playerInfo.SkillBonus = Bonus(("characterMana", 20), ("characterManaModifier", 50));
		_playerInfo.GearBonus = Bonus(("characterManaModifier", 10));

		// Act
		var result = _playerInfo.CalculatedMana;

		// Assert
		// (100 + 20 + 0) * (100 + 50 + 10) / 100 = 120 * 1.6 = 192
		result.Should().Be(192);
	}

	#endregion

	#region MustResetMasteries Tests

	[Fact]
	public void MustResetMasteries_WhenMasteriesAllowedDecreased_ReturnsTrue()
	{
		// Arrange - Set to 5 first (captured as OldValue), then set to 3
		_playerInfo.MasteriesAllowed = 5;
		_playerInfo.MasteriesAllowed = 3;
		_playerInfo.MasteriesResetRequiered = false;

		// Act
		var result = _playerInfo.MustResetMasteries;

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void MustResetMasteries_WhenMasteriesResetRequiredFlagSet_ReturnsTrue()
	{
		// Arrange - OldValue is null by default, so first condition is false
		_playerInfo.MasteriesAllowed = 3;
		_playerInfo.MasteriesResetRequiered = true;

		// Act
		var result = _playerInfo.MustResetMasteries;

		// Assert
		result.Should().BeTrue();
	}

	[Fact]
	public void MustResetMasteries_WhenNoConditionsMet_ReturnsFalse()
	{
		// Arrange - First assignment captures OldValue=3
		_playerInfo.MasteriesAllowed = 3;
		// Second assignment (same value) keeps OldValue=3
		_playerInfo.MasteriesAllowed = 3;
		_playerInfo.MasteriesResetRequiered = false;

		// Act
		var result = _playerInfo.MustResetMasteries;

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void MustResetMasteries_WhenOldValueNotSet_ReturnsFalse()
	{
		// Arrange - OldValue is null by default, so first condition is false
		_playerInfo.MasteriesAllowed = 3;
		_playerInfo.MasteriesResetRequiered = false;

		// Act
		var result = _playerInfo.MustResetMasteries;

		// Assert
		result.Should().BeFalse();
	}

	#endregion

	#region ResetMasteries Tests

	[Fact]
	public void ResetMasteries_WithNoActiveMasteries_ReturnsFalse()
	{
		// Arrange - no skills in list
		_playerInfo.SkillRecordList.Clear();

		// Act
		var result = _playerInfo.ResetMasteries();

		// Assert
		result.Should().BeFalse();
	}

	[Fact]
	public void ResetMasteries_WithActiveMasteries_ReturnsTrueAndMarksPlayerAsModified()
	{
		_playerInfo.Modified = false;
		_playerInfo.SkillRecordList.Add(new SkillRecord { skillName = """Records\Skills\Defensive\DefensiveMastery.dbr""", skillLevel = 5 });

		_playerInfo.ResetMasteries().Should().BeTrue();

		_playerInfo.Modified.Should().BeTrue();
		_playerInfo.SkillRecordList.Should().BeEmpty();
	}

	[Fact]
	public void ResetMasteries_RemovesTheMasteriesAndTheSkillsOfTheirPanes_WhereverTheirRecordsAre()
	{
		// Atlantis put skills of the first masteries in its own folder: the pane tells they belong to the mastery.
		const string iceShard = """Records\Skills\Storm\IceShard.dbr""";
		const string atlantisSkill = """records\xpack3\skills\allmasteries\storm_flash.dbr""";
		const string questSkill = """Records\Quests\Rewards\QuestSkill.dbr""";
		_playerInfo.MasteryTrees = [TestMasteries.Tree(Masteries.Storm, iceShard, atlantisSkill), TestMasteries.Tree(Masteries.Earth)];
		_playerInfo.SkillRecordList.Add(new SkillRecord { skillName = TestMasteries.BaseGameRecords[Masteries.Storm], skillLevel = 3 });
		_playerInfo.SkillRecordList.Add(new SkillRecord { skillName = iceShard, skillLevel = 5 });
		_playerInfo.SkillRecordList.Add(new SkillRecord { skillName = atlantisSkill, skillLevel = 2 });
		_playerInfo.SkillRecordList.Add(new SkillRecord { skillName = questSkill, skillLevel = 1 });

		_playerInfo.ResetMasteries();

		_playerInfo.SkillRecordList.Should().ContainSingle().Which.skillName.Should().Be(questSkill);
		_playerInfo.ReleasedSkillPoints.Should().Be(3 + 5 + 2);
	}

	[Fact]
	public void ResetMasteries_ModMasteryWithItsOwnRecords_RemovesItsSkills()
	{
		const string heal = """Records\Skills\ModMasteries\Heal.dbr""";
		_playerInfo.MasteryTrees = [TestMasteries.ModTree(Masteries.Nature, Healer, heal), TestMasteries.ModTree(Masteries.Spirit, Witch)];
		_playerInfo.SkillRecordList.Add(new SkillRecord { skillName = Healer, skillLevel = 4 });
		_playerInfo.SkillRecordList.Add(new SkillRecord { skillName = heal, skillLevel = 6 });

		_playerInfo.ResetMasteries().Should().BeTrue();

		_playerInfo.SkillRecordList.Should().BeEmpty();
		_playerInfo.ReleasedSkillPoints.Should().Be(10);
	}

	#endregion

	#region ReleasedSkillPoints Tests

	[Fact]
	public void ReleasedSkillPoints_WithNoRemovedSkills_ReturnsZero()
	{
		// Arrange - no removed skills
		_playerInfo.SkillRecordList.Clear();

		// Act
		var result = _playerInfo.ReleasedSkillPoints;

		// Assert
		result.Should().Be(0);
	}

	#endregion

	#region Mastery Detection Tests

	[Fact]
	public void ActiveMasteries_WhenMultipleMasteriesEnabled_ReturnsCombinedValue()
	{
		_playerInfo.SkillRecordList.Add(new SkillRecord { skillName = """Records\Skills\Defensive\DefensiveMastery.dbr""", skillLevel = 1 });
		_playerInfo.SkillRecordList.Add(new SkillRecord { skillName = """Records\Skills\Storm\StormMastery.dbr""", skillLevel = 1 });

		_playerInfo.ActiveMasteries.Should().Be(Masteries.Defensive | Masteries.Storm);
	}

	[Fact]
	public void ActiveMasteries_WhenNoMasteriesEnabled_ReturnsNull()
	{
		_playerInfo.SkillRecordList.Clear();

		_playerInfo.ActiveMasteries.Should().BeNull();
	}

	[Fact]
	public void ActiveMasteryTrees_TheTreesWhoseMasteryIsInTheSkills_AnyCasing()
	{
		_playerInfo.SkillRecordList.Add(new SkillRecord { skillName = """RECORDS\SKILLS\DEFENSIVE\DEFENSIVEMASTERY.DBR""", skillLevel = 1 });

		_playerInfo.ActiveMasteryTrees.Should().ContainSingle().Which.Mastery.Should().Be(Masteries.Defensive);
	}

	#endregion

	#region MasteriesAllowed Tests

	[Fact]
	public void MasteriesAllowed_InitialValue_SetsOldValueToNull()
	{
		// Arrange & Act - Create fresh PlayerInfo
		var freshPlayer = new PlayerInfo();

		// Assert - OldValue should be null initially (has private set)
		// We can't directly access OldValue, but we can test behavior via MustResetMasteries
		freshPlayer.MasteriesAllowed = 5;
		freshPlayer.MasteriesAllowed = 3;

		// MustResetMasteries should be true because OldValue (5) > Current (3)
		freshPlayer.MustResetMasteries.Should().BeTrue();
	}

	[Fact]
	public void MasteriesAllowed_OldValueNotOverwritten_AfterFirstSet()
	{
		// Arrange - Create fresh PlayerInfo
		var freshPlayer = new PlayerInfo();

		// Act - Set to 5, then 3, then 4
		freshPlayer.MasteriesAllowed = 5;
		freshPlayer.MasteriesAllowed = 3;
		freshPlayer.MasteriesAllowed = 4;

		// Assert - OldValue captured on first set should still be 5
		// Setting to 2 (less than 5) should trigger reset
		freshPlayer.MasteriesAllowed = 2;
		freshPlayer.MustResetMasteries.Should().BeTrue();
	}

	#endregion
}
