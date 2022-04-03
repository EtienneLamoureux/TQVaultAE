using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TQVaultAE.Domain.Entities
{
	public class BagButtonIconInfo
	{
		/// <summary>
		/// BagButton display mode
		/// </summary>
		[JsonProperty("mode")]
		public BagButtonDisplayMode DisplayMode;
		/// <summary>
		/// BagButton Custom label 
		/// </summary>
		[JsonProperty("label")]
		public string Label;
		/// <summary>
		/// BagButton On icon
		/// </summary>
		[JsonProperty("on")]
		public string On;
		/// <summary>
		/// BagButton Off icon
		/// </summary>
		[JsonProperty("off")]
		public string Off;
		/// <summary>
		/// BagButton Over icon
		/// </summary>
		[JsonProperty("over")]
		public string Over;
	}
}
