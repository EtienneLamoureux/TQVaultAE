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
		get => XmlSwitch.Write(this.AlignRows);
		set => this.AlignRows = XmlSwitch.Parse(value, this.AlignRows);
	}

	/// <summary><see cref="HideUnchanged"/> as written in the file.</summary>
	[XmlAttribute(nameof(HideUnchanged))]
	public string HideUnchangedValue
	{
		get => XmlSwitch.Write(this.HideUnchanged);
		set => this.HideUnchanged = XmlSwitch.Parse(value, this.HideUnchanged);
	}
}
