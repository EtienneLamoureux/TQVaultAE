using TQVaultAE.Domain.Entities;

namespace TQVaultAE.GUI.Models;

public class ItemRarity : ItemValue<Rarity> { }
public class ItemOrigin : ItemValue<GameDlc> { }
/// <summary>An installed mod of the highlight filters: its identity (<c>Item.Mod</c>) and its name.</summary>
public class ItemMod : ItemValue<string> { }

public class ItemValue<TValue>
{
	public required TValue Value { get; set; }
	public required string DisplayName { get; set; }
	public override string ToString()
		=> DisplayName;
}
