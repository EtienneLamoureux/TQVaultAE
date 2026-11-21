using AwesomeAssertions;
using TQVaultAE.Domain.Forge;
using TQVaultAE.Presentation;

namespace TQVaultAE.Tests.Presentation;

/// <summary>
/// <see cref="ForgeMessageText"/>: every forge refusal and God mode part has its own text.
/// </summary>
public class ForgeMessageTextTests
{
	private const string Tinkerer = "of the Tinkerer";

	[Fact]
	public void Of_EveryMessage_HasItsOwnText()
	{
		var texts = Enum.GetValues<ForgeMessage>().Select(m => ForgeMessageText.Of(m, Tinkerer)).ToList();

		// A message missing from the switch would show its enum name.
		texts.Should().NotContain(Enum.GetNames<ForgeMessage>());
		texts.Should().OnlyHaveUniqueItems();
	}

	[Fact]
	public void Of_TinkeredSuffix_NamesTheSuffix()
		=> ForgeMessageText.Of(ForgeMessage.MustHaveTinkeredSuffix, Tinkerer).Should().Contain(Tinkerer);

	[Fact]
	public void Of_PartOfAnotherMod_IsTheModText()
		=> ForgeMessageText.Of(ForgeMessage.PartOfAnotherMod, Tinkerer).Should().Be(Resources.ForgePartOfAnotherMod);

	[Theory]
	[InlineData(ForgePart.Prefix)]
	[InlineData(ForgePart.Suffix)]
	[InlineData(ForgePart.Relic1)]
	[InlineData(ForgePart.Relic2)]
	public void PartOf_EachMaterialPart_DiffersFromTheBase(ForgePart part)
		=> ForgeMessageText.PartOf(part).Should().NotBe(ForgeMessageText.PartOf(ForgePart.Base));

	[Fact]
	public void PartOf_Base_IsUseBase()
		=> ForgeMessageText.PartOf(ForgePart.Base).Should().Be(Resources.ForgeUseBase);
}
