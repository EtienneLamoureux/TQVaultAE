namespace TQVaultAE.Domain.Entities;

/// <summary>
/// The kind of game database (.arz) a record was read from.
/// </summary>
public enum RecordArchiveKind
{
	/// <summary>The base game's database.</summary>
	BaseGame = 0,

	/// <summary>Immortal Throne's database, when it is a separate file.</summary>
	ImmortalThrone,

	/// <summary>The database of a mod (a custom map), whose records win over the game's.</summary>
	Mod,
}

/// <summary>
/// The database a record was read from: the base game, Immortal Throne, or a mod. Kept on the read record
/// (<see cref="DBRecordCollection.Source"/>), never on its <see cref="RecordId"/>, which is the same in every map.
/// </summary>
/// <param name="Kind">The kind of database.</param>
/// <param name="Mod">For a mod's database, the mod's identity (<see cref="ModIdentity"/>); else null.</param>
public sealed record RecordSource(RecordArchiveKind Kind, string? Mod = null)
{
	/// <summary>A record of the base game.</summary>
	public static readonly RecordSource BaseGame = new(RecordArchiveKind.BaseGame);

	/// <summary>A record of Immortal Throne's own database.</summary>
	public static readonly RecordSource ImmortalThrone = new(RecordArchiveKind.ImmortalThrone);

	/// <summary>A record of a mod's database.</summary>
	/// <param name="mod">The mod's identity (<see cref="ModIdentity"/>).</param>
	public static RecordSource FromMod(string mod) => new(RecordArchiveKind.Mod, mod);

	/// <summary>True for a record read from a mod's database: a record of its own, or one it overrides.</summary>
	public bool IsMod => this.Kind == RecordArchiveKind.Mod;
}
