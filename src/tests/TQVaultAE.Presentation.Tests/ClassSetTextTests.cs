using System.Globalization;
using AwesomeAssertions;
using TQVaultAE.Application.Character;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation;

namespace TQVaultAE.Tests.Presentation;

/// <summary><see cref="ClassSetText.StatusToolTip"/>: the tool tip of the class set mark.</summary>
public class ClassSetTextTests
{
	private static string English(Func<string> text)
	{
		var (ui, current) = (CultureInfo.CurrentUICulture, CultureInfo.CurrentCulture);
		try
		{
			CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
			return text();
		}
		finally
		{
			(CultureInfo.CurrentUICulture, CultureInfo.CurrentCulture) = (ui, current);
		}
	}

	[Fact]
	public void StatusToolTip_ChangedInTheGame_ListsOnlyTheMasteriesAndAttributesThatDifferWithTheirSign()
	{
		var status = new ClassSetStatus(true, new ClassSetDifference(
			[(Masteries.Storm, 6), (Masteries.Earth, 0), (Masteries.Defensive, -3)],
			new ClassSetAttributes(0, 0, 2, 0, -1)));

		var text = English(() => ClassSetText.StatusToolTip(status, m => m.ToString()));

		text.Split(Environment.NewLine).Should().Equal(
			"Changed in the game since this class set was saved",
			"Storm: +6 skill points",
			"Defensive: -3 skill points",
			"Strength: +2 attribute points",
			"Dexterity: -1 attribute points",
			"Save to keep it in the class set, pick the class set to go back to it.");
	}

	[Fact]
	public void StatusToolTip_PointsMovedInsideAMastery_SaysTheEditorChangedItWithoutCounts()
	{
		var status = new ClassSetStatus(false, new ClassSetDifference([(Masteries.Storm, 0)], default));

		var text = English(() => ClassSetText.StatusToolTip(status, m => m.ToString()));

		text.Split(Environment.NewLine).Should().Equal(
			"Changed in the editor since this class set was applied",
			"Save to keep it in the class set, pick the class set to go back to it.");
	}
}
