using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using TQVaultAE.Domain.Contracts.Providers;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Helpers;
using TQVaultAE.GUI.Models;
using TQVaultAE.GUI.Tooltip;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Components
{

	public partial class ForgePanel : UserControl
	{
		#region Internal Types

		private enum ItemPropertyType
		{
			Base,
			Prefix,
			Suffix,
			Relic1,
			Relic2,
		}

		private class ForgeComboItem
		{
			public ItemPropertyType Value;
			public string Text;
			public override string ToString() => Text;
		}

		#endregion

		private readonly ISoundService SoundService;
		private readonly IUIService UIService;
		private readonly IFontService FontService;
		private readonly ITranslationService TranslationService;
		private readonly IItemProvider ItemProvider;

		internal Action CancelAction;
		internal Action ForgeAction;
		internal Action<string> NotifyAction;

		private Item BaseItem;
		private Item PrefixItem;
		private Item SuffixItem;
		private Item Relic1Item;
		private Item Relic2Item;
		private ScalingRadioButton lastMode;

		private const string SoundStricMode = @"Sounds\MONSTERS\GREECE\G_TELKINE\TELEKINEVOICE01.WAV";
		private const string SoundRelaxMode = @"Sounds\MONSTERS\GREECE\G_TELKINE\TELEKINEVOICE02.WAV";
		//private const string SoundGodMode = @"Sounds\MONSTERS\GREECE\G_TELKINE\TELEKINEVOICE03.WAV";
		private const string SoundGodMode = @"Sounds\AMBIENCE\RANDOMEVENT\TYPHONLAUGHDISTANCE.WAV";

		/// <summary>
		/// Gets or sets the dragInfo instance of any items being dragged.
		/// </summary>
		protected ItemDragInfo DragInfo;
		private readonly IServiceProvider ServiceProvider;
		private Bitmap dragingBmp;
		private Item lastDragedItem;

		private bool IsGodMode => scalingRadioButtonGod.Checked;
		private bool IsRelaxMode => scalingRadioButtonRelax.Checked;
		private bool IsStrictMode => scalingRadioButtonStrict.Checked;
		bool IsDraging => DragInfo != null && DragInfo.IsActive;

		private Point CurrentDragPicturePosition
		{
			get
			{
				var cursorPos = PointToClient(Cursor.Position);
				return new Point(
					cursorPos.X - (pictureBoxDragDrop.Size.Width / 2)
					, cursorPos.Y - pictureBoxDragDrop.Size.Height / 2
				);
			}
		}

		private readonly string OfTheTinkererTranslation;

		private readonly Color PictureBoxBaseColor = Color.FromArgb(32 * 7, 46, 41, 31);

		Item _PreviewItem = null;
		private Item PreviewItem
		{
			get
			{
				if (BaseItem is null) return null;
				if (_PreviewItem is null)
				{
					_PreviewItem = BaseItem.Clone();
					if (SuffixItem is not null || PrefixItem is not null || Relic1Item is not null || Relic2Item is not null)
					{
						MergeItemProperties(_PreviewItem);
						this.ItemProvider.GetDBData(_PreviewItem);
					}
				}
				return _PreviewItem;
			}
		}

		public ForgePanel()
		{
			InitializeComponent();
		}

		public ForgePanel(ItemDragInfo dragInfo, IServiceProvider serviceProvider)
		{
			InitializeComponent();

			DoubleBuffered = true;

			DragInfo = dragInfo;
			ServiceProvider = serviceProvider;
			SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor, true);

			SoundService = serviceProvider.GetService<ISoundService>();
			UIService = serviceProvider.GetService<IUIService>();
			FontService = serviceProvider.GetService<IFontService>();
			TranslationService = serviceProvider.GetService<ITranslationService>();
			this.ItemProvider = serviceProvider.GetService<IItemProvider>();

			#region Font, Scaling, Translate, BackGround

			BackgroundImageLayout = ImageLayout.Stretch;
			BackgroundImage = Resources.caravan_bg;

			tableLayoutPanelForge.BackgroundImageLayout = ImageLayout.Stretch;
			tableLayoutPanelForge.BackgroundImage = Resources.StashPanel;

			scalingLabelBaseItem.Font =
			scalingLabelPrefix.Font =
			scalingLabelRelic1.Font =
			scalingLabelRelic2.Font =
			scalingLabelSuffix.Font =
				FontService.GetFont(11F, FontStyle.Bold, UIService.Scale);

			scalingLabelBaseItem.ForeColor =
			scalingLabelPrefix.ForeColor =
			scalingLabelRelic1.ForeColor =
			scalingLabelRelic2.ForeColor =
			scalingLabelSuffix.ForeColor = TQColor.Yellow.Color();

			// Scalling
			var size = TextRenderer.MeasureText(scalingLabelBaseItem.Text, scalingLabelBaseItem.Font);
			scalingLabelBaseItem.Height =
			scalingLabelPrefix.Height =
			scalingLabelRelic1.Height =
			scalingLabelRelic2.Height =
			scalingLabelSuffix.Height = size.Height;

			ForgeButton.Font = FontService.GetFontLight(12F);
			ResetButton.Font = FontService.GetFontLight(12F);
			CancelButton.Font = FontService.GetFontLight(12F);

			scalingRadioButtonGod.Text = Resources.ForgeGodMode;
			toolTip.SetToolTip(scalingRadioButtonGod, Resources.ForgeGodModeTT);
			scalingRadioButtonGod.ForeColor = TQColor.Indigo.Color();

			OfTheTinkererTranslation = TranslationService.TranslateXTag("x3tagSuffix01");

			scalingRadioButtonRelax.Text = Resources.ForgeRelaxMode;
			toolTip.SetToolTip(scalingRadioButtonRelax, string.Format(Resources.ForgeRelaxModeTT, OfTheTinkererTranslation));
			scalingRadioButtonRelax.ForeColor = TQColor.Aqua.Color();

			scalingRadioButtonStrict.Text = Resources.ForgeStrictMode;
			toolTip.SetToolTip(scalingRadioButtonStrict, Resources.ForgeStrictModeTT);
			scalingRadioButtonStrict.ForeColor = TQColor.Yellow.Color();

			#endregion

			HideGodModeRows();

			// Everything in the Forge must respond
			this.ProcessAllControls(c =>
			{
				c.MouseEnter += ForgePanel_MouseEnter;
				c.MouseMove += pictureBoxDragDrop_MouseMove;
			});

			ResetSelection();
		}

		private void HideGodModeRows()
		{
			comboBoxPrefix.Visible =
			comboBoxRelic1.Visible =
			comboBoxRelic2.Visible =
			comboBoxSuffix.Visible = false;
		}

		private void ShowGodModeRows()
		{
			comboBoxPrefix.SelectedIndex =
			comboBoxRelic1.SelectedIndex =
			comboBoxRelic2.SelectedIndex =
			comboBoxSuffix.SelectedIndex = -1;

			comboBoxPrefix.Visible =
			comboBoxRelic1.Visible =
			comboBoxRelic2.Visible =
			comboBoxSuffix.Visible = true;
		}

		private void MergeItemProperties(Item itm)
		{
			if (PrefixItem is not null)
			{
				itm.prefixInfo = null;
				if (!IsGodMode)
					itm.prefixID = PrefixItem.prefixID;
				else
				{ // God Mode
					var selectedPropreties = comboBoxPrefix.SelectedItem as ForgeComboItem;
					switch (selectedPropreties.Value)
					{
						case ItemPropertyType.Prefix:
							itm.prefixID = PrefixItem.prefixID;
							break;
						case ItemPropertyType.Suffix:
							itm.prefixID = PrefixItem.suffixID;
							break;
						case ItemPropertyType.Relic1:
							itm.prefixID = PrefixItem.relicID;
							break;
						case ItemPropertyType.Relic2:
							itm.prefixID = PrefixItem.relic2ID;
							break;
						case ItemPropertyType.Base:
						default:
							itm.prefixID = PrefixItem.BaseItemId;
							break;
					}
				}
			}

			if (SuffixItem is not null)
			{
				itm.suffixInfo = null;
				if (!IsGodMode)
					itm.suffixID = SuffixItem.suffixID;
				else
				{ // God Mode
					var selectedPropreties = comboBoxSuffix.SelectedItem as ForgeComboItem;
					switch (selectedPropreties.Value)
					{
						case ItemPropertyType.Prefix:
							itm.suffixID = SuffixItem.prefixID;
							break;
						case ItemPropertyType.Suffix:
							itm.suffixID = SuffixItem.suffixID;
							break;
						case ItemPropertyType.Relic1:
							itm.suffixID = SuffixItem.relicID;
							break;
						case ItemPropertyType.Relic2:
							itm.suffixID = SuffixItem.relic2ID;
							break;
						case ItemPropertyType.Base:
						default:
							itm.suffixID = SuffixItem.BaseItemId;
							break;
					}
				}
			}

			if (Relic1Item is not null)
			{
				itm.RelicInfo = itm.RelicBonusInfo = null;

				if (!IsGodMode)
				{
					if (Relic1Item.IsRelic)
					{
						itm.relicID = Relic1Item.BaseItemId;
						itm.RelicBonusId = Relic1Item.RelicBonusId;
						itm.Var1 = Relic1Item.Var1;
					}
					else
					{
						itm.relicID = Relic1Item.relicID;
						itm.RelicBonusId = Relic1Item.RelicBonusId;
						itm.Var1 = Relic1Item.Var1;
					}
				}
				else
				{ // God Mode
					var selectedPropreties = comboBoxRelic1.SelectedItem as ForgeComboItem;
					switch (selectedPropreties.Value)
					{
						case ItemPropertyType.Prefix:
							itm.relicID = Relic1Item.prefixID;
							itm.RelicBonusId = null;
							itm.Var1 = 1;
							break;
						case ItemPropertyType.Suffix:
							itm.relicID = Relic1Item.suffixID;
							itm.RelicBonusId = null;
							itm.Var1 = 1;
							break;
						case ItemPropertyType.Relic1:
							itm.relicID = Relic1Item.relicID;
							itm.RelicBonusId = Relic1Item.RelicBonusId;
							itm.Var1 = Relic1Item.Var1;
							break;
						case ItemPropertyType.Relic2:
							itm.relicID = Relic1Item.relic2ID;
							itm.RelicBonusId = Relic1Item.RelicBonus2Id;
							itm.Var1 = Relic1Item.Var2;
							break;
						case ItemPropertyType.Base:
						default:
							itm.relicID = Relic1Item.BaseItemId;
							itm.RelicBonusId = Relic1Item.RelicBonusId;
							itm.Var1 = Relic1Item.Var1;
							break;
					}
				}
			}

			if (Relic2Item is not null)
			{
				itm.Relic2Info = itm.RelicBonus2Info = null;

				if (!IsGodMode)
				{
					if (Relic2Item.IsRelic)
					{
						itm.relic2ID = Relic2Item.BaseItemId;
						itm.RelicBonus2Id = Relic2Item.RelicBonusId;
						itm.Var2 = Relic2Item.Var1;
					}
					else
					{
						itm.relic2ID = Relic2Item.relicID;
						itm.RelicBonus2Id = Relic2Item.RelicBonusId;
						itm.Var2 = Relic2Item.Var1;
					}
				}
				else
				{ // God Mode
					var selectedPropreties = comboBoxRelic2.SelectedItem as ForgeComboItem;
					switch (selectedPropreties.Value)
					{
						case ItemPropertyType.Prefix:
							itm.relic2ID = Relic2Item.prefixID;
							itm.RelicBonus2Id = null;
							itm.Var2 = 1;
							break;
						case ItemPropertyType.Suffix:
							itm.relic2ID = Relic2Item.suffixID;
							itm.RelicBonus2Id = null;
							itm.Var2 = 1;
							break;
						case ItemPropertyType.Relic1:
							itm.relic2ID = Relic2Item.relicID;
							itm.RelicBonus2Id = Relic2Item.RelicBonusId;
							itm.Var2 = Relic2Item.Var1;
							break;
						case ItemPropertyType.Relic2:
							itm.relic2ID = Relic2Item.relic2ID;
							itm.RelicBonus2Id = Relic2Item.RelicBonus2Id;
							itm.Var2 = Relic2Item.Var2;
							break;
						case ItemPropertyType.Base:
						default:
							itm.relic2ID = Relic2Item.BaseItemId;
							itm.RelicBonus2Id = Relic2Item.RelicBonusId;
							itm.Var2 = Relic2Item.Var1;
							break;
					}
				}
			}
		}

		private void ResetSelection()
		{
			pictureBoxBaseItem.Image =
			pictureBoxPrefix.Image =
			pictureBoxRelic1.Image =
			pictureBoxRelic2.Image =
			pictureBoxSuffix.Image = null;

			pictureBoxBaseItem.BackColor =
			pictureBoxPrefix.BackColor =
			pictureBoxRelic1.BackColor =
			pictureBoxRelic2.BackColor =
			pictureBoxSuffix.BackColor = PictureBoxBaseColor;

			BaseItem =
			PrefixItem =
			Relic1Item =
			Relic2Item =
			SuffixItem =
			_PreviewItem = null;

			comboBoxPrefix.Items.Clear();
			comboBoxSuffix.Items.Clear();
			comboBoxRelic1.Items.Clear();
			comboBoxRelic2.Items.Clear();
		}

		private void CancelButton_Click(object sender, EventArgs e)
		{
			SoundService.PlayRandomCancel();

			ResetSelection();

			if (CancelAction is not null)
				CancelAction();
		}

		private void ForgeButton_Click(object sender, EventArgs e)
		{
			if (MustSelectBaseItem())
				return;

			if (MustSelectAtLeastAnotherItem())
				return;

			// Merge item properties into Base item
			var itm = BaseItem;

			MergeItemProperties(itm);

			// Sound
			if (IsGodMode)
				SoundService.PlayLevelUp();
			else
				SoundService.PlayRandomMetalHit();

			// External Code
			if (ForgeAction is not null)
				ForgeAction();
		}

		private void ForgeMode_Clicked(object sender, EventArgs e)
		{
			if (IsStrictMode)
			{
				SoundService.PlaySound(SoundStricMode);

				// Reset on Mode downgrade
				if (lastMode == scalingRadioButtonGod || lastMode == scalingRadioButtonRelax)
					ResetSelection();

				HideGodModeRows();
				lastMode = scalingRadioButtonStrict;
				return;
			}

			if (IsRelaxMode)
			{
				SoundService.PlaySound(SoundRelaxMode);

				// Reset on Mode downgrade
				if (lastMode == scalingRadioButtonGod)
					ResetSelection();

				HideGodModeRows();
				lastMode = scalingRadioButtonRelax;
				return;
			}

			if (IsGodMode)
			{
				SoundService.PlaySound(SoundGodMode);

				ShowGodModeRows();
				lastMode = scalingRadioButtonGod;
			}
		}
		private void ForgePanel_MouseEnter(object sender, EventArgs e)
		{
			// Enforce first autosize display
			if (MaximumSize == Size.Empty)
				MaximumSize = new Size(Size.Width, Size.Height + 50);

			if (lastDragedItem != DragInfo.Item)// Item Switching
			{
				lastDragedItem = DragInfo.Item;
				dragingBmp = null;
			}

			if (IsDraging && dragingBmp is null) // First time with this item
			{
				dragingBmp = UIService.LoadBitmap(DragInfo.Item.TexImageResourceId);

				pictureBoxDragDrop.Location = CurrentDragPicturePosition;
				pictureBoxDragDrop.Size = dragingBmp.Size;
				//pictureBoxDragDrop.BackColor = Color.FromArgb(46, 41, 31);// Not pretty but Transparent doesn't work
				pictureBoxDragDrop.Image = dragingBmp;
				pictureBoxDragDrop.BringToFront();
				pictureBoxDragDrop.Visible = true;

				// seek for mouse out of panel
				SubscribeExternalMouseMove();
			}
		}

		private void SubscribeExternalMouseMove()
		{
			Debug.WriteLine("SUBSCRIBE");
			// tout le form sauf ce control
			this.FindForm().ProcessAllControlsWithExclusion(c =>
			{
				if (c == this)
					return true;// Exclude myself and my children

				c.MouseMove += Parent_MouseMove;

				return false;
			});
		}

		private void UnsubscribeExternalMouseMove()
		{
			Debug.WriteLine("UNSUBSCRIBE");
			// tout le form sauf ce control
			var frm = this.FindForm();
			frm.ProcessAllControlsWithExclusion(c =>
				{
					if (c == this)
						return true;// Exclude myself and my children

					c.MouseMove -= Parent_MouseMove;

					return false;
				}
			);
		}

		private void pictureBoxDragDrop_MouseMove(object sender, MouseEventArgs e)
		{
			if (IsDraging)
			{
				pictureBoxDragDrop.Location = CurrentDragPicturePosition;
			}
		}

		private void Parent_MouseMove(object sender, EventArgs e)
		{
			Debug.WriteLine("Parent_MouseMove");
			// Out of Panel
			if (pictureBoxDragDrop.Visible)
			{
				UnsubscribeExternalMouseMove();
				pictureBoxDragDrop.Image = dragingBmp = null;
				pictureBoxDragDrop.Visible = false;
			}
		}

		/// <summary>
		/// Drop <see cref="pictureBoxDragDrop"/>
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void pictureBoxDragDrop_Click(object sender, EventArgs e)
		{
			if (IsAlreadyPlaced(DragInfo.Item))
				return;

			pictureBoxDragDrop.Visible = false;

			var found = tableLayoutPanelForge.GetChildAtPoint(
				tableLayoutPanelForge.PointToClient(Cursor.Position)
				, GetChildAtPointSkip.Invisible | GetChildAtPointSkip.Transparent
			) as PictureBox;

			var drgItm = DragInfo.Item;

			if (found == pictureBoxBaseItem)
			{
				if (StrictModeNoEpicLegendaryItem(drgItm))
					return;

				if (StrictModePrefixSuffixMustBeSameType(found, drgItm))
					return;

				if (StrictModeCantUsePrefixSuffixOfHigherLevelThanBaseItem(found, drgItm))
					return;

				// Strict reset relic 2 if non Tinkered forge result
				if (IsStrictMode
					&& Relic2Item is not null
					&& SuffixItem is not null
					&& !SuffixItem.IsOfTheTinkerer
					&& drgItm.IsOfTheTinkerer
				)
				{
					ResetRelic2();
					//return;
				}

				// Only Weapon/Armor allowed
				if (drgItm.IsWeaponShield || drgItm.IsArmor || drgItm.IsJewellery)
				{
					BaseItem = drgItm;
					pictureBoxBaseItem.Image = UIService.LoadBitmap(BaseItem.TexImageResourceId);
					pictureBoxBaseItem.BackColor = Color.FromArgb(127, ControlPaint.Dark(drgItm.ItemStyle.Color()));
					DragInfo.Cancel();
					SoundService.PlayRandomItemDrop();
					_PreviewItem = null;
					return;
				}
				// Not good item class
				if (NotifyAction is not null) NotifyAction(Resources.ForgeMustSelectArmorOrWeaponOrJewellery);
				SoundService.PlayRandomCancel();
			}

			if (found == pictureBoxPrefix)
			{
				if (BaseItemFirst())
					return;

				if (StrictModeNoEpicLegendaryItem(drgItm))
					return;

				if (StrictModePrefixSuffixMustBeSameType(found, drgItm))
					return;

				if (StrictModeCantUsePrefixSuffixOfHigherLevelThanBaseItem(found, drgItm))
					return;

				// Strict & Relax mode need item having prefix
				if (!IsGodMode && !drgItm.HasPrefix)
				{
					pictureBoxDragDrop.Visible = true;
					if (NotifyAction is not null) NotifyAction(Resources.ForgeMustHavePrefixOutsideOfGodMode);
					SoundService.PlayRandomCancel();
					return;
				}

				// Only Weapon/Armor allowed
				if (drgItm.IsWeaponShield || drgItm.IsArmor || drgItm.IsJewellery || IsGodMode)
				{
					PrefixItem = drgItm;
					pictureBoxPrefix.Image = UIService.LoadBitmap(PrefixItem.TexImageResourceId);
					pictureBoxPrefix.BackColor = Color.FromArgb(127, ControlPaint.Dark(drgItm.ItemStyle.Color()));
					DragInfo.Cancel();
					AdjustComboBox(comboBoxPrefix);
					SoundService.PlayRandomItemDrop();
					_PreviewItem = null;
					return;
				}

				// Not good item class
				if (NotifyAction is not null) NotifyAction(Resources.ForgeMustSelectArmorOrWeaponOrJewellery);
				SoundService.PlayRandomCancel();
			}

			if (found == pictureBoxSuffix)
			{
				if (BaseItemFirst())
					return;

				if (StrictModeNoEpicLegendaryItem(drgItm))
					return;

				if (StrictModePrefixSuffixMustBeSameType(found, drgItm))
					return;

				if (StrictModeCantUsePrefixSuffixOfHigherLevelThanBaseItem(found, drgItm))
					return;

				// Strict reset relic 2 if non Tinkered forge result
				if (IsStrictMode
					&& Relic2Item is not null
					&& !drgItm.IsOfTheTinkerer
				)
				{
					ResetRelic2();
				}

				// Strict & Relax mode need item having suffix
				if (!IsGodMode && !drgItm.HasSuffix)
				{
					pictureBoxDragDrop.Visible = true;
					if (NotifyAction is not null) NotifyAction(Resources.ForgeMustHaveSuffixOutsideOfGodMode);
					SoundService.PlayRandomCancel();
					return;
				}

				// Only Weapon/Armor allowed
				if (drgItm.IsWeaponShield || drgItm.IsArmor || drgItm.IsJewellery || IsGodMode)
				{
					SuffixItem = drgItm;
					pictureBoxSuffix.Image = UIService.LoadBitmap(SuffixItem.TexImageResourceId);
					pictureBoxSuffix.BackColor = Color.FromArgb(127, ControlPaint.Dark(drgItm.ItemStyle.Color()));
					DragInfo.Cancel();
					AdjustComboBox(comboBoxSuffix);
					SoundService.PlayRandomItemDrop();
					_PreviewItem = null;
					return;
				}
				// Not good item class
				if (NotifyAction is not null) NotifyAction(Resources.ForgeMustSelectArmorOrWeaponOrJewellery);
				SoundService.PlayRandomCancel();
			}

			if (found == pictureBoxRelic1)
			{
				if (BaseItemFirst())
					return;

				// Strict & Relax mode need item having relic
				if (!IsGodMode
					&& (!drgItm.IsRelic && !drgItm.HasRelicSlot1) // Not a Relic without relic
				)
				{
					pictureBoxDragDrop.Visible = true;
					if (NotifyAction is not null) NotifyAction(Resources.ForgeMustHaveRelicOutsideOfGodMode);
					SoundService.PlayRandomCancel();
					return;
				}

				// Only Weapon/Armor/Jewellery with relic allowed or a relic item
				if (
					IsGodMode
					|| (
						(drgItm.IsWeaponShield && drgItm.HasRelicSlot1)
						|| (drgItm.IsArmor && drgItm.HasRelicSlot1)
						|| (drgItm.IsJewellery && drgItm.HasRelicSlot1)
						|| drgItm.IsRelic
					)
				)
				{
					Relic1Item = drgItm;
					pictureBoxRelic1.Image = UIService.LoadBitmap(Relic1Item.TexImageResourceId);
					pictureBoxRelic1.BackColor = Color.FromArgb(127, ControlPaint.Dark(drgItm.ItemStyle.Color()));
					DragInfo.Cancel();
					AdjustComboBox(comboBoxRelic1);
					SoundService.PlayRandomItemDrop();
					_PreviewItem = null;
					return;
				}
				// Not good item class
				if (NotifyAction is not null) NotifyAction(Resources.ForgeMustSelectRelicOrItemWithRelic);
				SoundService.PlayRandomCancel();
			}

			if (found == pictureBoxRelic2)
			{
				if (BaseItemFirst())
					return;

				// Strict Mode only allowed on BaseItem "of the Tinkered"
				if (IsStrictMode
					&& !(
						// No Suffix Item && BaseItem of Tinkerer
						(SuffixItem is null && BaseItem.IsOfTheTinkerer)
						// Or Suffix Item of Tinkerer
						|| (SuffixItem is not null && SuffixItem.IsOfTheTinkerer)
					)
				)
				{
					pictureBoxDragDrop.Visible = true;
					if (NotifyAction is not null)
						NotifyAction(string.Format(Resources.ForgeMustHaveTinkeredSuffix, OfTheTinkererTranslation));
					SoundService.PlayRandomCancel();
					return;
				}

				// Strict & Relax mode need item having relic
				if (!IsGodMode
					&& (!drgItm.IsRelic && !drgItm.HasRelicSlot1)
				)
				{
					pictureBoxDragDrop.Visible = true;
					if (NotifyAction is not null) NotifyAction(Resources.ForgeMustHaveRelicOutsideOfGodMode);
					SoundService.PlayRandomCancel();
					return;
				}

				// Only Weapon/Armor/Jewellery with relic allowed or a relic item
				if (
					IsGodMode
					|| (
						(drgItm.IsWeaponShield && drgItm.HasRelicSlot1)
						|| (drgItm.IsArmor && drgItm.HasRelicSlot1)
						|| (drgItm.IsJewellery && drgItm.HasRelicSlot1)
						|| drgItm.IsRelic
					)
				)
				{
					Relic2Item = drgItm;
					pictureBoxRelic2.Image = UIService.LoadBitmap(Relic2Item.TexImageResourceId);
					pictureBoxRelic2.BackColor = Color.FromArgb(127, ControlPaint.Dark(drgItm.ItemStyle.Color()));
					DragInfo.Cancel();
					AdjustComboBox(comboBoxRelic2);
					SoundService.PlayRandomItemDrop();
					_PreviewItem = null;
					return;
				}
				// Not good item class
				if (NotifyAction is not null) NotifyAction(Resources.ForgeMustSelectRelicOrItemWithRelic);
				SoundService.PlayRandomCancel();
			}

			pictureBoxDragDrop.Visible = true;
		}

		#region Validate

		private void ResetRelic2()
		{
			Relic2Item = null;
			pictureBoxRelic2.Image = null;
			pictureBoxRelic2.BackColor = PictureBoxBaseColor;
			comboBoxRelic2.Items.Clear();
		}

		private bool IsAlreadyPlaced(Item item)
			=> BaseItem == item
				|| SuffixItem == item
				|| PrefixItem == item
				|| Relic2Item == item
				|| Relic1Item == item;
		private bool MustSelectAtLeastAnotherItem()
		{
			// Must select at least another item
			if (PrefixItem is null
				&& SuffixItem is null
				&& Relic1Item is null
				&& Relic2Item is null)
			{
				if (NotifyAction is not null)
					NotifyAction(Resources.ForgeMustSelectItemToMerge);

				SoundService.PlayRandomCancel();
				return true;
			}

			return false;
		}

		private bool MustSelectBaseItem()
		{
			// Must select Base Item
			if (BaseItem is null)
			{
				if (NotifyAction is not null)
					NotifyAction(Resources.ForgeMustSelectBaseItem);

				SoundService.PlayRandomCancel();
				return true;
			}

			return false;
		}

		private bool StrictModePrefixSuffixMustBeSameType(PictureBox from, Item drgItm)
		{
			// Strict Mode Can't use Prefix/Suffix of another type
			if (IsStrictMode)
			{
				var baseClass = string.IsNullOrWhiteSpace(BaseItem?.ItemClass) ? "NoClass" : BaseItem.ItemClass;
				var drgItmClass = string.IsNullOrWhiteSpace(drgItm?.ItemClass) ? "NoClass" : drgItm.ItemClass;

				if (from == pictureBoxBaseItem
					&& BaseItem is not null
					&& drgItmClass != baseClass
				)
				{
					Notify(true);// Reset & Notify
				}

				if ((from == pictureBoxSuffix || from == pictureBoxPrefix)
					&& drgItmClass != baseClass
				)
				{
					return Notify(false);// Notify & Prevent Drop
				}
			}

			return false;

			bool Notify(bool reset)
			{
				pictureBoxDragDrop.Visible = true;
				if (NotifyAction is not null) NotifyAction(Resources.ForgeCantUsePrefixSuffixOfAnotherGearType);
				SoundService.PlayRandomCancel();
				if (reset) ResetPrefixSuffix();
				return true;
			}
		}

		private bool StrictModeCantUsePrefixSuffixOfHigherLevelThanBaseItem(PictureBox from, Item drgItm)
		{
			// Strict Mode Can't use Prefix/Suffix of higher level than baseitem
			if (IsStrictMode)
			{
				if (from == pictureBoxBaseItem
					&& BaseItem is not null
					&& (
						((PrefixItem?.GearLevel ?? 0) > drgItm.GearLevel)
						|| ((SuffixItem?.GearLevel ?? 0) > drgItm.GearLevel)
					)
				)
				{
					Notify(true);// Reset & Notify 
				}

				if ((from == pictureBoxPrefix || from == pictureBoxSuffix)
					&& (BaseItem.GearLevel < drgItm.GearLevel)
				)
				{
					return Notify(false);// Notify & Prevent Drop
				}
			}

			return false;

			bool Notify(bool reset)
			{
				pictureBoxDragDrop.Visible = true;
				if (NotifyAction is not null) NotifyAction(Resources.ForgeCantUsePrefixSuffixBetterThanBase);
				SoundService.PlayRandomCancel();
				if (reset) ResetPrefixSuffix();
				return true;
			}
		}

		private void ResetPrefixSuffix()
		{
			SuffixItem = PrefixItem = null;
			pictureBoxSuffix.Image = pictureBoxPrefix.Image = null;
			pictureBoxSuffix.BackColor = pictureBoxPrefix.BackColor = PictureBoxBaseColor;
			comboBoxPrefix.Items.Clear();
			comboBoxSuffix.Items.Clear();
		}

		private bool BaseItemFirst()
		{
			// BaseItem First
			if (BaseItem is null)
			{
				pictureBoxDragDrop.Visible = true;
				if (NotifyAction is not null) NotifyAction(Resources.ForgeMustSelectBaseItemFirst);
				SoundService.PlayRandomCancel();
				return true;
			}

			return false;
		}

		private bool StrictModeNoEpicLegendaryItem(Item drgItm)
		{
			// No Epic/Legendary item in Strict Mode
			if (IsStrictMode
				&& (drgItm.GearLevel > ItemGearLevel.Rare)
			)
			{
				pictureBoxDragDrop.Visible = true;
				if (NotifyAction is not null) NotifyAction(Resources.ForgeNoEpicLegendaryAllowed);
				SoundService.PlayRandomCancel();
				return true;
			}

			return false;
		}

		#endregion

		private void AdjustComboBox(ComboBox comboBox)
		{
			Item current = null;
			if (comboBox == comboBoxPrefix) current = PrefixItem;
			if (comboBox == comboBoxSuffix) current = SuffixItem;
			if (comboBox == comboBoxRelic1) current = Relic1Item;
			if (comboBox == comboBoxRelic2) current = Relic2Item;

			comboBox.Items.Clear();
			comboBox.Items.Add(new ForgeComboItem
			{
				Value = ItemPropertyType.Base,
				Text = Resources.ForgeUseBase,
			});

			if (current.HasPrefix)
				comboBox.Items.Add(new ForgeComboItem
				{
					Value = ItemPropertyType.Prefix,
					Text = Resources.ForgeUsePrefix,
				});

			if (current.HasSuffix)
				comboBox.Items.Add(new ForgeComboItem
				{
					Value = ItemPropertyType.Suffix,
					Text = Resources.ForgeUseSuffix,
				});

			if (current.HasRelicSlot1)
				comboBox.Items.Add(new ForgeComboItem
				{
					Value = ItemPropertyType.Relic1,
					Text = Resources.ForgeUseRelic1,
				});

			if (current.HasRelicSlot2)
				comboBox.Items.Add(new ForgeComboItem
				{
					Value = ItemPropertyType.Relic2,
					Text = Resources.ForgeUseRelic2,
				});

			comboBox.SelectedIndex = 0;// default on Base
		}

		private void scalingButtonReset_Click(object sender, EventArgs e) => ResetSelection();

		private void pictureBox_MouseEnter(object sender, EventArgs e)
		{
			var picbox = sender as PictureBox;
			Item itm = null;
			if (picbox == pictureBoxPrefix) itm = PrefixItem;
			if (picbox == pictureBoxSuffix) itm = SuffixItem;
			if (picbox == pictureBoxBaseItem) itm = PreviewItem;
			if (picbox == pictureBoxRelic1) itm = Relic1Item;
			if (picbox == pictureBoxRelic2) itm = Relic2Item;

			if (itm is null) return;

			ItemTooltip.ShowTooltip(ServiceProvider, itm, picbox, true);
		}

		private void pictureBox_MouseLeave(object sender, EventArgs e)
		{
			ItemTooltip.HideTooltip();
		}

		private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			var combo = sender as ComboBox;
			_PreviewItem = null;
		}
	}
}
