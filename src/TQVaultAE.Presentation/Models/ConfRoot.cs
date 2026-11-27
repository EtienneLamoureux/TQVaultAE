using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Presentation.Models;

public class ConfRoot
{
	public List<ConfFile> list = [];
}

public class ConfFile
{
	[JsonPropertyName("file")]
	public string fileName = string.Empty;
	[JsonPropertyName("img")]
	public List<ConfMatch> imgMatch = [];
}

public class ConfMatch
{
	[JsonPropertyName("ca")]
	public IconCategory Category;
	[JsonPropertyName("lt")]
	public List<string>? Literal;
	[JsonPropertyName("pa")]
	public string? Pattern;
	[JsonPropertyName("on")]
	public string On = string.Empty;
	[JsonPropertyName("of")]
	public string Off = string.Empty;
	[JsonPropertyName("ov")]
	public string Over = string.Empty;
	[MemberNotNullWhen(true, nameof(Pattern))]
	public bool IsRegex => !string.IsNullOrWhiteSpace(this.Pattern);

}