namespace TQVaultAE.GUI.Models;

/// <summary>An entry of a list or drop-down: a value and the text shown for it.</summary>
internal sealed record DisplayItem<T>(T Value, string Text)
{
	public override string ToString() => this.Text;
}
