using TQ.SaveFilesExplorer.Entities.Players;
using TQ.SaveFilesExplorer.Entities.TransferStash;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GameFiles;

namespace TQ.SaveFilesExplorer.Entities;

public partial class TQFile
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
	public static string[] Ext_All { get => [Ext_Player, Ext_SharedStashBackup, Ext_SharedStash]; }

	/// <summary>
	/// Tells whether the file has the extension of a TQ save file, in any case.
	/// </summary>
	public static bool IsSaveFile(string path)
		=> Ext_All.Contains(System.IO.Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);


	public byte[] Content { get; private set; }
	public string Path { get; private set; }
	public string Ext { get; private set; }
	public TQFileRecord[] Records { get; private set; } = [];
	public TQFileRecord[] Childs { get; private set; } = [];

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

			return this._Version ?? throw new InvalidOperationException($"Unknown version for file \"{this.Path}\".");
		}
	}

	public bool IsStashFile
	{
		get => this.Ext.Equals(Ext_SharedStash, StringComparison.OrdinalIgnoreCase)
			|| this.Ext.Equals(Ext_SharedStashBackup, StringComparison.OrdinalIgnoreCase);
	}

	public bool IsPlayerFile
	{
		get => this.Ext.Equals(Ext_Player, StringComparison.OrdinalIgnoreCase);
	}
	private TQFile(string path, string ext, byte[] content)
	{
		this.Path = path;
		this.Ext = ext;
		this.Content = content;
	}

	public static TQFile ReadFile(string path)
	{
		return new TQFile(
			path,
			System.IO.Path.GetExtension(path).ToLowerInvariant(),
			File.ReadAllBytes(path)
		);
	}

	[GeneratedRegex(@"
(?<Len>.\x00{3})
(?<Key>
	(?<Open>\(\*)?
		(?<Name>[a-zA-Z0-9_\.]+)
			(?<Close>\))?
			(?<Ext>\[i\])?
)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline)]
	private static partial Regex RegexKeyMatch();

	/// <summary>
	/// Parse file data to raw records
	/// </summary>
	public void Parse()
	{
		var asString = TextEncodings.Cp1252.GetString(this.Content);

		// Regex save file / Where the magic lies
		var keyMatches = RegexKeyMatch().Matches(asString)
			.Cast<Match>().Where(m => m.Success).ToList();

		this.Records = keyMatches.Select(m => new TQFileRecord(this, m))
			// Remove all keys that don't match keylen (false match.Success)
			.Where(m => m.KeyLengthAsInt == m.KeyName.Length)
			.ToArray();
	}

	private void FindFileVersion()
	{
		if (_Version.HasValue) return;
		// Determine version
		switch (this.Ext)
		{
			case Ext_Player:
				var headerVersionKey = this.Records.FirstOrDefault(k => k.KeyName == TQFilePlayerRecordKey.headerVersion.ToString())
					?? throw new InvalidDataException($"No {TQFilePlayerRecordKey.headerVersion} key in file \"{this.Path}\".");
				var fileVersionValue = (PlayerFileHeaderVersion)BitConverter.ToInt32(new ArraySegment<byte>(this.Content, headerVersionKey.ValueStart, sizeof(int)).ToArray(), 0);
				_Version = fileVersionValue switch
				{
					PlayerFileHeaderVersion.TQ => TQVersion.TQ,
					PlayerFileHeaderVersion.TQIT => TQVersion.TQIT,
					PlayerFileHeaderVersion.TQAE => TQVersion.TQAE,
					_ => null,
				};
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
			TQFileRecord retval;
			switch (this.Ext)
			{
				case Ext_Player:
					retval = MapTo<TQFilePlayerRecord>(m);
					break;
				case Ext_SharedStash:
				case Ext_SharedStashBackup:
					retval = MapTo<TQFilePlayerTransferStashRecord>(m);
					break;
				default:
					throw new ArgumentException("must be a file with extension chr, dxb, dxg");
			}
			return retval;
		})
		.ToList();

		// Try read values
		foreach (var r in records) r.ReadValue();

		#endregion

		#region Remove all string keys that colide with values of another. Disambiguation of AnsiString values and keys

		List<TQFileRecord> falseKeys = [];
		for (int i = 1; i < records.Count; i++)
		{
			var falsekey = records[i];
			for (int ii = 0; ii < i; ii++)
			{
				if (falseKeys.Contains(records[ii]))
					continue;// Je passe sur les éléments déja écartés

				var legitkey = records[ii];
				if (falsekey.KeyIndex == legitkey.ValueStart)
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
		List<byte> orphans = [];
		List<KeyValuePair<int, TQFileRecord>> orphansRecords = [];
		for (int cursor = 0; cursor < this.Content.Length; cursor++)
		{
			for (var ii = 0; ii < records.Count; ii++)
			{
				var curr = records[ii];

				if (
					curr.DataType == TQFileDataType.Unknown
					&& curr.KeyIndex <= cursor && cursor < curr.ValueStart // byte is part of an unknown key
				)
				{
					MakeUnknownSegment(records, orphans, orphansRecords, ii);
					cursor = curr.ValueStart - 1; // Move cursor to farthest known position (-1 to compensate cursor++)
					goto skip;
				}
				else if (
					curr.DataType != TQFileDataType.Unknown
					&& curr.KeyIndex <= cursor && cursor <= curr.ValueEnd
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
			if (crc is not null && crc.IsUnknownSegment)// Should be true
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
		if (orphans.Count != 0)
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


	private (List<TQFileRecord> nodes, int newidx) MakeTreeRecords(TQFileRecord? parent = null, int idx = 0)
	{
		List<TQFileRecord> currentLvl = [];
		for (; idx < this.Records.Length; idx++)
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

	/// <summary>
	/// Copies a raw record into the record type of the file (<see cref="TQFilePlayerRecord"/>,
	/// <see cref="TQFilePlayerTransferStashRecord"/>).
	/// </summary>
	private static T MapTo<T>(TQFileRecord source) where T : TQFileRecord, new()
	{
		return new T
		{
			RegExMatch = source.RegExMatch,
			KeyLength = source.KeyLength,
			KeyLengthAsInt = source.KeyLengthAsInt,
			KeyRaw = source.KeyRaw,
			KeyName = source.KeyName,
			DataType = source.DataType,
			File = source.File,
			ValueStart = source.ValueStart,
			ValueEnd = source.ValueEnd,
			DataAsStr = source.DataAsStr,
			DataAsByteArray = source.DataAsByteArray,
			DataAsInt = source.DataAsInt,
			DataAsFloat = source.DataAsFloat
		};
	}
}
