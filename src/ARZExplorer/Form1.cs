//-----------------------------------------------------------------------
// <copyright file="Form1.cs" company="VillageIdiot">
//     Copyright (c) Village Idiot. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace ArzExplorer
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.IO;
	using System.Reflection;
	using System.Text;
	using System.Windows.Forms;
	using TQVaultAE.DAL;

	/// <summary>
	/// Main Form for ArzExplorer
	/// </summary>
	public partial class Form1 : Form
	{
		/// <summary>
		/// The static instance of the arcFile we are working on.
		/// </summary>
		private static ArcFile arcFile;

		/// <summary>
		/// The static instance of the arzFile we are working on.
		/// </summary>
		private static ArzFile arzFile;

		/// <summary>
		/// Holds the type of file we have open.
		/// </summary>
		private static CompressedFileType fileType;

		/// <summary>
		/// Name of the source file
		/// </summary>
		private string sourceFile;

		/// <summary>
		/// Destination directory path for extracted files.
		/// </summary>
		private string destDirectory;

		/// <summary>
		/// File name for a single extracted file.
		/// </summary>
		private string destFile;

		/// <summary>
		/// Holds the title text.  Used to display the current file.
		/// </summary>
		private string titleText;

		/// <summary>
		/// Holds the current record being viewed.
		/// </summary>
		private DBRecordCollection record;

		/// <summary>
		/// Holds the initial size of the form.
		/// </summary>
		private Size initialSize;

		/// <summary>
		/// Gutter size for sizing the form.
		/// </summary>
		private int gutter;

		/// <summary>
		/// Initializes a new instance of the Form1 class.
		/// </summary>
		public Form1()
		{
			this.InitializeComponent();
			Assembly a = Assembly.GetExecutingAssembly();
			AssemblyName aname = a.GetName();
			this.titleText = aname.Name;
			this.selectedFileToolStripMenuItem.Enabled = false;
			this.allFilesToolStripMenuItem.Enabled = false;
			fileType = CompressedFileType.Unknown;
			this.initialSize = this.Size;
			this.gutter = this.initialSize.Width - this.textBox1.Width - this.treeView1.Width;
		}

		/// <summary>
		/// Gets the instance of the current arcFile
		/// </summary>
		public static ArcFile ARCFile
		{
			get
			{
				return arcFile;
			}
		}

		/// <summary>
		/// Gets the instance of the current arzFile
		/// </summary>
		public static ArzFile ARZFile
		{
			get
			{
				return arzFile;
			}
		}

		/// <summary>
		/// Gets the type of file that is open.
		/// </summary>
		public static CompressedFileType FileType
		{
			get
			{
				return fileType;
			}
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

			// Try to read the game path from the registry
			string[] path = new string[4];
			path[0] = "SOFTWARE";
			path[1] = "Iron Lore";
			path[2] = "Titan Quest";
			path[3] = "Install Location";
			string startPath = TQData.ReadRegistryKey(Microsoft.Win32.Registry.LocalMachine, path);

			// If the registry fails then default to the save folder.
			if (startPath.Length < 1)
			{
				startPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games"), "Titan Quest");
			}

			openDialog.InitialDirectory = startPath;
			DialogResult result = openDialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				this.OpenFile(openDialog.FileName);
			}
			else
			{
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
			{
				return;
			}

			this.sourceFile = filename;
			string fullSrcPath = null;

			if (string.IsNullOrEmpty(this.sourceFile))
			{
				MessageBox.Show("You must enter a valid source file path.");
				return;
			}

			// See if path exists and create it if necessary
			if (!string.IsNullOrEmpty(this.sourceFile))
			{
				fullSrcPath = Path.GetFullPath(this.sourceFile);
			}

			if (!File.Exists(fullSrcPath))
			{
				// they did not give us a file
				MessageBox.Show("You must specify a file!");
				return;
			}

			// Try to read it as an ARC file since those have a header.
			arcFile = new ArcFile(this.sourceFile);
			if (arcFile.Read())
			{
				fileType = CompressedFileType.ArcFile;
			}
			else
			{
				arcFile = null;
				fileType = CompressedFileType.Unknown;
			}

			// Try reading the file as an ARZ file.
			if (fileType == CompressedFileType.Unknown)
			{
				// Read our ARZ file into memory.
				arzFile = new ArzFile(this.sourceFile);
				if (arzFile.Read())
				{
					fileType = CompressedFileType.ArzFile;
				}
				else
				{
					arzFile = null;
					fileType = CompressedFileType.Unknown;
				}
			}

			// We failed reading the file
			// so we just clear everything out.
			if (fileType == CompressedFileType.Unknown)
			{
				this.Text = this.titleText;
				this.treeView1.Nodes.Clear();
				this.selectedFileToolStripMenuItem.Enabled = false;
				this.allFilesToolStripMenuItem.Enabled = false;
				this.textBox1.Lines = null;
				MessageBox.Show(string.Format("Error Reading {0}", this.sourceFile));
				return;
			}

			this.selectedFileToolStripMenuItem.Enabled = true;
			this.allFilesToolStripMenuItem.Enabled = true;

			this.Text = string.Format("{0} - {1}", this.titleText, this.sourceFile);

			this.textBox1.Lines = null;
			this.pictureBox1.Visible = false;

			this.BuildTreeView();
		}

		/// <summary>
		/// Builds the tree view.  Assumes the list is pre-sorted.
		/// </summary>
		private void BuildTreeView()
		{
			// Display a wait cursor while the TreeNodes are being created.
			Cursor.Current = Cursors.WaitCursor;

			this.treeView1.BeginUpdate();
			this.treeView1.Nodes.Clear();

			int maxPaths = 20; // Hopefully there are no paths more than 20 deep.
			TreeNode lastNode = null;

			// Hold the nodes from the previous record.
			// We save these so we do not need to search the treeview
			TreeNode[] lastNodes = new TreeNode[maxPaths];
			string aggSubPath;
			string[] paths = new string[maxPaths];
			string[] prevPaths = new string[maxPaths];

			string[] dataRecords;
			if (fileType == CompressedFileType.ArzFile)
			{
				dataRecords = arzFile.GetKeyTable();
			}
			else if (fileType == CompressedFileType.ArcFile)
			{
				dataRecords = arcFile.GetKeyTable();
			}
			else
			{
				return;
			}

			// We failed so return.
			if (dataRecords == null)
			{
				return;
			}

			foreach (string recordID in dataRecords)
			{
				// Holds the aggregate path
				aggSubPath = string.Empty;
				paths.Initialize();
				string[] subPaths = recordID.Split('\\');
				int count = 0;

				foreach (string subPath in subPaths)
				{
					// We only add if not the last item in the path
					// which should be the dbr.
					if (count < subPaths.Length - 1)
					{
						aggSubPath += subPath + '\\';
						paths[count] = aggSubPath;

						// See if the paths are still the same.
						if (paths[count] != prevPaths[count])
						{
							if (lastNode == null || count == 0)
							{
								// The very top of the tree.
								lastNode = this.treeView1.Nodes.Add(aggSubPath, subPath);
							}
							else
							{
								// Add a new node to the previous one in the tree.
								lastNode = lastNode.Nodes.Add(aggSubPath, subPath);
							}

							// Save this guy so we do not need to search.
							lastNodes[count] = lastNode;
						}
						else
						{
							// Use the previous TreeNode since the strings match.
							// This saves the expensive lookup.
							lastNode = lastNodes[count];
						}
					}
					else
					{
						// This is the last thing so we just add it.
						aggSubPath += subPath;

						// There might not be any sub folders so we add the file to the root
						if (lastNode == null || count == 0)
						{
							// We do not assign last node since we are still at the root.
							this.treeView1.Nodes.Add(aggSubPath, subPath);
							lastNode = null;
						}
						else
						{
							lastNode = lastNode.Nodes.Add(aggSubPath, subPath);
						}

						// Clear out and save the previous paths.
						prevPaths.Initialize();
						Array.Copy(paths, prevPaths, paths.Length);
					}

					count++;
				}
			}

			// Reset the cursor to the default for all controls.
			Cursor.Current = Cursors.Default;

			this.treeView1.EndUpdate();
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
		/// Handler for clicking extract selected file.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void SelectedFileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(this.destFile) || fileType == CompressedFileType.Unknown)
			{
				return;
			}

			string filename = null;
			SaveFileDialog saveFileDialog = new SaveFileDialog();

			saveFileDialog.Filter = "TQ files (*.txt;*.dbr;*.tex;*.msh;*.anm;*.fnt;*.qst;*.pfx;*.ssh)|*.txt;*.dbr;*.tex;*.msh;*.anm;*.fnt;*.qst;*.pfx;*.ssh|All files (*.*)|*.*";
			saveFileDialog.FilterIndex = 1;
			saveFileDialog.RestoreDirectory = true;
			saveFileDialog.Title = "Save the Titan Quest File";
			string startPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games"), "Titan Quest");
			saveFileDialog.InitialDirectory = startPath;
			saveFileDialog.FileName = Path.GetFileName(this.destFile);

			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				filename = saveFileDialog.FileName;
			}

			if (fileType == CompressedFileType.ArzFile)
			{
				this.record.Write(Path.GetDirectoryName(filename), Path.GetFileName(filename));
			}
			else if (fileType == CompressedFileType.ArcFile)
			{
				arcFile.Write(Path.GetDirectoryName(filename), this.destFile, Path.GetFileName(filename));
			}
		}

		/// <summary>
		/// Handler for clicking extract all files from the menu.
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void AllFilesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (fileType == CompressedFileType.Unknown)
			{
				return;
			}

			if ((fileType == CompressedFileType.ArzFile && arzFile == null) || (fileType == CompressedFileType.ArcFile && arcFile == null))
			{
				MessageBox.Show("Please Open a source file.");
				return;
			}

			FolderBrowserDialog browseDialog = new FolderBrowserDialog();
			browseDialog.Description = "Select the destination folder for the extracted database records";
			browseDialog.ShowNewFolderButton = true;
			string startPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games"), "Titan Quest");
			browseDialog.SelectedPath = startPath;
			DialogResult result = browseDialog.ShowDialog();

			if (result == DialogResult.OK)
			{
				this.destDirectory = browseDialog.SelectedPath;
			}
			else
			{
				return;
			}

			string fullDestPath = null;

			if (string.IsNullOrEmpty(this.destDirectory))
			{
				MessageBox.Show("You must enter a valid destination folder.");
				return;
			}

			// See if path exists and create it if necessary
			if (!string.IsNullOrEmpty(this.destDirectory))
			{
				fullDestPath = Path.GetFullPath(this.destDirectory);
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
				{
					return;
				}

				Directory.CreateDirectory(fullDestPath);
			}

			ExtractProgress extractProgress = new ExtractProgress(fullDestPath);
			extractProgress.ShowDialog();
		}

		/// <summary>
		/// Handler for clicking Help->About
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AboutBox1 d = new AboutBox1();
			d.ShowDialog();
		}

		/// <summary>
		/// Handler for clicking on a treeView item
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">TreeViewEventArgs data</param>
		private void TreeView1_AfterSelect(object sender, TreeViewEventArgs e)
		{
			// Make sure we have selected the last child
			// otherwise this will be a directory.
			if (this.treeView1.SelectedNode.GetNodeCount(false) == 0)
			{
				this.destFile = this.treeView1.SelectedNode.FullPath;
				try
				{
					List<string> recordText = new List<string>();
					if (fileType == CompressedFileType.ArzFile)
					{
						this.record = arzFile.GetRecordNotCached(this.destFile);
						foreach (Variable variable in this.record)
						{
							recordText.Add(variable.ToString());
						}
					}
					else if (fileType == CompressedFileType.ArcFile)
					{
						string extension = Path.GetExtension(this.destFile).ToUpper();
						string arcDataPath = Path.Combine(Path.GetFileNameWithoutExtension(arcFile.FileName), this.destFile);
						if (extension == ".TXT")
						{
							byte[] rawData = arcFile.GetData(arcDataPath);

							if (rawData == null)
							{
								return;
							}

							// now read it like a text file
							using (StreamReader reader = new StreamReader(new MemoryStream(rawData), Encoding.Default))
							{
								string line;
								while ((line = reader.ReadLine()) != null)
								{
									recordText.Add(line);
								}
							}
						}
						else if (extension == ".TEX")
						{
							byte[] rawData = arcFile.GetData(arcDataPath);

							if (rawData == null)
							{
								return;
							}

							Bitmap bitmap = BitmapCode.LoadFromTexMemory(rawData, 0, rawData.Length);

							if (bitmap != null)
							{
								this.pictureBox1.Visible = true;
								this.pictureBox1.Image = bitmap;
							}
						}
						else
						{
							this.pictureBox1.Visible = false;
						}
					}
					else
					{
						this.pictureBox1.Visible = false;
						this.destFile = null;
						this.textBox1.Lines = null;
						return;
					}

					// Now display our results.
					if (recordText.Count != 0)
					{
						this.pictureBox1.Visible = false;
						string[] output = new string[recordText.Count];
						recordText.CopyTo(output);
						this.textBox1.Lines = output;
					}
					else
					{
						this.textBox1.Lines = null;
					}
				}
				catch (Exception)
				{
					// Eat the exception
				}
			}
			else
			{
				this.destFile = null;
				this.textBox1.Lines = null;
			}
		}

		/// <summary>
		/// Handler for resizing the form
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void Form1_Resize(object sender, EventArgs e)
		{
			// if we get smaller then resize both panes.
			if (this.Size.Width < this.initialSize.Width)
			{
				this.treeView1.Width = (this.Size.Width - this.gutter) / 2;
				this.textBox1.Width = (this.Size.Width - this.gutter) / 2;
			}
			else
			{
				this.treeView1.Width = (this.initialSize.Width - this.gutter) / 2;
				this.textBox1.Width = this.Size.Width - this.gutter - this.treeView1.Width;
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
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				e.Effect = DragDropEffects.Copy;
			}
			else
			{
				e.Effect = DragDropEffects.None;
			}
		}
	}
}