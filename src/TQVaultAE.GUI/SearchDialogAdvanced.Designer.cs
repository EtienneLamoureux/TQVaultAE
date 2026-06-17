using TQVaultAE.GUI.Components;

namespace TQVaultAE.GUI
{
	/// <summary>
	/// SearchDialog Designer class
	/// </summary>
	public partial class SearchDialogAdvanced
	{
		private ScalingLabel scalingLabelCharacters;

		/// <summary>
		/// Windows Form Find Button.
		/// </summary>
		private ScalingButton applyButton;

		/// <summary>
		/// Windows Form Cancel Button.
		/// </summary>
		private ScalingButton cancelButton;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (this.components != null))
			{
				this.components.Dispose();
			}

			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SearchDialogAdvanced));
			scalingLabelCharacters = new ScalingLabel();
			applyButton = new ScalingButton();
			cancelButton = new ScalingButton();
			vaultProgressBar = new VaultProgressBar();
			scalingLabelProgress = new ScalingLabel();
			toolTip = new ToolTip(components);
			tableLayoutPanelBottom = new TableLayoutPanel();
			scalingLabelProgressPanelAlignText = new Panel();
			scalingCheckedListBoxCharacters = new ScalingCheckedListBox();
			flowLayoutPanelCharacters = new FlowLayoutPanel();
			flowLayoutPanelItemType = new FlowLayoutPanel();
			scalingLabelItemType = new ScalingLabel();
			scalingCheckedListBoxItemType = new ScalingCheckedListBox();
			flowLayoutPanelItemAttributes = new FlowLayoutPanel();
			scalingLabelItemAttributes = new ScalingLabel();
			scalingCheckedListBoxItemAttributes = new ScalingCheckedListBox();
			flowLayoutPanelRarity = new FlowLayoutPanel();
			scalingLabelRarity = new ScalingLabel();
			scalingCheckedListBoxRarity = new ScalingCheckedListBox();
			flowLayoutPanelPrefixAttributes = new FlowLayoutPanel();
			scalingLabelPrefixAttributes = new ScalingLabel();
			scalingCheckedListBoxPrefixAttributes = new ScalingCheckedListBox();
			flowLayoutPanelSuffixAttributes = new FlowLayoutPanel();
			scalingLabelSuffixAttributes = new ScalingLabel();
			scalingCheckedListBoxSuffixAttributes = new ScalingCheckedListBox();
			flowLayoutPanelBaseAttributes = new FlowLayoutPanel();
			scalingLabelBaseAttributes = new ScalingLabel();
			scalingCheckedListBoxBaseAttributes = new ScalingCheckedListBox();
			flowLayoutPanelMain = new FlowLayoutPanel();
			flowLayoutPanelInVaults = new FlowLayoutPanel();
			scalingLabelInVaults = new ScalingLabel();
			scalingCheckedListBoxVaults = new ScalingCheckedListBox();
			flowLayoutPanelOrigin = new FlowLayoutPanel();
			scalingLabelOrigin = new ScalingLabel();
			scalingCheckedListBoxOrigin = new ScalingCheckedListBox();
			flowLayoutPanelQuality = new FlowLayoutPanel();
			scalingLabelQuality = new ScalingLabel();
			scalingCheckedListBoxQuality = new ScalingCheckedListBox();
			flowLayoutPanelStyle = new FlowLayoutPanel();
			scalingLabelStyle = new ScalingLabel();
			scalingCheckedListBoxStyle = new ScalingCheckedListBox();
			flowLayoutPanelSetItems = new FlowLayoutPanel();
			scalingLabelSetItems = new ScalingLabel();
			scalingCheckedListBoxSetItems = new ScalingCheckedListBox();
			flowLayoutPanelWithCharm = new FlowLayoutPanel();
			scalingLabelWithCharm = new ScalingLabel();
			scalingCheckedListBoxWithCharm = new ScalingCheckedListBox();
			flowLayoutPanelWithRelic = new FlowLayoutPanel();
			scalingLabelWithRelic = new ScalingLabel();
			scalingCheckedListBoxWithRelic = new ScalingCheckedListBox();
			flowLayoutPanelPrefixName = new FlowLayoutPanel();
			scalingLabelPrefixName = new ScalingLabel();
			scalingCheckedListBoxPrefixName = new ScalingCheckedListBox();
			flowLayoutPanelSuffixName = new FlowLayoutPanel();
			scalingLabelSuffixName = new ScalingLabel();
			scalingCheckedListBoxSuffixName = new ScalingCheckedListBox();
			flowLayoutPanelSearchTerm = new FlowLayoutPanel();
			scalingLabelSearchTerm = new ScalingLabel();
			scalingTextBoxSearchTerm = new ScalingTextBox();
			scalingButtonReset = new ScalingButton();
			flowLayoutPanelQueries = new FlowLayoutPanel();
			scalingLabelQueries = new ScalingLabel();
			scalingComboBoxQueryList = new ScalingComboBox();
			scalingButtonQuerySave = new ScalingButton();
			scalingButtonQueryDelete = new ScalingButton();
			tableLayoutPanelContent = new TableLayoutPanel();
			flowLayoutPanelMainAutoScroll = new FlowLayoutPanel();
			bufferedFlowLayoutPanelQuickFilters = new BufferedFlowLayoutPanel();
			scalingCheckBoxHavingPrefix = new ScalingCheckBox();
			scalingCheckBoxHavingSuffix = new ScalingCheckBox();
			scalingCheckBoxHavingRelic = new ScalingCheckBox();
			scalingCheckBoxHavingCharm = new ScalingCheckBox();
			scalingCheckBoxIsSetItem = new ScalingCheckBox();
			flowLayoutPanelMisc = new FlowLayoutPanel();
			scalingLabelMaxVisibleElement = new ScalingLabel();
			numericUpDownMaxElement = new NumericUpDown();
			scalingCheckBoxReduceDuringSelection = new ScalingCheckBox();
			buttonCollapseAll = new Button();
			buttonExpandAll = new Button();
			scalingLabelOperator = new ScalingLabel();
			scalingComboBoxOperator = new ScalingComboBox();
			scalingLabelFiltersSelected = new ScalingLabel();
			scalingLabelFilterCategories = new ScalingLabel();
			scalingTextBoxFilterCategories = new ScalingTextBox();
			bufferedFlowLayoutPanelRequierements = new BufferedFlowLayoutPanel();
			tableLayoutPanelRequierements = new TableLayoutPanel();
			scalingCheckBoxMinReq = new ScalingCheckBox();
			flowLayoutPanelMin = new FlowLayoutPanel();
			scalingLabelMinLvl = new ScalingLabel();
			numericUpDownMinLvl = new NumericUpDown();
			scalingLabelMinStr = new ScalingLabel();
			numericUpDownMinStr = new NumericUpDown();
			scalingLabelMinDex = new ScalingLabel();
			numericUpDownMinDex = new NumericUpDown();
			scalingLabelMinInt = new ScalingLabel();
			numericUpDownMinInt = new NumericUpDown();
			scalingCheckBoxMaxReq = new ScalingCheckBox();
			flowLayoutPanelMax = new FlowLayoutPanel();
			scalingLabelMaxLvl = new ScalingLabel();
			numericUpDownMaxLvl = new NumericUpDown();
			scalingLabelMaxStr = new ScalingLabel();
			numericUpDownMaxStr = new NumericUpDown();
			scalingLabelMaxDex = new ScalingLabel();
			numericUpDownMaxDex = new NumericUpDown();
			scalingLabelMaxInt = new ScalingLabel();
			numericUpDownMaxInt = new NumericUpDown();
			flowLayoutPanelLeftColumn = new FlowLayoutPanel();
			scalingButtonMenuCharacters = new ScalingButton();
			scalingButtonMenuVaults = new ScalingButton();
			scalingButtonMenuType = new ScalingButton();
			scalingButtonMenuRarity = new ScalingButton();
			scalingButtonMenuOrigin = new ScalingButton();
			scalingButtonMenuQuality = new ScalingButton();
			scalingButtonMenuStyle = new ScalingButton();
			scalingButtonMenuSetItems = new ScalingButton();
			scalingButtonMenuWithCharm = new ScalingButton();
			scalingButtonMenuWithRelic = new ScalingButton();
			scalingButtonMenuAttribute = new ScalingButton();
			scalingButtonMenuPrefixName = new ScalingButton();
			scalingButtonMenuPrefixAttribute = new ScalingButton();
			scalingButtonMenuBaseAttribute = new ScalingButton();
			scalingButtonMenuSuffixName = new ScalingButton();
			scalingButtonMenuSuffixAttribute = new ScalingButton();
			backgroundWorkerBuildDB = new System.ComponentModel.BackgroundWorker();
			typeAssistantSearchBox = new TypeAssistant();
			typeAssistantFilterCategories = new TypeAssistant();
			tableLayoutPanelBottom.SuspendLayout();
			scalingLabelProgressPanelAlignText.SuspendLayout();
			flowLayoutPanelCharacters.SuspendLayout();
			flowLayoutPanelItemType.SuspendLayout();
			flowLayoutPanelItemAttributes.SuspendLayout();
			flowLayoutPanelRarity.SuspendLayout();
			flowLayoutPanelPrefixAttributes.SuspendLayout();
			flowLayoutPanelSuffixAttributes.SuspendLayout();
			flowLayoutPanelBaseAttributes.SuspendLayout();
			flowLayoutPanelMain.SuspendLayout();
			flowLayoutPanelInVaults.SuspendLayout();
			flowLayoutPanelOrigin.SuspendLayout();
			flowLayoutPanelQuality.SuspendLayout();
			flowLayoutPanelStyle.SuspendLayout();
			flowLayoutPanelSetItems.SuspendLayout();
			flowLayoutPanelWithCharm.SuspendLayout();
			flowLayoutPanelWithRelic.SuspendLayout();
			flowLayoutPanelPrefixName.SuspendLayout();
			flowLayoutPanelSuffixName.SuspendLayout();
			flowLayoutPanelSearchTerm.SuspendLayout();
			flowLayoutPanelQueries.SuspendLayout();
			tableLayoutPanelContent.SuspendLayout();
			flowLayoutPanelMainAutoScroll.SuspendLayout();
			bufferedFlowLayoutPanelQuickFilters.SuspendLayout();
			flowLayoutPanelMisc.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)numericUpDownMaxElement).BeginInit();
			bufferedFlowLayoutPanelRequierements.SuspendLayout();
			tableLayoutPanelRequierements.SuspendLayout();
			flowLayoutPanelMin.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)numericUpDownMinLvl).BeginInit();
			((System.ComponentModel.ISupportInitialize)numericUpDownMinStr).BeginInit();
			((System.ComponentModel.ISupportInitialize)numericUpDownMinDex).BeginInit();
			((System.ComponentModel.ISupportInitialize)numericUpDownMinInt).BeginInit();
			flowLayoutPanelMax.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)numericUpDownMaxLvl).BeginInit();
			((System.ComponentModel.ISupportInitialize)numericUpDownMaxStr).BeginInit();
			((System.ComponentModel.ISupportInitialize)numericUpDownMaxDex).BeginInit();
			((System.ComponentModel.ISupportInitialize)numericUpDownMaxInt).BeginInit();
			flowLayoutPanelLeftColumn.SuspendLayout();
			SuspendLayout();
			// 
			// scalingLabelCharacters
			// 
			scalingLabelCharacters.AutoSize = true;
			scalingLabelCharacters.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelCharacters.ForeColor = Color.Gold;
			scalingLabelCharacters.Location = new Point(4, 2);
			scalingLabelCharacters.Name = "scalingLabelCharacters";
			scalingLabelCharacters.Size = new Size(81, 18);
			scalingLabelCharacters.TabIndex = 0;
			scalingLabelCharacters.Text = "Characters";
			scalingLabelCharacters.UseMnemonic = false;
			scalingLabelCharacters.MouseClick += scalingLabelCategory_MouseClick;
			// 
			// applyButton
			// 
			applyButton.Anchor = AnchorStyles.None;
			applyButton.BackColor = Color.Transparent;
			applyButton.DownBitmap = (Bitmap)resources.GetObject("applyButton.DownBitmap");
			applyButton.FlatAppearance.BorderSize = 0;
			applyButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 51, 44, 28);
			applyButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 51, 44, 28);
			applyButton.FlatStyle = FlatStyle.Flat;
			applyButton.Font = new Font("Microsoft Sans Serif", 12F);
			applyButton.ForeColor = Color.FromArgb(51, 44, 28);
			applyButton.Image = (Image)resources.GetObject("applyButton.Image");
			applyButton.Location = new Point(34, 40);
			applyButton.Name = "applyButton";
			applyButton.OverBitmap = (Bitmap)resources.GetObject("applyButton.OverBitmap");
			applyButton.Size = new Size(137, 30);
			applyButton.SizeToGraphic = false;
			applyButton.TabIndex = 2;
			applyButton.Text = "Apply";
			applyButton.UpBitmap = (Bitmap)resources.GetObject("applyButton.UpBitmap");
			applyButton.UseCustomGraphic = true;
			applyButton.UseVisualStyleBackColor = false;
			applyButton.Click += ApplyButtonClicked;
			// 
			// cancelButton
			// 
			cancelButton.Anchor = AnchorStyles.None;
			cancelButton.BackColor = Color.Transparent;
			cancelButton.DialogResult = DialogResult.Cancel;
			cancelButton.DownBitmap = (Bitmap)resources.GetObject("cancelButton.DownBitmap");
			cancelButton.FlatAppearance.BorderSize = 0;
			cancelButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 51, 44, 28);
			cancelButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 51, 44, 28);
			cancelButton.FlatStyle = FlatStyle.Flat;
			cancelButton.Font = new Font("Microsoft Sans Serif", 12F);
			cancelButton.ForeColor = Color.FromArgb(51, 44, 28);
			cancelButton.Image = (Image)resources.GetObject("cancelButton.Image");
			cancelButton.Location = new Point(1206, 40);
			cancelButton.Name = "cancelButton";
			cancelButton.OverBitmap = (Bitmap)resources.GetObject("cancelButton.OverBitmap");
			cancelButton.Size = new Size(137, 30);
			cancelButton.SizeToGraphic = false;
			cancelButton.TabIndex = 3;
			cancelButton.Text = "Cancel";
			cancelButton.UpBitmap = (Bitmap)resources.GetObject("cancelButton.UpBitmap");
			cancelButton.UseCustomGraphic = true;
			cancelButton.UseVisualStyleBackColor = false;
			cancelButton.Click += CancelButtonClicked;
			// 
			// vaultProgressBar
			// 
			vaultProgressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			vaultProgressBar.BackColor = Color.Transparent;
			vaultProgressBar.Location = new Point(209, 39);
			vaultProgressBar.Maximum = 0;
			vaultProgressBar.Minimum = 0;
			vaultProgressBar.Name = "vaultProgressBar";
			vaultProgressBar.Size = new Size(959, 42);
			vaultProgressBar.TabIndex = 4;
			vaultProgressBar.Value = 0;
			// 
			// scalingLabelProgress
			// 
			scalingLabelProgress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			scalingLabelProgress.BackColor = Color.Transparent;
			scalingLabelProgress.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelProgress.ForeColor = Color.DarkOrange;
			scalingLabelProgress.Location = new Point(422, 1);
			scalingLabelProgress.Name = "scalingLabelProgress";
			scalingLabelProgress.Size = new Size(96, 17);
			scalingLabelProgress.TabIndex = 5;
			scalingLabelProgress.Text = "Building Data...";
			scalingLabelProgress.TextAlign = ContentAlignment.MiddleCenter;
			scalingLabelProgress.UseMnemonic = false;
			scalingLabelProgress.TextChanged += scalingLabelProgress_TextChanged;
			scalingLabelProgress.MouseEnter += scalingLabelProgress_MouseEnter;
			scalingLabelProgress.MouseLeave += scalingLabelProgress_MouseLeave;
			// 
			// tableLayoutPanelBottom
			// 
			tableLayoutPanelBottom.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			tableLayoutPanelBottom.ColumnCount = 3;
			tableLayoutPanelContent.SetColumnSpan(tableLayoutPanelBottom, 3);
			tableLayoutPanelBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
			tableLayoutPanelBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
			tableLayoutPanelBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
			tableLayoutPanelBottom.Controls.Add(vaultProgressBar, 1, 1);
			tableLayoutPanelBottom.Controls.Add(applyButton, 0, 1);
			tableLayoutPanelBottom.Controls.Add(cancelButton, 2, 1);
			tableLayoutPanelBottom.Controls.Add(scalingLabelProgressPanelAlignText, 1, 0);
			tableLayoutPanelBottom.Location = new Point(3, 748);
			tableLayoutPanelBottom.Name = "tableLayoutPanelBottom";
			tableLayoutPanelBottom.RowCount = 2;
			tableLayoutPanelBottom.RowStyles.Add(new RowStyle());
			tableLayoutPanelBottom.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
			tableLayoutPanelBottom.Size = new Size(1379, 84);
			tableLayoutPanelBottom.TabIndex = 6;
			// 
			// scalingLabelProgressPanelAlignText
			// 
			scalingLabelProgressPanelAlignText.Controls.Add(scalingLabelProgress);
			scalingLabelProgressPanelAlignText.Dock = DockStyle.Fill;
			scalingLabelProgressPanelAlignText.Location = new Point(209, 3);
			scalingLabelProgressPanelAlignText.Name = "scalingLabelProgressPanelAlignText";
			scalingLabelProgressPanelAlignText.Size = new Size(959, 20);
			scalingLabelProgressPanelAlignText.TabIndex = 6;
			scalingLabelProgressPanelAlignText.SizeChanged += scalingLabelProgressPanelAlignText_SizeChanged;
			// 
			// scalingCheckedListBoxCharacters
			// 
			scalingCheckedListBoxCharacters.BackColor = Color.FromArgb(46, 31, 21);
			scalingCheckedListBoxCharacters.BorderStyle = BorderStyle.None;
			scalingCheckedListBoxCharacters.CheckOnClick = true;
			scalingCheckedListBoxCharacters.Font = new Font("Microsoft Sans Serif", 9F);
			scalingCheckedListBoxCharacters.ForeColor = Color.White;
			scalingCheckedListBoxCharacters.FormattingEnabled = true;
			scalingCheckedListBoxCharacters.HorizontalScrollbar = true;
			scalingCheckedListBoxCharacters.IntegralHeight = false;
			scalingCheckedListBoxCharacters.Items.AddRange(new object[] { "Char Name 1", "Char Name 2", "Char Name 3 (Not loaded)" });
			scalingCheckedListBoxCharacters.Location = new Point(4, 20);
			scalingCheckedListBoxCharacters.Margin = new Padding(3, 0, 3, 0);
			scalingCheckedListBoxCharacters.MultiColumn = true;
			scalingCheckedListBoxCharacters.Name = "scalingCheckedListBoxCharacters";
			scalingCheckedListBoxCharacters.Size = new Size(215, 80);
			scalingCheckedListBoxCharacters.TabIndex = 7;
			scalingCheckedListBoxCharacters.SelectedValueChanged += scalingCheckedListBox_SelectedValueChanged;
			scalingCheckedListBoxCharacters.MouseMove += scalingCheckedListBox_MouseMove;
			// 
			// flowLayoutPanelCharacters
			// 
			flowLayoutPanelCharacters.AutoSize = true;
			flowLayoutPanelCharacters.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			flowLayoutPanelCharacters.BorderStyle = BorderStyle.FixedSingle;
			flowLayoutPanelCharacters.Controls.Add(scalingLabelCharacters);
			flowLayoutPanelCharacters.Controls.Add(scalingCheckedListBoxCharacters);
			flowLayoutPanelCharacters.FlowDirection = FlowDirection.TopDown;
			flowLayoutPanelCharacters.Location = new Point(3, 3);
			flowLayoutPanelCharacters.MinimumSize = new Size(50, 10);
			flowLayoutPanelCharacters.Name = "flowLayoutPanelCharacters";
			flowLayoutPanelCharacters.Padding = new Padding(1, 2, 0, 0);
			flowLayoutPanelCharacters.Size = new Size(224, 102);
			flowLayoutPanelCharacters.TabIndex = 8;
			// 
			// flowLayoutPanelItemType
			// 
			flowLayoutPanelItemType.AutoSize = true;
			flowLayoutPanelItemType.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			flowLayoutPanelItemType.BorderStyle = BorderStyle.FixedSingle;
			flowLayoutPanelItemType.Controls.Add(scalingLabelItemType);
			flowLayoutPanelItemType.Controls.Add(scalingCheckedListBoxItemType);
			flowLayoutPanelItemType.FlowDirection = FlowDirection.TopDown;
			flowLayoutPanelItemType.Location = new Point(463, 3);
			flowLayoutPanelItemType.MinimumSize = new Size(50, 10);
			flowLayoutPanelItemType.Name = "flowLayoutPanelItemType";
			flowLayoutPanelItemType.Padding = new Padding(1, 2, 0, 0);
			flowLayoutPanelItemType.Size = new Size(224, 102);
			flowLayoutPanelItemType.TabIndex = 9;
			// 
			// scalingLabelItemType
			// 
			scalingLabelItemType.AutoSize = true;
			scalingLabelItemType.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelItemType.ForeColor = Color.Gold;
			scalingLabelItemType.Location = new Point(4, 2);
			scalingLabelItemType.Name = "scalingLabelItemType";
			scalingLabelItemType.Size = new Size(40, 18);
			scalingLabelItemType.TabIndex = 0;
			scalingLabelItemType.Text = "Type";
			scalingLabelItemType.UseMnemonic = false;
			scalingLabelItemType.MouseClick += scalingLabelCategory_MouseClick;
			// 
			// scalingCheckedListBoxItemType
			// 
			scalingCheckedListBoxItemType.BackColor = Color.FromArgb(46, 31, 21);
			scalingCheckedListBoxItemType.BorderStyle = BorderStyle.None;
			scalingCheckedListBoxItemType.CheckOnClick = true;
			scalingCheckedListBoxItemType.Font = new Font("Microsoft Sans Serif", 9F);
			scalingCheckedListBoxItemType.ForeColor = Color.White;
			scalingCheckedListBoxItemType.FormattingEnabled = true;
			scalingCheckedListBoxItemType.HorizontalScrollbar = true;
			scalingCheckedListBoxItemType.IntegralHeight = false;
			scalingCheckedListBoxItemType.Items.AddRange(new object[] { "Sword", "Helmet", "Ring" });
			scalingCheckedListBoxItemType.Location = new Point(4, 20);
			scalingCheckedListBoxItemType.Margin = new Padding(3, 0, 3, 0);
			scalingCheckedListBoxItemType.MultiColumn = true;
			scalingCheckedListBoxItemType.Name = "scalingCheckedListBoxItemType";
			scalingCheckedListBoxItemType.Size = new Size(215, 80);
			scalingCheckedListBoxItemType.TabIndex = 7;
			scalingCheckedListBoxItemType.SelectedValueChanged += scalingCheckedListBox_SelectedValueChanged;
			scalingCheckedListBoxItemType.MouseMove += scalingCheckedListBox_MouseMove;
			// 
			// flowLayoutPanelItemAttributes
			// 
			flowLayoutPanelItemAttributes.AutoSize = true;
			flowLayoutPanelItemAttributes.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			flowLayoutPanelItemAttributes.BorderStyle = BorderStyle.FixedSingle;
			flowLayoutPanelItemAttributes.Controls.Add(scalingLabelItemAttributes);
			flowLayoutPanelItemAttributes.Controls.Add(scalingCheckedListBoxItemAttributes);
			flowLayoutPanelItemAttributes.FlowDirection = FlowDirection.TopDown;
			flowLayoutPanelItemAttributes.Location = new Point(3, 219);
			flowLayoutPanelItemAttributes.MinimumSize = new Size(50, 10);
			flowLayoutPanelItemAttributes.Name = "flowLayoutPanelItemAttributes";
			flowLayoutPanelItemAttributes.Padding = new Padding(1, 2, 0, 0);
			flowLayoutPanelItemAttributes.Size = new Size(224, 102);
			flowLayoutPanelItemAttributes.TabIndex = 10;
			// 
			// scalingLabelItemAttributes
			// 
			scalingLabelItemAttributes.AutoSize = true;
			scalingLabelItemAttributes.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelItemAttributes.ForeColor = Color.Gold;
			scalingLabelItemAttributes.Location = new Point(4, 2);
			scalingLabelItemAttributes.Name = "scalingLabelItemAttributes";
			scalingLabelItemAttributes.Size = new Size(69, 18);
			scalingLabelItemAttributes.TabIndex = 0;
			scalingLabelItemAttributes.Text = "Attributes";
			scalingLabelItemAttributes.UseMnemonic = false;
			scalingLabelItemAttributes.MouseClick += scalingLabelCategory_MouseClick;
			// 
			// scalingCheckedListBoxItemAttributes
			// 
			scalingCheckedListBoxItemAttributes.BackColor = Color.FromArgb(46, 31, 21);
			scalingCheckedListBoxItemAttributes.BorderStyle = BorderStyle.None;
			scalingCheckedListBoxItemAttributes.CheckOnClick = true;
			scalingCheckedListBoxItemAttributes.Font = new Font("Microsoft Sans Serif", 9F);
			scalingCheckedListBoxItemAttributes.ForeColor = Color.White;
			scalingCheckedListBoxItemAttributes.FormattingEnabled = true;
			scalingCheckedListBoxItemAttributes.HorizontalScrollbar = true;
			scalingCheckedListBoxItemAttributes.IntegralHeight = false;
			scalingCheckedListBoxItemAttributes.Items.AddRange(new object[] { "Physical Damage", "Elemental Resistance", "Elemental Damage" });
			scalingCheckedListBoxItemAttributes.Location = new Point(4, 20);
			scalingCheckedListBoxItemAttributes.Margin = new Padding(3, 0, 3, 0);
			scalingCheckedListBoxItemAttributes.MultiColumn = true;
			scalingCheckedListBoxItemAttributes.Name = "scalingCheckedListBoxItemAttributes";
			scalingCheckedListBoxItemAttributes.Size = new Size(215, 80);
			scalingCheckedListBoxItemAttributes.TabIndex = 7;
			scalingCheckedListBoxItemAttributes.SelectedValueChanged += scalingCheckedListBox_SelectedValueChanged;
			scalingCheckedListBoxItemAttributes.MouseMove += scalingCheckedListBox_MouseMove;
			// 
			// flowLayoutPanelRarity
			// 
			flowLayoutPanelRarity.AutoSize = true;
			flowLayoutPanelRarity.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			flowLayoutPanelRarity.BorderStyle = BorderStyle.FixedSingle;
			flowLayoutPanelRarity.Controls.Add(scalingLabelRarity);
			flowLayoutPanelRarity.Controls.Add(scalingCheckedListBoxRarity);
			flowLayoutPanelRarity.FlowDirection = FlowDirection.TopDown;
			flowLayoutPanelRarity.Location = new Point(693, 3);
			flowLayoutPanelRarity.MinimumSize = new Size(50, 10);
			flowLayoutPanelRarity.Name = "flowLayoutPanelRarity";
			flowLayoutPanelRarity.Padding = new Padding(1, 2, 0, 0);
			flowLayoutPanelRarity.Size = new Size(224, 102);
			flowLayoutPanelRarity.TabIndex = 11;
			// 
			// scalingLabelRarity
			// 
			scalingLabelRarity.AutoSize = true;
			scalingLabelRarity.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelRarity.ForeColor = Color.Gold;
			scalingLabelRarity.Location = new Point(4, 2);
			scalingLabelRarity.Name = "scalingLabelRarity";
			scalingLabelRarity.Size = new Size(46, 18);
			scalingLabelRarity.TabIndex = 0;
			scalingLabelRarity.Text = "Rarity";
			scalingLabelRarity.UseMnemonic = false;
			scalingLabelRarity.MouseClick += scalingLabelCategory_MouseClick;
			// 
			// scalingCheckedListBoxRarity
			// 
			scalingCheckedListBoxRarity.BackColor = Color.FromArgb(46, 31, 21);
			scalingCheckedListBoxRarity.BorderStyle = BorderStyle.None;
			scalingCheckedListBoxRarity.CheckOnClick = true;
			scalingCheckedListBoxRarity.Font = new Font("Microsoft Sans Serif", 9F);
			scalingCheckedListBoxRarity.ForeColor = Color.White;
			scalingCheckedListBoxRarity.FormattingEnabled = true;
			scalingCheckedListBoxRarity.HorizontalScrollbar = true;
			scalingCheckedListBoxRarity.IntegralHeight = false;
			scalingCheckedListBoxRarity.Items.AddRange(new object[] { "Common", "Rare", "Epic", "Lengendary" });
			scalingCheckedListBoxRarity.Location = new Point(4, 20);
			scalingCheckedListBoxRarity.Margin = new Padding(3, 0, 3, 0);
			scalingCheckedListBoxRarity.MultiColumn = true;
			scalingCheckedListBoxRarity.Name = "scalingCheckedListBoxRarity";
			scalingCheckedListBoxRarity.Size = new Size(215, 80);
			scalingCheckedListBoxRarity.TabIndex = 7;
			scalingCheckedListBoxRarity.SelectedValueChanged += scalingCheckedListBox_SelectedValueChanged;
			scalingCheckedListBoxRarity.MouseMove += scalingCheckedListBox_MouseMove;
			// 
			// flowLayoutPanelPrefixAttributes
			// 
			flowLayoutPanelPrefixAttributes.AutoSize = true;
			flowLayoutPanelPrefixAttributes.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			flowLayoutPanelPrefixAttributes.BorderStyle = BorderStyle.FixedSingle;
			flowLayoutPanelPrefixAttributes.Controls.Add(scalingLabelPrefixAttributes);
			flowLayoutPanelPrefixAttributes.Controls.Add(scalingCheckedListBoxPrefixAttributes);
			flowLayoutPanelPrefixAttributes.FlowDirection = FlowDirection.TopDown;
			flowLayoutPanelPrefixAttributes.Location = new Point(463, 219);
			flowLayoutPanelPrefixAttributes.MinimumSize = new Size(50, 10);
			flowLayoutPanelPrefixAttributes.Name = "flowLayoutPanelPrefixAttributes";
			flowLayoutPanelPrefixAttributes.Padding = new Padding(1, 2, 0, 0);
			flowLayoutPanelPrefixAttributes.Size = new Size(224, 102);
			flowLayoutPanelPrefixAttributes.TabIndex = 12;
			// 
			// scalingLabelPrefixAttributes
			// 
			scalingLabelPrefixAttributes.AutoSize = true;
			scalingLabelPrefixAttributes.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelPrefixAttributes.ForeColor = Color.Gold;
			scalingLabelPrefixAttributes.Location = new Point(4, 2);
			scalingLabelPrefixAttributes.Name = "scalingLabelPrefixAttributes";
			scalingLabelPrefixAttributes.Size = new Size(110, 18);
			scalingLabelPrefixAttributes.TabIndex = 0;
			scalingLabelPrefixAttributes.Text = "Prefix Attributes";
			scalingLabelPrefixAttributes.UseMnemonic = false;
			scalingLabelPrefixAttributes.MouseClick += scalingLabelCategory_MouseClick;
			// 
			// scalingCheckedListBoxPrefixAttributes
			// 
			scalingCheckedListBoxPrefixAttributes.BackColor = Color.FromArgb(46, 31, 21);
			scalingCheckedListBoxPrefixAttributes.BorderStyle = BorderStyle.None;
			scalingCheckedListBoxPrefixAttributes.CheckOnClick = true;
			scalingCheckedListBoxPrefixAttributes.Font = new Font("Microsoft Sans Serif", 9F);
			scalingCheckedListBoxPrefixAttributes.ForeColor = Color.White;
			scalingCheckedListBoxPrefixAttributes.FormattingEnabled = true;
			scalingCheckedListBoxPrefixAttributes.HorizontalScrollbar = true;
			scalingCheckedListBoxPrefixAttributes.IntegralHeight = false;
			scalingCheckedListBoxPrefixAttributes.Items.AddRange(new object[] { "Physical Damage", "Elemental Resistance", "Elemental Damage" });
			scalingCheckedListBoxPrefixAttributes.Location = new Point(4, 20);
			scalingCheckedListBoxPrefixAttributes.Margin = new Padding(3, 0, 3, 0);
			scalingCheckedListBoxPrefixAttributes.MultiColumn = true;
			scalingCheckedListBoxPrefixAttributes.Name = "scalingCheckedListBoxPrefixAttributes";
			scalingCheckedListBoxPrefixAttributes.Size = new Size(215, 80);
			scalingCheckedListBoxPrefixAttributes.TabIndex = 7;
			scalingCheckedListBoxPrefixAttributes.SelectedValueChanged += scalingCheckedListBox_SelectedValueChanged;
			scalingCheckedListBoxPrefixAttributes.MouseMove += scalingCheckedListBox_MouseMove;
			// 
			// flowLayoutPanelSuffixAttributes
			// 
			flowLayoutPanelSuffixAttributes.AutoSize = true;
			flowLayoutPanelSuffixAttributes.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			flowLayoutPanelSuffixAttributes.BorderStyle = BorderStyle.FixedSingle;
			flowLayoutPanelSuffixAttributes.Controls.Add(scalingLabelSuffixAttributes);
			flowLayoutPanelSuffixAttributes.Controls.Add(scalingCheckedListBoxSuffixAttributes);
			flowLayoutPanelSuffixAttributes.FlowDirection = FlowDirection.TopDown;
			flowLayoutPanelSuffixAttributes.Location = new Point(3, 327);
			flowLayoutPanelSuffixAttributes.MinimumSize = new Size(50, 10);
			flowLayoutPanelSuffixAttributes.Name = "flowLayoutPanelSuffixAttributes";
			flowLayoutPanelSuffixAttributes.Padding = new Padding(1, 2, 0, 0);
			flowLayoutPanelSuffixAttributes.Size = new Size(224, 102);
			flowLayoutPanelSuffixAttributes.TabIndex = 13;
			flowLayoutPanelSuffixAttributes.WrapContents = false;
			// 
			// scalingLabelSuffixAttributes
			// 
			scalingLabelSuffixAttributes.AutoSize = true;
			scalingLabelSuffixAttributes.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelSuffixAttributes.ForeColor = Color.Gold;
			scalingLabelSuffixAttributes.Location = new Point(4, 2);
			scalingLabelSuffixAttributes.Name = "scalingLabelSuffixAttributes";
			scalingLabelSuffixAttributes.Size = new Size(109, 18);
			scalingLabelSuffixAttributes.TabIndex = 0;
			scalingLabelSuffixAttributes.Text = "Suffix Attributes";
			scalingLabelSuffixAttributes.UseMnemonic = false;
			scalingLabelSuffixAttributes.MouseClick += scalingLabelCategory_MouseClick;
			// 
			// scalingCheckedListBoxSuffixAttributes
			// 
			scalingCheckedListBoxSuffixAttributes.BackColor = Color.FromArgb(46, 31, 21);
			scalingCheckedListBoxSuffixAttributes.BorderStyle = BorderStyle.None;
			scalingCheckedListBoxSuffixAttributes.CheckOnClick = true;
			scalingCheckedListBoxSuffixAttributes.Font = new Font("Microsoft Sans Serif", 9F);
			scalingCheckedListBoxSuffixAttributes.ForeColor = Color.White;
			scalingCheckedListBoxSuffixAttributes.FormattingEnabled = true;
			scalingCheckedListBoxSuffixAttributes.HorizontalScrollbar = true;
			scalingCheckedListBoxSuffixAttributes.IntegralHeight = false;
			scalingCheckedListBoxSuffixAttributes.Items.AddRange(new object[] { "Physical Damage", "Elemental Resistance", "Elemental Damage" });
			scalingCheckedListBoxSuffixAttributes.Location = new Point(4, 20);
			scalingCheckedListBoxSuffixAttributes.Margin = new Padding(3, 0, 3, 0);
			scalingCheckedListBoxSuffixAttributes.MultiColumn = true;
			scalingCheckedListBoxSuffixAttributes.Name = "scalingCheckedListBoxSuffixAttributes";
			scalingCheckedListBoxSuffixAttributes.Size = new Size(215, 80);
			scalingCheckedListBoxSuffixAttributes.TabIndex = 7;
			scalingCheckedListBoxSuffixAttributes.SelectedValueChanged += scalingCheckedListBox_SelectedValueChanged;
			scalingCheckedListBoxSuffixAttributes.MouseMove += scalingCheckedListBox_MouseMove;
			// 
			// flowLayoutPanelBaseAttributes
			// 
			flowLayoutPanelBaseAttributes.AutoSize = true;
			flowLayoutPanelBaseAttributes.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			flowLayoutPanelBaseAttributes.BorderStyle = BorderStyle.FixedSingle;
			flowLayoutPanelBaseAttributes.Controls.Add(scalingLabelBaseAttributes);
			flowLayoutPanelBaseAttributes.Controls.Add(scalingCheckedListBoxBaseAttributes);
			flowLayoutPanelBaseAttributes.FlowDirection = FlowDirection.TopDown;
			flowLayoutPanelBaseAttributes.Location = new Point(693, 219);
			flowLayoutPanelBaseAttributes.MinimumSize = new Size(50, 10);
			flowLayoutPanelBaseAttributes.Name = "flowLayoutPanelBaseAttributes";
			flowLayoutPanelBaseAttributes.Padding = new Padding(1, 2, 0, 0);
			flowLayoutPanelBaseAttributes.Size = new Size(224, 102);
			flowLayoutPanelBaseAttributes.TabIndex = 14;
			// 
			// scalingLabelBaseAttributes
			// 
			scalingLabelBaseAttributes.AutoSize = true;
			scalingLabelBaseAttributes.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelBaseAttributes.ForeColor = Color.Gold;
			scalingLabelBaseAttributes.Location = new Point(4, 2);
			scalingLabelBaseAttributes.Name = "scalingLabelBaseAttributes";
			scalingLabelBaseAttributes.Size = new Size(107, 18);
			scalingLabelBaseAttributes.TabIndex = 0;
			scalingLabelBaseAttributes.Text = "Base Attributes";
			scalingLabelBaseAttributes.UseMnemonic = false;
			scalingLabelBaseAttributes.MouseClick += scalingLabelCategory_MouseClick;
			// 
			// scalingCheckedListBoxBaseAttributes
			// 
			scalingCheckedListBoxBaseAttributes.BackColor = Color.FromArgb(46, 31, 21);
			scalingCheckedListBoxBaseAttributes.BorderStyle = BorderStyle.None;
			scalingCheckedListBoxBaseAttributes.CheckOnClick = true;
			scalingCheckedListBoxBaseAttributes.Font = new Font("Microsoft Sans Serif", 9F);
			scalingCheckedListBoxBaseAttributes.ForeColor = Color.White;
			scalingCheckedListBoxBaseAttributes.FormattingEnabled = true;
			scalingCheckedListBoxBaseAttributes.HorizontalScrollbar = true;
			scalingCheckedListBoxBaseAttributes.IntegralHeight = false;
			scalingCheckedListBoxBaseAttributes.Items.AddRange(new object[] { "Physical Damage", "Elemental Resistance", "Elemental Damage" });
			scalingCheckedListBoxBaseAttributes.Location = new Point(4, 20);
			scalingCheckedListBoxBaseAttributes.Margin = new Padding(3, 0, 3, 0);
			scalingCheckedListBoxBaseAttributes.MultiColumn = true;
			scalingCheckedListBoxBaseAttributes.Name = "scalingCheckedListBoxBaseAttributes";
			scalingCheckedListBoxBaseAttributes.Size = new Size(215, 80);
			scalingCheckedListBoxBaseAttributes.TabIndex = 7;
			scalingCheckedListBoxBaseAttributes.SelectedValueChanged += scalingCheckedListBox_SelectedValueChanged;
			scalingCheckedListBoxBaseAttributes.MouseMove += scalingCheckedListBox_MouseMove;
			// 
			// flowLayoutPanelMain
			// 
			flowLayoutPanelMain.AutoSize = true;
			flowLayoutPanelMain.Controls.Add(flowLayoutPanelCharacters);
			flowLayoutPanelMain.Controls.Add(flowLayoutPanelInVaults);
			flowLayoutPanelMain.Controls.Add(flowLayoutPanelItemType);
			flowLayoutPanelMain.Controls.Add(flowLayoutPanelRarity);
			flowLayoutPanelMain.Controls.Add(flowLayoutPanelOrigin);
			flowLayoutPanelMain.Controls.Add(flowLayoutPanelQuality);
			flowLayoutPanelMain.Controls.Add(flowLayoutPanelStyle);
			flowLayoutPanelMain.Controls.Add(flowLayoutPanelSetItems);
			flowLayoutPanelMain.Controls.Add(flowLayoutPanelWithCharm);
			flowLayoutPanelMain.Controls.Add(flowLayoutPanelWithRelic);
			flowLayoutPanelMain.Controls.Add(flowLayoutPanelItemAttributes);
			flowLayoutPanelMain.Controls.Add(flowLayoutPanelPrefixName);
			flowLayoutPanelMain.Controls.Add(flowLayoutPanelPrefixAttributes);
			flowLayoutPanelMain.Controls.Add(flowLayoutPanelBaseAttributes);
			flowLayoutPanelMain.Controls.Add(flowLayoutPanelSuffixName);
			flowLayoutPanelMain.Controls.Add(flowLayoutPanelSuffixAttributes);
			flowLayoutPanelMain.Location = new Point(0, 0);
			flowLayoutPanelMain.Margin = new Padding(0);
			flowLayoutPanelMain.MinimumSize = new Size(200, 100);
			flowLayoutPanelMain.Name = "flowLayoutPanelMain";
			flowLayoutPanelMain.Size = new Size(1150, 432);
			flowLayoutPanelMain.TabIndex = 15;
			// 
			// flowLayoutPanelInVaults
			// 
			flowLayoutPanelInVaults.AutoSize = true;
			flowLayoutPanelInVaults.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			flowLayoutPanelInVaults.BorderStyle = BorderStyle.FixedSingle;
			flowLayoutPanelInVaults.Controls.Add(scalingLabelInVaults);
			flowLayoutPanelInVaults.Controls.Add(scalingCheckedListBoxVaults);
			flowLayoutPanelInVaults.FlowDirection = FlowDirection.TopDown;
			flowLayoutPanelInVaults.Location = new Point(233, 3);
			flowLayoutPanelInVaults.MinimumSize = new Size(50, 10);
			flowLayoutPanelInVaults.Name = "flowLayoutPanelInVaults";
			flowLayoutPanelInVaults.Padding = new Padding(1, 2, 0, 0);
			flowLayoutPanelInVaults.Size = new Size(224, 102);
			flowLayoutPanelInVaults.TabIndex = 19;
			// 
			// scalingLabelInVaults
			// 
			scalingLabelInVaults.AutoSize = true;
			scalingLabelInVaults.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelInVaults.ForeColor = Color.Gold;
			scalingLabelInVaults.Location = new Point(4, 2);
			scalingLabelInVaults.Name = "scalingLabelInVaults";
			scalingLabelInVaults.Size = new Size(48, 18);
			scalingLabelInVaults.TabIndex = 0;
			scalingLabelInVaults.Text = "Vaults";
			scalingLabelInVaults.UseMnemonic = false;
			scalingLabelInVaults.MouseClick += scalingLabelCategory_MouseClick;
			// 
			// scalingCheckedListBoxVaults
			// 
			scalingCheckedListBoxVaults.BackColor = Color.FromArgb(46, 31, 21);
			scalingCheckedListBoxVaults.BorderStyle = BorderStyle.None;
			scalingCheckedListBoxVaults.CheckOnClick = true;
			scalingCheckedListBoxVaults.Font = new Font("Microsoft Sans Serif", 9F);
			scalingCheckedListBoxVaults.ForeColor = Color.White;
			scalingCheckedListBoxVaults.FormattingEnabled = true;
			scalingCheckedListBoxVaults.HorizontalScrollbar = true;
			scalingCheckedListBoxVaults.IntegralHeight = false;
			scalingCheckedListBoxVaults.Items.AddRange(new object[] { "Vault Name 1", "Vault Name 2", "Vault Name 3" });
			scalingCheckedListBoxVaults.Location = new Point(4, 20);
			scalingCheckedListBoxVaults.Margin = new Padding(3, 0, 3, 0);
			scalingCheckedListBoxVaults.MultiColumn = true;
			scalingCheckedListBoxVaults.Name = "scalingCheckedListBoxVaults";
			scalingCheckedListBoxVaults.Size = new Size(215, 80);
			scalingCheckedListBoxVaults.TabIndex = 7;
			scalingCheckedListBoxVaults.SelectedValueChanged += scalingCheckedListBox_SelectedValueChanged;
			scalingCheckedListBoxVaults.MouseMove += scalingCheckedListBox_MouseMove;
			// 
			// flowLayoutPanelOrigin
			// 
			flowLayoutPanelOrigin.AutoSize = true;
			flowLayoutPanelOrigin.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			flowLayoutPanelOrigin.BorderStyle = BorderStyle.FixedSingle;
			flowLayoutPanelOrigin.Controls.Add(scalingLabelOrigin);
			flowLayoutPanelOrigin.Controls.Add(scalingCheckedListBoxOrigin);
			flowLayoutPanelOrigin.FlowDirection = FlowDirection.TopDown;
			flowLayoutPanelOrigin.Location = new Point(923, 3);
			flowLayoutPanelOrigin.MinimumSize = new Size(50, 10);
			flowLayoutPanelOrigin.Name = "flowLayoutPanelOrigin";
			flowLayoutPanelOrigin.Padding = new Padding(1, 2, 0, 0);
			flowLayoutPanelOrigin.Size = new Size(224, 102);
			flowLayoutPanelOrigin.TabIndex = 22;
			// 
			// scalingLabelOrigin
			// 
			scalingLabelOrigin.AutoSize = true;
			scalingLabelOrigin.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelOrigin.ForeColor = Color.Gold;
			scalingLabelOrigin.Location = new Point(4, 2);
			scalingLabelOrigin.Name = "scalingLabelOrigin";
			scalingLabelOrigin.Size = new Size(47, 18);
			scalingLabelOrigin.TabIndex = 0;
			scalingLabelOrigin.Text = "Origin";
			scalingLabelOrigin.UseMnemonic = false;
			scalingLabelOrigin.MouseClick += scalingLabelCategory_MouseClick;
			// 
			// scalingCheckedListBoxOrigin
			// 
			scalingCheckedListBoxOrigin.BackColor = Color.FromArgb(46, 31, 21);
			scalingCheckedListBoxOrigin.BorderStyle = BorderStyle.None;
			scalingCheckedListBoxOrigin.CheckOnClick = true;
			scalingCheckedListBoxOrigin.Font = new Font("Microsoft Sans Serif", 9F);
			scalingCheckedListBoxOrigin.ForeColor = Color.White;
			scalingCheckedListBoxOrigin.FormattingEnabled = true;
			scalingCheckedListBoxOrigin.HorizontalScrollbar = true;
			scalingCheckedListBoxOrigin.IntegralHeight = false;
			scalingCheckedListBoxOrigin.Items.AddRange(new object[] { "Titan Quest Original", "Immortal Throne", "Ragnarok", "Atlantis", "Eternal Embers" });
			scalingCheckedListBoxOrigin.Location = new Point(4, 20);
			scalingCheckedListBoxOrigin.Margin = new Padding(3, 0, 3, 0);
			scalingCheckedListBoxOrigin.MultiColumn = true;
			scalingCheckedListBoxOrigin.Name = "scalingCheckedListBoxOrigin";
			scalingCheckedListBoxOrigin.Size = new Size(215, 80);
			scalingCheckedListBoxOrigin.TabIndex = 7;
			scalingCheckedListBoxOrigin.SelectedValueChanged += scalingCheckedListBox_SelectedValueChanged;
			scalingCheckedListBoxOrigin.MouseMove += scalingCheckedListBox_MouseMove;
			// 
			// flowLayoutPanelQuality
			// 
			flowLayoutPanelQuality.AutoSize = true;
			flowLayoutPanelQuality.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			flowLayoutPanelQuality.BorderStyle = BorderStyle.FixedSingle;
			flowLayoutPanelQuality.Controls.Add(scalingLabelQuality);
			flowLayoutPanelQuality.Controls.Add(scalingCheckedListBoxQuality);
			flowLayoutPanelQuality.FlowDirection = FlowDirection.TopDown;
			flowLayoutPanelQuality.Location = new Point(3, 111);
			flowLayoutPanelQuality.MinimumSize = new Size(50, 10);
			flowLayoutPanelQuality.Name = "flowLayoutPanelQuality";
			flowLayoutPanelQuality.Padding = new Padding(1, 2, 0, 0);
			flowLayoutPanelQuality.Size = new Size(224, 102);
			flowLayoutPanelQuality.TabIndex = 21;
			// 
			// scalingLabelQuality
			// 
			scalingLabelQuality.AutoSize = true;
			scalingLabelQuality.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelQuality.ForeColor = Color.Gold;
			scalingLabelQuality.Location = new Point(4, 2);
			scalingLabelQuality.Name = "scalingLabelQuality";
			scalingLabelQuality.Size = new Size(53, 18);
			scalingLabelQuality.TabIndex = 0;
			scalingLabelQuality.Text = "Quality";
			scalingLabelQuality.UseMnemonic = false;
			scalingLabelQuality.MouseClick += scalingLabelCategory_MouseClick;
			// 
			// scalingCheckedListBoxQuality
			// 
			scalingCheckedListBoxQuality.BackColor = Color.FromArgb(46, 31, 21);
			scalingCheckedListBoxQuality.BorderStyle = BorderStyle.None;
			scalingCheckedListBoxQuality.CheckOnClick = true;
			scalingCheckedListBoxQuality.Font = new Font("Microsoft Sans Serif", 9F);
			scalingCheckedListBoxQuality.ForeColor = Color.White;
			scalingCheckedListBoxQuality.FormattingEnabled = true;
			scalingCheckedListBoxQuality.HorizontalScrollbar = true;
			scalingCheckedListBoxQuality.IntegralHeight = false;
			scalingCheckedListBoxQuality.Items.AddRange(new object[] { "Common", "Rare", "Epic", "Lengendary" });
			scalingCheckedListBoxQuality.Location = new Point(4, 20);
			scalingCheckedListBoxQuality.Margin = new Padding(3, 0, 3, 0);
			scalingCheckedListBoxQuality.MultiColumn = true;
			scalingCheckedListBoxQuality.Name = "scalingCheckedListBoxQuality";
			scalingCheckedListBoxQuality.Size = new Size(215, 80);
			scalingCheckedListBoxQuality.TabIndex = 7;
			scalingCheckedListBoxQuality.SelectedValueChanged += scalingCheckedListBox_SelectedValueChanged;
			scalingCheckedListBoxQuality.MouseMove += scalingCheckedListBox_MouseMove;
			// 
			// flowLayoutPanelStyle
			// 
			flowLayoutPanelStyle.AutoSize = true;
			flowLayoutPanelStyle.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			flowLayoutPanelStyle.BorderStyle = BorderStyle.FixedSingle;
			flowLayoutPanelStyle.Controls.Add(scalingLabelStyle);
			flowLayoutPanelStyle.Controls.Add(scalingCheckedListBoxStyle);
			flowLayoutPanelStyle.FlowDirection = FlowDirection.TopDown;
			flowLayoutPanelStyle.Location = new Point(233, 111);
			flowLayoutPanelStyle.MinimumSize = new Size(50, 10);
			flowLayoutPanelStyle.Name = "flowLayoutPanelStyle";
			flowLayoutPanelStyle.Padding = new Padding(1, 2, 0, 0);
			flowLayoutPanelStyle.Size = new Size(224, 102);
			flowLayoutPanelStyle.TabIndex = 20;
			// 
			// scalingLabelStyle
			// 
			scalingLabelStyle.AutoSize = true;
			scalingLabelStyle.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelStyle.ForeColor = Color.Gold;
			scalingLabelStyle.Location = new Point(4, 2);
			scalingLabelStyle.Name = "scalingLabelStyle";
			scalingLabelStyle.Size = new Size(40, 18);
			scalingLabelStyle.TabIndex = 0;
			scalingLabelStyle.Text = "Style";
			scalingLabelStyle.UseMnemonic = false;
			scalingLabelStyle.MouseClick += scalingLabelCategory_MouseClick;
			// 
			// scalingCheckedListBoxStyle
			// 
			scalingCheckedListBoxStyle.BackColor = Color.FromArgb(46, 31, 21);
			scalingCheckedListBoxStyle.BorderStyle = BorderStyle.None;
			scalingCheckedListBoxStyle.CheckOnClick = true;
			scalingCheckedListBoxStyle.Font = new Font("Microsoft Sans Serif", 9F);
			scalingCheckedListBoxStyle.ForeColor = Color.White;
			scalingCheckedListBoxStyle.FormattingEnabled = true;
			scalingCheckedListBoxStyle.HorizontalScrollbar = true;
			scalingCheckedListBoxStyle.IntegralHeight = false;
			scalingCheckedListBoxStyle.Items.AddRange(new object[] { "Aesir", "Alexandrian", "Asgardian", "Babylonian" });
			scalingCheckedListBoxStyle.Location = new Point(4, 20);
			scalingCheckedListBoxStyle.Margin = new Padding(3, 0, 3, 0);
			scalingCheckedListBoxStyle.MultiColumn = true;
			scalingCheckedListBoxStyle.Name = "scalingCheckedListBoxStyle";
			scalingCheckedListBoxStyle.Size = new Size(215, 80);
			scalingCheckedListBoxStyle.TabIndex = 7;
			scalingCheckedListBoxStyle.SelectedValueChanged += scalingCheckedListBox_SelectedValueChanged;
			scalingCheckedListBoxStyle.MouseMove += scalingCheckedListBox_MouseMove;
			// 
			// flowLayoutPanelSetItems
			// 
			flowLayoutPanelSetItems.AutoSize = true;
			flowLayoutPanelSetItems.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			flowLayoutPanelSetItems.BorderStyle = BorderStyle.FixedSingle;
			flowLayoutPanelSetItems.Controls.Add(scalingLabelSetItems);
			flowLayoutPanelSetItems.Controls.Add(scalingCheckedListBoxSetItems);
			flowLayoutPanelSetItems.FlowDirection = FlowDirection.TopDown;
			flowLayoutPanelSetItems.Location = new Point(463, 111);
			flowLayoutPanelSetItems.MinimumSize = new Size(50, 10);
			flowLayoutPanelSetItems.Name = "flowLayoutPanelSetItems";
			flowLayoutPanelSetItems.Padding = new Padding(1, 2, 0, 0);
			flowLayoutPanelSetItems.Size = new Size(224, 102);
			flowLayoutPanelSetItems.TabIndex = 23;
			// 
			// scalingLabelSetItems
			// 
			scalingLabelSetItems.AutoSize = true;
			scalingLabelSetItems.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelSetItems.ForeColor = Color.Gold;
			scalingLabelSetItems.Location = new Point(4, 2);
			scalingLabelSetItems.Name = "scalingLabelSetItems";
			scalingLabelSetItems.Size = new Size(38, 18);
			scalingLabelSetItems.TabIndex = 0;
			scalingLabelSetItems.Text = "Sets";
			scalingLabelSetItems.UseMnemonic = false;
			scalingLabelSetItems.MouseClick += scalingLabelCategory_MouseClick;
			// 
			// scalingCheckedListBoxSetItems
			// 
			scalingCheckedListBoxSetItems.BackColor = Color.FromArgb(46, 31, 21);
			scalingCheckedListBoxSetItems.BorderStyle = BorderStyle.None;
			scalingCheckedListBoxSetItems.CheckOnClick = true;
			scalingCheckedListBoxSetItems.Font = new Font("Microsoft Sans Serif", 9F);
			scalingCheckedListBoxSetItems.ForeColor = Color.White;
			scalingCheckedListBoxSetItems.FormattingEnabled = true;
			scalingCheckedListBoxSetItems.HorizontalScrollbar = true;
			scalingCheckedListBoxSetItems.IntegralHeight = false;
			scalingCheckedListBoxSetItems.Items.AddRange(new object[] { "Set item 1", "Set item 2", "Set item 3" });
			scalingCheckedListBoxSetItems.Location = new Point(4, 20);
			scalingCheckedListBoxSetItems.Margin = new Padding(3, 0, 3, 0);
			scalingCheckedListBoxSetItems.MultiColumn = true;
			scalingCheckedListBoxSetItems.Name = "scalingCheckedListBoxSetItems";
			scalingCheckedListBoxSetItems.Size = new Size(215, 80);
			scalingCheckedListBoxSetItems.TabIndex = 7;
			scalingCheckedListBoxSetItems.SelectedValueChanged += scalingCheckedListBox_SelectedValueChanged;
			scalingCheckedListBoxSetItems.MouseMove += scalingCheckedListBox_MouseMove;
			// 
			// flowLayoutPanelWithCharm
			// 
			flowLayoutPanelWithCharm.AutoSize = true;
			flowLayoutPanelWithCharm.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			flowLayoutPanelWithCharm.BorderStyle = BorderStyle.FixedSingle;
			flowLayoutPanelWithCharm.Controls.Add(scalingLabelWithCharm);
			flowLayoutPanelWithCharm.Controls.Add(scalingCheckedListBoxWithCharm);
			flowLayoutPanelWithCharm.FlowDirection = FlowDirection.TopDown;
			flowLayoutPanelWithCharm.Location = new Point(693, 111);
			flowLayoutPanelWithCharm.MinimumSize = new Size(50, 10);
			flowLayoutPanelWithCharm.Name = "flowLayoutPanelWithCharm";
			flowLayoutPanelWithCharm.Padding = new Padding(1, 2, 0, 0);
			flowLayoutPanelWithCharm.Size = new Size(224, 102);
			flowLayoutPanelWithCharm.TabIndex = 17;
			// 
			// scalingLabelWithCharm
			// 
			scalingLabelWithCharm.AutoSize = true;
			scalingLabelWithCharm.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelWithCharm.ForeColor = Color.Gold;
			scalingLabelWithCharm.Location = new Point(4, 2);
			scalingLabelWithCharm.Name = "scalingLabelWithCharm";
			scalingLabelWithCharm.Size = new Size(102, 18);
			scalingLabelWithCharm.TabIndex = 0;
			scalingLabelWithCharm.Text = "Having Charm";
			scalingLabelWithCharm.UseMnemonic = false;
			scalingLabelWithCharm.MouseClick += scalingLabelCategory_MouseClick;
			// 
			// scalingCheckedListBoxWithCharm
			// 
			scalingCheckedListBoxWithCharm.BackColor = Color.FromArgb(46, 31, 21);
			scalingCheckedListBoxWithCharm.BorderStyle = BorderStyle.None;
			scalingCheckedListBoxWithCharm.CheckOnClick = true;
			scalingCheckedListBoxWithCharm.Font = new Font("Microsoft Sans Serif", 9F);
			scalingCheckedListBoxWithCharm.ForeColor = Color.White;
			scalingCheckedListBoxWithCharm.FormattingEnabled = true;
			scalingCheckedListBoxWithCharm.HorizontalScrollbar = true;
			scalingCheckedListBoxWithCharm.IntegralHeight = false;
			scalingCheckedListBoxWithCharm.Items.AddRange(new object[] { "Demon's Blood", "Turtle Shell", "Saber Claw" });
			scalingCheckedListBoxWithCharm.Location = new Point(4, 20);
			scalingCheckedListBoxWithCharm.Margin = new Padding(3, 0, 3, 0);
			scalingCheckedListBoxWithCharm.MultiColumn = true;
			scalingCheckedListBoxWithCharm.Name = "scalingCheckedListBoxWithCharm";
			scalingCheckedListBoxWithCharm.Size = new Size(215, 80);
			scalingCheckedListBoxWithCharm.TabIndex = 7;
			scalingCheckedListBoxWithCharm.SelectedValueChanged += scalingCheckedListBox_SelectedValueChanged;
			scalingCheckedListBoxWithCharm.MouseMove += scalingCheckedListBox_MouseMove;
			// 
			// flowLayoutPanelWithRelic
			// 
			flowLayoutPanelWithRelic.AutoSize = true;
			flowLayoutPanelWithRelic.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			flowLayoutPanelWithRelic.BorderStyle = BorderStyle.FixedSingle;
			flowLayoutPanelWithRelic.Controls.Add(scalingLabelWithRelic);
			flowLayoutPanelWithRelic.Controls.Add(scalingCheckedListBoxWithRelic);
			flowLayoutPanelWithRelic.FlowDirection = FlowDirection.TopDown;
			flowLayoutPanelWithRelic.Location = new Point(923, 111);
			flowLayoutPanelWithRelic.MinimumSize = new Size(50, 10);
			flowLayoutPanelWithRelic.Name = "flowLayoutPanelWithRelic";
			flowLayoutPanelWithRelic.Padding = new Padding(1, 2, 0, 0);
			flowLayoutPanelWithRelic.Size = new Size(224, 102);
			flowLayoutPanelWithRelic.TabIndex = 18;
			// 
			// scalingLabelWithRelic
			// 
			scalingLabelWithRelic.AutoSize = true;
			scalingLabelWithRelic.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelWithRelic.ForeColor = Color.Gold;
			scalingLabelWithRelic.Location = new Point(4, 2);
			scalingLabelWithRelic.Name = "scalingLabelWithRelic";
			scalingLabelWithRelic.Size = new Size(90, 18);
			scalingLabelWithRelic.TabIndex = 0;
			scalingLabelWithRelic.Text = "Having Relic";
			scalingLabelWithRelic.UseMnemonic = false;
			scalingLabelWithRelic.MouseClick += scalingLabelCategory_MouseClick;
			// 
			// scalingCheckedListBoxWithRelic
			// 
			scalingCheckedListBoxWithRelic.BackColor = Color.FromArgb(46, 31, 21);
			scalingCheckedListBoxWithRelic.BorderStyle = BorderStyle.None;
			scalingCheckedListBoxWithRelic.CheckOnClick = true;
			scalingCheckedListBoxWithRelic.Font = new Font("Microsoft Sans Serif", 9F);
			scalingCheckedListBoxWithRelic.ForeColor = Color.White;
			scalingCheckedListBoxWithRelic.FormattingEnabled = true;
			scalingCheckedListBoxWithRelic.HorizontalScrollbar = true;
			scalingCheckedListBoxWithRelic.IntegralHeight = false;
			scalingCheckedListBoxWithRelic.Items.AddRange(new object[] { "Aegis of Athena", "Anubis' Wrath", "Rage of Ares" });
			scalingCheckedListBoxWithRelic.Location = new Point(4, 20);
			scalingCheckedListBoxWithRelic.Margin = new Padding(3, 0, 3, 0);
			scalingCheckedListBoxWithRelic.MultiColumn = true;
			scalingCheckedListBoxWithRelic.Name = "scalingCheckedListBoxWithRelic";
			scalingCheckedListBoxWithRelic.Size = new Size(215, 80);
			scalingCheckedListBoxWithRelic.TabIndex = 7;
			scalingCheckedListBoxWithRelic.SelectedValueChanged += scalingCheckedListBox_SelectedValueChanged;
			scalingCheckedListBoxWithRelic.MouseMove += scalingCheckedListBox_MouseMove;
			// 
			// flowLayoutPanelPrefixName
			// 
			flowLayoutPanelPrefixName.AutoSize = true;
			flowLayoutPanelPrefixName.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			flowLayoutPanelPrefixName.BorderStyle = BorderStyle.FixedSingle;
			flowLayoutPanelPrefixName.Controls.Add(scalingLabelPrefixName);
			flowLayoutPanelPrefixName.Controls.Add(scalingCheckedListBoxPrefixName);
			flowLayoutPanelPrefixName.FlowDirection = FlowDirection.TopDown;
			flowLayoutPanelPrefixName.Location = new Point(233, 219);
			flowLayoutPanelPrefixName.MinimumSize = new Size(50, 10);
			flowLayoutPanelPrefixName.Name = "flowLayoutPanelPrefixName";
			flowLayoutPanelPrefixName.Padding = new Padding(1, 2, 0, 0);
			flowLayoutPanelPrefixName.Size = new Size(224, 102);
			flowLayoutPanelPrefixName.TabIndex = 16;
			// 
			// scalingLabelPrefixName
			// 
			scalingLabelPrefixName.AutoSize = true;
			scalingLabelPrefixName.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelPrefixName.ForeColor = Color.Gold;
			scalingLabelPrefixName.Location = new Point(4, 2);
			scalingLabelPrefixName.Name = "scalingLabelPrefixName";
			scalingLabelPrefixName.Size = new Size(89, 18);
			scalingLabelPrefixName.TabIndex = 0;
			scalingLabelPrefixName.Text = "Prefix Name";
			scalingLabelPrefixName.UseMnemonic = false;
			scalingLabelPrefixName.MouseClick += scalingLabelCategory_MouseClick;
			// 
			// scalingCheckedListBoxPrefixName
			// 
			scalingCheckedListBoxPrefixName.BackColor = Color.FromArgb(46, 31, 21);
			scalingCheckedListBoxPrefixName.BorderStyle = BorderStyle.None;
			scalingCheckedListBoxPrefixName.CheckOnClick = true;
			scalingCheckedListBoxPrefixName.Font = new Font("Microsoft Sans Serif", 9F);
			scalingCheckedListBoxPrefixName.ForeColor = Color.White;
			scalingCheckedListBoxPrefixName.FormattingEnabled = true;
			scalingCheckedListBoxPrefixName.HorizontalScrollbar = true;
			scalingCheckedListBoxPrefixName.IntegralHeight = false;
			scalingCheckedListBoxPrefixName.Items.AddRange(new object[] { "Divine", "Sacred", "Defender's" });
			scalingCheckedListBoxPrefixName.Location = new Point(4, 20);
			scalingCheckedListBoxPrefixName.Margin = new Padding(3, 0, 3, 0);
			scalingCheckedListBoxPrefixName.MultiColumn = true;
			scalingCheckedListBoxPrefixName.Name = "scalingCheckedListBoxPrefixName";
			scalingCheckedListBoxPrefixName.Size = new Size(215, 80);
			scalingCheckedListBoxPrefixName.TabIndex = 7;
			scalingCheckedListBoxPrefixName.SelectedValueChanged += scalingCheckedListBox_SelectedValueChanged;
			scalingCheckedListBoxPrefixName.MouseMove += scalingCheckedListBox_MouseMove;
			// 
			// flowLayoutPanelSuffixName
			// 
			flowLayoutPanelSuffixName.AutoSize = true;
			flowLayoutPanelSuffixName.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			flowLayoutPanelSuffixName.BorderStyle = BorderStyle.FixedSingle;
			flowLayoutPanelSuffixName.Controls.Add(scalingLabelSuffixName);
			flowLayoutPanelSuffixName.Controls.Add(scalingCheckedListBoxSuffixName);
			flowLayoutPanelSuffixName.FlowDirection = FlowDirection.TopDown;
			flowLayoutPanelSuffixName.Location = new Point(923, 219);
			flowLayoutPanelSuffixName.MinimumSize = new Size(50, 10);
			flowLayoutPanelSuffixName.Name = "flowLayoutPanelSuffixName";
			flowLayoutPanelSuffixName.Padding = new Padding(1, 2, 0, 0);
			flowLayoutPanelSuffixName.Size = new Size(224, 102);
			flowLayoutPanelSuffixName.TabIndex = 15;
			// 
			// scalingLabelSuffixName
			// 
			scalingLabelSuffixName.AutoSize = true;
			scalingLabelSuffixName.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelSuffixName.ForeColor = Color.Gold;
			scalingLabelSuffixName.Location = new Point(4, 2);
			scalingLabelSuffixName.Name = "scalingLabelSuffixName";
			scalingLabelSuffixName.Size = new Size(88, 18);
			scalingLabelSuffixName.TabIndex = 0;
			scalingLabelSuffixName.Text = "Suffix Name";
			scalingLabelSuffixName.UseMnemonic = false;
			scalingLabelSuffixName.MouseClick += scalingLabelCategory_MouseClick;
			// 
			// scalingCheckedListBoxSuffixName
			// 
			scalingCheckedListBoxSuffixName.BackColor = Color.FromArgb(46, 31, 21);
			scalingCheckedListBoxSuffixName.BorderStyle = BorderStyle.None;
			scalingCheckedListBoxSuffixName.CheckOnClick = true;
			scalingCheckedListBoxSuffixName.Font = new Font("Microsoft Sans Serif", 9F);
			scalingCheckedListBoxSuffixName.ForeColor = Color.White;
			scalingCheckedListBoxSuffixName.FormattingEnabled = true;
			scalingCheckedListBoxSuffixName.HorizontalScrollbar = true;
			scalingCheckedListBoxSuffixName.IntegralHeight = false;
			scalingCheckedListBoxSuffixName.Items.AddRange(new object[] { "of Prowess", "of Finesse", "of Erudition" });
			scalingCheckedListBoxSuffixName.Location = new Point(4, 20);
			scalingCheckedListBoxSuffixName.Margin = new Padding(3, 0, 3, 0);
			scalingCheckedListBoxSuffixName.MultiColumn = true;
			scalingCheckedListBoxSuffixName.Name = "scalingCheckedListBoxSuffixName";
			scalingCheckedListBoxSuffixName.Size = new Size(215, 80);
			scalingCheckedListBoxSuffixName.TabIndex = 7;
			scalingCheckedListBoxSuffixName.SelectedValueChanged += scalingCheckedListBox_SelectedValueChanged;
			scalingCheckedListBoxSuffixName.MouseMove += scalingCheckedListBox_MouseMove;
			// 
			// flowLayoutPanelSearchTerm
			// 
			flowLayoutPanelSearchTerm.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			flowLayoutPanelSearchTerm.AutoSize = true;
			flowLayoutPanelSearchTerm.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			flowLayoutPanelSearchTerm.BorderStyle = BorderStyle.FixedSingle;
			tableLayoutPanelContent.SetColumnSpan(flowLayoutPanelSearchTerm, 2);
			flowLayoutPanelSearchTerm.Controls.Add(scalingLabelSearchTerm);
			flowLayoutPanelSearchTerm.Controls.Add(scalingTextBoxSearchTerm);
			flowLayoutPanelSearchTerm.Controls.Add(scalingButtonReset);
			flowLayoutPanelSearchTerm.Location = new Point(3, 3);
			flowLayoutPanelSearchTerm.Name = "flowLayoutPanelSearchTerm";
			flowLayoutPanelSearchTerm.Size = new Size(753, 38);
			flowLayoutPanelSearchTerm.TabIndex = 16;
			// 
			// scalingLabelSearchTerm
			// 
			scalingLabelSearchTerm.AutoSize = true;
			scalingLabelSearchTerm.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelSearchTerm.ForeColor = Color.Gold;
			scalingLabelSearchTerm.Location = new Point(3, 7);
			scalingLabelSearchTerm.Margin = new Padding(3, 7, 3, 0);
			scalingLabelSearchTerm.Name = "scalingLabelSearchTerm";
			scalingLabelSearchTerm.Size = new Size(63, 18);
			scalingLabelSearchTerm.TabIndex = 1;
			scalingLabelSearchTerm.Text = "Search :";
			scalingLabelSearchTerm.UseMnemonic = false;
			// 
			// scalingTextBoxSearchTerm
			// 
			scalingTextBoxSearchTerm.BorderStyle = BorderStyle.FixedSingle;
			scalingTextBoxSearchTerm.Font = new Font("Microsoft Sans Serif", 9F);
			scalingTextBoxSearchTerm.Location = new Point(72, 3);
			scalingTextBoxSearchTerm.Name = "scalingTextBoxSearchTerm";
			scalingTextBoxSearchTerm.Size = new Size(387, 24);
			scalingTextBoxSearchTerm.TabIndex = 2;
			scalingTextBoxSearchTerm.TextChanged += scalingTextBoxSearchTerm_TextChanged;
			// 
			// scalingButtonReset
			// 
			scalingButtonReset.Anchor = AnchorStyles.None;
			scalingButtonReset.BackColor = Color.Transparent;
			scalingButtonReset.DownBitmap = (Bitmap)resources.GetObject("scalingButtonReset.DownBitmap");
			scalingButtonReset.FlatAppearance.BorderSize = 0;
			scalingButtonReset.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonReset.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonReset.FlatStyle = FlatStyle.Flat;
			scalingButtonReset.Font = new Font("Microsoft Sans Serif", 9F);
			scalingButtonReset.ForeColor = Color.FromArgb(51, 44, 28);
			scalingButtonReset.Image = (Image)resources.GetObject("scalingButtonReset.Image");
			scalingButtonReset.Location = new Point(465, 3);
			scalingButtonReset.Name = "scalingButtonReset";
			scalingButtonReset.OverBitmap = (Bitmap)resources.GetObject("scalingButtonReset.OverBitmap");
			scalingButtonReset.Size = new Size(100, 30);
			scalingButtonReset.SizeToGraphic = false;
			scalingButtonReset.TabIndex = 7;
			scalingButtonReset.Text = "Reset";
			scalingButtonReset.UpBitmap = (Bitmap)resources.GetObject("scalingButtonReset.UpBitmap");
			scalingButtonReset.UseCustomGraphic = true;
			scalingButtonReset.UseVisualStyleBackColor = false;
			scalingButtonReset.Click += scalingButtonReset_Click;
			// 
			// flowLayoutPanelQueries
			// 
			flowLayoutPanelQueries.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			flowLayoutPanelQueries.AutoSize = true;
			flowLayoutPanelQueries.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			flowLayoutPanelQueries.BorderStyle = BorderStyle.FixedSingle;
			flowLayoutPanelQueries.Controls.Add(scalingLabelQueries);
			flowLayoutPanelQueries.Controls.Add(scalingComboBoxQueryList);
			flowLayoutPanelQueries.Controls.Add(scalingButtonQuerySave);
			flowLayoutPanelQueries.Controls.Add(scalingButtonQueryDelete);
			flowLayoutPanelQueries.Location = new Point(762, 3);
			flowLayoutPanelQueries.Name = "flowLayoutPanelQueries";
			flowLayoutPanelQueries.Size = new Size(620, 38);
			flowLayoutPanelQueries.TabIndex = 17;
			// 
			// scalingLabelQueries
			// 
			scalingLabelQueries.AutoSize = true;
			scalingLabelQueries.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelQueries.ForeColor = Color.Gold;
			scalingLabelQueries.Location = new Point(3, 7);
			scalingLabelQueries.Margin = new Padding(3, 7, 3, 0);
			scalingLabelQueries.Name = "scalingLabelQueries";
			scalingLabelQueries.Size = new Size(68, 18);
			scalingLabelQueries.TabIndex = 2;
			scalingLabelQueries.Text = "Queries :";
			scalingLabelQueries.UseMnemonic = false;
			// 
			// scalingComboBoxQueryList
			// 
			scalingComboBoxQueryList.DrawMode = DrawMode.OwnerDrawFixed;
			scalingComboBoxQueryList.Font = new Font("Microsoft Sans Serif", 9F);
			scalingComboBoxQueryList.FormattingEnabled = true;
			scalingComboBoxQueryList.Items.AddRange(new object[] { "Search term 1", "Search term 2" });
			scalingComboBoxQueryList.Location = new Point(77, 3);
			scalingComboBoxQueryList.MaximumSize = new Size(380, 0);
			scalingComboBoxQueryList.MinimumSize = new Size(170, 0);
			scalingComboBoxQueryList.Name = "scalingComboBoxQueryList";
			scalingComboBoxQueryList.Size = new Size(300, 25);
			scalingComboBoxQueryList.TabIndex = 3;
			scalingComboBoxQueryList.SelectedIndexChanged += scalingComboBoxQueryList_SelectedIndexChanged;
			// 
			// scalingButtonQuerySave
			// 
			scalingButtonQuerySave.Anchor = AnchorStyles.None;
			scalingButtonQuerySave.BackColor = Color.Transparent;
			scalingButtonQuerySave.DownBitmap = (Bitmap)resources.GetObject("scalingButtonQuerySave.DownBitmap");
			scalingButtonQuerySave.FlatAppearance.BorderSize = 0;
			scalingButtonQuerySave.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonQuerySave.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonQuerySave.FlatStyle = FlatStyle.Flat;
			scalingButtonQuerySave.Font = new Font("Microsoft Sans Serif", 9F);
			scalingButtonQuerySave.ForeColor = Color.FromArgb(51, 44, 28);
			scalingButtonQuerySave.Image = (Image)resources.GetObject("scalingButtonQuerySave.Image");
			scalingButtonQuerySave.Location = new Point(383, 3);
			scalingButtonQuerySave.Name = "scalingButtonQuerySave";
			scalingButtonQuerySave.OverBitmap = (Bitmap)resources.GetObject("scalingButtonQuerySave.OverBitmap");
			scalingButtonQuerySave.Size = new Size(100, 30);
			scalingButtonQuerySave.SizeToGraphic = false;
			scalingButtonQuerySave.TabIndex = 4;
			scalingButtonQuerySave.Text = "Save";
			scalingButtonQuerySave.UpBitmap = (Bitmap)resources.GetObject("scalingButtonQuerySave.UpBitmap");
			scalingButtonQuerySave.UseCustomGraphic = true;
			scalingButtonQuerySave.UseVisualStyleBackColor = false;
			scalingButtonQuerySave.Click += scalingButtonQuerySave_Click;
			// 
			// scalingButtonQueryDelete
			// 
			scalingButtonQueryDelete.Anchor = AnchorStyles.None;
			scalingButtonQueryDelete.BackColor = Color.Transparent;
			scalingButtonQueryDelete.DownBitmap = (Bitmap)resources.GetObject("scalingButtonQueryDelete.DownBitmap");
			scalingButtonQueryDelete.FlatAppearance.BorderSize = 0;
			scalingButtonQueryDelete.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonQueryDelete.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonQueryDelete.FlatStyle = FlatStyle.Flat;
			scalingButtonQueryDelete.Font = new Font("Microsoft Sans Serif", 9F);
			scalingButtonQueryDelete.ForeColor = Color.FromArgb(51, 44, 28);
			scalingButtonQueryDelete.Image = (Image)resources.GetObject("scalingButtonQueryDelete.Image");
			scalingButtonQueryDelete.Location = new Point(489, 3);
			scalingButtonQueryDelete.Name = "scalingButtonQueryDelete";
			scalingButtonQueryDelete.OverBitmap = (Bitmap)resources.GetObject("scalingButtonQueryDelete.OverBitmap");
			scalingButtonQueryDelete.Size = new Size(100, 30);
			scalingButtonQueryDelete.SizeToGraphic = false;
			scalingButtonQueryDelete.TabIndex = 5;
			scalingButtonQueryDelete.Text = "Delete";
			scalingButtonQueryDelete.UpBitmap = (Bitmap)resources.GetObject("scalingButtonQueryDelete.UpBitmap");
			scalingButtonQueryDelete.UseCustomGraphic = true;
			scalingButtonQueryDelete.UseVisualStyleBackColor = false;
			scalingButtonQueryDelete.Click += scalingButtonQueryDelete_Click;
			// 
			// tableLayoutPanelContent
			// 
			tableLayoutPanelContent.AutoSize = true;
			tableLayoutPanelContent.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			tableLayoutPanelContent.ColumnCount = 3;
			tableLayoutPanelContent.ColumnStyles.Add(new ColumnStyle());
			tableLayoutPanelContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
			tableLayoutPanelContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
			tableLayoutPanelContent.Controls.Add(flowLayoutPanelMainAutoScroll, 1, 4);
			tableLayoutPanelContent.Controls.Add(bufferedFlowLayoutPanelQuickFilters, 0, 3);
			tableLayoutPanelContent.Controls.Add(flowLayoutPanelSearchTerm, 0, 0);
			tableLayoutPanelContent.Controls.Add(flowLayoutPanelQueries, 2, 0);
			tableLayoutPanelContent.Controls.Add(flowLayoutPanelMisc, 0, 1);
			tableLayoutPanelContent.Controls.Add(bufferedFlowLayoutPanelRequierements, 0, 2);
			tableLayoutPanelContent.Controls.Add(flowLayoutPanelLeftColumn, 0, 4);
			tableLayoutPanelContent.Controls.Add(tableLayoutPanelBottom, 0, 5);
			tableLayoutPanelContent.Dock = DockStyle.Fill;
			tableLayoutPanelContent.Location = new Point(15, 25);
			tableLayoutPanelContent.Name = "tableLayoutPanelContent";
			tableLayoutPanelContent.RowCount = 6;
			tableLayoutPanelContent.RowStyles.Add(new RowStyle());
			tableLayoutPanelContent.RowStyles.Add(new RowStyle());
			tableLayoutPanelContent.RowStyles.Add(new RowStyle());
			tableLayoutPanelContent.RowStyles.Add(new RowStyle());
			tableLayoutPanelContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			tableLayoutPanelContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
			tableLayoutPanelContent.Size = new Size(1385, 835);
			tableLayoutPanelContent.TabIndex = 18;
			// 
			// flowLayoutPanelMainAutoScroll
			// 
			flowLayoutPanelMainAutoScroll.AutoScroll = true;
			tableLayoutPanelContent.SetColumnSpan(flowLayoutPanelMainAutoScroll, 2);
			flowLayoutPanelMainAutoScroll.Controls.Add(flowLayoutPanelMain);
			flowLayoutPanelMainAutoScroll.Dock = DockStyle.Fill;
			flowLayoutPanelMainAutoScroll.Location = new Point(134, 153);
			flowLayoutPanelMainAutoScroll.Margin = new Padding(0);
			flowLayoutPanelMainAutoScroll.Name = "flowLayoutPanelMainAutoScroll";
			flowLayoutPanelMainAutoScroll.Size = new Size(1251, 592);
			flowLayoutPanelMainAutoScroll.TabIndex = 21;
			// 
			// bufferedFlowLayoutPanelQuickFilters
			// 
			bufferedFlowLayoutPanelQuickFilters.AutoSize = true;
			bufferedFlowLayoutPanelQuickFilters.BorderStyle = BorderStyle.FixedSingle;
			tableLayoutPanelContent.SetColumnSpan(bufferedFlowLayoutPanelQuickFilters, 3);
			bufferedFlowLayoutPanelQuickFilters.Controls.Add(scalingCheckBoxHavingPrefix);
			bufferedFlowLayoutPanelQuickFilters.Controls.Add(scalingCheckBoxHavingSuffix);
			bufferedFlowLayoutPanelQuickFilters.Controls.Add(scalingCheckBoxHavingRelic);
			bufferedFlowLayoutPanelQuickFilters.Controls.Add(scalingCheckBoxHavingCharm);
			bufferedFlowLayoutPanelQuickFilters.Controls.Add(scalingCheckBoxIsSetItem);
			bufferedFlowLayoutPanelQuickFilters.Dock = DockStyle.Fill;
			bufferedFlowLayoutPanelQuickFilters.Location = new Point(3, 126);
			bufferedFlowLayoutPanelQuickFilters.Name = "bufferedFlowLayoutPanelQuickFilters";
			bufferedFlowLayoutPanelQuickFilters.Size = new Size(1379, 24);
			bufferedFlowLayoutPanelQuickFilters.TabIndex = 32;
			// 
			// scalingCheckBoxHavingPrefix
			// 
			scalingCheckBoxHavingPrefix.AutoSize = true;
			scalingCheckBoxHavingPrefix.Font = new Font("Microsoft Sans Serif", 9F);
			scalingCheckBoxHavingPrefix.ForeColor = Color.Gold;
			scalingCheckBoxHavingPrefix.Location = new Point(3, 0);
			scalingCheckBoxHavingPrefix.Margin = new Padding(3, 0, 3, 0);
			scalingCheckBoxHavingPrefix.Name = "scalingCheckBoxHavingPrefix";
			scalingCheckBoxHavingPrefix.Size = new Size(116, 22);
			scalingCheckBoxHavingPrefix.TabIndex = 0;
			scalingCheckBoxHavingPrefix.Text = "Having Prefix";
			scalingCheckBoxHavingPrefix.UseVisualStyleBackColor = true;
			scalingCheckBoxHavingPrefix.CheckedChanged += scalingCheckBoxQuickFilters_CheckedChanged;
			// 
			// scalingCheckBoxHavingSuffix
			// 
			scalingCheckBoxHavingSuffix.AutoSize = true;
			scalingCheckBoxHavingSuffix.Font = new Font("Microsoft Sans Serif", 9F);
			scalingCheckBoxHavingSuffix.ForeColor = Color.Gold;
			scalingCheckBoxHavingSuffix.Location = new Point(125, 0);
			scalingCheckBoxHavingSuffix.Margin = new Padding(3, 0, 3, 0);
			scalingCheckBoxHavingSuffix.Name = "scalingCheckBoxHavingSuffix";
			scalingCheckBoxHavingSuffix.Size = new Size(115, 22);
			scalingCheckBoxHavingSuffix.TabIndex = 1;
			scalingCheckBoxHavingSuffix.Text = "Having Suffix";
			scalingCheckBoxHavingSuffix.UseVisualStyleBackColor = true;
			scalingCheckBoxHavingSuffix.CheckedChanged += scalingCheckBoxQuickFilters_CheckedChanged;
			// 
			// scalingCheckBoxHavingRelic
			// 
			scalingCheckBoxHavingRelic.AutoSize = true;
			scalingCheckBoxHavingRelic.Font = new Font("Microsoft Sans Serif", 9F);
			scalingCheckBoxHavingRelic.ForeColor = Color.Gold;
			scalingCheckBoxHavingRelic.Location = new Point(246, 0);
			scalingCheckBoxHavingRelic.Margin = new Padding(3, 0, 3, 0);
			scalingCheckBoxHavingRelic.Name = "scalingCheckBoxHavingRelic";
			scalingCheckBoxHavingRelic.Size = new Size(112, 22);
			scalingCheckBoxHavingRelic.TabIndex = 2;
			scalingCheckBoxHavingRelic.Text = "Having Relic";
			scalingCheckBoxHavingRelic.UseVisualStyleBackColor = true;
			scalingCheckBoxHavingRelic.CheckedChanged += scalingCheckBoxQuickFilters_CheckedChanged;
			// 
			// scalingCheckBoxHavingCharm
			// 
			scalingCheckBoxHavingCharm.AutoSize = true;
			scalingCheckBoxHavingCharm.Font = new Font("Microsoft Sans Serif", 9F);
			scalingCheckBoxHavingCharm.ForeColor = Color.Gold;
			scalingCheckBoxHavingCharm.Location = new Point(364, 0);
			scalingCheckBoxHavingCharm.Margin = new Padding(3, 0, 3, 0);
			scalingCheckBoxHavingCharm.Name = "scalingCheckBoxHavingCharm";
			scalingCheckBoxHavingCharm.Size = new Size(124, 22);
			scalingCheckBoxHavingCharm.TabIndex = 3;
			scalingCheckBoxHavingCharm.Text = "Having Charm";
			scalingCheckBoxHavingCharm.UseVisualStyleBackColor = true;
			scalingCheckBoxHavingCharm.CheckedChanged += scalingCheckBoxQuickFilters_CheckedChanged;
			// 
			// scalingCheckBoxIsSetItem
			// 
			scalingCheckBoxIsSetItem.AutoSize = true;
			scalingCheckBoxIsSetItem.Font = new Font("Microsoft Sans Serif", 9F);
			scalingCheckBoxIsSetItem.ForeColor = Color.Gold;
			scalingCheckBoxIsSetItem.Location = new Point(494, 0);
			scalingCheckBoxIsSetItem.Margin = new Padding(3, 0, 3, 0);
			scalingCheckBoxIsSetItem.Name = "scalingCheckBoxIsSetItem";
			scalingCheckBoxIsSetItem.Size = new Size(84, 22);
			scalingCheckBoxIsSetItem.TabIndex = 4;
			scalingCheckBoxIsSetItem.Text = "Set Item";
			scalingCheckBoxIsSetItem.UseVisualStyleBackColor = true;
			scalingCheckBoxIsSetItem.CheckedChanged += scalingCheckBoxQuickFilters_CheckedChanged;
			// 
			// flowLayoutPanelMisc
			// 
			flowLayoutPanelMisc.AutoSize = true;
			flowLayoutPanelMisc.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			flowLayoutPanelMisc.BorderStyle = BorderStyle.FixedSingle;
			tableLayoutPanelContent.SetColumnSpan(flowLayoutPanelMisc, 3);
			flowLayoutPanelMisc.Controls.Add(scalingLabelMaxVisibleElement);
			flowLayoutPanelMisc.Controls.Add(numericUpDownMaxElement);
			flowLayoutPanelMisc.Controls.Add(scalingCheckBoxReduceDuringSelection);
			flowLayoutPanelMisc.Controls.Add(buttonCollapseAll);
			flowLayoutPanelMisc.Controls.Add(buttonExpandAll);
			flowLayoutPanelMisc.Controls.Add(scalingLabelOperator);
			flowLayoutPanelMisc.Controls.Add(scalingComboBoxOperator);
			flowLayoutPanelMisc.Controls.Add(scalingLabelFiltersSelected);
			flowLayoutPanelMisc.Controls.Add(scalingLabelFilterCategories);
			flowLayoutPanelMisc.Controls.Add(scalingTextBoxFilterCategories);
			flowLayoutPanelMisc.Dock = DockStyle.Fill;
			flowLayoutPanelMisc.Location = new Point(3, 47);
			flowLayoutPanelMisc.Name = "flowLayoutPanelMisc";
			flowLayoutPanelMisc.Size = new Size(1379, 36);
			flowLayoutPanelMisc.TabIndex = 18;
			// 
			// scalingLabelMaxVisibleElement
			// 
			scalingLabelMaxVisibleElement.AutoSize = true;
			scalingLabelMaxVisibleElement.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelMaxVisibleElement.ForeColor = Color.Gold;
			scalingLabelMaxVisibleElement.Location = new Point(3, 7);
			scalingLabelMaxVisibleElement.Margin = new Padding(3, 7, 3, 0);
			scalingLabelMaxVisibleElement.Name = "scalingLabelMaxVisibleElement";
			scalingLabelMaxVisibleElement.Size = new Size(183, 18);
			scalingLabelMaxVisibleElement.TabIndex = 5;
			scalingLabelMaxVisibleElement.Text = "Visible elements/category :";
			scalingLabelMaxVisibleElement.UseMnemonic = false;
			// 
			// numericUpDownMaxElement
			// 
			numericUpDownMaxElement.BorderStyle = BorderStyle.FixedSingle;
			numericUpDownMaxElement.Font = new Font("Microsoft Sans Serif", 8.25F);
			numericUpDownMaxElement.Location = new Point(192, 3);
			numericUpDownMaxElement.Maximum = new decimal(new int[] { 150, 0, 0, 0 });
			numericUpDownMaxElement.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
			numericUpDownMaxElement.Name = "numericUpDownMaxElement";
			numericUpDownMaxElement.Size = new Size(43, 23);
			numericUpDownMaxElement.TabIndex = 4;
			numericUpDownMaxElement.Value = new decimal(new int[] { 5, 0, 0, 0 });
			numericUpDownMaxElement.ValueChanged += numericUpDownMaxElement_ValueChanged;
			// 
			// scalingCheckBoxReduceDuringSelection
			// 
			scalingCheckBoxReduceDuringSelection.AutoSize = true;
			scalingCheckBoxReduceDuringSelection.Checked = true;
			scalingCheckBoxReduceDuringSelection.CheckState = CheckState.Checked;
			scalingCheckBoxReduceDuringSelection.Font = new Font("Microsoft Sans Serif", 9F);
			scalingCheckBoxReduceDuringSelection.ForeColor = Color.Gold;
			scalingCheckBoxReduceDuringSelection.Location = new Point(251, 7);
			scalingCheckBoxReduceDuringSelection.Margin = new Padding(13, 7, 3, 3);
			scalingCheckBoxReduceDuringSelection.Name = "scalingCheckBoxReduceDuringSelection";
			scalingCheckBoxReduceDuringSelection.Size = new Size(261, 22);
			scalingCheckBoxReduceDuringSelection.TabIndex = 3;
			scalingCheckBoxReduceDuringSelection.Text = "Reduce categories during selection";
			scalingCheckBoxReduceDuringSelection.UseVisualStyleBackColor = true;
			scalingCheckBoxReduceDuringSelection.CheckedChanged += scalingCheckBoxReduceDuringSelection_CheckedChanged;
			// 
			// buttonCollapseAll
			// 
			buttonCollapseAll.AutoSize = true;
			buttonCollapseAll.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			buttonCollapseAll.BackColor = Color.FromArgb(211, 190, 107);
			buttonCollapseAll.FlatStyle = FlatStyle.Popup;
			buttonCollapseAll.ForeColor = Color.FromArgb(51, 44, 28);
			buttonCollapseAll.Location = new Point(518, 3);
			buttonCollapseAll.Name = "buttonCollapseAll";
			buttonCollapseAll.Size = new Size(95, 28);
			buttonCollapseAll.TabIndex = 6;
			buttonCollapseAll.Text = "Collapse All";
			buttonCollapseAll.UseVisualStyleBackColor = false;
			buttonCollapseAll.Click += buttonCollapseAll_Click;
			// 
			// buttonExpandAll
			// 
			buttonExpandAll.AutoSize = true;
			buttonExpandAll.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			buttonExpandAll.BackColor = Color.FromArgb(211, 190, 107);
			buttonExpandAll.FlatStyle = FlatStyle.Popup;
			buttonExpandAll.ForeColor = Color.FromArgb(51, 44, 28);
			buttonExpandAll.Location = new Point(619, 3);
			buttonExpandAll.Name = "buttonExpandAll";
			buttonExpandAll.Size = new Size(86, 28);
			buttonExpandAll.TabIndex = 7;
			buttonExpandAll.Text = "Expand All";
			buttonExpandAll.UseVisualStyleBackColor = false;
			buttonExpandAll.Click += buttonExpandAll_Click;
			// 
			// scalingLabelOperator
			// 
			scalingLabelOperator.AutoSize = true;
			scalingLabelOperator.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelOperator.ForeColor = Color.Gold;
			scalingLabelOperator.Location = new Point(711, 7);
			scalingLabelOperator.Margin = new Padding(3, 7, 3, 0);
			scalingLabelOperator.Name = "scalingLabelOperator";
			scalingLabelOperator.Size = new Size(75, 18);
			scalingLabelOperator.TabIndex = 17;
			scalingLabelOperator.Text = "Operator :";
			scalingLabelOperator.UseMnemonic = false;
			// 
			// scalingComboBoxOperator
			// 
			scalingComboBoxOperator.DrawMode = DrawMode.OwnerDrawFixed;
			scalingComboBoxOperator.DropDownStyle = ComboBoxStyle.DropDownList;
			scalingComboBoxOperator.Font = new Font("Microsoft Sans Serif", 9F);
			scalingComboBoxOperator.FormattingEnabled = true;
			scalingComboBoxOperator.Items.AddRange(new object[] { "And", "Or" });
			scalingComboBoxOperator.Location = new Point(792, 3);
			scalingComboBoxOperator.MaxDropDownItems = 2;
			scalingComboBoxOperator.Name = "scalingComboBoxOperator";
			scalingComboBoxOperator.Size = new Size(60, 25);
			scalingComboBoxOperator.TabIndex = 18;
			scalingComboBoxOperator.SelectionChangeCommitted += scalingComboBoxOperator_SelectionChangeCommitted;
			// 
			// scalingLabelFiltersSelected
			// 
			scalingLabelFiltersSelected.AutoSize = true;
			scalingLabelFiltersSelected.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelFiltersSelected.ForeColor = Color.Gold;
			scalingLabelFiltersSelected.Location = new Point(858, 7);
			scalingLabelFiltersSelected.Margin = new Padding(3, 7, 3, 0);
			scalingLabelFiltersSelected.Name = "scalingLabelFiltersSelected";
			scalingLabelFiltersSelected.Size = new Size(124, 18);
			scalingLabelFiltersSelected.TabIndex = 8;
			scalingLabelFiltersSelected.Tag = "{0} filters selected";
			scalingLabelFiltersSelected.Text = "{0} filters selected";
			scalingLabelFiltersSelected.UseMnemonic = false;
			scalingLabelFiltersSelected.MouseEnter += scalingLabelFiltersSelected_MouseEnter;
			scalingLabelFiltersSelected.MouseLeave += scalingLabelFiltersSelected_MouseLeave;
			// 
			// scalingLabelFilterCategories
			// 
			scalingLabelFilterCategories.AutoSize = true;
			scalingLabelFilterCategories.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelFilterCategories.ForeColor = Color.Gold;
			scalingLabelFilterCategories.Location = new Point(988, 7);
			scalingLabelFilterCategories.Margin = new Padding(3, 7, 3, 0);
			scalingLabelFilterCategories.Name = "scalingLabelFilterCategories";
			scalingLabelFilterCategories.Size = new Size(121, 18);
			scalingLabelFilterCategories.TabIndex = 19;
			scalingLabelFilterCategories.Text = "Filter categories :";
			scalingLabelFilterCategories.UseMnemonic = false;
			// 
			// scalingTextBoxFilterCategories
			// 
			scalingTextBoxFilterCategories.BorderStyle = BorderStyle.FixedSingle;
			scalingTextBoxFilterCategories.Font = new Font("Microsoft Sans Serif", 9F);
			scalingTextBoxFilterCategories.Location = new Point(1115, 3);
			scalingTextBoxFilterCategories.Name = "scalingTextBoxFilterCategories";
			scalingTextBoxFilterCategories.Size = new Size(200, 24);
			scalingTextBoxFilterCategories.TabIndex = 20;
			scalingTextBoxFilterCategories.TextChanged += scalingTextBoxFilterCategories_TextChanged;
			// 
			// bufferedFlowLayoutPanelRequierements
			// 
			bufferedFlowLayoutPanelRequierements.AutoSize = true;
			bufferedFlowLayoutPanelRequierements.BorderStyle = BorderStyle.FixedSingle;
			tableLayoutPanelContent.SetColumnSpan(bufferedFlowLayoutPanelRequierements, 3);
			bufferedFlowLayoutPanelRequierements.Controls.Add(tableLayoutPanelRequierements);
			bufferedFlowLayoutPanelRequierements.Dock = DockStyle.Fill;
			bufferedFlowLayoutPanelRequierements.Location = new Point(3, 89);
			bufferedFlowLayoutPanelRequierements.Name = "bufferedFlowLayoutPanelRequierements";
			bufferedFlowLayoutPanelRequierements.Size = new Size(1379, 31);
			bufferedFlowLayoutPanelRequierements.TabIndex = 5;
			// 
			// tableLayoutPanelRequierements
			// 
			tableLayoutPanelRequierements.AutoSize = true;
			tableLayoutPanelRequierements.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			tableLayoutPanelRequierements.BackColor = Color.Transparent;
			tableLayoutPanelRequierements.ColumnCount = 5;
			tableLayoutPanelRequierements.ColumnStyles.Add(new ColumnStyle());
			tableLayoutPanelRequierements.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
			tableLayoutPanelRequierements.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
			tableLayoutPanelRequierements.ColumnStyles.Add(new ColumnStyle());
			tableLayoutPanelRequierements.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
			tableLayoutPanelRequierements.Controls.Add(scalingCheckBoxMinReq, 0, 0);
			tableLayoutPanelRequierements.Controls.Add(flowLayoutPanelMin, 1, 0);
			tableLayoutPanelRequierements.Controls.Add(scalingCheckBoxMaxReq, 3, 0);
			tableLayoutPanelRequierements.Controls.Add(flowLayoutPanelMax, 4, 0);
			tableLayoutPanelRequierements.Location = new Point(0, 0);
			tableLayoutPanelRequierements.Margin = new Padding(0);
			tableLayoutPanelRequierements.Name = "tableLayoutPanelRequierements";
			tableLayoutPanelRequierements.RowCount = 1;
			tableLayoutPanelRequierements.RowStyles.Add(new RowStyle());
			tableLayoutPanelRequierements.Size = new Size(1110, 29);
			tableLayoutPanelRequierements.TabIndex = 25;
			// 
			// scalingCheckBoxMinReq
			// 
			scalingCheckBoxMinReq.AutoSize = true;
			scalingCheckBoxMinReq.Font = new Font("Microsoft Sans Serif", 9F);
			scalingCheckBoxMinReq.ForeColor = Color.Gold;
			scalingCheckBoxMinReq.Location = new Point(3, 3);
			scalingCheckBoxMinReq.Name = "scalingCheckBoxMinReq";
			scalingCheckBoxMinReq.Size = new Size(158, 22);
			scalingCheckBoxMinReq.TabIndex = 3;
			scalingCheckBoxMinReq.Text = "Min Requierement :";
			scalingCheckBoxMinReq.UseVisualStyleBackColor = true;
			scalingCheckBoxMinReq.CheckedChanged += scalingCheckBoxMinMaxReq_CheckedChanged;
			// 
			// flowLayoutPanelMin
			// 
			flowLayoutPanelMin.AutoSize = true;
			flowLayoutPanelMin.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			flowLayoutPanelMin.Controls.Add(scalingLabelMinLvl);
			flowLayoutPanelMin.Controls.Add(numericUpDownMinLvl);
			flowLayoutPanelMin.Controls.Add(scalingLabelMinStr);
			flowLayoutPanelMin.Controls.Add(numericUpDownMinStr);
			flowLayoutPanelMin.Controls.Add(scalingLabelMinDex);
			flowLayoutPanelMin.Controls.Add(numericUpDownMinDex);
			flowLayoutPanelMin.Controls.Add(scalingLabelMinInt);
			flowLayoutPanelMin.Controls.Add(numericUpDownMinInt);
			flowLayoutPanelMin.Location = new Point(167, 3);
			flowLayoutPanelMin.Name = "flowLayoutPanelMin";
			flowLayoutPanelMin.Size = new Size(358, 23);
			flowLayoutPanelMin.TabIndex = 4;
			flowLayoutPanelMin.WrapContents = false;
			// 
			// scalingLabelMinLvl
			// 
			scalingLabelMinLvl.AutoSize = true;
			scalingLabelMinLvl.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelMinLvl.ForeColor = Color.Gold;
			scalingLabelMinLvl.Location = new Point(3, 0);
			scalingLabelMinLvl.Name = "scalingLabelMinLvl";
			scalingLabelMinLvl.Size = new Size(50, 18);
			scalingLabelMinLvl.TabIndex = 0;
			scalingLabelMinLvl.Text = "Level :";
			// 
			// numericUpDownMinLvl
			// 
			numericUpDownMinLvl.Font = new Font("Microsoft Sans Serif", 8.25F);
			numericUpDownMinLvl.Increment = new decimal(new int[] { 5, 0, 0, 0 });
			numericUpDownMinLvl.Location = new Point(56, 0);
			numericUpDownMinLvl.Margin = new Padding(0);
			numericUpDownMinLvl.Name = "numericUpDownMinLvl";
			numericUpDownMinLvl.Size = new Size(44, 23);
			numericUpDownMinLvl.TabIndex = 1;
			numericUpDownMinLvl.Value = new decimal(new int[] { 100, 0, 0, 0 });
			numericUpDownMinLvl.ValueChanged += numericUpDownMinMax_ValueChanged;
			// 
			// scalingLabelMinStr
			// 
			scalingLabelMinStr.AutoSize = true;
			scalingLabelMinStr.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelMinStr.ForeColor = Color.Gold;
			scalingLabelMinStr.Location = new Point(103, 0);
			scalingLabelMinStr.Name = "scalingLabelMinStr";
			scalingLabelMinStr.Size = new Size(35, 18);
			scalingLabelMinStr.TabIndex = 2;
			scalingLabelMinStr.Text = "Str :";
			// 
			// numericUpDownMinStr
			// 
			numericUpDownMinStr.Font = new Font("Microsoft Sans Serif", 8.25F);
			numericUpDownMinStr.Increment = new decimal(new int[] { 5, 0, 0, 0 });
			numericUpDownMinStr.Location = new Point(141, 0);
			numericUpDownMinStr.Margin = new Padding(0);
			numericUpDownMinStr.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
			numericUpDownMinStr.Name = "numericUpDownMinStr";
			numericUpDownMinStr.Size = new Size(44, 23);
			numericUpDownMinStr.TabIndex = 3;
			numericUpDownMinStr.Value = new decimal(new int[] { 9000, 0, 0, 0 });
			numericUpDownMinStr.ValueChanged += numericUpDownMinMax_ValueChanged;
			// 
			// scalingLabelMinDex
			// 
			scalingLabelMinDex.AutoSize = true;
			scalingLabelMinDex.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelMinDex.ForeColor = Color.Gold;
			scalingLabelMinDex.Location = new Point(188, 0);
			scalingLabelMinDex.Name = "scalingLabelMinDex";
			scalingLabelMinDex.Size = new Size(42, 18);
			scalingLabelMinDex.TabIndex = 4;
			scalingLabelMinDex.Text = "Dex :";
			// 
			// numericUpDownMinDex
			// 
			numericUpDownMinDex.Font = new Font("Microsoft Sans Serif", 8.25F);
			numericUpDownMinDex.Increment = new decimal(new int[] { 5, 0, 0, 0 });
			numericUpDownMinDex.Location = new Point(233, 0);
			numericUpDownMinDex.Margin = new Padding(0);
			numericUpDownMinDex.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
			numericUpDownMinDex.Name = "numericUpDownMinDex";
			numericUpDownMinDex.Size = new Size(44, 23);
			numericUpDownMinDex.TabIndex = 5;
			numericUpDownMinDex.Value = new decimal(new int[] { 9000, 0, 0, 0 });
			numericUpDownMinDex.ValueChanged += numericUpDownMinMax_ValueChanged;
			// 
			// scalingLabelMinInt
			// 
			scalingLabelMinInt.AutoSize = true;
			scalingLabelMinInt.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelMinInt.ForeColor = Color.Gold;
			scalingLabelMinInt.Location = new Point(280, 0);
			scalingLabelMinInt.Name = "scalingLabelMinInt";
			scalingLabelMinInt.Size = new Size(31, 18);
			scalingLabelMinInt.TabIndex = 6;
			scalingLabelMinInt.Text = "Int :";
			// 
			// numericUpDownMinInt
			// 
			numericUpDownMinInt.Font = new Font("Microsoft Sans Serif", 8.25F);
			numericUpDownMinInt.Increment = new decimal(new int[] { 5, 0, 0, 0 });
			numericUpDownMinInt.Location = new Point(314, 0);
			numericUpDownMinInt.Margin = new Padding(0);
			numericUpDownMinInt.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
			numericUpDownMinInt.Name = "numericUpDownMinInt";
			numericUpDownMinInt.Size = new Size(44, 23);
			numericUpDownMinInt.TabIndex = 7;
			numericUpDownMinInt.Value = new decimal(new int[] { 9000, 0, 0, 0 });
			numericUpDownMinInt.ValueChanged += numericUpDownMinMax_ValueChanged;
			// 
			// scalingCheckBoxMaxReq
			// 
			scalingCheckBoxMaxReq.AutoSize = true;
			scalingCheckBoxMaxReq.Font = new Font("Microsoft Sans Serif", 9F);
			scalingCheckBoxMaxReq.ForeColor = Color.Gold;
			scalingCheckBoxMaxReq.Location = new Point(581, 3);
			scalingCheckBoxMaxReq.Name = "scalingCheckBoxMaxReq";
			scalingCheckBoxMaxReq.Size = new Size(162, 22);
			scalingCheckBoxMaxReq.TabIndex = 5;
			scalingCheckBoxMaxReq.Text = "Max Requierement :";
			scalingCheckBoxMaxReq.UseVisualStyleBackColor = true;
			scalingCheckBoxMaxReq.CheckedChanged += scalingCheckBoxMinMaxReq_CheckedChanged;
			// 
			// flowLayoutPanelMax
			// 
			flowLayoutPanelMax.AutoSize = true;
			flowLayoutPanelMax.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			flowLayoutPanelMax.Controls.Add(scalingLabelMaxLvl);
			flowLayoutPanelMax.Controls.Add(numericUpDownMaxLvl);
			flowLayoutPanelMax.Controls.Add(scalingLabelMaxStr);
			flowLayoutPanelMax.Controls.Add(numericUpDownMaxStr);
			flowLayoutPanelMax.Controls.Add(scalingLabelMaxDex);
			flowLayoutPanelMax.Controls.Add(numericUpDownMaxDex);
			flowLayoutPanelMax.Controls.Add(scalingLabelMaxInt);
			flowLayoutPanelMax.Controls.Add(numericUpDownMaxInt);
			flowLayoutPanelMax.Location = new Point(749, 3);
			flowLayoutPanelMax.Name = "flowLayoutPanelMax";
			flowLayoutPanelMax.Size = new Size(358, 23);
			flowLayoutPanelMax.TabIndex = 25;
			flowLayoutPanelMax.WrapContents = false;
			// 
			// scalingLabelMaxLvl
			// 
			scalingLabelMaxLvl.AutoSize = true;
			scalingLabelMaxLvl.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelMaxLvl.ForeColor = Color.Gold;
			scalingLabelMaxLvl.Location = new Point(3, 0);
			scalingLabelMaxLvl.Name = "scalingLabelMaxLvl";
			scalingLabelMaxLvl.Size = new Size(50, 18);
			scalingLabelMaxLvl.TabIndex = 0;
			scalingLabelMaxLvl.Text = "Level :";
			// 
			// numericUpDownMaxLvl
			// 
			numericUpDownMaxLvl.Font = new Font("Microsoft Sans Serif", 8.25F);
			numericUpDownMaxLvl.Increment = new decimal(new int[] { 5, 0, 0, 0 });
			numericUpDownMaxLvl.Location = new Point(56, 0);
			numericUpDownMaxLvl.Margin = new Padding(0);
			numericUpDownMaxLvl.Name = "numericUpDownMaxLvl";
			numericUpDownMaxLvl.Size = new Size(44, 23);
			numericUpDownMaxLvl.TabIndex = 1;
			numericUpDownMaxLvl.Value = new decimal(new int[] { 100, 0, 0, 0 });
			numericUpDownMaxLvl.ValueChanged += numericUpDownMinMax_ValueChanged;
			// 
			// scalingLabelMaxStr
			// 
			scalingLabelMaxStr.AutoSize = true;
			scalingLabelMaxStr.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelMaxStr.ForeColor = Color.Gold;
			scalingLabelMaxStr.Location = new Point(103, 0);
			scalingLabelMaxStr.Name = "scalingLabelMaxStr";
			scalingLabelMaxStr.Size = new Size(35, 18);
			scalingLabelMaxStr.TabIndex = 2;
			scalingLabelMaxStr.Text = "Str :";
			// 
			// numericUpDownMaxStr
			// 
			numericUpDownMaxStr.Font = new Font("Microsoft Sans Serif", 8.25F);
			numericUpDownMaxStr.Increment = new decimal(new int[] { 5, 0, 0, 0 });
			numericUpDownMaxStr.Location = new Point(141, 0);
			numericUpDownMaxStr.Margin = new Padding(0);
			numericUpDownMaxStr.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
			numericUpDownMaxStr.Name = "numericUpDownMaxStr";
			numericUpDownMaxStr.Size = new Size(44, 23);
			numericUpDownMaxStr.TabIndex = 3;
			numericUpDownMaxStr.Value = new decimal(new int[] { 9000, 0, 0, 0 });
			numericUpDownMaxStr.ValueChanged += numericUpDownMinMax_ValueChanged;
			// 
			// scalingLabelMaxDex
			// 
			scalingLabelMaxDex.AutoSize = true;
			scalingLabelMaxDex.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelMaxDex.ForeColor = Color.Gold;
			scalingLabelMaxDex.Location = new Point(188, 0);
			scalingLabelMaxDex.Name = "scalingLabelMaxDex";
			scalingLabelMaxDex.Size = new Size(42, 18);
			scalingLabelMaxDex.TabIndex = 4;
			scalingLabelMaxDex.Text = "Dex :";
			// 
			// numericUpDownMaxDex
			// 
			numericUpDownMaxDex.Font = new Font("Microsoft Sans Serif", 8.25F);
			numericUpDownMaxDex.Increment = new decimal(new int[] { 5, 0, 0, 0 });
			numericUpDownMaxDex.Location = new Point(233, 0);
			numericUpDownMaxDex.Margin = new Padding(0);
			numericUpDownMaxDex.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
			numericUpDownMaxDex.Name = "numericUpDownMaxDex";
			numericUpDownMaxDex.Size = new Size(44, 23);
			numericUpDownMaxDex.TabIndex = 5;
			numericUpDownMaxDex.Value = new decimal(new int[] { 9000, 0, 0, 0 });
			numericUpDownMaxDex.ValueChanged += numericUpDownMinMax_ValueChanged;
			// 
			// scalingLabelMaxInt
			// 
			scalingLabelMaxInt.AutoSize = true;
			scalingLabelMaxInt.Font = new Font("Microsoft Sans Serif", 9F);
			scalingLabelMaxInt.ForeColor = Color.Gold;
			scalingLabelMaxInt.Location = new Point(280, 0);
			scalingLabelMaxInt.Name = "scalingLabelMaxInt";
			scalingLabelMaxInt.Size = new Size(31, 18);
			scalingLabelMaxInt.TabIndex = 6;
			scalingLabelMaxInt.Text = "Int :";
			// 
			// numericUpDownMaxInt
			// 
			numericUpDownMaxInt.Font = new Font("Microsoft Sans Serif", 8.25F);
			numericUpDownMaxInt.Increment = new decimal(new int[] { 5, 0, 0, 0 });
			numericUpDownMaxInt.Location = new Point(314, 0);
			numericUpDownMaxInt.Margin = new Padding(0);
			numericUpDownMaxInt.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
			numericUpDownMaxInt.Name = "numericUpDownMaxInt";
			numericUpDownMaxInt.Size = new Size(44, 23);
			numericUpDownMaxInt.TabIndex = 7;
			numericUpDownMaxInt.Value = new decimal(new int[] { 9000, 0, 0, 0 });
			numericUpDownMaxInt.ValueChanged += numericUpDownMinMax_ValueChanged;
			// 
			// flowLayoutPanelLeftColumn
			// 
			flowLayoutPanelLeftColumn.AutoSize = true;
			flowLayoutPanelLeftColumn.BorderStyle = BorderStyle.FixedSingle;
			flowLayoutPanelLeftColumn.Controls.Add(scalingButtonMenuCharacters);
			flowLayoutPanelLeftColumn.Controls.Add(scalingButtonMenuVaults);
			flowLayoutPanelLeftColumn.Controls.Add(scalingButtonMenuType);
			flowLayoutPanelLeftColumn.Controls.Add(scalingButtonMenuRarity);
			flowLayoutPanelLeftColumn.Controls.Add(scalingButtonMenuOrigin);
			flowLayoutPanelLeftColumn.Controls.Add(scalingButtonMenuQuality);
			flowLayoutPanelLeftColumn.Controls.Add(scalingButtonMenuStyle);
			flowLayoutPanelLeftColumn.Controls.Add(scalingButtonMenuSetItems);
			flowLayoutPanelLeftColumn.Controls.Add(scalingButtonMenuWithCharm);
			flowLayoutPanelLeftColumn.Controls.Add(scalingButtonMenuWithRelic);
			flowLayoutPanelLeftColumn.Controls.Add(scalingButtonMenuAttribute);
			flowLayoutPanelLeftColumn.Controls.Add(scalingButtonMenuPrefixName);
			flowLayoutPanelLeftColumn.Controls.Add(scalingButtonMenuPrefixAttribute);
			flowLayoutPanelLeftColumn.Controls.Add(scalingButtonMenuBaseAttribute);
			flowLayoutPanelLeftColumn.Controls.Add(scalingButtonMenuSuffixName);
			flowLayoutPanelLeftColumn.Controls.Add(scalingButtonMenuSuffixAttribute);
			flowLayoutPanelLeftColumn.FlowDirection = FlowDirection.TopDown;
			flowLayoutPanelLeftColumn.Location = new Point(3, 156);
			flowLayoutPanelLeftColumn.MinimumSize = new Size(128, 578);
			flowLayoutPanelLeftColumn.Name = "flowLayoutPanelLeftColumn";
			flowLayoutPanelLeftColumn.Size = new Size(128, 578);
			flowLayoutPanelLeftColumn.TabIndex = 19;
			// 
			// scalingButtonMenuCharacters
			// 
			scalingButtonMenuCharacters.BackColor = Color.Transparent;
			scalingButtonMenuCharacters.DownBitmap = (Bitmap)resources.GetObject("scalingButtonMenuCharacters.DownBitmap");
			scalingButtonMenuCharacters.FlatAppearance.BorderSize = 0;
			scalingButtonMenuCharacters.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuCharacters.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuCharacters.FlatStyle = FlatStyle.Flat;
			scalingButtonMenuCharacters.Font = new Font("Microsoft Sans Serif", 7.5F);
			scalingButtonMenuCharacters.ForeColor = Color.FromArgb(51, 44, 28);
			scalingButtonMenuCharacters.Image = (Image)resources.GetObject("scalingButtonMenuCharacters.Image");
			scalingButtonMenuCharacters.Location = new Point(3, 3);
			scalingButtonMenuCharacters.Name = "scalingButtonMenuCharacters";
			scalingButtonMenuCharacters.OverBitmap = (Bitmap)resources.GetObject("scalingButtonMenuCharacters.OverBitmap");
			scalingButtonMenuCharacters.Size = new Size(120, 30);
			scalingButtonMenuCharacters.SizeToGraphic = false;
			scalingButtonMenuCharacters.TabIndex = 8;
			scalingButtonMenuCharacters.Text = "Characters";
			scalingButtonMenuCharacters.UpBitmap = (Bitmap)resources.GetObject("scalingButtonMenuCharacters.UpBitmap");
			scalingButtonMenuCharacters.UseCustomGraphic = true;
			scalingButtonMenuCharacters.UseVisualStyleBackColor = false;
			scalingButtonMenuCharacters.Click += scalingButtonMenu_Click;
			// 
			// scalingButtonMenuVaults
			// 
			scalingButtonMenuVaults.BackColor = Color.Transparent;
			scalingButtonMenuVaults.DownBitmap = (Bitmap)resources.GetObject("scalingButtonMenuVaults.DownBitmap");
			scalingButtonMenuVaults.FlatAppearance.BorderSize = 0;
			scalingButtonMenuVaults.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuVaults.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuVaults.FlatStyle = FlatStyle.Flat;
			scalingButtonMenuVaults.Font = new Font("Microsoft Sans Serif", 7.5F);
			scalingButtonMenuVaults.ForeColor = Color.FromArgb(51, 44, 28);
			scalingButtonMenuVaults.Image = (Image)resources.GetObject("scalingButtonMenuVaults.Image");
			scalingButtonMenuVaults.Location = new Point(3, 39);
			scalingButtonMenuVaults.Name = "scalingButtonMenuVaults";
			scalingButtonMenuVaults.OverBitmap = (Bitmap)resources.GetObject("scalingButtonMenuVaults.OverBitmap");
			scalingButtonMenuVaults.Size = new Size(120, 30);
			scalingButtonMenuVaults.SizeToGraphic = false;
			scalingButtonMenuVaults.TabIndex = 9;
			scalingButtonMenuVaults.Text = "Vaults";
			scalingButtonMenuVaults.UpBitmap = (Bitmap)resources.GetObject("scalingButtonMenuVaults.UpBitmap");
			scalingButtonMenuVaults.UseCustomGraphic = true;
			scalingButtonMenuVaults.UseVisualStyleBackColor = false;
			scalingButtonMenuVaults.Click += scalingButtonMenu_Click;
			// 
			// scalingButtonMenuType
			// 
			scalingButtonMenuType.BackColor = Color.Transparent;
			scalingButtonMenuType.DownBitmap = (Bitmap)resources.GetObject("scalingButtonMenuType.DownBitmap");
			scalingButtonMenuType.FlatAppearance.BorderSize = 0;
			scalingButtonMenuType.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuType.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuType.FlatStyle = FlatStyle.Flat;
			scalingButtonMenuType.Font = new Font("Microsoft Sans Serif", 7.5F);
			scalingButtonMenuType.ForeColor = Color.FromArgb(51, 44, 28);
			scalingButtonMenuType.Image = (Image)resources.GetObject("scalingButtonMenuType.Image");
			scalingButtonMenuType.Location = new Point(3, 75);
			scalingButtonMenuType.Name = "scalingButtonMenuType";
			scalingButtonMenuType.OverBitmap = (Bitmap)resources.GetObject("scalingButtonMenuType.OverBitmap");
			scalingButtonMenuType.Size = new Size(120, 30);
			scalingButtonMenuType.SizeToGraphic = false;
			scalingButtonMenuType.TabIndex = 10;
			scalingButtonMenuType.Text = "Type";
			scalingButtonMenuType.UpBitmap = (Bitmap)resources.GetObject("scalingButtonMenuType.UpBitmap");
			scalingButtonMenuType.UseCustomGraphic = true;
			scalingButtonMenuType.UseVisualStyleBackColor = false;
			scalingButtonMenuType.Click += scalingButtonMenu_Click;
			// 
			// scalingButtonMenuRarity
			// 
			scalingButtonMenuRarity.BackColor = Color.Transparent;
			scalingButtonMenuRarity.DownBitmap = (Bitmap)resources.GetObject("scalingButtonMenuRarity.DownBitmap");
			scalingButtonMenuRarity.FlatAppearance.BorderSize = 0;
			scalingButtonMenuRarity.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuRarity.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuRarity.FlatStyle = FlatStyle.Flat;
			scalingButtonMenuRarity.Font = new Font("Microsoft Sans Serif", 7.5F);
			scalingButtonMenuRarity.ForeColor = Color.FromArgb(51, 44, 28);
			scalingButtonMenuRarity.Image = (Image)resources.GetObject("scalingButtonMenuRarity.Image");
			scalingButtonMenuRarity.Location = new Point(3, 111);
			scalingButtonMenuRarity.Name = "scalingButtonMenuRarity";
			scalingButtonMenuRarity.OverBitmap = (Bitmap)resources.GetObject("scalingButtonMenuRarity.OverBitmap");
			scalingButtonMenuRarity.Size = new Size(120, 30);
			scalingButtonMenuRarity.SizeToGraphic = false;
			scalingButtonMenuRarity.TabIndex = 11;
			scalingButtonMenuRarity.Text = "Rarity";
			scalingButtonMenuRarity.UpBitmap = (Bitmap)resources.GetObject("scalingButtonMenuRarity.UpBitmap");
			scalingButtonMenuRarity.UseCustomGraphic = true;
			scalingButtonMenuRarity.UseVisualStyleBackColor = false;
			scalingButtonMenuRarity.Click += scalingButtonMenu_Click;
			// 
			// scalingButtonMenuOrigin
			// 
			scalingButtonMenuOrigin.BackColor = Color.Transparent;
			scalingButtonMenuOrigin.DownBitmap = (Bitmap)resources.GetObject("scalingButtonMenuOrigin.DownBitmap");
			scalingButtonMenuOrigin.FlatAppearance.BorderSize = 0;
			scalingButtonMenuOrigin.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuOrigin.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuOrigin.FlatStyle = FlatStyle.Flat;
			scalingButtonMenuOrigin.Font = new Font("Microsoft Sans Serif", 7.5F);
			scalingButtonMenuOrigin.ForeColor = Color.FromArgb(51, 44, 28);
			scalingButtonMenuOrigin.Image = (Image)resources.GetObject("scalingButtonMenuOrigin.Image");
			scalingButtonMenuOrigin.Location = new Point(3, 147);
			scalingButtonMenuOrigin.Name = "scalingButtonMenuOrigin";
			scalingButtonMenuOrigin.OverBitmap = (Bitmap)resources.GetObject("scalingButtonMenuOrigin.OverBitmap");
			scalingButtonMenuOrigin.Size = new Size(120, 30);
			scalingButtonMenuOrigin.SizeToGraphic = false;
			scalingButtonMenuOrigin.TabIndex = 22;
			scalingButtonMenuOrigin.Text = "Origin";
			scalingButtonMenuOrigin.UpBitmap = (Bitmap)resources.GetObject("scalingButtonMenuOrigin.UpBitmap");
			scalingButtonMenuOrigin.UseCustomGraphic = true;
			scalingButtonMenuOrigin.UseVisualStyleBackColor = false;
			scalingButtonMenuOrigin.Click += scalingButtonMenu_Click;
			// 
			// scalingButtonMenuQuality
			// 
			scalingButtonMenuQuality.BackColor = Color.Transparent;
			scalingButtonMenuQuality.DownBitmap = (Bitmap)resources.GetObject("scalingButtonMenuQuality.DownBitmap");
			scalingButtonMenuQuality.FlatAppearance.BorderSize = 0;
			scalingButtonMenuQuality.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuQuality.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuQuality.FlatStyle = FlatStyle.Flat;
			scalingButtonMenuQuality.Font = new Font("Microsoft Sans Serif", 7.5F);
			scalingButtonMenuQuality.ForeColor = Color.FromArgb(51, 44, 28);
			scalingButtonMenuQuality.Image = (Image)resources.GetObject("scalingButtonMenuQuality.Image");
			scalingButtonMenuQuality.Location = new Point(3, 183);
			scalingButtonMenuQuality.Name = "scalingButtonMenuQuality";
			scalingButtonMenuQuality.OverBitmap = (Bitmap)resources.GetObject("scalingButtonMenuQuality.OverBitmap");
			scalingButtonMenuQuality.Size = new Size(120, 30);
			scalingButtonMenuQuality.SizeToGraphic = false;
			scalingButtonMenuQuality.TabIndex = 21;
			scalingButtonMenuQuality.Text = "Quality";
			scalingButtonMenuQuality.UpBitmap = (Bitmap)resources.GetObject("scalingButtonMenuQuality.UpBitmap");
			scalingButtonMenuQuality.UseCustomGraphic = true;
			scalingButtonMenuQuality.UseVisualStyleBackColor = false;
			scalingButtonMenuQuality.Click += scalingButtonMenu_Click;
			// 
			// scalingButtonMenuStyle
			// 
			scalingButtonMenuStyle.BackColor = Color.Transparent;
			scalingButtonMenuStyle.DownBitmap = (Bitmap)resources.GetObject("scalingButtonMenuStyle.DownBitmap");
			scalingButtonMenuStyle.FlatAppearance.BorderSize = 0;
			scalingButtonMenuStyle.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuStyle.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuStyle.FlatStyle = FlatStyle.Flat;
			scalingButtonMenuStyle.Font = new Font("Microsoft Sans Serif", 7.5F);
			scalingButtonMenuStyle.ForeColor = Color.FromArgb(51, 44, 28);
			scalingButtonMenuStyle.Image = (Image)resources.GetObject("scalingButtonMenuStyle.Image");
			scalingButtonMenuStyle.Location = new Point(3, 219);
			scalingButtonMenuStyle.Name = "scalingButtonMenuStyle";
			scalingButtonMenuStyle.OverBitmap = (Bitmap)resources.GetObject("scalingButtonMenuStyle.OverBitmap");
			scalingButtonMenuStyle.Size = new Size(120, 30);
			scalingButtonMenuStyle.SizeToGraphic = false;
			scalingButtonMenuStyle.TabIndex = 12;
			scalingButtonMenuStyle.Text = "Style";
			scalingButtonMenuStyle.UpBitmap = (Bitmap)resources.GetObject("scalingButtonMenuStyle.UpBitmap");
			scalingButtonMenuStyle.UseCustomGraphic = true;
			scalingButtonMenuStyle.UseVisualStyleBackColor = false;
			scalingButtonMenuStyle.Click += scalingButtonMenu_Click;
			// 
			// scalingButtonMenuSetItems
			// 
			scalingButtonMenuSetItems.BackColor = Color.Transparent;
			scalingButtonMenuSetItems.DownBitmap = (Bitmap)resources.GetObject("scalingButtonMenuSetItems.DownBitmap");
			scalingButtonMenuSetItems.FlatAppearance.BorderSize = 0;
			scalingButtonMenuSetItems.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuSetItems.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuSetItems.FlatStyle = FlatStyle.Flat;
			scalingButtonMenuSetItems.Font = new Font("Microsoft Sans Serif", 7.5F);
			scalingButtonMenuSetItems.ForeColor = Color.FromArgb(51, 44, 28);
			scalingButtonMenuSetItems.Image = (Image)resources.GetObject("scalingButtonMenuSetItems.Image");
			scalingButtonMenuSetItems.Location = new Point(3, 255);
			scalingButtonMenuSetItems.Name = "scalingButtonMenuSetItems";
			scalingButtonMenuSetItems.OverBitmap = (Bitmap)resources.GetObject("scalingButtonMenuSetItems.OverBitmap");
			scalingButtonMenuSetItems.Size = new Size(120, 30);
			scalingButtonMenuSetItems.SizeToGraphic = false;
			scalingButtonMenuSetItems.TabIndex = 23;
			scalingButtonMenuSetItems.Text = "Sets";
			scalingButtonMenuSetItems.UpBitmap = (Bitmap)resources.GetObject("scalingButtonMenuSetItems.UpBitmap");
			scalingButtonMenuSetItems.UseCustomGraphic = true;
			scalingButtonMenuSetItems.UseVisualStyleBackColor = false;
			scalingButtonMenuSetItems.Click += scalingButtonMenu_Click;
			// 
			// scalingButtonMenuWithCharm
			// 
			scalingButtonMenuWithCharm.BackColor = Color.Transparent;
			scalingButtonMenuWithCharm.DownBitmap = (Bitmap)resources.GetObject("scalingButtonMenuWithCharm.DownBitmap");
			scalingButtonMenuWithCharm.FlatAppearance.BorderSize = 0;
			scalingButtonMenuWithCharm.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuWithCharm.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuWithCharm.FlatStyle = FlatStyle.Flat;
			scalingButtonMenuWithCharm.Font = new Font("Microsoft Sans Serif", 7.5F);
			scalingButtonMenuWithCharm.ForeColor = Color.FromArgb(51, 44, 28);
			scalingButtonMenuWithCharm.Image = (Image)resources.GetObject("scalingButtonMenuWithCharm.Image");
			scalingButtonMenuWithCharm.Location = new Point(3, 291);
			scalingButtonMenuWithCharm.Name = "scalingButtonMenuWithCharm";
			scalingButtonMenuWithCharm.OverBitmap = (Bitmap)resources.GetObject("scalingButtonMenuWithCharm.OverBitmap");
			scalingButtonMenuWithCharm.Size = new Size(120, 30);
			scalingButtonMenuWithCharm.SizeToGraphic = false;
			scalingButtonMenuWithCharm.TabIndex = 13;
			scalingButtonMenuWithCharm.Text = "Having Charm";
			scalingButtonMenuWithCharm.UpBitmap = (Bitmap)resources.GetObject("scalingButtonMenuWithCharm.UpBitmap");
			scalingButtonMenuWithCharm.UseCustomGraphic = true;
			scalingButtonMenuWithCharm.UseVisualStyleBackColor = false;
			scalingButtonMenuWithCharm.Click += scalingButtonMenu_Click;
			// 
			// scalingButtonMenuWithRelic
			// 
			scalingButtonMenuWithRelic.BackColor = Color.Transparent;
			scalingButtonMenuWithRelic.DownBitmap = (Bitmap)resources.GetObject("scalingButtonMenuWithRelic.DownBitmap");
			scalingButtonMenuWithRelic.FlatAppearance.BorderSize = 0;
			scalingButtonMenuWithRelic.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuWithRelic.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuWithRelic.FlatStyle = FlatStyle.Flat;
			scalingButtonMenuWithRelic.Font = new Font("Microsoft Sans Serif", 7.5F);
			scalingButtonMenuWithRelic.ForeColor = Color.FromArgb(51, 44, 28);
			scalingButtonMenuWithRelic.Image = (Image)resources.GetObject("scalingButtonMenuWithRelic.Image");
			scalingButtonMenuWithRelic.Location = new Point(3, 327);
			scalingButtonMenuWithRelic.Name = "scalingButtonMenuWithRelic";
			scalingButtonMenuWithRelic.OverBitmap = (Bitmap)resources.GetObject("scalingButtonMenuWithRelic.OverBitmap");
			scalingButtonMenuWithRelic.Size = new Size(120, 30);
			scalingButtonMenuWithRelic.SizeToGraphic = false;
			scalingButtonMenuWithRelic.TabIndex = 14;
			scalingButtonMenuWithRelic.Text = "Having Relic";
			scalingButtonMenuWithRelic.UpBitmap = (Bitmap)resources.GetObject("scalingButtonMenuWithRelic.UpBitmap");
			scalingButtonMenuWithRelic.UseCustomGraphic = true;
			scalingButtonMenuWithRelic.UseVisualStyleBackColor = false;
			scalingButtonMenuWithRelic.Click += scalingButtonMenu_Click;
			// 
			// scalingButtonMenuAttribute
			// 
			scalingButtonMenuAttribute.BackColor = Color.Transparent;
			scalingButtonMenuAttribute.DownBitmap = (Bitmap)resources.GetObject("scalingButtonMenuAttribute.DownBitmap");
			scalingButtonMenuAttribute.FlatAppearance.BorderSize = 0;
			scalingButtonMenuAttribute.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuAttribute.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuAttribute.FlatStyle = FlatStyle.Flat;
			scalingButtonMenuAttribute.Font = new Font("Microsoft Sans Serif", 7.5F);
			scalingButtonMenuAttribute.ForeColor = Color.FromArgb(51, 44, 28);
			scalingButtonMenuAttribute.Image = (Image)resources.GetObject("scalingButtonMenuAttribute.Image");
			scalingButtonMenuAttribute.Location = new Point(3, 363);
			scalingButtonMenuAttribute.Name = "scalingButtonMenuAttribute";
			scalingButtonMenuAttribute.OverBitmap = (Bitmap)resources.GetObject("scalingButtonMenuAttribute.OverBitmap");
			scalingButtonMenuAttribute.Size = new Size(120, 30);
			scalingButtonMenuAttribute.SizeToGraphic = false;
			scalingButtonMenuAttribute.TabIndex = 15;
			scalingButtonMenuAttribute.Text = "All Attributes";
			scalingButtonMenuAttribute.UpBitmap = (Bitmap)resources.GetObject("scalingButtonMenuAttribute.UpBitmap");
			scalingButtonMenuAttribute.UseCustomGraphic = true;
			scalingButtonMenuAttribute.UseVisualStyleBackColor = false;
			scalingButtonMenuAttribute.Click += scalingButtonMenu_Click;
			// 
			// scalingButtonMenuPrefixName
			// 
			scalingButtonMenuPrefixName.BackColor = Color.Transparent;
			scalingButtonMenuPrefixName.DownBitmap = (Bitmap)resources.GetObject("scalingButtonMenuPrefixName.DownBitmap");
			scalingButtonMenuPrefixName.FlatAppearance.BorderSize = 0;
			scalingButtonMenuPrefixName.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuPrefixName.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuPrefixName.FlatStyle = FlatStyle.Flat;
			scalingButtonMenuPrefixName.Font = new Font("Microsoft Sans Serif", 7.5F);
			scalingButtonMenuPrefixName.ForeColor = Color.FromArgb(51, 44, 28);
			scalingButtonMenuPrefixName.Image = (Image)resources.GetObject("scalingButtonMenuPrefixName.Image");
			scalingButtonMenuPrefixName.Location = new Point(3, 399);
			scalingButtonMenuPrefixName.Name = "scalingButtonMenuPrefixName";
			scalingButtonMenuPrefixName.OverBitmap = (Bitmap)resources.GetObject("scalingButtonMenuPrefixName.OverBitmap");
			scalingButtonMenuPrefixName.Size = new Size(120, 30);
			scalingButtonMenuPrefixName.SizeToGraphic = false;
			scalingButtonMenuPrefixName.TabIndex = 16;
			scalingButtonMenuPrefixName.Text = "Prefix Name";
			scalingButtonMenuPrefixName.UpBitmap = (Bitmap)resources.GetObject("scalingButtonMenuPrefixName.UpBitmap");
			scalingButtonMenuPrefixName.UseCustomGraphic = true;
			scalingButtonMenuPrefixName.UseVisualStyleBackColor = false;
			scalingButtonMenuPrefixName.Click += scalingButtonMenu_Click;
			// 
			// scalingButtonMenuPrefixAttribute
			// 
			scalingButtonMenuPrefixAttribute.BackColor = Color.Transparent;
			scalingButtonMenuPrefixAttribute.DownBitmap = (Bitmap)resources.GetObject("scalingButtonMenuPrefixAttribute.DownBitmap");
			scalingButtonMenuPrefixAttribute.FlatAppearance.BorderSize = 0;
			scalingButtonMenuPrefixAttribute.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuPrefixAttribute.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuPrefixAttribute.FlatStyle = FlatStyle.Flat;
			scalingButtonMenuPrefixAttribute.Font = new Font("Microsoft Sans Serif", 7.5F);
			scalingButtonMenuPrefixAttribute.ForeColor = Color.FromArgb(51, 44, 28);
			scalingButtonMenuPrefixAttribute.Image = (Image)resources.GetObject("scalingButtonMenuPrefixAttribute.Image");
			scalingButtonMenuPrefixAttribute.Location = new Point(3, 435);
			scalingButtonMenuPrefixAttribute.Name = "scalingButtonMenuPrefixAttribute";
			scalingButtonMenuPrefixAttribute.OverBitmap = (Bitmap)resources.GetObject("scalingButtonMenuPrefixAttribute.OverBitmap");
			scalingButtonMenuPrefixAttribute.Size = new Size(120, 30);
			scalingButtonMenuPrefixAttribute.SizeToGraphic = false;
			scalingButtonMenuPrefixAttribute.TabIndex = 19;
			scalingButtonMenuPrefixAttribute.Text = "Prefix Attribute";
			scalingButtonMenuPrefixAttribute.UpBitmap = (Bitmap)resources.GetObject("scalingButtonMenuPrefixAttribute.UpBitmap");
			scalingButtonMenuPrefixAttribute.UseCustomGraphic = true;
			scalingButtonMenuPrefixAttribute.UseVisualStyleBackColor = false;
			scalingButtonMenuPrefixAttribute.Click += scalingButtonMenu_Click;
			// 
			// scalingButtonMenuBaseAttribute
			// 
			scalingButtonMenuBaseAttribute.BackColor = Color.Transparent;
			scalingButtonMenuBaseAttribute.DownBitmap = (Bitmap)resources.GetObject("scalingButtonMenuBaseAttribute.DownBitmap");
			scalingButtonMenuBaseAttribute.FlatAppearance.BorderSize = 0;
			scalingButtonMenuBaseAttribute.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuBaseAttribute.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuBaseAttribute.FlatStyle = FlatStyle.Flat;
			scalingButtonMenuBaseAttribute.Font = new Font("Microsoft Sans Serif", 7.5F);
			scalingButtonMenuBaseAttribute.ForeColor = Color.FromArgb(51, 44, 28);
			scalingButtonMenuBaseAttribute.Image = (Image)resources.GetObject("scalingButtonMenuBaseAttribute.Image");
			scalingButtonMenuBaseAttribute.Location = new Point(3, 471);
			scalingButtonMenuBaseAttribute.Name = "scalingButtonMenuBaseAttribute";
			scalingButtonMenuBaseAttribute.OverBitmap = (Bitmap)resources.GetObject("scalingButtonMenuBaseAttribute.OverBitmap");
			scalingButtonMenuBaseAttribute.Size = new Size(120, 30);
			scalingButtonMenuBaseAttribute.SizeToGraphic = false;
			scalingButtonMenuBaseAttribute.TabIndex = 17;
			scalingButtonMenuBaseAttribute.Text = "Base Attribute";
			scalingButtonMenuBaseAttribute.UpBitmap = (Bitmap)resources.GetObject("scalingButtonMenuBaseAttribute.UpBitmap");
			scalingButtonMenuBaseAttribute.UseCustomGraphic = true;
			scalingButtonMenuBaseAttribute.UseVisualStyleBackColor = false;
			scalingButtonMenuBaseAttribute.Click += scalingButtonMenu_Click;
			// 
			// scalingButtonMenuSuffixName
			// 
			scalingButtonMenuSuffixName.BackColor = Color.Transparent;
			scalingButtonMenuSuffixName.DownBitmap = (Bitmap)resources.GetObject("scalingButtonMenuSuffixName.DownBitmap");
			scalingButtonMenuSuffixName.FlatAppearance.BorderSize = 0;
			scalingButtonMenuSuffixName.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuSuffixName.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuSuffixName.FlatStyle = FlatStyle.Flat;
			scalingButtonMenuSuffixName.Font = new Font("Microsoft Sans Serif", 7.5F);
			scalingButtonMenuSuffixName.ForeColor = Color.FromArgb(51, 44, 28);
			scalingButtonMenuSuffixName.Image = (Image)resources.GetObject("scalingButtonMenuSuffixName.Image");
			scalingButtonMenuSuffixName.Location = new Point(3, 507);
			scalingButtonMenuSuffixName.Name = "scalingButtonMenuSuffixName";
			scalingButtonMenuSuffixName.OverBitmap = (Bitmap)resources.GetObject("scalingButtonMenuSuffixName.OverBitmap");
			scalingButtonMenuSuffixName.Size = new Size(120, 30);
			scalingButtonMenuSuffixName.SizeToGraphic = false;
			scalingButtonMenuSuffixName.TabIndex = 18;
			scalingButtonMenuSuffixName.Text = "Suffix Name";
			scalingButtonMenuSuffixName.UpBitmap = (Bitmap)resources.GetObject("scalingButtonMenuSuffixName.UpBitmap");
			scalingButtonMenuSuffixName.UseCustomGraphic = true;
			scalingButtonMenuSuffixName.UseVisualStyleBackColor = false;
			scalingButtonMenuSuffixName.Click += scalingButtonMenu_Click;
			// 
			// scalingButtonMenuSuffixAttribute
			// 
			scalingButtonMenuSuffixAttribute.BackColor = Color.Transparent;
			scalingButtonMenuSuffixAttribute.DownBitmap = (Bitmap)resources.GetObject("scalingButtonMenuSuffixAttribute.DownBitmap");
			scalingButtonMenuSuffixAttribute.FlatAppearance.BorderSize = 0;
			scalingButtonMenuSuffixAttribute.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuSuffixAttribute.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 51, 44, 28);
			scalingButtonMenuSuffixAttribute.FlatStyle = FlatStyle.Flat;
			scalingButtonMenuSuffixAttribute.Font = new Font("Microsoft Sans Serif", 7.5F);
			scalingButtonMenuSuffixAttribute.ForeColor = Color.FromArgb(51, 44, 28);
			scalingButtonMenuSuffixAttribute.Image = (Image)resources.GetObject("scalingButtonMenuSuffixAttribute.Image");
			scalingButtonMenuSuffixAttribute.Location = new Point(3, 543);
			scalingButtonMenuSuffixAttribute.Name = "scalingButtonMenuSuffixAttribute";
			scalingButtonMenuSuffixAttribute.OverBitmap = (Bitmap)resources.GetObject("scalingButtonMenuSuffixAttribute.OverBitmap");
			scalingButtonMenuSuffixAttribute.Size = new Size(120, 30);
			scalingButtonMenuSuffixAttribute.SizeToGraphic = false;
			scalingButtonMenuSuffixAttribute.TabIndex = 20;
			scalingButtonMenuSuffixAttribute.Text = "Suffix Attribute";
			scalingButtonMenuSuffixAttribute.UpBitmap = (Bitmap)resources.GetObject("scalingButtonMenuSuffixAttribute.UpBitmap");
			scalingButtonMenuSuffixAttribute.UseCustomGraphic = true;
			scalingButtonMenuSuffixAttribute.UseVisualStyleBackColor = false;
			scalingButtonMenuSuffixAttribute.Click += scalingButtonMenu_Click;
			// 
			// backgroundWorkerBuildDB
			// 
			backgroundWorkerBuildDB.WorkerReportsProgress = true;
			backgroundWorkerBuildDB.DoWork += backgroundWorkerBuildDB_DoWork;
			backgroundWorkerBuildDB.ProgressChanged += backgroundWorkerBuildDB_ProgressChanged;
			backgroundWorkerBuildDB.RunWorkerCompleted += backgroundWorkerBuildDB_RunWorkerCompleted;
			// 
			// typeAssistantSearchBox
			// 
			typeAssistantSearchBox.Idled += scalingTextBoxSearchTerm_TextChanged_Idled;
			// 
			// typeAssistantFilterCategories
			// 
			typeAssistantFilterCategories.Idled += typeAssistantFilterCategories_Idled;
			// 
			// SearchDialogAdvanced
			// 
			AcceptButton = applyButton;
			AutoScaleDimensions = new SizeF(9F, 18F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = Color.FromArgb(46, 31, 21);
			CancelButton = cancelButton;
			ClientSize = new Size(1415, 870);
			Controls.Add(tableLayoutPanelContent);
			DrawCustomBorder = true;
			Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
			ForeColor = Color.White;
			FormBorderStyle = FormBorderStyle.None;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "SearchDialogAdvanced";
			Padding = new Padding(15, 25, 15, 10);
			ResizeCustomAllowed = true;
			ShowIcon = false;
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Search for an Item";
			TitleTextColor = Color.FromArgb(51, 44, 28);
			TopMost = true;
			Load += SearchDialogAdvanced_Load;
			Shown += SearchDialogShown;
			Controls.SetChildIndex(tableLayoutPanelContent, 0);
			tableLayoutPanelBottom.ResumeLayout(false);
			scalingLabelProgressPanelAlignText.ResumeLayout(false);
			flowLayoutPanelCharacters.ResumeLayout(false);
			flowLayoutPanelCharacters.PerformLayout();
			flowLayoutPanelItemType.ResumeLayout(false);
			flowLayoutPanelItemType.PerformLayout();
			flowLayoutPanelItemAttributes.ResumeLayout(false);
			flowLayoutPanelItemAttributes.PerformLayout();
			flowLayoutPanelRarity.ResumeLayout(false);
			flowLayoutPanelRarity.PerformLayout();
			flowLayoutPanelPrefixAttributes.ResumeLayout(false);
			flowLayoutPanelPrefixAttributes.PerformLayout();
			flowLayoutPanelSuffixAttributes.ResumeLayout(false);
			flowLayoutPanelSuffixAttributes.PerformLayout();
			flowLayoutPanelBaseAttributes.ResumeLayout(false);
			flowLayoutPanelBaseAttributes.PerformLayout();
			flowLayoutPanelMain.ResumeLayout(false);
			flowLayoutPanelMain.PerformLayout();
			flowLayoutPanelInVaults.ResumeLayout(false);
			flowLayoutPanelInVaults.PerformLayout();
			flowLayoutPanelOrigin.ResumeLayout(false);
			flowLayoutPanelOrigin.PerformLayout();
			flowLayoutPanelQuality.ResumeLayout(false);
			flowLayoutPanelQuality.PerformLayout();
			flowLayoutPanelStyle.ResumeLayout(false);
			flowLayoutPanelStyle.PerformLayout();
			flowLayoutPanelSetItems.ResumeLayout(false);
			flowLayoutPanelSetItems.PerformLayout();
			flowLayoutPanelWithCharm.ResumeLayout(false);
			flowLayoutPanelWithCharm.PerformLayout();
			flowLayoutPanelWithRelic.ResumeLayout(false);
			flowLayoutPanelWithRelic.PerformLayout();
			flowLayoutPanelPrefixName.ResumeLayout(false);
			flowLayoutPanelPrefixName.PerformLayout();
			flowLayoutPanelSuffixName.ResumeLayout(false);
			flowLayoutPanelSuffixName.PerformLayout();
			flowLayoutPanelSearchTerm.ResumeLayout(false);
			flowLayoutPanelSearchTerm.PerformLayout();
			flowLayoutPanelQueries.ResumeLayout(false);
			flowLayoutPanelQueries.PerformLayout();
			tableLayoutPanelContent.ResumeLayout(false);
			tableLayoutPanelContent.PerformLayout();
			flowLayoutPanelMainAutoScroll.ResumeLayout(false);
			flowLayoutPanelMainAutoScroll.PerformLayout();
			bufferedFlowLayoutPanelQuickFilters.ResumeLayout(false);
			bufferedFlowLayoutPanelQuickFilters.PerformLayout();
			flowLayoutPanelMisc.ResumeLayout(false);
			flowLayoutPanelMisc.PerformLayout();
			((System.ComponentModel.ISupportInitialize)numericUpDownMaxElement).EndInit();
			bufferedFlowLayoutPanelRequierements.ResumeLayout(false);
			bufferedFlowLayoutPanelRequierements.PerformLayout();
			tableLayoutPanelRequierements.ResumeLayout(false);
			tableLayoutPanelRequierements.PerformLayout();
			flowLayoutPanelMin.ResumeLayout(false);
			flowLayoutPanelMin.PerformLayout();
			((System.ComponentModel.ISupportInitialize)numericUpDownMinLvl).EndInit();
			((System.ComponentModel.ISupportInitialize)numericUpDownMinStr).EndInit();
			((System.ComponentModel.ISupportInitialize)numericUpDownMinDex).EndInit();
			((System.ComponentModel.ISupportInitialize)numericUpDownMinInt).EndInit();
			flowLayoutPanelMax.ResumeLayout(false);
			flowLayoutPanelMax.PerformLayout();
			((System.ComponentModel.ISupportInitialize)numericUpDownMaxLvl).EndInit();
			((System.ComponentModel.ISupportInitialize)numericUpDownMaxStr).EndInit();
			((System.ComponentModel.ISupportInitialize)numericUpDownMaxDex).EndInit();
			((System.ComponentModel.ISupportInitialize)numericUpDownMaxInt).EndInit();
			flowLayoutPanelLeftColumn.ResumeLayout(false);
			ResumeLayout(false);
			PerformLayout();

		}

		#endregion

		private VaultProgressBar vaultProgressBar;
		private ScalingLabel scalingLabelProgress;
		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelBottom;
		private ScalingCheckedListBox scalingCheckedListBoxCharacters;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelCharacters;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelItemType;
		private ScalingLabel scalingLabelItemType;
		private ScalingCheckedListBox scalingCheckedListBoxItemType;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelItemAttributes;
		private ScalingLabel scalingLabelItemAttributes;
		private ScalingCheckedListBox scalingCheckedListBoxItemAttributes;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelRarity;
		private ScalingLabel scalingLabelRarity;
		private ScalingCheckedListBox scalingCheckedListBoxRarity;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelPrefixAttributes;
		private ScalingLabel scalingLabelPrefixAttributes;
		private ScalingCheckedListBox scalingCheckedListBoxPrefixAttributes;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelSuffixAttributes;
		private ScalingLabel scalingLabelSuffixAttributes;
		private ScalingCheckedListBox scalingCheckedListBoxSuffixAttributes;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelBaseAttributes;
		private ScalingLabel scalingLabelBaseAttributes;
		private ScalingCheckedListBox scalingCheckedListBoxBaseAttributes;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelMain;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelSearchTerm;
		private ScalingLabel scalingLabelSearchTerm;
		private ScalingTextBox scalingTextBoxSearchTerm;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelQueries;
		private ScalingLabel scalingLabelQueries;
		private ScalingComboBox scalingComboBoxQueryList;
		private ScalingButton scalingButtonQuerySave;
		private ScalingButton scalingButtonQueryDelete;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelContent;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelSuffixName;
		private ScalingLabel scalingLabelSuffixName;
		private ScalingCheckedListBox scalingCheckedListBoxSuffixName;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelPrefixName;
		private ScalingLabel scalingLabelPrefixName;
		private ScalingCheckedListBox scalingCheckedListBoxPrefixName;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelWithCharm;
		private ScalingLabel scalingLabelWithCharm;
		private ScalingCheckedListBox scalingCheckedListBoxWithCharm;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelWithRelic;
		private ScalingLabel scalingLabelWithRelic;
		private ScalingCheckedListBox scalingCheckedListBoxWithRelic;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelInVaults;
		private ScalingLabel scalingLabelInVaults;
		private ScalingCheckedListBox scalingCheckedListBoxVaults;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelMisc;
		private ScalingCheckBox scalingCheckBoxReduceDuringSelection;
		private ScalingButton scalingButtonReset;
		private ScalingLabel scalingLabelMaxVisibleElement;
		private System.Windows.Forms.NumericUpDown numericUpDownMaxElement;
		private System.ComponentModel.BackgroundWorker backgroundWorkerBuildDB;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelStyle;
		private ScalingLabel scalingLabelStyle;
		private ScalingCheckedListBox scalingCheckedListBoxStyle;
		private System.Windows.Forms.Button buttonCollapseAll;
		private System.Windows.Forms.Button buttonExpandAll;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelLeftColumn;
		private ScalingButton scalingButtonMenuCharacters;
		private ScalingButton scalingButtonMenuVaults;
		private ScalingButton scalingButtonMenuType;
		private ScalingButton scalingButtonMenuRarity;
		private ScalingButton scalingButtonMenuStyle;
		private ScalingButton scalingButtonMenuWithCharm;
		private ScalingButton scalingButtonMenuWithRelic;
		private ScalingButton scalingButtonMenuAttribute;
		private ScalingButton scalingButtonMenuPrefixName;
		private ScalingButton scalingButtonMenuBaseAttribute;
		private ScalingButton scalingButtonMenuSuffixName;
		private ScalingButton scalingButtonMenuPrefixAttribute;
		private ScalingButton scalingButtonMenuSuffixAttribute;
		private ScalingLabel scalingLabelFiltersSelected;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelMainAutoScroll;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelQuality;
		private ScalingLabel scalingLabelQuality;
		private ScalingCheckedListBox scalingCheckedListBoxQuality;
		private ScalingButton scalingButtonMenuQuality;
		private ScalingLabel scalingLabelOperator;
		private ScalingComboBox scalingComboBoxOperator;
		private Components.TypeAssistant typeAssistantSearchBox;
		private System.Windows.Forms.Panel scalingLabelProgressPanelAlignText;
		private ScalingLabel scalingLabelFilterCategories;
		private ScalingTextBox scalingTextBoxFilterCategories;
		private TypeAssistant typeAssistantFilterCategories;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelRequierements;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelMax;
        private ScalingLabel scalingLabelMaxLvl;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxLvl;
        private ScalingLabel scalingLabelMaxStr;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxStr;
        private ScalingLabel scalingLabelMaxDex;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxDex;
        private ScalingLabel scalingLabelMaxInt;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxInt;
        private ScalingCheckBox scalingCheckBoxMaxReq;
        private ScalingCheckBox scalingCheckBoxMinReq;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelMin;
        private ScalingLabel scalingLabelMinLvl;
        private System.Windows.Forms.NumericUpDown numericUpDownMinLvl;
        private ScalingLabel scalingLabelMinStr;
        private System.Windows.Forms.NumericUpDown numericUpDownMinStr;
        private ScalingLabel scalingLabelMinDex;
        private System.Windows.Forms.NumericUpDown numericUpDownMinDex;
        private ScalingLabel scalingLabelMinInt;
        private System.Windows.Forms.NumericUpDown numericUpDownMinInt;
        private BufferedFlowLayoutPanel bufferedFlowLayoutPanelQuickFilters;
        private ScalingCheckBox scalingCheckBoxHavingPrefix;
        private ScalingCheckBox scalingCheckBoxHavingSuffix;
        private ScalingCheckBox scalingCheckBoxHavingRelic;
        private ScalingCheckBox scalingCheckBoxHavingCharm;
        private ScalingCheckBox scalingCheckBoxIsSetItem;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelOrigin;
        private ScalingLabel scalingLabelOrigin;
        private ScalingCheckedListBox scalingCheckedListBoxOrigin;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelSetItems;
        private ScalingLabel scalingLabelSetItems;
        private ScalingCheckedListBox scalingCheckedListBoxSetItems;
        private ScalingButton scalingButtonMenuOrigin;
        private ScalingButton scalingButtonMenuSetItems;
        private BufferedFlowLayoutPanel bufferedFlowLayoutPanelRequierements;
    }
}