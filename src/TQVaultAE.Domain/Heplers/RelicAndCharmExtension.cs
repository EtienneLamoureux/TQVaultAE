using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Helpers
{
	public static class RelicAndCharmExtension
	{
		internal record RelicAndCharmMapItem(RelicAndCharm Value, string Name, string RecordId, string FileName, GearType Types);

		internal static ReadOnlyCollection<RelicAndCharmMapItem> RelicAndCharmMap =
		EnumsNET.Enums.GetValues<RelicAndCharm>()
		.Select(v => EnumsNET.Enums.GetMember(v))
		.Select(m =>
			new RelicAndCharmMapItem(
				m.Value,
				m.Name,
				RecordId: m.AsString(EnumsNET.EnumFormat.Description),
				FileName: Path.GetFileName(m.AsString(EnumsNET.EnumFormat.Description)),
				Types: m.Attributes.Get<GearTypeAttribute>().Type
			)
		).ToList().AsReadOnly();

		/// <summary>
		/// Gets the <see cref="GearType"/> for a <see cref="RelicAndCharm"/>
		/// </summary>
		/// <param name="relic"></param>
		/// <returns></returns>
		public static GearType GetGearType(this RelicAndCharm relic)
			=> RelicAndCharmMap.First(m => m.Value == relic).Types;

		/// <summary>
		/// Gets the recordId for a <see cref="RelicAndCharm"/>
		/// </summary>
		/// <param name="relic"></param>
		/// <returns></returns>
		public static string GetRecordId(this RelicAndCharm relic)
			=> RelicAndCharmMap.First(m => m.Value == relic).RecordId;

	}
}
