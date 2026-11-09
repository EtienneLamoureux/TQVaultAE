using AwesomeAssertions;
using TQVaultAE.Application;

namespace TQVaultAE.Tests.Application;

public class CommandLineArgsTests
{
	private const string Exe = "TQVaultAE.exe";

	[Fact]
	public void NoArguments_IsNotAutomatic()
	{
		var args = new CommandLineArgs([Exe]);

		args.IsAutomatic.Should().BeFalse();
		args.Error.Should().BeNull();
	}

	[Fact]
	public void Player_IsAutomatic()
	{
		var args = new CommandLineArgs([Exe, "Hero"]);

		args.Player.Should().Be("Hero");
		args.IsAutomatic.Should().BeTrue();
	}

	[Fact]
	public void ModArgument_IsAnError_SinceTheModComesFromTheCharacterLink()
	{
		var args = new CommandLineArgs([Exe, " /Mod:Atlantis "]);

		args.Player.Should().BeNull();
		args.Error.Should().Contain("Unknown argument");
		var act = args.ThrowIfInvalid;
		act.Should().Throw<ArgumentException>().WithMessage("*Unknown argument*");
	}

	[Theory]
	[InlineData("Hero", "/mod:Atlantis")]
	[InlineData("Hero", "Villain")]
	public void TwoArguments_IsAnError(string first, string second)
	{
		var args = new CommandLineArgs([Exe, first, second]);

		args.Error.Should().Contain("Too many arguments");
	}

	[Fact]
	public void ValidArguments_DoNotThrow()
	{
		var act = new CommandLineArgs([Exe, "Hero"]).ThrowIfInvalid;

		act.Should().NotThrow();
	}
}
