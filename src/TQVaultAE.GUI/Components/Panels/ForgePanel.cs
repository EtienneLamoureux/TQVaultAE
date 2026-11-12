using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Forge;
using TQVaultAE.GUI.Components.Icons;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.GUI.Models;
using TQVaultAE.GUI.Tooltips;
using TQVaultAE.Presentation.Win32.Contracts;
using TQVaultAE.Presentation.Win32;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Components.Panels;

/// <summary>
/// Forge: merges the prefix, suffix and relics of material items into a base item, under the rules of a
/// <see cref="ForgeMode"/> (<see cref="ForgeRules"/>).
/// </summary>
/// <remarks>
/// While an item is dragged over the panel it is shown as the mouse cursor (<see cref="ItemDragCursor"/>), so moving
/// the mouse repaints nothing; clicking a slot drops it there.
/// </remarks>
public partial class ForgePanel : UserControl, IScalingControl, IFontRebasing
{
	#region Internal Types

	private sealed class ForgeComboItem
	{
		public ForgePart Value;
		public string Text = string.Empty;
		public override string ToString() => Text;
	}

	/// <summary>Controls of one slot.</summary>
	private sealed record SlotView(ForgeSlot Slot, PictureBox Picture, ComboBox? Combo);

	#endregion

	private readonly ISoundService SoundService;
	private readonly IUIService UIService;
	private readonly IFontService FontService;
	private readonly ITranslationService TranslationService;

	internal Action? CancelAction;
	internal Action? ForgeAction;

	/// <summary>Items placed in the slots, and the sacks they come from.</summary>
	private readonly ForgeSelection Selection = new();
	private readonly Dictionary<ForgeSlot, SackCollection?> Sacks = [];
	private readonly SlotView[] Slots;
	private readonly ForgeRules Rules;

	/// <summary>Cards describing what each mode allows, on the mode radio buttons.</summary>
	private RichToolTip? modeToolTip;

	/// <summary>The mode chosen before the current one, to reset the selection when the rules get stricter.</summary>
	private ForgeMode? lastMode;

	private static RecordId SoundStricMode = @"Sounds\MONSTERS\GREECE\G_TELKINE\TELEKINEVOICE01.WAV";
	private static RecordId SoundRelaxMode = @"Sounds\MONSTERS\GREECE\G_TELKINE\TELEKINEVOICE02.WAV";
	private static RecordId SoundGameMode = @"Sounds\MONSTERS\GREECE\G_TELKINE\TELEKINEVOICE03.WAV";
	private static RecordId SoundGodMode = @"Sounds\AMBIENCE\RANDOMEVENT\TYPHONLAUGHDISTANCE.WAV";

	/// <summary>
	/// Gets or sets the dragInfo instance of any items being dragged.
	/// </summary>
	protected ItemDragInfo DragInfo;
	private readonly GuiServices Services;

	/// <summary>Cursor showing the dragged item while over the panel, and the item it was made for.</summary>
	private ItemDragCursor? dragCursor;
	private Item? dragCursorItem;

	/// <summary>Backgrounds drawn once at the control size, instead of being stretched on every paint.</summary>
	private Bitmap? panelBackground;
	private Bitmap? tableBackground;

	private bool IsGodMode => scalingRadioButtonGod.Checked;
	private bool IsRelaxMode => scalingRadioButtonRelax.Checked;
	private bool IsStrictMode => scalingRadioButtonStrict.Checked;
	private bool IsGameMode => scalingRadioButtonGame.Checked;
	private bool IsDraging => DragInfo is not null && DragInfo.IsActive;

	private ForgeMode Mode
		=> IsGodMode ? ForgeMode.God
		: IsRelaxMode ? ForgeMode.Relax
		: IsStrictMode ? ForgeMode.Strict
		: ForgeMode.Game;

	/// <summary>What the current mode allows.</summary>
	private ForgeCapabilities Capabilities => ForgeRules.Capabilities(this.Mode);

	private readonly string OfTheTinkererTranslation;

	private readonly Color PictureBoxBaseColor = Color.FromArgb(32 * 7, 46, 41, 31);

	Item? _PreviewItem = null;

	internal bool HardcoreMode;

	/// <summary>Base item with the current selection applied, for its tooltip.</summary>
	private Item? PreviewItem
	{
		get
		{
			if (Selection.Base is null) return null;
			return _PreviewItem ??= this.Services.ForgeService.Preview(Selection, this.Capabilities.AnyPartOfMaterial, GodPart);
		}
	}

#pragma warning disable CS8618 // Designer only: the runtime constructor sets the services and state.
	public ForgePanel()
	{
		InitializeComponent();
	}
#pragma warning restore CS8618

	public ForgePanel(ItemDragInfo dragInfo, GuiServices services)
	{
		InitializeComponent();

		DoubleBuffered = true;

		DragInfo = dragInfo;
		Services = services;
		SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor, true);

		SoundService = services.SoundService;
		UIService = services.UIService;
		FontService = services.FontService;
		TranslationService = services.TranslationService;

		Rules = services.ForgeService.CreateRules();
		Slots = [
			new(ForgeSlot.Base, pictureBoxBaseItem, null),
			new(ForgeSlot.Prefix, pictureBoxPrefix, comboBoxPrefix),
			new(ForgeSlot.Suffix, pictureBoxSuffix, comboBoxSuffix),
			new(ForgeSlot.Relic1, pictureBoxRelic1, comboBoxRelic1),
			new(ForgeSlot.Relic2, pictureBoxRelic2, comboBoxRelic2),
		];

		#region Font, Scaling, Translate, BackGround

		this.RenderBackgrounds();
		this.SizeChanged += (_, _) => this.RenderBackgrounds();
		tableLayoutPanelForge.SizeChanged += (_, _) => this.RenderBackgrounds();

		// Reduce slightly the designed component to fit better the UI.
		foreach (var slot in Slots)
			slot.Picture.Size = new Size(Convert.ToInt32(slot.Picture.Size.Width * 0.9), Convert.ToInt32(slot.Picture.Size.Height * 0.9));

		scalingCheckBoxHardcore.Font =
		scalingRadioButtonGame.Font =
		scalingRadioButtonGod.Font =
		scalingRadioButtonRelax.Font =
		scalingRadioButtonStrict.Font =
			FontService.GetFont(10F, FontStyle.Regular, UIService.FontZoom);

		scalingLabelBaseItem.Font =
		scalingLabelPrefix.Font =
		scalingLabelRelic1.Font =
		scalingLabelRelic2.Font =
		scalingLabelSuffix.Font =
			FontService.GetFont(11F, FontStyle.Bold, UIService.FontZoom);

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

		// ComboBox scaling
		comboBoxPrefix.Font =
		comboBoxSuffix.Font =
		comboBoxRelic1.Font =
		comboBoxRelic2.Font =
			FontService.GetFont(9.75F, FontStyle.Regular, UIService.FontZoom);

		scalingRadioButtonGod.Text = Resources.ForgeGodMode;
		scalingRadioButtonGod.ForeColor = TQColor.Indigo.Color();

		OfTheTinkererTranslation = TranslationService.TranslateXTag("x3tagSuffix01");

		scalingRadioButtonRelax.Text = Resources.ForgeRelaxMode;
		scalingRadioButtonRelax.ForeColor = TQColor.Aqua.Color();

		scalingRadioButtonStrict.Text = Resources.ForgeStrictMode;
		scalingRadioButtonStrict.ForeColor = TQColor.Yellow.Color();

		scalingRadioButtonGame.Text = Resources.ForgeGameMode;
		scalingRadioButtonGame.ForeColor = TQColor.Green.Color();

		this.SetModeToolTips();

		scalingCheckBoxHardcore.Text = Resources.GlobalHardcore;
		toolTip.SetToolTip(scalingCheckBoxHardcore, Resources.ForgeHardcoreTT);
		scalingCheckBoxHardcore.ForeColor = TQColor.Red.Color();

		#endregion

		HideGodModeRows();

		// Everything in the Forge takes part in dragging
		this.WireDragEvents(this);
		this.ProcessAllControls(this.WireDragEvents);

		ResetSelection();
	}

	private void WireDragEvents(Control c)
	{
		c.MouseEnter += Forge_MouseEnter;
		c.MouseMove += Forge_MouseMove;
		c.MouseLeave += Forge_MouseLeave;
		c.Click += Forge_Click;
	}

	/// <summary>
	/// The base font changed: the mode tooltips, drawn with fonts of their own, are made again in it.
	/// </summary>
	void IFontRebasing.RebaseFonts(IFontService fontService)
	{
		var previous = this.modeToolTip;
		if (previous is null)
			return;

		this.SetModeToolTips();
		previous.Dispose();
	}

	/// <summary>
	/// Describes on each mode radio button what the mode allows: one line per check of <see cref="ForgeRules"/>.
	/// </summary>
	private void SetModeToolTips()
	{
		this.modeToolTip = new RichToolTip(
			FontService.GetFont(11F, FontStyle.Bold, UIService.FontZoom),
			FontService.GetFont(9.5F, FontStyle.Regular, UIService.FontZoom),
			FontService.GetFont(10F, FontStyle.Bold, UIService.FontZoom),
			UIService.Scale);

		foreach (var (radio, mode) in new[]
		{
			(scalingRadioButtonGame, ForgeMode.Game),
			(scalingRadioButtonStrict, ForgeMode.Strict),
			(scalingRadioButtonRelax, ForgeMode.Relax),
			(scalingRadioButtonGod, ForgeMode.God),
		})
		{
			var card = ForgeModeCardText.Build(mode, OfTheTinkererTranslation);
			this.modeToolTip.Set(radio, new RichToolTip.Card(
				card.Title,
				radio.ForeColor,
				card.Summary,
				card.Sections.Select(s => new RichToolTip.Section(s.Title, s.Lines.Select(l => new RichToolTip.Line(l.Text, l.Allowed)).ToList())).ToList(),
				card.Footer));
		}
	}

	#region Backgrounds

	private void RenderBackgrounds()
	{
		ReplaceBackground(this, ref this.panelBackground, () => Images.caravan_bg);
		ReplaceBackground(this.tableLayoutPanelForge, ref this.tableBackground, () => Images.StashPanel);
	}

	/// <summary>Sets <paramref name="control"/>'s background to the image drawn once at its size.</summary>
	/// <param name="load">Loads the source image (a new copy each call); only called when the size changed.</param>
	private static void ReplaceBackground(Control control, ref Bitmap? cache, Func<Bitmap> load)
	{
		var size = control.ClientSize;
		if (size.Width <= 0 || size.Height <= 0 || (cache is not null && cache.Size == size))
			return;

		using (var source = load())
		{
			var rendered = new Bitmap(size.Width, size.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
			using (var g = Graphics.FromImage(rendered))
			{
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
				g.DrawImage(source, new Rectangle(Point.Empty, size));
			}

			control.BackgroundImageLayout = ImageLayout.None;
			control.BackgroundImage = rendered;
			cache?.Dispose();
			cache = rendered;
		}
	}

	#endregion

	#region Dragging

	private void Forge_MouseEnter(object? sender, EventArgs e)
	{
		// Enforce first autosize display
		if (MaximumSize == Size.Empty)
			MaximumSize = new Size(Size.Width, Size.Height + 50);

		this.UpdateDragCursor();
	}

	private void Forge_MouseMove(object? sender, MouseEventArgs e) => this.UpdateDragCursor();

	private void Forge_MouseLeave(object? sender, EventArgs e)
	{
		// Leaving a child for another one stays inside the panel
		if (!this.ClientRectangle.Contains(this.PointToClient(Cursor.Position)))
			this.EndDragCursor();
	}

	private void Forge_Click(object? sender, EventArgs e)
	{
		if (!IsDraging)
			return;

		var slot = Slots.FirstOrDefault(s => s.Picture.Visible
			&& s.Picture.RectangleToScreen(s.Picture.ClientRectangle).Contains(Cursor.Position));
		if (slot is not null)
			this.DropDraggedItem(slot.Slot);
	}

	/// <summary>Shows the dragged item as the cursor, or the normal cursor when nothing is dragged.</summary>
	private void UpdateDragCursor()
	{
		if (!IsDraging)
		{
			this.EndDragCursor();
			return;
		}

		if (this.dragCursor is not null && ReferenceEquals(this.dragCursorItem, DragInfo.Item))
			return;

		this.EndDragCursor();
		var bitmap = DragInfo.Item is { } dragged ? UIService.GetBitmap(dragged) : null;

		this.dragCursor = bitmap is null ? null : ItemDragCursor.Create(bitmap, UIService.Scale);
		this.dragCursorItem = DragInfo.Item;
		if (this.dragCursor is not null)
			this.Cursor = this.dragCursor.Cursor;
	}

	private void EndDragCursor()
	{
		if (this.dragCursor is null)
			return;
		this.Cursor = Cursors.Default;
		this.dragCursor.Dispose();
		this.dragCursor = null;
		this.dragCursorItem = null;
	}

	/// <summary>Puts the dragged item in <paramref name="slot"/> when the rules of the current mode allow it.</summary>
	private void DropDraggedItem(ForgeSlot slot)
	{
		if (DragInfo.SrcSack is null)
		{
			Notify(ForgeMessage.ItemMustBeStoredFirst);
			return;
		}

		if (DragInfo.Item is not { } item || Selection.Contains(item))
			return;

		var decision = Rules.CheckDrop(Mode, slot, item, Selection);

		foreach (var cleared in decision.Cleared)
			ClearSlot(cleared);
		foreach (var warning in decision.Warnings)
			Notify(warning);

		if (!decision.Accepted)
		{
			Notify(decision.Rejection!.Value);
			return;
		}

		PlaceInSlot(slot, item, DragInfo.SrcSack);
		DragInfo.Cancel();
		EndDragCursor();
		SoundService.PlayRandomItemDrop();
		ResetPreviewItem();
	}

	#endregion

	#region Slots

	private SlotView SlotOf(ForgeSlot slot) => Slots.First(s => s.Slot == slot);

	private void PlaceInSlot(ForgeSlot slot, Item item, SackCollection sack)
	{
		Selection[slot] = item;
		Sacks[slot] = sack;

		var view = SlotOf(slot);
		view.Picture.Image = UIService.GetBitmap(item);
		view.Picture.BackColor = Color.FromArgb(127, ControlPaint.Dark(item.ItemStyle.Color()));
		if (view.Combo is not null)
			FillPartComboBox(view.Combo, item);
	}

	private void ClearSlot(ForgeSlot slot)
	{
		Selection[slot] = null;
		Sacks[slot] = null;

		var view = SlotOf(slot);
		view.Picture.Image = null;
		view.Picture.BackColor = PictureBoxBaseColor;
		view.Combo?.Items.Clear();
	}

	private void ResetSelection()
	{
		foreach (var slot in Slots)
			ClearSlot(slot.Slot);

		ResetPreviewItem();
	}

	private void ResetPreviewItem()
	{
		if (_PreviewItem is not null)
			ItemTooltip.InvalidateCache(_PreviewItem);// Invalide old preview item
		_PreviewItem = null;
	}

	/// <summary>God mode: the part of the material item chosen in its combo box.</summary>
	private ForgePart? GodPart(ForgeSlot slot)
		=> (SlotOf(slot).Combo?.SelectedItem as ForgeComboItem)?.Value;

	/// <summary>Parts of <paramref name="item"/> usable in God mode.</summary>
	private void FillPartComboBox(ComboBox comboBox, Item item)
	{
		comboBox.Items.Clear();

		foreach (var part in this.Services.ForgeService.GodModeParts(item))
			comboBox.Items.Add(new ForgeComboItem { Value = part, Text = ForgeMessageText.PartOf(part) });

		comboBox.SelectedIndex = 0;// default on Base
	}

	private void HideGodModeRows()
	{
		foreach (var slot in Slots)
			if (slot.Combo is not null)
				slot.Combo.Visible = false;

		this.FitHeight();
	}

	private void ShowGodModeRows()
	{
		foreach (var slot in Slots)
			if (slot.Combo is not null)
			{
				slot.Combo.SelectedIndex = -1;
				slot.Combo.Visible = true;
			}

		this.FitHeight();
	}

	#endregion

	#region Height

	/// <summary>
	/// The height the panel may take, asked at each layout: the window's panels change size with the zoom and the
	/// monitor, in no fixed order. Null or 0 for no limit.
	/// </summary>
	public Func<int>? HeightLimit { get; set; }

	/// <summary>The pixels taken off each item slot's height so that the panel fits <see cref="HeightLimit"/>.</summary>
	private int slotCut;

	/// <summary>
	/// The part of its height an item slot keeps at least: the tallest item (4 units) takes about 55% of a full slot, and
	/// God mode's two rows of lists need the slots down to about 75% at the normal size.
	/// </summary>
	private const float MinSlotHeightRatio = 0.7F;

	/// <summary>
	/// Keeps the panel within <see cref="HeightLimit"/>: its three rows of item slots give up what is over, in every
	/// mode (God mode adds two rows of lists).
	/// </summary>
	public void FitHeight()
	{
		var maxHeight = this.HeightLimit?.Invoke() ?? 0;
		if (maxHeight <= 0 || Slots is null)
			return;

		const int slotRows = 3;
		var slotHeight = pictureBoxBaseItem.Height + this.slotCut;
		var fullHeight = this.GetPreferredSize(Size.Empty).Height + (slotRows * this.slotCut);
		var cut = Math.Max(0, (fullHeight - maxHeight + slotRows - 1) / slotRows);
		cut = Math.Min(cut, slotHeight - (int)(slotHeight * MinSlotHeightRatio));
		if (cut == this.slotCut)
			return;

		this.slotCut = cut;
		foreach (var slot in Slots)
			slot.Picture.Height = slotHeight - cut;
	}

	/// <summary>A fit is queued for when the current layout or rescale is over.</summary>
	private bool fitQueued;

	/// <summary>
	/// Fits again after each layout, and once more when the window is done: a rescale lays the panel out while its
	/// children and the panels of the limit are only partly scaled. No change, no further layout.
	/// </summary>
	protected override void OnLayout(LayoutEventArgs e)
	{
		base.OnLayout(e);
		this.FitHeight();
		if (this.fitQueued || !this.IsHandleCreated || this.Disposing)
			return;

		this.fitQueued = true;
		this.BeginInvoke(() =>
		{
			this.fitQueued = false;
			this.FitHeight();
		});
	}

	/// <summary>A queued fit is lost with the handle: the next layout may queue one again.</summary>
	protected override void OnHandleDestroyed(EventArgs e)
	{
		this.fitQueued = false;
		base.OnHandleDestroyed(e);
	}

	/// <summary>The item slots get their full height back before a rescale, so that the scale applies to it.</summary>
	protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
	{
		if (this.slotCut != 0 && Slots is not null)
		{
			foreach (var slot in Slots)
				slot.Picture.Height += this.slotCut;
			this.slotCut = 0;
		}

		base.ScaleControl(factor, specified);
	}

	#endregion

	#region Forge

	/// <summary>
	/// The forge key (<see cref="TQVaultAE.Application.KeyBinds.KeyBindAction.Forge"/>): forges as the Forge button would, not while
	/// an item is dragged.
	/// </summary>
	internal void ForgeFromShortcut()
	{
		if (!this.ForgeButton.Enabled || this.IsDraging)
			return;

		ForgeItem();
	}

	private void ForgeItem()
	{
		var result = this.Services.ForgeService.Forge(Rules, Selection, Sacks, this.Capabilities.AnyPartOfMaterial, GodPart, this.HardcoreMode);
		if (result.Problem is { } problem)
		{
			Notify(problem);
			return;
		}

		InvalidateTooltipCaches([.. result.Consumed, result.Forged!]);

		// Repaint the sacks the items left (and the forged item) at the next paint, not synchronously.
		this.FindForm()?.Invalidate(true);

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

	/// <summary>The forged and consumed items are drawn anew; the forge service refreshed their friendly names.</summary>
	private static void InvalidateTooltipCaches(Item[] items)
	{
		ItemTooltip.InvalidateCache(items);
		BagButtonTooltip.InvalidateCache(items);
	}

	#endregion

	#region Messages

	private void Notify(ForgeMessage message)
	{
#if DEBUG
		this.AutomationRecordMessage(message);
#endif
		this.UIService.NotifyUser(ForgeMessageText.Of(message, OfTheTinkererTranslation), TQColor.Red);
		SoundService.PlayRandomCancel();
	}

	#endregion

	#region Event handlers

	private void CancelButton_Click(object? sender, EventArgs e)
	{
		ResetSelection();

		if (CancelAction is not null)
			CancelAction();
	}

	private void ForgeButton_Click(object? sender, EventArgs e) => ForgeItem();

	private void scalingButtonReset_Click(object? sender, EventArgs e) => ResetSelection();

	private void ForgeMode_Clicked(object? sender, EventArgs e)
	{
		if (!IsGameMode && !IsStrictMode && !IsRelaxMode && !IsGodMode)
			return;

		var mode = Mode;

		SoundService.PlaySound(mode switch
		{
			ForgeMode.God => SoundGodMode,
			ForgeMode.Relax => SoundRelaxMode,
			ForgeMode.Strict => SoundStricMode,
			_ => SoundGameMode,
		});

		// Reset on Mode downgrade
		if (ForgeRules.IsDowngrade(lastMode, mode))
			ResetSelection();

		if (ForgeRules.Capabilities(mode).AnyPartOfMaterial)
			ShowGodModeRows();
		else
			HideGodModeRows();

		lastMode = mode;
	}

	private void pictureBox_MouseEnter(object? sender, EventArgs e)
	{
		// No tooltip over the slot the dragged item is about to be dropped on
		if (IsDraging)
			return;

		var slot = Slots.First(s => s.Picture == sender).Slot;
		var itm = slot == ForgeSlot.Base ? PreviewItem : Selection[slot];
		if (itm is null) return;

		ItemTooltip.ShowTooltip(Services, itm, (PictureBox)sender!, true);
	}

	private void pictureBox_MouseLeave(object? sender, EventArgs e)
	{
		ItemTooltip.HideTooltip();
	}

	private void comboBox_SelectedIndexChanged(object? sender, EventArgs e)
		=> ResetPreviewItem();

	private void scalingCheckBoxHardcore_CheckedChanged(object? sender, EventArgs e)
	{
		this.HardcoreMode = scalingCheckBoxHardcore.Checked;
	}

	#endregion

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			this.EndDragCursor();
			this.modeToolTip?.Dispose();
			this.panelBackground?.Dispose();
			this.tableBackground?.Dispose();
			this.components?.Dispose();
		}
		base.Dispose(disposing);
	}
}
