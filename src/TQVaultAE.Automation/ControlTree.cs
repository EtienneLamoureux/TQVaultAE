namespace TQVaultAE.Automation;

/// <summary>
/// Walks the controls of a form or container for the automation tools, which find controls by name or type at any depth.
/// </summary>
public static class ControlTree
{
	/// <summary>Every control inside <paramref name="parent"/>, at any depth, each before its own children.</summary>
	public static IEnumerable<Control> AllControls(Control parent)
	{
		ArgumentNullException.ThrowIfNull(parent);
		return Walk(parent);

		static IEnumerable<Control> Walk(Control parent)
		{
			foreach (Control child in parent.Controls)
			{
				yield return child;
				foreach (var descendant in Walk(child))
					yield return descendant;
			}
		}
	}
}
