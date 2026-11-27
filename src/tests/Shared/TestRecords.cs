using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Shared;

/// <summary>
/// Records as a map's database reads them, with the database they come from (<see cref="DBRecordCollection.Source"/>):
/// an item's mod (<see cref="Item.Mod"/>) is computed from them. Shared by the test projects (linked source).
/// </summary>
internal static class TestRecords
{
	/// <summary>The record a test item without a base item is given.</summary>
	public const string DefaultBaseItem = "records/test/item.dbr";

	/// <summary>A record read from a mod's database (<paramref name="mod"/> set), else from the base game's.</summary>
	/// <param name="id">The record.</param>
	/// <param name="mod">The mod's identity (<c>name|source</c>); null for the base game.</param>
	/// <param name="recordType">The record's class, also its <c>Class</c> variable.</param>
	public static Info InfoFrom(RecordId id, string? mod = null, string recordType = Item.ICLASS_HEAD)
	{
		var record = new DBRecordCollection(id, recordType) { Source = mod is null ? RecordSource.BaseGame : RecordSource.FromMod(mod) };
		record.Set(new Variable("Class", VariableDataType.StringVar, 1) { [0] = recordType });
		return new(record);
	}

	/// <summary>
	/// Gives the item its base item record, read from a mod's database (<paramref name="mod"/> set) or the base game's:
	/// <see cref="Item.Mod"/> then gives <paramref name="mod"/>.
	/// </summary>
	/// <param name="item">The item.</param>
	/// <param name="mod">The mod's identity (<c>name|source</c>); null for the base game.</param>
	/// <param name="recordType">The class of its base item record; by default the class it has.</param>
	public static Item ReadFrom(this Item item, string? mod, string? recordType = null)
	{
		if (RecordId.IsNullOrEmpty(item.BaseItemId))
			item.BaseItemId = DefaultBaseItem;

		string type = recordType ?? (string.IsNullOrEmpty(item.baseItemInfo?.ItemClass) ? Item.ICLASS_HEAD : item.baseItemInfo.ItemClass);
		item.baseItemInfo = InfoFrom(item.BaseItemId, mod, type);
		return item;
	}
}
