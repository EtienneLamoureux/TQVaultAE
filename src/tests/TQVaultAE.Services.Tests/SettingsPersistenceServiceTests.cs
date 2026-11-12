using AwesomeAssertions;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Results;
using TQVaultAE.Config;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

public class SettingsPersistenceServiceTests
{
	private static UserSettings NewSettings()
		=> new()
		{
			LoadLastVault = true,
			LastVaultName = "Main Vault",
			LoadLastCharacter = true,
			LastCharacterName = "Hero",
			AutoDetectLanguage = false,
			AutoDetectGamePath = false,
			TQLanguage = "EN",
			TQPath = """C:\TQ""",
			TQITPath = """C:\TQIT""",
		};

	[Fact]
	public void SaveConfiguration_NothingNewAndNoChangeInTheSession_DoesNotSaveNorSetTheSessionFlag()
	{
		var session = new SessionContext();
		var service = new SettingsPersistenceService(NewSettings(), new Mock<IDatabase>().Object, session);

		service.SaveConfiguration("Main Vault", "Hero", 1.0F).Should().BeFalse();

		session.ConfigChanged.Should().BeFalse();
	}

	[Fact]
	public void ApplySessionState_NothingNew_ReportsNoChange()
	{
		var settings = NewSettings();

		var changed = SettingsPersistenceService.ApplySessionState(settings, "main vault", "HERO", 1.0F);

		changed.Should().BeFalse();
		settings.LastVaultName.Should().Be("Main Vault");
		settings.LastCharacterName.Should().Be("Hero");
	}

	[Fact]
	public void ApplySessionState_OtherVaultAndCharacter_AreRecorded()
	{
		var settings = NewSettings();

		var changed = SettingsPersistenceService.ApplySessionState(settings, "Rings", "Villain", 1.0F);

		changed.Should().BeTrue();
		settings.LastVaultName.Should().Be("Rings");
		settings.LastCharacterName.Should().Be("Villain");
	}

	[Fact]
	public void ApplySessionState_AutoLoadOff_DoesNotRecordVaultOrCharacter()
	{
		var settings = NewSettings();
		settings.LoadLastVault = false;
		settings.LoadLastCharacter = false;

		var changed = SettingsPersistenceService.ApplySessionState(settings, "Rings", "Villain", 1.0F);

		changed.Should().BeFalse();
		settings.LastVaultName.Should().Be("Main Vault");
		settings.LastCharacterName.Should().Be("Hero");
	}

	[Fact]
	public void ApplySessionState_NoCharacterSelected_ClearsTheLastCharacter()
	{
		var settings = NewSettings();

		var changed = SettingsPersistenceService.ApplySessionState(settings, null, string.Empty, 1.0F);

		changed.Should().BeTrue();
		settings.LastCharacterName.Should().BeEmpty();
		settings.LastVaultName.Should().Be("Main Vault");
	}

	[Fact]
	public void ApplySessionState_AutoDetect_ClearsLanguageAndPathsWithoutAskingASave()
	{
		var settings = NewSettings();
		settings.AutoDetectLanguage = true;
		settings.AutoDetectGamePath = true;

		var changed = SettingsPersistenceService.ApplySessionState(settings, null, null, 1.0F);

		changed.Should().BeFalse();
		settings.TQLanguage.Should().BeEmpty();
		settings.TQPath.Should().BeEmpty();
		settings.TQITPath.Should().BeEmpty();
	}

	[Fact]
	public void ApplySessionState_Zoomed_StoresTheScale()
	{
		var settings = NewSettings();

		var changed = SettingsPersistenceService.ApplySessionState(settings, null, null, 1.25F);

		changed.Should().BeTrue();
		settings.Scale.Should().Be(1.25F);
	}

	[Theory]
	[InlineData(false, false, false, RestartReason.None)]
	[InlineData(true, true, false, RestartReason.Settings)]
	[InlineData(false, false, true, RestartReason.Settings)]
	[InlineData(true, false, true, RestartReason.Settings)]
	[InlineData(false, true, false, RestartReason.GamePath)]
	[InlineData(true, false, false, RestartReason.Language)]
	public void RestartRule_Evaluate_PicksTheMessageOfTheChange(bool language, bool gamePath, bool uiSetting, RestartReason expected)
	{
		RestartRule.Evaluate(language, gamePath, uiSetting).Should().Be(expected);
	}
}
