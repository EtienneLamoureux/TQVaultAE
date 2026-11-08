namespace TQVaultAE.GUI.Tooltips;

/// <summary>
/// Debug-only access to the cards for the automation endpoint.
/// </summary>
internal sealed partial class RichToolTip
{
	/// <summary>The card shown for <paramref name="control"/>, or <see langword="null"/>.</summary>
	internal Card? AutomationCard(Control control)
		=> this._cards.GetValueOrDefault(control);

	/// <summary>Draws <paramref name="card"/> as the tool tip does, on its background and border.</summary>
	internal Bitmap AutomationRender(Card card)
	{
		var size = this.Layout(card, null).Size;
		var bitmap = new Bitmap(size.Width, size.Height);
		using (var g = Graphics.FromImage(bitmap))
		{
			using (var back = new SolidBrush(Background))
				g.FillRectangle(back, new Rectangle(Point.Empty, size));
			using (var border = new Pen(Border))
				g.DrawRectangle(border, new Rectangle(0, 0, size.Width - 1, size.Height - 1));

			this.Layout(card, g);
		}

		return bitmap;
	}
}
