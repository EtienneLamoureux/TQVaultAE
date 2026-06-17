using System.Buffers.Binary;
using System.Text;

namespace TQVaultAE.Domain.Entities;

public class SkillRecord
{
	/// <summary>
	/// Lazy-initialized CP1252 encoding for Titan Quest file parsing.
	/// Encoding.RegisterProvider is idempotent - safe to call multiple times.
	/// </summary>
	private static readonly Lazy<Encoding> _encoding1252 = new(static () =>
	{
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		return Encoding.GetEncoding(1252);
	});

	/// <summary>
	/// Gets the CP1252 (Windows-1252) encoding used for TQ file parsing.
	/// </summary>
	internal static Encoding Encoding1252 => _encoding1252.Value;

	public string skillName { get; set; }
	public int skillLevel { get; set; }
	public int skillEnabled { get; set; }
	public int skillSubLevel { get; set; }
	public int skillActive { get; set; }
	public int skillTransition { get; set; }

	/// <summary>
	/// Binary serialize
	/// </summary>
	/// <returns></returns>
	public byte[] ToBinary(int beginBlockValue, int endBlockValue)
	{
		// Calculate lengths (character counts, NOT byte counts - the old implementation uses character length)
		int skillNameLen = skillName.Length;
		int beginBlockLen = "begin_block".Length;
		int endBlockLen = "end_block".Length;
		int skillNameKeyLen = nameof(skillName).Length;
		int skillLevelKeyLen = nameof(skillLevel).Length;
		int skillEnabledKeyLen = nameof(skillEnabled).Length;
		int skillSubLevelKeyLen = nameof(skillSubLevel).Length;
		int skillActiveKeyLen = nameof(skillActive).Length;
		int skillTransitionKeyLen = nameof(skillTransition).Length;

		// Each field: int(length) + string bytes + int(value for non-string fields)
		// For string fields: int(keyLen) + key bytes + int(valueLen) + value bytes
		// For int fields: int(keyLen) + key bytes + int(value)
		int totalSize =
			sizeof(int) + beginBlockLen + sizeof(int) +
			sizeof(int) + skillNameKeyLen + sizeof(int) + skillNameLen +
			sizeof(int) + skillLevelKeyLen + sizeof(int) +
			sizeof(int) + skillEnabledKeyLen + sizeof(int) +
			sizeof(int) + skillSubLevelKeyLen + sizeof(int) +
			sizeof(int) + skillActiveKeyLen + sizeof(int) +
			sizeof(int) + skillTransitionKeyLen + sizeof(int) +
			sizeof(int) + endBlockLen + sizeof(int);

		var result = new byte[totalSize];
		var span = result.AsSpan();
		int offset = 0;

		// begin_block
		WriteInt32(ref span, ref offset, "begin_block".Length);
		WriteString(ref span, ref offset, "begin_block");
		WriteInt32(ref span, ref offset, beginBlockValue);

		// skillName
		WriteInt32(ref span, ref offset, nameof(skillName).Length);
		WriteString(ref span, ref offset, nameof(skillName));
		WriteInt32(ref span, ref offset, skillName.Length);
		WriteString(ref span, ref offset, skillName);

		// skillLevel
		WriteInt32(ref span, ref offset, nameof(skillLevel).Length);
		WriteString(ref span, ref offset, nameof(skillLevel));
		WriteInt32(ref span, ref offset, skillLevel);

		// skillEnabled
		WriteInt32(ref span, ref offset, nameof(skillEnabled).Length);
		WriteString(ref span, ref offset, nameof(skillEnabled));
		WriteInt32(ref span, ref offset, skillEnabled);

		// skillSubLevel
		WriteInt32(ref span, ref offset, nameof(skillSubLevel).Length);
		WriteString(ref span, ref offset, nameof(skillSubLevel));
		WriteInt32(ref span, ref offset, skillSubLevel);

		// skillActive
		WriteInt32(ref span, ref offset, nameof(skillActive).Length);
		WriteString(ref span, ref offset, nameof(skillActive));
		WriteInt32(ref span, ref offset, skillActive);

		// skillTransition
		WriteInt32(ref span, ref offset, nameof(skillTransition).Length);
		WriteString(ref span, ref offset, nameof(skillTransition));
		WriteInt32(ref span, ref offset, skillTransition);

		// end_block
		WriteInt32(ref span, ref offset, "end_block".Length);
		WriteString(ref span, ref offset, "end_block");
		WriteInt32(ref span, ref offset, endBlockValue);

		return result;
	}

	private static void WriteInt32(ref Span<byte> span, ref int offset, int value)
	{
		BinaryPrimitives.WriteInt32LittleEndian(span.Slice(offset), value);
		offset += sizeof(int);
	}

	private static void WriteString(ref Span<byte> span, ref int offset, string value)
	{
		var bytes = Encoding1252.GetBytes(value);
		bytes.AsSpan().CopyTo(span.Slice(offset));
		offset += bytes.Length;
	}
}