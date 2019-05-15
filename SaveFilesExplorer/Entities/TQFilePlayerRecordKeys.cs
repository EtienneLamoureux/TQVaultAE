using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveFilesExplorer.Entities
{
	/// <summary>
	/// List of keys in player save file with data size as [DataType]
	/// Be carreful this is case sensitive.
	/// </summary>
	public enum TQFilePlayerRecordKey
	{

		// TODO Recheck every TQFileDataType based on file versions

		[TQFileDataType(TQVersion.All, TQFileDataType.Int)]
		headerVersion,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.TQ_AnsiString)]
		playerCharacterClass,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.ByteArrayFixedSize16)]
		uniqueId,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.TQ_SizedByteArray)]
		streamData,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.TQ_AnsiString)]
		playerClassTag,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		playerLevel,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		playerVersion,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		begin_block,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.TQ_UTF16String)]
		myPlayerName,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		isInMainQuest,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		disableAutoPopV2,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		numTutorialPagesV2,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		currentPageV2,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		versionCheckTeleportInfo,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		teleportUIDsSize,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.ByteArrayFixedSize16)]
		teleportUID,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		versionCheckMovementInfo,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		markerUIDsSize,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.ByteArrayFixedSize16)]
		markerUID,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		versionCheckRespawnInfo,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		respawnUIDsSize,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.ByteArrayFixedSize16)]
		respawnUID,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		versionRespawnPoint,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.ByteArrayFixedSize16)]
		[Description("strategicMovementRespawnPoint[i]")]
		strategicMovementRespawnPoint,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		money,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		compassState,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		skillWindowShowHelp,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		alternateConfig,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		alternateConfigEnabled,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.TQ_AnsiString)]
		playerTexture,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		itemsFoundOverLifetimeUniqueTotal,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		itemsFoundOverLifetimeRandomizedTotal,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		temp,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		hasBeenInGame,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		end_block,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		max,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.TQ_AnsiString)]
		skillName,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		skillLevel,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		skillEnabled,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		skillSubLevel,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		skillActive,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		skillTransition,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		masteriesAllowed,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		skillReclamationPointsUsed,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		equipmentSelection,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		skillWindowSelection,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		skillSettingValid,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		primarySkill1,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		secondarySkill1,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		skillActive1,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		primarySkill2,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		secondarySkill2,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		skillActive2,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		primarySkill3,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		secondarySkill3,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		skillActive3,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		primarySkill4,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		secondarySkill4,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		skillActive4,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		primarySkill5,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		secondarySkill5,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		skillActive5,

		//[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		//[Description("currentStats.charLevel")]
		//currentStats_charLevel,

		//[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		//[Description("currentStats.experiencePoints")]
		//currentStats_experiencePoints,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		modifierPoints,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		skillPoints,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		playTimeInSeconds,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		numberOfDeaths,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		numberOfKills,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		experienceFromKills,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		healthPotionsUsed,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		manaPotionsUsed,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		maxLevel,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		numHitsReceived,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		numHitsInflicted,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		greatestDamageInflicted,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.TQ_UTF16String)]
		[Description("(*greatestMonsterKilledName)[i]")]
		greatestMonsterKilledName,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		[Description("(*greatestMonsterKilledLevel)[i]")]
		greatestMonsterKilledLevel,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		[Description("(*greatestMonsterKilledLifeAndMana)[i]")]
		greatestMonsterKilledLifeAndMana,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		criticalHitsInflicted,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		criticalHitsReceived,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		controllerStreamed,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		itemPositionsSavedAsGridCoords,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		numberOfSacks,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		currentlyFocusedSackNumber,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		currentlySelectedSackNumber,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		tempBool,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		size,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.TQ_AnsiString)]
		baseName,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.TQ_AnsiString)]
		prefixName,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.TQ_AnsiString)]
		suffixName,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.TQ_AnsiString)]
		relicName,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.TQ_AnsiString)]
		relicBonus,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		seed,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		var1,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.TQ_AnsiString)]
		relicName2,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.TQ_AnsiString)]
		relicBonus2,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		var2,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		pointX,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		pointY,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		useAlternate,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		equipmentCtrlIOStreamVersion,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		itemAttached,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		alternate,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		storedType,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.Int)]
		isItemSkill,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.TQ_AnsiString)]
		itemName,

		[TQFileDataType(TQVersion.TQITAE_All, TQFileDataType.TQ_SizedByteArray)]
		description
	}
}
