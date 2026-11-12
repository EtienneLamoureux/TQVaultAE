using System.Xml.Serialization;

namespace TQVaultAE.Config;

/// <summary>
/// The toolbar choices of the item comparison dialog, saved as one element with attributes written 0 / 1 like the
/// other switches of <c>UserConfig.xml</c>: <c>&lt;ItemCompare AlignRows="1" HideUnchanged="0" /&gt;</c>.
/// </summary>
public class ItemCompareSettings
{
	/// <summary>Each difference on the row of its item line. On by default.</summary>
	[XmlIgnore]
	public bool AlignRows { get; set; } = true;

	/// <summary>The stats with the same numbers on both items are left out. Off by default.</summary>
	[XmlIgnore]
	public bool HideUnchanged { get; set; }

	/// <summary><see cref="AlignRows"/> as written in the file.</summary>
	[XmlAttribute(nameof(AlignRows))]
	public string AlignRowsValue
	{
		get => Write(this.AlignRows);
		set => this.AlignRows = Parse(value, this.AlignRows);
	}

	/// <summary><see cref="HideUnchanged"/> as written in the file.</summary>
	[XmlAttribute(nameof(HideUnchanged))]
	public string HideUnchangedValue
	{
		get => Write(this.HideUnchanged);
		set => this.HideUnchanged = Parse(value, this.HideUnchanged);
	}

	private static string Write(bool value) => value ? "1" : "0";

	/// <summary>"1" / "true" or "0" / "false" (any case, spaces ignored); <paramref name="fallback"/> for anything else.</summary>
	private static bool Parse(string? value, bool fallback)
		=> value?.Trim().ToUpperInvariant() switch
		{
			"1" or "TRUE" => true,
			"0" or "FALSE" => false,
			_ => fallback,
		};
}
