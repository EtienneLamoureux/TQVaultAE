using System.Globalization;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Presentation;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Components.Menus;

/// <summary>
/// The three ways an export menu (<see cref="MenuStyle.ExportSubMenu"/>) sends a JSON export: the clipboard, a file chosen
/// by the user, PasteBin. A failure is logged, and shown for PasteBin.
/// </summary>
internal static class ExportActions
{
	/// <summary>Copies the export to the clipboard; nothing when <paramref name="serialize"/> gives none.</summary>
	/// <param name="what">What is exported, for the log.</param>
	public static void ToClipboard(Func<string?> serialize, ILogger log, string what)
	{
		try
		{
			if (serialize() is { } json)
				Clipboard.SetText(json);
		}
		catch (Exception ex)
		{
			log.LogError(ex, "Failed to export {What} to clipboard", what);
		}
	}

	/// <summary>Asks for a JSON file, <paramref name="fileName"/> by default, then writes the export there.</summary>
	public static void ToFile(string fileName, Action<string> write, ILogger log, string what)
	{
		try
		{
			using var dialog = new SaveFileDialog
			{
				Filter = Resources.GlobalJsonFileFilter,
				DefaultExt = "json",
				FileName = fileName,
			};

			if (dialog.ShowDialog() == DialogResult.OK)
				write(dialog.FileName);
		}
		catch (Exception ex)
		{
			log.LogError(ex, "Failed to export {What} to file", what);
		}
	}

	/// <summary>
	/// Posts the export to PasteBin, copies the link to the clipboard and tells the user (<paramref name="successFormat"/>
	/// with the link), or shows why it failed.
	/// </summary>
	public static async Task ToPasteBinAsync(Func<Task<string>> post, IUIService uiService, string successFormat, ILogger log, string what)
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
			uiService.ShowError(string.Format(CultureInfo.CurrentCulture, Resources.ExportFailPasteBin, ex.Message), Buttons: ShowMessageButtons.OK);
		}
	}
}
