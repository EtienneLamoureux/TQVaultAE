using System.Drawing;
using System.Reflection;
using AwesomeAssertions;

namespace TQVaultAE.Tests.Win32.Gui;

/// <summary>
/// The item tool tip kept inside the working area of the panel's monitor (<c>BaseTooltip.TopWithin</c>): it was compared
/// with the primary monitor's height, which moved it off a monitor whose top is not 0.
/// </summary>
public sealed class TooltipPlacementTests
{
	private static int TopWithin(int top, int height, Rectangle workingArea)
		=> (int)typeof(TQVaultAE.GUI.Tooltips.BaseTooltip)
			.GetMethod("TopWithin", BindingFlags.NonPublic | BindingFlags.Static)!
			.Invoke(null, [top, height, workingArea])!;

	[Fact]
	public void TopWithin_Fits_KeepsTheTop()
		=> TopWithin(100, 300, new Rectangle(0, 0, 1920, 1040)).Should().Be(100);

	[Fact]
	public void TopWithin_MonitorBelowThePrimary_PushesUpToItsBottom()
		=> TopWithin(1900, 400, new Rectangle(0, 1080, 1920, 1040)).Should().Be(2120 - 400);

	[Fact]
	public void TopWithin_MonitorBelowThePrimary_FitsInsideIt_IsNotMoved()
		=> TopWithin(1200, 400, new Rectangle(0, 1080, 1920, 1040)).Should().Be(1200);

	[Fact]
	public void TopWithin_MonitorAboveThePrimary_TallerThanTheArea_StopsAtItsTop()
		=> TopWithin(-900, 2000, new Rectangle(0, -1080, 1920, 1040)).Should().Be(-1080);
}
