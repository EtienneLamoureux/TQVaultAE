using System.Collections.ObjectModel;
using TQVaultAE.Presentation.Win32.Models;

namespace TQVaultAE.Presentation.Win32.Contracts;

public interface IIconService
{
	/// <summary>
	/// Icons from the game files, loaded once on a background thread.
	/// </summary>
	/// <remarks>The first call starts the load; it is called after startup so the icons are ready when the bag button settings open.</remarks>
	Task<ReadOnlyCollection<IconInfo>> GetIconDatabaseAsync();
}