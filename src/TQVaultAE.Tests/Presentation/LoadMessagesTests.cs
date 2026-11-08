using AwesomeAssertions;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation;

namespace TQVaultAE.Tests.Presentation;

public class LoadMessagesTests
{
	private const string StashFile = """C:\saves\winsys.dxb""";

	[Fact]
	public void ForPlayer_ParseError_GivesOneErrorWithFileAndReason()
	{
		var player = new PlayerCollection("Hero", "Player.chr") { ArgumentException = new ArgumentException("bad block") };

		var messages = LoadMessages.ForPlayer(new PlayerLoadResult { PlayerFile = "Player.chr", Player = player });

		var message = messages.Should().ContainSingle().Subject;
		message.IsError.Should().BeTrue();
		message.Caption.Should().Be(Resources.GlobalError);
		message.Text.Should().Be($"{Resources.MainFormPlayerReadError}\nPlayer.chr\nbad block");
	}

	[Fact]
	public void ForPlayer_LoadedFine_GivesNoMessage()
	{
		var result = new PlayerLoadResult { PlayerFile = "Player.chr", Player = new PlayerCollection("Hero", "Player.chr") };

		LoadMessages.ForPlayer(result).Should().BeEmpty();
	}

	[Fact]
	public void ForStash_NotFound_GivesAnInformationWithTheDetail()
	{
		var stash = new Stash("Hero", StashFile) { StashFound = false };

		var messages = LoadMessages.ForStash(new StashLoadResult { Stash = stash, StashFile = StashFile }, "Character : Hero");

		var message = messages.Should().ContainSingle().Subject;
		message.IsError.Should().BeFalse();
		message.Caption.Should().Be(Resources.StashNotFound);
		message.Text.Should().Be($"{Resources.StashNotFoundMsg}\n\nCharacter : Hero");
	}

	[Fact]
	public void ForStash_NotFoundAndParseError_GivesBothInOrder()
	{
		var stash = new Stash("Hero", StashFile) { StashFound = false, ArgumentException = new ArgumentException("truncated") };

		var messages = LoadMessages.ForStash(new StashLoadResult { Stash = stash, StashFile = StashFile }, "Relic Stash");

		messages.Select(m => m.IsError).Should().Equal(false, true);
		messages[1].Text.Should().EndWith($"\n{StashFile}\ntruncated");
	}

	[Fact]
	public void ForStash_FoundOrUnknown_GivesNoMessage()
	{
		var found = new StashLoadResult { Stash = new Stash("Hero", StashFile) { StashFound = true } };
		var unknown = new StashLoadResult { Stash = new Stash("Hero", StashFile) };
		var none = new StashLoadResult();

		LoadMessages.ForStash(found, "x").Should().BeEmpty();
		LoadMessages.ForStash(unknown, "x").Should().BeEmpty();
		LoadMessages.ForStash(none, "x").Should().BeEmpty();
	}

	[Fact]
	public void ForPlayerStash_NotFound_NamesTheCharacter()
	{
		var pathIO = new Mock<IPathIO>();
		pathIO.Setup(x => x.GetFileName("""C:\saves\_Hero""")).Returns("_Hero");
		var save = new PlayerSave("""C:\saves\_Hero""", true, false, false, "", new Mock<ITranslationService>().Object, pathIO.Object);
		var result = new StashLoadResult { Stash = new Stash("Hero", StashFile) { StashFound = false }, StashFile = StashFile };

		var message = LoadMessages.ForPlayerStash(result, save).Should().ContainSingle().Subject;

		message.Text.Should().Be($"{Resources.StashNotFoundMsg}\n\nCharacter : {save}");
	}

	[Fact]
	public void ForSharedStashes_NotFound_NameTheStashAndItsFile()
	{
		var result = new StashLoadResult { Stash = new Stash("Shared", StashFile) { StashFound = false }, StashFile = StashFile };

		LoadMessages.ForTransferStash(result).Single().Text.Should().Be($"{Resources.StashNotFoundMsg}\n\nTransfer Stash\n\n{StashFile}");
		LoadMessages.ForRelicVaultStash(result).Single().Text.Should().Be($"{Resources.StashNotFoundMsg}\n\nRelic Stash\n\n{StashFile}");
	}

	[Fact]
	public void ReadError_FillsTheFileAndTheException()
	{
		var exception = new IOException("locked");

		var text = LoadMessages.ReadError(StashFile, exception);

		text.Should().Be(string.Format(Resources.MainFormReadError, StashFile, exception.ToString()));
		text.Should().NotContain("{0}");
	}
}
