namespace ArzExplorer.Models;

public sealed class TreeViewUpdateScope : IDisposable
{
	private readonly MainForm _form;

	public TreeViewUpdateScope(MainForm form)
	{
		_form = form;
		// Display a wait cursor while the TreeNodes are being created.
		Cursor.Current = Cursors.WaitCursor;
		_form.TreeViewToc.BeginUpdate();
	}

	public void Dispose()
	{
		// Only add the top nodes not shown yet: nodes added under a shown node are already in the tree. Clearing and
		// re-adding every top node rebuilt the whole native tree (one message per node), which froze the window for
		// minutes once the database was loaded.
		if (_form.dicoNodes.TryGetValue(string.Empty, out var rootNode) && rootNode.Nodes.Count > 0)
		{
			var missing = rootNode.Nodes.Cast<TreeNode>()
				.Where(node => node.TreeView is null)
				.ToArray();
			if (missing.Length > 0)
				this._form.TreeViewToc.Nodes.AddRange(missing);
		}

		_form.TreeViewToc.EndUpdate();
		// Reset the cursor to the default for all controls.
		Cursor.Current = Cursors.Default;
	}
}