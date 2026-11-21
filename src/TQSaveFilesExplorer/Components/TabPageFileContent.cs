using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TQ.SaveFilesExplorer.Services;
using TQ.SaveFilesExplorer.Entities;
using System.Diagnostics;
using System.IO;
using EnumsNET;
using TQVaultAE.GameFiles;

namespace TQ.SaveFilesExplorer.Components;

public partial class TabPageFileContent : UserControl
{
private bool _DisplayDataDecimal = true;
	private readonly TQFileService Service;

	/// <summary>
	/// File shown in this page. Set when the page loads.
	/// </summary>
	public TQFile? File { get; private set; }

	public TabPageFileContent()
	{
		Service = new TQFileService();

		InitializeComponent();
	}

	private void TabPageContent_Load(object sender, EventArgs e)
	{
		var path = this.Tag as string ?? throw new InvalidOperationException("The file path must be set in Tag before the page loads.");
		var file = this.Service.ReadFile(path);
		this.File = file;

		var tree = MakeTreeNode(file.Childs.ToList()).ToArray();
		this.treeViewKeys.Nodes.AddRange(tree);

		RecurseColor(tree);

		// init labels
		var recordcount = file.Records.Length;
		var errors = file.Records.Count(r => r.IsDataTypeError);
		var unknown = file.Records.Count(r => r.IsUnknownSegment);
		var foundkeys = file.Records.Count(r => !r.IsUnknownSegment);

		#region File

		this.linkLabelFilePath.Text = file.Path;

		this.labelFileDataTypeErrors.Text = string.Format(this.labelFileDataTypeErrors.Text, errors, recordcount);
		if (errors > 0) this.labelFileDataTypeErrors.ForeColor = Color.Orange;

		this.labelFileExtension.Text = string.Format(this.labelFileExtension.Text, file.Ext);
		this.labelFileName.Text = string.Format(this.labelFileName.Text, Path.GetFileNameWithoutExtension(file.Path));
		this.labelFileSize.Text = string.Format(this.labelFileSize.Text, file.Content.Length);

		this.labelFileUnknownSegments.Text = string.Format(this.labelFileUnknownSegments.Text, unknown, recordcount);
		if (unknown > 0) this.labelFileUnknownSegments.ForeColor = Color.Red;

		this.labelFileVersion.Text = string.Format(this.labelFileVersion.Text, file.Version);
		this.labelFileFoundKeys.Text = string.Format(this.labelFileFoundKeys.Text, foundkeys);

		#endregion

		// Init comboBoxCopyType
		this.comboBoxCopyType.Items.Clear();
		this.comboBoxCopyType.Items.AddRange([
			new ComboBoxItem(){ DisplayName = "Keys Only", Value = CopyKeysType.KeysOnly.ToString()  },
			new ComboBoxItem(){ DisplayName = "Keys + Type", Value = CopyKeysType.KeysType.ToString()  },
			new ComboBoxItem(){ DisplayName = "Keys + Type + Data", Value = CopyKeysType.KeysTypeData.ToString()  },
			new ComboBoxItem(){ DisplayName = "Keys Only : Distinct", Value = CopyKeysType.DistinctKeys.ToString()  },
			new ComboBoxItem(){ DisplayName = "Keys + Type : Distinct", Value = CopyKeysType.DistinctKeysType.ToString()  },
			new ComboBoxItem(){ DisplayName = "Keys + Type + Data : Distinct", Value = CopyKeysType.DistinctKeysTypeData.ToString()  }
		]);

	}

	private List<TreeNode> MakeTreeNode(List<TQFileRecord> childs)
	{
		List<TreeNode> currentLvl = [];
		foreach (var k in childs)
		{
			var tn = new TreeNode(k.KeyName) { Tag = k };

			if (k.Childs.Count != 0)
			{
				tn.Nodes.AddRange(MakeTreeNode(k.Childs).ToArray());
			}

			currentLvl.Add(tn);
		}

		return currentLvl;
	}

	/// <summary>
	/// Colorize nodes
	/// </summary>
	/// <param name="tree"></param>
	/// <returns></returns>
	private Color? RecurseColor(TreeNode[] tree)
	{
		Color? errlvl = null;
		foreach (var node in tree)
		{
			// Every node made by MakeTreeNode holds its record
			if (node.Tag is not TQFileRecord k) continue;
			var f = new Font("Microsoft Sans Serif", 8, FontStyle.Bold);

			if (k.DataType == TQFileDataType.Unknown)
			{
				if (errlvl != Color.Red) errlvl = Color.Orange;
				node.ForeColor = Color.Orange;
				node.NodeFont = f;
			}

			if (k.IsUnknownSegment)
			{
				errlvl = Color.Red;
				node.ForeColor = Color.Red;
				node.NodeFont = f;
			}

			var childs = node.Nodes.Cast<TreeNode>().ToArray();
			var nestedErrlvl = RecurseColor(childs);
			if (nestedErrlvl is not null)
			{
				if (errlvl != Color.Red) errlvl = nestedErrlvl;
				node.ForeColor = nestedErrlvl.Value;
				node.NodeFont = f;
			}
		}
		return errlvl;
	}

	private TQFileRecord? SelectedRecord
	{
		get => this.treeViewKeys.SelectedNode?.Tag as TQFileRecord;
	}

	private void SetVisibility(Control ctrl, bool isVisible)
	{
		foreach (Control c in ctrl.Controls) SetVisibility(c, isVisible);
		ctrl.Visible = isVisible;
	}

	private bool _flowLayoutPanelKeyInfos_FirstTime = true;
	private void TreeViewKeys_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
	{
		if (e.Node?.Tag is TQFileRecord k) DisplayNodeInfos(k);
	}

	/// <summary>
	/// Text format kept in the <see cref="Control.Tag"/> of a label.
	/// </summary>
	private static string TagFormat(Control label) => label.Tag?.ToString() ?? string.Empty;

	private void DisplayNodeInfos(TQFileRecord k)
	{
		#region flowLayoutPanelKeyInfos

		if (_flowLayoutPanelKeyInfos_FirstTime)
		{
			_flowLayoutPanelKeyInfos_FirstTime = false;
			SetVisibility(groupBoxKeyInfos, true);
			SetVisibility(groupBoxKeyData, true);
		}

		#region Key

		this.labelKeyIsDataTypeError.Text = string.Format(TagFormat(this.labelKeyIsDataTypeError), k.IsDataTypeError);
		this.labelKeyIsStructureClosing.Text = string.Format(TagFormat(this.labelKeyIsStructureClosing), k.IsStructureClosing);
		this.labelkeyIsSubStructureOpening.Text = string.Format(TagFormat(this.labelkeyIsSubStructureOpening), k.IsSubStructureOpening);
		this.labelKeyIsUnknownSegment.Text = string.Format(TagFormat(this.labelKeyIsUnknownSegment), k.IsUnknownSegment);
		this.labelKeyLength.Text = string.Format(TagFormat(this.labelKeyLength), k.KeyLengthAsInt);
		this.textBoxKeyName.Text = k.KeyName;
		this.labelIsKeyValue.Text = string.Format(TagFormat(this.labelIsKeyValue), k.IsKeyValue);

		#endregion

		Display_labelOffset(k);

		#endregion

		#region DataInfos

		this.labelDataType.Text = string.Format(TagFormat(this.labelDataType), k.DataType);
		this.labelDataLength.Text = string.Format(TagFormat(this.labelDataLength), k.ValueEnd - k.ValueStart + 1);

		Display_textData(k);

		#endregion
	}

	private void Display_textData(TQFileRecord k)
	{
		this.textBoxDataAsByteArray.Text = string.Join(" ", k.DataAsByteArray.Select(b => _DisplayDataDecimal ? b.ToString() : b.ToString("X2")));
		this.textBoxDataAsInt.Text = _DisplayDataDecimal ? $"{k.DataAsInt}" : $"{k.DataAsInt:X8}";
		this.textBoxDataAsFloat.Text = _DisplayDataDecimal ? $"{k.DataAsFloat}" : $"{k.DataAsInt:X8}";
		this.textBoxDataAsString.Text = k.DataType == TQFileDataType.Unknown ? SanitizeString(k.DataAsByteArray) : k.DataAsStr;
	}

	private static Dictionary<byte, string> specialCharsMapping = new Dictionary<byte, string>()
	{
		[0] = "[NUL]", // NUL nul
		[1] = "[SOH]", // SOH start of header
		[2] = "[STX]", // STX start of text
		[3] = "[ETX]", // ETX end of text
		[4] = "[EOT]", // EOT end of transmission
		[5] = "[ENQ]", // ENQ enquiry
		[6] = "[ACK]", // ACK acknowledege
		[7] = "[BEL]", // BEL bell
		[8] = "[BS]", // BS backspace [\b]
		[9] = "[HT]", // HT horizontal tab [\t]
		[10] = "[LF]", // LF line feed [\n]
		[11] = "[VT]", // VT vertical tab
		[12] = "[FF]", // FF form feed [\f]
		[13] = "[CR]", // CR carriage return [\r]
		[14] = "[SO]", // SO shift out
		[15] = "[SI]", // SI shift in
		[16] = "[DLE]", // DLE data link escape
		[17] = "[DC1]", // DC1 device control 1, XON resume transmission
		[18] = "[DC2]", // DC2 device control 2
		[19] = "[DC3]", // DC3 device control 3, XOFF pause transmission
		[20] = "[DC4]", // DC4 device control 4
		[21] = "[NAK]", // NAK negative acknowledge
		[22] = "[SYN]", // SYN synchronize
		[23] = "[ETB]", // ETB end text block
		[24] = "[CAN]", // CAN cancel
		[25] = "[EM]", // EM end message
		[26] = "[SUB]", // SUB substitute
		[27] = "[ESC]", // ESC escape
		[28] = "[FS]", // FS file separator
		[29] = "[GS]", // GS group separator
		[30] = "[RS]", // RS record separator
		[31] = "[US]", // US unit separator
	};

	private string SanitizeString(byte[] dataAsByteArray)
	{
		List<string> result = [];
		foreach (var b in dataAsByteArray)
		{
			result.Add(b < 32 ? specialCharsMapping[b] : TextEncodings.Cp1252.GetString([b]));
		}
		return string.Concat(result.ToArray());
	}

	private void Display_labelOffset(TQFileRecord k)
	{
		var end = k.KeyEnd;
		this.labelKeyOffset.Text = string.Format(TagFormat(this.labelKeyOffset)
			, _DisplayDataDecimal ? $"{k.KeyIndex}" : $"0x{k.KeyIndex:X8}"
			, _DisplayDataDecimal ? $"{end}" : $"0x{end:X8}"
		);
		this.labelDataOffset.Text = string.Format(TagFormat(this.labelDataOffset)
			, _DisplayDataDecimal ? $"{k.ValueStart}" : $"0x{k.ValueStart:X8}"
			, _DisplayDataDecimal ? $"{k.ValueEnd}" : $"0x{k.ValueEnd:X8}"
		);
	}

	private void LinkLabelFilePath_Click(object sender, EventArgs e)
	{
		if (this.File is null) return;

		// Focus in explorer
		Process.Start(@"explorer.exe", $@"/select,""{this.File.Path}""");
	}

	private void LabelKeyOffset_Click(object sender, EventArgs e)
	{
		var k = this.SelectedRecord;
		if (k is null) return;

		_DisplayDataDecimal = !_DisplayDataDecimal;
		Display_labelOffset(k);
		Display_textData(k);
	}

	private void ButtonCopyTree_Click(object sender, EventArgs e)
	{
		var item = this.comboBoxCopyType.SelectedItem as ComboBoxItem;
		if (item is null)
		{
			MainForm.SetStatusMessage("You must choose a format in the combobox !");
			return;
		}

		if (this.File is null) return;

		var type = Enums.Parse<CopyKeysType>(item.Value);
		string keylist = string.Empty;
		switch (type)
		{
			case CopyKeysType.KeysOnly:
				keylist = string.Join(Environment.NewLine, this.File.Records.Select(r => r.KeyName).ToArray());
				break;
			case CopyKeysType.KeysType:
				keylist = string.Join(Environment.NewLine, this.File.Records.Select(r => $"{r.KeyName} : {r.DataType}").ToArray());
				break;
			case CopyKeysType.KeysTypeData:
				keylist = string.Join(Environment.NewLine, this.File.Records.Select(r => $"{r.KeyName} : {r.DataType} : {r.GetDataAsString(_DisplayDataDecimal)}").ToArray());
				break;
			case CopyKeysType.DistinctKeys:
				keylist = string.Join(Environment.NewLine, this.File.Records.Select(r => r.KeyName).Distinct().ToArray());
				break;
			case CopyKeysType.DistinctKeysType:
				keylist = string.Join(Environment.NewLine, this.File.Records.Select(r => $"{r.KeyName} : {r.DataType}").Distinct().ToArray());
				break;
			case CopyKeysType.DistinctKeysTypeData:
				keylist = string.Join(Environment.NewLine, this.File.Records.Select(r => $"{r.KeyName} : {r.DataType} : {r.GetDataAsString(_DisplayDataDecimal)}").Distinct().ToArray());
				break;
			default:
				break;
		}
		System.Windows.Forms.Clipboard.SetText(keylist);
		MainForm.SetStatusMessage("The data is copied in your clipboard !");
	}

	private void TreeViewKeys_AfterSelect(object sender, TreeViewEventArgs e)
	{
		if (e.Node?.Tag is TQFileRecord k) DisplayNodeInfos(k);
	}
}
