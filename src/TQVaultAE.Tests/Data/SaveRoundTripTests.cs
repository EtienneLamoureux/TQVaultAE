using System.Buffers.Binary;
using AwesomeAssertions;
using TQVaultAE.Application;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Data;

/// <summary>
/// Reads the save files of the test character, writes them back and reads them again:
/// <c>Player.chr</c>, the stash <c>winsys.dxb</c>/<c>.dxg</c>, and the vaults (<c>.vault.json</c> and the legacy <c>.vault</c>).
/// </summary>
public sealed class SaveRoundTripTests : IDisposable
{
	private readonly SaveFixtures _fixtures = new();

	public void Dispose() => this._fixtures.Dispose();

	/// <summary>What the save files keep of an item.</summary>
	private sealed record ItemState(
		string BaseItem, string Prefix, string Suffix,
		string Relic, string RelicBonus, int Var1,
		bool Atlantis, string Relic2, string RelicBonus2, int Var2,
		int X, int Y, int StackSize, int Seed);

	private static ItemState StateOf(Item item) => new(
		RecordId.RawOf(item.BaseItemId), RecordId.RawOf(item.prefixID), RecordId.RawOf(item.suffixID),
		RecordId.RawOf(item.relicID), RecordId.RawOf(item.RelicBonusId), item.Var1,
		item.atlantis, RecordId.RawOf(item.relic2ID), RecordId.RawOf(item.RelicBonus2Id), item.Var2,
		item.PositionX, item.PositionY, item.StackSize, item.Seed);

	private static List<ItemState> StatesOf(SackCollection sack) => sack.Select(StateOf).ToList();

	private static List<List<ItemState>> StatesOf(PlayerCollection pc) => pc.Sacks!.Select(StatesOf).ToList();

	private static Item Find(SackCollection sack, string baseItemFileName)
		=> sack.Single(i => RecordId.RawOf(i.BaseItemId).EndsWith(baseItemFileName, StringComparison.OrdinalIgnoreCase));

	#region Player.chr

	/// <summary>
	/// Clears the seed of each item record written as a copy of a potion stack (<c>pointX</c> = -1):
	/// the game saves a stack as one record per potion and TQVault gives the copies new seeds.
	/// </summary>
	private static byte[] WithoutSeedsOfStackCopies(byte[] chr)
	{
		ReadOnlySpan<byte> seedTag = [4, 0, 0, 0, (byte)'s', (byte)'e', (byte)'e', (byte)'d'];
		ReadOnlySpan<byte> pointXTag = [6, 0, 0, 0, (byte)'p', (byte)'o', (byte)'i', (byte)'n', (byte)'t', (byte)'X'];
		var data = (byte[])chr.Clone();
		int offset = 0;
		int found;
		while ((found = data.AsSpan(offset).IndexOf(seedTag)) >= 0)
		{
			int seed = offset + found + seedTag.Length;
			int pointX = data.AsSpan(seed, Math.Min(200, data.Length - seed)).IndexOf(pointXTag);
			if (pointX >= 0 && BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(seed + pointX + pointXTag.Length)) == -1)
				data.AsSpan(seed, 4).Clear();

			offset = seed + 4;
		}

		return data;
	}

	[Fact]
	public void Save_UnchangedCharacter_WritesTheGameFileBackExceptSeedsOfPotionStackCopies()
	{
		var original = File.ReadAllBytes(SaveFixtures.PlayerChr);
		var pc = this._fixtures.LoadPlayer();
		var path = this._fixtures.TempFile("Player.chr");

		this._fixtures.PlayerProvider.Save(pc, path);

		var written = File.ReadAllBytes(path);
		written.Length.Should().Be(original.Length);
		WithoutSeedsOfStackCopies(written).Should().Equal(WithoutSeedsOfStackCopies(original));
		written.Should().NotEqual(original, "the copies of a potion stack get new seeds, as the test relies on to find them");
	}

	[Fact]
	public void Encode_UnchangedCharacter_ReadsBackTheSameSacksEquipmentAndStacks()
	{
		var pc = this._fixtures.LoadPlayer();

		var reloaded = this._fixtures.Reload(this._fixtures.PlayerProvider.Encode(pc));

		StatesOf(reloaded).Should().BeEquivalentTo(StatesOf(pc), o => o.WithStrictOrdering());
		StatesOf(reloaded.EquipmentSack!).Should().BeEquivalentTo(StatesOf(pc.EquipmentSack!), o => o.WithStrictOrdering());
		reloaded.Sacks![1].Where(i => i.DoesStack).Select(i => i.StackSize).Should().Equal(100, 35, 7, 99);
		reloaded.Sacks[1].CountTQItems().Should().Be(100 + 35 + 1 + 7 + 99 + 1 + 1);
		reloaded.equipmentCtrlIOStreamVersion.Should().Be(pc.equipmentCtrlIOStreamVersion);
		reloaded.currentlyFocusedSackNumber.Should().Be(pc.currentlyFocusedSackNumber);
		reloaded.currentlySelectedSackNumber.Should().Be(pc.currentlySelectedSackNumber);
	}

	[Fact]
	public void Encode_ItemRemovedFromSack_IsGoneAfterReloadAndTheOthersStay()
	{
		var pc = this._fixtures.LoadPlayer();
		var sack = pc.Sacks![0];
		sack.RemoveItem(Find(sack, "US_L_ImmortalHero_Arms.dbr"));
		var expected = StatesOf(pc);

		var reloaded = this._fixtures.Reload(this._fixtures.PlayerProvider.Encode(pc));

		StatesOf(reloaded).Should().BeEquivalentTo(expected, o => o.WithStrictOrdering());
		reloaded.Sacks![0].Should().HaveCount(4);
	}

	[Fact]
	public void Encode_ItemAddedToEmptySack_IsThereAfterReload()
	{
		var pc = this._fixtures.LoadPlayer();
		var copy = Find(pc.Sacks![0], "US_L_ImmortalHero_Ring_02.dbr").Duplicate(false);
		copy.PositionX = 3;
		copy.PositionY = 1;
		pc.Sacks[2].AddItem(copy);
		var expected = StatesOf(pc);

		var reloaded = this._fixtures.Reload(this._fixtures.PlayerProvider.Encode(pc));

		StatesOf(reloaded).Should().BeEquivalentTo(expected, o => o.WithStrictOrdering());
		StateOf(reloaded.Sacks![2].Single()).Should().Be(StateOf(copy));
	}

	[Fact]
	public void Encode_PotionStackMovedBetweenSacks_KeepsItsSizeAndNewPosition()
	{
		var pc = this._fixtures.LoadPlayer();
		var stack = pc.Sacks![1].First(i => i.StackSize == 35);
		pc.Sacks[1].RemoveItem(stack);
		stack.PositionX = 7;
		stack.PositionY = 4;
		pc.Sacks[3].AddItem(stack);

		var reloaded = this._fixtures.Reload(this._fixtures.PlayerProvider.Encode(pc));

		var moved = reloaded.Sacks![3].Single();
		moved.StackSize.Should().Be(35);
		(moved.PositionX, moved.PositionY).Should().Be((7, 4));
		moved.Seed.Should().Be(stack.Seed, "the first record of a stack keeps the seed of the stack");
		reloaded.Sacks[1].Where(i => i.DoesStack).Select(i => i.StackSize).Should().Equal(100, 7, 99);
	}

	[Fact]
	public void Encode_ChangedAffixesAndRelic_AreReadBackAfterReload()
	{
		var pc = this._fixtures.LoadPlayer();
		var item = Find(pc.Sacks![0], "US_L_ImmortalHero_Head.dbr");
		item.prefixID = """Records\Item\LootMagicalAffixes\Prefix\Default\Rare_StrLife_02.dbr""";
		item.suffixID = """Records\Item\LootMagicalAffixes\Suffix\Default\Character_AbilityDefensive_04.dbr""";
		item.relicID = """records\item\relics\03_act1_hermessandal.dbr""";
		item.RelicBonusId = """Records\Item\LootMagicalAffixes\Relics\BonusesGreece\03\01_HermesSandal_AttLife.dbr""";
		item.Var1 = 3;

		var reloaded = this._fixtures.Reload(this._fixtures.PlayerProvider.Encode(pc));

		StateOf(Find(reloaded.Sacks![0], "US_L_ImmortalHero_Head.dbr")).Should().Be(StateOf(item));
	}

	[Fact]
	public void Encode_SecondRelicAddedToItemWithoutOne_WritesTheAtlantisFields()
	{
		var pc = this._fixtures.LoadPlayer();
		var item = Find(pc.Sacks![0], "US_L_ImmortalHero_Greaves.dbr");
		item.atlantis.Should().BeFalse("the test needs an item saved without the second relic fields");
		item.atlantis = true;
		item.relic2ID = """records\xpack3\items\relics\03_x3_atlasendurance.dbr""";
		item.RelicBonus2Id = """RECORDS\ITEM\LOOTMAGICALAFFIXES\PREFIX\DEFAULT\CHARACTER_ATTRIBUTEENERGY_06.DBR""";
		item.Var2 = 2;

		var reloaded = this._fixtures.Reload(this._fixtures.PlayerProvider.Encode(pc));

		StateOf(Find(reloaded.Sacks![0], "US_L_ImmortalHero_Greaves.dbr")).Should().Be(StateOf(item));
	}

	[Fact]
	public void Encode_SecondRelicRemoved_LeavesTheDefaultVar2()
	{
		var pc = this._fixtures.LoadPlayer();
		var greaves = pc.EquipmentSack!.GetItem(3);
		var relic = this._fixtures.ItemProvider.RemoveRelic2(greaves);

		var reloaded = this._fixtures.Reload(this._fixtures.PlayerProvider.Encode(pc));

		relic.Should().NotBeNull();
		var item = reloaded.EquipmentSack!.GetItem(3);
		RecordId.RawOf(item.relic2ID).Should().BeEmpty();
		item.Var2.Should().Be(Item.var2Default);
		RecordId.RawOf(item.relicID).Should().Be("""records\item\relics\03_act1_hermessandal.dbr""", "the first relic stays");
	}

	[Theory]
	[InlineData(0)] // helmet
	[InlineData(5)] // ring
	[InlineData(8)] // shield of the first weapon set
	[InlineData(9)] // weapon of the second set
	[InlineData(11)] // artifact
	public void Encode_EquipmentSlotEmptied_IsEmptyAfterReloadAndTheOtherSlotsStay(int slot)
	{
		var pc = this._fixtures.LoadPlayer();
		var equipment = pc.EquipmentSack!;
		EquipmentRules.TakeOut(equipment, equipment.GetItem(slot));
		var expected = StatesOf(equipment);

		var reloaded = this._fixtures.Reload(this._fixtures.PlayerProvider.Encode(pc));

		var reloadedEquipment = reloaded.EquipmentSack!;
		reloadedEquipment.Should().HaveCount(12);
		RecordId.RawOf(reloadedEquipment.GetItem(slot).BaseItemId).Should().BeEmpty();
		StatesOf(reloadedEquipment).Where((_, i) => i != slot)
			.Should().BeEquivalentTo(expected.Where((_, i) => i != slot), o => o.WithStrictOrdering());
	}

	[Fact]
	public void Encode_ItemEquippedFromSack_IsInTheSlotAfterReload()
	{
		var pc = this._fixtures.LoadPlayer();
		var equipment = pc.EquipmentSack!;
		var helmet = Find(pc.Sacks![0], "US_L_ImmortalHero_Head.dbr");
		pc.Sacks[0].RemoveItem(helmet);

		var pickedUp = EquipmentRules.Equip(equipment, helmet, 0, equipment.GetItem(0));

		var reloaded = this._fixtures.Reload(this._fixtures.PlayerProvider.Encode(pc));

		RecordId.RawOf(pickedUp!.BaseItemId).Should().EndWith("us_l_chiyousspite.dbr");
		var equipped = reloaded.EquipmentSack!.GetItem(0);
		RecordId.RawOf(equipped.BaseItemId).Should().EndWith("US_L_ImmortalHero_Head.dbr");
		equipped.Seed.Should().Be(helmet.Seed);
		(equipped.PositionX, equipped.PositionY).Should().Be((4, 0));
		reloaded.Sacks![0].Should().HaveCount(4);
	}

	[Fact]
	public void Encode_EquipmentSlotsWithItems_AreFlaggedAttachedExceptTheSecondWeaponSet()
	{
		var pc = this._fixtures.LoadPlayer();
		EquipmentRules.TakeOut(pc.EquipmentSack!, pc.EquipmentSack!.GetItem(0));

		var chr = this._fixtures.PlayerProvider.Encode(pc);

		var attached = new List<int>();
		var next = 0;
		for (var slot = 0; slot < 12; slot++)
		{
			var flag = this._fixtures.TQData.ReadIntAfter(chr, "itemAttached", next);
			attached.Add(flag.valueAsInt);
			next = flag.nextOffset;
		}

		// Slot 0 is empty, 9 and 10 are the second weapon set.
		attached.Should().Equal(0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1);
	}

	[Fact]
	public void CommitPlayerInfo_ChangedStats_AreReadBackAfterSave()
	{
		var pc = this._fixtures.LoadPlayer();
		var info = pc.PlayerInfo!;
		var changed = new PlayerInfo(new TQVaultAE.Services.PathIO())
		{
			CurrentLevel = info.CurrentLevel + 1,
			MasteriesAllowed = info.MasteriesAllowed,
			CurrentXP = info.CurrentXP + 1000,
			DifficultyUnlocked = 2,
			AttributesPoints = 7,
			SkillPoints = 9,
			BaseStrength = 300,
			BaseDexterity = 200,
			BaseIntelligence = 100,
			BaseHealth = 1500,
			BaseMana = 700,
			Money = 123456,
		};

		this._fixtures.PlayerProvider.CommitPlayerInfo(pc, changed);
		var path = this._fixtures.TempFile("Player.chr");
		this._fixtures.PlayerProvider.Save(pc, path);

		info.Modified.Should().BeTrue();
		var reread = this._fixtures.LoadPlayer(path).PlayerInfo!;
		reread.Should().BeEquivalentTo(changed, o => o
			.Including(p => p.CurrentLevel).Including(p => p.CurrentXP).Including(p => p.DifficultyUnlocked)
			.Including(p => p.AttributesPoints).Including(p => p.SkillPoints).Including(p => p.Money)
			.Including(p => p.BaseStrength).Including(p => p.BaseDexterity).Including(p => p.BaseIntelligence)
			.Including(p => p.BaseHealth).Including(p => p.BaseMana).Including(p => p.MasteriesAllowed));
		reread.SkillRecordList.Should().HaveCount(info.SkillRecordList.Count);
		reread.Class.Should().Be(info.Class);
	}

	[Fact]
	public void CommitPlayerInfo_MasteryReset_WritesTheReducedSkillListAndClearsTheClass()
	{
		var pc = this._fixtures.LoadPlayer();
		var info = pc.PlayerInfo!;
		var kept = info.SkillRecordList.Take(5).ToList();
		var changed = new PlayerInfo(new TQVaultAE.Services.PathIO())
		{
			CurrentLevel = info.CurrentLevel,
			MasteriesAllowed = info.MasteriesAllowed,
			MasteriesResetRequiered = true,
			CurrentXP = info.CurrentXP,
			Money = info.Money,
			Class = string.Empty,
		};
		changed.SkillRecordList.AddRange(kept);
		info.MasteriesResetRequiered = true;

		this._fixtures.PlayerProvider.CommitPlayerInfo(pc, changed);
		var reloaded = this._fixtures.Reload(this._fixtures.PlayerProvider.Encode(pc));

		var reread = reloaded.PlayerInfo!;
		reread.SkillRecordList.Should().BeEquivalentTo(kept, o => o.WithStrictOrdering());
		reread.Class.Should().BeEmpty();
		reread.Money.Should().Be(info.Money);
		StatesOf(reloaded).Should().BeEquivalentTo(StatesOf(pc), o => o.WithStrictOrdering(), "the items after the skills are not touched");
	}

	[Fact]
	public void ReadPlayerInfo_GreatestMonsterKilled_ReadsItsLifeAndManaNotItsLevel()
	{
		var pc = this._fixtures.LoadPlayer();

		var info = pc.PlayerInfo!;

		info.GreatestMonster.Should().Be("{^r}Nidhoggr ~ Tormentor of the Dead");
		info.GreatestMonsterKilledLevel.Should().Be(48);
		info.GreatestMonsterKilledLifeAndMana.Should().Be(50400);
	}

	#endregion

	#region Stash (winsys.dxb / winsys.dxg)

	[Fact]
	public void Save_UnchangedStash_WritesTheGameDxbAndDxgBack()
	{
		var stash = this._fixtures.LoadStash();
		var path = this._fixtures.TempFile("winsys.dxb");

		this._fixtures.StashProvider.Save(stash, path);

		File.ReadAllBytes(path).Should().Equal(File.ReadAllBytes(SaveFixtures.StashDxb));
		File.ReadAllBytes(Path.ChangeExtension(path, ".dxg")).Should().Equal(File.ReadAllBytes(SaveFixtures.StashDxg));
	}

	[Fact]
	public void LoadFile_BackupDxg_ReadsTheSameItemsAsTheDxb()
	{
		var stash = this._fixtures.LoadStash();

		var backup = this._fixtures.LoadStash(SaveFixtures.StashDxg);

		StatesOf(backup.Sack!).Should().BeEquivalentTo(StatesOf(stash.Sack!), o => o.WithStrictOrdering());
		(backup.Width, backup.Height, backup.stashVersion).Should().Be((stash.Width, stash.Height, stash.stashVersion));
	}

	[Fact]
	public void Save_EditedStash_IsReadBackWithTheEdits()
	{
		var stash = this._fixtures.LoadStash();
		var sack = stash.Sack!;
		sack.RemoveItem(Find(sack, "Parchment16.dbr"));
		var potions = Find(sack, "PotionMana_05.dbr");
		potions.StackSize = 10;
		potions.PositionX = 8;
		potions.PositionY = 2;
		var copy = Find(sack, "e_la_darkcore.dbr").Duplicate(false);
		copy.PositionX = 0;
		copy.PositionY = 0;
		sack.AddItem(copy);
		var expected = StatesOf(sack);
		var path = this._fixtures.TempFile("winsys.dxb");

		this._fixtures.StashProvider.Save(stash, path);

		var reloaded = this._fixtures.LoadStash(path);
		StatesOf(reloaded.Sack!).Should().BeEquivalentTo(expected, o => o.WithStrictOrdering());
		var backup = this._fixtures.LoadStash(Path.ChangeExtension(path, ".dxg"));
		StatesOf(backup.Sack!).Should().BeEquivalentTo(expected, o => o.WithStrictOrdering());
	}

	[Fact]
	public void Save_Stash_WritesAValidChecksumInBothFiles()
	{
		var stash = this._fixtures.LoadStash();
		stash.Sack!.RemoveAtItem(0);
		var path = this._fixtures.TempFile("winsys.dxb");

		this._fixtures.StashProvider.Save(stash, path);

		foreach (var file in new[] { path, Path.ChangeExtension(path, ".dxg") })
		{
			var data = File.ReadAllBytes(file);
			var stored = BinaryPrimitives.ReadUInt32LittleEndian(data);
			data.AsSpan(0, 4).Clear();
			var computed = BinaryPrimitives.ReadUInt32LittleEndian(this._fixtures.StashProvider.CalculateCRC(data));
			computed.Should().Be(stored, file);
		}
	}

	#endregion

	#region Vaults (.vault.json and legacy .vault)

	[Fact]
	public void Save_UnchangedJsonVault_WritesTheSameFile()
	{
		var vault = this._fixtures.LoadVault(SaveFixtures.VaultJson);
		var path = this._fixtures.TempFile("claude-vault1.vault.json");

		this._fixtures.PlayerProvider.Save(vault, path);

		File.ReadAllBytes(path).Should().Equal(File.ReadAllBytes(SaveFixtures.VaultJson));
	}

	[Fact]
	public void Save_EditedJsonVault_IsReadBackWithTheEdits()
	{
		var vault = this._fixtures.LoadVault(SaveFixtures.VaultJson);
		var amulet = vault.Sacks![0].Single();
		vault.Sacks[0].RemoveItem(amulet);
		amulet.PositionX = 2;
		amulet.PositionY = 3;
		vault.Sacks[4].AddItem(amulet);
		var socketed = amulet.Duplicate(false);
		socketed.prefixID = """Records\Item\LootMagicalAffixes\Prefix\Default\Rare_StrLife_02.dbr""";
		socketed.relicID = """records\item\relics\03_act1_hermessandal.dbr""";
		socketed.RelicBonusId = """Records\Item\LootMagicalAffixes\Relics\BonusesGreece\03\01_HermesSandal_AttLife.dbr""";
		socketed.Var1 = 3;
		socketed.atlantis = true;
		socketed.relic2ID = """records\xpack3\items\relics\03_x3_atlasendurance.dbr""";
		socketed.RelicBonus2Id = """RECORDS\ITEM\LOOTMAGICALAFFIXES\PREFIX\DEFAULT\CHARACTER_ATTRIBUTEENERGY_06.DBR""";
		socketed.Var2 = 2;
		socketed.PositionX = 0;
		socketed.PositionY = 0;
		vault.Sacks[4].AddItem(socketed);
		vault.currentlySelectedSackNumber = 4;
		vault.DisabledTooltipBagId = [1, 3];
		var path = this._fixtures.TempFile("claude-vault1.vault.json");

		this._fixtures.PlayerProvider.Save(vault, path);

		var reloaded = this._fixtures.LoadVault(path);
		StatesOf(reloaded).Should().BeEquivalentTo(StatesOf(vault), o => o.WithStrictOrdering());
		reloaded.currentlySelectedSackNumber.Should().Be(4);
		reloaded.DisabledTooltipBagId.Should().Equal(1, 3);
	}

	[Fact]
	public void LegacyVault_EncodedFromJsonVault_IsReadBackWithTheSameItemsAndReencodedIdentically()
	{
		var vault = this._fixtures.LoadVault(SaveFixtures.VaultJson);
		var copy = vault.Sacks![0].Single().Duplicate(false);
		copy.PositionX = 1;
		copy.PositionY = 1;
		vault.Sacks[2].AddItem(copy);
		var legacy = this._fixtures.PlayerProvider.Encode(vault);
		var path = this._fixtures.TempFile("claude-vault1.vault");
		File.WriteAllBytes(path, legacy);

		var reloaded = this._fixtures.LoadVault(path);

		StatesOf(reloaded).Should().BeEquivalentTo(StatesOf(vault), o => o.WithStrictOrdering());
		reloaded.EquipmentSack.Should().BeNull("a vault has no equipment");
		reloaded.PlayerInfo.Should().BeNull();
		this._fixtures.PlayerProvider.Encode(reloaded).Should().Equal(legacy);
	}

	[Fact]
	public void Save_LegacyVault_IsWrittenAsJson()
	{
		var vault = this._fixtures.LoadVault(SaveFixtures.VaultJson);
		var path = this._fixtures.TempFile("claude-vault1.vault");
		File.WriteAllBytes(path, this._fixtures.PlayerProvider.Encode(vault));
		var legacy = this._fixtures.LoadVault(path);
		var jsonPath = this._fixtures.TempFile("claude-vault1.vault.json");

		this._fixtures.PlayerProvider.Save(legacy, jsonPath);

		StatesOf(this._fixtures.LoadVault(jsonPath)).Should().BeEquivalentTo(StatesOf(vault), o => o.WithStrictOrdering());
	}

	/// <summary>The bytes of an item written in a character's sack.</summary>
	private byte[] EncodeInSack(Item item)
	{
		using var stream = new MemoryStream();
		using (var writer = new BinaryWriter(stream))
		{
			item.Place.SackType = SackType.Player;
			this._fixtures.ItemProvider.Encode(item, writer);
		}

		return stream.ToArray();
	}

	/// <summary>
	/// An item of the character goes to a JSON vault, is saved, reloaded and put back in the character:
	/// nothing it holds is lost, even the second relic fields written empty by the game.
	/// </summary>
	[Theory]
	[InlineData(-1, 6)] // ring: prefix, suffix, relic and bonus, empty second relic, var2 2023419968
	[InlineData(-1, 5)] // ring: empty second relic, var2 0
	[InlineData(1, 6)] // shield in a sack: empty second relic, var2 1065353216
	[InlineData(0, 0)] // helmet saved without the second relic fields
	public void CharacterItem_ThroughJsonVaultAndBack_KeepsAllItsValuesAndBytes(int sackNumber, int index)
	{
		var pc = this._fixtures.LoadPlayer();
		var sack = sackNumber == -1 ? pc.EquipmentSack! : pc.Sacks![sackNumber];
		var item = sack.GetItem(index);
		EquipmentRules.TakeOut(sack, item);
		item.PositionX = 2;
		item.PositionY = 1;
		var original = StateOf(item);
		// The json keeps no block marker values: an item read from a vault gets the default ones.
		var withDefaultMarkers = item.Clone();
		withDefaultMarkers.beginBlockCrap1 = withDefaultMarkers.beginBlockCrap2 = this._fixtures.TQData.BeginBlockValue;
		withDefaultMarkers.endBlockCrap1 = withDefaultMarkers.endBlockCrap2 = this._fixtures.TQData.EndBlockValue;
		var originalBytes = this.EncodeInSack(withDefaultMarkers);
		var vault = this._fixtures.LoadVault(SaveFixtures.VaultJson);
		vault.Sacks![3].AddItem(item);
		var vaultPath = this._fixtures.TempFile("claude-vault1.vault.json");
		this._fixtures.PlayerProvider.Save(vault, vaultPath);

		var fromVault = this._fixtures.LoadVault(vaultPath).Sacks![3].Single();
		pc.Sacks![2].AddItem(fromVault);
		var reloaded = this._fixtures.Reload(this._fixtures.PlayerProvider.Encode(pc)).Sacks![2].Single();

		StateOf(fromVault).Should().Be(original);
		StateOf(reloaded).Should().Be(original);
		RecordId.RawOf(reloaded.relic2ID).Should().BeEmpty();
		this.EncodeInSack(reloaded).Should().Equal(originalBytes);
	}

	#endregion
}
