//-----------------------------------------------------------------------
// <copyright file="AboutBox.cs" company="VillageIdiot">
//     Copyright (c) Village Idiot. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Reflection;

namespace ArzExplorer;

/// <summary>
/// Windows Form which represents the About Box.
/// </summary>
public partial class AboutBox : Form
{
	/// <summary>
	/// Initializes a new instance of the AboutBox class.
	/// </summary>
	public AboutBox()
	{
		this.InitializeComponent();

		// The product information comes from the assembly attributes (project properties).
		this.Text = $"About {this.AssemblyTitle}";
		this.labelProductName.Text = this.AssemblyProduct;
		this.labelVersion.Text = $"Version {this.AssemblyVersion}";
		this.labelCopyright.Text = this.AssemblyCopyright;
		this.labelCompanyName.Text = this.AssemblyCompany;
		this.textBoxDescription.Text = this.AssemblyDescription;
	}

	#region Assembly Attribute Accessors

	/// <summary>
	/// Assembly title, or the executable name when it has none.
	/// </summary>
	public string AssemblyTitle
		=> GetAttribute<AssemblyTitleAttribute>()?.Title is { Length: > 0 } title
			? title
			: Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty;

	/// <summary>
	/// Assembly version.
	/// </summary>
	public string AssemblyVersion
		=> Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty;

	/// <summary>
	/// Assembly description.
	/// </summary>
	public string AssemblyDescription
		=> GetAttribute<AssemblyDescriptionAttribute>()?.Description ?? string.Empty;

	/// <summary>
	/// Assembly product name.
	/// </summary>
	public string AssemblyProduct
		=> GetAttribute<AssemblyProductAttribute>()?.Product ?? string.Empty;

	/// <summary>
	/// Assembly copyright message.
	/// </summary>
	public string AssemblyCopyright
		=> GetAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? string.Empty;

	/// <summary>
	/// Assembly company name.
	/// </summary>
	public string AssemblyCompany
		=> GetAttribute<AssemblyCompanyAttribute>()?.Company ?? string.Empty;

	/// <summary>
	/// The first attribute of the type on this assembly, or null.
	/// </summary>
	private static T? GetAttribute<T>() where T : Attribute
		=> Assembly.GetExecutingAssembly().GetCustomAttributes<T>().FirstOrDefault();

	#endregion Assembly Attribute Accessors
}
