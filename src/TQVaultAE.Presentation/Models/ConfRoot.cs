using Newtonsoft.Json;
using System.Collections.Generic;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Presentation.Models
{

	public class ConfRoot
	{
		public List<ConfFile> list;
	}

	public class ConfFile
	{
		[JsonProperty("file")]
		public string fileName;
		[JsonProperty("img")]
		public List<ConfMatch> imgMatch;
	}

	public class ConfMatch
	{
		[JsonProperty("ca")]
		public IconCategory Category;
		[JsonProperty("lt")]
		public List<string> Literal;
		[JsonProperty("pa")]
		public string Pattern;
		[JsonProperty("on")]
		public string On;
		[JsonProperty("of")]
		public string Off;
		[JsonProperty("ov")]
		public string Over;
		public bool IsRegex => !string.IsNullOrWhiteSpace(this.Pattern);

	}

}
