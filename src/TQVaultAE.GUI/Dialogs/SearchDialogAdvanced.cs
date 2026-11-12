using TQVaultAE.GUI.Components.Layout;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.Presentation;
using TQVaultAE.Domain.Helpers;
using Microsoft.Extensions.Logging;
using TQVaultAE.GUI.Models.Search;
using TQVaultAE.GUI.Tooltips;
using System.Text.RegularExpressions;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Results;

namespace TQVaultAE.GUI.Dialogs;

/// <summary>
/// Class for the Search Dialog box.
/// </summary>
public partial class SearchDialogAdvanced : VaultForm
{
	private readonly IItemDatabaseService ItemDatabaseService;
	private readonly ILogger Log;
	private readonly Bitmap ButtonImageUp;
	private readonly Bitmap ButtonImageDown;
	private readonly (ScalingButton Button, FlowLayoutPanel Panel)[] NavMap;
	private readonly List<BoxItem> SelectedFilters = [];
	private readonly HashSet<BoxItem> SelectedFiltersSet = [];
	private readonly List<VirtualCheckedListBox> CachedCheckedListBoxes = [];
	private readonly ISearchQueryService SearchQueryService;

	/// <summary>Live filters of each saved query (<see cref="LoadPersonnalQueries"/>).</summary>
	private readonly Dictionary<SearchQuery, IReadOnlyList<BoxItem>> QueryFilters = new(ReferenceEqualityComparer.Instance);

	public SearchResult[] QueryResults { get; private set; } = [];
	private bool scalingCheckBoxReduceDuringSelection_LastChecked;

	/// <summary>
	/// Initializes a new instance of SearchDialog class.
	/// </summary>
	public SearchDialogAdvanced(
		MainForm instance
		, ILogger<SearchDialogAdvanced> log
		, IItemDatabaseService itemDatabaseService
	) : base(instance.Services)
	{
		this.Owner = instance;
		this.Log = log;
		this.SearchQueryService = instance.Services.SearchQueryService;
		this.ItemDatabaseService = itemDatabaseService;

		this.InitializeComponent();
		MarbleBackground.ApplyTo(this);
		// The marble shows behind every panel, around the dark lists; the labels over it get a drop shadow.
		this.tableLayoutPanelContent.BackColor = Color.Transparent;
		this.tableLayoutPanelBottom.BackColor = Color.Transparent;
		this.ProcessAllControls(c =>
		{
			if (c is ScalingLabel label)
				label.TextShadow = true;
			else if (c is ScalingCheckBox box)
				box.TextShadow = true;
		});

		this.MinimizeBox = false;
		this.NormalizeBox = false;
		this.MaximizeBox = true;

		#region Apply custom font

		this.ProcessAllControls(c =>
		{
			if (c is IScalingControl) c.Font = FontService.GetFont(9F);
		});

		this.applyButton.Font = FontService.GetFontLight(12F);
		this.cancelButton.Font = FontService.GetFontLight(12F);

		this.Font = FontService.GetFont(9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte)(0));

		// The 17 category buttons, with the designer's 3 px margins, need more height than the column gets,
		// so the last one ("Suffix") wrapped into a second column, out of sight.
		// They take the look of Collapse All and Expand All: flat light gold buttons with dark text.
		foreach (Control button in this.flowLayoutPanelLeftColumn.Controls)
		{
			button.Margin = new Padding(button.Margin.Left, 1, button.Margin.Right, 1);
			if (button is ScalingButton category)
			{
				category.UseCustomGraphic = false;
				category.Image = null;
				category.FlatStyle = this.buttonCollapseAll.FlatStyle;
				category.BackColor = this.buttonCollapseAll.BackColor;
				category.ForeColor = this.buttonCollapseAll.ForeColor;
				category.Font = this.buttonCollapseAll.Font;
				category.UseVisualStyleBackColor = false;
			}
		}

		#endregion

		#region Load localized strings

		this.Text = Resources.SearchDialogCaption;
		this.applyButton.Text = TranslationService.TranslateXTag("tagMenuButton07");
		this.cancelButton.Text = TranslationService.TranslateXTag("tagMenuButton06");
		this.scalingLabelFiltersSelected.Tag = Resources.SearchFiltersSelected;
		this.scalingLabelFiltersSelected.Text = string.Empty;
		this.scalingLabelOperator.Text = $"{Resources.SearchOperator} :";
		this.buttonExpandAll.Text = Resources.GlobalExpandAll;
		this.buttonCollapseAll.Text = Resources.GlobalCollapseAll;
		this.scalingLabelMaxVisibleElement.Text = $"{Resources.SearchVisibleElementsPerCategory} :";
		this.scalingLabelSearchTerm.Text = $"{TranslationService.TranslateXTag("xtagLobbySearchSearch")} :";
		this.scalingButtonReset.Text = TranslationService.TranslateXTag("tagSkillReset");
		this.scalingLabelQueries.Text = $"{Resources.SearchQueries} :";
		this.scalingButtonQuerySave.Text = Resources.GlobalSave;
		this.scalingButtonQueryDelete.Text = TranslationService.TranslateXTag("tagMenuButton03");
		this.scalingCheckBoxReduceDuringSelection.Text = Resources.SearchReduceCategoriesDuringSelection;

		this.scalingComboBoxOperator.Items.Clear();
		this.scalingComboBoxOperator.Items.AddRange(new[] { Resources.SearchOperatorAnd, Resources.SearchOperatorOr });

		this.scalingButtonMenuAttribute.Text
			= this.scalingLabelItemAttributes.Text
			= TranslationService.TranslateXTag("tagCAttributes");
		this.scalingButtonMenuBaseAttribute.Text
			= this.scalingLabelBaseAttributes.Text
			= Resources.GlobalBaseAttribute;
		this.scalingButtonMenuCharacters.Text
			= this.scalingLabelCharacters.Text
			= TranslationService.TranslateXTag("tagWindowName01");
		this.scalingButtonMenuPrefixAttribute.Text
			= this.scalingLabelPrefixAttributes.Text
			= Resources.GlobalPrefix;
		this.scalingButtonMenuPrefixName.Text
			= this.scalingLabelPrefixName.Text
			= Resources.GlobalPrefixName;
		this.scalingButtonMenuQuality.Text
			= this.scalingLabelQuality.Text
			= Resources.ResultsQuality;
		this.scalingButtonMenuRarity.Text
			= this.scalingLabelRarity.Text
			= Resources.GlobalRarity;
		this.scalingButtonMenuStyle.Text
			= this.scalingLabelStyle.Text
			= Resources.GlobalStyle;
		this.scalingButtonMenuSuffixAttribute.Text
			= this.scalingLabelSuffixAttributes.Text
			= Resources.GlobalSuffix;
		this.scalingButtonMenuSuffixName.Text
			= this.scalingLabelSuffixName.Text
			= Resources.GlobalSuffixName;
		this.scalingButtonMenuType.Text
			= this.scalingLabelItemType.Text
			= Resources.GlobalType;
		this.scalingButtonMenuVaults.Text
			= this.scalingLabelInVaults.Text
			= Resources.GlobalVaults;
		this.scalingButtonMenuWithCharm.Text
			= this.scalingLabelWithCharm.Text
			= Resources.SearchHavingCharm;
		this.scalingButtonMenuWithRelic.Text
			= this.scalingLabelWithRelic.Text
			= Resources.SearchHavingRelic;
		this.scalingButtonMenuOrigin.Text
			= this.scalingLabelOrigin.Text
			= Resources.GlobalOrigin;
		this.scalingButtonMenuSetItems.Text
			= this.scalingLabelSetItems.Text
			= Resources.SearchSetItem;
		this.scalingButtonMenuSkills.Text
			= this.scalingLabelSkills.Text
			= Resources.SearchItemSkills;

		scalingCheckBoxMinReq.Text = $"{Resources.GlobalMin} {Resources.GlobalRequirements} :";
		scalingCheckBoxMaxReq.Text = $"{Resources.GlobalMax} {Resources.GlobalRequirements} :";
		scalingLabelMaxLvl.Text =
		scalingLabelMinLvl.Text = TranslationService.TranslateXTag("tagMenuImport05");
		scalingLabelMaxStr.Text =
		scalingLabelMinStr.Text = TranslationService.TranslateXTag("Strength");
		scalingLabelMaxDex.Text =
		scalingLabelMinDex.Text = TranslationService.TranslateXTag("Dexterity");
		scalingLabelMaxInt.Text =
		scalingLabelMinInt.Text = TranslationService.TranslateXTag("Intelligence");
		scalingCheckBoxHavingCharm.Text = Resources.SearchHavingCharm;
		scalingCheckBoxHavingRelic.Text = Resources.SearchHavingRelic;
		scalingCheckBoxHavingPrefix.Text = Resources.SearchHavingPrefix;
		scalingCheckBoxHavingSuffix.Text = Resources.SearchHavingSuffix;
		scalingCheckBoxIsSetItem.Text = Resources.SearchSetItem;
		scalingCheckBoxHavingSkill.Text = Resources.SearchHavingSkill;

		#endregion

		// Mapping between nav button & content component
		if (NavMap is null)
		{
			NavMap = [
				(this.scalingButtonMenuAttribute, this.flowLayoutPanelItemAttributes),
				(this.scalingButtonMenuBaseAttribute, this.flowLayoutPanelBaseAttributes),
				(this.scalingButtonMenuCharacters, this.flowLayoutPanelCharacters),
				(this.scalingButtonMenuPrefixAttribute, this.flowLayoutPanelPrefixAttributes),
				(this.scalingButtonMenuPrefixName, this.flowLayoutPanelPrefixName),
				(this.scalingButtonMenuRarity, this.flowLayoutPanelRarity),
				(this.scalingButtonMenuStyle, this.flowLayoutPanelStyle),
				(this.scalingButtonMenuSuffixAttribute, this.flowLayoutPanelSuffixAttributes),
				(this.scalingButtonMenuSuffixName, this.flowLayoutPanelSuffixName),
				(this.scalingButtonMenuType, this.flowLayoutPanelItemType),
				(this.scalingButtonMenuVaults, this.flowLayoutPanelInVaults),
				(this.scalingButtonMenuWithCharm, this.flowLayoutPanelWithCharm),
				(this.scalingButtonMenuWithRelic, this.flowLayoutPanelWithRelic),
				(this.scalingButtonMenuQuality, this.flowLayoutPanelQuality),
				(this.scalingButtonMenuOrigin, this.flowLayoutPanelOrigin),
				(this.scalingButtonMenuSetItems, this.flowLayoutPanelSetItems),
				(this.scalingButtonMenuSkills, this.flowLayoutPanelSkills),
			];
		}

		// Keep reference of base Up & Down button image
		this.ButtonImageUp = this.scalingButtonMenuAttribute.UpBitmap;
		this.ButtonImageDown = this.scalingButtonMenuAttribute.DownBitmap;

		this.scalingComboBoxOperator.SelectedIndex = (int)SearchOperator.And;

		// Remove design time fake elements
		scalingComboBoxQueryList.Items.Clear();

		// Cache all VirtualCheckedListBox references for performance
		this.flowLayoutPanelMain.ProcessAllControls(c =>
		{
			if (c is VirtualCheckedListBox lb)
				this.CachedCheckedListBoxes.Add(lb);
		});

		// Each category list in a caravan frame.
		foreach (var list in this.CachedCheckedListBoxes)
			CaravanFrame.FrameAround(list);

		// Now ClearAllCheckBoxes will work correctly
		CleanAllCheckBoxes();

		// Set numerical to default
		this.ProcessAllControls(c =>
		{
			if (c is NumericUpDown num && num != numericUpDownMaxElement) num.Value = 0;
		});

		this.VisibleChanged += this.SearchDialogVisibleChanged;
	}

	private void SetSearchBoxVisibility(bool isVisible)
		=> NavMap.ToList().ForEach(m => m.Panel.Visible = isVisible);

	#region Apply & Cancel

	/// <summary>
	/// Handler for clicking the apply button on the form.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void ApplyButtonClicked(object? sender, EventArgs e)
	{
		// The search text is applied a second after the last key: text typed just before the click is applied now,
		// else the search would run with the previous text.
		string typed = this.scalingTextBoxSearchTerm.Text.Trim();
		string applied = (this.scalingTextBoxSearchTerm.Tag as BoxItem)?.DisplayValue ?? string.Empty;
		if (!string.Equals(typed, applied, StringComparison.Ordinal))
		{
			this.MakeSearchTermBoxItem();
			this.Sync_SelectedFilters();
		}

		if (SelectedFilters.Count == 0)
		{
			scalingLabelProgress.Text = $"{Resources.SearchTermRequired} - {string.Format(Resources.SearchItemCountIs, this.ItemDatabaseService.ItemDatabase.Count)}";
			return;
		}

		// Nothing matches: stay open so the filters can be changed (the main window shows no result window then).
		if (this.QueryResults.Length == 0)
		{
			scalingLabelProgress.Text = $"{Resources.MainFormNoItemsFound2} - {string.Format(Resources.SearchItemCountIs, 0)}";
			return;
		}

		this.DialogResult = DialogResult.OK;
		this.Close();
	}

	/// <summary>
	/// Handler for clicking the cancel button on the form.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void CancelButtonClicked(object? sender, EventArgs e)
	{
		this.DialogResult = DialogResult.Cancel;
		this.Close();
	}

	#endregion

	#region Show & rebuild

	/// <summary>Item database the categories were built from, and its size at that time.</summary>
	private object? _builtDatabase;
	private int _builtCount = -1;

	/// <summary>Measures the time from opening the dialog to the categories being ready.</summary>
	private readonly System.Diagnostics.Stopwatch _openWatch = new();

	/// <summary>Steps run on the UI thread after the item data is built, each one a progress bar step.</summary>
	private Action[] UiBuildSteps => [
		.. this.CategoryLists.Select(c => (Action)(() => this.PopulateCategory(c.Category, c.List))),
		this.AdjustCheckBoxesWidth,
		this.AdjustCheckBoxesHeight,
		() => { this.SetSearchBoxVisibility(true); this.SyncNaveButton(); },
		this.LoadPersonnalQueries,
	];

	/// <summary>
	/// The dialog is kept between openings (see <c>MainForm.OpenSearchDialog</c>), and <see cref="Form.Shown"/> is only
	/// raised the first time, so each opening is detected here.
	/// </summary>
	private void SearchDialogVisibleChanged(object? sender, EventArgs e)
	{
		if (!this.Visible)
			return;

		this._openWatch.Restart();
		// Not awaited: the dialog stays responsive while the search data is built. The task never faults.
		this.BeginInvoke(() => _ = this.PrepareForSearchLoggedAsync());
	}

	/// <summary>
	/// Runs <see cref="PrepareForSearchAsync"/>, logging and showing its failure instead of letting it escape.
	/// </summary>
	private async Task PrepareForSearchLoggedAsync()
	{
		try
		{
			await this.PrepareForSearchAsync();
		}
		catch (Exception ex)
		{
			this.Log.LogError(ex, "Preparing the search dialog failed after {Elapsed} ms", this._openWatch.ElapsedMilliseconds);
			if (!this.IsDisposed)
			{
				this.scalingLabelProgress.Text = ex.Message;
				this.scalingLabelProgress.Visible = true;
			}
		}
	}

	/// <summary>
	/// Builds the categories on the first opening, and again only when the item database changed since then;
	/// otherwise the dialog is shown as it was left.
	/// </summary>
	private async Task PrepareForSearchAsync()
	{
		// Paint the dialog (VaultForm custom borders etc.) before the categories are built.
		this.Update();

		var database = this.ItemDatabaseService.ItemDatabase;
		if (ReferenceEquals(database, this._builtDatabase) && database.Count == this._builtCount)
		{
			this.Log.LogInformation("Search dialog reopened in {Elapsed} ms (items unchanged)", this._openWatch.ElapsedMilliseconds);
			this.scalingTextBoxSearchTerm.Focus();
			return;
		}

		if (this._buildingSearchData)
			return;

		if (this._builtDatabase is not null)
			this.ClearForRebuild();

		scalingLabelProgress.Text = Resources.SearchBuildingData;
		scalingLabelProgress.Visible = true;

		vaultProgressBar.Minimum = 0;
		vaultProgressBar.Value = 0;
		vaultProgressBar.Maximum = database.Count + this.UiBuildSteps.Length;
		vaultProgressBar.Visible = true;

		this._buildingSearchData = true;
		try
		{
			// Created on the UI thread, so the progress bar is updated there.
			var progress = new Progress<int>(this.vaultProgressBar.Increment);
			await Task.Run(() => this.ItemDatabaseService.PrepareForSearch(progress));
		}
		catch (Exception ex)
		{
			this.Log.LogError(ex, "Building the search data failed");
			scalingLabelProgress.Text = ex.Message;
			return;
		}
		finally
		{
			this._buildingSearchData = false;
		}

		this.SearchEngineReady();
	}

	/// <summary>True while <see cref="IItemDatabaseService.PrepareForSearch"/> runs.</summary>
	private bool _buildingSearchData;

	/// <summary>Empties the categories and the current selection, which refer to the previous items.</summary>
	private void ClearForRebuild()
	{
		this._Apply_SelectedFiltersDisabled = true;
		try
		{
			this.SetSearchBoxVisibility(false);
			this.CleanAllCheckBoxes();
			this.SelectedFilters.Clear();
			this.SelectedFiltersSet.Clear();
			this.QueryResults = [];
			this.scalingComboBoxQueryList.ResetText();
			this.ResetQuickFilters();
			this.ResetRequirements();
			this.TextBoxSearchTerm_UpdateText_Notrigger(string.Empty);
			this.scalingLabelFiltersSelected.Text = string.Empty;
		}
		finally
		{
			this._Apply_SelectedFiltersDisabled = false;
		}
	}

	/// <summary>Advances the progress bar and paints it now: the UI build steps keep the message loop busy.</summary>
	private void ProgressStep()
	{
		this.vaultProgressBar.Increment(1);
		this.vaultProgressBar.Update();
	}

	#endregion

	#region Load & Init

	private void SearchDialogAdvanced_Load(object? sender, EventArgs e)
	{
		// this.ItemDatabaseService.ItemDatabase is already populated during container loading via SessionContext
		// No need to rebuild it here - just lazy-load the friendly names
	}

	#endregion

	private void SearchEngineReady()
	{
		// Fill and size the categories, one progress step each, so the bar covers everything until the dialog is usable.
		var dataBuilt = this._openWatch.ElapsedMilliseconds;
		var stepTimes = new List<long>(this.UiBuildSteps.Length + 1);
		var stepWatch = System.Diagnostics.Stopwatch.StartNew();
		this.flowLayoutPanelMain.SuspendLayout();
		try
		{
			this._categoryItems = this.ItemDatabaseService.ItemDatabase.ToArray();
			foreach (var step in this.UiBuildSteps)
			{
				step();
				this.ProgressStep();
				stepTimes.Add(stepWatch.ElapsedMilliseconds);
				stepWatch.Restart();
			}
		}
		finally
		{
			this._categoryItems = [];
			this.flowLayoutPanelMain.ResumeLayout(true);
			stepTimes.Add(stepWatch.ElapsedMilliseconds);
		}
		this.FitCategoriesHeight();
		this.Log.LogInformation(
			"Search dialog build: data {DataMs} ms, UI steps [{Steps}] ms (last is ResumeLayout), list items [{Items}]"
			, dataBuilt
			, string.Join(", ", stepTimes)
			, string.Join(", ", this.CachedCheckedListBoxes.Select(lb => $"{lb.Name.Replace("scalingCheckedListBox", string.Empty)}={lb.Items.Count}")));

		this._builtDatabase = this.ItemDatabaseService.ItemDatabase;
		this._builtCount = this.ItemDatabaseService.ItemDatabase.Count;

		scalingLabelProgress.Text = $"{Resources.SearchEngineReady} - {string.Format(Resources.SearchItemCountIs, this._builtCount)}";
		this.Log.LogInformation("Search dialog ready in {Elapsed} ms ({Count} items)", this._openWatch.ElapsedMilliseconds, this._builtCount);

		// Start
		this.scalingTextBoxSearchTerm.Focus();
	}

	/// <summary>
	/// On a monitor of another DPI the fonts change: the category lists, sized from their text in code, are sized again.
	/// </summary>
	protected override void OnDpiChanged(DpiChangedEventArgs e)
	{
		base.OnDpiChanged(e);
		this.AdjustCheckBoxesWidth();
		this.AdjustCheckBoxesHeight();
		this.FitCategoriesHeight();
	}

	/// <summary>The user's zoom the dialog is scaled to: it is kept between openings, while the zoom may change.</summary>
	private float scaledZoom = 1.0F;

	/// <summary>
	/// Scales the dialog to the user's <paramref name="zoom"/>, as the other dialogs are when they open, from the zoom it
	/// was scaled to before; the category lists are sized again from their new fonts and the dialog kept on the screen.
	/// </summary>
	public void FollowZoom(float zoom)
	{
		if (zoom == this.scaledZoom)
			return;

		float factor = zoom / this.scaledZoom;
		using (ZoomScope.Enter())
			this.Scale(new SizeF(factor, factor));
		this.scaledZoom = zoom;

		this.AdjustCheckBoxesWidth();
		this.AdjustCheckBoxesHeight();
		var screen = Screen.FromControl(this.Owner ?? this).WorkingArea;
		this.Size = new Size(Math.Min(this.Width, screen.Width), Math.Min(this.Height, screen.Height));
		this.FitCategoriesHeight();
		this.LastFormSize = this.Size;
	}

	/// <summary>
	/// Override of ScaleControl which supports font scaling: the controls without a font of their own follow the dialog's.
	/// </summary>
	protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
	{
		this.Font = ZoomScope.ScaleFont(this.Font, factor.Height);
		base.ScaleControl(factor, specified);
	}

	/// <summary>
	/// Grows the dialog when the categories overflow their area by a few pixels: the rows are sized from the fonts, which
	/// do not scale exactly with the DPI (at 96 DPI the five rows took 10 pixels more than the area and both scroll bars
	/// showed). A larger overflow, from more visible elements per category, is left to the scroll bars.
	/// </summary>
	private void FitCategoriesHeight()
	{
		var area = this.flowLayoutPanelMainAutoScroll;
		area.PerformLayout();
		int overflow = this.flowLayoutPanelMain.Height + this.flowLayoutPanelMain.Margin.Vertical
			- (area.Height - area.Padding.Vertical);
		if (overflow <= 0 || overflow > area.Height / 10)
			return;

		var screen = Screen.FromControl(this).WorkingArea;
		int height = Math.Min(this.Height + overflow, screen.Height);
		int top = Math.Max(screen.Top, Math.Min(this.Top, screen.Bottom - height));
		this.SetBounds(this.Left, top, this.Width, height);
		this.LastFormSize = this.Size;
	}

	private void AdjustCheckBoxesWidth()
	{
		if (this.CachedCheckedListBoxes.Count == 0)
			return;

		var maxElement = (int)this.numericUpDownMaxElement.Value;
		foreach (var lb in this.CachedCheckedListBoxes)
			lb.AdjustToMaxTextWidth(maxElement);
	}

	#region Populate

	/// <summary>
	/// Label of the category that holds <paramref name="list"/>.
	/// </summary>
	private static ScalingLabel CategoryLabel(Control list)
		=> (list.Parent ?? throw new InvalidOperationException($"{list.Name} has no category."))
			.Controls.OfType<ScalingLabel>().First();

	#endregion

	private void numericUpDownMaxElement_ValueChanged(object? sender, EventArgs e)
		=> AdjustCheckBoxesHeight();

	private void buttonCollapseAll_Click(object? sender, EventArgs e)
		=> this.ShowCategoryLists(false);

	private void buttonExpandAll_Click(object? sender, EventArgs e)
		=> this.ShowCategoryLists(true);

	/// <summary>
	/// Shows or hides every category list in one batch: the panels are laid out and drawn once, not once per list.
	/// </summary>
	private void ShowCategoryLists(bool visible)
	{
		var containers = this.CachedCheckedListBoxes
			.Select(lb => lb.Parent)
			.OfType<Control>()
			.Distinct()
			.Prepend(this.flowLayoutPanelMain)
			.ToArray();

		using (new LayoutBatch(this.flowLayoutPanelMainAutoScroll, containers))
		{
			foreach (var lb in this.CachedCheckedListBoxes)
				lb.Visible = visible;
		}
	}

	private void scalingButtonMenu_Click(object? sender, EventArgs e)
	{
		// Toggle
		var (button, panel) = NavMap.First(m => object.ReferenceEquals(m.Button, sender));
		panel.Visible = !panel.Visible;

		SyncNaveButton();
	}

	private void scalingCheckedListBox_SelectedValueChanged(object? sender, EventArgs e)
		=> Sync_SelectedFilters();

	private void scalingLabelFiltersSelected_MouseEnter(object? sender, EventArgs e)
	{
		var ctrl = (Control)sender!;
		SearchFiltersTooltip.ShowTooltip(this.Services, ctrl, this.SelectedFilters, (SearchOperator)scalingComboBoxOperator.SelectedIndex);
	}

	private void scalingLabelFiltersSelected_MouseLeave(object? sender, EventArgs e)
		=> SearchFiltersTooltip.HideTooltip();

	bool _Apply_SelectedFiltersDisabled = false;

	private void scalingComboBoxOperator_SelectionChangeCommitted(object? sender, EventArgs e)
	{
		if (scalingCheckBoxReduceDuringSelection.Checked)
			ResetCheckBoxesToFirstLoad();

		Apply_SelectedFilters();
	}

	private void scalingCheckBoxReduceDuringSelection_CheckedChanged(object? sender, EventArgs e)
	{
		scalingCheckBoxReduceDuringSelection_LastChecked = !scalingCheckBoxReduceDuringSelection.Checked;
		ApplyCategoriesReducer();
		scalingCheckBoxReduceDuringSelection_LastChecked = scalingCheckBoxReduceDuringSelection.Checked;
	}

	private sealed class FilterCategoriesDetails
	{
		internal bool HasFilterCategory;
		internal bool IsRegex;
		internal string Search = string.Empty;
		internal Regex? Regex;
		internal bool RegexIsValid;
		internal string SearchRaw = string.Empty;
		internal string SearchRawPrevious = string.Empty;

		internal bool HasChanged => SearchRaw != SearchRawPrevious;
		internal bool IsMatch(string attributeText)
			=> (IsRegex && RegexIsValid)
				? Regex?.IsMatch(attributeText) ?? false
				: attributeText.ContainsIgnoreCase(Search);

		internal bool AvoidDisplayAttribute(string attributeText)
			=> HasFilterCategory && !IsMatch(attributeText);
	}
	private readonly FilterCategoriesDetails FilterCategories = new FilterCategoriesDetails();

	private void scalingButtonReset_Click(object? sender, EventArgs e)
		=> ResetSelectedFilters();

	/// <summary>Empties the category filter box and its state without waiting for the typing delay.</summary>
	private void ClearFilterCategories()
	{
		var f = this.FilterCategories;
		f.SearchRawPrevious = f.SearchRaw;
		f.SearchRaw = string.Empty;
		f.HasFilterCategory = false;
		this.scalingTextBoxFilterCategories.Text = string.Empty;
	}

	internal void ResetQuickFilters()
	{
		scalingCheckBoxIsSetItem.Checked =
		scalingCheckBoxHavingSkill.Checked =
		scalingCheckBoxHavingCharm.Checked =
		scalingCheckBoxHavingRelic.Checked =
		scalingCheckBoxHavingPrefix.Checked =
		scalingCheckBoxHavingSuffix.Checked = false;
	}

	private void TextBoxSearchTerm_UpdateText_Notrigger(string newText)
	{
		this.scalingTextBoxSearchTerm.TextChanged -= scalingTextBoxSearchTerm_TextChanged;

		this.scalingTextBoxSearchTerm.Text = newText;

		this.scalingTextBoxSearchTerm.TextChanged += scalingTextBoxSearchTerm_TextChanged;
	}

	private void scalingTextBoxSearchTerm_TextChanged(object? sender, EventArgs e)
		/// Wait for the end of typing by delaying the call to <see cref="scalingTextBoxSearchTerm_TextChanged_Idled"/>
		=> this.typeAssistantSearchBox.TextChanged();

	private void scalingTextBoxSearchTerm_TextChanged_Idled(object? sender, EventArgs e)
		=> TextBoxSearchTerm_TextChanged_Logic();

	private void scalingTextBoxFilterCategories_TextChanged(object? sender, EventArgs e)
		/// Wait for the end of typing by delaying the call to <see cref="typeAssistantFilterCategories_Idled"/>
		=> typeAssistantFilterCategories.TextChanged();

	private void scalingLabelProgress_MouseEnter(object? sender, EventArgs e)
	{
		var ctrl = (Control)sender!;
		FoundResultsTooltip.ShowTooltip(this.Services, ctrl, this.QueryResults);
	}

	private void scalingLabelProgress_MouseLeave(object? sender, EventArgs e)
		=> FoundResultsTooltip.HideTooltip();


	private void SearchQueriesInit()
	{
		scalingComboBoxQueryList.BeginUpdate();
		scalingComboBoxQueryList.Items.Clear();
		scalingComboBoxQueryList.Items.AddRange(this.SearchQueryService.Queries.ToArray());
		scalingComboBoxQueryList.EndUpdate();
	}

	private void scalingComboBoxQueryList_SelectedIndexChanged(object? sender, EventArgs e)
	{
		if (scalingComboBoxQueryList.SelectedItem is not SearchQuery query)
			return;

		Make_SelectedFilters(query, this.QueryFilters.GetValueOrDefault(query) ?? []);
	}

	private void scalingCheckBoxMinMaxReq_CheckedChanged(object? sender, EventArgs e)
	{
		Apply_SelectedFilters();
	}

	private void scalingCheckBoxQuickFilters_CheckedChanged(object? sender, EventArgs e)
	{
		Apply_SelectedFilters();
	}
}

public static class SearchDialogAdvancedExtension
{
	public static (VirtualCheckedListBox, BoxTag) GetBoxTag(this Control obj)
	{
		if (obj is not VirtualCheckedListBox ctr)
			throw new ArgumentException($"{obj.Name} is not a filter list.", nameof(obj));

		var Tag = ctr.Tag as BoxTag ?? new BoxTag();
		ctr.Tag = Tag;

		return (ctr, Tag);
	}

	public static (ScalingTextBox, BoxItem) GetBoxItem(this Control obj, bool newInstance)
	{
		if (obj is not ScalingTextBox ctr)
			throw new ArgumentException($"{obj.Name} is not a search box.", nameof(obj));

		var Tag = newInstance
			? new BoxItem()
			: ctr.Tag as BoxItem ?? new BoxItem();

		ctr.Tag = Tag;

		return (ctr, Tag);
	}
}