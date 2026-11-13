using TQVaultAE.Application;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Views;
using TQVaultAE.Presentation.Win32;

namespace TQVaultAE.GUI.Dialogs;

/// <summary>
/// The window of the character editor: it hosts the <see cref="CharacterEditorUserControl"/>, on the stretched marble,
/// and closes on its OK or Cancel.
/// </summary>
internal sealed class CharacterEditDialog : VaultForm
{
	/// <summary>The editor shown.</summary>
	private readonly CharacterEditorUserControl editor;

	/// <summary>Character to edit, set before the dialog is shown.</summary>
	internal PlayerCollection PlayerCollection { get; set; } = null!;

	/// <summary>
	/// Initializes a new instance of the CharacterEditDialog class.
	/// </summary>
	public CharacterEditDialog(GuiServices services) : base(services)
	{
		this.SuspendLayout();

		this.editor = new CharacterEditorUserControl(services) { Name = "characterEditor" };
		this.editor.Applied += (_, _) => this.Close();
		this.editor.Cancelled += (_, _) => this.Close();

		this.AutoScaleDimensions = new SizeF(96F, 96F);
		this.AutoScaleMode = AutoScaleMode.Dpi;
		this.BackColor = Color.FromArgb(46, 31, 21);
		this.ClientSize = new Size(1010, 710);
		this.Controls.Add(this.editor);
		this.DrawCustomBorder = true;
		this.Font = this.FontService.GetFontLight(11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
		this.ForeColor = Color.White;
		this.FormBorderStyle = FormBorderStyle.None;
		this.Margin = new Padding(4);
		this.MaximizeBox = false;
		this.MinimizeBox = false;
		this.Name = nameof(CharacterEditDialog);
		this.ShowInTaskbar = false;
		this.SizeGripStyle = SizeGripStyle.Hide;
		this.StartPosition = FormStartPosition.CenterParent;
		this.Text = "Character Editor";
		this.TopMost = true;

		// As in the other dialogs: the zoom reset of this button belongs to the main window.
		this.NormalizeBox = false;

		this.ResumeLayout(false);
	}

	/// <summary>
	/// Builds the editor for the character, under the title, and fits the dialog around it, on the plain marble of
	/// TQVault's own background picture stretched to the dialog.
	/// </summary>
	protected override void OnLoad(EventArgs e)
	{
		var marble = Images.CharacterEditorMarble;
		this.BackgroundImage = marble;
		this.BackgroundImageLayout = ImageLayout.Stretch;
		this.FormClosed += (_, _) => marble.Dispose();

		this.editor.Location = new Point(0, this.LogicalToDeviceUnits(26) - this.LogicalToDeviceUnits(14));
		this.editor.ShowCharacter(this.PlayerCollection);
		this.editor.Size = this.editor.ContentSize;
		this.ClientSize = new Size(this.editor.Right, this.editor.Bottom);

		base.OnLoad(e);
	}
}
