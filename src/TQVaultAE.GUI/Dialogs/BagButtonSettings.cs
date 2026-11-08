﻿﻿﻿using Microsoft.Extensions.Logging;
using TQVaultAE.GUI.Components.Buttons;
using TQVaultAE.GUI.Components.Icons;
using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.Presentation.Win32.Contracts;
using TQVaultAE.Presentation.Win32.Models;
using TQVaultAE.Presentation.Win32;
using TQVaultAE.Presentation;
using System.Collections.ObjectModel;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components;
using System.Diagnostics;
using TQVaultAE.Application.Contracts.Services;
using Cell = TQVaultAE.GUI.Components.Icons.IconGrid.Cell;

namespace TQVaultAE.GUI.Dialogs;

public partial class BagButtonSettings : VaultForm
{
	// DI
	private readonly IIconService iconService;
	private readonly ILogger<BagButtonSettings> Log;

	// Drag & Drop stuff
	private Cell? DraggedCell;
	private ItemDragCursor? DragCursor;

	/// <summary>Grid showing the icons of each category.</summary>
	private readonly Dictionary<IconCategory, IconGrid> GridByCategory;

	// From Database
	private ReadOnlyCollection<IconInfo> Pictures = [];

	/// <summary>Icon picture for each resource id, from the first icon that has it.</summary>
	private readonly Dictionary<RecordId, Cell> CellById = [];

	// Pictures chosen for the button, kept while the default icon is shown.
	private Cell? SelectedOn;
	private Cell? SelectedOff;
	private Cell? SelectedOver;

	private BagButtonIconInfo _DefaultBagButtonIconInfo = new()
	{
		DisplayMode = BagButtonDisplayMode.Default
	};

	#region CurrentBagButton

	// Set through CurrentBagButton before the dialog is shown.
	private BagButtonBase _CurrentBagButton = null!;
	private bool IsLoaded;

	internal BagButtonBase CurrentBagButton
	{
		set
		{
			_CurrentBagButton = value;
			ApplySelectedBagPath();
		}
	}

	#endregion

#if DEBUG
	// For Design Mode
#pragma warning disable CS8618 // Designer only: the runtime constructor sets the services and state.
	public BagButtonSettings() => InitializeComponent();
#pragma warning restore CS8618
#endif

	public BagButtonSettings(MainForm instance
		, IIconService iconService
		, ILogger<BagButtonSettings> log
	) : base(instance.Services)
	{
		this.Owner = instance;
		this.iconService = iconService;
		this.Log = log;

		this.InitializeComponent();

		this.MinimizeBox = false;
		this.NormalizeBox = false;
		this.MaximizeBox = true;

		// Map [Esc] and [Enter]
		this.CancelButton = this.cancelButton;
		this.AcceptButton = this.applyButton;

		#region Translations

		this.Text = Resources.BagButtonSettingsTitle;

		// Selected button
		this.scalingLabelSelectedButton.Text = Resources.BagButtonSettingsSelectedButton;

		// Button properties
		this.groupBoxEdit.Text = Resources.BagButtonSettingsButtonProperties;
		this.scalingRadioButtonDefaultIcon.Text = Resources.BagButtonSettingsUseDefault;
		this.scalingRadioButtonCustomIcon.Text = Resources.BagButtonSettingsCustomIcon;
		this.scalingCheckBoxNumber.Text = Resources.BagButtonSettingsNumber;
		this.scalingCheckBoxLabel.Text = Resources.BagButtonSettingsLabel;

		// Selected pictures
		this.groupBoxSelectedPictures.Text = Resources.BagButtonSettingsSelectedPictures;
		this.radioButtonPictureSimple.Text = Resources.BagButtonSettingsModeSimple;
		this.radioButtonPictureDetailed.Text = Resources.BagButtonSettingsModeDetailed;
		this.groupBoxOffBitmap.Text = Resources.BagButtonSettingsOffBitmap;
		this.groupBoxOnBitmap.Text = Resources.BagButtonSettingsOnBitmap;
		this.groupBoxOverBitmap.Text = Resources.BagButtonSettingsOverBitmap;
		this.groupBoxSimpleImage.Text = Resources.BagButtonSettingsImageBitmap;

		// Available pictures
		this.groupBoxAvailablePictures.Text = Resources.BagButtonSettingsAvailablePictures;
		this.scalingLabelDragDropNotice.Text = Resources.BagButtonSettingsDragDropNotice;

		// Tabs
		this.tabPageSkills.Text = TranslationService.TranslateXTag("tagWindowName02");
		this.tabPageRelics.Text = TranslationService.TranslateXTag("tagTutorialTip18Title");
		this.tabPageArtifacts.Text = TranslationService.TranslateXTag("xtagTutorialTip05Title");
		this.tabPageJewellery.Text = Resources.GlobalJewellery;
		this.tabPageScrolls.Text = TranslationService.TranslateXTag("xtagTutorialTip06Title");
		this.tabPagePotions.Text = TranslationService.TranslateXTag("tagTutorialTip14Title");
		this.tabPageButtons.Text = Resources.GlobalButtons;
		this.tabPageMisc.Text = Resources.GlobalMiscellaneous;
		this.tabPageHelmets.Text = TranslationService.TranslateXTag("tagItemHelmet");
		this.tabPageArmbands.Text = TranslationService.TranslateXTag("tagItemArmband");
		this.tabPageShields.Text = TranslationService.TranslateXTag("tagItemBuckler");
		this.tabPageGreaves.Text = TranslationService.TranslateXTag("tagItemGreaves");

		// Exit
		this.applyButton.Text = TranslationService.TranslateXTag("tagMenuButton07");
		this.cancelButton.Text = TranslationService.TranslateXTag("tagMenuButton06");

		#endregion

		#region Custom font & Scaling

		this.Font = FontService.GetFont(9F, FontStyle.Regular, GraphicsUnit.Point);

		var title = FontService.GetFont(12F, FontStyle.Regular, GraphicsUnit.Point);
		this.scalingLabelSelectedButton.Font = title;
		this.groupBoxEdit.Font = title;
		this.groupBoxSelectedPictures.Font = title;
		this.groupBoxAvailablePictures.Font = title;
		this.cancelButton.Font = title;
		this.applyButton.Font = title;

		var text = FontService.GetFontLight(11.25F, FontStyle.Regular, GraphicsUnit.Point);
		this.scalingLabelSelectedButtonValue.Font = text;
		this.groupBoxEdit.ProcessAllControls(ctr => ctr.Font = text);
		this.groupBoxSelectedPictures.ProcessAllControls(ctr => ctr.Font = text);
		this.groupBoxAvailablePictures.ProcessAllControls(ctr => ctr.Font = text);

		ScaleControl(this.UIService, this.cancelButton);
		ScaleControl(this.UIService, this.applyButton);

		this.FitExitButtons();
		this.ApplyBorderPadding();


		#endregion

#if DEBUG
		this.scalingTextBoxDebug.Visible = true;
#endif

		this.GridByCategory = new()
		{
			[IconCategory.Artifacts] = this.iconGridArtifacts,
			[IconCategory.Buttons] = this.iconGridButtons,
			[IconCategory.Jewellery] = this.iconGridJewellery,
			[IconCategory.Misc] = this.iconGridMisc,
			[IconCategory.Potions] = this.iconGridPotions,
			[IconCategory.Relics] = this.iconGridRelics,
			[IconCategory.Scrolls] = this.iconGridScrolls,
			[IconCategory.Skills] = this.iconGridSkills,
			[IconCategory.Helmets] = this.iconGridHelmets,
			[IconCategory.Shields] = this.iconGridShields,
			[IconCategory.Armbands] = this.iconGridArmbands,
			[IconCategory.Greaves] = this.iconGridGreaves,
		};

		foreach (var grid in this.GridByCategory.Values)
		{
			// Magnifier
			grid.HoverChanged += this.Grid_HoverChanged;

			// Drag & Drop events
			grid.MouseDown += this.Grid_MouseDown;
			grid.MouseMove += this.Grid_MouseMove;
			grid.MouseUp += this.Grid_MouseUp;
		}
	}

	private void ApplySelectedBagPath()
	{
		var vaultPanel = _CurrentBagButton.Parent as VaultPanel;
		this.scalingLabelSelectedButtonValue.Text = string.Format("{0} - {1} - ({2})"
			, vaultPanel?.Player?.PlayerName
			, _CurrentBagButton.ButtonNumber + 1
			, _CurrentBagButton.Sack?.BagButtonIconInfo?.Label
		);
	}

	private async void BagButtonSettings_Load(object? sender, EventArgs e)
	{
		// async void event handler: nothing may escape it, or the application goes down.
		try
		{
			await this.LoadSettingsAsync();
		}
		catch (Exception ex)
		{
			this.Log.LogError(ex, "Failed to show the bag button settings of button {Button}", this._CurrentBagButton?.ButtonNumber);
			this.Services.MessageBoxes.ShowException(ex);
		}
	}

	/// <summary>
	/// Fills the icon grids on the first showing, then shows the icon of the current bag button.
	/// </summary>
	private async Task LoadSettingsAsync()
	{
		if (!IsLoaded)
		{
			TogglePicturePickup();

			// The icons are usually loaded in the background after startup; until then the form shows without them.
			var watch = Stopwatch.StartNew();
			this.groupBoxAvailablePictures.Enabled = false;
			this.UseWaitCursor = true;
			try
			{
				this.Pictures = await this.iconService.GetIconDatabaseAsync();
			}
			catch (Exception ex)
			{
				this.Log.LogError(ex, "Failed to load the bag button icons");
			}
			finally
			{
				this.UseWaitCursor = false;
				this.groupBoxAvailablePictures.Enabled = true;
			}

			if (this.IsDisposed)
				return;

			long waited = watch.ElapsedMilliseconds;
			this.FillGrids();
			this.DisplayPictures();
			this.Log.LogInformation("Bag button settings ready in {Elapsed} ms ({Waited} ms waiting for the icons, {Count} icons)"
				, watch.ElapsedMilliseconds, waited, this.CellById.Count);

			IsLoaded = true;
		}

		ApplyIconInfo();
	}

	/// <summary>
	/// Puts each picture of each icon in the grid of its category, once.
	/// </summary>
	private void FillGrids()
	{
		var cells = this.GridByCategory.Keys.ToDictionary(category => category, _ => new List<Cell>());

		foreach (var (info, id) in IconInfo.DistinctPictures(this.Pictures))
		{
			var cell = new Cell(info, id);
			this.CellById.Add(id, cell);
			if (cells.TryGetValue(info.Category, out var list))
				list.Add(cell);
		}

		foreach (var (category, grid) in this.GridByCategory)
			grid.SetCells(cells[category]);
	}

	/// <summary>
	/// Adjuste form display at first load
	/// </summary>
	private void ApplyIconInfo()
	{
		var info = _CurrentBagButton.Sack?.BagButtonIconInfo;

		if (info is null)
			info = _DefaultBagButtonIconInfo;

		// Apply previously saved icons
		this.SelectedOn = CellOf(info.On);
		this.SelectedOff = CellOf(info.Off);
		this.SelectedOver = CellOf(info.Over);

		this.scalingTextBoxLabel.Text = info.Label;

		if (info.DisplayMode.HasFlag(BagButtonDisplayMode.CustomIcon))
			this.scalingRadioButtonCustomIcon.Checked = true;

		if (info.DisplayMode.HasFlag(BagButtonDisplayMode.Label))
			this.scalingCheckBoxLabel.Checked = true;

		if (info.DisplayMode.HasFlag(BagButtonDisplayMode.Number))
			this.scalingCheckBoxNumber.Checked = true;

		if (info.DisplayMode == BagButtonDisplayMode.Default)
			this.scalingRadioButtonDefaultIcon.Checked = true;

		this.SyncOptionsEnabled();
		this.ShowSelection();

		Cell? CellOf(RecordId? id)
			=> !RecordId.IsNullOrEmpty(id) && this.CellById.TryGetValue(id!, out var cell) ? cell : null;
	}

	/// <summary>
	/// Shows the chosen pictures in the placeholders, or the default bag pictures when the default icon is used.
	/// </summary>
	private void ShowSelection()
	{
		if (this.scalingRadioButtonDefaultIcon.Checked)
		{
			this.pictureBoxOn.Image = Images.inventorybagup01;
			this.pictureBoxOff.Image = this.pictureBoxSimple.Image = Images.inventorybagdown01;
			this.pictureBoxOver.Image = Images.inventorybagover01;
			return;
		}

		this.pictureBoxOn.Image = this.SelectedOn?.Bitmap;
		this.pictureBoxOff.Image = this.pictureBoxSimple.Image = this.SelectedOff?.Bitmap;
		this.pictureBoxOver.Image = this.SelectedOver?.Bitmap;
	}

	/// <summary>Picture chosen for the placeholder <paramref name="pictureBox"/>.</summary>
	private Cell? SelectionOf(object? pictureBox)
	{
		if (pictureBox == this.pictureBoxOn) return this.SelectedOn;
		if (pictureBox == this.pictureBoxOff || pictureBox == this.pictureBoxSimple) return this.SelectedOff;
		if (pictureBox == this.pictureBoxOver) return this.SelectedOver;
		return null;
	}

	void TogglePicturePickup()
	{
		if (this.radioButtonPictureSimple.Checked)
		{
			groupBoxOnBitmap.Visible = false;
			groupBoxOffBitmap.Visible = false;
			groupBoxOverBitmap.Visible = false;
			groupBoxSimpleImage.Visible = true;
		}
		else
		{
			groupBoxOnBitmap.Visible = true;
			groupBoxOffBitmap.Visible = true;
			groupBoxOverBitmap.Visible = true;
			groupBoxSimpleImage.Visible = false;
		}
	}

	private void DisplayPictures()
	{
		// Simple mode shows the Off picture of each icon only; the others come with it.
		Func<Cell, bool>? filter = this.radioButtonPictureSimple.Checked ? cell => cell.Id == cell.Info.Off : null;
		foreach (var grid in this.GridByCategory.Values)
			grid.Filter = filter;
	}

	private void CherryPickIconSimpleMode(IconInfo info)
	{
		var (on, off, over) = info.SimpleModePictures;
		this.SelectedOn = CellOf(on);
		this.SelectedOff = CellOf(off);
		this.SelectedOver = CellOf(over);

		Cell? CellOf(RecordId? id)
			=> id is null ? null : new Cell(info, id);
	}

	#region Magnifier

	private void Grid_HoverChanged(object? sender, Cell? cell)
	{
		if (this.DraggedCell is not null || sender is not IconGrid grid)
			return;

		if (cell is null)
		{
			this.iconMagnifier.Visible = false;
			return;
		}

		var bounds = grid.BoundsOf(cell);
		this.ShowMagnifier(cell, grid.PointToScreen(bounds.Location), bounds.Height);
	}

	private void PicOff_MouseEnter(object? sender, EventArgs e)
	{
		if (this.DraggedCell is not null) return;
		// No magnifier from icon placeholders for default icon
		if (this.scalingRadioButtonDefaultIcon.Checked) return;

		var picB = (PictureBox)sender!;
		var cell = this.SelectionOf(picB);
		if (cell is null) return;

		this.ShowMagnifier(cell, picB.PointToScreen(Point.Empty), picB.Height);
	}

	private void PicOff_MouseLeave(object? sender, EventArgs e)
	{
		if (this.DraggedCell is not null) return;

		this.iconMagnifier.Visible = false;
	}

	/// <summary>Shows <paramref name="cell"/> enlarged, below the icon at <paramref name="screenLocation"/>.</summary>
	private void ShowMagnifier(Cell cell, Point screenLocation, int iconHeight)
	{
		var picName = this.PathIO.GetFileNameWithoutExtension(cell.Id.Raw);
		if (picName.Length > 0)
			picName = picName[0] + picName[1..].ToLowerInvariant();// Titlecase

		var loc = this.PointToClient(screenLocation);
		this.iconMagnifier.Location = new Point(loc.X, loc.Y + iconHeight);// Locate below icon

		this.iconMagnifier.pictureBox.Image = cell.Bitmap;
		this.iconMagnifier.scalingLabelFilename.Text = picName;
		this.iconMagnifier.scalingLabelSize.Text = string.Format("{0} x {1}", cell.Bitmap.Width, cell.Bitmap.Height);

		this.iconMagnifier.Visible = true;
	}

	#endregion

	#region Drag & Drop

	private void Grid_MouseDown(object? sender, MouseEventArgs e)
	{
		if (sender is not IconGrid grid || grid.CellAt(e.Location) is not { } cell)
			return;

		this.iconMagnifier.Visible = false;

		// Auto assign on Right Click
		if (e.Button == MouseButtons.Right)
		{
			CherryPickIconSimpleMode(cell.Info);

			this.scalingRadioButtonCustomIcon.Checked = true;
			this.ShowSelection();

			UpdateTextBoxDebug(this.SelectedOff);

			return;
		}

		if (e.Button != MouseButtons.Left)
			return;

		// The dragged picture is the mouse cursor: Windows moves it, nothing is repainted.
		this.DraggedCell = cell;
		float scale = (float)IconGrid.IconSize / Math.Max(cell.Bitmap.Width, cell.Bitmap.Height);
		this.DragCursor = ItemDragCursor.Create(cell.Bitmap, scale);
		if (this.DragCursor is not null)
			Cursor.Current = grid.Cursor = this.DragCursor.Cursor;
	}

	private void Grid_MouseMove(object? sender, MouseEventArgs e)
	{
		// The grid has the mouse capture during the drag, so the cursor is not set back by the controls below it.
		if (this.DragCursor is not null)
			Cursor.Current = this.DragCursor.Cursor;
	}

	private void Grid_MouseUp(object? sender, MouseEventArgs e)
	{
		var cell = this.DraggedCell;
		this.EndDrag(sender as IconGrid);
		if (cell is null || e.Button != MouseButtons.Left)
			return;

		var found = flowLayoutPanelPicturePickup.GetChildAtPoint(
			flowLayoutPanelPicturePickup.PointToClient(Cursor.Position)
			, GetChildAtPointSkip.Invisible | GetChildAtPointSkip.Transparent
		);

		bool dropped = false;
		if (this.radioButtonPictureDetailed.Checked)
		{
			// The placeholder takes the dropped picture itself.
			if (found == this.groupBoxOnBitmap)
			{
				this.SelectedOn = cell;
				dropped = true;
			}
			if (found == this.groupBoxOffBitmap)
			{
				this.SelectedOff = cell;
				dropped = true;
			}
			if (found == this.groupBoxOverBitmap)
			{
				this.SelectedOver = cell;
				dropped = true;
			}
		}

		if (this.radioButtonPictureSimple.Checked && found == this.groupBoxSimpleImage)
		{
			CherryPickIconSimpleMode(cell.Info);
			dropped = true;
		}

		if (dropped)
		{
			this.scalingRadioButtonCustomIcon.Checked = true;
			this.ShowSelection();
			UpdateTextBoxDebug(cell);
		}
	}

	private void EndDrag(IconGrid? grid)
	{
		this.DraggedCell = null;
		if (grid is not null)
			grid.Cursor = Cursors.Default;
		Cursor.Current = Cursors.Default;
		this.DragCursor?.Dispose();
		this.DragCursor = null;
	}

	#endregion

	[Conditional("DEBUG")]
	private void UpdateTextBoxDebug(Cell? cell)
	{
		if (cell is null) return;
		this.scalingTextBoxDebug.Text = cell.Id.ToString() + $" - ({cell.Bitmap.Size.Width}x{cell.Bitmap.Size.Height})";
	}

	private void radioButtonPictureSimple_CheckedChanged(object? sender, EventArgs e)
	{ TogglePicturePickup(); DisplayPictures(); }

	private void radioButtonPictureDetailed_CheckedChanged(object? sender, EventArgs e)
	{ TogglePicturePickup(); DisplayPictures(); }

	private void applyButton_Click(object? sender, EventArgs e)
	{
		var mode = BagButtonIconInfo.DisplayModeOf(this.scalingRadioButtonDefaultIcon.Checked
			, this.scalingCheckBoxNumber.Checked, this.scalingCheckBoxLabel.Checked);
		var info = BagButtonIconInfo.Create(mode, this.scalingTextBoxLabel.Text
			, this.SelectedOn?.Id, this.SelectedOff?.Id, this.SelectedOver?.Id);

		if (!info.Validate())
		{
			this.Services.MessageBoxes.Inform(Resources.BagButtonSettingsValidationMessage, Resources.BagButtonSettingsValidationCaption, MessageBoxIcon.Error);

			this.DialogResult = DialogResult.None;
			return;
		}

		if (mode == BagButtonDisplayMode.Default)
		{
			// Reset to default settings
			this._CurrentBagButton.OnBitmap = Images.inventorybagup01;
			this._CurrentBagButton.OffBitmap = Images.inventorybagdown01;
			this._CurrentBagButton.OverBitmap = Images.inventorybagover01;

			this._CurrentBagButton.Sack?.ChangeBagButtonIconInfo(null);
		}
		else
		{
			this._CurrentBagButton.Sack?.ChangeBagButtonIconInfo(info);
			this._CurrentBagButton.ApplyIconInfo(this._CurrentBagButton.Sack);
		}

		this.DialogResult = DialogResult.OK;
		this.Close();
	}

	private void cancelButton_Click(object? sender, EventArgs e)
	{
		this.DialogResult = DialogResult.Cancel;
		this.Close();
	}

	private void checkBoxLabel_CheckedChanged(object? sender, EventArgs e)
	{
		this.scalingTextBoxLabel.Enabled = this.scalingCheckBoxLabel.Checked;

		if (!this.scalingTextBoxLabel.Enabled) this.scalingTextBoxLabel.Text = string.Empty;
	}

	/// <summary>Room for the custom border and its title bar at 96 DPI, around the layout table that fills the form.</summary>
	private static readonly Padding BorderPadding = new(23, 32, 22, 20);

	/// <summary>
	/// Sets the border room for the current DPI. Windows Forms does not scale the padding of the form on a DPI change, and
	/// anchoring the table instead lost a few pixels at each change while the dialog was hidden: computed from the
	/// design value each time, it cannot drift.
	/// </summary>
	private void ApplyBorderPadding()
	{
		float factor = this.DeviceDpi / 96F;
		int Scaled(int value) => (int)Math.Round(value * factor);
		this.Padding = new Padding(Scaled(BorderPadding.Left), Scaled(BorderPadding.Top), Scaled(BorderPadding.Right), Scaled(BorderPadding.Bottom));
	}

	protected override void OnDpiChanged(DpiChangedEventArgs e)
	{
		base.OnDpiChanged(e);
		this.ApplyBorderPadding();
	}

	/// <summary>
	/// Gives the apply and cancel buttons one width that holds the longer of their translated texts ("Appliquer" did
	/// not fit the 80 pixels designed for "Apply"). Not AutoSize: ScalingButton fits its font to its width, and with
	/// AutoSize the two chase each other. The bottom table places them.
	/// </summary>
	private void FitExitButtons()
	{
		// The margin ScalingButton keeps around its text.
		const int textMargin = 16;
		int width = new[] { this.applyButton, this.cancelButton }
			.Max(b => Math.Max(b.Width, TextRenderer.MeasureText(b.Text, b.Font).Width + textMargin));
		this.applyButton.Width = this.cancelButton.Width = width;
	}

	private void scalingRadioButtonDefaultIcon_CheckedChanged(object? sender, EventArgs e)
	{
		if (this.scalingRadioButtonDefaultIcon.Checked)
		{
			this.scalingCheckBoxLabel.Checked =
				this.scalingCheckBoxNumber.Checked = false;
		}

		this.SyncOptionsEnabled();
		this.ShowSelection();
	}

	/// <summary>
	/// The default icon always shows the bag number and never a label: Number and Label only apply to a custom icon,
	/// and the label box only when Label is ticked (else its text was dropped on Apply).
	/// </summary>
	private void SyncOptionsEnabled()
	{
		bool custom = !this.scalingRadioButtonDefaultIcon.Checked;
		this.scalingCheckBoxNumber.Enabled = custom;
		this.scalingCheckBoxLabel.Enabled = custom;
		this.scalingTextBoxLabel.Enabled = custom && this.scalingCheckBoxLabel.Checked;
	}
}
