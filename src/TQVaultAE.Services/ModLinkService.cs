using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GameFiles;

namespace TQVaultAE.Services;

/// <inheritdoc cref="IModLinkService"/>
public class ModLinkService(
	ILogger<ModLinkService> log
	, IGamePathService gamePathService
	, IFileIO fileIO
	, IPathIO pathIO
) : IModLinkService
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		PropertyNameCaseInsensitive = true,
		WriteIndented = true,
	};

	private readonly ILogger Log = log;
	private readonly IGamePathService GamePathService = gamePathService;
	private readonly IFileIO FileIO = fileIO;
	private readonly IPathIO PathIO = pathIO;

	private IReadOnlyList<InstalledMod>? _installedMods;

	/// <summary>
	/// What each record asked about was found in: null for the base game, else the identities of the installed mods
	/// having it (possibly none).
	/// </summary>
	private readonly ConcurrentDictionary<string, string[]?> _recordSources = new(StringComparer.OrdinalIgnoreCase);

	/// <inheritdoc/>
	public IReadOnlyList<InstalledMod> GetInstalledMods(bool refresh = false)
	{
		if (refresh)
			this._recordSources.Clear();

		if (this._installedMods is null || refresh)
			this._installedMods = [.. this.GamePathService.GetCustomMapList()
				.Select(entry => new InstalledMod(
					this.PathIO.GetFileName(Path.TrimEndingDirectorySeparator(entry.Path))
					, entry.Path
					, entry.DisplayName?.StartsWith("SteamWorkshop", StringComparison.OrdinalIgnoreCase) ?? false
						? ModSource.SteamWorkshop
						: ModSource.Legacy))];

		return this._installedMods;
	}

	/// <inheritdoc/>
	public InstalledMod? FindMod(string? key)
	{
		// A name without its source names no mod: a Steam Workshop mod and a custom map can share it.
		if (ModIdentity.SourceOf(key) is null)
			return null;

		return this.GetInstalledMods().FirstOrDefault(m => m.Id.Equals(key, StringComparison.OrdinalIgnoreCase));
	}

	private string LinkFile(string saveFolder) => this.PathIO.Combine(saveFolder, IModLinkService.LinkFileName);

	/// <inheritdoc/>
	public ModLinkFile? ReadLink(string saveFolder)
	{
		string file = this.LinkFile(saveFolder);
		if (!this.FileIO.Exists(file))
			return null;

		try
		{
			var link = JsonSerializer.Deserialize<ModLinkFile>(this.FileIO.ReadAllText(file), JsonOptions);
			// The name and the source make the mod: without both, the character is not linked.
			return string.IsNullOrWhiteSpace(link?.Mod) || string.IsNullOrWhiteSpace(link.Source) ? null : link;
		}
		catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
		{
			this.Log.LogError(ex, "Failed to read the mod link {File}", file);
			return null;
		}
	}

	/// <inheritdoc/>
	public CharacterMod Resolve(string saveFolder, bool isCustomCharacter)
	{
		if (!isCustomCharacter)
			return CharacterMod.BaseGame;

		var link = this.ReadLink(saveFolder);
		if (link is null)
			return CharacterMod.Unlinked;

		// The folder where it was linked first; else its name and source.
		if (this.GetInstalledMods().FirstOrDefault(m => link.Folder is not null && this.SameFolder(m.Folder, link.Folder)) is { } linked)
			return new CharacterMod(CharacterModState.Linked, linked, link);

		return this.FindMod(ModIdentity.Compose(link.Mod, link.Source)) is { } mod
			? new CharacterMod(CharacterModState.Linked, mod, link)
			: new CharacterMod(CharacterModState.LinkedModMissing, null, link);
	}

	private bool SameFolder(string a, string b)
		=> string.Equals(Path.TrimEndingDirectorySeparator(a), Path.TrimEndingDirectorySeparator(b), StringComparison.OrdinalIgnoreCase);

	/// <inheritdoc/>
	public ModLinkResult Link(string saveFolder, InstalledMod installedMod)
	{
		var link = new ModLinkFile
		{
			Mod = installedMod.Name,
			Source = installedMod.SourceKey,
			Folder = installedMod.Folder,
			LinkedUtc = DateTime.UtcNow,
		};

		try
		{
			this.FileIO.WriteAllText(this.LinkFile(saveFolder), JsonSerializer.Serialize(link, JsonOptions));
			return ModLinkResult.Succeeded;
		}
		catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
		{
			this.Log.LogError(ex, "Failed to link {SaveFolder} to the mod {Mod}", saveFolder, installedMod.Name);
			return ModLinkResult.Failed(ex);
		}
	}

	/// <inheritdoc/>
	public ModLinkResult Unlink(string saveFolder)
	{
		string file = this.LinkFile(saveFolder);
		try
		{
			if (this.FileIO.Exists(file))
				this.FileIO.Delete(file);
			return ModLinkResult.Succeeded;
		}
		catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
		{
			this.Log.LogError(ex, "Failed to unlink {SaveFolder}", saveFolder);
			return ModLinkResult.Failed(ex);
		}
	}

	/// <inheritdoc/>
	public ModSuggestion Suggest(string saveFolder)
	{
		var records = new HashSet<string>(StringComparer.Ordinal);
		foreach (var name in new[] { this.GamePathService.PlayerSaveFileName, this.GamePathService.PlayerStashFileNameB })
		{
			string file = this.PathIO.Combine(saveFolder, name);
			if (this.FileIO.Exists(file))
				records.UnionWith(SaveRecordScanner.ReadRecordIds(this.FileIO.ReadAllBytes(file)));
		}

		return this.SuggestFor(records);
	}

	/// <inheritdoc/>
	public ModSuggestion SuggestFor(IEnumerable<string> records)
	{
		var distinct = records.Where(r => !string.IsNullOrWhiteSpace(r)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
		this.LookUp(distinct.Where(r => !this._recordSources.ContainsKey(r)).ToList());

		var modOnly = distinct.Where(r => this._recordSources.TryGetValue(r, out var mods) && mods is not null).ToList();
		if (modOnly.Count == 0)
			return new ModSuggestion(0, []);

		var matches = this.GetInstalledMods()
			.Select(mod => new ModMatch(mod, modOnly.Count(r => this._recordSources[r]!.Contains(mod.Id, StringComparer.OrdinalIgnoreCase))))
			.Where(m => m.Matched > 0);

		return new ModSuggestion(modOnly.Count, [.. matches.OrderByDescending(m => m.Matched).ThenBy(m => m.Mod.Name, StringComparer.OrdinalIgnoreCase)]);
	}

	/// <summary>
	/// Finds where records come from: the base game, else the installed mods having them. Each database is opened once.
	/// </summary>
	private void LookUp(List<string> records)
	{
		if (records.Count == 0)
			return;

		var modOnly = this.WithoutBaseGameRecords(records.ToHashSet(StringComparer.OrdinalIgnoreCase));
		foreach (var record in records.Except(modOnly, StringComparer.OrdinalIgnoreCase))
			this._recordSources[record] = null;

		if (modOnly.Count == 0)
			return;

		var found = modOnly.ToDictionary(r => r, _ => new List<string>(), StringComparer.OrdinalIgnoreCase);
		foreach (var mod in this.GetInstalledMods())
		{
			string? database = this.ModDatabaseFile(mod);
			if (database is null)
				continue;

			foreach (var record in this.Found(database, modOnly))
				found[record].Add(mod.Id);
		}

		foreach (var (record, mods) in found)
			this._recordSources[record] = [.. mods];
	}

	/// <summary>
	/// The database of a mod: <c>database\&lt;mod name&gt;.arz</c>, as the game reads it.
	/// </summary>
	private string? ModDatabaseFile(InstalledMod mod)
	{
		string file = this.PathIO.Combine(mod.Folder, "database", $"{mod.Name}.arz");
		return this.FileIO.Exists(file) ? file : null;
	}

	private List<string> WithoutBaseGameRecords(IReadOnlySet<string> records)
	{
		var databases = new[] { this.GamePathService.GamePathTQIT, this.GamePathService.GamePathTQ }
			.Where(p => !string.IsNullOrWhiteSpace(p))
			.Select(p => this.PathIO.Combine(p, "Database", "database.arz"))
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.Where(this.FileIO.Exists)
			.ToList();

		var remaining = records.ToList();
		foreach (var database in databases)
		{
			try
			{
				using var arz = ArzDatabaseFactory.Open(database);
				remaining.RemoveAll(id => arz.Find(id) is not null);
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
			{
				// The game is updating or holds the file: the suggestion goes on without this database.
				this.Log.LogError(ex, "Failed to read the game database {File}", database);
			}
		}

		return remaining;
	}

	private List<string> Found(string database, List<string> records)
	{
		try
		{
			using var arz = ArzDatabaseFactory.Open(database);
			return [.. records.Where(id => arz.Find(id) is not null)];
		}
		catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException)
		{
			this.Log.LogError(ex, "Failed to read the mod database {Database}", database);
			return [];
		}
	}
}
