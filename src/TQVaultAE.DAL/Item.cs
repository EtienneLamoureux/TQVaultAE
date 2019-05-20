//-----------------------------------------------------------------------
// <copyright file="Item.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultData
{
	using ExpressionEvaluator;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Drawing;
	using System.Globalization;
	using System.IO;

	/// <summary>
	/// Item Style types
	/// </summary>
	public enum ItemStyle
	{
		/// <summary>
		/// Broken Item Enumeration
		/// </summary>
		Broken,

		/// <summary>
		/// Mundane Item Enumeration
		/// </summary>
		Mundane,

		/// <summary>
		/// Common Item Enumeration
		/// </summary>
		Common,

		/// <summary>
		/// Rare Item Enumeration
		/// </summary>
		Rare,

		/// <summary>
		/// Epic Item Enumeration
		/// </summary>
		Epic,

		/// <summary>
		/// Legendary Item Enumeration
		/// </summary>
		Legendary,

		/// <summary>
		/// Quest Item Enumeration
		/// </summary>
		Quest,

		/// <summary>
		/// Relic Enumeration
		/// </summary>
		Relic,

		/// <summary>
		/// Potion Enumeration
		/// </summary>
		Potion,

		/// <summary>
		/// Scroll Enumeration
		/// </summary>
		Scroll,

		/// <summary>
		/// Parchment Enumeration
		/// </summary>
		Parchment,

		/// <summary>
		/// Artifact Formulae Enumeration
		/// </summary>
		Formulae,

		/// <summary>
		/// Artifact Enumeration
		/// </summary>
		Artifact
	}

	/// <summary>
	/// Titan Quest pre defined colors
	/// </summary>
	public enum TQColor
	{
		/// <summary>
		/// Titan Quest Aqua color
		/// </summary>
		Aqua,

		/// <summary>
		/// Titan Quest Blue color
		/// </summary>
		Blue,

		/// <summary>
		/// Titan Quest Light Cyan color
		/// </summary>
		LightCyan,

		/// <summary>
		/// Titan Quest Dark Gray color
		/// </summary>
		DarkGray,

		/// <summary>
		/// Titan Quest Fuschia color
		/// </summary>
		Fuschia,

		/// <summary>
		/// Titan Quest Green color
		/// </summary>
		Green,

		/// <summary>
		/// Titan Quest Indigo color
		/// </summary>
		Indigo,

		/// <summary>
		/// Titan Quest Khaki color
		/// </summary>
		Khaki,

		/// <summary>
		/// Titan Quest Yellow Green color
		/// </summary>
		YellowGreen,

		/// <summary>
		/// Titan Quest Maroon color
		/// </summary>
		Maroon,

		/// <summary>
		/// Titan Quest Orange color
		/// </summary>
		Orange,

		/// <summary>
		/// Titan Quest Purple color
		/// </summary>
		Purple,

		/// <summary>
		/// Titan Quest Red color
		/// </summary>
		Red,

		/// <summary>
		/// Titan Quest Silver color
		/// </summary>
		Silver,

		/// <summary>
		/// Titan Quest Turquoise color
		/// </summary>
		Turquoise,

		/// <summary>
		/// Titan Quest White color
		/// </summary>
		White,

		/// <summary>
		/// Titan Quest Yellow color
		/// </summary>
		Yellow
	}

	/// <summary>
	/// Class for holding item information
	/// </summary>
	public class Item
	{
		#region Item Fields

		/// <summary>
		/// Default value for empty var2.
		/// </summary>
		private const int var2Default = 2035248;

		/// <summary>
		/// Random number used as a seed for new items
		/// </summary>
		private static Random random = InitializeRandom();

		/// <summary>
		/// Binary marker for the beginning of a block
		/// </summary>
		private int beginBlockCrap1;

		/// <summary>
		/// Binary marker for the end of a block
		/// </summary>
		private int endBlockCrap1;

		/// <summary>
		/// A different binary marker for the beginning of a block
		/// </summary>
		private int beginBlockCrap2;

		/// <summary>
		/// A different binary marker for the end of a block
		/// </summary>
		private int endBlockCrap2;

		private bool atlantis = false;
		/// <summary>
		/// Prefix database record ID
		/// </summary>
		private string prefixID;

		/// <summary>
		/// Suffix database record ID
		/// </summary>
		private string suffixID;

		/// <summary>
		/// Relic database record ID
		/// </summary>
		private string relicID;

		/// <summary>
		/// Relic database record ID
		/// </summary>
		private string relic2ID;

		/// <summary>
		/// Info structure for the base item
		/// </summary>
		private Info baseItemInfo;

		/// <summary>
		/// Info structure for the item's prefix
		/// </summary>
		private Info prefixInfo;

		/// <summary>
		/// Info structure for the item's suffix
		/// </summary>
		private Info suffixInfo;

		/// <summary>
		/// String containing all of the item's attributes
		/// </summary>
		private string attributesString;

		/// <summary>
		/// String containing all of the item's requirements
		/// </summary>
		private string requirementsString;

		/// <summary>
		/// String containing other items in a set
		/// if this is a set item.
		/// </summary>
		private string setItemsString;

		/// <summary>
		/// Used for level calculation
		/// </summary>
		private int attributeCount;

		/// <summary>
		/// Used so that attributes are not counted multiple times
		/// </summary>
		private bool isAttributeCounted;

		/// <summary>
		/// Used for properties display
		/// </summary>
		private string[] bareAttributes;

		/// <summary>
		/// Used for itemScalePercent calculation
		/// </summary>
		private float itemScalePercent;

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

		#region Item Properties

		/// <summary>
		/// Gets the weapon slot indicator value.
		/// This is a special value in the coordinates that signals an item is in a weapon slot.
		/// </summary>
		public static int WeaponSlotIndicator
		{
			get
			{
				return -3;
			}
		}

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
		public string BaseItemId { get; private set; }

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

			private set
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
		public Info RelicInfo { get; private set; }

		/// <summary>
		/// Gets or sets the relic bonus info
		/// </summary>
		public Info RelicBonusInfo { get; set; }

		/// <summary>
		/// Gets the second relic info
		/// </summary>
		public Info Relic2Info { get; private set; }

		/// <summary>
		/// Gets or sets the second relic bonus info
		/// </summary>
		public Info RelicBonus2Info { get; set; }

		/// <summary>
		/// Gets the item's bitmap
		/// </summary>
		public Bitmap ItemBitmap { get; private set; }

		/// <summary>
		/// Gets the item's width in cells
		/// </summary>
		public int Width { get; private set; }

		/// <summary>
		/// Gets the item's height in cells.
		/// </summary>
		public int Height { get; private set; }

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
					if (this.BaseItemId.ToUpperInvariant().IndexOf("\\PARCHMENTS\\", StringComparison.OrdinalIgnoreCase) >= 0)
					{
						return true;
					}
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

		/// <summary>
		/// Gets a value indicating whether the item is a quest item.
		/// </summary>
		public bool IsQuestItem
		{
			get
			{
				if (this.baseItemInfo != null)
				{
					if (this.baseItemInfo.ItemClassification.ToUpperInvariant().Equals("QUEST"))
					{
						return true;
					}
				}
				else if (!this.IsImmortalThrone && !this.IsRagnarok && !this.IsAtlantis)
				{
					if (this.BaseItemId.ToUpperInvariant().IndexOf("QUEST", StringComparison.OrdinalIgnoreCase) >= 0)
					{
						return true;
					}
				}
				else if (this.BaseItemId.ToUpperInvariant().IndexOf("QUESTS", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return true;
				}

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
		public bool HasRelic
		{
			get
			{
				return !this.IsRelic && this.relicID.Length > 0;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the item has a second embedded relic.
		/// </summary>
		public bool HasSecondRelic
		{
			get
			{
				return !this.IsRelic && this.relic2ID.Length > 0;
			}
		}

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
		/// Gets a value indicating whether the relic is completed or not.
		/// </summary>
		public bool IsRelicComplete
		{
			get
			{
				if (this.baseItemInfo != null)
				{
					return this.Var1 >= this.baseItemInfo.CompletedRelicLevel;
				}

				return false;
			}
		}

		/// <summary>
		/// Gets the artifact/charm/relic bonus loot table
		/// returns null if the item is not an artifact/charm/relic or does not contain a charm/relic
		/// </summary>
		public LootTableCollection BonusTable
		{
			get
			{
				if (this.baseItemInfo == null)
				{
					return null;
				}

				string lootTableID = null;
				if (this.IsRelic)
				{
					lootTableID = this.baseItemInfo.GetString("bonusTableName");
				}
				else if (this.HasRelic)
				{
					if (this.RelicInfo != null)
					{
						lootTableID = this.RelicInfo.GetString("bonusTableName");
					}
				}
				else if (this.IsArtifact)
				{
					// for artifacts we need to find the formulae that was used to create the artifact.  sucks to be us
					// The formulas seem to always be in the arcaneformulae subfolder with a _formula on the end
					// of the filename
					string folder = Path.GetDirectoryName(this.BaseItemId);
					folder = Path.Combine(folder, "arcaneformulae");
					string file = Path.GetFileNameWithoutExtension(this.BaseItemId);

					// Damn it, IL did not keep the filename consistent on Kingslayer (Sands of Kronos)
					if (file.ToUpperInvariant() == "E_GA_SANDOFKRONOS")
					{
						file = file.Insert(9, "s");
					}

					file = string.Concat(file, "_formula");
					file = Path.Combine(folder, file);
					file = Path.ChangeExtension(file, Path.GetExtension(this.BaseItemId));

					// Now lookup this record.
					DBRecordCollection record = Database.DB.GetRecordFromFile(file);
					if (record != null)
					{
						lootTableID = record.GetString("artifactBonusTableName", 0);
					}
				}

				if (lootTableID != null && lootTableID.Length > 0)
				{
					return new LootTableCollection(lootTableID);
				}

				return null;
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

		#region Item Public Methods

		#region Item Public Static Methods

		/// <summary>
		/// Gets the color for a particular item style
		/// </summary>
		/// <param name="style">ItemStyle enumeration</param>
		/// <returns>System.Drawing.Color for the particular itemstyle</returns>
		public static Color GetColor(ItemStyle style)
		{
			switch (style)
			{
				case ItemStyle.Broken:
					return GetColor(TQColor.DarkGray);

				case ItemStyle.Common:
					return GetColor(TQColor.Yellow);

				case ItemStyle.Epic:
					return GetColor(TQColor.Blue);

				case ItemStyle.Legendary:
					return GetColor(TQColor.Purple);

				case ItemStyle.Mundane:
					return GetColor(TQColor.White);

				case ItemStyle.Potion:
					return GetColor(TQColor.Red);

				case ItemStyle.Quest:
					return GetColor(TQColor.Purple);

				case ItemStyle.Rare:
					return GetColor(TQColor.Green);

				case ItemStyle.Relic:
					return GetColor(TQColor.Orange);

				case ItemStyle.Parchment:
					return GetColor(TQColor.Blue);

				case ItemStyle.Scroll:
					return GetColor(TQColor.YellowGreen);

				case ItemStyle.Formulae:
					return GetColor(TQColor.Turquoise);

				case ItemStyle.Artifact:
					return GetColor(TQColor.Turquoise);

				default:
					return Color.White;
			}
		}

		/// <summary>
		/// Gets the Color for a particular TQ defined color
		/// </summary>
		/// <param name="color">TQ color enumeration</param>
		/// <returns>System.Drawing.Color for the particular TQ color</returns>
		public static Color GetColor(TQColor color)
		{
			switch (color)
			{
				case TQColor.Aqua:
					return Color.FromArgb(0, 255, 255);

				case TQColor.Blue:
					return Color.FromArgb(0, 163, 255);

				case TQColor.DarkGray:
					return Color.FromArgb(153, 153, 153);

				case TQColor.Fuschia:
					return Color.FromArgb(255, 0, 255);

				case TQColor.Green:
					return Color.FromArgb(64, 255, 64);

				case TQColor.Indigo:
					return Color.FromArgb(75, 0, 130);

				case TQColor.Khaki:
					return Color.FromArgb(195, 176, 145);

				case TQColor.LightCyan:
					return Color.FromArgb(224, 255, 255);

				case TQColor.Maroon:
					return Color.FromArgb(128, 0, 0);

				case TQColor.Orange:
					return Color.FromArgb(255, 173, 0);

				case TQColor.Purple:
					return Color.FromArgb(217, 5, 255);

				case TQColor.Red:
					return Color.FromArgb(255, 0, 0);

				case TQColor.Silver:
					return Color.FromArgb(224, 224, 224);

				case TQColor.Turquoise:
					return Color.FromArgb(0, 255, 209);

				case TQColor.White:
					return Color.FromArgb(255, 255, 255);

				case TQColor.Yellow:
					return Color.FromArgb(255, 245, 43);

				case TQColor.YellowGreen:
					return Color.FromArgb(145, 203, 0);

				default:
					return Color.White;
			}
		}

		/// <summary>
		/// Generates a new seed that can be used on a new item
		/// </summary>
		/// <returns>New seed from 0 to 0x7FFF</returns>
		public static int GenerateSeed()
		{
			// The seed values in the player files seem to be limitted to 0x00007fff or less.
			return random.Next(0x00007fff);
		}

		/// <summary>
		/// Removes the color tag from a line of text. Should be called after GetColorTag().
		/// </summary>
		/// <param name="text">text to be clipped</param>
		/// <returns>text with color tag removed</returns>
		public static string ClipColorTag(string text)
		{
			if (text.Contains("{^"))
			{
				int i = text.IndexOf("{^");
				if (i != -1)
				{
					// Make sure there is a control code in there.
					text = text.Remove(i, 4);
				}
			}
			else if (text.StartsWith("^", StringComparison.OrdinalIgnoreCase))
			{
				// If there are not braces assume a 2 character code.
				text = text.Substring(2);
			}
			else if (text.StartsWith("^", StringComparison.OrdinalIgnoreCase))
			{
				// If there are not braces assume a 2 character code.
				text = text.Substring(2);
			}

			return text;
		}

		#endregion Item Public Static Methods

		/// <summary>
		/// Gets the item's color
		/// </summary>
		/// <returns>System.Drawing.Color for the item</returns>
		public Color GetColor()
		{
			return GetColor(this.ItemStyle);
		}

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
		/// Removes the relic from this item
		/// </summary>
		/// <returns>Returns the removed relic as a new Item, if the item has two relics, 
		/// only the first one is returned and the second one is also removed</returns>
		public Item RemoveRelic()
		{
			if (!this.HasRelic)
			{
				return null;
			}

			Item newRelic = this.MakeEmptyCopy(this.relicID);
			newRelic.GetDBData();
			newRelic.RelicBonusId = this.RelicBonusId;
			newRelic.RelicBonusInfo = this.RelicBonusInfo;

			// Now clear out our relic data
			this.relicID = string.Empty;
			this.relic2ID = string.Empty;
			this.RelicBonusId = string.Empty;
			this.RelicBonus2Id = string.Empty;
			this.Var1 = 0;
			this.Var2 = var2Default;
			this.RelicInfo = null;
			this.RelicBonusInfo = null;
			this.Relic2Info = null;
			this.RelicBonus2Info = null;

			this.MarkModified();

			return newRelic;
		}

		/// <summary>
		/// Marks the item as modified
		/// </summary>
		public void MarkModified()
		{
			this.attributesString = null;
			this.requirementsString = null;
			this.setItemsString = null;
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
		/// Create an artifact from its formulae
		/// </summary>
		/// <returns>A new artifact</returns>
		public Item CraftArtifact()
		{
			if (this.IsFormulae && this.baseItemInfo != null)
			{
				string artifactID = this.baseItemInfo.GetString("artifactName");
				Item newArtifact = this.MakeEmptyCopy(artifactID);
				newArtifact.GetDBData();

				this.MarkModified();

				return newArtifact;
			}
			return null;
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

		/// <summary>
		/// Gets a color tag for a line of text
		/// </summary>
		/// <param name="text">text containing the color tag</param>
		/// <returns>System.Drawing.Color of the embedded color code</returns>
		public Color GetColorTag(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				// Use the standard color code for the item
				return GetColor(this.ItemStyle);
			}

			// Look for a formatting tag in the beginning of the string
			string colorCode = null;
			if (text.Contains("{^"))
			{
				int i = text.IndexOf("{^");
				if (i == -1)
				{
					colorCode = null;
				}
				else
				{
					colorCode = text.Substring(i + 2, 1).ToUpperInvariant();
				}
			}
			else if (text.StartsWith("^"))
			{
				// If there are not braces assume a 2 character code.
				colorCode = text.Substring(1, 1).ToUpperInvariant();
			}


			// We didn't find a code so use the standard color code for the item
			if (string.IsNullOrEmpty(colorCode))
			{
				return GetColor(this.ItemStyle);
			}

			// We found something so lets try to find the code
			switch (colorCode)
			{
				case "A":
					return GetColor(TQColor.Aqua);

				case "B":
					return GetColor(TQColor.Blue);

				case "C":
					return GetColor(TQColor.LightCyan);

				case "D":
					return GetColor(TQColor.DarkGray);

				case "F":
					return GetColor(TQColor.Fuschia);

				case "G":
					return GetColor(TQColor.Green);

				case "I":
					return GetColor(TQColor.Indigo);

				case "K":
					return GetColor(TQColor.Khaki);

				case "L":
					return GetColor(TQColor.YellowGreen);

				case "M":
					return GetColor(TQColor.Maroon);

				case "O":
					return GetColor(TQColor.Orange);

				case "P":
					return GetColor(TQColor.Purple);

				case "R":
					return GetColor(TQColor.Red);

				case "S":
					return GetColor(TQColor.Silver);

				case "T":
					return GetColor(TQColor.Turquoise);

				case "W":
					return GetColor(TQColor.White);

				case "Y":
					return GetColor(TQColor.Yellow);

				default:
					return GetColor(this.ItemStyle);
			}
		}

		/// <summary>
		/// Gets a string containing the item name and attributes.
		/// </summary>
		/// <param name="basicInfoOnly">Flag indicating whether or not to return only basic info</param>
		/// <param name="relicInfoOnly">Flag indicating whether or not to return only relic info</param>
		/// /// <param name="secondRelic">Flag indicating whether or not to return second relic info</param>
		/// <returns>A string containing the item name and attributes</returns>
		public string ToString(bool basicInfoOnly = false, bool relicInfoOnly = false, bool secondRelic = false)
		{
			string[] parameters = new string[16];
			int parameterCount = 0;
			Info relicInfoTarget = secondRelic ? this.Relic2Info : this.RelicInfo;
			string relicIdTarget = secondRelic ? this.relic2ID : this.relicID;
			string relicBonusTarget = secondRelic ? this.RelicBonus2Id : this.RelicBonusId;

			if (!relicInfoOnly)
			{
				if (!this.IsRelic && !string.IsNullOrEmpty(this.prefixID))
				{
					if (this.prefixInfo != null)
					{
						parameters[parameterCount] = Database.DB.GetFriendlyName(this.prefixInfo.DescriptionTag);
						if (string.IsNullOrEmpty(parameters[parameterCount]))
						{
							parameters[parameterCount] = this.prefixID;
						}
					}
					else
					{
						parameters[parameterCount] = this.prefixID;
					}

					++parameterCount;
				}

				if (this.baseItemInfo == null)
				{
					parameters[parameterCount++] = this.BaseItemId;
				}
				else
				{
					// style quality description
					if (!string.IsNullOrEmpty(this.baseItemInfo.StyleTag))
					{
						if (!this.IsPotion && !this.IsRelic && !this.IsScroll && !this.IsParchment && !this.IsQuestItem)
						{
							parameters[parameterCount] = Database.DB.GetFriendlyName(this.baseItemInfo.StyleTag);
							if (string.IsNullOrEmpty(parameters[parameterCount]))
							{
								parameters[parameterCount] = this.baseItemInfo.StyleTag;
							}

							++parameterCount;
						}
					}

					if (!string.IsNullOrEmpty(this.baseItemInfo.QualityTag))
					{
						parameters[parameterCount] = Database.DB.GetFriendlyName(this.baseItemInfo.QualityTag);
						if (string.IsNullOrEmpty(parameters[parameterCount]))
						{
							parameters[parameterCount] = this.baseItemInfo.QualityTag;
						}

						++parameterCount;
					}

					parameters[parameterCount] = Database.DB.GetFriendlyName(this.baseItemInfo.DescriptionTag);
					if (string.IsNullOrEmpty(parameters[parameterCount]))
					{
						parameters[parameterCount] = this.BaseItemId;
					}

					++parameterCount;

					if (!basicInfoOnly && this.IsRelic)
					{
						// Add the number of charms in the set acquired.
						if (this.IsRelicComplete)
						{
							if (!string.IsNullOrEmpty(relicBonusTarget))
							{
								string bonus = Path.GetFileNameWithoutExtension(TQData.NormalizeRecordPath(relicBonusTarget));
								string tag = "tagRelicBonus";
								if (this.IsCharm)
								{
									tag = "tagAnimalPartcompleteBonus";
								}

								string bonusTitle = Database.DB.GetFriendlyName(tag);
								if (string.IsNullOrEmpty(bonusTitle))
								{
									bonusTitle = "Completion Bonus: ";
								}

								parameters[parameterCount] = string.Format(CultureInfo.CurrentCulture, "({0} {1})", bonusTitle, bonus);
							}
							else
							{
								string tag = "tagRelicComplete";
								if (this.IsCharm)
								{
									tag = "tagAnimalPartComplete";
								}

								string completed = Database.DB.GetFriendlyName(tag);
								if (string.IsNullOrEmpty(completed))
								{
									completed = "Completed";
								}

								parameters[parameterCount] = string.Concat("(", completed, ")");
							}
						}
						else
						{
							string tag1 = "tagRelicShard";
							string tag2 = "tagRelicRatio";
							if (this.IsCharm)
							{
								tag1 = "tagAnimalPart";
								tag2 = "tagAnimalPartRatio";
							}

							string type = Database.DB.GetFriendlyName(tag1);
							if (string.IsNullOrEmpty(type))
							{
								type = "Relic";
							}

							string formatSpec = Database.DB.GetFriendlyName(tag2);
							if (string.IsNullOrEmpty(formatSpec))
							{
								formatSpec = "{0} - {1} / {2}";
							}
							else
							{
								formatSpec = ItemAttributes.ConvertFormat(formatSpec);
							}

							parameters[parameterCount] = string.Concat("(", Item.Format(formatSpec, type, this.Number, this.baseItemInfo.CompletedRelicLevel), ")");
						}

						++parameterCount;
					}
					else if (!basicInfoOnly && this.IsArtifact)
					{
						// Added by VillageIdiot
						// Add Artifact completion bonus
						if (!string.IsNullOrEmpty(this.RelicBonusId))
						{
							string bonus = Path.GetFileNameWithoutExtension(TQData.NormalizeRecordPath(this.RelicBonusId));
							string tag = "xtagArtifactBonus";
							string bonusTitle = Database.DB.GetFriendlyName(tag);
							if (string.IsNullOrEmpty(bonusTitle))
							{
								bonusTitle = "Completion Bonus: ";
							}

							parameters[parameterCount] = string.Format(CultureInfo.CurrentCulture, "({0} {1})", bonusTitle, bonus);
						}

						++parameterCount;
					}
					else if (this.DoesStack)
					{
						// display the # potions
						if (this.Number > 1)
						{
							parameters[parameterCount] = string.Format(CultureInfo.CurrentCulture, "({0:n0})", this.Number);
							++parameterCount;
						}
					}
				}

				if (!this.IsRelic && this.suffixID.Length > 0)
				{
					if (this.suffixInfo != null)
					{
						parameters[parameterCount] = Database.DB.GetFriendlyName(this.suffixInfo.DescriptionTag);
						if (string.IsNullOrEmpty(parameters[parameterCount]))
						{
							parameters[parameterCount] = this.suffixID;
						}
					}
					else
					{
						parameters[parameterCount] = this.suffixID;
					}

					++parameterCount;
				}
			}

			if (!basicInfoOnly && this.HasRelic)
			{
				if (!relicInfoOnly)
				{
					parameters[parameterCount++] = Item.ItemWith;
				}

				if (relicInfoTarget != null)
				{
					parameters[parameterCount] = Database.DB.GetFriendlyName(relicInfoTarget.DescriptionTag);
					if (string.IsNullOrEmpty(parameters[parameterCount]))
					{
						parameters[parameterCount] = relicIdTarget;
					}
				}
				else
				{
					parameters[parameterCount] = relicIdTarget;
				}

				++parameterCount;

				// Add the number of charms in the set acquired.
				if (!relicInfoOnly)
				{
					if (relicInfoTarget != null)
					{
						if (this.Var1 >= relicInfoTarget.CompletedRelicLevel)
						{
							if (!relicInfoOnly && !string.IsNullOrEmpty(relicBonusTarget))
							{
								string bonus = Path.GetFileNameWithoutExtension(TQData.NormalizeRecordPath(relicBonusTarget));
								parameters[parameterCount] = string.Format(CultureInfo.CurrentCulture, Item.ItemRelicBonus, bonus);
							}
							else
							{
								parameters[parameterCount] = Item.ItemRelicCompleted;
							}
						}
						else
						{
							parameters[parameterCount] = string.Format(CultureInfo.CurrentCulture, "({0:n0}/{1:n0})", Math.Max(1, this.Var1), relicInfoTarget.CompletedRelicLevel);
						}
					}
					else
					{
						parameters[parameterCount] = string.Format(CultureInfo.CurrentCulture, "({0:n0}/??)", this.Var1);
					}

					++parameterCount;
				}
			}

			if (!relicInfoOnly && this.IsQuestItem)
			{
				parameters[parameterCount++] = Item.ItemQuest;
			}

			if (!basicInfoOnly && !relicInfoOnly)
			{
				if (this.IsImmortalThrone)
				{
					parameters[parameterCount++] = "(IT)";
				}
				else if (this.IsRagnarok)
				{
					parameters[parameterCount++] = "(RAG)";
				}
				else if (this.IsAtlantis)
				{
					parameters[parameterCount++] = "(ATL)";
				}
			}

			// Now combine it all with spaces between
			return string.Join(" ", parameters, 0, parameterCount);
		}

		/// <summary>
		/// Gets the item's requirements
		/// </summary>
		/// <returns>A string containing the items requirements</returns>
		public string GetRequirements()
		{
			if (this.requirementsString != null)
			{
				return this.requirementsString;
			}
			SortedList<string, Variable> requirementsList = GetRequirementVariables();

			// Get the format string to use to list a requirement
			string requirementFormat = Database.DB.GetFriendlyName("MeetsRequirement");
			if (requirementFormat == null)
			{
				// could not find one.  make up one.
				requirementFormat = "?Required? {0}: {1:f0}";
			}
			else
			{
				requirementFormat = ItemAttributes.ConvertFormat(requirementFormat);
			}

			// Now combine it all with spaces between
			List<string> requirements = new List<string>();
			foreach (KeyValuePair<string, Variable> kvp in requirementsList)
			{
				if (TQDebug.ItemDebugLevel > 1)
				{
					TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "Retrieving requirement {0}={1} (type={2})", kvp.Key, kvp.Value, kvp.Value.GetType().ToString()));
				}

				Variable variable = kvp.Value;

				// Format the requirement
				string requirementsText;
				if (variable.NumberOfValues > 1 || variable.DataType == VariableDataType.StringVar)
				{
					// reqs should only have 1 entry and should be a number type.  We must punt on this one
					requirementsText = string.Concat(kvp.Key, ": ", variable.ToStringValue());
				}
				else
				{
					// get the name of this requirement
					string reqName = Database.DB.GetFriendlyName(kvp.Key);
					if (reqName == null)
					{
						reqName = string.Concat("?", kvp.Key, "?");
					}

					// Now apply the format string
					requirementsText = Item.Format(requirementFormat, reqName, variable[0]);
				}

				// Changed by VillageIdiot - Change requirement text to Grey
				requirementsText = string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Broken)), Database.MakeSafeForHtml(requirementsText));
				requirements.Add(requirementsText);
			}

			// Now convert array of strings to a single string with <br>'s
			if (requirements.Count > 0)
			{
				string[] requirementsArray = new string[requirements.Count];
				requirements.CopyTo(requirementsArray);
				this.requirementsString = string.Join("<br>", requirementsArray);
			}
			else
			{
				this.requirementsString = string.Empty;
			}

			return this.requirementsString;
		}

		private SortedList<string, Variable> requirementsList;
		public SortedList<string, Variable> GetRequirementVariables()
		{
			if (this.requirementsList != null)
			{
				return this.requirementsList;
			}

			requirementsList = new SortedList<string, Variable>();
			if (this.baseItemInfo != null)
			{
				GetRequirementsFromRecord(requirementsList, Database.DB.GetRecordFromFile(this.BaseItemId));
				this.GetDynamicRequirementsFromRecord(requirementsList, this.baseItemInfo);
			}

			if (this.prefixInfo != null)
			{
				GetRequirementsFromRecord(requirementsList, Database.DB.GetRecordFromFile(this.prefixID));
			}

			if (this.suffixInfo != null)
			{
				GetRequirementsFromRecord(requirementsList, Database.DB.GetRecordFromFile(this.suffixID));
			}

			if (this.RelicInfo != null)
			{
				GetRequirementsFromRecord(requirementsList, Database.DB.GetRecordFromFile(this.relicID));
			}

			// Add Artifact level requirement to formula
			if (this.IsFormulae && this.baseItemInfo != null)
			{
				string artifactID = this.baseItemInfo.GetString("artifactName");
				GetRequirementsFromRecord(requirementsList, Database.DB.GetRecordFromFile(artifactID));
			}

			return requirementsList;
		}

		/// <summary>
		/// Gets the item's attributes
		/// </summary>
		/// <param name="filtering">Flag indicating whether or not we are filtering strings</param>
		/// <returns>returns a string containing the item's attributes</returns>
		public string GetAttributes(bool filtering)
		{
			if (this.attributesString != null)
			{
				return this.attributesString;
			}

			List<string> results = new List<string>();

			// Add the item name
			string itemName = Database.MakeSafeForHtml(this.ToString(true, false));
			Color color = this.GetColorTag(itemName);
			itemName = ClipColorTag(itemName);
			results.Add(string.Format(CultureInfo.CurrentCulture, "<font size={0} color={1}><b>{2}</b></font>", Convert.ToInt32(10.0F * Database.DB.Scale), Database.HtmlColor(color), itemName));

			// Add the sub-title for certain types
			if (this.baseItemInfo != null)
			{
				if (this.IsRelic)
				{
					string str;
					if (!this.IsRelicComplete)
					{
						string tag1 = "tagRelicShard";
						string tag2 = "tagRelicRatio";
						if (this.IsCharm)
						{
							tag1 = "tagAnimalPart";
							tag2 = "tagAnimalPartRatio";
						}

						string type = Database.DB.GetFriendlyName(tag1);
						if (type == null)
						{
							type = "?Relic?";
						}

						string formatSpec = Database.DB.GetFriendlyName(tag2);
						if (formatSpec == null)
						{
							formatSpec = "?{0} - {1} / {2}?";
						}
						else
						{
							formatSpec = ItemAttributes.ConvertFormat(formatSpec);
						}

						str = Item.Format(formatSpec, type, this.Number, this.baseItemInfo.CompletedRelicLevel);
					}
					else
					{
						string tag = "tagRelicComplete";
						if (this.IsCharm)
						{
							tag = "tagAnimalPartComplete";
						}

						str = Database.DB.GetFriendlyName(tag);
						if (str == null)
						{
							str = "?Completed Relic/Charm?";
						}
					}

					str = Database.MakeSafeForHtml(str);
					Color color1 = this.GetColorTag(str);
					str = ClipColorTag(str);
					results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(color1), str));
				}
				else if (this.IsArtifact)
				{
					// Added by VillageIdiot
					// Show Artifact Class (Lesser / Greater / Divine).
					string tag;
					string artifactClass;
					string artifactClassification = this.baseItemInfo.GetString("artifactClassification").ToUpperInvariant();

					if (artifactClassification == null)
					{
						tag = null;
					}
					else if (artifactClassification == "LESSER")
					{
						tag = "xtagArtifactClass01";
					}
					else if (artifactClassification == "GREATER")
					{
						tag = "xtagArtifactClass02";
					}
					else if (artifactClassification == "DIVINE")
					{
						tag = "xtagArtifactClass03";
					}
					else
					{
						tag = null;
					}

					if (tag != null)
					{
						artifactClass = Database.DB.GetFriendlyName(tag);
					}
					else
					{
						artifactClass = "?Unknown Artifact Class?";
					}

					artifactClass = Database.MakeSafeForHtml(artifactClass);
					results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(GetColor(ItemStyle.Broken)), artifactClass));
				}

				// Added by VillageIdiot
				// Show Formulae Reagents.
				if (this.IsFormulae)
				{
					string str;
					string tag = "xtagArtifactRecipe";

					// Added to show recipe type for Formulae
					string recipe = Database.DB.GetFriendlyName(tag);
					if (recipe == null)
					{
						recipe = "Recipe";
					}

					results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font><br>", Database.HtmlColor(GetColor(ItemStyle.Mundane)), recipe));

					tag = "xtagArtifactReagents";

					// Get Reagents format
					string formatSpec = Database.DB.GetFriendlyName(tag);
					if (formatSpec == null)
					{
						formatSpec = "Required Reagents  ({0}/{1})";
					}
					else
					{
						formatSpec = ItemAttributes.ConvertFormat(formatSpec);
					}

					// it looks like the formulae reagents is hard coded at 3
					str = Item.Format(formatSpec, (object)0, 3);
					str = Database.MakeSafeForHtml(str);
					results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(GetColor(ItemStyle.Relic)), str));
				}
			}

			// Add flavor text
			// Changed by VillageIdiot
			// Removed Scroll flavor text since it gets printed by the skill effect code
			if ((this.IsQuestItem || this.IsRelic || this.IsPotion || this.IsParchment || this.IsScroll) && this.baseItemInfo != null && this.baseItemInfo.StyleTag.Length > 0)
			{
				string tag = this.baseItemInfo.StyleTag;
				string flavor = Database.DB.GetFriendlyName(tag);
				if (flavor != null)
				{
					flavor = Database.MakeSafeForHtml(flavor);
					Collection<string> flavorTextArray = Database.WrapWords(flavor, 30);

					foreach (string flavorTextRow in flavorTextArray)
					{
						int nextColor = flavorTextRow.IndexOf("{^y}", StringComparison.OrdinalIgnoreCase);
						if (nextColor > -1)
						{
							string choppedString = flavorTextRow.Substring(4);
							results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(TQColor.Yellow)), choppedString));
						}
						else
						{
							results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Mundane)), flavorTextRow));
						}
					}

					results.Add(string.Empty);
				}
			}

			if (this.baseItemInfo != null)
			{
				this.GetAttributesFromRecord(Database.DB.GetRecordFromFile(this.BaseItemId), filtering, this.BaseItemId, results);
			}

			if (this.prefixInfo != null)
			{
				this.GetAttributesFromRecord(Database.DB.GetRecordFromFile(this.prefixID), filtering, this.prefixID, results);
			}

			if (this.suffixInfo != null)
			{
				this.GetAttributesFromRecord(Database.DB.GetRecordFromFile(this.suffixID), filtering, this.suffixID, results);
			}

			if (this.RelicInfo != null)
			{
				List<string> r = new List<string>();
				this.GetAttributesFromRecord(Database.DB.GetRecordFromFile(this.relicID), filtering, this.relicID, r);
				if (r.Count > 0)
				{
					string colorTag = string.Format(CultureInfo.CurrentCulture, "<hr color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Broken)));

					string relicName = Database.MakeSafeForHtml(this.ToString(false, true));

					// display the relic name
					results.Add(string.Format(
						CultureInfo.CurrentUICulture,
						"{2}<font size=+1 color={0}><b>{1}</b></font>",
						Database.HtmlColor(Item.GetColor(ItemStyle.Relic)),
						relicName,
						colorTag));

					// display the relic subtitle
					string str;
					if (this.Var1 < this.RelicInfo.CompletedRelicLevel)
					{
						string tag1 = "tagRelicShard";
						string tag2 = "tagRelicRatio";
						if (this.RelicInfo.ItemClass.ToUpperInvariant().Equals("ITEMCHARM"))
						{
							tag1 = "tagAnimalPart";
							tag2 = "tagAnimalPartRatio";
						}

						string type = Database.DB.GetFriendlyName(tag1);
						if (type == null)
						{
							type = "?Relic?";
						}

						string formatSpec = Database.DB.GetFriendlyName(tag2);
						if (formatSpec == null)
						{
							formatSpec = "?{0} - {1} / {2}?";
						}
						else
						{
							formatSpec = ItemAttributes.ConvertFormat(formatSpec);
						}

						str = Item.Format(formatSpec, type, Math.Max(1, this.Var1), this.RelicInfo.CompletedRelicLevel);
					}
					else
					{
						string tag = "tagRelicComplete";
						if (this.RelicInfo.ItemClass.ToUpperInvariant().Equals("ITEMCHARM"))
						{
							tag = "tagAnimalPartComplete";
						}

						str = Database.DB.GetFriendlyName(tag);
						if (str == null)
						{
							str = "?Completed Relic/Charm?";
						}
					}

					str = Database.MakeSafeForHtml(str);
					results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(GetColor(ItemStyle.Relic)), str));

					// display the relic bonuses
					results.AddRange(r);
				}
			}

			// Added by VillageIdiot
			// Show the Artifact completion bonus.
			if (this.IsArtifact && this.RelicBonusInfo != null)
			{
				List<string> r = new List<string>();
				this.GetAttributesFromRecord(Database.DB.GetRecordFromFile(this.RelicBonusId), filtering, this.RelicBonusId, r);
				if (r.Count > 0)
				{
					string tag = "xtagArtifactBonus";
					string bonusTitle = Database.DB.GetFriendlyName(tag);
					if (bonusTitle == null)
					{
						bonusTitle = "Completion Bonus: ";
					}

					string title = Database.MakeSafeForHtml(bonusTitle);

					results.Add(string.Empty);
					results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Relic)), title));
					results.AddRange(r);
				}
			}

			if ((this.IsRelic || this.HasRelic) && this.RelicBonusInfo != null)
			{
				List<string> r = new List<string>();
				this.GetAttributesFromRecord(Database.DB.GetRecordFromFile(this.RelicBonusId), filtering, this.RelicBonusId, r);
				if (r.Count > 0)
				{
					string tag = "tagRelicBonus";
					if (this.IsCharm || (this.HasRelic && this.RelicInfo.ItemClass.ToUpperInvariant().Equals("ITEMCHARM")))
					{
						tag = "tagAnimalPartcompleteBonus";
					}

					string bonusTitle = Database.DB.GetFriendlyName(tag);
					if (bonusTitle == null)
					{
						bonusTitle = "?Completed Relic/Charm Bonus:?";
					}

					string title = Database.MakeSafeForHtml(bonusTitle);

					results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Relic)), title));
					results.AddRange(r);
				}
			}

			if (this.Relic2Info != null)
			{
				List<string> r = new List<string>();
				this.GetAttributesFromRecord(Database.DB.GetRecordFromFile(this.relic2ID), filtering, this.relic2ID, r);
				if (r.Count > 0)
				{
					string colorTag = string.Format(CultureInfo.CurrentCulture, "<hr color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Broken)));

					string relicName = Database.MakeSafeForHtml(this.ToString(false, true, true));

					// display the relic name
					results.Add(string.Format(
						CultureInfo.CurrentUICulture,
						"{2}<font size=+1 color={0}><b>{1}</b></font>",
						Database.HtmlColor(Item.GetColor(ItemStyle.Relic)),
						relicName,
						colorTag));

					// display the relic subtitle
					string str;
					if (this.Var2 < this.Relic2Info.CompletedRelicLevel)
					{
						string tag1 = "tagRelicShard";
						string tag2 = "tagRelicRatio";
						if (this.Relic2Info.ItemClass.ToUpperInvariant().Equals("ITEMCHARM"))
						{
							tag1 = "tagAnimalPart";
							tag2 = "tagAnimalPartRatio";
						}

						string type = Database.DB.GetFriendlyName(tag1);
						if (type == null)
						{
							type = "?Relic?";
						}

						string formatSpec = Database.DB.GetFriendlyName(tag2);
						if (formatSpec == null)
						{
							formatSpec = "?{0} - {1} / {2}?";
						}
						else
						{
							formatSpec = ItemAttributes.ConvertFormat(formatSpec);
						}

						str = Item.Format(formatSpec, type, Math.Max(1, this.Var2), this.Relic2Info.CompletedRelicLevel);
					}
					else
					{
						string tag = "tagRelicComplete";
						if (this.Relic2Info.ItemClass.ToUpperInvariant().Equals("ITEMCHARM"))
						{
							tag = "tagAnimalPartComplete";
						}

						str = Database.DB.GetFriendlyName(tag);
						if (str == null)
						{
							str = "?Completed Relic/Charm?";
						}
					}

					str = Database.MakeSafeForHtml(str);
					results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(GetColor(ItemStyle.Relic)), str));

					// display the relic bonuses
					results.AddRange(r);
				}

				if (this.HasSecondRelic && (this.RelicBonus2Info != null))
				{
					r = new List<string>();
					this.GetAttributesFromRecord(Database.DB.GetRecordFromFile(this.RelicBonus2Id), filtering, this.RelicBonus2Id, r);
					if (r.Count > 0)
					{
						string tag = "tagRelicBonus";
						if (this.IsCharm || (this.HasRelic && this.Relic2Info.ItemClass.ToUpperInvariant().Equals("ITEMCHARM")))
						{
							tag = "tagAnimalPartcompleteBonus";
						}

						string bonusTitle = Database.DB.GetFriendlyName(tag);
						if (bonusTitle == null)
						{
							bonusTitle = "?Completed Relic/Charm Bonus:?";
						}

						string title = Database.MakeSafeForHtml(bonusTitle);

						results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Relic)), title));
						results.AddRange(r);
					}
				}
			}

			// Added by VillageIdiot
			// Shows Artifact stats for the formula
			if (this.IsFormulae && this.baseItemInfo != null)
			{
				List<string> r = new List<string>();
				string artifactID = this.baseItemInfo.GetString("artifactName");
				DBRecordCollection artifactRecord = Database.DB.GetRecordFromFile(artifactID);
				if (artifactID != null)
				{
					this.GetAttributesFromRecord(artifactRecord, filtering, artifactID, r);
					if (r.Count > 0)
					{
						string tag;

						// Display the name of the Artifact
						string artifactClass = Database.DB.GetFriendlyName(artifactRecord.GetString("description", 0));
						if (string.IsNullOrEmpty(artifactClass))
						{
							artifactClass = "?Unknown Artifact Name?";
						}

						string artifactName = Database.MakeSafeForHtml(artifactClass);
						results.Add(string.Empty);
						results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Artifact)), artifactName));

						// Display the class of the Artifact
						string artifactClassification = artifactRecord.GetString("artifactClassification", 0).ToUpperInvariant();
						if (artifactClassification == null)
						{
							tag = null;
						}
						else if (artifactClassification == "LESSER")
						{
							tag = "xtagArtifactClass01";
						}
						else if (artifactClassification == "GREATER")
						{
							tag = "xtagArtifactClass02";
						}
						else if (artifactClassification == "DIVINE")
						{
							tag = "xtagArtifactClass03";
						}
						else
						{
							tag = null;
						}

						if (tag != null)
						{
							artifactClass = Database.DB.GetFriendlyName(tag);
						}
						else
						{
							artifactClass = "?Unknown Artifact Class?";
						}

						artifactClass = Database.MakeSafeForHtml(artifactClass);
						results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(GetColor(ItemStyle.Broken)), artifactClass));

						// Add the stats
						results.AddRange(r);
					}
				}
			}

			// Add the item seed
			string hr1 = string.Format(CultureInfo.CurrentCulture, "<hr color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Broken)));
			string itemSeedString = Database.MakeSafeForHtml(string.Format(CultureInfo.CurrentCulture, Item.ItemSeed, this.Seed, (this.Seed != 0) ? (this.Seed / (float)Int16.MaxValue) : 0.0f));
			results.Add(string.Format(CultureInfo.CurrentCulture, "{2}<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Broken)), itemSeedString, hr1));

			// Add the Immortal Throne clause
			if (this.IsImmortalThrone)
			{
				string immortalThrone = Database.MakeSafeForHtml(Item.ItemIT);
				results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Rare)), immortalThrone));
			}

			// Add the Ragnarok clause
			if (this.IsRagnarok)
			{
				string ragnarok = Database.MakeSafeForHtml(Item.ItemRagnarok);
				results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Rare)), ragnarok));
			}

			// Add the Atlantis clause
			if (this.IsAtlantis)
			{
				string atlantis = Database.MakeSafeForHtml(Item.ItemAtlantis);
				results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Rare)), atlantis));
			}

			string[] ary = new string[results.Count];
			results.CopyTo(ary);
			this.attributesString = string.Join("<br>", ary);

			return this.attributesString;
		}

		/// <summary>
		/// Clears the stored attributes string so that it can be recreated
		/// </summary>
		public void RefreshBareAttributes()
		{
			Array.Clear(this.bareAttributes, 0, this.bareAttributes.Length);
		}

		/// <summary>
		/// Shows the bare attributes for properties display
		/// </summary>
		/// <param name="filtering">flag for filtering strings</param>
		/// <returns>string array containing the bare item attributes</returns>
		public string[] GetBareAttributes(bool filtering)
		{
			if (this.bareAttributes[0] != null)
			{
				return this.bareAttributes;
			}

			List<string> results = new List<string>();

			if (this.baseItemInfo != null)
			{
				string style = string.Empty;
				string quality = string.Empty;

				if (this.baseItemInfo.StyleTag.Length > 0)
				{
					if (!this.IsPotion && !this.IsRelic && !this.IsScroll && !this.IsParchment && !this.IsQuestItem)
					{
						style = string.Concat(Database.DB.GetFriendlyName(this.baseItemInfo.StyleTag), " ");
					}
				}

				if (this.baseItemInfo.QualityTag.Length > 0)
				{
					quality = string.Concat(Database.DB.GetFriendlyName(this.baseItemInfo.QualityTag), " ");
				}

				results.Add(string.Format(
					CultureInfo.CurrentCulture,
					"<font color={0}>{1}</font>",
					Database.HtmlColor(Item.GetColor(ItemStyle.Rare)),
					string.Concat(style, quality, Database.DB.GetFriendlyName(this.baseItemInfo.DescriptionTag))));

				results.Add(string.Format(
					CultureInfo.CurrentCulture,
					"<font color={0}>{1}</font>",
					Database.HtmlColor(Item.GetColor(ItemStyle.Relic)),
					this.BaseItemId));

				this.GetAttributesFromRecord(Database.DB.GetRecordFromFile(this.BaseItemId), filtering, this.BaseItemId, results, false);
			}

			if (results != null)
			{
				string[] ary = new string[results.Count];
				results.CopyTo(ary);
				this.bareAttributes[0] = string.Join("<br>", ary);
				results.Clear();
			}
			else
			{
				this.bareAttributes[0] = string.Empty;
			}

			if (this.prefixInfo != null)
			{
				results.Add(string.Format(
					CultureInfo.CurrentCulture,
					"<font color={0}>{1}</font>",
					Database.HtmlColor(Item.GetColor(ItemStyle.Rare)),
					Database.DB.GetFriendlyName(this.prefixInfo.DescriptionTag)));

				results.Add(string.Format(
					CultureInfo.CurrentCulture,
					"<font color={0}>{1}</font>",
					Database.HtmlColor(Item.GetColor(ItemStyle.Relic)),
					this.prefixID));

				this.GetAttributesFromRecord(Database.DB.GetRecordFromFile(this.prefixID), filtering, this.prefixID, results, false);
			}

			if (results != null)
			{
				string[] ary = new string[results.Count];
				results.CopyTo(ary);
				this.bareAttributes[2] = string.Join("<br>", ary);
				results.Clear();
			}
			else
			{
				this.bareAttributes[2] = string.Empty;
			}

			if (this.suffixInfo != null)
			{
				results.Add(string.Format(
					CultureInfo.CurrentCulture,
					"<font color={0}>{1}</font>",
					Database.HtmlColor(Item.GetColor(ItemStyle.Rare)),
					Database.DB.GetFriendlyName(this.suffixInfo.DescriptionTag)));

				results.Add(string.Format(
					CultureInfo.CurrentCulture,
					"<font color={0}>{1}</font>",
					Database.HtmlColor(Item.GetColor(ItemStyle.Relic)),
					this.suffixID));

				this.GetAttributesFromRecord(Database.DB.GetRecordFromFile(this.suffixID), filtering, this.suffixID, results, false);
			}

			if (results != null)
			{
				string[] ary = new string[results.Count];
				results.CopyTo(ary);
				this.bareAttributes[3] = string.Join("<br>", ary);
				results.Clear();
			}
			else
			{
				this.bareAttributes[3] = string.Empty;
			}

			return this.bareAttributes;
		}

		/// <summary>
		/// Gets the itemID's of all the items in the set.
		/// </summary>
		/// <param name="includeName">Flag to include the set name in the returned array</param>
		/// <returns>Returns a string array containing the remaining set items or null if the item is not part of a set.</returns>
		public string[] GetSetItems(bool includeName)
		{
			if (this.baseItemInfo == null)
			{
				return null;
			}

			string setID = this.baseItemInfo.GetString("itemSetName");
			if (string.IsNullOrEmpty(setID))
			{
				return null;
			}

			// Get the set record
			DBRecordCollection setRecord = Database.DB.GetRecordFromFile(setID);
			if (setRecord == null)
			{
				return null;
			}

			string[] ans = setRecord.GetAllStrings("setMembers");
			if (ans == null || ans.Length == 0)
			{
				return null;
			}

			// Added by VillageIdiot to support set Name
			if (includeName)
			{
				string[] setitems = new string[ans.Length + 1];
				setitems[0] = setRecord.GetString("setName", 0);
				ans.CopyTo(setitems, 1);
				return setitems;
			}
			else
			{
				return ans;
			}
		}

		/// <summary>
		/// Shows the items in a set for the set items
		/// </summary>
		/// <returns>string containing the set items</returns>
		public string GetItemSetString()
		{
			if (this.setItemsString != null)
			{
				return this.setItemsString;
			}

			string[] setMembers = this.GetSetItems(true);
			if (setMembers != null)
			{
				string[] results = new string[setMembers.Length];
				int i = 0;
				foreach (string s in setMembers)
				{
					string name;

					// Changed by VillageIdiot
					// The first entry is now the set name
					if (i == 0)
					{
						name = Database.DB.GetFriendlyName(s);
						results[i++] = string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Rare)), name);
					}
					else
					{
						Info info = Database.DB.GetInfo(s);
						name = string.Concat("&nbsp;&nbsp;&nbsp;&nbsp;", Database.DB.GetFriendlyName(info.DescriptionTag));
						results[i++] = string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Common)), name);
					}
				}

				this.setItemsString = string.Join("<br>", results);
			}
			else
			{
				this.setItemsString = string.Empty;
			}

			return this.setItemsString;
		}

		/// <summary>
		/// Encodes an item into the save file format
		/// </summary>
		/// <param name="writer">BinaryWriter instance</param>
		public void Encode(BinaryWriter writer)
		{
			int itemCount = this.StackSize;
			int cx = this.PositionX;
			int cy = this.PositionY;
			int cseed = this.Seed;

			if (this.ContainerType != SackType.Sack && this.ContainerType != SackType.Player)
			{
				// Equipment, Stashes, Vaults
				if (this.ContainerType == SackType.Stash)
				{
					TQData.WriteCString(writer, "stackCount");
					writer.Write(itemCount - 1);
				}

				TQData.WriteCString(writer, "begin_block");
				writer.Write(this.beginBlockCrap2);

				TQData.WriteCString(writer, "baseName");
				TQData.WriteCString(writer, this.BaseItemId);

				TQData.WriteCString(writer, "prefixName");
				TQData.WriteCString(writer, this.prefixID);

				TQData.WriteCString(writer, "suffixName");
				TQData.WriteCString(writer, this.suffixID);

				TQData.WriteCString(writer, "relicName");
				TQData.WriteCString(writer, this.relicID);

				TQData.WriteCString(writer, "relicBonus");
				TQData.WriteCString(writer, this.RelicBonusId);

				TQData.WriteCString(writer, "seed");
				writer.Write(cseed);

				TQData.WriteCString(writer, "var1");
				writer.Write(this.Var1);

				if (atlantis)
				{
					TQData.WriteCString(writer, "relicName2");
					TQData.WriteCString(writer, this.relic2ID);

					TQData.WriteCString(writer, "relicBonus2");
					TQData.WriteCString(writer, this.RelicBonus2Id);

					TQData.WriteCString(writer, "var2");
					writer.Write(this.Var2);
				}
				TQData.WriteCString(writer, "end_block");
				writer.Write(this.endBlockCrap2);

				if (this.ContainerType == SackType.Stash)
				{
					TQData.WriteCString(writer, "xOffset");
					writer.Write(Convert.ToSingle(cx, CultureInfo.InvariantCulture));

					TQData.WriteCString(writer, "yOffset");
					writer.Write(Convert.ToSingle(cy, CultureInfo.InvariantCulture));
				}
			}
			else
			{
				// This is a sack
				// enter a while() loop so we can print out each potion in the stack if it is a potion stack
				while (true)
				{
					TQData.WriteCString(writer, "begin_block");
					writer.Write(this.beginBlockCrap1);

					TQData.WriteCString(writer, "begin_block");
					writer.Write(this.beginBlockCrap2);

					TQData.WriteCString(writer, "baseName");
					TQData.WriteCString(writer, this.BaseItemId);

					TQData.WriteCString(writer, "prefixName");
					TQData.WriteCString(writer, this.prefixID);

					TQData.WriteCString(writer, "suffixName");
					TQData.WriteCString(writer, this.suffixID);

					TQData.WriteCString(writer, "relicName");
					TQData.WriteCString(writer, this.relicID);

					TQData.WriteCString(writer, "relicBonus");
					TQData.WriteCString(writer, this.RelicBonusId);

					TQData.WriteCString(writer, "seed");
					writer.Write(cseed);

					TQData.WriteCString(writer, "var1");
					writer.Write(this.Var1);

					if (atlantis)
					{
						TQData.WriteCString(writer, "relicName2");
						TQData.WriteCString(writer, this.relic2ID);

						TQData.WriteCString(writer, "relicBonus2");
						TQData.WriteCString(writer, this.RelicBonus2Id);

						TQData.WriteCString(writer, "var2");
						writer.Write(this.Var2);
					}
					TQData.WriteCString(writer, "end_block");
					writer.Write(this.endBlockCrap2);

					TQData.WriteCString(writer, "pointX");
					writer.Write(cx);

					TQData.WriteCString(writer, "pointY");
					writer.Write(cy);

					TQData.WriteCString(writer, "end_block");
					writer.Write(this.endBlockCrap1);

					if (!this.DoesStack)
					{
						return;
					}

					if (itemCount <= 1)
					{
						return;
					}

					// we have more items in the stack to write().
					--itemCount;
					cx = -1;
					cy = -1;
					cseed = GenerateSeed(); // get a new seed for the next potion
				}
			}
		}

		/// <summary>
		/// Parses an item from the save file format
		/// </summary>
		/// <param name="reader">BinaryReader instance</param>
		public void Parse(BinaryReader reader)
		{
			try
			{
				if (this.ContainerType == SackType.Stash)
				{
					TQData.ValidateNextString("stackCount", reader);
					this.beginBlockCrap1 = reader.ReadInt32();
				}
				else if (this.ContainerType == SackType.Sack || this.ContainerType == SackType.Player)
				{
					TQData.ValidateNextString("begin_block", reader); // make sure we just read a new block
					this.beginBlockCrap1 = reader.ReadInt32();
				}

				TQData.ValidateNextString("begin_block", reader); // make sure we just read a new block
				this.beginBlockCrap2 = reader.ReadInt32();

				TQData.ValidateNextString("baseName", reader);
				this.BaseItemId = TQData.ReadCString(reader);

				TQData.ValidateNextString("prefixName", reader);
				this.prefixID = TQData.ReadCString(reader);

				TQData.ValidateNextString("suffixName", reader);
				this.suffixID = TQData.ReadCString(reader);

				TQData.ValidateNextString("relicName", reader);
				this.relicID = TQData.ReadCString(reader);

				TQData.ValidateNextString("relicBonus", reader);
				this.RelicBonusId = TQData.ReadCString(reader);

				TQData.ValidateNextString("seed", reader);
				this.Seed = reader.ReadInt32();

				TQData.ValidateNextString("var1", reader);
				this.Var1 = reader.ReadInt32();

				if(TQData.MatchNextString("relicName2", reader))
				{
					string label = "relicName2";
					TQData.ValidateNextString("relicName2", reader);
					this.relic2ID = TQData.ReadCString(reader);
					atlantis = true;
				}

				if (atlantis)
				{
					TQData.ValidateNextString("relicBonus2", reader);
					this.RelicBonus2Id = TQData.ReadCString(reader);

					TQData.ValidateNextString("var2", reader);
					this.Var2 = reader.ReadInt32();
				}
				else
				{
					this.relic2ID = string.Empty;
					this.RelicBonus2Id = string.Empty;
					this.Var2 = var2Default;
				}

				TQData.ValidateNextString("end_block", reader);
				this.endBlockCrap2 = reader.ReadInt32();

				if (this.ContainerType == SackType.Stash)
				{
					TQData.ValidateNextString("xOffset", reader);
					this.PositionX = Convert.ToInt32(reader.ReadSingle(), CultureInfo.InvariantCulture);

					TQData.ValidateNextString("yOffset", reader);
					this.PositionY = Convert.ToInt32(reader.ReadSingle(), CultureInfo.InvariantCulture);
				}
				else if (this.ContainerType == SackType.Equipment)
				{
					// Initially set the coordinates to (0, 0)
					this.PositionX = 0;
					this.PositionY = 0;
				}
				else
				{
					TQData.ValidateNextString("pointX", reader);
					this.PositionX = reader.ReadInt32();

					TQData.ValidateNextString("pointY", reader);
					this.PositionY = reader.ReadInt32();

					TQData.ValidateNextString("end_block", reader);
					this.endBlockCrap1 = reader.ReadInt32();
				}

				this.GetDBData();

				if (this.ContainerType == SackType.Stash)
				{
					this.StackSize = this.beginBlockCrap1 + 1;
				}
				else
				{
					this.StackSize = 1;
				}
			}
			catch (ArgumentException)
			{
				// The ValidateNextString Method can throw an ArgumentException.
				// We just pass it along at this point.
				throw;
			}
		}

		/// <summary>
		/// Pulls data out of the TQ item database for this item.
		/// </summary>
		public void GetDBData()
		{
			if (TQDebug.ItemDebugLevel > 0)
			{
				TQDebug.DebugWrite(string.Format(CultureInfo.InvariantCulture, "Item.GetDBData ()"));
				TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "   baseItemID = {0}", this.BaseItemId));
			}

			this.BaseItemId = CheckExtension(this.BaseItemId);
			this.baseItemInfo = Database.DB.GetInfo(this.BaseItemId);

			this.prefixID = CheckExtension(this.prefixID);
			this.suffixID = CheckExtension(this.suffixID);

			if (TQDebug.ItemDebugLevel > 1)
			{
				TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "prefixID = {0}", this.prefixID));
				TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "suffixID = {0}", this.suffixID));
			}

			this.prefixInfo = Database.DB.GetInfo(this.prefixID);
			this.suffixInfo = Database.DB.GetInfo(this.suffixID);
			this.relicID = CheckExtension(this.relicID);
			this.RelicBonusId = CheckExtension(this.RelicBonusId);

			if (TQDebug.ItemDebugLevel > 1)
			{
				TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "relicID = {0}", this.relicID));
				TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "relicBonusID = {0}", this.RelicBonusId));
			}

			this.RelicInfo = Database.DB.GetInfo(this.relicID);
			this.RelicBonusInfo = Database.DB.GetInfo(this.RelicBonusId);

			this.Relic2Info = Database.DB.GetInfo(this.relic2ID);
			this.RelicBonus2Info = Database.DB.GetInfo(this.RelicBonus2Id);

			if (TQDebug.ItemDebugLevel > 1)
			{
				TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "'{0}' baseItemInfo is {1} null", this.ToString(), (this.baseItemInfo == null) ? string.Empty : "NOT"));
			}

			// Get the bitmaps we need
			if (this.baseItemInfo != null)
			{
				if (this.IsRelic && !this.IsRelicComplete)
				{
					this.ItemBitmap = Database.DB.LoadBitmap(this.baseItemInfo.ShardBitmap);
					if (TQDebug.ItemDebugLevel > 1)
					{
						TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "Loaded shardbitmap ({0})", this.baseItemInfo.ShardBitmap));
					}
				}
				else
				{
					this.ItemBitmap = Database.DB.LoadBitmap(this.baseItemInfo.Bitmap);
					if (TQDebug.ItemDebugLevel > 1)
					{
						TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "Loaded regular bitmap ({0})", this.baseItemInfo.Bitmap));
					}
				}
			}
			else
			{
				// Added by VillageIdiot
				// Try showing something so unknown items are not invisible.
				this.ItemBitmap = Database.DB.LoadBitmap("DefaultBitmap");
				if (TQDebug.ItemDebugLevel > 1)
				{
					TQDebug.DebugWriteLine("Try loading (DefaultBitmap)");
				}
			}

			// Changed by VillageIdiot
			// Moved outside of BaseItemInfo conditional since there are now 2 conditions
			if (this.ItemBitmap != null)
			{
				if (TQDebug.ItemDebugLevel > 1)
				{
					TQDebug.DebugWriteLine(string.Format(
						CultureInfo.InvariantCulture,
						"size = {0}x{1} (unitsize={2})",
						this.ItemBitmap.Width,
						this.ItemBitmap.Height,
						Database.DB.ItemUnitSize));
				}

				this.Width = Convert.ToInt32((float)this.ItemBitmap.Width * Database.DB.Scale / (float)Database.DB.ItemUnitSize);
				this.Height = Convert.ToInt32((float)this.ItemBitmap.Height * Database.DB.Scale / (float)Database.DB.ItemUnitSize);
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 1)
				{
					TQDebug.DebugWriteLine("bitmap is null");
				}

				this.Width = 1;
				this.Height = 1;
			}

			if (TQDebug.ItemDebugLevel > 0)
			{
				TQDebug.DebugWriteLine("Exiting Item.GetDBData ()");
			}
		}

		#endregion Item Public Methods

		#region Item Private Methods

		#region Item Private Static Methods

		/// <summary>
		/// Initializes the random numbers
		/// </summary>
		/// <returns>Random number instance</returns>
		private static Random InitializeRandom()
		{
			return new Random();
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

		/// <summary>
		/// Holds all of the keys which we are filtering
		/// </summary>
		/// <param name="key">key which we are checking whether or not it gets filtered.</param>
		/// <returns>true if key is present in this list</returns>
		private static bool FilterKey(string key)
		{
			string keyUpper = key.ToUpperInvariant();
			string[] notWanted =
			{
				"MAXTRANSPARENCY",
				"SCALE",
				"CASTSSHADOWS",
				"MARKETADJUSTMENTPERCENT",
				"LOOTRANDOMIZERCOST",
				"LOOTRANDOMIZERJITTER",
				"ACTORHEIGHT",
				"ACTORRADIUS",
				"SHADOWBIAS",
				"ITEMLEVEL",
				"ITEMCOST",
				"COMPLETEDRELICLEVEL",
				"CHARACTERBASEATTACKSPEED",
				"HIDESUFFIXNAME",
				"HIDEPREFIXNAME",
				"AMULET",
				"RING",
				"HELMET",
				"GREAVES",
				"ARMBAND",
				"BODYARMOR",
				"BOW",
				"SPEAR",
				"STAFF",
				"MACE",
				"SWORD",
				"RANGEDONEHAND",
				"AXE",
				"SHIELD",
				"BRACELET",
				"AMULET",
				"RING",
				"BLOCKABSORPTION",
				"ITEMCOSTSCALEPERCENT", // Added by VillageIdiot
				"ITEMSKILLLEVEL", // Added by VillageIdiot
				"USEDELAYTIME", // Added by VillageIdiot
				"CAMERASHAKEAMPLITUDE", // Added by VillageIdiot
				"SKILLMAXLEVEL", // Added by VillageIdiot
				"SKILLCOOLDOWNTIME", // Added by VillageIdiot
				"EXPANSIONTIME", // Added by VillageIdiot
				"SKILLTIER", // Added by VillageIdiot
				"CAMERASHAKEDURATIONSECS", // Added by VillageIdiot
				"SKILLULTIMATELEVEL", // Added by VillageIdiot
				"SKILLCONNECTIONSPACING", // Added by VillageIdiot
				"PETBURSTSPAWN", // Added by VillageIdiot
				"PETLIMIT", // Added by VillageIdiot
				"ISPETDISPLAYABLE", // Added by VillageIdiot
				"SPAWNOBJECTSTIMETOLIVE", // Added by VillageIdiot
				"SKILLPROJECTILENUMBER", // Added by VillageIdiot
				"SKILLMASTERYLEVELREQUIRED", // Added by VillageIdiot
				"EXCLUDERACIALDAMAGE", // Added by VillageIdiot
				"SKILLWEAPONTINTRED", // Added by VillageIdiot
				"SKILLWEAPONTINTGREEN", // Added by VillageIdiot
				"SKILLWEAPONTINTBLUE", // Added by VillageIdiot
				"DEBUFSKILL", // Added by VillageIdiot
				"HIDEFROMUI", // Added by VillageIdiot
				"INSTANTCAST", // Added by VillageIdiot
				"WAVEENDWIDTH", // Added by VillageIdiot
				"WAVEDISTANCE",  // Added by VillageIdiot
				"WAVEDEPTH", // Added by VillageIdiot
				"WAVESTARTWIDTH", // Added by VillageIdiot
				"RAGDOLLAMPLIFICATION", // Added by VillageIdiot
				"WAVETIME", // Added by VillageIdiot
				"SPARKGAP", // Added by VillageIdiot
				"SPARKCHANCE", // Added by VillageIdiot
				"PROJECTILEUSESALLDAMAGE", // Added by VillageIdiot
				"DROPOFFSET", // Added by VillageIdiot
				"DROPHEIGHT", // Added by VillageIdiot
				"NUMPROJECTILES", // Added by VillageIdiot
				"SWORD", // Added by VillageIdiot
				"AXE", // Added by VillageIdiot
				"SPEAR", // Added by VillageIdiot
				"MACE", // Added by VillageIdiot
				"QUEST", // Added by VillageIdiot
				"CANNOTPICKUPMULTIPLE" // Added by VillageIdiot
			};

			if (Array.IndexOf(notWanted, keyUpper) != -1)
			{
				return true;
			}

			if (keyUpper.EndsWith("SOUND", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			if (keyUpper.EndsWith("MESH", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			if (keyUpper.StartsWith("BODYMASK", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Holds all of the requirements which we are filtering
		/// </summary>
		/// <param name="key">key which we are checking whether or not it gets filtered.</param>
		/// <returns>true if key is present in this list</returns>
		private static bool FilterRequirements(string key)
		{
			string[] notWanted =
			{
				"LEVELREQUIREMENT",
				"INTELLIGENCEREQUIREMENT",
				"DEXTERITYREQUIREMENT",
				"STRENGTHREQUIREMENT",
			};

			if (Array.IndexOf(notWanted, key.ToUpperInvariant()) != -1)
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Gets s string containing the prefix of the item class for use in the requirements equation.
		/// </summary>
		/// <param name="itemClass">string containing the item class</param>
		/// <returns>string containing the prefix of the item class for use in the requirements equation</returns>
		private static string GetRequirementEquationPrefix(string itemClass)
		{
			switch (itemClass.ToUpperInvariant())
			{
				case "ARMORPROTECTIVE_HEAD":
					return "head";

				case "ARMORPROTECTIVE_FOREARM":
					return "forearm";

				case "ARMORPROTECTIVE_LOWERBODY":
					return "lowerBody";

				case "ARMORPROTECTIVE_UPPERBODY":
					return "upperBody";

				case "ARMORJEWELRY_BRACELET":
					return "bracelet";

				case "ARMORJEWELRY_RING":
					return "ring";

				case "ARMORJEWELRY_AMULET":
					return "amulet";

				case "WEAPONHUNTING_BOW":
					return "bow";

				case "WEAPONHUNTING_SPEAR":
					return "spear";

				case "WEAPONHUNTING_RANGEDONEHAND":
					return "bow";

				case "WEAPONMELEE_AXE":
					return "axe";

				case "WEAPONMELEE_SWORD":
					return "sword";

				case "WEAPONMELEE_MACE":
					return "mace";

				case "WEAPONMAGICAL_STAFF":
					return "staff";

				case "WEAPONARMOR_SHIELD":
					return "shield";

				default:
					return "none";
			}
		}

		/// <summary>
		/// Gets whether or not the variable contains a value which we are filtering.
		/// </summary>
		/// <param name="variable">Variable which we are checking.</param>
		/// <param name="allowStrings">Flag indicating whether or not we are allowing strings to show up</param>
		/// <returns>true if the variable contains a value which is filtered.</returns>
		private static bool FilterValue(Variable variable, bool allowStrings)
		{
			for (int i = 0; i < variable.NumberOfValues; i++)
			{
				switch (variable.DataType)
				{
					case VariableDataType.Integer:
						if (variable.GetInt32(i) != 0)
						{
							return false;
						}

						break;

					case VariableDataType.Float:
						if (variable.GetSingle(i) != 0.0)
						{
							return false;
						}

						break;

					case VariableDataType.StringVar:
						if ((allowStrings || variable.Name.ToUpperInvariant().Equals("CHARACTERBASEATTACKSPEEDTAG") ||
							variable.Name.ToUpperInvariant().Equals("ITEMSKILLNAME") || // Added by VillageIdiot for Granted skills
							variable.Name.ToUpperInvariant().Equals("SKILLNAME") || // Added by VillageIdiot for scroll skills
							variable.Name.ToUpperInvariant().Equals("PETBONUSNAME") || // Added by VillageIdiot for pet bonuses
							ItemAttributes.IsReagent(variable.Name)) &&
							(variable.GetString(i).Length > 0))
						{
							return false;
						}

						break;

					case VariableDataType.Boolean:
						if (variable.GetInt32(i) != 0)
						{
							return false;
						}

						break;
				}
			}

			return true;
		}

		/// <summary>
		/// Checks to see if the id ends with .dbr and adds it if not.
		/// Sometimes the .dbr extension is not written into the item
		/// </summary>
		/// <param name="itemId">item id to be checked</param>
		/// <returns>string containing itemId with a .dbr extension.</returns>
		private static string CheckExtension(string itemId)
		{
			if (itemId == null)
			{
				return null;
			}

			if (itemId.Length < 4)
			{
				return itemId;
			}

			if (Path.GetExtension(itemId).ToUpperInvariant().Equals(".DBR"))
			{
				return itemId;
			}
			else
			{
				return string.Concat(itemId, ".dbr");
			}
		}

		/// <summary>
		/// For displaying raw attribute data
		/// </summary>
		/// <param name="attributeList">ArrayList containing the arributes</param>
		/// <param name="results">List containing the attribute strings.</param>
		private static void ConvertBareAttributeListToString(List<Variable> attributeList, List<string> results)
		{
			foreach (Variable variable in attributeList)
			{
				if (variable != null)
				{
					results.Add(variable.ToString());
				}
			}
		}

		/// <summary>
		/// Formats a string based on the formatspec
		/// </summary>
		/// <param name="formatSpec">format specification</param>
		/// <param name="parameter1">first parameter</param>
		/// <param name="parameter2">second parameter</param>
		/// <param name="parameter3">third parameter</param>
		/// <returns>formatted string.</returns>
		private static string Format(string formatSpec, object parameter1, object parameter2 = null, object parameter3 = null)
		{
			try
			{
				return string.Format(CultureInfo.CurrentCulture, formatSpec, parameter1, parameter2, parameter3);
			}
			catch (ArgumentException)
			{
				string parameters = string.Format(
					CultureInfo.InvariantCulture,
					"\", '{0}', '{1}', '{2}'>",
					parameter1 == null ? "NULL" : parameter1,
					parameter2 == null ? "NULL" : parameter2,
					parameter3 == null ? "NULL" : parameter3);

				string error = string.Concat("FormatErr(\"", formatSpec, parameters);

				return error;
			}
		}

		/// <summary>
		/// Gets the item requirements from the database record
		/// </summary>
		/// <param name="requirements">SortedList of requirements</param>
		/// <param name="record">database record</param>
		private static void GetRequirementsFromRecord(SortedList<string, Variable> requirements, DBRecordCollection record)
		{
			if (TQDebug.ItemDebugLevel > 0)
			{
				TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "Item.GetDynamicRequirementsFromRecord({0}, {1})", requirements, record));
			}

			if (record == null)
			{
				if (TQDebug.ItemDebugLevel > 0)
				{
					TQDebug.DebugWriteLine("Error - record was null.");
				}

				return;
			}

			if (TQDebug.ItemDebugLevel > 1)
			{
				TQDebug.DebugWriteLine(record.Id);
			}

			foreach (Variable variable in record)
			{
				if (TQDebug.ItemDebugLevel > 2)
				{
					TQDebug.DebugWriteLine(variable.Name);
				}

				if (FilterValue(variable, false))
				{
					continue;
				}

				if (!FilterRequirements(variable.Name))
				{
					continue;
				}

				string value = variable.ToStringValue();
				string key = variable.Name.Replace("Requirement", string.Empty);

				// Upper-case the first char of key
				key = string.Concat(key.Substring(0, 1).ToUpper(System.Globalization.CultureInfo.InvariantCulture), key.Substring(1));

				// Level needs to be LevelRequirement bah
				if (key.Equals("Level"))
				{
					key = "LevelRequirement";
				}

				if (requirements.ContainsKey(key))
				{
					Variable oldVariable = (Variable)requirements[key];

					// Changed by VillageIdiot
					// Comparison was failing when level difference was too high
					// (single digit vs multi-digit)
					if (value.Contains(",") || oldVariable.Name.Contains(","))
					{
						// Just in case there is something with multiple values
						// Keep the original code
						if (string.Compare(value, oldVariable.ToStringValue(), StringComparison.OrdinalIgnoreCase) <= 0)
						{
							continue;
						}

						requirements.Remove(key);
					}
					else
					{
						if (variable.GetInt32(0) <= oldVariable.GetInt32(0))
						{
							continue;
						}

						requirements.Remove(key);
					}
				}

				if (TQDebug.ItemDebugLevel > 1)
				{
					TQDebug.DebugWriteLine(Item.Format("Added Requirement {0}={1}", key, value));
				}

				requirements.Add(key, variable);
			}

			if (TQDebug.ItemDebugLevel > 0)
			{
				TQDebug.DebugWriteLine("Exiting Item.GetDynamicRequirementsFromRecord()");
			}
		}

		/// <summary>
		/// Get + to a Mastery string
		/// </summary>
		/// <param name="record">DBRecord database record</param>
		/// <param name="variable">variable structure</param>
		/// <param name="attributeData">ItemAttributesData structure</param>
		/// <param name="font">display font string</param>
		/// <returns>formatted string with the + to mastery</returns>
		private static string GetAugmentMasteryLevel(DBRecordCollection record, Variable variable, ItemAttributesData attributeData, ref string font)
		{
			string augmentNumber = attributeData.FullAttribute.Substring(19, 1);
			string skillRecordKey = string.Concat("augmentMasteryName", augmentNumber);
			string skillRecordID = record.GetString(skillRecordKey, 0);
			if (string.IsNullOrEmpty(skillRecordID))
			{
				skillRecordID = skillRecordKey;
			}

			string skillName = null;
			DBRecordCollection skillRecord = Database.DB.GetRecordFromFile(skillRecordID);
			if (skillRecord != null)
			{
				string nameTag = skillRecord.GetString("skillDisplayName", 0);
				if (!string.IsNullOrEmpty(nameTag))
				{
					skillName = Database.DB.GetFriendlyName(nameTag);
				}
			}

			if (string.IsNullOrEmpty(skillName))
			{
				skillName = Path.GetFileNameWithoutExtension(skillRecordID);
				font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Legendary)));
			}

			// now get the formatSpec
			string formatSpec = Database.DB.GetFriendlyName("ItemMasteryIncrement");
			if (string.IsNullOrEmpty(formatSpec))
			{
				formatSpec = "?+{0} to skills in {1}?";
				if (font == null)
				{
					font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Legendary)));
				}
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 2)
				{
					TQDebug.DebugWriteLine("Item.formatspec (augment mastery) = " + formatSpec);
				}

				formatSpec = ItemAttributes.ConvertFormat(formatSpec);
				if (string.IsNullOrEmpty(font))
				{
					font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Epic)));
				}
			}

			return Item.Format(formatSpec, variable[0], skillName);
		}

		/// <summary>
		/// Gets the + to all skills string
		/// </summary>
		/// <param name="variableNumber">offset number of the variable value that we are using</param>
		/// <param name="variable">variable structure</param>
		/// <param name="font">display font string</param>
		/// <returns>formatted string for + to all skills</returns>
		private static string GetAugmentAllLevel(int variableNumber, Variable variable, ref string font)
		{
			string tag = "ItemAllSkillIncrement";
			string formatSpec = Database.DB.GetFriendlyName(tag);
			if (string.IsNullOrEmpty(formatSpec))
			{
				formatSpec = "?+{0} to all skills?";
				font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Legendary)));
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 2)
				{
					TQDebug.DebugWriteLine("Item.formatspec (augment level) = " + formatSpec);
				}

				formatSpec = ItemAttributes.ConvertFormat(formatSpec);
				font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Epic)));
			}

			return Item.Format(formatSpec, variable[System.Math.Min(variable.NumberOfValues - 1, variableNumber)]);
		}

		/// <summary>
		/// Gets the formatted racial bonus string(s)
		/// </summary>
		/// <param name="record">DBRecord of the databse record</param>
		/// <param name="results">List containing the results</param>
		/// <param name="varNum">offset number of the variable value that we are using</param>
		/// <param name="isGlobal">Flag to signal global parameters</param>
		/// <param name="globalIndent">global indent string</param>
		/// <param name="v">variable structure</param>
		/// <param name="d">ItemAttributesData structure</param>
		/// <param name="line">line string</param>
		/// <param name="font">display font string</param>
		/// <returns>formatted string of racial bonus(es)  adds to the results if there are multiple.</returns>
		private static string GetRacialBonus(DBRecordCollection record, List<string> results, int varNum, bool isGlobal, string globalIndent, Variable v, ItemAttributesData d, string line, ref string font)
		{
			// Added by VillageIdiot
			// Updated to accept multiple racial bonuses in record
			string[] races = record.GetAllStrings("racialBonusRace");
			if (races != null)
			{
				for (int j = 0; j < races.Length; ++j)
				{
					string finalRace = Database.DB.GetFriendlyName(races[j]);
					if (finalRace == null)
					{
						// Try to look up plural
						races[j] = string.Concat(races[j], "s");
						finalRace = Database.DB.GetFriendlyName(races[j]);
					}

					// If not plural, then use original
					if (finalRace == null)
					{
						finalRace = races[j].Remove(races[j].Length - 1);
					}

					string formatTag = string.Concat(d.FullAttribute.Substring(0, 1).ToUpperInvariant(), d.FullAttribute.Substring(1));
					string formatSpec = Database.DB.GetFriendlyName(formatTag);
					if (formatSpec == null)
					{
						formatSpec = string.Concat(formatTag, " {0} {1}");
					}
					else
					{
						if (TQDebug.ItemDebugLevel > 2)
						{
							TQDebug.DebugWriteLine("Item.formatspec (race bonus) = " + formatSpec);
						}

						formatSpec = ItemAttributes.ConvertFormat(formatSpec);
					}

					if (line != null)
					{
						line = Database.MakeSafeForHtml(line);
						if (d.Variable.Length > 0)
						{
							string s = Database.MakeSafeForHtml(d.Variable);
							s = string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Legendary)), s);
							line = string.Concat(line, s);
						}

						// Add the font tags if necessary
						if (font != null)
						{
							line = string.Concat(font, line, "</font>");
						}

						if (isGlobal)
						{
							line = string.Concat(globalIndent, line);
						}

						results.Add(line);
					}

					line = Item.Format(formatSpec, v[Math.Min(v.NumberOfValues - 1, varNum)], finalRace);
					font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Epic)));
				}
			}

			return line;
		}

		/// <summary>
		/// Gets the global chance string
		/// </summary>
		/// <param name="attributeList">Arraylist containing the attributes</param>
		/// <param name="varNum">offset number of the variable value that we are using</param>
		/// <param name="v">variable structure</param>
		/// <param name="font">font string</param>
		/// <returns>formatted global chance string</returns>
		private static string GetGlobalChance(List<Variable> attributeList, int varNum, Variable v, ref string font)
		{
			string line;
			string tag = "GlobalPercentChanceOfAllTag";

			// use our hack to determine if it was XOR or not.
			if (attributeList.Count > 2)
			{
				// Spurious global chance indicator.  Do not use
				line = string.Empty;
			}
			else
			{
				if (attributeList.Count > 1)
				{
					tag = "GlobalPercentChanceOfOneTag";
				}

				string formatSpec = Database.DB.GetFriendlyName(tag);
				if (formatSpec == null)
				{
					formatSpec = string.Format(CultureInfo.CurrentCulture, "{0:f1}% ?{0}?", tag);
					font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Legendary)));
				}
				else
				{
					if (TQDebug.ItemDebugLevel > 2)
					{
						TQDebug.DebugWriteLine("Item.formatspec (chance of one) = " + formatSpec);
					}

					formatSpec = ItemAttributes.ConvertFormat(formatSpec);
					font = String.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Epic)));
				}

				line = Item.Format(formatSpec, v[System.Math.Min(v.NumberOfValues - 1, varNum)]);
			}

			return line;
		}

		/// <summary>
		/// Gets a formatted chance modifier string
		/// </summary>
		/// <param name="varNum">offset number of the variable value that we are using</param>
		/// <param name="modifierChanceVar">Chance modifier variable</param>
		/// <returns>formatted chance modifier string</returns>
		private static string GetChanceModifier(int varNum, Variable modifierChanceVar)
		{
			string modifierChance = null;
			string color = null;
			string formatSpec = Database.DB.GetFriendlyName("ChanceOfTag");
			if (formatSpec == null)
			{
				formatSpec = "?{%.1f0}% Chance of?";
				color = Database.HtmlColor(Item.GetColor(ItemStyle.Legendary));
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 2)
				{
					TQDebug.DebugWriteLine("Item.formatspec (chance) = " + formatSpec);
				}

				formatSpec = ItemAttributes.ConvertFormat(formatSpec);
			}

			modifierChance = Item.Format(formatSpec, modifierChanceVar[Math.Min(modifierChanceVar.NumberOfValues - 1, varNum)]);
			modifierChance = Database.MakeSafeForHtml(modifierChance);
			if (color != null)
			{
				modifierChance = Item.Format("<font color={0}>{1}</font>", color, modifierChance);
			}

			return modifierChance;
		}

		/// <summary>
		/// Gets formatted duration modifier string
		/// </summary>
		/// <param name="varNum">offset number of the variable value that we are using</param>
		/// <param name="durationModifierVar">duration modifier variable</param>
		/// <returns>formatted duration modifier string</returns>
		private static string GetDurationModifier(int varNum, Variable durationModifierVar)
		{
			string durationModifier = null;
			string color = null;
			string formatSpec = Database.DB.GetFriendlyName("ImprovedTimeFormat");
			if (string.IsNullOrEmpty(formatSpec))
			{
				formatSpec = "?with {0:f0}% Improved Duration?";
				color = Database.HtmlColor(Item.GetColor(ItemStyle.Legendary));
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 2)
				{
					TQDebug.DebugWriteLine("Item.formatspec (improved time) = " + formatSpec);
				}

				formatSpec = ItemAttributes.ConvertFormat(formatSpec);
			}

			durationModifier = Item.Format(formatSpec, durationModifierVar[Math.Min(durationModifierVar.NumberOfValues - 1, varNum)]);
			durationModifier = Database.MakeSafeForHtml(durationModifier);
			if (color != null)
			{
				durationModifier = Item.Format("<font color={0}>{1}</font>", color, durationModifier);
			}

			return durationModifier;
		}

		/// <summary>
		/// Gets formatted modifier string
		/// </summary>
		/// <param name="data">ItemAttributesData for the attribute</param>
		/// <param name="varNum">offset number of the variable value that we are using</param>
		/// <param name="modifierData">ItemAttributesData for the modifier</param>
		/// <param name="modifierVar">modifier variable</param>
		/// <returns>formatted modifier string</returns>
		private static string GetModifier(ItemAttributesData data, int varNum, ItemAttributesData modifierData, Variable modifierVar)
		{
			string modifier = null;
			string color = null;
			string formatSpec = null;
			string tag = ItemAttributes.GetAttributeTextTag(data);
			if (string.IsNullOrEmpty(tag))
			{
				formatSpec = string.Concat("{0:f1}% ?", modifierData.FullAttribute, "?");
				color = Database.HtmlColor(Item.GetColor(ItemStyle.Legendary));
			}
			else
			{
				formatSpec = Database.DB.GetFriendlyName(tag);
				if (string.IsNullOrEmpty(formatSpec))
				{
					formatSpec = string.Concat("{0:f1}% ?", modifierData.FullAttribute, "?");
					color = Database.HtmlColor(Item.GetColor(ItemStyle.Legendary));
				}
				else
				{
					if (TQDebug.ItemDebugLevel > 2)
					{
						TQDebug.DebugWriteLine("Item.formatspec (percent) = " + formatSpec);
					}

					formatSpec = ItemAttributes.ConvertFormat(formatSpec);
				}
			}

			modifier = Item.Format(formatSpec, modifierVar[Math.Min(modifierVar.NumberOfValues - 1, varNum)]);
			modifier = Database.MakeSafeForHtml(modifier);
			if (!string.IsNullOrEmpty(color))
			{
				modifier = Item.Format("<font color={0}>{1}</font>", color, modifier);
			}

			return modifier;
		}

		/// <summary>
		/// Gets formatted chance string
		/// </summary>
		/// <param name="varNum">offset number of the variable value that we are using</param>
		/// <param name="chanceVar">chance variable</param>
		/// <returns>formatted chance string.</returns>
		private static string GetChance(int varNum, Variable chanceVar)
		{
			string chance = null;
			string color = null;
			string formatSpec = Database.DB.GetFriendlyName("ChanceOfTag");
			if (string.IsNullOrEmpty(formatSpec))
			{
				formatSpec = "?{%.1f0}% Chance of?";
				color = Database.HtmlColor(Item.GetColor(ItemStyle.Legendary));
			}

			if (TQDebug.ItemDebugLevel > 2)
			{
				TQDebug.DebugWriteLine("Item.formatspec (chance) = " + formatSpec);
			}

			formatSpec = ItemAttributes.ConvertFormat(formatSpec);
			if (chanceVar != null)
			{
				chance = Item.Format(formatSpec, chanceVar[Math.Min(chanceVar.NumberOfValues - 1, varNum)]);
				chance = Database.MakeSafeForHtml(chance);
				if (!string.IsNullOrEmpty(color))
				{
					chance = Item.Format("<font color={0}>{1}</font>", color, chance);
				}
			}

			return chance;
		}

		/// <summary>
		/// Gets the formatted damage ratio string
		/// </summary>
		/// <param name="varNum">offset number of the variable value that we are using</param>
		/// <param name="damageRatioData">ItemAttributesData for the damage ratio</param>
		/// <param name="damageRatioVar">Damage Ratio variable</param>
		/// <returns>formatted damage ratio string</returns>
		private static string GetDamageRatio(int varNum, ItemAttributesData damageRatioData, Variable damageRatioVar)
		{
			string damageRatio = null;
			string color = null;
			string formatSpec = null;

			string tag = string.Concat("Damage", damageRatioData.FullAttribute.Substring(9, damageRatioData.FullAttribute.Length - 20), "Ratio");
			formatSpec = Database.DB.GetFriendlyName(tag);

			if (string.IsNullOrEmpty(formatSpec))
			{
				formatSpec = string.Concat("{0:f1}% ?", damageRatioData.FullAttribute, "?");
				color = Database.HtmlColor(Item.GetColor(ItemStyle.Legendary));
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 2)
				{
					TQDebug.DebugWriteLine("Item.formatspec (percent) = " + formatSpec);
				}

				formatSpec = ItemAttributes.ConvertFormat(formatSpec);
			}

			damageRatio = Item.Format(formatSpec, damageRatioVar[Math.Min(damageRatioVar.NumberOfValues - 1, varNum)]);
			damageRatio = Database.MakeSafeForHtml(damageRatio);
			if (!string.IsNullOrEmpty(color))
			{
				damageRatio = Item.Format("<font color={0}>{1}</font>", color, damageRatio);
			}

			return damageRatio;
		}

		/// <summary>
		/// Gets the formatted duration single value
		/// </summary>
		/// <param name="varNum">offset number of the variable value that we are using</param>
		/// <param name="minDurVar">minimum duration variable</param>
		/// <param name="maxDurVar">maximum duration variable</param>
		/// <returns>formatted duration string</returns>
		private static string GetDurationSingle(int varNum, Variable minDurVar, Variable maxDurVar)
		{
			string duration = null;
			string color = null;
			string formatSpec = Database.DB.GetFriendlyName("DamageSingleFormatTime");
			if (string.IsNullOrEmpty(formatSpec))
			{
				formatSpec = "{0}";
				color = Database.HtmlColor(Item.GetColor(ItemStyle.Legendary));
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 2)
				{
					TQDebug.DebugWriteLine("Item.formatspec (time single) = " + formatSpec);
				}

				formatSpec = ItemAttributes.ConvertFormat(formatSpec);
			}

			Variable durationVariable = minDurVar;
			if (durationVariable == null)
			{
				durationVariable = maxDurVar;
			}

			if (durationVariable != null)
			{
				duration = Item.Format(formatSpec, durationVariable[Math.Min(durationVariable.NumberOfValues - 1, varNum)]);
				duration = Database.MakeSafeForHtml(duration);
				if (!string.IsNullOrEmpty(color))
				{
					duration = Item.Format("<font color={0}>{1}</font>", color, duration);
				}
			}

			return duration;
		}

		/// <summary>
		/// Gets the formatted duration range values
		/// </summary>
		/// <param name="varNum">offset number of the variable value that we are using</param>
		/// <param name="minDurVar">minimum duration variable</param>
		/// <param name="maxDurVar">maximum duration variable</param>
		/// <returns>formatted duration string</returns>
		private static string GetDurationRange(int varNum, Variable minDurVar, Variable maxDurVar)
		{
			string duration = null;
			string color = null;
			string formatSpec = Database.DB.GetFriendlyName("DamageRangeFormatTime");
			if (string.IsNullOrEmpty(formatSpec))
			{
				formatSpec = "for {0}..{1} seconds";
				color = Database.HtmlColor(Item.GetColor(ItemStyle.Legendary));
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 2)
				{
					TQDebug.DebugWriteLine("Item.formatspec (time range) = " + formatSpec);
				}

				formatSpec = ItemAttributes.ConvertFormat(formatSpec);
			}

			duration = Item.Format(formatSpec, minDurVar[Math.Min(minDurVar.NumberOfValues - 1, varNum)], maxDurVar[Math.Min(maxDurVar.NumberOfValues - 1, varNum)]);
			duration = Database.MakeSafeForHtml(duration);
			if (!string.IsNullOrEmpty(color))
			{
				duration = Item.Format("<font color={0}>{1}</font>", color, duration);
			}

			return duration;
		}

		/// <summary>
		/// Gets a formatted + to skill string
		/// </summary>
		/// <param name="record">DBRecord database record</param>
		/// <param name="variable">variable structure</param>
		/// <param name="attributeData">ItemAttributesData structure</param>
		/// <param name="line">line of text</param>
		/// <param name="font">display font string</param>
		/// <returns>formatted string containing + to skill</returns>
		private static string GetAugmentSkillLevel(DBRecordCollection record, Variable variable, ItemAttributesData attributeData, string line, ref string font)
		{
			string augmentSkillNumber = attributeData.FullAttribute.Substring(17, 1);
			string skillRecordKey = string.Concat("augmentSkillName", augmentSkillNumber);
			string skillRecordID = record.GetString(skillRecordKey, 0);
			if (!string.IsNullOrEmpty(skillRecordID))
			{
				string skillName = null;
				string nameTag = null;
				DBRecordCollection skillRecord = Database.DB.GetRecordFromFile(skillRecordID);
				if (skillRecord != null)
				{
					// Changed by VillageIdiot
					// for augmenting buff skills
					string buffSkillName = skillRecord.GetString("buffSkillName", 0);
					if (string.IsNullOrEmpty(buffSkillName))
					{
						// Not a buff so look up the name
						nameTag = skillRecord.GetString("skillDisplayName", 0);
						if (!string.IsNullOrEmpty(nameTag))
						{
							skillName = Database.DB.GetFriendlyName(nameTag);
						}
						else
						{
							// Added by VillageIdiot
							// Check to see if this is a pet skill
							nameTag = skillRecord.GetString("Class", 0);
							if (nameTag.Contains("PetModifier"))
							{
								string petSkillID = skillRecord.GetString("petSkillName", 0);
								DBRecordCollection petSkillRecord = Database.DB.GetRecordFromFile(petSkillID);
								if (petSkillRecord != null)
								{
									string petNameTag = petSkillRecord.GetString("skillDisplayName", 0);
									if (!string.IsNullOrEmpty(petNameTag))
									{
										skillName = Database.DB.GetFriendlyName(petNameTag);
									}
								}
							}
						}
					}
					else
					{
						// This is a buff skill
						DBRecordCollection buffSkillRecord = Database.DB.GetRecordFromFile(buffSkillName);
						if (buffSkillRecord != null)
						{
							nameTag = buffSkillRecord.GetString("skillDisplayName", 0);
							if (!string.IsNullOrEmpty(nameTag))
							{
								skillName = Database.DB.GetFriendlyName(nameTag);
							}
						}
					}
				}

				if (string.IsNullOrEmpty(skillName))
				{
					skillName = Path.GetFileNameWithoutExtension(skillRecordID);
				}

				// now get the formatSpec
				string formatSpec = Database.DB.GetFriendlyName("ItemSkillIncrement");
				if (string.IsNullOrEmpty(formatSpec))
				{
					formatSpec = "?+{0} to skill {1}?";
					font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Legendary)));
				}
				else
				{
					if (TQDebug.ItemDebugLevel > 2)
					{
						TQDebug.DebugWriteLine("Item.formatspec (item skill) = " + formatSpec);
					}

					formatSpec = ItemAttributes.ConvertFormat(formatSpec);
					font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Epic)));
				}

				line = Item.Format(formatSpec, variable[0], skillName);
			}

			return line;
		}

		/// <summary>
		/// Gets the formatted formulae string(s)
		/// </summary>
		/// <param name="results">results list</param>
		/// <param name="variable">variable structure</param>
		/// <param name="attributeData">ItemAttributesData structure</param>
		/// <param name="line">line of text</param>
		/// <param name="font">display font string</param>
		/// <returns>formatted formulae string</returns>
		private static string GetFormulae(List<string> results, Variable variable, ItemAttributesData attributeData, string line, ref string font)
		{
			// Special case for formulae reagents
			if (attributeData.FullAttribute.StartsWith("reagent", StringComparison.OrdinalIgnoreCase))
			{
				DBRecordCollection reagentRecord = Database.DB.GetRecordFromFile(variable.GetString(0));
				if (reagentRecord != null)
				{
					string nameTag = reagentRecord.GetString("description", 0);
					if (!string.IsNullOrEmpty(nameTag))
					{
						string reagentName = Database.DB.GetFriendlyName(nameTag);
						string formatSpec = "{0}";
						font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Common)));
						line = Item.Format(formatSpec, reagentName);
					}
				}
			}
			else if (attributeData.FullAttribute.Equals("artifactCreationCost"))
			{
				string formatSpec = Database.DB.GetFriendlyName("xtagArtifactCost");
				if (TQDebug.ItemDebugLevel > 2)
				{
					TQDebug.DebugWriteLine("Item.formatspec (Artifact cost) = " + formatSpec);
				}

				formatSpec = ItemAttributes.ConvertFormat(formatSpec);
				font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Rare)));
				results.Add(string.Empty);
				line = Item.Format(formatSpec, string.Format(CultureInfo.CurrentCulture, "{0:N0}", variable[0]));
			}

			return line;
		}

		/// <summary>
		/// Gets a formatted string of the granted skill
		/// </summary>
		/// <param name="record">DBRecord database record</param>
		/// <param name="results">results list</param>
		/// <param name="variable">variable structure</param>
		/// <param name="line">line of text</param>
		/// <param name="font">display font string</param>
		/// <returns>formatted granted skill string.</returns>
		private static string GetGrantedSkill(DBRecordCollection record, List<string> results, Variable variable, string line, ref string font)
		{
			// Added by VillageIdiot
			// Special case for granted skills
			DBRecordCollection skillRecord = Database.DB.GetRecordFromFile(variable.GetString(0));
			if (skillRecord != null)
			{
				// Add a blank line and then the Grants Skill text
				results.Add(string.Empty);
				font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Mundane)));
				string skillTag = Database.DB.GetFriendlyName("tagItemGrantSkill");
				if (string.IsNullOrEmpty(skillTag))
				{
					skillTag = "Grants Skill :";
				}

				results.Add(string.Concat(font, skillTag, "</font>"));

				string skillName = null;
				string nameTag = null;

				// Changed by VillageIdiot
				// Let's actually test if there is a buff skill
				string buffSkillName = skillRecord.GetString("buffSkillName", 0);
				if (string.IsNullOrEmpty(buffSkillName))
				{
					nameTag = skillRecord.GetString("skillDisplayName", 0);
					if (!string.IsNullOrEmpty(nameTag))
					{
						skillName = Database.DB.GetFriendlyName(nameTag);

						if (string.IsNullOrEmpty(skillName))
						{
							skillName = Path.GetFileNameWithoutExtension(variable.GetString(0));
						}
					}
				}
				else
				{
					// This is a buff skill
					DBRecordCollection buffSkillRecord = Database.DB.GetRecordFromFile(buffSkillName);
					if (buffSkillRecord != null)
					{
						nameTag = buffSkillRecord.GetString("skillDisplayName", 0);
						if (!string.IsNullOrEmpty(nameTag))
						{
							skillName = Database.DB.GetFriendlyName(nameTag);

							if (string.IsNullOrEmpty(skillName))
							{
								skillName = Path.GetFileNameWithoutExtension(variable.GetString(0));
							}
						}
					}
				}

				// Added by VillageIdiot to support skill activation text
				string triggerType = null;
				string activationTag = null;
				string activationText = null;
				string autoController = record.GetString("itemSkillAutoController", 0);
				if (!string.IsNullOrEmpty(autoController))
				{
					DBRecordCollection autoControllerRecord = Database.DB.GetRecordFromFile(autoController);
					if (autoControllerRecord != null)
					{
						triggerType = autoControllerRecord.GetString("triggerType", 0);
					}
				}

				// Convert TriggerType into text tag
				if (!string.IsNullOrEmpty(triggerType))
				{
					switch (triggerType.ToUpperInvariant())
					{
						case "LOWHEALTH":
							{
								// Activated on low health
								activationTag = "xtagAutoSkillCondition01";
								break;
							}

						case "LOWMANA":
							{
								// Activated on low energy
								activationTag = "xtagAutoSkillCondition02";
								break;
							}

						case "HITBYENEMY":
							{
								// Activated upon taking damage
								activationTag = "xtagAutoSkillCondition03";
								break;
							}

						case "HITBYMELEE":
							{
								// Activated upon taking melee damage
								activationTag = "xtagAutoSkillCondition04";
								break;
							}

						case "HITBYPROJECTILE":
							{
								// Activated upon taking ranged damage
								activationTag = "xtagAutoSkillCondition05";
								break;
							}

						case "CASTBUFF":
							{
								// Activated upon casting a buff
								activationTag = "xtagAutoSkillCondition06";
								break;
							}

						case "ATTACKENEMY":
							{
								// Activated on attack
								activationTag = "xtagAutoSkillCondition07";
								break;
							}

						case "ONEQUIP":
							{
								// Activated when equipped
								activationTag = "xtagAutoSkillCondition08";
								break;
							}

						default:
							{
								activationTag = string.Empty;
								break;
							}
					}
				}

				if (!string.IsNullOrEmpty(activationTag))
				{
					activationText = Database.DB.GetFriendlyName(activationTag);
				}
				else
				{
					activationText = string.Empty;
				}

				if (string.IsNullOrEmpty(activationText))
				{
					font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Epic)));
				}
				else
				{
					font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Mundane)));
				}

				line = Item.Format("{0} {1}", skillName, activationText);
			}

			return line;
		}

		/// <summary>
		/// Gets the pet bonus string
		/// </summary>
		/// <param name="font">display font string</param>
		/// <returns>formatted pet bonus name</returns>
		private static string GetPetBonusName(ref string font)
		{
			string tag = "xtagPetBonusNameAllPets";
			string formatSpec = Database.DB.GetFriendlyName(tag);
			if (string.IsNullOrEmpty(formatSpec))
			{
				formatSpec = "?Bonus to All Pets:?";
				font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Legendary)));
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 2)
				{
					TQDebug.DebugWriteLine("Item.formatspec (pet bonus) = " + formatSpec);
				}

				formatSpec = ItemAttributes.ConvertFormat(formatSpec);
				font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Relic)));
			}

			return formatSpec;
		}

		/// <summary>
		/// Gets the skill effects string
		/// </summary>
		/// <param name="baseAttributeData">ItemAttributesData structure of the base attribute</param>
		/// <param name="variableNumber">offset number of the variable value that we are using</param>
		/// <param name="variable">variable structure</param>
		/// <param name="currentAttributeData">ItemAttributesData structure of the current attribute</param>
		/// <param name="line">line of text</param>
		/// <param name="font">display font string</param>
		/// <returns>formatted skill effect string</returns>
		private static string GetSkillEffect(ItemAttributesData baseAttributeData, int variableNumber, Variable variable, ItemAttributesData currentAttributeData, string line, ref string font)
		{
			string labelTag = ItemAttributes.GetAttributeTextTag(baseAttributeData);
			if (string.IsNullOrEmpty(labelTag))
			{
				labelTag = string.Concat("?", baseAttributeData.FullAttribute, "?");
				font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Legendary)));
			}

			string label = Database.DB.GetFriendlyName(labelTag);
			if (string.IsNullOrEmpty(label))
			{
				label = string.Concat("?", labelTag, "?");
				font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Legendary)));
			}

			if (TQDebug.ItemDebugLevel > 2)
			{
				TQDebug.DebugWriteLine("Item.label (scroll) = " + label);
			}

			label = ItemAttributes.ConvertFormat(label);

			// Find the extra format tag for those that take 2 parameters.
			string formatSpecTag = null;
			string formatSpec = null;
			if (currentAttributeData.FullAttribute.EndsWith("Cost", StringComparison.OrdinalIgnoreCase))
			{
				formatSpecTag = "SkillIntFormat";
			}
			else if (currentAttributeData.FullAttribute.EndsWith("Duration", StringComparison.OrdinalIgnoreCase))
			{
				formatSpecTag = "SkillSecondFormat";
			}
			else if (currentAttributeData.FullAttribute.EndsWith("Radius", StringComparison.OrdinalIgnoreCase))
			{
				formatSpecTag = "SkillDistanceFormat";
			}

			if (!string.IsNullOrEmpty(formatSpecTag))
			{
				formatSpec = Database.DB.GetFriendlyName(formatSpecTag);

				if (string.IsNullOrEmpty(formatSpec))
				{
					formatSpec = "?{0} {1}?";
					font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Legendary)));
				}
				else
				{
					if (TQDebug.ItemDebugLevel > 2)
					{
						TQDebug.DebugWriteLine("Item.formatspec (2 parameter) = " + formatSpec);
					}

					formatSpec = ItemAttributes.ConvertFormat(formatSpec);
					font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Epic)));
				}
			}

			if (string.IsNullOrEmpty(formatSpecTag))
			{
				font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Epic)));
				line = Item.Format(label, variable[Math.Min(variable.NumberOfValues - 1, variableNumber)]);
			}
			else
			{
				line = Item.Format(formatSpec, variable[Math.Min(variable.NumberOfValues - 1, variableNumber)], label);
			}

			return line;
		}

		/// <summary>
		/// Gets a raw attribute string
		/// </summary>
		/// <param name="attributeData">ItemAttributesData structure</param>
		/// <param name="variableNumber">offset number of the variable value that we are using</param>
		/// <param name="variable">variable structure</param>
		/// <param name="font">display font string</param>
		/// <returns>formatted raw attribute string</returns>
		private static string GetRawAttribute(ItemAttributesData attributeData, int variableNumber, Variable variable, ref string font)
		{
			string line = null;
			string labelTag = ItemAttributes.GetAttributeTextTag(attributeData);
			if (labelTag == null)
			{
				labelTag = string.Concat("?", attributeData.FullAttribute, "?");
				font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Legendary)));
			}

			string label = Database.DB.GetFriendlyName(labelTag);
			if (label == null)
			{
				label = string.Concat("?", labelTag, "?");
				font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Legendary)));
			}

			label = ItemAttributes.ConvertFormat(label);
			if (label.IndexOf('{') >= 0)
			{
				// we have a format string.  try using it.
				line = Item.Format(label, variable[Math.Min(variable.NumberOfValues - 1, variableNumber)]);
				if (font == null)
				{
					font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Epic)));
				}
			}
			else
			{
				// no format string.
				line = Database.DB.VariableToStringNice(variable);
			}

			if (font == null)
			{
				font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Legendary))); // make these unknowns stand out
			}

			return line;
		}

		#endregion Item Private Static Methods

		/// <summary>
		/// Gets the dynamic requirements from a database record.
		/// </summary>
		/// <param name="requirements">SortedList of requirements</param>
		/// <param name="itemInfo">ItemInfo for the item</param>
		private void GetDynamicRequirementsFromRecord(SortedList<string, Variable> requirements, Info itemInfo)
		{
			if (TQDebug.ItemDebugLevel > 0)
			{
				TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "Item.GetDynamicRequirementsFromRecord({0}, {1})", requirements, itemInfo));
			}

			DBRecordCollection record = Database.DB.GetRecordFromFile(itemInfo.ItemId);
			if (record == null)
			{
				return;
			}

			string itemLevelTag = "itemLevel";
			Variable lvl = record[itemLevelTag];
			if (lvl == null)
			{
				return;
			}

			string itemLevel = lvl.ToStringValue();
			string itemCostID = itemInfo.GetString("itemCostName");

			record = Database.DB.GetRecordFromFile(itemCostID);
			if (record == null)
			{
				record = Database.DB.GetRecordFromFile("records/game/itemcost.dbr");
				if (record == null)
				{
					return;
				}
			}

			if (TQDebug.ItemDebugLevel > 1)
			{
				TQDebug.DebugWriteLine(record.Id);
			}

			string prefix = GetRequirementEquationPrefix(itemInfo.ItemClass);
			foreach (Variable variable in record)
			{
				if (string.Compare(variable.Name, 0, prefix, 0, prefix.Length, StringComparison.OrdinalIgnoreCase) != 0)
				{
					continue;
				}

				if (TQDebug.ItemDebugLevel > 2)
				{
					TQDebug.DebugWriteLine(variable.Name);
				}

				if (FilterValue(variable, true))
				{
					// our equation is a string, so we want also strings
					return;
				}

				string key = variable.Name.Replace(prefix, string.Empty);
				key = key.Replace("Equation", string.Empty);
				key = key.Replace(key[0], char.ToUpperInvariant(key[0]));

				// We need to ignore the cost equations.
				// Shields have costs so they will cause an overflow.
				if (key.Equals("Cost"))
				{
					continue;
				}

				var variableKey = key.ToLowerInvariant();
				if (variableKey == "level" || variableKey == "strength" || variableKey == "dexterity" || variableKey == "intelligence")
				{
					variableKey += "Requirement";
				}

				// Level needs to be LevelRequirement bah
				if (key.Equals("Level"))
				{
					key = "LevelRequirement";
				}

				string value = variable.ToStringValue().Replace(itemLevelTag, itemLevel);

				// Added by VillageIdiot
				// Changed to reflect Total Attribut count
				value = value.Replace("totalAttCount", Convert.ToString(this.attributeCount, CultureInfo.InvariantCulture));

				Expression expression = ExpressionEvaluate.CreateExpression(value);

				// Changed by Bman to fix random overflow crashes
				Variable ans = new Variable(variableKey, VariableDataType.Integer, 1);

				// Changed by VillageIdiot to fix random overflow crashes.
				double tempVal = Math.Ceiling(Convert.ToDouble(expression.Evaluate(), CultureInfo.InvariantCulture));

				int intVal = 0;
				try
				{
					intVal = Convert.ToInt32(tempVal, CultureInfo.InvariantCulture);
				}
				catch (OverflowException)
				{
					intVal = 0;
				}

				ans[0] = intVal;

				value = ans.ToStringValue();
				if (requirements.ContainsKey(key))
				{
					if (string.Compare(value, ((Variable)requirements[key]).ToStringValue(), StringComparison.OrdinalIgnoreCase) <= 0)
					{
						return;
					}

					requirements.Remove(key);
				}

				if (TQDebug.ItemDebugLevel > 2)
				{
					TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "Added Requirement {0}={1}", key, value));
				}

				requirements.Add(key, ans);
			}

			if (TQDebug.ItemDebugLevel > 0)
			{
				TQDebug.DebugWriteLine("Exiting Item.GetDynamicRequirementsFromRecord()");
			}
		}

		/// <summary>
		/// Gets the item's attributes from the database record.
		/// </summary>
		/// <remarks>
		/// Changed by VillageIdiot
		/// Added option to NOT translate the attributes to strings
		/// </remarks>
		/// <param name="record">DBRecord for the database record</param>
		/// <param name="filtering">whether or not we are filtering strings</param>
		/// <param name="recordId">string containing the database record id</param>
		/// <param name="results">List for the results</param>
		/// <param name="convertStrings">flag on whether we convert attributes to strings.</param>
		private void GetAttributesFromRecord(DBRecordCollection record, bool filtering, string recordId, List<string> results, bool convertStrings = true)
		{
			if (TQDebug.ItemDebugLevel > 0)
			{
				TQDebug.DebugWriteLine(string.Format(
					CultureInfo.InvariantCulture,
					"Item.GetAttributesFromRecord({0}, {1}, {2}, {3}, {4})",
					record,
					filtering,
					recordId,
					results,
					convertStrings));
			}

			// First get a list of attributes, grouped by effect.
			Dictionary<string, List<Variable>> attrByEffect = new Dictionary<string, List<Variable>>();
			if (record == null)
			{
				if (TQDebug.ItemDebugLevel > 0)
				{
					TQDebug.DebugWriteLine("Error - record was null.");
				}

				results.Add("<unknown>");
				return;
			}

			if (TQDebug.ItemDebugLevel > 1)
			{
				TQDebug.DebugWriteLine(record.Id);
			}

			// Added by Village Idiot
			// To keep track of groups so they are not counted twice
			List<string> countedGroups = new List<string>();
			countedGroups.Clear();

			foreach (Variable variable in record)
			{
				if (TQDebug.ItemDebugLevel > 2)
				{
					TQDebug.DebugWriteLine(variable.Name);
				}

				if (FilterValue(variable, !filtering))
				{
					continue;
				}

				if (filtering && FilterKey(variable.Name))
				{
					continue;
				}

				if (filtering && FilterRequirements(variable.Name))
				{
					continue;
				}

				ItemAttributesData data = ItemAttributes.GetAttributeData(variable.Name);
				if (data == null)
				{
					// unknown attribute
					if (TQDebug.ItemDebugLevel > 2)
					{
						TQDebug.DebugWriteLine("Unknown Attribute");
					}

					data = new ItemAttributesData(ItemAttributesEffectType.Other, variable.Name, variable.Name, string.Empty, 0);
				}

				string effectGroup;

				// Changed by VillageIdiot to group DamageQualifiers together.
				if (data.EffectType == ItemAttributesEffectType.DamageQualifierEffect)
				{
					effectGroup = string.Concat(data.EffectType.ToString(), ":", "DamageQualifier");
				}
				else
				{
					effectGroup = string.Concat(data.EffectType.ToString(), ":", data.Effect);
				}

				// Find or create the attrList for this effect
				List<Variable> attrList;
				try
				{
					attrList = attrByEffect[effectGroup];
				}
				catch (KeyNotFoundException)
				{
					attrList = new List<Variable>();
					attrByEffect[effectGroup] = attrList;
				}

				// Add this guy to the attrList
				attrList.Add(variable);

				// Added by VillageIdiot
				// Set number of attributes parameter for level calculation
				// Filter relics and relic bonuses
				if (recordId != this.relic2ID && recordId != this.RelicBonus2Id && recordId != this.relicID && recordId != this.RelicBonusId && !this.isAttributeCounted)
				{
					// Added test to see if this has already been done
					if (!countedGroups.Contains(effectGroup))
					{
						string normalizedVariableName = variable.Name.ToUpperInvariant();
						if (!normalizedVariableName.Contains("CHANCE") && !normalizedVariableName.Contains("DURATION"))
						{
							// Filter Attribute chance and duration tags
							// Filter base attributes
							if (normalizedVariableName != "CHARACTERBASEATTACKSPEEDTAG" &&
								normalizedVariableName != "OFFENSIVEPHYSICALMIN" &&
								normalizedVariableName != "OFFENSIVEPHYSICALMAX" &&
								normalizedVariableName != "DEFENSIVEPROTECTION" &&
								normalizedVariableName != "DEFENSIVEBLOCK" &&
								normalizedVariableName != "BLOCKRECOVERYTIME" &&
								normalizedVariableName != "OFFENSIVEGLOBALCHANCE" &&
								normalizedVariableName != "RETALIATIONGLOBALCHANCE" &&
								normalizedVariableName != "OFFENSIVEPIERCERATIOMIN")
							{
								// Chance of effects are still messed up.
								if (normalizedVariableName.StartsWith("AUGMENTSKILLLEVEL", StringComparison.OrdinalIgnoreCase))
								{
									// Add value of augment skill level to count instead of incrementing
									this.attributeCount += variable.GetInt32(0);
									countedGroups.Add(effectGroup);
								}
								else
								{
									++this.attributeCount;
									countedGroups.Add(effectGroup);
								}
							}
						}
					}
				}
			}

			// Added by VillageIdiot
			// Some attributes have been counted so set the flag so we do not count them again
			if (this.attributeCount != 0)
			{
				this.isAttributeCounted = true;
			}

			// Now we have all our attributes grouped by effect.  Now lets sort them
			List<Variable>[] attrArray = new List<Variable>[attrByEffect.Count];
			attrByEffect.Values.CopyTo(attrArray, 0);
			Array.Sort(attrArray, new ItemAttributeListCompare(this.IsArmor || this.IsShield));

			// Now for the global params, we need to check to see if they are XOR or all.
			// We do this by checking the effect just after the global param.
			for (int i = 0; i < attrArray.Length; ++i)
			{
				List<Variable> attrList = attrArray[i];

				if (ItemAttributes.AttributeGroupIs(new Collection<Variable>(attrList), "offensiveGlobalChance") ||
					ItemAttributes.AttributeGroupIs(new Collection<Variable>(attrList), "retaliationGlobalChance"))
				{
					// check the next effect group
					int j = i + 1;
					if (j < attrArray.Length)
					{
						List<Variable> next = attrArray[j];
						if (!ItemAttributes.AttributeGroupHas(new Collection<Variable>(next), "Global"))
						{
							// this is a spurious globalChance entry.  Let's add 2 null entries to signal it should be ignored
							attrList.Add(null);
							attrList.Add(null);
						}
						else if (ItemAttributes.AttributeGroupHas(new Collection<Variable>(next), "XOR"))
						{
							// Yes it is global and is also XOR
							// flag our current attribute as XOR
							// We do this by adding a second NULL entry to the list.  Its a hack but it works
							attrList.Add(null);
						}
					}
					else
					{
						// this is a spurious globalChance entry.  Let's add 2 null entries to signal it should be ignored
						attrList.Add(null);
						attrList.Add(null);
					}
				}
			}

			foreach (List<Variable> attrList in attrArray)
			{
				// Used to sort out the Damage Qualifier effects.
				if (ItemAttributes.AttributeGroupIs(new Collection<Variable>(attrList), ItemAttributesEffectType.DamageQualifierEffect))
				{
					attrList.Sort(new ItemAttributeSubListCompare());
				}

				if (!convertStrings)
				{
					ConvertBareAttributeListToString(attrList, results);
				}
				else
				{
					this.ConvertAttributeListToString(record, attrList, recordId, results);
				}
			}

			if (TQDebug.ItemDebugLevel > 0)
			{
				TQDebug.DebugWriteLine("Exiting Item.GetAttributesFromRecord()");
			}
		}

		/// <summary>
		/// Converts the item's offensice attributes to a string
		/// </summary>
		/// <param name="record">DBRecord of the database record</param>
		/// <param name="attributeList">ArrayList containing the attribute list</param>
		/// <param name="data">ItemAttributesData for the item</param>
		/// <param name="recordId">string containing the record id</param>
		/// <param name="results">List containing the results</param>
		private void ConvertOffenseAttributesToString(DBRecordCollection record, List<Variable> attributeList, ItemAttributesData data, string recordId, List<string> results)
		{
			if (TQDebug.ItemDebugLevel > 0)
			{
				TQDebug.DebugWriteLine(string.Format(
					CultureInfo.InvariantCulture,
					"Item.ConvertOffenseAttrToString({0}, {1}, {2}, {3}, {4})",
					record,
					attributeList,
					data,
					recordId,
					results));
			}

			// If we are a relic, then sometimes there are multiple values per variable depending on how many pieces we have.
			// Let's determine which variable we want in these cases.
			int variableNumber = 0;
			if (this.IsRelic && recordId == this.BaseItemId)
			{
				variableNumber = this.Number - 1;
			}
			else if (this.HasRelic && recordId == this.relicID)
			{
				variableNumber = Math.Max(this.Var1, 1) - 1;
			}
			else if (this.HasSecondRelic && recordId == this.relic2ID)
			{
				variableNumber = Math.Max(this.Var2, 1) - 1;
			}

			// Pet skills can also have multiple values so we attempt to decode it here
			if (this.IsScroll || this.IsRelic)
			{
				variableNumber = this.GetPetSkillLevel(record, recordId, variableNumber);
			}

			// Triggered skills can have also multiple values so we need to decode it here
			if (record.GetString("Class", 0).ToUpperInvariant().StartsWith("SKILL", StringComparison.OrdinalIgnoreCase))
			{
				variableNumber = this.GetTriggeredSkillLevel(record, recordId, variableNumber);
			}

			// See what variables we have
			ItemAttributesData minData = null;
			ItemAttributesData maxData = null;
			ItemAttributesData minDurData = null;
			ItemAttributesData maxDurData = null;
			ItemAttributesData chanceData = null;
			ItemAttributesData modifierData = null;
			ItemAttributesData durationModifierData = null;
			ItemAttributesData modifierChanceData = null;
			ItemAttributesData damageRatioData = null;  // Added by VillageIdiot
														////ItemAttributesData skillDurationData = null;  // Added by VillageIdiot
			Variable minVar = null;
			Variable maxVar = null;
			Variable minDurVar = null;
			Variable maxDurVar = null;
			Variable chanceVar = null;
			Variable modifierVar = null;
			Variable durationModifierVar = null;
			Variable modifierChanceVar = null;
			Variable damageRatioVar = null;  // Added by VillageIdiot
											 ////Variable skillDurationVar = null;  // Added by VillageIdiot

			bool isGlobal = ItemAttributes.AttributeGroupHas(new Collection<Variable>(attributeList), "Global");
			string globalIndent = null;
			if (isGlobal)
			{
				globalIndent = "&nbsp;&nbsp;&nbsp;&nbsp;";
			}

			foreach (Variable variable in attributeList)
			{
				if (variable == null)
				{
					continue;
				}

				ItemAttributesData attributeData = ItemAttributes.GetAttributeData(variable.Name);
				if (attributeData == null)
				{
					// unknown attribute
					attributeData = new ItemAttributesData(ItemAttributesEffectType.Other, variable.Name, variable.Name, string.Empty, 0);
				}

				switch (attributeData.Variable.ToUpperInvariant())
				{
					case "MIN":
						{
							minData = attributeData;
							minVar = variable;
							break;
						}

					case "MAX":
						{
							maxData = attributeData;
							maxVar = variable;
							break;
						}

					case "DURATIONMIN":
						{
							minDurData = attributeData;
							minDurVar = variable;
							break;
						}

					case "DURATIONMAX":
						{
							maxDurData = attributeData;
							maxDurVar = variable;
							break;
						}

					case "CHANCE":
						{
							chanceData = attributeData;
							chanceVar = variable;
							break;
						}

					case "MODIFIER":
						{
							modifierData = attributeData;
							modifierVar = variable;
							break;
						}

					case "MODIFIERCHANCE":
						{
							modifierChanceData = attributeData;
							modifierChanceVar = variable;
							break;
						}

					case "DURATIONMODIFIER":
						{
							durationModifierData = attributeData;
							durationModifierVar = variable;
							break;
						}

					case "DRAINMIN":
						{
							// Added by VillageIdiot
							minData = attributeData;
							minVar = variable;
							break;
						}

					case "DRAINMAX":
						{
							// Added by VillageIdiot
							maxData = attributeData;
							maxVar = variable;
							break;
						}

					case "DAMAGERATIO":
						{
							// Added by VillageIdiot
							damageRatioData = attributeData;
							damageRatioVar = variable;
							break;
						}
				}
			}

			// Figure out the label string
			string labelTag = null;
			string labelColor = null;
			string label = this.GetLabelAndColorFromTag(data, recordId, ref labelTag, ref labelColor);

			if (TQDebug.ItemDebugLevel > 1)
			{
				TQDebug.DebugWriteLine(string.Empty);
				TQDebug.DebugWriteLine("Full attribute = " + data.FullAttribute);
				TQDebug.DebugWriteLine("Item.label = " + label);
			}

			label = ItemAttributes.ConvertFormat(label);

			// Figure out the Amount string
			string amount = null;
			if (minData != null && maxData != null &&
				minVar.GetSingle(Math.Min(minVar.NumberOfValues - 1, variableNumber)) != maxVar.GetSingle(Math.Min(maxVar.NumberOfValues - 1, variableNumber)))
			{
				if (minDurVar != null)
				{
					amount = this.GetAmountRange(data, variableNumber, minVar, maxVar, ref label, labelColor, minDurVar);
				}
				else
				{
					amount = this.GetAmountRange(data, variableNumber, minVar, maxVar, ref label, labelColor);
				}
			}
			else
			{
				if (minDurVar != null)
				{
					amount = this.GetAmountSingle(data, variableNumber, minVar, maxVar, ref label, labelColor, minDurVar);
				}
				else
				{
					amount = this.GetAmountSingle(data, variableNumber, minVar, maxVar, ref label, labelColor);
				}
			}

			// Figure out the duration string
			string duration = null;
			//If we have both minDurData and maxDurData we also need to check if the actual Values of minDurVar and maxDurVar are actually different
			float minDurVarValue = -1;
			float maxDurVarValue = -1;
			if (minDurData != null)
			{
				minDurVarValue = (float)minDurVar[minDurVar.NumberOfValues - 1];
			}
			if (maxDurData != null)
			{
				maxDurVarValue = (float)maxDurVar[maxDurVar.NumberOfValues - 1];
			}
			if (minDurData != null && maxDurData != null && minDurVarValue != maxDurVarValue)
			{
				duration = GetDurationRange(variableNumber, minDurVar, maxDurVar);
			}
			else
			{
				duration = GetDurationSingle(variableNumber, minDurVar, maxDurVar);
			}

			// Figure out the Damage Ratio string
			string damageRatio = null;
			if (damageRatioData != null)
			{
				damageRatio = GetDamageRatio(variableNumber, damageRatioData, damageRatioVar);
			}

			// Figure out the chance string
			string chance = null;
			if (chanceData != null)
			{
				chance = GetChance(variableNumber, chanceVar);
			}

			// Display the chance + label + Amount + Duration + DamageRatio
			string[] strarray = new string[5];
			int numberOfStrings = 0;
			if (!string.IsNullOrEmpty(label))
			{
				label = Database.MakeSafeForHtml(label);
				if (!string.IsNullOrEmpty(labelColor))
				{
					label = Item.Format("<font color={0}>{1}</font>", labelColor, label);
				}
			}

			if (!string.IsNullOrEmpty(chance))
			{
				strarray[numberOfStrings++] = chance;
			}

			if (!string.IsNullOrEmpty(amount))
			{
				strarray[numberOfStrings++] = amount;
			}

			if (!string.IsNullOrEmpty(label))
			{
				strarray[numberOfStrings++] = label;
			}

			if (!string.IsNullOrEmpty(duration))
			{
				strarray[numberOfStrings++] = duration;
			}

			if (!string.IsNullOrEmpty(damageRatio))
			{
				// Added by VillageIdiot
				strarray[numberOfStrings++] = damageRatio;
			}

			if (!string.IsNullOrEmpty(amount) || !string.IsNullOrEmpty(duration))
			{
				string amountOrDurationText = string.Join(" ", strarray, 0, numberOfStrings);

				// Figure out what color to use
				string fontColor = null;
				if (!isGlobal && (string.IsNullOrEmpty(chance) || data.Effect.Equals("defensiveBlock"))
					&& recordId == this.BaseItemId && string.IsNullOrEmpty(duration) && !string.IsNullOrEmpty(amount))
				{
					if (this.IsWeapon)
					{
						if (data.Effect.Equals("offensivePierceRatio") ||
							data.Effect.Equals("offensivePhysical") ||
							data.Effect.Equals("offensiveBaseFire") ||
							data.Effect.Equals("offensiveBaseCold") ||
							data.Effect.Equals("offensiveBaseLightning") ||
							data.Effect.Equals("offensiveBaseLife"))
						{
							// mundane effect
							fontColor = Database.HtmlColor(Item.GetColor(ItemStyle.Mundane));
						}
					}

					if (this.IsShield)
					{
						if (data.Effect.Equals("defensiveBlock") ||
							data.Effect.Equals("blockRecoveryTime") ||
							data.Effect.Equals("offensivePhysical"))
						{
							fontColor = Database.HtmlColor(Item.GetColor(ItemStyle.Mundane));
						}
					}
				}

				if (string.IsNullOrEmpty(fontColor))
				{
					// magical effect
					fontColor = Database.HtmlColor(Item.GetColor(ItemStyle.Epic));
				}

				amountOrDurationText = string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", fontColor, amountOrDurationText);
				if (isGlobal)
				{
					amountOrDurationText = string.Concat(globalIndent, amountOrDurationText);
				}

				results.Add(amountOrDurationText);
			}
			else
			{
				// null these out to indicate they did not get used
				amount = null;
				duration = null;
				chance = null;
			}

			// now see if we have a modifier
			string modifier = null;
			if (modifierData != null)
			{
				modifier = GetModifier(data, variableNumber, modifierData, modifierVar);
			}

			string durationModifier = null;
			if (durationModifierData != null)
			{
				durationModifier = GetDurationModifier(variableNumber, durationModifierVar);
			}

			string modifierChance = null;
			if (modifierChanceData != null)
			{
				modifierChance = GetChanceModifier(variableNumber, modifierChanceVar);
			}

			numberOfStrings = 0;
			if (!string.IsNullOrEmpty(modifierChance))
			{
				strarray[numberOfStrings++] = modifierChance;
			}

			if (!string.IsNullOrEmpty(modifier))
			{
				strarray[numberOfStrings++] = modifier;
			}

			if (!string.IsNullOrEmpty(durationModifier))
			{
				strarray[numberOfStrings++] = durationModifier;
			}

			if (!string.IsNullOrEmpty(modifier))
			{
				string modifierText = string.Join(" ", strarray, 0, numberOfStrings);
				modifierText = Item.Format("<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Epic)), modifierText);
				if (isGlobal)
				{
					modifierText = string.Concat(globalIndent, modifierText);
				}

				results.Add(modifierText);
			}
			else
			{
				modifier = null;
				modifierChance = null;
				durationModifier = null;
			}

			// Added so we only show the title once for the group.
			bool displayDamageQualifierTitle = true;

			// Now display any other variables we did not see here.
			foreach (Variable variable in attributeList)
			{
				if (variable == null)
				{
					continue;
				}

				ItemAttributesData attributeData = ItemAttributes.GetAttributeData(variable.Name);
				if (attributeData == null)
				{
					// unknown attribute
					attributeData = new ItemAttributesData(ItemAttributesEffectType.Other, variable.Name, variable.Name, string.Empty, 0);
				}

				string normalizedAttributeVariable = attributeData.Variable.ToUpperInvariant();
				if (!(amount != null && (normalizedAttributeVariable == "MIN" || normalizedAttributeVariable == "MAX"
					|| normalizedAttributeVariable == "DRAINMIN" || attributeData.Variable == "DRAINMAX")) && // Added by VillageIdiot
					!(duration != null && (normalizedAttributeVariable == "DURATIONMIN" || normalizedAttributeVariable == "DURATIONMAX")) &&
					!(chance != null && normalizedAttributeVariable == "CHANCE") &&
					!(modifier != null && normalizedAttributeVariable == "MODIFIER") &&
					!(durationModifier != null && normalizedAttributeVariable == "DURATIONMODIFIER") &&
					!(modifierChance != null && normalizedAttributeVariable == "MODIFIERCHANCE") &&
					!(damageRatio != null && normalizedAttributeVariable == "DAMAGERATIO") && // Added by VillageIdiot
					normalizedAttributeVariable != "GLOBAL" &&
					!(normalizedAttributeVariable == "XOR" && isGlobal))
				{
					string line = null;
					string font = null;
					string normalizedFullAttribute = attributeData.FullAttribute.ToUpperInvariant();
					if (normalizedFullAttribute == "CHARACTERBASEATTACKSPEEDTAG")
					{
						// only display this tag if we are a basic weapon
						if (this.IsWeapon && recordId == this.BaseItemId)
						{
							font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Mundane)));
							line = Database.DB.GetFriendlyName(variable.GetString(0));
						}
						else
						{
							line = string.Empty;
						}
					}
					else if (normalizedFullAttribute.EndsWith("GLOBALCHANCE", StringComparison.OrdinalIgnoreCase))
					{
						line = GetGlobalChance(attributeList, variableNumber, variable, ref font);
					}
					else if (normalizedFullAttribute.StartsWith("RACIALBONUS", StringComparison.OrdinalIgnoreCase))
					{
						line = GetRacialBonus(record, results, variableNumber, isGlobal, globalIndent, variable, attributeData, line, ref font);
					}
					else if (normalizedFullAttribute == "AUGMENTALLLEVEL")
					{
						line = GetAugmentAllLevel(variableNumber, variable, ref font);
					}
					else if (normalizedFullAttribute.StartsWith("AUGMENTMASTERYLEVEL", StringComparison.OrdinalIgnoreCase))
					{
						line = GetAugmentMasteryLevel(record, variable, attributeData, ref font);
					}
					else if (normalizedFullAttribute.StartsWith("AUGMENTSKILLLEVEL", StringComparison.OrdinalIgnoreCase))
					{
						line = GetAugmentSkillLevel(record, variable, attributeData, line, ref font);
					}
					else if (this.IsFormulae && recordId == this.BaseItemId)
					{
						// Added by VillageIdiot
						line = GetFormulae(results, variable, attributeData, line, ref font);
					}
					else if (normalizedFullAttribute == "ITEMSKILLNAME")
					{
						line = GetGrantedSkill(record, results, variable, line, ref font);
					}

					// Added by VillageIdiot
					// Shows the header text for the pet bonus
					if (normalizedFullAttribute == "PETBONUSNAME")
					{
						line = GetPetBonusName(ref font);
					}

					// Added by VillageIdiot
					// Set the scale percent here
					if (recordId == this.BaseItemId && normalizedFullAttribute == "ATTRIBUTESCALEPERCENT" && this.itemScalePercent == 1.00)
					{
						this.itemScalePercent += variable.GetSingle(0) / 100;

						// Set line to nothing so we do not see the tag text.
						line = string.Empty;
					}
					else if (normalizedFullAttribute == "SKILLNAME")
					{
						// Added by VillageIdiot
						// This is for Scroll effects which get decoded in the skill code below
						// Set line to nothing so we do not see the tag text.
						line = string.Empty;
					}
					else if (attributeData.EffectType == ItemAttributesEffectType.SkillEffect)
					{
						line = GetSkillEffect(data, variableNumber, variable, attributeData, line, ref font);
					}
					else if (normalizedFullAttribute.EndsWith("DAMAGEQUALIFIER", StringComparison.OrdinalIgnoreCase))
					{
						// Added by VillageIdiot
						// for Damage Absorption

						// Get the qualifier title
						string title = Database.DB.GetFriendlyName("tagDamageAbsorptionTitle");
						if (string.IsNullOrEmpty(title))
						{
							title = "Protects Against :";
						}

						// We really only want to show the title once for the group.
						if (displayDamageQualifierTitle)
						{
							results.Add(title);
							displayDamageQualifierTitle = false;
						}

						// Show the damage type
						string damageTag = attributeData.FullAttribute.Remove(attributeData.FullAttribute.Length - 15);
						damageTag = string.Concat(damageTag.Substring(0, 1).ToUpperInvariant(), damageTag.Substring(1));
						string damageType = Database.DB.GetFriendlyName(string.Concat("tagQualifyingDamage", damageTag));

						string formatSpec = Database.DB.GetFriendlyName("formatQualifyingDamage");
						if (string.IsNullOrEmpty(formatSpec))
						{
							formatSpec = "{     0}";
						}
						else
						{
							if (TQDebug.ItemDebugLevel > 2)
							{
								TQDebug.DebugWriteLine("Item.formatspec (Damage type) = " + formatSpec);
							}

							formatSpec = ItemAttributes.ConvertFormat(formatSpec);
						}

						font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Mundane)));
						line = Item.Format(formatSpec, damageType);
					}

					// We have no line so just show the raw attribute
					if (line == null)
					{
						line = GetRawAttribute(data, variableNumber, variable, ref font);
					}

					// Start finalizing the line of text
					if (line.Length > 0)
					{
						line = Database.MakeSafeForHtml(line);
						if (attributeData.Variable.Length > 0)
						{
							string s = Database.MakeSafeForHtml(attributeData.Variable);
							s = string.Format(CultureInfo.CurrentCulture, " <font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Legendary)), s);
							line = string.Concat(line, s);
						}

						// Add another special case for skill name formatting
						// if it's an activated skill
						if (normalizedFullAttribute == "ITEMSKILLNAME")
						{
							string autoController = record.GetString("itemSkillAutoController", 0);
							if (!string.IsNullOrEmpty(autoController))
							{
								line = string.Concat("<b>", line, "</b>");
							}
						}

						// Add the font tags if necessary
						if (font != null)
						{
							line = string.Concat(font, line, "</font>");
						}

						if (isGlobal)
						{
							line = string.Concat(globalIndent, line);
						}

						// Indent formulae reagents
						if (this.IsFormulae && normalizedFullAttribute.StartsWith("REAGENT", StringComparison.OrdinalIgnoreCase))
						{
							line = string.Concat("&nbsp;&nbsp;&nbsp;&nbsp;", line);
						}

						results.Add(line);
					}

					// Added by VillageIdiot
					// This a special case for pet bonuses
					if (normalizedFullAttribute == "PETBONUSNAME")
					{
						string petBonusID = record.GetString("petBonusName", 0);
						DBRecordCollection petBonusRecord = Database.DB.GetRecordFromFile(petBonusID);
						if (petBonusRecord != null)
						{
							this.GetAttributesFromRecord(petBonusRecord, true, petBonusID, results);
							results.Add(string.Empty);
						}
					}

					// Added by VillageIdiot
					// Another special case for skill description and effects of activated skills
					if (normalizedFullAttribute == "ITEMSKILLNAME" || (this.IsScroll && normalizedFullAttribute == "SKILLNAME"))
					{
						this.GetSkillDescriptionAndEffects(record, results, variable, line);
					}
				}
			}

			if (TQDebug.ItemDebugLevel > 0)
			{
				TQDebug.DebugWriteLine("Exiting Item.ConvertOffenseAttrToString()");
			}
		}

		/// <summary>
		/// Adds the formatted skill description and effects for granted skills to the results list
		/// </summary>
		/// <param name="record">DBRecord databse record</param>
		/// <param name="results">results list</param>
		/// <param name="variable">variable structure</param>
		/// <param name="line">line of text</param>
		private void GetSkillDescriptionAndEffects(DBRecordCollection record, List<string> results, Variable variable, string line)
		{
			string autoController = record.GetString("itemSkillAutoController", 0);
			if (!string.IsNullOrEmpty(autoController) || this.IsScroll)
			{
				DBRecordCollection skillRecord = Database.DB.GetRecordFromFile(variable.GetString(0));

				// Changed by VillageIdiot
				// Get title from the last line
				// Remove the HTML formatting and use for word wrapping
				string lastline = string.Empty;
				if (!this.IsScroll)
				{
					lastline = line;
					lastline = lastline.Remove(lastline.IndexOf("</b>", StringComparison.OrdinalIgnoreCase));
					lastline = lastline.Substring(lastline.IndexOf("<b>", StringComparison.OrdinalIgnoreCase));
				}

				// Set the minimum column width to 30
				// Also takes care of scrolls
				int lineLength = lastline.Length;
				if (lineLength < 30)
				{
					lineLength = 30;
				}

				// Show the description text first
				if (skillRecord != null)
				{
					string buffSkillName = skillRecord.GetString("buffSkillName", 0);

					if (!this.IsScroll)
					{
						// Skip scrolls since they are handled elsewhere with the flavor text
						string descriptionTag, skillDescription;
						Collection<string> skillDescriptionList;

						// Changed by VillageIdiot
						// Let's actually test if it's a buff skill
						if (string.IsNullOrEmpty(buffSkillName))
						{
							descriptionTag = skillRecord.GetString("skillBaseDescription", 0);
							if (descriptionTag.Length != 0)
							{
								skillDescription = Database.DB.GetFriendlyName(descriptionTag);
								if (skillDescription.Length != 0)
								{
									skillDescription = Database.MakeSafeForHtml(skillDescription);
									skillDescriptionList = Database.WrapWords(skillDescription, lineLength);

									foreach (string skillDescriptionFromList in skillDescriptionList)
									{
										results.Add(string.Format(
											CultureInfo.CurrentCulture,
											"<font color={0}>&nbsp;&nbsp;&nbsp;&nbsp;{1}</font>",
											Database.HtmlColor(Item.GetColor(ItemStyle.Mundane)),
											skillDescriptionFromList));
									}

									// Show granted skill level
									if (Item.ShowSkillLevel)
									{
										string formatSpec = Database.DB.GetFriendlyName("MenuLevel");
										if (string.IsNullOrEmpty(formatSpec))
										{
											formatSpec = "Level:   {0}";
										}
										else
										{
											formatSpec = ItemAttributes.ConvertFormat(formatSpec);
										}

										int skillLevel = record.GetInt32("itemSkillLevel", 0);
										if (skillLevel > 0)
										{
											line = Item.Format(formatSpec, skillLevel);
											line = Database.MakeSafeForHtml(line);
											results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>&nbsp;&nbsp;&nbsp;&nbsp;{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Mundane)), line));
										}
									}
								}
							}
						}
						else
						{
							// This skill is a buff
							DBRecordCollection buffSkillRecord = Database.DB.GetRecordFromFile(buffSkillName);
							if (buffSkillRecord != null)
							{
								descriptionTag = buffSkillRecord.GetString("skillBaseDescription", 0);
								if (!string.IsNullOrEmpty(descriptionTag))
								{
									skillDescription = Database.DB.GetFriendlyName(descriptionTag);
									skillDescriptionList = Database.WrapWords(skillDescription, lineLength);

									foreach (string skillDescriptionFromList in skillDescriptionList)
									{
										results.Add(string.Format(
											CultureInfo.CurrentCulture,
											"<font color={0}>&nbsp;&nbsp;&nbsp;&nbsp;{1}</font>",
											Database.HtmlColor(Item.GetColor(ItemStyle.Mundane)),
											skillDescriptionFromList));
									}

									// Show granted skill level
									if (Item.ShowSkillLevel)
									{
										string formatSpec = Database.DB.GetFriendlyName("MenuLevel");
										if (string.IsNullOrEmpty(formatSpec))
										{
											formatSpec = "Level:   {0}";
										}
										else
										{
											formatSpec = ItemAttributes.ConvertFormat(formatSpec);
										}

										int skillLevel = record.GetInt32("itemSkillLevel", 0);
										if (skillLevel > 0)
										{
											line = Item.Format(formatSpec, skillLevel);
											line = Database.MakeSafeForHtml(line);
											results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>&nbsp;&nbsp;&nbsp;&nbsp;{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Mundane)), line));
										}
									}
								}
							}
						}
					}

					// Clear out effects for unnamed skills, unless it's a buff or a scroll.
					if (skillRecord.GetString("skillDisplayName", 0).Length == 0 && string.IsNullOrEmpty(buffSkillName) && !this.IsScroll)
					{
						skillRecord = null;
					}

					// Added by VillageIdiot
					// Adjust for the flavor text of scrolls
					if (skillRecord != null && !this.IsScroll)
					{
						results.Add(string.Empty);
					}

					// Added by VillageIdiot
					// Add the skill effects
					if (skillRecord != null)
					{
						if (skillRecord.GetString("Class", 0).ToUpperInvariant().Equals("SKILL_SPAWNPET"))
						{
							// This is a summon
							this.ConvertPetStats(skillRecord, results);
						}
						else
						{
							// Skill Effects
							if (!string.IsNullOrEmpty(buffSkillName))
							{
								this.GetAttributesFromRecord(Database.DB.GetRecordFromFile(buffSkillName), true, buffSkillName, results);
							}
							else
							{
								this.GetAttributesFromRecord(skillRecord, true, variable.GetString(0), results);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Gets a formatted single amount
		/// </summary>
		/// <param name="data">ItemAttributesData data</param>
		/// <param name="varNum">variable number to look up</param>
		/// <param name="minVar">minVar variable</param>
		/// <param name="maxVar">maxVar variable</param>
		/// <param name="label">label string</param>
		/// <param name="labelColor">label color</param>
		/// <param name="minDurVar">Duration of Damage</param>
		/// <returns>formatted single amount string</returns>
		private string GetAmountSingle(ItemAttributesData data, int varNum, Variable minVar, Variable maxVar, ref string label, string labelColor, Variable minDurVar = null)
		{
			string color = null;
			string amount = null;

			string tag = "DamageSingleFormat";
			if (data.Effect.EndsWith("Stun", StringComparison.OrdinalIgnoreCase) ||
				data.Effect.EndsWith("Freeze", StringComparison.OrdinalIgnoreCase) ||
				data.Effect.EndsWith("Petrify", StringComparison.OrdinalIgnoreCase) ||
				data.Effect.EndsWith("Trap", StringComparison.OrdinalIgnoreCase) ||
				data.Effect.EndsWith("Convert", StringComparison.OrdinalIgnoreCase) ||
				data.Effect.EndsWith("Fear", StringComparison.OrdinalIgnoreCase) ||
				data.Effect.EndsWith("Confusion", StringComparison.OrdinalIgnoreCase) ||
				data.Effect.EndsWith("Disruption", StringComparison.OrdinalIgnoreCase))
			{
				tag = "DamageInfluenceSingleFormat";
			}

			string formatSpec = Database.DB.GetFriendlyName(tag);
			if (string.IsNullOrEmpty(formatSpec))
			{
				formatSpec = "{0}";
				color = Database.HtmlColor(Item.GetColor(ItemStyle.Legendary));
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 2)
				{
					TQDebug.DebugWriteLine("Item.formatspec (single) = " + formatSpec);
				}

				formatSpec = ItemAttributes.ConvertFormat(formatSpec);
			}

			if (label.IndexOf('{') >= 0)
			{
				// the label has formatting codes.  Use it to format the amount
				formatSpec = label;
				label = null;
				color = labelColor;
			}

			Variable currentVariable = null;

			if (minVar != null)
			{
				currentVariable = minVar.clone();
			}
			else if (maxVar != null)
			{
				currentVariable = maxVar.clone();
			}

			if (currentVariable != null)
			{
				// Adjust for itemScalePercent
				// only for floats
				if (currentVariable.DataType == VariableDataType.Float)
				{
					if (minDurVar != null)
					{
						currentVariable[Math.Min(currentVariable.NumberOfValues - 1, varNum)] = (float)currentVariable[Math.Min(currentVariable.NumberOfValues - 1, varNum)] * (float)minDurVar[minDurVar.NumberOfValues - 1] * this.itemScalePercent;
					}
					else
					{
						currentVariable[Math.Min(currentVariable.NumberOfValues - 1, varNum)] = (float)currentVariable[Math.Min(currentVariable.NumberOfValues - 1, varNum)] * this.itemScalePercent;
					}
				}

				amount = Item.Format(formatSpec, currentVariable[Math.Min(currentVariable.NumberOfValues - 1, varNum)]);
				amount = Database.MakeSafeForHtml(amount);
				if (!string.IsNullOrEmpty(color))
				{
					amount = Item.Format("<font color={0}>{1}</font>", color, amount);
				}
			}

			return amount;
		}

		/// <summary>
		/// Gets a formatted range amount
		/// </summary>
		/// <param name="data">ItemAttributesData data</param>
		/// <param name="varNum">variable number to look up</param>
		/// <param name="minVar">minVar variable</param>
		/// <param name="maxVar">maxVar variable</param>
		/// <param name="label">label string</param>
		/// <param name="labelColor">label color</param>
		/// <returns>formatted range string</returns>
		private string GetAmountRange(ItemAttributesData data, int varNum, Variable minVar, Variable maxVar, ref string label, string labelColor, Variable minDurVar = null)
		{
			// Added by VillageIdiot : check to see if min and max are the same
			string color = null;
			string amount = null;

			Variable min = null;
			Variable max = null;
			if (minVar != null)
			{
				min = minVar.clone();
			}
			if (maxVar != null)
			{
				max = maxVar.clone();
			}

			// sweet we have a range
			string tag = "DamageRangeFormat";
			if (data.Effect.EndsWith("Stun", StringComparison.OrdinalIgnoreCase) ||
				data.Effect.EndsWith("Freeze", StringComparison.OrdinalIgnoreCase) ||
				data.Effect.EndsWith("Petrify", StringComparison.OrdinalIgnoreCase) ||
				data.Effect.EndsWith("Trap", StringComparison.OrdinalIgnoreCase) ||
				data.Effect.EndsWith("Convert", StringComparison.OrdinalIgnoreCase) ||
				data.Effect.EndsWith("Fear", StringComparison.OrdinalIgnoreCase) ||
				data.Effect.EndsWith("Confusion", StringComparison.OrdinalIgnoreCase) ||
				data.Effect.EndsWith("Disruption", StringComparison.OrdinalIgnoreCase))
			{
				tag = "DamageInfluenceRangeFormat";
			}
			else if (data.Effect.Equals("defensiveBlock"))
			{
				tag = "DefenseBlock";
			}

			string formatSpec = Database.DB.GetFriendlyName(tag);
			if (string.IsNullOrEmpty(formatSpec))
			{
				formatSpec = "{0}..{1}";
				color = Database.HtmlColor(Item.GetColor(ItemStyle.Legendary));
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 2)
				{
					TQDebug.DebugWriteLine("Item.formatspec (range) = " + formatSpec);
				}

				formatSpec = ItemAttributes.ConvertFormat(formatSpec);
			}

			if (label.IndexOf('{') >= 0)
			{
				// the label has formatting codes.  Use it to format the amount
				formatSpec = label;
				label = null;
				color = labelColor;
			}

			// Added by VillageIdiot
			// Adjust for itemScalePercent
			if (minDurVar != null)
			{
				min[Math.Min(min.NumberOfValues - 1, varNum)] = (float)min[Math.Min(min.NumberOfValues - 1, varNum)] * (float)minDurVar[minDurVar.NumberOfValues - 1] * this.itemScalePercent;
				max[Math.Min(max.NumberOfValues - 1, varNum)] = (float)max[Math.Min(max.NumberOfValues - 1, varNum)] * (float)minDurVar[minDurVar.NumberOfValues - 1] * this.itemScalePercent;
			}
			else
			{
				min[Math.Min(min.NumberOfValues - 1, varNum)] = (float)min[Math.Min(min.NumberOfValues - 1, varNum)] * this.itemScalePercent;
				max[Math.Min(max.NumberOfValues - 1, varNum)] = (float)max[Math.Min(max.NumberOfValues - 1, varNum)] * this.itemScalePercent;
			}

			amount = Item.Format(formatSpec, min[Math.Min(min.NumberOfValues - 1, varNum)], max[Math.Min(max.NumberOfValues - 1, varNum)]);
			amount = Database.MakeSafeForHtml(amount);
			if (!string.IsNullOrEmpty(color))
			{
				amount = Item.Format("<font color={0}>{1}</font>", color, amount);
			}

			return amount;
		}

		/// <summary>
		/// Gets the item label from the tag
		/// </summary>
		/// <param name="data">ItemAttributesData structure for the attribute</param>
		/// <param name="recordId">string containing the database record id</param>
		/// <param name="labelTag">the label tag</param>
		/// <param name="labelColor">the label color which gets modified here</param>
		/// <returns>string containing the label.</returns>
		private string GetLabelAndColorFromTag(ItemAttributesData data, string recordId, ref string labelTag, ref string labelColor)
		{
			labelTag = ItemAttributes.GetAttributeTextTag(data);
			string label;

			if (string.IsNullOrEmpty(labelTag))
			{
				labelTag = string.Concat("?", data.FullAttribute, "?");
				labelColor = Database.HtmlColor(Item.GetColor(ItemStyle.Legendary));
			}

			// if this is an Armor effect and we are not armor, then change it to bonus
			if (labelTag.ToUpperInvariant().Equals("DEFENSEABSORPTIONPROTECTION"))
			{
				if (!this.IsArmor || recordId != this.BaseItemId)
				{
					labelTag = "DefenseAbsorptionProtectionBonus";
					labelColor = Database.HtmlColor(Item.GetColor(ItemStyle.Epic));
				}
				else
				{
					// regular armor attribute is not magical
					labelColor = Database.HtmlColor(Item.GetColor(ItemStyle.Mundane));
				}
			}

			label = Database.DB.GetFriendlyName(labelTag);
			if (string.IsNullOrEmpty(label))
			{
				label = string.Concat("?", labelTag, "?");
				labelColor = Database.HtmlColor(Item.GetColor(ItemStyle.Legendary));
			}

			return label;
		}

		/// <summary>
		/// Gets the level of a triggered skill
		/// </summary>
		/// <param name="record">DBRecord for the triggered skill</param>
		/// <param name="recordId">string containing the record id</param>
		/// <param name="varNum">variable number which we are looking up since there can be multiple values</param>
		/// <returns>int containing the skill level</returns>
		private int GetTriggeredSkillLevel(DBRecordCollection record, string recordId, int varNum)
		{
			DBRecordCollection baseItem = Database.DB.GetRecordFromFile(this.baseItemInfo.ItemId);

			// Check to see if it's a Buff Skill
			if (baseItem.GetString("itemSkillAutoController", 0) != null)
			{
				int level = baseItem.GetInt32("itemSkillLevel", 0);
				if (record.GetString("Class", 0).ToUpperInvariant().StartsWith("SKILLBUFF", StringComparison.OrdinalIgnoreCase))
				{
					DBRecordCollection skill = Database.DB.GetRecordFromFile(this.baseItemInfo.GetString("itemSkillName"));
					if (skill != null && skill.GetString("buffSkillName", 0) == recordId)
					{
						// Use the level from the Base Item.
						varNum = Math.Max(level, 1) - 1;
					}
				}
				else if (baseItem.GetString("itemSkillName", 0) == recordId)
				{
					varNum = Math.Max(level, 1) - 1;
				}
			}

			return varNum;
		}

		/// <summary>
		/// Pet skills can also have multiple values so we attempt to decode it here
		/// unfortunately the level is not in the skill record so we need to look up
		/// the level from the pet record.
		/// </summary>
		/// <param name="record">DBRecord of the skill</param>
		/// <param name="recordId">string of the record id</param>
		/// <param name="varNum">Which variable we are using since there can be multiple values.</param>
		/// <returns>int containing the pet skill level</returns>
		private int GetPetSkillLevel(DBRecordCollection record, string recordId, int varNum)
		{
			// Check to see if this really is a skill
			if (record.GetString("Class", 0).ToUpperInvariant().StartsWith("SKILL_ATTACK", StringComparison.OrdinalIgnoreCase))
			{
				// Check to see if this item creates a pet
				DBRecordCollection petSkill = Database.DB.GetRecordFromFile(this.baseItemInfo.GetString("skillName"));
				string petID = petSkill.GetString("spawnObjects", 0);
				if (!string.IsNullOrEmpty(petID))
				{
					DBRecordCollection petRecord = Database.DB.GetRecordFromFile(petID);
					int foundSkillOffset = 0;
					for (int skillOffset = 0; skillOffset < 17; skillOffset++)
					{
						// There are upto 17 skills
						// Find the skill in the skill tree so that we can get the level
						if (petRecord.GetString(string.Concat("skillName", skillOffset), 0) == recordId)
						{
							break;
						}

						foundSkillOffset++;
					}

					int level = petRecord.GetInt32(string.Concat("skillLevel", foundSkillOffset), 0);
					varNum = Math.Max(level, 1) - 1;
				}
			}

			return varNum;
		}

		/// <summary>
		/// Used for showing the pet statistics
		/// </summary>
		/// <param name="skillRecord">DBRecord of the skill</param>
		/// <param name="results">List containing the results</param>
		private void ConvertPetStats(DBRecordCollection skillRecord, List<string> results)
		{
			string formatSpec, petLine;
			int summonLimit = skillRecord.GetInt32("petLimit", 0);
			if (summonLimit > 1)
			{
				formatSpec = Database.DB.GetFriendlyName("SkillPetLimit");
				if (string.IsNullOrEmpty(formatSpec))
				{
					formatSpec = "{0} Summon Limit";
				}
				else
				{
					formatSpec = ItemAttributes.ConvertFormat(formatSpec);
				}

				petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, summonLimit.ToString(CultureInfo.CurrentCulture));
				results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Mundane)), petLine));
			}

			DBRecordCollection petRecord = Database.DB.GetRecordFromFile(skillRecord.GetString("spawnObjects", 0));
			if (petRecord != null)
			{
				// Print out Pet attributes
				formatSpec = Database.DB.GetFriendlyName("SkillPetDescriptionHeading");
				if (string.IsNullOrEmpty(formatSpec))
				{
					formatSpec = "{0} Attributes:";
				}
				else
				{
					formatSpec = ItemAttributes.ConvertFormat(formatSpec);
				}

				string petNameTag = petRecord.GetString("description", 0);
				string petName = Database.DB.GetFriendlyName(petNameTag);
				float value = 0.0F;
				petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, petName);
				results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Mundane)), petLine));

				// Time to live
				formatSpec = Database.DB.GetFriendlyName("tagSkillPetTimeToLive");
				if (string.IsNullOrEmpty(formatSpec))
				{
					formatSpec = "Life Time {0} Seconds";
				}
				else
				{
					formatSpec = ItemAttributes.ConvertFormat(formatSpec);
				}

				petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, skillRecord.GetSingle("spawnObjectsTimeToLive", 0));
				results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Mundane)), petLine));

				// Health
				value = petRecord.GetSingle("characterLife", 0);
				if (value != 0.0F)
				{
					formatSpec = Database.DB.GetFriendlyName("SkillPetDescriptionHealth");
					if (string.IsNullOrEmpty(formatSpec))
					{
						formatSpec = "{0}  Health";
					}
					else
					{
						formatSpec = ItemAttributes.ConvertFormat(formatSpec);
					}

					petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, value);
					results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Mundane)), petLine));
				}

				// Energy
				value = petRecord.GetSingle("characterMana", 0);
				if (value != 0.0F)
				{
					formatSpec = Database.DB.GetFriendlyName("SkillPetDescriptionMana");
					if (string.IsNullOrEmpty(formatSpec))
					{
						formatSpec = "{0}  Energy";
					}
					else
					{
						formatSpec = ItemAttributes.ConvertFormat(formatSpec);
					}

					petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, value);
					results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Mundane)), petLine));
				}

				// Add abilities text
				results.Add(string.Empty);
				formatSpec = Database.DB.GetFriendlyName("tagSkillPetAbilities");
				if (string.IsNullOrEmpty(formatSpec))
				{
					formatSpec = "{0} Abilities:";
				}
				else
				{
					formatSpec = ItemAttributes.ConvertFormat(formatSpec);
				}

				petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, petName);
				results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Mundane)), petLine));

				// Show Physical Damage
				value = petRecord.GetSingle("handHitDamageMin", 0);
				float value2 = petRecord.GetSingle("handHitDamageMax", 0);

				if (value > 1.0F || value2 > 2.0F)
				{
					if (value2 == 0.0F || value == value2)
					{
						formatSpec = Database.DB.GetFriendlyName("SkillPetDescriptionDamageMinOnly");
						if (string.IsNullOrEmpty(formatSpec))
						{
							formatSpec = "{0}  Damage";
						}
						else
						{
							formatSpec = ItemAttributes.ConvertFormat(formatSpec);
						}

						petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, value);
						results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Mundane)), petLine));
					}
					else
					{
						formatSpec = Database.DB.GetFriendlyName("SkillPetDescriptionDamageMinMax");
						if (string.IsNullOrEmpty(formatSpec))
						{
							formatSpec = "{0} - {1}  Damage";
						}
						else
						{
							formatSpec = ItemAttributes.ConvertFormat(formatSpec);
						}

						petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, value, value2);
						results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Mundane)), petLine));
					}
				}

				// Show the pet's skills
				string[] skills = new string[17];  // Upto 17 skills in the tree
				int[] skillLevels = new int[17];
				int numSkills = 0;
				string tmp;
				int temp;

				for (int i = 0; i < skills.Length; i++)
				{
					tmp = petRecord.GetString(string.Concat("skillName", i), 0);
					if (string.IsNullOrEmpty(tmp))
					{
						continue;
					}

					skills[numSkills] = tmp;
					temp = petRecord.GetInt32(string.Concat("skillLevel", i), 0);
					if (temp < 1)
					{
						temp = 1;
					}

					skillLevels[numSkills] = temp;
					numSkills++;
				}

				for (int i = 0; i < numSkills; i++)
				{
					if (skills[i] != null && !skills[i].ToLower().StartsWith("records"))
					{
						continue;
					}

					DBRecordCollection skillRecord1 = Database.DB.GetRecordFromFile(skills[i]);
					DBRecordCollection record = null;
					string skillClass = skillRecord1.GetString("Class", 0);

					// Skip passive skills
					if (skillClass.ToUpperInvariant() == "SKILL_PASSIVE")
					{
						continue;
					}

					string skillNameTag = null;
					string skillName = null;
					string recordID = null;
					string buffSkillName = skillRecord1.GetString("buffSkillName", 0);

					if (string.IsNullOrEmpty(buffSkillName))
					{
						record = skillRecord1;
						recordID = skills[i];
						skillNameTag = skillRecord.GetString("skillDisplayName", 0);
						if (skillNameTag.Length != 0)
						{
							skillName = Database.DB.GetFriendlyName(skillNameTag);
						}
					}
					else
					{
						// This is a buff skill
						DBRecordCollection buffSkillRecord = Database.DB.GetRecordFromFile(buffSkillName);
						if (buffSkillRecord != null)
						{
							record = buffSkillRecord;
							recordID = buffSkillName;
							skillNameTag = buffSkillRecord.GetString("skillDisplayName", 0);
							if (skillNameTag.Length != 0)
							{
								skillName = Database.DB.GetFriendlyName(skillNameTag);
							}
						}
					}

					if (skillName.Length == 0)
					{
						results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Legendary)), skillNameTag));
					}
					else
					{
						results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Mundane)), skillName));
					}

					this.GetAttributesFromRecord(record, true, recordID, results);
					results.Add(string.Empty);
				}
			}
		}

		/// <summary>
		/// Converts the item's attribute list to a string
		/// </summary>
		/// <param name="record">DBRecord for the item</param>
		/// <param name="attributeList">ArrayList containing the attributes list</param>
		/// <param name="recordId">string containing the record id</param>
		/// <param name="results">List containing the results</param>
		private void ConvertAttributeListToString(DBRecordCollection record, List<Variable> attributeList, string recordId, List<string> results)
		{
			if (TQDebug.ItemDebugLevel > 0)
			{
				TQDebug.DebugWriteLine(string.Format(
					CultureInfo.InvariantCulture,
					"Item.ConvertAttrListToString ({0}, {1}, {2}, {3})",
					record,
					attributeList,
					recordId,
					results));
			}

			// see what kind of effects are in this list
			Variable variable = (Variable)attributeList[0];
			ItemAttributesData data = ItemAttributes.GetAttributeData(variable.Name);
			if (data == null)
			{
				// unknown attribute
				if (TQDebug.ItemDebugLevel > 0)
				{
					TQDebug.DebugWriteLine("Error - Unknown Attribute.");
				}

				data = new ItemAttributesData(ItemAttributesEffectType.Other, variable.Name, variable.Name, string.Empty, 0);
			}

			if (TQDebug.ItemDebugLevel > 0)
			{
				TQDebug.DebugWriteLine("Exiting Item.ConvertAttrListToString ()");
			}

			this.ConvertOffenseAttributesToString(record, attributeList, data, recordId, results);
			return;
		}

		#endregion Item Private Methods
	}
}
