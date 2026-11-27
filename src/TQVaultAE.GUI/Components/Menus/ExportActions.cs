using System.Globalization;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Presentation;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Components.Menus;

/// <summary>
/// What an export tells the user, beyond the log: <paramref name="Success"/> once done (for a file, a format given the
/// file), <paramref name="Failure"/> when it failed.
/// </summary>
internal sealed record ExportNotices(IUIService UI, string Success, string Failure);

/// <summary>
/// The three ways an export menu (<see cref="MenuStyle.ExportSubMenu"/>) sends a JSON export: the clipboard, a file chosen
/// by the user, PasteBin. A failure is logged, and shown for PasteBin or when <see cref="ExportNotices"/> are given.
/// </summary>
internal static class ExportActions
{
	/// <summary>Copies the export to the clipboard; nothing when <paramref name="serialize"/> gives none.</summary>
	/// <param name="what">What is exported, for the log.</param>
	public static void ToClipboard(Func<string?> serialize, ILogger log, string what, ExportNotices? notices = null)
	{
		try
		{
			if (serialize() is not { } json)
				return;

			Clipboard.SetText(json);
			notices?.UI.NotifyUser(notices.Success);
		}
		catch (Exception ex)
		{
			log.LogError(ex, "Failed to export {What} to clipboard", what);
			Fail(notices);
		}
	}

	/// <summary>Asks for a JSON file, <paramref name="fileName"/> by default, then writes the export there.</summary>
	public static void ToFile(string fileName, Action<string> write, ILogger log, string what, ExportNotices? notices = null)
	{
		try
		{
			using var dialog = new SaveFileDialog
			{
				Filter = Resources.GlobalJsonFileFilter,
				DefaultExt = "json",
				FileName = fileName,
			};

			if (dialog.ShowDialog() != DialogResult.OK)
				return;

			write(dialog.FileName);
			notices?.UI.NotifyUser(string.Format(CultureInfo.CurrentCulture, notices.Success, dialog.FileName));
		}
		catch (Exception ex)
		{
			log.LogError(ex, "Failed to export {What} to file", what);
			Fail(notices);
		}
	}

	/// <summary>
	/// Posts the export to PasteBin, copies the link to the clipboard and tells the user (<paramref name="successFormat"/>
	/// with the link), or shows why it failed (<paramref name="failureFormat"/> with the reason, the generic one by default).
	/// </summary>
	public static async Task ToPasteBinAsync(Func<Task<string>> post, IUIService uiService, string successFormat, ILogger log, string what
		, string? failureFormat = null)
	{
		try
		{
			var url = await post();
			Clipboard.SetText(url);
			uiService.NotifyUser(string.Format(CultureInfo.CurrentCulture, successFormat, url));
		}
		catch (Exception ex)
		{
			log.LogError(ex, "Failed to export {What} to PasteBin", what);
			uiService.ShowError(string.Format(CultureInfo.CurrentCulture, failureFormat ?? Resources.ExportFailPasteBin, ex.Message), Buttons: ShowMessageButtons.OK);
		}
	}

	private static void Fail(ExportNotices? notices)
		=> notices?.UI.ShowError(notices.Failure, Buttons: ShowMessageButtons.OK);
}
