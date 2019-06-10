using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace TQVaultData
{
	public static class DataSerializerExtension
	{
		public static XElement ToXElement<T>(this object obj)
		{
			using (var memoryStream = new MemoryStream())
			{
				using (TextWriter streamWriter = new StreamWriter(memoryStream))
				{
					var xmlSerializer = new XmlSerializer(typeof(T));
					xmlSerializer.Serialize(streamWriter, obj);
					var xmlstr = Encoding.UTF8.GetString(memoryStream.ToArray());
					return XElement.Parse(xmlstr);
				}
			}
		}

		public static T FromXElement<T>(this XElement xElement)
		{
			using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(xElement.ToString())))
			{
				var xmlSerializer = new XmlSerializer(typeof(T));
				return (T)xmlSerializer.Deserialize(memoryStream);
			}
		}
	}
}
