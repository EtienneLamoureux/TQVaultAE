using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;

namespace TQVaultAE.Services;

/// <inheritdoc cref="ICharacterReadOnlyService"/>
public class CharacterReadOnlyService(
	ILogger<CharacterReadOnlyService> log
	, SessionContext userContext
	, IGamePathService gamePathResolver
	, IFileIO fileIO
	, IPathIO pathIO
) : ICharacterReadOnlyService
{
	private readonly ILogger Log = log;
	private readonly SessionContext UserContext = userContext;
	private readonly IGamePathService GamePathResolver = gamePathResolver;
	private readonly IFileIO FileIO = fileIO;
	private readonly IPathIO PathIO = pathIO;

	/// <inheritdoc/>
	public IReadOnlyList<string> OwnedFiles(string saveFolder)
		=> [
			this.PlayerFile(saveFolder),
			this.StashFile(saveFolder),
			this.PathIO.Combine(saveFolder, this.GamePathResolver.PlayerStashFileNameG),
			this.PathIO.Combine(saveFolder, IModLinkService.LinkFileName),
			this.PathIO.Combine(saveFolder, ILoadoutService.FileName),
		];

	/// <inheritdoc/>
	public bool IsReadOnly(string saveFolder)
	{
		foreach (var file in this.OwnedFiles(saveFolder))
		{
			try
			{
				if (this.FileIO.Exists(file) && this.FileIO.IsReadOnly(file))
					return true;
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				// A file that cannot even be read for its attributes is not written either.
				this.Log.LogWarning(ex, "Failed to read the attributes of {File}", file);
				return true;
			}
		}

		return false;
	}

	/// <inheritdoc/>
	public ReadOnlyChangeResult SetReadOnly(PlayerSave save, bool makeReadOnly)
	{
		var player = this.Loaded(this.UserContext.Players, this.PlayerFile(save.Folder));
		var stash = this.Loaded(this.UserContext.Stashes, this.StashFile(save.Folder));

		if (makeReadOnly && ((player?.IsModified ?? false) || (stash?.IsModified ?? false)))
			return new ReadOnlyChangeResult(ReadOnlyChange.UnsavedChanges, this.IsReadOnly(save.Folder));

		Exception? error = null;
		try
		{
			this.SetAttribute(save.Folder, makeReadOnly);
		}
		catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
		{
			this.Log.LogError(ex, "Failed to set the read-only state of the character {Name} to {ReadOnly}", save.Name, makeReadOnly);
			error = ex;
		}

		// The state the files are really in, even after a partial failure.
		bool isReadOnly = this.IsReadOnly(save.Folder);
		save.IsReadOnly = isReadOnly;
		if (player is not null)
			player.IsReadOnly = isReadOnly;
		if (stash is not null)
			stash.IsReadOnly = isReadOnly;

		return error is null
			? new ReadOnlyChangeResult(ReadOnlyChange.Changed, isReadOnly)
			: new ReadOnlyChangeResult(ReadOnlyChange.Failed, isReadOnly, error);
	}

	/// <inheritdoc/>
	public void ClearReadOnly(string saveFolder) => this.SetAttribute(saveFolder, false);

	private void SetAttribute(string saveFolder, bool makeReadOnly)
	{
		foreach (var file in this.OwnedFiles(saveFolder))
		{
			if (this.FileIO.Exists(file))
				this.FileIO.SetReadOnly(file, makeReadOnly);
		}
	}

	private string PlayerFile(string saveFolder) => this.PathIO.Combine(saveFolder, this.GamePathResolver.PlayerSaveFileName);

	private string StashFile(string saveFolder) => this.PathIO.Combine(saveFolder, this.GamePathResolver.PlayerStashFileNameB);

	/// <summary>The value of a file already read in the session, or null.</summary>
	private T? Loaded<T>(Domain.Helpers.LazyConcurrentDictionary<string, T> files, string file) where T : class
		=> files.TryGetValue(file, out var entry) && entry.IsValueCreated ? entry.Value : null;
}
