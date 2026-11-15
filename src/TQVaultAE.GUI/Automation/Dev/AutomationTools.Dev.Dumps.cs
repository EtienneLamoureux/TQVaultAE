//-----------------------------------------------------------------------
// <copyright file="AutomationTools.Dumps.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Security.Cryptography;
using System.Text.Json;
using ModelContextProtocol;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Forms;

namespace TQVaultAE.GUI.Automation;

/// <summary>
/// What the dump tools (<c>tooltip_text_dump</c>, <c>item_properties_dump</c>) share: the items they read and the file
/// they write.
/// </summary>
internal static partial class AutomationTools
{
	/// <summary>
	/// The items a dump reads, with the place each one is reported at: the item on a panel cell when
	/// <paramref name="panel"/> is given, else the owned items of the search database sorted by place, record and seed,
	/// from <paramref name="skip"/> on and <paramref name="take"/> of them (0 for all).
	/// </summary>
	/// <returns>The items, the number of items to dump in all, and the skip applied.</returns>
	private static (List<(Item Item, object Location)> Targets, int Total, int Skip) DumpTargets(MainForm form, string? panel, int x, int y, int skip, int take)
	{
		if (!string.IsNullOrWhiteSpace(panel))
		{
			var named = ResolveShownPanel(form, panel);
			var item = named.Panel.AutomationFindItem(new Point(x, y))
				?? throw new McpException($"No item at ({x}, {y}) on panel '{named.Id}'.");
			return ([(item, new { panel = named.Id, x = item.PositionX, y = item.PositionY })], 1, 0);
		}

		var owned = form.Services.ItemDatabaseService.ItemDatabase
			.OrderBy(r => r.Container, StringComparer.Ordinal)
			.ThenBy(r => r.SackType)
			.ThenBy(r => r.SackNumber)
			.ThenBy(r => r.Item.PositionY)
			.ThenBy(r => r.Item.PositionX)
			.ThenBy(r => RecordId.RawOf(r.Item.BaseItemId), StringComparer.Ordinal)
			.ThenBy(r => r.Item.Seed)
			.ToList();
		var batch = owned.Skip(Math.Max(skip, 0));
		if (take > 0)
			batch = batch.Take(take);

		var targets = batch.Select(r => (r.Item, (object)new
		{
			container = r.ContainerName,
			sackType = r.SackType.ToString(),
			sack = r.SackNumber,
			x = r.Item.PositionX,
			y = r.Item.PositionY,
		})).ToList();
		return (targets, owned.Count, Math.Max(skip, 0));
	}

	/// <summary>Writes <paramref name="dump"/> as indented JSON to <paramref name="path"/>.</summary>
	/// <returns>The full path, the size in bytes and the SHA-256 of the file, to compare two dumps.</returns>
	private static (string Path, int Bytes, string Sha256) WriteDump(string path, object dump)
	{
		var json = JsonSerializer.SerializeToUtf8Bytes(dump, JsonOptions);
		var full = Path.GetFullPath(path);
		File.WriteAllBytes(full, json);
		return (full, json.Length, Convert.ToHexString(SHA256.HashData(json)));
	}
}
