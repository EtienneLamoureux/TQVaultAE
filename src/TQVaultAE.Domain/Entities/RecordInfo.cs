namespace TQVaultAE.Domain.Entities;

public class RecordInfo
{
	public int Offset;

	public int IdStringIndex;

	public int CompressedSize;

	public RecordInfo()
	{
		this.IdStringIndex = -1;
		this.RecordType = string.Empty;
	}

	public RecordId ID { get; set; }

	public string RecordType { get; set; }
}