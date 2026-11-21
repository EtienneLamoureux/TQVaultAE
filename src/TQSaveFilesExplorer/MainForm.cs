using TQ.SaveFilesExplorer.Components;
using TQ.SaveFilesExplorer.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TQ.SaveFilesExplorer.Services;

namespace TQ.SaveFilesExplorer;

public partial class MainForm : Form
{
	internal static MainForm? StaticRef { get; private set; }

	private readonly SaveFolderDetector saveFolders;

	public MainForm(SaveFolderDetector saveFolders)
	{
		InitializeComponent();
		StaticRef = this;
		this.saveFolders = saveFolders;
	}

	private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
	{
		var about = new AboutBox();
		about.ShowDialog();
	}

	private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (string.IsNullOrWhiteSpace(this.folderBrowserDialogMain.SelectedPath)) this.folderBrowserDialogMain.SelectedPath = this.saveFolders.DefaultSaveDirectory;
		var result = this.folderBrowserDialogMain.ShowDialog();
		if (result == DialogResult.OK)
		{
			LoadFilesFromDirectory(this.folderBrowserDialogMain.SelectedPath);
		}
	}

	private void LoadFilesFromDirectory(string path)
	{
		var tqfiles = Directory.GetFiles(path).Where(TQFile.IsSaveFile).ToArray();

		foreach (var file in tqfiles)
		{
			if (IsFileAlreadyOpened(file)) continue;
			AddFilePage(file);
		}
	}

	private void OpenFileToolStripMenuItem_Click(object sender, EventArgs e)
	{
		if (string.IsNullOrWhiteSpace(this.openFileDialogMain.InitialDirectory)) this.openFileDialogMain.InitialDirectory = this.saveFolders.DefaultSaveDirectory;
		DialogResult result = this.openFileDialogMain.ShowDialog();
		if (result == DialogResult.OK)
		{
			var tqfiles = this.openFileDialogMain.FileNames.Where(TQFile.IsSaveFile).ToArray();

			foreach (var file in tqfiles)
			{
				if (IsFileAlreadyOpened(file)) continue;
				AddFilePage(file);
			}
		}
	}

	private bool IsFileAlreadyOpened(string file)
	{
		var alreadyopend = this.tabControlFiles.TabPages.Cast<TabPage>().FirstOrDefault(p => ((string?)p.Tag) == file);
		if (alreadyopend is not null)
		{
			SetStatusMessage($@"""{alreadyopend.Text}"" file is already opened !");
		}
		;
		return alreadyopend is not null;
	}

	internal static void SetStatusMessage(string message)
	{
		var form = StaticRef;
		if (form is null) return;

		form.toolStripStatusLabelMessage.Visible = true;
		form.toolStripStatusLabelMessage.Text = message;
	}

	private void MainForm_Load(object sender, EventArgs e)
	{
		// Init
		this.tabControlFiles.TabPages.Clear();

		DetectLocalSaves();
	}


	private void toolStripMenuItem_DetectedPlayers_Click(object? sender, EventArgs e)
	{
		// Every detected player item holds its directory in Tag
		if (sender is ToolStripMenuItem { Tag: string path }) LoadFilesFromDirectory(path);
	}


	/// <summary>
	/// Init "Detected Players" menu items
	/// </summary>
	private void DetectLocalSaves()
	{
		// Characters, transfer stash, custom map transfer stashes, custom map characters: a separator between groups.
		List<ToolStripItem> items = [];
		var groups = this.saveFolders.Detect();
		for (var i = 0; i < groups.Count; i++)
		{
			if (i > 0) items.Add(new ToolStripSeparator());

			foreach (var folder in groups[i])
			{
				var itm = new ToolStripMenuItem(folder.Label) { Tag = folder.Path, };
				itm.Click += new System.EventHandler(this.toolStripMenuItem_DetectedPlayers_Click);
				items.Add(itm);
			}
		}

		toolStripMenuItem_DetectedPlayers.DropDownItems.AddRange(items.ToArray());
	}

	private void AddFilePage(string path)
	{
		var tabTxt = $"{Path.GetFileName(Path.GetDirectoryName(path))} : {Path.GetFileName(path)}";
		var page = new System.Windows.Forms.TabPage()
		{
			Location = this.tabPageTemplate.Location,
			Padding = this.tabPageTemplate.Padding,
			Size = this.tabPageTemplate.Size,
			Text = tabTxt,
			UseVisualStyleBackColor = this.tabPageTemplate.UseVisualStyleBackColor,
			Tag = path,
		};
		var content = new TabPageFileContent()
		{
			Tag = path,
			Dock = DockStyle.Fill,
		};
		page.Controls.Add(content);
		this.tabControlFiles.Controls.Add(page);
	}

	private void CloseTabToolStripMenuItem_Click(object sender, EventArgs e)
	{
		var selectedTab = this.tabControlFiles.SelectedTab;
		if (selectedTab is null) return;

		this.tabControlFiles.TabPages.Remove(selectedTab);
	}
}
