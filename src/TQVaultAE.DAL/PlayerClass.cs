using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TQVaultData
{

	/// <summary>
	/// Holds class tag information
	/// </summary>
	public class PlayerClass
	{

		static Dictionary<string, string> _classKey = new Dictionary<string, string> {
		};


		/// <summary>
		/// loads resource file, should be language specific
		/// </summary>
		/// <param name="fileContents"></param>
		public static void LoadClassDataFile(string fileContents)
		{
			using (var sr = new StringReader(fileContents))
			{
				var data = sr.ReadLine();
				while (data != null){
					var content = data.Split('=');
					if (content != null&&content.Length>1)
					{
						if (!_classKey.ContainsKey(content[0]))
						{
							_classKey.Add(content[0], content[1]);
						}
					}
					data = sr.ReadLine();
				}
			}
		}

		public static string GetClassDisplayName(string classTagkey)
		{
			if (_classKey.ContainsKey(classTagkey))
			{
				return (_classKey[classTagkey]);
			}
			return ("Unknown");
		}

	}
}
