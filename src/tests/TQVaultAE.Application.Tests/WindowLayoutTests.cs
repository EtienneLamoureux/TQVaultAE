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
}
