using System.Drawing.Imaging;

namespace ArzExplorer;

/// <summary>
/// Debug-only access to the About box, drag and drop and the sound player, for the automation endpoint.
/// Called on the UI thread.
/// </summary>
public partial class MainForm
{
	/// <summary>The sound player of the window.</summary>
	internal Components.SimpleSoundPlayer AutomationSoundPlayer => this.simpleSoundPlayer;

	/// <summary>
	/// Drops files on the window through its own DragEnter and DragDrop handlers, as Explorer does.
	/// </summary>
	/// <returns>The drop effect DragEnter chose.</returns>
	internal DragDropEffects AutomationDropFiles(string[] paths)
	{
		var data = new DataObject(DataFormats.FileDrop, paths);
		var enter = new DragEventArgs(data, 0, 0, 0, DragDropEffects.All, DragDropEffects.None);
		this.Form1_DragEnter(this, enter);
		if (enter.Effect != DragDropEffects.None)
			this.Form1_DragDrop(this, new DragEventArgs(data, 0, 0, 0, DragDropEffects.All, enter.Effect));
		return enter.Effect;
	}

	/// <summary>
	/// Opens the About box as the Help menu does, but not modally (a modal box would hold the UI thread), reads its
	/// texts and renders it, then closes it.
	/// </summary>
	internal (string[] Texts, byte[] Png) AutomationAbout()
	{
		using var box = new AboutBox();
		box.Show(this);
		try
		{
			var texts = All(box).Where(c => c is Label or TextBox or Button && !string.IsNullOrWhiteSpace(c.Text))
				.Select(c => $"{c.Name}: {c.Text.ReplaceLineEndings(" ")}")
				.Prepend($"title: {box.Text}")
				.ToArray();

			using var bitmap = new Bitmap(box.Width, box.Height);
			box.DrawToBitmap(bitmap, new Rectangle(Point.Empty, box.Size));
			using var stream = new MemoryStream();
			bitmap.Save(stream, ImageFormat.Png);
			return (texts, stream.ToArray());
		}
		finally
		{
			box.Close();
		}

		static IEnumerable<Control> All(Control parent)
		{
			foreach (Control child in parent.Controls)
			{
				yield return child;
				foreach (var nested in All(child))
					yield return nested;
			}
		}
	}
}
