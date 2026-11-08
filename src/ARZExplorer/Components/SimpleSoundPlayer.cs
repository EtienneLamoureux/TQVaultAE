using System.Media;
using TQVaultAE.Domain.Entities;

namespace ArzExplorer.Components;

/// <summary>
/// Play, loop and stop buttons for the selected sound entry.
/// </summary>
public partial class SimpleSoundPlayer : UserControl
{
	private RecordId? _currentSoundId;
	private SoundPlayer? _currentSoundPlayer;

	/// <summary>
	/// Player of the shown sound; setting it stops the previous one and plays the new one.
	/// </summary>
	public SoundPlayer? CurrentSoundPlayer
	{
		get => this._currentSoundPlayer;
		set
		{
			this._currentSoundPlayer?.Stop();

			this._currentSoundPlayer = value;

			this.buttonPlay.PerformClick();
		}
	}

	/// <summary>
	/// Id of the shown sound; its file name is shown in the label.
	/// </summary>
	public RecordId? CurrentSoundId
	{
		get => this._currentSoundId;
		set
		{
			this._currentSoundId = value;

			if (this._currentSoundId is not null)
				this.labelFileName.Text = Path.GetFileName(this._currentSoundId);
		}
	}

	/// <summary>
	/// WAV data of the shown sound, for Copy Sound.
	/// </summary>
	public byte[]? CurrentSoundWavData { get; set; }

	/// <summary>
	/// Whether the loop button is down.
	/// </summary>
	public bool MustLoop => this.buttonLoop.BackColor == SystemColors.ActiveCaption;

	public SimpleSoundPlayer()
	{
		this.InitializeComponent();
	}

	private void SimpleSoundPlayer_Load(object sender, EventArgs e)
	{
		// Adjust UI
		this.buttonLoop.BackColor = SystemColors.Control;

		// TODO SoundPlayer doesn't have a Freeze/Resume
		this.buttonPause.Visible = false;
	}

	private void buttonPlay_Click(object sender, EventArgs e)
	{
		if (this.MustLoop)
			this.CurrentSoundPlayer?.PlayLooping();
		else
			this.CurrentSoundPlayer?.Play();
	}

	private void buttonLoop_Click(object sender, EventArgs e)
	{
		this.buttonLoop.BackColor = this.MustLoop
			? SystemColors.Control
			: SystemColors.ActiveCaption;
	}

	// SoundPlayer cannot pause: the hidden Pause button stops.
	private void buttonPause_Click(object sender, EventArgs e) => this.CurrentSoundPlayer?.Stop();

	private void buttonStop_Click(object sender, EventArgs e) => this.CurrentSoundPlayer?.Stop();
}
