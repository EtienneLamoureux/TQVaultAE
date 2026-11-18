using TQVaultAE.Presentation.Win32;

namespace TQVaultAE.GUI.Components.Buttons;

/// <summary>
/// The default bag button pictures, decoded once: each read of an <see cref="Images"/> property decodes a new bitmap.
/// Shared by every bag button and the bag icon picker, never disposed.
/// </summary>
internal static class DefaultBagPictures
{
	public static Bitmap On { get; } = Images.inventorybagup01;

	public static Bitmap Off { get; } = Images.inventorybagdown01;

	public static Bitmap Over { get; } = Images.inventorybagover01;
}
