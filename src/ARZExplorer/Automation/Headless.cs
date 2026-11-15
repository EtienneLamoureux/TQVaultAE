using System.Runtime.InteropServices;
using TQVaultAE.Automation;

namespace ArzExplorer.Automation;

/// <summary>
/// ARZExplorer's part of the Debug-only <c>--headless</c> mode (<see cref="HeadlessDesktop"/>): the name of its desktop
/// and of the variable that hands it to the started instance, and muted sounds. ARZExplorer shows its message boxes with
/// <c>MessageBox.Show</c>: on the hidden desktop nobody could answer them, so they are cancelled like the other standard
/// dialogs and recorded with their caption and text (<see cref="HeadlessDesktop.CancelledDialogs"/>, tool <c>dialogs</c>).
/// </summary>
internal static partial class Headless
{
	/// <summary>Environment variable that gives the started instance the name of its hidden desktop.</summary>
	private const string DesktopVariable = "ARZEXPLORER_HEADLESS_DESKTOP";

	/// <summary>Control id of the message text in a message box.</summary>
	private const int MessageBoxTextId = 0xFFFF;

	/// <summary>Gets a value indicating whether the sounds of the headless instance are muted.</summary>
	public static bool Muted { get; private set; }

	/// <inheritdoc cref="HeadlessDesktop.TryAttach"/>
	public static bool TryAttach()
	{
		if (!HeadlessDesktop.TryAttach(DesktopVariable, Describe))
			return false;

		// Sounds play through the normal code path, silently: the hidden desktop hides windows, not audio.
		Muted = waveOutSetVolume(IntPtr.Zero, 0) == 0; // MMSYSERR_NOERROR
		return true;
	}

	/// <inheritdoc cref="HeadlessDesktop.StartOnHiddenDesktop"/>
	public static string? StartOnHiddenDesktop() => HeadlessDesktop.StartOnHiddenDesktop(DesktopVariable, "ARZExplorer");

	/// <summary>A cancelled dialog as recorded: its caption, then the message of a message box.</summary>
	private static string Describe(IntPtr dialog)
	{
		var message = Win32Window.Text(Win32Window.DialogItem(dialog, MessageBoxTextId)).ReplaceLineEndings(" ");
		return $"{Win32Window.Text(dialog).ReplaceLineEndings(" ")}: {message}".TrimEnd(' ', ':');
	}

	/// <summary>Sets the volume of this process' wave output (SoundPlayer); 0 mutes it.</summary>
	[LibraryImport("winmm.dll")]
	private static partial uint waveOutSetVolume(IntPtr device, uint volume);
}
