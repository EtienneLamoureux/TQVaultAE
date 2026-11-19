using TQVaultAE.Data.Dto;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Services.Export;

public static class ItemDtoExtensions
{
	/// <summary>
	/// The export of an item. Its mod is not written: an item takes the map of the container it is put in.
	/// </summary>
	/// <param name="item">The item.</param>
	public static ItemDto FromItem(Item item)
	{
		var dto = item.ToDto();
		// Its real size, even while it is shown 1 x 1 for want of room.
		dto.width = item.RealWidth;
		dto.height = item.RealHeight;
		return dto;
	}

	public static Item ToItem(this ItemDto dto)
	{
		var item = new Item
		{
			Width = dto.width,
			Height = dto.height,
			endBlockCrap2 = 0,
			endBlockCrap1 = 0
		};
		dto.ApplyTo(item);
		return item;
	}
}