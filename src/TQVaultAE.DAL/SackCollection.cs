//-----------------------------------------------------------------------
// <copyright file="SackCollection.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultData
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Drawing;
	using System.IO;

	/// <summary>
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
		TransferStash,

		/// <summary>
		/// Relic Vault stash
		/// </summary>
		RelicVaultStash
	}

	/// <summary>
	/// Encodes and decodes a Titan Quest item sack from a player file.
	/// </summary>
	public class SackCollection : IEnumerable<Item>
	{
		/// <summary>
		/// Cell offsets for the slots in the equipment panel.
		/// Indicates the upper left cell of the slot.
		/// </summary>
		private static Point[] equipmentLocationOffsets =
		{
			new Point(4, 0),  // Head
            new Point(4, 3),  // Neck
            new Point(4, 5),  // Body
            new Point(4, 9),  // Legs
            new Point(7, 6),  // Arms
            new Point(4, 12), // Ring1
            new Point(5, 12), // Ring2

            // Use x = -3 to flag as a weapon
            // Use y value as index into weaponLocationOffsets
            new Point(Item.WeaponSlotIndicator, 0), // Weapon1
            new Point(Item.WeaponSlotIndicator, 1), // Shield1
            new Point(Item.WeaponSlotIndicator, 2), // Weapon2
            new Point(Item.WeaponSlotIndicator, 3), // Shield2
            new Point(1, 6), // Artifact
        };

		/// <summary>
		/// Sizes of the slots in the equipment panel
		/// </summary>
		private static Size[] equipmentLocationSizes =
		{
			new Size(2, 2), // Head
            new Size(1, 1), // Neck
            new Size(2, 3), // Body
            new Size(2, 2), // Legs
            new Size(2, 2), // Arms
            new Size(1, 1), // Ring1
            new Size(1, 1), // Ring2
            new Size(2, 5), // Weapon1
            new Size(2, 5), // Shield1
            new Size(2, 5), // Weapon2
            new Size(2, 5), // Shield2
            new Size(2, 2), // Artifact
        };

		/// <summary>
		/// Used to properly draw the weapon the weapon box on the equipment panel
		/// These values are the upper left corner of the weapon box
		/// </summary>
		private static Point[] weaponLocationOffsets =
		{
			new Point(1, 0), // Weapon1
            new Point(7, 0), // Shield1
            new Point(1, 9), // Weapon2
            new Point(7, 9), // Shield2
        };

		/// <summary>
		/// Size of the weapon slots in the equipment panel
		/// </summary>
		private static Size weaponLocationSize = new Size(2, 5);

		/// <summary>
		/// Begin Block header in the file.
		/// </summary>
		private int beginBlockCrap;

		/// <summary>
		/// End Block header in the file
		/// </summary>
		private int endBlockCrap;

		/// <summary>
		/// TempBool entry in the file.
		/// </summary>
		private int tempBool;

		/// <summary>
		/// Number of items in the sack according to TQ.
		/// </summary>
		private int size;

		/// <summary>
		/// Items contained in this sack
		/// </summary>
		private List<Item> items;

		/// <summary>
		/// Flag to indicate this sack has been modified.
		/// </summary>
		private bool isModified;

		/// <summary>
		/// Indicates the type of sack
		/// </summary>
		private SackType sackType;

		/// <summary>
		/// Indicates whether this is Immortal Throne
		/// </summary>
		private bool isImmortalThrone;

		/// <summary>
		/// Number of equipment slots
		/// </summary>
		private int slots;

		/// <summary>
		/// Initializes a new instance of the SackCollection class.
		/// </summary>
		public SackCollection()
		{
			this.items = new List<Item>();
			this.sackType = SackType.Sack;
		}

		/// <summary>
		/// Gets the Weapon slot size
		/// </summary>
		public static Size WeaponLocationSize
		{
			get
			{
				return weaponLocationSize;
			}
		}

		/// <summary>
		/// Gets the total number of offsets in the weapon location offsets array.
		/// </summary>
		public static int NumberOfWeaponSlots
		{
			get
			{
				return weaponLocationOffsets.Length;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this sack has been modified
		/// </summary>
		public bool IsModified
		{
			get
			{
				return this.isModified;
			}

			set
			{
				this.isModified = value;
			}
		}

		/// <summary>
		/// Gets or sets the sack type
		/// </summary>
		public SackType SackType
		{
			get
			{
				return this.sackType;
			}

			set
			{
				this.sackType = value;
			}
		}

		/// <summary>
		/// Identifies the stash type.
		/// </summary>
		public SackType StashType {	get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this is from Immortal Throne
		/// </summary>
		public bool IsImmortalThrone
		{
			get
			{
				return this.isImmortalThrone;
			}

			set
			{
				this.isImmortalThrone = value;
			}
		}

		/// <summary>
		/// Gets the number of equipment slots
		/// </summary>
		public int NumberOfSlots
		{
			get
			{
				return this.slots;
			}
		}

		/// <summary>
		/// Gets the number of items in the sack.
		/// </summary>
		public int Count
		{
			get
			{
				return this.items.Count;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the number of items in the sack is equal to zero.
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				return this.items.Count == 0;
			}
		}

		/// <summary>
		/// Gets offset of the weapon slot
		/// </summary>
		/// <param name="weaponSlot">weapon slot number we are looking for</param>
		/// <returns>Point of the upper left corner cell of the slot</returns>
		public static Point GetWeaponLocationOffset(int weaponSlot)
		{
			if (weaponSlot < 0 || weaponSlot > NumberOfWeaponSlots)
			{
				return Point.Empty;
			}

			return weaponLocationOffsets[weaponSlot];
		}

		/// <summary>
		/// Gets the size of an equipment slot
		/// </summary>
		/// <param name="equipmentSlot">weapon slot number we are looking for</param>
		/// <returns>Size of the weapon slot</returns>
		public static Size GetEquipmentLocationSize(int equipmentSlot)
		{
			if (equipmentSlot < 0 || equipmentSlot > equipmentLocationSizes.Length)
			{
				return Size.Empty;
			}

			return equipmentLocationSizes[equipmentSlot];
		}

		/// <summary>
		/// Gets the upper left cell of an equipment slot
		/// </summary>
		/// <param name="equipmentSlot">equipment slot we are looking for.</param>
		/// <returns>Point of the upper left cell of the slot.</returns>
		public static Point GetEquipmentLocationOffset(int equipmentSlot)
		{
			if (equipmentSlot < 0 || equipmentSlot > equipmentLocationOffsets.Length)
			{
				return Point.Empty;
			}

			return equipmentLocationOffsets[equipmentSlot];
		}

		/// <summary>
		/// Gets whether the item is located in a weapon slot.
		/// </summary>
		/// <param name="equipmentSlot">slot that we are checking</param>
		/// <returns>true if the slot is a weapon slot.</returns>
		public static bool IsWeaponSlot(int equipmentSlot)
		{
			if (equipmentSlot < 0 || equipmentSlot > equipmentLocationOffsets.Length)
			{
				return false;
			}

			return equipmentLocationOffsets[equipmentSlot].X == Item.WeaponSlotIndicator;
		}

		/// <summary>
		/// IEnumerator interface implementation.  Iterates all of the items in the sack.
		/// </summary>
		/// <returns>Items in the sack.</returns>
		public IEnumerator<Item> GetEnumerator()
		{
			for (int i = 0; i < this.Count; i++)
			{
				yield return this.GetItem(i);
			}
		}

		/// <summary>
		/// Non Generic enumerator interface.
		/// </summary>
		/// <returns>Generic interface implementation.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		/// <summary>
		/// Gets the index in the item list of a particular Item
		/// </summary>
		/// <param name="item">Item that we are looking for</param>
		/// <returns>index in the item array</returns>
		public int IndexOfItem(Item item)
		{
			return this.items.IndexOf(item);
		}

		/// <summary>
		/// Removes an Item from the item list
		/// </summary>
		/// <param name="item">Item we are removing</param>
		public void RemoveItem(Item item)
		{
			this.items.Remove(item);
			this.IsModified = true;
		}

		/// <summary>
		/// Removes an Item from the item list at the specified index
		/// </summary>
		/// <param name="index">index position of the item we are removing</param>
		public void RemoveAtItem(int index)
		{
			this.items.RemoveAt(index);
			this.IsModified = true;
		}

		/// <summary>
		/// Adds an item to the end of the item list
		/// </summary>
		/// <param name="item">Item we are adding</param>
		public void AddItem(Item item)
		{
			this.items.Add(item);
			this.IsModified = true;
		}

		/// <summary>
		/// Inserts an item at a specific position in the item list.
		/// </summary>
		/// <param name="index">index where we are performing the insert.</param>
		/// <param name="item">item we are inserting</param>
		public void InsertItem(int index, Item item)
		{
			this.items.Insert(index, item);
			this.IsModified = true;
		}

		/// <summary>
		/// Clears the item list
		/// </summary>
		public void EmptySack()
		{
			this.items.Clear();
			this.IsModified = true;
		}

		/// <summary>
		/// Duplicates the whole sack contents
		/// </summary>
		/// <returns>Sack instance of the duplicate sack</returns>
		public SackCollection Duplicate()
		{
			SackCollection newSack = new SackCollection();
			foreach (Item item in this)
			{
				newSack.AddItem(item.Clone());
			}

			return newSack;
		}

		/// <summary>
		/// Gets an item at the specified index in the item array.
		/// </summary>
		/// <param name="index">index in the item array</param>
		/// <returns>Item from the array at the specified index</returns>
		public Item GetItem(int index)
		{
			return this.items[index];
		}

		/// <summary>
		/// Gets the number of items according to TQ where each potion counts for 1.
		/// </summary>
		/// <returns>integer containing the number of items</returns>
		public int CountTQItems()
		{
			int ans = 0;
			foreach (Item item in this)
			{
				if (item.DoesStack)
				{
					ans += item.StackSize;
				}
				else
				{
					ans++;
				}
			}

			return ans;
		}

		/// <summary>
		/// Encodes the sack into binary form
		/// </summary>
		/// <param name="writer">BinaryWriter instance</param>
		public void Encode(BinaryWriter writer)
		{
			if (this.sackType == SackType.Stash)
			{
				// Item stacks are stored as single items in the stash
				TQData.WriteCString(writer, "numItems");
				writer.Write(this.Count);
			}
			else if (this.sackType == SackType.Equipment)
			{
				// Nothing special except to skip all of the other header crap
				// since the number of items is always fixed.
			}
			else
			{
				TQData.WriteCString(writer, "begin_block");
				writer.Write(this.beginBlockCrap);

				TQData.WriteCString(writer, "tempBool");
				writer.Write(this.tempBool);

				TQData.WriteCString(writer, "size");
				writer.Write(this.CountTQItems());
			}

			int slotNumber = -1;
			foreach (Item item in this)
			{
				++slotNumber;
				item.ContainerType = this.sackType;
				int itemAttached = 0;
				int alternate = 0;

				// Additional logic to encode the weapon slots in the equipment section
				if (this.sackType == SackType.Equipment && (slotNumber == 7 || slotNumber == 9))
				{
					TQData.WriteCString(writer, "begin_block");
					writer.Write(this.beginBlockCrap);

					TQData.WriteCString(writer, "alternate");
					if (slotNumber == 9)
					{
						// Only set the flag for the second set of weapons
						alternate = 1;
					}
					else
					{
						// Otherwise set the flag to false.
						alternate = 0;
					}

					writer.Write(alternate);
				}

				item.Encode(writer);

				if (this.sackType == SackType.Equipment)
				{
					TQData.WriteCString(writer, "itemAttached");
					if (!string.IsNullOrEmpty(item.BaseItemId) && slotNumber != 9 && slotNumber != 10)
					{
						// If there is an item in this slot, set the flag.
						// Unless it's in the secondary weapon slot.
						itemAttached = 1;
					}
					else
					{
						// This is only a dummy item so we do not set the flag.
						itemAttached = 0;
					}

					writer.Write(itemAttached);
				}

				// Additional logic to encode the weapon slots in the equipment section
				if (this.sackType == SackType.Equipment && (slotNumber == 8 || slotNumber == 10))
				{
					TQData.WriteCString(writer, "end_block");
					writer.Write(this.endBlockCrap);
				}
			}

			TQData.WriteCString(writer, "end_block");
			writer.Write(this.endBlockCrap);
		}

		/// <summary>
		/// Parses the binary sack data to internal data
		/// </summary>
		/// <param name="reader">BinaryReader instance</param>
		public void Parse(BinaryReader reader)
		{
			try
			{
				this.isModified = false;

				if (this.sackType == SackType.Stash)
				{
					// IL decided to use a different format for the stash files.
					TQData.ValidateNextString("numItems", reader);
					this.size = reader.ReadInt32();
				}
				else if (this.sackType == SackType.Equipment)
				{
					if (this.isImmortalThrone)
					{
						this.size = 12;
						this.slots = 12;
					}
					else
					{
						this.size = 11;
						this.slots = 11;
					}
				}
				else
				{
					// This is just a regular sack.
					TQData.ValidateNextString("begin_block", reader); // make sure we just read a new block
					this.beginBlockCrap = reader.ReadInt32();

					TQData.ValidateNextString("tempBool", reader);
					this.tempBool = reader.ReadInt32();

					TQData.ValidateNextString("size", reader);
					this.size = reader.ReadInt32();
				}

				this.items = new List<Item>(this.size);

				Item prevItem = null;
				for (int i = 0; i < this.size; ++i)
				{
					// Additional logic to decode the weapon slots in the equipment section
					if (this.sackType == SackType.Equipment && (i == 7 || i == 9))
					{
						TQData.ValidateNextString("begin_block", reader);
						this.beginBlockCrap = reader.ReadInt32();

						// Eat the alternate tag and flag
						TQData.ValidateNextString("alternate", reader);

						// Skip over the alternateCrap
						reader.ReadInt32();
					}

					Item item = new Item();
					item.ContainerType = this.sackType;
					item.Parse(reader);

					// Stack this item with the previous item if necessary
					if ((prevItem != null) && item.DoesStack && (item.PositionX == -1) && (item.PositionY == -1))
					{
						prevItem.StackSize++;
					}
					else
					{
						prevItem = item;
						this.items.Add(item);
						if (this.sackType == SackType.Equipment)
						{
							// Get the item location from the table
							item.PositionX = GetEquipmentLocationOffset(i).X;
							item.PositionY = GetEquipmentLocationOffset(i).Y;

							// Eat the itemAttached tag and flag
							TQData.ValidateNextString("itemAttached", reader);

							// Skip over the itemAttachedCrap
							reader.ReadInt32();
						}
					}

					// Additional logic to decode the weapon slots in the equipment section
					if (this.sackType == SackType.Equipment && (i == 8 || i == 10))
					{
						TQData.ValidateNextString("end_block", reader);
						this.endBlockCrap = reader.ReadInt32();
					}
				}

				TQData.ValidateNextString("end_block", reader);
				this.endBlockCrap = reader.ReadInt32();
			}
			catch (ArgumentException)
			{
				// The ValidateNextString Method can throw an ArgumentException.
				// We just pass it along at this point.
				throw;
			}
		}
	}
}