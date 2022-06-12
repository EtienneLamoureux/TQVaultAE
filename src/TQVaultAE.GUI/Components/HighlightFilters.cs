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
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Components
{
	public partial class HighlightFilters : UserControl
	{
		public class ItemType
		{
			public string Class { get; set; }
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

			toolTip.SetToolTip(this.numericUpDownMinDex, Resources.HighlightFiltersZeroToIgnore);
			toolTip.SetToolTip(this.numericUpDownMinInt, Resources.HighlightFiltersZeroToIgnore);
			toolTip.SetToolTip(this.numericUpDownMinLvl, Resources.HighlightFiltersZeroToIgnore);
			toolTip.SetToolTip(this.numericUpDownMinStr, Resources.HighlightFiltersZeroToIgnore);
			toolTip.SetToolTip(this.numericUpDownMaxDex, Resources.HighlightFiltersZeroToIgnore);
			toolTip.SetToolTip(this.numericUpDownMaxInt, Resources.HighlightFiltersZeroToIgnore);
			toolTip.SetToolTip(this.numericUpDownMaxLvl, Resources.HighlightFiltersZeroToIgnore);
			toolTip.SetToolTip(this.numericUpDownMaxStr, Resources.HighlightFiltersZeroToIgnore);

			this.BackgroundImageLayout = ImageLayout.Stretch;
			this.BackgroundImage = Resources.caravan_bg;
		}

		internal void InitTypeList()
		{
			this.scalingCheckedListBoxTypes.Items.Clear();// Remove design mode items
			var list = new ItemType[] {
				new ItemType{
					Class = "ArmorProtective_Head",
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName("ArmorProtective_Head")).Trim(' ', ':')
				},
				new ItemType{
					Class = "ArmorProtective_Forearm",
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName("ArmorProtective_Forearm")).Trim(' ', ':')
				},
				new ItemType{
					Class = "ArmorProtective_UpperBody",
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName("ArmorProtective_UpperBody")).Trim(' ', ':')
				},
				new ItemType{
					Class = "ArmorProtective_LowerBody",
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName("ArmorProtective_LowerBody")).Trim(' ', ':')
				},
				new ItemType{
					Class = "WeaponMelee_Axe",
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName("WeaponMelee_Axe"))
				},
				new ItemType{
					Class = "WeaponMelee_Mace",
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName("WeaponMelee_Mace"))
				},
				new ItemType{
					Class = "WeaponMelee_Sword",
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName("WeaponMelee_Sword"))
				},
				new ItemType{
					Class = "WeaponArmor_Shield",
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName("WeaponArmor_Shield"))
				},
				new ItemType{
					Class = "WeaponHunting_RangedOneHand",
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName("WeaponHunting_RangedOneHand"), true)
				},
				new ItemType{
					Class = "WeaponHunting_Bow",
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName("WeaponHunting_Bow"))
				},
				new ItemType{
					Class = "WeaponHunting_Spear",
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName("WeaponHunting_Spear"))
				},
				new ItemType{
					Class = "WeaponMagical_Staff",
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName("WeaponMagical_Staff")).Trim(' ', ':')
				},
				new ItemType{
					Class = "ArmorJewelry_Amulet",
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName("ArmorJewelry_Amulet"))
				},
				new ItemType{
					Class = "ArmorJewelry_Ring",
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName("ArmorJewelry_Ring"))
				},
				new ItemType{
					Class = "ItemArtifact",
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName("ItemArtifact"))
				},
				new ItemType{
					Class = "ItemArtifactFormula",
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName("ItemArtifactFormula"))
				},
				new ItemType{
					Class = "ItemRelic",
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName("ItemRelic"))
				},
				new ItemType{
					Class = "ItemCharm",
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName("ItemCharm"))
				},
				new ItemType{
					Class = "OneShot_Scroll",
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName("OneShot_Scroll"))
				},
				new ItemType{
					Class = "OneShot_PotionMana|OneShot_PotionHealth|OneShot_Scroll_Eternal",
					DisplayName = TranslationService.TranslateXTag("tagTutorialTip14Title")
				},
				new ItemType{
					Class = "QuestItem",
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName("QuestItem"))
				},
				new ItemType{
					Class = "ItemEquipment",
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName("ItemEquipment")).Trim(' ', '~')
				},
				new ItemType{
					Class = "OneShot_Dye",
					DisplayName = TranslationService.TranslateXTag(Item.GetClassTagName("OneShot_Dye"))
				},
			};

			// Compute item space (ColumnWidth)
			var maxWidth = list.Select(i =>
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

		internal void ResetAll()
		{
			ResetNumeric();
			this.scalingCheckBoxMax.Checked = false;
			this.scalingCheckBoxMin.Checked = false;
			this.scalingCheckedListBoxTypes.ClearSelected();
			for (int i = 0; i < this.scalingCheckedListBoxTypes.Items.Count; i++)
				this.scalingCheckedListBoxTypes.SetItemChecked(i, false);
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
				ClassItem = this.scalingCheckedListBoxTypes.CheckedItems.OfType<ItemType>().SelectMany(x => x.Class.Split('|')).ToList(),
			};

			if (filter.MaxRequierement || filter.MinRequierement || filter.ClassItem.Any())
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
