namespace TQVaultAE.Domain.Entities
{

	/// <summary>
	/// Class to encapsulate the actual record information from the file.
	/// Also does the decoding of the raw data.
	/// </summary>
	public class RecordInfo
	{
		/// <summary>
		/// Offset in the file for this record.
		/// </summary>
		public int Offset;

		/// <summary>
		/// String index of ID
		/// </summary>
		public int IdStringIndex;

		/// <summary>
		/// Initializes a new instance of the RecordInfo class.
		/// </summary>
		public RecordInfo()
		{
			this.IdStringIndex = -1;
			this.RecordType = string.Empty;
		}

		/// <summary>
		/// Gets the string ID
		/// </summary>
		public string ID { get; set; }

		/// <summary>
		/// Gets the Record type.
		/// </summary>
		public string RecordType { get; set; }


	}

}
