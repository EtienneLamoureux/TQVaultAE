//-----------------------------------------------------------------------
// <copyright file="Item.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Entities
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;


	/// <summary>
	/// Class for holding item information
	/// </summary>
	public class Item
	{

		#region Item Fields

		/// <summary>
		/// Default value for empty var2.
		/// </summary>
		public const int var2Default = 2035248;

		/// <summary>
		/// Random number used as a seed for new items
		/// </summary>
		private static Random random = new Random();

		/// <summary>
		/// Binary marker for the beginning of a block
		/// </summary>
		public int beginBlockCrap1;

		/// <summary>
		/// Binary marker for the end of a block
		/// </summary>
		public int endBlockCrap1;

		/// <summary>
		/// A different binary marker for the beginning of a block
		/// </summary>
		public int beginBlockCrap2;

		/// <summary>
		/// A different binary marker for the end of a block
		/// </summary>
		public int endBlockCrap2;

		public bool atlantis = false;
		/// <summary>
		/// Prefix database record ID
		/// </summary>
		public string prefixID;

		/// <summary>
		/// Suffix database record ID
		/// </summary>
		public string suffixID;

		/// <summary>
		/// Relic database record ID
		/// </summary>
		public string relicID;

		/// <summary>
		/// Relic database record ID
		/// </summary>
		public string relic2ID;

		/// <summary>
		/// Info structure for the base item
		/// </summary>
		public Info baseItemInfo;

		/// <summary>
		/// Info structure for the item's prefix
		/// </summary>
		public Info prefixInfo;

		/// <summary>
		/// Info structure for the item's suffix
		/// </summary>
		public Info suffixInfo;

		/// <summary>
		/// String containing all of the item's attributes
		/// </summary>
		public string[] attributesStringArray = new string[] { };

		/// <summary>
		/// String containing all of the item's requirements
		/// </summary>
		public string[] requirementsStringArray = new string[] { };

		/// <summary>
		/// String containing other items in a set
		/// if this is a set item.
		/// </summary>
		public string[] setItemsStringArray = new string[] { };

		/// <summary>
		/// Used for level calculation
		/// </summary>
		public int attributeCount;

		/// <summary>
		/// Used so that attributes are not counted multiple times
		/// </summary>
		public bool isAttributeCounted;

		/// <summary>
		/// Used for properties display
		/// </summary>
		public string[] bareAttributes;

		/// <summary>
		/// Used for itemScalePercent calculation
		/// </summary>
		public float itemScalePercent;

		#endregion Item Fields

		/// <summary>
		/// Initializes a new instance of the Item class.
		/// </summary>
		public Item()
		{
			// Added by VillageIdiot
			// Used for bare item attributes in properties display in this order:
			// baseinfo, artifactCompletionBonus, prefixinfo, suffixinfo, relicinfo, relicCompletionBonus
			this.bareAttributes = new string[6];  // Hard coded to 6
			this.itemScalePercent = 1.00F;
			this.StackSize = 1;

			// Make sure the translatable strings have a value.
			SetDefaultStrings();
		}


		public string GameExtensionSuffix
		{
			get
			{
				string ext = ItemStyle.Quest.TQColor().ColorTag();
				if (this.IsImmortalThrone) ext += "(IT)";
				else if (this.IsRagnarok) ext += "(RAG)";
				else if (this.IsAtlantis) ext += "(ATL)";
				return ext;
			}
		}


		/// <summary>
		/// Sets the default string values for the translatable strings.
		/// These strings are specific to TQVault and are not in the TQ Text database.
		/// </summary>
		private static void SetDefaultStrings()
		{
			if (string.IsNullOrEmpty(Item.ItemIT))
			{
				Item.ItemIT = "Immortal Throne Item";
			}

			if (string.IsNullOrEmpty(Item.ItemRagnarok))
			{
				Item.ItemRagnarok = "Ragnarok Item";
			}

			if (string.IsNullOrEmpty(Item.ItemAtlantis))
			{
				Item.ItemAtlantis = "Atlantis Item";
			}

			if (string.IsNullOrEmpty(Item.ItemWith))
			{
				Item.ItemWith = "with";
			}

			if (string.IsNullOrEmpty(Item.ItemRelicBonus))
			{
				Item.ItemRelicBonus = "(Completion Bonus: {0})";
			}

			if (string.IsNullOrEmpty(Item.ItemRelicCompleted))
			{
				Item.ItemRelicCompleted = "(Completed)";
			}

			if (string.IsNullOrEmpty(Item.ItemQuest))
			{
				Item.ItemQuest = "(Quest Item)";
			}

			if (string.IsNullOrEmpty(Item.ItemSeed))
			{
				Item.ItemSeed = "itemSeed: {0} (0x{0:X8}) ({1:p3})";
			}
		}

		#region Item Properties

		/// <summary>
		/// Gets the weapon slot indicator value.
		/// This is a special value in the coordinates that signals an item is in a weapon slot.
		/// </summary>
		public const int WeaponSlotIndicator = -3;

		/// <summary>
		/// Gets or sets the string used for 'with'
		/// </summary>
		public static string ItemWith { get; set; }

		/// <summary>
		/// Gets or sets the relic completion bonus string.
		/// </summary>
		public static string ItemRelicBonus { get; set; }

		/// <summary>
		/// Gets or sets the relic completed string.
		/// </summary>
		public static string ItemRelicCompleted { get; set; }

		/// <summary>
		/// Gets or sets the quest item indicator string.
		/// </summary>
		public static string ItemQuest { get; set; }

		/// <summary>
		/// Gets or sets the item seed format string.
		/// </summary>
		public static string ItemSeed { get; set; }

		/// <summary>
		/// Gets or sets the string which indicates an Immortal Throne item.
		/// </summary>
		public static string ItemIT { get; set; }

		/// <summary>
		/// Gets or sets the string which indicates an Immortal Throne item.
		/// </summary>
		public static string ItemRagnarok { get; set; }

		/// <summary>
		/// Gets or sets the string which indicates an Atlantis item.
		/// </summary>
		public static string ItemAtlantis { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the skill level is shown on granted skills.
		/// </summary>
		public static bool ShowSkillLevel { get; set; }

		/// <summary>
		/// Gets the base item id
		/// </summary>
		public string BaseItemId { get; set; }

		/// <summary>
		/// Gets or sets the relic bonus id
		/// </summary>
		public string RelicBonusId { get; set; }

		/// <summary>
		/// Gets or sets the relic bonus2 id
		/// </summary>
		public string RelicBonus2Id { get; set; }

		/// <summary>
		/// Gets or sets the item seed
		/// </summary>
		public int Seed { get; set; }

		/// <summary>
		/// Gets the number of relics
		/// </summary>
		private int var1;
		public int Var1
		{
			get
			{
				if (IsRelic && var1 == 0)
				{
					// The "Power of Nerthus" relic is a special quest-reward relic with only 1 shard (it is always complete). 
					// Since its database var1 value is 0, we hard-set it to 1.
					return 1;
				}

				return var1;
			}

			set
			{
				var1 = value;
			}
		}

		/// <summary>
		/// Gets the number of relics for the second relic slot
		/// </summary>
		public int Var2 { get; set; }

		/// <summary>
		/// Gets or sets the stack size.
		/// Used for stackable items like potions.
		/// </summary>
		public int StackSize { get; set; }

		/// <summary>
		/// Gets or sets the X cell position of the item.
		/// </summary>
		public int PositionX { get; set; }

		/// <summary>
		/// Gets or sets the Y cell position of the item.
		/// </summary>
		public int PositionY { get; set; }

		/// <summary>
		/// Gets or sets the item's upper left corner in cell coordinates.
		/// </summary>
		public Point Location
		{
			get
			{
				return new Point(this.PositionX, this.PositionY);
			}

			set
			{
				this.PositionX = value.X;
				this.PositionY = value.Y;
			}
		}

		/// <summary>
		/// Gets the relic info
		/// </summary>
		public Info RelicInfo { get; set; }

		/// <summary>
		/// Gets or sets the relic bonus info
		/// </summary>
		public Info RelicBonusInfo { get; set; }

		/// <summary>
		/// Gets the second relic info
		/// </summary>
		public Info Relic2Info { get; set; }

		/// <summary>
		/// Gets or sets the second relic bonus info
		/// </summary>
		public Info RelicBonus2Info { get; set; }

		/// <summary>
		/// Raw image data from game resource file
		/// </summary>
		public byte[] TexImage { get; set; }

		/// <summary>
		/// ResourceId related to <see cref="TexImage"/>
		/// </summary>
		public string TexImageResourceId { get; set; }

		/// <summary>
		/// Gets the item's width in cells
		/// </summary>
		public int Width { get; set; }

		/// <summary>
		/// Gets the item's height in cells.
		/// </summary>
		public int Height { get; set; }

		/// <summary>
		/// Gets the item's size in cells.
		/// </summary>
		public Size Size
		{
			get
			{
				return new Size(this.Width, this.Height);
			}
		}

		/// <summary>
		/// Gets the item's right cell location
		/// </summary>
		public int Right
		{
			get
			{
				return this.Location.X + this.Width;
			}
		}

		/// <summary>
		/// Gets the item's bottom cell location.
		/// </summary>
		public int Bottom
		{
			get
			{
				return this.Location.Y + this.Height;
			}
		}


		/// <summary>
		/// Gets or sets the item container type
		/// </summary>
		public SackType ContainerType { get; set; }

		/// <summary>
		/// Gets a value indicating whether the item is in an equipment weapon slot.
		/// </summary>
		public bool IsInWeaponSlot
		{
			get
			{
				return this.PositionX == WeaponSlotIndicator;
			}
		}

		/// <summary>
		/// Gets a value indicating whether or not the item comes from Immortal Throne expansion pack.
		/// </summary>
		public bool IsImmortalThrone
		{
			get
			{
				if (this.BaseItemId.ToUpperInvariant().IndexOf("XPACK\\", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return true;
				}

				if (this.prefixID != null && this.prefixID.ToUpperInvariant().IndexOf("XPACK\\", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return true;
				}

				if (this.suffixID != null && this.suffixID.ToUpperInvariant().IndexOf("XPACK\\", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether or not the item comes from Ragnarok DLC.
		/// </summary>
		public bool IsRagnarok
		{
			get
			{
				if (this.BaseItemId.ToUpperInvariant().IndexOf("XPACK2\\", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return true;
				}

				if (this.prefixID != null && this.prefixID.ToUpperInvariant().IndexOf("XPACK2\\", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return true;
				}

				if (this.suffixID != null && this.suffixID.ToUpperInvariant().IndexOf("XPACK2\\", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether or not the item comes from Atlantis DLC.
		/// </summary>
		public bool IsAtlantis
		{
			get
			{
				if (this.BaseItemId.ToUpperInvariant().IndexOf("XPACK3\\", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return true;
				}

				if (this.prefixID != null && this.prefixID.ToUpperInvariant().IndexOf("XPACK3\\", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return true;
				}

				if (this.suffixID != null && this.suffixID.ToUpperInvariant().IndexOf("XPACK3\\", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item is a scroll.
		/// </summary>
		public bool IsScroll
		{
			get
			{
				if (this.baseItemInfo != null)
				{
					return this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ONESHOT_SCROLL");
				}
				else if (this.IsImmortalThrone || this.IsRagnarok || this.IsAtlantis)
				{
					if (this.BaseItemId.ToUpperInvariant().IndexOf("\\SCROLLS\\", StringComparison.OrdinalIgnoreCase) >= 0)
					{
						return true;
					}
				}

				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item is a parchment.
		/// </summary>
		public bool IsParchment
		{
			get
			{
				if (this.IsImmortalThrone || this.IsRagnarok || this.IsAtlantis)
				{
					if (this.BaseItemId.ToUpperInvariant().IndexOf("PARCHMENT", StringComparison.OrdinalIgnoreCase) >= 0)
						return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item is a formulae.
		/// </summary>
		public bool IsFormulae
		{
			get
			{
				if (this.baseItemInfo != null)
				{
					return this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ITEMARTIFACTFORMULA");
				}
				else if (this.IsImmortalThrone || this.IsRagnarok || this.IsAtlantis)
				{
					if (this.BaseItemId.ToUpperInvariant().IndexOf("\\ARCANEFORMULAE\\", StringComparison.OrdinalIgnoreCase) >= 0)
					{
						return true;
					}
				}

				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item is an artifact.
		/// </summary>
		public bool IsArtifact
		{
			get
			{
				if (this.baseItemInfo != null)
				{
					return this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ITEMARTIFACT");
				}
				else if (this.IsImmortalThrone || this.IsRagnarok || this.IsAtlantis)
				{
					if (!this.IsFormulae && this.BaseItemId.ToUpperInvariant().IndexOf("\\ARTIFACTS\\", StringComparison.OrdinalIgnoreCase) >= 0)
					{
						return true;
					}
				}

				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item is a shield.
		/// </summary>
		public bool IsShield
		{
			get
			{
				if (this.baseItemInfo != null)
				{
					return this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("WEAPONARMOR_SHIELD");
				}

				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item is armor.
		/// </summary>
		public bool IsArmor
		{
			get
			{
				if (this.baseItemInfo != null)
				{
					return this.baseItemInfo.ItemClass.ToUpperInvariant().StartsWith("ARMORPROTECTIVE", StringComparison.OrdinalIgnoreCase);
				}

				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item is a helm.
		/// </summary>
		public bool IsHelm
		{
			get
			{
				if (this.baseItemInfo != null)
				{
					return this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ARMORPROTECTIVE_HEAD");
				}

				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item is a bracer.
		/// </summary>
		public bool IsBracer
		{
			get
			{
				if (this.baseItemInfo != null)
				{
					return this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ARMORPROTECTIVE_FOREARM");
				}

				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item is torso armor.
		/// </summary>
		public bool IsTorsoArmor
		{
			get
			{
				if (this.baseItemInfo != null)
				{
					return this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ARMORPROTECTIVE_UPPERBODY");
				}

				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item is a greave.
		/// </summary>
		public bool IsGreave
		{
			get
			{
				if (this.baseItemInfo != null)
				{
					return this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ARMORPROTECTIVE_LOWERBODY");
				}

				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item is a ring.
		/// </summary>
		public bool IsRing
		{
			get
			{
				if (this.baseItemInfo != null)
				{
					return this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ARMORJEWELRY_RING");
				}

				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item is an amulet.
		/// </summary>
		public bool IsAmulet
		{
			get
			{
				if (this.baseItemInfo != null)
				{
					return this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ARMORJEWELRY_AMULET");
				}

				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item is a weapon.
		/// </summary>
		public bool IsWeapon
		{
			get
			{
				if (this.baseItemInfo != null)
				{
					return !this.IsShield && this.baseItemInfo.ItemClass.ToUpperInvariant().StartsWith("WEAPON", StringComparison.OrdinalIgnoreCase);
				}

				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item is a two handed weapon.
		/// </summary>
		public bool Is2HWeapon
		{
			get
			{
				if (this.baseItemInfo != null)
				{
					if (this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("WEAPONHUNTING_BOW") || this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("WEAPONMAGICAL_STAFF"))
					{
						return true;
					}
					else
					{
						return false;
					}
				}

				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item is a weapon or shield.
		/// </summary>
		public bool IsWeaponShield
		{
			get
			{
				if (this.baseItemInfo != null)
				{
					return this.baseItemInfo.ItemClass.ToUpperInvariant().StartsWith("WEAPON", StringComparison.OrdinalIgnoreCase);
				}

				return false;
			}
		}

		const string QUEST = "QUEST";
		const string QUESTS = "QUESTS";
		const string QUESTITEM = "QUESTITEM";
		/// <summary>
		/// Gets a value indicating whether the item is a quest item.
		/// </summary>
		public bool IsQuestItem
		{
			get
			{
				if (this.baseItemInfo != null)
				{
					if (this.baseItemInfo.ItemClassification.ToUpperInvariant().Equals(QUEST)
						|| this.baseItemInfo.ItemClass.ToUpperInvariant().Equals(QUESTITEM))
						return true;
				}
				else if (!this.IsImmortalThrone && !this.IsRagnarok && !this.IsAtlantis)
				{
					if (this.BaseItemId.ToUpperInvariant().IndexOf(QUEST, StringComparison.OrdinalIgnoreCase) >= 0)
						return true;
				}
				else if (this.BaseItemId.ToUpperInvariant().IndexOf(QUESTS, StringComparison.OrdinalIgnoreCase) >= 0)
					return true;

				return false;
			}
		}

		/// <summary>
		/// Gets the item style enumeration
		/// </summary>
		public ItemStyle ItemStyle
		{
			get
			{
				if (this.prefixInfo != null && this.prefixInfo.ItemClassification.ToUpperInvariant().Equals("BROKEN"))
				{
					return ItemStyle.Broken;
				}

				if (this.IsArtifact)
				{
					return ItemStyle.Artifact;
				}

				if (this.IsFormulae)
				{
					return ItemStyle.Formulae;
				}

				if (this.IsScroll)
				{
					return ItemStyle.Scroll;
				}

				if (this.IsParchment)
				{
					return ItemStyle.Parchment;
				}

				if (this.IsRelic)
				{
					return ItemStyle.Relic;
				}

				if (this.IsPotion)
				{
					return ItemStyle.Potion;
				}

				if (this.IsQuestItem)
				{
					return ItemStyle.Quest;
				}

				if (this.baseItemInfo != null)
				{
					if (this.baseItemInfo.ItemClassification.ToUpperInvariant().Equals("EPIC"))
					{
						return ItemStyle.Epic;
					}

					if (this.baseItemInfo.ItemClassification.ToUpperInvariant().Equals("LEGENDARY"))
					{
						return ItemStyle.Legendary;
					}

					if (this.baseItemInfo.ItemClassification.ToUpperInvariant().Equals("RARE"))
					{
						return ItemStyle.Rare;
					}
				}

				// At this point baseItem indicates Common.  Let's check affixes
				if (this.suffixInfo != null && this.suffixInfo.ItemClassification.ToUpperInvariant().Equals("RARE"))
				{
					return ItemStyle.Rare;
				}

				if (this.prefixInfo != null && this.prefixInfo.ItemClassification.ToUpperInvariant().Equals("RARE"))
				{
					return ItemStyle.Rare;
				}

				// Not rare.  If we have a suffix or prefix, then call it common, else mundane
				if (this.suffixInfo != null || this.prefixInfo != null)
				{
					return ItemStyle.Common;
				}

				return ItemStyle.Mundane;
			}
		}

		/// <summary>
		/// Gets the item class
		/// </summary>
		public string ItemClass
		{
			get
			{
				if (this.baseItemInfo == null)
				{
					return string.Empty;
				}
				else
				{
					return this.baseItemInfo.ItemClass;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether or not the item will stack.
		/// </summary>
		public bool DoesStack
		{
			get
			{
				return this.IsPotion || this.IsScroll;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item has a number attached.
		/// </summary>
		public bool HasNumber
		{
			get
			{
				return this.DoesStack || (this.IsRelic && !this.IsRelicComplete);
			}
		}

		/// <summary>
		/// Gets or sets the number attached to the item.
		/// </summary>
		public int Number
		{
			get
			{
				if (this.DoesStack)
				{
					return this.StackSize;
				}

				if (this.IsRelic)
				{
					return Math.Max(this.Var1, 1);
				}

				return 0;
			}

			set
			{
				// Added by VillageIdiot
				if (this.DoesStack)
				{
					this.StackSize = value;
				}
				else if (this.IsRelic)
				{
					// Limit value to complete Relic level
					if (value >= this.baseItemInfo.CompletedRelicLevel)
					{
						this.Var1 = this.baseItemInfo.CompletedRelicLevel;
					}
					else
					{
						this.Var1 = value;
					}
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item has an embedded relic.
		/// </summary>
		public bool HasRelicSlot1
		{
			get
			{
				return !this.IsRelic && this.relicID.Length > 0;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item has a second embedded relic.
		/// </summary>
		public bool HasRelicSlot2
		{
			get
			{
				return !this.IsRelic && this.relic2ID.Length > 0;
			}
		}

		/// <summary>
		/// Indicate that the item has an embedded relic.
		/// </summary>
		public bool HasRelic => HasRelicSlot1 | HasRelicSlot2;

		/// <summary>
		/// Gets a value indicating whether the item is a potion.
		/// </summary>
		public bool IsPotion
		{
			get
			{
				if (this.baseItemInfo != null)
				{
					return this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ONESHOT_POTIONHEALTH")
						|| this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ONESHOT_POTIONMANA");
				}
				else
				{
					return this.BaseItemId.ToUpperInvariant().IndexOf("ONESHOT\\POTION", StringComparison.OrdinalIgnoreCase) != -1;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item is a charm and only a charm.
		/// </summary>
		public bool IsCharm
		{
			get
			{
				if (this.baseItemInfo != null)
				{
					return this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ITEMCHARM");
				}
				else if (!this.IsImmortalThrone && !this.IsRagnarok && !this.IsAtlantis)
				{
					return this.BaseItemId.ToUpperInvariant().IndexOf("ANIMALRELICS", StringComparison.OrdinalIgnoreCase) != -1;
				}
				else
				{
					if (this.BaseItemId.ToUpperInvariant().IndexOf("\\CHARMS\\", StringComparison.OrdinalIgnoreCase) != -1)
					{
						return true;
					}

					return false;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item first relic is a charm and only a charm.
		/// </summary>
		public bool IsRelic1Charm
		{
			get
			{
				if (this.RelicInfo != null)
					return this.RelicInfo.ItemClass.ToUpperInvariant().Equals("ITEMCHARM");
				else if (!this.IsImmortalThrone && !this.IsRagnarok && !this.IsAtlantis)
					return (this.RelicInfo?.ItemId.ToUpperInvariant().IndexOf("ANIMALRELICS", StringComparison.OrdinalIgnoreCase) ?? -1) != -1;
				else
					return (this.RelicInfo?.ItemId.ToUpperInvariant().IndexOf("\\CHARMS\\", StringComparison.OrdinalIgnoreCase) ?? -1) != -1;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item second relic is a charm and only a charm.
		/// </summary>
		public bool IsRelic2Charm
		{
			get
			{
				if (this.Relic2Info != null)
					return this.Relic2Info.ItemClass.ToUpperInvariant().Equals("ITEMCHARM");
				else if (!this.IsImmortalThrone && !this.IsRagnarok && !this.IsAtlantis)
					return (this.Relic2Info?.ItemId.ToUpperInvariant().IndexOf("ANIMALRELICS", StringComparison.OrdinalIgnoreCase) ?? -1) != -1;
				else
					return (this.Relic2Info?.ItemId.ToUpperInvariant().IndexOf("\\CHARMS\\", StringComparison.OrdinalIgnoreCase) ?? -1) != -1;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item is a relic or a charm.
		/// </summary>
		public bool IsRelic
		{
			get
			{
				if (this.baseItemInfo != null)
				{
					return this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ITEMRELIC")
						|| this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ITEMCHARM");
				}
				else if (!this.IsImmortalThrone && !this.IsRagnarok && !this.IsAtlantis)
				{
					return this.BaseItemId.ToUpperInvariant().IndexOf("RELICS", StringComparison.OrdinalIgnoreCase) != -1;
				}
				else
				{
					if (this.BaseItemId.ToUpperInvariant().IndexOf("\\RELICS\\", StringComparison.OrdinalIgnoreCase) != -1)
					{
						return true;
					}

					if (this.BaseItemId.ToUpperInvariant().IndexOf("\\CHARMS\\", StringComparison.OrdinalIgnoreCase) != -1)
					{
						return true;
					}

					return false;
				}
			}
		}

		/// <summary>
		/// Indicate if this item is a completed relic.
		/// </summary>
		public bool IsRelicComplete
		{
			get
			{
				if (this.baseItemInfo != null)
					return this.Var1 >= this.baseItemInfo.CompletedRelicLevel;

				return false;
			}
		}

		/// <summary>
		/// Indicate if the first relic completion bonus apply.
		/// </summary>
		public bool IsRelicBonus1Complete
		{
			get
			{

				if (this.RelicBonusInfo != null)
					return this.Var1 >= this.RelicBonusInfo.CompletedRelicLevel;

				return false;
			}
		}

		/// <summary>
		/// Indicate if the second relic completion bonus apply.
		/// </summary>
		public bool IsRelicBonus2Complete
		{
			get
			{
				if (this.RelicBonus2Info != null)
					return this.Var2 >= this.RelicBonus2Info.CompletedRelicLevel;

				return false;
			}
		}



		/// <summary>
		/// Gets the item group.
		/// Used for grouping during autosort.
		/// </summary>
		public int ItemGroup
		{
			get
			{
				if (this.baseItemInfo == null)
				{
					return 0;
				}

				int group = 0;
				if (this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ONESHOT_POTIONHEALTH"))
				{
					group = 0;
				}
				else if (this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ONESHOT_POTIONMANA"))
				{
					group = 1;
				}
				else if (this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ARMORJEWELRY_AMULET"))
				{
					group = 2;
				}
				else if (this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ARMORJEWELRY_RING"))
				{
					group = 3;
				}
				else if (this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ITEMCHARM"))
				{
					group = 4;
				}
				else if (this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ITEMRELIC"))
				{
					group = 5;
				}
				else if (this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ITEMARTIFACTFORMULA"))
				{
					group = 6;
				}
				else if (this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ITEMARTIFACT"))
				{
					group = 7;
				}
				else if (this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ONESHOT_SCROLL"))
				{
					group = 8;
				}
				else if (this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ARMORPROTECTIVE_LOWERBODY"))
				{
					group = 9;
				}
				else if (this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ARMORPROTECTIVE_FOREARM"))
				{
					group = 10;
				}
				else if (this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ARMORPROTECTIVE_HEAD"))
				{
					group = 11;
				}
				else if (this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("ARMORPROTECTIVE_UPPERBODY"))
				{
					group = 12;
				}
				else if (this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("WEAPONARMOR_SHIELD"))
				{
					group = 13;
				}
				else if (this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("WEAPONMELEE_AXE"))
				{
					group = 14;
				}
				else if (this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("WEAPONMELEE_MACE"))
				{
					group = 15;
				}
				else if (this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("WEAPONMELEE_SWORD"))
				{
					group = 16;
				}
				else if (this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("WEAPONHUNTING_BOW"))
				{
					group = 17;
				}
				else if (this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("WEAPONMAGICAL_STAFF"))
				{
					group = 18;
				}
				else if (this.baseItemInfo.ItemClass.ToUpperInvariant().Equals("WEAPONHUNTING_SPEAR"))
				{
					group = 19;
				}
				else
				{
					group = 0;
				}

				return group;
			}
		}

		#endregion Item Properties

		#region Item Public Static Methods


		/// <summary>
		/// Generates a new seed that can be used on a new item
		/// </summary>
		/// <returns>New seed from 0 to 0x7FFF</returns>
		public static int GenerateSeed()
		{
			// The seed values in the player files seem to be limitted to 0x00007fff or less.
			return random.Next(0x00007fff);
		}

		#endregion Item Public Static Methods

		/// <summary>
		/// Creates an empty item
		/// </summary>
		/// <returns>Empty Item structure</returns>
		public Item MakeEmptyItem()
		{
			Item newItem = new Item();
			newItem.beginBlockCrap1 = this.beginBlockCrap1;
			newItem.endBlockCrap1 = this.endBlockCrap1;
			newItem.beginBlockCrap2 = this.beginBlockCrap2;
			newItem.endBlockCrap2 = this.endBlockCrap2;
			newItem.BaseItemId = string.Empty;
			newItem.prefixID = string.Empty;
			newItem.suffixID = string.Empty;
			newItem.relicID = string.Empty;
			newItem.relic2ID = string.Empty;
			newItem.RelicBonusId = string.Empty;
			newItem.RelicBonus2Id = string.Empty;
			newItem.Seed = GenerateSeed();
			newItem.Var1 = this.var1;
			newItem.Var2 = var2Default;
			newItem.PositionX = -1;
			newItem.PositionY = -1;

			return newItem;
		}

		/// <summary>
		/// Makes a new item based on the passed item id string
		/// </summary>
		/// <param name="baseItemId">base item id of the new item</param>
		/// <returns>Empty Item structure based on the passed item string</returns>
		public Item MakeEmptyCopy(string baseItemId)
		{
			if (string.IsNullOrEmpty(baseItemId))
			{
				throw new ArgumentNullException(baseItemId, "The base item ID cannot be NULL or Empty.");
			}

			Item newItem = MakeEmptyItem();
			newItem.BaseItemId = baseItemId;

			return newItem;
		}


		/// <summary>
		/// Marks the item as modified
		/// </summary>
		public void MarkModified()
		{
			this.attributesStringArray = new string[] { };
			this.requirementsStringArray = new string[] { };
			this.setItemsStringArray = new string[] { };
		}

		/// <summary>
		/// Makes a duplicate of the item
		/// VillageIdiot - Added option to keep item seed.
		/// </summary>
		/// <param name="keepItemSeed">flag on whether we are keeping our item seed or creating a new one.</param>
		/// <returns>New Duplicated item</returns>
		public Item Duplicate(bool keepItemSeed)
		{
			Item newItem = (Item)this.MemberwiseClone();
			if (!keepItemSeed)
			{
				newItem.Seed = GenerateSeed();
			}

			newItem.PositionX = -1;
			newItem.PositionY = -1;
			newItem.MarkModified();

			return newItem;
		}

		/// <summary>
		/// Clones the item
		/// </summary>
		/// <returns>A new clone of the item</returns>
		public Item Clone()
		{
			Item newItem = (Item)this.MemberwiseClone();
			newItem.MarkModified();
			return newItem;
		}



		/// <summary>
		/// Pops all but one item from the stack.
		/// </summary>
		/// <returns>Returns popped items as a new Item stack</returns>
		public Item PopAllButOneItem()
		{
			if (!this.DoesStack || this.StackSize < 2)
			{
				return null;
			}

			// make a complete copy then change a few things
			Item newItem = (Item)this.MemberwiseClone();

			newItem.StackSize = this.StackSize - 1;
			newItem.Var1 = 0;
			newItem.Seed = GenerateSeed();
			newItem.PositionX = -1;
			newItem.PositionY = -1;
			newItem.MarkModified();

			this.MarkModified();

			this.StackSize = 1;

			return newItem;
		}

		public SortedList<string, Variable> requirementsList;
	}
}
