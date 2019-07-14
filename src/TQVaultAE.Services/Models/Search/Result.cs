//-----------------------------------------------------------------------
// <copyright file="Result.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Services.Models.Search
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using TQVaultAE.Data;
	using TQVaultAE.Entities;
	using TQVaultAE.Presentation;

	/// <summary>
	/// Class for an individual result in the results list.
	/// </summary>
	public class Result
	{
		public readonly string Container;
		public readonly string ContainerName;
		public readonly int SackNumber;
		public readonly SackType SackType;
		public readonly Item Item;
		public readonly string ItemName;
		public readonly string ItemStyle;
		public readonly Color Color;
		public readonly int RequiredLevel;

		public Result(string container, string containerName, int sackNumber, SackType sackType, Item item)
		{
			this.Container = container ?? throw new ArgumentNullException(nameof(container));
			this.ContainerName = containerName ?? throw new ArgumentNullException(nameof(containerName));
			this.SackNumber = sackNumber;
			this.SackType = sackType;
			this.Item = item ?? throw new ArgumentNullException(nameof(item));
			this.ItemName = TQColorHelper.RemoveLeadingColorTag(ItemProvider.ToFriendlyName(item));
			ItemStyle computedItemStyle = item.ItemStyle;
			this.ItemStyle = ItemStyleHelper.Translate(computedItemStyle);
			this.Color = ItemGfxHelper.Color(computedItemStyle);
			var requirementVariables = ItemProvider.GetRequirementVariables(item).Values;
			this.RequiredLevel = GetRequirement(requirementVariables, "levelRequirement");
		}

		private int GetRequirement(IList<Variable> variables, string key)
		{
			return variables
				.Where(v => string.Equals(v.Name, key, StringComparison.InvariantCultureIgnoreCase) && v.DataType == VariableDataType.Integer && v.NumberOfValues > 0)
				.Select(v => v.GetInt32(0))
				.DefaultIfEmpty(0)
				.Max();
		}

	}
}