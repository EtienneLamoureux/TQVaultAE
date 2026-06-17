namespace TQVaultAE.Domain.Entities;

public record ItemClassMapItem<T>(string ItemClass, T Value)
{
	public string ItemClassUpper => ItemClass.ToUpper();
}
