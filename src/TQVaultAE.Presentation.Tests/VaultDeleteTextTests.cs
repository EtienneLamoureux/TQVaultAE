using System.Globalization;
using AwesomeAssertions;
using TQVaultAE.Application.Results;
using TQVaultAE.Presentation;

namespace TQVaultAE.Tests.Presentation;

public class VaultDeleteTextTests
{
	private static string BuildInEnglish(string name, VaultDeleteInfo info)
	{
		var culture = CultureInfo.CurrentUICulture;
		try
		{
			CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");
			return VaultDeleteText.Build(name, info);
		}
		finally
		{
			CultureInfo.CurrentUICulture = culture;
		}
	}

	private static string Lines(params string[] lines) => string.Join(Environment.NewLine, lines);

	[Fact]
	public void Build_WithItems_NamesTheVaultCountsAndSaysItCanBeRestored()
	{
		var text = BuildInEnglish("Armor Set", new VaultDeleteInfo(143, 12, ToRecycleBin: true));

		text.Should().Be(Lines(
			"Delete the vault \"Armor Set\"?",
			"",
			"It contains 143 items in 12 bags.",
			"The file will be moved to the Recycle Bin, where you can restore it."));
	}

	[Fact]
	public void Build_WithOneItem_UsesTheSingular()
	{
		var text = BuildInEnglish("Rings", new VaultDeleteInfo(1, 1, ToRecycleBin: true));

		text.Should().Contain("It contains 1 item in 1 bag.");
	}

	[Fact]
	public void Build_EmptyVault_SaysItIsEmpty()
	{
		var text = BuildInEnglish("Spare", new VaultDeleteInfo(0, 0, ToRecycleBin: true));

		text.Should().Contain("It is empty.").And.NotContain("contains");
	}

	[Fact]
	public void Build_WithoutRecycleBin_SaysItCannotBeUndone()
	{
		var text = BuildInEnglish("Gear", new VaultDeleteInfo(5, 2, ToRecycleBin: false));

		text.Should().EndWith("This cannot be undone.").And.NotContain("Recycle Bin");
	}
}
