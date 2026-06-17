//-----------------------------------------------------------------------
// <copyright file="Form1.cs" company="VillageIdiot">
//     Copyright (c) Village Idiot. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using ArzExplorer.Models;
using ArzExplorer.Properties;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Media;
using System.Reflection;
using System.Text;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Logs;
using TQVaultAE.Services.Win32;

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
	private readonly SoundServiceWin SoundService;

	internal TreeView TreeViewToc => treeViewTOC;

	/// <summary>
	/// Holds the initial size of the form.
	/// </summary>
	private Size initialSize;

	/// <summary>
	/// Search state - last search text
	/// </summary>
	private string lastSearchText = string.Empty;

	/// <summary>
	/// Search state - last search position
	/// </summary>
	private int lastSearchPosition = -1;

	/// <summary>
	/// Search state - last file type searched
	/// </summary>
	private CompressedFileType? lastFileType;

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
		, SoundServiceWin soundService
	)
	{
		this.InitializeComponent();

		this.Log = log;
		this.arcProv = arcFileProvider;
		this.arzProv = arzFileProvider;
		this.DBRecordCollectionProvider = dBRecordCollectionProvider;
		this.BitmapService = bitmapService;
		this.GamePathService = gamePathService;
		this.SoundService = soundService;
		Assembly a = Assembly.GetExecutingAssembly();
		this.assemblyName = a.GetName();

		this.selectedFileToolStripMenuItem.Enabled = false;
		this.allFilesToolStripMenuItem.Enabled = false;
		this.initialSize = this.Size;
		this.toolStripStatusLabel.Text = string.Empty;

		// Enable Search menu
		this.searchToolStripMenuItem.Visible = true;
	}

	private void MainForm_Load(object sender, EventArgs e)
	{
		// Seek for all arc files
		var arc = Directory.GetFiles(this.GamePathService.GamePathTQIT, "*.arc", SearchOption.AllDirectories);
		ArcFileList = arc.ToDictionary(
			k => k.Replace(this.GamePathService.GamePathTQIT + '\\', string.Empty).ToRecordId()
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
		StopSoundPlayer();

		this.simpleSoundPlayer.Visible =
		this.dataGridViewDetails.Visible =
		this.textBoxDetails.Visible =
		this.panelPicture.Visible = false;
		this.toolStripStatusLabel.Text = string.Empty;
	}

	private void ShowPictureBox(Image bitmap)
	{
		StopSoundPlayer();

		this.simpleSoundPlayer.Visible = false;
		this.dataGridViewDetails.Visible = false;
		this.textBoxDetails.Visible = false;
		this.panelPicture.Visible = true;
		this.pictureBoxItem.Image = bitmap;
		this.pictureBoxItem.Size = bitmap.Size;
		this.toolStripStatusLabel.Text = string.Format("PixelFormat : {0}, Size : {1}", bitmap.PixelFormat, bitmap.Size);
	}

	private void ShowSoundPlayer(RecordId soundId, SoundPlayer soundPlayer, byte[] soundWavData)
	{
		StopSoundPlayer();

		this.dataGridViewDetails.Visible = false;
		this.textBoxDetails.Visible = false;
		this.panelPicture.Visible = false;
		this.toolStripStatusLabel.Text = string.Format("Sound Name : {0}", Path.GetFileName(soundId.Raw));

		this.simpleSoundPlayer.Visible = true;
		this.simpleSoundPlayer.Dock = DockStyle.Fill;
		this.simpleSoundPlayer.CurrentSoundId = soundId;
		this.simpleSoundPlayer.CurrentSoundWavData = soundWavData;
		this.simpleSoundPlayer.CurrentSoundPlayer = soundPlayer;
	}

	private void ShowGridView(int Count)
	{
		StopSoundPlayer();

		this.dataGridViewDetails.Visible = true;
		this.dataGridViewDetails.Dock = DockStyle.Fill;
		this.simpleSoundPlayer.Visible = false;
		this.textBoxDetails.Visible = false;
		this.panelPicture.Visible = false;
		this.toolStripStatusLabel.Text = string.Format("Record Count : {0}", Count);
	}

	private void StopSoundPlayer()
	{
		if (this.simpleSoundPlayer.Visible)
			this.simpleSoundPlayer.CurrentSoundPlayer?.Stop();
	}

	private void ShowTextboxDetail(int Count)
	{
		StopSoundPlayer();

		this.simpleSoundPlayer.Visible = false;
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
	string DataBasePath => Path.Combine(this.GamePathService.GamePathTQIT, @"Database\database.arz");

	private readonly Dictionary<string, TQFileInfo> TQFileOpened = new();

	/// <summary>
	/// Found Arc Files
	/// </summary>
	Dictionary<RecordId, string> ArcFileList;

	private void toolStripButtonLoadDataBase_Click(object sender, EventArgs e)
	{
		try
		{
			using var scope = new TreeViewUpdateScope(this);
			this.OpenFile(this.DataBasePath);
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.Message);
			return;
		}
	}

	private void toolStripButtonLoadAllFiles_Click(object sender, EventArgs e)
	{
		LoadAllFilesToolStripMenuItem_Click(sender, e);
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
		if (this.TQFileOpened.Any(kv => kv.Value.SourceFile == filename))
			return;

		var fileInfo = new TQFileInfo();

		fileInfo.SourceFile = filename;

		if (string.IsNullOrEmpty(fileInfo.SourceFile))
		{
			MessageBox.Show("You must enter a valid source file path.");
			return;
		}

		// See if path exists and create it if necessary
		string fullSrcPath = Path.GetFullPath(fileInfo.SourceFile);

		if (!File.Exists(fullSrcPath))
		{
			// they did not give us a file
			MessageBox.Show("You must specify a file!");
			return;
		}

		// Try to read it as an ARC file since those have a header.
		fileInfo.ARCFile = new ArcFile(fileInfo.SourceFile);
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
			fileInfo.ARZFile = new ArzFile(fileInfo.SourceFile);
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
			MessageBox.Show(string.Format("Error Reading {0}", fileInfo.SourceFile));
			return;
		}

		this.selectedFileToolStripMenuItem.Enabled = true;
		this.allFilesToolStripMenuItem.Enabled = true;
		this.hideZeroValuesToolStripMenuItem.Enabled = fileInfo.FileType == CompressedFileType.ArzFile;

		UpdateTitle(fileInfo);

		this.textBoxDetails.Lines = null;

		this.toolStripStatusLabel.Text = string.Empty;

		this.TQFileOpened.Add(fullSrcPath, fileInfo);
		this.SelectedFile = fileInfo;

		this.BuildTreeView(fileInfo);
	}

	private void UpdateTitle(TQFileInfo fileInfo)
	{
		this.Text = string.Format("{0} - {1}", this.assemblyName.Name, fileInfo.SourceFile);
	}

	/// <summary>
	/// Handler for clicking Open on the menu.  Opens a file.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
	{
		using OpenFileDialog openDialog = new OpenFileDialog();
		openDialog.Filter = "Compressed TQ files (*.arz;*.arc)|*.arz;*.arc|All files (*.*)|*.*";
		openDialog.FilterIndex = 1;
		openDialog.RestoreDirectory = true;

		// Try to read the game path
		string startPath = this.GamePathService.GamePathTQIT;

		// If the registry fails then default to the save folder.
		if (startPath.Length < 1)
		{
			startPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games", "Titan Quest");
		}

		openDialog.InitialDirectory = startPath;
		DialogResult result = openDialog.ShowDialog();
		if (result == DialogResult.OK)
		{
			using var scope = new TreeViewUpdateScope(this);
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
				using var scope = new TreeViewUpdateScope(this);
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
		if (string.IsNullOrEmpty(this.SelectedFile.DestFile) || this.SelectedFile.FileType == CompressedFileType.Unknown)
			return;

		string filename = null;
		using SaveFileDialog saveFileDialog = new SaveFileDialog();

		saveFileDialog.Filter = "TQ files (*.txt;*.dbr;*.tex;*.msh;*.anm;*.fnt;*.qst;*.pfx;*.ssh)|*.txt;*.dbr;*.tex;*.msh;*.anm;*.fnt;*.qst;*.pfx;*.ssh|All files (*.*)|*.*";
		saveFileDialog.FilterIndex = 1;
		saveFileDialog.RestoreDirectory = true;
		saveFileDialog.Title = "Save the Titan Quest File";
		string startPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games", "Titan Quest");
		saveFileDialog.InitialDirectory = startPath;
		saveFileDialog.FileName = Path.GetFileName(this.SelectedFile.DestFile);

		if (saveFileDialog.ShowDialog() == DialogResult.OK)
		{
			filename = saveFileDialog.FileName;

			if (this.SelectedFile.FileType == CompressedFileType.ArzFile)
			{
				DBRecordCollectionProvider.Write(this.SelectedFile.Records, Path.GetDirectoryName(filename), Path.GetFileName(filename));
			}
			else if (this.SelectedFile.FileType == CompressedFileType.ArcFile)
			{
				arcProv.Write(
					this.SelectedFile.ARCFile
					, Path.GetDirectoryName(filename)
					, this.SelectedFile.DestFile
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

			this.SelectedFile.DestDirectory = browseDialog.SelectedPath;
		}

		string fullDestPath = null;

		if (string.IsNullOrEmpty(this.SelectedFile.DestDirectory))
		{
			MessageBox.Show("You must enter a valid destination folder.");
			return;
		}

		// See if path exists and create it if necessary
		if (!string.IsNullOrEmpty(this.SelectedFile.DestDirectory))
		{
			fullDestPath = Path.GetFullPath(this.SelectedFile.DestDirectory);
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
	internal Dictionary<string, TreeNode> dicoNodes = new();

	/// <summary>
	/// reverse directory of RecordId => TreeNode.Name
	/// </summary>
	internal Dictionary<RecordId, List<string>> dicoReverseKeys = new();

	/// <summary>
	/// Builds the tree view.  Assumes the list is pre-sorted.
	/// </summary>
	/// <param name="currentFileInfo"></param>
	private void BuildTreeView(TQFileInfo currentFileInfo)
	{

		List<RecordId> dataRecords;

		if (currentFileInfo.FileType == CompressedFileType.ArzFile)
			dataRecords = currentFileInfo.ARZFile.Keys.ToList();
		else if (currentFileInfo.FileType == CompressedFileType.ArcFile)
			dataRecords = currentFileInfo.ARCFile.Keys.ToList();
		else
			return;

		// We failed so return.
		if (dataRecords.Count == 0)
			return;

		// Track if this is the first file being loaded
		bool isFirstFile = dicoNodes.Count == 0;

		TreeNode arcRootNode = null, rootNode = new();

		string arcPrefix = string.Empty;
		var rootNodeKey = string.Empty;

		if (isFirstFile)
			dicoNodes.Add(rootNodeKey, rootNode);// First time
		else
			rootNode = dicoNodes[rootNodeKey];// Get it back

		if (currentFileInfo.FileType == CompressedFileType.ArcFile)
		{
			var tokens = currentFileInfo.SourceFileId.TokensRaw;

			// Node Xpack
			var arcPrefixXpack = tokens[tokens.Count - 2];
			if (!arcPrefixXpack.StartsWith("XPACK", StringComparison.OrdinalIgnoreCase))
				arcPrefixXpack = string.Empty;

			if (arcPrefixXpack != string.Empty)
				GetRootNode(currentFileInfo, arcPrefixXpack, rootNode, out arcRootNode);

			// Node File
			arcPrefix = Path.GetFileNameWithoutExtension(tokens[tokens.Count - 1]);
			if (arcPrefixXpack == string.Empty)
				GetRootNode(currentFileInfo, arcPrefix, rootNode, out arcRootNode);
			else
			{
				arcPrefix = arcPrefixXpack + '\\' + arcPrefix;
				GetRootNode(currentFileInfo, arcPrefix, arcRootNode, out arcRootNode);
			}
		}

		foreach (var record in dataRecords)
		{
			RecordId recordID = arcPrefix == string.Empty ? record : Path.Combine(arcPrefix, record.Raw);

			for (int tokIdx = 0; tokIdx < recordID.TokensRaw.Count; tokIdx++)
			{
				var token = recordID.TokensRaw[tokIdx];
				var parent = recordID.TokensRaw.Take(tokIdx).JoinString("\\").ToRecordId();
				var parentKey = $"{currentFileInfo.SourceFileId};{parent}";
				var parentnode = dicoNodes.TryGetValue(parentKey, out var node) ? node : rootNode;
				var currnodeKey = (parent.IsEmpty ? token : parent + '\\' + token).ToRecordId();
				var nodeKey = $"{currentFileInfo.SourceFileId};{currnodeKey}";

				if (parentnode.Nodes.ContainsKey(nodeKey)) continue;
				else
				{
					var currentNode = new TreeNode()
					{
						Name = $"{currentFileInfo.SourceFileId};{currnodeKey}",
						Text = parent.IsEmpty
							? $"{currentFileInfo.SourceFileId.TokensNormalized.Last()};{currnodeKey.Raw}"
							: $"{currnodeKey.Raw}",
						ToolTipText = currentFileInfo.SourceFile
					};
					currentNode.Tag = new NodeTag
					{
						thisNode = currentNode,
						File = currentFileInfo,

						Thread = recordID,
						Key = currnodeKey,
						DictionaryKey = nodeKey,
						TokIdx = tokIdx,

						Text = currentNode.Text,
					};
					parentnode.Nodes.Add(currentNode);

					dicoNodes.Add(nodeKey, currentNode);

					if (dicoReverseKeys.TryGetValue(currnodeKey, out var reversekeys)) reversekeys.Add(currentNode.Name);
					else dicoReverseKeys.Add(currnodeKey, [currentNode.Name]);
				}
			}
		}
	}

	void GetRootNode(TQFileInfo currentFileInfo, string arcPrefix, TreeNode rootNode, out TreeNode arcRootNode)
	{
		var arcPrefixId = arcPrefix.ToRecordId();
		var nodeKey = $"{currentFileInfo.SourceFileId};{arcPrefixId}";
		if (!dicoNodes.TryGetValue(nodeKey, out arcRootNode))
		{
			arcRootNode = new TreeNode()
			{
				Name = $"{currentFileInfo.SourceFileId};{arcPrefixId}",
				Text = arcPrefixId.TokensNormalized.Count == 1
					? $"{currentFileInfo.SourceFileId.TokensNormalized.Last()};{arcPrefix}"
					: $"{arcPrefix}",
				ToolTipText = currentFileInfo.SourceFile
			};
			arcRootNode.Tag = new NodeTag
			{
				thisNode = arcRootNode,
				File = currentFileInfo,

				Thread = null,
				Key = arcPrefixId,
				DictionaryKey = nodeKey,
				TokIdx = 0,

				Text = arcRootNode.Text,
			};
			dicoNodes.Add(nodeKey, arcRootNode);

			if (dicoReverseKeys.TryGetValue(arcPrefixId, out var reversekeys)) reversekeys.Add(arcRootNode.Name);
			else dicoReverseKeys.Add(arcPrefixId, [arcRootNode.Name]);

			rootNode.Nodes.Add(arcRootNode);
		}
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
			var tag = this.SelectedTag;

			this.SelectedFile = tag.File;

			UpdateTitle(this.SelectedFile);

			this.SelectedFile.DestFile
				= this.textBoxPath.Text
				= this.treeViewTOC.SelectedNode.Name.Split(';').Last();

			try
			{
				List<string> recordText = new List<string>();
				if (this.SelectedFile.FileType == CompressedFileType.ArzFile)
				{
					this.SelectedFile.Records = arzProv.GetRecordNotCached(this.SelectedFile.ARZFile, this.SelectedFile.DestFile);
					foreach (Variable variable in this.SelectedFile.Records)
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
					string extension = Path.GetExtension(tag.Key.TokensNormalized.Last());
					var arcDataRecordId = tag.Key;

					if (tag.Key.Normalized.Contains("XPACK"))
						arcDataRecordId = tag.Key.TokensRaw.Skip(1).JoinString("\\").ToRecordId();

					switch (extension)
					{
						case ".TXT":
							if (!tag.RecordText.Any())
							{
								tag.RawData = arcProv.GetData(this.SelectedFile.ARCFile, arcDataRecordId);

								if (tag.RawData == null)
									return;

								// now read it like a text file
								using (StreamReader reader = new StreamReader(new MemoryStream(tag.RawData), Encoding.Default))
								{
									string line;
									while ((line = reader.ReadLine()) != null)
									{
										tag.RecordText.Add(line);
									}
								}
							}

							this.textBoxDetails.Lines = tag.RecordText.ToArray();
							ShowTextboxDetail(tag.RecordText.Count);
							StackNavigation();
							break;
						case ".TEX":
							if (tag.Bitmap is null)
							{
								tag.RawData = arcProv.GetData(this.SelectedFile.ARCFile, arcDataRecordId);

								if (tag.RawData == null)
									return;

								tag.Bitmap = BitmapService.LoadFromTexMemory(tag.RawData, 0, tag.RawData.Length);
							}

							if (tag.Bitmap != null)
							{
								ShowPictureBox(tag.Bitmap);
								StackNavigation();
							}
							break;
						case ".MP3":
						case ".WAV":
							if (tag.SoundPlayer is null)
							{
								tag.RawData = arcProv.GetData(this.SelectedFile.ARCFile, arcDataRecordId);

								if (tag.RawData == null)
									return;

								SoundService.SetSoundResource(arcDataRecordId, tag.RawData);
								tag.SoundPlayer = SoundService.GetSoundPlayer(arcDataRecordId);
							}

							// Display SoundPlayer
							if (tag.SoundPlayer != null)
							{
								ShowSoundPlayer(
									arcDataRecordId
									, tag.SoundPlayer
									, SoundService.GetSoundResource(arcDataRecordId)
								);
								StackNavigation();
							}
							break;
						default:
							HideAllBox();
							this.SelectedFile.DestFile = null;
							this.textBoxDetails.Lines = null;
							break;
					}
				}
			}
			catch (Exception ex)
			{
				this.Log.ErrorException(ex);
			}
		}
		else
		{
			this.SelectedFile.DestFile = null;
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

		// Apply link style and adjust hyperlink
		for (int i = 0; i < grd.RowCount; i++)
		{
			var cell = grd.Rows[i].Cells[1];
			var val = cell.Value?.ToString() ?? string.Empty;

			if (val.EndsWith(".DBR", noCase)
				|| val.EndsWith(".TEX", noCase)
				|| val.EndsWith(".TXT", noCase)
				|| val.EndsWith(".WAV", noCase)
				|| val.EndsWith(".MP3", noCase)
			   )
			{
				cell.Style = PopulateGridView_hyperLinkCellStyle;
			}
		}
	}

	private void hideZeroValuesToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (this.SelectedFile.Records != null)
		{
			List<string> recordText = new List<string>();
			foreach (Variable variable in this.SelectedFile.Records)
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
		CopyPath();
	}

	private void CopyPath()
	{
		Clipboard.SetText(this.textBoxPath.Text);
	}

	private void textBoxDetails_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		CopyTXT();
	}

	private void CopyTXT()
	{
		Clipboard.SetText(this.textBoxDetails.Text);
	}

	private void copyPathToolStripMenuItem_Click(object sender, EventArgs e)
	{
		CopyPath();
	}

	private void copyTXTToolStripMenuItem_Click(object sender, EventArgs e)
	{
		CopyTXT();
	}

	private void copyDBRToolStripMenuItem_Click(object sender, EventArgs e)
	{
		// DataGridView into Clipboard
		DataGridViewCell currCell = null;
		if (this.dataGridViewDetails.SelectedCells.Count > 0)
			currCell = this.dataGridViewDetails.SelectedCells[0];

		this.dataGridViewDetails.MultiSelect = true;

		// Select all the cells
		this.dataGridViewDetails.SelectAll();

		// Copy selected cells to DataObject
		DataObject dataObject = this.dataGridViewDetails.GetClipboardContent();

		// Get the text of the DataObject, and serialize it to a file
		Clipboard.SetDataObject(dataObject);

		// Restore normal state
		this.dataGridViewDetails.MultiSelect = false;

		if (currCell is not null)
			currCell.Selected = true;
	}

	private void copyBitmapToolStripMenuItem_Click(object sender, EventArgs e)
	{
		Clipboard.SetImage(this.pictureBoxItem.Image);
	}

	private void copySoundToolStripMenuItem_Click(object sender, EventArgs e)
	{
		Clipboard.SetAudio(this.simpleSoundPlayer.CurrentSoundWavData);
	}

	#endregion

	#region Caps Treeview

	private void toolStripButtonCaps_CheckedChanged(object sender, EventArgs e)
	{
		TOCCapsToggle();
	}
	private void capsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		toolStripButtonCaps.Checked = !toolStripButtonCaps.Checked;
	}

	private void TOCCapsToggle()
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
		// IsHeader
		if (e.RowIndex < 0 || e.ColumnIndex < 0)
			return;

		var cell = dataGridViewDetails.Rows[e.RowIndex].Cells[e.ColumnIndex];
		if (cell.Style == PopulateGridView_hyperLinkCellStyle)
		{
			var value = cell.Value.ToString();
			NavigateTo(value);
		}
	}

	private void NavigateTo(string dictionaryKey)
	{
		var dictionaryKeyId = dictionaryKey.ToRecordId();

		#region Already loaded

		if (dicoNodes.TryGetValue(dictionaryKey, out var node))
		{
			SetFocus(node);
			return;
		}

		if (dicoReverseKeys.TryGetValue(dictionaryKey, out var list))
		{
			var firstnode = dicoNodes[list.First()];
			SetFocus(firstnode);// Best effort
			return;
		}

		#endregion

		// Try to extract RecordId after the semicolon
		var parts = dictionaryKey.Split(';', 2);

		RecordId recordId = parts.Length == 2 && !string.IsNullOrEmpty(parts[1])
			? parts[1].ToRecordId()
			: dictionaryKeyId;// Could not parse, try using the whole string as RecordId


		// DBR references are usually prefixed by RECORDS
		if (recordId.Normalized.EndsWith(".DBR") && !recordId.Normalized.StartsWith("RECORDS"))
		{
			var guessingId = RecordId.Create(Path.Combine("records", recordId.Raw));
			if (dicoReverseKeys.TryGetValue(guessingId, out var guessinglist))
			{
				var firstnode = dicoNodes[guessinglist.First()];
				SetFocus(firstnode);
				return;
			}
		}

		#region Auto load file

		// Resolve file name
		string xPackVersion, fileToken, fileName;
		RecordId dicoKey;
		if (recordId.Normalized.StartsWith("XPACK"))
		{
			xPackVersion = recordId.TokensNormalized[0];
			fileToken = recordId.TokensNormalized[1];
			fileName = $"{fileToken}.ARC";
			dicoKey = @$"RESOURCES\{xPackVersion}\{fileName}";
		}
		else if (recordId.Normalized.EndsWith(".MP3") || recordId.Normalized.EndsWith(".WAV"))
		{
			xPackVersion = string.Empty;
			fileToken = recordId.TokensNormalized[0];
			fileName = $"{fileToken}.ARC";
			dicoKey = @$"AUDIO\{fileName}";
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
			using (var scope = new TreeViewUpdateScope(this))
			{
				this.OpenFile(fullpath);
			}

			// Navigate - use the original dictionaryKey for lookup
			if (dicoNodes.TryGetValue(dictionaryKeyId.Normalized, out var autoLoadFound))
			{
				SetFocus(autoLoadFound);
				return;
			}

			if (dicoReverseKeys.TryGetValue(dictionaryKeyId, out var autoLoadList))
			{
				var firstnode = dicoNodes[autoLoadList.First()];
				SetFocus(firstnode);
				return;
			}

			// Database orphan ? - extract RecordId from node if available
			this.toolStripStatusLabel.Text = @$"Unable to find ""{dictionaryKeyId}""";
		}

		#endregion


		void SetFocus(TreeNode? focusnode)
		{
			this.treeViewTOC.SelectedNode = focusnode;
			this.treeViewTOC.Focus();
		}
	}

	private void previousToolStripMenuItem_Click(object sender, EventArgs e)
	{
		GoPrev();
	}

	private void nextToolStripMenuItem_Click(object sender, EventArgs e)
	{
		GoNext();
	}

	private void toolStripButtonPrev_Click(object sender, EventArgs e)
	{
		GoPrev();
	}

	private void GoPrev()
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
		GoNext();
	}

	private void GoNext()
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

	#region Search

	/// <summary>
	/// Find next occurrence of search text
	/// </summary>
	private void FindNext()
	{
		// Validate search text
		var searchText = this.toolStripTextBox.Text;
		if (string.IsNullOrEmpty(searchText))
		{
			this.toolStripStatusLabel.Text = "empty text !";
			return;
		}

		// Check if this is a new search
		bool isNewSearch = this.lastSearchText != searchText;
		if (isNewSearch)
		{
			this.lastSearchText = searchText;
			this.lastSearchPosition = -1;
		}

		int startPos = this.lastSearchPosition + 1;
		int count = this.dicoNodes.Count;

		// First pass: search from lastSearchPosition + 1 to end
		for (int i = startPos; i < count; i++)
		{
			var kvp = this.dicoNodes.ElementAt(i);
			var node = kvp.Value;

			// Skip root nodes (key is string.Empty)
			if (kvp.Key == string.Empty)
				continue;

			if (SearchNode(node, searchText, i))
				return;
		}

		// Second pass: search from beginning to lastSearchPosition (wrap around)
		for (int i = 0; i < startPos; i++)
		{
			var kvp = this.dicoNodes.ElementAt(i);
			var node = kvp.Value;

			// Skip root nodes (key is string.Empty)
			if (kvp.Key == string.Empty)
				continue;

			if (SearchNode(node, searchText, i))
				return;
		}

		// Not found - show in status label and reset position
		this.toolStripStatusLabel.Text = $"Not found: {searchText}";
		this.lastSearchPosition = -1;
	}

	/// <summary>
	/// Search a single node for the search text
	/// </summary>
	private bool SearchNode(TreeNode node, string searchText, int index)
	{
		// Extract key from node.Tag
		if (node.Tag is not NodeTag tag)
			return false;

		var key = tag.Key;

		// Search in record key/name
		if (key.Normalized.Contains(searchText, noCase))
		{
			this.lastSearchPosition = index;
			this.lastFileType = null;
			NavigateTo(node.Name);
			this.toolStripStatusLabel.Text = $"Found at: {key}";
			return true;
		}

		// Search in tag.Text
		if (tag.Text.Contains(searchText, noCase))
		{
			this.lastSearchPosition = index;
			this.lastFileType = null;
			NavigateTo(node.Name);
			this.toolStripStatusLabel.Text = $"Found in: {key}";
			return true;
		}

		// Search in tag.RecordText (List<string>)
		if (tag.RecordText.Any(line => line.Contains(searchText, noCase)))
		{
			this.lastSearchPosition = index;
			this.lastFileType = null;
			NavigateTo(node.Name);
			this.toolStripStatusLabel.Text = $"Found in: {key}";
			return true;
		}

		// Search in record variables (ARZ files only)
		var file = tag.File;
		if (file?.FileType == CompressedFileType.ArzFile && file.ARZFile != null)
		{
			try
			{
				var records = this.arzProv.GetRecordNotCached(file.ARZFile, key.Normalized);
				if (records != null)
				{
					foreach (Variable variable in records)
					{
						if (variable.ToString().Contains(searchText, noCase))
						{
							this.lastSearchPosition = index;
							this.lastFileType = CompressedFileType.ArzFile;
							NavigateTo(node.Name);
							this.toolStripStatusLabel.Text = $"Found in: {key}";
							return true;
						}
					}
				}
			}
			catch (KeyNotFoundException)
			{
				// Record not found in ARZ file, skip
			}
		}

		return false;
	}

	/// <summary>
	/// Find previous occurrence of search text
	/// </summary>
	private void FindPrevious()
	{
		// Validate search text
		var searchText = this.toolStripTextBox.Text;
		if (string.IsNullOrEmpty(searchText))
		{
			this.toolStripStatusLabel.Text = "empty text !";
			return;
		}

		// Check if this is a new search
		bool isNewSearch = this.lastSearchText != searchText;
		if (isNewSearch)
		{
			this.lastSearchText = searchText;
			this.lastSearchPosition = this.dicoNodes.Count;
		}

		int startPos = this.lastSearchPosition - 1;
		int count = this.dicoNodes.Count;

		// First pass: search from lastSearchPosition - 1 down to 0
		for (int i = startPos; i >= 0; i--)
		{
			var kvp = this.dicoNodes.ElementAt(i);
			var node = kvp.Value;

			// Skip root nodes (key is string.Empty)
			if (kvp.Key == string.Empty)
				continue;

			if (SearchNode(node, searchText, i))
				return;
		}

		// Second pass: search from Count - 1 down to lastSearchPosition (wrap around)
		for (int i = count - 1; i > startPos; i--)
		{
			var kvp = this.dicoNodes.ElementAt(i);
			var node = kvp.Value;

			// Skip root nodes (key is string.Empty)
			if (kvp.Key == string.Empty)
				continue;

			if (SearchNode(node, searchText, i))
				return;
		}

		// Not found - show in status label and reset position
		this.toolStripStatusLabel.Text = $"Not found: {searchText}";
		this.lastSearchPosition = this.dicoNodes.Count;
	}

	/// <summary>
	/// Process keyboard commands (F3 for Find Next, Shift+F3 for Find Previous)
	/// </summary>
	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if (keyData == Keys.F3)
		{
			FindNext();
			return true;
		}
		else if (keyData == (Keys.Shift | Keys.F3))
		{
			FindPrevious();
			return true;
		}

		return base.ProcessCmdKey(ref msg, keyData);
	}

	private void toolStripButtonSearchPrev_Click(object sender, EventArgs e)
	{
		FindPrevious();
	}

	private void toolStripButtonSearchNext_Click(object sender, EventArgs e)
	{
		FindNext();
	}

	private void searchNextToolStripMenuItem_Click(object sender, EventArgs e)
	{
		FindNext();
	}

	private void findPreviousToolStripMenuItem_Click(object sender, EventArgs e)
	{
		FindPrevious();
	}

	private void buttonFindNext_Click(object sender, EventArgs e)
	{
		FindNext();
	}

	#endregion

	#region Load All Files

	/// <summary>
	/// Load all ARZ and ARC files from the game directory
	/// </summary>
	private void LoadAllFilesToolStripMenuItem_Click(object sender, EventArgs e)
	{
		try
		{
			var gamePath = this.GamePathService.GamePathTQIT;
			if (string.IsNullOrEmpty(gamePath))
			{
				MessageBox.Show("Game path not found.");
				return;
			}

			// Scan for ARZ and ARC files
			var arzFiles = Directory.GetFiles(gamePath, "*.arz", SearchOption.AllDirectories);
			var arcFiles = Directory.GetFiles(gamePath, "*.arc", SearchOption.AllDirectories);
			var allFiles = arzFiles.Concat(arcFiles).OrderBy(f => f).ToArray();

			int loaded = 0;
			int skipped = 0;
			int failed = 0;

			using var scope = new TreeViewUpdateScope(this);
			foreach (var file in allFiles)
			{
				// Check if already loaded
				if (this.TQFileOpened.Any(kv => kv.Value.SourceFile == file))
				{
					skipped++;
					continue;
				}

				// Try to load the file - catch errors for individual files
				try
				{
					this.OpenFile(file);
					loaded++;
				}
				catch (Exception ex)
				{
					failed++;
					this.Log.LogWarning(ex, "Failed to load file: {File}", file);
				}
			}

			this.toolStripStatusLabel.Text = $"Loaded: {loaded}, Skipped: {skipped}, Failed: {failed}";
		}
		catch (Exception ex)
		{
			MessageBox.Show($"Error loading files: {ex.Message}");
		}
	}

	#endregion

}

