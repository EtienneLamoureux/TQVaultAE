using System.Runtime.CompilerServices;
using System.Windows.Forms.VisualStyles;
using TQVaultAE.GUI.Components.Rendering;
using TQVaultAE.Presentation.Win32;

namespace TQVaultAE.GUI.Infrastructure;

/// <summary>
/// The black box with a thin gold frame of <see cref="Images.caravan_bg"/>, at the size of any control: drawn in nine
/// parts, so that the frame keeps its thickness and only the black inside stretches.
/// </summary>
internal static class CaravanFrame
{
	/// <summary>The thickness of the frame in the picture, in pixels (7, and one of black inside).</summary>
	public const int Border = 8;

	/// <summary>The gap between the glyph and the text of a check box, as the system draws it.</summary>
	private const int CheckBoxTextGap = 3;

	/// <summary>How a framed group's top edge meets its caption (<see cref="ApplyWithCaption"/>).</summary>
	public enum CaptionEdge
	{
		/// <summary>The top edge is broken behind the caption, from its first to its last pixel of text (or of the check box's glyph).</summary>
		Broken,

		/// <summary>The top edge runs whole, level with the middle of the caption, the caption over it.</summary>
		Whole,
	}

	private static Bitmap? source;

	private static Bitmap Source => source ??= Images.caravan_bg;

	/// <summary>The controls given a frame, so that each gets one only once and the text inside is known to be on black.</summary>
	private static readonly ConditionalWeakTable<Control, object> Framed = new();

	/// <summary>The frame at <paramref name="size"/>.</summary>
	public static Bitmap Render(Size size)
	{
		var result = new Bitmap(Math.Max(1, size.Width), Math.Max(1, size.Height));
		using var g = Graphics.FromImage(result);
		g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
		g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
		g.Clear(Color.Black);

		var src = Source;
		int b = Math.Min(Border, Math.Min(result.Width, result.Height) / 2);
		int sw = src.Width, sh = src.Height, w = result.Width, h = result.Height;
		void Part(int dx, int dy, int dw, int dh, int sx, int sy, int sW, int sH)
		{
			if (dw > 0 && dh > 0)
				g.DrawImage(src, new Rectangle(dx, dy, dw, dh), new Rectangle(sx, sy, sW, sH), GraphicsUnit.Pixel);
		}

		// Corners, then the edges stretched along their length.
		Part(0, 0, b, b, 0, 0, b, b);
		Part(w - b, 0, b, b, sw - b, 0, b, b);
		Part(0, h - b, b, b, 0, sh - b, b, b);
		Part(w - b, h - b, b, b, sw - b, sh - b, b, b);
		Part(b, 0, w - 2 * b, b, b, 0, sw - 2 * b, b);
		Part(b, h - b, w - 2 * b, b, b, sh - b, sw - 2 * b, b);
		Part(0, b, b, h - 2 * b, 0, b, b, sh - 2 * b);
		Part(w - b, b, b, h - 2 * b, sw - b, b, b, sh - 2 * b);
		return result;
	}

	/// <summary>
	/// Whether <paramref name="control"/> or one of its parents has a frame: its text is then drawn on the frame's black,
	/// not over the window's background.
	/// </summary>
	public static bool IsInFrame(Control control)
	{
		for (Control? c = control; c is not null; c = c.Parent)
		{
			if (Framed.TryGetValue(c, out _))
				return true;
		}

		return false;
	}

	/// <summary>Records <paramref name="control"/> as framed; false when it already was.</summary>
	private static bool MarkFramed(Control control) => Framed.TryAdd(control, Framed);

	/// <summary>The width of one character of <paramref name="font"/>: the space kept between a frame and its text.</summary>
	public static int CharacterWidth(Font font)
		=> TextRenderer.MeasureText("0", font, Size.Empty, TextFormatFlags.NoPadding).Width;

	/// <summary>
	/// Makes <paramref name="control"/>, laid in a frame, transparent so that the black shows behind it. A grid or a list
	/// paints its own background (black in a frame) and cannot be transparent: it is left as it is.
	/// </summary>
	private static void ShowBlackBehind(Control control)
	{
		if (control is not (DataGridView or ListBox or ListView or TreeView))
			control.BackColor = Color.Transparent;
	}

	/// <summary>
	/// Gives <paramref name="control"/> the frame as its background, made again when its size changes (zoom, DPI), with
	/// a padding that keeps its text inside the frame.
	/// </summary>
	/// <param name="control">The control.</param>
	/// <param name="padding">The padding of its text; by default the frame's thickness on every side.</param>
	public static void ApplyTo(Control control, Padding? padding = null)
	{
		if (!MarkFramed(control))
			return;

		control.BackgroundImageLayout = ImageLayout.None;
		control.Padding = padding ?? new Padding(Border);
		void Refresh()
		{
			var old = control.BackgroundImage;
			control.BackgroundImage = Render(control.Size);
			old?.Dispose();
		}

		Refresh();
		control.SizeChanged += (_, _) => Refresh();
		control.Disposed += (_, _) => control.BackgroundImage?.Dispose();
	}

	/// <summary>
	/// A label in a frame as high as <paramref name="box"/>, the control it names on its right: from its left to just
	/// before the box, its text centered.
	/// </summary>
	public static void FrameLabel(Label label, Control box)
	{
		label.AutoSize = false;
		label.Bounds = new Rectangle(label.Left, box.Top, Math.Max(label.Width, box.Left - label.Left - 6), box.Height);
		label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		ApplyTo(label, new Padding(Border, 0, Border, 0));
	}

	/// <summary>
	/// Draws every group box under <paramref name="root"/> (a dialog, a panel) not framed yet as a framed group with its
	/// caption, see <see cref="ApplyWithCaption"/>.
	/// </summary>
	public static void ApplyWithCaptionToAll(Control root, CaptionEdge edge = CaptionEdge.Broken)
		=> root.ProcessAllControls(c =>
		{
			if (c is GroupBox box)
				ApplyWithCaption(box, edge);
		});

	/// <summary>
	/// Draws <paramref name="box"/> as the frame, whole, with its caption over the frame's top edge as a group box has
	/// it: its text, or the check box of a <see cref="Components.UIToolbox.CheckGroupBox"/>, in gold on a dark outline and
	/// no background. The caption is in the dialog's font: a font of the group's own is dropped, since it would follow
	/// neither the zoom nor a font change (the check box of a CheckGroupBox takes its group's font). The content gets a
	/// padding off the frame and is made transparent so that the black shows behind it (a grid or a list keeps its own
	/// background, black).
	/// </summary>
	/// <param name="box">The group.</param>
	/// <param name="edge">Whether the frame's top edge is broken behind the caption or runs whole under it.</param>
	public static void ApplyWithCaption(GroupBox box, CaptionEdge edge = CaptionEdge.Broken)
	{
		if (!MarkFramed(box))
			return;

		var frame = new FrameCache(box);
		box.ResetFont();
		box.BackColor = Color.Transparent;
		box.Padding = new Padding(Border + 2, box.Padding.Top, Border + 2, Border);
		foreach (Control child in box.Controls)
			ShowBlackBehind(child);

		// A text caption sits on the frame's top edge, lower than the middle of the text where a group box has its line:
		// the content goes down as much, so that the black above it keeps its height. A check box group's content is
		// laid out under its check box already.
		if (!box.Controls.OfType<CheckBox>().Any())
		{
			int shift = 0;
			void ShiftContent()
			{
				int next = CaptionShift(box.Font);
				if (next == shift)
					return;
				box.Padding = box.Padding with { Top = Math.Max(0, box.Padding.Top - shift + next) };
				shift = next;
			}

			ShiftContent();
			box.FontChanged += (_, _) => ShiftContent();
		}

		box.Paint += (_, e) =>
		{
			var check = box.Controls.OfType<CheckBox>().FirstOrDefault();
			var caption = check is not null
				? check.Bounds
				: new Rectangle(new Point(Border + 2, 0), TextRenderer.MeasureText(e.Graphics, box.Text, box.Font));
			// The caption sits on the frame's top edge: the bottom of that edge, where the black starts, is level with the
			// text's baseline. The text is drawn from the caption's top, or centred in the check box.
			var font = check?.Font ?? box.Font;
			int textTop = check is null
				? caption.Top
				: caption.Top + (check.ClientSize.Height - TextRenderer.MeasureText(e.Graphics, check.Text, font).Height) / 2;
			int top = Math.Max(0, textTop + Ascent(e.Graphics, font) - Border);

			// The group box drew its own caption and border line before this handler: above the frame they would show
			// next to the text drawn here, so that strip is painted again with the background. The frame is then drawn
			// whole, its top edge broken behind the caption when asked, as a group box breaks its line, and the caption
			// over it.
			if (top > 0)
				ButtonRenderer.DrawParentBackground(e.Graphics, new Rectangle(0, 0, box.Width, top), box);
			e.Graphics.DrawImageUnscaled(frame.Get(new Size(box.Width, box.Height - top)), 0, top);
			if (check is null)
			{
				if (edge == CaptionEdge.Broken)
				{
					var (pad, width) = TextExtent(e.Graphics, box.Text, box.Font, TextFormatFlags.Default);
					BreakTopEdge(e.Graphics, box, caption.Left + pad, caption.Left + pad + width, top);
				}

				TextShadow.DrawOutline(e.Graphics, box.Text, box.Font, caption, TextFormatFlags.Default);
				TextRenderer.DrawText(e.Graphics, box.Text, box.Font, caption, box.ForeColor);
				return;
			}

			// The check box is transparent: it paints this group behind itself, so the outline drawn here lies under the
			// text it draws, and its glyph, its clicks and its focus rectangle stay the system's.
			int left = CheckBoxRenderer.GetGlyphSize(e.Graphics, CheckBoxState.UncheckedNormal).Width + CheckBoxTextGap;
			var text = new Rectangle(
				check.Left + check.Padding.Left + left,
				check.Top,
				Math.Max(0, check.ClientSize.Width - check.Padding.Left - left),
				check.ClientSize.Height);
			var flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter;
			if (!check.UseMnemonic)
				flags |= TextFormatFlags.NoPrefix;
			if (edge == CaptionEdge.Broken)
			{
				var (pad, width) = TextExtent(e.Graphics, check.Text, check.Font, flags);
				BreakTopEdge(e.Graphics, box, check.Left + check.Padding.Left, text.Left + pad + width, top);
			}

			TextShadow.DrawOutline(e.Graphics, check.Text, check.Font, text, flags);
		};
	}

	/// <summary>
	/// How much lower the frame's top edge is under a text caption in <paramref name="font"/>, set on its baseline, than
	/// in the middle of the text.
	/// </summary>
	private static int CaptionShift(Font font)
	{
		using var graphics = Graphics.FromHwnd(IntPtr.Zero);
		int height = TextRenderer.MeasureText(graphics, "Ag", font).Height;
		return Math.Max(0, Ascent(graphics, font) - Border) - Math.Max(0, (height - Border) / 2);
	}

	/// <summary>The height of <paramref name="font"/> above its baseline, in pixels.</summary>
	private static int Ascent(Graphics graphics, Font font)
	{
		var family = font.FontFamily;
		return (int)Math.Round(font.GetHeight(graphics) * family.GetCellAscent(font.Style) / family.GetLineSpacing(font.Style));
	}

	/// <summary>
	/// Where the text drawn with <paramref name="flags"/> starts after the padding the text renderer adds, and its width
	/// without that padding.
	/// </summary>
	private static (int Pad, int Width) TextExtent(Graphics graphics, string text, Font font, TextFormatFlags flags)
	{
		int padded = TextRenderer.MeasureText(graphics, text, font, Size.Empty, flags).Width;
		int width = TextRenderer.MeasureText(graphics, text, font, Size.Empty, flags | TextFormatFlags.NoPadding).Width;
		return ((padded - width) / 2, width);
	}

	/// <summary>Paints the background back over the frame's top edge of <paramref name="box"/> between two columns.</summary>
	private static void BreakTopEdge(Graphics graphics, GroupBox box, int left, int right, int top)
	{
		if (right > left)
			ButtonRenderer.DrawParentBackground(graphics, new Rectangle(left, top, right - left, Border), box);
	}

	/// <summary>
	/// Draws the frame around <paramref name="control"/>, on its parent: the control gets a margin of the frame's
	/// thickness, in which the parent paints the frame. For a control that draws its own content from its corner (a list).
	/// </summary>
	public static void FrameAround(Control control)
	{
		if (control.Parent is not { } parent || !MarkFramed(control))
			return;

		control.Margin = new Padding(Border);
		var frame = new FrameCache(control);
		parent.Paint += (_, e) =>
		{
			if (!control.Visible)
				return;

			var bounds = Rectangle.Inflate(control.Bounds, Border, Border);
			e.Graphics.DrawImageUnscaled(frame.Get(bounds.Size), bounds.Location);
		};
		void Repaint(object? sender, EventArgs e) => parent.Invalidate();
		control.LocationChanged += Repaint;
		control.SizeChanged += Repaint;
		control.VisibleChanged += Repaint;
	}

	/// <summary>
	/// Draws <paramref name="box"/> as the frame instead of the group box's own border, without its caption. Its
	/// controls are moved off the frame's left edge and centered in its height (the caption's room is no longer needed),
	/// and made transparent so that the black shows behind them.
	/// </summary>
	/// <param name="box">The group box.</param>
	/// <param name="inset">The gap between the frame and its controls on the left; 4 pixels by default.</param>
	public static void ApplyTo(GroupBox box, int inset = 4)
	{
		if (!MarkFramed(box))
			return;

		var children = box.Controls.Cast<Control>().ToList();
		if (children.Count > 0)
		{
			int dy = (box.Height - children.Max(c => c.Bottom) - children.Min(c => c.Top)) / 2;
			foreach (var child in children)
			{
				ShowBlackBehind(child);
				child.Top += dy;
				if (child.Left < Border + inset)
					child.Left = Border + inset;
			}
		}

		var frame = new FrameCache(box);
		box.Paint += (_, e) => e.Graphics.DrawImageUnscaled(frame.Get(box.Size), 0, 0);
	}

	/// <summary>The frame drawn for one control, made again only when its size changes, disposed with the control.</summary>
	private sealed class FrameCache
	{
		private Bitmap? frame;

		public FrameCache(Control owner) => owner.Disposed += (_, _) => this.frame?.Dispose();

		public Bitmap Get(Size size)
		{
			if (this.frame is null || this.frame.Size != size)
			{
				this.frame?.Dispose();
				this.frame = Render(size);
			}

			return this.frame;
		}
	}
}
