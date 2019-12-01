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
		/// Return Class Name translation
		/// </summary>
		/// <param name="xTagClassKey"></param>
		/// <returns></returns>
		string TranslateCharacterClassName(string xTagClassKey);
		/// <summary>
		/// Gets the string which indicates an Immortal Throne item.
		/// </summary>
		string ItemIT { get; }
		/// <summary>
		/// Gets the string which indicates an Ragnarok item.
		/// </summary>
		string ItemRagnarok { get; }
		/// <summary>
		/// Gets the string which indicates an Atlantis item.
		/// </summary>
		string ItemAtlantis { get; }
	}
}