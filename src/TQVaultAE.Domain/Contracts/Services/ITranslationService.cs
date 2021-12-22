using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Contracts.Services
{
	public interface ITranslationService
	{
		/// <summary>
		/// Gets the string name of a particular item style
		/// </summary>
		/// <param name="itemStyle">ItemStyle enumeration</param>
		/// <returns>Localized string of the item style</returns>
		string Translate(ItemStyle itemStyle);
		/// <summary>
		/// Gets the string used for 'with'
		/// </summary>
		string ItemWith { get; }
		/// <summary>
		/// Gets the relic completion bonus string.
		/// </summary>
		string ItemRelicBonus { get; }
		/// <summary>
		/// Gets the relic completed string.
		/// </summary>
		string ItemRelicCompleted { get; }
		/// <summary>
		/// Gets the quest item indicator string.
		/// </summary>
		string ItemQuest { get; }
		/// <summary>
		/// Gets the item seed format string.
		/// </summary>
		string ItemSeed { get; }
		/// <summary>
		/// Return Difficulty translation
		/// </summary>
		/// <param name="difficultyFromSaveFile"></param>
		/// <returns></returns>
		string TranslateDifficulty(int difficultyFromSaveFile);
		/// <summary>
		/// Gets the string which indicates an Immortal Throne item.
		/// </summary>
		string ItemIT { get; }
		/// <summary>
		/// Gets the string which indicates an Ragnarok item.
		/// </summary>
		string ItemRagnarok { get; }
		/// <summary>
		/// Gets the string which indicates an Eternal Embers item.
		/// </summary>
		string ItemEmbers { get; }
		/// <summary>
		/// Gets the string which indicates an Atlantis item.
		/// </summary>
		string ItemAtlantis { get; }
		/// <summary>
		/// Translate <paramref name="xTagName"/> using resource file and database
		/// </summary>
		/// <param name="xTagName"></param>
		/// <returns></returns>
		string TranslateXTag(string xTagName);
		/// <summary>
		/// Translate character class to mastery
		/// </summary>
		/// <param name="characterXtagClass"></param>
		/// <returns></returns>
		string TranslateMastery(string characterXtagClass);
		/// <summary>
		/// Try translate <paramref name="xTagName"/> using resource file and database
		/// </summary>
		/// <param name="xTagName"></param>
		/// <param name="translation"></param>
		/// <returns></returns>
		bool TryTranslateXTag(string xTagName, out string translation);
	}
}