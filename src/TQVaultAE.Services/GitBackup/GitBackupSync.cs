using Microsoft.Extensions.Logging;

namespace TQVaultAE.Services.GitBackup;

/// <summary>
/// A local folder backed up in the git repository.
/// </summary>
/// <param name="RepositoryPath">Folder in the repository, with <c>/</c> separators, e.g. <c>TQVaultData</c>.</param>
/// <param name="LocalPath">The real folder on this computer.</param>
/// <param name="Includes">Whether a file, given by its path relative to the folder with <c>/</c> separators, is backed up.</param>
public sealed record GitBackupFolder(string RepositoryPath, string LocalPath, Func<string, bool> Includes);

/// <summary>
/// Synchronizes local folders (vaults, character saves) with a git repository, through a local clone that holds copies.
/// </summary>
/// <remarks>
/// <para>git only ever works inside the clone: the real folders are read and written by this class alone, file by file.
/// (An earlier version linked the real folders into the clone with NTFS junctions; a pulled deletion made git remove the
/// junction itself, and the next commit deleted every vault from the backup. Junctions left by that version are removed.)</para>
/// <para>Rules:</para>
/// <list type="bullet">
/// <item>The changes made on this computer since the last sync are committed first, so nothing is ever lost locally.</item>
/// <item>A file changed only on one side takes that side's version; a deletion is a change.</item>
/// <item>A file changed on both sides keeps the most recent save. git does not keep file dates, so every commit carries
/// <see cref="SaveTimesFile"/>, the real write time of each file; a backup made before it falls back to the commit time.
/// A modification wins over a deletion.</item>
/// <item>A file written from the backup gets its original write time back.</item>
/// <item>Files are taken whole, never merged as text.</item>
/// <item>A local folder that is empty while the backup holds files is restored from the backup instead of emptying it.</item>
/// </list>
/// </remarks>
public sealed class GitBackupSync
{
	private const string RemoteBranch = "@{u}";

	/// <summary>
	/// File at the root of the repository with the real write time of each backed up file: one
	/// <c>milliseconds since 1970 (UTC) TAB repository path</c> line per file.
	/// </summary>
	public const string SaveTimesFile = ".tqvault-savetimes";

	private readonly ILogger _log;
	private readonly string _repositoryDirectory;
	private readonly string _remoteUrl;
	private readonly IReadOnlyList<GitBackupFolder> _folders;
	private readonly GitRunner _git;

	public GitBackupSync(ILogger log, string repositoryDirectory, string remoteUrl, IReadOnlyList<GitBackupFolder> folders)
	{
		this._log = log;
		this._repositoryDirectory = repositoryDirectory;
		this._remoteUrl = remoteUrl;
		this._folders = folders;
		this._git = new GitRunner(repositoryDirectory);
	}

	/// <summary>
	/// Whether the local clone exists.
	/// </summary>
	public bool IsCloned => Directory.Exists(Path.Combine(this._repositoryDirectory, ".git"));

	/// <summary>
	/// Startup sync: clones the repository the first time, commits the local changes, then brings in the remote changes.
	/// </summary>
	/// <exception cref="GitCommandException">A git command failed.</exception>
	/// <exception cref="System.ComponentModel.Win32Exception">git cannot be started.</exception>
	public void Synchronize()
	{
		if (!this.IsCloned)
		{
			this.Clone();
			this.EnsureIdentity();
			this.ReconcileFreshClone();
		}
		else
		{
			this.PrepareExistingClone();
			this.CommitLocalChanges("TQVaultAE: changes made on this computer");
		}
		this.MergeRemote();
	}

	/// <summary>
	/// Exit sync: commits and tags the local changes, brings in what the remote received meanwhile, and pushes.
	/// </summary>
	/// <returns><c>true</c> when something was pushed.</returns>
	/// <exception cref="GitCommandException">A git command failed.</exception>
	/// <exception cref="System.ComponentModel.Win32Exception">git cannot be started.</exception>
	public bool CommitAndPush()
	{
		if (!this.IsCloned)
			return false;

		this.PrepareExistingClone();
		var committed = this.CommitLocalChanges("TQVaultAE update!");
		this.MergeRemote();

		if (!this.HasCommit())
			return false;

		// Something to push: a commit of this exit, or one an earlier sync could not push.
		var hasUpstream = this.HasUpstream();
		if (hasUpstream && !committed && this.AheadBehind().Ahead == 0)
			return false;

		var tag = DateTime.Now.ToString("yy.MM.dd.HHmmss");
		this._git.RunChecked("tag", tag);
		this._git.RunChecked("tag", "-f", "latest");

		if (hasUpstream)
			this._git.RunChecked("push", "origin", "HEAD");
		else
			this._git.RunChecked("push", "-u", "origin", "HEAD");
		this._git.RunChecked("push", "-f", "origin", "latest");
		this._git.RunChecked("push", "origin", tag);
		return true;
	}

	#region Clone

	private void Clone()
	{
		Directory.CreateDirectory(this._repositoryDirectory);
		var parent = Path.GetDirectoryName(Path.GetFullPath(this._repositoryDirectory))!;
		new GitRunner(parent).RunChecked("clone", this._remoteUrl, this._repositoryDirectory);
		this._log.LogInformation("Git backup cloned from {Url}", this._remoteUrl);
	}

	private void PrepareExistingClone()
	{
		this.RemoveLegacyJunctions();
		this.EnsureRemoteUrl();
		this.EnsureIdentity();
	}

	/// <summary>
	/// Removes the junctions an earlier version put in the clone. Deleting a junction never touches the folder it points to.
	/// </summary>
	private void RemoveLegacyJunctions()
	{
		foreach (var folder in this._folders)
		{
			var path = Path.Combine(this._repositoryDirectory, folder.RepositoryPath);
			var info = new DirectoryInfo(path);
			if (info.Exists && info.Attributes.HasFlag(FileAttributes.ReparsePoint))
			{
				Directory.Delete(path, recursive: false);
				// Put back the folder as last committed, so the files this class does not manage (the legacy Backups
				// folder, for example) are not committed as deleted.
				if (this.HasCommit())
					this._git.Run("checkout", "HEAD", "--", folder.RepositoryPath);
				this._log.LogInformation("Git backup: removed the junction {Path} left by an earlier version", path);
			}
		}
	}

	/// <summary>
	/// Points the clone at the repository of the settings, which the user may have changed since the clone.
	/// </summary>
	private void EnsureRemoteUrl()
	{
		var current = this._git.Run("remote", "get-url", "origin");
		if (current.Success && current.Output.Trim() == this._remoteUrl)
			return;

		// Remove and add rather than set-url: that also drops what the clone knew of the old repository's branches,
		// so the first push to the new one sends everything.
		if (current.Success)
			this._git.RunChecked("remote", "remove", "origin");
		this._git.RunChecked("remote", "add", "origin", this._remoteUrl);
		this._log.LogInformation("Git backup: origin set to {Url}", this._remoteUrl);
	}

	/// <summary>
	/// Gives the clone a committer name when git has none, since a commit fails without one.
	/// </summary>
	private void EnsureIdentity()
	{
		if (this._git.Run("config", "user.email").Success)
			return;

		this._git.RunChecked("config", "user.name", "TQVaultAE");
		this._git.RunChecked("config", "user.email", "tqvaultae@localhost");
	}

	/// <summary>
	/// First sync of this computer: the clone holds the remote files and the local folders were never synced, so no file
	/// is a deletion. Files only on one side are kept, files on both sides keep the most recent save.
	/// </summary>
	private void ReconcileFreshClone()
	{
		var remoteTimes = this.ReadSaveTimes("HEAD");
		foreach (var folder in this.ExistingFolders())
		{
			var repositoryRoot = this.RepositoryFolder(folder);
			var local = Enumerate(folder.LocalPath, folder.Includes);
			var remote = Enumerate(repositoryRoot, folder.Includes);

			foreach (var (relative, localFile) in local)
			{
				var repositoryFile = Path.Combine(repositoryRoot, relative);
				if (!remote.ContainsKey(relative))
					CopyFile(localFile, repositoryFile);
				else if (!SameContent(localFile, repositoryFile))
				{
					var path = folder.RepositoryPath + '/' + relative;
					if (File.GetLastWriteTimeUtc(localFile) > this.SaveTime(remoteTimes, "HEAD", path))
						CopyFile(localFile, repositoryFile);
					else
						CopyToLocal(repositoryFile, localFile, remoteTimes, path);
					this._log.LogInformation("Git backup: {File} differs, the most recent save is kept", relative);
				}
			}

			foreach (var (relative, repositoryFile) in remote)
			{
				if (!local.ContainsKey(relative))
					CopyToLocal(repositoryFile, Path.Combine(folder.LocalPath, relative), remoteTimes, folder.RepositoryPath + '/' + relative);
			}
		}
		this.CommitStaged("TQVaultAE: first sync of this computer");
	}

	#endregion

	#region Local to repository

	/// <summary>
	/// Copies the local folders into the clone (added, changed and deleted files) and commits.
	/// </summary>
	/// <returns><c>true</c> when a commit was made.</returns>
	private bool CommitLocalChanges(string message)
	{
		foreach (var folder in this.ExistingFolders())
			this.MirrorToRepository(folder);
		return this.CommitStaged(message);
	}

	private void MirrorToRepository(GitBackupFolder folder)
	{
		var repositoryRoot = this.RepositoryFolder(folder);
		var local = Enumerate(folder.LocalPath, folder.Includes);
		var repository = Enumerate(repositoryRoot, folder.Includes);

		if (local.Count == 0 && repository.Count > 0)
		{
			// An empty folder (a new vault path, a reinstall) must not empty the backup: restore it instead.
			this._log.LogWarning("Git backup: {Folder} is empty, restored from the backup ({Count} files)", folder.LocalPath, repository.Count);
			var times = this.ReadSaveTimes("HEAD");
			foreach (var (relative, repositoryFile) in repository)
				CopyToLocal(repositoryFile, Path.Combine(folder.LocalPath, relative), times, folder.RepositoryPath + '/' + relative);
			return;
		}

		foreach (var (relative, localFile) in local)
		{
			var repositoryFile = Path.Combine(repositoryRoot, relative);
			if (!SameFile(localFile, repositoryFile))
				CopyFile(localFile, repositoryFile);
		}

		foreach (var (relative, repositoryFile) in repository)
		{
			if (!local.ContainsKey(relative))
				DeleteFile(repositoryFile, repositoryRoot);
		}
	}

	private bool CommitStaged(string message)
	{
		this.WriteSaveTimes(this.LocalSaveTimes(this.ReadSaveTimes("HEAD")));
		this._git.RunChecked("add", "-A");
		// --quiet exits with 1 when something is staged.
		if (this._git.Run("diff", "--cached", "--quiet").Success)
			return false;

		this._git.RunChecked("commit", "-q", "-m", message);
		return true;
	}

	#endregion

	#region Remote to local

	/// <summary>
	/// Fetches, merges the remote commits by the rules of this class, and writes the files they changed to the local folders.
	/// </summary>
	private void MergeRemote()
	{
		this._git.RunChecked("fetch", "origin");
		this.EnsureUpstream();
		if (!this.HasUpstream() || !this.HasCommit())
			return;

		var (ahead, behind) = this.AheadBehind();
		if (behind == 0)
			return;

		var before = this._git.RunChecked("rev-parse", "HEAD").Output.Trim();
		if (ahead == 0)
			this._git.RunChecked("merge", "-q", "--ff-only", RemoteBranch);
		else
			this.MergeDiverged();

		this.ApplyToLocal(before);
	}

	/// <summary>
	/// Both sides have commits: keep this side, then take each file the remote changed unless the local version wins.
	/// </summary>
	private void MergeDiverged()
	{
		var mergeBase = this._git.RunChecked("merge-base", "HEAD", RemoteBranch).Output.Trim();
		var localChanges = this.NameStatus(mergeBase, "HEAD");
		var remoteChanges = this.NameStatus(mergeBase, RemoteBranch);
		var localTimes = this.ReadSaveTimes("HEAD");
		var remoteTimes = this.ReadSaveTimes(RemoteBranch);
		var mergedTimes = new Dictionary<string, long>(localTimes, StringComparer.Ordinal);

		this._git.RunChecked("merge", "-q", "-s", "ours", "--no-commit", "--no-ff", RemoteBranch);
		try
		{
			foreach (var (path, remoteStatus) in remoteChanges)
			{
				if (path == SaveTimesFile)
					continue;

				bool takeRemote;
				if (!localChanges.TryGetValue(path, out var localStatus))
					takeRemote = true;
				else if (remoteStatus == 'D')
					takeRemote = false; // Deleted there: a local modification wins, a local deletion is the same.
				else if (localStatus == 'D')
					takeRemote = true; // Modified there, deleted here: the modification wins.
				else
				{
					takeRemote = this.SaveTime(remoteTimes, RemoteBranch, path) > this.LocalWriteTime(path);
					this._log.LogInformation("Git backup: {File} changed on both computers, the {Side} save is the most recent", path, takeRemote ? "remote" : "local");
				}

				if (!takeRemote)
					continue;
				if (remoteStatus == 'D')
				{
					this._git.RunChecked("rm", "-q", "--", path);
					mergedTimes.Remove(path);
				}
				else
				{
					this._git.RunChecked("checkout", RemoteBranch, "--", path);
					if (remoteTimes.TryGetValue(path, out var time))
						mergedTimes[path] = time;
					else
						mergedTimes.Remove(path);
				}
			}
			this.WriteSaveTimes(mergedTimes);
			this._git.RunChecked("add", "--", SaveTimesFile);
			this._git.RunChecked("commit", "-q", "-m", "TQVaultAE: merge the changes of another computer");
		}
		catch
		{
			this._git.Run("merge", "--abort");
			throw;
		}
	}

	/// <summary>
	/// Writes to the local folders the files that changed in the clone since <paramref name="before"/>.
	/// </summary>
	private void ApplyToLocal(string before)
	{
		var changes = this.NameStatus(before, "HEAD");
		var times = this.ReadSaveTimes("HEAD");
		var written = 0;
		foreach (var (path, status) in changes)
		{
			if (!this.TryMapToLocal(path, out var folder, out var localFile))
				continue;

			if (status == 'D')
				DeleteFile(localFile, folder.LocalPath);
			else
				CopyToLocal(Path.Combine(this._repositoryDirectory, path), localFile, times, path);
			written++;
		}
		this._log.LogInformation("Git backup: {Count} local files updated from the remote", written);
	}

	#endregion

	#region git queries

	/// <summary>
	/// A changed repository URL drops the upstream (<see cref="EnsureRemoteUrl"/>): when the new repository already has
	/// this branch, follow it again, so its commits are merged before the push instead of the push being refused.
	/// </summary>
	private void EnsureUpstream()
	{
		if (this.HasUpstream() || !this.HasCommit())
			return;

		var branch = this._git.RunChecked("rev-parse", "--abbrev-ref", "HEAD").Output.Trim();
		if (this._git.Run("rev-parse", "--verify", "-q", "refs/remotes/origin/" + branch).Success)
			this._git.RunChecked("branch", "--set-upstream-to=origin/" + branch);
	}

	private bool HasUpstream() => this._git.Run("rev-parse", "--verify", "-q", RemoteBranch).Success;

	private bool HasCommit() => this._git.Run("rev-parse", "--verify", "-q", "HEAD").Success;

	private (int Ahead, int Behind) AheadBehind()
	{
		// Counts, not the text of git status, which git writes in the user's language.
		var counts = this._git.RunChecked("rev-list", "--left-right", "--count", "HEAD..." + RemoteBranch).Output
			.Split((char[])['\t', ' ', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
		return (int.Parse(counts[0]), int.Parse(counts[1]));
	}

	/// <summary>
	/// Files changed between two commits: path → A (added), M (modified) or D (deleted).
	/// </summary>
	private Dictionary<string, char> NameStatus(string from, string to)
	{
		var fields = this._git.RunChecked("diff", "--name-status", "--no-renames", "-z", from, to).Output
			.Split('\0', StringSplitOptions.RemoveEmptyEntries);
		var changes = new Dictionary<string, char>(StringComparer.Ordinal);
		for (var i = 0; i + 1 < fields.Length; i += 2)
			changes[fields[i + 1]] = fields[i][0] == 'D' ? 'D' : fields[i][0] == 'A' ? 'A' : 'M';
		return changes;
	}

	private DateTime LastCommitTime(string revision, string path)
	{
		var output = this._git.RunChecked("log", "-1", "--format=%ct", revision, "--", path).Output.Trim();
		return long.TryParse(output, out var seconds)
			? DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime
			: DateTime.MinValue;
	}

	private DateTime LocalWriteTime(string path)
	{
		var file = this.TryMapToLocal(path, out _, out var localFile) && File.Exists(localFile)
			? localFile
			: Path.Combine(this._repositoryDirectory, path);
		return File.Exists(file) ? File.GetLastWriteTimeUtc(file) : DateTime.MinValue;
	}

	#endregion

	#region Save times

	private Dictionary<string, long> ReadSaveTimes(string revision)
	{
		var times = new Dictionary<string, long>(StringComparer.Ordinal);
		var result = this._git.Run("show", revision + ':' + SaveTimesFile);
		if (!result.Success)
			return times;

		foreach (var line in result.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
		{
			var parts = line.TrimEnd('\r').Split('\t', 2);
			if (parts.Length == 2 && long.TryParse(parts[0], out var milliseconds))
				times[parts[1]] = milliseconds;
		}
		return times;
	}

	private void WriteSaveTimes(Dictionary<string, long> times)
	{
		var lines = times.OrderBy(t => t.Key, StringComparer.Ordinal).Select(t => $"{t.Value}\t{t.Key}\n");
		File.WriteAllText(Path.Combine(this._repositoryDirectory, SaveTimesFile), string.Concat(lines));
	}

	/// <summary>
	/// The save times of the local files, keeping the previous ones of the folders that do not exist on this computer.
	/// </summary>
	private Dictionary<string, long> LocalSaveTimes(Dictionary<string, long> previous)
	{
		var times = new Dictionary<string, long>(previous, StringComparer.Ordinal);
		foreach (var folder in this.ExistingFolders())
		{
			var prefix = folder.RepositoryPath + '/';
			foreach (var path in times.Keys.Where(p => p.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList())
				times.Remove(path);
			foreach (var (relative, localFile) in Enumerate(folder.LocalPath, folder.Includes))
				times[prefix + relative] = new DateTimeOffset(File.GetLastWriteTimeUtc(localFile)).ToUnixTimeMilliseconds();
		}
		return times;
	}

	/// <summary>
	/// When a file of <paramref name="revision"/> was saved: its recorded save time, else the time of its last commit.
	/// </summary>
	private DateTime SaveTime(Dictionary<string, long> times, string revision, string path)
		=> times.TryGetValue(path, out var milliseconds)
			? DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).UtcDateTime
			: this.LastCommitTime(revision, path);

	/// <summary>
	/// Copies a file of the clone to a local folder, with its recorded save time when there is one.
	/// </summary>
	private static void CopyToLocal(string repositoryFile, string localFile, Dictionary<string, long> times, string path)
	{
		CopyFile(repositoryFile, localFile);
		if (times.TryGetValue(path, out var milliseconds))
			File.SetLastWriteTimeUtc(localFile, DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).UtcDateTime);
	}

	#endregion

	#region Files

	private IEnumerable<GitBackupFolder> ExistingFolders()
	{
		foreach (var folder in this._folders)
		{
			if (Directory.Exists(folder.LocalPath))
				yield return folder;
			else
				this._log.LogInformation("Git backup: {Folder} does not exist, skipped", folder.LocalPath);
		}
	}

	private string RepositoryFolder(GitBackupFolder folder)
		=> Path.Combine(this._repositoryDirectory, folder.RepositoryPath.Replace('/', Path.DirectorySeparatorChar));

	/// <summary>
	/// Maps a repository path to its file in a backed up local folder that exists.
	/// </summary>
	private bool TryMapToLocal(string repositoryPath, out GitBackupFolder folder, out string localFile)
	{
		foreach (var candidate in this._folders)
		{
			var prefix = candidate.RepositoryPath + '/';
			if (!repositoryPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
				continue;

			var relative = repositoryPath[prefix.Length..];
			if (!candidate.Includes(relative) || !Directory.Exists(candidate.LocalPath))
				break;

			folder = candidate;
			localFile = Path.Combine(candidate.LocalPath, relative.Replace('/', Path.DirectorySeparatorChar));
			return true;
		}
		folder = null!;
		localFile = string.Empty;
		return false;
	}

	/// <summary>
	/// The backed up files of a folder: relative path with <c>/</c> separators → full path.
	/// </summary>
	private static Dictionary<string, string> Enumerate(string root, Func<string, bool> includes)
	{
		var files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		if (!Directory.Exists(root))
			return files;

		foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
		{
			var relative = Path.GetRelativePath(root, file).Replace(Path.DirectorySeparatorChar, '/');
			if (includes(relative))
				files[relative] = file;
		}
		return files;
	}

	/// <summary>
	/// Same size and write time: copies keep the write time, so an unchanged file is not read.
	/// </summary>
	private static bool SameFile(string a, string b)
	{
		var infoA = new FileInfo(a);
		var infoB = new FileInfo(b);
		return infoB.Exists && infoA.Length == infoB.Length && infoA.LastWriteTimeUtc == infoB.LastWriteTimeUtc;
	}

	private static bool SameContent(string a, string b)
		=> new FileInfo(a).Length == new FileInfo(b).Length && File.ReadAllBytes(a).AsSpan().SequenceEqual(File.ReadAllBytes(b));

	private static void CopyFile(string source, string destination)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
		File.Copy(source, destination, overwrite: true);
	}

	/// <summary>
	/// Deletes a file and the folders it leaves empty, up to <paramref name="root"/>.
	/// </summary>
	private static void DeleteFile(string file, string root)
	{
		if (File.Exists(file))
			File.Delete(file);

		var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar);
		var directory = Path.GetDirectoryName(Path.GetFullPath(file));
		while (directory is not null
			&& directory.Length > rootFull.Length
			&& directory.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase)
			&& !Directory.EnumerateFileSystemEntries(directory).Any())
		{
			Directory.Delete(directory);
			directory = Path.GetDirectoryName(directory);
		}
	}

	#endregion
}
