namespace TQ.SaveFilesExplorer.Components;

internal sealed class ComboBoxItem
{
	public required string DisplayName;
	public required string Value;

	public override string ToString()
	{
		// Generates the text shown in the combo box
		return DisplayName;
	}
}
