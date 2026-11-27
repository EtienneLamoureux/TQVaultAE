namespace TQVaultAE.Application.DTOs;

/// <summary>
/// An item type offered by the highlight filters: its translated name and the item classes it stands for.
/// </summary>
/// <param name="DisplayName">The translated name shown in the list.</param>
/// <param name="ItemClasses">The item classes matched, as the potions which gather several classes.</param>
public sealed record HighlightTypeOption(string DisplayName, IReadOnlyList<string> ItemClasses)
{
	/// <summary>The name shown by the list controls.</summary>
	public override string ToString() => this.DisplayName;
}
