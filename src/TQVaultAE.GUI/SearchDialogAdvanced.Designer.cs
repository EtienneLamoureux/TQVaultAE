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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SearchDialogAdvanced));
            this.scalingLabelCharacters = new TQVaultAE.GUI.Components.ScalingLabel();
            this.applyButton = new TQVaultAE.GUI.Components.ScalingButton();
            this.cancelButton = new TQVaultAE.GUI.Components.ScalingButton();
            this.vaultProgressBar = new TQVaultAE.GUI.Components.VaultProgressBar();
            this.scalingLabelProgress = new TQVaultAE.GUI.Components.ScalingLabel();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanelBottom = new System.Windows.Forms.TableLayoutPanel();
            this.scalingLabelProgressPanelAlignText = new System.Windows.Forms.Panel();
            this.scalingCheckedListBoxCharacters = new TQVaultAE.GUI.Components.ScalingCheckedListBox();
            this.flowLayoutPanelCharacters = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanelItemType = new System.Windows.Forms.FlowLayoutPanel();
            this.scalingLabelItemType = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingCheckedListBoxItemType = new TQVaultAE.GUI.Components.ScalingCheckedListBox();
            this.flowLayoutPanelItemAttributes = new System.Windows.Forms.FlowLayoutPanel();
            this.scalingLabelItemAttributes = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingCheckedListBoxItemAttributes = new TQVaultAE.GUI.Components.ScalingCheckedListBox();
            this.flowLayoutPanelRarity = new System.Windows.Forms.FlowLayoutPanel();
            this.scalingLabelRarity = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingCheckedListBoxRarity = new TQVaultAE.GUI.Components.ScalingCheckedListBox();
            this.flowLayoutPanelPrefixAttributes = new System.Windows.Forms.FlowLayoutPanel();
            this.scalingLabelPrefixAttributes = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingCheckedListBoxPrefixAttributes = new TQVaultAE.GUI.Components.ScalingCheckedListBox();
            this.flowLayoutPanelSuffixAttributes = new System.Windows.Forms.FlowLayoutPanel();
            this.scalingLabelSuffixAttributes = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingCheckedListBoxSuffixAttributes = new TQVaultAE.GUI.Components.ScalingCheckedListBox();
            this.flowLayoutPanelBaseAttributes = new System.Windows.Forms.FlowLayoutPanel();
            this.scalingLabelBaseAttributes = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingCheckedListBoxBaseAttributes = new TQVaultAE.GUI.Components.ScalingCheckedListBox();
            this.flowLayoutPanelMain = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanelInVaults = new System.Windows.Forms.FlowLayoutPanel();
            this.scalingLabelInVaults = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingCheckedListBoxVaults = new TQVaultAE.GUI.Components.ScalingCheckedListBox();
            this.flowLayoutPanelOrigin = new System.Windows.Forms.FlowLayoutPanel();
            this.scalingLabelOrigin = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingCheckedListBoxOrigin = new TQVaultAE.GUI.Components.ScalingCheckedListBox();
            this.flowLayoutPanelQuality = new System.Windows.Forms.FlowLayoutPanel();
            this.scalingLabelQuality = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingCheckedListBoxQuality = new TQVaultAE.GUI.Components.ScalingCheckedListBox();
            this.flowLayoutPanelStyle = new System.Windows.Forms.FlowLayoutPanel();
            this.scalingLabelStyle = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingCheckedListBoxStyle = new TQVaultAE.GUI.Components.ScalingCheckedListBox();
            this.flowLayoutPanelSetItems = new System.Windows.Forms.FlowLayoutPanel();
            this.scalingLabelSetItems = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingCheckedListBoxSetItems = new TQVaultAE.GUI.Components.ScalingCheckedListBox();
            this.flowLayoutPanelWithCharm = new System.Windows.Forms.FlowLayoutPanel();
            this.scalingLabelWithCharm = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingCheckedListBoxWithCharm = new TQVaultAE.GUI.Components.ScalingCheckedListBox();
            this.flowLayoutPanelWithRelic = new System.Windows.Forms.FlowLayoutPanel();
            this.scalingLabelWithRelic = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingCheckedListBoxWithRelic = new TQVaultAE.GUI.Components.ScalingCheckedListBox();
            this.flowLayoutPanelPrefixName = new System.Windows.Forms.FlowLayoutPanel();
            this.scalingLabelPrefixName = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingCheckedListBoxPrefixName = new TQVaultAE.GUI.Components.ScalingCheckedListBox();
            this.flowLayoutPanelSuffixName = new System.Windows.Forms.FlowLayoutPanel();
            this.scalingLabelSuffixName = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingCheckedListBoxSuffixName = new TQVaultAE.GUI.Components.ScalingCheckedListBox();
            this.flowLayoutPanelSearchTerm = new System.Windows.Forms.FlowLayoutPanel();
            this.scalingLabelSearchTerm = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingTextBoxSearchTerm = new TQVaultAE.GUI.Components.ScalingTextBox();
            this.scalingButtonReset = new TQVaultAE.GUI.Components.ScalingButton();
            this.flowLayoutPanelQueries = new System.Windows.Forms.FlowLayoutPanel();
            this.scalingLabelQueries = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingComboBoxQueryList = new TQVaultAE.GUI.Components.ScalingComboBox();
            this.scalingButtonQuerySave = new TQVaultAE.GUI.Components.ScalingButton();
            this.scalingButtonQueryDelete = new TQVaultAE.GUI.Components.ScalingButton();
            this.tableLayoutPanelContent = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanelMainAutoScroll = new System.Windows.Forms.FlowLayoutPanel();
            this.bufferedFlowLayoutPanelQuickFilters = new TQVaultAE.GUI.Components.BufferedFlowLayoutPanel();
            this.scalingCheckBoxHavingPrefix = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.scalingCheckBoxHavingSuffix = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.scalingCheckBoxHavingRelic = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.scalingCheckBoxHavingCharm = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.scalingCheckBoxIsSetItem = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.flowLayoutPanelMisc = new System.Windows.Forms.FlowLayoutPanel();
            this.scalingLabelMaxVisibleElement = new TQVaultAE.GUI.Components.ScalingLabel();
            this.numericUpDownMaxElement = new System.Windows.Forms.NumericUpDown();
            this.scalingCheckBoxReduceDuringSelection = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.buttonCollapseAll = new System.Windows.Forms.Button();
            this.buttonExpandAll = new System.Windows.Forms.Button();
            this.scalingLabelOperator = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingComboBoxOperator = new TQVaultAE.GUI.Components.ScalingComboBox();
            this.scalingLabelFiltersSelected = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingLabelFilterCategories = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingTextBoxFilterCategories = new TQVaultAE.GUI.Components.ScalingTextBox();
            this.bufferedFlowLayoutPanelRequierements = new TQVaultAE.GUI.Components.BufferedFlowLayoutPanel();
            this.tableLayoutPanelRequierements = new System.Windows.Forms.TableLayoutPanel();
            this.scalingCheckBoxMinReq = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.flowLayoutPanelMin = new System.Windows.Forms.FlowLayoutPanel();
            this.scalingLabelMinLvl = new TQVaultAE.GUI.Components.ScalingLabel();
            this.numericUpDownMinLvl = new System.Windows.Forms.NumericUpDown();
            this.scalingLabelMinStr = new TQVaultAE.GUI.Components.ScalingLabel();
            this.numericUpDownMinStr = new System.Windows.Forms.NumericUpDown();
            this.scalingLabelMinDex = new TQVaultAE.GUI.Components.ScalingLabel();
            this.numericUpDownMinDex = new System.Windows.Forms.NumericUpDown();
            this.scalingLabelMinInt = new TQVaultAE.GUI.Components.ScalingLabel();
            this.numericUpDownMinInt = new System.Windows.Forms.NumericUpDown();
            this.scalingCheckBoxMaxReq = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.flowLayoutPanelMax = new System.Windows.Forms.FlowLayoutPanel();
            this.scalingLabelMaxLvl = new TQVaultAE.GUI.Components.ScalingLabel();
            this.numericUpDownMaxLvl = new System.Windows.Forms.NumericUpDown();
            this.scalingLabelMaxStr = new TQVaultAE.GUI.Components.ScalingLabel();
            this.numericUpDownMaxStr = new System.Windows.Forms.NumericUpDown();
            this.scalingLabelMaxDex = new TQVaultAE.GUI.Components.ScalingLabel();
            this.numericUpDownMaxDex = new System.Windows.Forms.NumericUpDown();
            this.scalingLabelMaxInt = new TQVaultAE.GUI.Components.ScalingLabel();
            this.numericUpDownMaxInt = new System.Windows.Forms.NumericUpDown();
            this.flowLayoutPanelLeftColumn = new System.Windows.Forms.FlowLayoutPanel();
            this.scalingButtonMenuCharacters = new TQVaultAE.GUI.Components.ScalingButton();
            this.scalingButtonMenuVaults = new TQVaultAE.GUI.Components.ScalingButton();
            this.scalingButtonMenuType = new TQVaultAE.GUI.Components.ScalingButton();
            this.scalingButtonMenuRarity = new TQVaultAE.GUI.Components.ScalingButton();
            this.scalingButtonMenuOrigin = new TQVaultAE.GUI.Components.ScalingButton();
            this.scalingButtonMenuQuality = new TQVaultAE.GUI.Components.ScalingButton();
            this.scalingButtonMenuStyle = new TQVaultAE.GUI.Components.ScalingButton();
            this.scalingButtonMenuSetItems = new TQVaultAE.GUI.Components.ScalingButton();
            this.scalingButtonMenuWithCharm = new TQVaultAE.GUI.Components.ScalingButton();
            this.scalingButtonMenuWithRelic = new TQVaultAE.GUI.Components.ScalingButton();
            this.scalingButtonMenuAttribute = new TQVaultAE.GUI.Components.ScalingButton();
            this.scalingButtonMenuPrefixName = new TQVaultAE.GUI.Components.ScalingButton();
            this.scalingButtonMenuPrefixAttribute = new TQVaultAE.GUI.Components.ScalingButton();
            this.scalingButtonMenuBaseAttribute = new TQVaultAE.GUI.Components.ScalingButton();
            this.scalingButtonMenuSuffixName = new TQVaultAE.GUI.Components.ScalingButton();
            this.scalingButtonMenuSuffixAttribute = new TQVaultAE.GUI.Components.ScalingButton();
            this.backgroundWorkerBuildDB = new System.ComponentModel.BackgroundWorker();
            this.typeAssistantSearchBox = new TQVaultAE.GUI.Components.TypeAssistant();
            this.typeAssistantFilterCategories = new TQVaultAE.GUI.Components.TypeAssistant();
            this.tableLayoutPanelBottom.SuspendLayout();
            this.scalingLabelProgressPanelAlignText.SuspendLayout();
            this.flowLayoutPanelCharacters.SuspendLayout();
            this.flowLayoutPanelItemType.SuspendLayout();
            this.flowLayoutPanelItemAttributes.SuspendLayout();
            this.flowLayoutPanelRarity.SuspendLayout();
            this.flowLayoutPanelPrefixAttributes.SuspendLayout();
            this.flowLayoutPanelSuffixAttributes.SuspendLayout();
            this.flowLayoutPanelBaseAttributes.SuspendLayout();
            this.flowLayoutPanelMain.SuspendLayout();
            this.flowLayoutPanelInVaults.SuspendLayout();
            this.flowLayoutPanelOrigin.SuspendLayout();
            this.flowLayoutPanelQuality.SuspendLayout();
            this.flowLayoutPanelStyle.SuspendLayout();
            this.flowLayoutPanelSetItems.SuspendLayout();
            this.flowLayoutPanelWithCharm.SuspendLayout();
            this.flowLayoutPanelWithRelic.SuspendLayout();
            this.flowLayoutPanelPrefixName.SuspendLayout();
            this.flowLayoutPanelSuffixName.SuspendLayout();
            this.flowLayoutPanelSearchTerm.SuspendLayout();
            this.flowLayoutPanelQueries.SuspendLayout();
            this.tableLayoutPanelContent.SuspendLayout();
            this.flowLayoutPanelMainAutoScroll.SuspendLayout();
            this.bufferedFlowLayoutPanelQuickFilters.SuspendLayout();
            this.flowLayoutPanelMisc.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxElement)).BeginInit();
            this.bufferedFlowLayoutPanelRequierements.SuspendLayout();
            this.tableLayoutPanelRequierements.SuspendLayout();
            this.flowLayoutPanelMin.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinLvl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinStr)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinDex)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinInt)).BeginInit();
            this.flowLayoutPanelMax.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxLvl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxStr)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxDex)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxInt)).BeginInit();
            this.flowLayoutPanelLeftColumn.SuspendLayout();
            this.SuspendLayout();
            // 
            // scalingLabelCharacters
            // 
            this.scalingLabelCharacters.AutoSize = true;
            this.scalingLabelCharacters.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelCharacters.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelCharacters.Location = new System.Drawing.Point(4, 2);
            this.scalingLabelCharacters.Name = "scalingLabelCharacters";
            this.scalingLabelCharacters.Size = new System.Drawing.Size(66, 15);
            this.scalingLabelCharacters.TabIndex = 0;
            this.scalingLabelCharacters.Text = "Characters";
            this.scalingLabelCharacters.UseMnemonic = false;
            this.scalingLabelCharacters.MouseClick += new System.Windows.Forms.MouseEventHandler(this.scalingLabelCategory_MouseClick);
            // 
            // applyButton
            // 
            this.applyButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.applyButton.BackColor = System.Drawing.Color.Transparent;
            this.applyButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("applyButton.DownBitmap")));
            this.applyButton.FlatAppearance.BorderSize = 0;
            this.applyButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.applyButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.applyButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.applyButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.applyButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.applyButton.Image = ((System.Drawing.Image)(resources.GetObject("applyButton.Image")));
            this.applyButton.Location = new System.Drawing.Point(34, 40);
            this.applyButton.Name = "applyButton";
            this.applyButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("applyButton.OverBitmap")));
            this.applyButton.Size = new System.Drawing.Size(137, 30);
            this.applyButton.SizeToGraphic = false;
            this.applyButton.TabIndex = 2;
            this.applyButton.Text = "Apply";
            this.applyButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("applyButton.UpBitmap")));
            this.applyButton.UseCustomGraphic = true;
            this.applyButton.UseVisualStyleBackColor = false;
            this.applyButton.Click += new System.EventHandler(this.ApplyButtonClicked);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cancelButton.BackColor = System.Drawing.Color.Transparent;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("cancelButton.DownBitmap")));
            this.cancelButton.FlatAppearance.BorderSize = 0;
            this.cancelButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.cancelButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cancelButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.cancelButton.Image = ((System.Drawing.Image)(resources.GetObject("cancelButton.Image")));
            this.cancelButton.Location = new System.Drawing.Point(1206, 40);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("cancelButton.OverBitmap")));
            this.cancelButton.Size = new System.Drawing.Size(137, 30);
            this.cancelButton.SizeToGraphic = false;
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("cancelButton.UpBitmap")));
            this.cancelButton.UseCustomGraphic = true;
            this.cancelButton.UseVisualStyleBackColor = false;
            this.cancelButton.Click += new System.EventHandler(this.CancelButtonClicked);
            // 
            // vaultProgressBar
            // 
            this.vaultProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.vaultProgressBar.BackColor = System.Drawing.Color.Transparent;
            this.vaultProgressBar.Location = new System.Drawing.Point(209, 39);
            this.vaultProgressBar.Maximum = 0;
            this.vaultProgressBar.Minimum = 0;
            this.vaultProgressBar.Name = "vaultProgressBar";
            this.vaultProgressBar.Size = new System.Drawing.Size(959, 42);
            this.vaultProgressBar.TabIndex = 4;
            this.vaultProgressBar.Value = 0;
            // 
            // scalingLabelProgress
            // 
            this.scalingLabelProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scalingLabelProgress.BackColor = System.Drawing.Color.Transparent;
            this.scalingLabelProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelProgress.ForeColor = System.Drawing.Color.DarkOrange;
            this.scalingLabelProgress.Location = new System.Drawing.Point(422, 1);
            this.scalingLabelProgress.Name = "scalingLabelProgress";
            this.scalingLabelProgress.Size = new System.Drawing.Size(96, 17);
            this.scalingLabelProgress.TabIndex = 5;
            this.scalingLabelProgress.Text = "Building Data...";
            this.scalingLabelProgress.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.scalingLabelProgress.UseMnemonic = false;
            this.scalingLabelProgress.TextChanged += new System.EventHandler(this.scalingLabelProgress_TextChanged);
            this.scalingLabelProgress.MouseEnter += new System.EventHandler(this.scalingLabelProgress_MouseEnter);
            this.scalingLabelProgress.MouseLeave += new System.EventHandler(this.scalingLabelProgress_MouseLeave);
            // 
            // tableLayoutPanelBottom
            // 
            this.tableLayoutPanelBottom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanelBottom.ColumnCount = 3;
            this.tableLayoutPanelContent.SetColumnSpan(this.tableLayoutPanelBottom, 3);
            this.tableLayoutPanelBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanelBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanelBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanelBottom.Controls.Add(this.vaultProgressBar, 1, 1);
            this.tableLayoutPanelBottom.Controls.Add(this.applyButton, 0, 1);
            this.tableLayoutPanelBottom.Controls.Add(this.cancelButton, 2, 1);
            this.tableLayoutPanelBottom.Controls.Add(this.scalingLabelProgressPanelAlignText, 1, 0);
            this.tableLayoutPanelBottom.Location = new System.Drawing.Point(3, 748);
            this.tableLayoutPanelBottom.Name = "tableLayoutPanelBottom";
            this.tableLayoutPanelBottom.RowCount = 2;
            this.tableLayoutPanelBottom.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanelBottom.Size = new System.Drawing.Size(1379, 84);
            this.tableLayoutPanelBottom.TabIndex = 6;
            // 
            // scalingLabelProgressPanelAlignText
            // 
            this.scalingLabelProgressPanelAlignText.Controls.Add(this.scalingLabelProgress);
            this.scalingLabelProgressPanelAlignText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scalingLabelProgressPanelAlignText.Location = new System.Drawing.Point(209, 3);
            this.scalingLabelProgressPanelAlignText.Name = "scalingLabelProgressPanelAlignText";
            this.scalingLabelProgressPanelAlignText.Size = new System.Drawing.Size(959, 20);
            this.scalingLabelProgressPanelAlignText.TabIndex = 6;
            this.scalingLabelProgressPanelAlignText.SizeChanged += new System.EventHandler(this.scalingLabelProgressPanelAlignText_SizeChanged);
            // 
            // scalingCheckedListBoxCharacters
            // 
            this.scalingCheckedListBoxCharacters.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.scalingCheckedListBoxCharacters.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.scalingCheckedListBoxCharacters.CheckOnClick = true;
            this.scalingCheckedListBoxCharacters.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingCheckedListBoxCharacters.ForeColor = System.Drawing.Color.White;
            this.scalingCheckedListBoxCharacters.FormattingEnabled = true;
            this.scalingCheckedListBoxCharacters.HorizontalScrollbar = true;
            this.scalingCheckedListBoxCharacters.IntegralHeight = false;
            this.scalingCheckedListBoxCharacters.Items.AddRange(new object[] {
            "Char Name 1",
            "Char Name 2",
            "Char Name 3 (Not loaded)"});
            this.scalingCheckedListBoxCharacters.Location = new System.Drawing.Point(4, 17);
            this.scalingCheckedListBoxCharacters.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.scalingCheckedListBoxCharacters.MultiColumn = true;
            this.scalingCheckedListBoxCharacters.Name = "scalingCheckedListBoxCharacters";
            this.scalingCheckedListBoxCharacters.Size = new System.Drawing.Size(215, 80);
            this.scalingCheckedListBoxCharacters.TabIndex = 7;
            this.scalingCheckedListBoxCharacters.SelectedValueChanged += new System.EventHandler(this.scalingCheckedListBox_SelectedValueChanged);
            this.scalingCheckedListBoxCharacters.MouseMove += new System.Windows.Forms.MouseEventHandler(this.scalingCheckedListBox_MouseMove);
            // 
            // flowLayoutPanelCharacters
            // 
            this.flowLayoutPanelCharacters.AutoSize = true;
            this.flowLayoutPanelCharacters.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelCharacters.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanelCharacters.Controls.Add(this.scalingLabelCharacters);
            this.flowLayoutPanelCharacters.Controls.Add(this.scalingCheckedListBoxCharacters);
            this.flowLayoutPanelCharacters.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelCharacters.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanelCharacters.MinimumSize = new System.Drawing.Size(50, 10);
            this.flowLayoutPanelCharacters.Name = "flowLayoutPanelCharacters";
            this.flowLayoutPanelCharacters.Padding = new System.Windows.Forms.Padding(1, 2, 0, 0);
            this.flowLayoutPanelCharacters.Size = new System.Drawing.Size(224, 99);
            this.flowLayoutPanelCharacters.TabIndex = 8;
            // 
            // flowLayoutPanelItemType
            // 
            this.flowLayoutPanelItemType.AutoSize = true;
            this.flowLayoutPanelItemType.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelItemType.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanelItemType.Controls.Add(this.scalingLabelItemType);
            this.flowLayoutPanelItemType.Controls.Add(this.scalingCheckedListBoxItemType);
            this.flowLayoutPanelItemType.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelItemType.Location = new System.Drawing.Point(463, 3);
            this.flowLayoutPanelItemType.MinimumSize = new System.Drawing.Size(50, 10);
            this.flowLayoutPanelItemType.Name = "flowLayoutPanelItemType";
            this.flowLayoutPanelItemType.Padding = new System.Windows.Forms.Padding(1, 2, 0, 0);
            this.flowLayoutPanelItemType.Size = new System.Drawing.Size(224, 99);
            this.flowLayoutPanelItemType.TabIndex = 9;
            // 
            // scalingLabelItemType
            // 
            this.scalingLabelItemType.AutoSize = true;
            this.scalingLabelItemType.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelItemType.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelItemType.Location = new System.Drawing.Point(4, 2);
            this.scalingLabelItemType.Name = "scalingLabelItemType";
            this.scalingLabelItemType.Size = new System.Drawing.Size(33, 15);
            this.scalingLabelItemType.TabIndex = 0;
            this.scalingLabelItemType.Text = "Type";
            this.scalingLabelItemType.UseMnemonic = false;
            this.scalingLabelItemType.MouseClick += new System.Windows.Forms.MouseEventHandler(this.scalingLabelCategory_MouseClick);
            // 
            // scalingCheckedListBoxItemType
            // 
            this.scalingCheckedListBoxItemType.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.scalingCheckedListBoxItemType.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.scalingCheckedListBoxItemType.CheckOnClick = true;
            this.scalingCheckedListBoxItemType.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingCheckedListBoxItemType.ForeColor = System.Drawing.Color.White;
            this.scalingCheckedListBoxItemType.FormattingEnabled = true;
            this.scalingCheckedListBoxItemType.HorizontalScrollbar = true;
            this.scalingCheckedListBoxItemType.IntegralHeight = false;
            this.scalingCheckedListBoxItemType.Items.AddRange(new object[] {
            "Sword",
            "Helmet",
            "Ring"});
            this.scalingCheckedListBoxItemType.Location = new System.Drawing.Point(4, 17);
            this.scalingCheckedListBoxItemType.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.scalingCheckedListBoxItemType.MultiColumn = true;
            this.scalingCheckedListBoxItemType.Name = "scalingCheckedListBoxItemType";
            this.scalingCheckedListBoxItemType.Size = new System.Drawing.Size(215, 80);
            this.scalingCheckedListBoxItemType.TabIndex = 7;
            this.scalingCheckedListBoxItemType.SelectedValueChanged += new System.EventHandler(this.scalingCheckedListBox_SelectedValueChanged);
            this.scalingCheckedListBoxItemType.MouseMove += new System.Windows.Forms.MouseEventHandler(this.scalingCheckedListBox_MouseMove);
            // 
            // flowLayoutPanelItemAttributes
            // 
            this.flowLayoutPanelItemAttributes.AutoSize = true;
            this.flowLayoutPanelItemAttributes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelItemAttributes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanelItemAttributes.Controls.Add(this.scalingLabelItemAttributes);
            this.flowLayoutPanelItemAttributes.Controls.Add(this.scalingCheckedListBoxItemAttributes);
            this.flowLayoutPanelItemAttributes.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelItemAttributes.Location = new System.Drawing.Point(3, 213);
            this.flowLayoutPanelItemAttributes.MinimumSize = new System.Drawing.Size(50, 10);
            this.flowLayoutPanelItemAttributes.Name = "flowLayoutPanelItemAttributes";
            this.flowLayoutPanelItemAttributes.Padding = new System.Windows.Forms.Padding(1, 2, 0, 0);
            this.flowLayoutPanelItemAttributes.Size = new System.Drawing.Size(224, 99);
            this.flowLayoutPanelItemAttributes.TabIndex = 10;
            // 
            // scalingLabelItemAttributes
            // 
            this.scalingLabelItemAttributes.AutoSize = true;
            this.scalingLabelItemAttributes.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelItemAttributes.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelItemAttributes.Location = new System.Drawing.Point(4, 2);
            this.scalingLabelItemAttributes.Name = "scalingLabelItemAttributes";
            this.scalingLabelItemAttributes.Size = new System.Drawing.Size(57, 15);
            this.scalingLabelItemAttributes.TabIndex = 0;
            this.scalingLabelItemAttributes.Text = "Attributes";
            this.scalingLabelItemAttributes.UseMnemonic = false;
            this.scalingLabelItemAttributes.MouseClick += new System.Windows.Forms.MouseEventHandler(this.scalingLabelCategory_MouseClick);
            // 
            // scalingCheckedListBoxItemAttributes
            // 
            this.scalingCheckedListBoxItemAttributes.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.scalingCheckedListBoxItemAttributes.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.scalingCheckedListBoxItemAttributes.CheckOnClick = true;
            this.scalingCheckedListBoxItemAttributes.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingCheckedListBoxItemAttributes.ForeColor = System.Drawing.Color.White;
            this.scalingCheckedListBoxItemAttributes.FormattingEnabled = true;
            this.scalingCheckedListBoxItemAttributes.HorizontalScrollbar = true;
            this.scalingCheckedListBoxItemAttributes.IntegralHeight = false;
            this.scalingCheckedListBoxItemAttributes.Items.AddRange(new object[] {
            "Physical Damage",
            "Elemental Resistance",
            "Elemental Damage"});
            this.scalingCheckedListBoxItemAttributes.Location = new System.Drawing.Point(4, 17);
            this.scalingCheckedListBoxItemAttributes.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.scalingCheckedListBoxItemAttributes.MultiColumn = true;
            this.scalingCheckedListBoxItemAttributes.Name = "scalingCheckedListBoxItemAttributes";
            this.scalingCheckedListBoxItemAttributes.Size = new System.Drawing.Size(215, 80);
            this.scalingCheckedListBoxItemAttributes.TabIndex = 7;
            this.scalingCheckedListBoxItemAttributes.SelectedValueChanged += new System.EventHandler(this.scalingCheckedListBox_SelectedValueChanged);
            this.scalingCheckedListBoxItemAttributes.MouseMove += new System.Windows.Forms.MouseEventHandler(this.scalingCheckedListBox_MouseMove);
            // 
            // flowLayoutPanelRarity
            // 
            this.flowLayoutPanelRarity.AutoSize = true;
            this.flowLayoutPanelRarity.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelRarity.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanelRarity.Controls.Add(this.scalingLabelRarity);
            this.flowLayoutPanelRarity.Controls.Add(this.scalingCheckedListBoxRarity);
            this.flowLayoutPanelRarity.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelRarity.Location = new System.Drawing.Point(693, 3);
            this.flowLayoutPanelRarity.MinimumSize = new System.Drawing.Size(50, 10);
            this.flowLayoutPanelRarity.Name = "flowLayoutPanelRarity";
            this.flowLayoutPanelRarity.Padding = new System.Windows.Forms.Padding(1, 2, 0, 0);
            this.flowLayoutPanelRarity.Size = new System.Drawing.Size(224, 99);
            this.flowLayoutPanelRarity.TabIndex = 11;
            // 
            // scalingLabelRarity
            // 
            this.scalingLabelRarity.AutoSize = true;
            this.scalingLabelRarity.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelRarity.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelRarity.Location = new System.Drawing.Point(4, 2);
            this.scalingLabelRarity.Name = "scalingLabelRarity";
            this.scalingLabelRarity.Size = new System.Drawing.Size(38, 15);
            this.scalingLabelRarity.TabIndex = 0;
            this.scalingLabelRarity.Text = "Rarity";
            this.scalingLabelRarity.UseMnemonic = false;
            this.scalingLabelRarity.MouseClick += new System.Windows.Forms.MouseEventHandler(this.scalingLabelCategory_MouseClick);
            // 
            // scalingCheckedListBoxRarity
            // 
            this.scalingCheckedListBoxRarity.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.scalingCheckedListBoxRarity.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.scalingCheckedListBoxRarity.CheckOnClick = true;
            this.scalingCheckedListBoxRarity.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingCheckedListBoxRarity.ForeColor = System.Drawing.Color.White;
            this.scalingCheckedListBoxRarity.FormattingEnabled = true;
            this.scalingCheckedListBoxRarity.HorizontalScrollbar = true;
            this.scalingCheckedListBoxRarity.IntegralHeight = false;
            this.scalingCheckedListBoxRarity.Items.AddRange(new object[] {
            "Common",
            "Rare",
            "Epic",
            "Lengendary"});
            this.scalingCheckedListBoxRarity.Location = new System.Drawing.Point(4, 17);
            this.scalingCheckedListBoxRarity.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.scalingCheckedListBoxRarity.MultiColumn = true;
            this.scalingCheckedListBoxRarity.Name = "scalingCheckedListBoxRarity";
            this.scalingCheckedListBoxRarity.Size = new System.Drawing.Size(215, 80);
            this.scalingCheckedListBoxRarity.TabIndex = 7;
            this.scalingCheckedListBoxRarity.SelectedValueChanged += new System.EventHandler(this.scalingCheckedListBox_SelectedValueChanged);
            this.scalingCheckedListBoxRarity.MouseMove += new System.Windows.Forms.MouseEventHandler(this.scalingCheckedListBox_MouseMove);
            // 
            // flowLayoutPanelPrefixAttributes
            // 
            this.flowLayoutPanelPrefixAttributes.AutoSize = true;
            this.flowLayoutPanelPrefixAttributes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelPrefixAttributes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanelPrefixAttributes.Controls.Add(this.scalingLabelPrefixAttributes);
            this.flowLayoutPanelPrefixAttributes.Controls.Add(this.scalingCheckedListBoxPrefixAttributes);
            this.flowLayoutPanelPrefixAttributes.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelPrefixAttributes.Location = new System.Drawing.Point(463, 213);
            this.flowLayoutPanelPrefixAttributes.MinimumSize = new System.Drawing.Size(50, 10);
            this.flowLayoutPanelPrefixAttributes.Name = "flowLayoutPanelPrefixAttributes";
            this.flowLayoutPanelPrefixAttributes.Padding = new System.Windows.Forms.Padding(1, 2, 0, 0);
            this.flowLayoutPanelPrefixAttributes.Size = new System.Drawing.Size(224, 99);
            this.flowLayoutPanelPrefixAttributes.TabIndex = 12;
            // 
            // scalingLabelPrefixAttributes
            // 
            this.scalingLabelPrefixAttributes.AutoSize = true;
            this.scalingLabelPrefixAttributes.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelPrefixAttributes.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelPrefixAttributes.Location = new System.Drawing.Point(4, 2);
            this.scalingLabelPrefixAttributes.Name = "scalingLabelPrefixAttributes";
            this.scalingLabelPrefixAttributes.Size = new System.Drawing.Size(91, 15);
            this.scalingLabelPrefixAttributes.TabIndex = 0;
            this.scalingLabelPrefixAttributes.Text = "Prefix Attributes";
            this.scalingLabelPrefixAttributes.UseMnemonic = false;
            this.scalingLabelPrefixAttributes.MouseClick += new System.Windows.Forms.MouseEventHandler(this.scalingLabelCategory_MouseClick);
            // 
            // scalingCheckedListBoxPrefixAttributes
            // 
            this.scalingCheckedListBoxPrefixAttributes.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.scalingCheckedListBoxPrefixAttributes.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.scalingCheckedListBoxPrefixAttributes.CheckOnClick = true;
            this.scalingCheckedListBoxPrefixAttributes.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingCheckedListBoxPrefixAttributes.ForeColor = System.Drawing.Color.White;
            this.scalingCheckedListBoxPrefixAttributes.FormattingEnabled = true;
            this.scalingCheckedListBoxPrefixAttributes.HorizontalScrollbar = true;
            this.scalingCheckedListBoxPrefixAttributes.IntegralHeight = false;
            this.scalingCheckedListBoxPrefixAttributes.Items.AddRange(new object[] {
            "Physical Damage",
            "Elemental Resistance",
            "Elemental Damage"});
            this.scalingCheckedListBoxPrefixAttributes.Location = new System.Drawing.Point(4, 17);
            this.scalingCheckedListBoxPrefixAttributes.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.scalingCheckedListBoxPrefixAttributes.MultiColumn = true;
            this.scalingCheckedListBoxPrefixAttributes.Name = "scalingCheckedListBoxPrefixAttributes";
            this.scalingCheckedListBoxPrefixAttributes.Size = new System.Drawing.Size(215, 80);
            this.scalingCheckedListBoxPrefixAttributes.TabIndex = 7;
            this.scalingCheckedListBoxPrefixAttributes.SelectedValueChanged += new System.EventHandler(this.scalingCheckedListBox_SelectedValueChanged);
            this.scalingCheckedListBoxPrefixAttributes.MouseMove += new System.Windows.Forms.MouseEventHandler(this.scalingCheckedListBox_MouseMove);
            // 
            // flowLayoutPanelSuffixAttributes
            // 
            this.flowLayoutPanelSuffixAttributes.AutoSize = true;
            this.flowLayoutPanelSuffixAttributes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelSuffixAttributes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanelSuffixAttributes.Controls.Add(this.scalingLabelSuffixAttributes);
            this.flowLayoutPanelSuffixAttributes.Controls.Add(this.scalingCheckedListBoxSuffixAttributes);
            this.flowLayoutPanelSuffixAttributes.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelSuffixAttributes.Location = new System.Drawing.Point(3, 318);
            this.flowLayoutPanelSuffixAttributes.MinimumSize = new System.Drawing.Size(50, 10);
            this.flowLayoutPanelSuffixAttributes.Name = "flowLayoutPanelSuffixAttributes";
            this.flowLayoutPanelSuffixAttributes.Padding = new System.Windows.Forms.Padding(1, 2, 0, 0);
            this.flowLayoutPanelSuffixAttributes.Size = new System.Drawing.Size(224, 99);
            this.flowLayoutPanelSuffixAttributes.TabIndex = 13;
            this.flowLayoutPanelSuffixAttributes.WrapContents = false;
            // 
            // scalingLabelSuffixAttributes
            // 
            this.scalingLabelSuffixAttributes.AutoSize = true;
            this.scalingLabelSuffixAttributes.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelSuffixAttributes.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelSuffixAttributes.Location = new System.Drawing.Point(4, 2);
            this.scalingLabelSuffixAttributes.Name = "scalingLabelSuffixAttributes";
            this.scalingLabelSuffixAttributes.Size = new System.Drawing.Size(90, 15);
            this.scalingLabelSuffixAttributes.TabIndex = 0;
            this.scalingLabelSuffixAttributes.Text = "Suffix Attributes";
            this.scalingLabelSuffixAttributes.UseMnemonic = false;
            this.scalingLabelSuffixAttributes.MouseClick += new System.Windows.Forms.MouseEventHandler(this.scalingLabelCategory_MouseClick);
            // 
            // scalingCheckedListBoxSuffixAttributes
            // 
            this.scalingCheckedListBoxSuffixAttributes.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.scalingCheckedListBoxSuffixAttributes.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.scalingCheckedListBoxSuffixAttributes.CheckOnClick = true;
            this.scalingCheckedListBoxSuffixAttributes.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingCheckedListBoxSuffixAttributes.ForeColor = System.Drawing.Color.White;
            this.scalingCheckedListBoxSuffixAttributes.FormattingEnabled = true;
            this.scalingCheckedListBoxSuffixAttributes.HorizontalScrollbar = true;
            this.scalingCheckedListBoxSuffixAttributes.IntegralHeight = false;
            this.scalingCheckedListBoxSuffixAttributes.Items.AddRange(new object[] {
            "Physical Damage",
            "Elemental Resistance",
            "Elemental Damage"});
            this.scalingCheckedListBoxSuffixAttributes.Location = new System.Drawing.Point(4, 17);
            this.scalingCheckedListBoxSuffixAttributes.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.scalingCheckedListBoxSuffixAttributes.MultiColumn = true;
            this.scalingCheckedListBoxSuffixAttributes.Name = "scalingCheckedListBoxSuffixAttributes";
            this.scalingCheckedListBoxSuffixAttributes.Size = new System.Drawing.Size(215, 80);
            this.scalingCheckedListBoxSuffixAttributes.TabIndex = 7;
            this.scalingCheckedListBoxSuffixAttributes.SelectedValueChanged += new System.EventHandler(this.scalingCheckedListBox_SelectedValueChanged);
            this.scalingCheckedListBoxSuffixAttributes.MouseMove += new System.Windows.Forms.MouseEventHandler(this.scalingCheckedListBox_MouseMove);
            // 
            // flowLayoutPanelBaseAttributes
            // 
            this.flowLayoutPanelBaseAttributes.AutoSize = true;
            this.flowLayoutPanelBaseAttributes.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelBaseAttributes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanelBaseAttributes.Controls.Add(this.scalingLabelBaseAttributes);
            this.flowLayoutPanelBaseAttributes.Controls.Add(this.scalingCheckedListBoxBaseAttributes);
            this.flowLayoutPanelBaseAttributes.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelBaseAttributes.Location = new System.Drawing.Point(693, 213);
            this.flowLayoutPanelBaseAttributes.MinimumSize = new System.Drawing.Size(50, 10);
            this.flowLayoutPanelBaseAttributes.Name = "flowLayoutPanelBaseAttributes";
            this.flowLayoutPanelBaseAttributes.Padding = new System.Windows.Forms.Padding(1, 2, 0, 0);
            this.flowLayoutPanelBaseAttributes.Size = new System.Drawing.Size(224, 99);
            this.flowLayoutPanelBaseAttributes.TabIndex = 14;
            // 
            // scalingLabelBaseAttributes
            // 
            this.scalingLabelBaseAttributes.AutoSize = true;
            this.scalingLabelBaseAttributes.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelBaseAttributes.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelBaseAttributes.Location = new System.Drawing.Point(4, 2);
            this.scalingLabelBaseAttributes.Name = "scalingLabelBaseAttributes";
            this.scalingLabelBaseAttributes.Size = new System.Drawing.Size(88, 15);
            this.scalingLabelBaseAttributes.TabIndex = 0;
            this.scalingLabelBaseAttributes.Text = "Base Attributes";
            this.scalingLabelBaseAttributes.UseMnemonic = false;
            this.scalingLabelBaseAttributes.MouseClick += new System.Windows.Forms.MouseEventHandler(this.scalingLabelCategory_MouseClick);
            // 
            // scalingCheckedListBoxBaseAttributes
            // 
            this.scalingCheckedListBoxBaseAttributes.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.scalingCheckedListBoxBaseAttributes.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.scalingCheckedListBoxBaseAttributes.CheckOnClick = true;
            this.scalingCheckedListBoxBaseAttributes.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingCheckedListBoxBaseAttributes.ForeColor = System.Drawing.Color.White;
            this.scalingCheckedListBoxBaseAttributes.FormattingEnabled = true;
            this.scalingCheckedListBoxBaseAttributes.HorizontalScrollbar = true;
            this.scalingCheckedListBoxBaseAttributes.IntegralHeight = false;
            this.scalingCheckedListBoxBaseAttributes.Items.AddRange(new object[] {
            "Physical Damage",
            "Elemental Resistance",
            "Elemental Damage"});
            this.scalingCheckedListBoxBaseAttributes.Location = new System.Drawing.Point(4, 17);
            this.scalingCheckedListBoxBaseAttributes.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.scalingCheckedListBoxBaseAttributes.MultiColumn = true;
            this.scalingCheckedListBoxBaseAttributes.Name = "scalingCheckedListBoxBaseAttributes";
            this.scalingCheckedListBoxBaseAttributes.Size = new System.Drawing.Size(215, 80);
            this.scalingCheckedListBoxBaseAttributes.TabIndex = 7;
            this.scalingCheckedListBoxBaseAttributes.SelectedValueChanged += new System.EventHandler(this.scalingCheckedListBox_SelectedValueChanged);
            this.scalingCheckedListBoxBaseAttributes.MouseMove += new System.Windows.Forms.MouseEventHandler(this.scalingCheckedListBox_MouseMove);
            // 
            // flowLayoutPanelMain
            // 
            this.flowLayoutPanelMain.AutoSize = true;
            this.flowLayoutPanelMain.Controls.Add(this.flowLayoutPanelCharacters);
            this.flowLayoutPanelMain.Controls.Add(this.flowLayoutPanelInVaults);
            this.flowLayoutPanelMain.Controls.Add(this.flowLayoutPanelItemType);
            this.flowLayoutPanelMain.Controls.Add(this.flowLayoutPanelRarity);
            this.flowLayoutPanelMain.Controls.Add(this.flowLayoutPanelOrigin);
            this.flowLayoutPanelMain.Controls.Add(this.flowLayoutPanelQuality);
            this.flowLayoutPanelMain.Controls.Add(this.flowLayoutPanelStyle);
            this.flowLayoutPanelMain.Controls.Add(this.flowLayoutPanelSetItems);
            this.flowLayoutPanelMain.Controls.Add(this.flowLayoutPanelWithCharm);
            this.flowLayoutPanelMain.Controls.Add(this.flowLayoutPanelWithRelic);
            this.flowLayoutPanelMain.Controls.Add(this.flowLayoutPanelItemAttributes);
            this.flowLayoutPanelMain.Controls.Add(this.flowLayoutPanelPrefixName);
            this.flowLayoutPanelMain.Controls.Add(this.flowLayoutPanelPrefixAttributes);
            this.flowLayoutPanelMain.Controls.Add(this.flowLayoutPanelBaseAttributes);
            this.flowLayoutPanelMain.Controls.Add(this.flowLayoutPanelSuffixName);
            this.flowLayoutPanelMain.Controls.Add(this.flowLayoutPanelSuffixAttributes);
            this.flowLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanelMain.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanelMain.MinimumSize = new System.Drawing.Size(200, 100);
            this.flowLayoutPanelMain.Name = "flowLayoutPanelMain";
            this.flowLayoutPanelMain.Size = new System.Drawing.Size(1150, 420);
            this.flowLayoutPanelMain.TabIndex = 15;
            // 
            // flowLayoutPanelInVaults
            // 
            this.flowLayoutPanelInVaults.AutoSize = true;
            this.flowLayoutPanelInVaults.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelInVaults.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanelInVaults.Controls.Add(this.scalingLabelInVaults);
            this.flowLayoutPanelInVaults.Controls.Add(this.scalingCheckedListBoxVaults);
            this.flowLayoutPanelInVaults.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelInVaults.Location = new System.Drawing.Point(233, 3);
            this.flowLayoutPanelInVaults.MinimumSize = new System.Drawing.Size(50, 10);
            this.flowLayoutPanelInVaults.Name = "flowLayoutPanelInVaults";
            this.flowLayoutPanelInVaults.Padding = new System.Windows.Forms.Padding(1, 2, 0, 0);
            this.flowLayoutPanelInVaults.Size = new System.Drawing.Size(224, 99);
            this.flowLayoutPanelInVaults.TabIndex = 19;
            // 
            // scalingLabelInVaults
            // 
            this.scalingLabelInVaults.AutoSize = true;
            this.scalingLabelInVaults.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelInVaults.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelInVaults.Location = new System.Drawing.Point(4, 2);
            this.scalingLabelInVaults.Name = "scalingLabelInVaults";
            this.scalingLabelInVaults.Size = new System.Drawing.Size(40, 15);
            this.scalingLabelInVaults.TabIndex = 0;
            this.scalingLabelInVaults.Text = "Vaults";
            this.scalingLabelInVaults.UseMnemonic = false;
            this.scalingLabelInVaults.MouseClick += new System.Windows.Forms.MouseEventHandler(this.scalingLabelCategory_MouseClick);
            // 
            // scalingCheckedListBoxVaults
            // 
            this.scalingCheckedListBoxVaults.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.scalingCheckedListBoxVaults.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.scalingCheckedListBoxVaults.CheckOnClick = true;
            this.scalingCheckedListBoxVaults.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingCheckedListBoxVaults.ForeColor = System.Drawing.Color.White;
            this.scalingCheckedListBoxVaults.FormattingEnabled = true;
            this.scalingCheckedListBoxVaults.HorizontalScrollbar = true;
            this.scalingCheckedListBoxVaults.IntegralHeight = false;
            this.scalingCheckedListBoxVaults.Items.AddRange(new object[] {
            "Vault Name 1",
            "Vault Name 2",
            "Vault Name 3"});
            this.scalingCheckedListBoxVaults.Location = new System.Drawing.Point(4, 17);
            this.scalingCheckedListBoxVaults.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.scalingCheckedListBoxVaults.MultiColumn = true;
            this.scalingCheckedListBoxVaults.Name = "scalingCheckedListBoxVaults";
            this.scalingCheckedListBoxVaults.Size = new System.Drawing.Size(215, 80);
            this.scalingCheckedListBoxVaults.TabIndex = 7;
            this.scalingCheckedListBoxVaults.SelectedValueChanged += new System.EventHandler(this.scalingCheckedListBox_SelectedValueChanged);
            this.scalingCheckedListBoxVaults.MouseMove += new System.Windows.Forms.MouseEventHandler(this.scalingCheckedListBox_MouseMove);
            // 
            // flowLayoutPanelOrigin
            // 
            this.flowLayoutPanelOrigin.AutoSize = true;
            this.flowLayoutPanelOrigin.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelOrigin.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanelOrigin.Controls.Add(this.scalingLabelOrigin);
            this.flowLayoutPanelOrigin.Controls.Add(this.scalingCheckedListBoxOrigin);
            this.flowLayoutPanelOrigin.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelOrigin.Location = new System.Drawing.Point(923, 3);
            this.flowLayoutPanelOrigin.MinimumSize = new System.Drawing.Size(50, 10);
            this.flowLayoutPanelOrigin.Name = "flowLayoutPanelOrigin";
            this.flowLayoutPanelOrigin.Padding = new System.Windows.Forms.Padding(1, 2, 0, 0);
            this.flowLayoutPanelOrigin.Size = new System.Drawing.Size(224, 99);
            this.flowLayoutPanelOrigin.TabIndex = 22;
            // 
            // scalingLabelOrigin
            // 
            this.scalingLabelOrigin.AutoSize = true;
            this.scalingLabelOrigin.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelOrigin.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelOrigin.Location = new System.Drawing.Point(4, 2);
            this.scalingLabelOrigin.Name = "scalingLabelOrigin";
            this.scalingLabelOrigin.Size = new System.Drawing.Size(40, 15);
            this.scalingLabelOrigin.TabIndex = 0;
            this.scalingLabelOrigin.Text = "Origin";
            this.scalingLabelOrigin.UseMnemonic = false;
            this.scalingLabelOrigin.MouseClick += new System.Windows.Forms.MouseEventHandler(this.scalingLabelCategory_MouseClick);
            // 
            // scalingCheckedListBoxOrigin
            // 
            this.scalingCheckedListBoxOrigin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.scalingCheckedListBoxOrigin.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.scalingCheckedListBoxOrigin.CheckOnClick = true;
            this.scalingCheckedListBoxOrigin.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingCheckedListBoxOrigin.ForeColor = System.Drawing.Color.White;
            this.scalingCheckedListBoxOrigin.FormattingEnabled = true;
            this.scalingCheckedListBoxOrigin.HorizontalScrollbar = true;
            this.scalingCheckedListBoxOrigin.IntegralHeight = false;
            this.scalingCheckedListBoxOrigin.Items.AddRange(new object[] {
            "Titan Quest Original",
            "Immortal Throne",
            "Ragnarok",
            "Atlantis",
            "Eternal Embers"});
            this.scalingCheckedListBoxOrigin.Location = new System.Drawing.Point(4, 17);
            this.scalingCheckedListBoxOrigin.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.scalingCheckedListBoxOrigin.MultiColumn = true;
            this.scalingCheckedListBoxOrigin.Name = "scalingCheckedListBoxOrigin";
            this.scalingCheckedListBoxOrigin.Size = new System.Drawing.Size(215, 80);
            this.scalingCheckedListBoxOrigin.TabIndex = 7;
            this.scalingCheckedListBoxOrigin.SelectedValueChanged += new System.EventHandler(this.scalingCheckedListBox_SelectedValueChanged);
            this.scalingCheckedListBoxOrigin.MouseMove += new System.Windows.Forms.MouseEventHandler(this.scalingCheckedListBox_MouseMove);
            // 
            // flowLayoutPanelQuality
            // 
            this.flowLayoutPanelQuality.AutoSize = true;
            this.flowLayoutPanelQuality.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelQuality.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanelQuality.Controls.Add(this.scalingLabelQuality);
            this.flowLayoutPanelQuality.Controls.Add(this.scalingCheckedListBoxQuality);
            this.flowLayoutPanelQuality.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelQuality.Location = new System.Drawing.Point(3, 108);
            this.flowLayoutPanelQuality.MinimumSize = new System.Drawing.Size(50, 10);
            this.flowLayoutPanelQuality.Name = "flowLayoutPanelQuality";
            this.flowLayoutPanelQuality.Padding = new System.Windows.Forms.Padding(1, 2, 0, 0);
            this.flowLayoutPanelQuality.Size = new System.Drawing.Size(224, 99);
            this.flowLayoutPanelQuality.TabIndex = 21;
            // 
            // scalingLabelQuality
            // 
            this.scalingLabelQuality.AutoSize = true;
            this.scalingLabelQuality.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelQuality.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelQuality.Location = new System.Drawing.Point(4, 2);
            this.scalingLabelQuality.Name = "scalingLabelQuality";
            this.scalingLabelQuality.Size = new System.Drawing.Size(44, 15);
            this.scalingLabelQuality.TabIndex = 0;
            this.scalingLabelQuality.Text = "Quality";
            this.scalingLabelQuality.UseMnemonic = false;
            this.scalingLabelQuality.MouseClick += new System.Windows.Forms.MouseEventHandler(this.scalingLabelCategory_MouseClick);
            // 
            // scalingCheckedListBoxQuality
            // 
            this.scalingCheckedListBoxQuality.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.scalingCheckedListBoxQuality.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.scalingCheckedListBoxQuality.CheckOnClick = true;
            this.scalingCheckedListBoxQuality.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingCheckedListBoxQuality.ForeColor = System.Drawing.Color.White;
            this.scalingCheckedListBoxQuality.FormattingEnabled = true;
            this.scalingCheckedListBoxQuality.HorizontalScrollbar = true;
            this.scalingCheckedListBoxQuality.IntegralHeight = false;
            this.scalingCheckedListBoxQuality.Items.AddRange(new object[] {
            "Common",
            "Rare",
            "Epic",
            "Lengendary"});
            this.scalingCheckedListBoxQuality.Location = new System.Drawing.Point(4, 17);
            this.scalingCheckedListBoxQuality.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.scalingCheckedListBoxQuality.MultiColumn = true;
            this.scalingCheckedListBoxQuality.Name = "scalingCheckedListBoxQuality";
            this.scalingCheckedListBoxQuality.Size = new System.Drawing.Size(215, 80);
            this.scalingCheckedListBoxQuality.TabIndex = 7;
            this.scalingCheckedListBoxQuality.SelectedValueChanged += new System.EventHandler(this.scalingCheckedListBox_SelectedValueChanged);
            this.scalingCheckedListBoxQuality.MouseMove += new System.Windows.Forms.MouseEventHandler(this.scalingCheckedListBox_MouseMove);
            // 
            // flowLayoutPanelStyle
            // 
            this.flowLayoutPanelStyle.AutoSize = true;
            this.flowLayoutPanelStyle.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelStyle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanelStyle.Controls.Add(this.scalingLabelStyle);
            this.flowLayoutPanelStyle.Controls.Add(this.scalingCheckedListBoxStyle);
            this.flowLayoutPanelStyle.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelStyle.Location = new System.Drawing.Point(233, 108);
            this.flowLayoutPanelStyle.MinimumSize = new System.Drawing.Size(50, 10);
            this.flowLayoutPanelStyle.Name = "flowLayoutPanelStyle";
            this.flowLayoutPanelStyle.Padding = new System.Windows.Forms.Padding(1, 2, 0, 0);
            this.flowLayoutPanelStyle.Size = new System.Drawing.Size(224, 99);
            this.flowLayoutPanelStyle.TabIndex = 20;
            // 
            // scalingLabelStyle
            // 
            this.scalingLabelStyle.AutoSize = true;
            this.scalingLabelStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelStyle.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelStyle.Location = new System.Drawing.Point(4, 2);
            this.scalingLabelStyle.Name = "scalingLabelStyle";
            this.scalingLabelStyle.Size = new System.Drawing.Size(33, 15);
            this.scalingLabelStyle.TabIndex = 0;
            this.scalingLabelStyle.Text = "Style";
            this.scalingLabelStyle.UseMnemonic = false;
            this.scalingLabelStyle.MouseClick += new System.Windows.Forms.MouseEventHandler(this.scalingLabelCategory_MouseClick);
            // 
            // scalingCheckedListBoxStyle
            // 
            this.scalingCheckedListBoxStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.scalingCheckedListBoxStyle.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.scalingCheckedListBoxStyle.CheckOnClick = true;
            this.scalingCheckedListBoxStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingCheckedListBoxStyle.ForeColor = System.Drawing.Color.White;
            this.scalingCheckedListBoxStyle.FormattingEnabled = true;
            this.scalingCheckedListBoxStyle.HorizontalScrollbar = true;
            this.scalingCheckedListBoxStyle.IntegralHeight = false;
            this.scalingCheckedListBoxStyle.Items.AddRange(new object[] {
            "Aesir",
            "Alexandrian",
            "Asgardian",
            "Babylonian"});
            this.scalingCheckedListBoxStyle.Location = new System.Drawing.Point(4, 17);
            this.scalingCheckedListBoxStyle.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.scalingCheckedListBoxStyle.MultiColumn = true;
            this.scalingCheckedListBoxStyle.Name = "scalingCheckedListBoxStyle";
            this.scalingCheckedListBoxStyle.Size = new System.Drawing.Size(215, 80);
            this.scalingCheckedListBoxStyle.TabIndex = 7;
            this.scalingCheckedListBoxStyle.SelectedValueChanged += new System.EventHandler(this.scalingCheckedListBox_SelectedValueChanged);
            this.scalingCheckedListBoxStyle.MouseMove += new System.Windows.Forms.MouseEventHandler(this.scalingCheckedListBox_MouseMove);
            // 
            // flowLayoutPanelSetItems
            // 
            this.flowLayoutPanelSetItems.AutoSize = true;
            this.flowLayoutPanelSetItems.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelSetItems.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanelSetItems.Controls.Add(this.scalingLabelSetItems);
            this.flowLayoutPanelSetItems.Controls.Add(this.scalingCheckedListBoxSetItems);
            this.flowLayoutPanelSetItems.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelSetItems.Location = new System.Drawing.Point(463, 108);
            this.flowLayoutPanelSetItems.MinimumSize = new System.Drawing.Size(50, 10);
            this.flowLayoutPanelSetItems.Name = "flowLayoutPanelSetItems";
            this.flowLayoutPanelSetItems.Padding = new System.Windows.Forms.Padding(1, 2, 0, 0);
            this.flowLayoutPanelSetItems.Size = new System.Drawing.Size(224, 99);
            this.flowLayoutPanelSetItems.TabIndex = 23;
            // 
            // scalingLabelSetItems
            // 
            this.scalingLabelSetItems.AutoSize = true;
            this.scalingLabelSetItems.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelSetItems.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelSetItems.Location = new System.Drawing.Point(4, 2);
            this.scalingLabelSetItems.Name = "scalingLabelSetItems";
            this.scalingLabelSetItems.Size = new System.Drawing.Size(31, 15);
            this.scalingLabelSetItems.TabIndex = 0;
            this.scalingLabelSetItems.Text = "Sets";
            this.scalingLabelSetItems.UseMnemonic = false;
            this.scalingLabelSetItems.MouseClick += new System.Windows.Forms.MouseEventHandler(this.scalingLabelCategory_MouseClick);
            // 
            // scalingCheckedListBoxSetItems
            // 
            this.scalingCheckedListBoxSetItems.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.scalingCheckedListBoxSetItems.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.scalingCheckedListBoxSetItems.CheckOnClick = true;
            this.scalingCheckedListBoxSetItems.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingCheckedListBoxSetItems.ForeColor = System.Drawing.Color.White;
            this.scalingCheckedListBoxSetItems.FormattingEnabled = true;
            this.scalingCheckedListBoxSetItems.HorizontalScrollbar = true;
            this.scalingCheckedListBoxSetItems.IntegralHeight = false;
            this.scalingCheckedListBoxSetItems.Items.AddRange(new object[] {
            "Set item 1",
            "Set item 2",
            "Set item 3"});
            this.scalingCheckedListBoxSetItems.Location = new System.Drawing.Point(4, 17);
            this.scalingCheckedListBoxSetItems.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.scalingCheckedListBoxSetItems.MultiColumn = true;
            this.scalingCheckedListBoxSetItems.Name = "scalingCheckedListBoxSetItems";
            this.scalingCheckedListBoxSetItems.Size = new System.Drawing.Size(215, 80);
            this.scalingCheckedListBoxSetItems.TabIndex = 7;
            this.scalingCheckedListBoxSetItems.SelectedValueChanged += new System.EventHandler(this.scalingCheckedListBox_SelectedValueChanged);
            this.scalingCheckedListBoxSetItems.MouseMove += new System.Windows.Forms.MouseEventHandler(this.scalingCheckedListBox_MouseMove);
            // 
            // flowLayoutPanelWithCharm
            // 
            this.flowLayoutPanelWithCharm.AutoSize = true;
            this.flowLayoutPanelWithCharm.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelWithCharm.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanelWithCharm.Controls.Add(this.scalingLabelWithCharm);
            this.flowLayoutPanelWithCharm.Controls.Add(this.scalingCheckedListBoxWithCharm);
            this.flowLayoutPanelWithCharm.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelWithCharm.Location = new System.Drawing.Point(693, 108);
            this.flowLayoutPanelWithCharm.MinimumSize = new System.Drawing.Size(50, 10);
            this.flowLayoutPanelWithCharm.Name = "flowLayoutPanelWithCharm";
            this.flowLayoutPanelWithCharm.Padding = new System.Windows.Forms.Padding(1, 2, 0, 0);
            this.flowLayoutPanelWithCharm.Size = new System.Drawing.Size(224, 99);
            this.flowLayoutPanelWithCharm.TabIndex = 17;
            // 
            // scalingLabelWithCharm
            // 
            this.scalingLabelWithCharm.AutoSize = true;
            this.scalingLabelWithCharm.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelWithCharm.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelWithCharm.Location = new System.Drawing.Point(4, 2);
            this.scalingLabelWithCharm.Name = "scalingLabelWithCharm";
            this.scalingLabelWithCharm.Size = new System.Drawing.Size(85, 15);
            this.scalingLabelWithCharm.TabIndex = 0;
            this.scalingLabelWithCharm.Text = "Having Charm";
            this.scalingLabelWithCharm.UseMnemonic = false;
            this.scalingLabelWithCharm.MouseClick += new System.Windows.Forms.MouseEventHandler(this.scalingLabelCategory_MouseClick);
            // 
            // scalingCheckedListBoxWithCharm
            // 
            this.scalingCheckedListBoxWithCharm.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.scalingCheckedListBoxWithCharm.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.scalingCheckedListBoxWithCharm.CheckOnClick = true;
            this.scalingCheckedListBoxWithCharm.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingCheckedListBoxWithCharm.ForeColor = System.Drawing.Color.White;
            this.scalingCheckedListBoxWithCharm.FormattingEnabled = true;
            this.scalingCheckedListBoxWithCharm.HorizontalScrollbar = true;
            this.scalingCheckedListBoxWithCharm.IntegralHeight = false;
            this.scalingCheckedListBoxWithCharm.Items.AddRange(new object[] {
            "Demon\'s Blood",
            "Turtle Shell",
            "Saber Claw"});
            this.scalingCheckedListBoxWithCharm.Location = new System.Drawing.Point(4, 17);
            this.scalingCheckedListBoxWithCharm.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.scalingCheckedListBoxWithCharm.MultiColumn = true;
            this.scalingCheckedListBoxWithCharm.Name = "scalingCheckedListBoxWithCharm";
            this.scalingCheckedListBoxWithCharm.Size = new System.Drawing.Size(215, 80);
            this.scalingCheckedListBoxWithCharm.TabIndex = 7;
            this.scalingCheckedListBoxWithCharm.SelectedValueChanged += new System.EventHandler(this.scalingCheckedListBox_SelectedValueChanged);
            this.scalingCheckedListBoxWithCharm.MouseMove += new System.Windows.Forms.MouseEventHandler(this.scalingCheckedListBox_MouseMove);
            // 
            // flowLayoutPanelWithRelic
            // 
            this.flowLayoutPanelWithRelic.AutoSize = true;
            this.flowLayoutPanelWithRelic.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelWithRelic.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanelWithRelic.Controls.Add(this.scalingLabelWithRelic);
            this.flowLayoutPanelWithRelic.Controls.Add(this.scalingCheckedListBoxWithRelic);
            this.flowLayoutPanelWithRelic.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelWithRelic.Location = new System.Drawing.Point(923, 108);
            this.flowLayoutPanelWithRelic.MinimumSize = new System.Drawing.Size(50, 10);
            this.flowLayoutPanelWithRelic.Name = "flowLayoutPanelWithRelic";
            this.flowLayoutPanelWithRelic.Padding = new System.Windows.Forms.Padding(1, 2, 0, 0);
            this.flowLayoutPanelWithRelic.Size = new System.Drawing.Size(224, 99);
            this.flowLayoutPanelWithRelic.TabIndex = 18;
            // 
            // scalingLabelWithRelic
            // 
            this.scalingLabelWithRelic.AutoSize = true;
            this.scalingLabelWithRelic.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelWithRelic.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelWithRelic.Location = new System.Drawing.Point(4, 2);
            this.scalingLabelWithRelic.Name = "scalingLabelWithRelic";
            this.scalingLabelWithRelic.Size = new System.Drawing.Size(76, 15);
            this.scalingLabelWithRelic.TabIndex = 0;
            this.scalingLabelWithRelic.Text = "Having Relic";
            this.scalingLabelWithRelic.UseMnemonic = false;
            this.scalingLabelWithRelic.MouseClick += new System.Windows.Forms.MouseEventHandler(this.scalingLabelCategory_MouseClick);
            // 
            // scalingCheckedListBoxWithRelic
            // 
            this.scalingCheckedListBoxWithRelic.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.scalingCheckedListBoxWithRelic.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.scalingCheckedListBoxWithRelic.CheckOnClick = true;
            this.scalingCheckedListBoxWithRelic.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingCheckedListBoxWithRelic.ForeColor = System.Drawing.Color.White;
            this.scalingCheckedListBoxWithRelic.FormattingEnabled = true;
            this.scalingCheckedListBoxWithRelic.HorizontalScrollbar = true;
            this.scalingCheckedListBoxWithRelic.IntegralHeight = false;
            this.scalingCheckedListBoxWithRelic.Items.AddRange(new object[] {
            "Aegis of Athena",
            "Anubis\' Wrath",
            "Rage of Ares"});
            this.scalingCheckedListBoxWithRelic.Location = new System.Drawing.Point(4, 17);
            this.scalingCheckedListBoxWithRelic.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.scalingCheckedListBoxWithRelic.MultiColumn = true;
            this.scalingCheckedListBoxWithRelic.Name = "scalingCheckedListBoxWithRelic";
            this.scalingCheckedListBoxWithRelic.Size = new System.Drawing.Size(215, 80);
            this.scalingCheckedListBoxWithRelic.TabIndex = 7;
            this.scalingCheckedListBoxWithRelic.SelectedValueChanged += new System.EventHandler(this.scalingCheckedListBox_SelectedValueChanged);
            this.scalingCheckedListBoxWithRelic.MouseMove += new System.Windows.Forms.MouseEventHandler(this.scalingCheckedListBox_MouseMove);
            // 
            // flowLayoutPanelPrefixName
            // 
            this.flowLayoutPanelPrefixName.AutoSize = true;
            this.flowLayoutPanelPrefixName.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelPrefixName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanelPrefixName.Controls.Add(this.scalingLabelPrefixName);
            this.flowLayoutPanelPrefixName.Controls.Add(this.scalingCheckedListBoxPrefixName);
            this.flowLayoutPanelPrefixName.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelPrefixName.Location = new System.Drawing.Point(233, 213);
            this.flowLayoutPanelPrefixName.MinimumSize = new System.Drawing.Size(50, 10);
            this.flowLayoutPanelPrefixName.Name = "flowLayoutPanelPrefixName";
            this.flowLayoutPanelPrefixName.Padding = new System.Windows.Forms.Padding(1, 2, 0, 0);
            this.flowLayoutPanelPrefixName.Size = new System.Drawing.Size(224, 99);
            this.flowLayoutPanelPrefixName.TabIndex = 16;
            // 
            // scalingLabelPrefixName
            // 
            this.scalingLabelPrefixName.AutoSize = true;
            this.scalingLabelPrefixName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelPrefixName.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelPrefixName.Location = new System.Drawing.Point(4, 2);
            this.scalingLabelPrefixName.Name = "scalingLabelPrefixName";
            this.scalingLabelPrefixName.Size = new System.Drawing.Size(75, 15);
            this.scalingLabelPrefixName.TabIndex = 0;
            this.scalingLabelPrefixName.Text = "Prefix Name";
            this.scalingLabelPrefixName.UseMnemonic = false;
            this.scalingLabelPrefixName.MouseClick += new System.Windows.Forms.MouseEventHandler(this.scalingLabelCategory_MouseClick);
            // 
            // scalingCheckedListBoxPrefixName
            // 
            this.scalingCheckedListBoxPrefixName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.scalingCheckedListBoxPrefixName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.scalingCheckedListBoxPrefixName.CheckOnClick = true;
            this.scalingCheckedListBoxPrefixName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingCheckedListBoxPrefixName.ForeColor = System.Drawing.Color.White;
            this.scalingCheckedListBoxPrefixName.FormattingEnabled = true;
            this.scalingCheckedListBoxPrefixName.HorizontalScrollbar = true;
            this.scalingCheckedListBoxPrefixName.IntegralHeight = false;
            this.scalingCheckedListBoxPrefixName.Items.AddRange(new object[] {
            "Divine",
            "Sacred",
            "Defender\'s"});
            this.scalingCheckedListBoxPrefixName.Location = new System.Drawing.Point(4, 17);
            this.scalingCheckedListBoxPrefixName.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.scalingCheckedListBoxPrefixName.MultiColumn = true;
            this.scalingCheckedListBoxPrefixName.Name = "scalingCheckedListBoxPrefixName";
            this.scalingCheckedListBoxPrefixName.Size = new System.Drawing.Size(215, 80);
            this.scalingCheckedListBoxPrefixName.TabIndex = 7;
            this.scalingCheckedListBoxPrefixName.SelectedValueChanged += new System.EventHandler(this.scalingCheckedListBox_SelectedValueChanged);
            this.scalingCheckedListBoxPrefixName.MouseMove += new System.Windows.Forms.MouseEventHandler(this.scalingCheckedListBox_MouseMove);
            // 
            // flowLayoutPanelSuffixName
            // 
            this.flowLayoutPanelSuffixName.AutoSize = true;
            this.flowLayoutPanelSuffixName.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelSuffixName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanelSuffixName.Controls.Add(this.scalingLabelSuffixName);
            this.flowLayoutPanelSuffixName.Controls.Add(this.scalingCheckedListBoxSuffixName);
            this.flowLayoutPanelSuffixName.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelSuffixName.Location = new System.Drawing.Point(923, 213);
            this.flowLayoutPanelSuffixName.MinimumSize = new System.Drawing.Size(50, 10);
            this.flowLayoutPanelSuffixName.Name = "flowLayoutPanelSuffixName";
            this.flowLayoutPanelSuffixName.Padding = new System.Windows.Forms.Padding(1, 2, 0, 0);
            this.flowLayoutPanelSuffixName.Size = new System.Drawing.Size(224, 99);
            this.flowLayoutPanelSuffixName.TabIndex = 15;
            // 
            // scalingLabelSuffixName
            // 
            this.scalingLabelSuffixName.AutoSize = true;
            this.scalingLabelSuffixName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelSuffixName.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelSuffixName.Location = new System.Drawing.Point(4, 2);
            this.scalingLabelSuffixName.Name = "scalingLabelSuffixName";
            this.scalingLabelSuffixName.Size = new System.Drawing.Size(74, 15);
            this.scalingLabelSuffixName.TabIndex = 0;
            this.scalingLabelSuffixName.Text = "Suffix Name";
            this.scalingLabelSuffixName.UseMnemonic = false;
            this.scalingLabelSuffixName.MouseClick += new System.Windows.Forms.MouseEventHandler(this.scalingLabelCategory_MouseClick);
            // 
            // scalingCheckedListBoxSuffixName
            // 
            this.scalingCheckedListBoxSuffixName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.scalingCheckedListBoxSuffixName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.scalingCheckedListBoxSuffixName.CheckOnClick = true;
            this.scalingCheckedListBoxSuffixName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingCheckedListBoxSuffixName.ForeColor = System.Drawing.Color.White;
            this.scalingCheckedListBoxSuffixName.FormattingEnabled = true;
            this.scalingCheckedListBoxSuffixName.HorizontalScrollbar = true;
            this.scalingCheckedListBoxSuffixName.IntegralHeight = false;
            this.scalingCheckedListBoxSuffixName.Items.AddRange(new object[] {
            "of Prowess",
            "of Finesse",
            "of Erudition"});
            this.scalingCheckedListBoxSuffixName.Location = new System.Drawing.Point(4, 17);
            this.scalingCheckedListBoxSuffixName.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.scalingCheckedListBoxSuffixName.MultiColumn = true;
            this.scalingCheckedListBoxSuffixName.Name = "scalingCheckedListBoxSuffixName";
            this.scalingCheckedListBoxSuffixName.Size = new System.Drawing.Size(215, 80);
            this.scalingCheckedListBoxSuffixName.TabIndex = 7;
            this.scalingCheckedListBoxSuffixName.SelectedValueChanged += new System.EventHandler(this.scalingCheckedListBox_SelectedValueChanged);
            this.scalingCheckedListBoxSuffixName.MouseMove += new System.Windows.Forms.MouseEventHandler(this.scalingCheckedListBox_MouseMove);
            // 
            // flowLayoutPanelSearchTerm
            // 
            this.flowLayoutPanelSearchTerm.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanelSearchTerm.AutoSize = true;
            this.flowLayoutPanelSearchTerm.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelSearchTerm.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tableLayoutPanelContent.SetColumnSpan(this.flowLayoutPanelSearchTerm, 2);
            this.flowLayoutPanelSearchTerm.Controls.Add(this.scalingLabelSearchTerm);
            this.flowLayoutPanelSearchTerm.Controls.Add(this.scalingTextBoxSearchTerm);
            this.flowLayoutPanelSearchTerm.Controls.Add(this.scalingButtonReset);
            this.flowLayoutPanelSearchTerm.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanelSearchTerm.Name = "flowLayoutPanelSearchTerm";
            this.flowLayoutPanelSearchTerm.Size = new System.Drawing.Size(753, 38);
            this.flowLayoutPanelSearchTerm.TabIndex = 16;
            // 
            // scalingLabelSearchTerm
            // 
            this.scalingLabelSearchTerm.AutoSize = true;
            this.scalingLabelSearchTerm.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelSearchTerm.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelSearchTerm.Location = new System.Drawing.Point(3, 7);
            this.scalingLabelSearchTerm.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            this.scalingLabelSearchTerm.Name = "scalingLabelSearchTerm";
            this.scalingLabelSearchTerm.Size = new System.Drawing.Size(52, 15);
            this.scalingLabelSearchTerm.TabIndex = 1;
            this.scalingLabelSearchTerm.Text = "Search :";
            this.scalingLabelSearchTerm.UseMnemonic = false;
            // 
            // scalingTextBoxSearchTerm
            // 
            this.scalingTextBoxSearchTerm.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.scalingTextBoxSearchTerm.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingTextBoxSearchTerm.Location = new System.Drawing.Point(61, 3);
            this.scalingTextBoxSearchTerm.Name = "scalingTextBoxSearchTerm";
            this.scalingTextBoxSearchTerm.Size = new System.Drawing.Size(387, 21);
            this.scalingTextBoxSearchTerm.TabIndex = 2;
            this.scalingTextBoxSearchTerm.TextChanged += new System.EventHandler(this.scalingTextBoxSearchTerm_TextChanged);
            // 
            // scalingButtonReset
            // 
            this.scalingButtonReset.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.scalingButtonReset.BackColor = System.Drawing.Color.Transparent;
            this.scalingButtonReset.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonReset.DownBitmap")));
            this.scalingButtonReset.FlatAppearance.BorderSize = 0;
            this.scalingButtonReset.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonReset.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.scalingButtonReset.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingButtonReset.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonReset.Image = ((System.Drawing.Image)(resources.GetObject("scalingButtonReset.Image")));
            this.scalingButtonReset.Location = new System.Drawing.Point(454, 3);
            this.scalingButtonReset.Name = "scalingButtonReset";
            this.scalingButtonReset.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonReset.OverBitmap")));
            this.scalingButtonReset.Size = new System.Drawing.Size(100, 30);
            this.scalingButtonReset.SizeToGraphic = false;
            this.scalingButtonReset.TabIndex = 7;
            this.scalingButtonReset.Text = "Reset";
            this.scalingButtonReset.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonReset.UpBitmap")));
            this.scalingButtonReset.UseCustomGraphic = true;
            this.scalingButtonReset.UseVisualStyleBackColor = false;
            this.scalingButtonReset.Click += new System.EventHandler(this.scalingButtonReset_Click);
            // 
            // flowLayoutPanelQueries
            // 
            this.flowLayoutPanelQueries.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanelQueries.AutoSize = true;
            this.flowLayoutPanelQueries.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelQueries.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanelQueries.Controls.Add(this.scalingLabelQueries);
            this.flowLayoutPanelQueries.Controls.Add(this.scalingComboBoxQueryList);
            this.flowLayoutPanelQueries.Controls.Add(this.scalingButtonQuerySave);
            this.flowLayoutPanelQueries.Controls.Add(this.scalingButtonQueryDelete);
            this.flowLayoutPanelQueries.Location = new System.Drawing.Point(762, 3);
            this.flowLayoutPanelQueries.Name = "flowLayoutPanelQueries";
            this.flowLayoutPanelQueries.Size = new System.Drawing.Size(620, 38);
            this.flowLayoutPanelQueries.TabIndex = 17;
            // 
            // scalingLabelQueries
            // 
            this.scalingLabelQueries.AutoSize = true;
            this.scalingLabelQueries.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelQueries.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelQueries.Location = new System.Drawing.Point(3, 7);
            this.scalingLabelQueries.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            this.scalingLabelQueries.Name = "scalingLabelQueries";
            this.scalingLabelQueries.Size = new System.Drawing.Size(56, 15);
            this.scalingLabelQueries.TabIndex = 2;
            this.scalingLabelQueries.Text = "Queries :";
            this.scalingLabelQueries.UseMnemonic = false;
            // 
            // scalingComboBoxQueryList
            // 
            this.scalingComboBoxQueryList.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.scalingComboBoxQueryList.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingComboBoxQueryList.FormattingEnabled = true;
            this.scalingComboBoxQueryList.Items.AddRange(new object[] {
            "Search term 1",
            "Search term 2"});
            this.scalingComboBoxQueryList.Location = new System.Drawing.Point(65, 3);
            this.scalingComboBoxQueryList.MaximumSize = new System.Drawing.Size(380, 0);
            this.scalingComboBoxQueryList.MinimumSize = new System.Drawing.Size(170, 0);
            this.scalingComboBoxQueryList.Name = "scalingComboBoxQueryList";
            this.scalingComboBoxQueryList.Size = new System.Drawing.Size(300, 22);
            this.scalingComboBoxQueryList.TabIndex = 3;
            this.scalingComboBoxQueryList.SelectedIndexChanged += new System.EventHandler(this.scalingComboBoxQueryList_SelectedIndexChanged);
            // 
            // scalingButtonQuerySave
            // 
            this.scalingButtonQuerySave.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.scalingButtonQuerySave.BackColor = System.Drawing.Color.Transparent;
            this.scalingButtonQuerySave.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonQuerySave.DownBitmap")));
            this.scalingButtonQuerySave.FlatAppearance.BorderSize = 0;
            this.scalingButtonQuerySave.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonQuerySave.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonQuerySave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.scalingButtonQuerySave.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingButtonQuerySave.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonQuerySave.Image = ((System.Drawing.Image)(resources.GetObject("scalingButtonQuerySave.Image")));
            this.scalingButtonQuerySave.Location = new System.Drawing.Point(371, 3);
            this.scalingButtonQuerySave.Name = "scalingButtonQuerySave";
            this.scalingButtonQuerySave.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonQuerySave.OverBitmap")));
            this.scalingButtonQuerySave.Size = new System.Drawing.Size(100, 30);
            this.scalingButtonQuerySave.SizeToGraphic = false;
            this.scalingButtonQuerySave.TabIndex = 4;
            this.scalingButtonQuerySave.Text = "Save";
            this.scalingButtonQuerySave.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonQuerySave.UpBitmap")));
            this.scalingButtonQuerySave.UseCustomGraphic = true;
            this.scalingButtonQuerySave.UseVisualStyleBackColor = false;
            this.scalingButtonQuerySave.Click += new System.EventHandler(this.scalingButtonQuerySave_Click);
            // 
            // scalingButtonQueryDelete
            // 
            this.scalingButtonQueryDelete.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.scalingButtonQueryDelete.BackColor = System.Drawing.Color.Transparent;
            this.scalingButtonQueryDelete.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonQueryDelete.DownBitmap")));
            this.scalingButtonQueryDelete.FlatAppearance.BorderSize = 0;
            this.scalingButtonQueryDelete.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonQueryDelete.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonQueryDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.scalingButtonQueryDelete.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingButtonQueryDelete.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonQueryDelete.Image = ((System.Drawing.Image)(resources.GetObject("scalingButtonQueryDelete.Image")));
            this.scalingButtonQueryDelete.Location = new System.Drawing.Point(477, 3);
            this.scalingButtonQueryDelete.Name = "scalingButtonQueryDelete";
            this.scalingButtonQueryDelete.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonQueryDelete.OverBitmap")));
            this.scalingButtonQueryDelete.Size = new System.Drawing.Size(100, 30);
            this.scalingButtonQueryDelete.SizeToGraphic = false;
            this.scalingButtonQueryDelete.TabIndex = 5;
            this.scalingButtonQueryDelete.Text = "Delete";
            this.scalingButtonQueryDelete.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonQueryDelete.UpBitmap")));
            this.scalingButtonQueryDelete.UseCustomGraphic = true;
            this.scalingButtonQueryDelete.UseVisualStyleBackColor = false;
            this.scalingButtonQueryDelete.Click += new System.EventHandler(this.scalingButtonQueryDelete_Click);
            // 
            // tableLayoutPanelContent
            // 
            this.tableLayoutPanelContent.AutoSize = true;
            this.tableLayoutPanelContent.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanelContent.ColumnCount = 3;
            this.tableLayoutPanelContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelContent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelContent.Controls.Add(this.flowLayoutPanelMainAutoScroll, 1, 4);
            this.tableLayoutPanelContent.Controls.Add(this.bufferedFlowLayoutPanelQuickFilters, 0, 3);
            this.tableLayoutPanelContent.Controls.Add(this.flowLayoutPanelSearchTerm, 0, 0);
            this.tableLayoutPanelContent.Controls.Add(this.flowLayoutPanelQueries, 2, 0);
            this.tableLayoutPanelContent.Controls.Add(this.flowLayoutPanelMisc, 0, 1);
            this.tableLayoutPanelContent.Controls.Add(this.bufferedFlowLayoutPanelRequierements, 0, 2);
            this.tableLayoutPanelContent.Controls.Add(this.flowLayoutPanelLeftColumn, 0, 4);
            this.tableLayoutPanelContent.Controls.Add(this.tableLayoutPanelBottom, 0, 5);
            this.tableLayoutPanelContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelContent.Location = new System.Drawing.Point(15, 25);
            this.tableLayoutPanelContent.Name = "tableLayoutPanelContent";
            this.tableLayoutPanelContent.RowCount = 6;
            this.tableLayoutPanelContent.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelContent.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelContent.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelContent.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelContent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tableLayoutPanelContent.Size = new System.Drawing.Size(1385, 835);
            this.tableLayoutPanelContent.TabIndex = 18;
            // 
            // flowLayoutPanelMainAutoScroll
            // 
            this.flowLayoutPanelMainAutoScroll.AutoScroll = true;
            this.tableLayoutPanelContent.SetColumnSpan(this.flowLayoutPanelMainAutoScroll, 2);
            this.flowLayoutPanelMainAutoScroll.Controls.Add(this.flowLayoutPanelMain);
            this.flowLayoutPanelMainAutoScroll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelMainAutoScroll.Location = new System.Drawing.Point(134, 144);
            this.flowLayoutPanelMainAutoScroll.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanelMainAutoScroll.Name = "flowLayoutPanelMainAutoScroll";
            this.flowLayoutPanelMainAutoScroll.Size = new System.Drawing.Size(1251, 601);
            this.flowLayoutPanelMainAutoScroll.TabIndex = 21;
            // 
            // bufferedFlowLayoutPanelQuickFilters
            // 
            this.bufferedFlowLayoutPanelQuickFilters.AutoSize = true;
            this.bufferedFlowLayoutPanelQuickFilters.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tableLayoutPanelContent.SetColumnSpan(this.bufferedFlowLayoutPanelQuickFilters, 3);
            this.bufferedFlowLayoutPanelQuickFilters.Controls.Add(this.scalingCheckBoxHavingPrefix);
            this.bufferedFlowLayoutPanelQuickFilters.Controls.Add(this.scalingCheckBoxHavingSuffix);
            this.bufferedFlowLayoutPanelQuickFilters.Controls.Add(this.scalingCheckBoxHavingRelic);
            this.bufferedFlowLayoutPanelQuickFilters.Controls.Add(this.scalingCheckBoxHavingCharm);
            this.bufferedFlowLayoutPanelQuickFilters.Controls.Add(this.scalingCheckBoxIsSetItem);
            this.bufferedFlowLayoutPanelQuickFilters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bufferedFlowLayoutPanelQuickFilters.Location = new System.Drawing.Point(3, 120);
            this.bufferedFlowLayoutPanelQuickFilters.Name = "bufferedFlowLayoutPanelQuickFilters";
            this.bufferedFlowLayoutPanelQuickFilters.Size = new System.Drawing.Size(1379, 21);
            this.bufferedFlowLayoutPanelQuickFilters.TabIndex = 32;
            // 
            // scalingCheckBoxHavingPrefix
            // 
            this.scalingCheckBoxHavingPrefix.AutoSize = true;
            this.scalingCheckBoxHavingPrefix.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingCheckBoxHavingPrefix.ForeColor = System.Drawing.Color.Gold;
            this.scalingCheckBoxHavingPrefix.Location = new System.Drawing.Point(3, 0);
            this.scalingCheckBoxHavingPrefix.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.scalingCheckBoxHavingPrefix.Name = "scalingCheckBoxHavingPrefix";
            this.scalingCheckBoxHavingPrefix.Size = new System.Drawing.Size(98, 19);
            this.scalingCheckBoxHavingPrefix.TabIndex = 0;
            this.scalingCheckBoxHavingPrefix.Text = "Having Prefix";
            this.scalingCheckBoxHavingPrefix.UseVisualStyleBackColor = true;
            this.scalingCheckBoxHavingPrefix.CheckedChanged += new System.EventHandler(this.scalingCheckBoxQuickFilters_CheckedChanged);
            // 
            // scalingCheckBoxHavingSuffix
            // 
            this.scalingCheckBoxHavingSuffix.AutoSize = true;
            this.scalingCheckBoxHavingSuffix.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingCheckBoxHavingSuffix.ForeColor = System.Drawing.Color.Gold;
            this.scalingCheckBoxHavingSuffix.Location = new System.Drawing.Point(107, 0);
            this.scalingCheckBoxHavingSuffix.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.scalingCheckBoxHavingSuffix.Name = "scalingCheckBoxHavingSuffix";
            this.scalingCheckBoxHavingSuffix.Size = new System.Drawing.Size(97, 19);
            this.scalingCheckBoxHavingSuffix.TabIndex = 1;
            this.scalingCheckBoxHavingSuffix.Text = "Having Suffix";
            this.scalingCheckBoxHavingSuffix.UseVisualStyleBackColor = true;
            this.scalingCheckBoxHavingSuffix.CheckedChanged += new System.EventHandler(this.scalingCheckBoxQuickFilters_CheckedChanged);
            // 
            // scalingCheckBoxHavingRelic
            // 
            this.scalingCheckBoxHavingRelic.AutoSize = true;
            this.scalingCheckBoxHavingRelic.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingCheckBoxHavingRelic.ForeColor = System.Drawing.Color.Gold;
            this.scalingCheckBoxHavingRelic.Location = new System.Drawing.Point(210, 0);
            this.scalingCheckBoxHavingRelic.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.scalingCheckBoxHavingRelic.Name = "scalingCheckBoxHavingRelic";
            this.scalingCheckBoxHavingRelic.Size = new System.Drawing.Size(95, 19);
            this.scalingCheckBoxHavingRelic.TabIndex = 2;
            this.scalingCheckBoxHavingRelic.Text = "Having Relic";
            this.scalingCheckBoxHavingRelic.UseVisualStyleBackColor = true;
            this.scalingCheckBoxHavingRelic.CheckedChanged += new System.EventHandler(this.scalingCheckBoxQuickFilters_CheckedChanged);
            // 
            // scalingCheckBoxHavingCharm
            // 
            this.scalingCheckBoxHavingCharm.AutoSize = true;
            this.scalingCheckBoxHavingCharm.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingCheckBoxHavingCharm.ForeColor = System.Drawing.Color.Gold;
            this.scalingCheckBoxHavingCharm.Location = new System.Drawing.Point(311, 0);
            this.scalingCheckBoxHavingCharm.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.scalingCheckBoxHavingCharm.Name = "scalingCheckBoxHavingCharm";
            this.scalingCheckBoxHavingCharm.Size = new System.Drawing.Size(104, 19);
            this.scalingCheckBoxHavingCharm.TabIndex = 3;
            this.scalingCheckBoxHavingCharm.Text = "Having Charm";
            this.scalingCheckBoxHavingCharm.UseVisualStyleBackColor = true;
            this.scalingCheckBoxHavingCharm.CheckedChanged += new System.EventHandler(this.scalingCheckBoxQuickFilters_CheckedChanged);
            // 
            // scalingCheckBoxIsSetItem
            // 
            this.scalingCheckBoxIsSetItem.AutoSize = true;
            this.scalingCheckBoxIsSetItem.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingCheckBoxIsSetItem.ForeColor = System.Drawing.Color.Gold;
            this.scalingCheckBoxIsSetItem.Location = new System.Drawing.Point(421, 0);
            this.scalingCheckBoxIsSetItem.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.scalingCheckBoxIsSetItem.Name = "scalingCheckBoxIsSetItem";
            this.scalingCheckBoxIsSetItem.Size = new System.Drawing.Size(71, 19);
            this.scalingCheckBoxIsSetItem.TabIndex = 4;
            this.scalingCheckBoxIsSetItem.Text = "Set Item";
            this.scalingCheckBoxIsSetItem.UseVisualStyleBackColor = true;
            this.scalingCheckBoxIsSetItem.CheckedChanged += new System.EventHandler(this.scalingCheckBoxQuickFilters_CheckedChanged);
            // 
            // flowLayoutPanelMisc
            // 
            this.flowLayoutPanelMisc.AutoSize = true;
            this.flowLayoutPanelMisc.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelMisc.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tableLayoutPanelContent.SetColumnSpan(this.flowLayoutPanelMisc, 3);
            this.flowLayoutPanelMisc.Controls.Add(this.scalingLabelMaxVisibleElement);
            this.flowLayoutPanelMisc.Controls.Add(this.numericUpDownMaxElement);
            this.flowLayoutPanelMisc.Controls.Add(this.scalingCheckBoxReduceDuringSelection);
            this.flowLayoutPanelMisc.Controls.Add(this.buttonCollapseAll);
            this.flowLayoutPanelMisc.Controls.Add(this.buttonExpandAll);
            this.flowLayoutPanelMisc.Controls.Add(this.scalingLabelOperator);
            this.flowLayoutPanelMisc.Controls.Add(this.scalingComboBoxOperator);
            this.flowLayoutPanelMisc.Controls.Add(this.scalingLabelFiltersSelected);
            this.flowLayoutPanelMisc.Controls.Add(this.scalingLabelFilterCategories);
            this.flowLayoutPanelMisc.Controls.Add(this.scalingTextBoxFilterCategories);
            this.flowLayoutPanelMisc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelMisc.Location = new System.Drawing.Point(3, 47);
            this.flowLayoutPanelMisc.Name = "flowLayoutPanelMisc";
            this.flowLayoutPanelMisc.Size = new System.Drawing.Size(1379, 33);
            this.flowLayoutPanelMisc.TabIndex = 18;
            // 
            // scalingLabelMaxVisibleElement
            // 
            this.scalingLabelMaxVisibleElement.AutoSize = true;
            this.scalingLabelMaxVisibleElement.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelMaxVisibleElement.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelMaxVisibleElement.Location = new System.Drawing.Point(3, 7);
            this.scalingLabelMaxVisibleElement.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            this.scalingLabelMaxVisibleElement.Name = "scalingLabelMaxVisibleElement";
            this.scalingLabelMaxVisibleElement.Size = new System.Drawing.Size(152, 15);
            this.scalingLabelMaxVisibleElement.TabIndex = 5;
            this.scalingLabelMaxVisibleElement.Text = "Visible elements/category :";
            this.scalingLabelMaxVisibleElement.UseMnemonic = false;
            // 
            // numericUpDownMaxElement
            // 
            this.numericUpDownMaxElement.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numericUpDownMaxElement.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.numericUpDownMaxElement.Location = new System.Drawing.Point(161, 3);
            this.numericUpDownMaxElement.Maximum = new decimal(new int[] {
            150,
            0,
            0,
            0});
            this.numericUpDownMaxElement.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownMaxElement.Name = "numericUpDownMaxElement";
            this.numericUpDownMaxElement.Size = new System.Drawing.Size(43, 20);
            this.numericUpDownMaxElement.TabIndex = 4;
            this.numericUpDownMaxElement.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownMaxElement.ValueChanged += new System.EventHandler(this.numericUpDownMaxElement_ValueChanged);
            // 
            // scalingCheckBoxReduceDuringSelection
            // 
            this.scalingCheckBoxReduceDuringSelection.AutoSize = true;
            this.scalingCheckBoxReduceDuringSelection.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingCheckBoxReduceDuringSelection.ForeColor = System.Drawing.Color.Gold;
            this.scalingCheckBoxReduceDuringSelection.Location = new System.Drawing.Point(220, 7);
            this.scalingCheckBoxReduceDuringSelection.Margin = new System.Windows.Forms.Padding(13, 7, 3, 3);
            this.scalingCheckBoxReduceDuringSelection.Name = "scalingCheckBoxReduceDuringSelection";
            this.scalingCheckBoxReduceDuringSelection.Size = new System.Drawing.Size(219, 19);
            this.scalingCheckBoxReduceDuringSelection.TabIndex = 3;
            this.scalingCheckBoxReduceDuringSelection.Text = "Reduce categories during selection";
            this.scalingCheckBoxReduceDuringSelection.UseVisualStyleBackColor = true;
            this.scalingCheckBoxReduceDuringSelection.CheckedChanged += new System.EventHandler(this.scalingCheckBoxReduceDuringSelection_CheckedChanged);
            // 
            // buttonCollapseAll
            // 
            this.buttonCollapseAll.AutoSize = true;
            this.buttonCollapseAll.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonCollapseAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(190)))), ((int)(((byte)(107)))));
            this.buttonCollapseAll.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonCollapseAll.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.buttonCollapseAll.Location = new System.Drawing.Point(445, 3);
            this.buttonCollapseAll.Name = "buttonCollapseAll";
            this.buttonCollapseAll.Size = new System.Drawing.Size(81, 25);
            this.buttonCollapseAll.TabIndex = 6;
            this.buttonCollapseAll.Text = "Collapse All";
            this.buttonCollapseAll.UseVisualStyleBackColor = false;
            this.buttonCollapseAll.Click += new System.EventHandler(this.buttonCollapseAll_Click);
            // 
            // buttonExpandAll
            // 
            this.buttonExpandAll.AutoSize = true;
            this.buttonExpandAll.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonExpandAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(190)))), ((int)(((byte)(107)))));
            this.buttonExpandAll.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonExpandAll.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.buttonExpandAll.Location = new System.Drawing.Point(532, 3);
            this.buttonExpandAll.Name = "buttonExpandAll";
            this.buttonExpandAll.Size = new System.Drawing.Size(75, 25);
            this.buttonExpandAll.TabIndex = 7;
            this.buttonExpandAll.Text = "Expand All";
            this.buttonExpandAll.UseVisualStyleBackColor = false;
            this.buttonExpandAll.Click += new System.EventHandler(this.buttonExpandAll_Click);
            // 
            // scalingLabelOperator
            // 
            this.scalingLabelOperator.AutoSize = true;
            this.scalingLabelOperator.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelOperator.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelOperator.Location = new System.Drawing.Point(613, 7);
            this.scalingLabelOperator.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            this.scalingLabelOperator.Name = "scalingLabelOperator";
            this.scalingLabelOperator.Size = new System.Drawing.Size(61, 15);
            this.scalingLabelOperator.TabIndex = 17;
            this.scalingLabelOperator.Text = "Operator :";
            this.scalingLabelOperator.UseMnemonic = false;
            // 
            // scalingComboBoxOperator
            // 
            this.scalingComboBoxOperator.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.scalingComboBoxOperator.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.scalingComboBoxOperator.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingComboBoxOperator.FormattingEnabled = true;
            this.scalingComboBoxOperator.Items.AddRange(new object[] {
            "And",
            "Or"});
            this.scalingComboBoxOperator.Location = new System.Drawing.Point(680, 3);
            this.scalingComboBoxOperator.MaxDropDownItems = 2;
            this.scalingComboBoxOperator.Name = "scalingComboBoxOperator";
            this.scalingComboBoxOperator.Size = new System.Drawing.Size(60, 22);
            this.scalingComboBoxOperator.TabIndex = 18;
            this.scalingComboBoxOperator.SelectionChangeCommitted += new System.EventHandler(this.scalingComboBoxOperator_SelectionChangeCommitted);
            // 
            // scalingLabelFiltersSelected
            // 
            this.scalingLabelFiltersSelected.AutoSize = true;
            this.scalingLabelFiltersSelected.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelFiltersSelected.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelFiltersSelected.Location = new System.Drawing.Point(746, 7);
            this.scalingLabelFiltersSelected.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            this.scalingLabelFiltersSelected.Name = "scalingLabelFiltersSelected";
            this.scalingLabelFiltersSelected.Size = new System.Drawing.Size(103, 15);
            this.scalingLabelFiltersSelected.TabIndex = 8;
            this.scalingLabelFiltersSelected.Tag = "{0} filters selected";
            this.scalingLabelFiltersSelected.Text = "{0} filters selected";
            this.scalingLabelFiltersSelected.UseMnemonic = false;
            this.scalingLabelFiltersSelected.MouseEnter += new System.EventHandler(this.scalingLabelFiltersSelected_MouseEnter);
            this.scalingLabelFiltersSelected.MouseLeave += new System.EventHandler(this.scalingLabelFiltersSelected_MouseLeave);
            // 
            // scalingLabelFilterCategories
            // 
            this.scalingLabelFilterCategories.AutoSize = true;
            this.scalingLabelFilterCategories.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelFilterCategories.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelFilterCategories.Location = new System.Drawing.Point(855, 7);
            this.scalingLabelFilterCategories.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            this.scalingLabelFilterCategories.Name = "scalingLabelFilterCategories";
            this.scalingLabelFilterCategories.Size = new System.Drawing.Size(100, 15);
            this.scalingLabelFilterCategories.TabIndex = 19;
            this.scalingLabelFilterCategories.Text = "Filter categories :";
            this.scalingLabelFilterCategories.UseMnemonic = false;
            // 
            // scalingTextBoxFilterCategories
            // 
            this.scalingTextBoxFilterCategories.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.scalingTextBoxFilterCategories.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingTextBoxFilterCategories.Location = new System.Drawing.Point(961, 3);
            this.scalingTextBoxFilterCategories.Name = "scalingTextBoxFilterCategories";
            this.scalingTextBoxFilterCategories.Size = new System.Drawing.Size(200, 21);
            this.scalingTextBoxFilterCategories.TabIndex = 20;
            this.scalingTextBoxFilterCategories.TextChanged += new System.EventHandler(this.scalingTextBoxFilterCategories_TextChanged);
            // 
            // bufferedFlowLayoutPanelRequierements
            // 
            this.bufferedFlowLayoutPanelRequierements.AutoSize = true;
            this.bufferedFlowLayoutPanelRequierements.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tableLayoutPanelContent.SetColumnSpan(this.bufferedFlowLayoutPanelRequierements, 3);
            this.bufferedFlowLayoutPanelRequierements.Controls.Add(this.tableLayoutPanelRequierements);
            this.bufferedFlowLayoutPanelRequierements.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bufferedFlowLayoutPanelRequierements.Location = new System.Drawing.Point(3, 86);
            this.bufferedFlowLayoutPanelRequierements.Name = "bufferedFlowLayoutPanelRequierements";
            this.bufferedFlowLayoutPanelRequierements.Size = new System.Drawing.Size(1379, 28);
            this.bufferedFlowLayoutPanelRequierements.TabIndex = 5;
            // 
            // tableLayoutPanelRequierements
            // 
            this.tableLayoutPanelRequierements.AutoSize = true;
            this.tableLayoutPanelRequierements.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanelRequierements.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanelRequierements.ColumnCount = 5;
            this.tableLayoutPanelRequierements.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelRequierements.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelRequierements.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanelRequierements.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelRequierements.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelRequierements.Controls.Add(this.scalingCheckBoxMinReq, 0, 0);
            this.tableLayoutPanelRequierements.Controls.Add(this.flowLayoutPanelMin, 1, 0);
            this.tableLayoutPanelRequierements.Controls.Add(this.scalingCheckBoxMaxReq, 3, 0);
            this.tableLayoutPanelRequierements.Controls.Add(this.flowLayoutPanelMax, 4, 0);
            this.tableLayoutPanelRequierements.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelRequierements.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanelRequierements.Name = "tableLayoutPanelRequierements";
            this.tableLayoutPanelRequierements.RowCount = 1;
            this.tableLayoutPanelRequierements.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelRequierements.Size = new System.Drawing.Size(1009, 26);
            this.tableLayoutPanelRequierements.TabIndex = 25;
            // 
            // scalingCheckBoxMinReq
            // 
            this.scalingCheckBoxMinReq.AutoSize = true;
            this.scalingCheckBoxMinReq.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingCheckBoxMinReq.ForeColor = System.Drawing.Color.Gold;
            this.scalingCheckBoxMinReq.Location = new System.Drawing.Point(3, 3);
            this.scalingCheckBoxMinReq.Name = "scalingCheckBoxMinReq";
            this.scalingCheckBoxMinReq.Size = new System.Drawing.Size(135, 19);
            this.scalingCheckBoxMinReq.TabIndex = 3;
            this.scalingCheckBoxMinReq.Text = "Min Requierement :";
            this.scalingCheckBoxMinReq.UseVisualStyleBackColor = true;
            this.scalingCheckBoxMinReq.CheckedChanged += new System.EventHandler(this.scalingCheckBoxMinMaxReq_CheckedChanged);
            // 
            // flowLayoutPanelMin
            // 
            this.flowLayoutPanelMin.AutoSize = true;
            this.flowLayoutPanelMin.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelMin.Controls.Add(this.scalingLabelMinLvl);
            this.flowLayoutPanelMin.Controls.Add(this.numericUpDownMinLvl);
            this.flowLayoutPanelMin.Controls.Add(this.scalingLabelMinStr);
            this.flowLayoutPanelMin.Controls.Add(this.numericUpDownMinStr);
            this.flowLayoutPanelMin.Controls.Add(this.scalingLabelMinDex);
            this.flowLayoutPanelMin.Controls.Add(this.numericUpDownMinDex);
            this.flowLayoutPanelMin.Controls.Add(this.scalingLabelMinInt);
            this.flowLayoutPanelMin.Controls.Add(this.numericUpDownMinInt);
            this.flowLayoutPanelMin.Location = new System.Drawing.Point(144, 3);
            this.flowLayoutPanelMin.Name = "flowLayoutPanelMin";
            this.flowLayoutPanelMin.Size = new System.Drawing.Size(331, 20);
            this.flowLayoutPanelMin.TabIndex = 4;
            this.flowLayoutPanelMin.WrapContents = false;
            // 
            // scalingLabelMinLvl
            // 
            this.scalingLabelMinLvl.AutoSize = true;
            this.scalingLabelMinLvl.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelMinLvl.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelMinLvl.Location = new System.Drawing.Point(3, 0);
            this.scalingLabelMinLvl.Name = "scalingLabelMinLvl";
            this.scalingLabelMinLvl.Size = new System.Drawing.Size(42, 15);
            this.scalingLabelMinLvl.TabIndex = 0;
            this.scalingLabelMinLvl.Text = "Level :";
            // 
            // numericUpDownMinLvl
            // 
            this.numericUpDownMinLvl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.numericUpDownMinLvl.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownMinLvl.Location = new System.Drawing.Point(48, 0);
            this.numericUpDownMinLvl.Margin = new System.Windows.Forms.Padding(0);
            this.numericUpDownMinLvl.Name = "numericUpDownMinLvl";
            this.numericUpDownMinLvl.Size = new System.Drawing.Size(44, 20);
            this.numericUpDownMinLvl.TabIndex = 1;
            this.numericUpDownMinLvl.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownMinLvl.ValueChanged += new System.EventHandler(this.numericUpDownMinMax_ValueChanged);
            // 
            // scalingLabelMinStr
            // 
            this.scalingLabelMinStr.AutoSize = true;
            this.scalingLabelMinStr.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelMinStr.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelMinStr.Location = new System.Drawing.Point(95, 0);
            this.scalingLabelMinStr.Name = "scalingLabelMinStr";
            this.scalingLabelMinStr.Size = new System.Drawing.Size(28, 15);
            this.scalingLabelMinStr.TabIndex = 2;
            this.scalingLabelMinStr.Text = "Str :";
            // 
            // numericUpDownMinStr
            // 
            this.numericUpDownMinStr.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.numericUpDownMinStr.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownMinStr.Location = new System.Drawing.Point(126, 0);
            this.numericUpDownMinStr.Margin = new System.Windows.Forms.Padding(0);
            this.numericUpDownMinStr.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numericUpDownMinStr.Name = "numericUpDownMinStr";
            this.numericUpDownMinStr.Size = new System.Drawing.Size(44, 20);
            this.numericUpDownMinStr.TabIndex = 3;
            this.numericUpDownMinStr.Value = new decimal(new int[] {
            9000,
            0,
            0,
            0});
            this.numericUpDownMinStr.ValueChanged += new System.EventHandler(this.numericUpDownMinMax_ValueChanged);
            // 
            // scalingLabelMinDex
            // 
            this.scalingLabelMinDex.AutoSize = true;
            this.scalingLabelMinDex.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelMinDex.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelMinDex.Location = new System.Drawing.Point(173, 0);
            this.scalingLabelMinDex.Name = "scalingLabelMinDex";
            this.scalingLabelMinDex.Size = new System.Drawing.Size(35, 15);
            this.scalingLabelMinDex.TabIndex = 4;
            this.scalingLabelMinDex.Text = "Dex :";
            // 
            // numericUpDownMinDex
            // 
            this.numericUpDownMinDex.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.numericUpDownMinDex.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownMinDex.Location = new System.Drawing.Point(211, 0);
            this.numericUpDownMinDex.Margin = new System.Windows.Forms.Padding(0);
            this.numericUpDownMinDex.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numericUpDownMinDex.Name = "numericUpDownMinDex";
            this.numericUpDownMinDex.Size = new System.Drawing.Size(44, 20);
            this.numericUpDownMinDex.TabIndex = 5;
            this.numericUpDownMinDex.Value = new decimal(new int[] {
            9000,
            0,
            0,
            0});
            this.numericUpDownMinDex.ValueChanged += new System.EventHandler(this.numericUpDownMinMax_ValueChanged);
            // 
            // scalingLabelMinInt
            // 
            this.scalingLabelMinInt.AutoSize = true;
            this.scalingLabelMinInt.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelMinInt.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelMinInt.Location = new System.Drawing.Point(258, 0);
            this.scalingLabelMinInt.Name = "scalingLabelMinInt";
            this.scalingLabelMinInt.Size = new System.Drawing.Size(26, 15);
            this.scalingLabelMinInt.TabIndex = 6;
            this.scalingLabelMinInt.Text = "Int :";
            // 
            // numericUpDownMinInt
            // 
            this.numericUpDownMinInt.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.numericUpDownMinInt.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownMinInt.Location = new System.Drawing.Point(287, 0);
            this.numericUpDownMinInt.Margin = new System.Windows.Forms.Padding(0);
            this.numericUpDownMinInt.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numericUpDownMinInt.Name = "numericUpDownMinInt";
            this.numericUpDownMinInt.Size = new System.Drawing.Size(44, 20);
            this.numericUpDownMinInt.TabIndex = 7;
            this.numericUpDownMinInt.Value = new decimal(new int[] {
            9000,
            0,
            0,
            0});
            this.numericUpDownMinInt.ValueChanged += new System.EventHandler(this.numericUpDownMinMax_ValueChanged);
            // 
            // scalingCheckBoxMaxReq
            // 
            this.scalingCheckBoxMaxReq.AutoSize = true;
            this.scalingCheckBoxMaxReq.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingCheckBoxMaxReq.ForeColor = System.Drawing.Color.Gold;
            this.scalingCheckBoxMaxReq.Location = new System.Drawing.Point(531, 3);
            this.scalingCheckBoxMaxReq.Name = "scalingCheckBoxMaxReq";
            this.scalingCheckBoxMaxReq.Size = new System.Drawing.Size(138, 19);
            this.scalingCheckBoxMaxReq.TabIndex = 5;
            this.scalingCheckBoxMaxReq.Text = "Max Requierement :";
            this.scalingCheckBoxMaxReq.UseVisualStyleBackColor = true;
            this.scalingCheckBoxMaxReq.CheckedChanged += new System.EventHandler(this.scalingCheckBoxMinMaxReq_CheckedChanged);
            // 
            // flowLayoutPanelMax
            // 
            this.flowLayoutPanelMax.AutoSize = true;
            this.flowLayoutPanelMax.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelMax.Controls.Add(this.scalingLabelMaxLvl);
            this.flowLayoutPanelMax.Controls.Add(this.numericUpDownMaxLvl);
            this.flowLayoutPanelMax.Controls.Add(this.scalingLabelMaxStr);
            this.flowLayoutPanelMax.Controls.Add(this.numericUpDownMaxStr);
            this.flowLayoutPanelMax.Controls.Add(this.scalingLabelMaxDex);
            this.flowLayoutPanelMax.Controls.Add(this.numericUpDownMaxDex);
            this.flowLayoutPanelMax.Controls.Add(this.scalingLabelMaxInt);
            this.flowLayoutPanelMax.Controls.Add(this.numericUpDownMaxInt);
            this.flowLayoutPanelMax.Location = new System.Drawing.Point(675, 3);
            this.flowLayoutPanelMax.Name = "flowLayoutPanelMax";
            this.flowLayoutPanelMax.Size = new System.Drawing.Size(331, 20);
            this.flowLayoutPanelMax.TabIndex = 25;
            this.flowLayoutPanelMax.WrapContents = false;
            // 
            // scalingLabelMaxLvl
            // 
            this.scalingLabelMaxLvl.AutoSize = true;
            this.scalingLabelMaxLvl.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelMaxLvl.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelMaxLvl.Location = new System.Drawing.Point(3, 0);
            this.scalingLabelMaxLvl.Name = "scalingLabelMaxLvl";
            this.scalingLabelMaxLvl.Size = new System.Drawing.Size(42, 15);
            this.scalingLabelMaxLvl.TabIndex = 0;
            this.scalingLabelMaxLvl.Text = "Level :";
            // 
            // numericUpDownMaxLvl
            // 
            this.numericUpDownMaxLvl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.numericUpDownMaxLvl.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownMaxLvl.Location = new System.Drawing.Point(48, 0);
            this.numericUpDownMaxLvl.Margin = new System.Windows.Forms.Padding(0);
            this.numericUpDownMaxLvl.Name = "numericUpDownMaxLvl";
            this.numericUpDownMaxLvl.Size = new System.Drawing.Size(44, 20);
            this.numericUpDownMaxLvl.TabIndex = 1;
            this.numericUpDownMaxLvl.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownMaxLvl.ValueChanged += new System.EventHandler(this.numericUpDownMinMax_ValueChanged);
            // 
            // scalingLabelMaxStr
            // 
            this.scalingLabelMaxStr.AutoSize = true;
            this.scalingLabelMaxStr.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelMaxStr.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelMaxStr.Location = new System.Drawing.Point(95, 0);
            this.scalingLabelMaxStr.Name = "scalingLabelMaxStr";
            this.scalingLabelMaxStr.Size = new System.Drawing.Size(28, 15);
            this.scalingLabelMaxStr.TabIndex = 2;
            this.scalingLabelMaxStr.Text = "Str :";
            // 
            // numericUpDownMaxStr
            // 
            this.numericUpDownMaxStr.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.numericUpDownMaxStr.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownMaxStr.Location = new System.Drawing.Point(126, 0);
            this.numericUpDownMaxStr.Margin = new System.Windows.Forms.Padding(0);
            this.numericUpDownMaxStr.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numericUpDownMaxStr.Name = "numericUpDownMaxStr";
            this.numericUpDownMaxStr.Size = new System.Drawing.Size(44, 20);
            this.numericUpDownMaxStr.TabIndex = 3;
            this.numericUpDownMaxStr.Value = new decimal(new int[] {
            9000,
            0,
            0,
            0});
            this.numericUpDownMaxStr.ValueChanged += new System.EventHandler(this.numericUpDownMinMax_ValueChanged);
            // 
            // scalingLabelMaxDex
            // 
            this.scalingLabelMaxDex.AutoSize = true;
            this.scalingLabelMaxDex.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelMaxDex.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelMaxDex.Location = new System.Drawing.Point(173, 0);
            this.scalingLabelMaxDex.Name = "scalingLabelMaxDex";
            this.scalingLabelMaxDex.Size = new System.Drawing.Size(35, 15);
            this.scalingLabelMaxDex.TabIndex = 4;
            this.scalingLabelMaxDex.Text = "Dex :";
            // 
            // numericUpDownMaxDex
            // 
            this.numericUpDownMaxDex.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.numericUpDownMaxDex.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownMaxDex.Location = new System.Drawing.Point(211, 0);
            this.numericUpDownMaxDex.Margin = new System.Windows.Forms.Padding(0);
            this.numericUpDownMaxDex.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numericUpDownMaxDex.Name = "numericUpDownMaxDex";
            this.numericUpDownMaxDex.Size = new System.Drawing.Size(44, 20);
            this.numericUpDownMaxDex.TabIndex = 5;
            this.numericUpDownMaxDex.Value = new decimal(new int[] {
            9000,
            0,
            0,
            0});
            this.numericUpDownMaxDex.ValueChanged += new System.EventHandler(this.numericUpDownMinMax_ValueChanged);
            // 
            // scalingLabelMaxInt
            // 
            this.scalingLabelMaxInt.AutoSize = true;
            this.scalingLabelMaxInt.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.scalingLabelMaxInt.ForeColor = System.Drawing.Color.Gold;
            this.scalingLabelMaxInt.Location = new System.Drawing.Point(258, 0);
            this.scalingLabelMaxInt.Name = "scalingLabelMaxInt";
            this.scalingLabelMaxInt.Size = new System.Drawing.Size(26, 15);
            this.scalingLabelMaxInt.TabIndex = 6;
            this.scalingLabelMaxInt.Text = "Int :";
            // 
            // numericUpDownMaxInt
            // 
            this.numericUpDownMaxInt.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.numericUpDownMaxInt.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownMaxInt.Location = new System.Drawing.Point(287, 0);
            this.numericUpDownMaxInt.Margin = new System.Windows.Forms.Padding(0);
            this.numericUpDownMaxInt.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numericUpDownMaxInt.Name = "numericUpDownMaxInt";
            this.numericUpDownMaxInt.Size = new System.Drawing.Size(44, 20);
            this.numericUpDownMaxInt.TabIndex = 7;
            this.numericUpDownMaxInt.Value = new decimal(new int[] {
            9000,
            0,
            0,
            0});
            this.numericUpDownMaxInt.ValueChanged += new System.EventHandler(this.numericUpDownMinMax_ValueChanged);
            // 
            // flowLayoutPanelLeftColumn
            // 
            this.flowLayoutPanelLeftColumn.AutoSize = true;
            this.flowLayoutPanelLeftColumn.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowLayoutPanelLeftColumn.Controls.Add(this.scalingButtonMenuCharacters);
            this.flowLayoutPanelLeftColumn.Controls.Add(this.scalingButtonMenuVaults);
            this.flowLayoutPanelLeftColumn.Controls.Add(this.scalingButtonMenuType);
            this.flowLayoutPanelLeftColumn.Controls.Add(this.scalingButtonMenuRarity);
            this.flowLayoutPanelLeftColumn.Controls.Add(this.scalingButtonMenuOrigin);
            this.flowLayoutPanelLeftColumn.Controls.Add(this.scalingButtonMenuQuality);
            this.flowLayoutPanelLeftColumn.Controls.Add(this.scalingButtonMenuStyle);
            this.flowLayoutPanelLeftColumn.Controls.Add(this.scalingButtonMenuSetItems);
            this.flowLayoutPanelLeftColumn.Controls.Add(this.scalingButtonMenuWithCharm);
            this.flowLayoutPanelLeftColumn.Controls.Add(this.scalingButtonMenuWithRelic);
            this.flowLayoutPanelLeftColumn.Controls.Add(this.scalingButtonMenuAttribute);
            this.flowLayoutPanelLeftColumn.Controls.Add(this.scalingButtonMenuPrefixName);
            this.flowLayoutPanelLeftColumn.Controls.Add(this.scalingButtonMenuPrefixAttribute);
            this.flowLayoutPanelLeftColumn.Controls.Add(this.scalingButtonMenuBaseAttribute);
            this.flowLayoutPanelLeftColumn.Controls.Add(this.scalingButtonMenuSuffixName);
            this.flowLayoutPanelLeftColumn.Controls.Add(this.scalingButtonMenuSuffixAttribute);
            this.flowLayoutPanelLeftColumn.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelLeftColumn.Location = new System.Drawing.Point(3, 147);
            this.flowLayoutPanelLeftColumn.MinimumSize = new System.Drawing.Size(128, 578);
            this.flowLayoutPanelLeftColumn.Name = "flowLayoutPanelLeftColumn";
            this.flowLayoutPanelLeftColumn.Size = new System.Drawing.Size(128, 578);
            this.flowLayoutPanelLeftColumn.TabIndex = 19;
            // 
            // scalingButtonMenuCharacters
            // 
            this.scalingButtonMenuCharacters.BackColor = System.Drawing.Color.Transparent;
            this.scalingButtonMenuCharacters.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuCharacters.DownBitmap")));
            this.scalingButtonMenuCharacters.FlatAppearance.BorderSize = 0;
            this.scalingButtonMenuCharacters.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuCharacters.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuCharacters.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.scalingButtonMenuCharacters.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F);
            this.scalingButtonMenuCharacters.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuCharacters.Image = ((System.Drawing.Image)(resources.GetObject("scalingButtonMenuCharacters.Image")));
            this.scalingButtonMenuCharacters.Location = new System.Drawing.Point(3, 3);
            this.scalingButtonMenuCharacters.Name = "scalingButtonMenuCharacters";
            this.scalingButtonMenuCharacters.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuCharacters.OverBitmap")));
            this.scalingButtonMenuCharacters.Size = new System.Drawing.Size(120, 30);
            this.scalingButtonMenuCharacters.SizeToGraphic = false;
            this.scalingButtonMenuCharacters.TabIndex = 8;
            this.scalingButtonMenuCharacters.Text = "Characters";
            this.scalingButtonMenuCharacters.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuCharacters.UpBitmap")));
            this.scalingButtonMenuCharacters.UseCustomGraphic = true;
            this.scalingButtonMenuCharacters.UseVisualStyleBackColor = false;
            this.scalingButtonMenuCharacters.Click += new System.EventHandler(this.scalingButtonMenu_Click);
            // 
            // scalingButtonMenuVaults
            // 
            this.scalingButtonMenuVaults.BackColor = System.Drawing.Color.Transparent;
            this.scalingButtonMenuVaults.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuVaults.DownBitmap")));
            this.scalingButtonMenuVaults.FlatAppearance.BorderSize = 0;
            this.scalingButtonMenuVaults.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuVaults.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuVaults.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.scalingButtonMenuVaults.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F);
            this.scalingButtonMenuVaults.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuVaults.Image = ((System.Drawing.Image)(resources.GetObject("scalingButtonMenuVaults.Image")));
            this.scalingButtonMenuVaults.Location = new System.Drawing.Point(3, 39);
            this.scalingButtonMenuVaults.Name = "scalingButtonMenuVaults";
            this.scalingButtonMenuVaults.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuVaults.OverBitmap")));
            this.scalingButtonMenuVaults.Size = new System.Drawing.Size(120, 30);
            this.scalingButtonMenuVaults.SizeToGraphic = false;
            this.scalingButtonMenuVaults.TabIndex = 9;
            this.scalingButtonMenuVaults.Text = "Vaults";
            this.scalingButtonMenuVaults.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuVaults.UpBitmap")));
            this.scalingButtonMenuVaults.UseCustomGraphic = true;
            this.scalingButtonMenuVaults.UseVisualStyleBackColor = false;
            this.scalingButtonMenuVaults.Click += new System.EventHandler(this.scalingButtonMenu_Click);
            // 
            // scalingButtonMenuType
            // 
            this.scalingButtonMenuType.BackColor = System.Drawing.Color.Transparent;
            this.scalingButtonMenuType.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuType.DownBitmap")));
            this.scalingButtonMenuType.FlatAppearance.BorderSize = 0;
            this.scalingButtonMenuType.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuType.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.scalingButtonMenuType.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F);
            this.scalingButtonMenuType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuType.Image = ((System.Drawing.Image)(resources.GetObject("scalingButtonMenuType.Image")));
            this.scalingButtonMenuType.Location = new System.Drawing.Point(3, 75);
            this.scalingButtonMenuType.Name = "scalingButtonMenuType";
            this.scalingButtonMenuType.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuType.OverBitmap")));
            this.scalingButtonMenuType.Size = new System.Drawing.Size(120, 30);
            this.scalingButtonMenuType.SizeToGraphic = false;
            this.scalingButtonMenuType.TabIndex = 10;
            this.scalingButtonMenuType.Text = "Type";
            this.scalingButtonMenuType.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuType.UpBitmap")));
            this.scalingButtonMenuType.UseCustomGraphic = true;
            this.scalingButtonMenuType.UseVisualStyleBackColor = false;
            this.scalingButtonMenuType.Click += new System.EventHandler(this.scalingButtonMenu_Click);
            // 
            // scalingButtonMenuRarity
            // 
            this.scalingButtonMenuRarity.BackColor = System.Drawing.Color.Transparent;
            this.scalingButtonMenuRarity.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuRarity.DownBitmap")));
            this.scalingButtonMenuRarity.FlatAppearance.BorderSize = 0;
            this.scalingButtonMenuRarity.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuRarity.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuRarity.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.scalingButtonMenuRarity.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F);
            this.scalingButtonMenuRarity.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuRarity.Image = ((System.Drawing.Image)(resources.GetObject("scalingButtonMenuRarity.Image")));
            this.scalingButtonMenuRarity.Location = new System.Drawing.Point(3, 111);
            this.scalingButtonMenuRarity.Name = "scalingButtonMenuRarity";
            this.scalingButtonMenuRarity.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuRarity.OverBitmap")));
            this.scalingButtonMenuRarity.Size = new System.Drawing.Size(120, 30);
            this.scalingButtonMenuRarity.SizeToGraphic = false;
            this.scalingButtonMenuRarity.TabIndex = 11;
            this.scalingButtonMenuRarity.Text = "Rarity";
            this.scalingButtonMenuRarity.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuRarity.UpBitmap")));
            this.scalingButtonMenuRarity.UseCustomGraphic = true;
            this.scalingButtonMenuRarity.UseVisualStyleBackColor = false;
            this.scalingButtonMenuRarity.Click += new System.EventHandler(this.scalingButtonMenu_Click);
            // 
            // scalingButtonMenuOrigin
            // 
            this.scalingButtonMenuOrigin.BackColor = System.Drawing.Color.Transparent;
            this.scalingButtonMenuOrigin.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuOrigin.DownBitmap")));
            this.scalingButtonMenuOrigin.FlatAppearance.BorderSize = 0;
            this.scalingButtonMenuOrigin.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuOrigin.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuOrigin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.scalingButtonMenuOrigin.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F);
            this.scalingButtonMenuOrigin.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuOrigin.Image = ((System.Drawing.Image)(resources.GetObject("scalingButtonMenuOrigin.Image")));
            this.scalingButtonMenuOrigin.Location = new System.Drawing.Point(3, 147);
            this.scalingButtonMenuOrigin.Name = "scalingButtonMenuOrigin";
            this.scalingButtonMenuOrigin.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuOrigin.OverBitmap")));
            this.scalingButtonMenuOrigin.Size = new System.Drawing.Size(120, 30);
            this.scalingButtonMenuOrigin.SizeToGraphic = false;
            this.scalingButtonMenuOrigin.TabIndex = 22;
            this.scalingButtonMenuOrigin.Text = "Origin";
            this.scalingButtonMenuOrigin.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuOrigin.UpBitmap")));
            this.scalingButtonMenuOrigin.UseCustomGraphic = true;
            this.scalingButtonMenuOrigin.UseVisualStyleBackColor = false;
            this.scalingButtonMenuOrigin.Click += new System.EventHandler(this.scalingButtonMenu_Click);
            // 
            // scalingButtonMenuQuality
            // 
            this.scalingButtonMenuQuality.BackColor = System.Drawing.Color.Transparent;
            this.scalingButtonMenuQuality.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuQuality.DownBitmap")));
            this.scalingButtonMenuQuality.FlatAppearance.BorderSize = 0;
            this.scalingButtonMenuQuality.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuQuality.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuQuality.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.scalingButtonMenuQuality.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F);
            this.scalingButtonMenuQuality.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuQuality.Image = ((System.Drawing.Image)(resources.GetObject("scalingButtonMenuQuality.Image")));
            this.scalingButtonMenuQuality.Location = new System.Drawing.Point(3, 183);
            this.scalingButtonMenuQuality.Name = "scalingButtonMenuQuality";
            this.scalingButtonMenuQuality.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuQuality.OverBitmap")));
            this.scalingButtonMenuQuality.Size = new System.Drawing.Size(120, 30);
            this.scalingButtonMenuQuality.SizeToGraphic = false;
            this.scalingButtonMenuQuality.TabIndex = 21;
            this.scalingButtonMenuQuality.Text = "Quality";
            this.scalingButtonMenuQuality.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuQuality.UpBitmap")));
            this.scalingButtonMenuQuality.UseCustomGraphic = true;
            this.scalingButtonMenuQuality.UseVisualStyleBackColor = false;
            this.scalingButtonMenuQuality.Click += new System.EventHandler(this.scalingButtonMenu_Click);
            // 
            // scalingButtonMenuStyle
            // 
            this.scalingButtonMenuStyle.BackColor = System.Drawing.Color.Transparent;
            this.scalingButtonMenuStyle.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuStyle.DownBitmap")));
            this.scalingButtonMenuStyle.FlatAppearance.BorderSize = 0;
            this.scalingButtonMenuStyle.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuStyle.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuStyle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.scalingButtonMenuStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F);
            this.scalingButtonMenuStyle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuStyle.Image = ((System.Drawing.Image)(resources.GetObject("scalingButtonMenuStyle.Image")));
            this.scalingButtonMenuStyle.Location = new System.Drawing.Point(3, 219);
            this.scalingButtonMenuStyle.Name = "scalingButtonMenuStyle";
            this.scalingButtonMenuStyle.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuStyle.OverBitmap")));
            this.scalingButtonMenuStyle.Size = new System.Drawing.Size(120, 30);
            this.scalingButtonMenuStyle.SizeToGraphic = false;
            this.scalingButtonMenuStyle.TabIndex = 12;
            this.scalingButtonMenuStyle.Text = "Style";
            this.scalingButtonMenuStyle.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuStyle.UpBitmap")));
            this.scalingButtonMenuStyle.UseCustomGraphic = true;
            this.scalingButtonMenuStyle.UseVisualStyleBackColor = false;
            this.scalingButtonMenuStyle.Click += new System.EventHandler(this.scalingButtonMenu_Click);
            // 
            // scalingButtonMenuSetItems
            // 
            this.scalingButtonMenuSetItems.BackColor = System.Drawing.Color.Transparent;
            this.scalingButtonMenuSetItems.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuSetItems.DownBitmap")));
            this.scalingButtonMenuSetItems.FlatAppearance.BorderSize = 0;
            this.scalingButtonMenuSetItems.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuSetItems.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuSetItems.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.scalingButtonMenuSetItems.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F);
            this.scalingButtonMenuSetItems.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuSetItems.Image = ((System.Drawing.Image)(resources.GetObject("scalingButtonMenuSetItems.Image")));
            this.scalingButtonMenuSetItems.Location = new System.Drawing.Point(3, 255);
            this.scalingButtonMenuSetItems.Name = "scalingButtonMenuSetItems";
            this.scalingButtonMenuSetItems.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuSetItems.OverBitmap")));
            this.scalingButtonMenuSetItems.Size = new System.Drawing.Size(120, 30);
            this.scalingButtonMenuSetItems.SizeToGraphic = false;
            this.scalingButtonMenuSetItems.TabIndex = 23;
            this.scalingButtonMenuSetItems.Text = "Sets";
            this.scalingButtonMenuSetItems.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuSetItems.UpBitmap")));
            this.scalingButtonMenuSetItems.UseCustomGraphic = true;
            this.scalingButtonMenuSetItems.UseVisualStyleBackColor = false;
            this.scalingButtonMenuSetItems.Click += new System.EventHandler(this.scalingButtonMenu_Click);
            // 
            // scalingButtonMenuWithCharm
            // 
            this.scalingButtonMenuWithCharm.BackColor = System.Drawing.Color.Transparent;
            this.scalingButtonMenuWithCharm.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuWithCharm.DownBitmap")));
            this.scalingButtonMenuWithCharm.FlatAppearance.BorderSize = 0;
            this.scalingButtonMenuWithCharm.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuWithCharm.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuWithCharm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.scalingButtonMenuWithCharm.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F);
            this.scalingButtonMenuWithCharm.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuWithCharm.Image = ((System.Drawing.Image)(resources.GetObject("scalingButtonMenuWithCharm.Image")));
            this.scalingButtonMenuWithCharm.Location = new System.Drawing.Point(3, 291);
            this.scalingButtonMenuWithCharm.Name = "scalingButtonMenuWithCharm";
            this.scalingButtonMenuWithCharm.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuWithCharm.OverBitmap")));
            this.scalingButtonMenuWithCharm.Size = new System.Drawing.Size(120, 30);
            this.scalingButtonMenuWithCharm.SizeToGraphic = false;
            this.scalingButtonMenuWithCharm.TabIndex = 13;
            this.scalingButtonMenuWithCharm.Text = "Having Charm";
            this.scalingButtonMenuWithCharm.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuWithCharm.UpBitmap")));
            this.scalingButtonMenuWithCharm.UseCustomGraphic = true;
            this.scalingButtonMenuWithCharm.UseVisualStyleBackColor = false;
            this.scalingButtonMenuWithCharm.Click += new System.EventHandler(this.scalingButtonMenu_Click);
            // 
            // scalingButtonMenuWithRelic
            // 
            this.scalingButtonMenuWithRelic.BackColor = System.Drawing.Color.Transparent;
            this.scalingButtonMenuWithRelic.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuWithRelic.DownBitmap")));
            this.scalingButtonMenuWithRelic.FlatAppearance.BorderSize = 0;
            this.scalingButtonMenuWithRelic.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuWithRelic.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuWithRelic.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.scalingButtonMenuWithRelic.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F);
            this.scalingButtonMenuWithRelic.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuWithRelic.Image = ((System.Drawing.Image)(resources.GetObject("scalingButtonMenuWithRelic.Image")));
            this.scalingButtonMenuWithRelic.Location = new System.Drawing.Point(3, 327);
            this.scalingButtonMenuWithRelic.Name = "scalingButtonMenuWithRelic";
            this.scalingButtonMenuWithRelic.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuWithRelic.OverBitmap")));
            this.scalingButtonMenuWithRelic.Size = new System.Drawing.Size(120, 30);
            this.scalingButtonMenuWithRelic.SizeToGraphic = false;
            this.scalingButtonMenuWithRelic.TabIndex = 14;
            this.scalingButtonMenuWithRelic.Text = "Having Relic";
            this.scalingButtonMenuWithRelic.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuWithRelic.UpBitmap")));
            this.scalingButtonMenuWithRelic.UseCustomGraphic = true;
            this.scalingButtonMenuWithRelic.UseVisualStyleBackColor = false;
            this.scalingButtonMenuWithRelic.Click += new System.EventHandler(this.scalingButtonMenu_Click);
            // 
            // scalingButtonMenuAttribute
            // 
            this.scalingButtonMenuAttribute.BackColor = System.Drawing.Color.Transparent;
            this.scalingButtonMenuAttribute.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuAttribute.DownBitmap")));
            this.scalingButtonMenuAttribute.FlatAppearance.BorderSize = 0;
            this.scalingButtonMenuAttribute.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuAttribute.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuAttribute.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.scalingButtonMenuAttribute.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F);
            this.scalingButtonMenuAttribute.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuAttribute.Image = ((System.Drawing.Image)(resources.GetObject("scalingButtonMenuAttribute.Image")));
            this.scalingButtonMenuAttribute.Location = new System.Drawing.Point(3, 363);
            this.scalingButtonMenuAttribute.Name = "scalingButtonMenuAttribute";
            this.scalingButtonMenuAttribute.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuAttribute.OverBitmap")));
            this.scalingButtonMenuAttribute.Size = new System.Drawing.Size(120, 30);
            this.scalingButtonMenuAttribute.SizeToGraphic = false;
            this.scalingButtonMenuAttribute.TabIndex = 15;
            this.scalingButtonMenuAttribute.Text = "All Attributes";
            this.scalingButtonMenuAttribute.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuAttribute.UpBitmap")));
            this.scalingButtonMenuAttribute.UseCustomGraphic = true;
            this.scalingButtonMenuAttribute.UseVisualStyleBackColor = false;
            this.scalingButtonMenuAttribute.Click += new System.EventHandler(this.scalingButtonMenu_Click);
            // 
            // scalingButtonMenuPrefixName
            // 
            this.scalingButtonMenuPrefixName.BackColor = System.Drawing.Color.Transparent;
            this.scalingButtonMenuPrefixName.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuPrefixName.DownBitmap")));
            this.scalingButtonMenuPrefixName.FlatAppearance.BorderSize = 0;
            this.scalingButtonMenuPrefixName.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuPrefixName.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuPrefixName.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.scalingButtonMenuPrefixName.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F);
            this.scalingButtonMenuPrefixName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuPrefixName.Image = ((System.Drawing.Image)(resources.GetObject("scalingButtonMenuPrefixName.Image")));
            this.scalingButtonMenuPrefixName.Location = new System.Drawing.Point(3, 399);
            this.scalingButtonMenuPrefixName.Name = "scalingButtonMenuPrefixName";
            this.scalingButtonMenuPrefixName.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuPrefixName.OverBitmap")));
            this.scalingButtonMenuPrefixName.Size = new System.Drawing.Size(120, 30);
            this.scalingButtonMenuPrefixName.SizeToGraphic = false;
            this.scalingButtonMenuPrefixName.TabIndex = 16;
            this.scalingButtonMenuPrefixName.Text = "Prefix Name";
            this.scalingButtonMenuPrefixName.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuPrefixName.UpBitmap")));
            this.scalingButtonMenuPrefixName.UseCustomGraphic = true;
            this.scalingButtonMenuPrefixName.UseVisualStyleBackColor = false;
            this.scalingButtonMenuPrefixName.Click += new System.EventHandler(this.scalingButtonMenu_Click);
            // 
            // scalingButtonMenuPrefixAttribute
            // 
            this.scalingButtonMenuPrefixAttribute.BackColor = System.Drawing.Color.Transparent;
            this.scalingButtonMenuPrefixAttribute.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuPrefixAttribute.DownBitmap")));
            this.scalingButtonMenuPrefixAttribute.FlatAppearance.BorderSize = 0;
            this.scalingButtonMenuPrefixAttribute.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuPrefixAttribute.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuPrefixAttribute.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.scalingButtonMenuPrefixAttribute.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F);
            this.scalingButtonMenuPrefixAttribute.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuPrefixAttribute.Image = ((System.Drawing.Image)(resources.GetObject("scalingButtonMenuPrefixAttribute.Image")));
            this.scalingButtonMenuPrefixAttribute.Location = new System.Drawing.Point(3, 435);
            this.scalingButtonMenuPrefixAttribute.Name = "scalingButtonMenuPrefixAttribute";
            this.scalingButtonMenuPrefixAttribute.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuPrefixAttribute.OverBitmap")));
            this.scalingButtonMenuPrefixAttribute.Size = new System.Drawing.Size(120, 30);
            this.scalingButtonMenuPrefixAttribute.SizeToGraphic = false;
            this.scalingButtonMenuPrefixAttribute.TabIndex = 19;
            this.scalingButtonMenuPrefixAttribute.Text = "Prefix Attribute";
            this.scalingButtonMenuPrefixAttribute.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuPrefixAttribute.UpBitmap")));
            this.scalingButtonMenuPrefixAttribute.UseCustomGraphic = true;
            this.scalingButtonMenuPrefixAttribute.UseVisualStyleBackColor = false;
            this.scalingButtonMenuPrefixAttribute.Click += new System.EventHandler(this.scalingButtonMenu_Click);
            // 
            // scalingButtonMenuBaseAttribute
            // 
            this.scalingButtonMenuBaseAttribute.BackColor = System.Drawing.Color.Transparent;
            this.scalingButtonMenuBaseAttribute.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuBaseAttribute.DownBitmap")));
            this.scalingButtonMenuBaseAttribute.FlatAppearance.BorderSize = 0;
            this.scalingButtonMenuBaseAttribute.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuBaseAttribute.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuBaseAttribute.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.scalingButtonMenuBaseAttribute.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F);
            this.scalingButtonMenuBaseAttribute.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuBaseAttribute.Image = ((System.Drawing.Image)(resources.GetObject("scalingButtonMenuBaseAttribute.Image")));
            this.scalingButtonMenuBaseAttribute.Location = new System.Drawing.Point(3, 471);
            this.scalingButtonMenuBaseAttribute.Name = "scalingButtonMenuBaseAttribute";
            this.scalingButtonMenuBaseAttribute.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuBaseAttribute.OverBitmap")));
            this.scalingButtonMenuBaseAttribute.Size = new System.Drawing.Size(120, 30);
            this.scalingButtonMenuBaseAttribute.SizeToGraphic = false;
            this.scalingButtonMenuBaseAttribute.TabIndex = 17;
            this.scalingButtonMenuBaseAttribute.Text = "Base Attribute";
            this.scalingButtonMenuBaseAttribute.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuBaseAttribute.UpBitmap")));
            this.scalingButtonMenuBaseAttribute.UseCustomGraphic = true;
            this.scalingButtonMenuBaseAttribute.UseVisualStyleBackColor = false;
            this.scalingButtonMenuBaseAttribute.Click += new System.EventHandler(this.scalingButtonMenu_Click);
            // 
            // scalingButtonMenuSuffixName
            // 
            this.scalingButtonMenuSuffixName.BackColor = System.Drawing.Color.Transparent;
            this.scalingButtonMenuSuffixName.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuSuffixName.DownBitmap")));
            this.scalingButtonMenuSuffixName.FlatAppearance.BorderSize = 0;
            this.scalingButtonMenuSuffixName.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuSuffixName.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuSuffixName.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.scalingButtonMenuSuffixName.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F);
            this.scalingButtonMenuSuffixName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuSuffixName.Image = ((System.Drawing.Image)(resources.GetObject("scalingButtonMenuSuffixName.Image")));
            this.scalingButtonMenuSuffixName.Location = new System.Drawing.Point(3, 507);
            this.scalingButtonMenuSuffixName.Name = "scalingButtonMenuSuffixName";
            this.scalingButtonMenuSuffixName.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuSuffixName.OverBitmap")));
            this.scalingButtonMenuSuffixName.Size = new System.Drawing.Size(120, 30);
            this.scalingButtonMenuSuffixName.SizeToGraphic = false;
            this.scalingButtonMenuSuffixName.TabIndex = 18;
            this.scalingButtonMenuSuffixName.Text = "Suffix Name";
            this.scalingButtonMenuSuffixName.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuSuffixName.UpBitmap")));
            this.scalingButtonMenuSuffixName.UseCustomGraphic = true;
            this.scalingButtonMenuSuffixName.UseVisualStyleBackColor = false;
            this.scalingButtonMenuSuffixName.Click += new System.EventHandler(this.scalingButtonMenu_Click);
            // 
            // scalingButtonMenuSuffixAttribute
            // 
            this.scalingButtonMenuSuffixAttribute.BackColor = System.Drawing.Color.Transparent;
            this.scalingButtonMenuSuffixAttribute.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuSuffixAttribute.DownBitmap")));
            this.scalingButtonMenuSuffixAttribute.FlatAppearance.BorderSize = 0;
            this.scalingButtonMenuSuffixAttribute.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuSuffixAttribute.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuSuffixAttribute.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.scalingButtonMenuSuffixAttribute.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.5F);
            this.scalingButtonMenuSuffixAttribute.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.scalingButtonMenuSuffixAttribute.Image = ((System.Drawing.Image)(resources.GetObject("scalingButtonMenuSuffixAttribute.Image")));
            this.scalingButtonMenuSuffixAttribute.Location = new System.Drawing.Point(3, 543);
            this.scalingButtonMenuSuffixAttribute.Name = "scalingButtonMenuSuffixAttribute";
            this.scalingButtonMenuSuffixAttribute.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuSuffixAttribute.OverBitmap")));
            this.scalingButtonMenuSuffixAttribute.Size = new System.Drawing.Size(120, 30);
            this.scalingButtonMenuSuffixAttribute.SizeToGraphic = false;
            this.scalingButtonMenuSuffixAttribute.TabIndex = 20;
            this.scalingButtonMenuSuffixAttribute.Text = "Suffix Attribute";
            this.scalingButtonMenuSuffixAttribute.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("scalingButtonMenuSuffixAttribute.UpBitmap")));
            this.scalingButtonMenuSuffixAttribute.UseCustomGraphic = true;
            this.scalingButtonMenuSuffixAttribute.UseVisualStyleBackColor = false;
            this.scalingButtonMenuSuffixAttribute.Click += new System.EventHandler(this.scalingButtonMenu_Click);
            // 
            // backgroundWorkerBuildDB
            // 
            this.backgroundWorkerBuildDB.WorkerReportsProgress = true;
            this.backgroundWorkerBuildDB.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerBuildDB_DoWork);
            this.backgroundWorkerBuildDB.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorkerBuildDB_ProgressChanged);
            this.backgroundWorkerBuildDB.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerBuildDB_RunWorkerCompleted);
            // 
            // typeAssistantSearchBox
            // 
            this.typeAssistantSearchBox.Idled += new System.EventHandler(this.scalingTextBoxSearchTerm_TextChanged_Idled);
            // 
            // typeAssistantFilterCategories
            // 
            this.typeAssistantFilterCategories.Idled += new System.EventHandler(this.typeAssistantFilterCategories_Idled);
            // 
            // SearchDialogAdvanced
            // 
            this.AcceptButton = this.applyButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(1415, 870);
            this.Controls.Add(this.tableLayoutPanelContent);
            this.DrawCustomBorder = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SearchDialogAdvanced";
            this.Padding = new System.Windows.Forms.Padding(15, 25, 15, 10);
            this.ResizeCustomAllowed = true;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Search for an Item";
            this.TitleTextColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.TopMost = true;
            this.Load += new System.EventHandler(this.SearchDialogAdvanced_Load);
            this.Shown += new System.EventHandler(this.SearchDialogShown);
            this.Controls.SetChildIndex(this.tableLayoutPanelContent, 0);
            this.tableLayoutPanelBottom.ResumeLayout(false);
            this.scalingLabelProgressPanelAlignText.ResumeLayout(false);
            this.flowLayoutPanelCharacters.ResumeLayout(false);
            this.flowLayoutPanelCharacters.PerformLayout();
            this.flowLayoutPanelItemType.ResumeLayout(false);
            this.flowLayoutPanelItemType.PerformLayout();
            this.flowLayoutPanelItemAttributes.ResumeLayout(false);
            this.flowLayoutPanelItemAttributes.PerformLayout();
            this.flowLayoutPanelRarity.ResumeLayout(false);
            this.flowLayoutPanelRarity.PerformLayout();
            this.flowLayoutPanelPrefixAttributes.ResumeLayout(false);
            this.flowLayoutPanelPrefixAttributes.PerformLayout();
            this.flowLayoutPanelSuffixAttributes.ResumeLayout(false);
            this.flowLayoutPanelSuffixAttributes.PerformLayout();
            this.flowLayoutPanelBaseAttributes.ResumeLayout(false);
            this.flowLayoutPanelBaseAttributes.PerformLayout();
            this.flowLayoutPanelMain.ResumeLayout(false);
            this.flowLayoutPanelMain.PerformLayout();
            this.flowLayoutPanelInVaults.ResumeLayout(false);
            this.flowLayoutPanelInVaults.PerformLayout();
            this.flowLayoutPanelOrigin.ResumeLayout(false);
            this.flowLayoutPanelOrigin.PerformLayout();
            this.flowLayoutPanelQuality.ResumeLayout(false);
            this.flowLayoutPanelQuality.PerformLayout();
            this.flowLayoutPanelStyle.ResumeLayout(false);
            this.flowLayoutPanelStyle.PerformLayout();
            this.flowLayoutPanelSetItems.ResumeLayout(false);
            this.flowLayoutPanelSetItems.PerformLayout();
            this.flowLayoutPanelWithCharm.ResumeLayout(false);
            this.flowLayoutPanelWithCharm.PerformLayout();
            this.flowLayoutPanelWithRelic.ResumeLayout(false);
            this.flowLayoutPanelWithRelic.PerformLayout();
            this.flowLayoutPanelPrefixName.ResumeLayout(false);
            this.flowLayoutPanelPrefixName.PerformLayout();
            this.flowLayoutPanelSuffixName.ResumeLayout(false);
            this.flowLayoutPanelSuffixName.PerformLayout();
            this.flowLayoutPanelSearchTerm.ResumeLayout(false);
            this.flowLayoutPanelSearchTerm.PerformLayout();
            this.flowLayoutPanelQueries.ResumeLayout(false);
            this.flowLayoutPanelQueries.PerformLayout();
            this.tableLayoutPanelContent.ResumeLayout(false);
            this.tableLayoutPanelContent.PerformLayout();
            this.flowLayoutPanelMainAutoScroll.ResumeLayout(false);
            this.flowLayoutPanelMainAutoScroll.PerformLayout();
            this.bufferedFlowLayoutPanelQuickFilters.ResumeLayout(false);
            this.bufferedFlowLayoutPanelQuickFilters.PerformLayout();
            this.flowLayoutPanelMisc.ResumeLayout(false);
            this.flowLayoutPanelMisc.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxElement)).EndInit();
            this.bufferedFlowLayoutPanelRequierements.ResumeLayout(false);
            this.bufferedFlowLayoutPanelRequierements.PerformLayout();
            this.tableLayoutPanelRequierements.ResumeLayout(false);
            this.tableLayoutPanelRequierements.PerformLayout();
            this.flowLayoutPanelMin.ResumeLayout(false);
            this.flowLayoutPanelMin.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinLvl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinStr)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinDex)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinInt)).EndInit();
            this.flowLayoutPanelMax.ResumeLayout(false);
            this.flowLayoutPanelMax.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxLvl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxStr)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxDex)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxInt)).EndInit();
            this.flowLayoutPanelLeftColumn.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

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