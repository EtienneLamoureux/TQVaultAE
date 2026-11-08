using AwesomeAssertions;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

public class CharacterEditServiceTests
{
	private readonly Mock<IPathIO> _pathIO = new();
	private readonly Mock<IPlayerCollectionProvider> _provider = new();
	private readonly Mock<IDatabase> _database = new();
	private readonly CharacterEditService _service;

	public CharacterEditServiceTests()
	{
		this._pathIO.Setup(p => p.GetDirectoryName(It.IsAny<string>()))
			.Returns((string path) => path.Substring(0, path.LastIndexOf('\\')));
		this._service = new CharacterEditService(this._pathIO.Object, this._provider.Object, this._database.Object, new Mock<ISkillTreeProvider>().Object);
	}

	private PlayerInfo NewSource()
	{
		var source = new PlayerInfo(this._pathIO.Object)
		{
			CurrentLevel = 10,
			CurrentXP = 115652,
			Money = 1000,
			DifficultyUnlocked = 0,
			SkillPoints = 3,
			AttributesPoints = 2,
			BaseStrength = 62,
			BaseDexterity = 58,
			BaseIntelligence = 50,
			BaseHealth = 380,
			BaseMana = 300,
			Class = "tagCClass01",
		};
		source.SkillRecordList.AddRange(
		[
			new SkillRecord { skillName = """Records\Skills\Storm\StormMastery.dbr""", skillLevel = 4 },
			new SkillRecord { skillName = """Records\Skills\Storm\IceShard.dbr""", skillLevel = 6 },
		]);
		return source;
	}

	private static CharacterEditRequest RequestFrom(PlayerInfo source, string? xpText = null)
		=> new(
			Level: source.CurrentLevel
			, XpText: xpText ?? source.CurrentXP.ToString()
			, DifficultyUnlocked: null
			, SkillPoints: source.SkillPoints
			, AttributePoints: source.AttributesPoints
			, Strength: source.BaseStrength
			, Dexterity: source.BaseDexterity
			, Intelligence: source.BaseIntelligence
			, Health: source.BaseHealth
			, Mana: source.BaseMana);

	[Theory]
	[InlineData("")]
	[InlineData("abc")]
	[InlineData("99999999999")]
	[InlineData(null)]
	public void BuildPlayerInfo_XpTextNotANumber_KeepsTheExperience(string? xpText)
	{
		var source = this.NewSource();
		var request = RequestFrom(source) with { XpText = xpText };

		var result = this._service.BuildPlayerInfo(source, request);

		result.CurrentXP.Should().Be(source.CurrentXP);
	}

	[Fact]
	public void BuildPlayerInfo_XpText_IsParsed()
	{
		var source = this.NewSource();

		var result = this._service.BuildPlayerInfo(source, RequestFrom(source, "157642"));

		result.CurrentXP.Should().Be(157642);
	}

	[Theory]
	[InlineData(1, 0)]
	[InlineData(5, 1)]
	[InlineData(20, 2)]
	public void BuildPlayerInfo_MasteriesAllowed_FollowTheLevel(int level, int expected)
	{
		var source = this.NewSource();

		var result = this._service.BuildPlayerInfo(source, RequestFrom(source) with { Level = level });

		result.CurrentLevel.Should().Be(level);
		result.MasteriesAllowed.Should().Be(expected);
	}

	[Fact]
	public void BuildPlayerInfo_ResetAttributes_PutsThemBackToTheMinimumAndRefundsTheSourcePoints()
	{
		var source = this.NewSource();
		// The dialog values are ignored by the reset: it starts from the character.
		var request = RequestFrom(source) with { AttributePoints = 0, Strength = 70, ResetAttributes = true };

		var result = this._service.BuildPlayerInfo(source, request);

		result.BaseStrength.Should().Be(PlayerLevel.MinStrength);
		result.BaseDexterity.Should().Be(PlayerLevel.MinDexterity);
		result.BaseIntelligence.Should().Be(PlayerLevel.MinIntelligence);
		result.BaseHealth.Should().Be(PlayerLevel.MinHealth);
		result.BaseMana.Should().Be(PlayerLevel.MinMana);
		// 2 in the pool + 3 strength + 2 dexterity + 2 health.
		result.AttributesPoints.Should().Be(9);
	}

	[Fact]
	public void BuildPlayerInfo_ResetMasteries_RemovesTheirSkillsAndGivesBackTheSkillPoints()
	{
		var source = this.NewSource();

		var result = this._service.BuildPlayerInfo(source, RequestFrom(source) with { ResetMasteries = true });

		result.SkillRecordList.Should().BeEmpty();
		result.SkillPoints.Should().Be(3 + 4 + 6);
		result.Class.Should().BeEmpty();
		source.SkillRecordList.Should().HaveCount(2);
	}

	[Fact]
	public void BuildPlayerInfo_NoReset_KeepsSkillsAndDialogValues()
	{
		var source = this.NewSource();
		var request = RequestFrom(source) with { SkillPoints = 9, AttributePoints = 1, Strength = 66 };

		var result = this._service.BuildPlayerInfo(source, request);

		result.SkillRecordList.Should().HaveCount(2);
		result.SkillPoints.Should().Be(9);
		result.AttributesPoints.Should().Be(1);
		result.BaseStrength.Should().Be(66);
	}

	[Fact]
	public void BuildPlayerInfo_DifficultyUnlocked_RaisesTheMoney()
	{
		var source = this.NewSource();

		var result = this._service.BuildPlayerInfo(source, RequestFrom(source) with { DifficultyUnlocked = 2 });

		result.DifficultyUnlocked.Should().Be(2);
		result.Money.Should().Be(PlayerLevel.LegendaryMoneyFloor);
	}

	[Fact]
	public void BuildPlayerInfo_NoDifficulty_KeepsTheCharacterDifficultyAndMoney()
	{
		var source = this.NewSource();
		source.DifficultyUnlocked = 1;

		var result = this._service.BuildPlayerInfo(source, RequestFrom(source));

		result.DifficultyUnlocked.Should().Be(1);
		result.Money.Should().Be(1000);
	}

	[Fact]
	public void BuildPlayerInfo_NegativeMoney_BecomesZero()
	{
		var source = this.NewSource();
		source.Money = -5;

		var result = this._service.BuildPlayerInfo(source, RequestFrom(source));

		result.Money.Should().Be(0);
	}

	[Fact]
	public void Apply_CommitsTheBuiltPlayerInfo()
	{
		var source = this.NewSource();
		var player = new PlayerCollection("Hero", "player.chr") { PlayerInfo = source };
		PlayerInfo? committed = null;
		this._provider.Setup(p => p.CommitPlayerInfo(player, It.IsAny<PlayerInfo>()))
			.Callback((PlayerCollection _, PlayerInfo info) => committed = info);

		var applied = this._service.Apply(player, RequestFrom(source) with { Level = 12, XpText = "bad" });

		applied.Should().BeTrue();
		committed.Should().NotBeNull();
		committed!.CurrentLevel.Should().Be(12);
		committed.CurrentXP.Should().Be(source.CurrentXP);
	}

	[Fact]
	public void Apply_WithoutPlayerInfo_CommitsNothing()
	{
		var player = new PlayerCollection("Hero", "player.chr");

		var applied = this._service.Apply(player, RequestFrom(this.NewSource()));

		applied.Should().BeFalse();
		this._provider.Verify(p => p.CommitPlayerInfo(It.IsAny<PlayerCollection>(), It.IsAny<PlayerInfo>()), Times.Never);
	}

	[Fact]
	public void BuildPlayerInfo_EditedSkills_ReplaceTheSkillListAndKeepTheClass()
	{
		var source = this.NewSource();
		List<SkillRecord> edited = [new SkillRecord { skillName = """Records\Skills\Storm\StormMastery.dbr""", skillLevel = 8 }];

		var result = this._service.BuildPlayerInfo(source, RequestFrom(source) with { SkillRecords = edited });

		result.SkillRecordList.Should().Equal(edited);
		result.SkillsChanged.Should().BeTrue();
		result.MasteriesChanged.Should().BeFalse();
		result.Class.Should().Be("tagCClass01");
	}

	[Fact]
	public void BuildPlayerInfo_EditedMasteries_ClearTheClass()
	{
		var source = this.NewSource();

		var result = this._service.BuildPlayerInfo(source, RequestFrom(source) with { SkillRecords = [], MasteriesChanged = true });

		result.MasteriesChanged.Should().BeTrue();
		result.Class.Should().BeEmpty();
	}

	[Fact]
	public void BuildPlayerInfo_ResetMasteriesWithEditedSkills_ResetsTheSavedSkills()
	{
		var source = this.NewSource();
		List<SkillRecord> edited = [new SkillRecord { skillName = """Records\Skills\Storm\StormMastery.dbr""", skillLevel = 8 }];

		var result = this._service.BuildPlayerInfo(source, RequestFrom(source) with { SkillRecords = edited, ResetMasteries = true });

		result.SkillRecordList.Should().BeEmpty();
		result.SkillPoints.Should().Be(3 + 4 + 6, "the reset gives back the points of the saved skills");
		result.SkillsChanged.Should().BeFalse();
	}
}
