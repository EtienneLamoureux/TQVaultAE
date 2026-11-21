//-----------------------------------------------------------------------
// <copyright file="ArchiveSelection.cs" company="TQVaultAE">
//     Copyright (c) 2026 TQVaultAE Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using TQVaultAE.DbCli.Core;

namespace TQVaultAE.DbCli.Query;

/// <summary>Archive and entry selectors of <c>files</c> / <c>extract</c>. Repeated patterns of one kind are OR-ed, kinds are AND-ed.</summary>
internal sealed class ArchiveSelection
{
	/// <summary>Archive patterns, matched against the key (<c>Resources/XPack/Items</c>), the file name (<c>Items.arc</c>) and its stem (<c>Items</c>).</summary>
	public List<Pattern> Archives { get; } = [];

	/// <summary>Entry name patterns (path-like).</summary>
	public List<Pattern> Paths { get; } = [];

	/// <summary>Resource id patterns (path-like), e.g. <c>Items\weapons\*.tex</c>.</summary>
	public List<Pattern> Resources { get; } = [];

	/// <summary>Extensions without dot, lower-case.</summary>
	public HashSet<string> Extensions { get; } = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>Adds extensions given as <c>tex,.mp3</c>.</summary>
	public void AddExtensions(IEnumerable<string> extensions) {
		foreach (var e in extensions) {
			var x = e.Trim().TrimStart('*').TrimStart('.');
			if (x.Length > 0)
				this.Extensions.Add(x.ToLowerInvariant());
		}
	}

	/// <summary>Archive filter (before opening it).</summary>
	public bool MatchesArchive(string archiveKey, string archivePath) {
		if (this.Archives.Count == 0)
			return true;
		var file = Path.GetFileName(archivePath);
		var stem = Path.GetFileNameWithoutExtension(archivePath);
		foreach (var p in this.Archives)
			if (p.IsMatch(archiveKey) || p.IsMatch(file) || p.IsMatch(stem))
				return true;
		return false;
	}

	/// <summary>Entry filter.</summary>
	public bool MatchesEntry(ArcEntry entry) {
		if (this.Extensions.Count > 0) {
			var ext = Path.GetExtension(entry.Name);
			if (ext.Length < 2 || !this.Extensions.Contains(ext[1..]))
				return false;
		}
		if (this.Paths.Count > 0 && !AnyMatch(this.Paths, entry.Name))
			return false;
		if (this.Resources.Count > 0 && !AnyMatch(this.Resources, entry.ResourceId))
			return false;
		return true;
	}

	private static bool AnyMatch(List<Pattern> patterns, string value) {
		foreach (var p in patterns)
			if (p.IsMatch(value))
				return true;
		return false;
	}

	/// <summary>Coarse asset type from the extension.</summary>
	public static string TypeOf(string name) => Path.GetExtension(name).ToLowerInvariant() switch {
		".tex" => "texture",
		".msh" => "mesh",
		".anm" => "animation",
		".mp3" or ".wav" or ".ogg" => "sound",
		".fnt" => "font",
		".txt" => "text",
		".pfx" => "effect",
		".dbr" => "record",
		".qst" => "quest",
		".ssh" => "shader",
		".lua" => "script",
		".map" or ".sd" or ".wrl" or ".lvl" or ".rlv" => "map",
		".cnv" => "conversation",
		_ => "other",
	};
}
