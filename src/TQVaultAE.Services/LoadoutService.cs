using System.Text.Json;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Data.Dto;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Services;

/// <inheritdoc cref="ILoadoutService"/>
public partial class LoadoutService(
	ILogger<LoadoutService> log
	, IItemProvider itemProvider
	, IItemMovementService itemMovementService
	, IItemDatabaseService itemDatabaseService
	, SessionContext sessionContext
	, IFileIO fileIO
	, IPathIO pathIO
) : ILoadoutService
{
	/// <summary>Version written in the file, for later changes of its layout.</summary>
	internal const int FileVersion = 1;

	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		IncludeFields = true,
		PropertyNameCaseInsensitive = true,
		WriteIndented = true,
	};

	private readonly ILogger Log = log;
	private readonly IItemProvider ItemProvider = itemProvider;
	private readonly IItemMovementService ItemMovementService = itemMovementService;
	private readonly IItemDatabaseService ItemDatabaseService = itemDatabaseService;
	private readonly SessionContext Session = sessionContext;
	private readonly IFileIO FileIO = fileIO;
	private readonly IPathIO PathIO = pathIO;

	/// <inheritdoc/>
	public string FileOf(string saveFolder) => this.PathIO.Combine(saveFolder, ILoadoutService.FileName);

	/// <inheritdoc/>
	public LoadoutSet? Find(string saveFolder)
		=> this.Session.Loadouts.TryGetValue(this.FileOf(saveFolder), out var set) ? set : null;

	/// <inheritdoc/>
	public LoadoutSet Read(string saveFolder, string characterName, string? characterMod)
	{
		string file = this.FileOf(saveFolder);
		var set = new LoadoutSet(file, characterName, characterMod);
		if (this.FileIO.Exists(file))
		{
			try
			{
				var dto = JsonSerializer.Deserialize<LoadoutFileDto>(this.FileIO.ReadAllText(file), JsonOptions)
					?? throw new JsonException("Empty loadouts file");
				set.Loadouts = [.. dto.loadouts.Select(l => this.FromDto(l, characterMod))];
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
			{
				this.Log.LogError(ex, "Failed to read the loadouts {File}", file);
				set.ReadFailed = true;
			}
		}

		this.Session.Loadouts[file] = set;
		this.Changed(set);
		return set;
	}

	/// <inheritdoc/>
	public LoadoutNameError ValidateName(LoadoutSet loadouts, string? name, string? current = null)
	{
		if (string.IsNullOrWhiteSpace(name))
			return LoadoutNameError.Empty;

		var same = loadouts.Find(name);
		return same is null || string.Equals(same.Name, current, StringComparison.OrdinalIgnoreCase)
			? LoadoutNameError.None
			: LoadoutNameError.AlreadyUsed;
	}

	/// <inheritdoc/>
	public LoadoutChange Add(LoadoutSet loadouts, string name, PlayerCollection player)
	{
		if (this.ValidateName(loadouts, name) is var error and not LoadoutNameError.None)
			return LoadoutChange.Refused(error);
		if (loadouts.ReadFailed || player.EquipmentSack is null)
			return LoadoutChange.Missing;

		return this.Write(loadouts, [.. loadouts.Loadouts, Record(name.Trim(), player)]);
	}

	/// <inheritdoc/>
	public LoadoutChange Update(LoadoutSet loadouts, string name, PlayerCollection player)
	{
		if (loadouts.ReadFailed || loadouts.Find(name) is not { } loadout || player.EquipmentSack is null)
			return LoadoutChange.Missing;

		return this.Write(loadouts, [.. loadouts.Loadouts.Select(l => l == loadout ? Record(loadout.Name, player) : l)]);
	}

	/// <inheritdoc/>
	public LoadoutChange Rename(LoadoutSet loadouts, string name, string newName)
	{
		if (loadouts.ReadFailed || loadouts.Find(name) is not { } loadout)
			return LoadoutChange.Missing;
		if (this.ValidateName(loadouts, newName, loadout.Name) is var error and not LoadoutNameError.None)
			return LoadoutChange.Refused(error);

		return this.Write(loadouts, [.. loadouts.Loadouts.Select(l => l == loadout ? l with { Name = newName.Trim() } : l)]);
	}

	/// <inheritdoc/>
	public LoadoutChange Copy(LoadoutSet loadouts, string name, string newName)
	{
		if (loadouts.ReadFailed || loadouts.Find(name) is not { } loadout)
			return LoadoutChange.Missing;
		if (this.ValidateName(loadouts, newName) is var error and not LoadoutNameError.None)
			return LoadoutChange.Refused(error);

		// The copy has its own items: each search entry and each load works on its own pieces.
		var copy = new Loadout(newName.Trim(), loadout.UseAlternate, [.. loadout.Pieces.Select(p => p?.Clone())]);
		return this.Write(loadouts, [.. loadouts.Loadouts, copy]);
	}

	/// <inheritdoc/>
	public LoadoutChange Delete(LoadoutSet loadouts, string name)
	{
		if (loadouts.ReadFailed || loadouts.Find(name) is not { } loadout)
			return LoadoutChange.Missing;

		return this.Write(loadouts, [.. loadouts.Loadouts.Where(l => l != loadout)]);
	}

	/// <summary>A loadout of the worn equipment: copies of the pieces, and the weapon set in hand.</summary>
	private static Loadout Record(string name, PlayerCollection player)
		=> new(name, player.UseAlternate, [.. player.EquipmentSack!.Select(item => ItemKey.IsEmptyItem(item) ? null : item.Clone())]);

	/// <summary>Writes the loadouts (or deletes the file when none is left), then keeps them for the session.</summary>
	private LoadoutChange Write(LoadoutSet target, IReadOnlyList<Loadout> list)
	{
		try
		{
			if (list.Count == 0)
			{
				if (this.FileIO.Exists(target.FilePath))
					this.FileIO.Delete(target.FilePath);
			}
			else
			{
				var dto = new LoadoutFileDto { version = FileVersion, loadouts = [.. list.Select(ToDto)] };
				this.FileIO.WriteAllText(target.FilePath, JsonSerializer.Serialize(dto, JsonOptions));
			}
		}
		catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
		{
			this.Log.LogError(ex, "Failed to write the loadouts {File}", target.FilePath);
			return LoadoutChange.Failed(ex);
		}

		target.Loadouts = list;
		this.Changed(target);
		return LoadoutChange.Succeeded;
	}

	/// <summary>Once the loadouts of a character are read or changed: the search shows their pieces.</summary>
	private void Changed(LoadoutSet loadouts) => this.ItemDatabaseService.AddLoadoutsToDatabase(loadouts);

	private static LoadoutDto ToDto(Loadout loadout)
		=> new()
		{
			name = loadout.Name,
			useAlternate = loadout.UseAlternate,
			pieces = [.. loadout.Pieces
				.Select((item, slot) => (item, slot))
				.Where(p => p.item is not null)
				.Select(p => new LoadoutPieceDto { slot = p.slot, item = ItemDtoExtensions.FromItem(p.item!, ownMapOnly: true) })],
		};

	private Loadout FromDto(LoadoutDto dto, string? characterMod)
	{
		int slots = dto.pieces.Count == 0 ? 0 : dto.pieces.Max(p => p.slot) + 1;
		var pieces = new Item?[Math.Max(slots, 0)];
		foreach (var piece in dto.pieces.Where(p => p.slot >= 0 && p.item is not null))
		{
			var item = piece.item!.ToItem();
			// A piece without its own map is the character's, as its worn items.
			if (item.ModOrigin == ModOrigin.None)
				item.InheritMod(characterMod);
			item.InVault = false;
			item.PositionX = -1;
			item.PositionY = -1;
			this.ItemProvider.GetDBData(item);
			pieces[piece.slot] = item;
		}

		return new Loadout(dto.name ?? string.Empty, dto.useAlternate, pieces);
	}
}

/// <summary>The loadouts file.</summary>
internal sealed class LoadoutFileDto
{
	public int version { get; set; } = LoadoutService.FileVersion;

	public List<LoadoutDto> loadouts { get; set; } = [];
}

/// <summary>A loadout in the file: its pieces by slot (empty slots left out).</summary>
internal sealed class LoadoutDto
{
	public string? name { get; set; }

	public bool useAlternate { get; set; }

	public List<LoadoutPieceDto> pieces { get; set; } = [];
}

/// <summary>A piece of a loadout: its slot and the item, written like the item exports.</summary>
internal sealed class LoadoutPieceDto
{
	public int slot { get; set; }

	public ItemDto? item { get; set; }
}
