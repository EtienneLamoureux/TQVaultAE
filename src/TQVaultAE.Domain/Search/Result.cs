//-----------------------------------------------------------------------
// <copyright file="Result.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.Domain.Search
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using TQVaultAE.Domain.Entities;
	using TQVaultAE.Domain.Helpers;
	using TQVaultAE.Domain.Results;

	/// <summary>
	/// Class for an individual result in the results list.
	/// </summary>
	public class Result
	{
		public readonly string Container;
		public readonly string ContainerName;
		public readonly int SackNumber;
		public readonly SackType SackType;
		private readonly Lazy<ToFriendlyNameResult> FriendlyNamesLazyLoader;
		public ToFriendlyNameResult FriendlyNames { get; private set; }
		public string ItemName { get; private set; }
		public ItemStyle ItemStyle { get; private set; }
		public TQColor TQColor { get; private set; }
		public int RequiredLevel { get; private set; }

		public string IdString
			=> string.Join("|", new[] {
					Container
					, ContainerName
					, SackNumber.ToString()
					, this.SackType.ToString()
					, this.FriendlyNames.FullNameBagTooltip
				});


		/// <summary>
		/// Advanced Ctrs
		/// </summary>
		/// <param name="container"></param>
		/// <param name="containerName"></param>
		/// <param name="sackNumber"></param>
		/// <param name="sackType"></param>
		/// <param name="fnames"></param>
		public Result(string container, string containerName, int sackNumber, SackType sackType, Lazy<ToFriendlyNameResult> fnames)
		{
			this.Container = container ?? throw new ArgumentNullException(nameof(container));
			this.ContainerName = containerName ?? throw new ArgumentNullException(nameof(containerName));
			this.SackNumber = sackNumber;
			this.SackType = sackType;
			this.FriendlyNamesLazyLoader = fnames ?? throw new ArgumentNullException(nameof(fnames));
		}

		public void LazyLoad()
		{
			this.FriendlyNames = this.FriendlyNamesLazyLoader.Value;
			this.ItemName = this.FriendlyNames.FullNameClean;
			this.ItemStyle = this.FriendlyNames.Item.ItemStyle;
			this.TQColor = this.FriendlyNames.Item.ItemStyle.TQColor();
			this.RequiredLevel = GetRequirement(this.FriendlyNames.RequirementVariables.Values, "levelRequirement");
		}

		/// <summary>
		/// Old Ctrs
		/// </summary>
		/// <param name="container"></param>
		/// <param name="containerName"></param>
		/// <param name="sackNumber"></param>
		/// <param name="sackType"></param>
		/// <param name="fnames"></param>
		public Result(string container, string containerName, int sackNumber, SackType sackType, ToFriendlyNameResult fnames)
		{
			this.Container = container ?? throw new ArgumentNullException(nameof(container));
			this.ContainerName = containerName ?? throw new ArgumentNullException(nameof(containerName));
			this.SackNumber = sackNumber;
			this.SackType = sackType;
			this.FriendlyNames = fnames ?? throw new ArgumentNullException(nameof(fnames));
			this.ItemName = fnames.FullNameClean;
			this.ItemStyle = fnames.Item.ItemStyle;
			this.TQColor = fnames.Item.ItemStyle.TQColor();
			this.RequiredLevel = GetRequirement(fnames.RequirementVariables.Values, "levelRequirement");
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