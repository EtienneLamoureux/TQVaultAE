using System.Text.Json;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Services;

/// <inheritdoc cref="IClassSetService"/>
public class ClassSetService(ILogger<ClassSetService> log, IFileIO fileIO, IPathIO pathIO) : IClassSetService
{
	/// <summary>Version written in the file, for later changes of its layout.</summary>
	internal const int FileVersion = 1;

	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		WriteIndented = true,
	};

	private readonly ILogger Log = log;
	private readonly IFileIO FileIO = fileIO;
	private readonly IPathIO PathIO = pathIO;

	/// <inheritdoc/>
	public ClassSetBook Read(string saveFolder)
	{
		var book = new ClassSetBook(this.PathIO.Combine(saveFolder, IClassSetService.FileName));
		if (!this.FileIO.Exists(book.FilePath))
			return book;

		try
		{
			var dto = JsonSerializer.Deserialize<ClassSetFileDto>(this.FileIO.ReadAllText(book.FilePath), JsonOptions)
				?? throw new JsonException("Empty class sets file");
			book.Sets = [.. dto.classSets.Select(FromDto)];
		}
		catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
		{
			this.Log.LogError(ex, "Failed to read the class sets {File}", book.FilePath);
			book.ReadFailed = true;
		}

		return book;
	}

	/// <inheritdoc/>
	public void Write(ClassSetBook book, IReadOnlyList<ClassSet> sets)
	{
		if (book.ReadFailed)
			return;

		if (sets.Count == 0)
		{
			if (this.FileIO.Exists(book.FilePath))
				this.FileIO.Delete(book.FilePath);
		}
		else
		{
			var dto = new ClassSetFileDto { version = FileVersion, classSets = [.. sets.Select(ToDto)] };
			this.FileIO.WriteAllText(book.FilePath, JsonSerializer.Serialize(dto, JsonOptions));
		}

		book.Sets = [.. sets];
	}

	private static ClassSetDto ToDto(ClassSet set) => new()
	{
		name = set.Name,
		masteries = [.. set.Masteries.Select(m => new ClassSetMasteryDto
		{
			mastery = m.Mastery.ToString(),
			level = m.Level,
			skills = [.. m.Skills.Select(s => new ClassSetSkillDto { skill = s.Skill, level = s.Level })],
		})],
		attributes = new ClassSetAttributesDto
		{
			health = set.Attributes.Health,
			energy = set.Attributes.Energy,
			strength = set.Attributes.Strength,
			intelligence = set.Attributes.Intelligence,
			dexterity = set.Attributes.Dexterity,
		},
	};

	/// <summary>A set from the file; a mastery whose name is unknown is left out.</summary>
	private static ClassSet FromDto(ClassSetDto dto)
	{
		var attributes = dto.attributes ?? new ClassSetAttributesDto();
		return new ClassSet(
			dto.name ?? string.Empty,
			[.. dto.masteries
				.Where(m => Enum.TryParse<Masteries>(m.mastery, ignoreCase: true, out _))
				.Select(m => new ClassSetMastery(
					Enum.Parse<Masteries>(m.mastery!, ignoreCase: true),
					m.level,
					[.. m.skills.Where(s => !string.IsNullOrEmpty(s.skill)).Select(s => new ClassSetSkill(s.skill!, s.level))]))],
			new ClassSetAttributes(attributes.health, attributes.energy, attributes.strength, attributes.intelligence, attributes.dexterity));
	}
}

/// <summary>The class sets file.</summary>
internal sealed class ClassSetFileDto
{
	public int version { get; set; } = ClassSetService.FileVersion;

	public List<ClassSetDto> classSets { get; set; } = [];
}

/// <summary>A class set in the file.</summary>
internal sealed class ClassSetDto
{
	public string? name { get; set; }

	public List<ClassSetMasteryDto> masteries { get; set; } = [];

	public ClassSetAttributesDto? attributes { get; set; }
}

/// <summary>A mastery of a class set: its name (<see cref="Masteries"/>), its level and its skills.</summary>
internal sealed class ClassSetMasteryDto
{
	public string? mastery { get; set; }

	public int level { get; set; }

	public List<ClassSetSkillDto> skills { get; set; } = [];
}

/// <summary>A skill of a class set mastery: its record id and its points.</summary>
internal sealed class ClassSetSkillDto
{
	public string? skill { get; set; }

	public int level { get; set; }
}

/// <summary>The attribute points spent in each attribute.</summary>
internal sealed class ClassSetAttributesDto
{
	public int health { get; set; }

	public int energy { get; set; }

	public int strength { get; set; }

	public int intelligence { get; set; }

	public int dexterity { get; set; }
}
