using System;
using System.IO;
using TQVaultAE.Domain.Contracts.Services;

namespace TQVaultAE.Domain.Entities
{
	public class PlayerSave : IDisposable
	{
		public FileSystemWatcher PlayerSaveWatcher { get; set; }
		public FileSystemWatcher PlayerStashWatcher { get; set; }

		public readonly string Folder;
		public readonly string Name;
		public readonly bool IsCustom;
		public readonly string CustomMap;
		public readonly string CustomMapName;
		readonly ITranslationService Translate;
		public PlayerInfo Info;

		public PlayerSave(string folder, bool isCustom, string customMap, ITranslationService translate)
		{
			Folder = folder;
			// Copy the names over without the '_' and strip out the path information.
			Name = Path.GetFileName(folder).Substring(1);
			IsCustom = isCustom;
			CustomMap = customMap;
			CustomMapName = Path.GetFileName(customMap);
			Translate = translate;
		}

		public void Dispose()
		{
			if (PlayerSaveWatcher is not null)
				PlayerSaveWatcher.Dispose();
		}

		public override string ToString()
		{
			return string.Join(string.Empty, new[] {
				Info is null ? Name : $"{Name}"
				, Info?.Class is null ? string.Empty : $", {Translate.TranslateXTag(Info.Class)}"
				, !string.IsNullOrWhiteSpace(Info?.Class) && Info?.CurrentLevel != null ?  " -" : string.Empty
				, Info?.CurrentLevel is null ? string.Empty : $" {Translate.TranslateXTag("tagMenuImport05")} : {Info.CurrentLevel}"
				//, IsCustom ? $", IsCustom" : string.Empty // CustomMap is not specificaly related to this character
			});
		}
	}
}
