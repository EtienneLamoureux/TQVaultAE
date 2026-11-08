namespace ArzExplorer.Components;

/// <summary>
/// Debug-only access to the sound player for the automation endpoint. Called on the UI thread.
/// </summary>
public partial class SimpleSoundPlayer
{
	/// <summary>What the player shows: file name, loop state, sound loaded.</summary>
	internal object AutomationState() => new
	{
		file = this.labelFileName.Text,
		loop = this.MustLoop,
		hasPlayer = this.CurrentSoundPlayer is not null,
		loaded = this.CurrentSoundPlayer?.IsLoadCompleted,
		wavBytes = this.CurrentSoundWavData?.Length,
		buttons = new
		{
			play = this.buttonPlay.Visible,
			loop = this.buttonLoop.Visible,
			pause = this.buttonPause.Visible,
			stop = this.buttonStop.Visible,
		},
	};

	/// <summary>
	/// Clicks a button (play, loop, pause, stop), then loads the sound, which reads and checks the wave data: a sound
	/// that cannot play throws here.
	/// </summary>
	internal object AutomationClick(string button)
	{
		var target = button.ToLowerInvariant() switch
		{
			"play" => this.buttonPlay,
			"loop" => this.buttonLoop,
			"pause" => this.buttonPause,
			"stop" => this.buttonStop,
			_ => throw new ArgumentException($"No button '{button}': play, loop, pause or stop.", nameof(button)),
		};
		target.PerformClick();
		this.CurrentSoundPlayer?.Load();
		return this.AutomationState();
	}
}
