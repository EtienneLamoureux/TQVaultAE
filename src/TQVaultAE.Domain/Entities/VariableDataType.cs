namespace TQVaultAE.Domain.Entities
{

	/// <summary>
	/// The different data-types a variable can be.
	/// </summary>
	public enum VariableDataType
	{
		/// <summary>
		/// int
		/// values will be Int32
		/// </summary>
		Integer = 0,

		/// <summary>
		/// float
		/// values will be Single
		/// </summary>
		Float = 1,

		/// <summary>
		/// string
		/// Values will be string
		/// </summary>
		StringVar = 2,

		/// <summary>
		/// bool
		/// Values will be Int32
		/// </summary>
		Boolean = 3,

		/// <summary>
		/// unknown type
		/// values will be Int32
		/// </summary>
		Unknown = 4
	}
}
