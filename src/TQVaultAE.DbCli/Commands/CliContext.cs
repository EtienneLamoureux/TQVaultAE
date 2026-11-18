//-----------------------------------------------------------------------
// <copyright file="CliContext.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Concurrent;
using TQVaultAE.DbCli.Core;
using TQVaultAE.DbCli.Export;
using TQVaultAE.DbCli.Query;

namespace TQVaultAE.DbCli.Commands;

/// <summary>Not-found condition (record id, game install...). Maps to exit code 3.</summary>
internal sealed class NotFoundException(string message) : Exception(message);

/// <summary>Shared state of one invocation: global options, lazily opened game data, output streams.</summary>
internal sealed class CliContext : IDisposable
{
	private readonly ConcurrentDictionary<string, ITextDatabase> _texts = new(StringComparer.OrdinalIgnoreCase);
	private readonly ConcurrentDictionary<string, Lazy<IArcArchive>> _archives = new(StringComparer.OrdinalIgnoreCase);
	private readonly Lock _lock = new();
	private GameInstall? _install;
	private IArzDatabase? _database;

	public CliContext(ParsedArgs args, TextWriter error, CancellationToken cancellationToken) {
		this.GamePath = args.Get("game");
		this.Quiet = args.Has("quiet");
		this.Threads = args.GetInt("threads", Environment.ProcessorCount, 1, 1024);
		this.Error = error;
		this.CancellationToken = cancellationToken;
	}

	/// <summary><c>--game</c>.</summary>
	public string? GamePath { get; }

	/// <summary><c>--quiet</c>: no progress or informational messages.</summary>
	public bool Quiet { get; }

	/// <summary><c>--threads</c>: parallelism of decoding and archive opening.</summary>
	public int Threads { get; }

	/// <summary>stderr.</summary>
	public TextWriter Error { get; }

	/// <summary>Ctrl+C.</summary>
	public CancellationToken CancellationToken { get; }

	/// <summary>Located install (lazy).</summary>
	public GameInstall Install {
		get {
			if (this._install is not null)
				return this._install;
			lock (this._lock) {
				try {
					return this._install ??= GameLocator.Locate(this.GamePath);
				}
				catch (DirectoryNotFoundException ex) {
					throw new NotFoundException(NotFoundMessage(ex.Message, this.GamePath));
				}
			}
		}
	}

	/// <summary>
	/// Message of a failed game search: with --game, only that folder is tried, so the locator's reason alone;
	/// otherwise the reason and how to give the folder.
	/// </summary>
	internal static string NotFoundMessage(string locatorMessage, string? gamePath)
		=> string.IsNullOrWhiteSpace(gamePath)
			? $"{locatorMessage.TrimEnd('.')}. Use --game <path> or set {GameLocator.EnvironmentVariable}."
			: locatorMessage;

	/// <summary>database.arz (lazy).</summary>
	public IArzDatabase Database {
		get {
			if (this._database is not null)
				return this._database;
			lock (this._lock)
				return this._database ??= ArzDatabaseFactory.Open(this.Install.Database);
		}
	}

	/// <summary>Text database of a language (lazy, cached).</summary>
	public ITextDatabase Text(string language) => this._texts.GetOrAdd(language, lang => {
		var path = this.Install.TextArchive(lang) ?? throw new NotFoundException($"No text archive for language '{lang}' (expected Text\\Text_{lang.ToUpperInvariant()}.arc). See 'tqdb info' for available languages.");
		return TextDatabaseFactory.Load(path, lang.ToUpperInvariant());
	});

	/// <summary>Record namer for a language.</summary>
	public IRecordNamer Namer(string language) => new CoreRecordNamer(this.Text(language));

	/// <summary>Opens (once) an archive.</summary>
	public IArcArchive Archive(string path) => this._archives.GetOrAdd(path, p => new Lazy<IArcArchive>(() => ArcArchiveFactory.Open(p))).Value;

	/// <summary>Opens archives in parallel (bounded by <see cref="Threads"/>), keeping input order; failures are reported and skipped.</summary>
	public IReadOnlyList<IArcArchive> OpenArchives(IReadOnlyList<string> paths) {
		var result = new IArcArchive?[paths.Count];
		Parallel.For(0, paths.Count, new ParallelOptions { MaxDegreeOfParallelism = this.Threads, CancellationToken = this.CancellationToken }, i => {
			try {
				result[i] = this.Archive(paths[i]);
			}
			catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException) {
				this.Warn($"cannot open {paths[i]}: {ex.Message}");
			}
		});
		var list = new List<IArcArchive>(paths.Count);
		foreach (var a in result)
			if (a is not null)
				list.Add(a);
		return list;
	}

	/// <summary>Archive path relative to the game root, '/' separated, without extension (e.g. <c>Resources/XPack/Items</c>).</summary>
	public string ArchiveKey(string archivePath) {
		var rel = Path.GetRelativePath(this.Install.Root, archivePath);
		if (rel.StartsWith("..", StringComparison.Ordinal) || Path.IsPathRooted(rel))
			rel = Path.GetFileName(archivePath);
		return Path.ChangeExtension(rel, null).Replace('\\', '/');
	}

	/// <summary>Opens the output: a file (directories created) or buffered stdout.</summary>
	public static Stream OpenOutput(string? path) {
		if (string.IsNullOrEmpty(path) || path == "-")
			return new BufferedStream(Console.OpenStandardOutput(), Formats.BufferSize);
		var dir = Path.GetDirectoryName(Path.GetFullPath(path));
		if (!string.IsNullOrEmpty(dir))
			Directory.CreateDirectory(dir);
		return new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, Formats.BufferSize);
	}

	/// <summary>Text writer over <see cref="OpenOutput"/>.</summary>
	public static StreamWriter OpenTextOutput(string? path) => new(OpenOutput(path), Formats.Utf8, Formats.BufferSize);

	/// <summary>Informational message on stderr unless quiet.</summary>
	public void Info(string message) {
		if (!this.Quiet)
			this.Error.WriteLine(message);
	}

	/// <summary>Warning on stderr (always).</summary>
	public void Warn(string message) {
		lock (this.Error)
			this.Error.WriteLine("warning: " + message);
	}

	/// <inheritdoc/>
	public void Dispose() {
		this._database?.Dispose();
		foreach (var a in this._archives.Values)
			if (a.IsValueCreated)
				a.Value.Dispose();
	}
}
