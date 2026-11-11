namespace TQVaultAE.Domain.Entities;

public class PlayerLevel
{

	//Act I
	//Health: 75 (N)/150 (E)/200 (L) for a total of 425 additional health.
	//Attribute points: 2 bonus attribute points per difficulty (may be spent on any attribute).

	//Act II
	//Attribute points: 2 bonus attribute points per difficulty(may be spent on any attribute).

	//Act III
	//Attribute points: 2 bonus attribute points per difficulty(may be spent on any attribute).

	//Act IV(Immortal Throne Expansion only)
	//Health:        80 (N)/  150 (E)/  200 (L) for a total of 430 additional health.
	//Strength:       6 (N)/    8 (E)/   10 (L) 
	//              + 4 (N)/    6 (E)/    8 (L) for a total of 42 additonal strength.
	//Intelligence:   4 (N)/    6 (E)/    8 (L) for a total of 18 additional Intelligence.
	//Dexterity:      4 (N)/    6 (E)/    8 (L) for a total of 18 additional Dexterity.
	//
	//By the end of the Immortal Throne expansion, the player would have recieved 
	//a total of 855 Health, 42 Strength, 18 Intelligence, 18 Dexterity and 18 attribute points.
	public static int MaxAttributePoints => 168;

	public static int HealthAndManaIncrementPerPoint => 40;

	public static int AtrributeIncrementPerPoint => 4;

	public static int AtrributePointsPerLevel => 2;

	public static int MinStrength => 50;

	public static int MinDexterity => 50;

	public static int MinIntelligence => 50;

	public static int MinHealth => 300;

	public static int MinMana => 300;


	//The player starts at level 1 with 0 skill points. 
	//Each level gained grants the player 3 skill points for a total of 222 skill points at level 75 (maximum level with Immortal Throne expansion). 
	//Additional skill points may be earned through quests.

	//1 Skill point per difficulty in Act I.
	//4 Skill points per difficutly in Act II.
	//2 Skill points per difficutly in Act IV (Immortal throne expansion only).
	//This adds up to a maximum of 243 skill points.

	//With the Ragnarok expansion, the number of skill points available to a maximum level character 
	//is increased by 39 to a maximum of 282 skill points at level 85.

	//30 from the 10 new character levels
	//9 for completing quests in Act V(3 per difficulty)

	public static int MaxSkillPoints => 282;

	public static int SkillPointsPerLevel => 3;


	static Dictionary<int, int> _levelKey = new Dictionary<int, int>() {
		{1    ,       0},
		{2    ,     621},
		{3    ,    2315},
		{4    ,    5891},
		{5    ,   12162},
		{6    ,   21992},
		{7    ,   36290},
		{8    ,   56005},
		{9    ,   82121},
		{10   ,   115651},
		{11   ,   157641},
		{12   ,   209161},
		{13   ,   271305},
		{14   ,   345193},
		{15   ,   431965},
		{16   ,   532784},
		{17   ,   648832},
		{18   ,   781310},
		{19   ,   931439},
		{20   ,  1100457},
		{21   ,  1289620},
		{22   ,  1500203},
		{23   ,  1733497},
		{24   ,  1990811},
		{25 ,    2273471},
		{26 ,    2582821},
		{27 ,    2920225},
		{28 ,    3287064},
		{29 ,    3684740},
		{30 ,    4114678},
		{31 ,    4578325},
		{32 ,    5077155},
		{33 ,    5612669},
		{34 ,    6186403},
		{35 ,    6799926},
		{36 ,    7454853},
		{37 ,    8152847},
		{38 ,    8895627},
		{39 ,    9684982},
		{40 ,   10522780},
		{41 ,   11410986},
		{42 ,   12351677},
		{43 ,   13347070},
		{44 ,   14399546},
		{45 ,   15511685},
		{46 ,   16686309},
		{47 ,   17926530},
		{48 ,   19235814},
		{49 ,   20618054},
		{50 ,   22077662},
		{51 ,   23619679},
		{52 ,   25249911},
		{53 ,   26975090},
		{54 ,   28803077},
		{55 ,   30743102},
		{56 ,   32806057},
		{57 ,   35004854},
		{58 ,   37354860},
		{59 ,   39874424},
		{60 ,   42585514},
		{61 ,   45514498},
		{62 ,   48693085},
		{63 ,   52159470},
		{64 ,   55959726},
		{65 ,   60149489},
		{66 ,   64796032},
		{67 ,   69980607},
		{68 ,   75801742},
		{69 ,   82378607},
		{70 ,   89855592},
		{71 ,   98407643},
		{72 ,  108246785},
		{73 ,  119630017},
		{74 ,  132868885},
		{75 ,  148341092},
		{76 ,  166504562},
		{77 ,  187914491},
		{78 ,  213244003},
		{79 ,  243309184},
		{80 ,  279099406},
		{81 ,  321814070},
		{82 ,  372907109},
		{83 ,  434140895},
		{84 ,  507651523},
		{85 ,  596027881}
	};

	public static int GetLevelMinXP(int level)
	{
		if (_levelKey.TryGetValue(level, out int value))
		{
			return (value);
		}
		throw new ArgumentOutOfRangeException(nameof(level), level, "Level does not exist or is not supported");
	}

	/// <summary>
	/// The experience a character gets when its level is set: one point above the minimum of the level, so the game
	/// keeps it at that level.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException">The level is not supported.</exception>
	public static int LevelStartXp(int level) => GetLevelMinXP(level) + 1;

	/// <summary>
	/// Money the character gets at least when it unlocks Epic.
	/// </summary>
	public static int EpicMoneyFloor => 5000000;

	/// <summary>
	/// Money the character gets at least when it unlocks Legendary.
	/// </summary>
	public static int LegendaryMoneyFloor => 7500000;

	/// <summary>
	/// Number of masteries a character of <paramref name="level"/> may choose: none under 2, one under 8, else two.
	/// </summary>
	public static int GetMasteriesAllowed(int level)
		=> level < 2 ? 0
		: level < 8 ? 1
		: 2;

	/// <summary>
	/// Attribute points invested in the base attributes over their minimum, which a reset of the attributes gives back.
	/// </summary>
	public static int GetInvestedAttributePoints(int strength, int dexterity, int intelligence, int health, int mana)
		=> (strength - MinStrength) / AtrributeIncrementPerPoint
		+ (dexterity - MinDexterity) / AtrributeIncrementPerPoint
		+ (intelligence - MinIntelligence) / AtrributeIncrementPerPoint
		+ (health - MinHealth) / HealthAndManaIncrementPerPoint
		+ (mana - MinMana) / HealthAndManaIncrementPerPoint;

	/// <summary>
	/// Money of a character that unlocked another difficulty: at least 5M for Epic, 7.5M for Legendary.
	/// </summary>
	/// <param name="money">The money of the character.</param>
	/// <param name="previousDifficulty">The difficulty unlocked before the edit.</param>
	/// <param name="difficulty">The difficulty unlocked by the edit.</param>
	public static int GetMoneyAfterDifficultyChange(int money, int previousDifficulty, int difficulty)
	{
		if (difficulty == previousDifficulty)
			return money;

		return difficulty switch
		{
			1 => Math.Max(money, EpicMoneyFloor),
			2 => Math.Max(money, LegendaryMoneyFloor),
			_ => money,
		};
	}

	/// <summary>
	/// The attribute and skill points a level change gives, or takes back when the level goes down.
	/// </summary>
	/// <param name="previousLevel">The level before the change.</param>
	/// <param name="level">The new level.</param>
	public static (int AttributePoints, int SkillPoints) GetLevelChangePoints(int previousLevel, int level)
	{
		var levels = level - previousLevel;
		return (levels * AtrributePointsPerLevel, levels * SkillPointsPerLevel);
	}

	/// <summary>
	/// The attribute point pool after a base attribute moved from <paramref name="previousValue"/> to
	/// <paramref name="value"/>: raising it spends points, lowering it gives them back.
	/// </summary>
	/// <param name="previousValue">The attribute before the change.</param>
	/// <param name="value">The attribute after the change.</param>
	/// <param name="incrementPerPoint">How much of the attribute one point buys.</param>
	/// <param name="pool">The attribute points left before the change.</param>
	/// <param name="newPool">The attribute points left after the change.</param>
	/// <returns>false when the pool cannot pay for the raise: the change must be undone.</returns>
	public static bool TryChangeAttribute(int previousValue, int value, int incrementPerPoint, int pool, out int newPool)
	{
		newPool = pool + (previousValue - value) / incrementPerPoint;
		if (newPool >= 0)
			return true;

		newPool = pool;
		return false;
	}

	/// <summary>How much of an attribute one point buys: 40 health or energy, 4 strength, intelligence or dexterity.</summary>
	public static int IncrementPerPoint(bool healthOrEnergy)
		=> healthOrEnergy ? HealthAndManaIncrementPerPoint : AtrributeIncrementPerPoint;

	/// <summary>
	/// The value a click on an attribute's "+" button asks for: a left click (<paramref name="step"/> &gt; 0) one point
	/// more, with Shift every point left; a right click one point back, with Shift back to the value at the start (never
	/// above the current one). Kept within the attribute's bounds; the pool then pays or refunds it
	/// (<see cref="TryChangeAttribute"/>).
	/// </summary>
	public static int AttributeClickTarget(int current, int start, int incrementPerPoint, int pointsLeft, int step, bool toEnd, int minimum, int maximum)
	{
		var target = step > 0
			? current + incrementPerPoint * (toEnd ? pointsLeft : 1)
			: toEnd ? Math.Min(current, start) : current - incrementPerPoint;
		return Math.Clamp(target, minimum, maximum);
	}

	/// <summary>
	/// The order in which Undo puts the attributes back to their start values: the raised ones first, which gives their
	/// points back for the lowered ones.
	/// </summary>
	public static IEnumerable<int> AttributeUndoOrder(IReadOnlyList<int> current, IReadOnlyList<int> start)
		=> Enumerable.Range(0, current.Count).OrderBy(i => current[i] > start[i] ? 0 : 1);
}