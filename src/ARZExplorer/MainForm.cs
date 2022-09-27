//-----------------------------------------------------------------------
// <copyright file="Form1.cs" company="VillageIdiot">
//     Copyright (c) Village Idiot. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using ArzExplorer.Models;
using ArzExplorer.Properties;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using TQVaultAE.Domain.Contracts.Providers;
using TQVaultAE.Domain.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Logs;

namespace ArzExplorer;

/// <summary>
/// Main Form for ArzExplorer
/// </summary>
public partial class MainForm : Form
{
	const StringComparison noCase = StringComparison.OrdinalIgnoreCase;

	private readonly ILogger Log;
	private readonly IArcFileProvider arcProv;
	private readonly IArzFileProvider arzProv;
	private readonly IDBRecordCollectionProvider DBRecordCollectionProvider;
	private readonly IBitmapService BitmapService;
	private readonly IGamePathService GamePathService;

	/// <summary>
	/// Holds the initial size of the form.
	/// </summary>
	private Size initialSize;

	#region MainForm

	/// <summary>
	/// Initializes a new instance of the Form1 class.
	/// </summary>
	public MainForm(
		ILogger<MainForm> log
		, IArcFileProvider arcFileProvider
		, IArzFileProvider arzFileProvider
		, IDBRecordCollectionProvider dBRecordCollectionProvider
		, IBitmapService bitmapService
		, IGamePathService gamePathService
	)
	{
		this.InitializeComponent();

		this.Log = log;
		this.arcProv = arcFileProvider;
		this.arzProv = arzFileProvider;
		this.DBRecordCollectionProvider = dBRecordCollectionProvider;
		this.BitmapService = bitmapService;
		this.GamePathService = gamePathService;

		Assembly a = Assembly.GetExecutingAssembly();
		this.assemblyName = a.GetName();

		this.selectedFileToolStripMenuItem.Enabled = false;
		this.allFilesToolStripMenuItem.Enabled = false;
		this.initialSize = this.Size;
		this.toolStripStatusLabel.Text = string.Empty;
	}

	private void MainForm_Load(object sender, EventArgs e)
	{
		// Seek for all arc files
		var arc = Directory.GetFiles(this.GamePathService.ImmortalThronePath, "*.arc", SearchOption.AllDirectories);
		ArcFileList = arc.ToDictionary(
			k => k.Replace(this.GamePathService.ImmortalThronePath + '\\', string.Empty).ToRecordId()
		);
	}

	/// <summary>
	/// Handler for clicking exit on the menu
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
	{
		this.DialogResult = DialogResult.Cancel;
		this.Close();
	}

	private void HideAllBox()
	{
		this.dataGridViewDetails.Visible = false;
		this.textBoxDetails.Visible = false;
		this.panelPicture.Visible = false;
		this.toolStripStatusLabel.Text = string.Empty;
	}

	private void ShowPictureBox(Image bitmap)
	{
		this.dataGridViewDetails.Visible = false;
		this.textBoxDetails.Visible = false;
		this.panelPicture.Visible = true;
		this.pictureBoxItem.Image = bitmap;
		this.pictureBoxItem.Size = bitmap.Size;
		this.toolStripStatusLabel.Text = string.Format("PixelFormat : {0}, Size : {1}", bitmap.PixelFormat, bitmap.Size);
	}

	private void ShowGridView(int Count)
	{
		this.dataGridViewDetails.Visible = true;
		this.dataGridViewDetails.Dock = DockStyle.Fill;
		this.textBoxDetails.Visible = false;
		this.panelPicture.Visible = false;
		this.toolStripStatusLabel.Text = string.Format("Record Count : {0}", Count);
	}

	private void ShowTextboxDetail(int Count)
	{
		this.dataGridViewDetails.Visible = false;
		this.textBoxDetails.Visible = true;
		this.textBoxDetails.Dock = DockStyle.Fill;
		this.panelPicture.Visible = false;
		this.toolStripStatusLabel.Text = string.Format("Line Count : {0}", Count);
	}

	#endregion

	#region Open File

	internal TQFileInfo SelectedFile;

	/// <summary>
	/// Resolve database file
	/// </summary>
	string DataBasePath => Path.Combine(this.GamePathService.ImmortalThronePath, @"Database\database.arz");

	private readonly Dictionary<string, TQFileInfo> TQFileOpened = new();

	/// <summary>
	/// Found Arc Files
	/// </summary>
	Dictionary<RecordId, string> ArcFileList;

	private void toolStripButtonLoadDataBase_Click(object sender, EventArgs e)
	{
		try
		{
			this.OpenFile(this.DataBasePath);
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message);
			return;
		}
	}

	/// <summary>
	/// Opens a file and updates the tree view.
	/// </summary>
	/// <param name="filename">Name of the file we want to open.</param>
	private void OpenFile(string filename)
	{
		if (string.IsNullOrEmpty(filename))
			return;

		// Already loaded
		if (this.TQFileOpened.Any(kv => kv.Value.sourceFile == filename))
			return;

		var fileInfo = new TQFileInfo();

		fileInfo.sourceFile = filename;

		if (string.IsNullOrEmpty(fileInfo.sourceFile))
		{
			MessageBox.Show("You must enter a valid source file path.");
			return;
		}

		// See if path exists and create it if necessary
		string fullSrcPath = Path.GetFullPath(fileInfo.sourceFile);

		if (!File.Exists(fullSrcPath))
		{
			// they did not give us a file
			MessageBox.Show("You must specify a file!");
			return;
		}

		// Try to read it as an ARC file since those have a header.
		fileInfo.ARCFile = new ArcFile(fileInfo.sourceFile);
		if (arcProv.Read(fileInfo.ARCFile))
		{
			fileInfo.FileType = CompressedFileType.ArcFile;
		}
		else
		{
			fileInfo.ARCFile = null;
			fileInfo.FileType = CompressedFileType.Unknown;
		}

		// Try reading the file as an ARZ file.
		if (fileInfo.FileType == CompressedFileType.Unknown)
		{
			// Read our ARZ file into memory.
			fileInfo.ARZFile = new ArzFile(fileInfo.sourceFile);
			if (arzProv.Read(fileInfo.ARZFile))
			{
				fileInfo.FileType = CompressedFileType.ArzFile;
			}
			else
			{
				fileInfo.ARZFile = null;
				fileInfo.FileType = CompressedFileType.Unknown;
			}
		}

		// We failed reading the file
		// so we just clear everything out.
		if (fileInfo.FileType == CompressedFileType.Unknown)
		{
			this.Text = this.assemblyName.Name;
			this.treeViewTOC.Nodes.Clear();
			this.selectedFileToolStripMenuItem.Enabled = false;
			this.allFilesToolStripMenuItem.Enabled = false;
			this.textBoxDetails.Lines = null;
			MessageBox.Show(string.Format("Error Reading {0}", fileInfo.sourceFile));
			return;
		}

		this.selectedFileToolStripMenuItem.Enabled = true;
		this.allFilesToolStripMenuItem.Enabled = true;
		this.hideZeroValuesToolStripMenuItem.Enabled = fileInfo.FileType == CompressedFileType.ArzFile;

		this.Text = string.Format("{0} - {1}", this.assemblyName.Name, fileInfo.sourceFile);

		this.textBoxDetails.Lines = null;

		this.toolStripStatusLabel.Text = string.Empty;

		this.TQFileOpened.Add(fullSrcPath, fileInfo);
		this.SelectedFile = fileInfo;

		this.BuildTreeView();
	}

	/// <summary>
	/// Handler for clicking Open on the menu.  Opens a file.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
	{
		OpenFileDialog openDialog = new OpenFileDialog();
		openDialog.Filter = "Compressed TQ files (*.arz;*.arc)|*.arz;*.arc|All files (*.*)|*.*";
		openDialog.FilterIndex = 1;
		openDialog.RestoreDirectory = true;

		// Try to read the game path
		string startPath = this.GamePathService.ImmortalThronePath;

		// If the registry fails then default to the save folder.
		if (startPath.Length < 1)
		{
			startPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games", "Titan Quest");
		}

		openDialog.InitialDirectory = startPath;
		DialogResult result = openDialog.ShowDialog();
		if (result == DialogResult.OK)
		{
			this.OpenFile(openDialog.FileName);
		}
	}

	/// <summary>
	/// Drag and drop handler
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">DragEventArgs data</param>
	private void Form1_DragDrop(object sender, DragEventArgs e)
	{
		// Handle FileDrop data.
		if (e.Data.GetDataPresent(DataFormats.FileDrop))
		{
			// Assign the file names to a string array, in
			// case the user has selected multiple files.
			string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
			try
			{
				this.OpenFile(files[0]);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				return;
			}
		}
	}

	/// <summary>
	/// Handler for entering the form with a drag item.  Changes the cursor.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">DragEventArgs data</param>
	private void Form1_DragEnter(object sender, DragEventArgs e)
	{
		// If the data is a file display the copy cursor.
		e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
	}

	#endregion

	#region Extract selected file

	/// <summary>
	/// Handler for clicking extract selected file.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void SelectedFileToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (string.IsNullOrEmpty(this.SelectedFile.destFile) || this.SelectedFile.FileType == CompressedFileType.Unknown)
			return;

		string filename = null;
		SaveFileDialog saveFileDialog = new SaveFileDialog();

		saveFileDialog.Filter = "TQ files (*.txt;*.dbr;*.tex;*.msh;*.anm;*.fnt;*.qst;*.pfx;*.ssh)|*.txt;*.dbr;*.tex;*.msh;*.anm;*.fnt;*.qst;*.pfx;*.ssh|All files (*.*)|*.*";
		saveFileDialog.FilterIndex = 1;
		saveFileDialog.RestoreDirectory = true;
		saveFileDialog.Title = "Save the Titan Quest File";
		string startPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games", "Titan Quest");
		saveFileDialog.InitialDirectory = startPath;
		saveFileDialog.FileName = Path.GetFileName(this.SelectedFile.destFile);

		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			filename = saveFileDialog.FileName;

			if (this.SelectedFile.FileType == CompressedFileType.ArzFile)
			{
				DBRecordCollectionProvider.Write(this.SelectedFile.record, Path.GetDirectoryName(filename), Path.GetFileName(filename));
			}
			else if (this.SelectedFile.FileType == CompressedFileType.ArcFile)
			{
				arcProv.Write(
					this.SelectedFile.ARCFile
					, Path.GetDirectoryName(filename)
					, this.SelectedFile.destFile
					, Path.GetFileName(filename)
				);
			}
		}
	}

	#endregion

	#region Extract all files

	/// <summary>
	/// Handler for clicking extract all files from the menu.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void AllFilesToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (this.SelectedFile.FileType == CompressedFileType.Unknown)
			return;

		if ((this.SelectedFile.FileType == CompressedFileType.ArzFile && this.SelectedFile.ARZFile == null)
			|| (this.SelectedFile.FileType == CompressedFileType.ArcFile && this.SelectedFile.ARCFile == null)
		)
		{
			MessageBox.Show("Please Open a source file.");
			return;
		}

		using (FolderBrowserDialog browseDialog = new FolderBrowserDialog())
		{
			browseDialog.Description = "Select the destination folder for the extracted database records";
			browseDialog.ShowNewFolderButton = true;
			string startPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games", "Titan Quest");
			browseDialog.SelectedPath = startPath;
			DialogResult result = browseDialog.ShowDialog();

			if (result != DialogResult.OK)
				return;

			this.SelectedFile.destDirectory = browseDialog.SelectedPath;
		}

		string fullDestPath = null;

		if (string.IsNullOrEmpty(this.SelectedFile.destDirectory))
		{
			MessageBox.Show("You must enter a valid destination folder.");
			return;
		}

		// See if path exists and create it if necessary
		if (!string.IsNullOrEmpty(this.SelectedFile.destDirectory))
		{
			fullDestPath = Path.GetFullPath(this.SelectedFile.destDirectory);
		}

		if (File.Exists(fullDestPath))
		{
			// they did not give us a file
			MessageBox.Show("You must specify a folder, not a file!");
			return;
		}

		if (!Directory.Exists(fullDestPath))
		{
			// see if they want to create it
			string q = string.Format("{0} does not exist.  Would you like to create it now?", fullDestPath);

			if (MessageBox.Show(q, "Create folder?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
				return;

			Directory.CreateDirectory(fullDestPath);
		}


		var form = Program.ServiceProvider.GetService<ExtractProgress>();
		form.BaseFolder = fullDestPath;
		form.ShowDialog();
	}

	#endregion

	#region About

	/// <summary>
	/// Handler for clicking Help->About
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
	{
		AboutBox d = new AboutBox();
		d.ShowDialog();
	}

	#endregion

	#region TreeViewTOC

	NodeTag SelectedTag => this.treeViewTOC.SelectedNode.Tag as NodeTag;

	/// <summary>
	/// Treeview node database
	/// </summary>
	Dictionary<RecordId, TreeNode> dicoNodes = new();

	/// <summary>
	/// Builds the tree view.  Assumes the list is pre-sorted.
	/// </summary>
	private void BuildTreeView()
	{
		// Display a wait cursor while the TreeNodes are being created.
		Cursor.Current = Cursors.WaitCursor;

		this.treeViewTOC.BeginUpdate();
		//this.treeViewTOC.Nodes.Clear();
		//this.dicoNodes.Clear();

		// Hold the nodes from the previous record.
		// We save these so we do not need to search the treeview

		RecordId[] dataRecords;

		if (this.SelectedFile.FileType == CompressedFileType.ArzFile)
			dataRecords = arzProv.GetKeyTable(this.SelectedFile.ARZFile);
		else if (this.SelectedFile.FileType == CompressedFileType.ArcFile)
			dataRecords = arcProv.GetKeyTable(this.SelectedFile.ARCFile);
		else
			return;

		// We failed so return.
		if (dataRecords == null)
			return;

		TreeNode rootNode = new(), arcRootNode = null;
		string arcPrefix = string.Empty;

		if (dicoNodes.Any())
			rootNode = dicoNodes[RecordId.Empty];// Get it back
		else
			dicoNodes.Add(RecordId.Empty, rootNode);// First time

		if (this.SelectedFile.FileType == CompressedFileType.ArcFile)
		{
			arcPrefix = Path.GetFileNameWithoutExtension(this.SelectedFile.sourceFile);
			var arcPrefixId = arcPrefix.ToRecordId();
			if (!dicoNodes.TryGetValue(arcPrefixId, out arcRootNode))
			{
				arcRootNode = new TreeNode()
				{
					Name = arcPrefix,
					Text = arcPrefix,
					ToolTipText = this.SelectedFile.sourceFile
				};
				arcRootNode.Tag = new NodeTag
				{
					thisNode = arcRootNode,
					File = this.SelectedFile,

					Thread = null,
					Key = arcPrefix,
					RecIdx = 0,
					TokIdx = 0,

					Text = arcPrefix,
				};
				dicoNodes.Add(arcPrefixId, arcRootNode);
				rootNode.Nodes.Add(arcRootNode);
			}
		}

		for (int recIdx = 0; recIdx < dataRecords.Length; recIdx++)
		{
			RecordId recordID = arcPrefix == string.Empty ? dataRecords[recIdx] : Path.Combine(arcPrefix, dataRecords[recIdx].Raw);

			for (int tokIdx = 0; tokIdx < recordID.TokensRaw.Count; tokIdx++)
			{
				// Arz resolving
				var token = recordID.TokensRaw[tokIdx];
				var parent = recordID.TokensRaw.Take(tokIdx).JoinString("\\").ToRecordId();
				var parentnode = dicoNodes[parent];
				var currnodeKey = (parent.IsEmpty ? token : parent + '\\' + token).ToRecordId();

				if (parentnode.Nodes.ContainsKey(currnodeKey))
					continue;
				else
				{
					var currentNode = new TreeNode()
					{
						Name = currnodeKey,
						Text = token,
						ToolTipText = this.SelectedFile.sourceFile
					};
					currentNode.Tag = new NodeTag
					{
						thisNode = currentNode,
						File = this.SelectedFile,

						Thread = recordID,
						Key = currnodeKey,
						RecIdx = recIdx,
						TokIdx = tokIdx,

						Text = token,
					};
					parentnode.Nodes.Add(currentNode);
					dicoNodes.Add(currnodeKey, currentNode);
				}
			}
		}

		// Always add the newcomers
		this.treeViewTOC.Nodes.Add(rootNode.Nodes[rootNode.Nodes.Count - 1]);

		// Reset the cursor to the default for all controls.
		Cursor.Current = Cursors.Default;

		this.treeViewTOC.EndUpdate();
	}
	/// <summary>
	/// Handler for clicking on a treeView item
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">TreeViewEventArgs data</param>
	private void TreeViewTOC_AfterSelect(object sender, TreeViewEventArgs e)
	{
		// Make sure we have selected the last child
		// otherwise this will be a directory.
		if (this.treeViewTOC.SelectedNode.GetNodeCount(false) == 0)
		{
			this.SelectedFile = this.SelectedTag.File;

			this.SelectedFile.destFile
				= this.textBoxPath.Text
				= this.treeViewTOC.SelectedNode.FullPath;

			try
			{
				List<string> recordText = new List<string>();
				if (this.SelectedFile.FileType == CompressedFileType.ArzFile)
				{
					this.SelectedFile.record = arzProv.GetRecordNotCached(this.SelectedFile.ARZFile, this.SelectedFile.destFile);
					foreach (Variable variable in this.SelectedFile.record)
					{
						if (variable.IsValueNonZero() || !hideZeroValuesToolStripMenuItem.Checked)
							recordText.Add(variable.ToString());
					}
					PopulateGridView(recordText);
					ShowGridView(recordText.Count);
					StackNavigation();
				}

				else if (this.SelectedFile.FileType == CompressedFileType.ArcFile)
				{
					string extension = Path.GetExtension(this.SelectedFile.destFile).ToUpper();
					string arcDataPath = this.SelectedFile.destFile;
					var arcDataRecordId = arcDataPath.ToRecordId();
					if (extension == ".TXT")
					{
						byte[] rawData = arcProv.GetData(this.SelectedFile.ARCFile, arcDataRecordId);

						if (rawData == null)
							return;

						// now read it like a text file
						using (StreamReader reader = new StreamReader(new MemoryStream(rawData), Encoding.Default))
						{
							string line;
							while ((line = reader.ReadLine()) != null)
							{
								recordText.Add(line);
							}
						}

						this.textBoxDetails.Lines = recordText.ToArray();
						ShowTextboxDetail(recordText.Count);
						StackNavigation();
					}
					else if (extension == ".TEX")
					{
						byte[] rawData = arcProv.GetData(this.SelectedFile.ARCFile, arcDataRecordId);

						if (rawData == null)
							return;

						Bitmap bitmap = BitmapService.LoadFromTexMemory(rawData, 0, rawData.Length);

						if (bitmap != null)
						{
							ShowPictureBox(bitmap);
							StackNavigation();
						}
					}
				}
				else
				{
					HideAllBox();
					this.SelectedFile.destFile = null;
					this.textBoxDetails.Lines = null;
				}
			}
			catch (Exception ex)
			{
				this.Log.ErrorException(ex);
			}
		}
		else
		{
			this.SelectedFile.destFile = null;
			this.textBoxDetails.Lines = null;
		}
	}

	#endregion

	#region Griv View

	static char[] PopulateGridView_splitChars = new[] { ',' };

	static DataGridViewCellStyle PopulateGridView_hyperLinkCellStyle = new DataGridViewCellStyle
	{
		Font = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Underline),
		ForeColor = Color.Blue,
	};

	private void PopulateGridView(List<string> recordText)
	{
		var grd = dataGridViewDetails;
		grd.Rows.Clear();

		// Init data
		foreach (string line in recordText)
		{
			string[] values = line.Split(PopulateGridView_splitChars, StringSplitOptions.RemoveEmptyEntries);
			grd.Rows.Add(values);
		}

		// Apply link style
		for (int i = 0; i < grd.RowCount; i++)
		{
			var cell = grd.Rows[i].Cells[1];
			var val = cell.Value?.ToString() ?? string.Empty;

			if (val.EndsWith(".DBR", noCase) || val.EndsWith(".TEX", noCase) || val.EndsWith(".TXT", noCase))
				cell.Style = PopulateGridView_hyperLinkCellStyle;
		}
	}

	private void hideZeroValuesToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (this.SelectedFile.record != null)
		{
			List<string> recordText = new List<string>();
			foreach (Variable variable in this.SelectedFile.record)
			{
				if (variable.IsValueNonZero() || !hideZeroValuesToolStripMenuItem.Checked)
					recordText.Add(variable.ToString());
			}

			PopulateGridView(recordText);
		}
	}

	#endregion

	#region Clipboard

	private void textBoxPath_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		Clipboard.SetText(this.textBoxPath.Text);
	}

	private void textBoxDetails_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		Clipboard.SetText(this.textBoxDetails.Text);
	}

	#endregion

	#region Caps Treeview

	private void toolStripButtonCaps_CheckedChanged(object sender, EventArgs e)
	{
		toolStripButtonCaps.Image = toolStripButtonCaps.Checked ? Resources.CapsDown.ToBitmap() : Resources.CapsUP.ToBitmap();

		this.treeViewTOC.BeginUpdate();
		foreach (var node in this.dicoNodes)
		{
			if (node.Key != string.Empty)
			{
				var tag = node.Value.Tag as NodeTag;
				tag.thisNode.Text = toolStripButtonCaps.Checked
					? tag.TextU
					: tag.Text;
			}
		}
		this.treeViewTOC.EndUpdate();
	}

	#endregion

	#region NavHistory

	private readonly List<TreeNode> NavHistory = new();
	private readonly AssemblyName assemblyName;
	private int NavHistoryIndex;

	bool _StackNavigationDisabled = false;

	private void StackNavigation()
	{
		if (_StackNavigationDisabled)
			return;

		NavHistory.Add(this.treeViewTOC.SelectedNode);
		NavHistoryIndex = NavHistory.Count - 1;
	}

	private void dataGridViewDetails_CellClick(object sender, DataGridViewCellEventArgs e)
	{
		var cell = dataGridViewDetails.Rows[e.RowIndex].Cells[e.ColumnIndex];
		if (cell.Style == PopulateGridView_hyperLinkCellStyle)
		{
			var value = cell.Value.ToString();
			var values = value.Split(';');
			NavigateTo(values.First());
		}
	}

	private void NavigateTo(RecordId recordId)
	{
		// Already loaded
		if (dicoNodes.TryGetValue(recordId, out var node))
		{
			this.treeViewTOC.SelectedNode = node;
			this.treeViewTOC.Focus();
		}
		else
		{
			// auto load file

			// Resolve file name
			string xPackVersion, fileToken, fileName, dicoKey;
			if (recordId.Normalized.StartsWith("XPACK"))
			{
				xPackVersion = recordId.TokensNormalized[0];
				fileToken = recordId.TokensNormalized[1];
				fileName = $"{fileToken}.ARC";
				dicoKey = @$"RESOURCES\{xPackVersion}\{fileName}";
			}
			else
			{
				xPackVersion = string.Empty;
				fileToken = recordId.TokensNormalized[0];
				fileName = $"{fileToken}.ARC";
				dicoKey = @$"RESOURCES\{fileName}";
			}

			if (ArcFileList.TryGetValue(dicoKey, out var fullpath))
			{
				// Add TOC to treeview
				this.OpenFile(fullpath);

				// Navigate
				var found = dicoNodes[recordId];
				this.treeViewTOC.SelectedNode = found;
				this.treeViewTOC.Focus();
			}
		}
	}
	private void toolStripButtonPrev_Click(object sender, EventArgs e)
	{
		_StackNavigationDisabled = true;
		if (NavHistory.Count > 0 && NavHistoryIndex > 0)
		{
			NavHistoryIndex--;
			NavHistoryGoto();
		}
		_StackNavigationDisabled = false;
	}

	private void NavHistoryGoto()
	{
		var tag = NavHistory[NavHistoryIndex].Tag as NodeTag;
		this.SelectedFile = tag.File;
		this.treeViewTOC.SelectedNode = NavHistory[NavHistoryIndex];
		this.treeViewTOC.Focus();
	}

	private void toolStripButtonNext_Click(object sender, EventArgs e)
	{
		_StackNavigationDisabled = true;
		if (NavHistoryIndex < NavHistory.Count - 1)
		{
			NavHistoryIndex++;

			NavHistoryGoto();
		}
		_StackNavigationDisabled = false;
	}

	private void toolStripButtonClearHistory_Click(object sender, EventArgs e)
	{
		this.NavHistory.Clear();
		this.NavHistoryIndex = -1;
	}

	#endregion

}

