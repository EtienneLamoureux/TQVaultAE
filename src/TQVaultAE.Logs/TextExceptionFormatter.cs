using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading;

namespace TQVaultAE.Logs
{
	/// <summary>
	/// Format an exception in a human readable string format.
	/// </summary>
	/// <see cref="SizeOnDisk.Utilities.TextExceptionFormatter"/>
	public class TextExceptionFormatter
	{
		// Fields
		private int innerDepth;
		private readonly Exception exception;
		private NameValueCollection additionalInfo;
		private StringBuilder stringBuilder = new StringBuilder(1024);
		private static readonly List<string> IgnoredProperties = new List<string>() { "Source", "Message", "HelpLink", "InnerException", "StackTrace" };

		/// <summary>
		/// Ctrs
		/// </summary>
		/// <param name="exception"></param>
		public TextExceptionFormatter(Exception exception)
		{
			if (exception == null)
			{
				throw new ArgumentNullException("exception");
			}
			this.exception = exception;
		}

		/// <summary>
		/// Formattage
		/// </summary>
		/// <returns></returns>
		public string Format()
		{
			this.WriteDescription();
			this.WriteDateTime(DateTime.UtcNow);
			this.WriteException(this.exception, null);
			return this.stringBuilder.ToString();
		}

		private void WriteDescription()
		{
			this.stringBuilder.AppendLine(this.exception.GetType().FullName);
		}

		private void WriteDateTime(DateTime utcNow)
		{
			this.stringBuilder.AppendLine(utcNow.ToLocalTime().ToString("G", DateTimeFormatInfo.InvariantInfo));
		}

		private void Indent()
		{
			for (int i = 0; i < this.innerDepth; i++)
			{
				this.stringBuilder.Append("\t");
			}
		}

		private void WriteException(Exception exceptionToFormat, Exception outerException)
		{
			if (outerException != null)
			{
				this.innerDepth++;
				this.Indent();
				string innerException = "Inner Exception";
				this.stringBuilder.AppendLine(innerException);
				this.WriteException2(exceptionToFormat, outerException);
				this.innerDepth--;
			}
			else
			{
				this.WriteException2(exceptionToFormat, outerException);
			}
		}

		private void WriteException2(Exception exceptionToFormat, Exception outerException)
		{
			if (exceptionToFormat == null)
			{
				throw new ArgumentNullException("exceptionToFormat");
			}
			this.WriteExceptionType(exceptionToFormat.GetType());
			this.WriteMessage(exceptionToFormat.Message);
			this.WriteSource(exceptionToFormat.Source);
			this.WriteHelpLink(exceptionToFormat.HelpLink);
			this.WriteReflectionInfo(exceptionToFormat);
			this.WriteStackTrace(exceptionToFormat.StackTrace);
			if (outerException == null)
			{
				this.WriteAdditionalInfo(this.AdditionalInfo);
			}
			Exception innerException = exceptionToFormat.InnerException;
			if (innerException != null)
			{
				this.WriteException(innerException, exceptionToFormat);
			}
		}

		private void WriteExceptionType(Type exceptionType)
		{
			this.IndentAndWriteLine("Type: {0}", exceptionType.AssemblyQualifiedName);
		}

		private void WriteMessage(string message)
		{
			this.IndentAndWriteLine("Message: {0}", message);
		}

		private void WriteSource(string source)
		{
			this.IndentAndWriteLine("Source: {0}", source);
		}

		private void WriteHelpLink(string helpLink)
		{
			this.IndentAndWriteLine("HelpLink: {0}", helpLink);
		}

		private void WriteReflectionInfo(Exception exceptionToFormat)
		{
			object propertyAccessFailed;
			if (exceptionToFormat == null)
				throw new ArgumentNullException("exceptionToFormat");

			Type type = exceptionToFormat.GetType();
			PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
			foreach (PropertyInfo info in properties)
			{
				if ((info.CanRead && (IgnoredProperties.IndexOf(info.Name) == -1)) && (info.GetIndexParameters().Length == 0))
				{
					try
					{
						propertyAccessFailed = info.GetValue(exceptionToFormat, null);
					}
					catch (TargetInvocationException)
					{
						propertyAccessFailed = "Property Access Failed";
					}
					this.WritePropertyInfo(info, propertyAccessFailed);
				}
			}
			foreach (FieldInfo info2 in fields)
			{
				try
				{
					propertyAccessFailed = info2.GetValue(exceptionToFormat);
				}
				catch (TargetInvocationException)
				{
					propertyAccessFailed = "Field Access Failed";
				}
				this.WriteFieldInfo(info2, propertyAccessFailed);
			}
		}

		private void WritePropertyInfo(PropertyInfo propertyInfo, object value)
		{
			this.Indent();
			this.stringBuilder.Append(propertyInfo.Name);
			this.stringBuilder.Append(" : ");
			if (value == null)
			{
				this.stringBuilder.AppendLine("{null}");
			}
			else
			{
				this.stringBuilder.AppendLine(value.ToString());
			}
		}

		private void WriteFieldInfo(FieldInfo fieldInfo, object value)
		{
			this.Indent();
			this.stringBuilder.Append(fieldInfo.Name);
			this.stringBuilder.Append(" : ");
			if (value == null)
			{
				this.stringBuilder.AppendLine("{null}");
			}
			else
			{
				this.stringBuilder.AppendLine(value.ToString());
			}
		}

		private void WriteStackTrace(string stackTrace)
		{
			this.Indent();
			this.stringBuilder.Append("StackTrace: ");
			if ((stackTrace == null) || (stackTrace.Length == 0))
			{
				this.stringBuilder.AppendLine("Stack Trace Unavailable");
			}
			else
			{
				string str2 = stackTrace.Replace("\n", "\n" + new string('\t', this.innerDepth));
				this.stringBuilder.AppendLine(str2);
				this.stringBuilder.AppendLine();
			}
		}

		private void WriteAdditionalInfo(NameValueCollection additionalInformation)
		{
			this.stringBuilder.AppendLine("Additional Info:");
			this.stringBuilder.AppendLine();
			foreach (string str in additionalInformation.AllKeys)
			{
				this.stringBuilder.Append(str);
				this.stringBuilder.Append(" : ");
				this.stringBuilder.Append(additionalInformation[str]);
				this.stringBuilder.Append("\n");
			}
		}

		private void IndentAndWriteLine(string format, params object[] arg)
		{
			this.Indent();
			this.stringBuilder.AppendLine(string.Format(CultureInfo.CurrentCulture, format, arg));
		}

		/// <summary>
		/// Référence vers l'exception serialisée
		/// </summary>
		public Exception Exception
		{
			get
			{
				return this.exception;
			}
		}

		/// <summary>
		/// Informations additionnelles
		/// </summary>
		public NameValueCollection AdditionalInfo
		{
			get
			{
				if (this.additionalInfo == null)
				{
					this.additionalInfo = new NameValueCollection();
					this.additionalInfo.Add("MachineName", GetMachineName());
					this.additionalInfo.Add("TimeStamp", DateTime.UtcNow.ToString(CultureInfo.CurrentCulture));
					this.additionalInfo.Add("FullName", Assembly.GetExecutingAssembly().FullName);
					this.additionalInfo.Add("AppDomainName", AppDomain.CurrentDomain.FriendlyName);
					this.additionalInfo.Add("ThreadIdentity", Thread.CurrentPrincipal.Identity.Name);
				}
				return this.additionalInfo;
			}
		}

		private static string GetMachineName()
		{
			try
			{
				return Environment.MachineName;
			}
			catch (SecurityException)
			{
				return "Permission Denied";
			}
		}

		#region FormatException

		/// <summary>
		/// Format une exception en utilisant <see cref="SizeOnDisk.Utilities.TextExceptionFormatter"/>
		/// </summary>
		/// <param name="message"></param>
		/// <param name="ex"></param>
		/// <returns></returns>
		public static string FormatException(string message, Exception ex)
		{

			return string.Format(@"
{0}
Exception :
{1}
"
			, message
			, FormatException(ex)
			);

		}

		/// <summary>
		/// Format une exception en utilisant <see cref="Vp.UE.Common.Logging.TextExceptionFormatter"/>
		/// </summary>
		/// <param name="ex"></param>
		/// <returns></returns>
		public static string FormatException(Exception ex)
		{
			TextExceptionFormatter tef = new TextExceptionFormatter(ex);
			return tef.Format();
		}

		#endregion
	}
}
