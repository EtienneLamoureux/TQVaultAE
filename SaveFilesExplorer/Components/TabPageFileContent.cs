using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SaveFilesExplorer.Services;
using SaveFilesExplorer.Entities;

namespace SaveFilesExplorer.Components
{
	public partial class TabPageFileContent : UserControl
	{
		public TabPageFileContent()
		{
			InitializeComponent();
		}

		private void TabPageContent_Load(object sender, EventArgs e)
		{
			var prov = new TQFileService();
			var keymap = prov.ReadKeyMap(this.Tag as string);
			var tree = MakeTreeNode(keymap.ToList()).nodes.ToArray();
			this.treeViewKeys.Nodes.AddRange(tree);
			RecurseColor(tree);
		}


		private (List<TreeNode> nodes, int newidx) MakeTreeNode(List<TQFileRecord> keymap, int idx = 0)
		{
			List<TreeNode> currentLvl = new List<TreeNode>();
			for (; idx < keymap.Count(); idx++)
			{
				var k = keymap[idx];
				var tn = new TreeNode(k.Key);
				tn.Tag = k;
				if (k.IsSubStructureOpening)
				{
					var nextLvl = MakeTreeNode(keymap, idx + 1);
					idx = nextLvl.newidx;
					tn.Nodes.AddRange(nextLvl.nodes.ToArray());
				}

				currentLvl.Add(tn);
				if (k.IsStructureClosing) break;
			}
			return (currentLvl, idx);
		}

		private Color? RecurseColor(TreeNode[] tree)
		{
			Color? errlvl = null;
			foreach (var node in tree)
			{
				var k = node.Tag as TQFileRecord;
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
				if (nestedErrlvl != null)
				{
					if (errlvl != Color.Red) errlvl = nestedErrlvl;
					node.ForeColor = nestedErrlvl.Value;
					node.NodeFont = f;
				}
			}
			return errlvl;
		}
	}
}
