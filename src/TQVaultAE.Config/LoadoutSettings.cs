using System.Xml.Serialization;

namespace TQVaultAE.Config;

/// <summary>
/// The choices of the loadouts, saved as one element with attributes written 0 / 1 like the other switches of
/// <c>UserConfig.xml</c>: <c>&lt;Loadout ReplaceWorn="0" /&gt;</c>.
/// </summary>
public class LoadoutSettings
{
	/// <summary>
	/// Loading a loadout replaces the worn pieces it does not use instead of storing them in the vault bag shown: they are
	/// gone (a loadout keeps copies, so this saves piling up duplicates). Off by default.
	/// </summary>
	[XmlIgnore]
	public bool ReplaceWorn { get; set; }

	/// <summary><see cref="ReplaceWorn"/> as written in the file.</summary>
	[XmlAttribute(nameof(ReplaceWorn))]
	public string ReplaceWornValue
	{
		get => XmlSwitch.Write(this.ReplaceWorn);
		set => this.ReplaceWorn = XmlSwitch.Parse(value, this.ReplaceWorn);
	}
}
