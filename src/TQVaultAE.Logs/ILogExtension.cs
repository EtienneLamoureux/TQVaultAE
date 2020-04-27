using Microsoft.Extensions.Logging;
using System;

namespace TQVaultAE.Logs
{
	/// <summary>
	/// Extensions communes pour ILog
	/// </summary>
	public static class ILogExtension
	{

		/// <summary>
		/// Error Exception Only
		/// </summary>
		/// <param name="Log"></param>
		/// <param name="ex"></param>
		public static void ErrorException(this ILogger Log, Exception ex)
		{
			Log.LogError(ex, ex.Message);
		}

		/// <summary>
		/// format l'exception seulement
		/// </summary>
		/// <param name="Log"></param>
		/// <param name="ex"></param>
		public static string FormatException(this ILogger Log, Exception ex)
		{
			string mess = string.Empty;
			try
			{
				mess = new TextExceptionFormatter(ex).Format();
			}
			catch (Exception)
			{
				mess = $@"TextExceptionFormatter().Format() failed for error ""{ex.Message}"" !";
			}
			return mess;
		}
	}
}
