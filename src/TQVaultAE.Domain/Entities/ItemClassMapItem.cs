using System;
using System.Collections.Generic;
using System.Text;

namespace TQVaultAE.Domain.Entities;

public record ItemClassMapItem<T>(string ItemClass, T Value)
{
	public string ItemClassUpper => ItemClass.ToUpper();
}
