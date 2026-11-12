namespace TQVaultAE.Application.DTOs;

/// <summary>
/// Where an installed mod (custom map) was found.
/// </summary>
public enum ModSource
{
	/// <summary>
	/// <c>My Games\Titan Quest - Immortal Throne\CustomMaps\&lt;mod&gt;</c>.
	/// </summary>
	Legacy,

	/// <summary>
	/// <c>steamapps\workshop\content\475150\&lt;item id&gt;\&lt;mod&gt;</c>.
	/// </summary>
	SteamWorkshop,
}

/// <summary>
/// A mod (custom map) installed on this computer.
/// </summary>
/// <param name="Name">The mod's folder name: the game names the mod's stash folder (<c>SaveData\Sys\&lt;Name&gt;</c>)
/// and its database (<c>database\&lt;Name&gt;.arz</c>) after it.</param>
/// <param name="Folder">The mod's folder, the custom map path used for its data.</param>
/// <param name="Source">Where it was found.</param>
public sealed record InstalledMod(string Name, string Folder, ModSource Source)
{
	/// <summary>
	/// The name shown in lists, with its source.
	/// </summary>
	public string DisplayName => this.Source == ModSource.SteamWorkshop
		? $"Steam Workshop : {this.Name}"
		: $"Legacy : {this.Name}";

	public override string ToString() => this.DisplayName;
}

/// <summary>
/// Content of the link file written in a modded character's save folder: the mod the character plays.
/// </summary>
public sealed class ModLinkFile
{
	/// <summary>
	/// Version of the file format, for later changes.
	/// </summary>
	public int Version { get; set; } = 1;

	/// <summary>
	/// The mod's folder name (<see cref="InstalledMod.Name"/>), the key that finds the mod again when it moved
	/// (another Steam library, another computer).
	/// </summary>
	public string Mod { get; set; } = string.Empty;

	/// <summary>
	/// The mod's folder when the link was made, preferred when it still exists.
	/// </summary>
	public string? Folder { get; set; }

	/// <summary>
	/// When the link was made (UTC).
	/// </summary>
	public DateTime LinkedUtc { get; set; }
}

/// <summary>
/// How a character relates to the installed mods.
/// </summary>
public enum CharacterModState
{
	/// <summary>
	/// A main campaign character (<c>SaveData\Main</c>): base game data.
	/// </summary>
	BaseGame,

	/// <summary>
	/// A custom map character linked to an installed mod.
	/// </summary>
	Linked,

	/// <summary>
	/// A custom map character linked to a mod that is not installed (any more).
	/// </summary>
	LinkedModMissing,

	/// <summary>
	/// A custom map character without a link.
	/// </summary>
	Unlinked,
}

/// <summary>
/// The mod of a character, as resolved from its link file.
/// </summary>
/// <param name="State">How the character relates to the installed mods.</param>
/// <param name="Mod">The linked mod, when installed.</param>
/// <param name="Link">The link file, when there is one.</param>
public sealed record CharacterMod(CharacterModState State, InstalledMod? Mod, ModLinkFile? Link)
{
	public static readonly CharacterMod BaseGame = new(CharacterModState.BaseGame, null, null);

	public static readonly CharacterMod Unlinked = new(CharacterModState.Unlinked, null, null);

	/// <summary>
	/// Whether a character can be linked to <paramref name="selected"/>: a custom map character, a mod chosen, not the
	/// one it is linked to already.
	/// </summary>
	/// <param name="isCustomCharacter">True for a custom map character; the main campaign is never linked.</param>
	/// <param name="selected">The mod chosen, or <see langword="null"/>.</param>
	public bool CanLinkTo(bool isCustomCharacter, InstalledMod? selected)
		=> isCustomCharacter && selected is not null && !Equals(selected, this.Mod);
}

/// <summary>
/// How well an installed mod explains the records of a character.
/// </summary>
/// <param name="Mod">The mod.</param>
/// <param name="Matched">Number of the character's mod-only records found in the mod's database.</param>
public sealed record ModMatch(InstalledMod Mod, int Matched);

/// <summary>
/// The mods that could be the one of a character, from the records its save uses that the base game does not have.
/// </summary>
/// <param name="ModOnlyRecords">Number of distinct records of the save that the base game database does not have.</param>
/// <param name="Matches">Mods having at least one of those records, best first.</param>
public sealed record ModSuggestion(int ModOnlyRecords, IReadOnlyList<ModMatch> Matches)
{
	/// <summary>
	/// The best match, when one mod has more of the records than any other.
	/// </summary>
	public ModMatch? Best => this.Matches.Count switch
	{
		0 => null,
		1 => this.Matches[0],
		_ => this.Matches[0].Matched > this.Matches[1].Matched ? this.Matches[0] : null,
	};

	/// <summary>
	/// True when the save only uses base game records: no mod can be told from its data.
	/// </summary>
	public bool OnlyBaseGameRecords => this.ModOnlyRecords == 0;

	/// <summary>
	/// The mods tied first, with as many records as the first match; empty without a match.
	/// </summary>
	public IReadOnlyList<ModMatch> Tied
		=> this.Matches.Count == 0 ? [] : [.. this.Matches.TakeWhile(m => m.Matched == this.Matches[0].Matched)];
}
