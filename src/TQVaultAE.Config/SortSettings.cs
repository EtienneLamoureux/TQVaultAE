using System.Xml.Serialization;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Config;

/// <summary>
/// The choices of the bag sort button, saved as one element with attributes: <c>&lt;Sort Algorithm="Level" /&gt;</c>.
/// Missing or unknown, the defaults apply (the normal sort).
/// </summary>
public class SortSettings
{
	/// <summary>The order a plain click on a sort button uses, picked in its right-click menu.</summary>
	[XmlIgnore]
	public SackSortAlgorithm Algorithm { get; set; } = SackSortAlgorithm.Normal;

	/// <summary><see cref="Algorithm"/> as written in the file: its name.</summary>
	[XmlAttribute(nameof(Algorithm))]
	public string AlgorithmValue
	{
		get => this.Algorithm.ToString();
		set => this.Algorithm = ParseAlgorithm(value, this.Algorithm);
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
