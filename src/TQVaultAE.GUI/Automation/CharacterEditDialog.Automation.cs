namespace TQVaultAE.GUI.Dialogs;

/// <summary>Debug-only access to the character editor for the automation endpoint.</summary>
internal sealed partial class CharacterEditDialog
{
	/// <summary>
	/// The sums behind the computed statistics: the equipped items and sets, the skills that count (with the buffs
	/// option as set), their non-zero variables whose name contains <paramref name="filter"/>, and the racial bonuses.
	/// </summary>
	internal object AutomationStatSources(string filter)
	{
		this.ComputeStats();
		object Values(Domain.Entities.AttributeSum? sum) => sum is null ? new { } : new
		{
			values = sum.Values
				.Where(v => string.IsNullOrEmpty(filter) || v.Key.Contains(filter, StringComparison.OrdinalIgnoreCase))
				.OrderBy(v => v.Key, StringComparer.OrdinalIgnoreCase)
				.ToDictionary(v => v.Key, v => v.Value),
			races = sum.Races,
		};

		return new
		{
			difficulty = this._statsDifficulty.SelectedIndex,
			includeBuffs = this._includeBuffs.Checked,
			gear = Values(this._gearSum),
			skills = Values(this._skillSum?.Sum),
		};
	}
}
