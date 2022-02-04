namespace TQ.SaveFilesExplorer.Components
{
	partial class TabPageFileContent
	{
		/// <summary> 
		/// Variable nécessaire au concepteur.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Nettoyage des ressources utilisées.
		/// </summary>
		/// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Code généré par le Concepteur de composants

		/// <summary> 
		/// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
		/// le contenu de cette méthode avec l'éditeur de code.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBoxDetectedKeys = new System.Windows.Forms.GroupBox();
            this.panelTreeToolBox = new System.Windows.Forms.Panel();
            this.buttonCopyTree = new System.Windows.Forms.Button();
            this.comboBoxCopyType = new System.Windows.Forms.ComboBox();
            this.treeViewKeys = new System.Windows.Forms.TreeView();
            this.groupBoxKeyData = new System.Windows.Forms.GroupBox();
            this.labelDataAsInt = new System.Windows.Forms.Label();
            this.textBoxDataAsInt = new System.Windows.Forms.TextBox();
            this.labelDataAsByteArray = new System.Windows.Forms.Label();
            this.textBoxDataAsByteArray = new System.Windows.Forms.TextBox();
            this.labelDataAsString = new System.Windows.Forms.Label();
            this.textBoxDataAsString = new System.Windows.Forms.TextBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.labelDataOffset = new System.Windows.Forms.Label();
            this.labelDataLength = new System.Windows.Forms.Label();
            this.labelDataType = new System.Windows.Forms.Label();
            this.groupBoxKeyInfos = new System.Windows.Forms.GroupBox();
            this.labelKeyName = new System.Windows.Forms.Label();
            this.textBoxKeyName = new System.Windows.Forms.TextBox();
            this.flowLayoutPanelKeyInfos = new System.Windows.Forms.FlowLayoutPanel();
            this.labelKeyOffset = new System.Windows.Forms.Label();
            this.labelKeyLength = new System.Windows.Forms.Label();
            this.labelkeyIsSubStructureOpening = new System.Windows.Forms.Label();
            this.labelKeyIsStructureClosing = new System.Windows.Forms.Label();
            this.labelKeyIsUnknownSegment = new System.Windows.Forms.Label();
            this.labelKeyIsDataTypeError = new System.Windows.Forms.Label();
            this.labelIsKeyValue = new System.Windows.Forms.Label();
            this.groupBoxFileInfos = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanelFileInfos = new System.Windows.Forms.FlowLayoutPanel();
            this.labelFileSize = new System.Windows.Forms.Label();
            this.labelFileName = new System.Windows.Forms.Label();
            this.labelFileExtension = new System.Windows.Forms.Label();
            this.labelFileVersion = new System.Windows.Forms.Label();
            this.labelFileFoundKeys = new System.Windows.Forms.Label();
            this.labelFileDataTypeErrors = new System.Windows.Forms.Label();
            this.labelFileUnknownSegments = new System.Windows.Forms.Label();
            this.linkLabelFilePath = new System.Windows.Forms.LinkLabel();
            this.toolTipCopy = new System.Windows.Forms.ToolTip(this.components);
            this.labelDataAsFloat = new System.Windows.Forms.Label();
            this.textBoxDataAsFloat = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBoxDetectedKeys.SuspendLayout();
            this.panelTreeToolBox.SuspendLayout();
            this.groupBoxKeyData.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.groupBoxKeyInfos.SuspendLayout();
            this.flowLayoutPanelKeyInfos.SuspendLayout();
            this.groupBoxFileInfos.SuspendLayout();
            this.flowLayoutPanelFileInfos.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 82);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBoxDetectedKeys);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBoxKeyData);
            this.splitContainer1.Panel2.Controls.Add(this.groupBoxKeyInfos);
            this.splitContainer1.Size = new System.Drawing.Size(922, 543);
            this.splitContainer1.SplitterDistance = 306;
            this.splitContainer1.SplitterWidth = 11;
            this.splitContainer1.TabIndex = 12;
            // 
            // groupBoxDetectedKeys
            // 
            this.groupBoxDetectedKeys.Controls.Add(this.panelTreeToolBox);
            this.groupBoxDetectedKeys.Controls.Add(this.treeViewKeys);
            this.groupBoxDetectedKeys.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxDetectedKeys.Location = new System.Drawing.Point(0, 0);
            this.groupBoxDetectedKeys.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBoxDetectedKeys.Name = "groupBoxDetectedKeys";
            this.groupBoxDetectedKeys.Padding = new System.Windows.Forms.Padding(11, 10, 11, 10);
            this.groupBoxDetectedKeys.Size = new System.Drawing.Size(306, 543);
            this.groupBoxDetectedKeys.TabIndex = 9;
            this.groupBoxDetectedKeys.TabStop = false;
            this.groupBoxDetectedKeys.Text = "Detected Keys";
            // 
            // panelTreeToolBox
            // 
            this.panelTreeToolBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelTreeToolBox.AutoSize = true;
            this.panelTreeToolBox.Controls.Add(this.buttonCopyTree);
            this.panelTreeToolBox.Controls.Add(this.comboBoxCopyType);
            this.panelTreeToolBox.Location = new System.Drawing.Point(11, 17);
            this.panelTreeToolBox.Margin = new System.Windows.Forms.Padding(0);
            this.panelTreeToolBox.Name = "panelTreeToolBox";
            this.panelTreeToolBox.Size = new System.Drawing.Size(284, 31);
            this.panelTreeToolBox.TabIndex = 5;
            // 
            // buttonCopyTree
            // 
            this.buttonCopyTree.Location = new System.Drawing.Point(0, 2);
            this.buttonCopyTree.Margin = new System.Windows.Forms.Padding(0);
            this.buttonCopyTree.Name = "buttonCopyTree";
            this.buttonCopyTree.Size = new System.Drawing.Size(62, 26);
            this.buttonCopyTree.TabIndex = 3;
            this.buttonCopyTree.Text = "Copy";
            this.toolTipCopy.SetToolTip(this.buttonCopyTree, "Copy to Clipboard");
            this.buttonCopyTree.UseVisualStyleBackColor = true;
            this.buttonCopyTree.Click += new System.EventHandler(this.ButtonCopyTree_Click);
            // 
            // comboBoxCopyType
            // 
            this.comboBoxCopyType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxCopyType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCopyType.FormattingEnabled = true;
            this.comboBoxCopyType.Items.AddRange(new object[] {
            "Keys Only",
            "Keys + Type",
            "Keys + Type + Data"});
            this.comboBoxCopyType.Location = new System.Drawing.Point(64, 3);
            this.comboBoxCopyType.Margin = new System.Windows.Forms.Padding(0);
            this.comboBoxCopyType.Name = "comboBoxCopyType";
            this.comboBoxCopyType.Size = new System.Drawing.Size(220, 24);
            this.comboBoxCopyType.TabIndex = 4;
            // 
            // treeViewKeys
            // 
            this.treeViewKeys.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewKeys.Location = new System.Drawing.Point(11, 50);
            this.treeViewKeys.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.treeViewKeys.Name = "treeViewKeys";
            this.treeViewKeys.Size = new System.Drawing.Size(284, 483);
            this.treeViewKeys.TabIndex = 2;
            this.treeViewKeys.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewKeys_AfterSelect);
            this.treeViewKeys.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.TreeViewKeys_NodeMouseClick);
            // 
            // groupBoxKeyData
            // 
            this.groupBoxKeyData.Controls.Add(this.labelDataAsFloat);
            this.groupBoxKeyData.Controls.Add(this.textBoxDataAsFloat);
            this.groupBoxKeyData.Controls.Add(this.labelDataAsInt);
            this.groupBoxKeyData.Controls.Add(this.textBoxDataAsInt);
            this.groupBoxKeyData.Controls.Add(this.labelDataAsByteArray);
            this.groupBoxKeyData.Controls.Add(this.textBoxDataAsByteArray);
            this.groupBoxKeyData.Controls.Add(this.labelDataAsString);
            this.groupBoxKeyData.Controls.Add(this.textBoxDataAsString);
            this.groupBoxKeyData.Controls.Add(this.flowLayoutPanel1);
            this.groupBoxKeyData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxKeyData.Location = new System.Drawing.Point(0, 102);
            this.groupBoxKeyData.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBoxKeyData.Name = "groupBoxKeyData";
            this.groupBoxKeyData.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBoxKeyData.Size = new System.Drawing.Size(605, 441);
            this.groupBoxKeyData.TabIndex = 6;
            this.groupBoxKeyData.TabStop = false;
            this.groupBoxKeyData.Text = "Key Data";
            // 
            // labelDataAsInt
            // 
            this.labelDataAsInt.AutoSize = true;
            this.labelDataAsInt.Location = new System.Drawing.Point(16, 79);
            this.labelDataAsInt.Margin = new System.Windows.Forms.Padding(10, 5, 3, 0);
            this.labelDataAsInt.Name = "labelDataAsInt";
            this.labelDataAsInt.Size = new System.Drawing.Size(67, 17);
            this.labelDataAsInt.TabIndex = 13;
            this.labelDataAsInt.Text = "As a Int : ";
            this.labelDataAsInt.Visible = false;
            this.labelDataAsInt.Click += new System.EventHandler(this.LabelKeyOffset_Click);
            // 
            // textBoxDataAsInt
            // 
            this.textBoxDataAsInt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDataAsInt.Location = new System.Drawing.Point(96, 77);
            this.textBoxDataAsInt.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
            this.textBoxDataAsInt.Name = "textBoxDataAsInt";
            this.textBoxDataAsInt.ReadOnly = true;
            this.textBoxDataAsInt.Size = new System.Drawing.Size(497, 22);
            this.textBoxDataAsInt.TabIndex = 12;
            this.textBoxDataAsInt.Visible = false;
            // 
            // labelDataAsByteArray
            // 
            this.labelDataAsByteArray.AutoSize = true;
            this.labelDataAsByteArray.Location = new System.Drawing.Point(16, 204);
            this.labelDataAsByteArray.Margin = new System.Windows.Forms.Padding(10, 5, 3, 0);
            this.labelDataAsByteArray.Name = "labelDataAsByteArray";
            this.labelDataAsByteArray.Size = new System.Drawing.Size(116, 17);
            this.labelDataAsByteArray.TabIndex = 11;
            this.labelDataAsByteArray.Text = "As a byte array : ";
            this.labelDataAsByteArray.Visible = false;
            this.labelDataAsByteArray.Click += new System.EventHandler(this.LabelKeyOffset_Click);
            // 
            // textBoxDataAsByteArray
            // 
            this.textBoxDataAsByteArray.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDataAsByteArray.Location = new System.Drawing.Point(13, 227);
            this.textBoxDataAsByteArray.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
            this.textBoxDataAsByteArray.Multiline = true;
            this.textBoxDataAsByteArray.Name = "textBoxDataAsByteArray";
            this.textBoxDataAsByteArray.ReadOnly = true;
            this.textBoxDataAsByteArray.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxDataAsByteArray.Size = new System.Drawing.Size(580, 202);
            this.textBoxDataAsByteArray.TabIndex = 10;
            this.textBoxDataAsByteArray.Visible = false;
            // 
            // labelDataAsString
            // 
            this.labelDataAsString.AutoSize = true;
            this.labelDataAsString.Location = new System.Drawing.Point(16, 135);
            this.labelDataAsString.Margin = new System.Windows.Forms.Padding(10, 5, 3, 0);
            this.labelDataAsString.Name = "labelDataAsString";
            this.labelDataAsString.Size = new System.Drawing.Size(89, 17);
            this.labelDataAsString.TabIndex = 9;
            this.labelDataAsString.Text = "As a String : ";
            this.labelDataAsString.Visible = false;
            // 
            // textBoxDataAsString
            // 
            this.textBoxDataAsString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDataAsString.Location = new System.Drawing.Point(13, 159);
            this.textBoxDataAsString.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
            this.textBoxDataAsString.Multiline = true;
            this.textBoxDataAsString.Name = "textBoxDataAsString";
            this.textBoxDataAsString.ReadOnly = true;
            this.textBoxDataAsString.Size = new System.Drawing.Size(580, 43);
            this.textBoxDataAsString.TabIndex = 8;
            this.textBoxDataAsString.Visible = false;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.labelDataOffset);
            this.flowLayoutPanel1.Controls.Add(this.labelDataLength);
            this.flowLayoutPanel1.Controls.Add(this.labelDataType);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 17);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.flowLayoutPanel1.Size = new System.Drawing.Size(599, 54);
            this.flowLayoutPanel1.TabIndex = 4;
            // 
            // labelDataOffset
            // 
            this.labelDataOffset.AutoSize = true;
            this.labelDataOffset.Location = new System.Drawing.Point(10, 5);
            this.labelDataOffset.Margin = new System.Windows.Forms.Padding(10, 0, 3, 0);
            this.labelDataOffset.Name = "labelDataOffset";
            this.labelDataOffset.Size = new System.Drawing.Size(107, 17);
            this.labelDataOffset.TabIndex = 1;
            this.labelDataOffset.Tag = "Offset : {0} - {1}";
            this.labelDataOffset.Text = "Offset : {0} - {1}";
            this.labelDataOffset.Visible = false;
            this.labelDataOffset.Click += new System.EventHandler(this.LabelKeyOffset_Click);
            // 
            // labelDataLength
            // 
            this.labelDataLength.AutoSize = true;
            this.labelDataLength.Location = new System.Drawing.Point(130, 5);
            this.labelDataLength.Margin = new System.Windows.Forms.Padding(10, 0, 3, 0);
            this.labelDataLength.Name = "labelDataLength";
            this.labelDataLength.Size = new System.Drawing.Size(112, 17);
            this.labelDataLength.TabIndex = 0;
            this.labelDataLength.Tag = "DataLength : {0}";
            this.labelDataLength.Text = "DataLength : {0}";
            this.labelDataLength.Visible = false;
            // 
            // labelDataType
            // 
            this.labelDataType.AutoSize = true;
            this.labelDataType.Location = new System.Drawing.Point(256, 5);
            this.labelDataType.Margin = new System.Windows.Forms.Padding(11, 0, 3, 0);
            this.labelDataType.Name = "labelDataType";
            this.labelDataType.Size = new System.Drawing.Size(100, 17);
            this.labelDataType.TabIndex = 3;
            this.labelDataType.Tag = "DataType : {0}";
            this.labelDataType.Text = "DataType : {0}";
            this.labelDataType.Visible = false;
            // 
            // groupBoxKeyInfos
            // 
            this.groupBoxKeyInfos.Controls.Add(this.labelKeyName);
            this.groupBoxKeyInfos.Controls.Add(this.textBoxKeyName);
            this.groupBoxKeyInfos.Controls.Add(this.flowLayoutPanelKeyInfos);
            this.groupBoxKeyInfos.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxKeyInfos.Location = new System.Drawing.Point(0, 0);
            this.groupBoxKeyInfos.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBoxKeyInfos.Name = "groupBoxKeyInfos";
            this.groupBoxKeyInfos.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBoxKeyInfos.Size = new System.Drawing.Size(605, 102);
            this.groupBoxKeyInfos.TabIndex = 5;
            this.groupBoxKeyInfos.TabStop = false;
            this.groupBoxKeyInfos.Text = "Key Infos";
            // 
            // labelKeyName
            // 
            this.labelKeyName.AutoSize = true;
            this.labelKeyName.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelKeyName.Location = new System.Drawing.Point(6, 17);
            this.labelKeyName.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
            this.labelKeyName.Name = "labelKeyName";
            this.labelKeyName.Padding = new System.Windows.Forms.Padding(11, 0, 0, 0);
            this.labelKeyName.Size = new System.Drawing.Size(75, 17);
            this.labelKeyName.TabIndex = 6;
            this.labelKeyName.Tag = "";
            this.labelKeyName.Text = "Name : ";
            this.labelKeyName.Visible = false;
            // 
            // textBoxKeyName
            // 
            this.textBoxKeyName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxKeyName.Location = new System.Drawing.Point(87, 15);
            this.textBoxKeyName.Name = "textBoxKeyName";
            this.textBoxKeyName.ReadOnly = true;
            this.textBoxKeyName.Size = new System.Drawing.Size(506, 22);
            this.textBoxKeyName.TabIndex = 8;
            this.textBoxKeyName.Visible = false;
            // 
            // flowLayoutPanelKeyInfos
            // 
            this.flowLayoutPanelKeyInfos.Controls.Add(this.labelKeyOffset);
            this.flowLayoutPanelKeyInfos.Controls.Add(this.labelKeyLength);
            this.flowLayoutPanelKeyInfos.Controls.Add(this.labelkeyIsSubStructureOpening);
            this.flowLayoutPanelKeyInfos.Controls.Add(this.labelKeyIsStructureClosing);
            this.flowLayoutPanelKeyInfos.Controls.Add(this.labelKeyIsUnknownSegment);
            this.flowLayoutPanelKeyInfos.Controls.Add(this.labelKeyIsDataTypeError);
            this.flowLayoutPanelKeyInfos.Controls.Add(this.labelIsKeyValue);
            this.flowLayoutPanelKeyInfos.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanelKeyInfos.Location = new System.Drawing.Point(3, 42);
            this.flowLayoutPanelKeyInfos.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.flowLayoutPanelKeyInfos.Name = "flowLayoutPanelKeyInfos";
            this.flowLayoutPanelKeyInfos.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.flowLayoutPanelKeyInfos.Size = new System.Drawing.Size(599, 58);
            this.flowLayoutPanelKeyInfos.TabIndex = 7;
            // 
            // labelKeyOffset
            // 
            this.labelKeyOffset.AutoSize = true;
            this.labelKeyOffset.Location = new System.Drawing.Point(11, 5);
            this.labelKeyOffset.Margin = new System.Windows.Forms.Padding(11, 0, 3, 0);
            this.labelKeyOffset.Name = "labelKeyOffset";
            this.labelKeyOffset.Size = new System.Drawing.Size(107, 17);
            this.labelKeyOffset.TabIndex = 2;
            this.labelKeyOffset.Tag = "Offset : {0} - {1}";
            this.labelKeyOffset.Text = "Offset : {0} - {1}";
            this.labelKeyOffset.Visible = false;
            this.labelKeyOffset.Click += new System.EventHandler(this.LabelKeyOffset_Click);
            // 
            // labelKeyLength
            // 
            this.labelKeyLength.AutoSize = true;
            this.labelKeyLength.Location = new System.Drawing.Point(132, 5);
            this.labelKeyLength.Margin = new System.Windows.Forms.Padding(11, 0, 3, 0);
            this.labelKeyLength.Name = "labelKeyLength";
            this.labelKeyLength.Size = new System.Drawing.Size(82, 17);
            this.labelKeyLength.TabIndex = 1;
            this.labelKeyLength.Tag = "Length : {0}";
            this.labelKeyLength.Text = "Length : {0}";
            this.labelKeyLength.Visible = false;
            // 
            // labelkeyIsSubStructureOpening
            // 
            this.labelkeyIsSubStructureOpening.AutoSize = true;
            this.labelkeyIsSubStructureOpening.Location = new System.Drawing.Point(228, 5);
            this.labelkeyIsSubStructureOpening.Margin = new System.Windows.Forms.Padding(11, 0, 3, 0);
            this.labelkeyIsSubStructureOpening.Name = "labelkeyIsSubStructureOpening";
            this.labelkeyIsSubStructureOpening.Size = new System.Drawing.Size(185, 17);
            this.labelkeyIsSubStructureOpening.TabIndex = 4;
            this.labelkeyIsSubStructureOpening.Tag = "IsSubStructureOpening : {0}";
            this.labelkeyIsSubStructureOpening.Text = "IsSubStructureOpening : {0}";
            this.labelkeyIsSubStructureOpening.Visible = false;
            // 
            // labelKeyIsStructureClosing
            // 
            this.labelKeyIsStructureClosing.AutoSize = true;
            this.labelKeyIsStructureClosing.Location = new System.Drawing.Point(427, 5);
            this.labelKeyIsStructureClosing.Margin = new System.Windows.Forms.Padding(11, 0, 3, 0);
            this.labelKeyIsStructureClosing.Name = "labelKeyIsStructureClosing";
            this.labelKeyIsStructureClosing.Size = new System.Drawing.Size(152, 17);
            this.labelKeyIsStructureClosing.TabIndex = 5;
            this.labelKeyIsStructureClosing.Tag = "IsStructureClosing : {0}";
            this.labelKeyIsStructureClosing.Text = "IsStructureClosing : {0}";
            this.labelKeyIsStructureClosing.Visible = false;
            // 
            // labelKeyIsUnknownSegment
            // 
            this.labelKeyIsUnknownSegment.AutoSize = true;
            this.labelKeyIsUnknownSegment.Location = new System.Drawing.Point(11, 22);
            this.labelKeyIsUnknownSegment.Margin = new System.Windows.Forms.Padding(11, 0, 3, 0);
            this.labelKeyIsUnknownSegment.Name = "labelKeyIsUnknownSegment";
            this.labelKeyIsUnknownSegment.Size = new System.Drawing.Size(162, 17);
            this.labelKeyIsUnknownSegment.TabIndex = 6;
            this.labelKeyIsUnknownSegment.Tag = "IsUnknownSegment : {0}";
            this.labelKeyIsUnknownSegment.Text = "IsUnknownSegment : {0}";
            this.labelKeyIsUnknownSegment.Visible = false;
            // 
            // labelKeyIsDataTypeError
            // 
            this.labelKeyIsDataTypeError.AutoSize = true;
            this.labelKeyIsDataTypeError.Location = new System.Drawing.Point(187, 22);
            this.labelKeyIsDataTypeError.Margin = new System.Windows.Forms.Padding(11, 0, 3, 0);
            this.labelKeyIsDataTypeError.Name = "labelKeyIsDataTypeError";
            this.labelKeyIsDataTypeError.Size = new System.Drawing.Size(142, 17);
            this.labelKeyIsDataTypeError.TabIndex = 7;
            this.labelKeyIsDataTypeError.Tag = "IsDataTypeError : {0}";
            this.labelKeyIsDataTypeError.Text = "IsDataTypeError : {0}";
            this.labelKeyIsDataTypeError.Visible = false;
            // 
            // labelIsKeyValue
            // 
            this.labelIsKeyValue.AutoSize = true;
            this.labelIsKeyValue.Location = new System.Drawing.Point(343, 22);
            this.labelIsKeyValue.Margin = new System.Windows.Forms.Padding(11, 0, 3, 0);
            this.labelIsKeyValue.Name = "labelIsKeyValue";
            this.labelIsKeyValue.Size = new System.Drawing.Size(108, 17);
            this.labelIsKeyValue.TabIndex = 8;
            this.labelIsKeyValue.Tag = "IsKeyValue : {0}";
            this.labelIsKeyValue.Text = "IsKeyValue : {0}";
            this.labelIsKeyValue.Visible = false;
            // 
            // groupBoxFileInfos
            // 
            this.groupBoxFileInfos.Controls.Add(this.flowLayoutPanelFileInfos);
            this.groupBoxFileInfos.Controls.Add(this.linkLabelFilePath);
            this.groupBoxFileInfos.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxFileInfos.Location = new System.Drawing.Point(0, 0);
            this.groupBoxFileInfos.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBoxFileInfos.Name = "groupBoxFileInfos";
            this.groupBoxFileInfos.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBoxFileInfos.Size = new System.Drawing.Size(922, 82);
            this.groupBoxFileInfos.TabIndex = 11;
            this.groupBoxFileInfos.TabStop = false;
            this.groupBoxFileInfos.Text = "File Infos";
            // 
            // flowLayoutPanelFileInfos
            // 
            this.flowLayoutPanelFileInfos.Controls.Add(this.labelFileSize);
            this.flowLayoutPanelFileInfos.Controls.Add(this.labelFileName);
            this.flowLayoutPanelFileInfos.Controls.Add(this.labelFileExtension);
            this.flowLayoutPanelFileInfos.Controls.Add(this.labelFileVersion);
            this.flowLayoutPanelFileInfos.Controls.Add(this.labelFileFoundKeys);
            this.flowLayoutPanelFileInfos.Controls.Add(this.labelFileDataTypeErrors);
            this.flowLayoutPanelFileInfos.Controls.Add(this.labelFileUnknownSegments);
            this.flowLayoutPanelFileInfos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelFileInfos.Location = new System.Drawing.Point(3, 34);
            this.flowLayoutPanelFileInfos.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.flowLayoutPanelFileInfos.Name = "flowLayoutPanelFileInfos";
            this.flowLayoutPanelFileInfos.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.flowLayoutPanelFileInfos.Size = new System.Drawing.Size(916, 46);
            this.flowLayoutPanelFileInfos.TabIndex = 1;
            // 
            // labelFileSize
            // 
            this.labelFileSize.AutoSize = true;
            this.labelFileSize.Location = new System.Drawing.Point(11, 5);
            this.labelFileSize.Margin = new System.Windows.Forms.Padding(11, 0, 3, 0);
            this.labelFileSize.Name = "labelFileSize";
            this.labelFileSize.Size = new System.Drawing.Size(87, 17);
            this.labelFileSize.TabIndex = 0;
            this.labelFileSize.Text = "FileSize : {0}";
            // 
            // labelFileName
            // 
            this.labelFileName.AutoSize = true;
            this.labelFileName.Location = new System.Drawing.Point(112, 5);
            this.labelFileName.Margin = new System.Windows.Forms.Padding(11, 0, 3, 0);
            this.labelFileName.Name = "labelFileName";
            this.labelFileName.Size = new System.Drawing.Size(75, 17);
            this.labelFileName.TabIndex = 1;
            this.labelFileName.Text = "Name : {0}";
            // 
            // labelFileExtension
            // 
            this.labelFileExtension.AutoSize = true;
            this.labelFileExtension.Location = new System.Drawing.Point(201, 5);
            this.labelFileExtension.Margin = new System.Windows.Forms.Padding(11, 0, 3, 0);
            this.labelFileExtension.Name = "labelFileExtension";
            this.labelFileExtension.Size = new System.Drawing.Size(57, 17);
            this.labelFileExtension.TabIndex = 2;
            this.labelFileExtension.Text = "Ext : {0}";
            // 
            // labelFileVersion
            // 
            this.labelFileVersion.AutoSize = true;
            this.labelFileVersion.Location = new System.Drawing.Point(272, 5);
            this.labelFileVersion.Margin = new System.Windows.Forms.Padding(11, 0, 3, 0);
            this.labelFileVersion.Name = "labelFileVersion";
            this.labelFileVersion.Size = new System.Drawing.Size(86, 17);
            this.labelFileVersion.TabIndex = 3;
            this.labelFileVersion.Text = "Version : {0}";
            // 
            // labelFileFoundKeys
            // 
            this.labelFileFoundKeys.AutoSize = true;
            this.labelFileFoundKeys.Location = new System.Drawing.Point(372, 5);
            this.labelFileFoundKeys.Margin = new System.Windows.Forms.Padding(11, 0, 3, 0);
            this.labelFileFoundKeys.Name = "labelFileFoundKeys";
            this.labelFileFoundKeys.Size = new System.Drawing.Size(113, 17);
            this.labelFileFoundKeys.TabIndex = 4;
            this.labelFileFoundKeys.Text = "Found Keys : {0}";
            // 
            // labelFileDataTypeErrors
            // 
            this.labelFileDataTypeErrors.AutoSize = true;
            this.labelFileDataTypeErrors.Location = new System.Drawing.Point(499, 5);
            this.labelFileDataTypeErrors.Margin = new System.Windows.Forms.Padding(11, 0, 3, 0);
            this.labelFileDataTypeErrors.Name = "labelFileDataTypeErrors";
            this.labelFileDataTypeErrors.Size = new System.Drawing.Size(165, 17);
            this.labelFileDataTypeErrors.TabIndex = 5;
            this.labelFileDataTypeErrors.Text = "DataType Errors : {0}/{1}";
            // 
            // labelFileUnknownSegments
            // 
            this.labelFileUnknownSegments.AutoSize = true;
            this.labelFileUnknownSegments.Location = new System.Drawing.Point(678, 5);
            this.labelFileUnknownSegments.Margin = new System.Windows.Forms.Padding(11, 0, 3, 0);
            this.labelFileUnknownSegments.Name = "labelFileUnknownSegments";
            this.labelFileUnknownSegments.Size = new System.Drawing.Size(183, 17);
            this.labelFileUnknownSegments.TabIndex = 6;
            this.labelFileUnknownSegments.Text = "Unknown segments : {0}/{1}";
            // 
            // linkLabelFilePath
            // 
            this.linkLabelFilePath.AutoSize = true;
            this.linkLabelFilePath.Dock = System.Windows.Forms.DockStyle.Top;
            this.linkLabelFilePath.Location = new System.Drawing.Point(3, 17);
            this.linkLabelFilePath.Name = "linkLabelFilePath";
            this.linkLabelFilePath.Padding = new System.Windows.Forms.Padding(11, 0, 0, 0);
            this.linkLabelFilePath.Size = new System.Drawing.Size(126, 17);
            this.linkLabelFilePath.TabIndex = 0;
            this.linkLabelFilePath.TabStop = true;
            this.linkLabelFilePath.Text = "linkLabelFilePath";
            this.linkLabelFilePath.Click += new System.EventHandler(this.LinkLabelFilePath_Click);
            // 
            // labelDataAsFloat
            // 
            this.labelDataAsFloat.AutoSize = true;
            this.labelDataAsFloat.Location = new System.Drawing.Point(16, 110);
            this.labelDataAsFloat.Margin = new System.Windows.Forms.Padding(10, 5, 3, 0);
            this.labelDataAsFloat.Name = "labelDataAsFloat";
            this.labelDataAsFloat.Size = new System.Drawing.Size(79, 17);
            this.labelDataAsFloat.TabIndex = 15;
            this.labelDataAsFloat.Text = "As a Float :";
            this.labelDataAsFloat.Visible = false;
			this.labelDataAsFloat.Click += new System.EventHandler(this.LabelKeyOffset_Click);
			// 
			// textBoxDataAsFloat
			// 
			this.textBoxDataAsFloat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDataAsFloat.Location = new System.Drawing.Point(96, 108);
            this.textBoxDataAsFloat.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
            this.textBoxDataAsFloat.Name = "textBoxDataAsFloat";
            this.textBoxDataAsFloat.ReadOnly = true;
            this.textBoxDataAsFloat.Size = new System.Drawing.Size(497, 22);
            this.textBoxDataAsFloat.TabIndex = 14;
            this.textBoxDataAsFloat.Visible = false;
            // 
            // TabPageFileContent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.groupBoxFileInfos);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "TabPageFileContent";
            this.Size = new System.Drawing.Size(922, 625);
            this.Load += new System.EventHandler(this.TabPageContent_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBoxDetectedKeys.ResumeLayout(false);
            this.groupBoxDetectedKeys.PerformLayout();
            this.panelTreeToolBox.ResumeLayout(false);
            this.groupBoxKeyData.ResumeLayout(false);
            this.groupBoxKeyData.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.groupBoxKeyInfos.ResumeLayout(false);
            this.groupBoxKeyInfos.PerformLayout();
            this.flowLayoutPanelKeyInfos.ResumeLayout(false);
            this.flowLayoutPanelKeyInfos.PerformLayout();
            this.groupBoxFileInfos.ResumeLayout(false);
            this.groupBoxFileInfos.PerformLayout();
            this.flowLayoutPanelFileInfos.ResumeLayout(false);
            this.flowLayoutPanelFileInfos.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.GroupBox groupBoxDetectedKeys;
		private System.Windows.Forms.TreeView treeViewKeys;
		private System.Windows.Forms.GroupBox groupBoxKeyData;
		private System.Windows.Forms.GroupBox groupBoxKeyInfos;
		private System.Windows.Forms.GroupBox groupBoxFileInfos;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelFileInfos;
		private System.Windows.Forms.Label labelFileSize;
		private System.Windows.Forms.LinkLabel linkLabelFilePath;
		private System.Windows.Forms.Label labelFileName;
		private System.Windows.Forms.Label labelFileExtension;
		private System.Windows.Forms.Label labelFileVersion;
		private System.Windows.Forms.Label labelFileFoundKeys;
		private System.Windows.Forms.Label labelFileDataTypeErrors;
		private System.Windows.Forms.Label labelFileUnknownSegments;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelKeyInfos;
		private System.Windows.Forms.Label labelKeyOffset;
		private System.Windows.Forms.Label labelKeyLength;
		private System.Windows.Forms.Label labelDataType;
		private System.Windows.Forms.Label labelkeyIsSubStructureOpening;
		private System.Windows.Forms.Label labelKeyIsStructureClosing;
		private System.Windows.Forms.Label labelKeyIsUnknownSegment;
		private System.Windows.Forms.Label labelKeyIsDataTypeError;
		private System.Windows.Forms.Label labelKeyName;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.Label labelDataOffset;
		private System.Windows.Forms.Label labelDataLength;
		private System.Windows.Forms.Label labelDataAsInt;
		private System.Windows.Forms.TextBox textBoxDataAsInt;
		private System.Windows.Forms.Label labelDataAsByteArray;
		private System.Windows.Forms.TextBox textBoxDataAsByteArray;
		private System.Windows.Forms.Label labelDataAsString;
		private System.Windows.Forms.TextBox textBoxDataAsString;
		private System.Windows.Forms.Panel panelTreeToolBox;
		private System.Windows.Forms.Button buttonCopyTree;
		private System.Windows.Forms.ComboBox comboBoxCopyType;
		private System.Windows.Forms.ToolTip toolTipCopy;
		private System.Windows.Forms.TextBox textBoxKeyName;
		private System.Windows.Forms.Label labelIsKeyValue;
		private System.Windows.Forms.Label labelDataAsFloat;
		private System.Windows.Forms.TextBox textBoxDataAsFloat;
	}
}
