using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TQVaultAE.GUI.Components
{
	public partial class ComboBoxCharacterDropDown : UserControl
	{
		private ComboBoxCharacter _ComboBox;
		internal ComboBoxCharacter ComboBox
		{
			get => _ComboBox;
			set
			{
				_ComboBox = value;
				_ComboBox.Resize += ComboBox_Resize;
			}
		}

		private void ComboBox_Resize(object sender, EventArgs e)
		{
			this.Width = ComboBox.Width;
			ResizeAllItems();
		}

		private void ResizeAllItems()
		{
			foreach (var ctr in Items)
				ctr.Width = this.Width - SystemInformation.VerticalScrollBarWidth;
		}

		internal readonly BindingList<ComboBoxCharacterItem> Items = new();

		public ComboBoxCharacterDropDown()
		{
			InitializeComponent();

			Items.ListChanged += Items_ListChanged;
		}

		private void Items_ListChanged(object sender, ListChangedEventArgs e)
		{
			switch (e.ListChangedType)
			{
				case ListChangedType.ItemAdded:
					var itm = Items[e.NewIndex];
					itm.MinimumSize = new Size(this.Width - SystemInformation.VerticalScrollBarWidth, 0);
					this.bufferedFlowLayoutPanelVertical.Controls.Add(itm);
					break;
			}
		}
	}
}
