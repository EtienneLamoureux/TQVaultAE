using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Entities;

public class CharacterClassesTests
{
	[Theory]
	[InlineData(Masteries.Warfare | Masteries.Neidan, "x4tagNeidanWarfare")]
	[InlineData(Masteries.Warfare, "tagCClass08")]
	[InlineData(Masteries.Spirit | Masteries.Warfare, "tagCClass09")]
	[InlineData(Masteries.Dream | Masteries.Rune, "x2tag_class_dream_rm")]
	[InlineData(Masteries.Neidan, "x4tagNeidan")]
	public void TagOf_OneMasteryOrTwo_TheGamesClass(Masteries masteries, string expected)
		=> CharacterClasses.TagOf(masteries).Should().Be(expected);

	[Fact]
	public void TagOf_NoneOrThreeMasteries_NoClass()
	{
		CharacterClasses.TagOf((Masteries)0).Should().BeNull();
		CharacterClasses.TagOf(Masteries.Warfare | Masteries.Neidan | Masteries.Storm).Should().BeNull();
	}

	[Fact]
	public void TagOf_EveryMasteryAndPair_HasOneClass()
	{
		var all = Enum.GetValues<Masteries>();
		var pairs = all.SelectMany((a, i) => all.Skip(i + 1).Select(b => a | b));

		all.Concat(pairs).Select(CharacterClasses.TagOf).Should().OnlyHaveUniqueItems().And.NotContainNulls().And.HaveCount(66);
	}

	[Fact]
	public void MasteriesOf_InTheOrderTheGameNamesThem()
	{
		CharacterClasses.MasteriesOf("x4tagNeidanWarfare").Should().Equal(Masteries.Warfare, Masteries.Neidan);
		CharacterClasses.MasteriesOf("TAGCCLASS09").Should().Equal(Masteries.Spirit, Masteries.Warfare);
		CharacterClasses.MasteriesOf("tagCClass08").Should().Equal(Masteries.Warfare);
		CharacterClasses.MasteriesOf("unknown").Should().BeEmpty();
		CharacterClasses.MasteriesOf(null).Should().BeEmpty();
	}

	[Fact]
	public void BriefTag_EveryMastery()
	{
		CharacterClasses.BriefTag(Masteries.Warfare).Should().Be("tagMasteryBrief01");
		CharacterClasses.BriefTag(Masteries.Neidan).Should().Be("x4tagNeidan");
		Enum.GetValues<Masteries>().Select(CharacterClasses.BriefTag).Should().OnlyHaveUniqueItems();
	}
}
