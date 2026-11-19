using TQVaultAE.Presentation.Win32;

namespace TQVaultAE.GUI.Components.Rendering;

/// <summary>
/// Pictures of <see cref="Images"/> read once and shared by every control: each read of <see cref="Images"/> decodes a
/// new bitmap. Never dispose them; a control that changes a picture (flips it, scales it) draws its own copy.
/// </summary>
internal static class UIPictures
{
	/// <summary>The default button pictures of <see cref="Scaling.ScalingButton"/>.</summary>
	public static Bitmap MainButtonUp => field ??= Images.MainButtonUp;

	/// <inheritdoc cref="MainButtonUp"/>
	public static Bitmap MainButtonOver => field ??= Images.MainButtonOver;

	/// <inheritdoc cref="MainButtonUp"/>
	public static Bitmap MainButtonDown => field ??= Images.MainButtonDown;

	/// <summary>The medallion an item grid shows when it has no sack.</summary>
	public static Bitmap Medallion => field ??= Images.tqmedallion;

	/// <summary>The accent drawn over an item, tinted.</summary>
	public static Bitmap ItemAccent => field ??= Images.ItemAccent;

	/// <summary>The default vault bag button pictures.</summary>
	public static Bitmap BagUp => field ??= Images.inventorybagup01;

	/// <inheritdoc cref="BagUp"/>
	public static Bitmap BagDown => field ??= Images.inventorybagdown01;

	/// <inheritdoc cref="BagUp"/>
	public static Bitmap BagOver => field ??= Images.inventorybagover01;

	/// <summary>The stash tab button pictures.</summary>
	public static Bitmap StashTabUp => field ??= Images.StashTabUp;

	/// <inheritdoc cref="StashTabUp"/>
	public static Bitmap StashTabDown => field ??= Images.StashTabDown;

	/// <inheritdoc cref="StashTabUp"/>
	public static Bitmap StashTabOver => field ??= Images.StashTabOver;

	/// <summary>The window border pieces of <see cref="Forms.VaultForm"/>.</summary>
	public static Bitmap BorderTop => field ??= Images.BorderTop;

	/// <inheritdoc cref="BorderTop"/>
	public static Bitmap BorderBottom => field ??= Images.BorderBottom;

	/// <inheritdoc cref="BorderTop"/>
	public static Bitmap BorderSide => field ??= Images.BorderSide;

	/// <inheritdoc cref="BorderTop"/>
	public static Bitmap BorderBottomLeftCorner => field ??= Images.BorderBottomLeftCorner;

	/// <inheritdoc cref="BorderTop"/>
	public static Bitmap BorderBottomRightCorner => field ??= Images.BorderBottomRightCorner;

	/// <summary>The sort buttons' pictures: off, on and over; rotated for a vault, upright otherwise, mirrored or not.</summary>
	public static (Bitmap Off, Bitmap On, Bitmap Over) AutoSort(bool vault, bool mirrored)
	{
		int index = (vault ? 2 : 0) + (mirrored ? 1 : 0);
		return autoSort[index] ??= Make();

		(Bitmap, Bitmap, Bitmap) Make()
		{
			var set = vault
				? (Images.rotatedautosortup01, Images.rotatedautosortdown01, Images.rotatedautosortover01)
				: (Images.autosortup01, Images.autosortdown01, Images.autosortover01);
			if (mirrored)
			{
				set.Item1.RotateFlip(RotateFlipType.RotateNoneFlipX);
				set.Item2.RotateFlip(RotateFlipType.RotateNoneFlipX);
				set.Item3.RotateFlip(RotateFlipType.RotateNoneFlipX);
			}

			return set;
		}
	}

	private static readonly (Bitmap, Bitmap, Bitmap)?[] autoSort = new (Bitmap, Bitmap, Bitmap)?[4];
}
