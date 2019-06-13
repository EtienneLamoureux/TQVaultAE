using TQ.SaveFilesExplorer.Components;
using TQ.SaveFilesExplorer.Entities;
using TQ.SaveFilesExplorer.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TQ.SaveFilesExplorer.Helpers;

namespace TQ.SaveFilesExplorer
{
	public partial class MainForm : Form
	{
		internal static MainForm StaticRef { get; private set; } = null;

		public MainForm()
		{
			InitializeComponent();
			StaticRef = this;
		}

		private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var about = new AboutBox();
			about.ShowDialog();
		}

		private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(this.folderBrowserDialogMain.SelectedPath)) this.folderBrowserDialogMain.SelectedPath = TQPath.DefaultSaveDirectory;
			var result = this.folderBrowserDialogMain.ShowDialog();
			if (result == DialogResult.OK)
			{
				LoadFilesFromDirectory(this.folderBrowserDialogMain.SelectedPath);
			}
		}

		private void LoadFilesFromDirectory(string path)
		{
			var tqfiles = Directory.GetFiles(path).Where(p => TQFile.Ext_All.Contains(Path.GetExtension(p).ToLower())).ToArray();

			foreach (var file in tqfiles)
			{
				if (IsFileAlreadyOpened(file)) continue;
				AddFilePage(file);
			}
		}

		private void OpenFileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(this.openFileDialogMain.InitialDirectory)) this.openFileDialogMain.InitialDirectory = TQPath.DefaultSaveDirectory;
			DialogResult result = this.openFileDialogMain.ShowDialog();
			if (result == DialogResult.OK)
			{
				var tqfiles = this.openFileDialogMain.FileNames.Where(p => TQFile.Ext_All.Contains(Path.GetExtension(p))).ToArray();

				foreach (var file in tqfiles)
				{
					if (IsFileAlreadyOpened(file)) continue;
					AddFilePage(file);
				}
			}
		}

		private bool IsFileAlreadyOpened(string file)
		{
			var alreadyopend = this.tabControlFiles.TabPages.Cast<TabPage>().FirstOrDefault(p => ((string)p.Tag) == file);
			if (alreadyopend != null)
			{
				SetStatusMessage($@"""{alreadyopend.Text}"" file is already opened !");
			};
			return alreadyopend != null;
		}

		internal static void SetStatusMessage(string message)
		{
			StaticRef.toolStripStatusLabelMessage.Visible = true;
			StaticRef.toolStripStatusLabelMessage.Text = message;
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			// Init
			this.tabControlFiles.TabPages.Clear();

			DetectLocalSaves();

#if DEBUG
			//var path = @"C:\Users\hguyau\Documents\My Games\Titan Quest - Immortal Throne\SaveData\Main\_Davina\Player.chr";
			//var path = @"C:\Users\Hervé\Documents\my games\Titan Quest Backup\FirstRun\SaveData\Main\_yujiro\winsys.dxb";
			//var path = @"C:\Users\Hervé\Documents\my games\Titan Quest Backup\FirstRun\SaveData\Main\_yujiro\Player.chr";
			//AddFilePage(path);
#endif
		}


		private void toolStripMenuItem_DetectedPlayers_Click(object sender, EventArgs e)
		{
			var item = sender as ToolStripMenuItem;
			LoadFilesFromDirectory((string)item.Tag);
		}


		/// <summary>
		/// Init "Detected Players" menu items
		/// </summary>
		private void DetectLocalSaves()
		{
			// Detecting local players saves 
			List<ToolStripItem> items = new string[] { TQPath.SaveDirectoryTQIT, TQPath.SaveDirectoryTQ }
				.Where(p => !string.IsNullOrWhiteSpace(p))
				.SelectMany(p => Directory.GetDirectories(p))
				.Select(d =>
				{
					var itm = new ToolStripMenuItem($"{(d.StartsWith(TQPath.SaveDirectoryTQIT) ? "TQIT" : "TQ")} : {Path.GetFileName(d)}") { Tag = d, };
					itm.Click += new System.EventHandler(this.toolStripMenuItem_DetectedPlayers_Click);
					return itm;
				}
			).Cast<ToolStripItem>().ToList();

			items.Add(new ToolStripSeparator());

			// Add transfer stash
			var ts = TQPath.SaveDirectoryTQITTransferStash;
			if (ts != null)
			{
				var itm = new ToolStripMenuItem("TQIT : Transfer Stash") { Tag = ts, };
				itm.Click += new System.EventHandler(this.toolStripMenuItem_DetectedPlayers_Click);
				items.Add(itm);
			}

			items.Add(new ToolStripSeparator());

			// Add modded transfer stash
			var mod = TQPath.SaveDirectoryTQITModdedTransferStash;
			foreach (var m in mod)
			{
				var itm = new ToolStripMenuItem($"{Path.GetFileName(m)} : Transfer Stash") { Tag = m, };
				itm.Click += new System.EventHandler(this.toolStripMenuItem_DetectedPlayers_Click);
				items.Add(itm);
			}

			items.Add(new ToolStripSeparator());

			var modplayers = TQPath.SaveDirectoryTQITModdedPlayers;
			foreach (var m in modplayers)
			{
				var itm = new ToolStripMenuItem($"Mod : {Path.GetFileName(m)}") { Tag = m, };
				itm.Click += new System.EventHandler(this.toolStripMenuItem_DetectedPlayers_Click);
				items.Add(itm);
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
			var menuitem = sender as ToolStripMenuItem;
			this.tabControlFiles.TabPages.Remove(this.tabControlFiles.SelectedTab);
		}
	}
}
