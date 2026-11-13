using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Components.Character;
using TQVaultAE.GUI.Components.Layout;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Views;

namespace TQVaultAE.GUI.Forms;

/// <summary>
/// The views of the main window: under its toolbar, it shows one view at a time, the main content
/// (<see cref="MainFormUserControl"/>) by default.
/// </summary>
public partial class MainForm
{
	/// <summary>
	/// The main content: vault, character and their panels. Given by the container, placed by the designer code.
	/// </summary>
	private readonly MainFormUserControl mainView;

	#region Controls of the main content

	// The window builds and drives the panels of the main content: these are the view's controls it works on.
	private System.Windows.Forms.Panel itemTextPanel => this.mainView.itemTextPanel;
	internal ScalingLabel NotificationText => this.mainView.NotificationText;
	private ScalingComboBox vaultListComboBox => this.mainView.vaultListComboBox;
	private BufferedFlowLayoutPanel flowLayoutPanelVaultSelector => this.mainView.flowLayoutPanelVaultSelector;
	private System.Windows.Forms.PictureBox vaultPictureBox => this.mainView.vaultPictureBox;
	internal BufferedTableLayoutPanel tableLayoutPanelMain => this.mainView.tableLayoutPanelMain;
	private System.Windows.Forms.TableLayoutPanel tableLayoutPanelHighlight => this.mainView.tableLayoutPanelHighlight;
	private System.Windows.Forms.LinkLabel scalingLabelHighlight => this.mainView.scalingLabelHighlight;
	private ScalingTextBox scalingTextBoxHighlight => this.mainView.scalingTextBoxHighlight;
	private BufferedFlowLayoutPanel flowLayoutPanelRightPanels => this.mainView.flowLayoutPanelRightPanels;
	private BufferedFlowLayoutPanel flowLayoutPanelRightComboBox => this.mainView.flowLayoutPanelRightComboBox;
	private ComboBoxCharacter comboBoxCharacter => this.mainView.comboBoxCharacter;
	private BufferedFlowLayoutPanel bufferedFlowLayoutPanelsecondaryVaultList => this.mainView.bufferedFlowLayoutPanelsecondaryVaultList;
	private System.Windows.Forms.PictureBox pictureBoxSecondVault => this.mainView.pictureBoxSecondVault;
	private ScalingComboBox secondaryVaultListComboBox => this.mainView.secondaryVaultListComboBox;
	private HighlightFilters highlightFilters => this.mainView.highlightFilters;

	/// <summary>
	/// The main content's lists drive the window's loading of vaults and characters.
	/// </summary>
	private void WireMainView()
	{
		this.vaultListComboBox.SelectedIndexChanged += this.VaultListComboBoxSelectedIndexChanged;
		this.comboBoxCharacter.SelectedIndexChanged += this.CharacterComboBoxSelectedIndexChanged;
		this.secondaryVaultListComboBox.SelectedIndexChanged += this.SecondaryVaultListComboBoxSelectedIndexChanged;
	}

	#endregion
}
