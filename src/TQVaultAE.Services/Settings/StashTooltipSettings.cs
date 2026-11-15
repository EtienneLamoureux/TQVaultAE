using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Services.Settings;

/// <summary>
/// Maps the stash panel bags to their "disable tooltip" user settings.
/// </summary>
public static class StashTooltipSettings
{
	/// <summary>
	/// Gets whether the bag tooltip of a stash panel bag is turned off.
	/// </summary>
	/// <param name="settings">The user settings.</param>
	/// <param name="bagId">The bag id (see <see cref="BagIdConstants"/>).</param>
	/// <returns>True when turned off; also true for an id that is not a stash panel bag, which has no tooltip.</returns>
	public static bool IsStashTooltipDisabled(this UserSettings settings, int bagId)
		=> bagId switch
		{
			BagIdConstants.BAGID_EQUIPMENTPANEL => settings.DisableTooltipEquipment,
			BagIdConstants.BAGID_TRANSFERSTASH => settings.DisableTooltipTransfer,
			BagIdConstants.BAGID_PLAYERSTASH => settings.DisableTooltipStash,
			BagIdConstants.BAGID_RELICVAULTSTASH => settings.DisableTooltipRelic,
			_ => true,
		};

	/// <summary>
	/// Turns the bag tooltip of a stash panel bag off or on. An id that is not a stash panel bag is ignored.
	/// </summary>
	/// <param name="settings">The user settings.</param>
	/// <param name="bagId">The bag id (see <see cref="BagIdConstants"/>).</param>
	/// <param name="disabled">True to turn the tooltip off.</param>
	public static void SetStashTooltipDisabled(this UserSettings settings, int bagId, bool disabled)
	{
		switch (bagId)
		{
			case BagIdConstants.BAGID_EQUIPMENTPANEL:
				settings.DisableTooltipEquipment = disabled;
				break;
			case BagIdConstants.BAGID_TRANSFERSTASH:
				settings.DisableTooltipTransfer = disabled;
				break;
			case BagIdConstants.BAGID_PLAYERSTASH:
				settings.DisableTooltipStash = disabled;
				break;
			case BagIdConstants.BAGID_RELICVAULTSTASH:
				settings.DisableTooltipRelic = disabled;
				break;
		}
	}
}
