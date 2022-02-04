using EnumsNET;
using System.Linq;
using TQVaultAE.Domain.Contracts.Providers;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Presentation
{
	public class TranslationService : ITranslationService
	{
		private readonly IDatabase Database;

		public TranslationService(IDatabase database)
		{
			Database = database;
		}

		public string Translate(ItemStyle itemStyle)
		{
			var tags = itemStyle.AsString(EnumFormat.Description);
			var values = tags.Split('|');
			var trans = (values.Length > 1)
				? string.Format(values.Last(), this.TranslateXTag(values.First()))
				: this.TranslateXTag(values.First());
			return trans;
		}

		public string ItemWith => Resources.ItemWith;

		public string ItemRelicBonus => Resources.ItemRelicBonus;

		public string ItemRelicCompleted => Resources.ItemRelicCompleted;

		public string ItemQuest => Resources.ItemQuest;

		public string ItemSeed => Resources.ItemSeed;

		public string TranslateXTag(string xTagName)
		{
			string resx = LookFortranslation(xTagName);

			return string.IsNullOrWhiteSpace(resx) ? xTagName : resx;
		}

		public bool TryTranslateXTag(string xTagName, out string translation)
		{
			string resx = LookFortranslation(xTagName);

			if (string.IsNullOrWhiteSpace(resx))
			{
				translation = null;
				return false;
			}

			translation = resx;
			return true;
		}

		private string LookFortranslation(string xTagName)
		{
			// all xtag substitute must have a "TextTag_" prefix in resource file (avoid colision & strong naming rule).
			var resx = Resources.ResourceManager.GetString($"TextTag_{xTagName}");

			// Check if the value is a @redirectTag 
			if (resx != null && resx.StartsWith("@"))
				resx = this.Database.GetFriendlyName(resx.TrimStart('@'));

			if (resx is null)
				resx = this.Database.GetFriendlyName(xTagName);
			return resx;
		}

		public string TranslateDifficulty(int difficultyFromSaveFile)
			=> this.Database.GetFriendlyName($"tagRDifficultyTitle0{++difficultyFromSaveFile}");

		public string TranslateMastery(string characterXtagClass)
		{
			var tags = Resources.ResourceManager.GetString($"Masteries{characterXtagClass}");
			var dualclass = tags.Split('-');
			return dualclass.Count() > 1
				? $"{TranslateXTag(dualclass.First())}-{TranslateXTag(dualclass.Last())}"
				: TranslateXTag(dualclass.First());
		}

		public string ItemIT => Resources.ItemIT;

		public string ItemRagnarok => Resources.ItemRagnarok;

		public string ItemAtlantis => Resources.ItemAtlantis;

		public string ItemEmbers => Resources.ItemEmbers;
	}
}
