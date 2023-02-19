using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TQVaultAE.Domain.Contracts.Services;

namespace TQVaultAE.GUI.Models.SearchDialogAdvanced;

public class SearchQueries : List<SearchQuery>
{
	#region Logic

	static SearchQueries _Default;
	static IGamePathService GamePathService;

	public static SearchQueries Default(IGamePathService gamePathService)
	{
		if(GamePathService is null) 
			GamePathService = gamePathService;

		if (_Default is null) _Default = Read();
		return _Default;
	}

	public void Save()
	{
		string xmlPath = ResolveSearchQueriesFilePath();
		var json = JsonConvert.SerializeObject(this, Formatting.Indented);
		File.WriteAllText(xmlPath, json);
	}

	private static string ResolveSearchQueriesFilePath()
		=> Path.Combine(GamePathService.TQVaultConfigFolder, "SearchQueries.json");

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


