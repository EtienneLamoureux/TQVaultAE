//-----------------------------------------------------------------------
// <copyright file="AboutBox.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVault
{
	using System;
	using System.Drawing;
	using System.Globalization;
	using System.Reflection;
	using System.Windows.Forms;
	using TQVault.Properties;
	using TQVaultData;

	/// <summary>
	/// Class for About dialog box.
	/// </summary>
	internal partial class AboutBox : VaultForm
	{
		/// <summary>
		/// Initializes a new instance of the AboutBox class.
		/// </summary>
		public AboutBox()
		{
			this.InitializeComponent();

			// Initialize the AboutBox to display the product information from the assembly information.
			// Change assembly information settings for your application through either:
			// - Project->Properties->Application->Assembly Information
			// - AssemblyInfo.cs
			this.Text = string.Format(CultureInfo.CurrentCulture, Resources.AboutText, AssemblyTitle);
			this.labelProductName.Text = AssemblyProduct;
			this.labelVersion.Text = string.Format(CultureInfo.CurrentCulture, Resources.AboutVersion, AssemblyVersion);
			this.labelCopyright.Text = AssemblyCopyright;
			////this.labelCompanyName.Text = AssemblyCompany;
			this.textBoxDescription.Text = Resources.AboutDescription; // AssemblyDescription;

			// Check to see if we want to display the old UI.
			// There are other checks below as well when creating the panels.
			if (Settings.Default.EnableNewUI)
			{
				this.DrawCustomBorder = true;
			}
			else
			{
				this.Revert(new Size(435, 283));
			}
		}

		#region Assembly Attribute Accessors

		/// <summary>
		/// Gets Assembly Title.
		/// </summary>
		public static string AssemblyTitle
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
				return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
			}
		}

		/// <summary>
		/// Gets Assembly Version.
		/// </summary>
		public static string AssemblyVersion
		{
			get
			{
				return Assembly.GetExecutingAssembly().GetName().Version.ToString();
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
		/// Override of ScaleControl which supports picturebox image scaling.
		/// </summary>
		/// <param name="factor">SizeF for the scale factor</param>
		/// <param name="specified">BoundsSpecified value.</param>
		protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
		{
			this.Font = new Font(this.Font.Name, this.Font.SizeInPoints * factor.Height, this.Font.Style);

			if (this.logoPictureBox != null && this.logoPictureBox.Image != null)
			{
				this.logoPictureBox.Image = new Bitmap(
					this.logoPictureBox.Image,
					new Size(Convert.ToInt32((float)this.logoPictureBox.Size.Width * Database.DB.Scale), Convert.ToInt32((float)this.logoPictureBox.Size.Height * Database.DB.Scale)));
			}

			base.ScaleControl(factor, specified);
		}

		/// <summary>
		/// Reverts the form back to the original skin.
		/// </summary>
		/// <param name="originalSize">original size of the form before skinning.</param>
		protected override void Revert(Size originalSize)
		{
			// Restore the borders and set the form back to the original size.
			this.DrawCustomBorder = false;
			this.ClientSize = originalSize;

			this.labelProductName.Location = new Point(143, 0);
			this.labelProductName.Size = new Size(271, 17);
			this.labelProductName.Font = new Font("Albertus MT", 9.0F, FontStyle.Regular, GraphicsUnit.Point, (byte)0);

			this.labelVersion.Location = new Point(143, 26);
			this.labelVersion.Size = new Size(271, 17);
			this.labelVersion.Font = new Font("Albertus MT", 9.0F, FontStyle.Regular, GraphicsUnit.Point, (byte)0);

			this.labelCopyright.Location = new Point(143, 52);
			this.labelCopyright.Size = new Size(271, 17);
			this.labelCopyright.Font = new Font("Albertus MT", 9.0F, FontStyle.Regular, GraphicsUnit.Point, (byte)0);

			this.tableLayoutPanel.Location = new Point(9, 9);
			this.tableLayoutPanel.Size = new Size(417, 265);
			this.tableLayoutPanel.RowStyles[0].Height = 10.0F;
			this.tableLayoutPanel.RowStyles[1].Height = 10.0F;
			this.tableLayoutPanel.RowStyles[2].Height = 16.60377F;
			this.tableLayoutPanel.RowStyles[3].Height = 3.018868F;
			this.tableLayoutPanel.RowStyles[4].Height = 50.0F;
			this.tableLayoutPanel.RowStyles[5].Height = 10.0F;

			this.textBoxDescription.Location = new Point(193, 142);
			this.textBoxDescription.Size = new Size(372, 173);
			this.textBoxDescription.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, (byte)0);

			this.buttonOK.Revert(new Point(342, 240), new Size(75, 22));

			this.logoPictureBox.Size = new Size(131, 259);
			this.logoPictureBox.Image = new Bitmap(
				Resources.AboutGraphic,
				new Size(Convert.ToInt32((float)this.logoPictureBox.Size.Width * Database.DB.Scale), Convert.ToInt32((float)this.logoPictureBox.Size.Height * Database.DB.Scale)));
		}
	}
}