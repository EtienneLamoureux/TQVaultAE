using AwesomeAssertions;
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
/// The sums behind the character window read an equipped item in its own map (<see cref="Item.Mod"/>), whatever the
/// active one is: two databases, the base game's and a mod's, give different records under the same ids.
/// </summary>
public class ItemProviderItemMapTests
{
	private const string ModName = "ModA";
	private static readonly RecordId Sword = """records\test\sword.dbr""".ToRecordId();
	private static readonly RecordId Relic = """records\test\relic.dbr""".ToRecordId();
	private static readonly RecordId RelicBonus = """records\test\relicbonus.dbr""".ToRecordId();
	private static readonly RecordId PetBonus = """records\test\petbonus.dbr""".ToRecordId();
	private static readonly RecordId SetRecord = """records\test\set.dbr""".ToRecordId();

	/// <summary>The records of each map; the empty key is the base game.</summary>
	private readonly Dictionary<string, Dictionary<RecordId, DBRecordCollection>> _maps = new()
	{
		[string.Empty] = [],
		[ModName] = [],
	};

	/// <summary>The map the database answers with: the active map, or the item's inside its scope.</summary>
	private string _current = string.Empty;

	private readonly ItemProvider _provider;

	public ItemProviderItemMapTests()
	{
		var database = new Mock<IDatabase>();
		database.Setup(d => d.GetRecordFromFile(It.IsAny<RecordId>()))
			.Returns((RecordId id) => this._maps[this._current].GetValueOrDefault(id));

		// Like ItemModService.Scope: the item's map until disposed, nothing for an item without a map yet.
		var itemMods = new Mock<IItemModService>();
		itemMods.Setup(m => m.Scope(It.IsAny<Item?>())).Returns((Item? item) =>
		{
			if (item is null || item.ModOrigin == ModOrigin.None)
				return NoScope.Instance;

			var previous = this._current;
			this._current = item.Mod ?? string.Empty;
			return new Restore(() => this._current = previous);
		});

		this._provider = new ItemProvider(
			Mock.Of<ILogger<ItemProvider>>(),
			database.Object,
			Mock.Of<ILootTableCollectionProvider>(),
			new ItemAttributeProvider(Mock.Of<ILogger<ItemAttributeProvider>>()),
			Mock.Of<ITQDataService>(),
			Mock.Of<ITranslationService>(),
			Mock.Of<IPathIO>(),
			new UserSettings { ShowInGameValues = false },
			itemMods.Object);
	}

	[Fact]
	public void GetAttributeSums_ModItemOnABaseGameCharacter_ReadsTheModsRecords()
	{
		this.Add(ModName, Sword, "WeaponMelee_Sword", ("characterStrength", 30), ("petBonusName", PetBonus.Raw));
		this.Add(ModName, Relic, "ItemRelic", ("characterLife", 50));
		this.Add(ModName, RelicBonus, "ItemRelic", ("characterDexterity", 12));
		this.Add(ModName, PetBonus, "PetBonus", ("characterIntelligence", 7));
		var sword = this.Equipped(ModName);

		var sum = this._provider.GetAttributeSums(sword);

		sum["characterStrength"].Should().Be(30, "the base game has no such record: the mod's is read");
		sum["characterLife"].Should().Be(50, "the relic is read in the item's map too");
		sum["characterDexterity"].Should().Be(12, "and its completion bonus");
		sum[AttributeSum.PetBonusPrefix + "characterIntelligence"].Should().Be(7, "and the pet bonus record it names");
		this._current.Should().BeEmpty("the active map is back after the reading");
	}

	[Fact]
	public void GetAttributeSums_BaseGameItemOnAModCharacter_ReadsTheBaseGameRecords()
	{
		this.Add(string.Empty, Sword, "WeaponMelee_Sword", ("characterStrength", 30));
		this.Add(ModName, Sword, "WeaponMelee_Sword", ("characterStrength", 90));
		var sword = this.Equipped(mod: null);
		this._current = ModName;

		this._provider.GetAttributeSums(sword)["characterStrength"].Should().Be(30, "the mod changes the record, the item is the base game's");
	}

	[Fact]
	public void GetSetBonuses_ReadsTheSetInItsPiecesMap()
	{
		this.Add(ModName, SetRecord, "ItemSet", ("characterStrength", 5));
		var sword = this.Add(ModName, Sword, "WeaponMelee_Sword", ("itemSetName", SetRecord.Raw));
		var piece = this.Equipped(ModName);
		piece.baseItemInfo = new Info(sword);

		this._provider.GetSetBonuses([piece])["characterStrength"].Should().Be(5);
	}

	[Fact]
	public void GetGrantedSkillLevels_ReadsTheItemsMap()
	{
		var skill = """records\test\skill.dbr""";
		this.Add(ModName, Sword, "WeaponMelee_Sword", ("itemSkillName", skill), ("itemSkillLevel", 4));
		var sword = this.Equipped(ModName);

		this._provider.GetGrantedSkillLevels(sword).Should().Equal((skill.ToRecordId(), 4));
	}

	/// <summary>An equipped sword with a relic, linked to <paramref name="mod"/> by an own link (moved in from a vault).</summary>
	private Item Equipped(string? mod) => new()
	{
		BaseItemId = Sword,
		relicID = Relic,
		RelicBonusId = RelicBonus,
		Var1 = 1,
		Mod = mod,
		ModOrigin = ModOrigin.Own,
	};

	private DBRecordCollection Add(string map, RecordId id, string recordClass, params (string Name, object Value)[] variables)
	{
		var record = new DBRecordCollection(id, recordClass);
		record.Set(new Variable("Class", VariableDataType.StringVar, 1) { [0] = recordClass });
		foreach (var (name, value) in variables)
		{
			var type = value is string ? VariableDataType.StringVar : VariableDataType.Integer;
			record.Set(new Variable(name, type, 1) { [0] = value });
		}

		this._maps[map][id] = record;
		return record;
	}

	private sealed class Restore(Action restore) : IDisposable
	{
		public void Dispose() => restore();
	}
}
