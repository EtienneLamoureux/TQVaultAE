using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Character;
using TQVaultAE.Domain.Results;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Tooltips;

/// <summary>
/// The relic column and the granted skills, drawn on the right of the item panel as more tooltips.
/// </summary>
public partial class ItemTooltip
{
	/// <summary>Color made transparent around the two panels, so they look like two tooltips.</summary>
	private static readonly Color SkillsTransparencyKey = Color.FromArgb(255, 0, 255);

	/// <summary>Space between the item panel and the skills panel, in design pixels.</summary>
	private const int SkillsPanelGap = 4;

	/// <summary>Size of a skill icon, in design pixels (the game icons are 32 x 32).</summary>
	private const int SkillIconSize = 32;

	/// <summary>Height a panel may take on the screen of the main window, frame and padding included.</summary>
	private int MaxPanelHeight()
	{
		var screen = Screen.FromControl(this.Owner as Control ?? this).WorkingArea;
		return Math.Max(screen.Height - 2, 1);
	}

	/// <summary>
	/// Moves the rows <paramref name="start"/> to <paramref name="end"/> (excluded) of the item panel to a new panel,
	/// without the separator that opened them.
	/// </summary>
	private FlowLayoutPanel MoveRowsToColumn(int start, int end)
	{
		var source = this.flowLayoutPanelFriendlyNames;
		var rows = source.Controls.Cast<Control>().Skip(start).Take(end - start).ToList();
		var column = new FlowLayoutPanel()
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			BackColor = source.BackColor,
			BorderStyle = source.BorderStyle,
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			Padding = source.Padding,
			Margin = new Padding(0),
		};

		source.SuspendLayout();
		column.SuspendLayout();
		for (int i = 0; i < rows.Count; i++)
		{
			source.Controls.Remove(rows[i]);
			bool leadingSeparator = i == 0 && (rows[i].Anchor & AnchorStyles.Right) == AnchorStyles.Right;
			if (leadingSeparator)
				rows[i].Dispose();
			else
				column.Controls.Add(rows[i]);
		}
		column.ResumeLayout();
		source.ResumeLayout();
		this.PerformLayout();

		return column;
	}

	/// <summary>
	/// Shows the item panel with, on its right, the relic column if any and the granted skills in columns no taller than
	/// the screen, as one image with transparent gaps; caches that image under <paramref name="key"/>.
	/// </summary>
	private void ShowComposite(TooltipKey key, FlowLayoutPanel? relicColumn, GrantedSkill[] skills)
	{
		var bg = this.flowLayoutPanelFriendlyNames.BackColor;
		var scale = this.UIService.Scale;
		var gap = (int)(SkillsPanelGap * scale);
		var pad = this.flowLayoutPanelFriendlyNames.Padding.Left + 1;
		var separator = (int)(9 * scale);

		var panels = new List<Bitmap> { Rasterize(this.flowLayoutPanelFriendlyNames) };
		try
		{
			if (relicColumn is not null)
			{
				this.Controls.Add(relicColumn);
				relicColumn.PerformLayout();
				panels.Add(Rasterize(relicColumn));
				this.Controls.Remove(relicColumn);
				relicColumn.Dispose();
			}

			// A window cannot be taller than the screen: the skills that do not fit go in another column.
			var maxHeight = Math.Max(this.MaxPanelHeight() - (2 * pad), 1);
			var blocks = skills.Select(skill => this.RasterizeSkill(skill, bg)).ToList();
			try
			{
				var columnOf = ItemTooltipContentBuilder.PackColumns(blocks.Select(b => b.Height).ToList(), separator, maxHeight);
				var columns = new List<List<Bitmap>>();
				for (int i = 0; i < blocks.Count; i++)
				{
					bool newColumn = columnOf[i] == columns.Count;
#if DEBUG
					this.AutomationRecorder?.PlaceSkill(newColumn);
#endif
					if (newColumn)
						columns.Add([]);
					columns[^1].Add(blocks[i]);
				}

				panels.AddRange(columns.Select(column => DrawColumn(column, bg, pad, separator)));
			}
			finally
			{
				blocks.ForEach(b => b.Dispose());
			}

			var frame = this.Frame();
			var width = panels.Sum(p => p.Width + frame.Horizontal) + (gap * (panels.Count - 1));
			var composite = new Bitmap(width, panels.Max(p => p.Height) + frame.Vertical);
			using (var g = Graphics.FromImage(composite))
			{
				g.Clear(SkillsTransparencyKey);
				int x = 0;
				foreach (var panel in panels)
				{
					this.DrawFramed(g, panel, x, bg);
					x += panel.Width + frame.Horizontal + gap;
				}
			}

			ToImage.Add(key, composite, this.Data!);
		}
		finally
		{
			panels.ForEach(p => p.Dispose());
		}

		if (ToImage.TryGetCopy(key, out var shown, out _))
			this.ShowCompositeImage(shown);
	}

	/// <summary>Lays out the rows of one skill on the form and draws them.</summary>
	private Bitmap RasterizeSkill(GrantedSkill skill, Color bg)
	{
		var panel = new FlowLayoutPanel()
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			BackColor = bg,
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			Padding = new Padding(0),
			Margin = new Padding(0),
		};

		panel.SuspendLayout();
		this.AddSkillRows(panel, skill, bg);
		panel.ResumeLayout();

		this.Controls.Add(panel);
		panel.PerformLayout();
#if DEBUG
		this.AutomationRecorder?.SkillBlock(panel);
#endif
		try
		{
			return Rasterize(panel);
		}
		finally
		{
			this.Controls.Remove(panel);
			panel.Dispose();
		}
	}

	/// <summary>Stacks skill images in a panel like the item one: padded, with a line between two skills.</summary>
	private static Bitmap DrawColumn(List<Bitmap> blocks, Color bg, int pad, int separator)
	{
		var width = blocks.Max(b => b.Width) + (2 * pad);
		var height = blocks.Sum(b => b.Height) + (separator * (blocks.Count - 1)) + (2 * pad);
		var image = new Bitmap(width, height);
		using var g = Graphics.FromImage(image);
		g.Clear(bg);
		using (var frame = new Pen(Color.Black))
			g.DrawRectangle(frame, 0, 0, width - 1, height - 1);

		using var line = new Pen(TQColor.DarkGray.Color());
		int y = pad;
		for (int i = 0; i < blocks.Count; i++)
		{
			if (i > 0)
			{
				var middle = y + (separator / 2) - 1;
				g.DrawLine(line, pad, middle, width - pad - 1, middle);
				y += separator;
			}
			g.DrawImageUnscaled(blocks[i], pad, y);
			y += blocks[i].Height;
		}

		return image;
	}

	/// <summary>Shows a cached tooltip image; around composite panels, the transparency key color is transparent.</summary>
	private void ShowCompositeImage(Bitmap image)
	{
		this.BackColor = SkillsTransparencyKey;
		this.TransparencyKey = SkillsTransparencyKey;
		this.ShowCachedImage(image);
	}

	/// <summary>The game's golden rollover border, as the character editor's tool tips have; null without the game files.</summary>
	private GameTooltipFrame? GameFrame => GameTooltipFrame.Load(this.UIService);

	/// <summary>The room the border takes around a panel: the game's golden border, or a 1 pixel line without it.</summary>
	private Padding Frame() => this.GameFrame?.Thickness(this.UIService.Scale) ?? new Padding(1);

	/// <summary>
	/// Draws <paramref name="image"/> at <paramref name="x"/> inside the tooltip border: the game's golden rollover
	/// border, or a 1 pixel gray line when the game files lack it.
	/// </summary>
	private void DrawFramed(Graphics g, Bitmap image, int x, Color background)
	{
		var frame = this.Frame();
		var bounds = new Rectangle(x, 0, image.Width + frame.Horizontal, image.Height + frame.Vertical);
		if (this.GameFrame is { } gameFrame)
		{
			using (var back = new SolidBrush(background))
				g.FillRectangle(back, bounds);
			g.DrawImageUnscaled(image, x + frame.Left, frame.Top);
			gameFrame.Draw(g, bounds, this.UIService.Scale);
			return;
		}

		using var pen = new Pen(TQColor.DarkGray.Color());
		g.DrawRectangle(pen, x, 0, image.Width + 1, image.Height + 1);
		g.DrawImageUnscaled(image, x + 1, 1);
	}

	private static Bitmap Rasterize(Control control)
	{
		var image = new Bitmap(control.Width, control.Height);
		control.DrawToBitmap(image, new Rectangle(0, 0, control.Width, control.Height));
		return image;
	}

	/// <summary>Icon, name and level, activation, description, then the effects of one skill.</summary>
	private void AddSkillRows(FlowLayoutPanel panel, GrantedSkill skill, Color bg)
	{
		var header = new FlowLayoutPanel()
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			BackColor = bg,
			FlowDirection = FlowDirection.LeftToRight,
			WrapContents = false,
			Margin = new Padding(0, 0, 0, (int)(2 * this.UIService.Scale)),
			Padding = new Padding(0),
		};

		if (skill.Icon is not null)
		{
			var size = (int)(SkillIconSize * this.UIService.Scale);
			header.Controls.Add(new PictureBox()
			{
				Image = this.UIService.LoadBitmap(skill.Icon),
				SizeMode = PictureBoxSizeMode.StretchImage,
				Size = new Size(size, size),
				BackColor = bg,
				Margin = new Padding(0, 0, (int)(4 * this.UIService.Scale), 0),
			});
		}

		var title = new FlowLayoutPanel()
		{
			AutoSize = true,
			AutoSizeMode = AutoSizeMode.GrowAndShrink,
			BackColor = bg,
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			Margin = new Padding(0),
			Padding = new Padding(0),
			Anchor = AnchorStyles.Left,
		};
		var content = ItemTooltipContentBuilder.BuildSkill(skill);
		title.Controls.Add(this.MakeTitleLabel(content.Name, bg));
		title.Controls.Add(this.MakeTitleLabel(content.Level, bg));
		header.Controls.Add(title);
		panel.Controls.Add(header);

		foreach (var line in content.Body)
			AddPanelRow(panel, line, bg);
	}

	private Label MakeTitleLabel(TooltipLine line, Color bg)
		=> MakeSingleColorLabel(this.UIService, this.FontService, line.Text, line.Color?.Color(), 10F, line.Bold ? FontStyle.Bold : FontStyle.Regular, bg);

	private void AddPanelRow(FlowLayoutPanel panel, TooltipLine line, Color bg)
	{
		var text = line.Kind == TooltipLineKind.Spacer ? TOOLTIPSPACER : line.Text;
		if (MakeRow(this.UIService, this.FontService, text, line.Color?.Color(), BGColor: bg) is { } row)
			panel.Controls.Add(row);
	}
}
