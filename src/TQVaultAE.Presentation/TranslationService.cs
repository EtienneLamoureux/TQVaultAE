using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Presentation
{
	public class TranslationService : ITranslationService
	{

		/// <summary>
		/// Gets the string name of a particular item style
		/// </summary>
		/// <param name="itemStyle">ItemStyle enumeration</param>
		/// <returns>Localized string of the item style</returns>
		public string Translate(ItemStyle itemStyle) => Resources.ResourceManager.GetString($"ItemStyle{itemStyle}");
		/// <summary>
		/// Gets the string used for 'with'
		/// </summary>
		public string ItemWith => Resources.ItemWith ?? "with";
		/// <summary>
		/// Gets the relic completion bonus string.
		/// </summary>
		public string ItemRelicBonus => Resources.ItemRelicBonus ?? "(Completion Bonus: {0})";
		/// <summary>
		/// Gets the relic completed string.
		/// </summary>
		public string ItemRelicCompleted => Resources.ItemRelicCompleted ?? "(Completed)";
		/// <summary>
		/// Gets the quest item indicator string.
		/// </summary>
		public string ItemQuest => Resources.ItemQuest ?? "(Quest Item)";
		/// <summary>
		/// Gets the item seed format string.
		/// </summary>
		public string ItemSeed => Resources.ItemSeed ?? "ItemSeed: {0} (0x{0:X8}) ({1:p3})";
		/// <summary>
		/// Return Class Name translation
		/// </summary>
		/// <param name="xTagClassKey"></param>
		/// <returns></returns>
		public string TranslateCharacterClassName(string xTagClassKey) => Resources.ResourceManager.GetString(xTagClassKey);
		/// <summary>
		/// Gets the string which indicates an Immortal Throne item.
		/// </summary>
		public string ItemIT => Resources.ItemIT ?? "Immortal Throne Item";
		/// <summary>
		/// Gets the string which indicates an Ragnarok item.
		/// </summary>
		public string ItemRagnarok => Resources.ItemRagnarok ?? "Ragnarok Item";
		/// <summary>
		/// Gets the string which indicates an Atlantis item.
		/// </summary>
		public string ItemAtlantis => Resources.ItemAtlantis ?? "Atlantis Item";
	}
}
