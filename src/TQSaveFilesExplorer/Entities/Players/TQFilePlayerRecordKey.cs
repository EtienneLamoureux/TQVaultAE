using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TQ.SaveFilesExplorer.Entities.Players
{
	/// <summary>
	/// List of keys in player save file with data size as [DataType]
	/// Be carreful this is case sensitive.
	/// </summary>
	public enum TQFilePlayerRecordKey
	{
		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		headerVersion,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.String1252)]
		playerCharacterClass,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.ByteArray16)]
		uniqueId,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.ByteArrayVar)]
		streamData,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.String1252)]
		playerClassTag,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		playerLevel,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		playerVersion,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		begin_block,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.StringUTF16)]
		myPlayerName,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		isInMainQuest,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		disableAutoPopV2,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		numTutorialPagesV2,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		currentPageV2,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		versionCheckTeleportInfo,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		teleportUIDsSize,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.ByteArray16)]
		teleportUID,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		versionCheckMovementInfo,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		markerUIDsSize,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.ByteArray16)]
		markerUID,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		versionCheckRespawnInfo,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		respawnUIDsSize,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.ByteArray16)]
		respawnUID,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		versionRespawnPoint,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.ByteArray16)]
		[Description("strategicMovementRespawnPoint[i]")]
		strategicMovementRespawnPoint,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		money,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		compassState,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		skillWindowShowHelp,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		alternateConfig,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		alternateConfigEnabled,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.String1252)]
		playerTexture,

		[TQFileDataType(TQVersion.TQIT | TQVersion.TQAE, TQFileDataType.Int)]
		itemsFoundOverLifetimeUniqueTotal,

		[TQFileDataType(TQVersion.TQIT | TQVersion.TQAE, TQFileDataType.Int)]
		itemsFoundOverLifetimeRandomizedTotal,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Float)]
		temp,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		hasBeenInGame,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		end_block,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		max,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.String1252)]
		skillName,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		skillLevel,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		skillEnabled,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		skillSubLevel,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		skillActive,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		skillTransition,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		masteriesAllowed,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		skillReclamationPointsUsed,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		equipmentSelection,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		skillWindowSelection,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		skillSettingValid,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		primarySkill1,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		secondarySkill1,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		skillActive1,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		primarySkill2,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		secondarySkill2,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		skillActive2,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		primarySkill3,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		secondarySkill3,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		skillActive3,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		primarySkill4,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		secondarySkill4,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		skillActive4,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		primarySkill5,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		secondarySkill5,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		skillActive5,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		[Description("currentStats.charLevel")]
		currentStats_charLevel,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		[Description("currentStats.experiencePoints")]
		currentStats_experiencePoints,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		modifierPoints,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		skillPoints,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		playTimeInSeconds,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		numberOfDeaths,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		numberOfKills,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		experienceFromKills,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		healthPotionsUsed,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		manaPotionsUsed,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		maxLevel,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		numHitsReceived,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		numHitsInflicted,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		greatestDamageInflicted,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.StringUTF16)]
		[Description("(*greatestMonsterKilledName)[i]")]
		greatestMonsterKilledName,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		[Description("(*greatestMonsterKilledLevel)[i]")]
		greatestMonsterKilledLevel,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		[Description("(*greatestMonsterKilledLifeAndMana)[i]")]
		greatestMonsterKilledLifeAndMana,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		criticalHitsInflicted,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		criticalHitsReceived,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		controllerStreamed,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		itemPositionsSavedAsGridCoords,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		numberOfSacks,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		currentlyFocusedSackNumber,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		currentlySelectedSackNumber,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		tempBool,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		size,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.String1252)]
		baseName,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.String1252)]
		prefixName,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.String1252)]
		suffixName,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.String1252)]
		relicName,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.String1252)]
		relicBonus,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		seed,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		var1,

		[TQFileDataType(TQVersion.TQAE, TQFileDataType.String1252)]
		relicName2,

		[TQFileDataType(TQVersion.TQAE, TQFileDataType.String1252)]
		relicBonus2,

		[TQFileDataType(TQVersion.TQAE, TQFileDataType.Int)]
		var2,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		pointX,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		pointY,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		useAlternate,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		equipmentCtrlIOStreamVersion,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		itemAttached,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		alternate,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		storedType,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.Int)]
		isItemSkill,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.String1252)]
		itemName,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.ByteArrayVar)]
		description,

		[TQFileDataType(TQVersion.TQ, TQFileDataType.Int)]
		storedDefaultType,


		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.String1252)]
		scrollName,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.String1252)]
		bitmapUpName,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.String1252)]
		bitmapDownName,

		[TQFileDataType(TQVersion.TQ_All, TQFileDataType.StringUTF16)]
		defaultText,
	}
}
