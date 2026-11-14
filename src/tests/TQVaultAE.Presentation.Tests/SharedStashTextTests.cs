using AwesomeAssertions;
using TQVaultAE.Application;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation;

namespace TQVaultAE.Tests.Presentation;

/// <summary>
/// <see cref="SharedStashText"/>: the shared stashes' neutral names given by the services, translated for the UI.
/// </summary>
public class SharedStashTextTests
{
	[Fact]
	public void Localize_SharedStashName_TranslatesTheNameAndKeepsTheRest()
	{
		SharedStashText.Localize($"{SharedStashNames.TransferStash} [shadowchampions]", StashType.TransferStash)
			.Should().Be($"{Resources.GlobalTransferStash} [shadowchampions]");
		SharedStashText.Localize($"{SharedStashNames.RelicVault} - Immortal Throne", StashType.RelicVaultStash)
			.Should().Be($"{Resources.GlobalRelicVaultStash} - Immortal Throne");
		SharedStashText.Localize(SharedStashNames.TransferStash, StashType.TransferStash).Should().Be(Resources.GlobalTransferStash);
	}

	[Theory]
	[InlineData(StashType.PlayerStash)]
	[InlineData(null)]
	public void Localize_CharacterOrVaultNamedLikeASharedStash_StaysAsItIs(StashType? type)
		=> SharedStashText.Localize(SharedStashNames.RelicVault, type).Should().Be(SharedStashNames.RelicVault);

	[Fact]
	public void Localize_NameOfTheOtherSharedStash_StaysAsItIs()
		=> SharedStashText.Localize(SharedStashNames.RelicVault, StashType.TransferStash).Should().Be(SharedStashNames.RelicVault);
}
