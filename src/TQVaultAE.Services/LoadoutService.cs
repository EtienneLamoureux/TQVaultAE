using System.Text.Json;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Application.Session;
using TQVaultAE.Data.Dto;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services.Export;

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

		// Pieces moved in the loadout panel and not saved yet stay: a reload of the character does not undo them.
		if (this.Session.Loadouts.TryGetValue(file, out var kept) && kept.IsModified)
			return kept;

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
	public SetNameError ValidateName(LoadoutSet loadouts, string? name, string? current = null)
		=> SetNames.Validate(loadouts.Loadouts.Select(l => l.Name), name, current);

	/// <inheritdoc/>
	public LoadoutChange Add(LoadoutSet loadouts, string name)
	{
		if (this.ValidateName(loadouts, name) is var error and not SetNameError.None)
			return LoadoutChange.Refused(error);
		if (loadouts.ReadFailed)
			return LoadoutChange.Missing;

		var empty = new Loadout(name.Trim(), false, new Item?[EquipmentRules.ArtifactSlot + 1]);
		return this.Write(loadouts, [.. loadouts.Loadouts, empty]);
	}

	/// <inheritdoc/>
	public LoadoutChange Update(LoadoutSet loadouts, string name, PlayerCollection player)
	{
		if (loadouts.ReadFailed || loadouts.Find(name) is not { } loadout || player.EquipmentSack is null)
			return LoadoutChange.Missing;

		var worn = player.EquipmentSack.Select(item => ItemKey.IsEmptyItem(item) ? null : item.Clone()).ToList();
		var merged = new Loadout(loadout.Name, player.UseAlternate, EquipmentRules.MergeWorn(loadout.Pieces, worn));
		return this.Write(loadouts, [.. loadouts.Loadouts.Select(l => l == loadout ? merged : l)]);
	}

	/// <inheritdoc/>
	public LoadoutChange Rename(LoadoutSet loadouts, string name, string newName)
	{
		if (loadouts.ReadFailed || loadouts.Find(name) is not { } loadout)
			return LoadoutChange.Missing;
		if (this.ValidateName(loadouts, newName, loadout.Name) is var error and not SetNameError.None)
			return LoadoutChange.Refused(error);

		return this.Write(loadouts, [.. loadouts.Loadouts.Select(l => l == loadout ? l with { Name = newName.Trim() } : l)]);
	}

	/// <inheritdoc/>
	public LoadoutChange Copy(LoadoutSet loadouts, string name, string newName)
	{
		if (loadouts.ReadFailed || loadouts.Find(name) is not { } loadout)
			return LoadoutChange.Missing;
		if (this.ValidateName(loadouts, newName) is var error and not SetNameError.None)
			return LoadoutChange.Refused(error);

		// The copy has its own items: each search entry and each gear swap works on its own pieces.
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

	/// <inheritdoc/>
	public LoadoutChange TakePieces(LoadoutSet loadouts, IReadOnlyCollection<Item> pieces)
	{
		if (loadouts.ReadFailed)
			return LoadoutChange.Missing;

		var taken = new HashSet<Item>(pieces, ReferenceEqualityComparer.Instance);
		return this.Write(loadouts, [.. loadouts.Loadouts.Select(l => l.Pieces.Any(p => p is not null && taken.Contains(p))
			? l with { Pieces = [.. l.Pieces.Select(p => p is not null && taken.Contains(p) ? null : p)] }
			: l)]);
	}

	/// <inheritdoc/>
	public (LoadoutChange Change, IReadOnlyList<Item> PutBack) PutBackPieces(LoadoutSet loadouts, IReadOnlyList<LoadoutPiecePlacement> pieces)
	{
		if (loadouts.ReadFailed)
			return (LoadoutChange.Missing, []);

		var list = loadouts.Loadouts.ToList();
		var putBack = new List<Item>();
		foreach (var (slot, piece) in pieces)
		{
			int index = list.FindIndex(l => string.Equals(l.Name, slot.Loadout, StringComparison.OrdinalIgnoreCase));
			if (index < 0 || slot.Slot < 0)
				continue;

			var slots = list[index].Pieces.ToList();
			while (slots.Count <= slot.Slot)
				slots.Add(null);
			if (slots[slot.Slot] is not null)
				continue;

			// As read from the file: in no bag, with no cell.
			piece.PositionX = -1;
			piece.PositionY = -1;
			slots[slot.Slot] = piece;
			list[index] = list[index] with { Pieces = slots };
			putBack.Add(piece);
		}

		if (putBack.Count == 0)
			return (LoadoutChange.Succeeded, []);

		var change = this.Write(loadouts, [.. list]);
		return (change, change.IsSuccess ? putBack : []);
	}

	/// <inheritdoc/>
	public bool Compose(LoadoutSet loadouts, string name, IReadOnlyList<Item?> pieces)
	{
		if (loadouts.ReadFailed || loadouts.Find(name) is not { } loadout)
			return false;

		int slots = Math.Max(loadout.Pieces.Count, pieces.Count);
		bool same = Enumerable.Range(0, slots).All(slot => ReferenceEquals(PieceAt(loadout.Pieces, slot), PieceAt(pieces, slot)));
		if (same)
			return false;

		var composed = loadout with { Pieces = [.. pieces.Select(p => p is null || ItemKey.IsEmptyItem(p) ? null : p)] };
		loadouts.Loadouts = [.. loadouts.Loadouts.Select(l => l == loadout ? composed : l)];
		loadouts.IsModified = true;
		this.Changed(loadouts);
		return true;
	}

	/// <inheritdoc/>
	public IReadOnlyList<LoadoutSet> GetModified()
		=> [.. this.Session.Loadouts.Values.Where(set => set.IsModified && !set.ReadFailed)];

	/// <inheritdoc/>
	public void SaveModified(LoadoutSet loadouts)
	{
		if (!loadouts.IsModified || loadouts.ReadFailed)
			return;

		this.WriteFile(loadouts.FilePath, loadouts.Loadouts);
		loadouts.IsModified = false;
	}

	/// <summary>The piece of a slot, null for an empty slot or one past the list.</summary>
	private static Item? PieceAt(IReadOnlyList<Item?> pieces, int slot)
		=> slot < pieces.Count && pieces[slot] is { } piece && !ItemKey.IsEmptyItem(piece) ? piece : null;

	/// <summary>
	/// Writes the loadouts (or deletes the file when none is left), then keeps them for the session. The pieces composed in
	/// the loadout panel and not saved yet are written too: the file holds the loadouts as they are now.
	/// </summary>
	private LoadoutChange Write(LoadoutSet target, IReadOnlyList<Loadout> list)
	{
		try
		{
			this.WriteFile(target.FilePath, list);
		}
		catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
		{
			this.Log.LogError(ex, "Failed to write the loadouts {File}", target.FilePath);
			return LoadoutChange.Failed(ex);
		}

		target.Loadouts = list;
		target.IsModified = false;
		this.Changed(target);
		return LoadoutChange.Succeeded;
	}

	/// <summary>Writes a loadouts file, or deletes it when no loadout is left; file errors are thrown.</summary>
	private void WriteFile(string file, IReadOnlyList<Loadout> list)
	{
		if (list.Count == 0)
		{
			if (this.FileIO.Exists(file))
				this.FileIO.Delete(file);
			return;
		}

		var dto = new LoadoutFileDto { version = FileVersion, loadouts = [.. list.Select(ToDto)] };
		this.FileIO.WriteAllText(file, JsonSerializer.Serialize(dto, JsonOptions));
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
				.Select(p => new LoadoutPieceDto { slot = p.slot, item = ItemDtoExtensions.FromItem(p.item!) })],
		};

	private Loadout FromDto(LoadoutDto dto, string? characterMod)
	{
		int slots = dto.pieces.Count == 0 ? 0 : dto.pieces.Max(p => p.slot) + 1;
		var pieces = new Item?[Math.Max(slots, 0)];
		foreach (var piece in dto.pieces.Where(p => p.slot >= 0 && p.item is not null))
		{
			var item = piece.item!.ToItem();
			// A piece is read with the character's map, as its worn items.
			item.SetContainerMod(characterMod);
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
