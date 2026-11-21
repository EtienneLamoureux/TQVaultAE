using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using AwesomeAssertions;
using Moq;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Comparison;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Domain.Results;
using TQVaultAE.Presentation.ItemText;
using TQVaultAE.Tests.Data;

namespace TQVaultAE.Tests.Presentation;

/// <summary>
/// Pins the whole text of a varied set of items, as the item tooltips, the Item Properties dialog and the search read it:
/// every text of <see cref="ToFriendlyNameResult"/> (lines with their colour tags, in their order), the lines marked as
/// text, the granted skills, the requirements, the set, and the search's full text and attribute list.
/// </summary>
/// <remarks>
/// Each item's text is compared with its file in <c>ItemTextSnapshots</c>. Run with <c>UPDATE_ITEM_TEXT_SNAPSHOTS=1</c>
/// to write the files again, then review their diff: a refactoring of the text code must leave them unchanged.
/// </remarks>
public class ItemTextSnapshotTests : ItemProviderTextTestBase
{
	private const string SnapshotFolder = "ItemTextSnapshots";

	private const string SwordId = """records\test\sword.dbr""";
	private const string FirePrefixId = """records\test\prefix_fire.dbr""";
	private const string LifeSuffixId = """records\test\suffix_life.dbr""";
	private const string RelicId = """records\test\relic_ember.dbr""";
	private const string RelicBonusId = """records\test\relic_ember_bonus.dbr""";
	private const string CharmId = """records\test\charm_claw.dbr""";
	private const string CharmBonusId = """records\test\charm_claw_bonus.dbr""";
	private const string ItemCostId = """records\test\itemcost.dbr""";
	private const string SkillId = """records\test\skill_heal.dbr""";
	private const string BuffId = """records\test\buff_heal.dbr""";
	private const string ControllerId = """records\test\controller_lowhealth.dbr""";
	private const string ArtifactId = """records\test\artifact_eye.dbr""";
	private const string ArtifactBonusId = """records\test\artifact_eye_bonus.dbr""";
	private const string SetId = """records\test\set_hunter.dbr""";
	private const string SetHelmId = """records\test\set_hunter_helm.dbr""";
	private const string SetGreavesId = """records\test\set_hunter_greaves.dbr""";
	private const string PetSkillId = """records\test\summonwolf.dbr""";
	private const string PetId = """records\test\wolf.dbr""";
	private const string BiteId = """records\test\bite.dbr""";

	private readonly Mock<IItemModService> _itemMods;

	public ItemTextSnapshotTests() : this(new Mock<IItemModService>())
	{
	}

	private ItemTextSnapshotTests(Mock<IItemModService> itemMods) : base(itemMods.Object)
	{
		this._itemMods = itemMods;
		this._itemMods.Setup(m => m.Scope(It.IsAny<Item?>())).Returns(NoScope.Instance);

		var t = this.MockTranslation;
		t.SetupGet(x => x.ItemWith).Returns("with");
		t.SetupGet(x => x.ItemQuest).Returns("Quest Item");
		t.SetupGet(x => x.ItemIT).Returns("Immortal Throne Item");
		t.SetupGet(x => x.ItemRagnarok).Returns("Ragnarok Item");
		t.SetupGet(x => x.ItemAtlantis).Returns("Atlantis Item");
		t.SetupGet(x => x.ItemEmbers).Returns("Eternal Embers Item");
		t.Setup(x => x.Translate(It.IsAny<ItemStyle>())).Returns((ItemStyle style) => $"[{style}]");

		foreach (var (tag, text) in new Dictionary<string, string>
		{
			["MeetsRequirement"] = "Required {%s0}: {%d1}",
			["LevelRequirement"] = "Player Level",
			["Strength"] = "Strength",
			["Dexterity"] = "Dexterity",
			["Intelligence"] = "Intelligence",
			["tagSword"] = "Sword of Testing",
			["tagPrefixFire"] = "Burning",
			["tagSuffixLife"] = "of Life",
			["tagQualityFine"] = "Fine",
			["tagStyleBronze"] = "Bronze",
			["tagRelicEmber"] = "Ember",
			["tagRelicEmberText"] = "A shard of an ever burning ember, still warm to the touch.",
			["tagCharmClaw"] = "Claw",
			["tagAnimalPartComplete"] = "Completed Charm",
			["tagRelicComplete"] = "Completed Relic",
			["tagAnimalPartcompleteBonus"] = "Completion Bonus:",
			["tagRelicBonus"] = "Completion Bonus:",
			["tagAnimalPart"] = "Charm",
			["tagRelicShard"] = "Relic",
			["tagAnimalPartRatio"] = "{%s0} - {%d1} / {%d2}",
			["tagRelicRatio"] = "{%s0} - {%d1} / {%d2}",
			["tagTorso"] = "Plate of Testing",
			["tagShield"] = "Shield of Testing",
			["tagRing"] = "Ring of Testing",
			["tagAmulet"] = "Amulet of Testing",
			["tagArtifactEye"] = "Eye of Testing",
			["xtagArtifactClass02"] = "Greater Artifact",
			["xtagArtifactBonus"] = "Completion Bonus",
			["tagFormulaEye"] = "Arcane Formula - Eye of Testing",
			["xtagArtifactRecipe"] = "Arcane Formula",
			["xtagArtifactReagents"] = "Required Reagents ({%d0}/{%d1})",
			["tagWolfFang"] = "Wolf Fang",
			["tagSetHunter"] = "Hunter's Garb",
			["tagSetHunterHelm"] = "Hunter's Hood",
			["tagSetHunterGreaves"] = "Hunter's Boots",
			["tagPotion"] = "Health Potion",
			["tagPotionText"] = "Restores health over a short time.",
			["tagScroll"] = "Scroll of the Wolf",
			["tagItemGrantSkill"] = "Grants Skill :",
			["xtagAutoSkillCondition01"] = "(Activated on low health)",
			["tagSkillHeal"] = "Healing Light",
			["tagSkillHealDescription"] = "Bathes you in light that mends your wounds and those of your allies.",
			["SkillSecondFormat"] = "{%.1f0 Second %s1}",
			["SkillCostFormat"] = "{%.0f0 %s1}",
			["ManaCost"] = "Energy Cost",
			["ActiveDuration"] = "Duration",
			["CooldownTime"] = "^a{%.1f0} Second(s) Recharge",
			["ItemAllSkillIncrement"] = "{+%d0} to all Skills",
			["DefensePoison"] = "{%.0f0}% Poison Resistance",
			["DefenseBlockRecoveryTime"] = "{%.1f0} Second Shield Recovery",
			["DefenseBlockChance"] = "{%.0f0}% Chance to Block",
			["DamageCold"] = "Cold Damage",
			["DamageModifierCold"] = "{%+.0f0}% Cold Damage",
			["characterDexterity"] = "{%+.0f0} Dexterity",
			["characterAttackSpeed"] = "{%+.0f0} Attack Speed",
			["CharacterAttackSpeedFast"] = "Fast Attack Speed",
			["characterLifeRegen"] = "{%+.1f0} Health Regenerated per second",
			["SkillPetLimit"] = "{%d0} Summon Limit",
			["SkillPetDescriptionHeading"] = "{%s0} Attributes:",
			["tagSkillPetTimeToLive"] = "Life Time {%.1f0} Seconds",
			["SkillPetDescriptionHealth"] = "{%.0f0}  Health",
			["SkillPetDescriptionMana"] = "{%.0f0}  Energy",
			["tagSkillPetAbilities"] = "{%s0} Abilities:",
			["SkillPetDescriptionDamageMinMax"] = "{%.0f0} - {%.0f1}  Damage",
			["tagWolf"] = "Wolf",
			["tagBite"] = "Bite",
		})
			this.Translations[tag] = text;
	}

	public static TheoryData<string> Items => [.. Builders.Keys];

	private static readonly Dictionary<string, Func<ItemTextSnapshotTests, Item>> Builders = new()
	{
		["weapon-prefix-suffix-relic"] = t => t.SwordWithPrefixSuffixAndRelic(),
		["armor-torso"] = t => t.Torso(),
		["shield"] = t => t.Shield(),
		["jewellery-ring"] = t => t.Ring(),
		["jewellery-amulet-granted-skill"] = t => t.AmuletWithGrantedSkill(),
		["relic-incomplete"] = t => t.RelicItem(pieces: 1),
		["relic-completed"] = t => t.RelicItem(pieces: 3),
		["charm-completed"] = t => t.Charm(),
		["artifact"] = t => t.Artifact(),
		["formula"] = t => t.Formula(),
		["set-item"] = t => t.SetHelm(),
		["potion-stack"] = t => t.Potion(),
		["scroll-pet"] = t => t.Scroll(),
		["weapon-in-game-values"] = t => t.SwordWithInGameValues(),
	};

	[Theory]
	[MemberData(nameof(Items))]
	public void ItemText_IsUnchanged(string name)
	{
		var previous = CultureInfo.CurrentCulture;
		CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
		try
		{
			var item = Builders[name](this);
			var text = this.Snapshot(item);

			AssertSnapshot(name, text);
		}
		finally
		{
			CultureInfo.CurrentCulture = previous;
		}
	}

	[Fact]
	public void SkillAtLevel_IsUnchanged()
	{
		var previous = CultureInfo.CurrentCulture;
		CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
		try
		{
			this.CreateHealSkill();
			var sb = new StringBuilder();
			foreach (var level in new[] { 1, 3 })
			{
				sb.Append("== GetSkillAtLevel ").Append(level).Append('\n');
				AppendGrantedSkill(sb, "Skill", this.Formatter.GetSkillAtLevel(SkillId.ToRecordId(), level));
			}

			AssertSnapshot("skill-at-level", sb.ToString());
		}
		finally
		{
			CultureInfo.CurrentCulture = previous;
		}
	}

	[Fact]
	public void ItemOfAMod_IsReadInsideItsModScope()
	{
		// An item linked to a mod reads its records in the mod's database: the text must be built inside its scope.
		var item = this.Ring();

		this.Formatter.GetFriendlyNames(item, FriendlyNamesExtraScopes.ItemFullDisplay);
		this.Formatter.HasGrantedSkill(item);
		this.Formatter.GetGrantedSkillNames(item);

		this._itemMods.Verify(m => m.Scope(item), Times.AtLeast(3));
	}

	#region Snapshot

	private string Snapshot(Item item)
	{
		var sb = new StringBuilder();

		// The tooltip and the item comparison (all the scopes, granted skills apart).
		var full = this.Formatter.GetFriendlyNames(item, FriendlyNamesExtraScopes.ItemFullDisplay | FriendlyNamesExtraScopes.GrantedSkills);
		sb.Append("== GetFriendlyNames(ItemFullDisplay | GrantedSkills)\n");
		AppendResult(sb, full);

		// The Item Properties dialog with its filter unchecked.
		var unfiltered = this.Formatter.GetFriendlyNames(item, FriendlyNamesExtraScopes.ItemFullDisplay, filterExtra: false);
		sb.Append("== GetFriendlyNames(ItemFullDisplay, filterExtra: false)\n");
		AppendResult(sb, unfiltered);

		// The search: its entries read GetFriendlyNames(ItemFullDisplay), the text matched and the attribute filters.
		var search = this.Formatter.GetFriendlyNames(item, FriendlyNamesExtraScopes.ItemFullDisplay);
		sb.Append("== Search\n");
		AppendLines(sb, "FullText", FullText(search));
		AppendLines(sb, "AttributesAll", search.AttributesAll.Select(a => $"{(search.IsTextLine(a.Trim()) ? "[text] " : string.Empty)}{a}"));
		sb.Append("HasGrantedSkill: ").Append(this.Formatter.HasGrantedSkill(item)).Append('\n');
		AppendLines(sb, "GrantedSkillNames", this.Formatter.GetGrantedSkillNames(item));

		return sb.ToString();
	}

	/// <summary>The texts the search matches (<see cref="ToFriendlyNameResult.FulltextIsMatch"/>).</summary>
	private static IEnumerable<string> FullText(ToFriendlyNameResult result)
	{
		result.FulltextIsMatchIndexOf("\u0001");
		var field = typeof(ToFriendlyNameResult).GetField("_FullText", BindingFlags.Instance | BindingFlags.NonPublic)!;
		return (IEnumerable<string>)field.GetValue(result)!;
	}

	private static void AppendResult(StringBuilder sb, ToFriendlyNameResult result)
	{
		var properties = typeof(ToFriendlyNameResult)
			.GetProperties(BindingFlags.Instance | BindingFlags.Public)
			.OrderBy(p => p.MetadataToken);

		foreach (var property in properties)
		{
			var value = property.GetValue(result);
			switch (value)
			{
				case null:
					break;
				case string text:
					sb.Append(property.Name).Append(": ").Append(Escape(text)).Append('\n');
					break;
				case RecordId id:
					sb.Append(property.Name).Append(": ").Append(id.Raw).Append('\n');
					break;
				case GrantedSkill[] skills:
					for (int i = 0; i < skills.Length; i++)
						AppendGrantedSkill(sb, $"{property.Name}[{i}]", skills[i]);
					break;
				case StatSource[] sources:
					AppendLines(sb, property.Name, sources.Select(s => s.ToString()));
					break;
				case string?[] lines:
					AppendLines(sb, property.Name, lines.Select((l, i) => l is null ? "<null>" : $"{(result.IsTextLine(l.RemoveAllTQTags()?.Trim() ?? string.Empty) ? "[text] " : string.Empty)}{l}"));
					break;
				case SortedList<string, Variable> variables:
					AppendLines(sb, property.Name, variables.Select(v => $"{v.Key} = {v.Value.ToStringValue()}"));
					break;
				case RequirementInfo info:
					sb.Append(property.Name).Append(": Lvl=").Append(info.Lvl).Append(" Str=").Append(info.Str)
						.Append(" Dex=").Append(info.Dex).Append(" Int=").Append(info.Int).Append('\n');
					break;
				case SetItemInfo set:
					sb.Append(property.Name).Append(": ").Append(set.itemSetName).Append(" (").Append(set.setName).Append(")\n");
					AppendLines(sb, property.Name + ".Translations", set.Translations.Select(kv => $"{kv.Key} => {kv.Value}"));
					break;
				case DBRecordCollection record:
					sb.Append(property.Name).Append(": ").Append(record.Id.Raw).Append('\n');
					break;
				case Item:
				case IEnumerable:
					break;
				default:
					sb.Append(property.Name).Append(": ").Append(Convert.ToString(value, CultureInfo.InvariantCulture)).Append('\n');
					break;
			}
		}
	}

	private static void AppendGrantedSkill(StringBuilder sb, string name, GrantedSkill? skill)
	{
		if (skill is null)
		{
			sb.Append(name).Append(": <null>\n");
			return;
		}

		sb.Append(name).Append(".SkillId: ").Append(skill.SkillId.Raw).Append('\n');
		sb.Append(name).Append(".Name: ").Append(Escape(skill.Name)).Append('\n');
		sb.Append(name).Append(".Level: ").Append(skill.Level).Append('\n');
		sb.Append(name).Append(".LevelText: ").Append(Escape(skill.LevelText)).Append('\n');
		sb.Append(name).Append(".Icon: ").Append(skill.Icon?.Raw ?? "<null>").Append('\n');
		sb.Append(name).Append(".Activation: ").Append(skill.Activation is null ? "<null>" : Escape(skill.Activation)).Append('\n');
		AppendLines(sb, name + ".Description", skill.Description);
		AppendLines(sb, name + ".Attributes", skill.Attributes);
		AppendLines(sb, name + ".ModifiedLines", skill.ModifiedLines.Select(i => i.ToString(CultureInfo.InvariantCulture)));
	}

	private static void AppendLines(StringBuilder sb, string name, IEnumerable<string> lines)
	{
		var list = lines.ToList();
		sb.Append(name).Append(" (").Append(list.Count).Append(")\n");
		foreach (var line in list)
			sb.Append("  | ").Append(Escape(line)).Append('\n');
	}

	/// <summary>Shows the line breaks and tabs a text holds, so that each line of the snapshot is one text.</summary>
	private static string Escape(string text) => text.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");

	private static void AssertSnapshot(string name, string text, [CallerFilePath] string sourceFile = "")
	{
		var fileName = name + ".txt";
		if (Environment.GetEnvironmentVariable("UPDATE_ITEM_TEXT_SNAPSHOTS") == "1")
		{
			var folder = Path.Combine(Path.GetDirectoryName(sourceFile)!, SnapshotFolder);
			Directory.CreateDirectory(folder);
			File.WriteAllText(Path.Combine(folder, fileName), text, new UTF8Encoding(false));
			return;
		}

		var expected = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, SnapshotFolder, fileName));
		text.Should().Be(expected.Replace("\r\n", "\n"), $"the text of {name} is pinned in {SnapshotFolder}/{fileName}");
	}

	#endregion

	#region Items

	private DBRecordCollection CreateItemCost()
	{
		string prefix = this.Provider.GetRequirementEquationPrefix(Item.ICLASS_SWORD);
		return this.CreateRecord(ItemCostId, "ItemCost",
			Str(prefix + "LevelEquation", "itemLevel"),
			Str(prefix + "StrengthEquation", "(itemLevel * 2) + (totalAttCount * 3)"),
			Str(prefix + "DexterityEquation", "itemLevel + 10"),
			Str(prefix + "CostEquation", "itemLevel * 100"));
	}

	private Item SwordWithPrefixSuffixAndRelic()
	{
		this.CreateItemCost();
		var sword = this.CreateItemWithId(SwordId, Item.ICLASS_SWORD,
			Str("itemNameTag", "tagSword"), Str("itemQualityTag", "tagQualityFine"), Str("itemStyleTag", "tagStyleBronze"),
			Str("itemClassification", "Common"), Int("itemLevel", 20), Str("itemCostName", ItemCostId),
			Float("offensivePhysicalMin", 12f), Float("offensivePhysicalMax", 20f),
			Str("characterBaseAttackSpeedTag", "CharacterAttackSpeedFast"),
			Float("offensiveColdMin", 3f), Float("offensiveColdMax", 6f),
			Int("intelligenceRequirement", 30));
		var prefix = this.CreateRecord(FirePrefixId, "LootRandomizer",
			Str("lootRandomizerName", "tagPrefixFire"), Str("itemClassification", "Magical"),
			Float("offensiveFireMin", 5f), Float("offensiveFireMax", 9f), Float("offensiveFireModifier", 15f));
		var suffix = this.CreateRecord(LifeSuffixId, "LootRandomizer",
			Str("lootRandomizerName", "tagSuffixLife"), Str("itemClassification", "Rare"),
			Float("characterLife", 40f), Float("defensivePoison", 12f), Float("characterLifeRegen", 1.5f));
		var relic = this.CreateRecord(RelicId, Item.ICLASS_RELIC,
			Str("description", "tagRelicEmber"), Str("itemText", "tagRelicEmberText"), Int("completedRelicLevel", 3),
			Float("offensiveFireModifier", 10f), Int("characterStrength", 8));
		var bonus = this.CreateRecord(RelicBonusId, "LootRandomizer", Float("characterDexterity", 12f));

		sword.prefixID = prefix.Id;
		sword.prefixInfo = new Info(prefix);
		sword.suffixID = suffix.Id;
		sword.suffixInfo = new Info(suffix);
		sword.relicID = relic.Id;
		sword.RelicInfo = new Info(relic);
		sword.RelicBonusId = bonus.Id;
		sword.RelicBonusInfo = new Info(bonus);
		sword.Var1 = 3;
		sword.Seed = 1234;
		return sword;
	}

	private Item SwordWithInGameValues()
	{
		this.Settings.ShowInGameValues = true;
		var sword = this.SwordWithPrefixSuffixAndRelic();
		sword.Seed = 20511;
		return sword;
	}

	private Item Torso()
	{
		var torso = this.CreateItem(Item.ICLASS_UPPERBODY,
			Str("itemNameTag", "tagTorso"), Str("itemClassification", "Epic"),
			Float("defensiveProtection", 120f), Int("characterStrength", 15), Float("characterLife", 80f),
			Float("defensivePoison", 20f), Int("levelRequirement", 32), Int("strengthRequirement", 140));
		var prefix = this.CreateRecord(FirePrefixId, "LootRandomizer",
			Str("lootRandomizerName", "tagPrefixFire"), Str("itemClassification", "Magical"),
			Float("defensiveProtectionModifier", 8f));
		torso.prefixID = prefix.Id;
		torso.prefixInfo = new Info(prefix);
		torso.Seed = 4242;
		return torso;
	}

	private Item Shield()
	{
		var shield = this.CreateItem(Item.ICLASS_SHIELD,
			Str("itemNameTag", "tagShield"), Str("itemClassification", "Rare"),
			Float("defensiveBlock", 150f), Float("defensiveBlockChance", 25f), Float("blockRecoveryTime", 1.2f),
			Float("defensiveProtection", 60f), Int("levelRequirement", 18));
		shield.Seed = 77;
		return shield;
	}

	private Item Ring()
	{
		var ring = this.CreateItem(Item.ICLASS_RING,
			Str("itemNameTag", "tagRing"), Str("itemClassification", "Epic"),
			Int("augmentAllLevel", 1), Float("characterLife", 50f), Float("characterAttackSpeed", 5f),
			Int("levelRequirement", 25));
		var suffix = this.CreateRecord(LifeSuffixId, "LootRandomizer",
			Str("lootRandomizerName", "tagSuffixLife"), Str("itemClassification", "Rare"),
			Float("offensiveStunMin", 1f), Float("offensiveStunMax", 1.5f), Float("offensiveStunChance", 20f));
		ring.suffixID = suffix.Id;
		ring.suffixInfo = new Info(suffix);
		ring.Seed = 9;
		return ring;
	}

	private void CreateHealSkill()
	{
		this.CreateRecord(ControllerId, "ItemSkillAutoController", Str("triggerType", "LowHealth"), Float("chanceToRun", 25f));
		this.CreateRecord(SkillId, "Skill_BuffSelfDuration",
			Str("buffSkillName", BuffId), Str("skillUpBitmapName", """records\test\heal_up.tex"""));
		this.CreateRecord(BuffId, "SkillBuff_Passive",
			Str("skillDisplayName", "tagSkillHeal"), Str("skillBaseDescription", "tagSkillHealDescription"),
			Floats("skillActiveDuration", 5f, 6f, 7f), Floats("skillManaCost", 20f, 25f, 30f), Floats("skillCooldownTime", 30f, 28f, 26f),
			Floats("characterLifeRegen", 5f, 8f, 11f), Ints("characterStrength", 2, 4, 6));
	}

	private Item AmuletWithGrantedSkill()
	{
		this.CreateHealSkill();
		var amulet = this.CreateItem(Item.ICLASS_AMULET,
			Str("itemNameTag", "tagAmulet"), Str("itemClassification", "Legendary"),
			Float("characterLife", 30f), Str("itemSkillName", SkillId), Int("itemSkillLevel", 2),
			Str("itemSkillAutoController", ControllerId), Int("levelRequirement", 40));
		amulet.Seed = 31;
		return amulet;
	}

	private Item RelicItem(int pieces)
	{
		var relic = this.CreateItemWithId(RelicId, Item.ICLASS_RELIC,
			Str("description", "tagRelicEmber"), Str("itemText", "tagRelicEmberText"), Int("completedRelicLevel", 3),
			Float("offensiveFireModifier", 10f), Int("characterStrength", 8), Int("levelRequirement", 20));
		var bonus = this.CreateRecord(RelicBonusId, "LootRandomizer", Float("characterDexterity", 12f));
		relic.Var1 = pieces;
		if (pieces >= 3)
		{
			relic.RelicBonusId = bonus.Id;
			relic.RelicBonusInfo = new Info(bonus);
		}

		return relic;
	}

	private Item Charm()
	{
		var charm = this.CreateItemWithId(CharmId, Item.ICLASS_CHARM,
			Str("description", "tagCharmClaw"), Int("completedRelicLevel", 5),
			Float("offensivePhysicalModifier", 6f), Float("offensivePierceRatioModifier", 4f));
		var bonus = this.CreateRecord(CharmBonusId, "LootRandomizer", Float("defensivePoison", 15f));
		charm.Var1 = 5;
		charm.RelicBonusId = bonus.Id;
		charm.RelicBonusInfo = new Info(bonus);
		return charm;
	}

	private Item Artifact()
	{
		var artifact = this.CreateItemWithId(ArtifactId, Item.ICLASS_ARTIFACT,
			Str("description", "tagArtifactEye"), Str("artifactClassification", "Greater"),
			Float("characterLife", 150f), Int("characterStrength", 20), Float("defensivePoison", 25f), Int("levelRequirement", 45));
		var bonus = this.CreateRecord(ArtifactBonusId, "LootRandomizer", Float("characterAttackSpeed", 8f));
		artifact.RelicBonusId = bonus.Id;
		artifact.RelicBonusInfo = new Info(bonus);
		return artifact;
	}

	private Item Formula()
	{
		this.CreateRecord(ArtifactId, Item.ICLASS_ARTIFACT,
			Str("description", "tagArtifactEye"), Str("artifactClassification", "Greater"),
			Float("characterLife", 150f), Int("characterStrength", 20), Int("levelRequirement", 45));
		this.CreateRecord("""records\test\reagent.dbr""", Item.ICLASS_RELIC, Str("description", "tagWolfFang"));
		return this.CreateItem(Item.ICLASS_FORMULA,
			Str("description", "tagFormulaEye"), Str("artifactName", ArtifactId),
			Str("reagent1BaseName", """records\test\reagent.dbr"""), Str("reagent2BaseName", RelicId),
			Int("artifactCreationCost", 50000));
	}

	private Item SetHelm()
	{
		var greaves = this.CreateRecord(SetGreavesId, Item.ICLASS_LOWERBODY, Str("itemNameTag", "tagSetHunterGreaves"));
		var helm = this.CreateItemWithId(SetHelmId, Item.ICLASS_HEAD,
			Str("itemNameTag", "tagSetHunterHelm"), Str("itemClassification", "Legendary"), Str("itemSetName", SetId),
			Float("defensiveProtection", 90f), Int("characterDexterity", 25), Int("levelRequirement", 38));
		this.CreateRecord(SetId, "ItemSet",
			Str("setName", "tagSetHunter"), Strs("setMembers", SetHelmId, SetGreavesId, """records\test\set_missing.dbr"""),
			Floats("characterLife", 0f, 100f));
		_ = greaves;
		return helm;
	}

	private Item Potion()
	{
		var potion = this.CreateItem(Item.ICLASS_POTIONHEALTH,
			Str("description", "tagPotion"), Str("itemText", "tagPotionText"), Float("characterLife", 300f));
		potion.StackSize = 7;
		return potion;
	}

	private Item Scroll()
	{
		this.CreateRecord(PetSkillId, "Skill_SpawnPet",
			Int("petLimit", 2), Str("spawnObjects", PetId), Float("spawnObjectsTimeToLive", 45f));
		this.CreateRecord(PetId, "Monster",
			Str("description", "tagWolf"), Float("characterLife", 650f), Float("characterMana", 120f),
			Float("handHitDamageMin", 14f), Float("handHitDamageMax", 22f),
			Str("skillName0", BiteId), Int("skillLevel0", 2));
		this.CreateRecord(BiteId, "Skill_AttackWeapon", Str("skillDisplayName", "tagBite"), Floats("offensivePhysicalMin", 5f, 10f, 15f));
		var scroll = this.CreateItem(Item.ICLASS_SCROLL, Str("description", "tagScroll"), Str("skillName", PetSkillId));
		scroll.StackSize = 3;
		return scroll;
	}

	#endregion
}
