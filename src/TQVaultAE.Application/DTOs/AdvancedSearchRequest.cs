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
}