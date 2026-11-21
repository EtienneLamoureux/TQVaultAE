using TQ.SaveFilesExplorer.Entities.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TQVaultAE.GameFiles;

namespace TQ.SaveFilesExplorer.Entities;

public class TQFileRecord
{
	/// <summary>
	/// Unhandled data
	/// </summary>
	public const string unknown_segment = "Unknown segment";

	/// <summary>
	/// Key match in the file content. <see langword="null"/> for unknown segments.
	/// </summary>
	public Match? RegExMatch { get; set; }

	public string? KeyLength { get; set; }
	public int KeyLengthAsInt { get; set; }

	public string? KeyRaw { get; set; }
	public string KeyName { get; set; } = string.Empty;

	public TQFileDataType DataType { get; set; } = TQFileDataType.Unknown;

	/// <summary>
	/// File the record was parsed from. <see langword="null"/> for unknown segments.
	/// </summary>
	public TQFile? File { get; set; }

	/// <summary>
	/// File the record was parsed from, for the operations that need its content.
	/// </summary>
	protected TQFile AttachedFile
	{
		get => this.File ?? throw new InvalidOperationException($"Record \"{this.KeyName}\" is not attached to a file.");
	}

	public int KeyIndex
	{
		get => RegExMatch is null ? ValueStart : RegExMatch.Index;
	}

	/// <summary>
	/// Offset of the last byte of the key, or of the value for the records without key (<see cref="IsKeyValue"/>).
	/// </summary>
	public int KeyEnd
	{
		get => this.IsKeyValue ? this.ValueEnd : this.ValueStart - 1;
	}

	public int ValueStart { get; set; }
	public int ValueEnd { get; set; }

	public string? DataAsStr { get; set; }
	public byte[] DataAsByteArray { get; set; } = [];
	public int? DataAsInt { get; set; }
	public float? DataAsFloat { get; set; }
	public bool IsSubStructureOpening { get => this.KeyName == TQFilePlayerRecordKey.begin_block.ToString(); }
	public bool IsStructureClosing { get => this.KeyName == TQFilePlayerRecordKey.end_block.ToString(); }
	public bool IsUnknownSegment { get => this.KeyName == unknown_segment; }
	public bool HasError { get => this.DataType == TQFileDataType.Unknown || this.IsUnknownSegment; }
	public bool IsDataTypeError { get => this.DataType == TQFileDataType.Unknown && !this.IsUnknownSegment; }
	public TQFileRecord? Parent { get; internal set; }
	public List<TQFileRecord> Childs { get; internal set; } = [];
	/// <summary>
	/// Indicate that the key is irrelevant as data. Only data have a meaning.
	/// </summary>
	public bool IsKeyValue { get; internal set; } = false;

	public TQFileRecord()
	{ }

	public TQFileRecord(TQFile file, Match m)
	{
		this.RegExMatch = m;
		this.File = file;

		this.KeyLength = m.Groups["Len"].Value;
		this.KeyName = m.Groups["Key"].Value;
		this.KeyRaw = m.Groups["Key"].Value;

		var barray = TextEncodings.Cp1252.GetBytes(this.KeyLength.ToArray());
		this.KeyLengthAsInt = BitConverter.ToInt32(barray, 0);

		if (this.KeyLengthAsInt < this.KeyName.Length)
		{
			// Cut remaining chars
			this.KeyName = this.KeyName.Substring(0, this.KeyLengthAsInt);
		}

		this.ValueStart = this.RegExMatch.Index + sizeof(int) + this.KeyLengthAsInt;
	}

	/// <summary>
	/// Read the value of this key accordingly to the datatype defined for the version of this file
	/// </summary>
	public virtual void ReadValue()
	{
		int len = 0;
		this.ValueEnd = 0;
		switch (this.DataType)
		{
			case TQFileDataType.Int:
				ValueEnd = ValueStart + sizeof(int) - 1; // -1 because ValueStart is first relevant byte
				DataAsByteArray = new ArraySegment<byte>(this.AttachedFile.Content, ValueStart, sizeof(int)).ToArray();
				DataAsInt = BitConverter.ToInt32(DataAsByteArray, 0);
				DataAsFloat = BitConverter.ToSingle(DataAsByteArray, 0);
				break;
			case TQFileDataType.Float:
				ValueEnd = ValueStart + sizeof(float) - 1;
				DataAsByteArray = new ArraySegment<byte>(this.AttachedFile.Content, ValueStart, sizeof(float)).ToArray();
				DataAsInt = BitConverter.ToInt32(DataAsByteArray, 0);
				DataAsFloat = BitConverter.ToSingle(DataAsByteArray, 0);
				break;
			case TQFileDataType.String1252:
				// Read StrLen
				len = BitConverter.ToInt32(new ArraySegment<byte>(this.AttachedFile.Content, ValueStart, sizeof(int)).ToArray(), 0);
				// Read Str
				ValueEnd = ValueStart + sizeof(int) - 1 + len;
				DataAsByteArray = new ArraySegment<byte>(this.AttachedFile.Content, ValueStart + sizeof(int), len).ToArray();
				DataAsStr = TextEncodings.Cp1252.GetString(DataAsByteArray);
				break;
			case TQFileDataType.StringUTF16:
				// Read StrLen
				len = BitConverter.ToInt32(new ArraySegment<byte>(this.AttachedFile.Content, ValueStart, sizeof(int)).ToArray(), 0);
				// Read Str
				ValueEnd = ValueStart + sizeof(int) - 1 + (len * 2);
				DataAsByteArray = new ArraySegment<byte>(this.AttachedFile.Content, ValueStart + sizeof(int), len * 2).ToArray();// * 2 because UTF16 has 2 byte encoding
				DataAsStr = Encoding.Unicode.GetString(DataAsByteArray);
				break;
			case TQFileDataType.ByteArrayVar:
				// Read Len
				len = BitConverter.ToInt32(new ArraySegment<byte>(this.AttachedFile.Content, ValueStart, sizeof(int)).ToArray(), 0);
				// Read bytes
				ValueEnd = ValueStart + sizeof(int) - 1 + len;
				DataAsByteArray = new ArraySegment<byte>(this.AttachedFile.Content, ValueStart + sizeof(int), len).ToArray();
				break;
			case TQFileDataType.ByteArray16:
				// Read bytes
				ValueEnd = ValueStart + 16 - 1;
				DataAsByteArray = new ArraySegment<byte>(this.AttachedFile.Content, ValueStart, 16).ToArray();
				break;
		}
	}

	/// <summary>
	/// Define the datatype of this record
	/// </summary>
	public virtual void DefineDataType()
	{
		throw new NotImplementedException();
	}

	internal string GetDataAsString(bool _DisplayDataDecimal)
	{
		string data = string.Empty;
		switch (this.DataType)
		{
			case TQFileDataType.Int:
				data = _DisplayDataDecimal ? $"{this.DataAsInt}" : $"{this.DataAsInt:X8}";
				break;
			case TQFileDataType.Float:
				data = _DisplayDataDecimal ? $"{this.DataAsFloat}" : $"{this.DataAsInt:X8}";
				break;
			case TQFileDataType.String1252:
			case TQFileDataType.StringUTF16:
				data = this.DataAsStr ?? string.Empty;
				break;
			case TQFileDataType.ByteArrayVar:
			case TQFileDataType.ByteArray16:
			case TQFileDataType.Unknown:
				data = string.Join(" ", this.DataAsByteArray.Select(b => _DisplayDataDecimal ? b.ToString() : b.ToString("X2")));
				break;
		}
		return data;
	}
}
