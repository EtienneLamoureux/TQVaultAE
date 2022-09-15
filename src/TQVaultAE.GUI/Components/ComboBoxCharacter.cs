using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TQVaultAE.Domain.Contracts.Providers;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Presentation;
using static System.Net.Mime.MediaTypeNames;

namespace TQVaultAE.GUI.Components;

public partial class ComboBoxCharacter : UserControl
{
	internal const string TAGKEY = "TagItem_";

	#region Prop & Fields

	private IUIService UIService;
	private IFontService FontService;
	private ITranslationService TranslationService;
	private IDatabase Database;
	private IGamePathService GamePathService;
	private IGameFileService GameFileService;
	private ITagService TagService;
	private Bitmap HUDCHARACTERBUTTONUP01;
	private Bitmap HUDCHARACTERBUTTONOVER01;
	private Bitmap HUDCHARACTERBUTTONDOWN01;
	private Bitmap HUDMENUSKILLBUTTONUP01;
	private Bitmap HUDMENUSKILLBUTTONDOWN01;
	private Bitmap HUDMENUSKILLBUTTONOVER01;

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

	private VaultForm Form;
	private Point? DropDownLoc;
	private ComboBoxCharacterDropDown DropDown;

	/// <summary>
	/// a collection of <see cref="PlayerSave"/> or <see cref="string"/>
	/// </summary>
	public readonly BindingList<object> Items = new();

	private object _SelectedItem = null;
	private int _SelectedIndex = -1;
	private Action DuplicateCharacterAction;
	private bool _DisableMenuTriggers;

	/// <summary>
	/// a <see cref="PlayerSave"/> or a <see cref="string"/>
	/// </summary>
	public object SelectedItem
	{
		get => _SelectedItem;
		set
		{
			if (Items.Contains(value))
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
			if (Items.Count > 0 && value >= 0 && value < Items.Count)
			{
				_SelectedIndex = value;
				SelectedItem = this.Items[_SelectedIndex];
			}
		}
	}

	#endregion

	#region Events

	public event EventHandler SelectedIndexChanged;

	public event EventHandler SelectedItemChanged;

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
			if (item.ToString().ContainsIgnoreCase(search))
				return i;
		}
		return -1;
	}

	private void AdjustLocation()
	{
		this.DropDownLoc = this.Form.PointToClient(this.PointToScreen(Point.Empty));
		this.DropDownLoc = new Point(this.DropDownLoc.Value.X, this.DropDownLoc.Value.Y + Height);
		this.DropDown.Location = this.DropDownLoc.Value;
	}

	public void Init(
		IUIService uIService
		, IFontService fontService
		, ITranslationService translationService
		, IDatabase database
		, IGameFileService gameFileService
		, IGamePathService gamePathService
		, ITagService tagService
		, Action duplicateCharacterAction
	)
	{
		this.UIService = uIService;
		this.FontService = fontService;
		this.TranslationService = translationService;
		this.Database = database;
		this.GamePathService = gamePathService;
		this.GameFileService = gameFileService;
		this.TagService = tagService;
		this.Form = this.FindForm() as VaultForm;
		this.Form.GlobalMouseButtonLeft += Form_GlobalMouseButtonLeft;
		this.DuplicateCharacterAction = duplicateCharacterAction;

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

			this.scalingButtonTools.Image = HUDMENUSKILLBUTTONUP01;
			this.scalingButtonTools.Size = HUDMENUSKILLBUTTONUP01.Size;
			this.scalingButtonTools.UpBitmap = HUDMENUSKILLBUTTONUP01;
			this.scalingButtonTools.DownBitmap = HUDMENUSKILLBUTTONDOWN01;
			this.scalingButtonTools.OverBitmap = HUDMENUSKILLBUTTONOVER01;

			this.duplicateToolStripMenuItem.Text = Resources.DuplicateCharacter_ButtonText;
			this.archiveToolStripMenuItem.Text = Resources.GlobalArchived;
			this.tagsToolStripMenuItem.Text = Resources.GlobalTags;
			this.addTagToolStripMenuItem.Text = Resources.TagsAdd;
			this.deleteToolStripMenuItem.Text = TranslationService.TranslateXTag("tagMenuButton03");
			this.renameToolStripMenuItem.Text = Resources.GlobalRename;
			this.archiveAllToolStripMenuItem.Text = Resources.GlobalArchiveAll;
			this.unarchiveAllToolStripMenuItem.Text = Resources.GlobalUnArchiveAll;
			this.renameToolStripMenuItem.Text = Resources.GlobalRename;
			this.colorToolStripMenuItem.Text = TranslationService.TranslateXTag("x3tagGraphicOption02");

			this.scalingLabelCharName.Font = FontService.GetFontLight(13F, UIService.Scale);

			this.archiveAllToolStripMenuItem.Font =
			this.unarchiveAllToolStripMenuItem.Font =
			this.duplicateToolStripMenuItem.Font =
			this.archiveToolStripMenuItem.Font =
			this.tagsToolStripMenuItem.Font =
			this.addTagToolStripMenuItem.Font =
			this.toolStripTextBoxAdd.Font =
			this.toolStripTextBoxRename.Font =
			this.deleteToolStripMenuItem.Font =
			this.renameToolStripMenuItem.Font =
			this.colorToolStripMenuItem.Font =
			this.contextMenuStrip.Font = FontService.GetFontLight(this.contextMenuStrip.Font.Size);

			this.archiveAllToolStripMenuItem.BackColor =
			this.unarchiveAllToolStripMenuItem.BackColor =
			this.duplicateToolStripMenuItem.BackColor =
			this.archiveToolStripMenuItem.BackColor =
			this.tagsToolStripMenuItem.BackColor =
			this.addTagToolStripMenuItem.BackColor =
			this.toolStripTextBoxAdd.BackColor =
			this.toolStripTextBoxRename.BackColor =
			this.deleteToolStripMenuItem.BackColor =
			this.renameToolStripMenuItem.BackColor =
			this.colorToolStripMenuItem.BackColor =
			this.contextMenuStrip.BackColor = Color.FromArgb(46, 41, 31);

			this.archiveAllToolStripMenuItem.ForeColor =
			this.unarchiveAllToolStripMenuItem.ForeColor =
			this.duplicateToolStripMenuItem.ForeColor =
			this.archiveToolStripMenuItem.ForeColor =
			this.tagsToolStripMenuItem.ForeColor =
			this.addTagToolStripMenuItem.ForeColor =
			this.toolStripTextBoxAdd.ForeColor =
			this.toolStripTextBoxRename.ForeColor =
			this.deleteToolStripMenuItem.ForeColor =
			this.renameToolStripMenuItem.ForeColor =
			this.colorToolStripMenuItem.ForeColor =
			this.contextMenuStrip.ForeColor = Color.FromArgb(200, 200, 200);

			this.contextMenuStrip.Renderer = new CustomProfessionalRenderer();
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

			this.Form.Controls.Add(this.DropDown);

			RefreshMenuTagList();

		}
	}

	#region Refresh

	private void RefreshContent()
	{
		this._DisableMenuTriggers = true;

		this.bufferedFlowLayoutPanelContent.BackColor = Color.White;

		if (this.SelectedItem is PlayerSave ps)
		{
			this.scalingLabelCharName.Text = ps.ToString();

			// Show duplicate feature ?
			if (Config.UserSettings.Default.AllowCharacterEdit)
			{
				this.duplicateToolStripMenuItem.Visible =
				this.duplicateSeparatorToolStripMenuItem.Visible = true;
			}
			else
			{
				this.duplicateToolStripMenuItem.Visible =
				this.duplicateSeparatorToolStripMenuItem.Visible = false;
			}

			// Archiving toggle
			this.archiveToolStripMenuItem.Checked = ps.IsArchived;

			// Archived Color
			if (ps.IsArchived)
				this.bufferedFlowLayoutPanelContent.BackColor = Color.Silver;

			// Tags Toggle
			foreach (ToolStripItem tagItem in this.tagsToolStripMenuItem.DropDownItems)
			{
				if (tagItem is ToolStripSeparator)
					continue;

				if (tagItem is ToolStripMenuItem mi && tagItem.Name.StartsWith(TAGKEY))
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

	internal void RefreshItem(PlayerSave selectedSave)
	{
		var item = this.DropDown.Items.First(i => i.Item == selectedSave);
		item.RefreshContent();
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

	private void OpenDropDown_Click(object sender, EventArgs e)
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

	private void Saves_ListChanged(object sender, ListChangedEventArgs e)
	{
		switch (e.ListChangedType)
		{
			case ListChangedType.ItemAdded:
				var save = this.Items[e.NewIndex];

				var item = new ComboBoxCharacterItem();
				item.Init(this.DropDown, save, TranslationService, FontService, Database, TagService);
				item.SelectedItemChanged += Item_SelectedItemChanged;
				this.DropDown.Items.Add(item);
				item.RefreshContent();
				break;
		}
	}

	private void Item_SelectedItemChanged(object sender, EventArgs e)
	{
		var selected = sender as ComboBoxCharacterItem;

		if (this.SelectedItem != selected.Item)
		{
			this.SelectedItem = selected.Item;
			DropDownClose();
		}
	}

	private void Form_GlobalMouseButtonLeft(object sender, Point point)
	{
		// Close dropdown if left click outside it's surface
		var mp = MousePosition;
		var dropdownrectangle = DropDown.RectangleToScreen(DropDown.ClientRectangle);
		if (DropDown.Visible && !dropdownrectangle.Contains(mp))
			DropDownClose();
	}

	#endregion

	private void ScalingButtonTools_Click(object sender, EventArgs e)
	{
		this.contextMenuStrip.Show(scalingButtonTools, new Point(0, scalingButtonTools.Height));
	}

	private void TagColorItem_Click(object sender, EventArgs e)
	{
		if (this._DisableMenuTriggers)
			return;

		var colorItem = sender as ToolStripMenuItem;
		if (colorItem is not null)
		{
			var tagItem = colorItem.OwnerItem as ToolStripMenuItem;
			var result = this.colorDialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				var col = this.colorDialog.Color;

				this.TagService.UpdateTag(tagItem.Text, tagItem.Text, col.R, col.G, col.B);

				this.RefreshMenuTagList();
				this.RefreshItems();
				this.RefreshContent();
			}
		}
	}

	private void TagItem_CheckedChanged(object sender, EventArgs e)
	{
		if (this._DisableMenuTriggers)
			return;

		var tagItem = sender as ToolStripMenuItem;
		if (tagItem is not null)
		{
			var ps = this.SelectedItem as PlayerSave;
			if (ps is not null)
			{
				if (tagItem.Checked)
					this.TagService.AssignTag(ps, tagItem.Text);
				else
					this.TagService.UnassignTag(ps, tagItem.Text);

				RefreshItem(ps);
				RefreshContent();
			}
		}
	}

	private void duplicateToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (this._DisableMenuTriggers)
			return;

		this.DuplicateCharacterAction();
	}

	private void TagRenameTextBoxItem_KeyDown(object sender, KeyEventArgs e)
	{
		var textBox = sender as ToolStripTextBox;
		var parentTagMenuItem = textBox.OwnerItem.OwnerItem as ToolStripMenuItem;
		var oldTagName = parentTagMenuItem.Text;

		if (e.KeyCode == Keys.Escape)
		{
			ClearInputAndCollapse(textBox);
		}
		else if (e.KeyCode == Keys.Enter)
		{
			// No string.Empty
			var newTagName = textBox.Text;
			if (string.IsNullOrWhiteSpace(newTagName))
			{
				e.Handled = true;// Cancel event
				return;
			}

			// Tag Exist
			if (this.TagService.Tags.Keys.Any(t => t.Equals(newTagName, StringComparison.OrdinalIgnoreCase)))
			{
				e.Handled = true;// Cancel event
				this.UIService.ShowError(Resources.TagExists, Buttons: ShowMessageButtons.OK);
				return;
			}

			// Tag must be different
			if (oldTagName.Equals(newTagName, StringComparison.OrdinalIgnoreCase))
			{
				e.Handled = true;// Cancel event
				this.UIService.ShowError(Resources.TagMustBeDifferent, Buttons: ShowMessageButtons.OK);
				return;
			}

			// Update tag
			var col = this.TagService.Tags[oldTagName];
			this.TagService.UpdateTag(oldTagName, newTagName, col.R, col.G, col.B);

			RefreshItems();

			ClearInputAndCollapse(textBox);
			RefreshMenuTagList();
			RefreshContent();
		}

	}

	private void TagDeleteItem_Click(object sender, EventArgs e)
	{
		var deleteMenuItem = sender as ToolStripMenuItem;
		var parentTagMenuItem = deleteMenuItem.OwnerItem as ToolStripMenuItem;

		if (UIService.ShowWarning(Resources.GlobalConfirm).IsOK)
		{
			this.TagService.DeleteTag(parentTagMenuItem.Text);

			RefreshItems();
			RefreshMenuTagList();
			RefreshContent();
		}
	}

	private void TagUnArchiveAllItem_Click(object sender, EventArgs e)
	{
		var unArchiveAllMenuItem = sender as ToolStripMenuItem;
		var parentTagMenuItem = unArchiveAllMenuItem.OwnerItem as ToolStripMenuItem;

		// Find Saves having the tag that are archived
		var taggedSaves =
			from ps in this.Items.OfType<PlayerSave>()
			where ps.Tags.ContainsKey(parentTagMenuItem.Text) && ps.IsArchived 
			select ps;

		foreach (var taggedSave in taggedSaves)
			 this.GameFileService.Unarchive(taggedSave);

		RefreshItems();
		RefreshContent();
	}

	private void TagArchiveAllItem_Click(object sender, EventArgs e)
	{
		var archiveAllMenuItem = sender as ToolStripMenuItem;
		var parentTagMenuItem = archiveAllMenuItem.OwnerItem as ToolStripMenuItem;

		// Find Saves having the tag that are not archived
		var taggedSaves =
			from ps in this.Items.OfType<PlayerSave>()
			where ps.Tags.ContainsKey(parentTagMenuItem.Text) && !ps.IsArchived
			select ps;

		foreach (var taggedSave in taggedSaves)
			this.GameFileService.Archive(taggedSave);

		RefreshItems();
		RefreshContent();
	}

	private void ToolStripTextBoxAdd_KeyDown(object sender, KeyEventArgs e)
	{
		var textBox = sender as ToolStripTextBox;

		if (e.KeyCode == Keys.Escape)
		{
			ClearInputAndCollapse(textBox);
		}
		else if (e.KeyCode == Keys.Enter)
		{
			// No string.Empty
			var txt = textBox.Text;
			if (string.IsNullOrWhiteSpace(txt))
			{
				e.Handled = true;// Cancel event
				return;
			}

			// Tag Exist
			if (this.TagService.Tags.Keys.Any(t => t.Equals(txt, StringComparison.OrdinalIgnoreCase)))
			{
				e.Handled = true;// Cancel event
				this.UIService.ShowError(Resources.TagExists, Buttons: ShowMessageButtons.OK);
				return;
			}

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
		var parent = textbox.OwnerItem as ToolStripMenuItem;
		parent.HideDropDown();
	}

	private void toolStripTextBox_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (this._DisableMenuTriggers)
			return;

		var texBox = sender as ToolStripTextBox;
		// Filter input ??
		/*
		if (!Regex.IsMatch(e.KeyChar.ToString(), @"[\w\. \-\[\]]"))
		{
			e.Handled = true;// Means cancel event
			return;
		}
		*/
	}

	private void archiveToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
	{
		if (this._DisableMenuTriggers)
			return;

		var ps = this.SelectedItem as PlayerSave;
		if (ps is not null)
		{
			// Archive it
			if (archiveToolStripMenuItem.Checked)
			{
				GameFileService.Archive(ps);
				RefreshItem(ps);
				RefreshContent();
				return;
			}

			// Unarchive it
			GameFileService.Unarchive(ps);
			RefreshItem(ps);
			RefreshContent();
		}
	}
}
