using Microsoft.Extensions.Logging;
using System;
using System.Drawing;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;

namespace TQVaultAE.Domain.Contracts.Services;

public interface IUIService
{
	/// <summary>
	/// Gets the default bitmap
	/// </summary>
	Bitmap DefaultBitmap { get; }

	/// <summary>
	/// Gets the half of an item unit size which is the unit of measure of item size in TQ.
	/// Division takes place after internal scaling by db scale.
	/// </summary>
	int HalfUnitSize { get; }

	/// <summary>
	/// Gets the UI design DPI which is used to for scaling comparisons.
	/// </summary>
	/// <remarks>Use 96 DPI which is "normal" for Windows.</remarks>
	float DESIGNDPI { get; }

	/// <summary>
	/// Gets the item unit size which is the unit of measure of item size in TQ.
	/// An item with a ItemUnitSize x ItemUnitSize bitmap would be 1x1.
	/// Internally scaled by db scale.
	/// </summary>
	int ItemUnitSize { get; }

	/// <summary>
	/// Gets or sets the scaling of the UI
	/// </summary>
	float Scale { get; set; }

	/// <summary>
	/// Gets the item's bitmap
	/// </summary>
	Bitmap GetBitmap(Item itm);

	/// <summary>
	/// Loads a bitmap from a resource Id string
	/// </summary>
	/// <param name="resourceId">Resource Id which we are looking up.</param>
	/// <returns>Bitmap fetched from the database</returns>
	Bitmap LoadBitmap(RecordId resourceId);

	/// <summary>
	/// Loads a bitmap from <paramref name="texData"/> with an identifier <paramref name="resourceId"/>
	/// </summary>
	/// <param name="resourceId">Resource Id which we are looking up.</param>
	/// <param name="texData">raw DDS image data</param>
	/// <returns>Bitmap converted from <paramref name="texData"/></returns>
	Bitmap LoadBitmap(RecordId resourceId, byte[] texData);

	/// <summary>
	/// Loads the relic overlay bitmap from the database.
	/// </summary>
	/// <returns>Relic overlay bitmap</returns>
	Bitmap LoadRelicOverlayBitmap();

	/// <summary>
	/// Display notification to user
	/// </summary>
	/// <param name="message"></param>
	/// <param name="color"></param>
	void NotifyUser(string message, TQColor color = TQColor.Turquoise);

	/// <summary>
	/// Display notification to user
	/// </summary>
	/// <param name="message"></param>
	/// <param name="color"></param>
	void NotifyUser(string message, Color color);

	/// <summary>
	/// Notification event
	/// </summary>
	event NotifyUserEventHandler NotifyUserEvent;

	/// <summary>
	/// Notification event
	/// </summary>
	event ShowMessageUserEventHandler ShowMessageUserEvent;

	/// <summary>
	/// Display error to user
	/// </summary>
	/// <param name="exception"></param>
	/// <param name="message"></param>
	ShowMessageUserEventHandlerEventArgs ShowError(string message, Exception exception = null, ShowMessageButtons Buttons = ShowMessageButtons.OKCancel);
	/// <summary>
	/// Display warning to user
	/// </summary>
	/// <param name="exception"></param>
	/// <param name="message"></param>
	ShowMessageUserEventHandlerEventArgs ShowWarning(string message, Exception exception = null, ShowMessageButtons Buttons = ShowMessageButtons.OKCancel);
	/// <summary>
	/// Display error to user
	/// </summary>
	/// <param name="message"></param>
	ShowMessageUserEventHandlerEventArgs ShowInfo(string message, ShowMessageButtons Buttons = ShowMessageButtons.OK);
}

/// <summary>
/// Notification delegate
/// </summary>
/// <param name="sender"></param>
/// <param name="message"></param>
/// <param name="color"></param>
public delegate void NotifyUserEventHandler(object sender, string message, Color color);

/// <summary>
/// Show Message delegate
/// </summary>
/// <param name="sender"></param>
/// <param name="message"></param>
public delegate void ShowMessageUserEventHandler(object sender, ShowMessageUserEventHandlerEventArgs message);

/// <summary>
/// Show Message arguments
/// </summary>
public class ShowMessageUserEventHandlerEventArgs
{
	public LogLevel Level;
	public Exception Error;
	public string Message;
	public bool IsOK;
	public ShowMessageButtons Buttons;
}

public enum ShowMessageButtons
{
	/// <summary>
	/// The message box contains an OK button.
	/// </summary>
	OK,
	/// <summary>
	/// The message box contains OK and Cancel buttons.
	/// </summary>
	OKCancel,
}