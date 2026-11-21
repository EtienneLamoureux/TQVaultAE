using System.Drawing;
using AwesomeAssertions;
using TQVaultAE.Application.Character;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Application;

public class CaravanSizesTests
{
	private static readonly CaravanSizes BaseGame = new(new Size(10, 15), new Size(10, 15), new Size(16, 5));

	[Fact]
	public void Overrides_KeepsOnlyTheSizesTheModChanges()
	{
		var moreSpace = new CaravanSizes(new Size(20, 20), new Size(10, 15), new Size(20, 20));

		CaravanSizes.Overrides(moreSpace, BaseGame).Should().Be(new CaravanSizes(new Size(20, 20), null, new Size(20, 20)));
	}

	[Fact]
	public void Overrides_TheBaseGameItself_SetsNone()
		=> CaravanSizes.Overrides(BaseGame, BaseGame).Should().Be(CaravanSizes.None);

	[Theory]
	[InlineData(BagIdConstants.BAGID_TRANSFERSTASH, 3)]
	[InlineData(BagIdConstants.BAGID_PLAYERSTASH, 1)]
	[InlineData(BagIdConstants.BAGID_RELICVAULTSTASH, 2)]
	public void Of_GivesTheSizeOfEachStashBag(int bagId, int width)
		=> new CaravanSizes(new Size(1, 1), new Size(2, 2), new Size(3, 3)).Of(bagId)!.Value.Width.Should().Be(width);

	[Fact]
	public void Of_Equipment_HasNoSize()
		=> new CaravanSizes(new Size(1, 1), new Size(2, 2), new Size(3, 3)).Of(BagIdConstants.BAGID_EQUIPMENTPANEL).Should().BeNull();
}
