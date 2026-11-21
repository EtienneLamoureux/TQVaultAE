using AwesomeAssertions;

namespace TQVaultAE.Tests.DbCli;

/// <summary>
/// <see cref="ArchiveLayout.ResourcePrefix"/>: the first segments of the resource ids of an archive's entries, from its
/// place in the game install.
/// </summary>
public sealed class ArchiveLayoutTests
{
	private static string Native(string path) => path.Replace('/', Path.DirectorySeparatorChar);

	[Theory]
	[InlineData("C:/Games/TQ/Resources/Items.arc", "Items")]
	[InlineData("C:/Games/TQ/Resources/XPack2/Items.arc", """XPack2\Items""")]
	[InlineData("C:/Games/TQ/Resources/XPack4/Item.arc", """XPack4\Item""")]
	[InlineData("C:/Games/TQ/Resources/xpack/Creatures.arc", """xpack\Creatures""")] // Immortal Throne's folder is lowercase
	[InlineData("C:/Games/TQ/Resources/Sounds.arc", "Sounds")] // its xpack/ folder is part of the entry names
	[InlineData("C:/Games/TQ/Audio/Sounds.arc", "Sounds")]
	[InlineData("Resources/XPack3/Quests.arc", """XPack3\Quests""")] // relative to the game folder
	[InlineData("resources/XPACK2/items.arc", """XPACK2\items""")] // folder names in any case
	public void Prefix_IsThePathRelativeToResourcesOrAudio(string archive, string expected)
		=> ArchiveLayout.ResourcePrefix(Native(archive)).Should().Be(expected);

	[Theory]
	[InlineData("C:/Games/TQ/Audio/Dialog_DE.arc", "Dialog")]
	[InlineData("C:/Games/TQ/Audio/PlayerSounds_RU.arc", "PlayerSounds")]
	[InlineData("C:/Games/TQ/Resources/XPack2/Sounds_FR.arc", """XPack2\Sounds""")]
	[InlineData("C:/Games/TQ/Resources/xpack/Dialog_PL.arc", """xpack\Dialog""")]
	public void LocalizedArchive_HasThePrefixOfTheArchiveItReplaces(string archive, string expected)
		=> ArchiveLayout.ResourcePrefix(Native(archive)).Should().Be(expected);

	[Theory]
	[InlineData("C:/Games/TQ/Text/Text_EN.arc", "Text_EN")]
	[InlineData("C:/Games/TQ/Toolset/Tutorials.arc", "Tutorials")]
	[InlineData("C:/Temp/XPack2/Items.arc", "Items")] // an expansion folder outside Resources
	[InlineData("C:/Temp/Dialog_DE.arc", "Dialog_DE")]
	[InlineData("Items.arc", "Items")]
	public void OtherArchive_IsItsFileNameWithoutExtension(string archive, string expected)
		=> ArchiveLayout.ResourcePrefix(Native(archive)).Should().Be(expected);
}
