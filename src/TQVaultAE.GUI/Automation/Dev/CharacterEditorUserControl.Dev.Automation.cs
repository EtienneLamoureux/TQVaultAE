
namespace TQVaultAE.GUI.Views;

/// <summary>Debug access to the character editor's points left.</summary>
internal sealed partial class CharacterEditorUserControl
{
	/// <summary>The attribute points left to spend in the editor.</summary>
	internal int AutomationAttributePointsLeft => Convert.ToInt32(this.attributeNumericUpDown.Value);

	/// <summary>The skill points left to spend in the editor.</summary>
	internal int AutomationSkillPointsLeft => this._skills?.AvailablePoints ?? 0;
}
