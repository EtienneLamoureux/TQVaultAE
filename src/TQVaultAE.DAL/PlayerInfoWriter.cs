using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TQVaultData
{
	public class PlayerInfoWriter : PlayerInfoIO
	{
		private Dictionary<string, PlayerInfoKeyPair> _list = new Dictionary<string, PlayerInfoKeyPair>();
		private bool _validated = false;
		private bool _modified = false;


		/// <summary>
		/// Updates the raw file only if the value has changed.
		/// </summary>
		/// <param name="writer">data writer</param>
		/// <param name="data">data</param>
		/// <param name="newValue">new value to write</param>
		/// <returns></returns>
		private bool UpdateIfChanged(BinaryWriter writer, PlayerInfoKeyPair data, int newValue)
		{
			if (newValue != data.Value4byte)
			{
				data.Value4byte = newValue;
				WriteKeyValue(writer, data);
				///set modified to true to notify program the player.chr file needs to be updated
				_modified = true;
				return (true);
			}
			return (false);
		}

		/// <summary>
		/// Commits the player info changes to the player.chr file
		/// </summary>
		/// <param name="playerInfo"></param>
		/// <param name="playerFileRawData"></param>
		public void Commit(PlayerInfo playerInfo, byte[] playerFileRawData)
		{
			if (!_validated) return;
			_modified = false;
			using (var ms = new MemoryStream(playerFileRawData))
			{
				using (var writer = new BinaryWriter(ms))
				{
					
					var data = _list["playerLevel"];
					if (playerInfo.CurrentLevel != data.Value4byte)
					{
						UpdateIfChanged(writer, data, playerInfo.CurrentLevel);

						data = _list["playercurrentLevel"];
						UpdateIfChanged(writer, data, playerInfo.CurrentLevel);

						data = _list["playerMaxLevel"];
						UpdateIfChanged(writer, data, playerInfo.CurrentLevel);
					}

					data = _list["money"];
					UpdateIfChanged(writer, data, playerInfo.Money);

					data = _list["playerdifficulty"];
					UpdateIfChanged(writer, data, playerInfo.DifficultyUnlocked);

					data = _list["playercurrentxp"];
					UpdateIfChanged(writer, data, playerInfo.CurrentXP);

					data = _list["playermodifierpoints"];
					UpdateIfChanged(writer, data, playerInfo.AttributesPoints);

					data = _list["playerskillpoints"];
					UpdateIfChanged(writer, data, playerInfo.SkillPoints);

					data = _list["strength"];
					UpdateIfChanged(writer, data, playerInfo.BaseStrength);

					data = _list["dexterity"];
					UpdateIfChanged(writer, data, playerInfo.BaseDexterity);

					data = _list["intelligence"];
					UpdateIfChanged(writer, data, playerInfo.BaseIntelligence);

					data = _list["health"];
					UpdateIfChanged(writer, data, playerInfo.BaseHealth);

					data = _list["mana"];
					UpdateIfChanged(writer, data, playerInfo.BaseMana);


					///if this value is set to true, the TQVaultAE program will know save the player.chr file
					playerInfo.Modified = _modified;

				}
			}
		}


		/// <summary>
		/// Reads the raw file and validates the data has not changed since previously read.  Also stores offsets for commiting data.
		/// </summary>
		/// <param name="playerInfo">player info</param>
		/// <param name="playerFileRawData">raw player.chr file</param>
		public void Validate(PlayerInfo playerInfo, byte[] playerFileRawData)
		{
			_validated = false;
			_list.Clear();
			using (var ms = new MemoryStream(playerFileRawData))
			{
				using (var reader = new BinaryReader(ms))
				{
					var data = ReadPlayerLevel(reader);
					if (data == null)
					{
						throw new ArgumentException("Error reading Player Level");
					}
					_list.Add("playerLevel", data);

					data = ReadPlayerMoney(reader, reader.BaseStream.Position);
					if (data == null)
					{
						throw new ArgumentException("Error reading money");
					}
					_list.Add("money", data);

					data = ReadDiffcultyLevel(reader, reader.BaseStream.Position);
					if (data == null)
					{
						throw new ArgumentException("Error reading difficulty Level");
					}
					_list.Add("playerdifficulty", data);

					data = ReadPlayerCurrentLevel(reader, reader.BaseStream.Position);
					if (data == null)
					{
						throw new ArgumentException("Error reading current Level");
					}
					_list.Add("playercurrentLevel", data);

					data = ReadPlayerExperience(reader, reader.BaseStream.Position);
					if (data == null)
					{
						throw new ArgumentException("Error reading XP");
					}
					_list.Add("playercurrentxp", data);

					data = ReadPlayerModiferPoints(reader, reader.BaseStream.Position);
					if (data == null)
					{
						throw new ArgumentException("Error reading attributes");
					}
					_list.Add("playermodifierpoints", data);

					data = ReadPlayerSkillPoints(reader, reader.BaseStream.Position);
					if (data == null)
					{
						throw new ArgumentException("Error reading skill points");
					}
					_list.Add("playerskillpoints", data);

					var datalist = ReadAttributes(reader, reader.BaseStream.Position);
					if (datalist == null || datalist.Count != 5)
					{
						throw new ArgumentException("Error reading attributes");
					}
					_list.Add("strength", datalist[0]);
					_list.Add("dexterity", datalist[1]);
					_list.Add("intelligence", datalist[2]);
					_list.Add("health", datalist[3]);
					_list.Add("mana", datalist[4]);

					data = ReadPlayerMaxLevel(reader, reader.BaseStream.Position);
					if (data == null)
					{
						throw new ArgumentException("Error reading max level");
					}
					_list.Add("playerMaxLevel", data);
				}
				Validate(playerInfo);
				_validated = true;
			}
		}

		private void Validate(PlayerInfo playerInfo)
		{
			//var data = _list["playerMaxLevel"];
			//if (data.Value4byte != playerInfo.CurrentLevel)
			//{
			//	throw new ArgumentException("Invalid max level");
			//}
			var data = _list["playerLevel"];
			if (data.Value4byte != playerInfo.CurrentLevel)
			{
				throw new ArgumentException("Invalid player level");
			}
			data = _list["playercurrentLevel"];
			if (data.Value4byte != playerInfo.CurrentLevel)
			{
				throw new ArgumentException("Invalid player current level");
			}

			data = _list["playerdifficulty"];
			if (data.Value4byte != playerInfo.DifficultyUnlocked)
			{
				throw new ArgumentException("Invalid player difficulty");
			}

			data = _list["playercurrentxp"];
			if (data.Value4byte != playerInfo.CurrentXP)
			{
				throw new ArgumentException("Invalid player xp");
			}
			data = _list["playermodifierpoints"];
			if (data.Value4byte != playerInfo.AttributesPoints)
			{
				throw new ArgumentException("Invalid player attribute points");
			}
			data = _list["playerskillpoints"];
			if (data.Value4byte != playerInfo.SkillPoints)
			{
				throw new ArgumentException("Invalid player skill points");
			}
			data = _list["strength"];
			if (data.Value4byte != playerInfo.BaseStrength)
			{
				throw new ArgumentException("Invalid player strength");
			}
			data = _list["dexterity"];
			if (data.Value4byte != playerInfo.BaseDexterity)
			{
				throw new ArgumentException("Invalid player dexterity");
			}
			data = _list["intelligence"];
			if (data.Value4byte != playerInfo.BaseIntelligence)
			{
				throw new ArgumentException("Invalid player intelligence");
			}
			data = _list["health"];
			if (data.Value4byte != playerInfo.BaseHealth)
			{
				throw new ArgumentException("Invalid player health");
			}
			data = _list["mana"];
			if (data.Value4byte != playerInfo.BaseMana)
			{
				throw new ArgumentException("Invalid player mana");
			}
		}

	}
}
