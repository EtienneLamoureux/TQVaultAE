using System.Collections.Concurrent;
using System.Diagnostics;
using ArzExplorer.Models;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GameFiles;

namespace ArzExplorer;

/// <summary>
/// Debug-only sweep of the links of every DBR record: the grid's own link detection, <see cref="NavigateTo"/>'s
/// resolution and the loading each view does for the target.
/// </summary>
public partial class MainForm
{
	/// <summary>One problem found by the sweep.</summary>
	internal sealed record LinkProblem(string Kind, string Record, string Variable, string Value, string? Detail = null);

	/// <summary>
	/// Checks every link of the database records whose id contains <paramref name="contains"/>. Needs the database and
	/// the archives loaded (Load All Files). Runs off the UI thread: do not use the window meanwhile.
	/// </summary>
	internal object AutomationCheckLinks(string? contains, int maxRecords, int examplesPerKind, bool render)
	{
		var database = this._openedFiles.Values.FirstOrDefault(f => f.FileType == CompressedFileType.ArzFile)
			?? throw new InvalidOperationException("Open the database first (Load All Files opens it with every archive).");

		var watch = Stopwatch.StartNew();
		var problems = new ConcurrentQueue<LinkProblem>();
		var targets = new ConcurrentDictionary<string, (string Record, string Variable)>(StringComparer.OrdinalIgnoreCase);
		int records = 0, links = 0, resolved = 0, multiValueCells = 0, unstyled = 0;

		// File name -> node keys of every loaded leaf, to tell a missing file from one under another path.
		var fileNames = new Lazy<ILookup<string, string>>(() => this.NodesByKey
			.Where(kv => kv.Value.Nodes.Count == 0)
			.ToLookup(kv => Path.GetFileName(EntryOf(kv.Key)), kv => kv.Key, StringComparer.OrdinalIgnoreCase));

		var ids = database.Arz.Keys
			.Where(id => string.IsNullOrEmpty(contains) || id.Normalized.Contains(contains, StringComparison.OrdinalIgnoreCase))
			.Take(maxRecords <= 0 ? int.MaxValue : maxRecords)
			.ToArray();

		foreach (var id in ids)
		{
			records++;
			DBRecordCollection record;
			try
			{
				record = this.ArzProvider.GetRecordNotCached(database.Arz, id);
			}
			catch (Exception ex)
			{
				problems.Enqueue(new("record unreadable", id.Raw, string.Empty, string.Empty, ex.Message));
				continue;
			}

			foreach (Variable variable in record)
			{
				// Same split as PopulateGridView: name, then all the values joined by ';' in one cell.
				var cells = GridCells(variable.ToString());
				if (cells.Length < 2)
					continue;

				// PopulateGridView gives each value of a multi-value link variable its own row.
				var values = cells[1].Split(';');
				var linkValues = values.Where(IsGridLink).ToArray();
				if (linkValues.Length == 0)
					continue;

				if (values.Length > 1)
					multiValueCells++;
				else if (!IsGridLink(cells[1]))
					unstyled++;

				foreach (var value in linkValues)
				{
					links++;
					var key = this.ResolveLinkKey(value);
					if (key is null)
					{
						problems.Enqueue(ClassifyUnresolved(id.Raw, variable.Name, value, fileNames.Value));
						continue;
					}

					resolved++;
					targets.TryAdd(key, (id.Raw, variable.Name));

					// With only the database loaded, the click opens the candidate archives in turn: one must hold the target.
					if (!value.EndsWith(".DBR", StringComparison.OrdinalIgnoreCase) && this.NodesByKey[key].Tag is NodeTag tag
						&& !this.AutoLoadArchives(value).Contains(tag.File.SourceFile, StringComparer.OrdinalIgnoreCase))
					{
						problems.Enqueue(new("auto-load cannot reach the archive", id.Raw, variable.Name, value
							, $"tries {string.Join(" | ", this.AutoLoadArchives(value))}, target in {tag.File.SourceFile}"));
					}
				}
			}
		}

		int rendered = 0;
		if (render)
		{
			Parallel.ForEach(targets, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, target =>
			{
				var problem = this.RenderTarget(target.Key);
				if (problem is null)
					Interlocked.Increment(ref rendered);
				else
					problems.Enqueue(new("does not render", target.Value.Record, target.Value.Variable, target.Key, problem));
			});
		}

		var byKind = problems.GroupBy(p => p.Kind).OrderByDescending(g => g.Count()).ToArray();
		return new
		{
			seconds = Math.Round(watch.Elapsed.TotalSeconds, 1),
			records,
			links,
			resolved,
			uniqueTargets = targets.Count,
			rendered = render ? rendered : (int?)null,
			multiValueCells,
			unstyledSingleLinks = unstyled,
			problems = byKind.ToDictionary(g => g.Key, g => g.Count()),
			examples = byKind.ToDictionary(g => g.Key, g => g.Take(examplesPerKind).ToArray()),
			targetsByExtension = targets.Keys.GroupBy(k => Path.GetExtension(k).ToUpperInvariant()).ToDictionary(g => g.Key, g => g.Count()),
			all = problems.ToArray(),
		};
	}

	/// <summary>
	/// The key of the node a click on <paramref name="value"/> selects, with every archive loaded: the resolution of
	/// <see cref="NavigateTo"/> itself. Null when it finds none.
	/// </summary>
	private string? ResolveLinkKey(string value) => this.ResolveLink(value, openArchives: false)?.Name;

	/// <summary>Why a link resolves to nothing: a file under another path, or a file missing from the game.</summary>
	private static LinkProblem ClassifyUnresolved(string record, string variable, string value, ILookup<string, string> fileNames)
	{
		var sameName = fileNames[Path.GetFileName(value.Replace('/', '\\'))].Take(3).ToArray();
		return sameName.Length > 0
			? new("unresolved: same file name elsewhere", record, variable, value, string.Join(" | ", sameName))
			: new("unresolved: missing from the game files", record, variable, value);
	}

	/// <summary>The archives a click opens in turn to find <paramref name="value"/>, as <see cref="NavigateTo"/> does.</summary>
	private string[] AutoLoadArchives(string value)
		=> [.. GameArchives.ArcFileCandidates(value.Replace('/', '\\').ToRecordId())
			.Select(arcFile => this._arcFiles.TryGetValue(arcFile, out var path) ? path : null)
			.OfType<string>()];

	/// <summary>Loads a target as its view does; returns the problem, or null when it shows.</summary>
	private string? RenderTarget(string nodeKey)
	{
		try
		{
			if (this.NodesByKey[nodeKey].Tag is not NodeTag tag)
				return "node without tag";

			if (tag.File.FileType == CompressedFileType.ArzFile)
			{
				var record = this.ArzProvider.GetRecordNotCached(tag.File.Arz, EntryOf(nodeKey));
				return record.Count == 0 ? "empty record" : null;
			}

			var data = this.ArcProvider.GetData(tag.File.Arc, ArcPaths.DataId(tag.Key));
			if (data is null)
				return "no data in the archive";

			switch (Path.GetExtension(nodeKey).ToUpperInvariant())
			{
				case ".TEX":
					using (var bitmap = this.BitmapService.LoadFromTexMemory(data, 0, data.Length))
						return bitmap is null ? "texture not decoded" : null;
				case ".TXT":
					return TextDatabaseFactory.DecodeText(data).Length == 0 ? "empty text" : null;
				case ".WAV":
					return data.AsSpan().StartsWith("RIFF"u8) ? null : "not a RIFF wave";
				case ".MP3":
					return data.AsSpan().StartsWith("ID3"u8) || (data.Length > 1 && data[0] == 0xFF && (data[1] & 0xE0) == 0xE0)
						? null : "not an MP3 stream";
				default:
					return null;
			}
		}
		catch (Exception ex)
		{
			return $"{ex.GetType().Name}: {ex.Message}";
		}
	}
}
