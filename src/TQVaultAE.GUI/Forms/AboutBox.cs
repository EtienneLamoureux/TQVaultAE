//-----------------------------------------------------------------------
// <copyright file="AboutBox.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI.Forms;

using System;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using TQVaultAE.Presentation;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Infrastructure;

/// <summary>
/// Class for About dialog box.
/// </summary>
internal sealed partial class AboutBox : VaultForm
{
	/// <summary>
	/// Initializes a new instance of the AboutBox class.
	/// </summary>
	public AboutBox(GuiServices services) : base(services)
	{

		this.InitializeComponent();

		#region Apply custom font

		// The sizes of the other marble dialogs: the text in the base size, the button larger.
		var text = FontService.GetFontLight(CaravanLook.TextSize);
		this.labelProductName.Font = text;
		this.labelVersion.Font = text;
		this.labelCopyright.Font = text;
		this.textBoxDescription.Font = text;
		this.buttonOK.Font = FontService.GetFontLight(CaravanLook.ButtonTextSize);

		#endregion

		// Initialize the AboutBox to display the product information from the assembly information.
		// Change assembly information settings for your application through either:
		// - Project->Properties->Application->Assembly Information
		// - AssemblyInfo.cs
		this.Text = string.Format(CultureInfo.CurrentCulture, Resources.AboutText, AssemblyTitle);
		this.labelProductName.Text = AssemblyProduct;
		this.labelVersion.Text = string.Format(CultureInfo.CurrentCulture, Resources.AboutVersion, AssemblyVersion);
		this.labelCopyright.Text = AssemblyCopyright;
		this.textBoxDescription.Text = Resources.AboutDescription; // AssemblyDescription;
		this.buttonOK.Text = Resources.GlobalOK;

		// The look of Vault Maintenance: the marble behind the table, the labels over it with their shadow, and the
		// description, a text box that draws its own content from its corner, on black in a caravan frame drawn around it.
		CaravanFrame.FrameAround(this.textBoxDescription, spaceText: true);
		CaravanLook.Apply(this, this.tableLayoutPanel);

		this.NormalizeBox = false;
		this.DrawCustomBorder = true;
	}

	#region Assembly Attribute Accessors

	/// <summary>
	/// Gets Assembly Title.
	/// </summary>
	public static string? AssemblyTitle
	{
		get
		{
			// Get all Title attributes on this assembly
			object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);

			// If there is at least one Title attribute
			if (attributes.Length > 0)
			{
				// Select the first one
				AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];

				// If it is not an empty string, return it
				if (titleAttribute.Title.Length != 0)
				{
					return titleAttribute.Title;
				}
			}

			// If there was no Title attribute, or if the Title attribute was the empty string, return the .exe name
			return Assembly.GetExecutingAssembly().GetName().Name;
		}
	}

	/// <summary>
	/// Gets Assembly Version.
	/// </summary>
	public static string AssemblyVersion
	{
		get
		{
			return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty;
		}
	}

	/// <summary>
	/// Gets Assembly Description.
	/// </summary>
	public static string AssemblyDescription
	{
		get
		{
			// Get all Description attributes on this assembly
			object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);

			// If there aren't any Description attributes, return an empty string
			if (attributes.Length == 0)
			{
				return string.Empty;
			}

			// If there is a Description attribute, return its value
			return ((AssemblyDescriptionAttribute)attributes[0]).Description;
		}
	}

	/// <summary>
	/// Gets Assembly Product Name
	/// </summary>
	public static string AssemblyProduct
	{
		get
		{
			// Get all Product attributes on this assembly
			object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);

			// If there aren't any Product attributes, return an empty string
			if (attributes.Length == 0)
			{
				return string.Empty;
			}

			// If there is a Product attribute, return its value
			return ((AssemblyProductAttribute)attributes[0]).Product;
		}
	}

	/// <summary>
	/// Gets Assembly Copyright
	/// </summary>
	public static string AssemblyCopyright
	{
		get
		{
			// Get all Copyright attributes on this assembly
			object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);

			// If there aren't any Copyright attributes, return an empty string
			if (attributes.Length == 0)
			{
				return string.Empty;
			}

			// If there is a Copyright attribute, return its value
			return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
		}
	}

	/// <summary>
	/// Gets Assembly Company Name
	/// </summary>
	public static string AssemblyCompany
	{
		get
		{
			// Get all Company attributes on this assembly
			object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);

			// If there aren't any Company attributes, return an empty string
			if (attributes.Length == 0)
			{
				return string.Empty;
			}

			// If there is a Company attribute, return its value
			return ((AssemblyCompanyAttribute)attributes[0]).Company;
		}
	}

	#endregion Assembly Attribute Accessors

	/// <summary>
	/// Override of ScaleControl which grows the dialog's font with the zoom. The logo follows its box by itself
	/// (<see cref="PictureBoxSizeMode.Zoom"/>).
	/// </summary>
	/// <param name="factor">SizeF for the scale factor</param>
	/// <param name="specified">BoundsSpecified value.</param>
	protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
	{
		this.Font = ZoomScope.ScaleFont(this.Font, factor.Height);
		base.ScaleControl(factor, specified);
	}
}