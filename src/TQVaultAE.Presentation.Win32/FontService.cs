using EnumsNET;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.Presentation.Win32;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001", Justification = "Singleton: its font families live as long as the application.")]
public class FontService : IFontService
{
	private readonly UserSettings USettings;
	public IAddFontToOS FontLoader { get; private set; }

	const string ALBERTUSMT_NAME = "Albertus MT";
	const string ALBERTUSMTLIGHT_NAME = "Albertus MT Light";

	/// <summary>Raised by <see cref="Reload"/>, once the fonts use the family of <see cref="UserSettings.BaseFont"/>.</summary>
	public event EventHandler? FontChanged;

	/// <summary>The families of the base font in use, read from <see cref="UserSettings.BaseFont"/> on first use.</summary>
	private (FontFamily Regular, FontFamily Light)? _families;

	/// <summary>
	/// The families already made, one pair per base font: switching back to a base font reuses them, so the game font is
	/// added to the process once. Never disposed: the fonts made from them may still be on screen.
	/// </summary>
	private readonly Dictionary<FontFamilyList, (FontFamily Regular, FontFamily Light)> _familiesByBaseFont = [];

	/// <summary>
	/// Names of the families used before the last <see cref="Reload"/>, with whether it was the light one: a font of these
	/// families is one of the service's, maybe grown by a zoom, that <see cref="Rebase"/> moves to the new family.
	/// </summary>
	private readonly ConcurrentDictionary<string, bool> _retiredFamilies = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>Whether each font the service made is a light one, which a family name shared by both can't tell.</summary>
	private readonly ConditionalWeakTable<Font, StrongBox<bool>> _origins = [];

	private (FontFamily Regular, FontFamily Light) Families
	{
		get
		{
			if (this._families is { } families)
				return families;

			if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
			{
				// Code here won't run in Visual Studio designer but runtime
				if (this.FontLoader is null)
					return (FontFamily.GenericSansSerif, FontFamily.GenericSansSerif);

				var baseFont = Enums.Parse<FontFamilyList>(this.USettings.BaseFont ?? FontFamilyList.AlbertusMT.ToString());
				if (!this._familiesByBaseFont.TryGetValue(baseFont, out families))
				{
					families = (this.CreateFamily(baseFont, light: false), this.CreateFamily(baseFont, light: true));
					this._familiesByBaseFont[baseFont] = families;
				}
				families = (this.Covering(families.Regular), this.Covering(families.Light));
			}
			else
			{
				// Design time
				families = (CreateDesignFamily(ALBERTUSMT_NAME), CreateDesignFamily(ALBERTUSMTLIGHT_NAME));
			}

			this._families = families;
			return families;
		}
	}

	private FontFamily CreateFamily(FontFamilyList baseFont, bool light)
	{
		FontFamily? family = baseFont switch
		{
			FontFamilyList.AlbertusMT => light
				? this.FontLoader.AddFontToOS(ALBERTUSMTLIGHT_NAME, Resources.AlbertusMTLight)
				: this.FontLoader.AddFontToOS(ALBERTUSMT_NAME, Resources.AlbertusMT),
			FontFamilyList.Arial or FontFamilyList.Verdana => new FontFamily(baseFont.ToString()),
			FontFamilyList.TimesNewRoman => new FontFamily(baseFont.AsString(EnumFormat.Description) ?? baseFont.ToString()),
			_ => null,
		};

		// The font could not be created: let GDI+ pick a safe one rather than fail.
		return family ?? FontFamily.GenericSansSerif;
	}

	/// <summary>
	/// Text the fonts must draw (<see cref="EnsureCovers"/>); <see langword="null"/> before it is given. Shared by every
	/// instance: the game texts have one language for the whole process.
	/// </summary>
	private static volatile string? s_coverageSample;

	/// <summary>
	/// The family used for the letters the base font lacks: Arial, the font the game draws Russian with (its
	/// <c>arial_russian</c> font), which has the Latin, Greek and Cyrillic alphabets.
	/// </summary>
	private const string CoveringFamilyName = "Arial";

	/// <summary>Whether each family has every letter of a sample, by family name and sample.</summary>
	private static readonly ConcurrentDictionary<(string Family, string Sample), bool> s_covers = new();

	/// <summary>
	/// <paramref name="family"/>, or the covering family when <paramref name="family"/> lacks letters of the sample and
	/// that one has them all (no family is better for scripts it lacks too, such as Chinese: Windows' substitute then).
	/// </summary>
	private FontFamily Covering(FontFamily family)
	{
		if (s_coverageSample is not { } sample || this.CoversCached(family, sample))
			return family;

		var covering = this._coveringFamily ??= new FontFamily(CoveringFamilyName);
		return this.CoversCached(covering, sample) ? covering : family;
	}

	private FontFamily? _coveringFamily;

	private bool CoversCached(FontFamily family, string sample)
		=> s_covers.GetOrAdd((family.Name, sample), _ => this.Covers(family, sample));

	/// <inheritdoc/>
	public void EnsureCovers(string sample)
	{
		ArgumentNullException.ThrowIfNull(sample);
		if (string.Equals(sample, s_coverageSample, StringComparison.Ordinal))
			return;

		s_coverageSample = sample;
		var before = this._families;
		if (before is null)
			return;

		// Fonts were made already: the next ones use the covering family, and the windows move theirs to it.
		this._families = null;
		var after = this.Families;
		if (after.Regular.Name != before.Value.Regular.Name || after.Light.Name != before.Value.Light.Name)
		{
			this._families = before;
			this.Reload();
		}
	}

	/// <inheritdoc/>
	public bool Covers(FontFamily family, string text)
	{
		ArgumentNullException.ThrowIfNull(family);
		ArgumentNullException.ThrowIfNull(text);

		var letters = text.Where(char.IsLetter).Distinct().ToArray();
		if (letters.Length == 0)
			return true;

		using var font = new Font(family, 12F, family.IsStyleAvailable(FontStyle.Regular) ? FontStyle.Regular : FontStyle.Bold);
		return this.FontLoader.HasEveryGlyph(font, new string(letters));
	}

	private static FontFamily CreateDesignFamily(string name)
	{
		try
		{
			return new FontFamily(name);
		}
		catch (ArgumentException)
		{ // fallback to Safe font
			return new FontFamily("Arial");
		}
	}

	public FontService(IAddFontToOS addFontToOS, UserSettings uSettings)
	{
		this.FontLoader = addFontToOS;
		this.USettings = uSettings;
	}

	/// <summary>GDI+ default character set, the one the <see cref="Font"/> constructors without <c>gdiCharSet</c> use.</summary>
	private const byte DefaultCharSet = 1;

	/// <summary>
	/// Fonts already created, shared by every caller: a <see cref="Font"/> is immutable, and creating one per paint leaks GDI handles
	/// until the finalizer runs. Callers must not dispose them.
	/// </summary>
	private readonly ConcurrentDictionary<(bool Light, float Size, FontStyle Style, GraphicsUnit Unit, byte CharSet), Font> _fonts = new();

	private Font Get(bool light, float fontSize, FontStyle fontStyle = FontStyle.Regular, GraphicsUnit unit = GraphicsUnit.Point, byte charSet = DefaultCharSet)
		=> this._fonts.GetOrAdd((light, fontSize, fontStyle, unit, charSet), key =>
		{
			var families = this.Families;
			var font = new Font(key.Light ? families.Light : families.Regular, key.Size, key.Style, key.Unit, key.CharSet);
			this._origins.AddOrUpdate(font, new StrongBox<bool>(key.Light));
			return font;
		});

	public Font GetFont(float fontSize, FontStyle fontStyle, GraphicsUnit unit)
		=> this.Get(false, fontSize, fontStyle, unit);

	public Font GetFont(float fontSize, FontStyle fontStyle, GraphicsUnit unit, byte b)
		=> this.Get(false, fontSize, fontStyle, unit, b);

	public Font GetFont(float fontSize, GraphicsUnit unit)
		=> this.Get(false, fontSize, unit: unit);

	public Font GetFont(float fontSize, FontStyle fontStyle, float? scale = null)
		=> this.Get(false, fontSize * (scale ?? 1F), fontStyle);

	public Font GetFont(float fontSize, float? scale = null)
		=> this.Get(false, fontSize * (scale ?? 1F));

	public Font GetFontLight(float fontSize, GraphicsUnit unit)
		=> this.Get(false, fontSize, unit: unit);

	public Font GetFontLight(float fontSize, FontStyle fontStyle, GraphicsUnit unit)
		=> this.Get(true, fontSize, fontStyle, unit);

	public Font GetFontLight(float fontSize, FontStyle fontStyle, GraphicsUnit unit, byte b)
		=> this.Get(true, fontSize, fontStyle, unit, b);

	public Font GetFontLight(float fontSize)
		=> this.Get(true, fontSize);

	public Font GetFontLight(float fontSize, float? scale = null)
		=> this.Get(true, fontSize * (scale ?? 1F));

	public Font GetFontLight(float fontSize, FontStyle fontStyle, float? scale = null)
		=> this.Get(true, fontSize * (scale ?? 1F), fontStyle);

	private readonly ConcurrentDictionary<(string Family, float Size, FontStyle Style, GraphicsUnit Unit, byte CharSet), Font> _styled = new();

	public Font WithStyle(Font font, FontStyle style)
	{
		ArgumentNullException.ThrowIfNull(font);
		if (font.Style == style)
			return font;

		return this._styled.GetOrAdd((font.FontFamily.Name, font.Size, style, font.Unit, font.GdiCharSet), _ =>
		{
			var styled = new Font(font, style);
			if (this._origins.TryGetValue(font, out var light))
				this._origins.AddOrUpdate(styled, new StrongBox<bool>(light.Value));
			return styled;
		});
	}

	/// <inheritdoc/>
	public void Reload()
	{
		var previous = this._families;
		this._families = null;

		// Dropped, not disposed: the controls built with them keep showing them until they take the new ones.
		this._fonts.Clear();
		this._styled.Clear();

		if (previous is { } old)
		{
			// The light one last: a family name shared by both (a system font) is mostly used light. The fallback family
			// is left out: it is the default font of the controls the service never set.
			string fallback = FontFamily.GenericSansSerif.Name;
			if (!string.Equals(old.Regular.Name, fallback, StringComparison.OrdinalIgnoreCase))
				this._retiredFamilies[old.Regular.Name] = false;
			if (!string.Equals(old.Light.Name, fallback, StringComparison.OrdinalIgnoreCase))
				this._retiredFamilies[old.Light.Name] = true;
		}

		// Reads the setting again; a family back in use is no longer retired.
		var current = this.Families;
		this._retiredFamilies.TryRemove(current.Regular.Name, out _);
		this._retiredFamilies.TryRemove(current.Light.Name, out _);

		this.FontChanged?.Invoke(this, EventArgs.Empty);
	}

	/// <inheritdoc/>
	public Font Rebase(Font font)
	{
		ArgumentNullException.ThrowIfNull(font);

		var current = this.Families;
		string family = font.FontFamily.Name;
		bool light;
		if (this._origins.TryGetValue(font, out var origin))
			light = origin.Value;
		else if (!this._retiredFamilies.TryGetValue(family, out light))
			return font; // Not one of the service's fonts.

		// Already in the base font in use.
		if (string.Equals(family, (light ? current.Light : current.Regular).Name, StringComparison.OrdinalIgnoreCase))
			return font;

		return this.Get(light, font.Size, font.Style, font.Unit, font.GdiCharSet);
	}
}
