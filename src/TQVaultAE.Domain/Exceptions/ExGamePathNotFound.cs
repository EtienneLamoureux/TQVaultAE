using System;
using System.Collections.Generic;
using System.Text;

namespace TQVaultAE.Domain.Exceptions
{
	/// <summary>
	/// Raised when game path is not found
	/// </summary>
	public class ExGamePathNotFound : ApplicationException
	{
		public ExGamePathNotFound() : base() { }
		public ExGamePathNotFound(string message) : base(message) { }
		public ExGamePathNotFound(string message, Exception innerException) : base(message, innerException) { }
	}
}
