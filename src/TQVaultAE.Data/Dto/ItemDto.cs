namespace TQVaultAE.Data.Dto;

/// <summary>
/// Minimal dataset for item storage
/// </summary>
/// <remarks>Preserve original names</remarks>
public class ItemDto
{
	public int stackSize;
	public int seed;
	public string baseName = string.Empty;
	public string? prefixName;
	public string? suffixName;
	public string? relicName;
	public string? relicBonus;
	public int var1;
	public string? relicName2;
	public string? relicBonus2;
	public int var2;
	public int pointX;
	public int pointY;
	public int width;
	public int height;

	/// <summary>
	/// The mod the item is linked to (<c>Item.Mod</c>), when it is not the vault's.
	/// </summary>
	[System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
	public string? mod;
}