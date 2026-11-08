//-----------------------------------------------------------------------
// <copyright file="MainForm.cs" company="VillageIdiot">
//     Copyright (c) Village Idiot. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using ArzExplorer.Models;
using ArzExplorer.Properties;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Media;
using System.Reflection;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GameFiles;
using TQVaultAE.Presentation.Win32.Contracts;
using TQVaultAE.Services.Win32;

namespace ArzExplorer;

/// <summary>
/// Main window: the tree of the opened ARZ and ARC files and the view of the selected record or entry.
/// </summary>
public partial class MainForm : Form
{
	private const StringComparison NoCase = StringComparison.OrdinalIgnoreCase;

	private readonly ILogger Log;
	private readonly IArcFileProvider ArcProvider;
	private readonly IArzFileProvider ArzProvider;
	private readonly ArchiveExtractor Extractor;
	private readonly IBitmapService BitmapService;
	private readonly IGamePathService GamePathService;
	private readonly SoundServiceWin SoundService;
	private readonly AssemblyName _assemblyName;

	internal TreeView TreeViewToc => this.treeViewTOC;

	/// <summary>
	/// Search state - last search text
	/// </summary>
	private string _lastSearchText = string.Empty;

	/// <summary>
	/// Search state - last search position
	/// </summary>
	private int _lastSearchPosition = -1;

	#region MainForm

	/// <summary>
	/// Initializes a new instance of the MainForm class.
	/// </summary>
	public MainForm(
		ILogger<MainForm> log
		, IArcFileProvider arcFileProvider
		, IArzFileProvider arzFileProvider
		, ArchiveExtractor extractor
		, IBitmapService bitmapService
		, IGamePathService gamePathService
		, SoundServiceWin soundService
	)
	{
		this.InitializeComponent();

		this.Log = log;
		this.ArcProvider = arcFileProvider;
		this.ArzProvider = arzFileProvider;
		this.Extractor = extractor;
		this.BitmapService = bitmapService;
		this.GamePathService = gamePathService;
		this.SoundService = soundService;
		this._assemblyName = Assembly.GetExecutingAssembly().GetName();

		this.selectedFileToolStripMenuItem.Enabled = false;
		this.allFilesToolStripMenuItem.Enabled = false;
		this.toolStripStatusLabel.Text = string.Empty;

		// Enable Search menu
		this.searchToolStripMenuItem.Visible = true;
	}

	private void MainForm_Load(object sender, EventArgs e)
	{
		// Seek for all arc files
		var arc = Directory.GetFiles(this.GamePathService.GamePathTQIT, "*.arc", SearchOption.AllDirectories);
		this._arcFiles = arc.ToDictionary(
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

	/// <summary>
	/// Shows one detail view (or none) and hides the others, stopping the sound being played.
	/// </summary>
	/// <param name="view">The view to show, or null to hide them all.</param>
	/// <param name="status">Text of the status bar.</param>
	private void ShowView(Control? view, string status)
	{
		this.StopSoundPlayer();

		this.simpleSoundPlayer.Visible = view == this.simpleSoundPlayer;
		this.dataGridViewDetails.Visible = view == this.dataGridViewDetails;
		this.textBoxDetails.Visible = view == this.textBoxDetails;
		this.panelPicture.Visible = view == this.panelPicture;
		this.toolStripStatusLabel.Text = status;
	}

	private void HideAllBox() => this.ShowView(null, string.Empty);

	private void ShowPictureBox(Image bitmap)
	{
		this.ShowView(this.panelPicture, $"PixelFormat : {bitmap.PixelFormat}, Size : {bitmap.Size}");
		this.pictureBoxItem.Image = bitmap;
		this.pictureBoxItem.Size = bitmap.Size;
	}

	private void ShowSoundPlayer(RecordId soundId, SoundPlayer soundPlayer, byte[]? soundWavData)
	{
		this.ShowView(this.simpleSoundPlayer, $"Sound Name : {Path.GetFileName(soundId.Raw)}");

		this.simpleSoundPlayer.Dock = DockStyle.Fill;
		this.simpleSoundPlayer.CurrentSoundId = soundId;
		this.simpleSoundPlayer.CurrentSoundWavData = soundWavData;
		this.simpleSoundPlayer.CurrentSoundPlayer = soundPlayer;
	}

	private void ShowGridView(int count)
	{
		this.ShowView(this.dataGridViewDetails, $"Record Count : {count}");
		this.dataGridViewDetails.Dock = DockStyle.Fill;
	}

	private void StopSoundPlayer()
	{
		if (this.simpleSoundPlayer.Visible)
			this.simpleSoundPlayer.CurrentSoundPlayer?.Stop();
	}

	private void ShowTextboxDetail(int count)
	{
		this.ShowView(this.textBoxDetails, $"Line Count : {count}");
		this.textBoxDetails.Dock = DockStyle.Fill;
	}

	#endregion

	#region Open File

	internal TQFileInfo SelectedFile = new();

	/// <summary>
	/// Resolve database file
	/// </summary>
	private string DataBasePath => Path.Combine(this.GamePathService.GamePathTQIT, "Database", "database.arz");

	/// <summary>
	/// Folder the file dialogs start in: the game's save folder.
	/// </summary>
	private static string SaveFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games", "Titan Quest");

	/// <summary>
	/// Opened files, by full path, whatever its case: the game folder from the registry (<c>e:\games\...</c>) and the
	/// path of the Open dialog (<c>E:\GAMES\...</c>) name the same file.
	/// </summary>
	private readonly Dictionary<string, TQFileInfo> _openedFiles = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// ARC files of the game folder, by path relative to it.
	/// </summary>
	private Dictionary<RecordId, string> _arcFiles = [];

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
		}
	}

	/// <summary>
	/// Whether <paramref name="filename"/> is already open, however its path is written ('/' or '\', any case):
	/// opening it twice would add its entries to the tree again.
	/// </summary>
	private bool IsOpen(string filename)
		=> this._openedFiles.ContainsKey(Path.GetFullPath(filename));

	/// <summary>
	/// Opens a file and updates the tree view.
	/// </summary>
	/// <param name="filename">Name of the file we want to open.</param>
	/// <param name="quiet">Report an unreadable or empty file in the return value only, without a message box (Load All Files).</param>
	/// <returns>Whether the file is open (or was already).</returns>
	private bool OpenFile(string filename, bool quiet = false)
	{
		if (string.IsNullOrEmpty(filename))
			return false;

		// Already loaded
		if (this.IsOpen(filename))
			return true;

		var fileInfo = new TQFileInfo { SourceFile = filename };

		string fullSrcPath = Path.GetFullPath(fileInfo.SourceFile);

		if (!File.Exists(fullSrcPath))
		{
			// they did not give us a file
			if (!quiet)
				MessageBox.Show("You must specify a file!");
			return false;
		}

		// Try to read it as an ARC file since those have a header.
		fileInfo.ARCFile = new ArcFile(fileInfo.SourceFile);
		if (this.ArcProvider.Read(fileInfo.ARCFile))
		{
			fileInfo.FileType = CompressedFileType.ArcFile;
		}
		else
		{
			fileInfo.ARCFile = null;
			fileInfo.FileType = CompressedFileType.Unknown;

			// An ARC without entries (the game ships XPack4\Underground.arc empty) is not an ARZ either.
			if (HasArcHeader(fullSrcPath))
			{
				this.toolStripStatusLabel.Text = $"{Path.GetFileName(fullSrcPath)} contains no files.";
				if (!quiet)
					MessageBox.Show($"{fullSrcPath} contains no files.");
				return false;
			}
		}

		// Try reading the file as an ARZ file.
		if (fileInfo.FileType == CompressedFileType.Unknown)
		{
			// Read our ARZ file into memory.
			fileInfo.ARZFile = new ArzFile(fileInfo.SourceFile);
			if (this.ArzProvider.Read(fileInfo.ARZFile))
			{
				fileInfo.FileType = CompressedFileType.ArzFile;
			}
			else
			{
				fileInfo.ARZFile = null;
				fileInfo.FileType = CompressedFileType.Unknown;
			}
		}

		// We failed reading the file: it added nothing, the files already open stay as they are.
		if (fileInfo.FileType == CompressedFileType.Unknown)
		{
			if (!quiet)
				MessageBox.Show($"Error Reading {fileInfo.SourceFile}");
			return false;
		}

		this.selectedFileToolStripMenuItem.Enabled = true;
		this.allFilesToolStripMenuItem.Enabled = true;
		this.hideZeroValuesToolStripMenuItem.Enabled = fileInfo.FileType == CompressedFileType.ArzFile;

		this.UpdateTitle(fileInfo);

		this.textBoxDetails.Lines = null;

		this.toolStripStatusLabel.Text = string.Empty;

		this._openedFiles.Add(fullSrcPath, fileInfo);
		this.SelectedFile = fileInfo;

		this.BuildTreeView(fileInfo);
		return true;
	}

	/// <summary>Whether the file starts with the "ARC" magic of an ARC archive.</summary>
	internal static bool HasArcHeader(string path)
	{
		try
		{
			using var stream = File.OpenRead(path);
			Span<byte> magic = stackalloc byte[4];
			return stream.ReadAtLeast(magic, magic.Length, throwOnEndOfStream: false) == magic.Length && magic.SequenceEqual("ARC\0"u8);
		}
		catch (IOException)
		{
			return false;
		}
	}

	private void UpdateTitle(TQFileInfo fileInfo)
	{
		this.Text = $"{this._assemblyName.Name} - {fileInfo.SourceFile}";
	}

	/// <summary>
	/// Handler for clicking Open on the menu.  Opens a file.
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
	{
		// The game folder, else the save folder.
		string startPath = this.GamePathService.GamePathTQIT;
		if (startPath.Length == 0)
			startPath = SaveFolder;

		using var openDialog = new OpenFileDialog
		{
			Filter = "Compressed TQ files (*.arz;*.arc)|*.arz;*.arc|All files (*.*)|*.*",
			FilterIndex = 1,
			RestoreDirectory = true,
			InitialDirectory = startPath,
		};

		if (openDialog.ShowDialog() == DialogResult.OK)
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
		// Only the first file when the user dropped several.
		if (e.Data?.GetData(DataFormats.FileDrop) is string[] { Length: > 0 } files)
		{
			try
			{
				using var scope = new TreeViewUpdateScope(this);
				this.OpenFile(files[0]);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
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
		e.Effect = e.Data?.GetDataPresent(DataFormats.FileDrop) == true ? DragDropEffects.Copy : DragDropEffects.None;
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

		using var saveFileDialog = new SaveFileDialog
		{
			Filter = "TQ files (*.txt;*.dbr;*.tex;*.msh;*.anm;*.fnt;*.qst;*.pfx;*.ssh)|*.txt;*.dbr;*.tex;*.msh;*.anm;*.fnt;*.qst;*.pfx;*.ssh|All files (*.*)|*.*",
			FilterIndex = 1,
			RestoreDirectory = true,
			Title = "Save the Titan Quest File",
			InitialDirectory = SaveFolder,
			FileName = Path.GetFileName(this.SelectedFile.DestFile),
		};

		if (saveFileDialog.ShowDialog() == DialogResult.OK)
			this.ExtractSelectedTo(saveFileDialog.FileName);
	}

	/// <summary>Writes the selected record or entry to <paramref name="filename"/>.</summary>
	internal void ExtractSelectedTo(string filename)
	{
		if (string.IsNullOrEmpty(this.SelectedFile.DestFile) || this.SelectedFile.FileType == CompressedFileType.Unknown)
			return;

		this.Extractor.ExtractOne(this.SelectedFile, this.SelectedFile.DestFile, filename);
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

		if ((this.SelectedFile.FileType == CompressedFileType.ArzFile && this.SelectedFile.ARZFile is null)
			|| (this.SelectedFile.FileType == CompressedFileType.ArcFile && this.SelectedFile.ARCFile is null)
		)
		{
			MessageBox.Show("Please Open a source file.");
			return;
		}

		using var browseDialog = new FolderBrowserDialog
		{
			Description = "Select the destination folder for the extracted database records",
			ShowNewFolderButton = true,
			SelectedPath = SaveFolder,
		};

		if (browseDialog.ShowDialog() != DialogResult.OK)
			return;

		this.ExtractAllTo(browseDialog.SelectedPath);
	}

	/// <summary>
	/// Writes every record or entry of the selected file under <paramref name="folder"/>, with the progress dialog;
	/// asks before creating a missing folder.
	/// </summary>
	internal void ExtractAllTo(string folder)
	{
		this.SelectedFile.DestDirectory = folder;
		if (string.IsNullOrEmpty(folder))
		{
			MessageBox.Show("You must enter a valid destination folder.");
			return;
		}

		var fullDestPath = Path.GetFullPath(folder);

		if (File.Exists(fullDestPath))
		{
			// they did not give us a file
			MessageBox.Show("You must specify a folder, not a file!");
			return;
		}

		if (!Directory.Exists(fullDestPath))
		{
			// see if they want to create it
			var question = $"{fullDestPath} does not exist.  Would you like to create it now?";

			if (MessageBox.Show(question, "Create folder?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
				return;

			Directory.CreateDirectory(fullDestPath);
		}

		using var form = Program.ServiceProvider.GetRequiredService<ExtractProgress>();
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
		using var about = new AboutBox();
		about.ShowDialog();
	}

	#endregion

	#region TreeViewTOC

	/// <summary>
	/// Every tree node by key (<see cref="NodeKey"/>); the root node, which holds the top nodes, has the empty key.
	/// </summary>
	internal Dictionary<string, TreeNode> NodesByKey = [];

	/// <summary>
	/// Keys of the tree nodes of a record or entry id, one per file that holds it.
	/// </summary>
	internal Dictionary<RecordId, List<string>> NodeKeysByRecordId = [];

	/// <summary>Key of a tree node: the id of its file, then its record or entry id, separated by ';'.</summary>
	private static string NodeKey(TQFileInfo file, RecordId id) => $"{file.SourceFileId};{id}";

	/// <summary>The record or entry id of a tree node key (<see cref="NodeKey"/>): what follows the last ';'.</summary>
	internal static string EntryOf(string nodeKey) => nodeKey[(nodeKey.LastIndexOf(';') + 1)..];

	/// <summary>
	/// Adds the nodes of the records or entries of a newly opened file to the tree.
	/// </summary>
	private void BuildTreeView(TQFileInfo file)
	{
		List<RecordId> dataRecords = file.FileType switch
		{
			CompressedFileType.ArzFile => [.. file.Arz.Keys],
			CompressedFileType.ArcFile => [.. file.Arc.Keys],
			_ => [],
		};

		// We failed so return.
		if (dataRecords.Count == 0)
			return;

		// The first file creates the root node, the others add to it.
		if (!this.NodesByKey.TryGetValue(string.Empty, out var rootNode))
		{
			rootNode = new TreeNode();
			this.NodesByKey.Add(string.Empty, rootNode);
		}

		string arcPrefix = string.Empty;
		if (file.FileType == CompressedFileType.ArcFile)
		{
			// Node of the expansion folder, then node of the file
			arcPrefix = ArcPaths.TreePrefix(file.SourceFileId);
			var fileParentNode = rootNode;
			var separator = arcPrefix.IndexOf('\\');
			if (separator >= 0)
				fileParentNode = this.GetArcFolderNode(file, arcPrefix[..separator], rootNode);

			this.GetArcFolderNode(file, arcPrefix, fileParentNode);
		}

		foreach (var record in dataRecords)
		{
			RecordId recordId = arcPrefix.Length == 0 ? record : Path.Combine(arcPrefix, record.Raw);
			var tokens = recordId.TokensRaw;

			for (int tokIdx = 0; tokIdx < tokens.Count; tokIdx++)
			{
				var token = tokens[tokIdx];
				var parent = tokens.Take(tokIdx).JoinString("\\").ToRecordId();
				var parentNode = this.NodesByKey.TryGetValue(NodeKey(file, parent), out var node) ? node : rootNode;
				var key = (parent.IsEmpty ? token : parent + '\\' + token).ToRecordId();
				var nodeKey = NodeKey(file, key);

				if (parentNode.Nodes.ContainsKey(nodeKey))
					continue;

				var text = parent.IsEmpty
					? $"{file.SourceFileId.TokensNormalized.Last()};{key.Raw}"
					: key.Raw;
				this.AddNode(parentNode, file, nodeKey, key, text);
			}
		}
	}

	/// <summary>
	/// The node of an ARC file or of its expansion folder, created under <paramref name="parent"/> the first time.
	/// </summary>
	private TreeNode GetArcFolderNode(TQFileInfo file, string arcPrefix, TreeNode parent)
	{
		var key = arcPrefix.ToRecordId();
		var nodeKey = NodeKey(file, key);
		if (this.NodesByKey.TryGetValue(nodeKey, out var existingNode))
			return existingNode;

		var text = key.TokensNormalized.Count == 1
			? $"{file.SourceFileId.TokensNormalized.Last()};{arcPrefix}"
			: arcPrefix;
		return this.AddNode(parent, file, nodeKey, key, text);
	}

	/// <summary>
	/// Creates a tagged node under <paramref name="parent"/> and indexes it by key and by record id.
	/// </summary>
	private TreeNode AddNode(TreeNode parent, TQFileInfo file, string nodeKey, RecordId key, string text)
	{
		var node = new TreeNode
		{
			Name = nodeKey,
			Text = text,
			ToolTipText = file.SourceFile,
		};
		node.Tag = new NodeTag
		{
			Node = node,
			File = file,
			Key = key,
			Text = text,
		};
		parent.Nodes.Add(node);

		this.NodesByKey.Add(nodeKey, node);

		if (this.NodeKeysByRecordId.TryGetValue(key, out var nodeKeys))
			nodeKeys.Add(nodeKey);
		else
			this.NodeKeysByRecordId.Add(key, [nodeKey]);

		return node;
	}

	/// <summary>
	/// Handler for clicking on a treeView item
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">TreeViewEventArgs data</param>
	private void TreeViewTOC_AfterSelect(object sender, TreeViewEventArgs e)
	{
		// Only a leaf shows something: a node with children is a directory.
		if (this.treeViewTOC.SelectedNode is not { } selectedNode || selectedNode.GetNodeCount(false) != 0)
		{
			this.SelectedFile.DestFile = null;
			this.textBoxDetails.Lines = null;
			return;
		}

		if (selectedNode.Tag is not NodeTag tag)
			return;

		this.SelectedFile = tag.File;

		this.UpdateTitle(this.SelectedFile);

		this.SelectedFile.DestFile
			= this.textBoxPath.Text
			= EntryOf(selectedNode.Name);

		try
		{
			if (this.SelectedFile.FileType == CompressedFileType.ArzFile)
			{
				this.SelectedFile.Records = this.ArzProvider.GetRecordNotCached(this.SelectedFile.Arz, this.SelectedFile.DestFile);
				var recordText = this.RecordLines(this.SelectedFile.Records);
				this.PopulateGridView(recordText);
				this.ShowGridView(recordText.Count);
				this.StackNavigation();
			}
			else if (this.SelectedFile.FileType == CompressedFileType.ArcFile)
			{
				this.ShowArcEntry(tag);
			}
		}
		catch (Exception ex)
		{
			this.Log.LogError(ex, "Failed to show the entry {Entry} of {File}", this.SelectedFile.DestFile, this.SelectedFile.SourceFile);
		}
	}

	/// <summary>
	/// Shows an ARC entry in the view of its type, reading it the first time; the node keeps what it read.
	/// </summary>
	private void ShowArcEntry(NodeTag tag)
	{
		string extension = Path.GetExtension(tag.Key.TokensNormalized.Last());
		var arcDataRecordId = ArcPaths.DataId(tag.Key);

		switch (extension)
		{
			case ".TXT":
				if (tag.RecordText.Count == 0)
				{
					if (this.ArcProvider.GetData(this.SelectedFile.Arc, arcDataRecordId) is not { } rawText)
						return;

					// UTF-16 with a BOM, or cp1252 without one, like the game.
					foreach (var line in TextDatabaseFactory.DecodeText(rawText).AsSpan().EnumerateLines())
						tag.RecordText.Add(line.ToString());
				}

				this.textBoxDetails.Lines = [.. tag.RecordText];
				this.ShowTextboxDetail(tag.RecordText.Count);
				this.StackNavigation();
				break;
			case ".TEX":
				if (tag.Bitmap is null)
				{
					if (this.ArcProvider.GetData(this.SelectedFile.Arc, arcDataRecordId) is not { } rawTexture)
						return;

					tag.Bitmap = this.BitmapService.LoadFromTexMemory(rawTexture, 0, rawTexture.Length);
				}

				if (tag.Bitmap is not null)
				{
					this.ShowPictureBox(tag.Bitmap);
					this.StackNavigation();
				}
				break;
			case ".MP3":
			case ".WAV":
				if (tag.SoundPlayer is null)
				{
					if (this.ArcProvider.GetData(this.SelectedFile.Arc, arcDataRecordId) is not { } rawSound)
						return;

					this.SoundService.SetSoundResource(arcDataRecordId, rawSound);
					tag.SoundPlayer = this.SoundService.GetSoundPlayer(arcDataRecordId);
				}

				// Display SoundPlayer
				if (tag.SoundPlayer is not null)
				{
					this.ShowSoundPlayer(
						arcDataRecordId
						, tag.SoundPlayer
						, this.SoundService.GetSoundResource(arcDataRecordId)
					);
					this.StackNavigation();
				}
				break;
			default:
				// No viewer for this type, but Extract Selected File still writes it
				this.HideAllBox();
				this.textBoxDetails.Lines = null;
				break;
		}
	}

	#endregion

	#region Grid View

	private static readonly DataGridViewCellStyle LinkCellStyle = new()
	{
		Font = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Underline),
		ForeColor = Color.Blue,
	};

	/// <summary>
	/// The lines of a record shown in the grid: every variable, or only the non-zero ones with Hide Zero Values.
	/// </summary>
	private List<string> RecordLines(DBRecordCollection records)
	{
		var hideZeroValues = this.hideZeroValuesToolStripMenuItem.Checked;
		List<string> recordText = [];
		foreach (Variable variable in records)
		{
			if (variable.IsValueNonZero() || !hideZeroValues)
				recordText.Add(variable.ToString());
		}

		return recordText;
	}

	/// <summary>
	/// Splits a record line into its grid cells: the variable name, then its values joined by ';'.
	/// </summary>
	internal static string[] GridCells(string line) => line.Split(',', StringSplitOptions.RemoveEmptyEntries);

	private void PopulateGridView(List<string> recordText)
	{
		var grid = this.dataGridViewDetails;
		grid.Rows.Clear();

		foreach (string line in recordText)
		{
			string[] values = GridCells(line);

			// A variable holding several links (loot tables, sound lists) gets one row per value, so each one can be
			// clicked: joined with ';' in one cell, a click could only reach one of them.
			if (values.Length > 1 && values[1].Contains(';'))
			{
				var parts = values[1].Split(';');
				if (parts.Any(IsGridLink))
				{
					for (int i = 0; i < parts.Length; i++)
						grid.Rows.Add(i == 0 ? values[0] : string.Empty, parts[i]);
					continue;
				}
			}

			grid.Rows.Add(values);
		}

		// Apply link style and adjust hyperlink
		for (int i = 0; i < grid.RowCount; i++)
		{
			var cell = grid.Rows[i].Cells[1];
			if (IsGridLink(cell.Value?.ToString() ?? string.Empty))
				cell.Style = LinkCellStyle;
		}
	}

	/// <summary>Whether a record value names a record or a resource the grid can navigate to.</summary>
	internal static bool IsGridLink(string value)
		=> value.EndsWith(".DBR", NoCase)
			|| value.EndsWith(".TEX", NoCase)
			|| value.EndsWith(".TXT", NoCase)
			|| value.EndsWith(".WAV", NoCase)
			|| value.EndsWith(".MP3", NoCase);

	private void hideZeroValuesToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (this.SelectedFile.Records is null)
			return;

		var recordText = this.RecordLines(this.SelectedFile.Records);
		this.PopulateGridView(recordText);

		// The count of the record shown; another view (picture, text) stays as it is.
		if (this.dataGridViewDetails.Visible)
			this.ShowGridView(recordText.Count);
	}

	#endregion

	#region Clipboard

	private void textBoxPath_MouseDoubleClick(object sender, MouseEventArgs e) => this.CopyPath();

	private void copyPathToolStripMenuItem_Click(object sender, EventArgs e) => this.CopyPath();

	private void CopyPath() => Clipboard.SetText(this.textBoxPath.Text);

	private void textBoxDetails_MouseDoubleClick(object sender, MouseEventArgs e) => this.CopyText();

	private void copyTXTToolStripMenuItem_Click(object sender, EventArgs e) => this.CopyText();

	private void CopyText() => Clipboard.SetText(this.textBoxDetails.Text);

	private void copyDBRToolStripMenuItem_Click(object sender, EventArgs e)
	{
		// DataGridView into Clipboard
		DataGridViewCell? currCell = null;
		if (this.dataGridViewDetails.SelectedCells.Count > 0)
			currCell = this.dataGridViewDetails.SelectedCells[0];

		this.dataGridViewDetails.MultiSelect = true;

		// Select all the cells
		this.dataGridViewDetails.SelectAll();

		// Copy selected cells to DataObject
		DataObject? dataObject = this.dataGridViewDetails.GetClipboardContent();

		// Get the text of the DataObject, and serialize it to a file
		if (dataObject is not null)
			Clipboard.SetDataObject(dataObject);

		// Restore normal state
		this.dataGridViewDetails.MultiSelect = false;

		if (currCell is not null)
			currCell.Selected = true;
	}

	private void copyBitmapToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (this.pictureBoxItem.Image is not null)
			Clipboard.SetImage(this.pictureBoxItem.Image);
	}

	private void copySoundToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (this.simpleSoundPlayer.CurrentSoundWavData is not null)
			Clipboard.SetAudio(this.simpleSoundPlayer.CurrentSoundWavData);
	}

	#endregion

	#region Caps Treeview

	private void toolStripButtonCaps_CheckedChanged(object sender, EventArgs e) => this.ToggleTocCaps();

	private void capsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		this.toolStripButtonCaps.Checked = !this.toolStripButtonCaps.Checked;
	}

	private void ToggleTocCaps()
	{
		var caps = this.toolStripButtonCaps.Checked;
		this.toolStripButtonCaps.Image = caps ? Resources.CapsDown.ToBitmap() : Resources.CapsUP.ToBitmap();

		this.treeViewTOC.BeginUpdate();
		foreach (var (key, node) in this.NodesByKey)
		{
			// The root node (empty key) has no tag, every other node holds its NodeTag
			if (key.Length != 0 && node.Tag is NodeTag tag)
				tag.Node.Text = caps ? tag.TextU : tag.Text;
		}
		this.treeViewTOC.EndUpdate();
	}

	#endregion

	#region NavHistory

	private readonly List<TreeNode> _navHistory = [];
	private int _navHistoryIndex;
	private bool _stackNavigationDisabled;

	private void StackNavigation()
	{
		if (this._stackNavigationDisabled)
			return;

		if (this.treeViewTOC.SelectedNode is not null)
			this._navHistory.Add(this.treeViewTOC.SelectedNode);
		this._navHistoryIndex = this._navHistory.Count - 1;
	}

	private void dataGridViewDetails_CellClick(object sender, DataGridViewCellEventArgs e)
	{
		// IsHeader
		if (e.RowIndex < 0 || e.ColumnIndex < 0)
			return;

		var cell = this.dataGridViewDetails.Rows[e.RowIndex].Cells[e.ColumnIndex];
		if (cell.Style == LinkCellStyle)
			this.NavigateTo(cell.Value?.ToString() ?? string.Empty);
	}

	private void NavigateTo(string dictionaryKey)
	{
		var node = this.ResolveLink(dictionaryKey, openArchives: true);
		if (node is null)
		{
			// Not in the game files: the database references a record or resource that does not exist.
			this.toolStripStatusLabel.Text = @$"Unable to find ""{dictionaryKey}""";
			return;
		}

		this.treeViewTOC.SelectedNode = node;
		this.treeViewTOC.Focus();
	}

	/// <summary>
	/// Finds the node of a record or resource a record value references: among the loaded files, else in the archives
	/// the game searches for it (<see cref="GameArchives.ArcFileCandidates"/>), opened when <paramref name="openArchives"/>.
	/// </summary>
	/// <param name="value">A tree key (<c>file;key</c>) or a value as the database writes it.</param>
	/// <returns>The node, or null when the game files do not hold it.</returns>
	internal TreeNode? ResolveLink(string value, bool openArchives)
	{
		// A few records write their paths with '/' (Sounds/monsters/...): the tree keys use '\'.
		value = value.Replace('/', '\\');

		if (this.NodesByKey.TryGetValue(value, out var node))
			return node;

		var valueId = value.ToRecordId();
		if (this.NodeKeysByRecordId.TryGetValue(valueId, out var list))
			return this.NodesByKey[list[0]];

		// A tree key: the record id after the semicolon.
		var parts = value.Split(';', 2);
		RecordId recordId = parts.Length == 2 && !string.IsNullOrEmpty(parts[1]) ? parts[1].ToRecordId() : valueId;
		if (recordId.IsEmpty)
			return null;

		// DBR references are usually prefixed by RECORDS
		if (recordId.Normalized.EndsWith(".DBR", StringComparison.Ordinal) && !recordId.Normalized.StartsWith("RECORDS", StringComparison.Ordinal)
			&& this.NodeKeysByRecordId.TryGetValue(RecordId.Create(Path.Combine("records", recordId.Raw)), out var guessed))
			return this.NodesByKey[guessed[0]];

		// The archive the resource maps to, then the archives of the same name the game also searches.
		foreach (var arcFile in GameArchives.ArcFileCandidates(recordId))
		{
			if (!this._arcFiles.TryGetValue(arcFile, out var fullpath))
				continue;

			if (openArchives)
			{
				using var scope = new TreeViewUpdateScope(this);
				this.OpenFile(fullpath);
			}

			if (this.NodeKeysByRecordId.TryGetValue(valueId, out var found))
				return this.NodesByKey[found[0]];

			// The same entry in a fallback archive, under that archive's tree prefix: the game finds
			// XPack3\Items\x.tex in XPack2\Items.arc when only that one holds it.
			var fileId = fullpath.ToRecordId();
			var entryKey = RecordId.Create(ArcPaths.TreePrefix(fileId) + '\\' + ArcPaths.EntryId(recordId).Raw);
			if (this.NodeKeysByRecordId.TryGetValue(entryKey, out var inArchive))
			{
				var name = inArchive.FirstOrDefault(n => n.StartsWith(fileId.Normalized + ';', StringComparison.Ordinal)) ?? inArchive[0];
				return this.NodesByKey[name];
			}
		}

		return null;
	}

	private void GoPrevious_Click(object sender, EventArgs e) => this.GoPrevious();

	private void GoNext_Click(object sender, EventArgs e) => this.GoNext();

	private void GoPrevious()
	{
		if (this._navHistory.Count > 0 && this._navHistoryIndex > 0)
		{
			this._navHistoryIndex--;
			this.NavHistoryGoto();
		}
	}

	private void GoNext()
	{
		if (this._navHistoryIndex < this._navHistory.Count - 1)
		{
			this._navHistoryIndex++;
			this.NavHistoryGoto();
		}
	}

	/// <summary>
	/// Selects the history entry at the current index, without stacking it again.
	/// </summary>
	private void NavHistoryGoto()
	{
		// Only tagged nodes are shown in the tree, so only they get into the history
		if (this._navHistory[this._navHistoryIndex].Tag is not NodeTag tag)
			return;

		this._stackNavigationDisabled = true;
		try
		{
			this.SelectedFile = tag.File;
			this.treeViewTOC.SelectedNode = this._navHistory[this._navHistoryIndex];
			this.treeViewTOC.Focus();
		}
		finally
		{
			this._stackNavigationDisabled = false;
		}
	}

	private void toolStripButtonClearHistory_Click(object sender, EventArgs e)
	{
		this._navHistory.Clear();
		this._navHistoryIndex = -1;
	}

	#endregion

	#region Search

	private void FindNext_Click(object sender, EventArgs e) => this.Find(forward: true);

	private void FindPrevious_Click(object sender, EventArgs e) => this.Find(forward: false);

	/// <summary>
	/// Find the next or previous node matching the search text, wrapping around
	/// </summary>
	private void Find(bool forward)
	{
		// Validate search text
		var searchText = this.toolStripTextBox.Text;
		if (string.IsNullOrEmpty(searchText))
		{
			this.toolStripStatusLabel.Text = "empty text !";
			return;
		}

		// Snapshot the nodes once: ElementAt on the dictionary walks it from the start every time
		var nodes = this.NodesByKey.ToArray();
		int startFromEnd = forward ? -1 : nodes.Length;

		// Check if this is a new search
		if (this._lastSearchText != searchText)
		{
			this._lastSearchText = searchText;
			this._lastSearchPosition = startFromEnd;
		}

		// The first match in search order, looked for a block of nodes at a time, each block checked in parallel:
		// checking the records one by one took seconds on the UI thread (12 s for a text found nowhere).
		var order = SearchOrder.Indices(nodes.Length, this._lastSearchPosition, forward).ToArray();
		const int blockSize = 1024;
		for (int start = 0; start < order.Length; start += blockSize)
		{
			var block = order.AsSpan(start, Math.Min(blockSize, order.Length - start)).ToArray();
			var matches = new string?[block.Length];
			Parallel.For(0, block.Length, k => matches[k] = this.SearchMatch(nodes[block[k]], searchText));

			int first = Array.FindIndex(matches, match => match is not null);
			if (first < 0)
				continue;

			var (key, node) = nodes[block[first]];
			this._lastSearchPosition = block[first];
			this.NavigateTo(key);
			this.toolStripStatusLabel.Text = $"{matches[first]}: {(node.Tag as NodeTag)?.Key}";
			return;
		}

		// Not found - show in status label and reset position
		this.toolStripStatusLabel.Text = $"Not found: {searchText}";
		this._lastSearchPosition = startFromEnd;
	}

	/// <summary>
	/// Whether a node holds the search text: "Found at" when its key does, "Found in" when its text, the text shown for
	/// it or its record values do; null otherwise. Reads only, so the nodes can be checked in parallel.
	/// </summary>
	private string? SearchMatch(KeyValuePair<string, TreeNode> node, string searchText)
	{
		// Skip the root node (key is string.Empty)
		if (node.Key.Length == 0 || node.Value.Tag is not NodeTag tag)
			return null;

		var key = tag.Key;
		if (key.Normalized.Contains(searchText, NoCase))
			return "Found at";

		if (tag.Text.Contains(searchText, NoCase) || tag.RecordText.Any(line => line.Contains(searchText, NoCase)))
			return "Found in";

		// Search in record variables (ARZ files only)
		var file = tag.File;
		if (file.FileType != CompressedFileType.ArzFile || file.ARZFile is null)
			return null;

		try
		{
			foreach (Variable variable in this.ArzProvider.GetRecordNotCached(file.ARZFile, key.Normalized))
			{
				if (variable.TextContains(searchText))
					return "Found in";
			}
		}
		catch (KeyNotFoundException)
		{
			// Record not found in ARZ file, skip
		}

		return null;
	}

	/// <summary>
	/// Process keyboard commands (F3 for Find Next, Shift+F3 for Find Previous)
	/// </summary>
	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		switch (keyData)
		{
			case Keys.F3:
				this.Find(forward: true);
				return true;
			case Keys.Shift | Keys.F3:
				this.Find(forward: false);
				return true;
			default:
				return base.ProcessCmdKey(ref msg, keyData);
		}
	}

	#endregion

	#region Load All Files

	/// <summary>
	/// Load all ARZ and ARC files from the game directory
	/// </summary>
	private void LoadAllFiles_Click(object sender, EventArgs e)
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
				if (this.IsOpen(file))
				{
					skipped++;
					continue;
				}

				// Try to load the file - catch errors for individual files
				try
				{
					// Quiet: one message box per unreadable or empty file would stop the loop each time.
					if (this.OpenFile(file, quiet: true))
					{
						loaded++;
					}
					else
					{
						failed++;
						this.Log.LogWarning("Failed to load file: {File}", file);
					}
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

