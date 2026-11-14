﻿using System.ComponentModel;
using TQVaultAE.GUI.Infrastructure;

namespace TQVaultAE.GUI.Components.Character;

public partial class ComboBoxCharacterDropDown : UserControl
{
	// Set by the combo box that creates the drop down.
	private ComboBoxCharacter _ComboBox = null!;
	internal ComboBoxCharacter ComboBox
	{
		get => _ComboBox;
		set
		{
			_ComboBox = value;
			_ComboBox.Resize += ComboBox_Resize;
		}
	}

	private void ComboBox_Resize(object? sender, EventArgs e)
		=> this.FitWidth();

	/// <summary>The width of the combo box, its items as wide as the room left by the scroll bar.</summary>
	internal void FitWidth()
	{
		this.Width = ComboBox.Width;
		ResizeAllItems();
	}

	private void ResizeAllItems()
	{
		foreach (var ctr in Items)
			ctr.Width = this.Width - this.VerticalScrollBarWidth();
	}

	internal readonly BindingList<ComboBoxCharacterItem> Items = [];

	public ComboBoxCharacterDropDown()
	{
		InitializeComponent();

		Items.ListChanged += Items_ListChanged;
	}

	private void Items_ListChanged(object? sender, ListChangedEventArgs e)
	{
		switch (e.ListChangedType)
		{
			case ListChangedType.ItemAdded:
				var itm = Items[e.NewIndex];
				itm.MinimumSize = new Size(this.Width - this.VerticalScrollBarWidth(), 0);
				this.bufferedFlowLayoutPanelVertical.Controls.Add(itm);
				break;
		}
	}
}