//-----------------------------------------------------------------------
// <copyright file="MainForm.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI
{
	using Properties;
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Drawing;
	using System.Drawing.Text;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Security.Permissions;
	using System.Threading;
	using System.Windows.Forms;
	using Tooltip;
	using TQVaultData;

	/// <summary>
	/// Main Dialog class
	/// </summary>
	internal partial class MainForm : VaultForm
	{
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
		private PlayerPanel playerPanel;

		/// <summary>
		/// Holds the last sack that had mouse focus
		/// Used for Automoving
		/// </summary>
		private SackCollection lastSackHighlighted;

		/// <summary>
		/// Holds the last sack panel that had mouse focus
		/// Used for Automoving
		/// /// </summary>
		private SackPanel lastSackPanelHighlighted;

		/// <summary>
		/// Info for the current item being dragged by the mouse
		/// </summary>
		private ItemDragInfo dragInfo;

		/// <summary>
		/// Holds the coordinates of the last drag item
		/// </summary>
		private Point lastDragPoint;

		/// <summary>
		/// Dictionary of all loaded player files
		/// </summary>
		private Dictionary<string, PlayerCollection> players;

		/// <summary>
		/// Dictionary of all loaded vault files
		/// </summary>
		private Dictionary<string, PlayerCollection> vaults;

		/// <summary>
		/// Dictionary of all loaded player stash files
		/// </summary>
		private Dictionary<string, Stash> stashes;

		/// <summary>
		/// Instance of the vault panel control
		/// </summary>
		private VaultPanel vaultPanel;

		/// <summary>
		/// Instance of the second vault panel control
		/// This gets toggled with the player panel
		/// </summary>
		private VaultPanel secondaryVaultPanel;

		/// <summary>
		/// Instance of the stash panel control
		/// </summary>
		private StashPanel stashPanel;

		/// <summary>
		/// Holds that last stash that had focus
		/// </summary>
		private Stash lastStash;

		/// <summary>
		/// Bag number of the last bag with focus
		/// </summary>
		private int lastBag;

		/// <summary>
		/// Instance of the Action Button
		/// This is the animated button which pops relics and separates stacks
		/// </summary>
		private ActionButton actionButton;

		/// <summary>
		/// Holds the current program version
		/// </summary>
		private string currentVersion;

		/// <summary>
		/// Instance of the popup tool tip.
		/// </summary>
		private TTLib tooltip;

		/// <summary>
		/// Text in the tool tip
		/// </summary>
		private string tooltipText;

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
		private SplashScreenForm splashScreen;

		/// <summary>
		/// Holds the opacity interval for fading the form.
		/// </summary>
		private double fadeInterval;

		/// <summary>
		/// Initializes a new instance of the MainForm class.
		/// </summary>
		[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
		public MainForm()
		{
			this.Enabled = false;
			this.ShowInTaskbar = false;
			this.Opacity = 0;
			this.Hide();

			SetUILanguage();

			Program.LoadDB();

			this.InitializeComponent();

			this.SetupFormSize();

			#region Apply custom font & scaling

			this.exitButton.Font = Program.GetFontAlbertusMTLight(12F, TQVaultData.Database.DB.Scale);
			ScaleControl(this.exitButton);
			this.characterComboBox.Font = Program.GetFontAlbertusMTLight(13F, TQVaultData.Database.DB.Scale);
			ScaleControl(this.characterComboBox);
			ScaleControl(this.characterLabel);
			ScaleControl(this.itemTextPanel);
			ScaleControl(this.itemText);
			this.vaultListComboBox.Font = Program.GetFontAlbertusMTLight(13F, TQVaultData.Database.DB.Scale);
			ScaleControl(this.vaultListComboBox);
			this.vaultLabel.Font = Program.GetFontAlbertusMTLight(11F, TQVaultData.Database.DB.Scale);
			ScaleControl(this.vaultLabel);
			this.configureButton.Font = Program.GetFontAlbertusMTLight(12F, TQVaultData.Database.DB.Scale);
			ScaleControl(this.configureButton);
			this.customMapText.Font = Program.GetFontAlbertusMT(11.25F, TQVaultData.Database.DB.Scale);
			ScaleControl(this.customMapText);
			this.panelSelectButton.Font = Program.GetFontAlbertusMTLight(12F, TQVaultData.Database.DB.Scale);
			ScaleControl(this.panelSelectButton);
			this.secondaryVaultListComboBox.Font = Program.GetFontAlbertusMTLight(11F, TQVaultData.Database.DB.Scale);
			ScaleControl(this.secondaryVaultListComboBox);
			this.aboutButton.Font = Program.GetFontMicrosoftSansSerif(8.25F, TQVaultData.Database.DB.Scale);
			ScaleControl(this.aboutButton);
			this.titleLabel.Font = Program.GetFontAlbertusMTLight(24F, TQVaultData.Database.DB.Scale);
			ScaleControl(this.titleLabel);
			this.searchButton.Font = Program.GetFontAlbertusMTLight(12F, TQVaultData.Database.DB.Scale);
			ScaleControl(this.searchButton);

			#endregion

			try
			{
				this.tooltip = new TTLib();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				// Handle failure to create to tooltip here
				// VXPlib not registered
				checkVXPlibrary();
				this.tooltip = new TTLib();
			}

			// Changed to a global for versions in tqdebug
			Assembly a = Assembly.GetExecutingAssembly();
			AssemblyName aname = a.GetName();
			this.currentVersion = aname.Version.ToString();
			this.Text = string.Format(CultureInfo.CurrentCulture, "{0} {1}", aname.Name, this.currentVersion);

			// Setup debugging.
			TQDebug.DebugEnabled = Settings.Default.DebugEnabled;
			TQDebug.ArcFileDebugLevel = Settings.Default.ARCFileDebugLevel;
			TQDebug.DatabaseDebugLevel = Settings.Default.DatabaseDebugLevel;
			TQDebug.ItemDebugLevel = Settings.Default.ItemDebugLevel;
			TQDebug.ItemAttributesDebugLevel = Settings.Default.ItemAttributesDebugLevel;

			if (TQDebug.DebugEnabled)
			{
				// Write this version into the debug file.
				TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "Current TQVault Version: {0}", this.currentVersion));
				TQDebug.DebugWriteLine(string.Empty);
				TQDebug.DebugWriteLine("Debug Levels");
				TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "ARCFileDebugLevel: {0}", TQDebug.ArcFileDebugLevel));
				TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "DatabaseDebugLevel: {0}", TQDebug.DatabaseDebugLevel));
				TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "ItemAttributesDebugLevel: {0}", TQDebug.ItemAttributesDebugLevel));
				TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "ItemDebugLevel: {0}", TQDebug.ItemDebugLevel));
				TQDebug.DebugWriteLine(string.Empty);
			}

			SetupGamePaths();
			SetupMapName();

			// Setup localized strings.
			this.characterLabel.Text = Resources.MainFormLabel1;
			this.vaultLabel.Text = Resources.MainFormLabel2;
			this.configureButton.Text = Resources.MainFormBtnConfigure;
			this.exitButton.Text = Resources.GlobalExit;
			this.panelSelectButton.Text = Resources.MainFormBtnPanelSelect;
			this.Icon = Resources.TQVIcon;
			this.searchButton.Text = Resources.MainFormSearchButtonText;

			// Set up Item strings
			Item.ItemWith = Resources.ItemWith;
			Item.ItemRelicBonus = Resources.ItemRelicBonus;
			Item.ItemRelicCompleted = Resources.ItemRelicCompleted;
			Item.ItemQuest = Resources.ItemQuest;
			Item.ItemSeed = Resources.ItemSeed;
			Item.ItemIT = Resources.ItemIT;
			Item.ItemRagnarok = Resources.ItemRagnarok;
			Item.ItemAtlantis = Resources.ItemAtlantis;
			Item.ShowSkillLevel = Settings.Default.ShowSkillLevel;

			if (Settings.Default.NoToolTipDelay)
			{
				this.tooltip.SetNoDelay();
			}

			this.lastDragPoint.X = -1;
			this.dragInfo = new ItemDragInfo();

			this.players = new Dictionary<string, PlayerCollection>();
			this.vaults = new Dictionary<string, PlayerCollection>();
			this.stashes = new Dictionary<string, Stash>();

			this.CreatePanels();

			// Process the mouse scroll wheel to cycle through the vaults.
			this.MouseWheel += new MouseEventHandler(this.MainFormMouseWheel);

			this.splashScreen = new SplashScreenForm();
			this.splashScreen.MaximumValue = 1;
			this.splashScreen.FormClosed += new FormClosedEventHandler(this.SplashScreenClosed);

			if (Settings.Default.LoadAllFiles)
			{
				this.splashScreen.MaximumValue += LoadAllFilesTotal();
			}

			this.splashScreen.Show();
			this.splashScreen.Update();
		}

		/// <summary>
		/// Handles the registering and placement of the VXPlibrary.dll for tooltip creation.
		/// </summary>
		private void checkVXPlibrary()
		{
			if (File.Exists(Directory.GetCurrentDirectory() + "\\VXPlibrary.dll"))
			{
				// VXPlibrary.dll exists, but if it is not registered --> register it
				registerVXPlibrary();
			}
			else
			{
				throw new FileNotFoundException("VXPlibrary.dll missing from TQVault directory!");
			}
		}

		/// <summary>
		/// Registers the VXPlibrary.dll
		/// </summary>
		private void registerVXPlibrary()
		{
			Process proc = new Process();
			ProcessStartInfo info = new ProcessStartInfo();
			info.UseShellExecute = true;
			info.WorkingDirectory = "\"" + Directory.GetCurrentDirectory() + "\"";
			info.FileName = Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\Regsvr32.exe";
			info.Verb = "runas";
			info.Arguments = "/c \"" + Directory.GetCurrentDirectory() + "\\VXPlib.dll\"";
			try
			{
				proc.StartInfo = info;
				proc.Start();
				proc.WaitForExit();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		/// <summary>
		/// Handler for the ResizeEnd event.  Used to scale the internal controls after the window has been resized.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		protected override void ResizeEndCallback(object sender, EventArgs e)
		{
			base.ResizeEndCallback(sender, e);
		}

		/// <summary>
		/// Handler for the Resize event.  Used for handling the maximize and minimize functions.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		protected override void ResizeBeginCallback(object sender, EventArgs e)
		{
			base.ResizeBeginCallback(sender, e);
		}

		/// <summary>
		/// Scales the main form according to the scale factor.
		/// </summary>
		/// <param name="scaleFactor">Float which signifies the scale factor of the form.  This is an absolute from the original size unless useRelativeScaling is set to true.</param>
		/// <param name="useRelativeScaling">Indicates whether the scale factor is relative.  Used to support a resize operation.</param>
		protected override void ScaleForm(float scaleFactor, bool useRelativeScaling)
		{
			base.ScaleForm(scaleFactor, useRelativeScaling);

			this.itemText.Text = string.Empty;

			// hide the tooltip
			this.tooltipText = null;
			this.tooltip.ChangeText(this.tooltipText);

			this.Invalidate();
		}

		/// <summary>
		/// Reads the paths from the config files and sets them.
		/// </summary>
		private static void SetupGamePaths()
		{
			if (!Settings.Default.AutoDetectGamePath)
			{
				TQData.TQPath = Settings.Default.TQPath;
				TQData.ImmortalThronePath = Settings.Default.TQITPath;
			}

			// Show a message that the default path is going to be used.
			if (string.IsNullOrEmpty(Settings.Default.VaultPath))
			{
				string folderPath = Path.Combine(TQData.TQSaveFolder, "TQVaultData");

				// Check to see if we are still using a shortcut to specify the vault path and display a message
				// to use the configuration UI if we are.
				if (!Directory.Exists(folderPath) && File.Exists(Path.ChangeExtension(folderPath, ".lnk")))
				{
					MessageBox.Show(Resources.DataLinkMsg, Resources.DataLink, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				}
				else
				{
					MessageBox.Show(Resources.DataDefaultPathMsg, Resources.DataDefaultPath, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				}
			}

			TQData.TQVaultSaveFolder = Settings.Default.VaultPath;
		}

		/// <summary>
		/// Attempts to read the language from the config file and set the Current Thread's Culture and UICulture.
		/// Defaults to the OS UI Culture.
		/// </summary>
		private static void SetUILanguage()
		{
			string settingsCulture = null;
			if (!string.IsNullOrEmpty(Settings.Default.UILanguage))
			{
				settingsCulture = Settings.Default.UILanguage;
			}
			else if (!Settings.Default.AutoDetectLanguage)
			{
				settingsCulture = Settings.Default.TQLanguage;
			}

			if (!string.IsNullOrEmpty(settingsCulture))
			{
				string myCulture = null;
				foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
				{
					if (ci.EnglishName.Equals(settingsCulture, StringComparison.InvariantCultureIgnoreCase))
					{
						myCulture = ci.TextInfo.CultureName;
						break;
					}
				}

				// We found something so we will use it.
				if (!string.IsNullOrEmpty(myCulture))
				{
					try
					{
						// Sets the culture
						Thread.CurrentThread.CurrentCulture = new CultureInfo(myCulture);

						// Sets the UI culture
						Thread.CurrentThread.CurrentUICulture = new CultureInfo(myCulture);
					}
					catch (ArgumentNullException e)
					{
						TQDebug.DebugEnabled = true;
						TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "Argument Null Exception when setting the language:\n\n{0}", e));
					}
					catch (NotSupportedException e)
					{
						TQDebug.DebugEnabled = true;
						TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "Not Supported Exception when setting the language:\n\n{0}", e));
					}
				}

				// If not then we just default to the OS UI culture.
			}
		}

		/// <summary>
		/// Sets the name of the game map if a custom map is set in the config file.
		/// Defaults to Main otherwise.
		/// </summary>
		private static void SetupMapName()
		{
			// Set the map name.  Command line argument can override this setting in LoadResources().
			string mapName = "main";
			if (Settings.Default.ModEnabled)
			{
				mapName = Settings.Default.CustomMap;
			}

			TQData.MapName = mapName;
		}

		/// <summary>
		/// Updates VaultPath key from the configuration UI
		/// Needed since all vaults will need to be reloaded if this key changes.
		/// </summary>
		/// <param name="vaultPath">Path to the vault files</param>
		private static void UpdateVaultPath(string vaultPath)
		{
			Settings.Default.VaultPath = vaultPath;
			Settings.Default.Save();
		}

		/// <summary>
		/// Creates a new empty vault file
		/// </summary>
		/// <param name="name">Name of the vault.</param>
		/// <param name="file">file name of the vault.</param>
		/// <returns>Player instance of the new vault.</returns>
		private static PlayerCollection CreateVault(string name, string file)
		{
			PlayerCollection vault = new PlayerCollection(name, file);
			vault.IsVault = true;
			vault.CreateEmptySacks(12); // number of bags
			return vault;
		}

		/// <summary>
		/// Parses filename to try to determine the base character name.
		/// </summary>
		/// <param name="filename">filename of the character file</param>
		/// <returns>string containing the character name</returns>
		private static string GetNameFromFile(string filename)
		{
			// Strip off the filename
			string basePath = Path.GetDirectoryName(filename);

			// Get the containing folder
			string charName = Path.GetFileName(basePath);

			if (charName.ToUpperInvariant() == "SYS")
			{
				string fileAndExtension = Path.GetFileName(filename);
				if (fileAndExtension.ToUpperInvariant().Contains("MISC"))
				{
					// Check for the relic vault stash.
					charName = Resources.GlobalRelicVaultStash;
				}
				else if (fileAndExtension.ToUpperInvariant().Contains("WIN"))
				{
					// Check for the transfer stash.
					charName = Resources.GlobalTransferStash;
				}
				else {
					charName = null;
				}
			}
			else if (charName.StartsWith("_", StringComparison.Ordinal))
			{
				// See if it is a character folder.
				charName = charName.Substring(1);
			}
			else
			{
				// The name is bogus so return a null.
				charName = null;
			}

			return charName;
		}

		/// <summary>
		/// Gets the string name of a particular item style
		/// </summary>
		/// <param name="itemStyle">ItemStyle enumeration</param>
		/// <returns>Localized string of the item style</returns>
		public static string GetItemStyleString(ItemStyle itemStyle)
		{
			switch (itemStyle)
			{
				case ItemStyle.Broken:
					return Resources.ItemStyleBroken;

				case ItemStyle.Artifact:
					return Resources.ItemStyleArtifact;

				case ItemStyle.Formulae:
					return Resources.ItemStyleFormulae;

				case ItemStyle.Scroll:
					return Resources.ItemStyleScroll;

				case ItemStyle.Parchment:
					return Resources.ItemStyleParchment;

				case ItemStyle.Relic:
					return Resources.ItemStyleRelic;

				case ItemStyle.Potion:
					return Resources.ItemStylePotion;

				case ItemStyle.Quest:
					return Resources.ItemStyleQuest;

				case ItemStyle.Epic:
					return Resources.ItemStyleEpic;

				case ItemStyle.Legendary:
					return Resources.ItemStyleLegendary;

				case ItemStyle.Rare:
					return Resources.ItemStyleRare;

				case ItemStyle.Common:
					return Resources.ItemStyleCommon;

				default:
					return Resources.ItemStyleMundane;
			}
		}

		/// <summary>
		/// Queries the passed sack for items which contain the search string.
		/// </summary>
		/// <param name="predicate">Predicate that the items should match</param>
		/// <param name="sack">Sack that we are searching</param>
		/// <returns>List of items which contain the search string.</returns>
		private static List<Item> QuerySack(IItemPredicate predicate, SackCollection sack)
		{
			// Query the sack for the items containing the search string.
			var vaultQuery = from Item item in sack
							 where predicate.Apply(item)
							 select item;

			List<Item> tmpList = new List<Item>();

			foreach (Item item in vaultQuery)
			{
				tmpList.Add(item);
			}

			return tmpList;
		}

		private interface IItemPredicate
		{
			bool Apply(Item item);
		}

		private class ItemTruePredicate : IItemPredicate
		{
			public bool Apply(Item item)
			{
				return true;
			}

			public override string ToString()
			{
				return "true";
			}
		}

		private class ItemFalsePredicate : IItemPredicate
		{
			public bool Apply(Item item)
			{
				return false;
			}

			public override string ToString()
			{
				return "false";
			}
		}

		private class ItemAndPredicate : IItemPredicate
		{
			private readonly List<IItemPredicate> predicates;

			public ItemAndPredicate(params IItemPredicate[] predicates)
			{
				this.predicates = predicates.ToList();
			}

			public ItemAndPredicate(IEnumerable<IItemPredicate> predicates)
			{
				this.predicates = predicates.ToList();
			}

			public bool Apply(Item item)
			{
				return predicates.TrueForAll(predicate => predicate.Apply(item));
			}

			public override string ToString()
			{
				return "(" + String.Join(" && ", predicates.ConvertAll(p => p.ToString()).ToArray()) + ")";
			}
		}


		private class ItemOrPredicate : IItemPredicate
		{
			private readonly List<IItemPredicate> predicates;

			public ItemOrPredicate(params IItemPredicate[] predicates)
			{
				this.predicates = predicates.ToList();
			}

			public ItemOrPredicate(IEnumerable<IItemPredicate> predicates)
			{
				this.predicates = predicates.ToList();
			}

			public bool Apply(Item item)
			{
				return predicates.Exists(predicate => predicate.Apply(item));
			}

			public override string ToString()
			{
				return "(" + String.Join(" || ", predicates.ConvertAll(p => p.ToString()).ToArray()) + ")";
			}
		}

		private class ItemNamePredicate : IItemPredicate
		{
			private readonly string name;

			public ItemNamePredicate(string type)
			{
				this.name = type;
			}

			public bool Apply(Item item)
			{
				return item.ToString().ToUpperInvariant().Contains(name.ToUpperInvariant());
			}

			public override string ToString()
			{
				return $"Name({name})";
			}
		}

		private class ItemTypePredicate : IItemPredicate
		{
			private readonly string type;

			public ItemTypePredicate(string type)
			{
				this.type = type;
			}

			public bool Apply(Item item)
			{
				return item.ItemClass.ToUpperInvariant().Contains(type.ToUpperInvariant());
			}

			public override string ToString()
			{
				return $"Type({type})";
			}
		}

		private class ItemQualityPredicate : IItemPredicate
		{
			private readonly string quality;

			public ItemQualityPredicate(string quality)
			{
				this.quality = quality;
			}

			public bool Apply(Item item)
			{
				return GetItemStyleString(item.ItemStyle).ToUpperInvariant().Contains(quality.ToUpperInvariant());
			}

			public override string ToString()
			{
				return $"Quality({quality})";
			}
		}

		private class ItemAttributePredicate : IItemPredicate
		{
			private readonly string attribute;

			public ItemAttributePredicate(string attribute)
			{
				this.attribute = attribute;
			}

			public bool Apply(Item item)
			{
				return item.GetAttributes(true).ToUpperInvariant().Contains(attribute.ToUpperInvariant());
			}

			public override string ToString()
			{
				return $"Attribute({attribute})";
			}
		}


		/// <summary>
		/// Counts the number of files which LoadAllFiles will load.  Used to set the max value of the progress bar.
		/// </summary>
		/// <returns>Total number of files that LoadAllFiles() will load.</returns>
		private static int LoadAllFilesTotal()
		{
			string[] list;

			int numIT = 0;
			list = TQData.GetCharacterList();
			if (list != null)
			{
				numIT = list.Length;
			}

			int numVaults = 0;
			list = TQData.GetVaultList();
			if (list != null)
			{
				numVaults = list.Length;
			}

			return Math.Max(0, numIT + numIT + numVaults - 1);
		}

		/// <summary>
		/// Handler for closing the splash screen
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">FormClosedEventArgs data</param>
		private void SplashScreenClosed(object sender, FormClosedEventArgs e)
		{
			if (this.resourcesLoaded)
			{
				if (!this.loadingComplete)
				{
					this.backgroundWorker1.CancelAsync();
				}

				this.ShowMainForm();

				// the tooltip must be initialized after the main form is shown and active.
				this.tooltip.Initialize(this);
				this.tooltip.ActivateCallback = new TTLibToolTipActivate(this.ToolTipCallback);
			}
			else
			{
				Application.Exit();
			}
		}

		/// <summary>
		/// Starts the fade in of the main form.
		/// </summary>
		private void ShowMainForm()
		{
			this.fadeInTimer.Start();
			this.ShowInTaskbar = true;
			this.Enabled = true;
			this.Show();
			this.Activate();
		}

		/// <summary>
		/// Creates the form's internal panels
		/// </summary>
		private void CreatePanels()
		{
			this.CreatePlayerPanel();

			// Put the secondary vault list on top of the player list drop down
			// since only one can be shown at a time.
			this.secondaryVaultListComboBox.Location = this.characterComboBox.Location;
			this.secondaryVaultListComboBox.Enabled = false;
			this.secondaryVaultListComboBox.Visible = false;

			this.GetPlayerList();

			// Added support for custom map character list
			if (TQData.IsCustom)
			{
				this.customMapText.Visible = true;
				this.customMapText.Text = string.Format(CultureInfo.CurrentCulture, Resources.MainFormCustomMapLabel, TQData.MapName);
			}
			else
			{
				this.customMapText.Visible = false;
			}

			this.CreateVaultPanel(12); // # of bags in a vault.  This number is also buried in the CreateVault() function
			this.CreateSecondaryVaultPanel(12); // # of bags in a vault.  This number is also buried in the CreateVault() function
			this.secondaryVaultPanel.Enabled = false;
			this.secondaryVaultPanel.Visible = false;
			this.lastBag = -1;

			int textPanelOffset = Convert.ToInt32(18.0F * Database.DB.Scale);
			this.itemTextPanel.Size = new Size(this.vaultPanel.Width, Convert.ToInt32(22.0F * Database.DB.Scale));
			this.itemTextPanel.Location = new Point(this.vaultPanel.Location.X, this.ClientSize.Height - (this.itemTextPanel.Size.Height + textPanelOffset));
			this.itemText.Width = this.itemTextPanel.Width - Convert.ToInt32(4.0F * Database.DB.Scale);
			this.GetVaultList(false);

			// Now we always create the stash panel since everyone can have equipment
			this.CreateStashPanel();
			this.stashPanel.CurrentBag = 0; // set to default to the equipment panel
		}

		/// <summary>
		/// Sets the size of the main form along with scaling the internal controls for startup.
		/// </summary>
		private void SetupFormSize()
		{
			this.DrawCustomBorder = true;
			this.ResizeCustomAllowed = true;
			this.fadeInterval = Settings.Default.FadeInInterval;

			Rectangle workingArea = Screen.FromControl(this).WorkingArea;

			int formWidth = 1350;
			int formHeight = 910;
			float initialScale = 1.0F;
			Settings.Default.Scale = initialScale;
			
			// Ninakoru: trick to force close/min/max buttons to reposition...
			this.ScaleOnResize = false;
			if (workingArea.Width < formWidth || workingArea.Height < formHeight)
			{

				initialScale = Math.Min(Convert.ToSingle(workingArea.Width) / Convert.ToSingle(formWidth), Convert.ToSingle(workingArea.Height) / Convert.ToSingle(formHeight));


				if (Settings.Default.Scale > initialScale)
				{
					Settings.Default.Scale = initialScale;
				}
				this.ClientSize = new System.Drawing.Size((int)System.Math.Round(formWidth * Settings.Default.Scale), (int)System.Math.Round(formHeight * Settings.Default.Scale));
			}
			else
			{
				this.ClientSize = new System.Drawing.Size(formWidth, formHeight);
			}
			this.ScaleOnResize = true;


			TQVaultData.Database.DB.Scale = Settings.Default.Scale;

			Settings.Default.Save();

			// Save the height / width ratio for resizing.
			this.FormDesignRatio = (float)this.Height / (float)this.Width;
			this.FormMaximumSize = new Size(this.Width * 2, this.Height * 2);
			this.FormMinimumSize = new Size(
				Convert.ToInt32((float)this.Width * 0.4F),
				Convert.ToInt32((float)this.Height * 0.4F));

			this.OriginalFormSize = this.Size;
			this.OriginalFormScale = Settings.Default.Scale;

			if (CurrentAutoScaleDimensions.Width != Database.DesignDpi)
			{
				// We do not need to scale the main form controls since autoscaling will handle it.
				// Scale internally to 96 dpi for the drawing functions.
				Database.DB.Scale = this.CurrentAutoScaleDimensions.Width / Database.DesignDpi;
				this.OriginalFormScale = Database.DB.Scale;
			}

			this.LastFormSize = this.Size;

			// Set the maximized size but keep the aspect ratio.
			if (Convert.ToInt32((float)workingArea.Width * this.FormDesignRatio) < workingArea.Height)
			{
				this.MaximizedBounds = new Rectangle(
					0,
					(workingArea.Height - Convert.ToInt32((float)workingArea.Width * this.FormDesignRatio)) / 2,
					workingArea.Width,
					Convert.ToInt32((float)workingArea.Width * this.FormDesignRatio));
			}
			else
			{
				this.MaximizedBounds = new Rectangle(
					(workingArea.Width - Convert.ToInt32((float)workingArea.Height / this.FormDesignRatio)) / 2,
					0,
					Convert.ToInt32((float)workingArea.Height / this.FormDesignRatio),
					workingArea.Height);
			}
			this.Location = new Point(workingArea.Left + Convert.ToInt16((workingArea.Width - this.ClientSize.Width) / 2), workingArea.Top + Convert.ToInt16((workingArea.Height - this.ClientSize.Height) / 2));
		}

		/// <summary>
		/// Loads the resources.
		/// </summary>
		/// <param name="worker">Background worker</param>
		/// <param name="e">DoWorkEventArgs data</param>
		/// <returns>true when resource loading has completed successfully</returns>
		private bool LoadResources(BackgroundWorker worker, DoWorkEventArgs e)
		{
			// Abort the operation if the user has canceled.
			// Note that a call to CancelAsync may have set
			// CancellationPending to true just after the
			// last invocation of this method exits, so this
			// code will not have the opportunity to set the
			// DoWorkEventArgs.Cancel flag to true. This means
			// that RunWorkerCompletedEventArgs.Cancelled will
			// not be set to true in your RunWorkerCompleted
			// event handler. This is a race condition.
			if (worker.CancellationPending)
			{
				e.Cancel = true;
				return this.resourcesLoaded;
			}
			else
			{
				//read map name from ini file, main section
				if (!String.IsNullOrEmpty(IniProperties.Mod))
				{
					TQData.MapName = IniProperties.Mod;
				}

				if (!IniProperties.ShowEditingCopyFeatures)
				{
					Settings.Default.AllowItemCopy = false;
					Settings.Default.AllowItemEdit = false;
				}

				CommandLineArgs args = new CommandLineArgs();

				// Check to see if we loaded something from the command line.
				if (args.HasMapName)
				{
					TQData.MapName = args.MapName;
				}

				Database.DB.LoadDBFile();
				this.resourcesLoaded = true;
				this.backgroundWorker1.ReportProgress(1);

				if (Settings.Default.LoadAllFiles)
				{
					this.LoadAllFiles();
				}

				// Notify the form that the resources are loaded.
				return true;
			}
		}

		/// <summary>
		/// Background worker call to load the resources.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">DoWorkEventArgs data</param>
		private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
		{
			// Get the BackgroundWorker that raised this event.
			BackgroundWorker worker = sender as BackgroundWorker;

			// Assign the result of the resource loader
			// to the Result property of the DoWorkEventArgs
			// object. This is will be available to the
			// RunWorkerCompleted eventhandler.
			e.Result = this.LoadResources(worker, e);
		}

		/// <summary>
		/// Background worker has finished
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">RunWorkerCompletedEventArgs data</param>
		private void BackgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			// First, handle the case where an exception was thrown.
			if (e.Error != null)
			{
				if (MessageBox.Show(
					string.Concat(e.Error.Message, Resources.Form1BadLanguage),
					Resources.Form1ErrorLoadingResources,
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Exclamation,
					MessageBoxDefaultButton.Button1,
					RightToLeftOptions) == DialogResult.Yes)
				{
					Application.Restart();
				}
				else
				{
					Application.Exit();
				}
			}
			else if (e.Cancelled && !this.resourcesLoaded)
			{
				Application.Exit();
			}
			else if (e.Result.Equals(true))
			{
				this.loadingComplete = true;
				this.Enabled = true;
				this.LoadTransferStash();
				this.LoadRelicVaultStash();

				// Load last character here if selected
				if (Settings.Default.LoadLastCharacter)
				{
					int ind = this.characterComboBox.FindStringExact(Settings.Default.LastCharacterName);
					if (ind != -1)
					{
						this.characterComboBox.SelectedIndex = ind;
					}
				}

				string currentVault = "Main Vault";

				// See if we should load the last loaded vault
				if (Settings.Default.LoadLastVault)
				{
					currentVault = Settings.Default.LastVaultName;

					// Make sure there is something in the config file to load else load the Main Vault
					// We do not want to create new here.
					if (string.IsNullOrEmpty(currentVault) || !File.Exists(TQData.GetVaultFile(currentVault)))
					{
						currentVault = "Main Vault";
					}
				}

				this.vaultListComboBox.SelectedItem = currentVault;

				// Finally load Vault
				this.LoadVault(currentVault, false);

				this.splashScreen.UpdateText();
				this.splashScreen.ShowMainForm = true;

				CommandLineArgs args = new CommandLineArgs();

				// Allows skipping of title screen with setting
				if (args.IsAutomatic || Settings.Default.SkipTitle == true)
				{
					string player = args.Player;
					int index = this.characterComboBox.FindStringExact(player);
					if (index != -1)
					{
						this.characterComboBox.SelectedIndex = index;
					}

					this.splashScreen.CloseForm();
				}
			}
			else
			{
				// If for some reason the loading failed, but there was no error raised.
				MessageBox.Show(
					Resources.Form1ErrorLoadingResources,
					Resources.Form1ErrorLoadingResources,
					MessageBoxButtons.OK,
					MessageBoxIcon.Exclamation,
					MessageBoxDefaultButton.Button1,
					RightToLeftOptions);
				Application.Exit();
			}
		}

		/// <summary>
		/// Handler for updating the splash screen progress bar.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">ProgressChangedEventArgs data</param>
		private void BackgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			this.splashScreen.IncrementValue();
		}

		/// <summary>
		/// Creates the action button.
		/// This is the animated area which pops relics and stacks.
		/// </summary>
		private void CreateActionButton()
		{
			this.actionButton = new ActionButton(0 - this.vaultPanel.Right - 6, this.vaultPanel.Height - 30, this.dragInfo);
			this.actionButton.Location = new Point(this.vaultPanel.Right + 3, this.vaultPanel.Top + 20);
			Controls.Add(this.actionButton);
		}

		/// <summary>
		/// Creates the vault panel
		/// </summary>
		/// <param name="numBags">Number of bags in the vault panel.</param>
		private void CreateVaultPanel(int numBags)
		{
			this.vaultPanel = new VaultPanel(this.dragInfo, numBags, new Size(18, 20), this.tooltip, 1, AutoMoveLocation.Vault);

			int locationY = this.vaultListComboBox.Location.Y + Convert.ToInt32(28.0F * Database.DB.Scale);
			this.vaultPanel.DrawAsGroupBox = false;

			this.vaultPanel.Location = new Point(Convert.ToInt32(22.0F * Database.DB.Scale), locationY);
			this.vaultPanel.OnNewItemHighlighted += new EventHandler<SackPanelEventArgs>(this.NewItemHighlightedCallback);
			this.vaultPanel.OnAutoMoveItem += new EventHandler<SackPanelEventArgs>(this.AutoMoveItemCallback);
			this.vaultPanel.OnActivateSearch += new EventHandler<SackPanelEventArgs>(this.ActivateSearchCallback);
			this.vaultPanel.OnItemSelected += new EventHandler<SackPanelEventArgs>(this.ItemSelectedCallback);
			this.vaultPanel.OnClearAllItemsSelected += new EventHandler<SackPanelEventArgs>(this.ClearAllItemsSelectedCallback);
			this.vaultPanel.OnResizeForm += new EventHandler<ResizeEventArgs>(this.ResizeFormCallback);
			Controls.Add(this.vaultPanel);
		}

		/// <summary>
		/// Creates the secondary vault panel.  Player panel needs to be created before this.
		/// </summary>
		/// <param name="numBags">Number of bags in the secondary vault panel.</param>
		private void CreateSecondaryVaultPanel(int numBags)
		{
			this.secondaryVaultPanel = new VaultPanel(this.dragInfo, numBags, new Size(18, 20), this.tooltip, 1, AutoMoveLocation.SecondaryVault);
			this.secondaryVaultPanel.DrawAsGroupBox = false;

			// Place it with the same Y value as the character panel and X value of the vault panel.
			this.secondaryVaultPanel.Location = new Point(
				this.ClientSize.Width - (this.secondaryVaultPanel.Width + Convert.ToInt32(49.0F * Database.DB.Scale)),
				this.vaultPanel.Location.Y);

			this.secondaryVaultPanel.OnNewItemHighlighted += new EventHandler<SackPanelEventArgs>(this.NewItemHighlightedCallback);
			this.secondaryVaultPanel.OnAutoMoveItem += new EventHandler<SackPanelEventArgs>(this.AutoMoveItemCallback);
			this.secondaryVaultPanel.OnActivateSearch += new EventHandler<SackPanelEventArgs>(this.ActivateSearchCallback);
			this.secondaryVaultPanel.OnItemSelected += new EventHandler<SackPanelEventArgs>(this.ItemSelectedCallback);
			this.secondaryVaultPanel.OnClearAllItemsSelected += new EventHandler<SackPanelEventArgs>(this.ClearAllItemsSelectedCallback);
			this.secondaryVaultPanel.OnResizeForm += new EventHandler<ResizeEventArgs>(this.ResizeFormCallback);
			Controls.Add(this.secondaryVaultPanel);
		}

		/// <summary>
		/// Creates the player panel
		/// </summary>
		private void CreatePlayerPanel()
		{
			this.playerPanel = new PlayerPanel(this.dragInfo, 4, new Size(12, 5), new Size(8, 5), this.tooltip);

			this.playerPanel.Location = new Point(
				this.ClientSize.Width - (this.playerPanel.Width + Convert.ToInt32(22.0F * Database.DB.Scale)),
				this.characterComboBox.Location.Y + Convert.ToInt32(28.0F * Database.DB.Scale));

			this.playerPanel.DrawAsGroupBox = false;

			this.playerPanel.OnNewItemHighlighted += new EventHandler<SackPanelEventArgs>(this.NewItemHighlightedCallback);
			this.playerPanel.OnAutoMoveItem += new EventHandler<SackPanelEventArgs>(this.AutoMoveItemCallback);
			this.playerPanel.OnActivateSearch += new EventHandler<SackPanelEventArgs>(this.ActivateSearchCallback);
			this.playerPanel.OnItemSelected += new EventHandler<SackPanelEventArgs>(this.ItemSelectedCallback);
			this.playerPanel.OnClearAllItemsSelected += new EventHandler<SackPanelEventArgs>(this.ClearAllItemsSelectedCallback);
			this.playerPanel.OnResizeForm += new EventHandler<ResizeEventArgs>(this.ResizeFormCallback);
			Controls.Add(this.playerPanel);
		}

		/// <summary>
		/// Creates the stash panel
		/// </summary>
		private void CreateStashPanel()
		{
			// size params are width, height
			Size panelSize = new Size(17, 16);

			this.stashPanel = new StashPanel(this.dragInfo, panelSize, this.tooltip);

			// New location in bottom right of the Main Form.
			//Align to playerPanel
			this.stashPanel.Location = new Point(
				this.playerPanel.Location.X,
				this.ClientSize.Height - (this.stashPanel.Height + Convert.ToInt32(16.0F * Database.DB.Scale)));
			this.stashPanel.DrawAsGroupBox = false;

			this.stashPanel.OnNewItemHighlighted += new EventHandler<SackPanelEventArgs>(this.NewItemHighlightedCallback);
			this.stashPanel.OnAutoMoveItem += new EventHandler<SackPanelEventArgs>(this.AutoMoveItemCallback);
			this.stashPanel.OnActivateSearch += new EventHandler<SackPanelEventArgs>(this.ActivateSearchCallback);
			this.stashPanel.OnItemSelected += new EventHandler<SackPanelEventArgs>(this.ItemSelectedCallback);
			this.stashPanel.OnClearAllItemsSelected += new EventHandler<SackPanelEventArgs>(this.ClearAllItemsSelectedCallback);
			this.stashPanel.OnResizeForm += new EventHandler<ResizeEventArgs>(this.ResizeFormCallback);
			Controls.Add(this.stashPanel);
		}

		/// <summary>
		/// Loads the transfer stash for immortal throne
		/// </summary>
		private void LoadTransferStash()
		{
			string transferStashFile = TQData.TransferStashFile;

			// Get the transfer stash
			try
			{
				Stash stash;
				try
				{
					stash = this.stashes[transferStashFile];
				}
				catch (KeyNotFoundException)
				{
					bool stashLoaded = false;
					stash = new Stash(Resources.GlobalTransferStash, transferStashFile);
					stash.IsImmortalThrone = true;

					try
					{
						// Throw a message if the stash does not exist.
						bool stashPresent = stash.LoadFile();
						if (!stashPresent)
						{
							MessageBox.Show(Resources.StashNotFoundMsg, Resources.StashNotFound, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, RightToLeftOptions);
						}

						stashLoaded = stashPresent;
					}
					catch (ArgumentException argumentException)
					{
						string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, transferStashFile, argumentException.Message);
						MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
						stashLoaded = false;
					}

					if (stashLoaded)
					{
						this.stashes.Add(transferStashFile, stash);
					}
				}

				this.stashPanel.TransferStash = stash;
			}
			catch (IOException exception)
			{
				string msg = string.Format(CultureInfo.InvariantCulture, Resources.MainFormReadError, transferStashFile, exception.ToString());
				MessageBox.Show(msg, Resources.MainFormStashReadError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);

				this.stashPanel.TransferStash = null;
			}
		}


		/// <summary>
		/// Loads the relic vault stash
		/// </summary>
		private void LoadRelicVaultStash()
		{
			string relicVaultStashFile = TQData.RelicVaultStashFile;

			// Get the relic vault stash
			try
			{
				Stash stash;
				try
				{
					stash = this.stashes[relicVaultStashFile];
				}
				catch (KeyNotFoundException)
				{
					bool stashLoaded = false;
					stash = new Stash(Resources.GlobalRelicVaultStash, relicVaultStashFile);
					stash.IsImmortalThrone = true;

					try
					{
						// Throw a message if the stash does not exist.
						bool stashPresent = stash.LoadFile();
						if (!stashPresent)
						{
							MessageBox.Show(Resources.StashNotFoundMsg, Resources.StashNotFound, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, RightToLeftOptions);
						}

						stashLoaded = stashPresent;
					}
					catch (ArgumentException argumentException)
					{
						string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, relicVaultStashFile, argumentException.Message);
						MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
						stashLoaded = false;
					}

					if (stashLoaded)
					{
						stash.Sack.StashType = SackType.RelicVaultStash;
						this.stashes.Add(relicVaultStashFile, stash);
					}
				}

				this.stashPanel.RelicVaultStash = stash;
			}
			catch (IOException exception)
			{
				string msg = string.Format(CultureInfo.InvariantCulture, Resources.MainFormReadError, relicVaultStashFile, exception.ToString());
				MessageBox.Show(msg, Resources.MainFormStashReadError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);

				this.stashPanel.RelicVaultStash = null;
			}
		}

		/// <summary>
		/// Gets a list of available player files and populates the drop down list.
		/// </summary>
		private void GetPlayerList()
		{
			// Initialize the character combo-box
			this.characterComboBox.Items.Clear();

			string[] charactersIT = TQData.GetCharacterList();

			int numIT = 0;
			if (charactersIT != null)
			{
				numIT = charactersIT.Length;
			}

			if (numIT < 1)
			{
				this.characterComboBox.Items.Add(Resources.MainFormNoCharacters);
				this.characterComboBox.SelectedIndex = 0;
			}
			else
			{
				this.characterComboBox.Items.Add(Resources.MainFormSelectCharacter);
				this.characterComboBox.SelectedIndex = 0;

				string characterDesignator = string.Empty;

				// Modified by VillageIdiot
				// Added to support custom Maps
				if (TQData.IsCustom)
				{
					characterDesignator = string.Concat(characterDesignator, "<Custom Map>");
				}

				// Combine the 2 arrays into 1 then add them
				string[] characters = new string[numIT];
				int i;
				int j = 0;

				// Put the IT chars first since that is most likely what people want to use.
				for (i = 0; i < numIT; ++i)
				{
					characters[j++] = string.Concat(charactersIT[i], characterDesignator);
				}

				this.characterComboBox.Items.AddRange(characters);
			}
		}

		/// <summary>
		/// Gets a list of all available vault files and populates the drop down list.
		/// </summary>
		/// <param name="loadVault">Indicates whether the list will also load the last vault selected.</param>
		private void GetVaultList(bool loadVault)
		{
			string[] vaults = TQData.GetVaultList();

			// Added by VillageIdiot
			// See if the Vault path was set during GetVaultList and update the key accordingly
			if (TQData.VaultFolderChanged)
			{
				UpdateVaultPath(TQData.TQVaultSaveFolder);
			}

			string currentVault;

			// There was something already selected so we will save it.
			if (this.vaultListComboBox.Items.Count > 0)
			{
				currentVault = this.vaultListComboBox.SelectedItem.ToString();
			}
			else
			{
				currentVault = "Main Vault";
			}

			// Added by VillageIdiot
			// Clear the list before creating since this function can be called multiple times.
			this.vaultListComboBox.Items.Clear();

			this.vaultListComboBox.Items.Add(Resources.MainFormMaintainVault);

			// Add Main Vault first
			if (this.secondaryVaultListComboBox.SelectedItem == null || this.secondaryVaultListComboBox.SelectedItem.ToString() != "Main Vault")
			{
				this.vaultListComboBox.Items.Add("Main Vault");
			}

			if (vaults != null && vaults.Length > 0)
			{
				// now add everything EXCEPT for main vault
				foreach (string vault in vaults)
				{
					if (!vault.Equals("Main Vault"))
					{
						// we already added main vault
						if (this.secondaryVaultListComboBox.SelectedItem != null && vault.Equals(this.secondaryVaultListComboBox.SelectedItem.ToString()) && this.showSecondaryVault)
						{
							break;
						}

						this.vaultListComboBox.Items.Add(vault);
					}
				}
			}

			// See if we should load the last loaded vault
			if (Settings.Default.LoadLastVault)
			{
				currentVault = Settings.Default.LastVaultName;

				// Make sure there is something in the config file to load else load the Main Vault
				// We do not want to create new here.
				if (string.IsNullOrEmpty(currentVault) || !File.Exists(TQData.GetVaultFile(currentVault)))
				{
					currentVault = "Main Vault";
				}
			}

			if (loadVault)
			{
				this.vaultListComboBox.SelectedItem = currentVault;

				// Finally load Vault
				this.LoadVault(currentVault, false);
			}
		}

		/// <summary>
		/// Reads the list from the main vault combo box.
		/// To support adding another vault panel to the screen.
		/// </summary>
		private void GetSecondaryVaultList()
		{
			string currentVault;

			// There was something already selected so we will save it.
			if (this.secondaryVaultListComboBox.Items.Count > 0)
			{
				currentVault = this.secondaryVaultListComboBox.SelectedItem.ToString();
			}
			else
			{
				currentVault = Resources.MainFormSelectVault;
			}

			if (currentVault == this.vaultListComboBox.SelectedItem.ToString())
			{
				// Clear the selection if it is already loaded on the main panel.
				currentVault = Resources.MainFormSelectVault;
			}

			// Clear the list before creating since this function can be called multiple times.
			this.secondaryVaultListComboBox.Items.Clear();
			this.secondaryVaultListComboBox.Items.Add(Resources.MainFormSelectVault);

			if (this.vaultListComboBox.Items.Count > 1)
			{
				// Now add everything EXCEPT for the selected vault in the other panel.
				for (int i = 1; i < this.vaultListComboBox.Items.Count; ++i)
				{ // Skip over the maintenance selection.
					if (i != this.vaultListComboBox.SelectedIndex)
					{ // Skip over the selected item.
						this.secondaryVaultListComboBox.Items.Add(this.vaultListComboBox.Items[i]);
					}
				}
			}

			this.secondaryVaultListComboBox.SelectedItem = currentVault;

			// Finally load Vault
			this.LoadVault(currentVault, true);
		}

		/// <summary>
		/// Loads a vault file
		/// </summary>
		/// <param name="vaultName">Name of the vault</param>
		/// <param name="secondaryVault">flag indicating whether this selection is for the secondary panel</param>
		private void LoadVault(string vaultName, bool secondaryVault)
		{
			PlayerCollection vault = null;
			if (secondaryVault && vaultName == Resources.MainFormSelectVault)
			{
				if (this.secondaryVaultPanel.Player != null)
				{
					this.secondaryVaultPanel.Player = null;
				}
			}
			else
			{
				// Get the filename
				string filename = TQData.GetVaultFile(vaultName);

				// Check the cache
				try
				{
					vault = this.vaults[filename];
				}
				catch (KeyNotFoundException)
				{
					// We need to load the vault.
					bool vaultLoaded = false;
					if (!File.Exists(filename))
					{
						// the file does not exist so create a new vault.
						vault = CreateVault(vaultName, filename);
						vaultLoaded = true;
					}
					else
					{
						vault = new PlayerCollection(vaultName, filename);
						vault.IsVault = true;
						try
						{
							vault.LoadFile();
							vaultLoaded = true;
						}
						catch (ArgumentException argumentException)
						{
							string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, filename, argumentException.Message);
							MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
							vaultLoaded = false;
						}
					}

					// Add the vault to the cache, but only if we create it or it successfully loads.
					if (vaultLoaded)
					{
						this.vaults.Add(filename, vault);
					}
				}
			}

			// Now assign the vault to the vaultpanel
			if (secondaryVault)
			{
				this.secondaryVaultPanel.Player = vault;
			}
			else
			{
				this.vaultPanel.Player = vault;
			}
		}

		/// <summary>
		/// Used to toggle the upper display between the player panel or another vault.
		/// </summary>
		private void UpdateTopPanel()
		{
			if (this.showSecondaryVault)
			{
				this.playerPanel.Enabled = false;
				this.playerPanel.Visible = false;
				this.playerPanel.SackPanel.ClearSelectedItems();
				this.playerPanel.BagSackPanel.ClearSelectedItems();
				this.stashPanel.Visible = false;
				this.stashPanel.Enabled = false;
				this.secondaryVaultPanel.Enabled = true;
				this.secondaryVaultPanel.Visible = true;
				this.panelSelectButton.Text = Resources.MainFormBtnShowPlayer;
				this.characterComboBox.Enabled = false;
				this.characterComboBox.Visible = false;
				this.secondaryVaultListComboBox.Enabled = true;
				this.secondaryVaultListComboBox.Visible = true;
				this.characterLabel.Text = Resources.MainForm2ndVault;
				this.lastStash = this.stashPanel.Stash;
				this.lastBag = this.stashPanel.CurrentBag;
				this.stashPanel.Player = null;
				this.stashPanel.Stash = null;
				if (this.stashPanel.CurrentBag != 1)
				{
					this.stashPanel.SackPanel.ClearSelectedItems();
					this.stashPanel.CurrentBag = 1;
				}

				this.vaultPanel.SackPanel.SecondaryVaultShown = true;
				this.stashPanel.SackPanel.SecondaryVaultShown = true;

				this.secondaryVaultPanel.SackPanel.IsSecondaryVault = true;
				this.GetSecondaryVaultList();
			}
			else
			{
				this.stashPanel.Visible = true;
				this.stashPanel.Enabled = true;
				this.secondaryVaultPanel.Enabled = false;
				this.secondaryVaultPanel.Visible = false;
				this.secondaryVaultPanel.SackPanel.ClearSelectedItems();
				this.playerPanel.Enabled = true;
				this.playerPanel.Visible = true;
				this.panelSelectButton.Text = Resources.MainFormBtnPanelSelect;
				this.characterComboBox.Enabled = true;
				this.characterComboBox.Visible = true;
				this.secondaryVaultListComboBox.Enabled = false;
				this.secondaryVaultListComboBox.Visible = false;
				this.characterLabel.Text = Resources.MainFormLabel1;
				this.stashPanel.Player = this.playerPanel.Player;
				if (this.lastStash != null)
				{
					this.stashPanel.Stash = this.lastStash;
					if (this.lastBag != -1 && this.lastBag != this.stashPanel.CurrentBag)
					{
						this.stashPanel.CurrentBag = this.lastBag;
						this.stashPanel.SackPanel.ClearSelectedItems();
					}
				}

				this.vaultPanel.SackPanel.SecondaryVaultShown = false;
				this.stashPanel.SackPanel.SecondaryVaultShown = false;
			}
		}

		/// <summary>
		/// Handler for the exit button.
		/// Closes the main form
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void ExitButtonClick(object sender, EventArgs e)
		{
			this.Close();
		}

		/// <summary>
		/// Shows things that you may want to know before a close.
		/// Like holding an item
		/// </summary>
		/// <returns>TRUE if none of the conditions exist or the user selected to ignore the message</returns>
		private bool DoCloseStuff()
		{
			bool modifiedFiles = false;
			bool ok = false;
			try
			{
				// Make sure we are not dragging anything
				if (this.dragInfo.IsActive)
				{
					MessageBox.Show(Resources.MainFormHoldingItem, Resources.MainFormHoldingItem2, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, RightToLeftOptions);
					return false;
				}

				modifiedFiles = this.SaveAllModifiedFiles();

				// Added by VillageIdiot
				this.SaveConfiguration();

				ok = true;
			}
			catch (IOException exception)
			{
				MessageBox.Show(exception.ToString(), Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, RightToLeftOptions);
			}

			return ok;
		}

		/// <summary>
		/// Handler for changing the Character drop down selection.
		/// </summary>
		/// <param name="sender">Sender object</param>
		/// <param name="e">EventArgs data</param>
		private void CharacterComboBoxSelectedIndexChanged(object sender, EventArgs e)
		{
			// Hmm. We can load a character now!
			string selectedText = this.characterComboBox.SelectedItem.ToString();

			// See if they actually changed their selection and ignore "No TQ characters detected"
			if (selectedText.Equals(Resources.MainFormSelectCharacter) || selectedText.Equals(Resources.MainFormNoCharacters)
				|| selectedText.Equals(Resources.MainFormNoCustomChars))
			{
				// no char selected
				this.ClearPlayer();
			}
			else
			{
				this.LoadPlayer(selectedText);
			}
		}

		/// <summary>
		/// Clears out the selected player
		/// Changed by VillageIdiot to a separate function.
		/// </summary>
		private void ClearPlayer()
		{
			if (this.playerPanel.Player != null)
			{
				this.playerPanel.Player = null;
				this.stashPanel.Player = null;
				this.stashPanel.CurrentBag = 0;

				if (this.stashPanel.Stash != null)
				{
					this.stashPanel.Stash = null;
				}
			}
		}

		/// <summary>
		/// Loads a player using the drop down list.
		/// Assumes designators are appended to character name.
		/// Changed by VillageIdiot to a separate function.
		/// </summary>
		/// <param name="selectedText">Player string from the drop down list.</param>
		private void LoadPlayer(string selectedText)
		{
			string customDesignator = "<Custom Map>";

			bool isCustom = selectedText.EndsWith(customDesignator, StringComparison.Ordinal);
			if (isCustom)
			{
				// strip off the end from the player name.
				selectedText = selectedText.Remove(selectedText.IndexOf(customDesignator, StringComparison.Ordinal), customDesignator.Length);
			}

			string playerFile = TQData.GetPlayerFile(selectedText);

			// Get the player
			try
			{
				PlayerCollection player;
				try
				{
					player = this.players[playerFile];
				}
				catch (KeyNotFoundException)
				{
					bool playerLoaded = false;
					player = new PlayerCollection(selectedText, playerFile);
					try
					{
						player.LoadFile();
						playerLoaded = true;
					}
					catch (ArgumentException argumentException)
					{
						string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, playerFile, argumentException.Message);
						MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
						playerLoaded = false;
					}

					// Only add the player to the list if it loaded successfully.
					if (playerLoaded)
					{
						this.players.Add(playerFile, player);
					}
				}

				this.playerPanel.Player = player;
				this.stashPanel.Player = player;
				this.stashPanel.CurrentBag = 0;
			}
			catch (IOException exception)
			{
				string msg = string.Format(CultureInfo.InvariantCulture, Resources.MainFormReadError, playerFile, exception.ToString());
				MessageBox.Show(msg, Resources.MainFormPlayerReadError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);

				this.playerPanel.Player = null;
				this.characterComboBox.SelectedIndex = 0;
			}

			string stashFile = TQData.GetPlayerStashFile(selectedText);

			// Get the player's stash
			try
			{
				Stash stash;
				try
				{
					stash = this.stashes[stashFile];
				}
				catch (KeyNotFoundException)
				{
					bool stashLoaded = false;
					stash = new Stash(selectedText, stashFile);
					try
					{
						bool stashPresent = stash.LoadFile();

						// Throw a message if the stash is not present.
						if (!stashPresent)
						{
							MessageBox.Show(Resources.StashNotFoundMsg, Resources.StashNotFound, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, RightToLeftOptions);
						}

						stashLoaded = stashPresent;
					}
					catch (ArgumentException argumentException)
					{
						string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, stashFile, argumentException.Message);
						MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
						stashLoaded = false;
					}

					if (stashLoaded)
					{
						this.stashes.Add(stashFile, stash);
					}
				}

				this.stashPanel.Stash = stash;
			}
			catch (IOException exception)
			{
				string msg = string.Concat(Resources.MainFormReadError, stashFile, exception.ToString());
				MessageBox.Show(msg, Resources.MainFormStashReadError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);

				this.stashPanel.Stash = null;
			}

		}

		/// <summary>
		/// Loads all of the players, stashes, and vaults.
		/// Shows a progress dialog.
		/// Used for the searching function.
		/// </summary>
		private void LoadAllFiles()
		{
			// Check to see if we failed the last time we tried loading all of the files.
			// If we did fail then turn it off and skip it.
			if (!Settings.Default.LoadAllFilesCompleted)
			{
				if (MessageBox.Show(
					Resources.MainFormDisableLoadAllFiles,
					Resources.MainFormDisableLoadAllFilesCaption,
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Information,
					MessageBoxDefaultButton.Button1,
					RightToLeftOptions) == DialogResult.Yes)
				{
					Settings.Default.LoadAllFilesCompleted = true;
					Settings.Default.LoadAllFiles = false;
					Settings.Default.Save();
					return;
				}
			}

			string[] vaults = TQData.GetVaultList();

			string[] charactersIT = TQData.GetCharacterList();

			int numIT = 0;
			if (charactersIT != null)
			{
				numIT = charactersIT.Length;
			}

			int numVaults = 0;
			if (vaults != null)
			{
				numVaults = vaults.Length;
			}

			// Since this takes a while, show a progress dialog box.
			int total = numIT + numIT + numVaults - 1;

			if (total > 0)
			{
				// We were successful last time so we reset the flag for this attempt.
				Settings.Default.LoadAllFilesCompleted = false;
				Settings.Default.Save();
			}
			else
			{
				return;
			}

			// Load all of the Immortal Throne player files and stashes.
			for (int i = 0; i < numIT; ++i)
			{
				string playerFile = TQData.GetPlayerFile(charactersIT[i]);

				// Get the player
				try
				{
					PlayerCollection player;

					if (players.ContainsKey(playerFile))
					{
						player = this.players[playerFile];
					}
					else
					{
						bool playerLoaded = false;
						player = new PlayerCollection(charactersIT[i], playerFile);
						player.IsImmortalThrone = true;
						try
						{
							player.LoadFile();
							playerLoaded = true;
						}
						catch (ArgumentException argumentException)
						{
							string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, playerFile, argumentException.Message);
							MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
							playerLoaded = false;
						}

						if (playerLoaded)
						{
							this.players.Add(playerFile, player);
						}
					}
				}
				catch (IOException exception)
				{
					MessageBox.Show(exception.ToString(), Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				}

				this.backgroundWorker1.ReportProgress(1);

				string stashFile = TQData.GetPlayerStashFile(charactersIT[i]);

				// Get the player's stash
				try
				{
					Stash stash;
					if (stashes.ContainsKey(stashFile))
					{
						stash = this.stashes[stashFile];
					}
					else
					{
						bool stashLoaded = false;
						stash = new Stash(charactersIT[i], stashFile);
						stash.IsImmortalThrone = true;

						try
						{
							// Eat any file not found messages for the stash.
							stash.LoadFile();
							stashLoaded = true;
						}
						catch (ArgumentException argumentException)
						{
							string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, stashFile, argumentException.Message);
							MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
							stashLoaded = false;
						}

						if (stashLoaded)
						{
							this.stashes.Add(stashFile, stash);
						}
					}
				}
				catch (IOException exception)
				{
					MessageBox.Show(exception.ToString(), Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				}

				this.backgroundWorker1.ReportProgress(1);
			}

			// Load all of the vaults.
			for (int i = 0; i < numVaults; ++i)
			{
				// Get the filename
				string filename = TQData.GetVaultFile(vaults[i]);

				// Check the cache
				PlayerCollection vault;
				try
				{
					vault = this.vaults[filename];
				}
				catch (KeyNotFoundException)
				{
					// We need to load the vault.
					bool vaultLoaded = false;
					vault = new PlayerCollection(vaults[i], filename);
					vault.IsVault = true;
					try
					{
						vault.LoadFile();
						vaultLoaded = true;
					}
					catch (ArgumentException argumentException)
					{
						string msg = string.Format(CultureInfo.CurrentUICulture, "{0}\n{1}\n{2}", Resources.MainFormPlayerReadError, filename, argumentException.Message);
						MessageBox.Show(msg, Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, RightToLeftOptions);
						vaultLoaded = false;
					}

					// Add the vault to the cache
					// but only if we parse it successfully.
					if (vaultLoaded)
					{
						this.vaults.Add(filename, vault);
					}
				}

				this.backgroundWorker1.ReportProgress(1);
			}

			// We made it so set the flag to indicate we were successful.
			Settings.Default.LoadAllFilesCompleted = true;
			Settings.Default.Save();
		}

		/// <summary>
		/// Callback for highlighting a new item.
		/// Updates the text box on the main form.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">SackPanelEventArgs data</param>
		private void NewItemHighlightedCallback(object sender, SackPanelEventArgs e)
		{
			Item item = e.Item;
			SackCollection sack = e.Sack;
			SackPanel sackPanel = (SackPanel)sender;
			if (item == null)
			{
				// Only do something if this sack is the "owner" of the current item highlighted.
				if (sack == this.lastSackHighlighted)
				{
					this.itemText.Text = string.Empty;

					// hide the tooltip
					this.tooltipText = null;
					this.tooltip.ChangeText(this.tooltipText);
				}
			}
			else
			{
				string text = item.ToString();
				Color color = item.GetColorTag(text);
				text = Item.ClipColorTag(text);
				this.itemText.ForeColor = color;
				this.itemText.Text = text;

				string attributes = item.GetAttributes(true); // true means hide uninteresting attributes
				string setitems = item.GetItemSetString();
				string reqs = item.GetRequirements();

				// combine the 2
				if (reqs.Length < 1)
				{
					this.tooltipText = attributes;
				}
				else if (setitems.Length < 1)
				{
					string reqTitle = Database.MakeSafeForHtml("?Requirements?");
					reqTitle = string.Format(CultureInfo.InvariantCulture, "<font size=+2 color={0}>{1}</font><br>", Database.HtmlColor(Item.GetColor(ItemStyle.Potion)), reqTitle);
					string separator = string.Format(CultureInfo.InvariantCulture, "<hr color={0}><br>", Database.HtmlColor(Item.GetColor(ItemStyle.Broken)));
					this.tooltipText = string.Concat(attributes, separator, reqs);
				}
				else
				{
					string reqTitle = Database.MakeSafeForHtml("?Requirements?");
					reqTitle = string.Format(CultureInfo.InvariantCulture, "<font size=+2 color={0}>{1}</font><br>", Database.HtmlColor(Item.GetColor(ItemStyle.Potion)), reqTitle);
					string separator1 = string.Format(CultureInfo.InvariantCulture, "<hr color={0}>", Database.HtmlColor(Item.GetColor(ItemStyle.Broken)));
					string separator2 = string.Format(CultureInfo.InvariantCulture, "<hr color={0}><br>", Database.HtmlColor(Item.GetColor(ItemStyle.Broken)));
					this.tooltipText = string.Concat(attributes, separator1, setitems, separator2, reqs);
				}

				// show tooltip
				this.tooltipText = string.Concat(Database.DB.TooltipBodyTag, this.tooltipText);
				this.tooltip.ChangeText(this.tooltipText);
			}

			this.lastSackHighlighted = sack;
			this.lastSackPanelHighlighted = sackPanel;
		}

		/// <summary>
		/// Used to clear out selections on other panels if the user tries to select across multiple panels.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">SackPanelEventArgs data</param>
		private void ItemSelectedCallback(object sender, SackPanelEventArgs e)
		{
			SackPanel sackPanel = (SackPanel)sender;

			if (this.playerPanel.SackPanel == sackPanel)
			{
				this.playerPanel.BagSackPanel.ClearSelectedItems();
				this.vaultPanel.SackPanel.ClearSelectedItems();
				this.secondaryVaultPanel.SackPanel.ClearSelectedItems();
				this.stashPanel.SackPanel.ClearSelectedItems();
			}
			else if (this.playerPanel.BagSackPanel == sackPanel)
			{
				this.playerPanel.SackPanel.ClearSelectedItems();
				this.vaultPanel.SackPanel.ClearSelectedItems();
				this.secondaryVaultPanel.SackPanel.ClearSelectedItems();
				this.stashPanel.SackPanel.ClearSelectedItems();
			}
			else if (this.vaultPanel.SackPanel == sackPanel)
			{
				this.playerPanel.SackPanel.ClearSelectedItems();
				this.playerPanel.BagSackPanel.ClearSelectedItems();
				this.secondaryVaultPanel.SackPanel.ClearSelectedItems();
				this.stashPanel.SackPanel.ClearSelectedItems();
			}
			else if (this.secondaryVaultPanel.SackPanel == sackPanel)
			{
				this.playerPanel.SackPanel.ClearSelectedItems();
				this.playerPanel.BagSackPanel.ClearSelectedItems();
				this.vaultPanel.SackPanel.ClearSelectedItems();
				this.stashPanel.SackPanel.ClearSelectedItems();
			}
			else if (this.stashPanel.SackPanel == sackPanel)
			{
				this.playerPanel.SackPanel.ClearSelectedItems();
				this.playerPanel.BagSackPanel.ClearSelectedItems();
				this.vaultPanel.SackPanel.ClearSelectedItems();
				this.secondaryVaultPanel.SackPanel.ClearSelectedItems();
			}
		}

		/// <summary>
		/// Used to clear the selection when a bag button is clicked.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">SackPanelEventArgs data</param>
		private void ClearAllItemsSelectedCallback(object sender, SackPanelEventArgs e)
		{
			this.playerPanel.SackPanel.ClearSelectedItems();
			this.playerPanel.BagSackPanel.ClearSelectedItems();
			this.vaultPanel.SackPanel.ClearSelectedItems();
			this.secondaryVaultPanel.SackPanel.ClearSelectedItems();
			this.stashPanel.SackPanel.ClearSelectedItems();
		}

		/// <summary>
		/// Callback for activating the search text box.
		/// Used when a hot key is pressed.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">SackPanelEventArgs data</param>
		private void ActivateSearchCallback(object sender, SackPanelEventArgs e)
		{
			this.OpenSearchDialog();
		}

		/// <summary>
		/// Used for sending items between sacks or panels.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">SackPanelEventArgs data</param>
		private void AutoMoveItemCallback(object sender, SackPanelEventArgs e)
		{
			SackPanel sackPanel = (SackPanel)sender;

			// Make sure we have to move something.
			if (this.dragInfo.IsAutoMoveActive)
			{
				SackCollection oldSack = null;
				VaultPanel destinationPlayerPanel = null;
				int sackNumber = 0;

				SackPanel destinationSackPanel = null;
				if (this.dragInfo.AutoMove < AutoMoveLocation.Vault)
				{
					// This is a sack to sack move on the same panel.
					destinationSackPanel = sackPanel;
					switch (sackPanel.SackType)
					{
						case SackType.Vault:
							{
								if (sackPanel.IsSecondaryVault)
								{
									destinationPlayerPanel = this.secondaryVaultPanel;
								}
								else
								{
									destinationPlayerPanel = this.vaultPanel;
								}

								break;
							}

						default:
							{
								destinationPlayerPanel = this.playerPanel;
								break;
							}
					}

					sackNumber = (int)this.dragInfo.AutoMove;
				}
				else if (this.dragInfo.AutoMove == AutoMoveLocation.Vault)
				{
					// Vault
					destinationPlayerPanel = this.vaultPanel;
					destinationSackPanel = destinationPlayerPanel.SackPanel;
					sackNumber = destinationPlayerPanel.CurrentBag;
				}
				else if (this.dragInfo.AutoMove == AutoMoveLocation.Player)
				{
					// Player
					destinationPlayerPanel = this.playerPanel;
					destinationSackPanel = ((PlayerPanel)destinationPlayerPanel).SackPanel;

					// Main Player panel
					sackNumber = 0;
				}
				else if (this.dragInfo.AutoMove == AutoMoveLocation.SecondaryVault)
				{
					// Secondary Vault
					destinationPlayerPanel = this.secondaryVaultPanel;
					destinationSackPanel = destinationPlayerPanel.SackPanel;
					sackNumber = destinationPlayerPanel.CurrentBag;
				}

				// Special Case for moving to stash.
				if (this.dragInfo.AutoMove == AutoMoveLocation.Stash)
				{
					// Check if we are moving to the player's stash
					if (this.stashPanel.CurrentBag == 2 && this.stashPanel.Player == null)
					{
						// We have nowhere to send the item so cancel the move.
						this.dragInfo.Cancel();
						return;
					}

					// Check for the equipment panel
					if (this.stashPanel.CurrentBag == 0)
					{
						// Equipment Panel is active so switch to the transfer stash.
						this.stashPanel.CurrentBag = 1;
					}

					// Check the transfer stash
					if (this.stashPanel.TransferStash == null && this.stashPanel.CurrentBag == 1)
					{
						// We have nowhere to send the item so cancel the move.
						this.dragInfo.Cancel();
						return;
					}

					// Check the relic vault stash
					if (this.stashPanel.RelicVaultStash == null && this.stashPanel.CurrentBag == 3)
					{
						// We have nowhere to send the item so cancel the move.
						this.dragInfo.Cancel();
						return;
					}

					// See if we have an open space to put the item.
					Point location = this.stashPanel.SackPanel.FindOpenCells(this.dragInfo.Item.Width, this.dragInfo.Item.Height);

					// We have no space in the sack so we cancel.
					if (location.X == -1)
					{
						this.dragInfo.Cancel();
					}
					else
					{
						Item dragItem = this.dragInfo.Item;

						if (!this.stashPanel.SackPanel.IsItemValidForPlacement(dragItem))
						{
							this.dragInfo.Cancel();
							return;
						}

						// Use the same method as if we used to mouse to pickup and place the item.
						this.dragInfo.MarkPlaced(-1);
						dragItem.PositionX = location.X;
						dragItem.PositionY = location.Y;
						this.stashPanel.SackPanel.Sack.AddItem(dragItem);

						this.lastSackPanelHighlighted.Invalidate();
						this.stashPanel.Refresh();
					}
				}
				else
				{
					// The stash is not involved.
					if (destinationPlayerPanel.Player == null)
					{
						// We have nowhere to send the item so cancel the move.
						this.dragInfo.Cancel();
						return;
					}

					// Save the current sack.
					oldSack = destinationSackPanel.Sack;

					// Find the destination sack.
					destinationSackPanel.Sack = destinationPlayerPanel.Player.GetSack(sackNumber);

					// See if we have an open space to put the item.
					Point location = destinationSackPanel.FindOpenCells(this.dragInfo.Item.Width, this.dragInfo.Item.Height);

					// CurrentBag only returns the values for the bag panels and is zero based.  Main sack is not included.
					int destination = destinationPlayerPanel.CurrentBag;

					// We need to accout for the player panel offsets.
					if (sackPanel.SackType == SackType.Sack)
					{
						destination++;
					}
					else if (sackPanel.SackType == SackType.Player)
					{
						destination = 0;
					}

					// We either have no space or are sending the item to the same sack so we cancel.
					if (location.X == -1 || (int)this.dragInfo.AutoMove == destination)
					{
						destinationSackPanel.Sack = oldSack;
						this.dragInfo.Cancel();
					}
					else
					{
						Item dragItem = this.dragInfo.Item;

						// Use the same method as if we used to mouse to pickup and place the item.
						this.dragInfo.MarkPlaced(-1);
						dragItem.PositionX = location.X;
						dragItem.PositionY = location.Y;
						destinationSackPanel.Sack.AddItem(dragItem);

						// Set it back to the original sack so the display does not change.
						destinationSackPanel.Sack = oldSack;
						sackPanel.Invalidate();
						destinationPlayerPanel.Refresh();
					}
				}
			}
		}

		/// <summary>
		/// Method for the maintain vault files dialog
		/// </summary>
		private void MaintainVaultFiles()
		{
			try
			{
				this.SaveAllModifiedFiles();
				VaultMaintenanceDialog dlg = new VaultMaintenanceDialog();
				dlg.Scale(new SizeF(Database.DB.Scale, Database.DB.Scale));

				if (dlg.ShowDialog() == DialogResult.OK)
				{
					string newName = dlg.Target;
					string oldName = dlg.Source;
					bool handled = false;

					// Create a new vault?
					if (dlg.Action == VaultMaintenanceDialog.VaultMaintenance.New && newName != null)
					{
						// Add the name to the list
						this.vaultListComboBox.Items.Add(newName);

						// Select it
						this.vaultListComboBox.SelectedItem = newName;

						// Load it
						this.LoadVault(newName, false);
						handled = true;
					}
					else if (dlg.Action == VaultMaintenanceDialog.VaultMaintenance.Copy && newName != null && oldName != null)
					{
						string oldFilename = TQData.GetVaultFile(oldName);
						string newFilename = TQData.GetVaultFile(newName);

						// Make sure we save all modifications first.
						this.SaveAllModifiedFiles();

						// Make sure the vault file to copy exists and the new name does not.
						if (File.Exists(oldFilename) && !File.Exists(newFilename))
						{
							File.Copy(oldFilename, newFilename);

							// Add the new name to the list
							this.vaultListComboBox.Items.Add(newName);

							// Select the new name
							this.vaultListComboBox.SelectedItem = newName;

							// Load the new file.
							this.LoadVault(newName, false);
							handled = true;
						}
					}
					else if (dlg.Action == VaultMaintenanceDialog.VaultMaintenance.Delete && oldName != null)
					{
						string filename = TQData.GetVaultFile(oldName);

						// Make sure we save all modifications first.
						this.SaveAllModifiedFiles();

						// Make sure the vault file to delete exists.
						if (File.Exists(filename))
						{
							File.Delete(filename);
						}

						// Remove the file from the cache.
						this.vaults.Remove(filename);

						// Remove the deleted file from the list.
						this.vaultListComboBox.Items.Remove(oldName);

						// Select the Main Vault since we know it's still there.
						this.vaultListComboBox.SelectedIndex = 1;

						handled = true;
					}
					else if (dlg.Action == VaultMaintenanceDialog.VaultMaintenance.Rename && newName != null && oldName != null)
					{
						string oldFilename = TQData.GetVaultFile(oldName);
						string newFilename = TQData.GetVaultFile(newName);

						// Make sure we save all modifications first.
						this.SaveAllModifiedFiles();

						// Make sure the vault file to rename exists and the new name does not.
						if (File.Exists(oldFilename) && !File.Exists(newFilename))
						{
							File.Move(oldFilename, newFilename);

							// Remove the old vault from the cache.
							this.vaults.Remove(oldFilename);

							// Get rid of the old name from the list
							this.vaultListComboBox.Items.Remove(oldName);

							// If we renamed something to main vault we need to remove it,
							// since the list always contains Main Vault.
							if (newName == "Main Vault")
							{
								this.vaults.Remove(newFilename);
								this.vaultListComboBox.Items.Remove(newName);
							}

							// Add the new name to the list
							this.vaultListComboBox.Items.Add(newName);

							// Select the new name
							this.vaultListComboBox.SelectedItem = newName;

							// Load the new file.
							this.LoadVault(newName, false);
							handled = true;
						}
					}

					if ((newName == null && oldName == null) || !handled)
					{
						// put the vault back to what it was
						if (this.vaultPanel.Player != null)
						{
							this.vaultListComboBox.SelectedItem = this.vaultPanel.Player.PlayerName;
						}
					}
				}
				else
				{
					// put the vault back to what it was
					if (this.vaultPanel.Player != null)
					{
						this.vaultListComboBox.SelectedItem = this.vaultPanel.Player.PlayerName;
					}
				}
			}
			catch (IOException exception)
			{
				MessageBox.Show(exception.ToString(), Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, RightToLeftOptions);
			}
		}

		/// <summary>
		/// Attempts to save all modified player files
		/// </summary>
		/// <returns>True if there were any modified player files.</returns>
		private bool SaveAllModifiedPlayers()
		{
			int numModified = 0;

			// Save each player as necessary
			foreach (KeyValuePair<string, PlayerCollection> kvp in this.players)
			{
				string playerFile = kvp.Key;
				PlayerCollection player = kvp.Value;

				if (player == null)
				{
					continue;
				}

				if (player.IsModified)
				{
					++numModified;
					bool done = false;

					// backup the file
					while (!done)
					{
						try
						{
							TQData.BackupFile(player.PlayerName, playerFile);
							TQData.BackupStupidPlayerBackupFolder(playerFile);
							player.Save(playerFile);
							done = true;
						}
						catch (IOException exception)
						{
							string title = string.Format(CultureInfo.InvariantCulture, Resources.MainFormSaveError, player.PlayerName);
							switch (MessageBox.Show(exception.ToString(), title, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, RightToLeftOptions))
							{
								case DialogResult.Abort:
									{
										// rethrow the exception
										throw;
									}

								case DialogResult.Retry:
									{
										// retry
										break;
									}

								case DialogResult.Ignore:
									{
										done = true;
										break;
									}
							}
						}
					}
				}
			}

			return numModified > 0;
		}

		/// <summary>
		/// Attempts to save all modified vault files
		/// </summary>
		private void SaveAllModifiedVaults()
		{
			foreach (KeyValuePair<string, PlayerCollection> kvp in this.vaults)
			{
				string vaultFile = kvp.Key;
				PlayerCollection vault = kvp.Value;

				if (vault == null)
				{
					continue;
				}

				if (vault.IsModified)
				{
					bool done = false;

					// backup the file
					while (!done)
					{
						try
						{
							TQData.BackupFile(vault.PlayerName, vaultFile);
							vault.Save(vaultFile);
							done = true;
						}
						catch (IOException exception)
						{
							string title = string.Format(CultureInfo.InvariantCulture, Resources.MainFormSaveError, vault.PlayerName);
							switch (MessageBox.Show(exception.ToString(), title, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, RightToLeftOptions))
							{
								case DialogResult.Abort:
									{
										// rethrow the exception
										throw;
									}

								case DialogResult.Retry:
									{
										// retry
										break;
									}

								case DialogResult.Ignore:
									{
										done = true;
										break;
									}
							}
						}
					}
				}
			}

			return;
		}

		/// <summary>
		/// Attempts to save all modified stash files.
		/// </summary>
		private void SaveAllModifiedStashes()
		{
			// Save each stash as necessary
			foreach (KeyValuePair<string, Stash> kvp in this.stashes)
			{
				string stashFile = kvp.Key;
				Stash stash = kvp.Value;

				if (stash == null)
				{
					continue;
				}

				if (stash.IsModified)
				{
					bool done = false;

					// backup the file
					while (!done)
					{
						try
						{
							TQData.BackupFile(stash.PlayerName, stashFile);
							stash.Save(stashFile);
							done = true;
						}
						catch (IOException exception)
						{
							string title = string.Format(CultureInfo.InvariantCulture, Resources.MainFormSaveError, stash.PlayerName);
							switch (MessageBox.Show(exception.ToString(), title, MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, RightToLeftOptions))
							{
								case DialogResult.Abort:
									{
										// rethrow the exception
										throw;
									}

								case DialogResult.Retry:
									{
										// retry
										break;
									}

								case DialogResult.Ignore:
									{
										done = true;
										break;
									}
							}
						}
					}
				}
			}

			return;
		}

		/// <summary>
		/// Attempts to save all modified files.
		/// </summary>
		/// <returns>true if players have been modified</returns>
		private bool SaveAllModifiedFiles()
		{
			bool playersModified = this.SaveAllModifiedPlayers();
			this.SaveAllModifiedVaults();
			this.SaveAllModifiedStashes();
			return playersModified;
		}

		/// <summary>
		/// Handler for changing the vault list drop down selection
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void VaultListComboBoxSelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (this.vaultListComboBox.SelectedIndex == 0)
				{
					this.MaintainVaultFiles();
				}
				else
				{
					string vaultName = this.vaultListComboBox.SelectedItem.ToString();
					if (this.vaultPanel.Player == null || !vaultName.Equals(this.vaultPanel.Player.PlayerName))
					{
						this.LoadVault(vaultName, false);
					}
				}

				if (this.showSecondaryVault)
				{
					this.GetSecondaryVaultList();
				}
			}
			catch (IOException exception)
			{
				MessageBox.Show(exception.ToString(), Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, RightToLeftOptions);

				// put the vault back to what it was
				if (this.vaultPanel.Player != null)
				{
					this.vaultListComboBox.SelectedItem = this.vaultPanel.Player.PlayerName;
				}
			}
		}

		/// <summary>
		/// Handler for closing the main form
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">CancelEventArgs data</param>
		private void MainFormClosing(object sender, CancelEventArgs e)
		{
			if (this.DoCloseStuff())
			{
				e.Cancel = false;
			}
			else
			{
				e.Cancel = true;
			}
		}

		/// <summary>
		/// Handler for loading the main form
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void MainFormLoad(object sender, EventArgs e)
		{
			this.backgroundWorker1.RunWorkerAsync();
		}

		/// <summary>
		/// Tooltip callback
		/// </summary>
		/// <param name="windowHandle">handle of the main window form</param>
		/// <returns>tooltip string</returns>
		private string ToolTipCallback(int windowHandle)
		{
			string answer;

			answer = this.vaultPanel.ToolTipCallback(windowHandle);
			if (answer != null)
			{
				return answer;
			}

			answer = this.playerPanel.ToolTipCallback(windowHandle);
			if (answer != null)
			{
				return answer;
			}

			answer = this.stashPanel.ToolTipCallback(windowHandle);
			if (answer != null)
			{
				return answer;
			}

			answer = this.secondaryVaultPanel.ToolTipCallback(windowHandle);
			if (answer != null)
			{
				return answer;
			}

			// Changed by VillageIdiot
			// If we are dragging something around, clear the tooltip and text box.
			if (this.dragInfo.IsActive)
			{
				this.itemText.Text = string.Empty;
				this.tooltipText = null;
				this.tooltip.ChangeText(this.tooltipText);
				return null;
			}

			// If nothing else returned a tooltip then display the current item text
			return this.tooltipText;
		}

		/// <summary>
		/// Updates configuration settings
		/// </summary>
		private void SaveConfiguration()
		{
			// Update last loaded vault
			if (Settings.Default.LoadLastVault)
			{
				// Changed by VillageIdiot
				// Now check to see if the value is changed since the Main Vault would never auto load
				if (this.vaultListComboBox.SelectedItem != null && this.vaultListComboBox.SelectedItem.ToString().ToUpperInvariant() != Settings.Default.LastVaultName.ToUpperInvariant())
				{
					Settings.Default.LastVaultName = this.vaultListComboBox.SelectedItem.ToString();
					this.configChanged = true;
				}
			}

			// Update last loaded character
			if (Settings.Default.LoadLastCharacter)
			{
				// Changed by VillageIdiot
				// Now check the last value to see if it has changed since the logic would
				// always load a character even if no character was selected on the last run
				if (this.characterComboBox.SelectedItem.ToString().ToUpperInvariant() != Settings.Default.LastCharacterName.ToUpperInvariant())
				{
					// Clear the value if no character is selected
					string name = this.characterComboBox.SelectedItem.ToString();
					if (name == Resources.MainFormSelectCharacter)
					{
						name = string.Empty;
					}

					Settings.Default.LastCharacterName = name;
					this.configChanged = true;
				}
			}

			// Update custom map settings
			if (Settings.Default.ModEnabled)
			{
				this.configChanged = true;
			}

			// Clear out the key if we are autodetecting.
			if (Settings.Default.AutoDetectLanguage)
			{
				Settings.Default.TQLanguage = string.Empty;
			}

			// Clear out the settings if auto detecting.
			if (Settings.Default.AutoDetectGamePath)
			{
				Settings.Default.TQITPath = string.Empty;
				Settings.Default.TQPath = string.Empty;
			}

			if (Database.DB.Scale != 1.0F)
			{
				Settings.Default.Scale = Database.DB.Scale;
				this.configChanged = true;
			}

			if (this.configChanged)
			{
				Settings.Default.Save();
			}
		}

		/// <summary>
		/// Handler for clicking the configure button.
		/// Shows the Settings Dialog.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void ConfigureButtonClick(object sender, EventArgs e)
		{
			SettingsDialog settingsDialog = new SettingsDialog();
			DialogResult result = DialogResult.Cancel;
			settingsDialog.Scale(new SizeF(Database.DB.Scale, Database.DB.Scale));

			string title = string.Empty;
			string message = string.Empty;
			if (settingsDialog.ShowDialog() == DialogResult.OK && settingsDialog.ConfigurationChanged)
			{
				if (settingsDialog.PlayerFilterChanged)
				{
					this.characterComboBox.SelectedItem = Resources.MainFormSelectCharacter;
					if (this.playerPanel.Player != null)
					{
						this.playerPanel.Player = null;
					}

					this.GetPlayerList();
				}

				if (settingsDialog.VaultPathChanged)
				{
					TQData.TQVaultSaveFolder = settingsDialog.VaultPath;
					UpdateVaultPath(settingsDialog.VaultPath);
					this.GetVaultList(true);
				}

				if (settingsDialog.LanguageChanged || settingsDialog.GamePathChanged || settingsDialog.CustomMapsChanged || settingsDialog.UISettingChanged)
				{
					if ((settingsDialog.GamePathChanged && settingsDialog.LanguageChanged) || settingsDialog.UISettingChanged)
					{
						title = Resources.MainFormSettingsChanged;
						message = Resources.MainFormSettingsChangedMsg;
					}
					else if (settingsDialog.GamePathChanged)
					{
						title = Resources.MainFormPathsChanged;
						message = Resources.MainFormPathsChangedMsg;
					}
					else if (settingsDialog.CustomMapsChanged)
					{
						title = Resources.MainFormMapsChanged;
						message = Resources.MainFormMapsChangedMsg;
					}
					else
					{
						title = Resources.MainFormLanguageChanged;
						message = Resources.MainFormLanguageChangedMsg;
					}

					result = MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, RightToLeftOptions);
				}

				this.configChanged = true;
				this.SaveConfiguration();
				if (result == DialogResult.Yes)
				{
					if (this.DoCloseStuff())
					{
						Application.Restart();
					}
				}
			}
		}

		/// <summary>
		/// Handler for key presses on the main form
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">KeyPressEventArgs data</param>
		private void MainFormKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar != (char)27)
			{
				e.Handled = true;
			}
		}

		/// <summary>
		/// Handler for clicking the panel selection button.
		/// Switches between the player panel and seconday vault panel.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs strucure</param>
		private void PanelSelectButtonClick(object sender, EventArgs e)
		{
			this.showSecondaryVault = !this.showSecondaryVault;
			this.UpdateTopPanel();
		}

		/// <summary>
		/// Handler for changing the secondary vault list drop down selection.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void SecondaryVaultListComboBoxSelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (this.secondaryVaultListComboBox.SelectedIndex == 0)
				{
					// Clear the vault panel.
					if (this.secondaryVaultPanel.Player != null)
					{
						this.secondaryVaultPanel.Player = null;
					}
				}
				else
				{
					string vaultName = this.secondaryVaultListComboBox.SelectedItem.ToString();
					if (this.secondaryVaultPanel.Player == null || !vaultName.Equals(this.secondaryVaultPanel.Player.PlayerName))
					{
						this.LoadVault(vaultName, true);
					}
				}
			}
			catch (IOException exception)
			{
				MessageBox.Show(exception.ToString(), Resources.GlobalError, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, RightToLeftOptions);

				// put the vault back to what it was
				if (this.secondaryVaultPanel.Player != null)
				{
					this.secondaryVaultListComboBox.SelectedItem = this.secondaryVaultPanel.Player.PlayerName;
				}
			}
		}

		/// <summary>
		/// Handler for keypresses within the search text box.
		/// Used to handle the resizing hot keys.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">KeyEventArgs data</param>
		private void SearchTextBoxKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == (Keys.Control | Keys.Add) || e.KeyData == (Keys.Control | Keys.Oemplus))
			{
				this.ResizeFormCallback(this, new ResizeEventArgs(0.1F));
			}

			if (e.KeyData == (Keys.Control | Keys.Subtract) || e.KeyData == (Keys.Control | Keys.OemMinus))
			{
				this.ResizeFormCallback(this, new ResizeEventArgs(-0.1F));
			}

			if (e.KeyData == (Keys.Control | Keys.Home))
			{
				this.ResizeFormCallback(this, new ResizeEventArgs(1.0F));
			}
		}

		/// <summary>
		/// Searches loaded files based on the specified search string.  Internally normalized to UpperInvariant
		/// </summary>
		/// <param name="searchString">string that we are searching for</param>
		private void Search(string searchString)
		{
			if (searchString == null || searchString.Trim().Count() == 0)
			{
				return;
			}

			var filter = GetFilterFrom(searchString);
			var results = new List<Result>();
			this.SearchFiles(filter, results);

			if (results.Count < 1)
			{
				MessageBox.Show(
					string.Format(CultureInfo.CurrentCulture, Resources.MainFormNoItemsFound, searchString),
					Resources.MainFormNoItemsFound2,
					MessageBoxButtons.OK,
					MessageBoxIcon.Information,
					MessageBoxDefaultButton.Button1,
					RightToLeftOptions);

				return;
			}

			// Display a dialog with the results.
			ResultsDialog dlg = new ResultsDialog();
			dlg.ResultChanged += new ResultsDialog.EventHandler<ResultChangedEventArgs>(this.SelectResult);
			dlg.ResultsList.Clear();
			dlg.ResultsList.AddRange(results);
			dlg.SearchString = searchString;
			////dlg.ShowDialog();
			dlg.Show();
		}

		private static IItemPredicate GetFilterFrom(string searchString)
		{
			var predicates = new List<IItemPredicate>();
			searchString = searchString.Trim();

			var TOKENS = "@#$".ToCharArray();
			int fromIndex = 0;
			int toIndex;
			do
			{
				string term;

				toIndex = searchString.IndexOfAny(TOKENS, fromIndex + 1);
				if (toIndex < 0)
				{
					term = searchString.Substring(fromIndex);
				}
				else
				{
					term = searchString.Substring(fromIndex, toIndex - fromIndex);
					fromIndex = toIndex;
				}

				switch (term[0])
				{
					case '@':
						predicates.Add(GetPredicateFrom(term.Substring(1), it => new ItemTypePredicate(it)));
						break;
					case '#':
						predicates.Add(GetPredicateFrom(term.Substring(1), it => new ItemAttributePredicate(it)));
						break;
					case '$':
						predicates.Add(GetPredicateFrom(term.Substring(1), it => new ItemQualityPredicate(it)));
						break;
					default:
						foreach (var name in term.Split('&'))
						{
							predicates.Add(GetPredicateFrom(name, it => new ItemNamePredicate(it)));
						}
						break;
				}
			} while (toIndex >= 0);

			return new ItemAndPredicate(predicates);
		}

		private static IItemPredicate GetPredicateFrom(string term, Func<string, IItemPredicate> newPredicate)
		{
			var predicates = term.Split('|')
				.Select(it => it.Trim())
				.Where(it => it.Count() > 0)
				.Select(it => newPredicate(it));

			switch (predicates.Count())
			{
				case 0:
					return new ItemTruePredicate();
				case 1:
					return predicates.First();
				default:
					return new ItemOrPredicate(predicates);
			}
		}

		/// <summary>
		/// Selects the item highlighted in the results list.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">ResultChangedEventArgs data</param>
		private void SelectResult(object sender, ResultChangedEventArgs e)
		{
			Result selectedResult = e.Result;
			if (selectedResult == null || selectedResult.Item == null)
			{
				return;
			}

			this.ClearAllItemsSelectedCallback(this, new SackPanelEventArgs(null, null));

			if (selectedResult.SackType == SackType.Vault)
			{
				// Switch to the selected vault
				this.vaultListComboBox.SelectedItem = selectedResult.ContainerName;
				this.vaultPanel.CurrentBag = selectedResult.SackNumber;
				this.vaultPanel.SackPanel.SelectItem(selectedResult.Item.Location);
			}
			else if (selectedResult.SackType == SackType.Player || selectedResult.SackType == SackType.Equipment)
			{
				// Switch to the selected player
				if (this.showSecondaryVault)
				{
					this.showSecondaryVault = !this.showSecondaryVault;
					this.UpdateTopPanel();
				}

				string myName = selectedResult.ContainerName;

				if (TQData.IsCustom)
				{
					myName = string.Concat(myName, "<Custom Map>");
				}

				// Update the selection list and load the character.
				this.characterComboBox.SelectedItem = myName;
				if (selectedResult.SackNumber > 0)
				{
					this.playerPanel.CurrentBag = selectedResult.SackNumber - 1;
				}

				if (selectedResult.SackType != SackType.Equipment)
				{
					// Highlight the item if it's in the player inventory.
					if (selectedResult.SackNumber == 0)
					{
						this.playerPanel.SackPanel.SelectItem(selectedResult.Item.Location);
					}
					else
					{
						this.playerPanel.BagSackPanel.SelectItem(selectedResult.Item.Location);
					}
				}
			}
			else if (selectedResult.SackType == SackType.Stash)
			{
				// Switch to the selected player
				if (this.showSecondaryVault)
				{
					this.showSecondaryVault = !this.showSecondaryVault;
					this.UpdateTopPanel();
				}

				// Assume that only IT characters can have a stash.
				string myName = string.Concat(selectedResult.ContainerName, "<Immortal Throne>");

				if (TQData.IsCustom)
				{
					myName = string.Concat(myName, "<Custom Map>");
				}

				// Update the selection list and load the character.
				this.characterComboBox.SelectedItem = myName;

				// Switch to the Stash bag
				this.stashPanel.CurrentBag = selectedResult.SackNumber;
				this.stashPanel.SackPanel.SelectItem(selectedResult.Item.Location);
			}
			else if ((selectedResult.SackType == SackType.TransferStash) || (selectedResult.SackType == SackType.RelicVaultStash))
			{
				// Switch to the Stash bag
				this.stashPanel.CurrentBag = selectedResult.SackNumber;
				this.stashPanel.SackPanel.SelectItem(selectedResult.Item.Location);
			}
		}

		/// <summary>
		/// Searches all loaded vault files
		/// </summary>
		/// <param name="predicate">Predicate that the items should match</param>
		/// <param name="quality">Quality filter</param>
		/// <param name="searchByType">flag for whether we are searching by type or name</param>
		/// <param name="results">List holding the search results.</param>
		private void SearchVaults(IItemPredicate predicate, List<Result> results)
		{
			if (this.vaults == null || this.vaults.Count == 0)
			{
				return;
			}

			foreach (KeyValuePair<string, PlayerCollection> kvp in this.vaults)
			{
				string vaultFile = kvp.Key;
				PlayerCollection vault = kvp.Value;

				if (vault == null)
				{
					TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "vaultFile={0} returned null vault.", vaultFile));
					continue;
				}

				int vaultNumber = -1;
				foreach (SackCollection sack in vault)
				{
					vaultNumber++;
					if (sack == null)
					{
						TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "vaultFile={0}", vaultFile));
						TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "sack({0}) returned null.", vaultNumber));
						continue;
					}

					// Query the sack for the items containing the search string.
					foreach (Item item in QuerySack(predicate, sack))
					{
						results.Add(new Result(
							vaultFile,
							Path.GetFileNameWithoutExtension(vaultFile),
							vaultNumber,
							SackType.Vault,
							item
						));
					}
				}
			}
		}

		/// <summary>
		/// Searches all loaded player files
		/// </summary>
		/// <param name="predicate">Predicate that the items should match</param>
		/// <param name="quality">Quality filter</param>
		/// <param name="searchByType">flag for whether we are searching by type or name</param>
		/// <param name="results">List holding the search results.</param>
		private void SearchPlayers(IItemPredicate predicate, List<Result> results)
		{
			if (this.players == null || this.players.Count == 0)
			{
				return;
			}

			foreach (KeyValuePair<string, PlayerCollection> kvp in this.players)
			{
				string playerFile = kvp.Key;
				PlayerCollection player = kvp.Value;

				if (player == null)
				{
					// Make sure the name is valid and we have a player.
					TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "playerFile={0} returned null player.", playerFile));
					continue;
				}

				string playerName = GetNameFromFile(playerFile);
				if (playerName == null)
				{
					TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "playerFile={0} returned null playerName.", playerFile));
					continue;
				}

				int sackNumber = -1;
				foreach (SackCollection sack in player)
				{
					sackNumber++;
					if (sack == null)
					{
						TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "playerFile={0}", playerFile));
						TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "sack({0}) returned null.", sackNumber));
						continue;
					}

					// Query the sack for the items containing the search string.
					foreach (Item item in QuerySack(predicate, sack))
					{
						results.Add(new Result(
							playerFile,
							playerName,
							sackNumber,
							SackType.Player,
							item
						));
					}
				}

				// Now search the Equipment panel
				SackCollection equipmentSack = player.EquipmentSack;
				if (equipmentSack == null)
				{
					TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "playerFile={0} Equipment Sack returned null.", playerFile));
					continue;
				}

				foreach (Item item in QuerySack(predicate, equipmentSack))
				{
					results.Add(new Result(
						playerFile,
						playerName,
						0,
						SackType.Equipment,
						item
					));
				}
			}
		}

		/// <summary>
		/// Searches all loaded stashes including transfer stash.
		/// </summary>
		/// <param name="predicate">Predicate that the items should match</param>
		/// <param name="results">List holding the search results.</param>
		private void SearchStashes(IItemPredicate predicate, List<Result> results)
		{
			if (this.stashes == null || this.stashes.Count == 0)
			{
				return;
			}

			foreach (KeyValuePair<string, Stash> kvp in this.stashes)
			{
				string stashFile = kvp.Key;
				Stash stash = kvp.Value;

				// Make sure we have a valid name and stash.
				if (stash == null)
				{
					TQVaultData.TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "stashFile={0} returned null stash.", stashFile));
					continue;
				}

				string stashName = GetNameFromFile(stashFile);
				if (stashName == null)
				{
					TQVaultData.TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "stashFile={0} returned null stashName.", stashFile));
					continue;
				}

				SackCollection sack = stash.Sack;
				if (sack == null)
				{
					TQVaultData.TQDebug.DebugWriteLine(string.Format(CultureInfo.InvariantCulture, "stashFile={0} returned null sack.", stashFile));
					continue;
				}

				int sackNumber = 2;
				SackType sackType = SackType.Stash;
				if (stashName == Resources.GlobalTransferStash)
				{
					sackNumber = 1;
					sackType = SackType.TransferStash;
				}
				else if (stashName == Resources.GlobalRelicVaultStash)
				{
					sackNumber = 3;
					sackType = SackType.RelicVaultStash;
				}

				foreach (Item item in QuerySack(predicate, sack))
				{
					results.Add(new Result(
						stashFile,
						stashName,
						sackNumber,
						sackType,
						item
					));
				}
			}
		}

		/// <summary>
		/// Searches all loaded files
		/// </summary>
		/// <param name="predicate">Predicate that the items should match</param>
		/// <param name="results">List holding the search results.</param>
		private void SearchFiles(IItemPredicate predicate, List<Result> results)
		{
			this.SearchVaults(predicate, results);
			this.SearchPlayers(predicate, results);
			this.SearchStashes(predicate, results);
		}

		/// <summary>
		/// Handler for showing the main form.
		/// Used to switch focus to the search text box.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void MainFormShown(object sender, EventArgs e)
		{
			this.vaultPanel.SackPanel.Focus();
		}

		/// <summary>
		/// Handler for moving the mouse wheel.
		/// Used to scroll through the vault list.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">MouseEventArgs data</param>
		private void MainFormMouseWheel(object sender, MouseEventArgs e)
		{
			// Force a single line regardless of the delta value.
			int numberOfTextLinesToMove = ((e.Delta > 0) ? 1 : 0) - ((e.Delta < 0) ? 1 : 0);
			if (numberOfTextLinesToMove != 0)
			{
				int vaultSelection = this.vaultListComboBox.SelectedIndex;
				vaultSelection -= numberOfTextLinesToMove;
				if (vaultSelection < 1)
				{
					vaultSelection = 1;
				}

				if (vaultSelection >= this.vaultListComboBox.Items.Count)
				{
					vaultSelection = this.vaultListComboBox.Items.Count - 1;
				}

				this.vaultListComboBox.SelectedIndex = vaultSelection;
			}
		}

		/// <summary>
		/// Handler for clicking the about button.
		/// Shows the about dialog box.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void AboutButtonClick(object sender, EventArgs e)
		{
			AboutBox dlg = new AboutBox();
			dlg.Scale(new SizeF(Database.DB.Scale, Database.DB.Scale));
			dlg.ShowDialog();
		}

		/// <summary>
		/// Key Handler for the main form.  Most keystrokes should be handled by the individual panels or the search text box.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">KeyEventArgs data</param>
		private void MainFormKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == (Keys.Control | Keys.F))
			{
				this.ActivateSearchCallback(this, new SackPanelEventArgs(null, null));
			}

			if (e.KeyData == (Keys.Control | Keys.Add) || e.KeyData == (Keys.Control | Keys.Oemplus))
			{
				this.ResizeFormCallback(this, new ResizeEventArgs(0.1F));
			}

			if (e.KeyData == (Keys.Control | Keys.Subtract) || e.KeyData == (Keys.Control | Keys.OemMinus))
			{
				this.ResizeFormCallback(this, new ResizeEventArgs(-0.1F));
			}

			if (e.KeyData == (Keys.Control | Keys.Home))
			{
				this.ResizeFormCallback(this, new ResizeEventArgs(1.0F));
			}
		}

		/// <summary>
		/// Handler for clicking the search button on the form.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void SearchButtonClick(object sender, EventArgs e)
		{
			this.OpenSearchDialog();
		}

		/// <summary>
		/// Opens a scaled SearchDialog box and calls Search().
		/// </summary>
		private void OpenSearchDialog()
		{
			SearchDialog searchDialog = new SearchDialog();
			searchDialog.Scale(new SizeF(Database.DB.Scale, Database.DB.Scale));

			if (searchDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(searchDialog.SearchText))
			{
				this.Search(searchDialog.SearchText);
			}
		}

		/// <summary>
		/// Handles Timer ticks for fading in the main form.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void FadeInTimerTick(object sender, EventArgs e)
		{
			if (this.Opacity < 1)
			{
				this.Opacity = Math.Min(1.0F, this.Opacity + this.fadeInterval);
			}
			else
			{
				this.fadeInTimer.Stop();
			}
		}
	}
}
