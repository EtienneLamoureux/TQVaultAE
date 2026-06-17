using System.Text.Json;
using TQVaultAE.Application.Contracts.Services;

namespace TQVaultAE.GUI.Models.SearchDialogAdvanced;

public class SearchQueries : List<SearchQuery>
{
	#region Fields

	private readonly IGamePathService GamePathService;
	private readonly IFileIO FileIO;
	private readonly IPathIO PathIO;
	private readonly JsonSerializerOptions JsonOptions;
	private readonly string SearchQueriesFilePath;

	#endregion

	#region Constructor

	/// <summary>
	/// Empty ctor for deserialisation
	/// </summary>
	public SearchQueries()
	{ }

	/// <summary>
	/// For DI
	/// </summary>
	/// <param name="gamePathService"></param>
	/// <param name="fileIO"></param>
	/// <param name="pathIO"></param>
	/// <param name="jsonOptions"></param>
	public SearchQueries(IGamePathService gamePathService, IFileIO fileIO, IPathIO pathIO, JsonSerializerOptions jsonOptions)
	{
		GamePathService = gamePathService;
		FileIO = fileIO;
		PathIO = pathIO;
		JsonOptions = jsonOptions;
		SearchQueriesFilePath = PathIO.Combine(gamePathService.TQVaultConfigFolder, "SearchQueries.json");
		Read();
	}

	#endregion

	#region Public Methods

	public void Save()
	{
		var json = JsonSerializer.Serialize(this, JsonOptions);
		FileIO.WriteAllText(SearchQueriesFilePath, json);
	}

	private void Read()
	{
		if (FileIO.Exists(SearchQueriesFilePath))
		{
			var content = FileIO.ReadAllText(SearchQueriesFilePath);
			var searchQueries = JsonSerializer.Deserialize<SearchQueries>(content, JsonOptions);
			this.Clear();
			this.AddRange(searchQueries);
		}
	}

	#endregion
}
