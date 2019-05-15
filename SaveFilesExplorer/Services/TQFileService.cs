using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EnumsNET;
using System.Text.RegularExpressions;
using SaveFilesExplorer.Entities;

namespace SaveFilesExplorer.Services
{
	public class TQFileService
	{
		public TQFileRecord[] ReadKeyMap(string path)
		{
			// Regex save file
			var ext = Path.GetExtension(path).ToLower();
			var data = File.ReadAllBytes(path);
			var asString = TQFileRecord.Encoding1252.GetString(data);

			// Where the magic lies
			var keyMatches = Regex.Matches(asString
				, @"(?<Len>.\x00{3})(?<Key>(?<Open>\(\*)?(?<Name>[a-zA-Z0-9_]+)(?<Close>\))?(?<Ext>\[i\])?)"
				, RegexOptions.Singleline)
				.Cast<Match>().Where(m => m.Success).ToList();

			// Determine version
			// TODO
			TQVersion fileVersion = TQVersion.TQITAE_Atlantis;

			#region Remove noise and select record type

			var records = keyMatches.Select(m =>
				{
					TQFileRecord retval = null;
					switch (ext)
					{
						case ".chr":
							retval = new TQFilePlayerRecord(m, fileVersion);
							break;
						case ".dxg":
						case ".dxb":
							retval = new TQFilePlayerRecord(m, fileVersion);
							// Not Yet
							//throw new NotImplementedException();
							break;
						default:
							throw new ArgumentException("must be a file with extension chr, dxb, dxg", nameof(path));
					}
					return retval;
				})
				// Remove all keys that don't match keylen (false match.Success)
				.Where(m => m.KeyLenAsInt == m.Key.Length)
				.ToList();

			#endregion

			// Try read values
			records.ForEach(r => r.ReadValue(data));

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
			for (int cursor = 0; cursor < data.Length; cursor++)
			{
				for (var ii = 0; ii < records.Count; ii++)
				{
					curr = records[ii];

					if (
						curr.DataType == TQFileDataType.Unknown
						&& curr.RegExMatch.Index <= cursor && cursor < curr.ValueStart // byte is part of an unknown key
					)
					{
						TryMakeUnknownSegment(records, orphans, orphansRecords, ii);
						cursor = curr.ValueStart; // Move cursor to farthest known position
						goto skip;
					}
					else if (
						curr.DataType != TQFileDataType.Unknown
						&& curr.RegExMatch.Index <= cursor && cursor <= curr.ValueEnd
					)
					{
						TryMakeUnknownSegment(records, orphans, orphansRecords, ii);
						cursor = curr.ValueEnd + 1; // Move cursor to farthest known position
						goto skip;
					}
				}
				orphans.Add(data[cursor]);
			skip:;
			}
			for (var i = orphansRecords.Count - 1; i >= 0; i--)
			{
				records.Insert(orphansRecords[i].Key, orphansRecords[i].Value);
			}

			#endregion

			string keylist = string.Join(Environment.NewLine, records.Select(r => r.Key).Distinct().ToArray());
			return records.ToArray();
		}

		private static void TryMakeUnknownSegment(List<TQFileRecord> records, List<byte> orphans, List<KeyValuePair<int, TQFileRecord>> orphansRecords, int ii)
		{
			if (orphans.Any())
			{
				var previdx = ii - 1;
				var valueStart = previdx == -1 ? 0 : records[previdx].DataType == TQFileDataType.Unknown ? records[previdx].ValueStart : records[previdx].ValueEnd + 1;
				orphansRecords.Add(new KeyValuePair<int, TQFileRecord>(
					ii, new TQFileRecord()
					{
						Key = TQFileRecord.unknown_segment,
						KeyLenAsInt = TQFileRecord.unknown_segment.Length,
						ValueStart = valueStart,
						ValueEnd = valueStart + orphans.Count,
						DataAsByteArray = orphans.ToArray()
					})
				);

				orphans.Clear();
			}
		}
	}
}
