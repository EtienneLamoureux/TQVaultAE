using TQVaultAE.Domain.Entities;

namespace TQVaultAE.GUI.Models;

public class ItemRarity : ItemValue<Rarity> { }
public class ItemOrigin : ItemValue<GameDlc> { }

public class ItemValue<TValue>
{
	public required TValue Value { get; set; }
	public required string DisplayName { get; set; }
	public override string ToString()
		=> DisplayName;
}
