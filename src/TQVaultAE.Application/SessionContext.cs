using System.Collections.Concurrent;
using System.Drawing;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Application;

/// <summary>
/// Shared data context for all services - holds only data containers.
/// </summary>
public class SessionContext
{
	/// <summary>
	/// Currently selected player
	/// </summary>
	public PlayerCollection? CurrentPlayer { get; set; }

	/// <summary>
	/// The item tooltips show the detailed display for now (while the forge is open), whatever the setting, which is
	/// not changed so that a settings save cannot keep it.
	/// </summary>
	public bool ForceDetailedTooltipView { get; set; }

	/// <summary>
	/// The settings changed in this session (settings dialog, or a configuration save that wrote them): every later
	/// configuration save writes them again.
	/// </summary>
	public bool ConfigChanged { get; set; }

	private BagButtonIconInfo? iconInfoCopy;
	/// <summary>
	/// Last icon info copied
	/// </summary>
	public BagButtonIconInfo? IconInfoCopy
	{
		get => iconInfoCopy;
		set
		{
			iconInfoCopy = value;
			iconInfoCopied = true;
		}
	}

	private bool iconInfoCopied;
	/// <summary>
	/// Is there any IconInfo copied
	/// </summary>
	/// <remarks>this allow <see cref="IconInfoCopy"/> to have null relevant</remarks>
	public bool IconInfoCopied => iconInfoCopied;

	/// <summary>
	/// Dictionary of all loaded player files
	/// </summary>
	public readonly LazyConcurrentDictionary<string, PlayerCollection> Players = new();

	/// <summary>
	/// Dictionary of all loaded vault files
	/// </summary>
	public readonly LazyConcurrentDictionary<string, PlayerCollection> Vaults = new();

	/// <summary>
	/// Dictionary of all loaded player stash files
	/// </summary>
	public readonly LazyConcurrentDictionary<string, Stash> Stashes = new();

	/// <summary>
	/// The loadouts read for each character, by loadouts file path (case ignored).
	/// </summary>
	public readonly ConcurrentDictionary<string, LoadoutSet> Loadouts = new(StringComparer.OrdinalIgnoreCase);
}
