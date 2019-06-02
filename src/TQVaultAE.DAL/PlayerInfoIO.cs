using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TQVaultData
{
	public class PlayerInfoIO
	{

		static byte[] _playerLevelKey = new byte[] {0x00, 0x00, 0x00, 0x70, 0x6C, 0x61, 0x79, 0x65, 0x72, 0x4C, 0x65, 0x76, 0x65, 0x6C };

		static byte[] _charLevelKey = new byte[] {
			0x00, 0x00, 0x00, 0x63, 0x75, 0x72, 0x72, 0x65, 0x6e, 0x74, 0x53,
			0x74, 0x61, 0x74, 0x73, 0x2e, 0x63, 0x68, 0x61, 0x72, 0x4c, 0x65,
			0x76, 0x65, 0x6c };

		static byte[] _playerMaxLevel = new byte[] { 0x00, 0x00, 0x00, 0x6d, 0x61, 0x78, 0x4c, 0x65, 0x76, 0x65, 0x6c };

		static byte[] _tempKey = new byte[] { 0x00, 0x00, 0x00, 0x74, 0x65, 0x6D, 0x70 };

		static byte[] _moneyKey = new byte[] { 0x00, 0x00, 0x00, 0x6d, 0x6f, 0x6e, 0x65, 0x79 };

		static byte[] _experiencePointsKey = new byte[] {
			0x00, 0x00, 0x00, 0x63, 0x75, 0x72, 0x72, 0x65, 0x6e, 0x74,
			0x53, 0x74, 0x61, 0x74, 0x73, 0x2e, 0x65, 0x78, 0x70, 0x65,
			0x72, 0x69, 0x65, 0x6e, 0x63, 0x65, 0x50, 0x6f, 0x69, 0x6e, 0x74, 0x73 };

		static byte[] _modifierPointsKey = new byte[] {
			0x00, 0x00, 0x00, 0x6d, 0x6f, 0x64, 0x69, 0x66, 0x69, 0x65, 0x72, 0x50, 0x6f, 0x69, 0x6e, 0x74, 0x73 };

		static byte[] _skillPointsKey = new byte[] {
			0x00, 0x00, 0x00, 0x73, 0x6b, 0x69, 0x6c, 0x6c, 0x50, 0x6f, 0x69, 0x6e, 0x74, 0x73 };

		static byte[] _attributeKey = new byte[] {
			0x00, 0x00, 0x00, 0x62, 0x65, 0x67, 0x69, 0x6e, 0x5f, 0x62, 0x6c,
			0x6f, 0x63, 0x6b, 0xce, 0xfa, 0x1d, 0xb0, 0x04, 0x00, 0x00, 0x00, 0x74,
			0x65, 0x6d, 0x70 };

		static byte[] _statsKey = new byte[] {
			0x00, 0x00, 0x00, 0x62, 0x65, 0x67, 0x69, 0x6e, 0x5f, 0x62, 0x6c,
			0x6f, 0x63, 0x6b, 0xce, 0xfa, 0x1d, 0xb0, 0x11, 0x00, 0x00, 0x00, 0x70,
			0x6c, 0x61, 0x79, 0x54, 0x69, 0x6d, 0x65, 0x49, 0x6e, 0x53, 0x65, 0x63,
			0x6f, 0x6e, 0x64, 0x73 };

		private PlayerInfoKeyPair ReadOffsets(BinaryReader reader, long startOffset, byte[] key, byte keyStart)
		{
			var offset = startOffset>0? startOffset:0;
			var keyData = new PlayerInfoKeyPair();
			keyData.KeyNameLength = keyStart;
			keyData.KeyId = key;
			reader.BaseStream.Seek(offset, SeekOrigin.Begin);
			try
			{
				while (reader.BaseStream.Position < reader.BaseStream.Length)
				{
					keyData.KeyOffset = reader.BaseStream.Position;
					var b = reader.ReadByte();
					var backOf = reader.BaseStream.Position;
					if (b == keyStart)
					{
						var scan = reader.ReadBytes(key.Length);
						if (scan.SequenceEqual(key))
						{
							keyData.ValueOffset = reader.BaseStream.Position;
							return (keyData);
						}
						reader.BaseStream.Seek(backOf, SeekOrigin.Begin);
					}
				}
			}
			catch
			{

			}
			return (null);
		}


		protected void WriteKeyValue(BinaryWriter writer, PlayerInfoKeyPair keyPair)
		{
			writer.BaseStream.Seek(keyPair.ValueOffset, SeekOrigin.Begin);
			if (typeof(Single)== keyPair.Type)
			{
				writer.Write(Convert.ToSingle(keyPair.Value4byte));
			}
			else if (typeof(Int32) == keyPair.Type)
			{
				writer.Write(keyPair.Value4byte);
			}
			else
			{
				writer.Write(keyPair.Value4byte);
			}
		}

		private static byte KeyLength(byte[] key)
		{
			return ((byte)(key.Length - 3));
		}

		protected PlayerInfoKeyPair ReadPlayerCurrentLevel(BinaryReader reader, long offset)
		{
			//var keyData = ReadOffsets(reader, 0, _charLevelKey, 0x16);
			var keyData = ReadOffsets(reader, offset > 0 ? offset : 0, _charLevelKey, KeyLength(_charLevelKey));
			if (keyData != null)
			{
				keyData.Value4byte = reader.ReadInt32();
				keyData.Type = typeof(Int32);
				return (keyData);
			}
			return (null);
		}

		protected PlayerInfoKeyPair ReadPlayerLevel(BinaryReader reader)
		{
			var keyData = ReadOffsets(reader, 0, _playerLevelKey, KeyLength(_playerLevelKey));
			if (keyData != null)
			{
				keyData.Value4byte = reader.ReadInt32();
				keyData.Type = typeof(Int32);
				return (keyData);
			}
			return (null);
		}

		protected PlayerInfoKeyPair ReadPlayerMaxLevel(BinaryReader reader, long offset)
		{
			var keyData = ReadOffsets(reader, offset > 0 ? offset : 0, _playerMaxLevel, KeyLength(_playerMaxLevel));
			if (keyData != null)
			{
				keyData.Value4byte = reader.ReadInt32();
				keyData.Type = typeof(Int32);
				return (keyData);
			}
			return (null);
		}

		protected PlayerInfoKeyPair ReadPlayerExperience(BinaryReader reader, long offset)
		{
			var keyData = ReadOffsets(reader, offset > 0 ? offset : 0, _experiencePointsKey, KeyLength(_experiencePointsKey));
			if (keyData != null)
			{
				keyData.Value4byte = reader.ReadInt32();
				keyData.Type = typeof(Int32);
				return (keyData);
			}
			return (null);
		}

		protected PlayerInfoKeyPair ReadPlayerModiferPoints(BinaryReader reader, long offset)
		{
			var keyData = ReadOffsets(reader, offset > 0 ? offset : 0, _modifierPointsKey, KeyLength(_modifierPointsKey));
			if (keyData != null)
			{
				keyData.Value4byte = reader.ReadInt32();
				keyData.Type = typeof(Int32);
				return (keyData);
			}
			return (null);
		}

		protected PlayerInfoKeyPair ReadPlayerSkillPoints(BinaryReader reader, long offset)
		{
			var keyData = ReadOffsets(reader, offset>0? offset: 0, _skillPointsKey, KeyLength(_skillPointsKey));
			if (keyData != null)
			{
				keyData.Value4byte = reader.ReadInt32();
				keyData.Type = typeof(Int32);
				return (keyData);
			}
			return (null);
		}

		protected PlayerInfoKeyPair ReadPlayerMoney(BinaryReader reader, long offset)
		{
			var keyData = ReadOffsets(reader, offset > 0 ? offset : 0, _moneyKey, KeyLength(_moneyKey));
			if (keyData != null)
			{
				keyData.Value4byte = reader.ReadInt32();
				keyData.Type = typeof(Int32);
				return (keyData);
			}
			return (null);
		}


		protected IList<PlayerInfoKeyPair> ReadAttributes(BinaryReader reader, long offset)
		{
			var list = new List<PlayerInfoKeyPair>();

			var keyData = ReadOffsets(reader, offset > 0 ? offset : 0, _attributeKey, 0x0b);
			if (keyData != null)
			{
				//strength
				keyData.Value4byte = Convert.ToInt32(reader.ReadSingle());
				keyData.Type = typeof(Single);
				list.Add(keyData);

				keyData = ReadOffsets(reader, reader.BaseStream.Position, _tempKey, KeyLength(_tempKey));
				if (keyData != null)
				{
					//dexterity
					keyData.Value4byte = Convert.ToInt32(reader.ReadSingle());
					keyData.Type = typeof(Single);
					list.Add(keyData);

					keyData = ReadOffsets(reader, reader.BaseStream.Position, _tempKey, KeyLength(_tempKey));
					if (keyData != null)
					{
						//Intelligence
						keyData.Value4byte = Convert.ToInt32(reader.ReadSingle());
						keyData.Type = typeof(Single);
						list.Add(keyData);

						keyData = ReadOffsets(reader, reader.BaseStream.Position, _tempKey, KeyLength(_tempKey));
						if (keyData != null)
						{
							//health
							keyData.Value4byte = Convert.ToInt32(reader.ReadSingle());
							keyData.Type = typeof(Single);
							list.Add(keyData);

							keyData = ReadOffsets(reader, reader.BaseStream.Position, _tempKey, KeyLength(_tempKey));
							if (keyData != null)
							{
								//mana
								keyData.Value4byte = Convert.ToInt32(reader.ReadSingle());
								keyData.Type = typeof(Single);
								list.Add(keyData);
							}
						}
					}
				}
			}


			return (list);
		}


		protected PlayerInfoKeyPair ReadDiffcultyLevel(BinaryReader reader, long offset)
		{
			var keyData = ReadOffsets(reader, offset > 0 ? offset : 0, _tempKey, KeyLength(_tempKey));
			if (keyData != null)
			{
				keyData.Value4byte = reader.ReadInt32();
				keyData.Type = typeof(Int32);
				return (keyData);
			}
			return (null);
		}

		protected int ReadDifficultyUnlockedValue(BinaryReader reader)
		{
			var keyData = ReadDiffcultyLevel(reader,0);
			if (keyData!=null)
			{
				return (keyData.Value4byte);
			}
			return (0);
		}

		protected int ReadMoneyValue(BinaryReader reader)
		{
			var keyData = ReadPlayerMoney(reader, 0);
			if (keyData != null)
			{
				return (keyData.Value4byte);
			}
			return (0);
		}


	}
}
