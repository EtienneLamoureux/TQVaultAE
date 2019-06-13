//-----------------------------------------------------------------------
// <copyright file="AutoMoveLocation.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Models
{
	/// <summary>
	/// Enumeration of the available panel automovelocations.
	/// </summary>
	public enum AutoMoveLocation
	{
		/// <summary>
		/// Value indicating that the location has not been set.
		/// </summary>
		NotSet = -1,

		/// <summary>
		/// Value indicating that there is no location.
		/// </summary>
		None = 0,

		/// <summary>
		/// Value indicating that the location is the Main Vault panel.
		/// </summary>
		Vault = 20,

		/// <summary>
		/// Value indicating that the location is the Player panel.
		/// </summary>
		Player = 21,

		/// <summary>
		/// Value indicating that the location is the Trash panel.
		/// </summary>
		Trash = 22,

		/// <summary>
		/// Value indicating that the location is the Secondary Vault panel.
		/// </summary>
		SecondaryVault = 23,

		/// <summary>
		/// Value indicating that the location is the Stash panel.
		/// </summary>
		Stash = 24
	}
}