using System.Drawing;
using AwesomeAssertions;
using TQVaultAE.Application;

namespace TQVaultAE.Tests.Application;

public class ZoomStepsTests
{
	// The cells of each step are those the zoom always gave (32 x zoom x DPI, rounded): only the rest of the window
	// moves to the cell's ratio.
	[Theory]
	[InlineData(1.0F, new[] { 13, 16, 19, 22, 26, 29, 32, 35, 38, 42, 45, 48, 51, 54, 58, 61, 64 })]
	[InlineData(1.25F, new[] { 16, 20, 24, 28, 32, 36, 40, 44, 48, 52, 56, 60, 64, 68, 72, 76, 80 })]
	[InlineData(1.5F, new[] { 19, 24, 29, 34, 38, 43, 48, 53, 58, 62, 67, 72, 77, 82, 86, 91, 96 })]
	[InlineData(1.75F, new[] { 22, 28, 34, 39, 45, 50, 56, 62, 67, 73, 78, 84, 90, 95, 101, 106, 112 })]
	public void Cell_IsTheRoundedCellOfTheNominalZoom(float dpiScale, int[] cells)
	{
		for (int step = 0; step < ZoomSteps.Count; step++)
			ZoomSteps.Cell(step, dpiScale).Should().Be(cells[step], $"step {step} at {dpiScale}");
	}

	[Theory]
	[InlineData(1.0F)]
	[InlineData(1.25F)]
	[InlineData(1.5F)]
	[InlineData(1.75F)]
	public void Scale_GivesBackTheCell(float dpiScale)
	{
		for (int step = 0; step < ZoomSteps.Count; step++)
			Convert.ToInt32(ZoomSteps.DesignCell * ZoomSteps.Scale(step, dpiScale)).Should().Be(ZoomSteps.Cell(step, dpiScale));
	}

	[Fact]
	public void Nominal_GoesFrom04To2By01()
	{
		ZoomSteps.Nominal(0).Should().Be(0.4F);
		ZoomSteps.Nominal(ZoomSteps.Reset).Should().Be(1F);
		ZoomSteps.Nominal(ZoomSteps.Count - 1).Should().Be(2F);
		ZoomSteps.Nominal(3).Should().Be(0.7F);
	}

	// A zoom left by a drag of an older version, float noise, out of range values.
	[Theory]
	[InlineData(0.9494506F, 5)]
	[InlineData(1.0000001F, 6)]
	[InlineData(0.99999994F, 6)]
	[InlineData(0.8999999F, 5)]
	[InlineData(0.94F, 5)]
	[InlineData(0.96F, 6)]
	[InlineData(0.1F, 0)]
	[InlineData(3F, 16)]
	public void Nearest_SnapsToTheClosestStep(float zoom, int step)
		=> ZoomSteps.Nearest(zoom).Should().Be(step);

	// 120 dpi, Windows lets a window grow to 2572 x 1462: 1.2 (1365 px high) fits, 1.3 (1479 px) does not.
	[Fact]
	public void LargestFitting_IsLimitedByTheTighterSide()
		=> ZoomSteps.LargestFitting(new Size(1350, 910), 1.25F, new Size(2572, 1462)).Should().Be(8);

	// 96 dpi, a 1920 x 1300 limit: the width allows 1.4 (1890 px), not 1.5 (2025 px).
	[Fact]
	public void LargestFitting_CanBeLimitedByTheWidth()
		=> ZoomSteps.LargestFitting(new Size(1350, 910), 1.0F, new Size(1920, 1300)).Should().Be(10);

	[Fact]
	public void LargestFitting_IsTheFirstStepWhenNothingFits()
		=> ZoomSteps.LargestFitting(new Size(1350, 910), 1.0F, new Size(100, 100)).Should().Be(0);
}
