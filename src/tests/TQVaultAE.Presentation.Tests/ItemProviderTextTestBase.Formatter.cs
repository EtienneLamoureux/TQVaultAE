using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Presentation.ItemText;

namespace TQVaultAE.Tests.Data;

/// <summary>
/// The item text formatter of <see cref="ItemProviderTextTestBase"/>, on top of its <see cref="ItemProviderTextTestBase.Provider"/>
/// and the same database, translations and settings.
/// </summary>
public abstract partial class ItemProviderTextTestBase
{
	private ItemTextFormatter? _formatter;

	protected ItemTextFormatter Formatter => this._formatter ??= new ItemTextFormatter(
		Mock.Of<ILogger<ItemTextFormatter>>(),
		this.MockDatabase.Object,
		this.Provider,
		this.Attributes,
		this.MockTranslation.Object,
		this.PathIO,
		this.Settings,
		this.ItemMods
	);

	protected string[] BaseAttributes(Item item, bool filterExtra = true)
		=> this.Formatter.GetFriendlyNames(item, FriendlyNamesExtraScopes.BaseAttributes, filterExtra).BaseAttributes;

	/// <summary>The base attribute lines (without tags, trimmed) marked as text, not attributes, in their order.</summary>
	protected string[] BaseTextLines(Item item)
	{
		var names = this.Formatter.GetFriendlyNames(item, FriendlyNamesExtraScopes.BaseAttributes, true);
		return [.. names.BaseAttributes.Select(l => l.RemoveAllTQTags()!.Trim()).Where(names.IsTextLine)];
	}
}
