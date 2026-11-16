using AwesomeAssertions;
using TQVaultAE.Presentation.ItemText;

namespace TQVaultAE.Tests.Presentation;

public class GameNumberFormatTests
{
	[Theory]
	[InlineData("{0:+#0;-#0} Base Attack Speed", -0.35, "-0.35 Base Attack Speed")]
	[InlineData("{0:+#0;-#0} Base Attack Speed", 0.004, "+0.004 Base Attack Speed")]
	[InlineData("{0:+#0;-#0}%", -12.6, "-13%")]
	[InlineData("{0:+#0;-#0}%", 12.4, "+12%")]
	[InlineData("{0:+#0;-#0}%", 0.0, "+0%")]
	[InlineData("{0:+#0.0;-#0.0}%", -0.04, "-0.04%")]
	[InlineData("{0:+#0.0;-#0.0}%", 2.25, "+2.3%")]
	[InlineData("{0:#0} Energy Cost", 0.4, "0 Energy Cost")]
	public void KeepNonZero_OnlyAValueThatWouldReadZeroGetsDecimals(string format, double value, string expected)
		=> string.Format(System.Globalization.CultureInfo.InvariantCulture, GameNumberFormat.KeepNonZero(format, value), value).Should().Be(expected);

	[Fact]
	public void KeepNonZero_TextAndIntegerPlaceholdersUntouched()
	{
		GameNumberFormat.KeepNonZero("{0} to {1:#0}", "Undead", 3).Should().Be("{0} to {1:#0}");
		GameNumberFormat.KeepNonZero("{{^y}}{0:+#0;-#0}", 0.2F).Should().Be("{{^y}}{0:+#0.###;-#0.###}");
	}
}
