using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace TQVaultAE.Domain.Entities;

public enum RelicAndCharm
{
	/// <summary>
	/// Unknown
	/// </summary>
	[Description(@"NORECORDS")]
	[GearType(GearType.Undefined)]
	Unknown,

	/// <summary>
	/// Essence of the Aegis of Athena
	/// </summary>
	/// <remarks>
	/// The Aegis is the shield of Athena, Greek 
	/// goddess of battle. 
	/// Can enchant shields only.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\01_ACT1_AEGISOFATHENA.DBR")]
	[GearType(GearType.Shield)]
	AEGISOFATHENA_ACT1_01,

	/// <summary>
	/// Embodiment of the Aegis of Athena
	/// </summary>
	/// <remarks>
	/// The Aegis is the shield of Athena, Greek 
	/// goddess of battle. 
	/// Can enchant shields only.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\02_ACT1_AEGISOFATHENA.DBR")]
	[GearType(GearType.Shield)]
	AEGISOFATHENA_ACT1_02,

	/// <summary>
	/// Incarnation of the Aegis of Athena
	/// </summary>
	/// <remarks>
	/// The Aegis is the shield of Athena, Greek 
	/// goddess of battle. 
	/// Can enchant shields only.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\03_ACT1_AEGISOFATHENA.DBR")]
	[GearType(GearType.Shield)]
	AEGISOFATHENA_ACT1_03,

	/// <summary>
	/// Essence of Amun-Ra's Glory
	/// </summary>
	/// <remarks>
	/// Amun-Ra, ruler of the sun, is the 
	/// greatest of the Egyptian gods. 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\01_ACT2_AMUNRASGLORY.DBR")]
	[GearType(GearType.AllArmor)]
	AMUNRASGLORY_ACT2_01,

	/// <summary>
	/// Embodiment of Amun-Ra's Glory
	/// </summary>
	/// <remarks>
	/// Amun-Ra, ruler of the sun, is the 
	/// greatest of the Egyptian gods. 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\02_ACT2_AMUNRASGLORY.DBR")]
	[GearType(GearType.AllArmor)]
	AMUNRASGLORY_ACT2_02,

	/// <summary>
	/// Incarnation of Amun-Ra's Glory
	/// </summary>
	/// <remarks>
	/// Amun-Ra, ruler of the sun, is the 
	/// greatest of the Egyptian gods. 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\03_ACT2_AMUNRASGLORY.DBR")]
	[GearType(GearType.AllArmor)]
	AMUNRASGLORY_ACT2_03,

	/// <summary>
	/// Essence of the Ankh of Isis
	/// </summary>
	/// <remarks>
	/// Isis is queen of the Egyptian gods. Her 
	/// symbol, the ankh, is associated with 
	/// healing and immortality. 
	/// Can enchant rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\01_ACT2_ANKHOFISIS.DBR")]
	[GearType(GearType.Jewellery)]
	ANKHOFISIS_ACT2_01,

	/// <summary>
	/// Embodiment of the Ankh of Isis
	/// </summary>
	/// <remarks>
	/// Isis is queen of the Egyptian gods. Her 
	/// symbol, the ankh, is associated with 
	/// healing and immortality. 
	/// Can enchant rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\02_ACT2_ANKHOFISIS.DBR")]
	[GearType(GearType.Jewellery)]
	ANKHOFISIS_ACT2_02,

	/// <summary>
	/// Incarnation of the Ankh of Isis
	/// </summary>
	/// <remarks>
	/// Isis is queen of the Egyptian gods. Her 
	/// symbol, the ankh, is associated with 
	/// healing and immortality. 
	/// Can enchant rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\03_ACT2_ANKHOFISIS.DBR")]
	[GearType(GearType.Jewellery)]
	ANKHOFISIS_ACT2_03,

	/// <summary>
	/// Essence of Anubis' Wrath
	/// </summary>
	/// <remarks>
	/// Anubis, a jackal-headed god, is ruler of 
	/// the Egyptian underworld. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\01_ACT2_ANUBISWRATH.DBR")]
	[GearType(GearType.AllWeapons)]
	ANUBISWRATH_ACT2_01,

	/// <summary>
	/// Embodiment of Anubis' Wrath
	/// </summary>
	/// <remarks>
	/// Anubis, a jackal-headed god, is ruler of 
	/// the Egyptian underworld. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\02_ACT2_ANUBISWRATH.DBR")]
	[GearType(GearType.AllWeapons)]
	ANUBISWRATH_ACT2_02,

	/// <summary>
	/// Incarnation of Anubis' Wrath
	/// </summary>
	/// <remarks>
	/// Anubis, a jackal-headed god, is ruler of 
	/// the Egyptian underworld. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\03_ACT2_ANUBISWRATH.DBR")]
	[GearType(GearType.AllWeapons)]
	ANUBISWRATH_ACT2_03,

	/// <summary>
	/// Essence of Archimedes' Mirror
	/// </summary>
	/// <remarks>
	/// Archimedes was one of the greatest of 
	/// Greece's philosophers and scientists. 
	/// Can enchant shields only.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\01_ACT1_ARCHIMEDESMIRROR.DBR")]
	[GearType(GearType.Shield)]
	ARCHIMEDESMIRROR_ACT1_01,

	/// <summary>
	/// Embodiment of Archimedes' Mirror
	/// </summary>
	/// <remarks>
	/// Archimedes was one of the greatest of 
	/// Greece's philosophers and scientists. 
	/// Can enchant shields only.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\02_ACT1_ARCHIMEDESMIRROR.DBR")]
	[GearType(GearType.Shield)]
	ARCHIMEDESMIRROR_ACT1_02,

	/// <summary>
	/// Incarnation of Archimedes' Mirror
	/// </summary>
	/// <remarks>
	/// Archimedes was one of the greatest of 
	/// Greece's philosophers and scientists. 
	/// Can enchant shields only.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\03_ACT1_ARCHIMEDESMIRROR.DBR")]
	[GearType(GearType.Shield)]
	ARCHIMEDESMIRROR_ACT1_03,

	/// <summary>
	/// Essence of Artemis' Bowstring
	/// </summary>
	/// <remarks>
	/// Artemis, the Greek goddess of the hunt, 
	/// is renowned for her skill with the bow. 
	/// Can enchant bows only.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\01_ACT1_ARTEMISBOWSTRING.DBR")]
	[GearType(GearType.Bow)]
	ARTEMISBOWSTRING_ACT1_01,

	/// <summary>
	/// Embodiment of Artemis' Bowstring
	/// </summary>
	/// <remarks>
	/// Artemis, the Greek goddess of the hunt, 
	/// is renowned for her skill with the bow. 
	/// Can enchant bows only.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\02_ACT1_ARTEMISBOWSTRING.DBR")]
	[GearType(GearType.Bow)]
	ARTEMISBOWSTRING_ACT1_02,

	/// <summary>
	/// Incarnation of Artemis' Bowstring
	/// </summary>
	/// <remarks>
	/// Artemis, the Greek goddess of the hunt, 
	/// is renowned for her skill with the bow. 
	/// Can enchant bows only.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\03_ACT1_ARTEMISBOWSTRING.DBR")]
	[GearType(GearType.Bow)]
	ARTEMISBOWSTRING_ACT1_03,

	/// <summary>
	/// Atlantean Artifice
	/// </summary>
	/// <remarks>
	/// The strange contraption you assembled 
	/// under Meidias' guidance is buzzing with 
	/// electrical energy, ready to strike out 
	/// at unsuspecting foes. 
	/// Can enchant torso armor or shields.
	/// </remarks>
	[Description(@"RECORDS\XPACK3\ITEMS\CHARMS\01_X3_QUEST_ARTIFICE.DBR")]
	[GearType(GearType.Torso | GearType.Shield)]
	ARTIFICE_QUEST_X3_01,

	/// <summary>
	/// Epic Atlantean Artifice
	/// </summary>
	/// <remarks>
	/// The strange contraption you assembled 
	/// under Meidias' guidance is buzzing with 
	/// electrical energy, ready to strike out 
	/// at unsuspecting foes. 
	/// Can enchant torso armor or shields.
	/// </remarks>
	[Description(@"RECORDS\XPACK3\ITEMS\CHARMS\02_X3_QUEST_ARTIFICE.DBR")]
	[GearType(GearType.Torso | GearType.Shield)]
	ARTIFICE_QUEST_X3_02,

	/// <summary>
	/// Legendary Atlantean Artifice
	/// </summary>
	/// <remarks>
	/// The strange contraption you assembled 
	/// under Meidias' guidance is buzzing with 
	/// electrical energy, ready to strike out 
	/// at unsuspecting foes. 
	/// Can enchant torso armor or shields.
	/// </remarks>
	[Description(@"RECORDS\XPACK3\ITEMS\CHARMS\03_X3_QUEST_ARTIFICE.DBR")]
	[GearType(GearType.Torso | GearType.Shield)]
	ARTIFICE_QUEST_X3_03,

	/// <summary>
	/// Essence of Atlas' Endurance
	/// </summary>
	/// <remarks>
	/// After the war with the gods, the titan 
	/// Atlas was condemned to bear the heavens 
	/// upon his shoulders for all eternity. 
	/// Can enhance rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\XPACK3\ITEMS\RELICS\01_X3_ATLASENDURANCE.DBR")]
	[GearType(GearType.Jewellery)]
	ATLASENDURANCE_X3_01,

	/// <summary>
	/// Embodiment of Atlas' Endurance
	/// </summary>
	/// <remarks>
	/// After the war with the gods, the titan 
	/// Atlas was condemned to bear the heavens 
	/// upon his shoulders for all eternity. 
	/// Can enhance rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\XPACK3\ITEMS\RELICS\02_X3_ATLASENDURANCE.DBR")]
	[GearType(GearType.Jewellery)]
	ATLASENDURANCE_X3_02,

	/// <summary>
	/// Incarnation of Atlas' Endurance
	/// </summary>
	/// <remarks>
	/// After the war with the gods, the titan 
	/// Atlas was condemned to bear the heavens 
	/// upon his shoulders for all eternity. 
	/// Can enhance rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\XPACK3\ITEMS\RELICS\03_X3_ATLASENDURANCE.DBR")]
	[GearType(GearType.Jewellery)]
	ATLASENDURANCE_X3_03,

	/// <summary>
	/// Basilisk Eye
	/// </summary>
	/// <remarks>
	/// The fearsome eye of a Basilisk, still 
	/// retaining ist stare. 
	/// Can enchant amulets, staves or head 
	/// armor.
	/// </remarks>
	[Description(@"RECORDS\XPACK3\ITEMS\CHARMS\01_X3_BASILISKEYE.DBR")]
	[GearType(GearType.Amulet | GearType.Staff | GearType.Head)]
	BASILISKEYE_X3_01,

	/// <summary>
	/// Epic Basilisk Eye
	/// </summary>
	/// <remarks>
	/// The fearsome eye of a Basilisk, still 
	/// retaining ist stare. 
	/// Can enchant amulets, staves or head 
	/// armor.
	/// </remarks>
	[Description(@"RECORDS\XPACK3\ITEMS\CHARMS\02_X3_BASILISKEYE.DBR")]
	[GearType(GearType.Amulet | GearType.Staff | GearType.Head)]
	BASILISKEYE_X3_02,

	/// <summary>
	/// Legendary Basilisk Eye
	/// </summary>
	/// <remarks>
	/// The fearsome eye of a Basilisk, still 
	/// retaining ist stare. 
	/// Can enchant amulets, staves or head 
	/// armor.
	/// </remarks>
	[Description(@"RECORDS\XPACK3\ITEMS\CHARMS\03_X3_BASILISKEYE.DBR")]
	[GearType(GearType.Amulet | GearType.Staff | GearType.Head)]
	BASILISKEYE_X3_03,

	/// <summary>
	/// Bat Fang
	/// </summary>
	/// <remarks>
	/// Hollow fang of a Cave Bat.  
	/// Can enhance all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\01_ACT1_BATFANG.DBR")]
	[GearType(GearType.AllWeapons)]
	BATFANG_ACT1_01,

	/// <summary>
	/// Epic Bat Fang
	/// </summary>
	/// <remarks>
	/// Hollow fang of a Cave Bat.  
	/// Can enhance all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\02_ACT1_BATFANG.DBR")]
	[GearType(GearType.AllWeapons)]
	BATFANG_ACT1_02,

	/// <summary>
	/// Legendary Bat Fang
	/// </summary>
	/// <remarks>
	/// Hollow fang of a Cave Bat.  
	/// Can enhance all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\03_ACT1_BATFANG.DBR")]
	[GearType(GearType.AllWeapons)]
	BATFANG_ACT1_03,

	/// <summary>
	/// Essence of the Blade of Thanatos
	/// </summary>
	/// <remarks>
	/// Thanatos, the incarnation of death, 
	/// carries a sword with the power to rend 
	/// the very souls of mere mortals. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\RELICS\01_ACT4_BLADEOFTHANATOS.DBR")]
	[GearType(GearType.AllWeapons)]
	BLADEOFTHANATOS_ACT4_01,

	/// <summary>
	/// Embodiment of the Blade of Thanatos
	/// </summary>
	/// <remarks>
	/// Thanatos, the incarnation of death, 
	/// carries a sword with the power to rend 
	/// the very souls of mere mortals. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\RELICS\02_ACT4_BLADEOFTHANATOS.DBR")]
	[GearType(GearType.AllWeapons)]
	BLADEOFTHANATOS_ACT4_02,

	/// <summary>
	/// Incarnation of the Blade of Thanatos
	/// </summary>
	/// <remarks>
	/// Thanatos, the incarnation of death, 
	/// carries a sword with the power to rend 
	/// the very souls of mere mortals. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\RELICS\03_ACT4_BLADEOFTHANATOS.DBR")]
	[GearType(GearType.AllWeapons)]
	BLADEOFTHANATOS_ACT4_03,

	/// <summary>
	/// Boar Hide
	/// </summary>
	/// <remarks>
	/// Toughened hide of a wild boar.  
	/// Can enhance all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\01_ACT1_BOARSHIDE.DBR")]
	[GearType(GearType.AllArmor)]
	BOARSHIDE_ACT1_01,

	/// <summary>
	/// Epic Boar Hide
	/// </summary>
	/// <remarks>
	/// Toughened hide of a wild boar.  
	/// Can enhance all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\02_ACT1_BOARSHIDE.DBR")]
	[GearType(GearType.AllArmor)]
	BOARSHIDE_ACT1_02,

	/// <summary>
	/// Legendary Boar Hide
	/// </summary>
	/// <remarks>
	/// Toughened hide of a wild boar.  
	/// Can enhance all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\03_ACT1_BOARSHIDE.DBR")]
	[GearType(GearType.AllArmor)]
	BOARSHIDE_ACT1_03,

	/// <summary>
	/// Essence of Cernunnos' Majesty
	/// </summary>
	/// <remarks>
	/// Cernunnos is the enigmatic Celtic lord 
	/// of nature, a serene ruler with an 
	/// antlered head. 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\RELICS\01_ACT5_CERNUNNOSMAJESTY.DBR")]
	[GearType(GearType.AllArmor)]
	CERNUNNOSMAJESTY_ACT5_01,

	/// <summary>
	/// Embodiment of Cernunnos' Majesty
	/// </summary>
	/// <remarks>
	/// Cernunnos is the enigmatic Celtic lord 
	/// of nature, a serene ruler with an 
	/// antlered head. 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\RELICS\02_ACT5_CERNUNNOSMAJESTY.DBR")]
	[GearType(GearType.AllArmor)]
	CERNUNNOSMAJESTY_ACT5_02,

	/// <summary>
	/// Incarnation of Cernunnos' Majesty
	/// </summary>
	/// <remarks>
	/// Cernunnos is the enigmatic Celtic lord 
	/// of nature, a serene ruler with an 
	/// antlered head. 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\RELICS\03_ACT5_CERNUNNOSMAJESTY.DBR")]
	[GearType(GearType.AllArmor)]
	CERNUNNOSMAJESTY_ACT5_03,

	/// <summary>
	/// Essence of the Chill of Tartarus
	/// </summary>
	/// <remarks>
	/// In the deepest pits of the underworld 
	/// lies Tartarus. Cold as the grave and 
	/// swathed in darkness, it is here that 
	/// punishment is meted out for those 
	/// mortals foolish enough to oppose the 
	/// will of the gods. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\RELICS\01_ACT4_CHILLOFTARTARUS.DBR")]
	[GearType(GearType.AllWeapons)]
	CHILLOFTARTARUS_ACT4_01,

	/// <summary>
	/// Embodiment of the Chill of Tartarus
	/// </summary>
	/// <remarks>
	/// In the deepest pits of the underworld 
	/// lies Tartarus. Cold as the grave and 
	/// swathed in darkness, it is here that 
	/// punishment is meted out for those 
	/// mortals foolish enough to oppose the 
	/// will of the gods. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\RELICS\02_ACT4_CHILLOFTARTARUS.DBR")]
	[GearType(GearType.AllWeapons)]
	CHILLOFTARTARUS_ACT4_02,

	/// <summary>
	/// Incarnation of the Chill of Tartarus
	/// </summary>
	/// <remarks>
	/// In the deepest pits of the underworld 
	/// lies Tartarus. Cold as the grave and 
	/// swathed in darkness, it is here that 
	/// punishment is meted out for those 
	/// mortals foolish enough to oppose the 
	/// will of the gods. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\RELICS\03_ACT4_CHILLOFTARTARUS.DBR")]
	[GearType(GearType.AllWeapons)]
	CHILLOFTARTARUS_ACT4_03,

	/// <summary>
	/// Essence of the Code of Hammurabi
	/// </summary>
	/// <remarks>
	/// Hammurabi, king of Babylon, was revered 
	/// for bringing justice to his people with 
	/// his code of laws. 
	/// Can enchant rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\01_ACT3_CODEOFHAMMURABI.DBR")]
	[GearType(GearType.Jewellery)]
	CODEOFHAMMURABI_ACT3_01,

	/// <summary>
	/// Embodiment of the Code of Hammurabi
	/// </summary>
	/// <remarks>
	/// Hammurabi, king of Babylon, was revered 
	/// for bringing justice to his people with 
	/// his code of laws. 
	/// Can enchant rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\02_ACT3_CODEOFHAMMURABI.DBR")]
	[GearType(GearType.Jewellery)]
	CODEOFHAMMURABI_ACT3_02,

	/// <summary>
	/// Incarnation of the Code of Hammurabi
	/// </summary>
	/// <remarks>
	/// Hammurabi, king of Babylon, was revered 
	/// for bringing justice to his people with 
	/// his code of laws. 
	/// Can enchant rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\03_ACT3_CODEOFHAMMURABI.DBR")]
	[GearType(GearType.Jewellery)]
	CODEOFHAMMURABI_ACT3_03,

	/// <summary>
	/// Cold Essence
	/// </summary>
	/// <remarks>
	/// A translucent crystalline mass that 
	/// seems to flow and yet retains its shape. 
	/// 
	/// Can enhance arm armor only.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\CHARMS\01_ACT5_COLDESSENCE.DBR")]
	[GearType(GearType.Arm)]
	COLDESSENCE_ACT5_01,

	/// <summary>
	/// Epic Cold Essence
	/// </summary>
	/// <remarks>
	/// A translucent crystalline mass that 
	/// seems to flow and yet retains its shape. 
	/// 
	/// Can enhance arm armor only.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\CHARMS\02_ACT5_COLDESSENCE.DBR")]
	[GearType(GearType.Arm)]
	COLDESSENCE_ACT5_02,

	/// <summary>
	/// Legendary Cold Essence
	/// </summary>
	/// <remarks>
	/// A translucent crystalline mass that 
	/// seems to flow and yet retains its shape. 
	/// 
	/// Can enhance arm armor only.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\CHARMS\03_ACT5_COLDESSENCE.DBR")]
	[GearType(GearType.Arm)]
	COLDESSENCE_ACT5_03,

	/// <summary>
	/// Coral Fragment
	/// </summary>
	/// <remarks>
	/// A remarkably hard and sharp piece of 
	/// coral skeleton. 
	/// Can enchant torso armor or shields.
	/// </remarks>
	[Description(@"RECORDS\XPACK3\ITEMS\CHARMS\01_X3_CORALFRAGMENT.DBR")]
	[GearType(GearType.Torso | GearType.Shield)]
	CORALFRAGMENT_X3_01,

	/// <summary>
	/// Epic Coral Fragment
	/// </summary>
	/// <remarks>
	/// A remarkably hard and sharp piece of 
	/// coral skeleton. 
	/// Can enchant torso armor or shields.
	/// </remarks>
	[Description(@"RECORDS\XPACK3\ITEMS\CHARMS\02_X3_CORALFRAGMENT.DBR")]
	[GearType(GearType.Torso | GearType.Shield)]
	CORALFRAGMENT_X3_02,

	/// <summary>
	/// Legendary Coral Fragment
	/// </summary>
	/// <remarks>
	/// A remarkably hard and sharp piece of 
	/// coral skeleton. 
	/// Can enchant torso armor or shields.
	/// </remarks>
	[Description(@"RECORDS\XPACK3\ITEMS\CHARMS\03_X3_CORALFRAGMENT.DBR")]
	[GearType(GearType.Torso | GearType.Shield)]
	CORALFRAGMENT_X3_03,

	/// <summary>
	/// Essence of the Cunning of Odysseus
	/// </summary>
	/// <remarks>
	/// Although not as strong or swift as the 
	/// other heroes of legend, Odysseus' 
	/// formidable guile made him a match for 
	/// any foe. 
	/// Can enchant head armor only.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\RELICS\01_ACT4_CUNNINGOFODDYSEUS.DBR")]
	[GearType(GearType.Head)]
	CUNNINGOFODDYSEUS_ACT4_01,

	/// <summary>
	/// Embodiment of the Cunning of Odysseus
	/// </summary>
	/// <remarks>
	/// Although not as strong or swift as the 
	/// other heroes of legend, Odysseus' 
	/// formidable guile made him a match for 
	/// any foe. 
	/// Can enchant head armor only.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\RELICS\02_ACT4_CUNNINGOFODDYSEUS.DBR")]
	[GearType(GearType.Head)]
	CUNNINGOFODDYSEUS_ACT4_02,

	/// <summary>
	/// Incarnation of the Cunning of Odysseus
	/// </summary>
	/// <remarks>
	/// Although not as strong or swift as the 
	/// other heroes of legend, Odysseus' 
	/// formidable guile made him a match for 
	/// any foe. 
	/// Can enchant head armor only.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\RELICS\03_ACT4_CUNNINGOFODDYSEUS.DBR")]
	[GearType(GearType.Head)]
	CUNNINGOFODDYSEUS_ACT4_03,

	/// <summary>
	/// Demon's Blood
	/// </summary>
	/// <remarks>
	/// The dark blood of a fallen demon.  
	/// Can enhance rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\01_MULTACTS_DEMONSBLOOD.DBR")]
	[GearType(GearType.Jewellery)]
	DEMONSBLOOD_MULTACTS_01,

	/// <summary>
	/// Epic Demon's Blood
	/// </summary>
	/// <remarks>
	/// The dark blood of a fallen demon.  
	/// Can enhance rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\02_MULTACTS_DEMONSBLOOD.DBR")]
	[GearType(GearType.Jewellery)]
	DEMONSBLOOD_MULTACTS_02,

	/// <summary>
	/// Legendary Demon's Blood
	/// </summary>
	/// <remarks>
	/// The dark blood of a fallen demon.  
	/// Can enhance rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\03_MULTACTS_DEMONSBLOOD.DBR")]
	[GearType(GearType.Jewellery)]
	DEMONSBLOOD_MULTACTS_03,

	/// <summary>
	/// Essence of Dionysus' Wineskin
	/// </summary>
	/// <remarks>
	/// The drink of Dionysus, Greek god of wine 
	/// and revelry, is a potent brew indeed. 
	/// Can enchant rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\01_ACT1_DIONYSUSWINESKIN.DBR")]
	[GearType(GearType.Jewellery)]
	DIONYSUSWINESKIN_ACT1_01,

	/// <summary>
	/// Embodiment of Dionysus' Wineskin
	/// </summary>
	/// <remarks>
	/// The drink of Dionysus, Greek god of wine 
	/// and revelry, is a potent brew indeed. 
	/// Can enchant rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\02_ACT1_DIONYSUSWINESKIN.DBR")]
	[GearType(GearType.Jewellery)]
	DIONYSUSWINESKIN_ACT1_02,

	/// <summary>
	/// Incarnation of Dionysus' Wineskin
	/// </summary>
	/// <remarks>
	/// The drink of Dionysus, Greek god of wine 
	/// and revelry, is a potent brew indeed. 
	/// Can enchant rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\03_ACT1_DIONYSUSWINESKIN.DBR")]
	[GearType(GearType.Jewellery)]
	DIONYSUSWINESKIN_ACT1_03,

	/// <summary>
	/// Diseased Plumage
	/// </summary>
	/// <remarks>
	/// Feathers from a carrion bird.  
	/// Can enhance all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\01_MULTACTS_DISEASEDPLUMAGE.DBR")]
	[GearType(GearType.AllArmor)]
	DISEASEDPLUMAGE_MULTACTS_01,

	/// <summary>
	/// Epic Diseased Plumage
	/// </summary>
	/// <remarks>
	/// Feathers from a carrion bird.  
	/// Can enhance all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\02_MULTACTS_DISEASEDPLUMAGE.DBR")]
	[GearType(GearType.AllArmor)]
	DISEASEDPLUMAGE_MULTACTS_02,

	/// <summary>
	/// Legendary Diseased Plumage
	/// </summary>
	/// <remarks>
	/// Feathers from a carrion bird.  
	/// Can enhance all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\03_MULTACTS_DISEASEDPLUMAGE.DBR")]
	[GearType(GearType.AllArmor)]
	DISEASEDPLUMAGE_MULTACTS_03,

	/// <summary>
	/// Essence of the Djed of Osiris
	/// </summary>
	/// <remarks>
	/// The Djed is the backbone of Osiris, lord 
	/// of the dead. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\01_ACT2_DJEDOFOSIRIS.DBR")]
	[GearType(GearType.AllWeapons)]
	DJEDOFOSIRIS_ACT2_01,

	/// <summary>
	/// Embodiment of the Djed of Osiris
	/// </summary>
	/// <remarks>
	/// The Djed is the backbone of Osiris, lord 
	/// of the dead. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\02_ACT2_DJEDOFOSIRIS.DBR")]
	[GearType(GearType.AllWeapons)]
	DJEDOFOSIRIS_ACT2_02,

	/// <summary>
	/// Incarnation of the Djed of Osiris
	/// </summary>
	/// <remarks>
	/// The Djed is the backbone of Osiris, lord 
	/// of the dead. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\03_ACT2_DJEDOFOSIRIS.DBR")]
	[GearType(GearType.AllWeapons)]
	DJEDOFOSIRIS_ACT2_03,

	/// <summary>
	/// Essence of Donar's Hammer
	/// </summary>
	/// <remarks>
	/// Donar, or Thor, is the Germanic god of 
	/// thunder. A son of giants himself, he 
	/// uses his mighty hammer to protect the 
	/// world from all kinds of monsters. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\RELICS\01_ACT5_DONARSHAMMER.DBR")]
	[GearType(GearType.AllWeapons)]
	DONARSHAMMER_ACT5_01,

	/// <summary>
	/// Embodiment of Donar's Hammer
	/// </summary>
	/// <remarks>
	/// Donar, or Thor, is the Germanic god of 
	/// thunder. A son of giants himself, he 
	/// uses his mighty hammer to protect the 
	/// world from all kinds of monsters. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\RELICS\02_ACT5_DONARSHAMMER.DBR")]
	[GearType(GearType.AllWeapons)]
	DONARSHAMMER_ACT5_02,

	/// <summary>
	/// Incarnation of Donar's Hammer
	/// </summary>
	/// <remarks>
	/// Donar, or Thor, is the Germanic god of 
	/// thunder. A son of giants himself, he 
	/// uses his mighty hammer to protect the 
	/// world from all kinds of monsters. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\RELICS\03_ACT5_DONARSHAMMER.DBR")]
	[GearType(GearType.AllWeapons)]
	DONARSHAMMER_ACT5_03,

	/// <summary>
	/// Essence of the Domain of the Dragon-Kings
	/// </summary>
	/// <remarks>
	/// The Dragon-Kings are the masters of 
	/// water in China. Ocean, rain, lake, and 
	/// river - all are theirs to command. 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\01_ACT3_DRAGONKINGSDOMAIN.DBR")]
	[GearType(GearType.AllArmor)]
	DRAGONKINGSDOMAIN_ACT3_01,

	/// <summary>
	/// Embodiment of the Domain of the Dragon-Kings
	/// </summary>
	/// <remarks>
	/// The Dragon-Kings are the masters of 
	/// water in China. Ocean, rain, lake, and 
	/// river - all are theirs to command. 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\02_ACT3_DRAGONKINGSDOMAIN.DBR")]
	[GearType(GearType.AllArmor)]
	DRAGONKINGSDOMAIN_ACT3_02,

	/// <summary>
	/// Incarnation of the Domain of the Dragon-Kings
	/// </summary>
	/// <remarks>
	/// The Dragon-Kings are the masters of 
	/// water in China. Ocean, rain, lake, and 
	/// river - all are theirs to command. 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\03_ACT3_DRAGONKINGSDOMAIN.DBR")]
	[GearType(GearType.AllArmor)]
	DRAGONKINGSDOMAIN_ACT3_03,

	/// <summary>
	/// Eitr
	/// </summary>
	/// <remarks>
	/// A magical substance said to be the 
	/// origin of all living things, but also 
	/// harmful to the touch. 
	/// Can enhance rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\CHARMS\01_ACT5_EITR.DBR")]
	[GearType(GearType.Jewellery)]
	EITR_ACT5_01,

	/// <summary>
	/// Epic Eitr
	/// </summary>
	/// <remarks>
	/// A magical substance said to be the 
	/// origin of all living things, but also 
	/// harmful to the touch. 
	/// Can enhance rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\CHARMS\02_ACT5_EITR.DBR")]
	[GearType(GearType.Jewellery)]
	EITR_ACT5_02,

	/// <summary>
	/// Legendary Eitr
	/// </summary>
	/// <remarks>
	/// A magical substance said to be the 
	/// origin of all living things, but also 
	/// harmful to the touch. 
	/// Can enhance rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\CHARMS\03_ACT5_EITR.DBR")]
	[GearType(GearType.Jewellery)]
	EITR_ACT5_03,

	/// <summary>
	/// Essence of Epona's Horses
	/// </summary>
	/// <remarks>
	/// Epona is one of the most widely 
	/// venerated goddesses of the Celts, 
	/// protector of their horses and fertility. 
	/// 
	/// Can enchant leg armor only.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\RELICS\01_ACT5_EPONASHORSES.DBR")]
	[GearType(GearType.Leg)]
	EPONASHORSES_ACT5_01,

	/// <summary>
	/// Embodiment of Epona's Horses
	/// </summary>
	/// <remarks>
	/// Epona is one of the most widely 
	/// venerated goddesses of the Celts, 
	/// protector of their horses and fertility. 
	/// 
	/// Can enchant leg armor only.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\RELICS\02_ACT5_EPONASHORSES.DBR")]
	[GearType(GearType.Leg)]
	EPONASHORSES_ACT5_02,

	/// <summary>
	/// Incarnation of Epona's Horses
	/// </summary>
	/// <remarks>
	/// Epona is one of the most widely 
	/// venerated goddesses of the Celts, 
	/// protector of their horses and fertility. 
	/// 
	/// Can enchant leg armor only.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\RELICS\03_ACT5_EPONASHORSES.DBR")]
	[GearType(GearType.Leg)]
	EPONASHORSES_ACT5_03,

	/// <summary>
	/// Crystal of Erebus
	/// </summary>
	/// <remarks>
	/// A splinter from one of the Crystals of 
	/// Erebus, the source of Hades' power in 
	/// the world of the living. 
	/// Can enhance head armor only.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\CHARMS\01_ACT4_EREBANCRYSTAL.DBR")]
	[GearType(GearType.Head)]
	EREBANCRYSTAL_ACT4_01,

	/// <summary>
	/// Epic Crystal of Erebus
	/// </summary>
	/// <remarks>
	/// A splinter from one of the Crystals of 
	/// Erebus, the source of Hades' power in 
	/// the world of the living. 
	/// Can enhance head armor only.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\CHARMS\02_ACT4_EREBANCRYSTAL.DBR")]
	[GearType(GearType.Head)]
	EREBANCRYSTAL_ACT4_02,

	/// <summary>
	/// Legendary Crystal of Erebus
	/// </summary>
	/// <remarks>
	/// A splinter from one of the Crystals of 
	/// Erebus, the source of Hades' power in 
	/// the world of the living. 
	/// Can enhance head armor only.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\CHARMS\03_ACT4_EREBANCRYSTAL.DBR")]
	[GearType(GearType.Head)]
	EREBANCRYSTAL_ACT4_03,

	/// <summary>
	/// Albino Spider Web
	/// </summary>
	/// <remarks>
	/// A sticky mass of webbing pulled from an 
	/// Albino Spider abdomen. 
	/// Can enhance all armor.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\CHARMS\01_ACT4_FROZENCHITIN.DBR")]
	[GearType(GearType.AllArmor)]
	FROZENCHITIN_ACT4_01,

	/// <summary>
	/// Epic Albino Spider Web
	/// </summary>
	/// <remarks>
	/// A sticky mass of webbing pulled from an 
	/// Albino Spider abdomen. 
	/// Can enhance all armor.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\CHARMS\02_ACT4_FROZENCHITIN.DBR")]
	[GearType(GearType.AllArmor)]
	FROZENCHITIN_ACT4_02,

	/// <summary>
	/// Legendary Albino Spider Web
	/// </summary>
	/// <remarks>
	/// A sticky mass of webbing pulled from an 
	/// Albino Spider abdomen. 
	/// Can enhance all armor.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\CHARMS\03_ACT4_FROZENCHITIN.DBR")]
	[GearType(GearType.AllArmor)]
	FROZENCHITIN_ACT4_03,

	/// <summary>
	/// Fungoid Spores
	/// </summary>
	/// <remarks>
	/// Fungoids emit this soporific purple 
	/// powder to render their victims 
	/// defenseless. 
	/// Can enhance all weapons.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\CHARMS\01_ACT5_FUNGOIDSPORES.DBR")]
	[GearType(GearType.AllWeapons)]
	FUNGOIDSPORES_ACT5_01,

	/// <summary>
	/// Epic Fungoid Spores
	/// </summary>
	/// <remarks>
	/// Fungoids emit this soporific purple 
	/// powder to render their victims 
	/// defenseless. 
	/// Can enhance all weapons.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\CHARMS\02_ACT5_FUNGOIDSPORES.DBR")]
	[GearType(GearType.AllWeapons)]
	FUNGOIDSPORES_ACT5_02,

	/// <summary>
	/// Legendary Fungoid Spores
	/// </summary>
	/// <remarks>
	/// Fungoids emit this soporific purple 
	/// powder to render their victims 
	/// defenseless. 
	/// Can enhance all weapons.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\CHARMS\03_ACT5_FUNGOIDSPORES.DBR")]
	[GearType(GearType.AllWeapons)]
	FUNGOIDSPORES_ACT5_03,

	/// <summary>
	/// Fury's Heartblood
	/// </summary>
	/// <remarks>
	/// Blood drained from a Fury's 
	/// still-beating heart. 
	/// Can enhance all weapons.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\CHARMS\01_ACT4_FURYSHEARTBLOOD.DBR")]
	[GearType(GearType.AllWeapons)]
	FURYSHEARTBLOOD_ACT4_01,

	/// <summary>
	/// Epic Fury's Heartblood
	/// </summary>
	/// <remarks>
	/// Blood drained from a Fury's 
	/// still-beating heart. 
	/// Can enhance all weapons.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\CHARMS\02_ACT4_FURYSHEARTBLOOD.DBR")]
	[GearType(GearType.AllWeapons)]
	FURYSHEARTBLOOD_ACT4_02,

	/// <summary>
	/// Legendary Fury's Heartblood
	/// </summary>
	/// <remarks>
	/// Blood drained from a Fury's 
	/// still-beating heart. 
	/// Can enhance all weapons.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\CHARMS\03_ACT4_FURYSHEARTBLOOD.DBR")]
	[GearType(GearType.AllWeapons)]
	FURYSHEARTBLOOD_ACT4_03,

	/// <summary>
	/// Essence of the Golden Fleece
	/// </summary>
	/// <remarks>
	/// The Golden Fleece, sought after by Jason 
	/// and his Argonauts, is rumored to hold 
	/// great power as well as great beauty. 
	/// Can enchant torso armor only.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\01_ACT1_GOLDENFLEECE.DBR")]
	[GearType(GearType.Torso)]
	GOLDENFLEECE_ACT1_01,

	/// <summary>
	/// Embodiment of the Golden Fleece
	/// </summary>
	/// <remarks>
	/// The Golden Fleece, sought after by Jason 
	/// and his Argonauts, is rumored to hold 
	/// great power as well as great beauty. 
	/// Can enchant torso armor only.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\02_ACT1_GOLDENFLEECE.DBR")]
	[GearType(GearType.Torso)]
	GOLDENFLEECE_ACT1_02,

	/// <summary>
	/// Incarnation of the Golden Fleece
	/// </summary>
	/// <remarks>
	/// The Golden Fleece, sought after by Jason 
	/// and his Argonauts, is rumored to hold 
	/// great power as well as great beauty. 
	/// Can enchant torso armor only.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\03_ACT1_GOLDENFLEECE.DBR")]
	[GearType(GearType.Torso)]
	GOLDENFLEECE_ACT1_03,

	/// <summary>
	/// Golem Heart
	/// </summary>
	/// <remarks>
	/// A magnificent magical gem that gave a 
	/// golem its life. 
	/// Can enhance rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\CHARMS\01_ACT5_GOLEMHEART.DBR")]
	[GearType(GearType.Jewellery)]
	GOLEMHEART_ACT5_01,

	/// <summary>
	/// Epic Golem Heart
	/// </summary>
	/// <remarks>
	/// A magnificent magical gem that gave a 
	/// golem its life. 
	/// Can enhance rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\CHARMS\02_ACT5_GOLEMHEART.DBR")]
	[GearType(GearType.Jewellery)]
	GOLEMHEART_ACT5_02,

	/// <summary>
	/// Legendary Golem Heart
	/// </summary>
	/// <remarks>
	/// A magnificent magical gem that gave a 
	/// golem its life. 
	/// Can enhance rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\CHARMS\03_ACT5_GOLEMHEART.DBR")]
	[GearType(GearType.Jewellery)]
	GOLEMHEART_ACT5_03,

	/// <summary>
	/// Essence of Guan-Yu's Grace
	/// </summary>
	/// <remarks>
	/// True to the tenets of Taoism, even 
	/// Guan-Yu, the Taoist god of war, only 
	/// fights when all other options have been 
	/// exhausted. 
	/// Can enchant torso armor only.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\01_ACT3_GUANYUSGRACE.DBR")]
	[GearType(GearType.Torso)]
	GUANYUSGRACE_ACT3_01,

	/// <summary>
	/// Embodiment of Guan-Yu's Grace
	/// </summary>
	/// <remarks>
	/// True to the tenets of Taoism, even 
	/// Guan-Yu, the Taoist god of war, only 
	/// fights when all other options have been 
	/// exhausted. 
	/// Can enchant torso armor only.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\02_ACT3_GUANYUSGRACE.DBR")]
	[GearType(GearType.Torso)]
	GUANYUSGRACE_ACT3_02,

	/// <summary>
	/// Incarnation of Guan-Yu's Grace
	/// </summary>
	/// <remarks>
	/// True to the tenets of Taoism, even 
	/// Guan-Yu, the Taoist god of war, only 
	/// fights when all other options have been 
	/// exhausted. 
	/// Can enchant torso armor only.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\03_ACT3_GUANYUSGRACE.DBR")]
	[GearType(GearType.Torso)]
	GUANYUSGRACE_ACT3_03,

	/// <summary>
	/// Hag's Skin
	/// </summary>
	/// <remarks>
	/// A scrap of skin from a Desert Hag.  
	/// Can enhance all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\01_ACT2_HAGSSKIN.DBR")]
	[GearType(GearType.AllArmor)]
	HAGSSKIN_ACT2_01,

	/// <summary>
	/// Epic Hag's Skin
	/// </summary>
	/// <remarks>
	/// A scrap of skin from a Desert Hag.  
	/// Can enhance all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\02_ACT2_HAGSSKIN.DBR")]
	[GearType(GearType.AllArmor)]
	HAGSSKIN_ACT2_02,

	/// <summary>
	/// Legendary Hag's Skin
	/// </summary>
	/// <remarks>
	/// A scrap of skin from a Desert Hag.  
	/// Can enhance all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\03_ACT2_HAGSSKIN.DBR")]
	[GearType(GearType.AllArmor)]
	HAGSSKIN_ACT2_03,

	/// <summary>
	/// Essence of Hecate's Crescent
	/// </summary>
	/// <remarks>
	/// Hecate is a mysterious Greek goddess 
	/// associated with the moon, childbirth, 
	/// and magic. 
	/// Can enchant rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\01_ACT1_HECATESCRESCENT.DBR")]
	[GearType(GearType.Jewellery)]
	HECATESCRESCENT_ACT1_01,

	/// <summary>
	/// Embodiment of Hecate's Crescent
	/// </summary>
	/// <remarks>
	/// Hecate is a mysterious Greek goddess 
	/// associated with the moon, childbirth, 
	/// and magic. 
	/// Can enchant rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\02_ACT1_HECATESCRESCENT.DBR")]
	[GearType(GearType.Jewellery)]
	HECATESCRESCENT_ACT1_02,

	/// <summary>
	/// Incarnation of Hecate's Crescent
	/// </summary>
	/// <remarks>
	/// Hecate is a mysterious Greek goddess 
	/// associated with the moon, childbirth, 
	/// and magic. 
	/// Can enchant rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\03_ACT1_HECATESCRESCENT.DBR")]
	[GearType(GearType.Jewellery)]
	HECATESCRESCENT_ACT1_03,

	/// <summary>
	/// Essence of Herakles' Might
	/// </summary>
	/// <remarks>
	/// Herakles was the strongest of all the 
	/// Greek heroes. 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\01_ACT1_HERACLESMIGHT.DBR")]
	[GearType(GearType.AllArmor)]
	HERACLESMIGHT_ACT1_01,

	/// <summary>
	/// Embodiment of Herakles' Might
	/// </summary>
	/// <remarks>
	/// Herakles was the strongest of all the 
	/// Greek heroes. 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\02_ACT1_HERACLESMIGHT.DBR")]
	[GearType(GearType.AllArmor)]
	HERACLESMIGHT_ACT1_02,

	/// <summary>
	/// Incarnation of Herakles' Might
	/// </summary>
	/// <remarks>
	/// Herakles was the strongest of all the 
	/// Greek heroes. 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\03_ACT1_HERACLESMIGHT.DBR")]
	[GearType(GearType.AllArmor)]
	HERACLESMIGHT_ACT1_03,

	/// <summary>
	/// Essence of Hermes' Sandal
	/// </summary>
	/// <remarks>
	/// Hermes the Messenger is the swiftest of 
	/// the Greek gods. He is known for his 
	/// iconic winged sandals. 
	/// Can enchant leg armor only.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\01_ACT1_HERMESSANDAL.DBR")]
	[GearType(GearType.Leg)]
	HERMESSANDAL_ACT1_01,

	/// <summary>
	/// Embodiment of Hermes' Sandal
	/// </summary>
	/// <remarks>
	/// Hermes the Messenger is the swiftest of 
	/// the Greek gods. He is known for his 
	/// iconic winged sandals. 
	/// Can enchant leg armor only.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\02_ACT1_HERMESSANDAL.DBR")]
	[GearType(GearType.Leg)]
	HERMESSANDAL_ACT1_02,

	/// <summary>
	/// Incarnation of Hermes' Sandal
	/// </summary>
	/// <remarks>
	/// Hermes the Messenger is the swiftest of 
	/// the Greek gods. He is known for his 
	/// iconic winged sandals. 
	/// Can enchant leg armor only.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\03_ACT1_HERMESSANDAL.DBR")]
	[GearType(GearType.Leg)]
	HERMESSANDAL_ACT1_03,

	/// <summary>
	/// Essence of the Star of the Evening
	/// </summary>
	/// <remarks>
	/// Hesperos, brother of Atlas, is the star 
	/// of the magical time of dusk and 
	/// progenitor of the Hesperides. 
	/// Can enhance rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\XPACK3\ITEMS\RELICS\01_X3_HESPERIDES.DBR")]
	[GearType(GearType.Jewellery)]
	HESPERIDES_X3_01,

	/// <summary>
	/// Embodiment of the Star of the Evening
	/// </summary>
	/// <remarks>
	/// Hesperos, brother of Atlas, is the star 
	/// of the magical time of dusk and 
	/// progenitor of the Hesperides. 
	/// Can enhance rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\XPACK3\ITEMS\RELICS\02_X3_HESPERIDES.DBR")]
	[GearType(GearType.Jewellery)]
	HESPERIDES_X3_02,

	/// <summary>
	/// Incarnation of the Star of the Evening
	/// </summary>
	/// <remarks>
	/// Hesperos, brother of Atlas, is the star 
	/// of the magical time of dusk and 
	/// progenitor of the Hesperides. 
	/// Can enhance rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\XPACK3\ITEMS\RELICS\03_X3_HESPERIDES.DBR")]
	[GearType(GearType.Jewellery)]
	HESPERIDES_X3_03,

	/// <summary>
	/// Hydradon Hide
	/// </summary>
	/// <remarks>
	/// A patch of rough, durable skin ripped 
	/// from the flesh of a Hydradon. 
	/// Can enhance all armor.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\CHARMS\01_ACT4_HYDRADONHIDE.DBR")]
	[GearType(GearType.AllArmor)]
	HYDRADONHIDE_ACT4_01,

	/// <summary>
	/// Epic Hydradon Hide
	/// </summary>
	/// <remarks>
	/// A patch of rough, durable skin ripped 
	/// from the flesh of a Hydradon. 
	/// Can enhance all armor.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\CHARMS\02_ACT4_HYDRADONHIDE.DBR")]
	[GearType(GearType.AllArmor)]
	HYDRADONHIDE_ACT4_02,

	/// <summary>
	/// Legendary Hydradon Hide
	/// </summary>
	/// <remarks>
	/// A patch of rough, durable skin ripped 
	/// from the flesh of a Hydradon. 
	/// Can enhance all armor.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\CHARMS\03_ACT4_HYDRADONHIDE.DBR")]
	[GearType(GearType.AllArmor)]
	HYDRADONHIDE_ACT4_03,

	/// <summary>
	/// Membrane Wings
	/// </summary>
	/// <remarks>
	/// The silken wings of a large insect are a 
	/// popular dark magic ingredient. 
	/// Can enchant amulets, staves or head 
	/// armor.
	/// </remarks>
	[Description(@"RECORDS\XPACK3\ITEMS\CHARMS\01_X3_INSECTWINGS.DBR")]
	[GearType(GearType.Amulet | GearType.Staff | GearType.Head)]
	INSECTWINGS_X3_01,

	/// <summary>
	/// Epic Membrane Wings
	/// </summary>
	/// <remarks>
	/// The silken wings of a large insect are a 
	/// popular dark magic ingredient. 
	/// Can enchant amulets, staves or head 
	/// armor.
	/// </remarks>
	[Description(@"RECORDS\XPACK3\ITEMS\CHARMS\02_X3_INSECTWINGS.DBR")]
	[GearType(GearType.Amulet | GearType.Staff | GearType.Head)]
	INSECTWINGS_X3_02,

	/// <summary>
	/// Legendary Membrane Wings
	/// </summary>
	/// <remarks>
	/// The silken wings of a large insect are a 
	/// popular dark magic ingredient. 
	/// Can enchant amulets, staves or head 
	/// armor.
	/// </remarks>
	[Description(@"RECORDS\XPACK3\ITEMS\CHARMS\03_X3_INSECTWINGS.DBR")]
	[GearType(GearType.Amulet | GearType.Staff | GearType.Head)]
	INSECTWINGS_X3_03,

	/// <summary>
	/// Essence of the Iron Will of Ajax
	/// </summary>
	/// <remarks>
	/// Numbered among the greatest of the 
	/// heroes of the Trojan war, Ajax was known 
	/// not only for the strength of his arm, 
	/// but even more for his indomitable 
	/// spirit. 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\RELICS\01_ACT4_IRONWILLOFAJAX.DBR")]
	[GearType(GearType.AllArmor)]
	IRONWILLOFAJAX_ACT4_01,

	/// <summary>
	/// Embodiment of the Iron Will of Ajax
	/// </summary>
	/// <remarks>
	/// Numbered among the greatest of the 
	/// heroes of the Trojan war, Ajax was known 
	/// not only for the strength of his arm, 
	/// but even more for his indomitable 
	/// spirit. 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\RELICS\02_ACT4_IRONWILLOFAJAX.DBR")]
	[GearType(GearType.AllArmor)]
	IRONWILLOFAJAX_ACT4_02,

	/// <summary>
	/// Incarnation of the Iron Will of Ajax
	/// </summary>
	/// <remarks>
	/// Numbered among the greatest of the 
	/// heroes of the Trojan war, Ajax was known 
	/// not only for the strength of his arm, 
	/// but even more for his indomitable 
	/// spirit. 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\RELICS\03_ACT4_IRONWILLOFAJAX.DBR")]
	[GearType(GearType.AllArmor)]
	IRONWILLOFAJAX_ACT4_03,

	/// <summary>
	/// Essence of the Jade Emperor's Serenity
	/// </summary>
	/// <remarks>
	/// The Jade Emperor is the ruler of the 
	/// Chinese gods. He is a contemplative 
	/// deity who rarely interferes with the 
	/// affairs of god or man. 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\01_ACT3_JADEEMPERORSSERENITY.DBR")]
	[GearType(GearType.AllArmor)]
	JADEEMPERORSSERENITY_ACT3_01,

	/// <summary>
	/// Embodiment of the Jade Emperor's Serenity
	/// </summary>
	/// <remarks>
	/// The Jade Emperor is the ruler of the 
	/// Chinese gods. He is a contemplative 
	/// deity who rarely interferes with the 
	/// affairs of god or man. 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\02_ACT3_JADEEMPERORSSERENITY.DBR")]
	[GearType(GearType.AllArmor)]
	JADEEMPERORSSERENITY_ACT3_02,

	/// <summary>
	/// Incarnation of the Jade Emperor's Serenity
	/// </summary>
	/// <remarks>
	/// The Jade Emperor is the ruler of the 
	/// Chinese gods. He is a contemplative 
	/// deity who rarely interferes with the 
	/// affairs of god or man. 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\03_ACT3_JADEEMPERORSSERENITY.DBR")]
	[GearType(GearType.AllArmor)]
	JADEEMPERORSSERENITY_ACT3_03,

	/// <summary>
	/// Essence of the Light of Belenus
	/// </summary>
	/// <remarks>
	/// Belenus is the Celtic god of the sun and 
	/// the patron of healing springs. 
	/// Can enchant shields and amulets.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\RELICS\01_ACT5_LIGHTOFBELENUS.DBR")]
	[GearType(GearType.Shield | GearType.Amulet)]
	LIGHTOFBELENUS_ACT5_01,

	/// <summary>
	/// Embodiment of the Light of Belenus
	/// </summary>
	/// <remarks>
	/// Belenus is the Celtic god of the sun and 
	/// the patron of healing springs. 
	/// Can enchant shields and amulets.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\RELICS\02_ACT5_LIGHTOFBELENUS.DBR")]
	[GearType(GearType.Shield | GearType.Amulet)]
	LIGHTOFBELENUS_ACT5_02,

	/// <summary>
	/// Incarnation of the Light of Belenus
	/// </summary>
	/// <remarks>
	/// Belenus is the Celtic god of the sun and 
	/// the patron of healing springs. 
	/// Can enchant shields and amulets.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\RELICS\03_ACT5_LIGHTOFBELENUS.DBR")]
	[GearType(GearType.Shield | GearType.Amulet)]
	LIGHTOFBELENUS_ACT5_03,

	/// <summary>
	/// Essence of Li-Nezha's Guile
	/// </summary>
	/// <remarks>
	/// Li-Nezha, a Chinese trickster god, is as 
	/// nimble as he is crafty. 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\01_ACT3_LINEZHASGUILE.DBR")]
	[GearType(GearType.AllArmor)]
	LINEZHASGUILE_ACT3_01,

	/// <summary>
	/// Embodiment of Li-Nezha's Guile
	/// </summary>
	/// <remarks>
	/// Li-Nezha, a Chinese trickster god, is as 
	/// nimble as he is crafty. 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\02_ACT3_LINEZHASGUILE.DBR")]
	[GearType(GearType.AllArmor)]
	LINEZHASGUILE_ACT3_02,

	/// <summary>
	/// Incarnation of Li-Nezha's Guile
	/// </summary>
	/// <remarks>
	/// Li-Nezha, a Chinese trickster god, is as 
	/// nimble as he is crafty. 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\03_ACT3_LINEZHASGUILE.DBR")]
	[GearType(GearType.AllArmor)]
	LINEZHASGUILE_ACT3_03,

	/// <summary>
	/// Lupine Claw
	/// </summary>
	/// <remarks>
	/// A long, sharp claw.  
	/// Can enhance all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\01_ACT2_LUPINECLAW.DBR")]
	[GearType(GearType.AllWeapons)]
	LUPINECLAW_ACT2_01,

	/// <summary>
	/// Epic Lupine Claw
	/// </summary>
	/// <remarks>
	/// A long, sharp claw.  
	/// Can enhance all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\02_ACT2_LUPINECLAW.DBR")]
	[GearType(GearType.AllWeapons)]
	LUPINECLAW_ACT2_02,

	/// <summary>
	/// Legendary Lupine Claw
	/// </summary>
	/// <remarks>
	/// A long, sharp claw.  
	/// Can enhance all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\03_ACT2_LUPINECLAW.DBR")]
	[GearType(GearType.AllWeapons)]
	LUPINECLAW_ACT2_03,

	/// <summary>
	/// Mechanical Parts
	/// </summary>
	/// <remarks>
	/// The intricate inner workings of an 
	/// automaton. 
	/// Can enhance torso armor only.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\01_MULTACTS_MECHANICALPARTS.DBR")]
	[GearType(GearType.Torso)]
	MECHANICALPARTS_MULTACTS_01,

	/// <summary>
	/// Epic Mechanical Parts
	/// </summary>
	/// <remarks>
	/// The intricate inner workings of an 
	/// automaton. 
	/// Can enhance torso armor only.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\02_MULTACTS_MECHANICALPARTS.DBR")]
	[GearType(GearType.Torso)]
	MECHANICALPARTS_MULTACTS_02,

	/// <summary>
	/// Legendary Mechanical Parts
	/// </summary>
	/// <remarks>
	/// The intricate inner workings of an 
	/// automaton. 
	/// Can enhance torso armor only.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\03_MULTACTS_MECHANICALPARTS.DBR")]
	[GearType(GearType.Torso)]
	MECHANICALPARTS_MULTACTS_03,

	/// <summary>
	/// Essence of the Monkey King's Trickery
	/// </summary>
	/// <remarks>
	/// The Monkey King, the most mischievous of 
	/// all the Chinese gods, is also one of the 
	/// most powerful. His cunning has lured 
	/// many an unwary foe to do his bidding. 
	/// Can enchant rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\01_ACT3_MONKEYKINGSTRICKERY.DBR")]
	[GearType(GearType.Jewellery)]
	MONKEYKINGSTRICKERY_ACT3_01,

	/// <summary>
	/// Embodiment of the Monkey King's Trickery
	/// </summary>
	/// <remarks>
	/// The Monkey King, the most mischievous of 
	/// all the Chinese gods, is also one of the 
	/// most powerful. His cunning has lured 
	/// many an unwary foe to do his bidding. 
	/// Can enchant rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\02_ACT3_MONKEYKINGSTRICKERY.DBR")]
	[GearType(GearType.Jewellery)]
	MONKEYKINGSTRICKERY_ACT3_02,

	/// <summary>
	/// Incarnation of the Monkey King's Trickery
	/// </summary>
	/// <remarks>
	/// The Monkey King, the most mischievous of 
	/// all the Chinese gods, is also one of the 
	/// most powerful. His cunning has lured 
	/// many an unwary foe to do his bidding. 
	/// Can enchant rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\03_ACT3_MONKEYKINGSTRICKERY.DBR")]
	[GearType(GearType.Jewellery)]
	MONKEYKINGSTRICKERY_ACT3_03,

	/// <summary>
	/// Essence of the Power of Nerthus
	/// </summary>
	/// <remarks>
	/// A part of the mistletoe infused with the 
	/// power of the pagan earth goddess. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\RELICS\01_ACT5_NERTHUSMISTLETOE.DBR")]
	[GearType(GearType.AllWeapons)]
	NERTHUSMISTLETOE_ACT5_01,

	/// <summary>
	/// Embodiment of the Power of Nerthus
	/// </summary>
	/// <remarks>
	/// A part of the mistletoe infused with the 
	/// power of the pagan earth goddess. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\RELICS\02_ACT5_NERTHUSMISTLETOE.DBR")]
	[GearType(GearType.AllWeapons)]
	NERTHUSMISTLETOE_ACT5_02,

	/// <summary>
	/// Incarnation of the Power of Nerthus
	/// </summary>
	/// <remarks>
	/// A part of the mistletoe infused with the 
	/// power of the pagan earth goddess. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\RELICS\03_ACT5_NERTHUSMISTLETOE.DBR")]
	[GearType(GearType.AllWeapons)]
	NERTHUSMISTLETOE_ACT5_03,

	/// <summary>
	/// Essence of Ogmios' Eloquence
	/// </summary>
	/// <remarks>
	/// The Celtic god Ogmios uses the power of 
	/// persuasion to turn others into his 
	/// willing servants. 
	/// Can enchant amulets or head armor.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\RELICS\01_ACT5_OGMIOSELOQUENCE.DBR")]
	[GearType(GearType.Amulet | GearType.Head)]
	OGMIOSELOQUENCE_ACT5_01,

	/// <summary>
	/// Embodiment of Ogmios' Eloquence
	/// </summary>
	/// <remarks>
	/// The Celtic god Ogmios uses the power of 
	/// persuasion to turn others into his 
	/// willing servants. 
	/// Can enchant amulets or head armor.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\RELICS\02_ACT5_OGMIOSELOQUENCE.DBR")]
	[GearType(GearType.Amulet | GearType.Head)]
	OGMIOSELOQUENCE_ACT5_02,

	/// <summary>
	/// Incarnation of Ogmios' Eloquence
	/// </summary>
	/// <remarks>
	/// The Celtic god Ogmios uses the power of 
	/// persuasion to turn others into his 
	/// willing servants. 
	/// Can enchant amulets or head armor.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\RELICS\03_ACT5_OGMIOSELOQUENCE.DBR")]
	[GearType(GearType.Amulet | GearType.Head)]
	OGMIOSELOQUENCE_ACT5_03,

	/// <summary>
	/// Essence of the Children of Okeanos
	/// </summary>
	/// <remarks>
	/// Okeanos is the primordial, neutral titan 
	/// representing all water, and father of 
	/// over three thousand Okeanides. 
	/// Can enchant torso armor or shields.
	/// </remarks>
	[Description(@"RECORDS\XPACK3\ITEMS\RELICS\01_X3_OKEANOS.DBR")]
	[GearType(GearType.Torso | GearType.Shield)]
	OKEANOS_X3_01,

	/// <summary>
	/// Embodiment of the Children of Okeanos
	/// </summary>
	/// <remarks>
	/// Okeanos is the primordial, neutral titan 
	/// representing all water, and father of 
	/// over three thousand Okeanides. 
	/// Can enchant torso armor or shields.
	/// </remarks>
	[Description(@"RECORDS\XPACK3\ITEMS\RELICS\02_X3_OKEANOS.DBR")]
	[GearType(GearType.Torso | GearType.Shield)]
	OKEANOS_X3_02,

	/// <summary>
	/// Incarnation of the Children of Okeanos
	/// </summary>
	/// <remarks>
	/// Okeanos is the primordial, neutral titan 
	/// representing all water, and father of 
	/// over three thousand Okeanides. 
	/// Can enchant torso armor or shields.
	/// </remarks>
	[Description(@"RECORDS\XPACK3\ITEMS\RELICS\03_X3_OKEANOS.DBR")]
	[GearType(GearType.Torso | GearType.Shield)]
	OKEANOS_X3_03,

	/// <summary>
	/// Peng Claw
	/// </summary>
	/// <remarks>
	/// Claw of a Peng.  
	/// Can enhance all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\01_ACT3_PENGCLAW.DBR")]
	[GearType(GearType.AllWeapons)]
	PENGCLAW_ACT3_01,

	/// <summary>
	/// Epic Peng Claw
	/// </summary>
	/// <remarks>
	/// Claw of a Peng.  
	/// Can enhance all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\02_ACT3_PENGCLAW.DBR")]
	[GearType(GearType.AllWeapons)]
	PENGCLAW_ACT3_02,

	/// <summary>
	/// Legendary Peng Claw
	/// </summary>
	/// <remarks>
	/// Claw of a Peng.  
	/// Can enhance all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\03_ACT3_PENGCLAW.DBR")]
	[GearType(GearType.AllWeapons)]
	PENGCLAW_ACT3_03,

	/// <summary>
	/// Essence of Persephone's Tears
	/// </summary>
	/// <remarks>
	/// Persephone, favored daughter of the 
	/// earth goddess Demeter, was forced into 
	/// marriage with Hades. She now shares rule 
	/// of the underworld, yet it brings her 
	/// only sorrow. 
	/// Can enchant rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\RELICS\01_ACT4_PERSEPHONESTEARS.DBR")]
	[GearType(GearType.Jewellery)]
	PERSEPHONESTEARS_ACT4_01,

	/// <summary>
	/// Embodiment of Persephone's Tears
	/// </summary>
	/// <remarks>
	/// Persephone, favored daughter of the 
	/// earth goddess Demeter, was forced into 
	/// marriage with Hades. She now shares rule 
	/// of the underworld, yet it brings her 
	/// only sorrow. 
	/// Can enchant rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\RELICS\02_ACT4_PERSEPHONESTEARS.DBR")]
	[GearType(GearType.Jewellery)]
	PERSEPHONESTEARS_ACT4_02,

	/// <summary>
	/// Incarnation of Persephone's Tears
	/// </summary>
	/// <remarks>
	/// Persephone, favored daughter of the 
	/// earth goddess Demeter, was forced into 
	/// marriage with Hades. She now shares rule 
	/// of the underworld, yet it brings her 
	/// only sorrow. 
	/// Can enchant rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\RELICS\03_ACT4_PERSEPHONESTEARS.DBR")]
	[GearType(GearType.Jewellery)]
	PERSEPHONESTEARS_ACT4_03,

	/// <summary>
	/// Essence of Poseidon's Trident
	/// </summary>
	/// <remarks>
	/// The Cyclopes who crafted Zeus' 
	/// Thunderbolt also gifted his brother with 
	/// a magical trident. 
	/// Can enhance arm armor only.
	/// </remarks>
	[Description(@"RECORDS\XPACK3\ITEMS\RELICS\01_X3_POSEIDONSTRIDENT.DBR")]
	[GearType(GearType.Arm)]
	POSEIDONSTRIDENT_X3_01,

	/// <summary>
	/// Embodiment of Poseidon's Trident
	/// </summary>
	/// <remarks>
	/// The Cyclopes who crafted Zeus' 
	/// Thunderbolt also gifted his brother with 
	/// a magical trident. 
	/// Can enhance arm armor only.
	/// </remarks>
	[Description(@"RECORDS\XPACK3\ITEMS\RELICS\02_X3_POSEIDONSTRIDENT.DBR")]
	[GearType(GearType.Arm)]
	POSEIDONSTRIDENT_X3_02,

	/// <summary>
	/// Incarnation of Poseidon's Trident
	/// </summary>
	/// <remarks>
	/// The Cyclopes who crafted Zeus' 
	/// Thunderbolt also gifted his brother with 
	/// a magical trident. 
	/// Can enhance arm armor only.
	/// </remarks>
	[Description(@"RECORDS\XPACK3\ITEMS\RELICS\03_X3_POSEIDONSTRIDENT.DBR")]
	[GearType(GearType.Arm)]
	POSEIDONSTRIDENT_X3_03,

	/// <summary>
	/// Primal Magma
	/// </summary>
	/// <remarks>
	/// Magically animated lava taken from a 
	/// creature of fire. 
	/// Can enhance arm armor only.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\CHARMS\01_ACT5_PRIMALMAGMA.DBR")]
	[GearType(GearType.Arm)]
	PRIMALMAGMA_ACT5_01,

	/// <summary>
	/// Epic Primal Magma
	/// </summary>
	/// <remarks>
	/// Magically animated lava taken from a 
	/// creature of fire. 
	/// Can enhance arm armor only.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\CHARMS\02_ACT5_PRIMALMAGMA.DBR")]
	[GearType(GearType.Arm)]
	PRIMALMAGMA_ACT5_02,

	/// <summary>
	/// Legendary Primal Magma
	/// </summary>
	/// <remarks>
	/// Magically animated lava taken from a 
	/// creature of fire. 
	/// Can enhance arm armor only.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\CHARMS\03_ACT5_PRIMALMAGMA.DBR")]
	[GearType(GearType.Arm)]
	PRIMALMAGMA_ACT5_03,

	/// <summary>
	/// Pristine Plumage
	/// </summary>
	/// <remarks>
	/// High quality feathers.  
	/// Can enhance all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\01_ACT1_PRISTINEPLUMAGE.DBR")]
	[GearType(GearType.AllArmor)]
	PRISTINEPLUMAGE_ACT1_01,

	/// <summary>
	/// Epic Pristine Plumage
	/// </summary>
	/// <remarks>
	/// High quality feathers.  
	/// Can enhance all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\02_ACT1_PRISTINEPLUMAGE.DBR")]
	[GearType(GearType.AllArmor)]
	PRISTINEPLUMAGE_ACT1_02,

	/// <summary>
	/// Legendary Pristine Plumage
	/// </summary>
	/// <remarks>
	/// High quality feathers.  
	/// Can enhance all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\03_ACT1_PRISTINEPLUMAGE.DBR")]
	[GearType(GearType.AllArmor)]
	PRISTINEPLUMAGE_ACT1_03,

	/// <summary>
	/// Essence of Prometheus' Flame
	/// </summary>
	/// <remarks>
	/// Prometheus, son of the titan Iapetus, 
	/// gave the gift of fire to mankind. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\01_ACT1_PROMETHEUSFLAME.DBR")]
	[GearType(GearType.AllWeapons)]
	PROMETHEUSFLAME_ACT1_01,

	/// <summary>
	/// Embodiment of Prometheus' Flame
	/// </summary>
	/// <remarks>
	/// Prometheus, son of the titan Iapetus, 
	/// gave the gift of fire to mankind. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\02_ACT1_PROMETHEUSFLAME.DBR")]
	[GearType(GearType.AllWeapons)]
	PROMETHEUSFLAME_ACT1_02,

	/// <summary>
	/// Incarnation of Prometheus' Flame
	/// </summary>
	/// <remarks>
	/// Prometheus, son of the titan Iapetus, 
	/// gave the gift of fire to mankind. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\03_ACT1_PROMETHEUSFLAME.DBR")]
	[GearType(GearType.AllWeapons)]
	PROMETHEUSFLAME_ACT1_03,

	/// <summary>
	/// Essence of the Rage of Ares
	/// </summary>
	/// <remarks>
	/// Ares, the Greek god of war, is venerated 
	/// and feared in equal measure. He is 
	/// cruel, vengeful, and nearly mad - only a 
	/// fool would risk his wrath. 
	/// Can enchant armbands and bracelets.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\RELICS\01_ACT4_RAGEOFARES.DBR")]
	[GearType(GearType.Arm)]
	RAGEOFARES_ACT4_01,

	/// <summary>
	/// Embodiment of the Rage of Ares
	/// </summary>
	/// <remarks>
	/// Ares, the Greek god of war, is venerated 
	/// and feared in equal measure. He is 
	/// cruel, vengeful, and nearly mad - only a 
	/// fool would risk his wrath. 
	/// Can enchant armbands and bracelets.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\RELICS\02_ACT4_RAGEOFARES.DBR")]
	[GearType(GearType.Arm)]
	RAGEOFARES_ACT4_02,

	/// <summary>
	/// Incarnation of the Rage of Ares
	/// </summary>
	/// <remarks>
	/// Ares, the Greek god of war, is venerated 
	/// and feared in equal measure. He is 
	/// cruel, vengeful, and nearly mad - only a 
	/// fool would risk his wrath. 
	/// Can enchant armbands and bracelets.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\RELICS\03_ACT4_RAGEOFARES.DBR")]
	[GearType(GearType.Arm)]
	RAGEOFARES_ACT4_03,

	/// <summary>
	/// Raptor Tooth
	/// </summary>
	/// <remarks>
	/// Sharp, jagged tooth torn from a Raptor's 
	/// mouth. 
	/// Can enhance amulets only.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\01_ACT3_RAPTORTOOTH.DBR")]
	[GearType(GearType.Amulet)]
	RAPTORTOOTH_ACT3_01,

	/// <summary>
	/// Epic Raptor Tooth
	/// </summary>
	/// <remarks>
	/// Sharp, jagged tooth torn from a Raptor's 
	/// mouth. 
	/// Can enhance amulets only.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\02_ACT3_RAPTORTOOTH.DBR")]
	[GearType(GearType.Amulet)]
	RAPTORTOOTH_ACT3_02,

	/// <summary>
	/// Legendary Raptor Tooth
	/// </summary>
	/// <remarks>
	/// Sharp, jagged tooth torn from a Raptor's 
	/// mouth. 
	/// Can enhance amulets only.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\03_ACT3_RAPTORTOOTH.DBR")]
	[GearType(GearType.Amulet)]
	RAPTORTOOTH_ACT3_03,

	/// <summary>
	/// Rigid Carapace
	/// </summary>
	/// <remarks>
	/// The thick outer shell of an insectoid 
	/// monster. 
	/// Can enhance shields only.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\01_MULTACTS_RIGIDCARAPACE.DBR")]
	[GearType(GearType.Shield)]
	RIGIDCARAPACE_MULTACTS_01,

	/// <summary>
	/// Epic Rigid Carapace
	/// </summary>
	/// <remarks>
	/// The thick outer shell of an insectoid 
	/// monster. 
	/// Can enhance shields only.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\02_MULTACTS_RIGIDCARAPACE.DBR")]
	[GearType(GearType.Shield)]
	RIGIDCARAPACE_MULTACTS_02,

	/// <summary>
	/// Legendary Rigid Carapace
	/// </summary>
	/// <remarks>
	/// The thick outer shell of an insectoid 
	/// monster. 
	/// Can enhance shields only.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\03_MULTACTS_RIGIDCARAPACE.DBR")]
	[GearType(GearType.Shield)]
	RIGIDCARAPACE_MULTACTS_03,

	/// <summary>
	/// Saber Claw
	/// </summary>
	/// <remarks>
	/// The razor-sharp claw of a Saber Lion.  
	/// Can enhance all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\01_ACT3_SABERCLAW.DBR")]
	[GearType(GearType.AllWeapons)]
	SABERCLAW_ACT3_01,

	/// <summary>
	/// Epic Saber Claw
	/// </summary>
	/// <remarks>
	/// The razor-sharp claw of a Saber Lion.  
	/// Can enhance all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\02_ACT3_SABERCLAW.DBR")]
	[GearType(GearType.AllWeapons)]
	SABERCLAW_ACT3_02,

	/// <summary>
	/// Legendary Saber Claw
	/// </summary>
	/// <remarks>
	/// The razor-sharp claw of a Saber Lion.  
	/// Can enhance all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\03_ACT3_SABERCLAW.DBR")]
	[GearType(GearType.AllWeapons)]
	SABERCLAW_ACT3_03,

	/// <summary>
	/// Essence of Set's Betrayal
	/// </summary>
	/// <remarks>
	/// Set is the Egyptian god of evil. In an 
	/// attempt to take control of the throne of 
	/// the gods, Set turned on his brother 
	/// Osiris, brutally hacking the latter's 
	/// body apart and scattering the pieces 
	/// across the land. 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\01_ACT2_SETSBETRAYAL.DBR")]
	[GearType(GearType.AllArmor)]
	SETSBETRAYAL_ACT2_01,

	/// <summary>
	/// Embodiment of Set's Betrayal
	/// </summary>
	/// <remarks>
	/// Set is the Egyptian god of evil. In an 
	/// attempt to take control of the throne of 
	/// the gods, Set turned on his brother 
	/// Osiris, brutally hacking the latter's 
	/// body apart and scattering the pieces 
	/// across the land. 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\02_ACT2_SETSBETRAYAL.DBR")]
	[GearType(GearType.AllArmor)]
	SETSBETRAYAL_ACT2_02,

	/// <summary>
	/// Incarnation of Set's Betrayal
	/// </summary>
	/// <remarks>
	/// Set is the Egyptian god of evil. In an 
	/// attempt to take control of the throne of 
	/// the gods, Set turned on his brother 
	/// Osiris, brutally hacking the latter's 
	/// body apart and scattering the pieces 
	/// across the land. 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\03_ACT2_SETSBETRAYAL.DBR")]
	[GearType(GearType.AllArmor)]
	SETSBETRAYAL_ACT2_03,

	/// <summary>
	/// Essence of the Shade of Hector
	/// </summary>
	/// <remarks>
	/// Hector, prince of Troy, was renowned for 
	/// his noble spirit and steadfast nature. 
	/// He fell defending his homeland, mourned 
	/// even by his enemies. 
	/// Can enchant all shields.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\RELICS\01_ACT4_SHADEOFHEKTOR.DBR")]
	[GearType(GearType.Shield)]
	SHADEOFHEKTOR_ACT4_01,

	/// <summary>
	/// Embodiment of the Shade of Hector
	/// </summary>
	/// <remarks>
	/// Hector, prince of Troy, was renowned for 
	/// his noble spirit and steadfast nature. 
	/// He fell defending his homeland, mourned 
	/// even by his enemies. 
	/// Can enchant all shields.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\RELICS\02_ACT4_SHADEOFHEKTOR.DBR")]
	[GearType(GearType.Shield)]
	SHADEOFHEKTOR_ACT4_02,

	/// <summary>
	/// Incarnation of the Shade of Hector
	/// </summary>
	/// <remarks>
	/// Hector, prince of Troy, was renowned for 
	/// his noble spirit and steadfast nature. 
	/// He fell defending his homeland, mourned 
	/// even by his enemies. 
	/// Can enchant all shields.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\RELICS\03_ACT4_SHADEOFHEKTOR.DBR")]
	[GearType(GearType.Shield)]
	SHADEOFHEKTOR_ACT4_03,

	/// <summary>
	/// Essence of Shen-Nong's Dark Medicine
	/// </summary>
	/// <remarks>
	/// Shen-Nong is the Chinese god of plants 
	/// and medicine. He is a master of herbs - 
	/// and not all are meant to cure. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\01_ACT3_SHENNONGSDARKMEDICINE.DBR")]
	[GearType(GearType.AllWeapons)]
	SHENNONGSDARKMEDICINE_ACT3_01,

	/// <summary>
	/// Embodiment of Shen-Nong's Dark Medicine
	/// </summary>
	/// <remarks>
	/// Shen-Nong is the Chinese god of plants 
	/// and medicine. He is a master of herbs - 
	/// and not all are meant to cure. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\02_ACT3_SHENNONGSDARKMEDICINE.DBR")]
	[GearType(GearType.AllWeapons)]
	SHENNONGSDARKMEDICINE_ACT3_02,

	/// <summary>
	/// Incarnation of Shen-Nong's Dark Medicine
	/// </summary>
	/// <remarks>
	/// Shen-Nong is the Chinese god of plants 
	/// and medicine. He is a master of herbs - 
	/// and not all are meant to cure. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\03_ACT3_SHENNONGSDARKMEDICINE.DBR")]
	[GearType(GearType.AllWeapons)]
	SHENNONGSDARKMEDICINE_ACT3_03,

	/// <summary>
	/// Essence of Sigurd's Courage
	/// </summary>
	/// <remarks>
	/// Sigurd the dragon slayer was the hero of 
	/// the Völsung clan, famous for his bravery 
	/// and ambition. 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\RELICS\01_ACT5_SIGURDSCOURAGE.DBR")]
	[GearType(GearType.AllArmor)]
	SIGURDSCOURAGE_ACT5_01,

	/// <summary>
	/// Embodiment of Sigurd's Courage
	/// </summary>
	/// <remarks>
	/// Sigurd the dragon slayer was the hero of 
	/// the Völsung clan, famous for his bravery 
	/// and ambition. 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\RELICS\02_ACT5_SIGURDSCOURAGE.DBR")]
	[GearType(GearType.AllArmor)]
	SIGURDSCOURAGE_ACT5_02,

	/// <summary>
	/// Incarnation of Sigurd's Courage
	/// </summary>
	/// <remarks>
	/// Sigurd the dragon slayer was the hero of 
	/// the Völsung clan, famous for his bravery 
	/// and ambition. 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\RELICS\03_ACT5_SIGURDSCOURAGE.DBR")]
	[GearType(GearType.AllArmor)]
	SIGURDSCOURAGE_ACT5_03,

	/// <summary>
	/// Spectral Matter
	/// </summary>
	/// <remarks>
	/// A wispy, glowing substance.  
	/// Can enhance rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\01_MULTACTS_SPECTRALMATTER.DBR")]
	[GearType(GearType.Jewellery)]
	SPECTRALMATTER_MULTACTS_01,

	/// <summary>
	/// Epic Spectral Matter
	/// </summary>
	/// <remarks>
	/// A wispy, glowing substance.  
	/// Can enhance rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\02_MULTACTS_SPECTRALMATTER.DBR")]
	[GearType(GearType.Jewellery)]
	SPECTRALMATTER_MULTACTS_02,

	/// <summary>
	/// Legendary Spectral Matter
	/// </summary>
	/// <remarks>
	/// A wispy, glowing substance.  
	/// Can enhance rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\03_MULTACTS_SPECTRALMATTER.DBR")]
	[GearType(GearType.Jewellery)]
	SPECTRALMATTER_MULTACTS_03,

	/// <summary>
	/// Spiny Shell
	/// </summary>
	/// <remarks>
	/// The jagged shell of a Karkinos.  
	/// Can enhance shields.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\CHARMS\01_ACT4_SPINYSHELL.DBR")]
	[GearType(GearType.Shield)]
	SPINYSHELL_ACT4_01,

	/// <summary>
	/// Epic Spiny Shell
	/// </summary>
	/// <remarks>
	/// The jagged shell of a Karkinos.  
	/// Can enhance shields.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\CHARMS\02_ACT4_SPINYSHELL.DBR")]
	[GearType(GearType.Shield)]
	SPINYSHELL_ACT4_02,

	/// <summary>
	/// Legendary Spiny Shell
	/// </summary>
	/// <remarks>
	/// The jagged shell of a Karkinos.  
	/// Can enhance shields.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\CHARMS\03_ACT4_SPINYSHELL.DBR")]
	[GearType(GearType.Shield)]
	SPINYSHELL_ACT4_03,

	/// <summary>
	/// Essence of the Stew of Eldhrimnir
	/// </summary>
	/// <remarks>
	/// Every evening in Valhalla, the stew for 
	/// the Einherjar's feast is prepared in a 
	/// mighty cauldron out of the same 
	/// resurrecting beast. 
	/// Can enchant torso armor only.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\RELICS\01_ACT5_STEWOFELDHRIMNIR.DBR")]
	[GearType(GearType.Torso)]
	STEWOFELDHRIMNIR_ACT5_01,

	/// <summary>
	/// Embodiment of the Stew of Eldhrimnir
	/// </summary>
	/// <remarks>
	/// Every evening in Valhalla, the stew for 
	/// the Einherjar's feast is prepared in a 
	/// mighty cauldron out of the same 
	/// resurrecting beast. 
	/// Can enchant torso armor only.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\RELICS\02_ACT5_STEWOFELDHRIMNIR.DBR")]
	[GearType(GearType.Torso)]
	STEWOFELDHRIMNIR_ACT5_02,

	/// <summary>
	/// Incarnation of the Stew of Eldhrimnir
	/// </summary>
	/// <remarks>
	/// Every evening in Valhalla, the stew for 
	/// the Einherjar's feast is prepared in a 
	/// mighty cauldron out of the same 
	/// resurrecting beast. 
	/// Can enchant torso armor only.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\RELICS\03_ACT5_STEWOFELDHRIMNIR.DBR")]
	[GearType(GearType.Torso)]
	STEWOFELDHRIMNIR_ACT5_03,

	/// <summary>
	/// Tortured Soul
	/// </summary>
	/// <remarks>
	/// The agonized soul of a creature of 
	/// darkness. 
	/// Can enchant rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\CHARMS\01_ACT4_TORTUREDSOUL.DBR")]
	[GearType(GearType.Jewellery)]
	TORTUREDSOUL_ACT4_01,

	/// <summary>
	/// Epic Tortured Soul
	/// </summary>
	/// <remarks>
	/// The agonized soul of a creature of 
	/// darkness. 
	/// Can enchant rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\CHARMS\02_ACT4_TORTUREDSOUL.DBR")]
	[GearType(GearType.Jewellery)]
	TORTUREDSOUL_ACT4_02,

	/// <summary>
	/// Legendary Tortured Soul
	/// </summary>
	/// <remarks>
	/// The agonized soul of a creature of 
	/// darkness. 
	/// Can enchant rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\XPACK\ITEM\CHARMS\03_ACT4_TORTUREDSOUL.DBR")]
	[GearType(GearType.Jewellery)]
	TORTUREDSOUL_ACT4_03,

	/// <summary>
	/// Troll Tusks
	/// </summary>
	/// <remarks>
	/// The fearsome tusks of a great Troll.  
	/// Can enhance head armor only.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\CHARMS\01_ACT5_TROLLTUSKS.DBR")]
	[GearType(GearType.Head)]
	TROLLTUSKS_ACT5_01,

	/// <summary>
	/// Epic Troll Tusks
	/// </summary>
	/// <remarks>
	/// The fearsome tusks of a great Troll.  
	/// Can enhance head armor only.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\CHARMS\02_ACT5_TROLLTUSKS.DBR")]
	[GearType(GearType.Head)]
	TROLLTUSKS_ACT5_02,

	/// <summary>
	/// Legendary Troll Tusks
	/// </summary>
	/// <remarks>
	/// The fearsome tusks of a great Troll.  
	/// Can enhance head armor only.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\CHARMS\03_ACT5_TROLLTUSKS.DBR")]
	[GearType(GearType.Head)]
	TROLLTUSKS_ACT5_03,

	/// <summary>
	/// Turtle Shell
	/// </summary>
	/// <remarks>
	/// A large turtle shell.  
	/// Can enhance shields only.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\01_ACT1_TURTLESHELL.DBR")]
	[GearType(GearType.Shield)]
	TURTLESHELL_ACT1_01,

	/// <summary>
	/// Epic Turtle Shell
	/// </summary>
	/// <remarks>
	/// A large turtle shell.  
	/// Can enhance shields only.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\02_ACT1_TURTLESHELL.DBR")]
	[GearType(GearType.Shield)]
	TURTLESHELL_ACT1_02,

	/// <summary>
	/// Legendary Turtle Shell
	/// </summary>
	/// <remarks>
	/// A large turtle shell.  
	/// Can enhance shields only.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\03_ACT1_TURTLESHELL.DBR")]
	[GearType(GearType.Shield)]
	TURTLESHELL_ACT1_03,

	/// <summary>
	/// Essence of the Udjat of Horus
	/// </summary>
	/// <remarks>
	/// The Udjat is the eye of Horus, 
	/// hawk-headed god of the sky. It 
	/// symbolizes protection and power. 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\01_ACT2_UDJATOFHORUS.DBR")]
	[GearType(GearType.AllArmor)]
	UDJATOFHORUS_ACT2_01,

	/// <summary>
	/// Embodiment of the Udjat of Horus
	/// </summary>
	/// <remarks>
	/// The Udjat is the eye of Horus, 
	/// hawk-headed god of the sky. It 
	/// symbolizes protection and power. 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\02_ACT2_UDJATOFHORUS.DBR")]
	[GearType(GearType.AllArmor)]
	UDJATOFHORUS_ACT2_02,

	/// <summary>
	/// Incarnation of the Udjat of Horus
	/// </summary>
	/// <remarks>
	/// The Udjat is the eye of Horus, 
	/// hawk-headed god of the sky. It 
	/// symbolizes protection and power. 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\03_ACT2_UDJATOFHORUS.DBR")]
	[GearType(GearType.AllArmor)]
	UDJATOFHORUS_ACT2_03,

	/// <summary>
	/// Essence of the Valor of Achilles
	/// </summary>
	/// <remarks>
	/// Achilles was the greatest of all the 
	/// Greek warriors of the Trojan War. His 
	/// strength and speed in battle were 
	/// legendary. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\01_ACT1_VALOROFACHILLES.DBR")]
	[GearType(GearType.AllWeapons)]
	VALOROFACHILLES_ACT1_01,

	/// <summary>
	/// Embodiment of the Valor of Achilles
	/// </summary>
	/// <remarks>
	/// Achilles was the greatest of all the 
	/// Greek warriors of the Trojan War. His 
	/// strength and speed in battle were 
	/// legendary. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\02_ACT1_VALOROFACHILLES.DBR")]
	[GearType(GearType.AllWeapons)]
	VALOROFACHILLES_ACT1_02,

	/// <summary>
	/// Incarnation of the Valor of Achilles
	/// </summary>
	/// <remarks>
	/// Achilles was the greatest of all the 
	/// Greek warriors of the Trojan War. His 
	/// strength and speed in battle were 
	/// legendary. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\03_ACT1_VALOROFACHILLES.DBR")]
	[GearType(GearType.AllWeapons)]
	VALOROFACHILLES_ACT1_03,

	/// <summary>
	/// Venom Sac
	/// </summary>
	/// <remarks>
	/// Contains a deadly poison.  
	/// Can enhance all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\01_MULTACTS_VENOMSAC.DBR")]
	[GearType(GearType.AllWeapons)]
	VENOMSAC_MULTACTS_01,

	/// <summary>
	/// Epic Venom Sac
	/// </summary>
	/// <remarks>
	/// Contains a deadly poison.  
	/// Can enhance all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\02_MULTACTS_VENOMSAC.DBR")]
	[GearType(GearType.AllWeapons)]
	VENOMSAC_MULTACTS_02,

	/// <summary>
	/// Legendary Venom Sac
	/// </summary>
	/// <remarks>
	/// Contains a deadly poison.  
	/// Can enhance all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\03_MULTACTS_VENOMSAC.DBR")]
	[GearType(GearType.AllWeapons)]
	VENOMSAC_MULTACTS_03,

	/// <summary>
	/// Vile Ichor
	/// </summary>
	/// <remarks>
	/// A disgusting glob of pus, blood, and 
	/// bile. 
	/// Can enhance rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\01_ACT2_VILEICHOR.DBR")]
	[GearType(GearType.Jewellery)]
	VILEICHOR_ACT2_01,

	/// <summary>
	/// Epic Vile Ichor
	/// </summary>
	/// <remarks>
	/// A disgusting glob of pus, blood, and 
	/// bile. 
	/// Can enhance rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\02_ACT2_VILEICHOR.DBR")]
	[GearType(GearType.Jewellery)]
	VILEICHOR_ACT2_02,

	/// <summary>
	/// Legendary Vile Ichor
	/// </summary>
	/// <remarks>
	/// A disgusting glob of pus, blood, and 
	/// bile. 
	/// Can enhance rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\03_ACT2_VILEICHOR.DBR")]
	[GearType(GearType.Jewellery)]
	VILEICHOR_ACT2_03,

	/// <summary>
	/// Viny Growth
	/// </summary>
	/// <remarks>
	/// A lump of dense plant matter.  
	/// Can enhance all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\01_ACT3_VINYGROWTH.DBR")]
	[GearType(GearType.AllArmor)]
	VINYGROWTH_ACT3_01,

	/// <summary>
	/// Epic Viny Growth
	/// </summary>
	/// <remarks>
	/// A lump of dense plant matter.  
	/// Can enhance all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\02_ACT3_VINYGROWTH.DBR")]
	[GearType(GearType.AllArmor)]
	VINYGROWTH_ACT3_02,

	/// <summary>
	/// Legendary Viny Growth
	/// </summary>
	/// <remarks>
	/// A lump of dense plant matter.  
	/// Can enhance all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\03_ACT3_VINYGROWTH.DBR")]
	[GearType(GearType.AllArmor)]
	VINYGROWTH_ACT3_03,

	/// <summary>
	/// Essence of Wodan's Wisdom
	/// </summary>
	/// <remarks>
	/// No price, not even trading one of his 
	/// eyes, seems too great in the Allfather's 
	/// never-ending search for knowledge. 
	/// Can enchant armor and jewellery.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\RELICS\01_ACT5_WODANSWISDOM.DBR")]
	[GearType(GearType.AllArmor | GearType.Jewellery)]
	WODANSWISDOM_ACT5_01,

	/// <summary>
	/// Embodiment of Wodan's Wisdom
	/// </summary>
	/// <remarks>
	/// No price, not even trading one of his 
	/// eyes, seems too great in the Allfather's 
	/// never-ending search for knowledge. 
	/// Can enchant armor and jewellery.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\RELICS\02_ACT5_WODANSWISDOM.DBR")]
	[GearType(GearType.AllArmor | GearType.Jewellery)]
	WODANSWISDOM_ACT5_02,

	/// <summary>
	/// Incarnation of Wodan's Wisdom
	/// </summary>
	/// <remarks>
	/// No price, not even trading one of his 
	/// eyes, seems too great in the Allfather's 
	/// never-ending search for knowledge. 
	/// Can enchant armor and jewellery.
	/// </remarks>
	[Description(@"RECORDS\XPACK2\ITEM\RELICS\03_ACT5_WODANSWISDOM.DBR")]
	[GearType(GearType.AllArmor | GearType.Jewellery)]
	WODANSWISDOM_ACT5_03,

	/// <summary>
	/// Essence of Yen-Lo-Wang's Bloodletting
	/// </summary>
	/// <remarks>
	/// Yen-Lo-Wang is a god of Death and the 
	/// ruler of Feng-Du, the Chinese Hell. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\01_ACT3_YENLOWANGSBLOODLETTING.DBR")]
	[GearType(GearType.AllWeapons)]
	YENLOWANGSBLOODLETTING_ACT3_01,

	/// <summary>
	/// Embodiment of Yen-Lo-Wang's Bloodletting
	/// </summary>
	/// <remarks>
	/// Yen-Lo-Wang is a god of Death and the 
	/// ruler of Feng-Du, the Chinese Hell. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\02_ACT3_YENLOWANGSBLOODLETTING.DBR")]
	[GearType(GearType.AllWeapons)]
	YENLOWANGSBLOODLETTING_ACT3_02,

	/// <summary>
	/// Incarnation of Yen-Lo-Wang's Bloodletting
	/// </summary>
	/// <remarks>
	/// Yen-Lo-Wang is a god of Death and the 
	/// ruler of Feng-Du, the Chinese Hell. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\03_ACT3_YENLOWANGSBLOODLETTING.DBR")]
	[GearType(GearType.AllWeapons)]
	YENLOWANGSBLOODLETTING_ACT3_03,

	/// <summary>
	/// Yeti Fur
	/// </summary>
	/// <remarks>
	/// The thick, coarse pelt of the Yeti.  
	/// Can enhance all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\01_ACT3_YETIFUR.DBR")]
	[GearType(GearType.AllArmor)]
	YETIFUR_ACT3_01,

	/// <summary>
	/// Epic Yeti Fur
	/// </summary>
	/// <remarks>
	/// The thick, coarse pelt of the Yeti.  
	/// Can enhance all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\02_ACT3_YETIFUR.DBR")]
	[GearType(GearType.AllArmor)]
	YETIFUR_ACT3_02,

	/// <summary>
	/// Legendary Yeti Fur
	/// </summary>
	/// <remarks>
	/// The thick, coarse pelt of the Yeti.  
	/// Can enhance all armor.
	/// </remarks>
	[Description(@"RECORDS\ITEM\ANIMALRELICS\03_ACT3_YETIFUR.DBR")]
	[GearType(GearType.AllArmor)]
	YETIFUR_ACT3_03,

	/// <summary>
	/// Essence of Zeus' Thunderbolt
	/// </summary>
	/// <remarks>
	/// Zeus is the mightiest of the gods of 
	/// Olympus. The thunderbolt is his weapon 
	/// and symbol. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\01_ACT1_ZEUSTHUNDERBOLT.DBR")]
	[GearType(GearType.AllWeapons)]
	ZEUSTHUNDERBOLT_ACT1_01,

	/// <summary>
	/// Embodiment of Zeus' Thunderbolt
	/// </summary>
	/// <remarks>
	/// Zeus is the mightiest of the gods of 
	/// Olympus. The thunderbolt is his weapon 
	/// and symbol. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\02_ACT1_ZEUSTHUNDERBOLT.DBR")]
	[GearType(GearType.AllWeapons)]
	ZEUSTHUNDERBOLT_ACT1_02,

	/// <summary>
	/// Incarnation of Zeus' Thunderbolt
	/// </summary>
	/// <remarks>
	/// Zeus is the mightiest of the gods of 
	/// Olympus. The thunderbolt is his weapon 
	/// and symbol. 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\ITEM\RELICS\03_ACT1_ZEUSTHUNDERBOLT.DBR")]
	[GearType(GearType.AllWeapons)]
	ZEUSTHUNDERBOLT_ACT1_03,

	#region Eternal Embers

	/// <summary>
	/// Legendary Aspect of Chi
	/// </summary>
	/// <remarks>
	/// Fragmented essence of the five aspects 
	/// of Chi. 
	/// 
	/// Can enchant torso armor or shields.
	/// </remarks>
	[Description(@"RECORDS\XPACK4\ITEM\CHARMS\03_X4_ASPECTSOFCHI.DBR")]
	[GearType(GearType.Torso | GearType.Shield)]
	ASPECTSOFCHI_X4_03,

	/// <summary>
	/// Legendary Dragon Tendon
	/// </summary>
	/// <remarks>
	/// Tendons of the Dragonkin, spawn of Sihai 
	/// Longwang, ruler of the seas. 
	/// 
	/// Can enchant bows.
	/// </remarks>
	[Description(@"RECORDS\XPACK4\ITEM\CHARMS\03_X4_DRAGONTENDON.DBR")]
	[GearType(GearType.Bow)]
	DRAGONTENDON_X4_03,

	/// <summary>
	/// Essence of Chaos
	/// </summary>
	/// <remarks>
	/// A shard of a mystical artefact capable 
	/// of wielding the powers of Chaos itself. 
	/// Can enchant head armor only.
	/// </remarks>
	[Description(@"RECORDS\XPACK4\ITEM\RELICS\03_X4_ESSENCEOFCHAOS.DBR")]
	[GearType(GearType.Head)]
	ESSENCEOFCHAOS_X4_03,

	/// <summary>
	/// Incarnation of Embers
	/// </summary>
	/// <remarks>
	/// A shard of the Suns' celestial forms, 
	/// scattered throughout the land. 
	/// 
	/// Can enchant rings and amulets.
	/// </remarks>
	[Description(@"RECORDS\XPACK4\ITEM\RELICS\03_X4_INCARNATIONOFEMBER.DBR")]
	[GearType(GearType.Jewellery)]
	INCARNATIONOFEMBER_X4_03,

	/// <summary>
	/// Incarnation of Hou Yi's Determination
	/// </summary>
	/// <remarks>
	/// A fragment of the archery god's own bow. 
	/// 
	/// 
	/// Can enchant piercing weapons.
	/// </remarks>
	[Description(@"RECORDS\XPACK4\ITEM\RELICS\03_X4_INCARNATIONOFHOUYISDETERMINATION.DBR")]
	[GearType(GearType.Bow | GearType.Spear | GearType.Sword | GearType.Thrown)]
	INCARNATIONOFHOUYISDETERMINATION_X4_03,

	/// <summary>
	/// Incarnation of Serket's Touch
	/// </summary>
	/// <remarks>
	/// A shard of the original healing vial 
	/// used by Serket to treat venomous 
	/// injuries. 
	/// 
	/// Can enchant any ring.
	/// </remarks>
	[Description(@"RECORDS\XPACK4\ITEM\RELICS\03_X4_INCARNATIONOFSERKETSTOUCH.DBR")]
	[GearType(GearType.Ring)]
	INCARNATIONOFSERKETSTOUCH_X4_03,

	/// <summary>
	/// Incarnation of Sobek's Sanctuary
	/// </summary>
	/// <remarks>
	/// Sobek, the Egyptian god of military 
	/// prowess kneels for no man. 
	/// 
	/// Can enchant any armor.
	/// </remarks>
	[Description(@"RECORDS\XPACK4\ITEM\RELICS\03_X4_INCARNATIONOFSOBEKSANCTUARY.DBR")]
	[GearType(GearType.AllArmor)]
	INCARNATIONOFSOBEKSANCTUARY_X4_03,

	/// <summary>
	/// Incarnation of the Taotie's Gluttony
	/// </summary>
	/// <remarks>
	/// One of the Four Perils, the Taotie's 
	/// insatiable hunger is unmatched, 
	/// consuming everything in its path. 
	/// 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\XPACK4\ITEM\RELICS\03_X4_INCARNATIONOFTHETAOTIESGLUTTONY.DBR")]
	[GearType(GearType.AllWeapons)]
	INCARNATIONOFTHETAOTIESGLUTTONY_X4_03,

	/// <summary>
	/// Legendary Sunworshipper Ichor
	/// </summary>
	/// <remarks>
	/// Corrupted blood of the followers of 
	/// Akhenaten, the sun worshiping heretic. 
	/// 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\XPACK4\ITEM\CHARMS\03_X4_SUNWORSHIPPERICHOR.DBR")]
	[GearType(GearType.AllWeapons)]
	SUNWORSHIPPERICHOR_X4_03,

	/// <summary>
	/// Legendary Terracotta Plating
	/// </summary>
	/// <remarks>
	/// Chips of the the animated terracotta 
	/// soldiers' lamellar armor. 
	/// 
	/// Can enchant all armor.
	/// </remarks>
	[Description(@"RECORDS\XPACK4\ITEM\CHARMS\03_X4_TERRACOTTAPLATING.DBR")]
	[GearType(GearType.AllArmor)]
	TERRACOTTAPLATING_X4_03,

	/// <summary>
	/// Legendary Yaoguai Horn
	/// </summary>
	/// <remarks>
	/// Horns of adult Yaoguai, a soul devouring 
	/// demon, infused with the Chi of their 
	/// victims. 
	/// 
	/// Can enchant all weapons.
	/// </remarks>
	[Description(@"RECORDS\XPACK4\ITEM\CHARMS\03_X4_YAOGUAIHORN.DBR")]
	[GearType(GearType.AllWeapons)]
	YAOGUAIHORN_X4_03,

	#endregion
}

#region Related logic

public static class RelicAndCharmExtension
{
	internal record RelicAndCharmMapItem(RelicAndCharm Value, string Name, string RecordId, string FileName, GearType Types);

	internal static ReadOnlyCollection<RelicAndCharmMapItem> RelicAndCharmMap =
		EnumsNET.Enums.GetValues<RelicAndCharm>()
		.Select(v => EnumsNET.Enums.GetMember(v))
		.Select(m =>
			new RelicAndCharmMapItem(
				m.Value,
				m.Name,
				RecordId: m.AsString(EnumsNET.EnumFormat.Description),
				FileName: Path.GetFileName(m.AsString(EnumsNET.EnumFormat.Description)),
				Types: m.Attributes.Get<GearTypeAttribute>().Type
			)
		).ToList().AsReadOnly();

	/// <summary>
	/// Gets the <see cref="GearType"/> for a <see cref="RelicAndCharm"/>
	/// </summary>
	/// <param name="relic"></param>
	/// <returns></returns>
	public static GearType GetGearType(this RelicAndCharm relic)
		=> RelicAndCharmMap.First(m => m.Value == relic).Types;

	/// <summary>
	/// Gets the recordId for a <see cref="RelicAndCharm"/>
	/// </summary>
	/// <param name="relic"></param>
	/// <returns></returns>
	public static string GetRecordId(this RelicAndCharm relic)
		=> RelicAndCharmMap.First(m => m.Value == relic).RecordId;

}

#endregion
