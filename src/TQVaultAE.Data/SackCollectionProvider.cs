//-----------------------------------------------------------------------
// <copyright file="SackCollectionProvider.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Buffers.Binary;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using Microsoft.Extensions.Logging;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Data;

/// <summary>
/// Encodes and decodes a Titan Quest item sack from a player file.
/// </summary>
public class SackCollectionProvider(ILogger<SackCollectionProvider> log, IItemProvider itemProvider, ITQDataService tQData) : ISackCollectionProvider
{
	private readonly ILogger Log = log;
	private readonly IItemProvider ItemProvider = itemProvider;
	private readonly ITQDataService TQData = tQData;

	/// <summary>
	/// Encodes the sack into binary form
	/// </summary>
	/// <param name="writer">BinaryWriter instance</param>
	public void Encode(SackCollection sc, BinaryWriter writer)
	{
		if (sc.SackType == SackType.Stash)
		{
			// Item stacks are stored as single items in the stash
			TQData.WriteKeyInt(writer, "numItems", sc.Count);
		}
		else if (sc.SackType == SackType.Equipment)
		{
			// Nothing special except to skip all of the other header crap
			// since the number of items is always fixed.
		}
		else
		{
			TQData.WriteKeyInt(writer, "begin_block", sc.beginBlockCrap);

			TQData.WriteKeyInt(writer, "tempBool", sc.tempBool);

			TQData.WriteKeyInt(writer, "size", sc.CountTQItems());
		}

		int slotNumber = -1;
		foreach (Item item in sc)
		{
			++slotNumber;
			item.Place.SackType = sc.SackType;
			// Additional logic to encode the weapon slots in the equipment section
			if (sc.SackType == SackType.Equipment && (slotNumber == 7 || slotNumber == 9))
			{
				TQData.WriteKeyInt(writer, "begin_block", sc.beginBlockCrap);

				// Only set the flag for the second set of weapons
				TQData.WriteKeyInt(writer, "alternate", slotNumber == 9 ? 1 : 0);
			}

			ItemProvider.Encode(item, writer);

			if (sc.SackType == SackType.Equipment)
			{
				// If there is an item in the slot, set the flag, unless it's in the secondary weapon slot
				// (an empty slot holds a dummy item, without the flag).
				TQData.WriteKeyInt(writer, "itemAttached", !item.IsEmpty && slotNumber != 9 && slotNumber != 10 ? 1 : 0);
			}

			// Additional logic to encode the weapon slots in the equipment section
			if (sc.SackType == SackType.Equipment && (slotNumber == 8 || slotNumber == 10))
			{
				TQData.WriteKeyInt(writer, "end_block", sc.endBlockCrap);
			}
		}

		TQData.WriteKeyInt(writer, "end_block", sc.endBlockCrap);
	}

	/// <summary>
	/// Parses the binary sack data to internal data
	/// </summary>
	/// <param name="reader">BinaryReader instance</param>
	public void Parse(SackCollection sc, BinaryReader reader)
	{
		try
		{
			sc.IsModified = false;

			if (sc.SackType == SackType.Stash)
			{
				// IL decided to use a different format for the stash files.
				sc.size = TQData.ReadKeyInt(reader, "numItems");
			}
			else if (sc.SackType == SackType.Equipment)
			{
				if (sc.IsImmortalThrone)
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
				sc.beginBlockCrap = TQData.ReadKeyInt(reader, "begin_block"); // make sure we just read a new block

				sc.tempBool = TQData.ReadKeyInt(reader, "tempBool");

				sc.size = TQData.ReadKeyInt(reader, "size");
			}

			sc.items = new List<Item>(sc.size);

			Item? prevItem = null;
			for (int i = 0; i < sc.size; ++i)
			{
				// Additional logic to decode the weapon slots in the equipment section
				if (sc.SackType == SackType.Equipment && (i == 7 || i == 9))
				{
					sc.beginBlockCrap = TQData.ReadKeyInt(reader, "begin_block");

					// Eat the alternate tag and flag
					TQData.ReadKeyInt(reader, "alternate");
				}

				Item item = new Item();
				item.Place.SackType = sc.SackType;
				ItemProvider.Parse(item, reader);

				// Stack sc item with the previous item if necessary
				if ((prevItem is not null) && item.DoesStack && (item.PositionX == -1) && (item.PositionY == -1))
					prevItem.StackSize++;
				else
				{
					prevItem = item;
					sc.items.Add(item);
					if (sc.SackType == SackType.Equipment)
					{
						// Get the item location from the table
						item.PositionX = SackCollection.GetEquipmentLocationOffset(i).X;
						item.PositionY = SackCollection.GetEquipmentLocationOffset(i).Y;

						// Eat the itemAttached tag and flag
						TQData.ReadKeyInt(reader, "itemAttached");
					}
				}

				// Additional logic to decode the weapon slots in the equipment section
				if (sc.SackType == SackType.Equipment && (i == 8 || i == 10))
				{
					sc.endBlockCrap = TQData.ReadKeyInt(reader, "end_block");
				}
			}

			sc.endBlockCrap = TQData.ReadKeyInt(reader, "end_block");
		}
		catch (ArgumentException ex)
		{
			// The ValidateNextString Method can throw an ArgumentException.
			// We just pass it along at sc point.
			Log.LogDebug(ex, "ValidateNextString fail !");
			throw;
		}
	}

}