using AwesomeAssertions;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Data;

public class SkillTreeProviderTests
{
	private const string Pane = """records\xpack3\ui\skills\mastery 4\panectrl.dbr""";
	private const string Mastery = """Records\Skills\Storm\StormMastery.dbr""";
	private const string IceShard = """Records\Skills\Storm\IceShard.dbr""";
	private const string Velocity = """Records\Skills\Storm\IceShard_Velocity.dbr""";
	private const string Torrent = """Records\Skills\Storm\IceShard_Torrent.dbr""";
	private const string EnergyShield = """Records\Skills\Storm\EnergyShield.dbr""";
	private const string EnergyShieldBuff = """Records\Skills\Storm\EnergyShieldBuff.dbr""";
	private const string Flash = """records\xpack3\skills\allmasteries\storm_flash.dbr""";

	private readonly Mock<IDatabase> _database = new();
	private readonly Dictionary<RecordId, DBRecordCollection> _records = [];

	public SkillTreeProviderTests()
	{
		this._database.Setup(d => d.GetRecordFromFile(It.IsAny<RecordId>()))
			.Returns((RecordId id) => this._records.GetValueOrDefault(id));
	}

	private void Add(string id, params (string Name, object Value)[] vars)
	{
		var record = new DBRecordCollection(id.ToRecordId(), "test");
		foreach (var (name, value) in vars)
		{
			object[] values = value is string[] strings ? strings : [value];
			var variable = new Variable(name, value is int ? VariableDataType.Integer : VariableDataType.StringVar, values.Length);
			for (int i = 0; i < values.Length; i++)
				variable[i] = values[i];
			record.Set(variable);
		}

		this._records[record.Id] = record;
	}

	private void AddButton(int index, string skill, int x, int y)
		=> this.Add($"""records\ui\button{index}.dbr""", ("skillName", skill), ("bitmapPositionX", x), ("bitmapPositionY", y));

	/// <summary>
	/// The Storm pane: Ice Shard (tier 1) with its modifiers Velocity and Torrent above it, Energy Shield (tier 4) that
	/// casts a buff, and Flash, an Atlantis skill on the top row.
	/// </summary>
	private void AddStormPane()
	{
		this.Add(Pane, ("tabSkillButtons", Enumerable.Range(0, 6).Select(i => $"""records\ui\button{i}.dbr""").ToArray()));
		this.AddButton(0, Mastery, 29, 459);
		this.AddButton(1, Torrent, 228, 155);
		this.AddButton(2, IceShard, 228, 403);
		this.AddButton(3, Velocity, 228, 279);
		this.AddButton(4, EnergyShield, 628, 217);
		this.AddButton(5, Flash, 528, 31);
		this.Add(Mastery, ("Class", "Skill_Mastery"), ("skillDisplayName", "tagSkillName018"), ("skillBaseDescription", "tagSkillDescription018"),
			("skillUpBitmapName", """InGameUI\Skills\StormMasteryBtnUp01.tex"""), ("skillMaxLevel", 40));
		this.Add(IceShard, ("Class", "Skill_AttackProjectileBurst"), ("skillDisplayName", "tagSkillName022"), ("skillMaxLevel", 12),
			("skillUpBitmapName", """InGameUI\Icons\Skills\Storm\IceShardUp01.tex"""));
		this.Add(Velocity, ("Class", "Skill_Modifier"), ("skillDisplayName", "tagSkillName023"), ("skillMaxLevel", 8));
		this.Add(Torrent, ("Class", "Skill_Modifier"), ("skillDisplayName", "tagSkillName024"), ("skillMaxLevel", 8));
		this.Add(EnergyShield, ("Class", "Skill_BuffOther"), ("buffSkillName", EnergyShieldBuff));
		this.Add(EnergyShieldBuff, ("Class", "SkillBuff_Passive"), ("skillDisplayName", "tagSkillName180"), ("skillMaxLevel", 12),
			("skillBaseDescription", "tagSkillDescription180"), ("skillUpBitmapName", """InGameUI\Icons\Skills\Storm\EnergyShieldUp01.tex"""));
		this.Add(Flash, ("Class", "Skill_WeaponPool_WarmUp"), ("skillDisplayName", "x3tagSkillStormFlash"), ("skillMaxLevel", 6));
	}

	[Fact]
	public void GetMasteryTree_StormPane_ReadsTheMasteryAndItsSkillsByTier()
	{
		this.AddStormPane();

		var tree = new SkillTreeProvider(this._database.Object).GetMasteryTree(Masteries.Storm)!;

		tree.Id.Should().Be(Mastery.ToRecordId());
		tree.NameTag.Should().Be("tagSkillName018");
		tree.MaxLevel.Should().Be(40);
		tree.Skills.Select(s => (s.Id.Raw, s.Tier, s.RequiredMasteryLevel)).Should().Equal(
			(IceShard, 1, 1),
			(Velocity, 3, 10),
			(EnergyShield, 4, 16),
			(Torrent, 5, 24),
			(Flash, 7, 40));
	}

	[Fact]
	public void GetMasteryTree_Modifiers_HangOnTheSkillUnderThemInTheirColumn()
	{
		this.AddStormPane();

		var skills = new SkillTreeProvider(this._database.Object).GetMasteryTree(Masteries.Storm)!.Skills.ToDictionary(s => s.Id.Raw);

		skills[Velocity].ParentId.Should().Be(IceShard.ToRecordId());
		skills[Torrent].ParentId.Should().Be(IceShard.ToRecordId(), "a modifier hangs on a skill, not on another modifier");
		skills[IceShard].ParentId.Should().BeNull();
		skills[Flash].ParentId.Should().BeNull();
	}

	[Fact]
	public void GetMasteryTree_SkillCastingABuff_TakesItsNameDescriptionIconAndMaxFromTheBuff()
	{
		this.AddStormPane();

		var shield = new SkillTreeProvider(this._database.Object).GetMasteryTree(Masteries.Storm)!.Skills.Single(s => s.Id.Raw == EnergyShield);

		shield.Should().BeEquivalentTo(new
		{
			NameTag = "tagSkillName180",
			DescriptionTag = "tagSkillDescription180",
			Icon = """InGameUI\Icons\Skills\Storm\EnergyShieldUp01.tex""",
			MaxLevel = 12,
		});
	}

	[Fact]
	public void GetMasteryTree_OnlyTheOlderPane_IsUsedWhenAtlantisIsMissing()
	{
		this.AddStormPane();
		this._records["""records\ingameui\player skills\mastery 4\panectrl.dbr""".ToRecordId()] = this._records[Pane.ToRecordId()];
		this._records.Remove(Pane.ToRecordId());

		var tree = new SkillTreeProvider(this._database.Object).GetMasteryTree(Masteries.Storm);

		tree.Should().NotBeNull();
		tree!.Skills.Should().HaveCount(5);
	}

	[Fact]
	public void GetMasteryTree_NoPane_ReturnsNull()
		=> new SkillTreeProvider(this._database.Object).GetMasteryTree(Masteries.Neidan).Should().BeNull();

	[Theory]
	[InlineData(403, 1)]
	[InlineData(341, 2)]
	[InlineData(93, 6)]
	[InlineData(31, 7)]
	[InlineData(500, 1)]
	public void FromButtonY_PaneRow_GivesTheTier(int y, int tier)
		=> SkillTiers.FromButtonY(y).Should().Be(tier);
}
