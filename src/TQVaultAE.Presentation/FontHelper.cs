﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;

namespace TQVaultAE.Presentation
{
	public static class FontHelper
	{
		private static PrivateFontCollection privateFontCollection = new PrivateFontCollection();

		private static FontFamily initCustomFont(byte[] fontData)
		{
			unsafe
			{
				fixed (byte* pinptr = fontData)
				{
					IntPtr ptr = (IntPtr)pinptr;
					privateFontCollection.AddMemoryFont(ptr, fontData.Length);
				}
			}
			return privateFontCollection.Families.Last();
		}

		private static FontFamily _FONT_ALBERTUSMT = null;
		internal static FontFamily FONT_ALBERTUSMT
		{
			get
			{
				if (_FONT_ALBERTUSMT is null) _FONT_ALBERTUSMT = initCustomFont(Resources.AlbertusMT);
				return _FONT_ALBERTUSMT;
			}
		}

		private static FontFamily _FONT_ALBERTUSMTLIGHT = null;
		internal static FontFamily FONT_ALBERTUSMTLIGHT
		{
			get
			{
				if (_FONT_ALBERTUSMTLIGHT is null) _FONT_ALBERTUSMTLIGHT = initCustomFont(Resources.AlbertusMTLight);
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
