using Microsoft.Extensions.Logging;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Contracts.Providers;
using System.Linq;
using System.Media;
using System.IO;
using System;
using System.Collections.Generic;
using NAudio.Wave;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Services.Win32
{
	/// <summary>
	/// Win32 inplementation of <see cref="ISoundService"/> 
	/// </summary>
	public class SoundServiceWin : ISoundService, IDisposable
	{
		private readonly ILogger Log;
		private readonly IDatabase DataBase;

		#region Predefined sounds

		private static readonly RecordId[] SoundPoolMetalHitIds = new[] {
			Enumerable.Range(1, 3).Select(i => $@"Sounds\ARMOR\ARMORHITPLATE0{i}.WAV"),
			Enumerable.Range(1, 3).Select(i => $@"Sounds\SPELLS\DEFENSE\SHIELDBASHMETAL0{i}.WAV"),
			Enumerable.Range(1, 3).Select(i => $@"Sounds\ARMOR\SHIELDBLOCKMETAL0{i}.WAV"),
			Enumerable.Range(1, 3).Select(i => $@"Sounds\SPELLS\DEFENSE\SHIELDCHARGEHIT0{i}.WAV"),
		}.SelectMany(a => a).Select(r => r.ToRecordId()).ToArray();

		private static readonly RecordId[] SoundPoolItemDropIds = new[] {
			@"Sounds\UI\UI_BAGCHANGE.WAV",
			@"Sounds\UI\UI_GENERICCLICKBIG.WAV",
			@"Sounds\UI\UI_GENERICCLICKSMALL.WAV",
		}.Select(r => r.ToRecordId()).ToArray();

		private static readonly RecordId[] SoundPoolRelicDropIds = new[] {
			@"Sounds\UI\UI_GEMCLICK.WAV",
			@"Sounds\UI\UI_RELICSTACK.WAV",
			@"Sounds\UI\UI_RELICCOMPLETE.WAV",
			@"Sounds\UI\UI_RELICBINDTOITEM.WAV",
		}.Select(r => r.ToRecordId()).ToArray();

		private static readonly RecordId[] SoundPoolVoiceIds = Enumerable.Range(1, 3)
			.Select(i => $@"Sounds\MONSTERS\GREECE\G_TELKINE\TELEKINEVOICE0{i}.WAV")
			.Concat(new[] { @"Sounds\AMBIENCE\RANDOMEVENT\TYPHONLAUGHDISTANCE.WAV" })
			.Select(r => r.ToRecordId()).ToArray();

		private static readonly RecordId[] SoundPoolCancelIds = new[] {
			//@"Sounds\UI\UI_MOUSEOVERSTONE.WAV",
			@"Sounds\UI\UI_SKILLBUYBACK.WAV",
			@"Sounds\UI\UI_ERRORMESSAGE.WAV",
			@"Sounds\WEAPONS\UNARMEDHIT01.WAV",
			@"Sounds\UI\UI_HOTSLOTRIGHTCLICK.WAV",
			//@"Sounds\UI\UI_STONECLICK.WAV",
			//@"Sounds\UI\UI_WEAPONSWAP.WAV",
		}.Select(r => r.ToRecordId()).ToArray();

		private static readonly RecordId SoundLevelUp = @"Sounds\UI\UI_LEVELUP.MP3";

		private static SoundPlayer[] SoundPoolMetalHit;
		private static SoundPlayer[] SoundPoolItemDrop;
		private static SoundPlayer[] SoundPoolRelicDrop;
		private static SoundPlayer[] SoundPoolVoice;
		private static SoundPlayer[] SoundPoolCancel;

		#endregion

		// Cache
		private static Dictionary<RecordId, byte[]> SoundData = new();
		private static Dictionary<RecordId, (SoundPlayer Player, MemoryStream MS)> LoadedPlayers = new();

		public SoundServiceWin(ILogger<SoundServiceWin> log, IDatabase database)
		{
			this.Log = log;
			this.DataBase = database;

			InitAllPlayers();
		}

		public void InitAllPlayers()
		{
			LoadedPlayers.Clear();

			if (this.DataBase is null)
				return;

			SoundPoolMetalHit = InitPlayers(SoundPoolMetalHitIds);
			SoundPoolItemDrop = InitPlayers(SoundPoolItemDropIds);
			SoundPoolRelicDrop = InitPlayers(SoundPoolRelicDropIds);
			SoundPoolVoice = InitPlayers(SoundPoolVoiceIds);
			SoundPoolCancel = InitPlayers(SoundPoolCancelIds);
		}

		private SoundPlayer[] InitPlayers(RecordId[] list)
			=> list.Select(id => GetSoundPlayer(id)).Where(p => p is not null).ToArray();

		public void ConvertMp3ToWav(Stream inMp3, Stream outWav)
		{
			// Inspired from https://stackoverflow.com/questions/11446096/converting-mp3-data-to-wav-data-c-sharp
			using var mp3 = new Mp3FileReader(inMp3);
			using var pcm = WaveFormatConversionStream.CreatePcmStream(mp3);
			WaveFileWriter.WriteWavFileToStream(outWav, pcm);
		}

		public bool SetSoundResource(RecordId resourceId, byte[] resourceData)
		{
			if (!IsSoundRecordId(resourceId))
				return false;// Not a sound

			if (!(resourceData?.Any() ?? false))
				return false;// Not a sound

			if (resourceId.Normalized.EndsWith(".MP3"))
			{
				using (var mp3 = new MemoryStream(resourceData))
				using (var wav = new MemoryStream())
				{
					ConvertMp3ToWav(mp3, wav);
					resourceData = wav.ToArray();
				}
			}

			SoundData[resourceId] = resourceData;
			return true;
		}

		private static bool IsSoundRecordId(RecordId resourceId)
		{
			return resourceId.Normalized.Contains(".MP3") || resourceId.Normalized.Contains(".WAV");
		}

		public byte[] GetSoundResource(RecordId resourceId)
		{
			if (!IsSoundRecordId(resourceId))
				return null;// Not a sound

			if (SoundData.TryGetValue(resourceId, out var data))
				return data;

			if (DataBase is null)
				return null;// No DB

			data = DataBase.LoadResource(resourceId);

			if (data is not null && this.SetSoundResource(resourceId, data))
				return SoundData[resourceId];

			return null;
		}

		public SoundPlayer GetSoundPlayer(RecordId resourceId)
		{
			if (!Config.UserSettings.Default.EnableTQVaultSounds)
				return null;

			if (!IsSoundRecordId(resourceId))
				return null;// Not a sound

			if (LoadedPlayers.TryGetValue(resourceId, out var playerInstance))
				return playerInstance.Player;

			var data = this.GetSoundResource(resourceId);

			if (data is not null)
			{
				playerInstance.MS = new MemoryStream(data);
				playerInstance.Player = new SoundPlayer(playerInstance.MS);
				LoadedPlayers.Add(resourceId, playerInstance);
			}

			return playerInstance.Player;
		}

		private static SoundPlayer GetRandomPlayer(SoundPlayer[] pool)
		{
			if (!Config.UserSettings.Default.EnableTQVaultSounds)
				return null;

			var rand = new Random(DateTime.Now.Millisecond);
			var multi = (pool.Length * 100) - 1;
			var playerIdx = rand.Next(0, multi);
			playerIdx = playerIdx / 100;
			return pool[playerIdx];
		}

		public void PlaySound(RecordId resourceId)
			=> this.GetSoundPlayer(resourceId)?.Play();
		public void PlayLevelUp()
			=> this.GetSoundPlayer(SoundLevelUp)?.Play();
		public void PlayRandomCancel()
			=> GetRandomPlayer(SoundPoolCancel)?.Play();
		public void PlayRandomItemDrop()
			=> GetRandomPlayer(SoundPoolItemDrop)?.Play();
		public void PlayRandomMetalHit()
			=> GetRandomPlayer(SoundPoolMetalHit)?.Play();
		public void PlayRandomRelicDrop()
			=> GetRandomPlayer(SoundPoolRelicDrop)?.Play();
		public void PlayRandomVoice()
			=> GetRandomPlayer(SoundPoolVoice)?.Play();

		#region IDisposable

		private bool _disposed;

		/// <summary>
		/// I created it! i Dispose of it!
		/// </summary>
		/// <remarks>This class is handled by DI so it will be called by DI</remarks>
		public void Dispose()
		{
			// Dispose of unmanaged resources.
			Dispose(true);
			// Suppress finalization.
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				// dispose managed state (managed objects).
				// Cleanup SoundPlayers
				foreach (var player in LoadedPlayers)
				{
					var val = player.Value;
					val.Player?.Dispose();
					val.MS?.Dispose();
					val.Player = null;
					val.MS = null;
				}
			}

			// free unmanaged resources (unmanaged objects) and override a finalizer below.
			// set large fields to null.
			SoundData = null;
			LoadedPlayers = null;
			SoundPoolCancel = null;
			SoundPoolItemDrop = null;
			SoundPoolMetalHit = null;
			SoundPoolRelicDrop = null;
			SoundPoolVoice = null;

			_disposed = true;
		}

		#endregion
	}
}