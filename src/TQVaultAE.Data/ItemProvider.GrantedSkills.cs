using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Data;

public partial class ItemProvider
{
	/// <inheritdoc/>
	public IReadOnlyList<(RecordId SkillId, int Level)> GetGrantedSkillLevels(Item itm)
	{
		using var modScope = this.ModScope(itm);
		return [.. this.GetGrantedSkillSources(itm).Select(s => (s.SkillId, s.Level))];
	}

	/// <inheritdoc/>
	public IEnumerable<(DBRecordCollection Source, RecordId SkillId, int Level)> GetGrantedSkillSources(Item itm)
	{
		foreach (var sourceId in itm.RecordIds())
		{
			if (Database.GetRecordFromFile(sourceId) is not { } source)
				continue;

			string skillName = source.GetString("itemSkillName", 0);
			if (!string.IsNullOrEmpty(skillName))
				yield return (source, skillName.ToRecordId(), GrantedLevel(source));
		}
	}

	/// <summary>The level a record grants its skill at (<c>itemSkillLevel</c>), at least 1.</summary>
	private static int GrantedLevel(DBRecordCollection source) => Math.Max(source.GetInt32("itemSkillLevel", 0), 1);
}
