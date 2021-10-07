namespace TQVaultAE.GUI
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Threading;
	using System.Windows.Forms;
	using TQVaultAE.Domain.Contracts.Providers;
	using TQVaultAE.Domain.Contracts.Services;
	using TQVaultAE.Domain.Entities;
	using TQVaultAE.Domain.Search;
	using TQVaultAE.GUI.Components;
	using TQVaultAE.GUI.Helpers;
	using TQVaultAE.Presentation;
	using TQVaultAE.Domain.Helpers;
	using System.Drawing;
	using Microsoft.Extensions.Logging;
	using TQVaultAE.GUI.Models.SearchDialogAdvanced;
	using TQVaultAE.GUI.Tooltip;
	using Newtonsoft.Json;

	/// <summary>
	/// Class for the Search Dialog box.
	/// </summary>
	public partial class SearchDialogAdvanced : VaultForm
	{
		private readonly SessionContext Ctx;
		private readonly ITranslationService TranslationService;
		private readonly List<Result> ItemDatabase = new List<Result>();
		private readonly ILogger Log;
		private readonly Bitmap ButtonImageUp;
		private readonly Bitmap ButtonImageDown;
		private readonly (ScalingButton Button, FlowLayoutPanel Panel)[] _NavMap;
		private readonly List<BoxItem> _SelectedFilters = new List<BoxItem>();
		private readonly List<SearchQuery> _Queries = new List<SearchQuery>();
		public Result[] QueryResults { get; private set; } = new Result[] { };
		private bool scalingCheckBoxReduceDuringSelection_LastChecked;

		/// <summary>
		/// Initializes a new instance of the SearchDialog class.
		/// </summary>
		public SearchDialogAdvanced(
			MainForm instance
			, SessionContext sessionContext
			, IItemProvider itemProvider
			, ITranslationService translationService
			, ILogger<SearchDialogAdvanced> log
		) : base(instance.ServiceProvider)
		{
			this.Owner = instance;
			this.Ctx = sessionContext;
			this.TranslationService = translationService;
			this.Log = log;

			this.InitializeComponent();

			this.MinimizeBox = false;
			this.NormalizeBox = false;
			this.MaximizeBox = true;

			#region Apply custom font

			this.ProcessAllControls(c =>
			{
				if (c is IScalingControl || c is NumericUpDown) c.Font = FontService.GetFont(9F);
			});

			this.applyButton.Font = FontService.GetFontLight(12F);
			this.cancelButton.Font = FontService.GetFontLight(12F);

			this.Font = FontService.GetFont(9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte)(0));

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

			#endregion

			// Mapping between nav button & content component
			if (_NavMap is null)
			{
				_NavMap = new (ScalingButton Button, FlowLayoutPanel Panel)[] {
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
				};
			}

			// Keep reference of base Up & Down button image
			this.ButtonImageUp = this.scalingButtonMenuAttribute.UpBitmap;
			this.ButtonImageDown = this.scalingButtonMenuAttribute.DownBitmap;

			this.scalingComboBoxOperator.SelectedIndex = (int)SearchOperator.And;

			// Remove design time fake elements
			scalingComboBoxQueryList.Items.Clear();
			CleanAllCheckBoxes();
		}

		private void CleanAllCheckBoxes()
		{
			this.ProcessAllControls(c =>
			{
				if (c is ScalingCheckedListBox lb)
				{
					lb.BeginUpdate();
					lb.Items.Clear();
					lb.EndUpdate();
				}
			});
		}

		private void SetSearchBoxVisibility(bool isVisible)
			=> _NavMap.ToList().ForEach(m => m.Panel.Visible = isVisible);

		#region Apply & Cancel

		/// <summary>
		/// Handler for clicking the apply button on the form.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void ApplyButtonClicked(object sender, EventArgs e)
		{
			if (!_SelectedFilters.Any())
			{
				scalingLabelProgress.Text = $"{Resources.SearchTermRequired} - {string.Format(Resources.SearchItemCountIs, ItemDatabase.Count())}";
				return;
			};

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		/// <summary>
		/// Handler for clicking the cancel button on the form.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void CancelButtonClicked(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		#endregion

		/// <summary>
		/// Handler for showing the search dialog.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void SearchDialogShown(object sender, EventArgs e)
		{
			Application.DoEvents();// Force control rendering (VaultForm stuff like custom borders etc...)

			// Init Data Base
			scalingLabelProgress.Text = Resources.SearchBuildingData;
			scalingLabelProgress.Visible = true;

			vaultProgressBar.Minimum = 0;
			vaultProgressBar.Maximum = ItemDatabase.Count();
			vaultProgressBar.Visible = true;

			this.backgroundWorkerBuildDB.RunWorkerAsync();
		}

		#region Load & Init

		private void SearchDialogAdvanced_Load(object sender, EventArgs e)
			=> BuildItemDatabase();

		/// <summary>
		/// Seek for all available items
		/// </summary>
		private void BuildItemDatabase()
		{
			foreach (KeyValuePair<string, Lazy<PlayerCollection>> kvp in Ctx.Vaults)
			{
				string vaultFile = kvp.Key;
				PlayerCollection vault = kvp.Value.Value;

				if (vault == null)
					continue;

				int vaultNumber = -1;
				foreach (SackCollection sack in vault)
				{
					vaultNumber++;
					if (sack == null)
						continue;

					foreach (var item in sack.Cast<Item>())
					{
						ItemDatabase.Add(new Result(
							vaultFile
							, Path.GetFileNameWithoutExtension(vaultFile)
							, vaultNumber
							, SackType.Vault
							, new Lazy<Domain.Results.ToFriendlyNameResult>(
								() => ItemProvider.GetFriendlyNames(item, FriendlyNamesExtraScopes.ItemFullDisplay)
								, LazyThreadSafetyMode.ExecutionAndPublication
							)
						));
					}
				}
			}

			foreach (KeyValuePair<string, Lazy<PlayerCollection>> kvp in Ctx.Players)
			{
				string playerFile = kvp.Key;
				PlayerCollection player = kvp.Value.Value;

				if (player == null)
					continue;

				string playerName = this.GamePathResolver.GetNameFromFile(playerFile);
				if (playerName == null)
					continue;

				int sackNumber = -1;
				foreach (SackCollection sack in player)
				{
					sackNumber++;
					if (sack == null)
						continue;

					foreach (var item in sack.Cast<Item>())
					{
						this.ItemDatabase.Add(new Result(
							playerFile
							, playerName
							, sackNumber
							, SackType.Player
							, new Lazy<Domain.Results.ToFriendlyNameResult>(
								() => ItemProvider.GetFriendlyNames(item, FriendlyNamesExtraScopes.ItemFullDisplay)
								, LazyThreadSafetyMode.ExecutionAndPublication
							)
						));
					}
				}

				// Now search the Equipment panel
				var equipmentSack = player.EquipmentSack;
				if (equipmentSack == null)
					continue;

				foreach (var item in equipmentSack.Cast<Item>())
				{
					ItemDatabase.Add(new Result(
						playerFile
						, playerName
						, 0
						, SackType.Equipment
						, new Lazy<Domain.Results.ToFriendlyNameResult>(
							() => ItemProvider.GetFriendlyNames(item, FriendlyNamesExtraScopes.ItemFullDisplay)
							, LazyThreadSafetyMode.ExecutionAndPublication
						)
					));
				}
			}

			foreach (KeyValuePair<string, Lazy<Stash>> kvp in Ctx.Stashes)
			{
				string stashFile = kvp.Key;
				Stash stash = kvp.Value.Value;

				// Make sure we have a valid name and stash.
				if (stash == null)
					continue;

				string stashName = this.GamePathResolver.GetNameFromFile(stashFile);
				if (stashName == null)
					continue;

				SackCollection sack = stash.Sack;
				if (sack == null)
					continue;

				int sackNumber = 2;
				SackType sackType = SackType.Stash;
				if (stashName == Resources.GlobalTransferStash)
				{
					sackNumber = 1;
					sackType = SackType.TransferStash;
				}
				else if (stashName == Resources.GlobalRelicVaultStash)
				{
					sackNumber = 3;
					sackType = SackType.RelicVaultStash;
				}

				foreach (var item in sack.Cast<Item>())
				{
					ItemDatabase.Add(new Result(
						stashFile
						, stashName
						, sackNumber
						, sackType
						, new Lazy<Domain.Results.ToFriendlyNameResult>(
							() => ItemProvider.GetFriendlyNames(item, FriendlyNamesExtraScopes.ItemFullDisplay)
							, LazyThreadSafetyMode.ExecutionAndPublication
						)
					));
				}
			}
		}

		/// <summary>
		/// Load item data & display progress
		/// </summary>
		private void InitItemDatabase()
		{
			// Must not change UI Controls. Just update backgroundWorker which handle this for you through his event pipeline.
			foreach (var item in ItemDatabase)
			{
				item.LazyLoad();
				this.backgroundWorkerBuildDB.ReportProgress(1);
			}
			// Cleanup zombies
			ItemDatabase.RemoveAll(id => string.IsNullOrWhiteSpace(id.ItemName));
		}

		#endregion

		#region backgroundWorkerBuildDB

		private void backgroundWorkerBuildDB_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
			=> InitItemDatabase();

		private void backgroundWorkerBuildDB_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
			=> vaultProgressBar.Increment(1);

		private void backgroundWorkerBuildDB_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			SearchEngineReady();

			LoadPersonnalQueries();
		}

		#endregion

		private void SearchEngineReady()
		{
			scalingLabelProgress.Text = $"{Resources.SearchEngineReady} - {string.Format(Resources.SearchItemCountIs, ItemDatabase.Count())}";

			PopulateCheckBoxes();

			AdjustCheckBoxesWidth();

			AdjustCheckBoxesHeight();

			SetSearchBoxVisibility(true);

			SyncNaveButton();

			// Start
			this.scalingTextBoxSearchTerm.Focus();
		}

		private void AdjustCheckBoxesHeight()
		{
			flowLayoutPanelMain.ProcessAllControls(c =>
			{
				// Adjust to current UI setting
				if (c is ScalingCheckedListBox lb)
				{
					var maxRow = (int)this.numericUpDownMaxElement.Value;
					int height = 0, currrow = 0;
					foreach (var line in lb.Items)
					{
						if (currrow == maxRow) break;
						height += lb.GetItemHeight(currrow);
						currrow++;
					}
					height += SystemInformation.HorizontalScrollBarHeight;
					lb.Height = height;
				}
			});
		}

		private void AdjustCheckBoxesWidth()
		{
			flowLayoutPanelMain.ProcessAllControls(c =>
			{
				if (c is ScalingCheckedListBox lb)
					lb.AdjustToMaxTextWidth((int)this.numericUpDownMaxElement.Value);
			});
		}

		#region Populate

		private void PopulateCheckBoxes()
		{
			PopulateCharacters();
			PopulateItemType();
			PopulateRarity();
			PopulateStyle();
			PopulateQuality();
			PopulateVaults();
			PopulateWithCharm();
			PopulateWithRelic();

			PopulateItemAttributes();
			PopulateBaseAttributes();

			PopulatePrefixName();
			PopulatePrefixAttributes();

			PopulateSuffixName();
			PopulateSuffixAttributes();

		}

		private void PopulateWithRelic()
		{
			var clb = scalingCheckedListBoxWithRelic;
			var WithRelic =
				from id in ItemDatabase
				let itm = id.FriendlyNames.Item
				where itm.HasRelicSlot1 && !itm.IsRelic1Charm || itm.HasRelicSlot2 && !itm.IsRelic2Charm
				from reldesc in new[] { id.FriendlyNames.RelicInfo1Description, id.FriendlyNames.RelicInfo2Description }
				where !string.IsNullOrWhiteSpace(reldesc)
				let attClean = reldesc.RemoveAllTQTags().Trim()
				orderby attClean
				group id by attClean into grp
				select new BoxItem
				{
					DisplayValue = grp.Key,
					MatchingResults = grp,
					CheckedList = clb,
					Category = clb.Parent.Controls.OfType<ScalingLabel>().First(),
				};
			PopulateInit(clb, WithRelic);
		}

		private static void PopulateInit(ScalingCheckedListBox clb, IEnumerable<BoxItem> items)
		{
			var (_, tag) = clb.GetBoxTag();
			tag.DataSource = items.ToArray();
			clb.BeginUpdate();
			clb.Items.AddRange(tag.DataSource);
			clb.EndUpdate();
		}

		private void PopulateWithCharm()
		{
			var clb = scalingCheckedListBoxWithCharm;
			var WithCharm =
				from id in ItemDatabase
				let itm = id.FriendlyNames.Item
				where itm.HasRelicSlot1 && itm.IsRelic1Charm || itm.HasRelicSlot2 && itm.IsRelic2Charm
				from reldesc in new[] { id.FriendlyNames.RelicInfo1Description, id.FriendlyNames.RelicInfo2Description }
				where !string.IsNullOrWhiteSpace(reldesc)
				let attClean = reldesc.RemoveAllTQTags().Trim()
				orderby attClean
				group id by attClean into grp
				select new BoxItem
				{
					DisplayValue = grp.Key,
					MatchingResults = grp,
					CheckedList = clb,
					Category = clb.Parent.Controls.OfType<ScalingLabel>().First(),
				};
			PopulateInit(clb, WithCharm);
		}

		private void PopulateVaults()
		{
			var clb = scalingCheckedListBoxVaults;
			var Vaults =
				from id in ItemDatabase.Where(i => i.SackType == SackType.Vault)
				let att = id.ContainerName
				orderby att
				group id by att into grp
				select new BoxItem
				{
					DisplayValue = grp.Key,
					MatchingResults = grp,
					CheckedList = clb,
					Category = clb.Parent.Controls.OfType<ScalingLabel>().First(),
				};
			PopulateInit(clb, Vaults);
		}

		private void PopulateQuality()
		{
			var clb = scalingCheckedListBoxQuality;
			var Quality =
				from id in ItemDatabase
				let att = id.FriendlyNames.BaseItemInfoQuality
				where !string.IsNullOrWhiteSpace(att)
				let attClean = att.RemoveAllTQTags().Trim()
				orderby attClean
				group id by attClean into grp
				select new BoxItem
				{
					DisplayValue = grp.Key,
					MatchingResults = grp,
					CheckedList = clb,
					Category = clb.Parent.Controls.OfType<ScalingLabel>().First(),
				};
			PopulateInit(clb, Quality);
		}

		private void PopulateSuffixName()
		{
			var clb = scalingCheckedListBoxSuffixName;
			var SuffixName =
				from id in ItemDatabase
				let att = id.FriendlyNames.SuffixInfoDescription
				where !string.IsNullOrWhiteSpace(att)
				let attClean = att.RemoveAllTQTags().Trim()
				orderby attClean
				group id by attClean into grp
				select new BoxItem
				{
					DisplayValue = grp.Key,
					MatchingResults = grp,
					CheckedList = clb,
					Category = clb.Parent.Controls.OfType<ScalingLabel>().First(),
				};
			PopulateInit(clb, SuffixName);
		}

		private void PopulateSuffixAttributes()
		{
			var clb = scalingCheckedListBoxSuffixAttributes;
			var SuffixAttributes =
				from id in ItemDatabase
				from att in id.FriendlyNames.SuffixAttributes
				where !string.IsNullOrWhiteSpace(att)
				let attClean = att.RemoveAllTQTags().Trim()
				orderby attClean
				group id by attClean into grp
				select new BoxItem
				{
					DisplayValue = grp.Key,
					MatchingResults = grp,
					CheckedList = clb,
					Category = clb.Parent.Controls.OfType<ScalingLabel>().First(),
				};
			PopulateInit(clb, SuffixAttributes);
		}

		private void PopulateStyle()
		{
			var clb = scalingCheckedListBoxStyle;
			var Style =
				from id in ItemDatabase
				let att = id.FriendlyNames.BaseItemInfoStyle
				where !string.IsNullOrWhiteSpace(att)
				let attClean = att.RemoveAllTQTags().Trim()
				orderby attClean
				group id by attClean into grp
				select new BoxItem
				{
					DisplayValue = grp.Key,
					MatchingResults = grp,
					CheckedList = clb,
					Category = clb.Parent.Controls.OfType<ScalingLabel>().First(),
				};
			PopulateInit(clb, Style);
		}

		private void PopulateRarity()
		{
			var equipmentOnly = new[] {
				ItemStyle.Broken
				, ItemStyle.Mundane
				, ItemStyle.Common
				, ItemStyle.Rare
				, ItemStyle.Epic
				, ItemStyle.Legendary
			};
			var clb = scalingCheckedListBoxRarity;
			var Rarity =
				from id in ItemDatabase
				where equipmentOnly.Contains(id.ItemStyle)
				let att = id.FriendlyNames.BaseItemRarity
				where !string.IsNullOrWhiteSpace(att)
				let attClean = att.RemoveAllTQTags().Trim()
				orderby attClean
				group id by attClean into grp
				select new BoxItem
				{
					DisplayValue = grp.Key,
					MatchingResults = grp,
					CheckedList = clb,
					Category = clb.Parent.Controls.OfType<ScalingLabel>().First(),
				};
			PopulateInit(clb, Rarity);
		}

		private void PopulatePrefixName()
		{
			var clb = scalingCheckedListBoxPrefixName;
			var PrefixName =
				from id in ItemDatabase
				let att = id.FriendlyNames.PrefixInfoDescription
				where !string.IsNullOrWhiteSpace(att)
				let attClean = att.TQCleanup().Trim()
				orderby attClean
				group id by attClean into grp
				select new BoxItem
				{
					DisplayValue = grp.Key,
					MatchingResults = grp,
					CheckedList = clb,
					Category = clb.Parent.Controls.OfType<ScalingLabel>().First(),
				};
			PopulateInit(clb, PrefixName);
		}

		private void PopulatePrefixAttributes()
		{
			var clb = scalingCheckedListBoxPrefixAttributes;
			var PrefixAttributes =
				from id in ItemDatabase
				from att in id.FriendlyNames.PrefixAttributes
				where !string.IsNullOrWhiteSpace(att)
				let attClean = att.RemoveAllTQTags().Trim()
				orderby attClean
				group id by attClean into grp
				select new BoxItem
				{
					DisplayValue = grp.Key,
					MatchingResults = grp,
					CheckedList = clb,
					Category = clb.Parent.Controls.OfType<ScalingLabel>().First(),
				};
			PopulateInit(clb, PrefixAttributes);
		}

		private void PopulateCharacters()
		{
			var clb = scalingCheckedListBoxCharacters;
			var Players =
				from id in ItemDatabase
				where id.SackType == SackType.Player || id.SackType == SackType.Equipment
				let att = id.ContainerName
				orderby att
				group id by att into grp
				select new BoxItem
				{
					DisplayValue = grp.Key,
					MatchingResults = grp,
					CheckedList = clb,
					Category = clb.Parent.Controls.OfType<ScalingLabel>().First(),
				};
			PopulateInit(clb, Players);
		}

		private void PopulateItemType()
		{
			var clb = scalingCheckedListBoxItemType;
			var ItemType =
				from id in ItemDatabase
				let att = id.FriendlyNames.BaseItemInfoClass ?? id.FriendlyNames.Item.ItemClass
				where !string.IsNullOrWhiteSpace(att)
				let attClean = att.RemoveAllTQTags().Trim()
				orderby attClean
				group id by attClean into grp
				select new BoxItem
				{
					DisplayValue = grp.Key,
					MatchingResults = grp,
					CheckedList = clb,
					Category = clb.Parent.Controls.OfType<ScalingLabel>().First(),
				};
			PopulateInit(clb, ItemType);
		}

		private void PopulateItemAttributes()
		{
			var clb = scalingCheckedListBoxItemAttributes;
			var ItemAttributes =
				from id in ItemDatabase
				from att in id.FriendlyNames.AttributesAll
				where !string.IsNullOrWhiteSpace(att)
				let attClean = att.RemoveAllTQTags().Trim()
				orderby attClean
				group id by attClean into grp
				select new BoxItem
				{
					DisplayValue = grp.Key,
					MatchingResults = grp,
					CheckedList = clb,
					Category = clb.Parent.Controls.OfType<ScalingLabel>().First(),
				};
			PopulateInit(clb, ItemAttributes);
		}

		private void PopulateBaseAttributes()
		{
			var clb = scalingCheckedListBoxBaseAttributes;
			var BaseAttributes =
				from id in ItemDatabase
				from att in id.FriendlyNames.BaseAttributes
				where !string.IsNullOrWhiteSpace(att)
				let attClean = att.RemoveAllTQTags().Trim()
				orderby attClean
				group id by attClean into grp
				select new BoxItem
				{
					DisplayValue = grp.Key,
					MatchingResults = grp,
					CheckedList = clb,
					Category = clb.Parent.Controls.OfType<ScalingLabel>().First(),
				};
			PopulateInit(clb, BaseAttributes);
		}

		#endregion

		private void numericUpDownMaxElement_ValueChanged(object sender, EventArgs e)
			=> AdjustCheckBoxesHeight();

		private void scalingCheckedListBox_MouseMove(object sender, MouseEventArgs e)
		{
			var ctr = sender as Control;
			var (lstBox, tag) = ctr.GetBoxTag();

			var focusedIdx = lstBox.IndexFromPoint(e.Location);

			if (tag.LastTooltipIndex != focusedIdx)
			{
				tag.LastTooltipIndex = focusedIdx;
				if (tag.LastTooltipIndex > -1)
				{
					var item = lstBox.Items[focusedIdx] as BoxItem;
					toolTip.SetToolTip(lstBox, string.Format(Resources.SearchMatchingItemsTT, item.MatchingResults.Count()));
				}
			}

			lstBox.Tag = tag;
		}

		private void buttonCollapseAll_Click(object sender, EventArgs e)
		{
			flowLayoutPanelMain.ProcessAllControls(c =>
			{
				if (c is ScalingCheckedListBox lb)
					lb.Visible = false;
			});
		}

		private void buttonExpandAll_Click(object sender, EventArgs e)
		{
			flowLayoutPanelMain.ProcessAllControls(c =>
			{
				if (c is ScalingCheckedListBox lb)
					lb.Visible = true;
			});
		}

		private void scalingButtonMenu_Click(object sender, EventArgs e)
		{
			// Toggle
			var (button, panel) = _NavMap.First(m => object.ReferenceEquals(m.Button, sender));
			panel.Visible = !panel.Visible;

			SyncNaveButton();
		}

		private void SyncNaveButton()
		{
			/// Push invisible categories at the end of <see cref="flowLayoutPanelMain"/> so nav buttons define categories order in flowpanel
			var trail = _NavMap.Where(map => !map.Panel.Visible).Select(map => map.Panel).ToList();
			// Remove
			trail.ForEach(c => this.flowLayoutPanelMain.Controls.Remove(c));
			// Then Put it back at the end
			this.flowLayoutPanelMain.Controls.AddRange(trail.ToArray());

			SyncNavButtonImage();
		}

		private void SyncNavButtonImage()
		{
			foreach (var map in _NavMap)
			{
				// Invert Up & Down each time you click to make it behave like a toggle
				if (map.Panel.Visible)
				{
					map.Button.UpBitmap = this.ButtonImageDown;
					map.Button.OverBitmap = this.ButtonImageUp;
					map.Button.Image = map.Button.DownBitmap;
				}
				else
				{
					map.Button.UpBitmap = this.ButtonImageUp;
					map.Button.OverBitmap = this.ButtonImageDown;
					map.Button.Image = map.Button.UpBitmap;
				}
			}
		}
		private void scalingCheckedListBox_SelectedValueChanged(object sender, EventArgs e)
			=> Sync_SelectedFilters();

		private void Sync_SelectedFilters()
		{
			_SelectedFilters.Clear();

			// Add the SearchTerm on top 
			var (_, searchTermBoxItem) = this.scalingTextBoxSearchTerm.GetBoxItem(false);
			// if i get something to filter with
			if (!string.IsNullOrWhiteSpace(searchTermBoxItem.DisplayValue))
				_SelectedFilters.Add(searchTermBoxItem);

			// Crawl winform graf for selected BoxItem
			flowLayoutPanelMain.ProcessAllControls(c =>
			{
				if (c is ScalingCheckedListBox lb)
					_SelectedFilters.AddRange(lb.CheckedItems.Cast<BoxItem>());
			});

			this.Apply_SelectedFilters();
		}

		private void scalingLabelCategory_MouseClick(object sender, MouseEventArgs e)
		{
			var label = sender as ScalingLabel;
			var flowpanel = label.Parent;
			if (e.Button == MouseButtons.Left)
			{
				// Individual Expand/Collapse
				flowpanel.ProcessAllControls(c =>
				{
					if (c is ScalingCheckedListBox lb)
						lb.Visible = !lb.Visible;
				});
			}
			// Uncheck
			if (e.Button == MouseButtons.Right)
			{
				UncheckCategories(flowpanel);
				Sync_SelectedFilters();
			}
		}

		private void UncheckCategories(Control flowpanel)
		{
			flowpanel.ProcessAllControls(c =>
			{
				if (c is ScalingCheckedListBox lb)
				{
					lb.BeginUpdate();
					lb.CheckedIndices.Cast<int>().ToList().ForEach(idx => lb.SetItemChecked(idx, false));
					lb.EndUpdate();
				}
			});
		}

		private void scalingLabelFiltersSelected_MouseEnter(object sender, EventArgs e)
		{
			var ctrl = sender as Control;
			SearchFiltersTooltip.ShowTooltip(this.ServiceProvider, ctrl, this._SelectedFilters, (SearchOperator)scalingComboBoxOperator.SelectedIndex);
		}

		private void scalingLabelFiltersSelected_MouseLeave(object sender, EventArgs e)
			=> SearchFiltersTooltip.HideTooltip();

		private void Apply_SelectedFilters()
		{
			this.scalingLabelFiltersSelected.Text = string.Format(this.scalingLabelFiltersSelected.Tag.ToString(), _SelectedFilters.Count());

			var query = ItemDatabase.AsQueryable();
			if (this.scalingComboBoxOperator.SelectedIndex == (int)SearchOperator.And)
			{
				// AND operator => item must exist in every filter
				foreach (var filter in _SelectedFilters)
					query = query.Intersect(filter.MatchingResults);// Reducing result at every step
			}
			else
			{
				// OR Operator => Accumulate & Distinct
				query = _SelectedFilters.AsQueryable().SelectMany(f => f.MatchingResults).Distinct();
			}

			this.QueryResults = query.ToArray();

			scalingLabelProgress.Text = $"{string.Format(Resources.SearchItemCountIs, QueryResults.Count())}";

			ApplyCategoriesReducer();
		}

		private void scalingComboBoxOperator_SelectionChangeCommitted(object sender, EventArgs e)
		{
			if (scalingCheckBoxReduceDuringSelection.Checked)
				ResetCheckBoxesToFirstLoad();

			Apply_SelectedFilters();
		}

		private void scalingCheckBoxReduceDuringSelection_CheckedChanged(object sender, EventArgs e)
		{
			scalingCheckBoxReduceDuringSelection_LastChecked = !scalingCheckBoxReduceDuringSelection.Checked;
			ApplyCategoriesReducer();
			scalingCheckBoxReduceDuringSelection_LastChecked = scalingCheckBoxReduceDuringSelection.Checked;
		}

		private void ApplyCategoriesReducer()
		{
			// Category Reduced Display
			if (scalingCheckBoxReduceDuringSelection.Checked)
			{
				ReduceCheckBoxesToQueryResult();
				return;
			}

			if (
				// Category Full Display
				!scalingCheckBoxReduceDuringSelection.Checked
				/// But comes from <see cref="scalingCheckBoxReduceDuringSelection_CheckedChanged"/> meaning "from a Category Reduced Display"
				&& scalingCheckBoxReduceDuringSelection.Checked != scalingCheckBoxReduceDuringSelection_LastChecked
			)
			{
				ResetCheckBoxesToFirstLoad();// Restore Full Display
			}
		}

		private void ResetCheckBoxesToFirstLoad()
		{
			// Reset to FirstLoad
			CleanAllCheckBoxes();
			flowLayoutPanelMain.ProcessAllControls(c =>
			{
				if (c is ScalingCheckedListBox lb)
				{
					var (_, tag) = lb.GetBoxTag();
					lb.BeginUpdate();
					foreach (var item in tag.DataSource)
						lb.Items.Add(item, _SelectedFilters.Contains(item));
					lb.EndUpdate();
				}
			});
		}

		private void ReduceCheckBoxesToQueryResult()
		{
			if (this.QueryResults.Any())
			{
				CleanAllCheckBoxes();
				// Reset to QueryResult.
				flowLayoutPanelMain.ProcessAllControls(c =>
				{
					if (c is ScalingCheckedListBox lb)
					{
						var (_, tag) = lb.GetBoxTag();

						var DSvsQR =
							// Does boxitems match queryresult ?
							from boxitem in tag.DataSource
							from boxitemresult in boxitem.MatchingResults
							join QR in this.QueryResults on boxitemresult equals QR
							// distinct boxitems
							group boxitem by boxitem into grp
							select grp;

						lb.BeginUpdate();
						foreach (var item in DSvsQR)
							lb.Items.Add(item.Key, _SelectedFilters.Contains(item.Key));
						lb.EndUpdate();
					}
				});
			}
		}

		private void scalingButtonReset_Click(object sender, EventArgs e)
			=> ResetSelectedFilters();

		private void ResetSelectedFilters()
		{
			this.scalingComboBoxQueryList.ResetText();
			UncheckCategories(flowLayoutPanelMain);
			TextBoxSearchTerm_UpdateText_Notrigger(string.Empty);
			TextBoxSearchTerm_TextChanged_Logic();
		}

		private void TextBoxSearchTerm_UpdateText_Notrigger(string newText)
		{
			this.scalingTextBoxSearchTerm.TextChanged -= new System.EventHandler(this.scalingTextBoxSearchTerm_TextChanged);

			this.scalingTextBoxSearchTerm.Text = newText;

			this.scalingTextBoxSearchTerm.TextChanged += new System.EventHandler(this.scalingTextBoxSearchTerm_TextChanged);
		}

		private void scalingTextBoxSearchTerm_TextChanged(object sender, EventArgs e)
			/// Wait for the end of typing by delaying the call to <see cref="scalingTextBoxSearchTerm_TextChanged_Idled"/>
			=> this.typeAssistant.TextChanged();

		private void scalingTextBoxSearchTerm_TextChanged_Idled(object sender, EventArgs e)
			=> TextBoxSearchTerm_TextChanged_Logic();

		private void TextBoxSearchTerm_TextChanged_Logic()
		{
			MakeSearchTermBoxItem();

			/// Wrapped into an Invoke() because i'm currently in the thread of the 
			/// <see cref="typeAssistant"> and i need  <see cref="Sync_SelectedFilters"/> to be executed from the main thread to avoid concurrent access exception
			this.Invoke(new MethodInvoker(() => Sync_SelectedFilters()));
		}

		private BoxItem MakeSearchTermBoxItem(string searchTerm = null)
		{
			// Init a special BoxItem for the search term
			var txt = searchTerm is null
				? scalingTextBoxSearchTerm.Text.Trim()
				: searchTerm.Trim();

			var (_, searchTermBoxItem) = scalingTextBoxSearchTerm.GetBoxItem(true);// true : i need to make a new intance here to make the SearchQuery remember search term (must not share same object reference)

			searchTermBoxItem.Category = scalingLabelSearchTerm;
			searchTermBoxItem.DisplayValue = txt;

			if (string.IsNullOrWhiteSpace(txt))
				searchTermBoxItem.MatchingResults = Enumerable.Empty<Result>();
			else
			{
				// Item fulltext search
				searchTermBoxItem.MatchingResults = (
					from id in ItemDatabase
					where id.FriendlyNames.FullText.IndexOf(txt, StringComparison.OrdinalIgnoreCase) > -1
					select id
				).ToArray();
			}

			// When this method is used as a BoxItem factory, i don't want the textbox to keep the reference.
			if (searchTerm != null)
				scalingTextBoxSearchTerm.Tag = null;

			return searchTermBoxItem;
		}

		private void scalingLabelProgress_MouseEnter(object sender, EventArgs e)
		{
			var ctrl = sender as Control;
			FoundResultsTooltip.ShowTooltip(this.ServiceProvider, ctrl, this.QueryResults);
		}

		private void scalingLabelProgress_MouseLeave(object sender, EventArgs e)
			=> FoundResultsTooltip.HideTooltip();

		private void scalingLabelProgress_TextChanged(object sender, EventArgs e)
		{
			var ctrl = sender as Control;
			ScalingLabelProgressAdjustSizeAndPosition(ctrl);
		}

		private static void ScalingLabelProgressAdjustSizeAndPosition(Control ctrl)
		{
			// Adjust Control Size & position manualy because center align the control in it's container doesn't work with AutoSize = true
			ctrl.Size = TextRenderer.MeasureText(ctrl.Text, ctrl.Font);
			var loc = ctrl.Location;
			loc.X = (ctrl.Parent.Size.Width / 2) - (ctrl.Width / 2);
			ctrl.Location = loc;
		}

		private void scalingLabelProgressPanelAlignText_SizeChanged(object sender, EventArgs e)
		{
			// Prevent child text truncation during resize
			var ctrl = sender as Control;
			var label = ctrl.Controls.OfType<ScalingLabel>().First();
			ScalingLabelProgressAdjustSizeAndPosition(label);
		}

		private void SearchDialogAdvanced_FormClosing(object sender, FormClosingEventArgs e)
			=> SavePersonnalQueries();

		private void SavePersonnalQueries()
		{
			Config.Settings.Default.SearchQueries = JsonConvert.SerializeObject(this._Queries);
			Config.Settings.Default.Save();
		}

		private void LoadPersonnalQueries()
		{
			var save = Config.Settings.Default.SearchQueries;
			if (string.IsNullOrWhiteSpace(save))
				return;

			var queries = JsonConvert.DeserializeObject<SearchQuery[]>(save);

			// Try to retrieve actual instantiated BoxItems related to saved data.
			if (queries.Any())
			{
				var matrix = (
					from query in queries
					from boxi in query.CheckedItems
					select new
					{
						// Source
						query,
						query.QueryName,
						boxiSave = boxi,
						// Join
						boxi.DisplayValue,
						boxi.CategoryName,
						boxi.CheckedListName,
						found = new List<BoxItem>() // Late binding placeholder because anonymous types are immutable
					}
				).ToArray();

				// Retrieve CheckBoxes
				flowLayoutPanelMain.ProcessAllControls(c =>
				{
					if (c is ScalingCheckedListBox lb)
					{
						var (_, tag) = lb.GetBoxTag();

						(
							// Align Saved & Live BoxItems
							from ds in tag.DataSource
							join m in matrix on new { ds.CategoryName, ds.CheckedListName, ds.DisplayValue } equals new { m.CategoryName, m.CheckedListName, m.DisplayValue }
							select new { boxiLive = ds, matrix = m }
						).ToList().ForEach(r => r.matrix.found.Add(r.boxiLive));// Bind
					}
				});

				// Make Search terms
				(
					from m in matrix
					where m.CategoryName == scalingLabelSearchTerm.Name
					select new { boxiLive = MakeSearchTermBoxItem(m.DisplayValue), matrix = m }
				).ToList().ForEach(r => r.matrix.found.Add(r.boxiLive));// Bind

				// Make newList
				var newList = (
					from m in matrix
					where m.found.Any() // Saved boxitems may not be retrieved if you have lost the items that carry the corresponding properties.
					group m by m.QueryName into grp
					select new SearchQuery
					{
						QueryName = grp.Key,
						CheckedItems = grp.SelectMany(i => i.found).ToArray()
					}
				).ToList();

				SearchQueriesInit(newList);
			}
		}

		private void scalingButtonQuerySave_Click(object sender, EventArgs e)
		{
			var input = scalingComboBoxQueryList.Text.Trim();
			DialogResult? overrideIt = null;
			SearchQuery foundIt = null;

			#region Validation

			// You must have text
			if (string.IsNullOrWhiteSpace(input))
			{
				MessageBox.Show(
					Resources.SearchQueryNameMustBeSet
					, Resources.GlobalInputWarning
					, MessageBoxButtons.OK
					, MessageBoxIcon.Error
					, MessageBoxDefaultButton.Button1
					, RightToLeftOptions
				);
				return;
			}

			// You must have filters
			if (!this._SelectedFilters.Any())
			{
				MessageBox.Show(
					Resources.SearchTermRequired
					, Resources.GlobalInputWarning
					, MessageBoxButtons.OK
					, MessageBoxIcon.Error
					, MessageBoxDefaultButton.Button1
					, RightToLeftOptions
				);
				return;
			}

			// Name conflict
			foundIt = this._Queries.FirstOrDefault(q => q.QueryName.Equals(input, StringComparison.OrdinalIgnoreCase));
			if (foundIt != null)
			{
				overrideIt = MessageBox.Show(
					Resources.SearchQueryNameAlreadyExist
					, Resources.GlobalInputWarning
					, MessageBoxButtons.YesNo
					, MessageBoxIcon.Warning
					, MessageBoxDefaultButton.Button1
					, RightToLeftOptions
				);

				if (overrideIt == DialogResult.No)
					return;
			}

			#endregion

			if (overrideIt == DialogResult.Yes)
			{
				foundIt.QueryName = input;
				foundIt.CheckedItems = this._SelectedFilters.ToArray();// i need a clone here so ToArray() do the job
				scalingComboBoxQueryList.Refresh();
				return;
			}

			// Add scenario
			var newList = new IEnumerable<SearchQuery>[] {
				this._Queries
				, new[] { new SearchQuery {
					QueryName = input,
					CheckedItems = this._SelectedFilters.ToArray(),// i need a clone here so ToArray() do the job
				} }
			}
			.SelectMany(s => s)
			.OrderBy(s => s.QueryName)
			.ToArray();

			SearchQueriesInit(newList);
		}

		private void SearchQueriesInit(IEnumerable<SearchQuery> newList)
		{
			this._Queries.Clear();
			this._Queries.AddRange(newList);

			scalingComboBoxQueryList.BeginUpdate();
			scalingComboBoxQueryList.Items.Clear();
			scalingComboBoxQueryList.Items.AddRange(this._Queries.ToArray());
			scalingComboBoxQueryList.EndUpdate();
		}

		private void scalingButtonQueryDelete_Click(object sender, EventArgs e)
		{
			var idx = scalingComboBoxQueryList.SelectedIndex;
			if (idx == -1) return;

			var item = scalingComboBoxQueryList.SelectedItem;

			var deleteIt = MessageBox.Show(
				string.Format(Resources.GlobalDeleteConfirm, item)
				, Resources.GlobalInputWarning
				, MessageBoxButtons.YesNo
				, MessageBoxIcon.Question
				, MessageBoxDefaultButton.Button1
				, RightToLeftOptions
			);

			if (deleteIt == DialogResult.No)
				return;


			scalingComboBoxQueryList.Items.RemoveAt(idx);
			this._Queries.RemoveAt(idx);
		}

		private void scalingComboBoxQueryList_SelectedIndexChanged(object sender, EventArgs e)
		{
			var idx = scalingComboBoxQueryList.SelectedIndex;
			Make_SelectedFilters(this._Queries[idx]);
		}

		private void Make_SelectedFilters(SearchQuery searchQuery)
		{
			// Make _SelectedFilters from saved query
			_SelectedFilters.Clear();

			_SelectedFilters.AddRange(searchQuery.CheckedItems);

			ResetCheckBoxesToFirstLoad();

			// Restore SearchTerm ?
			var term = searchQuery.CheckedItems.FirstOrDefault(i => i.CheckedList is null);

			/// Avoid trigger of <see cref="scalingTextBoxSearchTerm_TextChanged"/>
			if (term is null)
				TextBoxSearchTerm_UpdateText_Notrigger(string.Empty);
			else
			{
				TextBoxSearchTerm_UpdateText_Notrigger(term.DisplayValue);
				this.scalingTextBoxSearchTerm.Tag = term;
			}

			MakeSearchTermBoxItem();

			this.Apply_SelectedFilters();
		}

	}

	public static class SearchDialogAdvancedExtension
	{

		public static (ScalingCheckedListBox, BoxTag) GetBoxTag(this Control obj)
		{
			var ctr = obj as ScalingCheckedListBox;

			if (ctr is null) return (null, null);

			var Tag = ctr.Tag as BoxTag ?? new BoxTag();
			ctr.Tag = Tag;

			return (ctr, Tag);
		}

		public static (ScalingTextBox, BoxItem) GetBoxItem(this Control obj, bool newInstance)
		{
			var ctr = obj as ScalingTextBox;

			if (ctr is null) return (null, null);

			var Tag = newInstance
				? new BoxItem()
				: ctr.Tag as BoxItem ?? new BoxItem();

			ctr.Tag = Tag;

			return (ctr, Tag);
		}
	}
}