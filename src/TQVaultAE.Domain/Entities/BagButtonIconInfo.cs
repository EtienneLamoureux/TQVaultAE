using System.Text.Json.Serialization;

namespace TQVaultAE.Domain.Entities;

public class BagButtonIconInfo
{
	/// <summary>
	/// BagButton display mode
	/// </summary>
	[JsonPropertyName("mode")]
	public BagButtonDisplayMode DisplayMode;
	/// <summary>
	/// BagButton Custom label 
	/// </summary>
	[JsonPropertyName("label")]
	public string Label;

	/// <summary>
	/// BagButton On icon
	/// </summary>
	[JsonPropertyName("on")]
	public string OnStr;

	[JsonIgnore]
	public RecordId On
	{
		get => OnStr;
		set => OnStr = value?.Raw ?? string.Empty;
	}

	/// <summary>
	/// BagButton Off icon
	/// </summary>
	[JsonPropertyName("off")]
	public string OffStr;

	[JsonIgnore]
	public RecordId Off
	{
		get => OffStr;
		set => OffStr = value?.Raw ?? string.Empty;
	}

	/// <summary>
	/// BagButton Over icon
	/// </summary>
	[JsonPropertyName("over")]
	public string OverStr;

	[JsonIgnore]
	public RecordId Over
	{
		get => OverStr;
		set => OverStr = value?.Raw ?? string.Empty;
	}
}