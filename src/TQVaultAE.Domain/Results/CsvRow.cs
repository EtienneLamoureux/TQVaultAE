using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Domain.Results;

public class CsvRow
{
	private readonly string vaultname;
	private readonly int bagid;
	private readonly ToFriendlyNameResult fnr;
	private readonly int rowIndex;
	private readonly char csvDelimiter;

	public CsvRow(string vaultname, int bagid, ToFriendlyNameResult fnr, int rowIndex, char csvDelimiter)
	{
		this.vaultname = vaultname;
		this.bagid = bagid;
		this.fnr = fnr;
		this.rowIndex = rowIndex;
		this.csvDelimiter = csvDelimiter;
	}

	public static string GetCSVHeader(char csvDelimiter)
		=> string.Join(csvDelimiter.ToString()
			, @"Row"
			, @"Vault"
			, @"BagId"
			, @"PosX"
			, @"PosY"

			, @"ItemClass"
			, @"ItemOrigin"
			, @"ItemSeed"

			, @"RequireLvl"
			, @"RequireStr"
			, @"RequireDex"
			, @"RequireInt"

			, @"BaseRarity"
			, @"BaseStyle"
			, @"BaseQuality"
			, @"BaseId"
			, @"BaseName"

			, @"PrefixId"
			, @"PrefixName"

			, @"SuffixId"
			, @"SuffixName"

			, @"RelicId"
			, @"RelicName"
			, @"RelicBonusId"
			, @"RelicVar"

			, @"Relic2Id"
			, @"Relic2Name"
			, @"Relic2BonusId"
			, @"Relic2Var"
			);
	public override string ToString()
	{
		return string.Join(csvDelimiter.ToString()
			, rowIndex // @"Row"
			, vaultname // @"Vault"
			, bagid // @"BagId"
			, fnr.Item.PositionX // @"PosX"
			, fnr.Item.PositionY // @"PosY"

			, fnr.Item.ItemClass //  @"ItemClass"
			, fnr.Item.GameExtensionCode // @"ItemOrigin"
			, fnr.Item.Seed // @"ItemSeed"

			, fnr.RequirementInfo.Lvl // @"RequireLvl"
			, fnr.RequirementInfo.Str // @"RequireStr"
			, fnr.RequirementInfo.Dex // @"RequireDex"
			, fnr.RequirementInfo.Int // @"RequireInt"

			, fnr.BaseItemRarity
			, fnr.BaseItemInfoStyle
			, fnr.BaseItemInfoQuality
			, fnr.Item.BaseItemId // @"BaseId"
			, fnr.BaseItemInfoDescription.RemoveAllTQTags() // @"BaseName"

			, fnr.Item.prefixID // @"PrefixId"
			, fnr.PrefixInfoDescription.RemoveAllTQTags() // @"PrefixName"

			, fnr.Item.suffixID // @"SuffixId"
			, fnr.SuffixInfoDescription.RemoveAllTQTags() // @"SuffixName"

			, fnr.Item.relicID // @"RelicId"
			, fnr.RelicInfo1Description.RemoveAllTQTags() // @"RelicName"
			, fnr.Item.RelicBonusId // @"RelicBonusId"
			, fnr.Item.Var1 // @"RelicVar"

			, fnr.Item.relic2ID // @"Relic2Id"
			, fnr.RelicInfo2Description.RemoveAllTQTags() // @"Relic2Name"
			, fnr.Item.RelicBonus2Id // @"Relic2BonusId"
			, fnr.Item.Var2 // @"Relic2Var"
			);
	}
}