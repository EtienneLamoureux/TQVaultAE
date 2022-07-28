using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Components
{
	public partial class HighlightFilters : UserControl
	{
		public class ItemType : ItemValue<string> { }
		public class ItemRarity : ItemValue<GearLevel> { }
		public class ItemOrigin : ItemValue<GameExtension> { }

		public class ItemValue<TValue>
		{
			public TValue Value { get; set; }
			public string DisplayName { get; set; }
			public override string ToString()
				=> DisplayName;
		}


		private LinkLabel link;
		public SessionContext UserContext { get; internal set; }
		public ITranslationService TranslationService { get; internal set; }

		public HighlightFilters()
		{
			InitializeComponent();

			DoubleBuffered = true;
			SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);

			this.BackgroundImageLayout = ImageLayout.Stretch;
			this.BackgroundImage = Resources.caravan_bg;
		}

		internal void InitializeFilters()
		{
			toolTip.SetToolTip(this.numericUpDownMinDex, Resources.HighlightFiltersZeroToIgnore);
			toolTip.SetToolTip(this.numericUpDownMinInt, Resources.HighlightFiltersZeroToIgnore);
			toolTip.SetToolTip(this.numericUpDownMinLvl, Resources.HighlightFiltersZeroToIgnore);
			toolTip.SetToolTip(this.numericUpDownMinStr, Resources.HighlightFiltersZeroToIgnore);
			toolTip.SetToolTip(this.numericUpDownMaxDex, Resources.HighlightFiltersZeroToIgnore);
			toolTip.SetToolTip(this.numericUpDownMaxInt, Resources.HighlightFiltersZeroToIgnore);
			toolTip.SetToolTip(this.numericUpDownMaxLvl, Resources.HighlightFiltersZeroToIgnore);
			toolTip.SetToolTip(this.numericUpDownMaxStr, Resources.HighlightFiltersZeroToIgnore);

			InitRarity();

			InitType();

			InitOrigin();

			// Translate
			this.scalingCheckBoxMin.Text = $"{Resources.GlobalMin} {Resources.GlobalRequirements} :";
			this.scalingCheckBoxMax.Text = $"{Resources.GlobalMax} {Resources.GlobalRequirements} :";
			this.scalingLabelMaxLvl.Text =
			this.scalingLabelMinLvl.Text = TranslationService.TranslateXTag("tagMenuImport05");
			this.scalingLabelMaxStr.Text =
			this.scalingLabelMinStr.Text = TranslationService.TranslateXTag("Strength");
			this.scalingLabelMaxDex.Text =
			this.scalingLabelMinDex.Text = TranslationService.TranslateXTag("Dexterity");
			this.scalingLabelMaxInt.Text =
			this.scalingLabelMinInt.Text = TranslationService.TranslateXTag("Intelligence");
			this.buttonReset.Text = TranslationService.TranslateXTag("tagSkillReset");
			this.buttonApply.Text = TranslationService.TranslateXTag("tagMenuButton07");
		}

		private void InitOrigin()
		{
			this.scalingCheckedListBoxOrigin.Items.Clear();// Remove design mode items
			var list = new ItemOrigin[] {
				new ItemOrigin{
					Value = GameExtension.TitanQuest,
					DisplayName = TranslationService.Translate(GameExtension.TitanQuest)
				},
				new ItemOrigin{
					Value = GameExtension.ImmortalThrone,
					DisplayName = TranslationService.Translate(GameExtension.ImmortalThrone)
				},
				new ItemOrigin{
					Value = GameExtension.Ragnarok,
					DisplayName = TranslationService.Translate(GameExtension.Ragnarok)
				},
				new ItemOrigin{
					Value = GameExtension.Atlantis,
					DisplayName = TranslationService.Translate(GameExtension.Atlantis)
				},
				new ItemOrigin{
					Value = GameExtension.EternalEmbers,
					DisplayName = TranslationService.Translate(GameExtension.EternalEmbers)
				},
			};

			// Compute item space (ColumnWidth)
			int maxWidth = list.Select(i =>
				TextRenderer.MeasureText(
					i.DisplayName
					, this.scalingCheckedListBoxOrigin.Font
					, this.scalingCheckedListBoxOrigin.Size
					, TextFormatFlags.SingleLine
				).Width
			).Max();
			maxWidth += SystemInformation.VerticalScrollBarWidth;// Add this for the size of the checkbox in front of the text
			this.scalingCheckedListBoxOrigin.ColumnWidth = maxWidth;

			this.scalingCheckedListBoxOrigin.Items.AddRange(list);
		}

		private void InitType()
		{
			this.scalingCheckedListBoxTypes.Items.Clear();// Remove design mode items
			var list = new ItemType[] {
				new ItemType{
					Value = Item.ICLASS_HEAD,
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_HEAD)).Trim(' ', ':')
				},
				new ItemType{
					Value = Item.ICLASS_FOREARM,
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_FOREARM)).Trim(' ', ':')
				},
				new ItemType{
					Value = Item.ICLASS_UPPERBODY,
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_UPPERBODY)).Trim(' ', ':')
				},
				new ItemType{
					Value = Item.ICLASS_LOWERBODY,
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_LOWERBODY)).Trim(' ', ':')
				},
				new ItemType{
					Value = Item.ICLASS_AXE,
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_AXE))
				},
				new ItemType{
					Value = Item.ICLASS_MACE,
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_MACE))
				},
				new ItemType{
					Value = Item.ICLASS_SWORD,
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_SWORD))
				},
				new ItemType{
					Value = Item.ICLASS_SHIELD,
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_SHIELD))
				},
				new ItemType{
					Value = Item.ICLASS_RANGEDONEHAND,
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_RANGEDONEHAND), true)
				},
				new ItemType{
					Value = Item.ICLASS_BOW,
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_BOW))
				},
				new ItemType{
					Value = Item.ICLASS_SPEAR,
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_SPEAR))
				},
				new ItemType{
					Value = Item.ICLASS_STAFF,
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_STAFF)).Trim(' ', ':')
				},
				new ItemType{
					Value = Item.ICLASS_AMULET,
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_AMULET))
				},
				new ItemType{
					Value = Item.ICLASS_RING,
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_RING))
				},
				new ItemType{
					Value = Item.ICLASS_ARTIFACT,
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_ARTIFACT))
				},
				new ItemType{
					Value = Item.ICLASS_FORMULA,
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_FORMULA))
				},
				new ItemType{
					Value = Item.ICLASS_RELIC,
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_RELIC))
				},
				new ItemType{
					Value = Item.ICLASS_CHARM,
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_CHARM))
				},
				new ItemType{
					Value = Item.ICLASS_SCROLL,
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_SCROLL))
				},
				new ItemType{
					Value = string.Join("|", Item.ICLASS_POTIONMANA, Item.ICLASS_POTIONHEALTH, Item.ICLASS_SCROLL_ETERNAL),
					DisplayName = TranslationService.TranslateXTag("tagTutorialTip14Title")
				},
				new ItemType{
					Value = Item.ICLASS_QUESTITEM,
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_QUESTITEM))
				},
				new ItemType{
					Value = Item.ICLASS_EQUIPMENT,
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_EQUIPMENT)).Trim(' ', '~')
				},
				new ItemType{
					Value = Item.ICLASS_DYE,
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName(Item.ICLASS_DYE))
				},
			};

			// Compute item space (ColumnWidth)
			int maxWidth = list.Select(i =>
				TextRenderer.MeasureText(
					i.DisplayName
					, this.scalingCheckedListBoxTypes.Font
					, this.scalingCheckedListBoxTypes.Size
					, TextFormatFlags.SingleLine
				).Width
			).Max();
			maxWidth += SystemInformation.VerticalScrollBarWidth;// Add this for the size of the checkbox in front of the text
			this.scalingCheckedListBoxTypes.ColumnWidth = maxWidth;

			this.scalingCheckedListBoxTypes.Items.AddRange(list);
		}

		private void InitRarity()
		{
			this.scalingCheckedListBoxRarity.Items.Clear();// Remove design mode items
			var listRarity = new ItemRarity[] {
				new ItemRarity(){
					Value = GearLevel.Broken,
					DisplayName = TranslationService.Translate(GearLevel.Broken)
				},
				new ItemRarity(){
					Value = GearLevel.Mundane,
					DisplayName = TranslationService.Translate(GearLevel.Mundane)
				},
				new ItemRarity(){
					Value = GearLevel.Common,
					DisplayName = TranslationService.Translate(GearLevel.Common)
				},
				new ItemRarity(){
					Value = GearLevel.Rare,
					DisplayName = TranslationService.Translate(GearLevel.Rare)
				},
				new ItemRarity(){
					Value = GearLevel.Epic,
					DisplayName = TranslationService.Translate(GearLevel.Epic)
				},
				new ItemRarity(){
					Value = GearLevel.Legendary,
					DisplayName = TranslationService.Translate(GearLevel.Legendary)
				},
			};
			// Compute item space (ColumnWidth)
			var maxWidth = listRarity.Select(i =>
				TextRenderer.MeasureText(
					i.DisplayName
					, this.scalingCheckedListBoxRarity.Font
					, this.scalingCheckedListBoxRarity.Size
					, TextFormatFlags.SingleLine
				).Width
			).Max();
			maxWidth += SystemInformation.VerticalScrollBarWidth;// Add this for the size of the checkbox in front of the text
			this.scalingCheckedListBoxRarity.ColumnWidth = maxWidth;

			this.scalingCheckedListBoxRarity.Items.AddRange(listRarity);
		}

		internal void ResetAll()
		{
			ResetNumeric();
			this.scalingCheckBoxMax.Checked =
			this.scalingCheckBoxMin.Checked =
			this.scalingCheckBoxHavingPrefix.Checked =
			this.scalingCheckBoxHavingSuffix.Checked = false;

			ClearSelected(this.scalingCheckedListBoxTypes);
			ClearSelected(this.scalingCheckedListBoxRarity);
			ClearSelected(this.scalingCheckedListBoxOrigin);
		}

		private static void ClearSelected(ScalingCheckedListBox listbox)
		{
			listbox.ClearSelected();
			for (int i = 0; i < listbox.Items.Count; i++)
				listbox.SetItemChecked(i, false);
		}

		internal void ResetNumeric()
		{
			this.numericUpDownMaxDex.Value =
			this.numericUpDownMaxInt.Value =
			this.numericUpDownMaxLvl.Value =
			this.numericUpDownMaxStr.Value =
			this.numericUpDownMinDex.Value =
			this.numericUpDownMinInt.Value =
			this.numericUpDownMinLvl.Value =
			this.numericUpDownMinStr.Value = 0;
		}

		/// <summary>
		/// Anchor link
		/// </summary>
		internal LinkLabel Link
		{
			set
			{
				link = value;
				AdjustLocation();
			}
		}

		private void AdjustLocation()
		{
			// Adjust location
			var frm = this.FindForm();
			var linklocation = frm.PointToClient(link.PointToScreen(Point.Empty));
			this.Location = new Point(linklocation.X, linklocation.Y - this.Height);
		}

		private void buttonReset_Click(object sender, EventArgs e)
		{
			var frm = this.FindForm();
			ResetAll();
			this.UserContext.HighlightFilter = null;
			link.LinkVisited = false;
			this.Visible = false;
			this.UserContext.FindHighlight();
			frm.Refresh();
		}

		private void buttonSave_Click(object sender, EventArgs e)
		{
			var frm = this.FindForm();
			var filter = new HighlightFilterValues
			{
				MaxLvl = (int)this.numericUpDownMaxLvl.Value,
				MaxDex = (int)this.numericUpDownMaxDex.Value,
				MaxInt = (int)this.numericUpDownMaxInt.Value,
				MaxStr = (int)this.numericUpDownMaxStr.Value,
				MinLvl = (int)this.numericUpDownMinLvl.Value,
				MinDex = (int)this.numericUpDownMinDex.Value,
				MinInt = (int)this.numericUpDownMinInt.Value,
				MinStr = (int)this.numericUpDownMinStr.Value,
				MaxRequierement = this.scalingCheckBoxMax.Checked,
				MinRequierement = this.scalingCheckBoxMin.Checked,
				ClassItem = this.scalingCheckedListBoxTypes.CheckedItems.OfType<ItemType>().SelectMany(x => x.Value.Split('|')).ToList(),
				Rarity = this.scalingCheckedListBoxRarity.CheckedItems.OfType<ItemRarity>().Select(x => x.Value).ToList(),
				Origin = this.scalingCheckedListBoxOrigin.CheckedItems.OfType<ItemOrigin>().Select(x => x.Value).ToList(),
				HavingPrefix = this.scalingCheckBoxHavingPrefix.Checked,
				HavingSuffix = this.scalingCheckBoxHavingSuffix.Checked,
			};

			if (filter.MaxRequierement || filter.MinRequierement || filter.HavingPrefix || filter.HavingSuffix
				|| filter.ClassItem.Any() || filter.Rarity.Any() || filter.Origin.Any())
			{
				this.UserContext.HighlightFilter = filter;
				link.LinkVisited = true;
				this.Visible = false;
				this.UserContext.FindHighlight();
				frm.Refresh();
				return;
			}

			// No filters
			this.UserContext.HighlightFilter = null;
			link.LinkVisited = false;
			this.Visible = false;
			this.UserContext.FindHighlight();
			frm.Refresh();
		}

		private void numericUpDownMinLvl_ValueChanged(object sender, EventArgs e)
		{
			this.scalingCheckBoxMin.Checked = true;
		}
		private void numericUpDownMaxLvl_ValueChanged(object sender, EventArgs e)
		{
			this.scalingCheckBoxMax.Checked = true;
		}
	}
}
