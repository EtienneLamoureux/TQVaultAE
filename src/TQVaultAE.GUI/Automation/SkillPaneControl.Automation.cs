using System.Drawing;
using TQVaultAE.Application;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.GUI.Components.Character;

/// <summary>
/// Debug-only access to the skill pane for the automation endpoint: what each button shows, and clicks that go through
/// the pane's own hit test and click event.
/// </summary>
internal sealed partial class SkillPaneControl
{
	/// <summary>What the pane draws, button by button.</summary>
	internal object AutomationState()
	{
		var tree = this.Tree;
		return new
		{
			pane = this.Name,
			mastery = tree?.Mastery.ToString(),
			masteryLevel = this.MasteryLevel,
			barFill = SkillPaneLayout.BarFill(this.MasteryLevel),
			availablePoints = this.AvailablePoints,
			scale = this.PaneScale,
			size = new { this.Width, this.Height },
			background = tree?.Pane?.Background,
			skills = tree?.Skills.Select(skill =>
			{
				var view = this.SkillView?.Invoke(skill);
				var button = skill.Button;
				return new
				{
					id = skill.Id.Raw,
					name = this.Translate(skill.NameTag),
					tier = skill.Tier,
					x = button?.X,
					y = button?.Y,
					round = button?.IsCircular,
					level = view?.Level,
					itemBonus = view?.ItemBonus,
					levelText = view is null ? null : SkillPaneLayout.LevelText(view.Level, view.ItemBonus, skill.MaxLevel, view.TierOpen),
					state = view?.State.ToString(),
					icon = view is { TierOpen: false } && button is { LockedIcon.Length: > 0 } ? button.LockedIcon : skill.Icon,
					connectors = button is null || view is null ? 0 : SkillPaneLayout.Connectors(button, view.Level > 0).Count(),
					toolTip = this.ToolTipLines?.Invoke(skill) is { } lines ? string.Join(Environment.NewLine, lines.Select(l => l.Text)) : null,
				};
			}).ToList(),
		};
	}

	/// <summary>
	/// Clicks the centre of a skill button, or of the mastery button for <see langword="null"/>, through the hit test,
	/// as a mouse click there would.
	/// </summary>
	/// <returns>The skill or mastery hit, or <see langword="null"/> when the click hit nothing.</returns>
	internal object? AutomationClick(MasterySkill? skill, int step, bool toEnd)
	{
		Point center;
		if (skill is null)
		{
			var art = this.Tree?.Pane ?? throw new InvalidOperationException("The pane shows no mastery.");
			center = new Point(art.MasteryButtonX + SkillPaneLayout.MasteryButtonSize / 2, art.MasteryButtonY + SkillPaneLayout.MasteryButtonSize / 2);
		}
		else
		{
			var button = skill.Button ?? throw new InvalidOperationException($"'{skill.Id}' has no button.");
			center = new Point(button.X + SkillPaneLayout.ButtonSize / 2, button.Y + SkillPaneLayout.ButtonSize / 2);
		}

		var location = new Point((int)Math.Round(center.X * this.PaneScale), (int)Math.Round(center.Y * this.PaneScale));
		var hit = this.HitTest(location);
		switch (hit)
		{
			case MasteryTree:
				this.RaiseClick(new SkillPaneClick(null, step, toEnd));
				break;
			case MasterySkill hitSkill:
				this.RaiseClick(new SkillPaneClick(hitSkill, step, toEnd));
				break;
		}

		return hit;
	}
}
