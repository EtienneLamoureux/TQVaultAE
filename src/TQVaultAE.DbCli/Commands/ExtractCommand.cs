//-----------------------------------------------------------------------
// <copyright file="ExtractCommand.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Concurrent;
using System.Diagnostics;
using TQVaultAE.DbCli.Core;
using TQVaultAE.DbCli.Export;
using TQVaultAE.DbCli.Query;

namespace TQVaultAE.DbCli.Commands;

/// <summary><c>tqdb extract</c>: copy archive entries to disk, optionally converting textures.</summary>
internal sealed class ExtractCommand : ICommand
{
	private const int MaxReportedErrors = 20;

	private readonly ConcurrentDictionary<string, byte> _createdDirectories = new(StringComparer.OrdinalIgnoreCase);

	/// <inheritdoc/>
	public CommandSpec Spec { get; } = new(
		"extract",
		"Extract archive entries (sounds, meshes, textures...) to a folder.",
		[
			.. FilesCommand.SelectorOptions,
			new("out", 'o', "dir", "Destination folder (required)."),
			new("convert", null, "mode", "Textures (.tex): none (raw copy, default) | dds | png."),
			new("frames", null, "mode", "Animated textures with --convert: first (default) | all (name_frameN)."),
			new("verify", null, null, "Verify Adler-32 checksums while extracting."),
			new("parallel", null, "N", "Concurrent extractions (default --threads)."),
			new("flat", null, null, "Write <out>/<name> instead of <out>/<archive>/<name>; for a name in several archives, the last archive wins."),
			new("skip-existing", null, null, "Do not overwrite files that already exist (raw copies with the same size)."),
		],
		[],
		[
			"tqdb extract --archive Items --ext tex --convert png --out ./textures",
			"tqdb extract --archive \"Audio/*\" --ext mp3,wav --out ./sounds --verify",
			"tqdb extract --resource \"Creatures\\monster\\*.msh\" --out ./meshes --flat",
		]) {
		Description = "Raw byte copies are the native form of every asset (mp3, wav, msh, anm, fnt...); only .tex can be converted.\n"
			+ "Output paths: <out>/<archive key>/<entry name>, e.g. out/Resources/XPack/Items/weapons/x.tex. Names are sanitized: '..' and\n"
			+ "rooted names are rejected, invalid characters replaced by '_'. Progress goes to stderr; the exit code is 1 when any entry failed.",
	};

	/// <inheritdoc/>
	public int Execute(ParsedArgs args, CliContext context) {
		var outDir = args.Get("out") ?? throw new UsageException("--out <dir> is required.", "extract");
		var convert = args.GetChoice("convert", "none", "none", "dds", "png");
		bool allFrames = args.GetChoice("frames", "first", "first", "all") == "all";
		bool verify = args.Has("verify");
		bool flat = args.Has("flat");
		bool skipExisting = args.Has("skip-existing");
		int parallel = args.GetInt("parallel", context.Threads, 1, 256);
		var selection = FilesCommand.BuildSelection(args);

		var work = new List<(string Key, ArcEntry Entry)>();
		foreach (var (key, archive) in FilesCommand.OpenSelected(context, selection)) {
			foreach (var e in archive.Entries) {
				if (selection.MatchesEntry(e))
					work.Add((key, e));
			}
		}

		if (work.Count == 0) {
			context.Info("No entry matched.");
			return ExitCodes.Ok;
		}

		int replaced = 0;
		if (flat) {
			// Same name in several archives: one target file. Keep the last archive's entry (archives are in path order,
			// so XPack, XPack2... win over the base game) instead of letting parallel writers race on the file.
			var lastByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < work.Count; i++)
				lastByName[work[i].Entry.Name.Replace('\\', '/')] = i;
			replaced = work.Count - lastByName.Count;
			if (replaced > 0)
				work = [.. lastByName.Values.Order().Select(i => work[i])];
		}

		long totalBytes = work.Sum(w => w.Entry.Size);
		if (replaced > 0)
			context.Info($"--flat: {replaced} entr{(replaced == 1 ? "y" : "ies")} replaced by a later archive with the same name.");

		Directory.CreateDirectory(outDir);
		var stats = new Stats();
		var errors = new ConcurrentQueue<string>();
		var watch = Stopwatch.StartNew();
		context.Info($"Extracting {work.Count} entr{(work.Count == 1 ? "y" : "ies")} ({FormatBytes(totalBytes)}) to {Path.GetFullPath(outDir)} with {parallel} worker(s)...");

		using var progress = context.Quiet || Console.IsErrorRedirected
			? null
			: new Timer(_ => context.Error.Write($"\r  {Volatile.Read(ref stats.Done)}/{work.Count} files, {FormatBytes(Interlocked.Read(ref stats.Bytes))}   "), null, 500, 500);

		try {
			Parallel.ForEach(work, new ParallelOptions { MaxDegreeOfParallelism = parallel, CancellationToken = context.CancellationToken }, item => {
				try {
					var relative = flat ? item.Entry.Name : item.Key + "/" + item.Entry.Name;
					var target = PathSanitizer.Combine(outDir, relative);
					this.EnsureDirectory(Path.GetDirectoryName(target)!);

					bool isTex = string.Equals(Path.GetExtension(item.Entry.Name), ".tex", StringComparison.OrdinalIgnoreCase);
					if (convert != "none" && isTex)
						this.ExtractTexture(context, item.Entry, target, convert, allFrames, verify, stats);
					else
						ExtractRaw(item.Entry, target, verify, skipExisting, stats);
				}
				catch (Exception ex) when (ex is not OperationCanceledException) {
					Interlocked.Increment(ref stats.Failed);
					errors.Enqueue($"{item.Key}/{item.Entry.Name}: {ex.Message}");
				}
				finally {
					Interlocked.Increment(ref stats.Done);
				}
			});
		}
		finally {
			progress?.Dispose();
			if (progress is not null)
				context.Error.Write("\r" + new string(' ', 60) + "\r");
		}

		int shown = 0;
		foreach (var e in errors) {
			if (shown++ == MaxReportedErrors) {
				context.Error.WriteLine($"error: ... and {errors.Count - MaxReportedErrors} more");
				break;
			}
			context.Error.WriteLine("error: " + e);
		}

		context.Info($"Done: {stats.Files} file(s) written ({FormatBytes(stats.Bytes)}), {stats.Skipped} skipped, "
			+ $"{stats.ConversionFailed} conversion fallback(s), {stats.Failed} failed, {watch.Elapsed.TotalSeconds:0.0}s.");
		return stats.Failed > 0 ? ExitCodes.Failure : ExitCodes.Ok;
	}

	private static void ExtractRaw(ArcEntry entry, string target, bool verify, bool skipExisting, Stats stats) {
		if (skipExisting) {
			var fi = new FileInfo(target);
			if (fi.Exists && fi.Length == entry.Size) {
				Interlocked.Increment(ref stats.Skipped);
				return;
			}
		}

		try {
			using (var fs = new FileStream(target, FileMode.Create, FileAccess.Write, FileShare.None, Formats.BufferSize)) {
				entry.Archive.CopyTo(entry, fs, verify);
			}
			File.SetLastWriteTimeUtc(target, entry.LastWrite);
		}
		catch {
			TryDelete(target);
			throw;
		}

		Interlocked.Increment(ref stats.Files);
		Interlocked.Add(ref stats.Bytes, entry.Size);
	}

	private void ExtractTexture(CliContext context, ArcEntry entry, string target, string convert, bool allFrames, bool verify, Stats stats) {
		byte[] tex;
		if (verify) {
			using var ms = new MemoryStream((int)Math.Min(entry.Size, int.MaxValue));
			entry.Archive.CopyTo(entry, ms, verify: true);
			tex = ms.ToArray();
		}
		else {
			tex = entry.Archive.ReadAll(entry);
		}

		var ext = convert == "png" ? ".png" : ".dds";
		try {
			int frames = allFrames ? Math.Max(1, TexConverter.FrameCount(tex)) : 1;
			for (int f = 0; f < frames; f++) {
				var data = convert == "png" ? TexConverter.ToPng(tex, f) : TexConverter.ToDds(tex, f);
				var path = frames == 1
					? Path.ChangeExtension(target, ext)
					: Path.Combine(Path.GetDirectoryName(target)!, $"{Path.GetFileNameWithoutExtension(target)}_frame{f}{ext}");
				File.WriteAllBytes(path, data);
				Interlocked.Increment(ref stats.Files);
				Interlocked.Add(ref stats.Bytes, data.Length);
			}
		}
		catch (Exception ex) when (ex is not OperationCanceledException and not IOException and not UnauthorizedAccessException) {
			// Unsupported texture: keep the raw bytes so nothing is lost.
			context.Warn($"{entry.ResourceId}: {convert} conversion failed ({ex.Message}); raw .tex written.");
			File.WriteAllBytes(target, tex);
			Interlocked.Increment(ref stats.ConversionFailed);
			Interlocked.Increment(ref stats.Files);
			Interlocked.Add(ref stats.Bytes, tex.Length);
		}
	}

	private void EnsureDirectory(string dir) {
		if (this._createdDirectories.ContainsKey(dir))
			return;
		Directory.CreateDirectory(dir);
		this._createdDirectories.TryAdd(dir, 0);
	}

	private static void TryDelete(string path) {
		try {
			File.Delete(path);
		}
		catch (IOException) {
		}
		catch (UnauthorizedAccessException) {
		}
	}

	/// <summary>Human-readable size.</summary>
	internal static string FormatBytes(long bytes) => bytes switch {
		< 1024 => $"{bytes} B",
		< 1024 * 1024 => $"{bytes / 1024.0:0.0} KB",
		< 1024L * 1024 * 1024 => $"{bytes / (1024.0 * 1024):0.0} MB",
		_ => $"{bytes / (1024.0 * 1024 * 1024):0.00} GB",
	};

	private sealed class Stats
	{
		public int Done;
		public int Files;
		public int Skipped;
		public int Failed;
		public int ConversionFailed;
		public long Bytes;
	}
}
