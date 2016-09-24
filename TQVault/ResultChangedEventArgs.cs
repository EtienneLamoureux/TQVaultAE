//-----------------------------------------------------------------------
// <copyright file="ResultChangedEventArgs.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVault
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Encapsulates the ResultsChanged event data
    /// </summary>
    public class ResultChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the ResultChangedEventArgs class.
        /// </summary>
        /// <param name="result">Result data</param>
        public ResultChangedEventArgs(Result result)
        {
            this.Result = result;
        }

        /// <summary>
        /// Gets the Result data.
        /// </summary>
        public Result Result { get; private set; }
    }
}
