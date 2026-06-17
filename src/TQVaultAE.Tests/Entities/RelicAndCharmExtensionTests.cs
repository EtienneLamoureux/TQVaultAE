using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Entities;

public class RelicAndCharmExtensionTests
{
	[Fact]
	public void GetGearType_Unknown_ReturnsUndefined()
	{
		var result = RelicAndCharm.Unknown.GetGearType();
		Assert.Equal(GearType.Undefined, result);
	}

	[Fact]
	public void GetRecordId_Unknown_ReturnsNoRecords()
	{
		var result = RelicAndCharm.Unknown.GetRecordId();
		Assert.Equal("NORECORDS", result);
	}

	[Fact]
	public void GetGearType_AegisOfAthena_ReturnsShield()
	{
		// Aegis is a shield relic
		var result = RelicAndCharm.AEGISOFATHENA_ACT1_01.GetGearType();
		Assert.Equal(GearType.Shield, result);
	}

	[Fact]
	public void GetRecordId_AegisOfAthena_ReturnsValidRecordPath()
	{
		var result = RelicAndCharm.AEGISOFATHENA_ACT1_01.GetRecordId();
		Assert.Contains("RECORDS", result);
		Assert.Contains("AEGISOFATHENA", result);
	}

	[Fact]
	public void GetGearType_AmunRaGlory_ReturnsAllArmor()
	{
		// Amun-Ra is an armor relic
		var result = RelicAndCharm.AMUNRASGLORY_ACT2_01.GetGearType();
		Assert.Equal(GearType.AllArmor, result);
	}

	[Fact]
	public void GetRecordId_AmunRaGlory_ReturnsValidRecordPath()
	{
		var result = RelicAndCharm.AMUNRASGLORY_ACT2_01.GetRecordId();
		Assert.Contains("RECORDS", result);
		Assert.Contains("AMUNRASGLORY", result);
	}

	[Fact]
	public void GetGearType_AnkhOfIsis_ReturnsJewellery()
	{
		// Ankh of Isis enchants rings and amulets
		var result = RelicAndCharm.ANKHOFISIS_ACT2_01.GetGearType();
		Assert.Equal(GearType.Jewellery, result);
	}

	[Fact]
	public void GetGearType_DifferentActLevels_ReturnsSameGearType()
	{
		// All 3 act levels of Aegis should be Shield
		var act1 = RelicAndCharm.AEGISOFATHENA_ACT1_01.GetGearType();
		var act2 = RelicAndCharm.AEGISOFATHENA_ACT1_02.GetGearType();
		var act3 = RelicAndCharm.AEGISOFATHENA_ACT1_03.GetGearType();

		Assert.Equal(GearType.Shield, act1);
		Assert.Equal(GearType.Shield, act2);
		Assert.Equal(GearType.Shield, act3);
	}

	[Fact]
	public void GetRecordId_DifferentActLevels_ReturnsDistinctRecords()
	{
		var act1 = RelicAndCharm.AEGISOFATHENA_ACT1_01.GetRecordId();
		var act2 = RelicAndCharm.AEGISOFATHENA_ACT1_02.GetRecordId();
		var act3 = RelicAndCharm.AEGISOFATHENA_ACT1_03.GetRecordId();

		Assert.NotEqual(act1, act2);
		Assert.NotEqual(act2, act3);
		Assert.NotEqual(act1, act3);
	}

	[Fact]
	public void GetRecordId_DifferentRelics_ReturnsDistinctRecords()
	{
		var aegis = RelicAndCharm.AEGISOFATHENA_ACT1_01.GetRecordId();
		var amunRa = RelicAndCharm.AMUNRASGLORY_ACT2_01.GetRecordId();

		Assert.NotEqual(aegis, amunRa);
	}

	[Fact]
	public void GetGearType_HC_ChaosEssence_ReturnsHead()
	{
		// Chaos Essence is head armor
		var result = RelicAndCharm.HCDUNGEON_ESSENCEOFCHAOS_X4_03.GetGearType();
		Assert.Equal(GearType.Head, result);
	}

	[Fact]
	public void GetRecordId_HC_ChaosEssence_ReturnsValidRecordPath()
	{
		var result = RelicAndCharm.HCDUNGEON_ESSENCEOFCHAOS_X4_03.GetRecordId();
		Assert.Contains("HCDUNGEON", result);
		Assert.Contains("ESSENCEOFCHAOS", result);
	}

	[Fact]
	public void AllRelicAndCharmValues_ReturnValidGearType()
	{
		var values = Enum.GetValues<RelicAndCharm>();
		foreach (RelicAndCharm value in values)
		{
			var gearType = value.GetGearType();
			// Unknown is the only value that returns Undefined
			if (value != RelicAndCharm.Unknown)
			{
				Assert.True(gearType != GearType.Undefined,
					$"GetGearType for {value} returned Undefined GearType");
			}
		}
	}

	[Fact]
	public void AllRelicAndCharmValues_ReturnNonEmptyRecordId()
	{
		var values = Enum.GetValues<RelicAndCharm>();
		foreach (RelicAndCharm value in values)
		{
			var recordId = value.GetRecordId();
			Assert.False(string.IsNullOrEmpty(recordId),
				$"GetRecordId for {value} returned null or empty");
		}
	}
}
