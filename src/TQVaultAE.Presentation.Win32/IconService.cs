using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Drawing;
using System.Text.Json;
using TQVaultAE.Presentation.Models;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Presentation.Win32.Contracts;
using TQVaultAE.Presentation.Win32.Models;

namespace TQVaultAE.Presentation.Win32;

/// <summary>
/// Loads Titan Quest Icons.
/// </summary>
public class IconService : IIconService
{
	private readonly ILogger Log;
	private readonly IDatabase Database;
	private readonly IUIService UIService;
	private readonly IGamePathService GamePathService;
	private readonly JsonSerializerOptions JsonOptions;

	private readonly object _loadLock = new();
	private Task<ReadOnlyCollection<IconInfo>>? _load;

	/// <summary>Icon resource ids, before their bitmaps are loaded.</summary>
	private sealed record Candidate(IconCategory Category, RecordId? On, RecordId? Off, RecordId? Over);

	public IconService(ILogger<IconService> log, IDatabase database, IUIService uiService, IGamePathService gamePathService, JsonSerializerOptions jsonOptions)
	{
		this.Log = log;
		this.Database = database;
		this.UIService = uiService;
		this.GamePathService = gamePathService;
		this.JsonOptions = jsonOptions;
	}

	/// <inheritdoc/>
	public Task<ReadOnlyCollection<IconInfo>> GetIconDatabaseAsync()
	{
		lock (this._loadLock)
		{
			// A failed load is tried again on the next call.
			if (this._load is null || this._load.IsFaulted || this._load.IsCanceled)
				// LongRunning gives a dedicated thread, so its priority can be lowered without touching the thread pool.
				this._load = Task.Factory.StartNew(this.Load, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

			return this._load;
		}
	}

	private ReadOnlyCollection<IconInfo> Load()
	{
		Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
		var watch = Stopwatch.StartNew();
		var configfile = JsonSerializer.Deserialize<ConfRoot>(Resources.IconServiceList, this.JsonOptions)!;
		long configTime = watch.ElapsedMilliseconds;

		// Every file name of the listed archives, read once.
		var keys = configfile.list
			.SelectMany(file => this.KeysOf(file.fileName))
			.ToArray();
		var existing = keys.Select(k => k.ToRecordId()).ToHashSet();
		long keysTime = watch.ElapsedMilliseconds;

		var candidates = this.Match(configfile, keys);
		long matchTime = watch.ElapsedMilliseconds;

		var pictures = new List<IconInfo>(candidates.Count);
		// The first of identical icons is kept.
		var seen = new HashSet<(RecordId?, RecordId?, RecordId?)>();
		foreach (var candidate in candidates)
		{
			// Pictures are decoded when the bag button settings first show them, not here.
			var info = new IconInfo(candidate.Category, candidate.On!, candidate.Off!, candidate.Over!)
			{
				HasOn = Exists(candidate.On),
				HasOff = Exists(candidate.Off),
				HasOver = Exists(candidate.Over),
				BitmapLoader = this.UIService.LoadBitmap,
			};

			if (info.IsEmpty)
				continue;

			// Square only for shields
			if (info.Category == IconCategory.Shields)
			{
				var first = info.HasOn ? info.On : info.HasOff ? info.Off : info.Over;
				if (this.Database.GetTextureSize(first) is not { } size || size.Width != size.Height)
					continue;
			}

			if (!seen.Add((candidate.Off, candidate.On, candidate.Over)))
				continue;

			if (!info.HasOn)
				this.Log.LogWarning(@"ON ""{On}"" not found !", info.On);
			if (!info.HasOff)
				this.Log.LogWarning(@"OFF ""{Off}"" not found !", info.Off);
			if (!info.HasOver)
				this.Log.LogWarning(@"OVER ""{Over}"" not found !", info.Over);

			pictures.Add(info);
		}

		this.Log.LogInformation(
			"Icon database loaded in {Elapsed} ms: {Count} icons, {Keys} archive keys (config {Config} ms, keys {KeysTime} ms, match {Match} ms)"
			, watch.ElapsedMilliseconds, pictures.Count, keys.Length
			, configTime, keysTime - configTime, matchTime - keysTime);

		return pictures.AsReadOnly();

		bool Exists(RecordId? id)
			=> id is not null && existing.Contains(id);
	}

	/// <summary>File names of the archive <paramref name="fileName"/>, prefixed with it.</summary>
	private IEnumerable<string> KeysOf(string fileName)
	{
		var arcpath = this.GamePathService.ResolveArcFileName(fileName.ToRecordId());
		var arcfile = this.Database.ReadARCFile(arcpath.ArcFileName);
		if (arcfile is null || arcfile.DirectoryEntries.Count == 0)
			return [];

		return arcfile.DirectoryEntries.Keys
			.Cast<RecordId>()
			.Select(key => key.Normalized)
			.Where(normalized => normalized is not null)
			.Select(normalized => fileName + '\\' + normalized);
	}

	/// <summary>
	/// Resource ids of every icon in the configuration, in configuration order.
	/// </summary>
	private List<Candidate> Match(ConfRoot configfile, string[] keys)
	{
		var entries = configfile.list
			.SelectMany(file => file.imgMatch.Select(img => (file, img)))
			.ToArray();

		// Each entry is matched on its own thread; the results keep the configuration order.
		var options = new ParallelOptions { MaxDegreeOfParallelism = WorkerCount };
		var patternMatches = new List<Candidate>[entries.Length];
		Parallel.For(0, entries.Length, options, i =>
		{
			var (file, img) = entries[i];
			var found = new List<Candidate>();
			patternMatches[i] = found;
			if (!img.IsRegex)
				return;

			var regex = new Regex(file.fileName.Replace(@"\", @"\\") + @"\\" + img.Pattern, RegexOptions.CultureInvariant);
			var onrep = img.On.Split('|');
			var ofrep = img.Off.Split('|');
			var ovrep = img.Over.Split('|');

			foreach (var key in keys)
			{
				if (!regex.IsMatch(key))
					continue;

				found.Add(new Candidate(
					img.Category
					, string.IsNullOrEmpty(img.On) ? null : Replace(key, onrep)
					, string.IsNullOrEmpty(img.Off) ? null : Replace(key, ofrep)
					, string.IsNullOrEmpty(img.Over) ? null : Replace(key, ovrep)
				));
			}
		});

		var literals =
			from entry in entries
			from lit in entry.img.Literal ?? []
			let resID = (entry.file.fileName + '\\' + lit).ToRecordId()
			select new Candidate(entry.img.Category, resID, resID, resID);

		// Pattern matches come first, then literals, as before.
		return patternMatches
			.SelectMany(found => found)
			.Concat(literals)
			.ToList();
	}

	/// <summary>Half the cores, so the main window stays responsive while this runs.</summary>
	private static int WorkerCount => Math.Max(1, Environment.ProcessorCount / 2);

	private static string Replace(string input, string[] onrep)
	{
		var oldval = onrep.First();
		var newval = onrep.Last();

		if (oldval == newval || oldval == string.Empty) return input;

		return input.Replace(oldval, newval);
	}
}
