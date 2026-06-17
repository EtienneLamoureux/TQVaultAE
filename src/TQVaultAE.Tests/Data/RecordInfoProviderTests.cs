using System.Buffers.Binary;
using AwesomeAssertions;

namespace TQVaultAE.Tests.Data;

public class RecordInfoProviderTests
{
	[Fact]
	public void FloatParsing_OldAndNewAlgorithm_ProduceSameResult()
	{
		// Arrange: Create binary data with float values
		// Format: dataType(int16) + valCount(int16) + variableID(int32) + float values
		float[] testFloats = { 0.0f, 1.5f, -3.14f, float.MaxValue, float.MinValue, float.Epsilon };

		using var ms = new MemoryStream();
		using var writer = new BinaryWriter(ms);

		// Write header: dataType = 1 (Float), valCount = number of floats
		writer.Write((short)1); // dataType = Float
		writer.Write((short)testFloats.Length); // valCount
		writer.Write(12345); // variableID

		// Write float values
		foreach (float f in testFloats)
			writer.Write(f);

		var data = ms.ToArray();

		// Act: Parse using OLD method (BinaryReader)
		var oldResult = ParseWithOldMethod(data);

		// Parse using NEW method (Span + BinaryPrimitives)
		var newResult = ParseWithNewMethod(data);

		// Assert: Both should produce identical results
		newResult.dataType.Should().Be(oldResult.dataType);
		newResult.valCount.Should().Be(oldResult.valCount);
		newResult.variableID.Should().Be(oldResult.variableID);
		newResult.floats.Should().Equal(oldResult.floats);
	}

	[Fact]
	public void FloatParsing_SingleFloat_KnownValue()
	{
		// Arrange: Single float with known value
		float expected = 123.456f;
		byte[] data;
		using (var ms = new MemoryStream())
		{
			using var writer = new BinaryWriter(ms);
			writer.Write((short)1); // dataType = Float
			writer.Write((short)1); // valCount = 1
			writer.Write(0); // variableID
			writer.Write(expected);
			data = ms.ToArray();
		}

		// Act
		var result = ParseWithNewMethod(data);

		// Assert
		result.floats.Should().HaveCount(1);
		result.floats[0].Should().Be(expected);
	}

	[Fact]
	public void IntegerParsing_OldAndNewAlgorithm_ProduceSameResult()
	{
		// Arrange: Create binary data with integer values
		int[] testInts = { 0, 1, -100, int.MaxValue, int.MinValue };

		using var ms = new MemoryStream();
		using var writer = new BinaryWriter(ms);

		writer.Write((short)0); // dataType = Integer
		writer.Write((short)testInts.Length);
		writer.Write(12345);

		foreach (int i in testInts)
			writer.Write(i);

		var data = ms.ToArray();

		// Act
		var oldResult = ParseIntegersOldMethod(data);
		var newResult = ParseIntegersNewMethod(data);

		// Assert
		newResult.Should().Equal(oldResult);
	}

	[Fact]
	public void StringParsing_OldAndNewAlgorithm_ProduceSameResult()
	{
		// Arrange: Create binary data with string reference (int32 string ID)
		int[] testStringIds = { 100, 200, 300 };

		using var ms = new MemoryStream();
		using var writer = new BinaryWriter(ms);

		writer.Write((short)2); // dataType = StringVar
		writer.Write((short)testStringIds.Length);
		writer.Write(12345);

		foreach (int id in testStringIds)
			writer.Write(id);

		var data = ms.ToArray();

		// Act
		var oldResult = ParseStringsOldMethod(data);
		var newResult = ParseStringsNewMethod(data);

		// Assert
		newResult.Should().Equal(oldResult);
	}

	/// <summary>
	/// OLD method: BinaryReader + MemoryStream
	/// </summary>
	private static (short dataType, short valCount, int variableID, float[] floats) ParseWithOldMethod(byte[] data)
	{
		using var reader = new BinaryReader(new MemoryStream(data, false));
		short dataType = reader.ReadInt16();
		short valCount = reader.ReadInt16();
		int variableID = reader.ReadInt32();

		var floats = new float[valCount];
		for (int i = 0; i < valCount; i++)
			floats[i] = reader.ReadSingle();

		return (dataType, valCount, variableID, floats);
	}

	/// <summary>
	/// NEW method: ReadOnlySpan + BinaryPrimitives
	/// </summary>
	private static (short dataType, short valCount, int variableID, float[] floats) ParseWithNewMethod(byte[] data)
	{
		var dataSpan = new ReadOnlySpan<byte>(data);
		int offset = 0;

		short dataType = BinaryPrimitives.ReadInt16LittleEndian(dataSpan.Slice(offset));
		offset += 2;

		short valCount = BinaryPrimitives.ReadInt16LittleEndian(dataSpan.Slice(offset));
		offset += 2;

		int variableID = BinaryPrimitives.ReadInt32LittleEndian(dataSpan.Slice(offset));
		offset += 4;

		var floats = new float[valCount];
		for (int i = 0; i < valCount; i++)
		{
			int intBits = BinaryPrimitives.ReadInt32LittleEndian(dataSpan.Slice(offset));
			byte[] bytes = BitConverter.GetBytes(intBits);
			floats[i] = BitConverter.ToSingle(bytes, 0);
			offset += 4;
		}

		return (dataType, valCount, variableID, floats);
	}

	/// <summary>
	/// OLD method for integers
	/// </summary>
	private static int[] ParseIntegersOldMethod(byte[] data)
	{
		using var reader = new BinaryReader(new MemoryStream(data, false));
		reader.ReadInt16(); // dataType
		short valCount = reader.ReadInt16();
		reader.ReadInt32(); // variableID

		var ints = new int[valCount];
		for (int i = 0; i < valCount; i++)
			ints[i] = reader.ReadInt32();

		return ints;
	}

	/// <summary>
	/// NEW method for integers
	/// </summary>
	private static int[] ParseIntegersNewMethod(byte[] data)
	{
		var dataSpan = new ReadOnlySpan<byte>(data);
		int offset = 2; // skip dataType

		short valCount = BinaryPrimitives.ReadInt16LittleEndian(dataSpan.Slice(offset));
		offset += 2;

		offset += 4; // skip variableID

		var ints = new int[valCount];
		for (int i = 0; i < valCount; i++)
		{
			ints[i] = BinaryPrimitives.ReadInt32LittleEndian(dataSpan.Slice(offset));
			offset += 4;
		}

		return ints;
	}

	/// <summary>
	/// OLD method for strings
	/// </summary>
	private static int[] ParseStringsOldMethod(byte[] data)
	{
		using var reader = new BinaryReader(new MemoryStream(data, false));
		reader.ReadInt16(); // dataType
		short valCount = reader.ReadInt16();
		reader.ReadInt32(); // variableID

		var ids = new int[valCount];
		for (int i = 0; i < valCount; i++)
			ids[i] = reader.ReadInt32();

		return ids;
	}

	/// <summary>
	/// NEW method for strings
	/// </summary>
	private static int[] ParseStringsNewMethod(byte[] data)
	{
		var dataSpan = new ReadOnlySpan<byte>(data);
		int offset = 2;

		short valCount = BinaryPrimitives.ReadInt16LittleEndian(dataSpan.Slice(offset));
		offset += 2;

		offset += 4;

		var ids = new int[valCount];
		for (int i = 0; i < valCount; i++)
		{
			ids[i] = BinaryPrimitives.ReadInt32LittleEndian(dataSpan.Slice(offset));
			offset += 4;
		}

		return ids;
	}
}