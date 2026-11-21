using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.GUI.Automation;

/// <summary>
/// Debug-only <see cref="ISoundService"/> decorator that records the sounds TQVault plays, for the <c>recent_sounds</c> tool.
/// </summary>
/// <remarks>
/// For a named sound it loads the sound data first (as the real service does), so a failed load or
/// MP3 to WAV conversion shows up in the record instead of passing silently.
/// </remarks>
internal sealed class RecordingSoundService : ISoundService
{
	/// <summary>The only MP3 TQVault plays, converted to WAV by NAudio.</summary>
	internal static readonly RecordId LevelUpSound = @"Sounds\UI\UI_LEVELUP.MP3".ToRecordId();

	internal sealed record PlayedSound(long Sequence, DateTime Time, string Call, string? Resource, bool Loaded, int? WavBytes, string? Error);

	private static readonly object Lock = new();
	private static readonly List<PlayedSound> Played = [];
	private static long _sequence;

	private readonly ISoundService _inner;
	private readonly UserSettings _settings;

	public RecordingSoundService(ISoundService inner, UserSettings settings)
	{
		this._inner = inner;
		this._settings = settings;
	}

	/// <summary>Gets the sequence number of the last recorded sound, to list what a tool call played.</summary>
	internal static long LastSequence
	{
		get
		{
			lock (Lock)
				return _sequence;
		}
	}

	internal static IReadOnlyList<PlayedSound> Since(long sequence)
	{
		lock (Lock)
			return Played.Where(p => p.Sequence > sequence).ToList();
	}

	internal static IReadOnlyList<PlayedSound> Last(int count)
	{
		lock (Lock)
			return Played.Skip(Math.Max(0, Played.Count - count)).ToList();
	}

	/// <summary>Gets a value indicating whether the user setting lets TQVault play sounds at all.</summary>
	internal bool SoundsEnabled => this._settings.EnableTQVaultSounds;

	public void PlaySound(RecordId resourceId)
	{
		this.RecordNamed(nameof(this.PlaySound), resourceId);
		this._inner.PlaySound(resourceId);
	}

	public void PlayLevelUp()
	{
		this.RecordNamed(nameof(this.PlayLevelUp), LevelUpSound);
		this._inner.PlayLevelUp();
	}

	public void PlayRandomItemDrop()
	{
		Record(nameof(this.PlayRandomItemDrop), null, true, null, null);
		this._inner.PlayRandomItemDrop();
	}

	public void PlayRandomRelicDrop()
	{
		Record(nameof(this.PlayRandomRelicDrop), null, true, null, null);
		this._inner.PlayRandomRelicDrop();
	}

	public void PlayRandomVoice()
	{
		Record(nameof(this.PlayRandomVoice), null, true, null, null);
		this._inner.PlayRandomVoice();
	}

	public void PlayRandomCancel()
	{
		Record(nameof(this.PlayRandomCancel), null, true, null, null);
		this._inner.PlayRandomCancel();
	}

	public void PlayRandomMetalHit()
	{
		Record(nameof(this.PlayRandomMetalHit), null, true, null, null);
		this._inner.PlayRandomMetalHit();
	}

	public bool SetSoundResource(RecordId resourceId, byte[] resourceData) => this._inner.SetSoundResource(resourceId, resourceData);

	public void InitAllPlayers() => this._inner.InitAllPlayers();

	public byte[]? GetSoundResource(RecordId resourceId) => this._inner.GetSoundResource(resourceId);

	public void ConvertMp3ToWav(Stream inMp3, Stream outWav) => this._inner.ConvertMp3ToWav(inMp3, outWav);

	private void RecordNamed(string call, RecordId resourceId)
	{
		try
		{
			// Loads and caches the WAV data (converting an MP3 with NAudio), as the real service does before playing.
			var data = this._inner.GetSoundResource(resourceId);
			Record(call, resourceId.ToString(), data is not null, data?.Length, data is null ? "Sound not found in the game files." : null);
		}
		catch (Exception ex)
		{
			Record(call, resourceId.ToString(), false, null, $"{ex.GetType().Name}: {ex.Message}");
		}
	}

	private static void Record(string call, string? resource, bool loaded, int? wavBytes, string? error)
	{
		lock (Lock)
		{
			Played.Add(new PlayedSound(++_sequence, DateTime.Now, call, resource, loaded, wavBytes, error));
			if (Played.Count > 200)
				Played.RemoveAt(0);
		}
	}
}
