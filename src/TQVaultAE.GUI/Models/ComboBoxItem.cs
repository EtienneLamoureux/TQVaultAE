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

	internal class ComboBoxItem<TComboValue, TValue>
	{
		public string DisplayName;
		public TComboValue ComboValue;
		public TValue Value;

		public override string ToString()
		{
			// Generates the text shown in the combo box
			return DisplayName;
		}
	}
}
