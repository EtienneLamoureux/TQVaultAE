using System.Drawing;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;

namespace TQVaultAE.Application.Session;

public sealed class PlayerSave : IDisposable
{
	private FileSystemWatcher? _playerSaveWatcher;
	private FileSystemWatcher? _playerStashWatcher;

	/// <summary>
	/// Watches the player file for the hot reload; owned by the save, which disposes it, and the one it replaces.
	/// </summary>
	public FileSystemWatcher? PlayerSaveWatcher
	{
		get => this._playerSaveWatcher;
		set => Replace(ref this._playerSaveWatcher, value);
	}

	/// <summary>
	/// Watches the player stash file for the hot reload; owned by the save, which disposes it, and the one it replaces.
	/// </summary>
	public FileSystemWatcher? PlayerStashWatcher
	{
		get => this._playerStashWatcher;
		set => Replace(ref this._playerStashWatcher, value);
	}

	private static void Replace(ref FileSystemWatcher? field, FileSystemWatcher? value)
	{
		if (!ReferenceEquals(field, value))
			field?.Dispose();
		field = value;
	}

	private IEnumerable<FileSystemWatcher> Watchers
		=> new[] { this._playerSaveWatcher, this._playerStashWatcher }.OfType<FileSystemWatcher>();

	/// <summary>
	/// Stops the change events of the watched files until disposed, while TQVault moves the save folder; then each
	/// watcher raises them again if it did before.
	/// </summary>
	public IDisposable PauseWatching()
	{
		var paused = this.Watchers.Where(w => w.EnableRaisingEvents).ToList();
		foreach (var watcher in paused)
			watcher.EnableRaisingEvents = false;

		return new WatchingResumer(paused);
	}

	private sealed class WatchingResumer(List<FileSystemWatcher> paused) : IDisposable
	{
		public void Dispose()
		{
			foreach (var watcher in paused)
				watcher.EnableRaisingEvents = true;
			paused.Clear();
		}
	}

	/// <summary>
	/// The save folder moved (archived or unarchived): the save and its watchers follow it.
	/// </summary>
	public void MovedTo(string folder, bool isArchived)
	{
		this.Folder = folder;
		this.IsArchived = isArchived;
		foreach (var watcher in this.Watchers)
			watcher.Path = folder;
	}

	public readonly Dictionary<string, Color> Tags = [];

	public string Folder;
	public readonly string Name;
	public bool IsArchived;

	/// <summary>
	/// Whether the character is read-only: one of the files TQVault writes for it carries the read-only attribute
	/// (<c>ICharacterReadOnlyService</c>). Read with the character list and at each load, changed by the toggle.
	/// </summary>
	public bool IsReadOnly { get; set; }

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
	/// How the character relates to the installed mods, set with <see cref="ModName"/>. The UI labels it
	/// (<c>ModLinkText.CharacterLabel</c> in Presentation).
	/// </summary>
	public CharacterModState ModState { get; set; }

	/// <summary>
	/// The name of the character's mod: its linked mod, installed or not; null when it has none.
	/// </summary>
	public string? ModName { get; set; }

	/// <summary>
	/// The class name, translated with the character's own map's text when it is read and each time it is described
	/// (<c>IPlayerService.DescribeCharacter</c>): a mod can rename the classes, and the active map may be another
	/// character's. Used while the class is still <c>Tag</c>.
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
		this.PlayerSaveWatcher = null;
		this.PlayerStashWatcher = null;
	}

	public override string ToString()
	{
		// The class the save or the base game's masteries give, else the one its own map's trees gave (a mod's masteries).
		var classTag = Info is null ? null : Info.ClassTag is { Length: > 0 } tag ? tag : ClassLabel?.Tag;
		bool hasClass = classTag is { Length: > 0 };
		return string.Join(string.Empty, new[] {
			Info is null ? Name : $"{Name}"
			, !hasClass ? string.Empty : $", {(ClassLabel is { } label && label.Tag == classTag ? label.Text : Translate.TranslateXTag(classTag!, true, true))}"
			, hasClass && Info?.CurrentLevel is not null ?  " -" : string.Empty
			, Info?.CurrentLevel is null ? string.Empty : $" {Translate.TranslateXTag("tagMenuImport05")} : {Info.CurrentLevel}"
			, IsImmortalThrone ? string.Empty : " (TQ)"
		});
	}
}