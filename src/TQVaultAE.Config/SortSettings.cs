using System.Xml.Serialization;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Config;

/// <summary>
/// The sort buttons that remember their own order, named like the panels of the automation endpoint. The stash panel
/// has one sort button for its Transfer, Storage and Relic tabs: it keeps one order, as its menu belongs to the button.
/// </summary>
public enum SortButtonKey
{
	/// <summary>The vault panel.</summary>
	Vault,

	/// <summary>The secondary vault panel.</summary>
	SecondaryVault,

	/// <summary>The main inventory of the character.</summary>
	Player,

	/// <summary>The bags of the character.</summary>
	PlayerBag,

	/// <summary>The stash panel (Transfer Area, Storage Area and Relic Vault tabs).</summary>
	Stash,
}

/// <summary>
/// The choices of the bag sort buttons, saved as one element with attributes:
/// <c>&lt;Sort Algorithm="Normal" MergeRelics="0" Vault="Level" Player="Type" /&gt;</c>. Each sort button keeps its own
/// order in the attribute of its <see cref="SortButtonKey"/> (<c>Vault</c>, <c>SecondaryVault</c>, <c>Player</c>,
/// <c>PlayerBag</c>, <c>Stash</c>); a button without one uses <c>Algorithm</c>, so files written before keep working.
/// Missing or unknown values fall back: a button to <c>Algorithm</c>, <c>Algorithm</c> to the normal sort, the shards
/// left as they are.
/// </summary>
public class SortSettings
{
	/// <summary>The orders picked in each sort button's menu.</summary>
	private readonly Dictionary<SortButtonKey, SackSortAlgorithm> ownAlgorithms = [];

	/// <summary>The order of a sort button that has none of its own (<see cref="AlgorithmFor"/>).</summary>
	[XmlIgnore]
	public SackSortAlgorithm Algorithm { get; set; } = SackSortAlgorithm.Normal;

	/// <summary>Sorting also stacks the shards of the same relic or charm up to completion. Off by default.</summary>
	[XmlIgnore]
	public bool MergeRelics { get; set; }

	/// <summary>The order a plain click on the sort button <paramref name="key"/> uses: its own, else <see cref="Algorithm"/>.</summary>
	public SackSortAlgorithm AlgorithmFor(SortButtonKey key)
		=> this.ownAlgorithms.TryGetValue(key, out var algorithm) ? algorithm : this.Algorithm;

	/// <summary>The sort button <paramref name="key"/> has an order of its own, picked in its menu.</summary>
	public bool HasOwnAlgorithm(SortButtonKey key) => this.ownAlgorithms.ContainsKey(key);

	/// <summary>Gives the sort button <paramref name="key"/> its own order; the other buttons keep theirs.</summary>
	/// <returns>True when the saved choices changed (to save the settings).</returns>
	public bool SetAlgorithm(SortButtonKey key, SackSortAlgorithm algorithm)
	{
		if (this.ownAlgorithms.TryGetValue(key, out var own) && own == algorithm)
			return false;

		this.ownAlgorithms[key] = algorithm;
		return true;
	}

	/// <summary><see cref="Algorithm"/> as written in the file: its name.</summary>
	[XmlAttribute(nameof(Algorithm))]
	public string AlgorithmValue
	{
		get => this.Algorithm.ToString();
		set => this.Algorithm = ParseAlgorithm(value) ?? this.Algorithm;
	}

	/// <summary><see cref="MergeRelics"/> as written in the file.</summary>
	[XmlAttribute(nameof(MergeRelics))]
	public string MergeRelicsValue
	{
		get => XmlSwitch.Write(this.MergeRelics);
		set => this.MergeRelics = XmlSwitch.Parse(value, this.MergeRelics);
	}

	/// <summary>The own order of the vault's sort button as written in the file; not written when it has none.</summary>
	[XmlAttribute(nameof(SortButtonKey.Vault))]
	public string? VaultValue
	{
		get => this.OwnValue(SortButtonKey.Vault);
		set => this.SetOwnValue(SortButtonKey.Vault, value);
	}

	/// <summary>The own order of the secondary vault's sort button as written in the file; not written when it has none.</summary>
	[XmlAttribute(nameof(SortButtonKey.SecondaryVault))]
	public string? SecondaryVaultValue
	{
		get => this.OwnValue(SortButtonKey.SecondaryVault);
		set => this.SetOwnValue(SortButtonKey.SecondaryVault, value);
	}

	/// <summary>The own order of the main inventory's sort button as written in the file; not written when it has none.</summary>
	[XmlAttribute(nameof(SortButtonKey.Player))]
	public string? PlayerValue
	{
		get => this.OwnValue(SortButtonKey.Player);
		set => this.SetOwnValue(SortButtonKey.Player, value);
	}

	/// <summary>The own order of the character bags' sort button as written in the file; not written when it has none.</summary>
	[XmlAttribute(nameof(SortButtonKey.PlayerBag))]
	public string? PlayerBagValue
	{
		get => this.OwnValue(SortButtonKey.PlayerBag);
		set => this.SetOwnValue(SortButtonKey.PlayerBag, value);
	}

	/// <summary>The own order of the stash's sort button as written in the file; not written when it has none.</summary>
	[XmlAttribute(nameof(SortButtonKey.Stash))]
	public string? StashValue
	{
		get => this.OwnValue(SortButtonKey.Stash);
		set => this.SetOwnValue(SortButtonKey.Stash, value);
	}

	/// <summary>The name of the own order of <paramref name="key"/>; null (attribute not written) when it has none.</summary>
	private string? OwnValue(SortButtonKey key)
		=> this.ownAlgorithms.TryGetValue(key, out var algorithm) ? algorithm.ToString() : null;

	/// <summary>Reads the own order of <paramref name="key"/>; an unknown value leaves the button without one.</summary>
	private void SetOwnValue(SortButtonKey key, string? value)
	{
		if (ParseAlgorithm(value) is { } algorithm)
			this.ownAlgorithms[key] = algorithm;
		else
			this.ownAlgorithms.Remove(key);
	}

	/// <summary>A member name of <see cref="SackSortAlgorithm"/> (any case, spaces ignored); null for anything else, numbers included.</summary>
	private static SackSortAlgorithm? ParseAlgorithm(string? value)
	{
		var name = value?.Trim();
		// Enum.TryParse also reads numbers and comma lists ("Level, Type" would give Rarity): names only.
		if (string.IsNullOrEmpty(name) || !name.All(char.IsLetter))
			return null;

		return Enum.TryParse(name, ignoreCase: true, out SackSortAlgorithm parsed) && Enum.IsDefined(parsed) ? parsed : null;
	}
}
