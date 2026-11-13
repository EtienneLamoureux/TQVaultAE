using AwesomeAssertions;
using TQVaultAE.DbCli.Query;

namespace TQVaultAE.Tests.DbCli;

public sealed class TextTagsTests
{
	[Theory]
	[InlineData("{^y}Legendary{^w} Sword", "Legendary Sword")]
	[InlineData("^gFrost Strike", "Frost Strike")]
	[InlineData("No codes", "No codes")]
	[InlineData("", "")]
	public void RemoveColorTags_RemovesBothForms(string raw, string expected)
		=> TextTags.RemoveColorTags(raw).Should().Be(expected);

	[Theory]
	[InlineData("Claw of a Peng.  {^n}{^y}Can enhance all weapons.", "Claw of a Peng.  \nCan enhance all weapons.")]
	[InlineData("Dear HeroA,{^N}{^N}A drought", "Dear HeroA,\n\nA drought")]
	[InlineData("^yAthenian Soldier^n^wReady for battle.", "Athenian Soldier\nReady for battle.")]
	public void ToPlainText_TurnsLineBreaksIntoNewLinesAndRemovesColors(string raw, string expected)
		=> TextTags.ToPlainText(raw).Should().Be(expected);

	[Fact]
	public void SplitLines_SplitsAtBothLineBreakForms_KeepingTheColors()
		=> TextTags.SplitLines("Deal damage around you.{^n}{^Y}Can trigger.^nLast{^N}{^N}After")
			.Should().Equal("Deal damage around you.", "{^Y}Can trigger.", "Last", "", "After");

	[Fact]
	public void SplitLines_TextWithoutBreak_IsOneLine()
		=> TextTags.SplitLines("No break").Should().Equal("No break");

	[Fact]
	public void ToPlainText_UsesTheGivenLineBreak()
		=> TextTags.ToPlainText("Satyr{^n}Hunter", " ").Should().Be("Satyr Hunter");

	private sealed class FakeText(Dictionary<string, string> entries) : ITextDatabase
	{
		public string Language => "EN";

		public IReadOnlyDictionary<string, string> Entries => entries;

		public string? Resolve(string? tag) => tag is not null && entries.TryGetValue(tag, out var v) ? v : null;
	}

	[Fact]
	public void ResolveTag_ReturnsPlainText()
	{
		var namer = new CoreRecordNamer(new FakeText(new() {
			["tagRelicDesc"] = "Blood of a Fury.  {^n}{^y}Can enhance all weapons.",
		}));

		namer.ResolveTag("tagRelicDesc").Should().Be("Blood of a Fury.  \nCan enhance all weapons.");
		namer.ResolveTag("tagUnknown").Should().BeNull();
		namer.ResolveTag(null).Should().BeNull();
	}
}
