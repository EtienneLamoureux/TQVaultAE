using System.Globalization;
using TQVaultAE.Application.Character;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Presentation;

/// <summary>Texts of the class sets: the tool tip of the mark telling that the build shown differs from the selected set.</summary>
public static class ClassSetText
{
	/// <summary>
	/// The mark's tool tip: where the change comes from (the game, the editor, or a set applied in part), the points each mastery and each
	/// attribute has more or less than the set, then what to do.
	/// </summary>
	/// <param name="status">How the build shown differs from the set.</param>
	/// <param name="masteryName">The name of a mastery, in the game's language.</param>
	public static string StatusToolTip(ClassSetStatus status, Func<Masteries, string> masteryName)
	{
		var culture = CultureInfo.CurrentCulture;
		static string Signed(int value) => value.ToString("+0;-0", CultureInfo.CurrentCulture);

		var lines = new List<string>
		{
			status.Change switch
			{
				ClassSetChange.InGame => Resources.ClassSetChangedInGame,
				ClassSetChange.AppliedInPart => Resources.ClassSetAppliedInPart,
				_ => Resources.ClassSetChangedInEditor,
			},
		};
		lines.AddRange(status.Difference.SkillPoints
			.Where(d => d.Points != 0)
			.Select(d => string.Format(culture, Resources.ClassSetSkillPointsDelta, masteryName(d.Mastery), Signed(d.Points))));

		string[] attributes = [Resources.CEHealth, Resources.CEMana, Resources.CEStrength, Resources.CEIntelligence, Resources.CEDexterity];
		lines.AddRange(status.Difference.Attributes.ToArray()
			.Select((points, i) => (points, name: attributes[i]))
			.Where(a => a.points != 0)
			.Select(a => string.Format(culture, Resources.ClassSetAttributePointsDelta, a.name, Signed(a.points))));

		// Points moved between skills of a mastery leave its total as it was: the first line alone says the build differs.
		lines.Add(Resources.ClassSetStatusHint);
		return string.Join(Environment.NewLine, lines);
	}
}
