using System.Collections.Generic;
using System.IO;

namespace TQVaultAE.Data
{

	/// <summary>
	/// Holds class tag information
	/// </summary>
	public class PlayerClass
	{

		static Dictionary<string, string> _classKey = new Dictionary<string, string> {
		};

		// TODO Resource files exists to avoid LoadClassDataFile. classTagkey should be used to get translation from Resx directly and not from a file that you have to parse.


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
