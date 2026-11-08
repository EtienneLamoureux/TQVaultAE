using System.Text.Json.Serialization;

namespace TQVaultAE.Domain.Entities;

public class BagButtonIconInfo
{
	/// <summary>
	/// BagButton display mode
	/// </summary>
	[JsonPropertyName("mode")]
	[JsonConverter(typeof(JsonStringEnumConverter<BagButtonDisplayMode>))]
	public BagButtonDisplayMode DisplayMode;
	/// <summary>
	/// BagButton Custom label 
	/// </summary>
	[JsonPropertyName("label")]
	public string Label = string.Empty;

	/// <summary>
	/// BagButton On icon
	/// </summary>
	[JsonPropertyName("on")]
	public string OnStr = string.Empty;

	[JsonIgnore]
	public RecordId On
	{
		get => OnStr;
		set => OnStr = RecordId.RawOf(value);
	}

	/// <summary>
	/// BagButton Off icon
	/// </summary>
	[JsonPropertyName("off")]
	public string OffStr = string.Empty;

	[JsonIgnore]
	public RecordId Off
	{
		get => OffStr;
		set => OffStr = RecordId.RawOf(value);
	}

	/// <summary>
	/// BagButton Over icon
	/// </summary>
	[JsonPropertyName("over")]
	public string OverStr = string.Empty;

	[JsonIgnore]
	public RecordId Over
	{
		get => OverStr;
		set => OverStr = RecordId.RawOf(value);
	}

	/// <summary>
	/// Returns a copy that shares nothing with this instance, so a pasted icon is not changed with its source.
	/// </summary>
	public BagButtonIconInfo Clone()
		=> (BagButtonIconInfo)this.MemberwiseClone();

	/// <summary>
	/// Display mode chosen in the bag button settings: the default icon has neither number nor label, a custom icon
	/// adds the ones ticked.
	/// </summary>
	public static BagButtonDisplayMode DisplayModeOf(bool useDefault, bool showNumber, bool showLabel)
	{
		if (useDefault)
			return BagButtonDisplayMode.Default;

		var mode = BagButtonDisplayMode.CustomIcon;
		if (showNumber) mode |= BagButtonDisplayMode.Number;
		if (showLabel) mode |= BagButtonDisplayMode.Label;
		return mode;
	}

	/// <summary>
	/// Icon info for <paramref name="mode"/> and the chosen pictures; check it with <see cref="Validate"/>.
	/// The label is trimmed, a missing picture is empty.
	/// </summary>
	public static BagButtonIconInfo Create(BagButtonDisplayMode mode, string? label, RecordId? on, RecordId? off, RecordId? over)
		=> new()
		{
			DisplayMode = mode,
			Label = label?.Trim() ?? string.Empty,
			On = on ?? RecordId.Empty,
			Off = off ?? RecordId.Empty,
			Over = over ?? RecordId.Empty,
		};

	/// <summary>
	/// A custom icon needs its On and Off pictures, and a label needs a text. The default icon needs nothing.
	/// </summary>
	public bool Validate()
	{
		if (this.DisplayMode == BagButtonDisplayMode.Default)
			return true;

		if (RecordId.IsNullOrEmpty(this.On) || RecordId.IsNullOrEmpty(this.Off))
			return false;

		return !this.DisplayMode.HasFlag(BagButtonDisplayMode.Label) || !string.IsNullOrWhiteSpace(this.Label);
	}

	/// <summary>Whether this info shows the default bag icon.</summary>
	public static bool IsDefault(BagButtonIconInfo? info)
		=> info is null || info.DisplayMode == BagButtonDisplayMode.Default;

	/// <summary>
	/// Whether <paramref name="left"/> and <paramref name="right"/> show the same button. No info and the default
	/// mode are the same; so are two default infos, whatever pictures they kept.
	/// </summary>
	public static bool AreSame(BagButtonIconInfo? left, BagButtonIconInfo? right)
	{
		if (IsDefault(left) || IsDefault(right))
			return IsDefault(left) && IsDefault(right);

		return left!.DisplayMode == right!.DisplayMode
			&& left.Label == right.Label
			&& left.On == right.On
			&& left.Off == right.Off
			&& left.Over == right.Over;
	}
}