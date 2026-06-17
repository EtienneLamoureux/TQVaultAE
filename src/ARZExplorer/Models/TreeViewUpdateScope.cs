namespace ArzExplorer.Models;

public class TreeViewUpdateScope : IDisposable
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
		if (_form.dicoNodes.TryGetValue(string.Empty, out var rootNode) && rootNode.Nodes.Count > 0)
		{
			this._form.TreeViewToc.Nodes.Clear();
			var nodes = rootNode.Nodes.Cast<TreeNode>()
				//.OrderBy(x => x.Text)
				.ToArray();
			this._form.TreeViewToc.Nodes.AddRange(nodes);
		}

		_form.TreeViewToc.EndUpdate();
		// Reset the cursor to the default for all controls.
		Cursor.Current = Cursors.Default;
	}
}