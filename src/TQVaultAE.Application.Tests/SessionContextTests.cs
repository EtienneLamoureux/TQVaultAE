using AwesomeAssertions;
using TQVaultAE.Application;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Application;

public class SessionContextTests
{
	[Fact]
	public void IconInfoCopied_InitiallyFalse()
	{
		// Arrange & Act
		var context = new SessionContext();

		// Assert
		context.IconInfoCopied.Should().BeFalse();
	}

	[Fact]
	public void IconInfoCopied_AfterSettingIconInfoCopy_True()
	{
		// Arrange
		var context = new SessionContext();
		var iconInfo = new BagButtonIconInfo
		{
			DisplayMode = BagButtonDisplayMode.CustomIcon,
			Label = "Test"
		};

		// Act
		context.IconInfoCopy = iconInfo;

		// Assert
		context.IconInfoCopied.Should().BeTrue();
	}

	[Fact]
	public void IconInfoCopy_CanBeSetToNull()
	{
		// Arrange
		var context = new SessionContext();
		context.IconInfoCopy = new BagButtonIconInfo { Label = "Test" };

		// Act
		context.IconInfoCopy = null!;

		// Assert
		context.IconInfoCopy.Should().BeNull();
		context.IconInfoCopied.Should().BeTrue(); // Still true since flag was set
	}
}
