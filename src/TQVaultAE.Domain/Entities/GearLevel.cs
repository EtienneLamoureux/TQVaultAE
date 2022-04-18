using System;

namespace TQVaultAE.Domain.Entities
{

	/// <summary>
	/// Gear level classification
	/// </summary>
	public enum GearLevel
	{
		/// <summary>
		/// Item is no gear
		/// </summary>
		NoGear = 0,
		Broken = 1,
		Mundane = 2,
		Common = 3,
		Rare = 4,
		Epic = 5,
		Legendary = 6,
	}
}
