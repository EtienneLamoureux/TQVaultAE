//-----------------------------------------------------------------------
// <copyright file="SackPanelEventArgs.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Models
{
	using System;
	using TQVaultData;

	/// <summary>
	/// Sack panel event data
	/// </summary>
	public class SackPanelEventArgs : EventArgs
	{
		/// <summary>
		/// Sack which we are working with
		/// </summary>
		private SackCollection sack;

		/// <summary>
		/// Item that we are working with.
		/// </summary>
		private Item item;

		/// <summary>
		/// Initializes a new instance of the SackPanelEventArgs class.
		/// </summary>
		/// <param name="sack">sack that we are working with</param>
		/// <param name="item">item that we are working with</param>
		public SackPanelEventArgs(SackCollection sack, Item item)
		{
			this.sack = sack;
			this.item = item;
		}

		/// <summary>
		/// Gets the sack.
		/// </summary>
		public SackCollection Sack
		{
			get
			{
				return this.sack;
			}
		}

		/// <summary>
		/// Gets the item.
		/// </summary>
		public Item Item
		{
			get
			{
				return this.item;
			}
		}
	}
}