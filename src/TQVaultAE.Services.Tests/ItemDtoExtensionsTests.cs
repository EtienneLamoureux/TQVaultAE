using AwesomeAssertions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// <see cref="ItemDtoExtensions"/>: the item part of the exports keeps the mod link, and the base game chosen for an item.
/// </summary>
public class ItemDtoExtensionsTests
{
	[Fact]
	public void ExportThenImport_BaseGameChosen_IsKept()
	{
		// Arrange
		var item = new Item { BaseItemId = RecordId.Create("records/a.dbr"), ModOrigin = ModOrigin.Own };

		// Act
		var dto = ItemDtoExtensions.FromItem(item);
		var imported = dto.ToItem();

		// Assert
		dto.noMod.Should().BeTrue();
		(imported.Mod, imported.ModOrigin).Should().Be(((string?)null, ModOrigin.Own));
	}

	[Fact]
	public void Export_ItemWithAMod_WritesNoBaseGameMark()
	{
		var dto = ItemDtoExtensions.FromItem(new Item { BaseItemId = RecordId.Create("records/a.dbr"), Mod = "Lilith" });

		(dto.mod, dto.noMod).Should().Be(("Lilith", (bool?)null));
		dto.ToItem().ModOrigin.Should().Be(ModOrigin.Own);
	}
}
