using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application;

/// <summary>
/// A mastery of a character as the Masteries box of the character editor shows it.
/// </summary>
/// <param name="Mastery">The mastery and its skills.</param>
/// <param name="SkillCount">The number of its skills that have points, the mastery included.</param>
/// <param name="PointsSpent">The points spent in the mastery and its skills.</param>
public sealed record MasterySummary(MasteryTree Mastery, int SkillCount, int PointsSpent);
