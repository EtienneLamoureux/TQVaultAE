//-----------------------------------------------------------------------
// <copyright file="Item.cs" company="None">
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
			{
				TQData.WriteCString(writer, "stackCount");
				writer.Write(itemCount - 1);
			}

			TQData.WriteCString(writer, "begin_block");
			writer.Write(itm.beginBlockCrap2);

			TQData.WriteCString(writer, "baseName");
			TQData.WriteCString(writer, itm.BaseItemId.Raw);

			TQData.WriteCString(writer, "prefixName");
			TQData.WriteCString(writer, itm.prefixID.Raw);

			TQData.WriteCString(writer, "suffixName");
			TQData.WriteCString(writer, itm.suffixID.Raw);

			TQData.WriteCString(writer, "relicName");
			TQData.WriteCString(writer, itm.relicID.Raw);

			TQData.WriteCString(writer, "relicBonus");
			TQData.WriteCString(writer, itm.RelicBonusId.Raw);

			TQData.WriteCString(writer, "seed");
			writer.Write(cseed);

			TQData.WriteCString(writer, "var1");
			writer.Write(itm.Var1);

			if (itm.atlantis)
			{
				TQData.WriteCString(writer, "relicName2");
				TQData.WriteCString(writer, itm.relic2ID.Raw);

				TQData.WriteCString(writer, "relicBonus2");
				TQData.WriteCString(writer, itm.RelicBonus2Id.Raw);

				TQData.WriteCString(writer, "var2");
				writer.Write(itm.Var2);
			}

			TQData.WriteCString(writer, "end_block");
			writer.Write(itm.endBlockCrap2);

			if (itm.Place.SackType == SackType.Stash)
			{
				TQData.WriteCString(writer, "xOffset");
				writer.Write(Convert.ToSingle(cx, CultureInfo.InvariantCulture));

				TQData.WriteCString(writer, "yOffset");
				writer.Write(Convert.ToSingle(cy, CultureInfo.InvariantCulture));
			}
		}
		else
		{
			// itm is a sack
			// enter a while() loop so we can print out each potion in the stack if it is a potion stack
			while (true)
			{
				TQData.WriteCString(writer, "begin_block");
				writer.Write(itm.beginBlockCrap1);

				TQData.WriteCString(writer, "begin_block");
				writer.Write(itm.beginBlockCrap2);

				TQData.WriteCString(writer, "baseName");
				TQData.WriteCString(writer, itm.BaseItemId.Raw);

				TQData.WriteCString(writer, "prefixName");
				TQData.WriteCString(writer, itm.prefixID.Raw);

				TQData.WriteCString(writer, "suffixName");
				TQData.WriteCString(writer, itm.suffixID.Raw);

				TQData.WriteCString(writer, "relicName");
				TQData.WriteCString(writer, itm.relicID.Raw);

				TQData.WriteCString(writer, "relicBonus");
				TQData.WriteCString(writer, itm.RelicBonusId.Raw);

				TQData.WriteCString(writer, "seed");
				writer.Write(cseed);

				TQData.WriteCString(writer, "var1");
				writer.Write(itm.Var1);

				if (itm.atlantis)
				{
					TQData.WriteCString(writer, "relicName2");
					TQData.WriteCString(writer, itm.relic2ID.Raw);

					TQData.WriteCString(writer, "relicBonus2");
					TQData.WriteCString(writer, itm.RelicBonus2Id.Raw);

					TQData.WriteCString(writer, "var2");
					writer.Write(itm.Var2);
				}

				TQData.WriteCString(writer, "end_block");
				writer.Write(itm.endBlockCrap2);

				TQData.WriteCString(writer, "pointX");
				writer.Write(cx);

				TQData.WriteCString(writer, "pointY");
				writer.Write(cy);

				TQData.WriteCString(writer, "end_block");
				writer.Write(itm.endBlockCrap1);

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
				TQData.ValidateNextString("stackCount", reader);
				itm.beginBlockCrap1 = reader.ReadInt32();
			}
			else if (itm.Place.SackType == SackType.Player)
			{
				TQData.ValidateNextString("begin_block", reader); // make sure we just read a new block
				itm.beginBlockCrap1 = reader.ReadInt32();
			}

			TQData.ValidateNextString("begin_block", reader); // make sure we just read a new block
			itm.beginBlockCrap2 = reader.ReadInt32();

			TQData.ValidateNextString("baseName", reader);
			itm.BaseItemId = TQData.ReadCString(reader);

			TQData.ValidateNextString("prefixName", reader);
			itm.prefixID = TQData.ReadCString(reader);

			TQData.ValidateNextString("suffixName", reader);
			itm.suffixID = TQData.ReadCString(reader);

			TQData.ValidateNextString("relicName", reader);
			itm.relicID = TQData.ReadCString(reader);

			TQData.ValidateNextString("relicBonus", reader);
			itm.RelicBonusId = TQData.ReadCString(reader);

			TQData.ValidateNextString("seed", reader);
			itm.Seed = reader.ReadInt32();

			TQData.ValidateNextString("var1", reader);
			itm.Var1 = reader.ReadInt32();

			if (TQData.MatchNextString("relicName2", reader))
			{
				TQData.ValidateNextString("relicName2", reader);
				itm.relic2ID = TQData.ReadCString(reader);
				itm.atlantis = true;
			}

			if (itm.atlantis)
			{
				TQData.ValidateNextString("relicBonus2", reader);
				itm.RelicBonus2Id = TQData.ReadCString(reader);

				TQData.ValidateNextString("var2", reader);
				itm.Var2 = reader.ReadInt32();
			}
			else
			{
				itm.relic2ID = RecordId.Empty;
				itm.RelicBonus2Id = RecordId.Empty;
				itm.Var2 = Item.var2Default;
			}

			TQData.ValidateNextString("end_block", reader);
			itm.endBlockCrap2 = reader.ReadInt32();

			if (itm.Place.SackType == SackType.Stash)
			{
				TQData.ValidateNextString("xOffset", reader);
				itm.PositionX = Convert.ToInt32(reader.ReadSingle(), CultureInfo.InvariantCulture);

				TQData.ValidateNextString("yOffset", reader);
				itm.PositionY = Convert.ToInt32(reader.ReadSingle(), CultureInfo.InvariantCulture);
			}
			else if (itm.Place.SackType == SackType.Equipment)
			{
				// Initially set the coordinates to (0, 0)
				itm.PositionX = 0;
				itm.PositionY = 0;
			}
			else
			{
				TQData.ValidateNextString("pointX", reader);
				itm.PositionX = reader.ReadInt32();

				TQData.ValidateNextString("pointY", reader);
				itm.PositionY = reader.ReadInt32();

				TQData.ValidateNextString("end_block", reader);
				itm.endBlockCrap1 = reader.ReadInt32();
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
