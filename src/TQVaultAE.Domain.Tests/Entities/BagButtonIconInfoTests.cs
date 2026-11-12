using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Entities;

public class BagButtonIconInfoTests
{
	private static BagButtonIconInfo CreateInfo() => new()
	{
		DisplayMode = BagButtonDisplayMode.CustomIcon | BagButtonDisplayMode.Label,
		Label = "Rings",
		OnStr = """Items\ring_on.tex""",
		OffStr = """Items\ring_off.tex""",
		OverStr = """Items\ring_over.tex""",
	};

	[Fact]
	public void Clone_CopiesEveryField()
	{
		var source = CreateInfo();

		var clone = source.Clone();

		// Compares field by field, so a field added later and left out of the copy fails here.
		clone.Should().BeEquivalentTo(source, options => options.IncludingFields());
	}

	[Fact]
	public void Clone_IsIndependentOfTheSource()
	{
		var source = CreateInfo();

		var clone = source.Clone();
		clone.Label = "Potions";
		clone.DisplayMode = BagButtonDisplayMode.Number;

		clone.Should().NotBeSameAs(source);
		source.Label.Should().Be("Rings");
		source.DisplayMode.Should().Be(BagButtonDisplayMode.CustomIcon | BagButtonDisplayMode.Label);
	}

	[Theory]
	[InlineData(true, true, true, BagButtonDisplayMode.Default)]
	[InlineData(false, false, false, BagButtonDisplayMode.CustomIcon)]
	[InlineData(false, true, false, BagButtonDisplayMode.CustomIcon | BagButtonDisplayMode.Number)]
	[InlineData(false, false, true, BagButtonDisplayMode.CustomIcon | BagButtonDisplayMode.Label)]
	[InlineData(false, true, true, BagButtonDisplayMode.CustomIcon | BagButtonDisplayMode.Number | BagButtonDisplayMode.Label)]
	public void DisplayModeOf_DefaultIgnoresNumberAndLabel(bool useDefault, bool number, bool label, BagButtonDisplayMode expected)
		=> BagButtonIconInfo.DisplayModeOf(useDefault, number, label).Should().Be(expected);

	[Fact]
	public void Create_TrimsTheLabelAndEmptiesMissingPictures()
	{
		var info = BagButtonIconInfo.Create(BagButtonDisplayMode.CustomIcon | BagButtonDisplayMode.Label, "  Rings ", """Items\on.tex""", """Items\off.tex""", null);

		info.Label.Should().Be("Rings");
		info.OnStr.Should().Be("""Items\on.tex""");
		info.OffStr.Should().Be("""Items\off.tex""");
		info.OverStr.Should().BeEmpty();
	}

	[Theory]
	// Default needs nothing, even without pictures or with Label in the text box.
	[InlineData(BagButtonDisplayMode.Default, null, null, null, true)]
	[InlineData(BagButtonDisplayMode.CustomIcon, "on", "off", null, true)]
	[InlineData(BagButtonDisplayMode.CustomIcon, null, "off", null, false)]
	[InlineData(BagButtonDisplayMode.CustomIcon, "on", null, null, false)]
	[InlineData(BagButtonDisplayMode.CustomIcon | BagButtonDisplayMode.Label, "on", "off", "Rings", true)]
	[InlineData(BagButtonDisplayMode.CustomIcon | BagButtonDisplayMode.Label, "on", "off", "   ", false)]
	[InlineData(BagButtonDisplayMode.CustomIcon | BagButtonDisplayMode.Label, "on", "off", null, false)]
	[InlineData(BagButtonDisplayMode.CustomIcon | BagButtonDisplayMode.Number, "on", "off", null, true)]
	public void Validate_RequiresOnOffForCustomAndTextForLabel(BagButtonDisplayMode mode, string? on, string? off, string? label, bool expected)
	{
		var info = BagButtonIconInfo.Create(mode, label, Id(on), Id(off), null);

		info.Validate().Should().Be(expected);
	}

	[Fact]
	public void AreSame_NullAndDefaultModeShowTheSameButton()
	{
		var keptPictures = BagButtonIconInfo.Create(BagButtonDisplayMode.Default, "old", "a.tex", "b.tex", null);

		BagButtonIconInfo.AreSame(null, null).Should().BeTrue();
		BagButtonIconInfo.AreSame(null, keptPictures).Should().BeTrue();
		BagButtonIconInfo.AreSame(CreateInfo(), null).Should().BeFalse();
		BagButtonIconInfo.AreSame(null, CreateInfo()).Should().BeFalse();
	}

	[Fact]
	public void AreSame_ComparesEveryShownValue()
	{
		BagButtonIconInfo.AreSame(CreateInfo(), CreateInfo()).Should().BeTrue();
		// Record ids compare without case, as the game does.
		var otherCase = CreateInfo();
		otherCase.OnStr = otherCase.OnStr.ToUpperInvariant();
		BagButtonIconInfo.AreSame(CreateInfo(), otherCase).Should().BeTrue();

		foreach (var change in new Action<BagButtonIconInfo>[]
		{
			i => i.DisplayMode = BagButtonDisplayMode.CustomIcon,
			i => i.Label = "Potions",
			i => i.OnStr = "x.tex",
			i => i.OffStr = "x.tex",
			i => i.OverStr = string.Empty,
		})
		{
			var changed = CreateInfo();
			change(changed);
			BagButtonIconInfo.AreSame(CreateInfo(), changed).Should().BeFalse();
		}
	}

	private static RecordId? Id(string? raw) => raw is null ? null : RecordId.Create(raw);
}
