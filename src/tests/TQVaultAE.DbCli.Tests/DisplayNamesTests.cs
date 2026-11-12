using AwesomeAssertions;
using TQVaultAE.DbCli.Core;

namespace TQVaultAE.Tests.DbCli;

public sealed class DisplayNamesTests
{
	private static ArzRecord Record(string cls, params (string Name, string Value)[] vars)
	{
		var info = new ArzRecordInfo(0, """records\x.dbr""", cls, 24, 1, DateTime.UnixEpoch);
		return new ArzRecord(info, [.. vars.Select(v => new ArzVariable(v.Name, ArzValueType.String, [v.Value]))]);
	}

	private sealed class FakeText(Dictionary<string, string> entries) : ITextDatabase
	{
		public string Language => "EN";

		public IReadOnlyDictionary<string, string> Entries => entries;

		public string? Resolve(string? tag) => tag is not null && entries.TryGetValue(tag, out var v) ? v : null;
	}

	private static readonly FakeText Text = new(new(StringComparer.OrdinalIgnoreCase) {
		["tagSword"] = "{^y}Hooked  Blade",
		["tagDesc"] = "Some description",
		["tagSkill"] = "Battle Rage",
		["tagSet"] = "Set of HeroB",
		["tagLoot"] = "of the Bear",
		["tagFr"] = "[fs]Lame crochue[ms]Autre",
	});

	[Theory]
	[InlineData("WeaponMelee_Sword")]
	[InlineData("ArmorProtective_Head")]
	[InlineData("WeaponArmor_Shield")]
	public void Equipment_UsesItemNameTag_EvenWhenDescriptionExists(string cls)
	{
		var r = Record(cls, ("description", "tagDesc"), ("itemNameTag", "tagSword"), ("itemQualityTag", "tagQ"));

		DisplayNames.NameTag(r).Should().Be("tagSword");
		DisplayNames.Resolve(r, Text).Should().Be("Hooked Blade");
	}

	[Theory]
	[InlineData("ItemRelic")]
	[InlineData("ItemCharm")]
	[InlineData("OneShot_Scroll")]
	[InlineData("Monster")]
	[InlineData("ItemArtifactFormula")]
	public void DescriptionClasses_UseDescription(string cls)
	{
		var r = Record(cls, ("skillDisplayName", "tagSkill"), ("description", "tagDesc"));

		DisplayNames.NameTag(r).Should().Be("tagDesc");
	}

	[Fact]
	public void Skills_UseSkillDisplayName()
	{
		var r = Record("Skill_AttackBuff", ("description", "tagDesc"), ("skillDisplayName", "tagSkill"));

		DisplayNames.Resolve(r, Text).Should().Be("Battle Rage");
	}

	[Fact]
	public void LootRandomizer_UsesLootRandomizerName()
	{
		var r = Record("LootRandomizer", ("description", "tagDesc"), ("lootRandomizerName", "tagLoot"));

		DisplayNames.Resolve(r, Text).Should().Be("of the Bear");
	}

	[Fact]
	public void EmptyClass_UsesSetName()
	{
		var r = Record(string.Empty, ("description", "tagDesc"), ("setName", "tagSet"));

		DisplayNames.Resolve(r, Text).Should().Be("Set of HeroB");
	}

	[Fact]
	public void UnknownClass_FallsBackInFieldOrder()
	{
		var r = Record("SomethingNew", ("setName", "tagSet"), ("skillDisplayName", "tagSkill"));

		DisplayNames.NameTag(r).Should().Be("tagSkill");
	}

	[Fact]
	public void KnownClassWithoutItsField_FallsBack()
	{
		var r = Record("WeaponMelee_Sword", ("itemNameTag", "  "), ("description", "tagDesc"));

		DisplayNames.NameTag(r).Should().Be("tagDesc");
	}

	[Theory]
	[InlineData("Old Man Leonidas", "Old Man Leonidas")]
	[InlineData("Telkines", "Telkines")]
	[InlineData("tagNpcUnknown", null)]
	[InlineData("x2tagNpcUnknown", null)]
	[InlineData("records/npc/thing.dbr", null)]
	public void Npc_UnresolvedLiteralDescription_IsReturned_UnresolvedTagIsNot(string description, string? expected)
	{
		var r = Record("NpcBasic", ("description", description));

		DisplayNames.Resolve(r, Text).Should().Be(expected);
	}

	[Fact]
	public void Resolve_KeepsFirstGenderVariant()
	{
		var r = Record("WeaponMelee_Sword", ("itemNameTag", "tagFr"));

		DisplayNames.Resolve(r, Text).Should().Be("Lame crochue");
	}

	[Theory]
	[InlineData("{^y}Legendary{^w} Sword", "Legendary Sword")]
	[InlineData("[ms]renforcé[mp]renforcés[fs]renforcée", "renforcé")]
	[InlineData("Plain", "Plain")]
	[InlineData("^gFrost Strike", "Frost Strike")]
	[InlineData("^yAthenian Soldier^n^wReady for battle.", "Athenian Soldier Ready for battle.")]
	[InlineData("Satyr{^n}Hunter", "Satyr Hunter")]
	public void Clean_StripsCodesAndMarkers(string raw, string expected)
		=> DisplayNames.Clean(raw).Should().Be(expected);

	[Fact]
	public void NoNameField_ReturnsNull()
	{
		var r = Record("WeaponMelee_Sword", ("Class", "WeaponMelee_Sword"));

		DisplayNames.NameTag(r).Should().BeNull();
		DisplayNames.Resolve(r, Text).Should().BeNull();
	}
}
