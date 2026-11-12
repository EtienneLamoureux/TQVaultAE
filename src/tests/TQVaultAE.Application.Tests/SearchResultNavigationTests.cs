using System.Drawing;
using AwesomeAssertions;
using TQVaultAE.Application;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Tests.Application;

public class SearchResultNavigationTests
{
	private static SearchResult ResultAt(SackType sackType, int sackNumber, StashType? stashType = null, int x = 3, int y = 4)
	{
		var item = new Item
		{
			Place = new ContainerPlace
			{
				Path = """C:\saves\container""",
				Name = "container",
				SackType = sackType,
				SackNumber = sackNumber,
				StashType = stashType,
			},
			Location = new Point(x, y),
		};
		return new SearchResult(item, new Lazy<ToFriendlyNameResult>(() => new ToFriendlyNameResult(item)));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(5)]
	public void Resolve_VaultSack_SelectsTheSameVaultBag(int sackNumber)
	{
		var location = SearchResultNavigation.Resolve(ResultAt(SackType.Vault, sackNumber));

		location.Should().Be(new ResultLocation(ResultPanelKind.Vault, sackNumber, new Point(3, 4)));
		location.NeedsCharacter.Should().BeFalse();
		location.OnStashPanel.Should().BeFalse();
	}

	[Fact]
	public void Resolve_PlayerSackZero_SelectsTheMainSack()
	{
		var location = SearchResultNavigation.Resolve(ResultAt(SackType.Player, 0));

		location.Panel.Should().Be(ResultPanelKind.PlayerMainSack);
		location.Cell.Should().Be(new Point(3, 4));
		location.NeedsCharacter.Should().BeTrue();
		location.OnStashPanel.Should().BeFalse();
	}

	[Theory]
	[InlineData(1, 0)]
	[InlineData(3, 2)]
	public void Resolve_PlayerSackN_SelectsBagNMinusOne(int sackNumber, int expectedBag)
	{
		var location = SearchResultNavigation.Resolve(ResultAt(SackType.Player, sackNumber));

		location.Panel.Should().Be(ResultPanelKind.PlayerBag);
		location.Bag.Should().Be(expectedBag);
		location.NeedsCharacter.Should().BeTrue();
	}

	[Fact]
	public void Resolve_Equipment_SelectsTheEquipmentTabOfTheCharacter()
	{
		var location = SearchResultNavigation.Resolve(ResultAt(SackType.Equipment, BagIdConstants.BAGID_EQUIPMENTPANEL, x: 0, y: 7));

		location.Should().Be(new ResultLocation(ResultPanelKind.Equipment, BagIdConstants.BAGID_EQUIPMENTPANEL, new Point(0, 7)));
		location.NeedsCharacter.Should().BeTrue();
		location.OnStashPanel.Should().BeTrue();
	}

	[Fact]
	public void Resolve_PlayerStash_SelectsTheStashTabOfTheCharacter()
	{
		var location = SearchResultNavigation.Resolve(ResultAt(SackType.Stash, BagIdConstants.BAGID_PLAYERSTASH, StashType.PlayerStash));

		location.Panel.Should().Be(ResultPanelKind.CharacterStash);
		location.Bag.Should().Be(BagIdConstants.BAGID_PLAYERSTASH);
		location.NeedsCharacter.Should().BeTrue();
		location.OnStashPanel.Should().BeTrue();
	}

	[Theory]
	[InlineData(StashType.TransferStash, BagIdConstants.BAGID_TRANSFERSTASH)]
	[InlineData(StashType.RelicVaultStash, BagIdConstants.BAGID_RELICVAULTSTASH)]
	public void Resolve_SharedStash_SelectsItsTabWithoutCharacter(StashType stashType, int expectedBag)
	{
		var location = SearchResultNavigation.Resolve(ResultAt(SackType.Stash, (int)stashType, stashType));

		location.Panel.Should().Be(ResultPanelKind.SharedStash);
		location.Bag.Should().Be(expectedBag);
		location.NeedsCharacter.Should().BeFalse();
		location.OnStashPanel.Should().BeTrue();
	}

	[Fact]
	public void Resolve_StashWithoutStashType_IsTheCharacterStash()
	{
		var location = SearchResultNavigation.Resolve(ResultAt(SackType.Stash, BagIdConstants.BAGID_PLAYERSTASH));

		location.Panel.Should().Be(ResultPanelKind.CharacterStash);
	}

	[Fact]
	public void Resolve_Trash_HasNoPanel()
	{
		var location = SearchResultNavigation.Resolve(ResultAt(SackType.Trash, 0));

		location.Panel.Should().Be(ResultPanelKind.None);
		location.NeedsCharacter.Should().BeFalse();
		location.OnStashPanel.Should().BeFalse();
	}

	[Theory]
	[InlineData(0, 1)]
	[InlineData(5, 6)]
	public void BagNumber_VaultSackN_IsBagNPlusOne(int sackNumber, int expected)
		=> SearchResultNavigation.Resolve(ResultAt(SackType.Vault, sackNumber)).BagNumber.Should().Be(expected);

	[Theory]
	[InlineData(1, 1)]
	[InlineData(3, 3)]
	public void BagNumber_PlayerSackN_IsBagN(int sackNumber, int expected)
		=> SearchResultNavigation.Resolve(ResultAt(SackType.Player, sackNumber)).BagNumber.Should().Be(expected);

	[Fact]
	public void BagNumber_PlayerMainSack_IsNull()
		=> SearchResultNavigation.Resolve(ResultAt(SackType.Player, 0)).BagNumber.Should().BeNull();

	[Theory]
	[InlineData(SackType.Equipment, BagIdConstants.BAGID_EQUIPMENTPANEL, null)]
	[InlineData(SackType.Stash, BagIdConstants.BAGID_PLAYERSTASH, StashType.PlayerStash)]
	[InlineData(SackType.Stash, BagIdConstants.BAGID_TRANSFERSTASH, StashType.TransferStash)]
	[InlineData(SackType.Stash, BagIdConstants.BAGID_RELICVAULTSTASH, StashType.RelicVaultStash)]
	[InlineData(SackType.Trash, 0, null)]
	public void BagNumber_EquipmentStashesAndTrash_IsNull(SackType sackType, int sackNumber, StashType? stashType)
		=> SearchResultNavigation.Resolve(ResultAt(sackType, sackNumber, stashType)).BagNumber.Should().BeNull();
}
