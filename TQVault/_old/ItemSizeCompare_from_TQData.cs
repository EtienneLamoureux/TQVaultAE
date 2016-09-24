//-----------------------------------------------------------------------
// <copyright file="ItemSizeCompare.cs" company="bman654">
//     Copyright (c) Brandon Wallace. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultData
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    
    /// <summary>
    /// Used to compare item sizes for autosorting
    /// </summary>
    public class ItemSizeCompare : IComparer<Item>
    {
        /// <summary>
        /// Initializes a new instance of the ItemSizeCompare class.
        /// </summary>
        public ItemSizeCompare()
        {
        }

        /// <summary>
        /// Compares 2 Items
        /// </summary>
        /// <param name="value1">First object to compare</param>
        /// <param name="value2">Second object to compare</param>
        /// <returns>-1 0 1 depending on comparison</returns>
        int IComparer<Item>.Compare(Item value1, Item value2)
        {
            return Compare(value1, value2);
        }

        /// <summary>
        /// Compares 2 Items
        /// </summary>
        /// <param name="value1">First object to compare</param>
        /// <param name="value2">Second object to compare</param>
        /// <returns>-1 0 1 depending on comparison</returns>
        protected static int Compare(Item value1, Item value2)
        {
            return DoCompare(value1, value2);
        }

        /// <summary>
        /// Compares the sizes of 2 Items
        /// Calculates the order with the largest items first.
        /// The height is weighted higher because it's more difficult to place long items.
        /// </summary>
        /// <param name="item1">First item to be compared</param>
        /// <param name="item2">Second item to be compared</param>
        /// <returns>-1 if item2 is larger, 1 if item1 is larger and 0 if equal</returns>
        private static int DoCompare(Item item1, Item item2)
        {
            int ordera = (((item1.Height * 3) + item1.Width) * 100) + item1.ItemGroup;
            int orderb = (((item2.Height * 3) + item2.Width) * 100) + item2.ItemGroup;

            return (ordera > orderb) ? -1 : (ordera < orderb) ? 1 : 0;
        }
    }
}
