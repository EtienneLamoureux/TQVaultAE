using System;
using System.Collections.Generic;
using System.Text;

namespace TQVaultAE.Domain.Entities
{
	/// <summary>
	/// Allowed <see cref="GearType"/> for this element
	/// </summary>
	[AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
	public class GearTypeAttribute : Attribute
	{
		public GearType Type { get; }

		public GearTypeAttribute(GearType type)
		{
			Type = type;
		}
	}
}
