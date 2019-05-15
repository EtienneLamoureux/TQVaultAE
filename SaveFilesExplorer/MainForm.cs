using SaveFilesExplorer.Components;
using SaveFilesExplorer.Entities;
using SaveFilesExplorer.Services;
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

namespace SaveFilesExplorer
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
		}

		private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var about = new AboutBox();
			about.ShowDialog();
		}

		private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.folderBrowserDialogMain.SelectedPath = TQVaultData.TQData.ImmortalThroneSaveFolder;
			var folder = this.folderBrowserDialogMain.ShowDialog();
		}

		private void OpenFileDialogMain_FileOk(object sender, CancelEventArgs e)
		{

		}

		private void OpenFileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (this.openFileDialogMain.InitialDirectory is null) this.openFileDialogMain.InitialDirectory = TQVaultData.TQData.ImmortalThroneSaveFolder;
			DialogResult result = this.openFileDialogMain.ShowDialog();
			if (result == DialogResult.OK)
			{

			}
		}


		private void MainForm_Load(object sender, EventArgs e)
		{
#if DEBUG
			var path = @"C:\Users\Hervé\Documents\my games\Titan Quest Backup\FirstRun\SaveData\Main\_yujiro\winsys.dxb";
			//var keymap = prov.ReadKeyMap(@"C:\Users\Hervé\Documents\my games\Titan Quest Backup\FirstRun\SaveData\Main\_yujiro\Player.chr");
			//var keymap = prov.ReadKeyMap(@"C:\Users\Hervé\Documents\my games\Titan Quest Backup\FirstRun\SaveData\Main\_yujiro\winsys.dxg");

			// Init
			this.tabControlFiles.TabPages.Clear();
			AddFilePage(path);

#endif
		}

		private void AddFilePage(string path)
		{
			var page = new System.Windows.Forms.TabPage()
			{
				Location = this.tabPageTemplate.Location,
				Padding = this.tabPageTemplate.Padding,
				Size = this.tabPageTemplate.Size,
				Text = Path.GetFileName(path),
				UseVisualStyleBackColor = this.tabPageTemplate.UseVisualStyleBackColor,
			};
			var content = new TabPageFileContent()
			{
				Tag = path,
				Dock = DockStyle.Fill,
			};
			page.Controls.Add(content);
			this.tabControlFiles.Controls.Add(page);
		}
	}
}
