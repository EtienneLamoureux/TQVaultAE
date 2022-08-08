namespace TQVaultAE.Domain.Entities;

public record AffixTableMapItem(string BrokenTable, string PrefixTable, string SuffixTable)
{
	public bool IsEmpty
		=> string.IsNullOrWhiteSpace(BrokenTable)
		&& string.IsNullOrWhiteSpace(PrefixTable)
		&& string.IsNullOrWhiteSpace(SuffixTable);
}
