//-----------------------------------------------------------------------
// <copyright file="MainForm.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Models;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation.Win32;
using TQVaultAE.Presentation;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;

namespace TQVaultAE.GUI.Forms;

/// <summary>
/// Main Dialog class
/// </summary>
public partial class MainForm : VaultForm
{
	private readonly ILogger Log;

	public ITagService TagService { get; }

	/// <summary>
	/// Application layer services
	/// </summary>
	private readonly IPlayerService playerService;
	private readonly IVaultService vaultService;
	private readonly IStashService stashService;
	private readonly IItemExchangeService itemExchangeService;
	private readonly ISettingsPersistenceService settingsPersistenceService;

	/// <summary>
	/// The command line, parsed once at startup.
	/// </summary>
	private readonly CommandLineArgs commandLineArgs;

	private readonly ILoadAllService loadAllService;

	private readonly ISaveOrchestrator saveOrchestrator;

	#region	Fields

	/// <summary>
	/// Indicates whether the database resources have completed loading.
	/// </summary>
	private bool resourcesLoaded;

	/// <summary>
	/// Indicates whether the entire load process has completed.
	/// </summary>
	private bool loadingComplete;

	/// <summary>
	/// Instance of the player panel control
	/// </summary>
	private PlayerPanel playerPanel = null!; // Created while the form loads.

	/// <summary>
	/// Holds the last sack that had mouse focus
	/// Used for Automoving
	/// </summary>
	private SackCollection? lastSackHighlighted;

	/// <summary>
	/// Holds the last sack panel that had mouse focus
	/// Used for Automoving
	/// /// </summary>
	private SackPanel? lastSackPanelHighlighted;

	/// <summary>
	/// Info for the current item being dragged by the mouse
	/// </summary>
	private ItemDragInfo DragInfo;

	/// <summary>
	/// Used for show/hide table panel layout borders during debug
	/// </summary>
	private bool DebugLayoutBorderVisible = false;

	/// <summary>
	/// Holds the coordinates of the last drag item
	/// </summary>
	private Point lastDragPoint;

	/// <summary>
	/// User current data context
	/// </summary>
	internal SessionContext userContext;

	/// <summary>
	/// Highlight search service
	/// </summary>
	private IHighlightService HighlightService;

	/// <summary>
	/// Instance of the vault panel control
	/// </summary>
	private VaultPanel vaultPanel = null!; // Created while the form loads.

	/// <summary>
	/// Instance of the second vault panel control
	/// This gets toggled with the player panel
	/// </summary>
	private VaultPanel secondaryVaultPanel = null!; // Created while the form loads.

	/// <summary>
	/// Instance of the stash panel control
	/// </summary>
	private StashPanel stashPanel = null!; // Created while the form loads.

	/// <summary>
	/// Holds that last stash that had focus
	/// </summary>
	private Stash? lastStash;

	/// <summary>
	/// Bag number of the last bag with focus
	/// </summary>
	private int lastBag;

	/// <summary>
	/// Holds the current program version
	/// </summary>
	private string currentVersion;

	/// <summary>
	/// Signals that the configuration UI was loaded and the user changed something in there.
	/// </summary>
	private bool configChanged;

	/// <summary>
	/// Flag which holds whether we are showing the secondary vault panel or the player panel.
	/// </summary>
	private bool showSecondaryVault;

	/// <summary>
	/// Form for the splash screen.
	/// </summary>
	private SplashScreenForm splashScreen = null!; // Created while the form loads.

	/// <summary>
	/// Holds the opacity interval for fading the form.
	/// </summary>
	private double fadeInterval;


	#endregion

#if DEBUG
	// For Design Mode
#pragma warning disable CS8618 // Designer only: the runtime constructor sets the services and state.
	public MainForm() => InitForm();
#pragma warning restore CS8618
#endif

	/// <summary>
	/// Initializes a new instance of the MainForm class.
	/// </summary>
	public MainForm(
			GuiServices services
			, ILogger<MainForm> log
			, SessionContext sessionContext
			, IPlayerService playerService
			, IVaultService vaultService
			, IStashService stashService
			, ITagService tagService
			, IItemExchangeService itemExchangeService
			, ISettingsPersistenceService settingsPersistenceService
			, CommandLineArgs commandLineArgs
			, ILoadAllService loadAllService
			, ISaveOrchestrator saveOrchestrator
		) : base(services)
	{
		this.userContext = sessionContext;
		this.HighlightService = services.HighlightService;
		this.playerService = playerService;
		this.vaultService = vaultService;
		this.stashService = stashService;
		this.TagService = tagService;
		this.itemExchangeService = itemExchangeService;
		this.settingsPersistenceService = settingsPersistenceService;
		this.commandLineArgs = commandLineArgs;
		this.loadAllService = loadAllService;
		this.saveOrchestrator = saveOrchestrator;

		Log = log;
		Log.LogInformation("TQVaultAE Initialization !");

		InitForm();

		#region Apply custom font & scaling

		this.exitButton.Font = FontService.GetFontLight(12F, UIService.FontZoom);
		ScaleControl(this.UIService, this.exitButton);

		ScaleControl(this.UIService, this.comboBoxCharacter, false);

		this.NotificationText.Font = FontService.GetFontLight(11F, FontStyle.Bold, UIService.FontZoom);

		this.vaultListComboBox.Font = FontService.GetFontLight(13F, UIService.FontZoom);
		ScaleControl(this.UIService, this.vaultListComboBox, false);
		this.secondaryVaultListComboBox.Font = FontService.GetFontLight(13F, UIService.FontZoom);
		ScaleControl(this.UIService, this.secondaryVaultListComboBox, false);
		this.configureButton.Font = FontService.GetFontLight(12F, UIService.FontZoom);
		ScaleControl(this.UIService, this.configureButton);
		this.customMapText.Font = FontService.GetFont(11.25F, UIService.FontZoom);
		ScaleControl(this.UIService, this.customMapText, false);
		this.showVaulButton.Font = FontService.GetFontLight(12F, UIService.FontZoom);
		ScaleControl(this.UIService, this.showVaulButton);
		this.aboutButton.Font = FontService.GetFontLight(8.25F, UIService.FontZoom);
		ScaleControl(this.UIService, this.aboutButton);
		this.titleLabel.Font = FontService.GetFontLight(24F, UIService.FontZoom);
		ScaleControl(this.UIService, this.titleLabel);
		this.searchButton.Font = FontService.GetFontLight(12F, UIService.FontZoom);
		ScaleControl(this.UIService, this.searchButton);
		ScaleControl(this.UIService, this.tableLayoutPanelMain);

		this.saveButton.Font = FontService.GetFontLight(12F, UIService.FontZoom);
		ScaleControl(this.UIService, this.saveButton);
		this.forgeButton.Font = FontService.GetFontLight(12F, UIService.FontZoom);
		ScaleControl(this.UIService, this.forgeButton);

		this.scalingLabelHighlight.Font = FontService.GetFontLight(10F, UIService.FontZoom);
		this.scalingTextBoxHighlight.Font = FontService.GetFontLight(10F, UIService.FontZoom);

		#endregion

		if (USettings.DebugEnabled)
		{
			// Write this version into the debug file.
			Log.LogDebug(
@"Current TQVault Version: {CurrentVersion}
Debug Enabled: {DebugEnabled}
LootTable Debug Enabled: {LootTableDebugEnabled}
ARCFile Debug Level: {ARCFileDebugLevel}
Database Debug Level: {DatabaseDebugLevel}
Item Attributes Debug Level: {ItemAttributesDebugLevel}
Item Debug Level: {ItemDebugLevel}
",
				this.currentVersion,
				USettings.DebugEnabled,
				USettings.LootTableDebugEnabled,
				USettings.ARCFileDebugLevel,
				USettings.DatabaseDebugLevel,
				USettings.ItemAttributesDebugLevel,
				USettings.ItemDebugLevel);
		}

		// Process the mouse scroll wheel to cycle through the vaults.
		this.MouseWheel += new MouseEventHandler(this.MainFormMouseWheel);
	}

	[MemberNotNull(nameof(currentVersion), nameof(DragInfo))]
	private void InitForm()
	{
		this.Enabled = false;
		this.ShowInTaskbar = false;
		this.Opacity = 0;
		this.Hide();

		this.InitializeComponent();

		this.vaultPictureBox.Image = Images.Majestic_Chest_small;
		this.vaultPictureBox.Size = Images.Majestic_Chest_small.Size;
		this.toolTip.SetToolTip(this.vaultPictureBox, Resources.MainFormLabel2);

		this.pictureBoxSecondVault.Image = Images.Majestic_Chest_small;
		this.pictureBoxSecondVault.Size = Images.Majestic_Chest_small.Size;
		this.toolTip.SetToolTip(this.pictureBoxSecondVault, Resources.MainForm2ndVault);

		this.comboBoxCharacter.Init(
			this.UIService
			, this.FontService
			, this.TranslationService
			, this.Services.CharacterEditService
			, this.TagService
			, this.playerService
			, USettings
			, () => DuplicateCharacter()
		);

		this.SetupFormSize();

		// Changed to a global for versions in tqdebug
		AssemblyName aname = Assembly.GetExecutingAssembly().GetName();
		this.currentVersion = aname.Version?.ToString() ?? string.Empty;
		this.Text = string.Format(CultureInfo.CurrentCulture, "{0} {1}", aname.Name, this.currentVersion);

		// Setup localized strings.
		this.configureButton.Text = Resources.MainFormBtnConfigure;
		this.exitButton.Text = Resources.GlobalExit;
		this.showVaulButton.Text = Resources.MainFormBtnPanelSelect;
		this.Icon = Images.TQVIcon;
		this.searchButton.Text = Resources.MainFormSearchButtonText;
		this.scalingLabelHighlight.Text = Resources.MainFormHighlightLabelText;

		this.lastDragPoint.X = -1;
		this.DragInfo = new ItemDragInfo(this.UIService);
#if DEBUG
		this.DebugLayoutBorderVisible = false;// Set here what you want during debug
#endif
		if (!this.DebugLayoutBorderVisible)
		{
			this.flowLayoutPanelRightComboBox.BorderStyle =
			this.flowLayoutPanelVaultSelector.BorderStyle =
			this.flowLayoutPanelRightPanels.BorderStyle =
			this.bufferedFlowLayoutPanelsecondaryVaultList.BorderStyle =
			this.pictureBoxSecondVault.BorderStyle =
			this.vaultPictureBox.BorderStyle =
			this.comboBoxCharacter.BorderStyle = BorderStyle.None;
			this.tableLayoutPanelMain.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
		}

		AdjustMenuButtonVisibility();

		// Wired first: the git sync below can ask questions.
		this.UIService.NotifyUserEvent += UIService_NotifyUserEvent;
		this.UIService.ShowMessageUserEvent += UIService_ShowMessageUserEvent;

		// Sync the git backup before the panels list the characters and vaults, so the ones a clone or pull brings
		// are listed.
		this.GameFileService.GitRepositorySetup();

		this.CreatePanels();

		SetupVaultExportButton();
	}

	private void UIService_ShowMessageUserEvent(object? sender, ShowMessageUserEventHandlerEventArgs message)
	{
		var caption = message.Level switch
		{
			LogLevel.Error => Resources.GlobalError,
			LogLevel.Warning => Resources.GlobalWarning,
			_ => Resources.GlobalInformation,
		};

		var icon = message.Level switch
		{
			LogLevel.Error => MessageBoxIcon.Error,
			LogLevel.Warning => MessageBoxIcon.Warning,
			_ => MessageBoxIcon.Information,
		};

		var buttons = message.Buttons == ShowMessageButtons.OK ? MessageBoxButtons.OK : MessageBoxButtons.OKCancel;

		// Propagate response to the caller
		message.IsOK = this.Services.MessageBoxes.Ask(message.Message, caption, buttons, icon) == DialogResult.OK;
	}

	private void UIService_NotifyUserEvent(object? sender, string message, Color color)
	{
		this.NotificationText.ForeColor = color;
		this.NotificationText.Text = message;
	}

	private void AdjustMenuButtonVisibility()
	{
		this.forgeButton.Visible = USettings.AllowItemEdit;
		this.saveButton.Visible = USettings.EnableHotReload;
		// Get last position
		var flowctr = this.flowLayoutPanelMenuButtons.Controls;
		var lastctr = flowctr[flowctr.Count - 1];
		var lastidx = flowctr.GetChildIndex(lastctr);
		// Force "Exit" button in last position
		flowctr.SetChildIndex(this.exitButton, lastidx + 1);
	}




	private void duplicateButton_Click(object? sender, EventArgs e) => DuplicateCharacter();

	private void saveButton_Click(object? sender, EventArgs e) => this.SaveAllModifiedFiles();

	#region HighlightItems

	private void typeAssistant_Idled(object? sender, EventArgs e)
	{
		var value = (scalingTextBoxHighlight.Text ?? string.Empty).Trim();

		this.HighlightService.HighlightSearch = value;
		this.HighlightService.FindHighlight();
		this.Invoke(new System.Windows.Forms.MethodInvoker(this.Refresh));
	}


	private void scalingTextBoxHighlight_TextChanged(object? sender, EventArgs e)
	{
		/// Wait for the end of typing by delaying the call to <see cref="typeAssistant_Idled"/>
		this.typeAssistant.TextChanged();
	}

	private void scalingLabelHighlight_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
	{
		var link = (LinkLabel)sender!;
		if (highlightFilters.Visible)
		{
			highlightFilters.Visible = false;
			return;
		}

		// Show it
		if (highlightFilters.UserContext is null)
		{ // First time
			highlightFilters.UserContext = userContext;
			highlightFilters.TranslationService = TranslationService;
			highlightFilters.FontService = FontService;
			highlightFilters.HighlightService = HighlightService;
			highlightFilters.ResetAll();
			highlightFilters.InitializeFilters();
			highlightFilters.Link = link;
		}

		highlightFilters.SendToBack();// To avoid flickering
		highlightFilters.Visible = true;
		highlightFilters.BringToFront();
	}

	#endregion


}
