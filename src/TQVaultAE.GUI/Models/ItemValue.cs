using TQVaultAE.Domain.Entities;

namespace TQVaultAE.GUI.Models;

public class ItemType : ItemValue<string> { }
public class ItemRarity : ItemValue<Rarity> { }
public class ItemOrigin : ItemValue<GameDlc> { }

public class ItemValue<TValue>
{
	public TValue Value { get; set; }
	public string DisplayName { get; set; }
	public override string ToString()
		=> DisplayName;
}
