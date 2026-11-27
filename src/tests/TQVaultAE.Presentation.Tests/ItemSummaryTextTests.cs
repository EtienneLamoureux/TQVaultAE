using System.Globalization;
using AwesomeAssertions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;
using TQVaultAE.Presentation;

namespace TQVaultAE.Tests.Presentation;

public class ItemSummaryTextTests
{
	[Theory]
	[InlineData(SackType.Vault, null)]
	[InlineData(SackType.Player, null)]
	[InlineData(SackType.Equipment, null)]
	[InlineData(SackType.Stash, StashType.TransferStash)]
	[InlineData(SackType.Stash, StashType.RelicVaultStash)]
	[InlineData(SackType.Stash, StashType.PlayerStash)]
	[InlineData(SackType.Stash, null)]
	public void ContainerTypeLabel_MapsEachContainerToItsResource(SackType sackType, StashType? stashType)
	{
		var expected = (sackType, stashType) switch
		{
			(SackType.Vault, _) => Resources.ResultsContainerVault,
			(SackType.Player, _) => Resources.ResultsContainerPlayer,
			(SackType.Equipment, _) => Resources.ResultsContainerEquip,
			(SackType.Stash, StashType.TransferStash) => Resources.GlobalTransferStash,
			(SackType.Stash, StashType.RelicVaultStash) => Resources.GlobalRelicVaultStash,
			_ => Resources.ResultsContainerStash,
		};

		ItemSummaryText.ContainerTypeLabel(sackType, stashType).Should().Be(expected);
	}

	[Fact]
	public void ContainerTypeLabel_UnmappedSackType_ReturnsUnknown()
		=> ItemSummaryText.ContainerTypeLabel((SackType)99, null).Should().Be("Unknown");

	[Fact]
	public void BagLabel_WithNumber_FormatsTheBagResource()
		=> ItemSummaryText.BagLabel(3).Should().Be(string.Format(CultureInfo.CurrentCulture, Resources.ResultsBag, 3));

	[Fact]
	public void BagLabel_WithoutNumber_IsEmpty()
		=> ItemSummaryText.BagLabel(null).Should().BeEmpty();

	[Fact]
	public void ResultsBag_EnglishAndFrench_ShowTheNumber()
	{
		Resources.ResourceManager.GetString(nameof(Resources.ResultsBag), CultureInfo.InvariantCulture).Should().Be("Bag {0}");
		Resources.ResourceManager.GetString(nameof(Resources.ResultsBag), CultureInfo.GetCultureInfo("fr")).Should().Be("Sac {0}");
	}

	private static ToFriendlyNameResult Named(string description)
		=> new(new Item()) { BaseItemInfoDescription = description };

	[Fact]
	public void SummarizeByName_CountsIdenticalNames_SortedWithoutColorTags()
	{
		// Arrange: the color tag must not drive the order ("{^Y}Zeal" sorts after "Axe")
		var axe1 = Named("Axe");
		var zeal = Named("{^Y}Zeal");
		var axe2 = Named("Axe");

		// Act
		var lines = ItemSummaryText.SummarizeByName([zeal, axe1, axe2]);

		// Assert
		lines.Select(l => l.Text).Should().Equal("2 x Axe", "{^Y}1 x Zeal");
		lines[0].First.Should().BeSameAs(axe1);
		lines[1].First.Should().BeSameAs(zeal);
	}

	[Fact]
	public void SummarizeByName_NoItems_ReturnsEmpty()
		=> ItemSummaryText.SummarizeByName([]).Should().BeEmpty();
}
