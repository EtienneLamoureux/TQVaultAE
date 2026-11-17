using System.Drawing;
using AwesomeAssertions;
using TQVaultAE.Application.Layout;

namespace TQVaultAE.Tests.Application;

public class CharacterWindowLayoutTests
{
	private static readonly Size Slots = new(340, 504);
	private static readonly Size First = new(296, 430);
	private static readonly Size Stats = new(256, 430);

	[Fact]
	public void Arrange_SlotsThenFourTabsInOneRow()
	{
		var (slots, stats, content) = CharacterWindowLayout.Arrange(Slots, First, Stats, 1F);

		slots.Should().Be(new Rectangle(0, 0, 340, 504));
		stats.Should().HaveCount(CharacterWindowLayout.StatsPanels);
		stats[0].Should().Be(new Rectangle(340 + CharacterWindowLayout.Gap, (504 - 430) / 2, 296, 430));
		for (int i = 1; i < stats.Count; i++)
		{
			// Spaced by the panels' picture: tab I's gold bar goes on under tab II.
			stats[i].Left.Should().Be(stats[i - 1].Left + Stats.Width + CharacterWindowLayout.Gap);
			stats[i].Size.Should().Be(Stats);
		}

		content.Should().Be(new Size(stats[^1].Right, 504));
	}

	[Fact]
	public void Arrange_ScalesEveryPart()
	{
		var (slots, stats, content) = CharacterWindowLayout.Arrange(Slots, First, Stats, 1.5F);

		slots.Size.Should().Be(new Size(510, 756));
		stats[0].Size.Should().Be(new Size(444, 645));
		stats[1].Size.Should().Be(new Size(384, 645));
		content.Height.Should().Be(756);
		content.Width.Should().Be(510 + (4 * 9) + (4 * 384));
	}

	[Theory]
	[InlineData(1F)]
	[InlineData(1.5F)]
	public void LoadoutList_RightOfTheGoldBarUnderTabII(float scale)
	{
		var (_, stats, _) = CharacterWindowLayout.Arrange(Slots, First, Stats, scale);
		var goldBar = new Rectangle(0, 400, 296, 30);

		var box = CharacterWindowLayout.LoadoutList(stats[0], stats[1], goldBar, 22, scale);

		int gap = (int)Math.Round(CharacterWindowLayout.Gap * scale);
		int middle = stats[0].Y + (int)Math.Round(415 * scale);
		box.Left.Should().Be(stats[0].X + (int)Math.Round(296 * scale) + gap);
		box.Right.Should().Be(stats[1].Right);
		box.Height.Should().Be((int)Math.Round(22 * scale));
		(box.Top + (box.Height / 2)).Should().BeInRange(middle - 1, middle + 1, "centred on the gold bar");
	}

	[Fact]
	public void FitScale_KeepsTheScaleWhenTheContentFits()
		=> CharacterWindowLayout.FitScale(1.25F, Slots, First, Stats, new Size(3000, 1500)).Should().Be(1.25F);

	[Fact]
	public void FitScale_ShrinksToTheNarrowerLimit()
	{
		var natural = CharacterWindowLayout.Arrange(Slots, First, Stats, 1F).Content;

		var scale = CharacterWindowLayout.FitScale(1.5F, Slots, First, Stats, new Size(1800, 1000));

		scale.Should().BeApproximately(1800F / natural.Width, 0.0001F);
		CharacterWindowLayout.Arrange(Slots, First, Stats, scale).Content.Width.Should().BeLessThanOrEqualTo(1802);
	}
}
