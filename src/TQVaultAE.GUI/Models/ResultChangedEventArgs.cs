//-----------------------------------------------------------------------
// <copyright file="ResultChangedEventArgs.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace TQVaultAE.GUI.Models;

using System;
using TQVaultAE.Application.Results;

/// <summary>
/// Encapsulates the ResultsChanged event data
/// </summary>
public class ResultChangedEventArgs : EventArgs
{
	/// <summary>
	/// Initializes a new instance of the ResultChangedEventArgs class.
	/// </summary>
	/// <param name="result">Result data</param>
	public ResultChangedEventArgs(SearchResult result)
	{
		this.Result = result;
	}

	/// <summary>
	/// Gets the Result data.
	/// </summary>
	public SearchResult Result { get; private set; }
}