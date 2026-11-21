using System.Drawing;
using AwesomeAssertions;
using TQVaultAE.Application.Character;

namespace TQVaultAE.Tests.Application;

public class GameValuesTests
{
	private static readonly Dictionary<string, IReadOnlyList<double>> Values = new(StringComparer.OrdinalIgnoreCase)
	{
		["inventoryXSize"] = [640, 1],
		["inventoryYSize"] = [330],
		["negative"] = [-64],
		["empty"] = [],
	};

	[Fact]
	public void First_GivesTheFirstValue_NullWhenMissingOrEmpty()
	{
		Values.First("INVENTORYXSIZE").Should().Be(640);
		Values.First("empty").Should().BeNull();
		Values.First("missing").Should().BeNull();
	}

	[Fact]
	public void PixelGrid_GivesWholeCells_NullWhenASideIsMissing()
	{
		Values.PixelGrid("inventoryXSize", "inventoryYSize").Should().Be(new Size(20, 10));
		Values.PixelGrid("negative", "inventoryYSize").Should().Be(new Size(0, 10));
		Values.PixelGrid("inventoryXSize", "empty").Should().BeNull();
	}
}
