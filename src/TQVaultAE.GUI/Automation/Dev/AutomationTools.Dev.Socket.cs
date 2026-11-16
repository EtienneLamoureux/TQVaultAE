using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Forge;

namespace TQVaultAE.GUI.Automation;

internal static partial class AutomationTools
{
	internal static partial class DevTools
	{
		[McpServerTool(Name = "socket_click", Destructive = true)]
		[Description("Clicks the item that receives the relic or charm of the Insert into item selection, like the user. Start the selection with context_menu_click(panel, x, y, 'Insert into item > Game rules') (or '> No rules') on a relic or charm, item editing on. Left on another item: refused (the reason in 'notification'), socketed right away into the only free slot that takes it, or the slot menu shows ('slotMenu': the entries, 'Slot N' or 'Slot N: replace <name>'); slot=1 or 2 then clicks that entry, no slot closes the menu without socketing. Left on the relic itself ends the selection; right cancels it. Returns the selection before and after, the relic slots of the clicked item before and after (relic, bonus, shards per slot), whether the relic is still in its sack, and the notification.")]
		public static string SocketClick(
			[Description("Panel id of the item that receives the relic.")] string panel,
			[Description("Cell column.")] int x,
			[Description("Cell row.")] int y,
			[Description("Slot entry to click in the slot menu, 1 or 2; empty closes the menu.")] int? slot = null,
			[Description("'left' or 'right'.")] string button = "left")
			=> AutomationHost.OnUi(main =>
			{
				if (slot is not (null or 1 or 2))
					throw new McpException("slot must be 1, 2 or empty.");

				var named = ResolveShownPanel(main, panel);
				var selection = main.AutomationDragInfo.PairSelection;
				var relic = selection.First;
				var relicSack = selection.FirstSack;
				var before = DescribeSelection(selection);
				var target = named.Panel.AutomationFindItem(new Point(x, y));
				var slotsBefore = Slots(target);

				named.Panel.AutomationMouseDown(named.Panel.AutomationCellCenter(new Point(x, y)), ParseButton(button));

				var menu = named.Panel.AutomationSocketSlotMenu();
				string? clicked = null;
				if (menu.Count > 0)
				{
					if (slot is { } number)
					{
						var wanted = number == 1 ? ForgeSlot.Relic1 : ForgeSlot.Relic2;
						if (!named.Panel.AutomationClickSocketSlot(wanted))
						{
							named.Panel.AutomationCloseContextMenu();
							throw new McpException($"The slot menu has no entry for slot {number}. Entries: {string.Join(" | ", menu.Select(m => m.Text))}");
						}

						clicked = menu.First(m => m.Slot == wanted).Text;
					}
					else
						named.Panel.AutomationCloseContextMenu();
				}

				return Serialize(new
				{
					panel = named.Id,
					clicked = target is null ? null : DescribeItem(target, null),
					before,
					after = DescribeSelection(selection),
					slotMenu = menu.Count == 0 ? null : menu.Select(m => m.Text),
					slotClicked = clicked,
					slots = target is null ? null : new { before = slotsBefore, after = Slots(target) },
					relicInItsSack = relic is null || relicSack is null ? (bool?)null : relicSack.Any(i => ReferenceEquals(i, relic)),
					targetModified = target?.IsModified,
					notification = main.AutomationNotificationText,
				});
			});

		private static object DescribeSelection(Domain.Actions.ItemPairSelection selection) => new
		{
			active = selection.IsActive,
			purpose = selection.IsActive ? selection.Purpose.ToString() : null,
			first = selection.First is { } f ? DescribeItem(f, null) : null,
		};

		/// <summary>What the two relic slots of <paramref name="item"/> hold.</summary>
		private static object? Slots(Item? item) => item is null ? null : new
		{
			slot1 = item.HasSocketedSlot1 ? new { relic = item.relicID.Raw, bonus = RecordId.RawOf(item.RelicBonusId), shards = item.Var1 } : null,
			slot2 = item.HasSocketedSlot2 ? new { relic = item.relic2ID.Raw, bonus = RecordId.RawOf(item.RelicBonus2Id), shards = item.Var2 } : null,
		};
	}
}
