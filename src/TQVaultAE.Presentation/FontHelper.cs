using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;

namespace TQVaultAE.Presentation
{
	public static class FontHelper
	{
		public static IAddFontToOS FontLoader { get; set; }

		private static FontFamily _FONT_ALBERTUSMT = new FontFamily("Albertus MT");
		internal static FontFamily FONT_ALBERTUSMT
		{
			get
			{
				if (FontLoader != null) _FONT_ALBERTUSMT = FontLoader.AddFontToOS(Resources.AlbertusMT);// Runtime
				return _FONT_ALBERTUSMT;
			}
		}

		private static FontFamily _FONT_ALBERTUSMTLIGHT = new FontFamily("Albertus MT Light");
		internal static FontFamily FONT_ALBERTUSMTLIGHT
		{
			get
			{
				if (FontLoader != null) _FONT_ALBERTUSMTLIGHT = FontLoader.AddFontToOS(Resources.AlbertusMTLight);// Runtime
				return _FONT_ALBERTUSMTLIGHT;
			}
		}

		public static Font GetFontMicrosoftSansSerif(float fontSize, float? scale = null)
		{
			scale = scale ?? 1F;
			return new Font("Microsoft Sans Serif", fontSize * scale.Value);
		}

		public static Font GetFontAlbertusMT(float fontSize, FontStyle fontStyle, GraphicsUnit unit, byte b)
		{
			return new Font(FontHelper.FONT_ALBERTUSMT, fontSize, fontStyle, unit, b);
		}

		public static Font GetFontAlbertusMT(float fontSize, GraphicsUnit unit)
		{
			return new Font(FontHelper.FONT_ALBERTUSMT, fontSize, unit);
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

	}
}
