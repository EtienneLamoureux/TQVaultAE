using System.IO;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Contracts.Services
{
	public interface ISoundService
	{
		/// <summary>
		/// Reload all player according to configuration
		/// </summary>
		void InitAllPlayers();
		/// <summary>
		/// Load and Play game sound identified by <paramref name="resourceId"/>
		/// </summary>
		/// <param name="resourceId"></param>
		void PlaySound(RecordId resourceId);
		/// <summary>
		/// Get game sound WAV data identified by <paramref name="resourceId"/>
		/// </summary>
		/// <param name="resourceId"></param>
		/// <returns>Wav file content</returns>
		byte[] GetSoundResource(RecordId resourceId);
		/// <summary>
		/// Play a random predefined "Item Drop" sound
		/// </summary>
		void PlayRandomItemDrop();
		/// <summary>
		/// Play a random predefined "Relic Drop" sound
		/// </summary>
		void PlayRandomRelicDrop();
		/// <summary>
		/// Play a random predefined "Voice" sound
		/// </summary>
		void PlayRandomVoice();
		/// <summary>
		/// Play a random predefined "Cancel" sound
		/// </summary>
		void PlayRandomCancel();
		/// <summary>
		/// Play a random predefined "Metal Hit" sound
		/// </summary>
		void PlayRandomMetalHit();
		/// <summary>
		/// Play in game "Level Up" sound
		/// </summary>
		void PlayLevelUp();
		/// <summary>
		/// Helper
		/// </summary>
		/// <param name="inMp3"></param>
		/// <param name="outWav"></param>
		void ConvertMp3ToWav(Stream inMp3, Stream outWav);
	}
}
