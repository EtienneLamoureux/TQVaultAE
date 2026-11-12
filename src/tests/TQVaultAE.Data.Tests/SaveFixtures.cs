using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Data;

/// <summary>
/// The real save file providers wired together, with only the game database mocked,
/// and the save files of the test character (<c>Data/SaveFiles</c>).
/// </summary>
/// <remarks>
/// Without the game database the items have no <see cref="Item.baseItemInfo"/>: they are read and written
/// from their record ids only, which is all the save code uses (potions and scrolls are told by their id).
/// </remarks>
internal sealed class SaveFixtures : IDisposable
{
	public static readonly string SaveFilesFolder = Path.Combine(AppContext.BaseDirectory, "Data", "SaveFiles");

	public static string PlayerChr => Path.Combine(SaveFilesFolder, "Player.chr");

	public static string StashDxb => Path.Combine(SaveFilesFolder, "winsys.dxb");

	public static string StashDxg => Path.Combine(SaveFilesFolder, "winsys.dxg");

	public static string VaultJson => Path.Combine(SaveFilesFolder, "claude-vault1.vault.json");

	public Mock<IDatabase> Database { get; } = new();

	public TQDataService TQData { get; } = new(NullLogger<TQDataService>.Instance);

	public ItemProvider ItemProvider { get; }

	public SackCollectionProvider SackProvider { get; }

	public PlayerCollectionProvider PlayerProvider { get; }

	public StashProvider StashProvider { get; }

	/// <summary>Folder of the files written by a test, deleted with the fixture.</summary>
	public string TempFolder { get; } = Path.Combine(Path.GetTempPath(), "TQVaultAE.Tests", Guid.NewGuid().ToString("N"));

	public SaveFixtures()
	{
		var pathIO = new PathIO();
		var fileIO = new FileIO();
		this.ItemProvider = new ItemProvider(
			NullLogger<ItemProvider>.Instance,
			this.Database.Object,
			new Mock<ILootTableCollectionProvider>().Object,
			new Mock<IItemAttributeProvider>().Object,
			this.TQData,
			new Mock<ITranslationService>().Object,
			pathIO,
			new UserSettings());
		this.SackProvider = new SackCollectionProvider(NullLogger<SackCollectionProvider>.Instance, this.ItemProvider, this.TQData);
		this.PlayerProvider = new PlayerCollectionProvider(NullLogger<PlayerCollectionProvider>.Instance, this.ItemProvider, this.SackProvider, this.TQData, fileIO, pathIO);
		this.StashProvider = new StashProvider(NullLogger<StashProvider>.Instance, this.SackProvider, this.TQData, fileIO, pathIO);
		Directory.CreateDirectory(this.TempFolder);
	}

	public string TempFile(string name) => Path.Combine(this.TempFolder, name);

	/// <summary>Loads a character file the way <c>PlayerService</c> does.</summary>
	public PlayerCollection LoadPlayer(string? path = null)
	{
		var pc = new PlayerCollection("claude-character1", path ?? PlayerChr) { IsImmortalThrone = true };
		this.PlayerProvider.LoadFile(pc);
		return pc;
	}

	/// <summary>Loads a vault file (json or legacy binary) the way <c>VaultService</c> does.</summary>
	public PlayerCollection LoadVault(string path)
	{
		var pc = new PlayerCollection("claude-vault1", path) { IsVault = true, IsImmortalThrone = true };
		this.PlayerProvider.LoadFile(pc);
		return pc;
	}

	/// <summary>Loads a stash file the way <c>StashService</c> does.</summary>
	public Stash LoadStash(string? path = null)
	{
		var stash = new Stash("claude-character1", path ?? StashDxb);
		this.StashProvider.LoadFile(stash).Should().BeTrue();
		return stash;
	}

	/// <summary>Re-reads an encoded character from bytes.</summary>
	public PlayerCollection Reload(byte[] chr)
	{
		var path = this.TempFile($"{Guid.NewGuid():N}{Path.DirectorySeparatorChar}Player.chr");
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		File.WriteAllBytes(path, chr);
		return this.LoadPlayer(path);
	}

	public void Dispose()
	{
		try
		{
			Directory.Delete(this.TempFolder, true);
		}
		catch (IOException)
		{
			// A file still open by a failed test: the temp folder is cleaned by the system.
		}
	}
}
