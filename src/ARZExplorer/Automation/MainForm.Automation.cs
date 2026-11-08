using System.Diagnostics;
using ArzExplorer.Models;
using TQVaultAE.Domain.Entities;

namespace ArzExplorer;

/// <summary>
/// Debug-only access to the main form for the automation endpoint. Called on the UI thread.
/// </summary>
public partial class MainForm
{
	/// <summary>What the window shows.</summary>
	internal object AutomationState() => new
	{
		title = this.Text,
		files = this.TQFileOpened.Keys.ToArray(),
		nodeCount = this.dicoNodes.Count,
		topNodes = this.treeViewTOC.Nodes.Cast<TreeNode>().Select(n => $"{n.Text} ({n.Nodes.Count})").ToArray(),
		selectedNode = this.treeViewTOC.SelectedNode?.Name,
		path = this.textBoxPath.Text,
		status = this.toolStripStatusLabel.Text,
		shown = new
		{
			picture = this.panelPicture.Visible,
			grid = this.dataGridViewDetails.Visible,
			text = this.textBoxDetails.Visible,
			sound = this.simpleSoundPlayer.Visible,
		},
		picture = this.pictureBoxItem.Image is { } image
			? new { image.Width, image.Height, pixelFormat = image.PixelFormat.ToString(), box = this.pictureBoxItem.Size.ToString() }
			: null,
	};

	/// <summary>Opens an ARC or ARZ file as the Open menu does.</summary>
	internal void AutomationOpenFile(string path)
	{
		using var scope = new TreeViewUpdateScope(this);
		this.OpenFile(path);
	}

	/// <summary>The keys of the leaf nodes whose key contains <paramref name="contains"/>.</summary>
	internal string[] AutomationFindNodes(string contains, int max)
		=> [.. this.dicoNodes
			.Where(kv => kv.Value.Nodes.Count == 0 && kv.Key.Contains(contains, StringComparison.OrdinalIgnoreCase))
			.Select(kv => kv.Key)
			.Take(max)];

	/// <summary>Selects a node as a click in the tree does.</summary>
	internal void AutomationSelectNode(string key)
	{
		if (!this.dicoNodes.TryGetValue(key, out var node))
			throw new ArgumentException($"No node '{key}'.", nameof(key));

		this.treeViewTOC.SelectedNode = node;
	}

	/// <summary>Every menu and toolbar item, by name.</summary>
	private IEnumerable<ToolStripItem> AutomationItems()
	{
		IEnumerable<ToolStripItem> Walk(ToolStripItemCollection items)
		{
			foreach (ToolStripItem item in items)
			{
				yield return item;
				if (item is ToolStripDropDownItem dropDown)
					foreach (var child in Walk(dropDown.DropDownItems))
						yield return child;
			}
		}

		return this.Controls.OfType<ToolStrip>().SelectMany(strip => Walk(strip.Items));
	}

	/// <summary>The menu and toolbar items: name, text, enabled.</summary>
	internal object[] AutomationListItems()
		=> [.. this.AutomationItems().Where(i => !string.IsNullOrEmpty(i.Name)).Select(i => (object)new { i.Name, i.Text, i.Enabled })];

	/// <summary>Clicks a menu or toolbar item by name.</summary>
	internal void AutomationClickItem(string name)
	{
		var item = this.AutomationItems().FirstOrDefault(i => i.Name == name)
			?? throw new ArgumentException($"No menu or toolbar item '{name}'.", nameof(name));
		item.PerformClick();
	}

	/// <summary>The link cells of the record grid: row, column, value.</summary>
	internal object[] AutomationGridLinks()
		=> [.. this.dataGridViewDetails.Rows.Cast<DataGridViewRow>()
			.SelectMany(row => row.Cells.Cast<DataGridViewCell>())
			.Where(cell => cell.Style == PopulateGridView_hyperLinkCellStyle)
			.Select(cell => (object)new { row = cell.RowIndex, column = cell.ColumnIndex, value = cell.Value?.ToString() })];

	/// <summary>Clicks a cell of the record grid as the mouse does (a link cell navigates to its target).</summary>
	internal void AutomationClickGridCell(int row, int column)
		=> this.dataGridViewDetails_CellClick(this.dataGridViewDetails, new DataGridViewCellEventArgs(column, row));

	/// <summary>The archive and entry id of an ARC node, to read it off the UI thread.</summary>
	internal (ArcFile Arc, RecordId Id) AutomationArcEntry(string key)
	{
		if (!this.dicoNodes.TryGetValue(key, out var node) || node.Tag is not NodeTag tag)
			throw new ArgumentException($"No node '{key}'.", nameof(key));
		if (tag.File.FileType != CompressedFileType.ArcFile)
			throw new ArgumentException($"'{key}' is not in an ARC file.", nameof(key));

		return (tag.File.Arc, ArcPaths.DataId(tag.Key));
	}

	/// <summary>Reads and decodes a TEX entry, timing each step; called off the UI thread.</summary>
	internal object AutomationDecodeTex(ArcFile arc, RecordId id)
	{
		var watch = Stopwatch.StartNew();
		var data = this.arcProv.GetData(arc, id);
		var readMs = watch.ElapsedMilliseconds;
		if (data is null)
			return new { found = false, readMs };

		watch.Restart();
		using var bitmap = this.BitmapService.LoadFromTexMemory(data, 0, data.Length);
		return new
		{
			found = true,
			bytes = data.Length,
			readMs,
			decodeMs = watch.ElapsedMilliseconds,
			width = bitmap?.Width,
			height = bitmap?.Height,
			pixelFormat = bitmap?.PixelFormat.ToString(),
		};
	}
}
