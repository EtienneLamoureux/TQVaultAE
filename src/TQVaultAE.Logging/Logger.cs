using log4net;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TQVaultAE.Logging
{
	/// <summary>
	/// Helper pour log4net.ILog
	/// </summary>
	public static class Logger
	{
		/// <summary>
		/// ILog static
		/// </summary>
		private static log4net.ILog _logger;
		/// <summary>
		/// ILog static
		/// </summary>
		public static log4net.ILog Log
		{
			get
			{
				if (_logger == null)
				{
					Type type = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType;
					_logger = Get(type);
				}
				return _logger;
			}
		}

		/// <summary>
		/// Constructeur
		/// </summary>
		static Logger()
		{
			// Configure log4net
			log4net.Config.XmlConfigurator.Configure();

			// By default, the EventID in Log4net is 0. But for ASP.NET or WCF, it should be, at least, 1.
			// http://blog.knuthaugen.no/2010/12/log4net-windows-event-log-and-iis-applications.html
			//log4net.ThreadContext.Properties["EventID"] = 1;
		}

		/// <summary>
		/// Factory
		/// </summary>
		/// <param name="typeInstance">instance du type emeteur</param>
		/// <returns></returns>
		public static log4net.ILog Get(object typeInstance)
		{
			if (typeInstance == null) throw new ArgumentNullException("typeInstance");
			var type = (typeInstance is Type) ? (Type)typeInstance : typeInstance.GetType();
			var _log = log4net.LogManager.GetLogger(type);
			return _log;
		}

		/// <summary>
		/// Change log4net root loglevel at runtime
		/// </summary>
		/// <param name="newLevel"></param>
		/// <returns>true if change occured, false if <paramref name="newLevel"/> was in place already</returns>
		public static bool ChangeRootLogLevel(Level newLevel)
		{
			var repo = (log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository();
			if (repo.Root.Level == newLevel) return false;
			repo.Root.Level = newLevel;
			repo.RaiseConfigurationChanged(EventArgs.Empty);
			return true;
		}

		/// <summary>
		/// Factory
		/// </summary>
		/// <param name="loggerName">from configuration file or arbitrary name</param>
		/// <returns></returns>
		public static log4net.ILog Get(string loggerName)
		{
			var _log = log4net.LogManager.GetLogger(loggerName);
			return _log;
		}
	}
}
