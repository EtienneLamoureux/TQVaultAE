using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TQVaultAE.Domain.Helpers;

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
		public string OnStr;

		[JsonIgnore]
		public RecordId On
		{
			get => OnStr.ToRecordId();
			set => OnStr = value?.Raw ?? string.Empty;
		}

		/// <summary>
		/// BagButton Off icon
		/// </summary>
		[JsonProperty("off")]
		public string OffStr;

		[JsonIgnore]
		public RecordId Off
		{
			get => OffStr.ToRecordId();
			set => OffStr = value?.Raw ?? string.Empty;
		}

		/// <summary>
		/// BagButton Over icon
		/// </summary>
		[JsonProperty("over")]
		public string OverStr;

		[JsonIgnore]
		public RecordId Over
		{
			get => OverStr.ToRecordId();
			set => OverStr = value?.Raw ?? string.Empty;
		}
	}
}
