using AutoMapper;
using TQ.SaveFilesExplorer.Entities.Players;
using TQ.SaveFilesExplorer.Entities.TransferStash;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TQ.SaveFilesExplorer.Entities
{
	public class TQFile
	{
		public const string Ext_Player = ".chr";
		/// <summary>
		/// the character's private stash.
		/// </summary>
		public const string Ext_SharedStash = ".dxb";
		/// <summary>
		/// the character's private stash backup.
		/// </summary>
		public const string Ext_SharedStashBackup = ".dxg";
		/// <summary>
		/// List of TQ save file extensions
		/// </summary>
		public static string[] Ext_All { get => new string[] { Ext_Player, Ext_SharedStashBackup, Ext_SharedStash }; }


		public byte[] Content { get; private set; }
		public string Path { get; private set; }
		public string Ext { get; private set; }
		public TQFileRecord[] Records { get; private set; }
		public TQFileRecord[] Childs { get; private set; }

		private TQVersion? _Version = null;
		public TQVersion Version
		{
			set
			{
				_Version = value;
			}
			get
			{
				// Define version by analysing records
				FindFileVersion();

				return _Version.Value;
			}
		}

		public bool IsStashFile
		{
			get => this.Ext.Equals(Ext_SharedStash, StringComparison.InvariantCultureIgnoreCase)
				|| this.Ext.Equals(Ext_SharedStashBackup, StringComparison.InvariantCultureIgnoreCase);
		}

		public bool IsPlayerFile
		{
			get => this.Ext.Equals(Ext_Player, StringComparison.InvariantCultureIgnoreCase);
		}

		private TQFile()
		{ }

		public static TQFile ReadFile(string path)
		{
			return new TQFile()
			{
				Path = path,
				Ext = System.IO.Path.GetExtension(path).ToLower(),
				Content = File.ReadAllBytes(path),
			};
		}

		/// <summary>
		/// Parse file data to raw records
		/// </summary>
		public void Parse()
		{
			var asString = TQFileRecord.Encoding1252.GetString(this.Content);

			// Regex save file / Where the magic lies
			var keyMatches = Regex.Matches(asString
				, Properties.Settings.Default.RegexKeyMatch
				, RegexOptions.Singleline)
				.Cast<Match>().Where(m => m.Success).ToList();

			this.Records = keyMatches.Select(m => new TQFileRecord(this, m))
				// Remove all keys that don't match keylen (false match.Success)
				.Where(m => m.KeyLengthAsInt == m.KeyName.Length)
				.ToArray();
		}

		private const int PLAYER_HEADERVERSION_VALUE_TQ = 1;
		private const int PLAYER_HEADERVERSION_VALUE_TQIT = 2;
		private const int PLAYER_HEADERVERSION_VALUE_TQAE = 3;

		private void FindFileVersion()
		{
			if (_Version.HasValue) return;
			// Determine version
			switch (this.Ext)
			{
				case Ext_Player:
					var headerVersionKey = this.Records.FirstOrDefault(k => k.KeyName == TQFilePlayerRecordKey.headerVersion.ToString());
					var fileVersionValue = BitConverter.ToInt32(new ArraySegment<byte>(this.Content, headerVersionKey.ValueStart, sizeof(int)).ToArray(), 0);
					if (fileVersionValue == PLAYER_HEADERVERSION_VALUE_TQ)
						_Version = TQVersion.TQ;
					else if (fileVersionValue == PLAYER_HEADERVERSION_VALUE_TQIT)
						_Version = TQVersion.TQIT;
					else if (fileVersionValue == PLAYER_HEADERVERSION_VALUE_TQAE)
						_Version = TQVersion.TQAE;
					break;
				case Ext_SharedStash:
				case Ext_SharedStashBackup:
					// Is there any hint by analysing file content ?
					_Version = TQVersion.TQAE;
					break;
				default:
					throw new ArgumentException("must be a file with extension chr, dxb, dxg");
			}
		}


		/// <summary>
		/// Analyse Parsed Records to produce fine detailed informations
		/// </summary>
		public void Analyse()
		{
			FindFileVersion();

			#region select record type

			var records = this.Records
			.Select(m =>
			{
				TQFileRecord retval = null;
				switch (this.Ext)
				{
					case Ext_Player:
						retval = Mapper.Map<TQFilePlayerRecord>(m);
						break;
					case Ext_SharedStash:
					case Ext_SharedStashBackup:
						retval = Mapper.Map<TQFilePlayerTransferStashRecord>(m);
						break;
					default:
						throw new ArgumentException("must be a file with extension chr, dxb, dxg");
				}
				return retval;
			})
			.ToList();

			// Try read values
			records.ForEach(r => r.ReadValue());

			#endregion

			#region Remove all string keys that colide with values of another. Disambiguation of AnsiString values and keys

			List<TQFileRecord> falseKeys = new List<TQFileRecord>();
			for (int i = 1; i < records.Count; i++)
			{
				var falsekey = records[i];
				for (int ii = 0; ii < i; ii++)
				{
					if (falseKeys.Contains(records[ii]))
						continue;// Je passe sur les éléments déja écartés

					var legitkey = records[ii];
					if (falsekey.RegExMatch.Index == legitkey.ValueStart)
					{
						falseKeys.Add(falsekey);
						break;
					}
				}
			}
			// Cleanup
			var rem = records.RemoveAll(r => falseKeys.Contains(r));

			#endregion

			// Reveal unknown keys based on enumlist
			// Already done ! They don't have known DataType

			#region Reveal unknown segments based on indexes gap

			// check if some bytes are not include in records
			List<byte> orphans = new List<byte>();
			List<KeyValuePair<int, TQFileRecord>> orphansRecords = new List<KeyValuePair<int, TQFileRecord>>();
			TQFileRecord curr = null;
			for (int cursor = 0; cursor < this.Content.Length; cursor++)
			{
				for (var ii = 0; ii < records.Count; ii++)
				{
					curr = records[ii];

					if (
						curr.DataType == TQFileDataType.Unknown
						&& curr.RegExMatch.Index <= cursor && cursor < curr.ValueStart // byte is part of an unknown key
					)
					{
						MakeUnknownSegment(records, orphans, orphansRecords, ii);
						cursor = curr.ValueStart - 1; // Move cursor to farthest known position (-1 to compensate cursor++)
						goto skip;
					}
					else if (
						curr.DataType != TQFileDataType.Unknown
						&& curr.RegExMatch.Index <= cursor && cursor <= curr.ValueEnd
					)
					{
						MakeUnknownSegment(records, orphans, orphansRecords, ii);
						cursor = curr.ValueEnd; // Move cursor to farthest known position
						goto skip;
					}
				}
				orphans.Add(this.Content[cursor]);
			skip:;
			}

			MakeUnknownSegment(records, orphans, orphansRecords, records.Count);// In cas there there is unknonwn trailing bytes

			for (var i = orphansRecords.Count - 1; i >= 0; i--)
			{
				records.Insert(orphansRecords[i].Key, orphansRecords[i].Value);
			}

			// Legitimate CRC on StashFiles
			if (this.IsStashFile)
			{
				var crc = records.FirstOrDefault();
				if (crc != null && crc.IsUnknownSegment)// Should be true
				{
					crc.DataType = TQFileDataType.Int;
					crc.DataAsInt = BitConverter.ToInt32(crc.DataAsByteArray, 0);
					crc.KeyName = TQFilePlayerTransferStashKey.CRC.ToString();
					crc.KeyLengthAsInt = crc.KeyName.Length;
					crc.IsKeyValue = true;
				}
			}

			#endregion

			this.Records = records.ToArray();

			this.Childs = MakeTreeRecords().nodes.ToArray();
		}

		private static void MakeUnknownSegment(List<TQFileRecord> records, List<byte> orphans, List<KeyValuePair<int, TQFileRecord>> orphansRecords, int ii)
		{
			if (orphans.Any())
			{
				var previdx = ii - 1;
				var valueStart = previdx == -1 ? 0 : records[previdx].DataType == TQFileDataType.Unknown ? records[previdx].ValueStart : records[previdx].ValueEnd + 1;
				orphansRecords.Add(new KeyValuePair<int, TQFileRecord>(
					ii, new TQFileRecord()
					{
						KeyName = TQFileRecord.unknown_segment,
						KeyLengthAsInt = TQFileRecord.unknown_segment.Length,
						ValueStart = valueStart,
						ValueEnd = valueStart + orphans.Count - 1,
						DataAsByteArray = orphans.ToArray(),
						IsKeyValue = true,
					})
				);

				orphans.Clear();
			}
		}


		private (List<TQFileRecord> nodes, int newidx) MakeTreeRecords(TQFileRecord parent = null, int idx = 0)
		{
			List<TQFileRecord> currentLvl = new List<TQFileRecord>();
			for (; idx < this.Records.Count(); idx++)
			{
				var k = this.Records[idx];
				k.Parent = parent;
				if (k.IsSubStructureOpening)
				{
					var nextLvl = MakeTreeRecords(k, idx + 1);
					idx = nextLvl.newidx;
					k.Childs.AddRange(nextLvl.nodes.ToArray());
				}

				currentLvl.Add(k);
				if (k.IsStructureClosing) break;
			}
			return (currentLvl, idx);
		}
	}
}
