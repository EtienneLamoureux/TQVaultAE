using System;
using System.ComponentModel;

namespace TQVaultAE.Domain.Entities
{
	[Flags]
	public enum Masteries
	{
		[Description(@"Records\Skills\Defensive\DefensiveMastery.dbr")]
		Defensive = 1 << 0,
		[Description(@"Records\Skills\Storm\StormMastery.dbr")]
		Storm = 1 << 1,
		[Description(@"Records\Skills\Earth\EarthMastery.dbr")]
		Earth = 1 << 2,
		[Description(@"Records\Skills\Nature\NatureMastery.dbr")]
		Nature = 1 << 3,
		[Description(@"Records\XPack\Skills\Dream\DreamMastery.dbr")]
		Dream = 1 << 4,
		[Description(@"records\xpack2\skills\RuneMaster\RuneMaster_Mastery.dbr")]
		Rune = 1 << 5,
		[Description(@"Records\Skills\Warfare\WarfareMastery.dbr")]
		Warfare = 1 << 6,
		[Description(@"Records\Skills\Hunting\HuntingMastery.dbr")]
		Hunting = 1 << 7,
		[Description(@"Records\Skills\Stealth\StealthMastery.dbr")]
		Stealth = 1 << 8,
		[Description(@"Records\Skills\Spirit\SpiritMastery.dbr")]
		Spirit = 1 << 9,
		[Description(@"Records\Xpack4\Skills\Neidan\NeidanMastery.dbr")]
		Neidan = 1 << 10,
	}
}
