using AwesomeAssertions;
using TQVaultAE.Application.Results;
using TQVaultAE.Presentation;

namespace TQVaultAE.Tests.Presentation;

public class ImportTextTests
{
	[Fact]
	public void Message_EveryError_HasAText()
	{
		foreach (var error in Enum.GetValues<ImportError>().Where(e => e != ImportError.None))
			ImportText.Message(ImportResult.Failed(error)).Should().NotBeNullOrWhiteSpace(error.ToString());
	}

	[Theory]
	[InlineData(ImportError.UnsupportedScope)]
	[InlineData(ImportError.InvalidJson)]
	[InlineData(ImportError.PasteBinFailed)]
	public void Message_NamesTheDetail(ImportError error)
		=> ImportText.Message(ImportResult.Failed(error, "detail-x")).Should().Contain("detail-x");
}
