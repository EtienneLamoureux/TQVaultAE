//-----------------------------------------------------------------------
// <copyright file="Result.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Models
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using TQVaultData;

	/// <summary>
	/// Class for an individual result in the results list.
	/// </summary>
	public class Result
	{
		private readonly string container;
		private readonly string containerName;
		private readonly int sackNumber;
		private readonly SackType sackType;
		private readonly Item item;

		private readonly string itemName;
		private readonly string itemStyle;
		private readonly Color color;

		private readonly int requiredLevel;

		public Result(string container, string containerName, int sackNumber, SackType sackType, Item item)
		{
			this.container = container ?? throw new ArgumentNullException(nameof(container));
			this.containerName = containerName ?? throw new ArgumentNullException(nameof(containerName));
			this.sackNumber = sackNumber;
			this.sackType = sackType;
			this.item = item ?? throw new ArgumentNullException(nameof(item));

			this.itemName = Item.ClipColorTag(item.ToString());

			ItemStyle computedItemStyle = item.ItemStyle;
			this.itemStyle = MainForm.GetItemStyleString(computedItemStyle);
			this.color = Item.GetColor(computedItemStyle);

			var requirementVariables = item.GetRequirementVariables().Values;
			this.requiredLevel = GetRequirement(requirementVariables, "levelRequirement");
		}

		private int GetRequirement(IList<Variable> variables, string key)
		{
			return variables
				.Where(v => string.Equals(v.Name, key, StringComparison.InvariantCultureIgnoreCase) && v.DataType == VariableDataType.Integer && v.NumberOfValues > 0)
				.Select(v => v.GetInt32(0))
				.DefaultIfEmpty(0)
				.Max();
		}

		public string Container => container;

		public string ContainerName => containerName;

		public SackType SackType => sackType;

		public int SackNumber => sackNumber;

		public Item Item => item;

		public string ItemStyle => itemStyle;

		public Color Color => color;

		public string ItemName => itemName;

		public int RequiredLevel => requiredLevel;
	}
}