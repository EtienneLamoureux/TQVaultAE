using System.Drawing;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Domain.Results;

namespace TQVaultAE.Presentation;

/// <summary>The property sections of the Item Properties dialog, top to bottom.</summary>
public enum ItemPropertySectionKind
{
	/// <summary>The properties of the base item.</summary>
	Base,
	/// <summary>The properties of the prefix.</summary>
	Prefix,
	/// <summary>The properties of the suffix.</summary>
	Suffix,
}

/// <summary>
/// One row of a property section.
/// </summary>
/// <param name="Text">The text, without its leading color tag; TQ new line tags are turned into new lines.</param>
/// <param name="Color">The color of the text.</param>
public sealed record ItemPropertyRow(string Text, Color Color);

/// <summary>
/// A property section of the Item Properties dialog.
/// </summary>
/// <param name="Kind">Which section.</param>
/// <param name="Title">The label above the section.</param>
/// <param name="Rows">The rows; empty when the section has nothing to show.</param>
public sealed record ItemPropertySection(ItemPropertySectionKind Kind, string Title, IReadOnlyList<ItemPropertyRow> Rows)
{
	/// <summary>The section is shown only when it has rows.</summary>
	public bool Visible => this.Rows.Count != 0;
}

/// <summary>
/// What the Item Properties dialog shows for an item.
/// </summary>
/// <param name="Name">The item's full name, without its color tags.</param>
/// <param name="NameColor">The color of the name.</param>
/// <param name="Sections">The base, prefix and suffix sections, in that order, hidden ones included.</param>
public sealed record ItemPropertiesContent(string Name, Color NameColor, IReadOnlyList<ItemPropertySection> Sections)
{
	/// <summary>Color of the record id row that starts each section (not shown when extra info is filtered).</summary>
	public static Color RecordIdColor => ItemStyle.Relic.Color();

	/// <summary>Color of a row without a leading color tag.</summary>
	public static readonly Color DefaultColor = Color.White;

	/// <summary>
	/// Builds what the dialog shows from the friendly names of an item.
	/// </summary>
	/// <param name="data">The item's friendly names, read with the extra info filtered or not as <paramref name="filterExtra"/>.</param>
	/// <param name="filterExtra">The "filter extra info" box: without it, each section starts with the id of its record.</param>
	public static ItemPropertiesContent Build(ToFriendlyNameResult data, bool filterExtra)
	{
		ArgumentNullException.ThrowIfNull(data);

		return new ItemPropertiesContent(
			data.FullNameClean,
			data.Item.ExtractTextColorOrItemColor(data.BaseItemInfoDescription),
			[
				Section(ItemPropertySectionKind.Base, Resources.ItemPropertiesLabelBaseItemProperties,
					data.BaseItemId.Normalized, data.BaseAttributes, filterExtra),
				Section(ItemPropertySectionKind.Prefix, Resources.ItemPropertiesLabelPrefixProperties,
					data.PrefixInfoRecords?.Id.Normalized ?? string.Empty, data.PrefixAttributes, filterExtra),
				Section(ItemPropertySectionKind.Suffix, Resources.ItemPropertiesLabelSuffixProperties,
					data.SuffixInfoRecords?.Id.Normalized ?? string.Empty, data.SuffixAttributes, filterExtra),
			]);
	}

	/// <summary>
	/// A row from a text that may start with a color tag: the tag gives the color, else <paramref name="defaultColor"/>.
	/// </summary>
	public static ItemPropertyRow Row(string text, Color defaultColor)
	{
		var (tqColor, untagged) = text.Replace(StringHelper.TQNewLineTag, "\n").SplitLeadingColorTag();
		return new ItemPropertyRow(untagged, tqColor?.Color() ?? defaultColor);
	}

	private static ItemPropertySection Section(ItemPropertySectionKind kind, string title, string recordId, string[] attributes, bool filterExtra)
	{
		// A section without attributes is hidden, record id included.
		if (attributes.Length == 0)
			return new ItemPropertySection(kind, title, []);

		var rows = new List<ItemPropertyRow>(attributes.Length + 1);
		if (!filterExtra)
			rows.Add(Row(recordId, RecordIdColor));

		rows.AddRange(attributes.Select(a => Row(a, DefaultColor)));
		return new ItemPropertySection(kind, title, rows);
	}
}
