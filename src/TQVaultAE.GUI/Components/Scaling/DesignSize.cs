namespace TQVaultAE.GUI.Components.Scaling;

/// <summary>
/// The size of a control in design pixels (at scale 1), kept from each size it is given outside a zoom, so that a zoom
/// sizes it from the scale rather than from its size at the zoom before: the rounding of each step does not pile up,
/// and a zoom back gives back its size.
/// </summary>
internal struct DesignSize
{
	private SizeF size;

	/// <summary>Gets a value indicating whether no size was kept yet.</summary>
	public readonly bool IsEmpty => this.size.IsEmpty;

	/// <summary>Keeps <paramref name="actual"/>, made at <paramref name="scale"/>, unless a zoom is being applied.</summary>
	public void Keep(Size actual, float scale)
	{
		if (!ZoomScope.IsZooming)
			this.size = new SizeF(actual.Width / scale, actual.Height / scale);
	}

	/// <summary>The kept size at <paramref name="scale"/>.</summary>
	public readonly Size At(float scale)
		=> new((int)Math.Round(this.size.Width * scale), (int)Math.Round(this.size.Height * scale));
}
