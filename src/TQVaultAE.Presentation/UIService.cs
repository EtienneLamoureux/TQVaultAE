﻿using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using TQVaultAE.Config;
using TQVaultAE.Domain.Contracts.Providers;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Presentation
{
	public class UIService : IUIService
	{
		private readonly ILogger Log = null;
		private readonly IDatabase Database;
		private readonly ITQDataService TQData;
		private readonly IBitmapService BitmapService;

		/// <summary>
		/// Item unit size in pixels for a 1x1 item.
		/// </summary>
		private const float ITEMUNITSIZE = 32.0F;

		/// <summary>
		/// Dictionary of all bitmaps in the database.
		/// </summary>
		private LazyConcurrentDictionary<RecordId, Bitmap> _Bitmaps = new();

		/// <summary>
		/// Default bitmap when one cannot be found in the database.
		/// </summary>
		private Bitmap _DefaultBitmap;

		/// <summary>
		/// Scaling factor used to scale the UI for higher DPI values than the default of 96.
		/// </summary>
		private float _Scale = 1.00F;

		/// <summary>
		/// Gets the default bitmap
		/// </summary>
		public Bitmap DefaultBitmap
		{
			get
			{
				if (this._DefaultBitmap == null)
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

		/// <summary>
		/// Gets or sets the scaling of the UI
		/// </summary>
		public float Scale
		{
			get => _Scale;

			set
			{
				// Set some bounds for the scale factors.
				if (value < 0.4F)
				{
					// Should be good enough for a 640 x 480 screen.
					value = 0.4F;
				}
				else if (value > 2.0F)
				{
					// Large fonts are 1.50 so this should be good enough.
					value = 2.0F;
				}

				_Scale = value;
			}
		}

		public UIService(ILogger<UIService> log, IDatabase database, ITQDataService tQData, IBitmapService bitmapService)
		{
			if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
			{
				// Code here won't run in Visual Studio designer
				this.Log = log;
				this.Database = database;
				this.TQData = tQData;
				this.BitmapService = bitmapService;
				this.LoadRelicOverlayBitmap();
			}
		}

		/// <summary>
		/// Creates a default bitmap for use when a bitmap cannot be found.
		/// </summary>
		private void CreateDefaultBitmap()
		{
			if (TQDebug.DatabaseDebugLevel > 0)
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

			if (TQDebug.DatabaseDebugLevel > 0)
				Log.LogDebug("Exiting Database.CreateDefaultBitmap()");
		}


		/// <summary>
		/// Loads a bitmap from a resource Id string
		/// </summary>
		/// <param name="resourceId">Resource Id which we are looking up.</param>
		/// <returns>Bitmap fetched from the database</returns>
		public Bitmap LoadBitmap(RecordId resourceId)
		{
			if (RecordId.IsNullOrEmpty(resourceId))
				return null;

			if (TQDebug.DatabaseDebugLevel > 0)
				Log.LogDebug("Database.LoadBitmap({0})", resourceId);

			Bitmap bitmap = _Bitmaps.GetOrAddAtomic(resourceId, k =>
			{
				// Load the resource
				byte[] texData = Database.LoadResource(k);
				return AddBitmap(k, texData);
			});

			if (TQDebug.DatabaseDebugLevel > 0)
				Log.LogDebug("Exiting Database.LoadBitmap()");

			return bitmap;
		}

		/// <summary>
		/// Loads a bitmap from <paramref name="texData"/> with an identifier <paramref name="resourceId"/>
		/// </summary>
		/// <param name="resourceId">Resource Id which we are looking up.</param>
		/// <param name="texData">raw DDS image data</param>
		/// <returns>Bitmap converted from <paramref name="texData"/></returns>
		public Bitmap LoadBitmap(RecordId resourceId, byte[] texData)
		{
			if (RecordId.IsNullOrEmpty(resourceId) || !(texData?.Any() ?? false))
				return null;

			if (TQDebug.DatabaseDebugLevel > 0)
				Log.LogDebug("Database.LoadBitmap({0})", resourceId);

			Bitmap bitmap = _Bitmaps.GetOrAddAtomic(resourceId, k =>
			{
				return AddBitmap(k, texData);
			});

			if (TQDebug.DatabaseDebugLevel > 0)
				Log.LogDebug("Exiting Database.LoadBitmap()");

			return bitmap;
		}

		/// <summary>
		/// Create and Add the bitmap to <see cref="_Bitmaps"/>.
		/// </summary>
		/// <param name="resourceId"></param>
		/// <param name="texData">image content from resource game file</param>
		/// <returns></returns>
		private Bitmap AddBitmap(RecordId resourceId, byte[] texData)
		{
			if (RecordId.IsNullOrEmpty(resourceId))
				return null;

			Bitmap bitmap;
			if (texData == null)
			{
				if (TQDebug.DatabaseDebugLevel > 0)
					Log.LogDebug("Failure loading resource.  Using default bitmap");

				// could not load the data.  Use a default bitmap
				bitmap = this.DefaultBitmap;
			}
			else
			{
				if (TQDebug.DatabaseDebugLevel > 1)
					Log.LogDebug("Loaded resource size={0}", texData.Length);

				// Create the bitmap
				bitmap = BitmapService.LoadFromTexMemory(texData, 0, texData.Length);
				if (bitmap == null)
				{
					if (TQDebug.DatabaseDebugLevel > 0)
						Log.LogDebug("Failure creating bitmap from resource data len={0}", texData.Length);

					// could not create the bitmap
					bitmap = this.DefaultBitmap;
				}

				if (TQDebug.DatabaseDebugLevel > 1)
					Log.LogDebug("Created Bitmap {0} x {1}", bitmap.Width, bitmap.Height);
			}

			return bitmap;
		}

		static readonly RecordId LoadRelicOverlayBitmapId = "Items\\Relic\\ItemRelicOverlay.tex";
		/// <summary>
		/// Loads the relic overlay bitmap from the database.
		/// </summary>
		/// <returns>Relic overlay bitmap</returns>
		public Bitmap LoadRelicOverlayBitmap()
		{
			Bitmap relicOverlayBitmap = this.LoadBitmap(LoadRelicOverlayBitmapId);

			// do not return the defaultbitmap
			if (relicOverlayBitmap == this.DefaultBitmap)
				return null;

			return relicOverlayBitmap;
		}

		/// <summary>
		/// Gets the item's bitmap
		/// </summary>
		public Bitmap GetBitmap(Item itm)
		{
			Bitmap bmp = null;
			if (itm.TexImageResourceId != null && itm.TexImage != null)
			{
				bmp = LoadBitmap(itm.TexImageResourceId, itm.TexImage);
				if (TQDebug.ItemDebugLevel > 1)
				{
					Log.LogDebug("size = {0}x{1} (unitsize={2})"
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
				if (TQDebug.ItemDebugLevel > 1)
					Log.LogDebug("bitmap is null");

				itm.Width = 1;
				itm.Height = 1;
			}
			return bmp;
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

		public ShowMessageUserEventHandlerEventArgs ShowError(string message, Exception exception = null, ShowMessageButtons buttons = ShowMessageButtons.OKCancel)
		{
			ShowMessageUserEventHandlerEventArgs mess = new() { Error = exception, Message = message, Level = LogLevel.Error, Buttons = buttons };
			ShowMessageUserEvent?.Invoke(this, mess);
			return mess;
		}

		public ShowMessageUserEventHandlerEventArgs ShowWarning(string message, Exception exception = null, ShowMessageButtons buttons = ShowMessageButtons.OKCancel)
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

		public event NotifyUserEventHandler NotifyUserEvent;

		public event ShowMessageUserEventHandler ShowMessageUserEvent;

		#endregion

	}
}
