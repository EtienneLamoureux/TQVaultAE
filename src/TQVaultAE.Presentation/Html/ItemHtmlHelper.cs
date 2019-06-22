using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TQVaultAE.Config;
using TQVaultAE.Data;
using TQVaultAE.Entities;
using TQVaultAE.Logs;

namespace TQVaultAE.Presentation.Html
{
	public static class ItemHtmlHelper
	{
		private static readonly log4net.ILog Log = Logger.Get(typeof(ItemHtmlHelper));

		/// <summary>
		/// Gets the item's attributes
		/// </summary>
		/// <param name="filtering">Flag indicating whether or not we are filtering strings</param>
		/// <returns>returns a string containing the item's attributes</returns>
		public static string GetAttributes(Item itm, bool filtering)
		{
			if (itm.attributesString != null)
			{
				return itm.attributesString;
			}

			List<string> results = new List<string>();

			// Add the item name
			string itemName = HtmlHelper.MakeSafeForHtml(ItemProvider.ToFriendlyName(itm, true, false));
			Color color = ItemGfxHelper.GetColorTag(itm, itemName);
			itemName = Item.ClipColorTag(itemName);
			results.Add(string.Format(CultureInfo.CurrentCulture, "<font size={0} color={1}><b>{2}</b></font>", Convert.ToInt32(10.0F * UIService.UI.Scale), HtmlHelper.HtmlColor(color), itemName));

			// Add the sub-title for certain types
			if (itm.baseItemInfo != null)
			{
				if (itm.IsRelic)
				{
					string str;
					if (!itm.IsRelicComplete)
					{
						string tag1 = "tagRelicShard";
						string tag2 = "tagRelicRatio";
						if (itm.IsCharm)
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
							formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
						}

						str = ItemProvider.Format(formatSpec, type, itm.Number, itm.baseItemInfo.CompletedRelicLevel);
					}
					else
					{
						string tag = "tagRelicComplete";
						if (itm.IsCharm)
						{
							tag = "tagAnimalPartComplete";
						}

						str = Database.DB.GetFriendlyName(tag);
						if (str == null)
						{
							str = "?Completed Relic/Charm?";
						}
					}

					str = HtmlHelper.MakeSafeForHtml(str);
					Color color1 = ItemGfxHelper.GetColorTag(itm, str);
					str = Item.ClipColorTag(str);
					results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", HtmlHelper.HtmlColor(color1), str));
				}
				else if (itm.IsArtifact)
				{
					// Added by VillageIdiot
					// Show Artifact Class (Lesser / Greater / Divine).
					string tag;
					string artifactClass;
					string artifactClassification = itm.baseItemInfo.GetString("artifactClassification").ToUpperInvariant();

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

					artifactClass = HtmlHelper.MakeSafeForHtml(artifactClass);
					results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Broken)), artifactClass));
				}

				// Added by VillageIdiot
				// Show Formulae Reagents.
				if (itm.IsFormulae)
				{
					string str;
					string tag = "xtagArtifactRecipe";

					// Added to show recipe type for Formulae
					string recipe = Database.DB.GetFriendlyName(tag);
					if (recipe == null)
					{
						recipe = "Recipe";
					}

					results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font><br>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Mundane)), recipe));

					tag = "xtagArtifactReagents";

					// Get Reagents format
					string formatSpec = Database.DB.GetFriendlyName(tag);
					if (formatSpec == null)
					{
						formatSpec = "Required Reagents  ({0}/{1})";
					}
					else
					{
						formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
					}

					// it looks like the formulae reagents is hard coded at 3
					str = ItemProvider.Format(formatSpec, (object)0, 3);
					str = HtmlHelper.MakeSafeForHtml(str);
					results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Relic)), str));
				}
			}

			// Add flavor text
			// Changed by VillageIdiot
			// Removed Scroll flavor text since it gets printed by the skill effect code
			if ((itm.IsQuestItem || itm.IsRelic || itm.IsPotion || itm.IsParchment || itm.IsScroll) && itm.baseItemInfo != null && itm.baseItemInfo.StyleTag.Length > 0)
			{
				string tag = itm.baseItemInfo.StyleTag;
				string flavor = Database.DB.GetFriendlyName(tag);
				if (flavor != null)
				{
					flavor = HtmlHelper.MakeSafeForHtml(flavor);
					Collection<string> flavorTextArray = Database.WrapWords(flavor, 30);

					foreach (string flavorTextRow in flavorTextArray)
					{
						int nextColor = flavorTextRow.IndexOf("{^y}", StringComparison.OrdinalIgnoreCase);
						if (nextColor > -1)
						{
							string choppedString = flavorTextRow.Substring(4);
							results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(TQColor.Yellow)), choppedString));
						}
						else
						{
							results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Mundane)), flavorTextRow));
						}
					}

					results.Add(string.Empty);
				}
			}

			if (itm.baseItemInfo != null)
			{
				GetAttributesFromRecord(itm, Database.DB.GetRecordFromFile(itm.BaseItemId), filtering, itm.BaseItemId, results);
			}

			if (itm.prefixInfo != null)
			{
				GetAttributesFromRecord(itm, Database.DB.GetRecordFromFile(itm.prefixID), filtering, itm.prefixID, results);
			}

			if (itm.suffixInfo != null)
			{
				GetAttributesFromRecord(itm, Database.DB.GetRecordFromFile(itm.suffixID), filtering, itm.suffixID, results);
			}

			if (itm.RelicInfo != null)
			{
				List<string> r = new List<string>();
				GetAttributesFromRecord(itm, Database.DB.GetRecordFromFile(itm.relicID), filtering, itm.relicID, r);
				if (r.Count > 0)
				{
					string colorTag = string.Format(CultureInfo.CurrentCulture, "<hr color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Broken)));

					string relicName = HtmlHelper.MakeSafeForHtml(ItemProvider.ToFriendlyName(itm, false, true));

					// display the relic name
					results.Add(string.Format(
						CultureInfo.CurrentUICulture,
						"{2}<font size=+1 color={0}><b>{1}</b></font>",
						HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Relic)),
						relicName,
						colorTag));

					// display the relic subtitle
					string str;
					if (itm.Var1 < itm.RelicInfo.CompletedRelicLevel)
					{
						string tag1 = "tagRelicShard";
						string tag2 = "tagRelicRatio";
						if (itm.RelicInfo.ItemClass.ToUpperInvariant().Equals("ITEMCHARM"))
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
							formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
						}

						str = ItemProvider.Format(formatSpec, type, Math.Max(1, itm.Var1), itm.RelicInfo.CompletedRelicLevel);
					}
					else
					{
						string tag = "tagRelicComplete";
						if (itm.RelicInfo.ItemClass.ToUpperInvariant().Equals("ITEMCHARM"))
						{
							tag = "tagAnimalPartComplete";
						}

						str = Database.DB.GetFriendlyName(tag);
						if (str == null)
						{
							str = "?Completed Relic/Charm?";
						}
					}

					str = HtmlHelper.MakeSafeForHtml(str);
					results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Relic)), str));

					// display the relic bonuses
					results.AddRange(r);
				}
			}

			// Added by VillageIdiot
			// Show the Artifact completion bonus.
			if (itm.IsArtifact && itm.RelicBonusInfo != null)
			{
				List<string> r = new List<string>();
				GetAttributesFromRecord(itm, Database.DB.GetRecordFromFile(itm.RelicBonusId), filtering, itm.RelicBonusId, r);
				if (r.Count > 0)
				{
					string tag = "xtagArtifactBonus";
					string bonusTitle = Database.DB.GetFriendlyName(tag);
					if (bonusTitle == null)
					{
						bonusTitle = "Completion Bonus: ";
					}

					string title = HtmlHelper.MakeSafeForHtml(bonusTitle);

					results.Add(string.Empty);
					results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Relic)), title));
					results.AddRange(r);
				}
			}

			if ((itm.IsRelic || itm.HasRelic) && itm.RelicBonusInfo != null)
			{
				List<string> r = new List<string>();
				GetAttributesFromRecord(itm, Database.DB.GetRecordFromFile(itm.RelicBonusId), filtering, itm.RelicBonusId, r);
				if (r.Count > 0)
				{
					string tag = "tagRelicBonus";
					if (itm.IsCharm || (itm.HasRelic && itm.RelicInfo.ItemClass.ToUpperInvariant().Equals("ITEMCHARM")))
					{
						tag = "tagAnimalPartcompleteBonus";
					}

					string bonusTitle = Database.DB.GetFriendlyName(tag);
					if (bonusTitle == null)
					{
						bonusTitle = "?Completed Relic/Charm Bonus:?";
					}

					string title = HtmlHelper.MakeSafeForHtml(bonusTitle);

					results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Relic)), title));
					results.AddRange(r);
				}
			}

			if (itm.Relic2Info != null)
			{
				List<string> r = new List<string>();
				GetAttributesFromRecord(itm, Database.DB.GetRecordFromFile(itm.relic2ID), filtering, itm.relic2ID, r);
				if (r.Count > 0)
				{
					string colorTag = string.Format(CultureInfo.CurrentCulture, "<hr color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Broken)));

					string relicName = HtmlHelper.MakeSafeForHtml(ItemProvider.ToFriendlyName(itm, false, true, true));

					// display the relic name
					results.Add(string.Format(
						CultureInfo.CurrentUICulture,
						"{2}<font size=+1 color={0}><b>{1}</b></font>",
						HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Relic)),
						relicName,
						colorTag));

					// display the relic subtitle
					string str;
					if (itm.Var2 < itm.Relic2Info.CompletedRelicLevel)
					{
						string tag1 = "tagRelicShard";
						string tag2 = "tagRelicRatio";
						if (itm.Relic2Info.ItemClass.ToUpperInvariant().Equals("ITEMCHARM"))
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
							formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
						}

						str = ItemProvider.Format(formatSpec, type, Math.Max(1, itm.Var2), itm.Relic2Info.CompletedRelicLevel);
					}
					else
					{
						string tag = "tagRelicComplete";
						if (itm.Relic2Info.ItemClass.ToUpperInvariant().Equals("ITEMCHARM"))
						{
							tag = "tagAnimalPartComplete";
						}

						str = Database.DB.GetFriendlyName(tag);
						if (str == null)
						{
							str = "?Completed Relic/Charm?";
						}
					}

					str = HtmlHelper.MakeSafeForHtml(str);
					results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Relic)), str));

					// display the relic bonuses
					results.AddRange(r);
				}

				if (itm.HasSecondRelic && (itm.RelicBonus2Info != null))
				{
					r = new List<string>();
					GetAttributesFromRecord(itm, Database.DB.GetRecordFromFile(itm.RelicBonus2Id), filtering, itm.RelicBonus2Id, r);
					if (r.Count > 0)
					{
						string tag = "tagRelicBonus";
						if (itm.IsCharm || (itm.HasRelic && itm.Relic2Info.ItemClass.ToUpperInvariant().Equals("ITEMCHARM")))
						{
							tag = "tagAnimalPartcompleteBonus";
						}

						string bonusTitle = Database.DB.GetFriendlyName(tag);
						if (bonusTitle == null)
						{
							bonusTitle = "?Completed Relic/Charm Bonus:?";
						}

						string title = HtmlHelper.MakeSafeForHtml(bonusTitle);

						results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Relic)), title));
						results.AddRange(r);
					}
				}
			}

			// Added by VillageIdiot
			// Shows Artifact stats for the formula
			if (itm.IsFormulae && itm.baseItemInfo != null)
			{
				List<string> r = new List<string>();
				string artifactID = itm.baseItemInfo.GetString("artifactName");
				DBRecordCollection artifactRecord = Database.DB.GetRecordFromFile(artifactID);
				if (artifactID != null)
				{
					GetAttributesFromRecord(itm, artifactRecord, filtering, artifactID, r);
					if (r.Count > 0)
					{
						string tag;

						// Display the name of the Artifact
						string artifactClass = Database.DB.GetFriendlyName(artifactRecord.GetString("description", 0));
						if (string.IsNullOrEmpty(artifactClass))
						{
							artifactClass = "?Unknown Artifact Name?";
						}

						string artifactName = HtmlHelper.MakeSafeForHtml(artifactClass);
						results.Add(string.Empty);
						results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Artifact)), artifactName));

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

						artifactClass = HtmlHelper.MakeSafeForHtml(artifactClass);
						results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Broken)), artifactClass));

						// Add the stats
						results.AddRange(r);
					}
				}
			}

			// Add the item seed
			string hr1 = string.Format(CultureInfo.CurrentCulture, "<hr color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Broken)));
			string itemSeedString = HtmlHelper.MakeSafeForHtml(string.Format(CultureInfo.CurrentCulture, Item.ItemSeed, itm.Seed, (itm.Seed != 0) ? (itm.Seed / (float)Int16.MaxValue) : 0.0f));
			results.Add(string.Format(CultureInfo.CurrentCulture, "{2}<font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Broken)), itemSeedString, hr1));

			// Add the Immortal Throne clause
			if (itm.IsImmortalThrone)
			{
				string immortalThrone = HtmlHelper.MakeSafeForHtml(Item.ItemIT);
				results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Rare)), immortalThrone));
			}

			// Add the Ragnarok clause
			if (itm.IsRagnarok)
			{
				string ragnarok = HtmlHelper.MakeSafeForHtml(Item.ItemRagnarok);
				results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Rare)), ragnarok));
			}

			// Add the Atlantis clause
			if (itm.IsAtlantis)
			{
				string atlantis = HtmlHelper.MakeSafeForHtml(Item.ItemAtlantis);
				results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Rare)), atlantis));
			}

			string[] ary = new string[results.Count];
			results.CopyTo(ary);
			itm.attributesString = string.Join("<br>", ary);

			return itm.attributesString;
		}

		/// <summary>
		/// Shows the bare attributes for properties display
		/// </summary>
		/// <param name="filtering">flag for filtering strings</param>
		/// <returns>string array containing the bare item attributes</returns>
		public static string[] GetBareAttributes(Item itm, bool filtering)
		{
			if (itm.bareAttributes[0] != null)
			{
				return itm.bareAttributes;
			}

			List<string> results = new List<string>();

			if (itm.baseItemInfo != null)
			{
				string style = string.Empty;
				string quality = string.Empty;

				if (itm.baseItemInfo.StyleTag.Length > 0)
				{
					if (!itm.IsPotion && !itm.IsRelic && !itm.IsScroll && !itm.IsParchment && !itm.IsQuestItem)
					{
						style = string.Concat(Database.DB.GetFriendlyName(itm.baseItemInfo.StyleTag), " ");
					}
				}

				if (itm.baseItemInfo.QualityTag.Length > 0)
				{
					quality = string.Concat(Database.DB.GetFriendlyName(itm.baseItemInfo.QualityTag), " ");
				}

				results.Add(string.Format(
					CultureInfo.CurrentCulture,
					"<font color={0}>{1}</font>",
					HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Rare)),
					string.Concat(style, quality, Database.DB.GetFriendlyName(itm.baseItemInfo.DescriptionTag))));

				results.Add(string.Format(
					CultureInfo.CurrentCulture,
					"<font color={0}>{1}</font>",
					HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Relic)),
					itm.BaseItemId));

				GetAttributesFromRecord(itm, Database.DB.GetRecordFromFile(itm.BaseItemId), filtering, itm.BaseItemId, results, false);
			}

			if (results != null)
			{
				string[] ary = new string[results.Count];
				results.CopyTo(ary);
				itm.bareAttributes[0] = string.Join("<br>", ary);
				results.Clear();
			}
			else
			{
				itm.bareAttributes[0] = string.Empty;
			}

			if (itm.prefixInfo != null)
			{
				results.Add(string.Format(
					CultureInfo.CurrentCulture,
					"<font color={0}>{1}</font>",
					HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Rare)),
					Database.DB.GetFriendlyName(itm.prefixInfo.DescriptionTag)));

				results.Add(string.Format(
					CultureInfo.CurrentCulture,
					"<font color={0}>{1}</font>",
					HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Relic)),
					itm.prefixID));

				GetAttributesFromRecord(itm, Database.DB.GetRecordFromFile(itm.prefixID), filtering, itm.prefixID, results, false);
			}

			if (results != null)
			{
				string[] ary = new string[results.Count];
				results.CopyTo(ary);
				itm.bareAttributes[2] = string.Join("<br>", ary);
				results.Clear();
			}
			else
			{
				itm.bareAttributes[2] = string.Empty;
			}

			if (itm.suffixInfo != null)
			{
				results.Add(string.Format(
					CultureInfo.CurrentCulture,
					"<font color={0}>{1}</font>",
					HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Rare)),
					Database.DB.GetFriendlyName(itm.suffixInfo.DescriptionTag)));

				results.Add(string.Format(
					CultureInfo.CurrentCulture,
					"<font color={0}>{1}</font>",
					HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Relic)),
					itm.suffixID));

				GetAttributesFromRecord(itm, Database.DB.GetRecordFromFile(itm.suffixID), filtering, itm.suffixID, results, false);
			}

			if (results != null)
			{
				string[] ary = new string[results.Count];
				results.CopyTo(ary);
				itm.bareAttributes[3] = string.Join("<br>", ary);
				results.Clear();
			}
			else
			{
				itm.bareAttributes[3] = string.Empty;
			}

			return itm.bareAttributes;
		}

		/// <summary>
		/// Shows the items in a set for the set items
		/// </summary>
		/// <returns>string containing the set items</returns>
		public static string GetItemSetString(Item itm)
		{
			if (itm.setItemsString != null)
			{
				return itm.setItemsString;
			}

			string[] setMembers = ItemProvider.GetSetItems(itm, true);
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
						results[i++] = string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Rare)), name);
					}
					else
					{
						Info info = Database.DB.GetInfo(s);
						name = string.Concat("&nbsp;&nbsp;&nbsp;&nbsp;", Database.DB.GetFriendlyName(info.DescriptionTag));
						results[i++] = string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Common)), name);
					}
				}

				itm.setItemsString = string.Join("<br>", results);
			}
			else
			{
				itm.setItemsString = string.Empty;
			}

			return itm.setItemsString;
		}

		/// <summary>
		/// Gets the item's requirements
		/// </summary>
		/// <returns>A string containing the items requirements</returns>
		public static string GetRequirements(Item itm)
		{
			if (itm.requirementsString != null)
			{
				return itm.requirementsString;
			}
			SortedList<string, Variable> requirementsList = ItemProvider.GetRequirementVariables(itm);

			// Get the format string to use to list a requirement
			string requirementFormat = Database.DB.GetFriendlyName("MeetsRequirement");
			if (requirementFormat == null)
			{
				// could not find one.  make up one.
				requirementFormat = "?Required? {0}: {1:f0}";
			}
			else
			{
				requirementFormat = ItemAttributeProvider.ConvertFormat(requirementFormat);
			}

			// Now combine it all with spaces between
			List<string> requirements = new List<string>();
			foreach (KeyValuePair<string, Variable> kvp in requirementsList)
			{
				if (TQDebug.ItemDebugLevel > 1)
				{
					Log.DebugFormat(CultureInfo.InvariantCulture, "Retrieving requirement {0}={1} (type={2})", kvp.Key, kvp.Value, kvp.Value.GetType().ToString());
				}

				Variable variable = kvp.Value;

				// Format the requirement
				string requirementsText;
				if (variable.NumberOfValues > 1 || variable.DataType == VariableDataType.StringVar)
				{
					// reqs should only have 1 entry and should be a number type.  We must punt on itm one
					requirementsText = string.Concat(kvp.Key, ": ", variable.ToStringValue());
				}
				else
				{
					// get the name of itm requirement
					string reqName = Database.DB.GetFriendlyName(kvp.Key);
					if (reqName == null)
					{
						reqName = string.Concat("?", kvp.Key, "?");
					}

					// Now apply the format string
					requirementsText = ItemProvider.Format(requirementFormat, reqName, variable[0]);
				}

				// Changed by VillageIdiot - Change requirement text to Grey
				requirementsText = string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Broken)), HtmlHelper.MakeSafeForHtml(requirementsText));
				requirements.Add(requirementsText);
			}

			// Now convert array of strings to a single string with <br>'s
			if (requirements.Count > 0)
			{
				string[] requirementsArray = new string[requirements.Count];
				requirements.CopyTo(requirementsArray);
				itm.requirementsString = string.Join("<br>", requirementsArray);
			}
			else
			{
				itm.requirementsString = string.Empty;
			}

			return itm.requirementsString;
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
				font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Legendary)));
			}

			// now get the formatSpec
			string formatSpec = Database.DB.GetFriendlyName("ItemMasteryIncrement");
			if (string.IsNullOrEmpty(formatSpec))
			{
				formatSpec = "?+{0} to skills in {1}?";
				if (font == null)
				{
					font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Legendary)));
				}
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 2)
				{
					Log.Debug("Item.formatspec (augment mastery) = " + formatSpec);
				}

				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
				if (string.IsNullOrEmpty(font))
				{
					font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Epic)));
				}
			}

			return ItemProvider.Format(formatSpec, variable[0], skillName);
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
				font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Legendary)));
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 2)
				{
					Log.Debug("Item.formatspec (augment level) = " + formatSpec);
				}

				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
				font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Epic)));
			}

			return ItemProvider.Format(formatSpec, variable[System.Math.Min(variable.NumberOfValues - 1, variableNumber)]);
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
							Log.Debug("Item.formatspec (race bonus) = " + formatSpec);
						}

						formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
					}

					if (line != null)
					{
						line = HtmlHelper.MakeSafeForHtml(line);
						if (d.Variable.Length > 0)
						{
							string s = HtmlHelper.MakeSafeForHtml(d.Variable);
							s = string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Legendary)), s);
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

					line = ItemProvider.Format(formatSpec, v[Math.Min(v.NumberOfValues - 1, varNum)], finalRace);
					font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Epic)));
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
					font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Legendary)));
				}
				else
				{
					if (TQDebug.ItemDebugLevel > 2)
					{
						Log.Debug("Item.formatspec (chance of one) = " + formatSpec);
					}

					formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
					font = String.Format(CultureInfo.CurrentCulture, "<font color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Epic)));
				}

				line = ItemProvider.Format(formatSpec, v[System.Math.Min(v.NumberOfValues - 1, varNum)]);
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
				color = HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Legendary));
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 2)
				{
					Log.Debug("Item.formatspec (chance) = " + formatSpec);
				}

				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
			}

			modifierChance = ItemProvider.Format(formatSpec, modifierChanceVar[Math.Min(modifierChanceVar.NumberOfValues - 1, varNum)]);
			modifierChance = HtmlHelper.MakeSafeForHtml(modifierChance);
			if (color != null)
			{
				modifierChance = ItemProvider.Format("<font color={0}>{1}</font>", color, modifierChance);
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
				color = HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Legendary));
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 2)
				{
					Log.Debug("Item.formatspec (improved time) = " + formatSpec);
				}

				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
			}

			durationModifier = ItemProvider.Format(formatSpec, durationModifierVar[Math.Min(durationModifierVar.NumberOfValues - 1, varNum)]);
			durationModifier = HtmlHelper.MakeSafeForHtml(durationModifier);
			if (color != null)
			{
				durationModifier = ItemProvider.Format("<font color={0}>{1}</font>", color, durationModifier);
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
			string tag = ItemAttributeProvider.GetAttributeTextTag(data);
			if (string.IsNullOrEmpty(tag))
			{
				formatSpec = string.Concat("{0:f1}% ?", modifierData.FullAttribute, "?");
				color = HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Legendary));
			}
			else
			{
				formatSpec = Database.DB.GetFriendlyName(tag);
				if (string.IsNullOrEmpty(formatSpec))
				{
					formatSpec = string.Concat("{0:f1}% ?", modifierData.FullAttribute, "?");
					color = HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Legendary));
				}
				else
				{
					if (TQDebug.ItemDebugLevel > 2)
					{
						Log.Debug("Item.formatspec (percent) = " + formatSpec);
					}

					formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
				}
			}

			modifier = ItemProvider.Format(formatSpec, modifierVar[Math.Min(modifierVar.NumberOfValues - 1, varNum)]);
			modifier = HtmlHelper.MakeSafeForHtml(modifier);
			if (!string.IsNullOrEmpty(color))
			{
				modifier = ItemProvider.Format("<font color={0}>{1}</font>", color, modifier);
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
				color = HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Legendary));
			}

			if (TQDebug.ItemDebugLevel > 2)
			{
				Log.Debug("Item.formatspec (chance) = " + formatSpec);
			}

			formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
			if (chanceVar != null)
			{
				chance = ItemProvider.Format(formatSpec, chanceVar[Math.Min(chanceVar.NumberOfValues - 1, varNum)]);
				chance = HtmlHelper.MakeSafeForHtml(chance);
				if (!string.IsNullOrEmpty(color))
				{
					chance = ItemProvider.Format("<font color={0}>{1}</font>", color, chance);
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
				color = HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Legendary));
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 2)
				{
					Log.Debug("Item.formatspec (percent) = " + formatSpec);
				}

				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
			}

			damageRatio = ItemProvider.Format(formatSpec, damageRatioVar[Math.Min(damageRatioVar.NumberOfValues - 1, varNum)]);
			damageRatio = HtmlHelper.MakeSafeForHtml(damageRatio);
			if (!string.IsNullOrEmpty(color))
			{
				damageRatio = ItemProvider.Format("<font color={0}>{1}</font>", color, damageRatio);
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
				color = HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Legendary));
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 2)
				{
					Log.Debug("Item.formatspec (time single) = " + formatSpec);
				}

				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
			}

			Variable durationVariable = minDurVar;
			if (durationVariable == null)
			{
				durationVariable = maxDurVar;
			}

			if (durationVariable != null)
			{
				duration = ItemProvider.Format(formatSpec, durationVariable[Math.Min(durationVariable.NumberOfValues - 1, varNum)]);
				duration = HtmlHelper.MakeSafeForHtml(duration);
				if (!string.IsNullOrEmpty(color))
				{
					duration = ItemProvider.Format("<font color={0}>{1}</font>", color, duration);
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
				color = HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Legendary));
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 2)
				{
					Log.Debug("Item.formatspec (time range) = " + formatSpec);
				}

				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
			}

			duration = ItemProvider.Format(formatSpec, minDurVar[Math.Min(minDurVar.NumberOfValues - 1, varNum)], maxDurVar[Math.Min(maxDurVar.NumberOfValues - 1, varNum)]);
			duration = HtmlHelper.MakeSafeForHtml(duration);
			if (!string.IsNullOrEmpty(color))
			{
				duration = ItemProvider.Format("<font color={0}>{1}</font>", color, duration);
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
					font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Legendary)));
				}
				else
				{
					if (TQDebug.ItemDebugLevel > 2)
					{
						Log.Debug("Item.formatspec (item skill) = " + formatSpec);
					}

					formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
					font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Epic)));
				}

				line = ItemProvider.Format(formatSpec, variable[0], skillName);
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
						font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Common)));
						line = ItemProvider.Format(formatSpec, reagentName);
					}
				}
			}
			else if (attributeData.FullAttribute.Equals("artifactCreationCost"))
			{
				string formatSpec = Database.DB.GetFriendlyName("xtagArtifactCost");
				if (TQDebug.ItemDebugLevel > 2)
				{
					Log.Debug("Item.formatspec (Artifact cost) = " + formatSpec);
				}

				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
				font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Rare)));
				results.Add(string.Empty);
				line = ItemProvider.Format(formatSpec, string.Format(CultureInfo.CurrentCulture, "{0:N0}", variable[0]));
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
				font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Mundane)));
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
					font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Epic)));
				}
				else
				{
					font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Mundane)));
				}

				line = ItemProvider.Format("{0} {1}", skillName, activationText);
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
				font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Legendary)));
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 2)
				{
					Log.Debug("Item.formatspec (pet bonus) = " + formatSpec);
				}

				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
				font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Relic)));
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
			string labelTag = ItemAttributeProvider.GetAttributeTextTag(baseAttributeData);
			if (string.IsNullOrEmpty(labelTag))
			{
				labelTag = string.Concat("?", baseAttributeData.FullAttribute, "?");
				font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Legendary)));
			}

			string label = Database.DB.GetFriendlyName(labelTag);
			if (string.IsNullOrEmpty(label))
			{
				label = string.Concat("?", labelTag, "?");
				font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Legendary)));
			}

			if (TQDebug.ItemDebugLevel > 2)
			{
				Log.Debug("Item.label (scroll) = " + label);
			}

			label = ItemAttributeProvider.ConvertFormat(label);

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
					font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Legendary)));
				}
				else
				{
					if (TQDebug.ItemDebugLevel > 2)
					{
						Log.Debug("Item.formatspec (2 parameter) = " + formatSpec);
					}

					formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
					font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Epic)));
				}
			}

			if (string.IsNullOrEmpty(formatSpecTag))
			{
				font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Epic)));
				line = ItemProvider.Format(label, variable[Math.Min(variable.NumberOfValues - 1, variableNumber)]);
			}
			else
			{
				line = ItemProvider.Format(formatSpec, variable[Math.Min(variable.NumberOfValues - 1, variableNumber)], label);
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
			string labelTag = ItemAttributeProvider.GetAttributeTextTag(attributeData);
			if (labelTag == null)
			{
				labelTag = string.Concat("?", attributeData.FullAttribute, "?");
				font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Legendary)));
			}

			string label = Database.DB.GetFriendlyName(labelTag);
			if (label == null)
			{
				label = string.Concat("?", labelTag, "?");
				font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Legendary)));
			}

			label = ItemAttributeProvider.ConvertFormat(label);
			if (label.IndexOf('{') >= 0)
			{
				// we have a format string.  try using it.
				line = ItemProvider.Format(label, variable[Math.Min(variable.NumberOfValues - 1, variableNumber)]);
				if (font == null)
				{
					font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Epic)));
				}
			}
			else
			{
				// no format string.
				line = Database.DB.VariableToStringNice(variable);
			}

			if (font == null)
			{
				font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Legendary))); // make these unknowns stand out
			}

			return line;
		}

		/// <summary>
		/// Converts the item's offensice attributes to a string
		/// </summary>
		/// <param name="record">DBRecord of the database record</param>
		/// <param name="attributeList">ArrayList containing the attribute list</param>
		/// <param name="data">ItemAttributesData for the item</param>
		/// <param name="recordId">string containing the record id</param>
		/// <param name="results">List containing the results</param>
		private static void ConvertOffenseAttributesToString(Item itm, DBRecordCollection record, List<Variable> attributeList, ItemAttributesData data, string recordId, List<string> results)
		{
			if (TQDebug.ItemDebugLevel > 0)
			{
				Log.DebugFormat(
					CultureInfo.InvariantCulture,
					"Item.ConvertOffenseAttrToString({0}, {1}, {2}, {3}, {4})",
					record,
					attributeList,
					data,
					recordId,
					results);
			}

			// If we are a relic, then sometimes there are multiple values per variable depending on how many pieces we have.
			// Let's determine which variable we want in these cases.
			int variableNumber = 0;
			if (itm.IsRelic && recordId == itm.BaseItemId)
			{
				variableNumber = itm.Number - 1;
			}
			else if (itm.HasRelic && recordId == itm.relicID)
			{
				variableNumber = Math.Max(itm.Var1, 1) - 1;
			}
			else if (itm.HasSecondRelic && recordId == itm.relic2ID)
			{
				variableNumber = Math.Max(itm.Var2, 1) - 1;
			}

			// Pet skills can also have multiple values so we attempt to decode it here
			if (itm.IsScroll || itm.IsRelic)
			{
				variableNumber = ItemProvider.GetPetSkillLevel(itm, record, recordId, variableNumber);
			}

			// Triggered skills can have also multiple values so we need to decode it here
			if (record.GetString("Class", 0).ToUpperInvariant().StartsWith("SKILL", StringComparison.OrdinalIgnoreCase))
			{
				variableNumber = ItemProvider.GetTriggeredSkillLevel(itm, record, recordId, variableNumber);
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

			bool isGlobal = ItemAttributeProvider.AttributeGroupHas(new Collection<Variable>(attributeList), "Global");
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

				ItemAttributesData attributeData = ItemAttributeProvider.GetAttributeData(variable.Name);
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
			string label = GetLabelAndColorFromTag(itm, data, recordId, ref labelTag, ref labelColor);

			if (TQDebug.ItemDebugLevel > 1)
			{
				Log.Debug("Full attribute = " + data.FullAttribute);
				Log.Debug("Item.label = " + label);
			}

			label = ItemAttributeProvider.ConvertFormat(label);

			// Figure out the Amount string
			string amount = null;
			if (minData != null && maxData != null &&
				minVar.GetSingle(Math.Min(minVar.NumberOfValues - 1, variableNumber)) != maxVar.GetSingle(Math.Min(maxVar.NumberOfValues - 1, variableNumber)))
			{
				if (minDurVar != null)
				{
					amount = GetAmountRange(itm, data, variableNumber, minVar, maxVar, ref label, labelColor, minDurVar);
				}
				else
				{
					amount = GetAmountRange(itm, data, variableNumber, minVar, maxVar, ref label, labelColor);
				}
			}
			else
			{
				if (minDurVar != null)
				{
					amount = GetAmountSingle(itm, data, variableNumber, minVar, maxVar, ref label, labelColor, minDurVar);
				}
				else
				{
					amount = GetAmountSingle(itm, data, variableNumber, minVar, maxVar, ref label, labelColor);
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
				label = HtmlHelper.MakeSafeForHtml(label);
				if (!string.IsNullOrEmpty(labelColor))
				{
					label = ItemProvider.Format("<font color={0}>{1}</font>", labelColor, label);
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
					&& recordId == itm.BaseItemId && string.IsNullOrEmpty(duration) && !string.IsNullOrEmpty(amount))
				{
					if (itm.IsWeapon)
					{
						if (data.Effect.Equals("offensivePierceRatio") ||
							data.Effect.Equals("offensivePhysical") ||
							data.Effect.Equals("offensiveBaseFire") ||
							data.Effect.Equals("offensiveBaseCold") ||
							data.Effect.Equals("offensiveBaseLightning") ||
							data.Effect.Equals("offensiveBaseLife"))
						{
							// mundane effect
							fontColor = HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Mundane));
						}
					}

					if (itm.IsShield)
					{
						if (data.Effect.Equals("defensiveBlock") ||
							data.Effect.Equals("blockRecoveryTime") ||
							data.Effect.Equals("offensivePhysical"))
						{
							fontColor = HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Mundane));
						}
					}
				}

				if (string.IsNullOrEmpty(fontColor))
				{
					// magical effect
					fontColor = HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Epic));
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
				modifierText = ItemProvider.Format("<font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Epic)), modifierText);
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

				ItemAttributesData attributeData = ItemAttributeProvider.GetAttributeData(variable.Name);
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
						// only display itm tag if we are a basic weapon
						if (itm.IsWeapon && recordId == itm.BaseItemId)
						{
							font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Mundane)));
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
					else if (itm.IsFormulae && recordId == itm.BaseItemId)
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
					if (recordId == itm.BaseItemId && normalizedFullAttribute == "ATTRIBUTESCALEPERCENT" && itm.itemScalePercent == 1.00)
					{
						itm.itemScalePercent += variable.GetSingle(0) / 100;

						// Set line to nothing so we do not see the tag text.
						line = string.Empty;
					}
					else if (normalizedFullAttribute == "SKILLNAME")
					{
						// Added by VillageIdiot
						// itm is for Scroll effects which get decoded in the skill code below
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
								Log.Debug("Item.formatspec (Damage type) = " + formatSpec);
							}

							formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
						}

						font = string.Format(CultureInfo.CurrentCulture, "<font color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Mundane)));
						line = ItemProvider.Format(formatSpec, damageType);
					}

					// We have no line so just show the raw attribute
					if (line == null)
					{
						line = GetRawAttribute(data, variableNumber, variable, ref font);
					}

					// Start finalizing the line of text
					if (line.Length > 0)
					{
						line = HtmlHelper.MakeSafeForHtml(line);
						if (attributeData.Variable.Length > 0)
						{
							string s = HtmlHelper.MakeSafeForHtml(attributeData.Variable);
							s = string.Format(CultureInfo.CurrentCulture, " <font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Legendary)), s);
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
						if (itm.IsFormulae && normalizedFullAttribute.StartsWith("REAGENT", StringComparison.OrdinalIgnoreCase))
						{
							line = string.Concat("&nbsp;&nbsp;&nbsp;&nbsp;", line);
						}

						results.Add(line);
					}

					// Added by VillageIdiot
					// itm a special case for pet bonuses
					if (normalizedFullAttribute == "PETBONUSNAME")
					{
						string petBonusID = record.GetString("petBonusName", 0);
						DBRecordCollection petBonusRecord = Database.DB.GetRecordFromFile(petBonusID);
						if (petBonusRecord != null)
						{
							GetAttributesFromRecord(itm, petBonusRecord, true, petBonusID, results);
							results.Add(string.Empty);
						}
					}

					// Added by VillageIdiot
					// Another special case for skill description and effects of activated skills
					if (normalizedFullAttribute == "ITEMSKILLNAME" || (itm.IsScroll && normalizedFullAttribute == "SKILLNAME"))
					{
						GetSkillDescriptionAndEffects(itm, record, results, variable, line);
					}
				}
			}

			if (TQDebug.ItemDebugLevel > 0)
			{
				Log.Debug("Exiting Item.ConvertOffenseAttrToString()");
			}
		}

		/// <summary>
		/// Adds the formatted skill description and effects for granted skills to the results list
		/// </summary>
		/// <param name="record">DBRecord databse record</param>
		/// <param name="results">results list</param>
		/// <param name="variable">variable structure</param>
		/// <param name="line">line of text</param>
		private static void GetSkillDescriptionAndEffects(Item itm, DBRecordCollection record, List<string> results, Variable variable, string line)
		{
			string autoController = record.GetString("itemSkillAutoController", 0);
			if (!string.IsNullOrEmpty(autoController) || itm.IsScroll)
			{
				DBRecordCollection skillRecord = Database.DB.GetRecordFromFile(variable.GetString(0));

				// Changed by VillageIdiot
				// Get title from the last line
				// Remove the HTML formatting and use for word wrapping
				string lastline = string.Empty;
				if (!itm.IsScroll)
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

					if (!itm.IsScroll)
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
									skillDescription = HtmlHelper.MakeSafeForHtml(skillDescription);
									skillDescriptionList = Database.WrapWords(skillDescription, lineLength);

									foreach (string skillDescriptionFromList in skillDescriptionList)
									{
										results.Add(string.Format(
											CultureInfo.CurrentCulture,
											"<font color={0}>&nbsp;&nbsp;&nbsp;&nbsp;{1}</font>",
											HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Mundane)),
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
											formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
										}

										int skillLevel = record.GetInt32("itemSkillLevel", 0);
										if (skillLevel > 0)
										{
											line = ItemProvider.Format(formatSpec, skillLevel);
											line = HtmlHelper.MakeSafeForHtml(line);
											results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>&nbsp;&nbsp;&nbsp;&nbsp;{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Mundane)), line));
										}
									}
								}
							}
						}
						else
						{
							// itm skill is a buff
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
											HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Mundane)),
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
											formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
										}

										int skillLevel = record.GetInt32("itemSkillLevel", 0);
										if (skillLevel > 0)
										{
											line = ItemProvider.Format(formatSpec, skillLevel);
											line = HtmlHelper.MakeSafeForHtml(line);
											results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>&nbsp;&nbsp;&nbsp;&nbsp;{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Mundane)), line));
										}
									}
								}
							}
						}
					}

					// Clear out effects for unnamed skills, unless it's a buff or a scroll.
					if (skillRecord.GetString("skillDisplayName", 0).Length == 0 && string.IsNullOrEmpty(buffSkillName) && !itm.IsScroll)
					{
						skillRecord = null;
					}

					// Added by VillageIdiot
					// Adjust for the flavor text of scrolls
					if (skillRecord != null && !itm.IsScroll)
					{
						results.Add(string.Empty);
					}

					// Added by VillageIdiot
					// Add the skill effects
					if (skillRecord != null)
					{
						if (skillRecord.GetString("Class", 0).ToUpperInvariant().Equals("SKILL_SPAWNPET"))
						{
							// itm is a summon
							ConvertPetStats(itm, skillRecord, results);
						}
						else
						{
							// Skill Effects
							if (!string.IsNullOrEmpty(buffSkillName))
							{
								GetAttributesFromRecord(itm, Database.DB.GetRecordFromFile(buffSkillName), true, buffSkillName, results);
							}
							else
							{
								GetAttributesFromRecord(itm, skillRecord, true, variable.GetString(0), results);
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
		private static string GetAmountSingle(Item itm, ItemAttributesData data, int varNum, Variable minVar, Variable maxVar, ref string label, string labelColor, Variable minDurVar = null)
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
				color = HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Legendary));
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 2)
				{
					Log.Debug("Item.formatspec (single) = " + formatSpec);
				}

				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
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
						currentVariable[Math.Min(currentVariable.NumberOfValues - 1, varNum)] = (float)currentVariable[Math.Min(currentVariable.NumberOfValues - 1, varNum)] * (float)minDurVar[minDurVar.NumberOfValues - 1] * itm.itemScalePercent;
					}
					else
					{
						currentVariable[Math.Min(currentVariable.NumberOfValues - 1, varNum)] = (float)currentVariable[Math.Min(currentVariable.NumberOfValues - 1, varNum)] * itm.itemScalePercent;
					}
				}

				amount = ItemProvider.Format(formatSpec, currentVariable[Math.Min(currentVariable.NumberOfValues - 1, varNum)]);
				amount = HtmlHelper.MakeSafeForHtml(amount);
				if (!string.IsNullOrEmpty(color))
				{
					amount = ItemProvider.Format("<font color={0}>{1}</font>", color, amount);
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
		private static string GetAmountRange(Item itm, ItemAttributesData data, int varNum, Variable minVar, Variable maxVar, ref string label, string labelColor, Variable minDurVar = null)
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
				color = HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Legendary));
			}
			else
			{
				if (TQDebug.ItemDebugLevel > 2)
				{
					Log.Debug("Item.formatspec (range) = " + formatSpec);
				}

				formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
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
				min[Math.Min(min.NumberOfValues - 1, varNum)] = (float)min[Math.Min(min.NumberOfValues - 1, varNum)] * (float)minDurVar[minDurVar.NumberOfValues - 1] * itm.itemScalePercent;
				max[Math.Min(max.NumberOfValues - 1, varNum)] = (float)max[Math.Min(max.NumberOfValues - 1, varNum)] * (float)minDurVar[minDurVar.NumberOfValues - 1] * itm.itemScalePercent;
			}
			else
			{
				min[Math.Min(min.NumberOfValues - 1, varNum)] = (float)min[Math.Min(min.NumberOfValues - 1, varNum)] * itm.itemScalePercent;
				max[Math.Min(max.NumberOfValues - 1, varNum)] = (float)max[Math.Min(max.NumberOfValues - 1, varNum)] * itm.itemScalePercent;
			}

			amount = ItemProvider.Format(formatSpec, min[Math.Min(min.NumberOfValues - 1, varNum)], max[Math.Min(max.NumberOfValues - 1, varNum)]);
			amount = HtmlHelper.MakeSafeForHtml(amount);
			if (!string.IsNullOrEmpty(color))
			{
				amount = ItemProvider.Format("<font color={0}>{1}</font>", color, amount);
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
		private static string GetLabelAndColorFromTag(Item itm, ItemAttributesData data, string recordId, ref string labelTag, ref string labelColor)
		{
			labelTag = ItemAttributeProvider.GetAttributeTextTag(data);
			string label;

			if (string.IsNullOrEmpty(labelTag))
			{
				labelTag = string.Concat("?", data.FullAttribute, "?");
				labelColor = HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Legendary));
			}

			// if itm is an Armor effect and we are not armor, then change it to bonus
			if (labelTag.ToUpperInvariant().Equals("DEFENSEABSORPTIONPROTECTION"))
			{
				if (!itm.IsArmor || recordId != itm.BaseItemId)
				{
					labelTag = "DefenseAbsorptionProtectionBonus";
					labelColor = HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Epic));
				}
				else
				{
					// regular armor attribute is not magical
					labelColor = HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Mundane));
				}
			}

			label = Database.DB.GetFriendlyName(labelTag);
			if (string.IsNullOrEmpty(label))
			{
				label = string.Concat("?", labelTag, "?");
				labelColor = HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Legendary));
			}

			return label;
		}

		/// <summary>
		/// Used for showing the pet statistics
		/// </summary>
		/// <param name="skillRecord">DBRecord of the skill</param>
		/// <param name="results">List containing the results</param>
		private static void ConvertPetStats(Item itm, DBRecordCollection skillRecord, List<string> results)
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
					formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
				}

				petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, summonLimit.ToString(CultureInfo.CurrentCulture));
				results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Mundane)), petLine));
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
					formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
				}

				string petNameTag = petRecord.GetString("description", 0);
				string petName = Database.DB.GetFriendlyName(petNameTag);
				float value = 0.0F;
				petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, petName);
				results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Mundane)), petLine));

				// Time to live
				formatSpec = Database.DB.GetFriendlyName("tagSkillPetTimeToLive");
				if (string.IsNullOrEmpty(formatSpec))
				{
					formatSpec = "Life Time {0} Seconds";
				}
				else
				{
					formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
				}

				petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, skillRecord.GetSingle("spawnObjectsTimeToLive", 0));
				results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Mundane)), petLine));

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
						formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
					}

					petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, value);
					results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Mundane)), petLine));
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
						formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
					}

					petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, value);
					results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Mundane)), petLine));
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
					formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
				}

				petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, petName);
				results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Mundane)), petLine));

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
							formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
						}

						petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, value);
						results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Mundane)), petLine));
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
							formatSpec = ItemAttributeProvider.ConvertFormat(formatSpec);
						}

						petLine = string.Format(CultureInfo.CurrentCulture, formatSpec, value, value2);
						results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Mundane)), petLine));
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
						results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Legendary)), skillNameTag));
					}
					else
					{
						results.Add(string.Format(CultureInfo.CurrentCulture, "<font color={0}>{1}</font>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Mundane)), skillName));
					}

					GetAttributesFromRecord(itm, record, true, recordID, results);
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
		private static void ConvertAttributeListToString(Item itm, DBRecordCollection record, List<Variable> attributeList, string recordId, List<string> results)
		{
			if (TQDebug.ItemDebugLevel > 0)
			{
				Log.DebugFormat(
					CultureInfo.InvariantCulture,
					"Item.ConvertAttrListToString ({0}, {1}, {2}, {3})",
					record,
					attributeList,
					recordId,
					results);
			}

			// see what kind of effects are in this list
			Variable variable = (Variable)attributeList[0];
			ItemAttributesData data = ItemAttributeProvider.GetAttributeData(variable.Name);
			if (data == null)
			{
				// unknown attribute
				if (TQDebug.ItemDebugLevel > 0)
				{
					Log.Debug("Error - Unknown Attribute.");
				}

				data = new ItemAttributesData(ItemAttributesEffectType.Other, variable.Name, variable.Name, string.Empty, 0);
			}

			if (TQDebug.ItemDebugLevel > 0)
			{
				Log.Debug("Exiting Item.ConvertAttrListToString ()");
			}

			ConvertOffenseAttributesToString(itm, record, attributeList, data, recordId, results);
			return;
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
		private static void GetAttributesFromRecord(Item itm, DBRecordCollection record, bool filtering, string recordId, List<string> results, bool convertStrings = true)
		{
			if (TQDebug.ItemDebugLevel > 0)
			{
				Log.DebugFormat(
					CultureInfo.InvariantCulture,
					"Item.GetAttributesFromRecord({0}, {1}, {2}, {3}, {4})",
					record,
					filtering,
					recordId,
					results,
					convertStrings);
			}

			// First get a list of attributes, grouped by effect.
			Dictionary<string, List<Variable>> attrByEffect = new Dictionary<string, List<Variable>>();
			if (record == null)
			{
				if (TQDebug.ItemDebugLevel > 0)
				{
					Log.Debug("Error - record was null.");
				}

				results.Add("<unknown>");
				return;
			}

			if (TQDebug.ItemDebugLevel > 1)
			{
				Log.Debug(record.Id);
			}

			// Added by Village Idiot
			// To keep track of groups so they are not counted twice
			List<string> countedGroups = new List<string>();
			countedGroups.Clear();

			foreach (Variable variable in record)
			{
				if (TQDebug.ItemDebugLevel > 2)
				{
					Log.Debug(variable.Name);
				}

				if (ItemProvider.FilterValue(variable, !filtering))
				{
					continue;
				}

				if (filtering && ItemProvider.FilterKey(variable.Name))
				{
					continue;
				}

				if (filtering && ItemProvider.FilterRequirements(variable.Name))
				{
					continue;
				}

				ItemAttributesData data = ItemAttributeProvider.GetAttributeData(variable.Name);
				if (data == null)
				{
					// unknown attribute
					if (TQDebug.ItemDebugLevel > 2)
					{
						Log.Debug("Unknown Attribute");
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

				// Find or create the attrList for itm effect
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

				// Add itm guy to the attrList
				attrList.Add(variable);

				// Added by VillageIdiot
				// Set number of attributes parameter for level calculation
				// Filter relics and relic bonuses
				if (recordId != itm.relic2ID && recordId != itm.RelicBonus2Id && recordId != itm.relicID && recordId != itm.RelicBonusId && !itm.isAttributeCounted)
				{
					// Added test to see if itm has already been done
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
									itm.attributeCount += variable.GetInt32(0);
									countedGroups.Add(effectGroup);
								}
								else
								{
									++itm.attributeCount;
									countedGroups.Add(effectGroup);
								}
							}
						}
					}
				}
			}

			// Added by VillageIdiot
			// Some attributes have been counted so set the flag so we do not count them again
			if (itm.attributeCount != 0)
			{
				itm.isAttributeCounted = true;
			}

			// Now we have all our attributes grouped by effect.  Now lets sort them
			List<Variable>[] attrArray = new List<Variable>[attrByEffect.Count];
			attrByEffect.Values.CopyTo(attrArray, 0);
			Array.Sort(attrArray, new ItemAttributeListCompare(itm.IsArmor || itm.IsShield));

			// Now for the global params, we need to check to see if they are XOR or all.
			// We do itm by checking the effect just after the global param.
			for (int i = 0; i < attrArray.Length; ++i)
			{
				List<Variable> attrList = attrArray[i];

				if (ItemAttributeProvider.AttributeGroupIs(new Collection<Variable>(attrList), "offensiveGlobalChance") ||
					ItemAttributeProvider.AttributeGroupIs(new Collection<Variable>(attrList), "retaliationGlobalChance"))
				{
					// check the next effect group
					int j = i + 1;
					if (j < attrArray.Length)
					{
						List<Variable> next = attrArray[j];
						if (!ItemAttributeProvider.AttributeGroupHas(new Collection<Variable>(next), "Global"))
						{
							// itm is a spurious globalChance entry.  Let's add 2 null entries to signal it should be ignored
							attrList.Add(null);
							attrList.Add(null);
						}
						else if (ItemAttributeProvider.AttributeGroupHas(new Collection<Variable>(next), "XOR"))
						{
							// Yes it is global and is also XOR
							// flag our current attribute as XOR
							// We do itm by adding a second NULL entry to the list.  Its a hack but it works
							attrList.Add(null);
						}
					}
					else
					{
						// itm is a spurious globalChance entry.  Let's add 2 null entries to signal it should be ignored
						attrList.Add(null);
						attrList.Add(null);
					}
				}
			}

			foreach (List<Variable> attrList in attrArray)
			{
				// Used to sort out the Damage Qualifier effects.
				if (ItemAttributeProvider.AttributeGroupIs(new Collection<Variable>(attrList), ItemAttributesEffectType.DamageQualifierEffect))
				{
					attrList.Sort(new ItemAttributeSubListCompare());
				}

				if (!convertStrings)
				{
					ConvertBareAttributeListToString(attrList, results);
				}
				else
				{
					ConvertAttributeListToString(itm, record, attrList, recordId, results);
				}
			}

			if (TQDebug.ItemDebugLevel > 0)
			{
				Log.Debug("Exiting Item.GetAttributesFromRecord()");
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

		public class NewItemHighlightedTooltipResult
		{
			public string FriendlyName;
			public Color ForeColor;
			public string Attributes;
			public string ItemSet;
			public string Requirements;
			public string TooltipText;
		}

		/// <summary>
		/// Produce Html content of Item Tooltip
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static NewItemHighlightedTooltipResult NewItemHighlightedTooltip(Item item)
		{
			var result = new NewItemHighlightedTooltipResult();
			result.FriendlyName = ItemProvider.ToFriendlyName(item);
			result.ForeColor = ItemGfxHelper.GetColorTag(item, result.FriendlyName);
			result.FriendlyName = Item.ClipColorTag(result.FriendlyName);
			result.Attributes = GetAttributes(item, true); // true means hide uninteresting attributes
			result.ItemSet = GetItemSetString(item);
			result.Requirements = GetRequirements(item);

			// combine the 2
			if (result.Requirements.Length < 1)
			{
				result.TooltipText = result.Attributes;
			}
			else if (result.ItemSet.Length < 1)
			{
				string reqTitle = HtmlHelper.MakeSafeForHtml("?Requirements?");
				reqTitle = string.Format(CultureInfo.InvariantCulture, "<font size=+2 color={0}>{1}</font><br>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Potion)), reqTitle);
				string separator = string.Format(CultureInfo.InvariantCulture, "<hr color={0}><br>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Broken)));
				result.TooltipText = string.Concat(result.Attributes, separator, result.Requirements);
			}
			else
			{
				string reqTitle = HtmlHelper.MakeSafeForHtml("?Requirements?");
				reqTitle = string.Format(CultureInfo.InvariantCulture, "<font size=+2 color={0}>{1}</font><br>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Potion)), reqTitle);
				string separator1 = string.Format(CultureInfo.InvariantCulture, "<hr color={0}>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Broken)));
				string separator2 = string.Format(CultureInfo.InvariantCulture, "<hr color={0}><br>", HtmlHelper.HtmlColor(ItemGfxHelper.GetColor(ItemStyle.Broken)));
				result.TooltipText = string.Concat(result.Attributes, separator1, result.ItemSet, separator2, result.Requirements);
			}

			result.TooltipText = string.Concat(HtmlHelper.TooltipBodyTag(UIService.UI.Scale), result.TooltipText);

			return result;
		}


		/// <summary>
		/// Gets the tooltip for a sack.  Summarizes the items within the sack
		/// </summary>
		/// <param name="button">button the mouse is over.  Corresponds to a sack</param>
		/// <returns>string to be displayed in the tooltip</returns>
		public static string GetSackToolTip(SackCollection sack, IEnumerable<Item> excluded = null)
		{
			if (sack.IsEmpty)
			{
				return string.Format(CultureInfo.CurrentCulture, "{0}<b>{1}</b>", HtmlHelper.TooltipTitleTag(UIService.UI.Scale), HtmlHelper.MakeSafeForHtml(Resources.VaultGroupBoxEmpty));
			}

			excluded = excluded ?? new Item[] { };
			StringBuilder answer = new StringBuilder();
			answer.Append(HtmlHelper.TooltipTitleTag(UIService.UI.Scale));
			bool first = true;
			foreach (Item item in sack)
			{
				if (excluded.Contains(item))
					// skip the item being dragged
					continue;

				if (item.BaseItemId.Length == 0)
					// skip empty items
					continue;

				if (!first)
					answer.Append("<br>");

				first = false;
				string text = HtmlHelper.MakeSafeForHtml(ItemProvider.ToFriendlyName(item));
				Color color = ItemGfxHelper.GetColorTag(item, text);
				text = Item.ClipColorTag(text);
				string htmlcolor = HtmlHelper.HtmlColor(color);
				string htmlLine = string.Format(CultureInfo.CurrentCulture, "<font color={0}><b>{1}</b></font>", htmlcolor, text);
				answer.Append(htmlLine);
			}

			return answer.ToString();
		}


		/// <summary>
		/// Gets the name of the item
		/// </summary>
		/// <param name="item">item being displayed</param>
		/// <returns>string with the item name</returns>
		public static string GetName(Item item)
		{
			string itemName = HtmlHelper.MakeSafeForHtml(ItemProvider.ToFriendlyName(item, true, false));
			string bgcolor = "#2e1f15";

			Color color = ItemGfxHelper.GetColorTag(item, itemName);
			itemName = Item.ClipColorTag(itemName);
			itemName = string.Format(CultureInfo.InvariantCulture, "<font size=+1 color={0}><b>{1}</b></font>", HtmlHelper.HtmlColor(color), itemName);
			return string.Format(CultureInfo.InvariantCulture, "<body bgcolor={0} text=white><font face=\"Albertus MT\" size=2>{1}", bgcolor, itemName);
		}

		public class LoadPropertiesResult
		{
			public string BaseItemAttributes = string.Empty;
			public string PrefixAttributes = string.Empty;
			public string SuffixAttributes = string.Empty;
			public string BgColor = "#2e1f15";
		}

		/// <summary>
		/// Loads the item properties
		/// </summary>
		public static LoadPropertiesResult LoadProperties(Item item, bool filterExtra)
		{
			var result = new LoadPropertiesResult();

			string[] bareAttr = GetBareAttributes(item, filterExtra);

			var pattern = "<body bgcolor={0} text=white><font face=\"Albertus MT\" size=1>{1}";

			// Base Item Attributes
			if (bareAttr[0].Any()) result.BaseItemAttributes = string.Format(CultureInfo.InvariantCulture, pattern, result.BgColor, bareAttr[0]);

			// Prefix Attributes
			if (bareAttr[2].Any()) result.PrefixAttributes = string.Format(CultureInfo.InvariantCulture, pattern, result.BgColor, bareAttr[2]);

			// Suffix Attributes
			if (bareAttr[3].Any()) result.SuffixAttributes = string.Format(CultureInfo.InvariantCulture, pattern, result.BgColor, bareAttr[3]);

			return result;
		}

	}
}
