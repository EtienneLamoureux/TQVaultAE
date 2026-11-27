using AwesomeAssertions;
using TQVaultAE.Application.Search;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Application;

/// <summary>
/// The rules of the item menu's <i>Search</i> (<see cref="ItemCopySearch"/>).
/// </summary>
public class ItemCopySearchTests
{
	private static Item Item(string baseName = "sword", string? prefix = "fiery", string? suffix = "ofkings", int seed = 1234
		, string? relic = null, string? bonus = null)
		=> new()
		{
			BaseItemId = RecordId.Create($"""records\item\{baseName}.dbr"""),
			prefixID = prefix is null ? RecordId.Empty : RecordId.Create($"""records\item\lootmagicalaffixes\prefix\{prefix}.dbr"""),
			suffixID = suffix is null ? RecordId.Empty : RecordId.Create($"""records\item\lootmagicalaffixes\suffix\{suffix}.dbr"""),
			Seed = seed,
			relicID = relic is null ? RecordId.Empty : RecordId.Create($"""records\item\relics\{relic}.dbr"""),
			RelicBonusId = bonus is null ? RecordId.Empty : RecordId.Create($"""records\item\relics\bonus\{bonus}.dbr"""),
		};

	private static bool Matches(Item source, Item other, CopySearchKind kind) => ItemCopySearch.Matcher(source, kind)(other);

	[Fact]
	public void Matcher_TheSamePieceWithOtherRelicsAndStack_MatchesEverySearch()
	{
		var source = Item(relic: "ruby", bonus: "fire");
		var copy = Item(relic: "emerald");
		copy.StackSize = 3;

		ItemCopySearch.Kinds.Should().OnlyContain(kind => Matches(source, copy, kind));
	}

	[Theory]
	[InlineData(CopySearchKind.SameSeed, false)]
	[InlineData(CopySearchKind.AllCopies, true)]
	[InlineData(CopySearchKind.SameBase, true)]
	public void Matcher_AnotherSeed_IsACopyButNotTheSamePiece(CopySearchKind kind, bool expected)
		=> Matches(Item(), Item(seed: 99), kind).Should().Be(expected);

	[Theory]
	[InlineData(CopySearchKind.SameSeed, false)]
	[InlineData(CopySearchKind.AllCopies, false)]
	[InlineData(CopySearchKind.SameBase, true)]
	[InlineData(CopySearchKind.SamePrefix, false)]
	[InlineData(CopySearchKind.SameSuffix, true)]
	public void Matcher_AnotherPrefix_OnlyTheBaseAndTheSuffixMatch(CopySearchKind kind, bool expected)
		=> Matches(Item(), Item(prefix: "icy"), kind).Should().Be(expected);

	[Theory]
	[InlineData(CopySearchKind.SameSeed, false)]
	[InlineData(CopySearchKind.AllCopies, false)]
	[InlineData(CopySearchKind.SameBase, true)]
	[InlineData(CopySearchKind.SamePrefix, true)]
	public void Matcher_ACopyWithoutTheSuffix_IsNotACopy(CopySearchKind kind, bool expected)
		=> Matches(Item(), Item(suffix: null), kind).Should().Be(expected);

	[Theory]
	[InlineData(CopySearchKind.SameBase, false)]
	[InlineData(CopySearchKind.SamePrefix, true)]
	[InlineData(CopySearchKind.SameSuffix, true)]
	public void Matcher_AnotherBaseWithTheSameAffixes_MatchesTheAffixSearches(CopySearchKind kind, bool expected)
		=> Matches(Item(), Item(baseName: "axe"), kind).Should().Be(expected);

	[Fact]
	public void Matcher_RecordIdsDifferingInCaseAndSeparators_AreTheSame()
	{
		var source = Item();
		var other = Item();
		other.BaseItemId = RecordId.Create("RECORDS/ITEM/SWORD.DBR");

		Matches(source, other, CopySearchKind.SameSeed).Should().BeTrue();
	}

	[Fact]
	public void Matcher_AnEmptySlotItem_NeverMatches()
		=> Matches(Item(), new Item(), CopySearchKind.SameBase).Should().BeFalse();

	[Theory]
	[InlineData(CopySearchKind.SamePrefix)]
	[InlineData(CopySearchKind.SameSuffix)]
	public void IsAvailable_AnAffixSearchOfAnItemWithoutIt_IsNotAndMatchesNothing(CopySearchKind kind)
	{
		var plain = Item(prefix: null, suffix: null);

		ItemCopySearch.IsAvailable(plain, kind).Should().BeFalse();
		// Two items without a prefix share no prefix: the search must not list every plain item.
		Matches(plain, Item(prefix: null, suffix: null), kind).Should().BeFalse();
	}

	[Fact]
	public void IsAvailable_ThePieceSearchesOfAPlainItem_Are()
	{
		var plain = Item(prefix: null, suffix: null);

		ItemCopySearch.IsAvailable(plain, CopySearchKind.SameSeed).Should().BeTrue();
		ItemCopySearch.IsAvailable(plain, CopySearchKind.AllCopies).Should().BeTrue();
		ItemCopySearch.IsAvailable(plain, CopySearchKind.SameBase).Should().BeTrue();
	}
}
