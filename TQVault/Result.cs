//-----------------------------------------------------------------------
// <copyright file="Result.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVault
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using TQVaultData;

    /// <summary>
    /// Class for an individual result in the results list.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Gets or sets the item string
        /// </summary>
        ////public string ItemName { get; set; }

        /// <summary>
        /// Gets or sets the container string
        /// </summary>
        public string Container { get; set; }

        /// <summary>
        /// Gets or sets the container name string
        /// </summary>
        public string ContainerName { get; set; }

        /// <summary>
        /// Gets or sets the sack number
        /// </summary>
        public int Sack { get; set; }

        /// <summary>
        /// Gets or sets the container type
        /// </summary>
        public SackType ContainerType { get; set; }

        /// <summary>
        /// Gets or sets the item location
        /// </summary>
        ////public Point Location { get; set; }

        /// <summary>
        /// Gets or sets the item style (quality)
        /// </summary>
        public string ItemStyle { get; set; }

        /// <summary>
        /// Gets or sets the Item instance for this result.
        /// </summary>
        public Item Item { get; set; }
    }
}
