using System;

namespace TQVaultAE.Logs
{
	/// <summary>
	/// Extensions communes pour ILog
	/// </summary>
	public static class ILogExtension
	{
		/// <summary>
		/// ErrorFormat avec exception.
		/// </summary>
		/// <param name="Log"></param>
		/// <param name="ex"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void ErrorFormat(this log4net.ILog Log, Exception ex, string format, params object[] args)
		{
			Log.Error(string.Format(format, args), ex);

		}

		/// <summary>
		/// Error Exception Only
		/// </summary>
		/// <param name="Log"></param>
		/// <param name="ex"></param>
		public static void ErrorException(this log4net.ILog Log, Exception ex)
		{
			Log.Error(ex.Message, ex);
		}

		/// <summary>
		/// format l'exception seulement
		/// </summary>
		/// <param name="Log"></param>
		/// <param name="ex"></param>
		public static string FormatException(this log4net.ILog Log, Exception ex)
		{
			string mess = string.Empty;
			try
			{
				mess = new TextExceptionFormatter(ex).Format();
			}
			catch (Exception)
			{
				mess = "TextExceptionFormatter().Format() failed !";
			}
			return mess;
		}

	}
}
