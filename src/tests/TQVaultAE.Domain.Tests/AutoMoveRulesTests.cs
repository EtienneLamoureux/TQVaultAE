using AwesomeAssertions;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Domain;

/// <summary>
/// <see cref="ItemActionRules.MoveDestinations"/> and <see cref="AutoMovePlanner"/>: the "Move to" menu, the quick move
/// keys and the bag merge.
/// </summary>
public class AutoMoveRulesTests
{
	private static readonly AutoMoveLocation[] AllPanels =
	[
		AutoMoveLocation.Player, AutoMoveLocation.Vault, AutoMoveLocation.SecondaryVault
		, AutoMoveLocation.Stash, AutoMoveLocation.Trash, AutoMoveLocation.Vault,
	];

	private static Item ItemAt(SackType sackType, int sackNumber, string id = "records/item/equipmentring/ring01.dbr")
		=> new()
		{
			BaseItemId = RecordId.Create(id),
			Place = new ContainerPlace { SackType = sackType, SackNumber = sackNumber },
		};

	private static Item Sized(int width, int height) => new()
	{
		BaseItemId = RecordId.Create($"records/item/{width}x{height}.dbr"),
		Width = width,
		Height = height,
	};

	private static IReadOnlyList<MoveDestination> Destinations(
		AutoMoveLocation here = AutoMoveLocation.Vault
		, SackType sackType = SackType.Vault
		, int maxSacks = 1
		, int playerSackCount = 0
		, bool secondaryVaultShown = false
		, bool? immortalThrone = true
		, Item? item = null
		, AutoMoveLocation[]? panels = null)
		=> ItemActionRules.MoveDestinations(new MoveContext(
			here, sackType, maxSacks, playerSackCount, panels ?? AllPanels, secondaryVaultShown, immortalThrone, item));

	#region MoveDestinations

	[Fact]
	public void MoveDestinations_VaultBags_SkipTheItemBagAndShowNumbersFromOne()
	{
		var destinations = Destinations(maxSacks: 3, item: ItemAt(SackType.Vault, 1), panels: []);

		destinations.Should().Equal(
			new MoveDestination(AutoMoveLocation.Vault, 0, 1)
			, new MoveDestination(AutoMoveLocation.Vault, 2, 3));
	}

	[Fact]
	public void MoveDestinations_SelectionOnEmptyCell_SkipsTheShownBagAndShowsVaultNumbersFromOne()
	{
		// A selection right-clicked on an empty cell: no item under the cursor, bag 2 shown.
		var destinations = ItemActionRules.MoveDestinations(new MoveContext(
			AutoMoveLocation.Vault, SackType.Vault, 3, 0, [], false, true, null, CurrentSack: 1));

		destinations.Should().Equal(
			new MoveDestination(AutoMoveLocation.Vault, 0, 1)
			, new MoveDestination(AutoMoveLocation.Vault, 2, 3));
	}

	[Fact]
	public void MoveDestinations_PlayerSacks_MainSackFirstAndOnlyTheSacksThePlayerHas()
	{
		var destinations = Destinations(AutoMoveLocation.Player, SackType.Player, maxSacks: 5, playerSackCount: 3
			, item: ItemAt(SackType.Player, 2), panels: []);

		destinations.Should().Equal(
			new MoveDestination(AutoMoveLocation.Player, 0, 0, IsMainPlayerSack: true)
			, new MoveDestination(AutoMoveLocation.Player, 1, 1));
	}

	[Fact]
	public void MoveDestinations_Panels_ExcludeHereAndDuplicatesInMenuOrder()
	{
		var destinations = Destinations(maxSacks: 1);

		destinations.Select(d => d.Location).Should().Equal(
			AutoMoveLocation.Player, AutoMoveLocation.Stash, AutoMoveLocation.Trash);
		destinations.Should().OnlyContain(d => d.BagIndex == null);
	}

	[Fact]
	public void MoveDestinations_SecondaryVaultShown_ReplacesPlayerAndStash()
	{
		var destinations = Destinations(secondaryVaultShown: true);

		destinations.Select(d => d.Location).Should().Equal(AutoMoveLocation.SecondaryVault, AutoMoveLocation.Trash);
	}

	[Theory]
	[InlineData("records/item/equipmentring/ring01.dbr", true)]
	[InlineData("records/xpack/items/equipmentring/ring01.dbr", false)]
	public void MoveDestinations_TitanQuestSave_NoStashNoTrashAndPlayerForBaseGameItemsOnly(string id, bool playerAllowed)
	{
		AutoMoveLocation[] expected = playerAllowed ? [AutoMoveLocation.Player] : [];

		var destinations = Destinations(immortalThrone: false, item: ItemAt(SackType.Vault, 0, id));

		destinations.Select(d => d.Location).Should().Equal(expected);
	}

	[Fact]
	public void MoveDestinations_TitanQuestSave_SelectionWithoutItem_CannotGoToThePlayer()
		=> Destinations(immortalThrone: false).Should().BeEmpty();

	[Fact]
	public void MoveDestinations_NoPlayerLoaded_KeepsEveryPanel()
		=> Destinations(immortalThrone: null).Select(d => d.Location).Should().Equal(
			AutoMoveLocation.Player, AutoMoveLocation.Stash, AutoMoveLocation.Trash);

	#endregion

	#region AutoMovePlanner

	[Fact]
	public void Order_LargestFirst_HeightCountsMoreThanWidth_NullsSkipped()
	{
		var small = Sized(1, 1);
		var wide = Sized(2, 1);
		var tall = Sized(1, 2);
		var big = Sized(2, 4);

		AutoMovePlanner.Order([small, null, wide, big, tall]).Should().Equal(big, tall, wide, small);
	}

	[Fact]
	public void Plan_Selection_IsOrderedAndIgnoresTheFocusedItem()
	{
		var small = Sized(1, 1);
		var big = Sized(2, 3);
		var focused = Sized(1, 3);

		var plan = AutoMovePlanner.Plan([small, big], focused, AutoMoveLocation.Stash, 2);

		plan.Items.Should().Equal(big, small);
		plan.Destination.Should().Be(AutoMoveLocation.Stash);
		plan.DestIndex.Should().Be(2);
	}

	[Fact]
	public void Plan_NoSelection_MovesTheFocusedItemOrNothing()
	{
		var focused = Sized(1, 1);

		AutoMovePlanner.Plan(null, focused, AutoMoveLocation.Vault).Items.Should().Equal(focused);
		AutoMovePlanner.Plan(null, null, AutoMoveLocation.Vault).Items.Should().BeEmpty();
	}

	[Theory]
	[InlineData(AutoMoveLocation.Vault, AutoMoveLocation.Vault, false, AutoMoveLocation.NotSet)]
	[InlineData(AutoMoveLocation.Stash, AutoMoveLocation.Vault, false, AutoMoveLocation.Stash)]
	[InlineData(AutoMoveLocation.Player, AutoMoveLocation.Vault, false, AutoMoveLocation.Player)]
	[InlineData(AutoMoveLocation.Player, AutoMoveLocation.Vault, true, AutoMoveLocation.SecondaryVault)]
	[InlineData(AutoMoveLocation.Stash, AutoMoveLocation.Vault, true, AutoMoveLocation.NotSet)]
	[InlineData(AutoMoveLocation.Vault, AutoMoveLocation.Player, true, AutoMoveLocation.Vault)]
	public void QuickMoveDestination_RemapsThePlayerKeyToTheSecondaryVault(
		AutoMoveLocation requested, AutoMoveLocation here, bool secondaryVaultShown, AutoMoveLocation expected)
		=> AutoMovePlanner.QuickMoveDestination(requested, here, secondaryVaultShown).Should().Be(expected);

	[Theory]
	// The equipment cannot take an auto-moved item: the transfer stash does.
	[InlineData(BagIdConstants.BAGID_EQUIPMENTPANEL, true, true, true, BagIdConstants.BAGID_TRANSFERSTASH, true)]
	[InlineData(BagIdConstants.BAGID_EQUIPMENTPANEL, true, false, true, BagIdConstants.BAGID_TRANSFERSTASH, false)]
	[InlineData(BagIdConstants.BAGID_PLAYERSTASH, true, false, false, BagIdConstants.BAGID_PLAYERSTASH, true)]
	[InlineData(BagIdConstants.BAGID_PLAYERSTASH, false, true, true, BagIdConstants.BAGID_PLAYERSTASH, false)]
	[InlineData(BagIdConstants.BAGID_TRANSFERSTASH, false, true, false, BagIdConstants.BAGID_TRANSFERSTASH, true)]
	[InlineData(BagIdConstants.BAGID_TRANSFERSTASH, true, false, true, BagIdConstants.BAGID_TRANSFERSTASH, false)]
	[InlineData(BagIdConstants.BAGID_RELICVAULTSTASH, false, false, true, BagIdConstants.BAGID_RELICVAULTSTASH, true)]
	[InlineData(BagIdConstants.BAGID_RELICVAULTSTASH, true, true, false, BagIdConstants.BAGID_RELICVAULTSTASH, false)]
	public void StashTarget_ShownStash_MustBeLoaded(
		int currentBag, bool hasPlayerStash, bool hasTransferStash, bool hasRelicVaultStash, int expectedBag, bool expectedAvailable)
		=> AutoMovePlanner.StashTarget(currentBag, hasPlayerStash, hasTransferStash, hasRelicVaultStash)
			.Should().Be(new StashMoveTarget(expectedBag, expectedAvailable));

	#endregion
}
