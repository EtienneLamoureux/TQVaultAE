using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace TQVaultData
{
	/// <summary>
	/// gathers character data from the player.chr file
	/// </summary>
	public class PlayerInfoReader : PlayerInfoIO
	{

		private PlayerInfo _playInfo = new PlayerInfo();

		/// records starting point offsets as PlayerCollction reads player.chr file 
		readonly Dictionary<string, int> _playerKeys = new Dictionary<string, int>
		{
			{ "MYPLAYERNAME", 0 },
			{ "CURRENTSTATS.CHARLEVEL", 0 },
			{ "TEMP", 0},
			{ "PLAYTIMEINSECONDS", 0},
		};

		public PlayerInfoReader()
		{

		}

		/// <summary>
		/// Will return true if one or more starting offsets found
		/// </summary>
		bool _foundPlayerInfo = false;
		public bool FoundPlayerInfo { get => _foundPlayerInfo; }


		public bool Match(string blockName)
		{
			if (string.IsNullOrEmpty(blockName)) return false;
			return (_playerKeys.ContainsKey(blockName.ToUpper()));
		}

		/// <summary>
		/// saves starting offset when found
		/// </summary>
		/// <param name="blockName">Name of block</param>
		/// <param name="offset">offset of value of block</param>
		public void Record(string blockName, int offset)
		{
			if (string.IsNullOrEmpty(blockName)) return;
			if (Match(blockName))
			{
				_playerKeys[blockName.ToUpper()] = offset;
				_foundPlayerInfo = true;
			}
		}

		/// <summary>
		/// Find character data in player.chr file
		/// </summary>
		/// <param name="reader"></param>
		public void Read(BinaryReader reader)
		{
			ReadInternal(reader);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader">Reader to player.chr file</param>
		private void ReadInternal(BinaryReader reader)
		{
			_playInfo.Modified = false;
			var offset = 0;
			reader.BaseStream.Seek(offset, SeekOrigin.Begin);
			TQData.ValidateNextString("headerVersion", reader);
			reader.ReadInt32();

			TQData.ValidateNextString("playerCharacterClass", reader);
			TQData.ReadCString(reader);

			TQData.ValidateNextString("uniqueId", reader);
			reader.ReadBytes(16);

			TQData.ValidateNextString("streamData", reader);
			var len = reader.ReadInt32();
			reader.ReadBytes(len);

			TQData.ValidateNextString("playerClassTag", reader);
			var tag = TQData.ReadCString(reader);
			_playInfo.Class = tag;

			_playInfo.Money = ReadMoneyValue(reader);

			_playInfo.DifficultyUnlocked =  ReadDifficultyUnlockedValue(reader);
			TQData.ValidateNextString("hasBeenInGame", reader);
			_playInfo.HasBeenInGame = reader.ReadInt32();

			offset = _playerKeys["CURRENTSTATS.CHARLEVEL"];
			reader.BaseStream.Seek(offset, SeekOrigin.Begin);
			_playInfo.CurrentLevel = reader.ReadInt32();

			TQData.ValidateNextString("currentStats.experiencePoints", reader);
			_playInfo.CurrentXP = reader.ReadInt32();

			TQData.ValidateNextString("modifierPoints", reader);
			_playInfo.AttributesPoints = reader.ReadInt32();

			TQData.ValidateNextString("skillPoints", reader);
			_playInfo.SkillPoints = reader.ReadInt32();

			offset = _playerKeys["TEMP"];
			reader.BaseStream.Seek(offset, SeekOrigin.Begin);
			_playInfo.BaseStrength = Convert.ToInt32(reader.ReadSingle());

			TQData.ValidateNextString("temp", reader);
			_playInfo.BaseDexterity = Convert.ToInt32(reader.ReadSingle());

			TQData.ValidateNextString("temp", reader);
			_playInfo.BaseIntelligence = Convert.ToInt32(reader.ReadSingle());

			TQData.ValidateNextString("temp", reader);
			_playInfo.BaseHealth = Convert.ToInt32(reader.ReadSingle());

			TQData.ValidateNextString("temp", reader);
			_playInfo.BaseMana = Convert.ToInt32(reader.ReadSingle());

			offset = _playerKeys["PLAYTIMEINSECONDS"];
			reader.BaseStream.Seek(offset, SeekOrigin.Begin);
			_playInfo.PlayTimeInSeconds = reader.ReadInt32();

			TQData.ValidateNextString("numberOfDeaths", reader);
			_playInfo.NumberOfDeaths = reader.ReadInt32();

			TQData.ValidateNextString("numberOfKills", reader);
			_playInfo.NumberOfKills = reader.ReadInt32();

			TQData.ValidateNextString("experienceFromKills", reader);
			_playInfo.ExperienceFromKills = reader.ReadInt32();

			TQData.ValidateNextString("healthPotionsUsed", reader);
			_playInfo.HealthPotionsUsed = reader.ReadInt32();

			TQData.ValidateNextString("manaPotionsUsed", reader);
			_playInfo.ManaPotionsUsed = reader.ReadInt32();

			TQData.ValidateNextString("maxLevel", reader);
			_playInfo.MaxLevel = reader.ReadInt32();

			TQData.ValidateNextString("numHitsReceived", reader);
			_playInfo.NumHitsReceived = reader.ReadInt32();

			TQData.ValidateNextString("numHitsInflicted", reader);
			_playInfo.NumHitsInflicted = reader.ReadInt32();

			TQData.ValidateNextString("greatestDamageInflicted", reader);
			_playInfo.GreatestDamageInflicted = reader.ReadInt32();

			TQData.ValidateNextString("(*greatestMonsterKilledName)[i]", reader);
			_playInfo.GreatestMonster = TQData.ReadUTF16String(reader);

			TQData.ValidateNextString("(*greatestMonsterKilledLevel)[i]", reader);
			reader.ReadInt32();

			TQData.ValidateNextString("(*greatestMonsterKilledLifeAndMana)[i]", reader);
			reader.ReadInt32();

			TQData.ValidateNextString("(*greatestMonsterKilledName)[i]", reader);
			_playInfo.GreatestMonster = TQData.ReadUTF16String(reader);

			TQData.ValidateNextString("(*greatestMonsterKilledLevel)[i]", reader);
			reader.ReadInt32();

			TQData.ValidateNextString("(*greatestMonsterKilledLifeAndMana)[i]", reader);
			reader.ReadInt32();

			TQData.ValidateNextString("(*greatestMonsterKilledName)[i]", reader);
			_playInfo.GreatestMonster = TQData.ReadUTF16String(reader);

			TQData.ValidateNextString("(*greatestMonsterKilledLevel)[i]", reader);
			reader.ReadInt32();

			TQData.ValidateNextString("(*greatestMonsterKilledLifeAndMana)[i]", reader);
			reader.ReadInt32();

			TQData.ValidateNextString("criticalHitsInflicted", reader);
			_playInfo.CriticalHitsInflicted = reader.ReadInt32();

			TQData.ValidateNextString("criticalHitsReceived", reader);
			_playInfo.CriticalHitsReceived = reader.ReadInt32();

		}

		/// <summary>
		/// returns player data found in player.chr file
		/// </summary>
		/// <returns></returns>
		public PlayerInfo GetPlayerInfo()
		{
			return (_playInfo);
		}


	}
}
