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
	static LootRandomizerItem _Empty = new LootRandomizerItem(
			string.Empty
			, string.Empty
			, 0
			, 0
			, string.Empty
			, string.Empty
			, string.Empty
			, string.Empty
			, string.Empty
			, string.Empty
		);

	public static LootRandomizerItem Empty => _Empty;
};
