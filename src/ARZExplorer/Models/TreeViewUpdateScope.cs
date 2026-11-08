namespace ArzExplorer.Models;

/// <summary>
/// Suspends the tree drawing and shows the wait cursor while files are opened; on dispose, adds the new top nodes.
/// </summary>
internal sealed class TreeViewUpdateScope : IDisposable
{
	private readonly MainForm _form;

	public TreeViewUpdateScope(MainForm form)
	{
		this._form = form;
		// Display a wait cursor while the TreeNodes are being created.
		Cursor.Current = Cursors.WaitCursor;
		this._form.TreeViewToc.BeginUpdate();
	}

	public void Dispose()
	{
		// Only add the top nodes not shown yet: nodes added under a shown node are already in the tree. Clearing and
		// re-adding every top node rebuilt the whole native tree (one message per node), which froze the window for
		// minutes once the database was loaded.
		if (this._form.NodesByKey.TryGetValue(string.Empty, out var rootNode) && rootNode.Nodes.Count > 0)
		{
			var missing = rootNode.Nodes.Cast<TreeNode>()
				.Where(node => node.TreeView is null)
				.ToArray();
			if (missing.Length > 0)
				this._form.TreeViewToc.Nodes.AddRange(missing);
		}

		this._form.TreeViewToc.EndUpdate();
		// Reset the cursor to the default for all controls.
		Cursor.Current = Cursors.Default;
	}
}
