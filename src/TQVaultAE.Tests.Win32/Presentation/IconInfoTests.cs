using AwesomeAssertions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation.Win32.Models;

namespace TQVaultAE.Tests.Win32.Presentation;

public class IconInfoTests
{
	private static IconInfo Icon(string on, string off, string over, IconCategory category = IconCategory.Misc, bool hasOn = true, bool hasOff = true, bool hasOver = true)
		=> new(category, on, off, over) { HasOn = hasOn, HasOff = hasOff, HasOver = hasOver };

	[Fact]
	public void SimpleModePictures_TakesTheThreePictures()
	{
		var icon = Icon("ring_on.tex", "ring_off.tex", "ring_over.tex");

		var (on, off, over) = icon.SimpleModePictures;

		on.Should().Be((RecordId)"ring_on.tex");
		off.Should().Be((RecordId)"ring_off.tex");
		over.Should().Be((RecordId)"ring_over.tex");
	}

	[Theory]
	[InlineData("ring_on.tex")]
	[InlineData("ring_off.tex")]
	public void SimpleModePictures_DropsOverWhenItRepeatsOnOrOff(string overPicture)
	{
		var icon = Icon("ring_on.tex", "ring_off.tex", overPicture);

		icon.SimpleModePictures.Over.Should().BeNull();
	}

	[Fact]
	public void SimpleModePictures_LeavesOutPicturesMissingFromTheArchives()
	{
		var icon = Icon("ring_on.tex", "ring_off.tex", "ring_over.tex", hasOn: false, hasOver: false);

		var (on, off, over) = icon.SimpleModePictures;

		on.Should().BeNull();
		off.Should().Be((RecordId)"ring_off.tex");
		over.Should().BeNull();
	}

	[Fact]
	public void DistinctPictures_KeepsEachPictureOnceWithTheFirstIconThatHasIt()
	{
		var ring = Icon("ring_on.tex", "ring_off.tex", "ring_on.tex", IconCategory.Jewellery);
		// Shares its Off picture with the ring, in another case; its Over is missing from the archives.
		var amulet = Icon("amulet_on.tex", "RING_OFF.tex", "amulet_over.tex", IconCategory.Misc, hasOver: false);
		var empty = Icon(string.Empty, string.Empty, string.Empty);

		var pictures = IconInfo.DistinctPictures([ring, amulet, empty])
			.Select(p => (p.Icon, p.Id.Raw))
			.ToList();

		pictures.Should().Equal(
			(ring, "ring_off.tex"),
			(ring, "ring_on.tex"),
			(amulet, "amulet_on.tex")
		);
	}
}
