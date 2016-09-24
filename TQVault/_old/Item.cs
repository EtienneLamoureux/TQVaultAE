//-----------------------------------------------------------------------
// <copyright file="Item.cs" company="bman654">
//     Copyright (c) Brandon Wallace. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVault
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using ExpressionEvaluator;
    using TQVault.Properties;
    using TQVaultData;

/*    /// <summary>
    /// Sack panel types
    /// </summary>
    public enum SackType
    {
        /// <summary>
        /// Sack panel
        /// </summary>
        Sack = 0,

        /// <summary>
        /// Stash panel
        /// </summary>
        Stash,

        /// <summary>
        /// Equipment panel
        /// </summary>
        Equipment,

        /// <summary>
        /// Player panel
        /// </summary>
        Player,

        /// <summary>
        /// Vault panel
        /// </summary>
        Vault,

        /// <summary>
        /// Trash panel
        /// </summary>
        Trash,

        /// <summary>
        /// Transfer stash
        /// </summary>
        TransferStash
    }*/

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
    /// Class for holding item information
    /// </summary>
    public class Item
    {
        /// <summary>
        /// Random number used as a seed for new items
        /// </summary>
        private static Random random = InitializeRandom();

        /// <summary>
        /// Indicator that the item is located in a weapon slot in the equipment panel.
        /// </summary>
        private static int weaponSlotIndicator = -3;

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

        /// <summary>
        /// Base item database record ID
        /// </summary>
        private string baseItemID;

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
        /// Relic bonus databse record ID
        /// </summary>
        private string relicBonusID;

        /// <summary>
        /// Item seed ranges from 0 to 0x7FFF
        /// </summary>
        private int seed;

        /// <summary>
        /// holds # relics
        /// </summary>
        private int var1; 

        /// <summary>
        ///  If this is a stack, the number of items in this stack
        /// </summary>
        private int stackSize;

        /// <summary>
        /// X location
        /// </summary>
        private int x;

        /// <summary>
        /// Y location
        /// </summary>
        private int y;

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
        /// Info structure for the item's embedded relic
        /// </summary>
        private Info relicInfo;

        /// <summary>
        /// Info structure for the completed relic bonus
        /// </summary>
        private Info relicBonusInfo;

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
        /// Added by VillageIdiot 
        /// Used for level calculation
        /// </summary>
        private int attributeCount;

        /// <summary>
        /// Added by VillageIdiot 
        /// Used so that attributes are not counted multiple times
        /// </summary>
        private bool isAttributeCounted;

        /// <summary>
        /// Added by VillageIdiot
        /// Used for properties display
        /// </summary>
        private string[] bareAttributes;

        /// <summary>
        /// Added by VillageIdiot
        /// Used for itemScalePercent calculation
        /// </summary>
        private float itemScalePercent;

        /// <summary>
        /// Holds container type for this item
        /// </summary>
        private SackType containerType;

        /// <summary>
        /// Bitmap for this item
        /// </summary>
        private Bitmap itemBitmap;

        /// <summary>
        /// Width in cells of this item
        /// </summary>
        private int itemWidth;

        /// <summary>
        /// Height in cells of this item
        /// </summary>
        private int itemHeight;

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
            this.stackSize = 1;
        }

        /// <summary>
        /// Gets the weapon slot indicator value
        /// This is a special value in the coordinates that signals an item is in a weapon slot.
        /// </summary>
        public static int WeaponSlotIndicator
        {
            get
            {
                return weaponSlotIndicator;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the item is in an equipment weapon slot.
        /// </summary>
        public bool IsInWeaponSlot
        {
            get
            {
                return this.x == weaponSlotIndicator;
            }
        }

        /// <summary>
        /// Gets the base item id
        /// </summary>
        public string BaseItemId
        {
            get
            {
                return this.baseItemID;
            }
        }

        /// <summary>
        /// Gets or sets the relic bonus id
        /// </summary>
        public string RelicBonusId
        {
            get
            {
                return this.relicBonusID;
            }

            set
            {
                this.relicBonusID = value;
            }
        }

        /// <summary>
        /// Gets or sets the item seed
        /// </summary>
        public int Seed
        {
            get
            {
                return this.seed;
            }

            set
            {
                this.seed = value;
            }
        }

        /// <summary>
        /// Gets the number of relics
        /// </summary>
        public int Var1
        {
            get
            {
                return this.var1;
            }
        }

        /// <summary>
        /// Gets or sets the stack size.
        /// Used for stackable items like potions.
        /// </summary>
        public int StackSize
        {
            get
            {
                return this.stackSize;
            }

            set
            {
                this.stackSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the X cell position of the item.
        /// </summary>
        public int PositionX
        {
            get
            {
                return this.x;
            }

            set
            {
                this.x = value;
            }
        }

        /// <summary>
        /// Gets or sets the Y cell position of the item.
        /// </summary>
        public int PositionY
        {
            get
            {
                return this.y;
            }

            set
            {
                this.y = value;
            }
        }

        /// <summary>
        /// Gets the relic info
        /// </summary>
        public Info RelicInfo
        {
            get
            {
                return this.relicInfo;
            }
        }

        /// <summary>
        /// Gets or sets the relic bonus info
        /// </summary>
        public Info RelicBonusInfo
        {
            get
            {
                return this.relicBonusInfo;
            }

            set
            {
                this.relicBonusInfo = value;
            }
        }                     

        /// <summary>
        /// Gets the item's bitmap
        /// </summary>
        public Bitmap ItemBitmap
        {
            get
            {
                return this.itemBitmap; 
            }
        }
        
        /// <summary>
        /// Gets the item's width in cells
        /// </summary>
        public int Width 
        {
            get
            {
                return this.itemWidth; 
            }
        }

        /// <summary>
        /// Gets the item's height in cells.
        /// </summary>
        public int Height
        {
            get
            {
                return this.itemHeight; 
            }
        }

        /// <summary>
        /// Gets or sets the item container type
        /// </summary>
        public SackType ContainerType
        {
            get
            {
                return this.containerType; 
            }

            set 
            {
                this.containerType = value; 
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not the item comes from the expansion pack.
        /// </summary>
        public bool IsImmortalThrone
        {
            get
            {
                if (this.baseItemID.ToUpperInvariant().IndexOf("XPACK\\", StringComparison.OrdinalIgnoreCase) >= 0)
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
                else if (this.IsImmortalThrone)
                {
                    if (this.baseItemID.ToUpperInvariant().IndexOf("\\SCROLLS\\", StringComparison.OrdinalIgnoreCase) >= 0)
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
                if (this.IsImmortalThrone)
                {
                    if (this.baseItemID.ToUpperInvariant().IndexOf("\\PARCHMENTS\\", StringComparison.OrdinalIgnoreCase) >= 0)
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
                else if (this.IsImmortalThrone)
                {
                    if (this.baseItemID.ToUpperInvariant().IndexOf("\\ARCANEFORMULAE\\", StringComparison.OrdinalIgnoreCase) >= 0)
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
                else if (this.IsImmortalThrone)
                {
                    if (!this.IsFormulae && this.baseItemID.ToUpperInvariant().IndexOf("\\ARTIFACTS\\", StringComparison.OrdinalIgnoreCase) >= 0)
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
                else if (!this.IsImmortalThrone)
                {
                    if (this.baseItemID.ToUpperInvariant().IndexOf("QUEST", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }
                else if (this.baseItemID.ToUpperInvariant().IndexOf("QUESTS", StringComparison.OrdinalIgnoreCase) >= 0)
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
                    return this.stackSize;
                }

                if (this.IsRelic)
                {
                    return Math.Max(this.var1, 1);
                }

                return 0;
            }

            set
            {
                // Added by VillageIdiot
                if (this.DoesStack)
                {
                    this.stackSize = value;
                }
                else if (this.IsRelic)
                {
                    // Limit value to complete Relic level
                    if (value >= this.baseItemInfo.CompletedRelicLevel)
                    {
                        this.var1 = this.baseItemInfo.CompletedRelicLevel;
                    }
                    else
                    {
                        this.var1 = value;
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
                    return this.baseItemID.ToUpperInvariant().IndexOf("ONESHOT\\POTION", StringComparison.OrdinalIgnoreCase) != -1;
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
                else if (!this.IsImmortalThrone)
                {
                    return this.baseItemID.ToUpperInvariant().IndexOf("ANIMALRELICS", StringComparison.OrdinalIgnoreCase) != -1;
                }
                else
                {
                    if (this.baseItemID.ToUpperInvariant().IndexOf("\\CHARMS\\", StringComparison.OrdinalIgnoreCase) != -1)
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
                else if (!this.IsImmortalThrone)
                {
                    return this.baseItemID.ToUpperInvariant().IndexOf("RELICS", StringComparison.OrdinalIgnoreCase) != -1;
                }
                else
                {
                    if (this.baseItemID.ToUpperInvariant().IndexOf("\\RELICS\\", StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        return true;
                    }

                    if (this.baseItemID.ToUpperInvariant().IndexOf("\\CHARMS\\", StringComparison.OrdinalIgnoreCase) != -1)
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
                    return this.var1 >= this.baseItemInfo.CompletedRelicLevel;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets the artifact/charm/relic bonus loot table
        /// returns null if the item is not an artifact/charm/relic or does not contain a charm/relic
        /// </summary>
        public LootTable BonusTable
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
                    if (this.relicInfo != null)
                    {
                        lootTableID = this.relicInfo.GetString("bonusTableName");
                    }
                }
                else if (this.IsArtifact && this.relicBonusInfo != null)
                {
                    // for artifacts we need to find the formulae that was used to create the artifact.  sucks to be us
                    // The formulas seem to always be in the arcaneformulae subfolder with a _formula on the end
                    // of the filename
                    string folder = Path.GetDirectoryName(this.baseItemID);
                    folder = Path.Combine(folder, "arcaneformulae");
                    string file = Path.GetFileNameWithoutExtension(this.baseItemID);

                    // Damn it, IL did not keep the filename consistent on Kingslayer (Sands of Kronos)
                    if (file.ToUpperInvariant() == "E_GA_SANDOFKRONOS")
                    {
                        file = file.Insert(9, "s");
                    }

                    file = string.Concat(file, "_formula");
                    file = Path.Combine(folder, file);
                    file = Path.ChangeExtension(file, Path.GetExtension(this.baseItemID));

                    // Now lookup this record.
                    DBRecord record = Database.DB.GetRecordFromFile(file);
                    if (record != null)
                    {
                        lootTableID = record.GetString("artifactBonusTableName", 0);
                    }
                }

                if (lootTableID != null && lootTableID.Length > 0)
                {
                    return new LootTable(lootTableID);
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

        /// <summary>
        /// Gets the color for a particular item style
        /// </summary>
        /// <param name="style">ItemStyle enumeration</param>
        /// <returns>System.Drawing.Color of the particular itemstyle</returns>
        public static Color GetColor(ItemStyle style)
        {
            switch (style)
            {
                case ItemStyle.Broken:
                    return Color.FromArgb(153, 153, 153);   // Silver
                case ItemStyle.Common:
                    return Color.FromArgb(255, 245, 43);    // Yellow
                case ItemStyle.Epic:
                    return Color.FromArgb(0, 163, 255);     // Blue
                case ItemStyle.Legendary:
                    return Color.FromArgb(217, 5, 255);     // Purple
                case ItemStyle.Mundane:
                    return Color.FromArgb(255, 255, 255);   // White
                case ItemStyle.Potion:
                    return Color.FromArgb(255, 0, 0);       // Red
                case ItemStyle.Quest:
                    return Color.FromArgb(217, 5, 255);     // Purple
                case ItemStyle.Rare:
                    return Color.FromArgb(64, 255, 64);     // Green
                case ItemStyle.Relic:
                    return Color.FromArgb(255, 173, 0);     // Orange
                case ItemStyle.Parchment:
                    return Color.FromArgb(0, 163, 255);     // Blue
                case ItemStyle.Scroll:
                    return Color.FromArgb(145, 203, 0);     // Yellow Green
                case ItemStyle.Formulae:
                    return Color.FromArgb(0, 255, 209);     // Turquoise
                case ItemStyle.Artifact:
                    return Color.FromArgb(0, 255, 209);     // Turquoise
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
        /// Gets the string name of a particular item style
        /// </summary>
        /// <param name="itemStyle">ItemStyle enumeration</param>
        /// <returns>Localized string of the item style</returns>
        public static string GetItemStyleString(ItemStyle itemStyle)
        {
            switch (itemStyle)
            {
                case ItemStyle.Broken:
                    return Resources.ItemStyleBroken;
                case ItemStyle.Artifact:
                    return Resources.ItemStyleArtifact;
                case ItemStyle.Formulae:
                    return Resources.ItemStyleFormulae;
                case ItemStyle.Scroll:
                    return Resources.ItemStyleScroll;
                case ItemStyle.Parchment:
                    return Resources.ItemStyleParchment;
                case ItemStyle.Relic:
                    return Resources.ItemStyleRelic;
                case ItemStyle.Potion:
                    return Resources.ItemStylePotion;
                case ItemStyle.Quest:
                    return Resources.ItemStyleQuest;
                case ItemStyle.Epic:
                    return Resources.ItemStyleEpic;
                case ItemStyle.Legendary:
                    return Resources.ItemStyleLegendary;
                case ItemStyle.Rare:
                    return Resources.ItemStyleRare;
                case ItemStyle.Common:
                    return Resources.ItemStyleCommon;
                default:
                    return Resources.ItemStyleMundane;
            }
        }

        /// <summary>
        /// Removes the color tag from a line of text.
        /// Should be called after GetColorTag()
        /// </summary>
        /// <param name="text">text to be clipped</param>
        /// <returns>text with color tag removed</returns>
        public static string ClipColorTag(string text)
        {
            if (text.StartsWith("^", StringComparison.OrdinalIgnoreCase))
            {
                // If there are not braces assume a 2 character code.
                text = text.Substring(2);
            }
            else if (text.StartsWith("{^", StringComparison.OrdinalIgnoreCase))
            {
                int i = text.IndexOf('^');
                if (i != -1)
                {
                    // Make sure there is a control code in there.
                    i = text.IndexOf('}', i + 1);
                    text = text.Substring(i + 1);
                }
            }

            return text;
        }

        /// <summary>
        /// Gets the item's color
        /// </summary>
        /// <returns>System.Drawing.Color for the item</returns>
        public Color GetColor()
        {
            return GetColor(ItemStyle);
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
            newItem.baseItemID = string.Empty;
            newItem.prefixID = string.Empty;
            newItem.suffixID = string.Empty;
            newItem.relicID = string.Empty;
            newItem.relicBonusID = string.Empty;
            newItem.seed = GenerateSeed();
            newItem.var1 = this.var1;
            newItem.x = -1;
            newItem.y = -1;

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

            Item newItem = new Item();
            newItem.beginBlockCrap1 = this.beginBlockCrap1;
            newItem.endBlockCrap1 = this.endBlockCrap1;
            newItem.beginBlockCrap2 = this.beginBlockCrap2;
            newItem.endBlockCrap2 = this.endBlockCrap2;
            newItem.baseItemID = baseItemId;
            newItem.prefixID = string.Empty;
            newItem.suffixID = string.Empty;
            newItem.relicID = string.Empty;
            newItem.relicBonusID = string.Empty;
            newItem.seed = GenerateSeed();
            newItem.var1 = this.var1;
            newItem.x = -1;
            newItem.y = -1;

            return newItem;
        }

        /// <summary>
        /// Removes the relic from this item
        /// </summary>
        /// <returns>Returns the removed relic as a new Item</returns>
        public Item RemoveRelic()
        {
            if (!this.HasRelic)
            {
                return null;
            }

            Item newRelic = this.MakeEmptyCopy(this.relicID);
            newRelic.GetDBData(Database.DB);

            // Now clear out our relic data
            this.relicID = string.Empty;
            this.relicBonusID = string.Empty;
            this.var1 = 0;
            this.relicInfo = null;
            this.relicBonusInfo = null;

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
                newItem.seed = GenerateSeed();
            }

            newItem.x = -1;
            newItem.y = -1;
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
            if (!this.DoesStack || this.stackSize < 2)
            {
                return null;
            }

            // make a complete copy then change a few things
            Item newItem = (Item)this.MemberwiseClone();

            newItem.stackSize = this.stackSize - 1;
            newItem.var1 = 0;
            newItem.seed = GenerateSeed();
            newItem.x = -1;
            newItem.y = -1;
            newItem.MarkModified();

            this.MarkModified();

            this.stackSize = 1;

            return newItem;
        }

        /// <summary>
        /// Gets a color tag for a line of text
        /// </summary>
        /// <param name="text">text containing the color tag</param>
        /// <returns>System.Drawing.Color of the embedded color code</returns>
        public Color GetColorTag(string text)
        {
            // Look for a formatting tag in the beginning of the string
            string colorCode = null;
            if (text.StartsWith("^", StringComparison.OrdinalIgnoreCase))
            {  
                // If there are not braces assume a 2 character code.
                colorCode = text.Substring(1, 1).ToUpperInvariant();
            }
            else if (text.StartsWith("{^", StringComparison.OrdinalIgnoreCase))
            {
                int i = text.IndexOf('^');
                if (i == -1)
                {
                    colorCode = null;
                }
                else
                {  
                    colorCode = text.Substring(i + 1, 1).ToUpperInvariant();
                }
            }

            if (colorCode != null)
            {
                // We found something so lets try to find the code
                if (colorCode == "A")
                {
                    // aqua
                    return Color.FromArgb(0, 255, 255);
                }
                else if (colorCode == "B")
                {
                    // blue
                    return GetColor(ItemStyle.Epic);
                }
                else if (colorCode == "C")
                {
                    // light cyan
                    return Color.FromArgb(224, 255, 255);
                }
                else if (colorCode == "D")
                {
                    // dark grey
                    return GetColor(ItemStyle.Broken);
                }
                else if (colorCode == "F")
                {
                    // fuschia
                    return Color.FromArgb(255, 0, 255);
                }
                else if (colorCode == "G")
                {
                    // green
                    return GetColor(ItemStyle.Rare);
                }
                else if (colorCode == "I")
                {
                    // indigo
                    return Color.FromArgb(75, 0, 130);
                }
                else if (colorCode == "K")
                {
                    // khaki
                    return Color.FromArgb(195, 176, 145);
                }
                else if (colorCode == "L")
                {
                    // yellow green
                    return GetColor(ItemStyle.Scroll);
                }
                else if (colorCode == "M")
                {
                    // maroon
                    return Color.FromArgb(128, 0, 0);
                }
                else if (colorCode == "O")
                {
                    // orange
                    return GetColor(ItemStyle.Relic);
                }
                else if (colorCode == "P")
                {
                    // purple
                    return GetColor(ItemStyle.Legendary);
                }
                else if (colorCode == "R")
                {
                    // red
                    return GetColor(ItemStyle.Potion);
                }
                else if (colorCode == "S")
                {
                    // silver
                    return Color.FromArgb(224, 224, 224);
                }
                else if (colorCode == "T")
                {
                    // turquoise
                    return GetColor(ItemStyle.Artifact);
                }
                else if (colorCode == "W")
                {
                    // white
                    return GetColor(ItemStyle.Mundane);
                }
                else if (colorCode == "Y")
                {
                    // yellow
                    return GetColor(ItemStyle.Common);
                }
                else
                {
                    return GetColor(ItemStyle);
                }
            }

            // We found nothing so use the standard color code for the item
            return GetColor(ItemStyle);
        }

        /// <summary>
        /// Gets a string containing the item name and attributes.
        /// </summary>
        /// <param name="db">database instance</param>
        /// <param name="basicInfoOnly">Flag indicating whether or not to return only basic info</param>
        /// <param name="relicInfoOnly">Flag indicating whether or not to return only relic info</param>
        /// <returns>A string containing the item name and attributes</returns>
        public string ToString(Database db, bool basicInfoOnly = false, bool relicInfoOnly = false)
        {
            string[] params1 = new string[16];
            int i = 0;

            if (!relicInfoOnly)
            {
                if (!this.IsRelic && this.prefixID.Length > 0)
                {
                    if (this.prefixInfo != null)
                    {
                        params1[i] = db.GetFriendlyName(this.prefixInfo.DescriptionTag);
                        if (params1[i] == null)
                        {
                            params1[i] = this.prefixID;
                        }
                    }
                    else
                    {
                        params1[i] = this.prefixID;
                    }

                    ++i;
                }

                if (this.baseItemInfo == null)
                {
                    params1[i++] = this.baseItemID;
                }
                else
                {
                    // style quality description
                    if (this.baseItemInfo.StyleTag.Length > 0)
                    {
                        if (!this.IsPotion && !this.IsRelic && !this.IsScroll && !this.IsParchment && !this.IsQuestItem)
                        {
                            params1[i] = db.GetFriendlyName(this.baseItemInfo.StyleTag);
                            if (params1[i] == null)
                            {
                                params1[i] = this.baseItemInfo.StyleTag;
                            }

                            ++i;
                        }
                    }

                    if (this.baseItemInfo.QualityTag.Length > 0)
                    {
                        params1[i] = db.GetFriendlyName(this.baseItemInfo.QualityTag);
                        if (params1[i] == null)
                        {
                            params1[i] = this.baseItemInfo.QualityTag;
                        }

                        ++i;
                    }

                    params1[i] = db.GetFriendlyName(this.baseItemInfo.DescriptionTag);
                    if (params1[i] == null)
                    {
                        params1[i] = this.baseItemID;
                    }

                    ++i;

                    if (!basicInfoOnly && this.IsRelic)
                    {
                        // Add the number of charms in the set acquired.
                        if (this.IsRelicComplete)
                        {
                            if (this.relicBonusID.Length > 0)
                            {
                                string bonus = Path.GetFileNameWithoutExtension(TQData.NormalizeRecordPath(this.relicBonusID));
                                string tag = "tagRelicBonus";
                                if (this.IsCharm)
                                {
                                    tag = "tagAnimalPartcompleteBonus";
                                }

                                string bonusTitle = db.GetFriendlyName(tag);
                                if (bonusTitle == null)
                                {
                                    bonusTitle = "Completion Bonus: ";
                                }

                                params1[i] = string.Format(CultureInfo.CurrentCulture, "({0} {1})", bonusTitle, bonus);
                            }
                            else
                            {
                                string tag = "tagRelicComplete";
                                if (this.IsCharm)
                                {
                                    tag = "tagAnimalPartComplete";
                                }

                                string completed = db.GetFriendlyName(tag);
                                if (completed == null)
                                {
                                    completed = "Completed";
                                }

                                params1[i] = string.Concat("(", completed, ")");
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
                            
                            string type = db.GetFriendlyName(tag1);
                            if (type == null)
                            {
                                type = "Relic";
                            }
                            
                            string formatSpec = db.GetFriendlyName(tag2);
                            if (formatSpec == null)
                            {
                                formatSpec = "{0} - {1} / {2}";
                            }
                            else
                            {
                                formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
                            }

                            params1[i] = string.Concat("(", Item.Format(formatSpec, type, this.Number, this.baseItemInfo.CompletedRelicLevel), ")");
                        }

                        ++i;
                    }
                    else if (!basicInfoOnly && this.IsArtifact)
                    {
                        // Added by VillageIdiot
                        // Add Artifact completion bonus
                        if (this.relicBonusID.Length > 0)
                        {
                            string bonus = Path.GetFileNameWithoutExtension(TQData.NormalizeRecordPath(this.relicBonusID));
                            string tag = "xtagArtifactBonus";
                            string bonusTitle = db.GetFriendlyName(tag);
                            if (bonusTitle == null)
                            {
                                bonusTitle = "Completion Bonus: ";
                            }

                            params1[i] = string.Format(CultureInfo.CurrentCulture, "({0} {1})", bonusTitle, bonus);
                        }

                        ++i;
                    }
                    else if (this.DoesStack)
                    {
                        // display the # potions
                        if (this.Number > 1)
                        {
                            params1[i] = string.Format(CultureInfo.CurrentCulture, "({0:n0})", this.Number);
                            ++i;
                        }
                    }
                }

                if (!this.IsRelic && this.suffixID.Length > 0)
                {
                    if (this.suffixInfo != null)
                    {
                        params1[i] = db.GetFriendlyName(this.suffixInfo.DescriptionTag);
                        if (params1[i] == null)
                        {
                            params1[i] = this.suffixID;
                        }
                    }
                    else
                    {
                        params1[i] = this.suffixID;
                    }

                    ++i;
                }
            }

            if (!basicInfoOnly && this.HasRelic)
            {
                if (!relicInfoOnly)
                {
                    params1[i++] = Resources.ItemWith;
                }

                if (this.relicInfo != null)
                {
                    params1[i] = db.GetFriendlyName(this.relicInfo.DescriptionTag);
                    if (params1[i] == null)
                    {
                        params1[i] = this.relicID;
                    }
                }
                else
                {
                    params1[i] = this.relicID;
                }

                ++i;

                // Add the number of charms in the set acquired.
                if (!relicInfoOnly)
                {
                    if (this.relicInfo != null)
                    {
                        if (this.var1 >= this.relicInfo.CompletedRelicLevel)
                        {
                            if (!relicInfoOnly && this.relicBonusID.Length > 0)
                            {
                                string bonus = Path.GetFileNameWithoutExtension(TQData.NormalizeRecordPath(this.relicBonusID));
                                params1[i] = string.Format(CultureInfo.CurrentCulture, Resources.ItemRelicBonus, bonus);
                            }
                            else
                            {
                                params1[i] = Resources.ItemRelicCompleted;
                            }
                        }
                        else
                        {
                            params1[i] = string.Format(CultureInfo.CurrentCulture, "({0:n0}/{1:n0})", Math.Max(1, this.var1), this.relicInfo.CompletedRelicLevel);
                        }
                    }
                    else
                    {
                        params1[i] = string.Format(CultureInfo.CurrentCulture, "({0:n0}/??)", this.var1);
                    }

                    ++i;
                }
            }

            if (!relicInfoOnly && this.IsQuestItem)
            {
                params1[i++] = Resources.ItemQuest;
            }

            if (!basicInfoOnly && !relicInfoOnly && this.IsImmortalThrone)
            {
                params1[i++] = "(IT)";
            }

            // Now combine it all with spaces between
            return string.Join(" ", params1, 0, i);
        }

        /// <summary>
        /// Gets the item's requirements
        /// </summary>
        /// <param name="db">database instance</param>
        /// <returns>A string containing the items requirements</returns>
        public string GetRequirements(Database db)
        {
            if (this.requirementsString != null)
            {
                return this.requirementsString;
            }

            SortedList<string, Variable> req = new SortedList<string, Variable>();
            if (this.baseItemInfo != null)
            {
                GetRequirementsFromRecord(req, db, db.GetRecordFromFile(this.baseItemID));
                this.GetDynamicRequirementsFromRecord(req, db, this.baseItemInfo);
            }

            if (this.prefixInfo != null)
            {
                GetRequirementsFromRecord(req, db, db.GetRecordFromFile(this.prefixID));
            }

            if (this.suffixInfo != null)
            {
                GetRequirementsFromRecord(req, db, db.GetRecordFromFile(this.suffixID));
            }

            if (this.relicInfo != null)
            {
                GetRequirementsFromRecord(req, db, db.GetRecordFromFile(this.relicID));
            }

            // Add Artifact level requirement to formula
            if (this.IsFormulae && this.baseItemInfo != null)
            {
                string artifactID = this.baseItemInfo.GetString("artifactName");
                GetRequirementsFromRecord(req, db, db.GetRecordFromFile(artifactID));
            }

            // Get the format string to use to list a requirement
            string requirementFormat = db.GetFriendlyName("MeetsRequirement");
            if (requirementFormat == null)
            {
                // could not find one.  make up one.
                requirementFormat = "?Required? {0}: {1:f0}";
            }
            else
            {
                requirementFormat = ItemAttributes.ConvertFormatString(requirementFormat);
            }

            // Now combine it all with spaces between
            List<string> requirements = new List<string>();
            foreach (KeyValuePair<string, Variable> kvp in req)
            {
                if (TQDebug.ItemDebugLevel > 1)
                {
                    TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "Retrieving requirement {0}={1} (type={2})", kvp.Key, kvp.Value, kvp.Value.GetType().ToString()));
                }

                Variable v = kvp.Value;

                // Format the requirement
                string requirementsText;
                if (v.NumberOfValues > 1 || v.DataType == VariableDataType.StringVar)
                {
                    // reqs should only have 1 entry and should be a number type.  We must punt on this one
                    requirementsText = string.Concat(kvp.Key, ": ", v.ToStringValue());
                }
                else
                {
                    // get the name of this requirement
                    string reqName = db.GetFriendlyName(kvp.Key);
                    if (reqName == null)
                    {
                        reqName = string.Concat("?", kvp.Key, "?");
                    }

                    // Now apply the format string
                    requirementsText = Item.Format(requirementFormat, reqName, v[0]);
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

        /// <summary>
        /// Gets the item's attributes
        /// </summary>
        /// <param name="db">database instance</param>
        /// <param name="filtering">Flag indicating whether or not we are filtering strings</param>
        /// <returns>returns a string containing the item's attributes</returns>
        public string GetAttributes(Database db, bool filtering)
        {
            if (this.attributesString != null)
            {
                return this.attributesString;
            }

            List<string> results = new List<string>();

            // Add the item name
            string itemName = Database.MakeSafeForHtml(this.ToString(db, true, false));
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

                        string type = db.GetFriendlyName(tag1);
                        if (type == null)
                        {
                            type = "?Relic?";
                        }
                        
                        string formatSpec = db.GetFriendlyName(tag2);
                        if (formatSpec == null)
                        {
                            formatSpec = "?{0} - {1} / {2}?";
                        }
                        else
                        {
                            formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
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

                        str = db.GetFriendlyName(tag);
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
                        artifactClass = db.GetFriendlyName(tag);
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
                    string recipe = db.GetFriendlyName(tag);
                    if (recipe == null)
                    {
                        recipe = "Recipe";
                    }

                    results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font><br>", Database.HtmlColor(GetColor(ItemStyle.Mundane)), recipe));

                    tag = "xtagArtifactReagents";

                    // Get Reagents format
                    string formatSpec = db.GetFriendlyName(tag);
                    if (formatSpec == null)
                    {
                        formatSpec = "Required Reagents  ({0}/{1})";
                    }
                    else
                    {
                        formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
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
                string flavor = db.GetFriendlyName(tag);
                if (flavor != null)
                {
                    flavor = Database.MakeSafeForHtml(flavor);
                    Collection<string> flavorTextArray = Database.WrapWords(flavor, 30);

                    for (int i = 0; i < flavorTextArray.Count; ++i)
                    {
                        results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Mundane)), flavorTextArray[i]));
                    }

                    results.Add(string.Empty);
                }
            }

            if (this.baseItemInfo != null)
            {
                this.GetAttributesFromRecord(db, db.GetRecordFromFile(this.baseItemID), filtering, this.baseItemID, results);
            }

            if (this.prefixInfo != null)
            {
                this.GetAttributesFromRecord(db, db.GetRecordFromFile(this.prefixID), filtering, this.prefixID, results);
            }

            if (this.suffixInfo != null)
            {
                this.GetAttributesFromRecord(db, db.GetRecordFromFile(this.suffixID), filtering, this.suffixID, results);
            }

            if (this.relicInfo != null)
            {
                List<string> r = new List<string>();
                this.GetAttributesFromRecord(db, db.GetRecordFromFile(this.relicID), filtering, this.relicID, r);
                if (r.Count > 0)
                {
                    string colorTag = string.Format(CultureInfo.CurrentCulture, "<hr color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Broken)));

                    string relicName = Database.MakeSafeForHtml(this.ToString(db, false, true));
                    
                    // display the relic name
                    results.Add(string.Format(
                        CultureInfo.CurrentUICulture,
                        "{2}<font size=+1 color={0}><b>{1}</b></font>",
                        Database.HtmlColor(Item.GetColor(ItemStyle.Relic)), 
                        relicName, 
                        colorTag));

                    // display the relic subtitle
                    string str;
                    if (this.var1 < this.relicInfo.CompletedRelicLevel)
                    {
                        string tag1 = "tagRelicShard";
                        string tag2 = "tagRelicRatio";
                        if (this.relicInfo.ItemClass.ToUpperInvariant().Equals("ITEMCHARM"))
                        {
                            tag1 = "tagAnimalPart";
                            tag2 = "tagAnimalPartRatio";
                        }

                        string type = db.GetFriendlyName(tag1);
                        if (type == null)
                        {
                            type = "?Relic?";
                        }

                        string formatSpec = db.GetFriendlyName(tag2);
                        if (formatSpec == null)
                        {
                            formatSpec = "?{0} - {1} / {2}?";
                        }
                        else
                        {
                            formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
                        }

                        str = Item.Format(formatSpec, type, Math.Max(1, this.var1), this.relicInfo.CompletedRelicLevel);
                    }
                    else
                    {
                        string tag = "tagRelicComplete";
                        if (this.relicInfo.ItemClass.ToUpperInvariant().Equals("ITEMCHARM"))
                        {
                            tag = "tagAnimalPartComplete";
                        }

                        str = db.GetFriendlyName(tag);
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
            if (this.IsArtifact && this.relicBonusInfo != null)
            {
                List<string> r = new List<string>();
                this.GetAttributesFromRecord(db, db.GetRecordFromFile(this.relicBonusID), filtering, this.relicBonusID, r);
                if (r.Count > 0)
                {
                    string tag = "xtagArtifactBonus";
                    string bonusTitle = db.GetFriendlyName(tag);
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

            if ((this.IsRelic || this.HasRelic) && this.relicBonusInfo != null)
            {
                List<string> r = new List<string>();
                this.GetAttributesFromRecord(db, db.GetRecordFromFile(this.relicBonusID), filtering, this.relicBonusID, r);
                if (r.Count > 0)
                {
                    string tag = "tagRelicBonus";
                    if (this.IsCharm || (this.HasRelic && this.relicInfo.ItemClass.ToUpperInvariant().Equals("ITEMCHARM")))
                    {
                        tag = "tagAnimalPartcompleteBonus";
                    }

                    string bonusTitle = db.GetFriendlyName(tag);
                    if (bonusTitle == null)
                    {
                        bonusTitle = "?Completed Relic/Charm Bonus:?";
                    }

                    string title = Database.MakeSafeForHtml(bonusTitle);

                    results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Relic)), title));
                    results.AddRange(r);
                }
            }

            // Added by VillageIdiot
            // Shows Artifact stats for the formula
            if (this.IsFormulae && this.baseItemInfo != null)
            {
                List<string> r = new List<string>();
                string artifactID = this.baseItemInfo.GetString("artifactName");
                DBRecord artifactRecord = db.GetRecordFromFile(artifactID);
                if (artifactID != null)
                {
                    this.GetAttributesFromRecord(db, artifactRecord, filtering, artifactID, r);
                    if (r.Count > 0)
                    {
                        string tag;

                        // Display the name of the Artifact
                        string artifactClass = db.GetFriendlyName(artifactRecord.GetString("description", 0));
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
                            artifactClass = db.GetFriendlyName(tag);
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
            string itemSeedString = Database.MakeSafeForHtml(string.Format(CultureInfo.CurrentCulture, Resources.ItemSeed, this.seed, (this.seed != 0) ? (this.seed / (float)Int16.MaxValue) : 0.0f));
            results.Add(string.Format(CultureInfo.CurrentCulture, "{2}<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Broken)), itemSeedString, hr1));

            // Add the Immortal Throne clause
            if (this.IsImmortalThrone)
            {
                string immortalThrone = Database.MakeSafeForHtml(Resources.ItemIT);
                results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Rare)), immortalThrone));
            }

            string[] ary = new string[results.Count];
            results.CopyTo(ary);
            this.attributesString = string.Join("<br>", ary);

            return this.attributesString;
        }

        /// <summary>
        /// Added by VillageIdiot
        /// Clears the stored attributes string so that it can be recreated
        /// </summary>
        public void RefreshBareAttributes()
        {
            Array.Clear(this.bareAttributes, 0, this.bareAttributes.Length);
        }

        /// <summary>
        /// Added by VillageIdiot
        /// Shows the bare attributes for properties display
        /// </summary>
        /// <param name="db">database instance</param>
        /// <param name="filtering">flag for filtering strings</param>
        /// <returns>string array containing the bare item attributes</returns>
        public string[] GetBareAttributes(Database db, bool filtering)
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
                        style = string.Concat(db.GetFriendlyName(this.baseItemInfo.StyleTag), " ");
                    }
                }

                if (this.baseItemInfo.QualityTag.Length > 0)
                {
                    quality = string.Concat(db.GetFriendlyName(this.baseItemInfo.QualityTag), " ");
                }

                results.Add(string.Format(
                    CultureInfo.CurrentCulture, 
                    "<font color={0}>{1}</font>",
                    Database.HtmlColor(Item.GetColor(ItemStyle.Rare)),
                    string.Concat(style, quality, db.GetFriendlyName(this.baseItemInfo.DescriptionTag))));

                results.Add(string.Format(
                    CultureInfo.CurrentCulture, 
                    "<font color={0}>{1}</font>",
                    Database.HtmlColor(Item.GetColor(ItemStyle.Relic)), 
                    this.baseItemID));

                this.GetAttributesFromRecord(db, db.GetRecordFromFile(this.baseItemID), filtering, this.baseItemID, results, false);
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
                    db.GetFriendlyName(this.prefixInfo.DescriptionTag)));

                results.Add(string.Format(
                    CultureInfo.CurrentCulture, 
                    "<font color={0}>{1}</font>",
                    Database.HtmlColor(Item.GetColor(ItemStyle.Relic)), 
                    this.prefixID));

                this.GetAttributesFromRecord(db, db.GetRecordFromFile(this.prefixID), filtering, this.prefixID, results, false);
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
                    db.GetFriendlyName(this.suffixInfo.DescriptionTag)));

                results.Add(string.Format(
                    CultureInfo.CurrentCulture, 
                    "<font color={0}>{1}</font>",
                    Database.HtmlColor(Item.GetColor(ItemStyle.Relic)), 
                    this.suffixID));

                this.GetAttributesFromRecord(db, db.GetRecordFromFile(this.suffixID), filtering, this.suffixID, results, false);
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
        /// <param name="db">database instance</param>
        /// <param name="includeName">Flag to include the set name in the returned array</param>
        /// <returns>Returns a string array containing the remaining set items or null if the item is not part of a set.</returns>
        public string[] GetSetItems(Database db, bool includeName)
        {
            if (this.baseItemInfo == null)
            {
                return null;
            }

            string setID = this.baseItemInfo.GetString("itemSetName");
            if (setID == null || setID.Length == 0)
            {
                return null;
            }

            // Get the set record
            DBRecord setRecord = db.GetRecordFromFile(setID);
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
        /// Added by VillageIdiot
        /// Shows the items in a set for the set items
        /// </summary>
        /// <param name="db">database instance</param>
        /// <returns>string containing the set items</returns>
        public string GetSetItemString(Database db)
        {
            if (this.setItemsString != null)
            {
                return this.setItemsString;
            }

            string[] setMembers = this.GetSetItems(db, true);
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
                        name = db.GetFriendlyName(s);
                        results[i++] = string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Rare)), name);
                    }
                    else
                    {
                        Info info = Database.DB.GetInfo(s);
                        name = string.Concat("&nbsp;&nbsp;&nbsp;&nbsp;", db.GetFriendlyName(info.DescriptionTag));
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
            int itemCount = this.stackSize;
            int cx = this.x;
            int cy = this.y;
            int cseed = this.seed;

            if (this.containerType != SackType.Sack && this.containerType != SackType.Player)
            {
                // Equipment, Stashes, Vaults
                if (this.containerType == SackType.Stash)
                {
                    TQData.WriteCString(writer, "stackCount");
                    writer.Write(itemCount - 1);
                }

                TQData.WriteCString(writer, "begin_block");
                writer.Write(this.beginBlockCrap2);

                TQData.WriteCString(writer, "baseName");
                TQData.WriteCString(writer, this.baseItemID);

                TQData.WriteCString(writer, "prefixName");
                TQData.WriteCString(writer, this.prefixID);

                TQData.WriteCString(writer, "suffixName");
                TQData.WriteCString(writer, this.suffixID);

                TQData.WriteCString(writer, "relicName");
                TQData.WriteCString(writer, this.relicID);

                TQData.WriteCString(writer, "relicBonus");
                TQData.WriteCString(writer, this.relicBonusID);

                TQData.WriteCString(writer, "seed");
                writer.Write(cseed);

                TQData.WriteCString(writer, "var1");
                writer.Write(this.var1);

                TQData.WriteCString(writer, "end_block");
                writer.Write(this.endBlockCrap2);

                if (this.containerType == SackType.Stash)
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
                    TQData.WriteCString(writer, this.baseItemID);

                    TQData.WriteCString(writer, "prefixName");
                    TQData.WriteCString(writer, this.prefixID);

                    TQData.WriteCString(writer, "suffixName");
                    TQData.WriteCString(writer, this.suffixID);

                    TQData.WriteCString(writer, "relicName");
                    TQData.WriteCString(writer, this.relicID);

                    TQData.WriteCString(writer, "relicBonus");
                    TQData.WriteCString(writer, this.relicBonusID);

                    TQData.WriteCString(writer, "seed");
                    writer.Write(cseed);

                    TQData.WriteCString(writer, "var1");
                    writer.Write(this.var1);

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
        /// <param name="db">database instance</param>
        /// <param name="reader">BinaryReader instance</param>
        public void Parse(Database db, BinaryReader reader)
        {
            if (this.containerType == SackType.Stash)
            {
                TQData.ValidateNextString("stackCount", reader);
                this.beginBlockCrap1 = reader.ReadInt32();
            }
            else if (this.containerType == SackType.Sack || this.containerType == SackType.Player)
            {
                TQData.ValidateNextString("begin_block", reader); // make sure we just read a new block
                this.beginBlockCrap1 = reader.ReadInt32();
            }

            TQData.ValidateNextString("begin_block", reader); // make sure we just read a new block
            this.beginBlockCrap2 = reader.ReadInt32();

            TQData.ValidateNextString("baseName", reader);
            this.baseItemID = TQData.ReadCString(reader);

            TQData.ValidateNextString("prefixName", reader);
            this.prefixID = TQData.ReadCString(reader);

            TQData.ValidateNextString("suffixName", reader);
            this.suffixID = TQData.ReadCString(reader);

            TQData.ValidateNextString("relicName", reader);
            this.relicID = TQData.ReadCString(reader);

            TQData.ValidateNextString("relicBonus", reader);
            this.relicBonusID = TQData.ReadCString(reader);

            TQData.ValidateNextString("seed", reader);
            this.seed = reader.ReadInt32();

            TQData.ValidateNextString("var1", reader);
            this.var1 = reader.ReadInt32();

            TQData.ValidateNextString("end_block", reader);
            this.endBlockCrap2 = reader.ReadInt32();

            if (this.containerType == SackType.Stash)
            {
                TQData.ValidateNextString("xOffset", reader);
                this.x = Convert.ToInt32(reader.ReadSingle(), CultureInfo.InvariantCulture);

                TQData.ValidateNextString("yOffset", reader);
                this.y = Convert.ToInt32(reader.ReadSingle(), CultureInfo.InvariantCulture);
            }
            else if (this.containerType == SackType.Equipment)
            {
                // Initially set the coordinates to (0, 0)
                this.x = 0;
                this.y = 0;
            }
            else
            {
                TQData.ValidateNextString("pointX", reader);
                this.x = reader.ReadInt32();

                TQData.ValidateNextString("pointY", reader);
                this.y = reader.ReadInt32();

                TQData.ValidateNextString("end_block", reader);
                this.endBlockCrap1 = reader.ReadInt32();
            }

            this.GetDBData(db);

            if (this.containerType == SackType.Stash)
            {
                this.stackSize = this.beginBlockCrap1 + 1;
            }
            else
            {
                this.stackSize = 1;
            }
        }

        /// <summary>
        /// Pulls data out of the TQ item database for this item.
        /// </summary>
        /// <param name="db">database instance</param>
        public void GetDBData(Database db)
        {
            if (TQDebug.ItemDebugLevel > 0)
            {
                TQDebug.DebugWrite(string.Format(CultureInfo.InvariantCulture, "Item.GetDBData ({0})", db));
                TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "   baseItemID = {0}", this.baseItemID));
            }

            this.baseItemID = CheckExtension(this.baseItemID);
            this.baseItemInfo = db.GetInfo(this.baseItemID);

            this.prefixID = CheckExtension(this.prefixID);
            this.suffixID = CheckExtension(this.suffixID); 
            
            if (TQDebug.ItemDebugLevel > 1)
            {
                TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "prefixID = {0}", this.prefixID)); 
                TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "suffixID = {0}", this.suffixID)); 
            }

            this.prefixInfo = db.GetInfo(this.prefixID);
            this.suffixInfo = db.GetInfo(this.suffixID);
            this.relicID = CheckExtension(this.relicID);
            this.relicBonusID = CheckExtension(this.relicBonusID);

            if (TQDebug.ItemDebugLevel > 1) 
            {
                TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "relicID = {0}", this.relicID)); 
                TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "relicBonusID = {0}", this.relicBonusID)); 
            }

            this.relicInfo = db.GetInfo(this.relicID);
            this.relicBonusInfo = db.GetInfo(this.relicBonusID);

            if (TQDebug.ItemDebugLevel > 1) 
            {
                TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "'{0}' baseItemInfo is {1} null", this.ToString(db), (this.baseItemInfo == null) ? string.Empty : "NOT")); 
            }

            // Get the bitmaps we need
            if (this.baseItemInfo != null)
            {
                if (this.IsRelic && !this.IsRelicComplete)
                {
                    this.itemBitmap = db.LoadBitmap(this.baseItemInfo.ShardBitmap);
                    if (TQDebug.ItemDebugLevel > 1) 
                    {
                        TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "Loaded shardbitmap ({0})", this.baseItemInfo.ShardBitmap)); 
                    }
                }
                else
                {
                    this.itemBitmap = db.LoadBitmap(this.baseItemInfo.Bitmap);
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
                this.itemBitmap = db.LoadBitmap("DefaultBitmap");
                if (TQDebug.ItemDebugLevel > 1) 
                {
                    TQDebug.DebugWriteLine("Try loading (DefaultBitmap)"); 
                }
            }

            // Changed by VillageIdiot
            // Moved outside of BaseItemInfo conditional since there are now 2 conditions
            if (this.itemBitmap != null)
            {
                if (TQDebug.ItemDebugLevel > 1)
                {
                    TQDebug.DebugWriteLine(string.Format(
                        CultureInfo.InvariantCulture, 
                        "size = {0}x{1} (unitsize={2})",
                        this.itemBitmap.Width,
                        this.itemBitmap.Height, 
                        db.ItemUnitSize));
                }

                this.itemWidth = Convert.ToInt32((float)this.itemBitmap.Width * Database.DB.Scale / (float)db.ItemUnitSize);
                this.itemHeight = Convert.ToInt32((float)this.itemBitmap.Height * Database.DB.Scale / (float)db.ItemUnitSize);
            }
            else
            {
                if (TQDebug.ItemDebugLevel > 1) 
                {
                    TQDebug.DebugWriteLine("bitmap is null"); 
                }

                this.itemWidth = 1;
                this.itemHeight = 1;
            }

            if (TQDebug.ItemDebugLevel > 0) 
            {
                TQDebug.DebugWriteLine("Exiting Item.GetDBData ()"); 
            }
        }

        /// <summary>
        /// Initializes the random numbers
        /// </summary>
        /// <returns>Random number instance</returns>
        private static Random InitializeRandom()
        {
            return new Random();
        }

        /// <summary>
        /// Holds all of the keys which we are filtering
        /// </summary>
        /// <param name="key">key which we are checking whether or not it gets filtered.</param>
        /// <returns>true if key is present in this list</returns>
        private static bool FilterKey(string key)
        {
            string[] notWanted = 
            {                               
                "maxTransparency",
                "scale",
                "castsShadows",                
                "marketAdjustmentPercent",
                "lootRandomizerCost",
                "lootRandomizerJitter",
                "actorHeight",
                "actorRadius",
                "shadowBias",
                "itemLevel",
                "itemCost",
                "completedRelicLevel",
                "characterBaseAttackSpeed",
                "hideSuffixName",
                "hidePrefixName",
                "amulet",
                "ring",
                "helmet",
                "greaves",
                "armband",
                "bodyArmor",
                "bow",
                "spear",
                "staff",
                "mace",
                "sword",
                "axe",
                "shield",
                "bracelet",
                "amulet",
                "ring",
                "blockAbsorption",
                "itemCostScalePercent", // Added by VillageIdiot
                "itemSkillLevel", // Added by VillageIdiot
                "useDelayTime", // Added by VillageIdiot
                "cameraShakeAmplitude", // Added by VillageIdiot
                "skillMaxLevel", // Added by VillageIdiot
                "skillCooldownTime", // Added by VillageIdiot
                "expansionTime", // Added by VillageIdiot
                "skillTier", // Added by VillageIdiot
                "cameraShakeDurationSecs", // Added by VillageIdiot
                "skillUltimateLevel", // Added by VillageIdiot
                "skillConnectionSpacing", // Added by VillageIdiot
                "petBurstSpawn", // Added by VillageIdiot
                "petLimit", // Added by VillageIdiot
                "isPetDisplayable", // Added by VillageIdiot
                "spawnObjectsTimeToLive", // Added by VillageIdiot
                "skillProjectileNumber", // Added by VillageIdiot
                "skillMasteryLevelRequired", // Added by VillageIdiot
                "excludeRacialDamage", // Added by VillageIdiot
                "skillWeaponTintRed", // Added by VillageIdiot
                "skillWeaponTintGreen", // Added by VillageIdiot
                "skillWeaponTintBlue", // Added by VillageIdiot
                "debufSkill", // Added by VillageIdiot
                "hideFromUI", // Added by VillageIdiot
                "instantCast", // Added by VillageIdiot
                "waveEndWidth", // Added by VillageIdiot
                "waveDistance",  // Added by VillageIdiot
                "waveDepth", // Added by VillageIdiot
                "waveStartWidth", // Added by VillageIdiot
                "ragDollAmplification", // Added by VillageIdiot
                "waveTime", // Added by VillageIdiot
                "sparkGap", // Added by VillageIdiot
                "sparkChance", // Added by VillageIdiot
                "projectileUsesAllDamage", // Added by VillageIdiot
                "dropOffset", // Added by VillageIdiot
                "dropHeight", // Added by VillageIdiot
                "numProjectiles", // Added by VillageIdiot
                "Sword", // Added by VillageIdiot
                "Axe", // Added by VillageIdiot
                "Spear", // Added by VillageIdiot
                "Mace", // Added by VillageIdiot
                "quest", // Added by VillageIdiot
                "cannotPickUpMultiple" // Added by VillageIdiot
            };

            if (Array.IndexOf(notWanted, key) != -1)
            {
                return true;
            }

            if (key.EndsWith("Sound", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (key.EndsWith("Mesh", StringComparison.OrdinalIgnoreCase))
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
                "levelRequirement",                                
                "intelligenceRequirement",
                "dexterityRequirement",
                "strengthRequirement",
            };

            if (Array.IndexOf(notWanted, key) != -1)
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
            string classification = itemClass.ToUpperInvariant();

            if (classification.Equals("ARMORPROTECTIVE_HEAD"))
            {
                return "head";
            }

            if (classification.Equals("ARMORPROTECTIVE_FOREARM"))
            {
                return "forearm";
            }

            if (classification.Equals("ARMORPROTECTIVE_LOWERBODY"))
            {
                return "lowerBody";
            }

            if (classification.Equals("ARMORPROTECTIVE_UPPERBODY"))
            {
                return "upperBody";
            }

            if (classification.Equals("ARMORJEWELRY_BRACELET"))
            {
                return "bracelet";
            }

            if (classification.Equals("ARMORJEWELRY_RING"))
            {
                return "ring";
            }

            if (classification.Equals("ARMORJEWELRY_AMULET"))
            {
                return "amulet";
            }

            if (classification.Equals("WEAPONHUNTING_BOW"))
            {
                return "bow";
            }

            if (classification.Equals("WEAPONHUNTING_SPEAR"))
            {
                return "spear";
            }

            if (classification.Equals("WEAPONMELEE_AXE"))
            {
                return "axe";
            }

            if (classification.Equals("WEAPONMELEE_SWORD"))
            {
                return "sword";
            }

            if (classification.Equals("WEAPONMELEE_MACE"))
            {
                return "mace";
            }

            if (classification.Equals("WEAPONMAGICAL_STAFF"))
            {
                return "staff";
            }

            if (classification.Equals("WEAPONARMOR_SHIELD"))
            {
                return "shield";
            }

            return "none";
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
        /// Added by VillageIdiot
        /// Sometimes the .dbr extension is not written into the item
        /// Checks to see if the id ends with .dbr and adds it if not.
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
        /// Added by VillageIdiot
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
        /// <param name="db">database instance</param>
        /// <param name="record">database record</param>
        private static void GetRequirementsFromRecord(SortedList<string, Variable> requirements, Database db, DBRecord record)
        {
            if (TQDebug.ItemDebugLevel > 0)
            {
                TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "Item.GetDynamicRequirementsFromRecord({0}, {1}, {2})", requirements, db, record));
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

            foreach (Variable v in record.VariableCollection)
            {
                if (TQDebug.ItemDebugLevel > 2)
                {
                    TQDebug.DebugWriteLine(v.Name);
                }

                if (FilterValue(v, false))
                {
                    continue;
                }

                if (!FilterRequirements(v.Name))
                {
                    continue;
                }

                string value = v.ToStringValue();
                string key = v.Name.Replace("Requirement", string.Empty);

                // Upper-case the first char of key
                key = string.Concat(key.Substring(0, 1).ToUpper(System.Globalization.CultureInfo.InvariantCulture), key.Substring(1));

                // Level needs to be LevelRequirement bah
                if (key.Equals("Level"))
                {
                    key = "LevelRequirement";
                }

                if (requirements.ContainsKey(key))
                {
                    Variable oldV = (Variable)requirements[key];

                    // Changed by VillageIdiot
                    // Comparison was failing when level difference was too high
                    // (single digit vs multi-digit)
                    if (value.Contains(",") || oldV.Name.Contains(","))
                    {
                        // Just in case there is something with multiple values
                        // Keep the original code
                        if (string.Compare(value, oldV.ToStringValue(), StringComparison.OrdinalIgnoreCase) <= 0)
                        {
                            continue;
                        }

                        requirements.Remove(key);
                    }
                    else
                    {
                        if (v.GetInt32(0) <= oldV.GetInt32(0))
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

                requirements.Add(key, v);
            }

            if (TQDebug.ItemDebugLevel > 0)
            {
                TQDebug.DebugWriteLine("Exiting Item.GetDynamicRequirementsFromRecord()");
            }
        }

        /// <summary>
        /// Get + to a Mastery string
        /// </summary>
        /// <param name="db">database instance</param>
        /// <param name="record">DBRecord database record</param>
        /// <param name="v">variable structure</param>
        /// <param name="d">ItemAttributesData structure</param>
        /// <param name="font">display font string</param>
        /// <returns>formatted string with the + to mastery</returns>
        private static string GetAugmentMasteryLevel(Database db, DBRecord record, Variable v, ItemAttributesData d, ref string font)
        {
            string num = d.FullAttribute.Substring(19, 1);
            string skillRecordKey = string.Concat("augmentMasteryName", num);
            string skillRecordID = record.GetString(skillRecordKey, 0);
            if (skillRecordID == null)
            {
                skillRecordID = skillRecordKey;
            }

            string skillName = null;
            DBRecord skillRecord = db.GetRecordFromFile(skillRecordID);
            if (skillRecord != null)
            {
                string nameTag = skillRecord.GetString("skillDisplayName", 0);
                if (nameTag != null)
                {
                    skillName = db.GetFriendlyName(nameTag);
                }
            }

            if (skillName == null)
            {
                skillName = Path.GetFileNameWithoutExtension(skillRecordID);
                font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Legendary)));
            }

            // now get the formatSpec
            string formatSpec = db.GetFriendlyName("ItemMasteryIncrement");
            if (formatSpec == null)
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

                formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
                if (font == null)
                {
                    font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Epic)));
                }
            }

            return Item.Format(formatSpec, v[0], skillName);
        }

        /// <summary>
        /// Gets the + to all skills string
        /// </summary>
        /// <param name="db">database instance</param>
        /// <param name="varNum">offset number of the variable value that we are using</param>
        /// <param name="v">variable structure</param>
        /// <param name="font">display font string</param>
        /// <returns>formatted string for + to all skills</returns>
        private static string GetAugmentAllLevel(Database db, int varNum, Variable v, ref string font)
        {
            string tag = "ItemAllSkillIncrement";
            string formatSpec = db.GetFriendlyName(tag);
            if (formatSpec == null)
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

                formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
                font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Epic)));
            }

            return Item.Format(formatSpec, v[System.Math.Min(v.NumberOfValues - 1, varNum)]);
        }

        /// <summary>
        /// Gets the formatted racial bonus string(s)
        /// </summary>
        /// <param name="db">database instance</param>
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
        private static string GetRacialBonus(Database db, DBRecord record, List<string> results, int varNum, bool isGlobal, string globalIndent, Variable v, ItemAttributesData d, string line, ref string font)
        {
            // Added by VillageIdiot
            // Updated to accept multiple racial bonuses in record             
            string[] race = record.GetAllStrings("racialBonusRace");
            if (race != null)
            {
                for (int j = 0; j < race.Length; ++j)
                {
                    string finalRace = db.GetFriendlyName(race[j]);
                    if (finalRace == null)
                    {
                        // Try to look up plural
                        race[j] = string.Concat(race[j], "s");
                        finalRace = db.GetFriendlyName(race[j]);
                    }

                    // If not plural, then use original
                    if (finalRace == null)
                    {
                        finalRace = race[j].Remove(race[j].Length - 1);
                    }

                    string formatTag = string.Concat(d.FullAttribute.Substring(0, 1).ToUpperInvariant(), d.FullAttribute.Substring(1));
                    string formatSpec = db.GetFriendlyName(formatTag);
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

                        formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
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
        /// <param name="db">database instance</param>
        /// <param name="attributeList">Arraylist containing the attributes</param>
        /// <param name="varNum">offset number of the variable value that we are using</param>
        /// <param name="v">variable structure</param>
        /// <param name="font">font string</param>
        /// <returns>formatted global chance string</returns>
        private static string GetGlobalChance(Database db, List<Variable> attributeList, int varNum, Variable v, ref string font)
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

                string formatSpec = db.GetFriendlyName(tag);
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

                    formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
                    font = String.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Epic)));
                }

                line = Item.Format(formatSpec, v[System.Math.Min(v.NumberOfValues - 1, varNum)]);
            }

            return line;
        }

        /// <summary>
        /// Gets a formatted chance modifier string
        /// </summary>
        /// <param name="db">database instance</param>
        /// <param name="varNum">offset number of the variable value that we are using</param>
        /// <param name="modifierChanceVar">Chance modifier variable</param>
        /// <returns>formatted chance modifier string</returns>
        private static string GetChanceModifier(Database db, int varNum, Variable modifierChanceVar)
        {
            string modifierChance = null;
            string color = null;
            string formatSpec = db.GetFriendlyName("ChanceOfTag");
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

                formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
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
        /// <param name="db">database instance</param>
        /// <param name="varNum">offset number of the variable value that we are using</param>
        /// <param name="durationModifierVar">duration modifier variable</param>
        /// <returns>formatted duration modifier string</returns>
        private static string GetDurationModifier(Database db, int varNum, Variable durationModifierVar)
        {
            string durationModifier = null;
            string color = null;
            string formatSpec = db.GetFriendlyName("ImprovedTimeFormat");
            if (formatSpec == null)
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

                formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
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
        /// <param name="db">database instance</param>
        /// <param name="data">ItemAttributesData for the attribute</param>
        /// <param name="varNum">offset number of the variable value that we are using</param>
        /// <param name="modifierData">ItemAttributesData for the modifier</param>
        /// <param name="modifierVar">modifier variable</param>
        /// <returns>formatted modifier string</returns>
        private static string GetModifier(Database db, ItemAttributesData data, int varNum, ItemAttributesData modifierData, Variable modifierVar)
        {
            string modifier = null;
            string color = null;
            string formatSpec = null;
            string tag = ItemAttributes.GetAttributeTextTag(data);
            if (tag == null)
            {
                formatSpec = string.Concat("{0:f1}% ?", modifierData.FullAttribute, "?");
                color = Database.HtmlColor(Item.GetColor(ItemStyle.Legendary));
            }
            else
            {
                formatSpec = db.GetFriendlyName(tag);
                if (formatSpec == null)
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

                    formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
                }
            }

            modifier = Item.Format(formatSpec, modifierVar[Math.Min(modifierVar.NumberOfValues - 1, varNum)]);
            modifier = Database.MakeSafeForHtml(modifier);
            if (color != null)
            {
                modifier = Item.Format("<font color={0}>{1}</font>", color, modifier);
            }

            return modifier;
        }

        /// <summary>
        /// Gets formatted chance string
        /// </summary>
        /// <param name="db">database instance</param>
        /// <param name="varNum">offset number of the variable value that we are using</param>
        /// <param name="chanceVar">chance variable</param>
        /// <returns>formatted chance string.</returns>
        private static string GetChance(Database db, int varNum, Variable chanceVar)
        {
            string chance = null;
            string color = null;
            string formatSpec = db.GetFriendlyName("ChanceOfTag");
            if (formatSpec == null)
            {
                formatSpec = "?{%.1f0}% Chance of?";
                color = Database.HtmlColor(Item.GetColor(ItemStyle.Legendary));
            }

            if (TQDebug.ItemDebugLevel > 2)
            {
                TQDebug.DebugWriteLine("Item.formatspec (chance) = " + formatSpec);
            }

            formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
            if (chanceVar != null)
            {
                chance = Item.Format(formatSpec, chanceVar[Math.Min(chanceVar.NumberOfValues - 1, varNum)]);
                chance = Database.MakeSafeForHtml(chance);
                if (color != null)
                {
                    chance = Item.Format("<font color={0}>{1}</font>", color, chance);
                }
            }

            return chance;
        }

        /// <summary>
        /// Gets the formatted damage ratio string
        /// </summary>
        /// <param name="db">database instance</param>
        /// <param name="varNum">offset number of the variable value that we are using</param>
        /// <param name="damageRatioData">ItemAttributesData for the damage ratio</param>
        /// <param name="damageRatioVar">Damage Ratio variable</param>
        /// <returns>formatted damage ratio string</returns>
        private static string GetDamageRatio(Database db, int varNum, ItemAttributesData damageRatioData, Variable damageRatioVar)
        {
            string damageRatio = null;
            string color = null;
            string formatSpec = null;

            string tag = string.Concat("Damage", damageRatioData.FullAttribute.Substring(9, damageRatioData.FullAttribute.Length - 20), "Ratio");
            formatSpec = db.GetFriendlyName(tag);

            if (formatSpec == null)
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

                formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
            }

            damageRatio = Item.Format(formatSpec, damageRatioVar[Math.Min(damageRatioVar.NumberOfValues - 1, varNum)]);
            damageRatio = Database.MakeSafeForHtml(damageRatio);
            if (color != null)
            {
                damageRatio = Item.Format("<font color={0}>{1}</font>", color, damageRatio);
            }

            return damageRatio;
        }

        /// <summary>
        /// Gets the formatted duration single value
        /// </summary>
        /// <param name="db">database instance</param>
        /// <param name="varNum">offset number of the variable value that we are using</param>
        /// <param name="minDurVar">minimum duration variable</param>
        /// <param name="maxDurVar">maximum duration variable</param>
        /// <returns>formatted duration string</returns>
        private static string GetDurationSingle(Database db, int varNum, Variable minDurVar, Variable maxDurVar)
        {
            string duration = null;
            string color = null;
            string formatSpec = db.GetFriendlyName("DamageFixedSingleFormatTime");
            if (formatSpec == null)
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

                formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
            }

            Variable v = minDurVar;
            if (v == null)
            {
                v = maxDurVar;
            }

            if (v != null)
            {
                duration = Item.Format(formatSpec, v[Math.Min(v.NumberOfValues - 1, varNum)]);
                duration = Database.MakeSafeForHtml(duration);
                if (color != null)
                {
                    duration = Item.Format("<font color={0}>{1}</font>", color, duration);
                }
            }

            return duration;
        }

        /// <summary>
        /// Gets the formatted duration range values
        /// </summary>
        /// <param name="db">database instance</param>
        /// <param name="varNum">offset number of the variable value that we are using</param>
        /// <param name="minDurVar">minimum duration variable</param>
        /// <param name="maxDurVar">maximum duration variable</param>
        /// <returns>formatted duration string</returns>
        private static string GetDurationRange(Database db, int varNum, Variable minDurVar, Variable maxDurVar)
        {
            string duration = null;
            string color = null;
            string formatSpec = db.GetFriendlyName("DamageFixedRangeFormatTime");
            if (formatSpec == null)
            {
                formatSpec = "over {0}..{1} seconds";
                color = Database.HtmlColor(Item.GetColor(ItemStyle.Legendary));
            }
            else
            {
                if (TQDebug.ItemDebugLevel > 2)
                {
                    TQDebug.DebugWriteLine("Item.formatspec (time range) = " + formatSpec);
                }

                formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
            }

            duration = Item.Format(formatSpec, minDurVar[Math.Min(minDurVar.NumberOfValues - 1, varNum)], maxDurVar[Math.Min(maxDurVar.NumberOfValues - 1, varNum)]);
            duration = Database.MakeSafeForHtml(duration);
            if (color != null)
            {
                duration = Item.Format("<font color={0}>{1}</font>", color, duration);
            }

            return duration;
        }

        /// <summary>
        /// Gets a formatted + to skill string
        /// </summary>
        /// <param name="db">database instance</param>
        /// <param name="record">DBRecord database record</param>
        /// <param name="v">variable structure</param>
        /// <param name="d">ItemAttributesData structure</param>
        /// <param name="line">line of text</param>
        /// <param name="font">display font string</param>
        /// <returns>formatted string containing + to skill</returns>
        private static string GetAugmentSkillLevel(Database db, DBRecord record, Variable v, ItemAttributesData d, string line, ref string font)
        {
            string num = d.FullAttribute.Substring(17, 1);
            string skillRecordKey = string.Concat("augmentSkillName", num);
            string skillRecordID = record.GetString(skillRecordKey, 0);
            if (skillRecordID != null)
            {
                string skillName = null;
                string nameTag = null;
                DBRecord skillRecord = db.GetRecordFromFile(skillRecordID);
                if (skillRecord != null)
                {
                    // Changed by VillageIdiot
                    // for augmenting buff skills
                    string buffSkillName = skillRecord.GetString("buffSkillName", 0);
                    if (string.IsNullOrEmpty(buffSkillName))
                    {
                        // Not a buff so look up the name
                        nameTag = skillRecord.GetString("skillDisplayName", 0);
                        if (nameTag.Length != 0)
                        {
                            skillName = db.GetFriendlyName(nameTag);
                        }
                        else
                        {
                            // Added by VillageIdiot
                            // Check to see if this is a pet skill
                            nameTag = skillRecord.GetString("Class", 0);
                            if (nameTag.Contains("PetModifier"))
                            {
                                string petSkillID = skillRecord.GetString("petSkillName", 0);
                                DBRecord petSkillRecord = db.GetRecordFromFile(petSkillID);
                                if (petSkillRecord != null)
                                {
                                    string petNameTag = petSkillRecord.GetString("skillDisplayName", 0);
                                    if (petNameTag.Length != 0)
                                    {
                                        skillName = db.GetFriendlyName(petNameTag);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // This is a buff skill
                        DBRecord buffSkillRecord = db.GetRecordFromFile(buffSkillName);
                        if (buffSkillRecord != null)
                        {
                            nameTag = buffSkillRecord.GetString("skillDisplayName", 0);
                            if (nameTag.Length != 0)
                            {
                                skillName = db.GetFriendlyName(nameTag);
                            }
                        }
                    }
                }

                if (skillName == null)
                {
                    skillName = Path.GetFileNameWithoutExtension(skillRecordID);
                }

                // now get the formatSpec
                string formatSpec = db.GetFriendlyName("ItemSkillIncrement");
                if (formatSpec == null)
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

                    formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
                    font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Epic)));
                }

                line = Item.Format(formatSpec, v[0], skillName);
            }

            return line;
        }

        /// <summary>
        /// Gets the formatted formulae string(s)
        /// </summary>
        /// <param name="db">database instance</param>
        /// <param name="results">results list</param>
        /// <param name="v">variable structure</param>
        /// <param name="d">ItemAttributesData structure</param>
        /// <param name="line">line of text</param>
        /// <param name="font">display font string</param>
        /// <returns>formatted formulae string</returns>
        private static string GetFormulae(Database db, List<string> results, Variable v, ItemAttributesData d, string line, ref string font)
        {
            // Special case for formulae reagents
            if (d.FullAttribute.StartsWith("reagent", StringComparison.OrdinalIgnoreCase))
            {
                DBRecord reagentRecord = db.GetRecordFromFile(v.GetString(0));
                if (reagentRecord != null)
                {
                    string nameTag = reagentRecord.GetString("description", 0);
                    if (nameTag != null)
                    {
                        string reagentName = db.GetFriendlyName(nameTag);
                        string formatSpec = "{0}";
                        font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Common)));
                        line = Item.Format(formatSpec, reagentName);
                    }
                }
            }
            else if (d.FullAttribute.Equals("artifactCreationCost"))
            {
                string formatSpec = db.GetFriendlyName("xtagArtifactCost");
                if (TQDebug.ItemDebugLevel > 2)
                {
                    TQDebug.DebugWriteLine("Item.formatspec (Artifact cost) = " + formatSpec);
                }

                formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
                font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Rare)));
                results.Add(string.Empty);
                line = Item.Format(formatSpec, string.Format(CultureInfo.CurrentCulture, "{0:N0}", v[0]));
            }

            return line;
        }

        /// <summary>
        /// Gets a formatted string of the granted skill
        /// </summary>
        /// <param name="db">database instance</param>
        /// <param name="record">DBRecord database record</param>
        /// <param name="results">results list</param>
        /// <param name="v">variable structure</param>
        /// <param name="line">line of text</param>
        /// <param name="font">display font string</param>
        /// <returns>formatted granted skill string.</returns>
        private static string GetGrantedSkill(Database db, DBRecord record, List<string> results, Variable v, string line, ref string font)
        {
            // Added by VillageIdiot
            // Special case for granted skills
            DBRecord skillRecord = db.GetRecordFromFile(v.GetString(0));
            if (skillRecord != null)
            {
                // Add a blank line and then the Grants Skill text
                results.Add(string.Empty);
                font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Mundane)));
                string skillTag = db.GetFriendlyName("tagItemGrantSkill");
                if (skillTag.Length == 0)
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
                    if (nameTag.Length != 0)
                    {
                        skillName = db.GetFriendlyName(nameTag);

                        if (skillName == null)
                        {
                            skillName = Path.GetFileNameWithoutExtension(v.GetString(0));
                        }
                    }
                }
                else
                {
                    // This is a buff skill
                    DBRecord buffSkillRecord = db.GetRecordFromFile(buffSkillName);
                    if (buffSkillRecord != null)
                    {
                        nameTag = buffSkillRecord.GetString("skillDisplayName", 0);
                        if (nameTag.Length != 0)
                        {
                            skillName = db.GetFriendlyName(nameTag);

                            if (skillName == null)
                            {
                                skillName = Path.GetFileNameWithoutExtension(v.GetString(0));
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
                    DBRecord autoControllerRecord = db.GetRecordFromFile(autoController);
                    if (autoControllerRecord != null)
                    {
                        triggerType = autoControllerRecord.GetString("triggerType", 0);
                    }
                }

                // Convert TriggerType into text tag
                if (triggerType != null)
                {
                    if (triggerType == "LowHealth")
                    {
                        // Activated on low health
                        activationTag = "xtagAutoSkillCondition01";
                    }
                    else if (triggerType == "LowMana")
                    {
                        // Activated on low energy
                        activationTag = "xtagAutoSkillCondition02";
                    }
                    else if (triggerType == "HitByEnemy")
                    {
                        // Activated upon taking damage
                        activationTag = "xtagAutoSkillCondition03";
                    }
                    else if (triggerType == "HitByMelee")
                    {
                        // Activated upon taking melee damage
                        activationTag = "xtagAutoSkillCondition04";
                    }
                    else if (triggerType == "HitByProjectile")
                    {
                        // Activated upon taking ranged damage
                        activationTag = "xtagAutoSkillCondition05";
                    }
                    else if (triggerType == "CastBuff")
                    {
                        // Activated upon casting a buff
                        activationTag = "xtagAutoSkillCondition06";
                    }
                    else if (triggerType == "AttackEnemy")
                    {
                        // Activated on attack
                        activationTag = "xtagAutoSkillCondition07";
                    }
                    else if (triggerType == "OnEquip")
                    {
                        // Activated when equipped
                        activationTag = "xtagAutoSkillCondition08";
                    }
                }

                if (activationTag != null)
                {
                    activationText = db.GetFriendlyName(activationTag);
                }
                else
                {
                    activationText = string.Empty;
                }

                if (activationText.Length == 0)
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
        /// <param name="db">databse instance</param>
        /// <param name="font">display font string</param>
        /// <returns>formatted pet bonus name</returns>
        private static string GetPetBonusName(Database db, ref string font)
        {
            string tag = "xtagPetBonusNameAllPets";
            string formatSpec = db.GetFriendlyName(tag);
            if (formatSpec == null)
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

                formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
                font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Relic)));
            }

            return formatSpec;
        }

        /// <summary>
        /// Gets the skill effects string
        /// </summary>
        /// <param name="db">database instance</param>
        /// <param name="data">ItemAttributesData structure of the base attribute</param>
        /// <param name="varNum">offset number of the variable value that we are using</param>
        /// <param name="v">variable structure</param>
        /// <param name="d">ItemAttributesData structure of the current attribute</param>
        /// <param name="line">line of text</param>
        /// <param name="font">display font string</param>
        /// <returns>formatted skill effect string</returns>
        private static string GetSkillEffect(Database db, ItemAttributesData data, int varNum, Variable v, ItemAttributesData d, string line, ref string font)
        {
            string labelTag = ItemAttributes.GetAttributeTextTag(data);
            if (labelTag == null)
            {
                labelTag = string.Concat("?", data.FullAttribute, "?");
                font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Legendary)));
            }

            string label = db.GetFriendlyName(labelTag);
            if (label == null)
            {
                label = string.Concat("?", labelTag, "?");
                font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Legendary)));
            }

            if (TQDebug.ItemDebugLevel > 2)
            {
                TQDebug.DebugWriteLine("Item.label (scroll) = " + label);
            }

            label = ItemAttributes.ConvertFormatString(label);

            // Find the extra format tag for those that take 2 parameters.
            string formatSpecTag = null;
            string formatSpec = null;
            if (d.FullAttribute.EndsWith("Cost", StringComparison.OrdinalIgnoreCase))
            {
                formatSpecTag = "SkillIntFormat";
            }
            else if (d.FullAttribute.EndsWith("Duration", StringComparison.OrdinalIgnoreCase))
            {
                formatSpecTag = "SkillSecondFormat";
            }
            else if (d.FullAttribute.EndsWith("Radius", StringComparison.OrdinalIgnoreCase))
            {
                formatSpecTag = "SkillDistanceFormat";
            }

            if (!string.IsNullOrEmpty(formatSpecTag))
            {
                formatSpec = db.GetFriendlyName(formatSpecTag);

                if (formatSpec == null)
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

                    formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
                    font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Epic)));
                }
            }

            if (string.IsNullOrEmpty(formatSpecTag))
            {
                font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Epic)));
                line = Item.Format(label, v[Math.Min(v.NumberOfValues - 1, varNum)]);
            }
            else
            {
                line = Item.Format(formatSpec, v[Math.Min(v.NumberOfValues - 1, varNum)], label);
            }

            return line;
        }

        /// <summary>
        /// Gets a raw attribute string
        /// </summary>
        /// <param name="db">database instance</param>
        /// <param name="data">ItemAttributesData structure</param>
        /// <param name="varNum">offset number of the variable value that we are using</param>
        /// <param name="v">variable structure</param>
        /// <param name="font">display font string</param>
        /// <returns>formatted raw attribute string</returns>
        private static string GetRawAttribute(Database db, ItemAttributesData data, int varNum, Variable v, ref string font)
        {
            string line = null;
            string labelTag = ItemAttributes.GetAttributeTextTag(data);
            if (labelTag == null)
            {
                labelTag = string.Concat("?", data.FullAttribute, "?");
                font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Legendary)));
            }

            string label = db.GetFriendlyName(labelTag);
            if (label == null)
            {
                label = string.Concat("?", labelTag, "?");
                font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Legendary)));
            }

            label = ItemAttributes.ConvertFormatString(label);
            if (label.IndexOf('{') >= 0)
            {
                // we have a format string.  try using it.
                line = Item.Format(label, v[Math.Min(v.NumberOfValues - 1, varNum)]);
                if (font == null)
                {
                    font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Epic)));
                }
            }
            else
            {
                // no format string.
                line = Database.DB.VariableToStringNice(v);
            }

            if (font == null)
            {
                font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Legendary))); // make these unknowns stand out
            }

            return line;
        }

        /// <summary>
        /// Gets the dynamic requirements from a database record.
        /// </summary>
        /// <param name="requirements">SortedList of requirements</param>
        /// <param name="db">database instance</param>
        /// <param name="itemInfo">ItemInfo for the item</param>
        private void GetDynamicRequirementsFromRecord(SortedList<string, Variable> requirements, Database db, Info itemInfo)
        {
            if (TQDebug.ItemDebugLevel > 0)
            {
                TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "Item.GetDynamicRequirementsFromRecord({0}, {1}, {2})", requirements, db, itemInfo));
            }

            DBRecord record = db.GetRecordFromFile(itemInfo.ItemId);
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

            record = db.GetRecordFromFile(itemCostID);
            if (record == null)
            {
                return;
            }

            if (TQDebug.ItemDebugLevel > 1)
            {
                TQDebug.DebugWriteLine(record.Id);
            }

            string prefix = GetRequirementEquationPrefix(itemInfo.ItemClass);
            foreach (Variable v in record.VariableCollection)
            {
                if (string.Compare(v.Name, 0, prefix, 0, prefix.Length, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    continue;
                }

                if (TQDebug.ItemDebugLevel > 2)
                {
                    TQDebug.DebugWriteLine(v.Name);
                }

                if (FilterValue(v, true))
                {
                    // our equation is a string, so we want also strings
                    return;
                }

                string key = v.Name.Replace(prefix, string.Empty);
                key = key.Replace("Equation", string.Empty);
                key = key.Replace(key[0], char.ToUpperInvariant(key[0]));

                // Level needs to be LevelRequirement bah
                if (key.Equals("Level"))
                {
                    key = "LevelRequirement";
                }

                // We need to ignore the cost equations.
                // Shields have costs so they will cause an overflow.
                if (key.Equals("Cost"))
                {
                    continue;
                }

                string value = v.ToStringValue().Replace(itemLevelTag, itemLevel);

                // Added by VillageIdiot
                // Changed to reflect Total Attribut count
                value = value.Replace("totalAttCount", Convert.ToString(this.attributeCount, CultureInfo.InvariantCulture));

                Expression expression = ExpressionEvaluate.CreateExpression(value);

                // Changed by Bman to fix random overflow crashes
                Variable ans = new Variable(string.Empty, VariableDataType.Integer, 1);

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
        /// Changed by VillageIdiot
        /// Added option to NOT translate the attributes to strings
        /// </summary>
        /// <param name="db">database instance</param>
        /// <param name="record">DBRecord for the database record</param>
        /// <param name="filtering">whether or not we are filtering strings</param>
        /// <param name="recordId">string containing the database record id</param>
        /// <param name="results">List for the results</param>
        /// <param name="convertStrings">flag on whether we convert attributes to strings.</param>
        private void GetAttributesFromRecord(Database db, DBRecord record, bool filtering, string recordId, List<string> results, bool convertStrings = true)
        {
            if (TQDebug.ItemDebugLevel > 0)
            {
                TQDebug.DebugWriteLine(string.Format(
                    CultureInfo.InvariantCulture,
                    "Item.GetAttributesFromRecord({0}, {1}, {2}, {3}, {4}, {5})",
                    db,
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

            foreach (Variable variable in record.VariableCollection)
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
                if (recordId != this.relicID && recordId != this.relicBonusID && !this.isAttributeCounted)
                {
                    // Added test to see if this has already been done
                    if (!countedGroups.Contains(effectGroup))
                    {
                        if (!variable.Name.Contains("Chance") && !variable.Name.Contains("Duration"))
                        {
                            // Filter Attribute chance and duration tags
                            // Filter base attributes
                            if (variable.Name != "characterBaseAttackSpeedTag" &&
                                variable.Name != "offensivePhysicalMin" &&
                                variable.Name != "offensivePhysicalMax" &&
                                variable.Name != "defensiveProtection" &&
                                variable.Name != "defensiveBlock" &&
                                variable.Name != "blockRecoveryTime" &&
                                variable.Name != "offensiveGlobalChance" &&
                                variable.Name != "retaliationGlobalChance" &&
                                variable.Name != "offensivePierceRatioMin")
                            {
                                // Chance of effects are still messed up.
                                if (variable.Name.StartsWith("augmentSkillLevel", StringComparison.OrdinalIgnoreCase))
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
                    this.ConvertAttributeListToString(db, record, attrList, recordId, results);
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
        /// <param name="db">database instance</param>
        /// <param name="record">DBRecord of the database record</param>
        /// <param name="attributeList">ArrayList containing the attribute list</param>
        /// <param name="data">ItemAttributesData for the item</param>
        /// <param name="recordId">string containing the record id</param>
        /// <param name="results">List containing the results</param>
        private void ConvertOffenseAttributesToString(Database db, DBRecord record, List<Variable> attributeList, ItemAttributesData data, string recordId, List<string> results)
        {
            if (TQDebug.ItemDebugLevel > 0)
            {
                TQDebug.DebugWriteLine(string.Format(
                    CultureInfo.InvariantCulture,
                    "Item.ConvertOffenseAttrToString({0}, {1}, {2}, {3}, {4}, {5})",
                    db,
                    record,
                    attributeList,
                    data,
                    recordId,
                    results));
            }

            // If we are a relic, then sometimes there are multiple values per variable depending on how many pieces we have.
            // let's determine which var we want in these cases
            int varNum = 0;
            if (this.IsRelic && recordId == this.baseItemID)
            {
                varNum = this.Number - 1;
            }
            else if (this.HasRelic && recordId == this.relicID)
            {
                varNum = Math.Max(this.var1, 1) - 1;
            }

            // Pet skills can also have multiple values so we attempt to decode it here
            if (this.IsScroll || this.IsRelic)
            {
                varNum = this.GetPetSkillLevel(db, record, recordId, varNum);
            }

            // Triggered skills can have also multiple values so we need to decode it here
            if (record.GetString("Class", 0).ToUpperInvariant().StartsWith("SKILL", StringComparison.OrdinalIgnoreCase))
            {
                varNum = this.GetTriggeredSkillLevel(db, record, recordId, varNum);
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

            for (int i = 0; i < attributeList.Count; ++i)
            {
                Variable v = (Variable)attributeList[i];
                if (v == null)
                {
                    continue;
                }

                ItemAttributesData d = ItemAttributes.GetAttributeData(v.Name);
                if (d == null)
                {
                    // unknown attribute
                    d = new ItemAttributesData(ItemAttributesEffectType.Other, v.Name, v.Name, string.Empty, 0);
                }

                if (d.Variable.Equals("Min"))
                {
                    minData = d;
                    minVar = v;
                }
                else if (d.Variable.Equals("Max"))
                {
                    maxData = d;
                    maxVar = v;
                }
                else if (d.Variable.Equals("DurationMin"))
                {
                    minDurData = d;
                    minDurVar = v;
                }
                else if (d.Variable.Equals("DurationMax"))
                {
                    maxDurData = d;
                    maxDurVar = v;
                }
                else if (d.Variable.Equals("Chance"))
                {
                    chanceData = d;
                    chanceVar = v;
                }
                else if (d.Variable.Equals("Modifier"))
                {
                    modifierData = d;
                    modifierVar = v;
                }
                else if (d.Variable.Equals("ModifierChance"))
                {
                    modifierChanceData = d;
                    modifierChanceVar = v;
                }
                else if (d.Variable.Equals("DurationModifier"))
                {
                    durationModifierData = d;
                    durationModifierVar = v;
                }
                else if (d.Variable.Equals("DrainMin"))
                {
                    // Added by VillageIdiot
                    minData = d;
                    minVar = v;
                }
                else if (d.Variable.Equals("DrainMax"))
                {
                    // Added by VillageIdiot
                    maxData = d;
                    maxVar = v;
                }
                else if (d.Variable.Equals("DamageRatio"))
                {
                    // Added by VillageIdiot
                    damageRatioData = d;
                    damageRatioVar = v;
                }
            }

            // Figure out the label string
            string labelTag = null;
            string labelColor = null;          
            string label = this.GetLabelAndColorFromTag(db, data, recordId, ref labelTag, ref labelColor);

            if (TQDebug.ItemDebugLevel > 1)
            {
                TQDebug.DebugWriteLine(string.Empty);
                TQDebug.DebugWriteLine("Full attribute = " + data.FullAttribute); 
                TQDebug.DebugWriteLine("Item.label = " + label);
            }

            label = ItemAttributes.ConvertFormatString(label);

            // Figure out the Amount string
            string amount = null;
            if (minData != null && maxData != null &&
                minVar.GetSingle(Math.Min(minVar.NumberOfValues - 1, varNum)) != maxVar.GetSingle(Math.Min(maxVar.NumberOfValues - 1, varNum)))
            {
                amount = this.GetAmountRange(db, data, varNum, minVar, maxVar, ref label, labelColor);
            }
            else
            {
                amount = this.GetAmountSingle(db, data, varNum, minVar, maxVar, ref label, labelColor);
            }

            // Figure out the duration string
            string duration = null;
            if (minDurData != null && maxDurData != null)
            {
                duration = GetDurationRange(db, varNum, minDurVar, maxDurVar);
            }
            else
            {
                duration = GetDurationSingle(db, varNum, minDurVar, maxDurVar);
            }

            // Added by VillageIdiot
            // Adds damage ratio
            // Figure out the Damage Ratio string
            string damageRatio = null;
            if (damageRatioData != null)
            {
                damageRatio = GetDamageRatio(db, varNum, damageRatioData, damageRatioVar);
            }

            // Figure out the chance string
            string chance = null;
            if (chanceData != null)
            {
                chance = GetChance(db, varNum, chanceVar);
            }

            // Display the chance + label + Amount + Duration + DamageRatio
            string[] strarray = new string[5];
            int numStrings = 0;
            if (label != null)
            {
                label = Database.MakeSafeForHtml(label);
                if (labelColor != null)
                {
                    label = Item.Format("<font color={0}>{1}</font>", labelColor, label);
                }
            }

            if (chance != null)
            {
                strarray[numStrings++] = chance;
            }

            if (amount != null)
            {
                strarray[numStrings++] = amount;
            }

            if (label != null)
            {
                strarray[numStrings++] = label;
            }

            if (duration != null)
            {
                strarray[numStrings++] = duration;
            }

            if (damageRatio != null)
            {
                // Added by VillageIdiot
                strarray[numStrings++] = damageRatio;
            }

            if ((amount != null) || (duration != null))
            {
                string txt = string.Join(" ", strarray, 0, numStrings);

                // Figure out what color to use
                string fontColor = null;
                if (!isGlobal && (chance == null || data.Effect.Equals("defensiveBlock"))
                    && recordId == this.baseItemID && duration == null && amount != null)
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

                if (fontColor == null)
                {
                    // magical effect
                    fontColor = Database.HtmlColor(Item.GetColor(ItemStyle.Epic));
                }

                txt = string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", fontColor, txt);
                if (isGlobal)
                {
                    txt = string.Concat(globalIndent, txt);
                }

                results.Add(txt);
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
                modifier = GetModifier(db, data, varNum, modifierData, modifierVar);
            }

            string durationModifier = null;
            if (durationModifierData != null)
            {
                durationModifier = GetDurationModifier(db, varNum, durationModifierVar);
            }

            string modifierChance = null;
            if (modifierChanceData != null)
            {
                modifierChance = GetChanceModifier(db, varNum, modifierChanceVar);
            }

            numStrings = 0;
            if (modifierChance != null)
            {
                strarray[numStrings++] = modifierChance;
            }

            if (modifier != null)
            {
                strarray[numStrings++] = modifier;
            }

            if (durationModifier != null)
            {
                strarray[numStrings++] = durationModifier;
            }

            if (modifier != null)
            {
                string txt = string.Join(" ", strarray, 0, numStrings);
                txt = Item.Format("<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Epic)), txt);
                if (isGlobal)
                {
                    txt = string.Concat(globalIndent, txt);
                }

                results.Add(txt);
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

                if (!(amount != null && (attributeData.Variable.Equals("Min") || attributeData.Variable.Equals("Max")
                    || attributeData.Variable.Equals("DrainMin") || attributeData.Variable.Equals("DrainMax"))) && // Added by VillageIdiot
                    !(duration != null && (attributeData.Variable.Equals("DurationMin") || attributeData.Variable.Equals("DurationMax"))) &&
                    !(chance != null && attributeData.Variable.Equals("Chance")) &&
                    !(modifier != null && attributeData.Variable.Equals("Modifier")) &&
                    !(durationModifier != null && attributeData.Variable.Equals("DurationModifier")) &&
                    !(modifierChance != null && attributeData.Variable.Equals("ModifierChance")) &&
                    !(damageRatio != null && attributeData.Variable.Equals("DamageRatio")) && // Added by VillageIdiot
                    !attributeData.Variable.Equals("Global") &&
                    !(attributeData.Variable.Equals("XOR") && isGlobal))
                {
                    string line = null;
                    string font = null;
                    if (attributeData.FullAttribute.Equals("characterBaseAttackSpeedTag"))
                    {
                        // only display this tag if we are a basic weapon
                        if (this.IsWeapon && recordId == this.baseItemID)
                        {
                            font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Mundane)));
                            line = db.GetFriendlyName(variable.GetString(0));
                        }
                        else
                        {
                            line = string.Empty;
                        }
                    }
                    else if (attributeData.FullAttribute.EndsWith("GlobalChance", StringComparison.OrdinalIgnoreCase))
                    {
                        line = GetGlobalChance(db, attributeList, varNum, variable, ref font);
                    }
                    else if (attributeData.FullAttribute.StartsWith("racialBonus", StringComparison.OrdinalIgnoreCase))
                    {
                        line = GetRacialBonus(db, record, results, varNum, isGlobal, globalIndent, variable, attributeData, line, ref font);
                    }
                    else if (attributeData.FullAttribute.Equals("augmentAllLevel"))
                    {
                        line = GetAugmentAllLevel(db, varNum, variable, ref font);
                    }
                    else if (attributeData.FullAttribute.StartsWith("augmentMasteryLevel", StringComparison.OrdinalIgnoreCase))
                    {
                        line = GetAugmentMasteryLevel(db, record, variable, attributeData, ref font);
                    }
                    else if (attributeData.FullAttribute.StartsWith("augmentSkillLevel", StringComparison.OrdinalIgnoreCase))
                    {
                        line = GetAugmentSkillLevel(db, record, variable, attributeData, line, ref font);
                    }
                    else if (this.IsFormulae && recordId == this.baseItemID)
                    {
                        // Added by VillageIdiot
                        line = GetFormulae(db, results, variable, attributeData, line, ref font);
                    }
                    else if (attributeData.FullAttribute.Equals("itemSkillName"))
                    {
                        line = GetGrantedSkill(db, record, results, variable, line, ref font);
                    }

                    // Added by VillageIdiot
                    // Shows the header text for the pet bonus
                    if (attributeData.FullAttribute.Equals("petBonusName"))
                    {
                        line = GetPetBonusName(db, ref font);
                    }

                    // Added by VillageIdiot
                    // Set the scale percent here
                    if (recordId == this.baseItemID && attributeData.FullAttribute.Equals("attributeScalePercent") && this.itemScalePercent == 1.00)
                    {
                        this.itemScalePercent += variable.GetSingle(0) / 100;

                        // Set line to nothing so we do not see the tag text.
                        line = string.Empty;
                    }
                    else if (attributeData.FullAttribute.Equals("skillName"))
                    {
                        // Added by VillageIdiot
                        // This is for Scroll effects which get decoded in the skill code below
                        // Set line to nothing so we do not see the tag text.
                        line = string.Empty;
                    }
                    else if (attributeData.EffectType == ItemAttributesEffectType.SkillEffect)
                    {
                        line = GetSkillEffect(db, data, varNum, variable, attributeData, line, ref font);
                    }
                    else if (attributeData.FullAttribute.EndsWith("DamageQualifier", StringComparison.OrdinalIgnoreCase))
                    {
                        // Added by VillageIdiot
                        // for Damage Absorption

                        // Get the qualifier title
                        string title = db.GetFriendlyName("tagDamageAbsorptionTitle");
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
                        string damageType = db.GetFriendlyName(string.Concat("tagQualifyingDamage", damageTag));

                        string formatSpec = db.GetFriendlyName("formatQualifyingDamage");
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

                            formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
                        }

                        font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Mundane)));
                        line = Item.Format(formatSpec, damageType);
                    }

                    // We have no line so just show the raw attribute
                    if (line == null)
                    {
                        line = GetRawAttribute(db, data, varNum, variable, ref font);
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
                        if (attributeData.FullAttribute.Equals("itemSkillName"))
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
                        if (this.IsFormulae && attributeData.FullAttribute.StartsWith("reagent", StringComparison.OrdinalIgnoreCase))
                        {
                            line = string.Concat("&nbsp;&nbsp;&nbsp;&nbsp;", line);
                        }

                        results.Add(line);
                    }

                    // Added by VillageIdiot
                    // This a special case for pet bonuses
                    if (attributeData.FullAttribute.Equals("petBonusName"))
                    {
                        string petBonusID = record.GetString("petBonusName", 0);
                        DBRecord petBonusRecord = db.GetRecordFromFile(petBonusID);
                        if (petBonusRecord != null)
                        {
                            this.GetAttributesFromRecord(db, petBonusRecord, true, petBonusID, results);
                            results.Add(string.Empty);
                        }
                    }

                    // Added by VillageIdiot
                    // Another special case for skill description and effects of activated skills
                    if (attributeData.FullAttribute.Equals("itemSkillName") || (this.IsScroll && attributeData.FullAttribute.Equals("skillName")))
                    {
                        this.GetSkillDescriptionAndEffects(db, record, results, variable, line);
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
        /// <param name="db">database instance</param>
        /// <param name="record">DBRecord databse record</param>
        /// <param name="results">results list</param>
        /// <param name="variable">variable structure</param>
        /// <param name="line">line of text</param>
        private void GetSkillDescriptionAndEffects(Database db, DBRecord record, List<string> results, Variable variable, string line)
        {
            string autoController = record.GetString("itemSkillAutoController", 0);
            if (!string.IsNullOrEmpty(autoController) || this.IsScroll)
            {
                DBRecord skillRecord = db.GetRecordFromFile(variable.GetString(0));

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
                                skillDescription = db.GetFriendlyName(descriptionTag);
                                if (skillDescription.Length != 0)
                                {
                                    skillDescription = Database.MakeSafeForHtml(skillDescription);
                                    skillDescriptionList = Database.WrapWords(skillDescription, lineLength);

                                    for (int k = 0; k < skillDescriptionList.Count; ++k)
                                    {
                                        skillDescriptionList[k] = string.Concat("&nbsp;&nbsp;&nbsp;&nbsp;", skillDescriptionList[k]);
                                        results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Mundane)), skillDescriptionList[k]));
                                    }

                                    // Show granted skill level
                                    if (Settings.Default.ShowSkillLevel)
                                    {
                                        string formatSpec = db.GetFriendlyName("MenuLevel");
                                        if (string.IsNullOrEmpty(formatSpec))
                                        {                                        
                                            formatSpec = "Level:   {0}";
                                        }
                                        else 
                                        {                                        
                                            formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
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
                            DBRecord buffSkillRecord = db.GetRecordFromFile(buffSkillName);
                            if (buffSkillRecord != null)
                            {
                                descriptionTag = buffSkillRecord.GetString("skillBaseDescription", 0);
                                if (descriptionTag.Length != 0)
                                {
                                    skillDescription = db.GetFriendlyName(descriptionTag);
                                    skillDescriptionList = Database.WrapWords(skillDescription, lineLength);

                                    for (int m = 0; m < skillDescriptionList.Count; ++m)
                                    {
                                        skillDescriptionList[m] = string.Concat("&nbsp;&nbsp;&nbsp;&nbsp;", skillDescriptionList[m]);
                                        results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Mundane)), skillDescriptionList[m]));
                                    }

                                    // Show granted skill level
                                    if (Settings.Default.ShowSkillLevel)
                                    {
                                        string formatSpec = db.GetFriendlyName("MenuLevel");
                                        if (string.IsNullOrEmpty(formatSpec)) 
                                        {
                                            formatSpec = "Level:   {0}";
                                        }
                                        else 
                                        {
                                            formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
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
                            this.ConvertPetStats(db, skillRecord, results);
                        }
                        else
                        {
                            // Skill Effects
                            if (!string.IsNullOrEmpty(buffSkillName))
                            {
                                this.GetAttributesFromRecord(db, db.GetRecordFromFile(buffSkillName), true, buffSkillName, results);
                            }
                            else
                            {
                                this.GetAttributesFromRecord(db, skillRecord, true, variable.GetString(0), results);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets a formatted single amount
        /// </summary>
        /// <param name="db">database instance</param>
        /// <param name="data">ItemAttributesData data</param>
        /// <param name="varNum">variable number to look up</param>
        /// <param name="minVar">minVar variable</param>
        /// <param name="maxVar">maxVar variable</param>
        /// <param name="label">label string</param>
        /// <param name="labelColor">label color</param>
        /// <returns>formatted single amount string</returns>
        private string GetAmountSingle(Database db, ItemAttributesData data, int varNum, Variable minVar, Variable maxVar, ref string label, string labelColor)
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

            string formatSpec = db.GetFriendlyName(tag);
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

                formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
            }

            if (label.IndexOf('{') >= 0)
            {
                // the label has formatting codes.  Use it to format the amount
                formatSpec = label;
                label = null;
                color = labelColor;
            }

            Variable currentVariable = minVar;
            if (currentVariable == null)
            {
                currentVariable = maxVar;
            }

            if (currentVariable != null)
            {
                // Adjust for itemScalePercent
                // only for floats
                if (currentVariable.DataType == VariableDataType.Float)
                {
                    currentVariable[Math.Min(currentVariable.NumberOfValues - 1, varNum)] = (float)currentVariable[Math.Min(currentVariable.NumberOfValues - 1, varNum)] * this.itemScalePercent;
                }

                amount = Item.Format(formatSpec, currentVariable[Math.Min(currentVariable.NumberOfValues - 1, varNum)]);
                amount = Database.MakeSafeForHtml(amount);
                if (color != null)
                {
                    amount = Item.Format("<font color={0}>{1}</font>", color, amount);
                }
            }

            return amount;
        }

        /// <summary>
        /// Gets a formatted range amount
        /// </summary>
        /// <param name="db">database instance</param>
        /// <param name="data">ItemAttributesData data</param>
        /// <param name="varNum">variable number to look up</param>
        /// <param name="minVar">minVar variable</param>
        /// <param name="maxVar">maxVar variable</param>
        /// <param name="label">label string</param>
        /// <param name="labelColor">label color</param>
        /// <returns>formatted range string</returns>
        private string GetAmountRange(Database db, ItemAttributesData data, int varNum, Variable minVar, Variable maxVar, ref string label, string labelColor)
        {
            // Added by VillageIdiot : check to see if min and max are the same
            string color = null;
            string amount = null;

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

            string formatSpec = db.GetFriendlyName(tag);
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

                formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
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
            minVar[Math.Min(minVar.NumberOfValues - 1, varNum)] = (float)minVar[Math.Min(minVar.NumberOfValues - 1, varNum)] * this.itemScalePercent;
            maxVar[Math.Min(maxVar.NumberOfValues - 1, varNum)] = (float)maxVar[Math.Min(maxVar.NumberOfValues - 1, varNum)] * this.itemScalePercent;

            amount = Item.Format(formatSpec, minVar[Math.Min(minVar.NumberOfValues - 1, varNum)], maxVar[Math.Min(maxVar.NumberOfValues - 1, varNum)]);
            amount = Database.MakeSafeForHtml(amount);
            if (color != null)
            {
                amount = Item.Format("<font color={0}>{1}</font>", color, amount);
            }

            return amount;
        }

        /// <summary>
        /// Gets the item label from the tag
        /// </summary>
        /// <param name="db">database instance</param>
        /// <param name="data">ItemAttributesData structure for the attribute</param>
        /// <param name="recordId">string containing the database record id</param>
        /// <param name="labelTag">the label tag</param>
        /// <param name="labelColor">the label color which gets modified here</param>
        /// <returns>string containing the label.</returns>
        private string GetLabelAndColorFromTag(Database db, ItemAttributesData data, string recordId, ref string labelTag, ref string labelColor)
        {
            labelTag = ItemAttributes.GetAttributeTextTag(data);
            string label;

            if (string.IsNullOrEmpty(labelTag))
            {
                labelTag = string.Concat("?", data.FullAttribute, "?");
                labelColor = Database.HtmlColor(Item.GetColor(ItemStyle.Legendary));
            }

            // if this is an Armor effect and we are not armor, then change it to bonus
            if (labelTag.Equals("DefenseAbsorptionProtection"))
            {
                if (!this.IsArmor || recordId != this.baseItemID)
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

            label = db.GetFriendlyName(labelTag);
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
        /// <param name="db">database instance</param>
        /// <param name="record">DBRecord for the triggered skill</param>
        /// <param name="recordId">string containing the record id</param>
        /// <param name="varNum">variable number which we are looking up since there can be multiple values</param>
        /// <returns>int containing the skill level</returns>
        private int GetTriggeredSkillLevel(Database db, DBRecord record, string recordId, int varNum)
        {
            DBRecord baseItem = db.GetRecordFromFile(this.baseItemInfo.ItemId);

            // Check to see if it's a Buff Skill
            if (baseItem.GetString("itemSkillAutoController", 0) != null)
            {
                int level = baseItem.GetInt32("itemSkillLevel", 0);
                if (record.GetString("Class", 0).ToUpperInvariant().StartsWith("SKILLBUFF", StringComparison.OrdinalIgnoreCase))
                {
                    DBRecord skill = db.GetRecordFromFile(this.baseItemInfo.GetString("itemSkillName"));
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
        /// <param name="db">database instance</param>
        /// <param name="record">DBRecord of the skill</param>
        /// <param name="recordId">string of the record id</param>
        /// <param name="varNum">Which variable we are using since there can be multiple values.</param>
        /// <returns>int containing the pet skill level</returns>
        private int GetPetSkillLevel(Database db, DBRecord record, string recordId, int varNum)
        {
            // Check to see if this really is a skill
            if (record.GetString("Class", 0).ToUpperInvariant().StartsWith("SKILL_ATTACK", StringComparison.OrdinalIgnoreCase))
            {
                // Check to see if this item creates a pet
                DBRecord petSkill = db.GetRecordFromFile(this.baseItemInfo.GetString("skillName"));
                string petID = petSkill.GetString("spawnObjects", 0);
                if (!string.IsNullOrEmpty(petID))
                {
                    DBRecord petRecord = db.GetRecordFromFile(petID);
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
        /// Added by VillageIdiot
        /// Used for showing the pet statistics
        /// </summary>
        /// <param name="db">database instance</param>
        /// <param name="skillRecord">DBRecord of the skill</param>
        /// <param name="results">List containing the results</param>
        private void ConvertPetStats(TQVaultData.Database db, DBRecord skillRecord, List<string> results)
        {
            string formatSpec, petLine;
            int summonLimit = skillRecord.GetInt32("petLimit", 0);
            if (summonLimit > 1)
            {
                formatSpec = db.GetFriendlyName("SkillPetLimit");
                if (string.IsNullOrEmpty(formatSpec))
                {
                    formatSpec = "{0} Summon Limit";
                }
                else
                {
                    formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
                }

                petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, summonLimit.ToString(CultureInfo.CurrentCulture));
                results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Mundane)), petLine));
            }

            DBRecord petRecord = db.GetRecordFromFile(skillRecord.GetString("spawnObjects", 0));
            if (petRecord != null)
            {
                // Print out Pet attributes
                formatSpec = db.GetFriendlyName("SkillPetDescriptionHeading");
                if (string.IsNullOrEmpty(formatSpec))
                {
                    formatSpec = "{0} Attributes:";
                }
                else
                {
                    formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
                }

                string petNameTag = petRecord.GetString("description", 0);
                string petName = db.GetFriendlyName(petNameTag);
                float value = 0.0F;
                petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, petName);
                results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Mundane)), petLine));

                // Time to live
                formatSpec = db.GetFriendlyName("tagSkillPetTimeToLive");
                if (string.IsNullOrEmpty(formatSpec))
                {
                    formatSpec = "Life Time {0} Seconds";
                }
                else
                {
                    formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
                }

                petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, skillRecord.GetSingle("spawnObjectsTimeToLive", 0));
                results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Mundane)), petLine));

                // Health
                value = petRecord.GetSingle("characterLife", 0);
                if (value != 0.0F)
                {
                    formatSpec = db.GetFriendlyName("SkillPetDescriptionHealth");
                    if (string.IsNullOrEmpty(formatSpec))
                    {
                        formatSpec = "{0}  Health";
                    }
                    else
                    {
                        formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
                    }

                    petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, value);
                    results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Mundane)), petLine));
                }

                // Energy
                value = petRecord.GetSingle("characterMana", 0);
                if (value != 0.0F)
                {
                    formatSpec = db.GetFriendlyName("SkillPetDescriptionMana");
                    if (string.IsNullOrEmpty(formatSpec))
                    {
                        formatSpec = "{0}  Energy";
                    }
                    else
                    {
                        formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
                    }

                    petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, value);
                    results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Mundane)), petLine));
                }

                // Add abilities text
                results.Add(string.Empty);
                formatSpec = db.GetFriendlyName("tagSkillPetAbilities");
                if (string.IsNullOrEmpty(formatSpec))
                {
                    formatSpec = "{0} Abilities:";
                }
                else
                {
                    formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
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
                        formatSpec = db.GetFriendlyName("SkillPetDescriptionDamageMinOnly");
                        if (string.IsNullOrEmpty(formatSpec))
                        {
                            formatSpec = "{0}  Damage";
                        }
                        else
                        {
                            formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
                        }

                        petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, value);
                        results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", Database.HtmlColor(Item.GetColor(ItemStyle.Mundane)), petLine));
                    }
                    else
                    {
                        formatSpec = db.GetFriendlyName("SkillPetDescriptionDamageMinMax");
                        if (string.IsNullOrEmpty(formatSpec))
                        {
                            formatSpec = "{0} - {1}  Damage";
                        }
                        else
                        {
                            formatSpec = ItemAttributes.ConvertFormatString(formatSpec);
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
                    DBRecord skillRecord1 = db.GetRecordFromFile(skills[i]);
                    DBRecord record = null;
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
                            skillName = db.GetFriendlyName(skillNameTag);
                        }
                    }
                    else
                    {
                        // This is a buff skill
                        DBRecord buffSkillRecord = db.GetRecordFromFile(buffSkillName);
                        if (buffSkillRecord != null)
                        {
                            record = buffSkillRecord;
                            recordID = buffSkillName;
                            skillNameTag = buffSkillRecord.GetString("skillDisplayName", 0);
                            if (skillNameTag.Length != 0)
                            {
                                skillName = db.GetFriendlyName(skillNameTag);
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

                    this.GetAttributesFromRecord(db, record, true, recordID, results);
                    results.Add(string.Empty);
                }
            }
        }

        /// <summary>
        /// Converts the item's attribute list to a string
        /// </summary>
        /// <param name="db">database instance</param>
        /// <param name="record">DBRecord for the item</param>
        /// <param name="attributeList">ArrayList containing the attributes list</param>
        /// <param name="recordId">string containing the record id</param>
        /// <param name="results">List containing the results</param>
        private void ConvertAttributeListToString(Database db, DBRecord record, List<Variable> attributeList, string recordId, List<string> results)
        {
            if (TQDebug.ItemDebugLevel > 0)
            {
                TQDebug.DebugWriteLine(string.Format(
                    CultureInfo.InvariantCulture,
                    "Item.ConvertAttrListToString ({0}, {1}, {2}, {3}, {4})",
                    db,
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

            this.ConvertOffenseAttributesToString(db, record, attributeList, data, recordId, results);
            return;
        }
    }
}
