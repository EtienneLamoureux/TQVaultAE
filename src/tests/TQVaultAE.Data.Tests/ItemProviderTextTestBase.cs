using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Data;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Data;

/// <summary>
/// Builds an <see cref="ItemProvider"/> with the real <see cref="ItemAttributeProvider"/>, a mocked database fed with
/// small hand-built records and translations written in the game's own format (<c>{%+.0f0}</c>...).
/// </summary>
public abstract class ItemProviderTextTestBase
{
	private delegate bool TryTranslate(string tag, out string translation, bool removeAllTQTags, bool doCleanup, bool doCleanupKeepLeadingColorTag);

	protected static readonly string Epic = ItemStyle.Epic.TQColor().ColorTag();
	protected static readonly string Mundane = ItemStyle.Mundane.TQColor().ColorTag();
	protected static readonly string Legendary = ItemStyle.Legendary.TQColor().ColorTag();
	protected static readonly string Relic = ItemStyle.Relic.TQColor().ColorTag();
	protected static readonly string Rare = ItemStyle.Rare.TQColor().ColorTag();
	protected static readonly string Common = ItemStyle.Common.TQColor().ColorTag();

	/// <summary>
	/// Translations by tag, in the game's format (from the English text resources of Titan Quest Anniversary Edition).
	/// </summary>
	protected readonly Dictionary<string, string> Translations = new(StringComparer.OrdinalIgnoreCase)
	{
		["DamageSingleFormat"] = "{%.0f0}",
		["DamageRangeFormat"] = "{%.0f0} ~ {%.0f1}",
		["DamageInfluenceSingleFormat"] = "{%.1f0}",
		["DamageInfluenceRangeFormat"] = "{%.1f0} ~ {%.1f1}",
		["DamageSingleFormatTime"] = "over {%.1f0} Seconds",
		["DamageRangeFormatTime"] = "over {%.1f0} - {%.1f1} Seconds",
		["DamageFixedSingleFormatTime"] = "for {%.1f0} Seconds",
		["DamageFixedRangeFormatTime"] = "for {%.1f0} - {%.1f1} Seconds",
		["ChanceOfTag"] = "{%.1f0}% Chance of",
		["ImprovedTimeFormat"] = "with {%+.0f0}% Improved Duration",
		["characterStrength"] = "{%+.0f0} Strength",
		["characterLife"] = "{%+.0f0} Health",
		["DamageBasePhysical"] = "Physical Damage",
		["DamageFire"] = "Fire Damage",
		["DamageModifierFire"] = "{%+.0f0}% Fire Damage",
		["DamageDurationFire"] = "Burn Damage",
		["DamageDurationTotalSpeed"] = "% Slowed",
		["DamageDurationModifierFire"] = "{%+.0f0}% Burn Damage",
		["DamageStun"] = "Stun",
		["DamageManaBurnRatio"] = "({%.0f0}% of lost Energy as Damage)",
		["DamageManaDrain"] = "% Energy Drain",
		["DefenseBlock"] = "{%.0f0} Damage Blocked",
		["DefenseAbsorptionProtection"] = "{%.0f0} Armor",
		["DefenseAbsorptionProtectionBonus"] = "+{%.0f0} Armor",
		["MenuLevel"] = "Level:   {%d0}",
	};

	protected readonly Mock<IDatabase> MockDatabase = new();
	protected readonly UserSettings Settings = new() { ShowInGameValues = false };
	protected readonly ItemProvider Provider;

	protected ItemProviderTextTestBase()
	{
		var mockTranslation = new Mock<ITranslationService>();
		mockTranslation.SetupGet(t => t.ItemSeed).Returns("Seed {0} ({1:0.00})");
		mockTranslation.Setup(t => t.TryTranslateXTag(It.IsAny<string>(), out It.Ref<string>.IsAny, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
			.Returns(new TryTranslate((string tag, out string translation, bool _, bool _, bool _) =>
			{
				translation = this.Translations.GetValueOrDefault(tag ?? string.Empty, string.Empty);
				return translation.Length > 0;
			}));
		mockTranslation.Setup(t => t.TranslateXTag(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
			.Returns((string tag, bool _, bool _, bool _) => this.Translations.GetValueOrDefault(tag ?? string.Empty, tag ?? string.Empty));

		this.MockDatabase.Setup(d => d.VariableToStringNice(It.IsAny<Variable>())).Returns((Variable v) => $"?{v.Name}?: {v.ToStringValue()}");

		var mockPath = new Mock<IPathIO>();
		mockPath.Setup(p => p.GetFileNameWithoutExtension(It.IsAny<string>())).Returns((string path) => Path.GetFileNameWithoutExtension(path.Replace('\\', '/')));
		mockPath.Setup(p => p.GetExtension(It.IsAny<string>())).Returns((string path) => Path.GetExtension(path));

		this.Provider = new ItemProvider(
			Mock.Of<ILogger<ItemProvider>>(),
			this.MockDatabase.Object,
			Mock.Of<ILootTableCollectionProvider>(),
			new ItemAttributeProvider(Mock.Of<ILogger<ItemAttributeProvider>>()),
			Mock.Of<ITQDataService>(),
			mockTranslation.Object,
			mockPath.Object,
			this.Settings
		);
	}

	/// <summary>
	/// Builds the item's base record (its <c>Class</c> is <paramref name="itemClass"/>) and the item with its base info.
	/// </summary>
	protected Item CreateItem(string itemClass, params Variable[] attributes)
		=> this.CreateItemWithId($"""records\test\{itemClass}.dbr""", itemClass, attributes);

	protected Item CreateItemWithId(string id, string itemClass, params Variable[] attributes)
	{
		var record = this.CreateRecord(id, itemClass, attributes);
		return new Item
		{
			BaseItemId = record.Id,
			baseItemInfo = new Info(record),
		};
	}

	protected DBRecordCollection CreateRecord(string id, string recordClass, params Variable[] variables)
	{
		var recordId = RecordId.Create(id);
		var record = new DBRecordCollection(recordId, recordClass);
		record.Set(Str("Class", recordClass));
		foreach (var variable in variables)
			record.Set(variable);

		this.MockDatabase.Setup(d => d.GetRecordFromFile(recordId)).Returns(record);
		this.MockDatabase.Setup(d => d.GetInfo(recordId)).Returns(new Info(record));
		return record;
	}

	protected string[] BaseAttributes(Item item, bool filterExtra = true)
		=> this.Provider.GetFriendlyNames(item, FriendlyNamesExtraScopes.BaseAttributes, filterExtra).BaseAttributes;

	/// <summary>The base attribute lines (without tags, trimmed) marked as text, not attributes, in their order.</summary>
	protected string[] BaseTextLines(Item item)
	{
		var names = this.Provider.GetFriendlyNames(item, FriendlyNamesExtraScopes.BaseAttributes, true);
		return [.. names.BaseAttributes.Select(l => l.RemoveAllTQTags()!.Trim()).Where(names.IsTextLine)];
	}

	protected static Variable Int(string name, int value) => Ints(name, value);

	protected static Variable Ints(string name, params int[] values)
	{
		var variable = new Variable(name, VariableDataType.Integer, values.Length);
		for (int i = 0; i < values.Length; i++)
			variable[i] = values[i];
		return variable;
	}

	protected static Variable Float(string name, float value) => Floats(name, value);

	protected static Variable Floats(string name, params float[] values)
	{
		var variable = new Variable(name, VariableDataType.Float, values.Length);
		for (int i = 0; i < values.Length; i++)
			variable[i] = values[i];
		return variable;
	}

	protected static Variable Str(string name, string value) => Strs(name, value);

	protected static Variable Strs(string name, params string[] values)
	{
		var variable = new Variable(name, VariableDataType.StringVar, values.Length);
		for (int i = 0; i < values.Length; i++)
			variable[i] = values[i];
		return variable;
	}
}
