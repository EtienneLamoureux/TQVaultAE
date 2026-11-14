using System.Security.Cryptography;
using AwesomeAssertions;
using TQVaultAE.Application;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Data;

/// <summary>
/// The exact bytes the save encoders write for edited saves, pinned by their SHA-256 when the encoders were factored
/// (one item record writer, key/value helpers, one splice): any byte they change fails here. Unchanged saves are
/// compared with the game's own files in <see cref="SaveRoundTripTests"/>; these cover what has no game file to
/// compare with. The seeds TQVault gives the copies of a potion stack are random and cleared before hashing.
/// </summary>
public sealed class SaveEncodingBaselineTests : IDisposable
{
	private readonly SaveFixtures _fixtures = new();

	public void Dispose() => this._fixtures.Dispose();

	private static string Hash(byte[] data) => Convert.ToHexString(SHA256.HashData(data));

	private static Item Find(SackCollection sack, string baseItemFileName)
		=> sack.First(i => RecordId.RawOf(i.BaseItemId).EndsWith(baseItemFileName, StringComparison.OrdinalIgnoreCase));

	[Fact]
	public void LegacyVault_EncodedFromTheJsonVault_KeepsItsBytes()
	{
		var vault = this._fixtures.LoadVault(SaveFixtures.VaultJson);

		var legacy = this._fixtures.PlayerProvider.Encode(vault);

		Hash(SaveFixtures.WithoutSeedsOfStackCopies(legacy)).Should().Be("0EAD146887DEC5EE7EAA5859056318312E7F2CEAC533C9DA7EF303A02B091D3D");
	}

	[Fact]
	public void EditedCharacter_Encoded_KeepsItsBytes()
	{
		var pc = this._fixtures.LoadPlayer();
		var sack = pc.Sacks![1];
		var stack = sack.First(i => i.DoesStack);
		sack.RemoveItem(stack);
		stack.StackSize = 5;
		stack.PositionX = 3;
		stack.PositionY = 3;
		pc.Sacks[2].AddItem(stack);
		var other = sack.First(i => !i.DoesStack);
		sack.RemoveItem(other);
		var equipped = pc.EquipmentSack!.GetItem(6);
		equipped.relic2ID = equipped.relicID;
		equipped.RelicBonus2Id = equipped.RelicBonusId;
		equipped.Var2 = 12345;
		equipped.atlantis = true;
		EquipmentRules.TakeOut(pc.EquipmentSack, pc.EquipmentSack.GetItem(0));
		pc.EquipmentSack.GetItem(0).Seed = 1; // the empty item left in the slot gets a random seed

		var chr = this._fixtures.PlayerProvider.Encode(pc);

		Hash(SaveFixtures.WithoutSeedsOfStackCopies(chr)).Should().Be("0E445DF994CEB79900B11744686154D53F7A0411BC48CCEDBF7D541DA74CE210");
	}

	[Fact]
	public void MasteryReset_Committed_KeepsItsBytes()
	{
		var pc = this._fixtures.LoadPlayer();
		var info = pc.PlayerInfo!;
		var changed = new PlayerInfo(new TQVaultAE.Services.PathIO())
		{
			CurrentLevel = info.CurrentLevel,
			MasteriesAllowed = info.MasteriesAllowed,
			MasteriesResetRequiered = true,
			CurrentXP = info.CurrentXP,
			Money = info.Money,
			Class = string.Empty,
		};
		changed.SkillRecordList.AddRange(info.SkillRecordList.Take(5));
		info.MasteriesResetRequiered = true;

		this._fixtures.PlayerProvider.CommitPlayerInfo(pc, changed);
		var chr = this._fixtures.PlayerProvider.Encode(pc);

		Hash(SaveFixtures.WithoutSeedsOfStackCopies(chr)).Should().Be("6CE3721B1E98E4486204282A6847685DA879B9E200F5E686FA168740031A5D5D");
	}

	[Fact]
	public void RenamedCharacter_KeepsItsBytes()
	{
		var pc = this._fixtures.LoadPlayer();
		var data = pc.rawData!.ToArray();

		this._fixtures.TQData.ReplaceUnicodeValueAfter(ref data, "myPlayerName", "HeroB").Should().BeTrue();

		Hash(data).Should().Be("641652B79D6C5E3C7FBF61A385922310076B19221A2FC98B6782FDED2223D057");
	}

	[Fact]
	public void EditedStash_Saved_KeepsTheBytesOfBothFiles()
	{
		var stash = this._fixtures.LoadStash();
		var sack = stash.Sack!;
		sack.RemoveItem(Find(sack, "Parchment16.dbr"));
		var potions = Find(sack, "PotionMana_05.dbr");
		potions.StackSize = 10;
		potions.PositionX = 8;
		potions.PositionY = 2;
		var path = this._fixtures.TempFile("winsys.dxb");

		this._fixtures.StashProvider.Save(stash, path);

		Hash(File.ReadAllBytes(path)).Should().Be("7197310E53DF8C1D54AFE98B4FE33FC09BAFCEB44B6EF990DAC569A8A19BDADF");
		Hash(File.ReadAllBytes(Path.ChangeExtension(path, ".dxg"))).Should().Be("67010AA653FF934F587923B886A7B1DA457577D6C754DDCF63AA4104B4D1E1F9");
	}
}
