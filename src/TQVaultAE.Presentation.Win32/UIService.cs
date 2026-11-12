using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.Presentation.Win32;

public class UIService : IUIService, IGameDataCache
{
	private readonly ILogger Log;
	private readonly IDatabase Database;
	private readonly IBitmapService BitmapService;
	private readonly UserSettings USettings;

	/// <summary>
	/// Item unit size in pixels for a 1x1 item.
	/// </summary>
	private const float ITEMUNITSIZE = 32.0F;

	/// <summary>
	/// Dictionary of all bitmaps in the database.
	/// </summary>
	/// <remarks>Per data scope: a mod may override a picture of the base game under the same id.</remarks>
	private LazyConcurrentDictionary<(string Scope, RecordId Id), Bitmap> _Bitmaps = new();

	private (string Scope, RecordId Id) BitmapKey(RecordId resourceId) => (this.Database.DataScope ?? string.Empty, resourceId);

	/// <summary>
	/// Forgets the bitmaps read so far, without disposing them (controls may still show them): the next ones come from
	/// the active map's archives.
	/// </summary>
	public void ClearGameDataCache() => this._Bitmaps = new();

	/// <summary>
	/// Default bitmap when one cannot be found in the database.
	/// </summary>
	private Bitmap? _DefaultBitmap;

	/// <summary>
	/// Gives an item linked to a mod its mod's database; null in tests that do not need it.
	/// </summary>
	private readonly IItemModService? ItemMods;

	/// <summary>
	/// Pixels per design pixel: <see cref="Zoom"/> times <see cref="DpiScale"/>.
	/// </summary>
	private float _Scale = 1.00F;

	private float _DpiScale = 1.00F;

	/// <summary>
	/// Gets the default bitmap
	/// </summary>
	public Bitmap DefaultBitmap
	{
		get
		{
			if (this._DefaultBitmap is null)
				this.CreateDefaultBitmap();

			return this._DefaultBitmap;
		}
	}

	/// <summary>
	/// Gets the UI design DPI which is used to for scaling comparisons.
	/// </summary>
	/// <remarks>Use 96 DPI which is "normal" for Windows.</remarks>
	public float DESIGNDPI => 96.0F;

	/// <summary>
	/// Gets the item unit size which is the unit of measure of item size in TQ.
	/// An item with a ItemUnitSize x ItemUnitSize bitmap would be 1x1.
	/// Internally scaled by db scale.
	/// </summary>
	public int ItemUnitSize => Convert.ToInt32(ITEMUNITSIZE * this.Scale);

	/// <summary>
	/// Gets the half of an item unit size which is the unit of measure of item size in TQ.
	/// Division takes place after internal scaling by db scale.
	/// </summary>
	public int HalfUnitSize => this.ItemUnitSize / 2;

	/// <summary>Smallest <see cref="Zoom"/>: good enough for a 640 x 480 screen.</summary>
	public const float MinZoom = 0.4F;

	/// <summary>Largest <see cref="Zoom"/>.</summary>
	public const float MaxZoom = 2.0F;

	/// <summary>
	/// Gets or sets the pixels per design pixel (the UI is designed at 96 DPI): what the item cells, panels and pixel
	/// fonts are multiplied by. It is the user's <see cref="Zoom"/> times <see cref="DpiScale"/>, and is kept within
	/// <see cref="MinZoom"/> and <see cref="MaxZoom"/> zoom.
	/// </summary>
	public float Scale
	{
		get => _Scale;

		set => _Scale = Math.Clamp(value, MinZoom * this._DpiScale, MaxZoom * this._DpiScale);
	}

	/// <summary>
	/// Gets or sets the screen DPI over <see cref="DESIGNDPI"/>: 1 for a DPI-unaware process, 1.25 at 120 DPI.
	/// Windows already applies it to fonts sized in points, so those follow <see cref="Zoom"/> only.
	/// </summary>
	public float DpiScale
	{
		get => _DpiScale;

		set => _DpiScale = value > 0F ? value : 1.0F;
	}

	/// <summary>
	/// Gets the user's zoom, set by resizing the main window: <see cref="Scale"/> without the DPI. It is what the
	/// settings keep, and what fonts sized in points and dialogs are scaled by.
	/// </summary>
	public float Zoom => this._Scale / this._DpiScale;

	private float _SystemDpiScale;

	/// <inheritdoc/>
	public float SystemDpiScale
	{
		get
		{
			if (this._SystemDpiScale == 0F)
			{
				// The screen device context gives the system DPI, also in a per monitor DPI aware process.
				using var g = Graphics.FromHwnd(IntPtr.Zero);
				this._SystemDpiScale = g.DpiX > 0F ? g.DpiX / DESIGNDPI : 1.0F;
			}

			return this._SystemDpiScale;
		}
	}

	/// <inheritdoc/>
	public float FontZoom => this._Scale / this.SystemDpiScale;

	public UIService(ILogger<UIService> log, IDatabase database, IBitmapService bitmapService, UserSettings uSettings, IItemModService? itemMods = null)
	{
		this.ItemMods = itemMods;
		this.Log = log;
		this.Database = database;
		this.BitmapService = bitmapService;
		USettings = uSettings;

		// Code here won't run in Visual Studio designer
		if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
		{
			// Until the main window reads its own DPI and the zoom, the windows built before it are at the system DPI
			// with no zoom: fonts made then (FontZoom) must not shrink by the system DPI.
			this._DpiScale = this.SystemDpiScale;
			this._Scale = this._DpiScale;

			this.LoadRelicOverlayBitmap();
		}
	}

	/// <summary>
	/// Pictures of items whose own picture cannot be shown, by size in cells: the warning icon of the incomplete item
	/// tooltip line on a dark tile with an orange frame. At design size (32 pixels a cell), like the game's pictures, since
	/// the panels scale them. Shared: never disposed.
	/// </summary>
	private static readonly ConcurrentDictionary<(int Width, int Height), Lazy<Bitmap>> MissingItemBitmaps = new();

	/// <summary>
	/// Gets the picture shown for an item of <paramref name="width"/> x <paramref name="height"/> cells whose own picture
	/// cannot be shown (shared, never dispose it).
	/// </summary>
	public static Bitmap MissingItemBitmap(int width, int height)
		=> MissingItemBitmaps.GetOrAdd((Math.Max(1, width), Math.Max(1, height)), size => new(() => CreateMissingItemBitmap(size.Width, size.Height))).Value;

	private static Bitmap CreateMissingItemBitmap(int widthCells, int heightCells)
	{
		const int cell = (int)ITEMUNITSIZE;
		int width = widthCells * cell, height = heightCells * cell;
		var bmp = new Bitmap(width, height);
		using var g = Graphics.FromImage(bmp);
		g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
		g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

		var tile = new Rectangle(1, 1, width - 3, height - 3);
		using (var fill = new SolidBrush(Color.FromArgb(160, 40, 24, 0)))
			g.FillRectangle(fill, tile);
		using (var frame = new Pen(Color.Orange, 1F))
			g.DrawRectangle(frame, tile);

		using var icon = SystemIcons.GetStockIcon(StockIconId.Warning, StockIconOptions.Default);
		using var iconBitmap = icon.ToBitmap();
		const int iconSize = 20;
		g.DrawImage(iconBitmap, new Rectangle((width - iconSize) / 2, (height - iconSize) / 2, iconSize, iconSize));
		return bmp;
	}

	/// <summary>
	/// Pictures already reported as missing or unreadable, so that the warning is written once, not at every paint.
	/// </summary>
	private readonly ConcurrentDictionary<(string Scope, RecordId Id), byte> ReportedPictures = new();

	/// <summary>
	/// Creates a default bitmap for use when a bitmap cannot be found.
	/// </summary>
	[MemberNotNull(nameof(_DefaultBitmap))]
	private void CreateDefaultBitmap()
	{
		if (USettings.DatabaseDebugLevel > 0)
			Log.LogDebug("Database.CreateDefaultBitmap()");

		// Make a bitmap with a small orange square with a ? mark in it.
		Bitmap tempDefaultBitmap = new Bitmap(this.ItemUnitSize, this.ItemUnitSize);
		Graphics graphics = Graphics.FromImage(tempDefaultBitmap);

		// First fill the whole thing black so we can set our transparency
		SolidBrush black = new SolidBrush(Color.Black);
		graphics.FillRectangle(black, 0, 0, this.ItemUnitSize, this.ItemUnitSize);

		// Now draw the orange square
		SolidBrush orange = new SolidBrush(Color.Orange);
		float border = 5.0F * this.Scale; // amount of area to leave black
		graphics.FillRectangle(orange, Convert.ToInt32(border), Convert.ToInt32(border), this.ItemUnitSize - Convert.ToInt32(2.0F * border), this.ItemUnitSize - Convert.ToInt32(2.0F * border));

		// Now put the Question Mark at the center
		// use a color that is slightly off-black so it does not become transparent
		SolidBrush textBrush = new SolidBrush(Color.FromArgb(1, 1, 1));
		Font textFont = new Font("Arial", (float)this.ItemUnitSize - Convert.ToInt32(4.0F * border), GraphicsUnit.Pixel);
		StringFormat textFormat = new StringFormat();
		textFormat.Alignment = StringAlignment.Center;

		graphics.DrawString("?", textFont, textBrush, new RectangleF(border, border, (float)this.ItemUnitSize - (2.0F * border), (float)this.ItemUnitSize - (2.0F * border)), textFormat);

		// now set our transparency
		tempDefaultBitmap.MakeTransparent(Color.Black);

		// all done
		this._DefaultBitmap = tempDefaultBitmap;

		if (USettings.DatabaseDebugLevel > 0)
			Log.LogDebug("Exiting Database.CreateDefaultBitmap()");
	}


	/// <summary>
	/// Loads a bitmap from a resource Id string
	/// </summary>
	/// <param name="resourceId">Resource Id which we are looking up.</param>
	/// <returns>Bitmap fetched from the database</returns>
	public Bitmap LoadBitmap(RecordId? resourceId)
	{
		if (RecordId.IsNullOrEmpty(resourceId))
			return this.DefaultBitmap;

		if (USettings.DatabaseDebugLevel > 0)
			Log.LogDebug("Database.LoadBitmap({ResourceId})", resourceId);

		Bitmap bitmap = _Bitmaps.GetOrAddAtomic(this.BitmapKey(resourceId), k =>
		{
			// Load the resource
			byte[]? texData = Database.LoadResource(k.Id);
			return AddBitmap(k.Id, texData);
		});

		if (USettings.DatabaseDebugLevel > 0)
			Log.LogDebug("Exiting Database.LoadBitmap()");

		return bitmap;
	}

	/// <summary>
	/// Loads a bitmap from <paramref name="texData"/> with an identifier <paramref name="resourceId"/>
	/// </summary>
	/// <param name="resourceId">Resource Id which we are looking up.</param>
	/// <param name="texData">raw DDS image data</param>
	/// <returns>Bitmap converted from <paramref name="texData"/></returns>
	public Bitmap? LoadBitmap(RecordId resourceId, byte[]? texData)
	{
		if (RecordId.IsNullOrEmpty(resourceId) || texData is not { Length: > 0 })
			return null;

		if (USettings.DatabaseDebugLevel > 0)
			Log.LogDebug("Database.LoadBitmap({ResourceId})", resourceId);

		Bitmap bitmap = _Bitmaps.GetOrAddAtomic(this.BitmapKey(resourceId), k =>
		{
			return AddBitmap(k.Id, texData);
		});

		if (USettings.DatabaseDebugLevel > 0)
			Log.LogDebug("Exiting Database.LoadBitmap()");

		return bitmap;
	}

	/// <summary>
	/// Create and Add the bitmap to <see cref="_Bitmaps"/>.
	/// </summary>
	/// <param name="resourceId"></param>
	/// <param name="texData">image content from resource game file</param>
	/// <returns></returns>
	private Bitmap AddBitmap(RecordId resourceId, byte[]? texData)
	{
		if (RecordId.IsNullOrEmpty(resourceId))
			return this.DefaultBitmap;

		Bitmap bitmap;
		if (texData is null)
		{
			if (USettings.DatabaseDebugLevel > 0)
				Log.LogDebug("Failure loading resource.  Using default bitmap");

			// could not load the data.  Use a default bitmap
			bitmap = this.DefaultBitmap;
		}
		else
		{
			if (USettings.DatabaseDebugLevel > 1)
				Log.LogDebug("Loaded resource size={TexDataLength}", texData.Length);

			// Create the bitmap. Data that is cut or damaged throws: it gives the default bitmap, cached like a good one, so
			// that it is decoded once and not again at every paint.
			Bitmap? loaded;
			try
			{
				loaded = BitmapService.LoadFromTexMemory(texData, 0, texData.Length);
			}
			catch (Exception ex) when (ex is InvalidDataException or InvalidOperationException or ArgumentException)
			{
				Log.LogWarning(ex, @"The picture ""{ResourceId}"" could not be decoded.", resourceId.Raw);
				loaded = null;
			}

			if (loaded is not null)
				bitmap = loaded;
			else
			{
				if (USettings.DatabaseDebugLevel > 0)
					Log.LogDebug("Failure creating bitmap from resource data len={TexDataLength}", texData.Length);

				// could not create the bitmap
				bitmap = this.DefaultBitmap;
			}

			if (USettings.DatabaseDebugLevel > 1)
				Log.LogDebug("Created Bitmap {BitmapWidth} x {BitmapHeight}", bitmap.Width, bitmap.Height);
		}

		return bitmap;
	}

	static readonly RecordId LoadRelicOverlayBitmapId = "Items\\Relic\\ItemRelicOverlay.tex";
	/// <summary>
	/// Loads the relic overlay bitmap from the database.
	/// </summary>
	/// <returns>Relic overlay bitmap</returns>
	public Bitmap? LoadRelicOverlayBitmap()
	{
		Bitmap? relicOverlayBitmap = this.LoadBitmap(LoadRelicOverlayBitmapId);

		// do not return the defaultbitmap
		if (relicOverlayBitmap == this.DefaultBitmap)
			return null;

		return relicOverlayBitmap;
	}

	/// <summary>
	/// Gets the item's bitmap
	/// </summary>
	public Bitmap? GetBitmap(Item itm)
	{
		// The picture of an item linked to a mod comes from that mod's files.
		using var modScope = this.ItemMods?.Scope(itm) ?? NoScope.Instance;
		Bitmap? bmp = null;
		if (!RecordId.IsNullOrEmpty(itm.TexImageResourceId))
		{
			// Both the raw resource and the decoded bitmap are cached: only the first call per resource reads and decodes.
			bmp = LoadBitmap(itm.TexImageResourceId, this.Database.LoadResource(itm.TexImageResourceId));

			// Data that could not be decoded gives the default picture: that is no picture of the item.
			if (bmp is not null && ReferenceEquals(bmp, this._DefaultBitmap))
				bmp = null;
		}

		if (bmp is not null)
		{
			if (USettings.ItemDebugLevel > 1)
			{
				Log.LogDebug("size = {BmpWidth}x{BmpHeight} (unitsize={ItemUnitSize})"
					, bmp.Width
					, bmp.Height
					, this.ItemUnitSize
				);
			}

			itm.Width = Convert.ToInt32((float)bmp.Width * this.Scale / (float)this.ItemUnitSize);
			itm.Height = Convert.ToInt32((float)bmp.Height * this.Scale / (float)this.ItemUnitSize);
		}
		else
		{
			if (USettings.ItemDebugLevel > 1)
				Log.LogDebug("bitmap is null");

			// No picture (its record is missing, as for a mod not loaded, or its picture is missing from the archives or
			// unreadable): the item keeps the size read at load (from the texture header when it could be read, else one
			// cell), so that room checks and drops see its real place, and shows a warning tile of that size, so that it can
			// be found, moved and its tooltip read.
			itm.Width = Math.Max(1, itm.Width);
			itm.Height = Math.Max(1, itm.Height);

			if (!RecordId.IsNullOrEmpty(itm.BaseItemId))
			{
				bmp = MissingItemBitmap(itm.Width, itm.Height);

				// A missing record is reported with the item's other missing records; a found record without its picture
				// is told here, once per picture.
				if (itm.baseItemInfo is not null)
					this.ReportMissingPicture(itm);
			}
		}
		return bmp;
	}

	/// <summary>
	/// Writes a warning, once per picture, for an item whose record was found but whose picture could not be loaded.
	/// </summary>
	private void ReportMissingPicture(Item itm)
	{
		RecordId picture = RecordId.IsNullOrEmpty(itm.TexImageResourceId) ? itm.BaseItemId : itm.TexImageResourceId;
		if (this.ReportedPictures.TryAdd(this.BitmapKey(picture), 0))
		{
			this.Log.LogWarning(@"The picture ""{Picture}"" of item record ""{Record}"" could not be loaded: a placeholder of {Width}x{Height} cells is shown."
				, itm.TexImageResourceId?.Raw ?? string.Empty, itm.BaseItemId.Raw, itm.Width, itm.Height);
		}
	}

	#region Notifications

	public void NotifyUser(string message, TQColor color = TQColor.Turquoise)
	{
		NotifyUserEvent?.Invoke(this, message, color.Color());
	}

	public void NotifyUser(string message, Color color)
	{
		NotifyUserEvent?.Invoke(this, message, color);
	}

	public ShowMessageUserEventHandlerEventArgs ShowError(string message, Exception? exception = null, ShowMessageButtons buttons = ShowMessageButtons.OKCancel)
	{
		ShowMessageUserEventHandlerEventArgs mess = new() { Error = exception, Message = message, Level = LogLevel.Error, Buttons = buttons };
		ShowMessageUserEvent?.Invoke(this, mess);
		return mess;
	}

	public ShowMessageUserEventHandlerEventArgs ShowWarning(string message, Exception? exception = null, ShowMessageButtons buttons = ShowMessageButtons.OKCancel)
	{
		ShowMessageUserEventHandlerEventArgs mess = new() { Error = exception, Message = message, Level = LogLevel.Warning, Buttons = buttons };
		ShowMessageUserEvent?.Invoke(this, mess);
		return mess;
	}
	public ShowMessageUserEventHandlerEventArgs ShowInfo(string message, ShowMessageButtons buttons = ShowMessageButtons.OK)
	{
		ShowMessageUserEventHandlerEventArgs mess = new() { Message = message, Level = LogLevel.Information, Buttons = buttons };
		ShowMessageUserEvent?.Invoke(this, mess);
		return mess;
	}

	public event NotifyUserEventHandler? NotifyUserEvent;

	public event ShowMessageUserEventHandler? ShowMessageUserEvent;

	#endregion

}