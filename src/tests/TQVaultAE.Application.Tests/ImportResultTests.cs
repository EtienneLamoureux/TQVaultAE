using AwesomeAssertions;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Application;

/// <summary>
/// <see cref="ImportResult"/>: which bags an import gives an icon to.
/// </summary>
public class ImportResultTests
{
	private static readonly BagButtonIconInfo Icon = BagButtonIconInfo.Create(BagButtonDisplayMode.Label, "Rings", "on.tex", "off.tex", null);

	[Fact]
	public void SetsTabIcon_OnlyATabImportWithAnIcon()
	{
		ImportResult.SucceededTab([], 2, Icon).SetsTabIcon.Should().BeTrue();
		ImportResult.SucceededTab([], 2).SetsTabIcon.Should().BeFalse();
		new ImportResult { Scope = ExportScope.MultiSelect, TabIconInfo = Icon }.SetsTabIcon.Should().BeFalse();
	}

	[Fact]
	public void SetsBagIcon_OnlyTheBagsTheVaultExportGaveAnIcon()
	{
		var result = ImportResult.SucceededVault("Gear", new() { [0] = [], [1] = [] }, new() { [1] = Icon });

		result.SetsBagIcon(0).Should().BeFalse();
		result.SetsBagIcon(1).Should().BeTrue();
		ImportResult.SucceededVault("Gear", new() { [0] = [] }).SetsBagIcon(0).Should().BeFalse();
	}
}
