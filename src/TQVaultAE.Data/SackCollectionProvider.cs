//-----------------------------------------------------------------------
// <copyright file="SackCollection.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Data
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using TQVaultAE.Domain.Contracts.Providers;
	using TQVaultAE.Domain.Contracts.Services;
	using TQVaultAE.Domain.Entities;
	using TQVaultAE.Logs;


	/// <summary>
	/// Encodes and decodes a Titan Quest item sack from a player file.
	/// </summary>
	public class SackCollectionProvider : ISackCollectionProvider
	{
		private readonly log4net.ILog Log;
		private readonly IItemProvider ItemProvider;
		private readonly ITQDataService TQData;

		public SackCollectionProvider(ILogger<SackCollectionProvider> log, IItemProvider itemProvider, ITQDataService tQData)
		{
			this.Log = log.Logger;
			this.ItemProvider = itemProvider;
			this.TQData = tQData;
		}

		/// <summary>
		/// Encodes the sack into binary form
		/// </summary>
		/// <param name="writer">BinaryWriter instance</param>
		public void Encode(SackCollection sc, BinaryWriter writer)
		{
			if (sc.sackType == SackType.Stash)
			{
				// Item stacks are stored as single items in the stash
				TQData.WriteCString(writer, "numItems");
				writer.Write(sc.Count);
			}
			else if (sc.sackType == SackType.Equipment)
			{
				// Nothing special except to skip all of the other header crap
				// since the number of items is always fixed.
			}
			else
			{
				TQData.WriteCString(writer, "begin_block");
				writer.Write(sc.beginBlockCrap);

				TQData.WriteCString(writer, "tempBool");
				writer.Write(sc.tempBool);

				TQData.WriteCString(writer, "size");
				writer.Write(sc.CountTQItems());
			}

			int slotNumber = -1;
			foreach (Item item in sc)
			{
				++slotNumber;
				item.ContainerType = sc.sackType;
				int itemAttached = 0;
				int alternate = 0;

				// Additional logic to encode the weapon slots in the equipment section
				if (sc.sackType == SackType.Equipment && (slotNumber == 7 || slotNumber == 9))
				{
					TQData.WriteCString(writer, "begin_block");
					writer.Write(sc.beginBlockCrap);

					TQData.WriteCString(writer, "alternate");
					if (slotNumber == 9)
						// Only set the flag for the second set of weapons
						alternate = 1;
					else
						// Otherwise set the flag to false.
						alternate = 0;

					writer.Write(alternate);
				}

				ItemProvider.Encode(item, writer);

				if (sc.sackType == SackType.Equipment)
				{
					TQData.WriteCString(writer, "itemAttached");
					if (!string.IsNullOrEmpty(item.BaseItemId) && slotNumber != 9 && slotNumber != 10)
						// If there is an item in sc slot, set the flag.
						// Unless it's in the secondary weapon slot.
						itemAttached = 1;
					else
						// sc is only a dummy item so we do not set the flag.
						itemAttached = 0;

					writer.Write(itemAttached);
				}

				// Additional logic to encode the weapon slots in the equipment section
				if (sc.sackType == SackType.Equipment && (slotNumber == 8 || slotNumber == 10))
				{
					TQData.WriteCString(writer, "end_block");
					writer.Write(sc.endBlockCrap);
				}
			}

			TQData.WriteCString(writer, "end_block");
			writer.Write(sc.endBlockCrap);
		}

		/// <summary>
		/// Parses the binary sack data to internal data
		/// </summary>
		/// <param name="reader">BinaryReader instance</param>
		public void Parse(SackCollection sc, BinaryReader reader)
		{
			try
			{
				sc.isModified = false;

				if (sc.sackType == SackType.Stash)
				{
					// IL decided to use a different format for the stash files.
					TQData.ValidateNextString("numItems", reader);
					sc.size = reader.ReadInt32();
				}
				else if (sc.sackType == SackType.Equipment)
				{
					if (sc.isImmortalThrone)
					{
						sc.size = 12;
						sc.slots = 12;
					}
					else
					{
						sc.size = 11;
						sc.slots = 11;
					}
				}
				else
				{
					// sc is just a regular sack.
					TQData.ValidateNextString("begin_block", reader); // make sure we just read a new block
					sc.beginBlockCrap = reader.ReadInt32();

					TQData.ValidateNextString("tempBool", reader);
					sc.tempBool = reader.ReadInt32();

					TQData.ValidateNextString("size", reader);
					sc.size = reader.ReadInt32();
				}

				sc.items = new List<Item>(sc.size);

				Item prevItem = null;
				for (int i = 0; i < sc.size; ++i)
				{
					// Additional logic to decode the weapon slots in the equipment section
					if (sc.sackType == SackType.Equipment && (i == 7 || i == 9))
					{
						TQData.ValidateNextString("begin_block", reader);
						sc.beginBlockCrap = reader.ReadInt32();

						// Eat the alternate tag and flag
						TQData.ValidateNextString("alternate", reader);

						// Skip over the alternateCrap
						reader.ReadInt32();
					}

					Item item = new Item();
					item.ContainerType = sc.sackType;
					ItemProvider.Parse(item, reader);

					// Stack sc item with the previous item if necessary
					if ((prevItem != null) && item.DoesStack && (item.PositionX == -1) && (item.PositionY == -1))
						prevItem.StackSize++;
					else
					{
						prevItem = item;
						sc.items.Add(item);
						if (sc.sackType == SackType.Equipment)
						{
							// Get the item location from the table
							item.PositionX = SackCollection.GetEquipmentLocationOffset(i).X;
							item.PositionY = SackCollection.GetEquipmentLocationOffset(i).Y;

							// Eat the itemAttached tag and flag
							TQData.ValidateNextString("itemAttached", reader);

							// Skip over the itemAttachedCrap
							reader.ReadInt32();
						}
					}

					// Additional logic to decode the weapon slots in the equipment section
					if (sc.sackType == SackType.Equipment && (i == 8 || i == 10))
					{
						TQData.ValidateNextString("end_block", reader);
						sc.endBlockCrap = reader.ReadInt32();
					}
				}

				TQData.ValidateNextString("end_block", reader);
				sc.endBlockCrap = reader.ReadInt32();
			}
			catch (ArgumentException ex)
			{
				// The ValidateNextString Method can throw an ArgumentException.
				// We just pass it along at sc point.
				Log.Debug("ValidateNextString fail !", ex);
				throw;
			}
		}

	}
}