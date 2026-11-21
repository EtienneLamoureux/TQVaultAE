using System.Drawing;
using AwesomeAssertions;
using TQVaultAE.GUI.Components.Panels;

namespace TQVaultAE.Tests.Win32.Gui;

/// <summary>
/// The room a stash bag takes in the stash panel (<see cref="SackViewport.Fit"/>): a bag that fits keeps its size, a larger
/// one, on either axis, scrolls in the whole room.
/// </summary>
public sealed class SackViewportTests
{
	private static readonly Size Room = new(500, 400);

	[Theory]
	[InlineData(480, 380)]
	[InlineData(500, 400)]
	public void Fit_SackThatFits_KeepsItsSize(int width, int height)
		=> SackViewport.Fit(new Size(width, height), Room).Should().Be(new Size(width, height));

	[Theory]
	[InlineData(700, 700)]
	[InlineData(700, 300)]
	[InlineData(300, 401)]
	public void Fit_SackLargerOnAnAxis_TakesTheWholeRoom(int width, int height)
		=> SackViewport.Fit(new Size(width, height), Room).Should().Be(Room);
}
