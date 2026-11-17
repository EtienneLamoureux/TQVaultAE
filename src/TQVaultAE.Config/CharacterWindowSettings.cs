using System.Xml.Serialization;

namespace TQVaultAE.Config;

/// <summary>
/// The floating character windows open when TQVault closed, reopened at the next start:
/// <c>&lt;CharacterWindows&gt;&lt;Window Character="IT/Main/HeroA" X="120" Y="80" /&gt;&lt;/CharacterWindows&gt;</c>.
/// Missing, none opens.
/// </summary>
public class CharacterWindowSettings
{
	/// <summary>One window per character, in the order they were opened.</summary>
	[XmlElement("Window")]
	public List<CharacterWindowPlace> Windows { get; set; } = [];

	/// <summary>Records the window of <paramref name="character"/> at its place, replacing the one it had.</summary>
	/// <returns>True when the settings changed.</returns>
	public bool Remember(string character, int x, int y)
	{
		ArgumentException.ThrowIfNullOrEmpty(character);
		var place = this.Find(character);
		if (place is null)
		{
			this.Windows.Add(new CharacterWindowPlace { Character = character, X = x, Y = y });
			return true;
		}

		if (place.X == x && place.Y == y)
			return false;

		place.X = x;
		place.Y = y;
		return true;
	}

	/// <summary>Forgets the window of <paramref name="character"/> (closed by the user).</summary>
	/// <returns>True when the settings changed.</returns>
	public bool Forget(string character)
		=> this.Windows.RemoveAll(w => string.Equals(w.Character, character, StringComparison.OrdinalIgnoreCase)) > 0;

	/// <summary>The place recorded for <paramref name="character"/>, or null.</summary>
	public CharacterWindowPlace? Find(string character)
		=> this.Windows.FirstOrDefault(w => string.Equals(w.Character, character, StringComparison.OrdinalIgnoreCase));
}

/// <summary>A floating character window: its character's key and the top left corner of the window, in screen pixels.</summary>
public class CharacterWindowPlace
{
	/// <summary>The character's key (<c>IT/Main/&lt;name&gt;</c>), as <c>LastCharacterName</c>.</summary>
	[XmlAttribute(nameof(Character))]
	public string Character { get; set; } = string.Empty;

	[XmlAttribute(nameof(X))]
	public int X { get; set; }

	[XmlAttribute(nameof(Y))]
	public int Y { get; set; }
}
