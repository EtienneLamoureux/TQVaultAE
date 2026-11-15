using AwesomeAssertions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services.Export;
using TQVaultAE.Tests.Shared;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// <see cref="ItemDtoExtensions"/>: the item part of the exports. An item's mod is never written: it is computed when the
/// item is read with the map of its container.
/// </summary>
public class ItemDtoExtensionsTests
{
	[Fact]
	public void Export_ModdedItem_WritesNoMod_AndTheImportedItemWaitsForItsContainersMap()
	{
		var item = new Item { BaseItemId = RecordId.Create("records/a.dbr") }.ReadFrom("shadowchampions|workshop:1949340853");
		item.SetContainerMod("shadowchampions|workshop:1949340853");

		var imported = ItemDtoExtensions.FromItem(item).ToItem();

		imported.Mod.Should().BeNull("its records are read once it is placed");
		imported.HasContainerMod.Should().BeFalse();
	}

	[Fact]
	public void Export_ItemShown1x1ForWantOfRoom_WritesItsRealSize()
	{
		var item = new Item { BaseItemId = RecordId.Create("records/a.dbr"), Width = 1, Height = 5 };
		item.ShowNoRoom();

		var dto = ItemDtoExtensions.FromItem(item);

		(dto.width, dto.height).Should().Be((1, 5));
		dto.ToItem().LacksRoom.Should().BeFalse("the lack of room is never saved");
	}
}
