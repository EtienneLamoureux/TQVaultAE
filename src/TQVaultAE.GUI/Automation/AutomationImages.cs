//-----------------------------------------------------------------------
// <copyright file="AutomationImages.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Buffers.Binary;
using System.Drawing.Imaging;
using ModelContextProtocol.Protocol;

namespace TQVaultAE.GUI.Automation;

/// <summary>
/// The pictures of the automation tools: rendering a control, encoding a PNG, saving it and returning it to the client.
/// </summary>
internal static class AutomationImages
{
	/// <summary>Encodes <paramref name="image"/> as PNG.</summary>
	public static byte[] Png(Image image)
	{
		using var stream = new MemoryStream();
		image.Save(stream, ImageFormat.Png);
		return stream.ToArray();
	}

	/// <summary>Draws <paramref name="control"/> to a bitmap of its size with <see cref="Control.DrawToBitmap"/>.</summary>
	public static Bitmap Render(Control control) => Render(control, control.Size);

	/// <summary>Draws <paramref name="control"/> to a bitmap of <paramref name="size"/> with <see cref="Control.DrawToBitmap"/>.</summary>
	public static Bitmap Render(Control control, Size size)
	{
		var bitmap = new Bitmap(size.Width, size.Height);
		control.DrawToBitmap(bitmap, new Rectangle(Point.Empty, size));
		return bitmap;
	}

	/// <summary>
	/// Draws the client area of <paramref name="form"/> with <paramref name="overlays"/> drawn over it at their screen
	/// places, on a magenta canvas that also holds the overlays outside the window (a tool tip below its border).
	/// </summary>
	public static Bitmap RenderWithOverlays(Form form, params (Image Image, Point Screen)[] overlays)
	{
		var formScreen = form.RectangleToScreen(form.ClientRectangle);
		var canvas = overlays.Aggregate(formScreen, (union, o) => Rectangle.Union(union, new Rectangle(o.Screen, o.Image.Size)));
		var bitmap = new Bitmap(canvas.Width, canvas.Height);
		using var g = Graphics.FromImage(bitmap);
		g.Clear(Color.Magenta);
		using (var main = Render(form, form.ClientSize))
			g.DrawImage(main, formScreen.X - canvas.X, formScreen.Y - canvas.Y);

		foreach (var (image, screen) in overlays)
			g.DrawImage(image, screen.X - canvas.X, screen.Y - canvas.Y);

		return bitmap;
	}

	/// <summary>
	/// Writes a capture to <paramref name="path"/>, or to the temp folder as <c>tqvault-capture-&lt;name&gt;-&lt;time&gt;.png</c>,
	/// and returns its full path. Every character a file name can't hold becomes '-': a ':' would otherwise write the picture
	/// into a hidden NTFS stream of a file named before it.
	/// </summary>
	public static string SaveCapture(string name, byte[] png, string? path = null)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			var invalid = Path.GetInvalidFileNameChars();
			var safe = new string(name.Select(c => c == '.' || Array.IndexOf(invalid, c) >= 0 ? '-' : c).ToArray());
			path = Path.Combine(Path.GetTempPath(), $"tqvault-capture-{safe}-{DateTime.Now:yyyyMMdd-HHmmss-fff}.png");
		}
		else
			path = Path.GetFullPath(path);

		File.WriteAllBytes(path, png);
		return path;
	}

	/// <summary>
	/// The content of a tool that returns a picture: its JSON when there is one, then the PNG when there is one. With
	/// <paramref name="saveAs"/>, the PNG is also saved (<see cref="SaveCapture"/>) and a last text block gives its size and
	/// file: "&lt;saveAs&gt;: &lt;width&gt;x&lt;height&gt; px, saved to &lt;file&gt;".
	/// </summary>
	public static List<ContentBlock> Result(string? json, byte[]? png, string? saveAs = null, string? path = null)
	{
		var blocks = new List<ContentBlock>();
		if (json is not null)
			blocks.Add(new TextContentBlock { Text = json });
		if (png is null)
			return blocks;

		blocks.Add(ImageContentBlock.FromBytes(png, "image/png"));
		if (saveAs is not null)
		{
			var file = SaveCapture(saveAs, png, path);
			var size = PngSize(png);
			blocks.Add(new TextContentBlock { Text = $"{saveAs}: {size.Width}x{size.Height} px, saved to {file}" });
		}

		return blocks;
	}

	/// <summary>The size of a PNG, read from its header (IHDR, the first chunk).</summary>
	private static Size PngSize(byte[] png)
		=> new(BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16, 4)), BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20, 4)));
}
