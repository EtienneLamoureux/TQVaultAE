using System;
using System.Collections.Generic;
using System.Text;

namespace TQVaultAE.Logs
{
	/// <summary>
	/// Simple interface as a log4net.ILog factory in DI.
	/// </summary>
	/// <typeparam name="TCategoryName"></typeparam>
	public interface ILogger<TCategoryName>
	{
		log4net.ILog Logger { get; }
	}
}
