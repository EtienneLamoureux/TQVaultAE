using EnumsNET;
using Microsoft.Extensions.Logging;
using System.Linq;
using TQVaultAE.Domain.Contracts.Providers;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Presentation
{
	public class TranslationService : ITranslationService
	{
		private readonly IDatabase Database;

		public TranslationService(IDatabase database)
		{
			Database = database;
		}

		/// <summary>
		/// Gets the string name of a particular item style
		/// </summary>
		/// <param name="itemStyle">ItemStyle enumeration</param>
		/// <returns>Localized string of the item style</returns>
		public string Translate(ItemStyle itemStyle)
		{
			var tags = itemStyle.AsString(EnumFormat.Description);
			var values = tags.Split('|');
			var trans = (values.Length > 1)
				? string.Format(values.Last(), this.TranslateXTag(values.First()))
				: this.TranslateXTag(values.First());
			return trans;
		}

		/// <summary>
		/// Gets the string used for 'with'
		/// </summary>
		public string ItemWith => Resources.ItemWith;
		/// <summary>
		/// Gets the relic completion bonus string.
		/// </summary>
		public string ItemRelicBonus => Resources.ItemRelicBonus;
		/// <summary>
		/// Gets the relic completed string.
		/// </summary>
		public string ItemRelicCompleted => Resources.ItemRelicCompleted;
		/// <summary>
		/// Gets the quest item indicator string.
		/// </summary>
		public string ItemQuest => Resources.ItemQuest;
		/// <summary>
		/// Gets the item seed format string.
		/// </summary>
		public string ItemSeed => Resources.ItemSeed;
		/// <summary>
		/// Translate <paramref name="xTagName"/> using resource file and database
		/// </summary>
		/// <param name="xTagName"></param>
		/// <returns></returns>
		public string TranslateXTag(string xTagName)
		{
			// all xtag substitute must have a "TextTag_" prefix in resource file (avoid colision & strong naming rule).
			var resx = Resources.ResourceManager.GetString($"TextTag_{xTagName}");

			if (resx is null)
				resx = this.Database.GetFriendlyName(xTagName);// Try DB

			return string.IsNullOrWhiteSpace(resx) ? $"{TQColor.Purple.ColorTag()}??{xTagName}??" : resx;
		}

		/// <summary>
		/// Return Difficulty translation
		/// </summary>
		/// <param name="difficultyFromSaveFile"></param>
		/// <returns></returns>
		public string TranslateDifficulty(int difficultyFromSaveFile)
			=> this.Database.GetFriendlyName($"tagRDifficultyTitle0{++difficultyFromSaveFile}");

		/// <summary>
		/// Translate character class to mastery
		/// </summary>
		/// <param name="characterXtagClass"></param>
		/// <returns></returns>
		public string TranslateMastery(string characterXtagClass)
		{
			var tags = Resources.ResourceManager.GetString($"Masteries{characterXtagClass}");
			var dualclass = tags.Split('-');
			return dualclass.Count() > 1
				? $"{TranslateXTag(dualclass.First())}-{TranslateXTag(dualclass.Last())}"
				: TranslateXTag(dualclass.First());
		}

		/// <summary>
		/// Gets the string which indicates an Immortal Throne item.
		/// </summary>
		public string ItemIT => Resources.ItemIT;
		/// <summary>
		/// Gets the string which indicates an Ragnarok item.
		/// </summary>
		public string ItemRagnarok => Resources.ItemRagnarok;
		/// <summary>
		/// Gets the string which indicates an Atlantis item.
		/// </summary>
		public string ItemAtlantis => Resources.ItemAtlantis;
	}
}
