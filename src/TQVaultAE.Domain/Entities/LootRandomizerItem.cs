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
