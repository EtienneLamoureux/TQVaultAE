namespace TQVaultAE.Application.Session;

/// <summary>What the item requirements are checked against: the level and the attributes with their gear and skill bonuses.</summary>
public sealed record RequirementAttributes(int Level, int Strength, int Dexterity, int Intelligence)
{
	/// <summary>The save's: <see cref="PlayerInfo.CalculatedStrength"/> and the others.</summary>
	public static RequirementAttributes Of(PlayerInfo info)
		=> new(info.CurrentLevel, info.CalculatedStrength, info.CalculatedDexterity, info.CalculatedIntelligence);
}
