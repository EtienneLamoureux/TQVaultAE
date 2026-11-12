using System.Text.Json;
using AwesomeAssertions;
using TQVaultAE.Domain.Jitter;

namespace TQVaultAE.Tests.Entities;

/// <summary>
/// In-game item checks from issue #552 (originally ejfudd's JitterVerificationTests.cs), ported to the
/// roll order of <see cref="EquipmentRollTable"/> (see <see cref="EquipmentRollTable"/> and issue #552,
/// https://github.com/EtienneLamoureux/TQVaultAE/issues/552).
/// </summary>
public class JitterVerificationTests
{
	private sealed record ItemRecord(uint Seed, string BaseName, string PrefixName, string SuffixName);

	private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

	[Fact]
	public void HallowedCirclet_FromVaultRecord_MatchesInGameTooltip()
	{
		// Arrange: item exactly as TQVault exports it.
		var item = JsonSerializer.Deserialize<ItemRecord>("""
			{
			  "stackSize": 1,
			  "seed": 13009,
			  "baseName": "Records\\Item\\EquipmentHelm\\C16_Circlet03.dbr",
			  "prefixName": "Records\\Item\\LootMagicalAffixes\\Prefix\\Default\\Rare_Hallowed_01.dbr",
			  "suffixName": "",
			  "var1": 0,
			  "var2": 2035248
			}
			""", JsonOptions)!;

		// Base  C16_Circlet03.dbr     : characterManaRegenModifier=20, defensiveProtection=273
		// Prefix Rare_Hallowed_01.dbr : characterLifeRegen=1, defensiveLife=50, lootRandomizerJitter=20
		// In game: +1 Life Regen, 21% Energy Regen, 52% Vitality Resistance.
		var rng = TitanQuestRNG.FromItemSeed(item.Seed);

		// Act: Char store #10 characterLifeRegen, prefix roll (always consumed for Char affixes).
		var lifeRegen = ItemJitterService.RollCharAffix(1f, 20f, rng);

		// Char store #15 characterManaRegenModifier, base value at the store's 20%.
		Span<float> energyRegen = [20f];
		ItemJitterService.AddJitter(energyRegen, ItemJitterService.StoreBaseJitterPercent, rng);

		// Defense store: base armor is never jittered; the life prefix rolls at the affix's 20%.
		Span<float> vitalityRes = [50f];
		ItemJitterService.AddJitter(vitalityRes, 20f, rng);

		// Assert
		lifeRegen.Should().Be(1f);
		energyRegen[0].Should().Be(21f);
		vitalityRes[0].Should().Be(52f);
	}

	[Theory(Skip = "Requires Titan Quest game installation", SkipType = typeof(TestingConditions), SkipUnless = nameof(TestingConditions.IsGameInstalled))]
	// ejfudd's "Watcher" helmet (German Wächterhelm): Insulated Sentinel's Helm of Endowment.
	[InlineData(6563u, true, @"records\item\equipmenthelm\mi_e_gorgonmelee.dbr",
		@"records\item\lootmagicalaffixes\prefix\default\defensive_resistfire_02.dbr",
		@"records\item\lootmagicalaffixes\suffix\default\character_abilitydefensive_03.dbr",
		"characterDexterity=16;characterDefensiveAbility=31;defensivePierce=15;defensivePoison=17;defensiveFire=24;defensiveProtection=210")]
	[InlineData(13009u, true, @"records\item\equipmenthelm\c16_circlet03.dbr",
		@"records\item\lootmagicalaffixes\prefix\default\rare_hallowed_01.dbr",
		null,
		"characterLifeRegen=1;characterManaRegenModifier=21;defensiveLife=52;defensiveProtection=273")]
	// A Defense and Earth character's "Jewel of Essence": the base ring's attributeScalePercent=25 scales the jittered suffix (82 -> 102, 14 -> 17).
	[InlineData(21654u, false, @"records\xpack\item\equipmentarmor\ring\c02_ring02.dbr",
		null,
		@"records\item\lootmagicalaffixes\suffix\default\character_attributeenergy_03.dbr",
		"characterMana=102;characterManaRegenModifier=17")]
	public void RealItem_FullRollTable_MatchesInGameTooltip(uint seed, bool isArmor, string baseId, string? prefixId, string? suffixId, string expected)
	{
		// Arrange: real database records, full roll table.
		var game = TQVaultAE.DbCli.Core.GameLocator.Locate(TestingConditions.TitanQuestPath);
		using var db = ArzDatabaseFactory.Open(game.Database);

		// Act
		var result = ItemRollSimulator.Roll(seed, isArmor, Load(db, baseId),
			prefixId is null ? null : Load(db, prefixId), suffixId is null ? null : Load(db, suffixId));

		// Assert: tooltip value = base + prefix + suffix contributions.
		foreach (var pair in expected.Split(';')) {
			var (name, value) = (pair.Split('=')[0], float.Parse(pair.Split('=')[1], System.Globalization.CultureInfo.InvariantCulture));
			var total = result.Base!.GetSingle(name, 0) + (result.Prefix?.GetSingle(name, 0) ?? 0f) + (result.Suffix?.GetSingle(name, 0) ?? 0f);
			total.Should().Be(value, name);
		}
	}

	private static TQVaultAE.Domain.Entities.DBRecordCollection Load(IArzDatabase db, string id)
	{
		var rec = db.Read(db.Find(id) ?? throw new InvalidOperationException($"Missing record {id}"));
		var record = new TQVaultAE.Domain.Entities.DBRecordCollection(TQVaultAE.Domain.Entities.RecordId.Create(id), rec.Info.Class);
		foreach (var v in rec.Variables) {
			var type = v.Type switch {
				ArzValueType.Int => TQVaultAE.Domain.Entities.VariableDataType.Integer,
				ArzValueType.Float => TQVaultAE.Domain.Entities.VariableDataType.Float,
				ArzValueType.Bool => TQVaultAE.Domain.Entities.VariableDataType.Boolean,
				_ => TQVaultAE.Domain.Entities.VariableDataType.StringVar,
			};
			var variable = new TQVaultAE.Domain.Entities.Variable(v.Name, type, v.Values.Length);
			for (int i = 0; i < v.Values.Length; i++)
				variable[i] = v.Values[i] is bool b ? (b ? 1 : 0) : v.Values[i];
			record.Set(variable);
		}
		return record;
	}
}
