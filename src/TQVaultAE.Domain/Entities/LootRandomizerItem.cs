namespace TQVaultAE.Domain.Entities;

public record LootRandomizerItem(
	RecordId Id
	, string Tag
	, int Cost
	, int LevelRequirement
	, string ItemClass
	, string FileDescription
	, string Translation
)
{
	/// <summary>
	/// Tells if this loot randomizer record is a fake and doesn't exist in the database.
	/// Orphan reference in a loot table.
	/// </summary>
	public bool Unknown { get; init; }

	public bool TranslationTagIsEmpty => string.IsNullOrWhiteSpace(this.Tag);

	public static LootRandomizerItem Default(RecordId rawRecordId)
	{
		// Make a default based on RecordId
		return new LootRandomizerItem(
			rawRecordId
			, string.Empty
			, 0
			, 0
			, string.Empty
			, string.Empty
			, rawRecordId.PrettyFileName
		);
	}
};
