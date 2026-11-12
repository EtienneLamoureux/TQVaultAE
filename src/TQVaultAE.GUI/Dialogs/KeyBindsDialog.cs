using TQVaultAE.Application.KeyBinds;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Rendering;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Dialogs;

/// <summary>
/// Lists every key and mouse binding of the application (<see cref="KeyBindRegistry"/>) and lets the user change them:
/// a click on a shortcut arms it, the next key (or, for a click action, the next click with its modifiers) replaces it.
/// Conflicts within a scope are shown in red and keep OK disabled. Opened from the settings dialog, which saves the
/// result with the other settings. A TQ window like the other dialogs: marble, the list in a caravan frame, gold
/// buttons; laid out in code at the user's zoom, following the DPI.
/// </summary>
internal sealed partial class KeyBindsDialog : VaultForm
{
	/// <summary>Widths of the action and shortcut columns, height of a row and of the list, at zoom 1.</summary>
	private const int ActionWidth = 340, GestureWidth = 180, RowHeight = 24, ListHeight = 430, ContentWidth = ActionWidth + (2 * GestureWidth) + 30;

	private static readonly Color CellBack = Color.FromArgb(30, 26, 18);
	private static readonly Color ArmedBack = Color.FromArgb(70, 58, 24);

	private readonly Panel listPanel;
	private readonly TableLayoutPanel layout;
	private readonly TableLayoutPanel table;
	private readonly ScalingLabel labelStatus;
	private readonly ScalingButton buttonClear;
	private readonly ScalingButton buttonDefaults;
	private readonly ScalingButton buttonOk;
	private readonly ScalingButton buttonCancel;
	private readonly ToolTip toolTip = new();

	/// <summary>The cell of each shortcut slot.</summary>
	private readonly Dictionary<(KeyBindAction Action, int Slot), Label> cells = [];

	private KeyBindMap map = KeyBindMap.Defaults();

	/// <summary>The slot waiting for its new gesture.</summary>
	private (KeyBindAction Action, int Slot)? armed;

	/// <summary>
	/// The bindings edited: set a copy before showing the dialog, read it after OK.
	/// </summary>
	public KeyBindMap Bindings
	{
		get => this.map;
		set
		{
			this.map = value.Clone();
			this.armed = null;
			this.RefreshCells();
		}
	}

	public KeyBindsDialog(GuiServices services) : base(services)
	{
		// Sizes at zoom 1 and 96 dpi: the zoom is applied here, the DPI by Windows Forms (AutoScaleMode.Dpi).
		float zoom = this.UIService.Zoom;
		int Zoomed(int value) => (int)Math.Round(value * zoom);

		var font = this.FontService.GetFontLight(11F, zoom);
		var headerFont = this.FontService.GetFontLight(11.5F, FontStyle.Bold, zoom);

		this.table = new TableLayoutPanel()
		{
			Name = nameof(this.table),
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			BackColor = Color.Black,
			ColumnCount = 3,
			Location = Point.Empty,
			Margin = new Padding(0),
			Padding = new Padding(Zoomed(4), 0, Zoomed(4), Zoomed(4)),
		};

		Label NewText(string text, Font textFont, Color color, int width) => new()
		{
			AutoSize = false,
			Font = textFont,
			ForeColor = color,
			BackColor = Color.Transparent,
			Margin = new Padding(0, Zoomed(1), Zoomed(4), Zoomed(1)),
			Size = new Size(Zoomed(width), Zoomed(RowHeight)),
			Text = text,
			TextAlign = ContentAlignment.MiddleLeft,
			AutoEllipsis = true,
			UseMnemonic = false,
		};

		this.table.SuspendLayout();
		this.table.Controls.Add(NewText(Resources.KeyBindsColumnAction, headerFont, TQColor.Yellow.Color(), ActionWidth));
		this.table.Controls.Add(NewText(Resources.KeyBindsColumnShortcut, headerFont, TQColor.Yellow.Color(), GestureWidth));
		this.table.Controls.Add(NewText(Resources.KeyBindsColumnAlternate, headerFont, TQColor.Yellow.Color(), GestureWidth));

		foreach (var group in KeyBindRegistry.All.GroupBy(d => d.Category))
		{
			var header = NewText(KeyBindText.Category(group.Key), headerFont, TQColor.Orange.Color(), ActionWidth + (2 * GestureWidth));
			header.Margin = new Padding(0, Zoomed(6), 0, Zoomed(1));
			this.table.Controls.Add(header);
			this.table.SetColumnSpan(header, 3);

			foreach (var definition in group)
			{
				var name = NewText(KeyBindText.Action(definition.Action), font, TQColor.White.Color(), ActionWidth);
				this.toolTip.SetToolTip(name, name.Text);
				this.table.Controls.Add(name);
				for (int slot = 0; slot < KeyBindMap.MaxGestures; slot++)
				{
					var cell = NewText(string.Empty, font, TQColor.Silver.Color(), GestureWidth);
					cell.Name = $"{definition.Action}{slot}";
					cell.TextAlign = ContentAlignment.MiddleCenter;
					if (definition.Editable)
					{
						cell.BackColor = CellBack;
						cell.Cursor = Cursors.Hand;
						var key = (definition.Action, slot);
						cell.MouseDown += (_, e) => this.CellMouseDown(key, e.Button, ModifierKeys);
					}
					else
						this.toolTip.SetToolTip(cell, Resources.KeyBindsFixed);

					this.cells[(definition.Action, slot)] = cell;
					this.table.Controls.Add(cell);
				}
			}
		}

		this.table.ResumeLayout();

		// The list draws its rows from its corner and scrolls: the frame goes around it (CaravanFrame.FrameAround).
		this.listPanel = new Panel()
		{
			Name = nameof(this.listPanel),
			AutoScroll = true,
			BackColor = Color.Black,
			Margin = new Padding(0, 0, 0, Zoomed(6)),
			Size = new Size(Zoomed(ContentWidth), Zoomed(ListHeight)),
		};
		this.listPanel.Controls.Add(this.table);
		this.table.SizeChanged += (_, _) => this.FitList();

		this.labelStatus = new ScalingLabel()
		{
			Name = nameof(this.labelStatus),
			AutoSize = true,
			MaximumSize = new Size(Zoomed(ContentWidth), 0),
			Margin = new Padding(0, 0, 0, Zoomed(6)),
			UseMnemonic = false,
		};

		ScalingButton NewButton(string name, string text)
		{
			var button = new ScalingButton()
			{
				Name = name,
				BackColor = Color.Transparent,
				FlatStyle = FlatStyle.Flat,
				Font = this.FontService.GetFontLight(12F, zoom),
				ForeColor = Theme.ButtonText,
				Margin = new Padding(Zoomed(3), 0, Zoomed(3), 0),
				Size = new Size(Zoomed(132), Zoomed(30)),
				SizeToGraphic = false,
				UseCustomGraphic = true,
				UseVisualStyleBackColor = false,
			};
			button.FlatAppearance.BorderSize = 0;
			button.FlatAppearance.MouseDownBackColor = Theme.TransparentButton;
			button.FlatAppearance.MouseOverBackColor = Theme.TransparentButton;
			// After the font: the button fits its text to its width.
			button.Text = text;
			return button;
		}

		this.buttonClear = NewButton(nameof(this.buttonClear), Resources.KeyBindsClear);
		this.buttonClear.Click += (_, _) => this.ClearArmed();
		this.buttonDefaults = NewButton(nameof(this.buttonDefaults), Resources.KeyBindsDefaults);
		this.buttonDefaults.Click += (_, _) => this.ResetAll();
		this.buttonOk = NewButton(nameof(this.buttonOk), Resources.GlobalOK);
		this.buttonOk.DialogResult = DialogResult.OK;
		this.buttonCancel = NewButton(nameof(this.buttonCancel), Resources.GlobalCancel);
		this.buttonCancel.DialogResult = DialogResult.Cancel;

		var buttons = new FlowLayoutPanel()
		{
			Anchor = AnchorStyles.None,
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			FlowDirection = FlowDirection.LeftToRight,
			Margin = new Padding(0, Zoomed(4), 0, 0),
			WrapContents = false,
		};
		buttons.Controls.AddRange([this.buttonClear, this.buttonDefaults, this.buttonOk, this.buttonCancel]);

		this.layout = new TableLayoutPanel()
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			ColumnCount = 1,
			Margin = new Padding(0),
		};
		this.layout.Controls.AddRange([this.listPanel, this.labelStatus, buttons]);
		this.layout.SizeChanged += (_, _) => this.FitToContent();
		CaravanFrame.FrameAround(this.listPanel);

		this.SuspendLayout();
		this.AutoScaleDimensions = new SizeF(96F, 96F);
		this.AutoScaleMode = AutoScaleMode.Dpi;
		this.BackColor = Color.FromArgb(46, 31, 21);
		this.CancelButton = this.buttonCancel;
		this.Font = font;
		this.ForeColor = Color.White;
		this.MaximizeBox = false;
		this.MinimizeBox = false;
		this.NormalizeBox = false;
		this.Name = nameof(KeyBindsDialog);
		this.ShowIcon = false;
		this.ShowInTaskbar = false;
		this.StartPosition = FormStartPosition.CenterParent;
		this.Text = Resources.KeyBindsTitle;
		var border = CustomBorderThickness;
		int gap = Zoomed(10);
		this.Padding = new Padding(border.Left + gap, border.Top + Zoomed(4), border.Right + gap, border.Bottom + Zoomed(6));
		this.layout.Location = new Point(this.Padding.Left, this.Padding.Top);
		this.Controls.Add(this.layout);
		this.DrawCustomBorder = true;
		CaravanLook.Apply(this, this.layout, buttons);
		this.ResumeLayout(false);
		this.PerformLayout();

		this.RefreshCells();
		this.FitToContent();
	}

	/// <summary>
	/// The list as wide as its rows and a vertical scroll bar at the window's DPI, the text under it wrapped at its width.
	/// </summary>
	private void FitList()
	{
		this.listPanel.Width = this.table.Width + SystemInformation.GetVerticalScrollBarWidthForDpi(this.DeviceDpi) + 2;
		this.labelStatus.MaximumSize = new Size(this.listPanel.Width + this.listPanel.Margin.Horizontal, 0);
	}

	/// <summary>
	/// The window around its table, as the settings dialog does: not Form.AutoSize, which kept the width the window had
	/// before the DPI scaling.
	/// </summary>
	private void FitToContent()
	{
		if (this.layout is null)
			return;

		this.ClientSize = new Size(this.layout.Right + this.Padding.Right, this.layout.Bottom + this.Padding.Bottom);
	}

	protected override void OnDpiChanged(DpiChangedEventArgs e)
	{
		base.OnDpiChanged(e);
		this.FitList();
		this.FitToContent();
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		this.FitList();
		this.FitToContent();
	}

	#region Capture

	/// <summary>
	/// A mouse button pressed on a shortcut cell: arms it; on the armed cell of a click action, a left click with its
	/// modifiers becomes the gesture; a right click, or a click on the armed cell of a key action, disarms it.
	/// </summary>
	private void CellMouseDown((KeyBindAction Action, int Slot) cell, MouseButtons button, Keys modifiers)
	{
		if (this.armed != cell)
		{
			if (button == MouseButtons.Left)
				this.Arm(cell);
			return;
		}

		if (button == MouseButtons.Left && KeyBindRegistry.Get(cell.Action).Kind == KeyBindKind.Click)
			this.SetArmed(KeyBindInput.Click(modifiers));
		else
			this.Disarm();
	}

	/// <summary>A key pressed while a key action is armed becomes its gesture; Escape disarms a click action.</summary>
	/// <returns>true when the key was taken.</returns>
	private bool CaptureKey(Keys keyData)
	{
		if (this.armed is not { } cell)
			return false;

		if (KeyBindRegistry.Get(cell.Action).Kind == KeyBindKind.Click)
		{
			if ((keyData & Keys.KeyCode) == Keys.Escape)
			{
				this.Disarm();
				return true;
			}

			return false;
		}

		// A modifier alone: wait for the key it goes with.
		var gesture = KeyBindInput.FromKeys(keyData);
		if (gesture.IsKey)
			this.SetArmed(gesture);

		return true;
	}

	/// <summary>Every key reaches the armed slot first, Tab, Enter, Escape and Alt combinations included.</summary>
	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		=> this.CaptureKey(keyData) || base.ProcessCmdKey(ref msg, keyData);

	private void Arm((KeyBindAction Action, int Slot) cell)
	{
		this.armed = cell;
		this.RefreshCells();
	}

	private void Disarm()
	{
		this.armed = null;
		this.RefreshCells();
	}

	private void SetArmed(KeyGesture gesture)
	{
		if (this.armed is { } cell)
			this.map.Set(cell.Action, cell.Slot, gesture);

		this.Disarm();
	}

	/// <summary>Empties the armed slot.</summary>
	private void ClearArmed() => this.SetArmed(KeyGesture.None);

	private void ResetAll()
	{
		this.map.ResetAll();
		this.Disarm();
	}

	#endregion

	/// <summary>Shows the gestures, the armed slot, the conflicts in red, and the conflicts or the hint under the list.</summary>
	private void RefreshCells()
	{
		foreach (var ((action, slot), cell) in this.cells)
		{
			var definition = KeyBindRegistry.Get(action);
			var gesture = this.map.Get(action, slot);
			bool isArmed = this.armed == (action, slot);

			cell.Text = isArmed
				? (definition.Kind == KeyBindKind.Click ? Resources.KeyBindsClickWithModifiers : Resources.KeyBindsPressKey)
				: gesture.IsNone ? (definition.Editable ? "-" : string.Empty) : KeyBindText.Gesture(gesture);
			cell.BackColor = !definition.Editable ? Color.Transparent : isArmed ? ArmedBack : CellBack;
			cell.ForeColor = isArmed ? TQColor.Aqua.Color()
				: this.map.HasConflict(action, slot) ? TQColor.Red.Color()
				: !definition.Editable ? TQColor.DarkGray.Color()
				: TQColor.Silver.Color();
		}

		var conflicts = this.map.Conflicts();
		this.labelStatus.ForeColor = conflicts.Count > 0 ? TQColor.Red.Color() : Color.White;
		this.labelStatus.Text = conflicts.Count > 0
			? string.Join(Environment.NewLine, conflicts.Select(KeyBindText.Conflict))
			: Resources.KeyBindsHint;
		this.buttonOk.Enabled = conflicts.Count == 0;
		this.buttonClear.Enabled = this.armed is not null;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
			this.toolTip.Dispose();

		base.Dispose(disposing);
	}
}
