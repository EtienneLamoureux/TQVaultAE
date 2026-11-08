using System.Globalization;
using AwesomeAssertions;
using TQVaultAE.Domain.Forge;
using TQVaultAE.Presentation;

namespace TQVaultAE.Tests.Presentation;

/// <summary>
/// <see cref="ForgeModeCardText"/>: each mode card lists the checks of <see cref="ForgeRules.Capabilities"/>.
/// </summary>
public class ForgeModeCardTextTests
{
	private const string Tinkerer = "of the Tinkerer";

	/// <summary>The card in English as lines: "# section", "+ allowed", "- restricted", "= footer".</summary>
	private static List<string> CardInEnglish(ForgeMode mode)
	{
		var culture = CultureInfo.CurrentUICulture;
		try
		{
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");
			var card = ForgeModeCardText.Build(mode, Tinkerer);
			var lines = new List<string> { card.Title, card.Summary };
			foreach (var section in card.Sections)
			{
				lines.Add("# " + section.Title);
				lines.AddRange(section.Lines.Select(l => (l.Allowed ? "+ " : "- ") + l.Text));
			}
			if (card.Footer is not null)
				lines.Add("= " + card.Footer);
			return lines;
		}
		finally
		{
			CultureInfo.CurrentUICulture = culture;
		}
	}

	[Fact]
	public void Build_GameMode_ListsEveryGameRuleAndNoReset()
		=> CardInEnglish(ForgeMode.Game).Should().Equal(
			"Game Mode",
			"Constrain the Forge to game rules",
			"# Base item",
			"- Weapon, shield, armor or jewellery only",
			"- No Epic or Legendary item",
			"# Prefix & suffix",
			"- Weapon, shield, armor or jewellery only",
			"- Taken from the same base item only",
			"- Level requirement not above the base item",
			"- Taken from the same gear type only",
			"- Not rarer than the base item",
			"- No Epic or Legendary item",
			"- The material item must have that prefix or suffix",
			"# Relics",
			"- Relics and charms only",
			"- Must be allowed on the base item gear",
			"""- Second relic only with the "of the Tinkerer" suffix""");

	[Fact]
	public void Build_StrictMode_DropsTheSameBaseItemAndLevelChecks()
		=> CardInEnglish(ForgeMode.Strict).Should().Equal(
			"Strict Mode",
			"Constrain the Forge to game rules with a twist",
			"# Base item",
			"- Weapon, shield, armor or jewellery only",
			"- No Epic or Legendary item",
			"# Prefix & suffix",
			"- Weapon, shield, armor or jewellery only",
			"- Taken from the same gear type only",
			"- Not rarer than the base item",
			"- No Epic or Legendary item",
			"- The material item must have that prefix or suffix",
			"# Relics",
			"- Relics and charms only",
			"- Must be allowed on the base item gear",
			"""- Second relic only with the "of the Tinkerer" suffix""",
			"= Switching to a stricter mode empties the slots");

	[Fact]
	public void Build_RelaxMode_AllowsEpicItemsAnyGearTypeSocketedRelicsAndASecondRelic()
		=> CardInEnglish(ForgeMode.Relax).Should().Equal(
			"Relax Mode",
			"""Allow the use of a second relic without "of the Tinkerer" suffix and an Epic/Legendary as a base item""",
			"# Base item",
			"- Weapon, shield, armor or jewellery only",
			"+ Epic and Legendary items allowed",
			"# Prefix & suffix",
			"- Weapon, shield, armor or jewellery only",
			"+ Taken from any gear type",
			"+ Epic and Legendary items allowed",
			"- The material item must have that prefix or suffix",
			"# Relics",
			"+ Relics, charms, or a weapon, shield, armor or jewellery with a socketed relic",
			"- Must be allowed on the base item gear",
			"+ Second relic on any item",
			"= Switching to a stricter mode empties the slots");

	[Fact]
	public void Build_GodMode_AllowsAnything()
		=> CardInEnglish(ForgeMode.God).Should().Equal(
			"God Mode",
			"Allow the use of all types of properties in placeholders",
			"# Base item",
			"+ Any item, including relics, charms and artifacts",
			"# Prefix & suffix",
			"+ Use any part of a material item: base, prefix, suffix or relics",
			"# Relics",
			"+ Any item or part in any relic slot",
			"+ Second relic on any item",
			"= Switching to a stricter mode empties the slots");
}
