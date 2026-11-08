using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TQVaultAE.Services.GitBackup;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// Runs <see cref="GitBackupSync"/> against a real bare repository in a temp folder, with two "computers" that each have
/// their own clone, vault folder and save folder.
/// </summary>
public sealed class GitBackupSyncTests : IDisposable
{
	private const string VaultRepositoryPath = "TQVaultData";
	private const string SaveRepositoryPath = "Titan Quest - Immortal Throne/SaveData";

	private readonly string _root = Path.Combine(Path.GetTempPath(), "tqvault-gitbackup-" + Guid.NewGuid().ToString("N")[..8]);
	private readonly string _remote;

	public GitBackupSyncTests()
	{
		Directory.CreateDirectory(this._root);
		this._remote = Path.Combine(this._root, "remote.git");
		new GitRunner(this._root).RunChecked("init", "-q", "--bare", "-b", "main", this._remote);
	}

	public void Dispose()
	{
		if (!Directory.Exists(this._root))
			return;
		// git writes its objects read-only.
		foreach (var file in Directory.EnumerateFiles(this._root, "*", SearchOption.AllDirectories))
			File.SetAttributes(file, FileAttributes.Normal);
		Directory.Delete(this._root, recursive: true);
	}

	[Fact]
	public void FirstSync_KeepsFilesOfBothComputers()
	{
		// Arrange
		var a = this.Computer("a");
		a.WriteVault("v1", "a1");
		a.Sync.Synchronize();
		a.Sync.CommitAndPush().Should().BeTrue();

		var b = this.Computer("b");
		b.WriteVault("v9", "b9");

		// Act
		b.Sync.Synchronize();
		b.Sync.CommitAndPush();
		a.Sync.Synchronize();

		// Assert
		b.ReadVault("v1").Should().Be("a1");
		b.ReadVault("v9").Should().Be("b9");
		a.ReadVault("v9").Should().Be("b9");
		this.RemoteFiles().Should().BeEquivalentTo(Vault("v1"), Vault("v9"));
	}

	[Fact]
	public void FirstSync_WhenBothComputersHaveTheFile_KeepsTheMostRecentSave()
	{
		// Arrange
		var a = this.Computer("a");
		a.WriteVault("v1", "remote");
		a.Sync.Synchronize();
		a.Sync.CommitAndPush();

		var b = this.Computer("b");
		b.WriteVault("v1", "old local", DateTime.UtcNow.AddDays(-1));
		b.WriteVault("v2", "new local", DateTime.UtcNow.AddHours(1));
		a.WriteVault("v2", "older remote");
		a.Sync.CommitAndPush();

		// Act
		b.Sync.Synchronize();
		b.Sync.CommitAndPush();

		// Assert
		b.ReadVault("v1").Should().Be("remote");
		b.ReadVault("v2").Should().Be("new local");
		this.RemoteFile(Vault("v2")).Should().Be("new local");
	}

	[Fact]
	public void RemoteDeletion_DeletesTheLocalFile_AndKeepsTheOthersInTheBackup()
	{
		// Arrange: the earlier junction version lost every vault from the backup after a pulled deletion.
		var (a, b) = this.TwoSyncedComputers(("v1", "1"), ("v2", "2"), ("v3", "3"));
		File.Delete(b.VaultFile("v3"));
		b.Sync.CommitAndPush();

		// Act
		a.Sync.Synchronize();
		a.Sync.CommitAndPush();

		// Assert
		File.Exists(a.VaultFile("v3")).Should().BeFalse();
		a.ReadVault("v1").Should().Be("1");
		this.RemoteFiles().Should().BeEquivalentTo(Vault("v1"), Vault("v2"));
	}

	[Fact]
	public void RemoteCharacterDeletion_DeletesItsLocalFolder()
	{
		// Arrange
		var a = this.Computer("a");
		a.WriteSave("Main/_hero/Player.chr", "hero");
		a.WriteSave("Main/_other/Player.chr", "other");
		a.Sync.Synchronize();
		a.Sync.CommitAndPush();
		var b = this.Computer("b");
		b.Sync.Synchronize();
		Directory.Delete(Path.Combine(b.Saves, "Main", "_hero"), recursive: true);
		b.Sync.CommitAndPush();

		// Act
		a.Sync.Synchronize();

		// Assert
		Directory.Exists(Path.Combine(a.Saves, "Main", "_hero")).Should().BeFalse();
		File.Exists(Path.Combine(a.Saves, "Main", "_other", "Player.chr")).Should().BeTrue();
	}

	[Theory]
	[InlineData(-24, "remote")]
	[InlineData(1, "local")]
	public void ChangedOnBothComputers_KeepsTheMostRecentSave(int localAgeHours, string winner)
	{
		// Arrange: both computers change v1 between two syncs, the other one pushes first.
		var (a, b) = this.TwoSyncedComputers(("v1", "base"));
		b.WriteVault("v1", "remote");
		b.Sync.CommitAndPush();
		a.WriteVault("v1", "local", DateTime.UtcNow.AddHours(localAgeHours));

		// Act
		var pushed = a.Sync.CommitAndPush();
		b.Sync.Synchronize();

		// Assert
		pushed.Should().BeTrue();
		a.ReadVault("v1").Should().Be(winner);
		b.ReadVault("v1").Should().Be(winner);
		this.RemoteFile(Vault("v1")).Should().Be(winner);
	}

	[Fact]
	public void ChangedOnBothComputers_ComparesSaveTimes_NotPushTimes()
	{
		// Arrange: the other computer saved two days ago but pushes now; this one saved yesterday and pushes after it.
		var (a, b) = this.TwoSyncedComputers(("v1", "base"));
		b.WriteVault("v1", "older save", DateTime.UtcNow.AddDays(-2));
		b.Sync.CommitAndPush();
		a.WriteVault("v1", "newer save", DateTime.UtcNow.AddDays(-1));

		// Act
		a.Sync.CommitAndPush();

		// Assert
		a.ReadVault("v1").Should().Be("newer save");
		this.RemoteFile(Vault("v1")).Should().Be("newer save");
	}

	[Fact]
	public void FileFromTheBackup_GetsItsOriginalSaveTime()
	{
		// Arrange
		var saved = new DateTime(2025, 3, 4, 5, 6, 7, DateTimeKind.Utc);
		var a = this.Computer("a");
		a.WriteVault("v1", "1", saved);
		a.Sync.Synchronize();
		a.Sync.CommitAndPush();
		var b = this.Computer("b");

		// Act
		b.Sync.Synchronize();

		// Assert
		File.GetLastWriteTimeUtc(b.VaultFile("v1")).Should().Be(saved);
	}

	[Fact]
	public void ChangedOnBothComputers_DifferentFiles_KeepsBothChanges()
	{
		// Arrange
		var (a, b) = this.TwoSyncedComputers(("v1", "base1"), ("v2", "base2"));
		b.WriteVault("v1", "b1");
		b.Sync.CommitAndPush();
		a.WriteVault("v2", "a2");

		// Act
		a.Sync.CommitAndPush();
		b.Sync.Synchronize();

		// Assert
		a.ReadVault("v1").Should().Be("b1");
		b.ReadVault("v2").Should().Be("a2");
		this.RemoteFile(Vault("v1")).Should().Be("b1");
		this.RemoteFile(Vault("v2")).Should().Be("a2");
	}

	[Fact]
	public void DeletedThere_ModifiedHere_KeepsTheModification()
	{
		// Arrange
		var (a, b) = this.TwoSyncedComputers(("v1", "base"));
		File.Delete(b.VaultFile("v1"));
		b.Sync.CommitAndPush();
		a.WriteVault("v1", "modified");

		// Act
		a.Sync.CommitAndPush();
		b.Sync.Synchronize();

		// Assert
		a.ReadVault("v1").Should().Be("modified");
		b.ReadVault("v1").Should().Be("modified");
	}

	[Fact]
	public void EmptyLocalFolder_IsRestoredFromTheBackup_InsteadOfEmptyingIt()
	{
		// Arrange
		var (a, _) = this.TwoSyncedComputers(("v1", "1"), ("v2", "2"));
		foreach (var file in Directory.GetFiles(a.Vaults))
			File.Delete(file);

		// Act
		a.Sync.Synchronize();
		a.Sync.CommitAndPush();

		// Assert
		a.ReadVault("v1").Should().Be("1");
		this.RemoteFiles().Should().BeEquivalentTo(Vault("v1"), Vault("v2"));
	}

	[Fact]
	public void Files_AreKeptByteForByte()
	{
		// Arrange: line endings of both kinds and binary bytes, which a line ending conversion would change.
		byte[] content = [0x41, 0x0D, 0x0A, 0x42, 0x0A, 0x00, 0xFF, 0x0D];
		var a = this.Computer("a");
		Directory.CreateDirectory(Path.Combine(a.Saves, "Main", "_hero"));
		File.WriteAllBytes(Path.Combine(a.Saves, "Main", "_hero", "Player.chr"), content);
		a.Sync.Synchronize();
		a.Sync.CommitAndPush();
		var b = this.Computer("b");

		// Act
		b.Sync.Synchronize();

		// Assert
		File.ReadAllBytes(Path.Combine(b.Saves, "Main", "_hero", "Player.chr")).Should().Equal(content);
	}

	[Fact]
	public void FilesTheFolderExcludes_AreNotBackedUp()
	{
		// Arrange
		var a = this.Computer("a");
		a.WriteVault("v1", "1");
		Directory.CreateDirectory(Path.Combine(a.Vaults, "Backups"));
		File.WriteAllText(Path.Combine(a.Vaults, "Backups", "old.vault.json"), "backup");

		// Act
		a.Sync.Synchronize();
		a.Sync.CommitAndPush();

		// Assert
		this.RemoteFiles().Should().BeEquivalentTo(Vault("v1"));
	}

	[Fact]
	public void CloneOfTheJunctionVersion_IsMigrated_WithoutDeletingAnything()
	{
		Assert.SkipUnless(OperatingSystem.IsWindows(), "NTFS junctions");

		// Arrange: a clone as the earlier version left it, its vault folder a junction to the real one, and a tracked file
		// this class does not manage.
		var (a, _) = this.TwoSyncedComputers(("v1", "1"), ("v2", "2"));
		var clone = Path.Combine(this._root, "a", "clone");
		var repositoryVaults = Path.Combine(clone, VaultRepositoryPath);
		Directory.CreateDirectory(Path.Combine(repositoryVaults, "Backups"));
		File.WriteAllText(Path.Combine(repositoryVaults, "Backups", "old.vault.json"), "legacy");
		var git = new GitRunner(clone);
		git.RunChecked("add", "-A");
		git.RunChecked("commit", "-q", "-m", "legacy backup");
		git.RunChecked("push", "-q", "origin", "HEAD");
		Directory.Delete(repositoryVaults, recursive: true);
		// mklink is a cmd built-in.
		System.Diagnostics.Process.Start("cmd.exe", ["/c", "mklink", "/J", repositoryVaults, a.Vaults])!.WaitForExit();
		new DirectoryInfo(repositoryVaults).Attributes.Should().HaveFlag(FileAttributes.ReparsePoint);
		a.WriteVault("v2", "changed");

		// Act
		a.Sync.CommitAndPush();

		// Assert
		new DirectoryInfo(repositoryVaults).Attributes.Should().NotHaveFlag(FileAttributes.ReparsePoint);
		a.ReadVault("v1").Should().Be("1");
		this.RemoteFiles().Should().BeEquivalentTo(Vault("v1"), Vault("v2"), $"{VaultRepositoryPath}/Backups/old.vault.json");
		this.RemoteFile(Vault("v2")).Should().Be("changed");
	}

	[Fact]
	public void CommitAndPush_WithNothingNew_PushesNothing()
	{
		// Arrange
		var (a, _) = this.TwoSyncedComputers(("v1", "1"));

		// Act
		var pushed = a.Sync.CommitAndPush();

		// Assert
		pushed.Should().BeFalse();
	}

	[Fact]
	public void ChangedRepositoryUrl_IsUsedByTheExistingClone()
	{
		// Arrange
		var a = this.Computer("a");
		a.WriteVault("v1", "1");
		a.Sync.Synchronize();
		a.Sync.CommitAndPush();
		var otherRemote = Path.Combine(this._root, "other.git");
		new GitRunner(this._root).RunChecked("init", "-q", "--bare", "-b", "main", otherRemote);
		var moved = this.Computer("a", otherRemote);

		// Act
		moved.Sync.CommitAndPush().Should().BeTrue();

		// Assert
		this.RemoteFiles(otherRemote).Should().BeEquivalentTo(Vault("v1"));
	}

	#region Helpers

	private static string Vault(string name) => $"{VaultRepositoryPath}/{name}.vault.json";

	private (TestComputer A, TestComputer B) TwoSyncedComputers(params (string Name, string Content)[] vaults)
	{
		var a = this.Computer("a");
		foreach (var (name, content) in vaults)
			a.WriteVault(name, content);
		a.Sync.Synchronize();
		a.Sync.CommitAndPush();
		var b = this.Computer("b");
		b.Sync.Synchronize();
		return (a, b);
	}

	private TestComputer Computer(string name, string? remote = null)
	{
		var home = Path.Combine(this._root, name);
		var vaults = Path.Combine(home, "vaults");
		var saves = Path.Combine(home, "SaveData");
		Directory.CreateDirectory(vaults);
		Directory.CreateDirectory(saves);
		var folders = new List<GitBackupFolder>
		{
			new(VaultRepositoryPath, vaults, relative => !relative.Contains('/') && relative.EndsWith(".vault.json", StringComparison.OrdinalIgnoreCase)),
			new(SaveRepositoryPath, saves, _ => true),
		};
		var sync = new GitBackupSync(NullLogger.Instance, Path.Combine(home, "clone"), remote ?? this._remote, folders);
		return new TestComputer(vaults, saves, sync);
	}

	/// <summary>
	/// The backed up files of the remote, without the save times file.
	/// </summary>
	private string[] RemoteFiles(string? remote = null)
		=> new GitRunner(remote ?? this._remote).RunChecked("ls-tree", "-r", "--name-only", "main").Output
			.Split('\n', StringSplitOptions.RemoveEmptyEntries)
			.Where(f => f != GitBackupSync.SaveTimesFile)
			.ToArray();

	private string RemoteFile(string path)
		=> new GitRunner(this._remote).RunChecked("show", "main:" + path).Output;

	private sealed record TestComputer(string Vaults, string Saves, GitBackupSync Sync)
	{
		public string VaultFile(string name) => Path.Combine(this.Vaults, name + ".vault.json");

		public void WriteVault(string name, string content, DateTime? writeTimeUtc = null)
		{
			File.WriteAllText(this.VaultFile(name), content);
			if (writeTimeUtc is not null)
				File.SetLastWriteTimeUtc(this.VaultFile(name), writeTimeUtc.Value);
		}

		public string ReadVault(string name) => File.ReadAllText(this.VaultFile(name));

		public void WriteSave(string relative, string content)
		{
			var file = Path.Combine(this.Saves, relative);
			Directory.CreateDirectory(Path.GetDirectoryName(file)!);
			File.WriteAllText(file, content);
		}
	}

	#endregion
}
