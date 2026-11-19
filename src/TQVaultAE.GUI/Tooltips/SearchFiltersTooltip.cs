using System.Text.RegularExpressions;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Dialogs;
using TQVaultAE.GUI.Models.Search;
using TQVaultAE.Presentation.Win32.Contracts;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Tooltips;

public partial class SearchFiltersTooltip : BaseTooltip
{
	private static object syncObj = new object();

	private static SearchFiltersTooltip? _Current = null;

	internal Control AnchorControl { get; private set; }
	internal List<BoxItem> Filters { get; private set; } = [];
	internal SearchOperator Operator { get; private set; }


#if DEBUG
	// For Design Mode
#pragma warning disable CS8618 // Designer only: the runtime constructor sets the services and state.
	public SearchFiltersTooltip() => InitializeComponent();
#pragma warning restore CS8618
#endif

	private SearchFiltersTooltip(SearchDialogAdvanced instance, GuiServices services, Control anchorControl) : base(services)
	{
		this.Owner = instance;
		this.AnchorControl = anchorControl;

		InitializeComponent();

		SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

		// Fill it outside of screen to avoid flickering
		this.Location = new Point(0, Screen.FromControl(this).WorkingArea.Height);
	}

	public static void HideTooltip()
	{
		lock (syncObj)
		{
			if (_Current is not null) _Current.Close();
			_Current = null;
		}
	}

	#region Factory

	public static SearchFiltersTooltip? ShowTooltip(GuiServices services, Control anchorControl, List<BoxItem> filters, SearchOperator ope)
	{
		if (anchorControl is null) return null;
		if (filters is not { Count: > 0 }) return null;

		SearchFiltersTooltip newTT;
		lock (syncObj)
		{
			HideTooltip();
			newTT = new SearchFiltersTooltip(
				(SearchDialogAdvanced)anchorControl.FindForm()!
				, services
				, anchorControl
			)
			{
				Filters = filters,
				Operator = ope
			};
			_Current = newTT;
			newTT.Show();
		}
		return newTT;
	}

	#endregion

	[GeneratedRegex(@"[^\w]")]
	private static partial Regex FillToolTipRegEx();
	/// <summary>
	/// Init the tooltip content.
	/// </summary>
	public void FillToolTip()
	{
		this.SuspendLayout();
		this.flowLayoutPanelFriendlyNames.SuspendLayout();

		// Operator description
		var opeTranslate = this.Operator == SearchOperator.And ? Resources.SearchOperatorAnd : Resources.SearchOperatorOr;
		AddRow($"{Resources.SearchOperatorTitle} : {opeTranslate}", FGColor: TQColor.Aqua.Color(), style: FontStyle.Bold);
		if (this.Operator == SearchOperator.And)
			AddRow(Resources.SearchOperatorDescAnd);
		else
			AddRow(Resources.SearchOperatorDescOr);

		AddRow(TOOLTIPDELIM);

		// Filters descriptions

		var searchTermsGroup = // TODO Maybe multiple in the future using old notation
			from f in this.Filters
			where f.CheckedList is null && f.Category is not null
			let cleanName = FillToolTipRegEx().Replace(f.Category!.Text, string.Empty)
			group f by cleanName into grp
			orderby grp.Key
			select grp;

		if (searchTermsGroup.Any())
		{
			foreach (var term in searchTermsGroup)
			{
				AddRow(term.Key, FGColor: TQColor.Orange.Color(), style: FontStyle.Bold);
				foreach (var filter in term)
					AddRow(filter.DisplayValue, FGColor: TQColor.Green.Color(), style: FontStyle.Regular);
			}

			AddRow(TOOLTIPDELIM);
		}

		var catgroups =
			from f in this.Filters
			where f.CheckedList is not null
			group f by f.Category into grp
			orderby grp.Key.Text
			select grp;

		var firstCategory = true;
		foreach (var category in catgroups)
		{
			if (firstCategory)
				firstCategory = false;
			else
				AddRow(TOOLTIPSPACER);

			AddRow(category.Key.Text, FGColor: category.Key.ForeColor, style: FontStyle.Bold);
			foreach (var filter in category)
				AddRow(filter.DisplayValue, FGColor: filter.CheckedList?.ForeColor, style: FontStyle.Regular);
		}

		this.flowLayoutPanelFriendlyNames.ResumeLayout();
		this.ResumeLayout();
	}

	private void AddRow(string? friendlyName = TOOLTIPSPACER, Color? FGColor = null, float fontSize = 10F, FontStyle style = FontStyle.Regular, Color? BGColor = null)
	{
		Control? row = MakeRow(this.UIService, this.FontService, friendlyName, FGColor, fontSize, style, BGColor: this.flowLayoutPanelFriendlyNames.BackColor);
		this.flowLayoutPanelFriendlyNames.Controls.Add(row);
	}

	private void ItemTooltip_Load(object? sender, EventArgs e)
	{
		// The tool tip may go multi-column, up to the size of the anchor's monitor.
		this.flowLayoutPanelFriendlyNames.MaximumSize = WorkingAreaOf(this.AnchorControl).Size;

		this.FillToolTip();

		this.PlaceUnder(this.AnchorControl);
	}

}