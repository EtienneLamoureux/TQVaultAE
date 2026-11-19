using System.Globalization;
using AwesomeAssertions;

namespace TQVaultAE.Tests.DbCli;

public sealed class DbrTextTests
{
	[Fact]
	public void Line_WritesTheGameToolsForm()
	{
		DbrText.Line("offensivePhysicalMin", [12.5f, 3f]).Should().Be("offensivePhysicalMin,12.500000;3.000000,");
		DbrText.Line("itemLevel", [25]).Should().Be("itemLevel,25,");
		DbrText.Line("hidePrefixName", [true, false]).Should().Be("hidePrefixName,1;0,");
		DbrText.Line("itemNameTag", ["tagSword"]).Should().Be("itemNameTag,tagSword,");
		DbrText.Line("empty", []).Should().Be("empty,,");
	}

	[Fact]
	public void Value_IgnoresTheCurrentCulture()
	{
		var previous = CultureInfo.CurrentCulture;
		try
		{
			CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

			DbrText.Value(0.25f).Should().Be("0.250000");
			DbrText.Value(1234567).Should().Be("1234567");
		}
		finally
		{
			CultureInfo.CurrentCulture = previous;
		}
	}
}
