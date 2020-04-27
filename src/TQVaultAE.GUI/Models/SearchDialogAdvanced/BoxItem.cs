using Newtonsoft.Json;
using System.Collections.Generic;
using TQVaultAE.Domain.Search;
using TQVaultAE.GUI.Components;

namespace TQVaultAE.GUI.Models.SearchDialogAdvanced
{
	public class BoxItem
	{
		ScalingCheckedListBox _CheckedList;
		[JsonIgnore]
		public ScalingCheckedListBox CheckedList
		{
			get => _CheckedList;
			set
			{
				CheckedListName = value.Name;
				_CheckedList = value;
			}
		}

		ScalingLabel _Category;
		[JsonIgnore]
		public ScalingLabel Category
		{
			get => _Category; 
			set
			{
				CategoryName = value.Name;
				_Category = value;
			}
		}

		[JsonIgnore]
		public IEnumerable<Result> MatchingResults { get; set; }

		public string CheckedListName { get; set; }
		public string CategoryName { get; set; }
		public string DisplayValue { get; set; }
		public override string ToString()
			=> this.DisplayValue;
	}
}
