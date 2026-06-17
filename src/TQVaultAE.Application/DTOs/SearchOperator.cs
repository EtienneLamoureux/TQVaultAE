using System.Text.Json.Serialization;

namespace TQVaultAE.Application.DTOs;

/// <summary>
/// Logical operators for combining search filters.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SearchOperator
{
	/// <summary>
	/// All filters must match (AND).
	/// </summary>
	And,

	/// <summary>
	/// Any filter can match (OR).
	/// </summary>
	Or
}