using EnumsNET;
using System.ComponentModel;
using System.Drawing;
using TQVaultAE.Domain.Contracts.Services;

namespace TQVaultAE.Presentation
{
	public class FontService : IFontService
	{
		public IAddFontToOS FontLoader { get; private set; }

		const string ALBERTUSMT_NAME = "Albertus MT";
		const string ALBERTUSMTLIGHT_NAME = "Albertus MT Light";

		private FontFamily _FONT_ALBERTUSMT = null;
		private FontFamily FONT_ALBERTUSMT
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
							_FONT_ALBERTUSMT = new FontFamily("Arial");
						}
					}
				}
				return _FONT_ALBERTUSMT;
			}
		}

		private FontFamily _FONT_ALBERTUSMTLIGHT = null;

		private FontFamily FONT_ALBERTUSMTLIGHT
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
							_FONT_ALBERTUSMTLIGHT = new FontFamily("Arial");
						}
					}
				}

				return _FONT_ALBERTUSMTLIGHT;
			}
		}

		public FontService(IAddFontToOS addFontToOS)
		{
			this.FontLoader = addFontToOS;
		}

		public Font GetFontAlbertusMT(float fontSize, FontStyle fontStyle, GraphicsUnit unit, byte b)
			=> new Font(FONT_ALBERTUSMT, fontSize, fontStyle, unit, b);

		public Font GetFontAlbertusMT(float fontSize, GraphicsUnit unit)
			=> new Font(FONT_ALBERTUSMT, fontSize, unit);

		public Font GetFontAlbertusMT(float fontSize, FontStyle fontStyle, float? scale = null)
		{
			scale = scale ?? 1F;
			return new Font(FONT_ALBERTUSMT, fontSize * scale.Value, fontStyle);
		}

		public Font GetFontAlbertusMT(float fontSize, float? scale = null)
		{
			scale = scale ?? 1F;
			return new Font(FONT_ALBERTUSMT, fontSize * scale.Value);
		}

		public Font GetFontAlbertusMTLight(float fontSize, GraphicsUnit unit)
			=> new Font(FONT_ALBERTUSMT, fontSize, unit);

		public Font GetFontAlbertusMTLight(float fontSize, FontStyle fontStyle, GraphicsUnit unit, byte b)
			=> new Font(FONT_ALBERTUSMTLIGHT, fontSize, fontStyle, unit, b);

		public Font GetFontAlbertusMTLight(float fontSize) => new Font(FONT_ALBERTUSMTLIGHT, fontSize);

		public Font GetFontAlbertusMTLight(float fontSize, float? scale = null)
		{
			scale = scale ?? 1F;
			return new Font(FONT_ALBERTUSMTLIGHT, fontSize * scale.Value);
		}
		public Font GetFontAlbertusMTLight(float fontSize, FontStyle fontStyle, float? scale = null)
		{
			scale = scale ?? 1F;
			return new Font(FONT_ALBERTUSMTLIGHT, fontSize * scale.Value, fontStyle);
		}

	}
}
