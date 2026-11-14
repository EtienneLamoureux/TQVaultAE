using System.Drawing;
using AwesomeAssertions;
using TQVaultAE.Application;

namespace TQVaultAE.Tests.Application;

public class WindowLayoutTests
{
	// The main window at 125 % DPI, zoom 1: 1688 x 1138 client, side pictures 4 px, title bar 17 px, bottom 4 px.
	[Fact]
	public void ContentArea_StaysInsideTheBorderAndClearOfTheGrips()
	{
		var area = WindowLayout.ContentArea(new Size(1688, 1138), side: 4, top: 17, bottom: 4, grip: 2);

		area.Should().Be(new Rectangle(6, 17, 1676, 1115));
		area.Right.Should().Be(1688 - 4 - 2);
		area.Bottom.Should().Be(1138 - 4 - 2);
	}

	[Fact]
	public void ContentArea_IsEmptyRatherThanNegativeInATinyWindow()
	{
		var area = WindowLayout.ContentArea(new Size(10, 10), side: 4, top: 17, bottom: 4, grip: 2);

		area.Width.Should().Be(0);
		area.Height.Should().Be(0);
	}

	// The panels' table placed at (19, 75) ends on the content area's edges, whatever size it had before (a maximize).
	[Fact]
	public void FillTo_ReachesTheRightAndBottomEdges()
	{
		var area = new Rectangle(6, 17, 1676, 1115);

		var size = WindowLayout.FillTo(area, new Point(19, 75));

		size.Should().Be(new Size(1663, 1057));
		(19 + size.Width).Should().Be(area.Right);
		(75 + size.Height).Should().Be(area.Bottom);
	}

	[Fact]
	public void FillTo_NeverGivesANegativeSize()
		=> WindowLayout.FillTo(new Rectangle(0, 0, 10, 10), new Point(20, 30)).Should().Be(Size.Empty);

	// The main window's design client size, 1350 x 910, at 125 % DPI and zoom 1, then at 96 dpi and zoom 0.8.
	[Theory]
	[InlineData(1.25F, 1688, 1138)]
	[InlineData(0.8F, 1080, 728)]
	public void Scaled_RoundsTheDesignSize(float scale, int width, int height)
		=> WindowLayout.Scaled(new Size(1350, 910), scale).Should().Be(new Size(width, height));

	// 96 dpi, zoom 0.8: the panels at (15, 48) need 686 px of height, the content area of a 728 px high window ends at
	// 722 (bottom picture 4 px, grip 2 px): the window grows by the 12 px missing, not in width.
	[Fact]
	public void GrowToFit_GrowsByWhatTheContentLacks()
	{
		var client = new Size(1080, 728);
		var area = WindowLayout.ContentArea(client, side: 4, top: 17, bottom: 4, grip: 2);

		var grown = WindowLayout.GrowToFit(client, area, new Point(15, 48), new Size(1040, 686));

		grown.Should().Be(new Size(1080, 740));
		(48 + 686).Should().Be(WindowLayout.ContentArea(grown, 4, 17, 4, 2).Bottom);
	}

	[Fact]
	public void GrowToFit_KeepsAClientTheContentFits()
	{
		var client = new Size(1688, 1138);
		var area = WindowLayout.ContentArea(client, side: 4, top: 17, bottom: 4, grip: 2);

		WindowLayout.GrowToFit(client, area, new Point(19, 75), new Size(1624, 1039)).Should().Be(client);
	}

	[Fact]
	public void Within_CutsOnlyTheSidesPastTheLimit()
	{
		WindowLayout.Within(new Size(2362, 1592), new Size(2572, 1462)).Should().Be(new Size(2362, 1462));
		WindowLayout.Within(new Size(1688, 1138), new Size(2572, 1462)).Should().Be(new Size(1688, 1138));
	}

	// A 1920 x 1032 working area of a monitor left of the primary one.
	[Fact]
	public void CenteredIn_CentersAWindowThatFits()
		=> WindowLayout.CenteredIn(new Rectangle(-1920, 358, 1920, 1032), new Size(1350, 910))
			.Should().Be(new Point(-1920 + 285, 358 + 61));

	[Fact]
	public void CenteredIn_KeepsTheCaptionOfALargerWindowOnScreen()
		=> WindowLayout.CenteredIn(new Rectangle(-1920, 358, 1920, 1032), new Size(1688, 1138))
			.Should().Be(new Point(-1920 + 116, 358));
}
