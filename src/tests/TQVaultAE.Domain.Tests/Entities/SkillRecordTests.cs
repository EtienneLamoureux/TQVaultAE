using System.Text;
using AwesomeAssertions;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Entities;

public class SkillRecordTests
{
	private readonly ITestOutputHelper _output;

	static SkillRecordTests()
	{
		// Register encoding provider for .NET 10+ - must be done before GetEncoding
		Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
	}

	public SkillRecordTests(ITestOutputHelper output)
	{
		_output = output;
	}

	private static Encoding Encoding1252 => Encoding.GetEncoding(1252);

	[Fact]
	public void ToBinary_OldAndNewAlgorithm_ProduceSameResult()
	{
		// Arrange
		var record = new SkillRecord
		{
			skillName = "Fireball",
			skillLevel = 5,
			skillEnabled = 1,
			skillSubLevel = 2,
			skillActive = 1,
			skillTransition = 0
		};

		int beginBlockValue = -1340212530;
		int endBlockValue = -559038242;

		// Act: Generate using old method
		byte[] oldResult = ToBinaryOld(record, beginBlockValue, endBlockValue);

		// Generate using new method
		byte[] newResult = record.ToBinary(beginBlockValue, endBlockValue);

		// Debug output
		_output.WriteLine($"Old length: {oldResult.Length}, New length: {newResult.Length}");

		// Assert: Both should produce identical results
		newResult.Length.Should().Be(oldResult.Length);
		newResult.Should().Equal(oldResult);
	}

	[Fact]
	public void ToBinary_KnownValues_ProduceDeterministicOutput()
	{
		// Arrange
		var record = new SkillRecord
		{
			skillName = "Test",
			skillLevel = 10,
			skillEnabled = 1,
			skillSubLevel = 0,
			skillActive = 1,
			skillTransition = 0
		};

		// Act
		var result1 = record.ToBinary(1, 2);
		var result2 = record.ToBinary(1, 2);

		// Assert: Same input should produce same output
		result1.Should().Equal(result2);
		result1.Length.Should().BeGreaterThan(0);
	}

	[Fact]
	public void ToBinary_EmptySkillName_ProducesValidOutput()
	{
		// Arrange
		var record = new SkillRecord
		{
			skillName = "",
			skillLevel = 1,
			skillEnabled = 0,
			skillSubLevel = 0,
			skillActive = 0,
			skillTransition = 0
		};

		// Act
		var result = record.ToBinary(1, 2);

		// Assert: Should produce valid binary data
		result.Should().NotBeEmpty();
		result.Length.Should().BeGreaterThan(0);
	}

	/// <summary>
	/// OLD method: Original LINQ-based implementation
	/// </summary>
	private static byte[] ToBinaryOld(SkillRecord record, int beginBlockValue, int endBlockValue)
	{
		var array = new[] {

			BitConverter.GetBytes("begin_block".Length),
			Encoding1252.GetBytes("begin_block"),
			BitConverter.GetBytes(beginBlockValue),

			BitConverter.GetBytes(nameof(record.skillName).Length),
			Encoding1252.GetBytes(nameof(record.skillName)),
			BitConverter.GetBytes(record.skillName.Length),
			Encoding1252.GetBytes(record.skillName),

			BitConverter.GetBytes(nameof(record.skillLevel).Length),
			Encoding1252.GetBytes(nameof(record.skillLevel)),
			BitConverter.GetBytes(record.skillLevel),

			BitConverter.GetBytes(nameof(record.skillEnabled).Length),
			Encoding1252.GetBytes(nameof(record.skillEnabled)),
			BitConverter.GetBytes(record.skillEnabled),

			BitConverter.GetBytes(nameof(record.skillSubLevel).Length),
			Encoding1252.GetBytes(nameof(record.skillSubLevel)),
			BitConverter.GetBytes(record.skillSubLevel),

			BitConverter.GetBytes(nameof(record.skillActive).Length),
			Encoding1252.GetBytes(nameof(record.skillActive)),
			BitConverter.GetBytes(record.skillActive),

			BitConverter.GetBytes(nameof(record.skillTransition).Length),
			Encoding1252.GetBytes(nameof(record.skillTransition)),
			BitConverter.GetBytes(record.skillTransition),

			BitConverter.GetBytes("end_block".Length),
			Encoding1252.GetBytes("end_block"),
			BitConverter.GetBytes(endBlockValue),

		}.SelectMany(arr => arr).ToArray();

		return array;
	}
}