using AwesomeAssertions;
using TQVaultAE.Application;
using TQVaultAE.Config;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

public class StartupSelectionTests
{
	private static readonly string[] Characters = ["Hero", "Villain"];
	private static readonly CommandLineArgs NoArgs = new(["TQVaultAE.exe"]);

	private readonly UserSettings _settings = new()
	{
		LoadLastCharacter = true,
		LastCharacterName = "Villain",
		LoadLastVault = true,
		LastVaultName = "Rings",
		SkipTitle = false,
	};

	[Fact]
	public void Resolve_LoadsTheLastCharacterAndVault()
	{
		var selection = StartupSelection.Resolve(this._settings, Characters, _ => true, NoArgs);

		selection.CharacterName.Should().Be("Villain");
		selection.VaultName.Should().Be("Rings");
		selection.SkipTitle.Should().BeFalse();
	}

	[Fact]
	public void Resolve_LastCharacterGone_KeepsTheSelection()
	{
		this._settings.LastCharacterName = "villain";

		var selection = StartupSelection.Resolve(this._settings, Characters, _ => true, NoArgs);

		selection.CharacterName.Should().BeNull();
	}

	[Fact]
	public void Resolve_AutoLoadOff_SelectsNoCharacterAndTheMainVault()
	{
		this._settings.LoadLastCharacter = false;
		this._settings.LoadLastVault = false;

		var selection = StartupSelection.Resolve(this._settings, Characters, _ => true, NoArgs);

		selection.CharacterName.Should().BeNull();
		selection.VaultName.Should().Be(VaultService.MAINVAULT);
	}

	[Fact]
	public void Resolve_LastVaultFileMissing_FallsBackToTheMainVault()
	{
		var selection = StartupSelection.Resolve(this._settings, Characters, name => name != "Rings", NoArgs);

		selection.VaultName.Should().Be(VaultService.MAINVAULT);
	}

	[Fact]
	public void Resolve_NoLastVault_FallsBackToTheMainVaultWithoutCheckingFiles()
	{
		this._settings.LastVaultName = string.Empty;
		var checkedFiles = new List<string>();

		var selection = StartupSelection.Resolve(this._settings, Characters, name => { checkedFiles.Add(name); return true; }, NoArgs);

		selection.VaultName.Should().Be(VaultService.MAINVAULT);
		checkedFiles.Should().BeEmpty();
	}

	[Fact]
	public void Resolve_CommandLinePlayer_SkipsTheTitleAndSelectsThePlayer()
	{
		var selection = StartupSelection.Resolve(this._settings, Characters, _ => true, new CommandLineArgs(["TQVaultAE.exe", "Hero"]));

		selection.SkipTitle.Should().BeTrue();
		selection.AutomaticPlayer.Should().Be("Hero");
	}

	[Fact]
	public void Resolve_SkipTitleSetting_SkipsTheTitleWithAnEmptyPlayer()
	{
		this._settings.SkipTitle = true;

		var selection = StartupSelection.Resolve(this._settings, Characters, _ => true, NoArgs);

		selection.SkipTitle.Should().BeTrue();
		selection.AutomaticPlayer.Should().BeEmpty();
	}

	[Fact]
	public void ResolveVault_LoadLastVaultOff_KeepsTheCurrentVaultWithoutCheckingFiles()
	{
		this._settings.LoadLastVault = false;
		var checkedFiles = new List<string>();

		var vault = StartupSelection.ResolveVault(this._settings, "Potions", name => { checkedFiles.Add(name); return true; });

		vault.Should().Be("Potions");
		checkedFiles.Should().BeEmpty();
	}

	[Theory]
	[InlineData(true, "Rings")]
	[InlineData(false, VaultService.MAINVAULT)]
	public void ResolveVault_LoadLastVaultOn_ReplacesTheCurrentVaultByTheLastOneWhenItsFileExists(bool exists, string expected)
	{
		var vault = StartupSelection.ResolveVault(this._settings, "Potions", name => name == "Rings" && exists);

		vault.Should().Be(expected);
	}
}
