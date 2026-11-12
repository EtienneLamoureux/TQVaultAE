using AwesomeAssertions;
using TQVaultAE.Config;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Services;

namespace TQVaultAE.Tests.Services;

public class StashTooltipSettingsTests
{
	[Theory]
	[InlineData(BagIdConstants.BAGID_EQUIPMENTPANEL)]
	[InlineData(BagIdConstants.BAGID_TRANSFERSTASH)]
	[InlineData(BagIdConstants.BAGID_PLAYERSTASH)]
	[InlineData(BagIdConstants.BAGID_RELICVAULTSTASH)]
	public void SetStashTooltipDisabled_ChangesOnlyTheFlagOfThatBag(int bagId)
	{
		var settings = new UserSettings
		{
			DisableTooltipEquipment = false,
			DisableTooltipTransfer = false,
			DisableTooltipStash = false,
			DisableTooltipRelic = false,
		};

		settings.SetStashTooltipDisabled(bagId, true);

		int[] bags = [BagIdConstants.BAGID_EQUIPMENTPANEL, BagIdConstants.BAGID_TRANSFERSTASH, BagIdConstants.BAGID_PLAYERSTASH, BagIdConstants.BAGID_RELICVAULTSTASH];
		foreach (int bag in bags)
			settings.IsStashTooltipDisabled(bag).Should().Be(bag == bagId, $"bag {bag} after disabling bag {bagId}");

		settings.SetStashTooltipDisabled(bagId, false);
		settings.IsStashTooltipDisabled(bagId).Should().BeFalse();
	}

	[Fact]
	public void IsStashTooltipDisabled_ReadsTheMatchingSetting()
	{
		var settings = new UserSettings
		{
			DisableTooltipEquipment = true,
			DisableTooltipTransfer = false,
			DisableTooltipStash = true,
			DisableTooltipRelic = false,
		};

		settings.IsStashTooltipDisabled(BagIdConstants.BAGID_EQUIPMENTPANEL).Should().BeTrue();
		settings.IsStashTooltipDisabled(BagIdConstants.BAGID_TRANSFERSTASH).Should().BeFalse();
		settings.IsStashTooltipDisabled(BagIdConstants.BAGID_PLAYERSTASH).Should().BeTrue();
		settings.IsStashTooltipDisabled(BagIdConstants.BAGID_RELICVAULTSTASH).Should().BeFalse();
	}

	[Fact]
	public void UnknownBagId_HasNoTooltipAndIsIgnoredBySet()
	{
		var settings = new UserSettings
		{
			DisableTooltipEquipment = false,
			DisableTooltipTransfer = false,
			DisableTooltipStash = false,
			DisableTooltipRelic = false,
		};

		settings.SetStashTooltipDisabled(9, true);

		settings.IsStashTooltipDisabled(9).Should().BeTrue();
		settings.DisableTooltipEquipment.Should().BeFalse();
		settings.DisableTooltipTransfer.Should().BeFalse();
		settings.DisableTooltipStash.Should().BeFalse();
		settings.DisableTooltipRelic.Should().BeFalse();
	}
}
