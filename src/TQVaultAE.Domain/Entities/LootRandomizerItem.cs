using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Domain.Entities;

public record LootRandomizerItem(
	string Id
	, string Tag
	, int Cost
	, int LevelRequirement
	, string ItemClass
	, string FileDescription
	, string Translation
	, string PrettyFileName
	, string Effect
	, string Number
)
{
	public static LootRandomizerItem Default(string rawRecordId)
	{
		var pretty = rawRecordId.PrettyFileName();
		var exploded = pretty.ExplodePrettyFileName();

		// Make a default based on RecordId prettyfied
		return new LootRandomizerItem(
			rawRecordId.NormalizeRecordPath()
			, string.Empty
			, 0
			, 0
			, string.Empty
			, string.Empty
			, pretty
			, pretty
			, exploded.Effect
			, exploded.Number
		);
	}
};
