using System.Text.Json;
using Moq;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Config;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

/// <summary>
/// Builds an <see cref="ItemExchangeService"/> with a mock for each dependency a test does not give.
/// </summary>
internal static class ItemExchangeServices
{
	/// <summary>
	/// The mods of a test that does not give its own: every mod is installed, so imports keep their links.
	/// </summary>
	public static IItemModService EveryModInstalled()
	{
		var mods = new Mock<IItemModService>();
		mods.Setup(m => m.Find(It.IsAny<string?>())).Returns<string?>(name => new InstalledMod(name ?? string.Empty, string.Empty, ModSource.Legacy));
		return mods.Object;
	}

	public static ItemExchangeService Create(
		JsonSerializerOptions jsonOptions
		, IPasteBinService? pasteBinService = null
		, UserSettings? userSettings = null
		, IItemProvider? itemProvider = null
		, IItemTextFormatter? itemText = null
		, IItemMovementService? itemMovementService = null
		, IFileIO? fileIO = null
		, IVaultService? vaultService = null
		, IItemModService? itemModService = null)
		=> new(
			jsonOptions
			, pasteBinService ?? Mock.Of<IPasteBinService>()
			, userSettings ?? new UserSettings()
			, itemProvider ?? Mock.Of<IItemProvider>()
			, itemText ?? Mock.Of<IItemTextFormatter>()
			, itemMovementService ?? Mock.Of<IItemMovementService>()
			, fileIO ?? Mock.Of<IFileIO>()
			, vaultService ?? Mock.Of<IVaultService>()
			, itemModService ?? EveryModInstalled());
}
