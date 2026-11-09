using System.Drawing;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;

namespace TQVaultAE.Application;

public sealed class PlayerSave : IDisposable
{
	public FileSystemWatcher? PlayerSaveWatcher { get; set; }
	public FileSystemWatcher? PlayerStashWatcher { get; set; }

	public readonly Dictionary<string, Color> Tags = [];

	public string Folder;
	public readonly string Name;
	public bool IsArchived;
	public readonly bool IsCustom;
	/// <summary>
	/// The custom map (mod) folder the character is played with; empty for none.
	/// </summary>
	public string CustomMap { get; set; }

	/// <summary>
	/// The custom map's folder name.
	/// </summary>
	public string CustomMapName => string.IsNullOrEmpty(this.CustomMap) ? string.Empty : this.PathIO.GetFileName(this.CustomMap);

	/// <summary>
	/// For a custom map character, the label of its mod in the character list (its linked mod, or that it has none).
	/// </summary>
	public string? ModLabel { get; set; }

	/// <summary>
	/// How the character relates to the installed mods, set with <see cref="ModLabel"/>.
	/// </summary>
	public CharacterModState ModState { get; set; }

	/// <summary>
	/// The name of the character's mod: its linked mod, installed or not; null when it has none.
	/// </summary>
	public string? ModName { get; set; }

	/// <summary>
	/// The class name, translated when the character was read, with its own map's text: a mod can rename the classes,
	/// and the active map may be another character's. Used while the class is still <c>Tag</c>.
	/// </summary>
	public (string Tag, string Text)? ClassLabel { get; set; }
	readonly ITranslationService Translate;
	private readonly IPathIO PathIO;
	public PlayerInfo? Info;
	public readonly bool IsImmortalThrone;

	public PlayerSave(string folder, bool isImmortalThrone, bool isArchived, bool isCustom, string customMap, ITranslationService translate, IPathIO pathIO)
	{
		Folder = folder;
		PathIO = pathIO;
		Translate = translate;
		// Copy the names over without the '_' and strip out the path information.
		Name = PathIO.GetFileName(folder).Substring(1);
		IsCustom = isCustom;
		CustomMap = customMap;
		IsImmortalThrone = isImmortalThrone;
		IsArchived = isArchived;
	}

	/// <summary>
	/// Identifies the character without ambiguity: the game, the kind of save (main campaign or custom map), then the
	/// name, e.g. <c>IT/Main/HeroA</c> and <c>IT/User/HeroA</c> for two characters both named HeroA. Archiving does not
	/// change it.
	/// </summary>
	public string Key => MakeKey(this.IsImmortalThrone, this.IsCustom, this.Name);

	/// <summary>
	/// Builds a <see cref="Key"/>. A character name cannot hold a <c>/</c> (it is a folder name).
	/// </summary>
	public static string MakeKey(bool isImmortalThrone, bool isCustom, string name)
		=> $"{(isImmortalThrone ? "IT" : "TQ")}/{(isCustom ? "User" : "Main")}/{name}";

	public void Dispose()
	{
		if (PlayerSaveWatcher is not null)
			PlayerSaveWatcher.Dispose();
	}

	public override string ToString()
	{
		return string.Join(string.Empty, new[] {
			Info is null ? Name : $"{Name}"
			, Info?.Class is null ? string.Empty : $", {(ClassLabel is { } label && label.Tag == Info.Class ? label.Text : Translate.TranslateXTag(Info.Class, true, true))}"
			, !string.IsNullOrWhiteSpace(Info?.Class) && Info?.CurrentLevel is not null ?  " -" : string.Empty
			, Info?.CurrentLevel is null ? string.Empty : $" {Translate.TranslateXTag("tagMenuImport05")} : {Info.CurrentLevel}"
			, IsImmortalThrone ? string.Empty : " (TQ)"
			, IsCustom && !string.IsNullOrEmpty(ModLabel) ? $" [{ModLabel}]" : string.Empty
		});
	}
}