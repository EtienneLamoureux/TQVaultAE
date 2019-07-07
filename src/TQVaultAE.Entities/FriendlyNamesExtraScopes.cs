using System;
using System.Collections.Generic;
using System.Text;

namespace TQVaultAE.Entities
{
	/// <summary>
	/// Extra data scopes when getting item translations
	/// </summary>
	[Flags]
	public enum FriendlyNamesExtraScopes
	{
		BaseAttributes = 1 << 1,
		PrefixAttributes = 1 << 2,
		SuffixAttributes = 1 << 3,
		RelicAttributes = 1 << 4,
		Relic2Attributes = 1 << 5,
		Requirements = 1 << 6,
		ItemSet = 1 << 7,
		ItemExtraAttributes = PrefixAttributes | SuffixAttributes | RelicAttributes | Relic2Attributes,
		ItemFullDisplay = BaseAttributes | ItemExtraAttributes | ItemSet | Requirements,
	}
}
