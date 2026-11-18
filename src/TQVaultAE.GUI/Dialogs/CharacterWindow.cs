using System.Globalization;
using TQVaultAE.Application.Layout;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Character;
using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.GUI.Components.Rendering;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Dialogs;

/// <summary>
/// A character in a floating window: its equipment on the stone of the slots and the four statistics tabs side by side
/// (I, II, III, Buffs), so that a toggle of the Buffs tab shows its effect on every number at once. A modeless window
/// owned by the main window, one per character (<see cref="Views.MainFormUserControl.OpenCharacterWindow"/>); its buff
/// toggles are its own, in memory only. Items go to and from the main window by drag and drop, with the main window's
/// rules; changes are saved by the main window's Save. A list right of the gold bar and the arrows on the stone show a
/// loadout's pieces read-only in place of the equipment, the statistics computed with them; Compare opens through the
/// main window.
/// </summary>
internal sealed class CharacterWindow : VaultForm, IItemComparisonHost
{
	/// <summary>The name of the window's form.</summary>
	public const string FormName = "CharacterWindow";

	/// <summary>The gap between the border and the content, in design pixels.</summary>
	private const int ContentGap = 6;

	private readonly Func<(Bitmap Picture, Rectangle Source, Rectangle Target, Size Slots)?> _slotsGeometry;
	private readonly ItemDragInfo _dragInfo;
	private readonly CharacterWindowEquipmentPanel _equipment;
	private readonly EquipmentSlotsView _slots;
	private readonly CharacterStatsPanel[] _tabs;
	private readonly CharacterStatsSource _stats;
	private readonly CharacterStatsLook _look;
	private readonly IItemComparisonHost _comparisonHost;
	private readonly GameDropDown _loadoutBox;

	/// <summary>The class set list under tab III: the save's build, then the character's class sets.</summary>
	private readonly GameDropDown _classSetBox;

	/// <summary>The character's class sets, read again each time the window comes to the front.</summary>
	private ClassSetBook? _classSetBook;
	private readonly ToolTip _toolTip = new();

	/// <summary>The font of the loadout list, made at the window's scale.</summary>
	private Font? _loadoutFont;

	/// <summary>The loadout shown in place of the equipment, or null for the equipment worn.</summary>
	private string? _loadoutName;

	/// <summary>The pieces of the loadout shown, as the loadout held them when its slots were built.</summary>
	private IReadOnlyList<Item?>? _shownPieces;

	/// <summary>Set while the window lays itself out: its moves are not the user's.</summary>
	private bool _layingOut;

	/// <summary>Initializes a new instance of the CharacterWindow class.</summary>
	/// <param name="owner">The main window, which owns the window.</param>
	/// <param name="save">The character.</param>
	/// <param name="player">The character's loaded save, shared with the main window.</param>
	/// <param name="dragInfo">The drag shared by every panel of the main window.</param>
	/// <param name="slotsGeometry">The slots picture and cells of the main equipment, at the main window's scale.</param>
	/// <param name="comparisonHost">The main content, which opens the item comparisons.</param>
	public CharacterWindow(MainForm owner, PlayerSave save, PlayerCollection player, ItemDragInfo dragInfo,
		Func<(Bitmap Picture, Rectangle Source, Rectangle Target, Size Slots)?> slotsGeometry, IItemComparisonHost comparisonHost)
		: base(owner.Services)
	{
		this.Owner = owner;
		this.Save = save;
		this.Player = player;
		this._dragInfo = dragInfo;
		this._slotsGeometry = slotsGeometry;
		this._comparisonHost = comparisonHost;

		// Laid out in code at the user's zoom and the window's own DPI (LayOutContent), again when either changes.
		this.UserZoomForLayout();
		CaravanDialog.SetUpWindow(this, FormName);
		this.AutoScaleMode = AutoScaleMode.None;
		this.StartPosition = FormStartPosition.Manual;
		this.Text = string.Format(CultureInfo.CurrentCulture, Resources.CharacterWindowTitle, save.Name);
		this.KeyPreview = true;

		// Solid black behind the stone and the panels.
		this.BackColor = Color.Black;

		this._equipment = new CharacterWindowEquipmentPanel(dragInfo, this.Services, save)
		{
			PlayerCollection = player,
		};
		this._slots = new EquipmentSlotsView(this._equipment, this.UIService) { Name = "characterWindowSlots" };

		// The game data of the character's own map while its statistics are computed (the main window may show another).
		this._stats = new CharacterStatsSource(this.Services) { ReadScope = () => this.Services.GameMapService.ReadWith(save) };
		this._look = new CharacterStatsLook(this.FontService, this.UIService);
		this._tabs = [.. Enumerable.Range(0, CharacterWindowLayout.StatsPanels).Select(tab => new CharacterStatsPanel(this.UIService, this.FontService)
		{
			Name = $"characterWindowTab{tab + 1}",
			Tab = tab,
			TabsFixed = true,
			GoldBarShown = tab == 0,
			View = () => this._stats.View(this.Player, this._equipment.WornItems),
		})];

		// Each window its own toggles: a click in its Buffs tab changes the four tabs at once.
		this._stats.BindBuffs(this._tabs[CharacterStatsPanel.BuffsTab], this.InvalidateTabs);
		this._stats.BindWeaponSets(this._tabs[0], this.InvalidateTabs);
		this._equipment.OnEquipmentChanged += (_, _) =>
		{
			this._stats.Invalidate();
			this.InvalidateTabs();
			this.EquipmentChanged?.Invoke(this, EventArgs.Empty);
		};

		// The loadouts: the previous and next arrows on the stone, as in the loadout panel, and a list headed by the
		// equipment worn right of the gold bar.
		this._slots.TurnRequested += (_, step) => this.TurnLoadout(step);
		this._loadoutBox = new GameDropDown(this.UIService, this.FontService) { Name = "characterWindowLoadout" };
		this._loadoutBox.ItemPicked += (_, index) => this.ShowLoadout(index <= 0 ? null : this._loadoutBox.Items[index]);
		this._toolTip.SetToolTip(this._loadoutBox, Resources.LoadoutTitle);

		// The class sets: the statistics computed with another build, as the character editor applies it; memory only.
		this._classSetBox = new GameDropDown(this.UIService, this.FontService) { Name = "characterWindowClassSet" };
		this._classSetBox.ItemPicked += (_, index) => this.ShowClassSet(index <= 0 ? null : this._classSetBox.Items[index]);
		this._toolTip.SetToolTip(this._classSetBox, Resources.ClassSetTitle);

		this.Controls.Add(this._slots);
		this.Controls.AddRange(this._tabs);
		this.Controls.Add(this._loadoutBox);
		this.Controls.Add(this._classSetBox);

		// In front of tabs II and III, whose panels take the gold bar's row too.
		this._loadoutBox.BringToFront();
		this._classSetBox.BringToFront();

		// Tab I's gold bar goes on under tab II: tab I in front.
		this._tabs[0].BringToFront();

		this.DrawCustomBorder = true;
		this.ResizeCustomAllowed = false;
		this._equipment.Sack = player.EquipmentSack;
		this.FillLoadouts();
		this.FillClassSets();
	}

	/// <summary>The character shown.</summary>
	public PlayerSave Save { get; }

	/// <summary>The character's loaded save, shared with the main window.</summary>
	public PlayerCollection Player { get; private set; }

	/// <summary>The equipment panel.</summary>
	internal CharacterWindowEquipmentPanel EquipmentPanel => this._equipment;

	/// <summary>The statistics panels, tab I first.</summary>
	internal IReadOnlyList<CharacterStatsPanel> StatsPanels => this._tabs;

	/// <summary>The loadout shown in place of the equipment, or null for the equipment worn.</summary>
	internal string? LoadoutName => this._loadoutName;

	/// <summary>The character's loadouts, in the order of the list (after the equipment worn).</summary>
	internal IReadOnlyList<string> LoadoutNames => [.. this.Loadouts()?.Loadouts.Select(l => l.Name) ?? []];

	/// <summary>The loadout list right of the gold bar.</summary>
	internal GameDropDown LoadoutList => this._loadoutBox;

	/// <summary>The class set list under tab III.</summary>
	internal GameDropDown ClassSetList => this._classSetBox;

	/// <summary>The class set the statistics are computed with, or null for the save's build.</summary>
	internal string? ClassSetName => this._stats.ClassSet?.Name;

	/// <summary>A statistics tab (0 for tab I).</summary>
	internal CharacterStatsPanel StatsTab(int index) => this._tabs[index];

	/// <summary>The buff toggles of this window.</summary>
	internal Application.Character.BuffSelection Buffs => this._stats.Buffs;

	/// <summary>The weapon set this window's statistics count; null for the one in hand in the save.</summary>
	internal bool? AlternateWeapons => this._stats.AlternateWeapons;

	/// <summary>Raised when an item was put in or taken out of the equipment from this window.</summary>
	public event EventHandler? EquipmentChanged;

	/// <summary>Raised when the user moved the window (not when it lays itself out).</summary>
	public event EventHandler? PlaceChanged;

	/// <summary>Follows the main window's zoom while open (<see cref="VaultForm.FollowUserZoom"/>).</summary>
	protected internal override bool FollowsUserZoomChanges => true;

	/// <summary>
	/// The equipment changed elsewhere (the main window shows the same character, or the save was read again): the
	/// slots and the statistics are drawn again, the toggles kept.
	/// </summary>
	public void EquipmentChangedElsewhere()
	{
		this._stats.Invalidate();
		this._equipment.Invalidate();
		this.InvalidateTabs();
	}

	/// <summary>
	/// The character's save was read again into a new instance (hot reload, another map): the window shows it, its toggles
	/// kept. An item held from the old equipment goes back first.
	/// </summary>
	public void Rebind(PlayerCollection player)
	{
		if (ReferenceEquals(player, this.Player))
			return;

		this.ReleaseDrag();
		this.Player = player;
		this._equipment.PlayerCollection = player;
		this._equipment.ShowsLoadout = false;
		this._equipment.Sack = player.EquipmentSack;
		this._shownPieces = null;
		this.FillLoadouts();
		this.ShowLoadout(this._loadoutName);
		this.FillClassSets();
	}

	/// <summary>
	/// The loadouts changed in the main window (added, renamed, deleted, other pieces): the list is filled again and the
	/// loadout shown follows; a loadout gone gives the equipment worn back.
	/// </summary>
	public void LoadoutsChanged()
	{
		this.FillLoadouts();
		this.ShowLoadout(this._loadoutName);
	}

	/// <summary>
	/// Shows a loadout's pieces, read-only, in place of the equipment, or the equipment worn (null, or a loadout not
	/// found); the statistics are computed with the pieces shown. An item held from the equipment goes back first.
	/// </summary>
	/// <param name="name">The loadout, or null.</param>
	internal void ShowLoadout(string? name)
	{
		var set = this.Loadouts();
		var loadout = name is null || set is not { ReadFailed: false } ? null : set.Find(name);
		if (loadout is null)
		{
			this._loadoutName = null;
			this._shownPieces = null;
			if (this._equipment.ShowsLoadout || !ReferenceEquals(this._equipment.Sack, this.Player.EquipmentSack))
			{
				this.ReleaseDrag();
				this._equipment.ShowsLoadout = false;
				this._equipment.Sack = this.Player.EquipmentSack;
			}
		}
		else
		{
			this._loadoutName = loadout.Name;
			if (!this._equipment.ShowsLoadout || !ReferenceEquals(loadout.Pieces, this._shownPieces))
			{
				this.ReleaseDrag();
				this._shownPieces = loadout.Pieces;
				this._equipment.ShowsLoadout = true;
				this._equipment.Sack = LoadoutPanel.BuildSack(loadout, this.Player.EquipmentSack?.NumberOfSlots ?? EquipmentRules.ArtifactSlot + 1, set!.Mod);
			}
		}

		this.SelectLoadoutInList();
		this.EquipmentChangedElsewhere();
	}

	/// <summary>The character's loadouts, read with its save; null when there are none to read.</summary>
	private LoadoutSet? Loadouts()
		=> this.Services.Loadouts.Find(Path.GetDirectoryName(this.Player.PlayerFile) ?? string.Empty);

	/// <summary>Fills the list: the equipment worn, then the loadouts; the arrows are greyed without a loadout.</summary>
	private void FillLoadouts()
	{
		List<string> names = this.Loadouts() is { ReadFailed: false } set ? [.. set.Loadouts.Select(l => l.Name)] : [];
		List<string> items = [Resources.CharacterWindowCurrentGear, .. names];
		this._loadoutBox.SetItems(items, this.LoadoutIndex(items));
		this._slots.CanTurn = names.Count > 0;
	}

	/// <summary>Selects the loadout shown in the list (the equipment worn first).</summary>
	private void SelectLoadoutInList() => this._loadoutBox.SetItems(this._loadoutBox.Items, this.LoadoutIndex(this._loadoutBox.Items));

	/// <summary>The place of the loadout shown in the list's entries; the equipment worn's (0) when it is not listed.</summary>
	private int LoadoutIndex(IReadOnlyList<string> items)
		=> this._loadoutName is null ? 0 : Math.Max(0, items.ToList().IndexOf(this._loadoutName));

	/// <summary>The previous (-1) or next (+1) entry of the list, the equipment worn included, the last followed by the first.</summary>
	internal void TurnLoadout(int step)
	{
		int count = this._loadoutBox.Items.Count;
		if (count < 2)
			return;

		int next = (((Math.Max(0, this._loadoutBox.SelectedIndex) + step) % count) + count) % count;
		this.ShowLoadout(next == 0 ? null : this._loadoutBox.Items[next]);
	}

	/// <summary>
	/// Fills the class set list: the save's build, then the character's class sets, read again (the character editor may
	/// have changed them). The set shown stays while it is listed, with its content of now; else the save's build.
	/// </summary>
	private void FillClassSets()
	{
		this._classSetBook = this.Services.ClassSets.Read(Path.GetDirectoryName(this.Player.PlayerFile) ?? string.Empty);
		this._classSetBox.SetItems([Resources.CharacterWindowCurrentBuild, .. this._classSetBook.Sets.Select(s => s.Name)], 0);
		this.ShowClassSet(this._stats.ClassSet?.Name);
	}

	/// <summary>
	/// Computes the statistics with a class set (<see cref="CharacterStatsSource.ChooseClassSet"/>), or with the save's build
	/// (null, or a set no longer listed); the mark tells a set that did not fit the character.
	/// </summary>
	/// <param name="name">The class set, or null.</param>
	internal void ShowClassSet(string? name)
	{
		var set = name is null ? null : this._classSetBook?.Sets.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
		this._stats.ChooseClassSet(set);
		this._classSetBox.SetItems(this._classSetBox.Items, set is null ? 0 : Math.Max(0, this._classSetBox.Items.ToList().IndexOf(set.Name)));

		// The view built now tells whether the set fitted.
		this._stats.View(this.Player, this._equipment.WornItems);
		var mark = this._stats.ClassSetComplete ? null : Resources.ClassSetAppliedInPart;
		this._classSetBox.MarkToolTip = mark;
		this._toolTip.SetToolTip(this._classSetBox, mark ?? Resources.ClassSetTitle);
		this.InvalidateTabs();
	}

	/// <summary>Compare from this window's equipment: the comparison opens through the main window, as from its panels.</summary>
	void IItemComparisonHost.ShowComparison(Item first, Item second) => this._comparisonHost.ShowComparison(first, second);

	/// <summary>Lays the content out again (the main window's scale changed: its equipment geometry follows).</summary>
	public void Relayout()
	{
		if (this.IsHandleCreated && !this.IsDisposed)
			this.LayOutContent(keepLocation: true);
	}

	private void InvalidateTabs()
	{
		foreach (var tab in this._tabs)
			tab.Invalidate();
	}

	/// <summary>An item held from this window's equipment goes back to its slot.</summary>
	/// <returns>False when it cannot (a modified item): the window must stay.</returns>
	private bool ReleaseDrag()
	{
		if (!this._dragInfo.IsActive || !ReferenceEquals(this._dragInfo.SrcSackPanel, this._equipment))
			return true;

		if (!this._dragInfo.CanBeCanceled)
			return false;

		this._dragInfo.Cancel();
		return true;
	}

	/// <summary>
	/// Lays the slots and the four tabs out at the window's scale (the user's zoom times its monitor's DPI factor, made
	/// smaller when the content would not fit its screen) and fits the window around them.
	/// </summary>
	/// <param name="keepLocation">True to keep the window where it is, within its screen; else its place is left as set.</param>
	private void LayOutContent(bool keepLocation)
	{
		if (this._slotsGeometry() is not var (picture, source, target, slots))
			return;

		this._layingOut = true;
		try
		{
			float mainScale = this.UIService.Scale;
			var region = EquipmentSlotsView.RegionSize(target, slots);
			var slotsDesign = new Size((int)Math.Ceiling(region.Width / mainScale), (int)Math.Ceiling(region.Height / mainScale));
			var firstDesign = CharacterStatsPanel.DesignSize;
			var statsDesign = CharacterStatsPanel.DesignSizeWithoutGoldBar;

			var chrome = this.ClientSize - this.ContentArea.Size;
			var area = Screen.FromControl(this).WorkingArea;
			float wanted = this.UIService.Zoom * this.DpiScale();
			int margin = (int)Math.Ceiling(ContentGap * wanted * 2);
			float scale = CharacterWindowLayout.FitScale(wanted, slotsDesign, firstDesign, statsDesign,
				new Size(area.Width - chrome.Width - margin, area.Height - chrome.Height - margin));

			var (slotsBounds, tabBounds, content) = CharacterWindowLayout.Arrange(slotsDesign, firstDesign, statsDesign, scale);
			int gap = (int)Math.Round(ContentGap * scale);
			var origin = this.ContentArea.Location + new Size(gap, gap);

			this._look.Apply(this._tabs, scale);
			for (int i = 0; i < this._tabs.Length; i++)
				this._tabs[i].Location = tabBounds[i].Location + (Size)origin;

			this.LayOutLoadoutSelector(tabBounds, origin, scale);

			slotsBounds.Offset(origin);
			this._slots.Bounds = slotsBounds;
			this._slots.LayOut(picture, source, target, slots, scale / mainScale);

			this.ClientSize = content + chrome + new Size(2 * gap, 2 * gap);
			if (keepLocation)
				this.Location = WindowLayout.KeepOnScreen(this.Location, this.Size, [area]);
		}
		finally
		{
			this._layingOut = false;
		}
	}

	/// <summary>
	/// The loadout list under tab II, right of tab I's gold bar, and the class set list under tab III, as high as the gold
	/// bar's boxes; their font follows the scale.
	/// </summary>
	private void LayOutLoadoutSelector(IReadOnlyList<Rectangle> tabBounds, Point origin, float scale)
	{
		var box = CharacterWindowLayout.LoadoutList(tabBounds[0], tabBounds[1], CharacterStatsPanel.GoldBarBounds, GameListBar.ReferenceHeight, scale);
		box.Offset(origin);

		var font = this._look.Light(15F, FontStyle.Regular);
		if (this._loadoutFont is null || Math.Abs(this._loadoutFont.Size - font.Size) > 0.01F || this._loadoutFont.FontFamily.Name != font.FontFamily.Name)
		{
			var old = this._loadoutFont;
			this._loadoutFont = new Font(font.FontFamily, font.Size, font.Style, font.Unit);
			this._loadoutBox.TextFont = this._loadoutFont;
			old?.Dispose();
		}

		this._loadoutBox.BarScale = scale;
		this._loadoutBox.Bounds = box;

		var classSets = CharacterWindowLayout.ClassSetList(tabBounds[0], tabBounds[2], CharacterStatsPanel.GoldBarBounds, GameListBar.ReferenceHeight, scale);
		classSets.Offset(origin);
		this._classSetBox.TextFont = this._loadoutFont;
		this._classSetBox.BarScale = scale;
		this._classSetBox.Bounds = classSets;
	}

	/// <summary>Back in front: the loadouts and the class sets may have changed in the main window meanwhile.</summary>
	protected override void OnActivated(EventArgs e)
	{
		base.OnActivated(e);
		this.LoadoutsChanged();
		this.FillClassSets();
	}

	/// <inheritdoc/>
	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		this.LayOutContent(keepLocation: true);
	}

	/// <summary>The window moved to a monitor of another DPI: laid out again at its new scale.</summary>
	protected override void FollowDpi()
	{
		this.LayOutContent(keepLocation: true);
		this.RescalePhase("content");
	}

	/// <summary>The user's zoom changed: laid out again at the new zoom.</summary>
	protected override void OnUserZoomFollowed()
	{
		this.LayOutContent(keepLocation: true);
		this.RescalePhase("content");
	}

	/// <summary>The base font changed: the statistics fonts are made again from it.</summary>
	protected override void OnBaseFontChanged()
	{
		base.OnBaseFontChanged();
		this._look.Reset();
		this.Relayout();
	}

	/// <inheritdoc/>
	protected override void OnLocationChanged(EventArgs e)
	{
		base.OnLocationChanged(e);
		if (!this._layingOut && this.Visible && this.WindowState == FormWindowState.Normal)
			this.PlaceChanged?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>Escape puts an item held back, as in the main window.</summary>
	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		if (e.KeyCode == Keys.Escape && this._dragInfo.IsActive && this._dragInfo.CanBeCanceled)
		{
			this._dragInfo.Cancel();
			this.Refresh();
			e.Handled = true;
		}
	}

	/// <summary>An item held from this window's equipment goes back first; a modified one keeps the window open.</summary>
	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		if (e.CloseReason == CloseReason.UserClosing && !this.ReleaseDrag())
		{
			this.Services.MessageBoxes.Inform(Resources.MainFormHoldingItem, Resources.MainFormHoldingItem2);
			e.Cancel = true;
			return;
		}

		this._look.Tooltip?.Hide();
		base.OnFormClosing(e);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			this._look.Dispose();
			this._toolTip.Dispose();
			this._loadoutFont?.Dispose();
		}

		base.Dispose(disposing);
	}
}
