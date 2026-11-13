namespace TQVaultAE.GUI.Views;

/// <summary>Debug-only access to the character editor for the automation endpoint.</summary>
internal sealed partial class CharacterEditorUserControl
{
	/// <summary>
	/// The sums behind the computed statistics: the equipped items and sets, the skills that count (with the buffs
	/// option as set), their non-zero variables whose name contains <paramref name="filter"/>, and the racial bonuses;
	/// tab I's offense with its inputs (stance, each weapon in hand, the other items, the hand damage).
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
			skills = Values(this._skillSum),
			offense = this.ComputeOffense() is var (speed, values) ? new
			{
				attackSpeed = speed,
				values,
				stance = this._stance?.ToString(),
				female = this.Info.IsFemale,
				durations = this.Services.CharacterStatsService.GetAttackAnimationDurations(this.Info.IsFemale).ToDictionary(d => d.Key.ToString(), d => d.Value),
				weapons = this._offenseGear?.Weapons.Select(w => Values(w)).ToArray(),
				other = Values(this._offenseGear?.Other),
				hands = new { this.Services.CharacterStatsService.GetGameBalance().HandHitDamageMin, this.Services.CharacterStatsService.GetGameBalance().HandHitDamageMax },
			} : null,
		};
	}
}
