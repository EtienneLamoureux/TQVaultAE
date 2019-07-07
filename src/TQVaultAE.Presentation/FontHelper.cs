using EnumsNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;

namespace TQVaultAE.Presentation
{
	public static class FontHelper
	{
		public static IAddFontToOS FontLoader { get; set; }

		private const string ALBERTUSMT_NAME = "Albertus MT";
		private const string ALBERTUSMTLIGHT_NAME = "Albertus MT Light";

		private static FontFamily _FONT_ALBERTUSMT = null;
		internal static FontFamily FONT_ALBERTUSMT
		{
			get
			{
				if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
				{
					// Code here won't run in Visual Studio designer but runtime 
					if (FontLoader != null && _FONT_ALBERTUSMT is null)
					{
						var baseFont = Enums.Parse<FontFamilyList>(Config.Settings.Default.BaseFont ?? FontFamilyList.AlbertusMT.ToString());
						switch (baseFont)
						{
							case FontFamilyList.AlbertusMT:
								_FONT_ALBERTUSMT = FontLoader.AddFontToOS(ALBERTUSMT_NAME, Resources.AlbertusMT);
								break;
							case FontFamilyList.Arial:
							case FontFamilyList.Verdana:
								_FONT_ALBERTUSMT = new FontFamily(baseFont.ToString());
								break;
							case FontFamilyList.TimesNewRoman:
								_FONT_ALBERTUSMT = new FontFamily(baseFont.AsString(EnumFormat.Description));
								break;
						}
					}
				}
				else
				{
					// Design time
					if (_FONT_ALBERTUSMT is null)
					{
						try
						{
							_FONT_ALBERTUSMT = new FontFamily(ALBERTUSMT_NAME);
						}
						catch
						{ // fallback to Safe font
							_FONT_ALBERTUSMT = new FontFamily("Times New Roman");
						}
					}
				}
				return _FONT_ALBERTUSMT;
			}
		}

		private static FontFamily _FONT_ALBERTUSMTLIGHT = null;
		internal static FontFamily FONT_ALBERTUSMTLIGHT
		{
			get
			{
				if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
				{
					// Code here won't run in Visual Studio designer
					if (FontLoader != null && _FONT_ALBERTUSMTLIGHT is null)
					{
						var baseFont = Enums.Parse<FontFamilyList>(Config.Settings.Default.BaseFont ?? FontFamilyList.AlbertusMT.ToString());
						switch (baseFont)
						{
							case FontFamilyList.AlbertusMT:
								_FONT_ALBERTUSMTLIGHT = FontLoader.AddFontToOS(ALBERTUSMTLIGHT_NAME, Resources.AlbertusMTLight);// Runtime
								break;
							case FontFamilyList.Arial:
							case FontFamilyList.Verdana:
								_FONT_ALBERTUSMTLIGHT = new FontFamily(baseFont.ToString());
								break;
							case FontFamilyList.TimesNewRoman:
								_FONT_ALBERTUSMTLIGHT = new FontFamily(baseFont.AsString(EnumFormat.Description));
								break;
						}
					}

				}
				else
				{
					// Design time
					if (_FONT_ALBERTUSMTLIGHT is null)
					{
						try
						{
							_FONT_ALBERTUSMTLIGHT = new FontFamily(ALBERTUSMTLIGHT_NAME);
						}
						catch
						{ // fallback to Safe font
							_FONT_ALBERTUSMTLIGHT = new FontFamily("Times New Roman");
						}
					}
				}

				return _FONT_ALBERTUSMTLIGHT;
			}
		}


		public static Font GetFontAlbertusMT(float fontSize, FontStyle fontStyle, GraphicsUnit unit, byte b)
		{
			return new Font(FontHelper.FONT_ALBERTUSMT, fontSize, fontStyle, unit, b);
		}

		public static Font GetFontAlbertusMT(float fontSize, GraphicsUnit unit)
		{
			return new Font(FontHelper.FONT_ALBERTUSMT, fontSize, unit);
		}

		public static Font GetFontAlbertusMT(float fontSize, FontStyle fontStyle, float? scale = null)
		{
			scale = scale ?? 1F;
			return new Font(FontHelper.FONT_ALBERTUSMT, fontSize * scale.Value, fontStyle);
		}

		public static Font GetFontAlbertusMT(float fontSize, float? scale = null)
		{
			scale = scale ?? 1F;
			return new Font(FontHelper.FONT_ALBERTUSMT, fontSize * scale.Value);
		}

		public static Font GetFontAlbertusMTLight(float fontSize, GraphicsUnit unit)
		{
			return new Font(FontHelper.FONT_ALBERTUSMT, fontSize, unit);
		}

		public static Font GetFontAlbertusMTLight(float fontSize, FontStyle fontStyle, GraphicsUnit unit, byte b)
		{
			return new Font(FontHelper.FONT_ALBERTUSMTLIGHT, fontSize, fontStyle, unit, b);
		}

		public static Font GetFontAlbertusMTLight(float fontSize)
		{
			return new Font(FontHelper.FONT_ALBERTUSMTLIGHT, fontSize);
		}

		public static Font GetFontAlbertusMTLight(float fontSize, float? scale = null)
		{
			scale = scale ?? 1F;
			return new Font(FontHelper.FONT_ALBERTUSMTLIGHT, fontSize * scale.Value);
		}
		public static Font GetFontAlbertusMTLight(float fontSize, FontStyle fontStyle, float? scale = null)
		{
			scale = scale ?? 1F;
			return new Font(FontHelper.FONT_ALBERTUSMTLIGHT, fontSize * scale.Value, fontStyle);
		}

	}
}
