//-----------------------------------------------------------------------
// <copyright file="ItemProvider.Serialization.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using System.Globalization;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Data;

public partial class ItemProvider
{


	/// <summary>
	/// Encodes an item into the save file format
	/// </summary>
	/// <param name="writer">BinaryWriter instance</param>
	public void Encode(Item itm, BinaryWriter writer)
	{
		int itemCount = itm.StackSize;
		int cx = itm.PositionX;
		int cy = itm.PositionY;
		int cseed = itm.Seed;

		if (itm.Place.SackType != SackType.Player)
		{
			// Equipment, Stashes, Vaults
			if (itm.Place.SackType == SackType.Stash)
				TQData.WriteKeyInt(writer, "stackCount", itemCount - 1);

			this.WriteItemRecord(itm, writer, cseed);

			if (itm.Place.SackType == SackType.Stash)
			{
				TQData.WriteKeyFloat(writer, "xOffset", Convert.ToSingle(cx, CultureInfo.InvariantCulture));

				TQData.WriteKeyFloat(writer, "yOffset", Convert.ToSingle(cy, CultureInfo.InvariantCulture));
			}
		}
		else
		{
			// itm is a sack
			// enter a while() loop so we can print out each potion in the stack if it is a potion stack
			while (true)
			{
				TQData.WriteKeyInt(writer, "begin_block", itm.beginBlockCrap1);

				this.WriteItemRecord(itm, writer, cseed);

				TQData.WriteKeyInt(writer, "pointX", cx);

				TQData.WriteKeyInt(writer, "pointY", cy);

				TQData.WriteKeyInt(writer, "end_block", itm.endBlockCrap1);

				if (!itm.DoesStack)
					return;

				if (itemCount <= 1)
					return;

				// we have more items in the stack to write().
				--itemCount;
				cx = -1;
				cy = -1;
				cseed = Item.GenerateSeed(); // get a new seed for the next potion
			}
		}
	}

	/// <summary>
	/// Writes the item record itself, from its <c>begin_block</c> to its <c>end_block</c>: the same in every container
	/// (the stash, the equipment, a vault or a character's sack add their own fields around it).
	/// </summary>
	/// <param name="seed">Seed to write: the item's, or a new one for the copies of a potion stack.</param>
	private void WriteItemRecord(Item itm, BinaryWriter writer, int seed)
	{
		TQData.WriteKeyInt(writer, "begin_block", itm.beginBlockCrap2);

		TQData.WriteKeyString(writer, "baseName", RecordId.RawOf(itm.BaseItemId));

		TQData.WriteKeyString(writer, "prefixName", RecordId.RawOf(itm.prefixID));

		TQData.WriteKeyString(writer, "suffixName", RecordId.RawOf(itm.suffixID));

		TQData.WriteKeyString(writer, "relicName", RecordId.RawOf(itm.relicID));

		TQData.WriteKeyString(writer, "relicBonus", RecordId.RawOf(itm.RelicBonusId));

		TQData.WriteKeyInt(writer, "seed", seed);

		TQData.WriteKeyInt(writer, "var1", itm.Var1);

		if (itm.atlantis)
		{
			TQData.WriteKeyString(writer, "relicName2", RecordId.RawOf(itm.relic2ID));

			TQData.WriteKeyString(writer, "relicBonus2", RecordId.RawOf(itm.RelicBonus2Id));

			TQData.WriteKeyInt(writer, "var2", itm.Var2);
		}

		TQData.WriteKeyInt(writer, "end_block", itm.endBlockCrap2);
	}

	/// <summary>
	/// Parses an item from the save file format
	/// </summary>
	/// <param name="reader">BinaryReader instance</param>
	public void Parse(Item itm, BinaryReader reader)
	{
		try
		{
			string valueStr = string.Empty;
			if (itm.Place.SackType == SackType.Stash)
			{
				itm.beginBlockCrap1 = TQData.ReadKeyInt(reader, "stackCount");
			}
			else if (itm.Place.SackType == SackType.Player)
			{
				itm.beginBlockCrap1 = TQData.ReadKeyInt(reader, "begin_block"); // make sure we just read a new block
			}

			itm.beginBlockCrap2 = TQData.ReadKeyInt(reader, "begin_block"); // make sure we just read a new block

			itm.BaseItemId = TQData.ReadKeyString(reader, "baseName");

			itm.prefixID = TQData.ReadKeyString(reader, "prefixName");

			itm.suffixID = TQData.ReadKeyString(reader, "suffixName");

			itm.relicID = TQData.ReadKeyString(reader, "relicName");

			itm.RelicBonusId = TQData.ReadKeyString(reader, "relicBonus");

			itm.Seed = TQData.ReadKeyInt(reader, "seed");

			itm.Var1 = TQData.ReadKeyInt(reader, "var1");

			if (TQData.MatchNextString("relicName2", reader))
			{
				itm.relic2ID = TQData.ReadKeyString(reader, "relicName2");
				itm.atlantis = true;
			}

			if (itm.atlantis)
			{
				itm.RelicBonus2Id = TQData.ReadKeyString(reader, "relicBonus2");

				itm.Var2 = TQData.ReadKeyInt(reader, "var2");
			}
			else
			{
				itm.relic2ID = RecordId.Empty;
				itm.RelicBonus2Id = RecordId.Empty;
				itm.Var2 = Item.var2Default;
			}

			itm.endBlockCrap2 = TQData.ReadKeyInt(reader, "end_block");

			if (itm.Place.SackType == SackType.Stash)
			{
				itm.PositionX = Convert.ToInt32(TQData.ReadKeyFloat(reader, "xOffset"), CultureInfo.InvariantCulture);

				itm.PositionY = Convert.ToInt32(TQData.ReadKeyFloat(reader, "yOffset"), CultureInfo.InvariantCulture);
			}
			else if (itm.Place.SackType == SackType.Equipment)
			{
				// Initially set the coordinates to (0, 0)
				itm.PositionX = 0;
				itm.PositionY = 0;
			}
			else
			{
				itm.PositionX = TQData.ReadKeyInt(reader, "pointX");

				itm.PositionY = TQData.ReadKeyInt(reader, "pointY");

				itm.endBlockCrap1 = TQData.ReadKeyInt(reader, "end_block");
			}

			GetDBData(itm);

			if (itm.Place.SackType == SackType.Stash)
				itm.StackSize = itm.beginBlockCrap1 + 1;
			else
				itm.StackSize = 1;
		}
		catch (ArgumentException ex)
		{
			// The ValidateNextString Method can throw an ArgumentException.
			// We just pass it along at itm point.
			Log.LogDebug(ex, "ValidateNextString() fail !");
			throw;
		}
	}
}
