using AwesomeAssertions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Forge;

namespace TQVaultAE.Tests.Domain;

/// <summary>
/// <see cref="ForgeRules"/> and <see cref="ForgeMerge"/>: what the Forge accepts in each mode, and what forging changes.
/// </summary>
public class ForgeRulesTests
{
	private const string Sword = Item.ICLASS_SWORD;
	private const string Shield = Item.ICLASS_SHIELD;
	private const string TinkererSuffix = @"records\item\lootmagicalaffixes\suffix\default\rare_extrarelic_01.dbr";

	/// <summary>Shield relic (Aegis of Athena).</summary>
	private static readonly RecordId ShieldRelic = RelicAndCharm.AEGISOFATHENA_ACT1_01.GetRecordId();

	private readonly Dictionary<Item, int> _levels = [];
	private readonly ForgeRules _rules;

	public ForgeRulesTests() => this._rules = new ForgeRules(i => this._levels.GetValueOrDefault(i));

	private static Item Make(string itemClass, string id, string classification = "COMMON", string? prefix = null, string? suffix = null)
	{
		var record = new DBRecordCollection(RecordId.Create(id), itemClass);
		record.Set(Text("Class", itemClass));
		record.Set(Text("itemClassification", classification));
		return new Item
		{
			BaseItemId = RecordId.Create(id),
			baseItemInfo = new Info(record),
			prefixID = prefix is null ? RecordId.Empty : RecordId.Create(prefix),
			suffixID = suffix is null ? RecordId.Empty : RecordId.Create(suffix),
		};

		static Variable Text(string name, string value)
		{
			var v = new Variable(name, VariableDataType.StringVar, 1);
			v[0] = value;
			return v;
		}
	}

	private static Item SwordItem(string id = @"records\item\sword01.dbr", string classification = "COMMON", string? prefix = @"records\prefix\p01.dbr", string? suffix = @"records\suffix\s01.dbr")
		=> Make(Sword, id, classification, prefix, suffix);

	private static Item Relic(RecordId id) => Make(Item.ICLASS_RELIC, id.Raw);

	private static ForgeSelection With(Item? baseItem = null, Item? prefix = null, Item? suffix = null, Item? relic1 = null, Item? relic2 = null)
	{
		var s = new ForgeSelection();
		s[ForgeSlot.Base] = baseItem;
		s[ForgeSlot.Prefix] = prefix;
		s[ForgeSlot.Suffix] = suffix;
		s[ForgeSlot.Relic1] = relic1;
		s[ForgeSlot.Relic2] = relic2;
		return s;
	}

	[Theory]
	[InlineData(ForgeMode.God, ForgeMode.Relax, true)]
	[InlineData(ForgeMode.God, ForgeMode.Game, true)]
	[InlineData(ForgeMode.Relax, ForgeMode.Strict, true)]
	[InlineData(ForgeMode.Strict, ForgeMode.Game, true)]
	[InlineData(ForgeMode.Game, ForgeMode.God, false)]
	[InlineData(ForgeMode.Strict, ForgeMode.Relax, false)]
	[InlineData(ForgeMode.Strict, ForgeMode.Strict, false)]
	public void IsDowngrade_IsTrueForAStricterMode(ForgeMode previous, ForgeMode next, bool expected)
		=> ForgeRules.IsDowngrade(previous, next).Should().Be(expected);

	[Fact]
	public void IsDowngrade_NoModeChosenBefore_IsFalse()
		=> ForgeRules.IsDowngrade(null, ForgeMode.Game).Should().BeFalse();

	[Theory]
	[InlineData(ForgeSlot.Prefix)]
	[InlineData(ForgeSlot.Suffix)]
	[InlineData(ForgeSlot.Relic1)]
	[InlineData(ForgeSlot.Relic2)]
	public void CheckDrop_WithoutBaseItem_IsRefused(ForgeSlot slot)
	{
		var decision = this._rules.CheckDrop(ForgeMode.God, slot, SwordItem(), With());

		decision.Accepted.Should().BeFalse();
		decision.Rejection.Should().Be(ForgeMessage.MustSelectBaseItemFirst);
	}

	[Fact]
	public void CheckDrop_Base_StrictMode_RefusesEpicItems()
		=> this._rules.CheckDrop(ForgeMode.Strict, ForgeSlot.Base, SwordItem(classification: "EPIC"), With())
			.Rejection.Should().Be(ForgeMessage.NoEpicLegendary);

	[Fact]
	public void CheckDrop_Base_GodMode_AcceptsAnything()
	{
		var relic = Relic(ShieldRelic);

		this._rules.CheckDrop(ForgeMode.God, ForgeSlot.Base, relic, With()).Accepted.Should().BeTrue();
		this._rules.CheckDrop(ForgeMode.Strict, ForgeSlot.Base, relic, With()).Rejection
			.Should().Be(ForgeMessage.MustSelectArmorOrWeaponOrJewellery);
	}

	[Fact]
	public void CheckDrop_Base_OfAnotherGearType_ClearsAffixesButIsAccepted()
	{
		// Changing the base item to another gear type does not refuse it: the affixes taken from the old type go.
		var current = With(SwordItem(), prefix: SwordItem(@"records\item\sword02.dbr"), suffix: SwordItem(@"records\item\sword03.dbr"));
		var shield = Make(Shield, @"records\item\shield01.dbr");

		var decision = this._rules.CheckDrop(ForgeMode.Strict, ForgeSlot.Base, shield, current);

		decision.Accepted.Should().BeTrue();
		decision.Warnings.Should().Equal(ForgeMessage.PrefixSuffixOfAnotherGearType);
		decision.Cleared.Should().BeEquivalentTo([ForgeSlot.Prefix, ForgeSlot.Suffix]);
	}

	[Fact]
	public void CheckDrop_Base_WhenAffixesAlreadyCleared_LaterChecksSeeThemCleared()
	{
		// Another gear type clears the rare prefix first, so "better than base" has nothing left to report.
		var current = With(SwordItem(), prefix: SwordItem(@"records\item\sword02.dbr", classification: "RARE"));
		var commonShield = Make(Shield, @"records\item\shield01.dbr");

		var decision = this._rules.CheckDrop(ForgeMode.Strict, ForgeSlot.Base, commonShield, current);

		decision.Warnings.Should().Equal(ForgeMessage.PrefixSuffixOfAnotherGearType);
	}

	[Fact]
	public void CheckDrop_Base_ClearsARelicTheNewBaseCannotHold()
	{
		var shieldRelic = Relic(ShieldRelic);
		var current = With(Make(Shield, @"records\item\shield01.dbr"), relic1: shieldRelic);

		var decision = this._rules.CheckDrop(ForgeMode.Relax, ForgeSlot.Base, SwordItem(), current);

		decision.Accepted.Should().BeTrue();
		decision.Cleared.Should().Contain(ForgeSlot.Relic1);
	}

	[Fact]
	public void CheckDrop_Prefix_OfAnotherGearType_IsRefusedInStrictMode()
	{
		var current = With(SwordItem());
		var shield = Make(Shield, @"records\item\shield01.dbr", prefix: @"records\prefix\p02.dbr");

		this._rules.CheckDrop(ForgeMode.Strict, ForgeSlot.Prefix, shield, current).Rejection
			.Should().Be(ForgeMessage.PrefixSuffixOfAnotherGearType);
		this._rules.CheckDrop(ForgeMode.Relax, ForgeSlot.Prefix, shield, current).Accepted
			.Should().BeTrue("Relax mode allows affixes from any gear type");
	}

	[Fact]
	public void CheckDrop_Prefix_ItemWithoutPrefix_IsRefusedOutsideGodMode()
	{
		var current = With(SwordItem());
		var plain = SwordItem(@"records\item\sword02.dbr", prefix: null);

		this._rules.CheckDrop(ForgeMode.Relax, ForgeSlot.Prefix, plain, current).Rejection.Should().Be(ForgeMessage.MustHavePrefix);
		this._rules.CheckDrop(ForgeMode.God, ForgeSlot.Prefix, plain, current).Accepted.Should().BeTrue();
	}

	[Fact]
	public void CheckDrop_Prefix_GameMode_RefusesHigherLevelRequirement()
	{
		var baseItem = SwordItem();
		var stronger = SwordItem();
		this._levels[baseItem] = 20;
		this._levels[stronger] = 30;

		this._rules.CheckDrop(ForgeMode.Game, ForgeSlot.Prefix, stronger, With(baseItem)).Rejection
			.Should().Be(ForgeMessage.PrefixSuffixHigherLevelRequirement);
	}

	[Fact]
	public void CheckDrop_Suffix_NotTinkerer_ClearsSecondRelicInStrictMode()
	{
		var current = With(SwordItem(suffix: TinkererSuffix), relic2: Relic(ShieldRelic));

		var decision = this._rules.CheckDrop(ForgeMode.Strict, ForgeSlot.Suffix, SwordItem(@"records\item\sword02.dbr"), current);

		decision.Accepted.Should().BeTrue();
		decision.Cleared.Should().Equal(ForgeSlot.Relic2);
	}

	[Fact]
	public void CheckDrop_Relic_StrictModeNeedsARelic_RelaxAcceptsItemWithRelic()
	{
		var current = With(Make(Shield, @"records\item\shield01.dbr"));
		var plainItem = SwordItem();

		this._rules.CheckDrop(ForgeMode.Strict, ForgeSlot.Relic1, plainItem, current).Rejection.Should().Be(ForgeMessage.MustBeRelic);
		this._rules.CheckDrop(ForgeMode.Relax, ForgeSlot.Relic1, plainItem, current).Rejection.Should().Be(ForgeMessage.MustHaveRelic);
	}

	[Fact]
	public void CheckDrop_Relic_NotAllowedOnBaseGear_IsRefused()
	{
		var current = With(SwordItem());

		this._rules.CheckDrop(ForgeMode.Strict, ForgeSlot.Relic1, Relic(ShieldRelic), current).Rejection
			.Should().Be(ForgeMessage.RelicNotAllowed);
		this._rules.CheckDrop(ForgeMode.God, ForgeSlot.Relic1, Relic(ShieldRelic), current).Accepted
			.Should().BeTrue();
	}

	[Fact]
	public void CheckDrop_SecondRelic_NeedsTinkererSuffixInStrictMode()
	{
		var relic = Relic(ShieldRelic);

		this._rules.CheckDrop(ForgeMode.Strict, ForgeSlot.Relic2, relic, With(Make(Shield, @"records\item\shield01.dbr")))
			.Rejection.Should().Be(ForgeMessage.MustHaveTinkeredSuffix);
		this._rules.CheckDrop(ForgeMode.Strict, ForgeSlot.Relic2, relic, With(Make(Shield, @"records\item\shield01.dbr", suffix: TinkererSuffix)))
			.Accepted.Should().BeTrue();
	}

	#region Capabilities: each one is what CheckDrop enforces

	public static TheoryData<ForgeMode> AllModes => [.. Enum.GetValues<ForgeMode>()];

	[Theory]
	[InlineData(ForgeMode.Game, false)]
	[InlineData(ForgeMode.Strict, true)]
	[InlineData(ForgeMode.Relax, true)]
	[InlineData(ForgeMode.God, true)]
	public void Capabilities_StricterModeExists_ExceptForTheStrictestMode(ForgeMode mode, bool expected)
		=> ForgeRules.Capabilities(mode).StricterModeExists.Should().Be(expected);

	[Theory]
	[MemberData(nameof(AllModes))]
	public void Capabilities_GearOnly_MatchesWhetherARelicCanBeTheBaseItem(ForgeMode mode)
		=> this._rules.CheckDrop(mode, ForgeSlot.Base, Relic(ShieldRelic), With()).Accepted
			.Should().Be(!ForgeRules.Capabilities(mode).GearOnly);

	[Theory]
	[MemberData(nameof(AllModes))]
	public void Capabilities_EpicLegendaryAllowed_MatchesTheBaseAndAffixChecks(ForgeMode mode)
	{
		var allowed = ForgeRules.Capabilities(mode).EpicLegendaryAllowed;
		var epicBase = SwordItem(classification: "EPIC");

		this._rules.CheckDrop(mode, ForgeSlot.Base, SwordItem(classification: "EPIC"), With()).Accepted.Should().Be(allowed);
		this._rules.CheckDrop(mode, ForgeSlot.Prefix, SwordItem(classification: "EPIC"), With(epicBase)).Accepted.Should().Be(allowed);
	}

	[Theory]
	[MemberData(nameof(AllModes))]
	public void Capabilities_AffixFromSameBaseItem_MatchesThePrefixCheck(ForgeMode mode)
		=> this._rules.CheckDrop(mode, ForgeSlot.Prefix, SwordItem(@"records\item\sword02.dbr"), With(SwordItem())).Rejection
			.Should().Be(ForgeRules.Capabilities(mode).AffixFromSameBaseItem ? ForgeMessage.PrefixSuffixOfAnotherBaseItem : null);

	[Theory]
	[MemberData(nameof(AllModes))]
	public void Capabilities_AffixLevelNotAboveBase_MatchesThePrefixCheck(ForgeMode mode)
	{
		var stronger = SwordItem();
		this._levels[stronger] = 20;

		this._rules.CheckDrop(mode, ForgeSlot.Prefix, stronger, With(SwordItem())).Rejection
			.Should().Be(ForgeRules.Capabilities(mode).AffixLevelNotAboveBase ? ForgeMessage.PrefixSuffixHigherLevelRequirement : null);
	}

	[Theory]
	[MemberData(nameof(AllModes))]
	public void Capabilities_AffixFromSameGearType_MatchesThePrefixCheck(ForgeMode mode)
	{
		// Same record as the base item, so that only the gear type differs
		var shield = Make(Shield, @"records\item\sword01.dbr", prefix: @"records\prefix\p01.dbr");

		this._rules.CheckDrop(mode, ForgeSlot.Prefix, shield, With(SwordItem())).Rejection
			.Should().Be(ForgeRules.Capabilities(mode).AffixFromSameGearType ? ForgeMessage.PrefixSuffixOfAnotherGearType : null);
	}

	[Theory]
	[MemberData(nameof(AllModes))]
	public void Capabilities_AffixNotRarerThanBase_MatchesThePrefixCheck(ForgeMode mode)
		=> this._rules.CheckDrop(mode, ForgeSlot.Prefix, SwordItem(classification: "RARE"), With(SwordItem())).Rejection
			.Should().Be(ForgeRules.Capabilities(mode).AffixNotRarerThanBase ? ForgeMessage.PrefixSuffixBetterThanBase : null);

	[Theory]
	[MemberData(nameof(AllModes))]
	public void Capabilities_AffixMustBePresent_MatchesThePrefixAndSuffixChecks(ForgeMode mode)
	{
		var required = ForgeRules.Capabilities(mode).AffixMustBePresent;
		var plain = SwordItem(prefix: null, suffix: null);

		this._rules.CheckDrop(mode, ForgeSlot.Prefix, plain, With(SwordItem())).Rejection
			.Should().Be(required ? ForgeMessage.MustHavePrefix : null);
		this._rules.CheckDrop(mode, ForgeSlot.Suffix, plain, With(SwordItem())).Rejection
			.Should().Be(required ? ForgeMessage.MustHaveSuffix : null);
	}

	[Theory]
	[MemberData(nameof(AllModes))]
	public void Capabilities_RelicSource_MatchesWhatTheRelicSlotAccepts(ForgeMode mode)
	{
		var source = ForgeRules.Capabilities(mode).RelicSource;
		var current = With(Make(Shield, @"records\item\shield01.dbr"));
		var socketed = Make(Shield, @"records\item\shield02.dbr");
		socketed.relicID = ShieldRelic;

		this._rules.CheckDrop(mode, ForgeSlot.Relic1, Relic(ShieldRelic), current).Accepted.Should().BeTrue();
		this._rules.CheckDrop(mode, ForgeSlot.Relic1, socketed, current).Accepted
			.Should().Be(source != ForgeRelicSource.RelicOrCharm);
		this._rules.CheckDrop(mode, ForgeSlot.Relic1, SwordItem(), current).Accepted
			.Should().Be(source == ForgeRelicSource.Anything);
	}

	[Theory]
	[MemberData(nameof(AllModes))]
	public void Capabilities_RelicMustFitBase_MatchesTheRelicCheck(ForgeMode mode)
		=> this._rules.CheckDrop(mode, ForgeSlot.Relic1, Relic(ShieldRelic), With(SwordItem())).Rejection
			.Should().Be(ForgeRules.Capabilities(mode).RelicMustFitBase ? ForgeMessage.RelicNotAllowed : null);

	[Theory]
	[MemberData(nameof(AllModes))]
	public void Capabilities_SecondRelicNeedsTinkerer_MatchesTheSecondRelicCheck(ForgeMode mode)
		=> this._rules.CheckDrop(mode, ForgeSlot.Relic2, Relic(ShieldRelic), With(Make(Shield, @"records\item\shield01.dbr"))).Rejection
			.Should().Be(ForgeRules.Capabilities(mode).SecondRelicNeedsTinkerer ? ForgeMessage.MustHaveTinkeredSuffix : null);

	#endregion

	[Fact]
	public void CheckForge_NeedsBaseAndAnotherItem()
	{
		this._rules.CheckForge(With()).Should().Be(ForgeMessage.MustSelectBaseItem);
		this._rules.CheckForge(With(SwordItem())).Should().Be(ForgeMessage.MustSelectItemToMerge);
		this._rules.CheckForge(With(SwordItem(), prefix: SwordItem())).Should().BeNull();
	}

	[Theory]
	[InlineData(null, true, null, true)]           // base game parts go anywhere
	[InlineData("modA", true, null, true)]
	[InlineData("modA", true, "modA", true)]       // same mod
	[InlineData("modA", true, "modB", false)]      // another mod's affix
	[InlineData(null, true, "modA", false)]        // a base game vault item cannot read a mod affix
	[InlineData(null, false, "modA", true)]        // a character item follows the active map: not known here
	public void CheckForge_PartOfAnotherMod_IsRefused(string? baseMod, bool baseInVault, string? materialMod, bool allowed)
	{
		// Arrange
		var item = SwordItem();
		item.Mod = baseMod;
		item.InVault = baseInVault;
		var material = SwordItem();
		material.Mod = materialMod;

		// Act
		var problem = this._rules.CheckForge(With(item, prefix: material));

		// Assert
		problem.Should().Be(allowed ? null : ForgeMessage.PartOfAnotherMod);
	}

	[Fact]
	public void Merge_TakesAffixesAndRelicFromMaterials()
	{
		var target = SwordItem(prefix: null, suffix: null);
		var prefix = SwordItem(@"records\item\sword02.dbr", prefix: @"records\prefix\fiery.dbr");
		var relic = Relic(ShieldRelic);
		relic.Var1 = 2;

		ForgeMerge.Apply(target, With(target, prefix: prefix, relic1: relic), godMode: false, _ => null);

		target.prefixID.Should().Be(RecordId.Create(@"records\prefix\fiery.dbr"));
		target.relicID.Should().Be(ShieldRelic);
		target.Var1.Should().Be(2);
		target.HasSuffix.Should().BeFalse("no suffix material was selected");
	}

	[Fact]
	public void Merge_GodMode_AffixAsRelic_HasNoBonusRecord()
	{
		// A null bonus made saving the vault crash (it writes every record id).
		var target = SwordItem(prefix: null, suffix: null);
		var material = SwordItem(@"records\item\sword02.dbr", prefix: @"records\prefix\fiery.dbr");

		ForgeMerge.Apply(target, With(target, relic1: material, relic2: material), godMode: true, _ => ForgePart.Prefix);

		target.relicID.Should().Be(RecordId.Create(@"records\prefix\fiery.dbr"));
		target.RelicBonusId.Should().NotBeNull().And.Be(RecordId.Empty);
		target.RelicBonus2Id.Should().NotBeNull().And.Be(RecordId.Empty);
		target.Var1.Should().Be(1);
	}

	[Fact]
	public void RawOf_NullRecord_IsEmpty()
	{
		RecordId.RawOf(null).Should().BeEmpty();
		RecordId.RawOf(RecordId.Create(@"records\a.dbr")).Should().Be(@"records\a.dbr");
	}

	[Fact]
	public void Merge_GodMode_UsesTheChosenPart()
	{
		var target = SwordItem(prefix: null, suffix: null);
		var material = SwordItem(@"records\item\sword02.dbr", prefix: @"records\prefix\fiery.dbr", suffix: @"records\suffix\of_bear.dbr");

		ForgeMerge.Apply(target, With(target, prefix: material), godMode: true, slot => slot == ForgeSlot.Prefix ? ForgePart.Suffix : null);

		target.prefixID.Should().Be(RecordId.Create(@"records\suffix\of_bear.dbr"), "God mode can use a suffix as a prefix");
	}
}
