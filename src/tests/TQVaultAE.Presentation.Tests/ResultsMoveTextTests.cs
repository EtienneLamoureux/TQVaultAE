using System.Globalization;
using AwesomeAssertions;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation;

namespace TQVaultAE.Tests.Presentation;

public class ResultsMoveTextTests
{
	private static T In<T>(string culture, Func<T> text)
	{
		var (ui, current) = (CultureInfo.CurrentUICulture, CultureInfo.CurrentCulture);
		try
		{
			CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
			return text();
		}
		finally
		{
			(CultureInfo.CurrentUICulture, CultureInfo.CurrentCulture) = (ui, current);
		}
	}

	private static Item Named(string name) => new() { BaseItemId = RecordId.Create($"records/item/{name}.dbr") };

	private static string NameOf(Item item) => Path.GetFileNameWithoutExtension(item.BaseItemId.Raw);

	private static ResultsMoveOutcome Outcome(IReadOnlyList<Item> done, params ResultsMoveSkipped[] skipped)
		=> new(done, skipped, [], [], [], [], null);

	[Fact]
	public void Report_GroupsTheItemsThatStayedByReason_WithTheirNames()
	{
		var ring = Named("ring");
		var sword = Named("sword");
		var helm = Named("helm");
		var outcome = Outcome([Named("amulet")],
			new(ring, ResultsMoveSkip.NoRoom), new(helm, ResultsMoveSkip.ReadOnly), new(sword, ResultsMoveSkip.NoRoom));

		string report = In("en", () => ResultsMoveText.Report(outcome, "Transfer Stash", NameOf));

		report.Should().Be("1 item(s) moved to Transfer Stash. Not moved (3): no room: ring, sword; read-only character: helm.");
	}

	[Fact]
	public void Report_OfAnUndo_CountsTheItemsPutBack()
	{
		var outcome = Outcome([Named("ring"), Named("sword")]);

		In("en", () => ResultsMoveText.Report(outcome, null, NameOf)).Should().Be("2 item(s) put back.");
		In("fr", () => ResultsMoveText.Report(outcome, null, NameOf)).Should().Be("2 objet(s) remis en place.");
	}

	[Theory]
	[InlineData("en")]
	[InlineData("fr")]
	public void Reason_EveryReasonHasItsOwnText(string culture)
	{
		var texts = In(culture, () => Enum.GetValues<ResultsMoveSkip>().Select(ResultsMoveText.Reason).ToList());

		texts.Should().OnlyHaveUniqueItems().And.NotContain(string.Empty);
	}

	[Fact]
	public void VaultBag_ShowsTheBagNumberOfItsButton()
		=> In("en", () => ResultsMoveText.VaultBag("Main Vault", 2)).Should().Be("Main Vault, bag 3");
}
