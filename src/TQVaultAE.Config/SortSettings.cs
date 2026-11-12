using System.Xml.Serialization;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Config;

/// <summary>
/// The choices of the bag sort button, saved as one element with attributes:
/// <c>&lt;Sort Algorithm="Level" MergeRelics="0" /&gt;</c>. Missing or unknown, the defaults apply (the normal sort, shards
/// left as they are).
/// </summary>
public class SortSettings
{
	/// <summary>The order a plain click on a sort button uses, picked in its right-click menu.</summary>
	[XmlIgnore]
	public SackSortAlgorithm Algorithm { get; set; } = SackSortAlgorithm.Normal;

	/// <summary>Sorting also stacks the shards of the same relic or charm up to completion. Off by default.</summary>
	[XmlIgnore]
	public bool MergeRelics { get; set; }

	/// <summary><see cref="Algorithm"/> as written in the file: its name.</summary>
	[XmlAttribute(nameof(Algorithm))]
	public string AlgorithmValue
	{
		get => this.Algorithm.ToString();
		set => this.Algorithm = ParseAlgorithm(value, this.Algorithm);
	}

	/// <summary><see cref="MergeRelics"/> as written in the file.</summary>
	[XmlAttribute(nameof(MergeRelics))]
	public string MergeRelicsValue
	{
		get => XmlSwitch.Write(this.MergeRelics);
		set => this.MergeRelics = XmlSwitch.Parse(value, this.MergeRelics);
	}

	/// <summary>A member name of <see cref="SackSortAlgorithm"/> (any case, spaces ignored); <paramref name="fallback"/> for anything else, numbers included.</summary>
	private static SackSortAlgorithm ParseAlgorithm(string? value, SackSortAlgorithm fallback)
	{
		var name = value?.Trim();
		// Enum.TryParse also reads numbers and comma lists ("Level, Type" would give Rarity): names only.
		if (string.IsNullOrEmpty(name) || !name.All(char.IsLetter))
			return fallback;

		return Enum.TryParse(name, ignoreCase: true, out SackSortAlgorithm parsed) && Enum.IsDefined(parsed) ? parsed : fallback;
	}
}
