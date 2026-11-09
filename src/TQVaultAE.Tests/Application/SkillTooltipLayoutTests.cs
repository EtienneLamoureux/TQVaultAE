using AwesomeAssertions;
using TQVaultAE.Application;

namespace TQVaultAE.Tests.Application;

public class SkillTooltipLayoutTests
{
	// Earth Enchantment, levels 5 and 6, as the game's skill window shows them.
	private static readonly string[] Level5 =
	[
		"1 Active Energy Cost per Second",
		"8.4 Meter Radius",
		"+44% Fire Damage",
		"+44% Burn Damage",
		"5 ~ 8 Fire Damage",
		"75 Energy Reserved",
		"^a4.0 Second(s) Recharge",
	];

	private static readonly string[] Level6 =
	[
		"1 Active Energy Cost per Second",
		"9.0 Meter Radius",
		"+52% Fire Damage",
		"+52% Burn Damage",
		"5 ~ 8 Fire Damage",
		"75 Energy Reserved",
		"^a4.0 Second(s) Recharge",
	];

	[Theory]
	[InlineData("^a4.0 Second(s) Recharge", true)]
	[InlineData("{^A}4.0 Second(s) Recharge", true)]
	[InlineData("8.4 Meter Radius", false)]
	[InlineData("+44% ^aFire Damage", false)]
	public void IsRecharge_IsTheLineStartingInAqua(string line, bool expected)
		=> SkillTooltipLayout.IsRecharge(line).Should().Be(expected);

	[Fact]
	public void CurrentEffects_PutTheRechargeFirst_KeepingTheOtherOrder()
		=> SkillTooltipLayout.CurrentEffects(Level5).Should().Equal(
			"^a4.0 Second(s) Recharge",
			"1 Active Energy Cost per Second",
			"8.4 Meter Radius",
			"+44% Fire Damage",
			"+44% Burn Damage",
			"5 ~ 8 Fire Damage",
			"75 Energy Reserved");

	[Fact]
	public void CurrentEffects_LeaveASummonsSkillRechargeInItsBlock()
		=> SkillTooltipLayout.CurrentEffects(["210 Energy Cost", "^a60.0 Second(s) Recharge", "Nightmare Abilities:", "Distort Reality", "^a12.0 Second(s) Recharge"])
			.Should().Equal("^a60.0 Second(s) Recharge", "210 Energy Cost", "Nightmare Abilities:", "Distort Reality", "^a12.0 Second(s) Recharge");

	[Fact]
	public void NextEffects_ShowTheRechargeAndOnlyWhatChanges()
		=> SkillTooltipLayout.NextEffects(Level5, Level6).Should().Equal(
			"^a4.0 Second(s) Recharge",
			"9.0 Meter Radius",
			"+52% Fire Damage",
			"+52% Burn Damage");

	[Fact]
	public void NextEffects_ShowEverythingUnderAHeading()
	{
		// Circle of Power, levels 1 and 2: the summon's blocks come back whole, unchanged lines included.
		string[] level1 =
		[
			"^a60.0 Second(s) Recharge", "64 Energy Cost", "Extra Life Time 12 Seconds",
			"Circle of Power Attributes:", "Life Time 12.0 Seconds", "200 Health", "",
			"Circle of Power Abilities:", "12.0 Second Duration", "+100% Life Leech",
		];
		string[] level2 =
		[
			"^a60.0 Second(s) Recharge", "68 Energy Cost", "Extra Life Time 12 Seconds",
			"Circle of Power Attributes:", "Life Time 12.0 Seconds", "225 Health", "",
			"Circle of Power Abilities:", "12.0 Second Duration", "+100% Life Leech",
		];

		SkillTooltipLayout.NextEffects(level1, level2).Should().Equal(
			"^a60.0 Second(s) Recharge", "68 Energy Cost",
			"Circle of Power Attributes:", "Life Time 12.0 Seconds", "225 Health", "",
			"Circle of Power Abilities:", "12.0 Second Duration", "+100% Life Leech");
	}

	[Fact]
	public void AbilitiesStart_IsAfterTheSecondHeading()
	{
		SkillTooltipLayout.AbilitiesStart(["64 Energy Cost", "Totem Attributes:", "200 Health", "", "Totem Abilities:", "+15% Casting Speed"])
			.Should().Be(5);
		SkillTooltipLayout.AbilitiesStart(Level5).Should().Be(Level5.Length);
	}

	[Fact]
	public void NextEffects_WithoutACurrentLevel_ShowEverything()
		=> SkillTooltipLayout.NextEffects([], Level6).Should().HaveCount(Level6.Length)
			.And.StartWith("^a4.0 Second(s) Recharge");
}
