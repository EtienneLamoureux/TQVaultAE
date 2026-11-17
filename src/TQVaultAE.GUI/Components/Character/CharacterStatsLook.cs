using System.Drawing;
using TQVaultAE.GUI.Tooltips;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Components.Character;

/// <summary>
/// The fonts and the game-style tool tip of read-only statistics panels at a scale: one rule for the main window's
/// panel and the floating character window's four. The fonts of the font service are shared; the two Arial fonts and the
/// tool tip are made here and disposed when made again.
/// </summary>
internal sealed class CharacterStatsLook(IFontService fontService, IUIService uiService) : IDisposable
{
	private readonly List<Font> _owned = [];

	/// <summary>The scale the fonts were made at; 0 before the first <see cref="Apply"/> or after <see cref="Reset"/>.</summary>
	public float Scale { get; private set; }

	/// <summary>The tool tip of the panels, made with the fonts.</summary>
	public GameTooltip? Tooltip { get; private set; }

	/// <summary>The fonts are made again at the next <see cref="Apply"/> (the base font changed).</summary>
	public void Reset() => this.Scale = 0;

	/// <summary>Gives the panels their scale, fonts and tool tip, made again only when the scale changed.</summary>
	/// <returns>True when they were made again.</returns>
	public bool Apply(IReadOnlyCollection<CharacterStatsPanel> panels, float scale)
	{
		if (scale == this.Scale)
			return false;

		this.Scale = scale;
		var old = this._owned.ToList();
		this._owned.Clear();
		var number = this.Owned(new Font("Arial", 13F * scale, FontStyle.Bold, GraphicsUnit.Pixel));
		var resistance = this.Owned(new Font("Arial", 14F * scale, FontStyle.Regular, GraphicsUnit.Pixel));
		foreach (var panel in panels)
		{
			panel.PanelScale = scale;
			panel.NameFont = this.Light(20F, FontStyle.Regular);
			panel.ClassFont = this.Light(16F, FontStyle.Regular);
			panel.TextFont = this.Light(15F, FontStyle.Regular);
			panel.TitleFont = this.Light(16F, FontStyle.Bold);
			panel.TabFont = this.Light(15F, FontStyle.Regular);
			panel.NumberFont = number;
			panel.ResistanceFont = resistance;
		}

		this.Tooltip?.Dispose();
		this.Tooltip = new GameTooltip(this.Light(17F, FontStyle.Bold), this.Light(15F, FontStyle.Regular), scale, GameTooltipFrame.Load(uiService));
		foreach (var panel in panels)
			panel.Tooltip = this.Tooltip;

		foreach (var font in old)
			font.Dispose();

		return true;
	}

	/// <summary>A light game font of <paramref name="pixels"/> at the scale.</summary>
	public Font Light(float pixels, FontStyle style) => fontService.GetFontLight(pixels * this.Scale, style, GraphicsUnit.Pixel);

	private Font Owned(Font font)
	{
		this._owned.Add(font);
		return font;
	}

	public void Dispose()
	{
		this.Tooltip?.Dispose();
		foreach (var font in this._owned)
			font.Dispose();
		this._owned.Clear();
	}
}
