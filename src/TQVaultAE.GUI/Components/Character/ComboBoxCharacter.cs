using System.ComponentModel;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Components.Rendering;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Forms;
using TQVaultAE.Presentation.Win32.Contracts;
using TQVaultAE.Presentation;
using TQVaultAE.Config;

namespace TQVaultAE.GUI.Components.Character;

public partial class ComboBoxCharacter : UserControl, IFontRebasing, IZoomFit
{
	internal const string TAGKEY = "TagItem_";

	#region Prop & Fields

	// Set by Init: the designer creates the control without services.
	private IUIService UIService = null!;
	private IFontService FontService = null!;
	private ITranslationService TranslationService = null!;
	private ITagService TagService = null!;
	private IPlayerService PlayerService = null!;
	private UserSettings USettings = null!;
	private Bitmap HUDCHARACTERBUTTONUP01 = null!;
	private Bitmap HUDCHARACTERBUTTONOVER01 = null!;
	private Bitmap HUDCHARACTERBUTTONDOWN01 = null!;
	private Bitmap HUDMENUSKILLBUTTONUP01 = null!;
	private Bitmap HUDMENUSKILLBUTTONDOWN01 = null!;
	private Bitmap HUDMENUSKILLBUTTONOVER01 = null!;

	private int _MaxVisibleItems = 5;
	public int MaxVisibleItems
	{
		get => _MaxVisibleItems;
		set
		{
			_MaxVisibleItems = value;
			ApplyHeight();
		}
	}

	private VaultForm? Form;
	private Point? DropDownLoc;
	private ComboBoxCharacterDropDown DropDown = null!;

	/// <summary>
	/// a collection of <see cref="PlayerSave"/> or <see cref="string"/>
	/// </summary>
	public readonly BindingList<object> Items = [];

	private object? _SelectedItem = null;
	private int _SelectedIndex = -1;
	private Action DuplicateCharacterAction = null!;
	private Action<PlayerSave> ModLinkAction = null!;
	private Action<PlayerSave> ReadOnlyAction = null!;

	/// <summary>
	/// Opens the mod link dialog of the selected custom map character.
	/// </summary>
	private readonly ToolStripMenuItem modLinkToolStripMenuItem = new() { Name = "modLinkToolStripMenuItem" };

	/// <summary>
	/// Shows and toggles the read-only state of the selected character (the read-only attribute of its save files).
	/// </summary>
	private readonly ToolStripMenuItem readOnlyToolStripMenuItem = new() { Name = "readOnlyToolStripMenuItem" };
	private bool _DisableMenuTriggers;

	/// <summary>
	/// a <see cref="PlayerSave"/> or a <see cref="string"/>
	/// </summary>
	public object? SelectedItem
	{
		get => _SelectedItem;
		set
		{
			if (value is not null && Items.Contains(value))
			{
				_SelectedItem = value;
				RefreshContent();
				RaiseSelectedItemsEvents();
			}
		}
	}

	public int SelectedIndex
	{
		get => _SelectedIndex;
		set
		{
			if (_SelectedIndex == value) return;
			if (Items.Count > 0 && value >= 0 && value < Items.Count)
			{
				_SelectedIndex = value;
				SelectedItem = this.Items[_SelectedIndex];
			}
		}
	}

	#endregion

	#region Events

	public event EventHandler? SelectedIndexChanged;

	public event EventHandler? SelectedItemChanged;

	private void RaiseSelectedItemsEvents()
	{
		var selected = SelectedItem;
		for (int i = 0; i < Items.Count; i++)
		{
			var item = Items[i];
			if (item == selected)
			{
				_SelectedIndex = i;
				SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
				SelectedItemChanged?.Invoke(this, EventArgs.Empty);
				break;
			}
		}
	}

	#endregion

	public ComboBoxCharacter()
	{
		InitializeComponent();
		Items.ListChanged += Saves_ListChanged;

		toolTip.SetToolTip(this.pictureBoxChar, Resources.MainFormLabel1);
	}

	/// <summary>
	/// Row height: the bitmaps, or the name box when taller, so the name box is not cut and looks like the vault combo box
	/// next to it (its text has the same font). The tools button fills it, keeping the bitmap proportions.
	/// </summary>
	private void ApplyRowHeight()
	{
		if (HUDCHARACTERBUTTONUP01 is null || HUDMENUSKILLBUTTONUP01 is null)
			return;

		var nameHeight = this.scalingLabelCharName.PreferredHeight + this.scalingLabelCharName.Margin.Vertical
			+ (this.bufferedFlowLayoutPanelContent.Height - this.bufferedFlowLayoutPanelContent.ClientSize.Height);
		var rowHeight = Math.Max(nameHeight, Math.Max(HUDCHARACTERBUTTONUP01.Height, HUDMENUSKILLBUTTONUP01.Height));
		this.bufferedTableLayoutPanelSkeleton.Size = new Size(this.bufferedTableLayoutPanelSkeleton.Width, rowHeight);
		// A fixed row: the picture docked in it takes this height, instead of growing the row with each DPI change.
		this.bufferedTableLayoutPanelSkeleton.RowStyles[0] = new RowStyle(SizeType.Absolute, rowHeight);

		var toolsWidth = (int)Math.Round(HUDMENUSKILLBUTTONUP01.Width * (float)rowHeight / HUDMENUSKILLBUTTONUP01.Height);
		this.scalingButtonTools.Size = new Size(toolsWidth, rowHeight);
		this.bufferedTableLayoutPanelSkeleton.ColumnStyles[2].Width = toolsWidth;
	}

	/// <summary>On a monitor of another DPI the name font changes: the row height follows it.</summary>
	protected override void OnDpiChangedAfterParent(EventArgs e)
	{
		base.OnDpiChangedAfterParent(e);
		this.ApplyRowHeight();
	}

	/// <summary>Nothing to give back: the row is set whole by <see cref="ApplyRowHeight"/>.</summary>
	void IZoomFit.UndoFit()
	{
	}

	/// <summary>The zoom changed the name font: the row height follows it, as after a DPI change.</summary>
	void IZoomFit.Fit() => this.ApplyRowHeight();

	private void ApplyHeight()
	{
		if (this.DropDown is not null)
			this.DropDown.Height = this.MaxVisibleItems * ComboBoxCharacterItem.BASE_HEIGHT;
	}

	internal int FindString(string search)
	{
		if (string.IsNullOrEmpty(search)) return -1;

		for (int i = 0; i < Items.Count; i++)
		{
			var item = Items[i];
			if ((item.ToString() ?? string.Empty).ContainsIgnoreCase(search))
				return i;
		}
		return -1;
	}

	private void AdjustLocation()
	{
		if (this.Form is null)
			return;

		this.DropDownLoc = this.Form.PointToClient(this.PointToScreen(Point.Empty));
		this.DropDownLoc = new Point(this.DropDownLoc.Value.X, this.DropDownLoc.Value.Y + Height);
		this.DropDown.Location = this.DropDownLoc.Value;
	}

	public void Init(
		IUIService uIService
		, IFontService fontService
		, ITranslationService translationService
		, ITagService tagService
		, IPlayerService playerService
		, UserSettings userSettings
		, Action duplicateCharacterAction
		, Action<PlayerSave> modLinkAction
		, Action<PlayerSave> readOnlyAction
	)
	{
		this.UIService = uIService;
		this.FontService = fontService;
		this.TranslationService = translationService;
		this.TagService = tagService;
		this.PlayerService = playerService;
		this.USettings = userSettings;
		this.Form = this.FindForm() as VaultForm;
		if (this.Form is not null)
			this.Form.GlobalMouseButtonLeft += Form_GlobalMouseButtonLeft;
		this.DuplicateCharacterAction = duplicateCharacterAction;
		this.ModLinkAction = modLinkAction;
		this.ReadOnlyAction = readOnlyAction;
		this.modLinkToolStripMenuItem.Click += this.ModLinkToolStripMenuItem_Click;
		this.contextMenuStrip.Items.Insert(this.contextMenuStrip.Items.IndexOf(this.duplicateSeparatorToolStripMenuItem), this.modLinkToolStripMenuItem);
		this.readOnlyToolStripMenuItem.Click += this.ReadOnlyToolStripMenuItem_Click;
		// Next to Archived: both are states of the character's files, shown by a check mark.
		this.contextMenuStrip.Items.Insert(this.contextMenuStrip.Items.IndexOf(this.archiveToolStripMenuItem) + 1, this.readOnlyToolStripMenuItem);

		if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
		{
			HUDCHARACTERBUTTONUP01 = UIService.LoadBitmap(@"INGAMEUI\HUDCHARACTERBUTTONUP01.TEX");
			HUDCHARACTERBUTTONOVER01 = UIService.LoadBitmap(@"INGAMEUI\HUDCHARACTERBUTTONOVER01.TEX");
			HUDCHARACTERBUTTONDOWN01 = UIService.LoadBitmap(@"INGAMEUI\HUDCHARACTERBUTTONDOWN01.TEX");

			HUDMENUSKILLBUTTONUP01 = UIService.LoadBitmap(@"INGAMEUI\HUDMENUSKILLBUTTONUP01.TEX");
			HUDMENUSKILLBUTTONDOWN01 = UIService.LoadBitmap(@"INGAMEUI\HUDMENUSKILLBUTTONDOWN01.TEX");
			HUDMENUSKILLBUTTONOVER01 = UIService.LoadBitmap(@"INGAMEUI\HUDMENUSKILLBUTTONOVER01.TEX");

			this.pictureBoxChar.Image = HUDCHARACTERBUTTONUP01;
			this.pictureBoxChar.Size = HUDCHARACTERBUTTONUP01.Size;
			this.pictureBoxChar.Dock = DockStyle.Fill;
			this.pictureBoxChar.SizeMode = PictureBoxSizeMode.Zoom;


			this.scalingButtonTools.Image = HUDMENUSKILLBUTTONUP01;
			this.scalingButtonTools.Size = HUDMENUSKILLBUTTONUP01.Size;
			this.scalingButtonTools.UpBitmap = HUDMENUSKILLBUTTONUP01;
			this.scalingButtonTools.DownBitmap = HUDMENUSKILLBUTTONDOWN01;
			this.scalingButtonTools.OverBitmap = HUDMENUSKILLBUTTONOVER01;

			this.duplicateToolStripMenuItem.Text = Resources.DuplicateCharacter_ButtonText;
			this.modLinkToolStripMenuItem.Text = Resources.ModLinkMenu;
			this.readOnlyToolStripMenuItem.Text = Resources.CharacterReadOnly;
			this.archiveToolStripMenuItem.Text = Resources.GlobalArchived;
			this.tagsToolStripMenuItem.Text = Resources.GlobalTags;
			this.addTagToolStripMenuItem.Text = Resources.TagsAdd;
			this.deleteToolStripMenuItem.Text = TranslationService.TranslateXTag("tagMenuButton03");
			this.renameToolStripMenuItem.Text = Resources.GlobalRename;
			this.archiveAllToolStripMenuItemGlobal.Text =
			this.archiveAllToolStripMenuItem.Text = Resources.GlobalArchiveAll;
			this.unarchiveAllToolStripMenuItemGlobal.Text =
			this.unarchiveAllToolStripMenuItem.Text = Resources.GlobalUnArchiveAll;
			this.renameToolStripMenuItem.Text = Resources.GlobalRename;
			this.colorToolStripMenuItem.Text = TranslationService.TranslateXTag("x3tagGraphicOption02");

			this.scalingLabelCharName.Font = FontService.GetFontLight(13F, UIService.FontZoom);
			this.ApplyRowHeight();

			this.archiveAllToolStripMenuItemGlobal.Font =
			this.unarchiveAllToolStripMenuItemGlobal.Font =
			this.archiveAllToolStripMenuItem.Font =
			this.unarchiveAllToolStripMenuItem.Font =
			this.duplicateToolStripMenuItem.Font =
			this.modLinkToolStripMenuItem.Font =
			this.readOnlyToolStripMenuItem.Font =
			this.archiveToolStripMenuItem.Font =
			this.tagsToolStripMenuItem.Font =
			this.addTagToolStripMenuItem.Font =
			this.toolStripTextBoxAdd.Font =
			this.toolStripTextBoxRename.Font =
			this.deleteToolStripMenuItem.Font =
			this.renameToolStripMenuItem.Font =
			this.colorToolStripMenuItem.Font =
			this.contextMenuStrip.Font = FontService.GetFontLight(this.contextMenuStrip.Font.Size);

			this.archiveAllToolStripMenuItemGlobal.BackColor =
			this.unarchiveAllToolStripMenuItemGlobal.BackColor =
			this.archiveAllToolStripMenuItem.BackColor =
			this.unarchiveAllToolStripMenuItem.BackColor =
			this.duplicateToolStripMenuItem.BackColor =
			this.modLinkToolStripMenuItem.BackColor =
			this.readOnlyToolStripMenuItem.BackColor =
			this.archiveToolStripMenuItem.BackColor =
			this.tagsToolStripMenuItem.BackColor =
			this.addTagToolStripMenuItem.BackColor =
			this.toolStripTextBoxAdd.BackColor =
			this.toolStripTextBoxRename.BackColor =
			this.deleteToolStripMenuItem.BackColor =
			this.renameToolStripMenuItem.BackColor =
			this.colorToolStripMenuItem.BackColor =
			this.contextMenuStrip.BackColor = Theme.Background;

			this.archiveAllToolStripMenuItemGlobal.ForeColor =
			this.unarchiveAllToolStripMenuItemGlobal.ForeColor =
			this.archiveAllToolStripMenuItem.ForeColor =
			this.unarchiveAllToolStripMenuItem.ForeColor =
			this.duplicateToolStripMenuItem.ForeColor =
			this.modLinkToolStripMenuItem.ForeColor =
			this.readOnlyToolStripMenuItem.ForeColor =
			this.archiveToolStripMenuItem.ForeColor =
			this.tagsToolStripMenuItem.ForeColor =
			this.addTagToolStripMenuItem.ForeColor =
			this.toolStripTextBoxAdd.ForeColor =
			this.toolStripTextBoxRename.ForeColor =
			this.deleteToolStripMenuItem.ForeColor =
			this.renameToolStripMenuItem.ForeColor =
			this.colorToolStripMenuItem.ForeColor =
			this.contextMenuStrip.ForeColor = Theme.MenuText;

			CustomProfessionalRenderer.Apply(this.contextMenuStrip);
			//this.contextMenuStrip.Opacity = 0.8D;

			this.DropDown = new ComboBoxCharacterDropDown()
			{
				AutoScroll = true,
				BackColor = Color.White,
				ForeColor = Color.Black,
				Margin = new Padding(0),
				Name = "comboBoxCharacterDropDown",
				Size = new Size(500, 150),
				Visible = false,
				ComboBox = this,
			};
			ApplyHeight();

			this.Form?.Controls.Add(this.DropDown);

			RefreshMenuTagList();

		}
	}

	#region Refresh

	private void RefreshContent()
	{
		this._DisableMenuTriggers = true;

		this.bufferedFlowLayoutPanelContent.BackColor = Color.White;
		this.modLinkToolStripMenuItem.Visible = false;
		this.readOnlyToolStripMenuItem.Visible = false;

		if (this.SelectedItem is PlayerSave ps)
		{
			this.scalingLabelCharName.Text = ps.ToString();

			// Show duplicate feature ?
			if (this.USettings.AllowCharacterEdit)
			{
				this.duplicateToolStripMenuItem.Visible =
				this.duplicateSeparatorToolStripMenuItem.Visible = true;
			}
			else
			{
				this.duplicateToolStripMenuItem.Visible =
				this.duplicateSeparatorToolStripMenuItem.Visible = false;
			}

			// Mod link, for custom map characters only
			this.modLinkToolStripMenuItem.Visible = ps.IsCustom;

			// Archiving toggle
			this.archiveToolStripMenuItem.Checked = ps.IsArchived;

			// Read-only toggle
			this.readOnlyToolStripMenuItem.Visible = true;
			this.readOnlyToolStripMenuItem.Checked = ps.IsReadOnly;

			// Status color: icy blue for a read-only character, silver for an archived one (read-only first: it is the
			// state that refuses changes).
			if (ps.IsReadOnly)
				this.bufferedFlowLayoutPanelContent.BackColor = ComboBoxCharacterItem.ReadOnlyBackColor;
			else if (ps.IsArchived)
				this.bufferedFlowLayoutPanelContent.BackColor = ComboBoxCharacterItem.ArchivedBackColor;

			// Tags Toggle
			foreach (ToolStripItem tagItem in this.tagsToolStripMenuItem.DropDownItems)
			{
				if (tagItem is ToolStripSeparator)
					continue;

				if (tagItem is ToolStripMenuItem mi && (tagItem.Name?.StartsWith(TAGKEY, StringComparison.Ordinal) ?? false))
				{
					mi.Checked = false;
					foreach (var tag in ps.Tags)
					{
						if (mi.Text == tag.Key)
						{
							mi.Checked = true;
							break;
						}
					}
				}
			}
		}
		else if (this.SelectedItem is string str)
			this.scalingLabelCharName.Text = str;
		else
			this.scalingLabelCharName.Text = string.Empty;


		this._DisableMenuTriggers = false;
	}

	public void RefreshItems()
	{
		foreach (var uctr in this.DropDown.Items)
			uctr.RefreshContent();
	}

	/// <summary>
	/// Reloads the tags of every listed character after a tag was renamed, recolored or deleted, then refreshes the list.
	/// </summary>
	private void ReloadTagsAndRefreshItems()
	{
		foreach (var ps in this.Items.OfType<PlayerSave>())
			this.TagService.LoadTags(ps);

		RefreshItems();
	}

	internal void RefreshItem(PlayerSave selectedSave)
	{
		var item = this.DropDown.Items.First(i => i.Item == selectedSave);
		item.RefreshContent();

		// The text shown for the selection too (its mod label, its class).
		if (this.SelectedItem == selectedSave)
			this.RefreshContent();
	}

	private void RefreshMenuTagList()
	{
		// Selective clean
		this.tagsToolStripMenuItem.DropDownItems.OfType<ToolStripItem>()
			.Where(item => !(item == this.addTagToolStripMenuItem || item == this.AddTagtoolStripSeparator))// i Keep that
			.ToList().ForEach(i => this.tagsToolStripMenuItem.DropDownItems.Remove(i));

		// Add existing tag list
		int idx = 0;
		foreach (var tag in this.TagService.Tags)
		{
			var tagItem = new ToolStripMenuItem
			{
				Name = TAGKEY + idx,
				Text = tag.Key,
				BackColor = tag.Value,
				Font = this.contextMenuStrip.Font,
				Checked = false,
				CheckOnClick = true,
			};
			tagItem.CheckedChanged += TagItem_CheckedChanged;
			this.tagsToolStripMenuItem.DropDownItems.Add(tagItem);

			var tagColorItem = new ToolStripMenuItem
			{
				Name = @"tagColorItem" + idx,
				Text = this.colorToolStripMenuItem.Text,
				Font = this.colorToolStripMenuItem.Font,
				BackColor = this.colorToolStripMenuItem.BackColor,
				ForeColor = this.colorToolStripMenuItem.ForeColor,
			};
			tagColorItem.Click += TagColorItem_Click;
			tagItem.DropDownItems.Add(tagColorItem);

			var tagDeleteItem = new ToolStripMenuItem
			{
				Name = @"tagDeleteItem" + idx,
				Text = this.deleteToolStripMenuItem.Text,
				Font = this.deleteToolStripMenuItem.Font,
				BackColor = this.deleteToolStripMenuItem.BackColor,
				ForeColor = this.deleteToolStripMenuItem.ForeColor,
			};
			tagDeleteItem.Click += TagDeleteItem_Click;
			tagItem.DropDownItems.Add(tagDeleteItem);

			var tagRenameItem = new ToolStripMenuItem
			{
				Name = @"tagRenameItem" + idx,
				Text = this.renameToolStripMenuItem.Text,
				Font = this.renameToolStripMenuItem.Font,
				BackColor = this.renameToolStripMenuItem.BackColor,
				ForeColor = this.renameToolStripMenuItem.ForeColor,
			};
			tagItem.DropDownItems.Add(tagRenameItem);

			var tagRenameTextBoxItem = new ToolStripTextBox
			{
				Name = @"tagRenameTextBoxItem" + idx,
				Font = this.toolStripTextBoxRename.Font,
				BackColor = this.toolStripTextBoxRename.BackColor,
				ForeColor = this.toolStripTextBoxRename.ForeColor,
			};
			tagRenameTextBoxItem.KeyPress += toolStripTextBox_KeyPress;
			tagRenameTextBoxItem.KeyDown += TagRenameTextBoxItem_KeyDown;
			tagRenameItem.DropDownItems.Add(tagRenameTextBoxItem);

			var tagArchiveAllItem = new ToolStripMenuItem
			{
				Name = @"tagArchiveAllItem" + idx,
				Text = this.archiveAllToolStripMenuItem.Text,
				Font = this.archiveAllToolStripMenuItem.Font,
				BackColor = this.archiveAllToolStripMenuItem.BackColor,
				ForeColor = this.archiveAllToolStripMenuItem.ForeColor,
			};
			tagArchiveAllItem.Click += TagArchiveAllItem_Click;
			tagItem.DropDownItems.Add(tagArchiveAllItem);

			var tagUnArchiveAllItem = new ToolStripMenuItem
			{
				Name = @"tagUnArchiveAllItem" + idx,
				Text = this.unarchiveAllToolStripMenuItem.Text,
				Font = this.unarchiveAllToolStripMenuItem.Font,
				BackColor = this.unarchiveAllToolStripMenuItem.BackColor,
				ForeColor = this.unarchiveAllToolStripMenuItem.ForeColor,
			};
			tagUnArchiveAllItem.Click += TagUnArchiveAllItem_Click;
			tagItem.DropDownItems.Add(tagUnArchiveAllItem);

			idx++;
		}
	}


	#endregion

	#region DropDown

	private void OpenDropDown_Click(object? sender, EventArgs e)
	{
		if (this.DropDownLoc is null)
			AdjustLocation();

		if (this.DropDown.Visible)
		{
			DropDownClose();
			return;
		}

		DropDownOpen();
	}

	private void DropDownOpen()
	{
		this.DropDown.SendToBack();
		this.DropDown.Visible = true;
		// Placed and sized once shown: the first show after a move to a monitor of another DPI scales the bounds the hidden
		// list got for the previous DPI.
		this.AdjustLocation();
		this.DropDown.FitWidth();
		this.DropDown.BringToFront();

		// Scroll to selected control
		if (this.SelectedItem is not null)
		{
			var cbci = this.DropDown.Items.FirstOrDefault(cbci => cbci.Item == this.SelectedItem);
			if (cbci is not null)
				this.DropDown.ScrollControlIntoView(cbci);
		}
	}

	private void DropDownClose()
	{
		this.DropDown.Visible = false;
	}

	private void Saves_ListChanged(object? sender, ListChangedEventArgs e)
	{
		switch (e.ListChangedType)
		{
			case ListChangedType.ItemAdded:
				var save = this.Items[e.NewIndex];

				var item = new ComboBoxCharacterItem();
				item.Init(save, TranslationService, FontService, PlayerService, TagService);
				item.SelectedItemChanged += Item_SelectedItemChanged;
				this.DropDown.Items.Add(item);
				item.RefreshContent();
				break;
		}
	}

	private void Item_SelectedItemChanged(object? sender, EventArgs e)
	{
		var selected = (ComboBoxCharacterItem)sender!;

		if (this.SelectedItem != selected.Item)
		{
			this.SelectedItem = selected.Item;
			DropDownClose();
		}
	}

	private void Form_GlobalMouseButtonLeft(object? sender, Point point)
	{
		// Close dropdown if left click outside it's surface
		var mp = MousePosition;
		var dropdownrectangle = DropDown.RectangleToScreen(DropDown.ClientRectangle);
		if (DropDown.Visible && !dropdownrectangle.Contains(mp))
			DropDownClose();
	}

	#endregion

	/// <summary>
	/// The base font changed: the tools menu it shows itself, outside the controls, takes it.
	/// </summary>
	void IFontRebasing.RebaseFonts(IFontService fontService)
		=> FontRebaser.RebaseMenu(this.contextMenuStrip, fontService);

	private void ScalingButtonTools_Click(object? sender, EventArgs e)
	{
		this.contextMenuStrip.Show(scalingButtonTools, new Point(0, scalingButtonTools.Height));
	}

	private void TagColorItem_Click(object? sender, EventArgs e)
	{
		if (this._DisableMenuTriggers)
			return;

		var colorItem = sender as ToolStripMenuItem;
		if (colorItem is not null)
		{
			var tagName = colorItem.OwnerItem?.Text ?? string.Empty;
			var result = this.colorDialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				var col = this.colorDialog.Color;

				this.TagService.UpdateTag(tagName, tagName, col.R, col.G, col.B);

				this.RefreshMenuTagList();
				this.ReloadTagsAndRefreshItems();
				this.RefreshContent();
			}
		}
	}

	private void TagItem_CheckedChanged(object? sender, EventArgs e)
	{
		if (this._DisableMenuTriggers)
			return;

		var tagItem = sender as ToolStripMenuItem;
		if (tagItem is not null)
		{
			var ps = this.SelectedItem as PlayerSave;
			if (ps is not null)
			{
				var tagName = tagItem.Text ?? string.Empty;
				if (tagItem.Checked)
					this.TagService.AssignTag(ps, tagName);
				else
					this.TagService.UnassignTag(ps, tagName);

				RefreshItem(ps);
				RefreshContent();
			}
		}
	}

	private void duplicateToolStripMenuItem_Click(object? sender, EventArgs e)
	{
		if (this._DisableMenuTriggers)
			return;

		this.DuplicateCharacterAction();
	}

	private void ModLinkToolStripMenuItem_Click(object? sender, EventArgs e)
	{
		if (this._DisableMenuTriggers || this.SelectedItem is not PlayerSave ps)
			return;

		this.ModLinkAction(ps);
	}

	/// <summary>
	/// Makes the selected character read-only, or writable again (the main window changes the files and tells the result).
	/// </summary>
	private void ReadOnlyToolStripMenuItem_Click(object? sender, EventArgs e)
	{
		if (this._DisableMenuTriggers || this.SelectedItem is not PlayerSave ps)
			return;

		this.ReadOnlyAction(ps);
	}

	private void TagRenameTextBoxItem_KeyDown(object? sender, KeyEventArgs e)
	{
		var textBox = (ToolStripTextBox)sender!;
		// The rename box sits in the menu of its tag.
		var oldTagName = textBox.OwnerItem?.OwnerItem?.Text ?? string.Empty;

		if (e.KeyCode == Keys.Escape)
		{
			ClearInputAndCollapse(textBox);
		}
		else if (e.KeyCode == Keys.Enter)
		{
			var newTagName = textBox.Text;
			if (!this.AcceptTagName(e, newTagName, oldTagName))
				return;

			// Update tag
			var col = this.TagService.Tags[oldTagName];
			this.TagService.UpdateTag(oldTagName, newTagName, col.R, col.G, col.B);

			this.ReloadTagsAndRefreshItems();

			ClearInputAndCollapse(textBox);
			RefreshMenuTagList();
			RefreshContent();
		}

	}

	/// <summary>
	/// Checks a tag name typed in a menu text box. When it is refused, cancels the key and tells why (nothing for an empty name).
	/// </summary>
	/// <param name="e">The Enter key event, cancelled when the name is refused.</param>
	/// <param name="newName">The name typed.</param>
	/// <param name="currentName">The tag being renamed, or null for a new tag.</param>
	/// <returns>Whether the name can be used.</returns>
	private bool AcceptTagName(KeyEventArgs e, string newName, string? currentName)
	{
		var error = this.TagService.ValidateTagName(newName, currentName);
		if (error == TagNameError.None)
			return true;

		e.Handled = true;
		string? message = error switch
		{
			TagNameError.AlreadyExists => Resources.TagExists,
			TagNameError.SameAsCurrent => Resources.TagMustBeDifferent,
			_ => null,
		};
		if (message is not null)
			this.UIService.ShowError(message, Buttons: ShowMessageButtons.OK);

		return false;
	}

	private void TagDeleteItem_Click(object? sender, EventArgs e)
	{
		var deleteMenuItem = (ToolStripMenuItem)sender!;
		var tagName = deleteMenuItem.OwnerItem?.Text ?? string.Empty;

		if (UIService.ShowWarning(Resources.GlobalConfirm).IsOK)
		{
			this.TagService.DeleteTag(tagName);

			this.ReloadTagsAndRefreshItems();
			RefreshMenuTagList();
			RefreshContent();
		}
	}

	private void TagUnArchiveAllItem_Click(object? sender, EventArgs e)
	{
		var unArchiveAllMenuItem = (ToolStripMenuItem)sender!;
		this.SetArchived(archive: false, unArchiveAllMenuItem.OwnerItem?.Text ?? string.Empty);
	}

	private void TagArchiveAllItem_Click(object? sender, EventArgs e)
	{
		var archiveAllMenuItem = (ToolStripMenuItem)sender!;
		this.SetArchived(archive: true, archiveAllMenuItem.OwnerItem?.Text ?? string.Empty);
	}

	/// <summary>
	/// Archives or unarchives the listed characters (only those having <paramref name="tag"/>, when given), then refreshes the list.
	/// </summary>
	private void SetArchived(bool archive, string? tag)
	{
		var result = this.PlayerService.SetArchived(this.Items.OfType<PlayerSave>(), archive, tag);
		if (!result.AnyChanged)
			return;

		RefreshItems();
		RefreshContent();
	}

	private void ToolStripTextBoxAdd_KeyDown(object? sender, KeyEventArgs e)
	{
		var textBox = (ToolStripTextBox)sender!;

		if (e.KeyCode == Keys.Escape)
		{
			ClearInputAndCollapse(textBox);
		}
		else if (e.KeyCode == Keys.Enter)
		{
			var txt = textBox.Text;
			if (!this.AcceptTagName(e, txt, null))
				return;

			// Add tag
			this.TagService.AddTag(txt);

			var ps = this.SelectedItem as PlayerSave;

			if (ps is not null)
			{
				this.TagService.AssignTag(ps, txt);
				RefreshItem(ps);
			}

			ClearInputAndCollapse(textBox);
			RefreshMenuTagList();
			RefreshContent();
		}
	}

	void ClearInputAndCollapse(ToolStripTextBox textbox)
	{
		textbox.Clear();
		(textbox.OwnerItem as ToolStripMenuItem)?.HideDropDown();
	}

	private void toolStripTextBox_KeyPress(object? sender, KeyPressEventArgs e)
	{
		if (this._DisableMenuTriggers)
			return;

		var texBox = (ToolStripTextBox)sender!;
		// Filter input ??
		/*
		if (!Regex.IsMatch(e.KeyChar.ToString(), @"[\w\. \-\[\]]"))
		{
			e.Handled = true;// Means cancel event
			return;
		}
		*/
	}

	private void archiveToolStripMenuItem_CheckedChanged(object? sender, EventArgs e)
	{
		if (this._DisableMenuTriggers)
			return;

		var ps = this.SelectedItem as PlayerSave;
		if (ps is not null)
		{
			// Archive it
			if (archiveToolStripMenuItem.Checked)
				this.PlayerService.Archive(ps);
			else
				this.PlayerService.Unarchive(ps);

			RefreshItem(ps);
			RefreshContent();
		}
	}

	private void archiveAllToolStripMenuItemGlobal_Click(object? sender, EventArgs e)
	{
		if (this._DisableMenuTriggers)
			return;

		this.SetArchived(archive: true, tag: null);
	}

	private void unarchiveAllToolStripMenuItemGlobal_Click(object? sender, EventArgs e)
	{
		if (this._DisableMenuTriggers)
			return;

		this.SetArchived(archive: false, tag: null);
	}
}

