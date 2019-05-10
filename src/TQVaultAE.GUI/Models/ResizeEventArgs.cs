//-----------------------------------------------------------------------
// <copyright file="ResizeEventArgs.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Models
{
	using System;

	/// <summary>
	/// Resizing event data
	/// </summary>
	public class ResizeEventArgs : EventArgs
	{
		/// <summary>
		/// Holds the value for how much we are resizing
		/// </summary>
		private float resizeDelta;

		/// <summary>
		/// Initializes a new instance of the ResizeEventArgs class.
		/// </summary>
		/// <param name="resizeDelta">The delta for the resize operation</param>
		public ResizeEventArgs(float resizeDelta)
		{
			this.resizeDelta = resizeDelta;
		}

		/// <summary>
		/// Gets the value for the resize delta.
		/// </summary>
		public float ResizeDelta
		{
			get
			{
				return this.resizeDelta;
			}
		}
	}
}