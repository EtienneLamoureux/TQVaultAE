namespace TQVaultAE.Logs
{
	/// <summary>
	/// implementation of log4net.ILog factory in DI
	/// </summary>
	/// <typeparam name="TCategoryName"></typeparam>
	public class ILoggerImpl<TCategoryName> : ILogger<TCategoryName>
	{
		public log4net.ILog Logger => Logs.Logger.Get(typeof(TCategoryName));
	}
}
