using System.Text.Json;
using AwesomeAssertions;
using TQVaultAE.Data.Dto;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Tests.Data;

/// <summary>
/// The generated vault serializer must read and write the vault files exactly as the reflection-based one
/// with the application's options did.
/// </summary>
public class VaultJsonContextTests
{
	private static readonly JsonSerializerOptions ReflectionOptions = new() { IncludeFields = true, PropertyNameCaseInsensitive = true, WriteIndented = true };

	private static VaultDto Sample() => new()
	{
		disabledtooltip = [1, 3],
		currentlyFocusedSackNumber = 2,
		currentlySelectedSackNumber = 1,
		sacks =
		[
			new SackDto
			{
				iconinfo = new BagButtonIconInfo { DisplayMode = BagButtonDisplayMode.CustomIcon | BagButtonDisplayMode.Label, Label = "Rings", OnStr = """Items\ring.tex""" },
				items =
				[
					new ItemDto { baseName = """records\item\ring.dbr""", prefixName = "p", seed = 1234, stackSize = 1, var1 = 3, relicName2 = "r2", pointX = 4, pointY = 5 },
				],
			},
			new SackDto(),
		],
	};

	[Fact]
	public void Serialize_WritesTheSameJsonAsReflection()
	{
		var dto = Sample();

		var generated = JsonSerializer.Serialize(dto, VaultJsonContext.Default.VaultDto);

		generated.Should().Be(JsonSerializer.Serialize(dto, ReflectionOptions));
	}

	[Fact]
	public void Deserialize_ReadsAFileWrittenByReflection_WhateverTheCase()
	{
		var json = JsonSerializer.Serialize(Sample(), ReflectionOptions)
			.Replace("\"sacks\"", "\"Sacks\"", StringComparison.Ordinal)
			.Replace("\"baseName\"", "\"BASENAME\"", StringComparison.Ordinal);

		var dto = JsonSerializer.Deserialize(json, VaultJsonContext.Default.VaultDto)!;

		dto.sacks.Should().HaveCount(2);
		dto.sacks[0].iconinfo!.DisplayMode.Should().Be(BagButtonDisplayMode.CustomIcon | BagButtonDisplayMode.Label);
		dto.sacks[0].items[0].baseName.Should().Be("""records\item\ring.dbr""");
		dto.sacks[0].items[0].seed.Should().Be(1234);
		dto.disabledtooltip.Should().Equal(1, 3);
	}
}
