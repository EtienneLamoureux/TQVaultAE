namespace TQVaultAE.GUI.Infrastructure;

/// <summary>What lies behind the caption of a group box drawn by <see cref="CaravanFrame.ApplyWithCaption"/>.</summary>
internal enum CaptionBackground
{
	/// <summary>A black rectangle over the frame's top edge.</summary>
	Black,

	/// <summary>
	/// Nothing: the frame is drawn whole and the caption straight over its top edge, with a dark outline around the text
	/// to keep it readable.
	/// </summary>
	None,
}
