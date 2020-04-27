using System.Collections.Generic;

namespace TQVaultAE.GUI.Models.SearchDialogAdvanced
{
	public class SearchQuery
	{
		public IEnumerable<BoxItem> CheckedItems;
		public string QueryName = string.Empty;
		public override string ToString()
			=> this.QueryName;
	}
}
