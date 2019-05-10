using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TQVaultAE.GUI
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
