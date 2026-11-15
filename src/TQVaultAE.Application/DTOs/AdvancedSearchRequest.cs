using TQVaultAE.Application.Results;

namespace TQVaultAE.Application.DTOs;

/// <summary>
/// Request object for advanced search filtering.
/// </summary>
public class AdvancedSearchRequest
{
	/// <summary>
	/// The initial results to filter.
	/// </summary>
	public required IReadOnlyList<SearchResult> InitialResults { get; init; }

	/// <summary>
	/// Filter items that have a prefix.
	/// </summary>
	public bool HasPrefix { get; init; }

	/// <summary>
	/// Filter items that have a suffix.
	/// </summary>
	public bool HasSuffix { get; init; }

	/// <summary>
	/// Filter items that have a relic.
	/// </summary>
	public bool HasRelic { get; init; }

	/// <summary>
	/// Filter items that have a charm.
	/// </summary>
	public bool HasCharm { get; init; }

	/// <summary>
	/// Filter items that are set items.
	/// </summary>
	public bool IsSetItem { get; init; }

	/// <summary>
	/// Filter items that grant a skill.
	/// </summary>
	public bool HasSkill { get; init; }

	/// <summary>
	/// Filter items that come from a mod.
	/// </summary>
	public bool IsModded { get; init; }

	/// <summary>
	/// Minimum item level.
	/// </summary>
	public int MinLevel { get; init; }

	/// <summary>
	/// Maximum item level.
	/// </summary>
	public int MaxLevel { get; init; }

	/// <summary>
	/// Minimum strength requirement.
	/// </summary>
	public int MinStrength { get; init; }

	/// <summary>
	/// Maximum strength requirement.
	/// </summary>
	public int MaxStrength { get; init; }

	/// <summary>
	/// Minimum dexterity requirement.
	/// </summary>
	public int MinDexterity { get; init; }

	/// <summary>
	/// Maximum dexterity requirement.
	/// </summary>
	public int MaxDexterity { get; init; }

	/// <summary>
	/// Minimum intelligence requirement.
	/// </summary>
	public int MinIntelligence { get; init; }

	/// <summary>
	/// Maximum intelligence requirement.
	/// </summary>
	public int MaxIntelligence { get; init; }

	/// <summary>
	/// Keeps a requirement range ordered after one of its bounds changed: the other bound follows it, so that
	/// minimum ≤ maximum. Raising the minimum above the maximum raises the maximum; lowering the maximum below the
	/// minimum lowers the minimum.
	/// </summary>
	/// <param name="min">The minimum bound.</param>
	/// <param name="max">The maximum bound.</param>
	/// <param name="minChanged"><see langword="true"/> when <paramref name="min"/> is the bound the user changed.</param>
	/// <returns>The ordered range.</returns>
	public static (int Min, int Max) OrderRange(int min, int max, bool minChanged)
	{
		if (min <= max)
			return (min, max);

		return minChanged ? (min, min) : (max, max);
	}
}