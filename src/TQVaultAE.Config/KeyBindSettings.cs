using System.Xml;
using System.Xml.Serialization;

namespace TQVaultAE.Config;

/// <summary>
/// The key and mouse bindings the user changed, saved as one element with an attribute per changed action:
/// <c>&lt;KeyBinds ItemDuplicateClick="Ctrl+Click" ZoomIn="Ctrl+Add, Ctrl+Oemplus" /&gt;</c>. An empty value is an
/// action without a binding; an action left out keeps its default. The action names and gestures are read by the
/// application's key bind map, not here.
/// </summary>
public sealed class KeyBindSettings : IEquatable<KeyBindSettings>
{
	/// <summary>The changed actions: action name, gestures.</summary>
	[XmlIgnore]
	public SortedDictionary<string, string> Overrides { get; } = new(StringComparer.Ordinal);

	/// <summary><see cref="Overrides"/> as written in the file; attributes whose name is not an XML name are skipped.</summary>
	[XmlAnyAttribute]
	public XmlAttribute[] Attributes
	{
		get
		{
			var document = new XmlDocument();
			return [.. this.Overrides
				.Where(o => IsName(o.Key))
				.Select(o =>
				{
					var attribute = document.CreateAttribute(o.Key);
					attribute.Value = o.Value;
					return attribute;
				})];
		}
		set
		{
			this.Overrides.Clear();
			foreach (var attribute in value ?? [])
				if (attribute.NamespaceURI.Length == 0)
					this.Overrides[attribute.LocalName] = attribute.Value;
		}
	}

	/// <summary>Replaces the changed actions.</summary>
	public void SetOverrides(IEnumerable<KeyValuePair<string, string>> overrides)
	{
		this.Overrides.Clear();
		foreach (var (name, value) in overrides)
			this.Overrides[name] = value ?? string.Empty;
	}

	/// <summary>A copy, edited without changing these settings.</summary>
	public KeyBindSettings Clone()
	{
		var copy = new KeyBindSettings();
		copy.SetOverrides(this.Overrides);
		return copy;
	}

	private static bool IsName(string name)
	{
		try
		{
			XmlConvert.VerifyName(name);
			return true;
		}
		catch (XmlException)
		{
			return false;
		}
	}

	/// <inheritdoc/>
	public bool Equals(KeyBindSettings? other)
		=> other is not null && this.Overrides.SequenceEqual(other.Overrides);

	/// <inheritdoc/>
	public override bool Equals(object? obj) => this.Equals(obj as KeyBindSettings);

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		var hash = new HashCode();
		foreach (var (name, value) in this.Overrides)
		{
			hash.Add(name, StringComparer.Ordinal);
			hash.Add(value, StringComparer.Ordinal);
		}

		return hash.ToHashCode();
	}
}
