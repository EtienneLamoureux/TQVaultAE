using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Dialogs;

namespace TQVaultAE.GUI.Automation;

/// <summary>
/// Tool that dumps what the Item Properties dialog shows.
/// </summary>
internal static partial class AutomationTools
{
	[McpServerTool(Name = "item_properties_dump", ReadOnly = true)]
	[Description("Opens the Item Properties dialog (the item's context menu > Properties) on every owned item (search database, sorted by location as tooltip_text_dump), or on the item of one panel cell, and returns what it shows with the 'filter extra info' box unchecked and checked: the item name and its color, then each section (base, prefix, suffix) with its label, whether it is visible and its rows (text, color, selection color as #AARRGGBB). The dialog is shown transparent and closed at the end; nothing is changed. With 'path', the full dump is written to that JSON file and the result is a summary with its SHA-256.")]
	public static string ItemPropertiesDump(
		[Description("JSON file the full dump is written to; empty to return the dump in the result (fine for one item).")] string? path = null,
		[Description("Only the item on this panel cell (with x and y), e.g. vault; empty for every owned item.")] string? panel = null,
		[Description("Cell column, with panel.")] int x = 0,
		[Description("Cell row, with panel.")] int y = 0,
		[Description("Owned items to skip, in the sorted order (to dump in batches).")] int skip = 0,
		[Description("Owned items to dump after skip; 0 for all.")] int take = 0)
		=> AutomationHost.OnUi(form =>
		{
			var (targets, total, skipped) = DumpTargets(form, panel, x, y, skip, take);

			using var dialog = form.Services.Forms.Create<ItemProperties>();
			dialog.Opacity = 0;
			dialog.ShowInTaskbar = false;
			dialog.StartPosition = FormStartPosition.Manual;
			dialog.Location = new Point(-32000, -32000);
			// Load runs LoadProperties on Item: give it the first item, AutomationLoad sets each one.
			if (targets.Count != 0)
				dialog.Item = targets[0].Item;

			List<object> items;
			try
			{
				if (targets.Count != 0)
					dialog.Show(form);

				items = targets.Select(t => (object)new
				{
					location = t.Location,
					baseName = RecordId.RawOf(t.Item.BaseItemId),
					prefixName = RecordId.RawOf(t.Item.prefixID),
					suffixName = RecordId.RawOf(t.Item.suffixID),
					relicName = RecordId.RawOf(t.Item.relicID),
					relicName2 = RecordId.RawOf(t.Item.relic2ID),
					seed = t.Item.Seed,
					all = dialog.AutomationLoad(t.Item, false),
					filtered = dialog.AutomationLoad(t.Item, true),
				}).ToList();
			}
			finally
			{
				dialog.Close();
			}

			var dump = new
			{
				total,
				skip = skipped,
				count = items.Count,
				items,
			};

			if (string.IsNullOrWhiteSpace(path))
				return Serialize(dump);

			var file = WriteDump(path, dump);
			return Serialize(new
			{
				path = file.Path,
				total,
				dump.skip,
				count = items.Count,
				bytes = file.Bytes,
				sha256 = file.Sha256,
			});
		});
}
