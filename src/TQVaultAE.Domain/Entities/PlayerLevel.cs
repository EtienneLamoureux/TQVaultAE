using System;
using System.Collections.Generic;

namespace TQVaultAE.Domain.Entities
{
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
			if (_levelKey.ContainsKey(level))
			{
				return (_levelKey[level]);
			}
			throw new ArgumentOutOfRangeException("Level does not exist or is not supported");
		}
	}
}
