namespace TQVaultAE.GUI.Models
{
	internal class ComboBoxItem
	{
		public string DisplayName;
		public string Value;

		public override string ToString()
		{
			// Generates the text shown in the combo box
			return DisplayName;
		}
	}
}
