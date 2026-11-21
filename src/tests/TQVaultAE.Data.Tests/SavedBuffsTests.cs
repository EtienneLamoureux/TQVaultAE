using System.Text;
using AwesomeAssertions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Tests.Data;

/// <summary>
/// The buffs <c>Player.chr</c> keeps after <c>hasSkillServices</c> (the temple curses): read into
/// <c>PlayerInfo.SavedBuffs</c>, kept as they are when the file is written.
/// </summary>
public sealed class SavedBuffsTests : IDisposable
{
	private readonly SaveFixtures _fixtures = new();

	public void Dispose() => this._fixtures.Dispose();

	private static readonly RecordId[] FixtureCurses =
	[
		"""records\item\heraclesway\buffs\heraclesdebuff02.dbr""".ToRecordId(),
		"""records\item\heraclesway\buffs\heraclesdebuff07.dbr""".ToRecordId(),
	];

	/// <summary>The test character's bytes with one key of the buff list renamed (same length).</summary>
	private static byte[] Renamed(string key, string replacement)
	{
		var data = File.ReadAllBytes(SaveFixtures.PlayerChr);
		var start = data.AsSpan().IndexOf("hasSkillServices"u8);
		var index = start + data.AsSpan(start).IndexOf(Encoding.ASCII.GetBytes(key));
		index.Should().BeGreaterThan(start);
		Encoding.ASCII.GetBytes(replacement).CopyTo(data, index);
		return data;
	}

	[Fact]
	public void ReadPlayerInfo_TestCharacter_ReadsItsTwoCurses()
		=> this._fixtures.LoadPlayer().PlayerInfo!.SavedBuffs.Should().Equal(FixtureCurses);

	[Fact]
	public void ReadPlayerInfo_WithoutSkillServices_ReadsNoBuff()
	{
		var data = File.ReadAllBytes(SaveFixtures.PlayerChr);
		var index = data.AsSpan().IndexOf("hasSkillServices"u8) + "hasSkillServices".Length;
		BitConverter.GetBytes(0).CopyTo(data, index);

		this._fixtures.Reload(data).PlayerInfo!.SavedBuffs.Should().BeEmpty();
	}

	[Theory]
	[InlineData("version", "versioX")]
	[InlineData("size", "sizX")]
	[InlineData("buffSkillName", "buffSkillNamX")]
	public void ReadPlayerInfo_ListNotWhereExpected_ReadsNoBuff(string key, string replacement)
		=> this._fixtures.Reload(Renamed(key, replacement)).PlayerInfo!.SavedBuffs.Should().BeEmpty(
			"a key that is not right after the value before it is not the list");

	[Fact]
	public void Save_EditedSkills_KeepsTheCursesOfTheSave()
	{
		var pc = this._fixtures.LoadPlayer();
		pc.PlayerInfo!.SkillsChanged = true;
		var path = this._fixtures.TempFile("Player.chr");

		this._fixtures.PlayerProvider.Save(pc, path);

		this._fixtures.Reload(File.ReadAllBytes(path)).PlayerInfo!.SavedBuffs.Should().Equal(FixtureCurses);
	}
}
