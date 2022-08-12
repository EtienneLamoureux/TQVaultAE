using Microsoft.Extensions.DependencyInjection;
using System;
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

namespace TQVaultAE.GUI.Components;

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

	private Item BaseItem;
	private SackCollection BaseSack;
	private Item PrefixItem;
	private SackCollection PrefixSack;
	private Item SuffixItem;
	private SackCollection SuffixSack;
	private Item Relic1Item;
	private SackCollection Relic1Sack;
	private Item Relic2Item;
	private SackCollection Relic2Sack;

	private ScalingRadioButton lastMode;

	private static RecordId SoundStricMode = @"Sounds\MONSTERS\GREECE\G_TELKINE\TELEKINEVOICE01.WAV";
	private static RecordId SoundRelaxMode = @"Sounds\MONSTERS\GREECE\G_TELKINE\TELEKINEVOICE02.WAV";
	private static RecordId SoundGameMode = @"Sounds\MONSTERS\GREECE\G_TELKINE\TELEKINEVOICE03.WAV";
	private static RecordId SoundGodMode = @"Sounds\AMBIENCE\RANDOMEVENT\TYPHONLAUGHDISTANCE.WAV";

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
	private bool IsGameMode => scalingRadioButtonGame.Checked;
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

	internal bool HardcoreMode;

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
		ItemProvider = serviceProvider.GetService<IItemProvider>();

		#region Font, Scaling, Translate, BackGround

		BackgroundImageLayout = ImageLayout.Stretch;
		BackgroundImage = Resources.caravan_bg;

		tableLayoutPanelForge.BackgroundImageLayout = ImageLayout.Stretch;
		tableLayoutPanelForge.BackgroundImage = Resources.StashPanel;

		scalingCheckBoxHardcore.Font =
		scalingRadioButtonGame.Font =
		scalingRadioButtonGod.Font =
		scalingRadioButtonRelax.Font =
		scalingRadioButtonStrict.Font =
			FontService.GetFont(10F, FontStyle.Regular, UIService.Scale);

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

		scalingRadioButtonGame.Text = Resources.ForgeGameMode;
		toolTip.SetToolTip(scalingRadioButtonGame, Resources.ForgeGameModeTT);
		scalingRadioButtonGame.ForeColor = TQColor.Green.Color();

		scalingCheckBoxHardcore.Text = Resources.GlobalHardcore;
		toolTip.SetToolTip(scalingCheckBoxHardcore, Resources.ForgeHardcoreTT);
		scalingCheckBoxHardcore.ForeColor = TQColor.Red.Color();

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

	private void InvalidateItemCacheAll(params Item[] items)
	{
		ItemTooltip.InvalidateCache(items);
		BagButtonTooltip.InvalidateCache(items);
		ItemProvider.InvalidateFriendlyNamesCache(items);
	}

	private void HardcoreDeleteMaterialItems()
	{
		if (this.HardcoreMode)
		{
			if (PrefixSack is not null)
			{
				PrefixSack.RemoveItem(PrefixItem);
				InvalidateItemCacheAll(PrefixItem);
			}

			if (SuffixSack is not null)
			{
				SuffixSack.RemoveItem(SuffixItem);
				InvalidateItemCacheAll(SuffixItem);
			}

			if (Relic1Sack is not null)
			{
				Relic1Sack.RemoveItem(Relic1Item);
				InvalidateItemCacheAll(Relic1Item);
			}

			if (Relic2Sack is not null)
			{
				Relic2Sack.RemoveItem(Relic2Item);
				InvalidateItemCacheAll(Relic2Item);
			}
		}

		this.FindForm().Refresh();
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
				if (selectedPropreties is not null)
				{
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
		}

		if (SuffixItem is not null)
		{
			itm.suffixInfo = null;
			if (!IsGodMode)
				itm.suffixID = SuffixItem.suffixID;
			else
			{ // God Mode
				var selectedPropreties = comboBoxSuffix.SelectedItem as ForgeComboItem;
				if (selectedPropreties is not null)
				{
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
				if (selectedPropreties is not null)
				{
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
				if (selectedPropreties is not null)
				{
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
		SuffixItem = null;

		BaseSack = null;

		comboBoxPrefix.Items.Clear();
		comboBoxSuffix.Items.Clear();
		comboBoxRelic1.Items.Clear();
		comboBoxRelic2.Items.Clear();

		ResetPreviewItem();
	}

	private void ResetPreviewItem()
	{
		if (_PreviewItem is not null)
			ItemTooltip.InvalidateCache(_PreviewItem);// Invalide old preview item
		_PreviewItem = null;
	}

	private void CancelButton_Click(object sender, EventArgs e)
	{
		ResetSelection();

		if (CancelAction is not null)
			CancelAction();
	}

	private void ForgeButton_Click(object sender, EventArgs e)
	{
		ForgeItem();
	}

	private void ForgeItem()
	{
		if (MustSelectBaseItem())
			return;

		if (MustSelectAtLeastAnotherItem())
			return;

		// Merge item properties into Base item
		var itm = BaseItem;

		MergeItemProperties(itm);

		ItemProvider.GetDBData(itm);

		BaseSack.IsModified = itm.IsModified = true;

		HardcoreDeleteMaterialItems();

		ItemTooltip.InvalidateCache(itm);
		ItemProvider.InvalidateFriendlyNamesCache(itm);

		ResetSelection();

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
		if (IsGameMode)
		{
			SoundService.PlaySound(SoundGameMode);

			// Reset on Mode downgrade
			if (lastMode == scalingRadioButtonGod || lastMode == scalingRadioButtonRelax || lastMode == scalingRadioButtonStrict)
				ResetSelection();

			HideGodModeRows();
			lastMode = scalingRadioButtonGame;
			return;
		}

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
			// TODO try to make transparency work with a picturebox with an overrided Paint() event https://stackoverflow.com/questions/34673496/override-picturbox-onpaint-event-to-rotate-the-image-create-custom-picturebox
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
		// tout le form sauf ce control
		this.FindForm().ProcessAllControlsWithExclusion(c =>
			{
				if (c == this)
					return true;// Exclude myself and my children

				c.MouseMove -= Parent_MouseMove;

				return false;
			}
		);
	}

	private void Parent_MouseMove(object sender, EventArgs e)
	{
		// Out of Panel
		if (pictureBoxDragDrop.Visible)
		{
			UnsubscribeExternalMouseMove();
			pictureBoxDragDrop.Image = dragingBmp = null;
			pictureBoxDragDrop.Visible = false;
		}
	}

	private void pictureBoxDragDrop_MouseMove(object sender, MouseEventArgs e)
	{
		if (IsDraging)
		{
			pictureBoxDragDrop.Location = CurrentDragPicturePosition;
		}
	}


	/// <summary>
	/// Drop <see cref="pictureBoxDragDrop"/>
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void pictureBoxDragDrop_Click(object sender, EventArgs e)
	{
		if (ItemMustBeInsideASack())
			return;

		if (IsAlreadyPlaced(DragInfo.Item))
			return;

		pictureBoxDragDrop.Visible = false;

		var found = tableLayoutPanelForge.GetChildAtPoint(
			tableLayoutPanelForge.PointToClient(Cursor.Position)
			, GetChildAtPointSkip.Invisible | GetChildAtPointSkip.Transparent
		) as PictureBox;

		var drgItm = DragInfo.Item;
		var drgSack = DragInfo.Sack;

		if (found == pictureBoxBaseItem)
		{
			if (GameModePrefixSuffixMustBeSameBaseItem(found, drgItm))
				return;

			if (GameModePrefixSuffixMustHaveLevelRequirementLowerThanEqualThanBaseItem(found, drgItm))
				return;

			if (StrictModeNoEpicLegendaryItem(drgItm))
				return;

			if (StrictModePrefixSuffixMustBeSameType(found, drgItm))
				return;

			if (StrictModeCantUsePrefixSuffixOfHigherLevelThanBaseItem(found, drgItm))
				return;

			// Strict reset relic 2 if non Tinkered forge result
			if ((IsStrictMode || IsGameMode)
				&& Relic2Item is not null
				&& SuffixItem is not null
				&& !SuffixItem.AcceptExtraRelic
				&& drgItm.AcceptExtraRelic
			)
			{
				ResetRelic2();
			}

			// Strict/Relax reset relic 1 & 2 if non allowed gear type
			if (!IsGodMode)
			{
				if (Relic1Item is not null
					&& (
						(Relic1Item.IsRelic && !drgItm.IsRelicAllowed(Relic1Item))
						|| (Relic1Item.HasRelicSlot1 && !drgItm.IsRelicAllowed(Relic1Item.relicID))
					)
				)
				{
					ResetRelic1();
				}
				if (Relic2Item is not null
					&& (
						(Relic2Item.IsRelic && !drgItm.IsRelicAllowed(Relic2Item))
						|| (Relic2Item.HasRelicSlot1 && !drgItm.IsRelicAllowed(Relic2Item.relicID))
					)
				)
				{
					ResetRelic2();
				}
			}

			// Only Weapon/Armor/Jewellery allowed
			if (drgItm.IsWeaponShield || drgItm.IsArmor || drgItm.IsJewellery || IsGodMode)
			{
				BaseItem = drgItm;
				BaseSack = drgSack;
				pictureBoxBaseItem.Image = UIService.LoadBitmap(BaseItem.TexImageResourceId);
				pictureBoxBaseItem.BackColor = Color.FromArgb(127, ControlPaint.Dark(drgItm.ItemStyle.Color()));
				DragInfo.Cancel();
				SoundService.PlayRandomItemDrop();
				ResetPreviewItem();
				return;
			}
			// Not good item class
			this.UIService.NotifyUser(Resources.ForgeMustSelectArmorOrWeaponOrJewellery, TQColor.Red);

			SoundService.PlayRandomCancel();
		}

		if (found == pictureBoxPrefix)
		{
			if (BaseItemFirst())
				return;

			if (GameModePrefixSuffixMustBeSameBaseItem(found, drgItm))
				return;

			if (GameModePrefixSuffixMustHaveLevelRequirementLowerThanEqualThanBaseItem(found, drgItm))
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
				this.UIService.NotifyUser(Resources.ForgeMustHavePrefixOutsideOfGodMode, TQColor.Red);
				SoundService.PlayRandomCancel();
				return;
			}

			// Only Weapon/Armor/Jewellery allowed
			if (drgItm.IsWeaponShield || drgItm.IsArmor || drgItm.IsJewellery || IsGodMode)
			{
				PrefixItem = drgItm;
				PrefixSack = drgSack;
				pictureBoxPrefix.Image = UIService.LoadBitmap(PrefixItem.TexImageResourceId);
				pictureBoxPrefix.BackColor = Color.FromArgb(127, ControlPaint.Dark(drgItm.ItemStyle.Color()));
				DragInfo.Cancel();
				AdjustComboBox(comboBoxPrefix);
				SoundService.PlayRandomItemDrop();
				ResetPreviewItem();
				return;
			}

			// Not good item class
			this.UIService.NotifyUser(Resources.ForgeMustSelectArmorOrWeaponOrJewellery, TQColor.Red);
			SoundService.PlayRandomCancel();
		}

		if (found == pictureBoxSuffix)
		{
			if (BaseItemFirst())
				return;

			if (GameModePrefixSuffixMustBeSameBaseItem(found, drgItm))
				return;

			if (GameModePrefixSuffixMustHaveLevelRequirementLowerThanEqualThanBaseItem(found, drgItm))
				return;

			if (StrictModeNoEpicLegendaryItem(drgItm))
				return;

			if (StrictModePrefixSuffixMustBeSameType(found, drgItm))
				return;

			if (StrictModeCantUsePrefixSuffixOfHigherLevelThanBaseItem(found, drgItm))
				return;

			// Strict mode : reset relic 2 if non Tinkered forge result
			if ((IsStrictMode || IsGameMode)
				&& Relic2Item is not null
				&& !drgItm.AcceptExtraRelic
			)
			{
				ResetRelic2();
			}

			// Strict & Relax mode need item having suffix
			if (!IsGodMode && !drgItm.HasSuffix)
			{
				pictureBoxDragDrop.Visible = true;
				this.UIService.NotifyUser(Resources.ForgeMustHaveSuffixOutsideOfGodMode, TQColor.Red);
				SoundService.PlayRandomCancel();
				return;
			}

			// Only Weapon/Armor/Jewellery allowed
			if (drgItm.IsWeaponShield || drgItm.IsArmor || drgItm.IsJewellery || IsGodMode)
			{
				SuffixItem = drgItm;
				SuffixSack = drgSack;
				pictureBoxSuffix.Image = UIService.LoadBitmap(SuffixItem.TexImageResourceId);
				pictureBoxSuffix.BackColor = Color.FromArgb(127, ControlPaint.Dark(drgItm.ItemStyle.Color()));
				DragInfo.Cancel();
				AdjustComboBox(comboBoxSuffix);
				SoundService.PlayRandomItemDrop();
				ResetPreviewItem();
				return;
			}
			// Not good item class
			this.UIService.NotifyUser(Resources.ForgeMustSelectArmorOrWeaponOrJewellery, TQColor.Red);
			SoundService.PlayRandomCancel();
		}

		if (found == pictureBoxRelic1)
		{
			if (BaseItemFirst())
				return;

			// Strict mode need relic
			if (StrictModeMustBeRelic(drgItm))
				return;

			// Relax mode need item having relic or a relic
			if (RelaxModeMustHaveRelic(drgItm))
				return;

			if (StrictRelaxEnforceRelicGearConstraint(drgItm))
				return;

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
				Relic1Sack = drgSack;
				pictureBoxRelic1.Image = UIService.LoadBitmap(Relic1Item.TexImageResourceId);
				pictureBoxRelic1.BackColor = Color.FromArgb(127, ControlPaint.Dark(drgItm.ItemStyle.Color()));
				DragInfo.Cancel();
				AdjustComboBox(comboBoxRelic1);
				SoundService.PlayRandomItemDrop();
				ResetPreviewItem();
				return;
			}
			// Not good item class
			this.UIService.NotifyUser(Resources.ForgeMustSelectRelicOrItemWithRelic, TQColor.Red);

			SoundService.PlayRandomCancel();
		}

		if (found == pictureBoxRelic2)
		{
			if (BaseItemFirst())
				return;

			// Strict mode need relic
			if (StrictModeMustBeRelic(drgItm))
				return;

			// Relax mode need item having relic or a relic
			if (RelaxModeMustHaveRelic(drgItm))
				return;

			if (StrictRelaxEnforceRelicGearConstraint(drgItm))
				return;

			// Strict Mode only allowed on BaseItem "of the Tinkered"
			if ((IsStrictMode || IsGameMode)
				&& !(
					// No Suffix Item && BaseItem of Tinkerer
					(SuffixItem is null && BaseItem.AcceptExtraRelic)
					// Or Suffix Item of Tinkerer
					|| (SuffixItem is not null && SuffixItem.AcceptExtraRelic)
				)
			)
			{
				pictureBoxDragDrop.Visible = true;
				this.UIService.NotifyUser(string.Format(Resources.ForgeMustHaveTinkeredSuffix, OfTheTinkererTranslation), TQColor.Red);
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
				Relic2Sack = drgSack;
				pictureBoxRelic2.Image = UIService.LoadBitmap(Relic2Item.TexImageResourceId);
				pictureBoxRelic2.BackColor = Color.FromArgb(127, ControlPaint.Dark(drgItm.ItemStyle.Color()));
				DragInfo.Cancel();
				AdjustComboBox(comboBoxRelic2);
				SoundService.PlayRandomItemDrop();
				ResetPreviewItem();
				return;
			}
			// Not good item class
			this.UIService.NotifyUser(Resources.ForgeMustSelectRelicOrItemWithRelic, TQColor.Red);
			SoundService.PlayRandomCancel();
		}

		pictureBoxDragDrop.Visible = true;
	}


	#region Validate

	private bool ItemMustBeInsideASack()
	{
		// Happen when i copy an item without droping it in a sack first
		if (DragInfo.Sack is null)
		{
			this.UIService.NotifyUser(Resources.ForgeItemMustBeStoredFirst, TQColor.Red);

			SoundService.PlayRandomCancel();
			return true;
		}

		return false;
	}

	/// <summary>
	/// Strict/Relax : Enforce relic gear constraint 
	/// </summary>
	/// <param name="drgItm"></param>
	/// <returns></returns>
	private bool StrictRelaxEnforceRelicGearConstraint(Item drgItm)
	{
		if (!IsGodMode
			&& (
				(drgItm.IsRelic && !BaseItem.IsRelicAllowed(drgItm))
				|| (drgItm.HasRelicSlot1 && !BaseItem.IsRelicAllowed(drgItm.relicID))
			)
		)
		{
			pictureBoxDragDrop.Visible = true;
			this.UIService.NotifyUser(Resources.ForgeRelicNotAllowed, TQColor.Red);
			SoundService.PlayRandomCancel();
			return true;
		}
		return false;
	}

	/// <summary>
	/// Relax mode need item having relic or a relic
	/// </summary>
	/// <param name="drgItm"></param>
	/// <returns></returns>
	private bool RelaxModeMustHaveRelic(Item drgItm)
	{
		// Relax mode need item having relic
		if (IsRelaxMode
			&& !(drgItm.IsRelic || drgItm.HasRelicSlot1)
		)
		{
			pictureBoxDragDrop.Visible = true;
			this.UIService.NotifyUser(Resources.ForgeMustHaveRelicInRelaxMode, TQColor.Red);
			SoundService.PlayRandomCancel();
			return true;
		}
		return false;
	}

	/// <summary>
	/// Strict mode need relic
	/// </summary>
	/// <param name="drgItm"></param>
	/// <returns></returns>
	private bool StrictModeMustBeRelic(Item drgItm)
	{
		// Strict mode need relic
		if ((IsStrictMode || IsGameMode)
			&& !drgItm.IsRelic
		)
		{
			pictureBoxDragDrop.Visible = true;
			this.UIService.NotifyUser(Resources.ForgeMustBeRelicInStrictMode, TQColor.Red);
			SoundService.PlayRandomCancel();
			return true;
		}
		return false;
	}

	private void ResetRelic1()
	{
		Relic1Item = null;
		pictureBoxRelic1.Image = null;
		pictureBoxRelic1.BackColor = PictureBoxBaseColor;
		comboBoxRelic1.Items.Clear();
	}

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
			this.UIService.NotifyUser(Resources.ForgeMustSelectItemToMerge, TQColor.Red);

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
			this.UIService.NotifyUser(Resources.ForgeMustSelectBaseItem, TQColor.Red);

			SoundService.PlayRandomCancel();
			return true;
		}

		return false;
	}

	private bool GameModePrefixSuffixMustHaveLevelRequirementLowerThanEqualThanBaseItem(PictureBox from, Item drgItm)
	{
		if (IsGameMode)
		{
			int drgLvl = 0, baseLvl = 0;
			var reqs = ItemProvider.GetRequirementVariables(drgItm);
			if (reqs.TryGetValue(Variable.KEY_LEVELREQ, out var varLvl))
				drgLvl = varLvl.GetInt32(0);

			if (BaseItem is not null)
			{
				reqs = ItemProvider.GetRequirementVariables(BaseItem);
				if (reqs.TryGetValue(Variable.KEY_LEVELREQ, out varLvl))
					baseLvl = varLvl.GetInt32(0);
			}

			if (from == pictureBoxBaseItem
				&& BaseItem is not null
				&& drgLvl > baseLvl
			)
			{
				Notify(true);// Reset & Notify
			}

			if ((from == pictureBoxSuffix || from == pictureBoxPrefix)
				&& drgLvl > baseLvl
			)
			{
				return Notify(false);// Notify & Prevent Drop
			}
		}

		return false;

		bool Notify(bool reset)
		{
			pictureBoxDragDrop.Visible = true;
			this.UIService.NotifyUser(Resources.ForgeCantUsePrefixSuffixHavingGreaterReqLevelThanBaseItem, TQColor.Red);
			SoundService.PlayRandomCancel();
			if (reset) ResetPrefixSuffix();
			return true;
		}
	}

	private bool GameModePrefixSuffixMustBeSameBaseItem(PictureBox from, Item drgItm)
	{
		if (IsGameMode)
		{
			var baseId = RecordId.IsNullOrEmpty(BaseItem?.BaseItemId) ? RecordId.Empty : BaseItem.BaseItemId;
			var drgItmBaseId = RecordId.IsNullOrEmpty(drgItm?.BaseItemId) ? RecordId.Empty : drgItm.BaseItemId;

			if (from == pictureBoxBaseItem
				&& BaseItem is not null
				&& drgItmBaseId != baseId
			)
			{
				Notify(true);// Reset & Notify
			}

			if ((from == pictureBoxSuffix || from == pictureBoxPrefix)
				&& drgItmBaseId != baseId
			)
			{
				return Notify(false);// Notify & Prevent Drop
			}
		}

		return false;

		bool Notify(bool reset)
		{
			pictureBoxDragDrop.Visible = true;
			this.UIService.NotifyUser(Resources.ForgeCantUsePrefixSuffixOfAnotherBaseItem, TQColor.Red);
			SoundService.PlayRandomCancel();
			if (reset) ResetPrefixSuffix();
			return true;
		}
	}

	/// <summary>
	/// Strict Mode Can't use Prefix/Suffix of another type
	/// </summary>
	/// <param name="from"></param>
	/// <param name="drgItm"></param>
	/// <returns></returns>
	private bool StrictModePrefixSuffixMustBeSameType(PictureBox from, Item drgItm)
	{
		if (IsStrictMode || IsGameMode)
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
			this.UIService.NotifyUser(Resources.ForgeCantUsePrefixSuffixOfAnotherGearType, TQColor.Red);
			SoundService.PlayRandomCancel();
			if (reset) ResetPrefixSuffix();
			return true;
		}
	}

	/// <summary>
	/// Strict Mode Can't use Prefix/Suffix of higher level than baseitem
	/// </summary>
	/// <param name="from"></param>
	/// <param name="drgItm"></param>
	/// <returns></returns>
	private bool StrictModeCantUsePrefixSuffixOfHigherLevelThanBaseItem(PictureBox from, Item drgItm)
	{
		if (IsStrictMode || IsGameMode)
		{
			if (from == pictureBoxBaseItem
				&& BaseItem is not null
				&& (
					((PrefixItem?.Rarity ?? 0) > drgItm.Rarity)
					|| ((SuffixItem?.Rarity ?? 0) > drgItm.Rarity)
				)
			)
			{
				Notify(true);// Reset & Notify 
			}

			if ((from == pictureBoxPrefix || from == pictureBoxSuffix)
				&& (BaseItem.Rarity < drgItm.Rarity)
			)
			{
				return Notify(false);// Notify & Prevent Drop
			}
		}

		return false;

		bool Notify(bool reset)
		{
			pictureBoxDragDrop.Visible = true;
			this.UIService.NotifyUser(Resources.ForgeCantUsePrefixSuffixBetterThanBase, TQColor.Red);
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
			this.UIService.NotifyUser(Resources.ForgeMustSelectBaseItemFirst, TQColor.Red);
			SoundService.PlayRandomCancel();
			return true;
		}

		return false;
	}

	/// <summary>
	/// No Epic/Legendary item in Strict Mode
	/// </summary>
	/// <param name="drgItm"></param>
	/// <returns></returns>
	private bool StrictModeNoEpicLegendaryItem(Item drgItm)
	{
		if ((IsStrictMode || IsGameMode)
			&& (drgItm.Rarity > Rarity.Rare)
		)
		{
			pictureBoxDragDrop.Visible = true;
			this.UIService.NotifyUser(Resources.ForgeNoEpicLegendaryAllowed, TQColor.Red);
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
		if (picbox == pictureBoxRelic1) itm = Relic1Item;
		if (picbox == pictureBoxRelic2) itm = Relic2Item;
		if (picbox == pictureBoxBaseItem) itm = PreviewItem;

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
		ResetPreviewItem();
	}

	private void scalingCheckBoxHardcore_CheckedChanged(object sender, EventArgs e)
	{
		this.HardcoreMode = scalingCheckBoxHardcore.Checked;
	}
}
