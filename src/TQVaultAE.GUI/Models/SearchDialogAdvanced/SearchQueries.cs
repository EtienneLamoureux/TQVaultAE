using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace TQVaultAE.GUI.Models.SearchDialogAdvanced;

public class SearchQueries : List<SearchQuery>
{
	#region Logic

	static SearchQueries _Default;

	public static SearchQueries Default
	{
		get
		{
			if (_Default is null) _Default = Read();
			return _Default;
		}
	}

	public void Save()
	{
		string xmlPath = ResolveSearchQueriesFilePath();
		var json = JsonConvert.SerializeObject(this, Formatting.Indented);
		File.WriteAllText(xmlPath, json);
	}

	private static string ResolveSearchQueriesFilePath()
	{
		var currentPath = new System.Uri(Assembly.GetExecutingAssembly().EscapedCodeBase).LocalPath;
		currentPath = Path.GetDirectoryName(currentPath);
		var jsonPath = Path.Combine(currentPath, "SearchQueries.json");
		return jsonPath;
	}

	public static SearchQueries Read()
	{
		string jsonPath = ResolveSearchQueriesFilePath();

		if (File.Exists(jsonPath))
			return ParseSettings(File.ReadAllText(jsonPath));

		return new SearchQueries();// Default
	}

	public static SearchQueries ParseSettings(string xmlData)
	{
		return JsonConvert.DeserializeObject<SearchQueries>(xmlData);
	}

	#endregion
}


