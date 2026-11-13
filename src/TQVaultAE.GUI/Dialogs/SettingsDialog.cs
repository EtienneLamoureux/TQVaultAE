//-----------------------------------------------------------------------
// <copyright file="SettingsDialog.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using TQVaultAE.Application.Results;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.GUI.Views;

namespace TQVaultAE.GUI.Dialogs;

/// <summary>
/// The window of the settings: it hosts the <see cref="SettingsUserControl"/> and closes on its OK or Cancel.
/// </summary>
internal sealed class SettingsDialog : VaultForm
{
	/// <summary>
	/// The settings shown.
	/// </summary>
	private readonly SettingsUserControl settings;

	/// <summary>
	/// Save path for the vault files
	/// </summary>
	public string VaultPath => this.settings.VaultPath;

	/// <summary>
	/// Indicates whether the vault path has been changed
	/// </summary>
	public bool VaultPathChanged => this.settings.VaultPathChanged;

	/// <summary>
	/// Indicates that any configuration item has been changed
	/// </summary>
	public bool ConfigurationChanged => this.settings.ConfigurationChanged;

	/// <summary>
	/// Indicates that the ItemBGColorOpacity setting has been changed
	/// </summary>
	public bool ItemBGColorOpacityChanged => this.settings.ItemBGColorOpacityChanged;

	/// <summary>
	/// Indicates that the <see cref="Config.UserSettings.EnableItemRequirementRestriction"/> setting has been changed
	/// </summary>
	public bool EnableItemRequirementRestrictionChanged => this.settings.EnableItemRequirementRestrictionChanged;

	/// <summary>
	/// Indicates that the allow character edit setting has been changed.
	/// </summary>
	public bool EnableCharacterEditChanged => this.settings.EnableCharacterEditChanged;

	/// <summary>
	/// Indicates that the <see cref="Config.UserSettings.EnableTQVaultSounds"/> setting has been changed
	/// </summary>
	public bool EnableTQVaultSoundsChanged => this.settings.EnableTQVaultSoundsChanged;

	/// <summary>
	/// Why the changes need a restart, if they do.
	/// </summary>
	public RestartReason RestartReason => this.settings.RestartReason;

	/// <summary>
	/// Initializes a new instance of the SettingsDialog class.
	/// </summary>
	public SettingsDialog(MainForm instance) : base(instance.Services)
	{
		this.Owner = instance;

		this.SuspendLayout();

		this.settings = new SettingsUserControl(instance.Services)
		{
			Location = new Point(10, 25),
		};
		this.settings.Applied += (_, _) => this.DialogResult = DialogResult.OK;
		this.settings.Cancelled += (_, _) => this.DialogResult = DialogResult.Cancel;

		this.AutoScaleDimensions = new SizeF(96F, 96F);
		this.AutoScaleMode = AutoScaleMode.Dpi;
		this.BackColor = Color.FromArgb(46, 31, 21);
		this.CancelButton = this.settings.CancelButton;
		this.ClientSize = new Size(1222, 666);
		this.Controls.Add(this.settings);
		this.Controls.SetChildIndex(this.settings, 0);
		this.DrawCustomBorder = true;
		this.ForeColor = Color.White;
		this.FormBorderStyle = FormBorderStyle.None;
		this.MaximizeBox = false;
		this.MinimizeBox = false;
		this.Name = nameof(SettingsDialog);
		this.Padding = new Padding(5, 5, 10, 5);
		this.ShowIcon = false;
		this.ShowInTaskbar = false;
		this.SizeGripStyle = SizeGripStyle.Hide;
		this.StartPosition = FormStartPosition.CenterParent;
		this.Text = this.settings.Text;
		this.TopMost = true;

		// As in the other dialogs: the zoom reset of this button belongs to the main window, here it reset the
		// zoom of the whole application without resizing the main window.
		this.NormalizeBox = false;

		// The window's marble under the settings, as when they were its own controls: the corners of its border, which
		// reach over the settings, show through them.
		MarbleBackground.ApplyTo(this);
		this.settings.BackgroundImage = null;
		this.settings.BackColor = Color.Transparent;

		this.ResumeLayout(false);
		this.PerformLayout();

		// The dialog follows the settings: the font, a translation, a group shown or a monitor of another DPI change them.
		this.FitAround(this.settings);
	}

	/// <summary>
	/// Each opening starts from the settings in use, the first one as the next ones: the main window keeps the dialog
	/// between openings, and Cancel or the close button only hide it, so their edits are dropped here; the settings
	/// changed elsewhere since the last opening show too. The focus left on a button by the last closing is cleared, so
	/// that the dialog picks its first control again, as a new one does.
	/// </summary>
	protected override void SetVisibleCore(bool value)
	{
		if (value && !this.Visible)
		{
			this.settings.ShowSettingsInUse();
			this.ActiveControl = null;
		}

		base.SetVisibleCore(value);
	}
}
