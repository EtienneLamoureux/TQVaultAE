using TQVaultAE.GUI.Components.Panels;

namespace TQVaultAE.GUI.Components.Character;

/// <summary>
/// The room right of the equipment slots, on the black field of the equipment background: the statistics panel, or the
/// loadout panel in its place, with the loadout bar under them and the Edit button under the bar. The room lays its parts
/// out itself: the statistics at their own size with the bar and the Edit button under them; the loadout panel takes the
/// whole room above the bar, which goes down to the line of the Edit button, hidden meanwhile.
/// </summary>
internal sealed class CharacterRoom : UserControl
{
	private bool _loadoutShown;
	private bool _editAllowed;

	/// <summary>Where the statistics panel sits in the room, at the scale of the last layout.</summary>
	private Point _statsLocation;

	/// <summary>The size of the Edit button, at the scale of the last layout.</summary>
	private Size _editSize;

	/// <summary>The gap between the parts, at the scale of the last layout.</summary>
	private int _gap;

	/// <summary>Initializes a new instance of the CharacterRoom class.</summary>
	/// <param name="stats">The statistics panel.</param>
	/// <param name="loadout">The loadout panel, with its Back and previous / next buttons.</param>
	/// <param name="bar">The loadout bar.</param>
	/// <param name="editButton">The Edit button of the character editor.</param>
	public CharacterRoom(CharacterStatsPanel stats, LoadoutView loadout, LoadoutBar bar, Control editButton)
	{
		this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
		this.BackColor = Color.Black;
		this.Name = "characterRoom";
		this.AutoScaleMode = AutoScaleMode.None;
		this.Stats = stats;
		this.Loadout = loadout;
		this.Bar = bar;
		this.EditButton = editButton;

		stats.Visible = true;
		loadout.Visible = false;
		bar.Visible = true;
		editButton.Visible = false;
		this.Controls.AddRange([stats, loadout, editButton, bar]);
	}

	/// <summary>The statistics panel.</summary>
	public CharacterStatsPanel Stats { get; }

	/// <summary>The loadout panel.</summary>
	public LoadoutView Loadout { get; }

	/// <summary>The loadout bar.</summary>
	public LoadoutBar Bar { get; }

	/// <summary>The Edit button of the character editor.</summary>
	public Control EditButton { get; }

	/// <summary>Whether the loadout panel shows, in place of the statistics and the Edit button.</summary>
	public bool LoadoutShown
	{
		get => this._loadoutShown;
		set
		{
			if (this._loadoutShown == value)
				return;

			this._loadoutShown = value;
			this.Arrange();
		}
	}

	/// <summary>Whether the Edit button shows with the statistics (the setting that allows editing characters).</summary>
	public bool EditAllowed
	{
		get => this._editAllowed;
		set
		{
			if (this._editAllowed == value)
				return;

			this._editAllowed = value;
			this.Arrange();
		}
	}

	/// <summary>Lays the parts out at a scale.</summary>
	/// <param name="statsLocation">Where the statistics panel sits in the room (the room goes up to the top of the field).</param>
	/// <param name="editSize">The size of the Edit button.</param>
	/// <param name="gap">The gap between the parts.</param>
	public void Arrange(Point statsLocation, Size editSize, int gap)
	{
		this._statsLocation = statsLocation;
		this._editSize = editSize;
		this._gap = gap;
		this.Arrange();
	}

	/// <summary>
	/// The stash panel places the room and the room its parts, from the scale (<see cref="Arrange(Point, Size, int)"/>):
	/// scaled by Windows Forms from their sizes before, a DPI change scaled them again after that layout (the statistics
	/// panel, the bar and the loadout panel shrank at each lower DPI).
	/// </summary>
	protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
	{
	}

	/// <inheritdoc cref="ScaleControl"/>
	protected override bool ScaleChildren => false;

	/// <summary>The room was resized: its parts follow.</summary>
	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		this.Arrange();
	}

	private void Arrange()
	{
		var stats = new Rectangle(this._statsLocation, this.Stats.Size);
		var bar = new Rectangle(stats.X, stats.Bottom + this._gap, stats.Width, LoadoutBar.HeightAt(this.Bar.BarScale));
		var edit = new Rectangle(new Point(stats.X, bar.Bottom + this._gap), this._editSize);

		// The loadout panel: the bar goes down to the line of the Edit button, the panel takes all the room above it.
		if (this._loadoutShown)
			bar.Y = edit.Bottom - bar.Height;

		SetBounds(this.Stats, stats);
		SetBounds(this.Bar, bar);
		SetBounds(this.EditButton, edit);
		SetBounds(this.Loadout, new Rectangle(0, 0, this.Width, Math.Max(0, bar.Top - this._gap)));

		this.Stats.Visible = !this._loadoutShown;
		this.EditButton.Visible = this._editAllowed && !this._loadoutShown;
		this.Loadout.Visible = this._loadoutShown;
	}

	private static void SetBounds(Control control, Rectangle bounds)
	{
		if (control.Bounds != bounds)
			control.Bounds = bounds;
	}
}
