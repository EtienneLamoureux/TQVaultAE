namespace TQVaultAE.GUI.Components.Scaling;

/// <summary>
/// A control whose <c>ScaleControl</c> grows its font by the zoom factor (<see cref="ZoomScope.ScaleFont"/>): a zoom
/// gives it back its font of the <see cref="ZoomSnapshot"/> before scaling, so the font is grown once from it.
/// </summary>
internal interface IZoomedFont
{
	/// <summary>Gets or sets the font the zoom grows: by default the control's font.</summary>
	Font ZoomedFont
	{
		get => ((Control)this).Font;
		set => ((Control)this).Font = value;
	}
}

/// <summary>
/// A control that sizes and places the controls it holds itself, from the scale: the <see cref="ZoomSnapshot"/> keeps
/// the control, none of the controls it holds.
/// </summary>
internal interface ILaysOutChildren
{
}

/// <summary>
/// A control that fits a part of itself to its content or to the room it is given, in code: the
/// <see cref="ZoomSnapshot"/> and each zoom start from its whole layout, and it fits again once the zoom is done.
/// </summary>
internal interface IZoomFit
{
	/// <summary>
	/// Gives back what the fit took, before the layout is kept or given back. By default nothing: a control that only
	/// places itself or sets a size whole at each fit has nothing to give back.
	/// </summary>
	void UndoFit()
	{
	}

	/// <summary>Fits again, once the window is scaled and laid out.</summary>
	void Fit();
}
