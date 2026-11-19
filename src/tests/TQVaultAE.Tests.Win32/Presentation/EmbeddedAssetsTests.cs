using System.Text.Json;
using AwesomeAssertions;
using TQVaultAE.Presentation.Win32;

namespace TQVaultAE.Tests.Win32.Presentation;

/// <summary>
/// Tests that the files built into TQVaultAE.Presentation.Win32 (fonts, icon list) are found under their names.
/// </summary>
public class EmbeddedAssetsTests
{
	[Fact]
	public void Fonts_AreTrueTypeFiles()
	{
		byte[] trueType = [0x00, 0x01, 0x00, 0x00];

		EmbeddedAssets.AlbertusMT.Take(4).Should().Equal(trueType);
		EmbeddedAssets.AlbertusMTLight.Take(4).Should().Equal(trueType);
	}

	[Fact]
	public void IconList_IsJson()
	{
		using var document = JsonDocument.Parse(EmbeddedAssets.IconServiceList);

		document.RootElement.ValueKind.Should().Be(JsonValueKind.Object);
	}
}
