using System.Xml.Linq;
using AwesomeAssertions;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Config;

/// <summary>
/// The order of each sort button: its own when picked in its menu, else the default <see cref="SortSettings.Algorithm"/>.
/// </summary>
public sealed class SortSettingsTests : IDisposable
{
	private readonly string _folder;
	private readonly string _xmlPath;

	public SortSettingsTests()
	{
		this._folder = Path.Combine(Path.GetTempPath(), "TQVaultAE.Tests", "SortSettings", Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(this._folder);
		this._xmlPath = Path.Combine(this._folder, "UserConfig.xml");
	}

	public void Dispose()
	{
		if (Directory.Exists(this._folder))
			Directory.Delete(this._folder, true);
	}

	public static TheoryData<SortButtonKey> Keys => [.. Enum.GetValues<SortButtonKey>()];

	[Theory]
	[MemberData(nameof(Keys))]
	public void AlgorithmFor_NoOwnOrder_UsesTheDefault(SortButtonKey key)
	{
		var sort = new SortSettings { Algorithm = SackSortAlgorithm.Rarity };

		sort.AlgorithmFor(key).Should().Be(SackSortAlgorithm.Rarity);
		sort.HasOwnAlgorithm(key).Should().BeFalse();
	}

	[Fact]
	public void AlgorithmFor_DefaultChangedLater_FollowedByButtonsWithoutOwnOrder()
	{
		var sort = new SortSettings();
		sort.SetAlgorithm(SortButtonKey.Vault, SackSortAlgorithm.Level);

		sort.Algorithm = SackSortAlgorithm.Type;

		sort.AlgorithmFor(SortButtonKey.Vault).Should().Be(SackSortAlgorithm.Level);
		sort.AlgorithmFor(SortButtonKey.Player).Should().Be(SackSortAlgorithm.Type);
	}

	[Fact]
	public void SetAlgorithm_OneButton_OthersAndDefaultUnchanged()
	{
		var sort = new SortSettings();

		sort.SetAlgorithm(SortButtonKey.Vault, SackSortAlgorithm.Level).Should().BeTrue();
		sort.SetAlgorithm(SortButtonKey.Player, SackSortAlgorithm.Type).Should().BeTrue();

		sort.AlgorithmFor(SortButtonKey.Vault).Should().Be(SackSortAlgorithm.Level);
		sort.AlgorithmFor(SortButtonKey.Player).Should().Be(SackSortAlgorithm.Type);
		sort.AlgorithmFor(SortButtonKey.SecondaryVault).Should().Be(SackSortAlgorithm.Normal);
		sort.AlgorithmFor(SortButtonKey.PlayerBag).Should().Be(SackSortAlgorithm.Normal);
		sort.AlgorithmFor(SortButtonKey.Stash).Should().Be(SackSortAlgorithm.Normal);
		sort.Algorithm.Should().Be(SackSortAlgorithm.Normal);
	}

	[Fact]
	public void SetAlgorithm_SameOrderAgain_NoChange()
	{
		var sort = new SortSettings();
		sort.SetAlgorithm(SortButtonKey.Stash, SackSortAlgorithm.Rarity);

		sort.SetAlgorithm(SortButtonKey.Stash, SackSortAlgorithm.Rarity).Should().BeFalse();
		sort.SetAlgorithm(SortButtonKey.Stash, SackSortAlgorithm.Level).Should().BeTrue();
	}

	[Fact]
	public void SetAlgorithm_SameAsDefault_KeptAsOwnOrder()
	{
		// Picked in the menu, the order is the button's even when it equals the default.
		var sort = new SortSettings();

		sort.SetAlgorithm(SortButtonKey.PlayerBag, SackSortAlgorithm.Normal).Should().BeTrue();

		sort.HasOwnAlgorithm(SortButtonKey.PlayerBag).Should().BeTrue();
		sort.PlayerBagValue.Should().Be("Normal");
	}

	[Fact]
	public void SaveThenRead_OwnOrders_OneAttributePerButtonWithOne()
	{
		var sort = new SortSettings { Algorithm = SackSortAlgorithm.Rarity, MergeRelics = true };
		sort.SetAlgorithm(SortButtonKey.Vault, SackSortAlgorithm.Level);
		sort.SetAlgorithm(SortButtonKey.Player, SackSortAlgorithm.Type);
		new UserSettings { Sort = sort }.Save(this._xmlPath);

		var element = XDocument.Load(this._xmlPath).Root!.Elements("Sort").Should().ContainSingle().Subject;
		element.Attributes().Select(a => (a.Name.LocalName, a.Value)).Should().BeEquivalentTo(
		[
			("Algorithm", "Rarity"),
			("MergeRelics", "1"),
			("Vault", "Level"),
			("Player", "Type"),
		]);
		element.HasElements.Should().BeFalse();

		var read = UserSettings.Read(this._xmlPath).Sort;
		read.Algorithm.Should().Be(SackSortAlgorithm.Rarity);
		read.MergeRelics.Should().BeTrue();
		read.AlgorithmFor(SortButtonKey.Vault).Should().Be(SackSortAlgorithm.Level);
		read.AlgorithmFor(SortButtonKey.Player).Should().Be(SackSortAlgorithm.Type);
		read.HasOwnAlgorithm(SortButtonKey.SecondaryVault).Should().BeFalse();
		read.HasOwnAlgorithm(SortButtonKey.PlayerBag).Should().BeFalse();
		read.AlgorithmFor(SortButtonKey.Stash).Should().Be(SackSortAlgorithm.Rarity);
	}

	[Fact]
	public void SaveThenRead_EveryButton_RoundTrip()
	{
		var orders = new Dictionary<SortButtonKey, SackSortAlgorithm>
		{
			[SortButtonKey.Vault] = SackSortAlgorithm.Level,
			[SortButtonKey.SecondaryVault] = SackSortAlgorithm.Type,
			[SortButtonKey.Player] = SackSortAlgorithm.Rarity,
			[SortButtonKey.PlayerBag] = SackSortAlgorithm.Normal,
			[SortButtonKey.Stash] = SackSortAlgorithm.Level,
		};
		var sort = new SortSettings { Algorithm = SackSortAlgorithm.Type };
		foreach (var (key, algorithm) in orders)
			sort.SetAlgorithm(key, algorithm);
		new UserSettings { Sort = sort }.Save(this._xmlPath);

		var read = UserSettings.Read(this._xmlPath).Sort;

		foreach (var (key, algorithm) in orders)
		{
			read.HasOwnAlgorithm(key).Should().BeTrue();
			read.AlgorithmFor(key).Should().Be(algorithm);
		}
	}

	[Fact]
	public void Read_FileWithoutButtonAttributes_EveryButtonUsesAlgorithm()
	{
		// A file written before the buttons had their own order.
		var sort = UserSettings.ParseSettings("""<UserSettings><Sort Algorithm="Level" MergeRelics="0" /></UserSettings>""").Sort;

		foreach (var key in Enum.GetValues<SortButtonKey>())
		{
			sort.HasOwnAlgorithm(key).Should().BeFalse();
			sort.AlgorithmFor(key).Should().Be(SackSortAlgorithm.Level);
		}
	}

	[Theory]
	[InlineData("Vault='Weight'")]
	[InlineData("Vault='2'")]
	[InlineData("Vault='Level, Type'")]
	[InlineData("Vault=''")]
	[InlineData("Vault='  '")]
	public void Read_InvalidButtonValue_ButtonUsesAlgorithm(string attribute)
	{
		var sort = UserSettings.ParseSettings($"""<UserSettings><Sort Algorithm='Rarity' {attribute} Player='type' /></UserSettings>""").Sort;

		sort.HasOwnAlgorithm(SortButtonKey.Vault).Should().BeFalse();
		sort.AlgorithmFor(SortButtonKey.Vault).Should().Be(SackSortAlgorithm.Rarity);
		sort.AlgorithmFor(SortButtonKey.Player).Should().Be(SackSortAlgorithm.Type, "a valid button keeps its order, any case");
	}

	[Fact]
	public void Read_UnknownButtonAttribute_Ignored()
	{
		var sort = UserSettings.ParseSettings("""<UserSettings><Sort Algorithm='Type' Trash='Level' Stash=' rarity ' /></UserSettings>""").Sort;

		sort.AlgorithmFor(SortButtonKey.Stash).Should().Be(SackSortAlgorithm.Rarity);
		sort.AlgorithmFor(SortButtonKey.Vault).Should().Be(SackSortAlgorithm.Type);
	}

	[Fact]
	public void Save_InvalidButtonValueRead_NotWrittenBack()
	{
		var settings = UserSettings.ParseSettings("""<UserSettings><Sort Algorithm='Normal' Vault='Weight' /></UserSettings>""");

		settings.Save(this._xmlPath);

		var element = XDocument.Load(this._xmlPath).Root!.Element("Sort")!;
		element.Attribute("Vault").Should().BeNull();
	}
}
