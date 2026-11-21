using TQVaultAE.Application.Contracts.Services;

namespace TQVaultAE.Data;

/// <summary>
/// The key/value pairs of the save files: a key (length-prefixed CP1252 string) followed by its value.
/// </summary>
internal static class SaveKeys
{
	/// <summary>Writes a key and its Int32 value.</summary>
	public static void WriteKeyInt(this ITQDataService tqData, BinaryWriter writer, string key, int value)
	{
		tqData.WriteCString(writer, key);
		writer.Write(value);
	}

	/// <summary>Writes a key and its Single value.</summary>
	public static void WriteKeyFloat(this ITQDataService tqData, BinaryWriter writer, string key, float value)
	{
		tqData.WriteCString(writer, key);
		writer.Write(value);
	}

	/// <summary>Writes a key and its string value (length-prefixed CP1252).</summary>
	public static void WriteKeyString(this ITQDataService tqData, BinaryWriter writer, string key, string value)
	{
		tqData.WriteCString(writer, key);
		tqData.WriteCString(writer, value);
	}

	/// <summary>Reads the Int32 value of a key, after checking the key (an <see cref="ArgumentException"/> when it differs).</summary>
	public static int ReadKeyInt(this ITQDataService tqData, BinaryReader reader, string key)
	{
		tqData.ValidateNextString(key, reader);
		return reader.ReadInt32();
	}

	/// <summary>Reads the Single value of a key, after checking the key (an <see cref="ArgumentException"/> when it differs).</summary>
	public static float ReadKeyFloat(this ITQDataService tqData, BinaryReader reader, string key)
	{
		tqData.ValidateNextString(key, reader);
		return reader.ReadSingle();
	}

	/// <summary>Reads the string value of a key, after checking the key (an <see cref="ArgumentException"/> when it differs).</summary>
	public static string ReadKeyString(this ITQDataService tqData, BinaryReader reader, string key)
	{
		tqData.ValidateNextString(key, reader);
		return tqData.ReadCString(reader);
	}
}
