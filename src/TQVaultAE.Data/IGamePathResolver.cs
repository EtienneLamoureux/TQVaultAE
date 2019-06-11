using System;
using System.Collections.Generic;
using System.Text;

namespace TQVaultAE.Data
{
	/// <summary>
	/// Used to inject environment specific code in agnostic layer 
	/// </summary>
	public interface IGamePathResolver
	{
		string ResolveTQ();
		string ResolveTQIT();
	}
}
