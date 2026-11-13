using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace TQVaultAE.GUI.Automation;

/// <summary>
/// MCP tool that finds text cut by its control: a translation longer than the English text, or a scaled font.
/// </summary>
internal static partial class AutomationTools
{
	[McpServerTool(Name = "text_fit", ReadOnly = true)]
	[Description("Measures the text of every label, button, check box, radio button and group box of an open form (the main window by default) with TextRenderer, in its font, and lists those whose text does not fit: wider or taller than the control (after word wrap where the control wraps), or a control that sticks out of its parent's client area. Each entry gives the control path, its text, its size, the size the text needs and the reason. Use it after changing the UI language to find translations that overflow.")]
	public static string TextFit(
		[Description("Part of the form title or its type name (case-insensitive); empty for the main window.")] string form = "",
		[Description("true to also check hidden controls (a hidden panel, an unselected tab page).")] bool includeHidden = false,
		[Description("Pixels of slack before a control counts as too small (default 1).")] int tolerance = 1)
		=> AutomationHost.OnUi(main =>
		{
			Form target = string.IsNullOrEmpty(form) ? main : FindOpenForm(form);
			var problems = new List<object>();
			int checkedControls = 0;

			foreach (var c in AllControls(target))
			{
				if (!includeHidden && !c.Visible)
					continue;
				if (string.IsNullOrWhiteSpace(c.Text) || c is not (Label or ButtonBase or GroupBox))
					continue;

				// Icon buttons (the title bar's minimize, maximize and close) carry a text they never draw.
				int iconSize = Math.Max((int)(2.5 * c.Font.Height), (int)Math.Ceiling(24 * c.DeviceDpi / 96.0));
				if (c is ButtonBase && c.Width < iconSize && c.Height < iconSize)
					continue;

				checkedControls++;
				foreach (var reason in FitProblems(c, tolerance))
				{
					problems.Add(new
					{
						path = ControlPath(c, target),
						type = c.GetType().Name,
						text = c.Text,
						font = $"{c.Font.Name} {c.Font.SizeInPoints:0.##}pt",
						size = new { c.Width, c.Height },
						needed = reason.Needed is { } n ? new { n.Width, n.Height } : null,
						reason = reason.Text,
						visible = c.Visible,
					});
				}
			}

			return Serialize(new { form = target.Text, type = target.GetType().Name, deviceDpi = target.DeviceDpi, checkedControls, problems });
		});

	private readonly record struct FitProblem(string Text, Size? Needed);

	private static IEnumerable<FitProblem> FitProblems(Control c, int tolerance)
	{
		// Room the control keeps around its text.
		int glyph = c is CheckBox or RadioButton ? (int)Math.Ceiling(16 * c.DeviceDpi / 96.0) : 0;
		int frame = c switch
		{
			Button => (int)Math.Ceiling(8 * c.DeviceDpi / 96.0),
			GroupBox => (int)Math.Ceiling(16 * c.DeviceDpi / 96.0),
			_ => 0,
		};
		int availableWidth = c.ClientSize.Width - (c is GroupBox ? 0 : c.Padding.Horizontal) - glyph - frame;
		int availableHeight = c.ClientSize.Height - c.Padding.Vertical;

		var text = c.Text.Replace("&&", "\u0001").Replace("&", string.Empty).Replace("\u0001", "&");
		var singleLine = TextRenderer.MeasureText(text, c.Font, Size.Empty, TextFormatFlags.NoPrefix);

		// AutoSize controls take the size of their text: check them against their parent instead.
		bool autoSize = c.AutoSize && c is not GroupBox;
		bool wraps = c is Label or ButtonBase && !autoSize;
		// A label of a fixed size with an ellipsis is shortened on purpose, the whole text in its tool tip: only its place
		// is checked. An auto size one wraps and draws no ellipsis on a word cut at its maximum width.
		bool ellipsis = c is Label { AutoEllipsis: true, AutoSize: false };
		bool fixedSize = !autoSize && !ellipsis;

		if (autoSize && c is Label && c.MaximumSize.Width > 0)
		{
			// An auto size label wrapped at its maximum width: a word longer than that width is cut.
			int longestWord = LongestWord(text, c.Font);
			if (longestWord > c.MaximumSize.Width - c.Padding.Horizontal + tolerance)
				yield return new FitProblem("a word is wider than the label's maximum width", new Size(longestWord, singleLine.Height));
		}
		else if (fixedSize && c is GroupBox)
		{
			if (singleLine.Width > availableWidth + tolerance)
				yield return new FitProblem("caption wider than the group box", singleLine);
		}
		else if (fixedSize && singleLine.Width > availableWidth + tolerance)
		{
			if (!wraps)
				yield return new FitProblem("text wider than the control", singleLine);
			else
			{
				var wrapped = TextRenderer.MeasureText(text, c.Font, new Size(Math.Max(availableWidth, 1), int.MaxValue),
					TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix | TextFormatFlags.TextBoxControl);
				int longestWord = LongestWord(text, c.Font);
				if (wrapped.Height > availableHeight + tolerance)
					yield return new FitProblem($"text needs {wrapped.Height}px of height after word wrap, has {availableHeight}px", wrapped);
				else if (longestWord > availableWidth + tolerance)
					yield return new FitProblem("a word is wider than the control", new Size(longestWord, wrapped.Height));
			}
		}
		else if (fixedSize && singleLine.Height > availableHeight + tolerance && !text.Contains('\n'))
		{
			yield return new FitProblem("text taller than the control", singleLine);
		}

		// Cut by the parent: the part outside its client area is not drawn.
		if (c.Parent is { } parent && parent is not Form or Form { AutoScroll: false })
		{
			var area = parent.ClientRectangle;
			if (parent is ScrollableControl { AutoScroll: true })
				area = Rectangle.Empty;
			if (!area.IsEmpty && (c.Right > area.Right + tolerance || c.Bottom > area.Bottom + tolerance))
				yield return new FitProblem($"sticks out of {parent.Name} ({c.Right}x{c.Bottom} beyond {area.Width}x{area.Height})", null);
		}
	}

	/// <summary>Width of the widest word of <paramref name="text"/>: the line can't wrap inside it.</summary>
	private static int LongestWord(string text, Font font)
		=> text.Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries)
			.Select(w => TextRenderer.MeasureText(w, font, Size.Empty, TextFormatFlags.NoPrefix).Width)
			.DefaultIfEmpty(0)
			.Max();

	private static string ControlPath(Control c, Control root)
	{
		var names = new List<string>();
		for (var x = c; x is not null && x != root; x = x.Parent)
			names.Add(string.IsNullOrEmpty(x.Name) ? x.GetType().Name : x.Name);
		names.Reverse();
		return string.Join('/', names);
	}
}
