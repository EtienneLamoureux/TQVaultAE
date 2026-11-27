using TQVaultAE.Domain.Helpers;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Components.Rendering;

/// <summary>
/// A small button drawn with one of the game's button pictures, in its up, over, down and disable states; its action
/// is raised from the mouse up (a synthetic click from the automation endpoint works too). Greyed, it stays enabled
/// so that its tool tip can tell why (a disabled control gets no mouse messages).
/// </summary>
internal sealed class GameButton : Control
{
	private readonly IUIService _uiService;
	private readonly string _resource;
	private readonly bool _hasDisable;
	private readonly string _restState;
	private readonly string _hoverState;
	private readonly ToolTip _toolTip = new();
	private readonly string _text;
	private bool _hovered;
	private bool _pressed;
	private bool _available = true;
	private string? _lockedText;

	/// <summary>Raised when the left button is released over the button it went down on.</summary>
	internal event EventHandler? Pressed;

	/// <param name="uiService">The UI service, which loads the pictures.</param>
	/// <param name="resource">The pictures' id, with {0} for the state: up, over, down or disable.</param>
	/// <param name="text">The tool tip and accessible name.</param>
	/// <param name="hasDisable">False when the game has no disable picture: the rest picture is greyed instead.</param>
	/// <param name="restState">The state whose picture shows at rest.</param>
	/// <param name="hoverState">The state whose picture shows under the mouse.</param>
	internal GameButton(IUIService uiService, string resource, string text, bool hasDisable = true, string restState = "up", string hoverState = "over")
	{
		this._uiService = uiService;
		this._resource = resource;
		this._hasDisable = hasDisable;
		this._restState = restState;
		this._hoverState = hoverState;
		this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
		this.BackColor = Color.Transparent;
		this.Cursor = Cursors.Hand;
		this._text = text;
		this.AccessibleName = text;
		this._toolTip.SetToolTip(this, text);
	}

	/// <summary>Whether a click raises the action; greyed otherwise.</summary>
	internal bool Available
	{
		get => this._available;
		set
		{
			if (value == this._available)
				return;

			this._available = value;
			this.Cursor = value ? Cursors.Hand : Cursors.Default;
			this._pressed = false;
			this.UpdateToolTip();
			this.Invalidate();
		}
	}

	/// <summary>The tool tip while greyed (why), or null to keep the button's text.</summary>
	internal string? LockedText
	{
		get => this._lockedText;
		set
		{
			this._lockedText = value;
			this.UpdateToolTip();
		}
	}

	private void UpdateToolTip()
		=> this._toolTip.SetToolTip(this, !this._available && this._lockedText is { } locked ? locked : this._text);

	protected override void OnMouseEnter(EventArgs e)
	{
		base.OnMouseEnter(e);
		this._hovered = true;
		this.Invalidate();
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		this._hovered = false;
		this._pressed = false;
		this.Invalidate();
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		if (e.Button != MouseButtons.Left)
			return;

		this._pressed = true;
		this.Invalidate();
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		bool clicked = this._pressed && this._available && e.Button == MouseButtons.Left && this.ClientRectangle.Contains(e.Location);
		this._pressed = false;
		this.Invalidate();
		if (clicked)
			this.Pressed?.Invoke(this, EventArgs.Empty);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		bool greyed = !this._available && !this._hasDisable;
		string state = !this._available ? (greyed ? this._restState : "disable") : this._pressed ? "down" : this._hovered ? this._hoverState : this._restState;
		string resource = string.Format(System.Globalization.CultureInfo.InvariantCulture, this._resource, state);
		e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
		GamePicture.Draw(e.Graphics, this._uiService.LoadBitmap(resource.ToRecordId()), this.ClientRectangle, greyed);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
			this._toolTip.Dispose();

		base.Dispose(disposing);
	}
}
