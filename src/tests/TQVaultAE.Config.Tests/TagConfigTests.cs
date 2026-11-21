using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using TQVaultAE.Config.Tags;

namespace TQVaultAE.Tests.Config;

/// <summary>
/// The JSON of the tag file: its field names are the format of the users' files.
/// </summary>
public class TagConfigTests
{
	/// <summary>The options registered in Program.cs.</summary>
	private static readonly JsonSerializerOptions Options = new() { IncludeFields = true, PropertyNameCaseInsensitive = true, WriteIndented = true };

	private const string SavedFile = """
		{
		  "tags": [
		    { "name": "Farm", "color": { "R": 255, "G": 128, "B": 0 } }
		  ],
		  "mapping": [
		    { "player": "Hero", "isTQIT": true, "isMod": false, "tags": [ "Farm" ] }
		  ]
		}
		""";

	[Fact]
	public void Deserialize_SavedFile_ReadsEveryField()
	{
		var config = JsonSerializer.Deserialize<TagConfig>(SavedFile, Options)!;

		config.tags.Should().ContainSingle();
		config.tags[0].name.Should().Be("Farm");
		config.tags[0].color.Should().BeEquivalentTo(new TagInfoColor { R = 255, G = 128, B = 0 });
		config.mapping.Should().ContainSingle();
		config.mapping[0].player.Should().Be("Hero");
		config.mapping[0].isTQIT.Should().BeTrue();
		config.mapping[0].isMod.Should().BeFalse();
		config.mapping[0].tags.Should().Equal("Farm");
	}

	[Fact]
	public void Serialize_WritesTheFieldNamesOfTheFile()
	{
		var config = JsonSerializer.Deserialize<TagConfig>(SavedFile, Options)!;

		var written = JsonNode.Parse(JsonSerializer.Serialize(config, Options))!;

		JsonNode.DeepEquals(written, JsonNode.Parse(SavedFile)).Should().BeTrue();
	}

	[Fact]
	public void Deserialize_FileWithoutLists_GivesEmptyLists()
	{
		var config = JsonSerializer.Deserialize<TagConfig>("""{ "mapping": [ { "player": "Hero" } ] }""", Options)!;

		config.tags.Should().BeEmpty();
		config.mapping[0].tags.Should().BeEmpty();
		config.mapping[0].player.Should().Be("Hero");
	}
}
