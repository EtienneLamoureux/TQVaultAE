//-----------------------------------------------------------------------
// <copyright file="MainForm.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using TQVaultAE.Application.Session;
using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.GUI.Components;
using TQVaultAE.GUI.Models;
using TQVaultAE.GUI.Views;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation.Win32;
using TQVaultAE.Presentation;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Contracts.Services;

namespace TQVaultAE.GUI.Forms;

/// <summary>
/// Main Dialog class
/// </summary>
public partial class MainForm : VaultForm
{
	private readonly ILogger Log;

	/// <summary>
	/// Application layer services, from <see cref="GuiServices"/>
	/// </summary>
	private readonly IVaultService vaultService;
	private readonly IItemExchangeService itemExchangeService;
	private readonly ISettingsPersistenceService settingsPersistenceService;

	/// <summary>
	/// The command line, parsed once at startup.
	/// </summary>
	private readonly CommandLineArgs commandLineArgs;

	private readonly ILoadAllService loadAllService;

	private readonly ISaveOrchestrator saveOrchestrator;

	/// <summary>
	/// The custom map (mod) whose data the session uses now.
	/// </summary>
	private readonly IGameMapService gameMapService;

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
	/// Used for show/hide table panel layout borders during debug
	/// </summary>
	private bool DebugLayoutBorderVisible = false;

	/// <summary>
	/// User current data context
	/// </summary>
	private readonly SessionContext userContext;

	/// <summary>
	/// Holds the current program version
	/// </summary>
	private string currentVersion;

	/// <summary>
	/// Form for the splash screen.
	/// </summary>
	private SplashScreenForm splashScreen = null!; // Created while the form loads.


	#endregion

#if DEBUG
	// For Design Mode
#pragma warning disable CS8618 // Designer only: the runtime constructor sets the services and state.
	public MainForm()
	{
		this.mainView = new MainFormUserControl();
		InitForm();
	}
#pragma warning restore CS8618
#endif

	/// <summary>
	/// Initializes a new instance of the MainForm class.
	/// </summary>
	public MainForm(
			GuiServices services
			, ILogger<MainForm> log
			, CommandLineArgs commandLineArgs
			, ILoadAllService loadAllService
			, ISaveOrchestrator saveOrchestrator
			, MainFormUserControl mainView
		) : base(services)
	{
		this.mainView = mainView;
		this.userContext = services.Session;
		this.vaultService = services.VaultService;
		this.itemExchangeService = services.ItemExchangeService;
		this.settingsPersistenceService = services.SettingsPersistence;
		this.commandLineArgs = commandLineArgs;
		this.loadAllService = loadAllService;
		this.saveOrchestrator = saveOrchestrator;
		this.gameMapService = services.GameMapService;
		this.gameMapService.ActiveMapChanged += this.GameMapService_ActiveMapChanged;

		Log = log;
		Log.LogInformation("TQVaultAE Initialization !");

		InitForm();

		#region Apply custom font

		// The window is built at zoom 1 (InitForm): the fonts are those of zoom 1, the user's zoom scales them below.
		this.exitButton.Font = FontService.GetFontLight(12F, UIService.FontZoom);
		this.configureButton.Font = FontService.GetFontLight(12F, UIService.FontZoom);
		this.showVaulButton.Font = FontService.GetFontLight(12F, UIService.FontZoom);
		this.aboutButton.Font = FontService.GetFontLight(8.25F, UIService.FontZoom);
		this.titleLabel.Font = FontService.GetFontLight(24F, UIService.FontZoom);
		this.searchButton.Font = FontService.GetFontLight(12F, UIService.FontZoom);
		this.saveButton.Font = FontService.GetFontLight(12F, UIService.FontZoom);
		this.forgeButton.Font = FontService.GetFontLight(12F, UIService.FontZoom);
		this.mainView.ApplyFonts();

		#endregion

		this.ApplyStartupZoom();

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

	[MemberNotNull(nameof(currentVersion))]
	private void InitForm()
	{
		this.Enabled = false;
		this.ShowInTaskbar = false;
		this.Opacity = 0;
		this.Hide();

		this.InitializeComponent();

		// Maximize, restore and minimize raise no ResizeBegin: the window reads its state change, and keeps a resize in
		// its design ratio, at each Resize (VaultForm wires ResizeBegin and ResizeEnd).
		this.Resize += this.ResizeBeginCallback;

		this.WireMainView();

		this.mainView.Initialize();

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

#if DEBUG
		this.DebugLayoutBorderVisible = false;// Set here what you want during debug
#endif
		this.mainView.ShowLayoutBorders(this.DebugLayoutBorderVisible);

		AdjustMenuButtonVisibility();

		// Wired first: the git sync below can ask questions.
		this.UIService.NotifyUserEvent += UIService_NotifyUserEvent;
		this.UIService.ShowMessageUserEvent += UIService_ShowMessageUserEvent;

		// Sync the git backup before the panels list the characters and vaults, so the ones a clone or pull brings
		// are listed.
		this.ShowGitBackupError(this.GameFileService.GitRepositorySetup());

		this.mainView.CreatePanels(this.mainView.WatchedCharacterSaves());
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
		this.mainView.ShowNotification(message, color);
	}
}
