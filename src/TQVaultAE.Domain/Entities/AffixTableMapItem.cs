namespace TQVaultAE.Domain.Entities;

public record AffixTableMapItem(RecordId BrokenTable, RecordId PrefixTable, RecordId SuffixTable)
{
	public bool IsEmpty
		=> BrokenTable.IsEmpty
		&& PrefixTable.IsEmpty
		&& SuffixTable.IsEmpty;
}
