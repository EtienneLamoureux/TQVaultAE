using System.Collections.Generic;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.GUI.Models.SearchDialogAdvanced;

public class SearchQuery
{
	public record VisibilityItem(string Name, bool Visible);

	public IEnumerable<BoxItem> CheckedItems;
	public string QueryName = string.Empty;
	public string Filter;
	public List<VisibilityItem> Visible;
	public decimal MaxElement;
	public bool Reduce;
	public SearchOperator Logic;

	public int MaxStr;
	public int MaxDex;
	public int MaxInt;
	public int MinStr;
	public int MinDex;
	public int MinInt;
	public bool MinRequirement;
	public bool MaxRequirement;
	public int MaxLvl;
	public int MinLvl;
	public bool HavingPrefix;
	public bool HavingSuffix;
	public bool HavingRelic;
	public bool HavingCharm;
	public bool IsSetItem;

	public override string ToString()
		=> this.QueryName;
}
