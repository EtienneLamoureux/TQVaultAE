using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Config;
using TQVaultAE.GUI.Forms;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI;

/// <summary>
/// Services shared by the forms, panels, bag buttons and tooltips of the GUI.
/// </summary>
/// <remarks>
/// Windows Forms controls are created by hand (<c>new SackPanel(...)</c>), so they cannot get their services by
/// constructor injection one by one. This bundle is injected once into each form and passed down to the controls
/// it creates, so every dependency of the GUI is listed here instead of being fetched from the container.
/// </remarks>
public sealed class GuiServices
{
	private readonly ILoggerFactory _loggerFactory;

	public GuiServices(
		ILoggerFactory loggerFactory
		, FormFactory forms
		, UserSettings settings
		, SessionContext session
		, IFontService fontService
		, IUIService uiService
		, ITranslationService translationService
		, IBitmapService bitmapService
		, ISoundService soundService
		, IItemProvider itemProvider
		, IGamePathService gamePathService
		, IGameFileService gameFileService
		, IHighlightService highlightService
		, IItemDatabaseService itemDatabaseService
		, IItemExchangeService itemExchangeService
		, IItemMovementService itemMovementService
		, IAffixCatalogService affixCatalogService
		, IIconService iconService
		, IPathIO pathIO
		, IFileIO fileIO
		, IDirectoryIO directoryIO
		, IMessageBoxService messageBoxes
		, IVaultService vaultService
		, IStashService stashService
		, IPlayerService playerService
		, IForgeService forgeService
		, IItemEditService itemEditService
		, IItemStackingService itemStackingService
		, IItemDropService itemDropService
		, ISearchCategoryService searchCategoryService
		, ISearchQueryService searchQueryService
		, ICharacterEditService characterEditService
		, ICharacterStatsService characterStatsService
		, ISettingsPersistenceService settingsPersistence
	)
	{
		this._loggerFactory = loggerFactory;
		this.Forms = forms;
		this.Settings = settings;
		this.Session = session;
		this.FontService = fontService;
		this.UIService = uiService;
		this.TranslationService = translationService;
		this.BitmapService = bitmapService;
		this.SoundService = soundService;
		this.ItemProvider = itemProvider;
		this.GamePathService = gamePathService;
		this.GameFileService = gameFileService;
		this.HighlightService = highlightService;
		this.ItemDatabaseService = itemDatabaseService;
		this.ItemExchangeService = itemExchangeService;
		this.ItemMovementService = itemMovementService;
		this.AffixCatalogService = affixCatalogService;
		this.IconService = iconService;
		this.PathIO = pathIO;
		this.FileIO = fileIO;
		this.DirectoryIO = directoryIO;
		this.MessageBoxes = messageBoxes;
		this.VaultService = vaultService;
		this.StashService = stashService;
		this.PlayerService = playerService;
		this.ForgeService = forgeService;
		this.ItemEditService = itemEditService;
		this.ItemStackingService = itemStackingService;
		this.ItemDropService = itemDropService;
		this.SearchCategoryService = searchCategoryService;
		this.SearchQueryService = searchQueryService;
		this.CharacterEditService = characterEditService;
		this.CharacterStatsService = characterStatsService;
		this.SettingsPersistence = settingsPersistence;
	}

	/// <summary>Creates dialogs and gives access to the main form.</summary>
	public FormFactory Forms { get; }

	public UserSettings Settings { get; }
	public SessionContext Session { get; }
	public IFontService FontService { get; }
	public IUIService UIService { get; }
	public ITranslationService TranslationService { get; }
	public IBitmapService BitmapService { get; }
	public ISoundService SoundService { get; }

	/// <summary>
	/// Item display texts and their cache, for the tooltips, the item properties dialog and the panels.
	/// </summary>
	/// <remarks>
	/// The last Data provider of the GUI: <see cref="IItemProvider.GetFriendlyNames"/>,
	/// <see cref="IItemProvider.GetSetItems"/> and <see cref="IItemProvider.InvalidateFriendlyNamesCache"/>.
	/// </remarks>
	public IItemProvider ItemProvider { get; }

	public IGamePathService GamePathService { get; }
	public IGameFileService GameFileService { get; }
	public IHighlightService HighlightService { get; }
	public IItemDatabaseService ItemDatabaseService { get; }
	public IItemExchangeService ItemExchangeService { get; }
	public IItemMovementService ItemMovementService { get; }
	public IAffixCatalogService AffixCatalogService { get; }
	public IIconService IconService { get; }
	public IPathIO PathIO { get; }
	public IFileIO FileIO { get; }
	public IDirectoryIO DirectoryIO { get; }

	/// <summary>Message boxes: information, confirmations and errors.</summary>
	public IMessageBoxService MessageBoxes { get; }

	/// <summary>Vault files, lists and bag operations.</summary>
	public IVaultService VaultService { get; }

	/// <summary>Stash files and the stash panel bags.</summary>
	public IStashService StashService { get; }

	/// <summary>Players: loading, saving, requirements and stat bonuses.</summary>
	public IPlayerService PlayerService { get; }

	/// <summary>Forge rules, God mode parts and the forge itself.</summary>
	public IForgeService ForgeService { get; }

	/// <summary>Item edits of the sack context menu: affixes, bonuses, relics, seed, copies.</summary>
	public IItemEditService ItemEditService { get; }

	/// <summary>Stacking of potions, scrolls, relics and charms dropped on each other.</summary>
	public IItemStackingService ItemStackingService { get; }

	/// <summary>Rules of an item dropped on a sack: refused, put back, stacked or placed.</summary>
	public IItemDropService ItemDropService { get; }

	/// <summary>Value lists of the advanced search categories.</summary>
	public ISearchCategoryService SearchCategoryService { get; }

	/// <summary>Search queries saved by the user.</summary>
	public ISearchQueryService SearchQueryService { get; }

	/// <summary>Character edits: level, experience, difficulty, attributes and masteries.</summary>
	public ICharacterEditService CharacterEditService { get; }

	/// <summary>The sums and game values behind the character window statistics.</summary>
	public ICharacterStatsService CharacterStatsService { get; }

	/// <summary>Session state saved in the user settings, and the language of the installed game.</summary>
	public ISettingsPersistenceService SettingsPersistence { get; }

	/// <summary>Logger for <typeparamref name="T"/>.</summary>
	public ILogger<T> Logger<T>() => this._loggerFactory.CreateLogger<T>();
}

/// <summary>
/// Creates the forms registered in the container: the only GUI code allowed to resolve from it.
/// </summary>
/// <remarks>
/// Dialogs are transient (a new instance per call); <see cref="MainForm"/> is the application singleton.
/// </remarks>
public sealed class FormFactory
{
	private readonly IServiceProvider _provider;

	public FormFactory(IServiceProvider provider)
		=> this._provider = provider;

	/// <summary>The main window, owner of the item and bag tooltips.</summary>
	public MainForm MainForm => this._provider.GetRequiredService<MainForm>();

	/// <summary>Creates (or, for singletons, returns) the form <typeparamref name="TForm"/>.</summary>
	public TForm Create<TForm>() where TForm : Form
		=> this._provider.GetRequiredService<TForm>();
}
